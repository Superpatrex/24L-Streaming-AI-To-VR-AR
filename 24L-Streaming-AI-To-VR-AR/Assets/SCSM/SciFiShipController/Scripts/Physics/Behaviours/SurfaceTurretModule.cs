using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Attach to stationary ground-based turrets.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Weapon Components/Surface Turret Module")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SurfaceTurretModule : MonoBehaviour
    {
        #region Enumerations

        #endregion

        #region Public variables

        /// <summary>
        /// If enabled, the Initialise() will be called as soon as Start() runs. This should be disabled if you are
        /// initialising the turret through code and using the SurfaceTurretModule API methods.
        /// </summary>
        public bool initialiseOnStart = false;

        /// <summary>
        /// The faction or alliance the item belongs to. This can be used to identify if an item is friend or foe.
        /// Default (neutral) is 0
        /// </summary>
        public int factionId = 0;

        /// <summary>
        /// Although normally representing a squadron of ships, this can be used on a turret to group it with other things in your scene
        /// </summary>
        public int squadronId = -1;

        /// <summary>
        /// Automatically create a Location in the SSCManager when turret is initialised
        /// </summary>
        public bool autoCreateLocation = false;

        /// <summary>
        /// Is this turret (Location) visible to radar queries? The turret needs a LocationData
        /// entry in SSCManager to appear on radar.
        /// </summary>
        public bool isVisibleToRadar;

        /// <summary>
        /// The relative size of the blip on the radar mini-map
        /// </summary>
        [Range(1, 5)] public byte radarBlipSize = 1;

        /// <summary>
        /// The magnitude of the acceleration (in metres per second squared) due to gravity.
        /// This affects how a projectile travels after it has been fired.
        /// </summary>
        public float gravitationalAcceleration;

        /// <summary>
        /// The direction in world space in which gravity acts upon the turret.
        /// This affects how a projectile travels after it has been fired.
        /// </summary>
        public Vector3 gravityDirection;

        /// <summary>
        /// The sound or particle FX used when the turret is destroyed. It should automatically be despawned when the turret is destroyed.
        /// If you modify this, call ReinitialiseTurretProjectilesAndEffects().
        /// </summary>
        public EffectsModule destructionEffectsObject;

        /// <summary>
        /// This is used when you want pre-build fragments of the turret to explode out from the turrets position when it is destroyed.
        /// If you modify this, call ReinitialiseTurretDestructObjects().
        /// </summary>
        public DestructModule destructObject;

        /// <summary>
        /// Should the turret be removed from the scene when its health reaches 0?
        /// </summary>
        public bool isDestroyOnNoHealth = false;

        /// <summary>
        /// [READONLY] Has the turret been initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// [READONLY] The position of the turret as a vector. Derived from the position of the transform.
        /// </summary>
        public Vector3 TransformPosition { get { return trfmPos; } }

        /// <summary>
        /// [READONLY] The forward direction of the turret in world space as a vector. Derived from the forward direction of the transform. 
        /// </summary>
        public Vector3 TransformForward { get { return trfmFwd; } }

        /// <summary>
        /// [READONLY] The right direction of the turret in world space as a vector. Derived from the right direction of the transform.
        /// </summary>
        public Vector3 TransformRight { get { return trfmRight; } }

        /// <summary>
        /// [READONLY] The up direction of the turret in world space as a vector. Derived from the up direction of the transform.
        /// </summary>
        public Vector3 TransformUp { get { return trfmUp; } }

        /// <summary>
        /// [READONLY] The rotation of the turret in world space as a quaternion. Derived from the rotation of the transform.
        /// </summary>
        public Quaternion TransformRotation { get { return trfmRot; } }

        /// <summary>
        /// [READONLY] The inverse rotation of the turret in world space as a quaternion. Derived from the rotation of the transform.
        /// </summary>
        public Quaternion TransformInverseRotation { get { return trfmInvRot; } }

        /// <summary>
        /// [READONLY] The number used by the SSCRadar system to identify this Surface Turret at a point in time.
        /// This should not be stored across frames and is updated as required by the system.
        /// </summary>
        public int RadarId { get { return radarItemIndex; } }

        /// <summary>
        /// [READONLY] Has the surface turret been destroyed (typically when there is no weapon Health)
        /// </summary>
        public bool IsDestroyed { get; private set; }

        /// <summary>
        /// If a ship is being targeted, will return its name.
        /// </summary>
        public string TargetShipName { get { return weapon.targetShip != null ? weapon.targetShip.name : "-"; } }

        /// <summary>
        /// If a ship damage region is being targeted, will return its name.
        /// </summary>
        public string TargetShipDamageRegionName { get { return weapon.targetShipDamageRegion != null ? weapon.targetShipDamageRegion.name : "-"; } }

        /// <summary>
        /// If a gameobject is being targeted, will return its name
        /// </summary>
        public string TargetGameObjectName { get { return weapon.target != null ? (string.IsNullOrEmpty(weapon.target.name) ? "no name" : weapon.target.name) : "-"; } }

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public Weapon weapon;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool allowRepaint = false;

        #endregion

        #region Public Delegates

        public delegate void CallbackOnDestroy(SurfaceTurretModule surfaceTurretModule);

        /// <summary>
        /// The name of the custom method that is called immediately
        /// before the turret is destroyed. Your method must take 1
        /// parameter of class SurfaceTurretModule. This should be a lightweight
        /// method to avoid performance issues. It could be used to update
        /// a score or affect the status of a mission.
        /// </summary>
        public CallbackOnDestroy callbackOnDestroy = null;

        #endregion

        #region Internal variables
        /// <summary>
        /// The ID number for this turret's destruction effects object prefab (as assigned by the SSCManager in the scene).
        /// This is the index in the SSCManager effectsObjectTemplatesList.
        /// [INTERNAL USE ONLY]
        /// </summary>
        internal int effectsObjectPrefabID = -1;

        /// <summary>
        /// Flag for whether the destruction effects object has been instantiated.
        /// [INTERNAL USE ONLY]
        /// </summary>
        internal bool isDestructionEffectsObjectInstantiated = false;

        /// <summary>
        /// The ID number for this turret's destruct prefab (as assigned by the SSCManager in the scene).
        /// This is the index in the SSCManager destructTemplateList.
        /// [INTERNAL USE ONLY]
        /// </summary>
        internal int destructObjectPrefabID = -1;

        /// <summary>
        /// Flag for whether the destruct object has been activated.
        /// [INTERNAL USE ONLY]
        /// </summary>
        internal bool isDestructObjectActivated = false;

        // Radar variables
        /// <summary>
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] internal int radarItemIndex = -1;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Contains a reference to the scene this turret is located in
        /// </summary>
        [System.NonSerialized] internal int sceneHandle = 0;

        #endregion

        #region Private varibles
        private bool isInitialised = false;
        private SSCManager sscManager = null;
        private SSCRadar sscRadar = null;
        private LocationData locationData = null;

        /// <summary>
        /// [INTERNAL ONLY] - instead use FireIfReady()
        /// Attempt to manually fire the weapon
        /// </summary>
        private bool isManualFireNow = false;

        /// <summary>
        /// [INTERNAL ONLY] - instead use SetAutoFire()
        /// </summary>
        private bool isAutoFire = false;

        // Temp firing variables
        private Vector3 weaponWorldBasePosition = Vector3.zero;
        private Vector3 weaponWorldFirePosition = Vector3.zero;
        private Vector3 weaponWorldFireDirection = Vector3.zero;
        private Vector3 weaponRelativeFirePosition = Vector3.zero;
        private Vector3 weaponRelativeFireDirection = Vector3.zero;

        // cached data
        private Vector3 worldVelocity = Vector3.zero;
        private Vector3 worldAngularVelocity = Vector3.zero;
        private Vector3 trfmUp;
        private Vector3 trfmFwd;
        private Vector3 trfmRight;
        private Vector3 trfmPos;
        private Quaternion trfmRot;
        private Quaternion trfmInvRot;

        private int weaponTypeInt;

        #endregion

        #region Initialise Methods

        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        /// <summary>
        /// Initialise the Turret
        /// </summary>
        public void Initialise()
        {
            // Only initialise once
            if (!isInitialised)
            {
                #if UNITY_EDITOR
                if (gameObject.GetComponent<ShipControlModule>())
                {
                    Debug.LogWarning("ERROR: This Turret Module is NOT designed to be used on a Ship. Please configure a Turret under Weapons on the Ship Control Module Combat tab.");
                    return;
                }
                #endif

                UpdatePositionData();

                sceneHandle = gameObject.scene.handle;

                if (weapon != null)
                {
                    // Before v1.2.3, surface turrets didn't set the weaponType in the editor.
                    if (weapon.weaponType != Weapon.WeaponType.TurretProjectile && weapon.weaponType != Weapon.WeaponType.TurretBeam) { weapon.weaponType = Weapon.WeaponType.TurretProjectile; }
                    weapon.Health = weapon.startingHealth;

                    // Added in v1.2.3
                    weapon.Initialise(trfmInvRot);

                    // Lookup the enumeration once
                    weaponTypeInt = weapon.weaponTypeInt;

                    // Get a reference to the Ship Controller Manager instance
                    sscManager = SSCManager.GetOrCreateManager(sceneHandle);

                    // Initialise projectiles and effects objects
                    ReinitialiseTurretProjectilesAndEffects();

                    // Initialise beams and (beam) effects
                    ReinitialiseTurretBeams();

                    // Initialise destruct modules
                    ReinitialiseTurretDestructObjects();

                    // When initialising, there should never be an existing Location
                    if (autoCreateLocation) { CreateLocation(false); }
                    else if (isVisibleToRadar)
                    {
                        // No location created but visible to radar, so create as a RadarItemType.GameObject
                        if (sscRadar == null) { sscRadar = SSCRadar.GetOrCreateRadar(); }
                        if (sscRadar != null) { radarItemIndex = sscRadar.EnableRadar(gameObject, transform.position, factionId, squadronId, 0, radarBlipSize); }
                    }

                    isManualFireNow = false;
                    isAutoFire = weapon.firingButton == Weapon.FiringButton.AutoFire;

                    if (weapon.turretPivotY == null)
                    {
                        isInitialised = false;
                        #if UNITY_EDITOR
                        Debug.LogWarning("ERROR: The surface turret (" + gameObject.name + ") needs to be assigned a Pivot Y transform before it can fire projectiles." );
                        #endif
                    }
                    else if (weapon.turretPivotX == null)
                    {
                        isInitialised = false;
                        #if UNITY_EDITOR
                        Debug.LogWarning("ERROR: The surface turret (" + gameObject.name + ") needs to be assigned a Pivot X transform before it can fire projectiles." );
                        #endif
                    }
                    else
                    {
                        // If there is a DamageReceiver component attached, setup the callback.
                        DamageReceiver damageReceiver = GetComponent<DamageReceiver>();
                        if (damageReceiver != null) { damageReceiver.callbackOnHit = TakeDamage; }
                        isInitialised = true;
                    }
                }
            }
        }

        #endregion

        #region Update Methods
        // Update is called once per frame
        void Update()
        {
            UpdateWeapon();
        }

        #endregion

        #region Private and Internal Member Methods

        /// <summary>
        /// This gets called from FixedUpdate in BeamModule. It performs the following:
        /// 1. Checks if the beam should be despawned
        /// 2. Moves the beam
        /// 3. Changes the length
        /// 4. Checks if it hits anything
        /// 5. Updates damage on what it hits
        /// 6. Instantiate effects object at hit point
        /// 7. Consumes weapon power
        /// It needs to be a member of the surfaceTurretModule instance as it requires both turret and beam data.
        /// Assumes the beam linerenderer has useWorldspace enabled.
        /// </summary>
        /// <param name="beamModule"></param>
        internal void MoveBeam(BeamModule beamModule)
        {
            if (beamModule.isInitialised && beamModule.isBeamEnabled && beamModule.weaponIndex >= 0 && beamModule.firePositionIndex >= 0)
            {
                SSCBeamItemKey beamItemKey = weapon.beamItemKeyList[beamModule.firePositionIndex];

                if (weapon.weaponTypeInt == Weapon.TurretBeamInt && beamItemKey.beamSequenceNumber == beamModule.itemSequenceNumber)
                {
                    // Is ready to fire - taken from UpdateWeapon()
                    bool isReadyToFire = (isManualFireNow || isAutoFire) && weapon.isLockedOnTarget && (!weapon.checkLineOfSight || WeaponHasLineOfSight(weapon.target));

                    // Should this beam be despawned or returned to the pool?
                    // a) No charge in weapon
                    // b) Weapon has no health
                    // c) Weapon has no performance
                    // d) user stopped firing and has fired for min time permitted
                    // e) has exceeded the maximum firing duration
                    if (weapon.chargeAmount <= 0f || weapon.health <= 0f || weapon.currentPerformance == 0f || (!isReadyToFire && beamModule.burstDuration > beamModule.minBurstDuration) || beamModule.burstDuration > beamModule.maxBurstDuration)
                    {
                        // Unassign the beam from this weapon's fire position
                        weapon.beamItemKeyList[beamModule.firePositionIndex] = new SSCBeamItemKey(-1, -1, 0);

                        beamModule.DestroyBeam();
                    }
                    else
                    {
                        // Move the beam start
                        // Calculate the World-space Fire Position (like when weapon is first fired)
                        // Note: That the surface turret could have moved and rotated, so we recalc here.
                        Vector3 _weaponWSBasePos = trfmPos;
                        Vector3 _weaponWSFireDir = GetWeaponWorldFireDirection();
                        Vector3 _weaponWSFirePos = GetWeaponWorldFirePosition(_weaponWSBasePos, beamModule.firePositionIndex);

                        // Move the beam transform but keep the first LineRenderer position at 0,0,0
                        beamModule.transform.position = _weaponWSFirePos;

                        beamModule.transform.SetPositionAndRotation(_weaponWSFirePos, Quaternion.LookRotation(_weaponWSFireDir, trfmUp));

                        // Calc length and end position
                        float _desiredLength = beamModule.burstDuration * beamModule.speed;

                        if (_desiredLength > weapon.maxRange) { _desiredLength = weapon.maxRange; }

                        Vector3 _endPosition = _weaponWSFirePos + (_weaponWSFireDir * _desiredLength);

                        RaycastHit raycastHitInfo;

                        // Check if it hit anything
                        if (Physics.Linecast(_weaponWSFirePos, _endPosition, out raycastHitInfo, ~0, QueryTriggerInteraction.Ignore))
                        {
                            // Adjust the end position to the hit point
                            _endPosition = raycastHitInfo.point;

                            // Update damage if it hit a ship or an object with a damage receiver
                            if (!BeamModule.CheckShipHit(raycastHitInfo, beamModule, Time.deltaTime))
                            {
                                // If it hit an object with a DamageReceiver script attached, take appropriate action like call a custom method
                                BeamModule.CheckObjectHit(raycastHitInfo, beamModule, Time.deltaTime);
                            }

                            // ISSUES - the effect may not be visible while firing if:
                            // 1) if the effect despawn time is less than the beam max burst duration (We have a warning in the BeamModule editor)
                            // 2) if the effect does not have looping enabled (this could be more expensive to check)

                            // Add or Move the effects object
                            if (beamModule.effectsObjectPrefabID >= 0)
                            {
                                if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(beamModule.sceneHandle); }

                                if (sscManager != null)
                                {
                                    // If the effect has not been spawned, do now
                                    if (beamModule.effectsItemKey.effectsObjectSequenceNumber == 0)
                                    {
                                        InstantiateEffectsObjectParameters ieParms = new InstantiateEffectsObjectParameters
                                        {
                                            effectsObjectPrefabID = beamModule.effectsObjectPrefabID,
                                            position = _endPosition,
                                            rotation = beamModule.transform.rotation
                                        };

                                        // Instantiate the hit effects
                                        if (sscManager.InstantiateEffectsObject(ref ieParms) != null)
                                        {
                                            if (ieParms.effectsObjectSequenceNumber > 0)
                                            {
                                                // Record the muzzle effect item key
                                                beamModule.effectsItemKey = new SSCEffectItemKey(ieParms.effectsObjectPrefabID, ieParms.effectsObjectPoolListIndex, ieParms.effectsObjectSequenceNumber);
                                            }
                                        }
                                    }
                                    // Move the existing effects object to the end of the beam
                                    else
                                    {
                                        // Currently we are not checking sequence number matching (for pooled effects) as it is faster and can
                                        // avoid doing an additional GetComponent().
                                        sscManager.MoveEffectsObject(beamModule.effectsItemKey, _endPosition, beamModule.transform.rotation, false);
                                    }
                                }
                            }
                        }
                        // The beam isn't hitting anything AND there is an active effects object
                        else if (beamModule.effectsObjectPrefabID >= 0 && beamModule.effectsItemKey.effectsObjectSequenceNumber > 0)
                        {
                            // Destroy the effects object or return it to the pool
                            if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(beamModule.sceneHandle); }

                            if (sscManager != null)
                            {
                                sscManager.DestroyEffectsObject(beamModule.effectsItemKey);
                            }
                        }

                        // Move the beam end (assumes world space)
                        // useWorldspace is enabled in beamModule.InitialiseBeam(..)
                        beamModule.lineRenderer.SetPosition(0, _weaponWSFirePos);
                        beamModule.lineRenderer.SetPosition(1, _endPosition);

                        // Consume weapon power. Consumes upt to 2x power when overheating.
                        if (weapon.rechargeTime > 0f) { weapon.chargeAmount -= Time.deltaTime * (weapon.heatLevel > weapon.overHeatThreshold ? 2f - weapon.currentPerformance : 1f) / beamModule.dischargeDuration; }

                        weapon.ManageHeat(Time.deltaTime, 1f);

                        // If we run out of power, de-activate it before weapon starts recharging
                        if (weapon.chargeAmount <= 0f)
                        {
                            weapon.chargeAmount = 0f;

                            // Unassign the beam from this weapon's fire position
                            weapon.beamItemKeyList[beamModule.firePositionIndex] = new SSCBeamItemKey(-1, -1, 0);

                            beamModule.DestroyBeam();
                        }
                    }
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR surfaceTurretModule.MoveBeam has been called on the wrong beam. isInitialised: " + beamModule.isInitialised + 
                  " isBeamEnabled: " + beamModule.isBeamEnabled + " weaponIndex: " + beamModule.weaponIndex + " firePositionIndex: " + beamModule.firePositionIndex); }
            #endif
        }

        /// <summary>
        /// If muzzle FX on this weapon are pooled and have been parented to the weapons,
        /// when a surface turret is destroyed, they need to be reparented to the pool.
        /// NOTE: This may impact GC (if this becomes a problem, we could precreate a list
        /// with the appropriate capacity.
        /// </summary>
        private void DestroyFX()
        {
            EffectsModule[] effectsModules = GetComponentsInChildren<EffectsModule>(true);

            int numEffectsModules = effectsModules == null ? 0 : effectsModules.Length;

            for (int emIdx = 0; emIdx < numEffectsModules; emIdx++)
            {
                EffectsModule effectsModule = effectsModules[emIdx];

                if (effectsModule.usePooling && effectsModule.isReparented)
                {
                    effectsModule.DestroyEffectsObject();
                }
            }
        }

        private Vector3 GetWeaponWorldFireDirection()
        {
            // Get relative fire direction
            weaponRelativeFireDirection = weapon.fireDirectionNormalised;

            // Adjust relative fire direction based on turret rotation
            // (turret rotation less parent object rotation) - (original turret rotation less original rotation) + relative fire direction
            weaponRelativeFireDirection = (trfmInvRot * weapon.turretPivotX.rotation) * weapon.intPivotYInvRot * weaponRelativeFireDirection;

            // Get weapon world fire direction
            return (trfmRight * weaponRelativeFireDirection.x) +
                   (trfmUp * weaponRelativeFireDirection.y) +
                   (trfmFwd * weaponRelativeFireDirection.z);
        }


        private Vector3 GetWeaponWorldFirePosition(Vector3 weaponWorldBasePosition, int firePositionIndex)
        {
            weaponRelativeFirePosition = weapon.relativePosition;

            // Check if there are multiple fire positions
            if (weapon.isMultipleFirePositions)
            {
                // Get relative fire position
                weaponRelativeFirePosition += weapon.firePositionList[firePositionIndex];
            }

            // Adjust relative fire position based on turret rotation
            weaponRelativeFirePosition = (trfmInvRot * weapon.turretPivotX.rotation) * weaponRelativeFirePosition;

            // Get weapon world fire position
            return weaponWorldBasePosition +
                (trfmRight * weaponRelativeFirePosition.x) +
                (trfmUp * weaponRelativeFirePosition.y) +
                (trfmFwd * weaponRelativeFirePosition.z);
        }

        /// <summary>
        /// Update cached values.
        /// NOTE: currently world velocity and angular velocity etc
        /// are NOT updated.
        /// </summary>
        private void UpdatePositionData()
        {
            // Update data obtained from transform
            trfmPos = transform.position;
            trfmFwd = transform.forward;
            trfmRight = transform.right;
            trfmUp = transform.up;
            trfmRot = transform.rotation;
            trfmInvRot = Quaternion.Inverse(trfmRot);
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Update weapon and fire if ready
        /// Weapons that have no health, cannot aim, reload or fire.
        /// </summary>
        private void UpdateWeapon()
        {
            // Check that the weapon has had a projectile ID or beam ID assigned
            // Assumes that the IDs have been correctly assigned based on the weaponType
            if (isInitialised && (weapon.projectilePrefabID >= 0 || weapon.beamPrefabID >= 0))
            {
                float _deltaTime = Time.deltaTime;

                // Does the weapon need to cool down??
                weapon.ManageHeat(_deltaTime, 0f);

                // A weapon with no health or performance cannot rotate, reload, or fire.
                if (weapon.health > 0f && weapon.currentPerformance > 0f)
                {
                    weapon.MoveTurret(worldVelocity, true);

                    // Does the beam weapon need charging?
                    if (weaponTypeInt == Weapon.TurretBeamInt && weapon.rechargeTime > 0f && weapon.chargeAmount < 1f)
                    {
                        weapon.chargeAmount += _deltaTime * 1f / weapon.rechargeTime;
                        if (weapon.chargeAmount > 1f) { weapon.chargeAmount = 1f; }
                    }

                    // Check if this projectile weapon is reloading or not
                    // Check if this beam weapon is powering-up or not
                    if (weapon.reloadTimer > 0f)
                    {
                        // If reloading (or powering-up), update reload timer
                        weapon.reloadTimer -= _deltaTime;
                    }
                    else if (weapon.ammunition != 0)
                    {
                        // Is turret locked on target and in direct line of sight?
                        // NOTE: If check LoS is enabled, it will still fire if another enemy is between the turret and the weapon.target.
                        bool isReadyToFire = (isManualFireNow || isAutoFire) && weapon.isLockedOnTarget && (!weapon.checkLineOfSight || WeaponHasLineOfSight(weapon.target));

                        if (isReadyToFire)
                        {
                            // Get base weapon world position (not accounting for relative fire position)
                            weaponWorldBasePosition = trfmPos;

                            // Get relative fire direction
                            weaponRelativeFireDirection = weapon.fireDirectionNormalised;
                            // Adjust relative fire direction based on turret rotation
                            // (turret rotation less parent object rotation) - (original turret rotation less original rotation) + relative fire direction
                            weaponRelativeFireDirection = (trfmInvRot * weapon.turretPivotX.rotation) * weapon.intPivotYInvRot * weaponRelativeFireDirection;

                            // Get weapon world fire direction
                            weaponWorldFireDirection = (trfmRight * weaponRelativeFireDirection.x) +
                                        (trfmUp * weaponRelativeFireDirection.y) +
                                        (trfmFwd * weaponRelativeFireDirection.z);

                            // Start the firing cycle with no heat generated
                            float heatValue = 0f;

                            // Loop through all fire positions
                            int firePositionListCount = weapon.isMultipleFirePositions ? weapon.firePositionList.Count : 1;
                            for (int fp = 0; fp < firePositionListCount; fp++)
                            {
                                // Only fire if there is unlimited ammunition (-1) or greater than 0 projectiles available
                                if (weapon.ammunition != 0)
                                {
                                    // If not unlimited ammo, decrement the quantity available
                                    if (weapon.ammunition > 0) { weapon.ammunition--; }

                                    // Get relative fire position
                                    weaponRelativeFirePosition = weapon.relativePosition;
                                    // If there are multiple fire positions, add the relative fire position
                                    if (weapon.isMultipleFirePositions)
                                    {
                                        weaponRelativeFirePosition += weapon.firePositionList[fp];
                                    }

                                    // Adjust relative fire position based on turret rotation
                                    //weaponRelativeFirePosition = (trfmInvRot * weapon.turretPivotX.rotation) * weapon.intPivotYInvRot * weaponRelativeFirePosition;
                                    weaponRelativeFirePosition = (trfmInvRot * weapon.turretPivotX.rotation) * weaponRelativeFirePosition;

                                    // Get weapon world fire position
                                    weaponWorldFirePosition = weaponWorldBasePosition +
                                        (trfmRight * weaponRelativeFirePosition.x) +
                                        (trfmUp * weaponRelativeFirePosition.y) +
                                        (trfmFwd * weaponRelativeFirePosition.z);

                                    // Create a new beam or projectile using the SSCManager
                                    // Velocity is world velocity of SurfaceTurretModule plus relative velocity of weapon due to angular velocity
                                    if (weaponTypeInt == Weapon.TurretBeamInt)
                                    {
                                        // if the sequence number is 0, it means it is not active
                                        if (weapon.beamItemKeyList[fp].beamSequenceNumber == 0)
                                        {
                                            InstantiateBeamParameters ibParms = new InstantiateBeamParameters()
                                            {
                                                beamPrefabID = weapon.beamPrefabID,
                                                position = weaponWorldFirePosition,
                                                fwdDirection = weaponWorldFireDirection,
                                                upDirection = trfmUp,
                                                shipId = 0,
                                                squadronId = squadronId,
                                                weaponIndex = 0,
                                                firePositionIndex = fp,
                                                beamSequenceNumber = 0,
                                                beamPoolListIndex = -1
                                            };

                                            // Create a beam using the ship control Manager (SSCManager)
                                            // Pass InstantiateBeamParameters by reference so we can get the beam index and sequence number back
                                            BeamModule beamModule = sscManager.InstantiateBeam(ref ibParms);
                                            if (beamModule != null)
                                            {
                                                beamModule.callbackOnMove = MoveBeam;
                                                // Retrieve the unique identifiers for the beam instance
                                                weapon.beamItemKeyList[fp] = new SSCBeamItemKey(weapon.beamPrefabID, ibParms.beamPoolListIndex, ibParms.beamSequenceNumber);
                                                // Immediately update the beam position (required for pooled beams that have previously been used)
                                                MoveBeam(beamModule);

                                                // Was a poolable muzzle fx spawned?
                                                if (beamModule.muzzleEffectsItemKey.effectsObjectSequenceNumber > 0)
                                                {
                                                    // Only get the transform if the muzzle EffectsModule has Is Reparented enabled.
                                                    Transform muzzleFXTrfm = sscManager.GetEffectsObjectTransform(beamModule.muzzleEffectsItemKey.effectsObjectTemplateListIndex, beamModule.muzzleEffectsItemKey.effectsObjectPoolListIndex, true);

                                                    if (muzzleFXTrfm != null)
                                                    {
                                                        muzzleFXTrfm.SetParent(beamModule.transform);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        InstantiateProjectileParameters ipParms = new InstantiateProjectileParameters()
                                        {
                                            projectilePrefabID = weapon.projectilePrefabID,
                                            position = weaponWorldFirePosition,
                                            fwdDirection = weaponWorldFireDirection,
                                            upDirection = trfmUp,
                                            weaponVelocity = worldVelocity + Vector3.Cross(worldAngularVelocity, weaponWorldFirePosition - trfmPos),
                                            gravity = gravitationalAcceleration,
                                            gravityDirection = gravityDirection,
                                            shipId = 0,
                                            squadronId = -1,
                                            targetShip = weapon.isProjectileKGuideToTarget ? weapon.targetShip : null,
                                            targetGameObject = weapon.isProjectileKGuideToTarget ? weapon.target : null,
                                            targetguidHash = weapon.isProjectileKGuideToTarget ? weapon.targetguidHash : 0,
                                        };

                                        sscManager.InstantiateProjectile(ref ipParms);

                                        // For now, assume projectile was instantiated
                                        // Heat value is inversely proportional to the firing interval (reload time)
                                        if (weapon.reloadTime > 0f) { heatValue += 1f / weapon.reloadTime; };

                                        // Was a muzzle fx spawned?
                                        if (ipParms.muzzleEffectsObjectPoolListIndex >= 0)
                                        {
                                            // Only get the transform if the muzzle EffectsModule has Is Reparented enabled.
                                            Transform muzzleFXTrfm = sscManager.GetEffectsObjectTransform(ipParms.muzzleEffectsObjectPrefabID, ipParms.muzzleEffectsObjectPoolListIndex, true);

                                            if (muzzleFXTrfm != null)
                                            {
                                                muzzleFXTrfm.SetParent(weapon.turretPivotX);
                                            }
                                        }
                                    }

                                    // Set reload timer to reload time
                                    weapon.reloadTimer = weapon.reloadTime;
                                }
                            }

                            if (heatValue > 0f) { weapon.ManageHeat(_deltaTime, heatValue); }
                        }
                    }
                }
                isManualFireNow = false;
            }
        }

        #endregion

        #region Internal Public Methods

        /// <summary>
        /// [INTERNAL ONLY]
        /// When DamageReceiver component is attached, this routine is called by Sci-Fi Ship Controller when a projectile hits it.
        /// If it is destroyed, it will no longer appear on radar.
        /// </summary>
        /// <param name="callbackOnObjectHitParameters"></param>
        public void TakeDamage(CallbackOnObjectHitParameters callbackOnObjectHitParameters)
        {
            if (isInitialised)
            {
                ProjectileModule projectile = callbackOnObjectHitParameters.projectilePrefab;
                if (projectile != null)
                {
                    weapon.Health -= projectile.damageAmount;

                    // Uncomment if you want to debug in the editor
                    //#if UNITY_EDITOR
                    //Debug.Log("Projectile: " + projectile.name + " hit " + gameObject.name +  " with damage amount of " + projectile.damageAmount);
                    //#endif
                }
                // Must have been hit by a (laser) beam
                else
                {
                    weapon.Health -= callbackOnObjectHitParameters.damageAmount;
                }

                if (weapon.Health <= 0f)
                {
                    if (!IsDestroyed && callbackOnDestroy != null) { callbackOnDestroy.Invoke(this); }
                    IsDestroyed = true;

                    DeactivateBeams(sscManager);

                    if (!isDestructionEffectsObjectInstantiated && effectsObjectPrefabID >= 0 && destructionEffectsObject != null && sscManager != null)
                    {
                        // Instantiate the turret destruction effects prefab
                        InstantiateEffectsObjectParameters ieParms = new InstantiateEffectsObjectParameters
                        {
                            effectsObjectPrefabID = effectsObjectPrefabID,
                            position = transform.position,
                            rotation = transform.rotation
                        };

                        sscManager.InstantiateEffectsObject(ref ieParms);
                        isDestructionEffectsObjectInstantiated = true;
                    }

                    // If this turret has a set Location, remove it
                    if (locationData != null)
                    {
                        if (isVisibleToRadar) { sscManager.DisableRadar(locationData); }
                        sscManager.DeleteLocation(locationData);
                        radarItemIndex = -1;
                        isVisibleToRadar = false;
                    }

                    // Was the turret added to radar as RadarItemType.GameObject?
                    if (isVisibleToRadar && radarItemIndex >= 0 && sscRadar != null)
                    {
                        sscRadar.DisableRadar(radarItemIndex);
                        radarItemIndex = -1;
                        isVisibleToRadar = false;
                    }

                    if (isDestroyOnNoHealth)
                    {
                        // Is there a destruct prefab?
                        if (!isDestructObjectActivated && destructObjectPrefabID >= 0 && destructObject != null && sscManager != null)
                        {
                            // Turn off all colliders. As we are going to destroy the gameobject,
                            // we can simply deactivate it.
                            gameObject.SetActive(false);

                            // Instantiate the turret destruct prefab
                            InstantiateDestructParameters dstParms = new InstantiateDestructParameters
                            {
                                destructPrefabID = destructObjectPrefabID,
                                position = transform.position,
                                //position = transform.position + (Vector3.up * 20f),
                                rotation = transform.rotation,
                                explosionPowerFactor = 1f,
                                explosionRadiusFactor = 1f
                            };

                            sscManager.InstantiateDestruct(ref dstParms);
                            isDestructObjectActivated = true;
                        }

                        DestroyFX();

                        Destroy(gameObject);
                    }
                }
            }
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Deactivate any beams that the weapon is currently firing
        /// </summary>
        public void DeactivateBeams (SSCManager sscManager)
        {
            if (sscManager != null && weapon != null && weaponTypeInt == Weapon.TurretBeamInt)
            {
                int numBeams = weapon.beamItemKeyList == null ? 0 : weapon.beamItemKeyList.Count;

                for (int bItemIdx = 0; bItemIdx < numBeams; bItemIdx++)
                {
                    if (weapon.beamItemKeyList[bItemIdx].beamSequenceNumber > 0)
                    {
                        sscManager.DeactivateBeam(weapon.beamItemKeyList[bItemIdx]);
                    }

                    weapon.beamItemKeyList[bItemIdx] = new SSCBeamItemKey(-1, -1, 0);
                }
            }
        }

        /// <summary>
        /// Reinitialises variables required for surface turret beam weapon and effects used by those beams.
        /// Call this after modifying any beams or beam effect data for this surface turret.
        /// </summary>
        public void ReinitialiseTurretBeams()
        {
            if (sscManager != null)
            {
                // Initialise beams, and effects objects used by those beams
                sscManager.UpdateBeamsAndEffects(this);
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("SurfaceTurretModule.ReinitialiseTurretBeams Warning: could not find SSCManager to update (weapon) beams");
            }
            #endif
        }

        /// <summary>
        /// Reinitialises variables required for projectiles and effects of the surface turret.
        /// Call after modifying any projectile or effect data for this surface turret.
        /// </summary>
        public void ReinitialiseTurretProjectilesAndEffects ()
        {
            if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(sceneHandle); }
            if (sscManager != null)
            {
                // Initialise projectiles and effects objects
                sscManager.UpdateProjectilesAndEffects(this);
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("SurfaceTurretModule.ReinitialiseTurretProjectilesAndEffects Warning: could not find SSCManager to update projectiles and effects.");
            }
            #endif
        }

        /// <summary>
        /// Reinitialises variables required for destruct objects of the surface turret.
        /// Call after modifying any destruct data for this surface turret.
        /// </summary>
        public void ReinitialiseTurretDestructObjects ()
        {
            if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(sceneHandle); }
            if (sscManager != null)
            {
                // Initialise destruct objects
                sscManager.UpdateDestructObjects(this);
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("SurfaceTurretModule.ReinitialiseTurretDestructObjects Warning: could not find SSCManager to update destruct objects.");
            }
            #endif
        }

        /// <summary>
        /// Create a new Location using the SSCManager and add it to Radar
        /// if required.
        /// </summary>
        /// <param name="removeExisting"></param>
        public void CreateLocation(bool removeExisting)
        {
            if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(sceneHandle); }
            if (sscManager != null)
            {
                if (removeExisting && locationData != null)
                {
                    sscManager.DeleteLocation(locationData);
                }

                locationData = sscManager.AppendLocation(this.gameObject);

                if (locationData != null)
                {
                    locationData.factionId = factionId;
                    locationData.radarBlipSize = radarBlipSize;
                    if (isVisibleToRadar) { sscManager.EnableRadar(locationData); }

                    // If isn't visible to radar (for whatever reason) it will be -1.
                    radarItemIndex = locationData.radarItemIndex;
                }
            }
        }

        /// <summary>
        /// Clears all targeting information for the weapon. This should be called if you do not know if the target
        /// is a ship or a gameobject.
        /// </summary>
        public void ClearWeaponTarget()
        {
            if (isInitialised) { weapon.ClearTarget(); }
        }

        /// <summary>
        /// The turret will track this gameobject.
        /// </summary>
        /// <param name="target"></param>
        public void SetWeaponTarget(GameObject target)
        {
            if (isInitialised) { weapon.SetTarget(target); }
        }

        /// <summary>
        /// The turret will track this ship
        /// </summary>
        /// <param name="targetShipControlModule"></param>
        public void SetWeaponTargetShip(ShipControlModule targetShipControlModule)
        {
            if (isInitialised) { weapon.SetTargetShip(targetShipControlModule); }
        }

        /// <summary>
        /// The turret will track this ship's localised damage region
        /// </summary>
        /// <param name="targetShipControlModule"></param>
        /// <param name="damageRegion"></param>
        public void SetWeaponTargetShipDamageRegion (ShipControlModule targetShipControlModule, DamageRegion damageRegion)
        {
            if (isInitialised) { weapon.SetTargetShipDamageRegion(targetShipControlModule, damageRegion); }
        }

        /// <summary>
        /// Fire all cannons on the weapon if they are loaded
        /// and ready. This is a single shot action. For continuous
        /// firing, call SetAutoFire().
        /// </summary>
        public void FireIfReady()
        {
            if (isInitialised && !isAutoFire)
            {
                isManualFireNow = true;
            }
        }

        /// <summary>
        /// Sets the weapon to automatically fire if a target is acquired
        /// and the weapon is ready.
        /// </summary>
        public void SetAutoFire()
        {
            isManualFireNow = false;

            if (weapon != null)
            {
                weapon.firingButton = Weapon.FiringButton.AutoFire;
                isAutoFire = true;
            }
        }

        /// <summary>
        /// For manually firing the weapon. After this is set,
        /// call FireIfReady() to fire the weapon.
        /// </summary>
        public void SetManualFire()
        {
            isManualFireNow = false;

            if (weapon != null)
            {
                weapon.firingButton = Weapon.FiringButton.None;
                isAutoFire = false;
            }
        }

        /// <summary>
        /// Returns whether a weapon has line of sight to a target.
        /// If directToTarget is set to true, will raycast directly from the weapon to the target.
        /// If directToTarget is set to false, will raycast in the direction the weapon is facing.
        /// This method will return true if the raycast hits:
        /// a) The target,
        /// b) An enemy ship (distinguished by faction ID) - even if it is not the target and anyEnemy is true,
        /// c) An object that isn't the target (if obstaclesBlockLineOfSight is set to false),
        /// d) Nothing.
        /// This method will return false if the raycast hits:
        /// a) A friendly ship (distinguished by faction ID),
        /// b) An object that isn't the target (if obstaclesBlockLineOfSight is set to true).
        /// c) An enemy ship that is not the target when anyEnemy is false 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="directToTarget"></param>
        /// <param name="obstaclesBlockLineOfSight"></param>
        /// <param name="anyEnemy"></param>
        /// <returns></returns>
        public bool WeaponHasLineOfSight(GameObject target, bool directToTarget = false, bool obstaclesBlockLineOfSight = true, bool anyEnemy = true)
        {
            if (target == null || !isInitialised) { return false; }

            // Update the line-of-sight property
            weapon.UpdateLineOfSight(target, trfmPos, trfmRight, trfmUp, trfmFwd, trfmInvRot, factionId,
                true, directToTarget, obstaclesBlockLineOfSight, anyEnemy);

            // Return the updated property
            return weapon.HasLineOfSight;
        }


        #endregion
    }
}
