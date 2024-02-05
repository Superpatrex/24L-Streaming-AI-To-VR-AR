using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// [INCOMPLTE] - Doesn't support ref. frame gravity yet...
    /// A poolable projectile that can be fired from a StickyWeapon
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyProjectileModule : StickyGenericModule
    {
        #region Public Variables

        /// <summary>
        /// The type of damage the projectile does. The amount of damage dealt to a character upon collision is dependent
        /// on the character's multiplier for this damage type (i.e. if a Type A projectile with a damage amount of 10 hits a character
        /// with a Type A damage multiplier of 2, a total damage of 20 will be done to the character). If the damage type is set to Default,
        /// the damage multipliers are ignored i.e. the damage amount is unchanged.
        /// </summary>
        public S3DDamageRegion.DamageType damageType = S3DDamageRegion.DamageType.Default;

        /// <summary>
        /// The amount of damage this projectile does to the character or object it hits.
        /// NOTE: Non-S3D characters objects need a StickyDamageReceiver component. 
        /// </summary>
        public float damageAmount = 10f;

        /// <summary>
        /// The speed the projectile starts traveling in metres per second. Is Ignored for Raycast weapons.
        /// </summary>
        public float startSpeed = 100f;

        /// <summary>
        /// The ID number for this projectile prefab (as assigned by the Sticky Manager in the scene).
        /// This is the index in the StickyManager projectileTemplatesList.
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] public int projectilePrefabID = -1;

        /// <summary>
        /// The default set of StickyDecalModule prefabs used to select a random decal when the projectile hits an object.
        /// </summary>
        public S3DDecals s3dDecals = null;

        /// <summary>
        /// The default particle and/or sound effect prefab that will be instantiated when the projectile hits something.
        /// This would typically be a non-looping effect.
        /// If you modify this, call stickyWeapon.ReInitialiseWeapon() for each weapon
        /// this beam is used on.
        /// </summary>
        public StickyEffectsModule effectsObject = null;

        /// <summary>
        /// The layer mask used for collision testing for this projectile.
        /// Default is everything. This comes from the weapon.
        /// </summary>
        public LayerMask collisionLayerMask = ~0;

        /// <summary>
        /// The amount of force that is applied when hitting a rigidbody at the point of impact.
        /// NOTE: Non-S3D characters objects need a StickyDamageReceiver component. 
        /// </summary>
        public float impactForce = 2000f;

        /// <summary>
        /// The projectile should point forward on the z-axis. If it say points upward (y-axis), it
        /// needs to be rotated here around the x-axis.
        /// </summary>
        public Vector3 modelRotationOffset = new Vector3(0f, 0f, 0f);

        /// <summary>
        /// The ID number for this projectile's default effects object prefab (as assigned by the Sticky Manager in the scene).
        /// This is the index in the StickyManager effectsObjectTemplatesList. Not defined = -1.
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] public int effectsObjectPrefabID = -1;

        /// <summary>
        /// The ammo type (0-25, A-Z) that was fired from a weapon. A different ammo type can be
        /// set each time the projectile is enabled in the pool. See Initialise()
        /// </summary>
        [System.NonSerialized] public int ammoTypeInt = -1;

        /// <summary>
        /// The faction or alliance of the character that fired the projectile belongs to.
        /// </summary>
        [System.NonSerialized] public int sourceFactionId = 0;
        /// <summary>
        /// The type, category, or model of the character that fired the projectile.
        /// </summary>
        [System.NonSerialized] public int sourceModelId = 0;
        /// <summary>
        /// The Id of the S3D character that fired the projectile
        /// </summary>
        [System.NonSerialized] public int sourceStickyId = 0;

        #endregion

        #region Public Properties

        /// <summary>
        /// [READONLY] Has the projectile been initialised, and is the (generic) module enabled?
        /// </summary>
        public bool IsProjectileEnabled { get { return isInitialised && isModuleEnabled; } }

        #endregion

        #region Public Static Variables

        #endregion

        #region Protected variables
        // These variables can be modified by classes that inherit from ProjectileModule

        protected Vector3 velocity = Vector3.zero;
        protected float speed = 0f;
        protected Vector3 lastFramePosition = Vector3.zero;
        protected Vector3 thisFramePosition = Vector3.zero;
        protected Vector3 currentDirection;
        protected float distanceSinceLastFrame = 0f;
        protected RaycastHit hitInfo;
        protected float projectileDespawnTimer = 0f;
        #endregion

        #region Private and Internal Variables

        private S3DInstantiateGenericObjectParameters igParms;
        [System.NonSerialized] internal StickyManager stickyManager = null;
        [System.NonSerialized] internal int numberOfDecals = 0;
        [System.NonSerialized] private StickySurface stickySurface = null;
        [System.NonSerialized] private StickyDecalModule stickyDecalModuleDamagePrefab;

        #endregion

        #region Public Delegates

        #endregion

        #region Private Initialise Methods

        #endregion

        #region Update Methods

        // FixedUpdate is called once per physics update (typically about 50 times per second)
        private void FixedUpdate()
        {
            if (isInitialised && isModuleEnabled)
            {
                // Remember last frame position as last frame position - we are about to update this frame position
                lastFramePosition = thisFramePosition;

                CalcPositionAndVelocity();

                // Check to see if the projectile has collided with anything during this frame
                // If nothing hit, decrement the timer.
                if (CheckCollision())
                {
                    DestroyProjectile();
                }
                else
                {
                    // To avoid the projectile being despawned at the incorrect time,
                    // do it here rather than calling Invoke(destroyMethodName, despawnTime)
                    // when the base GenericModule is initialised.
                    projectileDespawnTimer += Time.deltaTime;
                    if (projectileDespawnTimer >= despawnTime)
                    {
                        DestroyProjectile();
                    }
                    else
                    {
                        // Update the position of the object
                        transform.position = thisFramePosition;
                    }
                }
            }
        }

        #endregion

        #region Protected Methods - General

        /// <summary>
        /// Calculate new velocity and thisFramePosition
        /// </summary>
        protected virtual void CalcPositionAndVelocity()
        {
            /// TODO Calc gravity - need to implement IStickyGravity
            //if (useGravity)
            //{
            //    // F = ma
            //    // a = dv/dt
            //    // F = mdv/dt
            //    // dv = Fdt/m

            //    velocity += (gravitationalAcceleration * Time.deltaTime * gravityDirection);
            //}

            /// TODO - Guided Projectile method and potential callback here

            // Move the projectile according to its current velocity
            thisFramePosition = lastFramePosition + (velocity * Time.deltaTime);

            // Calc direction and distance moved in the last frame
            currentDirection = thisFramePosition - lastFramePosition;
            distanceSinceLastFrame = currentDirection.magnitude;
            currentDirection = distanceSinceLastFrame < Mathf.Epsilon ? Vector3.forward : currentDirection.normalized;
        }

        /// <summary>
        /// Check to see if the projectile has collided with anything during this frame
        /// </summary>
        /// <returns>True if a collision occurred</returns>
        protected virtual bool CheckCollision()
        {
            bool isHit = false;

            // LayerMask is set from the weapon during Initialise().

            //float damageMultiplier = stickyManager.GetAmmoDamageMultiplier(ammoTypeInt);

            // Did the projectile hit a character, another object with a StickyDamageReciever, or a non-trigger collider?
            if (stickyManager.CheckObjectHitByProjectile
            (
                lastFramePosition, currentDirection, distanceSinceLastFrame, collisionLayerMask, out hitInfo, 0, this, false
            ))
            {
                isHit = true;
                int decalPrefabID = -1;

                //Debug.Log("[DEBUG] Projectile.CheckCollision ammoTypeInt: " + ammoTypeInt + " projectile: " + name + " Hit FX ID: " + effectsObjectPrefabID + " T:" + Time.time);

                #region Effects
                if (effectsObjectPrefabID >= 0)
                {
                    S3DInstantiateEffectsObjectParameters ieParms = new S3DInstantiateEffectsObjectParameters
                    {
                        effectsObjectPrefabID = effectsObjectPrefabID,
                        position = hitInfo.point,
                        rotation = transform.rotation
                    };

                    // For projectiles we don't need to get the effectsObject key from ieParms.
                    stickyManager.InstantiateEffectsObject(ref ieParms);
                }
                #endregion

                // Check if projectile hit an object with a matching surface decal to override the default decal set.
                #region StickySurface decals

                if (hitInfo.transform.TryGetComponent(out stickySurface) && stickySurface.HasDamageDecals)
                {
                    // Get the prefab for the decal.
                    if (stickySurface.GetDamageDecalPrefab(out stickyDecalModuleDamagePrefab))
                    {
                        // Find (or create) the pool of decals from which we will use to place a decal
                        // on the surface of the object. To avoid having to create a new pool the first
                        // time this decal is used, the Damage Decals from the Surface Types scriptable object
                        // should be be added to the Startup Decal Sets in the StickyManager editor.
                        decalPrefabID = stickyManager.GetOrCreateDecalPool(stickyDecalModuleDamagePrefab);
                    }
                }

                #endregion

                #region Decals
                
                if (decalPrefabID >= 0 || numberOfDecals > 0)
                {
                    // If there wasn't a stickySurface decal, choose a default decal from a list randomly
                    if (decalPrefabID < 0)
                    {
                        decalPrefabID = stickyManager.GetProjectileDecalPrefabID(projectilePrefabID);
                    }

                    if (decalPrefabID >= 0)
                    {
                        // Get the decal from the pool and place it slightly in front of the object
                        // to avoid z-fighting with the object renderer the projectile hit.
                        S3DInstantiateDecalParameters idParms = new S3DInstantiateDecalParameters
                        {
                            decalPrefabID = decalPrefabID,
                            position = hitInfo.point + (hitInfo.normal * 0.0005f),
                            rotation = Quaternion.LookRotation(-hitInfo.normal),
                            fwdDirection = -hitInfo.normal,
                            collisionMaskLayerInt = (int)collisionLayerMask,
                            decalPoolListIndex = -1,
                            decalSequenceNumber = 0
                        };

                        StickyDecalModule decalModule = stickyManager.InstantiateDecal(ref idParms);

                        // Should we parent it to the object that was hit??
                        if (idParms.decalSequenceNumber > 0 && decalModule != null && decalModule.isReparented)
                        {
                            decalModule.transform.SetParent(hitInfo.transform);
                        }
                    }
                }
                #endregion
            }

            return isHit;
        }

        /// <summary>
        /// Despawns the projectile.
        /// When overriding, write your own logic then call base.DestroyProjectile().
        /// </summary>
        protected virtual void DestroyProjectile()
        {
            // If guided projectile clear target here    


            base.DestroyGenericObject();
        }

        #endregion

        #region Events

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Initialise the projectile module.
        /// This is automatically called from the StickyManager pooling system whenever it is spawned (instantiated) in the scene.
        /// </summary>
        /// <param name="ipParms"></param>
        /// <returns></returns>
        public virtual uint Initialise (S3DInstantiateProjectileParameters ipParms)
        {
            // Store the index to the ProjectileTemplate from the StickyManager projectileTemplatesList
            // This is used when we know the ProjectileModule but not the parent ProjectileTemplate.
            projectilePrefabID = ipParms.projectilePrefabID;

            projectileDespawnTimer = 0f;

            // Initialise generic object module
            // Currently igParms are required to be set for generic object Initialise(..).
            igParms.genericObjectPoolListIndex = -1;
            // We want to control despawn time ourselves rather than via the StickyGenericModule
            igParms.overrideAutoDestroy = true;
            Initialise(igParms);

            // Store the index to the EffectsObjectTemplate from the stickyManager.effectsObjectTemplatesList
            effectsObjectPrefabID = ipParms.effectsObjectPrefabID;

            // Initialise the velocity based on the forwards direction
            speed = startSpeed;
            // Use the weapon fire direction rather than the projectile tranform.forward as the projeciles
            // forward direction might have been adjusted for an incorrect model orientation with modelRotationOffset.
            currentDirection = ipParms.fwdDirection;
            velocity = ipParms.fwdDirection * speed;

            // Initialise last/this frame positions
            // Shift the position forward by the weapon velocity, so that projectiles don't end up behind the weapon
            lastFramePosition = transform.position + (ipParms.weaponVelocity * Time.fixedDeltaTime);
            thisFramePosition = lastFramePosition;

            transform.position = thisFramePosition;

            // Store the details on what fired the projectile
            sourceStickyId = ipParms.stickyId;
            sourceFactionId = ipParms.factionId;
            sourceModelId = ipParms.modelId;

            collisionLayerMask = ipParms.collisionMaskLayerInt;
            ammoTypeInt = ipParms.ammoTypeInt;

            // GUIDED PROJECTILE INIT GOES HERE


            return itemSequenceNumber;
        }

        #endregion

        #region Public Static API Methods
 
        #endregion
    }
}