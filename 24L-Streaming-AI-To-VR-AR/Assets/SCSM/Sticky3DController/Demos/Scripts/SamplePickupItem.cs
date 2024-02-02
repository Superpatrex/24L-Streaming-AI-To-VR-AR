using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Sample script to show how to pick up one or more items in front of the player.
    /// A similar technique could be used to drop items from an inventory system.
    /// Setup:
    /// 1. Add a S3D player prefab character (e.g. PlayerBob) to your scene
    /// 2. Ensure the character has Initialise on Awake enabled on the Move tab
    /// 3. Create a few items to pickup and add them to their own Unity Layer
    /// 4. Add this script to your S3D controller
    /// 5. On the script set the Item LayerMask and set the panel background.
    /// 6. Ensure there is a UI EventSystem in the scene
    /// 7. If you have a Pickup animation in your animation controller, ensure there
    ///    is a matching Animation Action configured on the S3D Animate tab. The
    ///    Standard Action type must be set to Custom.
    /// 8. Add a Custom Input to the StickyInputModule
    /// 9. Configure the button for the Custom Input e.g. G or P on the keyboard
    /// 10. Add a callback method to the Custom Input. Drag in the player under "Runtime Only"
    /// 11. Change the Function to SamplePickupItem.PickupItems() OR PickupFirstItem()
    /// 12. If you have a pickup animation, use PickupFirstItem() and add a Custom Animation Action
    ///     to your Custom Input. Select the Animate Action from step #7.
    /// WARNING
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Pick Up Item")]
    public class SamplePickupItem : MonoBehaviour
    {
        #region Public Variables
        public string itemCanvasName = "ItemCanvas";
        public LayerMask itemsLayerMask = 0;
        public float pickupDistance = 2f;
        public Sprite panelBackground = null;
        #endregion

        #region Private Variables
        private bool isInitialised = false;
        private GameObject itemPanel = null;
        private RectTransform itemPanelRectTfrm = null;
        private Vector2 refResolution = new Vector2(1920f, 1080f);
        private StickyControlModule stickyControlModule = null;
        private List<Transform> itemList = null;
        private List<UnityEngine.UI.Text> uiTextList = null;
        private List<UnityEngine.UI.Button> buttonPanelList = null;
        private readonly string doneButtonName = "btnDone";
        private readonly string doneButtonText = "Done";
        private EventSystem eventSystem = null;
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            stickyControlModule = GetComponent<StickyControlModule>();

            // Pre-allocate Lists
            itemList = new List<Transform>(10);
            uiTextList = new List<Text>(2);
            buttonPanelList = new List<UnityEngine.UI.Button>(10);

            #region Create UI
            // Create a canvas to display the items to pick up
            GameObject newItemCanvasGameObject = new GameObject(itemCanvasName);
            newItemCanvasGameObject.transform.position = Vector3.zero;
            newItemCanvasGameObject.transform.parent = null;

            newItemCanvasGameObject.layer = 5;
            newItemCanvasGameObject.AddComponent<Canvas>();

            Canvas itemCanvas = newItemCanvasGameObject.GetComponent<Canvas>();
            itemCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            itemCanvas.sortingOrder = 2;
            UnityEngine.UI.CanvasScaler canvasScaler = newItemCanvasGameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            if (canvasScaler != null)
            {
                canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.referenceResolution = refResolution;
                canvasScaler.matchWidthOrHeight = 0.5f;
            }

            // Add a Graphic Raycaster component so that we can get button input from mouse clicks
            newItemCanvasGameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            RectTransform itemGORectTfrm = newItemCanvasGameObject.GetComponent<RectTransform>();

            DefaultControls.Resources uiRes = new DefaultControls.Resources();

            // Create a new Panel for the item list
            itemPanel = DefaultControls.CreatePanel(uiRes);
            itemPanel.layer = 5;
            itemPanel.transform.SetParent(itemCanvas.transform, false);
            itemPanelRectTfrm = itemPanel.GetComponent<RectTransform>();

            // Give the popup panel a border
            UnityEngine.UI.Image img = itemPanel.GetComponent<UnityEngine.UI.Image>();
            img.sprite = panelBackground;
            img.type = Image.Type.Sliced;
            img.fillCenter = false;
            img.raycastTarget = false;
            img.enabled = true;
            img.color = new Color(img.color.r, img.color.g, img.color.b, 200f / 255f);

            itemPanelRectTfrm.anchorMin = new Vector2(0f, 0f);
            itemPanelRectTfrm.anchorMax = new Vector2(1f, 1f);

            // make panel half width and height of screen
            itemPanelRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, refResolution.x * 0.5f);
            itemPanelRectTfrm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, refResolution.y * 0.5f);
            // position in centre of screen
            itemPanelRectTfrm.anchoredPosition = new Vector2(0f, 0f);

            itemPanel.SetActive(false);
            eventSystem = EventSystem.current;
            isInitialised = true;
            #endregion
        }

        #endregion

        #region Private Methods

        private bool CheckIfSafeToAdd(Transform trfm)
        {
            bool isSafeToAdd = true;
            int numItems = itemList == null ? 0 : itemList.Count;

            for (int i = 0; i < numItems; i++)
            {
                // Is the object a child of an existing item in the list?
                if (trfm.IsChildOf(itemList[i]))
                {
                    isSafeToAdd = false;
                    break;
                }
                // Is an existing item a child of the object to check?
                else if (itemList[i].IsChildOf(trfm))
                {
                    isSafeToAdd = false;
                    // Replace the item in the list with this object
                    itemList[i] = trfm;
                }
            }

            return isSafeToAdd;
        }

        /// <summary>
        /// Clear any previous temporary list of items and delete
        /// any previous buttons on the canvas.
        /// </summary>
        private void ResetPickup()
        {
            // Clear the temporary list of items to pickup
            itemList.Clear();

            // Remove any buttons previously created
            itemPanel.GetComponentsInChildren(true,buttonPanelList);

            int numButtons = buttonPanelList == null ? 0 : buttonPanelList.Count;

            for (int bnIdx = numButtons - 1; bnIdx >= 0; bnIdx--)
            {
                #if UNITY_EDITOR
                DestroyImmediate(buttonPanelList[bnIdx].gameObject);
                #else
                Destroy(buttonPanelList[bnIdx].gameObject);
                #endif
            }
        }

        /// <summary>
        /// Disable the first button that has the same name as the transform.
        /// WARNING: This does multiple string comparisions so will generate GC.
        /// A production-ready solution would store a hashcode instead.
        /// </summary>
        /// <param name="itemTrfm"></param>
        private void DisableButton(Transform itemTrfm)
        {
            // Get a list of the current item buttons
            itemPanel.GetComponentsInChildren(false, buttonPanelList);

            int numButtons = buttonPanelList == null ? 0 : buttonPanelList.Count;

            for (int bnIdx = 0; bnIdx < numButtons; bnIdx++)
            {
                var btn = buttonPanelList[bnIdx];

                // Get the text of the button
                btn.GetComponentsInChildren(uiTextList);
                if (uiTextList != null && uiTextList.Count > 0)
                {
                    // Found matching button, so disable it
                    if (uiTextList[0].text == itemTrfm.name)
                    {
                        btn.interactable = false;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Public Methods called from Custom Input

        /// <summary>
        /// This can be called from a Custom Input of StickyInputModule when the user attempts to pickup items.
        /// It will display a list of available items to pickup from in front of the player.
        /// NOTE: This sample code is not production-ready and may generate unwanted Garbage Collection
        /// </summary>
        public void PickupItems()
        {
            if (isInitialised && itemPanel != null)
            {
                // Find objects to pickup
                if (stickyControlModule != null)
                {
                    Vector3 s3dPostion = stickyControlModule.GetCurrentCentre();
                    Vector3 s3dSize = stickyControlModule.ScaledSize;
                    RaycastHit[] hits = new RaycastHit[10];

                    ResetPickup();

                    // Look for items in front of the character
                    int numHits = Physics.BoxCastNonAlloc(s3dPostion, new Vector3(s3dSize.x, s3dSize.y / 2f, 0f), stickyControlModule.transform.forward, hits, Quaternion.identity, pickupDistance, itemsLayerMask);
                    if (numHits > 0)
                    {
                        for (int hIdx = 0; hIdx < numHits; hIdx++)
                        {
                            Transform itemTfrm = hits[hIdx].transform;

                            if (CheckIfSafeToAdd(itemTfrm))
                            {
                                itemList.Add(itemTfrm);
                            }
                        }

                        // Populate the panel with potential items to pickup
                        int numItems = itemList == null ? 0 : itemList.Count;

                        if (numItems > 0)
                        {
                            stickyControlModule.DisableMovement();
                            stickyControlModule.DisableLookMovement();

                            DefaultControls.Resources uiRes = new DefaultControls.Resources();

                            Vector2 panelSize = S3DMath.Abs(itemPanelRectTfrm.sizeDelta);
                            float buttonWidth = panelSize.x * 0.8f;
                            float buttonHeight = 0.1f * panelSize.y;
                            float buttonSpacing = buttonHeight * 0.5f;
                            float buttonTopOffsetY = buttonHeight * 0.5f;

                            // Allow a max 6 items to pickup (could do more with a scrollable list)
                            for (int i = 0; i < numItems && i < 6; i++)
                            {
                                GameObject buttonGO = DefaultControls.CreateButton(uiRes);
                                if (buttonGO != null)
                                {
                                    buttonGO.name = "btnObject" + (i + 1).ToString();
                                    buttonGO.layer = 5;

                                    // Set the Text of the button to the name of the transform to be picked up
                                    buttonGO.GetComponentsInChildren(uiTextList);

                                    Transform itemTfrm = itemList[i];

                                    if (uiTextList != null && uiTextList.Count > 0)
                                    {
                                        uiTextList[0].text = itemTfrm.name;
                                    }

                                    buttonGO.transform.SetParent(itemPanel.transform, false);

                                    RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();

                                    // Set position of button
                                    buttonRT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (float)i * (buttonHeight + buttonSpacing) + buttonTopOffsetY, buttonRT.sizeDelta.y);

                                    // Set size of button
                                    buttonRT.sizeDelta = new Vector2(buttonWidth, buttonHeight);

                                    // Create an event listener for users clicking on Object buttons to pickup that object
                                    UnityEngine.UI.Button objButton = buttonGO.GetComponent<UnityEngine.UI.Button>();

                                    if (objButton != null)
                                    {
                                        objButton.onClick.AddListener(() => { PickupItem(itemTfrm); });
                                    }
                                }
                            }

                            // Create a Done button to dismiss the Pickup Object panel
                            GameObject buttonDoneGO = DefaultControls.CreateButton(uiRes);
                            if (buttonDoneGO != null)
                            {
                                buttonDoneGO.name = doneButtonName;
                                buttonDoneGO.layer = 5;
                                RectTransform buttonDoneRT = buttonDoneGO.GetComponent<RectTransform>();
                                // Set the Text of the button to the name of the transform to be picked up
                                buttonDoneGO.GetComponentsInChildren(uiTextList);
                                if (uiTextList != null && uiTextList.Count > 0) { uiTextList[0].text = doneButtonText; }
                                buttonDoneRT.sizeDelta = new Vector2(panelSize.x * 0.1f, buttonHeight);
                                buttonDoneRT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, buttonHeight * 0.75f, buttonDoneRT.sizeDelta.y);

                                UnityEngine.UI.Button doneButton = buttonDoneGO.GetComponent<UnityEngine.UI.Button>();
                                if (doneButton != null)
                                {
                                    doneButton.onClick.AddListener(() => { itemPanel.SetActive(false); stickyControlModule.EnableMovement(); stickyControlModule.EnableLookMovement(); });
                                }

                                buttonDoneGO.transform.SetParent(itemPanel.transform, false);
                            }

                            // Show the panel
                            itemPanel.SetActive(true);

                            // Set done as the default UI button
                            if (eventSystem != null && buttonDoneGO != null) { eventSystem.SetSelectedGameObject(buttonDoneGO); }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This can be called from a Custom Input of StickyInputModule when the user attempts to pickup an item.
        /// </summary>
        public void PickupFirstItem()
        {
            if (isInitialised && stickyControlModule != null)
            {
                Vector3 s3dPostion = stickyControlModule.GetCurrentCentre();
                Vector3 s3dSize = stickyControlModule.ScaledSize;
                RaycastHit[] hits = new RaycastHit[10];

                // Look for items in front of the character
                int numHits = Physics.BoxCastNonAlloc(s3dPostion, new Vector3(s3dSize.x, s3dSize.y / 2f, 0f), stickyControlModule.transform.forward, hits, Quaternion.identity, pickupDistance, itemsLayerMask);

                if (numHits > 0)
                {
                    // Disable the actual object in the scene (assume you may wish to drop it at some point in your game)
                    hits[0].transform.gameObject.SetActive(false);

                    // Here is where you would add the item to your inventory
                }
            }
        }

        #endregion

        #region Public Methods - for item selection

        /// <summary>
        /// This is called when a user clicks on an item from the popup panel
        /// </summary>
        /// <param name="item"></param>
        public void PickupItem (Transform item)
        {
            //Debug.Log("Item picked up: " + item == null ? "unknown object" : item.name);

            if (item != null)
            {
                if (itemPanel.activeSelf && itemList != null)
                {
                    // Mark the item button inactive (assuming items have unique names)
                    DisableButton(item);

                    // Disable the actual object in the scene (assume you may wish to drop it at some point in your game)
                    item.gameObject.SetActive(false);

                    // Here is where you would add the item to your inventory
                }
            }
        }

        #endregion
    }
}
