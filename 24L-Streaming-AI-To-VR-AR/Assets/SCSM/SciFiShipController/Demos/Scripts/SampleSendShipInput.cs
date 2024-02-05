using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Attach a SampleSendShipInput component to a player ship that does
    /// NOT use the PlayerInputModule. This is an over-simplified way of sending
    /// input to a player ship without using the PlayerInputModule.
    /// This is only sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own
    /// namespace.
    /// For PC and Console typically you will use the PlayerInputModule
    /// with one of the Input Modes.
    /// e.g. Keyboard, Legacy or New Unity Input System, Rewired, Occulus, Vive VR etc
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Send Ship Input")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleSendShipInput : MonoBehaviour
    {
        #region Public Variables

        [Header("Sensitivty and Gravity")]
        [Range(0.01f, 10f)] public float longitudinalAxisSensitivity = 3f;
        [Range(0.01f, 10f)] public float longitudinalAxisGravity = 3f;

        [Range(0.01f, 10f)] public float pitchAxisSensitivity = 3f;
        [Range(0f, 10f)] public float pitchAxisGravity = 3f;

        [Range(0.01f, 10f)] public float yawAxisSensitivity = 3f;
        [Range(0.01f, 10f)] public float yawAxisGravity = 3f;

        [Range(0.01f, 10f)] public float rollAxisSensitivity = 3f;
        [Range(0.01f, 10f)] public float rollAxisGravity = 3f;
        #endregion

        #region Private Variables
        private ShipControlModule shipControlModule = null;
        private ShipInput shipInput = null;
        private ShipInput lastShipInput;
        private bool isInitialised = false;
        #endregion

        #region Initialisation Methods

        // Start is called before the first frame update
        void Start()
        {
            shipControlModule = GetComponent<ShipControlModule>();

            if (shipControlModule != null)
            {
                // If Initialise On Awake is not ticking on the ship, initialise the ship now.
                if (!shipControlModule.IsInitialised) { shipControlModule.InitialiseShip(); }

                if (shipControlModule.IsInitialised)
                {
                    // Create a new instance of the shipInput which we can send each frame to the ship.
                    shipInput = new ShipInput();

                    // Create a new instance of the shipInput which will be used with sensitivity and input gravity
                    lastShipInput = new ShipInput();

                    isInitialised = shipInput != null;
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("[ERROR] SampleSendShipInput.Start() could not find the ship! Did you attach this script to a ShipControlModule?");
            }
            #endif

            // Our sample uses the legacy input system to get keyboard input.
            // However, you can use any input system you like e.g. Touch screen input from a mobile or a controller etc.
            #if !ENABLE_LEGACY_INPUT_MANAGER && UNITY_2019_2_OR_NEWER
            Debug.LogWarning("ERROR: SampleSendShipInput - This sample uses keyboard input from Legacy Input System which is NOT enabled in this project.");
            #endif
        }

        #endregion

        #region Update Methods
        // Update is called once per frame
        void Update()
        {
            if (isInitialised && shipControlModule.ShipIsEnabled())
            {
                // This sample uses simple keyboard input from the legacy Unity Input System.
                // Here is where you'd get your input from your target devices.
                // For example touch screen on a mobile or accelerometer etc.
                
                // Reset input axes
                shipInput.horizontal = 0f;
                shipInput.vertical = 0f;
                shipInput.longitudinal = 0f;
                shipInput.pitch = 0f;
                shipInput.yaw = 0f;
                shipInput.roll = 0f;
                shipInput.dock = false;
            
                #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER             
                // You may wish to lerp towards a value over several frames or modify
                // you ship's pitch/yaw/roll/turn acceleration on the Physics tab.

                // Forward and Back
                if (Input.GetKey(KeyCode.UpArrow)) { shipInput.longitudinal += 1f; }
                if (Input.GetKey(KeyCode.DownArrow)) { shipInput.longitudinal -= 1f; }

                // Take into consideration input sensitivity and input gravity settings
                shipInput.horizontal = PlayerInputModule.CalculateAxisInput(lastShipInput.horizontal, shipInput.horizontal, longitudinalAxisSensitivity, longitudinalAxisGravity);
                // Store the calculated ship input from this frame for reference next frame
                lastShipInput.longitudinal = shipInput.longitudinal;

                // Pitch up and down
                if (Input.GetKey(KeyCode.W)) { shipInput.pitch += 1f; }
                if (Input.GetKey(KeyCode.S)) { shipInput.pitch -= 1f; }

                // Take into consideration input sensitivity and input gravity settings
                shipInput.pitch = PlayerInputModule.CalculateAxisInput(lastShipInput.pitch, shipInput.pitch, pitchAxisSensitivity, pitchAxisGravity);
                // Store the calculated ship input from this frame for reference next frame
                lastShipInput.pitch = shipInput.pitch;

                // Left and Right
                if (Input.GetKey(KeyCode.RightArrow)) { shipInput.yaw += 1f; }
                if (Input.GetKey(KeyCode.LeftArrow)) { shipInput.yaw -= 1f; }

                // Take into consideration input sensitivity and input gravity settings
                shipInput.yaw = PlayerInputModule.CalculateAxisInput(lastShipInput.yaw, shipInput.yaw, yawAxisSensitivity, yawAxisGravity);
                // Store the calculated ship input from this frame for reference next frame
                lastShipInput.yaw = shipInput.yaw;

                // Roll left and right
                if (Input.GetKey(KeyCode.D)) { shipInput.roll += 1f; }
                if (Input.GetKey(KeyCode.A)) { shipInput.roll -= 1f; }

                // Take into consideration input sensitivity and input gravity settings
                shipInput.roll = PlayerInputModule.CalculateAxisInput(lastShipInput.roll, shipInput.roll, yawAxisSensitivity, yawAxisGravity);
                // Store the calculated ship input from this frame for reference next frame
                lastShipInput.roll = shipInput.roll;

                // Keep firing when the space bar is held down
                shipInput.primaryFire = Input.GetKey(KeyCode.Space);
                #endif

                shipControlModule.SendInput(shipInput);
            }
        }
        #endregion
    }
}
