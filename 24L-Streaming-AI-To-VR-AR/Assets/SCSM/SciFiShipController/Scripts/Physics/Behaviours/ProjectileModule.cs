using UnityEngine;
using System.Collections.Generic;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    [AddComponentMenu("Sci-Fi Ship Controller/Weapon Components/Projectile Module")]
    public class ProjectileModule : MonoBehaviour
    {
        #region Public Enumerations

        public enum DamageType
        {
            /// <summary>
            /// Damage from projectiles of this type will be unaffected by ship damage multipliers
            /// i.e. the amount of damage done to the ship will be identical to damageAmount.
            /// </summary>
            Default = 0,
            TypeA = 100,
            TypeB = 105,
            TypeC = 110,
            TypeD = 115,
            TypeE = 120,
            TypeF = 125,
        }

        #endregion

        #region Public Variables

        /// <summary>
        /// The starting speed of the projectile in metres per second.
        /// </summary>
        public float startSpeed = 100f;
        /// <summary>
        /// Whether the projectile is affected by gravity.
        /// </summary>
        public bool useGravity = false;
        /// <summary>
        /// The type of damage the projectile does. The amount of damage dealt to a ship upon collision is dependent
        /// on the ship's multiplier for this damage type (i.e. if a Type A projectile with a damage amount of 10 hits a ship
        /// with a Type A damage multiplier of 2, a total damage of 20 will be done to the ship). If the damage type is set to Default,
        /// the damage multipliers are ignored i.e. the damage amount is unchanged.
        /// </summary>
        public DamageType damageType = DamageType.Default;
        /// <summary>
        /// The amount of damage the projectile does on collision with a ship or object. NOTE: Non-ship objects need
        /// a DamageReceiver component. 
        /// </summary>
        public float damageAmount = 10f;

        /// <summary>
        /// Whether Entity Component System and Job System is used when spawning projectiles of this type
        /// </summary>
        public bool useECS = false;

        /// <summary>
        /// Whether pooling is used when spawning projectiles of this type.
        /// Currently we don't support changing this at runtime.
        /// </summary>
        public bool usePooling = true;
        /// <summary>
        /// The starting size of the pool. Only relevant when usePooling is enabled.
        /// </summary>
        public int minPoolSize = 100;
        /// <summary>
        /// The maximum allowed size of the pool. Only relevant when usePooling is enabled.
        /// </summary>
        public int maxPoolSize = 1000;

        /// <summary>
        /// The projectile will be automatically despawned after this amount of time (in seconds) has elapsed.
        /// </summary>
        public float despawnTime = 1f;

        /// <summary>
        /// The ID number for this projectile prefab (as assigned by the Ship Controller Manager in the scene).
        /// This is the index in the SSCManager projectileTemplatesList.
        /// [INTERNAL USE ONLY]
        /// </summary>
        public int projectilePrefabID;

        /// <summary>
        /// The sound or particle FX used when a collision occurs. 
        /// If you modify this, call shipControlModule.ReinitialiseShipProjectilesAndEffects() and/or
        /// surfaceTurretModule.ReinitialiseTurretProjectilesAndEffects() for each ship/surface turret
        /// this projectile is used on.
        /// </summary>
        public EffectsModule effectsObject = null;
        
        /// <summary>
        /// The ID number for this projectile's destruction effects object prefab (as assigned by the Ship Controller Manager in the scene).
        /// This is the index in the SSCManager effectsObjectTemplatesList. Not defined = -1.
        /// [INTERNAL USE ONLY]
        /// </summary>
        public int effectsObjectPrefabID = -1;

        /// <summary>
        /// The sound or particle FX used when a collision occurs with a shield (instead of the effectsObject). 
        /// If you modify this, call shipControlModule.ReinitialiseShipProjectilesAndEffects() and/or
        /// surfaceTurretModule.ReinitialiseTurretProjectilesAndEffects() for each ship/surface turret
        /// this projectile is used on.
        /// </summary>
        public EffectsModule shieldEffectsObject = null;

        /// <summary>
        /// The ID number for this projectile's effects object prefab (as assigned by the Ship Controller Manager in the scene)
        /// for when a shielded ship is hit.
        /// This is the index in the SSCManager effectsObjectTemplatesList. Not defined = -1.
        /// [INTERNAL USE ONLY]
        /// </summary>
        public int shieldEffectsObjectPrefabID = -1;

        /// <summary>
        /// The sound and/or particle FX used when a projectile is fired from a weapon.
        /// If you modify this, call shipControlModule.ReinitialiseShipProjectilesAndEffects() and/or
        /// surfaceTurretModule.ReinitialiseTurretProjectilesAndEffects() for each ship/surface turret
        /// this projectile is used on.
        /// </summary>
        public EffectsModule muzzleEffectsObject = null;

        /// <summary>
        /// The distance in local space that the muzzle Effects Object should be instantiated
        /// from the weapon firing point. Typically only the z-axis would be used when the projectile
        /// is instantiated in front or forwards from the actual weapon.
        /// </summary>
        public Vector3 muzzleEffectsOffset = Vector3.zero;

        /// <summary>
        /// The ID number for this projectile's muzzle object prefab (as assigned by the Ship Controller Manager in the scene).
        /// This is the index in the SSCManager effectsObjectTemplatesList. Not defined = -1.
        /// [INTERNAL USE ONLY]
        /// </summary>
        public int muzzleEffectsObjectPrefabID = -1;

        /// <summary>
        /// The Id of the ship that fired the projectile
        /// </summary>
        public int sourceShipId = -1;

        /// <summary>
        /// The Squadron which the ship belonged to when it fired the projectile
        /// </summary>
        public int sourceSquadronId = -1;

        /// <summary>
        /// Is this projectile guided to a target with kinematics? Position and velocity will be determined by aiming at a target. It will
        /// not consider the forces required to move the projectile.
        /// NOTE: All projectiles in SSC use kinematics but by default they are fire and forget rather than guided towards a target.
        /// If you modify this, call shipControlModule.ReinitialiseShipProjectilesAndEffects() and/or
        /// surfaceTurretModule.ReinitialiseTurretProjectilesAndEffects() for each ship/surface turret
        /// this projectile is used on.
        /// Currently does not support DOTS/ECS.
        /// </summary>
        public bool isKinematicGuideToTarget = false;

        /// <summary>
        /// The max turning speed in degrees per second for a guided projectile. 
        /// Only relevant when isKinematicGuideToTarget is enabled. 
        /// </summary>
        [Range(10f, 360f)] public float guidedMaxTurnSpeed = 90f;

        /// <summary>
        /// The layer mask used for collision testing for this projectile.
        /// Default is everything.
        /// </summary>
        public LayerMask collisionLayerMask = ~0;

        /// <summary>
        /// The estimated range (in metres) of this projectile assuming it travels at a constant velocity.
        /// </summary>
        public float estimatedRange { get { return startSpeed * despawnTime; } }

        /// <summary>
        /// Current velocity of the projectile.
        /// Should only be updated when using the sscManager.callbackProjectileMoveTo delegate.
        /// </summary>
        public Vector3 Velocity { get { return velocity; } set { if (isKinematicGuideToTarget) { velocity = value; } } }

        /// <summary>
        /// The world space position of the projectile in the current frame
        /// </summary>
        public Vector3 ThisFramePosition { get { return thisFramePosition; } internal set { thisFramePosition = value; } }

        /// <summary>
        /// The world space position of the projectile in the last frame
        /// </summary>
        public Vector3 LastFramePosition { get { return lastFramePosition; } internal set { lastFramePosition = value; } }

        /// <summary>
        /// If a ship is being targeted, will return its name. If it is being targeted by is NULL, will assume destroyed.
        /// </summary>
        public string TargetShipName { get { return isTargetShip ? (targetShip == null ? "Destroyed" : targetShip.name) : "-"; } }

        /// <summary>
        /// If a ship damage region is being targeted, will return its name. If it is being targeted but is NULL, will assume destroyed.
        /// </summary>
        public string TargetShipDamageRegionName { get { return isTargetShipDamageRegion ? (targetShip == null ? "Destroyed" : targetShip.IsInitialised && targetguidHash != 0 ? targetShip.shipInstance.GetDamageRegion(targetguidHash).name : "-") : "-"; } }

        /// <summary>
        /// If a gameobject is being targeted, will return its name
        /// </summary>
        public string TargetGameObjectName { get { return targetGameObject != null ? (string.IsNullOrEmpty(targetGameObject.name) ? "no name" : targetGameObject.name) : "-"; } }

        #endregion

        #region Protected variables
        // These variables can be modified by classes that inherit from ProjectileModule

        protected Vector3 velocity = Vector3.zero;
        protected float speed = 0f;
        protected Vector3 lastFramePosition = Vector3.zero;
        protected Vector3 thisFramePosition = Vector3.zero;

        protected RaycastHit hitInfo;

        protected bool isInitialised = false;

        protected bool isProjectileEnabled = true;

        protected float despawnTimer = 0f;


        #endregion

        #region Private and internal variables

        /// <summary>
        /// From Ship, the magnitude of the acceleration (in metres per second squared) due to gravity.
        /// </summary>
        private float gravitationalAcceleration;
        /// <summary>
        /// From Ship, the direction in world space in which gravity acts upon the ship.
        /// </summary>
        private Vector3 gravityDirection;

        /// <summary>
        /// The current ship (if any) being targeted when isKinematicGuideToTarget is true.
        /// </summary>
        private ShipControlModule targetShip = null;

        /// <summary>
        /// The guidHash of the target. Currently only set for ship damage regions.
        /// </summary>
        private int targetguidHash = 0;

        /// <summary>
        /// The current ship damage region (if any) being targeted when isKinematicGuideToTarget is true.
        /// </summary>
        private DamageRegion targetShipDamageRegion = null;

        /// <summary>
        /// The current GameObject (if any) being targeted when isKinematicGuideToTarget is true.
        /// </summary>
        private GameObject targetGameObject = null;

        // Is a ship being targeted?
        private bool isTargetShip = false;

        /// <summary>
        /// Is a ship's damage region being targeted?
        /// </summary>
        private bool isTargetShipDamageRegion = false;

        /// <summary>
        /// Private reference to the SSCManager in the scene. Currently only populated
        /// when isKinematicGuideToTarget is true. Used to get the CallbackProjectileMoveTo
        /// method from SSCManager.
        /// </summary>
        private SSCManager sscManager = null;

        // Is there a user-defined CallbackProjectileMoveTo configured for this projectile?
        private bool isCallbackOnMoveTo = false;

        /// <summary>
        /// The handle to the scene this projectile is created in
        /// </summary>
        [System.NonSerialized] internal int sceneHandle;

        // Augmented Proportional Navigation (APN) variables
        // current and previous frame's Line of Sight (normalised)
        private Vector3 lastFrameLOSN = Vector3.zero;
        private Vector3 thisFrameLOSN = Vector3.zero;
        private static readonly float NavConst = 3f;

        #endregion

        #region Enable/Disable Event Methods

        void OnDisable()
        {
            isInitialised = false;
        }

        #endregion

        #region Update Methods

        // FixedUpdate is called once per physics update (typically about 50 times per second)
        private void FixedUpdate ()
        {
            if (isInitialised && isProjectileEnabled)
            {
                // Remember last frame position as last frame position - we are about to update this frame position
                lastFramePosition = thisFramePosition;

                CalcPositionAndVelocity();

                // Check to see if the projectile has collided with anything during this frame
                // If nothing hit but using pooling, decrement the timer.
                if (!CheckCollision() && usePooling)
                {
                    // To avoid a pooled projectile being despawned at the incorrect time,
                    // do it here rather than calling Invoke("DestroyProjectile", despawnTime)
                    // when it is initialised.
                    despawnTimer += Time.deltaTime;
                    if (despawnTimer >= despawnTime)
                    {
                        DestroyProjectile();
                    }
                }

                // Update the position of the object
                if (!isCallbackOnMoveTo) { transform.position = thisFramePosition; }
            }
        }

        #endregion

        #region Public Obsolete Methods

        [System.Obsolete("This method will be removed in a future version. Please use InitialiseProjectile (InstantiateProjectileParameters ipParms).")]
        public void InitialiseProjectile(Vector3 weaponVelocity, int projectilePrefabID, float gravity, Vector3 gravityDirection, int shipId, int squadronId)
        {
            InitialiseProjectile(new InstantiateProjectileParameters
            {
                weaponVelocity = weaponVelocity,
                projectilePrefabID = projectilePrefabID,
                gravity = gravity,
                gravityDirection = gravityDirection,
                shipId = shipId,
                squadronId = squadronId,
                // targets are not supported with this older method
                targetShip = null,
                targetGameObject = null,
                targetguidHash = 0
            }
            );
        }

        #endregion

        #region Virtual and Protected Methods

        /// <summary>
        /// Initialises the projectile. If you wish to override this in a child (inherited) class you
        /// almost always will want to call the base method first.
        /// public override void InitialiseProjectile(InstantiateProjectileParameters ipParms)
        /// {
        ///    base.InitialiseProjectile(ipParms);
        ///    // Do stuff here
        /// }
        /// </summary>
        public virtual void InitialiseProjectile(InstantiateProjectileParameters ipParms)
        {
            // Initialise the velocity based on the forwards direction
            // The forwards direction should have been set correctly prior to enabling the object
            speed = startSpeed;
            velocity = transform.forward * speed;
            // Initialise last/this frame positions
            // Shift the position forward by the weapon velocity, so that projectiles don't ever end up behind the ship
            lastFramePosition = transform.position + (velocity * Time.fixedDeltaTime);
            thisFramePosition = lastFramePosition;

            transform.position = thisFramePosition;

            // Add the weapon velocity to the projectile velocity
            // This needs to be done after the above so that the projectiles aren't spawned to the sides of the ship
            velocity += ipParms.weaponVelocity;

            // Store the index to the ProjectileTemplate in the SSCManager projectileTemplatesList
            // This is used with Projectile FX when we know the ProjectileModule but not the parent ProjectileTemplate.
            this.projectilePrefabID = ipParms.projectilePrefabID;

            // Store the index to the EffectsObjectTemplate from the sscManager.effectsObjectTemplatesList
            this.effectsObjectPrefabID = ipParms.effectsObjectPrefabID;
            this.shieldEffectsObjectPrefabID = ipParms.shieldEffectsObjectPrefabID;

            this.gravitationalAcceleration = ipParms.gravity;
            this.gravityDirection = ipParms.gravityDirection;
            this.sourceShipId = ipParms.shipId;
            this.sourceSquadronId = ipParms.squadronId;

            if (!useECS && isKinematicGuideToTarget)
            {
                targetShip = ipParms.targetShip;
                targetGameObject = ipParms.targetGameObject;
                targetguidHash = ipParms.targetguidHash;

                if (ipParms.targetShip != null)
                {
                    targetShipDamageRegion = null;

                    if (targetguidHash != 0 && targetShip.IsInitialised && targetShip.shipInstance != null)
                    {
                        targetShipDamageRegion = targetShip.shipInstance.GetDamageRegion(targetguidHash);
                    }

                    isTargetShipDamageRegion = targetShipDamageRegion != null;
                    isTargetShip = !isTargetShipDamageRegion;
                    
                    // If not a damage region or one was not found, reset the targetguidHash.
                    if (isTargetShip) { targetguidHash = 0; }
                }
                else
                {
                    isTargetShip = false;
                    isTargetShipDamageRegion = false;
                    targetShipDamageRegion = null;
                }

                // reset this frame Line of Sight (normalised)
                thisFrameLOSN.x = 0f;
                thisFrameLOSN.y = 0f;
                thisFrameLOSN.z = 0f;

                // If in a pool, this may have already been called
                if (!usePooling || sscManager == null) { sscManager = SSCManager.GetOrCreateManager(sceneHandle); }

                isCallbackOnMoveTo = sscManager != null && sscManager.callbackProjectileMoveTo != null;
            }
            else
            {
                ClearTarget();
            }

            // After a given amount of time, automatically destroy this projectile
            if (usePooling) { despawnTimer = 0f; }
            else { Invoke("DestroyProjectile", despawnTime); }

            isInitialised = true;
        }

        /// <summary>
        /// Calculate new velocity and thisFramePosition
        /// </summary>
        protected virtual void CalcPositionAndVelocity()
        {
            if (useGravity)
            {
                // F = ma
                // a = dv/dt
                // F = mdv/dt
                // dv = Fdt/m

                velocity += (gravitationalAcceleration * Time.deltaTime * gravityDirection);
            }

            if (isKinematicGuideToTarget)
            {
                if (isCallbackOnMoveTo) { sscManager.callbackProjectileMoveTo(this); }
                else { GuideToTarget(); }
            }

            // Move the projectile according to its current velocity
            thisFramePosition = lastFramePosition + (velocity * Time.deltaTime);
        }

        /// <summary>
        /// Check to see if the projectile has collided with anything during this frame
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckCollision()
        {
            // TODO: Look into whether this can be done using RaycastCommand with JobSystem
            // LayerMask defaults to everything (inverse of nothing ~0). Don't detect trigger colliders
            if (Physics.Linecast(lastFramePosition, thisFramePosition, out hitInfo, collisionLayerMask, QueryTriggerInteraction.Ignore))
            {
                bool isShieldHit = false;
                ShipControlModule shipControlModule;

                // Do we need to check for ship shield hits?
                if (shieldEffectsObjectPrefabID >= 0 && CheckShipHit(hitInfo, damageAmount, damageType, sourceShipId, sourceSquadronId, projectilePrefabID, out shipControlModule))
                {
                    isShieldHit = shipControlModule.shipInstance.HasActiveShield(hitInfo.point);
                }
                // No shield effects so perform a regular CheckShipHit
                else if (shieldEffectsObjectPrefabID < 0 && CheckShipHit(hitInfo, damageAmount, damageType, sourceShipId, sourceSquadronId, projectilePrefabID))
                {
                    // No need to do anything else here
                }
                else
                {
                    // If it hit an object with a DamageReceiver script attached, take appropriate action like call a custom method
                    CheckObjectHit(hitInfo, damageAmount, damageType, sourceShipId, sourceSquadronId, projectilePrefabID);
                }

                if (isShieldHit && shieldEffectsObjectPrefabID >= 0)
                {
                    if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(sceneHandle); }

                    if (sscManager != null)
                    {
                        InstantiateEffectsObjectParameters ieParms = new InstantiateEffectsObjectParameters
                        {
                            effectsObjectPrefabID = shieldEffectsObjectPrefabID,
                            position = hitInfo.point + (hitInfo.normal * 0.0005f),
                            rotation = Quaternion.LookRotation(-hitInfo.normal),
                        };

                        // For projectiles we don't need to get the effectsObject key from ieParms.
                        sscManager.InstantiateEffectsObject(ref ieParms);
                    }
                }
                else if (!isShieldHit && effectsObjectPrefabID >= 0 && effectsObject != null)
                {
                    if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(sceneHandle); }

                    if (sscManager != null)
                    {
                        InstantiateEffectsObjectParameters ieParms = new InstantiateEffectsObjectParameters
                        {
                            effectsObjectPrefabID = effectsObjectPrefabID,
                            position = hitInfo.point,
                            rotation = transform.rotation
                        };

                        // For projectiles we don't need to get the effectsObject key from ieParms.
                        sscManager.InstantiateEffectsObject(ref ieParms);
                    }
                }

                DestroyProjectile();

                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// Removes the projectile. How this is done depends on what system is being used (i.e. pooling etc.).
        /// When overriding, write your own logic then call base.DestroyProjectile().
        /// </summary>
        protected virtual void DestroyProjectile()
        {
            if (usePooling)
            {
                if (isKinematicGuideToTarget)
                {
                    ClearTarget();
                }

                // Deactivate the projectile
                gameObject.SetActive(false);
            }
            else
            {
                // Destroy the projectile
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// It MUST update the velocity and tranform.forward. Does not update the transform.position.
        /// FUTURE - be able to track other rigid bodies, not just Ships.
        /// FUTURE - be able to be guided towards a Location (which may not have a gameObject)
        /// To write your own version set sscManager.callbackProjectileMoveTo(..).
        /// </summary>
        protected void GuideToTarget()
        {
            if (isKinematicGuideToTarget && (isTargetShip || isTargetShipDamageRegion || targetGameObject != null))
            {
                // Did the target become null without us knowing?
                if ((isTargetShip || isTargetShipDamageRegion) && targetShip == null) { ClearTarget(); }
                // If the target ship has been destroyed, stop targetting this ship
                else if ((isTargetShip || isTargetShipDamageRegion) && targetShip.shipInstance.Destroyed()) { ClearTarget(); }
                else
                {
                    // Simple (rubbish) chase-style guidance
                    //transform.LookAt(targetShip.transform);
                    //velocity = transform.forward * velocity.magnitude;

                    // Augmented Proportional Navigation
                    Vector3 targetPosition = isTargetShip ? targetShip.shipInstance.TransformPosition : (isTargetShipDamageRegion ? targetShip.shipInstance.GetDamageRegionWSPosition(targetShipDamageRegion) : targetGameObject.transform.position);

                    lastFrameLOSN = thisFrameLOSN;
                    thisFrameLOSN = (targetPosition - transform.position).normalized;

                    if (lastFrameLOSN.x != 0f || lastFrameLOSN.y != 0f || lastFrameLOSN.z != 0f)
                    {
                        Vector3 deltaLOS = thisFrameLOSN - lastFrameLOSN;

                        // The rate at which the LOS angle is changing
                        // When angleRateLOS is zero the missile is on a collision course with the target.
                        float angleRateLOS = deltaLOS.magnitude;

                        // Closing velocity is -deltaLOS.

                        // Proportional Navigation v = NavConst * closing_velocity * angleRateLOS

                        float apnBias = gravitationalAcceleration * Time.deltaTime * (NavConst * 0.5f);

                        // NOTE: Final velocity should have the same magnitude (length) of the original velocity.
                        Vector3 apn_acceleration = (thisFrameLOSN * (angleRateLOS * NavConst) + (deltaLOS * apnBias)).normalized;
                        // Avoid forward = zero vector.
                        if (apn_acceleration.x == 0f && apn_acceleration.y == 0f && apn_acceleration.z == 0f) { }
                        else
                        {
                            // The initial rotation is defined by the transform.forwards direction
                            // The target rotation is defined by the apn_acceleration direction
                            // In this frame we are only allowed to turn by (guidedMaxTurnSpeed * Time.deltaTime) degrees
                            transform.forward = Quaternion.RotateTowards(Quaternion.LookRotation(transform.forward),
                                Quaternion.LookRotation(apn_acceleration), guidedMaxTurnSpeed * Time.deltaTime) * Vector3.forward;
                        }
                        velocity = transform.forward * velocity.magnitude;

                        // Commanded Acceleration = NavConst * Vc * angleRateLOS  + ( NavConst * Nt ) / 2
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// [INTERNAL ONLY]
        /// Stop targeting anything.
        /// </summary>
        internal void ClearTarget()
        {
            targetShip = null;
            targetGameObject = null;
            targetShipDamageRegion = null;
            targetguidHash = 0;
            isTargetShip = false;
            isTargetShipDamageRegion = false;
        }

        /// <summary>
        /// [INTERNAL ONLY] - subject to change without notice.
        /// Currently does not support DOTS/ECS
        /// </summary>
        /// <param name="shipControlModule"></param>
        internal void SetTargetShip(ShipControlModule shipControlModule)
        {
            if (!useECS && isKinematicGuideToTarget)
            {
                targetShip = shipControlModule;

                isTargetShip = targetShip != null;

                if (isTargetShip) { targetGameObject = shipControlModule.gameObject; }
                else { targetGameObject = null; }
            }
        }

        /// <summary>
        /// [INTERNAL ONLY] - subject to change without notice.
        /// Currently does not support DOTS/ECS.
        /// If a ship is being targeted, call SetTargetShip(..) instead.
        /// </summary>
        /// <param name="targetGameObj"></param>
        internal void SetTarget(GameObject targetGameObj)
        {
            if (!useECS && isKinematicGuideToTarget)
            {
                targetGameObject = targetGameObj;

                // clear target ship
                targetShip = null;
                isTargetShip = false;
            }
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Determine if a ship has been hit. Apply damage as required.
        /// Can be called by both an instance of ProjectileModule and ProjectileSystem.
        /// </summary>
        /// <param name="raycastHitInfo"></param>
        /// <param name="projectileDamageAmount"></param>
        /// <param name="projectileDamageType"></param>
        /// <param name="sourceShipId"></param>
        /// <param name="sourceShipSquadronId"></param>
        /// <param name="projectilePrefabID"></param>
        /// <returns></returns>
        public static bool CheckShipHit (RaycastHit raycastHitInfo, float projectileDamageAmount, DamageType projectileDamageType, 
            int sourceShipId, int sourceShipSquadronId, int projectilePrefabID)
        {
            bool isHit = false;
            Rigidbody hitRigidbody = raycastHitInfo.rigidbody;

            if (hitRigidbody != null)
            {
                // Check if there is a ship control module attached to the hit object
                ShipControlModule shipControlModule;

                if (hitRigidbody.TryGetComponent(out shipControlModule))
                {
                    // Apply damage to the ship
                    shipControlModule.shipInstance.ApplyNormalDamage(projectileDamageAmount, projectileDamageType, raycastHitInfo.point);

                    // If required, call the custom method
                    if (shipControlModule.callbackOnHit != null)
                    {
                        // Get a reference to the SSCManager (assume ship and projectile are in the same scene)
                        SSCManager sscManager = SSCManager.GetOrCreateManager(shipControlModule.sceneHandle);

                        // Create a struct with the necessary parameters
                        CallbackOnShipHitParameters callbackOnShipHitParameters = new CallbackOnShipHitParameters
                        {
                            hitInfo = raycastHitInfo,
                            projectilePrefab = sscManager.GetProjectilePrefab(projectilePrefabID),
                            beamPrefab = null,
                            damageAmount = projectileDamageAmount,
                            sourceSquadronId = sourceShipSquadronId,
                            sourceShipId = sourceShipId
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
        /// Determine if a ship has been hit. Apply damage as required.
        /// </summary>
        /// <param name="raycastHitInfo"></param>
        /// <param name="projectileDamageAmount"></param>
        /// <param name="sourceShipId"></param>
        /// <param name="sourceShipSquadronId"></param>
        public static bool CheckShipHit (RaycastHit raycastHitInfo, float projectileDamageAmount, DamageType projectileDamageType, 
            int sourceShipId, int sourceShipSquadronId, int projectilePrefabID, out ShipControlModule shipControlModule)
        {
            bool isHit = false;
            Rigidbody hitRigidbody = raycastHitInfo.rigidbody;

            if (hitRigidbody != null)
            {
                // Check if there is a ship control module attached to the hit object
                if (hitRigidbody.TryGetComponent(out shipControlModule))
                {
                    // Apply damage to the ship
                    shipControlModule.shipInstance.ApplyNormalDamage(projectileDamageAmount, projectileDamageType, raycastHitInfo.point);

                    // If required, call the custom method
                    if (shipControlModule.callbackOnHit != null)
                    {
                        // Get a reference to the SSCManager (assume ship and projectile are in the same scene)
                        SSCManager sscManager = SSCManager.GetOrCreateManager(shipControlModule.sceneHandle);

                        // Create a struct with the necessary parameters
                        CallbackOnShipHitParameters callbackOnShipHitParameters = new CallbackOnShipHitParameters
                        {
                            hitInfo = raycastHitInfo,
                            projectilePrefab = sscManager.GetProjectilePrefab(projectilePrefabID),
                            beamPrefab = null,
                            damageAmount = projectileDamageAmount,
                            sourceSquadronId = sourceShipSquadronId,
                            sourceShipId = sourceShipId
                        };
                        // Call the custom callback
                        shipControlModule.callbackOnHit(callbackOnShipHitParameters);
                    }

                    isHit = true;
                }
            }
            else { shipControlModule = null; }

            return isHit;
        }

        /// <summary>
        /// Determine if an object with a DamageReceiver component attached has been hit by a Projectile.
        /// TODO - check performance
        /// </summary>
        /// <param name="raycastHitInfo"></param>
        /// <param name="projectileDamageAmount"></param>
        /// <param name="sourceShipId"></param>
        /// <param name="sourceShipSquadronId"></param>
        /// <param name="projectilePrefabID"></param>
        /// <returns></returns>
        public static bool CheckObjectHit (RaycastHit raycastHitInfo, float projectileDamageAmount, DamageType projectileDamageType,
            int sourceShipId, int sourceShipSquadronId, int projectilePrefabID)
        {
            bool isHit = false;

            if (raycastHitInfo.collider != null)
            {
                DamageReceiver damageReceiver = raycastHitInfo.collider.GetComponent<DamageReceiver>();
                if (damageReceiver != null && damageReceiver.callbackOnHit != null)
                {
                    // Get a reference to the SSCManager (assume damage receiver and projectile are in the same scene)
                    SSCManager sscManager = SSCManager.GetOrCreateManager(damageReceiver.gameObject.scene.handle);

                    // Create a struct with the necessary parameters
                    CallbackOnObjectHitParameters callbackOnObjectHitParameters = new CallbackOnObjectHitParameters
                    {
                        hitInfo = raycastHitInfo,
                        projectilePrefab = sscManager.GetProjectilePrefab(projectilePrefabID),
                        beamPrefab = null,
                        damageAmount = projectileDamageAmount,
                        sourceSquadronId = sourceShipSquadronId
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
        /// If 
        /// </summary>
        /// <returns></returns>
        public ShipControlModule GetTargetShip()
        {
            if (isKinematicGuideToTarget) { return targetShip; }
            else { return null; }
        }

        /// <summary>
        /// Re-enable a projectile after it has been disabled with
        /// DisableProjectile(). See also SSCManager.ResumeProjectiles()
        /// </summary>
        public void EnableProjectile()
        {
            isProjectileEnabled = true;
        }

        /// <summary>
        /// Useful when you want to pause the action in a game.
        /// Should always be called BEFORE setting Time.timeScale to 0.
        /// See also SSCManager.PauseProjectiles().
        /// </summary>
        public void DisableProjectile()
        {
            isProjectileEnabled = false;
        }

        #endregion
    }
}
