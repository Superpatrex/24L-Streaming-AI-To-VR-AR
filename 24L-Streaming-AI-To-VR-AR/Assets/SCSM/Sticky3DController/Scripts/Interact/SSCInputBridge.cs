using UnityEngine;

#if SCSM_SSC
using SciFiShipController;
#endif

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This component allows you to take input from a Readable Interactive-enabled object,
    /// like a lever or joystick, and send it to a Sci-Fi Ship Controller ship.
    /// It bridges a gap between Sticky3D and Sci-Fi Ship Controller assets.
    /// See the "SSC Input Bridge" chapter in the manual for more details.
    /// Setup:
    /// 1. Add a readable interactive lever or joystick to your SSC ship.
    ///    See Demos\Prefabs\Props\S3D_Lever1
    /// 2. Attach this component to a lever or joystick on the same (typically child)
    ///    gameobject as the StickyInteractive component.
    /// 3. Tick "InitialiseOnStart" or call Initialise() from your code.
    /// 4. Drag your Sci-Fi Ship Controller ship or craft into the shipGameObject or
    ///    configure it in your game code by calling SetShip()
    /// 5. Configure the InputAxis. e.g., InputAxisX to Longitudinal
    /// 6. On the StickyInteractive object, add a new "On Readable Value Changed" event
    ///    drag the Lever gameobject (with contains the StickyInteractive and SSCInputBridge
    ///    components) into the event and select SSCInputBridge.OnInputChanged for the Function
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Utilities/SSC Input Bridge")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class SSCInputBridge : MonoBehaviour
    {
        #region Enumerations

        public enum InputAxis
        {
            None = 0,
            Horizontal = 1,
            Vertical = 2,
            Longitudinal = 3,
            Pitch = 4,
            Yaw = 5,
            Roll = 6
        }

        #endregion

        #region Public Variables
        public bool initialiseOnStart = false;

        #endregion

        #region Public Properties

        public bool IsInitialised { get { return false; } }

        #endregion

        #region Public Static Variables

        public static int InputAxisNoneInt = (int)InputAxis.None;
        public static int InputAxisHorzInt = (int)InputAxis.Horizontal;
        public static int InputAxisVertInt = (int)InputAxis.Vertical;
        public static int InputAxisLongInt = (int)InputAxis.Longitudinal;
        public static int InputAxisPitchInt = (int)InputAxis.Pitch;
        public static int InputAxisYawInt = (int)InputAxis.Yaw;
        public static int InputAxisRollInt = (int)InputAxis.Roll;

        #endregion

        #region Private and Protected Variables - Serialized

        /// <summary>
        /// Interactive Readable left-right data, to be send to the input axis of the Sci-Fi Ship Controller player ship
        /// </summary>
        [SerializeField] protected InputAxis inputAxisX = InputAxis.None;

        /// <summary>
        /// Interactive Readable forward-back data, to be send to the input axis of the Sci-Fi Ship Controller player ship
        /// </summary>
        [SerializeField] protected InputAxis inputAxisZ = InputAxis.None;

        /// <summary>
        /// The player ship from your scene which should contain a ShipControlModule on the same gameobject.
        /// </summary>
        [SerializeField] protected GameObject shipGameObject = null;

        #endregion

        #region Private Variables - General

        protected bool isInitialised = false;
        protected bool isPlayerInputAttached = false;
        protected bool isConfigured = false;
        protected int inputAxisXInt = InputAxisNoneInt;
        protected int inputAxisZInt = InputAxisNoneInt;

        #if SCSM_SSC
        protected ShipControlModule shipControlModule = null;
        protected PlayerInputModule playerInputModule = null;
        protected ShipInput shipInput = null;
        #endif

        #endregion

        #region Private Initialise Methods

        // Use this for initialization
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Protected and Internal Methods - General

        protected void CheckPlayerInputModule()
        {
            #if SCSM_SSC
            isPlayerInputAttached = shipControlModule != null && shipControlModule.TryGetComponent(out playerInputModule) ? true : false;
            #else
            isPlayerInputAttached = false;
            #endif
        }

        /// <summary>
        /// If configured, send input data to the ship
        /// </summary>
        /// <param name="inputDataValue"></param>
        protected void SendDataToShip (Vector3 inputDataValue)
        {
            #if SCSM_SSC

            if (isConfigured && shipControlModule != null && !shipControlModule.IsRespawning)
            {
                // Set the input data to the correct shipInput for the left-right interactive-enabled object readable value
                if (inputAxisXInt == InputAxisNoneInt) { }
                else if (inputAxisXInt == InputAxisHorzInt) { shipInput.horizontal = inputDataValue.x; }
                else if (inputAxisXInt == InputAxisVertInt) { shipInput.vertical = inputDataValue.x; }
                else if (inputAxisXInt == InputAxisLongInt) { shipInput.longitudinal = inputDataValue.x; }
                else if (inputAxisXInt == InputAxisPitchInt) { shipInput.pitch = -inputDataValue.x; }
                else if (inputAxisXInt == InputAxisYawInt) { shipInput.yaw = inputDataValue.x; }
                else if (inputAxisXInt == InputAxisRollInt) { shipInput.roll = inputDataValue.x; }

                // Set the input data to the correct shipInput for the forward-back interactive-enabled object readable value
                if (inputAxisZInt == InputAxisNoneInt) { }
                else if (inputAxisZInt == InputAxisHorzInt) { shipInput.horizontal = inputDataValue.z; }
                else if (inputAxisZInt == InputAxisVertInt) { shipInput.vertical = inputDataValue.z; }
                else if (inputAxisZInt == InputAxisLongInt) { shipInput.longitudinal = inputDataValue.z; }
                else if (inputAxisZInt == InputAxisPitchInt) { shipInput.pitch = -inputDataValue.z; }
                else if (inputAxisZInt == InputAxisYawInt) { shipInput.yaw = inputDataValue.z; }
                else if (inputAxisZInt == InputAxisRollInt) { shipInput.roll = inputDataValue.z; }

                shipControlModule.SendInput(shipInput);
            }

            #endif
        }

        #endregion

        #region Events

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Initialise this component. Either set Initialise on Start in the inspector OR call this from your game code.
        /// Has no effect if already initialised
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            if (!S3DUtils.IsSSCAvailable)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SSCInputBridge - Sci-Fi Ship Controller does not seem to be installed in this project.");
                #endif
            }
            else
            {
                isInitialised = true;

                // Attempt to set the ship
                SetShip(shipGameObject);
            }
        }

        /// <summary>
        /// Attempt to reconfigure the ship for receiving input.
        /// Currently we can only do this once, and cannot change the overrides at runtime unless
        /// we were to reset them all - which could cause issues with other input override scripts.
        /// </summary>
        public virtual void ReconfigureAxisData()
        {
            isConfigured = false;

            #if SCSM_SSC

            if (isInitialised && isPlayerInputAttached && playerInputModule != null)
            {
                if (shipInput == null) { shipInput = new ShipInput(); }
                else
                {
                    // Reset input values
                    shipInput.horizontal = 0f;
                    shipInput.vertical = 0f;
                    shipInput.longitudinal = 0f;
                    shipInput.pitch = 0f;
                    shipInput.yaw = 0f;
                    shipInput.roll = 0f;
                    shipInput.primaryFire = false;
                    shipInput.secondaryFire = false;
                    shipInput.dock = false;
                }

                inputAxisXInt = (int)inputAxisX;
                inputAxisZInt = (int)inputAxisZ;

                // Start by disabling everything. This helps to future-proof the code.
                // We'll be telling the Ship you can discard anything else that we don't enable below.
                shipInput.DisableAllData();

                // Override input axis if required
                // When we send data, we will tell the ship which data we'll be sending
                if (inputAxisXInt != InputAxisNoneInt)
                {
                    switch (inputAxisX)
                    {
                        case InputAxis.Horizontal:
                            playerInputModule.isHorizontalDataDiscarded = true;
                            shipInput.isHorizontalDataEnabled = true;
                            break;
                        case InputAxis.Vertical:
                            playerInputModule.isVerticalDataDiscarded = true;
                            shipInput.isVerticalDataEnabled = true;
                            break;
                        case InputAxis.Longitudinal:
                            playerInputModule.isLongitudinalDataDiscarded = true;
                            shipInput.isLongitudinalDataEnabled = true;
                            break;
                        case InputAxis.Pitch:
                            playerInputModule.isPitchDataDiscarded = true;
                            shipInput.isPitchDataEnabled = true;
                            break;
                        case InputAxis.Yaw:
                            playerInputModule.isYawDataDiscarded = true;
                            shipInput.isYawDataEnabled = true;
                            break;
                        case InputAxis.Roll:
                            playerInputModule.isRollDataDiscarded = true;
                            shipInput.isRollDataEnabled = true;
                            break;
                    }
                }

                // Override input axis if required
                // When we send data, we will tell the ship which data we'll be sending
                if (inputAxisZInt != InputAxisNoneInt && inputAxisZInt != inputAxisXInt)
                {
                    switch (inputAxisZ)
                    {
                        case InputAxis.Horizontal:
                            playerInputModule.isHorizontalDataDiscarded = true;
                            shipInput.isHorizontalDataEnabled = true;
                            break;
                        case InputAxis.Vertical:
                            playerInputModule.isVerticalDataDiscarded = true;
                            shipInput.isVerticalDataEnabled = true;
                            break;
                        case InputAxis.Longitudinal:
                            playerInputModule.isLongitudinalDataDiscarded = true;
                            shipInput.isLongitudinalDataEnabled = true;
                            break;
                        case InputAxis.Pitch:
                            playerInputModule.isPitchDataDiscarded = true;
                            shipInput.isPitchDataEnabled = true;
                            break;
                        case InputAxis.Yaw:
                            playerInputModule.isYawDataDiscarded = true;
                            shipInput.isYawDataEnabled = true;
                            break;
                        case InputAxis.Roll:
                            playerInputModule.isRollDataDiscarded = true;
                            shipInput.isRollDataEnabled = true;
                            break;
                    }
                }

                // Re-initialise the DiscardData shipInput settings in the PlayerInputModule
                // This is required after one or more of the [axis]DataDiscarded values are changed at runtime.
                playerInputModule.ReinitialiseDiscardData();

                isConfigured = true;
            }

            #endif
        }

        /// <summary>
        /// Set the player ship from your scene which should contain a ShipControlModule on the same gameobject.
        /// </summary>
        /// <param name="newShipGameObject"></param>
        public void SetShip (GameObject newShipGameObject)
        {
            if (newShipGameObject == null)
            {
                #if SCSM_SSC
                shipControlModule = null;
                playerInputModule = null;
                #endif

                shipGameObject = null;
            }
            #if SCSM_SSC
            else
            {
                shipGameObject = newShipGameObject;

                if (shipGameObject.TryGetComponent(out shipControlModule))
                {
                    SetShip(shipControlModule);
                }
            }
            #endif
        }


        #if SCSM_SSC

        /// <summary>
        /// Set the Sci-Fi Ship Controller player ship or spacecraft from the scene
        /// </summary>
        /// <param name="newShipControlModule"></param>
        public void SetShip (ShipControlModule newShipControlModule)
        {
            shipControlModule = newShipControlModule;

            if (shipControlModule == null)
            {
                playerInputModule = null;
            }
            else
            {
                CheckPlayerInputModule();
                ReconfigureAxisData();
            }
        }

        #endif


        #endregion


       #region Public Callback Methods

        /// <summary>
        /// This is automatically called by Sticky3D when you hook it up to the lever's OnReadableValueChanged event
        /// on the interactive-enabled object.
        /// </summary>
        /// <param name="stickyInteractiveID"></param>
        /// <param name="currentValue"></param>
        /// <param name="previousValue"></param>
        /// <param name="notused"></param>
        public void OnInputChanged (int stickyInteractiveID, Vector3 currentValue, Vector3 previousValue, Vector3 notused)
        {
            if (isInitialised)
            {
                SendDataToShip(currentValue);
            }
        }

        #endregion

    }
}