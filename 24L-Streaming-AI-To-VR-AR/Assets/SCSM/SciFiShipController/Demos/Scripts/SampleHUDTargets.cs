using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This sample shows how you can:
    /// 1. Run radar queries for a player ship to find nearby enemy
    /// 2. Use DisplayTargets on a HUD to track enemy on the screen
    /// 3. Assign targets to weapons that use Guided Projectiles
    /// 4. Get notified when an enemy is hit
    /// This is only sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own
    /// namespace.
    /// SETUP:
    /// 1. Add Prefabs\Visuals\HUD1 prefab to the scene
    /// 2. Add player ship from Prefabs\Ships to the scene (ones that don't have NPC in the name)
    /// 3. Remove or disable the default Main Camera
    /// 4. Add Prefabs\Environment\PlayerCamera prefab to the scene
    /// 5. Hook up the player ship to the PlayerCamera
    /// 6. Add this script to the player ship
    /// 7. Hook up the HUD to the slot on this script
    /// 8. Make sure you player ship has at least one weapon with a guided Projectile
    /// 9. Ensure the weapon has Auto Targeting enabled.
    /// NOTE: Most of the time you can simply add the AutoTargetingModule
    /// to your player ship.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/HUD Targets")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleHUDTargets : MonoBehaviour
    {
        #region Public Variables and Properties
        public ShipDisplayModule shipDisplayModule;
        #endregion  

        #region Private Variables
        private ShipControlModule playerShip = null;
        private bool isInitialised = false;
        private SSCRadar sscRadar = null;
        private SSCRadarQuery radarQuery = null;
        private List<SSCRadarBlip> sscRadarBlipsList = null;
        private int numDisplayTargets = 0;
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            #region Initialise Player Ship
            playerShip = GetComponent<ShipControlModule>();

            if (playerShip != null)
            {
                // If Initialise On Awake is not ticking on the ship, initialise the ship now.
                if (!playerShip.IsInitialised) { playerShip.InitialiseShip(); }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("[ERROR] SampleHUDTargets.Start() could not find the ship! Did you attach this script to you Player ship (ShipControlModule)?");
            }
            #endif
            #endregion

            #region Initialise HUD
            if (shipDisplayModule != null)
            {
                // If Initialise on Start is not ticked on the HUD, do it now.
                if (!shipDisplayModule.IsInitialised) { shipDisplayModule.Initialise(); }

                if (shipDisplayModule.IsInitialised)
                {
                    // Typically you can let the HUD update DisplayTarget positions on the screen
                    // automatically. However, in this sample we want to do things ourselves.
                    shipDisplayModule.autoUpdateTargetPositions = false;

                    numDisplayTargets = shipDisplayModule.GetNumberDisplayTargets;

                    // If no Display Targets are pre-configured, let's add some
                    // Typically, you will configure them in the HUD before runtime.
                    if (numDisplayTargets < 1)
                    {
                        int numAvailableReticles = shipDisplayModule.GetNumberDisplayReticles;

                        int numAdded = 0;

                        // Use the last 3 available reticles as Display Target reticles
                        for (int rIdx = numAvailableReticles - 1; rIdx >= 0; rIdx--)
                        {
                            int guidHash = shipDisplayModule.GetDisplayReticleGuidHash(rIdx);
                            shipDisplayModule.AddTarget(guidHash);
                            if (++numAdded == 3) { break; }
                        }

                        numDisplayTargets = shipDisplayModule.GetNumberDisplayTargets;
                    }
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("[ERROR] SampleHUDTargets.Start() the ShipDisplayModule is not set. Add the Prefabs/Visuals/HUD1 prefab to the scene, then drag it on to this script which should be attached to your player ship.");
            }
            #endif
            #endregion

            #region Create some "enemy" and added them to radar
            sscRadar = SSCRadar.GetOrCreateRadar();

            if (sscRadar != null && playerShip != null && playerShip.IsInitialised)
            {
                // Create a position 100m in front of the ship
                Vector3 locationWS = playerShip.shipInstance.TransformPosition + (playerShip.shipInstance.TransformForward * 100f);

                // Create some "enemy"
                for (int eIdx = 0; eIdx < 4; eIdx++)
                {
                    GameObject enemyGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                    if (enemyGO != null)
                    {
                        enemyGO.name = "Enemy " + (eIdx + 1).ToString();
                        enemyGO.transform.position = locationWS + (playerShip.shipInstance.TransformRight * 25 * (eIdx+1));
                        enemyGO.transform.localScale = Vector3.one * 3f;
                        enemyGO.GetComponent<Renderer>().material.color = new Color(1f, 1f / (eIdx + 2), 1f / (eIdx + 2));

                        // Let's get notified when our "enemy" is hit
                        DamageReceiver damageReceiver = enemyGO.AddComponent<DamageReceiver>();
                        damageReceiver.callbackOnHit = TakeDamage;

                        // Tell the radar system about this enemy gameobject
                        sscRadar.EnableRadar(enemyGO, enemyGO.transform.position, playerShip.shipInstance.factionId + 1, -1, 0, 5);                    
                    }
                }

                // Create a radar query with some default settings
                radarQuery = new SSCRadarQuery()
                {
                    range = 1000,
                    querySortOrder = SSCRadarQuery.QuerySortOrder.DistanceAsc3D,
                    factionsToExclude = new int[] { playerShip.shipInstance.factionId }
                };

                // Create an empty list to store the radar results
                sscRadarBlipsList = new List<SSCRadarBlip>(10);

                isInitialised = shipDisplayModule != null && sscRadarBlipsList != null && radarQuery != null;
            }
            #endregion
        }

        #endregion

        #region Update Methods
        // Update is called once per frame
        void Update()
        {
            if (isInitialised)
            {
                // Test code to allow you to move it around in the scene view at runtime
                //GameObject enGO = GameObject.Find("Enemy 1");

                //if (enGO != null)
                //{
                //    sscRadar.SetItemPosition(sscRadar.GetRadarItemIndexByHash(enGO.GetHashCode()), enGO.transform.position);
                //}

                sscRadar.GetRadarResults(radarQuery, sscRadarBlipsList);
                int numEnemy = sscRadarBlipsList.Count;

                Vector2 screenResolution = shipDisplayModule.ScreenResolution;

                int nextBlipIndex = 0;
                int nextWeaponIndex = 0;

                for (int dtIdx = 0; dtIdx < numDisplayTargets; dtIdx++)
                {
                    DisplayTarget displayTarget = shipDisplayModule.GetDisplayTargetByIndex(dtIdx);

                    // Are there any more enemies that could be in view?
                    if (dtIdx < numEnemy && nextBlipIndex >= 0)
                    {
                        // Find the next enemy in view
                        nextBlipIndex = sscRadar.GetNextBlipInView(sscRadarBlipsList, nextBlipIndex, shipDisplayModule.mainCamera, screenResolution);

                        // Is the enemy in view?
                        if (nextBlipIndex >= 0)
                        {
                            SSCRadarBlip blip = sscRadarBlipsList[nextBlipIndex];

                            // Move the DisplayTarget slot to the correct position on the screen
                            // For this sample we're just using 1 DisplayTarget slot (or 1 copy of each DT in the DT list in the HUD)
                            // We are ignoring the Max Number of Targets setting for each DisplayTarget.
                            shipDisplayModule.SetDisplayTargetPosition(displayTarget, 0, blip.wsPosition);

                            // Find a weapon to assign to the target
                            if (nextWeaponIndex >= 0)
                            {
                                nextWeaponIndex = playerShip.shipInstance.GetNextAutoTargetingWeaponIndex(nextWeaponIndex);

                                // Found an available weapon with a guided projectile
                                if (nextWeaponIndex >= 0)
                                {
                                    // This just one possible way you could do things. You could also investigate using other methods like
                                    // shipDisplayModule.AssignDisplayTargetSlot(..) like is used in the AutoTargetingModule.

                                    if (sscRadar.IsShipBlip(blip))
                                    {
                                        playerShip.shipInstance.SetWeaponTargetShip(nextWeaponIndex, blip.shipControlModule);
                                    }
                                    else if (sscRadar.IsGameObjectBlip(blip))
                                    {
                                        playerShip.shipInstance.SetWeaponTarget(nextWeaponIndex, blip.itemGameObject);
                                    }
                                    else if (nextBlipIndex >= numEnemy - 1)
                                    {
                                        // No more Ship or GameObject enemy so unassign remaining auto targeting weapons
                                        UnassignWeapons(ref nextWeaponIndex);

                                        // Go to the next DisplayTarget which will be turned off
                                        continue;
                                    }
                                    else
                                    {
                                        // find another enemy target
                                        nextBlipIndex++;
                                        continue;
                                    }

                                    // Advance to the next weapon
                                    nextWeaponIndex++;
                                }
                            }

                            // If the DisplayTarget is not already visible, show it
                            if (!displayTarget.showTarget) { shipDisplayModule.ShowDisplayTarget(displayTarget); }

                            // Advance to the next enemy target
                            nextBlipIndex++;
                        }
                        else
                        {
                            // If there are no more enemy, the remaining weapons (if any) should not have targets.
                            UnassignWeapons(ref nextWeaponIndex);

                            // If the DisplayTarget is visible, turn it off
                            if (displayTarget.showTarget)
                            {
                                shipDisplayModule.HideDisplayTarget(displayTarget);
                            }
                        }
                    }
                    else
                    {
                        // If there are no more enemy, the remaining weapons (if any) should not have targets.
                        UnassignWeapons(ref nextWeaponIndex);

                        // If the DisplayTarget is visible, turn it off
                        if (displayTarget.showTarget)
                        {
                            shipDisplayModule.HideDisplayTarget(displayTarget);
                        }
                    }
                }
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Unassign any remaining Auto Targeting weapons that could not be allocated a target.
        /// </summary>
        /// <param name="nextWeaponIndex"></param>
        private void UnassignWeapons(ref int nextWeaponIndex)
        {
            int numWeapons = playerShip.NumberOfWeapons;
            while (nextWeaponIndex >= 0 && nextWeaponIndex < numWeapons)
            {
                playerShip.shipInstance.ClearWeaponTarget(nextWeaponIndex);
                nextWeaponIndex++;
                nextWeaponIndex = playerShip.shipInstance.GetNextAutoTargetingWeaponIndex(nextWeaponIndex);
            }
        }

        #endregion

        #region Public Member Methods

        /// <summary>
        /// This routine is called by Sci-Fi Ship Controller when a projectile or beam hits our enemy
        /// </summary>
        /// <param name="callbackOnObjectHitParameters"></param>
        public void TakeDamage(CallbackOnObjectHitParameters callbackOnObjectHitParameters)
        {
            // Rather than taking damage, immediately our "enemy" is hit, we will destroy it.
            if (callbackOnObjectHitParameters.hitInfo.transform != null)
            {
                // Get the radar item by using the hashcode of the GameObject
                // Then tell the radar system to stop tracking this item.
                sscRadar.RemoveItem(sscRadar.GetRadarItemIndexByHash(callbackOnObjectHitParameters.hitInfo.transform.gameObject.GetHashCode()));

                // Now we can destroy the actual gameobject
                Destroy(callbackOnObjectHitParameters.hitInfo.transform.gameObject);
            }
        }

        #endregion
    }
}
