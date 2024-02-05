using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(ShipDisplayModule))]
    public class ShipDisplayModuleEditor : Editor
    {
        #region Enumerations

        #endregion

        #region Custom Editor private variables
        private ShipDisplayModule shipDisplayModule;

        private string sscHelpPDF;

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
        private bool isDebuggingShowTargets = false;

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
        private DisplayGauge.DGType gaugeType;

        #endregion

        #region SceneView Variables
        private Color hudPanelOutlineColour = Color.yellow;
        private Color targetsAreaOutlineColour = Color.red;
        #endregion

        #region Static Strings
        private readonly static string autoHideLockReticleWarning = "With Auto-hide Cursor and Lock Reticle to Cursor on at the same time you may get undesirable side effects";
        private readonly static string dgMissingFgndSpriteWarning = "The foreground sprite (texture) is required to display the gauge value";
        private readonly static string altitudeSpeedWarning = "Altitude and/or Air Speed will not be displayed if Show Overlay is not enabled under General settings";
        #endregion

        #region GUIContent General

        private readonly static GUIContent headerContent = new GUIContent("This module enables you to show a heads-up display for a ship. Functionality can be extended with your own scripts. See manual for details.");
        private readonly static GUIContent btnRefreshContent = new GUIContent("Refresh","Used in edit-mode after the window has been resized");
        private readonly static GUIContent generalSettingsContent = new GUIContent("General Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent initialiseOnStartContent = new GUIContent(" Initialise on Start", "If enabled, Initialise() will be called as soon as Start() runs. " +
          "This should be disabled if you are instantiating instantiating the HUD throug code.");
        private readonly static GUIContent isShowOnInitialiseContent = new GUIContent(" Show on Initialise", "Show the HUD when it is first Initialised");
        private readonly static GUIContent showOverlayContent = new GUIContent(" Show Overlay", "Show the overlay image on the HUD");
        private readonly static GUIContent autoHideCursorContent = new GUIContent(" Auto-hide Cursor", "Hide the screen cursor or mouse pointer after it has been stationary for x seconds");
        private readonly static GUIContent mainCameraContent = new GUIContent(" Main Camera", "The main camera used to perform calculations with the heads-up display. If blank will be auto-assigned to the first camera with a MainCamera tag.");
        private readonly static GUIContent hideCursorTimeContent = new GUIContent(" Hide Cursor Time", "The number of seconds to wait until after the cursor has not moved before hiding it");
        private readonly static GUIContent displayWidthContent = new GUIContent(" HUD Width", "The head-up display's normalised width of the screen. 1.0 is full width, 0.5 is half width. [Has no effect outside play mode]");
        private readonly static GUIContent displayHeightContent = new GUIContent(" HUD Height", "The head-up display's normalised height of the screen. 1.0 is full height, 0.5 is half height. [Has no effect outside play mode]");
        private readonly static GUIContent displayOffsetXContent = new GUIContent(" HUD Offset X", "The head-up display's normalised offset between the left (-1) and the right (1) from the centre (0) of the screen. [Has no effect outside play mode]");
        private readonly static GUIContent displayOffsetYContent = new GUIContent(" HUD Offset Y", "The head-up display's normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen. [Has no effect outside play mode]");
        private readonly static GUIContent primaryColourContent = new GUIContent(" Primary Colour", "The primary colour of the heads-up display. Used to colour the display overlay. This are affected by Brightness. Calibrate when Brightness = 1");
        private readonly static GUIContent brightnessContent = new GUIContent(" Brightness", "The overall brightness of the HUD");
        private readonly static GUIContent canvasSortOrderContent = new GUIContent(" Canvas Sort Order", "The sort order of the canvas in the scene. Higher numbers are on top.");
        private readonly static GUIContent showHUDOutlineInSceneContent = new GUIContent(" Show HUD Outline", "Show the HUD as a yellow outline in the scene view [Has no effect in play mode]. Click Refresh if screen has been resized.");

        private readonly static GUIContent spriteContent = new GUIContent(" Reticle Sprite", "Preview of the sprite that will be shown on the HUD");
        #endregion

        #region GUIContent - Reticle
        private readonly static GUIContent reticleSettingsContent = new GUIContent("Display Reticle Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent drPrimarySpriteContent = new GUIContent(" Sprite", "The sprite (texture) to be displayed in the heads-up display");
        private readonly static GUIContent activeDisplayReticleContent = new GUIContent(" Active Display Reticle", "The currently selected or displayed reticle. It is used to help aim your weapons.");
        private readonly static GUIContent showActiveDisplayReticleContent = new GUIContent(" Show Active Reticle", "Show or render the active Display Reticle on the HUD. [Has no effect outside play mode]");
        private readonly static GUIContent displayReticleOffsetXContent = new GUIContent(" Reticle Offset X", "The Display Reticle's normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.");
        private readonly static GUIContent displayReticleOffsetYContent = new GUIContent(" Reticle Offset Y", "The Display Reticle's normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.");
        private readonly static GUIContent lockDisplayReticleToCursorContent = new GUIContent(" Lock Reticle to Cursor", "Should the Display Reticle follow the cursor or mouse position on the screen?");
        private readonly static GUIContent activeDisplayReticleColourContent = new GUIContent(" Reticle Colour", "Colour of the active Display Reticle");
        #endregion

        #region GUIContent - Altitude and Speed
        private readonly static GUIContent altSpeedSettingsContent = new GUIContent("Altitude and Speed Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent showAltitudeContent = new GUIContent(" Show Altitude", "Show the Altitude indicator on the HUD. Typically only used when near the surface of a planet. [Has no effect outside play mode]");
        private readonly static GUIContent showAirspeedContent = new GUIContent(" Show Air Speed", "Show the Air Speed indicator on the HUD. Output is in km/h [Has no effect outside play mode]");
        private readonly static GUIContent groundPlaneHeightContent = new GUIContent(" Ground Plane Height", "Used to determine altitude when near a planet's surface. See manual for details.");
        private readonly static GUIContent sourceShipContent = new GUIContent(" Source Ship", "The ship in the scene that will supply the data for this HUD");
        private readonly static GUIContent altitudeTextColourContent = new GUIContent(" Altitude Text Colour", "The colour of the Altitude text (number). This are affected by Brightness. Calibrate when Brightness = 1");
        private readonly static GUIContent airspeedTextColourContent = new GUIContent(" Air Speed Text Colour", "The colour of the Air Speed text (number). This are affected by Brightness. Calibrate when Brightness = 1");
        #endregion

        #region GUIContent - Attitude
        private readonly static GUIContent displayAttitudeSettingsContent = new GUIContent("Display Attitude Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent showAttitudeContent = new GUIContent(" Show Attitude", "Show the attitude on the HUD");
        private readonly static GUIContent attitudeScrollSpriteContent = new GUIContent(" Scrolling Sprite", "The sprite (texture) that will scroll up and down. e.g. SSCUIPitchLadder1");
        private readonly static GUIContent attitudeMaskSpriteContent = new GUIContent(" Mask Sprite", "The sprite (texture) that will mask the scrollable attitude sprite. e.g. SSCUIFilled");
        private readonly static GUIContent attitudeOffsetXContent = new GUIContent(" Offset X", "The attitude normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.");
        private readonly static GUIContent attitudeOffsetYContent = new GUIContent(" Offset Y", "The attitude normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.");
        private readonly static GUIContent attitudeHeightContent = new GUIContent(" Height", "The normalised masked height of the scrollable attitude. 1.0 is full height of the screen, 0.5 is half height.");
        private readonly static GUIContent attitudePrimaryColourContent = new GUIContent(" Primary Colour", "Primary colour of the scrollable attitude.");
        private readonly static GUIContent attitudeScrollSpriteBorderWidthPropContent = new GUIContent(" Sprite Border Width", "The number of pixels between the top of the scroll sprite and the first (90) pitch line. It is assumed that this is the same for between the bottom and the -90 pitch line.");

        #endregion

        #region GUIContent - Fade
        private readonly static GUIContent displayFadeSettingsContent = new GUIContent("Display Fade Settings");
        private readonly static GUIContent fadeInOnStartContent = new GUIContent(" Fade In On Start", "Fade the display in when it first starts");
        private readonly static GUIContent fadeInDurationContent = new GUIContent(" Fade In Duration", "The length of time in seconds, it takes to fully fade in the display");
        private readonly static GUIContent fadeOutOnStartContent = new GUIContent(" Fade Out On Start", "Fade the display out when it first starts");
        private readonly static GUIContent fadeOutDurationContent = new GUIContent(" Fade Out Duration", "The length of time in seconds, it takes to fully fade out the display");
        private readonly static GUIContent fadeColourContent = new GUIContent(" Fade Colour", "The colour the display will fade from or to.");

        #endregion

        #region GUIContent - Flicker
        private readonly static GUIContent displayFlickerSettingsContent = new GUIContent("Display Flicker Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent flkIsShowHUDWithFlickerContent = new GUIContent(" Show HUD with Flicker", "Whenever the HUD is shown, should it flicker on?");
        private readonly static GUIContent flkIsHideHUDWithFlickerContent = new GUIContent(" Hide HUD with Flicker", "Whenever the HUD is hidden, should it flicker off?");
        private readonly static GUIContent flkDefaultDurationContent = new GUIContent(" Default Duration", "The time, in seconds, the effect takes to reach a steady state");
        private readonly static GUIContent flkMinInactiveTimeContent = new GUIContent(" Min Inactive Time", "The minimum time, in seconds, that the effect is inactive or off");
        private readonly static GUIContent flkMaxInactiveTimeContent = new GUIContent(" Max Inactive Time", "The maximum time, in seconds, that the effect is inactive or off");
        private readonly static GUIContent flkMinActiveTimeContent = new GUIContent(" Min Active Time", "The minimum time, in seconds, that the effect is active or on");
        private readonly static GUIContent flkMaxActiveTimeContent = new GUIContent(" Max Active Time", "The maximum time, in seconds, that the effect is active or on");
        private readonly static GUIContent flkVariableIntensityContent = new GUIContent(" Variable Intensity", "The intensity of the effect will randomly change");
        private readonly static GUIContent flkSmoothingContent = new GUIContent(" Smoothing", "Smooth the flickering effect. Higher values give a smoother effect");
        private readonly static GUIContent flkMaxIntensityContent = new GUIContent(" Max Intensity", "The maximum intensity of the effect used during the on cycle. This is a multiplier of the starting intensity of the effect.");
        #endregion

        #region GUIContent - Heading
        private readonly static GUIContent displayHeadingSettingsContent = new GUIContent("Display Heading Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent showHeadingContent = new GUIContent(" Show Heading", "Show the heading on the HUD");
        private readonly static GUIContent showHeadingIndicatorContent = new GUIContent(" Indicator", "Show the small heading indicator");
        private readonly static GUIContent headingScrollSpriteContent = new GUIContent(" Scrolling Sprite", "The sprite (texture) that will scroll left or right. e.g. SSCUIHeading1");
        private readonly static GUIContent headingMaskSpriteContent = new GUIContent(" Mask Sprite", "The sprite (texture) that will mask the scrollable heading sprite. e.g. SSCUIFilled");
        private readonly static GUIContent headingIndicatorSpriteContent = new GUIContent(" Indicator Sprite", "The small sprite (texture) that will indicate or point to the heading on the HUD. e.g. SSCUIIndicator1");
        private readonly static GUIContent headingOffsetXContent = new GUIContent(" Offset X", "The heading normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.");
        private readonly static GUIContent headingOffsetYContent = new GUIContent(" Offset Y", "The heading normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.");
        private readonly static GUIContent headingWidthContent = new GUIContent(" Width", "The normalised masked width of the scrollable heading. 1.0 is full width of the screen, 0.5 is half width.");
        private readonly static GUIContent headingPrimaryColourContent = new GUIContent(" Primary Colour", "Primary colour of the scrollable heading.");
        private readonly static GUIContent headingIndicatorColourContent = new GUIContent(" Indicator Colour", "The small indicator colour of the scrollable heading.");

        #endregion

        #region GUIContent - Gauges
        private readonly static GUIContent displayGaugeSettingsContent = new GUIContent("Display Gauge Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent displayGaugeHeaderContent = new GUIContent("Gauges are stacking or overlayed in the order they appear in the list.");
        private readonly static GUIContent dgExportJsonContent = new GUIContent("Export Json", "Export the Gauge to a Json file");
        private readonly static GUIContent dgImportJsonContent = new GUIContent("Import", "Import a Gauge from a Json file");
        private readonly static GUIContent dgGaugeNameSettingsContent = new GUIContent(" Gauge Name", "The name or description of the gauge. This can be used to identify the gauge.");
        private readonly static GUIContent dgGaugeStringSettingsContent = new GUIContent(" Gauge Text", "The text to display in the gauge. It can include RichText markup. e.g. <b>Bold Text</b>");
        private readonly static GUIContent dgGaugeLabelSettingsContent = new GUIContent(" Gauge Label", "The label text on a numeric gauge with label. It can include RichText markup. e.g. <b>Bold Text</b>.");
        private readonly static GUIContent dgGaugeValueSettingsContent = new GUIContent(" Gauge Value", "The current amount or reading on the gauge");
        private readonly static GUIContent dgGaugeTypeSettingsContent = new GUIContent(" Gauge Type", "The type or style of the gauge");
        private readonly static GUIContent dgShowGaugeSettingsContent = new GUIContent(" Show Gauge", "Show the gauge on the HUD. [Has no effect outside play mode]");
        private readonly static GUIContent dgOffsetXSettingsContent = new GUIContent(" Offset X", "The Display Gauge's normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.");
        private readonly static GUIContent dgOffsetYSettingsContent = new GUIContent(" Offset Y", "The Display Gauge's normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.");
        private readonly static GUIContent dgDisplayWidthSettingsContent = new GUIContent(" Display Width", "The Display Gauge's normalised width. 1.0 is full screen width, 0.5 is half width.");
        private readonly static GUIContent dgDisplayHeightSettingsContent = new GUIContent(" Display Height", "The Display Gauge's normalised height. 1.0 is full screen height, 0.5 is half height.");
        private readonly static GUIContent dgIsColourAffectedByValueSettingsContent = new GUIContent(" Value Affects Colour", "Does the colour of the foreground change based on the value of the gauge?");
        private readonly static GUIContent dgMediumColourValueContent = new GUIContent(" Medium Colour Value", "When isColourAffectByValue is true, the value for the foreground medium colour [default: 0.5]");
        private readonly static GUIContent dgForegroundColourSettingsContent = new GUIContent(" Foreground Colour", "Colour of the Gauge foreground");
        private readonly static GUIContent dgForegroundHighColourSettingsContent = new GUIContent(" Foreground Hi Colour", "Colour of the Gauge foreground when value is 1.0");
        private readonly static GUIContent dgForegroundMediumColourSettingsContent = new GUIContent(" Foreground Med Colour", "Colour of the Gauge foreground when value is Medium Colour Value.");
        private readonly static GUIContent dgForegroundLowColourSettingsContent = new GUIContent(" Foreground Low Colour", "Colour of the Gauge foreground when value is 0.0");
        private readonly static GUIContent dgForegroundSpriteSettingsContent = new GUIContent(" Foreground Sprite", "The sprite (texture) for the foreground of the gauge");
        private readonly static GUIContent dgBackgroundColourSettingsContent = new GUIContent(" Background Colour", "Colour of the Gauge background");
        private readonly static GUIContent dgBackgroundSpriteSettingsContent = new GUIContent(" Background Sprite", "The sprite (texture) for the background of the gauge");
        private readonly static GUIContent dgFillMethodSettingsContent = new GUIContent(" Fill Method", "Determines the method used to fill the gauge background sprite when the gaugeValue is modified");
        private readonly static GUIContent dgIsKeepAspectRatioSettingsContent = new GUIContent(" Keep Aspect Ratio", "Keep the original aspect ratio of the foreground and background sprites. Useful when creating circular gauges.");
        private readonly static GUIContent dgTextColourSettingsContent = new GUIContent(" Text Colour", "Colour of the Message text");
        private readonly static GUIContent dgTextAlignmentSettingsContent = new GUIContent(" Text Alignment", "The position of the text within the Display Gauge panel");
        private readonly static GUIContent dgLabelAlignmentSettingsContent = new GUIContent(" Label Alignment", "The position of the label within the Display Gauge panel");
        private readonly static GUIContent dgTextDirectionSettingsContent = new GUIContent(" Text Rotation", "The rotation of the text within the Display Gauge panel");
        private readonly static GUIContent dgTextFontStyleSettingsContent = new GUIContent(" Font Style", "The font style of the text within the Display Gauge panel");
        private readonly static GUIContent dgTextIsBestFitSettingsContent = new GUIContent(" Is Best Fit", "Is the text font size automatically changes within the bounds of Font Min Size and Font Max Size to fill the panel?");
        private readonly static GUIContent dgTextFontMinSizeSettingsContent = new GUIContent(" Font Min Size", "When Is Best Fit is true will use this minimum font size if required");
        private readonly static GUIContent dgTextFontMaxSizeSettingsContent = new GUIContent(" Font Max Size", "The font size. If isBestFit is true, this will be the maximum font size it can use.");
        private readonly static GUIContent dgMaxValueSettingsContent = new GUIContent(" Max Value", "When a numeric gaugeType is used, this is the number to display when gaugeValue is 1.0.");
        private readonly static GUIContent dgIsNumericPercentageSettingsContent = new GUIContent(" Is Percentage", "Is the numeric gauge to be displayed as a percentage?");
        private readonly static GUIContent dgDecimalPlacesSettingsContent = new GUIContent(" Decimal Places", "The number of decimal places to display for numeric gauges.");
        #endregion

        #region GUIContent - Messages
        private readonly static GUIContent displayMessageSettingsContent = new GUIContent("Display Message Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent displayMessageHeaderContent = new GUIContent("Messages are stacking or overlayed in the order they appear in the list.");
        private readonly static GUIContent dmExportJsonContent = new GUIContent("Export Json", "Export the Message to a Json file");
        private readonly static GUIContent dmImportJsonContent = new GUIContent("Import", "Import a Message from a Json file");
        private readonly static GUIContent dmMessageNameSettingsContent = new GUIContent(" Message Name", "The name or description of the message. This can be used to identify the message.");
        private readonly static GUIContent dmMessageStringSettingsContent = new GUIContent(" Message Text", "The text to display in the message. It can include RichText markup. e.g. <b>Bold Text</b>");
        private readonly static GUIContent dmShowMessageSettingsContent = new GUIContent(" Show Message", "Show the message on the HUD. [Has no effect outside play mode]");
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
        private readonly static GUIContent dmFadeInDurationContent = new GUIContent(" Fade-in Duration", "The number of seconds the message takes to become fully visible");
        private readonly static GUIContent dmFadeOutDurationContent = new GUIContent(" Fade-out Duration", "The number of seconds the message takes to fade out of view");

        #endregion

        #region GUIContent - Targets
        private readonly static GUIContent dtgtSettingsContent = new GUIContent("Display Target Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent dtgtAutoUpdateTargetPositionsContent = new GUIContent(" Auto Update Positions", "When DisplayTarget slots have an active RadarItem assigned to them, should the reticles be automatically moved on the HUD");
        private readonly static GUIContent dtgtShowTargetsOutlineInSceneContent = new GUIContent(" Show Viewport Outline", "Show the rendering limits as a red outline in the scene view [Has no effect in play mode]. Click Refresh if screen has been resized.");
        private readonly static GUIContent dtgtTargetsViewWidthContent = new GUIContent(" Viewport Width", "The width of the clipped area in which Targets are visible. 1.0 is full width, 0.5 is half width.");
        private readonly static GUIContent dtgtTargetsViewHeightContent = new GUIContent(" Viewport Height", "The height of the clipped area in which Targets are visible. 1.0 is full height, 0.5 is half height.");
        private readonly static GUIContent dtgtTargetsViewOffsetXContent = new GUIContent(" Viewport Offset X", "The X offset from centre of the screen for the viewport");
        private readonly static GUIContent dtgtTargetsViewOffsetYContent = new GUIContent(" Viewport Offset Y", "The Y offset from centre of the screen for the viewport");
        private readonly static GUIContent dtgtTargetingRangeContent = new GUIContent(" Targeting Range", "The maximum distance in metres that targets can be away from the ship");
        private readonly static GUIContent dtgtHeaderContent = new GUIContent("Targets use Reticles from the Display Reticles section above. Targets are stacking or overlayed in the order they appear in the list below.");
        private readonly static GUIContent dtgtDisplayReticleContent = new GUIContent(" Display Reticle", "The Display Reticle to use for this Target. To add more Reticles, see the list in the Display Reticle Settings section above.");
        private readonly static GUIContent dtgtShowTargetContent = new GUIContent(" Show Target", "Show the Target on the HUD. [Has no effect outside play mode]");
        private readonly static GUIContent dtgtReticleColourContent = new GUIContent(" Reticle Colour", "The colour of the Display Target");
        private readonly static GUIContent dtgtIsTargetableContent = new GUIContent(" Is Targetable", "Can this DisplayTarget be assigned to a weapon?");
        private readonly static GUIContent dtgtMaxNumberOfTargetsContent = new GUIContent(" Max Number of Targets", "The maximum number of these DisplayTargets that can be shown on the HUD at any one time");
        private readonly static GUIContent dtgtFactionsToIncludeContent = new GUIContent(" Factions to Include", "If the array is empty (0), any item belonging to a faction in the game can be used on this DisplayTarget. This is true except when Is Targetable is enabled, in which case only enemy factions can be shown");
        private readonly static GUIContent dtgtSquadronsToIncludeContent = new GUIContent(" Squadrons to Include", "If the array is empty (0), any item belonging to a squadron in the game can be used on this DisplayTarget. This is true except when Is Targetable is enabled, in which case only enemy squadrons can be shown. ");

        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent("Debug Mode", "Use this to troubleshoot the data at runtime in the editor.");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent("Is Initialised?");
        private readonly static GUIContent debugRefResolutionContent = new GUIContent("Reference Resolution");
        private readonly static GUIContent debugCanvasResolutionContent = new GUIContent("Current Resolution");
        private readonly static GUIContent debugIsHUDShownContent = new GUIContent("Is HUD Shown");
        private readonly static GUIContent debugShowTargetsContent = new GUIContent("Show Targets");
        private readonly static GUIContent debugTargetPositionContent = new GUIContent(" Position");
        private readonly static GUIContent debugTargetVisibleContent = new GUIContent(" Is Visable");
        private readonly static GUIContent debugTargetRadarIndexContent = new GUIContent("  Radar Item Index");
        private readonly static GUIContent debugTargetRadarItemTypeContent = new GUIContent("  Radar Item Type");
        private readonly static GUIContent debugTargetRadarPositionContent = new GUIContent("  Radar Position");
        private readonly static GUIContent debugTargetScreenPositionContent = new GUIContent("  Screen Position");
        private readonly static GUIContent debugTargetRadarGameObjectContent = new GUIContent("  Radar GameObject");
        private readonly static GUIContent debugTargetRadarShipContent = new GUIContent("  Radar Ship");
        #endregion

        #region Serialized Properties
        // general
        private SerializedProperty showGeneralSettingsInEditorProp;
        private SerializedProperty showReticleSettingsInEditorProp;
        private SerializedProperty showAltSpeedSettingsInEditorProp;
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
        private SerializedProperty showHUDOutlineInSceneProp;

        // reticles
        private SerializedProperty displayReticleListProp;
        private SerializedProperty displayReticleProp;
        private SerializedProperty showActiveDisplayReticleProp;
        private SerializedProperty lockDisplayReticleToCursorProp;
        private SerializedProperty guidHashActiveDisplayReticleProp;
        private SerializedProperty displayReticleShowInEditorProp;
        private SerializedProperty isDisplayReticleListExpandedProp;
        private SerializedProperty activeDisplayReticleColourProp;

        // altitude and speed
        private SerializedProperty showAltitudeProp;
        private SerializedProperty showAirspeedProp;
        private SerializedProperty sourceShipProp;
        private SerializedProperty altitudeTextColourProp;
        private SerializedProperty airspeedTextColourProp;

        // Attitude
        private SerializedProperty showAttitudeSettingsInEditorProp;
        private SerializedProperty showAttitudeProp;
        private SerializedProperty attitudeScrollSpriteProp;
        private SerializedProperty attitudeMaskSpriteProp;
        private SerializedProperty attitudeOffsetXProp;
        private SerializedProperty attitudeOffsetYProp;
        private SerializedProperty attitudeHeightProp;
        private SerializedProperty attitudePrimaryColourProp;
        private SerializedProperty attitudeScrollSpriteBorderWidthProp;

        // Fade
        private SerializedProperty showFadeSettingsInEditorProp;
        private SerializedProperty fadeInOnStartProp;
        private SerializedProperty fadeOutOnStartProp;
        private SerializedProperty fadeInDurationProp;
        private SerializedProperty fadeOutDurationProp;
        private SerializedProperty fadeColourProp;

        // Flicker
        private SerializedProperty showFlickerSettingsInEditorProp;
        private SerializedProperty flkIsShowHUDWithFlickerProp;
        private SerializedProperty flkIsHideHUDWithFlickerProp;
        private SerializedProperty flkDefaultDurationProp;
        private SerializedProperty flkMinInactiveTimeProp;
        private SerializedProperty flkMaxInactiveTimeProp;
        private SerializedProperty flkMinActiveTimeProp;
        private SerializedProperty flkMaxActiveTimeProp;
        private SerializedProperty flkVariableIntensityProp;
        private SerializedProperty flkSmoothingProp;
        private SerializedProperty flkMaxIntensityProp;

        // Heading
        private SerializedProperty showHeadingSettingsInEditorProp;
        private SerializedProperty showHeadingProp;
        private SerializedProperty showHeadingIndicatorProp;
        private SerializedProperty headingScrollSpriteProp;
        private SerializedProperty headingMaskSpriteProp;
        private SerializedProperty headingIndicatorSpriteProp;
        private SerializedProperty headingOffsetXProp;
        private SerializedProperty headingOffsetYProp;
        private SerializedProperty headingWidthProp;
        private SerializedProperty headingPrimaryColourProp;
        private SerializedProperty headingIndicatorColourProp;

        // gauges
        private SerializedProperty displayGaugeListProp;
        private SerializedProperty dgauProp;
        private SerializedProperty isDisplayGaugeListExpandedProp;
        private SerializedProperty dgauShowInEditorProp;
        private SerializedProperty dgauShowGaugeProp;
        private SerializedProperty dgauGaugeNameProp;
        private SerializedProperty dgauFillMethodProp;
        private SerializedProperty dgauIsKeepAspectRatioProp;
        private SerializedProperty dgauGaugeValueProp;
        private SerializedProperty dgauGaugeTypeProp;
        private SerializedProperty dgauIsColourAffectedByValueProp;
        private SerializedProperty dgauMediumColourValueProp;
        private SerializedProperty dgauForegroundColourProp;
        private SerializedProperty dgauForegroundHighColourProp;
        private SerializedProperty dgauForegroundMediumColourProp;
        private SerializedProperty dgauForegroundLowColourProp;
        private SerializedProperty dgauForegroundSpriteProp;
        private SerializedProperty dgauBackgroundColourProp;
        private SerializedProperty dgauBackgroundSpriteProp;
        private SerializedProperty dgauTextColourProp;
        private SerializedProperty dgauGaugeStringProp;
        private SerializedProperty dgauGaugeLabelProp;
        private SerializedProperty dgauTextAlignmentProp;
        private SerializedProperty dgauLabelAlignmentProp;
        private SerializedProperty dgauTextDirectionProp;
        private SerializedProperty dgauTextFontStyleProp;
        private SerializedProperty dgauDisplayWidthProp;
        private SerializedProperty dgauDisplayHeightProp;
        private SerializedProperty dgauOffsetXProp;
        private SerializedProperty dgauOffsetYProp;
        private SerializedProperty dgauIsBestFitProp;
        private SerializedProperty dgauFontMinSizeProp;
        private SerializedProperty dgauFontMaxSizeProp;
        private SerializedProperty dgauMaxValueProp;
        private SerializedProperty dgauIsNumericPercentageProp;
        private SerializedProperty dgauDecimalPlacesProp;

        // messages
        private SerializedProperty displayMessageListProp;
        private SerializedProperty dmsgProp;
        private SerializedProperty isDisplayMessageListExpandedProp;
        private SerializedProperty dmsgShowInEditorProp;
        private SerializedProperty dmsgMessageNameProp;
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
        private SerializedProperty dmsgFadeinDurationProp;
        private SerializedProperty dmsgFadeoutDurationProp;

        // targets
        private SerializedProperty displayTargetListProp;
        private SerializedProperty dtgtProp;
        private SerializedProperty isDisplayTargetListExpandedProp;
        private SerializedProperty dtgtShowInEditorProp;
        private SerializedProperty dtgtShowTargetProp;
        private SerializedProperty dtgtReticleColourProp;
        private SerializedProperty dtgtGuidHashDisplayReticleProp;
        private SerializedProperty dtgtShowTargetsOutlineInSceneProp;
        private SerializedProperty dtgtAutoUpdateTargetPositionsProp;
        private SerializedProperty dtgtTargetsViewWidthProp;
        private SerializedProperty dtgtTargetsViewHeightProp;
        private SerializedProperty dtgtTargetsViewOffsetXProp;
        private SerializedProperty dtgtTargetsViewOffsetYProp;
        private SerializedProperty dtgtTargetingRangeProp;
        private SerializedProperty dtgtIsTargetableProp;
        private SerializedProperty dtgtFactionsToIncludeProp;
        private SerializedProperty dtgtSquadronsToIncludeProp;
        private SerializedProperty dtgtMaxNumberOfTargetsProp;

        #endregion

        #region Events

        public void OnEnable()
        {
            shipDisplayModule = (ShipDisplayModule)target;

            shipDisplayModule.ValidateReticleList();
            shipDisplayModule.ValidateMessageList();
            shipDisplayModule.ValidateTargetList();

            #region Find Properties
            showGeneralSettingsInEditorProp = serializedObject.FindProperty("showGeneralSettingsInEditor");
            showReticleSettingsInEditorProp = serializedObject.FindProperty("showReticleSettingsInEditor");
            showAltSpeedSettingsInEditorProp = serializedObject.FindProperty("showAltSpeedSettingsInEditor");
            showHeadingSettingsInEditorProp = serializedObject.FindProperty("showHeadingSettingsInEditor");
            showAttitudeSettingsInEditorProp = serializedObject.FindProperty("showAttitudeSettingsInEditor");
            showFadeSettingsInEditorProp = serializedObject.FindProperty("showFadeSettingsInEditor");
            showFlickerSettingsInEditorProp = serializedObject.FindProperty("showFlickerSettingsInEditor");
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
            showHUDOutlineInSceneProp = serializedObject.FindProperty("showHUDOutlineInScene");
            showActiveDisplayReticleProp = serializedObject.FindProperty("showActiveDisplayReticle");
            lockDisplayReticleToCursorProp = serializedObject.FindProperty("lockDisplayReticleToCursor");
            guidHashActiveDisplayReticleProp = serializedObject.FindProperty("guidHashActiveDisplayReticle");
            displayReticleListProp = serializedObject.FindProperty("displayReticleList");
            activeDisplayReticleColourProp = serializedObject.FindProperty("activeDisplayReticleColour");

            showAltitudeProp = serializedObject.FindProperty("showAltitude");
            showAirspeedProp = serializedObject.FindProperty("showAirspeed");
            sourceShipProp = serializedObject.FindProperty("sourceShip");
            altitudeTextColourProp = serializedObject.FindProperty("altitudeTextColour");
            airspeedTextColourProp = serializedObject.FindProperty("airspeedTextColour");

            showAttitudeProp = serializedObject.FindProperty("showAttitude");
            attitudeScrollSpriteProp = serializedObject.FindProperty("attitudeScrollSprite");
            attitudeMaskSpriteProp = serializedObject.FindProperty("attitudeMaskSprite");
            attitudeHeightProp = serializedObject.FindProperty("attitudeHeight");
            attitudeOffsetXProp = serializedObject.FindProperty("attitudeOffsetX");
            attitudeOffsetYProp = serializedObject.FindProperty("attitudeOffsetY");
            attitudePrimaryColourProp = serializedObject.FindProperty("attitudePrimaryColour");
            attitudeScrollSpriteBorderWidthProp = serializedObject.FindProperty("attitudeScrollSpriteBorderWidth");

            fadeInOnStartProp = serializedObject.FindProperty("fadeInOnStart");
            fadeOutOnStartProp = serializedObject.FindProperty("fadeOutOnStart");
            fadeInDurationProp = serializedObject.FindProperty("fadeInDuration");
            fadeOutDurationProp = serializedObject.FindProperty("fadeOutDuration");
            fadeColourProp = serializedObject.FindProperty("fadeColour");

            flkIsShowHUDWithFlickerProp = serializedObject.FindProperty("isShowHUDWithFlicker");
            flkIsHideHUDWithFlickerProp = serializedObject.FindProperty("isHideHUDWithFlicker");
            flkDefaultDurationProp = serializedObject.FindProperty("flickerDefaultDuration");
            flkMinInactiveTimeProp = serializedObject.FindProperty("flickerMinInactiveTime");
            flkMaxInactiveTimeProp = serializedObject.FindProperty("flickerMaxInactiveTime");
            flkMinActiveTimeProp = serializedObject.FindProperty("flickerMinActiveTime");
            flkMaxActiveTimeProp = serializedObject.FindProperty("flickerMaxActiveTime");
            flkVariableIntensityProp = serializedObject.FindProperty("flickerVariableIntensity");
            flkSmoothingProp = serializedObject.FindProperty("flickerSmoothing");
            flkMaxIntensityProp = serializedObject.FindProperty("flickerMaxIntensity");

            showHeadingProp = serializedObject.FindProperty("showHeading");
            showHeadingIndicatorProp = serializedObject.FindProperty("showHeadingIndicator");
            headingScrollSpriteProp = serializedObject.FindProperty("headingScrollSprite");
            headingMaskSpriteProp = serializedObject.FindProperty("headingMaskSprite");
            headingIndicatorSpriteProp = serializedObject.FindProperty("headingIndicatorSprite");
            headingWidthProp = serializedObject.FindProperty("headingWidth");
            headingOffsetXProp = serializedObject.FindProperty("headingOffsetX");
            headingOffsetYProp = serializedObject.FindProperty("headingOffsetY");
            headingPrimaryColourProp = serializedObject.FindProperty("headingPrimaryColour");
            headingIndicatorColourProp = serializedObject.FindProperty("headingIndicatorColour");

            displayGaugeListProp = serializedObject.FindProperty("displayGaugeList");
            displayMessageListProp = serializedObject.FindProperty("displayMessageList");

            displayTargetListProp = serializedObject.FindProperty("displayTargetList");
            dtgtShowTargetsOutlineInSceneProp = serializedObject.FindProperty("showTargetsOutlineInScene");
            dtgtAutoUpdateTargetPositionsProp = serializedObject.FindProperty("autoUpdateTargetPositions");
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

            sscHelpPDF = SSCEditorHelper.GetHelpURL();

            // Prevent initialising editor at runtime or if clicking on a prefab in the project pane without going into Prefab Mode ("Open Prefab")
            // Clicking on a prefab in asset folder without going into Prefab Mode can create orphaned panels in the scene (no idea why!)
            if (!Application.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode && !SSCEditorHelper.IsPrefabAsset(shipDisplayModule.gameObject))
            {
                shipDisplayModule.InitialiseEditorEssentials();

                // Showing the outlines at runtime doesn't provide any additional benefit.
                // The only benefit would be if it could be rendered in the Gameview
                // Only use if require scene view interaction
                #if UNITY_2019_1_OR_NEWER
                SceneView.duringSceneGui -= SceneGUI;
                SceneView.duringSceneGui += SceneGUI;
                #else
                SceneView.onSceneGUIDelegate -= SceneGUI;
                SceneView.onSceneGUIDelegate += SceneGUI;
                #endif

                hudPanelOutlineColour.a = 0.7f;
                targetsAreaOutlineColour.a = 0.7f;
                shipDisplayModule.RefreshHUD();
            }
        }

        /// <summary>
        /// Called when the gameobject loses focus or Unity Editor enters/exits
        /// play mode
        /// </summary>
        void OnDestroy()
        {
            #if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= SceneGUI;
            #else
            SceneView.onSceneGUIDelegate -= SceneGUI;
            #endif
            
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
            if (shipDisplayModule.allowRepaint) { Repaint(); }
        }

        /// <summary>
        /// Draw gizmos and editable handles in the scene view
        /// </summary>
        /// <param name="sv"></param>
        private void SceneGUI(SceneView sv)
        {
            if (shipDisplayModule != null && shipDisplayModule.gameObject.activeInHierarchy)
            {
                
                if (shipDisplayModule.showHUDOutlineInScene || shipDisplayModule.showTargetsOutlineInScene)
                {
                    float _canvasWidth = shipDisplayModule.CanvasResolution.x * shipDisplayModule.CanvasScaleFactor.x;
                    float _canvasHeight = shipDisplayModule.CanvasResolution.y * shipDisplayModule.CanvasScaleFactor.y;

                    // Show yellow bounding box of current HUD size
                    if (shipDisplayModule.showHUDOutlineInScene)
                    {
                        float _width = _canvasWidth * shipDisplayModule.displayWidth;
                        float _height = _canvasHeight * shipDisplayModule.displayHeight;

                        float _offsetX = shipDisplayModule.displayOffsetX * _canvasWidth;
                        float _offsetY = shipDisplayModule.displayOffsetY * _canvasHeight;

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
                    if (shipDisplayModule.showTargetsOutlineInScene)
                    {
                        float _width = _canvasWidth * shipDisplayModule.targetsViewWidth;
                        float _height = _canvasHeight * shipDisplayModule.targetsViewHeight;

                        float _offsetX = shipDisplayModule.targetsViewOffsetX * _canvasWidth;
                        float _offsetY = shipDisplayModule.targetsViewOffsetY * _canvasHeight;

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

                if (compType == typeof(DisplayReticle))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as DisplayReticle).showInEditor = isExpanded;
                    }
                }
                else if (compType == typeof(DisplayMessage))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as DisplayMessage).showInEditor = isExpanded;
                    }
                }
                else if (compType == typeof(DisplayTarget))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as DisplayTarget).showInEditor = isExpanded;
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

                Undo.RecordObject(shipDisplayModule, "Set Active Reticle");
                shipDisplayModule.guidHashActiveDisplayReticle = objData.x;

                // Update at runtime or in the editor
                if (Application.isPlaying || shipDisplayModule.IsEditorMode)
                {
                    shipDisplayModule.ChangeDisplayReticle(objData.x);
                }
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

                Undo.RecordObject(shipDisplayModule, "Set Display Target Reticle");

                DisplayTarget displayTarget = shipDisplayModule.GetDisplayTarget(objData.x);
                if (displayTarget != null)
                {
                    displayTarget.guidHashDisplayReticle = objData.y;

                    // Update at runtime or in the editor
                    if (Application.isPlaying || shipDisplayModule.IsEditorMode)
                    {
                        shipDisplayModule.SetDisplayTargetReticle(objData.x, objData.y);
                    }
                }
            }
        }

        /// <summary>
        /// Apply any outstanding property changes.
        /// Start and undo group
        /// Record the current state of shipControlModule properties.
        /// PURPOSE: To assist with correctly recording undo events
        /// in the editor (not in play mode). In shipControlModule,
        /// Undo.Record(transform, string.empty) will record the correct
        /// tranform that needs to be undone (this bit isn't currently working correctly)
        /// </summary>
        /// <param name="changeText"></param>
        /// <returns></returns>
        private int ApplyAndRecord(string changeText)
        {
            int _undoGroup = 0;
            if (shipDisplayModule.IsEditorMode)
            {
                serializedObject.ApplyModifiedProperties();
                Undo.SetCurrentGroupName(changeText);
                _undoGroup = UnityEditor.Undo.GetCurrentGroup();
                Undo.RecordObject(shipDisplayModule, string.Empty);
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
        /// Read the current shipControlModule properties.
        /// </summary>
        /// <param name="undoGroup"></param>
        private void ReadProps(int undoGroup)
        {
            if (shipDisplayModule.IsEditorMode)
            {
                Undo.CollapseUndoOperations(undoGroup);
                serializedObject.Update();
            }
            else
            {
                // At runtime, simply read back the properties
            }
        }

        #endregion

        #region OnInspectorGUI

        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there

        public override void OnInspectorGUI()
        {
            #region Initialise

            shipDisplayModule.allowRepaint = false;
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
            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("<b>Sci-Fi Ship Controller</b> Version " + ShipControlModule.SSCVersion + " " + ShipControlModule.SSCBetaVersion, labelFieldRichText);
            GUILayout.EndVertical();

            EditorGUILayout.LabelField(headerContent, helpBoxRichText);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(SSCEditorHelper.btnTxtGetSupport, buttonCompact)) { Application.OpenURL(SSCEditorHelper.urlGetSupport); }
            if (GUILayout.Button(SSCEditorHelper.btnDiscordContent, buttonCompact)) { Application.OpenURL(SSCEditorHelper.urlDiscordChannel); }
            if (GUILayout.Button(SSCEditorHelper.btnHelpContent, buttonCompact)) { Application.OpenURL(sscHelpPDF); }
            if (GUILayout.Button(btnRefreshContent, buttonCompact)) { shipDisplayModule.RefreshHUD(); }
            EditorGUILayout.EndHorizontal();
            #endregion

            #region General Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            SSCEditorHelper.DrawSSCFoldout(showGeneralSettingsInEditorProp, generalSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showGeneralSettingsInEditorProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("initialiseOnStart"), initialiseOnStartContent);
                EditorGUILayout.PropertyField(isShowOnInitialiseProp, isShowOnInitialiseContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(showOverlayProp, showOverlayContent);
                // Always visible in outside of play mode
                if (EditorGUI.EndChangeCheck() && !shipDisplayModule.IsEditorMode && Application.isPlaying)
                {
                    serializedObject.ApplyModifiedProperties();
                    if (showOverlayProp.boolValue) { shipDisplayModule.ShowOverlay(); }
                    else { shipDisplayModule.HideOverlay(); }
                    serializedObject.Update();
                }

                EditorGUILayout.PropertyField(autoHideCursorProp, autoHideCursorContent);
                if (autoHideCursorProp.boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("hideCursorTime"), hideCursorTimeContent);
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("mainCamera"), mainCameraContent);
                EditorGUILayout.PropertyField(sourceShipProp, sourceShipContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(displayWidthProp, displayWidthContent);
                EditorGUILayout.PropertyField(displayHeightProp, displayHeightContent);
                if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                {
                    shipDisplayModule.SetHUDSize(displayWidthProp.floatValue, displayHeightProp.floatValue);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(displayOffsetXProp, displayOffsetXContent);
                EditorGUILayout.PropertyField(displayOffsetYProp, displayOffsetYContent);
                if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                {
                    shipDisplayModule.SetHUDOffset(displayOffsetXProp.floatValue, displayOffsetYProp.floatValue);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(primaryColourProp, primaryColourContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    int _undoGroup = ApplyAndRecord("HUD Set Primary Colour");
                    shipDisplayModule.SetPrimaryColour(primaryColourProp.colorValue);
                    ReadProps(_undoGroup);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(brightnessProp, brightnessContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    shipDisplayModule.SetBrightness(brightnessProp.floatValue);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(canvasSortOrderProp, canvasSortOrderContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    int _undoGroup = ApplyAndRecord("HUD sort order");
                    shipDisplayModule.SetCanvasSortOrder(canvasSortOrderProp.intValue);
                    ReadProps(_undoGroup);
                }

                EditorGUILayout.PropertyField(showHUDOutlineInSceneProp, showHUDOutlineInSceneContent);
            }

            EditorGUILayout.EndVertical();

            #endregion

            #region Display Reticle Settings

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            SSCEditorHelper.DrawSSCFoldout(showReticleSettingsInEditorProp, reticleSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showReticleSettingsInEditorProp.boolValue)
            {
                int numDisplayReticles = displayReticleListProp.arraySize;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(showActiveDisplayReticleProp, showActiveDisplayReticleContent);
                // Always visible outside play mode
                if (EditorGUI.EndChangeCheck() && !shipDisplayModule.IsEditorMode && Application.isPlaying)
                {
                    serializedObject.ApplyModifiedProperties();
                    if (showActiveDisplayReticleProp.boolValue) { shipDisplayModule.ShowDisplayReticle(); }
                    else { shipDisplayModule.HideDisplayReticle(); }
                    serializedObject.Update();
                }

                #region Current Reticle
                int selectedIdx = shipDisplayModule.displayReticleList.FindIndex(dr => dr.guidHash == guidHashActiveDisplayReticleProp.intValue);
                GUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(activeDisplayReticleContent, GUILayout.Width(defaultEditorLabelWidth - 30f));

                if (GUILayout.Button("..", buttonCompact, GUILayout.MaxWidth(20f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();

                    // Create a drop down list of all the locations
                    GenericMenu dropdown = new GenericMenu();
                    dropdown.AddItem(new GUIContent("None"), selectedIdx < 0, UpdateActiveDisplayReticle, new Vector2Int(0, 0));

                    for (int i = 0; i < numDisplayReticles; i++)
                    {
                        // Replace space #/%/& with different chars as Unity treats them as SHIFT/CTRL/ALT in menus.
                        tempDisplayReticleName = shipDisplayModule.displayReticleList[i].primarySprite == null ? "No Texture" : shipDisplayModule.displayReticleList[i].primarySprite.name.Replace(" #", "_#").Replace(" &", " &&").Replace(" %", "_%");
                        dropdown.AddItem(new GUIContent(tempDisplayReticleName), i == selectedIdx, UpdateActiveDisplayReticle, new Vector2Int(shipDisplayModule.displayReticleList[i].guidHash, 0));
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
                    tempDisplayReticleName = shipDisplayModule.displayReticleList[selectedIdx].primarySprite == null ? "no texture in reticle" : shipDisplayModule.displayReticleList[selectedIdx].primarySprite.name;
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
                if (EditorGUI.EndChangeCheck() && !lockDisplayReticleToCursorProp.boolValue && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {

                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(activeDisplayReticleColourProp, activeDisplayReticleColourContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    shipDisplayModule.SetDisplayReticleColour(activeDisplayReticleColourProp.colorValue);
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
                    shipDisplayModule.displayReticleList[0].SetClassDefaults();
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
                    ExpandList(shipDisplayModule.displayReticleList, isDisplayReticleListExpandedProp.boolValue);
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
                    Undo.RecordObject(shipDisplayModule, "Add Display Reticle");
                    shipDisplayModule.displayReticleList.Add(new DisplayReticle());
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

                        shipDisplayModule.displayReticleList.Insert(displayReticleInsertPos, new DisplayReticle(shipDisplayModule.displayReticleList[displayReticleInsertPos]));

                        // Read all properties from the ShipDisplayModule
                        serializedObject.Update();

                        // Hide original DisplayReticle
                        displayReticleListProp.GetArrayElementAtIndex(displayReticleInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                        displayReticleShowInEditorProp = displayReticleListProp.GetArrayElementAtIndex(displayReticleInsertPos).FindPropertyRelative("showInEditor");
                        // Generate a new hashcode for the duplicated DisplayReticle
                        displayReticleListProp.GetArrayElementAtIndex(displayReticleInsertPos).FindPropertyRelative("guidHash").intValue = SSCMath.GetHashCodeFromGuid();

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

            #region Altitude and Speed Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            SSCEditorHelper.DrawSSCFoldout(showAltSpeedSettingsInEditorProp, altSpeedSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showAltSpeedSettingsInEditorProp.boolValue)
            {
                if (!showOverlayProp.boolValue && (showAltitudeProp.boolValue || showAirspeedProp.boolValue))
                {
                    EditorGUILayout.HelpBox(altitudeSpeedWarning, MessageType.Warning);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(showAltitudeProp, showAltitudeContent);
                // Always visible in outside of play mode
                if (EditorGUI.EndChangeCheck() && !shipDisplayModule.IsEditorMode && Application.isPlaying)
                {
                    serializedObject.ApplyModifiedProperties();
                    if (showAltitudeProp.boolValue) { shipDisplayModule.ShowAltitude(); }
                    else { shipDisplayModule.HideAltitude(); }
                    serializedObject.Update();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(showAirspeedProp, showAirspeedContent);
                // Always visible in outside of play mode
                if (EditorGUI.EndChangeCheck() && !shipDisplayModule.IsEditorMode && Application.isPlaying)
                {
                    serializedObject.ApplyModifiedProperties();
                    if (showAirspeedProp.boolValue) { shipDisplayModule.ShowAirSpeed(); }
                    else { shipDisplayModule.HideAirSpeed(); }
                    serializedObject.Update();
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("groundPlaneHeight"), groundPlaneHeightContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(altitudeTextColourProp, altitudeTextColourContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    int _undoGroup = ApplyAndRecord("HUD Altitude Colour");
                    shipDisplayModule.SetAltitudeTextColour(altitudeTextColourProp.colorValue);
                    ReadProps(_undoGroup);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(airspeedTextColourProp, airspeedTextColourContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    int _undoGroup = ApplyAndRecord("HUD Air Speed Colour");
                    shipDisplayModule.SetAirSpeedTextColour(airspeedTextColourProp.colorValue);
                    ReadProps(_undoGroup);
                }
            }
            EditorGUILayout.EndVertical();
            #endregion

            #region Attitude Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            SSCEditorHelper.DrawSSCFoldout(showAttitudeSettingsInEditorProp, displayAttitudeSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showAttitudeSettingsInEditorProp.boolValue)
            {
                SSCEditorHelper.InTechPreview(true);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(showAttitudeProp, showAttitudeContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    serializedObject.ApplyModifiedProperties();
                    if (showAttitudeProp.boolValue) { shipDisplayModule.ShowAttitude(); }
                    else { shipDisplayModule.HideAttitude(); }
                    serializedObject.Update();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(attitudeScrollSpriteProp, attitudeScrollSpriteContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    serializedObject.ApplyModifiedProperties();
                    if (attitudeScrollSpriteProp.objectReferenceValue != null)
                    {
                        shipDisplayModule.SetDisplayAttitudeScrollSprite((Sprite)attitudeScrollSpriteProp.objectReferenceValue);
                    }
                    else
                    {
                        shipDisplayModule.SetDisplayAttitudeScrollSprite(null);
                    }
                    
                    serializedObject.Update();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(attitudeMaskSpriteProp, attitudeMaskSpriteContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    serializedObject.ApplyModifiedProperties();
                    if (attitudeMaskSpriteProp.objectReferenceValue != null)
                    {
                        shipDisplayModule.SetDisplayAttitudeMaskSprite((Sprite)attitudeMaskSpriteProp.objectReferenceValue);
                    }
                    else
                    {
                        shipDisplayModule.SetDisplayAttitudeMaskSprite(null);
                    }
                    
                    serializedObject.Update();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(attitudeHeightProp, attitudeHeightContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    int _undoGroup = ApplyAndRecord("Attitude Mask Size");
                    shipDisplayModule.SetDisplayAttitudeSize(1f, attitudeHeightProp.floatValue);
                    ReadProps(_undoGroup);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(attitudeOffsetXProp, attitudeOffsetXContent);
                EditorGUILayout.PropertyField(attitudeOffsetYProp, attitudeOffsetYContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    int _undoGroup = ApplyAndRecord("Attitude Offset");
                    shipDisplayModule.SetDisplayAttitudeOffset(attitudeOffsetXProp.floatValue, attitudeOffsetYProp.floatValue);
                    ReadProps(_undoGroup);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(attitudePrimaryColourProp, attitudePrimaryColourContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    int _undoGroup = ApplyAndRecord("Display Attitude Primary Colour");
                    shipDisplayModule.SetDisplayAttitudePrimaryColour(attitudePrimaryColourProp.colorValue);
                    ReadProps(_undoGroup);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(attitudeScrollSpriteBorderWidthProp, attitudeScrollSpriteBorderWidthPropContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    int _undoGroup = ApplyAndRecord("Attitude Border Width");
                    shipDisplayModule.SetDisplayAttitudeSpriteBorderWidth(attitudeScrollSpriteBorderWidthProp.floatValue);
                    ReadProps(_undoGroup);
                }
            }
            EditorGUILayout.EndVertical();
            #endregion

            #region Fade Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            SSCEditorHelper.DrawSSCFoldout(showFadeSettingsInEditorProp, displayFadeSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showFadeSettingsInEditorProp.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(fadeInOnStartProp, fadeInOnStartContent);
                if (EditorGUI.EndChangeCheck())
                {
                    if (fadeInOnStartProp.boolValue && fadeOutOnStartProp.boolValue)
                    {
                        fadeOutOnStartProp.boolValue = false;
                    }
                }
                EditorGUILayout.PropertyField(fadeInDurationProp, fadeInDurationContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(fadeOutOnStartProp, fadeOutOnStartContent);
                if (EditorGUI.EndChangeCheck())
                {
                    if (fadeInOnStartProp.boolValue && fadeOutOnStartProp.boolValue)
                    {
                        fadeInOnStartProp.boolValue = false;
                    }
                }
                EditorGUILayout.PropertyField(fadeOutDurationProp, fadeOutDurationContent);

                EditorGUILayout.PropertyField(fadeColourProp, fadeColourContent);
            }
            EditorGUILayout.EndVertical();
            #endregion

            #region Flicker Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            SSCEditorHelper.DrawSSCFoldout(showFlickerSettingsInEditorProp, displayFlickerSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showFlickerSettingsInEditorProp.boolValue)
            {
                EditorGUILayout.PropertyField(flkIsShowHUDWithFlickerProp, flkIsShowHUDWithFlickerContent);
                EditorGUILayout.PropertyField(flkIsHideHUDWithFlickerProp, flkIsHideHUDWithFlickerContent);

                EditorGUILayout.PropertyField(flkDefaultDurationProp, flkDefaultDurationContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(flkMinInactiveTimeProp, flkMinInactiveTimeContent);
                if (EditorGUI.EndChangeCheck())
                {
                    if (flkMinInactiveTimeProp.floatValue > flkMaxInactiveTimeProp.floatValue) { flkMaxInactiveTimeProp.floatValue = flkMinInactiveTimeProp.floatValue; }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(flkMaxInactiveTimeProp, flkMaxInactiveTimeContent);
                if (EditorGUI.EndChangeCheck())
                {
                    if (flkMaxInactiveTimeProp.floatValue < flkMinInactiveTimeProp.floatValue) { flkMinInactiveTimeProp.floatValue = flkMaxInactiveTimeProp.floatValue; }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(flkMinActiveTimeProp, flkMinActiveTimeContent);
                if (EditorGUI.EndChangeCheck())
                {
                    if (flkMinActiveTimeProp.floatValue > flkMaxActiveTimeProp.floatValue) { flkMaxActiveTimeProp.floatValue = flkMinActiveTimeProp.floatValue; }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(flkMaxActiveTimeProp, flkMaxActiveTimeContent);
                if (EditorGUI.EndChangeCheck())
                {
                    if (flkMaxActiveTimeProp.floatValue < flkMinActiveTimeProp.floatValue) { flkMinActiveTimeProp.floatValue = flkMaxActiveTimeProp.floatValue; }
                }

                EditorGUILayout.PropertyField(flkVariableIntensityProp, flkVariableIntensityContent);
                if (flkVariableIntensityProp.boolValue)
                {
                    EditorGUILayout.PropertyField(flkSmoothingProp, flkSmoothingContent);
                    EditorGUILayout.PropertyField(flkMaxIntensityProp, flkMaxIntensityContent);
                }
            }

            EditorGUILayout.EndVertical();
            #endregion

            #region Heading Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            SSCEditorHelper.DrawSSCFoldout(showHeadingSettingsInEditorProp, displayHeadingSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);
            if (showHeadingSettingsInEditorProp.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(showHeadingProp, showHeadingContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    serializedObject.ApplyModifiedProperties();
                    if (showHeadingProp.boolValue) { shipDisplayModule.ShowHeading(); }
                    else { shipDisplayModule.HideHeading(); }
                    serializedObject.Update();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(showHeadingIndicatorProp, showHeadingIndicatorContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    serializedObject.ApplyModifiedProperties();
                    if (showHeadingProp.boolValue) { shipDisplayModule.ShowHeadingIndicator(); }
                    else { shipDisplayModule.HideHeadingIndicator(); }
                    serializedObject.Update();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(headingScrollSpriteProp, headingScrollSpriteContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    serializedObject.ApplyModifiedProperties();
                    if (headingScrollSpriteProp.objectReferenceValue != null)
                    {
                        shipDisplayModule.SetDisplayHeadingScrollSprite((Sprite)headingScrollSpriteProp.objectReferenceValue);
                    }
                    else
                    {
                        shipDisplayModule.SetDisplayHeadingScrollSprite(null);
                    }
                    
                    serializedObject.Update();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(headingMaskSpriteProp, headingMaskSpriteContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    serializedObject.ApplyModifiedProperties();
                    if (headingMaskSpriteProp.objectReferenceValue != null)
                    {
                        shipDisplayModule.SetDisplayHeadingMaskSprite((Sprite)headingMaskSpriteProp.objectReferenceValue);
                    }
                    else
                    {
                        shipDisplayModule.SetDisplayHeadingMaskSprite(null);
                    }
                    
                    serializedObject.Update();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(headingIndicatorSpriteProp, headingIndicatorSpriteContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    serializedObject.ApplyModifiedProperties();
                    if (headingIndicatorSpriteProp.objectReferenceValue != null)
                    {
                        shipDisplayModule.SetDisplayHeadingIndicatorSprite((Sprite)headingIndicatorSpriteProp.objectReferenceValue);
                    }
                    else
                    {
                        shipDisplayModule.SetDisplayHeadingIndicatorSprite(null);
                    }

                    serializedObject.Update();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(headingWidthProp, headingWidthContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    int _undoGroup = ApplyAndRecord("Heading Mask Size");
                    shipDisplayModule.SetDisplayHeadingSize(headingWidthProp.floatValue, 1f);
                    ReadProps(_undoGroup);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(headingOffsetXProp, headingOffsetXContent);
                EditorGUILayout.PropertyField(headingOffsetYProp, headingOffsetYContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    int _undoGroup = ApplyAndRecord("Heading Offset");
                    shipDisplayModule.SetDisplayHeadingOffset(headingOffsetXProp.floatValue, headingOffsetYProp.floatValue);
                    ReadProps(_undoGroup);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(headingPrimaryColourProp, headingPrimaryColourContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    int _undoGroup = ApplyAndRecord("Display Heading Primary Colour");
                    shipDisplayModule.SetDisplayHeadingPrimaryColour(headingPrimaryColourProp.colorValue);
                    ReadProps(_undoGroup);
                }
                
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(headingIndicatorColourProp, headingIndicatorColourContent);
                if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                {
                    int _undoGroup = ApplyAndRecord("Display Heading Indicator Colour");
                    shipDisplayModule.SetDisplayHeadingIndicatorColour(headingIndicatorColourProp.colorValue);
                    ReadProps(_undoGroup);
                }
            }
            EditorGUILayout.EndVertical();
            #endregion

            #region Message Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            SSCEditorHelper.DrawSSCFoldout(showMessageSettingsInEditorProp, displayMessageSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

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
                SSCEditorHelper.DrawSSCFoldout(isDisplayMessageListExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(shipDisplayModule.displayMessageList, isDisplayMessageListExpandedProp.boolValue);
                    // Read in the properties
                    serializedObject.Update();
                }

                EditorGUILayout.LabelField("Display Messages: " + numDisplayMessages.ToString("00"), labelFieldRichText);

                if (GUILayout.Button(dmImportJsonContent, GUILayout.MaxWidth(65f)))
                {
                    string importPath = string.Empty, importFileName = string.Empty;
                    if (SSCEditorHelper.GetFilePathFromUser("Import Display Message", SSCSetup.sscFolder, new string[] { "JSON", "json" }, false, ref importPath, ref importFileName))
                    {
                        DisplayMessage importedDisplayMessage = shipDisplayModule.ImportMessageFromJson(importPath, importFileName);
                        if (importedDisplayMessage != null)
                        {
                            serializedObject.ApplyModifiedProperties();
                            Undo.RecordObject(shipDisplayModule, "Import Message");

                            shipDisplayModule.AddMessage(importedDisplayMessage);
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
                    }
                }

                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(shipDisplayModule, "Add Display Message");
                    shipDisplayModule.AddMessage("New Message","New Message Text");
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
                    dmsgMessageNameProp = dmsgProp.FindPropertyRelative("messageName");
                    #endregion

                    #region Display Message Move/Insert/Delete buttons
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel += 1;
                    dmsgShowInEditorProp.boolValue = EditorGUILayout.Foldout(dmsgShowInEditorProp.boolValue, "Display Message " + (dmIdx + 1).ToString("00"));
                    EditorGUI.indentLevel -= 1;

                    if (GUILayout.Button(dmExportJsonContent, buttonCompact, GUILayout.MaxWidth(80f)))
                    {
                        string exportPath = EditorUtility.SaveFilePanel("Save Message", "Assets",string.IsNullOrEmpty(dmsgMessageNameProp.stringValue) ? "Message " + (dmIdx + 1).ToString("00") : dmsgMessageNameProp.stringValue, "json");

                        if (shipDisplayModule.SaveMessageAsJson(shipDisplayModule.displayMessageList[dmIdx], exportPath))
                        {
                            // Check if path is in Project folder
                            if (exportPath.Contains(Application.dataPath))
                            {
                                // Get the folder to highlight in the Project folder
                                string folderPath = "Assets" + System.IO.Path.GetDirectoryName(exportPath).Replace(Application.dataPath, "");

                                // Get the json file in the Project folder
                                exportPath = "Assets" + exportPath.Replace(Application.dataPath, "");
                                AssetDatabase.ImportAsset(exportPath);

                                SSCEditorHelper.HighlightFolderInProjectWindow(folderPath, true, true);
                            }
                            Debug.Log("Message exported to " + exportPath);
                        }

                        GUIUtility.ExitGUI();
                    }

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
                        dmsgFadeinDurationProp = dmsgProp.FindPropertyRelative("fadeinDuration");
                        dmsgFadeoutDurationProp = dmsgProp.FindPropertyRelative("fadeoutDuration");
                        #endregion

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgShowMessageProp, dmShowMessageSettingsContent);
                        // Always visible outside play mode
                        if (EditorGUI.EndChangeCheck() && !shipDisplayModule.IsEditorMode && Application.isPlaying)
                        {
                            serializedObject.ApplyModifiedProperties();
                            if (dmsgShowMessageProp.boolValue) { shipDisplayModule.ShowDisplayMessage(shipDisplayModule.displayMessageList[dmIdx]); }
                            else { shipDisplayModule.HideDisplayMessage(shipDisplayModule.displayMessageList[dmIdx]); }
                            serializedObject.Update();
                        }

                        EditorGUILayout.PropertyField(dmsgMessageNameProp, dmMessageNameSettingsContent);
                        
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgMessageStringProp, dmMessageStringSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Message Text");
                            shipDisplayModule.SetDisplayMessageText(shipDisplayModule.displayMessageList[dmIdx], dmsgMessageStringProp.stringValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        if (!dmsgIsScrollFullscreenProp.boolValue || (dmsgScrollDirectionProp.intValue != DisplayMessage.ScrollDirectionLR && dmsgScrollDirectionProp.intValue != DisplayMessage.ScrollDirectionRL))
                        {
                            EditorGUILayout.PropertyField(dmsgOffsetXProp, dmOffsetXSettingsContent);
                        }
                        if (!dmsgIsScrollFullscreenProp.boolValue || (dmsgScrollDirectionProp.intValue != DisplayMessage.ScrollDirectionBT && dmsgScrollDirectionProp.intValue != DisplayMessage.ScrollDirectionTB))
                        {
                            EditorGUILayout.PropertyField(dmsgOffsetYProp, dmOffsetYSettingsContent);
                        }
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Message Offset");
                            shipDisplayModule.SetDisplayMessageOffset(shipDisplayModule.displayMessageList[dmIdx], dmsgOffsetXProp.floatValue, dmsgOffsetYProp.floatValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgDisplayWidthProp, dmDisplayWidthSettingsContent);
                        EditorGUILayout.PropertyField(dmsgDisplayHeightProp, dmDisplayHeightSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Message Size");
                            shipDisplayModule.SetDisplayMessageSize(shipDisplayModule.displayMessageList[dmIdx], dmsgDisplayWidthProp.floatValue, dmsgDisplayHeightProp.floatValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUILayout.PropertyField(dmsgFadeinDurationProp, dmFadeInDurationContent);
                        EditorGUILayout.PropertyField(dmsgFadeoutDurationProp, dmFadeOutDurationContent);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgShowBackgroundProp, dmShowBackgroundSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Message Show Background");
                            if (dmsgShowBackgroundProp.boolValue) { shipDisplayModule.ShowDisplayMessageBackground(shipDisplayModule.displayMessageList[dmIdx]); }
                            else  { shipDisplayModule.HideDisplayMessageBackground(shipDisplayModule.displayMessageList[dmIdx]); }
                            ReadProps(_undoGroup);
                        }

                        if (dmsgShowBackgroundProp.boolValue)
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(dmsgBackgroundColourProp, dmBackgroundColourSettingsContent);
                            if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                            {
                                int _undoGroup = ApplyAndRecord("Display Message Background Colour");
                                shipDisplayModule.SetDisplayMessageBackgroundColour(shipDisplayModule.displayMessageList[dmIdx], dmsgBackgroundColourProp.colorValue);
                                ReadProps(_undoGroup);
                            }
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgTextColourProp, dmTextColourSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Message Text Colour");
                            shipDisplayModule.SetDisplayMessageTextColour(shipDisplayModule.displayMessageList[dmIdx], dmsgTextColourProp.colorValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgTextAlignmentProp, dmTextAlignmentSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Message Alignment");
                            shipDisplayModule.SetDisplayMessageTextAlignment(shipDisplayModule.displayMessageList[dmIdx], (TextAnchor)dmsgTextAlignmentProp.enumValueIndex);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgIsBestFitProp, dmTextIsBestFitSettingsContent);
                        if (dmsgIsBestFitProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(dmsgFontMinSizeProp, dmTextFontMinSizeSettingsContent);
                        }
                        EditorGUILayout.PropertyField(dmsgFontMaxSizeProp, dmTextFontMaxSizeSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            //int _undoGroup = ApplyAndRecord("Display Message Font Size");
                            shipDisplayModule.SetDisplayMessageTextFontSize(shipDisplayModule.displayMessageList[dmIdx], dmsgIsBestFitProp.boolValue, dmsgFontMinSizeProp.intValue, dmsgFontMaxSizeProp.intValue);
                            //ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dmsgScrollDirectionProp, dmScrollDirectionSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            shipDisplayModule.SetDisplayMessageScrollDirection(shipDisplayModule.displayMessageList[dmIdx], dmsgScrollDirectionProp.intValue);
                        }

                        if (dmsgScrollDirectionProp.intValue != DisplayMessage.ScrollDirectionNone)
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
                        shipDisplayModule.RefreshMessagesSortOrder();
                        serializedObject.Update();                       

                        displayMessageMoveDownPos = -1;
                    }
                    else if (displayMessageInsertPos >= 0)
                    {
                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        Undo.SetCurrentGroupName("Insert Display Message");
                        int _undoGroup = UnityEditor.Undo.GetCurrentGroup();
                        Undo.RecordObject(shipDisplayModule, string.Empty);

                        DisplayMessage insertedMessage = new DisplayMessage(shipDisplayModule.displayMessageList[displayMessageInsertPos]);
                        insertedMessage.showInEditor = true;
                        // Generate a new hashcode for the duplicated DisplayMessage
                        insertedMessage.guidHash = SSCMath.GetHashCodeFromGuid();

                        shipDisplayModule.displayMessageList.Insert(displayMessageInsertPos, insertedMessage);
                        // Hide original DisplayMessage
                        shipDisplayModule.displayMessageList[displayMessageInsertPos + 1].showInEditor = false;
                        RectTransform insertedMessagePanel = shipDisplayModule.CreateMessagePanel(insertedMessage, shipDisplayModule.GetHUDPanel());
                        if (insertedMessagePanel != null)
                        {
                            // Make sure we can rollback the creation of the new message panel
                            Undo.RegisterCreatedObjectUndo(insertedMessagePanel.gameObject, string.Empty);
                            shipDisplayModule.RefreshMessagesSortOrder();
                        }

                        Undo.CollapseUndoOperations(_undoGroup);

                        // Read all properties from the ShipDisplayModule
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

                            shipDisplayModule.DeleteMessage(_guidHash);

                            displayMessageDeletePos = -1;

                            serializedObject.Update();

                            // Force shipDisplayModule to be serialized after a delete
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
            SSCEditorHelper.DrawSSCFoldout(showTargetSettingsInEditorProp, dtgtSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showTargetSettingsInEditorProp.boolValue)
            {
                #region Target Common Settings
                EditorGUILayout.LabelField(dtgtHeaderContent, helpBoxRichText);
                EditorGUILayout.PropertyField(dtgtAutoUpdateTargetPositionsProp, dtgtAutoUpdateTargetPositionsContent);
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
                SSCEditorHelper.DrawSSCFoldout(isDisplayTargetListExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(shipDisplayModule.displayTargetList, isDisplayTargetListExpandedProp.boolValue);
                    // Read in the properties
                    serializedObject.Update();
                }

                EditorGUILayout.LabelField("Display Targets: " + numDisplayTargets.ToString("00"), labelFieldRichText);

                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    int _undoGroup = ApplyAndRecord("Add Display Target");
                    shipDisplayModule.AddTarget(0);
                    isSceneModified = true;
                    ReadProps(_undoGroup);

                    // For some reason the displayTargetListProp doesn't get updated after the serializedObject.Update().
                    // This causes an issue with shipDisplayModule.displayTargetList and shipDisplayModule.UpdateTargetPositions()
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
                int _numDisplayReticles = shipDisplayModule.displayReticleList == null ? 0 : shipDisplayModule.displayReticleList.Count;

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

                        dtgtIsTargetableProp = dtgtProp.FindPropertyRelative("isTargetable");
                        dtgtFactionsToIncludeProp = dtgtProp.FindPropertyRelative("factionsToInclude");
                        dtgtSquadronsToIncludeProp = dtgtProp.FindPropertyRelative("squadronsToInclude");
                        dtgtMaxNumberOfTargetsProp = dtgtProp.FindPropertyRelative("maxNumberOfTargets");

                        #endregion

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dtgtShowTargetProp, dtgtShowTargetContent);
                        // Always visible outside play mode
                        if (EditorGUI.EndChangeCheck() && !shipDisplayModule.IsEditorMode && Application.isPlaying)
                        {
                            serializedObject.ApplyModifiedProperties();
                            if (dtgtShowTargetProp.boolValue) { shipDisplayModule.ShowDisplayTarget(shipDisplayModule.displayTargetList[dtgtIdx]); }
                            else { shipDisplayModule.HideDisplayTarget(shipDisplayModule.displayTargetList[dtgtIdx]); }
                            serializedObject.Update();
                        }

                        // Look up the reticle for this Display Target
                        int _selectedIdx = shipDisplayModule.displayReticleList.FindIndex(dr => dr.guidHash == dtgtGuidHashDisplayReticleProp.intValue);

                        GUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField(dtgtDisplayReticleContent, GUILayout.Width(defaultEditorLabelWidth - 30f));

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
                                tempDisplayReticleName = shipDisplayModule.displayReticleList[i].primarySprite == null ? "No Texture" : shipDisplayModule.displayReticleList[i].primarySprite.name.Replace(" #", "_#").Replace(" &", " &&").Replace(" %", "_%");
                                dropdown.AddItem(new GUIContent(tempDisplayReticleName), i == _selectedIdx, UpdateDisplayTargetReticle, new Vector2Int(_guidHashDisplayTarget, shipDisplayModule.displayReticleList[i].guidHash));
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
                            tempDisplayReticleName = shipDisplayModule.displayReticleList[_selectedIdx].primarySprite == null ? "no texture in reticle" : shipDisplayModule.displayReticleList[_selectedIdx].primarySprite.name;
                            EditorGUILayout.LabelField(string.IsNullOrEmpty(tempDisplayReticleName) ? "no texture name" : tempDisplayReticleName, GUILayout.MaxWidth(100f));

                            tempSprite = shipDisplayModule.displayReticleList[_selectedIdx].primarySprite;
                        }
                        GUILayout.EndHorizontal();

                        SSCEditorHelper.DrawSprite(spriteContent, tempSprite, 24, defaultEditorLabelWidth  - 4f);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dtgtReticleColourProp, dtgtReticleColourContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Target Colour");
                            shipDisplayModule.SetDisplayTargetReticleColour(shipDisplayModule.displayTargetList[dtgtIdx], dtgtReticleColourProp.colorValue);
                            ReadProps(_undoGroup);
                        }

                        if (shipDisplayModule.displayTargetList[dtgtIdx].factionsToInclude == null)
                        {
                            serializedObject.ApplyModifiedProperties();
                            shipDisplayModule.displayTargetList[dtgtIdx].factionsToInclude = new int[0];
                            serializedObject.Update();
                        }

                        if (shipDisplayModule.displayTargetList[dtgtIdx].squadronsToInclude == null)
                        {
                            serializedObject.ApplyModifiedProperties();
                            shipDisplayModule.displayTargetList[dtgtIdx].squadronsToInclude = new int[0];
                            serializedObject.Update();
                        }

                        EditorGUILayout.PropertyField(dtgtIsTargetableProp, dtgtIsTargetableContent);

                        SSCEditorHelper.DrawArray(dtgtFactionsToIncludeProp, dtgtFactionsToIncludeContent, defaultEditorLabelWidth, "Faction");
                        SSCEditorHelper.DrawArray(dtgtSquadronsToIncludeProp, dtgtSquadronsToIncludeContent, defaultEditorLabelWidth, "Squadron");

                        #region Add and Remove Slots
                        int numSlots = dtgtMaxNumberOfTargetsProp.intValue;
                        EditorGUI.BeginChangeCheck();
                        numSlots = EditorGUILayout.IntSlider(dtgtMaxNumberOfTargetsContent, numSlots, 1, DisplayTarget.MAX_DISPLAYTARGET_SLOTS);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            // Add or remove DisplayTarget Slots
                            if (numSlots > dtgtMaxNumberOfTargetsProp.intValue)
                            {
                                int _undoGroup = ApplyAndRecord("Add Display Target Slots");
                                shipDisplayModule.AddTargetSlots(shipDisplayModule.displayTargetList[dtgtIdx], numSlots - dtgtMaxNumberOfTargetsProp.intValue);
                                ReadProps(_undoGroup);
                            }
                            else if (numSlots < dtgtMaxNumberOfTargetsProp.intValue)
                            {
                                int _undoGroup = ApplyAndRecord("Delete Display Target Slots");
                                shipDisplayModule.DeleteTargetSlots(dtgtProp.FindPropertyRelative("guidHash").intValue, dtgtMaxNumberOfTargetsProp.intValue - numSlots);
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
                        shipDisplayModule.RefreshDisplayTargetSlots();

                        shipDisplayModule.RefreshTargetsSortOrder();
                        serializedObject.Update();                       

                        displayTargetMoveDownPos = -1;
                    }
                    else if (displayTargetInsertPos >= 0)
                    {
                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        Undo.SetCurrentGroupName("Insert Display Target");
                        int _undoGroup = UnityEditor.Undo.GetCurrentGroup();
                        Undo.RecordObject(shipDisplayModule, string.Empty);

                        DisplayTarget insertedTarget = new DisplayTarget(shipDisplayModule.displayTargetList[displayTargetInsertPos]);
                        insertedTarget.showInEditor = true;
                        // Generate a new hashcode for the duplicated DisplayTarget
                        insertedTarget.guidHash = SSCMath.GetHashCodeFromGuid();
                        insertedTarget.maxNumberOfTargets = 1;

                        shipDisplayModule.displayTargetList.Insert(displayTargetInsertPos, insertedTarget);
                        // Hide original DisplayTarget
                        shipDisplayModule.displayTargetList[displayTargetInsertPos + 1].showInEditor = false;
                        RectTransform insertedTargetPanel = shipDisplayModule.CreateTargetPanel(insertedTarget, 0);
                        if (insertedTargetPanel != null)
                        {
                            // Update the reticle to force it to take effect on the inserted (new) Target
                            shipDisplayModule.SetDisplayTargetReticle(insertedTarget.guidHash, insertedTarget.guidHashDisplayReticle);

                            // Make sure we can rollback the creation of the new target panel
                            Undo.RegisterCreatedObjectUndo(insertedTargetPanel.gameObject, string.Empty);
                            shipDisplayModule.RefreshTargetsSortOrder();
                        }

                        Undo.CollapseUndoOperations(_undoGroup);

                        // Read all properties from the ShipDisplayModule
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

                            shipDisplayModule.DeleteTarget(_guidHash);

                            displayTargetDeletePos = -1;

                            serializedObject.Update();

                            // Force shipDisplayModule to be serialized after a delete
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
            SSCEditorHelper.DrawSSCFoldout(showGaugeSettingsInEditorProp, displayGaugeSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

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
                SSCEditorHelper.DrawSSCFoldout(isDisplayGaugeListExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(shipDisplayModule.displayGaugeList, isDisplayGaugeListExpandedProp.boolValue);
                    // Read in the properties
                    serializedObject.Update();
                }

                EditorGUILayout.LabelField("Display Gauges: " + numDisplayGauges.ToString("00"), labelFieldRichText);

                if (GUILayout.Button(dgImportJsonContent, GUILayout.MaxWidth(65f)))
                {
                    string importPath = string.Empty, importFileName = string.Empty;
                    if (SSCEditorHelper.GetFilePathFromUser("Import Display Gauge", SSCSetup.sscFolder, new string[] { "JSON", "json" }, false, ref importPath, ref importFileName))
                    {
                        DisplayGauge importedDisplayGauge = shipDisplayModule.ImportGaugeFromJson(importPath, importFileName);
                        if (importedDisplayGauge != null)
                        {
                            serializedObject.ApplyModifiedProperties();
                            Undo.RecordObject(shipDisplayModule, "Import Gauge");

                            shipDisplayModule.AddGauge(importedDisplayGauge);
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
                    }
                }

                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(shipDisplayModule, "Add Display Gauge");
                    shipDisplayModule.AddGauge("New Gauge","New Gauge Text");
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
                    dgauGaugeNameProp = dgauProp.FindPropertyRelative("gaugeName");
                    #endregion

                    #region Display Gauge Export/Move/Insert/Delete buttons
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel += 1;
                    dgauShowInEditorProp.boolValue = EditorGUILayout.Foldout(dgauShowInEditorProp.boolValue, "Display Gauge " + (dgIdx + 1).ToString("00"));
                    EditorGUI.indentLevel -= 1;

                    if (GUILayout.Button(dgExportJsonContent, buttonCompact, GUILayout.MaxWidth(80f)))
                    {
                        string exportPath = EditorUtility.SaveFilePanel("Save Gauge", "Assets",string.IsNullOrEmpty(dgauGaugeNameProp.stringValue) ? "Gauge " + (dgIdx + 1).ToString("00") : dgauGaugeNameProp.stringValue, "json");

                        if (shipDisplayModule.SaveGaugeAsJson(shipDisplayModule.displayGaugeList[dgIdx], exportPath))
                        {
                            // Check if path is in Project folder
                            if (exportPath.Contains(Application.dataPath))
                            {
                                // Get the folder to highlight in the Project folder
                                string folderPath = "Assets" + System.IO.Path.GetDirectoryName(exportPath).Replace(Application.dataPath, "");

                                // Get the json file in the Project folder
                                exportPath = "Assets" + exportPath.Replace(Application.dataPath, "");
                                AssetDatabase.ImportAsset(exportPath);

                                SSCEditorHelper.HighlightFolderInProjectWindow(folderPath, true, true);
                            }
                            Debug.Log("Gauge exported to " + exportPath);
                        }

                        GUIUtility.ExitGUI();
                    }

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
                        dgauGaugeTypeProp = dgauProp.FindPropertyRelative("gaugeType");
                        dgauOffsetXProp = dgauProp.FindPropertyRelative("offsetX");
                        dgauOffsetYProp = dgauProp.FindPropertyRelative("offsetY");
                        dgauDisplayWidthProp = dgauProp.FindPropertyRelative("displayWidth");
                        dgauDisplayHeightProp = dgauProp.FindPropertyRelative("displayHeight");
                        dgauIsColourAffectedByValueProp = dgauProp.FindPropertyRelative("isColourAffectByValue");
                        dgauMediumColourValueProp = dgauProp.FindPropertyRelative("mediumColourValue");
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
                        dgauGaugeLabelProp = dgauProp.FindPropertyRelative("gaugeLabel");
                        dgauTextAlignmentProp = dgauProp.FindPropertyRelative("textAlignment");
                        dgauLabelAlignmentProp = dgauProp.FindPropertyRelative("labelAlignment");
                        dgauTextDirectionProp = dgauProp.FindPropertyRelative("textDirection");
                        dgauTextFontStyleProp = dgauProp.FindPropertyRelative("fontStyle");
                        dgauIsBestFitProp = dgauProp.FindPropertyRelative("isBestFit");
                        dgauFontMinSizeProp = dgauProp.FindPropertyRelative("fontMinSize");
                        dgauFontMaxSizeProp = dgauProp.FindPropertyRelative("fontMaxSize");
                        dgauMaxValueProp = dgauProp.FindPropertyRelative("gaugeMaxValue");
                        dgauIsNumericPercentageProp = dgauProp.FindPropertyRelative("isNumericPercentage");
                        dgauDecimalPlacesProp = dgauProp.FindPropertyRelative("gaugeDecimalPlaces");
                        #endregion

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauShowGaugeProp, dgShowGaugeSettingsContent);
                        // Always visible outside play mode
                        if (EditorGUI.EndChangeCheck() && !shipDisplayModule.IsEditorMode && Application.isPlaying)
                        {
                            serializedObject.ApplyModifiedProperties();
                            if (dgauShowGaugeProp.boolValue) { shipDisplayModule.ShowDisplayGauge(shipDisplayModule.displayGaugeList[dgIdx]); }
                            else { shipDisplayModule.HideDisplayGauge(shipDisplayModule.displayGaugeList[dgIdx]); }
                            serializedObject.Update();
                        }

                        EditorGUILayout.PropertyField(dgauGaugeNameProp, dgGaugeNameSettingsContent);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauGaugeTypeProp, dgGaugeTypeSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Type");
                            shipDisplayModule.SetDisplayGaugeType(shipDisplayModule.displayGaugeList[dgIdx], (DisplayGauge.DGType)dgauGaugeTypeProp.intValue);
                            ReadProps(_undoGroup);
                        }

                        gaugeType = (DisplayGauge.DGType)dgauGaugeTypeProp.intValue;

                        if (gaugeType == DisplayGauge.DGType.Filled)
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(dgauGaugeStringProp, dgGaugeStringSettingsContent);
                            if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                            {
                                int _undoGroup = ApplyAndRecord("Display Gauge Text");
                                shipDisplayModule.SetDisplayGaugeText(shipDisplayModule.displayGaugeList[dgIdx], dgauGaugeStringProp.stringValue);
                                ReadProps(_undoGroup);
                            }
                        }

                        if (gaugeType == DisplayGauge.DGType.NumberWithLabel1)
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(dgauGaugeLabelProp, dgGaugeLabelSettingsContent);
                            if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                            {
                                int _undoGroup = ApplyAndRecord("Display Gauge Label");
                                shipDisplayModule.SetDisplayGaugeLabel(shipDisplayModule.displayGaugeList[dgIdx], dgauGaugeLabelProp.stringValue);
                                ReadProps(_undoGroup);
                            }
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauGaugeValueProp, dgGaugeValueSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Value");
                            shipDisplayModule.SetDisplayGaugeValue(shipDisplayModule.displayGaugeList[dgIdx], dgauGaugeValueProp.floatValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauOffsetXProp, dgOffsetXSettingsContent);
                        EditorGUILayout.PropertyField(dgauOffsetYProp, dgOffsetYSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Offset");
                            shipDisplayModule.SetDisplayGaugeOffset(shipDisplayModule.displayGaugeList[dgIdx], dgauOffsetXProp.floatValue, dgauOffsetYProp.floatValue);
                            ReadProps(_undoGroup);
                        }

                        /// TODO - fix left/right top/bottom offset Draw method - has a few bugs
                        //bool hasChanged = SSCEditorHelper.DrawOffsetLeftRight(dgauOffsetXProp, dgauDisplayWidthProp, defaultEditorLabelWidth);
                        //hasChanged = SSCEditorHelper.DrawOffsetTopBottom(dgauOffsetYProp, dgauDisplayHeightProp, defaultEditorLabelWidth) || hasChanged;
                        //if (hasChanged && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        //{
                        //    int _undoGroup = ApplyAndRecord("Display Gauge Offset");
                        //    shipDisplayModule.SetDisplayGaugeOffset(shipDisplayModule.displayGaugeList[dgIdx], dgauOffsetXProp.floatValue, dgauOffsetYProp.floatValue);
                        //    ReadProps(_undoGroup);
                        //}

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauDisplayWidthProp, dgDisplayWidthSettingsContent);
                        EditorGUILayout.PropertyField(dgauDisplayHeightProp, dgDisplayHeightSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Size");
                            shipDisplayModule.SetDisplayGaugeSize(shipDisplayModule.displayGaugeList[dgIdx], dgauDisplayWidthProp.floatValue, dgauDisplayHeightProp.floatValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauIsColourAffectedByValueProp, dgIsColourAffectedByValueSettingsContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (shipDisplayModule.IsEditorMode || Application.isPlaying)
                            {
                                int _undoGroup = ApplyAndRecord("Display Gauge Value affects Colour");

                                if (dgauIsColourAffectedByValueProp.boolValue)
                                {
                                    // Copy current foreground colour into High colour
                                    shipDisplayModule.SetDisplayGaugeValueAffectsColourOn
                                    (
                                        shipDisplayModule.displayGaugeList[dgIdx],
                                        dgauForegroundLowColourProp.colorValue,
                                        dgauForegroundMediumColourProp.colorValue,
                                        dgauForegroundColourProp.colorValue
                                    );
                                }
                                else
                                {
                                    // Copy high colour back into foreground colour
                                    shipDisplayModule.SetDisplayGaugeValueAffectsColourOff(shipDisplayModule.displayGaugeList[dgIdx], dgauForegroundHighColourProp.colorValue);
                                }
                                ReadProps(_undoGroup);
                            }
                        }

                        if (dgauIsColourAffectedByValueProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(dgauMediumColourValueProp, dgMediumColourValueContent);
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(dgauForegroundLowColourProp, dgForegroundLowColourSettingsContent);
                            EditorGUILayout.PropertyField(dgauForegroundMediumColourProp, dgForegroundMediumColourSettingsContent);
                            EditorGUILayout.PropertyField(dgauForegroundHighColourProp, dgForegroundHighColourSettingsContent);
                            if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                            {
                                int _undoGroup = ApplyAndRecord("Display Gauge Foreground Colours");
                                shipDisplayModule.SetDisplayGaugeValue(shipDisplayModule.displayGaugeList[dgIdx], dgauGaugeValueProp.floatValue);
                                ReadProps(_undoGroup);
                            }
                        }
                        else
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(dgauForegroundColourProp, dgForegroundColourSettingsContent);
                            if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                            {
                                int _undoGroup = ApplyAndRecord("Display Gauge Foreground Colour");
                                shipDisplayModule.SetDisplayGaugeForegroundColour(shipDisplayModule.displayGaugeList[dgIdx], dgauForegroundColourProp.colorValue);
                                ReadProps(_undoGroup);
                            }
                        }

                        if (dgauForegroundSpriteProp.objectReferenceValue == null)
                        {
                            EditorGUILayout.HelpBox(dgMissingFgndSpriteWarning, MessageType.Warning);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauForegroundSpriteProp, dgForegroundSpriteSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Foreground Sprite");
                            shipDisplayModule.SetDisplayGaugeForegroundSprite(shipDisplayModule.displayGaugeList[dgIdx], (Sprite)dgauForegroundSpriteProp.objectReferenceValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauBackgroundColourProp, dgBackgroundColourSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Background Colour");
                            shipDisplayModule.SetDisplayGaugeBackgroundColour(shipDisplayModule.displayGaugeList[dgIdx], dgauBackgroundColourProp.colorValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauBackgroundSpriteProp, dgBackgroundSpriteSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Background Sprite");
                            shipDisplayModule.SetDisplayGaugeBackgroundSprite(shipDisplayModule.displayGaugeList[dgIdx], (Sprite)dgauBackgroundSpriteProp.objectReferenceValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauFillMethodProp, dgFillMethodSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Fill Method");
                            shipDisplayModule.SetDisplayGaugeFillMethod(shipDisplayModule.displayGaugeList[dgIdx], (DisplayGauge.DGFillMethod)dgauFillMethodProp.intValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauIsKeepAspectRatioProp, dgIsKeepAspectRatioSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Keep Aspect Ratio");
                            shipDisplayModule.SetDisplayGaugeKeepAspectRatio(shipDisplayModule.displayGaugeList[dgIdx], dgauIsKeepAspectRatioProp.boolValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauTextColourProp, dgTextColourSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Text Colour");
                            shipDisplayModule.SetDisplayGaugeTextColour(shipDisplayModule.displayGaugeList[dgIdx], dgauTextColourProp.colorValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauTextAlignmentProp, dgTextAlignmentSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Alignment");
                            shipDisplayModule.SetDisplayGaugeTextAlignment(shipDisplayModule.displayGaugeList[dgIdx], (TextAnchor)dgauTextAlignmentProp.enumValueIndex);
                            ReadProps(_undoGroup);
                        }

                        if (gaugeType == DisplayGauge.DGType.NumberWithLabel1)
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(dgauLabelAlignmentProp, dgLabelAlignmentSettingsContent);
                            if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                            {
                                int _undoGroup = ApplyAndRecord("Display Gauge Alignment");
                                shipDisplayModule.SetDisplayGaugeLabelAlignment(shipDisplayModule.displayGaugeList[dgIdx], (TextAnchor)dgauLabelAlignmentProp.enumValueIndex);
                                ReadProps(_undoGroup);
                            }
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauTextDirectionProp, dgTextDirectionSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Direction");
                            shipDisplayModule.SetDisplayGaugeTextDirection(shipDisplayModule.displayGaugeList[dgIdx], (DisplayGauge.DGTextDirection)dgauTextDirectionProp.intValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauTextFontStyleProp, dgTextFontStyleSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            int _undoGroup = ApplyAndRecord("Display Gauge Font Style");
                            shipDisplayModule.SetDisplayGaugeTextFontStyle(shipDisplayModule.displayGaugeList[dgIdx], (FontStyle)dgauTextFontStyleProp.intValue);
                            ReadProps(_undoGroup);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dgauIsBestFitProp, dgTextIsBestFitSettingsContent);
                        if (dgauIsBestFitProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(dgauFontMinSizeProp, dgTextFontMinSizeSettingsContent);
                        }
                        EditorGUILayout.PropertyField(dgauFontMaxSizeProp, dgTextFontMaxSizeSettingsContent);
                        if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                        {
                            //int _undoGroup = ApplyAndRecord("Display Gauge Font Size");
                            shipDisplayModule.SetDisplayGaugeTextFontSize(shipDisplayModule.displayGaugeList[dgIdx], dgauIsBestFitProp.boolValue, dgauFontMinSizeProp.intValue, dgauFontMaxSizeProp.intValue);
                            //ReadProps(_undoGroup);
                        }

                        // gaugeMaxValue only applies to numeric types
                        if (gaugeType == DisplayGauge.DGType.FilledNumber1 || gaugeType == DisplayGauge.DGType.NumberWithLabel1)
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(dgauIsNumericPercentageProp, dgIsNumericPercentageSettingsContent);
                            if (!dgauIsNumericPercentageProp.boolValue)
                            {
                                EditorGUILayout.PropertyField(dgauMaxValueProp, dgMaxValueSettingsContent);
                            }
                            EditorGUILayout.PropertyField(dgauDecimalPlacesProp, dgDecimalPlacesSettingsContent);
                            if (EditorGUI.EndChangeCheck() && (shipDisplayModule.IsEditorMode || Application.isPlaying))
                            {
                                serializedObject.ApplyModifiedProperties();
                                // Reapply the gauge value so that the new Max Value takes affect
                                shipDisplayModule.SetDisplayGaugeValue(shipDisplayModule.displayGaugeList[dgIdx], dgauGaugeValueProp.floatValue);
                                serializedObject.Update();
                            }
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
                        shipDisplayModule.RefreshGaugesSortOrder();
                        serializedObject.Update();                       

                        displayGaugeMoveDownPos = -1;
                    }
                    else if (displayGaugeInsertPos >= 0)
                    {
                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        Undo.SetCurrentGroupName("Insert Display Gauge");
                        int _undoGroup = UnityEditor.Undo.GetCurrentGroup();
                        Undo.RecordObject(shipDisplayModule, string.Empty);

                        DisplayGauge insertedGauge = new DisplayGauge(shipDisplayModule.displayGaugeList[displayGaugeInsertPos]);
                        insertedGauge.showInEditor = true;
                        // Generate a new hashcode for the duplicated DisplayGauge
                        insertedGauge.guidHash = SSCMath.GetHashCodeFromGuid();

                        shipDisplayModule.displayGaugeList.Insert(displayGaugeInsertPos, insertedGauge);
                        // Hide original DisplayGauge
                        shipDisplayModule.displayGaugeList[displayGaugeInsertPos + 1].showInEditor = false;
                        RectTransform insertedGaugePanel = shipDisplayModule.CreateGaugePanel(insertedGauge);
                        if (insertedGaugePanel != null)
                        {
                            if (insertedGauge.isKeepAspectRatio)
                            {
                                shipDisplayModule.SetDisplayGaugeKeepAspectRatio(insertedGauge, true);
                            }

                            // Make sure we can rollback the creation of the new gauge panel
                            Undo.RegisterCreatedObjectUndo(insertedGaugePanel.gameObject, string.Empty);

                            shipDisplayModule.RefreshGaugesSortOrder();
                        }

                        Undo.CollapseUndoOperations(_undoGroup);

                        if (insertedGauge != null)
                        {
                            shipDisplayModule.SetDisplayGaugeType(insertedGauge, insertedGauge.gaugeType);
                        }

                        // Read all properties from the ShipDisplayModule
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

                            shipDisplayModule.DeleteGauge(_guidHash);

                            displayGaugeDeletePos = -1;

                            serializedObject.Update();

                            // Force shipDisplayModule to be serialized after a delete
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

            if (isDebuggingEnabled && shipDisplayModule != null)
            {
                float rightLabelWidth = 150f;

                #region Debug General

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(shipDisplayModule.IsInitialised ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugRefResolutionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(shipDisplayModule.ReferenceResolution.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugCanvasResolutionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(shipDisplayModule.CanvasResolution.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsHUDShownContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(shipDisplayModule.IsHUDShown ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                #endregion

                #region Debug Display Target Slots

                isDebuggingShowTargets = EditorGUILayout.Toggle(debugShowTargetsContent, isDebuggingShowTargets);

                if (isDebuggingShowTargets)
                {
                    SSCEditorHelper.PerformanceImpact();

                    int numTargets = shipDisplayModule.GetNumberDisplayTargets;

                    EditorGUILayout.LabelField("Targets: " + numTargets);

                    for (int tgIdx = 0; tgIdx < numTargets; tgIdx++)
                    {
                        DisplayTarget displayTarget = shipDisplayModule.GetDisplayTargetByIndex(tgIdx);

                        int maxNumTargets = displayTarget.maxNumberOfTargets;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Display Target " + (tgIdx + 1).ToString("00"), labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                        EditorGUILayout.LabelField("[Max Targets " + maxNumTargets.ToString("000]"), GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();

                        // Loop through the display target slots
                        for (int slotIdx = 0; slotIdx < maxNumTargets; slotIdx++)
                        {
                            DisplayTargetSlot displayTargetSlot = shipDisplayModule.GetDisplayTargetSlotByIndex(displayTarget, slotIdx);

                            if (displayTargetSlot != null)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(" Slot " + slotIdx.ToString("00"), labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                                EditorGUILayout.LabelField(displayTargetSlot.showTargetSlot ? "Shown" : "Not Shown", GUILayout.MaxWidth(rightLabelWidth));
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(debugTargetRadarIndexContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                                EditorGUILayout.LabelField(displayTargetSlot.radarItemKey.radarItemIndex.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                                EditorGUILayout.EndHorizontal();

                                SSCRadarItem radarItem = shipDisplayModule.GetAssignedDisplayTargetRadarItem(displayTargetSlot);

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(debugTargetRadarItemTypeContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                                EditorGUILayout.LabelField(radarItem == null ? "--" : radarItem.radarItemType.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(debugTargetRadarGameObjectContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                                EditorGUILayout.LabelField(radarItem == null || radarItem.itemGameObject == null ? "--" : radarItem.itemGameObject.name, GUILayout.MaxWidth(rightLabelWidth));
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(debugTargetRadarShipContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                                EditorGUILayout.LabelField(radarItem == null || radarItem.shipControlModule == null ? "--" : radarItem.shipControlModule.name, GUILayout.MaxWidth(rightLabelWidth));
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(debugTargetRadarPositionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                                EditorGUILayout.LabelField(radarItem == null ? "--" : SSCEditorHelper.GetVector3Text(radarItem.position, 2), GUILayout.MaxWidth(rightLabelWidth));
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(debugTargetScreenPositionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                                EditorGUILayout.LabelField(radarItem == null || shipDisplayModule.mainCamera == null ? "--" : SSCEditorHelper.GetVector3Text(shipDisplayModule.mainCamera.WorldToScreenPoint(radarItem.position), 2), GUILayout.MaxWidth(rightLabelWidth));
                                EditorGUILayout.EndHorizontal();

                            }
                        }

                    }
                }

                #endregion
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

            shipDisplayModule.allowRepaint = true;
        }

        #endregion

        #region Public Static Methods
        // Add a menu item so that a HUD can be created via the GameObject > 3D Object menu
        [MenuItem("GameObject/3D Object/Sci-Fi Ship Controller/Ship Display Module")]
        public static void CreateHUD()
        {
            // If this scene does not already have a manager, create one
            GameObject newHUDGameObject = new GameObject("HUD");
            newHUDGameObject.transform.position = Vector3.zero;
            newHUDGameObject.transform.parent = null;

            newHUDGameObject.layer = 5;
            newHUDGameObject.AddComponent<Canvas>();

            Canvas hudCanvas = newHUDGameObject.GetComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 2;
            UnityEngine.UI.CanvasScaler canvasScaler = newHUDGameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            if (canvasScaler != null)
            {
                canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
                canvasScaler.matchWidthOrHeight = 0.5f;
            }

            RectTransform hudGORectTfrm = newHUDGameObject.GetComponent<RectTransform>();

            // Create the HUDPanel
            Selection.activeTransform = hudGORectTfrm;
            SSCEditorHelper.CallMenu("GameObject/UI/Panel");
            // The new panel is automatically selected
            Selection.activeTransform.name = ShipDisplayModule.hudPanelName;
            RectTransform hudPanel = Selection.activeTransform.GetComponent<RectTransform>();
            UnityEngine.UI.Image img = hudPanel.GetComponent<UnityEngine.UI.Image>();
            img.raycastTarget = false;
            img.enabled = false;

            // Create the Reticle panel
            Selection.activeTransform = hudPanel;
            SSCEditorHelper.CallMenu("GameObject/UI/Panel");
            // The new panel is automatically selected
            Selection.activeTransform.name = "DisplayReticlePanel";
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
            SSCEditorHelper.CallMenu("GameObject/UI/Panel");
            // The new panel is automatically selected
            Selection.activeTransform.name = ShipDisplayModule.overlayPanelName;
            RectTransform overlayPanel = Selection.activeTransform.GetComponent<RectTransform>();
            img = overlayPanel.GetComponent<UnityEngine.UI.Image>();
            img.sprite = (Sprite)AssetDatabase.LoadAssetAtPath(SSCSetup.texturesFolder + "/HUD/SSCUIOverlay2.png", typeof(Sprite));
            img.raycastTarget = false;
            overlayPanel.SetParent(hudPanel);

            // Create Altitude panel
            Selection.activeTransform = hudPanel;
            SSCEditorHelper.CallMenu("GameObject/UI/Text");
            // The new panel is automatically selected
            Selection.activeTransform.name = "AltitudeText";
            RectTransform altPanel = Selection.activeTransform.GetComponent<RectTransform>();
            UnityEngine.UI.Text txt = altPanel.GetComponent<UnityEngine.UI.Text>();
            txt.raycastTarget = false;
            txt.text = "0";
            txt.fontSize = 24;
            txt.alignment = TextAnchor.UpperLeft;
            altPanel.localPosition = new Vector3(740f, 465f, 0f);
            altPanel.SetParent(overlayPanel);

            // Create Airspeed panel
            Selection.activeTransform = hudPanel;
            SSCEditorHelper.CallMenu("GameObject/UI/Text");
            // The new panel is automatically selected
            Selection.activeTransform.name = "AirSpeedText";
            RectTransform airspeedPanel = Selection.activeTransform.GetComponent<RectTransform>();
            txt = airspeedPanel.GetComponent<UnityEngine.UI.Text>();
            txt.raycastTarget = false;
            txt.text = "0";
            txt.fontSize = 24;
            txt.alignment = TextAnchor.UpperRight;
            airspeedPanel.localPosition = new Vector3(-740f, 465f, 0f);
            airspeedPanel.SetParent(overlayPanel);

            ShipDisplayModule shipDisplayModule = newHUDGameObject.AddComponent<ShipDisplayModule>();
            Selection.activeTransform = newHUDGameObject.transform;

            // Add at least one reticle
            DisplayReticle displayReticle = new DisplayReticle();
            displayReticle.primarySprite = (Sprite)AssetDatabase.LoadAssetAtPath(SSCSetup.texturesFolder + "/HUD/SSCUIAim1.png", typeof(Sprite));
            shipDisplayModule.displayReticleList = new List<DisplayReticle>(10);
            shipDisplayModule.displayReticleList.Add(displayReticle);

            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(newHUDGameObject.scene);
                EditorUtility.SetDirty(shipDisplayModule);
            }
        }
        #endregion
    }
}