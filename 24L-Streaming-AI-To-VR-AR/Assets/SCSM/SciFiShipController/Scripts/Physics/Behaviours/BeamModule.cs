using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [AddComponentMenu("Sci-Fi Ship Controller/Weapon Components/Beam Module")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class BeamModule : MonoBehaviour
    {
        #region Public Enumerations

        #endregion

        #region Public Variables

        /// <summary>
        /// The speed the beam travels in metres per second
        /// </summary>
        public float speed = 200f;

        /// <summary>
        /// The type of damage the beam does. The amount of damage dealt to a ship upon collision is dependent
        /// on the ship's multiplier for this damage type (i.e. if a Type A beam with a damage amount of 10 hits a ship
        /// with a Type A damage multiplier of 2, a total damage of 20 will be done to the ship). If the damage type is set to Default,
        /// the damage multipliers are ignored i.e. the damage amount is unchanged.
        /// </summary>
        public ProjectileModule.DamageType damageType = ProjectileModule.DamageType.Default;
        /// <summary>
        /// The amount of damage this beam does, per second, to the ship or object it hits. NOTE: Non-ship objects need a DamageReceiver component. 
        /// </summary>
        public float damageRate = 10f;

        /// <summary>
        /// Whether pooling is used when spawning beams of this type.
        /// Currently we don't support changing this at runtime.
        /// </summary>
        public bool usePooling = true;
        /// <summary>
        /// The starting size of the pool. Only relevant when usePooling is enabled.
        /// </summary>
        public int minPoolSize = 10;
        /// <summary>
        /// The maximum allowed size of the pool. Only relevant when usePooling is enabled.
        /// </summary>
        public int maxPoolSize = 100;
        
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
        /// The ID number for this beam prefab (as assigned by the Ship Controller Manager in the scene).
        /// This is the index in the SSCManager beamTemplatesList.
        /// [INTERNAL USE ONLY]
        /// </summary>
        public int beamPrefabID;

        /// <summary>
        /// The sound or particle FX used when a collision occurs. 
        /// If you modify this, call shipControlModule.ReinitialiseShipBeams() for each ship
        /// this beam is used on.
        /// </summary>
        public EffectsModule effectsObject = null;

        /// <summary>
        /// The ID number for this beam's effects object prefab (as assigned by the Ship Controller Manager in the scene).
        /// This is the index in the SSCManager effectsObjectTemplatesList. Not defined = -1.
        /// [INTERNAL USE ONLY]
        /// </summary>
        public int effectsObjectPrefabID = -1;

        /// <summary>
        /// The sound and/or particle FX used when a beam is fired from a weapon.
        /// If you modify this, call shipControlModule.ReinitialiseShipBeams() for each ship
        /// this beam is used on.
        /// </summary>
        public EffectsModule muzzleEffectsObject = null;

        /// <summary>
        /// The distance in local space that the muzzle Effects Object should be instantiated
        /// from the weapon firing point. Typically only the z-axis would be used when the beam
        /// is instantiated in front or forwards from the actual weapon.
        /// </summary>
        public Vector3 muzzleEffectsOffset = Vector3.zero;

        /// <summary>
        /// The ID number for this beam's muzzle object prefab (as assigned by the Ship Controller Manager in the scene).
        /// This is the index in the SSCManager effectsObjectTemplatesList. Not defined = -1.
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] public int muzzleEffectsObjectPrefabID = -1;

        /// <summary>
        /// The Id of the ship that fired the beam
        /// </summary>
        [System.NonSerialized] public int sourceShipId = -1;

        /// <summary>
        /// The Squadron which the ship belonged to when it fired the beam
        /// </summary>
        [System.NonSerialized] public int sourceSquadronId = -1;

        #endregion

        #region Private and Internal Variables
        [System.NonSerialized] internal bool isInitialised = false;

        [System.NonSerialized] internal bool isBeamEnabled = false;

        [System.NonSerialized] internal float burstDuration = 0f;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Reference to the LineRenderer component which should be on a child
        /// gameobject of this module.
        /// </summary>
        [System.NonSerialized] internal LineRenderer lineRenderer = null;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used to determine uniqueness
        /// </summary>
        [System.NonSerialized] internal uint itemSequenceNumber;

        /// <summary>
        /// The zero-based index of the weapon on the ship that fired the beam
        /// </summary>
        [System.NonSerialized] internal int weaponIndex;

        /// <summary>
        /// The zero-based index of the fire position on the weapon that fired the beam
        /// </summary>
        [System.NonSerialized] internal int firePositionIndex;

        /// <summary>
        /// The item key of the muzzle effects object (if any) firing from this beam
        /// </summary>
        [System.NonSerialized] internal SSCEffectItemKey muzzleEffectsItemKey;

        /// <summary>
        /// The item key of the effects object that is spawned when the beam hits something
        /// </summary>
        [System.NonSerialized] internal SSCEffectItemKey effectsItemKey;

        /// <summary>
        /// The handle to the scene this beam is created in
        /// </summary>
        [System.NonSerialized] internal int sceneHandle;

        #endregion

        #region Internal Delegates
        internal delegate void CallbackOnMove(BeamModule beamModule);

        [System.NonSerialized] internal CallbackOnMove callbackOnMove = null;

        #endregion

        #region Internal Static Variables
        internal static uint nextSequenceNumber = 1;
        #endregion

        #region Enable/Disable Event Methods

        void OnDisable()
        {
            isInitialised = false;
        }

        #endregion

        #region Update Methods

        // FixedUpdate is called once per physics update (typically about 50 times per second)
        private void FixedUpdate()
        {
            if (isInitialised && isBeamEnabled)
            {
                burstDuration += Time.deltaTime;

                if (callbackOnMove != null) { callbackOnMove(this); }
            }
        }

        #endregion

        #region Private and Internal Methods

        /// <summary>
        /// Intialise the beam and return it's unique sequence number
        /// </summary>
        /// <param name="ibParms"></param>
        internal uint InitialiseBeam(InstantiateBeamParameters ibParms)
        {
            // Store the index to the BeamTemplate in the SSCManager beamTemplatesList
            // This is used with Beam FX when we know the BeamModule but not the parent BeamTemplate.
            this.beamPrefabID = ibParms.beamPrefabID;

            // Store the index to the EffectsObjectTemplate in sscManager.effectsObjectTemplatesList
            this.effectsObjectPrefabID = ibParms.effectsObjectPrefabID;

            // Store the details on what fired the beam
            this.sourceShipId = ibParms.shipId;
            this.sourceSquadronId = ibParms.squadronId;
            this.weaponIndex = ibParms.weaponIndex;
            this.firePositionIndex = ibParms.firePositionIndex;

            IncrementSequenceNumber();

            muzzleEffectsItemKey = new SSCEffectItemKey(-1, -1, 0);
            effectsItemKey = new SSCEffectItemKey(-1, -1, 0);

            if (lineRenderer == null) { lineRenderer = GetComponentInChildren<LineRenderer>(); }

            if (lineRenderer != null)
            {
                lineRenderer.startWidth = beamStartWidth;
                lineRenderer.endWidth = beamStartWidth;
                lineRenderer.alignment = LineAlignment.View;
                lineRenderer.startColor = Color.white;
                lineRenderer.endColor = Color.white;

                // Our calculations in ship.MoveBeam(..) assume world-space positions
                lineRenderer.useWorldSpace = true;

                // After a given amount of time, automatically destroy this beam
                burstDuration = 0f;

                isBeamEnabled = gameObject.activeSelf;

                isInitialised = true;
            }

            return itemSequenceNumber;
        }

        /// <summary>
        /// Makes this BeamModule unique from all others that have gone before them.
        /// This is called every time beam is Initialised. 
        /// </summary>
        internal void IncrementSequenceNumber()
        {
            itemSequenceNumber = nextSequenceNumber++;
            // if sequence number needs to be wrapped, do so to a high-ish number that is unlikely to be in use 
            if (nextSequenceNumber > uint.MaxValue - 100) { nextSequenceNumber = 100000; }
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Removes the beam. How this is done depends on what system is being used (i.e. pooling etc.).
        /// </summary>
        internal void DestroyBeam()
        {
            // Reset
            weaponIndex = -1;
            firePositionIndex = -1;
            isBeamEnabled = false;
            // Remove the reference
            callbackOnMove = null;

            SSCManager sscManager = null;

            // Stop muzzle fx
            if (muzzleEffectsItemKey.effectsObjectSequenceNumber > 0)
            {
                sscManager = SSCManager.GetOrCreateManager(sceneHandle);

                if (sscManager != null)
                {
                    sscManager.DestroyEffectsObject(muzzleEffectsItemKey);
                }
            }

            // Stop hit fx
            if (effectsItemKey.effectsObjectSequenceNumber > 0)
            {
                if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(sceneHandle); }

                if (sscManager != null)
                {
                    sscManager.DestroyEffectsObject(effectsItemKey);
                }
            }

            if (usePooling)
            {             
                // Deactivate the beam
                gameObject.SetActive(false);
            }
            else
            {
                // Destroy the beam
                Destroy(gameObject);
            }
        }

        #endregion

        #region Public API Static Methods

        /// <summary>
        /// Determine if a ship has been hit. Apply damage as required based on the damage rate, damage type
        /// and hit duration.
        /// Hit duration is the time in seconds that the beam has been in contact with the target object since
        /// it was last checked (typically the duration of the last frame or time step).
        /// </summary>
        /// <param name="raycastHitInfo"></param>
        /// <param name="beamModule">Must not be null</param>
        /// <param name="hitDuration"></param>
        /// <returns></returns>
        public static bool CheckShipHit (RaycastHit raycastHitInfo, BeamModule beamModule, float hitDuration)
        {
            bool isHit = false;
            Rigidbody hitRigidbody = raycastHitInfo.rigidbody;

            if (hitRigidbody != null && beamModule != null)
            {
                // Check if there is a ship control module attached to the hit object
                ShipControlModule shipControlModule = hitRigidbody.GetComponent<ShipControlModule>();

                if (shipControlModule != null)
                {
                    float damageAmount = beamModule.damageRate * hitDuration;
                    
                    // Apply damage to the ship
                    shipControlModule.shipInstance.ApplyNormalDamage(damageAmount, beamModule.damageType, raycastHitInfo.point);

                    // If required, call the custom method
                    if (shipControlModule.callbackOnHit != null)
                    {
                        // Get a reference to the SSCManager
                        SSCManager sscManager = SSCManager.GetOrCreateManager(beamModule.sceneHandle);

                        // Create a struct with the necessary parameters
                        CallbackOnShipHitParameters callbackOnShipHitParameters = new CallbackOnShipHitParameters
                        {
                            hitInfo = raycastHitInfo,
                            projectilePrefab = null,
                            beamPrefab = sscManager.GetBeamPrefab(beamModule.beamPrefabID),
                            damageAmount = damageAmount,
                            sourceSquadronId = beamModule.sourceSquadronId,
                            sourceShipId = beamModule.sourceShipId
                        };
                        // Call the custom callback
                        shipControlModule.callbackOnHit(callbackOnShipHitParameters);
                    }

                    isHit = true;
                }
            }

            return isHit;
        }

        /// <summary>
        /// Determine if an object with a DamageReceiver component attached has been hit by a Beam.
        /// </summary>
        /// <param name="raycastHitInfo"></param>
        /// <param name="beamModule">Must not be null</param>
        /// <returns></returns>
        public static bool CheckObjectHit (RaycastHit raycastHitInfo, BeamModule beamModule, float hitDuration)
        {
            bool isHit = false;
            Rigidbody hitRigidbody = raycastHitInfo.rigidbody;

            if (raycastHitInfo.collider != null)
            {
                DamageReceiver damageReceiver = raycastHitInfo.collider.GetComponent<DamageReceiver>();
                if (damageReceiver != null && damageReceiver.callbackOnHit != null)
                {
                    // Get a reference to the SSCManager
                    SSCManager sscManager = SSCManager.GetOrCreateManager(beamModule.sceneHandle);

                    // Create a struct with the necessary parameters
                    CallbackOnObjectHitParameters callbackOnObjectHitParameters = new CallbackOnObjectHitParameters
                    {
                        hitInfo = raycastHitInfo,
                        projectilePrefab = null,
                        beamPrefab = sscManager.GetBeamPrefab(beamModule.beamPrefabID),
                        damageAmount = beamModule.damageRate * hitDuration,
                        sourceSquadronId = beamModule.sourceSquadronId
                    };
                    // Call the custom callback
                    damageReceiver.callbackOnHit(callbackOnObjectHitParameters);

                    isHit = true;
                }
            }

            return isHit;
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Re-enable a beam after it has been disabled with
        /// DisableBeam(). See also SSCManager.ResumeBeams()
        /// </summary>
        public void EnableBeam()
        {
            isBeamEnabled = true;
        }

        /// <summary>
        /// Useful when you want to pause the action in a game.
        /// Should always be called BEFORE setting Time.timeScale to 0.
        /// See also SSCManager.PauseBeams().
        /// </summary>
        public void DisableBeam()
        {
            isBeamEnabled = false;
        }

        #endregion
    }
}
