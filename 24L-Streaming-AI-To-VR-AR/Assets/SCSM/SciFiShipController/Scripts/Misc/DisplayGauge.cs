using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class containing data for a Ship Display's guage or visual measuring device.
    /// </summary>
    [System.Serializable]
    public class DisplayGauge
    {
        #region Enumerations

        /// <summary>
        /// Equivalent to UI.Image.FillMethod
        /// </summary>
        public enum DGFillMethod
        {
            Horizontal = 0,
            Vertical = 1,
            Radial90 = 2,
            Radial180 = 3,
            Radial360 = 4,
            None = 99
        }

        /// <summary>
        /// The type or style of a gauge
        /// </summary>
        public enum DGType
        {
            Filled = 0,
            FilledNumber1 = 1,
            NumberWithLabel1 = 10
        }

        /// <summary>
        /// The direction as rotation in degrees
        /// </summary>
        public enum DGTextDirection
        {
            Horizontal = 0,
            BottomTop = 1,
            TopBottom = 2
        }

        /// <summary>
        /// Equavalent to UnityEngine.FontStyle
        /// </summary>
        public enum DGFontStyle
        {
            Normal = 0,
            Bold = 1,
            Italic = 2,
            BoldAndItalic = 3
        }

        #endregion

        #region Public Static variables
        public static int DGTypeFilledInt = (int)DGType.Filled;
        public static int DGTypeFilledNumber1Int = (int)DGType.FilledNumber1;
        public static int DGTypeNumberWithLabel1Int = (int)DGType.NumberWithLabel1;
        #endregion

        #region Public variables

        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// The name or description of the gauge. This can be used to identify
        /// the gauge. It is not displayed in the heads-up display.
        /// </summary>
        public string gaugeName;

        /// <summary>
        /// The text to display on the gauge. It can include RichText markup. e.g. <b>Bold Text</b>.
        /// At runtime call shipDisplayModule.SetDisplayGaugeText(..)
        /// </summary>
        public string gaugeString;

        /// <summary>
        /// The label text on a numeric gauge with a label. It can include RichText markup. e.g. <b>Bold Text</b>.
        /// For non-numeric gauges, see gaugeString.
        /// </summary>
        public string gaugeLabel;

        /// <summary>
        /// The current amount or reading on the gauge. Value must be between 0.0 (empty/min) and 1.0 (full/max)
        /// </summary>
        [Range(0f,1f)] public float gaugeValue;

        /// <summary>
        /// The type or style of the gauge. Default is Filled.
        /// </summary>
        public DGType gaugeType;

        /// <summary>
        /// Show (or hide) the gauge. At runtime use shipDisplayModule.ShowDisplayGauge() or HideDisplayGauge().
        /// </summary>
        public bool showGauge;

        /// <summary>
        /// The Display Gauge's normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.
        /// Offset is measured from the centre of the Gauge.
        /// At runtime call shipDisplayModule.SetDisplayGaugeOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float offsetX;

        /// <summary>
        /// The Display Gauge's normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.
        /// Offset is measured from the centre of the Gauge.
        /// At runtime call shipDisplayModule.SetDisplayGaugeOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float offsetY;

        /// <summary>
        /// The Display Gauge's normalised width. 1.0 is full screen width, 0.5 is half width.
        /// At runtime call shipDisplayModule.SetDisplayGaugeSize(..)
        /// </summary>
        [Range(0.01f, 1f)] public float displayWidth;

        /// <summary>
        /// The Display Gauge's normalised height. 1.0 is full screen height, 0.5 is half height.
        /// At runtime call shipDisplayModule.SetDisplayGaugeSize(..)
        /// </summary>
        [Range(0.01f, 1f)] public float displayHeight;

        /// <summary>
        /// Whether the gauge is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// Hashed GUID code to uniquely identify a gauge.
        /// [INTERNAL USE ONLY]
        /// </summary>
        public int guidHash;

        /// <summary>
        /// Does the colour of the foreground change based on the value of the gauge?
        /// At runtime call shipDisplayModule.SetDisplayGaugeValueAffectsColour(..)
        /// </summary>
        public bool isColourAffectByValue;

        /// <summary>
        /// When isColourAffectByValue is true, the value for the foreground medium colour [default: 0.5]
        /// </summary>
        [Range(0.1f, 0.9f)] public float mediumColourValue;

        /// <summary>
        /// Colour of the gauge foreground.
        /// At runtime call shipDisplayModule.SetDisplayGaugeForegroundColour(..)
        /// </summary>
        public Color foregroundColour;

        /// <summary>
        /// High foreground colour when gaugeValue = 1.0
        /// </summary>
        public Color foregroundHighColour;

        /// <summary>
        /// Medium foreground colour when gaugeValue = 0.5
        /// </summary>
        public Color foregroundMediumColour;

        /// <summary>
        /// Low foreground colour when gaugeValue = 0
        /// </summary>
        public Color foregroundLowColour;

        /// <summary>
        /// The sprite (texture) for the foreground of the gauge
        /// </summary>
        public Sprite foregroundSprite;

        /// <summary>
        /// Colour of the gauge background.
        /// At runtime call shipDisplayModule.SetDisplayGaugeBackgroundColour(..)
        /// </summary>
        public Color backgroundColour;

        /// <summary>
        /// The sprite (texture) for the background of the gauge
        /// </summary>
        public Sprite backgroundSprite;

        /// <summary>
        /// Determines the method used to fill the gauge foreground sprite when the gaugeValue is modified.
        /// </summary>
        public DGFillMethod fillMethod;

        /// <summary>
        /// Keep the original aspect ratio of the foreground and background sprites. Useful when creating circular gauges
        /// </summary>
        public bool isKeepAspectRatio;

        /// <summary>
        /// Colour of the gauge text.
        /// At runtime call shipDisplayModule.SetDisplayGaugeTextColour(..).
        /// </summary>
        public Color textColour;

        /// <summary>
        /// The position of the text within the diplay gauge panel.
        /// At runtime call shipDisplayModule.SetDisplayGaugeTextAlignment(..)
        /// </summary>
        public TextAnchor textAlignment;

        /// <summary>
        /// The position of the label within the display gauge panel.
        /// This only applies to numeric gauges with a label.
        /// At runtime call shipDisplayModule.SetDisplayGaugeLabelAlignment(..)
        /// </summary>
        public TextAnchor labelAlignment;

        /// <summary>
        /// The rotation of the text within the display gauge panel.
        /// At runtime call shipDisplayModule.SetDisplayGaugeTextDirection(..)
        /// </summary>
        public DGTextDirection textDirection;

        /// <summary>
        /// The style of the text with the display gauge panel.
        /// At runtime call shipDisplayGaugeTextFontStyle(..)
        /// </summary>
        public DGFontStyle fontStyle;

        /// <summary>
        /// Is the text font size automatically changes within the bounds of fontMinSize and fontMaxSize
        /// to fill the panel?
        /// At runtime call shipDisplayModule.SetDisplayGaugeTextFontSize(..)
        /// </summary>
        public bool isBestFit;

        /// <summary>
        /// When isBestFit is true will use this minimum font size if required.
        /// At runtime call shipDisplayModule.SetDisplayGaugeTextFontSize(..)
        /// </summary>
        public int fontMinSize;

        /// <summary>
        /// The font size. If isBestFit is true, this will be the maximum font size it can use.
        /// At runtime call shipDisplayModule.SetDisplayGaugeTextFontSize(..)
        /// </summary>
        public int fontMaxSize;

        /// <summary>
        /// When a numeric gaugeType is used, this is the number to display when gaugeValue = 1.0.
        /// </summary>
        public float gaugeMaxValue;

        /// <summary>
        /// The number of decimal places to display for numeric gauges
        /// </summary>
        [Range(0,3)] public int gaugeDecimalPlaces;

        /// <summary>
        /// Is the numeric gauge to be displayed as a percentage?
        /// </summary>
        public bool isNumericPercentage;

        #endregion

        #region Public Properties

        /// <summary>
        /// Is this a numeric Gauge with a label.
        /// </summary>
        public bool HasLabel { get { return gaugeType == DGType.NumberWithLabel1; } }

        #endregion

        #region Private or Internal variables and properties - not serialized

        /// <summary>
        /// [INTERNAL ONLY]
        /// Once initialised, the RectTransform of the gauge panel
        /// </summary>
        internal RectTransform CachedGaugePanel { get; set; }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Once initialised, the foreground Image gauge component
        /// </summary>
        internal UnityEngine.UI.Image CachedFgImgComponent { get; set; }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Once initialised, the background Image gauge component
        /// </summary>
        internal UnityEngine.UI.Image CachedBgImgComponent { get; set; }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Once initialised, the gauge Text component
        /// </summary>
        internal UnityEngine.UI.Text CachedTextComponent { get; set; }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Once initialised, the label Text component for numeric gauges
        /// </summary>
        internal UnityEngine.UI.Text CachedLabelTextComponent { get; set; }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used for background brightness
        /// </summary>
        [System.NonSerialized] internal SSCColour baseForegroundColour;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used for background brightness
        /// </summary>
        [System.NonSerialized] internal SSCColour baseBackgroundColour;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used for text brightness
        /// </summary>
        [System.NonSerialized] internal SSCColour baseTextColour;

        /// <summary>
        /// This is cached to avoid the enumeration lookup at runtime
        /// </summary>
        internal bool isFillMethodNone;

        private Transform CachedGaugePanelTfrm { get { return CachedGaugePanel == null ? null : CachedGaugePanel.transform; } }

        #endregion

        #region Constructors
        public DisplayGauge()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// DisplayMessage copy constructor
        /// </summary>
        /// <param name="displayGauge"></param>
        public DisplayGauge(DisplayGauge displayGauge)
        {
            if (displayGauge == null) { SetClassDefaults(); }
            else
            {
                gaugeName = displayGauge.gaugeName;
                gaugeString = displayGauge.gaugeString;
                if (string.IsNullOrEmpty(displayGauge.gaugeLabel)) { displayGauge.gaugeLabel = string.Empty; }
                else { gaugeLabel = string.Copy(displayGauge.gaugeLabel); }
                gaugeValue = displayGauge.gaugeValue;
                gaugeType = displayGauge.gaugeType;
                showGauge = displayGauge.showGauge;
                offsetX = displayGauge.offsetX;
                offsetY = displayGauge.offsetY;
                displayWidth = displayGauge.displayWidth;
                displayHeight = displayGauge.displayHeight;
                showInEditor = displayGauge.showInEditor;
                guidHash = displayGauge.guidHash;
                isColourAffectByValue = displayGauge.isColourAffectByValue;
                mediumColourValue = displayGauge.mediumColourValue;
                foregroundColour = new Color(displayGauge.foregroundColour.r, displayGauge.foregroundColour.g, displayGauge.foregroundColour.b, displayGauge.foregroundColour.a);
                foregroundHighColour = new Color(displayGauge.foregroundHighColour.r, displayGauge.foregroundHighColour.g, displayGauge.foregroundHighColour.b, displayGauge.foregroundHighColour.a);
                foregroundMediumColour = new Color(displayGauge.foregroundMediumColour.r, displayGauge.foregroundMediumColour.g, displayGauge.foregroundMediumColour.b, displayGauge.foregroundMediumColour.a);
                foregroundLowColour = new Color(displayGauge.foregroundLowColour.r, displayGauge.foregroundLowColour.g, displayGauge.foregroundLowColour.b, displayGauge.foregroundLowColour.a);
                foregroundSprite = displayGauge.foregroundSprite;
                backgroundColour = new Color(displayGauge.backgroundColour.r, displayGauge.backgroundColour.g, displayGauge.backgroundColour.b, displayGauge.backgroundColour.a);
                backgroundSprite = displayGauge.backgroundSprite;
                fillMethod = displayGauge.fillMethod;
                isKeepAspectRatio = displayGauge.isKeepAspectRatio;
                textColour = new Color(displayGauge.textColour.r, displayGauge.textColour.g, displayGauge.textColour.b, displayGauge.textColour.a);
                textAlignment = displayGauge.textAlignment;
                labelAlignment = displayGauge.labelAlignment;
                textDirection = displayGauge.textDirection;
                isBestFit = displayGauge.isBestFit;
                fontMinSize = displayGauge.fontMinSize;
                fontMaxSize = displayGauge.fontMaxSize;
                gaugeMaxValue = displayGauge.gaugeMaxValue;
                gaugeDecimalPlaces = displayGauge.gaugeDecimalPlaces;
                isNumericPercentage = displayGauge.isNumericPercentage;

                // Clear cached values
                CachedBgImgComponent = null;
                CachedFgImgComponent = null;
                CachedGaugePanel = null;
                CachedLabelTextComponent = null;
                CachedTextComponent = null;
            }
        }
        #endregion

        #region Public Member Methods

        /// <summary>
        /// Set the defaults values for this class
        /// </summary>
        public void SetClassDefaults()
        {
            gaugeName = string.Empty;
            gaugeString = string.Empty;
            gaugeLabel = string.Empty;
            gaugeValue = 0f;
            gaugeType = DGType.Filled;
            showGauge = false;
            // Default to centre of screen
            offsetX = 0f;
            offsetY = 0.5f;
            displayWidth = 0.25f;
            displayHeight = 0.05f;
            showInEditor = false;
            guidHash = SSCMath.GetHashCodeFromGuid();
            isColourAffectByValue = false;
            mediumColourValue = 0.5f;
            foregroundColour = Color.grey;
            foregroundHighColour = Color.grey;
            foregroundMediumColour = Color.grey;
            foregroundLowColour = Color.grey;
            foregroundSprite = null;
            backgroundColour = Color.white;
            backgroundSprite = null;
            fillMethod = DGFillMethod.Horizontal;
            isKeepAspectRatio = false;
            textColour = Color.black;
            textAlignment = TextAnchor.MiddleLeft;
            labelAlignment = TextAnchor.UpperLeft;
            textDirection = DGTextDirection.Horizontal;
            isBestFit = true;
            fontMinSize = 10;
            fontMaxSize = 36;
            gaugeMaxValue = 100f;
            gaugeDecimalPlaces = 0;
            isNumericPercentage = false;
        }

        #endregion
    }
}