using UnityEngine;
#if VIU_PLUGIN
using HTC.UnityPlugin.Vive;
#endif

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class containing input configuration data used in the Player Input Module and editor.
    /// A user can create custom inputs for the current InputMode and get notified when that
    /// action is taken. e.g. the player presses a button on a controller.
    [System.Serializable]
    public class CustomPlayerInput
    {

        #region Enumerations

        /// <summary>
        /// Used with CustomPlayerInputEvt
        /// </summary>
        public enum CustomPlayerInputEventType
        {
            Unknown = 0,
            Axis1D = 1,
            Axis2D = 2,
            Axis3D = 3,
            Button = 5,
            Key = 10
        }


        #endregion

        #region Public Variables

        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        #region Common input variables
        public PlayerInputModule.InputAxisMode inputAxisMode;
        public bool isSensitivityEnabled;
        [Range(0.01f, 10f)] public float sensitivity;
        [Range(0.01f, 10f)] public float gravity = 3f;
        public bool isButton;
        public bool isButtonEnabled;
        public bool canBeHeldDown;

        public CustomPlayerInputEvt customPlayerInputEvt;

        public bool showInEditor;

        internal float lastInputValueX;
        internal float lastInputValueY;

        #endregion

        #region Direct Keyboard or Mouse (dkm) input variables
        public KeyCode dkmPositiveKeycode;
        public KeyCode dkmNegativeKeycode;
        #endregion Direct Keyboard or Mouse (dkm) input variables

        #region Legacy Unity Input (lis) System
        public string lisPositiveAxisName;
        public string lisNegativeAxisName;
        public bool lisInvertAxis;
        public bool lisIsPositiveAxisValid;
        public bool lisIsNegativeAxisValid;
        #endregion Legacy Unity Input (lis) System

        #region New Unity Input (uis) System
        // Unity Input System stores actionId internally as string rather than System.Guid
        // We don't require positive and negative as Input System supports Composite Bindings
        public string uisPositiveInputActionId;
        // An Action can return a Control Type with or or more values.
        // e.g. bool, float, Vector2, Vector3. The SSC zero-based DataSlot indicates which value to use.
        // For composite bindings e.g. 2DVector/Dpad, the slot matches the axis.
        // Left/Right = x-axis = slot 1, Up/Down = y-axis = slot 2.
        public int uisPositiveInputActionDataSlot = 0;
        #endregion New Unity Input (uis) System

        #region Oculus OVR API input
        public PlayerInputModule.OculusInputType ovrInputType;
        public int ovrPositiveInput;
        public int ovrNegativeInput;
        #endregion Oculus OVR API input

        #region Rewired Input
        public int rwdPositiveInputActionId;
        public int rwdNegativeInputActionId;
        #endregion Rewired Input

        #region VIVE Input variables
        public PlayerInputModule.ViveInputType viveInputType;
        public PlayerInputModule.ViveRoleType viveRoleType;
        public int vivePositiveInputRoleId;
        public int viveNegativeInputRoleId;
        public int vivePositiveInputCtrl;
        public int viveNegativeInputCtrl;
        #endregion VIVE Input variables

        #region UnityXR
        // Unity Input System (which XR uses) stores actionMapId internally as string rather than System.Guid
        // We don't require positive and negative as Input System supports Composite Bindings
        public string xrPositiveInputActionMapId;
        #endregion UnityXR

        #endregion

        #region New Unity Input (uis) System Internal variables
        #if SSC_UIS
        // Used to map positive[inputtype]InputActionIdUIS to InputAction at runtime
        // We don't require positive and negative as Input System supports Composite Bindings
        // See PlayerInputModule.UpdateUnityInputSystemActions()
        internal UnityEngine.InputSystem.InputAction uisPositiveInputAction;
        internal int uisPositiveCtrlTypeHash;
        #endif
        #endregion

        #region Rewired Internal and Editor variables
        #if SSC_REWIRED
        // Non-serialized variables that get populated at runtime
        // Use int rather than enum for speed
        // 0 = Axis, 1 = Button
        // See PlayerInputModule.UpdateRewiredActionTypes()
        internal int rwdPositiveInputActionType;
        internal int rwdNegativeInputActionType;
        internal bool rwLastIsButtonPressed;

        #if UNITY_EDITOR
        // These variables are only required to populate the editor dropdowns in the editor
        [System.NonSerialized] public int[] actionIDsPositive, actionIDsNegative;
        [System.NonSerialized] public GUIContent[] ActionGUIContentPositive, ActionGUIContentNegative;
        #endif

        #endif
        #endregion

        #region Vive Internal variables
#if VIU_PLUGIN
        // Used for mapping the enum vive[..]RoleTypes to SystemTypes at runtime
        // See PlayerInputModule.UpdateViveInput(..) method.
        internal System.Type viveRoleSystemType = null;
        internal ControllerAxis vivePositiveInputCtrlAxis;
        internal ControllerAxis viveNegativeInputCtrlAxis;
        internal ControllerButton vivePositiveInputCtrlBtn;
        internal ControllerButton viveNegativeInputCtrlBtn;
#endif
        #endregion

        #region Constructors
        public CustomPlayerInput()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// CustomPlayerInput copy constructor
        /// </summary>
        /// <param name="customPlayerInput"></param>
        public CustomPlayerInput(CustomPlayerInput customPlayerInput)
        {
            if (customPlayerInput == null) { SetClassDefaults(); }
            else
            {
                #region Common variables
                inputAxisMode = customPlayerInput.inputAxisMode;
                isSensitivityEnabled = customPlayerInput.isSensitivityEnabled;
                sensitivity = customPlayerInput.sensitivity;
                showInEditor = customPlayerInput.showInEditor;
                isButton = customPlayerInput.isButton;
                canBeHeldDown = customPlayerInput.canBeHeldDown;
                isButtonEnabled = customPlayerInput.isButtonEnabled;

                // At the moment, don't attempt to copy
                customPlayerInputEvt = null;
                #endregion

                #region Direct Keyboard or Mouse input variables
                dkmPositiveKeycode = customPlayerInput.dkmPositiveKeycode;
                dkmNegativeKeycode = customPlayerInput.dkmNegativeKeycode;
                #endregion

                #region Legacy Unity Input (lis) System
                lisPositiveAxisName = customPlayerInput.lisPositiveAxisName;
                lisNegativeAxisName = customPlayerInput.lisNegativeAxisName;
                lisInvertAxis = customPlayerInput.lisInvertAxis;
                lisIsPositiveAxisValid = customPlayerInput.lisIsPositiveAxisValid;
                lisIsNegativeAxisValid = customPlayerInput.lisIsNegativeAxisValid;
                #endregion

                #region New Unity Input (uis) System
                uisPositiveInputActionId = string.IsNullOrEmpty(customPlayerInput.uisPositiveInputActionId) ? string.Empty : string.Copy(customPlayerInput.uisPositiveInputActionId);
                uisPositiveInputActionDataSlot = customPlayerInput.uisPositiveInputActionDataSlot;
                #endregion New Unity Input (uis) System

                #region Oculus OVR API input
                ovrInputType = customPlayerInput.ovrInputType;
                ovrPositiveInput = customPlayerInput.ovrPositiveInput;
                ovrNegativeInput = customPlayerInput.ovrNegativeInput;
                #endregion Oculus OVR API input

                #region Rewired Input
                rwdPositiveInputActionId = customPlayerInput.rwdPositiveInputActionId;
                rwdNegativeInputActionId = customPlayerInput.rwdNegativeInputActionId;
                #endregion Rewired Input

                #region VIVE Input variables
                viveInputType = customPlayerInput.viveInputType;
                viveRoleType = customPlayerInput.viveRoleType;
                vivePositiveInputRoleId = customPlayerInput.vivePositiveInputRoleId;
                viveNegativeInputRoleId = customPlayerInput.viveNegativeInputRoleId;
                vivePositiveInputCtrl = customPlayerInput.vivePositiveInputCtrl;
                viveNegativeInputCtrl = customPlayerInput.viveNegativeInputCtrl;
                #endregion VIVE Input variables

                #region UnityXR
                xrPositiveInputActionMapId = string.IsNullOrEmpty(customPlayerInput.xrPositiveInputActionMapId) ? string.Empty : string.Copy(customPlayerInput.xrPositiveInputActionMapId);
                #endregion UnityXR
            }
        }

        #endregion

        #region Public Member Methods

        /// <summary>
        /// Set the defaults values for this class
        /// </summary>
        public void SetClassDefaults()
        {
            #region Common variables
            inputAxisMode = PlayerInputModule.InputAxisMode.NoInput;
            isSensitivityEnabled = false;
            sensitivity = 3f;
            showInEditor = true;
            customPlayerInputEvt = null;
            isButton = false;
            canBeHeldDown = false;
            isButtonEnabled = false;
            #endregion

            #region Direct Keyboard or Mouse input variables
            dkmPositiveKeycode = KeyCode.None;
            dkmNegativeKeycode = KeyCode.None;
            #endregion

            #region Legacy Unity Input (lis) System
            lisPositiveAxisName = null;
            lisNegativeAxisName = null;
            lisInvertAxis = false;
            lisIsPositiveAxisValid = true;
            lisIsNegativeAxisValid = true;
            #endregion

            #region New Unity Input (uis) System
            uisPositiveInputActionId = "";
            uisPositiveInputActionDataSlot = 0;
            #endregion New Unity Input (uis) System

            #region Oculus OVR API input
            ovrInputType = PlayerInputModule.OculusInputType.Axis1D;
            // 0 = None in OVRInput
            ovrPositiveInput = 0;
            ovrNegativeInput = 0;
            #endregion Oculus OVR API input

            #region Rewired Input
            rwdPositiveInputActionId = -1;
            rwdNegativeInputActionId = -1;
            #endregion Rewired Input

            #region VIVE Input variables
            viveInputType = PlayerInputModule.ViveInputType.Axis;
            viveRoleType = PlayerInputModule.ViveRoleType.HandRole;
            vivePositiveInputRoleId = 0;
            viveNegativeInputRoleId = 0;
            vivePositiveInputCtrl = 0;
            viveNegativeInputCtrl = 0;
            #endregion VIVE Input variables

            #region UnityXR
            xrPositiveInputActionMapId = "";
            #endregion UnityXR
        }

        #endregion
    }
}