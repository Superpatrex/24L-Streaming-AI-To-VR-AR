using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// A centralised futuristic radar system based on Automatic Dependent Surveillance - Broadcast (ADS-B).
    /// Instead of the radar system doing a sweep of the environment, craft which need situational
    /// awareness broadcast (send) data to the central radar system.
    /// This system will be the Primary Radar System (PRS).
    /// This implementation requires all craft to query the radar system (incoming broadcasts are private
    /// and don't get sent to every craft - the radar system receives private msgs from ships).
    /// </summary>
    // [AddComponentMenu("Sci-Fi Ship Controller/Managers/Radar")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SSCRadar : MonoBehaviour
    {
        #region Static Read-only valiables
        public static readonly int NEUTRAL_FACTION = 0;
        #endregion

        #region Enumerations

        public enum RadarScreenLocale : int
        {
            TopLeft = 0,
            TopCenter = 1,
            TopRight = 2,
            MiddleLeft = 3,
            MiddleCenter = 4,
            MiddleRight = 5,
            BottomLeft = 6,
            BottomCenter = 7,
            BottomRight = 8,
            Custom = 99
        };

        #endregion

        #region Public variables and properties

        /// <summary>
        /// If enabled, the GetOrCreateRadar() will be called as soon as Start() runs. If there is a UI (mini-map) configured,
        /// it will automatically be made visible. This should be disabled if you are instantiating the SSCRadar through code
        /// and using the SSCRadar API methods.
        /// </summary>
        public bool initialiseOnStart = false;

        /// <summary>
        /// [READONLY] Has the radar been initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        public int poolInitialSize = 100;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// </summary>
        public int poolIncrementSize = 10;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// </summary>
        public bool allowRepaint = false;
        public bool generalShowInEditor = false;
        public bool visualsShowInEditor = false;
        public bool movementShowInEditor = false;
        public RadarScreenLocale screenLocale = RadarScreenLocale.TopLeft;
        public Vector2 screenLocaleCustomXY = Vector2.zero;
        // Normalised width as 0.0-1.0 as a proportion of the screen space.
        // e.g. 0.2 would be 20% of the screen width.
        [Range(0.1f, 1f)] public float radarDisplayWidthN = 0.2f;

        /// <summary>
        /// The sort order of the canvas in the scene. Higher numbers are on top.
        /// At runtime call SetCanvasSortOrder(..)
        /// </summary>
        public int canvasSortOrder = 1;

        /// <summary>
        /// When the built-in UI is used, this is the colour of the outer rim
        /// of the mini-map display along with any decals.
        /// </summary>
        public Color32 overlayColour = Color.white;

        /// <summary>
        /// When the built-in UI is used, this is the background colour of the
        /// mini-map.
        /// </summary>
        public Color32 backgroundColour = Color.clear;

        /// <summary>
        /// When the built-in UI is used, this is the colour of any blip that are considered
        /// as friendly. Determined by the factionId when available
        /// </summary>
        public Color32 blipFriendColour = Color.green;

        /// <summary>
        /// When the built-in UI is used, this is the colour of any blip that are considered
        /// as hostile. Determined by the factionId when available
        /// </summary>
        public Color32 blipFoeColour = Color.red;

        /// <summary>
        /// When the built-in UI is used, this is the colour of any blip that are considered
        /// as neutral. Determined by the factionId when available.
        /// </summary>
        public Color32 blipNeutralColour = Color.white;

        /// <summary>
        /// If changing this at runtime, call RefreshRadarImageStatus().
        /// </summary>
        public UnityEngine.UI.RawImage radarImage = null;

        /// <summary>
        /// The number of results returned in the last query.
        /// </summary>
        public int ResultCount { get; private set; }

        /// <summary>
        /// Uses 3D distances to determine range when querying the radar data.
        /// </summary>
        public bool is3DQueryEnabled = true;

        /// <summary>
        /// The sort order of the results. None is the fastest option and has
        /// the lowest performance impact.
        /// </summary>
        public SSCRadarQuery.QuerySortOrder querySortOrder;

        /// <summary>
        /// [READONLY] The direction the on-screen UI display is facing
        /// </summary>
        public Quaternion DisplayRotation { get { return displayUIRotation; } }

        /// <summary>
        /// [INTERNAL ONLY] Instead call SetDisplay(..) or GetRadarResults(..)
        /// Minimum range is 10 metres
        /// </summary>
        public float displayRange = 100f;

        /// <summary>
        /// [INTERNAL ONLY] Use FollowShip(..) instead.
        /// The centre of the radar will move around with this ship. 
        /// </summary>
        public ShipControlModule shipToFollow = null;

        /// <summary>
        /// [INTERNAL ONLY] Use FollowGameObject(..) instead.
        /// The centre of the radar will move around with this gameobject
        /// </summary>
        public GameObject gameobjectToFollow = null;

        /// <summary>
        /// [INTERNAL ONLY]
        /// The centre of the radar
        /// </summary>
        public Vector3 centrePosition = Vector3.zero;

        #endregion

        #region Public Delegates

        public delegate void CallbackOnDrawBlip(Texture2D tex, Quaternion displayRotation, int factionId, SSCRadarBlip sscRadarBlip);

        /// <summary>
        /// The name of the custom method that is called when a blip is to be
        /// draw on the radar display. Your method must take 4 parameters -
        /// Texture2D, Quaternion, Int, and SSCRadarBlip. Your custom method should
        /// "paint" the blip onto the texture by modifying the pixels.
        /// </summary>
        public CallbackOnDrawBlip callbackOnDrawBlip = null;

        #endregion

        #region Private variables
        private static SSCRadar currentRadar = null;
        private bool isInitialised = false;
        private List<SSCRadarItem> sscRadarItemList = null;
        private int numRadarItems = 0;
        private int currentPoolSize = 0;
        private BitArray radarItemBitArray = null;
        private bool isRadarImageAvailable = false;
        private bool isShowRadarImage = false;
        // The width of the RawImage texture if in use
        private int displayUITexWidth = 10;
        private int displayUITexHeight = 10;
        private int displayUITexCentreX = 5;
        private int displayUITexCentreY = 5;
        private Quaternion displayUIRotation = Quaternion.identity;
        private Vector3 defaultDisplayFwdDirection = Vector3.forward;

        // Used when our UI is enabled
        private bool isFollowShip = false;
        private bool isFollowGameObject = false;
        private const int displayRimWidth = 4;
        private Color32[] uiRimPixels; 
        private Color32[] uiInnerPixels; 

        private SSCRadarQuery sscRadarQuery;
        private List<SSCRadarBlip> sscRadarResultsList;

        #endregion

        #region Initialisation Methods

        /// <summary>
        /// Called after Awake() just before the scene is rendered
        /// </summary>
        private void Start()
        {
            if (initialiseOnStart)
            {
                GetOrCreateRadar();

                // If the UI is available, display it
                if (isInitialised && isRadarImageAvailable)
                {
                    ShowUI();
                }
            }
        }

        /// <summary>
        /// [INTERNAL ONLY] Instead use SSCRadar.GetOrCreateRadar() 
        /// </summary>
        private void Initialise()
        {
            if (sscRadarItemList == null)
            {
                if (poolInitialSize < 1) { sscRadarItemList = new List<SSCRadarItem>(10); }
                else { sscRadarItemList = new List<SSCRadarItem>(poolInitialSize); }
            }
            else { sscRadarItemList.Clear(); }

            // Reset the number items in the last query
            ResultCount = 0;

            FillPool();

            RefreshRadarImageStatus();

            isFollowShip = shipToFollow != null;
            if (isFollowShip)
            {
                isFollowGameObject = false;
            }
            else
            {
                isFollowGameObject = gameobjectToFollow != null;
            }

            // By default Radar UI is not visible in the scene
            if (isRadarImageAvailable)
            {
                ScreenResized();
                
                HideUI();
            }

            isInitialised = true;

            SetCanvasSortOrder(canvasSortOrder);
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            if (isShowRadarImage && isInitialised)
            {
                if (isFollowShip)
                {
                    if (shipToFollow != null && shipToFollow.IsInitialised)
                    {
                        centrePosition = shipToFollow.shipInstance.TransformPosition;
                    }
                }
                else if (isFollowGameObject)
                {
                    if (gameobjectToFollow != null) { centrePosition = gameobjectToFollow.transform.position; }
                }

                // Run the query
                sscRadarQuery.centrePosition = centrePosition;
                sscRadarQuery.range = displayRange;
                sscRadarQuery.is3DQueryEnabled = is3DQueryEnabled;
                sscRadarQuery.querySortOrder = querySortOrder;
                sscRadarQuery.factionId = SSCRadarQuery.IGNOREFACTION;
                GetRadarResults(sscRadarQuery, sscRadarResultsList);

                DisplayResults(false);
            }
        }

        #endregion

        #region Private Member Methods

        /// <summary>
        /// Fill the pool with empty radar items. These are added to the end
        /// of the existing pool up to the current capacity.
        /// </summary>
        private void FillPool()
        {
            int capacity = sscRadarItemList == null ? 0 : sscRadarItemList.Capacity;
            numRadarItems = sscRadarItemList == null ? 0 : sscRadarItemList.Count;

            int numNewItems = capacity - numRadarItems;
            if (radarItemBitArray == null) { radarItemBitArray = new BitArray(capacity, true); }

            // Make sure the bit array can hold enough data
            if (radarItemBitArray != null && radarItemBitArray.Length < capacity) { radarItemBitArray.Length = capacity; }

            for (int itemIdx = capacity - numNewItems; itemIdx < capacity; itemIdx++)
            {
                // Create a new (empty) slot
                sscRadarItemList.Add(new SSCRadarItem());
                // Mark this slot as empty
                radarItemBitArray[itemIdx] = true;
            }

            // Cache number of items
            numRadarItems = sscRadarItemList == null ? 0 : sscRadarItemList.Count;

            currentPoolSize = numRadarItems;
        }

        /// <summary>
        /// If required, increase the size of the pool
        /// </summary>
        private void ExpandPool()
        {
            if (isInitialised)
            {
                currentPoolSize = sscRadarItemList == null ? 0 : sscRadarItemList.Count;

                int capacity = sscRadarItemList == null ? 0 : sscRadarItemList.Capacity;

                // If there is less than poolIncrementSize left at the end of the pool for expansion,
                // add some more capacity
                if (capacity - currentPoolSize < poolIncrementSize)
                {
                    sscRadarItemList.Capacity += poolIncrementSize;
                }

                FillPool();
            }
        }

        /// <summary>
        /// Set a slot in the pool of radarItems as being empty or not.
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="isEmpty"></param>
        private void SetBitmap(int itemIndex, bool isEmpty)
        {
            if (isInitialised && itemIndex >= 0 && itemIndex < currentPoolSize)
            {
                radarItemBitArray[itemIndex] = isEmpty;
            }
        }

        /// <summary>
        /// Draw the background Radar UI, then add the resultant blips
        /// </summary>
        /// <param name="redraw">Force overlay inner and outer redraw</param>
        private void DisplayResults(bool redraw = false)
        {
            int numResults = ResultCount;
            int outerRadius = (int)(displayUITexWidth / 2f);

            Texture2D radarTex = radarImage.texture as Texture2D;

            // If the canvas image has been resized but doesn't match
            // the width of the texture, refresh it.
            if (displayUITexWidth > radarTex.width)
            {
                RefreshRadarImageStatus();
                return;
            }

            // Cache the texture pixels to improve performance and prevent GC in each frame
            DrawCircle(radarTex, ref uiRimPixels, displayUITexCentreX, displayUITexCentreY, outerRadius, overlayColour, true, redraw);
            DrawCircle(radarTex, ref uiInnerPixels, displayUITexCentreX, displayUITexCentreY, outerRadius - displayRimWidth, backgroundColour, true, redraw);

            bool isCallbackEnabled = callbackOnDrawBlip != null;

            int factionId = 0; // neutral

            // Get the direction the central ship or gameobject is facing
            if (isFollowShip && shipToFollow != null)
            {
                if (shipToFollow.shipInstance != null)
                {
                    displayUIRotation = Quaternion.Euler(0f, shipToFollow.shipInstance.TransformInverseRotation.eulerAngles.y, 0f);
                    factionId = shipToFollow.shipInstance.factionId;
                }
                else
                {
                    // Ship may be in the process of being destroyed
                    FollowShip(null);
                    displayUIRotation = Quaternion.LookRotation(defaultDisplayFwdDirection);
                }
            }
            else if (isFollowGameObject && gameobjectToFollow != null)
            {
                displayUIRotation = Quaternion.Euler(0f, Quaternion.Inverse(gameobjectToFollow.transform.rotation).eulerAngles.y, 0f);
            }
            else { displayUIRotation = Quaternion.LookRotation(defaultDisplayFwdDirection); }

            // Populate the UI display with blips
            for (int blipIdx = 0; blipIdx < numResults; blipIdx++)
            {
                if (isCallbackEnabled) { callbackOnDrawBlip(radarTex, displayUIRotation, factionId, sscRadarResultsList[blipIdx]); }
                else { DrawBlip(radarTex, displayUIRotation, factionId, sscRadarResultsList[blipIdx]); }
            }

            // Only apply once all operations have finished
            radarTex.Apply();
            radarImage.texture = radarTex;
        }

        /// <summary>
        /// Draw blips onto the radar UI display texture.
        /// The factionId is the factionId of the item or place running the query. Items with the same factionId
        /// will be displayed in the friend colour, items with factionId = 0 will be neutral, while all other items
        /// will be considered hostile (foe blip colour).
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="displayRotation"></param>
        /// <param name="factionId"></param>
        /// <param name="sscRadarBlip"></param>
        private void DrawBlip(Texture2D tex, Quaternion displayRotation, int factionId, SSCRadarBlip sscRadarBlip)
        {
            Vector3 rotatedPosition = displayRotation * (sscRadarBlip.wsPosition - sscRadarQuery.centrePosition);

            int blipOffsetX = (int)((rotatedPosition.x / sscRadarQuery.range) * displayUITexWidth / 2f);
            int blipOffsetZ = (int)((rotatedPosition.z / sscRadarQuery.range) * displayUITexHeight / 2f);

            Color blipColour = blipNeutralColour;

            //int itemTypeInt = (int)sscRadarBlip.radarItemType;

            // If the item running the query is neutral, all others are assumed friendly...
            // If the blip is not neutral, must be friend or foe
            if (factionId != 0 && sscRadarBlip.factionId != 0)
            {
                // Is this radar item in the same faction or alliance as the radar system
                if (sscRadarBlip.factionId == factionId) { blipColour = blipFriendColour; }
                else { blipColour = blipFoeColour; }
            }

            int blipSize = (int)sscRadarBlip.blipSize;

            // Draw a 3x3, 4x4, 5x5 blip etc
            for (int x = blipOffsetX + displayUITexCentreX - blipSize; x < blipOffsetX + displayUITexCentreX + blipSize; x++)
            {
                for (int y = blipOffsetZ + displayUITexCentreY - blipSize; y < blipOffsetZ + displayUITexCentreY + blipSize; y++)
                {
                    // Clip with display rim width indent
                    if (x > displayRimWidth + 1 && x < displayUITexWidth - displayRimWidth - 2 && y > displayRimWidth + 1 && y < displayUITexHeight - displayRimWidth - 2)
                    {
                        tex.SetPixel(x, y, blipColour);
                    }
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// This method uses SetPixels which may be slower on some devices. It has very minimal GC.
        /// If you pass in a Color32 as a parameter, it will be converted to a Color struct.
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="centreX"></param>
        /// <param name="centreY"></param>
        /// <param name="circleRadius"></param>
        /// <param name="pixelColour"></param>
        /// <param name="isFilled"></param>
        /// <param name="apply"></param>
        public void DrawCircle(Texture2D tex, int centreX, int centreY, int circleRadius, Color pixelColour, bool isFilled, bool apply)
        {
            for (int x = -circleRadius; x < circleRadius; x++)
            {
                int dist = (int)Mathf.Ceil(Mathf.Sqrt(circleRadius * circleRadius - x * x));

                if (isFilled)
                {
                    for (int y = -dist; y < dist; y++)
                    {
                        tex.SetPixel(x + centreX, y + centreY, pixelColour);
                    }
                }
                else
                {
                    tex.SetPixel(x + centreX, -dist + centreY, pixelColour);
                    tex.SetPixel(x + centreX, dist + centreY, pixelColour);

                    if (x < -circleRadius + 4 || x > circleRadius - 6)
                    {
                        for (int i = 0; i < 16 && i < circleRadius / 6f; i++)
                        {
                            tex.SetPixel(x + centreX, -dist + i + centreY, pixelColour);
                            tex.SetPixel(x + centreX, dist - i + centreY, pixelColour);
                        }
                    }
                }
            }
            if (apply) { tex.Apply(); }
        }

        /// <summary>
        /// Uses a cached copy of the pixels for each circle that needs to be regularly drawn. This is useful when wanting
        /// to update a Texture2D in the UI very regularly (like each frame). The first time it runs for a particular circle
        /// it will create GC but after that there is no GC unless redraw = true.
        /// IMPORTANT: After calling DrawCircle(), call tex.Apply() after you have finished drawing to the texture.
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="pixels"></param>
        /// <param name="centreX"></param>
        /// <param name="centreY"></param>
        /// <param name="circleRadius"></param>
        /// <param name="pixelColour"></param>
        /// <param name="isFilled"></param>
        /// <param name="redraw"></param>
        public void DrawCircle(Texture2D tex, ref Color32[] pixels, int centreX, int centreY, int circleRadius, Color32 pixelColour, bool isFilled, bool redraw)
        {
            if (redraw || pixels == null)
            {
                pixels = tex.GetPixels32();

                // Draw the circle
                for (int x = -circleRadius; x < circleRadius; x++)
                {
                    // NOTE: Mathf.Ceil is almost twice as slow as Mathf.Sqrt
                    int dist = (int)Mathf.Ceil(Mathf.Sqrt(circleRadius * circleRadius - x * x));

                    if (isFilled)
                    {
                        for (int y = -dist; y < dist; y++)
                        {
                            pixels[(y + centreY) * tex.width + x + centreX] = pixelColour;
                        }
                    }
                    else
                    {
                        pixels[(-dist + centreY) * tex.width + x + centreX] = pixelColour;
                        pixels[(dist + centreY) * tex.width + x + centreX] = pixelColour;

                        if (x < -circleRadius + 4 || x > circleRadius - 6)
                        {
                            for (int i = 0; i < 16 && i < circleRadius / 6f; i++)
                            {
                                pixels[(-dist + i + centreY) * tex.width + x + centreX] = pixelColour;
                                pixels[(dist - i + centreY) * tex.width + x + centreX] = pixelColour;
                            }
                        }
                    }
                }
            }
            tex.SetPixels32(pixels);
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Get the anchor points and correct rect transform offset for the radar display screen locale. e.g. BottomLeft, TopRight etc.
        /// </summary>
        /// <param name="screenLocaleInt"></param>
        /// <param name="canvasSize"></param>
        /// <param name="panelSize"></param>
        /// <param name="anchorMin"></param>
        /// <param name="anchorMax"></param>
        /// <param name="panelOffset"></param>
        public void GetMinimapScreenLocation(int screenLocaleInt, Vector2 canvasSize, Vector2 panelSize, ref Vector2 anchorMin, ref Vector2 anchorMax, ref Vector2 panelOffset)
        {
            float indentX = canvasSize.x * 0.01f;
            float indentY = canvasSize.y * 0.01f;

            switch (screenLocaleInt)
            {
                case (int)RadarScreenLocale.BottomLeft:
                    anchorMin.x = 0f; anchorMin.y = 0f; anchorMax.x = 0f; anchorMax.y = 0f;
                    panelOffset.x += indentX;
                    panelOffset.y += indentY;
                    break;
                case (int)RadarScreenLocale.BottomCenter:
                    anchorMin.x = 0.5f; anchorMin.y = 0f; anchorMax.x = 0.5f; anchorMax.y = 0f;
                    panelOffset.x += (canvasSize.x * 0.5f) - (panelSize.x * 0.5f);
                    panelOffset.y += indentY;
                    break;
                case (int)RadarScreenLocale.BottomRight:
                    anchorMin.x = 1f; anchorMin.y = 0f; anchorMax.x = 1f; anchorMax.y = 0f;
                    panelOffset.x += canvasSize.x - panelSize.x - indentX;
                    panelOffset.y += indentY;
                    break;
                case (int)RadarScreenLocale.TopRight:
                    anchorMin.x = 0f; anchorMin.y = 0f; anchorMax.x = 1f; anchorMax.y = 1f;
                    panelOffset.x += canvasSize.x - panelSize.x - indentX;
                    panelOffset.y += canvasSize.y - panelSize.y - indentY;
                    break;
                case (int)RadarScreenLocale.TopCenter:
                    anchorMin.x = 0.5f; anchorMin.y = 0f; anchorMax.x = 0.5f; anchorMax.y = 0f;
                    panelOffset.x += (canvasSize.x * 0.5f) - (panelSize.x * 0.5f);
                    panelOffset.y += canvasSize.y - panelSize.y - indentY;
                    break;
                case (int)RadarScreenLocale.TopLeft:
                    anchorMin.x = 0f; anchorMin.y = 1f; anchorMax.x = 0f; anchorMax.y = 1f;
                    panelOffset.x += indentX;
                    panelOffset.y += canvasSize.y - panelSize.y - indentY;
                    break;
                case (int)RadarScreenLocale.MiddleLeft:
                    anchorMin.x = 0f; anchorMin.y = 0.5f; anchorMax.x = 0; anchorMax.y = 0.5f;
                    panelOffset.x += indentX;
                    panelOffset.y += (canvasSize.y * 0.5f) - (panelSize.y * 0.5f);
                    break;
                case (int)RadarScreenLocale.MiddleCenter:
                    anchorMin.x = 0f; anchorMin.y = 0.5f; anchorMax.x = 0f; anchorMax.y = 0.5f;
                    panelOffset.x += (canvasSize.x * 0.5f) - (panelSize.x * 0.5f);
                    panelOffset.y += (canvasSize.y * 0.5f) - (panelSize.y * 0.5f);
                    break;
                case (int)RadarScreenLocale.MiddleRight:
                    anchorMin.x = 1f; anchorMin.y = 0.5f; anchorMax.x = 1f; anchorMax.y = 0.5f;
                    panelOffset.x += canvasSize.x - panelSize.x - indentX;
                    panelOffset.y += (canvasSize.y * 0.5f) - (panelSize.y * 0.5f);
                    break;
                default:
                    // custom
                    anchorMin.x = 0f; anchorMin.y = 0f; anchorMax.x = 0f; anchorMax.y = 0f;
                    panelOffset.x = screenLocaleCustomXY.x;
                    panelOffset.y = screenLocaleCustomXY.y;
                    break;
            }
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Get the Radar canvas if it exists, or create a new one.
        /// NOTE: When creating a new canvas before Unity 2019.3, the sizeDelta returns
        /// the incorrect canvas size. The correct value isn't returned until the next frame.
        /// At runtime does NOT create an instance of the EventSystem - you would need to
        /// do this some other way...
        /// </summary>
        /// <param name="radarCanvasGO"></param>
        /// <param name="radarCanvas"></param>
        /// <param name="canvasSize"></param>
        /// <param name="canvasScale"></param>
        public void GetorCreateRadarCanvas(out GameObject radarCanvasGO, out Canvas radarCanvas, out Vector2 canvasSize, out Vector3 canvasScale)
        {
            radarCanvasGO = null;
            canvasSize = Vector2.zero;
            canvasScale = Vector3.one;

            radarCanvas = SSCUtils.FindCanvas("SSCRadarCanvas");

            // If SSCRadarCanvas doesn't exist, create it
            if (radarCanvas == null)
            {
                radarCanvasGO = new GameObject("SSCRadarCanvas");
                radarCanvasGO.layer = 5;
                radarCanvasGO.AddComponent<Canvas>();

                radarCanvas = radarCanvasGO.GetComponent<Canvas>();
                radarCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                UnityEngine.UI.CanvasScaler canvasScaler = radarCanvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                if (canvasScaler != null)
                {
                    canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    canvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
                    canvasScaler.matchWidthOrHeight = 0.5f;
                }
            }

            #if UNITY_EDITOR
            // Add an Event System if it doesn't already exist.
            // NOTE: If it is disabled in scene, a new one isn't added.
            UnityEditor.EditorApplication.ExecuteMenuItem("GameObject/UI/Event System");
            #endif

            if (radarCanvas != null)
            {
                radarCanvasGO = radarCanvas.gameObject;
                canvasSize = radarCanvasGO.GetComponent<RectTransform>().sizeDelta;
                canvasScale = radarCanvasGO.GetComponent<RectTransform>().localScale;
            }
        }

        #endregion

        #region Public API Static Methods

        /// <summary>
        /// Attempt to find the first SSCRadar in the scene.
        /// Typically you should be using GetOrCreateRadar().
        /// </summary>
        /// <returns></returns>
        public static SSCRadar FindFirstRadar()
        {
            #if UNITY_2022_2_OR_NEWER
            return GameObject.FindFirstObjectByType<SSCRadar>();
            #else
            return GameObject.FindObjectOfType<SSCRadar>();
            #endif
        }

        /// <summary>
        /// Returns the current SSCRadar instance for this scene. If one does not already exist, a new one is created.
        /// USAGE: SSCRadar sscRadar = SSCRadar.GetOrCreateRadar();
        /// </summary>
        /// <returns></returns>
        public static SSCRadar GetOrCreateRadar()
        {
            // Check whether we have already found a radar instance for this scene
            if (currentRadar == null)
            {
                // Otherwise, check whether this scene already has a radar system
                currentRadar = FindFirstRadar();

                if (currentRadar == null)
                {
                    // If this scene does not already have a manager, create one
                    GameObject newRadarGameObject = new GameObject("SSC Radar");
                    newRadarGameObject.transform.position = Vector3.zero;
                    newRadarGameObject.transform.parent = null;
                    currentRadar = newRadarGameObject.AddComponent<SSCRadar>();
                }
            }

            if (currentRadar != null)
            {
                // Initialise the radar if it hasn't already been initialised
                if (!currentRadar.isInitialised) { currentRadar.Initialise(); }
            }
            #if UNITY_EDITOR
            // If currentRadar is still null, log a warning to the console
            else
            {
                Debug.LogWarning("SSCRadar GetOrCreateRadar() Warning: Could not find or create radar, so returned null.");
            }
            #endif

            return currentRadar;
        }

        #endregion

        #region Public API Member Methods - CORE

        /// <summary>
        /// Attempt to add a gameobject to the radar system. If added, it will be immediately visible
        /// to radar and the RadarId (itemIndex) will be return. A value of -1 indicates it wasn't added.
        /// NOTE: To add a ship to radar, use shipControlModule.EnableRadar(). For Locations use
        /// sscManager.EnableRadar(location).
        /// </summary>
        /// <param name="gameObjectToAdd"></param>
        /// <param name="position">typically the position of the gameobject</param>
        /// <param name="factionId"></param>
        /// <param name="squadronId"></param>
        /// <param name="guidHash">The unique hash to identify this gameObject. If it is 0 we will set it to gameObjectToAdd.GetHashCode()</param>
        /// <param name="blipSize">The relative size it appears to be on the radar mini-map. Acceptable values 1 to 5</param>
        /// <returns></returns>
        public int EnableRadar(GameObject gameObjectToAdd, Vector3 position, int factionId, int squadronId, int guidHash, int blipSize)
        {
            // Not assigned in the radar system
            int radarItemIndex = -1;

            if (gameObjectToAdd != null && isInitialised)
            {
                SSCRadarItem sscRadarItem = new SSCRadarItem();
                sscRadarItem.radarItemType = SSCRadarItem.RadarItemType.GameObject;
                sscRadarItem.itemGameObject = gameObjectToAdd;
                sscRadarItem.isVisibleToRadar = true;
                sscRadarItem.guidHash = guidHash == 0 ? gameObjectToAdd.GetHashCode() : guidHash;
                sscRadarItem.position = position;
                sscRadarItem.factionId = factionId;
                sscRadarItem.squadronId = squadronId;
                sscRadarItem.blipSize = blipSize > 0 && blipSize < 6 ? (byte)blipSize : (byte)1;

                radarItemIndex = AddItem(sscRadarItem);
            }
            return radarItemIndex;
        }

        /// <summary>
        /// The item will no longer be visible in the radar system.
        /// If you want to change the visibility to other radar
        /// consumers, consider changing the radar item data rather
        /// than disabling the radar and (later) calling EnableRadar again.
        /// </summary>
        /// <param name="itemIndex">This is the value returned from sscRadar.EnableRadar(gameObject...)</param>
        public void DisableRadar(int itemIndex)
        {
            if (isInitialised)
            {
                RemoveItem(itemIndex);
            }
        }

        /// <summary>
        /// Add an item to be tracked by radar. Items are not visible to radar initially, instead
        /// they are only made visible (if set to be so) for ships in ShipControlModule.Update(), 
        /// or sscManager.EnableRadar(..) for Locations or sscRadar.EnableRadar(..) for gameobjects. 
        /// </summary>
        /// <param name="sscRadarItemInput"></param>
        /// <returns></returns>
        public int AddItem(SSCRadarItem sscRadarItemInput)
        {
            int itemIndex = -1;

            // Find first available empty slots in sscRadarItemList using the bitmap
            for (int i = 0; i < currentPoolSize; i++)
            {
                // Is this an empty slot?
                if (radarItemBitArray[i]) { itemIndex = i; break; }
            }

            // If the current pool has been exhausted, add some more capacity
            if (itemIndex < 0)
            {
                int originalPoolSize = currentPoolSize;
                ExpandPool();
                // If the pool has been expanded, allocate the next (empty) slot that was added
                if (currentPoolSize > originalPoolSize) { itemIndex = originalPoolSize; }
            }

            if (itemIndex >= 0)
            {
                // Remove this from the available empty slots
                SetBitmap(itemIndex, false);

                if (sscRadarItemInput == null)
                {
                    // This should never be null.
                    SSCRadarItem sscRadarItem = sscRadarItemList[itemIndex];

                    sscRadarItem.SetClassDefaults();
                    sscRadarItem.isVisibleToRadar = false;
                }
                else
                {
                    SSCRadarItem sscRadarItem = sscRadarItemList[itemIndex];

                    // Set rarely changing fields
                    sscRadarItem.radarItemType = sscRadarItemInput.radarItemType;
                    sscRadarItem.itemGameObject = sscRadarItemInput.itemGameObject;
                    sscRadarItem.shipControlModule = sscRadarItemInput.shipControlModule;
                    sscRadarItem.guidHash = sscRadarItemInput.guidHash;
                    sscRadarItem.blipSize = sscRadarItemInput.blipSize;
                    sscRadarItem.IncrementSequenceNumber();

                    // Set frequently changing fields
                    UpdateItem(itemIndex, new SSCRadarPacket(sscRadarItemInput));
                }
            }

            return itemIndex;
        }

        /// <summary>
        /// Remove a single item from the Radar
        /// </summary>
        /// <param name="itemIndex"></param>
        public void RemoveItem(int itemIndex)
        {
            // Return item to available pool by setting isEmpty to true
            SetBitmap(itemIndex, true);
        }

        /// <summary>
        /// Remove all items from the Radar
        /// </summary>
        public void RemoveItemsAll()
        {
            if (isInitialised)
            {
                // All slots should be empty (true)
                radarItemBitArray.SetAll(true);
            }
        }

        /// <summary>
        /// Get an active (assigned) radar item.
        /// For ships and locations, the itemIndex is stored as RadarId. e.g. shipControlModule.shipInstance.RadarId
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <returns></returns>
        public SSCRadarItem GetRadarItem(int itemIndex)
        {
            // Is the itemIndex within the valid range?
            if (isInitialised && itemIndex >= 0 && itemIndex < currentPoolSize)
            {
                // Check to see if the slot is assigned
                if (!radarItemBitArray[itemIndex]) { return sscRadarItemList[itemIndex]; }
                else { return null; }
            }
            else { return null; }
        }

        /// <summary>
        /// Get an active (assigned) radar item given the itemIndex AND its sequenceNumber.
        /// The sequence number is used to validate that this is the correct radarItem.
        /// For ships and locations, the itemIndex is stored as RadarId. e.g. shipControlModule.shipInstance.RadarId
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="sequenceNumber"></param>
        /// <returns></returns>
        public SSCRadarItem GetRadarItem(int itemIndex, uint sequenceNumber)
        {
            // Is the itemIndex within the valid range?
            if (isInitialised && itemIndex >= 0 && itemIndex < currentPoolSize)
            {
                // Check to see if the slot is assigned and it matches the sequence number
                if (!radarItemBitArray[itemIndex] && sscRadarItemList[itemIndex].itemSequenceNumber == sequenceNumber) { return sscRadarItemList[itemIndex]; }
                else { return null; }
            }
            else { return null; }
        }

        /// <summary>
        /// Get an active (assigned) radar item given the SSCRadarItemKey.
        /// For ships and locations, the sscRadarItemKey.radarItemIndex is stored as RadarId. e.g. shipControlModule.shipInstance.RadarId
        /// </summary>
        /// <param name="sscRadarItemKey"></param>
        /// <returns></returns>
        public SSCRadarItem GetRadarItem(SSCRadarItemKey sscRadarItemKey)
        {
            // Is the itemIndex within the valid range?
            if (isInitialised && sscRadarItemKey.radarItemIndex >= 0 && sscRadarItemKey.radarItemIndex < currentPoolSize)
            {
                // Check to see if the slot is assigned and it matches the sequence number
                if (!radarItemBitArray[sscRadarItemKey.radarItemIndex] && sscRadarItemList[sscRadarItemKey.radarItemIndex].itemSequenceNumber == sscRadarItemKey.radarItemSequenceNumber)
                {
                    return sscRadarItemList[sscRadarItemKey.radarItemIndex];
                }
                else { return null; }
            }
            else { return null; }
        }

        /// <summary>
        /// Location radarItemTypes store a unique guidHash.
        /// GameObject radarItemTypes by default store a Unity hash code.
        /// Ship Damage Regions radarItemTypes store the damage region unique guidHash.
        /// For these three types, when the itemIndex (or RadarId) is unknown, this method
        /// can be used to retrieve it.
        /// If there are no matches, -1 is returned.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public int GetRadarItemIndexByHash(int guidHash)
        {
            int itemIndex = -1;

            // Unset guidHash is 0, so ignore this.
            if (guidHash != 0)
            {
                for (int iIdx = 0; iIdx < currentPoolSize; iIdx++)
                {
                    // Check to see if the slot is assigned AND the hashes match
                    if (!radarItemBitArray[iIdx] && sscRadarItemList[iIdx].guidHash == guidHash)
                    {
                        itemIndex = iIdx;
                        break;
                    }
                }
            }

            return itemIndex;
        }

        /// <summary>
        /// Does this item appear in Radar queries? If the itemIndex is invalid, this method
        /// will always return false.
        /// NOTE: For performance reasons, it doesn't check if this is non-empty radar item.
        /// For ships and locations, the itemIndex is stored as RadarId. e.g. shipControlModule.shipInstance.RadarId
        /// </summary>
        /// <param name="itemIndex"></param>
        public bool GetVisibility (int itemIndex)
        {
            // Is the itemIndex within the valid range?
            if (isInitialised && itemIndex >= 0 && itemIndex < currentPoolSize)
            {
                return sscRadarItemList[itemIndex].isVisibleToRadar;
            }
            else { return false; }
        }

        /// <summary>
        /// Set if this item should appear in Radar queries?
        /// NOTE: For performance reasons, it doesn't check if this is non-empty radar item.
        /// For ships and locations, the itemIndex is stored as RadarId. e.g. shipControlModule.shipInstance.RadarId
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="isVisibleToRadar"></param>
        public void SetVisibility(int itemIndex, bool isVisibleToRadar)
        {
            // Is the itemIndex within the valid range?
            if (isInitialised && itemIndex >= 0 && itemIndex < currentPoolSize)
            {
                sscRadarItemList[itemIndex].isVisibleToRadar = isVisibleToRadar;
            }
        }

        /// <summary>
        /// Set a new world space position for a radar item. See also UpdateItem(..).
        /// NOTE: For performance reasons, it doesn't check if this is non-empty radar item.
        /// For ships and locations, the itemIndex is stored as RadarId. e.g. locationData.RadarId
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="wsPosition"></param>
        public void SetItemPosition(int itemIndex, Vector3 wsPosition)
        {
            // Is the itemIndex within the valid range?
            if (isInitialised && itemIndex >= 0 && itemIndex < currentPoolSize)
            {
                sscRadarItemList[itemIndex].position = wsPosition;
            }
        }

        /// <summary>
        /// Set the type of the radarItem. Typically, this will only be performed once for
        /// each object (e.g. a ship or ground installation)
        /// NOTE: For performance reasons, it doesn't check if this is non-empty radar item.
        /// For ships, the itemIndex is stored as RadarId. e.g. shipControlModule.shipInstance.RadarId
        /// For surfaceTurretModule, the itemIndex is stored as RadarId
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="radarItemType"></param>
        public void SetItemType(int itemIndex, SSCRadarItem.RadarItemType radarItemType)
        {
            if (isInitialised && itemIndex >= 0 && itemIndex < currentPoolSize)
            {
                sscRadarItemList[itemIndex].radarItemType = radarItemType;
            }
        }

        /// <summary>
        /// Set the blip size of the radarItem. Typically this is set automatically when a ship or 
        /// Location is enabled for radar. However, this lets the blip size be overridden at runtime.
        /// NOTE: For performance reasons, it doesn't check if this is non-empty radar item.
        /// For ships, the itemIndex is stored as RadarId. e.g. shipControlModule.shipInstance.RadarId
        /// For surfaceTurretModule, the itemIndex is stored as RadarId
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="blipSize"></param>
        public void SetBlipSize (int itemIndex, byte blipSize)
        {
            if (isInitialised && itemIndex >= 0 && itemIndex < currentPoolSize)
            {
                if (blipSize < 1) { sscRadarItemList[itemIndex].blipSize = 1; }
                if (blipSize > 5) { sscRadarItemList[itemIndex].blipSize = 5; }
                else { sscRadarItemList[itemIndex].blipSize = blipSize; }
            }
        }

        /// <summary>
        /// Set the factionId of the radarItem.
        /// NOTE: For performance reasons, it doesn't check if this is non-empty radar item.
        /// For ships, the itemIndex is stored as RadarId. e.g. shipControlModule.shipInstance.RadarId
        /// For surfaceTurretModule, the itemIndex is stored as RadarId
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="factionId"></param>
        public void SetFactionId (int itemIndex, int factionId)
        {
            if (isInitialised && itemIndex >= 0 && itemIndex < currentPoolSize)
            {
                sscRadarItemList[itemIndex].factionId = factionId;
            }
        }

        /// <summary>
        /// Set the squadronId of the radarItem.
        /// NOTE: For performance reasons, it doesn't check if this is non-empty radar item.
        /// For ships, the itemIndex is stored as RadarId. e.g. shipControlModule.shipInstance.RadarId
        /// For surfaceTurretModule, the itemIndex is stored as RadarId
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="squadronId"></param>
        public void SetSquardronId (int itemIndex, int squadronId)
        {
            if (isInitialised && itemIndex >= 0 && itemIndex < currentPoolSize)
            {
                sscRadarItemList[itemIndex].squadronId = squadronId;
            }
        }

        /// <summary>
        /// Send information about a ship or object to the radar system.
        /// NOTE: For performance reasons, it doesn't check if this is non-empty radar item.
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="sscRadarPacket"></param>
        public void UpdateItem(int itemIndex, SSCRadarPacket sscRadarPacket)
        {
            // Is the itemIndex within the valid range?
            if (isInitialised && itemIndex >= 0 && itemIndex < currentPoolSize && sscRadarPacket != null)
            {
                // This should never be null.
                SSCRadarItem sscRadarItem = sscRadarItemList[itemIndex];

                // Update the radar system with the latest data from the source
                sscRadarItem.position = sscRadarPacket.position;
                sscRadarItem.velocity = sscRadarPacket.velocity;
                sscRadarItem.isVisibleToRadar = sscRadarPacket.isVisibleToRadar;
                sscRadarItem.factionId = sscRadarPacket.factionId;
                sscRadarItem.squadronId = sscRadarPacket.squadronId;
            }
        }


        #endregion

        #region Public API Member Methods - UI

        /// <summary>
        /// Redraw the results
        /// </summary>
        public void RefreshResults()
        {
            if (isShowRadarImage && isInitialised)
            {
                DisplayResults(true);
            }
        }

        /// <summary>
        /// Set the world space position of the radar system. Use when
        /// you want the radar system to "move" with gameobject.
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="range"></param>
        public void SetDisplay(GameObject centre, float range)
        {
            gameobjectToFollow = centre;
            displayRange = range;
        }

        /// <summary>
        /// Set the world space position of the radar system. Use when
        /// you want the radar system to be fixed to a given location or
        /// will move infrequently.
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="range"></param>
        public void SetDisplay(Vector3 centre, float range)
        {
            gameobjectToFollow = null;
            centrePosition = centre;
            displayRange = range;
        }

        /// <summary>
        /// If the UI has been configured in the Editor under Visuals,
        /// show the radar display on the screen.
        /// </summary>
        public void ShowUI()
        {
            // Only show the UI if it is available
            isShowRadarImage = isRadarImageAvailable;

            if (isShowRadarImage) { radarImage.gameObject.SetActive(true); }
        }

        /// <summary>
        /// If the UI has been configured in the Editor under Visuals,
        /// hide the radar display.
        /// </summary>
        public void HideUI()
        {
            isShowRadarImage = false;

            if (isRadarImageAvailable)
            {
                radarImage.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// The centre of the radar will follow the ship.
        /// Currently this only works when the built-in UI is configured in the
        /// editor under Visuals. The same can be achieve via the API
        /// when calling GetRadarResults(..) and passing in the ship
        /// TransformPosition.
        /// </summary>
        /// <param name="shipControlModule"></param>
        public void FollowShip (ShipControlModule shipControlModule)
        {
            shipToFollow = shipControlModule;
            isFollowShip = shipToFollow != null;
            if (isFollowShip)
            {
                gameobjectToFollow = null;
                isFollowGameObject = false;
            }
        }

        /// <summary>
        /// The centre of the radar will follow the gameobject.
        /// Currently this only works when the built-in UI is configured in the
        /// editor under Visuals. The same can be achieve via the API
        /// when calling GetRadarResults(..) and passing in the tranform
        /// position.
        /// </summary>
        /// <param name="gameobject"></param>
        public void FollowGameObject(GameObject gameobject)
        {
            gameobjectToFollow = gameobject;
            isFollowGameObject = gameobjectToFollow != null;
            if (isFollowGameObject)
            {
                shipToFollow = null;
                isFollowShip = false;
            }
        }

        /// <summary>
        /// Set the sort order in the scene of the radar mini-map. Higher values appear on top.
        /// </summary>
        /// <param name="newSortOrder"></param>
        public void SetCanvasSortOrder(int newSortOrder)
        {
            canvasSortOrder = newSortOrder;

            if (IsInitialised)
            {
                Canvas _canvas = radarImage == null ? null : radarImage.transform.parent.GetComponent<Canvas>();

                if (_canvas != null) { _canvas.sortingOrder = newSortOrder; }
            }
        }

        /// <summary>
        /// Set the radar mini-map to use a particular display. Displays or monitors are numbered from 1 to 8.
        /// </summary>
        /// <param name="displayNumber">1 to 8</param>
        public void SetCanvasTargetDisplay (int displayNumber)
        {
            if (IsInitialised && SSCUtils.VerifyTargetDisplay(displayNumber, true))
            {
                Canvas _canvas = radarImage == null ? null : radarImage.transform.parent.GetComponent<Canvas>();

                if (_canvas != null) { _canvas.targetDisplay = displayNumber - 1; }
            }
        }

        /// <summary>
        /// Call at runtime if radarImage is swapped or made null
        /// </summary>
        public void RefreshRadarImageStatus()
        {
            isRadarImageAvailable = radarImage != null && radarImage.texture != null;

            if (isRadarImageAvailable)
            {
                sscRadarQuery = new SSCRadarQuery();
                sscRadarResultsList = new List<SSCRadarBlip>(20);
                // There are currently no results.
                ResultCount = 0;
                displayUITexWidth = radarImage.texture.width;
                displayUITexHeight = radarImage.texture.height;
                displayUITexCentreX = Mathf.CeilToInt(displayUITexWidth * 0.5f);
                displayUITexCentreY = Mathf.CeilToInt(displayUITexHeight * 0.5f);
            }
            else
            {
                isShowRadarImage = false;
            }
        }

        /// <summary>
        /// When the screen is resize, we may need to refresh things.
        /// NOTE: Panel offset x/y seem to be incorrect...
        /// </summary>
        public void ScreenResized()
        {
            if (isRadarImageAvailable)
            {
                Canvas radarCanvas = SSCUtils.FindCanvas("SSCRadarCanvas");
                if (radarCanvas != null)
                {
                    GameObject radarCanvasGO = radarCanvas.gameObject;
                    Vector2 canvasSize = radarCanvasGO.GetComponent<RectTransform>().sizeDelta;
                    Vector3 canvasScale = radarCanvasGO.GetComponent<RectTransform>().localScale;

                    Vector2 anchorMin = Vector2.zero;
                    Vector2 anchorMax = Vector2.zero;
                    Vector2 panelOffset = Vector2.zero;

                    float panelWidth = Mathf.Ceil(radarDisplayWidthN * canvasSize.x), panelHeight = panelWidth;

                    GetMinimapScreenLocation((int)screenLocale, canvasSize, new Vector2(panelWidth, panelHeight), ref anchorMin, ref anchorMax, ref panelOffset);

                    int texWidth = Mathf.CeilToInt(panelWidth);
                    int texHeight = Mathf.CeilToInt(panelHeight);

                    Texture2D radarTex = (Texture2D)radarImage.texture;

                    if (radarTex.width != texWidth || radarTex.height != texHeight)
                    {
                        #if !UNITY_2021_2_OR_NEWER
                        radarTex.Resize(texWidth, texHeight);
                        #else
                        radarTex.Reinitialize(texWidth, texHeight);
                        #endif
                    }

                    SSCUtils.UpdateCanvasPanel(radarImage.rectTransform, panelOffset.x, panelOffset.y, panelWidth, panelHeight,
                                                      anchorMin.x, anchorMin.y, anchorMax.x, anchorMax.y, canvasScale);
                    SSCUtils.FillTexture(radarTex, Color.clear, false);
                }
            }
        }

        #endregion

        #region Public API Member Methods - Query

        /// <summary>
        /// Send a query to the radar system and populate a list of matching items.
        /// The queryResultsList must be a non-null, empty list.
        /// A query will never return items with isVisibleToRadar = false.
        /// TODO - consider items that are close in 2D but far in 3D space
        /// FILTERING ORDER:
        /// - Range
        /// - Factions to Exclude
        /// - FactionId
        /// - Squadrons to Exclude
        /// - Factions to Include
        /// - Squadrons to Include
        /// </summary>
        /// <param name="sscRadarQuery"></param>
        /// <param name="queryResultsList"></param>
        /// <returns></returns>
        public bool GetRadarResults (SSCRadarQuery sscRadarQuery, List<SSCRadarBlip> queryResultsList)
        {
            if (sscRadarQuery != null && queryResultsList != null && sscRadarQuery.range > 0f)
            {
                // Loop through the list of items and see which ones match the query
                queryResultsList.Clear();

                float sqrRange = sscRadarQuery.range * sscRadarQuery.range;
                float sqrDistance2D = 0f, sqrDistance3D = 0f;
                float deltaX, deltaY, deltaZ;
                int factionId, squadronId;

                // factionsToInclude is only considered if sscRadarQuery.factionId is not set.
                int numFactionsToInclude = sscRadarQuery.factionId != SSCRadarQuery.IGNOREFACTION || sscRadarQuery.factionsToInclude == null ? 0 : sscRadarQuery.factionsToInclude.Length;

                int numFactionsToExclude = sscRadarQuery.factionsToExclude == null ? 0 : sscRadarQuery.factionsToExclude.Length;

                int numSquadronsToInclude = sscRadarQuery.squadronsToInclude == null ? 0 : sscRadarQuery.squadronsToInclude.Length;
                int numSquadronsToExclude = sscRadarQuery.squadronsToExclude == null ? 0 : sscRadarQuery.squadronsToExclude.Length;

                for (int itemIndex = 0; itemIndex < currentPoolSize; itemIndex++)
                {
                    // Make sure slot isn't empty
                    if (!radarItemBitArray[itemIndex])
                    {
                        SSCRadarItem sscRadarItem = sscRadarItemList[itemIndex];

                        if (sscRadarItem.isVisibleToRadar)
                        {
                            deltaX = sscRadarItem.position.x - sscRadarQuery.centrePosition.x;
                            deltaY = sscRadarItem.position.y - sscRadarQuery.centrePosition.y;
                            deltaZ = sscRadarItem.position.z - sscRadarQuery.centrePosition.z;

                            sqrDistance2D = (deltaX * deltaX) + (deltaZ * deltaZ);
                            sqrDistance3D = sqrDistance2D + (deltaY * deltaY);

                            // Is this item within the range of the radar?
                            if ((sscRadarQuery.is3DQueryEnabled && sqrDistance3D <= sqrRange) || (!sscRadarQuery.is3DQueryEnabled && sqrDistance2D <= sqrRange))
                            {
                                // Moving the sscRadarItem outside the Array.IndexOf/Exists() reduces GC
                                factionId = sscRadarItem.factionId;
                                squadronId = sscRadarItem.squadronId;

                                // Avoid GC by using Array.IndexOf( ) > -1 rather than Array.Exists(..)
                                // Skip anything in the array of factions to exclude
                                if (numFactionsToExclude > 0 && System.Array.IndexOf(sscRadarQuery.factionsToExclude, factionId) > -1) { continue; }

                                // Does the faction match the query?
                                if (sscRadarQuery.factionId != SSCRadarQuery.IGNOREFACTION && sscRadarQuery.factionId != factionId) { continue; }

                                // Skip anything in the array of squadrons to exclude
                                if (numSquadronsToExclude > 0 && System.Array.IndexOf(sscRadarQuery.squadronsToExclude, squadronId) > -1) { continue; }

                                // Skip any factions not in the array of factions to include
                                if (numFactionsToInclude > 0 && System.Array.IndexOf(sscRadarQuery.factionsToInclude, factionId) < 0) { continue; }

                                // Skip any squadron not in the array of squadrons to include
                                if (numSquadronsToInclude > 0 && System.Array.IndexOf(sscRadarQuery.squadronsToInclude, squadronId) < 0) { continue; }

                                // Blips are structs rather than classes. These are created on the stack rather than in heap memory.
                                // Although, these are in a List... but generate no garbage and don't impact GC.
                                SSCRadarBlip blip = new SSCRadarBlip();
                                blip.wsPosition = sscRadarItem.position;
                                blip.radarItemType = sscRadarItem.radarItemType;

                                blip.distanceSqr2D = sqrDistance2D;
                                blip.distanceSqr3D = sqrDistance3D;

                                blip.itemGameObject = sscRadarItem.itemGameObject;
                                blip.shipControlModule = sscRadarItem.shipControlModule;

                                blip.factionId = sscRadarItem.factionId;
                                blip.squadronId = sscRadarItem.squadronId;
                                blip.blipSize = sscRadarItem.blipSize;
                                blip.radarItemIndex = itemIndex;
                                blip.radarItemSequenceNumber = sscRadarItem.itemSequenceNumber;

                                // Locations, GameObjects and Ship DamageRegions store a unique identifier to assist with lookups.
                                // For other types, the guidHash is 0.
                                blip.guidHash = sscRadarItem.guidHash;

                                //int radarItemTypeInt = (int)sscRadarItem.radarItemType;
                                // If this is a Location, set the guidHash so that Locations can be looked up using sscManager.GetLocation(blip.guidHash)
                                // GameObjects can store gameObject.GetHashCode() in this field.
                                //if (radarItemTypeInt == SSCRadarItem.RadarItemTypeLocationInt || radarItemTypeInt == SSCRadarItem.RadarItemTypeGameObjectInt)
                                //{
                                //    blip.guidHash = sscRadarItem.guidHash;
                                //}

                                queryResultsList.Add(blip);
                            }
                        }
                    }
                }
            }
            else { return false; }

            int querySortOrderInt = (int)sscRadarQuery.querySortOrder;

            // Quickly bypass if there is no sort order set.
            if (querySortOrderInt > 0)
            {
                if (querySortOrderInt == SSCRadarQuery.querySortOrderDistanceAsc2DInt)
                {
                    queryResultsList.Sort(delegate (SSCRadarBlip blip1, SSCRadarBlip blip2)
                    {
                        // If x > y return 1, if x < y return -1,  if x == y return 0 (this is equivalent to x.CompareTo(y))

                        // NOTE: CompareTo is slightly faster than if statements to return 1, -1 or 0 - but can impact GC
                        // Ascending order
                        if (blip1.distanceSqr2D > blip2.distanceSqr2D) { return 1; }
                        else if (blip1.distanceSqr2D < blip2.distanceSqr2D) { return -1; }
                        // With floats, x == y is less likely, so do last.
                        else { return 0; }
                    }
                    );
                }
                else if (querySortOrderInt == SSCRadarQuery.querySortOrderDistanceAsc3DInt)
                {
                    queryResultsList.Sort(delegate (SSCRadarBlip blip1, SSCRadarBlip blip2)
                    {
                        // Ascending order
                        if (blip1.distanceSqr3D > blip2.distanceSqr3D) { return 1; }
                        else if (blip1.distanceSqr3D < blip2.distanceSqr3D) { return -1; }
                        else { return 0; }
                    }
                    );
                }
                else if (querySortOrderInt == SSCRadarQuery.querySortOrderDistanceDesc2DInt)
                {
                    queryResultsList.Sort(delegate (SSCRadarBlip blip1, SSCRadarBlip blip2)
                    {
                        // Descending order
                        if (blip1.distanceSqr2D < blip2.distanceSqr2D) { return 1; }
                        else if (blip1.distanceSqr2D > blip2.distanceSqr2D) { return -1; }
                        else { return 0; }
                    }
                    );
                }
                else if (querySortOrderInt == SSCRadarQuery.querySortOrderDistanceDesc3DInt)
                {
                    queryResultsList.Sort(delegate (SSCRadarBlip blip1, SSCRadarBlip blip2)
                    {
                        // Descending order
                        if (blip1.distanceSqr3D < blip2.distanceSqr3D) { return 1; }
                        else if (blip1.distanceSqr3D > blip2.distanceSqr3D) { return -1; }
                        else { return 0; }
                    }
                    );
                }
            }

            ResultCount = queryResultsList == null ? 0 : queryResultsList.Count;

            return true;
        }

        /// <summary>
        /// Get the next blip in the supplied list, that is within the camera's viewing area. The startIndex
        /// is the zero-based index to begin searching the supplied list.
        /// If a match is found, the zero-based index of that blip will be returned, else -1 will be returned.
        /// NOTE: Currently assumes the camera is full screen.
        /// </summary>
        /// <param name="blipList"></param>
        /// <param name="startIndex"></param>
        /// <param name="camera"></param>
        /// <param name="viewSize">This is usually the screen resolution</param>
        /// <returns></returns>
        public int GetNextBlipInView(List<SSCRadarBlip> blipList, int startIndex, Camera camera, Vector2 viewSize)
        {
            int blipIndex = -1;

            int numBlips = blipList == null ? 0 : blipList.Count;

            for (int blpIdx = startIndex; blpIdx < numBlips; blpIdx++)
            {
                if (SSCUtils.PointViewDirection(camera, blipList[blpIdx].wsPosition, viewSize) == SSCUtils.ViewDirection.InFront)
                {
                    blipIndex = blpIdx;
                    break;
                }
            }

            return blipIndex;
        }

        /// <summary>
        /// Get the next blip in the supplied list, that is in front of the camera within a custom 2D viewport.
        /// The startIndex is the zero-based index to begin searching the supplied list.
        /// If a match is found, the zero-based index of that blip will be returned, else -1 will be returned.
        /// </summary>
        /// <param name="blipList"></param>
        /// <param name="startIndex"></param>
        /// <param name="camera"></param>
        /// <param name="viewSize">This is usually the screen resolution</param>
        /// <param name="viewportSize">The width and height as 0.0-1.0 values of the full viewSize</param>
        /// <param name="viewportOffset">-1.0 to 1.0 with 0,0 as the centre of the screen</param>
        /// <returns></returns>
        public int GetNextBlipinScreenViewPort(List<SSCRadarBlip> blipList, int startIndex, Camera camera, Vector2 viewSize, Vector2 viewportSize, Vector2 viewportOffset)
        {
            int blipIndex = -1;

            int numBlips = blipList == null ? 0 : blipList.Count;

            for (int blpIdx = startIndex; blpIdx < numBlips; blpIdx++)
            {
                if (SSCUtils.IsPointInScreenViewport(camera, blipList[blpIdx].wsPosition, viewSize, viewportSize, viewportOffset))
                {
                    blipIndex = blpIdx;
                    break;
                }
            }

            return blipIndex;
        }

        /// <summary>
        /// Is the blip within the camera's viewing area?
        /// NOTE: Currently assumes the camera is full screen.
        /// </summary>
        /// <param name="radarBlip"></param>
        /// <param name="camera"></param>
        /// <param name="viewSize">This is usually the screen resolution</param>
        /// <returns></returns>
        public bool IsBlipInView(SSCRadarBlip radarBlip, Camera camera, Vector2 viewSize)
        {
            return SSCUtils.PointViewDirection(camera, radarBlip.wsPosition, viewSize) == SSCUtils.ViewDirection.InFront;
        }

        /// <summary>
        /// Is the blip in front of the camera within a custom 2D viewport.
        /// Offset is -1.0 to 1.0 with 0,0 as the centre of the screen. 
        /// </summary>
        /// <param name="radarBlip"></param>
        /// <param name="camera"></param>
        /// <param name="viewSize">This is usually the screen resolution</param>
        /// <param name="viewportSize">The width and height as 0.0-1.0 values of the full viewSize</param>
        /// <param name="viewportOffset">-1.0 to 1.0 with 0,0 as the centre of the screen</param>
        /// <returns></returns>
        public bool IsBlipInScreenViewPort(SSCRadarBlip radarBlip, Camera camera, Vector2 viewSize, Vector2 viewportSize, Vector2 viewportOffset)
        {
            return SSCUtils.IsPointInScreenViewport(camera, radarBlip.wsPosition, viewSize, viewportSize, viewportOffset);
        }

        /// <summary>
        /// Does the radar blip contain information about a GameObject?
        /// </summary>
        /// <param name="radarBlip"></param>
        /// <returns></returns>
        public bool IsGameObjectBlip(SSCRadarBlip radarBlip)
        {
            return radarBlip.itemGameObject != null && radarBlip.radarItemType == SSCRadarItem.RadarItemType.GameObject;
        }

        /// <summary>
        /// Does the radar blip contain information about a Location?
        /// </summary>
        /// <param name="radarBlip"></param>
        /// <returns></returns>
        public bool IsLocationBlip(SSCRadarBlip radarBlip)
        {
            return radarBlip.guidHash != 0 && radarBlip.radarItemType == SSCRadarItem.RadarItemType.Location;
        }

        /// <summary>
        /// Does the radar blip contain information about a ship?
        /// </summary>
        /// <param name="radarBlip"></param>
        /// <returns></returns>
        public bool IsShipBlip(SSCRadarBlip radarBlip)
        {
            return radarBlip.shipControlModule != null && (radarBlip.radarItemType == SSCRadarItem.RadarItemType.AIShip || radarBlip.radarItemType == SSCRadarItem.RadarItemType.PlayerShip);
        }

        /// <summary>
        /// Does the radar blip contain information about a ship damage region?
        /// </summary>
        /// <param name="radarBlip"></param>
        /// <returns></returns>
        public bool IsShipDamageRegionBlip(SSCRadarBlip radarBlip)
        {
            return radarBlip.guidHash != 0 && radarBlip.shipControlModule != null && radarBlip.radarItemType == SSCRadarItem.RadarItemType.ShipDamageRegion;
        }

        #endregion

    }
}