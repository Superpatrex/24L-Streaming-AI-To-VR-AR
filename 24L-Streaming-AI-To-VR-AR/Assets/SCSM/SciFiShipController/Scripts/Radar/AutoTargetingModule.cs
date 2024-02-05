using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Attached to a weapon-based system to automatically assign
    /// targets to weapons.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Weapon Components/Auto Targeting Module")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class AutoTargetingModule : MonoBehaviour
    {
        #region Enumerations
        
        // If this is modified, also update VerifyModule()
        public enum ModuleMode
        {
            ShipControlModule = 0,
            SurfaceTurretModule = 5
        }

        #endregion

        #region Public variables

        /// <summary>
        /// If enabled, the Initialise() will be called as soon as Start() runs. This should be disabled if you are
        /// initialising the turret through code and using the SurfaceTurretModule API methods.
        /// </summary>
        public bool initialiseOnStart = false;

        /// <summary>
        /// The mode that the AutoTargetingModule will operate in. This should match the
        /// module it is attached to.
        /// </summary>
        public ModuleMode moduleMode = ModuleMode.SurfaceTurretModule;

        /// <summary>
        /// When acquiring a new target, when enabled, this will verify there is a direct line of sight between
        /// the weapon and the target.
        /// </summary>
        public bool isCheckLineOfSightNewTarget = false;

        /// <summary>
        /// Whether the target should be reassigned periodically (after a fixed period of time).
        /// </summary>
        public bool updateTargetPeriodically = false;
        /// <summary>
        /// The time to wait (in seconds) before assigning a new target. Only valid if updateTargetPeriodically is enabled.
        /// </summary>
        public float updateTargetTime = 10f;

        /// <summary>
        /// Whether the current target can be 'lost' through either loss of line of sight or an inability to lock on to the target.
        /// </summary>
        public bool canLoseTarget = false;
        /// <summary>
        /// How long (in seconds) a target must be invalid for it to be lost (prompting a new target to be assigned).
        /// Only valid if canLoseTarget is enabled.
        /// </summary>
        public float targetLostTime = 2f;
        /// <summary>
        /// Whether a target can be 'lost' if line-of-sight is lost. Only valid if canLoseTarget is enabled.
        /// </summary>
        public bool isValidTargetRequireLOS = true;
        /// <summary>
        /// Whether a target can be 'lost' if the turret is unable to lock on to it. Only valid if canLoseTarget is enabled.
        /// </summary>
        public bool isValidTargetRequireTargetLock = true;

        public bool IsModuleModeValid { get; internal set; }

        /// <summary>
        /// Is the AutoTargetingModule initialised and ready for use?
        /// If not, call Initialise() or set initialiseOnStart in the Inspector
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// The number of targets that are currently in range of the  weapons with auto-targeting enabled.
        /// </summary>
        public int NumberOfTargetsInRange { get; private set; }

        /// <summary>
        /// The HUD to be used with a player ship. Targeting data is sent
        /// to the HUD. At runtime call SetHUD(...).
        /// </summary>
        public ShipDisplayModule shipDisplayModule;

        /// <summary>
        /// Should the targets be shown on the ShipDisplayModule (HUD)?
        /// At runtime call ShowTargetsOnHUD() or HideTargetsOnHUD().
        /// This will use existing Display Targets on the HUD.
        /// </summary>
        public bool isTargetsShownOnHUD = false;

        /// <summary>
        /// Are the radar targets being sent to the HUD?
        /// </summary>
        public bool IsTargetsShownOnHUD { get { return isTargetsShownOnHUD; } }

        /// <summary>
        /// Used for debugging in the editor
        /// </summary>
        public SSCRadarQuery GetCurrentQuery { get { return sscRadarQuery; } }

        /// <summary>
        /// Used for debugging purposes only. Do NOT hold a reference to this
        /// list over multiple frames or any of the blips within the list.
        /// </summary>
        public List<SSCRadarBlip> GetBlipList { get { return blipsList; } }

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool allowRepaint = false;

        #endregion

        #region Private varibles

        private bool isInitialised = false;
        private SSCRadar sscRadar = null;
        private List<SSCRadarBlip> blipsList = null;
        private SSCRadarQuery sscRadarQuery = null;

        private int[] excludeNeutralAndSelf = null;

        private ShipControlModule shipControlModule = null;
        private SurfaceTurretModule surfaceTurretModule = null;

        // To avoid enumeration lookups
        private int moduleModeInt = -1;
        private int mmShipControlModuleInt = (int)ModuleMode.ShipControlModule;
        private int mmSurfaceTurretModuleInt = (int)ModuleMode.SurfaceTurretModule;

        #endregion

        #region Public Delegates

        public delegate void CallbackOnTargeted1 (ShipControlModule sourceShip, ShipControlModule targetShip, DamageRegion targetDamageRegion, GameObject targetObject);
        public delegate void CallbackOnTargeted2 (SurfaceTurretModule sourceSurfaceTurret, ShipControlModule targetShip, DamageRegion targetDamageRegion, GameObject targetObject);

        /// <summary>
        /// The name of the custom method that is called immediately after the ship weapon acquires a new target.
        /// Your method must take 4 parameters (ShipControlModule, ShipControlModule, DamageRegion and Gameobject).
        /// Any of the last 3 parameters can be null.
        /// This should be a lightweight method to avoid performance issues.
        /// </summary>
        [System.NonSerialized] public CallbackOnTargeted1 callbackOnTargeted1 = null;

        /// <summary>
        /// The name of the custom method that is called immediately after the surface turret acquires a new target.
        /// Your method must take 4 parameters (SurfaceTurretModule, ShipControlModule, DamageRegion and Gameobject).
        /// Any of the last 3 parameters can be null.
        /// This should be a lightweight method to avoid performance issues.
        /// </summary>
        [System.NonSerialized] public CallbackOnTargeted2 callbackOnTargeted2 = null;

        #endregion

        #region Initialise Methods

        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        /// <summary>
        /// Call this method if adding this component at runtime or after updating some fields.
        /// </summary>
        public void Initialise()
        {
            if (!isInitialised)
            {
                VerifyModule();

                if (IsModuleModeValid)
                {
                    sscRadar = SSCRadar.GetOrCreateRadar();

                    blipsList = new List<SSCRadarBlip>(20);

                    moduleModeInt = (int)moduleMode;

                    int selfFactionId = 0;
                    if (moduleModeInt == mmShipControlModuleInt)
                    {
                        if (shipControlModule.shipInstance != null) { selfFactionId = shipControlModule.shipInstance.factionId; }
                    }
                    else if (moduleModeInt == mmSurfaceTurretModuleInt)
                    {
                        selfFactionId = surfaceTurretModule.factionId;
                    }

                    excludeNeutralAndSelf = new int[] { 0, selfFactionId };

                    // Set up a default query for auto targeting
                    sscRadarQuery = new SSCRadarQuery()
                    {
                        // The centre position should be updated each time the query is run
                        centrePosition = transform.position,
                        factionId = -1,
                        // exclude neutral items (0), and anything in the same alliance as the turret
                        factionsToExclude = excludeNeutralAndSelf,
                        is3DQueryEnabled = true,
                        // return the closest enemy first
                        querySortOrder = SSCRadarQuery.QuerySortOrder.DistanceAsc3D,
                        // Start with no range
                        range = 0f
                    };

                    isInitialised = sscRadar != null;
                }
            }
        }

        #endregion

        #region Update Methods

        private void Update()
        {
            if (isInitialised)
            {
                if (moduleModeInt == mmSurfaceTurretModuleInt)
                {
                    UpdateSurfaceTurretTarget();
                }
                else if (moduleModeInt == mmShipControlModuleInt)
                {
                    UpdateShipWeaponTargets();
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used to verify if the module is attached to a matching component
        /// </summary>
        public void VerifyModule()
        {
            int modeInt = (int)moduleMode;

            // ShipControlModule
            if (modeInt == 0)
            {
                surfaceTurretModule = null;
                shipControlModule = GetComponent<ShipControlModule>();
                IsModuleModeValid = shipControlModule != null;
            }
            // SurfaceTurretModule
            else if (modeInt == 5)
            {
                shipControlModule = null;
                surfaceTurretModule = GetComponent<SurfaceTurretModule>();
                IsModuleModeValid = surfaceTurretModule != null;
            }
            // Anything unsupported
            else
            {
                surfaceTurretModule = null;
                shipControlModule = null;
                IsModuleModeValid = false;
            }
        }

        #endregion

        #region Private Member Methods

        /// <summary>
        /// Is the current weapon target valid, or is a new target required?
        /// If there are no enemy, the existing target must be invalid, so return true.
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="numEnemy"></param>
        /// <returns></returns>
        private bool IsNewTargetRequired(Weapon weapon, int numEnemy)
        {
            bool isNewTargetRequired = true;

            int targetGameObjectInstanceId = weapon.target.GetInstanceID();
            float estimatedRangeSqr = weapon.estimatedRange * weapon.estimatedRange;

            // Update the invalid target timer for this weapon (should happen even if no enemy)
            // FUTURE - is the fixed projectile weapon with guided projectile locked on to the target?
            if ((!isValidTargetRequireLOS || weapon.HasLineOfSight) &&
               (!isValidTargetRequireTargetLock || weapon.isLockedOnTarget || weapon.weaponTypeInt == Weapon.FixedProjectileInt || weapon.weaponTypeInt == Weapon.FixedBeamInt))
            {
                weapon.invalidTargetTimer = 0f;
            }
            else { weapon.invalidTargetTimer += Time.deltaTime; }

            // Is the target still in range?
            // FUTURE consideration
            // 1. Is the target within the firing cone of the weapon?
            // 2. How long has the weapon been trying to obtain a lock?
            for (int bIdx = 0; bIdx < numEnemy; bIdx++)
            {
                SSCRadarBlip blip = blipsList[bIdx];

                // Check ship damage region (with a childTransform) first
                if (blip.guidHash != 0 && blip.shipControlModule != null && blip.itemGameObject != null && blip.itemGameObject.GetInstanceID() == targetGameObjectInstanceId)
                {
                    // Found the matching blip for this target. If out of range of this weapon, then we need a new target
                    isNewTargetRequired = blip.distanceSqr3D > estimatedRangeSqr || weapon.invalidTargetTimer >= targetLostTime;
                    break;
                }
                // Check a ship or ship damage region (without a childTransform)
                else if (blip.shipControlModule != null && blip.shipControlModule.gameObject.GetInstanceID() == targetGameObjectInstanceId)
                {
                    // Found the matching blip for this target. If out of range of this weapon, then we need a new target
                    isNewTargetRequired = blip.distanceSqr3D > estimatedRangeSqr || weapon.invalidTargetTimer >= targetLostTime;
                    break;
                }
                else if (blip.itemGameObject != null && blip.itemGameObject.GetInstanceID() == targetGameObjectInstanceId)
                {
                    // Found the matching blip for this target. If out of range of this weapon, then we need a new target
                    isNewTargetRequired = blip.distanceSqr3D > estimatedRangeSqr || weapon.invalidTargetTimer >= targetLostTime;
                    break;
                }

            }

            return isNewTargetRequired;
        }

        /// <summary>
        /// Turn on/off all Display Targets on the ShipDisplayModule
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideTargetsOnHUD(bool isShown)
        {
            if (isInitialised && shipDisplayModule != null)
            {
                isTargetsShownOnHUD = isShown;
            }
            else { isTargetsShownOnHUD = false; }

            if (shipDisplayModule != null && shipDisplayModule.IsInitialised)
            {
                if (isTargetsShownOnHUD) { shipDisplayModule.ShowDisplayTargets(); }
                else { shipDisplayModule.HideDisplayTargets(); }
            }
        }

        /// <summary>
        /// Update all weapons on a ship with AutoTargeting enabled.
        /// NOTE: Currently, it will always run a radar query, even if there are no weapons that have AutoTargeting.
        /// Ships will never fire at radar items in the same faction.
        /// </summary>
        private void UpdateShipWeaponTargets()
        {
            if (shipControlModule != null && shipControlModule.IsInitialised && shipControlModule.ShipIsEnabled())
            {
                int numWeapons = shipControlModule.shipInstance.weaponList == null ? 0 : shipControlModule.shipInstance.weaponList.Count;
                int numBlips = 0;
                bool isNewTargetRequired = true;
                int selfFactionId = shipControlModule.shipInstance.factionId;

                // Display Target notes
                // 1. The number of display targets will depend on if this feature is enabled
                // 2. When assigned to a weapon, the weaponIndex wil be stored in the DisplayTarget class instance

                int numDisplayTargets = !isTargetsShownOnHUD || shipDisplayModule == null || !shipDisplayModule.IsHUDShown ? 0 : shipDisplayModule.GetNumberDisplayTargets;

                // If we have a HUD, build a minimal radar query with the required targets
                if (numDisplayTargets > 0) { shipDisplayModule.PopulateTargetingRadarQuery(sscRadarQuery); }
                // Otherwise just create a radar query that excludes our faction and the neutral faction
                else
                {
                    sscRadarQuery.factionsToInclude = null;
                    sscRadarQuery.factionsToExclude = excludeNeutralAndSelf;
                    sscRadarQuery.squadronsToInclude = null;
                    sscRadarQuery.squadronsToExclude = null;
                }

                if (numWeapons > 0)
                {
                    // NOTE: Assumes blip list only contains enemy
                    // If we want to also contain friendly ships it will need more work...

                    /// TODO - optimise by only updating when required
                    shipControlModule.shipInstance.UpdateMaxAutoTargetRange();

                    // Prepare and run a radar query
                    sscRadarQuery.range = shipControlModule.shipInstance.estimatedMaxAutoTargetRange;
                }

                if (numDisplayTargets > 0 || numWeapons > 0)
                {
                    // Prepare and run a radar query
                    sscRadarQuery.centrePosition = shipControlModule.shipInstance.TransformPosition;

                    sscRadar.GetRadarResults(sscRadarQuery, blipsList);
                    numBlips = blipsList == null ? 0 : blipsList.Count;
                }

                // ASSUMES all blips are enemy
                NumberOfTargetsInRange = numBlips;

                #region AutoTargeting Turret weapons
                for (int wpIdx = 0; wpIdx < numWeapons; wpIdx++)
                {
                    Weapon weapon = shipControlModule.shipInstance.weaponList[wpIdx];

                    // Only process turret weapons with Auto Targeting enabled
                    if (weapon != null && weapon.isAutoTargetingEnabled && (weapon.weaponTypeInt == Weapon.TurretProjectileInt || weapon.weaponTypeInt == Weapon.TurretBeamInt))
                    {
                        isNewTargetRequired = true;
                        float estimatedRangeSqr = weapon.estimatedRange * weapon.estimatedRange;

                        #region Check if a new target is required
                        if (weapon.target != null)
                        {
                            // Update assigned new target timer
                            if (updateTargetPeriodically) { weapon.assignedNewTargetTimer += Time.deltaTime; }

                            // Check if we need to get a new target
                            isNewTargetRequired = (updateTargetPeriodically && weapon.assignedNewTargetTimer > updateTargetTime) ||
                                IsNewTargetRequired(weapon, numBlips);

                            // If we had a target which has gone out of range and there are no other candidates
                            // remove the target from the weapon.
                            if (isNewTargetRequired && numBlips < 1)
                            {
                                shipControlModule.shipInstance.ClearWeaponTarget(wpIdx);
                                // Reset invalid target timer
                                weapon.invalidTargetTimer = 0f;
                                // Reset assigned new target timer
                                weapon.assignedNewTargetTimer = 0f;
                                // Do not try to find another target if there are no enemy
                                isNewTargetRequired = false;
                            }
                        }
                        #endregion

                        #region Find and assign a new target

                        // Target the first suitable candidate
                        // In the FUTURE we might consider:
                        // 1. Least time to rotate a turret weapon towards target
                        // 2. permit the targeting of Locations (which don't have a gameobject)
                        // 3. try not to assign the same enemy to multiple turret weapons
                        if (isNewTargetRequired && numBlips > 0)
                        {
                            // Loop through all the enemy radar blips
                            for (int bIdx = 0; bIdx < numBlips; bIdx++)
                            {
                                SSCRadarBlip blip = blipsList[bIdx];

                                #region Check how suitable this enemy blip is for targeting
                                // Weapon range could be less than estimatedMaxTurretRange of all weapons on the ship
                                if (blip.factionId == selfFactionId || blip.factionId == SSCRadar.NEUTRAL_FACTION || blip.distanceSqr3D > estimatedRangeSqr) { continue; }

                                // Skip this blip if it is not within the firing cone of the weapon

                                // Find the position of the target in turret space
                                Vector3 targetTurretSpacePos = weapon.turretPivotY.parent.InverseTransformPoint(blip.wsPosition);
                                // Get azimuth and altitude angles of the target
                                float azimuthAngle = Mathf.Atan2(targetTurretSpacePos.x, targetTurretSpacePos.z) * Mathf.Rad2Deg;
                                float altitudeAngle = Mathf.Atan(targetTurretSpacePos.y /
                                    Mathf.Sqrt((targetTurretSpacePos.x * targetTurretSpacePos.x) + (targetTurretSpacePos.z * targetTurretSpacePos.z)))
                                    * Mathf.Rad2Deg;
                                if (azimuthAngle < weapon.turretMinY || azimuthAngle > weapon.turretMaxY ||
                                    altitudeAngle < weapon.turretMinX || altitudeAngle > weapon.turretMaxX) { continue; }
                                #endregion

                                #region Assign the target and consider LoS and HUD if enabled
                                if (blip.shipControlModule != null)
                                {
                                    // Check if this target is a ship or the damage region of a ship
                                    if (blip.guidHash == 0)
                                    {
                                        // Skip this blip if LoS is enabled but no direct LoS is available to the damage region
                                        if (isCheckLineOfSightNewTarget && !shipControlModule.shipInstance.WeaponHasLineOfSight(weapon, blip.shipControlModule.gameObject, true, true, false)) { continue; }

                                        weapon.SetTargetShip(blip.shipControlModule);

                                        if (callbackOnTargeted1 != null) { callbackOnTargeted1.Invoke(shipControlModule, blip.shipControlModule, null, blip.shipControlModule.gameObject); }
                                    }
                                    else
                                    {
                                        // Skip this blip if LoS is enabled but no direct LoS is available
                                        // If available, use the damage region child transform which is stored in blip.itemGameObject
                                        if (isCheckLineOfSightNewTarget && !shipControlModule.shipInstance.WeaponHasLineOfSight(weapon, blip.itemGameObject == null ? blip.shipControlModule.gameObject : blip.itemGameObject, true, true, false)) { continue; }

                                        // Set the target as the ship's damage region
                                        DamageRegion _damageRegion = blip.shipControlModule.shipInstance.GetDamageRegion(blip.guidHash);
                                        weapon.SetTargetShipDamageRegion(blip.shipControlModule, _damageRegion);

                                        if (callbackOnTargeted1 != null) { callbackOnTargeted1.Invoke(shipControlModule, blip.shipControlModule, _damageRegion, null); }
                                    }

                                    // To "fix" issue of turret beams not working in builds
                                    // weapon.isLockedOnTarget is false in ship.MoveBeam() in build (fine in editor)
                                    // Reset invalid target timer
                                    weapon.invalidTargetTimer = 0f;
                                    // Reset assigned new target timer
                                    weapon.assignedNewTargetTimer = 0f;

                                    isNewTargetRequired = false;
                                    break;
                                }
                                else if (blip.itemGameObject != null)
                                {
                                    // Skip this blip if LoS is enabled but no direct LoS is available
                                    if (isCheckLineOfSightNewTarget && !shipControlModule.shipInstance.WeaponHasLineOfSight(weapon, blip.itemGameObject, true, true, false)) { continue; }

                                    weapon.SetTarget(blip.itemGameObject);

                                    if (callbackOnTargeted1 != null) { callbackOnTargeted1.Invoke(shipControlModule, null, null, blip.itemGameObject); }

                                    // To "fix" issue of turret beams not working in builds
                                    // weapon.isLockedOnTarget is false in ship.MoveBeam() in build (fine in editor)
                                    // Reset invalid target timer
                                    weapon.invalidTargetTimer = 0f;
                                    // Reset assigned new target timer
                                    weapon.assignedNewTargetTimer = 0f;

                                    isNewTargetRequired = false;
                                    break;
                                }
                                #endregion
                            }
                        }
                        #endregion

                        // If we tried to find a target but could not, the previous target may still be set
                        if (isNewTargetRequired && weapon.target != null)
                        {
                            shipControlModule.shipInstance.ClearWeaponTarget(wpIdx);
                            // Reset invalid target timer
                            weapon.invalidTargetTimer = 0f;
                            // Reset assigned new target timer
                            weapon.assignedNewTargetTimer = 0f;
                        }
                    }
                } // End weapon loop
                #endregion End Turret Weapons

                if (numDisplayTargets > 0)
                {
                    #region Allocate Display Targets

                    // Set the score for all the targets to be negative one
                    for (int dIdx = 0; dIdx < numDisplayTargets; dIdx++)
                    {
                        // Get the current display target
                        DisplayTarget displayTarget = shipDisplayModule.displayTargetList[dIdx];
                        // Loop through the slots
                        for (int sIdx = 0; sIdx < displayTarget.maxNumberOfTargets; sIdx++)
                        {
                            // Set the score to -1
                            displayTarget.displayTargetSlotList[sIdx].fixedWeaponTargetScore = -1f;
                            // Set the target slot to be invalid initially
                            shipDisplayModule.AssignDisplayTargetSlot(dIdx, sIdx, -1, 0, true);
                        }
                    }

                    // Loop through all the blips and find blips that match the criteria for each display target
                    for (int bIdx = 0; bIdx < numBlips; bIdx++)
                    {
                        // Get the current blip
                        SSCRadarBlip blip = blipsList[bIdx];

                        // TODO: Is this something that we always want to do? Maybe it should be optional...
                        // Skip this target if it would not be visible in the HUD viewport
                        // This will only make sense with forward facing weapons
                        if (!sscRadar.IsBlipInScreenViewPort(blip, shipDisplayModule.mainCamera, shipDisplayModule.ScreenResolution,
                                     shipDisplayModule.GetTargetsViewportSize, shipDisplayModule.GetTargetsViewportOffset)) { continue; }

                        // Loop through the list of display targets and check this blip to see if it matches the criteria
                        for (int dIdx = 0; dIdx < numDisplayTargets; dIdx++)
                        {
                            // Get the current display target
                            DisplayTarget displayTarget = shipDisplayModule.displayTargetList[dIdx];

                            // Skip any display target without any targets 
                            // NOTE: This should never the case but could be done in user code
                            if (displayTarget.maxNumberOfTargets < 1) { continue; }

                            #region Check If Blip Matches Display Target Criteria

                            // Check if the blip matches the faction criteria (it automatically matches if there are no specified factions)
                            int numDTFactionsToInclude = displayTarget.factionsToInclude == null ? 0 : displayTarget.factionsToInclude.Length;
                            bool blipMatchesFaction = numDTFactionsToInclude == 0;
                            for (int fIdx = 0; fIdx < numDTFactionsToInclude; fIdx++)
                            {
                                if (blip.factionId == displayTarget.factionsToInclude[fIdx]) { blipMatchesFaction = true; break; }
                            }

                            // We only need to check squadron criteria if the blip matched the faction criteria
                            bool blipMatchesSquadron = false;
                            if (blipMatchesFaction)
                            {
                                // Check if the blip matches the squadron criteria (it automatically matches if there are no specified squadrons)
                                int numDTSquadronsToInclude = displayTarget.squadronsToInclude == null ? 0 : displayTarget.squadronsToInclude.Length;
                                blipMatchesSquadron = numDTSquadronsToInclude == 0;
                                for (int sqIdx = 0; sqIdx < numDTSquadronsToInclude; sqIdx++)
                                {
                                    if (blip.squadronId == displayTarget.squadronsToInclude[sqIdx]) { blipMatchesSquadron = true; break; }
                                }
                            }

                            #endregion Check If Blip Matches Display Target Criteria

                            if (blipMatchesFaction && blipMatchesSquadron)
                            {
                                // Score this target based on its position
                                float thisTargetScore = CalculateFixedWeaponTargetScore(shipControlModule, blip);

                                #region Check If This is a Valid Target to Display

                                // Loop through the list of the current best scores for this display target, and 
                                // find the lowest score (and its corresponding index)
                                float displayTargetLowestScore = displayTarget.displayTargetSlotList[0].fixedWeaponTargetScore;
                                int displayTargetLowestScoreIndex = 0;
                                for (int sIdx = 1; sIdx < displayTarget.maxNumberOfTargets; sIdx++)
                                {
                                    // Compare this score to the current lowest score
                                    DisplayTargetSlot displayTargetSlot = displayTarget.displayTargetSlotList[sIdx];
                                    if (displayTargetSlot.fixedWeaponTargetScore < displayTargetLowestScore)
                                    {
                                        displayTargetLowestScore = displayTargetSlot.fixedWeaponTargetScore;
                                        displayTargetLowestScoreIndex = sIdx;
                                    }
                                }

                                // Check if this target's score is higher than the current lowest display target score
                                if (thisTargetScore > displayTargetLowestScore)
                                {
                                    // If so, replace the lowest display target score with this one
                                    displayTarget.displayTargetSlotList[displayTargetLowestScoreIndex].fixedWeaponTargetScore = thisTargetScore;
                                    // Set this target to be displayed as a display target (replacing the target with the lowest score)
                                    shipDisplayModule.AssignDisplayTargetSlot(dIdx, displayTargetLowestScoreIndex, blip.radarItemIndex, blip.radarItemSequenceNumber, true);
                                }

                                #endregion Check If This is a Valid Target to Display

                                // Stop looping once we find a display target match for this blip
                                dIdx = numDisplayTargets;
                            }
                        }
                    }

                    #endregion Allocate Display Targets

                    #region Assign Fixed Weapons Target

                    float fixedWeaponBestTargetScore = 0f;
                    SSCRadarItem fixedWeaponBestTargetRadarItem = null;

                    // Loop through the list of display targets
                    for (int dIdx = 0; dIdx < numDisplayTargets; dIdx++)
                    {
                        // Get the current display target
                        DisplayTarget displayTarget = shipDisplayModule.displayTargetList[dIdx];

                        // Only consider display targets that we have marked as targetable
                        if (displayTarget.isTargetable)
                        {
                            // Loop through the list of display target slots
                            for (int sIdx = 0; sIdx < displayTarget.maxNumberOfTargets; sIdx++)
                            {
                                // First try and get the radar item for this target - check that it is valid
                                SSCRadarItemKey targetRadarItemKey = shipDisplayModule.GetAssignedDisplayTarget(dIdx, sIdx);
                                SSCRadarItem targetRadarItem = sscRadar.GetRadarItem(targetRadarItemKey.radarItemIndex, targetRadarItemKey.radarItemSequenceNumber);
                                if (targetRadarItem != null)
                                {
                                    // If check line of sight for new targets is enabled, target validity depends on whether we
                                    // have line-of-sight to the new weapon
                                    bool isTargetValid = !isCheckLineOfSightNewTarget;
                                    if (isCheckLineOfSightNewTarget)
                                    {
                                        // Try and find the gameobject for this target
                                        GameObject targetGameObject = null;
                                        if (targetRadarItem.shipControlModule != null)
                                        {
                                            // Check for ship damage region target
                                            if (targetRadarItem.guidHash != 0 && targetRadarItem.itemGameObject != null)
                                            {
                                                targetGameObject = targetRadarItem.itemGameObject;
                                            }
                                            else
                                            {
                                                targetGameObject = targetRadarItem.shipControlModule.gameObject;
                                            }
                                        }
                                        else if (targetRadarItem.itemGameObject != null)
                                        {
                                            targetGameObject = targetRadarItem.itemGameObject;
                                        }

                                        if (targetGameObject != null)
                                        {
                                            // Loop through the fixed weapons and see if any of the weapons has line of sight to this target
                                            for (int wpIdx = 0; wpIdx < numWeapons; wpIdx++)
                                            {
                                                // Get the weapon reference
                                                Weapon weapon = shipControlModule.shipInstance.weaponList[wpIdx];

                                                // Only process fixed weapons with Auto Targeting enabled
                                                if (weapon != null && weapon.isAutoTargetingEnabled && (weapon.weaponTypeInt == Weapon.FixedProjectileInt || weapon.weaponTypeInt == Weapon.FixedBeamInt))
                                                {
                                                    // Check if this weapon has line of sight to the target
                                                    if (shipControlModule.shipInstance.WeaponHasLineOfSight(weapon, targetGameObject, true, true, false))
                                                    {
                                                        // If it does, skip all the other weapons and mark the target as valid
                                                        // We only need one weapon to have line-of-sight to the target
                                                        isTargetValid = true;
                                                        wpIdx = numWeapons;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // Compare this score to the current best target score
                                    DisplayTargetSlot displayTargetSlot = displayTarget.displayTargetSlotList[sIdx];
                                    if (isTargetValid && displayTargetSlot.fixedWeaponTargetScore > fixedWeaponBestTargetScore)
                                    {
                                        // If it is better, remember this as the best target score
                                        fixedWeaponBestTargetScore = displayTargetSlot.fixedWeaponTargetScore;
                                        // Remember the corresponding radar item for this target
                                        fixedWeaponBestTargetRadarItem = targetRadarItem;
                                    }
                                }
                            }
                        }
                    }

                    // Loop through the fixed weapons and set their targets to the best target we found
                    for (int wpIdx = 0; wpIdx < numWeapons; wpIdx++)
                    {
                        // Get the weapon reference
                        Weapon weapon = shipControlModule.shipInstance.weaponList[wpIdx];

                        // Only process fixed weapons with Auto Targeting enabled
                        if (weapon != null && weapon.isAutoTargetingEnabled && (weapon.weaponTypeInt == Weapon.FixedProjectileInt || weapon.weaponTypeInt == Weapon.FixedBeamInt))
                        {
                            // If the radar item retrieved is not null, we have a valid radar item
                            if (fixedWeaponBestTargetRadarItem != null)
                            {
                                // Set the target of the weapon
                                // TODO: Maybe at some point this should work for things that aren't ships/gameobjects?
                                if (fixedWeaponBestTargetRadarItem.shipControlModule != null)
                                {
                                    // Check if this is a damage region target
                                    if (fixedWeaponBestTargetRadarItem.guidHash == 0)
                                    {
                                        weapon.SetTargetShip(fixedWeaponBestTargetRadarItem.shipControlModule);
                                    }
                                    else
                                    {
                                        // Set the target as the ship's damage region
                                        weapon.SetTargetShipDamageRegion(fixedWeaponBestTargetRadarItem.shipControlModule,
                                                                         fixedWeaponBestTargetRadarItem.shipControlModule.shipInstance.GetDamageRegion(fixedWeaponBestTargetRadarItem.guidHash));
                                    }
                                    
                                }
                                else if (fixedWeaponBestTargetRadarItem.itemGameObject != null)
                                {
                                    weapon.SetTarget(fixedWeaponBestTargetRadarItem.itemGameObject);
                                }
                            }
                            // If we do not have a valid radar item, we need to unassign each weapon
                            else
                            {
                                weapon.ClearTarget();
                            }
                        }
                    }

                    #endregion Assign Fixed Weapons Target
                }
            }
            else { NumberOfTargetsInRange = 0; }
        }

        /// <summary>
        /// Check if the weapon has a target assigned. If so, check if it
        /// is still a valid target (i.e. within range).
        /// If required, look for a new target that is within range.
        /// </summary>
        private void UpdateSurfaceTurretTarget()
        {
            if (surfaceTurretModule != null && surfaceTurretModule.IsInitialised)
            {
                Weapon weapon = surfaceTurretModule.weapon;

                sscRadarQuery.centrePosition = surfaceTurretModule.TransformPosition;
                sscRadarQuery.range = weapon.estimatedRange;

                // The default Surface Turret query should only return enemy
                sscRadar.GetRadarResults(sscRadarQuery, blipsList);

                int numBlips = blipsList == null ? 0 : blipsList.Count;

                NumberOfTargetsInRange = numBlips;

                bool isNewTargetRequired = true;

                #region Check if a new target is required
                if (weapon.target != null)
                {
                    // Update assigned new target timer
                    if (updateTargetPeriodically) { weapon.assignedNewTargetTimer += Time.deltaTime; }

                    // Check if we need to get a new target
                    isNewTargetRequired = (updateTargetPeriodically && weapon.assignedNewTargetTimer > updateTargetTime) ||
                        IsNewTargetRequired(weapon, numBlips);

                    // If we had a target which has gone out of range and there are no other candidates
                    // remove the target from the weapon.
                    if (isNewTargetRequired && numBlips < 1)
                    {
                        //surfaceTurretModule.SetWeaponTarget(null);
                        surfaceTurretModule.ClearWeaponTarget();
                        // Reset invalid target timer
                        weapon.invalidTargetTimer = 0f;
                        // Reset assigned new target timer
                        weapon.assignedNewTargetTimer = 0f;
                    }
                }
                #endregion

                // Target the first suitable candidate
                // In the FUTURE we might consider:
                // 1. Least time to rotate weapon towards target
                // 2. blips within the firing cone of the weapon
                // 3. permit the targetting of Locations (which don't have a gameobject)
                if (isNewTargetRequired && numBlips > 0)
                {
                    for (int bIdx = 0; bIdx < numBlips; bIdx++)
                    {
                        SSCRadarBlip blip = blipsList[bIdx];

                        #region Check how suitable this enemy blip is for targeting
                        // Skip this blip if it is not within the firing cone of the weapon
                        // Find the position of the target in turret space
                        Quaternion turretParentInverseRotation = Quaternion.Inverse(weapon.turretPivotY.parent.rotation);
                        Vector3 turretRelativePosition = ((turretParentInverseRotation * weapon.turretPivotX.rotation) * weapon.relativePosition) + weapon.turretPivotY.parent.transform.position;
                        // Transform into turret space using rotation of pivot Y parent object and position of turret relative position
                        Vector3 targetTurretSpacePos = turretParentInverseRotation * (blip.wsPosition - turretRelativePosition);
                        // Get azimuth and altitude angles of the target
                        float azimuthAngle = Mathf.Atan2(targetTurretSpacePos.x, targetTurretSpacePos.z) * Mathf.Rad2Deg;
                        float altitudeAngle = Mathf.Atan(targetTurretSpacePos.y /
                            Mathf.Sqrt((targetTurretSpacePos.x * targetTurretSpacePos.x) + (targetTurretSpacePos.z * targetTurretSpacePos.z)))
                            * Mathf.Rad2Deg;
                        if (azimuthAngle < weapon.turretMinY || azimuthAngle > weapon.turretMaxY ||
                            altitudeAngle < weapon.turretMinX || altitudeAngle > weapon.turretMaxX) { continue; }
                        #endregion

                        #region Assign the target and consider LoS if enabled
                        if (blip.shipControlModule != null)
                        {
                            // Check if this target is a ship or the damage region of a ship
                            if (blip.guidHash == 0)
                            {
                                // Skip this blip if LoS is enabled but no direct LoS is available
                                if (isCheckLineOfSightNewTarget && !surfaceTurretModule.WeaponHasLineOfSight(blip.shipControlModule.gameObject, true, true, false)) { continue; }

                                weapon.SetTargetShip(blip.shipControlModule);

                                if (callbackOnTargeted2 != null) { callbackOnTargeted2.Invoke(surfaceTurretModule, blip.shipControlModule, null, blip.shipControlModule.gameObject); }
                            }
                            else
                            {
                                // Skip this blip if LoS is enabled but no direct LoS is available to the damage region
                                // If available, use the damage region child transform which is stored in blip.itemGameObject
                                if (isCheckLineOfSightNewTarget && !surfaceTurretModule.WeaponHasLineOfSight(blip.itemGameObject == null ? blip.shipControlModule.gameObject : blip.itemGameObject, true, true, false)) { continue; }

                                DamageRegion _damageRegion = blip.shipControlModule.shipInstance.GetDamageRegion(blip.guidHash);

                                // Set the target as the ship's damage region
                                weapon.SetTargetShipDamageRegion(blip.shipControlModule, _damageRegion);

                                if (callbackOnTargeted2 != null) { callbackOnTargeted2.Invoke(surfaceTurretModule, blip.shipControlModule, _damageRegion, null); }

                            }
                            break;
                        }
                        else if (blip.itemGameObject != null)
                        {
                            // Skip this blip if LoS is enabled but no direct LoS is available
                            if (isCheckLineOfSightNewTarget && !surfaceTurretModule.WeaponHasLineOfSight(blip.itemGameObject, true, true, false)) { continue; }

                            surfaceTurretModule.SetWeaponTarget(blip.itemGameObject);

                            if (callbackOnTargeted2 != null) { callbackOnTargeted2.Invoke(surfaceTurretModule, null, null, blip.itemGameObject); }
                            break;
                        }
                        #endregion
                    }
                }
            }
            else { NumberOfTargetsInRange = 0; }
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// If the Module Mode is ShipControlModule, assign the HUD
        /// to the AutoTargetingModule - else set it to null.
        /// </summary>
        /// <param name="shipDisplayModule"></param>
        public void SetHUD(ShipDisplayModule shipDisplayModule)
        {
            if (moduleModeInt == mmShipControlModuleInt)
            {
                this.shipDisplayModule = shipDisplayModule;
            }
            else { this.shipDisplayModule = null; }
        }

        /// <summary>
        /// If the HUD is initialised and shown, start sending Target data to the HUD.
        /// Only Display Targets already on the HUD will be updated.
        /// See also shipDisplayModule.AddTarget(..).
        /// </summary>
        public void ShowTargetsOnHUD()
        {
            ShowOrHideTargetsOnHUD(true);
        }

        /// <summary>
        /// Stop sending targeting data to the HUD. Also, turn off any Display Targets
        /// on the HUD.
        /// </summary>
        public void HideTargetsOnHUD()
        {
            ShowOrHideTargetsOnHUD(false);
        }

        /// <summary>
        /// Calculates a fixed weapon targeting score for a target given a source ship. Higher scores indicate better targets.
        /// The maximum score is 1000. Targets behind the ship get assigned a score of -1 to indicate that they are invalid.
        /// </summary>
        /// <param name="sourceShip"></param>
        /// <param name="targetBlip"></param>
        /// <returns></returns>
        public float CalculateFixedWeaponTargetScore(ShipControlModule sourceShip, SSCRadarBlip targetBlip)
        {
            // Score starts as 1000 (which is the maximum)
            float targetScore = 1000f;

            // Calculate the dot product of the ship's forward direction and the vector from the ship to the target
            float forwardsToShipDotProduct = Vector3.Dot(sourceShip.shipInstance.TransformForward, targetBlip.wsPosition - sourceShip.shipInstance.TransformPosition);
            // If this is positive, then the target is in front the ship
            if (forwardsToShipDotProduct > 0f)
            {
                // Normalise the dot product
                float targetBlipDistance3D = Mathf.Sqrt(targetBlip.distanceSqr3D);
                float normalisedDotProduct = forwardsToShipDotProduct / targetBlipDistance3D;
                // This normalised dot product is one when the target is straight ahead, and decreases as the angle increases
                // until it is zero when the target is at a position perpendicular to the forwards direction of the ship
                // We multiply the score by this value, so that the score increases as angle decreases
                targetScore *= normalisedDotProduct;
                // The score should also increase with decreasing distance, in the ratio of:
                // As distance decreases by a factor of 10, score increases by a factor of 1.1
                float logarithmicDistance = Mathf.Log10(targetBlipDistance3D / 100f);
                if (logarithmicDistance < 0f) { logarithmicDistance = 0f; }
                //float modifiedDistance = Mathf.Pow(2f, logarithmicDistance);
                float modifiedDistance = Mathf.Pow(1.1f, logarithmicDistance);
                // Divide the score by this modified distance
                targetScore /= modifiedDistance;
            }
            // Otherwise, the target is behind the ship
            else
            {
                // Targets behind the ship just get assigned a score of -1 (invalid target)
                targetScore = -1f;
            }

            return targetScore;
        }

        #endregion
    }
}