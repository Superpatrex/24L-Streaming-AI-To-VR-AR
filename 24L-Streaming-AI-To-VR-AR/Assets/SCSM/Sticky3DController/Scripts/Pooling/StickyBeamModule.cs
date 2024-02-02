using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A poolable beam that can be fired from a StickyWeapon
    /// </summary>
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyBeamModule : StickyGenericModule
    {
        #region Public Variables

        /// <summary>
        /// The speed the beam travels in metres per second
        /// </summary>
        public float speed = 400f;

        /// <summary>
        /// The type of damage the beam does. The amount of damage dealt to a character upon collision is dependent
        /// on the character's multiplier for this damage type (i.e. if a Type A beam with a damage amount of 10 hits a character
        /// with a Type A damage multiplier of 2, a total damage of 20 will be done to the character). If the damage type is set to Default,
        /// the damage multipliers are ignored i.e. the damage amount is unchanged.
        /// </summary>
        public S3DDamageRegion.DamageType damageType = S3DDamageRegion.DamageType.Default;

        /// <summary>
        /// The amount of damage this beam does, per second, to the character or object it hits.
        /// NOTE: Non-S3D characters objects need a StickyDamageReceiver component. 
        /// </summary>
        public float damageRate = 10f;

        /// <summary>
        /// The start width (in metres) of the beam on the local x-axis
        /// In this version the width will be the same for the entire length of the beam.
        /// </summary>
        [Range(0.001f, 5f)] public float beamStartWidth = 0.1f;

        /// <summary>
        /// The minimum amount of time, in seconds, the beam must be active
        /// </summary>
        [Range(0.1f, 5f)] public float minBurstDuration = 0.5f;

        /// <summary>
        /// The maximum amount of time, in seconds, the beam can be active in a single burst
        /// </summary>
        [Range(0.1f, 30f)] public float maxBurstDuration = 5f;

        /// <summary>
        /// The time (in seconds) it takes a single beam to discharge the beam weapon from full charge
        /// </summary>
        [Range(0.1f, 60f)] public float dischargeDuration = 10f;

        /// <summary>
        /// The ID number for this beam prefab (as assigned by the Sticky Manager in the scene).
        /// This is the index in the StickyManager beamTemplatesList.
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] public int beamPrefabID = -1;

        /// <summary>
        /// The default sound or particle FX used when a collision occurs. 
        /// If you modify this, call stickyWeapon.ReInitialiseWeapon() for each weapon
        /// this beam is used on.
        /// </summary>
        public StickyEffectsModule effectsObject = null;

        /// <summary>
        /// The ID number for this beam's default effects object prefab (as assigned by the Sticky Manager in the scene).
        /// This is the index in the StickyManager effectsObjectTemplatesList. Not defined = -1.
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] public int effectsObjectPrefabID = -1;

        /// <summary>
        /// The faction or alliance of the character that fired the beam belongs to.
        /// </summary>
        [System.NonSerialized] public int sourceFactionId = 0;
        /// <summary>
        /// The type, category, or model of the character that fired the beam.
        /// </summary>
        [System.NonSerialized] public int sourceModelId = 0;
        /// <summary>
        /// The Id of the S3D character that fired the beam
        /// </summary>
        [System.NonSerialized] public int sourceStickyId = 0;

        #endregion

        #region Public Properties

        /// <summary>
        /// [READONLY] Has the beam been initialised, and is the (generic) module enabled?
        /// </summary>
        public bool IsBeamEnabled { get { return isInitialised && isModuleEnabled; } }

        #endregion

        #region Public Static Variables

        #endregion

        #region Private Variables - General
        private S3DInstantiateGenericObjectParameters igParms;

        [System.NonSerialized] internal float burstDuration = 0f;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Reference to the LineRenderer component which should be on a child
        /// gameobject of this module.
        /// </summary>
        [System.NonSerialized] internal LineRenderer lineRenderer = null;

        /// <summary>
        /// The zero-based index of the fire position on the weapon that fired the beam
        /// </summary>
        [System.NonSerialized] internal int firePositionOffsetIndex;

        /// <summary>
        /// The item key of the effects object that is spawned when the beam hits something
        /// </summary>
        [System.NonSerialized] internal S3DEffectItemKey effectsItemKey;


        [System.NonSerialized] internal StickyManager stickyManager;

        #endregion

        #region Public Delegates

        #endregion

        #region Internal Delegates
        internal delegate void CallbackOnMove(StickyBeamModule beamModule);

        [System.NonSerialized] internal CallbackOnMove callbackOnMove = null;

        #endregion

        #region Update Methods

        // FixedUpdate is called once per physics update (typically about 50 times per second)
        private void FixedUpdate()
        {
            if (isInitialised && isModuleEnabled)
            {
                burstDuration += Time.deltaTime;

                if (callbackOnMove != null) { callbackOnMove(this); }
            }
        }

        #endregion

        #region Private and Internal Methods - General

        #endregion

        #region Events

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Destroy the beam by cleaning up and returning it to the pool
        /// </summary>
        public virtual void DestroyBeam()
        {
            // Reset
            firePositionOffsetIndex = -1;

            // Remove the reference
            callbackOnMove = null;

            // Stop hit fx
            if (effectsItemKey.effectsObjectSequenceNumber > 0)
            {
                stickyManager.DestroyEffectsObject(effectsItemKey);
            }

            DestroyGenericObject();
        }

        /// <summary>
        /// Initialise the beam module.
        /// This is automatically called from the StickyManager pooling system whenever it is spawned (instantiated) in the scene.
        /// </summary>
        /// <param name="ibParms"></param>
        /// <returns></returns>
        public virtual uint Initialise (S3DInstantiateBeamParameters ibParms)
        {
            // Store the index to the BeamTemplate from the StickyManager beamTemplatesList
            // This is used with Beam FX when we know the BeamModule but not the parent BeamTemplate.
            beamPrefabID = ibParms.beamPrefabID;

            // Max Burst essentially overrides despawnTime.
            despawnTime = -1;

            // Initialise generic object module
            // Currently igParms are required to be set for generic object Initialise(..).
            igParms.genericObjectPoolListIndex = -1;
            Initialise(igParms);

            // Store the index to the S3DEffectsObjectTemplate in stickyManager.effectsObjectTemplatesList
            effectsObjectPrefabID = ibParms.effectsObjectPrefabID;

            // Reset the (hit) effects item key. This is updated by the caller
            effectsItemKey = new S3DEffectItemKey(-1, -1, 0);

            // Store the details on what fired the beam
            sourceStickyId = ibParms.stickyId;
            sourceFactionId = ibParms.factionId;
            sourceModelId = ibParms.modelId;
            //this.weaponIndex = ibParms.weaponIndex;
            this.firePositionOffsetIndex = ibParms.firePositionOffsetIndex;

            if (lineRenderer == null) { lineRenderer = GetComponentInChildren<LineRenderer>(); }

            if (lineRenderer != null)
            {
                lineRenderer.startWidth = beamStartWidth;
                lineRenderer.endWidth = beamStartWidth;
                lineRenderer.alignment = LineAlignment.View;
                lineRenderer.startColor = Color.white;
                lineRenderer.endColor = Color.white;

                // Our calculations in weapon.MoveBeam(..) assume local-space positions
                lineRenderer.useWorldSpace = false;
                lineRenderer.SetPosition(0, Vector3.zero);

                // After a given amount of time, automatically destroy this beam
                burstDuration = 0f;

                //isBeamEnabled = gameObject.activeSelf;
            }

            return itemSequenceNumber;
        }

        #endregion
    }
}