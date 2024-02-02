using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class containing message data for a Sticky Popup Module
    /// </summary>
    [System.Serializable]
    public class S3DPopupMessage
    {
        #region Enumerations

        #endregion

        #region Public Variables

        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// The name or description of the message. This can be used to identify
        /// the message. It is not displayed in the popup display.
        /// </summary>
        public string messageName;

        /// <summary>
        /// The label text of the message to display. It can include RichText markup. e.g. <b>Bold Text</b>.
        /// At runtime call stickyPopupModule.SetMessageLabelText(..)
        /// </summary>
        public string messageLabelString;

        /// <summary>
        /// The text message to display. It can include RichText markup. e.g. <b>Bold Text</b>.
        /// At runtime call stickyPopupModule.SetMessageValueText(..)
        /// </summary>
        public string messageValueString;

        /// <summary>
        /// The UI Panel that holds the label and value RectTransforms
        /// </summary>
        public RectTransform messagePanel;

        /// <summary>
        /// The UI Panel for the message label
        /// </summary>
        public RectTransform labelPanel;

        /// <summary>
        /// The UI Panel for the message value text
        /// </summary>
        public RectTransform valuePanel;

        /// <summary>
        /// Show (or hide) the message. At runtime use stickyPopupModule.ShowMessage() or HideMessage().
        /// </summary>
        public bool showMessage;

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
        /// [INTERNAL USE ONLY]
        /// Records if ShowMessage is set when the popup is first initialised.
        /// Each time the popup is displayed, the message will attempt to show
        /// or hide it based on this initial setting.
        /// </summary>
        [System.NonSerialized] public bool isShowOnPopup;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Does the message have a valid RectTransform panel?
        /// </summary>
        [System.NonSerialized] public bool isMessageValid;


        [System.NonSerialized] public bool isLabelUIValid;
        [System.NonSerialized] public bool isValueUIValid;
        [System.NonSerialized] public UnityEngine.UI.Text labelUIText;
        [System.NonSerialized] public UnityEngine.UI.Text valueUIText;

        #endregion

        #region Public Properties

        #endregion

        #region Public Static Variables

        #endregion

        #region Private Variables - General

        #endregion

        #region Constructors
        public S3DPopupMessage()
        {
            SetClassDefaults();
        }

        // Copy constructor
        public S3DPopupMessage (S3DPopupMessage popupMessage)
        {
            if (popupMessage == null) { SetClassDefaults(); }
            else
            {
                messageName = popupMessage.messageName;
                messageLabelString = popupMessage.messageLabelString;
                messageValueString = popupMessage.messageValueString;
                messagePanel = popupMessage.messagePanel;
                labelPanel = popupMessage.labelPanel;
                valuePanel = popupMessage.valuePanel;
                showMessage = popupMessage.showMessage;
                guidHash = popupMessage.guidHash;
                showInEditor = popupMessage.showInEditor;
            }
        } 

        #endregion

        #region Private and Internal Methods - General

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Set the defaults values for this class
        /// </summary>
        public void SetClassDefaults()
        {
            messageName = string.Empty;
            messageLabelString = string.Empty;
            messageValueString = string.Empty;
            messagePanel = null;
            labelPanel = null;
            valuePanel = null;
            showMessage = false;
            showInEditor = false;
            guidHash = S3DMath.GetHashCodeFromGuid();
        }

        #endregion

    }
}