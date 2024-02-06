using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if VIU_PLUGIN
using HTC.UnityPlugin.Vive;
#endif

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [AddComponentMenu("Sci-Fi Ship Controller/Ship Components/Player Input Module")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    [RequireComponent(typeof(ShipControlModule))]
    [DisallowMultipleComponent]
    public class PlayerInputModule : MonoBehaviour
    {
        #region Enumerations

        public enum InputMode
        {
            DirectKeyboard = 0,
            LegacyUnity = 10,
            UnityInputSystem = 20,
            OculusAPI = 23,
            Rewired = 30,
            Vive = 80,
            UnityXR = 85
        }

        public enum InputAxisMode
        {
            NoInput = 0,
            SingleAxis = 10,
            CombinedAxis = 20
        }
   
        public enum OculusInputType
        {
            Button = 0,
            Axis1D = 1,
            Axis2D = 2,
            Pose = 3
        }

        public enum ViveInputType
        {
            Button = 0,
            Axis = 1,
            Pose = 2
        }

        // Vive defines each of these as enums.
        // e.g. BodyRole is an enumeration, DeviceRole is an enumeration etc.
        public enum ViveRoleType
        {
            BodyRole = 0,
            DeviceRole = 1,
            HandRole = 2,
            TrackerRole = 3
        }

        // Typically used with a Head Mounted Display (HMD) 
        public enum VRPoseRotation
        {
            RotationX = 0,
            RotationY = 1,
            RotationZ = 2
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a reference to the input being set from the PlayerInputModule to the ShipControlMode
        /// </summary>
        public ShipInput GetShipInput { get { return shipInput; } }

        /// <summary>
        /// Get a reference to the ShipControlModule that this PlayerInputModule will send input data to.
        /// </summary>
        public ShipControlModule GetShipControlModule { get { return shipControlModule; } }

        /// <summary>
        /// Is the Player Input Module initialised and ready for use?
        /// </summary>
        public bool IsInitialised { get; private set; }

        /// <summary>
        /// Is the PlayerInputModule currently enabled to send input to the ship?
        /// See EnableInput() and DisableInput(..)
        /// </summary>
        public bool IsInputEnabled { get { return isInputEnabled; } }

        /// <summary>
        /// Is all input except CustomerPlayerInputs ignored?
        /// See EnableInput() and DisableInput(..)
        /// </summary>
        public bool IsCustomPlayerInputOnlyEnabled { get { return isCustomPlayerInputsOnlyEnabled; } }

        /// <summary>
        /// Is the ship currently getting movement input from the ShipAIInputModule rather than PlayerInputModule?
        /// </summary>
        public bool IsShipAIModeEnabled { get { return isShipAIModeEnabled; } }

        #endregion

        #region Public Variables and properties

        public InputMode inputMode = InputMode.DirectKeyboard;

        /// <summary>
        /// The ship will attempt to maintain the same forward speed based on the last player longitudinal input [Default: OFF]
        /// </summary>
        public bool isAutoCruiseEnabled = false;

        /// <summary>
        /// If enabled, Initialise() will be called as soon as Awake() runs. This should be disabled if you want to control
        /// when the Player Input Module is enabled through code. You can also use with EnableInput().
        /// </summary>
        public bool initialiseOnAwake = true;

        /// <summary>
        /// Is input enabled when the module is first initialised?  See also EnableInput() and DisableInput().
        /// </summary>
        public bool isEnabledOnInitialise = true;

        /// <summary>
        /// Are only the custom player inputs enabled when the module is first initialised? See also DisableInput(..).
        /// </summary>
        public bool isCustomInputsOnlyOnInitialise = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool allowRepaint = false;

        #region Direct Keyboard Input
        public KeyCode horizontalInputPositiveKeycode = KeyCode.None;
        public KeyCode horizontalInputNegativeKeycode = KeyCode.None;
        public KeyCode verticalInputPositiveKeycode = KeyCode.None;
        public KeyCode verticalInputNegativeKeycode = KeyCode.None;
        public KeyCode longitudinalInputPositiveKeycode = KeyCode.UpArrow;
        public KeyCode longitudinalInputNegativeKeycode = KeyCode.DownArrow;
        public KeyCode pitchInputPositiveKeycode = KeyCode.W;
        public KeyCode pitchInputNegativeKeycode = KeyCode.S;
        public KeyCode yawInputPositiveKeycode = KeyCode.D;
        public KeyCode yawInputNegativeKeycode = KeyCode.A;
        public KeyCode rollInputPositiveKeycode = KeyCode.RightArrow;
        public KeyCode rollInputNegativeKeycode = KeyCode.LeftArrow;
        public KeyCode primaryFireKeycode = KeyCode.Mouse0;
        public KeyCode secondaryFireKeycode = KeyCode.Mouse1;
        public KeyCode dockingKeycode = KeyCode.None;

        public bool isMouseForPitchEnabledDKI = false;
        public bool isMouseForYawEnabledDKI = false;

        // Currently only used with Mouse
        [Range(0.1f, 1.0f)] public float pitchSensitivityDKI = 0.5f;
        [Range(0.1f, 1.0f)] public float yawSensitivityDKI = 0.5f;
        [Range(0f, 0.25f)] public float pitchDeadzoneDKI = 0.0125f;
        [Range(0f, 0.25f)] public float yawDeadzoneDKI = 0.01f;

        #endregion

        #region Common input variables
        public InputAxisMode horizontalInputAxisMode = InputAxisMode.NoInput;
        public InputAxisMode verticalInputAxisMode = InputAxisMode.NoInput;
        public InputAxisMode longitudinalInputAxisMode = InputAxisMode.SingleAxis;
        public InputAxisMode pitchInputAxisMode = InputAxisMode.SingleAxis;
        public InputAxisMode yawInputAxisMode = InputAxisMode.SingleAxis;
        public InputAxisMode rollInputAxisMode = InputAxisMode.SingleAxis;
        // Used with Occulus, Vive, Rewired, Unity Input System
        public bool primaryFireButtonEnabled = false;
        public bool secondaryFireButtonEnabled = false;
        public bool dockingButtonEnabled = false;

        /// <summary>
        /// Used for force feedback or vibration. Typically there are 2 motors on a gamepad.
        /// This value is used for the left low frequency motor when present.
        /// Call ReinitialiseRumble() after changing this value at runtime.
        /// </summary>
        [Range(0f,1f)] public float maxRumble1 = 0f;

        /// <summary>
        /// Used for force feedback or vibration. Typically there are 2 motors on a gamepad.
        /// This value is used for the right high frequency motor when present.
        /// Call ReinitialiseRumble() after changing this value at runtime.
        /// </summary>
        [Range(0f,1f)] public float maxRumble2 = 0f;

        #endregion

        #region Common input sensitivity variables
        public bool isSensitivityForHorizontalEnabled = false;
        public bool isSensitivityForVerticalEnabled = false;
        public bool isSensitivityForLongitudinalEnabled = false;
        public bool isSensitivityForPitchEnabled = false;
        public bool isSensitivityForYawEnabled = false;
        public bool isSensitivityForRollEnabled = false;
        [Range(0.01f, 10f)] public float horizontalAxisSensitivity = 3f;
        [Range(0.01f, 10f)] public float verticalAxisSensitivity = 3f;
        [Range(0.01f, 10f)] public float longitudinalAxisSensitivity = 3f;
        [Range(0.01f, 10f)] public float pitchAxisSensitivity = 3f;
        [Range(0.01f, 10f)] public float yawAxisSensitivity = 3f;
        [Range(0.01f, 10f)] public float rollAxisSensitivity = 3f;
        [Range(0.01f, 10f)] public float horizontalAxisGravity = 3f;
        [Range(0.01f, 10f)] public float verticalAxisGravity = 3f;
        [Range(0.01f, 10f)] public float longitudinalAxisGravity = 3f;
        [Range(0.01f, 10f)] public float pitchAxisGravity = 3f;
        [Range(0.01f, 10f)] public float yawAxisGravity = 3f;
        [Range(0.01f, 10f)] public float rollAxisGravity = 3f;
        #endregion

        #region Common Input Invert variables
        public bool invertHorizontalInputAxis = false;
        public bool invertVerticalInputAxis = false;
        public bool invertLongitudinalInputAxis = false;
        public bool invertPitchInputAxis = false;
        public bool invertYawInputAxis = false;
        public bool invertRollInputAxis = false;
        #endregion

        #region Common Data Discard variables (Advanced)
        /// <summary>
        /// Should we use or discard data from the horizontal axis?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isHorizontalDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the vertical axis?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isVerticalDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the longitudinal axis?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isLongitudinalDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the pitch axis?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isPitchDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the yaw axis?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isYawDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the roll axis?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isRollDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the primaryFire button?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isPrimaryFireDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the secondaryFire button?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isSecondaryFireDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the dock button?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isDockDataDiscarded;
        #endregion

        #region Legacy Unity Input System
        public string positiveHorizontalInputAxisName = "Horizontal";
        public string negativeHorizontalInputAxisName = "Horizontal";
        public string positiveVerticalInputAxisName = "Vertical";
        public string negativeVerticalInputAxisName = "Vertical";
        public string positiveLongitudinalInputAxisName = "Vertical";
        public string negativeLongitudinalInputAxisName = "Vertical";
        public string positivePitchInputAxisName = "Vertical";
        public string negativePitchInputAxisName = "Vertical";
        public string positiveYawInputAxisName = "Horizontal";
        public string negativeYawInputAxisName = "Horizontal";
        public string positiveRollInputAxisName = "Horizontal";
        public string negativeRollInputAxisName = "Horizontal";
        public string primaryFireInputAxisName = "Fire1";
        public string secondaryFireInputAxisName = "Fire2";
        public string dockingInputAxisName = "Dock";
        public bool isPositiveHorizontalInputAxisValid = true;
        public bool isNegativeHorizontalInputAxisValid = true;
        public bool isPositiveVerticalInputAxisValid = true;
        public bool isNegativeVerticalInputAxisValid = true;
        public bool isPositiveLongitudinalInputAxisValid = true;
        public bool isNegativeLongitudinalInputAxisValid = true;
        public bool isPositivePitchInputAxisValid = true;
        public bool isNegativePitchInputAxisValid = true;
        public bool isPositiveYawInputAxisValid = true;
        public bool isNegativeYawInputAxisValid = true;
        public bool isPositiveRollInputAxisValid = true;
        public bool isNegativeRollInputAxisValid = true;
        public bool isPrimaryFireInputAxisValid = true;
        public bool isSecondaryFireInputAxisValid = true;
        public bool isDockingInputAxisValid = true;

        public bool isMouseForPitchEnabledLIS = false;
        public bool isMouseForYawEnabledLIS = false;

        // Currently only used with Mouse
        [Range(0.1f, 1.0f)] public float pitchSensitivityLIS = 0.5f;
        [Range(0.1f, 1.0f)] public float yawSensitivityLIS = 0.5f;
        [Range(0f, 0.25f)] public float pitchDeadzoneLIS = 0.05f;
        [Range(0f, 0.25f)] public float yawDeadzoneLIS = 0.05f;
        #endregion

        #region Oculus OVR API input
        public bool isOVRInputUpdatedIfRequired = false;
        /// <summary>
        /// Oculus API Input Type 0 = Button, 1 = Axis1D, 2 = Axis2D
        /// </summary>
        public OculusInputType ovrHorizontalInputType = OculusInputType.Axis1D;
        public OculusInputType ovrVerticalInputType = OculusInputType.Axis1D;
        public OculusInputType ovrLongitudinalInputType = OculusInputType.Axis1D;
        public OculusInputType ovrPitchInputType = OculusInputType.Axis1D;
        public OculusInputType ovrYawInputType = OculusInputType.Axis1D;
        public OculusInputType ovrRollInputType = OculusInputType.Axis1D;
        // primary and secondary fire and docking will always be Button input type.
        // Store as Int so can be included when OVR is not installed (0 = None in OVRInput)
        public int positiveHorizontalInputOVR = 0;
        public int negativeHorizontalInputOVR = 0;
        public int positiveVerticalInputOVR = 0;
        public int negativeVerticalInputOVR = 0;
        public int positiveLongitudinalInputOVR = 0;
        public int negativeLongitudinalInputOVR = 0;
        public int positivePitchInputOVR = 0;
        public int negativePitchInputOVR = 0;
        public int positiveYawInputOVR = 0;
        public int negativeYawInputOVR = 0;
        public int positiveRollInputOVR = 0;
        public int negativeRollInputOVR = 0;
        public int primaryFireInputOVR = 0;
        public int secondaryFireInputOVR = 0;
        public int dockingInputOVR = 0;
        #endregion

        #region Unity Input System
        // Unity Input System stores actionId internally as string rather than System.Guid
        // We don't require positive and negative as Input System supports Composite Bindings
        public string positiveHorizontalInputActionIdUIS = "";
        public string positiveVerticalInputActionIdUIS = "";
        public string positiveLongitudinalInputActionIdUIS = "";
        public string positivePitchInputActionIdUIS = "";
        public string positiveYawInputActionIdUIS = "";
        public string positiveRollInputActionIdUIS = "";
        public string primaryFireInputActionIdUIS = "";
        public string secondaryFireInputActionIdUIS = "";
        public string dockingInputActionIdUIS = "";
        // quit is a special action that is added with Add Quit onscreen controls
        public string quitInputActionIdUIS = "QuitApp";
        // An Action can return a Control Type with or or more values.
        // e.g. bool, float, Vector2, Vector3. The SSC zero-based DataSlot indicates which value to use.
        // For composite bindings e.g. 2DVector/Dpad, the slot matches the axis.
        // Left/Right = x-axis = slot 1, Up/Down = y-axis = slot 2.
        public int positiveHorizontalInputActionDataSlotUIS = 0;
        public int positiveVerticalInputActionDataSlotUIS = 0;
        public int positiveLongitudinalInputActionDataSlotUIS = 0;
        public int positivePitchInputActionDataSlotUIS = 0;
        public int positiveYawInputActionDataSlotUIS = 0;
        public int positiveRollInputActionDataSlotUIS = 0;
        public int primaryFireInputActionDataSlotUIS = 0;
        public int secondaryFireInputActionDataSlotUIS = 0;
        public int dockingInputActionDataSlotUIS = 0;

        public bool isMouseForPitchEnabledUIS = false;
        public bool isMouseForYawEnabledUIS = false;

        // Currently only used with Mouse
        [Range(0.1f, 1.0f)] public float pitchSensitivityUIS = 0.5f;
        [Range(0.1f, 1.0f)] public float yawSensitivityUIS = 0.5f;
        [Range(0f, 0.25f)] public float pitchDeadzoneUIS = 0.05f;
        [Range(0f, 0.25f)] public float yawDeadzoneUIS = 0.05f;

        public delegate void CallbackOnQuitGame(ShipControlModule shipControlModule);

        /// <summary>
        /// The name of the custom method that is called immediately
        /// after user presses button to quit the game. Your method
        /// must take 1 parameter of class ShipControlModule.
        /// NOTE: Currently only works with Unity Input System.
        /// </summary>
        public CallbackOnQuitGame callbackOnQuitGame = null;

        #endregion

        #region Rewired Input
        public int rewiredPlayerNumber = 0; // Human player number 1+ (0 = no player assigned)
        // Action Ids are more efficient than using Action Friendly Names
        public int positiveHorizontalInputActionId = -1;
        public int negativeHorizontalInputActionId = -1;
        public int positiveVerticalInputActionId = -1;
        public int negativeVerticalInputActionId = -1;
        public int positiveLongitudinalInputActionId = -1;
        public int negativeLongitudinalInputActionId = -1;
        public int positivePitchInputActionId = -1;
        public int negativePitchInputActionId = -1;
        public int positiveYawInputActionId = -1;
        public int negativeYawInputActionId = -1;
        public int positiveRollInputActionId = -1;
        public int negativeRollInputActionId = -1;
        public int primaryFireInputActionId = -1;
        public int secondaryFireInputActionId = -1;
        public int dockingInputActionId = -1;
        #endregion

        #region VIVE Input variables
        public ViveInputType viveHorizontalInputType = ViveInputType.Axis;
        public ViveInputType viveVerticalInputType = ViveInputType.Axis;
        public ViveInputType viveLongitudinalInputType = ViveInputType.Axis;
        public ViveInputType vivePitchInputType = ViveInputType.Axis;
        public ViveInputType viveYawInputType = ViveInputType.Axis;
        public ViveInputType viveRollInputType = ViveInputType.Axis;

        // Inputs can be from a Device, Hand, Tracker or Body role
        // These are then broken down into their enum elements
        public ViveRoleType viveHorizontalRoleType = ViveRoleType.HandRole;
        public ViveRoleType viveVerticalRoleType = ViveRoleType.HandRole;
        public ViveRoleType viveLongitudinalRoleType = ViveRoleType.HandRole;
        public ViveRoleType vivePitchRoleType = ViveRoleType.HandRole;
        public ViveRoleType viveYawRoleType = ViveRoleType.HandRole;
        public ViveRoleType viveRollRoleType = ViveRoleType.HandRole;
        public ViveRoleType vivePrimaryFireRoleType = ViveRoleType.HandRole;
        public ViveRoleType viveSecondaryFireRoleType = ViveRoleType.HandRole;
        public ViveRoleType viveDockingRoleType = ViveRoleType.HandRole;

        // Each Role Type has an associated sub-category from an Enum called
        // rolevalue, named here the RoleId as an integer so that it can be stored
        // here even if Vive Input Utility is not installed.
        public int positiveHorizontalInputRoleId = 0;
        public int negativeHorizontalInputRoleId = 0;
        public int positiveVerticalInputRoleId = 0;
        public int negativeVerticalInputRoleId = 0;
        public int positiveLongitudinalInputRoleId = 0;
        public int negativeLongitudinalInputRoleId = 0;
        public int positivePitchInputRoleId = 0;
        public int negativePitchInputRoleId = 0;
        public int positiveYawInputRoleId = 0;
        public int negativeYawInputRoleId = 0;
        public int positiveRollInputRoleId = 0;
        public int negativeRollInputRoleId = 0;
        public int primaryFireInputRoleId = 0;
        public int secondaryFireInputRoleId = 0;
        public int dockingInputRoleId = 0;

        // Store as Int so can be included when Vive is not installed (0 = None in ViveInput)
        // This is the ControllerAxis, ControllerButton, or Pose Rotation enum value.
        public int positiveHorizontalInputCtrlVive = 0;
        public int negativeHorizontalInputCtrlVive = 0;
        public int positiveVerticalInputCtrlVive = 0;
        public int negativeVerticalInputCtrlVive = 0;
        public int positiveLongitudinalInputCtrlVive = 0;
        public int negativeLongitudinalInputCtrlVive = 0;
        public int positivePitchInputCtrlVive = 0;
        public int negativePitchInputCtrlVive = 0;
        public int positiveYawInputCtrlVive = 0;
        public int negativeYawInputCtrlVive = 0;
        public int positiveRollInputCtrlVive = 0;
        public int negativeRollInputCtrlVive = 0;
        public int primaryFireInputCtrlVive = 0;
        public int secondaryFireInputCtrlVive = 0;
        public int dockingInputCtrlVive = 0;
        #endregion

        #region UnityXR

        // Unity Input System (which XR uses) stores actionMapId internally as string rather than System.Guid
        // We don't require positive and negative as Input System supports Composite Bindings
        public string positiveHorizontalInputActionMapIdXR = "";
        public string positiveVerticalInputActionMapIdXR = "";
        public string positiveLongitudinalInputActionMapIdXR = "";
        public string positivePitchInputActionMapIdXR = "";
        public string positiveYawInputActionMapIdXR = "";
        public string positiveRollInputActionMapIdXR = "";
        public string primaryFireInputActionMapIdXR = "";
        public string secondaryFireInputActionMapIdXR = "";
        public string dockingInputActionMapIdXR = "";
        #endregion UnityXR

        #region Custom Inputs
        public List<CustomPlayerInput> customPlayerInputList;
        public bool isCustomPlayerInputListExpanded;
        #endregion

        #region Editor variables
        // Whether the input sections are shown as expanded in the inspector window of the editor.
        public bool horizontalShowInEditor;
        public bool verticalShowInEditor;
        public bool longitudinalShowInEditor;
        public bool pitchShowInEditor;
        public bool yawShowInEditor;
        public bool rollShowInEditor;
        public bool primaryFireShowInEditor;
        public bool secondaryFireShowInEditor;
        public bool dockingShowInEditor;
        public bool rumbleShowInEditor;
        public bool onscreenControlsShowInEditor;
        public bool customPlayerInputsShowInEditor;
        #endregion

        // General
        public bool primaryFireCanBeHeld = true;
        public bool secondaryFireCanBeHeld = true;

        #endregion

        #region Private Variables

        private ShipControlModule shipControlModule;
        private ShipInput shipInput;
        private ShipInput lastShipInput;
        private float targetCruiseSpeed = 0f;
        private float currentSpeed = 0f;

        // Currently only used by legacy input with mouse
        private float targetDisplayOffsetX = 0f;

        // Input is enabled by default after Initialise() runs.
        private bool isInputEnabled = false;

        /// <summary>
        /// This is true when everything except CustomPlayerInputs are disabled.
        /// See DisableInput(..) and EnableInput()
        /// </summary>
        private bool isCustomPlayerInputsOnlyEnabled = false;

        private int numberOfCustomPlayerInputs = 0;

        /// <summary>
        /// Has the ship been switched to Ship AI Input Module control?
        /// </summary>
        private bool isShipAIModeEnabled = false;

        #if SSC_OVR
        // See also CheckOVRManager()
        private bool isOVRInputUpdateRequired = false;
        private bool isOVRManagerPresent = false;
        private float poseRotation = 0f;
        #endif

        #if SSC_REWIRED
        private Rewired.Player rewiredPlayer = null;

        // Local variables that get populated at runtime
        // Use int rather than enum for speed
        // 0 = Axis, 1 = Button
        // See UpdateRewiredActionTypes()
        private int positiveHorizontalInputActionType = 0;
        private int negativeHorizontalInputActionType = 0;
        private int positiveVerticalInputActionType = 0;
        private int negativeVerticalInputActionType = 0;
        private int positiveLongitudinalInputActionType = 0;
        private int negativeLongitudinalInputActionType = 0;
        private int positivePitchInputActionType = 0;
        private int negativePitchInputActionType = 0;
        private int positiveYawInputActionType = 0;
        private int negativeYawInputActionType = 0;
        private int positiveRollInputActionType = 0;
        private int negativeRollInputActionType = 0;
        private int primaryFireInputActionType = 0;
        private int secondaryFireInputActionType = 0;
        private int dockingInputActionType = 0;
        #endif

        #if SSC_UIS
        // Ref to a Player Input component that should be attached to the same gameobject
        private UnityEngine.InputSystem.PlayerInput uisPlayerInput = null;
        // Used to map positive[inputtype]InputActionIdUIS to InputAction at runtime
        // We don't require positive and negative as Input System supports Composite Bindings
        // See UpdateUnityInputSystemActions()
        private UnityEngine.InputSystem.InputAction positiveHorizontalInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction positiveVerticalInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction positiveLongitudinalInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction positivePitchInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction positiveYawInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction positiveRollInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction primaryFireInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction secondaryFireInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction quitInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction dockingInputActionUIS = null;

        // InputAction Control Types can be Axis, Vector2/Dpad, Vector3 + others.
        // They are stored in inputAction.expectedControlType as a string. To avoid GC
        // in Update, convert them to a hashed int32 value.
        private int positiveHorizontalCtrlTypeHashUIS = 0;
        private int positiveVerticalCtrlTypeHashUIS = 0;
        private int positiveLongitudinalCtrlTypeHashUIS = 0;
        private int positivePitchCtrlTypeHashUIS = 0;
        private int positiveYawCtrlTypeHashUIS = 0;
        private int positiveRollCtrlTypeHashUIS = 0;
        private int primaryFireCtrlTypeHashUIS = 0;
        private int secondaryFireCtrlTypeHashUIS = 0;
        private int dockingCtrlTypeHashUIS = 0;

        private int controlTypeButtonHashUIS = 0;
        private int controlTypeAxisHashUIS = 0;
        private int controlTypeVector2HashUIS = 0;
        private int controlTypeDpadHashUIS = 0;
        private int controlTypeVector3HashUIS = 0;
        #endif

        #if VIU_PLUGIN
        // Used for mapping the enum vive[..]RoleTypes to SystemTypes at runtime
        // See UpdateViveInput(..) method.
        private System.Type viveHorizontalRoleSystemType = null;
        private System.Type viveVerticalRoleSystemType = null;
        private System.Type viveLongitudinalRoleSystemType = null;
        private System.Type vivePitchRoleSystemType = null;
        private System.Type viveYawRoleSystemType = null;
        private System.Type viveRollRoleSystemType = null;
        private System.Type vivePrimaryFireRoleSystemType = null;
        private System.Type viveSecondaryFireRoleSystemType = null;
        private System.Type viveDockingRoleSystemType = null;

        // Used for mapping positiveHorizontalInputCtrlVive integer to ControllerAxis enum at runtime
        private ControllerAxis positiveHorizontalInputCtrlAxisVive = ControllerAxis.None;
        private ControllerAxis negativeHorizontalInputCtrlAxisVive = ControllerAxis.None;
        private ControllerAxis positiveVerticalInputCtrlAxisVive = ControllerAxis.None;
        private ControllerAxis negativeVerticalInputCtrlAxisVive = ControllerAxis.None;
        private ControllerAxis positiveLongitudinalInputCtrlAxisVive = ControllerAxis.None;
        private ControllerAxis negativeLongitudinalInputCtrlAxisVive = ControllerAxis.None;
        private ControllerAxis positivePitchInputCtrlAxisVive = ControllerAxis.None;
        private ControllerAxis negativePitchInputCtrlAxisVive = ControllerAxis.None;
        private ControllerAxis positiveYawInputCtrlAxisVive = ControllerAxis.None;
        private ControllerAxis negativeYawInputCtrlAxisVive = ControllerAxis.None;
        private ControllerAxis positiveRollInputCtrlAxisVive = ControllerAxis.None;
        private ControllerAxis negativeRollInputCtrlAxisVive = ControllerAxis.None;

        // Used for mapping positiveHorizontalInputCtrlVive integer to ControllerButton enum at runtime
        private ControllerButton positiveHorizontalInputCtrlBtnVive = ControllerButton.None;
        private ControllerButton negativeHorizontalInputCtrlBtnVive = ControllerButton.None;
        private ControllerButton positiveVerticalInputCtrlBtnVive = ControllerButton.None;
        private ControllerButton negativeVerticalInputCtrlBtnVive = ControllerButton.None;
        private ControllerButton positiveLongitudinalInputCtrlBtnVive = ControllerButton.None;
        private ControllerButton negativeLongitudinalInputCtrlBtnVive = ControllerButton.None;
        private ControllerButton positivePitchInputCtrlBtnVive = ControllerButton.None;
        private ControllerButton negativePitchInputCtrlBtnVive = ControllerButton.None;
        private ControllerButton positiveYawInputCtrlBtnVive = ControllerButton.None;
        private ControllerButton negativeYawInputCtrlBtnVive = ControllerButton.None;
        private ControllerButton positiveRollInputCtrlBtnVive = ControllerButton.None;
        private ControllerButton negativeRollInputCtrlBtnVive = ControllerButton.None;
        private ControllerButton primaryFireInputCtrlBtnVive = ControllerButton.None;
        private ControllerButton secondaryFireInputCtrlBtnVive = ControllerButton.None;
        private ControllerButton dockingInputCtrlBtnVive = ControllerButton.None;
        #endif

        #if SCSM_XR && SSC_UIS
        /// <summary>
        /// The UIS scriptable object that contains the action maps and input actions.
        /// </summary>
        [SerializeField] private UnityEngine.InputSystem.InputActionAsset inputActionAssetXR = null;

        /// <summary>
        /// The main XR first person camera which is a child of the ship controller
        /// </summary>
        [SerializeField] private Camera firstPersonCamera1XR = null;

        /// <summary>
        /// The transform of the XR first person camera 
        /// </summary>
        [SerializeField] private Transform firstPersonTransform1XR = null;

        /// <summary>
        /// The transform of the XR left hand
        /// </summary>
        [SerializeField] private Transform leftHandTransformXR = null;

        /// <summary>
        /// The transform of the XR right hand
        /// </summary>
        [SerializeField] private Transform rightHandTransformXR = null;
        #endif

        #endregion

        #region Initalisation Methods

        // Use this for initialization
        void Awake()
        {
            if (initialiseOnAwake) { Initialise(); }
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            if (!isInputEnabled && !isCustomPlayerInputsOnlyEnabled) { return; }

            if (inputMode == InputMode.DirectKeyboard)
            {
                #region Direct Keyboard Input
                
                if (isInputEnabled)
                {
                    // Reset input axes
                    shipInput.horizontal = 0f;
                    shipInput.vertical = 0f;
                    shipInput.longitudinal = 0f;
                    shipInput.pitch = 0f;
                    shipInput.yaw = 0f;
                    shipInput.roll = 0f;
                    shipInput.dock = false;

                    #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER

                    if (Input.GetKey(horizontalInputPositiveKeycode)) { shipInput.horizontal += 1f; }
                    if (Input.GetKey(horizontalInputNegativeKeycode)) { shipInput.horizontal -= 1f; }

                    if (Input.GetKey(verticalInputPositiveKeycode)) { shipInput.vertical += 1f; }
                    if (Input.GetKey(verticalInputNegativeKeycode)) { shipInput.vertical -= 1f; }

                    if (Input.GetKey(longitudinalInputPositiveKeycode)) { shipInput.longitudinal += 1f; }
                    if (Input.GetKey(longitudinalInputNegativeKeycode)) { shipInput.longitudinal -= 1f; }

                    if (isMouseForPitchEnabledDKI)
                    {
                        shipInput.pitch = CalculateMouseYInput(Input.mousePosition.y, pitchSensitivityDKI, pitchDeadzoneDKI);
                    }
                    else
                    {
                        if (Input.GetKey(pitchInputPositiveKeycode)) { shipInput.pitch += 1f; }
                        if (Input.GetKey(pitchInputNegativeKeycode)) { shipInput.pitch -= 1f; }
                    }

                    if (isMouseForYawEnabledDKI)
                    {
                        shipInput.yaw = CalculateMouseXInput(Input.mousePosition.x, yawSensitivityDKI, yawDeadzoneDKI);
                    }
                    else
                    {
                        if (Input.GetKey(yawInputPositiveKeycode)) { shipInput.yaw += 1f; }
                        if (Input.GetKey(yawInputNegativeKeycode)) { shipInput.yaw -= 1f; }
                    }

                    if (Input.GetKey(rollInputPositiveKeycode)) { shipInput.roll += 1f; }
                    if (Input.GetKey(rollInputNegativeKeycode)) { shipInput.roll -= 1f; }

                    if (primaryFireCanBeHeld) { shipInput.primaryFire = Input.GetKey(primaryFireKeycode); }
                    else { shipInput.primaryFire = Input.GetKeyDown(primaryFireKeycode); }

                    if (secondaryFireCanBeHeld) { shipInput.secondaryFire = Input.GetKey(secondaryFireKeycode); }
                    else { shipInput.secondaryFire = Input.GetKeyDown(secondaryFireKeycode); }

                    shipInput.dock = Input.GetKeyDown(dockingKeycode);
                    #endif
                }

                #region Custom Player Input
                #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
                if (numberOfCustomPlayerInputs > 0)
                {
                    for (int cpiIdx = 0; cpiIdx < numberOfCustomPlayerInputs; cpiIdx++)
                    {
                        CustomPlayerInput customPlayerInput = customPlayerInputList[cpiIdx];

                        if (customPlayerInput != null && customPlayerInput.customPlayerInputEvt != null)
                        {
                            // CustomPlayerInput.CustomPlayerInputEventType is Key (10).
                            if (customPlayerInput.canBeHeldDown)
                            {
                                if (Input.GetKey(customPlayerInput.dkmPositiveKeycode)) { customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(1f,0f,0f), 10); }
                                if (!customPlayerInput.isButton && Input.GetKey(customPlayerInput.dkmNegativeKeycode)) { customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(-1f, 0f, 0f), 10); }
                            }
                            else
                            {
                                if (Input.GetKeyDown(customPlayerInput.dkmPositiveKeycode)) { customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(1f, 0f, 0f), 10); }
                                if (!customPlayerInput.isButton && Input.GetKeyDown(customPlayerInput.dkmNegativeKeycode)) { customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(-1f, 0f, 0f), 10); }
                            }
                        }
                    }
                }
                #endif
                #endregion Custom Player Input

                #endregion
            }
            else if (inputMode == InputMode.LegacyUnity)
            {
                #region Legacy Unity Input System

                #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER

                if (isInputEnabled)
                {
                    #region Legacy Unity Input System Horizontal Input

                    // Horizontal input
                    if (horizontalInputAxisMode != InputAxisMode.NoInput)
                    {
                        if (horizontalInputAxisMode == InputAxisMode.SingleAxis)
                        {
                            // Single axis
                            shipInput.horizontal = Input.GetAxis(positiveHorizontalInputAxisName);
                            if (invertHorizontalInputAxis) { shipInput.horizontal *= -1f; }
                        }
                        else
                        {
                            // Combined axis - two axes combined together for a single input
                            shipInput.horizontal = Input.GetAxis(positiveHorizontalInputAxisName) - Input.GetAxis(negativeHorizontalInputAxisName);
                        }
                    }
                    else { shipInput.horizontal = 0f; }

                    #endregion Legacy Unity Input System Horizontal Input

                    #region Legacy Unity Input System Vertical Input

                    // Vertical input
                    if (verticalInputAxisMode != InputAxisMode.NoInput)
                    {
                        if (verticalInputAxisMode == InputAxisMode.SingleAxis)
                        {
                            // Single axis
                            shipInput.vertical = Input.GetAxis(positiveVerticalInputAxisName);
                            if (invertVerticalInputAxis) { shipInput.vertical *= -1f; }
                        }
                        else
                        {
                            // Combined axis - two axes combined together for a single input
                            shipInput.vertical = Input.GetAxis(positiveVerticalInputAxisName) - Input.GetAxis(negativeVerticalInputAxisName);
                        }
                    }
                    else { shipInput.vertical = 0f; }

                    #endregion Legacy Unity Input System Vertical Input

                    #region Legacy Unity Input System Longitudinal Input

                    // Longitudinal input
                    if (longitudinalInputAxisMode != InputAxisMode.NoInput)
                    {
                        if (longitudinalInputAxisMode == InputAxisMode.SingleAxis)
                        {
                            // Single axis
                            shipInput.longitudinal = Input.GetAxis(positiveLongitudinalInputAxisName);
                            if (invertLongitudinalInputAxis) { shipInput.longitudinal *= -1f; }
                        }
                        else
                        {
                            // Combined axis - two axes combined together for a single input
                            shipInput.longitudinal = Input.GetAxis(positiveLongitudinalInputAxisName) - Input.GetAxis(negativeLongitudinalInputAxisName);
                        }
                    }
                    else { shipInput.longitudinal = 0f; }

                    #endregion Legacy Unity Input System Longitudinal Input

                    #region Legacy Unity Input System Pitch Input

                    // Pitch input
                    if (isMouseForPitchEnabledLIS)
                    {
                        shipInput.pitch = CalculateMouseYInput(Input.mousePosition.y, pitchSensitivityLIS, pitchDeadzoneLIS);
                    }
                    else if (pitchInputAxisMode != InputAxisMode.NoInput)
                    {
                        if (pitchInputAxisMode == InputAxisMode.SingleAxis)
                        {
                            // Single axis
                            shipInput.pitch = Input.GetAxis(positivePitchInputAxisName);
                            if (invertPitchInputAxis) { shipInput.pitch *= -1f; }
                        }
                        else
                        {
                            // Combined axis - two axes combined together for a single input
                            shipInput.pitch = Input.GetAxis(positivePitchInputAxisName) - Input.GetAxis(negativePitchInputAxisName);
                        }
                    }
                    else { shipInput.pitch = 0f; }

                    #endregion Legacy Unity Input System Pitch Input

                    #region Legacy Unity Input System Yaw Input

                    // Yaw input
                    if (isMouseForYawEnabledLIS)
                    {
                        shipInput.yaw = CalculateMouseXInput(Input.mousePosition.x, yawSensitivityLIS, yawDeadzoneLIS);
                    }
                    else if (yawInputAxisMode != InputAxisMode.NoInput)
                    {
                        if (yawInputAxisMode == InputAxisMode.SingleAxis)
                        {
                            // Single axis
                            shipInput.yaw = Input.GetAxis(positiveYawInputAxisName);
                            if (invertYawInputAxis) { shipInput.yaw *= -1f; }
                        }
                        else
                        {
                            // Combined axis - two axes combined together for a single input
                            shipInput.yaw = Input.GetAxis(positiveYawInputAxisName) - Input.GetAxis(negativeYawInputAxisName);
                        }
                    }
                    else { shipInput.yaw = 0f; }

                    #endregion Legacy Unity Input System Yaw Input

                    #region Legacy Unity Input System Roll Input

                    // Roll input
                    if (rollInputAxisMode != InputAxisMode.NoInput)
                    {
                        if (rollInputAxisMode == InputAxisMode.SingleAxis)
                        {
                            // Single axis
                            shipInput.roll = Input.GetAxis(positiveRollInputAxisName);
                            if (invertRollInputAxis) { shipInput.roll *= -1f; }
                        }
                        else
                        {
                            // Combined axis - two axes combined together for a single input
                            shipInput.roll = Input.GetAxis(positiveRollInputAxisName) - Input.GetAxis(negativeRollInputAxisName);
                        }
                    }
                    else { shipInput.roll = 0f; }

                    #endregion Legacy Unity Input System Roll Input

                    #region Legacy Unity Input System Primary Fire Input

                    if (primaryFireCanBeHeld) { shipInput.primaryFire = Input.GetButton(primaryFireInputAxisName); }
                    else { shipInput.primaryFire = Input.GetButtonDown(primaryFireInputAxisName); }

                    #endregion Legacy Unity Input System Primary Fire Input

                    #region Legacy Unity Input System Secondary Fire Input

                    if (secondaryFireCanBeHeld) { shipInput.secondaryFire = Input.GetButton(secondaryFireInputAxisName); }
                    else { shipInput.secondaryFire = Input.GetButtonDown(secondaryFireInputAxisName); }

                    #endregion Legacy Unity Input System Secondary Fire Input

                    #region Legacy Unity Input System Docking Input

                    // By default, there is no docking legacy input axis so check every time to avoid errors
                    shipInput.dock = isDockingInputAxisValid ? Input.GetButtonDown(dockingInputAxisName) : false;

                    #endregion Legacy Unity Input System Docking Input
                }

                #region Custom Player Input
                if (numberOfCustomPlayerInputs > 0)
                {
                    for (int cpiIdx = 0; cpiIdx < numberOfCustomPlayerInputs; cpiIdx++)
                    {
                        CustomPlayerInput customPlayerInput = customPlayerInputList[cpiIdx];

                        if (customPlayerInput != null && customPlayerInput.customPlayerInputEvt != null)
                        {
                            if (customPlayerInput.isButton)
                            {
                                // CustomPlayerInput.CustomPlayerInputEventType is Button (5).
                                if (customPlayerInput.canBeHeldDown && Input.GetButton(customPlayerInput.lisPositiveAxisName)) { customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(1f, 0f, 0f), 5); }
                                else if (Input.GetButtonDown(customPlayerInput.lisPositiveAxisName)) { customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(1f, 0f, 0f), 5); }
                            }
                            else if (customPlayerInput.inputAxisMode == InputAxisMode.SingleAxis)
                            {
                                Vector3 inputValue = new Vector3(Input.GetAxis(customPlayerInput.lisPositiveAxisName) * (customPlayerInput.lisInvertAxis ? -1f : 1f), 0f, 0f);
                                if (customPlayerInput.isSensitivityEnabled)
                                {
                                    inputValue.x = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValue.x, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                    customPlayerInput.lastInputValueX = inputValue.x;
                                    customPlayerInput.lastInputValueY = 0f;
                                }
                                // CustomPlayerInput.CustomPlayerInputEventType is Axis1D (1).
                                customPlayerInput.customPlayerInputEvt.Invoke(inputValue, 1);
                            }
                            else if (customPlayerInput.inputAxisMode == InputAxisMode.CombinedAxis)
                            {
                                // Combined axis - two axes combined together for a single input
                                Vector3 inputValue = new Vector3((Input.GetAxis(customPlayerInput.lisPositiveAxisName) - Input.GetAxis(customPlayerInput.lisNegativeAxisName)) * (customPlayerInput.lisInvertAxis ? -1f : 1f), 0f, 0f);
                                if (customPlayerInput.isSensitivityEnabled)
                                {
                                    inputValue.x = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValue.x, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                    customPlayerInput.lastInputValueX = inputValue.x;
                                    customPlayerInput.lastInputValueY = 0f;
                                }
                                // CustomPlayerInput.CustomPlayerInputEventType is Axis1D (1).
                                customPlayerInput.customPlayerInputEvt.Invoke(inputValue, 1);
                            }
                        }
                    }
                }
                #endregion Custom Player Input

                #else
                // Reset input axes
                shipInput.horizontal = 0f;
                shipInput.vertical = 0f;
                shipInput.longitudinal = 0f;
                shipInput.pitch = 0f;
                shipInput.yaw = 0f;
                shipInput.roll = 0f;
                shipInput.dock = false;
                #endif

                #endregion
            }
            else if (inputMode == InputMode.UnityInputSystem)
            {
                #region Unity Input System
                
                #if SSC_UIS

                // By default actions are enabled
                // InputAction needs to consider the ControlType returned.
                if (isInputEnabled)
                {
                    #region Unity Input System Horizontal Input
                    shipInput.horizontal = UISReadActionInputFloat(positiveHorizontalInputActionUIS, positiveHorizontalInputActionDataSlotUIS, positiveHorizontalCtrlTypeHashUIS);
                    if (invertHorizontalInputAxis) { shipInput.horizontal *= -1f; }
                    #endregion Unity Input System Horizontal Input

                    #region Unity Input System Vertical Input
                    shipInput.vertical = UISReadActionInputFloat(positiveVerticalInputActionUIS, positiveVerticalInputActionDataSlotUIS, positiveVerticalCtrlTypeHashUIS);
                    if (invertVerticalInputAxis) { shipInput.vertical *= -1f; }
                    #endregion Unity Input System Vertical Input

                    #region Unity Input System Longitudinal Input
                    shipInput.longitudinal = UISReadActionInputFloat(positiveLongitudinalInputActionUIS, positiveLongitudinalInputActionDataSlotUIS, positiveLongitudinalCtrlTypeHashUIS);
                    if (invertLongitudinalInputAxis) { shipInput.longitudinal *= -1f; }
                    #endregion Unity Input System Longitudinal Input

                    #region Unity Input System Pitch Input
                    if (isMouseForPitchEnabledUIS)
                    {
                        shipInput.pitch = CalculateMouseYInput(UnityEngine.InputSystem.Mouse.current.position.ReadValue().y, 
                            pitchSensitivityUIS, pitchDeadzoneUIS);
                    }
                    else
                    {
                        shipInput.pitch = UISReadActionInputFloat(positivePitchInputActionUIS, positivePitchInputActionDataSlotUIS, positivePitchCtrlTypeHashUIS);
                    }
                    if (invertPitchInputAxis) { shipInput.pitch *= -1f; }
                    #endregion Unity Input System Pitch Input

                    #region Unity Input System Yaw Input

                    if (isMouseForYawEnabledUIS)
                    {
                        shipInput.yaw = CalculateMouseXInput(UnityEngine.InputSystem.Mouse.current.position.ReadValue().x,
                            yawSensitivityUIS, yawDeadzoneUIS);
                    }
                    else
                    {
                        shipInput.yaw = UISReadActionInputFloat(positiveYawInputActionUIS, positiveYawInputActionDataSlotUIS, positiveYawCtrlTypeHashUIS);
                    }
                    if (invertYawInputAxis) { shipInput.yaw *= -1f; }
                    #endregion Unity Input System Yaw Input

                    #region Unity Input System Roll Input
                    shipInput.roll = UISReadActionInputFloat(positiveRollInputActionUIS, positiveRollInputActionDataSlotUIS, positiveRollCtrlTypeHashUIS);
                    if (invertRollInputAxis) { shipInput.roll *= -1f; }
                    #endregion Unity Input System Roll Input

                    #region Unity Input System Primary Fire Input
                    shipInput.primaryFire = UISReadActionInputBool(primaryFireInputActionUIS, primaryFireInputActionDataSlotUIS, primaryFireCtrlTypeHashUIS, primaryFireCanBeHeld);
                    #endregion  Unity Input System Primary Fire Input

                    #region Unity Input System Secondary Fire Input
                    shipInput.secondaryFire = UISReadActionInputBool(secondaryFireInputActionUIS, secondaryFireInputActionDataSlotUIS, secondaryFireCtrlTypeHashUIS, secondaryFireCanBeHeld);
                    #endregion  Unity Input System Secondary Fire Input

                    #region Unity Input System Docking Input
                    shipInput.dock = UISReadActionInputBool(dockingInputActionUIS, dockingInputActionDataSlotUIS, dockingCtrlTypeHashUIS, false);
                    #endregion

                    #region Unity Input System Quit
                    if (UISReadActionInputBool(quitInputActionUIS, 0, controlTypeButtonHashUIS, false))
                    {
                        if (callbackOnQuitGame != null) {  callbackOnQuitGame.Invoke(shipControlModule); }
                        else
                        {
                            #if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;
                            #else
                            Application.Quit();
                            #endif
                        }
                    }
                    #endregion
                }

                #region Custom Player Input
                if (numberOfCustomPlayerInputs > 0)
                {
                    for (int cpiIdx = 0; cpiIdx < numberOfCustomPlayerInputs; cpiIdx++)
                    {
                        CustomPlayerInput customPlayerInput = customPlayerInputList[cpiIdx];

                        if (customPlayerInput != null && customPlayerInput.customPlayerInputEvt != null)
                        {
                            if (customPlayerInput.isButton)
                            {
                                if (UISReadActionInputBool(customPlayerInput.uisPositiveInputAction, customPlayerInput.uisPositiveInputActionDataSlot, customPlayerInput.uisPositiveCtrlTypeHash, customPlayerInput.canBeHeldDown))
                                {
                                    // CustomPlayerInput.CustomPlayerInputEventType is Button (5).
                                    customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(1f, 0f, 0f), 5);
                                }
                            }
                            else
                            {
                                Vector3 inputValue = Vector3.zero;

                                if (customPlayerInput.uisPositiveInputAction != null)
                                {
                                    if (customPlayerInput.uisPositiveCtrlTypeHash == controlTypeAxisHashUIS || customPlayerInput.uisPositiveCtrlTypeHash == controlTypeButtonHashUIS)
                                    {
                                        // 1D axis and Buttons both retrun a float
                                        inputValue.x = customPlayerInput.uisPositiveInputAction.ReadValue<float>();

                                        if (customPlayerInput.isSensitivityEnabled)
                                        {
                                            inputValue.x = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValue.x, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                        }
                                        customPlayerInput.lastInputValueX = inputValue.x;
                                        // CustomPlayerInput.CustomPlayerInputEventType is Axis1D (1).
                                        customPlayerInput.customPlayerInputEvt.Invoke(inputValue, 1);
                                    }
                                    else if (customPlayerInput.uisPositiveCtrlTypeHash == controlTypeVector2HashUIS || customPlayerInput.uisPositiveCtrlTypeHash == controlTypeDpadHashUIS)
                                    {
                                        inputValue = customPlayerInput.uisPositiveInputAction.ReadValue<Vector2>();

                                        if (customPlayerInput.isSensitivityEnabled)
                                        {
                                            inputValue.x = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValue.x, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                            inputValue.y = CalculateAxisInput(customPlayerInput.lastInputValueY, inputValue.y, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                        }
                                        customPlayerInput.lastInputValueX = inputValue.x;
                                        customPlayerInput.lastInputValueY = inputValue.y;
                                        // CustomPlayerInput.CustomPlayerInputEventType is Axis2D (2).
                                        customPlayerInput.customPlayerInputEvt.Invoke(inputValue, 2);
                                    }
                                    else if (customPlayerInput.uisPositiveCtrlTypeHash == controlTypeVector3HashUIS)
                                    {
                                        inputValue = customPlayerInput.uisPositiveInputAction.ReadValue<Vector3>();

                                        if (customPlayerInput.isSensitivityEnabled)
                                        {
                                            // Currently only works on x,y axes
                                            inputValue.x = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValue.x, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                            inputValue.y = CalculateAxisInput(customPlayerInput.lastInputValueY, inputValue.y, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                        }
                                        customPlayerInput.lastInputValueX = inputValue.x;
                                        customPlayerInput.lastInputValueY = inputValue.y;
                                        // CustomPlayerInput.CustomPlayerInputEventType is Axis3D (3).
                                        customPlayerInput.customPlayerInputEvt.Invoke(inputValue, 3);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion Custom Player Input

                #else
                // Do nothing if Unity Input System is set but not installed in the project
                shipInput.horizontal = 0f;
                shipInput.vertical = 0f;
                shipInput.longitudinal = 0f;
                shipInput.pitch = 0f;
                shipInput.yaw = 0f;
                shipInput.roll = 0f;
                shipInput.primaryFire = false;
                shipInput.secondaryFire = false;
                shipInput.dock = false;
                #endif

                #endregion
            }
            else if (inputMode == InputMode.OculusAPI)
            {
                #region Oculus API
                #if SSC_OVR
                
                // NOTE:
                // Assumes input is from first active controller
                // We may need to allow this to be configurable

                try
                {
                    if (isOVRInputUpdateRequired) { OVRInput.Update(); }

                    if (isInputEnabled)
                    {
                        #region Oculus Horizontal Input
                        if (horizontalInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (horizontalInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                if (ovrHorizontalInputType == OculusInputType.Axis1D)
                                {
                                    shipInput.horizontal = OVRInput.Get((OVRInput.Axis1D)positiveHorizontalInputOVR);
                                }
                                else if (ovrHorizontalInputType == OculusInputType.Axis2D)
                                {
                                    // Axis2D is like a joystick. +X is left, -X is right. + Y is push forward, -Y is pull back
                                    shipInput.horizontal = OVRInput.Get((OVRInput.Axis2D)positiveHorizontalInputOVR).x;
                                }
                                else if (ovrHorizontalInputType == OculusInputType.Button)
                                {
                                    shipInput.horizontal = OVRInput.Get((OVRInput.Button)positiveHorizontalInputOVR) ? 1f : 0f;
                                }
                                else if (isOVRManagerPresent) // Pose which requires the OVRManager in the scene
                                {
                                    // positiveHorizontalInputOVR 0 = RotationX, 1 = RotationY, 2 = RotationZ
                                    if (positiveHorizontalInputOVR == 0)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.x;
                                    }
                                    else if (positiveHorizontalInputOVR == 1)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.y;
                                    }
                                    else
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.z;
                                    }

                                    // Convert to a value between -1 and 1
                                    if (poseRotation > 0) { while (poseRotation > 359.999f) { poseRotation -= 360f; } }
                                    else if (poseRotation < 0) { while (poseRotation < -359.999f) { poseRotation += 360f; } }
                                    shipInput.horizontal = poseRotation / 360f;
                                }
                                else { shipInput.horizontal = 0f; }
                            }
                            else
                            {
                                if (ovrHorizontalInputType == OculusInputType.Axis1D)
                                {
                                    shipInput.horizontal = OVRInput.Get((OVRInput.Axis1D)positiveHorizontalInputOVR) - OVRInput.Get((OVRInput.Axis1D)negativeHorizontalInputOVR);
                                }
                                // Axis2D joystick probably doesn't make much sense with CombinedAxis
                                else if (ovrHorizontalInputType == OculusInputType.Axis2D)
                                {
                                    shipInput.horizontal = OVRInput.Get((OVRInput.Axis2D)positiveHorizontalInputOVR).x - OVRInput.Get((OVRInput.Axis2D)negativeHorizontalInputOVR).x;
                                }
                                else if (ovrHorizontalInputType == OculusInputType.Button)
                                {
                                    shipInput.horizontal = (OVRInput.Get((OVRInput.Button)positiveHorizontalInputOVR) ? 1f : 0f) - (OVRInput.Get((OVRInput.Button)negativeHorizontalInputOVR) ? 1f : 0f);
                                }
                                // Combined Axis with Pose works EXACTLY the same way as Single Axis and cannot be selected in the PlayerInputModuleEditor
                                else if (isOVRManagerPresent) // Pose which requires the OVRManager in the scene
                                {
                                    // positiveHorizontalInputOVR 0 = RotationX, 1 = RotationY, 2 = RotationZ
                                    if (positiveHorizontalInputOVR == 0)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.x;
                                    }
                                    else if (positiveHorizontalInputOVR == 1)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.y;
                                    }
                                    else
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.z;
                                    }

                                    // Convert to a value between -1 and 1
                                    if (poseRotation > 0) { while (poseRotation > 359.999f) { poseRotation -= 360f; } }
                                    else if (poseRotation < 0) { while (poseRotation < -359.999f) { poseRotation += 360f; } }
                                    shipInput.horizontal = poseRotation / 360f;
                                }
                                else { shipInput.horizontal = 0f; }
                            }
                        }
                        else { shipInput.horizontal = 0f; }
                        #endregion Oculus Horizontal Input

                        #region Oculus Vertical Input
                        if (verticalInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (verticalInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                if (ovrVerticalInputType == OculusInputType.Axis1D)
                                {
                                    shipInput.vertical = OVRInput.Get((OVRInput.Axis1D)positiveVerticalInputOVR);
                                }
                                else if (ovrVerticalInputType == OculusInputType.Axis2D)
                                {
                                    // Axis2D is like a joystick. +Y is push forward, -Y is pull back
                                    shipInput.vertical = OVRInput.Get((OVRInput.Axis2D)positiveVerticalInputOVR).y;
                                }
                                else if (ovrVerticalInputType == OculusInputType.Button)
                                {
                                    shipInput.vertical = OVRInput.Get((OVRInput.Button)positiveVerticalInputOVR) ? 1f : 0f;
                                }
                                else if (isOVRManagerPresent) // Pose which requires the OVRManager in the scene
                                {
                                    // positiveVerticalInputOVR 0 = RotationX, 1 = RotationY, 2 = RotationZ
                                    if (positiveVerticalInputOVR == 0)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.x;
                                    }
                                    else if (positiveVerticalInputOVR == 1)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.y;
                                    }
                                    else
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.z;
                                    }

                                    // Convert to a value between -1 and 1
                                    if (poseRotation > 0) { while (poseRotation > 359.999f) { poseRotation -= 360f; } }
                                    else if (poseRotation < 0) { while (poseRotation < -359.999f) { poseRotation += 360f; } }
                                    shipInput.vertical = poseRotation / 360f;
                                }
                                else { shipInput.vertical = 0f; }
                            }
                            else
                            {
                                if (ovrVerticalInputType == OculusInputType.Axis1D)
                                {
                                    shipInput.vertical = OVRInput.Get((OVRInput.Axis1D)positiveVerticalInputOVR) - OVRInput.Get((OVRInput.Axis1D)negativeVerticalInputOVR);
                                }
                                // Axis2D joystick probably doesn't make much sense with CombinedAxis
                                else if (ovrVerticalInputType == OculusInputType.Axis2D)
                                {
                                    shipInput.vertical = OVRInput.Get((OVRInput.Axis2D)positiveVerticalInputOVR).y - OVRInput.Get((OVRInput.Axis2D)negativeVerticalInputOVR).y;
                                }
                                else if (ovrVerticalInputType == OculusInputType.Button)
                                {
                                    shipInput.vertical = (OVRInput.Get((OVRInput.Button)positiveVerticalInputOVR) ? 1f : 0f) - (OVRInput.Get((OVRInput.Button)negativeVerticalInputOVR) ? 1f : 0f);
                                }
                                // Combined Axis with Pose works EXACTLY the same way as Single Axis and cannot be selected in the PlayerInputModuleEditor
                                else if (isOVRManagerPresent) // Pose which requires the OVRManager in the scene
                                {
                                    // positiveVerticalInputOVR 0 = RotationX, 1 = RotationY, 2 = RotationZ
                                    if (positiveVerticalInputOVR == 0)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.x;
                                    }
                                    else if (positiveVerticalInputOVR == 1)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.y;
                                    }
                                    else
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.z;
                                    }

                                    // Convert to a value between -1 and 1
                                    if (poseRotation > 0) { while (poseRotation > 359.999f) { poseRotation -= 360f; } }
                                    else if (poseRotation < 0) { while (poseRotation < -359.999f) { poseRotation += 360f; } }
                                    shipInput.vertical = poseRotation / 360f;
                                }
                                else { shipInput.vertical = 0f; }
                            }
                        }
                        else { shipInput.vertical = 0f; }
                        #endregion Oculus Vertical Input

                        #region Oculus Longitudinal Input
                        if (longitudinalInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (longitudinalInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                if (ovrLongitudinalInputType == OculusInputType.Axis1D)
                                {
                                    shipInput.longitudinal = OVRInput.Get((OVRInput.Axis1D)positiveLongitudinalInputOVR);
                                }
                                else if (ovrLongitudinalInputType == OculusInputType.Axis2D)
                                {
                                    // Axis2D is like a joystick. +Y is push forward, -Y is pull back
                                    shipInput.longitudinal = OVRInput.Get((OVRInput.Axis2D)positiveLongitudinalInputOVR).y;
                                }
                                else if (ovrLongitudinalInputType == OculusInputType.Button)
                                {
                                    shipInput.longitudinal = OVRInput.Get((OVRInput.Button)positiveLongitudinalInputOVR) ? 1f : 0f;
                                }
                                else if (isOVRManagerPresent) // Pose which requires the OVRManager in the scene
                                {
                                    // positiveLongitudinalInputOVR 0 = RotationX, 1 = RotationY, 2 = RotationZ
                                    if (positiveLongitudinalInputOVR == 0)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.x;
                                    }
                                    else if (positiveLongitudinalInputOVR == 1)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.y;
                                    }
                                    else
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.z;
                                    }

                                    // Convert to a value between -1 and 1
                                    if (poseRotation > 0) { while (poseRotation > 359.999f) { poseRotation -= 360f; } }
                                    else if (poseRotation < 0) { while (poseRotation < -359.999f) { poseRotation += 360f; } }
                                    shipInput.longitudinal = poseRotation / 360f;
                                }
                                else { shipInput.longitudinal = 0f; }
                            }
                            else
                            {
                                if (ovrLongitudinalInputType == OculusInputType.Axis1D)
                                {
                                    shipInput.longitudinal = OVRInput.Get((OVRInput.Axis1D)positiveLongitudinalInputOVR) - OVRInput.Get((OVRInput.Axis1D)negativeLongitudinalInputOVR);
                                }
                                // Axis2D joystick probably doesn't make much sense with CombinedAxis
                                else if (ovrLongitudinalInputType == OculusInputType.Axis2D)
                                {
                                    shipInput.longitudinal = OVRInput.Get((OVRInput.Axis2D)positiveLongitudinalInputOVR).y - OVRInput.Get((OVRInput.Axis2D)negativeLongitudinalInputOVR).y;
                                }
                                else if (ovrLongitudinalInputType == OculusInputType.Button)
                                {
                                    shipInput.longitudinal = (OVRInput.Get((OVRInput.Button)positiveLongitudinalInputOVR) ? 1f : 0f) - (OVRInput.Get((OVRInput.Button)negativeLongitudinalInputOVR) ? 1f : 0f);
                                }
                                // Combined Axis with Pose works EXACTLY the same way as Single Axis and cannot be selected in the PlayerInputModuleEditor
                                else if (isOVRManagerPresent) // Pose which requires the OVRManager in the scene
                                {
                                    // positiveLongitudinalInputOVR 0 = RotationX, 1 = RotationY, 2 = RotationZ
                                    if (positiveLongitudinalInputOVR == 0)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.x;
                                    }
                                    else if (positiveLongitudinalInputOVR == 1)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.y;
                                    }
                                    else
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.z;
                                    }

                                    // Convert to a value between -1 and 1
                                    if (poseRotation > 0) { while (poseRotation > 359.999f) { poseRotation -= 360f; } }
                                    else if (poseRotation < 0) { while (poseRotation < -359.999f) { poseRotation += 360f; } }
                                    shipInput.longitudinal = poseRotation / 360f;
                                }
                                else { shipInput.longitudinal = 0f; }
                            }
                        }
                        else { shipInput.longitudinal = 0f; }
                        #endregion Oculus Longitudinal Input

                        #region Oculus Pitch Input
                        if (pitchInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (pitchInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                if (ovrPitchInputType == OculusInputType.Axis1D)
                                {
                                    shipInput.pitch = OVRInput.Get((OVRInput.Axis1D)positivePitchInputOVR);
                                }
                                else if (ovrPitchInputType == OculusInputType.Axis2D)
                                {
                                    // Axis2D is like a joystick. +Y is push forward, -Y is pull back
                                    shipInput.pitch = OVRInput.Get((OVRInput.Axis2D)positivePitchInputOVR).y;
                                }
                                else if (ovrPitchInputType == OculusInputType.Button)
                                {
                                    shipInput.pitch = OVRInput.Get((OVRInput.Button)positivePitchInputOVR) ? 1f : 0f;
                                }
                                else if (isOVRManagerPresent) // Pose which requires the OVRManager in the scene
                                {
                                    // positivePitchInputOVR 0 = RotationX, 1 = RotationY, 2 = RotationZ
                                    if (positivePitchInputOVR == 0)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.x;
                                    }
                                    else if (positivePitchInputOVR == 1)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.y;
                                    }
                                    else
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.z;
                                    }

                                    // Convert to a value between -1 and 1
                                    if (poseRotation > 0) { while (poseRotation > 359.999f) { poseRotation -= 360f; } }
                                    else if (poseRotation < 0) { while (poseRotation < -359.999f) { poseRotation += 360f; } }
                                    shipInput.pitch = poseRotation / 360f;
                                }
                                else { shipInput.pitch = 0f; }
                            }
                            else
                            {
                                if (ovrPitchInputType == OculusInputType.Axis1D)
                                {
                                    shipInput.pitch = OVRInput.Get((OVRInput.Axis1D)positivePitchInputOVR) - OVRInput.Get((OVRInput.Axis1D)negativePitchInputOVR);
                                }
                                // Axis2D joystick probably doesn't make much sense with CombinedAxis
                                else if (ovrPitchInputType == OculusInputType.Axis2D)
                                {
                                    shipInput.pitch = OVRInput.Get((OVRInput.Axis2D)positivePitchInputOVR).y - OVRInput.Get((OVRInput.Axis2D)negativePitchInputOVR).y;
                                }
                                else if (ovrPitchInputType == OculusInputType.Button)
                                {
                                    shipInput.pitch = (OVRInput.Get((OVRInput.Button)positivePitchInputOVR) ? 1f : 0f) - (OVRInput.Get((OVRInput.Button)negativePitchInputOVR) ? 1f : 0f);
                                }
                                // Combined Axis with Pose works EXACTLY the same way as Single Axis and cannot be selected in the PlayerInputModuleEditor
                                else if (isOVRManagerPresent) // Pose which requires the OVRManager in the scene
                                {
                                    // positivePitchInputOVR 0 = RotationX, 1 = RotationY, 2 = RotationZ
                                    if (positivePitchInputOVR == 0)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.x;
                                    }
                                    else if (positivePitchInputOVR == 1)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.y;
                                    }
                                    else
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.z;
                                    }

                                    // Convert to a value between -1 and 1
                                    if (poseRotation > 0) { while (poseRotation > 359.999f) { poseRotation -= 360f; } }
                                    else if (poseRotation < 0) { while (poseRotation < -359.999f) { poseRotation += 360f; } }
                                    shipInput.pitch = poseRotation / 360f;
                                }
                                else { shipInput.pitch = 0f; }
                            }
                        }
                        else { shipInput.pitch = 0f; }
                        #endregion Oculus Pitch Input

                        #region Oculus Yaw Input
                        if (yawInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (yawInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                if (ovrYawInputType == OculusInputType.Axis1D)
                                {
                                    shipInput.yaw = OVRInput.Get((OVRInput.Axis1D)positiveYawInputOVR);
                                }
                                else if (ovrYawInputType == OculusInputType.Axis2D)
                                {
                                    // Axis2D is like a joystick. +X is left, -X is right. + Y is push forward, -Y is pull back
                                    shipInput.yaw = OVRInput.Get((OVRInput.Axis2D)positiveYawInputOVR).x;
                                }
                                else if (ovrYawInputType == OculusInputType.Button)
                                {
                                    shipInput.yaw = OVRInput.Get((OVRInput.Button)positiveYawInputOVR) ? 1f : 0f;
                                }
                                else if (isOVRManagerPresent) // Pose which requires the OVRManager in the scene
                                {
                                    // positiveYawInputOVR 0 = RotationX, 1 = RotationY, 2 = RotationZ
                                    if (positiveYawInputOVR == 0)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.x;
                                    }
                                    else if (positiveYawInputOVR == 1)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.y;
                                    }
                                    else
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.z;
                                    }

                                    // Convert to a value between -1 and 1
                                    if (poseRotation > 0) { while (poseRotation > 359.999f) { poseRotation -= 360f; } }
                                    else if (poseRotation < 0) { while (poseRotation < -359.999f) { poseRotation += 360f; } }
                                    shipInput.yaw = poseRotation / 360f;
                                }
                                else { shipInput.yaw = 0f; }
                            }
                            else
                            {
                                if (ovrYawInputType == OculusInputType.Axis1D)
                                {
                                    shipInput.yaw = OVRInput.Get((OVRInput.Axis1D)positiveYawInputOVR) - OVRInput.Get((OVRInput.Axis1D)negativeYawInputOVR);
                                }
                                // Axis2D joystick probably doesn't make much sense with CombinedAxis
                                else if (ovrYawInputType == OculusInputType.Axis2D)
                                {
                                    shipInput.yaw = OVRInput.Get((OVRInput.Axis2D)positiveYawInputOVR).x - OVRInput.Get((OVRInput.Axis2D)negativeYawInputOVR).x;
                                }
                                else if (ovrYawInputType == OculusInputType.Button)
                                {
                                    shipInput.yaw = (OVRInput.Get((OVRInput.Button)positiveYawInputOVR) ? 1f : 0f) - (OVRInput.Get((OVRInput.Button)negativeYawInputOVR) ? 1f : 0f);
                                }
                                // Combined Axis with Pose works EXACTLY the same way as Single Axis and cannot be selected in the PlayerInputModuleEditor
                                else if (isOVRManagerPresent) // Pose which requires the OVRManager in the scene
                                {
                                    // positiveYawInputOVR 0 = RotationX, 1 = RotationY, 2 = RotationZ
                                    if (positiveYawInputOVR == 0)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.x;
                                    }
                                    else if (positiveYawInputOVR == 1)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.y;
                                    }
                                    else
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.z;
                                    }

                                    // Convert to a value between -1 and 1
                                    if (poseRotation > 0) { while (poseRotation > 359.999f) { poseRotation -= 360f; } }
                                    else if (poseRotation < 0) { while (poseRotation < -359.999f) { poseRotation += 360f; } }
                                    shipInput.yaw = poseRotation / 360f;
                                }
                                else { shipInput.yaw = 0f; }
                            }
                        }
                        else { shipInput.yaw = 0f; }
                        #endregion Oculus Yaw Input

                        #region Oculus Roll Input
                        if (rollInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (rollInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                if (ovrRollInputType == OculusInputType.Axis1D)
                                {
                                    shipInput.roll = OVRInput.Get((OVRInput.Axis1D)positiveRollInputOVR);
                                }
                                else if (ovrRollInputType == OculusInputType.Axis2D)
                                {
                                    // Axis2D is like a joystick. +X is left, -X is right. + Y is push forward, -Y is pull back
                                    shipInput.roll = OVRInput.Get((OVRInput.Axis2D)positiveRollInputOVR).x;
                                }
                                else if (ovrRollInputType == OculusInputType.Button)
                                {
                                    shipInput.roll = OVRInput.Get((OVRInput.Button)positiveRollInputOVR) ?  1f : 0f;
                                }
                                else if (isOVRManagerPresent) // Pose which requires the OVRManager in the scene
                                {
                                    // positiveRollInputOVR 0 = RotationX, 1 = RotationY, 2 = RotationZ
                                    if (positiveRollInputOVR == 0)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.x;
                                    }
                                    else if (positiveRollInputOVR == 1)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.y;
                                    }
                                    else
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.z;
                                    }

                                    // Convert to a value between -1 and 1
                                    if (poseRotation > 0) { while (poseRotation > 359.999f) { poseRotation -= 360f; } }
                                    else if (poseRotation < 0) { while (poseRotation < -359.999f) { poseRotation += 360f; } }
                                    shipInput.roll = poseRotation / 360f;
                                }
                                else { shipInput.roll = 0f; }
                            }
                            else
                            {
                                if (ovrRollInputType == OculusInputType.Axis1D)
                                {
                                    shipInput.roll = OVRInput.Get((OVRInput.Axis1D)positiveRollInputOVR) - OVRInput.Get((OVRInput.Axis1D)negativeRollInputOVR);
                                }
                                // Axis2D joystick probably doesn't make much sense with CombinedAxis
                                else if (ovrRollInputType == OculusInputType.Axis2D)
                                {
                                    shipInput.roll = OVRInput.Get((OVRInput.Axis2D)positiveRollInputOVR).x - OVRInput.Get((OVRInput.Axis2D)negativeRollInputOVR).x;
                                }
                                else if (ovrRollInputType == OculusInputType.Button)
                                {
                                    shipInput.roll = (OVRInput.Get((OVRInput.Button)positiveRollInputOVR) ? 1f : 0f) - (OVRInput.Get((OVRInput.Button)negativeRollInputOVR) ? 1f : 0f);
                                }
                                // Combined Axis with Pose works EXACTLY the same way as Single Axis and cannot be selected in the PlayerInputModuleEditor
                                else if (isOVRManagerPresent) // Pose which requires the OVRManager in the scene
                                {
                                    // positiveRollInputOVR 0 = RotationX, 1 = RotationY, 2 = RotationZ
                                    if (positiveRollInputOVR == 0)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.x;
                                    }
                                    else if (positiveRollInputOVR == 1)
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.y;
                                    }
                                    else
                                    {
                                        poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.z;
                                    }

                                    // Convert to a value between -1 and 1
                                    if (poseRotation > 0) { while (poseRotation > 359.999f) { poseRotation -= 360f; } }
                                    else if (poseRotation < 0) { while (poseRotation < -359.999f) { poseRotation += 360f; } }
                                    shipInput.roll = poseRotation / 360f;
                                }
                                else { shipInput.roll = 0f; }
                            }
                        }
                        else { shipInput.roll = 0f; }
                        #endregion Oculus Roll Input

                        #region Oculus Primary Fire Input
                        // There is no need to check if it is enabled, because by default it is 0 and the PlayerInputModuleEditor
                        // ensures it is 0 when not enabled. Also, it is possible to be enabled but set the input to None (0)
                        if (primaryFireInputOVR > 0)
                        {
                            if (primaryFireCanBeHeld) { shipInput.primaryFire = OVRInput.Get((OVRInput.Button)primaryFireInputOVR); }
                            else { shipInput.primaryFire = OVRInput.GetDown((OVRInput.Button)primaryFireInputOVR); }
                        }
                        else { shipInput.primaryFire = false; }

                        #endregion Oculus Primary Fire Input

                        #region Oculus Secondary Fire Input
                        // There is no need to check if it is enabled, because by default it is 0 and the PlayerInputModuleEditor
                        // ensures it is 0 when not enabled. Also, it is possible to be enabled but set the input to None (0)
                        if (secondaryFireInputOVR > 0)
                        {
                            if (primaryFireCanBeHeld) { shipInput.secondaryFire = OVRInput.Get((OVRInput.Button)secondaryFireInputOVR); }
                            else { shipInput.secondaryFire = OVRInput.GetDown((OVRInput.Button)secondaryFireInputOVR); }
                        }
                        else { shipInput.secondaryFire = false; }
                        #endregion Oculus Secondary Fire Input

                        #region Oculus Docking Input
                        // There is no need to check if it is enabled, because by default it is 0 and the PlayerInputModuleEditor
                        // ensures it is 0 when not enabled. Also, it is possible to be enabled but set the input to None (0)
                        if (dockingInputOVR > 0)
                        {
                            shipInput.dock = OVRInput.GetDown((OVRInput.Button)dockingInputOVR);
                        }
                        else { shipInput.dock = false; }
                        #endregion Oculus Docking Input
                    }

                    #region Custom Player Input
                    if (numberOfCustomPlayerInputs > 0)
                    {
                        for (int cpiIdx = 0; cpiIdx < numberOfCustomPlayerInputs; cpiIdx++)
                        {
                            CustomPlayerInput customPlayerInput = customPlayerInputList[cpiIdx];

                            if (customPlayerInput != null && customPlayerInput.customPlayerInputEvt != null)
                            {
                                if (customPlayerInput.isButton)
                                {
                                    // Is the button configured?
                                    if (customPlayerInput.ovrPositiveInput > 0)
                                    {
                                        if (customPlayerInput.canBeHeldDown)
                                        {
                                            // If the button is down, send a valve in Vector3.x of 1f.
                                            if (OVRInput.Get((OVRInput.Button)customPlayerInput.ovrPositiveInput))
                                            {
                                                customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(1f, 0f, 0f), 5);
                                            }
                                        }
                                        else if(OVRInput.GetDown((OVRInput.Button)customPlayerInput.ovrPositiveInput))
                                        {
                                            // If the button is pressed this frame, send a value in Vector3.x of 1f.
                                            customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(1f, 0f, 0f), 5);
                                        }
                                    }
                                }
                                else if (customPlayerInput.inputAxisMode != InputAxisMode.NoInput)
                                {
                                    if (customPlayerInput.inputAxisMode == InputAxisMode.SingleAxis)
                                    {
                                        if (customPlayerInput.ovrInputType == OculusInputType.Axis1D)
                                        {
                                            float inputValueF = OVRInput.Get((OVRInput.Axis1D)customPlayerInput.ovrPositiveInput);

                                            if (customPlayerInput.isSensitivityEnabled)
                                            {
                                                inputValueF = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValueF, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                            }
                                            customPlayerInput.lastInputValueX = inputValueF;

                                            customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(inputValueF, 0f, 0f), 1);
                                        }
                                        else if (customPlayerInput.ovrInputType == OculusInputType.Axis2D)
                                        {
                                            // Axis2D is like a joystick. +X is left, -X is right. + Y is push forward, -Y is pull back
                                            Vector2 inputValueV2 = OVRInput.Get((OVRInput.Axis2D)customPlayerInput.ovrPositiveInput);

                                            if (customPlayerInput.isSensitivityEnabled)
                                            {
                                                inputValueV2.x = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValueV2.x, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                                inputValueV2.y = CalculateAxisInput(customPlayerInput.lastInputValueY, inputValueV2.y, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                            }
                                            customPlayerInput.lastInputValueX = inputValueV2.x;
                                            customPlayerInput.lastInputValueY = inputValueV2.y;

                                            customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(inputValueV2.x, inputValueV2.y, 0f), 2);
                                        }
                                        else if (customPlayerInput.ovrInputType == OculusInputType.Button)
                                        {
                                            if (OVRInput.Get((OVRInput.Button)customPlayerInput.ovrPositiveInput))
                                            {
                                                customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(1f, 0f, 0f), 5);
                                            }
                                        }
                                        else if (isOVRManagerPresent) // Pose which requires the OVRManager in the scene
                                        {
                                            // ovrPositiveInput 0 = RotationX, 1 = RotationY, 2 = RotationZ
                                            if (customPlayerInput.ovrPositiveInput == 0)
                                            {
                                                poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.x;
                                            }
                                            else if (customPlayerInput.ovrPositiveInput == 1)
                                            {
                                                poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.y;
                                            }
                                            else
                                            {
                                                poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.z;
                                            }

                                            // Convert to a value between -1 and 1
                                            if (poseRotation > 0) { while (poseRotation > 359.999f) { poseRotation -= 360f; } }
                                            else if (poseRotation < 0) { while (poseRotation < -359.999f) { poseRotation += 360f; } }

                                            // We "may" want to pass this as the raw rotation angle..
                                            customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(poseRotation / 360f, 0f, 0f), 1);
                                        }
                                    }
                                    // Combined Axes
                                    else
                                    {
                                        if (customPlayerInput.ovrInputType == OculusInputType.Axis1D)
                                        {
                                            float inputValueF = OVRInput.Get((OVRInput.Axis1D)customPlayerInput.ovrPositiveInput) - OVRInput.Get((OVRInput.Axis1D)customPlayerInput.ovrNegativeInput);

                                            if (customPlayerInput.isSensitivityEnabled)
                                            {
                                                inputValueF = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValueF, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                            }
                                            customPlayerInput.lastInputValueX = inputValueF;

                                            customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(inputValueF, 0f, 0f), 1);
                                        }
                                        // Axis2D joystick probably doesn't make much sense with CombinedAxis
                                        else if (customPlayerInput.ovrInputType == OculusInputType.Axis2D)
                                        {
                                            Vector2 inputValueV2 = OVRInput.Get((OVRInput.Axis2D)customPlayerInput.ovrPositiveInput) - OVRInput.Get((OVRInput.Axis2D)customPlayerInput.ovrNegativeInput);

                                            if (customPlayerInput.isSensitivityEnabled)
                                            {
                                                inputValueV2.x = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValueV2.x, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                                inputValueV2.y = CalculateAxisInput(customPlayerInput.lastInputValueY, inputValueV2.y, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                            }
                                            customPlayerInput.lastInputValueX = inputValueV2.x;
                                            customPlayerInput.lastInputValueY = inputValueV2.y;

                                            customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(inputValueV2.x, inputValueV2.y, 0f), 2);
                                        }
                                        else if (customPlayerInput.ovrInputType == OculusInputType.Button)
                                        {
                                            float inputValueF = (OVRInput.Get((OVRInput.Button)customPlayerInput.ovrPositiveInput) ? 1f : 0f) - (OVRInput.Get((OVRInput.Button)customPlayerInput.ovrNegativeInput) ? 1f : 0f);

                                            if (inputValueF != 0f)
                                            {
                                                customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(inputValueF, 0f, 0f), 5);
                                            }
                                        }
                                        // Combined Axis with Pose works EXACTLY the same way as Single Axis and cannot be selected in the PlayerInputModuleEditor
                                        else if (isOVRManagerPresent) // Pose which requires the OVRManager in the scene
                                        {
                                            // ovrPositiveInput 0 = RotationX, 1 = RotationY, 2 = RotationZ
                                            if (customPlayerInput.ovrPositiveInput == 0)
                                            {
                                                poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.x;
                                            }
                                            else if (customPlayerInput.ovrPositiveInput == 1)
                                            {
                                                poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.y;
                                            }
                                            else
                                            {
                                                poseRotation = OVRManager.instance.headPoseRelativeOffsetRotation.z;
                                            }

                                            // Convert to a value between -1 and 1
                                            if (poseRotation > 0) { while (poseRotation > 359.999f) { poseRotation -= 360f; } }
                                            else if (poseRotation < 0) { while (poseRotation < -359.999f) { poseRotation += 360f; } }
                                            
                                            // We "may" want to pass this as the raw rotation angle..
                                            customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(poseRotation / 360f, 0f, 0f), 1);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion Custom Player Input
                }
                catch (System.Exception ex)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("PlayerInputModule Oculus API " + ex.Message);
                    #else
                    // Keep compiler happy
                    if (ex != null) { }
                    #endif
                }

                #else                
                // Do nothing if Oculus is set but not installed
                shipInput.horizontal = 0f;
                shipInput.vertical = 0f;
                shipInput.longitudinal = 0f;
                shipInput.pitch = 0f;
                shipInput.yaw = 0f;
                shipInput.roll = 0f;
                shipInput.primaryFire = false;
                shipInput.secondaryFire = false;
                shipInput.dock = false;
                #endif
                #endregion
            }
            else if (inputMode == InputMode.Rewired)
            {
                #region Rewired Input
                #if SSC_REWIRED
                
                if (rewiredPlayer != null)
                {
                    // GetAxis is safe to call with an invalid ActionId
                    // Rewired ignores -ve values and gracefully handles missing actions
                    // by raising a single warning, rather than outputting a zillion msgs.
            
                    if (isInputEnabled)
                    {
                        #region Rewired Horizontal Input
                        if (horizontalInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (horizontalInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                shipInput.horizontal = positiveHorizontalInputActionType == 1 ? (rewiredPlayer.GetButton(positiveHorizontalInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(positiveHorizontalInputActionId);
                            }
                            else
                            {
                                shipInput.horizontal = (positiveHorizontalInputActionType == 1 ? (rewiredPlayer.GetButton(positiveHorizontalInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(positiveHorizontalInputActionId)) -
                                                       (negativeHorizontalInputActionType == 1 ? (rewiredPlayer.GetButton(negativeHorizontalInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(negativeHorizontalInputActionId));
                            }
                        }
                        else { shipInput.horizontal = 0f; }
                        #endregion Rewired Horizontal Input

                        #region Rewired Vertical Input
                        if (verticalInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (verticalInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                shipInput.vertical = positiveVerticalInputActionType == 1 ? (rewiredPlayer.GetButton(positiveVerticalInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(positiveVerticalInputActionId);
                            }
                            else
                            {
                                shipInput.vertical = (positiveVerticalInputActionType == 1 ? (rewiredPlayer.GetButton(positiveVerticalInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(positiveVerticalInputActionId)) -
                                                     (negativeVerticalInputActionType == 1 ? (rewiredPlayer.GetButton(negativeVerticalInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(negativeVerticalInputActionId));
                            }
                        }
                        else { shipInput.vertical = 0f; }
                        #endregion Rewired Vertical Input

                        #region Rewired Longitudinal Input
                        if (longitudinalInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (longitudinalInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                shipInput.longitudinal = positiveLongitudinalInputActionType == 1 ? (rewiredPlayer.GetButton(positiveLongitudinalInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(positiveLongitudinalInputActionId);
                            }
                            else
                            {
                                shipInput.longitudinal = (positiveLongitudinalInputActionType == 1 ? (rewiredPlayer.GetButton(positiveLongitudinalInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(positiveLongitudinalInputActionId)) -
                                                         (negativeLongitudinalInputActionType == 1 ? (rewiredPlayer.GetButton(negativeLongitudinalInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(negativeLongitudinalInputActionId));
                            }
                        }
                        else { shipInput.longitudinal = 0f; }
                        #endregion Rewired Longitudinal Input

                        #region Rewired Pitch Input
                        if (pitchInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (pitchInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                shipInput.pitch = positivePitchInputActionType == 1 ? (rewiredPlayer.GetButton(positivePitchInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(positivePitchInputActionId);
                            }
                            else
                            {
                                shipInput.pitch = (positivePitchInputActionType == 1 ? (rewiredPlayer.GetButton(positivePitchInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(positivePitchInputActionId)) -
                                                  (negativePitchInputActionType == 1 ? (rewiredPlayer.GetButton(negativePitchInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(negativePitchInputActionId));
                            }
                        }
                        else { shipInput.pitch = 0f; }
                        #endregion Rewired Pitch Input

                        #region Rewired Yaw Input
                        if (yawInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (yawInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                shipInput.yaw = positiveYawInputActionType == 1 ? (rewiredPlayer.GetButton(positiveYawInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(positiveYawInputActionId);
                            }
                            else
                            {
                                shipInput.yaw = (positiveYawInputActionType == 1 ? (rewiredPlayer.GetButton(positiveYawInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(positiveYawInputActionId)) -
                                                (negativeYawInputActionType == 1 ? (rewiredPlayer.GetButton(negativeYawInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(negativeYawInputActionId));
                            }
                        }
                        else { shipInput.yaw = 0f; }
                        #endregion Rewired Yaw Input

                        #region Rewired Roll Input
                        if (rollInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (rollInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                shipInput.roll = positiveRollInputActionType == 1 ? (rewiredPlayer.GetButton(positiveRollInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(positiveRollInputActionId);
                            }
                            else
                            {
                                shipInput.roll = (positiveRollInputActionType == 1 ? (rewiredPlayer.GetButton(positiveRollInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(positiveRollInputActionId)) -
                                                 (negativeRollInputActionType == 1 ? (rewiredPlayer.GetButton(negativeRollInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(negativeRollInputActionId));
                            }
                        }
                        else { shipInput.roll = 0f; }
                        #endregion Rewired Roll Input

                        #region Rewired Primary Fire Input
                        // There is no need to check if it is enabled, because by default it is -1 and the PlayerInputModuleEditor
                        // ensures it is -1 when not enabled. Also, it is possible to be enabled but not yet set to +ve ActionId
                        if (primaryFireInputActionId >= 0)
                        {
                            if (primaryFireCanBeHeld)
                            {
                                shipInput.primaryFire = primaryFireInputActionType == 1 ? rewiredPlayer.GetButton(primaryFireInputActionId) : rewiredPlayer.GetAxis(primaryFireInputActionId) > 0.01f;
                            }
                            else
                            {
                                shipInput.primaryFire = primaryFireInputActionType == 1 ? rewiredPlayer.GetButtonDown(primaryFireInputActionId) : !shipInput.primaryFire && rewiredPlayer.GetAxis(primaryFireInputActionId) > 0.01f;
                            }
                        }
                        else { shipInput.primaryFire = false; }

                        #endregion Rewired Primary Fire Input

                        #region Rewired Secondary Fire Input

                        // There is no need to check if it is enabled, because by default it is -1 and the PlayerInputModuleEditor
                        // ensures it is -1 when not enabled. Also, it is possible to be enabled but not yet set to +ve ActionId
                        if (secondaryFireInputActionId >= 0)
                        {
                            if (secondaryFireCanBeHeld)
                            {
                                shipInput.secondaryFire = secondaryFireInputActionType == 1 ? rewiredPlayer.GetButton(secondaryFireInputActionId) : rewiredPlayer.GetAxis(secondaryFireInputActionId) > 0.01f;
                            }
                            else
                            {
                                shipInput.secondaryFire = secondaryFireInputActionType == 1 ? rewiredPlayer.GetButtonDown(secondaryFireInputActionId) : !shipInput.secondaryFire && rewiredPlayer.GetAxis(secondaryFireInputActionId) > 0.01f;
                            }
                        }
                        else { shipInput.secondaryFire = false; }

                        #endregion Rewired Secondary Fire Input

                        #region Rewired Docking Input

                        // There is no need to check if it is enabled, because by default it is -1 and the PlayerInputModuleEditor
                        // ensures it is -1 when not enabled. Also, it is possible to be enabled but not yet set to +ve ActionId
                        if (dockingInputActionId >= 0)
                        {
                            shipInput.dock = dockingInputActionType == 1 ? rewiredPlayer.GetButtonDown(dockingInputActionId) : !shipInput.dock && rewiredPlayer.GetAxis(dockingInputActionId) > 0.01f;
                        }
                        else { shipInput.dock = false; }
                        #endregion Rewired Docking Input
                    }

                    #region Custom Player Input
                    if (numberOfCustomPlayerInputs > 0)
                    {
                        for (int cpiIdx = 0; cpiIdx < numberOfCustomPlayerInputs; cpiIdx++)
                        {
                            CustomPlayerInput customPlayerInput = customPlayerInputList[cpiIdx];

                            if (customPlayerInput != null && customPlayerInput.customPlayerInputEvt != null)
                            {
                                if (customPlayerInput.isButton)
                                {
                                    if (customPlayerInput.rwdPositiveInputActionId >= 0)
                                    {
                                        if (customPlayerInput.canBeHeldDown)
                                        {
                                            customPlayerInput.rwLastIsButtonPressed = customPlayerInput.rwdPositiveInputActionType == 1 ? rewiredPlayer.GetButton(customPlayerInput.rwdPositiveInputActionId) : rewiredPlayer.GetAxis(customPlayerInput.rwdPositiveInputActionId) > 0.01f;
                                        }
                                        else
                                        {
                                            customPlayerInput.rwLastIsButtonPressed = customPlayerInput.rwdPositiveInputActionType == 1 ? rewiredPlayer.GetButtonDown(customPlayerInput.rwdPositiveInputActionId) : !customPlayerInput.rwLastIsButtonPressed && rewiredPlayer.GetAxis(customPlayerInput.rwdPositiveInputActionId) > 0.01f;
                                        }

                                        if (customPlayerInput.rwLastIsButtonPressed)
                                        {
                                            // CustomPlayerInput.CustomPlayerInputEventType is Button (5).
                                            customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(1f, 0f, 0f), 5);
                                        }
                                    }
                                }
                                else if (customPlayerInput.inputAxisMode != InputAxisMode.NoInput)
                                {
                                    if (customPlayerInput.inputAxisMode == InputAxisMode.SingleAxis)
                                    {
                                        float inputValueF = customPlayerInput.rwdPositiveInputActionType == 1 ? (rewiredPlayer.GetButton(customPlayerInput.rwdPositiveInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(customPlayerInput.rwdPositiveInputActionId);
                                        if (customPlayerInput.isSensitivityEnabled)
                                        {
                                            inputValueF = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValueF, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                        }
                                        customPlayerInput.lastInputValueX = inputValueF;
                                        // CustomPlayerInput.CustomPlayerInputEventType is Axis1D (1).
                                        customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(inputValueF, 0f, 0f), 1);
                                    }
                                    else // Combined
                                    {
                                        float inputValueF = (customPlayerInput.rwdPositiveInputActionType == 1 ? (rewiredPlayer.GetButton(customPlayerInput.rwdPositiveInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(customPlayerInput.rwdPositiveInputActionId)) -
                                                            (customPlayerInput.rwdNegativeInputActionType == 1 ? (rewiredPlayer.GetButton(customPlayerInput.rwdNegativeInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(customPlayerInput.rwdNegativeInputActionId));
                                        if (customPlayerInput.isSensitivityEnabled)
                                        {
                                            inputValueF = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValueF, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                        }
                                        customPlayerInput.lastInputValueX = inputValueF;
                                        // CustomPlayerInput.CustomPlayerInputEventType is Axis1D (1).
                                        customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(inputValueF, 0f, 0f), 1);
                                    }
                                }
                            }
                        }
                    }
                    #endregion Custom Player Input
                }
                else
                {
                    // Do nothing
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
                #else
                // Do nothing if rewired is set but not installed in the project
                shipInput.horizontal = 0f;
                shipInput.vertical = 0f;
                shipInput.longitudinal = 0f;
                shipInput.pitch = 0f;
                shipInput.yaw = 0f;
                shipInput.roll = 0f;
                shipInput.primaryFire = false;
                shipInput.secondaryFire = false;
                shipInput.dock = false;
                #endif
                #endregion Rewired Input
            }
            else if (inputMode == InputMode.Vive)
            {
                #region Vive Input
                
                #if VIU_PLUGIN

                // If viveHorizontalRoleSystemType etc is null, in the Unity Editor at runtime the following error will be raised:
                // PlayerInputModule VIVE Argument cannot be null.
                // The viveHorizontalRoleType enum in this class is converted into a System.Type at runtime by calling UpdateViveInput(..).
                // Axis or Button integers are converted to ControllerAxis or ControllerButton in UpdateViveInput(..) to avoid conversion overhead
                // in this event.

                // Head Mounted Device (HMD), VivePose.GetPoseEx(..) returns rot (rotation) property. viveRigidPose.rot.eulerAngles.x returns
                // the Unity -ve Y-axis where down is +ve and up is -ve starting at 359.99.. and decreasing in the up direction.
                // GetPoseEx seems to return X values 0-90 deg (down) and 359.9-270 (up). We convert these into Y 0.0 to 1.0 (down) and 0.0 to -1.0 (up).
                // GetPoseEx returns Y 0-360 deg. We convert them to X-axis in Unity. 0-179.99.. (right) becomes 0.0 to 1.0 (right). 180-359.99.. (left) becomes -1.0 to 0.0

                try
                {
                    if (isInputEnabled)
                    {
                        #region Vive Horizontal Input
                        if (horizontalInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (horizontalInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                if (viveHorizontalInputType == ViveInputType.Axis)
                                {
                                    shipInput.horizontal = ViveInput.GetAxisEx(viveHorizontalRoleSystemType, positiveHorizontalInputRoleId, positiveHorizontalInputCtrlAxisVive);
                                }
                                else if (viveHorizontalInputType == ViveInputType.Button)
                                {
                                    shipInput.horizontal = ViveInput.GetPressEx(viveHorizontalRoleSystemType, positiveHorizontalInputRoleId, positiveHorizontalInputCtrlBtnVive) ? 1f : 0f;
                                }
                                else // Pose
                                {
                                    HTC.UnityPlugin.Utility.RigidPose viveRigidPose = VivePose.GetPoseEx(viveHorizontalRoleSystemType, positiveHorizontalInputRoleId);
                                    // positiveHorizontalInputCtrlVive 0 = RotationX, 1 = RotationY, 2 = RotationZ

                                    if (positiveHorizontalInputCtrlVive == 0)
                                    {
                                        float horizontalX = viveRigidPose.rot.eulerAngles.y;
                                        shipInput.horizontal = horizontalX < 180f ? horizontalX / -180f : (horizontalX - 180f) / 180f;
                                    }
                                    else if (positiveHorizontalInputCtrlVive == 1)
                                    {
                                        float horizontalY = viveRigidPose.rot.eulerAngles.x;
                                        shipInput.horizontal = horizontalY <= 90f ? horizontalY / -90f : (360f - horizontalY) / 90f;
                                    }
                                    else
                                    {
                                        float horizontalZ = viveRigidPose.rot.eulerAngles.z;
                                        shipInput.horizontal = horizontalZ < 180f ? horizontalZ / 180f : (horizontalZ - 180f) / -180f;
                                    }
                                }
                            }
                            else // Combined Axis
                            {
                                if (viveHorizontalInputType == ViveInputType.Axis)
                                {
                                    shipInput.horizontal = ViveInput.GetAxisEx(viveHorizontalRoleSystemType, positiveHorizontalInputRoleId, positiveHorizontalInputCtrlAxisVive) - ViveInput.GetAxisEx(viveHorizontalRoleSystemType, negativeHorizontalInputRoleId, negativeHorizontalInputCtrlAxisVive);
                                }
                                else if (viveHorizontalInputType == ViveInputType.Button)
                                {
                                    shipInput.horizontal = (ViveInput.GetPressEx(viveHorizontalRoleSystemType, positiveHorizontalInputRoleId, positiveHorizontalInputCtrlBtnVive) ? 1f : 0f) - (ViveInput.GetPressEx(viveHorizontalRoleSystemType, negativeHorizontalInputRoleId, negativeHorizontalInputCtrlBtnVive) ? 1f : 0f);
                                }
                                else // Pose - currently only supports single-axis
                                {

                                }
                            }
                        }
                        else { shipInput.horizontal = 0f; }
                        #endregion  Vive Horizontal Input

                        #region Vive Vertical Input
                        if (verticalInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (verticalInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                if (viveVerticalInputType == ViveInputType.Axis)
                                {
                                    shipInput.vertical = ViveInput.GetAxisEx(viveVerticalRoleSystemType, positiveVerticalInputRoleId, positiveVerticalInputCtrlAxisVive);
                                }
                                else if (viveVerticalInputType == ViveInputType.Button)
                                {
                                    shipInput.vertical = ViveInput.GetPressEx(viveVerticalRoleSystemType, positiveVerticalInputRoleId, positiveVerticalInputCtrlBtnVive) ? 1f : 0f;
                                }
                                else // Pose
                                {
                                    HTC.UnityPlugin.Utility.RigidPose viveRigidPose = VivePose.GetPoseEx(viveVerticalRoleSystemType, positiveVerticalInputRoleId);
                                    // positiveVerticalInputCtrlVive 0 = RotationX, 1 = RotationY, 2 = RotationZ

                                    if (positiveVerticalInputCtrlVive == 0)
                                    {
                                        float verticalX = viveRigidPose.rot.eulerAngles.y;
                                        shipInput.vertical = verticalX < 180f ? verticalX / 180f : (verticalX - 180f) / -180f;
                                    }
                                    else if (positiveVerticalInputCtrlVive == 1)
                                    {
                                        float verticalY = viveRigidPose.rot.eulerAngles.x;
                                        shipInput.vertical = verticalY <= 90f ? verticalY / 90f : (360f - verticalY) / -90f;
                                    }
                                    else
                                    {
                                        float verticalZ = viveRigidPose.rot.eulerAngles.z;
                                        shipInput.vertical = verticalZ < 180f ? verticalZ / 180f : (verticalZ - 180f) / -180f;
                                    }
                                }
                            }
                            else // Combined Axis
                            {
                                if (viveVerticalInputType == ViveInputType.Axis)
                                {
                                    shipInput.vertical = ViveInput.GetAxisEx(viveVerticalRoleSystemType, positiveVerticalInputRoleId, positiveVerticalInputCtrlAxisVive) - ViveInput.GetAxisEx(viveVerticalRoleSystemType, negativeVerticalInputRoleId, negativeVerticalInputCtrlAxisVive);
                                }
                                else if (viveVerticalInputType == ViveInputType.Button)
                                {
                                    shipInput.vertical = (ViveInput.GetPressEx(viveVerticalRoleSystemType, positiveVerticalInputRoleId, positiveVerticalInputCtrlBtnVive) ? 1f : 0f) - (ViveInput.GetPressEx(viveVerticalRoleSystemType, negativeVerticalInputRoleId, negativeVerticalInputCtrlBtnVive) ? 1f : 0f);
                                }
                                else // Pose - currently only supports single-axis
                                {
                                    HTC.UnityPlugin.Utility.RigidPose viveRigidPose = VivePose.GetPoseEx(viveVerticalRoleSystemType, positiveVerticalInputRoleId);
                                    // positiveVerticalInputCtrlVive 0 = RotationX, 1 = RotationY, 2 = RotationZ

                                    if (positiveVerticalInputCtrlVive == 0)
                                    {
                                        float verticalX = viveRigidPose.rot.eulerAngles.y;
                                        shipInput.vertical = verticalX < 180f ? verticalX / 180f : (verticalX - 180f) / -180f;
                                    }
                                    else if (positiveVerticalInputCtrlVive == 1)
                                    {
                                        float verticalY = viveRigidPose.rot.eulerAngles.x;
                                        shipInput.vertical = verticalY <= 90f ? verticalY / 90f : (360f - verticalY) / -90f;
                                    }
                                    else
                                    {
                                        float verticalZ = viveRigidPose.rot.eulerAngles.z;
                                        shipInput.vertical = verticalZ < 180f ? verticalZ / 180f : (verticalZ - 180f) / -180f;
                                    }
                                }
                            }
                        }
                        else{ shipInput.vertical = 0f; }

                        #endregion Vive Vertical Input

                        #region Vive Longitudinal Input
                        if (longitudinalInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (longitudinalInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                if (viveLongitudinalInputType == ViveInputType.Axis)
                                {
                                    shipInput.longitudinal = ViveInput.GetAxisEx(viveLongitudinalRoleSystemType, positiveLongitudinalInputRoleId, positiveLongitudinalInputCtrlAxisVive);
                                }
                                else if (viveLongitudinalInputType == ViveInputType.Button)
                                {
                                    shipInput.longitudinal = ViveInput.GetPressEx(viveLongitudinalRoleSystemType, positiveLongitudinalInputRoleId, positiveLongitudinalInputCtrlBtnVive) ? 1f : 0f;
                                }
                                else // Pose
                                {
                                    HTC.UnityPlugin.Utility.RigidPose viveRigidPose = VivePose.GetPoseEx(viveLongitudinalRoleSystemType, positiveLongitudinalInputRoleId);
                                    // positiveLongitudinalInputCtrlVive 0 = RotationX, 1 = RotationY, 2 = RotationZ

                                    if (positiveLongitudinalInputCtrlVive == 0)
                                    {
                                        float longitudinalX = viveRigidPose.rot.eulerAngles.y;
                                        shipInput.longitudinal = longitudinalX < 180f ? longitudinalX / 180f : (longitudinalX - 180f) / -180f;                                
                                    }
                                    else if (positiveLongitudinalInputCtrlVive == 1)
                                    {
                                        float longitudinalY = viveRigidPose.rot.eulerAngles.x;
                                        shipInput.longitudinal = longitudinalY <= 90f ? longitudinalY / 90f : (360f - longitudinalY) / -90f;
                                    }
                                    else
                                    {
                                        float longitudinalZ = viveRigidPose.rot.eulerAngles.z;
                                        shipInput.longitudinal = longitudinalZ < 180f ? longitudinalZ / 180f : (longitudinalZ - 180f) / -180f;
                                    }
                                }
                            }
                            else // Combined Axis
                            {
                                if (viveLongitudinalInputType == ViveInputType.Axis)
                                {
                                    shipInput.longitudinal = ViveInput.GetAxisEx(viveLongitudinalRoleSystemType, positiveLongitudinalInputRoleId, positiveLongitudinalInputCtrlAxisVive) - ViveInput.GetAxisEx(viveLongitudinalRoleSystemType, negativeLongitudinalInputRoleId, negativeLongitudinalInputCtrlAxisVive);
                                }
                                else if (viveLongitudinalInputType == ViveInputType.Button)
                                {
                                    shipInput.longitudinal = (ViveInput.GetPressEx(viveLongitudinalRoleSystemType, positiveLongitudinalInputRoleId, positiveLongitudinalInputCtrlBtnVive) ? 1f : 0f) - (ViveInput.GetPressEx(viveLongitudinalRoleSystemType, negativeLongitudinalInputRoleId, negativeLongitudinalInputCtrlBtnVive) ? 1f : 0f);
                                }
                                else // Pose - currently only supports single-axis
                                {
                                    HTC.UnityPlugin.Utility.RigidPose viveRigidPose = VivePose.GetPoseEx(viveLongitudinalRoleSystemType, positiveLongitudinalInputRoleId);
                                    // positiveLongitudinalInputCtrlVive 0 = RotationX, 1 = RotationY, 2 = RotationZ

                                    if (positiveLongitudinalInputCtrlVive == 0)
                                    {
                                        float longitudinalX = viveRigidPose.rot.eulerAngles.y;
                                        shipInput.longitudinal = longitudinalX < 180f ? longitudinalX / 180f : (longitudinalX - 180f) / -180f;
                                    }
                                    else if (positiveLongitudinalInputCtrlVive == 1)
                                    {
                                        float longitudinalY = viveRigidPose.rot.eulerAngles.x;
                                        shipInput.longitudinal = longitudinalY <= 90f ? longitudinalY / 90f : (360f - longitudinalY) / -90f;
                                    }
                                    else
                                    {
                                        float longitudinalZ = viveRigidPose.rot.eulerAngles.z;
                                        shipInput.longitudinal = longitudinalZ < 180f ? longitudinalZ / 180f : (longitudinalZ - 180f) / -180f;
                                    }
                                }
                            }
                        }
                        else { shipInput.longitudinal = 0f; }
                        #endregion Vive Longitudinal Input

                        #region Vive Pitch Input
                        if (pitchInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (pitchInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                if (vivePitchInputType == ViveInputType.Axis)
                                {
                                    shipInput.pitch = ViveInput.GetAxisEx(vivePitchRoleSystemType, positivePitchInputRoleId, positivePitchInputCtrlAxisVive);
                                }
                                else if (vivePitchInputType == ViveInputType.Button)
                                {
                                    shipInput.pitch = ViveInput.GetPressEx(vivePitchRoleSystemType, positivePitchInputRoleId, positivePitchInputCtrlBtnVive) ? 1f : 0f;
                                }
                                else // Pose
                                {
                                    HTC.UnityPlugin.Utility.RigidPose viveRigidPose = VivePose.GetPoseEx(vivePitchRoleSystemType, positivePitchInputRoleId);
                                    // positivePitchInputCtrlVive 0 = RotationX, 1 = RotationY, 2 = RotationZ

                                    if (positivePitchInputCtrlVive == 0)
                                    {
                                        float pitchX = viveRigidPose.rot.eulerAngles.y;
                                        shipInput.pitch = pitchX < 180f ? pitchX / -180f : (pitchX - 180f) / 180f;
                                    }
                                    else if (positivePitchInputCtrlVive == 1)
                                    {
                                        float pitchY = viveRigidPose.rot.eulerAngles.x;
                                        shipInput.pitch = pitchY <= 90f ? pitchY/90f : (360f - pitchY) / -90f;
                                    }
                                    else
                                    {
                                        float pitchZ = viveRigidPose.rot.eulerAngles.z;
                                        shipInput.pitch = pitchZ < 180f ? pitchZ / -180f : (pitchZ - 180f) / 180f;
                                    }
                                }
                            }
                            else // Combined Axis
                            {
                                if (vivePitchInputType == ViveInputType.Axis)
                                {
                                    shipInput.pitch = ViveInput.GetAxisEx(vivePitchRoleSystemType, positivePitchInputRoleId, positivePitchInputCtrlAxisVive) - ViveInput.GetAxisEx(vivePitchRoleSystemType, negativePitchInputRoleId, negativePitchInputCtrlAxisVive);
                                }
                                else if (vivePitchInputType == ViveInputType.Button)
                                {
                                    shipInput.pitch = (ViveInput.GetPressEx(vivePitchRoleSystemType, positivePitchInputRoleId, positivePitchInputCtrlBtnVive) ? 1f : 0f) - (ViveInput.GetPressEx(vivePitchRoleSystemType, negativePitchInputRoleId, negativePitchInputCtrlBtnVive) ? 1f : 0f);
                                }
                                else // Pose - currently only supports single axis
                                {
                                    HTC.UnityPlugin.Utility.RigidPose viveRigidPose = VivePose.GetPoseEx(vivePitchRoleSystemType, positivePitchInputRoleId);
                                    // positivePitchInputCtrlVive 0 = RotationX, 1 = RotationY, 2 = RotationZ

                                    if (positivePitchInputCtrlVive == 0)
                                    {
                                        float pitchX = viveRigidPose.rot.eulerAngles.y;
                                        shipInput.pitch = pitchX < 180f ? pitchX / -180f : (pitchX - 180f) / 180f;
                                    }
                                    else if (positivePitchInputCtrlVive == 1)
                                    {
                                        float pitchY = viveRigidPose.rot.eulerAngles.x;
                                        shipInput.pitch = pitchY <= 90f ? pitchY / 90f : (360f - pitchY) / -90f;
                                    }
                                    else
                                    {
                                        float pitchZ = viveRigidPose.rot.eulerAngles.z;
                                        shipInput.pitch = pitchZ < 180f ? pitchZ / -180f : (pitchZ - 180f) / 180f;
                                    }
                                }
                            }
                        }
                        else { shipInput.pitch = 0f; }
                        #endregion Vive Pitch Input

                        #region Vive Yaw Input
                        if (yawInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (yawInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                if (viveYawInputType == ViveInputType.Axis)
                                {
                                    shipInput.yaw = ViveInput.GetAxisEx(viveYawRoleSystemType, positiveYawInputRoleId, positiveYawInputCtrlAxisVive);
                                }
                                else if (viveYawInputType == ViveInputType.Button)
                                {
                                    shipInput.yaw = ViveInput.GetPressEx(viveYawRoleSystemType, positiveYawInputRoleId, positiveYawInputCtrlBtnVive) ? 1f : 0f;
                                }
                                else // Pose
                                {
                                    HTC.UnityPlugin.Utility.RigidPose viveRigidPose = VivePose.GetPoseEx(viveYawRoleSystemType, positiveYawInputRoleId);
                                    // positiveYawInputCtrlVive 0 = RotationX, 1 = RotationY, 2 = RotationZ

                                    if (positiveYawInputCtrlVive == 0)
                                    {
                                        float yawX = viveRigidPose.rot.eulerAngles.y;
                                        shipInput.yaw = yawX < 180f ? yawX / 180f : (yawX - 180f) / -180f;
                                    }
                                    else if (positiveYawInputCtrlVive == 1)
                                    {
                                        float yawY = viveRigidPose.rot.eulerAngles.x;
                                        shipInput.yaw = yawY <= 90f ? yawY / 90f : (360f - yawY) / -90f;
                                    }
                                    else
                                    {
                                        float yawZ = viveRigidPose.rot.eulerAngles.z;
                                        shipInput.yaw = yawZ < 180f ? yawZ / 180f : (yawZ - 180f) / -180f;
                                    }
                                }
                            }
                            else // Combined Axis
                            {
                                if (viveYawInputType == ViveInputType.Axis)
                                {
                                    shipInput.yaw = ViveInput.GetAxisEx(viveYawRoleSystemType, positiveYawInputRoleId, positiveYawInputCtrlAxisVive) - ViveInput.GetAxisEx(viveYawRoleSystemType, negativeYawInputRoleId, negativeYawInputCtrlAxisVive);
                                }
                                else if (viveYawInputType == ViveInputType.Button)
                                {
                                    shipInput.yaw = (ViveInput.GetPressEx(viveYawRoleSystemType, positiveYawInputRoleId, positiveYawInputCtrlBtnVive) ? 1f : 0f) - (ViveInput.GetPressEx(viveYawRoleSystemType, negativeYawInputRoleId, negativeYawInputCtrlBtnVive) ? 1f : 0f);
                                }
                                else // Pose - currently only supports single axis
                                {
                                    HTC.UnityPlugin.Utility.RigidPose viveRigidPose = VivePose.GetPoseEx(viveYawRoleSystemType, positiveYawInputRoleId);
                                    // positiveYawInputCtrlVive 0 = RotationX, 1 = RotationY, 2 = RotationZ

                                    if (positiveYawInputCtrlVive == 0)
                                    {
                                        float yawX = viveRigidPose.rot.eulerAngles.y;
                                        shipInput.yaw = yawX < 180f ? yawX / 180f : (yawX - 180f) / -180f;
                                    }
                                    else if (positiveYawInputCtrlVive == 1)
                                    {
                                        float yawY = viveRigidPose.rot.eulerAngles.x;
                                        shipInput.yaw = yawY <= 90f ? yawY / 90f : (360f - yawY) / -90f;
                                    }
                                    else
                                    {
                                        float yawZ = viveRigidPose.rot.eulerAngles.z;
                                        shipInput.yaw = yawZ < 180f ? yawZ / 180f : (yawZ - 180f) / -180f;
                                    }
                                }
                            }
                        }
                        else { shipInput.yaw = 0f; }
                        #endregion Vive Yaw Input

                        #region Vive Roll Input
                        if (rollInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (rollInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                if (viveRollInputType == ViveInputType.Axis)
                                {
                                    shipInput.roll = ViveInput.GetAxisEx(viveRollRoleSystemType, positiveRollInputRoleId, positiveRollInputCtrlAxisVive);
                                }
                                else if (viveRollInputType == ViveInputType.Button)
                                {
                                    shipInput.roll = ViveInput.GetPressEx(viveRollRoleSystemType, positiveRollInputRoleId, positiveRollInputCtrlBtnVive) ? 1f : 0f;
                                }
                                else // Pose
                                {
                                    HTC.UnityPlugin.Utility.RigidPose viveRigidPose = VivePose.GetPoseEx(viveRollRoleSystemType, positiveRollInputRoleId);
                                    // positiveRollInputCtrlVive 0 = RotationX, 1 = RotationY, 2 = RotationZ

                                    if (positiveRollInputCtrlVive == 0)
                                    {
                                        float rollX = viveRigidPose.rot.eulerAngles.y;
                                        shipInput.roll = rollX < 180f ? rollX / 180f : (rollX - 180f) / -180f;
                                    }
                                    else if (positiveRollInputCtrlVive == 1)
                                    {
                                        float rollY = viveRigidPose.rot.eulerAngles.x;
                                        shipInput.roll = rollY <= 90f ? rollY / 90f : (360f - rollY) / -90f;
                                    }
                                    else
                                    {
                                        float rollZ = viveRigidPose.rot.eulerAngles.z;
                                        shipInput.roll = rollZ < 180f ? rollZ / 180f : (rollZ - 180f) / -180f;
                                    }
                                }
                            }
                            else // Combined Axis
                            {
                                if (viveRollInputType == ViveInputType.Axis)
                                {
                                    shipInput.roll = ViveInput.GetAxisEx(viveRollRoleSystemType, positiveRollInputRoleId, positiveRollInputCtrlAxisVive) - ViveInput.GetAxisEx(viveRollRoleSystemType, negativeRollInputRoleId, negativeRollInputCtrlAxisVive);
                                }
                                else if (viveRollInputType == ViveInputType.Button)
                                {
                                    shipInput.roll = (ViveInput.GetPressEx(viveRollRoleSystemType, positiveRollInputRoleId, positiveRollInputCtrlBtnVive) ? 1f : 0f) - (ViveInput.GetPressEx(viveRollRoleSystemType, negativeRollInputRoleId, negativeRollInputCtrlBtnVive) ? 1f : 0f);
                                }
                                else // Pose - Currently only supports single axis
                                {
                                    HTC.UnityPlugin.Utility.RigidPose viveRigidPose = VivePose.GetPoseEx(viveRollRoleSystemType, positiveRollInputRoleId);
                                    // positiveRollInputCtrlVive 0 = RotationX, 1 = RotationY, 2 = RotationZ

                                    if (positiveRollInputCtrlVive == 0)
                                    {
                                        float rollX = viveRigidPose.rot.eulerAngles.y;
                                        shipInput.roll = rollX < 180f ? rollX / 180f : (rollX - 180f) / -180f;
                                    }
                                    else if (positiveRollInputCtrlVive == 1)
                                    {
                                        float rollY = viveRigidPose.rot.eulerAngles.x;
                                        shipInput.roll = rollY <= 90f ? rollY / 90f : (360f - rollY) / -90f;
                                    }
                                    else
                                    {
                                        float rollZ = viveRigidPose.rot.eulerAngles.z;
                                        shipInput.roll = rollZ < 180f ? rollZ / 180f : (rollZ - 180f) / -180f;
                                    }
                                }
                            }
                        }
                        else { shipInput.roll = 0f; }
                        #endregion Vive Roll Input

                        #region Vive Primary Fire Input

                        if (primaryFireInputCtrlVive > 0)
                        {
                            if (primaryFireCanBeHeld)
                            {
                                shipInput.primaryFire = ViveInput.GetPressEx(vivePrimaryFireRoleSystemType, primaryFireInputRoleId, primaryFireInputCtrlBtnVive);
                            }
                            else
                            {
                                shipInput.primaryFire = ViveInput.GetPressDownEx(vivePrimaryFireRoleSystemType, primaryFireInputRoleId, primaryFireInputCtrlBtnVive);
                            }
                        }
                        else { shipInput.primaryFire = false; }
                        #endregion Vive Primary Fire Input

                        #region Vive Secondary Fire Input

                        if (secondaryFireInputCtrlVive > 0)
                        {
                            if (secondaryFireCanBeHeld)
                            {
                                shipInput.secondaryFire = ViveInput.GetPressEx(viveSecondaryFireRoleSystemType, secondaryFireInputRoleId, secondaryFireInputCtrlBtnVive);
                            }
                            else
                            {
                                shipInput.secondaryFire = ViveInput.GetPressDownEx(viveSecondaryFireRoleSystemType, secondaryFireInputRoleId, secondaryFireInputCtrlBtnVive);
                            }
                        }
                        else { shipInput.secondaryFire = false; }
                        #endregion Vive Secondary Fire Input

                        #region Vive Docking Input

                        if (dockingInputCtrlVive > 0)
                        {
                            shipInput.dock = ViveInput.GetPressDownEx(viveDockingRoleSystemType, dockingInputRoleId, dockingInputCtrlBtnVive);
                        }
                        else { shipInput.dock = false; }
                        #endregion Vive Docking Input
                    }

                    #region Custom Player Input
                    if (numberOfCustomPlayerInputs > 0)
                    {
                        for (int cpiIdx = 0; cpiIdx < numberOfCustomPlayerInputs; cpiIdx++)
                        {
                            CustomPlayerInput customPlayerInput = customPlayerInputList[cpiIdx];

                            if (customPlayerInput != null && customPlayerInput.customPlayerInputEvt != null)
                            {
                                if (customPlayerInput.isButton)
                                {
                                    if (customPlayerInput.vivePositiveInputCtrl > 0)
                                    {
                                        if (primaryFireCanBeHeld)
                                        {
                                            if (ViveInput.GetPressEx(customPlayerInput.viveRoleSystemType, customPlayerInput.vivePositiveInputRoleId, customPlayerInput.vivePositiveInputCtrlBtn))
                                            {
                                                // CustomPlayerInput.CustomPlayerInputEventType is Button (5).
                                                customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(1f, 0f, 0f), 5);
                                            }
                                        }
                                        else if (ViveInput.GetPressDownEx(customPlayerInput.viveRoleSystemType, customPlayerInput.vivePositiveInputRoleId, customPlayerInput.vivePositiveInputCtrlBtn))
                                        {
                                            // CustomPlayerInput.CustomPlayerInputEventType is Button (5).
                                            customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(1f, 0f, 0f), 5);
                                        }
                                    }
                                }
                                else if (customPlayerInput.inputAxisMode != InputAxisMode.NoInput)
                                {
                                    if (customPlayerInput.inputAxisMode == InputAxisMode.SingleAxis)
                                    {
                                        if (customPlayerInput.viveInputType == ViveInputType.Axis)
                                        {
                                            float inputValueF = ViveInput.GetAxisEx(customPlayerInput.viveRoleSystemType, customPlayerInput.vivePositiveInputRoleId, customPlayerInput.vivePositiveInputCtrlAxis);
                                            if (customPlayerInput.isSensitivityEnabled)
                                            {
                                                inputValueF = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValueF, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                            }
                                            customPlayerInput.lastInputValueX = inputValueF;
                                            // CustomPlayerInput.CustomPlayerInputEventType is Axis1D (1).
                                            customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(inputValueF, 0f, 0f), 1);
                                        }
                                        else if (customPlayerInput.viveInputType == ViveInputType.Button)
                                        {
                                            // Allow the user to send 0 or 1.
                                            float inputValueF = ViveInput.GetPressEx(customPlayerInput.viveRoleSystemType, customPlayerInput.vivePositiveInputRoleId, customPlayerInput.vivePositiveInputCtrlBtn) ? 1f : 0f;
                                            customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(inputValueF, 0f, 0f), 5);
                                        }
                                        else // Pose
                                        {
                                            HTC.UnityPlugin.Utility.RigidPose viveRigidPose = VivePose.GetPoseEx(customPlayerInput.viveRoleSystemType, customPlayerInput.vivePositiveInputRoleId);
                                            // customPlayerInput.vivePositiveInputCtrl 0 = RotationX, 1 = RotationY, 2 = RotationZ

                                            // We "may" want to pass these as the raw rotation angle..
                                            if (customPlayerInput.vivePositiveInputCtrl == 0)
                                            {
                                                float rollX = viveRigidPose.rot.eulerAngles.y;
                                                rollX = rollX < 180f ? rollX / 180f : (rollX - 180f) / -180f;
                                                customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(rollX, 0f, 0f), 1);
                                            }
                                            else if (customPlayerInput.vivePositiveInputCtrl == 1)
                                            {
                                                float rollY = viveRigidPose.rot.eulerAngles.x;
                                                rollY = rollY <= 90f ? rollY / 90f : (360f - rollY) / -90f;
                                                customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(rollY, 0f, 0f), 1);
                                            }
                                            else
                                            {
                                                float rollZ = viveRigidPose.rot.eulerAngles.z;
                                                rollZ = rollZ < 180f ? rollZ / 180f : (rollZ - 180f) / -180f;
                                                customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(rollZ, 0f, 0f), 1);
                                            }
                                        }
                                    }
                                    else // Combined
                                    {
                                        if (customPlayerInput.viveInputType == ViveInputType.Axis)
                                        {
                                            float inputValueF = ViveInput.GetAxisEx(customPlayerInput.viveRoleSystemType, customPlayerInput.vivePositiveInputRoleId, customPlayerInput.vivePositiveInputCtrlAxis) - ViveInput.GetAxisEx(customPlayerInput.viveRoleSystemType, customPlayerInput.viveNegativeInputRoleId, customPlayerInput.viveNegativeInputCtrlAxis);
                                            if (customPlayerInput.isSensitivityEnabled)
                                            {
                                                inputValueF = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValueF, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                            }
                                            customPlayerInput.lastInputValueX = inputValueF;
                                            customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(inputValueF, 0f, 0f), 1);
                                        }
                                        else if (customPlayerInput.viveInputType == ViveInputType.Button)
                                        {
                                            // Allow the user to send -1, 0, or 1
                                            float inputValueF = (ViveInput.GetPressEx(customPlayerInput.viveRoleSystemType, customPlayerInput.vivePositiveInputRoleId, customPlayerInput.vivePositiveInputCtrlBtn) ? 1f : 0f) - (ViveInput.GetPressEx(customPlayerInput.viveRoleSystemType, customPlayerInput.viveNegativeInputRoleId, customPlayerInput.viveNegativeInputCtrlBtn) ? 1f : 0f);
                                            customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(inputValueF, 0f, 0f), 5);
                                        }
                                        else // Pose - currently only supports single axis
                                        {
                                            HTC.UnityPlugin.Utility.RigidPose viveRigidPose = VivePose.GetPoseEx(customPlayerInput.viveRoleSystemType, customPlayerInput.vivePositiveInputRoleId);
                                            // customPlayerInput.vivePositiveInputCtrl 0 = RotationX, 1 = RotationY, 2 = RotationZ

                                            // We "may" want to pass these as the raw rotation angle..
                                            if (customPlayerInput.vivePositiveInputCtrl == 0)
                                            {
                                                float rollX = viveRigidPose.rot.eulerAngles.y;
                                                rollX = rollX < 180f ? rollX / 180f : (rollX - 180f) / -180f;
                                                customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(rollX, 0f, 0f), 1);
                                            }
                                            else if (customPlayerInput.vivePositiveInputCtrl == 1)
                                            {
                                                float rollY = viveRigidPose.rot.eulerAngles.x;
                                                rollY = rollY <= 90f ? rollY / 90f : (360f - rollY) / -90f;
                                                customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(rollY, 0f, 0f), 1);
                                            }
                                            else
                                            {
                                                float rollZ = viveRigidPose.rot.eulerAngles.z;
                                                rollZ = rollZ < 180f ? rollZ / 180f : (rollZ - 180f) / -180f;
                                                customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(rollZ, 0f, 0f), 1);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion Custom Player Input

                }
                catch (System.Exception ex)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("PlayerInputModule VIVE " + ex.Message);
                    #else
                    // Keep compiler happy
                    if (ex != null) { }
                    #endif
                }
                #else
                // Do nothing if VIVE is set but not installed
                shipInput.horizontal = 0f;
                shipInput.vertical = 0f;
                shipInput.longitudinal = 0f;
                shipInput.pitch = 0f;
                shipInput.yaw = 0f;
                shipInput.roll = 0f;
                shipInput.primaryFire = false;
                shipInput.secondaryFire = false;
                shipInput.dock = false;
                #endif
                #endregion
            }
            else if (inputMode == InputMode.UnityXR)
            {
                #region Unity XR
                #if SCSM_XR && SSC_UIS

                // By default actions are enabled
                // InputAction needs to consider the ControlType returned.
                if (isInputEnabled)
                {
                    #region Unity XR Horizontal Input
                    shipInput.horizontal = UISReadActionInputFloat(positiveHorizontalInputActionUIS, positiveHorizontalInputActionDataSlotUIS, positiveHorizontalCtrlTypeHashUIS);
                    #endregion Unity XR Horizontal Input

                    #region Unity XR Vertical Input
                    shipInput.vertical = UISReadActionInputFloat(positiveVerticalInputActionUIS, positiveVerticalInputActionDataSlotUIS, positiveVerticalCtrlTypeHashUIS);
                    #endregion Unity XR Verical Input

                    #region Unity XR Longitudinal Input
                    shipInput.longitudinal = UISReadActionInputFloat(positiveLongitudinalInputActionUIS, positiveLongitudinalInputActionDataSlotUIS, positiveLongitudinalCtrlTypeHashUIS);
                    #endregion Unity XR Longitudinal Input

                    #region Unity XR Pitch Input
                    shipInput.pitch = UISReadActionInputFloat(positivePitchInputActionUIS, positivePitchInputActionDataSlotUIS, positivePitchCtrlTypeHashUIS);
                    #endregion Unity XR Pitch Input

                    #region Unity XR Yaw Input
                    shipInput.yaw = UISReadActionInputFloat(positiveYawInputActionUIS, positiveYawInputActionDataSlotUIS, positiveYawCtrlTypeHashUIS);
                    #endregion Unity XR Yaw Input

                    #region Unity XR Roll Input
                    shipInput.roll = UISReadActionInputFloat(positiveRollInputActionUIS, positiveRollInputActionDataSlotUIS, positiveRollCtrlTypeHashUIS);
                    #endregion Unity XR Roll Input

                    #region Unity XR Primary Fire Input
                    shipInput.primaryFire = UISReadActionInputBool(primaryFireInputActionUIS, primaryFireInputActionDataSlotUIS, primaryFireCtrlTypeHashUIS, primaryFireCanBeHeld);
                    #endregion Unity XR Primary Fire Input

                    #region Unity XR Secondary Fire Input
                    shipInput.secondaryFire = UISReadActionInputBool(secondaryFireInputActionUIS, secondaryFireInputActionDataSlotUIS, secondaryFireCtrlTypeHashUIS, secondaryFireCanBeHeld);
                    #endregion Unity XR Secondary Fire Input

                    #region Unity XR Dock Input
                    shipInput.dock = UISReadActionInputBool(dockingInputActionUIS, dockingInputActionDataSlotUIS, dockingCtrlTypeHashUIS, false);
                    #endregion Unity XR Dock Input

                    #region Unity XR Quit - currently disabled
                    //if (UISReadActionInputBool(quitInputActionUIS, 0, controlTypeButtonHashUIS, false))
                    //{
                    //    if (callbackOnQuitGame != null) {  callbackOnQuitGame.Invoke(shipControlModule); }
                    //    else
                    //    {
                    //        #if UNITY_EDITOR
                    //        UnityEditor.EditorApplication.isPlaying = false;
                    //        #else
                    //        Application.Quit();
                    //        #endif
                    //    }
                    //}
                    #endregion
                }

                #region XR Custom Player Input
                if (numberOfCustomPlayerInputs > 0)
                {
                    for (int cpiIdx = 0; cpiIdx < numberOfCustomPlayerInputs; cpiIdx++)
                    {
                        CustomPlayerInput customPlayerInput = customPlayerInputList[cpiIdx];

                        if (customPlayerInput != null && customPlayerInput.customPlayerInputEvt != null)
                        {
                            if (customPlayerInput.isButton)
                            {
                                if (UISReadActionInputBool(customPlayerInput.uisPositiveInputAction, customPlayerInput.uisPositiveInputActionDataSlot, customPlayerInput.uisPositiveCtrlTypeHash, customPlayerInput.canBeHeldDown))
                                {
                                    // CustomPlayerInput.CustomPlayerInputEventType is Button (5).
                                    customPlayerInput.customPlayerInputEvt.Invoke(new Vector3(1f, 0f, 0f), 5);
                                }
                            }
                            else
                            {
                                Vector3 inputValue = Vector3.zero;

                                if (customPlayerInput.uisPositiveInputAction != null)
                                {
                                    if (customPlayerInput.uisPositiveCtrlTypeHash == controlTypeAxisHashUIS || customPlayerInput.uisPositiveCtrlTypeHash == controlTypeButtonHashUIS)
                                    {
                                        // 1D axis and Buttons both retrun a float
                                        inputValue.x = customPlayerInput.uisPositiveInputAction.ReadValue<float>();

                                        if (customPlayerInput.isSensitivityEnabled)
                                        {
                                            inputValue.x = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValue.x, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                        }
                                        customPlayerInput.lastInputValueX = inputValue.x;
                                        // CustomPlayerInput.CustomPlayerInputEventType is Axis1D (1).
                                        customPlayerInput.customPlayerInputEvt.Invoke(inputValue, 1);
                                    }
                                    else if (customPlayerInput.uisPositiveCtrlTypeHash == controlTypeVector2HashUIS || customPlayerInput.uisPositiveCtrlTypeHash == controlTypeDpadHashUIS)
                                    {
                                        inputValue = customPlayerInput.uisPositiveInputAction.ReadValue<Vector2>();

                                        if (customPlayerInput.isSensitivityEnabled)
                                        {
                                            inputValue.x = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValue.x, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                            inputValue.y = CalculateAxisInput(customPlayerInput.lastInputValueY, inputValue.y, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                        }
                                        customPlayerInput.lastInputValueX = inputValue.x;
                                        customPlayerInput.lastInputValueY = inputValue.y;
                                        // CustomPlayerInput.CustomPlayerInputEventType is Axis2D (2).
                                        customPlayerInput.customPlayerInputEvt.Invoke(inputValue, 2);
                                    }
                                    else if (customPlayerInput.uisPositiveCtrlTypeHash == controlTypeVector3HashUIS)
                                    {
                                        inputValue = customPlayerInput.uisPositiveInputAction.ReadValue<Vector3>();

                                        if (customPlayerInput.isSensitivityEnabled)
                                        {
                                            // Currently only works on x,y axes
                                            inputValue.x = CalculateAxisInput(customPlayerInput.lastInputValueX, inputValue.x, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                            inputValue.y = CalculateAxisInput(customPlayerInput.lastInputValueY, inputValue.y, customPlayerInput.sensitivity, customPlayerInput.gravity);
                                        }
                                        customPlayerInput.lastInputValueX = inputValue.x;
                                        customPlayerInput.lastInputValueY = inputValue.y;
                                        // CustomPlayerInput.CustomPlayerInputEventType is Axis3D (3).
                                        customPlayerInput.customPlayerInputEvt.Invoke(inputValue, 3);
                                    }
                                }
                            }
                        }

                    }
                }
                #endregion

                #else
                // Do nothing if XR or Unity Input System is set but not installed in the project
                shipInput.horizontal = 0f;
                shipInput.vertical = 0f;
                shipInput.longitudinal = 0f;
                shipInput.pitch = 0f;
                shipInput.yaw = 0f;
                shipInput.roll = 0f;
                shipInput.primaryFire = false;
                shipInput.secondaryFire = false;
                shipInput.dock = false;
                #endif
                #endregion
            }
            #if UNITY_EDITOR
            else { Debug.Log("Invalid input mode: " + inputMode.ToString()); }
            #endif

            // cater for situation where input is disabled but CustomPlayerInput is allowed.
            if (!isInputEnabled) { return; }

            #region Sensitivity Implementation

            if (isSensitivityForHorizontalEnabled && !isHorizontalDataDiscarded)
            {
                shipInput.horizontal = CalculateAxisInput(lastShipInput.horizontal, shipInput.horizontal, horizontalAxisSensitivity, horizontalAxisGravity);
                // Store the calculated ship input from this frame for reference next frame
                lastShipInput.horizontal = shipInput.horizontal;
            }

            if (isSensitivityForVerticalEnabled && !isVerticalDataDiscarded)
            {
                shipInput.vertical = CalculateAxisInput(lastShipInput.vertical, shipInput.vertical, verticalAxisSensitivity, verticalAxisGravity);
                // Store the calculated ship input from this frame for reference next frame
                lastShipInput.vertical = shipInput.vertical;
            }

            if (isSensitivityForLongitudinalEnabled && !isLongitudinalDataDiscarded)
            {
                shipInput.longitudinal = CalculateAxisInput(lastShipInput.longitudinal, shipInput.longitudinal, longitudinalAxisSensitivity, longitudinalAxisGravity);
                // Store the calculated ship input from this frame for reference next frame
                lastShipInput.longitudinal = shipInput.longitudinal;
            }

            if (isSensitivityForPitchEnabled && !isPitchDataDiscarded)
            {
                shipInput.pitch = CalculateAxisInput(lastShipInput.pitch, shipInput.pitch, pitchAxisSensitivity, pitchAxisGravity);
                // Store the calculated ship input from this frame for reference next frame
                lastShipInput.pitch = shipInput.pitch;
            }

            if (isSensitivityForYawEnabled && !isYawDataDiscarded)
            {
                shipInput.yaw = CalculateAxisInput(lastShipInput.yaw, shipInput.yaw, yawAxisSensitivity, yawAxisGravity);
                // Store the calculated ship input from this frame for reference next frame
                lastShipInput.yaw = shipInput.yaw;
            }

            if (isSensitivityForRollEnabled && !isRollDataDiscarded)
            {
                shipInput.roll = CalculateAxisInput(lastShipInput.roll, shipInput.roll, rollAxisSensitivity, rollAxisGravity);
                // Store the calculated ship input from this frame for reference next frame
                lastShipInput.roll = shipInput.roll;
            }

            #endregion

            #region Send calculated input to the ship control module
            if (shipControlModule != null)
            {
                // If auto cruise is enabled, check to see if we need more forwards thrust
                if (isAutoCruiseEnabled && shipControlModule.IsInitialised && !isLongitudinalDataDiscarded && shipControlModule.ShipIsEnabled())
                {
                    currentSpeed = (shipControlModule.shipInstance.TransformInverseRotation * shipControlModule.shipInstance.WorldVelocity).z;
                    if (shipInput.longitudinal == 0f)
                    {
                        /// TODO - as the current speed approaches the target speed we need to apply less thrust.
                        if (targetCruiseSpeed > 0f && currentSpeed < targetCruiseSpeed)
                        {
                            shipInput.longitudinal = 0.25f;
                        }
                    }
                    else
                    {
                        targetCruiseSpeed = currentSpeed;
                    }
                }

                shipControlModule.SendInput(shipInput);
            }
            #endregion
        }

        #endregion

        #region Event Methods

        private void OnEnable()
        {
            #if SCSM_XR && SSC_UIS
            if (inputMode == InputMode.UnityXR)
            {
                EnableOrDisableActions(inputActionAssetXR, true);
            }
            #endif
        }

        private void OnDisable()
        {
            #if SCSM_XR && SSC_UIS
            if (inputMode == InputMode.UnityXR)
            {
                EnableOrDisableActions(inputActionAssetXR, false);
            }
            #endif
        }

        private void OnDestroy()
        {
            #if SCSM_XR && SSC_UIS
            if (inputMode == InputMode.UnityXR)
            {
                EnableOrDisableActions(inputActionAssetXR, false);
            }
            #endif
        }

        #endregion

        #region Private or Internal Member Methods

        /// <summary>
        /// Calculates a mouse X input for an axis, taking into account sensitivity and deadzone settings.
        /// In a build, can also compensate for getting input from another display or monitor.
        /// See also SetTargetDisplay(displayNumber).
        /// </summary>
        /// <param name="mouseX"></param>
        /// <param name="mouseXSensitivity"></param>
        /// <param name="mouseXDeadzone"></param>
        /// <returns></returns>
        private float CalculateMouseXInput (float mouseX, float mouseXSensitivity, float mouseXDeadzone)
        {
            // TODO call this from Unity Input System code
            // Mouse position can be outside gameview

            #if !UNITY_EDITOR
            mouseX -= targetDisplayOffsetX;
            #endif

            // Make sure to clamp it to be inside bounds of the screen
            if (mouseX < 0f) { mouseX = 0f; }
            else if (mouseX > Screen.width) { mouseX = Screen.width; }
            // Normalise between -1.0 and +1.0
            mouseX = ((mouseX - (Screen.width / 2f)) * 2f / Screen.width);
            // Make any values inside the deadzone zero
            if (mouseX > -mouseXDeadzone * 2f && mouseX < mouseXDeadzone * 2f) { mouseX = 0; }

            // Multiply by sensitivity
            return mouseX * mouseXSensitivity;
        }

        /// <summary>
        /// Calculates a mouse Y input for an axis, taking into account sensitivity and deadzone settings.
        /// </summary>
        /// <param name="mouseX"></param>
        /// <param name="mouseXSensitivity"></param>
        /// <param name="mouseXDeadzone"></param>
        /// <returns></returns>
        private float CalculateMouseYInput(float mouseY, float mouseYSensitivity, float mouseYDeadzone)
        {
            // TODO call this from Unity Input System code
            // Mouse position can be outside gameview
            // Make sure to clamp it to be inside bounds of the screen
            if (mouseY < 0f) { mouseY = 0f; }
            else if (mouseY > Screen.height) { mouseY = Screen.height; }
            // Normalise between -1.0 and +1.0
            mouseY = -((mouseY - (Screen.height / 2f)) * 2f / Screen.height);
            // Make any values inside the deadzone zero
            if (mouseY > -mouseYDeadzone * 2f && mouseY < mouseYDeadzone * 2f) { mouseY = 0; }
            // Multiply by sensitivity
            return mouseY * mouseYSensitivity;
        }

        /// <summary>
        /// Attempt to enable or disable Ship AI mode.
        /// When AI mode is enabled, the Custom Inputs are still enabled.
        /// </summary>
        /// <param name="isEnabled"></param>
        private void EnableOrDisableAIMode(bool isEnabled)
        {
            ShipAIInputModule aiShip = shipControlModule == null ? null : shipControlModule.GetShipAIInputModule(false);

            if (aiShip != null)
            {
                if (aiShip.IsInitialised)
                {
                    if (isEnabled)
                    {
                        DisableInput(true);
                        aiShip.EnableAI();
                        isShipAIModeEnabled = true;
                    }
                    else
                    {
                        aiShip.DisableAI();
                        EnableInput();
                        isShipAIModeEnabled = false;
                    }
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ERROR: PlayerInputModule.EnableOrDisableAIMode - cannot " + (isEnabled ? "enable" : "disable") + " AI mode as Ship AI Input Module is not initialised"); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: PlayerInputModule.EnableOrDisableAIMode - cannot " + (isEnabled ? "enable" : "disable") + " AI mode as Ship AI Input Module may not be attached to " + gameObject.name); }
            #endif
        }

#if SSC_UIS
         
        /// <summary>
        /// Enable or disable Actions in a InputActionAsset
        /// </summary>
        /// <param name="inputActionAsset"></param>
        /// <param name="isEnabled"></param>
        private void EnableOrDisableActions(UnityEngine.InputSystem.InputActionAsset inputActionAsset, bool isEnabled)
        {
            if (inputActionAsset != null)
            {
                if (isEnabled)
                {
                    inputActionAsset.Enable();
                }
                else
                {
                    inputActionAsset.Disable();
                }
            }
        }

        /// <summary>
        /// Get an InputAction from a scriptableobject InputActionAsset in a given ActionMap. We are using strings so DO NOT run this every frame.
        /// </summary>
        /// <param name="inputActionAsset"></param>
        /// <param name="actionMapId"></param>
        /// <param name="actionId"></param>
        /// <returns></returns>
        private UnityEngine.InputSystem.InputAction UISGetAction(UnityEngine.InputSystem.InputActionAsset inputActionAsset, string actionMapId, string actionId)
        {
            UnityEngine.InputSystem.InputAction inputAction = null;

            if (inputActionAsset != null && !string.IsNullOrEmpty(actionMapId) || !string.IsNullOrEmpty(actionId))
            {
                UnityEngine.InputSystem.InputActionMap actionMap = inputActionAsset.FindActionMap(actionMapId);

                if (actionMap != null)
                {
                    inputAction = actionMap.FindAction(actionId);
                }
            }

            return inputAction;
        }
        
        /// <summary>
        /// Given a Unity Input System action, read the current value being generated by that
        /// action. This assumes the inputAction will be null if it has not been configured
        /// in the PlayerInputModuleEditor. It doesn't check the inputAxisMode.
        /// </summary>
        /// <param name="inputAction"></param>
        /// <param name="inputActionDataSlot"></param>
        /// <param name="actionControlTypeHash"></param>
        /// <returns></returns>
        private float UISReadActionInputFloat(UnityEngine.InputSystem.InputAction inputAction, int inputActionDataSlot, int actionControlTypeHash)
        {
            float inputActionValue = 0f;

            // Unity Input System only requires SingleAxis as multiple can be supported by
            // using Composite Bindings. It doesn't check if the inputAxisMode is NoInput or CombinedAxis.

            if (inputAction != null)
            {
                // Use int32 hashcodes to avoid GC
                if (actionControlTypeHash == controlTypeAxisHashUIS)
                {
                    inputActionValue = inputAction.ReadValue<float>();
                }
                else if (actionControlTypeHash == controlTypeVector2HashUIS || actionControlTypeHash == controlTypeDpadHashUIS)
                {
                    if (inputActionDataSlot == 0)
                    {
                        inputActionValue = inputAction.ReadValue<Vector2>().x;
                    }
                    else { inputActionValue = inputAction.ReadValue<Vector2>().y; }
                }
                else if (actionControlTypeHash == controlTypeVector3HashUIS)
                {
                    if (inputActionDataSlot == 0)
                    {
                        inputActionValue = inputAction.ReadValue<Vector3>().x;
                    }
                    else if (inputActionDataSlot == 1)
                    {
                        inputActionValue = inputAction.ReadValue<Vector3>().y;
                    }
                    else { inputActionValue = inputAction.ReadValue<Vector3>().z; }
                }
                else if (actionControlTypeHash == controlTypeButtonHashUIS)
                {
                    // Buttons return a float (not a bool)
                    inputActionValue = inputAction.ReadValue<float>();
                }
            }

            return inputActionValue;
        }

        /// <summary>
        /// Given a Unity Input System action, read the current value being generated by that
        /// action. This assumes the inputAction will be null if it has not been configured
        /// in the PlayerInputModuleEditor. It doesn't check the inputAxisMode or
        /// [primary/secondary]FireButtonEnabled values.
        /// </summary>
        /// <param name="inputAction"></param>
        /// <param name="inputActionDataSlot"></param>
        /// <param name="actionControlTypeHash"></param>
        /// <param name="canBeHeldDown"></param>
        /// <returns></returns>
        private bool UISReadActionInputBool(UnityEngine.InputSystem.InputAction inputAction, int inputActionDataSlot, int actionControlTypeHash, bool canBeHeldDown)
        {
            bool inputActioned = false;
            float inputActionValue = 0f;

            // It doesn't check if the primary/secondaryFireButtonEnabled are enabled.

            if (inputAction != null)
            {
                // Use int32 hashcodes to avoid GC
                if (actionControlTypeHash == controlTypeButtonHashUIS)
                {
                    if (canBeHeldDown)
                    {
                        // Buttons return a float (not a bool)
                        inputActionValue = inputAction.ReadValue<float>();
                        inputActioned = inputActionValue < -0.1f || inputActionValue > 0.1f;
                    }
                    // Only respond the first time it is pressed
                    else { inputActioned = inputAction.triggered; }
                }
                else if (actionControlTypeHash == controlTypeAxisHashUIS || actionControlTypeHash == controlTypeDpadHashUIS)
                {
                    inputActionValue = inputAction.ReadValue<float>();
                    inputActioned = inputActionValue > 0.1f || inputActionValue < 0.1f;
                }
                else if (actionControlTypeHash == controlTypeVector2HashUIS)
                {
                    if (inputActionDataSlot == 0)
                    {
                        inputActionValue = inputAction.ReadValue<Vector2>().x;
                    }
                    else { inputActionValue = inputAction.ReadValue<Vector2>().y; }
                    inputActioned = inputActionValue > 0.1f || inputActionValue < 0.1f;
                }
                else if (actionControlTypeHash == controlTypeVector3HashUIS)
                {
                    if (inputActionDataSlot == 0)
                    {
                        inputActionValue = inputAction.ReadValue<Vector3>().x;
                    }
                    else if (inputActionDataSlot == 1)
                    {
                        inputActionValue = inputAction.ReadValue<Vector3>().y;
                    }
                    else { inputActionValue = inputAction.ReadValue<Vector3>().z; }
                    inputActioned = inputActionValue < -0.1f || inputActionValue > 0.1f;
                }
            }

            return inputActioned;
        }

#endif

        #endregion

        #region Private XR Methods


        #if SCSM_XR && SSC_UIS

        private void ConfigureLeftHandXR()
        {
            // Prevent XR hands causing damage when colliding with ship
            if (leftHandTransformXR != null)
            {
                // The ship shouldn't have to be initialised for this to work
                shipControlModule.shipInstance.AttachColliders(leftHandTransformXR.GetComponentsInChildren<Collider>());
            }
        }

        private void ConfigureRightHandXR()
        {
            // Prevent XR hands causing damage when colliding with ship
            if (rightHandTransformXR != null)
            {
                // The ship shouldn't have to be initialised for this to work
                shipControlModule.shipInstance.AttachColliders(rightHandTransformXR.GetComponentsInChildren<Collider>());
            }
        }

        /// <summary>
        /// Attempt to enable or disable the XRCamera
        /// </summary>
        /// <param name="isEnabled"></param>
        private void EnableOrDisableXRCamera(bool isEnabled)
        {
            if (firstPersonTransform1XR != null) { firstPersonTransform1XR.gameObject.SetActive(isEnabled); }
        }

        /// <summary>
        /// Attempt to enable or disble the XR Hands
        /// </summary>
        /// <param name="isEnabled"></param>
        private void EnableOrDisableXRHands(bool isEnabled)
        {
            if (leftHandTransformXR != null) { leftHandTransformXR.gameObject.SetActive(isEnabled); }
            if (rightHandTransformXR != null) { rightHandTransformXR.gameObject.SetActive(isEnabled); }
        }

        #endif

        #endregion

        #region Public Member Methods

        /// <summary>
        /// Validate all the legacy input axis names. Update their status.
        /// </summary>
        public void ValidateLegacyInput()
        {
            isPositiveHorizontalInputAxisValid = IsLegacyAxisValid(positiveHorizontalInputAxisName, false);
            isNegativeHorizontalInputAxisValid = IsLegacyAxisValid(negativeHorizontalInputAxisName, false);
            isPositiveVerticalInputAxisValid = IsLegacyAxisValid(positiveVerticalInputAxisName, false);
            isNegativeVerticalInputAxisValid = IsLegacyAxisValid(negativeVerticalInputAxisName, false);
            isPositiveLongitudinalInputAxisValid = IsLegacyAxisValid(positiveLongitudinalInputAxisName, false);
            isNegativeLongitudinalInputAxisValid = IsLegacyAxisValid(negativeLongitudinalInputAxisName, false);
            isPositivePitchInputAxisValid = IsLegacyAxisValid(positivePitchInputAxisName, false);
            isNegativePitchInputAxisValid = IsLegacyAxisValid(negativePitchInputAxisName, false);
            isPositiveYawInputAxisValid = IsLegacyAxisValid(positiveYawInputAxisName, false);
            isNegativeYawInputAxisValid = IsLegacyAxisValid(negativeYawInputAxisName, false);
            isPositiveRollInputAxisValid = IsLegacyAxisValid(positiveRollInputAxisName, false);
            isNegativeRollInputAxisValid = IsLegacyAxisValid(negativeRollInputAxisName, false);
            isPrimaryFireInputAxisValid = IsLegacyAxisValid(primaryFireInputAxisName, false);
            isSecondaryFireInputAxisValid = IsLegacyAxisValid(secondaryFireInputAxisName, false);
            isDockingInputAxisValid = IsLegacyAxisValid(dockingInputAxisName, false);
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Check that a Unity Legacy Input system axis is defined
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="showError"></param>
        /// <returns></returns>
        public static bool IsLegacyAxisValid(string axisName, bool showError)
        {
            bool isValid = true;

            try
            {
                #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
                Input.GetAxis(axisName);
                #else
                isValid = false;
                #endif
            }
            catch (System.Exception ex)
            {
                isValid = false;
                if (showError) { Debug.LogWarning("PlayerInputModule: " + ex.Message); }
            }

            return isValid;
        }

        #endregion

        #region Public Static Rewired Integration Methods
        #if SSC_REWIRED

        /// <summary>
        /// Verify that Rewired's Input Manager is in the scene
        /// and has been initialised.
        /// </summary>
        /// <param name="showErrors"></param>
        /// <returns></returns>
        public static bool CheckRewired(bool showErrors)
        {
            bool isInitialised = false;

            try
            {
                isInitialised = Rewired.ReInput.isReady;
            }
            catch (System.Exception ex)
            {
                #if UNITY_EDITOR
                if (showErrors) { Debug.LogWarning("SSC.PlayerInputModule.CheckRewired - Rewired Input Manager is not initialised.\n" + ex.Message); }
                #else
                // Keep compiler happy
                if (ex != null) {}
                #endif
            }

            return isInitialised;
        }

        /// <summary>
        /// Get the Rewired Player class instance given a human user number.
        /// e.g. Get the first human player. 
        /// Rewired.Player rewiredPlayer = PlayerInputModule.GetRewiredPlayer(1, false);
        /// </summary>
        /// <param name="userNumber"></param>
        /// <param name="showErrors"></param>
        /// <returns></returns>
        public static Rewired.Player GetRewiredPlayer(int userNumber, bool showErrors)
        {
            Rewired.Player rwPlayer = null;

            if (CheckRewired(showErrors))
            {
                int numRewiredPlayers = Rewired.ReInput.players == null ? 0 : Rewired.ReInput.players.playerCount;

                if (userNumber < 1 || userNumber > numRewiredPlayers)
                {
                    #if UNITY_EDITOR
                    if (showErrors)
                    {
                        if (numRewiredPlayers < 1)
                        {
                            Debug.LogWarning("ERROR: PlayerInputModule.GetRewiredPlayer - there does not seem to be any Players setup in Rewired. Check the Players tab in the Rewired Editor.");
                        }
                        else
                        {
                            Debug.LogWarning("ERROR: PlayerInputModule.GetRewiredPlayer - invalid UserNumber " + userNumber);
                        }
                    }
                    #endif
                }
                else
                {
                    rwPlayer = Rewired.ReInput.players.Players[userNumber - 1];
                }
            }

            return rwPlayer;
        }

        #endif
        #endregion Rewired Integration Methods

        #region Public API Methods

        #region Public API - General

        /// <summary>
        /// This must be called on Awake or via code before the Player Input Module can be used.
        /// </summary>
        public void Initialise()
        {
            if (IsInitialised) { return; }

            shipControlModule = GetComponent<ShipControlModule>();
            shipInput = new ShipInput();
            lastShipInput = new ShipInput();

            ReinitialiseDiscardData();

            #if UNITY_EDITOR
            bool showErrors = true;
            #else
            bool showErrors = false;
            #endif

            if (inputMode == InputMode.UnityInputSystem)
            {
                #if SSC_UIS
                UpdateUnityInputSystemActions(showErrors);
                #endif
            }
            else if (inputMode == InputMode.Rewired)
            {
                #if SSC_REWIRED

                UpdateRewiredActionTypes(showErrors);
                
                if (rewiredPlayerNumber > 0)
                {
                    // The player number has been configured in the editor, so attempt to find the rewired player
                    SetRewiredPlayer(rewiredPlayerNumber, showErrors);

                   //Debug.Log("[DEBUG] roll: " +  Rewired.ReInput.mapping.GetAction(negativeRollInputActionId).descriptiveName);
                }
                #else
                #if UNITY_EDITOR
                Debug.LogWarning("PlayerInputModule.Awake - the InputMode is set to Rewired on " + gameObject.name + ", however Rewired does not seem to be installed in this project.");
                #endif                
                #endif
            }
            else if (inputMode == InputMode.OculusAPI)
            {
                #if SSC_OVR
                // OVRInput requires either OVRManager to be present in the scene OR OVRInput.Update() to called once per frame at the beginning of any component's Update() method.
                CheckOVRManager();
                #endif
            }
            else if (inputMode == InputMode.Vive)
            {
                #if VIU_PLUGIN
                UpdateViveInput(showErrors);
                #else
                #if UNITY_EDITOR
                Debug.LogWarning("PlayerInputModule.Awake - the InputMode is set to Vive on " + gameObject.name + ", however Vive Input Utility does not seem to be installed in this project.");
                #endif 
                #endif
            }
            else if (inputMode == InputMode.UnityXR)
            {
                #if SCSM_XR && SSC_UIS
                UpdateUnityXRActions(showErrors);
                EnableOrDisableActions(inputActionAssetXR, true);
                ConfigureLeftHandXR();
                ConfigureRightHandXR();
                #endif
            }
            else if (inputMode == InputMode.LegacyUnity)
            {
                ValidateLegacyInput();
            }

            ReinitialiseRumble();
            ReinitialiseCustomPlayerInput();

            // Keep compiler happy
            if (showErrors) { }
            if (isOVRInputUpdatedIfRequired) { }
          
            if (isEnabledOnInitialise)
            {
                isCustomPlayerInputsOnlyEnabled = isCustomInputsOnlyOnInitialise;
                isInputEnabled = !isCustomInputsOnlyOnInitialise;
            }
            else
            {
                isInputEnabled = false;
                // This option is only available when isEnabledOnInitialise is true
                // This restriction is enforced in the editor
                isCustomPlayerInputsOnlyEnabled = false;

                DisableXRCamera();
                DisableXRHands();
            }          

            IsInitialised = true;
        }

        /// <summary>
        /// Enable the Player Input Module to receive input from the configured device.
        /// The module will be initialised if it isn't already.
        /// </summary>
        public void EnableInput()
        {
            if (!IsInitialised) { Initialise(); }

            if (IsInitialised)
            {
                isInputEnabled = true;
                isCustomPlayerInputsOnlyEnabled = false;
            }
        }

        /// <summary>
        /// Disable input or stop the Player Input Module from receiving input from the configured device.
        /// When allowCustomPlayerInput is true, all other input except CustomPlayerInputs are ignored.
        /// This can be useful when you want to still receive actions that generally don't involve ship movement.
        /// </summary>
        /// <param name="allowCustomPlayerInput"></param>
        public void DisableInput(bool allowCustomPlayerInput = false)
        {
            if (IsInitialised)
            {
                ResetInput();
                isInputEnabled = false;
                isCustomPlayerInputsOnlyEnabled = allowCustomPlayerInput;
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: PlayerInputModule.DisableInput() was called before Awake() or Initialise() ran. DisableInput will have no effect."); }
            #endif
        }

        /// <summary>
        /// Call this when you wish to remove any custom event listeners, like
        /// after creating them in code and then destroying the object.
        /// You could add this to your game play OnDestroy code.
        /// </summary>
        public void RemoveListeners()
        {
            if (IsInitialised && numberOfCustomPlayerInputs > 0)
            {
                for (int cpi = 0; cpi < numberOfCustomPlayerInputs; cpi++)
                {
                    CustomPlayerInput customPlayerInput = customPlayerInputList[cpi];

                    if (customPlayerInput != null && customPlayerInput.customPlayerInputEvt != null)
                    {
                        customPlayerInput.customPlayerInputEvt.RemoveAllListeners();
                    }
                }
            }
        }

        /// <summary>
        /// Reset and send 0 input on each axis to the ship
        /// Also calls ReinitialiseDiscardData().
        /// </summary>
        public void ResetInput()
        {
            if (IsInitialised)
            {
                shipInput.horizontal = 0f;
                shipInput.vertical = 0f;
                shipInput.longitudinal = 0f;
                shipInput.pitch = 0f;
                shipInput.yaw = 0f;
                shipInput.roll = 0f;
                shipInput.primaryFire = false;
                shipInput.secondaryFire = false;
                shipInput.dock = false;

                lastShipInput.horizontal = 0f;
                lastShipInput.vertical = 0f;
                lastShipInput.longitudinal = 0f;
                lastShipInput.pitch = 0f;
                lastShipInput.yaw = 0f;
                lastShipInput.roll = 0f;
                lastShipInput.primaryFire = false;
                lastShipInput.secondaryFire = false;
                lastShipInput.dock = false;

                targetCruiseSpeed = 0f;

                ReinitialiseDiscardData();

                if (shipControlModule != null)
                {
                    shipControlModule.SendInput(shipInput);
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: PlayerInputModule.ResetInput() was called before Awake() or Initialise() ran. ResetInput will have no effect."); }
            #endif
        }

        /// <summary>
        /// Re-initialise (set) the shipInput based on the is[axis/button]DataDiscard field settings.
        /// Must be called after each change to any of those fields/variables.
        /// Retrospectively also updates lastShipInput with same DataDiscard values.
        /// </summary>
        public void ReinitialiseDiscardData()
        {
            shipInput.isHorizontalDataEnabled = !isHorizontalDataDiscarded;
            shipInput.isVerticalDataEnabled = !isVerticalDataDiscarded;
            shipInput.isLongitudinalDataEnabled = !isLongitudinalDataDiscarded;
            shipInput.isPitchDataEnabled = !isPitchDataDiscarded;
            shipInput.isYawDataEnabled = !isYawDataDiscarded;
            shipInput.isRollDataEnabled = !isRollDataDiscarded;
            shipInput.isPrimaryFireDataEnabled = !isPrimaryFireDataDiscarded;
            shipInput.isSecondaryFireDataEnabled = !isSecondaryFireDataDiscarded;
            shipInput.isDockDataEnabled = !isDockDataDiscarded;

            lastShipInput.isHorizontalDataEnabled = !isHorizontalDataDiscarded;
            lastShipInput.isVerticalDataEnabled = !isVerticalDataDiscarded;
            lastShipInput.isLongitudinalDataEnabled = !isLongitudinalDataDiscarded;
            lastShipInput.isPitchDataEnabled = !isPitchDataDiscarded;
            lastShipInput.isYawDataEnabled = !isYawDataDiscarded;
            lastShipInput.isRollDataEnabled = !isRollDataDiscarded;
            lastShipInput.isPrimaryFireDataEnabled = !isPrimaryFireDataDiscarded;
            lastShipInput.isSecondaryFireDataEnabled = !isSecondaryFireDataDiscarded;
            lastShipInput.isDockDataEnabled = !isDockDataDiscarded;
        }

        /// <summary>
        /// This should be called if you modify the CustomPlayerInputs at runtime
        /// </summary>
        public void ReinitialiseCustomPlayerInput()
        {
            numberOfCustomPlayerInputs = customPlayerInputList == null ? 0 : customPlayerInputList.Count;
        }

        /// <summary>
        /// Used on a PC with multiple screens (display monitors) to adjust mouse position when player
        /// wants to use a screen other than Display 1.
        /// </summary>
        /// <param name="displayNumber">1 to 8</param>
        public void SetTargetDisplay(int displayNumber)
        {
            targetDisplayOffsetX = 0f;

            #if UNITY_EDITOR
            // keep compiler happy - do nothing
            if (targetDisplayOffsetX > 0f) { }
            #else
            // In the editor it reports the mouse position within the (single) game view which has focus.
            // In a build the mouse position spans all displays.
            if (IsInitialised && SSCUtils.VerifyTargetDisplay(displayNumber, true))
            {
                for (int tgtdIdx = 0; tgtdIdx < displayNumber - 1; tgtdIdx++)
                {
                    targetDisplayOffsetX += Display.displays[tgtdIdx].renderingWidth;
                }
            }
            #endif
        }

        #endregion

        #region Public API - Static General

        /// <summary>
        /// Calculates an input for this frame for an axis, taking into account sensitivity and gravity settings.
        /// </summary>
        /// <param name="currentInput"></param>
        /// <param name="targetInput"></param>
        /// <param name="axisSensitivity"></param>
        /// <param name="axisGravity"></param>
        /// <returns></returns>
        public static float CalculateAxisInput (float currentInput, float targetInput, float axisSensitivity, float axisGravity)
        {
            // Check that the current input is different from the target input
            if (currentInput != targetInput)
            {
                #region Pre-Calculation

                // Check which side of the origin the current input position is
                bool currentInputPositive = currentInput > 0f;
                // Check which side of the origin the target input position is
                bool targetInputPositive = targetInput > 0f;
                // Calculate the distance from the origin to the current input position
                float currentDistFromOrigin = currentInputPositive ? currentInput : -currentInput;
                // Calculate the distance from the origin to the target input position
                float targetDistFromOrigin = targetInputPositive ? targetInput : -targetInput;

                #endregion

                #region Calculate Speed

                // Now, calculate speed to move towards target position at
                float inputMoveSpeed = 0f;
                // If the current input position and the target input position are on opposite sides of the origin,
                // move towards target position at speed of sensitivity + gravity
                if (currentInputPositive != targetInputPositive && currentInput != 0f && targetInput != 0f)
                {
                    inputMoveSpeed = axisSensitivity + axisGravity;
                }
                // Otherwise, if the current input position and the target input position are on the same side of the origin:
                // If the target input position is farther from the origin than the actual input position,
                // move towards target position at speed of sensitivity
                else if (targetDistFromOrigin > currentDistFromOrigin)
                {
                    inputMoveSpeed = axisSensitivity;
                }
                // If the target input position is closer to the origin than the actual input position,
                // move towards target position at speed of gravity
                else
                {
                    inputMoveSpeed = axisGravity;
                }

                #endregion

                #region Calculate New input

                // Calculate the difference between the target input and the current input
                float targetInputDelta = targetInput - currentInput;
                float absTargetInputDelta = targetInputDelta > 0f ? targetInputDelta : -targetInputDelta;
                // Calculate how far the input is able to move this frame, purely due to speed constraints
                float thisFrameInputMovement = inputMoveSpeed * Time.deltaTime;
                // If the possible frame movement is more than the difference between the target input and the current input,
                // simply jump straight to the target input
                if (thisFrameInputMovement > absTargetInputDelta)
                {
                    return targetInput;
                }
                // Otherwise, simply add this frame's input movement to the current input
                else
                {
                    // If the difference between the target input and the current input is negative, 
                    // apply a negative movement
                    return currentInput + (targetInputDelta > 0f ? thisFrameInputMovement : -thisFrameInputMovement);
                }

                #endregion
            }
            else
            {
                // If the current input is the same as the target input, leave it unchanged
                return currentInput;
            }
        }

        #endregion

        #region Public API Oculus API

        /// <summary>
        /// Check if the OVRManager is in the scene. If Oculus API is not installed,
        /// it does nothing. If the OVRManager is add or removed from the scene at runtime,
        /// call this method after making the change.
        /// </summary>
        public void CheckOVRManager()
        {
            #if SSC_OVR

            #if UNITY_2022_2_OR_NEWER
            if (FindFirstObjectByType<OVRManager>() == null)
            #else
            if (FindObjectOfType<OVRManager>() == null)
            #endif
            {
                isOVRInputUpdateRequired = isOVRInputUpdatedIfRequired;
                isOVRManagerPresent = false;
            }
            else
            {
                isOVRManagerPresent = true;
                // If the OVRManager is in the scene, we never need to call OVRInput.Update() in PlayerInputModule.
                isOVRInputUpdateRequired = false;
            }
            #endif
        }


        #endregion

        #region Public API Unity Input System Methods
        #if SSC_UIS

        /// <summary>
        /// Get the Unity Input System PlayerInput component attached to this (player) ship.
        /// The component reference is cached so that GetComponent isn't called multiple times.
        /// </summary>
        /// <returns></returns>
        public UnityEngine.InputSystem.PlayerInput GetUISPlayerInput()
        {
            if (uisPlayerInput == null) { uisPlayerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>(); }

            return uisPlayerInput;
        }

        #endif

        /// <summary>
        /// Used at runtime to convert string unique identifiers for actions (GUIDs) into
        /// Unity Input System ActionInputs to avoid looking them up and incurring GC overhead each Update.
        /// If actions are modified at runtime, call this method.
        /// Has no effect if Unity Input System package is not installed.
        /// </summary>
        /// <param name="showErrors"></param>
        public void UpdateUnityInputSystemActions(bool showErrors = false)
        {
            #if SSC_UIS

            #if UNITY_EDITOR
            string methodName = "PlayerInputModule.UpdateUnityInputSystemActions";
            #endif

            // Get the hashcodes for the Control Types that we support
            controlTypeButtonHashUIS = SSCMath.GetHashCode("Button");
            controlTypeAxisHashUIS = SSCMath.GetHashCode("Axis");
            // Dpad and Vector2 are equivalent
            controlTypeDpadHashUIS = SSCMath.GetHashCode("Dpad");
            controlTypeVector2HashUIS = SSCMath.GetHashCode("Vector2");
            controlTypeVector3HashUIS = SSCMath.GetHashCode("Vector3");

            // Make sure PlayerInput component is available
            // The current version of SSC depends on this Unity component to be attached to the same gameobject
            if (GetUISPlayerInput() != null)
            {
                int numCPI = customPlayerInputList == null ? 0 : customPlayerInputList.Count;

                if (uisPlayerInput.actions != null)
                {
                    //UnityEngine.InputSystem.InputAction inputAction = null;

                    positiveHorizontalInputActionUIS = uisPlayerInput.actions.FindAction(positiveHorizontalInputActionIdUIS);
                    positiveVerticalInputActionUIS = uisPlayerInput.actions.FindAction(positiveVerticalInputActionIdUIS);
                    positiveLongitudinalInputActionUIS = uisPlayerInput.actions.FindAction(positiveLongitudinalInputActionIdUIS);
                    positivePitchInputActionUIS = uisPlayerInput.actions.FindAction(positivePitchInputActionIdUIS);
                    positiveYawInputActionUIS = uisPlayerInput.actions.FindAction(positiveYawInputActionIdUIS);
                    positiveRollInputActionUIS = uisPlayerInput.actions.FindAction(positiveRollInputActionIdUIS);
                    primaryFireInputActionUIS = uisPlayerInput.actions.FindAction(primaryFireInputActionIdUIS);
                    secondaryFireInputActionUIS = uisPlayerInput.actions.FindAction(secondaryFireInputActionIdUIS);
                    quitInputActionUIS = uisPlayerInput.actions.FindAction(quitInputActionIdUIS);
                    dockingInputActionUIS = uisPlayerInput.actions.FindAction(dockingInputActionIdUIS);

                    positiveHorizontalCtrlTypeHashUIS = positiveHorizontalInputActionUIS == null ? 0 : SSCMath.GetHashCode(positiveHorizontalInputActionUIS.expectedControlType);
                    positiveVerticalCtrlTypeHashUIS = positiveVerticalInputActionUIS == null ? 0 : SSCMath.GetHashCode(positiveVerticalInputActionUIS.expectedControlType);
                    positiveLongitudinalCtrlTypeHashUIS = positiveLongitudinalInputActionUIS == null ? 0 : SSCMath.GetHashCode(positiveLongitudinalInputActionUIS.expectedControlType);
                    positivePitchCtrlTypeHashUIS = positivePitchInputActionUIS == null ? 0 : SSCMath.GetHashCode(positivePitchInputActionUIS.expectedControlType);
                    positiveYawCtrlTypeHashUIS = positiveYawInputActionUIS == null ? 0 : SSCMath.GetHashCode(positiveYawInputActionUIS.expectedControlType);
                    positiveRollCtrlTypeHashUIS = positiveRollInputActionUIS == null ? 0 : SSCMath.GetHashCode(positiveRollInputActionUIS.expectedControlType);
                    primaryFireCtrlTypeHashUIS = primaryFireInputActionUIS == null ? 0 : SSCMath.GetHashCode(primaryFireInputActionUIS.expectedControlType);
                    secondaryFireCtrlTypeHashUIS = secondaryFireInputActionUIS == null ? 0 : SSCMath.GetHashCode(secondaryFireInputActionUIS.expectedControlType);
                    dockingCtrlTypeHashUIS = dockingInputActionUIS == null ? 0 : SSCMath.GetHashCode(dockingInputActionUIS.expectedControlType);

                    // Update all the Custom Player Inputs
                    for (int cpiIdx = 0; cpiIdx < numCPI; cpiIdx++)
                    {
                        CustomPlayerInput customPlayerInput = customPlayerInputList[cpiIdx];
                        if (customPlayerInput != null)
                        {
                            customPlayerInput.uisPositiveInputAction = uisPlayerInput.actions.FindAction(customPlayerInput.uisPositiveInputActionId);
                            customPlayerInput.uisPositiveCtrlTypeHash = customPlayerInput.uisPositiveInputAction == null ? 0 : SSCMath.GetHashCode(customPlayerInput.uisPositiveInputAction.expectedControlType);
                            customPlayerInput.lastInputValueX = 0f;
                            customPlayerInput.lastInputValueY = 0f;
                        }
                    }
                }
                else
                {
                    // set all actions to null
                    positiveHorizontalInputActionUIS = null;
                    positiveVerticalInputActionUIS = null;
                    positiveLongitudinalInputActionUIS = null;
                    positivePitchInputActionUIS = null;
                    positiveYawInputActionUIS = null;
                    positiveRollInputActionUIS = null;
                    primaryFireInputActionUIS = null;
                    secondaryFireInputActionUIS = null;
                    quitInputActionUIS = null;
                    dockingInputActionUIS = null;

                    positiveHorizontalCtrlTypeHashUIS = 0;
                    positiveVerticalCtrlTypeHashUIS = 0;
                    positiveLongitudinalCtrlTypeHashUIS = 0;
                    positivePitchCtrlTypeHashUIS = 0;
                    positiveYawCtrlTypeHashUIS = 0;
                    positiveRollCtrlTypeHashUIS = 0;
                    primaryFireCtrlTypeHashUIS = 0;
                    secondaryFireCtrlTypeHashUIS = 0;
                    dockingCtrlTypeHashUIS = 0;

                    // Update all the Custom Player Inputs
                    for (int cpiIdx = 0; cpiIdx < numCPI; cpiIdx++)
                    {
                        CustomPlayerInput customPlayerInput = customPlayerInputList[cpiIdx];
                        if (customPlayerInput != null)
                        {
                            customPlayerInput.uisPositiveInputAction = null;
                            customPlayerInput.uisPositiveCtrlTypeHash = 0;
                        }
                    }

                    #if UNITY_EDITOR
                    if (showErrors) { Debug.LogWarning(methodName + " - PlayerInput component attached to " + this.name + " has no Input Action Asset."); }
                    #endif
                }
            }
            #if UNITY_EDITOR
            else { if (showErrors) { Debug.LogWarning(methodName + " couldn't find PlayerInput component attached to " + this.name); } }
            #endif

            #endif
        }

        #endregion

        #region Public API Rewired Methods
        /// <summary>
        /// Set the human player number. This should be 1 or greater. The player
        /// must be first assigned in Rewired, before this is called.
        /// NOTE: If Rewired is not installed or the InputMode is not Rewired,
        /// the rewiredPlayerNumber is set to 0 (unassigned).
        /// </summary>
        /// <param name="playerNumber"></param>
        /// <param name="showErrors"></param>
        public void SetRewiredPlayer(int playerNumber, bool showErrors = false)
        {
            if (playerNumber > 0)
            {
                #if SSC_REWIRED
                rewiredPlayer = GetRewiredPlayer(playerNumber, showErrors);
                // Only assign player number if the rewired player is found
                rewiredPlayerNumber = rewiredPlayer == null ? 0 : playerNumber;
                #else
                rewiredPlayerNumber = playerNumber;
                #endif
            }
            else
            {
                rewiredPlayerNumber = 0;
                #if SSC_REWIRED
                rewiredPlayer = null;
                #endif
            }
        }

        /// <summary>
        /// A Rewired Action can be an Axis or a Button. To avoid looking up the Action
        /// within the Update event, the type is set outside the loop and need only be
        /// called once at runtime for this ship, and then when ever the Actions are
        /// changed. Has no effect if Rewired is not installed.
        /// </summary>
        public void UpdateRewiredActionTypes(bool showErrors)
        {
            #if SSC_REWIRED
            if (CheckRewired(showErrors))
            {
                Rewired.InputAction inputAction = null;
                inputAction = Rewired.ReInput.mapping.GetAction(positiveHorizontalInputActionId);
                if (inputAction != null) { positiveHorizontalInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(negativeHorizontalInputActionId);
                if (inputAction != null) { negativeHorizontalInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(positiveVerticalInputActionId);
                if (inputAction != null) { positiveVerticalInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(negativeVerticalInputActionId);
                if (inputAction != null) { negativeVerticalInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(positiveLongitudinalInputActionId);
                if (inputAction != null) { positiveLongitudinalInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(negativeLongitudinalInputActionId);
                if (inputAction != null) { negativeLongitudinalInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(positivePitchInputActionId);
                if (inputAction != null) { positivePitchInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(negativePitchInputActionId);
                if (inputAction != null) { negativePitchInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(positiveYawInputActionId);
                if (inputAction != null) { positiveYawInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(negativeYawInputActionId);
                if (inputAction != null) { negativeYawInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(positiveRollInputActionId);
                if (inputAction != null) { positiveRollInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(negativeRollInputActionId);
                if (inputAction != null) { negativeRollInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(primaryFireInputActionId);
                if (inputAction != null) { primaryFireInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(secondaryFireInputActionId);
                if (inputAction != null) { secondaryFireInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(dockingInputActionId);
                if (inputAction != null) { dockingInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                int numCPI = customPlayerInputList == null ? 0 : customPlayerInputList.Count;

                for (int cpiIdx = 0; cpiIdx < numCPI; cpiIdx++)
                {
                    CustomPlayerInput customPlayerInput = customPlayerInputList[cpiIdx];
                    if (customPlayerInput != null)
                    {
                        inputAction = Rewired.ReInput.mapping.GetAction(customPlayerInput.rwdPositiveInputActionId);
                        if (inputAction != null) { customPlayerInput.rwdPositiveInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }
                        else { customPlayerInput.rwdPositiveInputActionType = 0; }

                        customPlayerInput.lastInputValueX = 0f;
                        customPlayerInput.lastInputValueY = 0f;
                        customPlayerInput.rwLastIsButtonPressed = false;

                        inputAction = Rewired.ReInput.mapping.GetAction(customPlayerInput.rwdNegativeInputActionId);
                        if (inputAction != null) { customPlayerInput.rwdNegativeInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }
                        else { customPlayerInput.rwdNegativeInputActionType = 0; }
                    }
                }
            }
            #endif
        }

        #endregion

        #region Public API Vive Methods
        /// <summary>
        /// Vive Input configuration is stored in SSC in a generic form. To avoid converting it with the Update event loops,
        /// it is converted here once. If the Vive Input is changed via the PlayerInputModule at runtime, call this method.
        /// If this is not called, in the Editor you might receive 'PlayerInputModule VIVE Argument cannot be null.'
        /// </summary>
        /// <param name="showErrors"></param>
        public void UpdateViveInput(bool showErrors)
        {
            // We could get the type from the assembly OR we could just check for the current known types...
            #if VIU_PLUGIN
            viveHorizontalRoleSystemType = viveHorizontalRoleType == ViveRoleType.BodyRole ? typeof(BodyRole) : (viveHorizontalRoleType == ViveRoleType.DeviceRole ? typeof(DeviceRole) : (viveHorizontalRoleType == ViveRoleType.TrackerRole ? typeof(TrackerRole) : typeof(HandRole)));
            viveVerticalRoleSystemType = viveVerticalRoleType == ViveRoleType.BodyRole ? typeof(BodyRole) : (viveVerticalRoleType == ViveRoleType.DeviceRole ? typeof(DeviceRole) : (viveVerticalRoleType == ViveRoleType.TrackerRole ? typeof(TrackerRole) : typeof(HandRole)));
            viveLongitudinalRoleSystemType = viveLongitudinalRoleType == ViveRoleType.BodyRole ? typeof(BodyRole) : (viveLongitudinalRoleType == ViveRoleType.DeviceRole ? typeof(DeviceRole) : (viveLongitudinalRoleType == ViveRoleType.TrackerRole ? typeof(TrackerRole) : typeof(HandRole)));
            vivePitchRoleSystemType = vivePitchRoleType == ViveRoleType.BodyRole ? typeof(BodyRole) : (vivePitchRoleType == ViveRoleType.DeviceRole ? typeof(DeviceRole) : (vivePitchRoleType == ViveRoleType.TrackerRole ? typeof(TrackerRole) : typeof(HandRole)));
            viveYawRoleSystemType = viveYawRoleType == ViveRoleType.BodyRole ? typeof(BodyRole) : (viveYawRoleType == ViveRoleType.DeviceRole ? typeof(DeviceRole) : (viveYawRoleType == ViveRoleType.TrackerRole ? typeof(TrackerRole) : typeof(HandRole)));
            viveRollRoleSystemType = viveRollRoleType == ViveRoleType.BodyRole ? typeof(BodyRole) : (viveRollRoleType == ViveRoleType.DeviceRole ? typeof(DeviceRole) : (viveRollRoleType == ViveRoleType.TrackerRole ? typeof(TrackerRole) : typeof(HandRole)));
            vivePrimaryFireRoleSystemType = vivePrimaryFireRoleType == ViveRoleType.BodyRole ? typeof(BodyRole) : (vivePrimaryFireRoleType == ViveRoleType.DeviceRole ? typeof(DeviceRole) : (vivePrimaryFireRoleType == ViveRoleType.TrackerRole ? typeof(TrackerRole) : typeof(HandRole)));
            viveSecondaryFireRoleSystemType = viveSecondaryFireRoleType == ViveRoleType.BodyRole ? typeof(BodyRole) : (viveSecondaryFireRoleType == ViveRoleType.DeviceRole ? typeof(DeviceRole) : (viveSecondaryFireRoleType == ViveRoleType.TrackerRole ? typeof(TrackerRole) : typeof(HandRole)));
            viveDockingRoleSystemType = viveDockingRoleType == ViveRoleType.BodyRole ? typeof(BodyRole) : (viveDockingRoleType == ViveRoleType.DeviceRole ? typeof(DeviceRole) : (viveDockingRoleType == ViveRoleType.TrackerRole ? typeof(TrackerRole) : typeof(HandRole)));

            // Avoid Enum conversion in Update event
            // Assume +ve and -ve must always be the same type (Axis or Button)

            if (viveHorizontalInputType == ViveInputType.Axis)
            {
                positiveHorizontalInputCtrlAxisVive = (ControllerAxis)positiveHorizontalInputCtrlVive;
                negativeHorizontalInputCtrlAxisVive = (ControllerAxis)negativeHorizontalInputCtrlVive;
                positiveHorizontalInputCtrlBtnVive = ControllerButton.None;
                negativeHorizontalInputCtrlBtnVive = ControllerButton.None;
            }
            else if (viveHorizontalInputType == ViveInputType.Button)
            {
                positiveHorizontalInputCtrlBtnVive = (ControllerButton)positiveHorizontalInputCtrlVive;
                negativeHorizontalInputCtrlBtnVive = (ControllerButton)negativeHorizontalInputCtrlVive;
                positiveHorizontalInputCtrlAxisVive = ControllerAxis.None;
                negativeHorizontalInputCtrlAxisVive = ControllerAxis.None;
            }
            else
            {
                positiveHorizontalInputCtrlAxisVive = ControllerAxis.None;
                negativeHorizontalInputCtrlAxisVive = ControllerAxis.None;
                positiveHorizontalInputCtrlBtnVive = ControllerButton.None;
                negativeHorizontalInputCtrlBtnVive = ControllerButton.None;
            }

            if (viveVerticalInputType == ViveInputType.Axis)
            {
                positiveVerticalInputCtrlAxisVive = (ControllerAxis)positiveVerticalInputCtrlVive;
                negativeVerticalInputCtrlAxisVive = (ControllerAxis)negativeVerticalInputCtrlVive;
                positiveVerticalInputCtrlBtnVive = ControllerButton.None;
                negativeVerticalInputCtrlBtnVive = ControllerButton.None;
            }
            else if (viveVerticalInputType == ViveInputType.Button)
            {
                positiveVerticalInputCtrlBtnVive = (ControllerButton)positiveVerticalInputCtrlVive;
                negativeVerticalInputCtrlBtnVive = (ControllerButton)negativeVerticalInputCtrlVive;
                positiveVerticalInputCtrlAxisVive = ControllerAxis.None;
                negativeVerticalInputCtrlAxisVive = ControllerAxis.None;
            }
            else
            {
                positiveVerticalInputCtrlAxisVive = ControllerAxis.None;
                negativeVerticalInputCtrlAxisVive = ControllerAxis.None;
                positiveVerticalInputCtrlBtnVive = ControllerButton.None;
                negativeVerticalInputCtrlBtnVive = ControllerButton.None;
            }

            if (viveLongitudinalInputType == ViveInputType.Axis)
            {
                positiveLongitudinalInputCtrlAxisVive = (ControllerAxis)positiveLongitudinalInputCtrlVive;
                negativeLongitudinalInputCtrlAxisVive = (ControllerAxis)negativeLongitudinalInputCtrlVive;
                positiveLongitudinalInputCtrlBtnVive = ControllerButton.None;
                negativeLongitudinalInputCtrlBtnVive = ControllerButton.None;
            }
            else if (viveLongitudinalInputType == ViveInputType.Button)
            {
                positiveLongitudinalInputCtrlBtnVive = (ControllerButton)positiveLongitudinalInputCtrlVive;
                negativeLongitudinalInputCtrlBtnVive = (ControllerButton)negativeLongitudinalInputCtrlVive;
                positiveLongitudinalInputCtrlAxisVive = ControllerAxis.None;
                negativeLongitudinalInputCtrlAxisVive = ControllerAxis.None;
            }
            else
            {
                positiveLongitudinalInputCtrlAxisVive = ControllerAxis.None;
                negativeLongitudinalInputCtrlAxisVive = ControllerAxis.None;
                positiveLongitudinalInputCtrlBtnVive = ControllerButton.None;
                negativeLongitudinalInputCtrlBtnVive = ControllerButton.None;
            }

            if (vivePitchInputType == ViveInputType.Axis)
            {
                positivePitchInputCtrlAxisVive = (ControllerAxis)positivePitchInputCtrlVive;
                negativePitchInputCtrlAxisVive = (ControllerAxis)negativePitchInputCtrlVive;
                positivePitchInputCtrlBtnVive = ControllerButton.None;
                negativePitchInputCtrlBtnVive = ControllerButton.None;
            }
            else if (vivePitchInputType == ViveInputType.Button)
            {
                positivePitchInputCtrlBtnVive = (ControllerButton)positivePitchInputCtrlVive;
                negativePitchInputCtrlBtnVive = (ControllerButton)negativePitchInputCtrlVive;
                positivePitchInputCtrlAxisVive = ControllerAxis.None;
                negativePitchInputCtrlAxisVive = ControllerAxis.None;
            }
            else
            {
                positivePitchInputCtrlAxisVive = ControllerAxis.None;
                negativePitchInputCtrlAxisVive = ControllerAxis.None;
                positivePitchInputCtrlBtnVive = ControllerButton.None;
                negativePitchInputCtrlBtnVive = ControllerButton.None;
            }

            if (viveYawInputType == ViveInputType.Axis)
            {
                positiveYawInputCtrlAxisVive = (ControllerAxis)positiveYawInputCtrlVive;
                negativeYawInputCtrlAxisVive = (ControllerAxis)negativeYawInputCtrlVive;
                positiveYawInputCtrlBtnVive = ControllerButton.None;
                negativeYawInputCtrlBtnVive = ControllerButton.None;
            }
            else if (viveYawInputType == ViveInputType.Button)
            {
                positiveYawInputCtrlBtnVive = (ControllerButton)positiveYawInputCtrlVive;
                negativeYawInputCtrlBtnVive = (ControllerButton)negativeYawInputCtrlVive;
                positiveYawInputCtrlAxisVive = ControllerAxis.None;
                negativeYawInputCtrlAxisVive = ControllerAxis.None;
            }
            else
            {
                positiveYawInputCtrlAxisVive = ControllerAxis.None;
                negativeYawInputCtrlAxisVive = ControllerAxis.None;
                positiveYawInputCtrlBtnVive = ControllerButton.None;
                negativeYawInputCtrlBtnVive = ControllerButton.None;
            }

            if (viveRollInputType == ViveInputType.Axis)
            {
                positiveRollInputCtrlAxisVive = (ControllerAxis)positiveRollInputCtrlVive;
                negativeRollInputCtrlAxisVive = (ControllerAxis)negativeRollInputCtrlVive;
                positiveRollInputCtrlBtnVive = ControllerButton.None;
                negativeRollInputCtrlBtnVive = ControllerButton.None;
            }
            else if (viveRollInputType == ViveInputType.Button)
            {
                positiveRollInputCtrlBtnVive = (ControllerButton)positiveRollInputCtrlVive;
                negativeRollInputCtrlBtnVive = (ControllerButton)negativeRollInputCtrlVive;
                positiveRollInputCtrlAxisVive = ControllerAxis.None;
                negativeRollInputCtrlAxisVive = ControllerAxis.None;
            }
            else
            {
                positiveRollInputCtrlAxisVive = ControllerAxis.None;
                negativeRollInputCtrlAxisVive = ControllerAxis.None;
                positiveRollInputCtrlBtnVive = ControllerButton.None;
                negativeRollInputCtrlBtnVive = ControllerButton.None;
            }

            // Primary and Secondary Fire and Docking will always be buttons
            primaryFireInputCtrlBtnVive = (ControllerButton)primaryFireInputCtrlVive;
            secondaryFireInputCtrlBtnVive = (ControllerButton)secondaryFireInputCtrlVive;
            dockingInputCtrlBtnVive = (ControllerButton)dockingInputCtrlVive;

            int numCPI = customPlayerInputList == null ? 0 : customPlayerInputList.Count;

            for (int cpiIdx = 0; cpiIdx < numCPI; cpiIdx++)
            {
                CustomPlayerInput customPlayerInput = customPlayerInputList[cpiIdx];
                if (customPlayerInput != null)
                {
                    // Check for the known types
                    customPlayerInput.viveRoleSystemType = customPlayerInput.viveRoleType == ViveRoleType.BodyRole ? typeof(BodyRole) : (customPlayerInput.viveRoleType == ViveRoleType.DeviceRole ? typeof(DeviceRole) : (customPlayerInput.viveRoleType == ViveRoleType.TrackerRole ? typeof(TrackerRole) : typeof(HandRole)));

                    // Avoid Enum conversion in Update event
                    // Assume +ve and -ve must always be the same type (Axis or Button)
                    if (customPlayerInput.viveInputType == ViveInputType.Axis)
                    {
                        customPlayerInput.vivePositiveInputCtrlAxis = (ControllerAxis)positiveYawInputCtrlVive;
                        customPlayerInput.viveNegativeInputCtrlAxis = (ControllerAxis)negativeYawInputCtrlVive;
                        customPlayerInput.vivePositiveInputCtrlBtn = ControllerButton.None;
                        customPlayerInput.viveNegativeInputCtrlBtn = ControllerButton.None;
                    }
                    else if (customPlayerInput.viveInputType == ViveInputType.Button)
                    {
                        customPlayerInput.vivePositiveInputCtrlBtn = (ControllerButton)positiveYawInputCtrlVive;
                        customPlayerInput.viveNegativeInputCtrlBtn = (ControllerButton)negativeYawInputCtrlVive;
                        customPlayerInput.vivePositiveInputCtrlAxis = ControllerAxis.None;
                        customPlayerInput.viveNegativeInputCtrlAxis = ControllerAxis.None;
                    }
                    else
                    {
                        customPlayerInput.vivePositiveInputCtrlAxis = ControllerAxis.None;
                        customPlayerInput.viveNegativeInputCtrlAxis = ControllerAxis.None;
                        customPlayerInput.vivePositiveInputCtrlBtn = ControllerButton.None;
                        customPlayerInput.viveNegativeInputCtrlBtn = ControllerButton.None;
                    }

                    customPlayerInput.lastInputValueX = 0f;
                    customPlayerInput.lastInputValueY = 0f;
                }
            }

            #endif
        }

        #endregion

        #region Public API Rumble Methods

        /// <summary>
        /// Set the rumble / vibration / force feedback amount on the current
        /// device. Currently only applies to Unity Input System and Rewired.
        /// </summary>
        /// <param name="rumbleAmount"></param>
        public void SetRumble(float rumbleAmount)
        {
            // Clamp 0.0 - 1.0
            float actualRumbleAmount = rumbleAmount < 0f ? 0f : rumbleAmount > 1f ? 1f : rumbleAmount;

            try
            {
                if (inputMode == InputMode.UnityInputSystem)
                {
                    #if SSC_UIS
                    /// TODO - check for device assigned to this player ship
                    if (UnityEngine.InputSystem.Gamepad.current != null)
                    {
                        UnityEngine.InputSystem.Gamepad.current.SetMotorSpeeds(actualRumbleAmount * maxRumble1, actualRumbleAmount * maxRumble2);
                    }
                    #endif
                }
                else if (inputMode == InputMode.Rewired)
                {
                    #if SSC_REWIRED
                    if (rewiredPlayer != null && rewiredPlayer.controllers.joystickCount > 0)
                    {
                        float rumble1Amount = actualRumbleAmount * maxRumble1;
                        float rumble2Amount = actualRumbleAmount * maxRumble2;

                        if (rumble1Amount > 0.01f || rumble2Amount > 0.01f)
                        {
                            // Set the motor values to be proportional to the maximum specified
                            rewiredPlayer.controllers.Joysticks[0].SetVibration(0, rumble1Amount);
                            rewiredPlayer.controllers.Joysticks[0].SetVibration(1, rumble2Amount);
                        }
                        else { rewiredPlayer.controllers.Joysticks[0].StopVibration(); }
                    }
                    #endif
                }
                else
                {
                    // keep compiler happy but do nothing
                    if (actualRumbleAmount > 0f) { }
                }
            }
            catch (System.Exception ex)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SetRumble - something went wrong. " + ex.Message);
                #else
                if (ex != null) { }
                #endif
            }
        }

        /// <summary>
        /// This should be called if you change maxRumble1 or maxRumble2 at runtime.
        /// </summary>
        public void ReinitialiseRumble()
        {
            if (shipControlModule != null)
            {
                // Is rumble enabled and within acceptable values?
                if ((maxRumble1 > 0f || maxRumble2 > 0f) && maxRumble1 <= 1f && maxRumble2 <= 1f)
                {
                    shipControlModule.callbackOnRumble = SetRumble;
                }
                else { shipControlModule.callbackOnRumble = null; }
            }
        }

        #endregion

        #region Public API XR Methods

        /// <summary>
        /// Attempt to disable the XR Camera. Has no effect if UnityXR is not configured.
        /// </summary>
        public void DisableXRCamera()
        {
            #if SCSM_XR && SSC_UIS
            EnableOrDisableXRCamera(false);
            #endif
        }

        /// <summary>
        /// Attempt to disable the XR Hands. Has no effect if UnityXR is not configured.
        /// </summary>
        public void DisableXRHands()
        {
            #if SCSM_XR && SSC_UIS
            EnableOrDisableXRHands(false);
            #endif
        }

        /// <summary>
        /// Attempt to enable the XR Camera. Has no effect if UnityXR is not configured.
        /// </summary>
        public void EnableXRCamera()
        {
            #if SCSM_XR && SSC_UIS
            EnableOrDisableXRCamera(true);
            #endif
        }

        /// <summary>
        /// Attempt to enable the XR Hands. Has no effect if UnityXR is not configured.
        /// </summary>
        public void EnableXRHands()
        {
            #if SCSM_XR && SSC_UIS
            EnableOrDisableXRHands(true);
            #endif
        }

        #if SCSM_XR && SSC_UIS

        /// <summary>
        /// Get the Unity Input System InputActionAsset scriptableobject for XR
        /// </summary>
        /// <returns></returns>
        public UnityEngine.InputSystem.InputActionAsset GetXRInputActionAsset()
        {
            return inputActionAssetXR;
        }

        /// <summary>
        /// Set the XR camera which will be rotated by the Tracked Pose Driver.
        /// </summary>
        /// <param name="newCamera"></param>
        /// <param name="cameraTrfm"></param>
        /// <param name="isAutoEnable"></param>
        /// <returns></returns>
        public bool SetXRFirstPersonCamera1 (Camera newCamera, Transform cameraTrfm, bool isAutoEnable)
        {
            bool isSuccessful = false;

            if (newCamera == null || cameraTrfm == null)
            {
                firstPersonCamera1XR = null;
                firstPersonTransform1XR = null;
            }
            // Ensure camera is a child of the PlayerInputModule (and ShipControlModule) and the camera is on the transform being passed in.
            else if (cameraTrfm.IsChildOf(transform) && newCamera.transform.GetInstanceID() == cameraTrfm.GetInstanceID())
            {
                firstPersonCamera1XR = newCamera;
                firstPersonTransform1XR = cameraTrfm;

                firstPersonCamera1XR.enabled = isAutoEnable;
            }

            return isSuccessful;
        }

        /// <summary>
        /// Used at runtime to convert string unique identifiers for actions (GUIDs) into
        /// Unity Input System ActionInputs to avoid looking them up and incurring GC overhead each Update.
        /// If actions are modified at runtime, call this method.
        /// Has no effect if Unity Input System package is not installed.
        /// </summary>
        /// <param name="showErrors"></param>
        public void UpdateUnityXRActions (bool showErrors = false)
        {
            #if UNITY_EDITOR
            string methodName = "PlayerInputModule.UpdateUnityXRActions";
            #endif

            // Get the hashcodes for the Control Types that we support
            controlTypeButtonHashUIS = SSCMath.GetHashCode("Button");
            controlTypeAxisHashUIS = SSCMath.GetHashCode("Axis");
            // Dpad and Vector2 are equivalent
            controlTypeDpadHashUIS = SSCMath.GetHashCode("Dpad");
            controlTypeVector2HashUIS = SSCMath.GetHashCode("Vector2");
            controlTypeVector3HashUIS = SSCMath.GetHashCode("Vector3");

            if (inputActionAssetXR != null)
            {
                int numCPI = customPlayerInputList == null ? 0 : customPlayerInputList.Count;

                //UnityEngine.InputSystem.InputAction inputAction = null;

                positiveHorizontalInputActionUIS = UISGetAction(inputActionAssetXR, positiveHorizontalInputActionMapIdXR, positiveHorizontalInputActionIdUIS);
                positiveVerticalInputActionUIS = UISGetAction(inputActionAssetXR, positiveVerticalInputActionMapIdXR, positiveVerticalInputActionIdUIS);
                positiveLongitudinalInputActionUIS = UISGetAction(inputActionAssetXR, positiveLongitudinalInputActionMapIdXR, positiveLongitudinalInputActionIdUIS);
                positivePitchInputActionUIS = UISGetAction(inputActionAssetXR, positivePitchInputActionMapIdXR, positivePitchInputActionIdUIS);
                positiveYawInputActionUIS = UISGetAction(inputActionAssetXR, positiveYawInputActionMapIdXR, positiveYawInputActionIdUIS);
                positiveRollInputActionUIS = UISGetAction(inputActionAssetXR, positiveRollInputActionMapIdXR, positiveRollInputActionIdUIS);
                primaryFireInputActionUIS = UISGetAction(inputActionAssetXR, primaryFireInputActionMapIdXR, primaryFireInputActionIdUIS);
                secondaryFireInputActionUIS = UISGetAction(inputActionAssetXR, secondaryFireInputActionMapIdXR, secondaryFireInputActionIdUIS);
                dockingInputActionUIS = UISGetAction(inputActionAssetXR, dockingInputActionMapIdXR, dockingInputActionIdUIS);

                positiveHorizontalCtrlTypeHashUIS = positiveHorizontalInputActionUIS == null ? 0 : SSCMath.GetHashCode(positiveHorizontalInputActionUIS.expectedControlType);
                positiveVerticalCtrlTypeHashUIS = positiveVerticalInputActionUIS == null ? 0 : SSCMath.GetHashCode(positiveVerticalInputActionUIS.expectedControlType);
                positiveLongitudinalCtrlTypeHashUIS = positiveLongitudinalInputActionUIS == null ? 0 : SSCMath.GetHashCode(positiveLongitudinalInputActionUIS.expectedControlType);
                positivePitchCtrlTypeHashUIS = positivePitchInputActionUIS == null ? 0 : SSCMath.GetHashCode(positivePitchInputActionUIS.expectedControlType);
                positiveYawCtrlTypeHashUIS = positiveYawInputActionUIS == null ? 0 : SSCMath.GetHashCode(positiveYawInputActionUIS.expectedControlType);
                positiveRollCtrlTypeHashUIS = positiveRollInputActionUIS == null ? 0 : SSCMath.GetHashCode(positiveRollInputActionUIS.expectedControlType);
                primaryFireCtrlTypeHashUIS = primaryFireInputActionUIS == null ? 0 : SSCMath.GetHashCode(primaryFireInputActionUIS.expectedControlType);
                secondaryFireCtrlTypeHashUIS = secondaryFireInputActionUIS == null ? 0 : SSCMath.GetHashCode(secondaryFireInputActionUIS.expectedControlType);
                dockingCtrlTypeHashUIS = dockingInputActionUIS == null ? 0 : SSCMath.GetHashCode(dockingInputActionUIS.expectedControlType);

                // Update all the Custom Player Inputs
                for (int cpiIdx = 0; cpiIdx < numCPI; cpiIdx++)
                {
                    CustomPlayerInput customPlayerInput = customPlayerInputList[cpiIdx];
                    if (customPlayerInput != null)
                    {
                        customPlayerInput.uisPositiveInputAction = UISGetAction(inputActionAssetXR, customPlayerInput.xrPositiveInputActionMapId, customPlayerInput.uisPositiveInputActionId);
                        customPlayerInput.uisPositiveCtrlTypeHash = customPlayerInput.uisPositiveInputAction == null ? 0 : SSCMath.GetHashCode(customPlayerInput.uisPositiveInputAction.expectedControlType);
                        customPlayerInput.lastInputValueX = 0f;
                        customPlayerInput.lastInputValueY = 0f;
                    }
                }
            }
            #if UNITY_EDITOR
            else { if (showErrors) { Debug.LogWarning(methodName + " couldn't find inputActionAssetXR for " + this.name); } }
            #endif
        }

        #endif
        #endregion

        #region Public API - AI Assist Methods

        /// <summary>
        /// Attempt to enter Ship AI-assisted mode, then attempt docking.
        /// </summary>
        public void EnableAIDocking()
        {
            bool originalAIMode = isShipAIModeEnabled;

            if (!isShipAIModeEnabled) { EnableOrDisableAIMode(true); }

            // Is AI mode now enabled?
            if (isShipAIModeEnabled)
            {
                ShipDocking shipDocking = shipControlModule.GetShipDocking(false);

                if (shipDocking != null)
                {
                    int dockingStateInt = shipDocking.GetStateInt();

                    //Debug.Log("[DEBUG] EnableAIDocking T:" + Time.time);

                    // v1.2.3 add ability to dock when in undocking state
                    if (dockingStateInt == ShipDocking.notDockedInt || dockingStateInt == ShipDocking.undockingInt)
                    {
                        shipDocking.SetState(ShipDocking.DockingState.Docking);
                    }
                    else
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("ERROR: PlayerInputModule.EnableAIDocking - cannot enable AI Docking as " + gameObject.name + " is " + shipDocking.GetState().ToString() + ". It must be in the NotDocked or UnDocking state.");
                        #endif

                        // Attempt to restore original AI Mode
                        if (!originalAIMode) { EnableOrDisableAIMode(false); }
                    }
                }
                else
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: PlayerInputModule.EnableAIDocking - cannot enable AI Docking as Ship Docking component may not be attached to " + gameObject.name);
                    #endif

                    // Attempt to restore original AI Mode
                    if (!originalAIMode) { EnableOrDisableAIMode(false); }
                }
            }
        }

        /// <summary>
        /// Attempt to enter Ship AI-assisted mode, then attempt undocking.
        /// </summary>
        public void EnableAIUndocking()
        {
            bool originalAIMode = isShipAIModeEnabled;

            if (!isShipAIModeEnabled) { EnableOrDisableAIMode(true); }

            // Is AI mode now enabled?
            if (isShipAIModeEnabled)
            {
                ShipDocking shipDocking = shipControlModule.GetShipDocking(false);

                if (shipDocking != null)
                {
                    int dockingStateInt = shipDocking.GetStateInt();

                    //Debug.Log("[DEBUG] EnableAIUndocking T:" + Time.time);

                    // v1.2.3 add ability to undock when in docking state
                    if (dockingStateInt == ShipDocking.dockedInt || dockingStateInt == ShipDocking.dockingInt)
                    {
                        shipDocking.SetState(ShipDocking.DockingState.Undocking);
                    }
                    else
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("ERROR: PlayerInputModule.EnableAIUnDocking - cannot enable AI Undocking as " + gameObject.name + " is " + shipDocking.GetState().ToString() + ". It must be in the Docked or Docking state.");
                        #endif

                        // Attempt to restore original AI Mode
                        if (!originalAIMode) { EnableOrDisableAIMode(false); }
                    }
                }
                else
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: PlayerInputModule.EnableAIDocking - cannot enable AI Docking as Ship Docking component may not be attached to " + gameObject.name);
                    #endif

                    // Attempt to restore original AI Mode
                    if (!originalAIMode) { EnableOrDisableAIMode(false); }
                }
            }
        }

        /// <summary>
        /// Attempt to disable AI control mode. This requires a Ship AI Input Module to also be attached
        /// to the player ship. After calling this method, check if it was successful with the
        /// IsShipAIModeEnabled property.
        /// </summary>
        public void DisableAIMode()
        {
            EnableOrDisableAIMode(false);
        }

        /// <summary>
        /// Attempt to enable AI control mode. This requires a Ship AI Input Module to also be attached
        /// to the player ship. After calling this method, check if it was successful with the
        /// IsShipAIModeEnabled property.
        /// </summary>
        public void EnableAIMode()
        {
            EnableOrDisableAIMode(true);
        }

        /// <summary>
        /// Attempt to switch between Player and AI control or vis versa.
        /// If the mode is changed during this method, we can assume that the Ship AI Input Module
        /// component is present and initialised.
        /// </summary>
        public void ToggleAIMode()
        {
            EnableOrDisableAIMode(!isShipAIModeEnabled);
        }

        /// <summary>
        /// Currently in Technical Preview
        /// Attempt to enter Ship AI-assisted mode, then attempt docking or undocking depending upon the current DockignState.
        /// </summary>
        public void ToggleAIDocking()
        {
            bool originalAIMode = isShipAIModeEnabled;

            if (!isShipAIModeEnabled) { EnableOrDisableAIMode(true); }

            // Is AI mode now enabled?
            if (isShipAIModeEnabled)
            {
                ShipDocking shipDocking = shipControlModule.GetShipDocking(false);

                if (shipDocking != null)
                {
                    int currentDockingState = shipDocking.GetStateInt();
                    
                    if (currentDockingState == ShipDocking.dockedInt)
                    {
                        shipDocking.SetState(ShipDocking.DockingState.Undocking);
                    }
                    else if (currentDockingState == ShipDocking.notDockedInt)
                    {
                        shipDocking.SetState(ShipDocking.DockingState.Docking);
                    }
                    else
                    {
                        // Attempt to restore original AI Mode
                        if (!originalAIMode) { EnableOrDisableAIMode(false); }
                    }
                }
                else
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: PlayerInputModule.ToggleAIDocking - cannot toggle AI Docking as Ship Docking component may not be attached to " + gameObject.name);
                    #endif

                    // Attempt to restore original AI Mode
                    if (!originalAIMode) { EnableOrDisableAIMode(false); }
                }
            }
        }

        #endregion

        #endregion
    }
}

