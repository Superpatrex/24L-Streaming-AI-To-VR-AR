using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;

// Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(StickyDisplayModule))]
    public class StickyDisplayModuleEditor : Editor
    {
        #region Enumerations

        #endregion

        #region Custom Editor private variables
        private StickyDisplayModule stickyDisplayModule;

        private string s3dHelpPDF;

        // Formatting and style variables
        private string txtColourName = "Black";
        private Color defaultTextColour = Color.black;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle foldoutStyleNoLabel;
        private static GUIStyle toggleCompactButtonStyleNormal = null;  // Small Toggle button. e.g. G(izmo) on/off
        private static GUIStyle toggleCompactButtonStyleToggled = null;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        private Sprite tempSprite = null;

        // Similar to isSceneDirtyRequired (SceneView variabled) but used for Inspector modifications.
        private bool isSceneModified = false;
        private bool isDebuggingEnabled = false;

        private int displayReticleMoveDownPos = -1;
        private int displayReticleInsertPos = -1;
        private int displayReticleDeletePos = -1;

        private int displayGaugeMoveDownPos = -1;
        private int displayGaugeInsertPos = -1;
        private int displayGaugeDeletePos = -1;

        private int displayMessageMoveDownPos = -1;
        private int displayMessageInsertPos = -1;
        private int displayMessageDeletePos = -1;

        private int displayTargetMoveDownPos = -1;
        private int displayTargetInsertPos = -1;
        private int displayTargetDeletePos = -1;

        private string tempDisplayReticleName = string.Empty;

        #endregion

        #region SceneView Variables
        private Color hudPanelOutlineColour = Color.yellow;
        private Color targetsAreaOutlineColour = Color.red;
        #endregion

        #region Static Strings
        private readonly static string autoHideLockReticleWarning = "With Auto-hide Cursor and Lock Reticle to Cursor on at the same time you may get undesirable side effects";
        private readonly static string dgMissingFgndSpriteWarning = "The foreground sprite (texture) is required to display the gauge value";
        #endregion

        #region GUIContent General

        private readonly static GUIContent headerContent = new GUIContent("This module enables you to show information for the player. Functionality can be extended with your own scripts. See manual for details.");
        private readonly static GUIContent btnRefreshContent = new GUIContent("Refresh", "Used in edit-mode after the window has been resized");
        private readonly static GUIContent generalSettingsContent = new GUIContent("General Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent initialiseOnStartContent = new GUIContent(" Initialise on Start", "If enabled, Initialise() will be called as soon as Start() runs. " +
          "This should be disabled if you are instantiating instantiating the display through code.");
        private readonly static GUIContent isShowOnInitialiseContent = new GUIContent(" Show on Initialise", "Show the display when it is first Initialised");
        private readonly static GUIContent showOverlayContent = new GUIContent(" Show Overlay", "Show the overlay image on the display");
        private readonly static GUIContent autoHideCursorContent = new GUIContent(" Auto-hide Cursor", "Hide the screen cursor or mouse pointer after it has been stationary for x seconds");
        private readonly static GUIContent mainCameraContent = new GUIContent(" Main Camera", "The main camera used to perform calculations with the display. If blank will be auto-assigned to the first camera with a MainCamera tag.");
        private readonly static GUIContent hideCursorTimeContent = new GUIContent(" Hide Cursor Time", "The number of seconds to wait until after the cursor has not moved before hiding it");
        private readonly static GUIContent displayWidthContent = new GUIContent(" Display Width", "The display's normalised width of the screen. 1.0 is full width, 0.5 is half width. [Has no effect outside play mode]");
        private readonly static GUIContent displayHeightContent = new GUIContent(" Display Height", "The display's normalised height of the screen. 1.0 is full height, 0.5 is half height. [Has no effect outside play mode]");
        private readonly static GUIContent displayOffsetXContent = new GUIContent(" Display Offset X", "The display's normalised offset between the left (-1) and the right (1) from the centre (0) of the screen. [Has no effect outside play mode]");
        private readonly static GUIContent displayOffsetYContent = new GUIContent(" Display Offset Y", "The display's normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen. [Has no effect outside play mode]");
        private readonly static GUIContent primaryColourContent = new GUIContent(" Primary Colour", "The primary colour of the display. Used to colour the display overlay. This are affected by Brightness. Calibrate when Brightness = 1");
        private readonly static GUIContent brightnessContent = new GUIContent(" Brightness", "The overall brightness of the display");
        private readonly static GUIContent canvasSortOrderContent = new GUIContent(" Canvas Sort Order", "The sort order of the canvas in the scene. Higher numbers are on top.");
        private readonly static GUIContent showDisplayOutlineInSceneContent = new GUIContent(" Show Display Outline", "Show the display as a yellow outline in the scene view [Has no effect in play mode]. Click Refresh if screen has been resized.");

        private readonly static GUIContent spriteContent = new GUIContent(" Reticle Sprite", "Preview of the sprite that will be shown on the display");
        #endregion

        #region GUIContent - Reticle
        private readonly static GUIContent reticleSettingsContent = new GUIContent("Display Reticle Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent drPrimarySpriteContent = new GUIContent(" Sprite", "The sprite (texture) to be displayed in the heads-up display");
        private readonly static GUIContent activeDisplayReticleContent = new GUIContent(" Active Display Reticle", "The currently selected or displayed reticle. It is used to help aim at things in front of the character.");
        private readonly static GUIContent showActiveDisplayReticleContent = new GUIContent(" Show Active Reticle", "Show or render the active Reticle on the display. [Has no effect outside play mode]");
        private readonly static GUIContent displayReticleOffsetXContent = new GUIContent(" Reticle Offset X", "The Display Reticle's normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.");
        private readonly static GUIContent displayReticleOffsetYContent = new GUIContent(" Reticle Offset Y", "The Display Reticle's normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.");
        private readonly static GUIContent lockDisplayReticleToCursorContent = new GUIContent(" Lock Reticle to Cursor", "Should the Display Reticle follow the cursor or mouse position on the screen?");
        private readonly static GUIContent activeDisplayReticleColourContent = new GUIContent(" Reticle Colour", "Colour of the active Display Reticle");
        #endregion

        #region GUIContent - Gauges
        private readonly static GUIContent displayGaugeSettingsContent = new GUIContent("Display Gauge Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent displayGaugeHeaderContent = new GUIContent("Gauges are stacking or overlayed in the order they appear in the list.");
        private readonly static GUIContent dgGaugeNameSettingsContent = new GUIContent(" Gauge Name", "The name or description of the gauge. This can be used to identify the gauge.");
        private readonly static GUIContent dgGaugeStringSettingsContent = new GUIContent(" Gauge Text", "The text to display in the gauge. It can include RichText markup. e.g. <b>Bold Text</b>");
        private readonly static GUIContent dgGaugeValueSettingsContent = new GUIContent(" Gauge Value", "The current amount or reading on the gauge");
        private readonly static GUIContent dgShowGaugeSettingsContent = new GUIContent(" Show Gauge", "Show the gauge on the display. [Has no effect outside play mode]");
        private readonly static GUIContent dgOffsetXSettingsContent = new GUIContent(" Offset X", "The Display Gauge's normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.");
        private readonly static GUIContent dgOffsetYSettingsContent = new GUIContent(" Offset Y", "The Display Gauge's normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.");
        private readonly static GUIContent dgDisplayWidthSettingsContent = new GUIContent(" Display Width", "The Display Gauge's normalised width. 1.0 is full screen width, 0.5 is half width.");
        private readonly static GUIContent dgDisplayHeightSettingsContent = new GUIContent(" Display Height", "The Display Gauge's normalised height. 1.0 is full screen height, 0.5 is half height.");
        private readonly static GUIContent dgIsColourAffectedByValueSettingsContent = new GUIContent(" Value Affects Colour", "Does the colour of the foreground change based on the value of the gauge?");
        private readonly static GUIContent dgForegroundColourSettingsContent = new GUIContent(" Foreground Colour", "Colour of the Gauge foreground");
        private readonly static GUIContent dgForegroundHighColourSettingsContent = new GUIContent(" Foreground Hi Colour", "Colour of the Gauge foreground when value is 1.0");
        private readonly static GUIContent dgForegroundMediumColourSettingsContent = new GUIContent(" Foreground Med Colour", "Colour of the Gauge foreground when value is 0.5");
        private readonly static GUIContent dgForegroundLowColourSettingsContent = new GUIContent(" Foreground Low Colour", "Colour of the Gauge foreground when value is 0.0");
        private readonly static GUIContent dgForegroundSpriteSettingsContent = new GUIContent(" Foreground Sprite", "The sprite (texture) for the foreground of the gauge");
        private readonly static GUIContent dgBackgroundColourSettingsContent = new GUIContent(" Background Colour", "Colour of the Gauge background");
        private readonly static GUIContent dgBackgroundSpriteSettingsContent = new GUIContent(" Background Sprite", "The sprite (texture) for the background of the gauge");
        private readonly static GUIContent dgFillMethodSettingsContent = new GUIContent(" Fill Method", "Determines the method used to fill the gauge background sprite when the gaugeValue is modified");
        private readonly static GUIContent dgIsKeepAspectRatioSettingsContent = new GUIContent(" Keep Aspect Ratio", "Keep the original aspect ratio of the foreground and background sprites. Useful when creating circular gauges.");
        private readonly static GUIContent dgTextColourSettingsContent = new GUIContent(" Text Colour", "Colour of the Message text");
        private readonly static GUIContent dgTextAlignmentSettingsContent = new GUIContent(" Text Alignment", "The position of the text within the Display Gauge panel");
        private readonly static GUIContent dgTextDirectionSettingsContent = new GUIContent(" Text Rotation", "The rotation of the text within the Display Gauge panel");
        private readonly static GUIContent dgTextFontStyleSettingsContent = new GUIContent(" Font Style", "The font style of the text within the Display Gauge panel");
        private readonly static GUIContent dgTextIsBestFitSettingsContent = new GUIContent(" Is Best Fit", "Is the text font size automatically changes within the bounds of Font Min Size and Font Max Size to fill the panel?");
        private readonly static GUIContent dgTextFontMinSizeSettingsContent = new GUIContent(" Font Min Size", "When Is Best Fit is true will use this minimum font size if required");
        private readonly static GUIContent dgTextFontMaxSizeSettingsContent = new GUIContent(" Font Max Size", "The font size. If isBestFit is true, this will be the maximum font size it can use.");
        #endregion

        #region GUIContent - Messages
        private readonly static GUIContent displayMessageSettingsContent = new GUIContent("Display Message Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent displayMessageHeaderContent = new GUIContent("Messages are stacking or overlayed in the order they appear in the list.");
        private readonly static GUIContent dmMessageNameSettingsContent = new GUIContent(" Message Name", "The name or description of the message. This can be used to identify the message.");
        private readonly static GUIContent dmMessageStringSettingsContent = new GUIContent(" Message Text", "The text to display in the message. It can include RichText markup. e.g. <b>Bold Text</b>");
        private readonly static GUIContent dmShowMessageSettingsContent = new GUIContent(" Show Message", "Show the message on the display. [Has no effect outside play mode]");
        private readonly static GUIContent dmOffsetXSettingsContent = new GUIContent(" Offset X", "The Display Message's normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.");
        private readonly static GUIContent dmOffsetYSettingsContent = new GUIContent(" Offset Y", "The Display Message's normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.");
        private readonly static GUIContent dmDisplayWidthSettingsContent = new GUIContent(" Display Width", "The Display Message's normalised width. 1.0 is full screen width, 0.5 is half width.");
        private readonly static GUIContent dmDisplayHeightSettingsContent = new GUIContent(" Display Height", "The Display Message's normalised height. 1.0 is full screen height, 0.5 is half height.");
        private readonly static GUIContent dmShowBackgroundSettingsContent = new GUIContent(" Show Background", "Show the message background");
        private readonly static GUIContent dmBackgroundColourSettingsContent = new GUIContent(" Background Colour", "Colour of the Message background");
        private readonly static GUIContent dmTextColourSettingsContent = new GUIContent(" Text Colour", "Colour of the Message text");
        private readonly static GUIContent dmTextAlignmentSettingsContent = new GUIContent(" Text Alignment", "The position of the text within the Display Message panel");
        private readonly static GUIContent dmTextIsBestFitSettingsContent = new GUIContent(" Is Best Fit", "Is the text font size automatically changes within the bounds of Font Min Size and Font Max Size to fill the panel?");
        private readonly static GUIContent dmTextFontMinSizeSettingsContent = new GUIContent(" Font Min Size", "When Is Best Fit is true will use this minimum font size if required");
        private readonly static GUIContent dmTextFontMaxSizeSettingsContent = new GUIContent(" Font Max Size", "The font size. If isBestFit is true, this will be the maximum font size it can use.");
        private readonly static GUIContent dmScrollDirectionSettingsContent = new GUIContent(" Scroll Direction", "The direction (if any) the text should scroll across the screen");
        private readonly static GUIContent dmScrollSpeedSettingsContent = new GUIContent(" Scroll Speed", "Speed or rate at which the text will scroll across the display");
        private readonly static GUIContent dmIsScrollFullscreenSettingsContent = new GUIContent(" Is Scroll Fullscreen", "Scroll full screen regardless of message width and height");

        #endregion

        #region GUIContent - Targets
        private readonly static GUIContent dtgtSettingsContent = new GUIContent("Display Target Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent dtgtShowTargetsOutlineInSceneContent = new GUIContent(" Show Viewport Outline", "Show the rendering limits as a red outline in the scene view [Has no effect in play mode]. Click Refresh if screen has been resized.");
        private readonly static GUIContent dtgtTargetsViewWidthContent = new GUIContent(" Viewport Width", "The width of the clipped area in which Targets are visible. 1.0 is full width, 0.5 is half width.");
        private readonly static GUIContent dtgtTargetsViewHeightContent = new GUIContent(" Viewport Height", "The height of the clipped area in which Targets are visible. 1.0 is full height, 0.5 is half height.");
        private readonly static GUIContent dtgtTargetsViewOffsetXContent = new GUIContent(" Viewport Offset X", "The X offset from centre of the screen for the viewport");
        private readonly static GUIContent dtgtTargetsViewOffsetYContent = new GUIContent(" Viewport Offset Y", "The Y offset from centre of the screen for the viewport");
        private readonly static GUIContent dtgtTargetingRangeContent = new GUIContent(" Targeting Range", "The maximum distance in metres that targets can be away from the character");
        private readonly static GUIContent dtgtHeaderContent = new GUIContent("Targets use Reticles from the Display Reticles section above. Targets are stacking or overlayed in the order they appear in the list below.");
        private readonly static GUIContent dtgtDisplayReticleContent = new GUIContent(" Display Reticle", "The Display Reticle to use for this Target. To add more Reticles, see the list in the Display Reticle Settings section above.");
        private readonly static GUIContent dtgtShowTargetContent = new GUIContent(" Show Target", "Show the Target on the HUD. [Has no effect outside play mode]");
        private readonly static GUIContent dtgtReticleColourContent = new GUIContent(" Reticle Colour", "The colour of the Display Target");
        private readonly static GUIContent dtgtMaxNumberOfTargetsContent = new GUIContent(" Max Number of Targets", "The maximum number of these DisplayTargets that can be shown on the HUD at any one time");

        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent("Debug Mode", "Use this to troubleshoot the data at runtime in the editor.");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent("Is Initialised?");
        private readonly static GUIContent debugRefResolutionContent = new GUIContent("Reference Resolution");
        private readonly static GUIContent debugCanvasResolutionContent = new GUIContent("Current Resolution");
        #endregion

        #region Serialized Properties
        // general
        private SerializedProperty showGeneralSettingsInEditorProp;
        private SerializedProperty showReticleSettingsInEditorProp;
        private SerializedProperty showGaugeSettingsInEditorProp;
        private SerializedProperty showMessageSettingsInEditorProp;
        private SerializedProperty showTargetSettingsInEditorProp;
        private SerializedProperty isShowOnInitialiseProp;
        private SerializedProperty showOverlayProp;
        private SerializedProperty displayWidthProp;
        private SerializedProperty displayHeightProp;
        private SerializedProperty displayOffsetXProp;
        private SerializedProperty displayOffsetYProp;
        private SerializedProperty primaryColourProp;
        private SerializedProperty brightnessProp;
        private SerializedProperty autoHideCursorProp;
        private SerializedProperty canvasSortOrderProp;
        private SerializedProperty showDisplayOutlineInSceneProp;

        // reticles
        private SerializedProperty displayReticleListProp;
        private SerializedProperty displayReticleProp;
        private SerializedProperty showActiveDisplayReticleProp;
        private SerializedProperty lockDisplayReticleToCursorProp;
        private SerializedProperty guidHashActiveDisplayReticleProp;
        private SerializedProperty displayReticleShowInEditorProp;
        private SerializedProperty isDisplayReticleListExpandedProp;
        private SerializedProperty activeDisplayReticleColourProp;

        // gauges
        private SerializedProperty displayGaugeListProp;
        private SerializedProperty dgauProp;
        private SerializedProperty isDisplayGaugeListExpandedProp;
        private SerializedProperty dgauShowInEditorProp;
        private SerializedProperty dgauShowGaugeProp;
        private SerializedProperty dgauFillMethodProp;
        private SerializedProperty dgauIsKeepAspectRatioProp;
        private SerializedProperty dgauGaugeValueProp;
        private SerializedProperty dgauIsColourAffectedByValueProp;
        private SerializedProperty dgauForegroundColourProp;
        private SerializedProperty dgauForegroundHighColourProp;
        private SerializedProperty dgauForegroundMediumColourProp;
        private SerializedProperty dgauForegroundLowColourProp;
        private SerializedProperty dgauForegroundSpriteProp;
        private SerializedProperty dgauBackgroundColourProp;
        private SerializedProperty dgauBackgroundSpriteProp;
        private SerializedProperty dgauTextColourProp;
        private SerializedProperty dgauGaugeStringProp;
        private SerializedProperty dgauTextAlignmentProp;
        private SerializedProperty dgauTextDirectionProp;
        private SerializedProperty dgauTextFontStyleProp;
        private SerializedProperty dgauDisplayWidthProp;
        private SerializedProperty dgauDisplayHeightProp;
        private SerializedProperty dgauOffsetXProp;
        private SerializedProperty dgauOffsetYProp;
        private SerializedProperty dgauIsBestFitProp;
        private SerializedProperty dgauFontMinSizeProp;
        private SerializedProperty dgauFontMaxSizeProp;

        // messages
        private SerializedProperty displayMessageListProp;
        private SerializedProperty dmsgProp;
        private SerializedProperty isDisplayMessageListExpandedProp;
        private SerializedProperty dmsgShowInEditorProp;
        private SerializedProperty dmsgShowMessageProp;
        private SerializedProperty dmsgShowBackgroundProp;
        private SerializedProperty dmsgBackgroundColourProp;
        private SerializedProperty dmsgTextColourProp;
        private SerializedProperty dmsgMessageStringProp;
        private SerializedProperty dmsgTextAlignmentProp;
        private SerializedProperty dmsgDisplayWidthProp;
        private SerializedProperty dmsgDisplayHeightProp;
        private SerializedProperty dmsgOffsetXProp;
        private SerializedProperty dmsgOffsetYProp;
        private SerializedProperty dmsgIsBestFitProp;
        private SerializedProperty dmsgFontMinSizeProp;
        private SerializedProperty dmsgFontMaxSizeProp;
        private SerializedProperty dmsgScrollDirectionProp;
        private SerializedProperty dmsgScrollSpeedProp;
        private SerializedProperty dmsgIsScrollFullscreenProp;

        // targets
        private SerializedProperty displayTargetListProp;
        private SerializedProperty dtgtProp;
        private SerializedProperty isDisplayTargetListExpandedProp;
        private SerializedProperty dtgtShowInEditorProp;
        private SerializedProperty dtgtShowTargetProp;
        private SerializedProperty dtgtReticleColourProp;
        private SerializedProperty dtgtGuidHashDisplayReticleProp;
        private SerializedProperty dtgtShowTargetsOutlineInSceneProp;
        private SerializedProperty dtgtTargetsViewWidthProp;
        private SerializedProperty dtgtTargetsViewHeightProp;
        private SerializedProperty dtgtTargetsViewOffsetXProp;
        private SerializedProperty dtgtTargetsViewOffsetYProp;
        private SerializedProperty dtgtTargetingRangeProp;
        private SerializedProperty dtgtMaxNumberOfTargetsProp;

        #endregion

        #region Events

        public void OnEnable()
        {
            stickyDisplayModule = (StickyDisplayModule)target;

            stickyDisplayModule.ValidateReticleList();
            stickyDisplayModule.ValidateMessageList();
            stickyDisplayModule.ValidateTargetList();

            #region Find Properties
            showGeneralSettingsInEditorProp = serializedObject.FindProperty("showGeneralSettingsInEditor");
            showReticleSettingsInEditorProp = serializedObject.FindProperty("showReticleSettingsInEditor");
            showGaugeSettingsInEditorProp = serializedObject.FindProperty("showGaugeSettingsInEditor");
            showMessageSettingsInEditorProp = serializedObject.FindProperty("showMessageSettingsInEditor");
            showTargetSettingsInEditorProp = serializedObject.FindProperty("showTargetSettingsInEditor");
            isShowOnInitialiseProp = serializedObject.FindProperty("isShowOnInitialise");
            showOverlayProp = serializedObject.FindProperty("showOverlay");
            displayWidthProp = serializedObject.FindProperty("displayWidth");
            displayHeightProp = serializedObject.FindProperty("displayHeight");
            displayOffsetXProp = serializedObject.FindProperty("displayOffsetX");
            displayOffsetYProp = serializedObject.FindProperty("displayOffsetY");
            primaryColourProp = serializedObject.FindProperty("primaryColour");
            brightnessProp = serializedObject.FindProperty("brightness");
            autoHideCursorProp = serializedObject.FindProperty("autoHideCursor");
            canvasSortOrderProp = serializedObject.FindProperty("canvasSortOrder");
            showDisplayOutlineInSceneProp = serializedObject.FindProperty("showDisplayOutlineInScene");
            showActiveDisplayReticleProp = serializedObject.FindProperty("showActiveDisplayReticle");
            lockDisplayReticleToCursorProp = serializedObject.FindProperty("lockDisplayReticleToCursor");
            guidHashActiveDisplayReticleProp = serializedObject.FindProperty("guidHashActiveDisplayReticle");
            displayReticleListProp = serializedObject.FindProperty("displayReticleList");
            activeDisplayReticleColourProp = serializedObject.FindProperty("activeDisplayReticleColour");

            displayGaugeListProp = serializedObject.FindProperty("displayGaugeList");
            displayMessageListProp = serializedObject.FindProperty("displayMessageList");

            displayTargetListProp = serializedObject.FindProperty("displayTargetList");
            dtgtShowTargetsOutlineInSceneProp = serializedObject.FindProperty("showTargetsOutlineInScene");
            dtgtTargetsViewWidthProp = serializedObject.FindProperty("targetsViewWidth");
            dtgtTargetsViewHeightProp = serializedObject.FindProperty("targetsViewHeight");
            dtgtTargetsViewOffsetXProp = serializedObject.FindProperty("targetsViewOffsetX");
            dtgtTargetsViewOffsetYProp = serializedObject.FindProperty("targetsViewOffsetY");
            dtgtTargetingRangeProp = serializedObject.FindProperty("targetingRange");
            #endregion

            // Used in Richtext labels
            if (EditorGUIUtility.isProSkin) { txtColourName = "White"; defaultTextColour = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f); }

            // Keep compiler happy - can remove this later if it isn't required
            if (defaultTextColour.a > 0f) { }
            if (string.IsNullOrEmpty(txtColourName)) { }

            defaultEditorLabelWidth = 175f; // EditorGUIUtility.labelWidth;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            // Reset GUIStyles
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            foldoutStyleNoLabel = null;

            s3dHelpPDF = StickyEditorHelper.GetHelpURL();

            if (!Application.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                stickyDisplayModule.InitialiseEditorEssentials();

                // Showing the outlines at runtime doesn't provide any additional benefit.
                // The only benefit would be if it could be rendered in the Gameview
                // Only use if require scene view interaction
                SceneView.duringSceneGui -= SceneGUI;
                SceneView.duringSceneGui += SceneGUI;

                // +1.0.9 - always load the current display reticle
                stickyDisplayModule.ChangeDisplayReticle(guidHashActiveDisplayReticleProp.intValue);

                hudPanelOutlineColour.a = 0.7f;
                targetsAreaOutlineColour.a = 0.7f;
                stickyDisplayModule.RefreshDisplay();
            }
        }

        /// <summary>
        /// Called when the gameobject loses focus or Unity Editor enters/exits
        /// play mode
        /// </summary>
        void OnDestroy()
        {
            SceneView.duringSceneGui -= SceneGUI;
            
            // Always unhide Unity tools when losing focus on this gameObject
            Tools.hidden = false;
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
            if (stickyDisplayModule.allowRepaint) { Repaint(); }
        }

        /// <summary>
        /// Draw gizmos and editable handles in the scene view
        /// </summary>
        /// <param name="sv"></param>
        private void SceneGUI(SceneView sv)
        {
            if (stickyDisplayModule != null && stickyDisplayModule.gameObject.activeInHierarchy)
            {
                
                if (stickyDisplayModule.showDisplayOutlineInScene || stickyDisplayModule.showTargetsOutlineInScene)
                {
                    float _canvasWidth = stickyDisplayModule.CanvasResolution.x * stickyDisplayModule.CanvasScaleFactor.x;
                    float _canvasHeight = stickyDisplayModule.CanvasResolution.y * stickyDisplayModule.CanvasScaleFactor.y;

                    // Show yellow bounding box of current display size
                    if (stickyDisplayModule.showDisplayOutlineInScene)
                    {
                        float _width = _canvasWidth * stickyDisplayModule.displayWidth;
                        float _height = _canvasHeight * stickyDisplayModule.displayHeight;

                        float _offsetX = stickyDisplayModule.displayOffsetX * _canvasWidth;
                        float _offsetY = stickyDisplayModule.displayOffsetY * _canvasHeight;

                        Vector3 bottomLeft = new Vector3(_canvasWidth * 0.5f - (_width * 0.5f) + _offsetX, _canvasHeight * 0.5f - (_height * 0.5f) + _offsetY, 0f);
                        Vector3 bottomRight = new Vector3(_canvasWidth * 0.5f + (_width * 0.5f) + _offsetX, _canvasHeight * 0.5f - (_height * 0.5f) + _offsetY, 0f);
                        Vector3 topLeft = new Vector3(_canvasWidth * 0.5f - (_width * 0.5f) + _offsetX, _canvasHeight * 0.5f + (_height * 0.5f) + _offsetY, 0f);
                        Vector3 topRight = new Vector3(_canvasWidth * 0.5f + (_width * 0.5f) + _offsetX, _canvasHeight * 0.5f + (_height * 0.5f) + _offsetY, 0f);

                        using (new Handles.DrawingScope(hudPanelOutlineColour))
                        {
                            Handles.DrawAAPolyLine(4f, bottomLeft, bottomRight, topRight, topLeft, bottomLeft);
                        }
                    }

                    // Show red bounding box of current Targets clipped visible area
                    if (stickyDisplayModule.showTargetsOutlineInScene)
                    {
                        float _width = _canvasWidth * stickyDisplayModule.targetsViewWidth;
                        float _height = _canvasHeight * stickyDisplayModule.targetsViewHeight;

                        float _offsetX = stickyDisplayModule.targetsViewOffsetX * _canvasWidth;
                        float _offsetY = stickyDisplayModule.targetsViewOffsetY * _canvasHeight;

                        Vector3 bottomLeft = new Vector3(_canvasWidth * 0.5f - (_width * 0.5f) + _offsetX, _canvasHeight * 0.5f - (_height * 0.5f) + _offsetY, 0f);
                        Vector3 bottomRight = new Vector3(_canvasWidth * 0.5f + (_width * 0.5f) + _offsetX, _canvasHeight * 0.5f - (_height * 0.5f) + _offsetY, 0f);
                        Vector3 topLeft = new Vector3(_canvasWidth * 0.5f - (_width * 0.5f) + _offsetX, _canvasHeight * 0.5f + (_height * 0.5f) + _offsetY, 0f);
                        Vector3 topRight = new Vector3(_canvasWidth * 0.5f + (_width * 0.5f) + _offsetX, _canvasHeight * 0.5f + (_height * 0.5f) + _offsetY, 0f);

                        using (new Handles.DrawingScope(targetsAreaOutlineColour))
                        {
                            Handles.DrawAAPolyLine(3f, bottomLeft, bottomRight, topRight, topLeft, bottomLeft);
                        }
                    }
                }
            }
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

                if (compType == typeof(S3DDisplayReticle))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as S3DDisplayReticle).showInEditor = isExpanded;
                    }
                }
                else if (compType == typeof(S3DDisplayMessage))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as S3DDisplayMessage).showInEditor = isExpanded;
                    }
                }
                else if (compType == typeof(S3DDisplayTarget))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as S3DDisplayTarget).showInEditor = isExpanded;
                    }
                }
            }
        }

        /// <summary>
        /// Dropdown menu callback method used when a Display Reticle is selected
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateActiveDisplayReticle(object obj)
        {
            // The menu data is passed as Vector2Int. But it could be passed as say a Vector3Int
            // if more data is required. For this we only need the first int.
            if (obj != null && obj.GetType() == typeof(Vector2Int))
            {
                Vector2Int objData = (Vector2Int)obj;

                Undo.RecordObject(stickyDisplayModule, "Set Active Reticle");
                stickyDisplayModule.guidHashActiveDisplayReticle = objData.x;

                // Update at runtime or in the editor
                if (Application.isPlaying || stickyDisplayModule.IsEditorMode)
                {
                    //Debug.Log("[DEBUG] change to guidHash: " + objData.x);

                    stickyDisplayModule.ChangeDisplayReticle(objData.x);
                }
            }
        }

        /// <summary>
        /// Apply any outstanding property changes.
        /// Start and undo group
        /// Record the current state of stickyControlModule properties.
        /// PURPOSE: To assist with correctly recording undo events
        /// in the editor (not in play mode). In stickyControlModule,
        /// Undo.Record(transform, string.empty) will record the correct
        /// tranform that needs to be undone (this bit isn't currently working correctly)
        /// </summary>
        /// <param name="changeText"></param>
        /// <returns></returns>
        private int ApplyAndRecord(string changeText)
        {
            int _undoGroup = 0;
            if (stickyDisplayModule.IsEditorMode)
            {
                serializedObject.ApplyModifiedProperties();
                Undo.SetCurrentGroupName(changeText);
                _undoGroup = UnityEditor.Undo.GetCurrentGroup();
                Undo.RecordObject(stickyDisplayModule, string.Empty);
            }
            else
            {
                // At runtime, simply apply any changes
                serializedObject.ApplyModifiedProperties();
            }
            return _undoGroup;
        }

        /// <summary>
        /// Finish the undo group.
        /// Read the current stickyControlModule properties.
        /// </summary>
        /// <param name="undoGroup"></param>
        private void ReadProps(int undoGroup)
        {
            if (stickyDisplayModule.IsEditorMode)
            {
                Undo.CollapseUndoOperations(undoGroup);
                serializedObject.Update();
            }
            else
            {
                // At runtime, simply read back the properties
            }
        }

        /// <summary>
        /// Dropdown menu callback method used when a Display Target reticle is selected
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateDisplayTargetReticle(object obj)
        {
            // The menu data is passed as Vector2Int. But it could be passed as say a Vector3Int
            // if more data is required.
            // Vector2Int.x is DisplayTarget guidHash
            // Vector2Int.y is guidHash of the DisplayRectile from displayReticleList.
            if (obj != null && obj.GetType() == typeof(Vector2Int))
            {
                Vector2Int objData = (Vector2Int)obj;

                Undo.RecordObject(stickyDisplayModule, "Set Display Target Reticle");

                S3DDisplayTarget displayTarget = stickyDisplayModule.GetDisplayTarget(objData.x);
                if (displayTarget != null)
                {
                    displayTarget.guidHashDisplayReticle = objData.y;

                    // Update at runtime or in the editor
                    if (Application.isPlaying || stickyDisplayModule.IsEditorMode)
                    {
                        stickyDisplayModule.SetDisplayTargetReticle(objData.x, objData.y);
                    }
                }
            }
        }

        #endregion

        #region OnInspectorGUI

        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there

        public override void OnInspectorGUI()
        {
            #region Initialise

            stickyDisplayModule.allowRepaint = false;
            isSceneModified = false;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;

            #endregion

            #region Configure Buttons and Styles

            // Set up rich text GUIStyles
            helpBoxRichText = new GUIStyle("HelpBox");
            helpBoxRichText.richText = true;

            labelFieldRichText = new GUIStyle("Label");
            labelFieldRichText.richText = true;

            buttonCompact = new GUIStyle("Button");
            buttonCompact.fontSize = 10;

            // Set up the toggle buttons styles
            if (toggleCompactButtonStyleNormal == null)
            {
                // Create a new button or else will effect the Button style for other buttons too
                toggleCompactButtonStyleNormal = new GUIStyle("Button");
                toggleCompactButtonStyleToggled = new GUIStyle(toggleCompactButtonStyleNormal);
                toggleCompactButtonStyleNormal.fontStyle = FontStyle.Normal;
                toggleCompactButtonStyleToggled.fontStyle = FontStyle.Bold;
                toggleCompactButtonStyleToggled.normal.background = toggleCompactButtonStyleToggled.active.background;
            }

            if (foldoutStyleNoLabel == null)
            {
                // When using a no-label foldout, don't forget to set the global
                // EditorGUIUtility.fieldWidth to a small value like 15, then back
                // to the original afterward.
                foldoutStyleNoLabel = new GUIStyle(EditorStyles.foldout);
                foldoutStyleNoLabel.fixedWidth = 0.01f;
            }

            #endregion

            // Read in all the properties
            serializedObject.Update();

            #region Header Info and Buttons
            StickyEditorHelper.DrawStickyVersionLabel(labelFieldRichText);

            EditorGUILayout.LabelField(headerContent, helpBoxRichText);

            StickyEditorHelper.DrawGetHelpButtons(buttonCompact);
            #endregion

            #region General Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawS3DFoldout(showGeneralSettingsInEditorProp, generalSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showGeneralSettingsInEditorProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("initialiseOnStart"), initialiseOnStartContent);
                EditorGUILayout.PropertyField(isShowOnInitialiseProp, isShowOnInitialiseContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(showOverlayProp, showOverlayContent);
                // Always visible in outside of play mode
                if (EditorGUI.EndChangeCheck() && !stickyDisplayModule.IsEditorMode && Application.isPlaying)
                {
                    serializedObject.ApplyModifiedProperties();
                    if (showOverlayProp.boolValue) { stickyDisplayModule.ShowOverlay(); }
                    else { stickyDisplayModule.HideOverlay(); }
                    serializedObject.Update();
                }

                EditorGUILayout.PropertyField(autoHideCursorProp, autoHideCursorContent);
                if (autoHideCursorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("hideCursorTime"), hideCursorTimeContent);
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("mainCamera"), mainCameraContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(displayWidthProp, displayWidthContent);
                EditorGUILayout.PropertyField(displayHeightProp, displayHeightContent);
                if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                {
                    stickyDisplayModule.SetDisplaySize(displayWidthProp.floatValue, displayHeightProp.floatValue);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(displayOffsetXProp, displayOffsetXContent);
                EditorGUILayout.PropertyField(displayOffsetYProp, displayOffsetYContent);
                if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                {
                    stickyDisplayModule.SetDisplayOffset(displayOffsetXProp.floatValue, displayOffsetYProp.floatValue);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(primaryColourProp, primaryColourContent);
                if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    int _undoGroup = ApplyAndRecord("Display Set Primary Colour");
                    stickyDisplayModule.SetPrimaryColour(primaryColourProp.colorValue);
                    ReadProps(_undoGroup);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(brightnessProp, brightnessContent);
                if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    stickyDisplayModule.SetBrightness(brightnessProp.floatValue);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(canvasSortOrderProp, canvasSortOrderContent);
                if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    int _undoGroup = ApplyAndRecord("Display sort order");
                    stickyDisplayModule.SetCanvasSortOrder(canvasSortOrderProp.intValue);
                    ReadProps(_undoGroup);
                }

                EditorGUILayout.PropertyField(showDisplayOutlineInSceneProp, showDisplayOutlineInSceneContent);
            }

            EditorGUILayout.EndVertical();

            #endregion

            #region Display Reticle Settings

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawS3DFoldout(showReticleSettingsInEditorProp, reticleSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showReticleSettingsInEditorProp.boolValue)
            {
                int numDisplayReticles = displayReticleListProp.arraySize;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(showActiveDisplayReticleProp, showActiveDisplayReticleContent);
                // Always visible outside play mode
                if (EditorGUI.EndChangeCheck() && !stickyDisplayModule.IsEditorMode && Application.isPlaying)
                {
                    serializedObject.ApplyModifiedProperties();
                    if (showActiveDisplayReticleProp.boolValue) { stickyDisplayModule.ShowDisplayReticle(); }
                    else { stickyDisplayModule.HideDisplayReticle(); }
                    serializedObject.Update();
                }

                #region Current Reticle
                int selectedIdx = stickyDisplayModule.displayReticleList.FindIndex(dr => dr.guidHash == guidHashActiveDisplayReticleProp.intValue);
                GUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(activeDisplayReticleContent, GUILayout.Width(defaultEditorLabelWidth - 26f));

                if (GUILayout.Button("..", buttonCompact, GUILayout.MaxWidth(20f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();

                    // Create a drop down list of all the reticles
                    GenericMenu dropdown = new GenericMenu();
                    dropdown.AddItem(new GUIContent("None"), selectedIdx < 0, UpdateActiveDisplayReticle, new Vector2Int(0, 0));

                    for (int i = 0; i < numDisplayReticles; i++)
                    {
                        // Replace space #/%/& with different chars as Unity treats them as SHIFT/CTRL/ALT in menus.
                        tempDisplayReticleName = stickyDisplayModule.displayReticleList[i].primarySprite == null ? "No Texture" : stickyDisplayModule.displayReticleList[i].primarySprite.name.Replace(" #", "_#").Replace(" &", " &&").Replace(" %", "_%");
                        dropdown.AddItem(new GUIContent(tempDisplayReticleName), i == selectedIdx, UpdateActiveDisplayReticle, new Vector2Int(stickyDisplayModule.displayReticleList[i].guidHash, 0));
                    }
                    dropdown.ShowAsContext();
                    SceneView.RepaintAll();

                    serializedObject.Update();
                }
                if (selectedIdx < 0 || selectedIdx > numDisplayReticles - 1)
                {
                    EditorGUILayout.LabelField("None", GUILayout.MaxWidth(100f));                    
                }
                else
                {
                    tempDisplayReticleName = stickyDisplayModule.displayReticleList[selectedIdx].primarySprite == null ? "no texture in reticle" : stickyDisplayModule.displayReticleList[selectedIdx].primarySprite.name;
                    EditorGUILayout.LabelField(string.IsNullOrEmpty(tempDisplayReticleName) ? "no texture name" : tempDisplayReticleName, GUILayout.MaxWidth(100f));
                }
                GUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("displayReticleOffsetX"), displayReticleOffsetXContent);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("displayReticleOffsetY"), displayReticleOffsetYContent);
                if (lockDisplayReticleToCursorProp.boolValue && autoHideCursorProp.boolValue)
                {
                    EditorGUILayout.HelpBox(autoHideLockReticleWarning, MessageType.Warning);
                }
                EditorGUILayout.PropertyField(lockDisplayReticleToCursorProp, lockDisplayReticleToCursorContent);
                if (EditorGUI.EndChangeCheck() && !lockDisplayReticleToCursorProp.boolValue && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    // +SMS v1.1.0 - reset reticle to centre when turning it off at runtime
                    stickyDisplayModule.SetDisplayReticleOffset(0f, 0f);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(activeDisplayReticleColourProp, activeDisplayReticleColourContent);
                if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    stickyDisplayModule.SetDisplayReticleColour(activeDisplayReticleColourProp.colorValue);
                }

                #endregion

                #region Display Reticles       

                // There should always be at least 1
                if (numDisplayReticles == 0)
                {
                    // For some reason, increasing the array size doesn't call the
                    // constructor. So set the defaults in code.
                    displayReticleListProp.arraySize = 1;
                    serializedObject.ApplyModifiedProperties();
                    stickyDisplayModule.displayReticleList[0].SetClassDefaults();
                    isSceneModified = true;
                    serializedObject.Update();
                }

                #region Add-Remove Display Reticles
                // Reset button variables
                displayReticleMoveDownPos = -1;
                displayReticleInsertPos = -1;
                displayReticleDeletePos = -1;

                GUILayout.BeginHorizontal();

                EditorGUI.indentLevel += 1;
                EditorGUIUtility.fieldWidth = 15f;
                isDisplayReticleListExpandedProp = serializedObject.FindProperty("isDisplayReticleListExpanded");
                EditorGUI.BeginChangeCheck();
                isDisplayReticleListExpandedProp.boolValue = EditorGUILayout.Foldout(isDisplayReticleListExpandedProp.boolValue, "", foldoutStyleNoLabel);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(stickyDisplayModule.displayReticleList, isDisplayReticleListExpandedProp.boolValue);
                    // Read in the properties
                    serializedObject.Update();
                }
                EditorGUI.indentLevel -= 1;

                EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                EditorGUILayout.LabelField("Display Reticles: " + numDisplayReticles.ToString("00"), labelFieldRichText);

                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(stickyDisplayModule, "Add Display Reticle");
                    stickyDisplayModule.displayReticleList.Add(new S3DDisplayReticle());
                    isSceneModified = true;
                    // Read in the properties
                    serializedObject.Update();

                    numDisplayReticles = displayReticleListProp.arraySize;
                    if (numDisplayReticles > 0)
                    {
                        // Force new reticle to be serialized in scene
                        displayReticleProp = displayReticleListProp.GetArrayElementAtIndex(numDisplayReticles - 1);
                        displayReticleShowInEditorProp = displayReticleProp.FindPropertyRelative("showInEditor");
                        displayReticleShowInEditorProp.boolValue = !displayReticleShowInEditorProp.boolValue;
                        // Show the new reticle in the editor
                        displayReticleShowInEditorProp.boolValue = true;
                    }
                }
                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (numDisplayReticles > 0) { displayReticleDeletePos = displayReticleListProp.arraySize - 1; }
                }

                GUILayout.EndHorizontal();

                #endregion

                #region Display Reticles List

                numDisplayReticles = displayReticleListProp.arraySize;

                for (int dpIdx = 0; dpIdx < numDisplayReticles; dpIdx++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    displayReticleProp = displayReticleListProp.GetArrayElementAtIndex(dpIdx);

                    displayReticleShowInEditorProp = displayReticleProp.FindPropertyRelative("showInEditor");

                    #region Display Reticle Move/Insert/Delete buttons
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel += 1;
                    displayReticleShowInEditorProp.boolValue = EditorGUILayout.Foldout(displayReticleShowInEditorProp.boolValue, "Display Reticle " + (dpIdx + 1).ToString("00"));
                    EditorGUI.indentLevel -= 1;

                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numDisplayReticles > 1) { displayReticleMoveDownPos = dpIdx; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { displayReticleInsertPos = dpIdx; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { displayReticleDeletePos = dpIdx; }
                    GUILayout.EndHorizontal();
                    #endregion

                    if (displayReticleShowInEditorProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(displayReticleProp.FindPropertyRelative("primarySprite"), drPrimarySpriteContent);

                    }
                    GUILayout.EndVertical();
                }

                #endregion

                #region Move/Insert/Delete Display Reticles
                if (displayReticleDeletePos >= 0 || displayReticleInsertPos >= 0 || displayReticleMoveDownPos >= 0)
                {
                    GUI.FocusControl(null);

                    // Don't permit multiple operations in the same pass
                    if (displayReticleMoveDownPos >= 0)
                    {
                        // Move down one position, or wrap round to start of list
                        if (displayReticleMoveDownPos < displayReticleListProp.arraySize - 1)
                        {
                            displayReticleListProp.MoveArrayElement(displayReticleMoveDownPos, displayReticleMoveDownPos + 1);
                        }
                        else { displayReticleListProp.MoveArrayElement(displayReticleMoveDownPos, 0); }

                        displayReticleMoveDownPos = -1;
                    }
                    else if (displayReticleInsertPos >= 0)
                    {
                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        stickyDisplayModule.displayReticleList.Insert(displayReticleInsertPos, new S3DDisplayReticle(stickyDisplayModule.displayReticleList[displayReticleInsertPos]));

                        // Read all properties from the StickyDisplayModule
                        serializedObject.Update();

                        // Hide original DisplayReticle
                        displayReticleListProp.GetArrayElementAtIndex(displayReticleInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                        displayReticleShowInEditorProp = displayReticleListProp.GetArrayElementAtIndex(displayReticleInsertPos).FindPropertyRelative("showInEditor");
                        // Generate a new hashcode for the duplicated DisplayReticle
                        displayReticleListProp.GetArrayElementAtIndex(displayReticleInsertPos).FindPropertyRelative("guidHash").intValue = S3DMath.GetHashCodeFromGuid();

                        // Force new diplay reticle to be serialized in scene
                        displayReticleShowInEditorProp.boolValue = !displayReticleShowInEditorProp.boolValue;

                        // Show inserted duplicate display reticle
                        displayReticleShowInEditorProp.boolValue = true;

                        displayReticleInsertPos = -1;

                        isSceneModified = true;
                    }
                    else if (displayReticleDeletePos >= 0)
                    {
                        // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and displayReticleDeletePos is reset to -1.
                        int _deleteIndex = displayReticleDeletePos;

                        if (EditorUtility.DisplayDialog("Delete Display Reticle " + (displayReticleDeletePos + 1) + "?", "Display Reticle " + (displayReticleDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the display reticle from the list and cannot be undone.", "Delete Now", "Cancel"))
                        {
                            displayReticleListProp.DeleteArrayElementAtIndex(_deleteIndex);
                            displayReticleDeletePos = -1;
                        }
                    }

                    #if UNITY_2019_3_OR_NEWER
                    serializedObject.ApplyModifiedProperties();
                    // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                    if (!Application.isPlaying)
                    {
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                    GUIUtility.ExitGUI();
                    #endif
                }

                #endregion

                #endregion

            }
            EditorGUILayout.EndVertical();
            #endregion

            #region Message Settings

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawS3DFoldout(showMessageSettingsInEditorProp, displayMessageSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showMessageSettingsInEditorProp.boolValue)
            {
                EditorGUILayout.LabelField(displayMessageHeaderContent, helpBoxRichText);

                int numDisplayMessages = displayMessageListProp.arraySize;

                #region Add-Remove Display Messages
                // Reset button variables
                displayMessageMoveDownPos = -1;
                displayMessageInsertPos = -1;
                displayMessageDeletePos = -1;

                GUILayout.BeginHorizontal();
                isDisplayMessageListExpandedProp = serializedObject.FindProperty("isDisplayMessageListExpanded");
                EditorGUI.BeginChangeCheck();
                StickyEditorHelper.DrawS3DFoldout(isDisplayMessageListExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(stickyDisplayModule.displayMessageList, isDisplayMessageListExpandedProp.boolValue);
                    // Read in the properties
                    serializedObject.Update();
                }

                EditorGUILayout.LabelField("Display Messages: " + numDisplayMessages.ToString("00"), labelFieldRichText);

                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(stickyDisplayModule, "Add Display Message");
                    stickyDisplayModule.AddMessage("New Message","New Message Text");
                    isSceneModified = true;
                    // Read in the properties
                    serializedObject.Update();

                    numDisplayMessages = displayMessageListProp.arraySize;
                    if (numDisplayMessages > 0)
                    {
                        // Force new message to be serialized in scene
                        dmsgProp = displayMessageListProp.GetArrayElementAtIndex(numDisplayMessages - 1);
                        dmsgShowInEditorProp = dmsgProp.FindPropertyRelative("showInEditor");
                        dmsgShowInEditorProp.boolValue = !dmsgShowInEditorProp.boolValue;
                        // Show the new message in the editor
                        dmsgShowInEditorProp.boolValue = true;
                    }
                }

                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (numDisplayMessages > 0) { displayMessageDeletePos = displayMessageListProp.arraySize - 1; }
                }

                GUILayout.EndHorizontal();
                #endregion

                #region Display Message List
                numDisplayMessages = displayMessageListProp.arraySize;

                for (int dmIdx = 0; dmIdx < numDisplayMessages; dmIdx++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    dmsgProp = displayMessageListProp.GetArrayElementAtIndex(dmIdx);

                    #region Find Display Message Properties
                    dmsgShowInEditorProp = dmsgProp.FindPropertyRelative("showInEditor");
                    #endregion

                    #region Display Message Move/Insert/Delete buttons
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel += 1;
                    dmsgShowInEditorProp.boolValue = EditorGUILayout.Foldout(dmsgShowInEditorProp.boolValue, "Display Message " + (dmIdx + 1).ToString("00"));
                    EditorGUI.indentLevel -= 1;

                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numDisplayMessages > 1) { displayMessageMoveDownPos = dmIdx; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { displayMessageInsertPos = dmIdx; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { displayMessageDeletePos = dmIdx; }
                    GUILayout.EndHorizontal();
                    #endregion

                    if (dmsgShowInEditorProp.boolValue)
                    {
                        #region Find Display Message Properties
                        dmsgShowMessageProp = dmsgProp.FindPropertyRelative("showMessage");
                        dmsgOffsetXProp = dmsgProp.FindPropertyRelative("offsetX");
                        dmsgOffsetYProp = dmsgProp.FindPropertyRelative("offsetY");
                        dmsgDisplayWidthProp = dmsgProp.FindPropertyRelative("displayWidth");
                        dmsgDisplayHeightProp = dmsgProp.FindPropertyRelative("displayHeight");
                        dmsgShowBackgroundProp = dmsgProp.FindPropertyRelative("showBackground");
                        dmsgBackgroundColourProp = dmsgProp.FindPropertyRelative("backgroundColour");
                        dmsgTextColourProp = dmsgProp.FindPropertyRelative("textColour");
                        dmsgMessageStringProp = dmsgProp.FindPropertyRelative("messageString");
                        dmsgTextAlignmentProp = dmsgProp.FindPropertyRelative("textAlignment");
                        dmsgIsBestFitProp = dmsgProp.FindPropertyRelative("isBestFit");
                        dmsgFontMinSizeProp = dmsgProp.FindPropertyRelative("fontMinSize");
                        dmsgFontMaxSizeProp = dmsgProp.FindPropertyRelative("fontMaxSize");
                        dmsgScrollDirectionProp = dmsgProp.FindPropertyRelative("scrollDirection");
                        dmsgScrollSpeedProp = dmsgProp.FindPropertyRelative("scrollSpeed");
                        dmsgIsScrollFullscreenProp = dmsgProp.FindPropertyRelative("isScrollFullscreen");
                        #endregion

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgShowMessageProp, dmShowMessageSettingsContent);
                        // Always visible outside play mode
                        if (EditorGUI.EndChangeCheck() && !stickyDisplayModule.IsEditorMode && Application.isPlaying)
                        {
                            serializedObject.ApplyModifiedProperties();
                            if (dmsgShowMessageProp.boolValue) { stickyDisplayModule.ShowDisplayMessage(stickyDisplayModule.displayMessageList[dmIdx]); }
                            else { stickyDisplayModule.HideDisplayMessage(stickyDisplayModule.displayMessageList[dmIdx]); }
                            serializedObject.Update();
                        }

                        EditorGUILayout.PropertyField(dmsgProp.FindPropertyRelative("messageName"), dmMessageNameSettingsContent);
                        
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgMessageStringProp, dmMessageStringSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Message Text");
                            stickyDisplayModule.SetDisplayMessageText(stickyDisplayModule.displayMessageList[dmIdx], dmsgMessageStringProp.stringValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        if (!dmsgIsScrollFullscreenProp.boolValue || (dmsgScrollDirectionProp.intValue != S3DDisplayMessage.ScrollDirectionLR && dmsgScrollDirectionProp.intValue != S3DDisplayMessage.ScrollDirectionRL))
                        {
                            EditorGUILayout.PropertyField(dmsgOffsetXProp, dmOffsetXSettingsContent);
                        }
                        if (!dmsgIsScrollFullscreenProp.boolValue || (dmsgScrollDirectionProp.intValue != S3DDisplayMessage.ScrollDirectionBT && dmsgScrollDirectionProp.intValue != S3DDisplayMessage.ScrollDirectionTB))
                        {
                            EditorGUILayout.PropertyField(dmsgOffsetYProp, dmOffsetYSettingsContent);
                        }
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Message Offset");
                            stickyDisplayModule.SetDisplayMessageOffset(stickyDisplayModule.displayMessageList[dmIdx], dmsgOffsetXProp.floatValue, dmsgOffsetYProp.floatValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgDisplayWidthProp, dmDisplayWidthSettingsContent);
                        EditorGUILayout.PropertyField(dmsgDisplayHeightProp, dmDisplayHeightSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Message Size");
                            stickyDisplayModule.SetDisplayMessageSize(stickyDisplayModule.displayMessageList[dmIdx], dmsgDisplayWidthProp.floatValue, dmsgDisplayHeightProp.floatValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgShowBackgroundProp, dmShowBackgroundSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Message Show Background");
                            if (dmsgShowBackgroundProp.boolValue) { stickyDisplayModule.ShowDisplayMessageBackground(stickyDisplayModule.displayMessageList[dmIdx]); }
                            else  { stickyDisplayModule.HideDisplayMessageBackground(stickyDisplayModule.displayMessageList[dmIdx]); }
                            ReadProps(_undoGroup);
                        }

                        if (dmsgShowBackgroundProp.boolValue)
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(dmsgBackgroundColourProp, dmBackgroundColourSettingsContent);
                            if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                            {
                                int _undoGroup = ApplyAndRecord("Display Message Background Colour");
                                stickyDisplayModule.SetDisplayMessageBackgroundColour(stickyDisplayModule.displayMessageList[dmIdx], dmsgBackgroundColourProp.colorValue);
                                ReadProps(_undoGroup);
                            }
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgTextColourProp, dmTextColourSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Message Text Colour");
                            stickyDisplayModule.SetDisplayMessageTextColour(stickyDisplayModule.displayMessageList[dmIdx], dmsgTextColourProp.colorValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgTextAlignmentProp, dmTextAlignmentSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Message Alignment");
                            stickyDisplayModule.SetDisplayMessageTextAlignment(stickyDisplayModule.displayMessageList[dmIdx], (TextAnchor)dmsgTextAlignmentProp.enumValueIndex);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgIsBestFitProp, dmTextIsBestFitSettingsContent);
                        if (dmsgIsBestFitProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(dmsgFontMinSizeProp, dmTextFontMinSizeSettingsContent);
                        }
                        EditorGUILayout.PropertyField(dmsgFontMaxSizeProp, dmTextFontMaxSizeSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            //int _undoGroup = ApplyAndRecord("Display Message Font Size");
                            stickyDisplayModule.SetDisplayMessageTextFontSize(stickyDisplayModule.displayMessageList[dmIdx], dmsgIsBestFitProp.boolValue, dmsgFontMinSizeProp.intValue, dmsgFontMaxSizeProp.intValue);
                            //ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgScrollDirectionProp, dmScrollDirectionSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            stickyDisplayModule.SetDisplayMessageScrollDirection(stickyDisplayModule.displayMessageList[dmIdx], dmsgScrollDirectionProp.intValue);
                        }

                        if (dmsgScrollDirectionProp.intValue != S3DDisplayMessage.ScrollDirectionNone)
                        {
                            EditorGUILayout.PropertyField(dmsgScrollSpeedProp, dmScrollSpeedSettingsContent);
                            EditorGUILayout.PropertyField(dmsgIsScrollFullscreenProp, dmIsScrollFullscreenSettingsContent);
                        }
                        
                    }
                    GUILayout.EndVertical();
                }

                #endregion

                #region Move/Insert/Delete Display Messages
                if (displayMessageDeletePos >= 0 || displayMessageInsertPos >= 0 || displayMessageMoveDownPos >= 0)
                {
                    GUI.FocusControl(null);

                    // Don't permit multiple operations in the same pass
                    if (displayMessageMoveDownPos >= 0)
                    {
                        // Move down one position, or wrap round to start of list
                        if (displayMessageMoveDownPos < displayMessageListProp.arraySize - 1)
                        {
                            displayMessageListProp.MoveArrayElement(displayMessageMoveDownPos, displayMessageMoveDownPos + 1);
                        }
                        else { displayMessageListProp.MoveArrayElement(displayMessageMoveDownPos, 0); }

                        serializedObject.ApplyModifiedProperties();
                        // For some reason this fully executes but doesn't update hierarchy order... (but works elsewhere)
                        stickyDisplayModule.RefreshMessagesSortOrder();
                        serializedObject.Update();                       

                        displayMessageMoveDownPos = -1;
                    }
                    else if (displayMessageInsertPos >= 0)
                    {
                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        Undo.SetCurrentGroupName("Insert Display Message");
                        int _undoGroup = UnityEditor.Undo.GetCurrentGroup();
                        Undo.RecordObject(stickyDisplayModule, string.Empty);

                        S3DDisplayMessage insertedMessage = new S3DDisplayMessage(stickyDisplayModule.displayMessageList[displayMessageInsertPos]);
                        insertedMessage.showInEditor = true;
                        // Generate a new hashcode for the duplicated DisplayMessage
                        insertedMessage.guidHash = S3DMath.GetHashCodeFromGuid();

                        stickyDisplayModule.displayMessageList.Insert(displayMessageInsertPos, insertedMessage);
                        // Hide original DisplayMessage
                        stickyDisplayModule.displayMessageList[displayMessageInsertPos + 1].showInEditor = false;
                        RectTransform insertedMessagePanel = stickyDisplayModule.CreateMessagePanel(insertedMessage, stickyDisplayModule.GetDisplayPanel());
                        if (insertedMessagePanel != null)
                        {
                            // Make sure we can rollback the creation of the new message panel
                            Undo.RegisterCreatedObjectUndo(insertedMessagePanel.gameObject, string.Empty);
                            stickyDisplayModule.RefreshMessagesSortOrder();
                        }

                        Undo.CollapseUndoOperations(_undoGroup);

                        // Read all properties from the StickyDisplayModule
                        serializedObject.Update();

                        displayMessageInsertPos = -1;

                        isSceneModified = true;
                    }
                    else if (displayMessageDeletePos >= 0)
                    {
                        // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and displayMessageDeletePos is reset to -1.
                        int _deleteIndex = displayMessageDeletePos;
                        serializedObject.ApplyModifiedProperties();

                        if (EditorUtility.DisplayDialog("Delete Display Message " + (displayMessageDeletePos + 1) + "?", "Display Message " + (displayMessageDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the display message from the list.", "Delete Now", "Cancel"))
                        {
                            int _guidHash = displayMessageListProp.GetArrayElementAtIndex(_deleteIndex).FindPropertyRelative("guidHash").intValue;

                            stickyDisplayModule.DeleteMessage(_guidHash);

                            displayMessageDeletePos = -1;

                            serializedObject.Update();

                            // Force stickyDisplayModule to be serialized after a delete
                            showGeneralSettingsInEditorProp.boolValue = !showGeneralSettingsInEditorProp.boolValue;
                            showGeneralSettingsInEditorProp.boolValue = !showGeneralSettingsInEditorProp.boolValue;
                        }
                        else { serializedObject.Update(); }
                    }

                    #if UNITY_2019_3_OR_NEWER
                    serializedObject.ApplyModifiedProperties();
                    // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                    if (!Application.isPlaying)
                    {
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                    GUIUtility.ExitGUI();
                    #endif
                }

            #endregion
            }

            EditorGUILayout.EndVertical();

            #endregion

            #region Display Gauge Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawS3DFoldout(showGaugeSettingsInEditorProp, displayGaugeSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showGaugeSettingsInEditorProp.boolValue)
            {
                EditorGUILayout.LabelField(displayGaugeHeaderContent, helpBoxRichText);

                int numDisplayGauges = displayGaugeListProp.arraySize;

                #region Add-Remove Display Gauges
                // Reset button variables
                displayGaugeMoveDownPos = -1;
                displayGaugeInsertPos = -1;
                displayGaugeDeletePos = -1;

                GUILayout.BeginHorizontal();
                isDisplayGaugeListExpandedProp = serializedObject.FindProperty("isDisplayGaugeListExpanded");
                EditorGUI.BeginChangeCheck();
                StickyEditorHelper.DrawS3DFoldout(isDisplayGaugeListExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(stickyDisplayModule.displayGaugeList, isDisplayGaugeListExpandedProp.boolValue);
                    // Read in the properties
                    serializedObject.Update();
                }

                EditorGUILayout.LabelField("Display Gauges: " + numDisplayGauges.ToString("00"), labelFieldRichText);

                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(stickyDisplayModule, "Add Display Gauge");
                    stickyDisplayModule.AddGauge("New Gauge","New Gauge Text");
                    isSceneModified = true;
                    // Read in the properties
                    serializedObject.Update();

                    numDisplayGauges = displayGaugeListProp.arraySize;
                    if (numDisplayGauges > 0)
                    {
                        // Force new gauge to be serialized in scene
                        dmsgProp = displayGaugeListProp.GetArrayElementAtIndex(numDisplayGauges - 1);
                        dmsgShowInEditorProp = dmsgProp.FindPropertyRelative("showInEditor");
                        dmsgShowInEditorProp.boolValue = !dmsgShowInEditorProp.boolValue;
                        // Show the new gauge in the editor
                        dmsgShowInEditorProp.boolValue = true;
                    }
                }

                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (numDisplayGauges > 0) { displayGaugeDeletePos = displayGaugeListProp.arraySize - 1; }
                }

                GUILayout.EndHorizontal();
                #endregion

                #region Display Gauge List
                numDisplayGauges = displayGaugeListProp.arraySize;

                for (int dgIdx = 0; dgIdx < numDisplayGauges; dgIdx++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    dgauProp = displayGaugeListProp.GetArrayElementAtIndex(dgIdx);

                    #region Find Display Gauge Properties
                    dgauShowInEditorProp = dgauProp.FindPropertyRelative("showInEditor");
                    #endregion

                    #region Display Gauge Move/Insert/Delete buttons
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel += 1;
                    dgauShowInEditorProp.boolValue = EditorGUILayout.Foldout(dgauShowInEditorProp.boolValue, "Display Gauge " + (dgIdx + 1).ToString("00"));
                    EditorGUI.indentLevel -= 1;

                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numDisplayGauges > 1) { displayGaugeMoveDownPos = dgIdx; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { displayGaugeInsertPos = dgIdx; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { displayGaugeDeletePos = dgIdx; }
                    GUILayout.EndHorizontal();
                    #endregion

                    if (dgauShowInEditorProp.boolValue)
                    {
                        #region Find Display Gauge Properties
                        dgauShowGaugeProp = dgauProp.FindPropertyRelative("showGauge");
                        dgauGaugeValueProp = dgauProp.FindPropertyRelative("gaugeValue");
                        dgauOffsetXProp = dgauProp.FindPropertyRelative("offsetX");
                        dgauOffsetYProp = dgauProp.FindPropertyRelative("offsetY");
                        dgauDisplayWidthProp = dgauProp.FindPropertyRelative("displayWidth");
                        dgauDisplayHeightProp = dgauProp.FindPropertyRelative("displayHeight");
                        dgauIsColourAffectedByValueProp = dgauProp.FindPropertyRelative("isColourAffectByValue");
                        dgauForegroundColourProp = dgauProp.FindPropertyRelative("foregroundColour");
                        dgauForegroundHighColourProp = dgauProp.FindPropertyRelative("foregroundHighColour");
                        dgauForegroundMediumColourProp = dgauProp.FindPropertyRelative("foregroundMediumColour");
                        dgauForegroundLowColourProp = dgauProp.FindPropertyRelative("foregroundLowColour");
                        dgauForegroundSpriteProp = dgauProp.FindPropertyRelative("foregroundSprite");
                        dgauBackgroundColourProp = dgauProp.FindPropertyRelative("backgroundColour");
                        dgauBackgroundSpriteProp = dgauProp.FindPropertyRelative("backgroundSprite");
                        dgauFillMethodProp = dgauProp.FindPropertyRelative("fillMethod");
                        dgauIsKeepAspectRatioProp = dgauProp.FindPropertyRelative("isKeepAspectRatio");
                        dgauTextColourProp = dgauProp.FindPropertyRelative("textColour");
                        dgauGaugeStringProp = dgauProp.FindPropertyRelative("gaugeString");
                        dgauTextAlignmentProp = dgauProp.FindPropertyRelative("textAlignment");
                        dgauTextDirectionProp = dgauProp.FindPropertyRelative("textDirection");
                        dgauTextFontStyleProp = dgauProp.FindPropertyRelative("fontStyle");
                        dgauIsBestFitProp = dgauProp.FindPropertyRelative("isBestFit");
                        dgauFontMinSizeProp = dgauProp.FindPropertyRelative("fontMinSize");
                        dgauFontMaxSizeProp = dgauProp.FindPropertyRelative("fontMaxSize");
                        #endregion

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauShowGaugeProp, dgShowGaugeSettingsContent);
                        // Always visible outside play mode
                        if (EditorGUI.EndChangeCheck() && !stickyDisplayModule.IsEditorMode && Application.isPlaying)
                        {
                            serializedObject.ApplyModifiedProperties();
                            if (dgauShowGaugeProp.boolValue) { stickyDisplayModule.ShowDisplayGauge(stickyDisplayModule.displayGaugeList[dgIdx]); }
                            else { stickyDisplayModule.HideDisplayGauge(stickyDisplayModule.displayGaugeList[dgIdx]); }
                            serializedObject.Update();
                        }

                        EditorGUILayout.PropertyField(dgauProp.FindPropertyRelative("gaugeName"), dgGaugeNameSettingsContent);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauGaugeStringProp, dgGaugeStringSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Text");
                            stickyDisplayModule.SetDisplayGaugeText(stickyDisplayModule.displayGaugeList[dgIdx], dgauGaugeStringProp.stringValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauGaugeValueProp, dgGaugeValueSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Value");
                            stickyDisplayModule.SetDisplayGaugeValue(stickyDisplayModule.displayGaugeList[dgIdx], dgauGaugeValueProp.floatValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauOffsetXProp, dgOffsetXSettingsContent);
                        EditorGUILayout.PropertyField(dgauOffsetYProp, dgOffsetYSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Offset");
                            stickyDisplayModule.SetDisplayGaugeOffset(stickyDisplayModule.displayGaugeList[dgIdx], dgauOffsetXProp.floatValue, dgauOffsetYProp.floatValue);
                            ReadProps(_undoGroup);
                        }

                        /// TODO - fix left/right top/bottom offset Draw method - has a few bugs
                        //bool hasChanged = StickyEditorHelper.DrawOffsetLeftRight(dgauOffsetXProp, dgauDisplayWidthProp, defaultEditorLabelWidth);
                        //hasChanged = StickyEditorHelper.DrawOffsetTopBottom(dgauOffsetYProp, dgauDisplayHeightProp, defaultEditorLabelWidth) || hasChanged;
                        //if (hasChanged && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        //{
                        //    int _undoGroup = ApplyAndRecord("Display Gauge Offset");
                        //    stickyDisplayModule.SetDisplayGaugeOffset(stickyDisplayModule.displayGaugeList[dgIdx], dgauOffsetXProp.floatValue, dgauOffsetYProp.floatValue);
                        //    ReadProps(_undoGroup);
                        //}

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauDisplayWidthProp, dgDisplayWidthSettingsContent);
                        EditorGUILayout.PropertyField(dgauDisplayHeightProp, dgDisplayHeightSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Size");
                            stickyDisplayModule.SetDisplayGaugeSize(stickyDisplayModule.displayGaugeList[dgIdx], dgauDisplayWidthProp.floatValue, dgauDisplayHeightProp.floatValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauIsColourAffectedByValueProp, dgIsColourAffectedByValueSettingsContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (stickyDisplayModule.IsEditorMode || Application.isPlaying)
                            {
                                int _undoGroup = ApplyAndRecord("Display Gauge Value affects Colour");

                                if (dgauIsColourAffectedByValueProp.boolValue)
                                {
                                    // Copy current foreground colour into High colour
                                    stickyDisplayModule.SetDisplayGaugeValueAffectsColourOn
                                    (
                                        stickyDisplayModule.displayGaugeList[dgIdx],
                                        dgauForegroundLowColourProp.colorValue,
                                        dgauForegroundMediumColourProp.colorValue,
                                        dgauForegroundColourProp.colorValue
                                    );
                                }
                                else
                                {
                                    // Copy high colour back into foreground colour
                                    stickyDisplayModule.SetDisplayGaugeValueAffectsColourOff(stickyDisplayModule.displayGaugeList[dgIdx], dgauForegroundHighColourProp.colorValue);
                                }
                                ReadProps(_undoGroup);
                            }
                        }

                        if (dgauIsColourAffectedByValueProp.boolValue)
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(dgauForegroundLowColourProp, dgForegroundLowColourSettingsContent);
                            EditorGUILayout.PropertyField(dgauForegroundMediumColourProp, dgForegroundMediumColourSettingsContent);
                            EditorGUILayout.PropertyField(dgauForegroundHighColourProp, dgForegroundHighColourSettingsContent);
                            if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                            {
                                int _undoGroup = ApplyAndRecord("Display Gauge Foreground Colours");
                                stickyDisplayModule.SetDisplayGaugeValue(stickyDisplayModule.displayGaugeList[dgIdx], dgauGaugeValueProp.floatValue);
                                ReadProps(_undoGroup);
                            }
                        }
                        else
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(dgauForegroundColourProp, dgForegroundColourSettingsContent);
                            if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                            {
                                int _undoGroup = ApplyAndRecord("Display Gauge Foreground Colour");
                                stickyDisplayModule.SetDisplayGaugeForegroundColour(stickyDisplayModule.displayGaugeList[dgIdx], dgauForegroundColourProp.colorValue);
                                ReadProps(_undoGroup);
                            }
                        }

                        if (dgauForegroundSpriteProp.objectReferenceValue == null)
                        {
                            EditorGUILayout.HelpBox(dgMissingFgndSpriteWarning, MessageType.Warning);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauForegroundSpriteProp, dgForegroundSpriteSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Foreground Sprite");
                            stickyDisplayModule.SetDisplayGaugeForegroundSprite(stickyDisplayModule.displayGaugeList[dgIdx], (Sprite)dgauForegroundSpriteProp.objectReferenceValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauBackgroundColourProp, dgBackgroundColourSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Background Colour");
                            stickyDisplayModule.SetDisplayGaugeBackgroundColour(stickyDisplayModule.displayGaugeList[dgIdx], dgauBackgroundColourProp.colorValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauBackgroundSpriteProp, dgBackgroundSpriteSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Background Sprite");
                            stickyDisplayModule.SetDisplayGaugeBackgroundSprite(stickyDisplayModule.displayGaugeList[dgIdx], (Sprite)dgauBackgroundSpriteProp.objectReferenceValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauFillMethodProp, dgFillMethodSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Fill Method");
                            stickyDisplayModule.SetDisplayGaugeFillMethod(stickyDisplayModule.displayGaugeList[dgIdx], (S3DDisplayGauge.DGFillMethod)dgauFillMethodProp.intValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauIsKeepAspectRatioProp, dgIsKeepAspectRatioSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Keep Aspect Ratio");
                            stickyDisplayModule.SetDisplayGaugeKeepAspectRatio(stickyDisplayModule.displayGaugeList[dgIdx], dgauIsKeepAspectRatioProp.boolValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauTextColourProp, dgTextColourSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Text Colour");
                            stickyDisplayModule.SetDisplayGaugeTextColour(stickyDisplayModule.displayGaugeList[dgIdx], dgauTextColourProp.colorValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauTextAlignmentProp, dgTextAlignmentSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Alignment");
                            stickyDisplayModule.SetDisplayGaugeTextAlignment(stickyDisplayModule.displayGaugeList[dgIdx], (TextAnchor)dgauTextAlignmentProp.enumValueIndex);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauTextDirectionProp, dgTextDirectionSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Direction");
                            stickyDisplayModule.SetDisplayGaugeTextDirection(stickyDisplayModule.displayGaugeList[dgIdx], (S3DDisplayGauge.DGTextDirection)dgauTextDirectionProp.intValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauTextFontStyleProp, dgTextFontStyleSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Font Style");
                            stickyDisplayModule.SetDisplayGaugeTextFontStyle(stickyDisplayModule.displayGaugeList[dgIdx], (FontStyle)dgauTextFontStyleProp.intValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauIsBestFitProp, dgTextIsBestFitSettingsContent);
                        if (dgauIsBestFitProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(dgauFontMinSizeProp, dgTextFontMinSizeSettingsContent);
                        }
                        EditorGUILayout.PropertyField(dgauFontMaxSizeProp, dgTextFontMaxSizeSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            //int _undoGroup = ApplyAndRecord("Display Gauge Font Size");
                            stickyDisplayModule.SetDisplayGaugeTextFontSize(stickyDisplayModule.displayGaugeList[dgIdx], dgauIsBestFitProp.boolValue, dgauFontMinSizeProp.intValue, dgauFontMaxSizeProp.intValue);
                            //ReadProps(_undoGroup);
                        }                      
                    }
                    GUILayout.EndVertical();
                }

                #endregion

                #region Move/Insert/Delete Display Gauges
                if (displayGaugeDeletePos >= 0 || displayGaugeInsertPos >= 0 || displayGaugeMoveDownPos >= 0)
                {
                    GUI.FocusControl(null);

                    // Don't permit multiple operations in the same pass
                    if (displayGaugeMoveDownPos >= 0)
                    {
                        // Move down one position, or wrap round to start of list
                        if (displayGaugeMoveDownPos < displayGaugeListProp.arraySize - 1)
                        {
                            displayGaugeListProp.MoveArrayElement(displayGaugeMoveDownPos, displayGaugeMoveDownPos + 1);
                        }
                        else { displayGaugeListProp.MoveArrayElement(displayGaugeMoveDownPos, 0); }

                        serializedObject.ApplyModifiedProperties();
                        // For some reason this fully executes but doesn't update hierarchy order... (but works elsewhere)
                        stickyDisplayModule.RefreshGaugesSortOrder();
                        serializedObject.Update();                       

                        displayGaugeMoveDownPos = -1;
                    }
                    else if (displayGaugeInsertPos >= 0)
                    {
                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        Undo.SetCurrentGroupName("Insert Display Gauge");
                        int _undoGroup = UnityEditor.Undo.GetCurrentGroup();
                        Undo.RecordObject(stickyDisplayModule, string.Empty);

                        S3DDisplayGauge insertedGauge = new S3DDisplayGauge(stickyDisplayModule.displayGaugeList[displayGaugeInsertPos]);
                        insertedGauge.showInEditor = true;
                        // Generate a new hashcode for the duplicated DisplayGauge
                        insertedGauge.guidHash = S3DMath.GetHashCodeFromGuid();

                        stickyDisplayModule.displayGaugeList.Insert(displayGaugeInsertPos, insertedGauge);
                        // Hide original DisplayGauge
                        stickyDisplayModule.displayGaugeList[displayGaugeInsertPos + 1].showInEditor = false;
                        RectTransform insertedGaugePanel = stickyDisplayModule.CreateGaugePanel(insertedGauge);
                        if (insertedGaugePanel != null)
                        {
                            if (insertedGauge.isKeepAspectRatio)
                            {
                                stickyDisplayModule.SetDisplayGaugeKeepAspectRatio(insertedGauge, true);
                            }

                            // Make sure we can rollback the creation of the new gauge panel
                            Undo.RegisterCreatedObjectUndo(insertedGaugePanel.gameObject, string.Empty);
                            stickyDisplayModule.RefreshGaugesSortOrder();
                        }

                        Undo.CollapseUndoOperations(_undoGroup);

                        // Read all properties from the StickyDisplayModule
                        serializedObject.Update();

                        displayGaugeInsertPos = -1;

                        isSceneModified = true;
                    }
                    else if (displayGaugeDeletePos >= 0)
                    {
                        // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and displayGaugeDeletePos is reset to -1.
                        int _deleteIndex = displayGaugeDeletePos;
                        serializedObject.ApplyModifiedProperties();

                        if (EditorUtility.DisplayDialog("Delete Display Gauge " + (displayGaugeDeletePos + 1) + "?", "Display Gauge " + (displayGaugeDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the display gauge from the list.", "Delete Now", "Cancel"))
                        {
                            int _guidHash = displayGaugeListProp.GetArrayElementAtIndex(_deleteIndex).FindPropertyRelative("guidHash").intValue;

                            stickyDisplayModule.DeleteGauge(_guidHash);

                            displayGaugeDeletePos = -1;

                            serializedObject.Update();

                            // Force stickyDisplayModule to be serialized after a delete
                            showGeneralSettingsInEditorProp.boolValue = !showGeneralSettingsInEditorProp.boolValue;
                            showGeneralSettingsInEditorProp.boolValue = !showGeneralSettingsInEditorProp.boolValue;
                        }
                        else { serializedObject.Update(); }
                    }

                    #if UNITY_2019_3_OR_NEWER
                    serializedObject.ApplyModifiedProperties();
                    // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                    if (!Application.isPlaying)
                    {
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                    GUIUtility.ExitGUI();
                    #endif
                }
                #endregion
            }

            EditorGUILayout.EndVertical();
            #endregion

            #region Display Target Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawS3DFoldout(showTargetSettingsInEditorProp, dtgtSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showTargetSettingsInEditorProp.boolValue)
            {
                #region Target Common Settings
                EditorGUILayout.LabelField(dtgtHeaderContent, helpBoxRichText);
                EditorGUILayout.PropertyField(dtgtShowTargetsOutlineInSceneProp, dtgtShowTargetsOutlineInSceneContent);

                EditorGUILayout.PropertyField(dtgtTargetsViewWidthProp, dtgtTargetsViewWidthContent);
                EditorGUILayout.PropertyField(dtgtTargetsViewHeightProp, dtgtTargetsViewHeightContent);
                EditorGUILayout.PropertyField(dtgtTargetsViewOffsetXProp, dtgtTargetsViewOffsetXContent);
                EditorGUILayout.PropertyField(dtgtTargetsViewOffsetYProp, dtgtTargetsViewOffsetYContent);
                EditorGUILayout.PropertyField(dtgtTargetingRangeProp, dtgtTargetingRangeContent);
                #endregion

                int numDisplayTargets = displayTargetListProp.arraySize;

                #region Add-Remove Display Target
                // Reset button variables
                displayTargetMoveDownPos = -1;
                displayTargetInsertPos = -1;
                displayTargetDeletePos = -1;

                GUILayout.BeginHorizontal();
                isDisplayTargetListExpandedProp = serializedObject.FindProperty("isDisplayTargetListExpanded");
                EditorGUI.BeginChangeCheck();
                StickyEditorHelper.DrawS3DFoldout(isDisplayTargetListExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(stickyDisplayModule.displayTargetList, isDisplayTargetListExpandedProp.boolValue);
                    // Read in the properties
                    serializedObject.Update();
                }

                EditorGUILayout.LabelField("Display Targets: " + numDisplayTargets.ToString("00"), labelFieldRichText);

                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    int _undoGroup = ApplyAndRecord("Add Display Target");
                    stickyDisplayModule.AddTarget(0);
                    isSceneModified = true;
                    ReadProps(_undoGroup);

                    // For some reason the displayTargetListProp doesn't get updated after the serializedObject.Update().
                    // This causes an issue with stickyDisplayModule.displayTargetList.
                    if (Application.isPlaying) { EditorGUIUtility.ExitGUI(); }

                    numDisplayTargets = displayTargetListProp.arraySize;
                    if (numDisplayTargets > 0)
                    {
                        // Force new target to be serialized in scene
                        dtgtProp = displayTargetListProp.GetArrayElementAtIndex(numDisplayTargets - 1);
                        dtgtShowInEditorProp = dtgtProp.FindPropertyRelative("showInEditor");
                        dtgtShowInEditorProp.boolValue = !dtgtShowInEditorProp.boolValue;
                        // Show the new display target in the editor
                        dtgtShowInEditorProp.boolValue = true;
                    }
                }

                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (numDisplayTargets > 0) { displayTargetDeletePos = displayTargetListProp.arraySize - 1; }
                }

                GUILayout.EndHorizontal();
                #endregion

                #region Display Target List
                numDisplayTargets = displayTargetListProp.arraySize;

                // Used to populate the list of available reticles for the Display Targets to use.
                int _numDisplayReticles = stickyDisplayModule.displayReticleList == null ? 0 : stickyDisplayModule.displayReticleList.Count;

                for (int dtgtIdx = 0; dtgtIdx < numDisplayTargets; dtgtIdx++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    dtgtProp = displayTargetListProp.GetArrayElementAtIndex(dtgtIdx);

                    #region Find Display Target Properties
                    dtgtShowInEditorProp = dtgtProp.FindPropertyRelative("showInEditor");
                    #endregion

                    #region Display Target Move/Insert/Delete buttons
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel += 1;
                    dtgtShowInEditorProp.boolValue = EditorGUILayout.Foldout(dtgtShowInEditorProp.boolValue, "Display Target " + (dtgtIdx + 1).ToString("00"));
                    EditorGUI.indentLevel -= 1;

                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numDisplayTargets > 1) { displayTargetMoveDownPos = dtgtIdx; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { displayTargetInsertPos = dtgtIdx; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { displayTargetDeletePos = dtgtIdx; }
                    GUILayout.EndHorizontal();
                    #endregion

                    if (dtgtShowInEditorProp.boolValue)
                    {
                        #region Find Display Target Properties
                        dtgtGuidHashDisplayReticleProp = dtgtProp.FindPropertyRelative("guidHashDisplayReticle");
                        dtgtShowTargetProp = dtgtProp.FindPropertyRelative("showTarget");
                        dtgtReticleColourProp = dtgtProp.FindPropertyRelative("reticleColour");
                        dtgtMaxNumberOfTargetsProp = dtgtProp.FindPropertyRelative("maxNumberOfTargets");

                        #endregion

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dtgtShowTargetProp, dtgtShowTargetContent);
                        // Always visible outside play mode
                        if (EditorGUI.EndChangeCheck() && !stickyDisplayModule.IsEditorMode && Application.isPlaying)
                        {
                            serializedObject.ApplyModifiedProperties();
                            if (dtgtShowTargetProp.boolValue) { stickyDisplayModule.ShowDisplayTarget(stickyDisplayModule.displayTargetList[dtgtIdx]); }
                            else { stickyDisplayModule.HideDisplayTarget(stickyDisplayModule.displayTargetList[dtgtIdx]); }
                            serializedObject.Update();
                        }

                        // Look up the reticle for this Display Target
                        int _selectedIdx = stickyDisplayModule.displayReticleList.FindIndex(dr => dr.guidHash == dtgtGuidHashDisplayReticleProp.intValue);

                        GUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField(dtgtDisplayReticleContent, GUILayout.Width(defaultEditorLabelWidth - 26f));

                        // Build a list of Reticles for use with this DisplayTarget
                        if (GUILayout.Button("..", buttonCompact, GUILayout.MaxWidth(20f)))
                        {
                            int _guidHashDisplayTarget = dtgtProp.FindPropertyRelative("guidHash").intValue;

                            // Apply property changes
                            serializedObject.ApplyModifiedProperties();

                            // Create a drop down list of all the locations
                            GenericMenu dropdown = new GenericMenu();
                            dropdown.AddItem(new GUIContent("None"), _selectedIdx < 0, UpdateDisplayTargetReticle, new Vector2Int(_guidHashDisplayTarget, 0));

                            for (int i = 0; i < _numDisplayReticles; i++)
                            {
                                // Replace space #/%/& with different chars as Unity treats them as SHIFT/CTRL/ALT in menus.
                                tempDisplayReticleName = stickyDisplayModule.displayReticleList[i].primarySprite == null ? "No Texture" : stickyDisplayModule.displayReticleList[i].primarySprite.name.Replace(" #", "_#").Replace(" &", " &&").Replace(" %", "_%");
                                dropdown.AddItem(new GUIContent(tempDisplayReticleName), i == _selectedIdx, UpdateDisplayTargetReticle, new Vector2Int(_guidHashDisplayTarget, stickyDisplayModule.displayReticleList[i].guidHash));
                            }
                            dropdown.ShowAsContext();
                            SceneView.RepaintAll();

                            serializedObject.Update();
                        }
                        if (_selectedIdx < 0 || _selectedIdx > _numDisplayReticles - 1)
                        {
                            EditorGUILayout.LabelField("None", GUILayout.MaxWidth(100f));
                            tempSprite = null;
                        }
                        else
                        {
                            tempDisplayReticleName = stickyDisplayModule.displayReticleList[_selectedIdx].primarySprite == null ? "no texture in reticle" : stickyDisplayModule.displayReticleList[_selectedIdx].primarySprite.name;
                            EditorGUILayout.LabelField(string.IsNullOrEmpty(tempDisplayReticleName) ? "no texture name" : tempDisplayReticleName, GUILayout.MaxWidth(100f));

                            tempSprite = stickyDisplayModule.displayReticleList[_selectedIdx].primarySprite;
                        }
                        GUILayout.EndHorizontal();

                        StickyEditorHelper.DrawSprite(spriteContent, tempSprite, 24, defaultEditorLabelWidth  - 4f);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dtgtReticleColourProp, dtgtReticleColourContent);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Target Colour");
                            stickyDisplayModule.SetDisplayTargetReticleColour(stickyDisplayModule.displayTargetList[dtgtIdx], dtgtReticleColourProp.colorValue);
                            ReadProps(_undoGroup);
                        }

                        #region Add and Remove Slots
                        int numSlots = dtgtMaxNumberOfTargetsProp.intValue;
                        EditorGUI.BeginChangeCheck();
                        numSlots = EditorGUILayout.IntSlider(dtgtMaxNumberOfTargetsContent, numSlots, 1, S3DDisplayTarget.MAX_DISPLAYTARGET_SLOTS);
                        if (EditorGUI.EndChangeCheck() && (stickyDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            // Add or remove DisplayTarget Slots
                            if (numSlots > dtgtMaxNumberOfTargetsProp.intValue)
                            {
                                int _undoGroup = ApplyAndRecord("Add Display Target Slots");
                                stickyDisplayModule.AddTargetSlots(stickyDisplayModule.displayTargetList[dtgtIdx], numSlots - dtgtMaxNumberOfTargetsProp.intValue);
                                ReadProps(_undoGroup);
                            }
                            else if (numSlots < dtgtMaxNumberOfTargetsProp.intValue)
                            {
                                int _undoGroup = ApplyAndRecord("Delete Display Target Slots");
                                stickyDisplayModule.DeleteTargetSlots(dtgtProp.FindPropertyRelative("guidHash").intValue, dtgtMaxNumberOfTargetsProp.intValue - numSlots);
                                ReadProps(_undoGroup);
                            }
                        }
                        #endregion
                    }

                    GUILayout.EndVertical();
                }

                #endregion

                #region Move/Insert/Delete Target List

                if (displayTargetDeletePos >= 0 || displayTargetInsertPos >= 0 || displayTargetMoveDownPos >= 0)
                {
                    GUI.FocusControl(null);

                    // Don't permit multiple operations in the same pass
                    if (displayTargetMoveDownPos >= 0)
                    {
                        // Move down one position, or wrap round to start of list
                        if (displayTargetMoveDownPos < displayTargetListProp.arraySize - 1)
                        {
                            displayTargetListProp.MoveArrayElement(displayTargetMoveDownPos, displayTargetMoveDownPos + 1);
                        }
                        else { displayTargetListProp.MoveArrayElement(displayTargetMoveDownPos, 0); }

                        serializedObject.ApplyModifiedProperties();

                        // MoveArrayElement does not move the displayTargetSlotList, so we need to do it manually here.
                        stickyDisplayModule.RefreshDisplayTargetSlots();

                        stickyDisplayModule.RefreshTargetsSortOrder();
                        serializedObject.Update();                       

                        displayTargetMoveDownPos = -1;
                    }
                    else if (displayTargetInsertPos >= 0)
                    {
                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        Undo.SetCurrentGroupName("Insert Display Target");
                        int _undoGroup = UnityEditor.Undo.GetCurrentGroup();
                        Undo.RecordObject(stickyDisplayModule, string.Empty);

                        S3DDisplayTarget insertedTarget = new S3DDisplayTarget(stickyDisplayModule.displayTargetList[displayTargetInsertPos]);
                        insertedTarget.showInEditor = true;
                        // Generate a new hashcode for the duplicated DisplayTarget
                        insertedTarget.guidHash = S3DMath.GetHashCodeFromGuid();
                        insertedTarget.maxNumberOfTargets = 1;

                        stickyDisplayModule.displayTargetList.Insert(displayTargetInsertPos, insertedTarget);
                        // Hide original DisplayTarget
                        stickyDisplayModule.displayTargetList[displayTargetInsertPos + 1].showInEditor = false;
                        RectTransform insertedTargetPanel = stickyDisplayModule.CreateTargetPanel(insertedTarget, 0);
                        if (insertedTargetPanel != null)
                        {
                            // Update the reticle to force it to take effect on the inserted (new) Target
                            stickyDisplayModule.SetDisplayTargetReticle(insertedTarget.guidHash, insertedTarget.guidHashDisplayReticle);

                            // Make sure we can rollback the creation of the new target panel
                            Undo.RegisterCreatedObjectUndo(insertedTargetPanel.gameObject, string.Empty);
                            stickyDisplayModule.RefreshTargetsSortOrder();
                        }

                        Undo.CollapseUndoOperations(_undoGroup);

                        // Read all properties from the StickyDisplayModule
                        serializedObject.Update();

                        displayTargetInsertPos = -1;

                        isSceneModified = true;
                    }
                    else if (displayTargetDeletePos >= 0)
                    {
                        // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and displayTargetDeletePos is reset to -1.
                        int _deleteIndex = displayTargetDeletePos;
                        serializedObject.ApplyModifiedProperties();

                        if (EditorUtility.DisplayDialog("Delete Display Target " + (displayTargetDeletePos + 1) + "?", "Display Target " + (displayTargetDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the display target from the list.", "Delete Now", "Cancel"))
                        {
                            int _guidHash = displayTargetListProp.GetArrayElementAtIndex(_deleteIndex).FindPropertyRelative("guidHash").intValue;

                            stickyDisplayModule.DeleteTarget(_guidHash);

                            displayTargetDeletePos = -1;

                            serializedObject.Update();

                            // Force stickyDisplayModule to be serialized after a delete
                            showGeneralSettingsInEditorProp.boolValue = !showGeneralSettingsInEditorProp.boolValue;
                            showGeneralSettingsInEditorProp.boolValue = !showGeneralSettingsInEditorProp.boolValue;
                        }
                        else { serializedObject.Update(); }
                    }

                    #if UNITY_2019_3_OR_NEWER
                    serializedObject.ApplyModifiedProperties();
                    // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                    if (!Application.isPlaying)
                    {
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                    GUIUtility.ExitGUI();
                    #endif
                }

                #endregion
            }

            EditorGUILayout.EndVertical();
            #endregion

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            // Debug at runtime in the editor

            if (isDebuggingEnabled && stickyDisplayModule != null)
            {
                float rightLabelWidth = 150f;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(stickyDisplayModule.IsInitialised ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugRefResolutionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(stickyDisplayModule.ReferenceResolution.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugCanvasResolutionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(stickyDisplayModule.CanvasResolution.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            #endregion

            #region Mark Scene Dirty if required

            if (isSceneModified && !Application.isPlaying)
            {
                isSceneModified = false;
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            #endregion

            stickyDisplayModule.allowRepaint = true;
        }

        #endregion

        #region Public Static Methods
        // Add a menu item so that a Display can be created via the GameObject > 3D Object menu
        [MenuItem("GameObject/3D Object/Sticky3D Controller/Sticky Display Module")]
        public static void CreateStickyDisplay()
        {
            // If this scene does not already have a manager, create one
            GameObject newDisplayGameObject = new GameObject("StickyDisplay");
            newDisplayGameObject.transform.position = Vector3.zero;
            newDisplayGameObject.transform.parent = null;

            newDisplayGameObject.layer = 5;
            newDisplayGameObject.AddComponent<Canvas>();

            Canvas hudCanvas = newDisplayGameObject.GetComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 2;
            UnityEngine.UI.CanvasScaler canvasScaler = newDisplayGameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            if (canvasScaler != null)
            {
                canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
                canvasScaler.matchWidthOrHeight = 0.5f;
            }

            RectTransform hudGORectTfrm = newDisplayGameObject.GetComponent<RectTransform>();

            // Create the DisplayPanel
            Selection.activeTransform = hudGORectTfrm;
            StickyEditorHelper.CallMenu("GameObject/UI/Panel");
            // The new panel is automatically selected
            Selection.activeTransform.name = StickyDisplayModule.displayPanelName;
            RectTransform hudPanel = Selection.activeTransform.GetComponent<RectTransform>();
            UnityEngine.UI.Image img = hudPanel.GetComponent<UnityEngine.UI.Image>();
            img.raycastTarget = false;
            img.enabled = false;

            // Create the Reticle panel
            Selection.activeTransform = hudPanel;
            StickyEditorHelper.CallMenu("GameObject/UI/Panel");
            // The new panel is automatically selected
            Selection.activeTransform.name = StickyDisplayModule.reticlePanelName;
            RectTransform reticlePanel = Selection.activeTransform.GetComponent<RectTransform>();
            img = reticlePanel.GetComponent<UnityEngine.UI.Image>();
            img.raycastTarget = false;
            reticlePanel.anchorMin = new Vector2(0.5f, 0.5f);
            reticlePanel.anchorMax = new Vector2(0.5f, 0.5f);
            reticlePanel.pivot = new Vector2(0.5f, 0.5f);

            reticlePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 64f);
            reticlePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 64f);
            reticlePanel.SetParent(hudPanel);

            // Create the Overlay panel
            Selection.activeTransform = hudPanel;
            StickyEditorHelper.CallMenu("GameObject/UI/Panel");
            // The new panel is automatically selected
            Selection.activeTransform.name = StickyDisplayModule.overlayPanelName;
            RectTransform overlayPanel = Selection.activeTransform.GetComponent<RectTransform>();
            img = overlayPanel.GetComponent<UnityEngine.UI.Image>();
            img.sprite = (Sprite)AssetDatabase.LoadAssetAtPath(StickySetup.texturesFolder + "/Display/S3DUIOverlay1.png", typeof(Sprite));
            img.raycastTarget = false;
            overlayPanel.SetParent(hudPanel);

            StickyDisplayModule stickyDisplayModule = newDisplayGameObject.AddComponent<StickyDisplayModule>();
            Selection.activeTransform = newDisplayGameObject.transform;

            // Add at least one reticle
            S3DDisplayReticle displayReticle = new S3DDisplayReticle();
            displayReticle.primarySprite = (Sprite)AssetDatabase.LoadAssetAtPath(StickySetup.texturesFolder + "/Display/S3DUIAim01.png", typeof(Sprite));
            stickyDisplayModule.displayReticleList = new List<S3DDisplayReticle>(10);
            stickyDisplayModule.displayReticleList.Add(displayReticle);

            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(newDisplayGameObject.scene);
                EditorUtility.SetDirty(stickyDisplayModule);
            }
        }
        #endregion
    }
}