using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2019-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class containing input configuration data used in the Sticky Input Module and editor.
    /// A user can create custom inputs for the current InputMode and get notified when that
    /// action is taken. e.g. the player presses a button on a controller.
    /// </summary>
    [System.Serializable]
    public class CustomInput
    {
        #region Enumerations

        /// <summary>
        /// Used with CustomInputEvt
        /// </summary>
        public enum CustomInputEventType
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
        public StickyInputModule.InputAxisMode inputAxisMode;
        public bool isSensitivityEnabled;
        [Range(0.01f, 10f)] public float sensitivity;
        [Range(0.01f, 10f)] public float gravity = 3f;
        public bool isButton;
        public bool isButtonEnabled;
        public bool canBeHeldDown;

        public CustomInputEvt customInputEvt;

        public bool showInEditor;

        /// <summary>
        /// List of guidHash's for the S3DAnimActions from the StickyControlModule
        /// Animate tab. Input is sent to none, one or more S3DAnimActions.
        /// </summary>
        public List<int> animActionHashList;

        public bool isAnimActionHashListExpanded;

        [System.NonSerialized] internal float lastInputValueX;
        [System.NonSerialized] internal float lastInputValueY;
        [System.NonSerialized] internal int numCustomAnimActions;

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

        #region Rewired Input
        public int rwdPositiveInputActionId;
        public int rwdNegativeInputActionId;
        #endregion Rewired Input

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
        // See StickyInputModule.UpdateUnityInputSystemActions()
        internal UnityEngine.InputSystem.InputAction uisPositiveInputAction;
        internal int uisPositiveCtrlTypeHash;
        #endif
        #endregion

        #region Rewired Internal and Editor variables
        #if SSC_REWIRED
        // Non-serialized variables that get populated at runtime
        // Use int rather than enum for speed
        // 0 = Axis, 1 = Button
        // See StickyInputModule.UpdateRewiredActionTypes()
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

        #region Constructors
        public CustomInput()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Create a custom event with a listener
        /// </summary>
        /// <param name="call"></param>
        public CustomInput (UnityEngine.Events.UnityAction<Vector3, int> call)
        {
            SetClassDefaults();

            customInputEvt = new CustomInputEvt();
            customInputEvt.AddListener(call);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="customInput"></param>
        public CustomInput(CustomInput customInput)
        {
            if (customInput == null) { SetClassDefaults(); }
            else
            {
                #region Common variables
                inputAxisMode = customInput.inputAxisMode;
                isSensitivityEnabled = customInput.isSensitivityEnabled;
                sensitivity = customInput.sensitivity;
                showInEditor = customInput.showInEditor;
                isButton = customInput.isButton;
                canBeHeldDown = customInput.canBeHeldDown;
                isButtonEnabled = customInput.isButtonEnabled;

                // At the moment, don't attempt to copy
                customInputEvt = null;
                #endregion

                #region Direct Keyboard or Mouse input variables
                dkmPositiveKeycode = customInput.dkmPositiveKeycode;
                dkmNegativeKeycode = customInput.dkmNegativeKeycode;
                #endregion

                #region Legacy Unity Input (lis) System
                lisPositiveAxisName = customInput.lisPositiveAxisName;
                lisNegativeAxisName = customInput.lisNegativeAxisName;
                lisInvertAxis = customInput.lisInvertAxis;
                lisIsPositiveAxisValid = customInput.lisIsPositiveAxisValid;
                lisIsNegativeAxisValid = customInput.lisIsNegativeAxisValid;
                #endregion

                #region New Unity Input (uis) System
                uisPositiveInputActionId = customInput.uisPositiveInputActionId;
                uisPositiveInputActionDataSlot = customInput.uisPositiveInputActionDataSlot;
                #endregion New Unity Input (uis) System

                #region Rewired Input
                rwdPositiveInputActionId = customInput.rwdPositiveInputActionId;
                rwdNegativeInputActionId = customInput.rwdNegativeInputActionId;
                #endregion Rewired Input
            }
        }

        #endregion

        #region Public Member Methods

        /// <summary>
        /// Make this custom input a button.
        /// It is enabled by default.
        /// </summary>
        /// <param name="canBeHeld"></param>
        public void AddButton (bool canBeHeld)
        {
            canBeHeldDown = canBeHeld;
            isButton = true;
            isButtonEnabled = true;
        }

        /// <summary>
        /// Set the defaults values for this class
        /// </summary>
        public void SetClassDefaults()
        {
            #region Common variables
            inputAxisMode = StickyInputModule.InputAxisMode.NoInput;
            isSensitivityEnabled = false;
            sensitivity = 3f;
            showInEditor = true;
            customInputEvt = null;
            isButton = false;
            canBeHeldDown = false;
            isButtonEnabled = false;
            isAnimActionHashListExpanded = true;
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

            #region Rewired Input
            rwdPositiveInputActionId = -1;
            rwdNegativeInputActionId = -1;
            #endregion Rewired Input
        }

        #endregion
    }
}
