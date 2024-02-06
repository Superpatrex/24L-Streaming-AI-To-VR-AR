using scsmmedia;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Heads Up Display - typically used from an in-cockpit player view or setup.
    /// The general approach here is to use RectTransforms that are NOT stretched
    /// and are anchored at the centre.
    /// Setup Notes:
    /// HUDPanel should be anchored at four corners of screen (stretched)
    /// Display Reticle panel should be anchored at the centre of HUD
    /// Altitude Panel should be anchored to centre
    /// Altitude Text pivot point should be 0,0 (left bottom corner of textbox)
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Misc/Ship Display Module")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class ShipDisplayModule : MonoBehaviour
    {
        #region Enumerations

        #endregion

        #region Public Static Variables
        public readonly static string hudPanelName = "HUDPanel";
        public readonly static string fadePanelName = "FadePanel";
        public readonly static string targetsPanelName = "TargetsPanel";
        public readonly static string gaugesPanelName = "GaugesPanel";
        public readonly static string attitudePanelName = "AttitudePanel";
        public readonly static string headingPanelName = "HeadingPanel";
        public readonly static string overlayPanelName = "OverlayPanel";
        public readonly static string attitudeScrollName = "AttitudeScroll";
        public readonly static string headingScrollName = "HeadingScroll";
        public readonly static string headingIndicatorName = "HeadingIndicator";

        [System.NonSerialized] public static readonly AnimationCurve easeInOutCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        #endregion

        #region Public Variables and Properties - General

        /// <summary>
        /// If enabled, the Initialise() will be called as soon as Start() runs. This should be disabled if you are
        /// instantiating the HUD through code.
        /// </summary>
        public bool initialiseOnStart = false;

        /// <summary>
        /// Show or Hide the HUD when it is first Initialised
        /// </summary>
        public bool isShowOnInitialise = true;

        /// <summary>
        /// Show overlay image on HUD. At runtime call ShowOverlay() or HideOverlay()
        /// </summary>
        public bool showOverlay = true;

        /// <summary>
        /// The head-up display's normalised width of the screen. 1.0 is full width, 0.5 is half width.
        /// At runtime call shipDisplayModule.SetHUDSize(..)
        /// </summary>
        [Range(0.1f, 1f)] public float displayWidth = 0.5f;

        /// <summary>
        /// The head-up display's normalised height of the screen. 1.0 is full height, 0.5 is half height.
        /// At runtime call shipDisplayModule.SetHUDSize(..)
        /// </summary>
        [Range(0.1f, 1f)] public float displayHeight = 0.75f;

        /// <summary>
        /// The head-up display's normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.
        /// At runtime call shipDisplayModule.SetHUDOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float displayOffsetX = 0f;

        /// <summary>
        /// The head-up display's normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.
        /// At runtime call shipDisplayModule.SetHUDOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float displayOffsetY = 0f;

        /// <summary>
        /// Primary colour of the heads-up display.
        /// At runtime call shipDisplayModule.SetPrimaryColour(..)
        /// </summary>
        public Color32 primaryColour = Color.grey;

        /// <summary>
        /// Automatically hide the screen cursor or mouse pointer after it has been stationary
        /// for a fixed period of time. Automatically show the cursor if the mouse if moved
        /// provided that the Display Reticle is on shown.
        /// </summary>
        public bool autoHideCursor = true;

        /// <summary>
        /// The number of seconds to wait until after the cursor has not moved before hiding it
        /// </summary>
        public float hideCursorTime = 3f;

        /// <summary>
        /// The overall brightness of the HUD. At runtime use
        /// SetBrightness(value)
        /// </summary>
        [Range(0f, 1f)] public float brightness = 1f;

        /// <summary>
        /// The sort order of the canvas in the scene. Higher numbers are on top.
        /// At runtime call shipDisplayModule.SetCanvasSortOrder(..)
        /// </summary>
        public int canvasSortOrder = 2;

        /// <summary>
        /// Is the ship display module initialised? Gets set at runtime when Initialise() is called.
        /// </summary>
        public bool IsInitialised { get; private set; }

        /// <summary>
        /// Is the heads-up display shown? To set use ShowHUD() or HideHUD().
        /// IsHUDShown should never be true at runtime if IsInitialised is false
        /// </summary>
        public bool IsHUDShown { get; private set; }

        /// <summary>
        /// Get a reference to the HUD canvas
        /// </summary>
        public Canvas GetCanvas { get { return IsInitialised ? canvas : GetComponent<Canvas>(); } }

        /// <summary>
        /// The default size the HUD was designed with. This should always
        /// return 1920x1080
        /// </summary>
        public Vector2 ReferenceResolution { get { return refResolution; } }

        /// <summary>
        /// The current scaled resolution of the HUD canvas
        /// </summary>
        public Vector2 CanvasResolution { get { return cvsResolutionFull; } }

        /// <summary>
        /// The current scale factor of the HUD canvas
        /// </summary>
        public Vector3 CanvasScaleFactor { get { return cvsScaleFactor; } }

        /// <summary>
        /// The current actual screen resolution. This may be different from
        /// what the 
        /// </summary>
        public Vector2 ScreenResolution { get { return screenResolution; } }

        #endregion

        #region Public Variables and Properties - Fade

        /// <summary>
        /// The colour the display will fade from or to.
        /// </summary>
        public Color fadeColour = Color.black;

        /// <summary>
        /// Fade the display in when it first starts
        /// </summary>
        public bool fadeInOnStart = false;

        /// <summary>
        /// Fade the display out when it first starts
        /// </summary>
        public bool fadeOutOnStart = false;

        /// <summary>
        /// The length of time in seconds, it takes to fully fade in the display
        /// </summary>
        [Range(0f,20f)] public float fadeInDuration = 5f;

        /// <summary>
        /// The length of time in seconds, it takes to fully fade out the display
        /// </summary>
        [Range(0f,20f)] public float fadeOutDuration = 5f;

        /// <summary>
        /// Is the Display (HUD), currently fading in?
        /// </summary>
        public bool IsDisplayFadingIn { get { return isFadingIn; } }

        /// <summary>
        /// Is the Display (HUD), currently fading out?
        /// </summary>
        public bool IsDisplayFadingOut { get { return isFadingOut; } }

        #endregion

        #region Public Variables and Properties - Flicker

        /// <summary>
        /// Whenever the HUD is shown, should it flicker on?
        /// </summary>
        public bool isShowHUDWithFlicker = false;

        /// <summary>
        /// Whenever the HUD is hidden, should it flicker off?
        /// </summary>
        public bool isHideHUDWithFlicker = false;

        /// <summary>
        /// The time, in seconds, the effect takes to reach a steady state
        /// </summary>
        [Range(0.01f, 5f)] public float flickerDefaultDuration = 1f;

        /// <summary>
        /// The minimum time, in seconds, that the effect is inactive or off
        /// </summary>
        [Range(0f, 2f)] public float flickerMinInactiveTime = 0.1f;

        /// <summary>
        /// The maximum time, in seconds, that the effect is inactive or off
        /// </summary>
        [Range(0f, 2f)] public float flickerMaxInactiveTime = 0.2f;

        /// <summary>
        /// The minimum time, in seconds, that the effect is active or on
        /// </summary>
        [Range(0f, 2f)] public float flickerMinActiveTime = 0.1f;

        /// <summary>
        /// The maximum time, in seconds, that the effect is active or on
        /// </summary>
        [Range(0f, 2f)] public float flickerMaxActiveTime = 0.2f;

        /// <summary>
        /// Smooth the flickering effect. Higher values give a smoother effect
        /// </summary>
        [Range(0, 5)] public float flickerSmoothing = 3f;

        /// <summary>
        /// The intensity of the effect will randomly change
        /// </summary>
        public bool flickerVariableIntensity = false;

        /// <summary>
        /// The maximum intensity of the effect used during the on cycle when
        /// flickerVariableIntensity is enabled.
        /// This is a multiplier of the starting intensity of the effect.
        /// Value must be between 0.01 and 1.0.
        /// </summary>
        [Range(0.01f, 1f)] public float flickerMaxIntensity = 1f;

        #endregion

        #region Public Variables and Properties - Reticles

        /// <summary>
        /// The guidHash of the currently selected / displayed reticle
        /// </summary>
        public int guidHashActiveDisplayReticle = 0;

        /// <summary>
        /// The list of display reticles that can be used with
        /// this Ship Display. Call ReinitialiseVariables() if
        /// you modify the list at runtime.
        /// </summary>
        public List<DisplayReticle> displayReticleList;

        /// <summary>
        /// Get the current number of display reticles
        /// </summary>
        public int GetNumberDisplayReticles { get { return IsInitialised ? numDisplayReticles : displayReticleList == null ? 0 : displayReticleList.Count; } }

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// </summary>
        public bool isDisplayReticleListExpanded = true;

        /// <summary>
        /// For consistency, is the display reticle currently shown when the
        /// module has already been initialised?
        /// </summary>
        public bool IsDisplayReticleShown { get { return showActiveDisplayReticle; } }

        /// <summary>
        /// Get the Display Reticle position on the screen in Viewport coordinates. x = 0.0-1.0, y = 0.0-1.0. 0,0 is bottom left corner.
        /// </summary>
        public Vector2 DisplayReticleViewportPoint { get { return new Vector2((displayReticleOffsetX + 1f) * 0.5f, (displayReticleOffsetY + 1f) * 0.5f); } }

        /// <summary>
        /// Show the current (active) Display Reticle
        /// on the HUD. At runtime use ShowDisplayReticle()
        /// or HideDisplayReticle().
        /// </summary>
        public bool showActiveDisplayReticle = false;

        /// <summary>
        /// The Display Reticle's normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.
        /// At runtime call shipDisplayModule.SetDisplayReticleOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float displayReticleOffsetX = 0f;

        /// <summary>
        /// The Display Reticle's normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.
        /// At runtime call shipDisplayModule.SetDisplayReticleOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float displayReticleOffsetY = 0f;

        /// <summary>
        /// Should the Display Reticle follow the cursor or mouse screen position?
        /// </summary>
        public bool lockDisplayReticleToCursor = false;

        /// <summary>
        /// Colour of the active Display Reticle.
        /// At runtime call shipDisplayModule.SetDisplayReticleColour(..)
        /// </summary>
        public Color32 activeDisplayReticleColour = Color.white;

        /// <summary>
        /// This is the main camera that HUD will reference. If left empty, this will
        /// be auto-populated with your first Camera with a tag of MainCamera.
        /// Can be changed at runtime with shipDisplayModule.SetCamera(..).
        /// </summary>
        public Camera mainCamera = null;

        #endregion

        #region Public Variables and Properties - Altitude and Speed

        /// <summary>
        /// The ship in the scene that will supply the data for this HUD
        /// </summary>
        public ShipControlModule sourceShip = null;

        /// <summary>
        /// Show the Altitude indicator on the HUD. At runtime use ShowAltitude() or HideAltitude().
        /// Typically only used when near the surface of a planet. See also groundPlaneHeight.
        /// </summary>
        public bool showAltitude = false;

        /// <summary>
        /// Used to determine altitude when near a planet's surface. On Earth this would typically
        /// be sea-level but it can be used to set an artificial zero-height which may be useful
        /// when flying over the surface of a planet.
        /// </summary>
        public float groundPlaneHeight = 0f;

        /// <summary>
        /// The colour of the Altitude text (number).
        /// At runtime call shipDisplayModule.SetAltitudeTextColour(..)
        /// </summary>
        public Color32 altitudeTextColour = Color.grey;

        /// <summary>
        /// Show the air speed indicator on the HUD. At runtime use ShowAirSpeed() or HideAirSpeed().
        /// </summary>
        public bool showAirspeed = false;

        /// <summary>
        /// The colour of the Air Speed text (number).
        /// At runtime call shipDisplayModule.SetAirSpeedTextColour(..)
        /// </summary>
        public Color32 airspeedTextColour = Color.grey;

        #endregion

        #region Public Variables and Properties - Attitude

        /// <summary>
        /// Show or hide the Attitude.
        /// At runtime use ShowAttitude() or HideAttitude()
        /// </summary>
        public bool showAttitude = false;

        /// <summary>
        /// Primary colour of the scrollable heading
        /// At runtime call shipDisplayModule.SetDisplayAttitudePrimaryColour(..)
        /// </summary>
        public Color attitudePrimaryColour = Color.white;

        #endregion

        #region Public Variables and Properties - Heading

        /// <summary>
        /// Show or hide the Heading or direction as a scrollable ribbon in the UI.
        /// At runtime use ShowHeading() or HideHeading()
        /// </summary>
        public bool showHeading = false;

        /// <summary>
        /// Show or hide the small heading indicator
        /// At runtime use ShowHeadingIndictor() or HideHeadingIndicator()
        /// </summary>
        public bool showHeadingIndicator = true;

        /// <summary>
        /// Primary colour of the scrollable heading
        /// At runtime call shipDisplayModule.SetDisplayHeadingPrimaryColour(..)
        /// </summary>
        public Color headingPrimaryColour = Color.white;

        /// <summary>
        /// The small indicator colour of the scrollable heading
        /// At runtime call shipDisplayModule.SetDisplayHeadingIndicatorColour(..)
        /// </summary>
        public Color headingIndicatorColour = Color.green;

        #endregion

        #region Public Variables and Properties - Gauges

        /// <summary>
        /// The list of display gauges that can be used with
        /// this Ship Display. Call ReinitialiseVariables() if
        /// you modify this list at runtime. Where possible use the API
        /// methods to Add, Delete, or set gauge attributes.
        /// </summary>
        public List<DisplayGauge> displayGaugeList;

        /// <summary>
        /// Get the current number of display gauge
        /// </summary>
        public int GetNumberDisplayGauges { get { return IsInitialised ? numDisplayGauges : displayGaugeList == null ? 0 : displayGaugeList.Count; } }

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// </summary>
        public bool isDisplayGaugeListExpanded = true;

        #endregion

        #region Public Variables and Properties - Messages

        /// <summary>
        /// The list of display messages that can be used with
        /// this Ship Display. Call ReinitialiseVariables() if
        /// you modify this list at runtime. Where possible use the API
        /// methods to Add, Delete, or set messages attributes.
        /// </summary>
        public List<DisplayMessage> displayMessageList;

        /// <summary>
        /// Get the current number of display messages
        /// </summary>
        public int GetNumberDisplayMessages { get { return IsInitialised ? numDisplayMessages : displayMessageList == null ? 0 : displayMessageList.Count; } }

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// </summary>
        public bool isDisplayMessageListExpanded = true;

        #endregion

        #region Public Variables and Properties - Targets

        /// <summary>
        /// The list of display targets that can be used with
        /// this Ship Display. Call ReinitialiseVariables() if
        /// you modify this list at runtime. Where possible use the API
        /// methods to Add, Delete, or set target attributes.
        /// </summary>
        public List<DisplayTarget> displayTargetList;

        /// <summary>
        /// Get the current number of Display Targets.
        /// </summary>
        public int GetNumberDisplayTargets { get { return IsInitialised ? numDisplayTargets : displayTargetList == null ? 0 : displayTargetList.Count; } }

        /// <summary>
        /// Get the size of the Targets viewport as a normalised (0.0-1.0) value of the current screensize
        /// </summary>
        public Vector2 GetTargetsViewportSize { get { return new Vector2(targetsViewWidth, targetsViewHeight); } }

        /// <summary>
        /// Get the offset from the screen centre of the Targets viewport. Values are -1.0 to 1.0
        /// </summary>
        public Vector2 GetTargetsViewportOffset { get { return new Vector2(targetsViewOffsetX, targetsViewOffsetY); } }

        /// <summary>
        /// When DisplayTarget slots have an active RadarItem assigned to them, should the reticles be automatically
        /// moved on the HUD?
        /// </summary>
        public bool autoUpdateTargetPositions = true;

        /// <summary>
        /// The Targets normalised viewable width of the screen.
        /// 1.0 is full width, 0.5 is half width.
        /// At runtime call shipDisplayModule.SetTargetsViewportSize(..)
        /// </summary>
        [Range(0.1f, 1f)] public float targetsViewWidth = 1.0f;

        /// <summary>
        /// The Targets normalised viewable height of the screen.
        /// 1.0 is full height, 0.5 is half height.
        /// At runtime call shipDisplayModule.SetTargetsViewportSize(..)
        /// </summary>
        [Range(0.1f, 1f)] public float targetsViewHeight = 0.5f;

        /// <summary>
        /// The X offset from centre of the screen for the Targets viewport
        /// At runtime call shipDisplayModule.SetTargetsViewportOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float targetsViewOffsetX = 0f;

        /// <summary>
        /// The Y offset from centre of the screen for the Targets viewport
        /// At runtime call shipDisplayModule.SetTargetsViewportOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float targetsViewOffsetY = 0.5f;

        /// <summary>
        /// The maximum distance, in metres, that targets can be away from the ship
        /// </summary>
        public float targetingRange = 5000f;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// </summary>
        public bool isDisplayTargetListExpanded = true;

        #endregion

        #region Public Variables and Properties - FUTURE FEATURES

        /// <summary>
        /// FUTURE
        /// </summary>
        //public bool showArtificialHorizon = false;

        // FUTURE - FPV indicator
        //public bool showFlightPathVector = false;

        // FUTURE - wing's angle relative to airflow
        //public bool showAngleOfAttack = false;

        // FUTURE - target designation indicator
        //public bool showTargetDesignator = false;

        // Other options could be closing (target) velocity, range (to target), selected weapons, amno available etc.

        #endregion

        #region Public Variables and Properties - INTERNAL ONLY

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [HideInInspector] public bool allowRepaint = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool showGeneralSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool showReticleSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool showAltSpeedSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool showAttitudeSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool showFadeSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool showFlickerSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool showHeadingSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool showGaugeSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool showMessageSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool showTargetSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Outside play mode, show a bounding box for where the HUD will be placed
        /// </summary>
        public bool showHUDOutlineInScene = true;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Outside play mode, show a bounding box for where Targets can be displayed
        /// </summary>
        public bool showTargetsOutlineInScene = false;
        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool IsEditorMode { get { return editorMode; } }

        #endregion

        #region Public Delegates

        public delegate void CallbackOnBrightnessChange(float newBrightness);
        public delegate void CallbackOnSizeChange(float newWidth, float newHeight);

        /// <summary>
        /// The name of the custom method that is called immediately
        /// after brightness has changed. Your method must take 1 float
        /// parameter. This should be a lightweight method to avoid
        /// performance issues. It could be used to update your custom
        /// HUD elements.
        /// </summary>
        public CallbackOnBrightnessChange callbackOnBrightnessChange = null;

        /// <summary>
        /// The name of the custom method that is called immediately after the
        /// HUD size has changed. This method must take 2 float parameters. It
        /// should be a lightweight method to avoid performance issues. It could
        /// be used to update your custom HUD elements.
        /// </summary>
        public CallbackOnSizeChange callbackOnSizeChange = null;

        #endregion

        #region Private Variables - General
        private Canvas canvas = null;
        private CanvasScaler canvasScaler = null;

        // Scaled canvas resolution. See CheckHUDResize()
        private Vector2 cvsResolutionFull = Vector2.one;
        private Vector2 cvsResolutionHalf = Vector2.one;
        private Vector2 prevResolutionFull = Vector2.one;
        // Default reference resolution e.g. 1920x1080
        private Vector2 refResolution = Vector2.one;
        private Vector3 cvsScaleFactor = Vector3.one;
        private Vector2 screenResolution = Vector2.one;

        private Transform hudPanel = null;
        private bool isMainCameraAssigned = false;
        private Color tempColour = Color.clear;

        // Used to update HUD from the editor when not in play mode
        private bool editorMode = false;

        private List<Text> tempTextList = null;
        private List<UnityEngine.UI.Image> tempImgList = null;

        #endregion

        #region Private Variables - Attitude
        private RectTransform attitudeRectTfrm = null;
        private RectTransform attitudeScrollPanel = null;
        private UnityEngine.UI.Image attitudeScrollImg = null;
        private UnityEngine.UI.Image attitudeMaskImg = null;
        private bool isAttitudeScrolling = false;
        private Vector3 tempAttitudeOffset = Vector3.zero;
        private Vector3 attitudeInitPosition = Vector3.zero;

        /// <summary>
        /// This gets calculated in InitialiseAttitude(..) at runtime.
        /// </summary>
        private float attitudePixelsPerDegree = 1080f / 180f;

        /// <summary>
        /// The attitude normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.
        /// At runtime call shipDisplayModule.SetAttitudeOffset(..)
        /// </summary>
        [SerializeField, Range(-1f, 1f)] private float attitudeOffsetX = 0f;

        /// <summary>
        /// The attitude normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.
        /// At runtime call shipDisplayModule.SetAttitudeOffset(..)
        /// </summary>
        [SerializeField, Range(-1f, 1f)] private float attitudeOffsetY = 0f;

        /// <summary>
        /// The normalised masked width of the scrollable attitude. 1.0 is full width of the screen, 0.5 is half width. 
        /// </summary>
        [SerializeField, Range(0.1f, 1f)] private float attitudeWidth = 0.75f;

        /// <summary>
        /// The normalised masked height of the scrollable attitude. 1.0 is full height of the screen, 0.5 is half height.
        /// </summary>
        [SerializeField, Range(0.1f, 1f)] private float attitudeHeight = 1f;

        /// <summary>
        /// The sprite (texture) that will scroll up/down
        /// </summary>
        [SerializeField] private Sprite attitudeScrollSprite = null;

        /// <summary>
        /// The sprite (texture) that will mask the scrollable attitude sprite
        /// </summary>
        [SerializeField] private Sprite attitudeMaskSprite = null;

        /// <summary>
        /// The number of pixels between the top of the scroll sprite and the first (90) pitch line.
        /// It is assumed that this is the same for between the bottom and the -90 pitch line.
        /// </summary>
        [SerializeField, Range(0f, 1000f)] private float attitudeScrollSpriteBorderWidth = 250f;

        #endregion

        #region Private Variables - Fade

        private bool isFadingIn = false;
        private bool isFadingOut = false;
        private float fadeStartAlpha = 255f;
        private float fadeTargetValue = 0f;
        private Image fadeImage = null;
        private Color currentFadeColour;

        #endregion

        #region Private Variables - Flickering
        /// <summary>
        /// Is the HUD currently flickering and will it end in the Shown state?
        /// This tracks if flickering is currently happening.
        /// </summary>
        private bool isFlickeringEndStateOn = false;
        /// <summary>
        /// Is the HUD currently flickering and will it end in the Hidden state?
        /// This tracks if flickering is currently happening.
        /// </summary>
        private bool isFlickeringEndStateOff = false;

        private float flickerDurationTimer = 0f;
        private float flickerInactiveTimer = 0f;
        private float flickerActiveTimer = 0f;
        private float flickerDuration = 0f;
        private float flickerActiveTime = 0f;
        private float flickerInactiveTime = 0f;
        private bool isFlickerWaiting = false;
        private SSCRandom hudRandom = null;
        private float flickerIntensity = 0f;
        private float flickerStartIntensity = 0f;
        private Queue<float> flickerHistory = null;
        private float flickerHistoryTotal = 0f;

        #endregion

        #region Private Variables - Brightness
        private SSCColour baseReticleColour;
        private SSCColour baseOverlayColour;
        private SSCColour baseAltitudeTextColour;
        private SSCColour baseAirspeedTextColour;
        private SSCColour baseAttitudePrimaryColour;
        private SSCColour baseHeadingPrimaryColour;
        private SSCColour baseHeadingIndicatorColour;
        #endregion

        #region Private Variables - Heading
        private RectTransform headingRectTfrm = null;
        private RectTransform headingScrollPanel = null;
        private RectTransform headingIndicatorPanel = null;
        private UnityEngine.UI.Image headingScrollImg = null;
        private UnityEngine.UI.Image headingMaskImg = null;
        private UnityEngine.UI.Image headingIndicatorImg = null;

        private bool isHeadingScrolling = false;
        private Vector3 headingInitPosition = Vector3.zero;
        private Vector3 tempHeadingOffset = Vector3.zero;

        /// <summary>
        /// This gets calculated in InitialiseHeading(..) at runtime.
        /// </summary>
        private float headingPixelsPerDegree = 1920f / 360f;

        /// <summary>
        /// The heading normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.
        /// At runtime call shipDisplayModule.SetHeadingOffset(..)
        /// </summary>
        [SerializeField, Range(-1f, 1f)] private float headingOffsetX = 0f;

        /// <summary>
        /// The heading normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.
        /// At runtime call shipDisplayModule.SetHeadingOffset(..)
        /// </summary>
        [SerializeField, Range(-1f, 1f)] private float headingOffsetY = 0f;

        /// <summary>
        /// The normalised masked width of the scrollable heading. 1.0 is full width of the screen, 0.5 is half width. 
        /// </summary>
        [SerializeField, Range(0.1f, 1f)] private float headingWidth = 0.75f;

        /// <summary>
        /// The normalised masked height of the scrollable heading. 1.0 is full height of the screen, 0.5 is half height.
        /// </summary>
        [SerializeField, Range(0.1f, 1f)] private float headingHeight = 1f;

        /// <summary>
        /// The sprite (texture) that will scroll left/right
        /// </summary>
        [SerializeField] private Sprite headingScrollSprite = null;

        /// <summary>
        /// The sprite (texture) that will mask the scrollable heading sprite
        /// </summary>
        [SerializeField] private Sprite headingMaskSprite = null;

        /// <summary>
        /// The small sprite (texture) that will indicate or point to the heading on the HUD
        /// </summary>
        [SerializeField] private Sprite headingIndicatorSprite = null;

        #endregion

        #region Private Variables - Reticle
        private RectTransform hudRectTfrm = null;
        private Transform reticlePanel = null;
        private float reticleWidth = 1f;
        private float reticleHeight = 1f;
        private DisplayReticle currentDisplayReticle = null;
        private UnityEngine.UI.Image displayReticleImg = null;
        private int numDisplayReticles = 0;
        #endregion

        #region Private Variables - Gauges
        private int numDisplayGauges = 0;
        private RectTransform gaugesRectTfrm = null;
        private Vector3 tempGaugeOffset = Vector3.zero;
        #endregion

        #region Private Variables - Message
        private int numDisplayMessages = 0;       
        private Vector3 tempMessageOffset;
        #endregion

        #region Private Variables - Cursor
        private bool isCursorVisible = true;
        private float cursorTimer = 0f;

        // Switch to using the New Input System if it is available
#if SSC_UIS
        private Vector2 currentMousePosition = Vector2.zero;
        private Vector2 lastMousePosition = Vector2.zero;
#else
        private Vector3 currentMousePosition = Vector3.zero;
        private Vector3 lastMousePosition = Vector3.zero;
#endif
        #endregion

        #region Private Variables - Altitude and Speed
        private UnityEngine.UI.Image primaryOverlayImg = null;
        private RectTransform overlayPanel = null;
        private RectTransform altitudeTextRectTfrm = null;
        private Text altitudeText = null;
        private Vector3 altitudeInitPosition;
        private Vector3 altitudeCurrentPosition;
        private SCSMString altitudeString = null;
        private RectTransform airspeedTextRectTfrm = null;
        private Text airspeedText = null;
        private Vector3 airspeedInitPosition;
        private Vector3 airspeedCurrentPosition;
        private SCSMString airspeedString = null;
        #endregion

        #region Private Variables - Targets
        private int numDisplayTargets = 0;
        private RectTransform targetsRectTfrm = null;
        private Vector3 tempTargetOffset;
        private List<int> tempIncludeFactionList = null;
        private List<int> tempIncludeSquadronList = null;
        private SSCRadar sscRadar = null;
        #endregion

        #region Private Initialisation Methods

        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            float dTime = Time.deltaTime;

            #region Flickering
            if (isFlickeringEndStateOn || isFlickeringEndStateOff)
            {
                flickerDurationTimer += dTime;
                // Disable flickering
                if (flickerDurationTimer > flickerDuration)
                {
                    flickerHistory.Clear();
                    if (isFlickeringEndStateOn)
                    {
                        isFlickeringEndStateOn = false;
                        // Restore the original brightness
                        SetBrightness(flickerStartIntensity);
                        ShowOrHideHUD(true);
                    }
                    // FlickeringOff
                    else
                    {
                        isFlickeringEndStateOff = false;
                        // Restore the original brightness
                        brightness = flickerStartIntensity;
                        ShowOrHideHUD(false);
                    }                    
                }
                else if (isFlickerWaiting)
                {
                    flickerInactiveTimer += dTime;
                    if (flickerInactiveTimer > flickerInactiveTime)
                    {
                        isFlickerWaiting = false;
                        flickerActiveTimer = 0f;
                        flickerActiveTime = hudRandom != null ? hudRandom.Range(flickerMinActiveTime, flickerMaxActiveTime + 0.001f): 1f;
                        ShowOrHideHUD(true);                       
                    }
                }
                else
                {
                    flickerActiveTimer += dTime;
                    if (flickerActiveTimer > flickerActiveTime)
                    {
                        flickerHistory.Clear();
                        flickerHistory.Enqueue(0f);
                        flickerInactiveTimer = 0f;
                        flickerInactiveTime = hudRandom != null ? hudRandom.Range(flickerMinInactiveTime, flickerMaxInactiveTime + 0.001f) : 1f;
                        isFlickerWaiting = true;
                        ShowOrHideHUD(false);
                    }
                    else if (flickerVariableIntensity)
                    {
                        if (flickerSmoothing > 0)
                        {
                            // Remove the first value from the queue
                            if (flickerHistory.Count > 0 && flickerHistory.Count > flickerSmoothing + 1) { flickerHistoryTotal -= flickerHistory.Dequeue(); }

                            flickerIntensity = hudRandom.Range(0f, flickerStartIntensity * flickerMaxIntensity);
                            flickerHistoryTotal += flickerIntensity;

                            // Add the new value to the end of the queue
                            flickerHistory.Enqueue(flickerIntensity);

                            SetBrightness(flickerHistoryTotal / (float)flickerHistory.Count);
                        }
                        else
                        {
                            // No smoothing
                            flickerIntensity = hudRandom.Range(0f, flickerStartIntensity * flickerMaxIntensity);
                            SetBrightness(flickerIntensity);
                        }
                    }
                }
            }
            #endregion

            #region Check Window Resize
            if (IsHUDShown)
            {
                // Would be nice to not call this so often
                CheckHUDResize(true);
            }
            #endregion

            #region AutoHideCursor and Reticle cursor control
            if (autoHideCursor || (lockDisplayReticleToCursor && showActiveDisplayReticle))
            {
                #if SSC_UIS
                currentMousePosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
                #else
                currentMousePosition = Input.mousePosition;
                #endif

                #region Auto-hide Cursor
                if (autoHideCursor)
                {
                    if (isCursorVisible)
                    {
                        cursorTimer += dTime;
                        // If use has move the mouse, reset the timer
                        if (lastMousePosition != currentMousePosition) { lastMousePosition = currentMousePosition; cursorTimer = 0f; }
                        // After hideCursorTime secs, hide it
                        else if (cursorTimer > hideCursorTime) { ShowOrHideCursor(false); }
                    }
                    // Check if mouse has moved (does user wish to click on something?)
                    else if (lastMousePosition != currentMousePosition)
                    {
                        lastMousePosition = currentMousePosition;
                        ShowOrHideCursor(true);
                    }
                }

                #endregion

                #region Local Display Reticle to Cursor
                if (IsInitialised && isMainCameraAssigned && lockDisplayReticleToCursor && showActiveDisplayReticle)
                {
                    // It is safe to switch between Vector3 and Vector2 as each has an expicit
                    // operator which either adds z = 0 or drops the z component.
                    Vector2 viewPoint = mainCamera.ScreenToViewportPoint(currentMousePosition);

                    // Convert 0.0 to 1.0 into -1.0 to 1.0.
                    SetDisplayReticleOffset(viewPoint.x * 2f - 1f, viewPoint.y * 2f - 1f);

                    // v1.2.8+ cursor should be hidden when reticle is following the cursor.
                    if (isCursorVisible && !autoHideCursor && IsHUDShown) { ShowOrHideCursor(false); }
                }
                #endregion
            }
            #endregion

            #region Update Altitude, Speed, Heading

            if ((showAltitude || showAirspeed || isHeadingScrolling || isAttitudeScrolling) && IsHUDShown && sourceShip != null && sourceShip.ShipIsEnabled() && sourceShip.IsInitialised)
            {
                if (showAltitude)
                {
                    /// TODO - round altitude to 100s
                    altitudeString.Set((int)(sourceShip.shipInstance.TransformPosition.y - groundPlaneHeight));
                    if (altitudeText != null) { altitudeText.text = altitudeString.ToString(); }
                }
                if (showAirspeed)
                {
                    // Show in km/h
                    airspeedString.Set((int)(sourceShip.shipInstance.LocalVelocity.z*3.6f));
                    if (airspeedText != null) { airspeedText.text = airspeedString.ToString(); }
                }

                if (isHeadingScrolling)
                {
                    Vector3 _shipFwd = sourceShip.shipInstance.TransformForward;
                    Vector3 _headingPerpendicular = Vector3.Cross(Vector3.forward, _shipFwd);
                    float _headingDir = -Vector3.Dot(_headingPerpendicular, Vector3.up);
                    // Get the forward angle of the ship in WS (ignoring the Up axis)
                    headingScrollImg.transform.position = headingInitPosition + new Vector3(Vector3.Angle(new Vector3(_shipFwd.x, 0f, _shipFwd.z), Vector3.forward) * Mathf.Sign(_headingDir) * headingPixelsPerDegree, 0f, 0f);
                }

                if (isAttitudeScrolling)
                {
                    float _rollAngle = sourceShip.shipInstance.RollAngle;
                    float _rollAngleRAD = -_rollAngle * Mathf.Deg2Rad;
                    float _scrollDistance = -sourceShip.shipInstance.PitchAngle * attitudePixelsPerDegree;
                    float _scrollRadius = _scrollDistance * Mathf.Sqrt(2 * (1 - Mathf.Cos(_rollAngleRAD)));
                    float _scrollX = _scrollRadius * Mathf.Cos(_rollAngleRAD/2f);
                    float _scrollY = _scrollRadius * Mathf.Sin(_rollAngleRAD/2f);

                    if (_rollAngle > 0f)
                    {
                        // Flip values when rolling to the right
                        _scrollX = -_scrollX;
                        _scrollY = -_scrollY;
                    }

                    Vector3 attitudeScrollOffset = new Vector3(_scrollX, _scrollDistance - _scrollY, 0f);

                    // Adjust the position and rotation
                    attitudeScrollImg.transform.position = attitudeInitPosition + attitudeScrollOffset;
                    // Rotate the panel so that the mask rotates at the same time.
                    attitudeRectTfrm.localRotation = Quaternion.Euler(0f, 0f, _rollAngle);

                    // Seems like we can adjust the image WS rotation at the same time, rather than having to do attitudeRectTfrm.localRotation.
                    //attitudeScrollImg.transform.SetPositionAndRotation(attitudeInitPosition + attitudeScrollOffset, Quaternion.Euler(0f, 0f, _rollAngle));

                    //Debug.Log("[DEBUG]  _rollAngle: " + _rollAngle.ToString("0.00") + " _rollAngleRAD: " + _rollAngleRAD.ToString("0.00") +
                    //         " _scrollDistance: " + _scrollDistance.ToString("0.00") + " _scrollRadius: " + _scrollRadius.ToString("0.00") +
                    //         " scrollX: " + _scrollX.ToString("0.00") + " scrollY: " + _scrollY.ToString("0.00"));
                }
            }

            #endregion

            #region Message updates
            if (IsHUDShown)
            {
                // NOTE: It might be cheaper to maintain an int[] of scrollable messages
                for (int dmIdx = 0; dmIdx < numDisplayMessages; dmIdx++)
                {
                    DisplayMessage displayMsg = displayMessageList[dmIdx];

                    #region Fade-in Message
                    if (displayMsg.fadeinTimer > 0f && displayMsg.fadeinDuration > 0f)
                    {
                        displayMsg.fadeinTimer -= dTime;

                        if (displayMsg.fadeinTimer <= 0f)
                        {
                            displayMsg.fadeinTimer = 0f;
                        }

                        SetDisplayMessageTextFade(displayMsg, false);
                        if (displayMsg.showBackground)
                        {
                            SetDisplayMessageBackgroundFade(displayMsg, false);
                        }
                    }
                    #endregion

                    #region Fade-out Message
                    else if (displayMsg.fadeoutTimer > 0f && displayMsg.fadeoutDuration > 0f)
                    {
                        displayMsg.fadeoutTimer -= dTime;

                        if (displayMsg.fadeoutTimer <= 0f)
                        {
                            displayMsg.fadeoutTimer = 0f;
                            displayMsg.showMessage = false;
                        }

                        SetDisplayMessageTextFade(displayMsg, true);
                        if (displayMsg.showBackground)
                        {
                            SetDisplayMessageBackgroundFade(displayMsg, true);
                        }
                    }

                    #endregion

                    if (displayMsg.scrollDirectionInt != DisplayMessage.ScrollDirectionNone)
                    {                     
                        ScrollMessage(displayMessageList[dmIdx]);
                    }
                }
            }

            #endregion

            #region Target Position Updates
            if (autoUpdateTargetPositions && IsHUDShown) { UpdateTargetPositions(); }
            #endregion
        }

        #endregion

        #region Private and Internal Methods - General

        /// <summary>
        /// Show or hide the hardware (mouse) cursor in the game view
        /// NOTE: This will sometimes fail to turn off the cursor in the editor
        /// Game View when it doesn't have focus, but will work fine in a build.
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideCursor(bool isShown)
        {
            Cursor.visible = isShown;
            isCursorVisible = isShown;
            if (isShown) { cursorTimer = 0f; }
        }

        /// <summary>
        /// Show or Hide the HUD.
        /// IsHUDShown should never be true at runtime if IsInitialised is false
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideHUD(bool isShown)
        {
            if (IsInitialised)
            {
                IsHUDShown = isShown;
                hudPanel.gameObject.SetActive(isShown);
            }
            // HUD should never be shown at runtime if IsInitialised is false
            else { IsHUDShown = false; }
        }

        private IEnumerator UnlockCursor()
        {
            yield return new WaitForEndOfFrame();
            Cursor.lockState = CursorLockMode.None;
        }

        /// <summary>
        /// Check to see if the window has been resized. Ideally should be
        /// called a max once per frame (less often would be nice).
        /// TODO - trigger a HUD refresh if changed.
        /// </summary>
        private void CheckHUDResize(bool isRefreshIfChanged)
        {
            cvsResolutionFull = GetHUDFullSize(true);
            cvsResolutionHalf.x = cvsResolutionFull.x * 0.5f;
            cvsResolutionHalf.y = cvsResolutionFull.y * 0.5f;

            if (cvsResolutionFull.x != prevResolutionFull.x || cvsResolutionFull.y != prevResolutionFull.y)
            {
                refResolution = GetHUDFullSize(false);

                cvsScaleFactor = canvas == null ? Vector3.one : canvas.transform.localScale;
                screenResolution = new Vector2(Screen.width, Screen.height);

                if (isRefreshIfChanged) { RefreshHUD(); }
                prevResolutionFull.x = cvsResolutionFull.x;
                prevResolutionFull.y = cvsResolutionFull.y;
            }
        }

        /// <summary>
        /// Return full the width and height of the HUD Panel.
        /// If the module hasn't been initialised it will return 1920x1080.
        /// Assume the dev hasn't changed the CanvasScaler reference resolution settings.
        /// When using the HUD canvas current dimensions, always set isScaled = true.
        /// Returns x,y values in pixels.
        /// </summary>
        /// <param name="isScaled"></param>
        /// <returns></returns>
        private Vector2 GetHUDFullSize(bool isScaled)
        {
            // Default SSC scaler resolution is 1920x1080
            if (isScaled)
            {
                if (IsInitialised || canvas != null)
                {
                    // The default [width, height] * inversed scaled [width, height]
                    //return new Vector2(canvas.pixelRect.width / canvas.scaleFactor, canvas.pixelRect.height / canvas.scaleFactor);
                    return ((RectTransform)canvas.transform).sizeDelta;
                }
                // We don't know, so just return the original size...
                else { return new Vector2(1920f, 1080f); }
            }
            else
            {
                if (IsInitialised || canvasScaler != null)
                {
                    return canvasScaler.referenceResolution;
                }
                else { return new Vector2(1920f, 1080f); }
            }
        }

        /// <summary>
        /// Return half the width and height of the HUD Panel.
        /// If the module hasn't been initialised it will return half 1920x1080.
        /// Assuming the dev hasn't changed it, it should always return the same
        /// when initialised.
        /// When using the HUD canvas current dimensions, always set isScaled = true.
        /// Returns x,y values in pixels.
        /// </summary>
        /// <param name="isScaled"></param>
        /// <returns></returns>
        private Vector2 GetHUDHalfSize(bool isScaled)
        {
            // Default scaler resolution is 1920x1080
            if (isScaled)
            {
                if (IsInitialised || canvas != null)
                {
                    // The default [width, height] * inversed scaled [width, height]
                    //return new Vector2(canvas.pixelRect.width / canvas.scaleFactor * 0.5f, canvas.pixelRect.height / canvas.scaleFactor * 0.5f);
                    return ((RectTransform)canvas.transform).sizeDelta * 0.5f;
                }
                // We don't know, so just return the original half size...
                else { return new Vector2(960f, 540f); }
            }
            else
            {
                if (IsInitialised || canvasScaler != null)
                {
                    return new Vector2(canvasScaler.referenceResolution.x * 0.5f, canvasScaler.referenceResolution.y * 0.5f);
                }
                else { return new Vector2(960f, 540f); }
            }
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Call CheckHUDResize(false) before calling this method
        /// </summary>
        private void GetHUDPanels()
        {
            if (hudRectTfrm != null)
            {
                hudPanel = hudRectTfrm.transform;
                reticlePanel = SSCUtils.GetChildTransform(hudPanel, "DisplayReticlePanel", this.name);
                overlayPanel = SSCUtils.GetChildRectTransform(hudPanel, overlayPanelName, this.name);

                // For backward compatibility with earlier versions of the HUD1 prefab
                if (overlayPanel == null)
                {
                    overlayPanel = SSCUtils.GetChildRectTransform(hudPanel, "AltitudePanel", this.name);
                    if (overlayPanel != null) { overlayPanel.name = overlayPanelName; }
                }

                altitudeTextRectTfrm = SSCUtils.GetChildRectTransform(overlayPanel, "AltitudeText", this.name);
                airspeedTextRectTfrm = SSCUtils.GetChildRectTransform(overlayPanel, "AirSpeedText", this.name);

                targetsRectTfrm = GetTargetsPanel();
                gaugesRectTfrm = GetGaugesPanel();
                headingRectTfrm = GetHeadingPanel();
            }
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        private void GetImgComponents()
        {
            if (reticlePanel != null) { displayReticleImg = reticlePanel.GetComponent<Image>(); }
            if (overlayPanel != null) { primaryOverlayImg = overlayPanel.GetComponent<Image>(); }
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        private void GetTextComponents()
        {
            if (altitudeTextRectTfrm != null) { altitudeText = altitudeTextRectTfrm.GetComponent<Text>(); }
            if (airspeedTextRectTfrm != null) { airspeedText = airspeedTextRectTfrm.GetComponent<Text>(); }
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used to change HUD outside play mode
        /// </summary>
        public void InitialiseEditorEssentials()
        {
            if (canvas == null) { canvas = GetCanvas; }

            hudRectTfrm = SSCUtils.GetChildRectTransform(transform, hudPanelName, this.name);

            if (hudRectTfrm != null)
            {
                // Call before GetHUDPanels()
                CheckHUDResize(false);

                GetHUDPanels();
                GetImgComponents();
                GetTextComponents();

                InitialiseAttitude(true);
                InitialiseHeading(true);
                InitialiseMessages();
                InitialiseTargets();
                editorMode = true;

                ShowOrHideAttitude(showAttitude);
                ShowOrHideHeading(showHeading);
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: ShipDisplayModule.InitialiseEditorEssentials() could not find canvas HUDPanel. Did you start with the sample HUD prefab from Prefabs/Visuals folder?"); }
            #endif
        }

        /// <summary>
        /// The Overlay Panel contains the Overlay image and child objects
        /// Altitude Text and Airspeed Text.
        /// </summary>
        private void ShowOrHideOverlayPanel()
        {
            // Should the Overlay panel be enabled?
            if ((showAltitude || showAirspeed || showOverlay) && !overlayPanel.gameObject.activeSelf)
            {
                overlayPanel.gameObject.SetActive(true);
            }
            else if (!showAltitude && !showAirspeed && !showOverlay && overlayPanel.gameObject.activeSelf)
            {
                overlayPanel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Show or Hide the Overlay image on the HUD. Turn on HUD if required
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideOverlay(bool isShown)
        {
            if (IsInitialised)
            {
                showOverlay = isShown;

                ShowOrHideOverlayPanel();

                if (primaryOverlayImg != null) { primaryOverlayImg.enabled = isShown; }

                if (showOverlay && !IsHUDShown) { ShowOrHideHUD(true); }
            }
        }

        #endregion

        #region Private and Internal Methods - Brightness

        /// <summary>
        /// Remembers the starting colours of various elements so that they can be
        /// used to apply brightness to those elements at runtime.
        /// Sets the initial brightness.
        /// NOTE: Brightness doesn't work so well if colour has low alpha to begin with.
        /// </summary>
        private void InitialiseBrightness()
        {
            if (displayReticleImg != null) { baseReticleColour = displayReticleImg.color; }
            if (primaryOverlayImg != null) { baseOverlayColour = primaryOverlayImg.color; }
            if (altitudeText != null) { baseAltitudeTextColour = altitudeText.color; }
            if (airspeedText != null) { baseAirspeedTextColour = airspeedText.color; }
            if (attitudeScrollImg != null) { baseAttitudePrimaryColour = attitudeScrollImg.color; }
            if (headingScrollImg != null) { baseHeadingPrimaryColour = headingScrollImg.color; }
            if (headingIndicatorImg != null) { baseHeadingIndicatorColour = headingIndicatorImg.color; }

            SetBrightness(brightness);
        }

        private void SetOverlayBrightness()
        {
            if (primaryOverlayImg != null)
            {
                // Skip more expensive HSV->RGBA and new Color op if possible
                if (brightness == 1f)
                {
                    SSCUtils.Color32toColorNoAlloc(ref primaryColour, ref tempColour);
                    primaryOverlayImg.color = tempColour;
                }
                else
                {
                    primaryOverlayImg.color = baseOverlayColour.GetColorWithBrightness(brightness);
                }
            }
        }

        private void SetReticleBrightness()
        {
            if (displayReticleImg != null)
            {
                // Skip more expensive HSV->RGBA and new Color op if possible
                if (brightness == 1f)
                {
                    SSCUtils.Color32toColorNoAlloc(ref activeDisplayReticleColour, ref tempColour);
                    displayReticleImg.color = tempColour;
                }
                else
                {
                    displayReticleImg.color = baseReticleColour.GetColorWithBrightness(brightness);
                }
            }
        }

        private void SetAltitudeTextBrightness()
        {
            if (altitudeText != null)
            {
                // Skip more expensive HSV->RGBA and new Color op if possible
                if (brightness == 1f)
                {
                    SSCUtils.Color32toColorNoAlloc(ref altitudeTextColour, ref tempColour);
                    altitudeText.color = tempColour;
                }
                else
                {
                    altitudeText.color = baseAltitudeTextColour.GetColorWithBrightness(brightness);
                }
            }
        }

        private void SetAirSpeedTextBrightness()
        {
            if (airspeedText != null)
            {
                // Skip more expensive HSV->RGBA and new Color op if possible
                if (brightness == 1f)
                {
                    SSCUtils.Color32toColorNoAlloc(ref airspeedTextColour, ref tempColour);
                    airspeedText.color = tempColour;
                }
                else
                {
                    airspeedText.color = baseAirspeedTextColour.GetColorWithBrightness(brightness);
                }
            }
        }

        private void SetAttitudeBrightness()
        {
            if (attitudeScrollImg != null)
            {
                // Skip more expensive HSV->RGBA and new Color op if possible
                if (brightness == 1f)
                {
                    attitudeScrollImg.color = attitudePrimaryColour;
                }
                else
                {
                    attitudeScrollImg.color = baseAttitudePrimaryColour.GetColorWithBrightness(brightness);
                }
            }
        }

        private void SetHeadingBrightness()
        {
            if (headingScrollImg != null)
            {
                // Skip more expensive HSV->RGBA and new Color op if possible
                if (brightness == 1f)
                {
                    headingScrollImg.color = headingPrimaryColour;
                    headingIndicatorImg.color = headingIndicatorColour;
                }
                else
                {
                    headingScrollImg.color = baseHeadingPrimaryColour.GetColorWithBrightness(brightness);
                    headingIndicatorImg.color = baseHeadingIndicatorColour.GetColorWithBrightness(brightness);
                }
            }
        }

        private void SetDisplayGaugeForegroundBrightness(DisplayGauge displayGauge)
        {
            if (displayGauge != null)
            {
                if (IsInitialised || editorMode)
                {
                    if (displayGauge.CachedFgImgComponent != null)
                    {
                        if (brightness == 1f) { displayGauge.CachedFgImgComponent.color = displayGauge.foregroundColour; }
                        else { displayGauge.CachedFgImgComponent.color = displayGauge.baseForegroundColour.GetColorWithBrightness(brightness); }
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeForegroundBrightness - displayGauge.CachedFgImgComponent is null"); }
                    #endif
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeForegroundBrightness - displayGauge is null"); }
            #endif
        }

        private void SetDisplayGaugeBackgroundBrightness(DisplayGauge displayGauge)
        {
            if (displayGauge != null)
            {
                if (IsInitialised || editorMode)
                {
                    if (displayGauge.CachedBgImgComponent != null)
                    {
                        if (brightness == 1f) { displayGauge.CachedBgImgComponent.color = displayGauge.backgroundColour; }
                        else { displayGauge.CachedBgImgComponent.color = displayGauge.baseBackgroundColour.GetColorWithBrightness(brightness); }
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeBackgroundBrightness - displayGauge.CachedBgImgComponent is null"); }
                    #endif
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeBackgroundBrightness - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Adjust the brightness of the gauge text. If it is a numeric gauge with a label,
        /// also adjust the brightness of the label.
        /// </summary>
        /// <param name="displayGauge"></param>
        private void SetDisplayGaugeTextBrightness(DisplayGauge displayGauge)
        {
            if (displayGauge != null)
            {
                if (IsInitialised || editorMode)
                {
                    if (displayGauge.CachedTextComponent != null)
                    {
                        if (brightness == 1f) { displayGauge.CachedTextComponent.color = displayGauge.textColour; }
                        else { displayGauge.CachedTextComponent.color = displayGauge.baseTextColour.GetColorWithBrightness(brightness); }
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeTextBrightness - displayGauge.CachedTextComponent is null"); }
                    #endif

                    if (displayGauge.CachedLabelTextComponent != null)
                    {
                        if (brightness == 1f) { displayGauge.CachedLabelTextComponent.color = displayGauge.textColour; }
                        else { displayGauge.CachedLabelTextComponent.color = displayGauge.baseTextColour.GetColorWithBrightness(brightness); }
                    }
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeTextBrightness - displayGauge is null"); }
            #endif
        }

        private void SetDisplayMessageBackgroundBrightness(DisplayMessage displayMessage)
        {
            if (displayMessage != null)
            {
                if (IsInitialised || editorMode)
                {
                    if (displayMessage.CachedBgImgComponent != null)
                    {
                        if (brightness == 1f) { displayMessage.CachedBgImgComponent.color = displayMessage.backgroundColour; }
                        else { displayMessage.CachedBgImgComponent.color = displayMessage.baseBackgroundColour.GetColorWithBrightness(brightness); }
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayMessageBackgroundBrightness - displayMessage.CachedBgImgComponent is null"); }
                    #endif
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayMessageBackgroundBrightness - displayMessage is null"); }
            #endif
        }

        private void SetDisplayMessageBackgroundFade (DisplayMessage displayMessage, bool isFadeOut)
        {
            if (displayMessage != null)
            {
                if (IsInitialised || editorMode)
                {
                    if (displayMessage.CachedBgImgComponent != null)
                    {
                        float fadeValue = easeInOutCurve.Evaluate(isFadeOut ? displayMessage.GetFadeOutValue() : displayMessage.GetFadeInValue());

                        displayMessage.CachedBgImgComponent.color = displayMessage.baseBackgroundColour.GetColorWithFadedBrightness(brightness, fadeValue);
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayMessageBackgroundFade - displayMessage.CachedBgImgComponent is null"); }
                    #endif
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayMessageBackgroundFade - displayMessage is null"); }
            #endif
        }

        private void SetDisplayMessageTextBrightness(DisplayMessage displayMessage)
        {
            if (displayMessage != null)
            {
                if (IsInitialised || editorMode)
                {
                    if (displayMessage.CachedTextComponent != null)
                    {
                        if (brightness == 1f) { displayMessage.CachedTextComponent.color = displayMessage.textColour; }
                        else { displayMessage.CachedTextComponent.color = displayMessage.baseTextColour.GetColorWithBrightness(brightness); }
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayMessageTextBrightness - displayMessage.CachedTextComponent is null"); }
                    #endif
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayMessageTextBrightness - displayMessage is null"); }
            #endif
        }

        private void SetDisplayMessageTextFade (DisplayMessage displayMessage, bool isFadeOut)
        {
            if (displayMessage != null)
            {
                if (IsInitialised || editorMode)
                {
                    if (displayMessage.CachedTextComponent != null)
                    {
                        float fadeValue = easeInOutCurve.Evaluate(isFadeOut ? displayMessage.GetFadeOutValue() : displayMessage.GetFadeInValue());

                        //Debug.Log("[DEBUG] msg fadeinTimer: " + displayMessage.fadeinTimer + " fadeValue: " + fadeValue);

                        displayMessage.CachedTextComponent.color = displayMessage.baseTextColour.GetColorWithFadedBrightness(brightness, fadeValue);
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayMessageTextFade - displayMessage.CachedTextComponent is null"); }
                    #endif
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayMessageTextFade - displayMessage is null"); }
            #endif
        }

        /// <summary>
        /// Set the brightness of all slots (copies) of a DisplayTarget
        /// </summary>
        /// <param name="displayTarget"></param>
        private void SetDisplayTargetBrightness(DisplayTarget displayTarget)
        {
            if (displayTarget != null)
            {
                if (IsInitialised || editorMode)
                {
                    for (int sIdx = 0; sIdx < displayTarget.maxNumberOfTargets; sIdx++)
                    {
                        if (displayTarget.displayTargetSlotList[sIdx].CachedImgComponent != null)
                        {
                            if (brightness == 1f) { displayTarget.displayTargetSlotList[sIdx].CachedImgComponent.color = displayTarget.reticleColour; }
                            else { displayTarget.displayTargetSlotList[sIdx].CachedImgComponent.color = displayTarget.baseReticleColour.GetColorWithBrightness(brightness); }
                        }
                        #if UNITY_EDITOR
                        else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayTargetBrightness - displayTarget slot " + sIdx + " CachedImgComponent is null"); }
                        #endif
                    }
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayTargetBrightness - displayTarget is null"); }
            #endif
        }

        #endregion

        #region Private or Internal Methods - Reticle

        /// <summary>
        /// Show or Hide the reticle. Turn on the HUD if required.
        /// Always hide the cursor (hardware pointer) when the reticle is shown.
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideReticle(bool isShown)
        {
            if (IsInitialised)
            {
                showActiveDisplayReticle = isShown;

                if (isShown)
                {
                    // Check the active display reticle
                    if (guidHashActiveDisplayReticle == 0)
                    {
                        showActiveDisplayReticle = false;
                        currentDisplayReticle = null;
                        LoadDisplayReticleSprite(currentDisplayReticle);

                        #if UNITY_EDITOR
                        Debug.LogWarning("ShipDisplayModule - could not show the Display Rectile because there was no active rectile");
                        #endif
                    }
                    // If there is no current reticle, or if we need to change the reticle,
                    // look up the hash value and load the sprite (if there is a match)
                    else if (currentDisplayReticle == null || (currentDisplayReticle.guidHash != guidHashActiveDisplayReticle))
                    {
                        currentDisplayReticle = displayReticleList.Find(dr => dr.guidHash == guidHashActiveDisplayReticle);
                        LoadDisplayReticleSprite(currentDisplayReticle);
                    }
                }

                if (showActiveDisplayReticle && lockDisplayReticleToCursor) { Cursor.visible = false; }

                reticlePanel.gameObject.SetActive(showActiveDisplayReticle);

                if (showActiveDisplayReticle && !IsHUDShown) { ShowOrHideHUD(true); }
            }
        }

        /// <summary>
        /// Load the reticle sprite into the UI image on the panel
        /// </summary>
        /// <param name="displayReticle"></param>
        private void LoadDisplayReticleSprite(DisplayReticle displayReticle)
        {
            if (displayReticle != null)
            {
                displayReticleImg.sprite = displayReticle.primarySprite;
            }
            else
            {
                displayReticleImg.sprite = null;
            }
        }

        #endregion

        #region Private or Internal Methods - Overlay, Altitude and Speed

        /// <summary>
        /// Show or Hide the Altitude indicator on the HUD. Turn on HUD if required.
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideAltitude(bool isShown)
        {
            if (IsInitialised)
            {
                showAltitude = isShown;

                ShowOrHideOverlayPanel();

                if (altitudeText != null) { altitudeText.gameObject.SetActive(showAltitude); }

                if (showAltitude && !IsHUDShown) { ShowOrHideHUD(true); }
            }
        }

        /// <summary>
        /// Show or Hide the Air Speed indicator on the HUD. Turn on HUD if required
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideAirSpeed(bool isShown)
        {
            if (IsInitialised)
            {
                showAirspeed = isShown;

                ShowOrHideOverlayPanel();

                if (airspeedText != null) { airspeedText.gameObject.SetActive(showAirspeed); }

                if (showAirspeed && !IsHUDShown) { ShowOrHideHUD(true); }
            }
        }

        #endregion

        #region Private or Internal Methods - Attitude

        /// <summary>
        /// Get or Create the scrollable attitude.
        /// The attitude image is masked by the parent mask image.
        /// </summary>
        private void GetOrCreateAttitude()
        {
            if (attitudeRectTfrm == null) { attitudeRectTfrm = GetAttitudePanel(); }

            if (attitudeRectTfrm != null)
            {
                // Find or create the Mask for the scrollable attitude
                if (attitudeMaskImg == null)
                {
                    attitudeMaskImg = attitudeRectTfrm.GetComponent<UnityEngine.UI.Image>();

                    if (attitudeMaskImg == null)
                    {
                        attitudeMaskImg = attitudeRectTfrm.gameObject.AddComponent<UnityEngine.UI.Image>();
                        UnityEngine.UI.Mask attitudeMask = attitudeRectTfrm.gameObject.AddComponent<UnityEngine.UI.Mask>();

                        if (attitudeMaskImg != null)
                        {
                            attitudeMaskImg.raycastTarget = false;
                            attitudeMaskImg.preserveAspect = false;
                        }
                        #if UNITY_EDITOR
                        else { Debug.LogWarning("ERROR: shipDisplayModule.GetOrCreateAttitude() - could not add mask image component to Attitude panel"); }
                        #endif

                        if (attitudeMask != null)
                        {
                            attitudeMask.showMaskGraphic = false;
                        }
                        #if UNITY_EDITOR
                        else { Debug.LogWarning("ERROR: shipDisplayModule.GetOrCreateAttitude() - could not add mask component to Attitude panel"); }
                        #endif
                    }
                }
            }

            // Find or create the scrollable panel and image. This is a child of the attitude panel and mask
            if (attitudeScrollImg == null)
            {
                // The panel width and height are 1.0 which is the full resolution e.g. 1920x1080
                attitudeScrollPanel = SSCUtils.GetOrCreateChildRectTransform(attitudeRectTfrm, cvsResolutionFull, attitudeScrollName,
                                                    0f, 0f, 1f, 1f, 0.5f, 0.5f, 0.5f, 0.5f);

                if (attitudeScrollPanel != null)
                {
                    attitudeScrollImg = attitudeScrollPanel.GetComponent<UnityEngine.UI.Image>();

                    if (attitudeScrollImg == null)
                    {
                        attitudeScrollImg = attitudeScrollPanel.gameObject.AddComponent<UnityEngine.UI.Image>();

                        if (attitudeScrollImg != null)
                        {
                            attitudeScrollImg.raycastTarget = false;
                            attitudeScrollImg.preserveAspect = true;
                        }
                        #if UNITY_EDITOR
                        else { Debug.LogWarning("ERROR: shipDisplayModule.GetOrCreateAttitude() - could not add background to Attitude panel"); }
                        #endif
                    }
                }
            }


            // TODO - Find or create the small indicator panel and image (NOT SURE IF WE NEED THIS YET).
        }

        /// <summary>
        /// Initialise the scrollable Attitude.
        /// </summary>
        /// <param name="isInitialising"></param>
        private void InitialiseAttitude(bool isInitialising)
        {
            GetOrCreateAttitude();

            SetDisplayAttitudeScrollSprite(attitudeScrollSprite);
            SetDisplayAttitudeMaskSprite(attitudeMaskSprite);

            SetDisplayAttitudeOffset(attitudeOffsetX, attitudeOffsetY);
            SetDisplayAttitudeSize(attitudeWidth, attitudeHeight);

            if (attitudeScrollImg != null)
            {
                if (isInitialising)
                {
                    attitudeInitPosition = attitudeScrollImg.transform.position;
                }

                RefreshAttitudeAfterResize();
            }
        }

        /// <summary>
        /// Show or hide the scrollable Attitude display. Turn on HUD if required.
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideAttitude(bool isShown)
        {
            if (IsInitialised || IsEditorMode)
            {
                showAttitude = isShown;

                if (attitudeRectTfrm != null) { attitudeRectTfrm.gameObject.SetActive(showAttitude); }

                if (attitudeScrollImg != null)
                {
                    attitudeScrollImg.enabled = isShown;
                    isAttitudeScrolling = isShown;
                }
                else
                {
                    isAttitudeScrolling = false;
                }

                if (showAttitude && !IsHUDShown) { ShowOrHideHUD(true); }
            }
        }

        #endregion

        #region Private or Internal Methods - Fade

        /// <summary>
        /// This is private because it is called from StartDisplayFade()
        /// </summary>
        /// <returns></returns>
        private IEnumerator FadeDisplayIn()
        {
            float currentAlphaValue = fadeStartAlpha;
            isFadingIn = true;
            fadeImage.enabled = true;
            if (fadeInDuration == 0) { fadeInDuration = 0.1f; }

            float currentTimeFadeValue = fadeStartAlpha;

            while (currentAlphaValue > fadeTargetValue)
            {
                // Use Unscaled time so not to be affected by Time.timeScale
                currentTimeFadeValue -= Time.unscaledDeltaTime / fadeInDuration;

                // Fade out slowly at first because alpha values < 0.4 have limited visual effect
                currentAlphaValue = Mathf.Pow(Mathf.Clamp01(currentTimeFadeValue), 0.33f);

                currentFadeColour.a = currentAlphaValue;
                fadeImage.color = currentFadeColour;
                //fadeImage.color = new Color(Color.black.r, Color.black.g, Color.black.b, currentAlphaValue);
                yield return null;
            }
            fadeImage.enabled = false;
            isFadingIn = false;
            yield return null;
        }

        /// <summary>
        /// This is private because it is called from StartDisplayFade()
        /// </summary>
        private IEnumerator FadeDisplayOut()
        {
            float currentAlphaValue = fadeStartAlpha;
            isFadingOut = true;
            fadeImage.enabled = true;
            if (fadeOutDuration == 0) { fadeOutDuration = 0.1f; }
            while (currentAlphaValue <= fadeTargetValue)
            {
                // Use Unscaled time so not to be affected by Time.timeScale
                currentAlphaValue += Time.unscaledDeltaTime / fadeOutDuration;

                currentFadeColour.a = currentAlphaValue;
                fadeImage.color = currentFadeColour;
                //fadeImage.color = new Color(Color.black.r, Color.black.g, Color.black.b, currentAlphaValue);
                yield return null;
            }            
            isFadingOut = false;
            yield return null;
        }

        #endregion

        #region Private or Internal Methods - Gauges

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Create a new Gauge RectTransform under the HUD GaugesPanel in the hierarchy
        /// Add a background Image
        /// Add a (foreground) fillable iamge
        /// Add a text panel
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <returns></returns>
        public RectTransform CreateGaugePanel(DisplayGauge displayGauge)
        {
            RectTransform gaugePanel = null;

            if (gaugesRectTfrm == null) { gaugesRectTfrm = GetGaugesPanel(); }

            if (gaugesRectTfrm != null && displayGauge != null)
            {
                float panelWidth = displayGauge.displayWidth;
                float panelHeight = displayGauge.displayHeight;

                gaugePanel = SSCUtils.GetOrCreateChildRectTransform(gaugesRectTfrm, cvsResolutionFull, "Gauge_" + displayGauge.guidHash, 0f, 0f,
                                                                        panelWidth, panelHeight, 0.5f, 0.5f, 0.5f, 0.5f);

                if (gaugePanel != null)
                {
                    Image bgimgComponent = gaugePanel.gameObject.AddComponent<Image>();
                    if (bgimgComponent != null)
                    {
                        bgimgComponent.raycastTarget = false;
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: shipDisplayModule.CreateGaugePanel() - could not add background to Gauge panel"); }
                    #endif

                    RectTransform gaugeAmtPanel = SSCUtils.GetOrCreateChildRectTransform(gaugePanel, cvsResolutionFull, "GaugeAmount", 0f, 0f,
                                                                        panelWidth, panelHeight, 0.5f, 0.5f, 0.5f, 0.5f);

                    if (gaugeAmtPanel != null)
                    {
                        Image amtimgComponent = gaugeAmtPanel.gameObject.AddComponent<Image>();
                        if (amtimgComponent != null)
                        {
                            amtimgComponent.raycastTarget = false;
                            amtimgComponent.type = Image.Type.Filled;
                            amtimgComponent.fillMethod = (Image.FillMethod)displayGauge.fillMethod;
                        }
                        #if UNITY_EDITOR
                        else { Debug.LogWarning("ERROR: shipDisplayModule.CreateGaugePanel() - could not add fill image to Gauge panel"); }
                        #endif
                    }

                    RectTransform gaugeTxtPanel = SSCUtils.GetOrCreateChildRectTransform(gaugePanel, cvsResolutionFull, "GaugeText", 0f, 0f,
                                                                        panelWidth, panelHeight, 0.5f, 0.5f, 0.5f, 0.5f);

                    if (gaugeTxtPanel != null)
                    {
                        UnityEngine.UI.Text textComponent = gaugeTxtPanel.gameObject.AddComponent<UnityEngine.UI.Text>();
                        if (textComponent != null)
                        {
                            textComponent.raycastTarget = false;
                            textComponent.resizeTextForBestFit = true;
                            // Text is add in InitialiseGauge().
                            textComponent.text = string.Empty;
                            if (Application.isPlaying)
                            {
                                textComponent.font = SSCUtils.GetDefaultFont();
                            }
                        }
                        #if UNITY_EDITOR
                        else { Debug.LogWarning("ERROR: shipDisplayModule.CreateGaugePanel() - could not add text to Gauge panel"); }
                        #endif
                    }

                    if (IsInitialised || editorMode)
                    {
                        // Update the number of gauges. Count the list rather than assume the gauge didn't already exist.
                        numDisplayGauges = displayGaugeList == null ? 0 : displayGaugeList.Count;
                        InitialiseGauge(displayGauge);
                    }
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: ShipDisplayModule.CreateGaugePanel() - displayGauge is null or could not find or create " + gaugesPanelName); }
            #endif

            return gaugePanel;
        }

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Find the Gauge RectTransform under the HUD in the hierarchy.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        private RectTransform GetGaugePanel(int guidHash)
        {
            if (gaugesRectTfrm == null) { gaugesRectTfrm = GetGaugesPanel(); }
            if (gaugesRectTfrm != null)
            {
                return SSCUtils.GetChildRectTransform(gaugesRectTfrm.transform, "Gauge_" + guidHash, this.name);
            }
            else { return null; }
        }

        /// <summary>
        /// Get or create a gauge label. This only applies to numeric gauges with an additional
        /// text component or label.
        /// If it is the wrong gauge type, remove any secondary label.
        /// This will set or update the displayGauge.CachedLabelTextComponent.
        /// </summary>
        /// <param name="guidHash"></param>
        private void GetOrCreateOrRemoveGaugeLabel(DisplayGauge displayGauge)
        {
            UnityEngine.UI.Text textComponent = null;

            if (displayGauge != null)
            {
                int gaugeTypeInt = (int)displayGauge.gaugeType;

                // Is the component already cached?
                if (displayGauge.CachedLabelTextComponent != null)
                {
                    // If this gauge should have a secondary label, set it to the cached component
                    if (gaugeTypeInt == DisplayGauge.DGTypeNumberWithLabel1Int)
                    {
                        textComponent = displayGauge.CachedLabelTextComponent;
                    }
                    else
                    {
                        // A secondary label (Text component) does not apply to this gauge, so remove it.
                        if (Application.isPlaying) { Destroy(displayGauge.CachedLabelTextComponent.gameObject); }
                        else
                        {
                            #if UNITY_EDITOR
                            UnityEditor.Undo.DestroyObjectImmediate(displayGauge.CachedLabelTextComponent.gameObject);
                            #else
                            DestroyImmediate(displayGauge.CachedLabelTextComponent.gameObject);
                            #endif
                        }

                        // Clear the cached component
                        displayGauge.CachedLabelTextComponent = null;
                    }                  
                }
                else if (gaugeTypeInt == DisplayGauge.DGTypeNumberWithLabel1Int)
                {
                    // Not cached, so attempt to find it
                    RectTransform gaugePanel = displayGauge.CachedGaugePanel == null ? GetGaugePanel(displayGauge.guidHash) : displayGauge.CachedGaugePanel;

                    if (gaugePanel != null)
                    {
                        float panelWidth = displayGauge.displayWidth;
                        float panelHeight = displayGauge.displayHeight / 2f;

                        RectTransform gaugeLabelPanel = SSCUtils.GetOrCreateChildRectTransform(gaugePanel, cvsResolutionFull, "GaugeLabel", 0f, 0f,
                                                        panelWidth, panelHeight, 0.5f, 0.5f, 0.5f, 0.5f);

                        if (gaugeLabelPanel != null)
                        {
                            textComponent = gaugeLabelPanel.GetComponent<UnityEngine.UI.Text>();

                            if (textComponent == null)
                            {
                                // Text component doesn't exist, so add and configure it
                                textComponent = gaugeLabelPanel.gameObject.AddComponent<UnityEngine.UI.Text>();

                                if (textComponent != null)
                                {
                                    textComponent.raycastTarget = false;
                                    textComponent.resizeTextForBestFit = true;

                                    textComponent.text = displayGauge.gaugeLabel;
                                    if (Application.isPlaying)
                                    {
                                        textComponent.font = SSCUtils.GetDefaultFont();
                                    }

                                    displayGauge.CachedLabelTextComponent = textComponent;
                                }
                                #if UNITY_EDITOR
                                else { Debug.LogWarning("ERROR: shipDisplayModule.CreateGaugePanel() - could not add text to Gauge panel"); }
                                #endif
                            }
                            else
                            {
                                displayGauge.CachedLabelTextComponent = textComponent;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cache the DisplayGauge RectTransform, Image and Text component.
        /// Set the Text colour.
        /// </summary>
        /// <param name="displayGauge"></param>
        private void InitialiseGauge(DisplayGauge displayGauge)
        {
            displayGauge.CachedGaugePanel = GetGaugePanel(displayGauge.guidHash);
            // Cache FillMethod is None to avoid enum lookup at runtime
            displayGauge.isFillMethodNone = displayGauge.fillMethod == DisplayGauge.DGFillMethod.None;

            if (displayGauge.CachedGaugePanel != null)
            {
                displayGauge.CachedBgImgComponent = displayGauge.CachedGaugePanel.GetComponent<Image>();

                if (displayGauge.CachedBgImgComponent != null)
                {
                    // If there is a sprite but it hasn't been loaded into the image, do it now
                    if (displayGauge.backgroundSprite != null && displayGauge.CachedBgImgComponent.sprite == null)
                    {
                        SetDisplayGaugeBackgroundSprite(displayGauge, displayGauge.backgroundSprite);
                    }

                    // Cache the gauge foreground Image component
                    if (tempImgList == null) { tempImgList = new List<Image>(1); }
                    displayGauge.CachedGaugePanel.GetComponentsInChildren(tempImgList);
                    if (tempImgList.Count > 0)
                    {
                        // GetComponentsInChildren will also return the background image which we don't want
                        tempImgList.Remove(displayGauge.CachedBgImgComponent);

                        if (tempImgList.Count > 0)
                        {
                            displayGauge.CachedFgImgComponent = tempImgList[0];

                            // If there is a sprite but it hasn't been loaded into the image, do it now
                            if (displayGauge.foregroundSprite != null && displayGauge.CachedFgImgComponent.sprite == null)
                            {
                                SetDisplayGaugeForegroundSprite(displayGauge, displayGauge.foregroundSprite);
                            }

                            SetDisplayGaugeValue(displayGauge, displayGauge.gaugeValue);
                        }
                    }
                }

                // Cache the gauge Text component
                if (tempTextList == null) { tempTextList = new List<Text>(2); }
                displayGauge.CachedGaugePanel.GetComponentsInChildren(tempTextList);
                if (tempTextList.Count > 0)
                {
                    displayGauge.CachedTextComponent = tempTextList[0];

                    // NOTE: The order may not be guaranteed here so this might or might not be an issue
                    // with numeric gauges.
                    if (tempTextList.Count > 1)
                    {
                        displayGauge.CachedLabelTextComponent = tempTextList[1];
                    }
                }

                // SetDisplayGaugeForegroundColour does not update the baseForegroundColour if the foregroundColour has not changed. So do it in here instead
                displayGauge.baseForegroundColour.Set(displayGauge.foregroundColour.r, displayGauge.foregroundColour.g, displayGauge.foregroundColour.b, displayGauge.foregroundColour.a, true);
                SetDisplayGaugeForegroundBrightness(displayGauge);

                // SetDisplayGaugeBackgroundColour does not update the baseBackgroundColour if the backgroundColour has not changed. So do it in here instead
                displayGauge.baseBackgroundColour.Set(displayGauge.backgroundColour.r, displayGauge.backgroundColour.g, displayGauge.backgroundColour.b, displayGauge.backgroundColour.a, true);
                SetDisplayGaugeBackgroundBrightness(displayGauge);

                // SetDisplayGaugeTextColour does not update the baseTextColour if the textColour has not changed. So do it in here instead
                displayGauge.baseTextColour.Set(displayGauge.textColour.r, displayGauge.textColour.g, displayGauge.textColour.b, displayGauge.textColour.a, true);
                SetDisplayGaugeTextBrightness(displayGauge);

                SetDisplayGaugeText(displayGauge, displayGauge.gaugeString);
                SetDisplayGaugeTextAlignment(displayGauge, displayGauge.textAlignment);
                SetDisplayGaugeTextFontStyle(displayGauge, (UnityEngine.FontStyle)displayGauge.fontStyle);
                SetDisplayGaugeTextFontSize(displayGauge, displayGauge.isBestFit, displayGauge.fontMinSize, displayGauge.fontMaxSize);

                if (displayGauge.CachedLabelTextComponent != null)
                {
                    SetDisplayGaugeLabel(displayGauge, displayGauge.gaugeLabel);
                    SetDisplayGaugeLabelAlignment(displayGauge, displayGauge.labelAlignment);
                }

                SetDisplayGaugeSize(displayGauge, displayGauge.displayWidth, displayGauge.displayHeight);

                SetDisplayGaugeOffset(displayGauge, displayGauge.offsetX, displayGauge.offsetY);

                // By default gauges are initialised off. This is to prevent them suddenly appearing when first created.
                if (Application.isPlaying)
                {
                    displayGauge.CachedGaugePanel.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Initialise all the DisplayGauges by caching the RectTransforms.
        /// </summary>
        private void InitialiseGauges()
        {
            int _numDisplayGauges = GetNumberDisplayGauges;
            
            if (_numDisplayGauges > 0)
            {
                RectTransform hudRectTfrm = GetHUDPanel();
                if (hudRectTfrm != null)
                {
                    CheckHUDResize(false);
                    for (int dgIdx = 0; dgIdx < _numDisplayGauges; dgIdx++)
                    {
                        InitialiseGauge(displayGaugeList[dgIdx]);
                    }

                    if (_numDisplayGauges > 0) { RefreshGaugesSortOrder(); }
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ShipDisplayModule.InitialiseGauges() - could not find HUD Panel"); }
                #endif
            }
        }

        /// <summary>
        /// Show or Hide the Display Gauge on the HUD. Turn on the HUD if required.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="isShown"></param>
        private void ShowOrHideGauge(DisplayGauge displayGauge, bool isShown)
        {
            if (IsInitialised)
            {
                if (displayGauge.CachedGaugePanel != null)
                {
                    displayGauge.showGauge = isShown;
                    displayGauge.CachedGaugePanel.gameObject.SetActive(isShown);
                }

                if (isShown && !IsHUDShown) { ShowOrHideHUD(true); }
            }
        }

        /// <summary>
        /// Show or hide all gauges
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideGauges(bool isShown)
        {
            if (IsInitialised)
            {
                for (int dmIdx = 0; dmIdx < numDisplayGauges; dmIdx++)
                {
                    ShowOrHideGauge(displayGaugeList[dmIdx], isShown);
                }
            }
        }

        /// <summary>
        /// Show or Hide all gauges based on their current settings
        /// </summary>
        private void ShowOrHideGauges()
        {
            if (IsInitialised)
            {
                for (int dmIdx = 0; dmIdx < numDisplayGauges; dmIdx++)
                {
                    DisplayGauge displayGauge = displayGaugeList[dmIdx];
                    ShowOrHideGauge(displayGauge, displayGauge.showGauge);
                }
            }
        }

        #endregion

        #region Private or Internal Methods - Heading

        /// <summary>
        /// Initialise the scrollable heading or compass.
        /// </summary>
        /// <param name="isInitialising"></param>
        private void InitialiseHeading(bool isInitialising)
        {
            GetOrCreateHeading();

            SetDisplayHeadingScrollSprite(headingScrollSprite);
            SetDisplayHeadingMaskSprite(headingMaskSprite);
            SetDisplayHeadingIndicatorSprite(headingIndicatorSprite);

            SetDisplayHeadingOffset(headingOffsetX, headingOffsetY);
            SetDisplayHeadingSize(headingWidth, headingHeight);

            if (isInitialising && headingScrollImg != null)
            {
                headingInitPosition = headingScrollImg.transform.position;

                // The heading img should have 2 sets of 0-360 deg to avoid scrolling partly out of view.
                // The image is autoscaled to the full width of the default HUD screen size.
                headingPixelsPerDegree = cvsResolutionFull.x / 720f;
            }
        }

        /// <summary>
        /// Get or Create the scrollable heading.
        /// The heading image is masked by the parent mask image.
        /// </summary>
        private void GetOrCreateHeading()
        {
            if (headingRectTfrm == null) { headingRectTfrm = GetHeadingPanel(); }

            if (headingRectTfrm != null)
            {
                // Find or create the Mask for the scrollable heading
                if (headingMaskImg == null)
                {
                    headingMaskImg = headingRectTfrm.GetComponent<UnityEngine.UI.Image>();

                    if (headingMaskImg == null)
                    {
                        headingMaskImg = headingRectTfrm.gameObject.AddComponent<UnityEngine.UI.Image>();
                        UnityEngine.UI.Mask headingMask = headingRectTfrm.gameObject.AddComponent<UnityEngine.UI.Mask>();

                        if (headingMaskImg != null)
                        {
                            headingMaskImg.raycastTarget = false;
                            headingMaskImg.preserveAspect = false;
                        }
                        #if UNITY_EDITOR
                        else { Debug.LogWarning("ERROR: shipDisplayModule.GetOrCreateHeading() - could not add mask image component to Heading panel"); }
                        #endif

                        if (headingMask != null)
                        {
                            headingMask.showMaskGraphic = false;
                        }
                        #if UNITY_EDITOR
                        else { Debug.LogWarning("ERROR: shipDisplayModule.GetOrCreateHeading() - could not add mask component to Heading panel"); }
                        #endif
                    }
                }

                // Find or create the scrollable panel and image. This is a child of the heading panel and mask
                if (headingScrollImg == null)
                {
                    // The panel width and height are 1.0 which is the full resolution e.g. 1920x1080
                    headingScrollPanel = SSCUtils.GetOrCreateChildRectTransform(headingRectTfrm, cvsResolutionFull, headingScrollName,
                                                        0f, 0f, 1f, 1f, 0.5f, 0.5f, 0.5f, 0.5f);

                    if (headingScrollPanel != null)
                    {
                        headingScrollImg = headingScrollPanel.GetComponent<UnityEngine.UI.Image>();

                        if (headingScrollImg == null)
                        {
                            headingScrollImg = headingScrollPanel.gameObject.AddComponent<UnityEngine.UI.Image>();

                            if (headingScrollImg != null)
                            {
                                headingScrollImg.raycastTarget = false;
                                headingScrollImg.preserveAspect = true;
                            }
                            #if UNITY_EDITOR
                            else { Debug.LogWarning("ERROR: shipDisplayModule.GetOrCreateHeading() - could not add background to Heading panel"); }
                            #endif
                        }
                    }
                }

                // Find or create the small indicator panel and image.
                if (headingIndicatorImg == null)
                {
                    // The panel width and height are 1.0 which is the full resolution e.g. 1920x1080
                    headingIndicatorPanel = SSCUtils.GetOrCreateChildRectTransform(headingRectTfrm, cvsResolutionFull, headingIndicatorName,
                                                        0f, 0f, 16f / cvsResolutionFull.x, 16f / cvsResolutionFull.y, 0.5f, 0.5f, 0.5f, 0.5f);

                    if (headingIndicatorPanel != null)
                    {
                        headingIndicatorImg = headingIndicatorPanel.GetComponent<UnityEngine.UI.Image>();

                        if (headingIndicatorImg == null)
                        {
                            headingIndicatorImg = headingIndicatorPanel.gameObject.AddComponent<UnityEngine.UI.Image>();

                            if (headingIndicatorImg != null)
                            {
                                headingIndicatorImg.raycastTarget = false;
                                headingIndicatorImg.preserveAspect = true;
                            }
                            #if UNITY_EDITOR
                            else { Debug.LogWarning("ERROR: shipDisplayModule.GetOrCreateHeading() - could not add indicator to Heading panel"); }
                            #endif
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Show or Hide the scrollable Heading on the HUD. Turn on HUD if required.
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideHeading(bool isShown)
        {
            if (IsInitialised || IsEditorMode)
            {
                showHeading = isShown;

                if (headingRectTfrm != null) { headingRectTfrm.gameObject.SetActive(showHeading); }

                if (headingScrollImg != null)
                {
                    headingScrollImg.enabled = isShown;
                    isHeadingScrolling = isShown;
                }
                else 
                {
                    isHeadingScrolling = false;
                }

                if (showHeading && !IsHUDShown) { ShowOrHideHUD(true); }
            }
        }

        /// <summary>
        /// Show or Hide the small Heading indicator on the HUD.
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideHeadingIndicator(bool isShown)
        {
            if (IsInitialised || IsEditorMode)
            {
                showHeadingIndicator = isShown;

                if (headingIndicatorImg != null)
                {
                    headingIndicatorImg.enabled = isShown;
                }
            }
        }

        #endregion

        #region Private or Internal Methods - Messages

        /// <summary>
        /// Cache the DisplayMessage RectTransform, Image and Text component.
        /// Set the Text colour.
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="hudRectTfrm"></param>
        private void InitialiseMessage(DisplayMessage displayMessage, RectTransform hudRectTfrm)
        {
            displayMessage.CachedMessagePanel = GetMessagePanel(displayMessage.guidHash, hudRectTfrm);

            displayMessage.scrollDirectionInt = (int)displayMessage.scrollDirection;
            displayMessage.scrollOffsetX = 0f;
            displayMessage.scrollOffsetY = 0f;

            if (displayMessage.CachedMessagePanel != null)
            {
                displayMessage.CachedBgImgComponent = displayMessage.CachedMessagePanel.GetComponent<Image>();

                if (displayMessage.CachedBgImgComponent != null)
                {
                    displayMessage.CachedBgImgComponent.enabled = displayMessage.showBackground;
                }

                // Cache the message Text component
                if (tempTextList == null) { tempTextList = new List<Text>(1); }
                displayMessage.CachedMessagePanel.GetComponentsInChildren(tempTextList);
                if (tempTextList.Count > 0)
                {
                    displayMessage.CachedTextComponent = tempTextList[0];
                }

                // SetDisplayMessageBackgroundColour does not update the baseBackgroundColour if the backgroundColour has not changed. So do it in here instead
                displayMessage.baseBackgroundColour.Set(displayMessage.backgroundColour.r, displayMessage.backgroundColour.g, displayMessage.backgroundColour.b, displayMessage.backgroundColour.a, true);
                SetDisplayMessageBackgroundBrightness(displayMessage);

                // SetDisplayMessageTextColour does not update the baseTextColour if the textColour has not changed. So do it in here instead
                displayMessage.baseTextColour.Set(displayMessage.textColour.r, displayMessage.textColour.g, displayMessage.textColour.b, displayMessage.textColour.a, true);
                SetDisplayMessageTextBrightness(displayMessage);

                SetDisplayMessageText(displayMessage, displayMessage.messageString);
                SetDisplayMessageTextAlignment(displayMessage, displayMessage.textAlignment);
                SetDisplayMessageTextFontSize(displayMessage, displayMessage.isBestFit, displayMessage.fontMinSize, displayMessage.fontMaxSize);
                SetDisplayMessageSize(displayMessage, displayMessage.displayWidth, displayMessage.displayHeight);

                if (displayMessage.scrollDirectionInt != DisplayMessage.ScrollDirectionNone)
                {
                    // scrollWidth/Height should be the same as those used in ScrollMessage(..)
                    // The left/right and top/bottom scroll limits can be ignored OR can consider the width/height of the message.
                    // Fullscreen scrolling = +/- half width of message + half the width of canvas
                    float scrollWidth = displayMessage.isScrollFullscreen ? displayMessage.displayWidth * cvsResolutionHalf.x + cvsResolutionHalf.x : displayMessage.displayWidth * cvsResolutionFull.x;

                    // Fullscreen scrolling = +/- half height of message + half the height of canvas
                    float scrollHeight = displayMessage.isScrollFullscreen ? displayMessage.displayHeight * cvsResolutionHalf.y + cvsResolutionHalf.y : displayMessage.displayHeight * cvsResolutionFull.y;

                    // Start at beginning of scrolling position (e.g. Offscreen left/right)
                    if (displayMessage.scrollDirectionInt == DisplayMessage.ScrollDirectionLR)
                    {
                        displayMessage.scrollOffsetX = -scrollWidth;
                    }
                    else if (displayMessage.scrollDirectionInt == DisplayMessage.ScrollDirectionRL)
                    {
                        displayMessage.scrollOffsetX = scrollWidth;
                    }
                    if (displayMessage.scrollDirectionInt == DisplayMessage.ScrollDirectionBT)
                    {
                        displayMessage.scrollOffsetY = -scrollHeight;
                    }
                    else if (displayMessage.scrollDirectionInt == DisplayMessage.ScrollDirectionTB)
                    {
                        displayMessage.scrollOffsetY = scrollHeight;
                    }
                }
                SetDisplayMessageOffset(displayMessage, displayMessage.offsetX, displayMessage.offsetY);
                
                // By default messages are initialised off. This is to prevent them suddenly appearing when first created.
                if (Application.isPlaying)
                {
                    displayMessage.CachedMessagePanel.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Initialise all the DisplayMessages by caching the RectTransforms.
        /// </summary>
        private void InitialiseMessages()
        {
            int _numDisplayMessages = GetNumberDisplayMessages;
            
            if (_numDisplayMessages > 0)
            {
                RectTransform hudRectTfrm = GetHUDPanel();
                if (hudRectTfrm != null)
                {
                    CheckHUDResize(false);
                    for (int dmIdx = 0; dmIdx < _numDisplayMessages; dmIdx++)
                    {
                        InitialiseMessage(displayMessageList[dmIdx], hudRectTfrm);
                    }

                    if (_numDisplayMessages > 0) { RefreshMessagesSortOrder(); }
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ShipDisplayModule.InitialiseMessages() - could not find HUD Panel"); }
                #endif
            }
        }

        /// <summary>
        /// Show or Hide the Display Message on the HUD. Turn on the HUD if required.
        /// When fading out a message, it isn't instantly turned off.
        /// Option to override the fade settings and instantly hide or show the message.
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="isShown"></param>
        /// <param name="isOverrideFade"></param>
        private void ShowOrHideMessage(DisplayMessage displayMessage, bool isShown, bool isOverrideFade = false)
        {
            if (IsInitialised)
            {
                if (displayMessage.CachedMessagePanel != null)
                {
                    if (isShown)
                    {
                        if (isOverrideFade)
                        {
                            displayMessage.fadeinTimer = 0f;
                        }
                        // Is a fade-out already in progress?
                        else if (displayMessage.fadeoutDuration > 0f && displayMessage.fadeoutTimer > 0f)
                        {
                            // (1 - Normalise amount faded out) * fade in duration.
                            displayMessage.fadeinTimer = (1f - (displayMessage.fadeoutTimer / displayMessage.fadeoutDuration)) * displayMessage.fadeinDuration;
                        }
                        else
                        {
                            displayMessage.fadeinTimer = displayMessage.fadeinDuration;

                            if (displayMessage.fadeinTimer > 0f)
                            {
                                // Instantly update to avoid flashing the faded in message before Update() is called.
                                SetDisplayMessageTextFade(displayMessage, false);
                                if (displayMessage.showBackground)
                                {
                                    SetDisplayMessageBackgroundFade(displayMessage, false);
                                }
                            }
                        }

                        // Should never be fading out when turning the message on
                        displayMessage.fadeoutTimer = 0f;
                    }
                    else if (isOverrideFade)
                    {
                        displayMessage.fadeoutTimer = 0f;
                    }
                    else if (displayMessage.fadeoutDuration > 0f)
                    {
                        // Is a fade-in already in progress?
                        if (displayMessage.fadeinDuration > 0f && displayMessage.fadeinTimer > 0f)
                        {
                            // (1 - Normalise amount faded in) * fade out duration.
                            displayMessage.fadeoutTimer = (1f - (displayMessage.fadeinTimer / displayMessage.fadeinDuration)) * displayMessage.fadeoutDuration;
                        }
                        else
                        {
                            displayMessage.fadeoutTimer = displayMessage.fadeoutDuration;
                        }

                        // Should never be fading in when turning the message off
                        displayMessage.fadeinTimer = 0f;

                        // Override instantly turning off the message
                        isShown = true;
                    }
                    else
                    {
                        displayMessage.showMessage = false;
                    }

                    displayMessage.showMessage = isShown;

                    displayMessage.CachedMessagePanel.gameObject.SetActive(isShown);
                }

                if (isShown && !IsHUDShown) { ShowOrHideHUD(true); }
            }
        }

        /// <summary>
        /// Show or hide all messages.
        /// Option to override fade settings and instantly show or hide.
        /// </summary>
        /// <param name="isShown"></param>
        /// <param name="isOverrideFade"></param>
        private void ShowOrHideMessages(bool isShown, bool isOverrideFade = false)
        {
            if (IsInitialised)
            {
                for (int dmIdx = 0; dmIdx < numDisplayMessages; dmIdx++)
                {
                    ShowOrHideMessage(displayMessageList[dmIdx], isShown, isOverrideFade);
                }
            }
        }

        /// <summary>
        /// Show or Hide all messages based on their current settings.
        /// This is called by 
        /// </summary>
        private void ShowOrHideMessages()
        {
            if (IsInitialised)
            {
                for (int dmIdx = 0; dmIdx < numDisplayMessages; dmIdx++)
                {
                    DisplayMessage displayMessage = displayMessageList[dmIdx];
                    // Shown messages should fade in if required.
                    // Hidden messages should be instantly hidden.
                    ShowOrHideMessage(displayMessage, displayMessage.showMessage, !displayMessage.showMessage);
                }
            }
        }

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Find the Message RectTransform under the HUD in the hierarchy.
        /// If hudRectTrfm is null, it will attempt to find it.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <param name="hudRectTrfm"></param>
        /// <returns></returns>
        private RectTransform GetMessagePanel(int guidHash, RectTransform hudRectTrfm)
        {
            if (hudRectTfrm == null) { hudRectTfrm = GetHUDPanel(); }
            if (hudRectTfrm != null)
            {
                return SSCUtils.GetChildRectTransform(hudRectTfrm.transform, "Message_" + guidHash, this.name);
            }
            else { return null; }
        }

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Create a new Message RectTransform under the HUD in the hierarchy
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="hudRectTfrm"></param>
        /// <returns></returns>
        public RectTransform CreateMessagePanel(DisplayMessage displayMessage, RectTransform hudRectTfrm)
        {
            RectTransform messagePanel = null;

            if (displayMessage != null)
            {
                float panelWidth = displayMessage.displayWidth;
                float panelHeight = displayMessage.displayHeight;

                //Canvas _canvas = GetCanvas;
                //Vector3 _canvasScale = _canvas == null ? Vector3.one : _canvas.transform.localScale;

                if (IsInitialised) { numDisplayMessages++; }
                messagePanel = SSCUtils.GetOrCreateChildRectTransform(hudRectTfrm, cvsResolutionFull, "Message_" + displayMessage.guidHash, 0f, 0f,
                                                                        panelWidth, panelHeight, 0.5f, 0.5f, 0.5f, 0.5f);

                if (messagePanel != null)
                {
                    Image imgComponent = messagePanel.gameObject.AddComponent<Image>();
                    if (imgComponent != null)
                    {
                        imgComponent.raycastTarget = false;
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: shipDisplayModule.CreateMessagePanel() - could not add background to Message panel"); }
                    #endif

                    RectTransform messageTxtPanel = SSCUtils.GetOrCreateChildRectTransform(messagePanel, cvsResolutionFull, "MessageText", 0f, 0f,
                                                                        panelWidth, panelHeight, 0.5f, 0.5f, 0.5f, 0.5f);

                    if (messageTxtPanel != null)
                    {
                        Text textComponent = messageTxtPanel.gameObject.AddComponent<Text>();
                        if (textComponent != null)
                        {
                            textComponent.raycastTarget = false;
                            textComponent.resizeTextForBestFit = true;
                            // Text is addd in InitialiseMessage().
                            textComponent.text = string.Empty;
                            if (Application.isPlaying)
                            {
                                textComponent.font = SSCUtils.GetDefaultFont();
                            }
                        }
                        #if UNITY_EDITOR
                        else { Debug.LogWarning("ERROR: shipDisplayModule.CreateMessagePanel() - could not add text to Message panel"); }
                        #endif
                    }

                    if (IsInitialised || editorMode) { InitialiseMessage(displayMessage, hudRectTfrm); }
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ERROR: shipDisplayModule.CreateMessagePanel() - could not create Message panel"); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: ShipDisplayModule.CreateMessagePanel() - displayMessage is null"); }
            #endif

            return messagePanel;
        }

        /// <summary>
        /// Show or hide the message background by enabling or disabling the image script
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="isShown"></param>
        private void ShowOrHideDisplayMessageBackground(DisplayMessage displayMessage, bool isShown)
        {
            if (IsInitialised || editorMode)
            {
                if (!editorMode) { displayMessage.showBackground = isShown; }
                displayMessage.CachedBgImgComponent.enabled = isShown;
            }
        }

        /// <summary>
        /// Scroll message across the display. Assumes module is initialised, message is not null,
        /// and scrollDirection is not None.
        /// </summary>
        /// <param name="displayMessage"></param>
        private void ScrollMessage(DisplayMessage displayMessage)
        {
            if (displayMessage.showMessage)
            {
                // Determine the distance to scroll
                float scrollFactor = displayMessage.scrollSpeed * Time.deltaTime * canvas.scaleFactor * 0.1f;
                float deltaX = scrollFactor * cvsResolutionFull.x;
                float deltaY = scrollFactor * cvsResolutionFull.y;

                // scrollWidth/Height should be the same as starting positions set in InitialiseMessage(..)
                // The left/right and top/bottom scroll limits can be ignored OR can consider the width/height of the message.
                // Fullscreen scrolling = +/- half width of message + half the width of canvas
                float scrollWidth = displayMessage.isScrollFullscreen ? displayMessage.displayWidth * cvsResolutionHalf.x + cvsResolutionHalf.x : displayMessage.displayWidth * cvsResolutionFull.x;

                // Fullscreen scrolling = +/- half height of message + half the height of canvas
                float scrollHeight = displayMessage.isScrollFullscreen ? displayMessage.displayHeight * cvsResolutionHalf.y + cvsResolutionHalf.y : displayMessage.displayHeight * cvsResolutionFull.y;

                // Calc the localPosition of the message
                tempMessageOffset.x = displayMessage.offsetX * cvsResolutionHalf.x;
                tempMessageOffset.y = displayMessage.offsetY * cvsResolutionHalf.y;

                if (displayMessage.scrollDirectionInt == DisplayMessage.ScrollDirectionLR)
                {
                    if (displayMessage.isScrollFullscreen) { tempMessageOffset.x = 0f; }
                    displayMessage.scrollOffsetX += deltaX;
                    displayMessage.scrollOffsetY = 0f;

                    if (displayMessage.scrollOffsetX > scrollWidth) { displayMessage.scrollOffsetX = -scrollWidth; }
                }
                else if (displayMessage.scrollDirectionInt == DisplayMessage.ScrollDirectionRL)
                {
                    if (displayMessage.isScrollFullscreen) { tempMessageOffset.x = 0f; }
                    displayMessage.scrollOffsetX -= deltaX;
                    displayMessage.scrollOffsetY = 0f;

                    if (displayMessage.scrollOffsetX < -scrollWidth) { displayMessage.scrollOffsetX = scrollWidth; }
                }
                else if (displayMessage.scrollDirectionInt == DisplayMessage.ScrollDirectionBT)
                {
                    if (displayMessage.isScrollFullscreen) { tempMessageOffset.y = 0f; }
                    displayMessage.scrollOffsetX = 0f;
                    displayMessage.scrollOffsetY += deltaY;

                    if (displayMessage.scrollOffsetY > scrollHeight) { displayMessage.scrollOffsetY = -scrollHeight; }
                }
                else // Top to Bottom
                {
                    if (displayMessage.isScrollFullscreen) { tempMessageOffset.y = 0f; }
                    displayMessage.scrollOffsetX = 0f;
                    displayMessage.scrollOffsetY -= deltaY;

                    if (displayMessage.scrollOffsetY < -scrollHeight) { displayMessage.scrollOffsetY = scrollHeight; }
                }

                Transform dmTrfm = displayMessage.CachedMessagePanel.transform;

                tempMessageOffset.x += displayMessage.scrollOffsetX;
                tempMessageOffset.y += displayMessage.scrollOffsetY;
                tempMessageOffset.z = dmTrfm.localPosition.z;
                dmTrfm.localPosition = tempMessageOffset;
            }
        }

        #endregion

        #region Private or Internal Methods - Targets

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Create a new Target RectTransform under the HUD in the hierarchy for the give slot
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <param name="slotIndex"></param>
        /// <returns></returns>
        public RectTransform CreateTargetPanel(DisplayTarget displayTarget, int slotIndex)
        {
            RectTransform targetPanel = null;

            if (targetsRectTfrm == null) { targetsRectTfrm = GetTargetsPanel(); }

            if (displayTarget != null && targetsRectTfrm != null)
            {
                if (slotIndex >= 0 && slotIndex < DisplayTarget.MAX_DISPLAYTARGET_SLOTS)
                {
                    // Convert into 0.0 to 1.0 value. The width/height values assume default 1920x1080.
                    float panelWidth = displayTarget.width / refResolution.x;
                    float panelHeight = displayTarget.height / refResolution.y;

                    //if (IsInitialised && slotIndex == 0) { numDisplayTargets++; }
                    targetPanel = SSCUtils.GetOrCreateChildRectTransform(targetsRectTfrm, cvsResolutionFull, "Target_" + displayTarget.guidHash + "_" + slotIndex.ToString(), 0f, 0f,
                                                                            panelWidth, panelHeight, 0.5f, 0.5f, 0.5f, 0.5f);

                    if (targetPanel != null)
                    {
                        Image imgComponent = targetPanel.gameObject.AddComponent<Image>();
                        if (imgComponent != null)
                        {
                            imgComponent.raycastTarget = false;
                        }
                        #if UNITY_EDITOR
                        else { Debug.LogWarning("ERROR: shipDisplayModule.CreateTargetPanel() - could not add image to Target panel"); }
                        #endif

                        if ((IsInitialised || editorMode) && !displayTarget.isInitialised) { InitialiseTarget(displayTarget); }
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: shipDisplayModule.CreateTargetPanel() - could not create Target panel"); }
                    #endif
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ERROR: shipDisplayModule.CreateTargetPanel() - could not create Target panel. Slot number is outside the range 0 to " + DisplayTarget.MAX_DISPLAYTARGET_SLOTS.ToString()); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: ShipDisplayModule.CreateTargetPanel() - displayTarget is null or could not find or create " + targetsPanelName); }
            #endif

            return targetPanel;
        }

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Find the Target RectTransform under the HUD in the hierarchy.
        /// If the parent targetsRectTfrm is null, it will attempt to find or create it.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <param name="slotIndex"></param>
        /// <returns></returns>
        private RectTransform GetTargetPanel(int guidHash, int slotIndex)
        {
            if (targetsRectTfrm == null) { targetsRectTfrm = GetTargetsPanel(); }
            if (targetsRectTfrm != null)
            {
                return SSCUtils.GetChildRectTransform(targetsRectTfrm.transform, "Target_" + guidHash + "_" + slotIndex, this.name);
            }
            else { return null; }
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used by the editor when moving DisplayTargets around in the list.
        /// </summary>
        /// <param name="displayTargetIndex"></param>
        public void RefreshDisplayTargetSlots()
        {
            numDisplayTargets = displayTargetList == null ? 0 : displayTargetList.Count;

            for (int dtIdx = 0; dtIdx < numDisplayTargets; dtIdx++)
            {
                DisplayTarget displayTarget = displayTargetList[dtIdx];

                // Initialise the Display Target slots
                displayTarget.displayTargetSlotList = new List<DisplayTargetSlot>(displayTarget.maxNumberOfTargets);

                for (int slotIdx = 0; slotIdx < displayTarget.maxNumberOfTargets; slotIdx++)
                {
                    DisplayTargetSlot displayTargetSlot = InitialiseTargetSlot(displayTarget, slotIdx);

                    displayTarget.displayTargetSlotList.Add(displayTargetSlot);
                }

                // InitialiseTargetSlot will turn all the reticles off, so update the DisplayTarget property too
                displayTarget.showTarget = false;
            }
        }

        /// <summary>
        /// Initialise the display target
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <param name="hudRectTfrm"></param>
        private void InitialiseTarget(DisplayTarget displayTarget)
        {
            if (displayTarget.maxNumberOfTargets < 0) { displayTarget.maxNumberOfTargets = 1; }
            else if (displayTarget.maxNumberOfTargets > DisplayTarget.MAX_DISPLAYTARGET_SLOTS) { displayTarget.maxNumberOfTargets = DisplayTarget.MAX_DISPLAYTARGET_SLOTS; }

            // Initialise the Display Target slots
            displayTarget.displayTargetSlotList = new List<DisplayTargetSlot>(displayTarget.maxNumberOfTargets);

            for (int slotIdx = 0; slotIdx < displayTarget.maxNumberOfTargets; slotIdx++)
            {
                DisplayTargetSlot displayTargetSlot = InitialiseTargetSlot(displayTarget, slotIdx);

                displayTarget.displayTargetSlotList.Add(displayTargetSlot);
            }

            SetDisplayTargetSize(displayTarget, displayTarget.width, displayTarget.height);

            // SetDisplayTargetReticleColour does not update the baseReticleColour if the reticleColour has not changed. So do it in here instead
            displayTarget.baseReticleColour.Set(displayTarget.reticleColour.r, displayTarget.reticleColour.g, displayTarget.reticleColour.b, displayTarget.reticleColour.a, true);
            SetDisplayTargetBrightness(displayTarget);

            displayTarget.isInitialised = true;
        }

        /// <summary>
        /// Initialise a DisplayTarget Slot. This includes:
        /// 1. caching the panel and image component
        /// 2. Loading the reticle sprite
        /// 3. Setting the offset to 0,0
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <param name="slotIdx"></param>
        /// <returns></returns>
        private DisplayTargetSlot InitialiseTargetSlot(DisplayTarget displayTarget, int slotIdx)
        {
            DisplayTargetSlot displayTargetSlot = new DisplayTargetSlot();

            displayTargetSlot.CachedTargetPanel = GetTargetPanel(displayTarget.guidHash, slotIdx);

            if (displayTargetSlot.CachedTargetPanel != null)
            {
                displayTargetSlot.CachedImgComponent = displayTargetSlot.CachedTargetPanel.GetComponent<Image>();

                // Add the Reticle sprite to the image for this Display Target
                if (displayTargetSlot.CachedImgComponent != null)
                {
                    // TODO - consider getting the sprite only once per DisplayTarget
                    displayTargetSlot.CachedImgComponent.sprite = GetDisplayReticleSprite(displayTarget.guidHashDisplayReticle);
                }

                SetDisplayTargetOffset(displayTargetSlot, 0f, 0f);

                // By default targets are initialised off. This is to prevent them suddenly appearing when first created.
                if (Application.isPlaying)
                {
                    displayTargetSlot.showTargetSlot = false;
                    displayTargetSlot.CachedTargetPanel.gameObject.SetActive(false);
                }
            }

            return displayTargetSlot;
        }

        /// <summary>
        /// Initialise all the DisplayTargets by caching the RectTransforms.
        /// </summary>
        private void InitialiseTargets()
        {
            int _numDisplayTargets = GetNumberDisplayTargets;
            
            if (_numDisplayTargets > 0)
            {
                if (targetsRectTfrm == null) { targetsRectTfrm = GetTargetsPanel(); }
                if (targetsRectTfrm != null)
                {
                    CheckHUDResize(false);

                    for (int dmIdx = 0; dmIdx < _numDisplayTargets; dmIdx++)
                    {
                        InitialiseTarget(displayTargetList[dmIdx]);
                    }

                    if (_numDisplayTargets > 0) { RefreshTargetsSortOrder(); }
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ShipDisplayModule.InitialiseTargets() - could not find " + targetsPanelName); }
                #endif
            }
        }

        /// <summary>
        /// Show or Hide DisplayTarget slot on the HUD.
        /// By design, if the HUD is not shown, the Target slot will not be show.
        /// </summary>
        /// <param name="displayTargetSlot"></param>
        /// <param name="isShown"></param>
        private void ShowOrHideTargetSlot(DisplayTargetSlot displayTargetSlot, bool isShown)
        {
            if ((IsInitialised || editorMode) && displayTargetSlot != null && displayTargetSlot.CachedTargetPanel != null)
            {
                displayTargetSlot.showTargetSlot = isShown;
                displayTargetSlot.CachedTargetPanel.gameObject.SetActive(isShown);
            }
        }

        /// <summary>
        /// Show or Hide all slots of a Display Target on the HUD.
        /// By design, if the HUD is not shown, the Targets will not be show.
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <param name="isShown"></param>
        private void ShowOrHideTarget(DisplayTarget displayTarget, bool isShown)
        {
            if (IsInitialised || editorMode)
            {
                displayTarget.showTarget = isShown;

                for (int sIdx = 0; sIdx < displayTarget.maxNumberOfTargets; sIdx++)
                {
                    ShowOrHideTargetSlot(displayTarget.displayTargetSlotList[sIdx], isShown);
                }
            }
        }

        /// <summary>
        /// Show or hide all targets
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideTargets(bool isShown)
        {
            if (IsInitialised)
            {
                for (int dmIdx = 0; dmIdx < numDisplayTargets; dmIdx++)
                {
                    ShowOrHideTarget(displayTargetList[dmIdx], isShown);
                }
            }
        }

        /// <summary>
        /// Show or Hide all targets based on their current settings
        /// </summary>
        private void ShowOrHideTargets()
        {
            if (IsInitialised)
            {
                for (int dmIdx = 0; dmIdx < numDisplayTargets; dmIdx++)
                {
                    DisplayTarget displayTarget = displayTargetList[dmIdx];
                    ShowOrHideTarget(displayTarget, displayTarget.showTarget);
                }
            }
        }

        /// <summary>
        /// Update the position on the HUD for each of the Display Target slots
        /// that are shown.
        /// </summary>
        private void UpdateTargetPositions()
        {
            if (IsInitialised)
            {
                if (sscRadar == null) { sscRadar = SSCRadar.GetOrCreateRadar(); }

                if (sscRadar != null)
                {
                    // Loop through the display targets
                    for (int dtgtIdx = 0; dtgtIdx < numDisplayTargets; dtgtIdx++)
                    {
                        DisplayTarget displayTarget = displayTargetList[dtgtIdx];

                        if (displayTarget.showTarget)
                        {
                            // Loop through the display target slots
                            for (int slotIdx = 0; slotIdx < displayTarget.maxNumberOfTargets; slotIdx++)
                            {
                                DisplayTargetSlot displayTargetSlot = displayTarget.displayTargetSlotList[slotIdx];

                                if (displayTargetSlot.radarItemKey.radarItemIndex >= 0 && displayTargetSlot.showTargetSlot)
                                {
                                    SSCRadarItem radarItem = sscRadar.GetRadarItem(displayTargetSlot.radarItemKey);

                                    if (radarItem != null)
                                    {
                                        SetDisplayTargetPosition(displayTarget, slotIdx, radarItem.position);
                                    }
                                    //#if UNITY_EDITOR
                                    //else
                                    //{
                                    //    // This could be a timing issue
                                    //    Debug.LogWarning("ShipDisplayModule.UpdateTargetPositions - could not find radar item with index " + displayTargetSlot.radarItemKey.radarItemIndex + " for DisplayTarget " + (dtgtIdx+1).ToString("000") + " slot " + slotIdx);
                                    //}
                                    //#endif
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Initialise the ShipDisplayModule. Either set initialiseOnStart to false and call this
        /// method in your code, or set initialiseOnStart to true in the inspector and don't call
        /// this method.
        /// </summary>
        public void Initialise()
        {
            // calling this method twice may incorrectly set the positions of the Altitude and speed panels.
            if (IsInitialised) { return; }

            canvas = GetComponent<Canvas>();
            canvasScaler = GetComponent<CanvasScaler>();

            hudRectTfrm = SSCUtils.GetChildRectTransform(transform, hudPanelName, this.name);

            if (canvas != null && hudRectTfrm != null)
            {
                canvas.sortingOrder = canvasSortOrder;

                // Get the reference resolution from the canvas scaler. If it hasn't been changed this should
                // always be 1920x1080 for SSC.
                //if (canvasScaler != null) { refResolutionFull = canvasScaler.referenceResolution; }
                //else { refResolutionFull.x = 1920f; refResolutionFull.y = 1080f; }

                // use the scaled size rather than the original reference resolution size.
                CheckHUDResize(false);

                GetHUDPanels();

                if (reticlePanel != null && overlayPanel != null && altitudeTextRectTfrm != null && airspeedTextRectTfrm != null)
                {
                    GetImgComponents();
                    GetTextComponents();

                    // Store the starting position of the Altitude text
                    altitudeInitPosition = altitudeTextRectTfrm.localPosition;
                    altitudeCurrentPosition = altitudeInitPosition;   
                    // Initialise the string used to store the Altitude value and reduce GC.
                    altitudeString = new SCSMString(6);
                    altitudeString.SetMaxIntLength(6);

                    // Store the starting position of the Air speed text
                    airspeedInitPosition = airspeedTextRectTfrm.localPosition;
                    airspeedCurrentPosition = airspeedInitPosition;
                    // Initialise the string used to store the Airspeed value and reduce GC.
                    airspeedString = new SCSMString(6);
                    airspeedString.SetMaxIntLength(6);

                    // Create an empty array of integers to use when processing
                    tempIncludeFactionList = new List<int>(20);
                    tempIncludeSquadronList = new List<int>(20);

                    InitialiseAttitude(true);
                    InitialiseHeading(true);

                    ReinitialiseVariables();

                    hudRandom = new SSCRandom();
                    // Set the seed to an arbitary prime number (but it could be anything really)
                    if (hudRandom != null) { hudRandom.SetSeed(691); }
                    flickerHistory = new Queue<float>(5);

                    #if UNITY_EDITOR
                    if (!isMainCameraAssigned) { Debug.LogWarning("ShipDisplayModule could not find a camera with the MainCamera tag set"); }
                    #endif

                    IsInitialised = displayReticleImg != null && isMainCameraAssigned;

                    RefreshHUD();

                    if (isShowHUDWithFlicker && isShowOnInitialise) { FlickerOn(flickerDefaultDuration); }
                    else  { ShowOrHideHUD(isShowOnInitialise); }

                    // Should we fade in or out the display when it first starts?
                    if (fadeInOnStart) { StartDisplayFade(true); }
                    else if (fadeOutOnStart) { StartDisplayFade(false); }
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: ShipDisplayModule.Initialise() could not find canvas HUDPanel. Did you start with the sample HUD prefab from Prefabs/Visuals folder?"); }
            #endif
        }

        /// <summary>
        /// Check to see if the ship is the same as the sourceShip currently assigned to the HUD.
        /// If either are null, the method will return false.
        /// </summary>
        /// <param name="shipControlModule"></param>
        /// <returns></returns>
        public bool IsSourceShip(ShipControlModule shipControlModule)
        {
            if (sourceShip == null || shipControlModule == null) { return false; }
            else { return shipControlModule.GetShipId == sourceShip.GetShipId; }
        }

        /// <summary>
        /// Check to see if the ship is the same as the soureShip currently assigned to the HUD.
        /// If either are null, the method will return false.
        /// </summary>
        /// <param name="ship"></param>
        /// <returns></returns>
        public bool IsSourceShip(Ship ship)
        {
            if (sourceShip == null || ship == null) { return false; }
            else { return ship.shipId == sourceShip.GetShipId; }
        }

        /// <summary>
        /// Call this if you modify any of the following at runtime.
        /// 1) displayReticleList
        /// 2) The DisplayReticlePanel size
        /// 3) Add or remove the MainCamera tag from a camera
        /// 4) displayMessageList
        /// 5) displayTargetList
        /// </summary>
        public void ReinitialiseVariables()
        {
            ValidateReticleList();
            numDisplayReticles = displayReticleList == null ? 0 : displayReticleList.Count;

            if (reticlePanel != null)
            {
                RectTransform reticleRectTransform = reticlePanel.GetComponent<RectTransform>();
                if (reticleRectTransform != null)
                {
                    reticleWidth = reticleRectTransform.sizeDelta.x;
                    reticleHeight = reticleRectTransform.sizeDelta.y;
                }
            }

            // If the camera hasn't been assigned, attempt to use the first camera with the MainCamera tag.
            if (mainCamera == null && Camera.main != null) { mainCamera = Camera.main; }

            isMainCameraAssigned = mainCamera != null;

            ValidateGaugeList();
            numDisplayGauges = displayGaugeList == null ? 0 : displayGaugeList.Count;

            ValidateMessageList();
            numDisplayMessages = displayMessageList == null ? 0 : displayMessageList.Count;

            ValidateTargetList();
            numDisplayTargets = displayTargetList == null ? 0 : displayTargetList.Count;
        }

        /// <summary>
        /// Refresh all components of the HUD. Typically called after the screen has been resized.
        /// </summary>
        public void RefreshHUD()
        {
            if (IsInitialised || editorMode)
            {
                // Set the initial position
                SetDisplayReticleOffset(displayReticleOffsetX, displayReticleOffsetY);

                SetHUDSize(displayWidth, displayHeight);
                SetHUDOffset(displayOffsetX, displayOffsetY);

                // Show or Hide initial setup
                ShowOrHideReticle(showActiveDisplayReticle);
                ShowOrHideAltitude(showAltitude);
                SetPrimaryColour(primaryColour);
                SetDisplayReticleColour(activeDisplayReticleColour);
                SetAltitudeTextColour(altitudeTextColour);
                ShowOrHideAirSpeed(showAirspeed);
                SetAirSpeedTextColour(airspeedTextColour);
                ShowOrHideOverlay(showOverlay);
                InitialiseAttitude(false);
                ShowOrHideAttitude(showAttitude);
                InitialiseHeading(false);
                ShowOrHideHeading(showHeading);
                InitialiseMessages();
                ShowOrHideMessages();
                InitialiseTargets();
                ShowOrHideTargets();
                InitialiseGauges();
                ShowOrHideGauges();

                // Set brightness after the colours have been set.
                InitialiseBrightness();
            }
        }

        /// <summary>
        /// Show the heads-up display. Has no effect if IsInitialised is false.
        /// </summary>
        public void ShowHUD()
        {
            // Prob makes sense to start in a non-flickering state.
            StopFlickering();

            if (isShowHUDWithFlicker) { FlickerOn(flickerDefaultDuration); }
            else { ShowOrHideHUD(true); }
        }

        /// <summary>
        /// Hide the heads-up display. Has no effect if IsInitialised is false.
        /// </summary>
        public void HideHUD()
        {
            // Prob makes sense to start in a non-flickering state.
            StopFlickering();

            if (isHideHUDWithFlicker) { FlickerOff(flickerDefaultDuration); }
            else { ShowOrHideHUD(false); }
        }

        /// <summary>
        /// Show the Overlay image on the heads-up display. Has no effect if IsInitialised is false.
        /// </summary>
        public void ShowOverlay()
        {
            ShowOrHideOverlay(true);
        }

        /// <summary>
        /// Hide the Overlay image on the heads-up display. Has no effect if IsInitialised is false.
        /// </summary>
        public void HideOverlay()
        {
            ShowOrHideOverlay(false);
        }

        /// <summary>
        /// Set or assign the main camera used by the heads-up display for calculations.
        /// </summary>
        /// <param name="camera"></param>
        public void SetCamera(Camera camera)
        {
            mainCamera = camera;
            isMainCameraAssigned = camera != null;
        }

        /// <summary>
        /// Set the HUD to use a particular display. Displays or monitors are numbered from 1 to 8.
        /// </summary>
        /// <param name="displayNumber">1 to 8</param>
        public void SetCanvasTargetDisplay (int displayNumber)
        {
            if (IsInitialised && SSCUtils.VerifyTargetDisplay(displayNumber, true))
            {
                Canvas _canvas = GetCanvas;

                if (_canvas != null) { _canvas.targetDisplay = displayNumber - 1; }
            }
        }

        /// <summary>
        /// Set the offset (position) of the HUD.
        /// If the module has been initialised, this will also re-position the HUD.
        /// </summary>
        /// <param name="offsetX">Horizontal offset from centre. Range between -1 and 1</param>
        /// <param name="offsetY">Vertical offset from centre. Range between -1 and 1</param>
        public void SetHUDOffset(float offsetX, float offsetY)
        {
            // Clamp the x,y values -1 to 1.
            if (offsetX < -1f) { displayOffsetX = -1f; }
            else if (offsetX > 1f) { displayOffsetX = 1f; }
            else { if (offsetX != displayOffsetX) { displayOffsetX = offsetX; } }

            if (offsetY < -1f) { displayOffsetY = -1f; }
            else if (offsetY > 1f) { displayOffsetY = 1f; }
            else { if (offsetY != displayOffsetY) { displayOffsetY = offsetY; } }

            if (IsInitialised)
            {
                // Here we use the half original (reference) HUD size as it doesn't need to be scaled in any way.
                // The parent HUD canvas is correctly scaled and sized to the actual monitor or device display.
                overlayPanel.localPosition = new Vector3(offsetX * cvsResolutionFull.x, offsetY * cvsResolutionFull.y, overlayPanel.localPosition.z);
            }
        }

        /// <summary>
        /// Set the size of the HUD overlay image and text.
        /// If the module has been initialised, this will also resize the HUD.
        /// The values are only updated if they are outside the range 0.0 to 1.0 or have changed.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetHUDSize(float width, float height)
        {
            // Clamp the x,y values 0.0 to 1.0
            if (width < 0f) { displayWidth = 0f; }
            else if (width > 1f) { displayWidth = 1f; }
            else if (width != displayWidth) { displayWidth = width; }

            if (height < 0f) { displayHeight = 0f; }
            else if (height > 1f) { displayHeight = 1f; }
            else if (height != displayHeight) { displayHeight = height; }

            if (IsInitialised)
            {
                // Here we use the original (reference) HUD size as it doesn't need to be scaled in any way.
                // The parent HUD canvas is correctly scaled and sized to the actual monitor or device display.
                overlayPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, displayWidth * cvsResolutionFull.x);
                overlayPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, displayHeight * cvsResolutionFull.y);

                // altitudeInitPosition is in 1920x1080 sizing. Convert into canvas space with cvsResolutionFull.x / refResolution.x

                // Move the altitude text relative to the resized overlay image
                altitudeCurrentPosition.x = altitudeInitPosition.x * displayWidth * cvsResolutionFull.x / refResolution.x;
                altitudeCurrentPosition.y = altitudeInitPosition.y * displayHeight * cvsResolutionFull.y / refResolution.y;
                altitudeTextRectTfrm.localPosition = altitudeCurrentPosition;

                // Move the airspeed text relative to the resized overlay image
                airspeedCurrentPosition.x = airspeedInitPosition.x * displayWidth * cvsResolutionFull.x / refResolution.x;
                airspeedCurrentPosition.y = airspeedInitPosition.y * displayHeight * cvsResolutionFull.y / refResolution.y;
                airspeedTextRectTfrm.localPosition = airspeedCurrentPosition;

                if (callbackOnSizeChange != null) { callbackOnSizeChange(displayWidth, displayHeight); }
            }
        }

        /// <summary>
        /// Set the primary colour of the heads-up display. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the HUD with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetPrimaryColour(Color32 newColour)
        {
            SSCUtils.UpdateColour(ref newColour, ref primaryColour, ref baseOverlayColour, true);

            if (IsInitialised || editorMode)
            {
                if (primaryOverlayImg != null)
                {
                    SetOverlayBrightness();
                }
            }
        }

        /// <summary>
        /// Set the primary colour of the heads-up display. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the HUD with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetPrimaryColour(Color newColour)
        {
            SSCUtils.UpdateColour(ref newColour, ref primaryColour, ref baseOverlayColour, true);

            if (IsInitialised || editorMode)
            {
                if (primaryOverlayImg != null)
                {
                    if (brightness == 1f) { primaryOverlayImg.color = newColour; }
                    else { primaryOverlayImg.color = baseOverlayColour.GetColorWithBrightness(brightness); }
                }
            }
        }

        /// <summary>
        /// Set the overall brightness of the HUD.
        /// </summary>
        /// <param name="newBrightness">Range 0.0 to 1.0</param>
        public void SetBrightness(float newBrightness)
        {
            // Clamp 0.0 to 1.0
            if (newBrightness < 0f) { brightness = 0f; }
            else if (newBrightness > 1f) { brightness = 1f; }

            brightness = newBrightness;

            if (IsInitialised || editorMode)
            {
                SetOverlayBrightness();
                SetReticleBrightness();
                SetAltitudeTextBrightness();
                SetAirSpeedTextBrightness();
                SetAttitudeBrightness();
                SetHeadingBrightness();

                int _numDisplayMessages = GetNumberDisplayMessages;

                // Set the brightness of all the messages
                for (int dmIdx = 0; dmIdx < _numDisplayMessages; dmIdx++)
                {
                    SetDisplayMessageBackgroundBrightness(displayMessageList[dmIdx]);
                    SetDisplayMessageTextBrightness(displayMessageList[dmIdx]);
                }

                int _numDisplayTargets = GetNumberDisplayTargets;

                // Set the brightness for all the targets
                for (int dtgtIdx = 0; dtgtIdx < _numDisplayTargets; dtgtIdx++)
                {
                    SetDisplayTargetBrightness(displayTargetList[dtgtIdx]);
                }

                int _numDisplayGauges = GetNumberDisplayGauges;

                // Set the brightness for all the gauges
                for (int dgIdx = 0; dgIdx < _numDisplayGauges; dgIdx++)
                {
                    SetDisplayGaugeForegroundBrightness(displayGaugeList[dgIdx]);
                    SetDisplayGaugeBackgroundBrightness(displayGaugeList[dgIdx]);
                    SetDisplayGaugeTextBrightness(displayGaugeList[dgIdx]);
                }
            }

            if (callbackOnBrightnessChange != null) { callbackOnBrightnessChange(brightness); }
        }

        /// <summary>
        /// Set the sort order in the scene of the HUD. Higher values appear on top.
        /// </summary>
        /// <param name="newSortOrder"></param>
        public void SetCanvasSortOrder(int newSortOrder)
        {
            canvasSortOrder = newSortOrder;

            if (IsInitialised || editorMode)
            {
                Canvas _canvas = GetCanvas;
                if (_canvas != null) { _canvas.sortingOrder = newSortOrder; }
            }
        }

        /// <summary>
        /// Turn on or off the HUD. Has no effect if not initialised. See also ShowHUD() and HideHUD()
        /// </summary>
        public void ToggleHUD()
        {
            // Prob makes sense to start in a non-flickering state.
            StopFlickering();

            IsHUDShown = !IsHUDShown;

            if (isShowHUDWithFlicker && IsHUDShown) { FlickerOn(flickerDefaultDuration); }
            else if (isHideHUDWithFlicker && !IsHUDShown) { FlickerOff(flickerDefaultDuration); }
            else { ShowOrHideHUD(IsHUDShown); }
        }

        #endregion

        #region Public API Methods - Panels

        /// <summary>
        /// Get the Attitude Panel. Create it if it does not already exist
        /// </summary>
        /// <returns></returns>
        public RectTransform GetAttitudePanel()
        {
            if (hudRectTfrm == null) { hudRectTfrm = GetHUDPanel(); }

            if (hudRectTfrm != null)
            {
                //if (cvsResolutionFull.x == 0f) { cvsResolutionFull = ReferenceResolution; }

                // Centred panel (like messages)
                return SSCUtils.GetOrCreateChildRectTransform(hudRectTfrm, cvsResolutionFull, attitudePanelName, 0f, 0f, 1f, 1f, 0.5f, 0.5f, 0.5f, 0.5f);
            }
            else { return null; }
        }

        /// <summary>
        /// Get the Fade Panel. Create it if it does not already exist.
        /// </summary>
        /// <returns></returns>
        public RectTransform GetFadePanel()
        {
            if (hudRectTfrm == null) { hudRectTfrm = GetHUDPanel(); }

            if (hudRectTfrm != null)
            {
                // Stretched panel
                return SSCUtils.GetOrCreateChildRectTransform(hudRectTfrm, cvsResolutionFull, fadePanelName, 0f, 0f, 1f, 1f, 0f, 0f, 1f, 1f);
            }
            else { return null; }
        }

        /// <summary>
        /// Get the HUD RectTransform or panel
        /// </summary>
        /// <returns></returns>
        public RectTransform GetHUDPanel()
        {
            return SSCUtils.GetChildRectTransform(transform, hudPanelName, this.name);
        }

        /// <summary>
        /// Get the Heading Panel
        /// Create it if it does not already exist
        /// </summary>
        /// <returns></returns>
        public RectTransform GetHeadingPanel()
        {
            if (hudRectTfrm == null) { hudRectTfrm = GetHUDPanel(); }

            if (hudRectTfrm != null)
            {
                // Stretched panel
                //return SSCUtils.GetOrCreateChildRectTransform(hudRectTfrm, cvsResolutionFull, headingPanelName, 0f, 0f, 1f, 1f, 0f, 0f, 1f, 1f);
                // Centred panel (like messages)
                return SSCUtils.GetOrCreateChildRectTransform(hudRectTfrm, cvsResolutionFull, headingPanelName, 0f, 0f, 1f, 1f, 0.5f, 0.5f, 0.5f, 0.5f);
            }
            else { return null; }
        }

        /// <summary>
        /// Get the parent panel for the Display Targets.
        /// Create it if it does not already exist
        /// </summary>
        /// <returns></returns>
        public RectTransform GetTargetsPanel()
        {
            if (hudRectTfrm == null) { hudRectTfrm = GetHUDPanel(); }

            if (hudRectTfrm != null)
            {
                // Stretched panel
                return SSCUtils.GetOrCreateChildRectTransform(hudRectTfrm, cvsResolutionFull, targetsPanelName, 0f, 0f, 1f, 1f, 0f, 0f, 1f, 1f);
            }
            else { return null; }
        }

        /// <summary>
        /// Get the parent panel for the Display Gauges.
        /// Create it if it does not already exist
        /// </summary>
        /// <returns></returns>
        public RectTransform GetGaugesPanel()
        {
            if (hudRectTfrm == null) { hudRectTfrm = GetHUDPanel(); }

            if (hudRectTfrm != null)
            {
                // Stretched panel
                return SSCUtils.GetOrCreateChildRectTransform(hudRectTfrm, cvsResolutionFull, gaugesPanelName, 0f, 0f, 1f, 1f, 0f, 0f, 1f, 1f);
            }
            else { return null; }
        }

        #endregion

        #region Public API Methods - Cursor

        /// <summary>
        /// Show the hardware (mouse) cursor.
        /// This also restarts the countdown auto-hide
        /// timer if that is enabled.
        /// </summary>
        public void ShowCursor()
        {
            ShowOrHideCursor(true);
        }

        /// <summary>
        /// Hide the hardware (mouse) cursor.
        /// NOTE: This will sometimes fail to turn off the cursor in the editor
        /// Game View when it doesn't have focus, but will work fine in a build.
        /// </summary>
        public void HideCursor()
        {
            ShowOrHideCursor(false);
        }

        /// <summary>
        /// Centre the hardware (mouse) cursor in the centre of the screen.
        /// WARNING: This will wait until the next frame before it returns.
        /// </summary>
        public void CentreCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            StartCoroutine(UnlockCursor());
        }

        /// <summary>
        /// Toggle the hardware (mouse) cursor on or off.
        /// NOTE: This will sometimes fail to turn off the cursor in the editor
        /// Game View when it doesn't have focus, but will work fine in a build.
        /// </summary>
        public void ToggleCursor()
        {
            ShowOrHideCursor(!Cursor.visible);
        }

        #endregion

        #region Public API Methods - Display Reticle

        /// <summary>
        /// Returns the guidHash of the Reticle in the list given the index or
        /// zero-based position in the list. Will return 0 if no matching Reticle
        /// is found.
        /// Will return 0 if the module hasn't been initialised.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetDisplayReticleGuidHash(int index)
        {
            int guidHash = 0;

            if (IsInitialised || editorMode)
            {
                if (index >= 0 && index < GetNumberDisplayReticles)
                {
                    if (displayReticleList[index] != null) { guidHash = displayReticleList[index].guidHash; }
                }
            }

            return guidHash;
        }

        /// <summary>
        /// Returns the guidHash of the Reticle in the list given the name of the sprite.
        /// Will return 0 if no matching Reticle is found.
        /// WARNING: This will increase GC. Use GetDisplayReticleGuidHash(int index) where possible.
        /// </summary>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public int GetDisplayReticleGuidHash(string spriteName)
        {
            int guidHash = 0;
            DisplayReticle _tempDR = null;

            if ((IsInitialised || editorMode) && !string.IsNullOrEmpty(spriteName))
            {
                int _numDisplayReticles = GetNumberDisplayReticles;
                string _spriteNameLowerCase = spriteName.ToLower();

                for (int drIdx = 0; drIdx < _numDisplayReticles; drIdx++)
                {
                    _tempDR = displayReticleList[drIdx];
                    if (_tempDR != null && _tempDR.primarySprite != null && _tempDR.primarySprite.name.ToLower() == _spriteNameLowerCase)
                    {
                        guidHash = _tempDR.guidHash;
                        break;
                    }
                }
            }

            return guidHash;
        }

        /// <summary>
        /// Get a DisplayReticle given its guidHash.
        /// See also GetDisplayReticleGuidHash(..).
        /// Will return null if guidHash parameter is 0, it cannot be found
        /// or the module has not been initialised.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public DisplayReticle GetDisplayReticle(int guidHash)
        {
            DisplayReticle displayReticle = null;
            DisplayReticle _tempDR = null;

            if (IsInitialised || editorMode)
            {
                int _numDisplayReticles = GetNumberDisplayReticles;

                for (int drIdx = 0; drIdx < _numDisplayReticles; drIdx++)
                {
                    _tempDR = displayReticleList[drIdx];
                    if (_tempDR != null && _tempDR.guidHash == guidHash)
                    {
                        displayReticle = _tempDR;
                        break;
                    }
                }
            }

            return displayReticle;
        }

        /// <summary>
        /// Get the UI sprite (image) for a DisplayReticle.
        /// See also GetDisplayReticleGuidHash(..).
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public Sprite GetDisplayReticleSprite(int guidHash)
        {
            Sprite sprite = null;
            DisplayReticle _tempDR = null;

            if (IsInitialised || editorMode)
            {
                int _numDisplayReticles = GetNumberDisplayReticles;

                for (int drIdx = 0; drIdx < _numDisplayReticles; drIdx++)
                {
                    _tempDR = displayReticleList[drIdx];
                    if (_tempDR != null && _tempDR.guidHash == guidHash)
                    {
                        sprite = _tempDR.primarySprite;
                        break;
                    }
                }
            }

            return sprite;
        }

        /// <summary>
        /// Change the DiplayReticle sprite on the UI panel.
        /// See also GetDisplayReticleGuidHash(..).
        /// NOTE: The module must be initialised.
        /// </summary>
        /// <param name="guidHash"></param>
        public void ChangeDisplayReticle(int guidHash)
        {
            if (IsInitialised || editorMode)
            {
                guidHashActiveDisplayReticle = guidHash;
                currentDisplayReticle = GetDisplayReticle(guidHash);

                LoadDisplayReticleSprite(currentDisplayReticle);
            }
        }

        /// <summary>
        /// Hide or turn off the Display Reticle
        /// </summary>
        public void HideDisplayReticle()
        {
            ShowOrHideReticle(false);
        }

        /// <summary>
        /// Set the offset (position) of the Display Reticle on the HUD.
        /// If the module has been initialised, this will also re-position the Display Reticle.
        /// Same as SetDisplayReticleOffset(offset.x, offset.y)
        /// </summary>
        /// <param name="offset">Horizontal and Vertical offset from centre. Values should be between -1 and 1</param>
        public void SetDisplayReticleOffset(Vector2 offset)
        {
            SetDisplayReticleOffset(offset.x, offset.y);
        }

        /// <summary>
        /// Set the offset (position) of the Display Reticle on the HUD.
        /// If the module has been initialised, this will also re-position the Display Reticle.
        /// </summary>
        /// <param name="offsetX">Horizontal offset from centre. Range between -1 and 1</param>
        /// <param name="offsetY">Vertical offset from centre. Range between -1 and 1</param>
        public void SetDisplayReticleOffset(float offsetX, float offsetY)
        {
            // Verify the x,y values are within range -1 to 1.
            if (offsetX >= -1f && offsetX <= 1f)
            {
                displayReticleOffsetX = offsetX;
            }

            if (offsetY >= -1f && offsetY <= 1f)
            {
                displayReticleOffsetY = offsetY;
            }

            if (IsInitialised || editorMode)
            {
                Vector2 halfHUDSize = GetHUDHalfSize(true);

                if (lockDisplayReticleToCursor)
                {
                    // Centre on mouse pointer more accurately (reticle can go half outside screen dimensions on edges)
                    displayReticleImg.transform.localPosition = new Vector3(offsetX * halfHUDSize.x, offsetY * halfHUDSize.y, displayReticleImg.transform.localPosition.z);
                }
                else
                {
                    // Original default behaviour - reticle remains within screen dimensions.
                    displayReticleImg.transform.localPosition = new Vector3(offsetX * (halfHUDSize.x - (reticleWidth / 2f)), offsetY * (halfHUDSize.y - (reticleHeight / 2f)), displayReticleImg.transform.localPosition.z);
                }
            }
        }

        /// <summary>
        /// Set the active Display Reticle colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the reticle with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayReticleColour(Color32 newColour)
        {
            SSCUtils.UpdateColour(ref newColour, ref activeDisplayReticleColour, ref baseReticleColour, true);

            if (IsInitialised)
            {
                if (displayReticleImg != null)
                {
                    SetReticleBrightness();
                }
            }
        }

        /// <summary>
        /// Set the active Display Reticle colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the reticle with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayReticleColour(Color newColour)
        {
            SSCUtils.UpdateColour(ref newColour, ref activeDisplayReticleColour, ref baseReticleColour, true);

            if (IsInitialised || editorMode)
            {
                if (displayReticleImg != null)
                {
                    if (brightness == 1f) { displayReticleImg.color = newColour; }
                    else { displayReticleImg.color = baseReticleColour.GetColorWithBrightness(brightness); }
                }
            }
        }

        /// <summary>
        /// Show the Display Reticle on the HUD. The HUD will automatically be shown if it is not already visible.
        /// </summary>
        public void ShowDisplayReticle()
        {
            ShowOrHideReticle(true);
        }

        /// <summary>
        /// Show or Hide the Display Reticle on the HUD. The HUD will automatically be shown if required.
        /// </summary>
        public void ToggleDisplayReticle()
        {
            ShowOrHideReticle(!showActiveDisplayReticle);
        }

        /// <summary>
        /// Create a new list if required
        /// </summary>
        public void ValidateReticleList()
        {
            if (displayReticleList == null) { displayReticleList = new List<DisplayReticle>(1); }
        }

        #endregion

        #region Public API Methods - Altitude

        /// <summary>
        /// Attempt to show the Altitude indicator. Turn on HUD if required.
        /// </summary>
        public void ShowAltitude()
        {
            ShowOrHideAltitude(true);
        }

        /// <summary>
        /// Attempt to turn off the Altitude indicator
        /// </summary>
        public void HideAltitude()
        {
            ShowOrHideAltitude(false);
        }

        /// <summary>
        /// Set the altitude text colour on the heads-up display. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the HUD with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetAltitudeTextColour(Color32 newColour)
        {
            SSCUtils.UpdateColour(ref newColour, ref altitudeTextColour, ref baseAltitudeTextColour, true);

            if (IsInitialised || editorMode)
            {
                if (altitudeText != null)
                {
                    SetAltitudeTextBrightness();
                }
            }
        }

        /// <summary>
        /// Set the altitude text colour on the heads-up display. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the HUD with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetAltitudeTextColour(Color newColour)
        {
            SSCUtils.UpdateColour(ref newColour, ref altitudeTextColour, ref baseAltitudeTextColour, true);

            if (IsInitialised || editorMode)
            {
                if (altitudeText != null)
                {
                    if (brightness == 1f) { altitudeText.color = newColour; }
                    else { altitudeText.color = baseAltitudeTextColour.GetColorWithBrightness(brightness); }
                }
            }
        }

        #endregion

        #region Public API Methods - Air Speed

        /// <summary>
        /// Attempt to show the Air speed indicator. Turn on HUD if required.
        /// </summary>
        public void ShowAirSpeed()
        {
            ShowOrHideAirSpeed(true);
        }

        /// <summary>
        /// Attempt to turn off the AirSpeed indicator
        /// </summary>
        public void HideAirSpeed()
        {
            ShowOrHideAirSpeed(false);
        }

        /// <summary>
        /// Set the airspeed text colour on the heads-up display. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the HUD with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetAirSpeedTextColour(Color32 newColour)
        {
            SSCUtils.UpdateColour(ref newColour, ref airspeedTextColour, ref baseAirspeedTextColour, true);

            if (IsInitialised || editorMode)
            {
                if (airspeedText != null)
                {
                    SetAirSpeedTextBrightness();
                }
            }
        }

        /// <summary>
        /// Set the airspeed text colour on the heads-up display. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the HUD with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetAirSpeedTextColour(Color newColour)
        {
            SSCUtils.UpdateColour(ref newColour, ref airspeedTextColour, ref baseAirspeedTextColour, true);

            if (IsInitialised || editorMode)
            {
                if (airspeedText != null)
                {
                    if (brightness == 1f) { airspeedText.color = newColour; }
                    else { airspeedText.color = baseAirspeedTextColour.GetColorWithBrightness(brightness); }
                }
            }
        }

        #endregion

        #region Public API Methods - Attitude

        /// <summary>
        /// Attempt to turn off the Attitude display
        /// </summary>
        public void HideAttitude()
        {
            ShowOrHideAttitude(false);
        }

        /// <summary>
        /// Set the sprite that will mask the Display Attitude scrollable sprite
        /// </summary>
        /// <param name="newSprite"></param>
        public void SetDisplayAttitudeMaskSprite (Sprite newSprite)
        {
            attitudeMaskSprite = newSprite;

            if (attitudeMaskImg != null)
            {
                attitudeMaskImg.sprite = attitudeMaskSprite;
            }
        }

        /// <summary>
        /// This is called when the HUD is resized or some elements of the Attitude control are modified
        /// </summary>
        public void RefreshAttitudeAfterResize()
        {
            // Recalculate the pixels per degree in the pitch ladder
            if (attitudeScrollImg != null && attitudeScrollImg.mainTexture != null)
            {
                // The attitude scroll img should be -90 to 90 deg. Ideally it should have a "border"
                // at the top and bottom of blank (transparent) space to prevent +-90 being right
                // at the top/bottom of the HUD.
                // The image is autoscaled to the full height of the default HUD screen size.

                float imgHeight = attitudeScrollImg.mainTexture.height;

                // If the image
                if (imgHeight < 10f) { imgHeight = 1024f; }

                attitudePixelsPerDegree = cvsResolutionFull.y * ((imgHeight - attitudeScrollSpriteBorderWidth * 2f) / imgHeight) / 180f;
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("RefreshAttitudeAfterResize - Attitude scroll sprite has no UI texture");
            }
            #endif
        }

        /// <summary>
        /// Set the Display Attitude primary colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the attitude with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayAttitudePrimaryColour (Color newColour)
        {
            SSCUtils.UpdateColour(ref newColour, ref attitudePrimaryColour, ref baseAttitudePrimaryColour, true);

            SetAttitudeBrightness();
        }

        /// <summary>
        /// Set the Display Attitude scroll sprite.
        /// </summary>
        /// <param name="newSprite"></param>
        public void SetDisplayAttitudeScrollSprite (Sprite newSprite)
        {
            attitudeScrollSprite = newSprite;

            if (attitudeScrollImg != null)
            {
                attitudeScrollImg.sprite = attitudeScrollSprite;

                // Recalculate the pixels per degree for scrolling.
                RefreshAttitudeAfterResize();
            }
        }

        /// <summary>
        /// Set the normalised size of the scrollable Display Attitude.
        /// Currently width is always 1.0
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetDisplayAttitudeSize (float width, float height)
        {
            // Verify the width,height values are within range 0.1 to 1.
            if (height >= 0.1f && height <= 1f)
            {
                attitudeHeight = height;
            }

            attitudeWidth = 1f;

            if (IsInitialised || editorMode)
            {
                #if UNITY_EDITOR
                if (editorMode)
                {
                    CheckHUDResize(false);
                    UnityEditor.Undo.RecordObject(attitudeRectTfrm.transform, string.Empty);
                }
                #endif

                // Here we use the original (reference) HUD size as it doesn't need to be scaled in any way.
                // The parent HUD canvas is correctly scaled and sized to the actual monitor or device display.
                attitudeRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, attitudeWidth * cvsResolutionFull.x);
                attitudeRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, attitudeHeight * cvsResolutionFull.y);
            }
        }

        /// <summary>
        /// Set the normalised offset (position) of the Display Attitude on the HUD.
        /// If the module has been initialised, this will also re-position the Display Attitude.
        /// </summary>
        /// <param name="offsetX">Horizontal offset from centre. Range between -1 and 1</param>
        /// <param name="offsetY">Vertical offset from centre. Range between -1 and 1</param>
        public void SetDisplayAttitudeOffset (float offsetX, float offsetY)
        {
            // Verify the x,y values are within range -1 to 1.
            if (offsetX >= -1f && offsetX <= 1f)
            {
                attitudeOffsetX = offsetX;
            }

            if (offsetY >= -1f && offsetY <= 1f)
            {
                attitudeOffsetY = offsetY;
            }

            if (IsInitialised || editorMode)
            {
                Transform attTrfm = attitudeRectTfrm.transform;

                #if UNITY_EDITOR
                if (editorMode)
                {
                    CheckHUDResize(false);
                    UnityEditor.Undo.RecordObject(attTrfm, string.Empty);
                }
                #endif

                // Use the scaled HUD size. This works in and out of play mode
                tempAttitudeOffset.x = attitudeOffsetX * cvsResolutionHalf.x;
                tempAttitudeOffset.y = attitudeOffsetY * cvsResolutionHalf.y;
                tempAttitudeOffset.z = attTrfm.localPosition.z;

                attTrfm.localPosition = tempAttitudeOffset;

                // Reset the position of the scrollable image
                if (attitudeScrollPanel != null)
                {
                    attitudeScrollPanel.localPosition = Vector3.zero;
                    attitudeInitPosition = attitudeScrollPanel.transform.position;

                    //if (showAttitudeIndicator && attitudeIndicatorPanel != null)
                    //{
                    //    attitudeIndicatorPanel.localPosition = new Vector3(attitudeScrollPanel.localPosition.x, attitudeScrollPanel.localPosition.y - 20f, attitudeScrollPanel.localPosition.z);
                    //}
                }
            }
        }

        /// <summary>
        /// Set the border width of the Scroll Sprite (texture) in pixels
        /// </summary>
        /// <param name="newBorderWidth"></param>
        public void SetDisplayAttitudeSpriteBorderWidth (float newBorderWidth)
        {
            if (newBorderWidth >= 0f)
            {
                attitudeScrollSpriteBorderWidth = newBorderWidth;

                RefreshAttitudeAfterResize();
            }
        }

        /// <summary>
        /// Attempt to show the scrollable Attitude. Turn on HUD if required.
        /// </summary>
        public void ShowAttitude()
        {
            ShowOrHideAttitude(true);
        }

        #endregion

        #region Public API Methods - Fade

        /// <summary>
        /// Set the colour the display will fade in from or out to.
        /// </summary>
        /// <param name="newFadeColour"></param>
        public void SetDisplayFadeColour (Color newFadeColour)
        {
            fadeColour = newFadeColour;
        }

        /// <summary>
        /// Start fading in or out the display.
        /// </summary>
        /// <param name="isFadeIn"></param>
        public void StartDisplayFade (bool isFadeIn)
        {
            if (IsInitialised)
            {
                RectTransform fadePanel = GetFadePanel();

                if (fadePanel != null)
                {
                    // Add an Image component if one doesn't already exist
                    if (fadeImage == null && !fadePanel.TryGetComponent<Image>(out fadeImage))
                    {
                        fadeImage = fadePanel.gameObject.AddComponent<Image>();
                        fadeImage.raycastTarget = false;
                    }

                    // immediately turn off the image in case it was just added
                    fadeImage.enabled = false;

                    // Set up the transistion values
                    if (isFadeIn)
                    { fadeStartAlpha = 1f; fadeTargetValue = 0f; }
                    else { fadeStartAlpha = 0f; fadeTargetValue = 1f; }
                    // Initialise the fade image colour
                    currentFadeColour = fadeColour;
                    currentFadeColour.a = fadeStartAlpha;
                    //fadeImage.color = new Color(Color.black.r, Color.black.g, Color.black.b, fadeStartAlpha);
                    fadeImage.color = currentFadeColour;

                    // Could use fadeImage.CrossFadeAlpha(fadeTargetValue, fadeInDuration, true)
                    // but have no notification when done - would need to check in Update()

                    if (isFadeIn) { StartCoroutine(FadeDisplayIn()); }
                    else { StartCoroutine(FadeDisplayOut()); }
                }
            }
        }

        /// <summary>
        /// If fading in or out, turn the fade off.
        /// </summary>
        public void StopDisplayFade()
        {
            if (isFadingIn)
            {
                isFadingIn = false;
                StopCoroutine(FadeDisplayIn());
            }

            if (isFadingOut)
            {
                isFadingOut = false;
                StopCoroutine(FadeDisplayOut());
            }

            if (fadeImage != null) { fadeImage.enabled = false; }
        }

        #endregion

        #region Public API Methods - Flickering

        /// <summary>
        /// Flicker the HUD on/off. Override the Flicker Default Duration.
        /// Show the HUD when flickering finishes.
        /// </summary>
        /// <param name="duration"></param>
        public void FlickerOn (float duration)
        {
            if (duration > 0f)
            {
                flickerDuration = duration;
                flickerDurationTimer = 0f;
                isFlickerWaiting = false;

                isFlickeringEndStateOn = true;
                // If the HUD is flickering to the Shown state, it cannot also be flickering off.
                isFlickeringEndStateOff = false;

                if (flickerHistory != null) { flickerHistory.Clear(); }

                // Remember the current overall HUD brightness setting
                flickerStartIntensity = brightness;

                if (!flickerVariableIntensity) { flickerIntensity = flickerStartIntensity; }
            }
        }

        /// <summary>
        /// Flicker the HUD on/off. Override the Flicker Default Duration.
        /// Hide the HUD when flickering finishers.
        /// </summary>
        /// <param name="duration"></param>
        public void FlickerOff (float duration)
        {
            if (duration > 0f)
            {
                flickerDuration = duration;
                flickerDurationTimer = 0f;
                isFlickerWaiting = false;

                isFlickeringEndStateOff = true;
                // If the HUD is flickering to the Hidden state, it cannot also be flickering on.
                isFlickeringEndStateOn = false;

                if (flickerHistory != null) { flickerHistory.Clear(); }

                // Remember the current overall HUD brightness setting
                flickerStartIntensity = brightness;

                if (!flickerVariableIntensity) { flickerIntensity = flickerStartIntensity; }
            }
        }


        public void StopFlickering()
        {
            isFlickeringEndStateOn = false;
            isFlickeringEndStateOff = false;
        }

        #endregion

        #region Public API Methods - Gauges

        /// <summary>
        /// Add a new gauge to the HUD. By design, they are not visible at runtime when first added.
        /// </summary>
        /// <param name="gaugeName"></param>
        /// <param name="gaugeText"></param>
        /// <returns></returns>
        public DisplayGauge AddGauge(string gaugeName, string gaugeText)
        {
            DisplayGauge displayGauge = null;

            if (GetHUDPanel() == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: ShipDisplayModule.AddGauge() - could not find HUDPanel for the HUD. Did you use the prefab from Prefabs/Visuals folder?");
                #endif
            }
            else
            {
                displayGauge = new DisplayGauge();
                if (displayGauge != null)
                {
                    displayGauge.gaugeName = gaugeName;
                    displayGauge.gaugeString = gaugeText;
                    ValidateGaugeList();
                    displayGaugeList.Add(displayGauge);

                    numDisplayGauges = displayGaugeList.Count;

                    RectTransform gaugePanel = CreateGaugePanel(displayGauge);
                    if (gaugePanel != null)
                    {
                        #if UNITY_EDITOR
                        // Make sure we can rollback the creation of the new gauge panel for this slot
                        UnityEditor.Undo.RegisterCreatedObjectUndo(gaugePanel.gameObject, string.Empty);
                        #endif
                    }
                 }
            }

            return displayGauge;
        }

        /// <summary>
        /// Add a gauge to the HUD using a displayGauge instance. Typically, this is used
        /// with CopyDisplayGauge(..).
        /// </summary>
        /// <param name="displayGauge"></param>
        public void AddGauge(DisplayGauge displayGauge)
        {
            if (displayGauge != null)
            {
                if (GetHUDPanel() == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: ShipDisplayModule.AddGauge() - could not find HUDPanel for the HUD. Did you use the prefab from Prefabs/Visuals folder?");
                    #endif
                }
                else
                {
                    ValidateGaugeList();
                    displayGaugeList.Add(displayGauge);

                    numDisplayGauges = displayGaugeList.Count;

                    RectTransform gaugePanel = CreateGaugePanel(displayGauge);
                    if (gaugePanel != null)
                    {
                        #if UNITY_EDITOR
                        // Make sure we can rollback the creation of the new gauge panel for this slot
                        UnityEditor.Undo.RegisterCreatedObjectUndo(gaugePanel.gameObject, string.Empty);
                        #endif
                    }
                }
            }
        }

        /// <summary>
        /// Delete a gauge from the HUD.
        /// NOTE: It is much cheaper to HideDisplayGauge(..) than completely remove it.
        /// </summary>
        /// <param name="guidHash"></param>
        public void DeleteGauge(int guidHash)
        {
            #if UNITY_EDITOR
            bool isRecordUndo = !Application.isPlaying;
            int undoGroup=0;
            #else
            bool isRecordUndo = false;
            #endif

            RectTransform gaugeRectTfrm = GetGaugePanel(guidHash);
            int _dgIndex = GetDisplayGaugeIndex(guidHash);

            // Only record undo if in the editor AND is not playing AND something needs to be recorded
            isRecordUndo = isRecordUndo && (gaugeRectTfrm != null || _dgIndex >= 0);

            #if UNITY_EDITOR
            if (isRecordUndo)
            {
                UnityEditor.Undo.SetCurrentGroupName("Delete Display Gauge " + (_dgIndex + 1).ToString());
                undoGroup = UnityEditor.Undo.GetCurrentGroup();

                if (_dgIndex >= 0)
                {
                    UnityEditor.Undo.RecordObject(this, string.Empty);
                }
            }
            #endif

            // Check that the gauge exists and it is within the list constraints
            // NOTE: The script element must be modified BEFORE the Undo.DestroyObjectImmediate is called
            // in order for Undo to correctly undo both the gauge change AND the destroy of the panel.
            if (_dgIndex >= 0 && _dgIndex < (displayGaugeList == null ? 0 : displayGaugeList.Count))
            {
                displayGaugeList.RemoveAt(_dgIndex);
                numDisplayGauges = displayGaugeList.Count;
            }

            if (gaugeRectTfrm != null)
            {
                if (Application.isPlaying) { Destroy(gaugeRectTfrm.gameObject); }
                else
                {
                    #if UNITY_EDITOR
                    UnityEditor.Undo.DestroyObjectImmediate(gaugeRectTfrm.gameObject);
                    #else
                    DestroyImmediate(gaugeRectTfrm.gameObject);
                    #endif
                }
            }

            #if UNITY_EDITOR
            if (isRecordUndo)
            {               
                UnityEditor.Undo.CollapseUndoOperations(undoGroup);
            }
            #endif
        }

        /// <summary>
        /// Create a copy of an existing DisplayGauge, and give it a new name.
        /// Call AddGauge(newDisplayGauge) to make it useable in the game.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="NameOfCopy"></param>
        /// <returns></returns>
        public DisplayGauge CopyDisplayGauge(DisplayGauge displayGauge, string NameOfCopy)
        {
            DisplayGauge dgCopy = new DisplayGauge(displayGauge);

            if (dgCopy != null)
            {
                // make it unique
                dgCopy.guidHash = SSCMath.GetHashCodeFromGuid();
                dgCopy.gaugeName = NameOfCopy;
            }
            return dgCopy;
        }

        /// <summary>
        /// Returns the guidHash of the Gauge in the list given the index or zero-based position in the list.
        /// Will return 0 if no matching Gauge is found.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetDisplayGaugeGuidHash(int index)
        {
            int guidHash = 0;

            if (index >= 0 && index < GetNumberDisplayGauges)
            {
                if (displayGaugeList[index] != null) { guidHash = displayGaugeList[index].guidHash; }
            }

            return guidHash;
        }

        /// <summary>
        /// Get the zero-based index of the Gauge in the list. Will return -1 if not found.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public int GetDisplayGaugeIndex(int guidHash)
        {
            int index = -1;

            int _numDisplayGauges = GetNumberDisplayGauges;

            for (int dgIdx = 0; dgIdx < _numDisplayGauges; dgIdx++)
            {
                if (displayGaugeList[dgIdx] != null && displayGaugeList[dgIdx].guidHash == guidHash) { index = dgIdx; break; }
            }

            return index;
        }

        /// <summary>
        /// Get a DisplayGauge given its guidHash.
        /// See also GetDisplayGaugeGuidHash(..).
        /// Will return null if guidHash parameter is 0, it cannot be found.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public DisplayGauge GetDisplayGauge(int guidHash)
        {
            DisplayGauge displayGauge = null;
            DisplayGauge _tempDG = null;

            int _numDisplayGauges = GetNumberDisplayGauges;

            for (int dgIdx = 0; dgIdx < _numDisplayGauges; dgIdx++)
            {
                _tempDG = displayGaugeList[dgIdx];
                if (_tempDG != null && _tempDG.guidHash == guidHash)
                {
                    displayGauge = _tempDG;
                    break;
                }
            }

            return displayGauge;
        }

        /// <summary>
        /// Get the display gauge give the description title of the gauge.
        /// WARNING: This will increase Garbage Collection (GC). Where possible
        /// use GetDisplayGauge(guidHash) and/or GetDisplayGaugeGuidHash(index)
        /// </summary>
        /// <param name="displayGaugeName"></param>
        /// <returns></returns>
        public DisplayGauge GetDisplayGauge(string displayGaugeName)
        {
            DisplayGauge displayGauge = null;
            DisplayGauge _tempDG = null;

            if (!string.IsNullOrEmpty(displayGaugeName))
            {
                int _numDisplayGauges = GetNumberDisplayGauges;

                for (int dgIdx = 0; dgIdx < _numDisplayGauges; dgIdx++)
                {
                    _tempDG = displayGaugeList[dgIdx];
                    if (_tempDG != null && !string.IsNullOrEmpty(_tempDG.gaugeName))
                    {
                        if (_tempDG.gaugeName.ToLower() == displayGaugeName.ToLower())
                        {
                            displayGauge = _tempDG;
                            break;
                        }
                    }
                }
            }
            return displayGauge;
        }

        /// <summary>
        /// Hide or turn off the Display Gauge
        /// </summary>
        /// <param name="guidHash"></param>
        public void HideDisplayGauge(int guidHash)
        {
            DisplayGauge displayGauge = GetDisplayGauge(guidHash);
            if (displayGauge != null) { ShowOrHideGauge(displayGauge, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayGauge - Could not find gauge with guidHash: " + guidHash); }
            #endif
        }

        /// <summary>
        /// Hide or turn off the Display Gauge
        /// </summary>
        /// <param name="displayGauge"></param>
        public void HideDisplayGauge(DisplayGauge displayGauge)
        {
            if (displayGauge != null) { ShowOrHideGauge(displayGauge, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayGauge - Could not find gauge - parameter was null"); }
            #endif
        }

        /// <summary>
        /// Hide or turn off all Display Gauges. ShipDisplayModule must be initialised.
        /// </summary>
        public void HideDisplayGauges()
        {
            ShowOrHideGauges(false);
        }

        /// <summary>
        /// After adding or moving DisplayGauges, they may need to be sorted to
        /// have the correct z-order in on the HUD.
        /// </summary>
        public void RefreshGaugesSortOrder()
        {
            if (gaugesRectTfrm != null)
            {
                // Gauges should begin at index 0.
                int zIndex = -1;

                // Update number of DisplayGauges. This can help issues in the Editor, like moving DislayGauges
                // up or down in the list while in play mode.
                numDisplayGauges = displayGaugeList == null ? 0 : displayGaugeList.Count;

                for (int dtIdx = 0; dtIdx < numDisplayGauges; dtIdx++)
                {
                    DisplayGauge displayGauge = displayGaugeList[dtIdx];

                    if (displayGauge != null)
                    {
                        RectTransform _rt = displayGauge.CachedGaugePanel;

                        if (_rt != null)
                        {
                            _rt.SetSiblingIndex(++zIndex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Import a json file from disk and return as DisplayGauge
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public DisplayGauge ImportGaugeFromJson (string folderPath, string fileName)
        {
            DisplayGauge displayGauge = null;

            if (!string.IsNullOrEmpty(folderPath) && !string.IsNullOrEmpty(fileName))
            {
                try
                {
                    string filePath = System.IO.Path.Combine(folderPath, fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        string jsonText = System.IO.File.ReadAllText(filePath);

                        displayGauge = new DisplayGauge();
                        int displayGaugeGuidHash = displayGauge.guidHash;

                        JsonUtility.FromJsonOverwrite(jsonText, displayGauge);

                        if (displayGauge != null)
                        {
                            // make hash code unique
                            displayGauge.guidHash = displayGaugeGuidHash;
                        }
                    }
                    #if UNITY_EDITOR
                    else
                    {
                        Debug.LogWarning("ERROR: Import Gauge. Could not find file at " + filePath);
                    }
                    #endif
                }
                catch (System.Exception ex)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: ShipDisplayModule - could not import gauge from: " + folderPath + " PLEASE REPORT - " + ex.Message);
                    #else
                    // Keep compiler happy
                    if (ex != null) { }
                    #endif
                }
            }

            return displayGauge;
        }

        /// <summary>
        /// Save the Gauge to a json file on disk.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="filePath"></param>
        public bool SaveGaugeAsJson (DisplayGauge displayGauge, string filePath)
        {
            bool isSuccessful = false;

            if (displayGauge != null && !string.IsNullOrEmpty(filePath))
            {
                try
                {
                    string jsonGaugeData = JsonUtility.ToJson(displayGauge);

                    if (!string.IsNullOrEmpty(jsonGaugeData) && !string.IsNullOrEmpty(filePath))
                    {
                        System.IO.File.WriteAllText(filePath, jsonGaugeData);
                        isSuccessful = true;
                    }
                }
                catch (System.Exception ex)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: ShipDisplayModule SaveGaugeAsJson - could not export: " + displayGauge.gaugeName + " PLEASE REPORT - " + ex.Message);
                    #else
                    // Keep compiler happy
                    if (ex != null) { }
                    #endif
                }
            }

            return isSuccessful;
        }

        /// <summary>
        /// The foreground colour of the gauge will be determined by the gauge value and the low, medium and high colours.
        /// LowColour = value of 0, MediumColour when value is 0.5, and HighColour when value is 1.0
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="lowColour"></param>
        /// <param name="mediumColour"></param>
        /// <param name="highColour"></param>
        public void SetDisplayGaugeValueAffectsColourOn(DisplayGauge displayGauge, Color lowColour, Color mediumColour, Color highColour)
        {
            if (displayGauge != null)
            {
                SSCUtils.ColortoColorNoAlloc(ref lowColour, ref displayGauge.foregroundLowColour);
                SSCUtils.ColortoColorNoAlloc(ref mediumColour, ref displayGauge.foregroundMediumColour);
                SSCUtils.ColortoColorNoAlloc(ref highColour, ref displayGauge.foregroundHighColour);

                displayGauge.isColourAffectByValue = true;

                if (IsInitialised || editorMode)
                {
                    SetDisplayGaugeValue(displayGauge, displayGauge.gaugeValue);
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeValueAffectsColour - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// The value of the gauge does not affect the foreground colour.
        /// When turning off this feature the new foreground colour would typically be the old foregroundHighColour.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="newForegroundColour"></param>
        public void SetDisplayGaugeValueAffectsColourOff(DisplayGauge displayGauge, Color newForegroundColour)
        {
            if (displayGauge != null)
            {
                displayGauge.isColourAffectByValue = false;

                if (IsInitialised || editorMode)
                {
                    SetDisplayGaugeForegroundColour(displayGauge, newForegroundColour);
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeValueAffectsColour - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the gauge value at which the foreground medium colour should be set.
        /// The default value is 0.5.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="newValue"></param>
        public void SetDisplayGaugeMediumColourValue (DisplayGauge displayGauge, float newValue)
        {
            if (displayGauge != null)
            {
                if (newValue > 0f && newValue < 1f)
                {
                    displayGauge.mediumColourValue = newValue;
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeMediumColourValue - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the offset (position) of the Display Gauge on the HUD.
        /// If the module has been initialised, this will also re-position the Display Gauge.
        /// </summary>
        /// <param name="offsetX">Horizontal offset from centre. Range between -1 and 1</param>
        /// <param name="offsetY">Vertical offset from centre. Range between -1 and 1</param>
        public void SetDisplayGaugeOffset(DisplayGauge displayGauge, float offsetX, float offsetY)
        {
            // Verify the x,y values are within range -1 to 1.
            if (offsetX >= -1f && offsetX <= 1f)
            {
                displayGauge.offsetX = offsetX;
            }

            if (offsetY >= -1f && offsetY <= 1f)
            {
                displayGauge.offsetY = offsetY;
            }

            if (IsInitialised || editorMode)
            {
                Transform dgTrfm = displayGauge.CachedGaugePanel.transform;

                #if UNITY_EDITOR
                if (editorMode)
                {
                    CheckHUDResize(false);
                    UnityEditor.Undo.RecordObject(dgTrfm, string.Empty);
                }
                #endif

                // Use the scaled HUD size. This works in and out of play mode
                tempGaugeOffset.x = displayGauge.offsetX * cvsResolutionHalf.x;
                tempGaugeOffset.y = displayGauge.offsetY * cvsResolutionHalf.y;
                tempGaugeOffset.z = dgTrfm.localPosition.z;

                dgTrfm.localPosition = tempGaugeOffset;
            }
        }

        /// <summary>
        /// Set the size of the Gauge Panel.
        /// If the module has been initialised, this will also resize the Gauge Panel.
        /// The values are only updated if they are outside the range 0.0 to 1.0 or have changed.
        /// Also updates the Text direction
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="width">Range between 0.0 and 1.0</param>
        /// <param name="height">Range between 0.0 and 1.0</param>
        public void SetDisplayGaugeSize(DisplayGauge displayGauge, float width, float height)
        {
            // Clamp the x,y values 0.0 to 1.0
            if (width < 0f) { displayGauge.displayWidth = 0f; }
            else if (width > 1f) { displayGauge.displayWidth = 1f; }
            else if (width != displayGauge.displayWidth) { displayGauge.displayWidth = width; }

            if (height < 0f) { displayGauge.displayHeight = 0f; }
            else if (height > 1f) { displayGauge.displayHeight = 1f; }
            else if (height != displayGauge.displayHeight) { displayGauge.displayHeight = height; }

            if (IsInitialised || editorMode)
            {
                #if UNITY_EDITOR
                if (editorMode) { CheckHUDResize(false); }
                #endif

                RectTransform dgRectTfrm = displayGauge.CachedGaugePanel;

                float pixelWidth = displayGauge.displayWidth * cvsResolutionFull.x;
                float pixelHeight = displayGauge.displayHeight * cvsResolutionFull.y;

                if (dgRectTfrm != null)
                {
                    // Here we use the original (reference) HUD size as it doesn't need to be scaled in any way.
                    // The parent HUD canvas is correctly scaled and sized to the actual monitor or device display.
                    dgRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixelWidth);
                    dgRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixelHeight);
                }

                if (displayGauge.CachedFgImgComponent != null)
                {
                    RectTransform dgFgImgRectTfrm = displayGauge.CachedFgImgComponent.rectTransform;

                    dgFgImgRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixelWidth);
                    dgFgImgRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixelHeight);
                }

                SetDisplayGaugeTextDirection(displayGauge, displayGauge.textDirection);
            }
        }

        /// <summary>
        /// Set the type or style of the gauge.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="dgType"></param>
        public void SetDisplayGaugeType (DisplayGauge displayGauge, DisplayGauge.DGType dgType)
        {
            if (displayGauge != null)
            {
                displayGauge.gaugeType = dgType;

                GetOrCreateOrRemoveGaugeLabel(displayGauge);

                if (displayGauge.HasLabel)
                {
                    // Refresh the label
                    SetDisplayGaugeTextBrightness(displayGauge);

                    SetDisplayGaugeTextFontStyle(displayGauge, (UnityEngine.FontStyle)displayGauge.fontStyle);
                    SetDisplayGaugeTextFontSize(displayGauge, displayGauge.isBestFit, displayGauge.fontMinSize, displayGauge.fontMaxSize);

                    SetDisplayGaugeLabelAlignment(displayGauge, displayGauge.labelAlignment);
                }

                // Refresh the Text direction which also does the sizing for gauges with and without labels
                SetDisplayGaugeTextDirection(displayGauge, displayGauge.textDirection);

                SetDisplayGaugeValue(displayGauge, displayGauge.gaugeValue);
            }
        }

        /// <summary>
        /// Update the value or reading of the gauge. If Value Affects Colour (isColourAffectByValue)
        /// is enabled, the foreground colour of the gauge will also be updated.
        /// Values should be in range 0.0 to 1.0.
        /// NOTE: Numeric gauges can increase GC at runtime. Where possible, only call this method when
        /// the value changes.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="gaugeValue"></param>
        public void SetDisplayGaugeValue (DisplayGauge displayGauge, float gaugeValue)
        {
            if (displayGauge != null)
            {
                // Clamp 0.0 to 1.0
                float _gaugeValue = gaugeValue < 0f ? 0f : gaugeValue > 1f ? 1f : gaugeValue;

                displayGauge.gaugeValue = _gaugeValue;
                UnityEngine.UI.Image fgImg = displayGauge.CachedFgImgComponent;

                if (fgImg != null)
                {
                    fgImg.fillAmount = displayGauge.isFillMethodNone ? 0f : _gaugeValue;

                    if (displayGauge.isColourAffectByValue)
                    {
                        if (_gaugeValue > displayGauge.mediumColourValue)
                        {
                            Color _newColour = Color.Lerp(displayGauge.foregroundMediumColour, displayGauge.foregroundHighColour, SSCMath.Normalise(_gaugeValue,  displayGauge.mediumColourValue, 1f));

                            SSCUtils.UpdateColour(ref _newColour, ref displayGauge.foregroundColour, ref displayGauge.baseForegroundColour, false);
                            SetDisplayGaugeForegroundBrightness(displayGauge);
                        }
                        else
                        {
                            Color _newColour = Color.Lerp(displayGauge.foregroundLowColour, displayGauge.foregroundMediumColour, SSCMath.Normalise(_gaugeValue, 0f, displayGauge.mediumColourValue));

                            SSCUtils.UpdateColour(ref _newColour, ref displayGauge.foregroundColour, ref displayGauge.baseForegroundColour, true);
                            SetDisplayGaugeForegroundBrightness(displayGauge);
                        }
                    }
                }

                int gaugeTypeInt = (int)displayGauge.gaugeType;

                // If this is a numeric gauge, update the text with the numeric value
                if (gaugeTypeInt == DisplayGauge.DGTypeFilledNumber1Int || gaugeTypeInt == DisplayGauge.DGTypeNumberWithLabel1Int)
                {
                    float _displayTxtValue = displayGauge.isNumericPercentage ? _gaugeValue * 100f : _gaugeValue * displayGauge.gaugeMaxValue;

                    displayGauge.gaugeString = SSCUtils.GetNumericString(_displayTxtValue, displayGauge.gaugeDecimalPlaces, displayGauge.isNumericPercentage);

                    // Update the text field for the gauge on the HUD UI
                    Text uiText = displayGauge.CachedTextComponent;

                    if (uiText != null) { uiText.text = displayGauge.gaugeString; }
                }

            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeValue - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Update the text of the gauge label. This only applies to numeric gauges with labels.
        /// For non-numeric gauges see SetDisplayGaugeText(..).
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="gaugeLabel"></param>
        public void SetDisplayGaugeLabel (DisplayGauge displayGauge, string gaugeLabel)
        {
            if (displayGauge != null)
            {
                displayGauge.gaugeLabel = gaugeLabel;
                Text uiText = displayGauge.CachedLabelTextComponent;

                if (uiText != null) { uiText.text = gaugeLabel; }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeLabel - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Update the position of the label within the gauge panel.
        /// Only applies to numeric gauges with a label.
        /// See also SetDisplayGaugeTextAlignment(..)
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="textAlignment"></param>
        public void SetDisplayGaugeLabelAlignment (DisplayGauge displayGauge, TextAnchor textAlignment)
        {
            if (displayGauge != null)
            {
                if (!editorMode) { displayGauge.labelAlignment = textAlignment; }

                // If this is not a numeric gauge with a label, this will return null.
                Text uiText = displayGauge.CachedLabelTextComponent;

                if (uiText != null) { uiText.alignment = textAlignment; }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeLabelAlignment - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Update the text of the gauge. For numeric gauges with labels, see
        /// SetDisplayGaugeLabel(..).
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="gaugeText"></param>
        public void SetDisplayGaugeText (DisplayGauge displayGauge, string gaugeText)
        {
            if (displayGauge != null)
            {
                displayGauge.gaugeString = gaugeText;
                Text uiText = displayGauge.CachedTextComponent;

                if (uiText != null) { uiText.text = gaugeText; }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeText - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Update the position of the text within the gauge panel
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="textAlignment"></param>
        public void SetDisplayGaugeTextAlignment(DisplayGauge displayGauge, TextAnchor textAlignment)
        {
            if (displayGauge != null)
            {
                if (!editorMode) { displayGauge.textAlignment = textAlignment; }

                Text uiText = displayGauge.CachedTextComponent;

                if (uiText != null) { uiText.alignment = textAlignment; }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeTextAlignment - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the font of the DisplayGauge Text component
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="font"></param>
        public void SetDisplayGaugeTextFont(DisplayGauge displayGauge, Font font)
        {
            if (displayGauge != null)
            {
                Text uiText = displayGauge.CachedTextComponent;

                if (uiText != null)
                {
                    uiText.font = font;
                }

                // If this is numeric gauge with a label, also set the label
                if (displayGauge.HasLabel && displayGauge.CachedLabelTextComponent != null)
                {
                    displayGauge.CachedLabelTextComponent.font = font;
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeTextFont - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the font style of the DisplayGauge Text component
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="font"></param>
        public void SetDisplayGaugeTextFontStyle(DisplayGauge displayGauge, FontStyle fontStyle)
        {
            if (displayGauge != null)
            {
                displayGauge.fontStyle = (DisplayGauge.DGFontStyle)fontStyle;

                Text uiText = displayGauge.CachedTextComponent;

                if (uiText != null)
                {
                    uiText.fontStyle = fontStyle;
                }

                // If this is numeric gauge with a label, also set the label
                if (displayGauge.HasLabel && displayGauge.CachedLabelTextComponent != null)
                {
                    displayGauge.CachedLabelTextComponent.fontStyle = fontStyle;
                }

            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeTextFontStyle - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the font size of the display gauge text. If isBestFit is false, maxSize is the font size set.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="isBestFit"></param>
        /// <param name="minSize"></param>
        /// <param name="maxSize"></param>
        public void SetDisplayGaugeTextFontSize(DisplayGauge displayGauge, bool isBestFit, int minSize, int maxSize)
        {
            if (displayGauge != null)
            {
                if (!editorMode)
                {
                    displayGauge.isBestFit = isBestFit;
                    displayGauge.fontMinSize = minSize;
                    displayGauge.fontMaxSize = maxSize;
                }

                Text uiText = displayGauge.CachedTextComponent;

                if (uiText != null)
                {
                    uiText.resizeTextForBestFit = isBestFit;
                    uiText.resizeTextMinSize = minSize;
                    uiText.resizeTextMaxSize = maxSize;
                    uiText.fontSize = maxSize;
                }

                // If this is numeric gauge with a label, also set the label
                if (displayGauge.HasLabel && displayGauge.CachedLabelTextComponent != null)
                {
                    Text uiLabelText = displayGauge.CachedLabelTextComponent;
                    uiLabelText.resizeTextForBestFit = isBestFit;
                    uiLabelText.resizeTextMinSize = minSize;
                    uiLabelText.resizeTextMaxSize = maxSize;
                    uiLabelText.fontSize = maxSize;
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeTextFontSize - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Gauge text colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the gauge text with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayGaugeTextColour(DisplayGauge displayGauge, Color newColour)
        {
            if (displayGauge != null)
            {
                SSCUtils.UpdateColour(ref newColour, ref displayGauge.textColour, ref displayGauge.baseTextColour, true);

                SetDisplayGaugeTextBrightness(displayGauge);
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeTextColour - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Update the direction of the text within the gauge panel
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="textDirection"></param>
        public void SetDisplayGaugeTextDirection(DisplayGauge displayGauge, DisplayGauge.DGTextDirection textDirection)
        {
            if (displayGauge != null)
            {
                if (!editorMode) { displayGauge.textDirection = textDirection; }

                Text uiText = displayGauge.CachedTextComponent;
                Text uiLabelText = displayGauge.CachedLabelTextComponent;

                bool hasLabel = uiLabelText != null && displayGauge.HasLabel;

                if (uiText != null)
                {
                    #if UNITY_EDITOR
                    if (editorMode) { CheckHUDResize(false); }
                    #endif

                    float pixelWidth = displayGauge.displayWidth * cvsResolutionFull.x;
                    float pixelHeight = displayGauge.displayHeight * cvsResolutionFull.y;
                    float textRotation = 0f;

                    RectTransform dgTextRectTfrm = displayGauge.CachedTextComponent.rectTransform;
                    RectTransform dgLabelRectTfrm = hasLabel ? displayGauge.CachedLabelTextComponent.rectTransform : null;

                    if (textDirection == DisplayGauge.DGTextDirection.Horizontal)
                    {
                        if (hasLabel)
                        {
                            pixelHeight *= 0.5f;
                            dgLabelRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixelWidth);
                            dgLabelRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixelHeight);
                        }

                        dgTextRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixelWidth);
                        dgTextRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixelHeight);
                    }
                    else
                    {
                        if (textDirection == DisplayGauge.DGTextDirection.BottomTop)
                        {
                            textRotation = 90f;
                        }
                        else { textRotation = 270f; }

                        if (hasLabel)
                        {
                            pixelWidth *= 0.5f;
                            dgLabelRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixelHeight);
                            dgLabelRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixelWidth);
                        }

                        dgTextRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixelHeight);
                        dgTextRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixelWidth);
                    }

                    uiText.transform.localRotation = Quaternion.Euler(uiText.transform.localRotation.x, uiText.transform.localRotation.y, textRotation);

                    if (hasLabel)
                    {
                        uiLabelText.transform.localRotation = Quaternion.Euler(uiLabelText.transform.localRotation.x, uiLabelText.transform.localRotation.y, textRotation);

                        if (textDirection == DisplayGauge.DGTextDirection.Horizontal)
                        {
                            uiLabelText.transform.localPosition = new Vector3(0f, pixelHeight / 2f, 0f);
                            uiText.transform.localPosition = new Vector3(0f, -pixelHeight / 2f, 0f);
                        }
                        else if (textDirection == DisplayGauge.DGTextDirection.BottomTop)
                        {
                            uiLabelText.transform.localPosition = new Vector3(-pixelWidth / 2f, 0f, 0f);
                            uiText.transform.localPosition = new Vector3(pixelWidth / 2f, 0f, 0f);
                        }
                        else
                        {
                            uiLabelText.transform.localPosition = new Vector3(pixelWidth / 2f, 0f, 0f);
                            uiText.transform.localPosition = new Vector3(-pixelWidth / 2f, 0f, 0f);
                        }
                    }
                    else
                    {
                        // This will correct the position if the dev has (incorrectly) moved the panel manually in the scene
                        uiText.transform.localPosition = Vector3.zero;
                    }
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeTextRotation - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Gauge foreground colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the gauge foreground with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayGaugeForegroundColour(DisplayGauge displayGauge, Color newColour)
        {
            if (displayGauge != null)
            {
                SSCUtils.UpdateColour(ref newColour, ref displayGauge.foregroundColour, ref displayGauge.baseForegroundColour, true);

                SetDisplayGaugeForegroundBrightness(displayGauge);
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeForegroundColour - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Gauge foreground sprite. This is used to render the gauge value by partially filling it.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="newSprite"></param>
        public void SetDisplayGaugeForegroundSprite(DisplayGauge displayGauge, Sprite newSprite)
        {
            if (displayGauge != null)
            {
                displayGauge.foregroundSprite = newSprite;

                UnityEngine.UI.Image fgImg = displayGauge.CachedFgImgComponent;

                if (fgImg != null)
                {
                    fgImg.sprite = newSprite;
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeForegroundSprite - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Gauge background colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the gauge background with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayGaugeBackgroundColour(DisplayGauge displayGauge, Color newColour)
        {
            if (displayGauge != null)
            {
                SSCUtils.UpdateColour(ref newColour, ref displayGauge.backgroundColour, ref displayGauge.baseBackgroundColour, true);

                SetDisplayGaugeBackgroundBrightness(displayGauge);
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeBackgroundColour - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Gauge background sprite. This is used to render the background image of the gauge.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="newSprite"></param>
        public void SetDisplayGaugeBackgroundSprite(DisplayGauge displayGauge, Sprite newSprite)
        {
            if (displayGauge != null)
            {
                displayGauge.backgroundSprite = newSprite;

                UnityEngine.UI.Image bgImg = displayGauge.CachedBgImgComponent;

                if (bgImg != null)
                {
                    bgImg.sprite = newSprite;
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeBackgroundSprite - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Gauge fill method. This determines how the gauge is filled
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="fillMethod"></param>
        public void SetDisplayGaugeFillMethod(DisplayGauge displayGauge, DisplayGauge.DGFillMethod fillMethod)
        {
            if (displayGauge != null)
            {
                UnityEngine.UI.Image fgImg = displayGauge.CachedFgImgComponent;

                if (fgImg != null)
                {
                    displayGauge.isFillMethodNone = displayGauge.fillMethod == DisplayGauge.DGFillMethod.None;

                    fgImg.fillMethod = displayGauge.isFillMethodNone ? Image.FillMethod.Horizontal : (UnityEngine.UI.Image.FillMethod)fillMethod;

                    // Refresh the gauge if case changing from/to Fill Method None.
                    SetDisplayGaugeValue(displayGauge, displayGauge.gaugeValue);
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeFillMethod - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Sets whether or not the foreground and background sprites keep their original texture aspect ratio.
        /// This can be useful when creating circular gauges.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="fillMethod"></param>
        public void SetDisplayGaugeKeepAspectRatio(DisplayGauge displayGauge, bool isKeepAspectRatio)
        {
            if (displayGauge != null)
            {
                UnityEngine.UI.Image fgImg = displayGauge.CachedFgImgComponent;
                UnityEngine.UI.Image bgImg = displayGauge.CachedBgImgComponent;

                if (fgImg != null)
                {
                    fgImg.preserveAspect = isKeepAspectRatio;
                }

                if (bgImg != null)
                {
                    bgImg.preserveAspect = isKeepAspectRatio;
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayGaugeKeepAspectRatio - displayGauge is null"); }
            #endif
        }
      
        /// <summary>
        /// Show the Display Gauge on the HUD. The HUD will automatically be shown if it is not already visible.
        /// </summary>
        /// <param name="guidHash"></param>
        public void ShowDisplayGauge(int guidHash)
        {
            DisplayGauge displayGauge = GetDisplayGauge(guidHash);
            if (displayGauge != null) { ShowOrHideGauge(displayGauge, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayGauge - Could not find gauge with guidHash: " + guidHash); }
            #endif
        }

        /// <summary>
        /// Show the Display Gauge on the HUD. The HUD will automatically be shown if it is not already visible.
        /// </summary>
        /// <param name="displayGauge"></param>
        public void ShowDisplayGauge(DisplayGauge displayGauge)
        {
            if (displayGauge != null) { ShowOrHideGauge(displayGauge, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayGauge - Could not find gauge - parameter was null"); }
            #endif
        }

        /// <summary>
        /// Show or turn on all Display Gauges. ShipDisplayModule must be initialised.
        /// The HUD will automatically be shown if it is not already visible.
        /// </summary>
        public void ShowDisplayGauges()
        {
            ShowOrHideGauges(true);
        }

        /// <summary>
        /// Create a new list if required
        /// </summary>
        public void ValidateGaugeList()
        {
            if (displayGaugeList == null) { displayGaugeList = new List<DisplayGauge>(1); }
        }

        #endregion

        #region Public API Methods - Heading

        /// <summary>
        /// Set the small sprite that will indicate or point to the heading on the HUD
        /// </summary>
        /// <param name="newSprite"></param>
        public void SetDisplayHeadingIndicatorSprite (Sprite newSprite)
        {
            headingIndicatorSprite = newSprite;

            if (headingIndicatorImg != null)
            {
                headingIndicatorImg.sprite = headingIndicatorSprite;
            }
        }

        /// <summary>
        /// Set the sprite that will mask the Display Heading scrollable sprite
        /// </summary>
        /// <param name="newSprite"></param>
        public void SetDisplayHeadingMaskSprite (Sprite newSprite)
        {
            headingMaskSprite = newSprite;

            if (headingMaskImg != null)
            {
                headingMaskImg.sprite = headingMaskSprite;
            }
        }

        /// <summary>
        /// Set the Display Heading small indicator colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the heading with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayHeadingIndicatorColour (Color newColour)
        {
            SSCUtils.UpdateColour(ref newColour, ref headingIndicatorColour, ref baseHeadingIndicatorColour, true);

            SetHeadingBrightness();
        }

        /// <summary>
        /// Set the Display Heading primary colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the heading with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayHeadingPrimaryColour (Color newColour)
        {
            SSCUtils.UpdateColour(ref newColour, ref headingPrimaryColour, ref baseHeadingPrimaryColour, true);

            SetHeadingBrightness();
        }

        /// <summary>
        /// Set the Display Heading scroll sprite.
        /// </summary>
        /// <param name="newSprite"></param>
        public void SetDisplayHeadingScrollSprite (Sprite newSprite)
        {
            headingScrollSprite = newSprite;

            if (headingScrollImg != null)
            {
                headingScrollImg.sprite = headingScrollSprite;
            }
        }

        /// <summary>
        /// Set the normalised size of the scrollable Display Heading.
        /// Currently height is always 1.0
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetDisplayHeadingSize (float width, float height)
        {
            // Verify the width,height values are within range 0.1 to 1.
            if (width >= 0.1f && width <= 1f)
            {
                headingWidth = width;
            }

            headingHeight = 1f;

            if (IsInitialised || editorMode)
            {
                #if UNITY_EDITOR
                if (editorMode)
                {
                    CheckHUDResize(false);
                    UnityEditor.Undo.RecordObject(headingRectTfrm.transform, string.Empty);
                }
                #endif

                // Here we use the original (reference) HUD size as it doesn't need to be scaled in any way.
                // The parent HUD canvas is correctly scaled and sized to the actual monitor or device display.
                headingRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, headingWidth * cvsResolutionFull.x);
                headingRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, headingHeight * cvsResolutionFull.y);
            }
        }

         /// <summary>
        /// Set the normalised offset (position) of the Display Heading on the HUD.
        /// If the module has been initialised, this will also re-position the Display Heading.
        /// </summary>
        /// <param name="offsetX">Horizontal offset from centre. Range between -1 and 1</param>
        /// <param name="offsetY">Vertical offset from centre. Range between -1 and 1</param>
        public void SetDisplayHeadingOffset (float offsetX, float offsetY)
        {
            // Verify the x,y values are within range -1 to 1.
            if (offsetX >= -1f && offsetX <= 1f)
            {
                headingOffsetX = offsetX;
            }

            if (offsetY >= -1f && offsetY <= 1f)
            {
                headingOffsetY = offsetY;
            }

            if (IsInitialised || editorMode)
            {
                Transform hdgTrfm = headingRectTfrm.transform;

                #if UNITY_EDITOR
                if (editorMode)
                {
                    CheckHUDResize(false);
                    UnityEditor.Undo.RecordObject(hdgTrfm, string.Empty);
                }
                #endif

                // Use the scaled HUD size. This works in and out of play mode
                tempHeadingOffset.x = headingOffsetX * cvsResolutionHalf.x;
                tempHeadingOffset.y = headingOffsetY * cvsResolutionHalf.y;
                tempHeadingOffset.z = hdgTrfm.localPosition.z;

                hdgTrfm.localPosition = tempHeadingOffset;

                // Reset the position of the scrollable image
                if (headingScrollPanel != null)
                {
                    headingScrollPanel.localPosition = Vector3.zero;
                    headingInitPosition = headingScrollPanel.transform.position;

                    if (showHeadingIndicator && headingIndicatorPanel != null)
                    {
                        headingIndicatorPanel.localPosition = new Vector3(headingScrollPanel.localPosition.x, headingScrollPanel.localPosition.y - 20f, headingScrollPanel.localPosition.z);
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to turn off the scrollable Heading
        /// </summary>
        public void HideHeading()
        {
            ShowOrHideHeading(false);
        }

        /// <summary>
        /// Attempt to turn off the Heading indicator
        /// </summary>
        public void HideHeadingIndicator()
        {
            ShowOrHideHeadingIndicator(false);
        }

        /// <summary>
        /// Attempt to show the scrollable Heading. Turn on HUD if required.
        /// </summary>
        public void ShowHeading()
        {
            ShowOrHideHeading(true);
        }

        /// <summary>
        /// Attempt to turn on the Heading indicator
        /// </summary>
        public void ShowHeadingIndicator()
        {
            ShowOrHideHeadingIndicator(true);
        }

        #endregion

        #region Public API Methods - Messages

        /// <summary>
        /// Add a new message to the HUD. By design, they are not visible at runtime when first added.
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="messageText"></param>
        /// <returns></returns>
        public DisplayMessage AddMessage(string messageName, string messageText)
        {
            DisplayMessage displayMessage = null;

            Canvas canvas = GetCanvas;
            hudRectTfrm = GetHUDPanel();

            if (canvas == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: ShipDisplayModule.AddMessage() - could not find canvas for the HUD. Did you use the prefab from Prefabs/Visuals folder?");
                #endif
            }
            else if (hudRectTfrm == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: ShipDisplayModule.AddMessage() - could not find HUDPanel for the HUD. Did you use the prefab from Prefabs/Visuals folder?");
                #endif
            }
            else
            {
                displayMessage = new DisplayMessage();
                if (displayMessage != null)
                {
                    displayMessage.messageName = messageName;
                    displayMessage.messageString = messageText;
                    ValidateMessageList();
                    displayMessageList.Add(displayMessage);

                    CreateMessagePanel(displayMessage, hudRectTfrm);

                    numDisplayMessages = displayMessageList.Count;
                 }
            }

            return displayMessage;
        }

        /// <summary>
        /// Add a message to the HUD using a displayMessage instance. Typically, this is used
        /// with CopyDisplayMessage(..).
        /// </summary>
        /// <param name="displayMessage"></param>
        public void AddMessage(DisplayMessage displayMessage)
        {
            if (displayMessage != null)
            {
                RectTransform hudRectTfrm = SSCUtils.GetChildRectTransform(transform, hudPanelName, this.name);

                if (hudRectTfrm == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: ShipDisplayModule.AddMessage() - could not find HUDPanel for the HUD. Did you use the prefab from Prefabs/Visuals folder?");
                    #endif
                }
                else
                {
                    ValidateMessageList();
                    displayMessageList.Add(displayMessage);

                    CreateMessagePanel(displayMessage, hudRectTfrm);

                    numDisplayMessages = displayMessageList.Count;
                }
            }
        }

        /// <summary>
        /// Delete a message from the HUD.
        /// NOTE: It is much cheaper to HideDisplayMessage(..) than completely remove it.
        /// </summary>
        /// <param name="guidHash"></param>
        public void DeleteMessage(int guidHash)
        {
            #if UNITY_EDITOR
            bool isRecordUndo = !Application.isPlaying;
            int undoGroup=0;
            #else
            bool isRecordUndo = false;
            #endif

            RectTransform msgRectTfrm = GetMessagePanel(guidHash, null);
            int _msgIndex = GetDisplayMessageIndex(guidHash);

            // Only record undo if in the editor AND is not playing AND something needs to be recorded
            isRecordUndo = isRecordUndo && (msgRectTfrm != null || _msgIndex >= 0);

            #if UNITY_EDITOR
            if (isRecordUndo)
            {
                UnityEditor.Undo.SetCurrentGroupName("Delete Display Message " + (_msgIndex + 1).ToString());
                undoGroup = UnityEditor.Undo.GetCurrentGroup();

                if (_msgIndex >= 0)
                {
                    UnityEditor.Undo.RecordObject(this, string.Empty);
                }
            }
            #endif

            // Check that the message exists and it is within the list constraints
            // NOTE: The script element must be modified BEFORE the Undo.DestroyObjectImmediate is called
            // in order for Undo to correctly undo both the message change AND the destroy of the panel.
            if (_msgIndex >= 0 && _msgIndex < (displayMessageList == null ? 0 : displayMessageList.Count))
            {
                displayMessageList.RemoveAt(_msgIndex);
                numDisplayMessages = displayMessageList.Count;
            }

            if (msgRectTfrm != null)
            {
                if (Application.isPlaying) { Destroy(msgRectTfrm.gameObject); }
                else
                {
                    #if UNITY_EDITOR
                    UnityEditor.Undo.DestroyObjectImmediate(msgRectTfrm.gameObject);
                    #else
                    DestroyImmediate(msgRectTfrm.gameObject);
                    #endif
                }
            }

            #if UNITY_EDITOR
            if (isRecordUndo)
            {               
                UnityEditor.Undo.CollapseUndoOperations(undoGroup);
            }
            #endif
        }

        /// <summary>
        /// Create a copy of an existing DisplayMessage, and give it a new name.
        /// Call AddMessage(newDisplayMessage) to make it useable in the game.
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="NameOfCopy"></param>
        /// <returns></returns>
        public DisplayMessage CopyDisplayMessage(DisplayMessage displayMessage, string NameOfCopy)
        {
            DisplayMessage dmCopy = new DisplayMessage(displayMessage);

            if (dmCopy != null)
            {
                // make it unique
                dmCopy.guidHash = SSCMath.GetHashCodeFromGuid();
                dmCopy.messageName = NameOfCopy;
            }
            return dmCopy;
        }

        /// <summary>
        /// Returns the guidHash of the Message in the list given the index or
        /// zero-based position in the list. Will return 0 if no matching Message
        /// is found.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetDisplayMessageGuidHash(int index)
        {
            int guidHash = 0;

            if (index >= 0 && index < GetNumberDisplayMessages)
            {
                if (displayMessageList[index] != null) { guidHash = displayMessageList[index].guidHash; }
            }

            return guidHash;
        }

        /// <summary>
        /// Get the zero-based index of the Message in the list. Will return -1 if not found.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public int GetDisplayMessageIndex(int guidHash)
        {
            int index = -1;

            int _numDisplayMessages = GetNumberDisplayMessages;

            for (int dmIdx = 0; dmIdx < _numDisplayMessages; dmIdx++)
            {
                if (displayMessageList[dmIdx] != null && displayMessageList[dmIdx].guidHash == guidHash) { index = dmIdx; break; }
            }

            return index;
        }

        /// <summary>
        /// Get a DisplayMessage given its guidHash.
        /// See also GetDisplayMessageGuidHash(..).
        /// Will return null if guidHash parameter is 0, it cannot be found.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public DisplayMessage GetDisplayMessage(int guidHash)
        {
            DisplayMessage displayMessage = null;
            DisplayMessage _tempDM = null;

            int _numDisplayMessages = GetNumberDisplayMessages;

            for (int dmIdx = 0; dmIdx < _numDisplayMessages; dmIdx++)
            {
                _tempDM = displayMessageList[dmIdx];
                if (_tempDM != null && _tempDM.guidHash == guidHash)
                {
                    displayMessage = _tempDM;
                    break;
                }
            }

            return displayMessage;
        }

        /// <summary>
        /// Get the display message give the description title of the message.
        /// WARNING: This will increase Garbage Collection (GC). Where possible
        /// use GetDisplayMessage(guidHash) and/or GetDisplayMessageGuidHash(index)
        /// </summary>
        /// <param name="displayMessageName"></param>
        /// <returns></returns>
        public DisplayMessage GetDisplayMessage(string displayMessageName)
        {
            DisplayMessage displayMessage = null;
            DisplayMessage _tempDM = null;

            if (!string.IsNullOrEmpty(displayMessageName))
            {
                int _numDisplayMessages = GetNumberDisplayMessages;

                for (int dmIdx = 0; dmIdx < _numDisplayMessages; dmIdx++)
                {
                    _tempDM = displayMessageList[dmIdx];
                    if (_tempDM != null && !string.IsNullOrEmpty(_tempDM.messageName))
                    {
                        if (_tempDM.messageName.ToLower() == displayMessageName.ToLower())
                        {
                            displayMessage = _tempDM;
                            break;
                        }
                    }
                }
            }
            return displayMessage;
        }

        /// <summary>
        /// Is the Display Message currently being shown on the HUD?
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <returns></returns>
        public bool IsDisplayMessageShown (DisplayMessage displayMessage)
        {
            if (displayMessage != null)
            {
                return IsHUDShown && displayMessage.showMessage;
            }
            else { return false; }
        }

        /// <summary>
        /// After adding or moving DisplayMessages, they may need to be sorted to
        /// have the correct z-order in on the HUD.
        /// </summary>
        public void RefreshMessagesSortOrder()
        {
            if (hudPanel != null)
            {
                // Messages should begin after index 1.
                int zIndex = 1;

                // Start messages after Altitude in hierarchy.
                if (overlayPanel != null) { zIndex = overlayPanel.GetSiblingIndex(); }

                int _numDisplayMessages = GetNumberDisplayMessages;

                for (int dmIdx = 0; dmIdx < _numDisplayMessages; dmIdx++)
                {
                    DisplayMessage displayMessage = displayMessageList[dmIdx];

                    if (displayMessage != null)
                    {
                        RectTransform _rt = displayMessage.CachedMessagePanel;

                        if (_rt != null)
                        {
                            _rt.SetSiblingIndex(++zIndex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Import a json file from disk and return as DisplayMessage
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public DisplayMessage ImportMessageFromJson (string folderPath, string fileName)
        {
            DisplayMessage displayMessage = null;

            if (!string.IsNullOrEmpty(folderPath) && !string.IsNullOrEmpty(fileName))
            {
                try
                {
                    string filePath = System.IO.Path.Combine(folderPath, fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        string jsonText = System.IO.File.ReadAllText(filePath);

                        displayMessage = new DisplayMessage();
                        int displayGaugeGuidHash = displayMessage.guidHash;

                        JsonUtility.FromJsonOverwrite(jsonText, displayMessage);

                        if (displayMessage != null)
                        {
                            // make hash code unique
                            displayMessage.guidHash = displayGaugeGuidHash;
                        }
                    }
                    #if UNITY_EDITOR
                    else
                    {
                        Debug.LogWarning("ERROR: Import Message. Could not find file at " + filePath);
                    }
                    #endif
                }
                catch (System.Exception ex)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: ShipDisplayModule - could not import message from: " + folderPath + " PLEASE REPORT - " + ex.Message);
                    #else
                    // Keep compiler happy
                    if (ex != null) { }
                    #endif
                }
            }

            return displayMessage;
        }

        /// <summary>
        /// Save the Message to a json file on disk.
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="filePath"></param>
        public bool SaveMessageAsJson (DisplayMessage displayMessage, string filePath)
        {
            bool isSuccessful = false;

            if (displayMessage != null && !string.IsNullOrEmpty(filePath))
            {
                try
                {
                    string jsonMessageData = JsonUtility.ToJson(displayMessage);

                    if (!string.IsNullOrEmpty(jsonMessageData) && !string.IsNullOrEmpty(filePath))
                    {
                        System.IO.File.WriteAllText(filePath, jsonMessageData);
                        isSuccessful = true;
                    }
                }
                catch (System.Exception ex)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: ShipDisplayModule SaveMessageAsJson - could not export: " + displayMessage.messageName + " PLEASE REPORT - " + ex.Message);
                    #else
                    // Keep compiler happy
                    if (ex != null) { }
                    #endif
                }
            }

            return isSuccessful;
        }

        /// <summary>
        /// Set the offset (position) of the Display Message on the HUD.
        /// If the module has been initialised, this will also re-position the Display Message.
        /// </summary>
        /// <param name="offsetX">Horizontal offset from centre. Range between -1 and 1</param>
        /// <param name="offsetY">Vertical offset from centre. Range between -1 and 1</param>
        public void SetDisplayMessageOffset(DisplayMessage displayMessage, float offsetX, float offsetY)
        {
            // Verify the x,y values are within range -1 to 1.
            if (offsetX >= -1f && offsetX <= 1f)
            {
                displayMessage.offsetX = offsetX;
            }

            if (offsetY >= -1f && offsetY <= 1f)
            {
                displayMessage.offsetY = offsetY;
            }

            if (IsInitialised || editorMode)
            {
                Transform dmTrfm = displayMessage.CachedMessagePanel.transform;

                #if UNITY_EDITOR
                if (editorMode)
                {
                    CheckHUDResize(false);
                    UnityEditor.Undo.RecordObject(dmTrfm, string.Empty);
                }
                #endif

                // Use original refResolution rather than the scaled size of the HUD. This works in and out of play mode
                //tempMessageOffset.x = displayMessage.offsetX * (refResolutionHalf.x - (displayMessage.displayWidth / 2f));
                //tempMessageOffset.y = displayMessage.offsetY * (refResolutionHalf.y - (displayMessage.displayHeight / 2f));

                // Use the scaled HUD size. This works in and out of play mode
                tempMessageOffset.x = displayMessage.offsetX * cvsResolutionHalf.x;
                tempMessageOffset.y = displayMessage.offsetY * cvsResolutionHalf.y;
                tempMessageOffset.z = dmTrfm.localPosition.z;

                dmTrfm.localPosition = tempMessageOffset;
            }
        }

        /// <summary>
        /// Set the size of the Message Panel.
        /// If the module has been initialised, this will also resize the Message Panel.
        /// The values are only updated if they are outside the range 0.0 to 1.0 or have changed.
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="width">Range between 0.0 and 1.0</param>
        /// <param name="height">Range between 0.0 and 1.0</param>
        public void SetDisplayMessageSize(DisplayMessage displayMessage, float width, float height)
        {
            // Clamp the x,y values 0.0 to 1.0
            if (width < 0f) { displayMessage.displayWidth = 0f; }
            else if (width > 1f) { displayMessage.displayWidth = 1f; }
            else if (width != displayMessage.displayWidth) { displayMessage.displayWidth = width; }

            if (height < 0f) { displayMessage.displayHeight = 0f; }
            else if (height > 1f) { displayMessage.displayHeight = 1f; }
            else if (height != displayMessage.displayHeight) { displayMessage.displayHeight = height; }

            if (IsInitialised || editorMode)
            {
                #if UNITY_EDITOR
                if (editorMode) { CheckHUDResize(false); }
                #endif

                RectTransform dmRectTfrm = displayMessage.CachedMessagePanel;    

                if (dmRectTfrm != null)
                {
                    // Here we use the original (reference) HUD size as it doesn't need to be scaled in any way.
                    // The parent HUD canvas is correctly scaled and sized to the actual monitor or device display.
                    dmRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, displayMessage.displayWidth * cvsResolutionFull.x);
                    dmRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, displayMessage.displayHeight * cvsResolutionFull.y);
                }

                if (displayMessage.CachedTextComponent != null)
                {
                    RectTransform dmTextRectTfrm = displayMessage.CachedTextComponent.rectTransform;

                    dmTextRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, displayMessage.displayWidth * cvsResolutionFull.x);
                    dmTextRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, displayMessage.displayHeight * cvsResolutionFull.y);
                }
            }
        }

        /// <summary>
        /// Update the text of the message
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="messageText"></param>
        public void SetDisplayMessageText(DisplayMessage displayMessage, string messageText)
        {
            if (displayMessage != null)
            {
                displayMessage.messageString = messageText;
                Text uiText = displayMessage.CachedTextComponent;

                if (uiText != null) { uiText.text = messageText; }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayMessageText - displayMessage is null"); }
            #endif
        }

        /// <summary>
        /// Update the position of the text within the message panel
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="textAlignment"></param>
        public void SetDisplayMessageTextAlignment(DisplayMessage displayMessage, TextAnchor textAlignment)
        {
            if (displayMessage != null)
            {
                if (!editorMode) { displayMessage.textAlignment = textAlignment; }

                Text uiText = displayMessage.CachedTextComponent;

                if (uiText != null) { uiText.alignment = textAlignment; }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayMessageTextAlignment - displayMessage is null"); }
            #endif
        }

        /// <summary>
        /// Set the font of the DisplayMessage Text component
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="font"></param>
        public void SetDisplayMessageTextFont(DisplayMessage displayMessage, Font font)
        {
            if (displayMessage != null)
            {
                Text uiText = displayMessage.CachedTextComponent;

                if (uiText != null)
                {
                    uiText.font = font;
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayMessageTextFont - displayMessage is null"); }
            #endif
        }

        /// <summary>
        /// Set the font size of the display message text. If isBestFit is false, maxSize is the font size set.
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="isBestFit"></param>
        /// <param name="minSize"></param>
        /// <param name="maxSize"></param>
        public void SetDisplayMessageTextFontSize(DisplayMessage displayMessage, bool isBestFit, int minSize, int maxSize)
        {
            if (displayMessage != null)
            {
                if (!editorMode)
                {
                    displayMessage.isBestFit = isBestFit;
                    displayMessage.fontMinSize = minSize;
                    displayMessage.fontMaxSize = maxSize;
                }

                Text uiText = displayMessage.CachedTextComponent;

                if (uiText != null)
                {
                    uiText.resizeTextForBestFit = isBestFit;
                    uiText.resizeTextMinSize = minSize;
                    uiText.resizeTextMaxSize = maxSize;
                    uiText.fontSize = maxSize;
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayMessageTextFontSize - displayMessage is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Message background colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the message background with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayMessageBackgroundColour(DisplayMessage displayMessage, Color newColour)
        {
            if (displayMessage != null)
            {
                SSCUtils.UpdateColour(ref newColour, ref displayMessage.backgroundColour, ref displayMessage.baseBackgroundColour, true);

                SetDisplayMessageBackgroundBrightness(displayMessage);
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayMessageBackgroundColour - displayMessage is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Message scroll direction.
        /// USAGE: SetDisplayMessageScrollDirection(displayMessage, DisplayMessage.ScrollDirectionLR)
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="scrollDirection"></param>
        public void SetDisplayMessageScrollDirection(DisplayMessage displayMessage, int scrollDirection)
        {
            if (displayMessage != null)
            {
                if (scrollDirection >= 0 && scrollDirection < 5)
                {
                    displayMessage.scrollDirectionInt = scrollDirection;
                    displayMessage.scrollDirection = (DisplayMessage.ScrollDirection)scrollDirection;
                    if (scrollDirection == DisplayMessage.ScrollDirectionNone)
                    {
                        displayMessage.scrollOffsetX = 0f;
                        displayMessage.scrollOffsetY = 0f;
                        SetDisplayMessageOffset(displayMessage, displayMessage.offsetX, displayMessage.offsetY);
                    }
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayMessageScrollDirection - displayMessage is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Message to scroll across or up/down the full screen regardless of the message width and height.
        /// Can also be set directly with displayMessage.isScrollFullscreen = true;
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="isScrollFullscreen"></param>
        public void SetDisplayMessageScrollFullscreen(DisplayMessage displayMessage, bool isScrollFullscreen)
        {
            if (displayMessage != null) { displayMessage.isScrollFullscreen = isScrollFullscreen; }
        }

        /// <summary>
        /// Set the Display Message scroll speed.
        /// Can also be set directly with displayMessage.scrollSpeed = scrollSpeed;
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="scrollSpeed"></param>
        public void SetDisplayMessageScrollSpeed(DisplayMessage displayMessage, float scrollSpeed)
        {
            if (displayMessage != null) { displayMessage.scrollSpeed = scrollSpeed; }
        }

        /// <summary>
        /// Set the Display Message text colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the message text with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayMessageTextColour(DisplayMessage displayMessage, Color newColour)
        {
            if (displayMessage != null)
            {
                SSCUtils.UpdateColour(ref newColour, ref displayMessage.textColour, ref displayMessage.baseTextColour, true);

                SetDisplayMessageTextBrightness(displayMessage);
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayMessageTextColour - displayMessage is null"); }
            #endif
        }

        /// <summary>
        /// Show the Display Message on the HUD. The HUD will automatically be shown if it is not already visible.
        /// </summary>
        /// <param name="guidHash"></param>
        public void ShowDisplayMessage (int guidHash)
        {
            DisplayMessage displayMessage = GetDisplayMessage(guidHash);
            if (displayMessage != null) { ShowOrHideMessage(displayMessage, true, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayMessage - Could not find message with guidHash: " + guidHash); }
            #endif
        }

        /// <summary>
        /// Show the Display Message on the HUD. The HUD will automatically be shown if it is not already visible.
        /// </summary>
        /// <param name="displayMessage"></param>
        public void ShowDisplayMessage (DisplayMessage displayMessage)
        {
            if (displayMessage != null) { ShowOrHideMessage(displayMessage, true, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayMessage - Could not find message - parameter was null"); }
            #endif
        }

        /// <summary>
        /// Show the Display Message on the HUD instantly by ignoring fade settings. The HUD will automatically be shown if it is not already visible.
        /// </summary>
        /// <param name="guidHash"></param>
        public void ShowDisplayMessageInstant (int guidHash)
        {
            DisplayMessage displayMessage = GetDisplayMessage(guidHash);
            if (displayMessage != null) { ShowOrHideMessage(displayMessage, true, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayMessageInstant - Could not find message with guidHash: " + guidHash); }
            #endif
        }

        /// <summary>
        /// Show the Display Message on the HUD instantly by ignoring fade settings. The HUD will automatically be shown if it is not already visible.
        /// </summary>
        /// <param name="displayMessage"></param>
        public void ShowDisplayMessageInstant (DisplayMessage displayMessage)
        {
            if (displayMessage != null) { ShowOrHideMessage(displayMessage, true, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayMessageInstant - Could not find message - parameter was null"); }
            #endif
        }

        /// <summary>
        /// Show or turn on all Display Messages. ShipDisplayModule must be initialised.
        /// The HUD will automatically be shown if it is not already visible.
        /// </summary>
        public void ShowDisplayMessages()
        {
            ShowOrHideMessages(true, false);
        }

        /// <summary>
        /// Show the Display Message background on the HUD. The display the actual message, you would
        /// need to call ShowDisplayMessage(..).
        /// </summary>
        /// <param name="displayMessage"></param>
        public void ShowDisplayMessageBackground(DisplayMessage displayMessage)
        {
            if (displayMessage != null) { ShowOrHideDisplayMessageBackground(displayMessage, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayMessageBackground - Could not find message - parameter was null"); }
            #endif
        }

        /// <summary>
        /// Hide or turn off the Display Message
        /// </summary>
        /// <param name="guidHash"></param>
        public void HideDisplayMessage(int guidHash)
        {
            DisplayMessage displayMessage = GetDisplayMessage(guidHash);
            if (displayMessage != null) { ShowOrHideMessage(displayMessage, false, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayMessage - Could not find message with guidHash: " + guidHash); }
            #endif
        }

        /// <summary>
        /// Hide or turn off the Display Message
        /// </summary>
        /// <param name="displayMessage"></param>
        public void HideDisplayMessage (DisplayMessage displayMessage)
        {
            if (displayMessage != null) { ShowOrHideMessage(displayMessage, false, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayMessage - Could not find message - parameter was null"); }
            #endif
        }

        /// <summary>
        /// Hide or turn off the Display Message instantly and ignore any fade-out settings.
        /// </summary>
        /// <param name="guidHash"></param>
        public void HideDisplayMessageInstant (int guidHash)
        {
            DisplayMessage displayMessage = GetDisplayMessage(guidHash);
            if (displayMessage != null) { ShowOrHideMessage(displayMessage, false, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayMessageInstant - Could not find message with guidHash: " + guidHash); }
            #endif
        }

        /// <summary>
        /// Hide or turn off the Display Message instantly and ignore any fade-out settings.
        /// </summary>
        /// <param name="displayMessage"></param>
        public void HideDisplayMessageInstant (DisplayMessage displayMessage)
        {
            if (displayMessage != null) { ShowOrHideMessage(displayMessage, false, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayMessageInstant - Could not find message - parameter was null"); }
            #endif
        }

        /// <summary>
        /// Hide or turn off all Display Messages.
        /// ShipDisplayModule must be initialised.
        /// </summary>
        public void HideDisplayMessages()
        {
            ShowOrHideMessages(false, false);
        }

        /// <summary>
        /// Hide the Display Message background on the HUD. The hide the actual message, you would
        /// need to call HideDisplayMessage(..).
        /// </summary>
        /// <param name="displayMessage"></param>
        public void HideDisplayMessageBackground(DisplayMessage displayMessage)
        {
            if (displayMessage != null) { ShowOrHideDisplayMessageBackground(displayMessage, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: HideDisplayMessageBackground - Could not find message - parameter was null"); }
            #endif
        }

        /// <summary>
        /// Create a new list if required
        /// </summary>
        public void ValidateMessageList()
        {
            if (displayMessageList == null) { displayMessageList = new List<DisplayMessage>(1); }
        }

        #endregion

        #region Public API Methods - Targets

        /// <summary>
        /// Add a new Display Target to the HUD. By design, they are not visible at runtime when first added.
        /// </summary>
        /// <param name="guidHashDisplayReticle"></param>
        /// <returns></returns>
        public DisplayTarget AddTarget(int guidHashDisplayReticle)
        {
            DisplayTarget displayTarget = null;

            if (GetHUDPanel() == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: ShipDisplayModule.AddTarget() - could not find HUDPanel for the HUD. Did you use the prefab from Prefabs/Visuals folder?");
                #endif
            }
            else
            {
                displayTarget = new DisplayTarget();
                if (displayTarget != null)
                {
                    displayTarget.guidHashDisplayReticle = guidHashDisplayReticle;

                    ValidateTargetList();
                    displayTargetList.Add(displayTarget);

                    numDisplayTargets = displayTargetList.Count;

                    RectTransform targetSlotPanel = CreateTargetPanel(displayTarget, 0);
                    if (targetSlotPanel != null)
                    {
                        #if UNITY_EDITOR
                        // Make sure we can rollback the creation of the new target panel for this slot
                        UnityEditor.Undo.RegisterCreatedObjectUndo(targetSlotPanel.gameObject, string.Empty);
                        #endif
                    }
                }
            }
            return displayTarget;
        }

        /// <summary>
        /// Add another DisplayTarget slot to a DisplayTarget. This allows you to display another
        /// copy of the target on the HUD.
        /// If the DisplayTarget has not been initialised, the new slot panel will be added to the
        /// scene but this method will return null.
        /// This automatically updates displayTarget.maxNumberOfTargets.
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <param name="numberToAdd"></param>
        /// <returns></returns>
        public DisplayTargetSlot AddTargetSlots(DisplayTarget displayTarget, int numberToAdd)
        {
            DisplayTargetSlot displayTargetSlot = null;

            if (displayTarget != null)
            {
                if (displayTarget.maxNumberOfTargets + numberToAdd <= DisplayTarget.MAX_DISPLAYTARGET_SLOTS)
                {
                    int numberAdded = 0;

                    // slotIndex is zero-based so start to 1 beyond the end of the existing list
                    for (int slotIndex = displayTarget.maxNumberOfTargets; slotIndex < displayTarget.maxNumberOfTargets + numberToAdd; slotIndex++)
                    {
                        RectTransform targetSlotPanel = CreateTargetPanel(displayTarget, slotIndex);

                        // If the DisplayTarget has already been initialised, this slot will need to be initialised
                        if (targetSlotPanel != null)
                        {
                            numberAdded++;

                            #if UNITY_EDITOR
                            // Make sure we can rollback the creation of the new target panel for this slot
                            UnityEditor.Undo.RegisterCreatedObjectUndo(targetSlotPanel.gameObject, string.Empty);
                            #endif

                            if (displayTarget.isInitialised)
                            {
                                displayTargetSlot = InitialiseTargetSlot(displayTarget, slotIndex);
                                displayTarget.displayTargetSlotList.Add(displayTargetSlot);

                                // Refresh the size of the slot reticle panel
                                #if UNITY_EDITOR
                                if (editorMode) { CheckHUDResize(false); }
                                #endif

                                RectTransform dtRectTfrm = displayTargetSlot.CachedTargetPanel;
                                if (dtRectTfrm != null)
                                {
                                    float pixelWidth = (float)displayTarget.width * screenResolution.x / refResolution.x;
                                    float pixelHeight = (float)displayTarget.height * screenResolution.y / refResolution.y;

                                    // Target size is in pixels - this will result in non-square scaling.
                                    // (Future) provide an option to keep aspect ratio of original 64x64 reticle
                                    dtRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixelWidth);
                                    dtRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixelHeight);
                                }

                                // Refresh brightness of this slot
                                if (displayTargetSlot.CachedImgComponent != null)
                                {
                                    if (brightness == 1f) { displayTargetSlot.CachedImgComponent.color = displayTarget.reticleColour; }
                                    else { displayTargetSlot.CachedImgComponent.color = displayTarget.baseReticleColour.GetColorWithBrightness(brightness); }
                                }
                            }
                        }
                    }
                    displayTarget.maxNumberOfTargets += numberAdded;
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("WARNING: shipDisplayModule.AddTargetSlot - not enough empty DisplayTarget slots"); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.AddTargetSlot - displayTarget is null"); }
            #endif

            return displayTargetSlot;
        }

        /// <summary>
        /// Delete a target from the HUD.
        /// NOTE: It is much cheaper to HideDisplayTarget(..) than completely remove it.
        /// </summary>
        /// <param name="guidHash"></param>
        public void DeleteTarget(int guidHash)
        {
            #if UNITY_EDITOR
            bool isRecordUndo = !Application.isPlaying;
            int undoGroup=0;
            #else
            bool isRecordUndo = false;
            #endif

            DisplayTarget displayTarget = GetDisplayTarget(guidHash);

            List<RectTransform> tgtRectTfrmList = null;

            // Get a list of DisplayTarget slot panels
            if (displayTarget != null)
            {
                tgtRectTfrmList = new List<RectTransform>(displayTarget.maxNumberOfTargets);
                for (int sIdx = 0; sIdx < displayTarget.maxNumberOfTargets; sIdx++)
                {
                    RectTransform tgtRectTfrm = GetTargetPanel(guidHash, sIdx);
                    if (tgtRectTfrm != null)
                    {
                        tgtRectTfrmList.Add(tgtRectTfrm);
                    }
                }
            }

            int _numPanels = tgtRectTfrmList == null ? 0 : tgtRectTfrmList.Count;
            int _tgtIndex = GetDisplayTargetIndex(guidHash);

            // Only record undo if in the editor AND is not playing AND something needs to be recorded
            isRecordUndo = isRecordUndo && (_numPanels > 0 || _tgtIndex >= 0);

            #if UNITY_EDITOR
            if (isRecordUndo)
            {
                UnityEditor.Undo.SetCurrentGroupName("Delete Display Target " + (_tgtIndex + 1).ToString());
                undoGroup = UnityEditor.Undo.GetCurrentGroup();

                if (_tgtIndex >= 0)
                {
                    UnityEditor.Undo.RecordObject(this, string.Empty);
                }
            }
            #endif

            // Check that the display target exists and it is within the list constraints
            // NOTE: The script element must be modified BEFORE the Undo.DestroyObjectImmediate is called
            // in order for Undo to correctly undo both the target change AND the destroy of the panels.
            if (_tgtIndex >= 0 && _tgtIndex < (displayTargetList == null ? 0 : displayTargetList.Count))
            {
                displayTargetList.RemoveAt(_tgtIndex);
                numDisplayTargets = displayTargetList.Count;
            }

            if (_numPanels > 0)
            {
                if (Application.isPlaying)
                {
                    // Destroy all the slot panels
                    for (int sIdx = _numPanels - 1; sIdx >= 0; sIdx--)
                    {
                        Destroy(tgtRectTfrmList[sIdx].gameObject);
                        tgtRectTfrmList.RemoveAt(sIdx);
                    }
                }
                else
                {
                    // Destroy all the slot panels
                    for (int sIdx = _numPanels - 1; sIdx >= 0; sIdx--)
                    {
                        #if UNITY_EDITOR
                        UnityEditor.Undo.DestroyObjectImmediate(tgtRectTfrmList[sIdx].gameObject);
                        #else
                        DestroyImmediate(tgtRectTfrmList[sIdx].gameObject);
                        #endif
                        tgtRectTfrmList.RemoveAt(sIdx);
                    }
                }
            }

            #if UNITY_EDITOR
            if (isRecordUndo)
            {               
                UnityEditor.Undo.CollapseUndoOperations(undoGroup);
            }
            #endif
        }

        /// <summary>
        /// Delete or remove a DisplayTarget slot from the HUD. This is an expensive operation.
        /// It is much cheaper to HideDisplayTargetSlot(..) than completely remove it.
        /// Automatically updates displayTarget.maxNumberOfTargets.
        /// NOTE: You cannot remove slot 0.
        /// </summary>
        /// <param name="guidHash">guidHash of the DisplayTarget</param>
        /// <returns></returns>
        public void DeleteTargetSlots(int guidHash, int numberToDelete)
        {
            DisplayTarget displayTarget = GetDisplayTarget(guidHash);

            if (displayTarget != null)
            {
                int remainingTargets = displayTarget.maxNumberOfTargets - numberToDelete;

                if (remainingTargets > 0)
                {
                    #if UNITY_EDITOR
                    bool isRecordUndo = !Application.isPlaying;
                    int undoGroup=0;
                    #else
                    bool isRecordUndo = false;
                    #endif

                    List<RectTransform> tgtRectTfrmList = null;

                    // Get a list of DisplayTarget slot panels to remove
                    if (displayTarget != null)
                    {
                        tgtRectTfrmList = new List<RectTransform>(displayTarget.maxNumberOfTargets);
                        for (int sIdx = remainingTargets; sIdx < displayTarget.maxNumberOfTargets; sIdx++)
                        {
                            RectTransform tgtRectTfrm = GetTargetPanel(guidHash, sIdx);
                            if (tgtRectTfrm != null)
                            {
                                tgtRectTfrmList.Add(tgtRectTfrm);
                            }
                        }
                    }

                    int _numPanels = tgtRectTfrmList == null ? 0 : tgtRectTfrmList.Count;
                    int _tgtIndex = GetDisplayTargetIndex(guidHash);

                    // Only record undo if in the editor AND is not playing AND something needs to be recorded
                    isRecordUndo = isRecordUndo && (_numPanels > 0 || _tgtIndex >= 0);

                    #if UNITY_EDITOR
                    if (isRecordUndo)
                    {
                        UnityEditor.Undo.SetCurrentGroupName("Delete Display Target " + (_tgtIndex + 1).ToString() + " slots");
                        undoGroup = UnityEditor.Undo.GetCurrentGroup();

                        if (_tgtIndex >= 0)
                        {
                            UnityEditor.Undo.RecordObject(this, string.Empty);
                        }
                    }
                    #endif

                    // NOTE: The script element must be modified BEFORE the Undo.DestroyObjectImmediate is called
                    // in order for Undo to correctly undo both the target change AND the destroy of the panels.
                    if (displayTarget.isInitialised)
                    {
                        for (int sIdx = displayTarget.maxNumberOfTargets - 1; sIdx >= remainingTargets; sIdx--)
                        {
                            if (sIdx < displayTarget.displayTargetSlotList.Count)
                            {
                                displayTarget.displayTargetSlotList[sIdx] = null;
                                displayTarget.displayTargetSlotList.RemoveAt(sIdx);
                            }
                            #if UNITY_EDITOR
                            else { Debug.LogWarning("ERROR: shipDisplayModule.DeleteTargetSlot - tried to remove initialised slot " + sIdx + " but only " + displayTarget.displayTargetSlotList.Count + " exist."); }
                            #endif
                        }
                    }

                    displayTarget.maxNumberOfTargets = remainingTargets;

                    if (_numPanels > 0)
                    {
                        if (Application.isPlaying)
                        {
                            // Destroy the slot panels
                            for (int sIdx = _numPanels - 1; sIdx >= 0; sIdx--)
                            {
                                Destroy(tgtRectTfrmList[sIdx].gameObject);
                                tgtRectTfrmList.RemoveAt(sIdx);
                            }
                        }
                        else
                        {
                            // Destroy the slot panels
                            for (int sIdx = _numPanels - 1; sIdx >= 0; sIdx--)
                            {
                                #if UNITY_EDITOR
                                UnityEditor.Undo.DestroyObjectImmediate(tgtRectTfrmList[sIdx].gameObject);
                                #else
                                DestroyImmediate(tgtRectTfrmList[sIdx].gameObject);
                                #endif
                                tgtRectTfrmList.RemoveAt(sIdx);
                            }
                        }
                    }

                    #if UNITY_EDITOR
                    if (isRecordUndo)
                    {               
                        UnityEditor.Undo.CollapseUndoOperations(undoGroup);
                    }
                    #endif

                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("WARNING: shipDisplayModule.DeleteTargetSlot - cannot delete that many DisplayTarget slots"); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.DeleteTargetSlot - displayTarget cannot be found with guidHash: " + guidHash); }
            #endif
        }

        /// <summary>
        /// Assign a radar target to a DisplayTarget's slot.
        /// See also GetAssignedDisplayTarget(..)
        /// </summary>
        /// <param name="displayTargetIndex"></param>
        /// <param name="instanceIndex"></param>
        /// <param name="radarItemIndex"></param>
        /// <param name="radarItemSequenceNumber"></param>
        public void AssignDisplayTargetSlot(int displayTargetIndex, int slotIndex, int radarItemIndex, uint radarItemSequenceNumber, bool isAutoShow)
        {
            if (displayTargetIndex >= 0 && displayTargetIndex < GetNumberDisplayTargets)
            {
                DisplayTarget displayTarget = displayTargetList[displayTargetIndex];
                if (displayTarget != null && slotIndex >= 0 && slotIndex < displayTarget.maxNumberOfTargets)
                {
                    DisplayTargetSlot displayTargetSlot = displayTarget.displayTargetSlotList[slotIndex];

                    displayTargetSlot.radarItemKey.radarItemIndex = radarItemIndex;
                    displayTargetSlot.radarItemKey.radarItemSequenceNumber = radarItemSequenceNumber;

                    if (isAutoShow)
                    {
                        // Should the DisplayTarget be shown?
                        if (radarItemIndex >= 0)
                        {
                            // If not already shown, turn it on.
                            if (!displayTargetSlot.showTargetSlot)
                            {
                                ShowOrHideTargetSlot(displayTargetSlot, true);
                                // If one slot is on, mark the displayTarget as shown also.
                                displayTarget.showTarget = true;
                            }
                        }
                        // If should not be shown, but currently is, turn it off
                        else if (displayTargetSlot.showTargetSlot) { ShowOrHideTargetSlot(displayTargetSlot, false); }
                    }
                }
            }
        }

        /// <summary>
        /// Get the radarItemKey for a DisplayTarget's slot.
        /// SSCRadarItemKey.radarItemIndex will be -1 if there is no target.
        /// </summary>
        /// <param name="displayTargetIndex"></param>
        /// <param name="slotIndex"></param>
        /// <returns></returns>
        public SSCRadarItemKey GetAssignedDisplayTarget(int displayTargetIndex, int slotIndex)
        {
            if (displayTargetIndex >= 0 && displayTargetIndex < GetNumberDisplayTargets)
            {
                DisplayTarget displayTarget = displayTargetList[displayTargetIndex];
                if (displayTarget != null && slotIndex >= 0 && slotIndex < displayTarget.maxNumberOfTargets)
                {
                    return displayTarget.displayTargetSlotList[slotIndex].radarItemKey;
                }
                else
                {
                    return new SSCRadarItemKey() { radarItemIndex = -1, radarItemSequenceNumber = 0 };
                }
            }
            else { return new SSCRadarItemKey() { radarItemIndex = -1, radarItemSequenceNumber = 0 }; }
        }

        /// <summary>
        /// Get the SSCRadarItem currently assigned to a DisplayTarget slot (instance).
        /// </summary>
        /// <param name="displayTargetSlot"></param>
        /// <returns></returns>
        public SSCRadarItem GetAssignedDisplayTargetRadarItem (DisplayTargetSlot displayTargetSlot)
        {
            if (displayTargetSlot != null && displayTargetSlot.radarItemKey.radarItemIndex >= 0 && sscRadar != null)
            {
                return sscRadar.GetRadarItem(displayTargetSlot.radarItemKey.radarItemIndex);
            }
            else { return null; }
        }

        /// <summary>
        /// Returns the guidHash of the Target in the list given the index or
        /// zero-based position in the list. Will return 0 if no matching Target
        /// is found.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetDisplayTargetGuidHash(int index)
        {
            int guidHash = 0;

            if (index >= 0 && index < GetNumberDisplayTargets)
            {
                if (displayTargetList[index] != null) { guidHash = displayTargetList[index].guidHash; }
            }

            return guidHash;
        }

        /// <summary>
        /// Get the zero-based index of the Target in the list. Will return -1 if not found.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public int GetDisplayTargetIndex(int guidHash)
        {
            int index = -1;

            int _numDisplayTargets = GetNumberDisplayTargets;

            for (int dmIdx = 0; dmIdx < _numDisplayTargets; dmIdx++)
            {
                if (displayTargetList[dmIdx] != null && displayTargetList[dmIdx].guidHash == guidHash) { index = dmIdx; break; }
            }

            return index;
        }

        /// <summary>
        /// Get a DisplayTarget given its guidHash.
        /// See also GetDisplayTargetGuidHash(..).
        /// Will return null if guidHash parameter is 0, it cannot be found.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public DisplayTarget GetDisplayTarget(int guidHash)
        {
            DisplayTarget displayTarget = null;
            DisplayTarget _tempDM = null;

            int _numDisplayTargets = GetNumberDisplayTargets;

            for (int dmIdx = 0; dmIdx < _numDisplayTargets; dmIdx++)
            {
                _tempDM = displayTargetList[dmIdx];
                if (_tempDM != null && _tempDM.guidHash == guidHash)
                {
                    displayTarget = _tempDM;
                    break;
                }
            }

            return displayTarget;
        }

        /// <summary>
        /// Get a DisplayTarget given a zero-based index in the list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DisplayTarget GetDisplayTargetByIndex(int index)
        {
            if (index >= 0 && index < GetNumberDisplayTargets)
            {
                return displayTargetList[index];
            }
            else { return null; }
        }

        /// <summary>
        /// Get the (sprite) name of the DisplayTarget.
        /// WARNING: This will create GC, so not recommended to be called each frame.
        /// This is typically used for debugging purposes only.
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <returns></returns>
        public string GetDisplayTargetName(DisplayTarget displayTarget)
        {
            if (displayTarget != null && displayReticleList != null)
            {
                int _selectedIdx = displayReticleList.FindIndex(dr => dr.guidHash == displayTarget.guidHashDisplayReticle);

                if (_selectedIdx >= 0)
                {
                    string tempDisplayReticleName = displayReticleList[_selectedIdx].primarySprite == null ? "no texture in reticle" : displayReticleList[_selectedIdx].primarySprite.name;
                    return string.IsNullOrEmpty(tempDisplayReticleName) ? "no texture name" : tempDisplayReticleName;
                }
                else { return "--"; }
            }
            else { return "--"; }
        }

        /// <summary>
        /// Get the (sprite) name of the DisplayTarget.
        /// WARNING: This will create GC, so not recommended to be called each frame.
        /// This is typically used for debugging purposes only.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetDisplayTargetName(int index)
        {
            return GetDisplayTargetName(GetDisplayTargetByIndex(index));            
        }

        /// <summary>
        /// Get a DisplayTargetSlot given the zero-based slot index. This is an instance of a DisplayTarget.
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <param name="slotIndex"></param>
        /// <returns></returns>
        public DisplayTargetSlot GetDisplayTargetSlotByIndex (DisplayTarget displayTarget, int slotIndex)
        {
            if (IsInitialised && displayTarget != null && slotIndex > -1 && slotIndex < displayTarget.maxNumberOfTargets)
            {
                return displayTarget.displayTargetSlotList[slotIndex];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Show the Display Target slots on the HUD. By design, if the HUD is not shown, the Targets will not be show.
        /// </summary>
        /// <param name="guidHash"></param>
        public void ShowDisplayTarget(int guidHash)
        {
            DisplayTarget displayTarget = GetDisplayTarget(guidHash);
            if (displayTarget != null) { ShowOrHideTarget(displayTarget, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayTarget - Could not find displayTarget with guidHash: " + guidHash); }
            #endif
        }

        /// <summary>
        /// Show the Display Target slots on the HUD. By design, if the HUD is not shown, the Targets will not be show.
        /// </summary>
        /// <param name="displayTarget"></param>
        public void ShowDisplayTarget(DisplayTarget displayTarget)
        {
            if (displayTarget != null) { ShowOrHideTarget(displayTarget, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayTarget - displayTarget parameter was null"); }
            #endif
        }

        /// <summary>
        /// Show the Display Target slot on the HUD. By design, if the HUD is not shown, the Target in this slot will not be show.
        /// </summary>
        /// <param name="displayTargetSlot"></param>
        public void ShowDisplayTargetSlot(DisplayTargetSlot displayTargetSlot)
        {
            if (displayTargetSlot != null) { ShowOrHideTargetSlot(displayTargetSlot, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayTargetSlot - displayTargetSlot parameter was null"); }
            #endif
        }

        /// <summary>
        /// Show or turn on all Display Targets.
        /// ShipDisplayModule must be initialised.
        /// </summary>
        public void ShowDisplayTargets()
        {
            ShowOrHideTargets(true);
        }

        /// <summary>
        /// Hide or turn off all slots the Display Target
        /// </summary>
        /// <param name="guidHash"></param>
        public void HideDisplayTarget(int guidHash)
        {
            DisplayTarget displayTarget = GetDisplayTarget(guidHash);
            if (displayTarget != null) { ShowOrHideTarget(displayTarget, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayTarget - Could not find message with guidHash: " + guidHash); }
            #endif
        }

        /// <summary>
        /// Hide or turn off all slots of the Display Target
        /// </summary>
        /// <param name="displayTarget"></param>
        public void HideDisplayTarget(DisplayTarget displayTarget)
        {
            if (displayTarget != null) { ShowOrHideTarget(displayTarget, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayTarget - Could not find message - parameter was null"); }
            #endif
        }

        /// <summary>
        /// Hide the Display Target slot on the HUD. By design, if the HUD is not shown, the Target in this slot will not be show.
        /// </summary>
        /// <param name="displayTargetSlot"></param>
        public void HideDisplayTargetSlot(DisplayTargetSlot displayTargetSlot)
        {
            if (displayTargetSlot != null) { ShowOrHideTargetSlot(displayTargetSlot, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: HideDisplayTargetSlot - displayTargetSlot parameter was null"); }
            #endif
        }

        /// <summary>
        /// Hide or turn off all all slots of all Display Targets.
        /// ShipDisplayModule must be initialised.
        /// </summary>
        public void HideDisplayTargets()
        {
            ShowOrHideTargets(false);
        }

        /// <summary>
        /// Given a radar query, add rules based on the current DisplayTargets.
        /// TODO - WARNING - NEED TO PERFORMANCE TEST THIS
        /// </summary>
        public void PopulateTargetingRadarQuery(SSCRadarQuery sscRadarQuery)
        {
            // Build a common list of factions and squadrons to include
            if (IsInitialised)
            {
                int _numDisplayTargets = GetNumberDisplayTargets;

                if (_numDisplayTargets > 0)
                {
                    tempIncludeFactionList.Clear();
                    tempIncludeSquadronList.Clear();

                    for (int dtIdx = 0; dtIdx < _numDisplayTargets; dtIdx++)
                    {
                        DisplayTarget _tempDT = displayTargetList[dtIdx];

                        if (_tempDT != null)
                        {
                            int _numFactionsToInclude = _tempDT.factionsToInclude == null ? 0 : _tempDT.factionsToInclude.Length;
                            int _numSquadronsToInclude = _tempDT.squadronsToInclude == null ? 0 : _tempDT.squadronsToInclude.Length;

                            // By default there is no exclude
                            sscRadarQuery.factionsToExclude = null;
                            sscRadarQuery.squadronsToExclude = null;

                            // Determine which Factions (if any) to include as a filter
                            for (int fIdx = 0; fIdx < _numFactionsToInclude; fIdx++)
                            {
                                // If faction is not already in list add it.
                                int _factionId = _tempDT.factionsToInclude[fIdx];
                                if (tempIncludeFactionList.FindIndex(f => f == _factionId) < 0)
                                {
                                    tempIncludeFactionList.Add(_factionId);
                                }
                            }

                            // Determine which Squadrons (if any) to include as a filter
                            for (int sqIdx = 0; sqIdx < _numSquadronsToInclude; sqIdx++)
                            {
                                // If squadron is not already in list add it.
                                int _squadronId = _tempDT.squadronsToInclude[sqIdx];
                                if (tempIncludeSquadronList.FindIndex(sq => sq == _squadronId) < 0)
                                {
                                    tempIncludeSquadronList.Add(_squadronId);
                                }
                            }

                            if (tempIncludeFactionList.Count > 0)
                            {
                                // Check if this generates work for GC
                                sscRadarQuery.factionsToInclude = tempIncludeFactionList.ToArray();
                            }
                            else { sscRadarQuery.factionsToInclude = null; }

                            if (tempIncludeSquadronList.Count > 0)
                            {
                                // Check if this generates work for GC
                                sscRadarQuery.squadronsToInclude = tempIncludeSquadronList.ToArray();
                            }
                            else { sscRadarQuery.squadronsToInclude = null; }
                        }
                    }
                }

                sscRadarQuery.range = targetingRange;
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: PopulateTargetingRadarQuery - shipDisplayModule is not initialised"); }
            #endif
        }

        /// <summary>
        /// Set or change the Reticle assigned to a DisplayTarget.
        /// The Reticle must belong to the list of available reticles for the HUD.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <param name="guidHashDisplayReticle"></param>
        public void SetDisplayTargetReticle(int guidHash, int guidHashDisplayReticle)
        {
            if (IsInitialised || editorMode)
            {
                DisplayTarget displayTarget = GetDisplayTarget(guidHash);

                if (displayTarget != null)
                {
                    displayTarget.guidHashDisplayReticle = guidHashDisplayReticle;

                    DisplayReticle _displayReticle = GetDisplayReticle(guidHashDisplayReticle);

                    // Set the reticle for all slots of this DisplayTarget
                    for (int sIdx = 0; sIdx < displayTarget.maxNumberOfTargets; sIdx++)
                    {
                        if (displayTarget.displayTargetSlotList[sIdx].CachedImgComponent != null)
                        {
                            displayTarget.displayTargetSlotList[sIdx].CachedImgComponent.sprite = _displayReticle == null ? null : _displayReticle.primarySprite;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set the Display Target Reticle colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the reticle with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayTargetReticleColour(DisplayTarget displayTarget, Color newColour)
        {
            if (displayTarget != null)
            {
                SSCUtils.UpdateColour(ref newColour, ref displayTarget.reticleColour, ref displayTarget.baseReticleColour, true);

                SetDisplayTargetBrightness(displayTarget);
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayTargetReticleColour - displayTarget is null"); }
            #endif
        }

        /// <summary>
        /// Set the offset (position) of the Display Target slot on the HUD.
        /// If the module has been initialised, this will also re-position the Display Target.
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <param name="slotIndex"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public void SetDisplayTargetOffset(DisplayTarget displayTarget, int slotIndex, float offsetX, float offsetY)
        {
            if (displayTarget != null)
            {
                int numSlots = displayTarget.displayTargetSlotList == null ? 0 : displayTarget.displayTargetSlotList.Count;

                if (slotIndex >= 0 & slotIndex < numSlots)
                {
                    SetDisplayTargetOffset(displayTarget.displayTargetSlotList[slotIndex], offsetX, offsetY);
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayTargetOffset - the displayTarget slot is out-of-range (" + slotIndex + ")"); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayTargetOffset - displayTarget is null"); }
            #endif
        }

        /// <summary>
        /// Set the offset (position) of the Display Target slot on the HUD.
        /// If the module has been initialised, this will also re-position the Display Target.
        /// </summary>
        /// <param name="displayTargetSlot"></param>
        /// <param name="offsetX">Horizontal offset from centre. Range between -1 and 1</param>
        /// <param name="offsetY">Vertical offset from centre. Range between -1 and 1</param>
        public void SetDisplayTargetOffset(DisplayTargetSlot displayTargetSlot, float offsetX, float offsetY)
        {
            if (displayTargetSlot != null)
            {
                // Verify the x,y values are within range -1 to 1.
                if (offsetX < -1f) { displayTargetSlot.offsetX = -1f; }
                else if (offsetX > 1f) { displayTargetSlot.offsetX = 1f; }
                else
                {
                    displayTargetSlot.offsetX = offsetX;
                }

                if (offsetY < -1f) { displayTargetSlot.offsetY = -1f; }
                else if (offsetY > 1f) { displayTargetSlot.offsetY = 1f; }
                else
                {
                    displayTargetSlot.offsetY = offsetY;
                }

                if (IsInitialised || editorMode)
                {
                    Transform dtgtTrfm = displayTargetSlot.CachedTargetPanel.transform;

                    #if UNITY_EDITOR
                    if (editorMode)
                    {
                        CheckHUDResize(false);
                        UnityEditor.Undo.RecordObject(dtgtTrfm, string.Empty);
                    }
                    #endif

                    // Use the scaled HUD size. This works in and out of play mode
                    tempTargetOffset.x = displayTargetSlot.offsetX * cvsResolutionHalf.x;
                    tempTargetOffset.y = displayTargetSlot.offsetY * cvsResolutionHalf.y;
                    tempTargetOffset.z = dtgtTrfm.localPosition.z;

                    dtgtTrfm.localPosition = tempTargetOffset;
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayTargetOffset - displayTargetSlot is null"); }
            #endif
        }

        /// <summary>
        /// Move the DisplayTarget Slot to the correct 2D position on the HUD, based on a 3D world space position.
        /// If the camera has not been automatically or manually assigned, the DisplayTarget will not be moved.
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <param name="slotIndex"></param>
        /// <param name="wsPosition"></param>
        public void SetDisplayTargetPosition(DisplayTarget displayTarget, int slotIndex, Vector3 wsPosition)
        {
            if (isMainCameraAssigned)
            {
                if (displayTarget != null)
                {
                    Vector3 screenPosition = mainCamera.WorldToScreenPoint(wsPosition);

                    // NOTE: cvsResolutionFull.x / cvsScaleFactor.x is NOT the same as Screen.width
                    // x 2 and subtracting 1 moves it into our HUD -1 to 1 space.
                    SetDisplayTargetOffset(displayTarget, slotIndex, 2f * screenPosition.x / cvsResolutionFull.x / cvsScaleFactor.x - 1f, 2f * screenPosition.y / cvsResolutionFull.y / cvsScaleFactor.y - 1f);
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ERROR: shipDisplayModule.SetDisplayTargetPosition - displayTarget is null"); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: ShipDisplayModule.SetDisplayTargetPosition - Could not find mainCamera."); }
            #endif
        }

        /// <summary>
        /// Set the size of the Target Reticle in each slot.
        /// If the module has been initialised, this will also resize the Target Reticle.
        /// The values are only updated if they are outside the range 8 to 256 or have changed.
        /// The size is based on a screen resolution of 1920x1080
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <param name="width">Range between 8 and 256</param>
        /// <param name="height">Range between 8 and 256</param>
        public void SetDisplayTargetSize(DisplayTarget displayTarget, int width, int height)
        {
            // Clamp the x,y values 8 to 256
            if (width < 8) { displayTarget.width = 8; }
            else if (width > 256) { displayTarget.width = 256; }
            else if (width != displayTarget.width) { displayTarget.width = width; }

            if (height < 8) { displayTarget.height = 8; }
            else if (height > 256) { displayTarget.height = 256; }
            else if (height != displayTarget.height) { displayTarget.height = height; }

            if (IsInitialised || editorMode)
            {
                #if UNITY_EDITOR
                if (editorMode) { CheckHUDResize(false); }
                #endif

                float pixelWidth = (float)displayTarget.width * screenResolution.x / refResolution.x;
                float pixelHeight = (float)displayTarget.height * screenResolution.y / refResolution.y;

                // Set the size of all the reticles in the DisplayTarget slots
                // There can be multiple copies or slots of the same DisplayTarget.
                for (int sIdx = 0; sIdx < displayTarget.maxNumberOfTargets; sIdx++)
                {
                    RectTransform dtRectTfrm = displayTarget.displayTargetSlotList[sIdx].CachedTargetPanel;

                    if (dtRectTfrm != null)
                    {
                        // Target size is in pixels - this will result in non-square scaling.
                        // (Future) provide an option to keep aspect ratio of original 64x64 reticle
                        dtRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixelWidth);
                        dtRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixelHeight);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the clipped viewable area of the screen that DisplayTargets can be shown. When Targets
        /// are outside this area, they will be hidden.
        /// See also SetTargetsViewportOffset(..).
        /// </summary>
        /// <param name="width">Range between 0.1 and 1.0</param>
        /// <param name="height">Range between 0.1 and 1.0</param>
        public void SetTargetsViewportSize(float width, float height)
        {
            // Currently we just set the values after basic validation. This may change in future versions

            // Perform basic validation
            if (width >= 0.1f && width <= 1.0f) { targetsViewWidth = width; }
            if (height >= 0.1f && height <= 1.0f) { targetsViewHeight = height; }
        }

        /// <summary>
        /// Sets the Targets viewport offset from the centre of the screen.
        /// Values can be between -1.0 and 1.0 with 0,0 depicting the centre of the screen.
        /// See also SetTargetsViewportSize(..).
        /// </summary>
        /// <param name="offsetX">Range between -1.0 and 1.0</param>
        /// <param name="offsetY">Range between -1.0 and 1.0</param>
        public void SetTargetsViewportOffset(float offsetX, float offsetY)
        {
            // Currently we just set the values after basic validation. This may change in future versions

            if (offsetX >= -1f && offsetX <= 1.0f) { targetsViewOffsetX = offsetX; }
            if (offsetY >= -1f && offsetY <= 1.0f) { targetsViewOffsetY = offsetY; }
        }

        /// <summary>
        /// After adding or moving DisplayTargets, they may need to be sorted to
        /// have the correct z-order in on the HUD.
        /// </summary>
        public void RefreshTargetsSortOrder()
        {
            if (targetsRectTfrm != null)
            {
                // Targets should begin at index 0.
                int zIndex = -1;

                // Update number of DisplayTargets. This can help issues in the Editor, like moving DislayTargets
                // up or down in the list while in play mode.
                numDisplayTargets = displayTargetList == null ? 0 : displayTargetList.Count;

                for (int dtIdx = 0; dtIdx < numDisplayTargets; dtIdx++)
                {
                    DisplayTarget displayTarget = displayTargetList[dtIdx];

                    if (displayTarget != null)
                    {
                        //Debug.Log("[DEBUG] dt: " + dtIdx + " maxNum: " + displayTarget.maxNumberOfTargets + " actual: " + displayTarget.displayTargetSlotList.Count);

                        // Loop through all the slots for this DisplayTarget
                        for (int sIdx = 0; sIdx < displayTarget.maxNumberOfTargets; sIdx++)
                        {
                            RectTransform _rt = displayTarget.displayTargetSlotList[sIdx].CachedTargetPanel;

                            if (_rt != null)
                            {
                                _rt.SetSiblingIndex(++zIndex);
                            }
                        }
                    }
                }
            }
        }

         /// <summary>
        /// Create a new list if required
        /// </summary>
        public void ValidateTargetList()
        {
            if (displayTargetList == null) { displayTargetList = new List<DisplayTarget>(1); }
        }

        #endregion
    }
}
