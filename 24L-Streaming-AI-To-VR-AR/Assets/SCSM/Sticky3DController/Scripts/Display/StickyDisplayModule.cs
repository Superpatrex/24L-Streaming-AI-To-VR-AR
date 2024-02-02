using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This module enables you to show information for the player.
    /// Setup Notes:
    /// DisplayPanel should be anchored at four corners of screen (stretched)
    /// Display Reticle panel should be anchored at the centre of display
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Utilities/Sticky Display Module")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyDisplayModule : MonoBehaviour
    {
        #region Enumerations

        #endregion

        #region Public Static Variables

        [System.NonSerialized] private static StickyDisplayModule activeDisplayModule = null;

        public readonly static string displayPanelName = "DisplayPanel";
        public readonly static string gaugesPanelName = "GaugesPanel";
        public readonly static string overlayPanelName = "OverlayPanel";
        public readonly static string targetsPanelName = "TargetsPanel";
        public readonly static string reticlePanelName = "DisplayReticlePanel";
        #endregion

        #region Public Variables and Properties - General

        /// <summary>
        /// If enabled, the Initialise() will be called as soon as Start() runs. This should be disabled if you are
        /// instantiating the display through code.
        /// </summary>
        public bool initialiseOnStart = false;

        /// <summary>
        /// Show or Hide the Display when it is first Initialised
        /// </summary>
        public bool isShowOnInitialise = true;

        /// <summary>
        /// Show overlay image on Display. At runtime call ShowOverlay() or HideOverlay()
        /// </summary>
        public bool showOverlay = false;

        /// <summary>
        /// The display's normalised width of the screen. 1.0 is full width, 0.5 is half width.
        /// At runtime call stickyDisplayModule.SetDisplaySize(..)
        /// </summary>
        [Range(0.1f, 1f)] public float displayWidth = 0.5f;

        /// <summary>
        /// The display's normalised height of the screen. 1.0 is full height, 0.5 is half height.
        /// At runtime call stickyDisplayModule.SetDisplaySize(..)
        /// </summary>
        [Range(0.1f, 1f)] public float displayHeight = 0.75f;

        /// <summary>
        /// The display's normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.
        /// At runtime call stickyDisplayModule.SetDisplayOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float displayOffsetX = 0f;

        /// <summary>
        /// The display's normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.
        /// At runtime call stickyDisplayModule.SetDisplayOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float displayOffsetY = 0f;

        /// <summary>
        /// Primary colour of the display.
        /// At runtime call stickyDisplayModule.SetPrimaryColour(..)
        /// </summary>
        public Color32 primaryColour = Color.grey;

        /// <summary>
        /// Automatically hide the screen cursor or mouse pointer after it has been stationary
        /// for a fixed period of time. Automatically show the cursor if the mouse if moved
        /// provided that the S3D Display Reticle is on shown.
        /// </summary>
        public bool autoHideCursor = true;

        /// <summary>
        /// The number of seconds to wait until after the cursor has not moved before hiding it
        /// </summary>
        public float hideCursorTime = 3f;

        /// <summary>
        /// The overall brightness of the Display. At runtime use
        /// SetBrightness(value)
        /// </summary>
        [Range(0f, 1f)] public float brightness = 1f;

        /// <summary>
        /// The sort order of the canvas in the scene. Higher numbers are on top.
        /// At runtime call stickyDisplayModule.SetCanvasSortOrder(..)
        /// </summary>
        public int canvasSortOrder = 2;

        #endregion

        #region Public Variables and Properties - Reticles

        /// <summary>
        /// The guidHash of the currently selected / displayed reticle
        /// </summary>
        public int guidHashActiveDisplayReticle = 0;

        /// <summary>
        /// The list of display reticles that can be used with
        /// this Sticky Display. Call ReinitialiseVariables() if
        /// you modify the list at runtime.
        /// </summary>
        public List<S3DDisplayReticle> displayReticleList;

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
        /// Get the Display Reticle position in screen-space. x = 0.0 to screen.width, y = 0.0 to screen.height. 0,0 is bottom left corner.
        /// </summary>
        public Vector2 DisplayReticleScreenPoint { get { return IsInitialised ? new Vector2((displayReticleOffsetX + 1f) * 0.5f * screenResolution.x, (displayReticleOffsetY + 1f) * 0.5f * screenResolution.y) : new Vector2((displayReticleOffsetX + 1f) * 0.5f * Screen.width, (displayReticleOffsetY + 1f) * 0.5f * Screen.height); } }

        /// <summary>
        /// Get the Display Reticle position on the screen in Viewport coordinates. x = 0.0-1.0, y = 0.0-1.0. 0,0 is bottom left corner.
        /// </summary>
        public Vector2 DisplayReticleViewportPoint { get { return new Vector2((displayReticleOffsetX + 1f) * 0.5f, (displayReticleOffsetY + 1f) * 0.5f); } }

        /// <summary>
        /// Show the current (active) Display Reticle
        /// on the Display. At runtime use ShowDisplayReticle()
        /// or HideDisplayReticle().
        /// </summary>
        public bool showActiveDisplayReticle = false;

        /// <summary>
        /// The Display Reticle's normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.
        /// At runtime call stickyDisplayModule.SetDisplayReticleOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float displayReticleOffsetX = 0f;

        /// <summary>
        /// The Display Reticle's normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.
        /// At runtime call stickyDisplayModule.SetDisplayReticleOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float displayReticleOffsetY = 0f;

        /// <summary>
        /// Should the Display Reticle follow the cursor or mouse screen position?
        /// </summary>
        public bool lockDisplayReticleToCursor = false;

        /// <summary>
        /// Colour of the active Display Reticle.
        /// At runtime call stickyDisplayModule.SetDisplayReticleColour(..)
        /// </summary>
        public Color32 activeDisplayReticleColour = Color.white;

        /// <summary>
        /// This is the main camera that Display will reference. If left empty, this will
        /// be auto-populated with your first Camera with a tag of MainCamera.
        /// Can be changed at runtime with stickyDisplayModule.SetCamera(..).
        /// </summary>
        public Camera mainCamera = null;

        #endregion

        #region Public Variables and Properties - Gauges

        /// <summary>
        /// The list of display gauges that can be used with
        /// this Sticky Display. Call ReinitialiseVariables() if
        /// you modify this list at runtime. Where possible use the API
        /// methods to Add, Delete, or set gauge attributes.
        /// </summary>
        public List<S3DDisplayGauge> displayGaugeList;

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
        /// this Sticky Display. Call ReinitialiseVariables() if
        /// you modify this list at runtime. Where possible use the API
        /// methods to Add, Delete, or set messages attributes.
        /// </summary>
        public List<S3DDisplayMessage> displayMessageList;

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
        /// this Sticky Display. Call ReinitialiseVariables() if
        /// you modify this list at runtime. Where possible use the API
        /// methods to Add, Delete, or set target attributes.
        /// </summary>
        public List<S3DDisplayTarget> displayTargetList;

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
        /// The Targets normalised viewable width of the screen.
        /// 1.0 is full width, 0.5 is half width.
        /// At runtime call stickyDisplayModule.SetTargetsViewportSize(..)
        /// </summary>
        [Range(0.1f, 1f)] public float targetsViewWidth = 1.0f;

        /// <summary>
        /// The Targets normalised viewable height of the screen.
        /// 1.0 is full height, 0.5 is half height.
        /// At runtime call stickyDisplayModule.SetTargetsViewportSize(..)
        /// </summary>
        [Range(0.1f, 1f)] public float targetsViewHeight = 0.5f;

        /// <summary>
        /// The X offset from centre of the screen for the Targets viewport
        /// At runtime call stickyDisplayModule.SetTargetsViewportOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float targetsViewOffsetX = 0f;

        /// <summary>
        /// The Y offset from centre of the screen for the Targets viewport
        /// At runtime call stickyDisplayModule.SetTargetsViewportOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float targetsViewOffsetY = 0.25f;

        /// <summary>
        /// The maximum distance, in metres, that targets can be away from the character
        /// </summary>
        public float targetingRange = 5000f;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// </summary>
        public bool isDisplayTargetListExpanded = true;

        #endregion

        #region Public Properties

        /// <summary>
        /// Is the sticky display module initialised? Gets set at runtime when Initialise() is called.
        /// </summary>
        public bool IsInitialised { get; private set; }

        /// <summary>
        /// Is the heads-up display shown? To set use ShowDisplay() or HideDisplay().
        /// IsDisplayShown should never be true at runtime if IsInitialised is false
        /// </summary>
        public bool IsDisplayShown { get; private set; }

        /// <summary>
        /// Get a reference to the Display canvas
        /// </summary>
        public Canvas GetCanvas { get { return IsInitialised ? canvas : GetComponent<Canvas>(); } }

        /// <summary>
        /// The default size the Display was designed with. This should always
        /// return 1920x1080
        /// </summary>
        public Vector2 ReferenceResolution { get { return refResolution; } }

        /// <summary>
        /// The current scaled resolution of the Display canvas
        /// </summary>
        public Vector2 CanvasResolution { get { return cvsResolutionFull; } }

        /// <summary>
        /// The current scale factor of the Display canvas
        /// </summary>
        public Vector3 CanvasScaleFactor { get { return cvsScaleFactor; } }

        /// <summary>
        /// The current actual screen resolution. This may be different from
        /// what the 
        /// </summary>
        public Vector2 ScreenResolution { get { return screenResolution; } }

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
        /// Outside play mode, show a bounding box for where the Display will be placed
        /// </summary>
        public bool showDisplayOutlineInScene = true;

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
        /// Display elements.
        /// </summary>
        public CallbackOnBrightnessChange callbackOnBrightnessChange = null;

        /// <summary>
        /// The name of the custom method that is called immediately after the
        /// Display size has changed. This method must take 2 float parameters. It
        /// should be a lightweight method to avoid performance issues. It could
        /// be used to update your custom Display elements.
        /// </summary>
        public CallbackOnSizeChange callbackOnSizeChange = null;

        #endregion

        #region Private Variables - General
        private Canvas canvas = null;
        private CanvasScaler canvasScaler = null;

        // Scaled canvas resolution. See CheckDisplayResize()
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

        // Used to update Display from the editor when not in play mode
        private bool editorMode = false;

        private List<Text> tempTextList = null;
        private List<UnityEngine.UI.Image> tempImgList = null;
        #endregion

        #region Private Variables - Overlay
        private UnityEngine.UI.Image primaryOverlayImg = null;
        private RectTransform overlayPanel = null;
        #endregion

        #region Private Variables - Brightness
        private S3DColour baseReticleColour;
        private S3DColour baseOverlayColour;
        private S3DColour baseAltitudeTextColour;
        private S3DColour baseAirspeedTextColour;
        #endregion

        #region Private Variables - Reticle
        private RectTransform hudRectTfrm = null;
        private Transform reticlePanel = null;
        private float reticleWidth = 1f;
        private float reticleHeight = 1f;
        private S3DDisplayReticle currentDisplayReticle = null;
        private UnityEngine.UI.Image displayReticleImg = null;
        private int numDisplayReticles = 0;
        #endregion

        #region Private Variables - Gauges
        private int numDisplayGauges = 0;
        private RectTransform gaugesRectTfrm = null;
        private Vector3 tempGaugeOffset;
        #endregion

        #region Private Variables - Message
        private int numDisplayMessages = 0;
        private Vector3 tempMessageOffset;
        #endregion

        #region Private Variables - Targets
        private int numDisplayTargets = 0;
        private RectTransform targetsRectTfrm = null;
        private Vector3 tempTargetOffset;
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

        #region Private Initialisation Methods

        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Update Methods

        private void Update()
        {
            #region Check Window Resize
            if (IsDisplayShown)
            {
                // Would be nice to not call this so often
                CheckDisplayResize(true);
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
                        cursorTimer += Time.deltaTime;
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

                    //DebugExtension.DebugPoint(currentMousePosition, Color.yellow, 0.3f);

                    // Convert 0.0 to 1.0 into -1.0 to 1.0.
                    SetDisplayReticleOffset(viewPoint.x * 2f - 1f, viewPoint.y * 2f - 1f);
                }
                #endregion
            }
            #endregion

            #region Message updates
            if (IsDisplayShown)
            {
                // NOTE: It might be cheaper to maintain an int[] of scrollable messages
                for (int dmIdx = 0; dmIdx < numDisplayMessages; dmIdx++)
                {
                    if (displayMessageList[dmIdx].scrollDirectionInt != S3DDisplayMessage.ScrollDirectionNone)
                    {                     
                        ScrollMessage(displayMessageList[dmIdx]);
                    }
                }
            }

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
        /// Show or Hide the Display.
        /// IsDisplayShown should never be true at runtime if IsInitialised is false
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideDisplay(bool isShown)
        {
            if (IsInitialised)
            {
                IsDisplayShown = isShown;
                hudPanel.gameObject.SetActive(isShown);
            }
            // Display should never be shown at runtime if IsInitialised is false
            else { IsDisplayShown = false; }
        }

        private IEnumerator UnlockCursor()
        {
            yield return new WaitForEndOfFrame();
            Cursor.lockState = CursorLockMode.None;
        }

        /// <summary>
        /// Check to see if the window has been resized. Ideally should be
        /// called a max once per frame (less often would be nice).
        /// </summary>
        private void CheckDisplayResize(bool isRefreshIfChanged)
        {
            cvsResolutionFull = GetDisplayFullSize(true);
            cvsResolutionHalf.x = cvsResolutionFull.x * 0.5f;
            cvsResolutionHalf.y = cvsResolutionFull.y * 0.5f;

            if (cvsResolutionFull.x != prevResolutionFull.x || cvsResolutionFull.y != prevResolutionFull.y)
            {
                refResolution = GetDisplayFullSize(false);

                cvsScaleFactor = canvas == null ? Vector3.one : canvas.transform.localScale;
                screenResolution = new Vector2(Screen.width, Screen.height);

                if (isRefreshIfChanged) { RefreshDisplay(); }
                prevResolutionFull.x = cvsResolutionFull.x;
                prevResolutionFull.y = cvsResolutionFull.y;
            }
        }

        /// <summary>
        /// Return full the width and height of the Display Panel.
        /// If the module hasn't been initialised it will return 1920x1080.
        /// Assume the dev hasn't changed the CanvasScaler reference resolution settings.
        /// When using the Display canvas current dimensions, always set isScaled = true.
        /// Returns x,y values in pixels.
        /// </summary>
        /// <param name="isScaled"></param>
        /// <returns></returns>
        private Vector2 GetDisplayFullSize(bool isScaled)
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
        /// Return half the width and height of the Display Panel.
        /// If the module hasn't been initialised it will return half 1920x1080.
        /// Assuming the dev hasn't changed it, it should always return the same
        /// when initialised.
        /// When using the Display canvas current dimensions, always set isScaled = true.
        /// Returns x,y values in pixels.
        /// </summary>
        /// <param name="isScaled"></param>
        /// <returns></returns>
        private Vector2 GetDisplayHalfSize(bool isScaled)
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
        /// Call CheckDisplayResize(false) before calling this method
        /// </summary>
        private void GetDisplayPanels()
        {
            if (hudRectTfrm != null)
            {
                hudPanel = hudRectTfrm.transform;
                reticlePanel = S3DUtils.GetChildTransform(hudPanel, reticlePanelName, this.name);
                overlayPanel = S3DUtils.GetChildRectTransform(hudPanel, overlayPanelName, this.name);

                gaugesRectTfrm = GetGaugesPanel();
                targetsRectTfrm = GetTargetsPanel();
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
            //if (altitudeTextRectTfrm != null) { altitudeText = altitudeTextRectTfrm.GetComponent<Text>(); }
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used to change Display outside play mode
        /// </summary>
        public void InitialiseEditorEssentials()
        {
            if (canvas == null) { canvas = GetCanvas; }

            hudRectTfrm = S3DUtils.GetChildRectTransform(transform, displayPanelName, this.name);

            if (hudRectTfrm != null)
            {
                // Call before GetDisplayPanels()
                CheckDisplayResize(false);

                GetDisplayPanels();
                GetImgComponents();
                GetTextComponents();

                InitialiseMessages();
                InitialiseTargets();

                editorMode = true;
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: StickyDisplayModule.InitialiseEditorEssentials() could not find canvas DisplayPanel. Did you start with the sample Display prefab from Prefabs/Visuals folder?"); }
            #endif
        }

        /// <summary>
        /// The Overlay Panel contains the Overlay image
        /// </summary>
        private void ShowOrHideOverlayPanel()
        {
            // Should the Overlay panel be enabled?
            if (showOverlay && !overlayPanel.gameObject.activeSelf)
            {
                overlayPanel.gameObject.SetActive(true);
            }
            else if (!showOverlay && overlayPanel.gameObject.activeSelf)
            {
                overlayPanel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Show or Hide the Overlay image on the Display. Turn on Display if required
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideOverlay(bool isShown)
        {
            if (IsInitialised)
            {
                showOverlay = isShown;

                ShowOrHideOverlayPanel();

                if (primaryOverlayImg != null) { primaryOverlayImg.enabled = isShown; }

                if (showOverlay && !IsDisplayShown) { ShowOrHideDisplay(true); }
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

            SetBrightness(brightness);
        }

        private void SetOverlayBrightness()
        {
            if (primaryOverlayImg != null)
            {
                // Skip more expensive HSV->RGBA and new Color op if possible
                if (brightness == 1f)
                {
                    S3DUtils.Color32toColorNoAlloc(ref primaryColour, ref tempColour);
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
                    S3DUtils.Color32toColorNoAlloc(ref activeDisplayReticleColour, ref tempColour);
                    displayReticleImg.color = tempColour;
                }
                else
                {
                    displayReticleImg.color = baseReticleColour.GetColorWithBrightness(brightness);
                }
            }
        }

        private void SetDisplayGaugeForegroundBrightness(S3DDisplayGauge displayGauge)
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
                    else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeForegroundBrightness - displayGauge.CachedFgImgComponent is null"); }
                    #endif
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeForegroundBrightness - displayGauge is null"); }
            #endif
        }

        private void SetDisplayGaugeBackgroundBrightness(S3DDisplayGauge displayGauge)
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
                    else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeBackgroundBrightness - displayGauge.CachedBgImgComponent is null"); }
                    #endif
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeBackgroundBrightness - displayGauge is null"); }
            #endif
        }

        private void SetDisplayGaugeTextBrightness(S3DDisplayGauge displayGauge)
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
                    else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeTextBrightness - displayGauge.CachedTextComponent is null"); }
                    #endif
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeTextBrightness - displayGauge is null"); }
            #endif
        }

        private void SetDisplayMessageBackgroundBrightness(S3DDisplayMessage displayMessage)
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
                    else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayMessageBackgroundBrightness - displayMessage.CachedBgImgComponent is null"); }
                    #endif
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayMessageBackgroundBrightness - displayMessage is null"); }
            #endif
        }

        private void SetDisplayMessageTextBrightness(S3DDisplayMessage displayMessage)
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
                    else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayMessageTextBrightness - displayMessage.CachedTextComponent is null"); }
                    #endif
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayMessageTextBrightness - displayMessage is null"); }
            #endif
        }


        /// <summary>
        /// Set the brightness of all slots (copies) of a DisplayTarget
        /// </summary>
        /// <param name="displayTarget"></param>
        private void SetDisplayTargetBrightness(S3DDisplayTarget displayTarget)
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
                        else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayTargetBrightness - displayTarget slot " + sIdx + " CachedImgComponent is null"); }
                        #endif
                    }
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayTargetBrightness - displayTarget is null"); }
            #endif
        }

        #endregion

        #region Private or Internal Methods - Reticle

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Used by StickyWeapon to replace the current sprite at runtime
        /// with one specific to a weapon.
        /// See stickyWeapon.CheckReticle() and stickyWeapon.RestoreReticle().
        /// </summary>
        /// <param name="reticleSprite"></param>
        internal void LoadDisplayReticleSprite (Sprite reticleSprite)
        {
            displayReticleImg.sprite = reticleSprite;
        }

        /// <summary>
        /// Load the reticle sprite into the UI image on the panel
        /// </summary>
        /// <param name="displayReticle"></param>
        private void LoadDisplayReticleSprite(S3DDisplayReticle displayReticle)
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

        /// <summary>
        /// Show or Hide the reticle. Turn on the Display if required.
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
                        Debug.LogWarning("StickyDisplayModule - could not show the Display Rectile because there was no active rectile");
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

                if (showActiveDisplayReticle && !IsDisplayShown) { ShowOrHideDisplay(true); }
            }
        }

        #endregion

        #region Private or Internal Methods - Gauges

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Create a new Gauge RectTransform under the Display GaugesPanel in the hierarchy
        /// Add a background Image
        /// Add a (foreground) fillable iamge
        /// Add a text panel
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <returns></returns>
        public RectTransform CreateGaugePanel(S3DDisplayGauge displayGauge)
        {
            RectTransform gaugePanel = null;

            if (gaugesRectTfrm == null) { gaugesRectTfrm = GetGaugesPanel(); }

            if (gaugesRectTfrm != null && displayGauge != null)
            {
                float panelWidth = displayGauge.displayWidth;
                float panelHeight = displayGauge.displayHeight;

                gaugePanel = S3DUtils.GetOrCreateChildRectTransform(gaugesRectTfrm, cvsResolutionFull, "Gauge_" + displayGauge.guidHash, 0f, 0f,
                                                                        panelWidth, panelHeight, 0.5f, 0.5f, 0.5f, 0.5f);

                if (gaugePanel != null)
                {
                    Image bgimgComponent = gaugePanel.gameObject.AddComponent<Image>();
                    if (bgimgComponent != null)
                    {
                        bgimgComponent.raycastTarget = false;
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: stickyDisplayModule.CreateGaugePanel() - could not add background to Gauge panel"); }
                    #endif

                    RectTransform gaugeAmtPanel = S3DUtils.GetOrCreateChildRectTransform(gaugePanel, cvsResolutionFull, "GaugeAmount", 0f, 0f,
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
                        else { Debug.LogWarning("ERROR: stickyDisplayModule.CreateGaugePanel() - could not add fill image to Gauge panel"); }
                        #endif
                    }

                    RectTransform gaugeTxtPanel = S3DUtils.GetOrCreateChildRectTransform(gaugePanel, cvsResolutionFull, "GaugeText", 0f, 0f,
                                                                        panelWidth, panelHeight, 0.5f, 0.5f, 0.5f, 0.5f);

                    if (gaugeTxtPanel != null)
                    {
                        Text textComponent = gaugeTxtPanel.gameObject.AddComponent<Text>();
                        if (textComponent != null)
                        {
                            textComponent.raycastTarget = false;
                            textComponent.resizeTextForBestFit = true;
                            // Text is add in InitialiseGauge().
                            textComponent.text = string.Empty;
                            if (Application.isPlaying)
                            {
                                textComponent.font = S3DUtils.GetDefaultFont();
                            }
                        }
                        #if UNITY_EDITOR
                        else { Debug.LogWarning("ERROR: stickyDisplayModule.CreateGaugePanel() - could not add text to Gauge panel"); }
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
            else { Debug.LogWarning("ERROR: stickyDisplayModule.CreateGaugePanel() - displayGauge is null or could not find or create " + gaugesPanelName); }
            #endif

            return gaugePanel;
        }

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Find the Gauge RectTransform under the Display in the hierarchy.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        private RectTransform GetGaugePanel(int guidHash)
        {
            if (gaugesRectTfrm == null) { gaugesRectTfrm = GetGaugesPanel(); }
            if (gaugesRectTfrm != null)
            {
                return S3DUtils.GetChildRectTransform(gaugesRectTfrm.transform, "Gauge_" + guidHash, this.name);
            }
            else { return null; }
        }

        /// <summary>
        /// Cache the S3DDisplayGauge RectTransform, Image and Text component.
        /// Set the Text colour.
        /// </summary>
        /// <param name="displayGauge"></param>
        private void InitialiseGauge(S3DDisplayGauge displayGauge)
        {
            displayGauge.CachedGaugePanel = GetGaugePanel(displayGauge.guidHash);

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
                if (tempTextList == null) { tempTextList = new List<Text>(1); }
                displayGauge.CachedGaugePanel.GetComponentsInChildren(tempTextList);
                if (tempTextList.Count > 0)
                {
                    displayGauge.CachedTextComponent = tempTextList[0];
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
        /// Initialise all the S3DDisplayGauges by caching the RectTransforms.
        /// </summary>
        private void InitialiseGauges()
        {
            int _numDisplayGauges = GetNumberDisplayGauges;

            if (_numDisplayGauges > 0)
            {
                RectTransform hudRectTfrm = GetDisplayPanel();
                if (hudRectTfrm != null)
                {
                    CheckDisplayResize(false);
                    for (int dgIdx = 0; dgIdx < _numDisplayGauges; dgIdx++)
                    {
                        InitialiseGauge(displayGaugeList[dgIdx]);
                    }

                    if (_numDisplayGauges > 0) { RefreshGaugesSortOrder(); }
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("stickyDisplayModule.InitialiseGauges() - could not find Display Panel"); }
                #endif
            }
        }

        /// <summary>
        /// Show or Hide the Display Gauge on the Display. Turn on the Display if required.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="isShown"></param>
        private void ShowOrHideGauge(S3DDisplayGauge displayGauge, bool isShown)
        {
            if (IsInitialised)
            {
                if (displayGauge.CachedGaugePanel != null)
                {
                    displayGauge.showGauge = isShown;
                    displayGauge.CachedGaugePanel.gameObject.SetActive(isShown);
                }

                if (isShown && !IsDisplayShown) { ShowOrHideDisplay(true); }
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
                    S3DDisplayGauge displayGauge = displayGaugeList[dmIdx];
                    ShowOrHideGauge(displayGauge, displayGauge.showGauge);
                }
            }
        }

        #endregion

        #region Private or Internal Methods - Messages

        /// <summary>
        /// Cache the S3DDisplayMessage RectTransform, Image and Text component.
        /// Set the Text colour.
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="hudRectTfrm"></param>
        private void InitialiseMessage(S3DDisplayMessage displayMessage, RectTransform hudRectTfrm)
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

                if (displayMessage.scrollDirectionInt != S3DDisplayMessage.ScrollDirectionNone)
                {
                    // scrollWidth/Height should be the same as those used in ScrollMessage(..)
                    // The left/right and top/bottom scroll limits can be ignored OR can consider the width/height of the message.
                    // Fullscreen scrolling = +/- half width of message + half the width of canvas
                    float scrollWidth = displayMessage.isScrollFullscreen ? displayMessage.displayWidth * cvsResolutionHalf.x + cvsResolutionHalf.x : displayMessage.displayWidth * cvsResolutionFull.x;

                    // Fullscreen scrolling = +/- half height of message + half the height of canvas
                    float scrollHeight = displayMessage.isScrollFullscreen ? displayMessage.displayHeight * cvsResolutionHalf.y + cvsResolutionHalf.y : displayMessage.displayHeight * cvsResolutionFull.y;

                    // Start at beginning of scrolling position (e.g. Offscreen left/right)
                    if (displayMessage.scrollDirectionInt == S3DDisplayMessage.ScrollDirectionLR)
                    {
                        displayMessage.scrollOffsetX = -scrollWidth;
                    }
                    else if (displayMessage.scrollDirectionInt == S3DDisplayMessage.ScrollDirectionRL)
                    {
                        displayMessage.scrollOffsetX = scrollWidth;
                    }
                    if (displayMessage.scrollDirectionInt == S3DDisplayMessage.ScrollDirectionBT)
                    {
                        displayMessage.scrollOffsetY = -scrollHeight;
                    }
                    else if (displayMessage.scrollDirectionInt == S3DDisplayMessage.ScrollDirectionTB)
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
        /// Initialise all the S3DDisplayMessages by caching the RectTransforms.
        /// </summary>
        private void InitialiseMessages()
        {
            int _numDisplayMessages = GetNumberDisplayMessages;

            if (_numDisplayMessages > 0)
            {
                RectTransform hudRectTfrm = GetDisplayPanel();
                if (hudRectTfrm != null)
                {
                    CheckDisplayResize(false);
                    for (int dmIdx = 0; dmIdx < _numDisplayMessages; dmIdx++)
                    {
                        InitialiseMessage(displayMessageList[dmIdx], hudRectTfrm);
                    }

                    if (_numDisplayMessages > 0) { RefreshMessagesSortOrder(); }
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("stickyDisplayModule.InitialiseMessages() - could not find Display Panel"); }
                #endif
            }
        }

        /// <summary>
        /// Show or Hide the Display Message on the Display. Turn on the Display if required.
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="isShown"></param>
        private void ShowOrHideMessage(S3DDisplayMessage displayMessage, bool isShown)
        {
            if (IsInitialised)
            {
                if (displayMessage.CachedMessagePanel != null)
                {
                    displayMessage.showMessage = isShown;
                    displayMessage.CachedMessagePanel.gameObject.SetActive(isShown);
                }

                if (isShown && !IsDisplayShown) { ShowOrHideDisplay(true); }
            }
        }

        /// <summary>
        /// Show or hide all messages
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideMessages(bool isShown)
        {
            if (IsInitialised)
            {
                for (int dmIdx = 0; dmIdx < numDisplayMessages; dmIdx++)
                {
                    ShowOrHideMessage(displayMessageList[dmIdx], isShown);
                }
            }
        }

        /// <summary>
        /// Show or Hide all messages based on their current settings
        /// </summary>
        private void ShowOrHideMessages()
        {
            if (IsInitialised)
            {
                for (int dmIdx = 0; dmIdx < numDisplayMessages; dmIdx++)
                {
                    S3DDisplayMessage displayMessage = displayMessageList[dmIdx];
                    ShowOrHideMessage(displayMessage, displayMessage.showMessage);
                }
            }
        }

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Find the Message RectTransform under the Display in the hierarchy.
        /// If hudRectTrfm is null, it will attempt to find it.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <param name="hudRectTrfm"></param>
        /// <returns></returns>
        private RectTransform GetMessagePanel(int guidHash, RectTransform hudRectTrfm)
        {
            if (hudRectTfrm == null) { hudRectTfrm = GetDisplayPanel(); }
            if (hudRectTfrm != null)
            {
                return S3DUtils.GetChildRectTransform(hudRectTfrm.transform, "Message_" + guidHash, this.name);
            }
            else { return null; }
        }

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Create a new Message RectTransform under the Display in the hierarchy
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="hudRectTfrm"></param>
        /// <returns></returns>
        public RectTransform CreateMessagePanel(S3DDisplayMessage displayMessage, RectTransform hudRectTfrm)
        {
            RectTransform messagePanel = null;

            if (displayMessage != null)
            {
                float panelWidth = displayMessage.displayWidth;
                float panelHeight = displayMessage.displayHeight;

                //Canvas _canvas = GetCanvas;
                //Vector3 _canvasScale = _canvas == null ? Vector3.one : _canvas.transform.localScale;

                if (IsInitialised) { numDisplayMessages++; }
                messagePanel = S3DUtils.GetOrCreateChildRectTransform(hudRectTfrm, cvsResolutionFull, "Message_" + displayMessage.guidHash, 0f, 0f,
                                                                        panelWidth, panelHeight, 0.5f, 0.5f, 0.5f, 0.5f);

                if (messagePanel != null)
                {
                    Image imgComponent = messagePanel.gameObject.AddComponent<Image>();
                    if (imgComponent != null)
                    {
                        imgComponent.raycastTarget = false;
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: stickyDisplayModule.CreateMessagePanel() - could not add background to Message panel"); }
                    #endif

                    RectTransform messageTxtPanel = S3DUtils.GetOrCreateChildRectTransform(messagePanel, cvsResolutionFull, "MessageText", 0f, 0f,
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
                                textComponent.font = S3DUtils.GetDefaultFont();
                            }
                        }
                        #if UNITY_EDITOR
                        else { Debug.LogWarning("ERROR: stickyDisplayModule.CreateMessagePanel() - could not add text to Message panel"); }
                        #endif
                    }

                    if (IsInitialised || editorMode) { InitialiseMessage(displayMessage, hudRectTfrm); }
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ERROR: stickyDisplayModule.CreateMessagePanel() - could not create Message panel"); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.CreateMessagePanel() - displayMessage is null"); }
            #endif

            return messagePanel;
        }

        /// <summary>
        /// Show or hide the message background by enabling or disabling the image script
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="isShown"></param>
        private void ShowOrHideDisplayMessageBackground(S3DDisplayMessage displayMessage, bool isShown)
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
        private void ScrollMessage(S3DDisplayMessage displayMessage)
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

                if (displayMessage.scrollDirectionInt == S3DDisplayMessage.ScrollDirectionLR)
                {
                    if (displayMessage.isScrollFullscreen) { tempMessageOffset.x = 0f; }
                    displayMessage.scrollOffsetX += deltaX;
                    displayMessage.scrollOffsetY = 0f;

                    if (displayMessage.scrollOffsetX > scrollWidth) { displayMessage.scrollOffsetX = -scrollWidth; }
                }
                else if (displayMessage.scrollDirectionInt == S3DDisplayMessage.ScrollDirectionRL)
                {
                    if (displayMessage.isScrollFullscreen) { tempMessageOffset.x = 0f; }
                    displayMessage.scrollOffsetX -= deltaX;
                    displayMessage.scrollOffsetY = 0f;

                    if (displayMessage.scrollOffsetX < -scrollWidth) { displayMessage.scrollOffsetX = scrollWidth; }
                }
                else if (displayMessage.scrollDirectionInt == S3DDisplayMessage.ScrollDirectionBT)
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
        public RectTransform CreateTargetPanel(S3DDisplayTarget displayTarget, int slotIndex)
        {
            RectTransform targetPanel = null;

            if (targetsRectTfrm == null) { targetsRectTfrm = GetTargetsPanel(); }

            if (displayTarget != null && targetsRectTfrm != null)
            {
                if (slotIndex >= 0 && slotIndex < S3DDisplayTarget.MAX_DISPLAYTARGET_SLOTS)
                {
                    // Convert into 0.0 to 1.0 value. The width/height values assume default 1920x1080.
                    float panelWidth = displayTarget.width / refResolution.x;
                    float panelHeight = displayTarget.height / refResolution.y;

                    //if (IsInitialised && slotIndex == 0) { numS3DDisplayTargets++; }
                    targetPanel = S3DUtils.GetOrCreateChildRectTransform(targetsRectTfrm, cvsResolutionFull, "Target_" + displayTarget.guidHash + "_" + slotIndex.ToString(), 0f, 0f,
                                                                            panelWidth, panelHeight, 0.5f, 0.5f, 0.5f, 0.5f);

                    if (targetPanel != null)
                    {
                        Image imgComponent = targetPanel.gameObject.AddComponent<Image>();
                        if (imgComponent != null)
                        {
                            imgComponent.raycastTarget = false;
                        }
                        #if UNITY_EDITOR
                        else { Debug.LogWarning("ERROR: stickyDisplayModule.CreateTargetPanel() - could not add image to Target panel"); }
                        #endif

                        if ((IsInitialised || editorMode) && !displayTarget.isInitialised) { InitialiseTarget(displayTarget); }
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: stickyDisplayModule.CreateTargetPanel() - could not create Target panel"); }
                    #endif
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ERROR: stickyDisplayModule.CreateTargetPanel() - could not create Target panel. Slot number is outside the range 0 to " + S3DDisplayTarget.MAX_DISPLAYTARGET_SLOTS.ToString()); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: StickyDisplayModule.CreateTargetPanel() - displayTarget is null or could not find or create " + targetsPanelName); }
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
                return S3DUtils.GetChildRectTransform(targetsRectTfrm.transform, "Target_" + guidHash + "_" + slotIndex, this.name);
            }
            else { return null; }
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used by the editor when moving S3DDisplayTargets around in the list.
        /// </summary>
        /// <param name="displayTargetIndex"></param>
        public void RefreshDisplayTargetSlots()
        {
            numDisplayTargets = displayTargetList == null ? 0 : displayTargetList.Count;

            for (int dtIdx = 0; dtIdx < numDisplayTargets; dtIdx++)
            {
                S3DDisplayTarget displayTarget = displayTargetList[dtIdx];

                // Initialise the Display Target slots
                displayTarget.displayTargetSlotList = new List<S3DDisplayTargetSlot>(displayTarget.maxNumberOfTargets);

                for (int slotIdx = 0; slotIdx < displayTarget.maxNumberOfTargets; slotIdx++)
                {
                    S3DDisplayTargetSlot displayTargetSlot = InitialiseTargetSlot(displayTarget, slotIdx);

                    displayTarget.displayTargetSlotList.Add(displayTargetSlot);
                }

                // InitialiseTargetSlot will turn all the reticles off, so update the S3DDisplayTarget property too
                displayTarget.showTarget = false;
            }
        }

        /// <summary>
        /// Initialise the display target
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <param name="hudRectTfrm"></param>
        private void InitialiseTarget(S3DDisplayTarget displayTarget)
        {
            if (displayTarget.maxNumberOfTargets < 0) { displayTarget.maxNumberOfTargets = 1; }
            else if (displayTarget.maxNumberOfTargets > S3DDisplayTarget.MAX_DISPLAYTARGET_SLOTS) { displayTarget.maxNumberOfTargets = S3DDisplayTarget.MAX_DISPLAYTARGET_SLOTS; }

            // Initialise the Display Target slots
            displayTarget.displayTargetSlotList = new List<S3DDisplayTargetSlot>(displayTarget.maxNumberOfTargets);

            for (int slotIdx = 0; slotIdx < displayTarget.maxNumberOfTargets; slotIdx++)
            {
                S3DDisplayTargetSlot displayTargetSlot = InitialiseTargetSlot(displayTarget, slotIdx);

                displayTarget.displayTargetSlotList.Add(displayTargetSlot);
            }

            SetDisplayTargetSize(displayTarget, displayTarget.width, displayTarget.height);

            // SetDisplayTargetReticleColour does not update the baseReticleColour if the reticleColour has not changed. So do it in here instead
            displayTarget.baseReticleColour.Set(displayTarget.reticleColour.r, displayTarget.reticleColour.g, displayTarget.reticleColour.b, displayTarget.reticleColour.a, true);
            SetDisplayTargetBrightness(displayTarget);

            displayTarget.isInitialised = true;
        }

        /// <summary>
        /// Initialise a S3DDisplayTarget Slot. This includes:
        /// 1. caching the panel and image component
        /// 2. Loading the reticle sprite
        /// 3. Setting the offset to 0,0
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <param name="slotIdx"></param>
        /// <returns></returns>
        private S3DDisplayTargetSlot InitialiseTargetSlot(S3DDisplayTarget displayTarget, int slotIdx)
        {
            S3DDisplayTargetSlot displayTargetSlot = new S3DDisplayTargetSlot();

            displayTargetSlot.CachedTargetPanel = GetTargetPanel(displayTarget.guidHash, slotIdx);

            if (displayTargetSlot.CachedTargetPanel != null)
            {
                displayTargetSlot.CachedImgComponent = displayTargetSlot.CachedTargetPanel.GetComponent<Image>();

                // Add the Reticle sprite to the image for this Display Target
                if (displayTargetSlot.CachedImgComponent != null)
                {
                    // TODO - consider getting the sprite only once per S3DDisplayTarget
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
        /// Initialise all the S3DDisplayTargets by caching the RectTransforms.
        /// </summary>
        private void InitialiseTargets()
        {
            int _numS3DDisplayTargets = GetNumberDisplayTargets;
            
            if (_numS3DDisplayTargets > 0)
            {
                if (targetsRectTfrm == null) { targetsRectTfrm = GetTargetsPanel(); }
                if (targetsRectTfrm != null)
                {
                    CheckDisplayResize(false);

                    for (int dmIdx = 0; dmIdx < _numS3DDisplayTargets; dmIdx++)
                    {
                        InitialiseTarget(displayTargetList[dmIdx]);
                    }

                    if (_numS3DDisplayTargets > 0) { RefreshTargetsSortOrder(); }
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("StickyDisplayModule.InitialiseTargets() - could not find " + targetsPanelName); }
                #endif
            }
        }

        /// <summary>
        /// Show or Hide S3DDisplayTarget slot on the HUD.
        /// By design, if the HUD is not shown, the Target slot will not be show.
        /// </summary>
        /// <param name="displayTargetSlot"></param>
        /// <param name="isShown"></param>
        private void ShowOrHideTargetSlot(S3DDisplayTargetSlot displayTargetSlot, bool isShown)
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
        private void ShowOrHideTarget(S3DDisplayTarget displayTarget, bool isShown)
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
                    S3DDisplayTarget displayTarget = displayTargetList[dmIdx];
                    ShowOrHideTarget(displayTarget, displayTarget.showTarget);
                }
            }
        }

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Initialise the StickyDisplayModule. Either set initialiseOnStart to false and call this
        /// method in your code, or set initialiseOnStart to true in the inspector and don't call
        /// this method.
        /// </summary>
        public void Initialise()
        {
            // calling this method twice may incorrectly set the positions of the Altitude and speed panels.
            if (IsInitialised) { return; }

            canvas = GetComponent<Canvas>();
            canvasScaler = GetComponent<CanvasScaler>();

            hudRectTfrm = S3DUtils.GetChildRectTransform(transform, displayPanelName, this.name);

            if (canvas != null && hudRectTfrm != null)
            {
                canvas.sortingOrder = canvasSortOrder;

                // Use the scaled size rather than the original reference resolution size.
                CheckDisplayResize(false);

                GetDisplayPanels();

                if (reticlePanel != null && overlayPanel != null)
                {
                    GetImgComponents();
                    GetTextComponents();

                    ReinitialiseVariables();

                    #if UNITY_EDITOR
                    if (!isMainCameraAssigned) { Debug.LogWarning("StickyDisplayModule could not find a camera with the MainCamera tag set for " + gameObject.name); }
                    #endif

                    IsInitialised = displayReticleImg != null && isMainCameraAssigned;

                    RefreshDisplay();

                    ShowOrHideDisplay(isShowOnInitialise);

                    // If there isn't an active display module, set this
                    // to be the active module.
                    if (IsInitialised && activeDisplayModule == null)
                    {
                        activeDisplayModule = this;
                    }
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: StickyDisplayModule.Initialise() could not find canvas DisplayPanel. Did you start with the sample Display prefab from Prefabs/Visuals folder?"); }
            #endif
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
        /// Refresh all components of the Display. Typically called after the screen has been resized.
        /// </summary>
        public void RefreshDisplay()
        {
            if (IsInitialised || editorMode)
            {
                // Set the initial position
                SetDisplayReticleOffset(displayReticleOffsetX, displayReticleOffsetY);

                SetDisplaySize(displayWidth, displayHeight);
                SetDisplayOffset(displayOffsetX, displayOffsetY);

                // Show or Hide initial setup
                ShowOrHideReticle(showActiveDisplayReticle);
                SetPrimaryColour(primaryColour);
                SetDisplayReticleColour(activeDisplayReticleColour);
                ShowOrHideOverlay(showOverlay);
                InitialiseMessages();
                ShowOrHideMessages();
                InitialiseGauges();
                ShowOrHideGauges();
                InitialiseTargets();
                ShowOrHideTargets();

                // Set brightness after the colours have been set.
                InitialiseBrightness();
            }
        }

        /// <summary>
        /// Show the display. Has no effect if IsInitialised is false.
        /// </summary>
        public void ShowDisplay()
        {
            ShowOrHideDisplay(true);
        }

        /// <summary>
        /// Hide the display. Has no effect if IsInitialised is false.
        /// </summary>
        public void HideDisplay()
        {
            ShowOrHideDisplay(false);
        }

        /// <summary>
        /// Show the Overlay image on the display. Has no effect if IsInitialised is false.
        /// </summary>
        public void ShowOverlay()
        {
            ShowOrHideOverlay(true);
        }

        /// <summary>
        /// Hide the Overlay image on the display. Has no effect if IsInitialised is false.
        /// </summary>
        public void HideOverlay()
        {
            ShowOrHideOverlay(false);
        }

        /// <summary>
        /// Set or assign the main camera used by the display for calculations.
        /// </summary>
        /// <param name="camera"></param>
        public void SetCamera(Camera camera)
        {
            mainCamera = camera;
            isMainCameraAssigned = camera != null;
        }

        /// <summary>
        /// Set the Sticky3D Display to use a particular monitor. Displays or monitors are numbered from 1 to 8.
        /// </summary>
        /// <param name="displayNumber">1 to 8</param>
        public void SetCanvasTargetDisplay (int displayNumber)
        {
            if (IsInitialised && S3DUtils.VerifyTargetDisplay(displayNumber, true))
            {
                Canvas _canvas = GetCanvas;

                if (_canvas != null) { _canvas.targetDisplay = displayNumber - 1; }
            }
        }

        /// <summary>
        /// Set the offset (position) of the Display.
        /// If the module has been initialised, this will also re-position the Display.
        /// </summary>
        /// <param name="offsetX">Horizontal offset from centre. Range between -1 and 1</param>
        /// <param name="offsetY">Vertical offset from centre. Range between -1 and 1</param>
        public void SetDisplayOffset(float offsetX, float offsetY)
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
                // Here we use the half original (reference) Display size as it doesn't need to be scaled in any way.
                // The parent display canvas is correctly scaled and sized to the actual monitor or device display.
                overlayPanel.localPosition = new Vector3(offsetX * cvsResolutionFull.x, offsetY * cvsResolutionFull.y, overlayPanel.localPosition.z);
            }
        }

        /// <summary>
        /// Set the size of the display overlay image and text.
        /// If the module has been initialised, this will also resize the display.
        /// The values are only updated if they are outside the range 0.0 to 1.0 or have changed.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetDisplaySize(float width, float height)
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
                // Here we use the original (reference) display size as it doesn't need to be scaled in any way.
                // The parent display canvas is correctly scaled and sized to the actual monitor or device display.
                overlayPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, displayWidth * cvsResolutionFull.x);
                overlayPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, displayHeight * cvsResolutionFull.y);

                if (callbackOnSizeChange != null) { callbackOnSizeChange(displayWidth, displayHeight); }
            }
        }

        /// <summary>
        /// Set the primary colour of the heads-up display. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the display with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetPrimaryColour(Color32 newColour)
        {
            S3DUtils.UpdateColour(ref newColour, ref primaryColour, ref baseOverlayColour, true);

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
        /// If the module has been initialised, this will also re-colour the display with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetPrimaryColour(Color newColour)
        {
            S3DUtils.UpdateColour(ref newColour, ref primaryColour, ref baseOverlayColour, true);

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
        /// Set the overall brightness of the display.
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

                int _numDisplayMessages = GetNumberDisplayMessages;

                // Set the brightness of all the messages
                for (int dmIdx = 0; dmIdx < _numDisplayMessages; dmIdx++)
                {
                    SetDisplayMessageBackgroundBrightness(displayMessageList[dmIdx]);
                    SetDisplayMessageTextBrightness(displayMessageList[dmIdx]);
                }

                int _numDisplayGauges = GetNumberDisplayGauges;

                // Set the brightness for all the gauges
                for (int dgIdx = 0; dgIdx < _numDisplayGauges; dgIdx++)
                {
                    SetDisplayGaugeForegroundBrightness(displayGaugeList[dgIdx]);
                    SetDisplayGaugeBackgroundBrightness(displayGaugeList[dgIdx]);
                    SetDisplayGaugeTextBrightness(displayGaugeList[dgIdx]);
                }

                int _numDisplayTargets = GetNumberDisplayTargets;

                // Set the brightness for all the targets
                for (int dtgtIdx = 0; dtgtIdx < _numDisplayTargets; dtgtIdx++)
                {
                    SetDisplayTargetBrightness(displayTargetList[dtgtIdx]);
                }
            }

            if (callbackOnBrightnessChange != null) { callbackOnBrightnessChange(brightness); }
        }

        /// <summary>
        /// Set the sort order in the scene of the display. Higher values appear on top.
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
        /// Turn on or off the display. Has no effect if not initialised. See also ShowDisplay() and HideDisplay()
        /// </summary>
        public void ToggleDisplay()
        {
            ShowOrHideDisplay(!IsDisplayShown);
        }

        #endregion

        #region Public API Methods - Panels

        /// <summary>
        /// Get the Display RectTransform or panel
        /// </summary>
        /// <returns></returns>
        public RectTransform GetDisplayPanel()
        {
            return S3DUtils.GetChildRectTransform(transform, displayPanelName, this.name);
        }

        /// <summary>
        /// Get the parent panel for the Display Gauges.
        /// Create it if it does not already exist
        /// </summary>
        /// <returns></returns>
        public RectTransform GetGaugesPanel()
        {
            if (hudRectTfrm == null) { hudRectTfrm = GetDisplayPanel(); }

            if (hudRectTfrm != null)
            {
                // Stretched panel
                return S3DUtils.GetOrCreateChildRectTransform(hudRectTfrm, cvsResolutionFull, gaugesPanelName, 0f, 0f, 1f, 1f, 0f, 0f, 1f, 1f);
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
            if (hudRectTfrm == null) { hudRectTfrm = GetDisplayPanel(); }

            if (hudRectTfrm != null)
            {
                // Stretched panel
                return S3DUtils.GetOrCreateChildRectTransform(hudRectTfrm, cvsResolutionFull, targetsPanelName, 0f, 0f, 1f, 1f, 0f, 0f, 1f, 1f);
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
            S3DDisplayReticle _tempDR = null;

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
        public S3DDisplayReticle GetDisplayReticle(int guidHash)
        {
            S3DDisplayReticle displayReticle = null;
            S3DDisplayReticle _tempDR = null;

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
            S3DDisplayReticle _tempDR = null;

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
        /// Set the offset (position) of the Display Reticle on the display.
        /// If the module has been initialised, this will also re-position the Display Reticle.
        /// Same as SetDisplayReticleOffset(offset.x, offset.y)
        /// </summary>
        /// <param name="offset">Horizontal and Vertical offset from centre. Values should be between -1 and 1</param>
        public void SetDisplayReticleOffset(Vector2 offset)
        {
            SetDisplayReticleOffset(offset.x, offset.y);
        }

        /// <summary>
        /// Set the offset (position) of the Display Reticle on the display.
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
                Vector2 halfHUDSize = GetDisplayHalfSize(true);

                if (lockDisplayReticleToCursor)
                {
                    // Centre on mouse pointer more accurately (reticle can go half outside screen dimensions on edges)
                    displayReticleImg.transform.localPosition = new Vector3(offsetX * halfHUDSize.x, offsetY * halfHUDSize.y, displayReticleImg.transform.localPosition.z);
                }
                else
                {
                    // Original behaviour like SSC
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
            S3DUtils.UpdateColour(ref newColour, ref activeDisplayReticleColour, ref baseReticleColour, true);

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
            S3DUtils.UpdateColour(ref newColour, ref activeDisplayReticleColour, ref baseReticleColour, true);

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
        /// Show the Display Reticle on the display. The display will automatically be shown if it is not already visible.
        /// </summary>
        public void ShowDisplayReticle()
        {
            ShowOrHideReticle(true);
        }

        /// <summary>
        /// Show or Hide the Display Reticle. The display will be shown if required.
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
            if (displayReticleList == null) { displayReticleList = new List<S3DDisplayReticle>(1); }
        }

        #endregion

        #region Public API Methods - Gauges

        /// <summary>
        /// Add a new gauge to the display. By design, they are not visible at runtime when first added.
        /// </summary>
        /// <param name="gaugeName"></param>
        /// <param name="gaugeText"></param>
        /// <returns></returns>
        public S3DDisplayGauge AddGauge(string gaugeName, string gaugeText)
        {
            S3DDisplayGauge displayGauge = null;

            if (GetDisplayPanel() == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: StickyDisplayModule.AddGauge() - could not find DisplayPanel for the display. Did you use the prefab from Prefabs/Visuals folder?");
                #endif
            }
            else
            {
                displayGauge = new S3DDisplayGauge();
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
        /// Add a gauge to the display using a displayGauge instance. Typically, this is used
        /// with CopyDisplayGauge(..).
        /// </summary>
        /// <param name="displayGauge"></param>
        public void AddGauge(S3DDisplayGauge displayGauge)
        {
            if (displayGauge != null)
            {
                if (GetDisplayPanel() == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: StickyDisplayModule.AddGauge() - could not find DisplayPanel for the display. Did you use the prefab from Prefabs/Visuals folder?");
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
        /// Delete a gauge from the display.
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
        /// Create a copy of an existing S3DDisplayGauge, and give it a new name.
        /// Call AddGauge(newDisplayGauge) to make it useable in the game.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="NameOfCopy"></param>
        /// <returns></returns>
        public S3DDisplayGauge CopyDisplayGauge(S3DDisplayGauge displayGauge, string NameOfCopy)
        {
            S3DDisplayGauge dgCopy = new S3DDisplayGauge(displayGauge);

            if (dgCopy != null)
            {
                // make it unique
                dgCopy.guidHash = S3DMath.GetHashCodeFromGuid();
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
        /// Get a S3DDisplayGauge given its guidHash.
        /// See also GetDisplayGaugeGuidHash(..).
        /// Will return null if guidHash parameter is 0, it cannot be found.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public S3DDisplayGauge GetDisplayGauge(int guidHash)
        {
            S3DDisplayGauge displayGauge = null;
            S3DDisplayGauge _tempDG = null;

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
        public S3DDisplayGauge GetDisplayGauge(string displayGaugeName)
        {
            S3DDisplayGauge displayGauge = null;
            S3DDisplayGauge _tempDG = null;

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
            S3DDisplayGauge displayGauge = GetDisplayGauge(guidHash);
            if (displayGauge != null) { ShowOrHideGauge(displayGauge, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayGauge - Could not find gauge with guidHash: " + guidHash); }
            #endif
        }

        /// <summary>
        /// Hide or turn off the Display Gauge
        /// </summary>
        /// <param name="displayGauge"></param>
        public void HideDisplayGauge(S3DDisplayGauge displayGauge)
        {
            if (displayGauge != null) { ShowOrHideGauge(displayGauge, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayGauge - Could not find gauge - parameter was null"); }
            #endif
        }

        /// <summary>
        /// Hide or turn off all Display Gauges. StickyDisplayModule must be initialised.
        /// </summary>
        public void HideDisplayGauges()
        {
            ShowOrHideGauges(false);
        }

        /// <summary>
        /// After adding or moving S3DDisplayGauges, they may need to be sorted to
        /// have the correct z-order in on the display.
        /// </summary>
        public void RefreshGaugesSortOrder()
        {
            if (gaugesRectTfrm != null)
            {
                // Gauges should begin at index 0.
                int zIndex = -1;

                // Update number of S3DDisplayGauges. This can help issues in the Editor, like moving DislayGauges
                // up or down in the list while in play mode.
                numDisplayGauges = displayGaugeList == null ? 0 : displayGaugeList.Count;

                for (int dtIdx = 0; dtIdx < numDisplayGauges; dtIdx++)
                {
                    S3DDisplayGauge displayGauge = displayGaugeList[dtIdx];

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
        /// The foreground colour of the gauge will be determined by the gauge value and the low, medium and high colours.
        /// LowColour = value of 0, MediumColour when value is 0.5, and HighColour when value is 1.0
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="lowColour"></param>
        /// <param name="mediumColour"></param>
        /// <param name="highColour"></param>
        public void SetDisplayGaugeValueAffectsColourOn(S3DDisplayGauge displayGauge, Color lowColour, Color mediumColour, Color highColour)
        {
            if (displayGauge != null)
            {
                S3DUtils.ColortoColorNoAlloc(ref lowColour, ref displayGauge.foregroundLowColour);
                S3DUtils.ColortoColorNoAlloc(ref mediumColour, ref displayGauge.foregroundMediumColour);
                S3DUtils.ColortoColorNoAlloc(ref highColour, ref displayGauge.foregroundHighColour);

                displayGauge.isColourAffectByValue = true;

                if (IsInitialised || editorMode)
                {
                    SetDisplayGaugeValue(displayGauge, displayGauge.gaugeValue);
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeValueAffectsColour - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// The value of the gauge does not affect the foreground colour.
        /// When turning off this feature the new foreground colour would typically be the old foregroundHighColour.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="newForegroundColour"></param>
        public void SetDisplayGaugeValueAffectsColourOff(S3DDisplayGauge displayGauge, Color newForegroundColour)
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
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeValueAffectsColour - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the offset (position) of the Display Gauge on the display.
        /// If the module has been initialised, this will also re-position the Display Gauge.
        /// </summary>
        /// <param name="offsetX">Horizontal offset from centre. Range between -1 and 1</param>
        /// <param name="offsetY">Vertical offset from centre. Range between -1 and 1</param>
        public void SetDisplayGaugeOffset(S3DDisplayGauge displayGauge, float offsetX, float offsetY)
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
                    CheckDisplayResize(false);
                    UnityEditor.Undo.RecordObject(dgTrfm, string.Empty);
                }
                #endif

                // Use the scaled display size. This works in and out of play mode
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
        public void SetDisplayGaugeSize(S3DDisplayGauge displayGauge, float width, float height)
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
                if (editorMode) { CheckDisplayResize(false); }
                #endif

                RectTransform dgRectTfrm = displayGauge.CachedGaugePanel;

                float pixelWidth = displayGauge.displayWidth * cvsResolutionFull.x;
                float pixelHeight = displayGauge.displayHeight * cvsResolutionFull.y;

                if (dgRectTfrm != null)
                {
                    // Here we use the original (reference) display size as it doesn't need to be scaled in any way.
                    // The parent display canvas is correctly scaled and sized to the actual monitor or device display.
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
        /// Update the value or reading of the gauge. If Value Affects Colour (isColourAffectByValue)
        /// is enabled, the foreground colour of the gauge will also be updated
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="gaugeValue"></param>
        public void SetDisplayGaugeValue(S3DDisplayGauge displayGauge, float gaugeValue)
        {
            if (displayGauge != null)
            {
                // Clamp 0.0 to 1.0
                float _gaugeValue = gaugeValue < 0f ? 0f : gaugeValue > 1f ? 1f : gaugeValue;

                displayGauge.gaugeValue = _gaugeValue;
                UnityEngine.UI.Image fgImg = displayGauge.CachedFgImgComponent;

                if (fgImg != null)
                {
                    fgImg.fillAmount = _gaugeValue;

                    if (displayGauge.isColourAffectByValue)
                    {
                        if (_gaugeValue > 0.5f)
                        {
                            Color _newColour = Color.Lerp(displayGauge.foregroundMediumColour, displayGauge.foregroundHighColour, (_gaugeValue - 0.5f) * 2f);

                            S3DUtils.UpdateColour(ref _newColour, ref displayGauge.foregroundColour, ref displayGauge.baseForegroundColour, false);
                            SetDisplayGaugeForegroundBrightness(displayGauge);
                        }
                        else
                        {
                            Color _newColour = Color.Lerp(displayGauge.foregroundLowColour, displayGauge.foregroundMediumColour, _gaugeValue * 2f);

                            S3DUtils.UpdateColour(ref _newColour, ref displayGauge.foregroundColour, ref displayGauge.baseForegroundColour, true);
                            SetDisplayGaugeForegroundBrightness(displayGauge);
                        }
                    }
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeValue - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Update the text of the gauge
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="gaugeText"></param>
        public void SetDisplayGaugeText(S3DDisplayGauge displayGauge, string gaugeText)
        {
            if (displayGauge != null)
            {
                displayGauge.gaugeString = gaugeText;
                Text uiText = displayGauge.CachedTextComponent;

                if (uiText != null) { uiText.text = gaugeText; }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeText - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Update the position of the text within the gauge panel
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="textAlignment"></param>
        public void SetDisplayGaugeTextAlignment(S3DDisplayGauge displayGauge, TextAnchor textAlignment)
        {
            if (displayGauge != null)
            {
                if (!editorMode) { displayGauge.textAlignment = textAlignment; }

                Text uiText = displayGauge.CachedTextComponent;

                if (uiText != null) { uiText.alignment = textAlignment; }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeTextAlignment - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the font of the S3DDisplayGauge Text component
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="font"></param>
        public void SetDisplayGaugeTextFont(S3DDisplayGauge displayGauge, Font font)
        {
            if (displayGauge != null)
            {
                Text uiText = displayGauge.CachedTextComponent;

                if (uiText != null)
                {
                    uiText.font = font;
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeTextFont - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the font style of the S3DDisplayGauge Text component
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="font"></param>
        public void SetDisplayGaugeTextFontStyle(S3DDisplayGauge displayGauge, FontStyle fontStyle)
        {
            if (displayGauge != null)
            {
                displayGauge.fontStyle = (S3DDisplayGauge.DGFontStyle)fontStyle;

                Text uiText = displayGauge.CachedTextComponent;

                if (uiText != null)
                {
                    uiText.fontStyle = fontStyle;
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeTextFontStyle - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the font size of the display gauge text. If isBestFit is false, maxSize is the font size set.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="isBestFit"></param>
        /// <param name="minSize"></param>
        /// <param name="maxSize"></param>
        public void SetDisplayGaugeTextFontSize(S3DDisplayGauge displayGauge, bool isBestFit, int minSize, int maxSize)
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
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeTextFontSize - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Gauge text colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the gauge text with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayGaugeTextColour(S3DDisplayGauge displayGauge, Color newColour)
        {
            if (displayGauge != null)
            {
                S3DUtils.UpdateColour(ref newColour, ref displayGauge.textColour, ref displayGauge.baseTextColour, true);

                SetDisplayGaugeTextBrightness(displayGauge);
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeTextColour - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Update the direction of the text within the gauge panel
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="textDirection"></param>
        public void SetDisplayGaugeTextDirection(S3DDisplayGauge displayGauge, S3DDisplayGauge.DGTextDirection textDirection)
        {
            if (displayGauge != null)
            {
                if (!editorMode) { displayGauge.textDirection = textDirection; }

                Text uiText = displayGauge.CachedTextComponent;

                if (uiText != null)
                {
                    #if UNITY_EDITOR
                    if (editorMode) { CheckDisplayResize(false); }
                    #endif

                    float pixelWidth = displayGauge.displayWidth * cvsResolutionFull.x;
                    float pixelHeight = displayGauge.displayHeight * cvsResolutionFull.y;
                    float textRotation = 0f;

                    RectTransform dgTextRectTfrm = displayGauge.CachedTextComponent.rectTransform;

                    if (textDirection == S3DDisplayGauge.DGTextDirection.Horizontal)
                    {
                        dgTextRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixelWidth);
                        dgTextRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixelHeight);
                    }
                    else
                    {
                        if (textDirection == S3DDisplayGauge.DGTextDirection.BottomTop)
                        {
                            textRotation = 90f;
                        }
                        else { textRotation = 270f; }

                        dgTextRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixelHeight);
                        dgTextRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixelWidth);
                    }

                    uiText.transform.localRotation = Quaternion.Euler(uiText.transform.localRotation.x, uiText.transform.localRotation.y, textRotation);
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeTextRotation - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Gauge foreground colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the gauge foreground with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayGaugeForegroundColour(S3DDisplayGauge displayGauge, Color newColour)
        {
            if (displayGauge != null)
            {
                S3DUtils.UpdateColour(ref newColour, ref displayGauge.foregroundColour, ref displayGauge.baseForegroundColour, true);

                SetDisplayGaugeForegroundBrightness(displayGauge);
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeForegroundColour - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Gauge foreground sprite. This is used to render the gauge value by partially filling it.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="newSprite"></param>
        public void SetDisplayGaugeForegroundSprite(S3DDisplayGauge displayGauge, Sprite newSprite)
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
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeForegroundSprite - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Gauge background colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the gauge background with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayGaugeBackgroundColour(S3DDisplayGauge displayGauge, Color newColour)
        {
            if (displayGauge != null)
            {
                S3DUtils.UpdateColour(ref newColour, ref displayGauge.backgroundColour, ref displayGauge.baseBackgroundColour, true);

                SetDisplayGaugeBackgroundBrightness(displayGauge);
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeBackgroundColour - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Gauge background sprite. This is used to render the background image of the gauge.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="newSprite"></param>
        public void SetDisplayGaugeBackgroundSprite(S3DDisplayGauge displayGauge, Sprite newSprite)
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
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeBackgroundSprite - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Gauge fill method. This determines how the gauge is filled
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="fillMethod"></param>
        public void SetDisplayGaugeFillMethod(S3DDisplayGauge displayGauge, S3DDisplayGauge.DGFillMethod fillMethod)
        {
            if (displayGauge != null)
            {
                UnityEngine.UI.Image fgImg = displayGauge.CachedFgImgComponent;

                if (fgImg != null)
                {
                    fgImg.fillMethod = (UnityEngine.UI.Image.FillMethod)fillMethod;
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeFillMethod - displayGauge is null"); }
            #endif
        }

        /// <summary>
        /// Sets whether or not the foreground and background sprites keep their original texture aspect ratio.
        /// This can be useful when creating circular gauges.
        /// </summary>
        /// <param name="displayGauge"></param>
        /// <param name="fillMethod"></param>
        public void SetDisplayGaugeKeepAspectRatio(S3DDisplayGauge displayGauge, bool isKeepAspectRatio)
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
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayGaugeKeepAspectRatio - displayGauge is null"); }
            #endif
        }
      
        /// <summary>
        /// Show the Display Gauge on the display. The display will automatically be shown if it is not already visible.
        /// </summary>
        /// <param name="guidHash"></param>
        public void ShowDisplayGauge(int guidHash)
        {
            S3DDisplayGauge displayGauge = GetDisplayGauge(guidHash);
            if (displayGauge != null) { ShowOrHideGauge(displayGauge, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayGauge - Could not find gauge with guidHash: " + guidHash); }
            #endif
        }

        /// <summary>
        /// Show the Display Gauge on the display. The display will automatically be shown if it is not already visible.
        /// </summary>
        /// <param name="displayGauge"></param>
        public void ShowDisplayGauge(S3DDisplayGauge displayGauge)
        {
            if (displayGauge != null) { ShowOrHideGauge(displayGauge, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayGauge - Could not find gauge - parameter was null"); }
            #endif
        }

        /// <summary>
        /// Show or turn on all Display Gauges. StickyDisplayModule must be initialised.
        /// The display will automatically be shown if it is not already visible.
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
            if (displayGaugeList == null) { displayGaugeList = new List<S3DDisplayGauge>(1); }
        }

        #endregion

        #region Public API Methods - Messages

        /// <summary>
        /// Add a new message to the display. By design, they are not visible at runtime when first added.
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="messageText"></param>
        /// <returns></returns>
        public S3DDisplayMessage AddMessage(string messageName, string messageText)
        {
            S3DDisplayMessage displayMessage = null;

            Canvas canvas = GetCanvas;
            hudRectTfrm = GetDisplayPanel();

            if (canvas == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: StickyDisplayModule.AddMessage() - could not find canvas for the dipslay. Did you use the prefab from Prefabs/Visuals folder?");
                #endif
            }
            else if (hudRectTfrm == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: StickyDisplayModule.AddMessage() - could not find DisplayPanel for the display. Did you use the prefab from Prefabs/Visuals folder?");
                #endif
            }
            else
            {
                displayMessage = new S3DDisplayMessage();
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
        /// Add a message to the display using a displayMessage instance. Typically, this is used
        /// with CopyDisplayMessage(..).
        /// </summary>
        /// <param name="displayMessage"></param>
        public void AddMessage(S3DDisplayMessage displayMessage)
        {
            if (displayMessage != null)
            {
                RectTransform hudRectTfrm = S3DUtils.GetChildRectTransform(transform, displayPanelName, this.name);

                if (hudRectTfrm == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: StickyDisplayModule.AddMessage() - could not find DisplayPanel for the display. Did you use the prefab from Prefabs/Visuals folder?");
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
        /// Delete a message from the display.
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
        /// Create a copy of an existing S3DDisplayMessage, and give it a new name.
        /// Call AddMessage(newDisplayMessage) to make it useable in the game.
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="NameOfCopy"></param>
        /// <returns></returns>
        public S3DDisplayMessage CopyDisplayMessage(S3DDisplayMessage displayMessage, string NameOfCopy)
        {
            S3DDisplayMessage dmCopy = new S3DDisplayMessage(displayMessage);

            if (dmCopy != null)
            {
                // make it unique
                dmCopy.guidHash = S3DMath.GetHashCodeFromGuid();
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
        /// Get a S3DDisplayMessage given its guidHash.
        /// See also GetDisplayMessageGuidHash(..).
        /// Will return null if guidHash parameter is 0, it cannot be found.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public S3DDisplayMessage GetDisplayMessage(int guidHash)
        {
            S3DDisplayMessage displayMessage = null;
            S3DDisplayMessage _tempDM = null;

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
        public S3DDisplayMessage GetDisplayMessage(string displayMessageName)
        {
            S3DDisplayMessage displayMessage = null;
            S3DDisplayMessage _tempDM = null;

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
        /// After adding or moving S3DDisplayMessages, they may need to be sorted to
        /// have the correct z-order in on the display.
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
                    S3DDisplayMessage displayMessage = displayMessageList[dmIdx];

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
        /// Set the offset (position) of the Display Message on the display.
        /// If the module has been initialised, this will also re-position the Display Message.
        /// </summary>
        /// <param name="offsetX">Horizontal offset from centre. Range between -1 and 1</param>
        /// <param name="offsetY">Vertical offset from centre. Range between -1 and 1</param>
        public void SetDisplayMessageOffset(S3DDisplayMessage displayMessage, float offsetX, float offsetY)
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
                    CheckDisplayResize(false);
                    UnityEditor.Undo.RecordObject(dmTrfm, string.Empty);
                }
                #endif

                // Use original refResolution rather than the scaled size of the display. This works in and out of play mode
                //tempMessageOffset.x = displayMessage.offsetX * (refResolutionHalf.x - (displayMessage.displayWidth / 2f));
                //tempMessageOffset.y = displayMessage.offsetY * (refResolutionHalf.y - (displayMessage.displayHeight / 2f));

                // Use the scaled display size. This works in and out of play mode
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
        public void SetDisplayMessageSize(S3DDisplayMessage displayMessage, float width, float height)
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
                if (editorMode) { CheckDisplayResize(false); }
                #endif

                RectTransform dmRectTfrm = displayMessage.CachedMessagePanel;    

                if (dmRectTfrm != null)
                {
                    // Here we use the original (reference) display size as it doesn't need to be scaled in any way.
                    // The parent display canvas is correctly scaled and sized to the actual monitor or device display.
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
        public void SetDisplayMessageText(S3DDisplayMessage displayMessage, string messageText)
        {
            if (displayMessage != null)
            {
                displayMessage.messageString = messageText;
                Text uiText = displayMessage.CachedTextComponent;

                if (uiText != null) { uiText.text = messageText; }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayMessageText - displayMessage is null"); }
            #endif
        }

        /// <summary>
        /// Update the position of the text within the message panel
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="textAlignment"></param>
        public void SetDisplayMessageTextAlignment(S3DDisplayMessage displayMessage, TextAnchor textAlignment)
        {
            if (displayMessage != null)
            {
                if (!editorMode) { displayMessage.textAlignment = textAlignment; }

                Text uiText = displayMessage.CachedTextComponent;

                if (uiText != null) { uiText.alignment = textAlignment; }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayMessageTextAlignment - displayMessage is null"); }
            #endif
        }

        /// <summary>
        /// Set the font of the S3DDisplayMessage Text component
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="font"></param>
        public void SetDisplayMessageTextFont(S3DDisplayMessage displayMessage, Font font)
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
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayMessageTextFont - displayMessage is null"); }
            #endif
        }

        /// <summary>
        /// Set the font size of the display message text. If isBestFit is false, maxSize is the font size set.
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="isBestFit"></param>
        /// <param name="minSize"></param>
        /// <param name="maxSize"></param>
        public void SetDisplayMessageTextFontSize(S3DDisplayMessage displayMessage, bool isBestFit, int minSize, int maxSize)
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
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayMessageTextFontSize - displayMessage is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Message background colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the message background with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayMessageBackgroundColour(S3DDisplayMessage displayMessage, Color newColour)
        {
            if (displayMessage != null)
            {
                S3DUtils.UpdateColour(ref newColour, ref displayMessage.backgroundColour, ref displayMessage.baseBackgroundColour, true);

                SetDisplayMessageBackgroundBrightness(displayMessage);
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayMessageBackgroundColour - displayMessage is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Message scroll direction.
        /// USAGE: SetDisplayMessageScrollDirection(displayMessage, S3DDisplayMessage.ScrollDirectionLR)
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="scrollDirection"></param>
        public void SetDisplayMessageScrollDirection(S3DDisplayMessage displayMessage, int scrollDirection)
        {
            if (displayMessage != null)
            {
                if (scrollDirection >= 0 && scrollDirection < 5)
                {
                    displayMessage.scrollDirectionInt = scrollDirection;
                    displayMessage.scrollDirection = (S3DDisplayMessage.ScrollDirection)scrollDirection;
                    if (scrollDirection == S3DDisplayMessage.ScrollDirectionNone)
                    {
                        displayMessage.scrollOffsetX = 0f;
                        displayMessage.scrollOffsetY = 0f;
                        SetDisplayMessageOffset(displayMessage, displayMessage.offsetX, displayMessage.offsetY);
                    }
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayMessageScrollDirection - displayMessage is null"); }
            #endif
        }

        /// <summary>
        /// Set the Display Message to scroll across or up/down the full screen regardless of the message width and height.
        /// Can also be set directly with displayMessage.isScrollFullscreen = true;
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="isScrollFullscreen"></param>
        public void SetDisplayMessageScrollFullscreen(S3DDisplayMessage displayMessage, bool isScrollFullscreen)
        {
            if (displayMessage != null) { displayMessage.isScrollFullscreen = isScrollFullscreen; }
        }

        /// <summary>
        /// Set the Display Message scroll speed.
        /// Can also be set directly with displayMessage.scrollSpeed = scrollSpeed;
        /// </summary>
        /// <param name="displayMessage"></param>
        /// <param name="scrollSpeed"></param>
        public void SetDisplayMessageScrollSpeed(S3DDisplayMessage displayMessage, float scrollSpeed)
        {
            if (displayMessage != null) { displayMessage.scrollSpeed = scrollSpeed; }
        }

        /// <summary>
        /// Set the Display Message text colour. Only update the colour if it has actually changed.
        /// If the module has been initialised, this will also re-colour the message text with the appropriate brightness.
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDisplayMessageTextColour(S3DDisplayMessage displayMessage, Color newColour)
        {
            if (displayMessage != null)
            {
                S3DUtils.UpdateColour(ref newColour, ref displayMessage.textColour, ref displayMessage.baseTextColour, true);

                SetDisplayMessageTextBrightness(displayMessage);
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayMessageTextColour - displayMessage is null"); }
            #endif
        }

        /// <summary>
        /// Show the Display Message on the display. The display will automatically be shown if it is not already visible.
        /// </summary>
        /// <param name="guidHash"></param>
        public void ShowDisplayMessage(int guidHash)
        {
            S3DDisplayMessage displayMessage = GetDisplayMessage(guidHash);
            if (displayMessage != null) { ShowOrHideMessage(displayMessage, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayMessage - Could not find message with guidHash: " + guidHash); }
            #endif
        }

        /// <summary>
        /// Show the Display Message on the display. The display will automatically be shown if it is not already visible.
        /// </summary>
        /// <param name="displayMessage"></param>
        public void ShowDisplayMessage(S3DDisplayMessage displayMessage)
        {
            if (displayMessage != null) { ShowOrHideMessage(displayMessage, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayMessage - Could not find message - parameter was null"); }
            #endif
        }

        /// <summary>
        /// Show or turn on all Display Messages. StickyDisplayModule must be initialised.
        /// The display will automatically be shown if it is not already visible.
        /// </summary>
        public void ShowDisplayMessages()
        {
            ShowOrHideMessages(true);
        }

        /// <summary>
        /// Show the Display Message background on the display. The display the actual message, you would
        /// need to call ShowDisplayMessage(..).
        /// </summary>
        /// <param name="displayMessage"></param>
        public void ShowDisplayMessageBackground(S3DDisplayMessage displayMessage)
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
            S3DDisplayMessage displayMessage = GetDisplayMessage(guidHash);
            if (displayMessage != null) { ShowOrHideMessage(displayMessage, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayMessage - Could not find message with guidHash: " + guidHash); }
            #endif
        }

        /// <summary>
        /// Hide or turn off the Display Message
        /// </summary>
        /// <param name="displayMessage"></param>
        public void HideDisplayMessage(S3DDisplayMessage displayMessage)
        {
            if (displayMessage != null) { ShowOrHideMessage(displayMessage, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayMessage - Could not find message - parameter was null"); }
            #endif
        }

        /// <summary>
        /// Hide or turn off all Display Messages.
        /// StickyDisplayModule must be initialised.
        /// </summary>
        public void HideDisplayMessages()
        {
            ShowOrHideMessages(false);
        }

        /// <summary>
        /// Hide the Display Message background on the display. The hide the actual message, you would
        /// need to call HideDisplayMessage(..).
        /// </summary>
        /// <param name="displayMessage"></param>
        public void HideDisplayMessageBackground(S3DDisplayMessage displayMessage)
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
            if (displayMessageList == null) { displayMessageList = new List<S3DDisplayMessage>(1); }
        }

        #endregion

        #region Public API Methods - Targets

        /// <summary>
        /// Add a new Display Target to the HUD. By design, they are not visible at runtime when first added.
        /// </summary>
        /// <param name="guidHashDisplayReticle"></param>
        /// <returns></returns>
        public S3DDisplayTarget AddTarget(int guidHashDisplayReticle)
        {
            S3DDisplayTarget displayTarget = null;

            if (GetDisplayPanel() == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: StickyDisplayModule.AddTarget() - could not find HUDPanel for the HUD. Did you use the prefab from Demos/Prefabs/Visuals folder?");
                #endif
            }
            else
            {
                displayTarget = new S3DDisplayTarget();
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
        public S3DDisplayTargetSlot AddTargetSlots(S3DDisplayTarget displayTarget, int numberToAdd)
        {
            S3DDisplayTargetSlot displayTargetSlot = null;

            if (displayTarget != null)
            {
                if (displayTarget.maxNumberOfTargets + numberToAdd <= S3DDisplayTarget.MAX_DISPLAYTARGET_SLOTS)
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
                                if (editorMode) { CheckDisplayResize(false); }
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
                else { Debug.LogWarning("WARNING: stickyDisplayModule.AddTargetSlot - not enough empty DisplayTarget slots"); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.AddTargetSlot - displayTarget is null"); }
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

            S3DDisplayTarget displayTarget = GetDisplayTarget(guidHash);

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
            S3DDisplayTarget displayTarget = GetDisplayTarget(guidHash);

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
                            else { Debug.LogWarning("ERROR: stickyDisplayModule.DeleteTargetSlot - tried to remove initialised slot " + sIdx + " but only " + displayTarget.displayTargetSlotList.Count + " exist."); }
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
                else { Debug.LogWarning("WARNING: stickyDisplayModule.DeleteTargetSlot - cannot delete that many DisplayTarget slots"); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.DeleteTargetSlot - displayTarget cannot be found with guidHash: " + guidHash); }
            #endif
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
        public S3DDisplayTarget GetDisplayTarget(int guidHash)
        {
            S3DDisplayTarget displayTarget = null;
            S3DDisplayTarget _tempDM = null;

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
        public S3DDisplayTarget GetDisplayTargetByIndex(int index)
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
        public string GetDisplayTargetName(S3DDisplayTarget displayTarget)
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
        /// Show the Display Target slots on the HUD. By design, if the HUD is not shown, the Targets will not be show.
        /// </summary>
        /// <param name="guidHash"></param>
        public void ShowDisplayTarget(int guidHash)
        {
            S3DDisplayTarget displayTarget = GetDisplayTarget(guidHash);
            if (displayTarget != null) { ShowOrHideTarget(displayTarget, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayTarget - Could not find displayTarget with guidHash: " + guidHash); }
            #endif
        }

        /// <summary>
        /// Show the Display Target slots on the HUD. By design, if the HUD is not shown, the Targets will not be show.
        /// </summary>
        /// <param name="displayTarget"></param>
        public void ShowDisplayTarget(S3DDisplayTarget displayTarget)
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
        public void ShowDisplayTargetSlot(S3DDisplayTargetSlot displayTargetSlot)
        {
            if (displayTargetSlot != null) { ShowOrHideTargetSlot(displayTargetSlot, true); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayTargetSlot - displayTargetSlot parameter was null"); }
            #endif
        }

        /// <summary>
        /// Show or turn on all Display Targets.
        /// StickyDisplayModule must be initialised.
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
            S3DDisplayTarget displayTarget = GetDisplayTarget(guidHash);
            if (displayTarget != null) { ShowOrHideTarget(displayTarget, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: ShowDisplayTarget - Could not find message with guidHash: " + guidHash); }
            #endif
        }

        /// <summary>
        /// Hide or turn off all slots of the Display Target
        /// </summary>
        /// <param name="displayTarget"></param>
        public void HideDisplayTarget(S3DDisplayTarget displayTarget)
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
        public void HideDisplayTargetSlot(S3DDisplayTargetSlot displayTargetSlot)
        {
            if (displayTargetSlot != null) { ShowOrHideTargetSlot(displayTargetSlot, false); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("WARNING: HideDisplayTargetSlot - displayTargetSlot parameter was null"); }
            #endif
        }

        /// <summary>
        /// Hide or turn off all all slots of all Display Targets.
        /// StickyDisplayModule must be initialised.
        /// </summary>
        public void HideDisplayTargets()
        {
            ShowOrHideTargets(false);
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
                S3DDisplayTarget displayTarget = GetDisplayTarget(guidHash);

                if (displayTarget != null)
                {
                    displayTarget.guidHashDisplayReticle = guidHashDisplayReticle;

                    S3DDisplayReticle _displayReticle = GetDisplayReticle(guidHashDisplayReticle);

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
        public void SetDisplayTargetReticleColour(S3DDisplayTarget displayTarget, Color newColour)
        {
            if (displayTarget != null)
            {
                S3DUtils.UpdateColour(ref newColour, ref displayTarget.reticleColour, ref displayTarget.baseReticleColour, true);

                SetDisplayTargetBrightness(displayTarget);
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayTargetReticleColour - displayTarget is null"); }
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
        public void SetDisplayTargetOffset(S3DDisplayTarget displayTarget, int slotIndex, float offsetX, float offsetY)
        {
            if (displayTarget != null)
            {
                int numSlots = displayTarget.displayTargetSlotList == null ? 0 : displayTarget.displayTargetSlotList.Count;

                if (slotIndex >= 0 & slotIndex < numSlots)
                {
                    SetDisplayTargetOffset(displayTarget.displayTargetSlotList[slotIndex], offsetX, offsetY);
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayTargetOffset - displayTarget is null"); }
            #endif
        }

        /// <summary>
        /// Set the offset (position) of the Display Target slot on the HUD.
        /// If the module has been initialised, this will also re-position the Display Target.
        /// </summary>
        /// <param name="displayTargetSlot"></param>
        /// <param name="offsetX">Horizontal offset from centre. Range between -1 and 1</param>
        /// <param name="offsetY">Vertical offset from centre. Range between -1 and 1</param>
        public void SetDisplayTargetOffset(S3DDisplayTargetSlot displayTargetSlot, float offsetX, float offsetY)
        {
            if (displayTargetSlot != null)
            {
                // Verify the x,y values are within range -1 to 1.
                if (offsetX >= -1f && offsetX <= 1f)
                {
                    displayTargetSlot.offsetX = offsetX;
                }

                if (offsetY >= -1f && offsetY <= 1f)
                {
                    displayTargetSlot.offsetY = offsetY;
                }

                if (IsInitialised || editorMode)
                {
                    Transform dtgtTrfm = displayTargetSlot.CachedTargetPanel.transform;

                    #if UNITY_EDITOR
                    if (editorMode)
                    {
                        CheckDisplayResize(false);
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
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayTargetOffset - displayTargetSlot is null"); }
            #endif
        }

        /// <summary>
        /// Move the DisplayTarget Slot to the correct 2D position on the HUD, based on a 3D world space position.
        /// If the camera has not been automatically or manually assigned, the DisplayTarget will not be moved.
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <param name="slotIndex"></param>
        /// <param name="wsPosition"></param>
        public void SetDisplayTargetPosition(S3DDisplayTarget displayTarget, int slotIndex, Vector3 wsPosition)
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
                else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayTargetPosition - displayTarget is null"); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: stickyDisplayModule.SetDisplayTargetPosition - Could not find mainCamera."); }
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
        public void SetDisplayTargetSize(S3DDisplayTarget displayTarget, int width, int height)
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
                if (editorMode) { CheckDisplayResize(false); }
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
                    S3DDisplayTarget displayTarget = displayTargetList[dtIdx];

                    if (displayTarget != null)
                    {
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
            if (displayTargetList == null) { displayTargetList = new List<S3DDisplayTarget>(1); }
        }

        #endregion

        #region Public API Methods - Special Purpose

        /// <summary>
        /// This is a special method that can be called from the Engage Tab on a S3D character when looking at interactive-enabled
        /// objects in the scene. See the On Look At Changed event on the StickyControlModule for more details. 
        /// </summary>
        /// <param name="StickyID"></param>
        /// <param name="oldLookAtInteractiveID"></param>
        /// <param name="lookingAtInteractiveId"></param>
        /// <param name="reticleColour"></param>
        public void InteractiveLookAtChanged (int StickyID, int oldLookAtInteractiveID, int lookingAtInteractiveId, Color32 reticleColour)
        {
            SetDisplayReticleColour(reticleColour);
        }

        #endregion

        #region Public Static API Methods

        /// <summary>
        /// Get the current active StickyDisplayModule
        /// </summary>
        /// <returns></returns>
        public static StickyDisplayModule GetActiveDisplayModule()
        {
            return activeDisplayModule;
        }

        /// <summary>
        /// Set the active StickyDisplayModule
        /// </summary>
        /// <param name="stickyDisplayModule"></param>
        public static void SetActiveDisplayModule (StickyDisplayModule stickyDisplayModule)
        {
            activeDisplayModule = stickyDisplayModule;
        }

        #endregion
    }
}