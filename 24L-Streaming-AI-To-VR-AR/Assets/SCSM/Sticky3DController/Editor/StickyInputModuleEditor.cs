using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
#if SSC_REWIRED
using Rewired;
#endif

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(StickyInputModule))]
    public class StickyInputModuleEditor : Editor
    {
        #region Enumerations
        /// <summary>
        /// Used internally in StickyInputModuleEditor only
        /// </summary>
        public enum PIMCategory
        {
            HorizontalMove = 0,
            VerticalMove = 1,
            HorizontalLook = 2,
            VerticalLook = 3,
            ZoomLook = 4,
            OrbitLook = 5,
            Sprint = 10,
            Jump = 11,
            Crouch = 12,
            JetPack = 13,
            SwitchLook = 14,
            CustomInput = 21
        };
        #endregion

        #region Custom Editor private variables
        private StickyInputModule stickyInputModule;
        private StickyControlModule stickyControlModule;
        private CharacterInput characterInput = null;  // used for debugging
        private CharacterInputXR characterInputXR = null;  // used for debugging
        private bool isStylesInitialised = false;
        private bool isRefreshing = false;
        // Formatting and style variables
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle headingFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle foldoutStyleNoLabel;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        private bool isDebuggingEnabled = false;
        private Color separatorColor;
        // custom input variables
        private int ciDeletePos = -1;
        private int ciMoveDownPos = -1;
        private int ciAADeletePos = -1;

        // The AnimAction GUID/Name lists are a pair and must remain in sync
        private List<int> animActionGUIDHashList;
        private List<string> animActionNameList;
        private float animActionListLastRefreshed = 0f;

        [System.NonSerialized] private List<S3DAnimParm> animParmList;

        #if SSC_UIS
        // Unity Input System variables
        // Ref to a Player Input component that should be attached to the same gameobject
        // Refresh this by calling stickyInputModule.GetUISPlayerInput().
        private UnityEngine.InputSystem.PlayerInput uisPlayerInput = null;
        private string uisVersion = "";
        private string[] uisActionNames;
        private string[] uisActionIDs;
        private GUIContent[] uisActionGUIContent;
        private readonly static string uisDropDownNone = "<None>";
        #endif

        #if SSC_REWIRED
        // Rewired variables
        private string rwVersion = "";
        /// <summary>
        /// Array of Action Category Names
        /// </summary>
        private string[] actionCategories;
        /// <summary>
        /// List of Action Category Id values (may not be the "index" in the array)
        /// </summary>
        private int[] actionCategoryIDs;
        private GUIContent[] ActionCategoryGUIContent;
        private int[] actionIDsHztlMovePositive, actionIDsHztlMoveNegative;
        private GUIContent[] ActionGUIContentHztlMovePositive, ActionGUIContentHztlMoveNegative;
        private int[] actionIDsVertMovePositive, actionIDsVertMoveNegative;
        private GUIContent[] ActionGUIContentVertMovePositive, ActionGUIContentVertMoveNegative;
        private int[] actionIDsHztlLookPositive, actionIDsHztlLookNegative;
        private GUIContent[] ActionGUIContentHztlLookPositive, ActionGUIContentHztlLookNegative;
        private int[] actionIDsVertLookPositive, actionIDsVertLookNegative;
        private GUIContent[] ActionGUIContentVertLookPositive, ActionGUIContentVertLookNegative;
        private int[] actionIDsZoomLookPositive, actionIDsZoomLookNegative;
        private GUIContent[] ActionGUIContentZoomLookPositive, ActionGUIContentZoomLookNegative;
        private int[] actionIDsOrbitLookPositive, actionIDsOrbitLookNegative;
        private GUIContent[] ActionGUIContentOrbitLookPositive, ActionGUIContentOrbitLookNegative;

        private int[] actionIDsJump, actionIDsSprint, actionIDsCrouch, actionIDsJetPack, actionIDsSwitchLook;
        private GUIContent[] ActionGUIContentJump, ActionGUIContentSprint, ActionGUIContentCrouch, ActionGUIContentJetPack, ActionGUIContentSwitchLook;

        private int[] actionIDsLeftFire1, actionIDsLeftFire2, actionIDsRightFire1, actionIDsRightFire2;
        private GUIContent[] ActionGUIContentLeftFire1, ActionGUIContentLeftFire2, ActionGUIContentRightFire1, ActionGUIContentRightFire2;

        private Rewired.InputManager rewiredInputManager = null;
        private Rewired.Data.UserData rewiredUserData = null;
        // Re-usable list to construct rewired action arrays
        private List<int> actionIDList = new List<int>(20);
        private List<GUIContent> ActionGUIContentList = new List<GUIContent>(20);
        private readonly static string dropDownNone = "None";
        #endif

        #if SCSM_XR && SSC_UIS
        private string xrVersion = "";
        private string openXRVersion = "";
        private string oculusXRVersion = "";
        private string[] xrActionMapIDs;
        private List<string> xrActionNameList = null;
        private List<string> xrActionIDList = null;
        private List<GUIContent> xrActionGUIContentList = null;
        private string xrCurrentActionMapId = string.Empty;
        private bool xrForceActionRefresh = true;
        private readonly static string xrHandsOffsetName = "XR Hands Offset";
        private readonly static string xrLeftHandName = "XR Left Hand";
        private readonly static string xrRightHandName = "XR Right Hand";
        private readonly static string xrPluginNotInstalled = "Not Installed";
        #endif

        #endregion

        #region Static Strings
        private readonly static string emptyString = "";
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("This module enables you to configure user input");
        private readonly static GUIContent horizontalMoveHeaderContent = new GUIContent("Horizontal Move Input Axis", "Left or right movement");
        private readonly static GUIContent verticalMoveHeaderContent = new GUIContent("Vertical Move Input Axis", "Forward or backward movement");
        private readonly static GUIContent jumpHeaderContent = new GUIContent("Jump & Jet Pack Up Button", "Jump or move up when Jet Pack is enabled");
        private readonly static GUIContent sprintHeaderContent = new GUIContent("Sprint Button", "Sprint or run");
        private readonly static GUIContent crouchHeaderContent = new GUIContent("Crouch & Jet Pack Down Button", "Crouch down or move down when the Jet Pack is enabled");
        private readonly static GUIContent jetpackHeaderContent = new GUIContent("Jet Pack Enable Button", "Toggle the Jet Pack on or off");
        private readonly static GUIContent horizontalLookHeaderContent = new GUIContent("Horizontal Look Input Axis", "Look left or right");
        private readonly static GUIContent verticalLookHeaderContent = new GUIContent("Vertical Look Input Axis", "Look up or down");
        private readonly static GUIContent switchLookHeaderContent = new GUIContent("Switch Look Button", "Switch between first- and third-person view");
        private readonly static GUIContent zoomLookHeaderContent = new GUIContent("Zoom Input Axis", "Zoom the camera in or out");
        private readonly static GUIContent orbitLookHeaderContent = new GUIContent("Orbit Input Axis");
        private readonly static GUIContent leftFire1HeaderContent = new GUIContent("Left Fire 1 Button", "Fire the primary weapon mechanism");
        private readonly static GUIContent leftFire2HeaderContent = new GUIContent("Left Fire 2 Button", "Fire the secondary weapon mechanism");
        private readonly static GUIContent rightFire1HeaderContent = new GUIContent("Right Fire 1 Button", "Fire the primary weapon mechanism");
        private readonly static GUIContent rightFire2HeaderContent = new GUIContent("Right Fire 2 Button", "Fire the secondary weapon mechanism");
        private readonly static GUIContent ciHeaderContent = new GUIContent("Custom Inputs");
        #endregion

        #region GUIContent - General
        private readonly static GUIContent inputModeContent = new GUIContent(" Input Mode", "How input is received from the user. When " +
            "Direct Keyboard is selected, keyboard is received directly from the keyboard. When Legacy Unity is selected, input is received " +
            "from the axes defined in the legacy input manager.");
        private readonly static GUIContent initialiseOnAwakeContent = new GUIContent(" Initialise on Awake", "If enabled, Initialise() will be called as soon as Awake() runs. This should be disabled if you want to control when the Sticky Input Module is initialised through code. [Default: ON]");
        private readonly static GUIContent isEnabledOnInitialiseContent = new GUIContent(" Enable on Initialise", "Is input enabled when the module is first initialised?  See also EnableInput() and DisableInput(). [Default: ON]");
        private readonly static GUIContent canBeHeldContent = new GUIContent(" Can Be Held", "When enabled, holding the key or button down will cause button events to be sent continuously.");
        private readonly static GUIContent crouchIsToggledContent = new GUIContent(" Is Crouch Toggled", "When enabled, the character will start or stop crouching each time input is detected. It has no effect when the Jet Pack is enabled.");
        private readonly static GUIContent isJetPackOnlyContent = new GUIContent(" Is Jet Pack Only", "This input will only take effect when in Jet Pack mode");

        private readonly static GUIContent sensitivityEnabledContent = new GUIContent(" Use Sensitivity", "Should axis sensitivity and gravity be applied to this axis?");
        private readonly static GUIContent sensitivityContent = new GUIContent(" Sensitivity", "Speed to move towards target values. Lower values make it less sensitive");
        private readonly static GUIContent gravityContent = new GUIContent(" Gravity", "The rate at which the values return to the middle or neutral position");

        private readonly static GUIContent ciEventContent = new GUIContent(" Callback Method", "This is your method that gets called when the input event occurs.");
        private readonly static GUIContent ciIsButtonContent = new GUIContent(" Is Button", "Is this input a pressable button");
        private readonly static GUIContent ciCanBeHeldDownContent = new GUIContent(" Can Be Held Down", "Can this button be continuously held down?");
        private readonly static GUIContent ciNoCustomAnimActionsContent = new GUIContent(" No Custom Anim Actions on Animate tab", "Check the Animate tab on the StickyControlModule");
        private readonly static GUIContent ciAnimActionContent = new GUIContent(" Animate Action", "The custom Animate Action from the Animate tab on the StickyControlModule");

        #endregion

        #region GUIContent - Direct Keyboard Input

        #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
        private readonly static GUIContent positiveKeycodeContent = new GUIContent(" Positive Key", "The key mapped to positive input " +
            "on this axis. Select None if you don't want to receive positive input on this axis.");
        private readonly static GUIContent negativeKeycodeContent = new GUIContent(" Negative Key", "The key mapped to negative input " +
            "on this axis. Select None if you don't want to receive positive input on this axis.");
        private readonly static string identicalKeycodesWarningContent = "Positive and negative keycodes are required to be different.";
        private readonly static GUIContent singleKeycodeContent = new GUIContent(" Key", "The key mapped to input " +
            "on this axis. Select None if you don't want to receive input on this axis.");
        private readonly static GUIContent dkiMouseXContent = new GUIContent(" Mouse or Trackpad X axis", "This is preconfigured for Direct Keyboard mode");
        private readonly static GUIContent dkiMouseYContent = new GUIContent(" Mouse or Trackpad Y axis", "This is preconfigured for Direct Keyboard mode");
        private readonly static GUIContent dkiMouseScrollWheelContent = new GUIContent(" Mouse ScrollWheel", "Use the scroll wheel to control zoom");

        private readonly static GUIContent dkiMouseXIsDistanceCentreContent = new GUIContent(" Distance from centre", "Use the distance the mouse X is from the centre of the screen rather than the movement delta");
        private readonly static GUIContent dkiMouseYIsDistanceCentreContent = new GUIContent(" Distance from centre", "Use the distance the mouse Y is from the centre of the screen rather than the movement delta");

        private readonly static GUIContent dkiMouseXSensitivityContent = new GUIContent(" Mouse X sensitivity");
        private readonly static GUIContent dkiMouseYSensitivityContent = new GUIContent(" Mouse Y sensitivity");
        private readonly static GUIContent dkiMouseXDeadzoneContent = new GUIContent(" Mouse X deadzone");
        private readonly static GUIContent dkiMouseYDeadzoneContent = new GUIContent(" Mouse Y deadzone");
        #endif
        #endregion

        #region GUIContent - Legacy Unity Input System

        #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
        private readonly static GUIContent inputAxisModeContent = new GUIContent(" Axis Mode", "How input will be received from the input " +
            "manager. When No Input is selected, no input will be received from the input manager on that axis (use this option if you " +
            "don't want the player to be able to control that axis of movement). When Single Axis is selected, input will be received " +
            "from a single axis specified in the input manager. When Combined Axis is selected, input will be received from two axes " +
            "specified in the input manager and added together to give the final input.");
        private readonly static GUIContent singleAxisNameContent = new GUIContent(" Axis Name", "The name of this axis in the input manager.");
        private readonly static GUIContent invertInputAxisContent = new GUIContent(" Invert Axis", "When enabled, the input received from " +
            "this axis will be inverted.");
        private readonly static GUIContent positiveAxisNameContent = new GUIContent(" Positive Axis Name", "The name of the positive axis " +
            "in the input manager.");
        private readonly static GUIContent negativeAxisNameContent = new GUIContent(" Negative Axis Name", "The name of the negative axis " +
            "in the input manager.");
        private readonly static string identicalAxisNamesWarningContent = "Positive and negative axis names are required to be different.";
        private readonly static string invalidLegacyAxisNameWarningContent = "The axis is undefined. Check the input manager.";

        //private readonly static GUIContent lisPitchInputMouseContent = new GUIContent(" Use Mouse");
        //private readonly static GUIContent lisYawInputMouseContent = new GUIContent(" Use Mouse");

        //private readonly static GUIContent lisPitchSensitivityContent = new GUIContent(" Mouse sensitivity");
        //private readonly static GUIContent lisYawSensitivityContent = new GUIContent(" Mouse sensitivity");
        //private readonly static GUIContent lisPitchDeadzoneContent = new GUIContent(" Mouse deadzone");
        //private readonly static GUIContent lisYawDeadzoneContent = new GUIContent(" Mouse deadzone");
#endif

        #endregion GUIContent - Legacy Unity Input System

        #region GUIContent - Unity Input System (from package manager)
#if SSC_UIS
        private readonly static GUIContent inputSystemAxisModeContent = new GUIContent(" Axis Mode", "How input will be received from the Unity " +
            "Input System. When No Input is selected, no input will be received on that axis (use this option if you don't want the player " +
            "to be able to control that axis of movement). When Single Axis is selected, input will be received from a single Action configured " +
            "in the Input Action Importer Editor. Combined Axis is not used with Unity Input System because a single Action can be configured with Composite Bindings.");

        private readonly static GUIContent uisVersionContent = new GUIContent(" Input System version", "The version of the Unity Input System installed from Package Manager");

        private readonly static GUIContent[] uisDataSlots2 = { new GUIContent("Slot1"), new GUIContent("Slot2") };
        private readonly static GUIContent[] uisDataSlots3 = { new GUIContent("Slot1"), new GUIContent("Slot2"), new GUIContent("Slot3") };

        private readonly static GUIContent uisInputATypeContent = new GUIContent(" Action Type");
        private readonly static GUIContent uisInputCTypeContent = new GUIContent(" Control Type", "Supported Control Types are Button, Axis, Vector2/Dpad and Vector3");
        private readonly static GUIContent uisInputDataSlotContent = new GUIContent(" Data Slot", "The slot or position of the value being used in the returned Control Type. For example a Vector2 returns two floats. To use the first float use Slot1.");

        private readonly static GUIContent uisHztlMoveInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisVertMoveInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisHztlLookInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisVertLookInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisZoomLookInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisOrbitLookInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisLeftFire1InputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisLeftFire2InputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisRightFire1InputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisRightFire2InputAContent = new GUIContent(" Action");

        private readonly static GUIContent uisJumpEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent uisJumpInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisSprintEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent uisSprintInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisCrouchEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent uisCrouchInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisJetPackEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent uisJetPackInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisSwitchLookEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent uisSwitchLookInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisLeftFire1EnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent uisLeftFire2EnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent uisRightFire1EnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent uisRightFire2EnabledContent = new GUIContent(" Is Enabled");

        private readonly static GUIContent uisHztlLookInputMouseContent = new GUIContent(" Use Mouse");
        private readonly static GUIContent uisVertLookInputMouseContent = new GUIContent(" Use Mouse");

        private readonly static GUIContent uisHztlLookSensitivityContent = new GUIContent(" Mouse sensitivity");
        private readonly static GUIContent uisVertLookSensitivityContent = new GUIContent(" Mouse sensitivity");
        private readonly static GUIContent uisHztlLookDeadzoneContent = new GUIContent(" Mouse deadzone");
        private readonly static GUIContent uisVertLookDeadzoneContent = new GUIContent(" Mouse deadzone");

        private readonly static GUIContent uisCustomEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent uisCustomInputAContent = new GUIContent(" Action");

        private readonly static GUIContent uisDebugApplyChangedContent = new GUIContent("Apply Changes", "Apply Axis setup changes");

        private readonly static GUIContent uisAddActionsContent = new GUIContent("Add Actions", "Add a typical set of actions to the Default Map in the attached Unity Player Input component. Does not override existing Actions. Will then configure all Axes.");
        //private readonly static GUIContent uisOnScreenControlsContent = new GUIContent("On-Screen Controls", "Add or configure on-screen controls");
#endif
        #endregion

        #region GUIContent - Rewired
#if SSC_REWIRED
        // Rewired
        private readonly static string rwidenticalAxisIdWarningContent = "Positive and negative axis actions are required to be different.";

        private readonly static GUIContent rwPlayerNumberContent = new GUIContent(" Player Number", "The human player controlling this character. " +
              "0 = unassigned. Typically 1,2,3,4 etc. If the Rewired player is set using StickyInputModule .SetRewiredPlayer(..) in your code, leave it as 0 here. " +
              "NOTE: At runtime, Players must be first assigned in Rewired.");

        private readonly static GUIContent rwVersionContent = new GUIContent(" Rewired version");

        private readonly static GUIContent rwHztlMoveInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwHztlMoveInputACPositiveContent = new GUIContent(" Positive Action Category");
        private readonly static GUIContent rwHztlMoveInputACNegativeContent = new GUIContent(" Negative Action Category");
        private readonly static GUIContent rwHztlMoveInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwHztlMoveInputAIPositiveContent = new GUIContent(" Positive Action Input");
        private readonly static GUIContent rwHztlMoveInputAINegativeContent = new GUIContent(" Negative Action Input");

        private readonly static GUIContent rwVertMoveInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwVertMoveInputACPositiveContent = new GUIContent(" Positive Action Category");
        private readonly static GUIContent rwVertMoveInputACNegativeContent = new GUIContent(" Negative Action Category");
        private readonly static GUIContent rwVertMoveInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwVertMoveInputAIPositiveContent = new GUIContent(" Positive Action Input");
        private readonly static GUIContent rwVertMoveInputAINegativeContent = new GUIContent(" Negative Action Input");

        private readonly static GUIContent rwHztlLookInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwHztlLookInputACPositiveContent = new GUIContent(" Positive Action Category");
        private readonly static GUIContent rwHztlLookInputACNegativeContent = new GUIContent(" Negative Action Category");
        private readonly static GUIContent rwHztlLookInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwHztlLookInputAIPositiveContent = new GUIContent(" Positive Action Input");
        private readonly static GUIContent rwHztlLookInputAINegativeContent = new GUIContent(" Negative Action Input");

        private readonly static GUIContent rwVertLookInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwVertLookInputACPositiveContent = new GUIContent(" Positive Action Category");
        private readonly static GUIContent rwVertLookInputACNegativeContent = new GUIContent(" Negative Action Category");
        private readonly static GUIContent rwVertLookInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwVertLookInputAIPositiveContent = new GUIContent(" Positive Action Input");
        private readonly static GUIContent rwVertLookInputAINegativeContent = new GUIContent(" Negative Action Input");

        private readonly static GUIContent rwZoomLookInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwZoomLookInputACPositiveContent = new GUIContent(" Positive Action Category");
        private readonly static GUIContent rwZoomLookInputACNegativeContent = new GUIContent(" Negative Action Category");
        private readonly static GUIContent rwZoomLookInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwZoomLookInputAIPositiveContent = new GUIContent(" Positive Action Input");
        private readonly static GUIContent rwZoomLookInputAINegativeContent = new GUIContent(" Negative Action Input");

        private readonly static GUIContent rwOrbitLookInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwOrbitLookInputACPositiveContent = new GUIContent(" Positive Action Category");
        private readonly static GUIContent rwOrbitLookInputACNegativeContent = new GUIContent(" Negative Action Category");
        private readonly static GUIContent rwOrbitLookInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwOrbitLookInputAIPositiveContent = new GUIContent(" Positive Action Input");
        private readonly static GUIContent rwOrbitLookInputAINegativeContent = new GUIContent(" Negative Action Input");

        private readonly static GUIContent rwJumpEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent rwJumpInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwJumpInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwSprintEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent rwSprintInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwSprintInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwCrouchEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent rwCrouchInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwCrouchInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwJetPackEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent rwJetPackInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwJetPackInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwSwitchLookEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent rwSwitchLookInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwSwitchLookInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwLeftFire1EnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent rwLeftFire1InputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwLeftFire1InputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwLeftFire2EnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent rwLeftFire2InputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwLeftFire2InputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwRightFire1EnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent rwRightFire1InputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwRightFire1InputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwRightFire2EnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent rwRightFire2InputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwRightFire2InputAIContent = new GUIContent(" Action Input");

        private readonly static GUIContent rwCustomEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent rwCustomInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwCustomInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwCustomInputACPositiveContent = new GUIContent(" Positive Action Category");
        private readonly static GUIContent rwCustomInputACNegativeContent = new GUIContent(" Negative Action Category");
        private readonly static GUIContent rwCustomInputAIPositiveContent = new GUIContent(" Positive Action Input");
        private readonly static GUIContent rwCustomInputAINegativeContent = new GUIContent(" Negative Action Input");
#endif
        #endregion

        #region GUIContent - XR
        #if SCSM_XR && SSC_UIS
        private readonly static GUIContent xrHMDPosHeaderContent = new GUIContent("HMD Position Input Axis", "The position input from the VR head-mounted device");
        private readonly static GUIContent xrLeftHandPosHeaderContent = new GUIContent("Left Hand Position Input Axis", "The position input from the VR left hand controller");
        private readonly static GUIContent xrLeftHandRotHeaderContent = new GUIContent("Left Hand Rotation Input Axis", "The rotation input from the VR left hand controller");
        private readonly static GUIContent xrRightHandPosHeaderContent = new GUIContent("Right Hand Position Input Axis", "The position input from the VR right hand controller");
        private readonly static GUIContent xrRightHandRotHeaderContent = new GUIContent("Right Hand Rotation Input Axis", "The rotation input from the VR right hand controller");
        private readonly static GUIContent xrLookHMDHeaderContent = new GUIContent("Look HMD Input Axis", "Look in any direction using the VR headset");
        private readonly static GUIContent xrLookTurnHeaderContent = new GUIContent("Look Turn Input Axis", "Look or turn left or right");
        private readonly static GUIContent xrPluginVersionContent = new GUIContent(" XR Plugin version", "The version of the Unity XR Management plugin installed from Package Manager");
        private readonly static GUIContent xrOpenXRVersionContent = new GUIContent(" OpenXR version", "The version of the OpenXR plugin installed from Package Manager");
        private readonly static GUIContent xrOculusXRVersionContent = new GUIContent(" Oculus XR version", "The version of the Oculus plugin installed from Package Manager");
        private readonly static GUIContent xrInputActionAssetContent = new GUIContent(" Input Action Asset", "A scriptableobject asset containing Unity Input System action maps and control schemes.");
        private readonly static GUIContent xrFirstPersonTransform1Content = new GUIContent(" XR Camera Transform", "The transform of the XR first person camera.");
        private readonly static GUIContent xrFirstPersonCamera1Content = new GUIContent(" XR Camera", "The transform of the XR first person camera.");
        private readonly static GUIContent xrLeftHandTransformContent = new GUIContent(" Left Hand", "The transform of the XR left hand");
        private readonly static GUIContent xrRightHandTransformContent = new GUIContent(" Right Hand", "The transform of the XR right hand");
        private readonly static GUIContent xrLeftHandOffsetRotContent = new GUIContent(" LH Offset Rotation", "The left-hand offset rotation, in Euler angles. Used to correct hand controller rotation. E.g., 45 deg on x-axis");
        private readonly static GUIContent xrRightHandOffsetRotContent = new GUIContent(" RH Offset Rotation", "The right-hand offset rotation, in Euler angles. Used to correct hand controller rotation. E.g., 45 deg on x-axis");
        private readonly static GUIContent xrFirstPersonNewCamera1Content = new GUIContent("New", "Create a new XR first person camera as a child of the Sticky3D character");
        private readonly static GUIContent xrNewHandContent = new GUIContent("New", "Create a new XR hand as a child of the Sticky3D character");
        private readonly static GUIContent xrInputAMContent = new GUIContent(" Action Map", "Unity Input System Action Map in the Input Action Asset");
        private static GUIContent[] xrActionMapGUIContent;

        private readonly static GUIContent xrLeftHandPosInputAContent = new GUIContent(" Action", "This should return a vector3. E.g., <XRController>{LeftHand}/pointerPosition");
        private readonly static GUIContent xrLeftHandRotInputAContent = new GUIContent(" Action", "This should return a quaternion. E.g., <XRController>{LeftHand}/pointerRotation");
        private readonly static GUIContent xrRightHandPosInputAContent = new GUIContent(" Action", "This should return a vector3. E.g., <XRController>{RightHand}/pointerPosition");
        private readonly static GUIContent xrRightHandRotInputAContent = new GUIContent(" Action", "This should return a quaternion. E.g., <XRController>{RightHand}/pointerRotation");
        private readonly static GUIContent xrHMDPosInputAContent = new GUIContent(" Action", "This should return a vector3. E.g., <XRHMD>/centerEyePosition");
        private readonly static GUIContent xrLookHMDInputAContent = new GUIContent(" Action", "This should return a quaternion. E.g., <XRHMD>/centerEyeRotation");
        private readonly static GUIContent xrLookTurnInputAContent = new GUIContent(" Action", "This should return a quaternion. E.g., <XRController>{RightHand}/Primary2DAxis");
        #endif
        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent(" Debug Mode", "Use this to troubleshoot the data at runtime in the editor.");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent(" Is Initialised?");
        private readonly static GUIContent debugIsInputEnabledContent = new GUIContent(" Is Input Enabled?");
        private readonly static GUIContent debugIsCustomInputOnlyEnabledContent = new GUIContent(" Is Custom Input Only?");
        private readonly static GUIContent debugHorizontalMoveContent = new GUIContent(" Horizontal Move");
        private readonly static GUIContent debugVerticalMoveContent = new GUIContent(" Vertical Move");
        private readonly static GUIContent debugJumpContent = new GUIContent(" Is Jumping?");
        private readonly static GUIContent debugSprintContent = new GUIContent(" Is Sprinting?");
        private readonly static GUIContent debugCrouchContent = new GUIContent(" Is Crouching?");
        private readonly static GUIContent debugJetBackToggleContent = new GUIContent(" Is Changing JetPack");
        private readonly static GUIContent debugJetPackEnabledContent = new GUIContent(" Is Jet Pack Enabled");
        private readonly static GUIContent debugHorizontalLookContent = new GUIContent(" Horizontal Look");
        private readonly static GUIContent debugVerticalLookContent = new GUIContent(" Vertical Look");
        private readonly static GUIContent debugXRLookContent = new GUIContent(" XR Look");
        private readonly static GUIContent debugZoomLookContent = new GUIContent(" Zoom Look");
        private readonly static GUIContent debugOrbitLookContent = new GUIContent(" Orbit Look");
        private readonly static GUIContent debugLeftFire1Content = new GUIContent(" Left Fire1");
        private readonly static GUIContent debugLeftFire2Content = new GUIContent(" Left Fire2");
        private readonly static GUIContent debugRightFire1Content = new GUIContent(" Right Fire1");
        private readonly static GUIContent debugRightFire2Content = new GUIContent(" Right Fire2");

        #if SCSM_XR && SSC_UIS
        private readonly static GUIContent debugHMDXRPositionContent = new GUIContent(" HMD Position");
        private readonly static GUIContent debugLeftHandXRGripContent = new GUIContent(" Left Hand Grip");
        private readonly static GUIContent debugLeftHandXRPositionContent = new GUIContent(" Left Hand Position");
        private readonly static GUIContent debugLeftHandXRTriggerContent = new GUIContent(" Left Hand Trigger");
        private readonly static GUIContent debugLeftHandXRRotationContent = new GUIContent(" Left Hand Rotation");
        private readonly static GUIContent debugRightHandXRGripContent = new GUIContent(" Right Hand Grip");
        private readonly static GUIContent debugRightHandXRPositionContent = new GUIContent(" Right Hand Position");
        private readonly static GUIContent debugRightHandXRTriggerContent = new GUIContent(" Right Hand Trigger");
        private readonly static GUIContent debugRightHandXRRotationContent = new GUIContent(" Right Hand Rotation");
        #endif

        #endregion

        #region Serialized Properties - General
        private SerializedProperty inputModeProp;
        private SerializedProperty initialiseOnAwakeProp;
        private SerializedProperty isEnabledOnInitialiseProp;
        private SerializedProperty hztlMoveShowInEditorProp;
        private SerializedProperty vertMoveShowInEditorProp;
        private SerializedProperty jumpShowInEditorProp;
        private SerializedProperty jumpCanBeHeldProp;
        private SerializedProperty leftFire1CanBeHeldProp;
        private SerializedProperty leftFire2CanBeHeldProp;
        private SerializedProperty rightFire1CanBeHeldProp;
        private SerializedProperty rightFire2CanBeHeldProp;
        private SerializedProperty sprintShowInEditorProp;
        private SerializedProperty crouchShowInEditorProp;
        private SerializedProperty crouchCanBeHeldProp;
        private SerializedProperty crouchIsToggledProp;
        private SerializedProperty crouchIsJetPackOnlyProp;
        private SerializedProperty jetpackShowInEditorProp;
        private SerializedProperty switchLookShowInEditorProp;
        private SerializedProperty hztlLookShowInEditorProp;
        private SerializedProperty vertLookShowInEditorProp;
        private SerializedProperty zoomLookShowInEditorProp;
        private SerializedProperty orbitLookShowInEditorProp;
        private SerializedProperty leftFire1ShowInEditorProp;
        private SerializedProperty leftFire2ShowInEditorProp;
        private SerializedProperty rightFire1ShowInEditorProp;
        private SerializedProperty rightFire2ShowInEditorProp;

        // Common to Legacy Input System and (New) Input Syste
        private SerializedProperty hztlMoveInputAxisModeProp;
        private SerializedProperty vertMoveInputAxisModeProp;
        private SerializedProperty hztlLookInputAxisModeProp;
        private SerializedProperty vertLookInputAxisModeProp;
        private SerializedProperty zoomLookInputAxisModeProp;
        private SerializedProperty orbitLookInputAxisModeProp;
        
        #endregion

        #region SerializedProperties - Direct Keyboard Input
        #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
        private SerializedProperty posHztrlMoveInputKeycodeProp;
        private SerializedProperty negHztrlMoveInputKeycodeProp;
        private SerializedProperty posVertMoveInputKeycodeProp;
        private SerializedProperty negVertMoveInputKeycodeProp;
        private SerializedProperty jumpInputKeycodeProp;
        private SerializedProperty sprintInputKeycodeProp;
        private SerializedProperty crouchInputKeycodeProp;
        private SerializedProperty jetpackInputKeycodeProp;
        private SerializedProperty switchLookInputKeycodeProp;
        private SerializedProperty hztlLookSensitivityDKIProp;
        private SerializedProperty vertLookSensitivityDKIProp;
        private SerializedProperty hztlLookDeadzoneDKIProp;
        private SerializedProperty vertLookDeadzoneDKIProp;
        private SerializedProperty isMouseCentreHztlLookDKIProp;
        private SerializedProperty isMouseCentreVertLookDKIProp;
        private SerializedProperty posZoomInputKeycodeProp;
        private SerializedProperty negZoomInputKeycodeProp;
        private SerializedProperty isMouseScrollForZoomEnabledDKIProp;
        private SerializedProperty posOrbitInputKeycodeProp;
        private SerializedProperty negOrbitInputKeycodeProp;
        private SerializedProperty leftFire1InputKeycodeProp;
        private SerializedProperty leftFire2InputKeycodeProp;
        private SerializedProperty rightFire1InputKeycodeProp;
        private SerializedProperty rightFire2InputKeycodeProp;
        #endif
        #endregion

        #region SerializedProperties - Legacy Unity input
        #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
        private SerializedProperty posHztlMoveInputAxisNameProp;
        private SerializedProperty negHztlMoveInputAxisNameProp;
        private SerializedProperty posVertMoveInputAxisNameProp;
        private SerializedProperty negVertMoveInputAxisNameProp;
        private SerializedProperty posHztlLookInputAxisNameProp;
        private SerializedProperty negHztlLookInputAxisNameProp;
        private SerializedProperty posVertLookInputAxisNameProp;
        private SerializedProperty negVertLookInputAxisNameProp;
        private SerializedProperty posZoomLookInputAxisNameProp;
        private SerializedProperty negZoomLookInputAxisNameProp;
        private SerializedProperty posOrbitLookInputAxisNameProp;
        private SerializedProperty negOrbitLookInputAxisNameProp;
        private SerializedProperty jumpInputAxisNameProp;
        private SerializedProperty sprintInputAxisNameProp;
        private SerializedProperty crouchInputAxisNameProp;
        private SerializedProperty jetpackInputAxisNameProp;
        private SerializedProperty switchLookInputAxisNameProp;
        private SerializedProperty leftFire1InputAxisNameProp;
        private SerializedProperty leftFire2InputAxisNameProp;
        private SerializedProperty rightFire1InputAxisNameProp;
        private SerializedProperty rightFire2InputAxisNameProp;
        private SerializedProperty invertHztlMoveInputAxisProp;
        private SerializedProperty invertVertMoveInputAxisProp;
        private SerializedProperty invertHztlLookInputAxisProp;
        private SerializedProperty invertVertLookInputAxisProp;
        private SerializedProperty invertZoomLookInputAxisProp;
        private SerializedProperty invertOrbitLookInputAxisProp;

        private SerializedProperty isPosHztlMoveInputAxisValidProp;
        private SerializedProperty isNegHztlMoveInputAxisValidProp;
        private SerializedProperty isPosVertMoveInputAxisValidProp;
        private SerializedProperty isNegVertMoveInputAxisValidProp;
        private SerializedProperty isJumpInputAxisValidProp;
        private SerializedProperty isSprintInputAxisValidProp;
        private SerializedProperty isCrouchInputAxisValidProp;
        private SerializedProperty isJetPackInputAxisValidProp;
        private SerializedProperty isSwitchLookInputAxisValidProp;
        private SerializedProperty isPosHztlLookInputAxisValidProp;
        private SerializedProperty isNegHztlLookInputAxisValidProp;
        private SerializedProperty isPosVertLookInputAxisValidProp;
        private SerializedProperty isNegVertLookInputAxisValidProp;
        private SerializedProperty isPosZoomLookInputAxisValidProp;
        private SerializedProperty isNegZoomLookInputAxisValidProp;
        private SerializedProperty isPosOrbitLookInputAxisValidProp;
        private SerializedProperty isNegOrbitLookInputAxisValidProp;
        private SerializedProperty isLeftFire1InputAxisValidProp;
        private SerializedProperty isLeftFire2InputAxisValidProp;
        private SerializedProperty isRightFire1InputAxisValidProp;
        private SerializedProperty isRightFire2InputAxisValidProp;

        #endif
        #endregion

        #region SerializedProperties - Rewired or Unity Input System
        #if SSC_REWIRED || SSC_UIS
        private SerializedProperty jumpButtonEnabledProp;
        private SerializedProperty sprintButtonEnabledProp;
        private SerializedProperty crouchButtonEnabledProp;
        private SerializedProperty jetpackButtonEnabledProp;
        private SerializedProperty switchLookButtonEnabledProp;
        private SerializedProperty leftFire1ButtonEnabledProp;
        private SerializedProperty leftFire2ButtonEnabledProp;
        private SerializedProperty rightFire1ButtonEnabledProp;
        private SerializedProperty rightFire2ButtonEnabledProp;
        #endif
        #endregion SerializedProperties - Rewired or Unity Input System

        #region SerializedProperties - Unity Input System
        #if SSC_UIS
        private SerializedProperty posHztlMoveInputActionIdUISProp;
        private SerializedProperty posHztlMoveInputActionDataSlotUISProp;
        private SerializedProperty posVertMoveInputActionIdUISProp;
        private SerializedProperty posVertMoveInputActionDataSlotUISProp;
        private SerializedProperty posHztlLookInputActionIdUISProp;
        private SerializedProperty posHztlLookInputActionDataSlotUISProp;
        private SerializedProperty posVertLookInputActionIdUISProp;
        private SerializedProperty posVertLookInputActionDataSlotUISProp;
        private SerializedProperty posZoomLookInputActionIdUISProp;
        private SerializedProperty posZoomLookInputActionDataSlotUISProp;
        private SerializedProperty posOrbitLookInputActionIdUISProp;
        private SerializedProperty posOrbitLookInputActionDataSlotUISProp;
        private SerializedProperty jumpInputActionIdUISProp;
        private SerializedProperty jumpInputActionDataSlotUISProp;
        private SerializedProperty sprintInputActionIdUISProp;
        private SerializedProperty sprintInputActionDataSlotUISProp;
        private SerializedProperty crouchInputActionIdUISProp;
        private SerializedProperty crouchInputActionDataSlotUISProp;
        private SerializedProperty jetpackInputActionIdUISProp;
        private SerializedProperty jetpackInputActionDataSlotUISProp;
        private SerializedProperty switchLookInputActionIdUISProp;
        private SerializedProperty switchLookInputActionDataSlotUISProp;
        private SerializedProperty leftFire1InputActionIdUISProp;
        private SerializedProperty leftFire1InputActionDataSlotUISProp;
        private SerializedProperty leftFire2InputActionIdUISProp;
        private SerializedProperty leftFire2InputActionDataSlotUISProp;
        private SerializedProperty rightFire1InputActionIdUISProp;
        private SerializedProperty rightFire1InputActionDataSlotUISProp;
        private SerializedProperty rightFire2InputActionIdUISProp;
        private SerializedProperty rightFire2InputActionDataSlotUISProp;

        private SerializedProperty isMouseForHztlLookEnabledUISProp;
        private SerializedProperty isMouseForVertLookEnabledUISProp;
        private SerializedProperty hztlLookSensitivityUISProp;
        private SerializedProperty vertLookSensitivityUISProp;
        private SerializedProperty hztlLookDeadzoneUISProp;
        private SerializedProperty vertLookDeadzoneUISProp;
        #endif
        #endregion SerializedProperties - Unity Input System

        #region SerializedProperties - Rewired
        #if SSC_REWIRED
        private SerializedProperty rewiredPlayerNumberProp;
        private SerializedProperty posHztlMoveInputActionIdRWDProp;
        private SerializedProperty negHztlMoveInputActionIdRWDProp;
        private SerializedProperty posVertMoveInputActionIdRWDProp;
        private SerializedProperty negVertMoveInputActionIdRWDProp;
        private SerializedProperty posHztlLookInputActionIdRWDProp;
        private SerializedProperty negHztlLookInputActionIdRWDProp;
        private SerializedProperty posVertLookInputActionIdRWDProp;
        private SerializedProperty negVertLookInputActionIdRWDProp;
        private SerializedProperty posZoomLookInputActionIdRWDProp;
        private SerializedProperty negZoomLookInputActionIdRWDProp;
        private SerializedProperty posOrbitLookInputActionIdRWDProp;
        private SerializedProperty negOrbitLookInputActionIdRWDProp;
        private SerializedProperty jumpInputActionIdRWDProp;
        private SerializedProperty sprintInputActionIdRWDProp;
        private SerializedProperty crouchInputActionIdRWDProp;
        private SerializedProperty jetpackInputActionIdRWDProp;
        private SerializedProperty switchLookInputActionIdRWDProp;
        private SerializedProperty leftFire1InputActionIdRWDProp;
        private SerializedProperty leftFire2InputActionIdRWDProp;
        private SerializedProperty rightFire1InputActionIdRWDProp;
        private SerializedProperty rightFire2InputActionIdRWDProp;
        #endif
        #endregion

        #region SerializedProperties - XR
        #if SCSM_XR && SSC_UIS
        private SerializedProperty hmdPosShowInEditorProp;
        private SerializedProperty leftHandPosShowInEditorProp;
        private SerializedProperty leftHandRotShowInEditorProp;
        private SerializedProperty rightHandPosShowInEditorProp;
        private SerializedProperty rightHandRotShowInEditorProp;

        private SerializedProperty inputActionAssetXRProp;
        private SerializedProperty firstPersonTransform1XRProp;
        private SerializedProperty firstPersonCamera1XRProp;
        private SerializedProperty leftHandTransformXRProp;
        private SerializedProperty rightHandTransformXRProp;
        private SerializedProperty leftHandOffsetRotXRProp;
        private SerializedProperty rightHandOffsetRotXRProp;

        private SerializedProperty hmdPosInputAxisModeProp;
        private SerializedProperty leftHandPosInputAxisModeProp;
        private SerializedProperty leftHandRotInputAxisModeProp;
        private SerializedProperty rightHandPosInputAxisModeProp;
        private SerializedProperty rightHandRotInputAxisModeProp;

        private SerializedProperty hmdPosInputActionIdXRProp;
        private SerializedProperty leftHandPosInputActionIdXRProp;
        private SerializedProperty leftHandRotInputActionIdXRProp;
        private SerializedProperty rightHandPosInputActionIdXRProp;
        private SerializedProperty rightHandRotInputActionIdXRProp;

        private SerializedProperty hmdPosInputActionDataSlotXRProp;
        private SerializedProperty leftHandPosInputActionDataSlotXRProp;
        private SerializedProperty leftHandRotInputActionDataSlotXRProp;
        private SerializedProperty rightHandPosInputActionDataSlotXRProp;
        private SerializedProperty rightHandRotInputActionDataSlotXRProp;

        private SerializedProperty posHztlMoveInputActionMapIdXRProp;
        private SerializedProperty posVertMoveInputActionMapIdXRProp;
        private SerializedProperty posHztlLookInputActionMapIdXRProp;
        private SerializedProperty posVertLookInputActionMapIdXRProp;
        private SerializedProperty posZoomLookInputActionMapIdXRProp;
        private SerializedProperty posOrbitLookInputActionMapIdXRProp;
        private SerializedProperty jumpInputActionMapIdXRProp;
        private SerializedProperty sprintInputActionMapIdXRProp;
        private SerializedProperty crouchInputActionMapIdXRProp;
        private SerializedProperty jetpackInputActionMapIdXRProp;
        private SerializedProperty switchLookInputActionMapIdXRProp;
        private SerializedProperty leftFire1InputActionMapIdXRProp;
        private SerializedProperty leftFire2InputActionMapIdXRProp;
        private SerializedProperty rightFire1InputActionMapIdXRProp;
        private SerializedProperty rightFire2InputActionMapIdXRProp;

        private SerializedProperty hmdPosInputActionMapIdXRProp;
        private SerializedProperty leftHandPosInputActionMapIdXRProp;
        private SerializedProperty leftHandRotInputActionMapIdXRProp;
        private SerializedProperty rightHandPosInputActionMapIdXRProp;
        private SerializedProperty rightHandRotInputActionMapIdXRProp;

        #endif
        #endregion

        #region SerializedProperties - Custom Inputs (ci)

        private SerializedProperty ciListProp;
        private SerializedProperty ciItemProp;
        private SerializedProperty ciItemShowInEditorProp;
        private SerializedProperty ciShowInEditorProp;
        private SerializedProperty ciIsListExpandedProp;
        private SerializedProperty ciEventProp;
        private SerializedProperty ciIsButtonProp;
        private SerializedProperty ciCanBeHeldDownProp;
        private SerializedProperty ciInputAxisModeProp;
        private SerializedProperty ciIsSensitivityEnabledProp;
        private SerializedProperty ciSensitivityProp;
        private SerializedProperty ciGravityProp;

        private SerializedProperty ciAAGUIDHashListProp;
        private SerializedProperty ciAAGUIDHashProp;
        private SerializedProperty ciAAIsListExpandedProp;

        #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
        private SerializedProperty cidkmPositiveKeycodeProp;
        private SerializedProperty cidkmNegativeKeycodeProp;
        private SerializedProperty cilisPositiveAxisNameProp;
        private SerializedProperty cilisNegativeAxisNameProp;
        private SerializedProperty cilisInvertAxisProp;
        private SerializedProperty cilisIsPositiveAxisValidProp;
        private SerializedProperty cilisIsNegativeAxisValidProp;
        #endif

        #if SSC_UIS
        private SerializedProperty ciuisPositiveInputActionIdProp;
        private SerializedProperty ciuisPositiveInputActionDataSlotProp;
        #endif

        #if SSC_REWIRED
        private SerializedProperty cirwdPositiveInputActionIdProp;
        private SerializedProperty cirwdNegativeInputActionIdProp;
        #endif

        #if SSC_UIS || SSC_REWIRED
        private SerializedProperty ciIsButtonEnabledProp;
        #endif

        #if SCSM_XR && SSC_UIS
        private SerializedProperty cixrPositiveInputActionMapIdProp;
        #endif

        #endregion

        #region Events

        public void OnEnable()
        {
            isRefreshing = true;
            stickyInputModule = (StickyInputModule)target;

            if (stickyInputModule != null) { stickyControlModule = stickyInputModule.GetStickyControlModule(true); }
            else { stickyControlModule = null; }

            defaultEditorLabelWidth = 150f; // EditorGUIUtility.labelWidth;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;
            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            #region Find Properties - General
            inputModeProp = serializedObject.FindProperty("inputMode");
            initialiseOnAwakeProp = serializedObject.FindProperty("initialiseOnAwake");
            isEnabledOnInitialiseProp = serializedObject.FindProperty("isEnabledOnInitialise");

            hztlMoveShowInEditorProp = serializedObject.FindProperty("hztlMoveShowInEditor");
            vertMoveShowInEditorProp = serializedObject.FindProperty("vertMoveShowInEditor");
            jumpShowInEditorProp = serializedObject.FindProperty("jumpShowInEditor");
            sprintShowInEditorProp = serializedObject.FindProperty("sprintShowInEditor");
            crouchShowInEditorProp = serializedObject.FindProperty("crouchShowInEditor");
            jetpackShowInEditorProp = serializedObject.FindProperty("jetpackShowInEditor");
            switchLookShowInEditorProp = serializedObject.FindProperty("switchLookShowInEditor");
            hztlLookShowInEditorProp = serializedObject.FindProperty("hztlLookShowInEditor");
            vertLookShowInEditorProp = serializedObject.FindProperty("vertLookShowInEditor");
            zoomLookShowInEditorProp = serializedObject.FindProperty("zoomLookShowInEditor");
            orbitLookShowInEditorProp = serializedObject.FindProperty("orbitLookShowInEditor");
            leftFire1ShowInEditorProp = serializedObject.FindProperty("leftFire1ShowInEditor");
            leftFire2ShowInEditorProp = serializedObject.FindProperty("leftFire2ShowInEditor");
            rightFire1ShowInEditorProp = serializedObject.FindProperty("rightFire1ShowInEditor");
            rightFire2ShowInEditorProp = serializedObject.FindProperty("rightFire2ShowInEditor");

            // Common to legacy input system and (new) input system
            hztlMoveInputAxisModeProp = serializedObject.FindProperty("hztlMoveInputAxisMode");
            vertMoveInputAxisModeProp = serializedObject.FindProperty("vertMoveInputAxisMode");
            hztlLookInputAxisModeProp = serializedObject.FindProperty("hztlLookInputAxisMode");
            vertLookInputAxisModeProp = serializedObject.FindProperty("vertLookInputAxisMode");
            zoomLookInputAxisModeProp = serializedObject.FindProperty("zoomLookInputAxisMode");
            orbitLookInputAxisModeProp = serializedObject.FindProperty("orbitLookInputAxisMode");

            jumpCanBeHeldProp = serializedObject.FindProperty("jumpCanBeHeld");
            crouchCanBeHeldProp = serializedObject.FindProperty("crouchCanBeHeld");
            crouchIsToggledProp = serializedObject.FindProperty("crouchIsToggled");
            crouchIsJetPackOnlyProp = serializedObject.FindProperty("crouchIsJetPackOnly");
            leftFire1CanBeHeldProp = serializedObject.FindProperty("leftFire1CanBeHeld");
            leftFire2CanBeHeldProp = serializedObject.FindProperty("leftFire2CanBeHeld");
            rightFire1CanBeHeldProp = serializedObject.FindProperty("rightFire1CanBeHeld");
            rightFire2CanBeHeldProp = serializedObject.FindProperty("rightFire2CanBeHeld");

            #endregion

            #region Find Properties - Direct Keyboard
            #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
            posHztrlMoveInputKeycodeProp = serializedObject.FindProperty("posHztrlMoveInputKeycode");
            negHztrlMoveInputKeycodeProp = serializedObject.FindProperty("negHztrlMoveInputKeycode");
            posVertMoveInputKeycodeProp = serializedObject.FindProperty("posVertMoveInputKeycode");
            negVertMoveInputKeycodeProp = serializedObject.FindProperty("negVertMoveInputKeycode");
            jumpInputKeycodeProp = serializedObject.FindProperty("jumpInputKeycode");
            sprintInputKeycodeProp = serializedObject.FindProperty("sprintInputKeycode");
            crouchInputKeycodeProp = serializedObject.FindProperty("crouchInputKeycode");
            jetpackInputKeycodeProp = serializedObject.FindProperty("jetpackInputKeycode");
            switchLookInputKeycodeProp = serializedObject.FindProperty("switchLookInputKeycode");
            hztlLookSensitivityDKIProp = serializedObject.FindProperty("hztlLookSensitivityDKI");
            vertLookSensitivityDKIProp = serializedObject.FindProperty("vertLookSensitivityDKI");
            hztlLookDeadzoneDKIProp = serializedObject.FindProperty("hztlLookDeadzoneDKI");
            vertLookDeadzoneDKIProp = serializedObject.FindProperty("vertLookDeadzoneDKI");
            isMouseCentreHztlLookDKIProp = serializedObject.FindProperty("isMouseCentreHztlLookDKI");
            isMouseCentreVertLookDKIProp = serializedObject.FindProperty("isMouseCentreVertLookDKI");
            posZoomInputKeycodeProp = serializedObject.FindProperty("posZoomInputKeycode");
            negZoomInputKeycodeProp = serializedObject.FindProperty("negZoomInputKeycode");
            isMouseScrollForZoomEnabledDKIProp = serializedObject.FindProperty("isMouseScrollForZoomEnabledDKI");
            posOrbitInputKeycodeProp = serializedObject.FindProperty("posOrbitInputKeycode");
            negOrbitInputKeycodeProp = serializedObject.FindProperty("negOrbitInputKeycode");
            leftFire1InputKeycodeProp = serializedObject.FindProperty("leftFire1InputKeycode");
            leftFire2InputKeycodeProp = serializedObject.FindProperty("leftFire2InputKeycode");
            rightFire1InputKeycodeProp = serializedObject.FindProperty("rightFire1InputKeycode");
            rightFire2InputKeycodeProp = serializedObject.FindProperty("rightFire2InputKeycode");
            #endif
            #endregion

            #region Find Properties - legacy unity input
            #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
            posHztlMoveInputAxisNameProp = serializedObject.FindProperty("posHztlMoveInputAxisName");
            negHztlMoveInputAxisNameProp = serializedObject.FindProperty("negHztlMoveInputAxisName");
            posVertMoveInputAxisNameProp = serializedObject.FindProperty("posVertMoveInputAxisName");
            negVertMoveInputAxisNameProp = serializedObject.FindProperty("negVertMoveInputAxisName");
            jumpInputAxisNameProp = serializedObject.FindProperty("jumpInputAxisName");
            sprintInputAxisNameProp = serializedObject.FindProperty("sprintInputAxisName");
            crouchInputAxisNameProp = serializedObject.FindProperty("crouchInputAxisName");
            jetpackInputAxisNameProp = serializedObject.FindProperty("jetpackInputAxisName");
            switchLookInputAxisNameProp = serializedObject.FindProperty("switchLookInputAxisName");
            posHztlLookInputAxisNameProp = serializedObject.FindProperty("posHztlLookInputAxisName");
            negHztlLookInputAxisNameProp = serializedObject.FindProperty("negHztlLookInputAxisName");
            posVertLookInputAxisNameProp = serializedObject.FindProperty("posVertLookInputAxisName");
            negVertLookInputAxisNameProp = serializedObject.FindProperty("negVertLookInputAxisName");
            posZoomLookInputAxisNameProp = serializedObject.FindProperty("posZoomLookInputAxisName");
            negZoomLookInputAxisNameProp = serializedObject.FindProperty("negZoomLookInputAxisName");
            posOrbitLookInputAxisNameProp = serializedObject.FindProperty("posOrbitLookInputAxisName");
            negOrbitLookInputAxisNameProp = serializedObject.FindProperty("negOrbitLookInputAxisName");
            leftFire1InputAxisNameProp = serializedObject.FindProperty("leftFire1InputAxisName");
            leftFire2InputAxisNameProp = serializedObject.FindProperty("leftFire2InputAxisName");
            rightFire1InputAxisNameProp = serializedObject.FindProperty("rightFire1InputAxisName");
            rightFire2InputAxisNameProp = serializedObject.FindProperty("rightFire2InputAxisName");
            invertHztlMoveInputAxisProp = serializedObject.FindProperty("invertHztlMoveInputAxis");
            invertVertMoveInputAxisProp = serializedObject.FindProperty("invertVertMoveInputAxis");
            invertHztlLookInputAxisProp = serializedObject.FindProperty("invertHztlLookInputAxis");
            invertVertLookInputAxisProp = serializedObject.FindProperty("invertVertLookInputAxis");
            invertZoomLookInputAxisProp = serializedObject.FindProperty("invertZoomLookInputAxis");
            invertOrbitLookInputAxisProp = serializedObject.FindProperty("invertOrbitLookInputAxis");

            isPosHztlMoveInputAxisValidProp = serializedObject.FindProperty("isPosHztlMoveInputAxisValid");
            isNegHztlMoveInputAxisValidProp = serializedObject.FindProperty("isNegHztlMoveInputAxisValid");
            isPosVertMoveInputAxisValidProp = serializedObject.FindProperty("isPosVertMoveInputAxisValid");
            isNegVertMoveInputAxisValidProp = serializedObject.FindProperty("isNegVertMoveInputAxisValid");
            isJumpInputAxisValidProp = serializedObject.FindProperty("isJumpInputAxisValid");
            isSprintInputAxisValidProp = serializedObject.FindProperty("isSprintInputAxisValid");
            isCrouchInputAxisValidProp = serializedObject.FindProperty("isCrouchInputAxisValid");
            isJetPackInputAxisValidProp = serializedObject.FindProperty("isJetPackInputAxisValid");
            isSwitchLookInputAxisValidProp = serializedObject.FindProperty("isSwitchLookInputAxisValid");
            isPosHztlLookInputAxisValidProp = serializedObject.FindProperty("isPosHztlLookInputAxisValid");
            isNegHztlLookInputAxisValidProp = serializedObject.FindProperty("isNegHztlLookInputAxisValid");
            isPosVertLookInputAxisValidProp = serializedObject.FindProperty("isPosVertLookInputAxisValid");
            isNegVertLookInputAxisValidProp = serializedObject.FindProperty("isNegVertLookInputAxisValid");
            isPosZoomLookInputAxisValidProp = serializedObject.FindProperty("isPosZoomLookInputAxisValid");
            isNegZoomLookInputAxisValidProp = serializedObject.FindProperty("isNegZoomLookInputAxisValid");
            isPosOrbitLookInputAxisValidProp = serializedObject.FindProperty("isPosOrbitLookInputAxisValid");
            isNegOrbitLookInputAxisValidProp = serializedObject.FindProperty("isNegOrbitLookInputAxisValid");
            isLeftFire1InputAxisValidProp = serializedObject.FindProperty("isLeftFire1InputAxisValid");
            isLeftFire2InputAxisValidProp = serializedObject.FindProperty("isLeftFire2InputAxisValid");
            isRightFire1InputAxisValidProp = serializedObject.FindProperty("isRightFire1InputAxisValid");
            isRightFire2InputAxisValidProp = serializedObject.FindProperty("isRightFire2InputAxisValid");

            #endif
            #endregion

            #region Find Properties - Rewired or Unity Input System
            #if SSC_REWIRED || SSC_UIS
            jumpButtonEnabledProp = serializedObject.FindProperty("jumpButtonEnabled");
            sprintButtonEnabledProp = serializedObject.FindProperty("sprintButtonEnabled");
            crouchButtonEnabledProp = serializedObject.FindProperty("crouchButtonEnabled");
            jetpackButtonEnabledProp = serializedObject.FindProperty("jetpackButtonEnabled");
            switchLookButtonEnabledProp = serializedObject.FindProperty("switchLookButtonEnabled");
            leftFire1ButtonEnabledProp = serializedObject.FindProperty("leftFire1ButtonEnabled");
            leftFire2ButtonEnabledProp = serializedObject.FindProperty("leftFire2ButtonEnabled");
            rightFire1ButtonEnabledProp = serializedObject.FindProperty("rightFire1ButtonEnabled");
            rightFire2ButtonEnabledProp = serializedObject.FindProperty("rightFire2ButtonEnabled");
            #endif
            #endregion SerializedProperties - Rewired or Unity Input System

            #region Find Properties - Unity Input System (from Package Manager)
            #if SSC_UIS
            posHztlMoveInputActionIdUISProp = serializedObject.FindProperty("posHztlMoveInputActionIdUIS");
            posVertMoveInputActionIdUISProp = serializedObject.FindProperty("posVertMoveInputActionIdUIS");
            posHztlLookInputActionIdUISProp = serializedObject.FindProperty("posHztlLookInputActionIdUIS");
            posVertLookInputActionIdUISProp = serializedObject.FindProperty("posVertLookInputActionIdUIS");
            posZoomLookInputActionIdUISProp = serializedObject.FindProperty("posZoomLookInputActionIdUIS");
            posOrbitLookInputActionIdUISProp = serializedObject.FindProperty("posOrbitLookInputActionIdUIS");
            jumpInputActionIdUISProp = serializedObject.FindProperty("jumpInputActionIdUIS");
            sprintInputActionIdUISProp = serializedObject.FindProperty("sprintInputActionIdUIS");
            crouchInputActionIdUISProp = serializedObject.FindProperty("crouchInputActionIdUIS");
            jetpackInputActionIdUISProp = serializedObject.FindProperty("jetpackInputActionIdUIS");
            switchLookInputActionIdUISProp = serializedObject.FindProperty("switchLookInputActionIdUIS");
            leftFire1InputActionIdUISProp = serializedObject.FindProperty("leftFire1InputActionIdUIS");
            leftFire2InputActionIdUISProp = serializedObject.FindProperty("leftFire2InputActionIdUIS");
            rightFire1InputActionIdUISProp = serializedObject.FindProperty("rightFire1InputActionIdUIS");
            rightFire2InputActionIdUISProp = serializedObject.FindProperty("rightFire2InputActionIdUIS");

            isMouseForHztlLookEnabledUISProp = serializedObject.FindProperty("isMouseForHztlLookEnabledUIS");
            isMouseForVertLookEnabledUISProp = serializedObject.FindProperty("isMouseForVertLookEnabledUIS");

            hztlLookSensitivityUISProp = serializedObject.FindProperty("hztlLookSensitivityUIS");
            vertLookSensitivityUISProp = serializedObject.FindProperty("vertLookSensitivityUIS");
            hztlLookDeadzoneUISProp = serializedObject.FindProperty("hztrLookDeadzoneUIS");
            vertLookDeadzoneUISProp = serializedObject.FindProperty("vertLookDeadzoneUIS");

            posHztlMoveInputActionDataSlotUISProp = serializedObject.FindProperty("posHztlMoveInputActionDataSlotUIS");
            posVertMoveInputActionDataSlotUISProp = serializedObject.FindProperty("posVertMoveInputActionDataSlotUIS");
            posHztlLookInputActionDataSlotUISProp = serializedObject.FindProperty("posHztlLookInputActionDataSlotUIS");
            posVertLookInputActionDataSlotUISProp = serializedObject.FindProperty("posVertLookInputActionDataSlotUIS");
            posZoomLookInputActionDataSlotUISProp = serializedObject.FindProperty("posZoomLookInputActionDataSlotUIS");
            posOrbitLookInputActionDataSlotUISProp = serializedObject.FindProperty("posOrbitLookInputActionDataSlotUIS");
            jumpInputActionDataSlotUISProp = serializedObject.FindProperty("jumpInputActionDataSlotUIS");
            sprintInputActionDataSlotUISProp = serializedObject.FindProperty("sprintInputActionDataSlotUIS");
            crouchInputActionDataSlotUISProp = serializedObject.FindProperty("crouchInputActionDataSlotUIS");
            jetpackInputActionDataSlotUISProp = serializedObject.FindProperty("jetpackInputActionDataSlotUIS");
            switchLookInputActionDataSlotUISProp = serializedObject.FindProperty("switchLookInputActionDataSlotUIS");
            leftFire1InputActionDataSlotUISProp = serializedObject.FindProperty("leftFire1InputActionDataSlotUIS");
            leftFire2InputActionDataSlotUISProp = serializedObject.FindProperty("leftFire2InputActionDataSlotUIS");
            rightFire1InputActionDataSlotUISProp = serializedObject.FindProperty("rightFire1InputActionDataSlotUIS");
            rightFire2InputActionDataSlotUISProp = serializedObject.FindProperty("rightFire2InputActionDataSlotUIS");

            #endif
            #endregion

            #region Find Properties - Rewired
            #if SSC_REWIRED
            rewiredPlayerNumberProp = serializedObject.FindProperty("rewiredPlayerNumber");
            posHztlMoveInputActionIdRWDProp = serializedObject.FindProperty("posHztlMoveInputActionIdRWD");
            negHztlMoveInputActionIdRWDProp = serializedObject.FindProperty("negHztlMoveInputActionIdRWD");
            posVertMoveInputActionIdRWDProp = serializedObject.FindProperty("posVertMoveInputActionIdRWD");
            negVertMoveInputActionIdRWDProp = serializedObject.FindProperty("negVertMoveInputActionIdRWD");
            posHztlLookInputActionIdRWDProp = serializedObject.FindProperty("posHztlLookInputActionIdRWD");
            negHztlLookInputActionIdRWDProp = serializedObject.FindProperty("negHztlLookInputActionIdRWD");
            posVertLookInputActionIdRWDProp = serializedObject.FindProperty("posVertLookInputActionIdRWD");
            negVertLookInputActionIdRWDProp = serializedObject.FindProperty("negVertLookInputActionIdRWD");
            posZoomLookInputActionIdRWDProp = serializedObject.FindProperty("posZoomLookInputActionIdRWD");
            negZoomLookInputActionIdRWDProp = serializedObject.FindProperty("negZoomLookInputActionIdRWD");
            posOrbitLookInputActionIdRWDProp = serializedObject.FindProperty("posOrbitLookInputActionIdRWD");
            negOrbitLookInputActionIdRWDProp = serializedObject.FindProperty("negOrbitLookInputActionIdRWD");

            jumpInputActionIdRWDProp = serializedObject.FindProperty("jumpInputActionIdRWD");
            sprintInputActionIdRWDProp = serializedObject.FindProperty("sprintInputActionIdRWD");
            crouchInputActionIdRWDProp = serializedObject.FindProperty("crouchInputActionIdRWD");
            jetpackInputActionIdRWDProp = serializedObject.FindProperty("jetpackInputActionIdRWD");
            switchLookInputActionIdRWDProp = serializedObject.FindProperty("switchLookInputActionIdRWD");
            leftFire1InputActionIdRWDProp = serializedObject.FindProperty("leftFire1InputActionIdRWD");
            leftFire2InputActionIdRWDProp = serializedObject.FindProperty("leftFire2InputActionIdRWD");
            rightFire1InputActionIdRWDProp = serializedObject.FindProperty("rightFire1InputActionIdRWD");
            rightFire2InputActionIdRWDProp = serializedObject.FindProperty("rightFire2InputActionIdRWD");
            #endif
            #endregion

            #region Find Properties - XR
            #if SCSM_XR && SSC_UIS
            hmdPosShowInEditorProp = serializedObject.FindProperty("hmdPosShowInEditor");
            leftHandPosShowInEditorProp = serializedObject.FindProperty("leftHandPosShowInEditor");
            leftHandRotShowInEditorProp = serializedObject.FindProperty("leftHandRotShowInEditor");
            rightHandPosShowInEditorProp = serializedObject.FindProperty("rightHandPosShowInEditor");
            rightHandRotShowInEditorProp = serializedObject.FindProperty("rightHandRotShowInEditor");

            hmdPosInputAxisModeProp = serializedObject.FindProperty("hmdPosInputAxisMode");
            leftHandPosInputAxisModeProp = serializedObject.FindProperty("leftHandPosInputAxisMode");
            leftHandRotInputAxisModeProp = serializedObject.FindProperty("leftHandRotInputAxisMode");
            rightHandPosInputAxisModeProp = serializedObject.FindProperty("rightHandPosInputAxisMode");
            rightHandRotInputAxisModeProp = serializedObject.FindProperty("rightHandRotInputAxisMode");

            inputActionAssetXRProp = serializedObject.FindProperty("inputActionAssetXR");
            //firstPersonTransform1XRProp = serializedObject.FindProperty("firstPersonTransform1XR");
            //firstPersonCamera1XRProp = serializedObject.FindProperty("firstPersonCamera1XR");
            leftHandTransformXRProp = serializedObject.FindProperty("leftHandTransformXR");
            rightHandTransformXRProp = serializedObject.FindProperty("rightHandTransformXR");
            leftHandOffsetRotXRProp = serializedObject.FindProperty("leftHandOffsetRotXR");
            rightHandOffsetRotXRProp = serializedObject.FindProperty("rightHandOffsetRotXR");

            hmdPosInputActionIdXRProp = serializedObject.FindProperty("hmdPosInputActionIdXR");
            leftHandPosInputActionIdXRProp = serializedObject.FindProperty("leftHandPosInputActionIdXR");
            leftHandRotInputActionIdXRProp = serializedObject.FindProperty("leftHandRotInputActionIdXR");
            rightHandPosInputActionIdXRProp = serializedObject.FindProperty("rightHandPosInputActionIdXR");
            rightHandRotInputActionIdXRProp = serializedObject.FindProperty("rightHandRotInputActionIdXR");

            hmdPosInputActionDataSlotXRProp = serializedObject.FindProperty("hmdPosInputActionDataSlotXR");
            leftHandPosInputActionDataSlotXRProp = serializedObject.FindProperty("leftHandPosInputActionDataSlotXR");
            leftHandRotInputActionDataSlotXRProp = serializedObject.FindProperty("leftHandRotInputActionDataSlotXR");
            rightHandPosInputActionDataSlotXRProp = serializedObject.FindProperty("rightHandPosInputActionDataSlotXR");
            rightHandRotInputActionDataSlotXRProp = serializedObject.FindProperty("rightHandRotInputActionDataSlotXR");

            hmdPosInputActionMapIdXRProp = serializedObject.FindProperty("hmdPosInputActionMapIdXR");
            leftHandPosInputActionMapIdXRProp = serializedObject.FindProperty("leftHandPosInputActionMapIdXR");
            leftHandRotInputActionMapIdXRProp = serializedObject.FindProperty("leftHandRotInputActionMapIdXR");
            rightHandPosInputActionMapIdXRProp = serializedObject.FindProperty("rightHandPosInputActionMapIdXR");
            rightHandRotInputActionMapIdXRProp = serializedObject.FindProperty("rightHandRotInputActionMapIdXR");

            posHztlMoveInputActionMapIdXRProp = serializedObject.FindProperty("posHztlMoveInputActionMapIdXR");
            posVertMoveInputActionMapIdXRProp = serializedObject.FindProperty("posVertMoveInputActionMapIdXR");
            posHztlLookInputActionMapIdXRProp = serializedObject.FindProperty("posHztlLookInputActionMapIdXR");
            posVertLookInputActionMapIdXRProp = serializedObject.FindProperty("posVertLookInputActionMapIdXR");
            posZoomLookInputActionMapIdXRProp = serializedObject.FindProperty("posZoomLookInputActionMapIdXR");
            posOrbitLookInputActionMapIdXRProp = serializedObject.FindProperty("posOrbitLookInputActionMapIdXR");
            jumpInputActionMapIdXRProp = serializedObject.FindProperty("jumpInputActionMapIdXR");
            sprintInputActionMapIdXRProp = serializedObject.FindProperty("sprintInputActionMapIdXR");
            crouchInputActionMapIdXRProp = serializedObject.FindProperty("crouchInputActionMapIdXR");
            jetpackInputActionMapIdXRProp = serializedObject.FindProperty("jetpackInputActionMapIdXR");
            switchLookInputActionMapIdXRProp = serializedObject.FindProperty("switchLookInputActionMapIdXR");
            leftFire1InputActionMapIdXRProp = serializedObject.FindProperty("leftFire1InputActionMapIdXR");
            leftFire2InputActionMapIdXRProp = serializedObject.FindProperty("leftFire2InputActionMapIdXR");
            rightFire1InputActionMapIdXRProp = serializedObject.FindProperty("rightFire1InputActionMapIdXR");
            rightFire2InputActionMapIdXRProp = serializedObject.FindProperty("rightFire2InputActionMapIdXR");

            #endif
            #endregion

            #region Properties - Custom Input
            ciListProp = serializedObject.FindProperty("customInputList");
            ciShowInEditorProp = serializedObject.FindProperty("customInputsShowInEditor");
            ciIsListExpandedProp = serializedObject.FindProperty("isCustomInputListExpanded");
            #endregion

            stickyInputModule.ValidateLegacyInput();

            #if SSC_UIS
            uisPlayerInput = stickyInputModule.GetUISPlayerInput();
            uisVersion = StickySetup.GetPackageVersion("Packages/com.unity.inputsystem/package.json");
            UISRefreshActions();
            #endif

            #if SSC_REWIRED
            FindRewiredInputManager();
            // Populate arrays for rewired input options
            RWRefreshActionCategories();
            RWRefreshActionsAll();
            #endif

            #if SCSM_XR && SSC_UIS
            xrVersion = StickySetup.GetPackageVersion("Packages/com.unity.xr.management/package.json");
            openXRVersion = StickySetup.GetPackageVersion("Packages/com.unity.xr.openxr/package.json");
            oculusXRVersion = StickySetup.GetPackageVersion("Packages/com.unity.xr.oculus/package.json");
            if (stickyInputModule.inputMode == StickyInputModule.InputMode.UnityXR)
            {
                XRRefreshActionMaps();
                xrForceActionRefresh = true;
            }
            #endif

            RefreshAnimActionList();

            // Get a reference to the CharacterInput class instance used
            // to set data to the StickyControlModule. Used for debugging
            characterInput = stickyInputModule.GetCharacterInput;
            characterInputXR = stickyInputModule.GetCharacterInputXR;
            isRefreshing = false;
        }

        #endregion

        #region OnInspectorGUI
        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            #region Initialise
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
            #endregion

            #region Configure Buttons and Styles

            // Set up rich text GUIStyles
            if (!isStylesInitialised)
            {
                helpBoxRichText = new GUIStyle("HelpBox");
                helpBoxRichText.richText = true;

                labelFieldRichText = new GUIStyle("Label");
                labelFieldRichText.richText = true;

                headingFieldRichText = new GUIStyle(UnityEditor.EditorStyles.miniLabel);
                headingFieldRichText.richText = true;
                headingFieldRichText.normal.textColor = helpBoxRichText.normal.textColor;

                // Overide default styles
                EditorStyles.foldout.fontStyle = FontStyle.Bold;

                // When using a no-label foldout, don't forget to set the global
                // EditorGUIUtility.fieldWidth to a small value like 15, then back
                // to the original afterward.
                foldoutStyleNoLabel = new GUIStyle(EditorStyles.foldout);
                foldoutStyleNoLabel.fixedWidth = 0.01f;

                buttonCompact = new GUIStyle("Button");
                buttonCompact.fontSize = 10;

                isStylesInitialised = true;
            }

            #endregion

            // Read in all the properties
            serializedObject.Update();

            #region Header Info and Buttons
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            #endregion

            #region General Settings
            EditorGUILayout.PropertyField(initialiseOnAwakeProp, initialiseOnAwakeContent);
            EditorGUILayout.PropertyField(isEnabledOnInitialiseProp, isEnabledOnInitialiseContent);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(inputModeProp, inputModeContent, GUILayout.MaxWidth(300f));
            if (EditorGUI.EndChangeCheck())
            {
                #if SCSM_XR && SSC_UIS
                if (inputModeProp.intValue == (int)StickyInputModule.InputMode.UnityXR)
                {
                    xrForceActionRefresh = true;
                    XRRefreshActionMaps();
                }
                else if (inputModeProp.intValue == (int)StickyInputModule.InputMode.UnityInputSystem)
                {
                    UISRefreshActions();
                }
                #endif

                if (EditorApplication.isPlaying && stickyInputModule.IsInitialised)
                {
                    serializedObject.ApplyModifiedProperties();
                    // This is a little untested but "should" work.
                    stickyInputModule.Reinitalise();
                    serializedObject.Update();
                }
            }

            #endregion

            #region Direct Keyboard and mouse Settings

            if (inputModeProp.intValue == (int)StickyInputModule.InputMode.DirectKeyboard)
            {
                #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER

                #region Direct Keyboad - Horizontal Move
                DrawFoldoutWithLabel(hztlMoveShowInEditorProp, horizontalMoveHeaderContent);
                if (hztlMoveShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(posHztrlMoveInputKeycodeProp, positiveKeycodeContent);
                    EditorGUILayout.PropertyField(negHztrlMoveInputKeycodeProp, negativeKeycodeContent);
                    if (posHztrlMoveInputKeycodeProp.intValue == negHztrlMoveInputKeycodeProp.intValue
                        && posHztrlMoveInputKeycodeProp.intValue != 0)
                    {
                        EditorGUILayout.HelpBox(identicalKeycodesWarningContent, MessageType.Warning);
                    }
                }
                #endregion

                #region Direct Keyboad - Vertical Move
                DrawFoldoutWithLabel(vertMoveShowInEditorProp, verticalMoveHeaderContent);
                if (vertMoveShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(posVertMoveInputKeycodeProp, positiveKeycodeContent);
                    EditorGUILayout.PropertyField(negVertMoveInputKeycodeProp, negativeKeycodeContent);
                    if (posVertMoveInputKeycodeProp.intValue == negVertMoveInputKeycodeProp.intValue
                        && posVertMoveInputKeycodeProp.intValue != 0)
                    {
                        EditorGUILayout.HelpBox(identicalKeycodesWarningContent, MessageType.Warning);
                    }
                }
                #endregion
                
                #region Direct Keyboard - Sprint
                DrawFoldoutWithLabel(sprintShowInEditorProp, sprintHeaderContent);
                if (sprintShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(sprintInputKeycodeProp, singleKeycodeContent);
                }
                #endregion

                #region Direct Keyboard - Jump
                DrawDirectKeyboardBool(jumpShowInEditorProp, jumpHeaderContent, jumpInputKeycodeProp, jumpCanBeHeldProp);
                #endregion

                #region Direct Keyboard - Crouch
                DrawFoldoutWithLabel(crouchShowInEditorProp, crouchHeaderContent);
                if (crouchShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(crouchInputKeycodeProp, singleKeycodeContent);
                    EditorGUILayout.PropertyField(crouchCanBeHeldProp, canBeHeldContent);
                    EditorGUILayout.PropertyField(crouchIsToggledProp, crouchIsToggledContent);
                    EditorGUILayout.PropertyField(crouchIsJetPackOnlyProp, isJetPackOnlyContent);
                }
                #endregion

                #region Direct Keyboard - JetPack
                DrawFoldoutWithLabel(jetpackShowInEditorProp, jetpackHeaderContent);
                if (jetpackShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(jetpackInputKeycodeProp, singleKeycodeContent);
                }
                #endregion

                #region Direct Keyboard - Horizontal Look
                DrawFoldoutWithLabel(hztlLookShowInEditorProp, horizontalLookHeaderContent);
                if (hztlLookShowInEditorProp.boolValue)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(dkiMouseXContent);
                    GUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(hztlLookSensitivityDKIProp, dkiMouseXSensitivityContent);
                    EditorGUILayout.PropertyField(hztlLookDeadzoneDKIProp, dkiMouseXDeadzoneContent);
                    EditorGUILayout.PropertyField(isMouseCentreHztlLookDKIProp, dkiMouseXIsDistanceCentreContent);
                }
                #endregion

                #region Direct Keyboard - Vertical Look
                DrawFoldoutWithLabel(vertLookShowInEditorProp, verticalLookHeaderContent);
                if (vertLookShowInEditorProp.boolValue)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(dkiMouseYContent);
                    GUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(vertLookSensitivityDKIProp, dkiMouseYSensitivityContent);
                    EditorGUILayout.PropertyField(vertLookDeadzoneDKIProp, dkiMouseYDeadzoneContent);
                    EditorGUILayout.PropertyField(isMouseCentreVertLookDKIProp, dkiMouseYIsDistanceCentreContent);
                }
                #endregion

                #region Direct Keyboard - Switch look
                DrawFoldoutWithLabel(switchLookShowInEditorProp, switchLookHeaderContent);
                if (switchLookShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(switchLookInputKeycodeProp, singleKeycodeContent);
                }
                #endregion

                #region Direct Keyboard - Zoom Look
                DrawFoldoutWithLabel(zoomLookShowInEditorProp, zoomLookHeaderContent);
                if (zoomLookShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(posZoomInputKeycodeProp, positiveKeycodeContent);
                    EditorGUILayout.PropertyField(negZoomInputKeycodeProp, negativeKeycodeContent);
                    if (posZoomInputKeycodeProp.intValue == negZoomInputKeycodeProp.intValue
                        && posZoomInputKeycodeProp.intValue != 0)
                    {
                        EditorGUILayout.HelpBox(identicalKeycodesWarningContent, MessageType.Warning);
                    }

                    EditorGUILayout.PropertyField(isMouseScrollForZoomEnabledDKIProp, dkiMouseScrollWheelContent);
                }
                #endregion

                #region Direct Keyboard - Orbit Look
                DrawFoldoutWithLabel(orbitLookShowInEditorProp, orbitLookHeaderContent);
                if (orbitLookShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(posOrbitInputKeycodeProp, positiveKeycodeContent);
                    EditorGUILayout.PropertyField(negOrbitInputKeycodeProp, negativeKeycodeContent);
                    if (posOrbitInputKeycodeProp.intValue == negOrbitInputKeycodeProp.intValue
                        && posOrbitInputKeycodeProp.intValue != 0)
                    {
                        EditorGUILayout.HelpBox(identicalKeycodesWarningContent, MessageType.Warning);
                    }
                }
                #endregion

                #region Direct Keyboard - Left Fire 1
                DrawDirectKeyboardBool(leftFire1ShowInEditorProp, leftFire1HeaderContent, leftFire1InputKeycodeProp, leftFire1CanBeHeldProp);
                #endregion

                #region Direct Keyboard - Left Fire 2
                DrawDirectKeyboardBool(leftFire2ShowInEditorProp, leftFire2HeaderContent, leftFire2InputKeycodeProp, leftFire2CanBeHeldProp);
                #endregion

                #region Direct Keyboard - Right Fire 1
                DrawDirectKeyboardBool(rightFire1ShowInEditorProp, rightFire1HeaderContent, rightFire1InputKeycodeProp, rightFire1CanBeHeldProp);
                #endregion

                #region Direct Keyboard - Right Fire 2
                DrawDirectKeyboardBool(rightFire2ShowInEditorProp, rightFire2HeaderContent, rightFire2InputKeycodeProp, rightFire2CanBeHeldProp);
                #endregion

                #else
                EditorGUILayout.HelpBox("Legacy Unity Input is not enabled in the project.", MessageType.Warning, true);
                #endif
            }

            #endregion

            #region Legacy Unity Input System

            else if (inputModeProp.intValue == (int)StickyInputModule.InputMode.LegacyUnity)
            {
                #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER

                #region Horizontal Move
                DrawFoldoutWithLabel(hztlMoveShowInEditorProp, horizontalMoveHeaderContent);
                if (hztlMoveShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(hztlMoveInputAxisModeProp, inputAxisModeContent);
                    if (hztlMoveInputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.NoInput) { }
                    else if (hztlMoveInputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.SingleAxis)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(posHztlMoveInputAxisNameProp, singleAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPosHztlMoveInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(posHztlMoveInputAxisNameProp.stringValue, false);
                        }
                        if (!isPosHztlMoveInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }
                        EditorGUILayout.PropertyField(invertHztlMoveInputAxisProp, invertInputAxisContent);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(posHztlMoveInputAxisNameProp, positiveAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPosHztlMoveInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(posHztlMoveInputAxisNameProp.stringValue, false);
                        }
                        if (!isPosHztlMoveInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(negHztlMoveInputAxisNameProp, negativeAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isNegHztlMoveInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(negHztlMoveInputAxisNameProp.stringValue, false);
                        }
                        if (!isNegHztlMoveInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        if (posHztlMoveInputAxisNameProp.stringValue == negHztlMoveInputAxisNameProp.stringValue)
                        {
                            EditorGUILayout.HelpBox(identicalAxisNamesWarningContent, MessageType.Warning);
                        }
                    }
                }
                #endregion

                #region Vertical Move
                DrawFoldoutWithLabel(vertMoveShowInEditorProp, verticalMoveHeaderContent);
                if (vertMoveShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(vertMoveInputAxisModeProp, inputAxisModeContent);
                    if (vertMoveInputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.NoInput) { }
                    else if (vertMoveInputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.SingleAxis)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(posVertMoveInputAxisNameProp, singleAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPosVertMoveInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(posVertMoveInputAxisNameProp.stringValue, false);
                        }
                        if (!isPosVertMoveInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }
                        EditorGUILayout.PropertyField(invertVertMoveInputAxisProp, invertInputAxisContent);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(posVertMoveInputAxisNameProp, positiveAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPosVertMoveInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(posVertMoveInputAxisNameProp.stringValue, false);
                        }
                        if (!isPosVertMoveInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(negVertMoveInputAxisNameProp, negativeAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isNegVertMoveInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(negVertMoveInputAxisNameProp.stringValue, false);
                        }
                        if (!isNegVertMoveInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        if (posVertMoveInputAxisNameProp.stringValue == negVertMoveInputAxisNameProp.stringValue)
                        {
                            EditorGUILayout.HelpBox(identicalAxisNamesWarningContent, MessageType.Warning);
                        }
                    }
                }
                #endregion

                #region Sprint
                DrawFoldoutWithLabel(sprintShowInEditorProp, sprintHeaderContent);
                if (sprintShowInEditorProp.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(sprintInputAxisNameProp, singleAxisNameContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        isSprintInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(sprintInputAxisNameProp.stringValue, false);
                    }
                    if (!isSprintInputAxisValidProp.boolValue)
                    {
                        EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                    }
                }
                #endregion

                #region Jump

                DrawLegacyBool(jumpShowInEditorProp, jumpHeaderContent, jumpInputAxisNameProp, isJumpInputAxisValidProp, jumpCanBeHeldProp);

                //DrawFoldoutWithLabel(jumpShowInEditorProp, jumpHeaderContent);
                //if (jumpShowInEditorProp.boolValue)
                //{
                //    EditorGUI.BeginChangeCheck();
                //    EditorGUILayout.PropertyField(jumpInputAxisNameProp, singleAxisNameContent);
                //    EditorGUILayout.PropertyField(jumpCanBeHeldProp, canBeHeldContent);
                //    if (EditorGUI.EndChangeCheck())
                //    {
                //        isJumpInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(jumpInputAxisNameProp.stringValue, false);
                //    }
                //    if (!isJumpInputAxisValidProp.boolValue)
                //    {
                //        EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                //    }
                //}
                #endregion

                #region Crouch
                DrawFoldoutWithLabel(crouchShowInEditorProp, crouchHeaderContent);
                if (crouchShowInEditorProp.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(crouchInputAxisNameProp, singleAxisNameContent);
                    EditorGUILayout.PropertyField(crouchCanBeHeldProp, canBeHeldContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        isCrouchInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(crouchInputAxisNameProp.stringValue, false);
                    }
                    if (!isCrouchInputAxisValidProp.boolValue)
                    {
                        EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                    }
                    EditorGUILayout.PropertyField(crouchIsToggledProp, crouchIsToggledContent);
                    EditorGUILayout.PropertyField(crouchIsJetPackOnlyProp, isJetPackOnlyContent);
                }
                #endregion

                #region Jet Pack
                DrawFoldoutWithLabel(jetpackShowInEditorProp, jetpackHeaderContent);
                if (jetpackShowInEditorProp.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(jetpackInputAxisNameProp, singleAxisNameContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        isJetPackInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(jetpackInputAxisNameProp.stringValue, false);
                    }
                    if (!isJetPackInputAxisValidProp.boolValue)
                    {
                        EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                    }
                }
                #endregion

                #region Horizontal Look
                DrawFoldoutWithLabel(hztlLookShowInEditorProp, horizontalLookHeaderContent);
                if (hztlLookShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(hztlLookInputAxisModeProp, inputAxisModeContent);
                    if (hztlLookInputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.NoInput) { }
                    else if (hztlLookInputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.SingleAxis)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(posHztlLookInputAxisNameProp, singleAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPosHztlLookInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(posHztlLookInputAxisNameProp.stringValue, false);
                        }
                        if (!isPosHztlLookInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }
                        EditorGUILayout.PropertyField(invertHztlLookInputAxisProp, invertInputAxisContent);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(posHztlLookInputAxisNameProp, positiveAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPosHztlLookInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(posHztlLookInputAxisNameProp.stringValue, false);
                        }
                        if (!isPosHztlLookInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(negHztlLookInputAxisNameProp, negativeAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isNegHztlLookInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(negHztlLookInputAxisNameProp.stringValue, false);
                        }
                        if (!isNegHztlLookInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        if (posHztlLookInputAxisNameProp.stringValue == negHztlLookInputAxisNameProp.stringValue)
                        {
                            EditorGUILayout.HelpBox(identicalAxisNamesWarningContent, MessageType.Warning);
                        }
                    }
                }
                #endregion

                #region Vertical Look
                DrawFoldoutWithLabel(vertLookShowInEditorProp, verticalLookHeaderContent);
                if (vertLookShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(vertLookInputAxisModeProp, inputAxisModeContent);
                    if (vertLookInputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.NoInput) { }
                    else if (vertLookInputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.SingleAxis)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(posVertLookInputAxisNameProp, singleAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPosVertLookInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(posVertLookInputAxisNameProp.stringValue, false);
                        }
                        if (!isPosVertLookInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }
                        EditorGUILayout.PropertyField(invertVertLookInputAxisProp, invertInputAxisContent);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(posVertLookInputAxisNameProp, positiveAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPosVertLookInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(posVertLookInputAxisNameProp.stringValue, false);
                        }
                        if (!isPosVertLookInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(negVertLookInputAxisNameProp, negativeAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isNegVertLookInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(negVertLookInputAxisNameProp.stringValue, false);
                        }
                        if (!isNegVertLookInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        if (posVertLookInputAxisNameProp.stringValue == negVertLookInputAxisNameProp.stringValue)
                        {
                            EditorGUILayout.HelpBox(identicalAxisNamesWarningContent, MessageType.Warning);
                        }
                    }
                }
                #endregion

                #region Switch Look
                DrawFoldoutWithLabel(switchLookShowInEditorProp, switchLookHeaderContent);
                if (switchLookShowInEditorProp.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(switchLookInputAxisNameProp, singleAxisNameContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        isSwitchLookInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(switchLookInputAxisNameProp.stringValue, false);
                    }
                    if (!isSwitchLookInputAxisValidProp.boolValue)
                    {
                        EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                    }
                }
                #endregion

                #region Zoom Look
                DrawFoldoutWithLabel(zoomLookShowInEditorProp, zoomLookHeaderContent);
                if (zoomLookShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(zoomLookInputAxisModeProp, inputAxisModeContent);
                    if (zoomLookInputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.NoInput) { }
                    else if (zoomLookInputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.SingleAxis)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(posZoomLookInputAxisNameProp, singleAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPosZoomLookInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(posZoomLookInputAxisNameProp.stringValue, false);
                        }
                        if (!isPosZoomLookInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }
                        EditorGUILayout.PropertyField(invertZoomLookInputAxisProp, invertInputAxisContent);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(posZoomLookInputAxisNameProp, positiveAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPosZoomLookInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(posZoomLookInputAxisNameProp.stringValue, false);
                        }
                        if (!isPosZoomLookInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(negZoomLookInputAxisNameProp, negativeAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isNegZoomLookInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(negZoomLookInputAxisNameProp.stringValue, false);
                        }
                        if (!isNegZoomLookInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        if (posZoomLookInputAxisNameProp.stringValue == negZoomLookInputAxisNameProp.stringValue)
                        {
                            EditorGUILayout.HelpBox(identicalAxisNamesWarningContent, MessageType.Warning);
                        }
                    }
                }
                #endregion

                #region Orbit Look
                DrawFoldoutWithLabel(orbitLookShowInEditorProp, orbitLookHeaderContent);
                if (orbitLookShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(orbitLookInputAxisModeProp, inputAxisModeContent);
                    if (orbitLookInputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.NoInput) { }
                    else if (orbitLookInputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.SingleAxis)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(posOrbitLookInputAxisNameProp, singleAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPosOrbitLookInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(posOrbitLookInputAxisNameProp.stringValue, false);
                        }
                        if (!isPosOrbitLookInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }
                        EditorGUILayout.PropertyField(invertOrbitLookInputAxisProp, invertInputAxisContent);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(posOrbitLookInputAxisNameProp, positiveAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPosOrbitLookInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(posOrbitLookInputAxisNameProp.stringValue, false);
                        }
                        if (!isPosOrbitLookInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(negOrbitLookInputAxisNameProp, negativeAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isNegOrbitLookInputAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(negOrbitLookInputAxisNameProp.stringValue, false);
                        }
                        if (!isNegOrbitLookInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        if (posOrbitLookInputAxisNameProp.stringValue == negOrbitLookInputAxisNameProp.stringValue)
                        {
                            EditorGUILayout.HelpBox(identicalAxisNamesWarningContent, MessageType.Warning);
                        }
                    }
                }
                #endregion

                #region Left Fire 1
                DrawLegacyBool(leftFire1ShowInEditorProp, leftFire1HeaderContent, leftFire1InputAxisNameProp, isLeftFire1InputAxisValidProp, leftFire1CanBeHeldProp);
                #endregion

                #region Left Fire 2
                DrawLegacyBool(leftFire2ShowInEditorProp, leftFire2HeaderContent, leftFire2InputAxisNameProp, isLeftFire2InputAxisValidProp, leftFire2CanBeHeldProp);
                #endregion

                #region Right Fire 1
                DrawLegacyBool(rightFire1ShowInEditorProp, rightFire1HeaderContent, rightFire1InputAxisNameProp, isRightFire1InputAxisValidProp, rightFire1CanBeHeldProp);
                #endregion

                #region Right Fire 2
                DrawLegacyBool(rightFire2ShowInEditorProp, rightFire2HeaderContent, rightFire2InputAxisNameProp, isRightFire2InputAxisValidProp, rightFire2CanBeHeldProp);
                #endregion

                #else
                EditorGUILayout.HelpBox("Legacy Unity Input is not enabled in the project.", MessageType.Warning, true);
                #endif
            }
            #endregion

            #region Unity Input System (add with Unity package manager)
            else if (inputModeProp.intValue == (int)StickyInputModule.InputMode.UnityInputSystem)
            {
                #if SSC_UIS
                
                if (uisPlayerInput != null)
                {
                    EditorGUILayout.HelpBox("Setup the attached Player Input component, refresh the Available Actions, then configure the Input Axis as required.", MessageType.Info, true);
                }
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(uisVersionContent, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(uisVersion);
                EditorGUILayout.EndHorizontal(); 

                if (uisPlayerInput == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("The Unity Input System Player Input component is missing.", MessageType.Error, true);
                    if (GUILayout.Button("Fix Now", GUILayout.MaxWidth(70f), GUILayout.MaxHeight(35f)) )
                    {
                        Undo.AddComponent(stickyInputModule.gameObject, typeof(UnityEngine.InputSystem.PlayerInput));
                        // Ensure the StickyInputModule reference is updated for this PlayerInput component
                        uisPlayerInput = stickyInputModule.GetUISPlayerInput();
                        if (uisPlayerInput != null) { uisPlayerInput.notificationBehavior = UnityEngine.InputSystem.PlayerNotifications.InvokeUnityEvents; }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else if (uisPlayerInput.actions == null)
                {
                    EditorGUILayout.HelpBox("The Unity Input System Player Input component requires an Input Action Asset.", MessageType.Error, true);
                }
                else if (uisPlayerInput.notificationBehavior != UnityEngine.InputSystem.PlayerNotifications.InvokeUnityEvents)
                {
                    EditorGUILayout.HelpBox("This version of Sticky3D Controller only supports a Behaviour of Invoke Unity Events on the Player Input component", MessageType.Error, true);
                }
                else if (string.IsNullOrEmpty(uisPlayerInput.defaultActionMap))
                {
                    EditorGUILayout.HelpBox("The Unity Input System Player Input component requires a Default Map", MessageType.Error, true);
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(" Available Actions", GUILayout.Width(defaultEditorLabelWidth));
                    if (GUILayout.Button("Refresh", GUILayout.MaxWidth(70f))) { UISRefreshActions(); }
                    if (GUILayout.Button(uisAddActionsContent, GUILayout.MaxWidth(90f)))
                    {
                        #region Add Default Events
                        if (UISAddDefaultEvents())
                        {
                            UISRefreshActions();

                            hztlMoveInputAxisModeProp.intValue = (int)StickyInputModule.InputAxisMode.SingleAxis;
                            vertMoveInputAxisModeProp.intValue = (int)StickyInputModule.InputAxisMode.SingleAxis;
                            hztlLookInputAxisModeProp.intValue = (int)StickyInputModule.InputAxisMode.SingleAxis;
                            vertLookInputAxisModeProp.intValue = (int)StickyInputModule.InputAxisMode.SingleAxis;
                            zoomLookInputAxisModeProp.intValue = (int)StickyInputModule.InputAxisMode.SingleAxis;
                            orbitLookInputAxisModeProp.intValue = (int)StickyInputModule.InputAxisMode.SingleAxis;
                            jumpButtonEnabledProp.intValue = (int)StickyInputModule.InputAxisMode.SingleAxis;
                            sprintButtonEnabledProp.intValue = (int)StickyInputModule.InputAxisMode.SingleAxis;
                            crouchButtonEnabledProp.intValue = (int)StickyInputModule.InputAxisMode.SingleAxis;
                            jetpackButtonEnabledProp.intValue = (int)StickyInputModule.InputAxisMode.SingleAxis;
                            switchLookButtonEnabledProp.intValue = (int)StickyInputModule.InputAxisMode.SingleAxis;
                            leftFire1ButtonEnabledProp.intValue = (int)StickyInputModule.InputAxisMode.NoInput;
                            leftFire2ButtonEnabledProp.intValue = (int)StickyInputModule.InputAxisMode.NoInput;
                            rightFire1ButtonEnabledProp.intValue = (int)StickyInputModule.InputAxisMode.NoInput;
                            rightFire2ButtonEnabledProp.intValue = (int)StickyInputModule.InputAxisMode.NoInput;

                            isMouseForHztlLookEnabledUISProp.boolValue = false;
                            isMouseForVertLookEnabledUISProp.boolValue = false;

                            UISSetActionId("HorizontalMove", 0, posHztlMoveInputActionIdUISProp, posHztlMoveInputActionDataSlotUISProp);
                            UISSetActionId("VerticalMove", 0, posVertMoveInputActionIdUISProp, posVertMoveInputActionDataSlotUISProp);
                            UISSetActionId("HorizontalLook", 0, posHztlLookInputActionIdUISProp, posHztlLookInputActionDataSlotUISProp);
                            UISSetActionId("VerticalLook", 0, posVertLookInputActionIdUISProp, posVertLookInputActionDataSlotUISProp);
                            UISSetActionId("ZoomLook", 0, posZoomLookInputActionIdUISProp, posZoomLookInputActionDataSlotUISProp);
                            UISSetActionId("OrbitLook", 0, posOrbitLookInputActionIdUISProp, posOrbitLookInputActionDataSlotUISProp);
                            UISSetActionId("Jump", 0, jumpInputActionIdUISProp, jumpInputActionDataSlotUISProp);
                            UISSetActionId("Sprint", 0, sprintInputActionIdUISProp, sprintInputActionDataSlotUISProp);
                            UISSetActionId("Crouch", 0, crouchInputActionIdUISProp, crouchInputActionDataSlotUISProp);
                            UISSetActionId("JetPack", 0, jetpackInputActionIdUISProp, jetpackInputActionDataSlotUISProp);
                            UISSetActionId("SwitchLook", 0, switchLookInputActionIdUISProp, switchLookInputActionDataSlotUISProp);
                        }
                        else { UISRefreshActions(); }
                        #endregion
                        
                        serializedObject.ApplyModifiedProperties();
                        GUIUtility.ExitGUI();
                    }
                    EditorGUILayout.EndHorizontal();

                    // The array of actions can become null when all actions are manually deleted from the default map
                    // using the UIS asset editor
                    if (uisActionGUIContent == null) { UISRefreshActions(); }

                    int numActionIDs = uisActionIDs == null ? 0 : uisActionIDs.Length;

                    #region Horizontal Move Input
                    DrawFoldoutWithLabel(hztlMoveShowInEditorProp, horizontalMoveHeaderContent);
                    if (hztlMoveShowInEditorProp.boolValue)
                    {
                        DrawUnityInputSystem(hztlMoveInputAxisModeProp, inputSystemAxisModeContent,
                        posHztlMoveInputActionIdUISProp, posHztlMoveInputActionDataSlotUISProp,
                        uisHztlMoveInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionIDs, true
                        );
                    }
                    #endregion

                    #region Vertical Move Input
                    DrawFoldoutWithLabel(vertMoveShowInEditorProp, verticalMoveHeaderContent);
                    if (vertMoveShowInEditorProp.boolValue)
                    {
                        DrawUnityInputSystem(vertMoveInputAxisModeProp, inputSystemAxisModeContent,
                        posVertMoveInputActionIdUISProp, posVertMoveInputActionDataSlotUISProp,
                        uisVertMoveInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionIDs, true
                        );
                    }
                    #endregion

                    #region Sprint Input
                    DrawFoldoutWithLabel(sprintShowInEditorProp, sprintHeaderContent);
                    if (sprintShowInEditorProp.boolValue)
                    {
                        DrawUnityInputButton(sprintButtonEnabledProp, uisSprintEnabledContent,
                        sprintInputActionIdUISProp, sprintInputActionDataSlotUISProp,
                        null, uisSprintInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        null, uisDataSlots2, uisDataSlots3, numActionIDs
                        );
                    }
                    #endregion

                    #region Jump Input
                    DrawFoldoutWithLabel(jumpShowInEditorProp, jumpHeaderContent);
                    if (jumpShowInEditorProp.boolValue)
                    {
                        DrawUnityInputButton(jumpButtonEnabledProp, uisJumpEnabledContent,
                        jumpInputActionIdUISProp, jumpInputActionDataSlotUISProp,
                        jumpCanBeHeldProp, uisJumpInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        canBeHeldContent, uisDataSlots2, uisDataSlots3, numActionIDs
                        );
                    }
                    #endregion

                    #region Crouch Input
                    DrawFoldoutWithLabel(crouchShowInEditorProp, crouchHeaderContent);
                    if (crouchShowInEditorProp.boolValue)
                    {
                        DrawUnityInputButton(crouchButtonEnabledProp, uisCrouchEnabledContent,
                        crouchInputActionIdUISProp, crouchInputActionDataSlotUISProp,
                        crouchCanBeHeldProp, uisCrouchInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        canBeHeldContent, uisDataSlots2, uisDataSlots3, numActionIDs
                        );
                        if (crouchButtonEnabledProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(crouchIsToggledProp, crouchIsToggledContent);
                            EditorGUILayout.PropertyField(crouchIsJetPackOnlyProp, isJetPackOnlyContent);
                        }
                    }
                    #endregion

                    #region JetPack Input
                    DrawFoldoutWithLabel(jetpackShowInEditorProp, jetpackHeaderContent);
                    if (jetpackShowInEditorProp.boolValue)
                    {
                        DrawUnityInputButton(jetpackButtonEnabledProp, uisJetPackEnabledContent,
                        jetpackInputActionIdUISProp, jetpackInputActionDataSlotUISProp,
                        null, uisJetPackInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        null, uisDataSlots2, uisDataSlots3, numActionIDs
                        );
                    }
                    #endregion

                    #region Horizontal Look Input
                    DrawFoldoutWithLabel(hztlLookShowInEditorProp, horizontalLookHeaderContent);
                    if (hztlLookShowInEditorProp.boolValue)
                    {
                        DrawUnityInputSystem(hztlLookInputAxisModeProp, inputSystemAxisModeContent,
                        posHztlLookInputActionIdUISProp, posHztlLookInputActionDataSlotUISProp,
                        uisHztlLookInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionIDs, true
                        );
                    }
                    #endregion

                    #region Vertical Look Input
                    DrawFoldoutWithLabel(vertLookShowInEditorProp, verticalLookHeaderContent);
                    if (vertLookShowInEditorProp.boolValue)
                    {
                        DrawUnityInputSystem(vertLookInputAxisModeProp, inputSystemAxisModeContent,
                        posVertLookInputActionIdUISProp, posVertLookInputActionDataSlotUISProp,
                        uisVertLookInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionIDs, true
                        );
                    }
                    #endregion

                    #region Switch Look Input
                    DrawFoldoutWithLabel(switchLookShowInEditorProp, switchLookHeaderContent);
                    if (switchLookShowInEditorProp.boolValue)
                    {
                        DrawUnityInputButton(switchLookButtonEnabledProp, uisSwitchLookEnabledContent,
                        switchLookInputActionIdUISProp, switchLookInputActionDataSlotUISProp,
                        null, uisSwitchLookInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        null, uisDataSlots2, uisDataSlots3, numActionIDs
                        );
                    }
                    #endregion

                    #region Zoom Look Input
                    DrawFoldoutWithLabel(zoomLookShowInEditorProp, zoomLookHeaderContent);
                    if (zoomLookShowInEditorProp.boolValue)
                    {
                        DrawUnityInputSystem(zoomLookInputAxisModeProp, inputSystemAxisModeContent,
                        posZoomLookInputActionIdUISProp, posZoomLookInputActionDataSlotUISProp,
                        uisZoomLookInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionIDs, true
                        );
                    }
                    #endregion

                    #region Orbit Look Input
                    DrawFoldoutWithLabel(orbitLookShowInEditorProp, orbitLookHeaderContent);
                    if (orbitLookShowInEditorProp.boolValue)
                    {
                        DrawUnityInputSystem(orbitLookInputAxisModeProp, inputSystemAxisModeContent,
                        posOrbitLookInputActionIdUISProp, posOrbitLookInputActionDataSlotUISProp,
                        uisOrbitLookInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionIDs, true
                        );
                    }
                    #endregion

                    #region Left Fire 1 Input
                    DrawFoldoutWithLabel(leftFire1ShowInEditorProp, leftFire1HeaderContent);
                    if (leftFire1ShowInEditorProp.boolValue)
                    {
                        DrawUnityInputButton(leftFire1ButtonEnabledProp, uisLeftFire1EnabledContent,
                        leftFire1InputActionIdUISProp, leftFire1InputActionDataSlotUISProp,
                        leftFire1CanBeHeldProp, uisLeftFire1InputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        canBeHeldContent, uisDataSlots2, uisDataSlots3, numActionIDs
                        );
                    }
                    #endregion

                    #region Left Fire 2 Input
                    DrawFoldoutWithLabel(leftFire2ShowInEditorProp, leftFire2HeaderContent);
                    if (leftFire2ShowInEditorProp.boolValue)
                    {
                        DrawUnityInputButton(leftFire2ButtonEnabledProp, uisLeftFire2EnabledContent,
                        leftFire2InputActionIdUISProp, leftFire2InputActionDataSlotUISProp,
                        leftFire2CanBeHeldProp, uisLeftFire2InputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        canBeHeldContent, uisDataSlots2, uisDataSlots3, numActionIDs
                        );
                    }
                    #endregion

                    #region Right Fire 1 Input
                    DrawFoldoutWithLabel(rightFire1ShowInEditorProp, rightFire1HeaderContent);
                    if (rightFire1ShowInEditorProp.boolValue)
                    {
                        DrawUnityInputButton(rightFire1ButtonEnabledProp, uisRightFire1EnabledContent,
                        rightFire1InputActionIdUISProp, rightFire1InputActionDataSlotUISProp,
                        rightFire1CanBeHeldProp, uisRightFire1InputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        canBeHeldContent, uisDataSlots2, uisDataSlots3, numActionIDs
                        );
                    }
                    #endregion

                    #region Right Fire 2 Input
                    DrawFoldoutWithLabel(rightFire2ShowInEditorProp, rightFire2HeaderContent);
                    if (rightFire2ShowInEditorProp.boolValue)
                    {
                        DrawUnityInputButton(rightFire2ButtonEnabledProp, uisRightFire2EnabledContent,
                        rightFire2InputActionIdUISProp, rightFire2InputActionDataSlotUISProp,
                        rightFire2CanBeHeldProp, uisRightFire2InputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        canBeHeldContent, uisDataSlots2, uisDataSlots3, numActionIDs
                        );
                    }
                    #endregion
                }

                #else
                #if !UNITY_2019_1_OR_NEWER
                EditorGUILayout.HelpBox("Unity Input System requires Unity 2019.1+ and Input System 1.0.0 or newer", MessageType.Error, true);
                #else
                EditorGUILayout.HelpBox("Could not find the Unity Input System. Did you install Input System 1.0.0 or newer from the Package Manager?", MessageType.Warning, true);
                #endif
                #endif
            }
            #endregion

            #region Rewired Input
            else if (inputModeProp.intValue == (int)StickyInputModule.InputMode.Rewired && !isRefreshing)
            #if SSC_REWIRED
            {
                EditorGUILayout.HelpBox("Set up Actions in the Rewired Input Manager and then configure them below.", MessageType.Info, true);

                // NOTE: Cannot add an ActionCategory and any Action in code (it is not supported by Rewired)
                // The user must manually configure them in the Rewired Input Manager

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(rwVersionContent, GUILayout.Width(defaultEditorLabelWidth-3f));
                EditorGUILayout.LabelField(rwVersion);
                EditorGUILayout.EndHorizontal(); 

                if (rewiredUserData != null)
                {
                    int numActionCategories = actionCategoryIDs == null ? 0 : actionCategoryIDs.Length;

                    #region Rewired Player
                    EditorGUILayout.PropertyField(rewiredPlayerNumberProp, rwPlayerNumberContent);
                    #endregion

                    #region Refresh
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("", GUILayout.Width(1f));
                    if (GUILayout.Button(new GUIContent("Refresh"), GUILayout.MaxWidth(90f)))
                    {
                        isRefreshing = true;
                        // Populate arrays for rewired input options
                        RWRefreshActionCategories();
                        RWRefreshActionsAll();
                        isRefreshing = false;
                        EditorGUIUtility.ExitGUI();
                        return;
                    }
                    EditorGUILayout.EndHorizontal();
                    #endregion

                    #region Rewired - Horizontal Move Input
                    DrawFoldoutWithLabel(hztlMoveShowInEditorProp, horizontalMoveHeaderContent);
                    if (hztlMoveShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(PIMCategory.HorizontalMove, hztlMoveInputAxisModeProp, posHztlMoveInputActionIdRWDProp, negHztlMoveInputActionIdRWDProp,
                                         rwHztlMoveInputACPositiveContent, rwHztlMoveInputACNegativeContent, rwHztlMoveInputACContent,
                                         rwHztlMoveInputAIPositiveContent, rwHztlMoveInputAINegativeContent, rwHztlMoveInputAIContent,
                                         ref actionIDsHztlMovePositive, ref actionIDsHztlMoveNegative, ref ActionGUIContentHztlMovePositive, ref ActionGUIContentHztlMoveNegative, numActionCategories);

                        // DrawSensitivity(hztlMoveInputAxisModeProp,
                        // isSensitivityForHztlMoveEnabledProp, sensitivityEnabledContent,
                        // hztlMoveAxisSensitivityProp, sensitivityContent,
                        // hztlMoveAxisGravityProp, gravityContent);
                    }
                    #endregion

                    #region Rewired - Vertical Move Input
                    DrawFoldoutWithLabel(vertMoveShowInEditorProp, verticalMoveHeaderContent);
                    if (vertMoveShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(PIMCategory.VerticalMove, vertMoveInputAxisModeProp, posVertMoveInputActionIdRWDProp, negVertMoveInputActionIdRWDProp,
                                         rwVertMoveInputACPositiveContent, rwVertMoveInputACNegativeContent, rwVertMoveInputACContent,
                                         rwVertMoveInputAIPositiveContent, rwVertMoveInputAINegativeContent, rwVertMoveInputAIContent,
                                         ref actionIDsVertMovePositive, ref actionIDsVertMoveNegative, ref ActionGUIContentVertMovePositive, ref ActionGUIContentVertMoveNegative, numActionCategories);

                        // DrawSensitivity(vertMoveInputAxisModeProp,
                        // isSensitivityForVertMoveEnabledProp, sensitivityEnabledContent,
                        // vertMoveAxisSensitivityProp, sensitivityContent,
                        // vertMoveAxisGravityProp, gravityContent);
                    }
                    #endregion

                    #region Rewired - Sprint
                    DrawFoldoutWithLabel(sprintShowInEditorProp, sprintHeaderContent);
                    if (sprintShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(sprintButtonEnabledProp, sprintInputActionIdRWDProp, null, rwSprintEnabledContent, rwSprintInputACContent, rwSprintInputAIContent, null, ref actionIDsSprint, ref ActionGUIContentSprint, numActionCategories);
                    }
                    #endregion

                    #region Rewired - Jump
                    DrawFoldoutWithLabel(jumpShowInEditorProp, jumpHeaderContent);
                    if (jumpShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(jumpButtonEnabledProp, jumpInputActionIdRWDProp, jumpCanBeHeldProp, rwJumpEnabledContent, rwJumpInputACContent, rwJumpInputAIContent, canBeHeldContent, ref actionIDsJump, ref ActionGUIContentJump, numActionCategories);
                    }
                    #endregion

                    #region Rewired - Crouch
                    DrawFoldoutWithLabel(crouchShowInEditorProp, crouchHeaderContent);
                    if (crouchShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(crouchButtonEnabledProp, crouchInputActionIdRWDProp, crouchCanBeHeldProp, rwCrouchEnabledContent, rwCrouchInputACContent, rwCrouchInputAIContent, canBeHeldContent, ref actionIDsCrouch, ref ActionGUIContentCrouch, numActionCategories);
                        if (crouchButtonEnabledProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(crouchIsToggledProp, crouchIsToggledContent);
                            EditorGUILayout.PropertyField(crouchIsJetPackOnlyProp, isJetPackOnlyContent);
                        }
                    }
                    #endregion

                    #region Rewired - Jet Pack
                    DrawFoldoutWithLabel(jetpackShowInEditorProp, jetpackHeaderContent);
                    if (jetpackShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(jetpackButtonEnabledProp, jetpackInputActionIdRWDProp, null, rwJetPackEnabledContent, rwJetPackInputACContent, rwJetPackInputAIContent, null, ref actionIDsJetPack, ref ActionGUIContentJetPack, numActionCategories);
                    }
                    #endregion

                    #region Rewired - Horizontal Look Input
                    DrawFoldoutWithLabel(hztlLookShowInEditorProp, horizontalLookHeaderContent);
                    if (hztlLookShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(PIMCategory.HorizontalLook, hztlLookInputAxisModeProp, posHztlLookInputActionIdRWDProp, negHztlLookInputActionIdRWDProp,
                                         rwHztlLookInputACPositiveContent, rwHztlLookInputACNegativeContent, rwHztlLookInputACContent,
                                         rwHztlLookInputAIPositiveContent, rwHztlLookInputAINegativeContent, rwHztlLookInputAIContent,
                                         ref actionIDsHztlLookPositive, ref actionIDsHztlLookNegative, ref ActionGUIContentHztlLookPositive, ref ActionGUIContentHztlLookNegative, numActionCategories);

                        // DrawSensitivity(hztlLookInputAxisModeProp,
                        // isSensitivityForHztlLookEnabledProp, sensitivityEnabledContent,
                        // hztlLookAxisSensitivityProp, sensitivityContent,
                        // hztlLookAxisGravityProp, gravityContent);
                    }
                    #endregion

                    #region Rewired - Vertical Look Input
                    DrawFoldoutWithLabel(vertLookShowInEditorProp, verticalLookHeaderContent);
                    if (vertLookShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(PIMCategory.VerticalLook, vertLookInputAxisModeProp, posVertLookInputActionIdRWDProp, negVertLookInputActionIdRWDProp,
                                         rwVertLookInputACPositiveContent, rwVertLookInputACNegativeContent, rwVertLookInputACContent,
                                         rwVertLookInputAIPositiveContent, rwVertLookInputAINegativeContent, rwVertLookInputAIContent,
                                         ref actionIDsVertLookPositive, ref actionIDsVertLookNegative, ref ActionGUIContentVertLookPositive, ref ActionGUIContentVertLookNegative, numActionCategories);

                        // DrawSensitivity(vertLookInputAxisModeProp,
                        // isSensitivityForVertLookEnabledProp, sensitivityEnabledContent,
                        // vertLookAxisSensitivityProp, sensitivityContent,
                        // vertLookAxisGravityProp, gravityContent);
                    }
                    #endregion

                    #region Rewired - Switch Look
                    DrawFoldoutWithLabel(switchLookShowInEditorProp, switchLookHeaderContent);
                    if (switchLookShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(switchLookButtonEnabledProp, switchLookInputActionIdRWDProp, null, rwSwitchLookEnabledContent, rwSwitchLookInputACContent, rwSwitchLookInputAIContent, null, ref actionIDsSwitchLook, ref ActionGUIContentSwitchLook, numActionCategories);
                    }
                    #endregion

                    #region Rewired - Zoom Look
                    DrawFoldoutWithLabel(zoomLookShowInEditorProp, zoomLookHeaderContent);
                    if (zoomLookShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(PIMCategory.ZoomLook, zoomLookInputAxisModeProp, posZoomLookInputActionIdRWDProp, negZoomLookInputActionIdRWDProp,
                                         rwZoomLookInputACPositiveContent, rwZoomLookInputACNegativeContent, rwZoomLookInputACContent,
                                         rwZoomLookInputAIPositiveContent, rwZoomLookInputAINegativeContent, rwZoomLookInputAIContent,
                                         ref actionIDsZoomLookPositive, ref actionIDsZoomLookNegative, ref ActionGUIContentZoomLookPositive, ref ActionGUIContentZoomLookNegative, numActionCategories);

                        // DrawSensitivity(zoomLookInputAxisModeProp,
                        // isSensitivityForZoomLookEnabledProp, sensitivityEnabledContent,
                        // zoomLookAxisSensitivityProp, sensitivityContent,
                        // zoomLookAxisGravityProp, gravityContent);
                    }
                    #endregion

                    #region Rewired - Orbit Look
                    DrawFoldoutWithLabel(orbitLookShowInEditorProp, orbitLookHeaderContent);
                    if (orbitLookShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(PIMCategory.OrbitLook, orbitLookInputAxisModeProp, posOrbitLookInputActionIdRWDProp, negOrbitLookInputActionIdRWDProp,
                                         rwOrbitLookInputACPositiveContent, rwOrbitLookInputACNegativeContent, rwOrbitLookInputACContent,
                                         rwOrbitLookInputAIPositiveContent, rwOrbitLookInputAINegativeContent, rwOrbitLookInputAIContent,
                                         ref actionIDsOrbitLookPositive, ref actionIDsOrbitLookNegative, ref ActionGUIContentOrbitLookPositive, ref ActionGUIContentOrbitLookNegative, numActionCategories);
                    }
                    #endregion

                    #region Rewired - Left Fire 1
                    DrawFoldoutWithLabel(leftFire1ShowInEditorProp, leftFire1HeaderContent);
                    if (leftFire1ShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(leftFire1ButtonEnabledProp, leftFire1InputActionIdRWDProp, leftFire1CanBeHeldProp, rwLeftFire1EnabledContent, rwLeftFire1InputACContent, rwLeftFire1InputAIContent, canBeHeldContent, ref actionIDsLeftFire1, ref ActionGUIContentLeftFire1, numActionCategories);
                    }
                    #endregion

                    #region Rewired - Left Fire 2
                    DrawFoldoutWithLabel(leftFire2ShowInEditorProp, leftFire2HeaderContent);
                    if (leftFire2ShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(leftFire2ButtonEnabledProp, leftFire2InputActionIdRWDProp, leftFire2CanBeHeldProp, rwLeftFire2EnabledContent, rwLeftFire2InputACContent, rwLeftFire2InputAIContent, canBeHeldContent, ref actionIDsLeftFire2, ref ActionGUIContentLeftFire2, numActionCategories);
                    }
                    #endregion

                    #region Rewired - Right Fire 1
                    DrawFoldoutWithLabel(rightFire1ShowInEditorProp, rightFire1HeaderContent);
                    if (rightFire1ShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(rightFire1ButtonEnabledProp, rightFire1InputActionIdRWDProp, rightFire1CanBeHeldProp, rwRightFire1EnabledContent, rwRightFire1InputACContent, rwRightFire1InputAIContent, canBeHeldContent, ref actionIDsRightFire1, ref ActionGUIContentRightFire1, numActionCategories);
                    }
                    #endregion

                    #region Rewired - Right Fire 2
                    DrawFoldoutWithLabel(rightFire2ShowInEditorProp, rightFire2HeaderContent);
                    if (rightFire2ShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(rightFire2ButtonEnabledProp, rightFire2InputActionIdRWDProp, rightFire2CanBeHeldProp, rwRightFire2EnabledContent, rwRightFire2InputACContent, rwRightFire2InputAIContent, canBeHeldContent, ref actionIDsRightFire2, ref ActionGUIContentRightFire2, numActionCategories);
                    }
                    #endregion

                }
                else
                {
                    EditorGUILayout.HelpBox("Could not find Rewired Input Manager in the scene", MessageType.Warning, true);
                }
            }
            #else
            {
                EditorGUILayout.HelpBox("Rewired does not appear to be installed in the project.", MessageType.Warning, true);
            }
#endif
            #endregion Rewired Input

            #region UnityXR Input
            else if (inputModeProp.intValue == (int)StickyInputModule.InputMode.UnityXR && !isRefreshing)
            {
                #if SCSM_XR && SSC_UIS
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(xrPluginVersionContent, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(string.IsNullOrEmpty(xrVersion) ? xrPluginNotInstalled : xrVersion);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(xrOpenXRVersionContent, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(string.IsNullOrEmpty(xrVersion) ? xrPluginNotInstalled : openXRVersion);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(xrOculusXRVersionContent, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(string.IsNullOrEmpty(oculusXRVersion) ? xrPluginNotInstalled : oculusXRVersion);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(uisVersionContent, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(uisVersion);
                EditorGUILayout.EndHorizontal(); 

                StickyEditorHelper.InTechPreview(false);

                EditorGUILayout.PropertyField(inputActionAssetXRProp, xrInputActionAssetContent);

                if (inputActionAssetXRProp.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("Unity XR uses the Unity Input System which requires a Input Action Asset.", MessageType.Error, true);
                }
                else
                {
                    #region Get Action Maps
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(" Available Actions", GUILayout.Width(defaultEditorLabelWidth));
                    if (GUILayout.Button("Refresh", GUILayout.MaxWidth(70f))) { XRRefreshActionMaps(); }
                    EditorGUILayout.EndHorizontal();

                    if (xrActionMapGUIContent == null) { XRRefreshActionMaps(); }

                    int numActionMapIDs = xrActionMapIDs == null ? 0 : xrActionMapIDs.Length;
                    #endregion

                    // For XR Camera, see StickyControlModuleEditor - Look tab

                    #region XR Hands

                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(xrLeftHandTransformContent, GUILayout.Width(defaultEditorLabelWidth - 53f));
                    if (GUILayout.Button(xrNewHandContent, buttonCompact, GUILayout.MaxWidth(50f)) && leftHandTransformXRProp.objectReferenceValue == null)
                    {
                        XRCreateHand(leftHandTransformXRProp, true);
                    }
                    EditorGUILayout.PropertyField(leftHandTransformXRProp, GUIContent.none);

                    GUILayout.EndHorizontal();

                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(xrRightHandTransformContent, GUILayout.Width(defaultEditorLabelWidth - 53f));
                    if (GUILayout.Button(xrNewHandContent, buttonCompact, GUILayout.MaxWidth(50f)) && rightHandTransformXRProp.objectReferenceValue == null)
                    {
                        XRCreateHand(rightHandTransformXRProp, false);
                    }
                    EditorGUILayout.PropertyField(rightHandTransformXRProp, GUIContent.none);

                    GUILayout.EndHorizontal();

                    EditorGUILayout.PropertyField(leftHandOffsetRotXRProp, xrLeftHandOffsetRotContent);
                    EditorGUILayout.PropertyField(rightHandOffsetRotXRProp, xrRightHandOffsetRotContent);

                    #endregion

                    #region HMD Position Input
                    DrawFoldoutWithLabel(hmdPosShowInEditorProp, xrHMDPosHeaderContent);
                    if (hmdPosShowInEditorProp.boolValue)
                    {
                        // This needs to be a Vector3 - don't show the data slots.
                        DrawXR(hmdPosInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, hmdPosInputActionMapIdXRProp,
                        hmdPosInputActionIdXRProp, hmdPosInputActionDataSlotXRProp,
                        xrInputAMContent, xrHMDPosInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, false, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Left Hand Position Input
                    DrawFoldoutWithLabel(leftHandPosShowInEditorProp, xrLeftHandPosHeaderContent);
                    if (leftHandPosShowInEditorProp.boolValue)
                    {
                        // This needs to be a Vector3 - don't show the data slots.
                        DrawXR(leftHandPosInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, leftHandPosInputActionMapIdXRProp,
                        leftHandPosInputActionIdXRProp, leftHandPosInputActionDataSlotXRProp,
                        xrInputAMContent, xrLeftHandPosInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, false, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Left Hand Rotation Input
                    DrawFoldoutWithLabel(leftHandRotShowInEditorProp, xrLeftHandRotHeaderContent);
                    if (leftHandRotShowInEditorProp.boolValue)
                    {
                        // This needs to be a Vector3 - don't show the data slots.
                        DrawXR(leftHandRotInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, leftHandRotInputActionMapIdXRProp,
                        leftHandRotInputActionIdXRProp, leftHandRotInputActionDataSlotXRProp,
                        xrInputAMContent, xrLeftHandRotInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, false, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Right Hand Position Input
                    DrawFoldoutWithLabel(rightHandPosShowInEditorProp, xrRightHandPosHeaderContent);
                    if (rightHandPosShowInEditorProp.boolValue)
                    {
                        // This needs to be a Vector3 - don't show the data slots.
                        DrawXR(rightHandPosInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, rightHandPosInputActionMapIdXRProp,
                        rightHandPosInputActionIdXRProp, rightHandPosInputActionDataSlotXRProp,
                        xrInputAMContent, xrRightHandPosInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, false, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Right Hand Rotation Input
                    DrawFoldoutWithLabel(rightHandRotShowInEditorProp, xrRightHandRotHeaderContent);
                    if (rightHandRotShowInEditorProp.boolValue)
                    {
                        // This needs to be a Vector3 - don't show the data slots.
                        DrawXR(rightHandRotInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, rightHandRotInputActionMapIdXRProp,
                        rightHandRotInputActionIdXRProp, rightHandRotInputActionDataSlotXRProp,
                        xrInputAMContent, xrRightHandRotInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, false, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Horizontal Move Input
                    DrawFoldoutWithLabel(hztlMoveShowInEditorProp, horizontalMoveHeaderContent);
                    if (hztlMoveShowInEditorProp.boolValue)
                    {
                        DrawXR(hztlMoveInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, posHztlMoveInputActionMapIdXRProp,
                        posHztlMoveInputActionIdUISProp, posHztlMoveInputActionDataSlotUISProp,
                        xrInputAMContent, uisHztlMoveInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, true, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Vertical Move Input
                    DrawFoldoutWithLabel(vertMoveShowInEditorProp, verticalMoveHeaderContent);
                    if (vertMoveShowInEditorProp.boolValue)
                    {
                        DrawXR(vertMoveInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, posVertMoveInputActionMapIdXRProp,
                        posVertMoveInputActionIdUISProp, posVertMoveInputActionDataSlotUISProp,
                        xrInputAMContent, uisVertMoveInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, true, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Sprint Input
                    DrawFoldoutWithLabel(sprintShowInEditorProp, sprintHeaderContent);
                    if (sprintShowInEditorProp.boolValue)
                    {
                        DrawXRButton(sprintButtonEnabledProp, uisSprintEnabledContent,
                        inputActionAssetXRProp, sprintInputActionMapIdXRProp,
                        sprintInputActionIdUISProp, sprintInputActionDataSlotUISProp,
                        null, xrInputAMContent, uisSprintInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        null, uisDataSlots2, uisDataSlots3, numActionMapIDs, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Jump Input
                    DrawFoldoutWithLabel(jumpShowInEditorProp, jumpHeaderContent);
                    if (jumpShowInEditorProp.boolValue)
                    {
                        DrawXRButton(jumpButtonEnabledProp, uisJumpEnabledContent,
                        inputActionAssetXRProp, jumpInputActionMapIdXRProp,
                        jumpInputActionIdUISProp, jumpInputActionDataSlotUISProp,
                        jumpCanBeHeldProp, xrInputAMContent, uisJumpInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        canBeHeldContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Crouch Input
                    DrawFoldoutWithLabel(crouchShowInEditorProp, crouchHeaderContent);
                    if (crouchShowInEditorProp.boolValue)
                    {
                        DrawXRButton(crouchButtonEnabledProp, uisCrouchEnabledContent,
                        inputActionAssetXRProp, crouchInputActionMapIdXRProp,
                        crouchInputActionIdUISProp, crouchInputActionDataSlotUISProp,
                        crouchCanBeHeldProp, xrInputAMContent, uisCrouchInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        canBeHeldContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, xrForceActionRefresh
                        );
                        if (crouchButtonEnabledProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(crouchIsToggledProp, crouchIsToggledContent);
                            EditorGUILayout.PropertyField(crouchIsJetPackOnlyProp, isJetPackOnlyContent);
                        }
                    }
                    #endregion

                    #region JetPack Input
                    DrawFoldoutWithLabel(jetpackShowInEditorProp, jetpackHeaderContent);
                    if (jetpackShowInEditorProp.boolValue)
                    {
                        DrawXRButton(jetpackButtonEnabledProp, uisJetPackEnabledContent,
                        inputActionAssetXRProp, jetpackInputActionMapIdXRProp,
                        jetpackInputActionIdUISProp, jetpackInputActionDataSlotUISProp,
                        null, xrInputAMContent, uisJetPackInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        null, uisDataSlots2, uisDataSlots3, numActionMapIDs, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Look Input (uses Horizontal variables)
                    DrawFoldoutWithLabel(hztlLookShowInEditorProp, xrLookTurnHeaderContent);
                    if (hztlLookShowInEditorProp.boolValue)
                    {
                        DrawXR(hztlLookInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, posHztlLookInputActionMapIdXRProp,
                        posHztlLookInputActionIdUISProp, posHztlLookInputActionDataSlotUISProp,
                        xrInputAMContent, xrLookTurnInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, true, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Look HMD Input (uses Vertical variables)
                    DrawFoldoutWithLabel(vertLookShowInEditorProp, xrLookHMDHeaderContent);
                    if (vertLookShowInEditorProp.boolValue)
                    {
                        DrawXR(vertLookInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, posVertLookInputActionMapIdXRProp,
                        posVertLookInputActionIdUISProp, posVertLookInputActionDataSlotUISProp,
                        xrInputAMContent, xrLookHMDInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, true, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Switch Look Input
                    DrawFoldoutWithLabel(switchLookShowInEditorProp, switchLookHeaderContent);
                    if (switchLookShowInEditorProp.boolValue)
                    {
                        DrawXRButton(switchLookButtonEnabledProp, uisSwitchLookEnabledContent,
                        inputActionAssetXRProp, switchLookInputActionMapIdXRProp,
                        switchLookInputActionIdUISProp, switchLookInputActionDataSlotUISProp,
                        null, xrInputAMContent, uisSwitchLookInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        null, uisDataSlots2, uisDataSlots3, numActionMapIDs, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Zoom Look Input
                    DrawFoldoutWithLabel(zoomLookShowInEditorProp, zoomLookHeaderContent);
                    if (zoomLookShowInEditorProp.boolValue)
                    {
                        DrawXR(zoomLookInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, posZoomLookInputActionMapIdXRProp,
                        posZoomLookInputActionIdUISProp, posZoomLookInputActionDataSlotUISProp,
                        xrInputAMContent, uisZoomLookInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, true, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Orbit Look Input (not used with XR)
                    //DrawFoldoutWithLabel(orbitLookShowInEditorProp, orbitLookHeaderContent);
                    //if (orbitLookShowInEditorProp.boolValue)
                    //{
                    //    DrawXR(orbitLookInputAxisModeProp, inputSystemAxisModeContent,
                    //    inputActionAssetXRProp, posOrbitLookInputActionMapIdXRProp,
                    //    posOrbitLookInputActionIdUISProp, posOrbitLookInputActionDataSlotUISProp,
                    //    xrInputAMContent, uisOrbitLookInputAContent, uisInputATypeContent, uisInputCTypeContent,
                    //    uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, true, xrForceActionRefresh
                    //    );
                    //}
                    #endregion

                    #region Left Fire 1 Input
                    DrawFoldoutWithLabel(leftFire1ShowInEditorProp, leftFire1HeaderContent);
                    if (leftFire1ShowInEditorProp.boolValue)
                    {
                        DrawXRButton(leftFire1ButtonEnabledProp, uisLeftFire1EnabledContent,
                        inputActionAssetXRProp, leftFire1InputActionMapIdXRProp,
                        leftFire1InputActionIdUISProp, leftFire1InputActionDataSlotUISProp,
                        leftFire1CanBeHeldProp, xrInputAMContent, uisLeftFire1InputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        canBeHeldContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Left Fire 2 Input
                    DrawFoldoutWithLabel(leftFire2ShowInEditorProp, leftFire2HeaderContent);
                    if (leftFire2ShowInEditorProp.boolValue)
                    {
                        DrawXRButton(leftFire2ButtonEnabledProp, uisLeftFire2EnabledContent,
                        inputActionAssetXRProp, leftFire2InputActionMapIdXRProp,
                        leftFire2InputActionIdUISProp, leftFire2InputActionDataSlotUISProp,
                        leftFire2CanBeHeldProp, xrInputAMContent, uisLeftFire2InputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        canBeHeldContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Right Fire 1 Input
                    DrawFoldoutWithLabel(rightFire1ShowInEditorProp, rightFire1HeaderContent);
                    if (rightFire1ShowInEditorProp.boolValue)
                    {
                        DrawXRButton(rightFire1ButtonEnabledProp, uisRightFire1EnabledContent,
                        inputActionAssetXRProp, rightFire1InputActionMapIdXRProp,
                        rightFire1InputActionIdUISProp, rightFire1InputActionDataSlotUISProp,
                        rightFire1CanBeHeldProp, xrInputAMContent, uisRightFire1InputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        canBeHeldContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, xrForceActionRefresh
                        );
                    }
                    #endregion

                    #region Right Fire 2 Input
                    DrawFoldoutWithLabel(rightFire2ShowInEditorProp, rightFire2HeaderContent);
                    if (rightFire2ShowInEditorProp.boolValue)
                    {
                        DrawXRButton(rightFire2ButtonEnabledProp, uisRightFire2EnabledContent,
                        inputActionAssetXRProp, rightFire2InputActionMapIdXRProp,
                        rightFire2InputActionIdUISProp, rightFire2InputActionDataSlotUISProp,
                        rightFire2CanBeHeldProp, xrInputAMContent, uisRightFire2InputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        canBeHeldContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, xrForceActionRefresh
                        );
                    }
                    #endregion

                    xrForceActionRefresh = false;
                }

                #elif !UNITY_2020_3_OR_NEWER
                EditorGUILayout.HelpBox("In S3D, Unity XR is only supported on Unity 2020.3 or newer.", MessageType.Warning, true);
                #else
                EditorGUILayout.HelpBox("Unity XR package or Unity Input System is not installed. Did you install the XR Management package?", MessageType.Warning, true);
                #endif
            }
            #endregion

            #region Custom Input
            DrawFoldoutWithLabel(ciShowInEditorProp, ciHeaderContent);
            if (ciShowInEditorProp.boolValue)
            {
                ciDeletePos = -1;
                ciMoveDownPos = -1;

                #region Check if list is null and get size
                // Checking the property for being NULL doesn't check if the list is actually null.
                if (stickyInputModule.customInputList == null)
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    stickyInputModule.customInputList = new List<CustomInput>(4);
                    // Read in the properties
                    serializedObject.Update();
                }

                int numCustomInputs = ciListProp.arraySize;
                #endregion

                #region Add or Remove items

                GUILayout.BeginHorizontal();

                EditorGUI.indentLevel += 1;
                EditorGUIUtility.fieldWidth = 15f;
                EditorGUI.BeginChangeCheck();
                ciIsListExpandedProp.boolValue = EditorGUILayout.Foldout(ciIsListExpandedProp.boolValue, GUIContent.none, foldoutStyleNoLabel);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(stickyInputModule.customInputList, ciIsListExpandedProp.boolValue);
                    // Read in the properties
                    serializedObject.Update();
                }
                EditorGUI.indentLevel -= 1;

                EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                EditorGUILayout.LabelField("Custom Inputs: " + numCustomInputs.ToString("00"), labelFieldRichText);

                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(stickyInputModule, "Add Custom Input");
                    stickyInputModule.customInputList.Add(new CustomInput());
                    ExpandList(stickyInputModule.customInputList, false);
                    // Read in the properties
                    serializedObject.Update();

                    numCustomInputs = ciListProp.arraySize;
                    if (numCustomInputs > 0)
                    {
                        // Force new custom input to be serialized in scene
                        ciItemProp = ciListProp.GetArrayElementAtIndex(numCustomInputs - 1);
                        ciItemShowInEditorProp = ciItemProp.FindPropertyRelative("showInEditor");
                        ciItemShowInEditorProp.boolValue = !ciItemShowInEditorProp.boolValue;
                        // Show the new custom player input
                        ciItemShowInEditorProp.boolValue = true;
                    }
                }
                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (numCustomInputs > 0) { ciDeletePos = ciListProp.arraySize - 1; }
                }

                GUILayout.EndHorizontal();

                #endregion

                #region Custom Input List items

                #if SSC_REWIRED
                int numActionCategories = actionCategoryIDs == null ? 0 : actionCategoryIDs.Length;
                #endif

                #if SSC_UIS
                bool isUISAvailable = inputModeProp.intValue == (int)StickyInputModule.InputMode.UnityInputSystem &&
                                      uisPlayerInput != null && uisPlayerInput.actions != null &&
                                      uisPlayerInput.notificationBehavior == UnityEngine.InputSystem.PlayerNotifications.InvokeUnityEvents &&
                                      !string.IsNullOrEmpty(uisPlayerInput.defaultActionMap);
                int numActionIDs = uisActionIDs == null ? 0 : uisActionIDs.Length;
                #endif

                #if SCSM_XR && SSC_UIS                
                bool isXRAvailable = inputModeProp.intValue == (int)StickyInputModule.InputMode.UnityXR &&
                                     inputActionAssetXRProp != null && inputActionAssetXRProp.objectReferenceValue != null;
                int numActionMapIDs = xrActionMapIDs == null ? 0 : xrActionMapIDs.Length;
                #endif

                for (int ciIdx = 0; ciIdx < numCustomInputs; ciIdx++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    ciItemProp = ciListProp.GetArrayElementAtIndex(ciIdx);

                    ciItemShowInEditorProp = ciItemProp.FindPropertyRelative("showInEditor");
                    ciIsButtonProp = ciItemProp.FindPropertyRelative("isButton");
                    ciCanBeHeldDownProp = ciItemProp.FindPropertyRelative("canBeHeldDown");
                    ciInputAxisModeProp = ciItemProp.FindPropertyRelative("inputAxisMode");
                    ciIsSensitivityEnabledProp = ciItemProp.FindPropertyRelative("isSensitivityEnabled");
                    ciSensitivityProp = ciItemProp.FindPropertyRelative("sensitivity");
                    ciGravityProp = ciItemProp.FindPropertyRelative("gravity");

                    #region Custom input name and Delete button
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel += 1;
                    ciItemShowInEditorProp.boolValue = EditorGUILayout.Foldout(ciItemShowInEditorProp.boolValue, "Custom Input " + (ciIdx + 1).ToString("00"));
                    EditorGUI.indentLevel -= 1;

                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numCustomInputs > 1) { ciMoveDownPos = ciIdx; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { ciDeletePos = ciIdx; }
                    GUILayout.EndHorizontal();
                    #endregion

                    if (ciItemShowInEditorProp.boolValue)
                    {
                        #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
                        // If isButton is changes from false to true, we may need to reset some values below
                        // to indicate only one input axis is possible.
                        bool hasButtonChanged = false;

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(ciIsButtonProp, ciIsButtonContent);
                        if (EditorGUI.EndChangeCheck()) { hasButtonChanged = true; }
                        #else
                        EditorGUILayout.PropertyField(ciIsButtonProp, ciIsButtonContent);
                        #endif

                        if (inputModeProp.intValue != (int)StickyInputModule.InputMode.Rewired &&
                            inputModeProp.intValue != (int)StickyInputModule.InputMode.UnityInputSystem &&
                            inputModeProp.intValue != (int)StickyInputModule.InputMode.UnityXR)
                        {
                            if (ciIsButtonProp.boolValue)
                            {
                                EditorGUILayout.PropertyField(ciCanBeHeldDownProp, ciCanBeHeldDownContent);
                            }
                        }

                        #region Direct Keyboard Input
                        if (inputModeProp.intValue == (int)StickyInputModule.InputMode.DirectKeyboard)
                        {
                            #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER                            
                            cidkmPositiveKeycodeProp = ciItemProp.FindPropertyRelative("dkmPositiveKeycode");
                            cidkmNegativeKeycodeProp = ciItemProp.FindPropertyRelative("dkmNegativeKeycode");

                            EditorGUILayout.PropertyField(cidkmPositiveKeycodeProp, positiveKeycodeContent);
                            if (!ciIsButtonProp.boolValue)
                            {
                                EditorGUILayout.PropertyField(cidkmNegativeKeycodeProp, negativeKeycodeContent);

                                if (cidkmPositiveKeycodeProp.intValue == cidkmNegativeKeycodeProp.intValue
                                    && cidkmPositiveKeycodeProp.intValue != 0)
                                {
                                    EditorGUILayout.HelpBox(identicalKeycodesWarningContent, MessageType.Warning);
                                }
                            }
                            else if (hasButtonChanged) { cidkmNegativeKeycodeProp.intValue = 0; }

                            #endif
                        }
                        #endregion

                        #region Legacy Unity Input System
                        else if (inputModeProp.intValue == (int)StickyInputModule.InputMode.LegacyUnity)
                        {
                            #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER

                            cilisPositiveAxisNameProp = ciItemProp.FindPropertyRelative("lisPositiveAxisName");
                            cilisNegativeAxisNameProp = ciItemProp.FindPropertyRelative("lisNegativeAxisName");
                            cilisInvertAxisProp = ciItemProp.FindPropertyRelative("lisInvertAxis");
                            cilisIsPositiveAxisValidProp = ciItemProp.FindPropertyRelative("lisIsPositiveAxisValid");
                            cilisIsNegativeAxisValidProp = ciItemProp.FindPropertyRelative("lisIsNegativeAxisValid");

                            if (ciIsButtonProp.boolValue)
                            {
                                // If this is a button, there is only one input
                                if (hasButtonChanged) { ciInputAxisModeProp.intValue = (int)StickyInputModule.InputAxisMode.SingleAxis; }

                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(cilisPositiveAxisNameProp, singleAxisNameContent);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    cilisIsPositiveAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(cilisPositiveAxisNameProp.stringValue, false);
                                }
                                if (!cilisIsPositiveAxisValidProp.boolValue)
                                {
                                    EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                                }
                            }
                            else
                            {
                                // Single or Combined Axis
                                EditorGUILayout.PropertyField(ciInputAxisModeProp, inputAxisModeContent);
                                if (ciInputAxisModeProp.intValue != (int)StickyInputModule.InputAxisMode.NoInput)
                                {
                                    if (ciInputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.SingleAxis)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        EditorGUILayout.PropertyField(cilisPositiveAxisNameProp, singleAxisNameContent);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            cilisIsPositiveAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(cilisPositiveAxisNameProp.stringValue, false);
                                        }
                                        if (!cilisIsPositiveAxisValidProp.boolValue)
                                        {
                                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                                        }
                                    }
                                    else
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        EditorGUILayout.PropertyField(cilisPositiveAxisNameProp, positiveAxisNameContent);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            cilisIsPositiveAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(cilisPositiveAxisNameProp.stringValue, false);
                                        }
                                        if (!cilisIsPositiveAxisValidProp.boolValue)
                                        {
                                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                                        }

                                        EditorGUI.BeginChangeCheck();
                                        EditorGUILayout.PropertyField(cilisNegativeAxisNameProp, negativeAxisNameContent);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            cilisIsNegativeAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(cilisNegativeAxisNameProp.stringValue, false);
                                        }
                                        if (!cilisIsNegativeAxisValidProp.boolValue)
                                        {
                                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                                        }

                                        if (cilisPositiveAxisNameProp.stringValue == cilisNegativeAxisNameProp.stringValue)
                                        {
                                            EditorGUILayout.HelpBox(identicalAxisNamesWarningContent, MessageType.Warning);
                                        }
                                    }

                                    EditorGUILayout.PropertyField(cilisInvertAxisProp, invertInputAxisContent);
                                    DrawSensitivity(ciInputAxisModeProp, ciIsSensitivityEnabledProp, sensitivityEnabledContent,
                                                    ciSensitivityProp, sensitivityContent, ciGravityProp, gravityContent);
                                }
                            }

                            #endif
                        }
                        #endregion

                        #region Unity Input System (add with Unity package manager)
                        else if (inputModeProp.intValue == (int)StickyInputModule.InputMode.UnityInputSystem)
                        {
                            #if SSC_UIS
                            if (isUISAvailable)
                            {
                                ciIsButtonEnabledProp = ciItemProp.FindPropertyRelative("isButtonEnabled");
                                ciuisPositiveInputActionIdProp = ciItemProp.FindPropertyRelative("uisPositiveInputActionId");
                                ciuisPositiveInputActionDataSlotProp = ciItemProp.FindPropertyRelative("uisPositiveInputActionDataSlot");

                                if (ciIsButtonProp.boolValue)
                                {
                                    DrawUnityInputButton(ciIsButtonEnabledProp, uisCustomEnabledContent,
                                    ciuisPositiveInputActionIdProp, ciuisPositiveInputActionDataSlotProp,
                                    ciCanBeHeldDownProp, uisCustomInputAContent,
                                    uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                                    ciCanBeHeldDownContent, uisDataSlots2, uisDataSlots3, numActionIDs
                                    );
                                }
                                else
                                {
                                    DrawUnityInputSystem(ciInputAxisModeProp, inputSystemAxisModeContent,
                                    ciuisPositiveInputActionIdProp, ciuisPositiveInputActionDataSlotProp,
                                    uisCustomInputAContent, uisInputATypeContent, uisInputCTypeContent,
                                    uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionIDs, false
                                    );
                                    //DrawSensitivity(ciInputAxisModeProp,
                                    //ciIsSensitivityEnabledProp, sensitivityEnabledContent,
                                    //ciSensitivityProp, sensitivityContent,
                                    //ciGravityProp, gravityContent);
                                }
                            }
                            #endif
                        }
                        #endregion

                        #region Rewired
                        else if (inputModeProp.intValue == (int)StickyInputModule.InputMode.Rewired && !isRefreshing)
                        {
                            #if SSC_REWIRED
                            ciIsButtonEnabledProp = ciItemProp.FindPropertyRelative("isButtonEnabled");
                            cirwdPositiveInputActionIdProp = ciItemProp.FindPropertyRelative("rwdPositiveInputActionId");
                            cirwdNegativeInputActionIdProp = ciItemProp.FindPropertyRelative("rwdNegativeInputActionId");

                            CustomInput customInput = stickyInputModule.customInputList[ciIdx];

                            if (ciIsButtonProp.boolValue)
                            {
                                DrawRewiredInput(ciIsButtonEnabledProp, cirwdPositiveInputActionIdProp, ciCanBeHeldDownProp, rwCustomEnabledContent, rwCustomInputACContent, rwCustomInputAIContent,
                                                 ciCanBeHeldDownContent, ref customInput.actionIDsPositive, ref customInput.ActionGUIContentPositive, numActionCategories);
                            }
                            else
                            {
                                DrawRewiredInput(PIMCategory.CustomInput, ciInputAxisModeProp, cirwdPositiveInputActionIdProp, cirwdNegativeInputActionIdProp,
                                                 rwCustomInputACPositiveContent, rwCustomInputACNegativeContent, rwCustomInputACContent,
                                                 rwCustomInputAIPositiveContent, rwCustomInputAINegativeContent, rwCustomInputAIContent,
                                                 ref customInput.actionIDsPositive, ref customInput.actionIDsNegative,
                                                 ref customInput.ActionGUIContentPositive, ref customInput.ActionGUIContentNegative, numActionCategories);

                                //DrawSensitivity(ciInputAxisModeProp, ciIsSensitivityEnabledProp, sensitivityEnabledContent,
                                //ciSensitivityProp, sensitivityContent, ciGravityProp, gravityContent);
                            }

                            #endif
                        }

                        #endregion

                        #region UnityXR
                        else if (inputModeProp.intValue == (int)StickyInputModule.InputMode.UnityXR && !isRefreshing)
                        {
                            #if SCSM_XR && SSC_UIS
                            if (isXRAvailable)
                            {
                                ciIsButtonEnabledProp = ciItemProp.FindPropertyRelative("isButtonEnabled");
                                cixrPositiveInputActionMapIdProp = ciItemProp.FindPropertyRelative("xrPositiveInputActionMapId");
                                ciuisPositiveInputActionIdProp = ciItemProp.FindPropertyRelative("uisPositiveInputActionId");
                                ciuisPositiveInputActionDataSlotProp = ciItemProp.FindPropertyRelative("uisPositiveInputActionDataSlot");

                                if (ciIsButtonProp.boolValue)
                                {
                                    DrawXRButton(ciIsButtonEnabledProp, uisCustomEnabledContent,
                                    inputActionAssetXRProp, cixrPositiveInputActionMapIdProp,
                                    ciuisPositiveInputActionIdProp, ciuisPositiveInputActionDataSlotProp,
                                    ciCanBeHeldDownProp, xrInputAMContent, uisCustomInputAContent,
                                    uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                                    ciCanBeHeldDownContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, xrForceActionRefresh
                                    );
                                }
                                else
                                {
                                    DrawXR(ciInputAxisModeProp, inputSystemAxisModeContent,
                                    inputActionAssetXRProp, cixrPositiveInputActionMapIdProp,
                                    ciuisPositiveInputActionIdProp, ciuisPositiveInputActionDataSlotProp,
                                    xrInputAMContent, uisCustomInputAContent, uisInputATypeContent, uisInputCTypeContent,
                                    uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, false, xrForceActionRefresh
                                    );

                                    DrawSensitivity(ciInputAxisModeProp,
                                    ciIsSensitivityEnabledProp, sensitivityEnabledContent,
                                    ciSensitivityProp, sensitivityContent,
                                    ciGravityProp, gravityContent);
                                }
                            }
                            #endif                          
                        }
                        #endregion

                        ciEventProp = ciItemProp.FindPropertyRelative("customInputEvt");
                        EditorGUILayout.PropertyField(ciEventProp, ciEventContent);

                        #region AninActions

                        #region Check if AnimAction hash list is null and get size
                        // Checking the property for being NULL doesn't check if the list is actually null.
                        if (stickyInputModule.customInputList[ciIdx].animActionHashList == null)
                        {
                            // Apply property changes
                            serializedObject.ApplyModifiedProperties();
                            stickyInputModule.customInputList[ciIdx].animActionHashList = new List<int>(4);
                            // Read in the properties
                            serializedObject.Update();
                        }
                        #endregion

                        #region AnimAction properties
                        ciAAGUIDHashListProp = ciItemProp.FindPropertyRelative("animActionHashList");
                        int numAnimActionHashCodes = ciAAGUIDHashListProp.arraySize;
                        ciAAIsListExpandedProp = ciItemProp.FindPropertyRelative("isAnimActionHashListExpanded");
                        ciAADeletePos = -1;
                        #endregion

                        #region Refresh Anim Action list if required
                        if (numAnimActionHashCodes > 0 && ciDeletePos <= 0 && Time.realtimeSinceStartup - animActionListLastRefreshed > 3f && !EditorApplication.isPlayingOrWillChangePlaymode)
                        {
                            RefreshAnimActionList();
                        }
                        #endregion

                        #region Add or Remove AnimAction items

                        GUILayout.BeginHorizontal();

                        EditorGUI.indentLevel += 1;
                        EditorGUIUtility.fieldWidth = 15f;
                        EditorGUI.BeginChangeCheck();
                        ciAAIsListExpandedProp.boolValue = EditorGUILayout.Foldout(ciAAIsListExpandedProp.boolValue, GUIContent.none, foldoutStyleNoLabel);
                        if (EditorGUI.EndChangeCheck())
                        {
                            serializedObject.ApplyModifiedProperties();
                            ExpandList(stickyInputModule.customInputList[ciIdx].animActionHashList, ciAAIsListExpandedProp.boolValue);
                            // Read in the properties
                            serializedObject.Update();
                        }
                        EditorGUI.indentLevel -= 1;

                        EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                        EditorGUILayout.LabelField("Custom Animation Actions: " + numAnimActionHashCodes.ToString("00"), labelFieldRichText);

                        if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                        {
                            // Apply property changes
                            serializedObject.ApplyModifiedProperties();
                            Undo.RecordObject(stickyInputModule, "Add Animation Action");
                            stickyInputModule.customInputList[ciIdx].animActionHashList.Add(0);
                            // Read in the properties
                            serializedObject.Update();

                            numAnimActionHashCodes = ciAAGUIDHashListProp.arraySize;
                        }
                        if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                        {
                            if (numAnimActionHashCodes > 0) { ciAADeletePos = ciAAGUIDHashListProp.arraySize - 1; }
                        }

                        GUILayout.EndHorizontal();

                        #endregion

                        #region AnimAction List

                        // Show or all none of the (custom) Animation Actions
                        if (ciAAIsListExpandedProp.boolValue)
                        {
                            for (int ciAAHCIdx = 0; ciAAHCIdx < numAnimActionHashCodes; ciAAHCIdx++)
                            {
                                ciAAGUIDHashProp = ciAAGUIDHashListProp.GetArrayElementAtIndex(ciAAHCIdx);
                                int aaGUIDHash = ciAAGUIDHashProp.intValue;

                                #region Custom linked input AnimAction and Delete button
                                StickyEditorHelper.DrawUILine(separatorColor);
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField((aaGUIDHash == 0 ? " Unlinked" : " Linked") + " Animation Action " + (ciAAHCIdx + 1).ToString("00"));
                                // Delete button
                                if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { ciAADeletePos = ciAAHCIdx; }
                                GUILayout.EndHorizontal();
                                #endregion

                                if (animActionNameList.Count < 1) { EditorGUILayout.LabelField(ciNoCustomAnimActionsContent); }
                                else
                                {
                                    // Get the index of the AnimAction from the list of available Custom Anim Actions
                                    // This list gets refreshed approx every 3 seconds in the editor (not at runtime)
                                    int aaLookupCustomIndex = animActionGUIDHashList.FindIndex(guidHash => guidHash == aaGUIDHash);

                                    // Display the custom AnimActions from the list of matching AnimActions
                                    EditorGUI.BeginChangeCheck();
                                    aaLookupCustomIndex = EditorGUILayout.Popup(ciAnimActionContent, aaLookupCustomIndex, animActionNameList.ToArray());
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        // Lookup the GUID for the selected AnimAction
                                        if (aaLookupCustomIndex < 0) { ciAAGUIDHashProp.intValue = 0; }
                                        else { ciAAGUIDHashProp.intValue = animActionGUIDHashList[aaLookupCustomIndex]; }
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Delete Animation Action hash codes

                        if (ciAADeletePos >= 0)
                        {
                            // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and ciAADeletePos is reset to -1.
                            int _deleteIndex = ciAADeletePos;
                            //Debug.Log("[DEBUG] predialog arraysize: " + ciAAGUIDHashListProp.arraySize);

                            if (EditorUtility.DisplayDialog("Delete Animation Action " + (ciAADeletePos + 1) + "?", "Anim Action " + (ciAADeletePos + 1).ToString("00") + " will be deleted\n\nThis will unlink this Animation Action from the Custom Input " + (ciIdx+1).ToString("00") + " and cannot be undone.", "Delete Now", "Cancel"))
                            {
                                //serializedObject.Update();
                                //Debug.Log("[DEBUG] ciIdx: " + ciIdx + " delIdx: " + _deleteIndex + " arraysize: " + ciAAGUIDHashListProp.arraySize);
                                ciAAGUIDHashListProp.DeleteArrayElementAtIndex(_deleteIndex);
                                ciAADeletePos = -1;
                                serializedObject.ApplyModifiedProperties();
                                GUIUtility.ExitGUI();
                            }
                        }

                        #endregion

                        #endregion
                    }

                    GUILayout.EndVertical();
                }

                #endregion

                #region Delete/Move Items
                if (ciDeletePos >= 0 || ciMoveDownPos >= 0)
                {
                    GUI.FocusControl(null);
                    // Don't permit multiple operations in the same pass
                    if (ciMoveDownPos >= 0)
                    {
                        // Move down one position, or wrap round to start of list
                        if (ciMoveDownPos < ciListProp.arraySize - 1)
                        {
                            ciListProp.MoveArrayElement(ciMoveDownPos, ciMoveDownPos + 1);
                        }
                        else { ciListProp.MoveArrayElement(ciMoveDownPos, 0); }

                        ciMoveDownPos = -1;

                        serializedObject.ApplyModifiedProperties();
                        GUIUtility.ExitGUI();
                    }
                    else if (ciDeletePos >= 0)
                    {
                        // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and ciDeletePos is reset to -1.
                        int _deleteIndex = ciDeletePos;

                        if (EditorUtility.DisplayDialog("Delete Custom Input " + (ciDeletePos + 1) + "?", "Custom Input " + (ciDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the custom input from the list and cannot be undone.", "Delete Now", "Cancel"))
                        {
                            ciListProp.DeleteArrayElementAtIndex(_deleteIndex);
                            ciDeletePos = -1;
                            serializedObject.ApplyModifiedProperties();
                            GUIUtility.ExitGUI();
                        }
                    }
                }
                #endregion

            }
            #endregion Custom Input

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            stickyInputModule.allowRepaint = true;

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && stickyInputModule != null)
            {
                float rightLabelWidth = 175f;
                //characterInput = stickyInputModule.GetCharacterInput;
                bool isInputNull = characterInput == null;

                StickyEditorHelper.PerformanceImpact();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyInputModule.IsInitialised ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInputEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyInputModule.IsInputEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsCustomInputOnlyEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyInputModule.IsCustomInputOnlyEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                #if SCSM_XR && SSC_UIS
                if (inputModeProp.intValue == (int)StickyInputModule.InputMode.UnityXR && !isRefreshing)
                {
                    bool isInputNullXR = characterInputXR == null;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugHMDXRPositionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(isInputNullXR ? "--" : StickyEditorHelper.GetVector3Text(characterInputXR.hmdPosition, 2), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugLeftHandXRGripContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(isInputNullXR ? "--" : characterInputXR.leftHandGrip.ToString("0.000"), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugLeftHandXRPositionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(isInputNullXR ? "--" : StickyEditorHelper.GetVector3Text(characterInputXR.leftHandPosition, 2), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugLeftHandXRTriggerContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(isInputNullXR ? "--" : characterInputXR.leftHandTrigger.ToString("0.000"), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugLeftHandXRRotationContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(isInputNullXR ? "--" : StickyEditorHelper.GetVector3Text(characterInputXR.leftHandRotation.eulerAngles, 2), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugRightHandXRGripContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(isInputNullXR ? "--" : characterInputXR.rightHandGrip.ToString("0.000"), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugRightHandXRPositionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(isInputNullXR ? "--" : StickyEditorHelper.GetVector3Text(characterInputXR.rightHandPosition, 2), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();
 
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugRightHandXRTriggerContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(isInputNullXR ? "--" : characterInputXR.rightHandTrigger.ToString("0.000"), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugRightHandXRRotationContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(isInputNullXR ? "--" : StickyEditorHelper.GetVector3Text(characterInputXR.rightHandRotation.eulerAngles, 2), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();
                }
                #endif

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugHorizontalMoveContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isInputNull ? "--" : characterInput.horizontalMove.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugVerticalMoveContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isInputNull ? "--" : characterInput.verticalMove.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugJumpContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isInputNull ? "--" : characterInput.jump ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugSprintContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isInputNull ? "--" : characterInput.sprint ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugCrouchContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isInputNull ? "--" : characterInput.crouch ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugJetBackToggleContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isInputNull ? "--" : characterInput.jetpack? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugHorizontalLookContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isInputNull ? "--" : characterInput.horizontalLook.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugVerticalLookContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isInputNull ? "--" : characterInput.verticalLook.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugXRLookContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isInputNull ? "--" : StickyEditorHelper.GetVector3Text(characterInput.xrLook.eulerAngles, 2), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugZoomLookContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isInputNull ? "--" : characterInput.zoomLook.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugOrbitLookContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isInputNull ? "--" : characterInput.orbitLook.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugLeftFire1Content, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isInputNull ? "--" : characterInput.leftFire1 ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugLeftFire2Content, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isInputNull ? "--" : characterInput.leftFire2 ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugRightFire1Content, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isInputNull ? "--" : characterInput.rightFire1 ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugRightFire2Content, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isInputNull ? "--" : characterInput.rightFire2 ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            #endregion
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Expand (show) or collapse (hide) all items in a list in the editor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="componentList"></param>
        /// <param name="isExpanded"></param>
        private void ExpandList<T>(List<T> componentList, bool isExpanded)
        {
            int numComponents = componentList == null ? 0 : componentList.Count;

            if (numComponents > 0)
            {
                System.Type compType = typeof(T);

                if (compType == typeof(CustomInput))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as CustomInput).showInEditor = isExpanded;
                    }
                }
            }
        }

        /// <summary>
        /// Refresh the list of available Custom animation actions from the StickyControlModule Animate tab.
        /// </summary>
        private void RefreshAnimActionList()
        {
            animActionListLastRefreshed = Time.realtimeSinceStartup;

            int numAimActions = stickyControlModule == null || stickyControlModule.s3dAnimActionList == null ? 0 : stickyControlModule.s3dAnimActionList.Count;

            // Create a GUID/name pair of AnimActions from StickyControlModule Animate tab
            if (animActionGUIDHashList == null) { animActionGUIDHashList = new List<int>(numAimActions); }
            if (animActionNameList == null) { animActionNameList = new List<string>(numAimActions); }

            if (animActionGUIDHashList != null && animActionNameList != null)
            {
                animActionGUIDHashList.Clear();
                animActionNameList.Clear();

                animActionGUIDHashList.Add(0);
                animActionNameList.Add("Not Set");

                if (animParmList == null) { animParmList = new List<S3DAnimParm>(20); }
                S3DAnimParm.GetParameterListAll(stickyControlModule.defaultAnimator, animParmList);

                int numParms = animParmList == null ? 0 : animParmList.Count;

                for (int aaIdx = 0; aaIdx < numAimActions; aaIdx++)
                {
                    S3DAnimAction animAction = stickyControlModule.s3dAnimActionList[aaIdx];
                    if (animAction.standardAction == S3DAnimAction.StandardAction.Custom)
                    {
                        animActionGUIDHashList.Add(animAction.guidHash);

                        S3DAnimParm animParm = animParmList.Find(par => par.hashCode == animAction.paramHashCode);

                        string _parmNam = " [" + animParm.paramName + "]";

                        animActionNameList.Add("Anim Action " + (aaIdx+1).ToString("00") + " " + animAction.parameterType.ToString() + _parmNam);
                    }
                }
            }
        }

        #endregion

        #region Private Methods - Unity Input System
        #if SSC_UIS

        /// <summary>
        /// Add events to the default map selected in Unity's PlayerInput script.
        /// </summary>
        public bool UISAddDefaultEvents()
        {
            bool isSuccess = false;

            if (uisPlayerInput != null && uisPlayerInput.actions != null)
            {
                UnityEngine.InputSystem.InputActionMap actionMap = uisPlayerInput.actions.FindActionMap(uisPlayerInput.defaultActionMap);

                if (EditorUtility.DisplayDialog("Add Actions", "Add Actions to Default Map: " + actionMap.name + "?", "Yes", "CANCEL!"))
                {
                    UnityEngine.InputSystem.InputAction action = null;

                    // Composites can be 1DAxis (or Axis), 2DVector (or Dpad), ButtonWithOneModifier, or ButtonWithTwoModifiers.
                    // When adding bindings AddCompositeBinding(action, "1DAxis(normalize=true)"), normalize=true seems to be on by default.

                    // For composite bindings e.g. 2DVector/Dpad, the value is returned according to the axis - not the order it is added with AddCompositeBinding().
                    // Left/Right = x-axis = vector2.x = SSC DataSlot1, Up/Down = y-axis = vector2.y = SSC DataSlot2.

                    // EXAMPLE of adding a Vector2/Dpad in a single line
                    // UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Gamepad>/leftStick", null, null, "Gamepad").WithName("LeftStick");

                    // EXAMPLE Vector2 composite binding
                    //action = actionMap.FindAction("Vertical_Roll");
                    //if (action == null)
                    //{
                    //    action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                    //                actionMap, "Vertical_Roll", UnityEngine.InputSystem.InputActionType.Value, null, null, null, null, "Dpad");
                    //    if (action != null)
                    //    {
                    //        UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "2DVector") // Or "Dpad"
                    //             .With("Up", "<Gamepad>/rightStick/up", "Gamepad")
                    //             .With("Down", "<Gamepad>/rightStick/down", "Gamepad")
                    //             .With("Left", "<Gamepad>/rightStick/left", "Gamepad")
                    //             .With("Right", "<Gamepad>/rightStick/right", "Gamepad");

                    //        UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "Dpad") // Or "2DVector"
                    //             .With("Up", "<Keyboard>/e", "Keyboard&Mouse")
                    //             .With("Down", "<Keyboard>/q", "Keyboard&Mouse")
                    //             .With("Left", "<Keyboard>/leftArrow", "Keyboard&Mouse")
                    //             .With("Right", "<Keyboard>/rightArrow", "Keyboard&Mouse");
                    //    }
                    //}

                    #region Horizontal Move
                    action = actionMap.FindAction("HorizontalMove");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "HorizontalMove", UnityEngine.InputSystem.InputActionType.Value, null, null, null, null, "Axis");
                        if (action != null)
                        {
                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "Axis") // Or "1DAxis"
                                 .With("Positive", "<Gamepad>/leftStick/right", "Gamepad")
                                 .With("Negative", "<Gamepad>/leftStick/left", "Gamepad");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis") // Or "Axis"
                                 .With("Positive", "<Keyboard>/d", "Keyboard&Mouse")
                                 .With("Negative", "<Keyboard>/a", "Keyboard&Mouse");
                        }
                    }
                    #endregion

                    #region Vertical Move
                    action = actionMap.FindAction("VerticalMove");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "VerticalMove", UnityEngine.InputSystem.InputActionType.Value, null, null, null, null, "Axis");
                        if (action != null)
                        {
                            //UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                            //    .With("Positive", "<Gamepad>/rightTrigger", "Gamepad")
                            //    .With("Negative", "<Gamepad>/leftTrigger", "Gamepad");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "Axis") // Or "1DAxis"
                                 .With("Positive", "<Gamepad>/leftStick/up", "Gamepad")
                                 .With("Negative", "<Gamepad>/leftStick/down", "Gamepad");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                .With("Positive", "<Keyboard>/w", "Keyboard&Mouse")
                                .With("Negative", "<Keyboard>/s", "Keyboard&Mouse");
                        }
                    }
                    #endregion

                    #region Horizontal Look
                    action = actionMap.FindAction("HorizontalLook");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "HorizontalLook", UnityEngine.InputSystem.InputActionType.Value, null, null, null, null, "Axis");
                        if (action != null)
                        {
                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                 .With("Positive", "<Keyboard>/rightArrow", "Keyboard&Mouse")
                                 .With("Negative", "<Keyboard>/leftArrow", "Keyboard&Mouse");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                .With("Positive", "<Gamepad>/rightStick/right", "Gamepad")
                                .With("Negative", "<Gamepad>/rightStick/left", "Gamepad");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                .With("Positive", "<Joystick>/stick/right", "Joystick")
                                .With("Negative", "<Joystick>/stick/left", "Joystick");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Mouse>/delta/x", null, null, "Keyboard&Mouse");
                        }
                    }
                    #endregion

                    #region Vertical Look
                    action = actionMap.FindAction("VerticalLook");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "VerticalLook", UnityEngine.InputSystem.InputActionType.Value, null, null, null, null, "Axis");
                        if (action != null)
                        {
                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                 .With("Positive", "<Keyboard>/upArrow", "Keyboard&Mouse")
                                 .With("Negative", "<Keyboard>/downArrow", "Keyboard&Mouse");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                .With("Positive", "<Gamepad>/rightStick/up", "Gamepad")
                                .With("Negative", "<Gamepad>/rightStick/down", "Gamepad");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                .With("Positive", "<Joystick>/stick/up", "Joystick")
                                .With("Negative", "<Joystick>/stick/down", "Joystick");
                            
                            UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Mouse>/delta/y", null, null, "Keyboard&Mouse");
                        }
                    }
                    #endregion

                    #region Zoom Look
                    action = actionMap.FindAction("ZoomLook");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "ZoomLook", UnityEngine.InputSystem.InputActionType.Value, null, null, null, null, "Axis");
                        if (action != null)
                        {
                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                 .With("Positive", "<Keyboard>/rightBracket", "Keyboard&Mouse")
                                 .With("Negative", "<Keyboard>/leftBracket", "Keyboard&Mouse");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                .With("Positive", "<Gamepad>/dpad/up", "Gamepad")
                                .With("Negative", "<Gamepad>/dpad/down", "Gamepad");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis(whichSideWins=1)")
                                 .With("Positive", "<Mouse>/scroll/y", "Keyboard&Mouse")
                                 .With("Negative", "<Mouse>/scroll/y", "Keyboard&Mouse");
                        }
                    }
                    #endregion

                    #region Orbit Look
                    action = actionMap.FindAction("OrbitLook");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "OrbitLook", UnityEngine.InputSystem.InputActionType.Value, null, null, null, null, "Axis");
                        if (action != null)
                        {
                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                 .With("Positive", "<Keyboard>/0", "Keyboard&Mouse")
                                 .With("Negative", "<Keyboard>/9", "Keyboard&Mouse");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                .With("Positive", "<Gamepad>/dpad/right", "Gamepad")
                                .With("Negative", "<Gamepad>/dpad/left", "Gamepad");
                        }
                    }
                    #endregion

                    #region Sprint
                    action = actionMap.FindAction("Sprint");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "Sprint", UnityEngine.InputSystem.InputActionType.Button, null, null, null, null, "Button");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Keyboard>/shift", null, null, "Keyboard&Mouse").WithName("Shift");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Gamepad>/buttonSouth", null, null, "Gamepad").WithName("AorCrossButton");
                    }
                    #endregion

                    #region Jump
                    action = actionMap.FindAction("Jump");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "Jump", UnityEngine.InputSystem.InputActionType.Button, null, null, null, null, "Button");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Keyboard>/space", null, null, "Keyboard&Mouse").WithName("SpaceBar");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Gamepad>/rightShoulder", null, null, "Gamepad").WithName("rightShoulder");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Joystick>/trigger", null, null, "Joystick").WithName("JoyTrigger");
                    }
                    #endregion

                    #region Crouch
                    action = actionMap.FindAction("Crouch");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "Crouch", UnityEngine.InputSystem.InputActionType.Button, null, null, null, null, "Button");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Keyboard>/c", null, null, "Keyboard&Mouse").WithName("c");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Gamepad>/leftShoulder", null, null, "Gamepad").WithName("leftShoulder");
                    }
                    #endregion

                    #region JetPack
                    action = actionMap.FindAction("JetPack");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "JetPack", UnityEngine.InputSystem.InputActionType.Button, null, null, null, null, "Button");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Keyboard>/j", null, null, "Keyboard&Mouse").WithName("j");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Gamepad>/buttonWest", null, null, "Gamepad").WithName("XorSquareButton");
                    }
                    #endregion

                    #region Switch Look
                    action = actionMap.FindAction("SwitchLook");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "SwitchLook", UnityEngine.InputSystem.InputActionType.Button, null, null, null, null, "Button");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Keyboard>/v", null, null, "Keyboard&Mouse").WithName("v");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Gamepad>/buttonNorth", null, null, "Gamepad").WithName("YorTriangleButton");
                    }
                    #endregion

                    #region Find or Use
                    action = actionMap.FindAction("FindOrUse");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "FindOrUse", UnityEngine.InputSystem.InputActionType.Button, null, null, null, null, "Button");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Keyboard>/f", null, null, "Keyboard&Mouse").WithName("f");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Gamepad>/buttonEast", null, null, "Gamepad").WithName("BorCircleButton");
                    }
                    #endregion

                    #region Pickup
                    action = actionMap.FindAction("Pickup");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "Pickup", UnityEngine.InputSystem.InputActionType.Button, null, null, null, null, "Button");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Keyboard>/g", null, null, "Keyboard&Mouse").WithName("g");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Gamepad>/buttonEast", null, null, "Gamepad").WithName("BorCircleButton");
                    }
                    #endregion

                    #region Fire1
                    action = actionMap.FindAction("Fire1");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "Fire1", UnityEngine.InputSystem.InputActionType.Button, null, null, null, null, "Button");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Mouse>/leftButton", null, null, "Keyboard&Mouse").WithName("lmb");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Gamepad>/rightTrigger", null, null, "Gamepad").WithName("RightTrigger");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Joystick>/trigger", null, null, "Joystick").WithName("joysticktrigger");
                    }
                    #endregion

                    #region Aim
                    action = actionMap.FindAction("Aim");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "Aim", UnityEngine.InputSystem.InputActionType.Button, null, null, null, null, "Button");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Mouse>/rightButton", null, null, "Keyboard&Mouse").WithName("rmb");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Gamepad>/leftTrigger", null, null, "Gamepad").WithName("LeftTrigger");
                        //UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Joystick>/trigger", null, null, "Joystick").WithName("joysticktrigger");
                    }
                    #endregion

                    UISSaveInputAsset();

                    isSuccess = true;
                }
            }

            return isSuccess;
        }

        /// <summary>
        /// Attempt to save the Input Sytem asset to disk
        /// </summary>
        private void UISSaveInputAsset()
        {
            if (uisPlayerInput != null && uisPlayerInput.actions != null)
            {
                try
                {
                    string playerInputPath = AssetDatabase.GetAssetPath(uisPlayerInput.actions);
                    string jsonActions = uisPlayerInput.actions.ToJson();
                    //Debug.Log("asset path: " + playerInputPath + " name: " + uisPlayerInput.actions.name);

                    if (!string.IsNullOrEmpty(jsonActions) && !string.IsNullOrEmpty(playerInputPath))
                    {
                        System.IO.File.WriteAllText(playerInputPath, jsonActions);

                        // This was required for 1.0.0 preview 3 and earlier but causes issues
                        // and isn't required for 1.0.0 preview 4+
                        //AssetDatabase.ImportAsset(playerInputPath);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("ERROR: StickyInputModule - could not save asset: " + uisPlayerInput.actions.name + " PLEASE REPORT - " + ex.Message);
                }

                //EditorUtility.SetDirty(uisPlayerInput.actions);
            }
        }

        /// <summary>
        /// Refresh the list of Actions available for the Default Map
        /// </summary>
        private void UISRefreshActions()
        {
            List<string> uisActionNameList = new List<string>(10);
            List<string> uisActionIDList = new List<string>(10);
            List<GUIContent> uisActionGUIContentList = new List<GUIContent>(10);

            if (uisPlayerInput != null && uisPlayerInput.actions != null)
            {
                UnityEngine.InputSystem.InputActionMap actionMap = uisPlayerInput.actions.FindActionMap(uisPlayerInput.defaultActionMap);

                // This readonly array can not be null but the map can be.
                // The map can be null when it was previously set in PlayerInput but was deleted in the UIS PlayerInput Editor.
                int numActions = actionMap == null ? 0 : actionMap.actions.Count;

                for (int actIdx = 0; actIdx < numActions; actIdx++)
                {
                    UnityEngine.InputSystem.InputAction action = actionMap.actions[actIdx];
                    uisActionNameList.Add(action.name);
                    uisActionIDList.Add(action.id.ToString());
                    uisActionGUIContentList.Add(new GUIContent(action.name));
                }
            }

            // Populate the lookup arrays and the GUIContent used in the dropdown list
            if (uisActionGUIContentList.Count > 0)
            {
                uisActionGUIContent = uisActionGUIContentList.ToArray();
                uisActionIDs = uisActionIDList.ToArray();
                uisActionNames = uisActionNameList.ToArray();
            }
            else
            {
                uisActionGUIContent = new GUIContent[] { new GUIContent(uisDropDownNone) };
                uisActionNames = new string[] { uisDropDownNone };
                uisActionIDs = new string[] { "" };
            }
        }

        /// <summary>
        /// Get the index of the action in the list of uisActionIDs.
        /// </summary>
        /// <param name="actionId"></param>
        /// <returns></returns>
        private int UISGetActionIdx(string actionId)
        {
            if (uisActionIDs != null)
            {
                return ArrayUtility.FindIndex<string>(uisActionIDs, s => s == actionId);
            }
            else { return -1; }
        }

        /// <summary>
        /// Set the action Id (which is a string) for the action name supplied. Set the zero-based dataSlot
        /// (without validation) to the one supplied (set to 0 if no matching action name).
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="dataSlot"></param>
        /// <param name="positiveInputActionIdProp"></param>
        /// <param name="positiveInputActionDataSlotProp"></param>
        private void UISSetActionId(string actionName, int dataSlot, SerializedProperty positiveInputActionIdProp, SerializedProperty positiveInputActionDataSlotProp)
        {
            int acNameIdx = ArrayUtility.FindIndex(uisActionNames, an => an == actionName && !string.IsNullOrEmpty(actionName));
            if (acNameIdx >= 0 && acNameIdx < uisActionNames.Length)
            {
                positiveInputActionIdProp.stringValue = uisActionIDs[acNameIdx];
                positiveInputActionDataSlotProp.intValue = dataSlot;
            }
            else
            {
                positiveInputActionIdProp.stringValue = string.Empty;
                positiveInputActionDataSlotProp.intValue = 0;
            }
        }

        #endif
        #endregion

        #region Private Draw methods - General

        private void DrawFoldoutWithLabel(SerializedProperty showInEditorProp, GUIContent headerLabel)
        {
            GUILayout.BeginHorizontal();
            EditorGUI.indentLevel += 1;
            EditorGUIUtility.fieldWidth = 15f;
            showInEditorProp.boolValue = EditorGUILayout.Foldout(showInEditorProp.boolValue, emptyString, foldoutStyleNoLabel);
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.LabelField(headerLabel, headingFieldRichText);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw sensitivity for Direct keyboard
        /// </summary>
        /// <param name="positiveIntProp"></param>
        /// <param name="negativeIntProp"></param>
        /// <param name="isSensitivityEnabledProp"></param>
        /// <param name="isSensitivityEnabledContent"></param>
        /// <param name="axisSensitivityProp"></param>
        /// <param name="axisSensitivityContent"></param>
        /// <param name="axisGravityProp"></param>
        /// <param name="axisGravityContent"></param>
        private void DrawSensitivity
        (
         SerializedProperty positiveIntProp,
         SerializedProperty negativeIntProp,
         SerializedProperty isSensitivityEnabledProp,
         GUIContent isSensitivityEnabledContent,
         SerializedProperty axisSensitivityProp,
         GUIContent axisSensitivityContent,
         SerializedProperty axisGravityProp,
         GUIContent axisGravityContent
        )
        {
            if (positiveIntProp.intValue != 0 || negativeIntProp.intValue != 0)
            {
                EditorGUILayout.PropertyField(isSensitivityEnabledProp, isSensitivityEnabledContent);
                if (isSensitivityEnabledProp.boolValue)
                {
                    EditorGUILayout.PropertyField(axisSensitivityProp, axisSensitivityContent);
                    EditorGUILayout.PropertyField(axisGravityProp, axisGravityContent);
                }
            }
        }

        /// <summary>
        /// Draw sensitivity for Legacy Unity, UIS, Rewired, Oculus, VIVE
        /// </summary>
        /// <param name="inputAxisModeProp"></param>
        /// <param name="isSensitivityEnabledProp"></param>
        /// <param name="isSensitivityEnabledContent"></param>
        /// <param name="axisSensitivityProp"></param>
        /// <param name="axisSensitivityContent"></param>
        /// <param name="axisGravityProp"></param>
        /// <param name="axisGravityContent"></param>
        private void DrawSensitivity
        (
         SerializedProperty inputAxisModeProp,
         SerializedProperty isSensitivityEnabledProp,
         GUIContent isSensitivityEnabledContent,
         SerializedProperty axisSensitivityProp,
         GUIContent axisSensitivityContent,
         SerializedProperty axisGravityProp,
         GUIContent axisGravityContent
        )
        {
            if (inputAxisModeProp.intValue != (int)StickyInputModule.InputAxisMode.NoInput)
            {
                EditorGUILayout.PropertyField(isSensitivityEnabledProp, isSensitivityEnabledContent);
                if (isSensitivityEnabledProp.boolValue)
                {
                    EditorGUILayout.PropertyField(axisSensitivityProp, axisSensitivityContent);
                    EditorGUILayout.PropertyField(axisGravityProp, axisGravityContent);
                }
            }
        }

        #endregion

        #region Draw methods - Direct Keyboard

        #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER

        /// <summary>
        /// Draw a DirectKeyboard bool axis which also supports CanBeHeld
        /// </summary>
        /// <param name="showInEditorProp"></param>
        /// <param name="foldoutLabelContent"></param>
        /// <param name="keycodeProp"></param>
        /// <param name="canBeHeldProp"></param>
        private void DrawDirectKeyboardBool (SerializedProperty showInEditorProp, GUIContent foldoutLabelContent, SerializedProperty keycodeProp, SerializedProperty canBeHeldProp)
        {
            DrawFoldoutWithLabel(showInEditorProp, foldoutLabelContent);
            if (showInEditorProp.boolValue)
            {
                EditorGUILayout.PropertyField(keycodeProp, singleKeycodeContent);
                EditorGUILayout.PropertyField(canBeHeldProp, canBeHeldContent);
            }
        }

        #endif

        #endregion

        #region Draw methods - Legacy Input
        #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER

        /// <summary>
        /// Draw a legacy input system bool axis which also supports CanBeHeld
        /// </summary>
        /// <param name="showInEditorProp"></param>
        /// <param name="foldoutLabelContent"></param>
        /// <param name="axisNameProp"></param>
        /// <param name="isAxisValidProp"></param>
        /// <param name="canBeHeldProp"></param>
        private void DrawLegacyBool (SerializedProperty showInEditorProp, GUIContent foldoutLabelContent, SerializedProperty axisNameProp, SerializedProperty isAxisValidProp, SerializedProperty canBeHeldProp)
        {
            DrawFoldoutWithLabel(showInEditorProp, foldoutLabelContent);
            if (showInEditorProp.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(axisNameProp, singleAxisNameContent);
                EditorGUILayout.PropertyField(canBeHeldProp, canBeHeldContent);
                if (EditorGUI.EndChangeCheck())
                {
                    isAxisValidProp.boolValue = StickyInputModule.IsLegacyAxisValid(axisNameProp.stringValue, false);
                }
                if (!isAxisValidProp.boolValue)
                {
                    EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                }
            }
        }

        #endif
        #endregion

        #region Private Draw methods - Unity Input System
#if SSC_UIS

        /// <summary>
        /// Unity Input System draw method used for axis.
        /// showSlots should be false for CustomInputs as they will return
        /// all slot values at runtime.
        /// </summary>
        /// <param name="inputAxisModeProp"></param>
        /// <param name="inputAxisModeContent"></param>
        /// <param name="positiveInputActionIdProp"></param>
        /// <param name="positiveInputActionDataSlotProp"></param>
        /// <param name="uisInputACContent"></param>
        /// <param name="uisInputCTypeContent"></param>
        /// <param name="uisInputATypeContent"></param>
        /// <param name="uisInputADataSlotContent"></param>
        /// <param name="uisInputADataSlots2"></param>
        /// <param name="uisInputADataSlots3"></param>
        /// <param name="numActionIDs"></param>
        /// <param name="showSlots">set to false for CustomInputs</param>
        private void DrawUnityInputSystem
        (
         SerializedProperty inputAxisModeProp,
         GUIContent inputAxisModeContent,
         SerializedProperty positiveInputActionIdProp,
         SerializedProperty positiveInputActionDataSlotProp,
         GUIContent uisInputACContent,
         GUIContent uisInputCTypeContent,
         GUIContent uisInputATypeContent,
         GUIContent uisInputADataSlotContent,
         GUIContent[] uisInputADataSlots2,
         GUIContent[] uisInputADataSlots3,
         int numActionIDs, bool showSlots
        )
        {
            // NOTE: uisActionIDs[] and uisActionGUIContent have class-scope and are not passed in as parameters

            EditorGUILayout.PropertyField(inputAxisModeProp, inputAxisModeContent);
            if (inputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.NoInput) { positiveInputActionIdProp.stringValue = string.Empty; }
            else if (inputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.CombinedAxis)
            {
                EditorGUILayout.HelpBox("To create a combined axis input with Unity Input System, configure an Action with a Composite Binding.", MessageType.Error, true);
                positiveInputActionIdProp.stringValue = string.Empty;
            }
            else
            {
                // Find the Action Category for this Action
                int actionIdx = UISGetActionIdx(positiveInputActionIdProp.stringValue);

                int prevId = actionIdx;

                actionIdx = EditorGUILayout.Popup(uisInputACContent, actionIdx, uisActionGUIContent);
                if (actionIdx != prevId)
                {
                    if (actionIdx >= 0 && actionIdx < numActionIDs)
                    {
                        positiveInputActionIdProp.stringValue = uisActionIDs[actionIdx];

                    }
                    else { positiveInputActionIdProp.stringValue = string.Empty; }
                }

                if (actionIdx >= 0 && actionIdx < numActionIDs)
                {
                    UnityEngine.InputSystem.InputAction inputAction = uisPlayerInput.actions.FindAction(positiveInputActionIdProp.stringValue);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(uisInputCTypeContent, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(inputAction == null ? "unknown" : inputAction.type.ToString());
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(uisInputATypeContent, GUILayout.Width(defaultEditorLabelWidth));                    
                    EditorGUILayout.LabelField(inputAction == null ? "unknown" : inputAction.expectedControlType);
                    EditorGUILayout.EndHorizontal();

                    // expectedControlType can be null which may cause us some grief. Somewhere should make a note that this must be set
                    // to work with SSC.

                    // For a list of control types see UnityEngine.InputSystem.Controls.
                    // Library\PackageCache\com.unity.inputsystem@1.0.0-preview\InputSystem\Controls in current project
                    // AnyKeyControl - if any action returns 1.0f, else 0.0f
                    // AxisControl - float can be < -1 and > 1 unless normalize is set
                    // ButtonControl - based on AxisControl with default pressPoint of -1.
                    // DiscreteButtonControl - based on ButtonControl
                    // DoubleControl
                    // DpadControl - vector2
                    // IntegerControl - int
                    // KeyControl - based on ButtonControl
                    // QuaternionControl - Quaternion
                    // StickControl - vector2
                    // TouchControl - can return various things like pressed, screen position, delta position, touchid, pressure etc

                    if (inputAction != null)
                    {
                        if (!showSlots)
                        {
                            // CustomPlayerInputs can use all slots so don't need to show them
                            positiveInputActionDataSlotProp.intValue = 0;
                        }
                        else if (inputAction.expectedControlType == "Button" || inputAction.expectedControlType == "Axis")
                        {
                            positiveInputActionDataSlotProp.intValue = 0;
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(uisInputADataSlotContent, GUILayout.Width(defaultEditorLabelWidth));
                            EditorGUILayout.LabelField("Slot1");
                            EditorGUILayout.EndHorizontal();
                        }
                        else if (inputAction.expectedControlType == "Vector2" || inputAction.expectedControlType == "Dpad")
                        {
                            positiveInputActionDataSlotProp.intValue = EditorGUILayout.Popup(uisInputADataSlotContent, positiveInputActionDataSlotProp.intValue, uisInputADataSlots2) ;
                        }
                        else if (inputAction.expectedControlType == "Vector3")
                        {
                            positiveInputActionDataSlotProp.intValue = EditorGUILayout.Popup(uisInputADataSlotContent, positiveInputActionDataSlotProp.intValue, uisInputADataSlots3);
                        }
                        else { positiveInputActionDataSlotProp.intValue = 0; }
                    }
                }
            }
        }

        /// <summary>
        /// Unity Input System draw method used for buttons
        /// </summary>
        /// <param name="inputEnabledProp"></param>
        /// <param name="inputEnabledContent"></param>
        /// <param name="positiveInputActionIdProp"></param>
        /// <param name="positiveInputActionDataSlotProp"></param>
        /// <param name="uisInputACContent"></param>
        /// <param name="uisInputCTypeContent"></param>
        /// <param name="uisInputATypeContent"></param>
        /// <param name="uisInputADataSlotContent"></param>
        /// <param name="uisInputADataSlots2"></param>
        /// <param name="uisInputADataSlots3"></param>
        /// <param name="numActionIDs"></param>
        private void DrawUnityInputButton
        (
         SerializedProperty inputEnabledProp,
         GUIContent inputEnabledContent,
         SerializedProperty positiveInputActionIdProp,
         SerializedProperty positiveInputActionDataSlotProp,
         SerializedProperty inputCanBeHeldDownProp,
         GUIContent uisInputACContent,
         GUIContent uisInputCTypeContent,
         GUIContent uisInputATypeContent,
         GUIContent uisInputADataSlotContent,
         GUIContent uisInputCanBeHeldDownContent,
         GUIContent[] uisInputADataSlots2,
         GUIContent[] uisInputADataSlots3,
         int numActionIDs
        )
        {
            EditorGUILayout.PropertyField(inputEnabledProp, inputEnabledContent);
            if (!inputEnabledProp.boolValue) { positiveInputActionIdProp.stringValue = string.Empty; }
            else
            {
                // Find the Action Category for this Action
                int actionIdx = UISGetActionIdx(positiveInputActionIdProp.stringValue);

                int prevId = actionIdx;
                actionIdx = EditorGUILayout.Popup(uisInputACContent, actionIdx, uisActionGUIContent);
                if (actionIdx != prevId)
                {
                    if (actionIdx >= 0 && actionIdx < numActionIDs)
                    {
                        positiveInputActionIdProp.stringValue = uisActionIDs[actionIdx];

                    }
                    else { positiveInputActionIdProp.stringValue = string.Empty; }
                }

                if (actionIdx >= 0 && actionIdx < numActionIDs)
                {
                    UnityEngine.InputSystem.InputAction inputAction = uisPlayerInput.actions.FindAction(positiveInputActionIdProp.stringValue);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(uisInputCTypeContent, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(inputAction == null ? "unknown" : inputAction.type.ToString());
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(uisInputATypeContent, GUILayout.Width(defaultEditorLabelWidth));                    
                    EditorGUILayout.LabelField(inputAction == null ? "unknown" : inputAction.expectedControlType);
                    EditorGUILayout.EndHorizontal();

                    // expectedControlType can be null which may cause us some grief. Somewhere should make a note that this must be set
                    // to work with SSC.

                    if (inputAction != null)
                    {
                        if (inputAction.expectedControlType == "Button" || inputAction.expectedControlType == "Axis")
                        {
                            positiveInputActionDataSlotProp.intValue = 0;
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(uisInputADataSlotContent, GUILayout.Width(defaultEditorLabelWidth));
                            EditorGUILayout.LabelField("Slot1");
                            EditorGUILayout.EndHorizontal();
                        }
                        else if (inputAction.expectedControlType == "Vector2" || inputAction.expectedControlType == "Dpad")
                        {
                            positiveInputActionDataSlotProp.intValue = EditorGUILayout.Popup(uisInputADataSlotContent, positiveInputActionDataSlotProp.intValue, uisInputADataSlots2) ;
                        }
                        else if (inputAction.expectedControlType == "Vector3")
                        {
                            positiveInputActionDataSlotProp.intValue = EditorGUILayout.Popup(uisInputADataSlotContent, positiveInputActionDataSlotProp.intValue, uisInputADataSlots3);
                        }
                        else { positiveInputActionDataSlotProp.intValue = 0; }

                        // Docking doesn't have can be held down option
                        if (inputCanBeHeldDownProp != null)
                        {
                            EditorGUILayout.PropertyField(inputCanBeHeldDownProp, uisInputCanBeHeldDownContent);
                        }
                    }
                }
            }
        }

#endif
        #endregion

        #region Private Draw methods - Rewired
#if SSC_REWIRED

        /// <summary>
        /// Rewired Draw method used for Axis
        /// </summary>
        /// <param name="inputAxisModeProp"></param>
        /// <param name="positiveInputActionIdProp"></param>
        /// <param name="negativeInputActionIdProp"></param>
        /// <param name="rwInputACPositiveContent"></param>
        /// <param name="rwInputACNegativeContent"></param>
        /// <param name="rwInputACContent"></param>
        /// <param name="rwInputAIPositiveContent"></param>
        /// <param name="rwInputAINegativeContent"></param>
        /// <param name="rwInputAIContent"></param>
        /// <param name="actionIDsPositive"></param>
        /// <param name="actionIDsNegative"></param>
        /// <param name="ActionGUIContentPositive"></param>
        /// <param name="ActionGUIContentNegative"></param>
        /// <param name="numActionCategories"></param>
        private void DrawRewiredInput
        (
         PIMCategory pimCategory,
         SerializedProperty inputAxisModeProp,
         SerializedProperty positiveInputActionIdProp,
         SerializedProperty negativeInputActionIdProp,
         GUIContent rwInputACPositiveContent,
         GUIContent rwInputACNegativeContent,
         GUIContent rwInputACContent,
         GUIContent rwInputAIPositiveContent,
         GUIContent rwInputAINegativeContent,
         GUIContent rwInputAIContent,
         ref int[] actionIDsPositive,
         ref int[] actionIDsNegative,
         ref GUIContent[] ActionGUIContentPositive,
         ref GUIContent[] ActionGUIContentNegative,
         int numActionCategories
        )
        {
            EditorGUILayout.PropertyField(inputAxisModeProp, inputAxisModeContent);
            if (inputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.NoInput) { }
            else
            {
                bool isCombined = inputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.CombinedAxis;

                // Find the Action Category for this Action
                int actionCategoryID = RWGetActionCategoryID(positiveInputActionIdProp.intValue);

                int prevId = actionCategoryID;
                actionCategoryID = EditorGUILayout.IntPopup(isCombined ? rwInputACPositiveContent : rwInputACContent, actionCategoryID, ActionCategoryGUIContent, actionCategoryIDs);
                if (prevId != actionCategoryID)
                {
                    // Get the new list of actions if switching action categories
                    RWRefreshActions(actionCategoryID, ref actionIDsPositive, ref ActionGUIContentPositive);
                    // Default to first action in category
                    if (actionIDsPositive != null && actionIDsPositive.Length > 0) { positiveInputActionIdProp.intValue = actionIDsPositive[0]; }
                }

                int actionCategoryIndex = rewiredUserData.GetActionCategoryIndex(actionCategoryID);

                // Show list of actions for this category
                if (actionCategoryIndex >= 0 && actionCategoryIndex < numActionCategories && actionCategories[actionCategoryIndex] != dropDownNone)
                {
                    positiveInputActionIdProp.intValue = EditorGUILayout.IntPopup(isCombined ? rwInputAIPositiveContent : rwInputAIContent, positiveInputActionIdProp.intValue, ActionGUIContentPositive, actionIDsPositive);
                }
                else
                {
                    EditorGUILayout.LabelField(isCombined ? rwInputAIPositiveContent : rwInputAIContent);
                }

                if (inputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.CombinedAxis)
                {
                    actionCategoryID = RWGetActionCategoryID(negativeInputActionIdProp.intValue);

                    prevId = actionCategoryID;
                    actionCategoryID = EditorGUILayout.IntPopup(rwInputACNegativeContent, actionCategoryID, ActionCategoryGUIContent, actionCategoryIDs);
                    if (prevId != actionCategoryID)
                    {
                        // Get the new list of actions if switching action categories
                        RWRefreshActions(actionCategoryID, ref actionIDsNegative, ref ActionGUIContentNegative);
                        // Default to first action in category
                        if (actionIDsNegative != null && actionIDsNegative.Length > 0) { negativeInputActionIdProp.intValue = actionIDsNegative[0]; }
                    }

                    actionCategoryIndex = rewiredUserData.GetActionCategoryIndex(actionCategoryID);

                    // Show list of actions for this category
                    if (actionCategoryIndex >= 0 && actionCategoryIndex < numActionCategories && actionCategories[actionCategoryIndex] != dropDownNone)
                    {
                        negativeInputActionIdProp.intValue = EditorGUILayout.IntPopup(rwInputAINegativeContent, negativeInputActionIdProp.intValue, ActionGUIContentNegative, actionIDsNegative);
                    }
                    else
                    {
                        EditorGUILayout.LabelField(rwInputAINegativeContent);
                    }

                    if (positiveInputActionIdProp.intValue == negativeInputActionIdProp.intValue)
                    {
                        EditorGUILayout.HelpBox(rwidenticalAxisIdWarningContent, MessageType.Warning);
                    }
                }
            }
        }

        /// <summary>
        /// Rewired Draw method used for Input Buttons
        /// </summary>
        /// <param name="inputEnabledProp"></param>
        /// <param name="inputActionIdProp"></param>
        /// <param name="inputCanBeHeldDownProp"></param>
        /// <param name="rwInputEnabledContent"></param>
        /// <param name="rwInputACContent"></param>
        /// <param name="rwInputAIContent"></param>
        /// <param name="rwInputCanBeHeldDownContent"></param>
        /// <param name="actionIDs"></param>
        /// <param name="ActionGUIContent"></param>
        /// <param name="numActionCategories"></param>
        private void DrawRewiredInput
        (SerializedProperty inputEnabledProp,
         SerializedProperty inputActionIdProp,
         SerializedProperty inputCanBeHeldDownProp,
         GUIContent rwInputEnabledContent,
         GUIContent rwInputACContent,
         GUIContent rwInputAIContent,
         GUIContent rwInputCanBeHeldDownContent,
         ref int[] actionIDs,
         ref GUIContent[] ActionGUIContent,
         int numActionCategories
        )
        {
            bool prevEnabled = inputEnabledProp.boolValue;
            EditorGUILayout.PropertyField(inputEnabledProp, rwInputEnabledContent);
            // If enabled has been turned off, reset the ActionId
            if (prevEnabled != inputEnabledProp.boolValue && !inputEnabledProp.boolValue) { inputActionIdProp.intValue = -1; }

            if (inputEnabledProp.boolValue)
            {
                // Find the Action Category for this Action
                int actionCategoryID = RWGetActionCategoryID(inputActionIdProp.intValue);

                int prevId = actionCategoryID;
                actionCategoryID = EditorGUILayout.IntPopup(rwInputACContent, actionCategoryID, ActionCategoryGUIContent, actionCategoryIDs);
                if (prevId != actionCategoryID)
                {
                    // Get the new list of actions if switching action categories
                    RWRefreshActions(actionCategoryID, ref actionIDs, ref ActionGUIContent);
                    // Default to first action in category
                    if (actionIDs != null && actionIDs.Length > 0) { inputActionIdProp.intValue = actionIDs[0]; }
                }

                int actionCategoryIndex = rewiredUserData.GetActionCategoryIndex(actionCategoryID);

                // Show list of actions for this category
                if (actionCategoryIndex >= 0 && actionCategoryIndex < numActionCategories && actionCategories[actionCategoryIndex] != dropDownNone)
                {
                    inputActionIdProp.intValue = EditorGUILayout.IntPopup(rwInputAIContent, inputActionIdProp.intValue, ActionGUIContent, actionIDs);
                }
                else
                {
                    EditorGUILayout.LabelField(rwInputAIContent);
                }

                if (inputCanBeHeldDownProp != null)
                {
                    EditorGUILayout.PropertyField(inputCanBeHeldDownProp, rwInputCanBeHeldDownContent);
                }
            }
        }

#endif
        #endregion

        #region Private Draw methods - XR
#if SCSM_XR && SSC_UIS

        /// <summary>
        /// XR draw method used for axis. Gets data from the Unity Input System
        /// showSlots should be false for CustomInputs as they will return
        /// all slot values at runtime.
        /// </summary>
        /// <param name="inputAxisModeProp"></param>
        /// <param name="inputAxisModeContent"></param>
        /// <param name="inputActionAssetProp"></param>
        /// <param name="positiveInputActionMapIdProp"></param>
        /// <param name="positiveInputActionIdProp"></param>
        /// <param name="positiveInputActionDataSlotProp"></param>
        /// <param name="uisInputAMContent"></param>
        /// <param name="uisInputACContent"></param>
        /// <param name="uisInputCTypeContent"></param>
        /// <param name="uisInputATypeContent"></param>
        /// <param name="uisInputADataSlotContent"></param>
        /// <param name="uisInputADataSlots2"></param>
        /// <param name="uisInputADataSlots3"></param>
        /// <param name="numActionMapIDs"></param>
        /// <param name="showSlots">Set to false for CustomInputs</param>
        /// <param name="forceRefresh"></param>
        private void DrawXR
        (
         SerializedProperty inputAxisModeProp,
         GUIContent inputAxisModeContent,
         SerializedProperty inputActionAssetProp,
         SerializedProperty positiveInputActionMapIdProp,
         SerializedProperty positiveInputActionIdProp,
         SerializedProperty positiveInputActionDataSlotProp,
         GUIContent uisInputAMContent,
         GUIContent uisInputACContent,
         GUIContent uisInputCTypeContent,
         GUIContent uisInputATypeContent,
         GUIContent uisInputADataSlotContent,
         GUIContent[] uisInputADataSlots2,
         GUIContent[] uisInputADataSlots3,
         int numActionMapIDs,
         bool showSlots,
         bool forceRefresh
        )
        {
            // NOTE: uisActionIDs[] and uisActionGUIContent have class-scope and are not passed in as parameters

            int actionMapIdx = XRGetActionMapIdx(positiveInputActionMapIdProp.stringValue);

            int prevActionMapId = actionMapIdx;

            // Get the actionMap and refresh the available actions
            actionMapIdx = EditorGUILayout.Popup(uisInputAMContent, actionMapIdx, xrActionMapGUIContent);

            if (actionMapIdx != prevActionMapId)
            {
                if (actionMapIdx >= 0 && actionMapIdx < numActionMapIDs)
                {
                    positiveInputActionMapIdProp.stringValue = xrActionMapIDs[actionMapIdx];

                }
                else { positiveInputActionMapIdProp.stringValue = string.Empty; }
            }

            EditorGUILayout.PropertyField(inputAxisModeProp, inputAxisModeContent);
            if (inputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.NoInput) { positiveInputActionIdProp.stringValue = string.Empty; }
            else if (inputAxisModeProp.intValue == (int)StickyInputModule.InputAxisMode.CombinedAxis)
            {
                EditorGUILayout.HelpBox("To create a combined axis input with Unity Input System for XR, configure an Action with a Composite Binding.", MessageType.Error, true);
                positiveInputActionIdProp.stringValue = string.Empty;
            }
            else
            {
                XRRefreshActions(positiveInputActionMapIdProp.stringValue, forceRefresh);

                int numActionIDs = uisActionIDs == null ? 0 : uisActionIDs.Length;

                // Find the Action Category for this Action
                int actionIdx = UISGetActionIdx(positiveInputActionIdProp.stringValue);

                int prevActionId = actionIdx;

                actionIdx = EditorGUILayout.Popup(uisInputACContent, actionIdx, uisActionGUIContent);
                if (actionIdx != prevActionId)
                {
                    if (actionIdx >= 0 && actionIdx < numActionIDs)
                    {
                        positiveInputActionIdProp.stringValue = uisActionIDs[actionIdx];

                    }
                    else { positiveInputActionIdProp.stringValue = string.Empty; }
                }

                if (actionIdx >= 0 && actionIdx < numActionIDs && inputActionAssetProp.objectReferenceValue != null)
                {
                    // Get the action map for this axis
                    UnityEngine.InputSystem.InputActionMap actionMap = ((UnityEngine.InputSystem.InputActionAsset)inputActionAssetProp.objectReferenceValue).FindActionMap(positiveInputActionMapIdProp.stringValue);

                    if (actionMap != null)
                    {
                        // Find the action within the Unity Input System action map
                        UnityEngine.InputSystem.InputAction inputAction = actionMap.FindAction(positiveInputActionIdProp.stringValue);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(uisInputCTypeContent, GUILayout.Width(defaultEditorLabelWidth));
                        EditorGUILayout.LabelField(inputAction == null ? "unknown" : inputAction.type.ToString());
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(uisInputATypeContent, GUILayout.Width(defaultEditorLabelWidth));
                        EditorGUILayout.LabelField(inputAction == null ? "unknown" : inputAction.expectedControlType);
                        EditorGUILayout.EndHorizontal();

                        // expectedControlType can be null which may cause us some grief. Somewhere should make a note that this must be set
                        // to work with S3D.

                        // For a list of control types see UnityEngine.InputSystem.Controls.
                        // Library\PackageCache\com.unity.inputsystem@1.0.0-preview\InputSystem\Controls in current project
                        // AnyKeyControl - if any action returns 1.0f, else 0.0f
                        // AxisControl - float can be < -1 and > 1 unless normalize is set
                        // ButtonControl - based on AxisControl with default pressPoint of -1.
                        // DiscreteButtonControl - based on ButtonControl
                        // DoubleControl
                        // DpadControl - vector2
                        // IntegerControl - int
                        // KeyControl - based on ButtonControl
                        // QuaternionControl - Quaternion
                        // StickControl - vector2
                        // TouchControl - can return various things like pressed, screen position, delta position, touchid, pressure etc

                        if (inputAction != null)
                        {
                            if (!showSlots)
                            {
                                // CustomPlayerInputs can use all slots so don't need to show them
                                positiveInputActionDataSlotProp.intValue = 0;
                            }
                            else if (inputAction.expectedControlType == "Button" || inputAction.expectedControlType == "Axis")
                            {
                                positiveInputActionDataSlotProp.intValue = 0;
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(uisInputADataSlotContent, GUILayout.Width(defaultEditorLabelWidth));
                                EditorGUILayout.LabelField("Slot1");
                                EditorGUILayout.EndHorizontal();
                            }
                            else if (inputAction.expectedControlType == "Vector2" || inputAction.expectedControlType == "Dpad")
                            {
                                positiveInputActionDataSlotProp.intValue = EditorGUILayout.Popup(uisInputADataSlotContent, positiveInputActionDataSlotProp.intValue, uisInputADataSlots2);
                            }
                            else if (inputAction.expectedControlType == "Vector3")
                            {
                                positiveInputActionDataSlotProp.intValue = EditorGUILayout.Popup(uisInputADataSlotContent, positiveInputActionDataSlotProp.intValue, uisInputADataSlots3);
                            }
                            else { positiveInputActionDataSlotProp.intValue = 0; }
                        }
                    }
                    else
                    {
                        positiveInputActionIdProp.stringValue = string.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// XR draw method used for button response.
        /// </summary>
        /// <param name="inputEnabledProp"></param>
        /// <param name="inputEnabledContent"></param>
        /// <param name="inputActionAssetProp"></param>
        /// <param name="positiveInputActionMapIdProp"></param>
        /// <param name="positiveInputActionIdProp"></param>
        /// <param name="positiveInputActionDataSlotProp"></param>
        /// <param name="inputCanBeHeldDownProp"></param>
        /// <param name="uisInputAMContent"></param>
        /// <param name="uisInputACContent"></param>
        /// <param name="uisInputCTypeContent"></param>
        /// <param name="uisInputATypeContent"></param>
        /// <param name="uisInputADataSlotContent"></param>
        /// <param name="uisInputCanBeHeldDownContent"></param>
        /// <param name="uisInputADataSlots2"></param>
        /// <param name="uisInputADataSlots3"></param>
        /// <param name="numActionMapIDs"></param>
        /// <param name="forceRefresh"></param>
        private void DrawXRButton
        (
         SerializedProperty inputEnabledProp,
         GUIContent inputEnabledContent,
         SerializedProperty inputActionAssetProp,
         SerializedProperty positiveInputActionMapIdProp,
         SerializedProperty positiveInputActionIdProp,
         SerializedProperty positiveInputActionDataSlotProp,
         SerializedProperty inputCanBeHeldDownProp,
         GUIContent uisInputAMContent,
         GUIContent uisInputACContent,
         GUIContent uisInputCTypeContent,
         GUIContent uisInputATypeContent,
         GUIContent uisInputADataSlotContent,
         GUIContent uisInputCanBeHeldDownContent,
         GUIContent[] uisInputADataSlots2,
         GUIContent[] uisInputADataSlots3,
         int numActionMapIDs,
         bool forceRefresh
        )
        {
            int actionMapIdx = XRGetActionMapIdx(positiveInputActionMapIdProp.stringValue);

            int prevActionMapId = actionMapIdx;

            // Get the actionMap and refresh the available actions
            actionMapIdx = EditorGUILayout.Popup(uisInputAMContent, actionMapIdx, xrActionMapGUIContent);

            if (actionMapIdx != prevActionMapId)
            {
                if (actionMapIdx >= 0 && actionMapIdx < numActionMapIDs)
                {
                    positiveInputActionMapIdProp.stringValue = xrActionMapIDs[actionMapIdx];

                }
                else { positiveInputActionMapIdProp.stringValue = string.Empty; }
            }

            EditorGUILayout.PropertyField(inputEnabledProp, inputEnabledContent);
            if (!inputEnabledProp.boolValue) { positiveInputActionIdProp.stringValue = string.Empty; }
            else
            {
                XRRefreshActions(positiveInputActionMapIdProp.stringValue, forceRefresh);

                int numActionIDs = uisActionIDs == null ? 0 : uisActionIDs.Length;

                // Find the Action Category for this Action
                int actionIdx = UISGetActionIdx(positiveInputActionIdProp.stringValue);

                int prevId = actionIdx;
                actionIdx = EditorGUILayout.Popup(uisInputACContent, actionIdx, uisActionGUIContent);
                if (actionIdx != prevId)
                {
                    if (actionIdx >= 0 && actionIdx < numActionIDs)
                    {
                        positiveInputActionIdProp.stringValue = uisActionIDs[actionIdx];

                    }
                    else { positiveInputActionIdProp.stringValue = string.Empty; }
                }

                if (actionIdx >= 0 && actionIdx < numActionIDs && inputActionAssetProp.objectReferenceValue != null)
                {
                    // Get the action map for this axis
                    UnityEngine.InputSystem.InputActionMap actionMap = ((UnityEngine.InputSystem.InputActionAsset)inputActionAssetProp.objectReferenceValue).FindActionMap(positiveInputActionMapIdProp.stringValue);

                    if (actionMap != null)
                    {
                        UnityEngine.InputSystem.InputAction inputAction = actionMap.FindAction(positiveInputActionIdProp.stringValue);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(uisInputCTypeContent, GUILayout.Width(defaultEditorLabelWidth));
                        EditorGUILayout.LabelField(inputAction == null ? "unknown" : inputAction.type.ToString());
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(uisInputATypeContent, GUILayout.Width(defaultEditorLabelWidth));
                        EditorGUILayout.LabelField(inputAction == null ? "unknown" : inputAction.expectedControlType);
                        EditorGUILayout.EndHorizontal();

                        // expectedControlType can be null which may cause us some grief. Somewhere should make a note that this must be set
                        // to work with SSC.

                        if (inputAction != null)
                        {
                            if (inputAction.expectedControlType == "Button" || inputAction.expectedControlType == "Axis")
                            {
                                positiveInputActionDataSlotProp.intValue = 0;
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(uisInputADataSlotContent, GUILayout.Width(defaultEditorLabelWidth));
                                EditorGUILayout.LabelField("Slot1");
                                EditorGUILayout.EndHorizontal();
                            }
                            else if (inputAction.expectedControlType == "Vector2" || inputAction.expectedControlType == "Dpad")
                            {
                                positiveInputActionDataSlotProp.intValue = EditorGUILayout.Popup(uisInputADataSlotContent, positiveInputActionDataSlotProp.intValue, uisInputADataSlots2);
                            }
                            else if (inputAction.expectedControlType == "Vector3")
                            {
                                positiveInputActionDataSlotProp.intValue = EditorGUILayout.Popup(uisInputADataSlotContent, positiveInputActionDataSlotProp.intValue, uisInputADataSlots3);
                            }
                            else { positiveInputActionDataSlotProp.intValue = 0; }

                            // Docking doesn't have can be held down option
                            if (inputCanBeHeldDownProp != null)
                            {
                                EditorGUILayout.PropertyField(inputCanBeHeldDownProp, uisInputCanBeHeldDownContent);
                            }
                        }
                    }
                    else
                    {
                        positiveInputActionIdProp.stringValue = string.Empty;
                    }
                }
            }
        }

#endif
        #endregion

        #region Private Rewired Methods
#if SSC_REWIRED

        /// <summary>
        /// Find the rewired Input Manager in the scene (if there is one).
        /// If found, get a reference to the userdata.
        /// </summary>
        private void FindRewiredInputManager()
        {
            #if UNITY_2022_2_OR_NEWER
            rewiredInputManager = Object.FindFirstObjectByType<Rewired.InputManager>();
            #else
            rewiredInputManager = Object.FindObjectOfType<Rewired.InputManager>();
            #endif

            if (rewiredInputManager != null)
            {
                rewiredUserData = rewiredInputManager.userData;
                rwVersion = ReInput.programVersion;
            }
            else { rwVersion = string.Empty; }
        }

        /// <summary>
        /// Get the Rewired Action Category an Action belongs to
        /// </summary>
        /// <param name="actionId"></param>
        /// <returns></returns>
        private int RWGetActionCategoryID(int actionId)
        {
            if (rewiredUserData != null)
            {
                InputAction inputAction = rewiredUserData.GetActionById(actionId);
                if (inputAction == null) { return -1; }
                else { return inputAction.categoryId; }
            }
            else { return -1; }
        }

        /// <summary>
        /// Refresh the Rewired action arrays supplied for a given Action Category
        /// </summary>
        /// <param name="actionCategoryId"></param>
        /// <param name="actionIDs"></param>
        /// <param name="actionGUIContents"></param>
        private void RWRefreshActions(int actionCategoryId, ref int[] actionIDs, ref GUIContent[] actionGUIContents)
        {
            if (actionIDList == null || ActionGUIContentList == null) { return; }

            // Empty the re-usable lists
            actionIDList.Clear();
            ActionGUIContentList.Clear();

            // Basic validation
            if (rewiredUserData != null && actionCategoryId >= 0 && actionIDs != null && actionGUIContents != null)
            {
                int[] actionIds = rewiredUserData.GetSortedActionIdsInCategory(actionCategoryId);
                int numActions = actionIds == null ? 0 : actionIds.Length;

                for (int actionIdx = 0; actionIdx < numActions; actionIdx++)
                {
                    InputAction inputAction = rewiredUserData.GetActionById(actionIds[actionIdx]);
  
                    if (inputAction != null && inputAction.userAssignable)
                    {
                        actionIDList.Add(inputAction.id);
                        ActionGUIContentList.Add(new GUIContent(inputAction.name, inputAction.descriptiveName));

                        //Debug.Log("Adding " + inputAction.name);
                    }
                }
            }

            if (actionIDList.Count > 0)
            {
                actionIDs = actionIDList.ToArray();
                actionGUIContents = ActionGUIContentList.ToArray();
            }
            else
            {
                actionIDs = new int[] { 0 };
                actionGUIContents = new GUIContent[] { new GUIContent(dropDownNone) };
            }
        }

        /// <summary>
        /// Update the arrays for all the user-defined Rewired Actions. These populate the dropdown
        /// lists in the editor.
        /// </summary>
        private void RWRefreshActionsAll()
        {
            if (rewiredUserData != null)
            {
                // To build the lists, the Action Category is required for each list

                actionIDsHztlMovePositive = new int[1];
                ActionGUIContentHztlMovePositive = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.posHztlMoveInputActionIdRWD), ref actionIDsHztlMovePositive, ref ActionGUIContentHztlMovePositive);

                actionIDsHztlMoveNegative = new int[1];
                ActionGUIContentHztlMoveNegative = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.negHztlMoveInputActionIdRWD), ref actionIDsHztlMoveNegative, ref ActionGUIContentHztlMoveNegative);

                actionIDsVertMovePositive = new int[1];
                ActionGUIContentVertMovePositive = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.posVertMoveInputActionIdRWD), ref actionIDsVertMovePositive, ref ActionGUIContentVertMovePositive);

                actionIDsVertMoveNegative = new int[1];
                ActionGUIContentVertMoveNegative = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.negVertMoveInputActionIdRWD), ref actionIDsVertMoveNegative, ref ActionGUIContentVertMoveNegative);

                actionIDsHztlLookPositive = new int[1];
                ActionGUIContentHztlLookPositive = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.posHztlLookInputActionIdRWD), ref actionIDsHztlLookPositive, ref ActionGUIContentHztlLookPositive);

                actionIDsHztlLookNegative = new int[1];
                ActionGUIContentHztlLookNegative = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.negHztlLookInputActionIdRWD), ref actionIDsHztlLookNegative, ref ActionGUIContentHztlLookNegative);

                actionIDsVertLookPositive = new int[1];
                ActionGUIContentVertLookPositive = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.posVertLookInputActionIdRWD), ref actionIDsVertLookPositive, ref ActionGUIContentVertLookPositive);

                actionIDsVertLookNegative = new int[1];
                ActionGUIContentVertLookNegative = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.negVertLookInputActionIdRWD), ref actionIDsVertLookNegative, ref ActionGUIContentVertLookNegative);

                actionIDsZoomLookPositive = new int[1];
                ActionGUIContentZoomLookPositive = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.posZoomLookInputActionIdRWD), ref actionIDsZoomLookPositive, ref ActionGUIContentZoomLookPositive);

                actionIDsZoomLookNegative = new int[1];
                ActionGUIContentZoomLookNegative = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.negZoomLookInputActionIdRWD), ref actionIDsZoomLookNegative, ref ActionGUIContentZoomLookNegative);

                actionIDsOrbitLookPositive = new int[1];
                ActionGUIContentOrbitLookPositive = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.posOrbitLookInputActionIdRWD), ref actionIDsOrbitLookPositive, ref ActionGUIContentOrbitLookPositive);

                actionIDsOrbitLookNegative = new int[1];
                ActionGUIContentOrbitLookNegative = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.negOrbitLookInputActionIdRWD), ref actionIDsOrbitLookNegative, ref ActionGUIContentOrbitLookNegative);

                actionIDsJump = new int[1];
                ActionGUIContentJump = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.jumpInputActionIdRWD), ref actionIDsJump, ref ActionGUIContentJump);

                actionIDsSprint = new int[1];
                ActionGUIContentSprint = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.sprintInputActionIdRWD), ref actionIDsSprint, ref ActionGUIContentSprint);

                actionIDsCrouch = new int[1];
                ActionGUIContentCrouch = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.crouchInputActionIdRWD), ref actionIDsCrouch, ref ActionGUIContentCrouch);

                actionIDsJetPack = new int[1];
                ActionGUIContentJetPack = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.jetpackInputActionIdRWD), ref actionIDsJetPack, ref ActionGUIContentJetPack);

                actionIDsSwitchLook = new int[1];
                ActionGUIContentSwitchLook = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.switchLookInputActionIdRWD), ref actionIDsSwitchLook, ref ActionGUIContentSwitchLook);

                actionIDsLeftFire1 = new int[1];
                ActionGUIContentLeftFire1 = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.leftFire1InputActionIdRWD), ref actionIDsLeftFire1, ref ActionGUIContentLeftFire1);

                actionIDsLeftFire2 = new int[1];
                ActionGUIContentLeftFire2 = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.leftFire2InputActionIdRWD), ref actionIDsLeftFire2, ref ActionGUIContentLeftFire2);

                actionIDsRightFire1 = new int[1];
                ActionGUIContentRightFire1 = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.rightFire1InputActionIdRWD), ref actionIDsRightFire1, ref ActionGUIContentRightFire1);

                actionIDsRightFire2 = new int[1];
                ActionGUIContentRightFire2 = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(stickyInputModule.rightFire2InputActionIdRWD), ref actionIDsRightFire2, ref ActionGUIContentRightFire2);

                int numCI = stickyInputModule.customInputList == null ? 0 : stickyInputModule.customInputList.Count;

                for (int ciIdx = 0; ciIdx < numCI; ciIdx++)
                {
                    CustomInput customInput = stickyInputModule.customInputList[ciIdx];

                    if (customInput != null)
                    {
                        customInput.actionIDsPositive = new int[1];
                        customInput.ActionGUIContentPositive = new GUIContent[1];
                        RWRefreshActions(RWGetActionCategoryID(customInput.rwdPositiveInputActionId), ref customInput.actionIDsPositive, ref customInput.ActionGUIContentPositive);

                        customInput.actionIDsNegative = new int[1];
                        customInput.ActionGUIContentNegative = new GUIContent[1];
                        RWRefreshActions(RWGetActionCategoryID(customInput.rwdNegativeInputActionId), ref customInput.actionIDsNegative, ref customInput.ActionGUIContentNegative);
                    }
                }
            }
            else
            {
                actionIDsHztlMovePositive = null;
                ActionGUIContentHztlMovePositive = null;
                actionIDsHztlMoveNegative = null;
                ActionGUIContentHztlMoveNegative = null;

                actionIDsVertMovePositive = null;
                ActionGUIContentVertMovePositive = null;
                actionIDsVertMoveNegative = null;
                ActionGUIContentVertMoveNegative = null;

                actionIDsHztlLookPositive = null;
                ActionGUIContentHztlLookPositive = null;
                actionIDsHztlLookNegative = null;
                ActionGUIContentHztlLookNegative = null;

                actionIDsVertLookPositive = null;
                ActionGUIContentVertLookPositive = null;
                actionIDsVertLookNegative = null;
                ActionGUIContentVertLookNegative = null;

                actionIDsZoomLookPositive = null;
                ActionGUIContentZoomLookPositive = null;
                actionIDsZoomLookNegative = null;
                ActionGUIContentZoomLookNegative = null;

                actionIDsVertLookPositive = null;
                ActionGUIContentVertLookPositive = null;
                actionIDsVertLookNegative = null;
                ActionGUIContentVertLookNegative = null;

                actionIDsOrbitLookPositive = null;
                ActionGUIContentOrbitLookPositive = null;
                actionIDsOrbitLookNegative = null;
                ActionGUIContentOrbitLookNegative = null;

                actionIDsJump = null;
                ActionGUIContentJump = null;

                actionIDsSprint = null;
                ActionGUIContentSprint = null;

                actionIDsCrouch = null;
                ActionGUIContentCrouch = null;

                actionIDsJetPack = null;
                ActionGUIContentJetPack = null;

                actionIDsSwitchLook = null;
                ActionGUIContentSwitchLook = null;

                actionIDsLeftFire1 = null;
                ActionGUIContentLeftFire1 = null;

                actionIDsLeftFire2 = null;
                ActionGUIContentLeftFire2 = null;

                actionIDsRightFire1 = null;
                ActionGUIContentRightFire1 = null;

                actionIDsRightFire2 = null;
                ActionGUIContentRightFire2 = null;

                int numCPI = stickyInputModule.customInputList == null ? 0 : stickyInputModule.customInputList.Count;

                for (int cpiIdx = 0; cpiIdx < numCPI; cpiIdx++)
                {
                    CustomInput customInput = stickyInputModule.customInputList[cpiIdx];

                    if (customInput != null)
                    {
                        customInput.actionIDsPositive = null;
                        customInput.actionIDsNegative = null;
                        customInput.ActionGUIContentPositive = null;
                        customInput.ActionGUIContentNegative = null;
                    }
                }
            }
        }

        /// <summary>
        /// Populate arrays for the list of Action Categories
        /// </summary>
        private void RWRefreshActionCategories()
        {
            List<string> actionCategoriesList = new List<string>(5);
            List<int> actionCategoryIDList = new List<int>(5);
            List<GUIContent> ActionCategoryGUIContentList = new List<GUIContent>(5);

            if (rewiredUserData != null)
            {
                int[] actionCategoryIds = rewiredUserData.GetActionCategoryIds();

                int numCategories = actionCategoryIds == null ? 0 : actionCategoryIds.Length;

                for (int catIdx = 0; catIdx < numCategories; catIdx++)
                {
                    InputCategory inputCategory = rewiredUserData.GetActionCategoryById(actionCategoryIds[catIdx]);

                    if (inputCategory != null && inputCategory.userAssignable)
                    {
                        actionCategoriesList.Add(inputCategory.name);
                        actionCategoryIDList.Add(inputCategory.id);
                        ActionCategoryGUIContentList.Add(new GUIContent(inputCategory.name, inputCategory.descriptiveName));
                    }
                }
            }

            if (actionCategoriesList.Count > 0)
            {
                actionCategories = actionCategoriesList.ToArray();
                actionCategoryIDs = actionCategoryIDList.ToArray();
                ActionCategoryGUIContent = ActionCategoryGUIContentList.ToArray();
            }
            else
            {
                actionCategories = new string[] { dropDownNone };
                actionCategoryIDs = new int[] { 0 };
                ActionCategoryGUIContent = new GUIContent[] { new GUIContent(dropDownNone) };
            }
        }

#endif
        #endregion

        #region Private XR Methods
#if SCSM_XR && SSC_UIS

        /// <summary>
        /// Create a hand transform under "XR Hands Offset"
        /// </summary>
        /// <param name="handProperty"></param>
        /// <param name="isLefthand"></param>
        private void XRCreateHand(SerializedProperty handProperty, bool isLefthand)
        {
            // Check if it already exists to help with Undo.
            Transform xrHandsTrfm = stickyInputModule.transform.Find(xrHandsOffsetName);

            if (xrHandsTrfm == null)
            {
                xrHandsTrfm = S3DUtils.GetOrCreateChildTransform(stickyInputModule.transform, xrHandsOffsetName);

                if (xrHandsTrfm != null)
                {
                    Undo.RegisterCreatedObjectUndo(xrHandsTrfm.gameObject, string.Empty);
                    xrHandsTrfm.localPosition = Vector3.zero;
                    xrHandsTrfm.localRotation = Quaternion.identity;
                }
            }

            if (xrHandsTrfm != null)
            {
                // Check if it already exists to help with Undo.
                Transform xrHandTrfm = xrHandsTrfm.Find(isLefthand ? xrLeftHandName : xrRightHandName);

                if (xrHandTrfm == null)
                {
                    xrHandTrfm = S3DUtils.GetOrCreateChildTransform(xrHandsTrfm, isLefthand ? xrLeftHandName : xrRightHandName);
                    Undo.RegisterCreatedObjectUndo(xrHandTrfm.gameObject, string.Empty);

                    xrHandTrfm.localPosition = Vector3.zero;
                    xrHandTrfm.localRotation = Quaternion.identity;

                    // Use our own solution rather than a TrackPoseDriver. See StickyInputModule.MoveXRHands().
                    //// Distinguish between the old XR TrackPoseDriver and the one with the new Unity Input System.
                    //var _tposeDriver = Undo.AddComponent(xrHandTrfm.gameObject, typeof(UnityEngine.InputSystem.XR.TrackedPoseDriver)) as UnityEngine.InputSystem.XR.TrackedPoseDriver;

                    //if (_tposeDriver != null)
                    //{
                    //    _tposeDriver.trackingType = UnityEngine.InputSystem.XR.TrackedPoseDriver.TrackingType.RotationAndPosition;

                    //    UnityEngine.InputSystem.InputAction _tposPosition = new UnityEngine.InputSystem.InputAction(null, UnityEngine.InputSystem.InputActionType.Value);
                    //    if (_tposPosition != null)
                    //    {
                    //        //_tposPosition.expectedControlType = "Quaternion";
                    //        _tposeDriver.positionAction = _tposPosition;
                    //    }

                    //    UnityEngine.InputSystem.InputAction _tposRotation = new UnityEngine.InputSystem.InputAction(null, UnityEngine.InputSystem.InputActionType.Value);
                    //    if (_tposRotation != null)
                    //    {
                    //        _tposRotation.expectedControlType = "Quaternion";
                    //        _tposeDriver.rotationAction = _tposRotation;
                    //    }
                    //}
                }

                if (xrHandTrfm != null)
                {
                    handProperty.objectReferenceValue = xrHandTrfm;
                }
            }
        }

        /// <summary>
        /// Get the index of the action map in the list of xrActionMapIDs.
        /// </summary>
        /// <param name="actionId"></param>
        /// <returns></returns>
        private int XRGetActionMapIdx(string actionMapId)
        {
            if (xrActionMapIDs != null)
            {
                return ArrayUtility.FindIndex<string>(xrActionMapIDs, s => s == actionMapId);
            }
            else { return -1; }
        }

        /// <summary>
        /// Refresh the list of Actions available for selected Action Map
        /// </summary>
        private void XRRefreshActions(string actionMapId, bool forceRefresh)
        {
            bool hasChanged = string.Compare(actionMapId, xrCurrentActionMapId, false) != 0;

            if (hasChanged || forceRefresh)
            {
                xrCurrentActionMapId = string.Copy(actionMapId);

                // cache the lists to reduce GC in the editor at runtime
                if (xrActionNameList == null) { xrActionNameList = new List<string>(10); }
                else { xrActionNameList.Clear(); }
                if (xrActionIDList == null) { xrActionIDList = new List<string>(10); }
                else { xrActionIDList.Clear(); }
                if (xrActionGUIContentList == null) { xrActionGUIContentList = new List<GUIContent>(10); }
                else { xrActionGUIContentList.Clear(); }

                UnityEngine.InputSystem.InputActionAsset inputActionAsset = stickyInputModule.GetXRInputActionAsset();
                UnityEngine.InputSystem.InputActionMap actionMap = inputActionAsset == null ? null : inputActionAsset.FindActionMap(actionMapId);

                if (actionMap != null)
                {
                    // This readonly array can not be null but the map can be.
                    // The map can be null when it was previously set in PlayerInput but was deleted in the UIS PlayerInput Editor.
                    int numActions = actionMap == null ? 0 : actionMap.actions.Count;

                    for (int actIdx = 0; actIdx < numActions; actIdx++)
                    {
                        UnityEngine.InputSystem.InputAction action = actionMap.actions[actIdx];
                        xrActionNameList.Add(action.name);
                        xrActionIDList.Add(action.id.ToString());
                        xrActionGUIContentList.Add(new GUIContent(action.name));
                    }
                }

                // Populate the lookup arrays and the GUIContent used in the dropdown list
                if (xrActionGUIContentList.Count > 0)
                {
                    uisActionGUIContent = xrActionGUIContentList.ToArray();
                    uisActionIDs = xrActionIDList.ToArray();
                    uisActionNames = xrActionNameList.ToArray();
                }
                else
                {
                    uisActionGUIContent = new GUIContent[] { new GUIContent(uisDropDownNone) };
                    uisActionNames = new string[] { uisDropDownNone };
                    uisActionIDs = new string[] { "" };
                }
            }
        }

        /// <summary>
        /// Refresh the list of action maps available in the InputActionAsset assigned to the XR inputmode
        /// </summary>
        private void XRRefreshActionMaps()
        {
            if (inputActionAssetXRProp != null && inputActionAssetXRProp.objectReferenceValue != null)
            {
                UnityEngine.InputSystem.InputActionAsset inputActions = (UnityEngine.InputSystem.InputActionAsset)inputActionAssetXRProp.objectReferenceValue;

                var actionMaps = inputActions.actionMaps;

                xrActionMapGUIContent = new GUIContent[actionMaps.Count + 1];
                xrActionMapIDs = new string[actionMaps.Count + 1];
                xrActionMapGUIContent[0] = new GUIContent(EditorGUIUtility.TrTextContent(uisDropDownNone));
                xrActionMapIDs[0] = string.Empty;

                for (int i = 0; i < actionMaps.Count; ++i)
                {
                    var actionMap = actionMaps[i];
                    xrActionMapGUIContent[i + 1] = new GUIContent(actionMap.name);
                    xrActionMapIDs[i + 1] = actionMap.id.ToString();
                }
            }
            else
            {
                xrActionMapGUIContent = new GUIContent[1];
                xrActionMapGUIContent[0] = new GUIContent(EditorGUIUtility.TrTextContent(uisDropDownNone));
            }
        }
#endif
        #endregion
    }
}