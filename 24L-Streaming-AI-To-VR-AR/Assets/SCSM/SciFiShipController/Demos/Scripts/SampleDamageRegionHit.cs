using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Sample code to show how you can get notified when a localised damage region
    /// on a ship is hit by a projectile or (laser) beam.
    /// NOTE:
    /// This is only sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own
    /// namespace.
    /// SETUP:
    /// 1. Add player ship from Prefabs\Ships to the scene (ones that don't have NPC in the name)
    /// 2. Ensure the player ship has at least one weapon
    /// 3. Remove or disable the default Main Camera
    /// 4. Add Prefabs\Environment\PlayerCamera prefab to the scene
    /// 5. Hook up the player ship to the PlayerCamera
    /// 6. Add NPC ship from Prefabs\Ships and make sure it is initialised
    /// 7. On the Combat tab of the NPC ship, set the damage mode to Localised
    /// 8. On the NPC ship, configure at least one localised damage region
    /// 9. Add this script to gameobject of the NPC ship
    /// 10. Enter the name of the damage region you want to check
    /// 11. Play the scene and attack the NPC ship
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Damage Region Hit")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleDamageRegionHit : MonoBehaviour
    {
        #region Public Variables and Properties
        public string damageRegionName = string.Empty;
        #endregion

        #region Private variables
        private ShipControlModule shipControlModule;
        private DamageRegion damageRegion;
        #endregion

        #region Initialise Methods
        // Start is called before the first frame update
        void Start()
        {
            // Get the ShipControlModule component
            shipControlModule = gameObject.GetComponent<ShipControlModule>();

            if (shipControlModule != null)
            {
                // Make sure the NPC ship is already initialised
                if (shipControlModule.IsInitialised)
                {
                    if (shipControlModule.shipInstance.shipDamageModel == Ship.ShipDamageModel.Localised)
                    {
                        // Find the damage region on the NPC ship
                        int drIdx = shipControlModule.shipInstance.GetDamageRegionIndexByName(damageRegionName);

                        if (drIdx >= 0)
                        {
                            damageRegion = shipControlModule.shipInstance.GetDamageRegionByIndex(drIdx);

                            // Assign the callback so we get notified.
                            if (shipControlModule.callbackOnHit == null)
                            {
                                shipControlModule.callbackOnHit = ShipHit;
                            }
                            #if UNITY_EDITOR
                            else { Debug.LogWarning("SampleDamageRegionHit.Start - callbackOnHit is already assigned to " + shipControlModule.callbackOnHit.Target + "." + shipControlModule.callbackOnHit.Method.Name + "(..)"); }
                            #endif
                        }
                        #if UNITY_EDITOR
                        else { Debug.LogWarning("SampleDamageRegionHit.Start - could not find damage region (" + (string.IsNullOrEmpty(damageRegionName) ? "no name" : damageRegionName) + ") on " + shipControlModule.name); }
                        #endif
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("SampleDamageRegionHit.Start - ship damage model is not Localised. Check the Combat tab"); }
                    #endif
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("SampleDamageRegionHit.Start - ship " + shipControlModule.name + " is not initialised."); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("SampleDamageRegionHit.Start - could not find the ShipControlModule. Did you attache this to the same gameobject?"); }
            #endif
        }
        #endregion

        #region Public Methods
        
        /// <summary>
        /// This is automatically called by the ship when it is hit by a projectile or (laser) beam
        /// </summary>
        /// <param name="callbackOnShipHitParameters"></param>
        public void ShipHit(CallbackOnShipHitParameters callbackOnShipHitParameters)
        {
            Vector3 hitPoint = callbackOnShipHitParameters.hitInfo.point;
            bool isDamageRegionHit = shipControlModule.shipInstance.IsPointInDamageRegion(damageRegion, callbackOnShipHitParameters.hitInfo.point);

            Debug.Log("SampleDamageRegionHit " + damageRegion.name + " was " + (isDamageRegionHit ? "" : "not ") + "hit at worldspace " + hitPoint);
        }

        #endregion
    }
}
