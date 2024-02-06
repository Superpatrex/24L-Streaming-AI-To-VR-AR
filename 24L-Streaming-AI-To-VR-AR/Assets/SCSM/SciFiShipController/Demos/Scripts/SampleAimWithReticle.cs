using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This is used with the ShipDisplayModule to aim fixed weapons where the
    /// active display reticle is pointing.
    /// Attach this script to a gameobject in the scene.
    /// NOTE: It is not physically accurate as the fixed weapon on your model
    /// will still be pointing in its original direction. e.g. straight ahead.
    /// This is only sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own
    /// namespace.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Aim With Reticle")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleAimWithReticle : MonoBehaviour
    {
        #region Public Variables
        [Header("HUD from the scene")]
        public ShipDisplayModule shipDisplayModule = null;
        [Header("Player ship from the scene")]
        public ShipControlModule shipControlModule = null;
        [Header("Weapon")]
        [Range(1,50)] public int weaponNumber = 1;

        #endregion

        #region Private Variables
        private bool isInitialised = false;
        private Weapon weapon = null;

        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            if (shipDisplayModule != null)
            {
                if (shipControlModule != null)
                {
                    int numWeapons = shipControlModule.shipInstance == null ? 0 : shipControlModule.shipInstance.NumberOfWeapons;

                    if (numWeapons >= weaponNumber)
                    {
                        weapon = shipControlModule.shipInstance.GetWeaponByIndex(weaponNumber - 1);
                        isInitialised = weapon != null;
                    }
                    #if UNITY_EDITOR
                    else
                    {
                        Debug.LogWarning("SampleAimWithReticle - You need to select a valid weapon number. Check the Combat tab on " + shipControlModule.name);
                    }
                    #endif 
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("SampleAimWithReticle - shipControlModule is not set. Did you forget to add the player ship in the inspector?");
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("SampleAimWithReticle - shipDisplayModule is not set in the inspector.");
            }
            #endif
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            // Notice that the Ship Display Module may have been initialised AFTER Start() ran in this script.
            if (isInitialised && shipDisplayModule.IsInitialised && shipDisplayModule.lockDisplayReticleToCursor && shipDisplayModule.IsDisplayReticleShown)
            {
                // In your game code, you may wish to pre-initialise or cache some of these values
                if (shipControlModule.IsInitialised && !shipControlModule.shipInstance.Destroyed())
                {
                    // the Vector3.z is automatically set to 0 when the vector2 is implicitly converted to a vector3.
                    Ray ray = shipDisplayModule.mainCamera.ViewportPointToRay(shipDisplayModule.DisplayReticleViewportPoint);

                    // Uncomment to see in scene view
                    //Debug.DrawRay(shipControlModule.shipInstance.TransformPosition, ray.direction.normalized * weapon.estimatedRange, Color.red);
              
                    // This uses the weapon's max range but your game code could use an alternative distance if required
                    // You could also do a physics raycast to see if there are any enemy within your sights
                    shipControlModule.shipInstance.SetWeaponFireDirection(weaponNumber - 1, shipControlModule.shipInstance.TransformPosition + (ray.direction.normalized * weapon.estimatedRange));
                }
            }
        }

        #endregion
    }
}