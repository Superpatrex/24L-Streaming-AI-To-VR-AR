using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A popup canvas-based selectable menu that uses the Sticky Manager's pooling system.
    /// If a new module inherits from this, it is possible to add your own list of item
    /// types - like say from TextMeshPro. Then override InitialisePopup() to initialise
    /// them.
    /// NOTE: This requires a UI EventSystem to be in the scene.
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    [DisallowMultipleComponent]
    public class StickyPopupModule : StickyGenericModule
    {
        #region Public Variables

        /// <summary>
        /// If a character is assigned to the popup, any held weapons will not fire while the popup is shown.
        /// </summary>
        public bool isPauseWeaponsFiring = false;

        /// <summary>
        /// When the popup is closed and it was initiated from a character (directly or indirectly), delaying
        /// the unpausing of weapons by a small amount can help to prevent unintentional weapon firing.
        /// Smaller delays are typically better. Try 0.1 seconds to start with and increase if required.
        /// </summary>
        [Range(0f,3f)] public float unPauseWeaponsFiringDelay = 0.1f;

        [Tooltip("This can be useful when button has no backgound image and the Text should change colour based on button transistion tint colours")]
        public bool useButtonColourForText = false;

        public List<S3DPopupMessage> popupMessageList;

        #endregion

        #region Public Properties

        /// <summary>
        /// Is the popup currently in use or shown in the scene?
        /// </summary>
        public bool IsPopupActive { get { return isPopupInitialised && isPopupActive; } }

        /// <summary>
        /// The number of possible options or items in the Popup.
        /// It it possible to show some and hide others.
        /// </summary>
        public int NumberOfItems { get { return numberOfItems; } }

        /// <summary>
        /// This will return the number of potential messages for this popup. It does not assume they are all configured correctly.
        /// </summary>
        public int NumberOfMessages { get { return isPopupInitialised ? numberOfMessages : popupMessageList == null ? 0 : popupMessageList.Count; } }

        #endregion

        #region Public Static Variables

        #endregion

        #region Private Variables - General

        // Editor only
        [SerializeField] private bool isMessagesExpanded = true;

        /// <summary>
        /// The ID number for this popup prefab (as assigned by the Sticky Manager in the scene).
        /// This is the index in the StickyManager genericObjectTemplatesList.
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] public int popupPrefabID = -1;

        [System.NonSerialized] protected Canvas canvas = null;
        [System.NonSerialized] protected Transform canvasTrfm = null;
        [System.NonSerialized] protected Transform cameraTfrm = null;
        protected bool isPopupInitialised = false;
        protected bool isPopupActive = false;
        [System.NonSerialized] protected StickyInteractive stickyInteractive = null;
        [System.NonSerialized] protected StickyControlModule stickyControlModule = null;
        [System.NonSerialized] protected StickyControlModule stickyControlModuleIndirect = null;
        [System.NonSerialized] protected StickySocket stickySocket = null;

        /// <summary>
        /// The number of options to select from. This will most likely
        /// be the number of child buttons. Not all need to be made
        /// active at runtime.
        /// </summary>
        protected int numberOfItems = 0;

        protected int numberOfMessages = 0;

        /// <summary>
        /// If a new module inherits from this, it is possible to add your
        /// own lists of item types - like say from TextMeshPro
        /// </summary>
        protected List<UnityEngine.UI.Button> buttonItemList = null;

        protected List<UnityEngine.UI.Text> buttomTextItemList = null;
        protected List<UnityEngine.CanvasRenderer> buttomCanvasRendererItemList = null;

        protected List<UnityEngine.UI.Text> tempTextList = null;

        #endregion

        #region Public Delegates

        public delegate void CallbackOnItemClick (StickyPopupModule stickyPopupModule, int itemNumber, StickyInteractive stickyInteractive);
        public delegate void CallbackOnItemClick2 (StickyPopupModule stickyPopupModule, int itemNumber, StickySocket stickySocket);

        public delegate void CallbackOnClosing (StickyPopupModule stickyPopupModule);

        [System.NonSerialized] public CallbackOnItemClick callbackOnItemClick;
        [System.NonSerialized] public CallbackOnItemClick2 callbackOnItemClick2;

        /// <summary>
        /// This is invoked immediately before the popup is closed.
        /// This should be a lightweight method that does not keep any references
        /// to avoid performance issues.
        /// After it is invoked, it is set to null.
        /// </summary>
        [System.NonSerialized] public CallbackOnClosing callbackOnClosing;

        #endregion

        #region Private Initialise Methods

        #endregion

        #region Update Methods

        private void Update()
        {
            if (isPopupActive)
            {
                // Make the popup face the camera
                if (canvasTrfm != null && cameraTfrm != null) { FaceCamera(); }

                // Attempt to copy the button background image colour over to the Button Text item.
                if (useButtonColourForText) { UpdateTextColour(); }
            }
        }

        #endregion

        #region Private and Internal Methods - General

        /// <summary>
        /// Attempt to show or hide a message
        /// </summary>
        /// <param name="popupMessage"></param>
        /// <param name="isShow"></param>
        protected void ShowOrHideMessage (S3DPopupMessage popupMessage, bool isShow)
        {
            if (popupMessage != null && popupMessage.isMessageValid)
            {
                if (isShow)
                {
                    if (!popupMessage.messagePanel.gameObject.activeSelf)
                    {
                        popupMessage.messagePanel.gameObject.SetActive(true); 
                    }
                }
                else if (popupMessage.messagePanel.gameObject.activeSelf)
                {
                    popupMessage.messagePanel.gameObject.SetActive(false);
                }

                popupMessage.showMessage = isShow;
            }
        }

        #endregion

        #region Events

        #endregion

        #region Override Methods

        /// <summary>
        /// This is called when the item is "destroyed". i.e. returned to the pool.
        /// </summary>
        public override void DestroyGenericObject()
        {
            DeactivatePopup();

            base.DestroyGenericObject();
        }

        /// <summary>
        /// Override the Initialise method so we can add our own stuff as well.
        /// </summary>
        /// <param name="igParms"></param>
        /// <returns></returns>
        public override uint Initialise (S3DInstantiateGenericObjectParameters igParms)
        {
            uint _itemSeqNum = base.Initialise(igParms);

            popupPrefabID = igParms.genericModulePrefabID;

            if (canvas == null && !TryGetComponent(out canvas))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] StickyPopupModule - could not find Canvas attached to " + gameObject.name);
                #endif

                // Keep compiler happy
                if (isMessagesExpanded) { }

                isPopupInitialised = false;
            }
            else
            {
                canvasTrfm = canvas.transform;
                isPopupInitialised = InitialisePopup();
            }

            isPopupActive = isPopupInitialised;

            return _itemSeqNum;
        }

        #endregion

        #region Public Virtual API Methods

        /// <summary>
        /// Add a message to the popup
        /// Will return the index in the zero-based list, or -1 if it fails.
        /// </summary>
        /// <param name="popupMessage">The damage Index in the list</param>
        /// <returns></returns>
        public virtual int AddMessage (S3DPopupMessage popupMessage)
        {
            if (popupMessage != null)
            {
                if (popupMessageList == null) { popupMessageList = new List<S3DPopupMessage>(5); }

                popupMessageList.Add(popupMessage);

                numberOfMessages = popupMessageList.Count;

                return numberOfMessages - 1;
            }
            else { return -1; }
        }

        /// <summary>
        /// Before the popup is deactivate / destroyed and returned
        /// to the pool, this cleans up the state of the items.
        /// If there was an interactive-enabled object, inform that
        /// object that the popup is no longer active.
        /// </summary>
        public virtual void DeactivatePopup()
        {
            if (callbackOnClosing != null)
            {
                callbackOnClosing.Invoke(this);

                // Set to null after being invoked.
                callbackOnClosing = null;
            }

            for (int itemIdx = 0; itemIdx < numberOfItems; itemIdx++)
            {
                // Get the button from the pre-populated list
                UnityEngine.UI.Button button = buttonItemList[itemIdx];

                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
            }

            if (stickyInteractive != null)
            {
                // Clear the Popup ID from the interactive object
                stickyInteractive.SetActivePopupID(0);
                stickyInteractive = null;
            }

            if (stickyControlModule != null)
            {
                // Clear the Popup ID from the character
                stickyControlModule.ClearActivePopup(this.GetInstanceID());

                // If required, unpause weapon firing
                if (isPauseWeaponsFiring) { stickyControlModule.UnpauseWeaponsFiring(unPauseWeaponsFiringDelay); }

                stickyControlModule = null;
            }

            if (stickyControlModuleIndirect != null)
            {
                // Clear the Popup ID from the character
                stickyControlModuleIndirect.ClearActivePopup(this.GetInstanceID());

                // If required, unpause weapon firing
                if (isPauseWeaponsFiring) { stickyControlModuleIndirect.UnpauseWeaponsFiring(unPauseWeaponsFiringDelay); }
            }

            if (stickySocket != null)
            {
                // Clear the Popup ID from the socket
                stickySocket.SetActivePopupID(0);
                stickySocket = null;
            }

            if (canvas != null)
            {
                canvas.worldCamera = null;
                cameraTfrm = null;
            }

            isPopupActive = false;
        }

        /// <summary>
        /// Keep the popup facing the camera. This gets automatically called each frame from Update().
        /// Override this is you want the canvas to rotate or move specifically for your game.
        /// </summary>
        public virtual void FaceCamera()
        {
            // Improved billboard style popup always facing the camera
            canvasTrfm.LookAt(canvasTrfm.position + cameraTfrm.rotation * Vector3.forward, cameraTfrm.rotation * Vector3.up);

            // Keep the popup "flat" on the camera forward direction
            //canvasTrfm.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(cameraTfrm.rotation * Vector3.forward, Vector3.up));
        }

        /// <summary>
        /// Get the first message with a matching name (is case sensitive).
        /// WARNING: This is likely to impact Garbage Collection.
        /// Where possible use GetPopupMessageByIndex(..)
        /// </summary>
        /// <returns></returns>
        public virtual S3DPopupMessage GetPopupMessage (string messageName)
        {
            S3DPopupMessage popupMessage = null;

            int _numMessages = isInitialised ? numberOfMessages : NumberOfMessages;

            for (int msgIdx = 0; msgIdx < _numMessages; msgIdx++)
            {
                S3DPopupMessage _message = popupMessageList[msgIdx];
                if (_message != null && !String.IsNullOrEmpty(_message.messageName) && _message.messageName == messageName)
                {
                    popupMessage = _message;
                    break;
                }
            }

            return popupMessage;
        }

        /// <summary>
        /// Get the message using the zero-based index in the list of messages
        /// </summary>
        /// <param name="messageName"></param>
        /// <returns></returns>
        public virtual S3DPopupMessage GetPopupMessageByIndex (int messageIndex)
        {
            int _numMessages = isInitialised ? numberOfMessages : NumberOfMessages;

            if (messageIndex >= 0 && messageIndex < (isInitialised ? numberOfMessages : NumberOfMessages))
            {
                return popupMessageList[messageIndex];
            }
            else { return null; }
        }

        /// <summary>
        /// Hide the option or item in the popup.
        /// </summary>
        /// <param name="itemNumber">Range: 1 to NumberOfItems</param>
        public virtual void HideItem (int itemNumber)
        {
            // Check that the item is in range
            if (itemNumber > 0 && itemNumber < numberOfItems + 1)
            {
                // Get the button from the pre-populated list
                UnityEngine.UI.Button button = buttonItemList[itemNumber - 1];

                if (button != null)
                {
                    // Check if we need to hide the button
                    if (button.gameObject.activeSelf) { button.gameObject.SetActive(false); }
                }
            }
        }

        /// <summary>
        /// Attempt to hide the message on the popup
        /// </summary>
        /// <param name="popupMessage"></param>
        public virtual void HideMessage (S3DPopupMessage popupMessage)
        {
            ShowOrHideMessage(popupMessage, false);
        }

        /// <summary>
        /// Initialise the items in the Popup
        /// </summary>
        public virtual bool InitialisePopup()
        {
            bool isSuccessful = false;

            // Get a list of the options or potential items for this popup.
            // This should only run the first time the item is activated.
            if (numberOfItems == 0)
            {
                #if UNITY_EDITOR
                if (UnityEngine.EventSystems.EventSystem.current == null)
                {
                    Debug.LogWarning("ERROR: Cannot find a UI EventSystem in the scene, did you forget to add one? Without it, UI interaction will not work.");
                }
                #endif

                buttonItemList = new List<Button>(5);

                gameObject.GetComponentsInChildren<UnityEngine.UI.Button>(true, buttonItemList);

                numberOfItems = buttonItemList == null ? 0 : buttonItemList.Count;

                // Add enough capacity to hold 1 Text component for each button
                buttomTextItemList = new List<Text>(numberOfItems);

                // Add enough capacity to store a canvas renderer for each button
                buttomCanvasRendererItemList = new List<CanvasRenderer>(numberOfItems);

                // Create a temporary list of Text component to reduce GC
                tempTextList = new List<Text>(1);

                // Populate the list of Text components - assume 1 Text per button
                for (int itemIdx = 0; itemIdx < numberOfItems; itemIdx++)
                {
                    UnityEngine.UI.Button button = buttonItemList[itemIdx];

                    if (button != null)
                    {
                        // Get a reference to the canvas renderer for this button
                        buttomCanvasRendererItemList.Add(button.GetComponent<CanvasRenderer>());

                        button.GetComponentsInChildren<UnityEngine.UI.Text>(true, tempTextList);

                        if (tempTextList.Count > 0)
                        {
                            buttomTextItemList.Add(tempTextList[0]);

                        }
                        else { buttomTextItemList.Add(null); }
                    }
                }

                numberOfMessages = popupMessageList == null ? 0 : popupMessageList.Count;

                for (int msgIdx = 0; msgIdx < numberOfMessages; msgIdx++)
                {
                    S3DPopupMessage popupMessage = popupMessageList[msgIdx];

                    // Record if the messages should be shown or not
                    popupMessage.isShowOnPopup = popupMessage.showMessage;

                    popupMessage.isMessageValid = popupMessage.messagePanel != null;

                    popupMessage.isLabelUIValid = false;
                    popupMessage.isValueUIValid = false;

                    // Cache the UI.Text components for the message
                    if (popupMessage.isMessageValid)
                    {
                        UnityEngine.UI.Image imageUI;
                        if (popupMessage.messagePanel.TryGetComponent(out imageUI)) { imageUI.raycastTarget = false; }

                        if (popupMessage.labelPanel != null)
                        {
                            if (popupMessage.labelPanel.TryGetComponent(out imageUI)) { imageUI.raycastTarget = false; }

                            popupMessage.labelPanel.GetComponentsInChildren<UnityEngine.UI.Text>(true, tempTextList);

                            if (tempTextList.Count > 0)
                            {
                                popupMessage.labelUIText = tempTextList[0];
                                popupMessage.labelUIText.raycastTarget = false;
                                popupMessage.isLabelUIValid = true;
                                popupMessage.labelUIText.text = popupMessage.messageLabelString;
                            }
                        }

                        if (popupMessage.valuePanel != null)
                        {
                            if (popupMessage.valuePanel.TryGetComponent(out imageUI)) { imageUI.raycastTarget = false; }

                            popupMessage.valuePanel.GetComponentsInChildren<UnityEngine.UI.Text>(true, tempTextList);

                            if (tempTextList.Count > 0)
                            {
                                popupMessage.valueUIText = tempTextList[0];
                                popupMessage.valueUIText.raycastTarget = false;
                                popupMessage.isValueUIValid = true;
                                popupMessage.valueUIText.text = popupMessage.messageValueString;
                            }
                        }
                    }
                }

                // Here we could reset everything to default values if desired.

            }

            isSuccessful = numberOfItems > 0;

            if (isSuccessful)
            {
                // Reset the state of the messages (if any)
                for (int msgIdx = 0; msgIdx < numberOfMessages; msgIdx++)
                {
                    S3DPopupMessage popupMessage = popupMessageList[msgIdx];
                    ShowOrHideMessage(popupMessage, popupMessage.isShowOnPopup);
                }
            }

            return isSuccessful;
        }

        /// <summary>
        /// Sets the world camera of the canvas so that the popup can face the camera
        /// </summary>
        /// <param name="camera"></param>
        public virtual void SetCamera (Camera camera)
        {
            if (canvas != null)
            {
                canvas.worldCamera = camera;

                if (camera != null)
                {
                    cameraTfrm = camera.transform;
                }
                else
                {
                    cameraTfrm = null;
                }
            }
            else
            {
                canvasTrfm = null;
                cameraTfrm = null;
            }
        }

        /// <summary>
        /// Set the character reference for this popup.
        /// This enables you to discover which character is showing the popup
        /// when an item is clicked. It informs the character this popup is active
        /// and parents itself to the character if IsReparented is enabled on the
        /// popup prefab.
        /// If isPauseWeaponsFiring is true, weapon firing will be paused on the character.
        /// See also SetCharacterIndirect(..).
        /// </summary>
        /// <param name="stickyInteractive"></param>
        public virtual void SetCharacter (StickyControlModule stickyControlModule)
        {
            this.stickyControlModule = stickyControlModule;

            if (stickyControlModule != null)
            {
                stickyControlModule.SetActivePopupID(this.GetInstanceID());

                if (isPauseWeaponsFiring) { stickyControlModule.PauseWeaponsFiring(); }

                // Parent the popup to the character so it moves with it
                if (isReparented) { transform.SetParent(stickyControlModule.transform); }
            }
        }

        /// <summary>
        /// Set a reference to a character which indirectly showed this popup. For example,
        /// when a character engages with a StickySocket which then shows this popup. The
        /// popup may be parented to the Socket, but not the character.
        /// This enables you to discover which character is showing the popup.
        /// when an item is clicked. It informs the character this popup is active.
        /// If isPauseWeaponsFiring is true, weapon firing will be paused on the character.
        /// See also SetCharacter(..).
        /// </summary>
        /// <param name="indirectCharacter"></param>
        public virtual void SetCharacterIndirect (StickyControlModule indirectCharacter)
        {
            stickyControlModuleIndirect = indirectCharacter;

            if (indirectCharacter != null)
            {
                indirectCharacter.SetActivePopupID(this.GetInstanceID());

                if (isPauseWeaponsFiring) { indirectCharacter.PauseWeaponsFiring(); }
            }
        }

        /// <summary>
        /// Set the interactive-enabled object reference for this popup.
        /// This enables you to discover which interactive-enabled object is showing the popup
        /// when an item is clicked. It informs the interactive object this popup is active
        /// and parents itself to the object if IsReparented is enabled on the Popup prefab.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        public virtual void SetInteractiveObject (StickyInteractive stickyInteractive)
        {
            this.stickyInteractive = stickyInteractive;

            if (stickyInteractive != null)
            {
                stickyInteractive.SetActivePopupID(this.GetInstanceID());

                // Parent the popup to the interactive object so it moves with it
                if (isReparented) { transform.SetParent(stickyInteractive.transform); }
            }
        }

        /// <summary>
        /// Set the option or item in the popup
        /// </summary>
        /// <param name="itemNumber">Range: 1 to NumberOfItems</param>
        /// <param name="isShown">Should the item be displayed?</param>
        /// <param name="isEnabled">Should the item be interactable?</param>
        public virtual void SetItem (int itemNumber, bool isShown, bool isEnabled = true)
        {
            // Check that the item is in range
            if (itemNumber > 0 && itemNumber < numberOfItems + 1)
            {
                // Get the button from the pre-populated list
                UnityEngine.UI.Button button = buttonItemList[itemNumber - 1];

                if (button != null)
                {
                    button.interactable = isEnabled;

                    // Check if we need to show or hide the button
                    if (button.gameObject.activeSelf != isShown) { button.gameObject.SetActive(isShown); }
                }
            }
        }

        /// <summary>
        /// Set the option or item in the popup
        /// </summary>
        /// <param name="itemNumber">Range: 1 to NumberOfItems</param>
        /// <param name="itemText"></param>
        /// <param name="isShown">Should the item be displayed?</param>
        /// <param name="isEnabled">Should the item be interactable?</param>
        public virtual void SetItem (int itemNumber, string itemText, bool isShown = true, bool isEnabled = true)
        {
            // Check that the item is in range
            if (itemNumber > 0 && itemNumber < numberOfItems+1)
            {
                // Get the button from the pre-populated list
                UnityEngine.UI.Button button = buttonItemList[itemNumber - 1];

                if (button != null)
                {
                    button.interactable = isEnabled;

                    // Check if we need to show or hide the button
                    if (button.gameObject.activeSelf != isShown) { button.gameObject.SetActive(isShown); }

                    // Get the UI Text item from the pre-populated list
                    UnityEngine.UI.Text _textUI = buttomTextItemList[itemNumber - 1];

                    if (_textUI != null)
                    {
                        _textUI.text = itemText;

                        //_textUI.color = button.GetComponent<CanvasRenderer>().GetColor();
                        
                    }
                }
            }
        }

        /// <summary>
        /// Assign a callback method for all items. This enables you to call your own game code when
        /// any item is clicked. If a StickyInteractive has been set, that will be passed to the
        /// callback method.
        /// </summary>
        /// <param name="itemNumber"></param>
        /// <param name="onItemClickedMethod"></param>
        public virtual void SetItemAction (CallbackOnItemClick onItemClickedMethod)
        {
            // Loop through all the items
            for (int itemNumber = 0; itemNumber < numberOfItems; itemNumber++)
            {
                // Get the button from the pre-populated list
                UnityEngine.UI.Button button = buttonItemList[itemNumber];

                if (button != null)
                {
                    // Remove any existing listeners first
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(delegate { onItemClickedMethod(this, itemNumber+1, stickyInteractive); } );
                }
            }
        }

        /// <summary>
        /// Assign a callback method for an item. This enables you to call your own game code when
        /// an item is clicked. If an interactive object has been set, that will be passed to the
        /// callback method.
        /// </summary>
        /// <param name="itemNumber"></param>
        /// <param name="onItemClickedMethod"></param>
        public virtual void SetItemAction (int itemNumber, CallbackOnItemClick onItemClickedMethod)
        {
            // Check that the item is in range
            if (itemNumber > 0 && itemNumber < numberOfItems+1)
            {
                // Get the button from the pre-populated list
                UnityEngine.UI.Button button = buttonItemList[itemNumber - 1];

                if (button != null)
                {
                    // Remove any existing listeners first
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(delegate { onItemClickedMethod(this, itemNumber, stickyInteractive); } );
                }
            }
        }

        /// <summary>
        /// Assign a callback method for an item. This enables you to call your own game code when
        /// an item is clicked. If a StickySocket has been set, that will be passed to the
        /// callback method.
        /// </summary>
        /// <param name="itemNumber"></param>
        /// <param name="onItemClickedMethod"></param>
        public virtual void SetItemAction2 (int itemNumber, CallbackOnItemClick2 onItemClickedMethod)
        {
            // Check that the item is in range
            if (itemNumber > 0 && itemNumber < numberOfItems+1)
            {
                // Get the button from the pre-populated list
                UnityEngine.UI.Button button = buttonItemList[itemNumber - 1];

                if (button != null)
                {
                    // Remove any existing listeners first
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(delegate { onItemClickedMethod(this, itemNumber, stickySocket); } );
                }
            }
        }

        /// <summary>
        /// Assign a callback method for all items. This enables you to call your own game code when
        /// any item is clicked. If a StickySocket has been set, that will be passed to the
        /// callback method.
        /// </summary>
        /// <param name="itemNumber"></param>
        /// <param name="onItemClickedMethod"></param>
        public virtual void SetItemAction2 (CallbackOnItemClick2 onItemClickedMethod)
        {
            // Loop through all the items
            for (int itemNumber = 0; itemNumber < numberOfItems; itemNumber++)
            {
                // Get the button from the pre-populated list
                UnityEngine.UI.Button button = buttonItemList[itemNumber];

                if (button != null)
                {
                    // Remove any existing listeners first
                    button.onClick.RemoveAllListeners();
                    // Captured variables are always evaluated when the delegate is actually invoked,
                    // not when the variables were captured. So, we need to create a new item number for each delegate.
                    int _itemNo = itemNumber + 1;
                    button.onClick.AddListener(delegate { onItemClickedMethod(this, _itemNo, stickySocket); } );
                }
            }
        }

        /// <summary>
        /// Attempt to update the text of a message label
        /// </summary>
        /// <param name="popupMessage"></param>
        /// <param name="newLabelText"></param>
        public virtual void SetMessageLabelText (S3DPopupMessage popupMessage, string newLabelText)
        {
            if (popupMessage != null && popupMessage.isLabelUIValid)
            {
                popupMessage.labelUIText.text = newLabelText;
            }
        }

        /// <summary>
        /// Attempt to update the text of a message value
        /// </summary>
        /// <param name="popupMessage"></param>
        /// <param name="newValueText"></param>
        public virtual void SetMessageValueText (S3DPopupMessage popupMessage, string newValueText)
        {
            if (popupMessage != null && popupMessage.isValueUIValid)
            {
                popupMessage.valueUIText.text = newValueText;
            }
        }

        /// <summary>
        /// Set the socket reference for this popup.
        /// This enables you to discover which StickySocket is showing the popup
        /// when an item is clicked. It informs the socket this popup is active
        /// and parents itself to the object if IsReparented is enabled on the Popup prefab.
        /// </summary>
        /// <param name="stickySocket"></param>
        public virtual void SetSocket (StickySocket stickySocket)
        {
            this.stickySocket = stickySocket;

            if (stickySocket != null)
            {
                stickySocket.SetActivePopupID(this.GetInstanceID());

                // Parent the popup to the interactive object so it moves with it
                if (isReparented) { transform.SetParent(stickySocket.transform); }
            }
        }

        /// <summary>
        /// Show the option or item in the popup.
        /// </summary>
        /// <param name="itemNumber">Range: 1 to NumberOfItems</param>
        public virtual void ShowItem (int itemNumber)
        {
            // Check that the item is in range
            if (itemNumber > 0 && itemNumber < numberOfItems + 1)
            {
                // Get the button from the pre-populated list
                UnityEngine.UI.Button button = buttonItemList[itemNumber - 1];

                if (button != null)
                {
                    // Check if we need to show the button
                    if (!button.gameObject.activeSelf) { button.gameObject.SetActive(true); }
                }
            }
        }

        /// <summary>
        /// Attempt to Show the message in the popup
        /// </summary>
        /// <param name="popupMessage"></param>
        public virtual void ShowMessage (S3DPopupMessage popupMessage)
        {
            ShowOrHideMessage(popupMessage, true);
        }

        /// <summary>
        /// Attempt to copy the button background image colour over to the Button Text item.
        /// This is called from Update() when useButtonColourForText is true.
        /// </summary>
        public virtual void UpdateTextColour()
        {
            for (int itemIdx = 0; itemIdx < numberOfItems; itemIdx++)
            {
                // This may not be very efficient...
                buttomTextItemList[itemIdx].color = buttomCanvasRendererItemList[itemIdx].GetColor();
            }
        }

        #endregion
    }
}