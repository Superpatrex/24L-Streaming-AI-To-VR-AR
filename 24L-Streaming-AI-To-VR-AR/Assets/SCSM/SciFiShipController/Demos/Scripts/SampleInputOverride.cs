using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Attach a SampleInputOverride component to a player ship that has a
    /// PlayerInputModule. It demonstrates how you could override the input
    /// from an axis on the PlayerInputModule. In this case we are controlling
    /// the speed of the ship and overriding the Longitudinal input. The player
    /// now has no control over forwards input.
    /// This is only a sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own
    /// namespace.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Input Override")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleInputOverride : MonoBehaviour
    {
        #region Public Variables
        [Header("Speed in metres per second")]
        [Range(0f, 1000f)] public float targetSpeed = 20f;
        [Range(0.05f, 1f)] public float accelerationRate = 0.75f;
        #endregion

        #region Private Variables
        private PlayerInputModule playerInputModule = null;
        private ShipControlModule shipControlModule = null;
        private bool isInitialised = false;
        private ShipInput shipInput = null;
        private float currentSpeed = 0f;
        private float deltaSpeed = 0f;
        #endregion

        #region Initialisation Methods

        // Start is called before the first frame update
        void Start()
        {
            playerInputModule = GetComponent<PlayerInputModule>();
            if (playerInputModule != null && playerInputModule.IsInitialised)
            {
                shipControlModule = playerInputModule.GetShipControlModule;

                if (shipControlModule == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SampleInputOverride - could not get the ShipControlModule. Did you attach the script to the player ship?");
                    #endif
                }
                else if (!shipControlModule.IsInitialised)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SampleInputOverride - is Initialise on Awake is enabled on the Physics tab of the ShipControlModule?");
                    #endif
                }
                else
                {
                    // Override the Longitudinal (foward/backward) axis
                    playerInputModule.isLongitudinalDataDiscarded = true;
                    // Re-initialise the DiscardData shipInput settings in the PlayerInputModule
                    // This is required after one or more of the [axis]DataDiscarded values are changed at runtime.
                    playerInputModule.ReinitialiseDiscardData();

                    // Create a new instance of the shipInput which we can send each frame to the ship.
                    shipInput = new ShipInput();
                    if (shipInput != null)
                    {
                        // Start by disabling everything. This helps to future-proof the code.
                        // We'll be telling the Ship you can discard anything else that we don't enable below.
                        shipInput.DisableAllData();

                        // When we send data, we will tell the ship we'll be sending Longitudinal data only.
                        shipInput.isLongitudinalDataEnabled = true;

                        isInitialised = true;
                    }
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: SampleInputOverride - did you forget to attach to the PlayerInputModule? Also check if Initialise on Awake is enabled.");
            }
            #endif
        }

        #endregion

        #region Update Methods
        // Update is called once per frame
        void Update()
        {
            if (isInitialised && shipControlModule.ShipMovementIsEnabled())
            {
                currentSpeed = (shipControlModule.shipInstance.TransformInverseRotation * shipControlModule.shipInstance.WorldVelocity).z;

                deltaSpeed = targetSpeed - currentSpeed;

                // slow the approach acceleration as we get near the target speed
                if (deltaSpeed == 0f) { shipInput.longitudinal = 0f; }
                else if (targetSpeed != 0f)
                {
                    shipInput.longitudinal = Mathf.Clamp(deltaSpeed / (targetSpeed * 0.2f), -1f, 1f) * accelerationRate;
                }
                else
                {
                    // Add 1 to avoid divO
                    shipInput.longitudinal = Mathf.Clamp((deltaSpeed+1f) / ((targetSpeed + 1f) * 0.2f), -1f, 1f) * accelerationRate;
                }

                // Tell the ship we want only apply longitudinal input
                shipControlModule.SendInput(shipInput);
            }
        }
        #endregion
    }
}