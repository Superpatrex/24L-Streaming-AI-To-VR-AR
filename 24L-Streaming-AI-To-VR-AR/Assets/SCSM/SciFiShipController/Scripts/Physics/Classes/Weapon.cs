using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class containing data for a weapon.
    /// </summary>
    [System.Serializable]
    public class Weapon
    {
        #region Enumerations

        public enum FiringButton
        {
            None = 0,
            Primary = 1,
            Secondary = 2,
            AutoFire = 3
        }

        public enum WeaponType
        {
            FixedProjectile = 0,
            FixedBeam = 1,
            TurretProjectile = 10,
            TurretBeam = 11
        }

        #endregion Enumerations

        #region Public Static Variables
        public static readonly int FixedProjectileInt = (int)WeaponType.FixedProjectile;
        public static readonly int FixedBeamInt = (int)WeaponType.FixedBeam;
        public static readonly int TurretProjectileInt = (int)WeaponType.TurretProjectile;
        public static readonly int TurretBeamInt = (int)WeaponType.TurretBeam;
        #endregion

        #region Public variables

        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// Name of the weapon
        /// </summary>
        public string name;

        /// <summary>
        /// The type of weapon. e.g. FixedProjectile, FixedBeam, TurretProjectile etc.
        /// NOTE: We do not support changing this at runtime once the weapon has been initialised.
        /// </summary>
        public WeaponType weaponType;

        /// <summary>
        /// Position of the centre of the weapon in local space relative to the pivot point of the ship or surface turret.
        /// This is the position where projectiles will be fired from.
        /// </summary>
        public Vector3 relativePosition;

        /// <summary>
        /// The local space direction in which the weapon fires. If you modify this, call Initialise().
        /// </summary>
        public Vector3 fireDirection;

        public bool isMultipleFirePositions;

        public List<Vector3> firePositionList;

        /// <summary>
        /// The prefab for the projectile fired by this weapon. 
        /// If you modify this, call shipControlModule.ReinitialiseShipProjectilesAndEffects() or
        /// surfaceTurretModule.ReinitialiseTurretProjectilesAndEffects() depending on which component
        /// this weapon is a member of.
        /// </summary>
        public ProjectileModule projectilePrefab;
        /// <summary>
        /// [INTERNAL USE ONLY]
        /// The ID number for this weapon's projectile prefab (as assigned by the SSCManager in the scene).
        /// </summary>
        public int projectilePrefabID;

        /// <summary>
        /// The minimum time (in seconds) between consecutive firings of the weapon.
        /// </summary>
        [Range(0.05f, 10f)] public float reloadTime;
        /// <summary>
        /// The time (in seconds) until this weapon can fire again. Used for both
        /// fixed and beam weapons.
        /// </summary>
        public float reloadTimer;

        /// <summary>
        /// The prefab for the beam to be fired by this weapon.
        /// If you modify this, call shipControlModule.ReinitialiseShipBeams()
        /// </summary>
        public BeamModule beamPrefab;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// The ID number for this weapon's beam prefab (as assigned by the SSCManager in the scene).
        /// </summary>
        public int beamPrefabID;

        /// <summary>
        /// The index of the damage region this weapon is associated with. When the damage model of the ship is set to simple, this 
        /// is irrelevant. A negative value means it is associated with no damage region (so the weapon's performance will not be 
        /// affected by damage). When the damage model of the ship is set to progressive, a value of zero means it is 
        /// associated with the main damage region. When the damage model of the ship is set to localised, a zero or positive value
        /// indicates which damage region it is associated with (using a zero-based indexing system).
        /// </summary>
        public int damageRegionIndex;
        /// <summary>
        /// The starting health value of this weapon.
        /// </summary>
        public float startingHealth;

        /// <summary>
        /// Whether the weapon is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;
        
        /// <summary>
        /// Whether the weapon is shown as selected in the scene view of the editor.
        /// </summary>
        public bool selectedInSceneView;

        /// <summary>
        /// Whether the gizmos for this weapon are shown in the scene view of the editor.
        /// </summary>
        public bool showGizmosInSceneView;

        /// <summary>
        /// The trigger for the weapon to fire e.g. None, Primary, or Secondary.
        /// Linked to the PlayerInputModule. Or AutoFire for AutoTargetingModule.
        /// </summary>
        public FiringButton firingButton;

        /// <summary>
        /// The quantity of projectiles available for use with this weapon.
        /// Setting the value to -1, will give an unlimted amount of ammunition.
        /// For beam weapons, instead, use chargeAmount.
        /// </summary>
        public int ammunition;

        /// <summary>
        /// The amount of charge or power the beam weapon has.
        /// For projectile weapons, instead, use ammunition.
        /// </summary>
        [Range(0f,1f)] public float chargeAmount;

        /// <summary>
        /// The time (in seconds) it takes the fully discharged beam weapon to reach maximum charge
        /// </summary>
        [Range(0f, 30f)] public float rechargeTime;

        /// <summary>
        /// For turrets, the transform of the local y-axis pivot point.
        /// The turret rotates around this point on the local x-z plane.
        /// </summary>
        public Transform turretPivotY;

        /// <summary>
        /// For turrets, the tranform of the local X-axis pivot point
        /// This is the axis on which the barrel(s) move up and down
        /// </summary>
        public Transform turretPivotX;

        /// <summary>
        /// The minimum angle on the local x-axis the turret can rotate to
        /// </summary>
        [Range(-359.99f, 359.99f)] public float turretMinX;
        /// <summary>
        /// The maximum angle on the local x-axis the turret can rotate to
        /// </summary>
        [Range(-359.99f, 359.99f)] public float turretMaxX;
        /// <summary>
        /// The minimum angle on the local y-axis the turret can rotate to
        /// </summary>
        [Range(-359.99f, 359.99f)] public float turretMinY;
        /// <summary>
        /// The maximum angle on the local y-axis the turret can rotate to
        /// </summary>
        [Range(-359.99f, 359.99f)] public float turretMaxY;

        /// <summary>
        /// The speed at which the turret can rotate or raise the barrel(s) up and down
        /// </summary>
        [Range(0f, 720f)] public float turretMoveSpeed;

        /// <summary>
        /// When greater than 0, the number of seconds a turret will wait, after losing a target, to begin returning to the original orientation.
        /// </summary>
        [Range(0f, 60f)] public float turretReturnToParkInterval;

        /// <summary>
        /// Whether the weapon checks line of sight before firing (in order to prevent friendly fire) each frame. Only
        /// relevant if the weaponType is TurretProjectile and the firingButton is AutoFire. Since this uses raycasts it
        /// can lead to reduced performance.
        /// </summary>
        public bool checkLineOfSight;

        /// <summary>
        /// When the Auto Targeting Module is attached, use this to indicate targets should be assigned to the weapon.
        /// </summary>
        public bool isAutoTargetingEnabled;

        /// <summary>
        /// Currently only applies to turrets. Default to 0 which is the weapon
        /// will attempt to hit the target. Range from 0.0 to 1.0. This will
        /// have little or no effect on guided projectiles.
        /// </summary>
        [Range(0f, 1f)] public float inaccuracy;

        /// <summary>
        /// The maximum range (in metres) of the weapon. Currently only applies to beam weapons
        /// </summary>
        public float maxRange;

        /// <summary>
        /// The heat of the weapon - range 0.0 (starting temp) to 100.0 (max temp).
        /// At runtime call either weapon.SetHeatLevel(..) or shipInstance.SetHeatLevel(..)
        /// </summary>
        [Range(0f, 100f)] public float heatLevel;

        /// <summary>
        /// The rate heat is added per second for beam weapons. For projectile weapons,
        /// it is inversely proportional to the firing interval (reload time).
        /// If rate is 0, heat level never changes.
        /// </summary>
        [Range(0f, 20f)] public float heatUpRate;

        /// <summary>
        /// The rate heat is removed per second. This is the rate the weapon cools when not in use.
        /// </summary>
        [Range(0f, 20f)] public float heatDownRate = 2f;

        /// <summary>
        /// The heat level that the weapon will begin to overheat and start being less efficient.
        /// </summary>
        [Range(50f, 100f)] public float overHeatThreshold = 80f;

        /// <summary>
        /// When the weapon reaches max heat level of 100, will the weapon be inoperable
        /// until it is repaired?
        /// </summary>
        public bool isBurnoutOnMaxHeat;

        #endregion

        #region Public Properties

        /// <summary>
        /// The current performance level of this weapon (determined by the Health and Heat). The performance level affects how
        /// efficient it is. At a performance level of one it operates normally. At a performance level of 0 it can do nothing.
        /// </summary>
        public float CurrentPerformance { get { return currentPerformance; } }

        /// <summary>
        /// The estimated range (in metres) of the weapon
        /// </summary>
        public float estimatedRange { get; internal set; }

        /// <summary>
        /// The normalised local space direction in which the weapon fires.
        /// </summary>
        public Vector3 fireDirectionNormalised { get; private set; }

        /// <summary>
        /// Returns whether the weapon has line-of-sight to the target. NOTE: This does not calculate line-of-sight, it
        /// merely returns the last calculated value. To calculate line-of-sight for a weapon, call WeaponHasLineOfSight() on the
        /// ship or surface turret module that it is attached to.
        /// </summary>
        public bool HasLineOfSight { get; private set; }

        /// <summary>
        /// The current health value of this weapon. 
        /// </summary>
        public float Health
        {
            get { return health; }
            set
            {
                // Update the health value
                health = value;

                // Update the current performance value
                UpdateWeaponPerformance();
            }
        }

        /// <summary>
        /// [READONLY] When initialised, is the weapon in the parked or original orientation?
        /// </summary>
        public bool IsParked { get { return isParked; } }

        /// <summary>
        /// [READ ONLY] Is the weapon one that can fire projectiles?
        /// </summary>
        public bool IsProjectileWeapon { get { return weaponType == WeaponType.FixedProjectile || weaponType == WeaponType.TurretProjectile; } }

        /// <summary>
        /// [READ ONLY] Is the weapon one that can fire beams, rays or lasers?
        /// </summary>
        public bool IsBeamWeapon { get { return weaponType == WeaponType.FixedBeam || weaponType == WeaponType.TurretBeam; } }

        /// <summary>
        /// [READ ONLY] Is this weapon a turret?
        /// </summary>
        public bool IsTurretWeapon { get { return weaponType == WeaponType.TurretBeam || weaponType == WeaponType.TurretProjectile; } }

        /// <summary>
        /// Cached from the projectilePrefab for faster lookup when a projectile is instantiated 
        /// </summary>
        public bool isProjectileKGuideToTarget { get; internal set; }

        #endregion

        #region Public non-serialized variables

        /// <summary>
        /// Is the weapon locked onto the target. If it is fired, is it
        /// likely to hit the target? i.e. is it facing the correct direction?
        /// </summary>
        [System.NonSerialized] public bool isLockedOnTarget;

        /// <summary>
        /// After the weapon has been initialised, this can be used to avoid enumeration lookups
        /// </summary>
        [System.NonSerialized] public int weaponTypeInt;

        #endregion

        #region Private variables

        private Ray raycastRay;
        private RaycastHit raycastHitInfo;
        private Vector3 weaponRelativeFireDirectionLOS = Vector3.zero;
        private float returnToParkTimer;
        private bool isParked;

        [Range(0f, 1f)] internal float currentPerformance;

        #endregion

        #region Internal Variables

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// See Health property
        /// </summary>
        internal float health;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// For turrets, stores the initial inverse rotation to be subtracted
        /// from the current rotation on the Y-axis. Also subtracts parent
        /// object rotation.
        /// </summary>
        [System.NonSerialized] internal Quaternion intPivotYInvRot;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// For turrets, stores the initial local rotation for the optional return to parked rotation
        /// </summary>
        [System.NonSerialized] internal Quaternion intPivotYLocalRot;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// For turrets, stores the initial local rotation for the optional return to parked rotation
        /// </summary>
        [System.NonSerialized] internal Quaternion intPivotXLocalRot;

        /// <summary>
        /// [INTERNAL USE ONLY] Call weapon.SetTarget(..), ship.SetWeaponTarget(..),
        /// surfaceTurretModule.SetWeaponTarget(..), weapon.GetTarget()
        /// Target of the weapon. Currently only applicable to Turret WeaponTypes and projectile
        /// weapons with guided projectiles.
        /// Determines where the turret or guided projectile should aim.
        /// </summary>
        [System.NonSerialized] internal GameObject target;

        /// <summary>
        /// [INTERNAL USE ONLY] Call weapon.SetTargetShip(..) or ship.SetWeaponTargetShip(...),
        /// surfaceTurretModule.SetWeaponTargetShip(..),or weapon.GetTargetShip().
        /// Target of the weapon. Currently only applicable to Turret WeaponTypes and projectile
        /// weapons with guided projectiles.
        /// Determines where the turret or guided projectile should aim.
        /// </summary>
        [System.NonSerialized] internal ShipControlModule targetShip;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Target's guidHash of the weapon. Currenly used with targetShip to define a DamageRegion
        /// </summary>
        [System.NonSerialized] internal int targetguidHash;

        /// <summary>
        /// [INTERNAL USE ONLY] Call weapon.SetTargetShipDamageRegion(..) or ship.SetWeaponTargetShipDamageRegion(..).
        /// The damage region being targeted.
        /// </summary>
        [System.NonSerialized] internal DamageRegion targetShipDamageRegion;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Currently only updated for Turret weapons
        /// </summary>
        [System.NonSerialized] internal Vector3 targetLastPosition;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// </summary>
        //[System.NonSerialized] internal Quaternion turretTargetRotation;

        /// <summary>
        /// Timer recording how long the current target has been invalid (lost line-of-sight etc) for.
        /// If the current target is valid, will be set to zero. Used by the Auto Targeting Module.
        /// </summary>
        [System.NonSerialized] internal float invalidTargetTimer = 0f;

        /// <summary>
        /// Timer recording how long it has been since we were assigned a new target. Used by the Auto Targeting Module.
        /// </summary>
        [System.NonSerialized] internal float assignedNewTargetTimer = 0f;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// For beam weapons, this is used to identify the active beam for each firing point
        /// </summary>
        [System.NonSerialized] internal List<SSCBeamItemKey> beamItemKeyList = null;

        #endregion

        #region Class constructors

        public Weapon()
        {
            SetClassDefaults();
        }

        // Copy constructor
        public Weapon(Weapon weapon)
        {
            if (weapon == null) { SetClassDefaults(); }
            else
            {
                this.name = weapon.name;
                this.weaponType = weapon.weaponType;
                this.relativePosition = weapon.relativePosition;
                this.fireDirection = weapon.fireDirection;
                this.isMultipleFirePositions = weapon.isMultipleFirePositions;
                if (weapon.firePositionList == null || weapon.firePositionList.Count == 0)
                {
                    this.firePositionList = new List<Vector3>(2);
                    this.firePositionList.Add(Vector3.zero);
                }
                else
                {
                    if (this.firePositionList == null) { this.firePositionList = new List<Vector3>(weapon.firePositionList.Count); }
                    this.firePositionList.AddRange(weapon.firePositionList);
                }
                this.projectilePrefab = weapon.projectilePrefab;
                this.projectilePrefabID = weapon.projectilePrefabID;
                this.beamPrefab = weapon.beamPrefab;
                this.beamPrefabID = weapon.beamPrefabID;
                this.reloadTime = weapon.reloadTime;
                this.damageRegionIndex = weapon.damageRegionIndex;
                this.startingHealth = weapon.startingHealth;
                this.Health = weapon.Health;
                this.showInEditor = weapon.showInEditor;
                this.selectedInSceneView = weapon.selectedInSceneView;
                this.showGizmosInSceneView = weapon.showGizmosInSceneView;
                this.firingButton = weapon.firingButton;
                this.ammunition = weapon.ammunition;
                this.rechargeTime = weapon.rechargeTime;
                this.chargeAmount = weapon.chargeAmount;
                this.turretPivotY = weapon.turretPivotY;
                this.turretPivotX = weapon.turretPivotX;
                this.turretMinX = weapon.turretMinX;
                this.turretMaxX = weapon.turretMaxX;
                this.turretMinY = weapon.turretMinY;
                this.turretMaxY = weapon.turretMaxY;
                this.turretMoveSpeed = weapon.turretMoveSpeed;
                this.turretReturnToParkInterval = weapon.turretReturnToParkInterval;
                this.isLockedOnTarget = weapon.isLockedOnTarget;
                this.checkLineOfSight = weapon.checkLineOfSight;
                this.isAutoTargetingEnabled = weapon.isAutoTargetingEnabled;
                this.inaccuracy = weapon.inaccuracy;
                this.maxRange = weapon.maxRange;
                this.heatLevel = weapon.heatLevel;
                this.heatUpRate = weapon.heatUpRate;
                this.heatDownRate = weapon.heatDownRate;
                this.overHeatThreshold = weapon.overHeatThreshold;
                this.isBurnoutOnMaxHeat = weapon.isBurnoutOnMaxHeat;
                this.Initialise(Quaternion.Inverse(Quaternion.identity));
            }
        }

        #endregion

        #region Internal Member Methods

        /// <summary>
        /// Check if we need to change the heat level on this weapon. If
        /// heat up rate is 0, then do nothing.
        /// When heatInput = 0, the weapon starts to cool down.
        /// heatInput is the amount per second.
        /// For projectile weapons it is inversely proportional to the
        /// firing interval (reloadTime).
        /// </summary>
        /// <param name="dTime"></param>
        /// <param name="heatInput"></param>
        internal void ManageHeat (float dTime, float heatInput)
        {
            if (heatUpRate > 0f)
            {
                // Heat or cool weapon independently to the health level.
                if (heatInput > 0f)
                {
                    // Heating up
                    if (heatLevel < 100f)
                    {
                        SetHeatLevel(heatLevel + (heatInput * heatUpRate * dTime));
                    }
                }
                // Only cool down if not burnt out
                else if (heatDownRate > 0f && (!isBurnoutOnMaxHeat || heatLevel < 100f))
                {
                    // Cooling down
                    SetHeatLevel(heatLevel - (heatDownRate * dTime));
                }
            }
        }

        /// <summary>
        /// Rotate the weapon's turret to face the target.
        /// Call SetWeaponTarget(..), SetWeaponTarget to set a weapon's target.
        /// For performance reasons, it assumes the weaponType is a turret.
        /// Updates isLockedOnTarget when turret is facing the target +/- 5 degrees.
        /// Auto-park the weapon after losing the target if the park interval > 0 seconds.
        /// </summary>
        /// <param name="worldVelocity"></param>
        internal void MoveTurret(Vector3 worldVelocity, bool isSurfaceTurret)
        {
            if (turretPivotY != null)
            {
                if (target != null)
                {
                    // Adjust the target position for the relative speed of the ship or surfaceTurret aiming at the target
                    Vector3 targetRelativeVelocity = -worldVelocity; // Target velo - our velo

                    if (targetShip != null && targetShip.IsInitialised)
                    {
                        bool isTargetShipDamageRegion = targetguidHash != 0 && targetShipDamageRegion != null;

                        // If the target ship has been destroyed, stop targeting that ship.
                        // Somehow the shipInstance can become null even when targetShip is not null...
                        if (targetShip.shipInstance == null || targetShip.shipInstance.Destroyed())
                        {
                            ClearTarget();
                            return;
                        }
                        // If this is a damage region target, and it has been destroyed, then stop targeting it.
                        else if (isTargetShipDamageRegion && targetShipDamageRegion.isDestroyed)
                        {
                            ClearTarget();
                            return;
                        }
                        else
                        {
                            targetLastPosition = isTargetShipDamageRegion ? targetShip.shipInstance.GetDamageRegionWSPosition(targetShipDamageRegion) : targetShip.shipInstance.TransformPosition;
                            //targetLastPosition = targetShip.shipInstance.TransformPosition;
                            // If the target velocity is known, add it. Target velo - our velo
                            targetRelativeVelocity += targetShip.shipInstance.WorldVelocity;
                        }
                    }
                    else { targetLastPosition = target.transform.position; }

                    float targetRelativeSpeed = targetRelativeVelocity.magnitude;
                    Vector3 turretToTargetVector = targetLastPosition - turretPivotX.position;
                    float distToTarget = turretToTargetVector.magnitude;
                    Vector3 adjustedTargetPosition = targetLastPosition;

                    // Only adjust the target position if it has speed relative to the turret
                    // For guided projectiles, we don't want to aim in front of the target, else the projectiles
                    // suddenly veer off towards the target (rather than the predicted target position) due to the
                    // way Augmented Proportional Navigation works.
                    // Turret Beam weapons should always aim directly at the target (assuming inaccuracy = 0)
                    if (targetRelativeSpeed > 0.1f && !isProjectileKGuideToTarget && weaponTypeInt != TurretBeamInt)
                    {
                        // We need to calculate which position we should aim at so that the turret fire will intercept the target
                        float projectileSpeed = projectilePrefab.startSpeed;
                        float cosineTheta = Vector3.Dot(targetRelativeVelocity, -turretToTargetVector) / (targetRelativeSpeed * distToTarget);
                        float interceptionTime = 0f;
                        // Is this is a quadratic equation?
                        if (targetRelativeSpeed != projectileSpeed)
                        {
                            // This is a quadratic equation, so first check if there is a solution/s to the problem
                            float quadraticDiscriminant = ((targetRelativeSpeed * targetRelativeSpeed) * ((cosineTheta * cosineTheta) - 1)) +
                                (projectileSpeed * projectileSpeed);
                            if (quadraticDiscriminant >= 0f)
                            {
                                // Always choose the positive time solution
                                interceptionTime = (-targetRelativeSpeed * distToTarget * cosineTheta) +
                                    (distToTarget * Mathf.Sqrt(quadraticDiscriminant));
                                interceptionTime /= (projectileSpeed * projectileSpeed) - (targetRelativeSpeed * targetRelativeSpeed);
                            }
                        }
                        else if (targetRelativeSpeed * cosineTheta != 0f)
                        {
                            // Linear solution - this is an edge case but is just here to avoid any divide by zero issues
                            interceptionTime = distToTarget / (2f * targetRelativeSpeed * cosineTheta);
                        }

                        // Calculate the interception position from the interception time
                        adjustedTargetPosition += targetRelativeVelocity * interceptionTime;
                    }

                    if (inaccuracy > 0f)
                    {
                        // Adjust the target position based on inaccuracy
                        // Inaccuracy is proportional to the distance to the target
                        adjustedTargetPosition += Random.insideUnitSphere * (distToTarget * 0.1f * inaccuracy);
                    }

                    // Find the position of the target in turret local space
                    Vector3 targetTurretSpacePos = Vector3.forward;
                    if (!isSurfaceTurret)
                    {
                        // Ship turret: Aim from the turret pivot Y parent object
                        targetTurretSpacePos = turretPivotY.parent.InverseTransformPoint(adjustedTargetPosition);
                    }
                    else
                    {
                        // Surface turret: Aim from the turret relative position
                        Quaternion turretParentInverseRotation = Quaternion.Inverse(turretPivotY.parent.rotation);
                        Vector3 turretRelativePosition = ((turretParentInverseRotation * turretPivotX.rotation) * relativePosition) + turretPivotY.parent.transform.position;
                        // Transform into turret space using rotation of pivot Y parent object and position of turret relative position
                        targetTurretSpacePos = turretParentInverseRotation * (adjustedTargetPosition - turretRelativePosition);
                    }

                    bool isFacingTarget = true;

                    // Turn the base of the turret
                    // Calculate the angle to rotate the turret base by
                    float azimuthAngle = Mathf.Atan2(targetTurretSpacePos.x, targetTurretSpacePos.z) * Mathf.Rad2Deg;
                    // Clamp the angle by rotation limits
                    if (azimuthAngle < turretMinY) { azimuthAngle = turretMinY; isFacingTarget = false; }
                    else if (azimuthAngle > turretMaxY) { azimuthAngle = turretMaxY; isFacingTarget = false; }
                    // Rotate the turret base towards the calculated angle at a given speed
                    turretPivotY.localRotation = Quaternion.RotateTowards(turretPivotY.localRotation,
                        Quaternion.Euler(0f, azimuthAngle, 0f), turretMoveSpeed * Time.deltaTime);

                    if (isFacingTarget)
                    {
                        // Is the turret facing roughly in the correct direction? +/- 5 degrees
                        float fixedEulerAngleY = turretPivotY.localRotation.eulerAngles.y > 180f ? turretPivotY.localRotation.eulerAngles.y - 360f : turretPivotY.localRotation.eulerAngles.y;
                        isFacingTarget = fixedEulerAngleY > azimuthAngle - 5f && fixedEulerAngleY < azimuthAngle + 5f;
                    }

                    // Move the barrel(s) up and down
                    if (turretPivotX != null)
                    {
                        // Calculate the angle of inclination for the turret guns
                        float altitudeAngle = Mathf.Atan(targetTurretSpacePos.y /
                            Mathf.Sqrt((targetTurretSpacePos.x * targetTurretSpacePos.x) + (targetTurretSpacePos.z * targetTurretSpacePos.z)))
                            * Mathf.Rad2Deg;
                        // Clamp the angle by rotation limits
                        if (altitudeAngle < turretMinX) { altitudeAngle = turretMinX; isFacingTarget = false; }
                        else if (altitudeAngle > turretMaxX) { altitudeAngle = turretMaxX; isFacingTarget = false; }
                        // Rotate the turret guns towards the calculated angle at a given speed
                        turretPivotX.localRotation = Quaternion.RotateTowards(turretPivotX.localRotation, Quaternion.Euler(-altitudeAngle, 0f, 0f), turretMoveSpeed * Time.deltaTime);

                        //Debug.DrawRay(turretPivotX.position, turretPivotX.forward * 1000f, Color.black);

                        if (isFacingTarget)
                        {
                            // Is the turret facing roughly in the correct direction? +/- 5 degrees
                            float fixedEulerAngleX = turretPivotX.localRotation.eulerAngles.x > 180f ? turretPivotX.localRotation.eulerAngles.x - 360f : turretPivotX.localRotation.eulerAngles.x;
                            isFacingTarget = fixedEulerAngleX > -altitudeAngle - 5f && fixedEulerAngleX < -altitudeAngle + 5f;
                        }
                    }
                    else { isFacingTarget = false; }

                    // The turret has most likely moved, so it is probably not parked.
                    // This is required here in case some code directly updated target variable
                    isParked = false;

                    // Check range to target

                    // If the weapon is at its full rotation angle or inclination, assume it is not facing the target
                    isLockedOnTarget = isFacingTarget;
                }
                else
                {
                    // If there is no target, it can't be locked on to a target
                    isLockedOnTarget = false;

                    if (turretReturnToParkInterval > 0f && !isParked)
                    {
                        // If the turret should auto-park but hasn't started yet, increment the timer
                        if (returnToParkTimer < turretReturnToParkInterval) { returnToParkTimer += Time.deltaTime; }
                        else
                        {
                            // Move the barrel(s) up or down towards original rotation
                            turretPivotY.localRotation = Quaternion.RotateTowards(turretPivotY.localRotation, intPivotYLocalRot, turretMoveSpeed * Time.deltaTime);

                            // Rotate the base left or right towards the initial rotation
                            if (turretPivotX != null)
                            {
                                turretPivotX.localRotation = Quaternion.RotateTowards(turretPivotX.localRotation, intPivotYLocalRot, turretMoveSpeed * Time.deltaTime);
                            }

                            // Once parked we need to opt out of this
                            isParked = turretPivotY.localRotation == intPivotYLocalRot && (turretPivotX == null || turretPivotX.localRotation == intPivotXLocalRot);
                        }
                    }
                }
            }
            // If there is no pivot point, it can't be locked on to a target
            else { isLockedOnTarget = false; }
        }

        /// <summary>
        /// Returns whether a weapon has line of sight to a target.
        /// If directToTarget is set to true, will raycast directly from the weapon to the target.
        /// If directToTarget is set to false, will raycast in the direction the weapon is facing.
        /// This method will return true if the raycast hits:
        /// a) The target,
        /// b) An enemy ship (distingushed by faction ID) - even if it is not the target when anyEnemy is true,
        /// c) An object that isn't the target (if obstaclesBlockLineOfSight is set to false),
        /// d) Nothing.
        /// This method will return false if the raycast hits:
        /// a) A friendly ship (distinguished by faction ID),
        /// b) An object that isn't the target (if obstaclesBlockLineOfSight is set to true).
        /// c) An enemy ship that is not the target when anyEnemy is false
        /// The trfmPos, trfmRight, trfmUp, trfmFwd and trfmInvRot vectors/quaternions refer to the object the turret is 
        /// attached to (either a ship or a surface turret module). The same is true for the factionId integer.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="trfmPos"></param>
        /// <param name="trfmRight"></param>
        /// <param name="trfmUp"></param>
        /// <param name="trfmFwd"></param>
        /// <param name="trfmInvRot"></param>
        /// <param name="factionId"></param>
        /// <param name="isSurfaceTurret"></param>
        /// <param name="directToTarget"></param>
        /// <param name="obstaclesBlockLineOfSight"></param>
        /// <param name="anyEnemy"></param>
        /// <returns></returns>
        internal void UpdateLineOfSight(GameObject target, Vector3 trfmPos, Vector3 trfmRight, Vector3 trfmUp, 
            Vector3 trfmFwd, Quaternion trfmInvRot, int factionId, bool isSurfaceTurret, bool directToTarget = false, 
            bool obstaclesBlockLineOfSight = true, bool anyEnemy = true)
        {
            // v1.2.4 fixed MissingReferenceException
            if (target == null) { HasLineOfSight = false; return; }

            #region Calculate Raycast Origin

            // Get base weapon world position (not accounting for relative fire position)
            if (isSurfaceTurret)
            {
                raycastRay.origin = trfmPos;
            }
            else
            {
                raycastRay.origin = (trfmRight * relativePosition.x) +
                                    (trfmUp * relativePosition.y) +
                                    (trfmFwd * relativePosition.z) +
                                    trfmPos;
            }

            #endregion

            #region Calculate Raycast Direction

            Vector3 targetTrfmPos = target.transform.position;

            if (directToTarget)
            {
                // Check line of sight from the weapon directly to the target
                raycastRay.direction = targetTrfmPos - raycastRay.origin;
            }
            else
            {
                // Check line of sight in the direction the weapon is aiming

                // Get relative fire direction
                weaponRelativeFireDirectionLOS = fireDirectionNormalised;
                // If this is a turret, adjust relative fire direction based on turret rotation
                if (weaponTypeInt == TurretProjectileInt || weaponTypeInt == TurretBeamInt)
                {
                    if (isSurfaceTurret)
                    {
                        weaponRelativeFireDirectionLOS = (trfmInvRot * turretPivotX.rotation) * weaponRelativeFireDirectionLOS;
                    }
                    else
                    {
                        weaponRelativeFireDirectionLOS = (trfmInvRot * turretPivotX.rotation) * intPivotYInvRot * weaponRelativeFireDirectionLOS;
                    }
                }
                // Get weapon world fire direction
                raycastRay.direction = (trfmRight * weaponRelativeFireDirectionLOS.x) +
                                       (trfmUp * weaponRelativeFireDirectionLOS.y) +
                                       (trfmFwd * weaponRelativeFireDirectionLOS.z);
            }

            #endregion

            #region Check Line Of Sight

            bool hasLineOfSight = true;

            // Raycast in the direction we have chosen
            // NOTE: If there is a rigidbody on the object (other than the collider), raycastHitInfo.transform will be that
            // of the rigidbody, not the collider.
            //if (Physics.Raycast(raycastRay, out raycastHitInfo, projectilePrefab.startSpeed * projectilePrefab.despawnTime))
            // Use estimatedRange to cater for projectiles and beams.
            if (Physics.Raycast(raycastRay, out raycastHitInfo, estimatedRange))
            {
                float sqrDistToTarget = (targetTrfmPos - raycastRay.origin).sqrMagnitude;

                // If required, check if we hit (only) the target indicated. This is particularly important for
                // ship damage regions.
                if (!anyEnemy)
                {
                    // Determine if the collider is child of the target. Will also be true if the collider is directly on the target.
                    bool isColliderChildOfTarget = raycastHitInfo.collider.transform.IsChildOf(target.transform);

                    // This is almost always going to be true for ship damage regions because it has volume and the
                    // hitpoint will be closer than the middle of the damage region.
                    if (obstaclesBlockLineOfSight && sqrDistToTarget > raycastHitInfo.distance * raycastHitInfo.distance)
                    {
                        // Check whether it is the target or if it is some other object
                        hasLineOfSight = isColliderChildOfTarget;
                    }

                    // If we hit a friendly ship, set hasLineOfSight to false (maybe we incorrectly targeted a friendly ship)
                    if (hasLineOfSight && raycastHitInfo.rigidbody != null)
                    {
                        ShipControlModule hitShipControlModule = raycastHitInfo.rigidbody.GetComponent<ShipControlModule>();
                        if (hitShipControlModule != null && hitShipControlModule.IsInitialised && hitShipControlModule.shipInstance != null)
                        {
                            // If we hit a ship, check if it has the same faction ID as we do
                            // If it has the same faction ID as we do, it is a friend, so we don't have line of sight
                            // Otherwise it is an enemy, so we have line of sight
                            hasLineOfSight = hitShipControlModule.shipInstance.factionId != factionId;
                        }
                    }
                }
                else if (raycastHitInfo.rigidbody != null)
                {
                    // Performance has been verified - No GC and low overhead in U201746f1.
                    // What we hit had a rigidbody attached, so it could be a ship

                    ShipControlModule hitShipControlModule = raycastHitInfo.rigidbody.GetComponent<ShipControlModule>();
                    if (hitShipControlModule != null && hitShipControlModule.IsInitialised && hitShipControlModule.shipInstance != null)
                    {
                        // If we hit a ship, check if it has the same faction ID as we do
                        // If it has the same faction ID as we do, it is a friend, so we don't have line of sight
                        // Otherwise it is an enemy, so we have line of sight
                        hasLineOfSight = hitShipControlModule.shipInstance.factionId != factionId;
                    }
                    // Only check for obstacles blocking line of sight if what we hit was between us and the target
                    else if (obstaclesBlockLineOfSight && sqrDistToTarget > raycastHitInfo.distance * raycastHitInfo.distance)
                    {
                        // If we did not hit a ship, check if it is the target
                        hasLineOfSight = raycastHitInfo.transform.IsChildOf(target.transform);
                    }
                }
                // Only check for obstacles blocking line of sight if what we hit was between us and the target
                else if (obstaclesBlockLineOfSight && sqrDistToTarget > raycastHitInfo.distance * raycastHitInfo.distance)
                {
                    // What we hit did not have a rigidbody attached, so it is a static object
                    // We now need to check whether it is the target or if it is some other object
                    hasLineOfSight = raycastHitInfo.transform.IsChildOf(target.transform);
                }
            }

            #endregion

            // Update the HasLineOfSight property accordingly
            HasLineOfSight = hasLineOfSight;
        }

        #endregion

        #region Public Member Methods

        public void SetClassDefaults()
        {
            this.name = "Weapon";
            this.weaponType = WeaponType.FixedProjectile;
            this.relativePosition = Vector3.zero;
            this.fireDirection = Vector3.forward;
            this.isMultipleFirePositions = false;
            // The most common scenarios would be 1 or 2 fire positions / cannons on a single weapon
            this.firePositionList = new List<Vector3>(2);
            this.firePositionList.Add(Vector3.zero);

            this.projectilePrefab = null;
            this.projectilePrefabID = -1;
            this.beamPrefab = null;
            this.beamPrefabID = -1;
            this.reloadTime = 0.5f;
            this.damageRegionIndex = -1;
            this.startingHealth = 100f;
            this.Health = 100f;
            this.showInEditor = true;
            this.selectedInSceneView = false;
            this.showGizmosInSceneView = true;
            // None by default so that weapon doesn't automatically fire when a weapon is added at runtime
            this.firingButton = FiringButton.None;
            this.ammunition = -1; // Unlimited (this should always be the default so it works with adding Beams at runtime)
            this.rechargeTime = 0f; // Instantly recharge
            this.chargeAmount = 1f; // Fully charged
            this.turretPivotY = null;
            this.turretPivotX = null;
            this.turretMinX = -180f;
            this.turretMaxX = 180f;
            this.turretMinY = -180f;
            this.turretMaxY = 180f;
            this.turretMoveSpeed = 20f;
            this.turretReturnToParkInterval = 0f;
            this.isLockedOnTarget = false;
            this.checkLineOfSight = false;
            this.isAutoTargetingEnabled = false;
            this.inaccuracy = 0f;
            this.maxRange = 2000f;
            this.HasLineOfSight = false;
            this.invalidTargetTimer = 0f;
            this.assignedNewTargetTimer = 0f;
            this.heatLevel = 0f;
            this.heatUpRate = 0f;
            this.heatDownRate = 2f;
            this.overHeatThreshold = 80f;
            this.isBurnoutOnMaxHeat = false;
            this.Initialise(Quaternion.Inverse(Quaternion.identity));
        }

        /// <summary>
        /// Initialises data for the weapon. This does some precalculation to allow for performance improvements.
        /// Call after modifying fireDirection.
        /// The rootObject is typically the gameobject of the shipControlModule or the surfaceTurretModule.
        /// Initialised Beam weapons.
        /// </summary>
        /// <param name="rootObjectInvRotation"></param>
        public void Initialise(Quaternion rootObjectInvRotation)
        {
            weaponTypeInt = (int)weaponType;

            #if UNITY_EDITOR
            if (weaponTypeInt == TurretProjectileInt || weaponTypeInt == TurretBeamInt)
            {
                if (turretPivotX == null || turretPivotY == null) { Debug.Log("[ERROR] The " + name + (weaponTypeInt == TurretBeamInt ? " Beam" : " Projectile") + " turret is missing Turret Pivot setup."); }
            }
            #endif

            // Calculate normalised vectors
            if ((weaponTypeInt == TurretProjectileInt || weaponTypeInt == TurretBeamInt) && turretPivotY != null)
            {
                // The turret pivot y parent gameobject may be rotated
                intPivotYInvRot = Quaternion.Inverse(rootObjectInvRotation * turretPivotY.rotation);

                fireDirectionNormalised = fireDirection.normalized;

                // Remember intial rotations for self-parking option.
                intPivotYLocalRot = turretPivotY.localRotation;
                if (turretPivotX != null) { intPivotXLocalRot = turretPivotX.localRotation; }
                else { intPivotXLocalRot = Quaternion.identity; }
            }
            else
            {
                intPivotYInvRot = Quaternion.Inverse(Quaternion.identity);

                fireDirectionNormalised = fireDirection.normalized;

                intPivotXLocalRot = Quaternion.identity;
                intPivotYLocalRot = Quaternion.identity;
            }

            // Intialise beam weapon
            if (weaponTypeInt == FixedBeamInt || weaponTypeInt == TurretBeamInt)
            {
                // If instant recharge, make fully charged
                if (rechargeTime == 0f) { chargeAmount = 1f; }

                int numFirePositions = firePositionList == null ? 0 : firePositionList.Count;
                int numBeamItemKeys = beamItemKeyList == null ? 0 : beamItemKeyList.Count;

                // Note: This may need changing for Ship.SetWeaponFireDirection(int, Vector3)

                if (beamItemKeyList == null) { beamItemKeyList = new List<SSCBeamItemKey>(numFirePositions); }
                else
                {
                    if (numBeamItemKeys != numFirePositions) { beamItemKeyList.Clear(); }
                }

                for (int fIdx = 0; fIdx < numFirePositions; fIdx++)
                {
                    if (fIdx >= numBeamItemKeys)
                    {
                        beamItemKeyList.Add(new SSCBeamItemKey(-1, -1, 0));
                        numBeamItemKeys++;
                    }
                    // Reset exiting beam keys. This might need changing for Ship.SetWeaponFireDirection(..)
                    else
                    {
                        SSCBeamItemKey beamItemKey = new SSCBeamItemKey(-1, -1, 0);
                        beamItemKeyList[fIdx] = beamItemKey;
                    }
                }
            }
            else { beamItemKeyList = null; }

            // Initialise raycast ray
            raycastRay = new Ray(Vector3.zero, Vector3.up);

            UpdateWeaponPerformance();

            // Reset
            isLockedOnTarget = false;
            HasLineOfSight = false;
            invalidTargetTimer = 0f;
            assignedNewTargetTimer = 0f;
            returnToParkTimer = 0f;
            isParked = true;
        }

        /// <summary>
        /// Reset the heat level to 0 and reset health
        /// </summary>
        public void Repair()
        {
            heatLevel = 0;

            // Ensure threshold is a sensible value
            if (overHeatThreshold < 50f) { overHeatThreshold = 80f; }

            // This will also update the current performance
            Health = startingHealth;
        }

        /// <summary>
        /// Set the new heat level on this weapon.
        /// Range 0.0 (min) to 100.0 (max).
        /// </summary>
        /// <param name="newHeatLevel"></param>
        public void SetHeatLevel (float newHeatLevel)
        {
            if (newHeatLevel < 0f) { newHeatLevel = 0f; }
            else if (newHeatLevel > 100f) { newHeatLevel = 100f; }

            // Only update the weapon performance if we need to
            // Update when:
            // a) heatLevel will be equal or above the overheat threshold
            // b) heatLevel will fallen below the overheat threshold
            // c) AND it has changed
            if (newHeatLevel != heatLevel && (newHeatLevel >= overHeatThreshold || (newHeatLevel < overHeatThreshold && heatLevel >= overHeatThreshold)))
            {
                heatLevel = newHeatLevel;
                UpdateWeaponPerformance();
            }
            else
            {
                heatLevel = newHeatLevel;
            }
        }

        /// <summary>
        /// Set the target GameObject for the weapon. Applies to Turret-type weapons and fixed projectile
        /// weapons with guided projectiles. If targetShip was previously set, this will be reset to null.
        /// </summary>
        /// <param name="targetGameObject"></param>
        public void SetTarget(GameObject targetGameObject)
        {
            isLockedOnTarget = false;

            targetShip = null;
            targetShipDamageRegion = null;
            targetguidHash = 0;

            if (targetGameObject == null)
            {
                // Reset last known position of the target
                targetLastPosition = Vector3.zero;

                // Used for turrets
                returnToParkTimer = 0f;
                isParked = false;
            }
            else
            {
                targetLastPosition = targetGameObject.transform.position;
            }

            this.target = targetGameObject;
        }

        /// <summary>
        /// Set the target ship for the weapon. Applies to Turret-type weapons and fixed projectile
        /// weapons with guided projectiles.
        /// Also sets the target (GameObject) for the weapon.
        /// </summary>
        /// <param name="targetShipControlModule"></param>
        public void SetTargetShip(ShipControlModule targetShipControlModule)
        {
            isLockedOnTarget = false;
            targetShip = targetShipControlModule;
            targetShipDamageRegion = null;
            targetguidHash = 0;

            if (targetShip == null)
            {
                target = null;
                // Reset last known position of the target
                targetLastPosition = Vector3.zero;

                // Used for turrets
                returnToParkTimer = 0f;
                isParked = false;
            }
            else
            {
                target = targetShip.gameObject;
                if (targetShip.IsInitialised) { targetLastPosition = targetShip.shipInstance.TransformPosition; }
                else { targetLastPosition = Vector3.zero; }
            }
        }

        /// <summary>
        /// Set the target ship damage region for the weapon.
        /// Also sets the target (GameObject) for the weapon.
        /// </summary>
        /// <param name="targetShipControlModule"></param>
        /// <param name="damageRegion"></param>
        public void SetTargetShipDamageRegion (ShipControlModule targetShipControlModule, DamageRegion damageRegion)
        {
            isLockedOnTarget = false;
            targetShip = targetShipControlModule;

            if (targetShip == null || damageRegion == null || damageRegion.guidHash == 0)
            {
                target = null;
                targetShip = null;
                targetguidHash = 0;
                targetShipDamageRegion = null;
                // Reset last known position of the target
                targetLastPosition = Vector3.zero;

                // Used for turrets
                returnToParkTimer = 0f;
                isParked = false;
            }
            else
            {
                // If the damage region child transform is set, consider using this for the gameObject to help with line of sight.
                target = damageRegion.regionChildTransform == null ? targetShip.gameObject : damageRegion.regionChildTransform.gameObject;
                targetguidHash = damageRegion.guidHash;
                targetShipDamageRegion = damageRegion;

                if (targetShip.IsInitialised)
                {
                    targetLastPosition = targetShip.shipInstance.GetDamageRegionWSPosition(damageRegion);
                }
                else { targetLastPosition = Vector3.zero; }
            }
        }

        /// <summary>
        /// Clear all targeting information for this weapon. This should be called if you do not know if the target
        /// is a gameobject or a ship.
        /// </summary>
        public void ClearTarget()
        {
            isLockedOnTarget = false;
            target = null;
            targetShip = null;
            targetguidHash = 0;
            // Reset last known position of the target
            targetLastPosition = Vector3.zero;

            // Used for turrets
            returnToParkTimer = 0f;
            isParked = false;
        }

        /// <summary>
        /// Get the current target for this weapon. Only applies to Turret-type weapons and FixedProjectile weapons with guided projectiles.
        /// </summary>
        /// <returns></returns>
        public GameObject GetTarget()
        {
            return target;
        }

        /// <summary>
        /// Get the current target ship for this weapon. Only applies to Turret-type weapons and FixedProjectile weapons with guided projectiles.
        /// NOTE: A weapon may target a GameObject but not a Ship. For this case call GetTarget().
        /// </summary>
        /// <returns></returns>
        public ShipControlModule GetTargetShip()
        {
            return targetShip;
        }

        /// <summary>
        /// Update the CurrentPerformance value of this weapon. It gets automatically
        /// called when the Health is changed and when heat levels change.
        /// </summary>
        public void UpdateWeaponPerformance()
        {
            // Update the current performance value
            if (heatLevel < 100f)
            {  
                currentPerformance = health / startingHealth;
                if (heatUpRate > 0f && heatLevel >= overHeatThreshold)
                {
                    currentPerformance *= 1f - SSCMath.Normalise(heatLevel, overHeatThreshold, 100f);
                }
                //currentPerformance = currentPerformance > minPerformance ? currentPerformance : minPerformance;
                currentPerformance = currentPerformance < 1f ? currentPerformance : 1f;
            }
            else { currentPerformance = 0f; }
        }

        #endregion
    }
}
