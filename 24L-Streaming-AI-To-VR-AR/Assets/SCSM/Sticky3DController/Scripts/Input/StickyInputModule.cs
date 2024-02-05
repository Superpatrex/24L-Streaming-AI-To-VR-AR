using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if SCSM_XR && SSC_UIS
using UnityEngine.XR;
#endif

// Sticky3D Control Module Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [RequireComponent(typeof(StickyControlModule))]
    [AddComponentMenu("Sticky3D Controller/Character/Sticky Input Module")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyInputModule : MonoBehaviour
    {

        #region Enumerations

        public enum InputMode
        {
            DirectKeyboard = 0,
            LegacyUnity = 10,
            UnityInputSystem = 20,
            //OculusAPI = 23,
            Rewired = 30,
            //Vive = 80,
            UnityXR = 85
        }

        public enum InputAxisMode
        {
            NoInput = 0,
            SingleAxis = 10,
            CombinedAxis = 20
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Is the Sticky Input Module initialised and ready for use?
        /// </summary>
        public bool IsInitialised { get; private set; }

        /// <summary>
        /// Is the StickyInputModule currently enabled to send input to the Sticky Control Module?
        /// See EnableInput() and DisableInput(..)
        /// </summary>
        public bool IsInputEnabled { get { return isInputEnabled; } }

        /// <summary>
        /// Is all input except CustomerInputs ignored?
        /// See EnableInput() and DisableInput(..)
        /// </summary>
        public bool IsCustomInputOnlyEnabled { get { return isCustomInputsOnlyEnabled; } }

        /// <summary>
        /// Gets a reference to the input being sent from the StickyInputMode to the StickyControlModule
        /// </summary>
        public CharacterInput GetCharacterInput { get { return characterInput; } }

        /// <summary>
        /// Gets a reference to the XR input being sent from the StickyInputMode to the StickyControlModule
        /// </summary>
        public CharacterInputXR GetCharacterInputXR { get { return characterInputXR; } }       

        #if SCSM_XR && SSC_UIS

        /// <summary>
        /// Is there a valid Head-Mounted XR device (HMD)?
        /// </summary>
        public bool IsHMDInputDeviceXRValid { get { return hmdInputDeviceXR.isValid; } }

        /// <summary>
        /// Is there a valid Left Hand XR device?
        /// </summary>
        public bool IsLeftHandInputDeviceXRValid { get { return leftHandInputDeviceXR.isValid; } }

        /// <summary>
        /// Is there a valid Right Hand XR device?
        /// </summary>
        public bool IsRightHandInputDeviceXRValid { get { return rightHandInputDeviceXR.isValid; } }

        /// <summary>
        /// Has the XR system been initialised?
        /// </summary>
        public bool IsInitialisedXR { get { return isInitialisedXR; } }

        /// <summary>
        /// Is the XR system currently being initialised?
        /// </summary>
        public bool IsInitialisingXR { get { return isInitialisingXR; } }

        /// <summary>
        /// How many attempts where made to initialise the XR subsystems?
        /// </summary>
        public int InitialisedXRCount { get { return initialiseXRCount; } }

        #endif

        #endregion

        #region Public Variables

        public InputMode inputMode = InputMode.DirectKeyboard;

        /// <summary>
        /// If enabled, Initialise() will be called as soon as Awake() runs. This should be disabled if you want to control
        /// when the Sticky Input Module is initialised through code.
        /// </summary>
        public bool initialiseOnAwake = true;

        /// <summary>
        /// Is input enabled when the module is first initialised?  See also EnableInput() and DisableInput().
        /// </summary>
        public bool isEnabledOnInitialise = true;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool allowRepaint = false;

        #region Common input variables
        public InputAxisMode hztlMoveInputAxisMode = InputAxisMode.SingleAxis;
        public InputAxisMode vertMoveInputAxisMode = InputAxisMode.SingleAxis;
        public InputAxisMode hztlLookInputAxisMode = InputAxisMode.SingleAxis;
        public InputAxisMode vertLookInputAxisMode = InputAxisMode.SingleAxis;
        public InputAxisMode zoomLookInputAxisMode = InputAxisMode.SingleAxis;
        public InputAxisMode orbitLookInputAxisMode = InputAxisMode.SingleAxis;
        public bool jumpCanBeHeld = true;
        public bool crouchCanBeHeld = true;
        public bool crouchIsToggled = false;
        public bool crouchIsJetPackOnly = false;
        public bool leftFire1CanBeHeld = false;
        public bool leftFire2CanBeHeld = false;
        public bool rightFire1CanBeHeld = false;
        public bool rightFire2CanBeHeld = false;

        // Used with Rewired, Unity Input System
        public bool jumpButtonEnabled = false;
        public bool sprintButtonEnabled = false;
        public bool crouchButtonEnabled = false;
        public bool jetpackButtonEnabled = false;
        public bool switchLookButtonEnabled = false;
        public bool leftFire1ButtonEnabled = false;
        public bool leftFire2ButtonEnabled = false;
        public bool rightFire1ButtonEnabled = false;
        public bool rightFire2ButtonEnabled = false;
        #endregion

        #region Editor variables
        // Whether the input sections are shown as expanded in the inspector window of the editor.
        public bool hztlMoveShowInEditor;
        public bool vertMoveShowInEditor;
        public bool jumpShowInEditor;
        public bool sprintShowInEditor;
        public bool crouchShowInEditor;
        public bool jetpackShowInEditor;
        public bool switchLookShowInEditor;
        public bool hztlLookShowInEditor;
        public bool vertLookShowInEditor;
        public bool zoomLookShowInEditor;
        public bool orbitLookShowInEditor;
        public bool leftFire1ShowInEditor;
        public bool leftFire2ShowInEditor;
        public bool rightFire1ShowInEditor;
        public bool rightFire2ShowInEditor;
        public bool customInputsShowInEditor;
        #endregion

        #region Direct Keyboard and Mouse
        public KeyCode posHztrlMoveInputKeycode = KeyCode.D;
        public KeyCode negHztrlMoveInputKeycode = KeyCode.A;
        public KeyCode posVertMoveInputKeycode = KeyCode.W;
        public KeyCode negVertMoveInputKeycode = KeyCode.S;
        public KeyCode jumpInputKeycode = KeyCode.Space;
        public KeyCode sprintInputKeycode = KeyCode.LeftShift;
        public KeyCode crouchInputKeycode = KeyCode.C;
        public KeyCode jetpackInputKeycode = KeyCode.J;
        public KeyCode switchLookInputKeycode = KeyCode.V;
        public KeyCode posZoomInputKeycode = KeyCode.RightBracket;
        public KeyCode negZoomInputKeycode = KeyCode.LeftBracket;
        public KeyCode posOrbitInputKeycode = KeyCode.Alpha0;
        public KeyCode negOrbitInputKeycode = KeyCode.Alpha9;
        public KeyCode leftFire1InputKeycode = KeyCode.None;
        public KeyCode leftFire2InputKeycode = KeyCode.None;
        public KeyCode rightFire1InputKeycode = KeyCode.None;
        public KeyCode rightFire2InputKeycode = KeyCode.None;
        public bool isMouseScrollForZoomEnabledDKI = true;
        public bool isMouseCentreHztlLookDKI = false;
        public bool isMouseCentreVertLookDKI = false;
        public string mouseHztlLookInputAxisNameDKI = "Mouse X";
        public string mouseVertLookInputAxisNameDKI = "Mouse Y";
        public bool isMouseHztlLookInputAxisValidDKI = true;
        public bool isMouseVertLookInputAxisValidDKI = true;
        [Range(0.1f, 1.0f)] public float hztlLookSensitivityDKI = 0.5f;
        [Range(0.1f, 1.0f)] public float vertLookSensitivityDKI = 0.25f;
        [Range(0f, 0.25f)] public float hztlLookDeadzoneDKI = 0.01f;
        [Range(0f, 0.25f)] public float vertLookDeadzoneDKI = 0.0125f;
        
        #endregion

        #region Legacy Unity Input System
        public string posHztlMoveInputAxisName = "Horizontal";
        public string negHztlMoveInputAxisName = "Horizontal";
        public string posVertMoveInputAxisName = "Vertical";
        public string negVertMoveInputAxisName = "Vertical";
        public string posHztlLookInputAxisName = "Mouse X";
        public string negHztlLookInputAxisName = "Mouse X";
        public string posVertLookInputAxisName = "Mouse Y";
        public string negVertLookInputAxisName = "Mouse Y";
        public string posZoomLookInputAxisName = "Mouse ScrollWheel";
        public string negZoomLookInputAxisName = "";
        public string posOrbitLookInputAxisName = "";
        public string negOrbitLookInputAxisName = "";
        public string jumpInputAxisName = "Jump";
        // By default Unity does not have a input axes for Run, Crouch, orbig, jetpack or switch look
        public string sprintInputAxisName = "";
        public string crouchInputAxisName = "";
        public string jetpackInputAxisName = "";
        public string switchLookInputAxisName = "";
        public string leftFire1InputAxisName = "Fire 2";
        public string leftFire2InputAxisName = "";
        public string rightFire1InputAxisName = "Fire 1";
        public string rightFire2InputAxisName = "";
        public bool invertHztlMoveInputAxis = false;
        public bool invertVertMoveInputAxis = false;
        public bool invertHztlLookInputAxis = false;
        public bool invertVertLookInputAxis = false;
        public bool invertZoomLookInputAxis = false;
        public bool invertOrbitLookInputAxis = false;
        public bool isPosHztlMoveInputAxisValid = true;
        public bool isNegHztlMoveInputAxisValid = true;
        public bool isPosVertMoveInputAxisValid = true;
        public bool isNegVertMoveInputAxisValid = true;
        public bool isJumpInputAxisValid = true;
        public bool isSprintInputAxisValid = true;
        public bool isCrouchInputAxisValid = true;
        public bool isJetPackInputAxisValid = true;
        public bool isSwitchLookInputAxisValid = true;
        public bool isPosHztlLookInputAxisValid = true;
        public bool isNegHztlLookInputAxisValid = true;
        public bool isPosVertLookInputAxisValid = true;
        public bool isNegVertLookInputAxisValid = true;
        public bool isPosZoomLookInputAxisValid = true;
        public bool isNegZoomLookInputAxisValid = true;
        public bool isPosOrbitLookInputAxisValid = true;
        public bool isNegOrbitLookInputAxisValid = true;
        public bool isLeftFire1InputAxisValid = true;
        public bool isLeftFire2InputAxisValid = true;
        public bool isRightFire1InputAxisValid = true;
        public bool isRightFire2InputAxisValid = true;
        #endregion

        #region Unity Input System
        // Unity Input System stores actionId internally as string rather than System.Guid
        // We don't require positive and negative as Input System supports Composite Bindings
        public string posHztlMoveInputActionIdUIS = "";
        public string posVertMoveInputActionIdUIS = "";
        public string posHztlLookInputActionIdUIS = "";
        public string posVertLookInputActionIdUIS = "";
        public string posZoomLookInputActionIdUIS = "";
        public string posOrbitLookInputActionIdUIS = "";
        public string jumpInputActionIdUIS = "";
        public string sprintInputActionIdUIS = "";
        public string crouchInputActionIdUIS = "";
        public string jetpackInputActionIdUIS = "";
        public string switchLookInputActionIdUIS = "";
        public string leftFire1InputActionIdUIS = "";
        public string leftFire2InputActionIdUIS = "";
        public string rightFire1InputActionIdUIS = "";
        public string rightFire2InputActionIdUIS = "";
        // An Action can return a Control Type with or or more values.
        // e.g. bool, float, Vector2, Vector3. The S3D zero-based DataSlot indicates which value to use.
        // For composite bindings e.g. 2DVector/Dpad, the slot matches the axis.
        // Left/Right = x-axis = slot 1, Up/Down = y-axis = slot 2.
        public int posHztlMoveInputActionDataSlotUIS = 0;
        public int posVertMoveInputActionDataSlotUIS = 0;
        public int posHztlLookInputActionDataSlotUIS = 0;
        public int posVertLookInputActionDataSlotUIS = 0;
        public int posZoomLookInputActionDataSlotUIS = 0;
        public int posOrbitLookInputActionDataSlotUIS = 0;
        public int jumpInputActionDataSlotUIS = 0;
        public int sprintInputActionDataSlotUIS = 0;
        public int crouchInputActionDataSlotUIS = 0;
        public int jetpackInputActionDataSlotUIS = 0;
        public int switchLookInputActionDataSlotUIS = 0;
        public int leftFire1InputActionDataSlotUIS = 0;
        public int leftFire2InputActionDataSlotUIS = 0;
        public int rightFire1InputActionDataSlotUIS = 0;
        public int rightFire2InputActionDataSlotUIS = 0;

        public bool isMouseForHztlLookEnabledUIS = false;
        public bool isMouseForVertLookEnabledUIS = false;

        // Currently only used with Mouse
        [Range(0.1f, 1.0f)] public float hztlLookSensitivityUIS = 0.5f;
        [Range(0.1f, 1.0f)] public float vertLookSensitivityUIS = 0.5f;
        [Range(0f, 0.25f)] public float hztrLookDeadzoneUIS = 0.05f;
        [Range(0f, 0.25f)] public float vertLookDeadzoneUIS = 0.05f;
        #endregion

        #region Rewired

        public int rewiredPlayerNumber = 0; // Human player number 1+ (0 = no player assigned)
        // Action Ids are more efficient than using Action Friendly Names
        public int posHztlMoveInputActionIdRWD = -1;
        public int negHztlMoveInputActionIdRWD = -1;
        public int posVertMoveInputActionIdRWD = -1;
        public int negVertMoveInputActionIdRWD = -1;
        public int posHztlLookInputActionIdRWD = -1;
        public int negHztlLookInputActionIdRWD = -1;
        public int posVertLookInputActionIdRWD = -1;
        public int negVertLookInputActionIdRWD = -1;
        public int posZoomLookInputActionIdRWD = -1;
        public int negZoomLookInputActionIdRWD = -1;
        public int posOrbitLookInputActionIdRWD = -1;
        public int negOrbitLookInputActionIdRWD = -1;
        public int jumpInputActionIdRWD = -1;
        public int sprintInputActionIdRWD = -1;
        public int crouchInputActionIdRWD = -1;
        public int jetpackInputActionIdRWD = -1;
        public int switchLookInputActionIdRWD = -1;
        public int leftFire1InputActionIdRWD = -1;
        public int leftFire2InputActionIdRWD = -1;
        public int rightFire1InputActionIdRWD = -1;
        public int rightFire2InputActionIdRWD = -1;

        #endregion

        #region UnityXR

        public InputAxisMode hmdPosInputAxisMode = InputAxisMode.SingleAxis;
        public InputAxisMode leftHandPosInputAxisMode = InputAxisMode.SingleAxis;
        public InputAxisMode leftHandRotInputAxisMode = InputAxisMode.SingleAxis;
        public InputAxisMode rightHandPosInputAxisMode = InputAxisMode.SingleAxis;
        public InputAxisMode rightHandRotInputAxisMode = InputAxisMode.SingleAxis;

        // Unity Input System stores actionId internally as string rather than System.Guid
        public string hmdPosInputActionIdXR = "";
        public string leftHandPosInputActionIdXR = "";
        public string leftHandRotInputActionIdXR = "";
        public string rightHandPosInputActionIdXR = "";
        public string rightHandRotInputActionIdXR = "";

        // An Action can return a Control Type with or or more values.
        // e.g. bool, float, Vector2, Vector3. The S3D zero-based DataSlot indicates which value to use.
        // For composite bindings e.g. 2DVector/Dpad, the slot matches the axis.
        // Left/Right = x-axis = slot 1, Up/Down = y-axis = slot 2.
        public int hmdPosInputActionDataSlotXR = 0;
        public int leftHandPosInputActionDataSlotXR = 0;
        public int leftHandRotInputActionDataSlotXR = 0;
        public int rightHandPosInputActionDataSlotXR = 0;
        public int rightHandRotInputActionDataSlotXR = 0;

        // Unity Input System (which XR uses) stores actionMapId internally as string rather than System.Guid
        // We don't require positive and negative as Input System supports Composite Bindings

        public string posHztlMoveInputActionMapIdXR = "";
        public string posVertMoveInputActionMapIdXR = "";
        public string posHztlLookInputActionMapIdXR = "";
        public string posVertLookInputActionMapIdXR = "";
        public string posZoomLookInputActionMapIdXR = "";
        public string posOrbitLookInputActionMapIdXR = "";
        public string jumpInputActionMapIdXR = "";
        public string sprintInputActionMapIdXR = "";
        public string crouchInputActionMapIdXR = "";
        public string jetpackInputActionMapIdXR = "";
        public string switchLookInputActionMapIdXR = "";
        public string leftFire1InputActionMapIdXR = "";
        public string leftFire2InputActionMapIdXR = "";
        public string rightFire1InputActionMapIdXR = "";
        public string rightFire2InputActionMapIdXR = "";

        public string hmdPosInputActionMapIdXR = "";
        public string leftHandPosInputActionMapIdXR = "";
        public string leftHandRotInputActionMapIdXR = "";
        public string rightHandPosInputActionMapIdXR = "";
        public string rightHandRotInputActionMapIdXR = "";

        #endregion UnityXR

        #region Custom Inputs
        public List<CustomInput> customInputList;
        public bool isCustomInputListExpanded;
        #endregion

        #endregion

        #region Private or Internal Variables - General
        private StickyControlModule stickyControlModule = null;
        private bool isInputEnabled = false;
        private CharacterInput characterInput;
        private CharacterInput lastCharacterInput;
        private CharacterInputXR characterInputXR = null;
        private int numberOfCustomInputs = 0;
        private S3DRamp scrollWheelRamp = null;
        private bool isJetPackEnabled = false;

        /// <summary>
        /// This is true when everything except CustomInputs are disabled.
        /// See DisableInput(..) and EnableInput()
        /// </summary>
        private bool isCustomInputsOnlyEnabled = false;
        #endregion 

        #region Private Variables - Unity Input System
        #if SSC_UIS
        // Ref to a Player Input component that should be attached to the same gameobject
        private UnityEngine.InputSystem.PlayerInput uisPlayerInput = null;
        // Used to map positive[inputtype]InputActionIdUIS to InputAction at runtime
        // We don't require positive and negative as Input System supports Composite Bindings
        // See UpdateUnityInputSystemActions()
        private UnityEngine.InputSystem.InputAction posHztlMoveInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction posVertMoveInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction posHztlLookInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction posVertLookInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction posZoomLookInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction posOrbitLookInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction jumpInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction sprintInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction crouchInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction jetpackInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction switchLookInputActionUIS = null;
        private UnityEngine.InputSystem.InputAction leftFire1InputActionUIS = null;
        private UnityEngine.InputSystem.InputAction leftFire2InputActionUIS = null;
        private UnityEngine.InputSystem.InputAction rightFire1InputActionUIS = null;
        private UnityEngine.InputSystem.InputAction rightFire2InputActionUIS = null;

        // InputAction Control Types can be Axis, Vector2/Dpad, Vector3 + others.
        // They are stored in inputAction.expectedControlType as a string. To avoid GC
        // in Update, convert them to a hashed int32 value.
        private int posHztlMoveCtrlTypeHashUIS = 0;
        private int posVertMoveCtrlTypeHashUIS = 0;
        private int posHztlLookCtrlTypeHashUIS = 0;
        private int posVertLookCtrlTypeHashUIS = 0;
        private int posZoomLookCtrlTypeHashUIS = 0;
        private int posOrbitLookCtrlTypeHashUIS = 0;
        private int jumpCtrlTypeHashUIS = 0;
        private int sprintCtrlTypeHashUIS = 0;
        private int crouchCtrlTypeHashUIS = 0;
        private int jetpackCtrlTypeHashUIS = 0;
        private int switchLookCtrlTypeHashUIS = 0;
        private int leftFire1CtrlTypeHashUIS = 0;
        private int leftFire2CtrlTypeHashUIS = 0;
        private int rightFire1CtrlTypeHashUIS = 0;
        private int rightFire2CtrlTypeHashUIS = 0;

        private int controlTypeButtonHashUIS = 0;
        private int controlTypeAxisHashUIS = 0;
        private int controlTypeVector2HashUIS = 0;
        private int controlTypeDpadHashUIS = 0;
        private int controlTypeVector3HashUIS = 0;
        private int controlTypeQuaternionHashUIS = 0;
        #endif

        #endregion

        #region Private Variables - Rewired
#if SSC_REWIRED
        private Rewired.Player rewiredPlayer = null;

        // Local variables that get populated at runtime
        // Use int rather than enum for speed
        // 0 = Axis, 1 = Button
        // See UpdateRewiredActionTypes()
        private int posHztlMoveInputActionTypeRWD = 0;
        private int negHztlMoveInputActionTypeRWD = 0;
        private int posVertMoveInputActionTypeRWD = 0;
        private int negVertMoveInputActionTypeRWD = 0;
        private int posHztlLookInputActionTypeRWD = 0;
        private int negHztlLookInputActionTypeRWD = 0;
        private int posVertLookInputActionTypeRWD = 0;
        private int negVertLookInputActionTypeRWD = 0;
        private int posZoomLookInputActionTypeRWD = 0;
        private int negZoomLookInputActionTypeRWD = 0;
        private int posOrbitLookInputActionTypeRWD = 0;
        private int negOrbitLookInputActionTypeRWD = 0;
        private int jumpInputActionTypeRWD = 0;
        private int sprintInputActionTypeRWD = 0;
        private int crouchInputActionTypeRWD = 0;
        private int jetpackInputActionTypeRWD = 0;
        private int switchLookInputActionTypeRWD = 0;
        private int leftFire1InputActionTypeRWD = 0;
        private int leftFire2InputActionTypeRWD = 0;
        private int rightFire1InputActionTypeRWD = 0;
        private int rightFire2InputActionTypeRWD = 0;
#endif
        #endregion

        #region Private Variables - UnityXR

        #if SCSM_XR && SSC_UIS
        /// <summary>
        /// The UIS scriptable object that contains the action maps and input actions.
        /// </summary>
        [SerializeField] private UnityEngine.InputSystem.InputActionAsset inputActionAssetXR = null;

        /// <summary>
        /// The offset rotation, in Euler angles. Used to correct hand controller rotation
        /// </summary>
        [SerializeField] private Vector3 leftHandOffsetRotXR = Vector3.zero;

        /// <summary>
        /// The offset rotation, in Euler angles. Used to correct hand controller rotation
        /// </summary>
        [SerializeField] private Vector3 rightHandOffsetRotXR = Vector3.zero;

        /// <summary>
        /// The transform of the XR left hand
        /// </summary>
        [SerializeField] private Transform leftHandTransformXR = null;

        /// <summary>
        /// The transform of the XR right hand
        /// </summary>
        [SerializeField] private Transform rightHandTransformXR = null;

        // Whether the input sections are shown as expanded in the inspector window of the editor.
        [SerializeField] private bool hmdPosShowInEditor;
        [SerializeField] private bool leftHandPosShowInEditor;
        [SerializeField] private bool leftHandRotShowInEditor;
        [SerializeField] private bool rightHandPosShowInEditor;
        [SerializeField] private bool rightHandRotShowInEditor;

        // Initial local space rotation of the XR Hands Offset transform
        //Vector3 xrHandsOffsetInitRotation = Vector3.zero;

        // Used to map [inputtype]InputActionIdXR to InputAction at runtime
        // See UpdateUnityXRActions()
        private UnityEngine.InputSystem.InputAction hmdPosInputActionXR = null;
        private UnityEngine.InputSystem.InputAction rightHandPosInputActionXR = null;
        private UnityEngine.InputSystem.InputAction rightHandRotInputActionXR = null;
        private UnityEngine.InputSystem.InputAction leftHandPosInputActionXR = null;
        private UnityEngine.InputSystem.InputAction leftHandRotInputActionXR = null;

        // InputAction Control Types can be Axis, Vector2/Dpad, Vector3 + others.
        // They are stored in inputAction.expectedControlType as a string. To avoid GC
        // in Update, convert them to a hashed int32 value.
        private int hmdPosCtrlTypeHashXR = 0;
        private int rightHandPosCtrlTypeHashXR = 0;
        private int rightHandRotCtrlTypeHashXR = 0;
        private int leftHandPosCtrlTypeHashXR = 0;
        private int leftHandRotCtrlTypeHashXR = 0;

        internal static readonly List<XRInputSubsystem> xrInputSubsystemList = new List<XRInputSubsystem>();

        // Device-based input - will at some point convert to UnityEngine.InputSystem.InputDevice leftHandInputDeviceUIS
        private UnityEngine.XR.InputDevice leftHandInputDeviceXR;
        private UnityEngine.XR.InputDevice rightHandInputDeviceXR;
        private UnityEngine.XR.InputDevice hmdInputDeviceXR;

        // Quest2 HeldInHand, TrackedDevice, Controller, Left
        private InputDeviceCharacteristics leftHandCharacteristicsXR = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller;
        private InputDeviceCharacteristics rightHandCharacteristicsXR = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller;
        private InputDeviceCharacteristics hmdCharacteristicsXR = InputDeviceCharacteristics.HeadMounted;

        private WaitForSeconds xrWaitForSeconds = null;
        private bool isInitialisedXR = false;
        private bool isInitialisingXR = false;
        private int initialiseXRCount = 0;
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

        private void StickyInputUpdate(bool isLateUpdate)
        {
            if (!isInputEnabled && !isCustomInputsOnlyEnabled) { return; }

            if (inputMode == InputMode.DirectKeyboard)
            {
                #region Direct Keyboard and Mouse
                if (isInputEnabled)
                {
                    characterInput.horizontalMove = 0f;
                    characterInput.verticalMove = 0f;
                    characterInput.jump = false;
                    characterInput.crouch = false;
                    characterInput.crouchIsToggled = crouchIsToggled;
                    characterInput.jetpack = false;
                    characterInput.switchLook = false;
                    characterInput.sprint = false;
                    characterInput.horizontalLook = 0f;
                    characterInput.verticalLook = 0f;
                    characterInput.zoomLook = 0f;
                    characterInput.orbitLook = 0f;
                    characterInput.leftFire1 = false;
                    characterInput.leftFire2 = false;
                    characterInput.rightFire1 = false;
                    characterInput.rightFire2 = false;

                    #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER

                    isJetPackEnabled = stickyControlModule.IsJetPackEnabled;

                    if (Input.GetKey(posHztrlMoveInputKeycode)) { characterInput.horizontalMove += 1f; }
                    if (Input.GetKey(negHztrlMoveInputKeycode)) { characterInput.horizontalMove -= 1f; }

                    if (Input.GetKey(posVertMoveInputKeycode)) { characterInput.verticalMove += 1f; }
                    if (Input.GetKey(negVertMoveInputKeycode)) { characterInput.verticalMove -= 1f; }

                    if (jumpCanBeHeld) { characterInput.jump = Input.GetKey(jumpInputKeycode); }
                    else { characterInput.jump = Input.GetKeyDown(jumpInputKeycode); }
                    
                    if (!crouchIsJetPackOnly || isJetPackEnabled)
                    {
                        if (crouchCanBeHeld && (isJetPackEnabled || !crouchIsToggled)) { characterInput.crouch = Input.GetKey(crouchInputKeycode); }
                        else { characterInput.crouch = Input.GetKeyDown(crouchInputKeycode); }
                    }

                    characterInput.sprint = Input.GetKey(sprintInputKeycode);
                    characterInput.jetpack = Input.GetKeyDown(jetpackInputKeycode);
                    characterInput.switchLook = Input.GetKeyDown(switchLookInputKeycode);

                    // Look around always uses mouse or trackpad for Direct Keyboard (and Mouse) input
                    if (isMouseCentreHztlLookDKI || isMouseHztlLookInputAxisValidDKI)
                    {
                        characterInput.horizontalLook = CalculateMouseXInput
                        (
                            isMouseCentreHztlLookDKI ? Input.mousePosition.x : characterInput.horizontalLook = Input.GetAxis(mouseHztlLookInputAxisNameDKI),
                            hztlLookSensitivityDKI, hztlLookDeadzoneDKI, isMouseCentreHztlLookDKI
                        );
                    }

                    // For mouse look, we invert the calculated Y value
                    if (isMouseCentreVertLookDKI || isMouseVertLookInputAxisValidDKI)
                    {
                        characterInput.verticalLook = CalculateMouseYInput
                        (
                            isMouseCentreVertLookDKI ? Input.mousePosition.y : characterInput.verticalLook = Input.GetAxis(mouseVertLookInputAxisNameDKI),
                            vertLookSensitivityDKI, vertLookDeadzoneDKI, isMouseCentreVertLookDKI
                        );
                    }

                    //characterInput.verticalLook = -CalculateMouseYInput(Input.mousePosition.y, vertLookSensitivityDKI, vertLookDeadzoneDKI, isMouseCentreVertLookDKI);

                    // Mouse Scroll wheel.  NOTE: It may require some kind of scale factor if it is too fast.
                    // Currently this is non-continuous in that only get a single value within a scrolling "session"
                    if (isMouseScrollForZoomEnabledDKI)
                    {
                        scrollWheelRamp.currentInput = Input.mouseScrollDelta.y;
                        scrollWheelRamp.SmoothInput(Time.deltaTime);
                        characterInput.zoomLook = scrollWheelRamp.currentInput;
                    }

                    if (Input.GetKey(posZoomInputKeycode)) { characterInput.zoomLook += 1f; }
                    if (Input.GetKey(negZoomInputKeycode)) { characterInput.zoomLook -= 1f; }

                    if (Input.GetKey(posOrbitInputKeycode)) { characterInput.orbitLook += 1f; }
                    if (Input.GetKey(negOrbitInputKeycode)) { characterInput.orbitLook -= 1f; }

                    if (leftFire1CanBeHeld) { characterInput.leftFire1 = Input.GetKey(leftFire1InputKeycode); }
                    else { characterInput.leftFire1 = Input.GetKeyDown(leftFire1InputKeycode); }

                    if (leftFire2CanBeHeld) { characterInput.leftFire2 = Input.GetKey(leftFire2InputKeycode); }
                    else { characterInput.leftFire2 = Input.GetKeyDown(leftFire2InputKeycode); }

                    if (rightFire1CanBeHeld) { characterInput.rightFire1 = Input.GetKey(rightFire1InputKeycode); }
                    else { characterInput.rightFire1 = Input.GetKeyDown(rightFire1InputKeycode); }

                    if (rightFire2CanBeHeld) { characterInput.rightFire2 = Input.GetKey(rightFire2InputKeycode); }
                    else { characterInput.rightFire2 = Input.GetKeyDown(rightFire2InputKeycode); }

                    #endif
                }

                #region Custom Input
                #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
                if (numberOfCustomInputs > 0)
                {
                    for (int cpiIdx = 0; cpiIdx < numberOfCustomInputs; cpiIdx++)
                    {
                        CustomInput customInput = customInputList[cpiIdx];

                        if (customInput != null)
                        {
                            Vector3 evtVector = Vector3.zero;

                            if (customInput.canBeHeldDown)
                            {
                                if (Input.GetKey(customInput.dkmPositiveKeycode)) { evtVector.x = 1f; }
                                if (!customInput.isButton && Input.GetKey(customInput.dkmNegativeKeycode)) { evtVector.x = -1f; }
                            }
                            else
                            {
                                if (Input.GetKeyDown(customInput.dkmPositiveKeycode)) { evtVector.x = 1f; }
                                if (!customInput.isButton && Input.GetKeyDown(customInput.dkmNegativeKeycode)) { evtVector.x = -1f; }
                            }

                            if (evtVector.x != 0f)
                            {
                                // CustomInput.CustomInputEventType is Key (10).
                                if (customInput.customInputEvt != null) { customInput.customInputEvt.Invoke(evtVector, 10); }
                            }

                            // Send anim action values to the stickyControlModule when non-zero value or not a button
                            if (customInput.numCustomAnimActions > 0 && (evtVector.x != 0f || !customInput.isButton)) { SetAninActionValues(customInput, evtVector); }
                        }
                    }
                }
                #endif
                #endregion Custom Input

                #endregion
            }
            else if (inputMode == InputMode.LegacyUnity)
            {
                #region Legacy Unity Input

                #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
                if (isInputEnabled)
                {
                    isJetPackEnabled = stickyControlModule.IsJetPackEnabled;    

                    #region Legacy Unity Input System Horizontal Move Input

                    // Horizontal Move input
                    if (hztlMoveInputAxisMode != InputAxisMode.NoInput && isPosHztlMoveInputAxisValid)
                    {
                        if (hztlMoveInputAxisMode == InputAxisMode.SingleAxis)
                        {
                            // Single axis
                            characterInput.horizontalMove = Input.GetAxis(posHztlMoveInputAxisName);
                            if (invertHztlMoveInputAxis) { characterInput.horizontalMove *= -1f; }

                        }
                        else if (isNegHztlMoveInputAxisValid)
                        {
                            // Combined axis - two axes combined together for a single input
                            characterInput.horizontalMove = Input.GetAxis(posHztlMoveInputAxisName) - Input.GetAxis(negHztlMoveInputAxisName);
                        }
                        else { characterInput.horizontalMove = 0f; }
                    }
                    else { characterInput.horizontalMove = 0f; }

                    #endregion Legacy Unity Input System Horizontal Move Input

                    #region Legacy Unity Input System Vertical Move Input

                    // Vertical Move input
                    if (vertMoveInputAxisMode != InputAxisMode.NoInput && isPosVertMoveInputAxisValid)
                    {
                        if (vertMoveInputAxisMode == InputAxisMode.SingleAxis)
                        {
                            // Single axis
                            characterInput.verticalMove = Input.GetAxis(posVertMoveInputAxisName);
                            if (invertVertMoveInputAxis) { characterInput.verticalMove *= -1f; }
                        }
                        else if (isNegVertMoveInputAxisValid)
                        {
                            // Combined axis - two axes combined together for a single input
                            characterInput.verticalMove = Input.GetAxis(posVertMoveInputAxisName) - Input.GetAxis(negVertMoveInputAxisName);
                        }
                        else { characterInput.verticalMove = 0f; }
                    }
                    else { characterInput.verticalMove = 0f; }

                    #endregion Legacy Unity Input System Vertical Move Input

                    #region Legacy Unity Input System Horizontal Look Input

                    // Horizontal Look input
                    if (hztlLookInputAxisMode != InputAxisMode.NoInput && isPosHztlLookInputAxisValid)
                    {
                        if (hztlLookInputAxisMode == InputAxisMode.SingleAxis)
                        {
                            // Single axis
                            characterInput.horizontalLook = Input.GetAxis(posHztlLookInputAxisName);
                            if (invertHztlLookInputAxis) { characterInput.horizontalLook *= -1f; }
                        }
                        else if (isNegHztlLookInputAxisValid)
                        {
                            // Combined axis - two axes combined together for a single input
                            characterInput.horizontalLook = Input.GetAxis(posHztlLookInputAxisName) - Input.GetAxis(negHztlLookInputAxisName);
                        }
                        else { characterInput.horizontalLook = 0f; }
                    }
                    else { characterInput.horizontalLook = 0f; }

                    #endregion Legacy Unity Input System Horizontal Look Input

                    #region Legacy Unity Input System Vertical Look Input

                    // Vertical Look input
                    if (vertLookInputAxisMode != InputAxisMode.NoInput && isPosVertLookInputAxisValid)
                    {
                        if (vertLookInputAxisMode == InputAxisMode.SingleAxis)
                        {
                            // Single axis
                            characterInput.verticalLook = Input.GetAxis(posVertLookInputAxisName);
                            if (invertVertLookInputAxis) { characterInput.verticalLook *= -1f; }
                        }
                        else if (isNegVertLookInputAxisValid)
                        {
                            // Combined axis - two axes combined together for a single input
                            characterInput.verticalLook = Input.GetAxis(posVertLookInputAxisName) - Input.GetAxis(negVertLookInputAxisName);
                        }
                        else { characterInput.verticalLook = 0f; }
                    }
                    else { characterInput.verticalLook = 0f; }

                    #endregion Legacy Unity Input System Vertical Look Input

                    #region Legacy Unity Input System Sprint Input

                    characterInput.sprint = isSprintInputAxisValid ? Input.GetButton(sprintInputAxisName) : false;

                    #endregion Legacy Unity Input System Sprint Input

                    #region Legacy Unity Input System Jump Input

                    if (isJumpInputAxisValid)
                    {
                        if (jumpCanBeHeld) { characterInput.jump = Input.GetButton(jumpInputAxisName); }
                        else { characterInput.jump = Input.GetButtonDown(jumpInputAxisName); }
                    }
                    else { characterInput.jump = false; }

                    #endregion Legacy Unity Input System Jump Input

                    #region Legacy Unity Input System Crouch Input

                    if (isCrouchInputAxisValid && (!crouchIsJetPackOnly || isJetPackEnabled))
                    {
                        if (crouchCanBeHeld && (isJetPackEnabled || !crouchIsToggled)) { characterInput.crouch = Input.GetButton(crouchInputAxisName); }
                        else { characterInput.crouch = Input.GetButtonDown(crouchInputAxisName); }

                        characterInput.crouchIsToggled = crouchIsToggled;
                    }
                    else { characterInput.crouch = false; }

                    #endregion Legacy Unity Input System Crouch Input

                    #region Legacy Unity Input System Jetpack Input

                    characterInput.jetpack = isJetPackInputAxisValid ? Input.GetButtonDown(jetpackInputAxisName): false;

                    #endregion Legacy Unity Input System JetPack Input

                    #region Legacy Unity Input System Switch Look Input

                    characterInput.switchLook = isSwitchLookInputAxisValid ? Input.GetButtonDown(switchLookInputAxisName): false;

                    #endregion Legacy Unity Input System Switch look Input

                    #region Legacy Unity Input System Zoom Look Input

                    // Zoom Look input
                    if (zoomLookInputAxisMode != InputAxisMode.NoInput && isPosZoomLookInputAxisValid)
                    {
                        if (zoomLookInputAxisMode == InputAxisMode.SingleAxis)
                        {
                            // Single axis
                            // Currently this is non-continuous in that only get a single value within a mouse scroll wheel "session"

                            scrollWheelRamp.currentInput = Input.GetAxis(posZoomLookInputAxisName);
                            scrollWheelRamp.SmoothInput(Time.deltaTime);
                            characterInput.zoomLook = scrollWheelRamp.currentInput;
                            
                            //characterInput.zoomLook = Input.GetAxis(posZoomLookInputAxisName);
                            if (invertZoomLookInputAxis) { characterInput.zoomLook *= -1f; }
                        }
                        else if (isNegZoomLookInputAxisValid)
                        {
                            // Combined axis - two axes combined together for a single input
                            characterInput.zoomLook = Input.GetAxis(posZoomLookInputAxisName) - Input.GetAxis(negZoomLookInputAxisName);
                        }
                        else { characterInput.zoomLook = 0f; }
                    }
                    else { characterInput.zoomLook = 0f; }

                    #endregion Legacy Unity Input System Zoom Look Input

                    #region Legacy Unity Input System Orbit Look Input

                    // Orbit Look input
                    if (orbitLookInputAxisMode != InputAxisMode.NoInput && isPosOrbitLookInputAxisValid)
                    {
                        if (orbitLookInputAxisMode == InputAxisMode.SingleAxis)
                        {
                            // Single axis
                            characterInput.zoomLook = Input.GetAxis(posOrbitLookInputAxisName);
                            if (invertOrbitLookInputAxis) { characterInput.orbitLook *= -1f; }
                        }
                        else if (isNegOrbitLookInputAxisValid)
                        {
                            // Combined axis - two axes combined together for a single input
                            characterInput.orbitLook = Input.GetAxis(posOrbitLookInputAxisName) - Input.GetAxis(negOrbitLookInputAxisName);
                        }
                        else { characterInput.orbitLook = 0f; }
                    }
                    else { characterInput.orbitLook = 0f; }

                    #endregion Legacy Unity Input System Orbit Look Input

                    #region Legacy Unity Input System Left Fire 1 Input

                    if (isLeftFire1InputAxisValid)
                    {
                        if (leftFire1CanBeHeld) { characterInput.leftFire1 = Input.GetButton(leftFire1InputAxisName); }
                        else { characterInput.leftFire1 = Input.GetButtonDown(leftFire1InputAxisName); }
                    }
                    else { characterInput.leftFire1 = false; }

                    #endregion Legacy Unity Input System Left Fire 1 Input

                    #region Legacy Unity Input System Left Fire 2 Input

                    if (isLeftFire2InputAxisValid)
                    {
                        if (leftFire2CanBeHeld) { characterInput.leftFire2 = Input.GetButton(leftFire2InputAxisName); }
                        else { characterInput.leftFire2 = Input.GetButtonDown(leftFire2InputAxisName); }
                    }
                    else { characterInput.leftFire2 = false; }

                    #endregion Legacy Unity Input System Left Fire 2 Input

                    #region Legacy Unity Input System Right Fire 1 Input

                    if (isRightFire1InputAxisValid)
                    {
                        if (rightFire1CanBeHeld) { characterInput.rightFire1 = Input.GetButton(rightFire1InputAxisName); }
                        else { characterInput.rightFire1 = Input.GetButtonDown(rightFire1InputAxisName); }
                    }
                    else { characterInput.rightFire1 = false; }

                    #endregion Legacy Unity Input System Right Fire 1 Input

                    #region Legacy Unity Input System Right Fire 2 Input

                    if (isRightFire2InputAxisValid)
                    {
                        if (rightFire2CanBeHeld) { characterInput.rightFire2 = Input.GetButton(rightFire2InputAxisName); }
                        else { characterInput.rightFire2 = Input.GetButtonDown(rightFire2InputAxisName); }
                    }
                    else { characterInput.rightFire2 = false; }

                    #endregion Legacy Unity Input System Right Fire 2 Input
                }

                #region Custom Input
                if (numberOfCustomInputs > 0)
                {
                    for (int cpiIdx = 0; cpiIdx < numberOfCustomInputs; cpiIdx++)
                    {
                        CustomInput customInput = customInputList[cpiIdx];

                        if (customInput != null && customInput.lisIsPositiveAxisValid)
                        {
                            Vector3 inputValue = Vector3.zero;

                            if (customInput.isButton)
                            {
                                // CustomInput.CustomInputEventType is Button (5).
                                if (customInput.canBeHeldDown && Input.GetButton(customInput.lisPositiveAxisName)) { inputValue.x = 1f; }
                                else if (Input.GetButtonDown(customInput.lisPositiveAxisName)) { inputValue.x = 1f; }

                                if (inputValue.x != 0f && customInput.customInputEvt != null) { customInput.customInputEvt.Invoke(inputValue, 5); } 
                            }
                            else if (customInput.inputAxisMode == InputAxisMode.SingleAxis)
                            {
                                inputValue.x = Input.GetAxis(customInput.lisPositiveAxisName) * (customInput.lisInvertAxis ? -1f : 1f);
                                if (customInput.isSensitivityEnabled)
                                {
                                    inputValue.x = CalculateAxisInput(customInput.lastInputValueX, inputValue.x, customInput.sensitivity, customInput.gravity);
                                    customInput.lastInputValueX = inputValue.x;
                                    customInput.lastInputValueY = 0f;
                                }
                                // CustomInput.CustomInputEventType is Axis1D (1).
                                if (customInput.customInputEvt != null) { customInput.customInputEvt.Invoke(inputValue, 1); }
                            }
                            else if (customInput.inputAxisMode == InputAxisMode.CombinedAxis && customInput.lisIsNegativeAxisValid)
                            {
                                // Combined axis - two axes combined together for a single input
                                inputValue.x = (Input.GetAxis(customInput.lisPositiveAxisName) - Input.GetAxis(customInput.lisNegativeAxisName)) * (customInput.lisInvertAxis ? -1f : 1f);
                                if (customInput.isSensitivityEnabled)
                                {
                                    inputValue.x = CalculateAxisInput(customInput.lastInputValueX, inputValue.x, customInput.sensitivity, customInput.gravity);
                                    customInput.lastInputValueX = inputValue.x;
                                    customInput.lastInputValueY = 0f;
                                }
                                // CustomInput.CustomInputEventType is Axis1D (1).
                                if (customInput.customInputEvt != null) { customInput.customInputEvt.Invoke(inputValue, 1); }
                            }

                            // Send anim action values to the stickyControlModule when not a button or there is a non-zero value 
                            if (customInput.numCustomAnimActions > 0 && (inputValue.x != 0f || !customInput.isButton)) { SetAninActionValues(customInput, inputValue); }
                        }
                    }
                }
                #endregion Custom Input

                #else
                characterInput.horizontalMove = 0f;
                characterInput.verticalMove = 0f;
                characterInput.jump = false;
                characterInput.crouch = false;
                characterInput.crouchIsToggled = crouchIsToggled;
                characterInput.jetpack = false;
                characterInput.switchLook = false;
                characterInput.sprint = false;
                characterInput.horizontalLook = 0f;
                characterInput.verticalLook = 0f;
                characterInput.xrLook = Quaternion.identity;
                characterInput.zoomLook = 0f;
                characterInput.orbitLook = 0f;
                characterInput.leftFire1 = false;
                characterInput.leftFire2 = false;
                characterInput.rightFire1 = false;
                characterInput.rightFire2 = false;
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
                    isJetPackEnabled = stickyControlModule.IsJetPackEnabled;

                    #region Unity Input System Horizontal Move Input
                    characterInput.horizontalMove = UISReadActionInputFloat(posHztlMoveInputActionUIS, posHztlMoveInputActionDataSlotUIS, posHztlMoveCtrlTypeHashUIS);
                    #endregion Unity Input System Horizontal Move Input

                    #region Unity Input System Vertical Move Input
                    characterInput.verticalMove = UISReadActionInputFloat(posVertMoveInputActionUIS, posVertMoveInputActionDataSlotUIS, posVertMoveCtrlTypeHashUIS);
                    #endregion Unity Input System Vertical Move Input

                    #region Unity Input System Sprint Input
                    characterInput.sprint = UISReadActionInputBool(sprintInputActionUIS, sprintInputActionDataSlotUIS, sprintCtrlTypeHashUIS, true);
                    #endregion Unity Input System Sprint Input

                    #region Unity Input System Jump Input
                    characterInput.jump = UISReadActionInputBool(jumpInputActionUIS, jumpInputActionDataSlotUIS, jumpCtrlTypeHashUIS, jumpCanBeHeld);
                    #endregion Unity Input System Jump Input

                    #region Unity Input System Crouch Input
                    if (!crouchIsJetPackOnly || isJetPackEnabled)
                    {
                        characterInput.crouchIsToggled = crouchIsToggled;
                        characterInput.crouch = UISReadActionInputBool(crouchInputActionUIS, crouchInputActionDataSlotUIS, crouchCtrlTypeHashUIS, crouchCanBeHeld && (!crouchIsToggled || isJetPackEnabled));
                    }
                    else { characterInput.crouch = false; }
                    #endregion Unity Input System Crouch Input

                    #region Unity Input System Jet Pack Input
                    characterInput.jetpack = UISReadActionInputBool(jetpackInputActionUIS, jetpackInputActionDataSlotUIS, jetpackCtrlTypeHashUIS, false);
                    #endregion Unity Input System Jet Pack Input

                    #region Unity Input System Horizontal Look Input
                    characterInput.horizontalLook = UISReadActionInputFloat(posHztlLookInputActionUIS, posHztlLookInputActionDataSlotUIS, posHztlLookCtrlTypeHashUIS);
                    #endregion Unity Input System Horizontal Look Input

                    #region Unity Input System Vertical Look Input
                    characterInput.verticalLook = UISReadActionInputFloat(posVertLookInputActionUIS, posVertLookInputActionDataSlotUIS, posVertLookCtrlTypeHashUIS);
                    #endregion Unity Input System Vertical Look Input

                    #region Unity Input System Switch Look Input
                    characterInput.switchLook = UISReadActionInputBool(switchLookInputActionUIS, switchLookInputActionDataSlotUIS, switchLookCtrlTypeHashUIS, false);
                    #endregion Unity Input System Switch Look Input

                    #region Unity Input System Zoom Look Input
                    characterInput.zoomLook = UISReadActionInputFloat(posZoomLookInputActionUIS, posZoomLookInputActionDataSlotUIS, posZoomLookCtrlTypeHashUIS);
                    #endregion Unity Input System Zoom Look Input

                    #region Unity Input System Orbit Look Input
                    characterInput.orbitLook = UISReadActionInputFloat(posOrbitLookInputActionUIS, posOrbitLookInputActionDataSlotUIS, posOrbitLookCtrlTypeHashUIS);
                    #endregion Unity Input System Orbit Look Input

                    #region Unity Input System Left Fire 1 Input
                    characterInput.leftFire1 = UISReadActionInputBool(leftFire1InputActionUIS, leftFire1InputActionDataSlotUIS, leftFire1CtrlTypeHashUIS, leftFire1CanBeHeld);
                    #endregion Unity Input System Left Fire 1 Input

                    #region Unity Input System Left Fire 2 Input
                    characterInput.leftFire2 = UISReadActionInputBool(leftFire2InputActionUIS, leftFire2InputActionDataSlotUIS, leftFire2CtrlTypeHashUIS, leftFire2CanBeHeld);
                    #endregion Unity Input System Left Fire 2 Input

                    #region Unity Input System Right Fire 1 Input
                    characterInput.rightFire1 = UISReadActionInputBool(rightFire1InputActionUIS, rightFire1InputActionDataSlotUIS, rightFire1CtrlTypeHashUIS, rightFire1CanBeHeld);
                    #endregion Unity Input System Right Fire 1 Input

                    #region Unity Input System Right Fire 2 Input
                    characterInput.rightFire2 = UISReadActionInputBool(rightFire2InputActionUIS, rightFire2InputActionDataSlotUIS, rightFire2CtrlTypeHashUIS, rightFire2CanBeHeld);
                    #endregion Unity Input System Right Fire 2 Input
                }

                #region Unity Input System Custom Input
                if (numberOfCustomInputs > 0)
                {
                    for (int ciIdx = 0; ciIdx < numberOfCustomInputs; ciIdx++)
                    {
                        CustomInput customInput = customInputList[ciIdx];

                        if (customInput != null)
                        {
                            Vector3 inputValue = Vector3.zero;

                            if (customInput.isButton)
                            {
                                if (UISReadActionInputBool(customInput.uisPositiveInputAction, customInput.uisPositiveInputActionDataSlot, customInput.uisPositiveCtrlTypeHash, customInput.canBeHeldDown))
                                {
                                    // CustomInput.CustomInputEventType is Button (5).
                                    inputValue.x = 1f;
                                    if (customInput.customInputEvt != null) { customInput.customInputEvt.Invoke(inputValue, 5); }
                                }
                            }
                            else
                            {
                                if (customInput.uisPositiveInputAction != null)
                                {
                                    if (customInput.uisPositiveCtrlTypeHash == controlTypeAxisHashUIS || customInput.uisPositiveCtrlTypeHash == controlTypeButtonHashUIS)
                                    {
                                        // 1D axis and Buttons both retrun a float
                                        inputValue.x = customInput.uisPositiveInputAction.ReadValue<float>();

                                        if (customInput.isSensitivityEnabled)
                                        {
                                            inputValue.x = CalculateAxisInput(customInput.lastInputValueX, inputValue.x, customInput.sensitivity, customInput.gravity);
                                        }
                                        customInput.lastInputValueX = inputValue.x;
                                        // CustomInput.CustomInputEventType is Axis1D (1).
                                        if (customInput.customInputEvt != null) { customInput.customInputEvt.Invoke(inputValue, 1); }
                                    }
                                    else if (customInput.uisPositiveCtrlTypeHash == controlTypeVector2HashUIS || customInput.uisPositiveCtrlTypeHash == controlTypeDpadHashUIS)
                                    {
                                        inputValue = customInput.uisPositiveInputAction.ReadValue<Vector2>();

                                        if (customInput.isSensitivityEnabled)
                                        {
                                            inputValue.x = CalculateAxisInput(customInput.lastInputValueX, inputValue.x, customInput.sensitivity, customInput.gravity);
                                            inputValue.y = CalculateAxisInput(customInput.lastInputValueY, inputValue.y, customInput.sensitivity, customInput.gravity);
                                        }
                                        customInput.lastInputValueX = inputValue.x;
                                        customInput.lastInputValueY = inputValue.y;
                                        // CustomInput.CustomInputEventType is Axis2D (2).
                                        if (customInput.customInputEvt != null) { customInput.customInputEvt.Invoke(inputValue, 2); }
                                    }
                                    else if (customInput.uisPositiveCtrlTypeHash == controlTypeVector3HashUIS)
                                    {
                                        inputValue = customInput.uisPositiveInputAction.ReadValue<Vector3>();

                                        if (customInput.isSensitivityEnabled)
                                        {
                                            // Currently only works on x,y axes
                                            inputValue.x = CalculateAxisInput(customInput.lastInputValueX, inputValue.x, customInput.sensitivity, customInput.gravity);
                                            inputValue.y = CalculateAxisInput(customInput.lastInputValueY, inputValue.y, customInput.sensitivity, customInput.gravity);
                                        }
                                        customInput.lastInputValueX = inputValue.x;
                                        customInput.lastInputValueY = inputValue.y;
                                        // CustomInput.CustomInputEventType is Axis3D (3).
                                        if (customInput.customInputEvt != null) { customInput.customInputEvt.Invoke(inputValue, 3); }
                                    }
                                }
                            }

                            // Send anim action values to the stickyControlModule when non-zero or not a button.
                            if (customInput.numCustomAnimActions > 0 && (inputValue != Vector3.zero || !customInput.isButton)) { SetAninActionValues(customInput, inputValue); }
                        }
                    }
                }
                #endregion Custom Input

                #else
                characterInput.horizontalMove = 0f;
                characterInput.verticalMove = 0f;
                characterInput.jump = false;
                characterInput.crouch = false;
                characterInput.crouchIsToggled = crouchIsToggled;
                characterInput.sprint = false;
                characterInput.jetpack = false;
                characterInput.switchLook = false;
                characterInput.horizontalLook = 0f;
                characterInput.verticalLook = 0f;
                characterInput.xrLook = Quaternion.identity;
                characterInput.zoomLook = 0f;
                characterInput.orbitLook = 0f;
                characterInput.leftFire1 = false;
                characterInput.leftFire2 = false;
                characterInput.rightFire1 = false;
                characterInput.rightFire2 = false;
                #endif

                #endregion Unity Input System
            }
            else if (inputMode == InputMode.Rewired)
            {
                #region Rewired
                #if SSC_REWIRED
                if (rewiredPlayer != null)
                {
                    // GetAxis is safe to call with an invalid ActionId
                    // Rewired ignores -ve values and gracefully handles missing actions
                    // by raising a single warning, rather than outputting a zillion msgs.
                    if (isInputEnabled)
                    {
                        isJetPackEnabled = stickyControlModule.IsJetPackEnabled;

                        #region Rewired Horizontal Move Input
                        if (hztlMoveInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (hztlMoveInputAxisMode == InputAxisMode.SingleAxis)
                            {                               
                                characterInput.horizontalMove = posHztlMoveInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(posHztlMoveInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(posHztlMoveInputActionIdRWD);
                            }
                            else
                            {
                                characterInput.horizontalMove = (posHztlMoveInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(posHztlMoveInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(posHztlMoveInputActionIdRWD)) -
                                                       (negHztlMoveInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(negHztlMoveInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(negHztlMoveInputActionIdRWD));
                            }
                        }
                        else { characterInput.horizontalMove = 0f; }
                        #endregion Rewired Horizontal Move Input

                        #region Rewired Vertical Move Input
                        if (vertMoveInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (vertMoveInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                characterInput.verticalMove = posVertMoveInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(posVertMoveInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(posVertMoveInputActionIdRWD);
                            }
                            else
                            {
                                characterInput.verticalMove = (posVertMoveInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(posVertMoveInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(posVertMoveInputActionIdRWD)) -
                                                       (negVertMoveInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(negVertMoveInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(negVertMoveInputActionIdRWD));
                            }
                        }
                        else { characterInput.verticalMove = 0f; }
                        #endregion Rewired Vertical Move Input

                        #region Rewired Horizontal Look Input
                        if (hztlLookInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (hztlLookInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                characterInput.horizontalLook = posHztlLookInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(posHztlLookInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(posHztlLookInputActionIdRWD);
                            }
                            else
                            {
                                characterInput.horizontalLook = (posHztlLookInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(posHztlLookInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(posHztlLookInputActionIdRWD)) -
                                                       (negHztlLookInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(negHztlLookInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(negHztlLookInputActionIdRWD));
                            }
                        }
                        else { characterInput.horizontalLook = 0f; }
                        #endregion Rewired Horizontal Look Input

                        #region Rewired Vertical Look Input
                        if (vertLookInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (vertLookInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                characterInput.verticalLook = posVertLookInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(posVertLookInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(posVertLookInputActionIdRWD);
                            }
                            else
                            {
                                characterInput.verticalLook = (posVertLookInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(posVertLookInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(posVertLookInputActionIdRWD)) -
                                                       (negVertLookInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(negVertLookInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(negVertLookInputActionIdRWD));
                            }
                        }
                        else { characterInput.verticalLook = 0f; }
                        #endregion Rewired Vertical Look Input

                        #region Rewired Sprint Input

                        // There is no need to check if it is enabled, because by default it is -1 and the StickyInputModuleEditor
                        // ensures it is -1 when not enabled. Also, it is possible to be enabled but not yet set to +ve ActionId
                        if (sprintInputActionIdRWD >= 0)
                        {
                            characterInput.sprint = sprintInputActionTypeRWD == 1 ? rewiredPlayer.GetButton(sprintInputActionIdRWD) : !characterInput.sprint && rewiredPlayer.GetAxis(sprintInputActionIdRWD) > 0.01f;
                        }
                        else { characterInput.sprint = false; }
                        #endregion Rewired Sprint Input

                        #region Rewired Jump Input

                        // There is no need to check if it is enabled, because by default it is -1 and the StickyInputModuleEditor
                        // ensures it is -1 when not enabled. Also, it is possible to be enabled but not yet set to +ve ActionId
                        if (jumpInputActionIdRWD >= 0)
                        {
                            if (jumpCanBeHeld)
                            {
                                characterInput.jump = jumpInputActionTypeRWD == 1 ? rewiredPlayer.GetButton(jumpInputActionIdRWD) : rewiredPlayer.GetAxis(jumpInputActionIdRWD) > 0.01f;
                            }
                            else
                            {
                                characterInput.jump = jumpInputActionTypeRWD == 1 ? rewiredPlayer.GetButtonDown(jumpInputActionIdRWD) : !characterInput.jump && rewiredPlayer.GetAxis(jumpInputActionIdRWD) > 0.01f;
                            }
                        }
                        else { characterInput.jump = false; }
                        #endregion Rewired Jump Input

                        #region Rewired Crouch Input

                        // There is no need to check if it is enabled, because by default it is -1 and the StickyInputModuleEditor
                        // ensures it is -1 when not enabled. Also, it is possible to be enabled but not yet set to +ve ActionId
                        if (crouchInputActionIdRWD >= 0 && (!crouchIsJetPackOnly || isJetPackEnabled))
                        {
                            characterInput.crouchIsToggled = crouchIsToggled;

                            if (crouchCanBeHeld && (!crouchIsToggled || isJetPackEnabled))
                            {
                                characterInput.crouch = crouchInputActionTypeRWD == 1 ? rewiredPlayer.GetButton(crouchInputActionIdRWD) : rewiredPlayer.GetAxis(crouchInputActionIdRWD) > 0.01f;
                            }
                            else
                            {
                                characterInput.crouch = crouchInputActionTypeRWD == 1 ? rewiredPlayer.GetButtonDown(crouchInputActionIdRWD) : !characterInput.crouch && rewiredPlayer.GetAxis(crouchInputActionIdRWD) > 0.01f;
                            }
                        }
                        else { characterInput.crouch = false; }
                        #endregion Rewired Crouch Input

                        #region Rewired Jet Pack Input

                        // There is no need to check if it is enabled, because by default it is -1 and the StickyInputModuleEditor
                        // ensures it is -1 when not enabled. Also, it is possible to be enabled but not yet set to +ve ActionId
                        if (jetpackInputActionIdRWD >= 0)
                        {
                            characterInput.jetpack = jetpackInputActionTypeRWD == 1 ? rewiredPlayer.GetButtonDown(jetpackInputActionIdRWD) : !characterInput.jetpack && rewiredPlayer.GetAxis(jetpackInputActionIdRWD) > 0.01f;
                        }
                        else { characterInput.jetpack = false; }
                        #endregion Rewired Jet Pack Input

                        #region Rewired Switch Look Input

                        // There is no need to check if it is enabled, because by default it is -1 and the StickyInputModuleEditor
                        // ensures it is -1 when not enabled. Also, it is possible to be enabled but not yet set to +ve ActionId
                        if (switchLookInputActionIdRWD >= 0)
                        {
                            characterInput.switchLook = switchLookInputActionTypeRWD == 1 ? rewiredPlayer.GetButtonDown(switchLookInputActionIdRWD) : !characterInput.switchLook && rewiredPlayer.GetAxis(switchLookInputActionIdRWD) > 0.01f;
                        }
                        else { characterInput.switchLook = false; }
                        #endregion Rewired Switch Look Input

                        #region Rewired Zoom Look Input
                        if (zoomLookInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (zoomLookInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                scrollWheelRamp.currentInput = posZoomLookInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(posZoomLookInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(posZoomLookInputActionIdRWD);
                            }
                            else
                            {
                                scrollWheelRamp.currentInput = (posZoomLookInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(posZoomLookInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(posZoomLookInputActionIdRWD)) -
                                                       (negZoomLookInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(negZoomLookInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(negZoomLookInputActionIdRWD));
                            }
                            scrollWheelRamp.SmoothInput(Time.deltaTime);
                            characterInput.zoomLook = scrollWheelRamp.currentInput;
                        }
                        else { characterInput.zoomLook = 0f; }
                        #endregion Rewired Zoom Look Input

                        #region Rewired Orbit Look Input
                        if (orbitLookInputAxisMode != InputAxisMode.NoInput)
                        {
                            if (orbitLookInputAxisMode == InputAxisMode.SingleAxis)
                            {
                                characterInput.orbitLook = posOrbitLookInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(posOrbitLookInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(posOrbitLookInputActionIdRWD);
                            }
                            else
                            {
                                characterInput.orbitLook = (posOrbitLookInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(posOrbitLookInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(posOrbitLookInputActionIdRWD)) -
                                                       (negOrbitLookInputActionTypeRWD == 1 ? (rewiredPlayer.GetButton(negOrbitLookInputActionIdRWD) ? 1f : 0f) : rewiredPlayer.GetAxis(negOrbitLookInputActionIdRWD));
                            }
                        }
                        else { characterInput.orbitLook = 0f; }
                        #endregion Rewired Orbit Look Input

                        #region Rewired Left Fire 1 Input
                        // There is no need to check if it is enabled, because by default it is -1 and the StickyInputModuleEditor
                        // ensures it is -1 when not enabled. Also, it is possible to be enabled but not yet set to +ve ActionId
                        if (leftFire1InputActionIdRWD >= 0)
                        {
                            if (leftFire1CanBeHeld)
                            {
                                characterInput.leftFire1 = leftFire1InputActionTypeRWD == 1 ? rewiredPlayer.GetButton(leftFire1InputActionIdRWD) : rewiredPlayer.GetAxis(leftFire1InputActionIdRWD) > 0.01f;
                            }
                            else
                            {
                                characterInput.leftFire1 = leftFire1InputActionTypeRWD == 1 ? rewiredPlayer.GetButtonDown(leftFire1InputActionIdRWD) : !characterInput.leftFire1 && rewiredPlayer.GetAxis(leftFire1InputActionIdRWD) > 0.01f;
                            }
                        }
                        else { characterInput.leftFire1 = false; }
                        #endregion Rewired Left Fire 1 Input

                        #region Rewired Left Fire 2 Input
                        // There is no need to check if it is enabled, because by default it is -1 and the StickyInputModuleEditor
                        // ensures it is -1 when not enabled. Also, it is possible to be enabled but not yet set to +ve ActionId
                        if (leftFire2InputActionIdRWD >= 0)
                        {
                            if (leftFire2CanBeHeld)
                            {
                                characterInput.leftFire2 = leftFire2InputActionTypeRWD == 1 ? rewiredPlayer.GetButton(leftFire2InputActionIdRWD) : rewiredPlayer.GetAxis(leftFire2InputActionIdRWD) > 0.01f;
                            }
                            else
                            {
                                characterInput.leftFire2 = leftFire2InputActionTypeRWD == 1 ? rewiredPlayer.GetButtonDown(leftFire2InputActionIdRWD) : !characterInput.leftFire2 && rewiredPlayer.GetAxis(leftFire2InputActionIdRWD) > 0.01f;
                            }
                        }
                        else { characterInput.leftFire2 = false; }
                        #endregion Rewired Left Fire 2 Input

                        #region Rewired Right Fire 1 Input
                        // There is no need to check if it is enabled, because by default it is -1 and the StickyInputModuleEditor
                        // ensures it is -1 when not enabled. Also, it is possible to be enabled but not yet set to +ve ActionId
                        if (rightFire1InputActionIdRWD >= 0)
                        {
                            if (rightFire1CanBeHeld)
                            {
                                characterInput.rightFire1 = rightFire1InputActionTypeRWD == 1 ? rewiredPlayer.GetButton(rightFire1InputActionIdRWD) : rewiredPlayer.GetAxis(rightFire1InputActionIdRWD) > 0.01f;
                            }
                            else
                            {
                                characterInput.rightFire1 = rightFire1InputActionTypeRWD == 1 ? rewiredPlayer.GetButtonDown(rightFire1InputActionIdRWD) : !characterInput.rightFire1 && rewiredPlayer.GetAxis(rightFire1InputActionIdRWD) > 0.01f;
                            }
                        }
                        else { characterInput.rightFire1 = false; }
                        #endregion Rewired Right Fire 1 Input

                        #region Rewired Right Fire 2 Input
                        // There is no need to check if it is enabled, because by default it is -1 and the StickyInputModuleEditor
                        // ensures it is -1 when not enabled. Also, it is possible to be enabled but not yet set to +ve ActionId
                        if (rightFire2InputActionIdRWD >= 0)
                        {
                            if (rightFire2CanBeHeld)
                            {
                                characterInput.rightFire2 = rightFire2InputActionTypeRWD == 1 ? rewiredPlayer.GetButton(rightFire2InputActionIdRWD) : rewiredPlayer.GetAxis(rightFire2InputActionIdRWD) > 0.01f;
                            }
                            else
                            {
                                characterInput.rightFire2 = rightFire2InputActionTypeRWD == 1 ? rewiredPlayer.GetButtonDown(rightFire2InputActionIdRWD) : !characterInput.rightFire2 && rewiredPlayer.GetAxis(rightFire2InputActionIdRWD) > 0.01f;
                            }
                        }
                        else { characterInput.rightFire2 = false; }
                        #endregion Rewired Right Fire 2 Input
                    }

                    #region Custom Input
                    if (numberOfCustomInputs > 0)
                    {
                        for (int ciIdx = 0; ciIdx < numberOfCustomInputs; ciIdx++)
                        {
                            CustomInput customInput = customInputList[ciIdx];

                            if (customInput != null)
                            {
                                Vector3 inputValue = Vector3.zero;

                                if (customInput.isButton)
                                {
                                    if (customInput.rwdPositiveInputActionId >= 0)
                                    {
                                        if (customInput.canBeHeldDown)
                                        {
                                            customInput.rwLastIsButtonPressed = customInput.rwdPositiveInputActionType == 1 ? rewiredPlayer.GetButton(customInput.rwdPositiveInputActionId) : rewiredPlayer.GetAxis(customInput.rwdPositiveInputActionId) > 0.01f;
                                        }
                                        else
                                        {
                                            customInput.rwLastIsButtonPressed = customInput.rwdPositiveInputActionType == 1 ? rewiredPlayer.GetButtonDown(customInput.rwdPositiveInputActionId) : !customInput.rwLastIsButtonPressed && rewiredPlayer.GetAxis(customInput.rwdPositiveInputActionId) > 0.01f;
                                        }

                                        if (customInput.rwLastIsButtonPressed)
                                        {
                                            // CustomInput.CustomInputEventType is Button (5).
                                            inputValue.x = 1f;
                                            if (customInput.customInputEvt != null) { customInput.customInputEvt.Invoke(inputValue, 5);}
                                        }
                                    }
                                }
                                else if (customInput.inputAxisMode != InputAxisMode.NoInput)
                                {
                                    if (customInput.inputAxisMode == InputAxisMode.SingleAxis)
                                    {
                                        inputValue.x = customInput.rwdPositiveInputActionType == 1 ? (rewiredPlayer.GetButton(customInput.rwdPositiveInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(customInput.rwdPositiveInputActionId);
                                        if (customInput.isSensitivityEnabled)
                                        {
                                            inputValue.x = CalculateAxisInput(customInput.lastInputValueX, inputValue.x, customInput.sensitivity, customInput.gravity);
                                        }
                                        customInput.lastInputValueX = inputValue.x;
                                        // CustomInput.CustomInputEventType is Axis1D (1).
                                        if (customInput.customInputEvt != null) { customInput.customInputEvt.Invoke(inputValue, 1);}
                                    }
                                    else // Combined
                                    {
                                        inputValue.x = (customInput.rwdPositiveInputActionType == 1 ? (rewiredPlayer.GetButton(customInput.rwdPositiveInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(customInput.rwdPositiveInputActionId)) -
                                                       (customInput.rwdNegativeInputActionType == 1 ? (rewiredPlayer.GetButton(customInput.rwdNegativeInputActionId) ? 1f : 0f) : rewiredPlayer.GetAxis(customInput.rwdNegativeInputActionId));
                                        if (customInput.isSensitivityEnabled)
                                        {
                                            inputValue.x = CalculateAxisInput(customInput.lastInputValueX, inputValue.x, customInput.sensitivity, customInput.gravity);
                                        }
                                        customInput.lastInputValueX = inputValue.x;
                                        // CustomInput.CustomInputEventType is Axis1D (1).
                                        if (customInput.customInputEvt != null) { customInput.customInputEvt.Invoke(inputValue, 1);}
                                    }
                                }
                                
                                // Send anim action values to the stickyControlModule when non-zero or not isButton.
                                if (customInput.numCustomAnimActions > 0 && (inputValue != Vector3.zero || !customInput.isButton)) { SetAninActionValues(customInput, inputValue); }
                            }
                        }
                    }
                    #endregion Custom Input

                }
                else
                {
                    // Do nothing
                    characterInput.horizontalMove = 0f;
                    characterInput.verticalMove = 0f;
                    characterInput.jump = false;
                    characterInput.crouch = false;
                    characterInput.crouchIsToggled = crouchIsToggled;
                    characterInput.sprint = false;
                    characterInput.jetpack = false;
                    characterInput.switchLook = false;
                    characterInput.horizontalLook = 0f;
                    characterInput.verticalLook = 0f;
                    characterInput.xrLook = Quaternion.identity;
                    characterInput.zoomLook = 0f;
                    characterInput.orbitLook = 0f;
                    characterInput.leftFire1 = false;
                    characterInput.leftFire2 = false;
                    characterInput.rightFire1 = false;
                    characterInput.rightFire2 = false;
                }
                #else
                // Do nothing if rewired is set but not installed in the project
                characterInput.horizontalMove = 0f;
                characterInput.verticalMove = 0f;
                characterInput.jump = false;
                characterInput.crouch = false;
                characterInput.crouchIsToggled = crouchIsToggled;
                characterInput.sprint = false;
                characterInput.jetpack = false;
                characterInput.switchLook = false;
                characterInput.horizontalLook = 0f;
                characterInput.verticalLook = 0f;
                characterInput.xrLook = Quaternion.identity;
                characterInput.zoomLook = 0f;
                characterInput.orbitLook = 0f;
                characterInput.leftFire1 = false;
                characterInput.leftFire2 = false;
                characterInput.rightFire1 = false;
                characterInput.rightFire2 = false;
                #endif
                #endregion Rewired
            }
            else if (inputMode == InputMode.UnityXR)
            {
                #region UnityXR

                #if SCSM_XR && SSC_UIS

                // By default actions are enabled
                // InputAction needs to consider the ControlType returned.
                if (isInputEnabled)
                {
                    isJetPackEnabled = stickyControlModule.IsJetPackEnabled;

                    #region XR HMD Position Input
                    // XR Head Mounted Device position
                    characterInputXR.hmdPosition = UISReadActionInputVector3(hmdPosInputActionXR, hmdPosCtrlTypeHashXR);
                    #endregion

                    #region XR Left Hand Grip Input

                    // Thumb trigger on Oculus Quest 2
                    if (leftHandInputDeviceXR.isValid && leftHandInputDeviceXR.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out float lhGripValue))
                    {
                        characterInputXR.leftHandGrip = lhGripValue;
                    }

                    #endregion

                    #region XR Left Hand Position Input
                    // XR Hand position is returned in local space
                    characterInputXR.leftHandPosition = UISReadActionInputVector3(leftHandPosInputActionXR, leftHandPosCtrlTypeHashXR);
                    #endregion

                    #region XR Left Hand Rotation Input
                    // XR Hand rotation is returned in local space
                    characterInputXR.leftHandRotation = UISReadActionInputQuaternion(leftHandRotInputActionXR, leftHandRotCtrlTypeHashXR);
                    #endregion

                    #region XR Left Hand Trigger Input

                    // Index finger trigger on Oculus Quest 2
                    if (leftHandInputDeviceXR.isValid && leftHandInputDeviceXR.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float lhTriggerValue))
                    {
                        characterInputXR.leftHandTrigger = lhTriggerValue;
                    }

                    #endregion

                    #region XR Right Hand Grip Input

                    // Thumb trigger on Oculus Quest 2
                    if (rightHandInputDeviceXR.isValid && rightHandInputDeviceXR.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out float rhGripValue))
                    {
                        characterInputXR.rightHandGrip = rhGripValue;
                    }

                    #endregion

                    #region XR Right Hand Position Input
                    // XR Hand position is returned in local space
                    characterInputXR.rightHandPosition = UISReadActionInputVector3(rightHandPosInputActionXR, rightHandPosCtrlTypeHashXR);
                    #endregion

                    #region XR Right Hand Rotation Input
                    // XR Hand rotation is returned in local space
                    characterInputXR.rightHandRotation = UISReadActionInputQuaternion(rightHandRotInputActionXR, rightHandRotCtrlTypeHashXR);
                    #endregion

                    #region XR Right Hand Trigger Input

                    // Index finger trigger on Oculus Quest 2
                    if (rightHandInputDeviceXR.isValid && rightHandInputDeviceXR.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float rhTriggerValue))
                    {
                        characterInputXR.rightHandTrigger = rhTriggerValue;
                    }

                    #endregion

                    #region XR Horizontal Move Input
                    characterInput.horizontalMove = UISReadActionInputFloat(posHztlMoveInputActionUIS, posHztlMoveInputActionDataSlotUIS, posHztlMoveCtrlTypeHashUIS);
                    #endregion

                    #region XR Vertical Move Input
                    characterInput.verticalMove = UISReadActionInputFloat(posVertMoveInputActionUIS, posVertMoveInputActionDataSlotUIS, posVertMoveCtrlTypeHashUIS);
                    #endregion

                    #region XR Sprint Input
                    characterInput.sprint = UISReadActionInputBool(sprintInputActionUIS, sprintInputActionDataSlotUIS, sprintCtrlTypeHashUIS, true);
                    #endregion

                    #region XR Jump Input
                    characterInput.jump = UISReadActionInputBool(jumpInputActionUIS, jumpInputActionDataSlotUIS, jumpCtrlTypeHashUIS, jumpCanBeHeld);
                    #endregion

                    #region XR Crouch Input
                    if (!crouchIsJetPackOnly || isJetPackEnabled)
                    {
                        characterInput.crouchIsToggled = crouchIsToggled;
                        characterInput.crouch = UISReadActionInputBool(crouchInputActionUIS, crouchInputActionDataSlotUIS, crouchCtrlTypeHashUIS, crouchCanBeHeld && (!crouchIsToggled || isJetPackEnabled));
                    }
                    else { characterInput.crouch = false; }
                    #endregion

                    #region XR Jet Pack Input
                    characterInput.jetpack = UISReadActionInputBool(jetpackInputActionUIS, jetpackInputActionDataSlotUIS, jetpackCtrlTypeHashUIS, false);
                    #endregion

                    #region XR Look HMD Input
                    // XR uses the Head Mounted Device to return rotation
                    characterInput.xrLook = UISReadActionInputQuaternion(posVertLookInputActionUIS, posVertLookCtrlTypeHashUIS);
                    #endregion

                    #region XR Horizontal Look (turn) Input
                    characterInput.horizontalLook = UISReadActionInputFloat(posHztlLookInputActionUIS, posHztlLookInputActionDataSlotUIS, posHztlLookCtrlTypeHashUIS);
                    #endregion XR Horizontal Look (turn) Input

                    #region XR Switch Look Input
                    characterInput.switchLook = UISReadActionInputBool(switchLookInputActionUIS, switchLookInputActionDataSlotUIS, switchLookCtrlTypeHashUIS, false);
                    #endregion

                    #region XR Zoom Look Input
                    characterInput.zoomLook = UISReadActionInputFloat(posZoomLookInputActionUIS, posZoomLookInputActionDataSlotUIS, posZoomLookCtrlTypeHashUIS);
                    #endregion

                    #region XR Orbit Look Input (not used in XR)
                    characterInput.orbitLook = 0f;
                    //characterInput.orbitLook = UISReadActionInputFloat(posOrbitLookInputActionUIS, posOrbitLookInputActionDataSlotUIS, posOrbitLookCtrlTypeHashUIS);
                    #endregion
            
                    #region XR Left Fire 1 Input
                    characterInput.leftFire1 = UISReadActionInputBool(leftFire1InputActionUIS, leftFire1InputActionDataSlotUIS, leftFire1CtrlTypeHashUIS, leftFire1CanBeHeld);
                    #endregion

                    #region XR Left Fire 2 Input
                    characterInput.leftFire2 = UISReadActionInputBool(leftFire2InputActionUIS, leftFire2InputActionDataSlotUIS, leftFire2CtrlTypeHashUIS, leftFire2CanBeHeld);
                    #endregion

                    #region XR Right Fire 1 Input
                    characterInput.rightFire1 = UISReadActionInputBool(rightFire1InputActionUIS, rightFire1InputActionDataSlotUIS, rightFire1CtrlTypeHashUIS, rightFire1CanBeHeld);
                    #endregion

                    #region XR Right Fire 2 Input
                    characterInput.rightFire2 = UISReadActionInputBool(rightFire2InputActionUIS, rightFire2InputActionDataSlotUIS, rightFire2CtrlTypeHashUIS, rightFire2CanBeHeld);
                    #endregion

                    if (stickyControlModule != null) { stickyControlModule.SendInputXR(characterInputXR); }
                }

                #region XR Custom Input
                if (numberOfCustomInputs > 0)
                {
                    for (int ciIdx = 0; ciIdx < numberOfCustomInputs; ciIdx++)
                    {
                        CustomInput customInput = customInputList[ciIdx];

                        if (customInput != null)
                        {
                            Vector3 inputValue = Vector3.zero;

                            if (customInput.isButton)
                            {
                                if (UISReadActionInputBool(customInput.uisPositiveInputAction, customInput.uisPositiveInputActionDataSlot, customInput.uisPositiveCtrlTypeHash, customInput.canBeHeldDown))
                                {
                                    // CustomInput.CustomInputEventType is Button (5).
                                    inputValue.x = 1f;
                                    if (customInput.customInputEvt != null) { customInput.customInputEvt.Invoke(inputValue, 5); }
                                }
                            }
                            else
                            {
                                if (customInput.uisPositiveInputAction != null)
                                {
                                    if (customInput.uisPositiveCtrlTypeHash == controlTypeAxisHashUIS || customInput.uisPositiveCtrlTypeHash == controlTypeButtonHashUIS)
                                    {
                                        // 1D axis and Buttons both retrun a float
                                        inputValue.x = customInput.uisPositiveInputAction.ReadValue<float>();

                                        if (customInput.isSensitivityEnabled)
                                        {
                                            inputValue.x = CalculateAxisInput(customInput.lastInputValueX, inputValue.x, customInput.sensitivity, customInput.gravity);
                                        }
                                        customInput.lastInputValueX = inputValue.x;
                                        // CustomInput.CustomInputEventType is Axis1D (1).
                                        if (customInput.customInputEvt != null) { customInput.customInputEvt.Invoke(inputValue, 1); }
                                    }
                                    else if (customInput.uisPositiveCtrlTypeHash == controlTypeVector2HashUIS || customInput.uisPositiveCtrlTypeHash == controlTypeDpadHashUIS)
                                    {
                                        inputValue = customInput.uisPositiveInputAction.ReadValue<Vector2>();

                                        if (customInput.isSensitivityEnabled)
                                        {
                                            inputValue.x = CalculateAxisInput(customInput.lastInputValueX, inputValue.x, customInput.sensitivity, customInput.gravity);
                                            inputValue.y = CalculateAxisInput(customInput.lastInputValueY, inputValue.y, customInput.sensitivity, customInput.gravity);
                                        }
                                        customInput.lastInputValueX = inputValue.x;
                                        customInput.lastInputValueY = inputValue.y;
                                        // CustomInput.CustomInputEventType is Axis2D (2).
                                        if (customInput.customInputEvt != null) { customInput.customInputEvt.Invoke(inputValue, 2); }
                                    }
                                    else if (customInput.uisPositiveCtrlTypeHash == controlTypeVector3HashUIS)
                                    {
                                        inputValue = customInput.uisPositiveInputAction.ReadValue<Vector3>();

                                        if (customInput.isSensitivityEnabled)
                                        {
                                            // Currently only works on x,y axes
                                            inputValue.x = CalculateAxisInput(customInput.lastInputValueX, inputValue.x, customInput.sensitivity, customInput.gravity);
                                            inputValue.y = CalculateAxisInput(customInput.lastInputValueY, inputValue.y, customInput.sensitivity, customInput.gravity);
                                        }
                                        customInput.lastInputValueX = inputValue.x;
                                        customInput.lastInputValueY = inputValue.y;
                                        // CustomInput.CustomInputEventType is Axis3D (3).
                                        if (customInput.customInputEvt != null) { customInput.customInputEvt.Invoke(inputValue, 3); }
                                    }
                                }
                            }

                            // Send anim action values to the stickyControlModule when non-zero or not a button.
                            if (customInput.numCustomAnimActions > 0 && (inputValue != Vector3.zero || !customInput.isButton)) { SetAninActionValues(customInput, inputValue); }
                        }
                    }
                }
                #endregion

                #else
                // Do nothing if XR or Unity Input System is set but not installed in the project
                characterInput.horizontalMove = 0f;
                characterInput.verticalMove = 0f;
                characterInput.jump = false;
                characterInput.crouch = false;
                characterInput.crouchIsToggled = crouchIsToggled;
                characterInput.sprint = false;
                characterInput.jetpack = false;
                characterInput.switchLook = false;
                characterInput.horizontalLook = 0f;
                characterInput.verticalLook = 0f;
                characterInput.xrLook = Quaternion.identity;
                characterInput.zoomLook = 0f;
                characterInput.orbitLook = 0f;
                characterInput.leftFire1 = false;
                characterInput.leftFire2 = false;
                characterInput.rightFire1 = false;
                characterInput.rightFire2 = false;

                //characterInputXR.leftHandGrip = 0f;
                //characterInputXR.leftHandPosition = Vector3.zero;
                //characterInputXR.leftHandRotation = Quaternion.identity;
                //characterInputXR.leftHandTrigger = 0f;
                //characterInputXR.rightHandGrip = 0f;
                //characterInputXR.rightHandPosition = Vector3.zero;
                //characterInputXR.rightHandRotation = Quaternion.identity;
                //characterInputXR.rightHandTrigger = 0f;
                #endif

                #endregion UnityXR
            }
            #if UNITY_EDITOR
            else { Debug.Log("Invalid input mode: " + inputMode.ToString()); }
            #endif

            // cater for situation where input is disabled but CustomInput is allowed.
            if (!isInputEnabled) { return; }

            if (stickyControlModule != null) { stickyControlModule.SendInput(characterInput); }
        }

        private void Update()
        {
            StickyInputUpdate(false);
        }

        #if SCSM_XR && SSC_UIS
        // Currently late update is only used for XR
        private void LateUpdate()
        {
            if (isInputEnabled && inputMode == InputMode.UnityXR)
            {
                // Test v1.1.0 Beta 16a
                StickyInputUpdate(true);
                MoveXRHands();
            }
        }
        #endif

        #endregion

        #region Events

        private void OnEnable()
        {
            #if SCSM_XR && SSC_UIS
            if (inputMode == InputMode.UnityXR)
            {
                EnableOrDisableActions(inputActionAssetXR, true);

                // XR Devices may not get connected in the first few frames when the scene loads,
                // so get notified when XR devices get connected.
                InputDevices.deviceConnected -= XRDeviceConnected;
                InputDevices.deviceConnected += XRDeviceConnected;

            }
            #endif
        }

        private void OnDisable()
        {
            #if SCSM_XR && SSC_UIS
            if (inputMode == InputMode.UnityXR)
            {
                EnableOrDisableActions(inputActionAssetXR, false);

                InputDevices.deviceConnected -= XRDeviceConnected;
            }
            #endif
        }

        /// <summary>
        /// Clean up custom input listeners on destroy
        /// </summary>
        private void OnDestroy()
        {
            #if SCSM_XR && SSC_UIS
            if (inputMode == InputMode.UnityXR)
            {
                EnableOrDisableActions(inputActionAssetXR, false);

                InputDevices.deviceConnected -= XRDeviceConnected;

                if (xrInputSubsystemList != null)
                {
                    foreach (XRInputSubsystem xrInputSubsystem in xrInputSubsystemList)
                    {
                        if (xrInputSubsystem != null)
                        {
                            xrInputSubsystem.trackingOriginUpdated -= XRTrackingOriginChanged;
                        }
                    }
                }
            }
            #endif

            if (numberOfCustomInputs > 0)
            {
                for (int ciIdx = 0; ciIdx < numberOfCustomInputs; ciIdx++)
                {
                    CustomInput customInput = customInputList[ciIdx];
                    if (customInput != null && customInput.customInputEvt != null)
                    {
                        customInput.customInputEvt.RemoveAllListeners();
                    }
                }
            }
        }

        #endregion

        #region Internal and Private Methods - General

        /// <summary>
        /// Calculates a mouse X input for an axis, taking into account sensitivity and deadzone settings.
        /// </summary>
        /// <param name="mouseX"></param>
        /// <param name="mouseXSensitivity"></param>
        /// <param name="mouseXDeadzone"></param>
        /// <param name="useDistanceFromCentre"></param>
        /// <returns></returns>
        private float CalculateMouseXInput (float mouseX, float mouseXSensitivity, float mouseXDeadzone, bool useDistanceFromCentre)
        {
            // Mouse position can be outside gameview

            // FUTURE - consider multiple monitors. See SSC PlayerInputModule.
            //#if !UNITY_EDITOR
            //mouseX -= targetDisplayOffsetX;
            //#endif

            if (useDistanceFromCentre)
            {
                // Make sure to clamp it to be inside bounds of the screen
                if (mouseX < 0f) { mouseX = 0f; }
                else if (mouseX > Screen.width) { mouseX = Screen.width; }
                // Normalise between -1.0 and +1.0
                mouseX = ((mouseX - (Screen.width / 2f)) * 2f / Screen.width);
            }

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
        /// <param name="useDistanceFromCentre"></param>
        /// <returns></returns>
        private float CalculateMouseYInput(float mouseY, float mouseYSensitivity, float mouseYDeadzone, bool useDistanceFromCentre)
        {
            if (useDistanceFromCentre)
            {
                // Mouse position can be outside gameview
                // Make sure to clamp it to be inside bounds of the screen
                if (mouseY < 0f) { mouseY = 0f; }
                else if (mouseY > Screen.height) { mouseY = Screen.height; }
                // Normalise between -1.0 and +1.0
                mouseY = -((mouseY - (Screen.height / 2f)) * 2f / Screen.height);
            }

            // Make any values inside the deadzone zero
            if (mouseY > -mouseYDeadzone * 2f && mouseY < mouseYDeadzone * 2f) { mouseY = 0; }

            // Multiply by sensitivity
            return mouseY * mouseYSensitivity;
        }

        /// <summary>
        /// Use a S3DRamp for the mouse scroll wheel for DirectKeyboard, LegacyUnity or Rewired.
        /// Could tweak the rampDownDuration to make scrolling faster (bigger duration) or slower.
        /// </summary>
        public void InitialiseScrollWheel()
        {
            if (inputMode == InputMode.DirectKeyboard || inputMode == InputMode.LegacyUnity || inputMode == InputMode.Rewired)
            {
                if (scrollWheelRamp == null)
                {
                    scrollWheelRamp = new S3DRamp()
                    {
                        rampUpDuration = 0f,
                        rampDownDuration = 0.7f
                    };
                }
            }
        }

        /// <summary>
        /// Send any input to custom S3dAnimActions on the StickyControlModule. A CustomInput
        /// can be linked to multiple S3dAnimActions on the Animate tab.
        /// Checks any conditions on the S3DAnimAction before sending.
        /// NOTE: Doesn't retest for customInput being non-null.
        /// Assumes stickyControlModule is never null when this runs.
        /// </summary>
        /// <param name="customInput"></param>
        /// <param name="evtVector"></param>
        private void SetAninActionValues(CustomInput customInput, Vector3 evtVector)
        {
            // numCustomAnimActions is cached in ReinitialiseCustomInput()
            for (int aaIdx = 0; aaIdx < customInput.numCustomAnimActions; aaIdx++)
            {
                S3DAnimAction animAction = stickyControlModule.GetAnimAction(customInput.animActionHashList[aaIdx]);

                if (animAction != null && (int)animAction.standardAction == S3DAnimAction.StandardActionCustomInt && stickyControlModule.CheckAnimateConditions(animAction))
                {
                    int parameterTypeInt = (int)animAction.parameterType;
                    if (parameterTypeInt == S3DAnimAction.ParameterTypeBoolInt)
                    {
                        // NOTE: Button input only set true values.
                        //Debug.Log("[DEBUG] Custom bool set to " + (evtVector.x != 0f) + " T:" + Time.time);
                        stickyControlModule.SetCustomAnimActionBoolValue(animAction, evtVector.x == 0f ? false : true);
                    }
                    else if (parameterTypeInt == S3DAnimAction.ParameterTypeFloatInt)
                    {
                        stickyControlModule.SetCustomAnimActionFloatValue(animAction, evtVector.x);
                    }
                    else if (parameterTypeInt == S3DAnimAction.ParameterTypeTriggerInt)
                    {
                        // Only send when the trigger is set. This avoids the problem of it being set back to false in an
                        // Update() frame before FixedUpdate() runs on StickyControlModule.
                        if (evtVector.x != 0f) { stickyControlModule.SetCustomAnimActionTriggerValue(animAction, true); }

                        //stickyControlModule.SetCustomAnimActionTriggerValue(animAction, evtVector.x == 0f ? false : true);
                    }
                    else if (parameterTypeInt == S3DAnimAction.ParameterTypeIntegerInt)
                    {
                        stickyControlModule.SetCustomAnimActionIntegerValue(animAction, (int)evtVector.x);
                    }
                }
            }
        }

        #endregion

        #region Internal and Private Methods - Unity Input System

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
        /// in the StickyInputModuleEditor. It doesn't check the inputAxisMode.
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
        /// in the StickyInputModuleEditor. It doesn't check the inputAxisMode or
        /// [jump/sprint/crouch]ButtonEnabled values.
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

            // It doesn't check if the [jump/sprint/crouch/left or right fire 1 or 2]ButtonEnabled are enabled.

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

        /// <summary>
        /// Given a Unity Input System action, read the current value being generated by that
        /// action. This assumes the inputAction will be null if it has not been configured
        /// in the StickyInputModuleEditor. It doesn't check the inputAxisMode.
        /// </summary>
        /// <param name="inputAction"></param>
        /// <param name="actionControlTypeHash"></param>
        /// <returns></returns>
        private Quaternion UISReadActionInputQuaternion(UnityEngine.InputSystem.InputAction inputAction, int actionControlTypeHash)
        {
            // Unity Input System only requires SingleAxis as multiple can be supported by
            // using Composite Bindings. It doesn't check if the inputAxisMode is NoInput or CombinedAxis.

            if (inputAction != null && actionControlTypeHash == controlTypeQuaternionHashUIS)
            {
                return inputAction.ReadValue<Quaternion>();
            }
            else { return Quaternion.identity; }
        }

        /// <summary>
        /// Given a Unity Input System action, read the current value being generated by that
        /// action. This assumes the inputAction will be null if it has not been configured
        /// in the StickyInputModuleEditor. It doesn't check the inputAxisMode.
        /// </summary>
        /// <param name="inputAction"></param>
        /// <param name="actionControlTypeHash"></param>
        /// <returns></returns>
        private Vector3 UISReadActionInputVector3(UnityEngine.InputSystem.InputAction inputAction, int actionControlTypeHash)
        {
            // Unity Input System only requires SingleAxis as multiple can be supported by
            // using Composite Bindings. It doesn't check if the inputAxisMode is NoInput or CombinedAxis.

            if (inputAction != null && actionControlTypeHash == controlTypeVector3HashUIS)
            {
                return inputAction.ReadValue<Vector3>();
            }
            else { return Vector3.zero; }
        }

#endif

        #endregion

        #region Internal and Private Methods - Unity XR

        #if SSC_UIS && SCSM_XR

        /// <summary>
        /// When the device (HMD) is avaialable (IsValid), and it has a non-zero
        /// height above the floor, recalculate floor offset and calibrate S3D
        /// based on setting in LookVR.
        /// It can take a second or 2 to get height data from a Quest 2.
        /// </summary>
        /// <param name="inputDevice"></param>
        /// <returns></returns>
        private IEnumerator CalibrateXR(UnityEngine.XR.InputDevice inputDevice)
        {
            if (xrWaitForSeconds == null) { xrWaitForSeconds = new WaitForSeconds(0.1f); }

            do
            {
                yield return xrWaitForSeconds;

                //Debug.Log("[DEBUG] attempting to calibrate XR... T:" + Time.time);

                if (inputDevice.isValid && characterInputXR.hmdPosition.y > 0.1f && !stickyControlModule.IsHeightCalibratedVR )
                {
                    stickyControlModule.RecalculateFloorOffsetVR();
                    stickyControlModule.ReinitiliseCameraVR();
                }

            } while (!stickyControlModule.IsHeightCalibratedVR);

            //Debug.Log("[DEBUG] Calibrated XR at T:" + Time.time);
        }

        /// <summary>
        /// Attempt to initialise the XR. This may not be instant,
        /// so perform up to 30 attempts with 0.1 second intervals.
        /// </summary>
        private IEnumerator InitialiseXR()
        {
            isInitialisingXR = true;

            if (characterInputXR == null) { characterInputXR = new CharacterInputXR(); }

             if (xrWaitForSeconds == null) { xrWaitForSeconds = new WaitForSeconds(0.1f); }

            while (!isInitialisedXR && initialiseXRCount < 30)
            {
                yield return xrWaitForSeconds;
                if (!isInitialisedXR)
                {
                    isInitialisedXR = InitialiseXRSubsystems();
                    initialiseXRCount++;
                }
            }

            #if !UNITY_EDITOR
            XRRefreshDevices();
            #endif

            isInitialisingXR = false;
        }

        /// <summary>
        /// Attempt to initialise the XR system.
        /// Attempt to set the tracking origin to floor.
        /// </summary>
        private bool InitialiseXRSubsystems()
        {
            SubsystemManager.GetInstances(xrInputSubsystemList);

            int numXRInputSystems = xrInputSubsystemList == null ? 0 : xrInputSubsystemList.Count;
            bool isSuccessful = false;
            bool isRoomScale = stickyControlModule.IsLookRoomScaleVR;

            //TrackingOriginModeFlags trackingOriginModeFlags = isRoomScale ? TrackingOriginModeFlags.Device : TrackingOriginModeFlags.Floor;
            TrackingOriginModeFlags trackingOriginModeFlags = TrackingOriginModeFlags.Floor;

            for (int xrInSysIdx = 0; xrInSysIdx < numXRInputSystems; xrInSysIdx++)
            {
                XRInputSubsystem xrInputSubsystem = xrInputSubsystemList[xrInSysIdx];

                if (xrInputSubsystem != null)
                {
                    TrackingOriginModeFlags supportedOriginModes = xrInputSubsystem.GetSupportedTrackingOriginModes();

                    if (supportedOriginModes == TrackingOriginModeFlags.Unknown) { continue; }
                    else if ((supportedOriginModes & trackingOriginModeFlags) == 0)
                    {
                        Debug.LogWarning($"Tracking origin mode {trackingOriginModeFlags} is not supported. Supported types: {supportedOriginModes:F}.", this);
                    }
                    else
                    {
                        isSuccessful = xrInputSubsystem.TrySetTrackingOriginMode(trackingOriginModeFlags);

                        if (isSuccessful && isRoomScale)
                        {
                            // This should prob also be called if TrackingOriginModeFlags.Device was previously set...
                            isSuccessful = xrInputSubsystem.TryRecenter();
                        }

                        if (isSuccessful)
                        {
                            xrInputSubsystem.trackingOriginUpdated -= XRTrackingOriginChanged;
                            xrInputSubsystem.trackingOriginUpdated += XRTrackingOriginChanged;
                        }
                    }
                }
            }

            // Keep track of the initial XR Hands Offset angles in degrees. The use may have
            // adjusted the x and z rotations to fix a hand model issue.
            //if (leftHandTransformXR != null)
            //{
            //    xrHandsOffsetInitRotation = leftHandTransformXR.parent.localRotation.eulerAngles;
            //}

            return isSuccessful;
        }

        /// <summary>
        /// XR Devices may not get connected in the first few frames when the scene loads,
        /// so get notified when XR devices get connected.
        /// </summary>
        /// <param name="inputDevice"></param>
        private void XRDeviceConnected(UnityEngine.XR.InputDevice inputDevice)
        {
            if ((inputDevice.characteristics & leftHandCharacteristicsXR) == leftHandCharacteristicsXR)
            {
                //Debug.Log("[DEBUG] Left " + inputDevice.characteristics);

                leftHandInputDeviceXR = inputDevice;
                //Debug.Log("[DEBUG] Left Device connected! name: " + inputDevice.name + " T:" + Time.time);
            }
            else if ((inputDevice.characteristics & rightHandCharacteristicsXR) == rightHandCharacteristicsXR)
            {
                rightHandInputDeviceXR = inputDevice;
                //Debug.Log("[DEBUG] Right Device connected! name: " + inputDevice.name + " T:" + Time.time);
            }
            else if (inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.HeadMounted))
            {
                hmdInputDeviceXR = inputDevice;
                //Debug.Log("[DEBUG] HMD connected! name: " + inputDevice.name + " T:" + Time.time);
                StartCoroutine(CalibrateXR(inputDevice));
            }
        }

        /// <summary>
        /// Attempt to refresh or get hands and head-mounted device that is not yet valid.
        /// </summary>
        private void XRRefreshDevices()
        {
            List<InputDevice> xrInputDevices = new List<InputDevice>(1);

            if (!leftHandInputDeviceXR.isValid)
            {
                InputDevices.GetDevicesWithCharacteristics(leftHandCharacteristicsXR, xrInputDevices);
                if (xrInputDevices.Count > 0) { leftHandInputDeviceXR = xrInputDevices[0]; }
            }

            if (!rightHandInputDeviceXR.isValid)
            {
                InputDevices.GetDevicesWithCharacteristics(rightHandCharacteristicsXR, xrInputDevices);
                if (xrInputDevices.Count > 0) { rightHandInputDeviceXR = xrInputDevices[0]; }
            }

            if (!hmdInputDeviceXR.isValid)
            {
                InputDevices.GetDevicesWithCharacteristics(hmdCharacteristicsXR, xrInputDevices);
                if (xrInputDevices.Count > 0)
                {
                    hmdInputDeviceXR = xrInputDevices[0];
                    StartCoroutine(CalibrateXR(hmdInputDeviceXR));
                }
            }
        }

        private void XRTrackingOriginChanged(XRInputSubsystem xrInputSubsystem)
        {
            //if (!stickyControlModule.IsHeightCalibratedVR && characterInputXR.hmdPosition.y > 0.1f)
            //{
            //    stickyControlModule.RecalculateFloorOffsetVR();
            //}          

            //Debug.Log("[DEBUG] Tracking orgin has changed at T:" + Time.time + " Origin Mode: " + xrInputSubsystem.GetTrackingOriginMode());
        }

        /// <summary>
        /// Move the XR hand offset transforms based on the hand input.
        /// Compensate for HMD being moved in StickyControlModule.UpdateController(..)
        /// to match HMD fwd direction.
        /// </summary>
        private void MoveXRHands()
        {
            // If using the stationary VR position, the Head Mounted Device will always
            // be relative to forwards. However, with Snap Turn enabled, it may not match
            // the character forward direction.
            float hmdAngleY = -characterInput.xrLook.eulerAngles.y;

            bool isMatchHumanHeightVR = stickyControlModule.IsLookMatchHumanHeightVR;
            StickyControlModule.HumanPostureVR humanPostureVR = stickyControlModule.GetHumanPostureVR();
            Vector3 initialHMDCameraDelta = stickyControlModule.initialHMDCameraDelta;

            // When the human player moves from the original centre position of their bounding area,
            // for some reason we need to subtract the HMD x-z position from the hand positions.
            // The hands seem to be relative to the start position rather than the relative position of the HMD.
            Vector3 handFloorOffsetAdjustment = -characterInputXR.hmdPosition;
            // Based on the starting human posture we may need to move the hands up or down
            // If the avatar matches the height of the human player, then we don't need to adjust the height of the hands
            handFloorOffsetAdjustment.y = isMatchHumanHeightVR ? 0f : -initialHMDCameraDelta.y;

            // Hand position and rotation is returned in local space
            if (leftHandTransformXR != null)
            {
                // Hand parent should always face forwards
                if (stickyControlModule.IsLookSnapTurnVREnabled)
                {
                    //leftHandTransformXR.parent.localRotation = Quaternion.Euler(xrHandsOffsetInitRotation.x, 0f, xrHandsOffsetInitRotation.z);
                    leftHandTransformXR.parent.localRotation = Quaternion.identity;
                }
                else
                {
                    leftHandTransformXR.parent.localRotation = Quaternion.Euler(0f, hmdAngleY, 0f);
                }

                leftHandTransformXR.localPosition = characterInputXR.leftHandPosition + handFloorOffsetAdjustment;
                leftHandTransformXR.localRotation = characterInputXR.leftHandRotation * Quaternion.Euler(leftHandOffsetRotXR);
            }

            if (rightHandTransformXR != null)
            {
                rightHandTransformXR.localPosition = characterInputXR.rightHandPosition + handFloorOffsetAdjustment;
                rightHandTransformXR.localRotation = characterInputXR.rightHandRotation * Quaternion.Euler(rightHandOffsetRotXR);
            }
        }

        #endif

        #endregion

        #region Public Static Methods

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
                #if UNITY_EDITOR
                if (showError) { Debug.LogWarning("StickyInputModule: " + ex.Message); }
                #else
                if (ex != null) { }
                #endif
            }

            return isValid;
        }

        #endregion

        #region Public API - General

        /// <summary>
        /// This must be called on Awake or via code before the Sticky Input Module can be used.
        /// </summary>
        public void Initialise()
        {
            if (IsInitialised) { return; }

            characterInput = new CharacterInput();
            lastCharacterInput = new CharacterInput();

            stickyControlModule = GetComponent<StickyControlModule>();

            #if UNITY_EDITOR
            bool showErrors = true;
            #else
            bool showErrors = false;
            #endif

            if (inputMode == InputMode.UnityInputSystem)
            {
                #if SSC_UIS
                UpdateUnityInputSystemActions(showErrors);
                #else
                if (showErrors) { }
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
                }
                #else
                #if UNITY_EDITOR
                Debug.LogWarning("StickyInputModule.Awake - the InputMode is set to Rewired on " + gameObject.name + ", however Rewired does not seem to be installed in this project.");
                #endif                
                #endif
            }
            else if (inputMode == InputMode.UnityXR)
            {
                #if SCSM_XR && SSC_UIS
                UpdateUnityXRActions(showErrors);
                EnableOrDisableActions(inputActionAssetXR, true);
                if (Application.isPlaying) { StartCoroutine(InitialiseXR()); }
                #endif
            }
            else if (inputMode == InputMode.LegacyUnity)
            {
                ValidateLegacyInput();
            }
            else if (inputMode == InputMode.DirectKeyboard)
            {
                ValidateDirectKeyboardInput();
            }

            ReinitialiseCustomInput();

            InitialiseScrollWheel();

            isInputEnabled = isEnabledOnInitialise;
            IsInitialised = true;
        }

        /// <summary>
        /// Retrieves a reference to the StickyControlModule script if one was attached at the time this
        /// module was initiated.
        /// </summary>
        /// <param name="forceCheck">Ignore cached value and call GetComponent when true</param>
        /// <returns></returns>
        public StickyControlModule GetStickyControlModule (bool forceCheck = false)
        {
            if (forceCheck) { stickyControlModule = GetComponent<StickyControlModule>(); }

            return stickyControlModule;
        }

        /// <summary>
        /// Enable the Sticky Input Module to receive input from the configured device.
        /// The module will be initialised if it isn't already.
        /// </summary>
        public void EnableInput()
        {
            if (!IsInitialised) { Initialise(); }

            if (IsInitialised)
            {
                isInputEnabled = true;
                isCustomInputsOnlyEnabled = false;
            }
        }

        /// <summary>
        /// Disable input or stop the Sticky Input Module from receiving input from the configured device.
        /// When allowCustomInput is true, all other input except CustomInputs are ignored.
        /// This can be useful when you want to still receive actions that generally don't involve character movement.
        /// </summary>
        /// <param name="allowCustomInput"></param>
        public void DisableInput(bool allowCustomInput = false)
        {
            if (IsInitialised)
            {
                ResetInput();
                isInputEnabled = false;
                isCustomInputsOnlyEnabled = allowCustomInput;
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: StickyInputModule.DisableInput() was called before Awake() or Initialise() ran. DisableInput will have no effect."); }
            #endif
        }

        /// <summary>
        /// Reset and send 0 input on each axis to the controller
        /// </summary>
        public void ResetInput()
        {
            if (IsInitialised)
            {
                characterInput.horizontalMove = 0f;
                characterInput.verticalMove = 0f;
                characterInput.jump = false;
                characterInput.crouch = false;
                characterInput.sprint = false;
                characterInput.jetpack = false;
                characterInput.switchLook = false;
                characterInput.horizontalLook = 0f;
                characterInput.verticalLook = 0f;
                characterInput.xrLook = Quaternion.identity;
                characterInput.zoomLook = 0f;
                characterInput.orbitLook = 0f;
                characterInput.leftFire1 = false;
                characterInput.leftFire2 = false;
                characterInput.rightFire1 = false;
                characterInput.rightFire2 = false;

                lastCharacterInput.horizontalMove = 0f;
                lastCharacterInput.verticalMove = 0f;
                lastCharacterInput.jump = false;
                lastCharacterInput.crouch = false;
                lastCharacterInput.sprint = false;
                lastCharacterInput.jetpack = false;
                lastCharacterInput.switchLook = false;
                lastCharacterInput.horizontalLook = 0f;
                lastCharacterInput.verticalLook = 0f;
                lastCharacterInput.xrLook = Quaternion.identity;
                lastCharacterInput.zoomLook = 0f;
                lastCharacterInput.orbitLook = 0f;
                lastCharacterInput.leftFire1 = false;
                lastCharacterInput.leftFire2 = false;
                lastCharacterInput.rightFire1 = false;
                lastCharacterInput.rightFire2 = false;

                if (scrollWheelRamp != null) { scrollWheelRamp.Reset(); }

                if (stickyControlModule != null)
                {
                    stickyControlModule.SendInput(characterInput);
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: StickyInputModule.ResetInput() was called before Awake() or Initialise() ran. ResetInput will have no effect."); }
            #endif
        }

        /// <summary>
        /// This should be called if you modify the CustomInputs at runtime
        /// </summary>
        public void ReinitialiseCustomInput()
        {
            numberOfCustomInputs = customInputList == null ? 0 : customInputList.Count;

            if (numberOfCustomInputs > 0)
            {
                for (int ciIdx = 0; ciIdx < numberOfCustomInputs; ciIdx++)
                {
                    CustomInput customInput = customInputList[ciIdx];
                    if (customInput != null)
                    {
                        // Validate legacy input axis
                        if (inputMode == InputMode.LegacyUnity)
                        {
                            customInput.lisIsPositiveAxisValid = IsLegacyAxisValid(customInput.lisPositiveAxisName, false);
                            customInput.lisIsNegativeAxisValid = IsLegacyAxisValid(customInput.lisNegativeAxisName, false);
                        }

                        // Cache the number of custom animation actions for this CustomInput
                        customInput.numCustomAnimActions = customInput.animActionHashList == null ? 0 : customInput.animActionHashList.Count;
                    }
                }
            }
        }

        /// <summary>
        /// This hasn't had much testing and is generally NOT recommended. If you think you need it,
        /// chat with us on Discord or our Unity forum... We are basically using it to avoid errors
        /// in the editor when changing the Input Mode.
        /// </summary>
        public void Reinitalise()
        {
            IsInitialised = false;
            isInputEnabled = false;
            isCustomInputsOnlyEnabled = false;
            Initialise();
        }

        /// <summary>
        /// Validate all the legacy input axis names. Update their status.
        /// </summary>
        public void ValidateLegacyInput()
        {
            isPosHztlMoveInputAxisValid = IsLegacyAxisValid(posHztlMoveInputAxisName, false);
            isNegHztlMoveInputAxisValid = IsLegacyAxisValid(negHztlMoveInputAxisName, false);
            isPosVertMoveInputAxisValid = IsLegacyAxisValid(posVertMoveInputAxisName, false);
            isNegVertMoveInputAxisValid = IsLegacyAxisValid(negVertMoveInputAxisName, false);
            isJumpInputAxisValid = IsLegacyAxisValid(jumpInputAxisName, false);
            isSprintInputAxisValid = IsLegacyAxisValid(sprintInputAxisName, false);
            isCrouchInputAxisValid = IsLegacyAxisValid(crouchInputAxisName, false);
            isJetPackInputAxisValid = IsLegacyAxisValid(jetpackInputAxisName, false);
            isSwitchLookInputAxisValid = IsLegacyAxisValid(switchLookInputAxisName, false);
            isPosHztlLookInputAxisValid = IsLegacyAxisValid(posHztlLookInputAxisName, false);
            isNegHztlLookInputAxisValid = IsLegacyAxisValid(negHztlLookInputAxisName, false);
            isPosVertLookInputAxisValid = IsLegacyAxisValid(posVertLookInputAxisName, false);
            isNegVertLookInputAxisValid = IsLegacyAxisValid(negVertLookInputAxisName, false);
            isPosZoomLookInputAxisValid = IsLegacyAxisValid(posZoomLookInputAxisName, false);
            isNegZoomLookInputAxisValid = IsLegacyAxisValid(negZoomLookInputAxisName, false);
            isPosOrbitLookInputAxisValid = IsLegacyAxisValid(posOrbitLookInputAxisName, false);
            isNegOrbitLookInputAxisValid = IsLegacyAxisValid(negOrbitLookInputAxisName, false);
            isLeftFire1InputAxisValid = IsLegacyAxisValid(leftFire1InputAxisName, false);
            isLeftFire2InputAxisValid = IsLegacyAxisValid(leftFire2InputAxisName, false);
            isRightFire1InputAxisValid = IsLegacyAxisValid(rightFire1InputAxisName, false);
            isRightFire2InputAxisValid = IsLegacyAxisValid(rightFire2InputAxisName, false);
        }

        /// <summary>
        /// Validate the legacy mouse input axis we use for Mouse Look in Direct Keyboard and mouse
        /// </summary>
        public void ValidateDirectKeyboardInput()
        {
            isMouseHztlLookInputAxisValidDKI = IsLegacyAxisValid(mouseHztlLookInputAxisNameDKI, false);
            isMouseVertLookInputAxisValidDKI = IsLegacyAxisValid(mouseVertLookInputAxisNameDKI, false);
        }

        #endregion

        #region Public API Unity Input System Methods
        #if SSC_UIS

        /// <summary>
        /// Get the Unity Input System PlayerInput component attached to this (player) character.
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
            string methodName = "StickyInputModule.UpdateUnityInputSystemActions";
            #endif

            // Get the hashcodes for the Control Types that we support
            controlTypeButtonHashUIS = S3DMath.GetHashCode("Button");
            controlTypeAxisHashUIS = S3DMath.GetHashCode("Axis");
            // Dpad and Vector2 are equivalent
            controlTypeDpadHashUIS = S3DMath.GetHashCode("Dpad");
            controlTypeVector2HashUIS = S3DMath.GetHashCode("Vector2");
            controlTypeVector3HashUIS = S3DMath.GetHashCode("Vector3");
            controlTypeQuaternionHashUIS = S3DMath.GetHashCode("Quaternion");

            // Make sure PlayerInput component is available
            // The current version of S3D depends on this Unity component to be attached to the same gameobject
            if (GetUISPlayerInput() != null)
            {
                int numCPI = customInputList == null ? 0 : customInputList.Count;

                if (uisPlayerInput.actions != null)
                {
                    //UnityEngine.InputSystem.InputAction inputAction = null;

                    posHztlMoveInputActionUIS = uisPlayerInput.actions.FindAction(posHztlMoveInputActionIdUIS);
                    posVertMoveInputActionUIS = uisPlayerInput.actions.FindAction(posVertMoveInputActionIdUIS);
                    posHztlLookInputActionUIS = uisPlayerInput.actions.FindAction(posHztlLookInputActionIdUIS);
                    posVertLookInputActionUIS = uisPlayerInput.actions.FindAction(posVertLookInputActionIdUIS);
                    posZoomLookInputActionUIS = uisPlayerInput.actions.FindAction(posZoomLookInputActionIdUIS);
                    posOrbitLookInputActionUIS = uisPlayerInput.actions.FindAction(posOrbitLookInputActionIdUIS);
                    jumpInputActionUIS = uisPlayerInput.actions.FindAction(jumpInputActionIdUIS);
                    sprintInputActionUIS = uisPlayerInput.actions.FindAction(sprintInputActionIdUIS);
                    crouchInputActionUIS = uisPlayerInput.actions.FindAction(crouchInputActionIdUIS);
                    jetpackInputActionUIS = uisPlayerInput.actions.FindAction(jetpackInputActionIdUIS);
                    switchLookInputActionUIS = uisPlayerInput.actions.FindAction(switchLookInputActionIdUIS);
                    leftFire1InputActionUIS = uisPlayerInput.actions.FindAction(leftFire1InputActionIdUIS);
                    leftFire2InputActionUIS = uisPlayerInput.actions.FindAction(leftFire2InputActionIdUIS);
                    rightFire1InputActionUIS = uisPlayerInput.actions.FindAction(rightFire1InputActionIdUIS);
                    rightFire2InputActionUIS = uisPlayerInput.actions.FindAction(rightFire2InputActionIdUIS);

                    posHztlMoveCtrlTypeHashUIS = posHztlMoveInputActionUIS == null ? 0 : S3DMath.GetHashCode(posHztlMoveInputActionUIS.expectedControlType);
                    posVertMoveCtrlTypeHashUIS = posVertMoveInputActionUIS == null ? 0 : S3DMath.GetHashCode(posVertMoveInputActionUIS.expectedControlType);
                    posHztlLookCtrlTypeHashUIS = posHztlLookInputActionUIS == null ? 0 : S3DMath.GetHashCode(posHztlLookInputActionUIS.expectedControlType);
                    posVertLookCtrlTypeHashUIS = posVertLookInputActionUIS == null ? 0 : S3DMath.GetHashCode(posVertLookInputActionUIS.expectedControlType);
                    posZoomLookCtrlTypeHashUIS = posZoomLookInputActionUIS == null ? 0 : S3DMath.GetHashCode(posZoomLookInputActionUIS.expectedControlType);
                    posOrbitLookCtrlTypeHashUIS = posOrbitLookInputActionUIS == null ? 0 : S3DMath.GetHashCode(posOrbitLookInputActionUIS.expectedControlType);
                    jumpCtrlTypeHashUIS = jumpInputActionUIS == null ? 0 : S3DMath.GetHashCode(jumpInputActionUIS.expectedControlType);
                    sprintCtrlTypeHashUIS = sprintInputActionUIS == null ? 0 : S3DMath.GetHashCode(sprintInputActionUIS.expectedControlType);
                    crouchCtrlTypeHashUIS = crouchInputActionUIS == null ? 0 : S3DMath.GetHashCode(crouchInputActionUIS.expectedControlType);
                    jetpackCtrlTypeHashUIS = jetpackInputActionUIS == null ? 0 : S3DMath.GetHashCode(jetpackInputActionUIS.expectedControlType);
                    switchLookCtrlTypeHashUIS = switchLookInputActionUIS == null ? 0 : S3DMath.GetHashCode(switchLookInputActionUIS.expectedControlType);
                    leftFire1CtrlTypeHashUIS = leftFire1InputActionUIS == null ? 0 : S3DMath.GetHashCode(leftFire1InputActionUIS.expectedControlType);
                    leftFire2CtrlTypeHashUIS = leftFire2InputActionUIS == null ? 0 : S3DMath.GetHashCode(leftFire2InputActionUIS.expectedControlType);
                    rightFire1CtrlTypeHashUIS = rightFire1InputActionUIS == null ? 0 : S3DMath.GetHashCode(rightFire1InputActionUIS.expectedControlType);
                    rightFire2CtrlTypeHashUIS = rightFire2InputActionUIS == null ? 0 : S3DMath.GetHashCode(rightFire2InputActionUIS.expectedControlType);

                    // Update all the Custom Inputs
                    for (int cpiIdx = 0; cpiIdx < numCPI; cpiIdx++)
                    {
                        CustomInput customInput = customInputList[cpiIdx];
                        if (customInput != null)
                        {
                            customInput.uisPositiveInputAction = uisPlayerInput.actions.FindAction(customInput.uisPositiveInputActionId);
                            customInput.uisPositiveCtrlTypeHash = customInput.uisPositiveInputAction == null ? 0 : S3DMath.GetHashCode(customInput.uisPositiveInputAction.expectedControlType);
                            customInput.lastInputValueX = 0f;
                            customInput.lastInputValueY = 0f;
                        }
                    }
                }
                else
                {
                    // set all actions to null
                    posHztlMoveInputActionUIS = null;
                    posVertMoveInputActionUIS = null;
                    posHztlLookInputActionUIS = null;
                    posVertLookInputActionUIS = null;
                    posZoomLookInputActionUIS = null;
                    posOrbitLookInputActionUIS = null;
                    jumpInputActionUIS = null;
                    sprintInputActionUIS = null;
                    crouchInputActionUIS = null;
                    jetpackInputActionUIS = null;
                    switchLookInputActionUIS = null;
                    leftFire1InputActionUIS = null;
                    leftFire2InputActionUIS = null;
                    rightFire1InputActionUIS = null;
                    rightFire2InputActionUIS = null;

                    posHztlMoveCtrlTypeHashUIS = 0;
                    posVertMoveCtrlTypeHashUIS = 0;
                    posHztlLookCtrlTypeHashUIS = 0;
                    posVertLookCtrlTypeHashUIS = 0;
                    posZoomLookCtrlTypeHashUIS = 0;
                    posOrbitLookCtrlTypeHashUIS = 0;
                    jumpCtrlTypeHashUIS = 0;
                    sprintCtrlTypeHashUIS = 0;
                    crouchCtrlTypeHashUIS = 0;
                    jetpackCtrlTypeHashUIS = 0;
                    switchLookCtrlTypeHashUIS = 0;
                    leftFire1CtrlTypeHashUIS = 0;
                    leftFire2CtrlTypeHashUIS = 0;
                    rightFire1CtrlTypeHashUIS = 0;
                    rightFire2CtrlTypeHashUIS = 0;

                    // Update all the Custom Player Inputs
                    for (int cpiIdx = 0; cpiIdx < numCPI; cpiIdx++)
                    {
                        CustomInput customInput = customInputList[cpiIdx];
                        if (customInput != null)
                        {
                            customInput.uisPositiveInputAction = null;
                            customInput.uisPositiveCtrlTypeHash = 0;
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

        #region Public API Methods - Rewired static

        #if SSC_REWIRED
        /// <summary>
        /// Verify that Rewired's Input Manager is in the scene and has been initialised.
        /// </summary>
        /// <param name="showErrors"></param>
        /// <returns></returns>
        public static bool CheckRewired (bool showErrors)
        {
            bool isInitialised = false;

            try
            {
                isInitialised = Rewired.ReInput.isReady;
            }
            catch (System.Exception ex)
            {
                #if UNITY_EDITOR
                if (showErrors) { Debug.LogWarning("StickyInputModule.CheckRewired - Rewired Input Manager is not initialised.\n" + ex.Message); }
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
        /// Rewired.Player rewiredPlayer = StickyInputModule.GetRewiredPlayer(1, false);
        /// </summary>
        /// <param name="userNumber"></param>
        /// <param name="showErrors"></param>
        /// <returns></returns>
        public static Rewired.Player GetRewiredPlayer (int userNumber, bool showErrors)
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
                            Debug.LogWarning("ERROR: StickyInputModule.GetRewiredPlayer - there does not seem to be any Players setup in Rewired. Check the Players tab in the Rewired Editor.");
                        }
                        else
                        {
                            Debug.LogWarning("ERROR: StickyInputModule.GetRewiredPlayer - invalid UserNumber " + userNumber);
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
        #endregion

        #region Public API Methods - Rewired
        /// <summary>
        /// Set the human player number. This should be 1 or greater. The player
        /// must be first assigned in Rewired, before this is called.
        /// NOTE: If Rewired is not installed or the InputMode is not Rewired,
        /// the rewiredPlayerNumber is set to 0 (unassigned).
        /// </summary>
        /// <param name="playerNumber"></param>
        /// <param name="showErrors"></param>
        public void SetRewiredPlayer (int playerNumber, bool showErrors = false)
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
        /// called once at runtime for this character, and then whenever the Actions are
        /// changed. Has no effect if Rewired is not installed.
        /// </summary>
        public void UpdateRewiredActionTypes (bool showErrors)
        {
            #if SSC_REWIRED
            if (CheckRewired(showErrors))
            {
                Rewired.InputAction inputAction = null;
                inputAction = Rewired.ReInput.mapping.GetAction(posHztlMoveInputActionIdRWD);
                if (inputAction != null) { posHztlMoveInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(negHztlMoveInputActionIdRWD);
                if (inputAction != null) { negHztlMoveInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(posVertMoveInputActionIdRWD);
                if (inputAction != null) { posVertMoveInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(negVertMoveInputActionIdRWD);
                if (inputAction != null) { negVertMoveInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(posHztlLookInputActionIdRWD);
                if (inputAction != null) { posHztlLookInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }
                
                inputAction = Rewired.ReInput.mapping.GetAction(negHztlLookInputActionIdRWD);
                if (inputAction != null) { negHztlLookInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(posVertLookInputActionIdRWD);
                if (inputAction != null) { posVertLookInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(negVertLookInputActionIdRWD);
                if (inputAction != null) { negVertLookInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(posZoomLookInputActionIdRWD);
                if (inputAction != null) { posZoomLookInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(negZoomLookInputActionIdRWD);
                if (inputAction != null) { negZoomLookInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(posOrbitLookInputActionIdRWD);
                if (inputAction != null) { posOrbitLookInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(negOrbitLookInputActionIdRWD);
                if (inputAction != null) { negOrbitLookInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(jumpInputActionIdRWD);
                if (inputAction != null) { jumpInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(sprintInputActionIdRWD);
                if (inputAction != null) { sprintInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(crouchInputActionIdRWD);
                if (inputAction != null) { crouchInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(jetpackInputActionIdRWD);
                if (inputAction != null) { jetpackInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(switchLookInputActionIdRWD);
                if (inputAction != null) { switchLookInputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(leftFire1InputActionIdRWD);
                if (inputAction != null) { leftFire1InputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(leftFire2InputActionIdRWD);
                if (inputAction != null) { leftFire2InputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(rightFire1InputActionIdRWD);
                if (inputAction != null) { rightFire1InputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                inputAction = Rewired.ReInput.mapping.GetAction(rightFire2InputActionIdRWD);
                if (inputAction != null) { rightFire2InputActionTypeRWD = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }

                int numCI = customInputList == null ? 0 : customInputList.Count;

                for (int ciIdx = 0; ciIdx < numCI; ciIdx++)
                {
                    CustomInput customInput = customInputList[ciIdx];
                    if (customInput != null)
                    {
                        inputAction = Rewired.ReInput.mapping.GetAction(customInput.rwdPositiveInputActionId);
                        if (inputAction != null) { customInput.rwdPositiveInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }
                        else { customInput.rwdPositiveInputActionType = 0; }

                        customInput.lastInputValueX = 0f;
                        customInput.lastInputValueY = 0f;
                        customInput.rwLastIsButtonPressed = false;

                        inputAction = Rewired.ReInput.mapping.GetAction(customInput.rwdNegativeInputActionId);
                        if (inputAction != null) { customInput.rwdNegativeInputActionType = inputAction.type == Rewired.InputActionType.Button ? 1 : 0; }
                        else { customInput.rwdNegativeInputActionType = 0; }
                    }
                }
            }
            #endif
        }


        #endregion

        #region Public API Methods - Unity XR

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
        /// Used at runtime to convert string unique identifiers for actions (GUIDs) into
        /// Unity Input System ActionInputs to avoid looking them up and incurring GC overhead each Update.
        /// If actions are modified at runtime, call this method.
        /// Has no effect if Unity Input System package is not installed.
        /// </summary>
        /// <param name="showErrors"></param>
        public void UpdateUnityXRActions(bool showErrors = false)
        {
            #if UNITY_EDITOR
            string methodName = "StickyInputModule.UpdateUnityXRActions";
            #endif

            // Get the hashcodes for the Control Types that we support
            controlTypeButtonHashUIS = S3DMath.GetHashCode("Button");
            controlTypeAxisHashUIS = S3DMath.GetHashCode("Axis");
            // Dpad and Vector2 are equivalent
            controlTypeDpadHashUIS = S3DMath.GetHashCode("Dpad");
            controlTypeVector2HashUIS = S3DMath.GetHashCode("Vector2");
            controlTypeVector3HashUIS = S3DMath.GetHashCode("Vector3");
            controlTypeQuaternionHashUIS = S3DMath.GetHashCode("Quaternion");

            if (inputActionAssetXR != null)
            {
                int numCPI = customInputList == null ? 0 : customInputList.Count;

                //UnityEngine.InputSystem.InputAction inputAction = null;

                posHztlMoveInputActionUIS = UISGetAction(inputActionAssetXR, posHztlMoveInputActionMapIdXR, posHztlMoveInputActionIdUIS);
                posVertMoveInputActionUIS = UISGetAction(inputActionAssetXR, posVertMoveInputActionMapIdXR, posVertMoveInputActionIdUIS);
                posHztlLookInputActionUIS = UISGetAction(inputActionAssetXR, posHztlLookInputActionMapIdXR, posHztlLookInputActionIdUIS);
                posVertLookInputActionUIS = UISGetAction(inputActionAssetXR, posVertLookInputActionMapIdXR, posVertLookInputActionIdUIS);
                posZoomLookInputActionUIS = UISGetAction(inputActionAssetXR, posZoomLookInputActionMapIdXR, posZoomLookInputActionIdUIS);
                posOrbitLookInputActionUIS = UISGetAction(inputActionAssetXR, posOrbitLookInputActionMapIdXR, posOrbitLookInputActionIdUIS);
                jumpInputActionUIS = UISGetAction(inputActionAssetXR, jumpInputActionMapIdXR, jumpInputActionIdUIS);
                sprintInputActionUIS = UISGetAction(inputActionAssetXR, sprintInputActionMapIdXR, sprintInputActionIdUIS);
                crouchInputActionUIS = UISGetAction(inputActionAssetXR, crouchInputActionMapIdXR, crouchInputActionIdUIS);
                jetpackInputActionUIS = UISGetAction(inputActionAssetXR, jetpackInputActionMapIdXR, jetpackInputActionIdUIS);
                switchLookInputActionUIS = UISGetAction(inputActionAssetXR, switchLookInputActionMapIdXR, switchLookInputActionIdUIS);
                leftFire1InputActionUIS = UISGetAction(inputActionAssetXR, leftFire1InputActionMapIdXR, leftFire1InputActionIdUIS);
                leftFire2InputActionUIS = UISGetAction(inputActionAssetXR, leftFire2InputActionMapIdXR, leftFire2InputActionIdUIS);
                rightFire1InputActionUIS = UISGetAction(inputActionAssetXR, rightFire1InputActionMapIdXR, rightFire1InputActionIdUIS);
                rightFire2InputActionUIS = UISGetAction(inputActionAssetXR, rightFire2InputActionMapIdXR, rightFire2InputActionIdUIS);

                hmdPosInputActionXR = UISGetAction(inputActionAssetXR, hmdPosInputActionMapIdXR, hmdPosInputActionIdXR);
                leftHandPosInputActionXR = UISGetAction(inputActionAssetXR, leftHandPosInputActionMapIdXR, leftHandPosInputActionIdXR);
                leftHandRotInputActionXR = UISGetAction(inputActionAssetXR, leftHandRotInputActionMapIdXR, leftHandRotInputActionIdXR);
                rightHandPosInputActionXR = UISGetAction(inputActionAssetXR, rightHandPosInputActionMapIdXR, rightHandPosInputActionIdXR);
                rightHandRotInputActionXR = UISGetAction(inputActionAssetXR, rightHandRotInputActionMapIdXR, rightHandRotInputActionIdXR);

                posHztlMoveCtrlTypeHashUIS = posHztlMoveInputActionUIS == null ? 0 : S3DMath.GetHashCode(posHztlMoveInputActionUIS.expectedControlType);
                posVertMoveCtrlTypeHashUIS = posVertMoveInputActionUIS == null ? 0 : S3DMath.GetHashCode(posVertMoveInputActionUIS.expectedControlType);
                posHztlLookCtrlTypeHashUIS = posHztlLookInputActionUIS == null ? 0 : S3DMath.GetHashCode(posHztlLookInputActionUIS.expectedControlType);
                posVertLookCtrlTypeHashUIS = posVertLookInputActionUIS == null ? 0 : S3DMath.GetHashCode(posVertLookInputActionUIS.expectedControlType);
                posZoomLookCtrlTypeHashUIS = posZoomLookInputActionUIS == null ? 0 : S3DMath.GetHashCode(posZoomLookInputActionUIS.expectedControlType);
                posOrbitLookCtrlTypeHashUIS = posOrbitLookInputActionUIS == null ? 0 : S3DMath.GetHashCode(posOrbitLookInputActionUIS.expectedControlType);
                jumpCtrlTypeHashUIS = jumpInputActionUIS == null ? 0 : S3DMath.GetHashCode(jumpInputActionUIS.expectedControlType);
                sprintCtrlTypeHashUIS = sprintInputActionUIS == null ? 0 : S3DMath.GetHashCode(sprintInputActionUIS.expectedControlType);
                crouchCtrlTypeHashUIS = crouchInputActionUIS == null ? 0 : S3DMath.GetHashCode(crouchInputActionUIS.expectedControlType);
                jetpackCtrlTypeHashUIS = jetpackInputActionUIS == null ? 0 : S3DMath.GetHashCode(jetpackInputActionUIS.expectedControlType);
                switchLookCtrlTypeHashUIS = switchLookInputActionUIS == null ? 0 : S3DMath.GetHashCode(switchLookInputActionUIS.expectedControlType);
                leftFire1CtrlTypeHashUIS = leftFire1InputActionUIS == null ? 0 : S3DMath.GetHashCode(leftFire1InputActionUIS.expectedControlType);
                leftFire2CtrlTypeHashUIS = leftFire2InputActionUIS == null ? 0 : S3DMath.GetHashCode(leftFire2InputActionUIS.expectedControlType);
                rightFire1CtrlTypeHashUIS = rightFire1InputActionUIS == null ? 0 : S3DMath.GetHashCode(rightFire1InputActionUIS.expectedControlType);
                rightFire2CtrlTypeHashUIS = rightFire2InputActionUIS == null ? 0 : S3DMath.GetHashCode(rightFire2InputActionUIS.expectedControlType);

                hmdPosCtrlTypeHashXR = hmdPosInputActionXR == null ? 0 : S3DMath.GetHashCode(hmdPosInputActionXR.expectedControlType);
                leftHandPosCtrlTypeHashXR = leftHandPosInputActionXR == null ? 0 : S3DMath.GetHashCode(leftHandPosInputActionXR.expectedControlType);
                leftHandRotCtrlTypeHashXR = leftHandRotInputActionXR == null ? 0 : S3DMath.GetHashCode(leftHandRotInputActionXR.expectedControlType);
                rightHandPosCtrlTypeHashXR = rightHandPosInputActionXR == null ? 0 : S3DMath.GetHashCode(rightHandPosInputActionXR.expectedControlType);
                rightHandRotCtrlTypeHashXR = rightHandRotInputActionXR == null ? 0 : S3DMath.GetHashCode(rightHandRotInputActionXR.expectedControlType);

                // Update all the Custom Player Inputs
                for (int ciIdx = 0; ciIdx < numCPI; ciIdx++)
                {
                    CustomInput customInput = customInputList[ciIdx];
                    if (customInput != null)
                    {
                        customInput.uisPositiveInputAction = UISGetAction(inputActionAssetXR, customInput.xrPositiveInputActionMapId, customInput.uisPositiveInputActionId);
                        customInput.uisPositiveCtrlTypeHash = customInput.uisPositiveInputAction == null ? 0 : S3DMath.GetHashCode(customInput.uisPositiveInputAction.expectedControlType);
                        customInput.lastInputValueX = 0f;
                        customInput.lastInputValueY = 0f;
                    }
                }
            }
            #if UNITY_EDITOR
            else { if (showErrors) { Debug.LogWarning(methodName + " couldn't find inputActionAssetXR for " + this.name); } }
            #endif
        }

        #endif

        #endregion
    }
}