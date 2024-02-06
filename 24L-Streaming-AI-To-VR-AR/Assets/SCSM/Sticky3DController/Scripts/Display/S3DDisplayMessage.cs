using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class containing data for a Sticky Display's informational message.
    /// </summary>
    [System.Serializable]
    public class S3DDisplayMessage
    {
        #region Enumerations
        public enum ScrollDirection
        {
            None = 0,
            LeftRight = 1,
            RightLeft = 2,
            BottomTop = 3,
            TopBottom = 4
        }

        #endregion

        #region Public Static variables

        // To avoid enumeration lookup at runtime
        public static readonly int ScrollDirectionNone = (int)ScrollDirection.None;
        public static readonly int ScrollDirectionLR = (int)ScrollDirection.LeftRight;
        public static readonly int ScrollDirectionRL = (int)ScrollDirection.RightLeft;
        public static readonly int ScrollDirectionBT = (int)ScrollDirection.BottomTop;
        public static readonly int ScrollDirectionTB = (int)ScrollDirection.TopBottom;

        #endregion

        #region Public variables

        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// The name or description of the message. This can be used to identify
        /// the message. It is not displayed in the heads-up display.
        /// </summary>
        public string messageName;

        /// <summary>
        /// The text message to display. It can include RichText markup. e.g. <b>Bold Text</b>.
        /// At runtime call stickyDisplayModule.SetDisplayMessageText(..)
        /// </summary>
        public string messageString;

        /// <summary>
        /// Show (or hide) the message. At runtime use stickyDisplayModule.ShowDisplayMessage() or HideDisplayMessage().
        /// </summary>
        public bool showMessage;

        /// <summary>
        /// The Display Message's normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.
        /// Offset is measured from the centre of the message.
        /// At runtime call stickyDisplayModule.SetDisplayMessageOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float offsetX;

        /// <summary>
        /// The Display Message's normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.
        /// Offset is measured from the centre of the message.
        /// At runtime call stickyDisplayModule.SetDisplayMessageOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float offsetY;

        /// <summary>
        /// The Display Message's normalised width. 1.0 is full screen width, 0.5 is half width.
        /// At runtime call stickyDisplayModule.SetDisplayMessageSize(..)
        /// </summary>
        [Range(0.01f, 1f)] public float displayWidth;

        /// <summary>
        /// The Display Message's normalised height. 1.0 is full screen height, 0.5 is half height.
        /// At runtime call stickyDisplayModule.SetDisplayMessageSize(..)
        /// </summary>
        [Range(0.01f, 1f)] public float displayHeight;

        /// <summary>
        /// Whether the message is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// Hashed GUID code to uniquely identify a message.
        /// [INTERNAL USE ONLY]
        /// </summary>
        public int guidHash;

        /// <summary>
        /// Show the background of the message.
        /// </summary>
        public bool showBackground;

        /// <summary>
        /// Colour of the message background.
        /// At runtime call stickyDisplayModule.SetDisplayMessageBackgroundColour(..)
        /// </summary>
        public Color backgroundColour;

        /// <summary>
        /// Colour of the message text.
        /// At runtime call stickyDisplayModule.SetDisplayMessageTextColour(..).
        /// </summary>
        public Color textColour;

        /// <summary>
        /// The position of the text within the diplay message panel.
        /// At runtime call stickyDisplayModule.SetDisplayMessageTextAlignment(..)
        /// </summary>
        public TextAnchor textAlignment;

        /// <summary>
        /// Is the text font size automatically changes within the bounds of fontMinSize and fontMaxSize
        /// to fill the panel?
        /// At runtime call stickyDisplayModule.SetDisplayMessageTextFontSize(..)
        /// </summary>
        public bool isBestFit;

        /// <summary>
        /// When isBestFit is true will use this minimum font size if required.
        /// At runtime call stickyDisplayModule.SetDisplayMessageTextFontSize(..)
        /// </summary>
        public int fontMinSize;

        /// <summary>
        /// The font size. If isBestFit is true, this will be the maximum font size it can use.
        /// At runtime call stickyDisplayModule.SetDisplayMessageTextFontSize(..)
        /// </summary>
        public int fontMaxSize;

        /// <summary>
        /// The direction (if any) the text should scroll across the screen.
        /// At runtime call stickyDisplayModule.SetDisplayMessageScrollDirection(..)
        /// </summary>
        public ScrollDirection scrollDirection;

        /// <summary>
        /// Speed or rate at which the text will scroll across the display.
        /// Range: 0.0 to 10.0
        /// </summary>
        [Range(0f, 10f)] public float scrollSpeed;

        /// <summary>
        /// Scroll full screen regardless of message width and height
        /// </summary>
        public bool isScrollFullscreen;


        // FUTURE - rotation None, Left, Right on z-axis

        #endregion

        #region Private or Internal variables and properties - not serialized

        /// <summary>
        /// [INTERNAL ONLY]
        /// To avoid enum lookup at runtime
        /// </summary>
        [System.NonSerialized] internal int scrollDirectionInt;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Once initialised, the RectTransform of the message panel
        /// </summary>
        internal RectTransform CachedMessagePanel { get; set; }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Once initialised, the background Image message component
        /// </summary>
        internal UnityEngine.UI.Image CachedBgImgComponent { get; set; }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Once initialised, the message Text component
        /// </summary>
        internal UnityEngine.UI.Text CachedTextComponent { get; set; }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used for background brightness
        /// </summary>
        [System.NonSerialized] internal S3DColour baseBackgroundColour;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used for text brightness
        /// </summary>
        [System.NonSerialized] internal S3DColour baseTextColour;

        [System.NonSerialized] internal float scrollOffsetX;
        [System.NonSerialized] internal float scrollOffsetY;

        private Transform CachedMessagePanelTfrm { get { return CachedMessagePanel == null ? null : CachedMessagePanel.transform; } }

        #endregion

        #region Constructors
        public S3DDisplayMessage()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// S3DDisplayMessage copy constructor
        /// </summary>
        /// <param name="displayMessage"></param>
        public S3DDisplayMessage(S3DDisplayMessage displayMessage)
        {
            if (displayMessage == null) { SetClassDefaults(); }
            else
            {
                messageName = displayMessage.messageName;
                messageString = displayMessage.messageString;
                showMessage = displayMessage.showMessage;
                offsetX = displayMessage.offsetX;
                offsetY = displayMessage.offsetY;
                displayWidth = displayMessage.displayWidth;
                displayHeight = displayMessage.displayHeight;
                showInEditor = displayMessage.showInEditor;
                guidHash = displayMessage.guidHash;
                showBackground = displayMessage.showBackground;
                backgroundColour = new Color(displayMessage.backgroundColour.r, displayMessage.backgroundColour.g, displayMessage.backgroundColour.b, displayMessage.backgroundColour.a);
                textColour = new Color(displayMessage.textColour.r, displayMessage.textColour.g, displayMessage.textColour.b, displayMessage.textColour.a);
                textAlignment = displayMessage.textAlignment;
                isBestFit = displayMessage.isBestFit;
                fontMinSize = displayMessage.fontMinSize;
                fontMaxSize = displayMessage.fontMaxSize;
                scrollDirection = displayMessage.scrollDirection;
                scrollSpeed = displayMessage.scrollSpeed;
                isScrollFullscreen = displayMessage.isScrollFullscreen;
            }
        }
        #endregion

        #region Public Member Methods

        /// <summary>
        /// Set the defaults values for this class
        /// </summary>
        public void SetClassDefaults()
        {
            messageName = string.Empty;
            messageString = string.Empty;
            showMessage = false;
            // Default to centre of screen
            offsetX = 0f;
            offsetY = 0f;
            displayWidth = 1.0f;
            displayHeight = 0.1f;
            showInEditor = false;
            guidHash = S3DMath.GetHashCodeFromGuid();
            showBackground = false;
            backgroundColour = Color.white;
            textColour = Color.black;
            textAlignment = TextAnchor.MiddleCenter;
            isBestFit = true;
            fontMinSize = 10;
            fontMaxSize = 72;
            scrollDirection = ScrollDirection.None;
            scrollSpeed = 0f;
            isScrollFullscreen = false;
        }

        #endregion
    }
}