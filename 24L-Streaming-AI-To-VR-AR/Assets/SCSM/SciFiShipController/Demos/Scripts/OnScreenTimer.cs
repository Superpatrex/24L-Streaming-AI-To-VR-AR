using UnityEngine;
using UnityEngine.UI;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// On-screen timer.
    /// NOTE: This will inpact Garbage Collection (GC) as the on-screen text is updated
    /// with stringbuilder.
    /// </summary>
    public class OnScreenTimer : MonoBehaviour
    {
        #region Public Variables and Properties
        public bool initialiseOnAwake = true;
        public float dayLength = 86400f;                 // Day length in seconds
        public Color screenClockTextColour = new Color(240f / 255f, 240f / 255f, 240f / 255f, 33f / 255f);
        public bool useScreenClockSeconds = true;

        // Screen clock defaults
        public Color GetDefaultScreenClockTextColour { get { return new Color(240f / 255f, 240f / 255f, 240f / 255f, 33f / 255f); } }

        #endregion

        #region Private variables
        private bool isInitialised = false;
        private Canvas timerCanvas = null;
        private float dayTimer = 0f;
        private float screenClockUpdateTimer = 0f;
        private float screenClockUpdateInterval = 60f;      // # seconds between updates
        private float simTimeRealTimeRatio = 1f;            // How many seconds pass in simulated time to every real time second?
        private RectTransform screenClockPanel;
        private Text screenClockText;
        #endregion

        #region Initialisation Methods

        // Use this for initialization
        void Awake()
        {
            if (initialiseOnAwake) { Initialise(); }
        }

        /// <summary>
        /// WARNING: This will affect GC as it does string comparisons
        /// </summary>
        public void Initialise()
        {
            isInitialised = false;
            ValidateOnScreenTimer();

            // How many seconds pass in simulated time to every real time second?
            simTimeRealTimeRatio = 86400 / dayLength;

            // Determine how often to update the clock
            screenClockUpdateInterval = useScreenClockSeconds ? 1f : 60f;

            if (screenClockText != null)
            {
                screenClockText.text = CurrentTimeString(useScreenClockSeconds);
                UpdateScreenClockColour();
                isInitialised = true;
            }
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            dayTimer += Time.deltaTime;
            if (isInitialised && screenClockText != null)
            {
                // Clock displays hours and minutes. So need no to update every frame
                screenClockUpdateTimer += Time.deltaTime * simTimeRealTimeRatio;
                if (screenClockUpdateTimer > screenClockUpdateInterval)
                {
                    screenClockUpdateTimer = 0f;
                    screenClockText.text = CurrentTimeString(useScreenClockSeconds);
                }
            }
        }

        #endregion

        #region Private Methods

        private void ValidateOnScreenTimer()
        {
            if (timerCanvas == null)
            {
                // Attempt to find the canvas
                GameObject timerCanvasGO = null;

                #if UNITY_2022_2_OR_NEWER
                Canvas[] canvasArray = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                #else
                Canvas[] canvasArray = FindObjectsOfType<Canvas>();
                #endif

                // ArrayUtility.Find only works in editor
                int numCanvas = canvasArray == null ? 0 : canvasArray.Length;
                for (int cvsIdx = 0; cvsIdx < numCanvas; cvsIdx++)
                {
                    if (canvasArray[cvsIdx].name == "OnScreenTimerCanvas") { timerCanvas = canvasArray[cvsIdx]; break; }
                }

                // If OnScreenTimer canvas doesn't exist, create it
                if (timerCanvas == null)
                {
                    timerCanvasGO = new GameObject("OnScreenTimerCanvas");
                    timerCanvasGO.layer = 5;
                    timerCanvasGO.AddComponent<Canvas>();

                    timerCanvas = timerCanvasGO.GetComponent<Canvas>();
                    timerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    timerCanvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                    timerCanvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
            }

            if (timerCanvas != null && screenClockPanel == null)
            {
                if (screenClockPanel == null)
                {
                    RectTransform[] rectTransforms = timerCanvas.GetComponentsInChildren<RectTransform>(true);
                    if (rectTransforms != null)
                    {
                        foreach (RectTransform rt in rectTransforms)
                        {
                            if (rt.name == "ClockPanel")
                            {
                                screenClockPanel = rt;
                                break;
                            }
                        }
                    }

                    // If the clock panel doesn't exist, create it
                    if (screenClockPanel == null)
                    {
                        // Add the clock panel as a child of the Canvas
                        GameObject screenClockPanelGameObject = new GameObject("ClockPanel");
                        if (screenClockPanelGameObject != null)
                        {
                            screenClockPanelGameObject.transform.parent = timerCanvas.transform;
                            screenClockPanel = screenClockPanelGameObject.AddComponent<RectTransform>();

                            // Configure clock Panel rect transform
                            if (screenClockPanel != null)
                            {
                                // Place the panel in the lower right corner
                                screenClockPanel.anchorMin = new Vector2(0.8f, 0f);
                                screenClockPanel.anchorMax = new Vector2(0.97f, 0.15f);
                                screenClockPanel.sizeDelta = Vector2.zero;
                                screenClockPanel.anchoredPosition = Vector2.zero;
                                screenClockPanel.localScale = Vector3.one;
                            }

                            // Add the UI Text component as a child of the ClockPanel
                            GameObject screenClockTextGameObject = new GameObject("ClockText");
                            if (screenClockTextGameObject != null)
                            {
                                screenClockTextGameObject.transform.parent = screenClockPanelGameObject.transform;

                                // Add and configure the clock Text RectTransform
                                RectTransform screenClockTextRectTranform = screenClockTextGameObject.AddComponent<RectTransform>();
                                if (screenClockTextRectTranform != null)
                                {
                                    screenClockTextRectTranform.anchorMin = Vector2.zero;
                                    screenClockTextRectTranform.anchorMax = Vector2.one;
                                    screenClockTextRectTranform.sizeDelta = Vector2.zero;
                                    screenClockTextRectTranform.anchoredPosition = Vector2.zero;
                                    screenClockTextRectTranform.localScale = Vector3.one;

                                    // Add and configure the clock Text
                                    screenClockText = screenClockTextGameObject.AddComponent<Text>();
                                    if (screenClockText != null)
                                    {
                                        // Configure the UI Text
                                        screenClockText.font = SSCUtils.GetDefaultFont();
                                        screenClockText.raycastTarget = false;
                                        screenClockText.resizeTextForBestFit = true;
                                        screenClockText.alignment = TextAnchor.MiddleRight;
                                        screenClockText.text = "00:00";
                                    }
                                }
                            }
                        }
                    }

                    if (screenClockPanel != null && screenClockText == null)
                    {
                        screenClockText = timerCanvas.GetComponentInChildren<Text>(true);
                    }
                }

            }
        }

        /// <summary>
        /// Returns a formatted time string in HH:MM or HH:MM:SS
        /// </summary>
        /// <param name="includeSeconds"></param>
        /// <returns></returns>
        private string CurrentTimeString(bool includeSeconds = false)
        {
            float timeOfDay = (CurrentTime() / dayLength) * 24f;
            float minutes = (timeOfDay % 1f) * 60f;
            if (includeSeconds)
            {
                int minsInt = (int)System.Math.Floor(minutes);

                return FormatTime((int)System.Math.Floor(timeOfDay), minsInt, (int)((minutes - minsInt) * 60f));
            }
            else
            {
                return FormatTime((int)System.Math.Floor(timeOfDay), (int)System.Math.Floor(minutes));
            }
        }

        /// <summary>
        /// Returns the time given the hours, minutes, and seconds
        /// Displays -- if the values exceed their limits.
        /// For the sake of performance, assumes all values are +ve.
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="secs"></param>
        /// <returns></returns>
        private string FormatTime(int hours, int minutes, int secs)
        {
            System.Text.StringBuilder sf = new System.Text.StringBuilder(15);

            // Deal with most common scenarios first
            if (hours == 0) { sf.Append("00"); }
            else if (hours < 10) { sf.Append("0"); sf.Append(hours); }
            else if (hours < 99) { sf.Append(hours); }
            else { sf.Append("--"); }
            sf.Append(":");

            if (minutes > 59) { sf.Append("--"); }
            else if (minutes > 9) { sf.Append(minutes); }
            else { sf.Append("0"); sf.Append(minutes); }
            sf.Append(":");

            if (secs > 59) { sf.Append("--"); }
            else if (secs > 9) { sf.Append(secs); }
            else { sf.Append("0"); sf.Append(secs); }

            return sf.ToString();
        }

        /// <summary>
        /// Returns the time given the hours and minutes
        /// Displays -- if the values exceed their limits.
        /// For the sake of performance, assumes all values are +ve.
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <returns></returns>
        private string FormatTime(int hours, int minutes)
        {
            System.Text.StringBuilder sf = new System.Text.StringBuilder(6);

            if (hours == 0) { sf.Append("00"); }
            else if (hours < 10) { sf.Append("0"); sf.Append(hours); }
            else if (hours < 99) { sf.Append(hours); }
            else { sf.Append("--"); }
            sf.Append(":");

            if (minutes > 59) { sf.Append("--"); }
            else if (minutes > 9) { sf.Append(minutes); }
            else { sf.Append("0"); sf.Append(minutes); }

            return sf.ToString();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the time in seconds from the start of the current day
        /// </summary>
        /// <returns>The time.</returns>
        public float CurrentTime()
        {
            return dayTimer % dayLength;
        }

        public void UpdateScreenClockColour()
        {
            if (screenClockText != null)
            {
                try
                {
                    screenClockText.color = screenClockTextColour;
                }
                catch (System.Exception ex)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("OnScreenTimer.UpdateScreenClockColour " + ex.Message);
                    #else
                    if (ex != null) { }
                    #endif
                }
            }
        }

        #endregion
    }
}