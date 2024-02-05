using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if SSC_REWIRED
using Rewired;
#endif
#if VIU_PLUGIN
using HTC.UnityPlugin.Vive;
#endif
#if SCSM_XR
using UnityEngine.XR;
// using UnityEngine.XR.Management; // NOT IN 2021.2+
#endif

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(PlayerInputModule))]
    public class PlayerInputModuleEditor : Editor
    {
        #region Enumerations

        /// <summary>
        /// Used internally in PlayerInputModuleEditor only
        /// </summary>
        public enum PIMCategory
        {
            Horizontal = 0,
            Vertical = 1,
            Longitidinal = 2,
            Pitch = 3,
            Yaw = 4,
            Roll = 5,
            PrimaryFire = 10,
            SecondaryFire = 11,
            CustomPlayerInput = 21
        };

        #endregion

        #region Custom Editor private variables
        private PlayerInputModule playerInputModule = null;
        private bool isStylesInitialised = false;
        private bool isRefreshing = false;
        private bool isDebuggingEnabled = false;
        private ShipInput shipInput = null;  // used for debugging
        private ShipControlModule shipControlModule = null;

        private readonly static string emptyString = "";

        // Formatting and style variables
        //private string txtColourName = "Black";
        //private Color defaultTextColour = Color.black;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle headingFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle foldoutStyleNoLabel;
        private GUIStyle buttonCompact;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;

        #if SSC_OVR
        private string ovrPluginVersion = "";
        #endif

        #if SSC_UIS
        // Unity Input System variables
        // Ref to a Player Input component that should be attached to the same gameobject
        // Refresh this by calling playerInputModule.GetUISPlayerInput().
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
        private string[] actionCategories;
        private int[] actionCategoryIDs;
        private GUIContent[] ActionCategoryGUIContent;
        private int[] actionIDsHorizPositive, actionIDsHorizNegative;
        private GUIContent[] ActionGUIContentHorizPositive, ActionGUIContentHorizNegative;
        private int[] actionIDsVertPositive, actionIDsVertNegative;
        private GUIContent[] ActionGUIContentVertPositive, ActionGUIContentVertNegative;
        private int[] actionIDsLngtdlPositive, actionIDsLngtdlNegative;
        private GUIContent[] ActionGUIContentLngtdlPositive, ActionGUIContentLngtdlNegative;
        private int[] actionIDsPitchPositive, actionIDsPitchNegative;
        private GUIContent[] ActionGUIContentPitchPositive, ActionGUIContentPitchNegative;
        private int[] actionIDsYawPositive, actionIDsYawNegative;
        private GUIContent[] ActionGUIContentYawPositive, ActionGUIContentYawNegative;
        private int[] actionIDsRollPositive, actionIDsRollNegative;
        private GUIContent[] ActionGUIContentRollPositive, ActionGUIContentRollNegative;
        private int[] actionIDsPrimaryFire, actionIDsSecondaryFire, actionIDsDocking;
        private GUIContent[] ActionGUIContentPrimaryFire, ActionGUIContentSecondaryFire, ActionGUIContentDocking;

        private Rewired.InputManager rewiredInputManager = null;
        private Rewired.Data.UserData rewiredUserData = null;
        // Re-usable list to construct rewired action arrays
        private List<int> actionIDList = new List<int>(20);
        private List<GUIContent> ActionGUIContentList = new List<GUIContent>(20);
        private readonly static string dropDownNone = "None";
        #endif

        #if VIU_PLUGIN
        private string viveVersion = "";
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

        // custom player input variables
        private int cpiDeletePos = -1;

        #endregion

        #region Static GUIContent

        #region GUIContent - Headers

        private readonly static GUIContent headerContent = new GUIContent("Player Input Module\n\nThis module enables you to " +
            "map inputs from different input systems to the six axes of the ship: Horizontal (left/right movement), " +
            "Vertical (up/down movement), Longitudinal (forwards/backwards movement), Pitch (rotation on the local " +
            "x-axis), Yaw (rotation on the local y-axis) and Roll (rotation on the local z-axis). It also enables you to " +
            "map inputs for the weapon system and call custom code from any supported input type.");
        private readonly static GUIContent inputModeContent = new GUIContent(" Input Mode", "How input is received from the user. When " +
            "Direct Keyboard is selected, keyboard is received directly from the keyboard. When Legacy Unity is selected, input is received " +
            "from the axes defined in the legacy input manager.");
        private readonly static GUIContent horizontalHeaderContent = new GUIContent("Horizontal Input Axis");
        private readonly static GUIContent verticalHeaderContent = new GUIContent("Vertical Input Axis");
        private readonly static GUIContent longitudinalHeaderContent = new GUIContent("Longitudinal Input Axis");
        private readonly static GUIContent pitchHeaderContent = new GUIContent("Pitch Input Axis");
        private readonly static GUIContent yawHeaderContent = new GUIContent("Yaw Input Axis");
        private readonly static GUIContent rollHeaderContent = new GUIContent("Roll Input Axis");
        private readonly static GUIContent primaryFireHeaderContent = new GUIContent("Primary Fire Input Button");
        private readonly static GUIContent secondaryFireHeaderContent = new GUIContent("Secondary Fire Input Button");
        private readonly static GUIContent dockingHeaderContent = new GUIContent("Docking Input Button");
        private readonly static GUIContent rumbleHeaderContent = new GUIContent("Rumble Device Output");
        private readonly static GUIContent cpiHeaderContent = new GUIContent("Custom Player Inputs");

        #endregion GUIContent - Headers

        #region GUIContent - General

        private readonly static GUIContent initialiseOnAwakeContent = new GUIContent(" Initialise on Awake", "If enabled, Initialise() will be called as soon as Awake() runs. This should be disabled if you want to control when the Player Input Module is enabled through code. [Default: ON]");
        private readonly static GUIContent isEnabledOnInitialiseContent = new GUIContent(" Enable on Initialise", "Is input enabled when the module is first initialised?  See also EnableInput() and DisableInput(). [Default: ON]");
        private readonly static GUIContent isCustomInputsOnlyOnInitialiseContent = new GUIContent(" Custom Inputs Only", "Are only the custom player inputs enabled when the module is first initialised? [Default: OFF]");
        private readonly static GUIContent isAutoCruiseEnabledContent = new GUIContent(" Auto Cruise", "The ship will attempt to maintain the same forward speed based on the last player longitudinal input [Default: OFF]");
        private readonly static string brakeAssistWarningContent = "Auto Cruise overrides Brake Flight Assist when moving forwards";
        private readonly static GUIContent fireCanBeHeldContent = new GUIContent(" Can Be Held", "When enabled, holding the key or button down will cause fire events to be sent continuously.");
        private readonly static GUIContent maxRumble1Content = new GUIContent(" Max Rumble Motor 1", "The maximum output to the first device rumble motor. On a gamepad this would typically be the left low frequency force feedback or vibration motor.");
        private readonly static GUIContent maxRumble2Content = new GUIContent(" Max Rumble Motor 2", "The maximum output to the second device rumble motor. On a gamepad this would typically be the right high frequency force feedback or vibration motor.");
        private readonly static GUIContent sensitivityEnabledContent = new GUIContent(" Use Sensitivity", "Should axis sensitivity and gravity be applied to this axis?");
        private readonly static GUIContent sensitivityContent = new GUIContent(" Sensitivity", "Speed to move towards target values. Lower values make it less sensitive");
        private readonly static GUIContent gravityContent = new GUIContent(" Gravity", "The rate at which the values return to the middle or neutral position");
        private readonly static GUIContent discardDataContent = new GUIContent(" Override in Code", "[DEFAULT: OFF] Discard the data from this axis or button and update in custom code with shipControlModule.SendInput(..). See manual for help.");
        private readonly static GUIContent cpiEventContent = new GUIContent(" Callback Method", "This is your method that gets called when the input event occurs.");
        private readonly static GUIContent cpiIsButtonContent = new GUIContent(" Is Button", "Is this input a pressable button");
        private readonly static GUIContent cpiCanBeHeldDownContent = new GUIContent(" Can Be Held Down", "Can this button be continuously held down?");

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

        private readonly static GUIContent dkiPitchInputMouseContent = new GUIContent(" Use Mouse");
        private readonly static GUIContent dkiYawInputMouseContent = new GUIContent(" Use Mouse");

        private readonly static GUIContent dkiPitchSensitivityContent = new GUIContent(" Mouse sensitivity");
        private readonly static GUIContent dkiYawSensitivityContent = new GUIContent(" Mouse sensitivity");
        private readonly static GUIContent dkiPitchDeadzoneContent = new GUIContent(" Mouse deadzone");
        private readonly static GUIContent dkiYawDeadzoneContent = new GUIContent(" Mouse deadzone");
        #endif

        #endregion GUIContent - Direct Keyboard Input

        #region GUIContent - Legacy Unity Input System or (Rewired or Oculus without Legacy Unity Input System)
        #if (ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER) || SSC_OVR || VIU_PLUGIN || SSC_REWIRED
        private readonly static GUIContent inputAxisModeContent = new GUIContent(" Axis Mode", "How input will be received from the input " +
            "manager. When No Input is selected, no input will be received from the input manager on that axis (use this option if you " +
            "don't want the player to be able to control that axis of movement). When Single Axis is selected, input will be received " +
            "from a single axis specified in the input manager. When Combined Axis is selected, input will be received from two axes " +
            "specified in the input manager and added together to give the final input.");

        #endif
        #endregion

        #region GUIContent - Shared (Legacy Unity Input System, New Input System)
        #if ENABLE_LEGACY_INPUT_MANAGER || SSC_UIS
        private readonly static GUIContent invertInputAxisContent = new GUIContent(" Invert Axis", "When enabled, the input received from " +
            "this axis will be inverted.");
        #endif

        #endregion

        #region GUIContent - Legacy Unity Input System
        #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
        private readonly static GUIContent singleAxisNameContent = new GUIContent(" Axis Name", "The name of this axis in the input manager.");
        private readonly static GUIContent positiveAxisNameContent = new GUIContent(" Positive Axis Name", "The name of the positive axis " +
            "in the input manager.");
        private readonly static GUIContent negativeAxisNameContent = new GUIContent(" Negative Axis Name", "The name of the negative axis " +
            "in the input manager.");
        private readonly static string identicalAxisNamesWarningContent = "Positive and negative axis names are required to be different.";
        private readonly static string invalidLegacyAxisNameWarningContent = "The axis is undefined. Check the input manager.";

        private readonly static GUIContent lisPitchInputMouseContent = new GUIContent(" Use Mouse");
        private readonly static GUIContent lisYawInputMouseContent = new GUIContent(" Use Mouse");

        private readonly static GUIContent lisPitchSensitivityContent = new GUIContent(" Mouse sensitivity");
        private readonly static GUIContent lisYawSensitivityContent = new GUIContent(" Mouse sensitivity");
        private readonly static GUIContent lisPitchDeadzoneContent = new GUIContent(" Mouse deadzone");
        private readonly static GUIContent lisYawDeadzoneContent = new GUIContent(" Mouse deadzone");
        #endif
        #endregion GUIContent - Legacy Unity Input System

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent("Debug Mode", "Use this to display the data being set to the Ship Control Module at runtime in the editor.");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent("Is Initialised?");
        private readonly static GUIContent debugIsInputEnabledContent = new GUIContent("Is Input Enabled?");
        private readonly static GUIContent debugIsCustomPlayerInputOnlyEnabledContent = new GUIContent("Is Custom Input Only?");
        private readonly static GUIContent debugIsShipAIModeEnabledContent = new GUIContent("Is Ship AI Mode Enabled?");
        private readonly static GUIContent debugHorizontalContent = new GUIContent("Horizontal");
        private readonly static GUIContent debugVerticalContent = new GUIContent("Vertical");
        private readonly static GUIContent debugLongitudinalContent = new GUIContent("Longitudinal");
        private readonly static GUIContent debugPitchContent = new GUIContent("Pitch");
        private readonly static GUIContent debugYawContent = new GUIContent("Yaw");
        private readonly static GUIContent debugRollContent = new GUIContent("Roll");
        private readonly static GUIContent debugPrimaryFireContent = new GUIContent("Primary Fire");
        private readonly static GUIContent debugSecondaryFireContent = new GUIContent("Secondary Fire");
        private readonly static GUIContent debugDockingContent = new GUIContent("Docking");
        private readonly static GUIContent debugShipSpeedContent = new GUIContent("Ship Speed km/h");
        #endregion GUIContent - Debug

        #region GUIContent - Oculus API
#if SSC_OVR
        private readonly static GUIContent ovrHorizontalInputTypeContent = new GUIContent(" Input Type", "Button, Axis or (HMD) Pose. The later requires the OVRManager in the scene.");
        private readonly static GUIContent ovrVerticalInputTypeContent = new GUIContent(" Input Type", "Button, Axis or (HMD) Pose. The later requires the OVRManager in the scene.");
        private readonly static GUIContent ovrLongitudinalInputTypeContent = new GUIContent(" Input Type", "Button, Axis or (HMD) Pose. The later requires the OVRManager in the scene.");
        private readonly static GUIContent ovrPitchInputTypeContent = new GUIContent(" Input Type", "Button, Axis or (HMD) Pose. The later requires the OVRManager in the scene.");
        private readonly static GUIContent ovrYawInputTypeContent = new GUIContent(" Input Type", "Button, Axis or (HMD) Pose. The later requires the OVRManager in the scene.");
        private readonly static GUIContent ovrRollInputTypeContent = new GUIContent(" Input Type", "Button, Axis or (HMD) Pose. The later requires the OVRManager in the scene.");
        private readonly static GUIContent ovrCustomInputTypeContent = new GUIContent(" Input Type", "Button, Axis or (HMD) Pose. The later requires the OVRManager in the scene.");

        private readonly string identicalOVRInputWarningContent = "Positive and negative input sources are required to be different.";

        private readonly static GUIContent ovrIsInputUpdateRequiredContent = new GUIContent("Update Input", "If the OVRManager is not located in the scene, OVRInput.Update() needs to be called on 1 PlayerInputModule. If OVRManager is added or removed at runtime, you need to call PlayerInputModule.CheckOVRManager() in your code.");
        private readonly static GUIContent ovrHorizontalPositiveInputContent = new GUIContent(" Positive Input", "Button, Axis, or Pose Rotation Axis used for positive input");
        private readonly static GUIContent ovrHorizontalNegativeInputContent = new GUIContent(" Negative Input", "Button, Axis, or Pose Rotation Axis used for negative input");
        private readonly static GUIContent ovrHorizontalInputContent = new GUIContent(" Input", "Button, Axis, or Pose Rotation Axis used for input");
        private readonly static GUIContent ovrVerticalPositiveInputContent = new GUIContent(" Positive Input", "Button, Axis, or Pose Rotation Axis used for positive input");
        private readonly static GUIContent ovrVerticalNegativeInputContent = new GUIContent(" Negative Input", "Button, Axis, or Pose Rotation Axis used for negative input");
        private readonly static GUIContent ovrVerticalInputContent = new GUIContent(" Input", "Button, Axis, or Pose Rotation Axis used for input");
        private readonly static GUIContent ovrLongitudinalPositiveInputContent = new GUIContent(" Positive Input", "Button, Axis, or Pose Rotation Axis used for positive input");
        private readonly static GUIContent ovrLongitudinalNegativeInputContent = new GUIContent(" Negative Input", "Button, Axis, or Pose Rotation Axis used for negative input");
        private readonly static GUIContent ovrLongitudinalInputContent = new GUIContent(" Input", "Button, Axis, or Pose Rotation Axis used for input");
        private readonly static GUIContent ovrPitchPositiveInputContent = new GUIContent(" Positive Input", "Button, Axis, or Pose Rotation Axis used for positive input");
        private readonly static GUIContent ovrPitchNegativeInputContent = new GUIContent(" Negative Input", "Button, Axis, or Pose Rotation Axis used for negative input");
        private readonly static GUIContent ovrPitchInputContent = new GUIContent(" Input", "Button, Axis, or Pose Rotation Axis used for input");
        private readonly static GUIContent ovrYawPositiveInputContent = new GUIContent(" Positive Input", "Button, Axis, or Pose Rotation Axis used for positive input");
        private readonly static GUIContent ovrYawNegativeInputContent = new GUIContent(" Negative Input", "Button, Axis, or Pose Rotation Axis used for negative input");
        private readonly static GUIContent ovrYawInputContent = new GUIContent(" Input", "Button, Axis, or Pose Rotation Axis used for input");
        private readonly static GUIContent ovrRollPositiveInputContent = new GUIContent(" Positive Input", "Button, Axis, or Pose Rotation Axis used for positive input");
        private readonly static GUIContent ovrRollNegativeInputContent = new GUIContent(" Negative Input", "Button, Axis, or Pose Rotation Axis used for negative input");
        private readonly static GUIContent ovrRollInputContent = new GUIContent(" Input", "Button, Axis, or Pose Rotation Axis used for input");
        private readonly static GUIContent ovrCustomInputContent = new GUIContent(" Input", "Button, Axis, or Pose Rotation Axis used for input");
        private readonly static GUIContent ovrCustomPositiveInputContent = new GUIContent(" Positive Input", "Button, Axis, or Pose Rotation Axis used for positive input");
        private readonly static GUIContent ovrCustomNegativeInputContent = new GUIContent(" Negative Input", "Button, Axis, or Pose Rotation Axis used for negative input");
        private readonly static GUIContent ovrCustomButtonInputContent = new GUIContent(" Button Input", "Button used for input");

        private readonly static GUIContent ovrPrimaryFireEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent ovrPrimaryFireInputContent = new GUIContent(" Input", "Button used for input");
        private readonly static GUIContent ovrSecondaryFireEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent ovrSecondaryFireInputContent = new GUIContent(" Input", "Button used for input");
        private readonly static GUIContent ovrDockingEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent ovrDockingInputContent = new GUIContent(" Input", "Button used for input");

#endif
        #endregion GUIContent - Oculus API

        #region GUIContent - Unity Input System (from package manager)
#if SSC_UIS
        private readonly static GUIContent inputSystemAxisModeContent = new GUIContent(" Axis Mode", "How input will be received from the Unity " +
            "Input System. When No Input is selected, no input will be received on that axis (use this option if you don't want the player " +
            "to be able to control that axis of movement). When Single Axis is selected, input will be received from a single Action configured " +
            "in the Input Action Importer Editor. Combined Axis is not used with Unity Input System because a single Action can be configured with Composite Bindings.");

        private readonly static GUIContent[] uisDataSlots2 = { new GUIContent("Slot1"), new GUIContent("Slot2") };
        private readonly static GUIContent[] uisDataSlots3 = { new GUIContent("Slot1"), new GUIContent("Slot2"), new GUIContent("Slot3") };

        private readonly static GUIContent uisInputATypeContent = new GUIContent(" Action Type");
        private readonly static GUIContent uisInputCTypeContent = new GUIContent(" Control Type", "Supported Control Types are Button, Axis, Vector2/Dpad and Vector3");
        private readonly static GUIContent uisInputDataSlotContent = new GUIContent(" Data Slot", "The slot or position of the value being used in the returned Control Type. For example a Vector2 returns two floats. To use the first float use Slot1.");

        private readonly static GUIContent uisHorizontalInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisVerticalInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisLongitudinalInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisPitchInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisYawInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisRollInputAContent = new GUIContent(" Action");

        private readonly static GUIContent uisPrimaryFireEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent uisPrimaryFireInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisSecondaryFireEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent uisSecondaryFireInputAContent = new GUIContent(" Action");
        private readonly static GUIContent uisDockingEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent uisDockingInputAContent = new GUIContent(" Action");

        private readonly static GUIContent uisPitchInputMouseContent = new GUIContent(" Use Mouse");
        private readonly static GUIContent uisYawInputMouseContent = new GUIContent(" Use Mouse");

        private readonly static GUIContent uisPitchSensitivityContent = new GUIContent(" Mouse sensitivity");
        private readonly static GUIContent uisYawSensitivityContent = new GUIContent(" Mouse sensitivity");
        private readonly static GUIContent uisPitchDeadzoneContent = new GUIContent(" Mouse deadzone");
        private readonly static GUIContent uisYawDeadzoneContent = new GUIContent(" Mouse deadzone");

        private readonly static GUIContent uisCustomEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent uisCustomInputAContent = new GUIContent(" Action");

        private readonly static GUIContent uisDebugApplyChangedContent = new GUIContent("Apply Changes", "Apply Axis setup changes");

        private readonly static GUIContent uisAddActionsContent = new GUIContent(" Add Actions", "Add a typical set of actions to the Default Map in the attached Unity Player Input component. Does not override existing Actions. Will then configure all Axes.");
        private readonly static GUIContent uisOnScreenControlsContent = new GUIContent(" On-Screen Controls", "Add or configure on-screen controls");
#endif
        #endregion

        #region GUIContent - Rewired
#if SSC_REWIRED
        // Rewired
        private readonly static string rwidenticalAxisIdWarningContent = "Positive and negative axis actions are required to be different.";

        private readonly static GUIContent rwPlayerNumberContent = new GUIContent(" Player Number", "The human player controlling this ship. " +
              "0 = unassigned. Typically 1,2,3,4 etc. If the Rewired player is set using PlayerInputModule .SetRewiredPlayer(..) in your code, leave it as 0 here. " +
              "NOTE: At runtime, Players must be first assigned in Rewired.");

        private readonly static GUIContent rwVersionContent = new GUIContent(" Rewired version");
        private readonly static GUIContent rwHorizontalInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwHorizontalInputACPositiveContent = new GUIContent(" Positive Action Category");
        private readonly static GUIContent rwHorizontalInputACNegativeContent = new GUIContent(" Negative Action Category");
        private readonly static GUIContent rwHorizontalInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwHorizontalInputAIPositiveContent = new GUIContent(" Positive Action Input");
        private readonly static GUIContent rwHorizontalInputAINegativeContent = new GUIContent(" Negative Action Input");

        private readonly static GUIContent rwVerticalInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwVerticalInputACPositiveContent = new GUIContent(" Positive Action Category");
        private readonly static GUIContent rwVerticalInputACNegativeContent = new GUIContent(" Negative Action Category");
        private readonly static GUIContent rwVerticalInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwVerticalInputAIPositiveContent = new GUIContent(" Positive Action Input");
        private readonly static GUIContent rwVerticalInputAINegativeContent = new GUIContent(" Negative Action Input");

        private readonly static GUIContent rwLongitudinalInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwLongitudinalInputACPositiveContent = new GUIContent(" Positive Action Category");
        private readonly static GUIContent rwLongitudinalInputACNegativeContent = new GUIContent(" Negative Action Category");
        private readonly static GUIContent rwLongitudinalInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwLongitudinalInputAIPositiveContent = new GUIContent(" Positive Action Input");
        private readonly static GUIContent rwLongitudinalInputAINegativeContent = new GUIContent(" Negative Action Input");

        private readonly static GUIContent rwPitchInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwPitchInputACPositiveContent = new GUIContent(" Positive Action Category");
        private readonly static GUIContent rwPitchInputACNegativeContent = new GUIContent(" Negative Action Category");
        private readonly static GUIContent rwPitchInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwPitchInputAIPositiveContent = new GUIContent(" Positive Action Input");
        private readonly static GUIContent rwPitchInputAINegativeContent = new GUIContent(" Negative Action Input");

        private readonly static GUIContent rwYawInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwYawInputACPositiveContent = new GUIContent(" Positive Action Category");
        private readonly static GUIContent rwYawInputACNegativeContent = new GUIContent(" Negative Action Category");
        private readonly static GUIContent rwYawInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwYawInputAIPositiveContent = new GUIContent(" Positive Action Input");
        private readonly static GUIContent rwYawInputAINegativeContent = new GUIContent(" Negative Action Input");

        private readonly static GUIContent rwRollInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwRollInputACPositiveContent = new GUIContent(" Positive Action Category");
        private readonly static GUIContent rwRollInputACNegativeContent = new GUIContent(" Negative Action Category");
        private readonly static GUIContent rwRollInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwRollInputAIPositiveContent = new GUIContent(" Positive Action Input");
        private readonly static GUIContent rwRollInputAINegativeContent = new GUIContent(" Negative Action Input");

        private readonly static GUIContent rwPrimaryFireEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent rwPrimaryFireInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwPrimaryFireInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwSecondaryFireEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent rwSecondaryFireInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwSecondaryFireInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwDockingEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent rwDockingInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwDockingInputAIContent = new GUIContent(" Action Input");

        private readonly static GUIContent rwCustomEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent rwCustomInputACContent = new GUIContent(" Action Category");
        private readonly static GUIContent rwCustomInputAIContent = new GUIContent(" Action Input");
        private readonly static GUIContent rwCustomInputACPositiveContent = new GUIContent(" Positive Action Category");
        private readonly static GUIContent rwCustomInputACNegativeContent = new GUIContent(" Negative Action Category");
        private readonly static GUIContent rwCustomInputAIPositiveContent = new GUIContent(" Positive Action Input");
        private readonly static GUIContent rwCustomInputAINegativeContent = new GUIContent(" Negative Action Input");
#endif
        #endregion

        #region GUIContent - Vive
#if VIU_PLUGIN
        private readonly static GUIContent viveHorizontalInputTypeContent = new GUIContent(" Input Type", "Button, Axis or Pose (single axis only)");
        private readonly static GUIContent viveVerticalInputTypeContent = new GUIContent(" Input Type", "Button, Axis or Pose (single axis only)");
        private readonly static GUIContent viveLongitudinalInputTypeContent = new GUIContent(" Input Type", "Button, Axis or Pose (single axis only)");
        private readonly static GUIContent vivePitchInputTypeContent = new GUIContent(" Input Type", "Button, Axis or Pose (single axis only)");
        private readonly static GUIContent viveYawInputTypeContent = new GUIContent(" Input Type", "Button, Axis or Pose (single axis only)");
        private readonly static GUIContent viveRollInputTypeContent = new GUIContent(" Input Type", "Button, Axis or Pose (single axis only)");
        private readonly static GUIContent viveCustomInputTypeContent = new GUIContent(" Input Type", "Button, Axis or Pose (single axis only)");
        private readonly static GUIContent viveHorizontalRoleTypeContent = new GUIContent(" Vive Role Type", "");
        private readonly static GUIContent viveVerticalRoleTypeContent = new GUIContent(" Vive Role Type", "");
        private readonly static GUIContent viveLongitudinalRoleTypeContent = new GUIContent(" Vive Role Type", "");
        private readonly static GUIContent vivePitchRoleTypeContent = new GUIContent(" Vive Role Type", "");
        private readonly static GUIContent viveYawRoleTypeContent = new GUIContent(" Vive Role Type", "");
        private readonly static GUIContent viveRollRoleTypeContent = new GUIContent(" Vive Role Type", "");
        private readonly static GUIContent vivePrimaryFireEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent vivePrimaryFireRoleTypeContent = new GUIContent(" Vive Role Type", "");
        private readonly static GUIContent viveSecondaryFireEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent viveSecondaryFireRoleTypeContent = new GUIContent(" Vive Role Type", "");
        private readonly static GUIContent viveDockingRoleTypeContent = new GUIContent(" Vive Role Type", "");
        private readonly static GUIContent viveDockingEnabledContent = new GUIContent(" Is Enabled");
        private readonly static GUIContent viveCustomRoleTypeContent = new GUIContent(" Vive Role Type", "");
        private readonly static GUIContent viveCustomEnabledContent = new GUIContent(" Is Enabled");

        private readonly static GUIContent viveHorizontalPositiveRoleIdContent = new GUIContent(" Positive Controller Role", "The Vive Role for this Controller used for positive input");
        private readonly static GUIContent viveHorizontalNegativeRoleIdContent = new GUIContent(" Negative Controller Role", "The Vive Role for this Controller used for negative input");
        private readonly static GUIContent viveHorizontalRoleIdContent = new GUIContent(" Controller Role", "The Vive Role for this Controller");
        private readonly static GUIContent viveVerticalPositiveRoleIdContent = new GUIContent(" Positive Controller Role", "The Vive Role for this Controller used for positive input");
        private readonly static GUIContent viveVerticalNegativeRoleIdContent = new GUIContent(" Negative Controller Role", "The Vive Role for this Controller for negative input");
        private readonly static GUIContent viveVerticalRoleIdContent = new GUIContent(" Controller Role", "The Vive Role for this Controller");
        private readonly static GUIContent viveLongitudinalPositiveRoleIdContent = new GUIContent(" Positive Controller Role", "The Vive Role for this Controller used for positive input");
        private readonly static GUIContent viveLongitudinalNegativeRoleIdContent = new GUIContent(" Negative Controller Role", "The Vive Role for this Controller used for negative input");
        private readonly static GUIContent viveLongitudinalRoleIdContent = new GUIContent(" Controller Role", "The Vive Role for this Controller");
        private readonly static GUIContent vivePitchPositiveRoleIdContent = new GUIContent(" Positive Controller Role", "The Vive Role for this Controller used for positive input");
        private readonly static GUIContent vivePitchNegativeRoleIdContent = new GUIContent(" Negative Controller Role", "The Vive Role for this Controller used for negative input");
        private readonly static GUIContent vivePitchRoleIdContent = new GUIContent(" Controller Role", "The Vive Role for this Controller");
        private readonly static GUIContent viveYawPositiveRoleIdContent = new GUIContent(" Positive Controller Role", "The Vive Role for this Controller used for positive input");
        private readonly static GUIContent viveYawNegativeRoleIdContent = new GUIContent(" Negative Controller Role", "The Vive Role for this Controller used for negative input");
        private readonly static GUIContent viveYawRoleIdContent = new GUIContent(" Controller Role", "The Vive Role for this Controller");
        private readonly static GUIContent viveRollPositiveRoleIdContent = new GUIContent(" Positive Controller Role", "The Vive Role for this Controller used for positive input");
        private readonly static GUIContent viveRollNegativeRoleIdContent = new GUIContent(" Negative Controller Role", "The Vive Role for this Controller used for negative input");
        private readonly static GUIContent viveRollRoleIdContent = new GUIContent(" Controller Role", "The Vive Role for this Controller");
        private readonly static GUIContent vivePrimaryFireRoleIdContent = new GUIContent(" Controller Role", "The Vive Role for this Controller");
        private readonly static GUIContent viveSecondaryFireRoleIdContent = new GUIContent(" Controller Role", "The Vive Role for this Controller");
        private readonly static GUIContent viveDockingRoleIdContent = new GUIContent(" Controller Role", "The Vive Role for this Controller");
        private readonly static GUIContent viveCustomPositiveRoleIdContent = new GUIContent(" Positive Controller Role", "The Vive Role for this Controller used for positive input");
        private readonly static GUIContent viveCustomNegativeRoleIdContent = new GUIContent(" Negative Controller Role", "The Vive Role for this Controller used for negative input");
        private readonly static GUIContent viveCustomRoleIdContent = new GUIContent(" Controller Role", "The Vive Role for this Controller");


        private readonly string identicalViveInputWarningContent = "Positive and negative input sources are required to be different.";

        private readonly static GUIContent viveHorizontalPositiveInputCtrlContent = new GUIContent(" Positive Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for positive input.");
        private readonly static GUIContent viveHorizontalNegativeInputCtrlContent = new GUIContent(" Negative Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for negative input");
        private readonly static GUIContent viveHorizontalInputCtrlContent = new GUIContent(" Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for input");
        private readonly static GUIContent viveVerticalPositiveInputCtrlContent = new GUIContent(" Positive Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for positive input");
        private readonly static GUIContent viveVerticalNegativeInputCtrlContent = new GUIContent(" Negative Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for negative input");
        private readonly static GUIContent viveVerticalInputCtrlContent = new GUIContent(" Controller", "The device Controller Button or Axis used for input");
        private readonly static GUIContent viveLongitudinalPositiveInputCtrlContent = new GUIContent(" Positive Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for positive input");
        private readonly static GUIContent viveLongitudinalNegativeInputCtrlContent = new GUIContent(" Negative Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for negative input");
        private readonly static GUIContent viveLongitudinalInputCtrlContent = new GUIContent(" Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for input");
        private readonly static GUIContent vivePitchPositiveInputCtrlContent = new GUIContent(" Positive Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for positive input");
        private readonly static GUIContent vivePitchNegativeInputCtrlContent = new GUIContent(" Negative Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for negative input");
        private readonly static GUIContent vivePitchInputCtrlContent = new GUIContent(" Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for input");
        private readonly static GUIContent viveYawPositiveInputCtrlContent = new GUIContent(" Positive Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for positive input");
        private readonly static GUIContent viveYawNegativeInputCtrlContent = new GUIContent(" Negative Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for negative input");
        private readonly static GUIContent viveYawInputCtrlContent = new GUIContent(" Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for input");
        private readonly static GUIContent viveRollPositiveInputCtrlContent = new GUIContent(" Positive Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for positive input");
        private readonly static GUIContent viveRollNegativeInputCtrlContent = new GUIContent(" Negative Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for negative input");
        private readonly static GUIContent viveRollInputCtrlContent = new GUIContent(" Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for input");
        private readonly static GUIContent vivePrimaryFireInputCtrlContent = new GUIContent(" Controller", "The device Controller Button used for input");
        private readonly static GUIContent viveSecondaryFireInputCtrlContent = new GUIContent(" Controller", "The device Controller Button used for input");
        private readonly static GUIContent viveDockingInputCtrlContent = new GUIContent(" Controller", "The device Controller Button used for input");
        private readonly static GUIContent viveCustomPositiveInputCtrlContent = new GUIContent(" Positive Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for positive input");
        private readonly static GUIContent viveCustomNegativeInputCtrlContent = new GUIContent(" Negative Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for negative input");
        private readonly static GUIContent viveCustomInputCtrlContent = new GUIContent(" Controller", "The device Controller Button, Axis, or Pose Rotation Axis used for input");

#endif
        #endregion GUIContent - Vive

        #region GUIContent - XR
        #if SCSM_XR && SSC_UIS
        private readonly static GUIContent xrPluginVersionContent = new GUIContent(" XR Plugin version", "The version of the Unity XR Management plugin installed from Package Manager");
        private readonly static GUIContent xrOpenXRVersionContent = new GUIContent(" OpenXR version", "The version of the OpenXR plugin installed from Package Manager");
        private readonly static GUIContent xrOculusXRVersionContent = new GUIContent(" Oculus version", "The version of the Oculus plugin installed from Package Manager");
        private readonly static GUIContent xrInputActionAssetContent = new GUIContent(" Input Action Asset", "An scriptableobject asset containing Unity Input System action maps and control schemes. ");
        private readonly static GUIContent xrFirstPersonTransform1Content = new GUIContent(" XR Camera Transform", "The transform of the XR first person camera.");
        private readonly static GUIContent xrFirstPersonCamera1Content = new GUIContent(" XR Camera", "The transform of the XR first person camera.");
        private readonly static GUIContent xrLeftHandTransformContent = new GUIContent(" Left Hand", "The transform of the XR left hand");
        private readonly static GUIContent xrRightHandTransformContent = new GUIContent(" Right Hand", "The transform of the XR right hand");
        private readonly static GUIContent xrFirstPersonNewCamera1Content = new GUIContent("New", "Create a new XR first person camera as a child of the ship controller");
        private readonly static GUIContent xrNewHandContent = new GUIContent("New", "Create a new XR hand as a child of the ship controller");
        private readonly static GUIContent xrInputAMContent = new GUIContent(" Action Map", "Unity Input System Action Map in the Input Action Asset");
        private static GUIContent[] xrActionMapGUIContent;
        #endif
        #endregion

        #endregion

        #region SerializedProperties - General

        private SerializedProperty inputModeProp;
        private SerializedProperty initialiseOnAwakeProp;
        private SerializedProperty isEnabledOnInitialiseProp;
        private SerializedProperty isCustomInputsOnlyOnInitialiseProp;
        private SerializedProperty isAutoCruiseEnabledProp;
        private SerializedProperty horizontalShowInEditorProp;
        private SerializedProperty verticalShowInEditorProp;
        private SerializedProperty longitudinalShowInEditorProp;
        private SerializedProperty pitchShowInEditorProp;
        private SerializedProperty yawShowInEditorProp;
        private SerializedProperty rollShowInEditorProp;
        private SerializedProperty primaryFireShowInEditorProp;
        private SerializedProperty secondaryFireShowInEditorProp;
        private SerializedProperty dockingShowInEditorProp;
        private SerializedProperty rumbleShowInEditorProp;
        private SerializedProperty primaryFireCanBeHeldProp;
        private SerializedProperty secondaryFireCanBeHeldProp;
        private SerializedProperty maxRumble1Prop;
        private SerializedProperty maxRumble2Prop;
        private SerializedProperty isSensitivityForHorizontalEnabledProp;
        private SerializedProperty isSensitivityForVerticalEnabledProp;
        private SerializedProperty isSensitivityForLongitudinalEnabledProp;
        private SerializedProperty isSensitivityForPitchEnabledProp;
        private SerializedProperty isSensitivityForYawEnabledProp;
        private SerializedProperty isSensitivityForRollEnabledProp;
        private SerializedProperty horizontalAxisSensitivityProp;
        private SerializedProperty verticalAxisSensitivityProp;
        private SerializedProperty longitudinalAxisSensitivityProp;
        private SerializedProperty pitchAxisSensitivityProp;
        private SerializedProperty yawAxisSensitivityProp;
        private SerializedProperty rollAxisSensitivityProp;
        private SerializedProperty horizontalAxisGravityProp;
        private SerializedProperty verticalAxisGravityProp;
        private SerializedProperty longitudinalAxisGravityProp;
        private SerializedProperty pitchAxisGravityProp;
        private SerializedProperty yawAxisGravityProp;
        private SerializedProperty rollAxisGravityProp;

        private SerializedProperty isHorizontalDataDiscardedProp;
        private SerializedProperty isVerticalDataDiscardedProp;
        private SerializedProperty isLongitudinalDataDiscardedProp;
        private SerializedProperty isPitchDataDiscardedProp;
        private SerializedProperty isYawDataDiscardedProp;
        private SerializedProperty isRollDataDiscardedProp;
        private SerializedProperty isPrimaryFireDataDiscardedProp;
        private SerializedProperty isSecondaryFireDataDiscardedProp;
        private SerializedProperty isDockDataDiscardedProp;

        #endregion SerializedProperties - General

        #region SerializedProperties - Shared (Legacy Unity Input System, New Input System, Rewired, XR)
        private SerializedProperty horizontalInputAxisModeProp;
        private SerializedProperty verticalInputAxisModeProp;
        private SerializedProperty longitudinalInputAxisModeProp;
        private SerializedProperty pitchInputAxisModeProp;
        private SerializedProperty yawInputAxisModeProp;
        private SerializedProperty rollInputAxisModeProp;
        #endregion

        #region SerializedProperties - Shared (Legacy Unity Input System, New Input System)
        #if ENABLE_LEGACY_INPUT_MANAGER || SSC_UIS
        private SerializedProperty invertHorizontalInputAxisProp;
        private SerializedProperty invertVerticalInputAxisProp;
        private SerializedProperty invertLongitudinalInputAxisProp;
        private SerializedProperty invertPitchInputAxisProp;
        private SerializedProperty invertYawInputAxisProp;
        private SerializedProperty invertRollInputAxisProp;
        #endif
        #endregion

        #region SerializedProperties - Direct Keyboard Input
        #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
        private SerializedProperty horizontalInputPositiveKeycodeProp;
        private SerializedProperty horizontalInputNegativeKeycodeProp;
        private SerializedProperty verticalInputPositiveKeycodeProp;
        private SerializedProperty verticalInputNegativeKeycodeProp;
        private SerializedProperty longitudinalInputPositiveKeycodeProp;
        private SerializedProperty longitudinalInputNegativeKeycodeProp;
        private SerializedProperty pitchInputPositiveKeycodeProp;
        private SerializedProperty pitchInputNegativeKeycodeProp;
        private SerializedProperty yawInputPositiveKeycodeProp;
        private SerializedProperty yawInputNegativeKeycodeProp;
        private SerializedProperty rollInputPositiveKeycodeProp;
        private SerializedProperty rollInputNegativeKeycodeProp;
        private SerializedProperty primaryFireInputKeycodeProp;
        private SerializedProperty secondaryFireInputKeycodeProp;
        private SerializedProperty dockingInputKeycodeProp;

        private SerializedProperty isMouseForPitchEnabledDKIProp;
        private SerializedProperty isMouseForYawEnabledDKIProp;
        private SerializedProperty pitchSensitivityDKIProp;
        private SerializedProperty yawSensitivityDKIProp;
        private SerializedProperty pitchDeadzoneDKIProp;
        private SerializedProperty yawDeadzoneDKIProp;
        #endif
        #endregion

        #region SerializedProperties - Legacy Unity Input System
        #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
        private SerializedProperty positiveHorizontalInputAxisNameProp;
        private SerializedProperty negativeHorizontalInputAxisNameProp;
        private SerializedProperty positiveVerticalInputAxisNameProp;
        private SerializedProperty negativeVerticalInputAxisNameProp;
        private SerializedProperty positiveLongitudinalInputAxisNameProp;
        private SerializedProperty negativeLongitudinalInputAxisNameProp;
        private SerializedProperty positivePitchInputAxisNameProp;
        private SerializedProperty negativePitchInputAxisNameProp;
        private SerializedProperty positiveYawInputAxisNameProp;
        private SerializedProperty negativeYawInputAxisNameProp;
        private SerializedProperty positiveRollInputAxisNameProp;
        private SerializedProperty negativeRollInputAxisNameProp;
        private SerializedProperty primaryFireInputAxisNameProp;
        private SerializedProperty secondaryFireInputAxisNameProp;
        private SerializedProperty dockingInputAxisNameProp;
        private SerializedProperty isPositiveHorizontalInputAxisValidProp;
        private SerializedProperty isNegativeHorizontalInputAxisValidProp;
        private SerializedProperty isPositiveVerticalInputAxisValidProp;
        private SerializedProperty isNegativeVerticalInputAxisValidProp;
        private SerializedProperty isPositiveLongitudinalInputAxisValidProp;
        private SerializedProperty isNegativeLongitudinalInputAxisValidProp;
        private SerializedProperty isPositivePitchInputAxisValidProp;
        private SerializedProperty isNegativePitchInputAxisValidProp;
        private SerializedProperty isPositiveYawInputAxisValidProp;
        private SerializedProperty isNegativeYawInputAxisValidProp;
        private SerializedProperty isPositiveRollInputAxisValidProp;
        private SerializedProperty isNegativeRollInputAxisValidProp;
        private SerializedProperty isPrimaryFireInputAxisValidProp;
        private SerializedProperty isSecondaryFireInputAxisValidProp;
        private SerializedProperty isDockingInputAxisValidProp;

        private SerializedProperty isMouseForPitchEnabledLISProp;
        private SerializedProperty isMouseForYawEnabledLISProp;
        private SerializedProperty pitchSensitivityLISProp;
        private SerializedProperty yawSensitivityLISProp;
        private SerializedProperty pitchDeadzoneLISProp;
        private SerializedProperty yawDeadzoneLISProp;
        #endif
        #endregion

        #region SerializedProperties - Unity Input System (and XR)
#if SSC_UIS
        private SerializedProperty positiveHorizontalInputActionIdUISProp;
        private SerializedProperty positiveHorizontalInputActionDataSlotUISProp;
        private SerializedProperty positiveVerticalInputActionIdUISProp;
        private SerializedProperty positiveVerticalInputActionDataSlotUISProp;
        private SerializedProperty positiveLongitudinalInputActionIdUISProp;
        private SerializedProperty positiveLongitudinalInputActionDataSlotUISProp;
        private SerializedProperty positivePitchInputActionIdUISProp;
        private SerializedProperty positivePitchInputActionDataSlotUISProp;
        private SerializedProperty positiveYawInputActionIdUISProp;
        private SerializedProperty positiveYawInputActionDataSlotUISProp;
        private SerializedProperty positiveRollInputActionIdUISProp;
        private SerializedProperty positiveRollInputActionDataSlotUISProp;
        private SerializedProperty primaryFireInputActionIdUISProp;
        private SerializedProperty primaryFireInputActionDataSlotUISProp;
        private SerializedProperty secondaryFireInputActionIdUISProp;
        private SerializedProperty secondaryFireInputActionDataSlotUISProp;
        private SerializedProperty dockingInputActionIdUISProp;
        private SerializedProperty dockingInputActionDataSlotUISProp;

        private SerializedProperty isMouseForPitchEnabledUISProp;
        private SerializedProperty isMouseForYawEnabledUISProp;
        private SerializedProperty pitchSensitivityUISProp;
        private SerializedProperty yawSensitivityUISProp;
        private SerializedProperty pitchDeadzoneUISProp;
        private SerializedProperty yawDeadzoneUISProp;
        private SerializedProperty onscreenControlsShowInEditorProp;
#endif
        #endregion SerializedProperties - Unity Input System

        #region SerializedProperties - Oculus API
#if SSC_OVR
        private SerializedProperty isOVRInputUpdatedIfRequiredProp;
        private SerializedProperty ovrHorizontalInputTypeProp;
        private SerializedProperty ovrVerticalInputTypeProp;
        private SerializedProperty ovrLongitudinalInputTypeProp;
        private SerializedProperty ovrPitchInputTypeProp;
        private SerializedProperty ovrYawInputTypeProp;
        private SerializedProperty ovrRollInputTypeProp;
        private SerializedProperty positiveHorizontalInputOVRProp;
        private SerializedProperty negativeHorizontalInputOVRProp;
        private SerializedProperty positiveVerticalInputOVRProp;
        private SerializedProperty negativeVerticalInputOVRProp;
        private SerializedProperty positiveLongitudinalInputOVRProp;
        private SerializedProperty negativeLongitudinalInputOVRProp;
        private SerializedProperty positivePitchInputOVRProp;
        private SerializedProperty negativePitchInputOVRProp;
        private SerializedProperty positiveYawInputOVRProp;
        private SerializedProperty negativeYawInputOVRProp;
        private SerializedProperty positiveRollInputOVRProp;
        private SerializedProperty negativeRollInputOVRProp;
        private SerializedProperty primaryFireInputOVRProp;
        private SerializedProperty secondaryFireInputOVRProp;
        private SerializedProperty dockingInputOVRProp;
#endif
        #endregion SerializedProperties - Oculus API

        #region SerializedProperties - Rewired
#if SSC_REWIRED
        private SerializedProperty rewiredPlayerNumberProp;
        private SerializedProperty positiveHorizontalInputActionIdProp;
        private SerializedProperty negativeHorizontalInputActionIdProp;
        private SerializedProperty positiveVerticalInputActionIdProp;
        private SerializedProperty negativeVerticalInputActionIdProp;
        private SerializedProperty positiveLongitudinalInputActionIdProp;
        private SerializedProperty negativeLongitudinalInputActionIdProp;
        private SerializedProperty positivePitchInputActionIdProp;
        private SerializedProperty negativePitchInputActionIdProp;
        private SerializedProperty positiveYawInputActionIdProp;
        private SerializedProperty negativeYawInputActionIdProp;
        private SerializedProperty positiveRollInputActionIdProp;
        private SerializedProperty negativeRollInputActionIdProp;
        private SerializedProperty primaryFireInputActionIdProp;
        private SerializedProperty secondaryFireInputActionIdProp;
        private SerializedProperty dockingInputActionIdProp;
#endif
        #endregion SerializedProperties - Rewired

        #region SerializedProperties - Vive
#if VIU_PLUGIN
        private SerializedProperty viveHorizontalInputTypeProp;
        private SerializedProperty viveVerticalInputTypeProp;
        private SerializedProperty viveLongitudinalInputTypeProp;
        private SerializedProperty vivePitchInputTypeProp;
        private SerializedProperty viveYawInputTypeProp;
        private SerializedProperty viveRollInputTypeProp;
        private SerializedProperty viveHorizontalRoleTypeProp;
        private SerializedProperty viveVerticalRoleTypeProp;
        private SerializedProperty viveLongitudinalRoleTypeProp;
        private SerializedProperty vivePitchRoleTypeProp;
        private SerializedProperty viveYawRoleTypeProp;
        private SerializedProperty viveRollRoleTypeProp;
        private SerializedProperty vivePrimaryFireRoleTypeProp;
        private SerializedProperty viveSecondaryFireRoleTypeProp;
        private SerializedProperty viveDockingRoleTypeProp;

        private SerializedProperty positiveHorizontalInputRoleIdProp;
        private SerializedProperty negativeHorizontalInputRoleIdProp;
        private SerializedProperty positiveVerticalInputRoleIdProp;
        private SerializedProperty negativeVerticalInputRoleIdProp;
        private SerializedProperty positiveLongitudinalInputRoleIdProp;
        private SerializedProperty negativeLongitudinalInputRoleIdProp;
        private SerializedProperty positivePitchInputRoleIdProp;
        private SerializedProperty negativePitchInputRoleIdProp;
        private SerializedProperty positiveYawInputRoleIdProp;
        private SerializedProperty negativeYawInputRoleIdProp;
        private SerializedProperty positiveRollInputRoleIdProp;
        private SerializedProperty negativeRollInputRoleIdProp;
        private SerializedProperty primaryFireInputRoleIdProp;
        private SerializedProperty secondaryFireInputRoleIdProp;
        private SerializedProperty dockingInputRoleIdProp;

        private SerializedProperty positiveHorizontalInputCtrlViveProp;
        private SerializedProperty negativeHorizontalInputCtrlViveProp;
        private SerializedProperty positiveVerticalInputCtrlViveProp;
        private SerializedProperty negativeVerticalInputCtrlViveProp;
        private SerializedProperty positiveLongitudinalInputCtrlViveProp;
        private SerializedProperty negativeLongitudinalInputCtrlViveProp;
        private SerializedProperty positivePitchInputCtrlViveProp;
        private SerializedProperty negativePitchInputCtrlViveProp;
        private SerializedProperty positiveYawInputCtrlViveProp;
        private SerializedProperty negativeYawInputCtrlViveProp;
        private SerializedProperty positiveRollInputCtrlViveProp;
        private SerializedProperty negativeRollInputCtrlViveProp;
        private SerializedProperty primaryFireInputCtrlViveProp;
        private SerializedProperty secondaryFireInputCtrlViveProp;
        private SerializedProperty dockingInputCtrlViveProp;
#endif
        #endregion

        #region SerializedProperties - XR
#if SCSM_XR && SSC_UIS
        private SerializedProperty inputActionAssetXRProp;
        private SerializedProperty firstPersonTransform1XRProp;
        private SerializedProperty firstPersonCamera1XRProp;
        private SerializedProperty leftHandTransformXRProp;
        private SerializedProperty rightHandTransformXRProp;

        private SerializedProperty positiveHorizontalInputActionMapIdXRProp;
        private SerializedProperty positiveVerticalInputActionMapIdXRProp;
        private SerializedProperty positiveLongitudinalInputActionMapIdXRProp;
        private SerializedProperty positivePitchInputActionMapIdXRProp;
        private SerializedProperty positiveYawInputActionMapIdXRProp;
        private SerializedProperty positiveRollInputActionMapIdXRProp;
        private SerializedProperty primaryFireInputActionMapIdXRProp;
        private SerializedProperty secondaryFireInputActionMapIdXRProp;
        private SerializedProperty dockingInputActionMapIdXRProp;
#endif
        #endregion

        #region SerializedProperties - Oculus or Vive or Rewired or Unity Input System
#if SSC_OVR || SSC_REWIRED || VIU_PLUGIN || SSC_UIS
        private SerializedProperty primaryFireButtonEnabledProp;
        private SerializedProperty secondaryFireButtonEnabledProp;
        private SerializedProperty dockingButtonEnabledProp;
#endif
        #endregion SerializedProperties - Oculus or Vive or Rewired

        #region SerializedProperties - Custom Player Inputs (cpi)

        private SerializedProperty cpiListProp;
        private SerializedProperty cpiItemProp;
        private SerializedProperty cpiItemShowInEditorProp;
        private SerializedProperty cpiShowInEditorProp;
        private SerializedProperty cpiIsListExpandedProp;
        private SerializedProperty cpiEventProp;
        private SerializedProperty cpiIsButtonProp;
        private SerializedProperty cpiCanBeHeldDownProp;
        private SerializedProperty cpiInputAxisModeProp;
        private SerializedProperty cpiIsSensitivityEnabledProp;
        private SerializedProperty cpiSensitivityProp;
        private SerializedProperty cpiGravityProp;

        #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
        private SerializedProperty cpidkmPositiveKeycodeProp;
        private SerializedProperty cpidkmNegativeKeycodeProp;
        private SerializedProperty cpilisPositiveAxisNameProp;
        private SerializedProperty cpilisNegativeAxisNameProp;
        private SerializedProperty cpilisInvertAxisProp;
        private SerializedProperty cpilisIsPositiveAxisValidProp;
        private SerializedProperty cpilisIsNegativeAxisValidProp;
        #endif

        #if SSC_UIS
        private SerializedProperty cpiuisPositiveInputActionIdProp;
        private SerializedProperty cpiuisPositiveInputActionDataSlotProp;
        #endif

        #if SSC_OVR
        private SerializedProperty cpiovrInputTypeProp;
        private SerializedProperty cpiovrPositiveInputProp;
        private SerializedProperty cpiovrNegativeInputProp;
        #endif

        #if SSC_REWIRED
        private SerializedProperty cpirwdPositiveInputActionIdProp;
        private SerializedProperty cpirwdNegativeInputActionIdProp;
        #endif

        #if VIU_PLUGIN
        private SerializedProperty cpiviveInputTypeProp;
        private SerializedProperty cpiviveRoleTypeProp;
        private SerializedProperty cpivivePositiveInputRoleIdProp;
        private SerializedProperty cpiviveNegativeInputRoleIdProp;
        private SerializedProperty cpivivePositiveInputCtrlProp;
        private SerializedProperty cpiviveNegativeInputCtrlProp;
        #endif

        #if SCSM_XR && SSC_UIS
        private SerializedProperty cpixrPositiveInputActionMapIdProp;
        #endif

        #if SSC_UIS || SSC_REWIRED || VIU_PLUGIN
        private SerializedProperty cpiIsButtonEnabledProp;
        #endif

        #endregion

        #region Events

        public void OnEnable()
        {
            isRefreshing = true;
            playerInputModule = (PlayerInputModule)target;

            defaultEditorLabelWidth = 160f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            // Initialise properties

            #region Find Properties - General

            inputModeProp = serializedObject.FindProperty("inputMode");
            initialiseOnAwakeProp = serializedObject.FindProperty("initialiseOnAwake");
            isEnabledOnInitialiseProp = serializedObject.FindProperty("isEnabledOnInitialise");
            isCustomInputsOnlyOnInitialiseProp = serializedObject.FindProperty("isCustomInputsOnlyOnInitialise");
            isAutoCruiseEnabledProp = serializedObject.FindProperty("isAutoCruiseEnabled");
            horizontalShowInEditorProp = serializedObject.FindProperty("horizontalShowInEditor");
            verticalShowInEditorProp = serializedObject.FindProperty("verticalShowInEditor");
            longitudinalShowInEditorProp = serializedObject.FindProperty("longitudinalShowInEditor");
            pitchShowInEditorProp = serializedObject.FindProperty("pitchShowInEditor");
            yawShowInEditorProp = serializedObject.FindProperty("yawShowInEditor");
            rollShowInEditorProp = serializedObject.FindProperty("rollShowInEditor");
            primaryFireShowInEditorProp = serializedObject.FindProperty("primaryFireShowInEditor");
            secondaryFireShowInEditorProp = serializedObject.FindProperty("secondaryFireShowInEditor");
            dockingShowInEditorProp = serializedObject.FindProperty("dockingShowInEditor");
            rumbleShowInEditorProp = serializedObject.FindProperty("rumbleShowInEditor");
            primaryFireCanBeHeldProp = serializedObject.FindProperty("primaryFireCanBeHeld");
            secondaryFireCanBeHeldProp = serializedObject.FindProperty("secondaryFireCanBeHeld");
            maxRumble1Prop = serializedObject.FindProperty("maxRumble1");
            maxRumble2Prop = serializedObject.FindProperty("maxRumble2");

            // Keep compiler happy for inputModes that don't support Rumble
            if (rumbleShowInEditorProp.boolValue) { }
            if (maxRumble1Prop.floatValue > 0f || maxRumble2Prop.floatValue > 0f) { }
            if (rumbleHeaderContent != null && maxRumble1Content != null || maxRumble2Content != null) { }

            isSensitivityForHorizontalEnabledProp = serializedObject.FindProperty("isSensitivityForHorizontalEnabled");
            isSensitivityForVerticalEnabledProp = serializedObject.FindProperty("isSensitivityForVerticalEnabled");
            isSensitivityForLongitudinalEnabledProp = serializedObject.FindProperty("isSensitivityForLongitudinalEnabled");
            isSensitivityForPitchEnabledProp = serializedObject.FindProperty("isSensitivityForPitchEnabled");
            isSensitivityForYawEnabledProp = serializedObject.FindProperty("isSensitivityForYawEnabled");
            isSensitivityForRollEnabledProp = serializedObject.FindProperty("isSensitivityForRollEnabled");

            horizontalAxisSensitivityProp = serializedObject.FindProperty("horizontalAxisSensitivity");
            verticalAxisSensitivityProp = serializedObject.FindProperty("verticalAxisSensitivity");
            longitudinalAxisSensitivityProp = serializedObject.FindProperty("longitudinalAxisSensitivity");
            pitchAxisSensitivityProp = serializedObject.FindProperty("pitchAxisSensitivity");
            yawAxisSensitivityProp = serializedObject.FindProperty("yawAxisSensitivity");
            rollAxisSensitivityProp = serializedObject.FindProperty("rollAxisSensitivity");

            horizontalAxisGravityProp = serializedObject.FindProperty("horizontalAxisGravity");
            verticalAxisGravityProp = serializedObject.FindProperty("verticalAxisGravity");
            longitudinalAxisGravityProp = serializedObject.FindProperty("longitudinalAxisGravity");
            pitchAxisGravityProp = serializedObject.FindProperty("pitchAxisGravity");
            yawAxisGravityProp = serializedObject.FindProperty("yawAxisGravity");
            rollAxisGravityProp = serializedObject.FindProperty("rollAxisGravity");

            isHorizontalDataDiscardedProp = serializedObject.FindProperty("isHorizontalDataDiscarded");
            isVerticalDataDiscardedProp = serializedObject.FindProperty("isVerticalDataDiscarded");
            isLongitudinalDataDiscardedProp = serializedObject.FindProperty("isLongitudinalDataDiscarded");
            isPitchDataDiscardedProp = serializedObject.FindProperty("isPitchDataDiscarded");
            isYawDataDiscardedProp = serializedObject.FindProperty("isYawDataDiscarded");
            isRollDataDiscardedProp = serializedObject.FindProperty("isRollDataDiscarded");
            isPrimaryFireDataDiscardedProp = serializedObject.FindProperty("isPrimaryFireDataDiscarded");
            isSecondaryFireDataDiscardedProp = serializedObject.FindProperty("isSecondaryFireDataDiscarded");
            isDockDataDiscardedProp = serializedObject.FindProperty("isDockDataDiscarded");

            #endregion

            #region Find Properties - keyboard

            #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
            horizontalInputPositiveKeycodeProp = serializedObject.FindProperty("horizontalInputPositiveKeycode");
            horizontalInputNegativeKeycodeProp = serializedObject.FindProperty("horizontalInputNegativeKeycode");
            verticalInputPositiveKeycodeProp = serializedObject.FindProperty("verticalInputPositiveKeycode");
            verticalInputNegativeKeycodeProp = serializedObject.FindProperty("verticalInputNegativeKeycode");
            longitudinalInputPositiveKeycodeProp = serializedObject.FindProperty("longitudinalInputPositiveKeycode");
            longitudinalInputNegativeKeycodeProp = serializedObject.FindProperty("longitudinalInputNegativeKeycode");
            pitchInputPositiveKeycodeProp = serializedObject.FindProperty("pitchInputPositiveKeycode");
            pitchInputNegativeKeycodeProp = serializedObject.FindProperty("pitchInputNegativeKeycode");
            yawInputPositiveKeycodeProp = serializedObject.FindProperty("yawInputPositiveKeycode");
            yawInputNegativeKeycodeProp = serializedObject.FindProperty("yawInputNegativeKeycode");
            rollInputPositiveKeycodeProp = serializedObject.FindProperty("rollInputPositiveKeycode");
            rollInputNegativeKeycodeProp = serializedObject.FindProperty("rollInputNegativeKeycode");
            primaryFireInputKeycodeProp = serializedObject.FindProperty("primaryFireKeycode");
            secondaryFireInputKeycodeProp = serializedObject.FindProperty("secondaryFireKeycode");
            dockingInputKeycodeProp = serializedObject.FindProperty("dockingKeycode");

            isMouseForPitchEnabledDKIProp = serializedObject.FindProperty("isMouseForPitchEnabledDKI");
            isMouseForYawEnabledDKIProp = serializedObject.FindProperty("isMouseForYawEnabledDKI");

            pitchSensitivityDKIProp = serializedObject.FindProperty("pitchSensitivityDKI");
            yawSensitivityDKIProp = serializedObject.FindProperty("yawSensitivityDKI");
            pitchDeadzoneDKIProp = serializedObject.FindProperty("pitchDeadzoneDKI");
            yawDeadzoneDKIProp = serializedObject.FindProperty("yawDeadzoneDKI");
            #endif

            #endregion

            #region Find Properties - Shared (Legacy Input, Unity Input System, Rewired)
            horizontalInputAxisModeProp = serializedObject.FindProperty("horizontalInputAxisMode");
            verticalInputAxisModeProp = serializedObject.FindProperty("verticalInputAxisMode");
            longitudinalInputAxisModeProp = serializedObject.FindProperty("longitudinalInputAxisMode");
            pitchInputAxisModeProp = serializedObject.FindProperty("pitchInputAxisMode");
            yawInputAxisModeProp = serializedObject.FindProperty("yawInputAxisMode");
            rollInputAxisModeProp = serializedObject.FindProperty("rollInputAxisMode");
            #endregion Find Properties - Shared (Legacy Input, Unity Input System, Rewired)

            #region Find Properties - Shared (Legacy Unity Input System, New Input System)
            #if ENABLE_LEGACY_INPUT_MANAGER || SSC_UIS
            invertHorizontalInputAxisProp = serializedObject.FindProperty("invertHorizontalInputAxis");
            invertVerticalInputAxisProp = serializedObject.FindProperty("invertVerticalInputAxis");
            invertLongitudinalInputAxisProp = serializedObject.FindProperty("invertLongitudinalInputAxis");
            invertPitchInputAxisProp = serializedObject.FindProperty("invertPitchInputAxis");
            invertYawInputAxisProp = serializedObject.FindProperty("invertYawInputAxis");
            invertRollInputAxisProp = serializedObject.FindProperty("invertRollInputAxis");
            #endif
            #endregion

            #region Find Properties - Legacy Input system
            #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
            positiveHorizontalInputAxisNameProp = serializedObject.FindProperty("positiveHorizontalInputAxisName");
            negativeHorizontalInputAxisNameProp = serializedObject.FindProperty("negativeHorizontalInputAxisName");
            positiveVerticalInputAxisNameProp = serializedObject.FindProperty("positiveVerticalInputAxisName");
            negativeVerticalInputAxisNameProp = serializedObject.FindProperty("negativeVerticalInputAxisName");
            positiveLongitudinalInputAxisNameProp = serializedObject.FindProperty("positiveLongitudinalInputAxisName");
            negativeLongitudinalInputAxisNameProp = serializedObject.FindProperty("negativeLongitudinalInputAxisName");
            positivePitchInputAxisNameProp = serializedObject.FindProperty("positivePitchInputAxisName");
            negativePitchInputAxisNameProp = serializedObject.FindProperty("negativePitchInputAxisName");
            positiveYawInputAxisNameProp = serializedObject.FindProperty("positiveYawInputAxisName");
            negativeYawInputAxisNameProp = serializedObject.FindProperty("negativeYawInputAxisName");
            positiveRollInputAxisNameProp = serializedObject.FindProperty("positiveRollInputAxisName");
            negativeRollInputAxisNameProp = serializedObject.FindProperty("negativeRollInputAxisName");
            primaryFireInputAxisNameProp = serializedObject.FindProperty("primaryFireInputAxisName");
            secondaryFireInputAxisNameProp = serializedObject.FindProperty("secondaryFireInputAxisName");
            dockingInputAxisNameProp = serializedObject.FindProperty("dockingInputAxisName");
            isPositiveHorizontalInputAxisValidProp = serializedObject.FindProperty("isPositiveHorizontalInputAxisValid");
            isNegativeHorizontalInputAxisValidProp = serializedObject.FindProperty("isNegativeHorizontalInputAxisValid");
            isPositiveVerticalInputAxisValidProp = serializedObject.FindProperty("isPositiveVerticalInputAxisValid");
            isNegativeVerticalInputAxisValidProp = serializedObject.FindProperty("isNegativeVerticalInputAxisValid");
            isPositiveLongitudinalInputAxisValidProp = serializedObject.FindProperty("isPositiveLongitudinalInputAxisValid");
            isNegativeLongitudinalInputAxisValidProp = serializedObject.FindProperty("isNegativeLongitudinalInputAxisValid");
            isPositivePitchInputAxisValidProp = serializedObject.FindProperty("isPositivePitchInputAxisValid");
            isNegativePitchInputAxisValidProp = serializedObject.FindProperty("isNegativePitchInputAxisValid");
            isPositiveYawInputAxisValidProp = serializedObject.FindProperty("isPositiveYawInputAxisValid");
            isNegativeYawInputAxisValidProp = serializedObject.FindProperty("isNegativeYawInputAxisValid");
            isPositiveRollInputAxisValidProp = serializedObject.FindProperty("isPositiveRollInputAxisValid");
            isNegativeRollInputAxisValidProp = serializedObject.FindProperty("isNegativeRollInputAxisValid");
            isPrimaryFireInputAxisValidProp = serializedObject.FindProperty("isPrimaryFireInputAxisValid");
            isSecondaryFireInputAxisValidProp = serializedObject.FindProperty("isSecondaryFireInputAxisValid");
            isDockingInputAxisValidProp = serializedObject.FindProperty("isDockingInputAxisValid");

            isMouseForPitchEnabledLISProp = serializedObject.FindProperty("isMouseForPitchEnabledLIS");
            isMouseForYawEnabledLISProp = serializedObject.FindProperty("isMouseForYawEnabledLIS");

            pitchSensitivityLISProp = serializedObject.FindProperty("pitchSensitivityLIS");
            yawSensitivityLISProp = serializedObject.FindProperty("yawSensitivityLIS");
            pitchDeadzoneLISProp = serializedObject.FindProperty("pitchDeadzoneLIS");
            yawDeadzoneLISProp = serializedObject.FindProperty("yawDeadzoneLIS");
            #endif
            #endregion

            #region Find Properties - Unity Input System (from Package Manager)
#if SSC_UIS
            positiveHorizontalInputActionIdUISProp = serializedObject.FindProperty("positiveHorizontalInputActionIdUIS");
            positiveVerticalInputActionIdUISProp = serializedObject.FindProperty("positiveVerticalInputActionIdUIS");
            positiveLongitudinalInputActionIdUISProp = serializedObject.FindProperty("positiveLongitudinalInputActionIdUIS");
            positivePitchInputActionIdUISProp = serializedObject.FindProperty("positivePitchInputActionIdUIS");
            positiveYawInputActionIdUISProp = serializedObject.FindProperty("positiveYawInputActionIdUIS");
            positiveRollInputActionIdUISProp = serializedObject.FindProperty("positiveRollInputActionIdUIS");
            primaryFireInputActionIdUISProp = serializedObject.FindProperty("primaryFireInputActionIdUIS");
            secondaryFireInputActionIdUISProp = serializedObject.FindProperty("secondaryFireInputActionIdUIS");
            dockingInputActionIdUISProp = serializedObject.FindProperty("dockingInputActionIdUIS");

            isMouseForPitchEnabledUISProp = serializedObject.FindProperty("isMouseForPitchEnabledUIS");
            isMouseForYawEnabledUISProp = serializedObject.FindProperty("isMouseForYawEnabledUIS");

            pitchSensitivityUISProp = serializedObject.FindProperty("pitchSensitivityUIS");
            yawSensitivityUISProp = serializedObject.FindProperty("yawSensitivityUIS");
            pitchDeadzoneUISProp = serializedObject.FindProperty("pitchDeadzoneUIS");
            yawDeadzoneUISProp = serializedObject.FindProperty("yawDeadzoneUIS");

            positiveHorizontalInputActionDataSlotUISProp = serializedObject.FindProperty("positiveHorizontalInputActionDataSlotUIS");
            positiveVerticalInputActionDataSlotUISProp = serializedObject.FindProperty("positiveVerticalInputActionDataSlotUIS");
            positiveLongitudinalInputActionDataSlotUISProp = serializedObject.FindProperty("positiveLongitudinalInputActionDataSlotUIS");
            positivePitchInputActionDataSlotUISProp = serializedObject.FindProperty("positivePitchInputActionDataSlotUIS");
            positiveYawInputActionDataSlotUISProp = serializedObject.FindProperty("positiveYawInputActionDataSlotUIS");
            positiveRollInputActionDataSlotUISProp = serializedObject.FindProperty("positiveRollInputActionDataSlotUIS");
            primaryFireInputActionDataSlotUISProp = serializedObject.FindProperty("primaryFireInputActionDataSlotUIS");
            secondaryFireInputActionDataSlotUISProp = serializedObject.FindProperty("secondaryFireInputActionDataSlotUIS");
            dockingInputActionDataSlotUISProp = serializedObject.FindProperty("dockingInputActionDataSlotUIS");

            onscreenControlsShowInEditorProp = serializedObject.FindProperty("onscreenControlsShowInEditor");
#endif
            #endregion

            #region Find Properties - Oculus API
#if SSC_OVR
            isOVRInputUpdatedIfRequiredProp = serializedObject.FindProperty("isOVRInputUpdatedIfRequired");

            ovrHorizontalInputTypeProp = serializedObject.FindProperty("ovrHorizontalInputType");
            ovrVerticalInputTypeProp = serializedObject.FindProperty("ovrVerticalInputType");
            ovrLongitudinalInputTypeProp = serializedObject.FindProperty("ovrLongitudinalInputType");
            ovrPitchInputTypeProp = serializedObject.FindProperty("ovrPitchInputType");
            ovrYawInputTypeProp = serializedObject.FindProperty("ovrYawInputType");
            ovrRollInputTypeProp = serializedObject.FindProperty("ovrRollInputType");

            positiveHorizontalInputOVRProp = serializedObject.FindProperty("positiveHorizontalInputOVR");
            negativeHorizontalInputOVRProp = serializedObject.FindProperty("negativeHorizontalInputOVR");
            positiveVerticalInputOVRProp = serializedObject.FindProperty("positiveVerticalInputOVR");
            negativeVerticalInputOVRProp = serializedObject.FindProperty("negativeVerticalInputOVR");
            positiveLongitudinalInputOVRProp = serializedObject.FindProperty("positiveLongitudinalInputOVR");
            negativeLongitudinalInputOVRProp = serializedObject.FindProperty("negativeLongitudinalInputOVR");
            positivePitchInputOVRProp = serializedObject.FindProperty("positivePitchInputOVR");
            negativePitchInputOVRProp = serializedObject.FindProperty("negativePitchInputOVR");
            positiveYawInputOVRProp = serializedObject.FindProperty("positiveYawInputOVR");
            negativeYawInputOVRProp = serializedObject.FindProperty("negativeYawInputOVR");
            positiveRollInputOVRProp = serializedObject.FindProperty("positiveRollInputOVR");
            negativeRollInputOVRProp = serializedObject.FindProperty("negativeRollInputOVR");
            primaryFireInputOVRProp = serializedObject.FindProperty("primaryFireInputOVR");
            secondaryFireInputOVRProp = serializedObject.FindProperty("secondaryFireInputOVR");
            dockingInputOVRProp = serializedObject.FindProperty("dockingInputOVR");

#endif
            #endregion

            #region Find Properties - Rewired
#if SSC_REWIRED
            rewiredPlayerNumberProp = serializedObject.FindProperty("rewiredPlayerNumber");
            positiveHorizontalInputActionIdProp = serializedObject.FindProperty("positiveHorizontalInputActionId");
            negativeHorizontalInputActionIdProp = serializedObject.FindProperty("negativeHorizontalInputActionId");
            positiveVerticalInputActionIdProp = serializedObject.FindProperty("positiveVerticalInputActionId");
            negativeVerticalInputActionIdProp = serializedObject.FindProperty("negativeVerticalInputActionId");
            positiveLongitudinalInputActionIdProp = serializedObject.FindProperty("positiveLongitudinalInputActionId");
            negativeLongitudinalInputActionIdProp = serializedObject.FindProperty("negativeLongitudinalInputActionId");
            positivePitchInputActionIdProp = serializedObject.FindProperty("positivePitchInputActionId");
            negativePitchInputActionIdProp = serializedObject.FindProperty("negativePitchInputActionId");
            positiveYawInputActionIdProp = serializedObject.FindProperty("positiveYawInputActionId");
            negativeYawInputActionIdProp = serializedObject.FindProperty("negativeYawInputActionId");
            positiveRollInputActionIdProp = serializedObject.FindProperty("positiveRollInputActionId");
            negativeRollInputActionIdProp = serializedObject.FindProperty("negativeRollInputActionId");
            primaryFireInputActionIdProp = serializedObject.FindProperty("primaryFireInputActionId");
            secondaryFireInputActionIdProp = serializedObject.FindProperty("secondaryFireInputActionId");
            dockingInputActionIdProp = serializedObject.FindProperty("dockingInputActionId");
#endif
            #endregion

            #region Find Properties - Vive
#if VIU_PLUGIN
            viveHorizontalInputTypeProp = serializedObject.FindProperty("viveHorizontalInputType");
            viveVerticalInputTypeProp = serializedObject.FindProperty("viveVerticalInputType");
            viveLongitudinalInputTypeProp = serializedObject.FindProperty("viveLongitudinalInputType");
            vivePitchInputTypeProp = serializedObject.FindProperty("vivePitchInputType");
            viveYawInputTypeProp = serializedObject.FindProperty("viveYawInputType");
            viveRollInputTypeProp = serializedObject.FindProperty("viveRollInputType");
            viveHorizontalRoleTypeProp = serializedObject.FindProperty("viveHorizontalRoleType");
            viveVerticalRoleTypeProp = serializedObject.FindProperty("viveVerticalRoleType");
            viveLongitudinalRoleTypeProp = serializedObject.FindProperty("viveLongitudinalRoleType");
            vivePitchRoleTypeProp = serializedObject.FindProperty("vivePitchRoleType");
            viveYawRoleTypeProp = serializedObject.FindProperty("viveYawRoleType");
            viveRollRoleTypeProp = serializedObject.FindProperty("viveRollRoleType");
            vivePrimaryFireRoleTypeProp = serializedObject.FindProperty("vivePrimaryFireRoleType");
            viveSecondaryFireRoleTypeProp = serializedObject.FindProperty("viveSecondaryFireRoleType");
            viveDockingRoleTypeProp = serializedObject.FindProperty("viveDockingRoleType");

            positiveHorizontalInputRoleIdProp = serializedObject.FindProperty("positiveHorizontalInputRoleId");
            negativeHorizontalInputRoleIdProp = serializedObject.FindProperty("negativeHorizontalInputRoleId");
            positiveVerticalInputRoleIdProp = serializedObject.FindProperty("positiveVerticalInputRoleId");
            negativeVerticalInputRoleIdProp = serializedObject.FindProperty("negativeVerticalInputRoleId");
            positiveLongitudinalInputRoleIdProp = serializedObject.FindProperty("positiveLongitudinalInputRoleId");
            negativeLongitudinalInputRoleIdProp = serializedObject.FindProperty("negativeLongitudinalInputRoleId");
            positivePitchInputRoleIdProp = serializedObject.FindProperty("positivePitchInputRoleId");
            negativePitchInputRoleIdProp = serializedObject.FindProperty("negativePitchInputRoleId");
            positiveYawInputRoleIdProp = serializedObject.FindProperty("positiveYawInputRoleId");
            negativeYawInputRoleIdProp = serializedObject.FindProperty("negativeYawInputRoleId");
            positiveRollInputRoleIdProp = serializedObject.FindProperty("positiveRollInputRoleId");
            negativeRollInputRoleIdProp = serializedObject.FindProperty("negativeRollInputRoleId");
            primaryFireInputRoleIdProp = serializedObject.FindProperty("primaryFireInputRoleId");
            secondaryFireInputRoleIdProp = serializedObject.FindProperty("secondaryFireInputRoleId");
            dockingInputRoleIdProp = serializedObject.FindProperty("dockingInputRoleId");

            positiveHorizontalInputCtrlViveProp = serializedObject.FindProperty("positiveHorizontalInputCtrlVive");
            negativeHorizontalInputCtrlViveProp = serializedObject.FindProperty("negativeHorizontalInputCtrlVive");
            positiveVerticalInputCtrlViveProp = serializedObject.FindProperty("positiveVerticalInputCtrlVive");
            negativeVerticalInputCtrlViveProp = serializedObject.FindProperty("negativeVerticalInputCtrlVive");
            positiveLongitudinalInputCtrlViveProp = serializedObject.FindProperty("positiveLongitudinalInputCtrlVive");
            negativeLongitudinalInputCtrlViveProp = serializedObject.FindProperty("negativeLongitudinalInputCtrlVive");
            positivePitchInputCtrlViveProp = serializedObject.FindProperty("positivePitchInputCtrlVive");
            negativePitchInputCtrlViveProp = serializedObject.FindProperty("negativePitchInputCtrlVive");
            positiveYawInputCtrlViveProp = serializedObject.FindProperty("positiveYawInputCtrlVive");
            negativeYawInputCtrlViveProp = serializedObject.FindProperty("negativeYawInputCtrlVive");
            positiveRollInputCtrlViveProp = serializedObject.FindProperty("positiveRollInputCtrlVive");
            negativeRollInputCtrlViveProp = serializedObject.FindProperty("negativeRollInputCtrlVive");
            primaryFireInputCtrlViveProp = serializedObject.FindProperty("primaryFireInputCtrlVive");
            secondaryFireInputCtrlViveProp = serializedObject.FindProperty("secondaryFireInputCtrlVive");
            dockingInputCtrlViveProp = serializedObject.FindProperty("dockingInputCtrlVive");
#endif
            #endregion

            #region Find Properties - XR
            #if SCSM_XR && SSC_UIS
            inputActionAssetXRProp = serializedObject.FindProperty("inputActionAssetXR");
            firstPersonTransform1XRProp = serializedObject.FindProperty("firstPersonTransform1XR");
            firstPersonCamera1XRProp = serializedObject.FindProperty("firstPersonCamera1XR");
            leftHandTransformXRProp = serializedObject.FindProperty("leftHandTransformXR");
            rightHandTransformXRProp = serializedObject.FindProperty("rightHandTransformXR");

            positiveHorizontalInputActionMapIdXRProp = serializedObject.FindProperty("positiveHorizontalInputActionMapIdXR");
            positiveVerticalInputActionMapIdXRProp = serializedObject.FindProperty("positiveVerticalInputActionMapIdXR");
            positiveLongitudinalInputActionMapIdXRProp = serializedObject.FindProperty("positiveLongitudinalInputActionMapIdXR");
            positivePitchInputActionMapIdXRProp = serializedObject.FindProperty("positivePitchInputActionMapIdXR");
            positiveYawInputActionMapIdXRProp = serializedObject.FindProperty("positiveYawInputActionMapIdXR");
            positiveRollInputActionMapIdXRProp = serializedObject.FindProperty("positiveRollInputActionMapIdXR");
            primaryFireInputActionMapIdXRProp = serializedObject.FindProperty("primaryFireInputActionMapIdXR");
            secondaryFireInputActionMapIdXRProp = serializedObject.FindProperty("secondaryFireInputActionMapIdXR");
            dockingInputActionMapIdXRProp = serializedObject.FindProperty("dockingInputActionMapIdXR");

            #endif
            #endregion

            #region Find Properties - Oculus or Vive or Rewired or Unity Input System
#if SSC_OVR || SSC_REWIRED || VIU_PLUGIN || SSC_UIS
            primaryFireButtonEnabledProp = serializedObject.FindProperty("primaryFireButtonEnabled");
            secondaryFireButtonEnabledProp = serializedObject.FindProperty("secondaryFireButtonEnabled");
            dockingButtonEnabledProp = serializedObject.FindProperty("dockingButtonEnabled");
#endif
            #endregion SerializedProperties - Oculus or Vive or Rewired

            #region Properties - Custom Player Input
            cpiListProp = serializedObject.FindProperty("customPlayerInputList");
            cpiShowInEditorProp = serializedObject.FindProperty("customPlayerInputsShowInEditor");
            cpiIsListExpandedProp = serializedObject.FindProperty("isCustomPlayerInputListExpanded");
            #endregion

            playerInputModule.ValidateLegacyInput();

            #if SSC_OVR
            // Get once
            ovrPluginVersion = OVRPlugin.version.ToString();
            #endif

            #if SSC_UIS
            uisPlayerInput = playerInputModule.GetUISPlayerInput();
            uisVersion = SSCSetup.GetPackageVersion("Packages/com.unity.inputsystem/package.json");
            UISRefreshActions();
            #endif

            #if SSC_REWIRED
            FindRewiredInputManager();
            // Populate arrays for rewired input options
            RWRefreshActionCategories();
            RWRefreshActionsAll();
            #endif

            #if VIU_PLUGIN
            // Get once
            viveVersion = VIUVersion.current.ToString(); 
            #endif

            #if SCSM_XR && SSC_UIS
            xrVersion = SSCSetup.GetPackageVersion("Packages/com.unity.xr.management/package.json");
            openXRVersion = SSCSetup.GetPackageVersion("Packages/com.unity.xr.openxr/package.json");
            oculusXRVersion = SSCSetup.GetPackageVersion("Packages/com.unity.xr.oculus/package.json");
            if (playerInputModule.inputMode == PlayerInputModule.InputMode.UnityXR)
            {
                XRRefreshActionMaps();
                xrForceActionRefresh = true;
            }
            #endif

            // Get a reference to the ShipInput class instance used
            // to set data to the ShipControlModule. Used for debugging
            shipInput = playerInputModule.GetShipInput;

            shipControlModule = playerInputModule.GetComponent<ShipControlModule>();

            isRefreshing = false;
        }

        /// <summary>
        /// Called when the gameobject loses focus or Unity Editor enters/exits
        /// play mode
        /// </summary>
        void OnDestroy()
        {
            //#if UNITY_2019_1_OR_NEWER
            //SceneView.duringSceneGui -= SceneGUI;
            //#else
            //SceneView.onSceneGUIDelegate -= SceneGUI;
            //#endif
        }

        /// <summary>
        /// Gets called automatically 10 times per second
        /// Comment out if not required
        /// </summary>
        void OnInspectorUpdate()
        {
            // OnInspectorGUI() only registers events when the mouse is positioned over the custom editor window
            // This code forces OnInspectorGUI() to run every frame, so it registers events even when the mouse
            // is positioned over the scene view
            if (playerInputModule.allowRepaint) { Repaint(); }
        }

        #endregion

        #region OnInspectorGUI
        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            #region Initialise

            playerInputModule.allowRepaint = false;

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

            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(initialiseOnAwakeProp, initialiseOnAwakeContent);
            EditorGUILayout.PropertyField(isEnabledOnInitialiseProp, isEnabledOnInitialiseContent);
            if (isEnabledOnInitialiseProp.boolValue)
            {
                EditorGUILayout.PropertyField(isCustomInputsOnlyOnInitialiseProp, isCustomInputsOnlyOnInitialiseContent);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(inputModeProp, inputModeContent, GUILayout.MaxWidth(300f));
            if (EditorGUI.EndChangeCheck())
            {
                #if SCSM_XR && SSC_UIS
                if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.UnityXR)
                {
                    xrForceActionRefresh = true;
                    XRRefreshActionMaps();
                }
                else if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.UnityInputSystem)
                {
                    UISRefreshActions();
                }
                #endif
            }

            if (isAutoCruiseEnabledProp.boolValue && shipControlModule != null && shipControlModule.shipInstance != null && shipControlModule.shipInstance.brakeFlightAssistStrength > 0f)
            {
                EditorGUILayout.HelpBox(brakeAssistWarningContent, MessageType.Warning);
            }

            EditorGUILayout.PropertyField(isAutoCruiseEnabledProp, isAutoCruiseEnabledContent);

            #region Direct Keyboard Input

            if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.DirectKeyboard)
            {
                #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
                #region Direct Keyboard Horizontal input
                DrawFoldoutWithLabel(horizontalShowInEditorProp, horizontalHeaderContent);
                if (horizontalShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(horizontalInputPositiveKeycodeProp, positiveKeycodeContent);
                    EditorGUILayout.PropertyField(horizontalInputNegativeKeycodeProp, negativeKeycodeContent);
                    if (horizontalInputPositiveKeycodeProp.intValue == horizontalInputNegativeKeycodeProp.intValue
                        && horizontalInputPositiveKeycodeProp.intValue != 0)
                    {
                        EditorGUILayout.HelpBox(identicalKeycodesWarningContent, MessageType.Warning);
                    }
                    if (horizontalInputPositiveKeycodeProp.intValue != 0 || horizontalInputNegativeKeycodeProp.intValue != 0)
                    {
                        VerifyThrusterPresent(PIMCategory.Horizontal);
                    }

                    DrawSensitivity(horizontalInputPositiveKeycodeProp, horizontalInputNegativeKeycodeProp,
                    isSensitivityForHorizontalEnabledProp, sensitivityEnabledContent,
                    horizontalAxisSensitivityProp, sensitivityContent,
                    horizontalAxisGravityProp, gravityContent);
                    DrawDiscardData(isHorizontalDataDiscardedProp, discardDataContent);
                }
                #endregion

                #region Direct Keybaord Vertical input
                DrawFoldoutWithLabel(verticalShowInEditorProp, verticalHeaderContent);
                if (verticalShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(verticalInputPositiveKeycodeProp, positiveKeycodeContent);
                    EditorGUILayout.PropertyField(verticalInputNegativeKeycodeProp, negativeKeycodeContent);
                    if (verticalInputPositiveKeycodeProp.intValue == verticalInputNegativeKeycodeProp.intValue
                        && verticalInputPositiveKeycodeProp.intValue != 0)
                    {
                        EditorGUILayout.HelpBox(identicalKeycodesWarningContent, MessageType.Warning);
                    }
                    if (verticalInputPositiveKeycodeProp.intValue != 0 || verticalInputNegativeKeycodeProp.intValue != 0)
                    {
                        VerifyThrusterPresent(PIMCategory.Vertical);
                    }

                    DrawSensitivity(verticalInputPositiveKeycodeProp, verticalInputNegativeKeycodeProp,
                    isSensitivityForVerticalEnabledProp, sensitivityEnabledContent,
                    verticalAxisSensitivityProp, sensitivityContent,
                    verticalAxisGravityProp, gravityContent);
                    DrawDiscardData(isVerticalDataDiscardedProp, discardDataContent);
                }
                #endregion

                #region Direct keyboard Longitudinal input
                DrawFoldoutWithLabel(longitudinalShowInEditorProp, longitudinalHeaderContent);
                if (longitudinalShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(longitudinalInputPositiveKeycodeProp, positiveKeycodeContent);
                    EditorGUILayout.PropertyField(longitudinalInputNegativeKeycodeProp, negativeKeycodeContent);
                    if (longitudinalInputPositiveKeycodeProp.intValue == longitudinalInputNegativeKeycodeProp.intValue
                        && longitudinalInputPositiveKeycodeProp.intValue != 0)
                    {
                        EditorGUILayout.HelpBox(identicalKeycodesWarningContent, MessageType.Warning);
                    }
                    if (longitudinalInputPositiveKeycodeProp.intValue != 0 || longitudinalInputNegativeKeycodeProp.intValue != 0)
                    {
                        VerifyThrusterPresent(PIMCategory.Longitidinal);
                    }

                    DrawSensitivity(longitudinalInputPositiveKeycodeProp, longitudinalInputNegativeKeycodeProp,
                    isSensitivityForLongitudinalEnabledProp, sensitivityEnabledContent,
                    longitudinalAxisSensitivityProp, sensitivityContent,
                    longitudinalAxisGravityProp, gravityContent);
                    DrawDiscardData(isLongitudinalDataDiscardedProp, discardDataContent);
                }
                #endregion

                #region Direct Keyboard Pitch input
                DrawFoldoutWithLabel(pitchShowInEditorProp, pitchHeaderContent);
                if (pitchShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(isMouseForPitchEnabledDKIProp, dkiPitchInputMouseContent);
                    if (isMouseForPitchEnabledDKIProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(pitchSensitivityDKIProp, dkiPitchSensitivityContent);
                        EditorGUILayout.PropertyField(pitchDeadzoneDKIProp, dkiPitchDeadzoneContent);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(pitchInputPositiveKeycodeProp, positiveKeycodeContent);
                        EditorGUILayout.PropertyField(pitchInputNegativeKeycodeProp, negativeKeycodeContent);
                        if (pitchInputPositiveKeycodeProp.intValue == pitchInputNegativeKeycodeProp.intValue
                            && pitchInputPositiveKeycodeProp.intValue != 0)
                        {
                            EditorGUILayout.HelpBox(identicalKeycodesWarningContent, MessageType.Warning);
                        }

                        DrawSensitivity(pitchInputPositiveKeycodeProp, pitchInputNegativeKeycodeProp,
                        isSensitivityForPitchEnabledProp, sensitivityEnabledContent,
                        pitchAxisSensitivityProp, sensitivityContent,
                        pitchAxisGravityProp, gravityContent);
                    }
                    DrawDiscardData(isPitchDataDiscardedProp, discardDataContent);
                }
                #endregion

                #region Direct Keyboard Yaw input
                DrawFoldoutWithLabel(yawShowInEditorProp, yawHeaderContent);
                if (yawShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(isMouseForYawEnabledDKIProp, dkiYawInputMouseContent);
                    if (isMouseForYawEnabledDKIProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(yawSensitivityDKIProp, dkiYawSensitivityContent);
                        EditorGUILayout.PropertyField(yawDeadzoneDKIProp, dkiYawDeadzoneContent);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(yawInputPositiveKeycodeProp, positiveKeycodeContent);
                        EditorGUILayout.PropertyField(yawInputNegativeKeycodeProp, negativeKeycodeContent);
                        if (yawInputPositiveKeycodeProp.intValue == yawInputNegativeKeycodeProp.intValue
                            && yawInputPositiveKeycodeProp.intValue != 0)
                        {
                            EditorGUILayout.HelpBox(identicalKeycodesWarningContent, MessageType.Warning);
                        }

                        DrawSensitivity(yawInputPositiveKeycodeProp, yawInputNegativeKeycodeProp,
                        isSensitivityForYawEnabledProp, sensitivityEnabledContent,
                        yawAxisSensitivityProp, sensitivityContent,
                        yawAxisGravityProp, gravityContent);
                    }
                    DrawDiscardData(isYawDataDiscardedProp, discardDataContent);
                }
                #endregion

                #region Direct Keyboard Roll input
                DrawFoldoutWithLabel(rollShowInEditorProp, rollHeaderContent);
                if (rollShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(rollInputPositiveKeycodeProp, positiveKeycodeContent);
                    EditorGUILayout.PropertyField(rollInputNegativeKeycodeProp, negativeKeycodeContent);
                    if (rollInputPositiveKeycodeProp.intValue == rollInputNegativeKeycodeProp.intValue
                         && rollInputPositiveKeycodeProp.intValue != 0)
                    {
                        EditorGUILayout.HelpBox(identicalKeycodesWarningContent, MessageType.Warning);
                    }

                    DrawSensitivity(rollInputPositiveKeycodeProp, rollInputNegativeKeycodeProp,
                    isSensitivityForRollEnabledProp, sensitivityEnabledContent,
                    rollAxisSensitivityProp, sensitivityContent,
                    rollAxisGravityProp, gravityContent);
                    DrawDiscardData(isRollDataDiscardedProp, discardDataContent);
                }
                #endregion

                #region Direct Keyboard Primary fire input
                DrawFoldoutWithLabel(primaryFireShowInEditorProp, primaryFireHeaderContent);
                if (primaryFireShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(primaryFireInputKeycodeProp, singleKeycodeContent);
                    EditorGUILayout.PropertyField(primaryFireCanBeHeldProp, fireCanBeHeldContent);
                    DrawDiscardData(isPrimaryFireDataDiscardedProp, discardDataContent);
                }
                #endregion

                #region Direct Keyboard Secondary fire input
                DrawFoldoutWithLabel(secondaryFireShowInEditorProp, secondaryFireHeaderContent);
                if (secondaryFireShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(secondaryFireInputKeycodeProp, singleKeycodeContent);
                    EditorGUILayout.PropertyField(secondaryFireCanBeHeldProp, fireCanBeHeldContent);
                    DrawDiscardData(isSecondaryFireDataDiscardedProp, discardDataContent);
                }
                #endregion

                #region Direct Keyboard Docking input
                DrawFoldoutWithLabel(dockingShowInEditorProp, dockingHeaderContent);
                if (dockingShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(dockingInputKeycodeProp, singleKeycodeContent);
                    DrawDiscardData(isDockDataDiscardedProp, discardDataContent);
                }
                #endregion
                
                #else
                EditorGUILayout.HelpBox("Legacy Unity Input is not enabled in Project Player settings.", MessageType.Warning, true);
                #endif
            }

            #endregion Direct Keyboard Input

            #region Legacy Unity Input System

            else if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.LegacyUnity)
            {
                #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
                #region Horizontal input

                DrawFoldoutWithLabel(horizontalShowInEditorProp, horizontalHeaderContent);
                if (horizontalShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(horizontalInputAxisModeProp, inputAxisModeContent);
                    if (horizontalInputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.NoInput) { }
                    else if (horizontalInputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.SingleAxis)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(positiveHorizontalInputAxisNameProp, singleAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPositiveHorizontalInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(positiveHorizontalInputAxisNameProp.stringValue, false);
                        }
                        if (!isPositiveHorizontalInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }
                        EditorGUILayout.PropertyField(invertHorizontalInputAxisProp, invertInputAxisContent);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(positiveHorizontalInputAxisNameProp, positiveAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPositiveHorizontalInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(positiveHorizontalInputAxisNameProp.stringValue, false);
                        }
                        if (!isPositiveHorizontalInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(negativeHorizontalInputAxisNameProp, negativeAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isNegativeHorizontalInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(negativeHorizontalInputAxisNameProp.stringValue, false);
                        }
                        if (!isNegativeHorizontalInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        if (positiveHorizontalInputAxisNameProp.stringValue == negativeHorizontalInputAxisNameProp.stringValue)
                        {
                            EditorGUILayout.HelpBox(identicalAxisNamesWarningContent, MessageType.Warning);
                        }
                    }

                    if (horizontalInputAxisModeProp.intValue != (int)PlayerInputModule.InputAxisMode.NoInput) { VerifyThrusterPresent(PIMCategory.Horizontal); }

                    DrawSensitivity(horizontalInputAxisModeProp,
                    isSensitivityForHorizontalEnabledProp, sensitivityEnabledContent,
                    horizontalAxisSensitivityProp, sensitivityContent,
                    horizontalAxisGravityProp, gravityContent);
                    DrawDiscardData(isHorizontalDataDiscardedProp, discardDataContent);
                }

                #endregion

                #region Vertical input

                DrawFoldoutWithLabel(verticalShowInEditorProp, verticalHeaderContent);
                if (verticalShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(verticalInputAxisModeProp, inputAxisModeContent);
                    if (verticalInputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.NoInput) { }
                    else if (verticalInputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.SingleAxis)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(positiveVerticalInputAxisNameProp, singleAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPositiveVerticalInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(positiveVerticalInputAxisNameProp.stringValue, false);
                        }
                        if (!isPositiveVerticalInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }
                        EditorGUILayout.PropertyField(invertVerticalInputAxisProp, invertInputAxisContent);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(positiveVerticalInputAxisNameProp, positiveAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPositiveVerticalInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(positiveVerticalInputAxisNameProp.stringValue, false);
                        }
                        if (!isPositiveVerticalInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(negativeVerticalInputAxisNameProp, negativeAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isNegativeVerticalInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(negativeVerticalInputAxisNameProp.stringValue, false);
                        }
                        if (!isNegativeVerticalInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        if (positiveVerticalInputAxisNameProp.stringValue == negativeVerticalInputAxisNameProp.stringValue)
                        {
                            EditorGUILayout.HelpBox(identicalAxisNamesWarningContent, MessageType.Warning);
                        }
                    }

                    if (verticalInputAxisModeProp.intValue != (int)PlayerInputModule.InputAxisMode.NoInput) { VerifyThrusterPresent(PIMCategory.Vertical); }

                    DrawSensitivity(verticalInputAxisModeProp,
                    isSensitivityForVerticalEnabledProp, sensitivityEnabledContent,
                    verticalAxisSensitivityProp, sensitivityContent,
                    verticalAxisGravityProp, gravityContent);
                    DrawDiscardData(isVerticalDataDiscardedProp, discardDataContent);
                }

                #endregion

                #region Longitudinal input

                DrawFoldoutWithLabel(longitudinalShowInEditorProp, longitudinalHeaderContent);
                if (longitudinalShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(longitudinalInputAxisModeProp, inputAxisModeContent);
                    if (longitudinalInputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.NoInput) { }
                    else if (longitudinalInputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.SingleAxis)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(positiveLongitudinalInputAxisNameProp, singleAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPositiveLongitudinalInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(positiveLongitudinalInputAxisNameProp.stringValue, false);
                        }
                        if (!isPositiveLongitudinalInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        EditorGUILayout.PropertyField(invertLongitudinalInputAxisProp, invertInputAxisContent);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(positiveLongitudinalInputAxisNameProp, positiveAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPositiveLongitudinalInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(positiveLongitudinalInputAxisNameProp.stringValue, false);
                        }
                        if (!isPositiveLongitudinalInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(negativeLongitudinalInputAxisNameProp, negativeAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isNegativeLongitudinalInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(negativeLongitudinalInputAxisNameProp.stringValue, false);
                        }
                        if (!isNegativeLongitudinalInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        if (positiveLongitudinalInputAxisNameProp.stringValue == negativeLongitudinalInputAxisNameProp.stringValue)
                        {
                            EditorGUILayout.HelpBox(identicalAxisNamesWarningContent, MessageType.Warning);
                        }
                    }

                    if (longitudinalInputAxisModeProp.intValue != (int)PlayerInputModule.InputAxisMode.NoInput) { VerifyThrusterPresent(PIMCategory.Longitidinal); }

                    DrawSensitivity(longitudinalInputAxisModeProp,
                    isSensitivityForLongitudinalEnabledProp, sensitivityEnabledContent,
                    longitudinalAxisSensitivityProp, sensitivityContent,
                    longitudinalAxisGravityProp, gravityContent);
                    DrawDiscardData(isLongitudinalDataDiscardedProp, discardDataContent);
                }

                #endregion

                #region Pitch input

                DrawFoldoutWithLabel(pitchShowInEditorProp, pitchHeaderContent);
                if (pitchShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(isMouseForPitchEnabledLISProp, lisPitchInputMouseContent);
                    if (isMouseForPitchEnabledLISProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(pitchSensitivityLISProp, lisPitchSensitivityContent);
                        EditorGUILayout.PropertyField(pitchDeadzoneLISProp, lisPitchDeadzoneContent);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(pitchInputAxisModeProp, inputAxisModeContent);
                        if (pitchInputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.NoInput) { }
                        else if (pitchInputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.SingleAxis)
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(positivePitchInputAxisNameProp, singleAxisNameContent);
                            if (EditorGUI.EndChangeCheck())
                            {
                                isPositivePitchInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(positivePitchInputAxisNameProp.stringValue, false);
                            }
                            if (!isPositivePitchInputAxisValidProp.boolValue)
                            {
                                EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                            }

                            EditorGUILayout.PropertyField(invertPitchInputAxisProp, invertInputAxisContent);
                        }
                        else
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(positivePitchInputAxisNameProp, positiveAxisNameContent);
                            if (EditorGUI.EndChangeCheck())
                            {
                                isPositivePitchInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(positivePitchInputAxisNameProp.stringValue, false);
                            }
                            if (!isPositivePitchInputAxisValidProp.boolValue)
                            {
                                EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                            }

                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(negativePitchInputAxisNameProp, negativeAxisNameContent);
                            if (EditorGUI.EndChangeCheck())
                            {
                                isNegativePitchInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(negativePitchInputAxisNameProp.stringValue, false);
                            }
                            if (!isNegativePitchInputAxisValidProp.boolValue)
                            {
                                EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                            }

                            if (positivePitchInputAxisNameProp.stringValue == negativePitchInputAxisNameProp.stringValue)
                            {
                                EditorGUILayout.HelpBox(identicalAxisNamesWarningContent, MessageType.Warning);
                            }
                        }
                        DrawSensitivity(pitchInputAxisModeProp,
                        isSensitivityForPitchEnabledProp, sensitivityEnabledContent,
                        pitchAxisSensitivityProp, sensitivityContent,
                        pitchAxisGravityProp, gravityContent);
                    }
                    DrawDiscardData(isPitchDataDiscardedProp, discardDataContent);
                }

                #endregion

                #region Yaw input

                DrawFoldoutWithLabel(yawShowInEditorProp, yawHeaderContent);
                if (yawShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(isMouseForYawEnabledLISProp, lisYawInputMouseContent);
                    if (isMouseForYawEnabledLISProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(yawSensitivityLISProp, lisYawSensitivityContent);
                        EditorGUILayout.PropertyField(yawDeadzoneLISProp, lisYawDeadzoneContent);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(yawInputAxisModeProp, inputAxisModeContent);
                        if (yawInputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.NoInput) { }
                        else if (yawInputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.SingleAxis)
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(positiveYawInputAxisNameProp, singleAxisNameContent);
                            if (EditorGUI.EndChangeCheck())
                            {
                                isPositiveYawInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(positiveYawInputAxisNameProp.stringValue, false);
                            }
                            if (!isPositiveYawInputAxisValidProp.boolValue)
                            {
                                EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                            }

                            EditorGUILayout.PropertyField(invertYawInputAxisProp, invertInputAxisContent);
                        }
                        else
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(positiveYawInputAxisNameProp, positiveAxisNameContent);
                            if (EditorGUI.EndChangeCheck())
                            {
                                isPositiveYawInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(positiveYawInputAxisNameProp.stringValue, false);
                            }
                            if (!isPositiveYawInputAxisValidProp.boolValue)
                            {
                                EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                            }

                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(negativeYawInputAxisNameProp, negativeAxisNameContent);
                            if (EditorGUI.EndChangeCheck())
                            {
                                isNegativeYawInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(negativeYawInputAxisNameProp.stringValue, false);
                            }
                            if (!isNegativeYawInputAxisValidProp.boolValue)
                            {
                                EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                            }

                            if (positiveYawInputAxisNameProp.stringValue == negativeYawInputAxisNameProp.stringValue)
                            {
                                EditorGUILayout.HelpBox(identicalAxisNamesWarningContent, MessageType.Warning);
                            }
                        }

                        DrawSensitivity(yawInputAxisModeProp,
                        isSensitivityForYawEnabledProp, sensitivityEnabledContent,
                        yawAxisSensitivityProp, sensitivityContent,
                        yawAxisGravityProp, gravityContent);
                    }
                    DrawDiscardData(isYawDataDiscardedProp, discardDataContent);
                }

                #endregion

                #region Roll input

                DrawFoldoutWithLabel(rollShowInEditorProp, rollHeaderContent);
                if (rollShowInEditorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(rollInputAxisModeProp, inputAxisModeContent);
                    if (rollInputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.NoInput) { }
                    else if (rollInputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.SingleAxis)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(positiveRollInputAxisNameProp, singleAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPositiveRollInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(positiveRollInputAxisNameProp.stringValue, false);
                        }
                        if (!isPositiveRollInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }
                        EditorGUILayout.PropertyField(invertRollInputAxisProp, invertInputAxisContent);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(positiveRollInputAxisNameProp, positiveAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isPositiveRollInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(positiveRollInputAxisNameProp.stringValue, false);
                        }
                        if (!isPositiveRollInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(negativeRollInputAxisNameProp, negativeAxisNameContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isNegativeRollInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(negativeRollInputAxisNameProp.stringValue, false);
                        }
                        if (!isNegativeRollInputAxisValidProp.boolValue)
                        {
                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                        }

                        if (positiveRollInputAxisNameProp.stringValue == negativeRollInputAxisNameProp.stringValue)
                        {
                            EditorGUILayout.HelpBox(identicalAxisNamesWarningContent, MessageType.Warning);
                        }
                    }
                    DrawSensitivity(rollInputAxisModeProp,
                    isSensitivityForRollEnabledProp, sensitivityEnabledContent,
                    rollAxisSensitivityProp, sensitivityContent,
                    rollAxisGravityProp, gravityContent);
                    DrawDiscardData(isRollDataDiscardedProp, discardDataContent);
                }

                #endregion

                #region Primary Fire Input

                DrawFoldoutWithLabel(primaryFireShowInEditorProp, primaryFireHeaderContent);
                if (primaryFireShowInEditorProp.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(primaryFireInputAxisNameProp, singleAxisNameContent);
                    EditorGUILayout.PropertyField(primaryFireCanBeHeldProp, fireCanBeHeldContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        isPrimaryFireInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(primaryFireInputAxisNameProp.stringValue, false);
                    }
                    if (!isPrimaryFireInputAxisValidProp.boolValue)
                    {
                        EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                    }
                    DrawDiscardData(isPrimaryFireDataDiscardedProp, discardDataContent);
                }

                #endregion

                #region Secondary Fire Input

                DrawFoldoutWithLabel(secondaryFireShowInEditorProp, secondaryFireHeaderContent);
                if (secondaryFireShowInEditorProp.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(secondaryFireInputAxisNameProp, singleAxisNameContent);
                    EditorGUILayout.PropertyField(secondaryFireCanBeHeldProp, fireCanBeHeldContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        isSecondaryFireInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(secondaryFireInputAxisNameProp.stringValue, false);
                    }
                    if (!isSecondaryFireInputAxisValidProp.boolValue)
                    {
                        EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                    }
                    DrawDiscardData(isSecondaryFireDataDiscardedProp, discardDataContent);
                }

                #endregion

                #region Docking Input

                DrawFoldoutWithLabel(dockingShowInEditorProp, dockingHeaderContent);
                if (dockingShowInEditorProp.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(dockingInputAxisNameProp, singleAxisNameContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        isDockingInputAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(dockingInputAxisNameProp.stringValue, false);
                    }
                    if (!isDockingInputAxisValidProp.boolValue)
                    {
                        EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                    }
                    DrawDiscardData(isDockDataDiscardedProp, discardDataContent);
                }

                #endregion
                #else
                EditorGUILayout.HelpBox("Legacy Unity Input is not enabled in Project Player settings.", MessageType.Warning, true);
                #endif
            }

            #endregion Legacy Unity Input System

            #region Unity Input System (add with Unity package manager)
            else if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.UnityInputSystem)
            {
                #if SSC_UIS

                if (uisPlayerInput != null)
                {
                    EditorGUILayout.HelpBox("Setup the attached Player Input component, refresh the Available Actions, then configure the Input Axis as required.", MessageType.Info, true);
                }
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(" Input System version", GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(uisVersion);
                EditorGUILayout.EndHorizontal();             

                if (uisPlayerInput == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("The Unity Input System Player Input component is missing.", MessageType.Error, true);
                    if (GUILayout.Button("Fix Now", GUILayout.MaxWidth(70f), GUILayout.MaxHeight(35f)) )
                    {
                        Undo.AddComponent(playerInputModule.gameObject, typeof(UnityEngine.InputSystem.PlayerInput));
                        // Ensure the PlayerInputModule reference is updated for this PlayerInput component
                        uisPlayerInput = playerInputModule.GetUISPlayerInput();
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
                    EditorGUILayout.HelpBox("This version of Sci-Fi Ship Controller only supports a Behaviour of Invoke Unity Events on the Player Input component", MessageType.Error, true);
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

                            horizontalInputAxisModeProp.intValue = (int)PlayerInputModule.InputAxisMode.NoInput;
                            verticalInputAxisModeProp.intValue = (int)PlayerInputModule.InputAxisMode.SingleAxis;
                            longitudinalInputAxisModeProp.intValue = (int)PlayerInputModule.InputAxisMode.SingleAxis;
                            pitchInputAxisModeProp.intValue = (int)PlayerInputModule.InputAxisMode.SingleAxis;
                            yawInputAxisModeProp.intValue = (int)PlayerInputModule.InputAxisMode.SingleAxis;
                            primaryFireButtonEnabledProp.intValue = (int)PlayerInputModule.InputAxisMode.SingleAxis;
                            secondaryFireButtonEnabledProp.intValue = (int)PlayerInputModule.InputAxisMode.SingleAxis;
                            dockingButtonEnabledProp.intValue = (int)PlayerInputModule.InputAxisMode.NoInput;

                            isMouseForPitchEnabledUISProp.boolValue = false;
                            isMouseForYawEnabledUISProp.boolValue = false;

                            UISSetActionId("Vertical", 0, positiveVerticalInputActionIdUISProp, positiveVerticalInputActionDataSlotUISProp);
                            UISSetActionId("Longitudinal", 0, positiveLongitudinalInputActionIdUISProp, positiveLongitudinalInputActionDataSlotUISProp);
                            UISSetActionId("Pitch", 0, positivePitchInputActionIdUISProp, positivePitchInputActionDataSlotUISProp);
                            UISSetActionId("Yaw", 0, positiveYawInputActionIdUISProp, positiveYawInputActionDataSlotUISProp);
                            UISSetActionId("Roll", 0, positiveRollInputActionIdUISProp, positiveRollInputActionDataSlotUISProp);
                            UISSetActionId("PrimaryFire", 0, primaryFireInputActionIdUISProp, primaryFireInputActionDataSlotUISProp);
                            UISSetActionId("SecondaryFire", 0, secondaryFireInputActionIdUISProp, secondaryFireInputActionDataSlotUISProp);
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

                    #region UIS On-screen controls
                    DrawFoldoutWithLabel(onscreenControlsShowInEditorProp, uisOnScreenControlsContent);
                    if (onscreenControlsShowInEditorProp.boolValue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(" Add Virtual Joysticks", GUILayout.Width(defaultEditorLabelWidth));
                        if (GUILayout.Button("Left Stick", GUILayout.MaxWidth(75f))) { UISAddOnScreenControls("LeftStick"); }
                        if (GUILayout.Button("Right Stick", GUILayout.MaxWidth(75f))) { UISAddOnScreenControls("RightStick"); }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(" Add Buttons", GUILayout.Width(defaultEditorLabelWidth));
                        if (GUILayout.Button("Left Btns", GUILayout.MaxWidth(75f))) { UISAddOnScreenControls("LeftButtons"); }
                        if (GUILayout.Button("Right Btns", GUILayout.MaxWidth(75f))) { UISAddOnScreenControls("RightButtons"); }
                        // We currently don't have a quit input action
                        if (GUILayout.Button("Quit Btn", GUILayout.MaxWidth(70f))) { UISAddOnScreenQuit(); }
                        EditorGUILayout.EndHorizontal();
                    }
                    #endregion

                    #region Horizontal Input
                    DrawFoldoutWithLabel(horizontalShowInEditorProp, horizontalHeaderContent);
                    if (horizontalShowInEditorProp.boolValue)
                    {
                        DrawUnityInputSystem(horizontalInputAxisModeProp, inputSystemAxisModeContent,
                        positiveHorizontalInputActionIdUISProp, positiveHorizontalInputActionDataSlotUISProp,
                        uisHorizontalInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionIDs, true
                        );
                        EditorGUILayout.PropertyField(invertHorizontalInputAxisProp, invertInputAxisContent);
                        DrawSensitivity(horizontalInputAxisModeProp,
                        isSensitivityForHorizontalEnabledProp, sensitivityEnabledContent,
                        horizontalAxisSensitivityProp, sensitivityContent,
                        horizontalAxisGravityProp, gravityContent);
                        DrawDiscardData(isHorizontalDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Vertical Input
                    DrawFoldoutWithLabel(verticalShowInEditorProp, verticalHeaderContent);
                    if (verticalShowInEditorProp.boolValue)
                    {
                        DrawUnityInputSystem(verticalInputAxisModeProp, inputSystemAxisModeContent,
                        positiveVerticalInputActionIdUISProp, positiveVerticalInputActionDataSlotUISProp,
                        uisVerticalInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionIDs, true
                        );
                        EditorGUILayout.PropertyField(invertVerticalInputAxisProp, invertInputAxisContent);
                        DrawSensitivity(verticalInputAxisModeProp,
                        isSensitivityForVerticalEnabledProp, sensitivityEnabledContent,
                        verticalAxisSensitivityProp, sensitivityContent,
                        verticalAxisGravityProp, gravityContent);
                        DrawDiscardData(isVerticalDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Longitudinal input
                    DrawFoldoutWithLabel(longitudinalShowInEditorProp, longitudinalHeaderContent);
                    if (longitudinalShowInEditorProp.boolValue)
                    {
                        DrawUnityInputSystem(longitudinalInputAxisModeProp, inputSystemAxisModeContent,
                        positiveLongitudinalInputActionIdUISProp, positiveLongitudinalInputActionDataSlotUISProp,
                        uisLongitudinalInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionIDs, true
                        );
                        EditorGUILayout.PropertyField(invertLongitudinalInputAxisProp, invertInputAxisContent);
                        DrawSensitivity(longitudinalInputAxisModeProp,
                        isSensitivityForLongitudinalEnabledProp, sensitivityEnabledContent,
                        longitudinalAxisSensitivityProp, sensitivityContent,
                        longitudinalAxisGravityProp, gravityContent);
                        DrawDiscardData(isLongitudinalDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Pitch input
                    DrawFoldoutWithLabel(pitchShowInEditorProp, pitchHeaderContent);
                    if (pitchShowInEditorProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(isMouseForPitchEnabledUISProp, uisPitchInputMouseContent);
                        if (isMouseForPitchEnabledUISProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(pitchSensitivityUISProp, uisPitchSensitivityContent);
                            EditorGUILayout.PropertyField(pitchDeadzoneUISProp, uisPitchDeadzoneContent);
                        }
                        else
                        {
                            DrawUnityInputSystem(pitchInputAxisModeProp, inputSystemAxisModeContent,
                            positivePitchInputActionIdUISProp, positivePitchInputActionDataSlotUISProp,
                            uisPitchInputAContent, uisInputATypeContent, uisInputCTypeContent,
                            uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionIDs, true
                            );
                            EditorGUILayout.PropertyField(invertPitchInputAxisProp, invertInputAxisContent);
                            DrawSensitivity(pitchInputAxisModeProp,
                            isSensitivityForPitchEnabledProp, sensitivityEnabledContent,
                            pitchAxisSensitivityProp, sensitivityContent,
                            pitchAxisGravityProp, gravityContent);
                        }
                        DrawDiscardData(isPitchDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Yaw input
                    DrawFoldoutWithLabel(yawShowInEditorProp, yawHeaderContent);
                    if (yawShowInEditorProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(isMouseForYawEnabledUISProp, uisYawInputMouseContent);
                        if (isMouseForYawEnabledUISProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(yawSensitivityUISProp, uisYawSensitivityContent);
                            EditorGUILayout.PropertyField(yawDeadzoneUISProp, uisYawDeadzoneContent);
                        }
                        else
                        {
                            DrawUnityInputSystem(yawInputAxisModeProp, inputSystemAxisModeContent,
                            positiveYawInputActionIdUISProp, positiveYawInputActionDataSlotUISProp,
                            uisYawInputAContent, uisInputATypeContent, uisInputCTypeContent,
                            uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionIDs, true
                            );
                            EditorGUILayout.PropertyField(invertYawInputAxisProp, invertInputAxisContent);
                            DrawSensitivity(yawInputAxisModeProp,
                            isSensitivityForYawEnabledProp, sensitivityEnabledContent,
                            yawAxisSensitivityProp, sensitivityContent,
                            yawAxisGravityProp, gravityContent);
                        }
                        DrawDiscardData(isYawDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Roll input
                    DrawFoldoutWithLabel(rollShowInEditorProp, rollHeaderContent);
                    if (rollShowInEditorProp.boolValue)
                    {
                        DrawUnityInputSystem(rollInputAxisModeProp, inputSystemAxisModeContent,
                        positiveRollInputActionIdUISProp, positiveRollInputActionDataSlotUISProp,
                        uisRollInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionIDs, true
                        );
                        EditorGUILayout.PropertyField(invertRollInputAxisProp, invertInputAxisContent);
                        DrawSensitivity(rollInputAxisModeProp,
                        isSensitivityForRollEnabledProp, sensitivityEnabledContent,
                        rollAxisSensitivityProp, sensitivityContent,
                        rollAxisGravityProp, gravityContent);
                        DrawDiscardData(isRollDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Primary Fire Input
                    DrawFoldoutWithLabel(primaryFireShowInEditorProp, primaryFireHeaderContent);
                    if (primaryFireShowInEditorProp.boolValue)
                    {
                        DrawUnityInputSystemFire(primaryFireButtonEnabledProp, uisPrimaryFireEnabledContent,
                        primaryFireInputActionIdUISProp, primaryFireInputActionDataSlotUISProp,
                        primaryFireCanBeHeldProp, uisPrimaryFireInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        fireCanBeHeldContent, uisDataSlots2, uisDataSlots3, numActionIDs
                        );
                        DrawDiscardData(isPrimaryFireDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Secondary Fire Input
                    DrawFoldoutWithLabel(secondaryFireShowInEditorProp, secondaryFireHeaderContent);
                    if (secondaryFireShowInEditorProp.boolValue)
                    {
                        DrawUnityInputSystemFire(secondaryFireButtonEnabledProp, uisSecondaryFireEnabledContent,
                        secondaryFireInputActionIdUISProp, secondaryFireInputActionDataSlotUISProp,
                        secondaryFireCanBeHeldProp, uisSecondaryFireInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        fireCanBeHeldContent, uisDataSlots2, uisDataSlots3, numActionIDs
                        );
                        DrawDiscardData(isSecondaryFireDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Docking Input
                    DrawFoldoutWithLabel(dockingShowInEditorProp, dockingHeaderContent);
                    if (dockingShowInEditorProp.boolValue)
                    {
                        DrawUnityInputSystemFire(dockingButtonEnabledProp, uisDockingEnabledContent,
                        dockingInputActionIdUISProp, dockingInputActionDataSlotUISProp,
                        null, uisDockingInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        null, uisDataSlots2, uisDataSlots3, numActionIDs
                        );
                        DrawDiscardData(isDockDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Rumble Controller output
                    DrawFoldoutWithLabel(rumbleShowInEditorProp, rumbleHeaderContent);
                    if (rumbleShowInEditorProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(maxRumble1Prop, maxRumble1Content);
                        EditorGUILayout.PropertyField(maxRumble2Prop, maxRumble2Content);
                    }
                    #endregion
                }
                #else
                EditorGUILayout.HelpBox("Could not find the Unity Input System. Did you install Input System 1.0.0 or newer from the Package Manager?", MessageType.Warning, true);
                #endif
            }
            #endregion Unity Input System

            #region Oculus OVR API input
            else if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.OculusAPI && !isRefreshing)
            {
                #if SSC_OVR
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(" Oculus Plugin version ", GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(ovrPluginVersion);
                EditorGUILayout.EndHorizontal();

                //EditorGUILayout.HelpBox("This feature is in technical preview", MessageType.Warning);

                EditorGUILayout.PropertyField(isOVRInputUpdatedIfRequiredProp, ovrIsInputUpdateRequiredContent);
                
                #region OVR Horizontal Input
                DrawFoldoutWithLabel(horizontalShowInEditorProp, horizontalHeaderContent);
                if (horizontalShowInEditorProp.boolValue)
                {
                    DrawOVRInput(PIMCategory.Horizontal, horizontalInputAxisModeProp, ovrHorizontalInputTypeProp,
                    positiveHorizontalInputOVRProp, negativeHorizontalInputOVRProp,
                    ovrHorizontalInputTypeContent,
                    ovrHorizontalPositiveInputContent,
                    ovrHorizontalNegativeInputContent,
                    ovrHorizontalInputContent
                    );

                    DrawSensitivity(horizontalInputAxisModeProp,
                    isSensitivityForHorizontalEnabledProp, sensitivityEnabledContent,
                    horizontalAxisSensitivityProp, sensitivityContent,
                    horizontalAxisGravityProp, gravityContent);
                    DrawDiscardData(isHorizontalDataDiscardedProp, discardDataContent);
                }
                #endregion OVR Horizontal Input             

                #region OVR Vertical Input
                DrawFoldoutWithLabel(verticalShowInEditorProp, verticalHeaderContent);
                if (verticalShowInEditorProp.boolValue)
                {
                    DrawOVRInput(PIMCategory.Vertical, verticalInputAxisModeProp, ovrVerticalInputTypeProp,
                    positiveVerticalInputOVRProp, negativeVerticalInputOVRProp,
                    ovrVerticalInputTypeContent,
                    ovrVerticalPositiveInputContent,
                    ovrVerticalNegativeInputContent,
                    ovrVerticalInputContent
                    );

                    DrawSensitivity(verticalInputAxisModeProp,
                    isSensitivityForVerticalEnabledProp, sensitivityEnabledContent,
                    verticalAxisSensitivityProp, sensitivityContent,
                    verticalAxisGravityProp, gravityContent);
                    DrawDiscardData(isVerticalDataDiscardedProp, discardDataContent);
                }
                #endregion OVR Vertical Input

                #region OVR Longitudinal Input
                DrawFoldoutWithLabel(longitudinalShowInEditorProp, longitudinalHeaderContent);
                if (longitudinalShowInEditorProp.boolValue)
                {
                    DrawOVRInput(PIMCategory.Longitidinal, longitudinalInputAxisModeProp, ovrLongitudinalInputTypeProp,
                    positiveLongitudinalInputOVRProp, negativeLongitudinalInputOVRProp,
                    ovrLongitudinalInputTypeContent,
                    ovrLongitudinalPositiveInputContent,
                    ovrLongitudinalNegativeInputContent,
                    ovrLongitudinalInputContent
                    );

                    DrawSensitivity(longitudinalInputAxisModeProp,
                    isSensitivityForLongitudinalEnabledProp, sensitivityEnabledContent,
                    longitudinalAxisSensitivityProp, sensitivityContent,
                    longitudinalAxisGravityProp, gravityContent);
                    DrawDiscardData(isLongitudinalDataDiscardedProp, discardDataContent);
                }
                #endregion OVR Longitudinal Input

                #region OVR Pitch Input
                DrawFoldoutWithLabel(pitchShowInEditorProp, pitchHeaderContent);
                if (pitchShowInEditorProp.boolValue)
                {
                    DrawOVRInput(PIMCategory.Pitch, pitchInputAxisModeProp, ovrPitchInputTypeProp,
                    positivePitchInputOVRProp, negativePitchInputOVRProp,
                    ovrPitchInputTypeContent,
                    ovrPitchPositiveInputContent,
                    ovrPitchNegativeInputContent,
                    ovrPitchInputContent
                    );

                    DrawSensitivity(pitchInputAxisModeProp,
                    isSensitivityForPitchEnabledProp, sensitivityEnabledContent,
                    pitchAxisSensitivityProp, sensitivityContent,
                    pitchAxisGravityProp, gravityContent);
                    DrawDiscardData(isPitchDataDiscardedProp, discardDataContent);
                }
                #endregion OVR Pitch Input

                #region OVR Yaw Input
                DrawFoldoutWithLabel(yawShowInEditorProp, yawHeaderContent);
                if (yawShowInEditorProp.boolValue)
                {
                    DrawOVRInput(PIMCategory.Yaw, yawInputAxisModeProp, ovrYawInputTypeProp,
                    positiveYawInputOVRProp, negativeYawInputOVRProp,
                    ovrYawInputTypeContent,
                    ovrYawPositiveInputContent,
                    ovrYawNegativeInputContent,
                    ovrYawInputContent
                    );

                    DrawSensitivity(yawInputAxisModeProp,
                    isSensitivityForYawEnabledProp, sensitivityEnabledContent,
                    yawAxisSensitivityProp, sensitivityContent,
                    yawAxisGravityProp, gravityContent);
                    DrawDiscardData(isYawDataDiscardedProp, discardDataContent);
                }
                #endregion OVR Yaw Input

                #region OVR Roll Input
                DrawFoldoutWithLabel(rollShowInEditorProp, rollHeaderContent);
                if (rollShowInEditorProp.boolValue)
                {
                    DrawOVRInput(PIMCategory.Roll, rollInputAxisModeProp, ovrRollInputTypeProp,
                    positiveRollInputOVRProp, negativeRollInputOVRProp,
                    ovrRollInputTypeContent,
                    ovrRollPositiveInputContent,
                    ovrRollNegativeInputContent,
                    ovrRollInputContent
                    );

                    DrawSensitivity(rollInputAxisModeProp,
                    isSensitivityForRollEnabledProp, sensitivityEnabledContent,
                    rollAxisSensitivityProp, sensitivityContent,
                    rollAxisGravityProp, gravityContent);
                    DrawDiscardData(isRollDataDiscardedProp, discardDataContent);
                }
                #endregion OVR Roll Input

                #region OVR Primary Fire Input
                DrawFoldoutWithLabel(primaryFireShowInEditorProp, primaryFireHeaderContent);
                if (primaryFireShowInEditorProp.boolValue)
                {
                    DrawOVRInput(primaryFireButtonEnabledProp, primaryFireInputOVRProp, primaryFireCanBeHeldProp, ovrPrimaryFireEnabledContent, ovrPrimaryFireInputContent, fireCanBeHeldContent);
                    DrawDiscardData(isPrimaryFireDataDiscardedProp, discardDataContent);
                }
                #endregion OVR Primary Fire Input

                #region OVR Secondary Fire Input
                DrawFoldoutWithLabel(secondaryFireShowInEditorProp, secondaryFireHeaderContent);
                if (secondaryFireShowInEditorProp.boolValue)
                {
                    DrawOVRInput(secondaryFireButtonEnabledProp, secondaryFireInputOVRProp, secondaryFireCanBeHeldProp, ovrSecondaryFireEnabledContent, ovrSecondaryFireInputContent, fireCanBeHeldContent);
                    DrawDiscardData(isSecondaryFireDataDiscardedProp, discardDataContent);
                }
                #endregion OVR Secondary Fire Input

                #region OVR Docking Input
                DrawFoldoutWithLabel(dockingShowInEditorProp, dockingHeaderContent);
                if (dockingShowInEditorProp.boolValue)
                {
                    DrawOVRInput(dockingButtonEnabledProp, dockingInputOVRProp, null, ovrDockingEnabledContent, ovrDockingInputContent, null);
                    DrawDiscardData(isDockDataDiscardedProp, discardDataContent);
                }
                #endregion OVR Docking Input

                #else
                EditorGUILayout.HelpBox("Could not find Oculus OVR API. Did you install the Oculus Integration package 1.3.2 or newer?", MessageType.Warning, true);
                #endif
            }
            #endregion Oculus API input

            #region Rewired Input
            else if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.Rewired && !isRefreshing)
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

                    #region Horizontal Input
                    DrawFoldoutWithLabel(horizontalShowInEditorProp, horizontalHeaderContent);
                    if (horizontalShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(PIMCategory.Horizontal, horizontalInputAxisModeProp, positiveHorizontalInputActionIdProp, negativeHorizontalInputActionIdProp,
                                         rwHorizontalInputACPositiveContent, rwHorizontalInputACNegativeContent, rwHorizontalInputACContent,
                                         rwHorizontalInputAIPositiveContent, rwHorizontalInputAINegativeContent, rwHorizontalInputAIContent,
                                         ref actionIDsHorizPositive, ref actionIDsHorizNegative, ref ActionGUIContentHorizPositive, ref ActionGUIContentHorizNegative, numActionCategories);

                        DrawSensitivity(horizontalInputAxisModeProp,
                        isSensitivityForHorizontalEnabledProp, sensitivityEnabledContent,
                        horizontalAxisSensitivityProp, sensitivityContent,
                        horizontalAxisGravityProp, gravityContent);
                        DrawDiscardData(isHorizontalDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Vertical Input
                    DrawFoldoutWithLabel(verticalShowInEditorProp, verticalHeaderContent);
                    if (verticalShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(PIMCategory.Vertical, verticalInputAxisModeProp, positiveVerticalInputActionIdProp, negativeVerticalInputActionIdProp,
                                         rwVerticalInputACPositiveContent, rwVerticalInputACNegativeContent, rwVerticalInputACContent,
                                         rwVerticalInputAIPositiveContent, rwVerticalInputAINegativeContent, rwVerticalInputAIContent,
                                         ref actionIDsVertPositive, ref actionIDsVertNegative, ref ActionGUIContentVertPositive, ref ActionGUIContentVertNegative, numActionCategories);

                        DrawSensitivity(verticalInputAxisModeProp,
                        isSensitivityForVerticalEnabledProp, sensitivityEnabledContent,
                        verticalAxisSensitivityProp, sensitivityContent,
                        verticalAxisGravityProp, gravityContent);
                        DrawDiscardData(isVerticalDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Longitudinal Input
                    DrawFoldoutWithLabel(longitudinalShowInEditorProp, longitudinalHeaderContent);
                    if (longitudinalShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(PIMCategory.Longitidinal, longitudinalInputAxisModeProp, positiveLongitudinalInputActionIdProp, negativeLongitudinalInputActionIdProp,
                                         rwLongitudinalInputACPositiveContent, rwLongitudinalInputACNegativeContent, rwLongitudinalInputACContent,
                                         rwLongitudinalInputAIPositiveContent, rwLongitudinalInputAINegativeContent, rwLongitudinalInputAIContent,
                                         ref actionIDsLngtdlPositive, ref actionIDsLngtdlNegative, ref ActionGUIContentLngtdlPositive, ref ActionGUIContentLngtdlNegative, numActionCategories);

                        DrawSensitivity(longitudinalInputAxisModeProp,
                        isSensitivityForLongitudinalEnabledProp, sensitivityEnabledContent,
                        longitudinalAxisSensitivityProp, sensitivityContent,
                        longitudinalAxisGravityProp, gravityContent);
                        DrawDiscardData(isLongitudinalDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Pitch input
                    DrawFoldoutWithLabel(pitchShowInEditorProp, pitchHeaderContent);
                    if (pitchShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(PIMCategory.Pitch, pitchInputAxisModeProp, positivePitchInputActionIdProp, negativePitchInputActionIdProp,
                                         rwPitchInputACPositiveContent, rwPitchInputACNegativeContent, rwPitchInputACContent,
                                         rwPitchInputAIPositiveContent, rwPitchInputAINegativeContent, rwPitchInputAIContent,
                                         ref actionIDsPitchPositive, ref actionIDsPitchNegative, ref ActionGUIContentPitchPositive, ref ActionGUIContentPitchNegative, numActionCategories);

                        DrawSensitivity(pitchInputAxisModeProp,
                        isSensitivityForPitchEnabledProp, sensitivityEnabledContent,
                        pitchAxisSensitivityProp, sensitivityContent,
                        pitchAxisGravityProp, gravityContent);
                        DrawDiscardData(isPitchDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Yaw input
                    DrawFoldoutWithLabel(yawShowInEditorProp, yawHeaderContent);
                    if (yawShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(PIMCategory.Yaw, yawInputAxisModeProp, positiveYawInputActionIdProp, negativeYawInputActionIdProp,
                                         rwYawInputACPositiveContent, rwYawInputACNegativeContent, rwYawInputACContent,
                                         rwYawInputAIPositiveContent, rwYawInputAINegativeContent, rwYawInputAIContent,
                                         ref actionIDsYawPositive, ref actionIDsYawNegative, ref ActionGUIContentYawPositive, ref ActionGUIContentYawNegative, numActionCategories);

                        DrawSensitivity(yawInputAxisModeProp,
                        isSensitivityForYawEnabledProp, sensitivityEnabledContent,
                        yawAxisSensitivityProp, sensitivityContent,
                        yawAxisGravityProp, gravityContent);
                        DrawDiscardData(isYawDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Roll input
                    DrawFoldoutWithLabel(rollShowInEditorProp, rollHeaderContent);
                    if (rollShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(PIMCategory.Roll, rollInputAxisModeProp, positiveRollInputActionIdProp, negativeRollInputActionIdProp,
                                         rwRollInputACPositiveContent, rwRollInputACNegativeContent, rwRollInputACContent,
                                         rwRollInputAIPositiveContent, rwRollInputAINegativeContent, rwRollInputAIContent,
                                         ref actionIDsRollPositive, ref actionIDsRollNegative, ref ActionGUIContentRollPositive, ref ActionGUIContentRollNegative, numActionCategories);

                        DrawSensitivity(rollInputAxisModeProp,
                        isSensitivityForRollEnabledProp, sensitivityEnabledContent,
                        rollAxisSensitivityProp, sensitivityContent,
                        rollAxisGravityProp, gravityContent);
                        DrawDiscardData(isRollDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Rewired Primary Fire Input
                    DrawFoldoutWithLabel(primaryFireShowInEditorProp, primaryFireHeaderContent);
                    if (primaryFireShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(primaryFireButtonEnabledProp, primaryFireInputActionIdProp, primaryFireCanBeHeldProp, rwPrimaryFireEnabledContent, rwPrimaryFireInputACContent, rwPrimaryFireInputAIContent, fireCanBeHeldContent, ref actionIDsPrimaryFire, ref ActionGUIContentPrimaryFire, numActionCategories);
                        DrawDiscardData(isPrimaryFireDataDiscardedProp, discardDataContent);
                    }
                    #endregion Rewired Primary Fire Input

                    #region Rewired Secondary Fire Input
                    DrawFoldoutWithLabel(secondaryFireShowInEditorProp, secondaryFireHeaderContent);
                    if (secondaryFireShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(secondaryFireButtonEnabledProp, secondaryFireInputActionIdProp, secondaryFireCanBeHeldProp, rwSecondaryFireEnabledContent, rwSecondaryFireInputACContent, rwSecondaryFireInputAIContent, fireCanBeHeldContent, ref actionIDsSecondaryFire, ref ActionGUIContentSecondaryFire, numActionCategories);
                        DrawDiscardData(isSecondaryFireDataDiscardedProp, discardDataContent);
                    }
                    #endregion Rewired Secondary Fire Input

                    #region Rewired Docking Input
                    DrawFoldoutWithLabel(dockingShowInEditorProp, dockingHeaderContent);
                    if (dockingShowInEditorProp.boolValue)
                    {
                        DrawRewiredInput(dockingButtonEnabledProp, dockingInputActionIdProp, null, rwDockingEnabledContent, rwDockingInputACContent, rwDockingInputAIContent, null, ref actionIDsDocking, ref ActionGUIContentDocking, numActionCategories);
                        DrawDiscardData(isDockDataDiscardedProp, discardDataContent);
                    }
                    #endregion Rewired Docking Input

                    #region Rumble Controller output
                    DrawFoldoutWithLabel(rumbleShowInEditorProp, rumbleHeaderContent);
                    if (rumbleShowInEditorProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(maxRumble1Prop, maxRumble1Content);
                        EditorGUILayout.PropertyField(maxRumble2Prop, maxRumble2Content);
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

            #region VIVE
            else if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.Vive && !isRefreshing)
            {
                // Vive has a hierarchy of Role, Controller, input type (ControllerButton, ControllerAxis)

                #if VIU_PLUGIN

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Vive Input Utility version ", GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(viveVersion);
                EditorGUILayout.EndHorizontal();

                #region VIVE Horizontal Input
                DrawFoldoutWithLabel(horizontalShowInEditorProp, horizontalHeaderContent);
                if (horizontalShowInEditorProp.boolValue)
                {
                    DrawViveInput(PIMCategory.Horizontal, horizontalInputAxisModeProp, viveHorizontalInputTypeProp,
                    viveHorizontalRoleTypeProp,
                    positiveHorizontalInputRoleIdProp, negativeHorizontalInputRoleIdProp,
                    positiveHorizontalInputCtrlViveProp, negativeHorizontalInputCtrlViveProp,
                    viveHorizontalInputTypeContent,
                    viveHorizontalRoleTypeContent,
                    viveHorizontalPositiveRoleIdContent,
                    viveHorizontalNegativeRoleIdContent,
                    viveHorizontalRoleIdContent,
                    viveHorizontalPositiveInputCtrlContent,
                    viveHorizontalNegativeInputCtrlContent,
                    viveHorizontalInputCtrlContent
                    );

                    DrawSensitivity(horizontalInputAxisModeProp,
                    isSensitivityForHorizontalEnabledProp, sensitivityEnabledContent,
                    horizontalAxisSensitivityProp, sensitivityContent,
                    horizontalAxisGravityProp, gravityContent);
                    DrawDiscardData(isHorizontalDataDiscardedProp, discardDataContent);
                }
                #endregion VIVE Horizontal Input

                #region VIVE Vertical Input
                DrawFoldoutWithLabel(verticalShowInEditorProp, verticalHeaderContent);
                if (verticalShowInEditorProp.boolValue)
                {
                    DrawViveInput(PIMCategory.Vertical, verticalInputAxisModeProp, viveVerticalInputTypeProp,
                    viveVerticalRoleTypeProp,
                    positiveVerticalInputRoleIdProp, negativeVerticalInputRoleIdProp,
                    positiveVerticalInputCtrlViveProp, negativeVerticalInputCtrlViveProp,
                    viveVerticalInputTypeContent,
                    viveVerticalRoleTypeContent,
                    viveVerticalPositiveRoleIdContent,
                    viveVerticalNegativeRoleIdContent,
                    viveVerticalRoleIdContent,
                    viveVerticalPositiveInputCtrlContent,
                    viveVerticalNegativeInputCtrlContent,
                    viveVerticalInputCtrlContent
                    );

                    DrawSensitivity(verticalInputAxisModeProp,
                    isSensitivityForVerticalEnabledProp, sensitivityEnabledContent,
                    verticalAxisSensitivityProp, sensitivityContent,
                    verticalAxisGravityProp, gravityContent);
                    DrawDiscardData(isVerticalDataDiscardedProp, discardDataContent);
                }
                #endregion VIVE Vertical Input

                #region VIVE Longitudinal Input
                DrawFoldoutWithLabel(longitudinalShowInEditorProp, longitudinalHeaderContent);
                if (longitudinalShowInEditorProp.boolValue)
                {
                    DrawViveInput(PIMCategory.Longitidinal, longitudinalInputAxisModeProp, viveLongitudinalInputTypeProp,
                    viveLongitudinalRoleTypeProp,
                    positiveLongitudinalInputRoleIdProp, negativeLongitudinalInputRoleIdProp,
                    positiveLongitudinalInputCtrlViveProp, negativeLongitudinalInputCtrlViveProp,
                    viveLongitudinalInputTypeContent,
                    viveLongitudinalRoleTypeContent,
                    viveLongitudinalPositiveRoleIdContent,
                    viveLongitudinalNegativeRoleIdContent,
                    viveLongitudinalRoleIdContent,
                    viveLongitudinalPositiveInputCtrlContent,
                    viveLongitudinalNegativeInputCtrlContent,
                    viveLongitudinalInputCtrlContent
                    );

                    DrawSensitivity(longitudinalInputAxisModeProp,
                    isSensitivityForLongitudinalEnabledProp, sensitivityEnabledContent,
                    longitudinalAxisSensitivityProp, sensitivityContent,
                    longitudinalAxisGravityProp, gravityContent);
                    DrawDiscardData(isLongitudinalDataDiscardedProp, discardDataContent);
                }
                #endregion VIVE Longitudinal Input

                #region VIVE Pitch Input
                DrawFoldoutWithLabel(pitchShowInEditorProp, pitchHeaderContent);
                if (pitchShowInEditorProp.boolValue)
                {
                    DrawViveInput(PIMCategory.Pitch, pitchInputAxisModeProp, vivePitchInputTypeProp,
                    vivePitchRoleTypeProp,
                    positivePitchInputRoleIdProp, negativePitchInputRoleIdProp,
                    positivePitchInputCtrlViveProp, negativePitchInputCtrlViveProp,
                    vivePitchInputTypeContent,
                    vivePitchRoleTypeContent,
                    vivePitchPositiveRoleIdContent,
                    vivePitchNegativeRoleIdContent,
                    vivePitchRoleIdContent,
                    vivePitchPositiveInputCtrlContent,
                    vivePitchNegativeInputCtrlContent,
                    vivePitchInputCtrlContent
                    );

                    DrawSensitivity(pitchInputAxisModeProp,
                    isSensitivityForPitchEnabledProp, sensitivityEnabledContent,
                    pitchAxisSensitivityProp, sensitivityContent,
                    pitchAxisGravityProp, gravityContent);
                    DrawDiscardData(isPitchDataDiscardedProp, discardDataContent);
                }
                #endregion VIVE Pitch Input

                #region VIVE Yaw Input
                DrawFoldoutWithLabel(yawShowInEditorProp, yawHeaderContent);
                if (yawShowInEditorProp.boolValue)
                {
                    DrawViveInput(PIMCategory.Yaw, yawInputAxisModeProp, viveYawInputTypeProp,
                    viveYawRoleTypeProp,
                    positiveYawInputRoleIdProp, negativeYawInputRoleIdProp,
                    positiveYawInputCtrlViveProp, negativeYawInputCtrlViveProp,
                    viveYawInputTypeContent,
                    viveYawRoleTypeContent,
                    viveYawPositiveRoleIdContent,
                    viveYawNegativeRoleIdContent,
                    viveYawRoleIdContent,
                    viveYawPositiveInputCtrlContent,
                    viveYawNegativeInputCtrlContent,
                    viveYawInputCtrlContent
                    );

                    DrawSensitivity(yawInputAxisModeProp,
                    isSensitivityForYawEnabledProp, sensitivityEnabledContent,
                    yawAxisSensitivityProp, sensitivityContent,
                    yawAxisGravityProp, gravityContent);
                    DrawDiscardData(isYawDataDiscardedProp, discardDataContent);
                }
                #endregion VIVE Yaw Input

                #region VIVE Roll Input
                DrawFoldoutWithLabel(rollShowInEditorProp, rollHeaderContent);
                if (rollShowInEditorProp.boolValue)
                {
                    DrawViveInput(PIMCategory.Roll, rollInputAxisModeProp, viveRollInputTypeProp,
                    viveRollRoleTypeProp,
                    positiveRollInputRoleIdProp, negativeRollInputRoleIdProp,
                    positiveRollInputCtrlViveProp, negativeRollInputCtrlViveProp,
                    viveRollInputTypeContent,
                    viveRollRoleTypeContent,
                    viveRollPositiveRoleIdContent,
                    viveRollNegativeRoleIdContent,
                    viveRollRoleIdContent,
                    viveRollPositiveInputCtrlContent,
                    viveRollNegativeInputCtrlContent,
                    viveRollInputCtrlContent
                    );

                    DrawSensitivity(rollInputAxisModeProp,
                    isSensitivityForRollEnabledProp, sensitivityEnabledContent,
                    rollAxisSensitivityProp, sensitivityContent,
                    rollAxisGravityProp, gravityContent);
                    DrawDiscardData(isRollDataDiscardedProp, discardDataContent);
                }
                #endregion VIVE Roll Input

                #region VIVE Primary Fire Input
                DrawFoldoutWithLabel(primaryFireShowInEditorProp, primaryFireHeaderContent);
                if (primaryFireShowInEditorProp.boolValue)
                {
                    DrawViveInput(
                    primaryFireButtonEnabledProp,
                    vivePrimaryFireRoleTypeProp,
                    primaryFireInputRoleIdProp,
                    primaryFireInputCtrlViveProp,
                    primaryFireCanBeHeldProp,
                    vivePrimaryFireEnabledContent,
                    vivePrimaryFireRoleTypeContent,
                    vivePrimaryFireRoleIdContent,
                    vivePrimaryFireInputCtrlContent,
                    fireCanBeHeldContent
                    );
                    DrawDiscardData(isPrimaryFireDataDiscardedProp, discardDataContent);
                }
                #endregion VIVE Primary Fire Input

                #region VIVE Secondary Fire Input
                DrawFoldoutWithLabel(secondaryFireShowInEditorProp, secondaryFireHeaderContent);
                if (secondaryFireShowInEditorProp.boolValue)
                {
                    DrawViveInput(
                    secondaryFireButtonEnabledProp,
                    viveSecondaryFireRoleTypeProp,
                    secondaryFireInputRoleIdProp,
                    secondaryFireInputCtrlViveProp,
                    secondaryFireCanBeHeldProp,
                    viveSecondaryFireEnabledContent,
                    viveSecondaryFireRoleTypeContent,
                    viveSecondaryFireRoleIdContent,
                    viveSecondaryFireInputCtrlContent,
                    fireCanBeHeldContent
                    );
                    DrawDiscardData(isSecondaryFireDataDiscardedProp, discardDataContent);
                }
                #endregion VIVE Secondary Fire Input

                #region VIVE Docking Input
                DrawFoldoutWithLabel(dockingShowInEditorProp, dockingHeaderContent);
                if (dockingShowInEditorProp.boolValue)
                {
                    DrawViveInput(
                    dockingButtonEnabledProp,
                    viveDockingRoleTypeProp,
                    dockingInputRoleIdProp,
                    dockingInputCtrlViveProp,
                    null,
                    viveDockingEnabledContent,
                    viveDockingRoleTypeContent,
                    viveDockingRoleIdContent,
                    viveDockingInputCtrlContent,
                    null
                    );
                    DrawDiscardData(isDockDataDiscardedProp, discardDataContent);
                }
                #endregion VIVE Secondary Fire Input

                #else
                EditorGUILayout.HelpBox("Could not find the VIVE plugin. Did you install the Vive Input Utility package 1.10.1 or newer?", MessageType.Warning, true);
                #endif
            }
            #endregion
            
            #region UnityXR
            else if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.UnityXR && !isRefreshing)
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

                SSCEditorHelper.InTechPreview(false);

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

                    #region XR Camera
                    // XR First Person has a Camera and a tranform that can be rotated (they could be on the same gameobject)
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(firstPersonTransform1XRProp, xrFirstPersonTransform1Content);
                    if (EditorGUI.EndChangeCheck() && firstPersonTransform1XRProp.objectReferenceValue != null)
                    {
                        if (!((Transform)firstPersonTransform1XRProp.objectReferenceValue).IsChildOf(playerInputModule.transform))
                        {
                            firstPersonTransform1XRProp.objectReferenceValue = null;
                            Debug.LogWarning("For XR First Person, the transform must be a child of the parent Ship Control Module gameobject or part of the prefab.");
                        }
                        // If the camera hasn't been setup, assume it is a child of the look transform
                        else if (firstPersonCamera1XRProp.objectReferenceValue == null)
                        {
                            // Find the first camera
                            firstPersonCamera1XRProp.objectReferenceValue = ((Transform)firstPersonTransform1XRProp.objectReferenceValue).GetComponentInChildren<Camera>();
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(xrFirstPersonCamera1Content, GUILayout.Width(defaultEditorLabelWidth - 53f));

                    if (GUILayout.Button(xrFirstPersonNewCamera1Content, buttonCompact, GUILayout.MaxWidth(50f)) && firstPersonCamera1XRProp.objectReferenceValue == null)
                    {
                        serializedObject.ApplyModifiedProperties();

                        Undo.SetCurrentGroupName("New XR First Person Camera");
                        int undoGroup = UnityEditor.Undo.GetCurrentGroup();

                        Undo.RecordObject(playerInputModule, string.Empty);

                        //GameObject camOffsetGameObject = new GameObject("XR Camera Offset");
                        //GameObject camGameObject = new GameObject("XR Camera");

                        GameObject camOffsetGameObject = SSCEditorHelper.GetOrCreateChildGameObject(playerInputModule.gameObject, "XR Camera Offset", true);
                        GameObject camGameObject = SSCEditorHelper.GetOrCreateChildGameObject(camOffsetGameObject, "XR Camera", false);

                        if (camOffsetGameObject != null && camGameObject != null)
                        {
                            Undo.RegisterCreatedObjectUndo(camOffsetGameObject, string.Empty);
                            Undo.RegisterCreatedObjectUndo(camGameObject, string.Empty);

                            Camera _newCamera;

                            if (!camGameObject.TryGetComponent(out _newCamera))
                            {
                                _newCamera = Undo.AddComponent(camGameObject, typeof(Camera)) as Camera;
                            }
            
                            _newCamera.tag = "MainCamera";

                            // When seated in cockpit with no visible body, we don't want to clip through the sides of the cockpit
                            // or through say the canopy.
                            _newCamera.nearClipPlane = 0.05f;
                            //playerInputModule.lookFirstPersonCamera1 = _newCamera;

                            // Distinguish between the old XR TrackPoseDriver and the one with the new Unity Input System.
                            var _tposeDriver = Undo.AddComponent(camGameObject, typeof(UnityEngine.InputSystem.XR.TrackedPoseDriver)) as UnityEngine.InputSystem.XR.TrackedPoseDriver;

                            if (_tposeDriver != null)
                            {
                                // Permit both rotation AND position for the head-mounted device (HMD). This will give a more natural
                                // feel for the user and may help reduce motion sickness.
                                _tposeDriver.trackingType = UnityEngine.InputSystem.XR.TrackedPoseDriver.TrackingType.RotationAndPosition;

                                UnityEngine.InputSystem.InputAction _tposPosition = new UnityEngine.InputSystem.InputAction(null, UnityEngine.InputSystem.InputActionType.Value);
                                if (_tposPosition != null)
                                {
                                    _tposeDriver.positionAction = _tposPosition;
                                }

                                UnityEngine.InputSystem.InputAction _tposRotation = new UnityEngine.InputSystem.InputAction(null, UnityEngine.InputSystem.InputActionType.Value);
                                if (_tposRotation != null)
                                {
                                    _tposRotation.expectedControlType = "Quaternion";
                                    _tposeDriver.rotationAction = _tposRotation;
                                }
                            }

                            //camOffsetGameObject.transform.SetParent(playerInputModule.transform, false);
                            camGameObject.transform.SetParent(camOffsetGameObject.transform, false);
                            playerInputModule.SetXRFirstPersonCamera1(_newCamera, camGameObject.transform, true);

                            // Look for an existing AudioListener
                            AudioListener _audioListener = playerInputModule.GetComponentInChildren<AudioListener>();
                            if (_audioListener == null)
                            {
                                // Didn't find one, so add an AudioListener now
                                _audioListener = Undo.AddComponent(camGameObject, typeof(AudioListener)) as AudioListener;
                            }
                        }

                        Undo.CollapseUndoOperations(undoGroup);

                        // Should be non-scene objects but is required to force being set as dirty
                        EditorUtility.SetDirty(playerInputModule);

                        GUIUtility.ExitGUI();

                        //serializedObject.Update();
                    }
                    EditorGUILayout.PropertyField(firstPersonCamera1XRProp, GUIContent.none);

                    GUILayout.EndHorizontal();

                    #endregion

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

                    #endregion

                    #region Horizontal Input
                    DrawFoldoutWithLabel(horizontalShowInEditorProp, horizontalHeaderContent);
                    if (horizontalShowInEditorProp.boolValue)
                    {
                        DrawXR(horizontalInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, positiveHorizontalInputActionMapIdXRProp,
                        positiveHorizontalInputActionIdUISProp, positiveHorizontalInputActionDataSlotUISProp,
                        xrInputAMContent, uisHorizontalInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, true, xrForceActionRefresh
                        );
                        DrawSensitivity(horizontalInputAxisModeProp,
                        isSensitivityForHorizontalEnabledProp, sensitivityEnabledContent,
                        horizontalAxisSensitivityProp, sensitivityContent,
                        horizontalAxisGravityProp, gravityContent);
                        DrawDiscardData(isHorizontalDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Vertical Input
                    DrawFoldoutWithLabel(verticalShowInEditorProp, verticalHeaderContent);
                    if (verticalShowInEditorProp.boolValue)
                    {
                        DrawXR(verticalInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, positiveVerticalInputActionMapIdXRProp,
                        positiveVerticalInputActionIdUISProp, positiveVerticalInputActionDataSlotUISProp,
                        xrInputAMContent, uisVerticalInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, true, xrForceActionRefresh
                        );
                        DrawSensitivity(verticalInputAxisModeProp,
                        isSensitivityForVerticalEnabledProp, sensitivityEnabledContent,
                        verticalAxisSensitivityProp, sensitivityContent,
                        verticalAxisGravityProp, gravityContent);
                        DrawDiscardData(isVerticalDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Longitudinal Input
                    DrawFoldoutWithLabel(longitudinalShowInEditorProp, longitudinalHeaderContent);
                    if (longitudinalShowInEditorProp.boolValue)
                    {
                        DrawXR(longitudinalInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, positiveLongitudinalInputActionMapIdXRProp,
                        positiveLongitudinalInputActionIdUISProp, positiveLongitudinalInputActionDataSlotUISProp,
                        xrInputAMContent, uisLongitudinalInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, true, xrForceActionRefresh
                        );
                        DrawSensitivity(longitudinalInputAxisModeProp,
                        isSensitivityForLongitudinalEnabledProp, sensitivityEnabledContent,
                        longitudinalAxisSensitivityProp, sensitivityContent,
                        longitudinalAxisGravityProp, gravityContent);
                        DrawDiscardData(isLongitudinalDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Pitch Input
                    DrawFoldoutWithLabel(pitchShowInEditorProp, pitchHeaderContent);
                    if (pitchShowInEditorProp.boolValue)
                    {
                        DrawXR(pitchInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, positivePitchInputActionMapIdXRProp,
                        positivePitchInputActionIdUISProp, positivePitchInputActionDataSlotUISProp,
                        xrInputAMContent, uisPitchInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, true, xrForceActionRefresh
                        );
                        DrawSensitivity(pitchInputAxisModeProp,
                        isSensitivityForPitchEnabledProp, sensitivityEnabledContent,
                        pitchAxisSensitivityProp, sensitivityContent,
                        pitchAxisGravityProp, gravityContent);
                        DrawDiscardData(isPitchDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Yaw Input
                    DrawFoldoutWithLabel(yawShowInEditorProp, yawHeaderContent);
                    if (yawShowInEditorProp.boolValue)
                    {
                        DrawXR(yawInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, positiveYawInputActionMapIdXRProp,
                        positiveYawInputActionIdUISProp, positiveYawInputActionDataSlotUISProp,
                        xrInputAMContent, uisYawInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, true, xrForceActionRefresh
                        );
                        DrawSensitivity(yawInputAxisModeProp,
                        isSensitivityForYawEnabledProp, sensitivityEnabledContent,
                        yawAxisSensitivityProp, sensitivityContent,
                        yawAxisGravityProp, gravityContent);
                        DrawDiscardData(isYawDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Roll Input
                    DrawFoldoutWithLabel(rollShowInEditorProp, rollHeaderContent);
                    if (rollShowInEditorProp.boolValue)
                    {
                        DrawXR(rollInputAxisModeProp, inputSystemAxisModeContent,
                        inputActionAssetXRProp, positiveRollInputActionMapIdXRProp,
                        positiveRollInputActionIdUISProp, positiveRollInputActionDataSlotUISProp,
                        xrInputAMContent, uisRollInputAContent, uisInputATypeContent, uisInputCTypeContent,
                        uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, true, xrForceActionRefresh
                        );
                        DrawSensitivity(rollInputAxisModeProp,
                        isSensitivityForRollEnabledProp, sensitivityEnabledContent,
                        rollAxisSensitivityProp, sensitivityContent,
                        rollAxisGravityProp, gravityContent);
                        DrawDiscardData(isRollDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Primary Fire Input
                    DrawFoldoutWithLabel(primaryFireShowInEditorProp, primaryFireHeaderContent);
                    if (primaryFireShowInEditorProp.boolValue)
                    {
                        DrawXRButton(primaryFireButtonEnabledProp, uisPrimaryFireEnabledContent,
                        inputActionAssetXRProp, primaryFireInputActionMapIdXRProp,
                        primaryFireInputActionIdUISProp, primaryFireInputActionDataSlotUISProp,
                        primaryFireCanBeHeldProp, xrInputAMContent, uisPrimaryFireInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        fireCanBeHeldContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, xrForceActionRefresh
                        );
                        DrawDiscardData(isPrimaryFireDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Secondary Fire Input
                    DrawFoldoutWithLabel(secondaryFireShowInEditorProp, secondaryFireHeaderContent);
                    if (secondaryFireShowInEditorProp.boolValue)
                    {
                        DrawXRButton(secondaryFireButtonEnabledProp, uisSecondaryFireEnabledContent,
                        inputActionAssetXRProp, secondaryFireInputActionMapIdXRProp,
                        secondaryFireInputActionIdUISProp, secondaryFireInputActionDataSlotUISProp,
                        secondaryFireCanBeHeldProp, xrInputAMContent, uisSecondaryFireInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        fireCanBeHeldContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, xrForceActionRefresh
                        );
                        DrawDiscardData(isSecondaryFireDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Docking Input
                    DrawFoldoutWithLabel(dockingShowInEditorProp, dockingHeaderContent);
                    if (dockingShowInEditorProp.boolValue)
                    {
                        DrawXRButton(dockingButtonEnabledProp, uisDockingEnabledContent,
                        inputActionAssetXRProp, dockingInputActionMapIdXRProp,
                        dockingInputActionIdUISProp, dockingInputActionDataSlotUISProp,
                        null, xrInputAMContent, uisDockingInputAContent,
                        uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                        null, uisDataSlots2, uisDataSlots3, numActionMapIDs, xrForceActionRefresh
                        );
                        DrawDiscardData(isDockDataDiscardedProp, discardDataContent);
                    }
                    #endregion

                    #region Rumble Controller output
                    DrawFoldoutWithLabel(rumbleShowInEditorProp, rumbleHeaderContent);
                    if (rumbleShowInEditorProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(maxRumble1Prop, maxRumble1Content);
                        EditorGUILayout.PropertyField(maxRumble2Prop, maxRumble2Content);
                    }
                    #endregion

                    xrForceActionRefresh = false;
                }
                
                #elif !UNITY_2020_3_OR_NEWER
                EditorGUILayout.HelpBox("In SSC, Unity XR is only supported on Unity 2020.3 or newer. You could use Oculus API or VIVE API instead.", MessageType.Warning, true);
                #else
                EditorGUILayout.HelpBox("Unity XR package or Unity Input System is not installed. Did you install the XR Management package?", MessageType.Warning, true);
                #endif               
            }
            #endregion

            #region Custom Player Input
            DrawFoldoutWithLabel(cpiShowInEditorProp, cpiHeaderContent);
            if (cpiShowInEditorProp.boolValue)
            {
                cpiDeletePos = -1;

                #region Check if list is null and get size
                
                // Checking the property for being NULL doesn't check if the list is actually null.
                if (playerInputModule.customPlayerInputList == null)
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    playerInputModule.customPlayerInputList = new List<CustomPlayerInput>(4);
                    // Read in the properties
                    serializedObject.Update();
                }

                int numCustomPlayerInputs = cpiListProp.arraySize;
                #endregion

                #region Add or Remove items

                GUILayout.BeginHorizontal();

                EditorGUI.indentLevel += 1;
                EditorGUIUtility.fieldWidth = 15f;
                EditorGUI.BeginChangeCheck();
                cpiIsListExpandedProp.boolValue = EditorGUILayout.Foldout(cpiIsListExpandedProp.boolValue, GUIContent.none, foldoutStyleNoLabel);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(playerInputModule.customPlayerInputList, cpiIsListExpandedProp.boolValue);
                    // Read in the properties
                    serializedObject.Update();
                }
                EditorGUI.indentLevel -= 1;

                EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                EditorGUILayout.LabelField("Custom Player Inputs: " + numCustomPlayerInputs.ToString("00"), labelFieldRichText);

                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(playerInputModule, "Add Custom Player Input");
                    playerInputModule.customPlayerInputList.Add(new CustomPlayerInput());
                    ExpandList(playerInputModule.customPlayerInputList, false);
                    // Read in the properties
                    serializedObject.Update();

                    numCustomPlayerInputs = cpiListProp.arraySize;
                    if (numCustomPlayerInputs > 0)
                    {
                        // Force new custom input to be serialized in scene
                        cpiItemProp = cpiListProp.GetArrayElementAtIndex(numCustomPlayerInputs - 1);
                        cpiItemShowInEditorProp = cpiItemProp.FindPropertyRelative("showInEditor");
                        cpiItemShowInEditorProp.boolValue = !cpiItemShowInEditorProp.boolValue;
                        // Show the new custom player input
                        cpiItemShowInEditorProp.boolValue = true;
                    }
                }
                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (numCustomPlayerInputs > 0) { cpiDeletePos = cpiListProp.arraySize - 1; }
                }

                GUILayout.EndHorizontal();

                #endregion

                #region Custom Player Input List items

                #if SSC_REWIRED
                int numActionCategories = actionCategoryIDs == null ? 0 : actionCategoryIDs.Length;
                #endif

                #if SSC_UIS
                bool isUISAvailable = inputModeProp.intValue == (int)PlayerInputModule.InputMode.UnityInputSystem &&
                                      uisPlayerInput != null && uisPlayerInput.actions != null &&
                                      uisPlayerInput.notificationBehavior == UnityEngine.InputSystem.PlayerNotifications.InvokeUnityEvents &&
                                      !string.IsNullOrEmpty(uisPlayerInput.defaultActionMap);
                int numActionIDs = uisActionIDs == null ? 0 : uisActionIDs.Length;
                #endif

                #if SCSM_XR && SSC_UIS                
                bool isXRAvailable = inputModeProp.intValue == (int)PlayerInputModule.InputMode.UnityXR &&
                                     inputActionAssetXRProp != null && inputActionAssetXRProp.objectReferenceValue != null;
                int numActionMapIDs = xrActionMapIDs == null ? 0 : xrActionMapIDs.Length;
                #endif

                for (int cpiIdx = 0; cpiIdx < numCustomPlayerInputs; cpiIdx++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    cpiItemProp = cpiListProp.GetArrayElementAtIndex(cpiIdx);

                    cpiItemShowInEditorProp = cpiItemProp.FindPropertyRelative("showInEditor");
                    cpiIsButtonProp = cpiItemProp.FindPropertyRelative("isButton");
                    cpiCanBeHeldDownProp = cpiItemProp.FindPropertyRelative("canBeHeldDown");
                    cpiInputAxisModeProp = cpiItemProp.FindPropertyRelative("inputAxisMode");
                    cpiIsSensitivityEnabledProp = cpiItemProp.FindPropertyRelative("isSensitivityEnabled");
                    cpiSensitivityProp = cpiItemProp.FindPropertyRelative("sensitivity");
                    cpiGravityProp = cpiItemProp.FindPropertyRelative("gravity");                 

                    #region Custom input name and Delete button
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel += 1;
                    cpiItemShowInEditorProp.boolValue = EditorGUILayout.Foldout(cpiItemShowInEditorProp.boolValue, "Custom Player Input " + (cpiIdx + 1).ToString("00"));
                    EditorGUI.indentLevel -= 1;
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { cpiDeletePos = cpiIdx; }
                    GUILayout.EndHorizontal();
                    #endregion

                    if (cpiItemShowInEditorProp.boolValue)
                    {
                        #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER || SSC_OVR
                        // If isButton is changes from false to true, we may need to reset some values below
                        // to indicate only one input axis is possible.
                        bool hasButtonChanged = false;

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(cpiIsButtonProp, cpiIsButtonContent);
                        if (EditorGUI.EndChangeCheck()) { hasButtonChanged = true; }
                        #else
                        EditorGUILayout.PropertyField(cpiIsButtonProp, cpiIsButtonContent);
                        #endif

                        if (inputModeProp.intValue != (int)PlayerInputModule.InputMode.Rewired &&
                            inputModeProp.intValue != (int)PlayerInputModule.InputMode.Vive && 
                            inputModeProp.intValue != (int)PlayerInputModule.InputMode.UnityInputSystem &&
                            inputModeProp.intValue != (int)PlayerInputModule.InputMode.UnityXR)
                        {
                            if (cpiIsButtonProp.boolValue)
                            {
                                EditorGUILayout.PropertyField(cpiCanBeHeldDownProp, cpiCanBeHeldDownContent);
                            }
                        }

                        #region Direct Keyboard Input
                        if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.DirectKeyboard)
                        {
                            #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER                            
                            cpidkmPositiveKeycodeProp = cpiItemProp.FindPropertyRelative("dkmPositiveKeycode");
                            cpidkmNegativeKeycodeProp = cpiItemProp.FindPropertyRelative("dkmNegativeKeycode");

                            EditorGUILayout.PropertyField(cpidkmPositiveKeycodeProp, positiveKeycodeContent);
                            if (!cpiIsButtonProp.boolValue)
                            {
                                EditorGUILayout.PropertyField(cpidkmNegativeKeycodeProp, negativeKeycodeContent);

                                if (cpidkmPositiveKeycodeProp.intValue == cpidkmNegativeKeycodeProp.intValue
                                    && cpidkmPositiveKeycodeProp.intValue != 0)
                                {
                                    EditorGUILayout.HelpBox(identicalKeycodesWarningContent, MessageType.Warning);
                                }
                            }
                            else if (hasButtonChanged) { cpidkmNegativeKeycodeProp.intValue = 0; }

                            #endif
                        }
                        #endregion

                        #region Legacy Unity Input System
                        else if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.LegacyUnity)
                        {
                            #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER

                            cpilisPositiveAxisNameProp = cpiItemProp.FindPropertyRelative("lisPositiveAxisName");
                            cpilisNegativeAxisNameProp = cpiItemProp.FindPropertyRelative("lisNegativeAxisName");
                            cpilisInvertAxisProp = cpiItemProp.FindPropertyRelative("lisInvertAxis");
                            cpilisIsPositiveAxisValidProp = cpiItemProp.FindPropertyRelative("lisIsPositiveAxisValid");
                            cpilisIsNegativeAxisValidProp = cpiItemProp.FindPropertyRelative("lisIsNegativeAxisValid");

                            if (cpiIsButtonProp.boolValue)
                            {
                                // If this is a button, there is only one input
                                if (hasButtonChanged) { cpiInputAxisModeProp.intValue = (int)PlayerInputModule.InputAxisMode.SingleAxis; }

                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(cpilisPositiveAxisNameProp, singleAxisNameContent);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    cpilisIsPositiveAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(cpilisPositiveAxisNameProp.stringValue, false);
                                }
                                if (!cpilisIsPositiveAxisValidProp.boolValue)
                                {
                                    EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                                }
                            }
                            else
                            {
                                // Single or Combined Axis
                                EditorGUILayout.PropertyField(cpiInputAxisModeProp, inputAxisModeContent);
                                if (cpiInputAxisModeProp.intValue != (int)PlayerInputModule.InputAxisMode.NoInput)
                                {
                                    if (cpiInputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.SingleAxis)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        EditorGUILayout.PropertyField(cpilisPositiveAxisNameProp, singleAxisNameContent);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            cpilisIsPositiveAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(cpilisPositiveAxisNameProp.stringValue, false);
                                        }
                                        if (!cpilisIsPositiveAxisValidProp.boolValue)
                                        {
                                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                                        }
                                    }
                                    else
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        EditorGUILayout.PropertyField(cpilisPositiveAxisNameProp, positiveAxisNameContent);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            cpilisIsPositiveAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(cpilisPositiveAxisNameProp.stringValue, false);
                                        }
                                        if (!cpilisIsPositiveAxisValidProp.boolValue)
                                        {
                                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                                        }

                                        EditorGUI.BeginChangeCheck();
                                        EditorGUILayout.PropertyField(cpilisNegativeAxisNameProp, negativeAxisNameContent);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            cpilisIsNegativeAxisValidProp.boolValue = PlayerInputModule.IsLegacyAxisValid(cpilisNegativeAxisNameProp.stringValue, false);
                                        }
                                        if (!cpilisIsNegativeAxisValidProp.boolValue)
                                        {
                                            EditorGUILayout.HelpBox(invalidLegacyAxisNameWarningContent, MessageType.Warning);
                                        }

                                        if (cpilisPositiveAxisNameProp.stringValue == cpilisNegativeAxisNameProp.stringValue)
                                        {
                                            EditorGUILayout.HelpBox(identicalAxisNamesWarningContent, MessageType.Warning);
                                        }
                                    }

                                    EditorGUILayout.PropertyField(cpilisInvertAxisProp, invertInputAxisContent);
                                    DrawSensitivity(cpiInputAxisModeProp, cpiIsSensitivityEnabledProp, sensitivityEnabledContent,
                                                    cpiSensitivityProp, sensitivityContent, cpiGravityProp, gravityContent);
                                }
                            }

                            #endif
                        }
                        #endregion

                        #region Unity Input System (add with Unity package manager)
                        else if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.UnityInputSystem)
                        {
                            #if SSC_UIS
                            if (isUISAvailable)
                            {
                                cpiIsButtonEnabledProp = cpiItemProp.FindPropertyRelative("isButtonEnabled");
                                cpiuisPositiveInputActionIdProp = cpiItemProp.FindPropertyRelative("uisPositiveInputActionId");
                                cpiuisPositiveInputActionDataSlotProp = cpiItemProp.FindPropertyRelative("uisPositiveInputActionDataSlot");

                                if (cpiIsButtonProp.boolValue)
                                {
                                    DrawUnityInputSystemFire(cpiIsButtonEnabledProp, uisCustomEnabledContent,
                                    cpiuisPositiveInputActionIdProp, cpiuisPositiveInputActionDataSlotProp,
                                    cpiCanBeHeldDownProp, uisCustomInputAContent,
                                    uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                                    cpiCanBeHeldDownContent, uisDataSlots2, uisDataSlots3, numActionIDs
                                    );
                                }
                                else
                                {
                                    DrawUnityInputSystem(cpiInputAxisModeProp, inputSystemAxisModeContent,
                                    cpiuisPositiveInputActionIdProp, cpiuisPositiveInputActionDataSlotProp,
                                    uisCustomInputAContent, uisInputATypeContent, uisInputCTypeContent,
                                    uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionIDs, false
                                    );
                                    DrawSensitivity(cpiInputAxisModeProp,
                                    cpiIsSensitivityEnabledProp, sensitivityEnabledContent,
                                    cpiSensitivityProp, sensitivityContent,
                                    cpiGravityProp, gravityContent);
                                }
                            }
                            #endif
                        }
                        #endregion

                        #region Oculus OVR API input
                        else if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.OculusAPI && !isRefreshing)
                        {
                            #if SSC_OVR

                            cpiovrInputTypeProp = cpiItemProp.FindPropertyRelative("ovrInputType");
                            cpiovrPositiveInputProp = cpiItemProp.FindPropertyRelative("ovrPositiveInput");
                            cpiovrNegativeInputProp = cpiItemProp.FindPropertyRelative("ovrNegativeInput");

                            if (cpiIsButtonProp.boolValue)
                            {
                                DrawOVRInput(cpiovrPositiveInputProp, null, ovrCustomButtonInputContent, null);
                                if (hasButtonChanged) { cpiovrNegativeInputProp.intValue = 0; }
                            }
                            else
                            {
                                DrawOVRInput(PIMCategory.CustomPlayerInput, cpiInputAxisModeProp, cpiovrInputTypeProp,
                                cpiovrPositiveInputProp, cpiovrNegativeInputProp,
                                ovrCustomInputTypeContent,
                                ovrCustomPositiveInputContent,
                                ovrCustomNegativeInputContent,
                                ovrCustomInputContent
                                );

                                DrawSensitivity(cpiInputAxisModeProp, cpiIsSensitivityEnabledProp, sensitivityEnabledContent,
                                cpiSensitivityProp, sensitivityContent, cpiGravityProp, gravityContent);
                            }
                            #endif
                        }
                        #endregion

                        #region Rewired Input
                        else if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.Rewired && !isRefreshing)
                        {
                            #if SSC_REWIRED
                            cpiIsButtonEnabledProp = cpiItemProp.FindPropertyRelative("isButtonEnabled");
                            cpirwdPositiveInputActionIdProp = cpiItemProp.FindPropertyRelative("rwdPositiveInputActionId");
                            cpirwdNegativeInputActionIdProp = cpiItemProp.FindPropertyRelative("rwdNegativeInputActionId");

                            CustomPlayerInput customPlayerInput = playerInputModule.customPlayerInputList[cpiIdx];

                            if (cpiIsButtonProp.boolValue)
                            {
                                DrawRewiredInput(cpiIsButtonEnabledProp, cpirwdPositiveInputActionIdProp, cpiCanBeHeldDownProp, rwCustomEnabledContent, rwCustomInputACContent, rwCustomInputAIContent,
                                                 cpiCanBeHeldDownContent, ref customPlayerInput.actionIDsPositive, ref customPlayerInput.ActionGUIContentPositive, numActionCategories);
                            }
                            else
                            {
                                DrawRewiredInput(PIMCategory.CustomPlayerInput, cpiInputAxisModeProp, cpirwdPositiveInputActionIdProp, cpirwdNegativeInputActionIdProp,
                                                 rwCustomInputACPositiveContent, rwCustomInputACNegativeContent, rwCustomInputACContent,
                                                 rwCustomInputAIPositiveContent, rwCustomInputAINegativeContent, rwCustomInputAIContent,
                                                 ref customPlayerInput.actionIDsPositive, ref customPlayerInput.actionIDsNegative,
                                                 ref customPlayerInput.ActionGUIContentPositive, ref customPlayerInput.ActionGUIContentNegative, numActionCategories);

                                DrawSensitivity(cpiInputAxisModeProp, cpiIsSensitivityEnabledProp, sensitivityEnabledContent,
                                cpiSensitivityProp, sensitivityContent, cpiGravityProp, gravityContent);
                            }

                            #endif
                        }
                        #endregion

                        #region VIVE
                        else if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.Vive && !isRefreshing)
                        {
                            #if VIU_PLUGIN
                            cpiIsButtonEnabledProp = cpiItemProp.FindPropertyRelative("isButtonEnabled");
                            cpiviveInputTypeProp = cpiItemProp.FindPropertyRelative("viveInputType");
                            cpiviveRoleTypeProp = cpiItemProp.FindPropertyRelative("viveRoleType");
                            cpivivePositiveInputRoleIdProp = cpiItemProp.FindPropertyRelative("vivePositiveInputRoleId");
                            cpiviveNegativeInputRoleIdProp = cpiItemProp.FindPropertyRelative("viveNegativeInputRoleId");
                            cpivivePositiveInputCtrlProp = cpiItemProp.FindPropertyRelative("vivePositiveInputCtrl");
                            cpiviveNegativeInputCtrlProp = cpiItemProp.FindPropertyRelative("viveNegativeInputCtrl");

                            if (cpiIsButtonProp.boolValue)
                            {
                                DrawViveInput(
                                cpiIsButtonEnabledProp,
                                cpiviveRoleTypeProp,
                                cpivivePositiveInputRoleIdProp,
                                cpivivePositiveInputCtrlProp,
                                cpiCanBeHeldDownProp,
                                viveCustomEnabledContent,
                                viveCustomRoleTypeContent,
                                viveCustomRoleIdContent,
                                viveCustomInputCtrlContent,
                                cpiCanBeHeldDownContent
                                );
                            }
                            else
                            {
                                DrawViveInput(PIMCategory.CustomPlayerInput, cpiInputAxisModeProp, cpiviveInputTypeProp,
                                cpiviveRoleTypeProp,
                                cpivivePositiveInputRoleIdProp, cpiviveNegativeInputRoleIdProp,
                                cpivivePositiveInputCtrlProp, cpiviveNegativeInputCtrlProp,
                                viveCustomInputTypeContent,
                                viveCustomRoleTypeContent,
                                viveCustomPositiveRoleIdContent,
                                viveCustomNegativeRoleIdContent,
                                viveCustomRoleIdContent,
                                viveCustomPositiveInputCtrlContent,
                                viveCustomNegativeInputCtrlContent,
                                viveCustomInputCtrlContent
                                );

                                DrawSensitivity(cpiInputAxisModeProp, cpiIsSensitivityEnabledProp, sensitivityEnabledContent,
                                cpiSensitivityProp, sensitivityContent, cpiGravityProp, gravityContent);
                            }
                            #endif
                        }
                        #endregion

                        #region UnityXR
                        else if (inputModeProp.intValue == (int)PlayerInputModule.InputMode.UnityXR && !isRefreshing)
                        {
                            #if SCSM_XR && SSC_UIS
                            if (isXRAvailable)
                            {
                                cpiIsButtonEnabledProp = cpiItemProp.FindPropertyRelative("isButtonEnabled");
                                cpixrPositiveInputActionMapIdProp = cpiItemProp.FindPropertyRelative("xrPositiveInputActionMapId");
                                cpiuisPositiveInputActionIdProp = cpiItemProp.FindPropertyRelative("uisPositiveInputActionId");
                                cpiuisPositiveInputActionDataSlotProp = cpiItemProp.FindPropertyRelative("uisPositiveInputActionDataSlot");

                                if (cpiIsButtonProp.boolValue)
                                {
                                    DrawXRButton(cpiIsButtonEnabledProp, uisCustomEnabledContent,
                                    inputActionAssetXRProp, cpixrPositiveInputActionMapIdProp,
                                    cpiuisPositiveInputActionIdProp, cpiuisPositiveInputActionDataSlotProp,
                                    cpiCanBeHeldDownProp, xrInputAMContent, uisCustomInputAContent,
                                    uisInputATypeContent, uisInputCTypeContent, uisInputDataSlotContent,
                                    cpiCanBeHeldDownContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, xrForceActionRefresh
                                    );
                                }
                                else
                                {
                                    DrawXR(cpiInputAxisModeProp, inputSystemAxisModeContent,
                                    inputActionAssetXRProp, cpixrPositiveInputActionMapIdProp,
                                    cpiuisPositiveInputActionIdProp, cpiuisPositiveInputActionDataSlotProp,
                                    xrInputAMContent, uisCustomInputAContent, uisInputATypeContent, uisInputCTypeContent,
                                    uisInputDataSlotContent, uisDataSlots2, uisDataSlots3, numActionMapIDs, false, xrForceActionRefresh
                                    );

                                    DrawSensitivity(cpiInputAxisModeProp,
                                    cpiIsSensitivityEnabledProp, sensitivityEnabledContent,
                                    cpiSensitivityProp, sensitivityContent,
                                    cpiGravityProp, gravityContent);
                                }
                            }
                            #endif                          
                        }
                        #endregion

                        cpiEventProp = cpiItemProp.FindPropertyRelative("customPlayerInputEvt");
                        EditorGUILayout.PropertyField(cpiEventProp, cpiEventContent);
                    }

                    GUILayout.EndVertical();
                }

                #endregion

                #region Delete Items
                if (cpiDeletePos >= 0)
                {
                    // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and cpiDeletePos is reset to -1.
                    int _deleteIndex = cpiDeletePos;

                    if (EditorUtility.DisplayDialog("Delete Custom Player Input " + (cpiDeletePos + 1) + "?", "Custom Player Input " + (cpiDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the custom player input from the list and cannot be undone.", "Delete Now", "Cancel"))
                    {
                        cpiListProp.DeleteArrayElementAtIndex(_deleteIndex);
                        cpiDeletePos = -1;
                        serializedObject.ApplyModifiedProperties();
                        GUIUtility.ExitGUI();
                    }
                }
                #endregion

            }
            #endregion

            EditorGUILayout.EndVertical();

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            // Debug at runtime in the editor
            if (isDebuggingEnabled && shipInput != null && playerInputModule != null)
            {
                SSCEditorHelper.PerformanceImpact();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(playerInputModule.IsInitialised ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInputEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(playerInputModule.IsInputEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsCustomPlayerInputOnlyEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(playerInputModule.IsCustomPlayerInputOnlyEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsShipAIModeEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(playerInputModule.IsShipAIModeEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugHorizontalContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isHorizontalDataDiscardedProp.boolValue ? "---" : shipInput.horizontal.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugVerticalContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isVerticalDataDiscardedProp.boolValue ? "---" : shipInput.vertical.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugLongitudinalContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isLongitudinalDataDiscardedProp.boolValue ? "---" : shipInput.longitudinal.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugPitchContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isPitchDataDiscardedProp.boolValue ? "---" : shipInput.pitch.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugYawContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isYawDataDiscardedProp.boolValue ? "---" : shipInput.yaw.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugRollContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isRollDataDiscardedProp.boolValue ? "---" : shipInput.roll.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugPrimaryFireContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isPrimaryFireDataDiscardedProp.boolValue ? "---" : shipInput.primaryFire.ToString(), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugSecondaryFireContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isSecondaryFireDataDiscardedProp.boolValue ? "---" : shipInput.secondaryFire.ToString(), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugDockingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(isDockDataDiscardedProp.boolValue ? "---" : shipInput.dock.ToString(), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugShipSpeedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                if (shipControlModule != null)
                {
                    // z-axis of Local space velocity. Convert to km/h
                    float shipVelocity = (shipControlModule.shipInstance.TransformInverseRotation * shipControlModule.ShipRigidbody.velocity).z * 3.6f;
                    EditorGUILayout.LabelField(shipVelocity.ToString("0.0"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                }
                EditorGUILayout.EndHorizontal();

                #if SSC_UIS
                if (GUILayout.Button(uisDebugApplyChangedContent, GUILayout.MaxWidth(120f)))
                {
                    serializedObject.ApplyModifiedProperties();
                    ((PlayerInputModule)serializedObject.targetObject).UpdateUnityInputSystemActions(true);
                }
                #endif

            }
            EditorGUILayout.EndVertical();
            #endregion Debug Mode

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            playerInputModule.allowRepaint = true;
        }


        #endregion

        #region Private methods - General

        /// <summary>
        /// Verify that the appropriate Thrusters are present in the ShipControlModule. If not, display a warning in the inspector.
        /// </summary>
        /// <param name="pimCategory"></param>
        private void VerifyThrusterPresent(PIMCategory pimCategory)
        {

            if (shipControlModule != null && shipControlModule.shipInstance != null)
            {
                // Force use: 0 - none. 1 - forwards thrust. 2 - backwards thrust. 3 - upwards thrust. 4 - downwards thrust. 5 - rightwards thrust. 6 - leftwards thrust.

                bool isThrusterMissing = false;
                string thrustInputText = string.Empty;

                switch (pimCategory)
                {
                    case PIMCategory.Horizontal:
                        isThrusterMissing = !shipControlModule.shipInstance.thrusterList.Exists(th => th.forceUse == 5 || th.forceUse == 6);
                        thrustInputText = "Leftward or Rightward";
                        break;
                    case PIMCategory.Vertical:
                        isThrusterMissing = !shipControlModule.shipInstance.thrusterList.Exists(th => th.forceUse == 3 || th.forceUse == 4);
                        thrustInputText = "Upward or Downward";
                        break;
                    case PIMCategory.Longitidinal:
                        isThrusterMissing = !shipControlModule.shipInstance.thrusterList.Exists(th => th.forceUse == 1 || th.forceUse == 2);
                        thrustInputText = "Forward or Backward";
                        break;
                }

                if (isThrusterMissing)
                {
                    EditorGUILayout.HelpBox("This input will have no effect because there is no Thruster in the ShipControlModule using it. To fix this add at least one Thruster configured with " + thrustInputText + " Thrust.", MessageType.Warning);
                }

            }
        }

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

                if (compType == typeof(CustomPlayerInput))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as CustomPlayerInput).showInEditor = isExpanded;
                    }
                }
            }
        }

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
            if (inputAxisModeProp.intValue != (int)PlayerInputModule.InputAxisMode.NoInput)
            {
                EditorGUILayout.PropertyField(isSensitivityEnabledProp, isSensitivityEnabledContent);
                if (isSensitivityEnabledProp.boolValue)
                {
                    EditorGUILayout.PropertyField(axisSensitivityProp, axisSensitivityContent);
                    EditorGUILayout.PropertyField(axisGravityProp, axisGravityContent);
                }
            }
        }

        private void DrawDiscardData(SerializedProperty sendDataProp, GUIContent sendDataContent)
        {
            EditorGUILayout.PropertyField(sendDataProp, sendDataContent);
        }

        #endregion

        #region Private Draw methods - Oculus API
        #if SSC_OVR

        private void DrawOVRInput
        (
         PIMCategory pimCategory,
         SerializedProperty inputAxisModeProp,
         SerializedProperty inputTypeProp,
         SerializedProperty positiveInputOVRProp,
         SerializedProperty negativeInputOVRProp,
         GUIContent ovrInputTypeContent,
         GUIContent ovrPositiveInputContent,
         GUIContent ovrNegativeInputContent,
         GUIContent ovrInputContent
        )
        {
            EditorGUILayout.PropertyField(inputAxisModeProp, inputAxisModeContent);
            if (inputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.NoInput) { }
            else
            {
                bool isCombined = inputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.CombinedAxis;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(inputTypeProp, ovrInputTypeContent);
                if (EditorGUI.EndChangeCheck())
                {
                    positiveInputOVRProp.intValue = 0;
                    negativeInputOVRProp.intValue = 0;
                }

                if (inputTypeProp.intValue == (int)PlayerInputModule.OculusInputType.Axis1D)
                {
                    OVRInput.Axis1D ovrInputAxis1D = (OVRInput.Axis1D)positiveInputOVRProp.intValue;
                    positiveInputOVRProp.intValue = (int)(OVRInput.Axis1D)EditorGUILayout.EnumPopup(isCombined ? ovrPositiveInputContent : ovrInputContent, ovrInputAxis1D);
                }
                else if (inputTypeProp.intValue == (int)PlayerInputModule.OculusInputType.Axis2D)
                {
                    OVRInput.Axis2D ovrInputAxis2D = (OVRInput.Axis2D)positiveInputOVRProp.intValue;
                    positiveInputOVRProp.intValue = (int)(OVRInput.Axis2D)EditorGUILayout.EnumPopup(isCombined ? ovrPositiveInputContent : ovrInputContent, ovrInputAxis2D);
                }
                else if (inputTypeProp.intValue == (int)PlayerInputModule.OculusInputType.Button)
                {
                    OVRInput.Button ovrInputButton = (OVRInput.Button)positiveInputOVRProp.intValue;
                    positiveInputOVRProp.intValue = (int)(OVRInput.Button)EditorGUILayout.EnumPopup(isCombined ? ovrPositiveInputContent : ovrInputContent, ovrInputButton);
                }
                else // Pose
                {
                    PlayerInputModule.VRPoseRotation ovrInputRotation = (PlayerInputModule.VRPoseRotation)positiveInputOVRProp.intValue;
                    positiveInputOVRProp.intValue = (int)(PlayerInputModule.VRPoseRotation)EditorGUILayout.EnumPopup(isCombined ? ovrPositiveInputContent : ovrInputContent, ovrInputRotation);
                }

                if (isCombined)
                {
                    if (inputTypeProp.intValue == (int)PlayerInputModule.OculusInputType.Axis1D)
                    {
                        OVRInput.Axis1D ovrInputAxis1D = (OVRInput.Axis1D)negativeInputOVRProp.intValue;
                        negativeInputOVRProp.intValue = (int)(OVRInput.Axis1D)EditorGUILayout.EnumPopup(ovrNegativeInputContent, ovrInputAxis1D);
                    }
                    else if (inputTypeProp.intValue == (int)PlayerInputModule.OculusInputType.Axis2D)
                    {
                        OVRInput.Axis2D ovrInputAxis2D = (OVRInput.Axis2D)negativeInputOVRProp.intValue;
                        negativeInputOVRProp.intValue = (int)(OVRInput.Axis2D)EditorGUILayout.EnumPopup(ovrNegativeInputContent, ovrInputAxis2D);
                    }
                    else if (inputTypeProp.intValue == (int)PlayerInputModule.OculusInputType.Button)
                    {
                        OVRInput.Button ovrInputButton = (OVRInput.Button)negativeInputOVRProp.intValue;
                        negativeInputOVRProp.intValue = (int)(OVRInput.Button)EditorGUILayout.EnumPopup(ovrNegativeInputContent, ovrInputButton);
                    }
                    else // Pose
                    {
                        // Reset back to single axis as it currently only supports the Head Mounted Display which doesn't make sense with a Combined Axis.
                        negativeInputOVRProp.intValue = 0;
                        inputAxisModeProp.intValue = (int)PlayerInputModule.InputAxisMode.SingleAxis;
                    }

                    if (positiveInputOVRProp.intValue == negativeInputOVRProp.intValue)
                    {
                        EditorGUILayout.HelpBox(identicalOVRInputWarningContent, MessageType.Warning);
                    }
                }

                VerifyThrusterPresent(pimCategory);
            }
        }

        // Oculus API Button-only input
        private void DrawOVRInput
        (
         SerializedProperty inputEnabledProp,
         SerializedProperty inputOVRProp,
         SerializedProperty inputCanBeHeldDownProp,
         GUIContent ovrInputEnabledContent,
         GUIContent ovrInputContent,
         GUIContent ovrInputCanBeHeldDownContent
        )
        {
            bool prevEnabled = inputEnabledProp.boolValue;
            EditorGUILayout.PropertyField(inputEnabledProp, ovrInputEnabledContent);
            // If enabled has been turned off, reset the xxxxInputOVR to 0 (None)
            if (prevEnabled != inputEnabledProp.boolValue && !inputEnabledProp.boolValue) { inputOVRProp.intValue = 0; }

            if (inputEnabledProp.boolValue)
            {
                OVRInput.Button ovrInputButton = (OVRInput.Button)inputOVRProp.intValue;
                inputOVRProp.intValue = (int)(OVRInput.Button)EditorGUILayout.EnumPopup(ovrInputContent, ovrInputButton);

                if (inputCanBeHeldDownProp != null)
                {
                    EditorGUILayout.PropertyField(inputCanBeHeldDownProp, ovrInputCanBeHeldDownContent);
                }
            }
        }

        // Oculus API Button-only input (without Is Enabled)
        private void DrawOVRInput
        (
         SerializedProperty inputOVRProp,
         SerializedProperty inputCanBeHeldDownProp,
         GUIContent ovrInputContent,
         GUIContent ovrInputCanBeHeldDownContent
        )
        {
            OVRInput.Button ovrInputButton = (OVRInput.Button)inputOVRProp.intValue;
            inputOVRProp.intValue = (int)(OVRInput.Button)EditorGUILayout.EnumPopup(ovrInputContent, ovrInputButton);

            if (inputCanBeHeldDownProp != null)
            {
                EditorGUILayout.PropertyField(inputCanBeHeldDownProp, ovrInputCanBeHeldDownContent);
            }
        }

        #endif
        #endregion

        #region Private Draw methods - Unity Input System
        #if SSC_UIS

        /// <summary>
        /// Unity Input System draw method used for axis.
        /// showSlots should be false for CustomPlayerInputs as they will return
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
        /// <param name="showSlots">set to false for CustomPlayerInputs</param>
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
            if (inputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.NoInput) { positiveInputActionIdProp.stringValue = string.Empty; }
            else if (inputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.CombinedAxis)
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
        /// Unity Input System draw method used for fire and docking buttons
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
        private void DrawUnityInputSystemFire
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
            if (inputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.NoInput) { }
            else
            {
                bool isCombined = inputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.CombinedAxis;

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

                if (inputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.CombinedAxis)
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

                // Check thrusters in shipcontrolmodule
                if (isCombined ? positiveInputActionIdProp.intValue >= 0 || negativeInputActionIdProp.intValue >= 0 : positiveInputActionIdProp.intValue >= 0) { VerifyThrusterPresent(pimCategory); }
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

        #region Private Draw methods - Vive
        #if VIU_PLUGIN

        private void DrawViveInput
        (
         PIMCategory pimCategory,
         SerializedProperty inputAxisModeProp,
         SerializedProperty inputTypeProp,
         SerializedProperty roleTypeProp,
         SerializedProperty positiveRoleIdProp,
         SerializedProperty negativeRoleIdProp,
         SerializedProperty positiveInputViveProp,
         SerializedProperty negativeInputViveProp,
         GUIContent viveInputTypeContent,
         GUIContent viveRoleTypeContent,
         GUIContent vivePositiveRoleIdContent,
         GUIContent viveNegativeRoleIdContent,
         GUIContent viveRoleIdContent,
         GUIContent vivePositiveInputContent,
         GUIContent viveNegativeInputContent,
         GUIContent viveInputContent
        )
        {
            EditorGUILayout.PropertyField(inputAxisModeProp, inputAxisModeContent);
            if (inputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.NoInput) { }
            else
            {
                bool isCombined = inputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.CombinedAxis;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(inputTypeProp, viveInputTypeContent);
                if (EditorGUI.EndChangeCheck())
                {
                    // When switching between types (Axis, Button, Pose) reset the enumeration values (which are stored as integers)
                    positiveInputViveProp.intValue = 0;
                    negativeInputViveProp.intValue = 0;
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(roleTypeProp, viveRoleTypeContent);
                if (EditorGUI.EndChangeCheck())
                {
                    // When switching between role types (DeviceRole, HandRole etc) reset the enumeration values (which are stored as integers)
                    positiveRoleIdProp.intValue = 0;
                    negativeRoleIdProp.intValue = 0;
                }

                // PlayerInputModule.ViveRoleType enum: BodyRole = 0,  DeviceRole = 1, HandRole = 2,TrackerRole = 3
                // Display an enumeration list for the selected Vive Role Type
                switch (roleTypeProp.intValue)
                {
                    case 0:
                        BodyRole bodyRole = (BodyRole)positiveRoleIdProp.intValue;
                        positiveRoleIdProp.intValue = (int)(BodyRole)EditorGUILayout.EnumPopup(isCombined ? vivePositiveRoleIdContent : viveRoleIdContent, bodyRole);
                        break;
                    case 1:
                        DeviceRole deviceRole = (DeviceRole)positiveRoleIdProp.intValue;
                        positiveRoleIdProp.intValue = (int)(DeviceRole)EditorGUILayout.EnumPopup(isCombined ? vivePositiveRoleIdContent : viveRoleIdContent, deviceRole);
                        break;
                    case 3:
                        TrackerRole trackerRole = (TrackerRole)positiveRoleIdProp.intValue;
                        positiveRoleIdProp.intValue = (int)(TrackerRole)EditorGUILayout.EnumPopup(isCombined ? vivePositiveRoleIdContent : viveRoleIdContent, trackerRole);
                        break;
                    default:    // HandRole 2
                        HandRole handRole = (HandRole)positiveRoleIdProp.intValue;
                        positiveRoleIdProp.intValue = (int)(HandRole)EditorGUILayout.EnumPopup(isCombined ? vivePositiveRoleIdContent : viveRoleIdContent, handRole);
                        break;
                }

                //string roleTypeName = System.Enum.GetName(typeof(PlayerInputModule.ViveRoleType), roleTypeProp.intValue);
                //System.Type roleType = System.Type.GetType("HTC.UnityPlugin.Vive." + roleTypeName + ", Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                //ViveRoleProperty viveRoleProp = ViveRoleProperty.New(roleType, positiveRoleIdProp.intValue);

                if (inputTypeProp.intValue == (int)PlayerInputModule.ViveInputType.Axis)
                {
                    ControllerAxis viveInputAxis = (ControllerAxis)positiveInputViveProp.intValue;
                    positiveInputViveProp.intValue = (int)(ControllerAxis)EditorGUILayout.EnumPopup(isCombined ? vivePositiveInputContent : viveInputContent, viveInputAxis);
                }
                else if (inputTypeProp.intValue == (int)PlayerInputModule.ViveInputType.Button)
                {
                    ControllerButton viveInputButton = (ControllerButton)positiveInputViveProp.intValue;
                    positiveInputViveProp.intValue = (int)(ControllerButton)EditorGUILayout.EnumPopup(isCombined ? vivePositiveInputContent : viveInputContent, viveInputButton);
                }
                else // Assume Pose
                {
                    PlayerInputModule.VRPoseRotation viveInputButton = (PlayerInputModule.VRPoseRotation)positiveInputViveProp.intValue;
                    positiveInputViveProp.intValue = (int)(PlayerInputModule.VRPoseRotation)EditorGUILayout.EnumPopup(isCombined ? vivePositiveInputContent : viveInputContent, viveInputButton);
                }

                if (isCombined)
                {
                    // PlayerInputModule.ViveRoleType enum: BodyRole = 0,  DeviceRole = 1, HandRole = 2,TrackerRole = 3
                    // Display an enumeration list for the selected Vive Role Type
                    switch (roleTypeProp.intValue)
                    {
                        case 0:
                            BodyRole bodyRole = (BodyRole)negativeRoleIdProp.intValue;
                            negativeRoleIdProp.intValue = (int)(BodyRole)EditorGUILayout.EnumPopup(viveNegativeRoleIdContent, bodyRole);
                            break;
                        case 1:
                            DeviceRole deviceRole = (DeviceRole)negativeRoleIdProp.intValue;
                            negativeRoleIdProp.intValue = (int)(DeviceRole)EditorGUILayout.EnumPopup(viveNegativeRoleIdContent, deviceRole);
                            break;
                        case 3:
                            TrackerRole trackerRole = (TrackerRole)negativeRoleIdProp.intValue;
                            negativeRoleIdProp.intValue = (int)(TrackerRole)EditorGUILayout.EnumPopup(viveNegativeRoleIdContent, trackerRole);
                            break;
                        default:    // HandRole 2
                            HandRole handRole = (HandRole)negativeRoleIdProp.intValue;
                            negativeRoleIdProp.intValue = (int)(HandRole)EditorGUILayout.EnumPopup(viveNegativeRoleIdContent, handRole);
                            break;
                    }

                    if (inputTypeProp.intValue == (int)PlayerInputModule.ViveInputType.Axis)
                    {
                        ControllerAxis viveInputAxis = (ControllerAxis)negativeInputViveProp.intValue;
                        negativeInputViveProp.intValue = (int)(ControllerAxis)EditorGUILayout.EnumPopup(viveNegativeInputContent, viveInputAxis);
                    }
                    else if (inputTypeProp.intValue == (int)PlayerInputModule.ViveInputType.Button)
                    {
                        ControllerButton viveInputButton = (ControllerButton)negativeInputViveProp.intValue;
                        negativeInputViveProp.intValue = (int)(ControllerButton)EditorGUILayout.EnumPopup(viveNegativeInputContent, viveInputButton);
                    }
                    else // assume Pose
                    {
                        PlayerInputModule.VRPoseRotation viveInputButton = (PlayerInputModule.VRPoseRotation)negativeInputViveProp.intValue;
                        negativeInputViveProp.intValue = (int)(PlayerInputModule.VRPoseRotation)EditorGUILayout.EnumPopup(viveNegativeInputContent, viveInputButton);
                    }

                    if (positiveRoleIdProp.intValue == negativeRoleIdProp.intValue && positiveInputViveProp.intValue == negativeInputViveProp.intValue)
                    {
                        EditorGUILayout.HelpBox(identicalViveInputWarningContent, MessageType.Warning);
                    }
                }

                // Check thrusters in shipcontrolmodule
                VerifyThrusterPresent(pimCategory);
            }
        }

        // Vive Button Input only
        // Currently used for primary and secondary fire and docking buttons
        private void DrawViveInput
        (
         SerializedProperty inputEnabledProp,
         SerializedProperty roleTypeProp,
         SerializedProperty inputRoleIdProp,
         SerializedProperty inputViveCtrlProp,
         SerializedProperty inputCanBeHeldDownProp,
         GUIContent viveInputEnabledContent,
         GUIContent viveRoleTypeContent,
         GUIContent viveRoleIdContent,
         GUIContent viveButtonInputContent,
         GUIContent viveInputCanBeHeldDownContent
        )
        {
            bool prevEnabled = inputEnabledProp.boolValue;
            EditorGUILayout.PropertyField(inputEnabledProp, viveInputEnabledContent);
            // If enabled has been turned off, reset the inputViveXXX to 0 (None)
            if (prevEnabled != inputEnabledProp.boolValue && !inputEnabledProp.boolValue) { inputViveCtrlProp.intValue = 0; }

            if (inputEnabledProp.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(roleTypeProp, viveRoleTypeContent);
                if (EditorGUI.EndChangeCheck())
                {
                    // When switching between role types (DeviceRole, HandRole etc) reset the enumeration values (which are stored as integers)
                    inputRoleIdProp.intValue = 0;
                }

                // PlayerInputModule.ViveRoleType enum: BodyRole = 0,  DeviceRole = 1, HandRole = 2,TrackerRole = 3
                // Display an enumeration list for the selected Vive Role Type
                switch (roleTypeProp.intValue)
                {
                    case 0:
                        BodyRole bodyRole = (BodyRole)inputRoleIdProp.intValue;
                        inputRoleIdProp.intValue = (int)(BodyRole)EditorGUILayout.EnumPopup(viveRoleIdContent, bodyRole);
                        break;
                    case 1:
                        DeviceRole deviceRole = (DeviceRole)inputRoleIdProp.intValue;
                        inputRoleIdProp.intValue = (int)(DeviceRole)EditorGUILayout.EnumPopup(viveRoleIdContent, deviceRole);
                        break;
                    case 3:
                        TrackerRole trackerRole = (TrackerRole)inputRoleIdProp.intValue;
                        inputRoleIdProp.intValue = (int)(TrackerRole)EditorGUILayout.EnumPopup(viveRoleIdContent, trackerRole);
                        break;
                    default:    // HandRole 2
                        HandRole handRole = (HandRole)inputRoleIdProp.intValue;
                        inputRoleIdProp.intValue = (int)(HandRole)EditorGUILayout.EnumPopup(viveRoleIdContent, handRole);
                        break;
                }

                ControllerButton viveInputButton = (ControllerButton)inputViveCtrlProp.intValue;
                inputViveCtrlProp.intValue = (int)(ControllerButton)EditorGUILayout.EnumPopup(viveButtonInputContent, viveInputButton);

                // Docking does not have the Can Be Held Down option
                if (inputCanBeHeldDownProp != null)
                {
                    EditorGUILayout.PropertyField(inputCanBeHeldDownProp, viveInputCanBeHeldDownContent);
                }
            }
        }

        #endif
        #endregion

        #region Private Draw methods - XR
        #if SCSM_XR && SSC_UIS

        /// <summary>
        /// XR draw method used for axis. Gets data from the Unity Input System
        /// showSlots should be false for CustomPlayerInputs as they will return
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
        /// <param name="showSlots">Set to false for CustomPlayerInputs</param>
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
            if (inputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.NoInput) { positiveInputActionIdProp.stringValue = string.Empty; }
            else if (inputAxisModeProp.intValue == (int)PlayerInputModule.InputAxisMode.CombinedAxis)
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

        #region Unity Input System Methods
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

                    #region Horizontal (currently not used in sample events

                    #endregion

                    #region Vertical
                    action = actionMap.FindAction("Vertical");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "Vertical", UnityEngine.InputSystem.InputActionType.Value, null, null, null, null, "Axis");
                        if (action != null)
                        {
                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "Axis") // Or "1DAxis"
                                 .With("Positive", "<Gamepad>/rightStick/up", "Gamepad")
                                 .With("Negative", "<Gamepad>/rightStick/down", "Gamepad");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis") // Or "Axis"
                                 .With("Positive", "<Keyboard>/e", "Keyboard&Mouse")
                                 .With("Negative", "<Keyboard>/q", "Keyboard&Mouse");
                        }
                    }
                    #endregion

                    #region Longitudinal
                    action = actionMap.FindAction("Longitudinal");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "Longitudinal", UnityEngine.InputSystem.InputActionType.Value, null, null, null, null, "Axis");
                        if (action != null)
                        {
                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                .With("Positive", "<Gamepad>/rightTrigger", "Gamepad")
                                .With("Negative", "<Gamepad>/leftTrigger", "Gamepad");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                .With("Positive", "<Keyboard>/upArrow", "Keyboard&Mouse")
                                .With("Negative", "<Keyboard>/downArrow", "Keyboard&Mouse");
                        }
                    }
                    #endregion

                    #region Pitch
                    action = actionMap.FindAction("Pitch");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "Pitch", UnityEngine.InputSystem.InputActionType.Value, null, null, null, null, "Axis");
                        if (action != null)
                        {
                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                 .With("Positive", "<Keyboard>/w", "Keyboard&Mouse")
                                 .With("Negative", "<Keyboard>/s", "Keyboard&Mouse");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                .With("Positive", "<Gamepad>/leftStick/up", "Gamepad")
                                .With("Negative", "<Gamepad>/leftStick/down", "Gamepad");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                .With("Positive", "<Joystick>/stick/up", "Joystick")
                                .With("Negative", "<Joystick>/stick/down", "Joystick");
                        }
                    }
                    #endregion

                    #region Yaw
                    action = actionMap.FindAction("Yaw");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "Yaw", UnityEngine.InputSystem.InputActionType.Value, null, null, null, null, "Axis");
                        if (action != null)
                        {
                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                 .With("Positive", "<Keyboard>/d", "Keyboard&Mouse")
                                 .With("Negative", "<Keyboard>/a", "Keyboard&Mouse");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                .With("Positive", "<Gamepad>/leftStick/right", "Gamepad")
                                .With("Negative", "<Gamepad>/leftStick/left", "Gamepad");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis")
                                .With("Positive", "<Joystick>/stick/right", "Joystick")
                                .With("Negative", "<Joystick>/stick/left", "Joystick");
                        }
                    }
                    #endregion

                    #region Roll
                    action = actionMap.FindAction("Roll");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "Roll", UnityEngine.InputSystem.InputActionType.Value, null, null, null, null, "Axis");
                        if (action != null)
                        {
                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis") // Or "Axis"
                                 .With("Positive", "<Gamepad>/rightStick/right", "Gamepad")
                                 .With("Negative", "<Gamepad>/rightStick/left", "Gamepad");

                            UnityEngine.InputSystem.InputActionSetupExtensions.AddCompositeBinding(action, "1DAxis") // Or "Axis"
                                 .With("Positive", "<Keyboard>/rightArrow", "Keyboard&Mouse")
                                 .With("Negative", "<Keyboard>/leftArrow", "Keyboard&Mouse");
                        }
                    }
                    #endregion

                    #region Primary Fire
                    action = actionMap.FindAction("PrimaryFire");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "PrimaryFire", UnityEngine.InputSystem.InputActionType.Button, null, null, null, null, "Button");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Keyboard>/space", null, null, "Keyboard&Mouse").WithName("SpaceBar");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Gamepad>/buttonSouth", null, null, "Gamepad").WithName("AorCrossButton");
                        // Instead of using Touch here, use OnScreen controls.
                        //UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Touchscreen>/primaryTouch/tap", null, null, "Touch").WithName("TouchTap");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Joystick>/trigger", null, null, "Joystick").WithName("JoyTrigger");
                    }
                    #endregion

                    #region Secondary Fire
                    action = actionMap.FindAction("SecondaryFire");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "SecondaryFire", UnityEngine.InputSystem.InputActionType.Button, null, null, null, null, "Button");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Mouse>/leftButton", null, null, "Keyboard&Mouse").WithName("LeftMouseButton");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Gamepad>/buttonNorth", null, null, "Gamepad").WithName("YorTriangleButton");
                    }
                    #endregion

                    #region Dock
                    action = actionMap.FindAction("Dock");
                    if (action == null)
                    {
                        action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                    actionMap, "Dock", UnityEngine.InputSystem.InputActionType.Button, null, null, null, null, "Button");
                        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Keyboard>/u", null, null, "Keyboard&Mouse").WithName("KeyU");
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
                    Debug.LogWarning("ERROR: PlayerInputModule - could not save asset: " + uisPlayerInput.actions.name + " PLEASE REPORT - " + ex.Message);
                }

                //EditorUtility.SetDirty(uisPlayerInput.actions);
            }
        }

        private void UISGetorCreateCanvas(out GameObject controlsCanvasGO, out Canvas controlCanvas, out Vector2 canvasSize)
        {
            controlsCanvasGO = null;
            canvasSize = Vector2.zero;
            
            #if UNITY_2022_2_OR_NEWER
            Canvas[] canvasArray = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            #else
            Canvas[] canvasArray = FindObjectsOfType<Canvas>();
            #endif

            controlCanvas = ArrayUtility.Find(canvasArray, cv => cv.name == "ControlsCanvas");

            // If ControlsCanvas doesn't exist, create it
            if (controlCanvas == null)
            {
                controlsCanvasGO = new GameObject("ControlsCanvas");
                controlsCanvasGO.layer = 5;
                controlsCanvasGO.AddComponent<Canvas>();

                controlCanvas = controlsCanvasGO.GetComponent<Canvas>();
                controlCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                controlsCanvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                controlsCanvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // Add an Event System if it doesn't already exist.
            // NOTE: If it is disabled in scene, a new one isn't added.
            EditorApplication.ExecuteMenuItem("GameObject/UI/Event System");

            if (controlCanvas != null)
            {
                controlsCanvasGO = controlCanvas.gameObject;
                canvasSize = controlsCanvasGO.GetComponent<RectTransform>().sizeDelta;
            }
        }

        private void UISAddOnScreenControls(string controlToAdd)
        {
            GameObject controlsCanvasGO;
            Canvas controlCanvas;
            Vector2 canvasSize;

            UISGetorCreateCanvas(out controlsCanvasGO, out controlCanvas, out canvasSize);

            if (controlCanvas != null)
            {
                // Get a list of onscreen joysticks on the control canvas
                UnityEngine.InputSystem.OnScreen.OnScreenStick[] stickArray = controlsCanvasGO.GetComponentsInChildren<UnityEngine.InputSystem.OnScreen.OnScreenStick>();

                if (controlToAdd == "LeftStick")
                {
                    // Offset from bottom-left of canvas
                    UISAddOnScreenStickIfMissing(stickArray, "LeftStick", "/<GamePad>/LeftStick", 100f, 150f, 0f, 0f, 0f, 0f, controlsCanvasGO.transform);
                }
                if (controlToAdd == "RightStick")
                {
                    // Offset from bottom-right of canvas
                    UISAddOnScreenStickIfMissing(stickArray, "RightStick", "/<GamePad>/RightStick", canvasSize.x - 100f, 150f, 1f, 0f, 1f, 0f, controlsCanvasGO.transform);
                }
                else
                {
                    // Get a list of onscreen buttons on the control canvas
                    UnityEngine.InputSystem.OnScreen.OnScreenButton[] buttonsArray = controlsCanvasGO.GetComponentsInChildren<UnityEngine.InputSystem.OnScreen.OnScreenButton>();

                    if (controlToAdd == "LeftButtons")
                    {
                        // Offset from bottom-left of canvas
                        UISAddOnScreenButtonIfMissing(buttonsArray, "LButtonUp", "<Keyboard>/w", 210f, 205f, 0f, 0f, 0f, 0f, controlsCanvasGO.transform);
                        UISAddOnScreenButtonIfMissing(buttonsArray, "LButtonDown", "<Keyboard>/s", 210f, 100f, 0f, 0f, 0f, 0f, controlsCanvasGO.transform);
                        UISAddOnScreenButtonIfMissing(buttonsArray, "LButtonLeft", "<Keyboard>/a", 105f, 100f, 0f, 0f, 0f, 0f, controlsCanvasGO.transform);
                        UISAddOnScreenButtonIfMissing(buttonsArray, "LButtonRight", "<Keyboard>/d", 315f, 100f, 0f, 0f, 0f, 0f, controlsCanvasGO.transform);
                    }
                    else if (controlToAdd == "RightButtons")
                    {
                        // Offset from bottom-right of canvas

                        UISAddOnScreenButtonIfMissing(buttonsArray, "ButtonUp", "<GamePad>/rightTrigger", canvasSize.x - 210f, 205f, 1f, 0f, 1f, 0f, controlsCanvasGO.transform);
                        UISAddOnScreenButtonIfMissing(buttonsArray, "ButtonDown", "<GamePad>/leftTrigger", canvasSize.x - 210f, 100f, 1f, 0f, 1f, 0f, controlsCanvasGO.transform);
                        UISAddOnScreenButtonIfMissing(buttonsArray, "ButtonRollRight", "<Keyboard>/rightArrow", canvasSize.x - 105f, 100f, 1f, 0f, 1f, 0f, controlsCanvasGO.transform);
                        UISAddOnScreenButtonIfMissing(buttonsArray, "ButtonRollLeft", "<Keyboard>/leftArrow", canvasSize.x - 315f, 100f, 1f, 0f, 1f, 0f, controlsCanvasGO.transform);

                        UISAddOnScreenTxtButtonIfMissing(buttonsArray, "ButtonFB1", "FB1", "<Gamepad>/buttonSouth", canvasSize.x / 2f - 315f, 205f + (canvasSize.y / 2f), 1f, 0f, 1f, 0f, controlsCanvasGO.transform);
                        UISAddOnScreenTxtButtonIfMissing(buttonsArray, "ButtonFB2", "FB2", "<Gamepad>/buttonNorth", canvasSize.x / 2f - 105f, 205f + (canvasSize.y / 2f), 1f, 0f, 1f, 0f, controlsCanvasGO.transform);
                    }
                }
            }
        }

        private void UISAddOnScreenQuit()
        {
            GameObject controlsCanvasGO;
            Canvas controlCanvas;
            Vector2 canvasSize;

            UISGetorCreateCanvas(out controlsCanvasGO, out controlCanvas, out canvasSize);

            if (controlCanvas != null)
            {
                // Get a list of onscreen buttons on the control canvas
                UnityEngine.InputSystem.OnScreen.OnScreenButton[] buttonsArray = controlsCanvasGO.GetComponentsInChildren<UnityEngine.InputSystem.OnScreen.OnScreenButton>();

                // Place in top-right corner
                UISAddOnScreenTxtButtonIfMissing(buttonsArray, "ButtonQuit", "Quit", "<Gamepad>/start", canvasSize.x / 2f - 50f, canvasSize.y * 1.5f - 50f, 1f, 0f, 1f, 0f, controlsCanvasGO.transform);

                // Add the action to go with the button
                if (uisPlayerInput != null && uisPlayerInput.actions != null)
                {
                    UnityEngine.InputSystem.InputActionMap actionMap = uisPlayerInput.actions.FindActionMap(uisPlayerInput.defaultActionMap);
                    if (EditorUtility.DisplayDialog("Add Action", "Add Quit Action to Default Map: " + actionMap.name + "?", "Yes", "CANCEL!"))
                    {
                        UnityEngine.InputSystem.InputAction action = actionMap.FindAction("QuitApp");
                        if (action == null)
                        {
                            action = UnityEngine.InputSystem.InputActionSetupExtensions.AddAction(
                                        actionMap, "QuitApp", UnityEngine.InputSystem.InputActionType.Button, null, null, null, null, "Button");
                            UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, "<Gamepad>/start", null, null, "Gamepad").WithName("StartQuitButton");
                            UISSaveInputAsset();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If an OnScreen button is missing from the canvas, add it.
        /// </summary>
        /// <param name="buttonsArray"></param>
        /// <param name="buttonName"></param>
        /// <param name="controlPath"></param>
        /// <param name="buttonOffsetX"></param>
        /// <param name="buttonOffsetY"></param>
        /// <param name="parentTrfm"></param>
        private void UISAddOnScreenButtonIfMissing
        (
         UnityEngine.InputSystem.OnScreen.OnScreenButton[] buttonsArray,
         string buttonName,
         string controlPath,
         float buttonOffsetX, float buttonOffsetY,
         float anchorMinX, float anchorMinY,
         float anchorMaxX, float anchorMaxY,
         Transform parentTrfm
        )
        {
            UnityEngine.InputSystem.OnScreen.OnScreenButton button = ArrayUtility.Find(buttonsArray, btn => btn.name == buttonName);
            if (button == null)
            {
                GameObject buttonGO = new GameObject(buttonName);
                buttonGO.layer = 5;
                buttonGO.transform.SetParent(parentTrfm);
                RectTransform rectTrfm = buttonGO.AddComponent<RectTransform>();
                rectTrfm.anchorMin = new Vector2(anchorMinX, anchorMinY);
                rectTrfm.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
                buttonGO.transform.position = new Vector3(buttonOffsetX, buttonOffsetY, 0f);
                buttonGO.AddComponent<CanvasRenderer>();
                UnityEngine.UI.Image buttomImg = buttonGO.AddComponent<UnityEngine.UI.Image>();
                UnityEngine.UI.Button buttonBtn = buttonGO.AddComponent<UnityEngine.UI.Button>();
                buttomImg.color = new Color(1f,1f,1f, 50f / 255f);
                buttomImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                buttomImg.type = UnityEngine.UI.Image.Type.Sliced;
                UnityEngine.InputSystem.OnScreen.OnScreenButton newButton = buttonGO.AddComponent<UnityEngine.InputSystem.OnScreen.OnScreenButton>();
                newButton.controlPath = controlPath;
            }
        }

        /// <summary>
        /// If an OnScreen fire button is missing from the canvas, add it.
        /// </summary>
        /// <param name="buttonsArray"></param>
        /// <param name="buttonName"></param>
        /// <param name="controlPath"></param>
        /// <param name="buttonOffsetX"></param>
        /// <param name="buttonOffsetY"></param>
        /// <param name="anchorMinX"></param>
        /// <param name="anchorMinY"></param>
        /// <param name="anchorMaxX"></param>
        /// <param name="anchorMaxY"></param>
        /// <param name="parentTrfm"></param>
        private void UISAddOnScreenTxtButtonIfMissing
        (
         UnityEngine.InputSystem.OnScreen.OnScreenButton[] buttonsArray,
         string buttonName,
         string buttonText,
         string controlPath,
         float buttonOffsetX, float buttonOffsetY,
         float anchorMinX, float anchorMinY,
         float anchorMaxX, float anchorMaxY,
         Transform parentTrfm
        )
        {
            UnityEngine.InputSystem.OnScreen.OnScreenButton button = ArrayUtility.Find(buttonsArray, btn => btn.name == buttonName);
            if (button == null)
            {
                GameObject buttonGO = new GameObject(buttonName);
                buttonGO.layer = 5;
                buttonGO.transform.position = new Vector3(buttonOffsetX, buttonOffsetY, 0f);
                buttonGO.transform.SetParent(parentTrfm);
                RectTransform rectTrfm = buttonGO.AddComponent<RectTransform>();
                rectTrfm.anchorMin = new Vector2(anchorMinX, anchorMinY);
                rectTrfm.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
                buttonGO.AddComponent<CanvasRenderer>();
                UnityEngine.UI.Image buttomImg = buttonGO.AddComponent<UnityEngine.UI.Image>();
                UnityEngine.UI.Button buttonBtn = buttonGO.AddComponent<UnityEngine.UI.Button>();
                buttomImg.color = new Color(1f,1f,1f, 50f / 255f);
                Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                if (sprite != null) { buttomImg.sprite = sprite; }
                buttomImg.type = UnityEngine.UI.Image.Type.Simple;
                // Add text
                GameObject buttonTxtGO = new GameObject("Text");
                buttonTxtGO.layer = 5;
                rectTrfm = buttonTxtGO.AddComponent<RectTransform>();
                // Centre the text in the button
                rectTrfm.anchorMin = new Vector2(0f, 0f);
                rectTrfm.anchorMax = new Vector2(1f, 1f);
                buttonTxtGO.AddComponent<CanvasRenderer>();
                UnityEngine.UI.Text buttonTxt = buttonTxtGO.AddComponent<UnityEngine.UI.Text>();
                buttonTxt.text = buttonText;
                buttonTxt.alignment = TextAnchor.MiddleCenter;
                buttonTxtGO.transform.SetParent(buttonGO.transform);
                buttonTxt.transform.localPosition = new Vector3(0f, 0f, 0f);
                // Add onscreen button
                UnityEngine.InputSystem.OnScreen.OnScreenButton newButton = buttonGO.AddComponent<UnityEngine.InputSystem.OnScreen.OnScreenButton>();
                newButton.controlPath = controlPath;
            }
        }

        /// <summary>
        /// If an OnScreenStick is missing from the canvas, add it
        /// </summary>
        /// <param name="stickArray"></param>
        /// <param name="stickName"></param>
        /// <param name="controlPath"></param>
        /// <param name="buttonOffsetX"></param>
        /// <param name="buttonOffsetY"></param>
        /// <param name="anchorMinX"></param>
        /// <param name="anchorMinY"></param>
        /// <param name="anchorMaxX"></param>
        /// <param name="anchorMaxY"></param>
        /// <param name="parentTrfm"></param>
        private void UISAddOnScreenStickIfMissing
        (
         UnityEngine.InputSystem.OnScreen.OnScreenStick[] stickArray,
         string stickName,
         string controlPath,
         float buttonOffsetX, float buttonOffsetY,
         float anchorMinX, float anchorMinY,
         float anchorMaxX, float anchorMaxY,
         Transform parentTrfm
        )
        {
            UnityEngine.InputSystem.OnScreen.OnScreenStick stick = ArrayUtility.Find(stickArray, btn => btn.name == stickName);
            if (stick == null)
            {
                GameObject stickGO = new GameObject(stickName);
                stickGO.layer = 5;
                stickGO.transform.SetParent(parentTrfm);
                RectTransform rectTrfm = stickGO.AddComponent<RectTransform>();
                rectTrfm.anchorMin = new Vector2(anchorMinX, anchorMinY);
                rectTrfm.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
                stickGO.transform.position = new Vector3(buttonOffsetX, buttonOffsetY, 0f);
                stickGO.AddComponent<CanvasRenderer>();
                UnityEngine.UI.Image stickImg = stickGO.AddComponent<UnityEngine.UI.Image>();
                UnityEngine.UI.Button stickBtn = stickGO.AddComponent<UnityEngine.UI.Button>();
                stickImg.color = new Color(1f,1f,1f, 80f / 255f);
                Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                if (sprite != null) { stickImg.sprite = sprite; }
                stickImg.type = UnityEngine.UI.Image.Type.Simple;
                UnityEngine.InputSystem.OnScreen.OnScreenStick newStick = stickGO.AddComponent<UnityEngine.InputSystem.OnScreen.OnScreenStick>();
                newStick.controlPath = controlPath;
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

        #region Rewired Methods
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

                actionIDsHorizPositive = new int[1];
                ActionGUIContentHorizPositive = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(playerInputModule.positiveHorizontalInputActionId), ref actionIDsHorizPositive, ref ActionGUIContentHorizPositive);

                actionIDsHorizNegative = new int[1];
                ActionGUIContentHorizNegative = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(playerInputModule.negativeHorizontalInputActionId), ref actionIDsHorizNegative, ref ActionGUIContentHorizNegative);

                actionIDsVertPositive = new int[1];
                ActionGUIContentVertPositive = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(playerInputModule.positiveVerticalInputActionId), ref actionIDsVertPositive, ref ActionGUIContentVertPositive);

                actionIDsVertNegative = new int[1];
                ActionGUIContentVertNegative = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(playerInputModule.negativeVerticalInputActionId), ref actionIDsVertNegative, ref ActionGUIContentVertNegative);

                actionIDsLngtdlPositive = new int[1];
                ActionGUIContentLngtdlPositive = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(playerInputModule.positiveLongitudinalInputActionId), ref actionIDsLngtdlPositive, ref ActionGUIContentLngtdlPositive);

                actionIDsLngtdlNegative = new int[1];
                ActionGUIContentLngtdlNegative = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(playerInputModule.negativeLongitudinalInputActionId), ref actionIDsLngtdlNegative, ref ActionGUIContentLngtdlNegative);

                actionIDsPitchPositive = new int[1];
                ActionGUIContentPitchPositive = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(playerInputModule.positivePitchInputActionId), ref actionIDsPitchPositive, ref ActionGUIContentPitchPositive);

                actionIDsPitchNegative = new int[1];
                ActionGUIContentPitchNegative = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(playerInputModule.negativePitchInputActionId), ref actionIDsPitchNegative, ref ActionGUIContentPitchNegative);

                actionIDsYawPositive = new int[1];
                ActionGUIContentYawPositive = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(playerInputModule.positiveYawInputActionId), ref actionIDsYawPositive, ref ActionGUIContentYawPositive);

                actionIDsYawNegative = new int[1];
                ActionGUIContentYawNegative = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(playerInputModule.negativeYawInputActionId), ref actionIDsYawNegative, ref ActionGUIContentYawNegative);

                actionIDsRollPositive = new int[1];
                ActionGUIContentRollPositive = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(playerInputModule.positiveRollInputActionId), ref actionIDsRollPositive, ref ActionGUIContentRollPositive);

                actionIDsRollNegative = new int[1];
                ActionGUIContentRollNegative = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(playerInputModule.negativeRollInputActionId), ref actionIDsRollNegative, ref ActionGUIContentRollNegative);

                actionIDsPrimaryFire = new int[1];
                ActionGUIContentPrimaryFire = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(playerInputModule.primaryFireInputActionId), ref actionIDsPrimaryFire, ref ActionGUIContentPrimaryFire);

                actionIDsSecondaryFire = new int[1];
                ActionGUIContentSecondaryFire = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(playerInputModule.secondaryFireInputActionId), ref actionIDsSecondaryFire, ref ActionGUIContentSecondaryFire);

                actionIDsDocking = new int[1];
                ActionGUIContentDocking = new GUIContent[1];
                RWRefreshActions(RWGetActionCategoryID(playerInputModule.dockingInputActionId), ref actionIDsDocking, ref ActionGUIContentDocking);

                int numCPI = playerInputModule.customPlayerInputList == null ? 0 : playerInputModule.customPlayerInputList.Count;

                for (int cpiIdx = 0; cpiIdx < numCPI; cpiIdx++)
                {
                    CustomPlayerInput customPlayerInput = playerInputModule.customPlayerInputList[cpiIdx];

                    if (customPlayerInput != null)
                    {
                        customPlayerInput.actionIDsPositive = new int[1];
                        customPlayerInput.ActionGUIContentPositive = new GUIContent[1];
                        RWRefreshActions(RWGetActionCategoryID(customPlayerInput.rwdPositiveInputActionId), ref customPlayerInput.actionIDsPositive, ref customPlayerInput.ActionGUIContentPositive);

                        customPlayerInput.actionIDsNegative = new int[1];
                        customPlayerInput.ActionGUIContentNegative = new GUIContent[1];
                        RWRefreshActions(RWGetActionCategoryID(customPlayerInput.rwdNegativeInputActionId), ref customPlayerInput.actionIDsNegative, ref customPlayerInput.ActionGUIContentNegative);
                    }
                }
            }
            else
            {
                actionIDsHorizPositive = null;
                ActionGUIContentHorizPositive = null;
                actionIDsHorizNegative = null;
                ActionGUIContentHorizNegative = null;

                actionIDsVertPositive = null;
                ActionGUIContentVertPositive = null;
                actionIDsVertNegative = null;
                ActionGUIContentVertNegative = null;

                actionIDsLngtdlPositive = null;
                ActionGUIContentLngtdlPositive = null;
                actionIDsLngtdlNegative = null;
                ActionGUIContentLngtdlNegative = null;

                actionIDsPitchPositive = null;
                ActionGUIContentPitchPositive = null;
                actionIDsPitchNegative = null;
                ActionGUIContentPitchNegative = null;

                actionIDsYawPositive = null;
                ActionGUIContentYawPositive = null;
                actionIDsYawNegative = null;
                ActionGUIContentYawNegative = null;

                actionIDsRollPositive = null;
                ActionGUIContentRollPositive = null;
                actionIDsRollNegative = null;
                ActionGUIContentRollNegative = null;

                actionIDsPrimaryFire = null;
                ActionGUIContentPrimaryFire = null;

                actionIDsSecondaryFire = null;
                ActionGUIContentSecondaryFire = null;

                actionIDsDocking = null;
                ActionGUIContentDocking = null;

                int numCPI = playerInputModule.customPlayerInputList == null ? 0 : playerInputModule.customPlayerInputList.Count;

                for (int cpiIdx = 0; cpiIdx < numCPI; cpiIdx++)
                {
                    CustomPlayerInput customPlayerInput = playerInputModule.customPlayerInputList[cpiIdx];

                    if (customPlayerInput != null)
                    {
                        customPlayerInput.actionIDsPositive = null;
                        customPlayerInput.actionIDsNegative = null;
                        customPlayerInput.ActionGUIContentPositive = null;
                        customPlayerInput.ActionGUIContentNegative = null;
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

        #else

        #endif
        #endregion

        #region XR Methods
        #if SCSM_XR && SSC_UIS

        /// <summary>
        /// Create a hand transform under "XR Hands Offset"
        /// </summary>
        /// <param name="handProperty"></param>
        /// <param name="isLefthand"></param>
        private void XRCreateHand(SerializedProperty handProperty, bool isLefthand)
        {
            // Check if it already exists to help with Undo.
            Transform xrHandsTrfm = playerInputModule.transform.Find(xrHandsOffsetName);

            if (xrHandsTrfm == null)
            {
                xrHandsTrfm = SSCUtils.GetOrCreateChildTransform(playerInputModule.transform, xrHandsOffsetName);

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
                    xrHandTrfm = SSCUtils.GetOrCreateChildTransform(xrHandsTrfm, isLefthand ? xrLeftHandName : xrRightHandName);
                    Undo.RegisterCreatedObjectUndo(xrHandTrfm.gameObject, string.Empty);

                    xrHandTrfm.localPosition = Vector3.zero;
                    xrHandTrfm.localRotation = Quaternion.identity;

                    // Distinguish between the old XR TrackPoseDriver and the one with the new Unity Input System.
                    var _tposeDriver = Undo.AddComponent(xrHandTrfm.gameObject, typeof(UnityEngine.InputSystem.XR.TrackedPoseDriver)) as UnityEngine.InputSystem.XR.TrackedPoseDriver;

                    if (_tposeDriver != null)
                    {
                        _tposeDriver.trackingType = UnityEngine.InputSystem.XR.TrackedPoseDriver.TrackingType.RotationAndPosition;

                        UnityEngine.InputSystem.InputAction _tposPosition = new UnityEngine.InputSystem.InputAction(null, UnityEngine.InputSystem.InputActionType.Value);
                        if (_tposPosition != null)
                        {
                            //_tposPosition.expectedControlType = "Quaternion";
                            _tposeDriver.positionAction = _tposPosition;
                        }

                        UnityEngine.InputSystem.InputAction _tposRotation = new UnityEngine.InputSystem.InputAction(null, UnityEngine.InputSystem.InputActionType.Value);
                        if (_tposRotation != null)
                        {
                            _tposRotation.expectedControlType = "Quaternion";
                            _tposeDriver.rotationAction = _tposRotation;
                        }
                    }
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

                UnityEngine.InputSystem.InputActionAsset inputActionAsset = playerInputModule.GetXRInputActionAsset();
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
