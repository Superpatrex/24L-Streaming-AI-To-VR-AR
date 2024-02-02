using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Sample script which shows a popup in VR when you touch an
    /// interactive-enabled object. This example, shows data about a
    /// StickyInteractorBridge component.
    /// Setup:
    /// 1. In the scene add an interactive-enabled object and enable IsTouchable
    /// 2. In the scene add an empty gameobject and add this script to it
    /// 3. From the project pane, add the Demos -> Prefabs -> Visuals -> StickyPopup5 prefab to this script in the scene.
    /// 4. From the scene, locate your StickyInteractorBridge component and add it to this script in the slot provided.
    /// 5. On the StickyInteractorBridge component, enable Can Touch.
    /// 6. On your interactive-enabled object, on the Events tab, add a new On Touched event
    /// 7. Drag this script gameobject, from the scene into the new event
    /// 8. Set the Function to be SampleXRDataPopup.ShowPopup(StickyInteractive).
    /// 9. Drag the interactive object from the scene into the "(None) StickyInteractive" parameter slot.
    /// NOTE:
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/XR Data Popup")]
    public class SampleXRDataPopup : MonoBehaviour
    {
        #region Public Variables

        [Tooltip("This is your popup prefab. e.g. Demos -> Prefabs -> Visuals -> StickyPopup5")]
        public StickyPopupModule stickyPopupModulePrefab;

        [Tooltip("A StickyInteractorBridge component that could be used with non-Sticky3D VR hands, like say with Sci-Fi Ship Controller.")]
        public StickyInteractorBridge stickyInteractorBridge;

        [Tooltip("The camera where the popup should face")]
        public Camera playerCamera = null;

        [Tooltip("Override the time the popup remains visible before automatically being returned to the pool")]
        [Range(5f, 300f)] public float maxPopupTime = 20f;

        #endregion

        #region Private Variables

        private bool isInitialised = false;
        private StickyManager stickyManager = null;
        private int popupPrefabID = StickyManager.NoPrefabID;
        private int numMessagesRequired = 3;
        /// <summary>
        /// The popup that is currently being shown in the scene.
        /// </summary>
        private StickyPopupModule activeStickyPopupModule = null;
        private bool isPopupActive = false;
        private S3DPopupMessage gripMessage = null;
        private S3DPopupMessage triggerMessage = null;
        private S3DPopupMessage infoMessage = null;

        private readonly string msgYes = "Yes";
        private readonly string msgNo = "No";

        #endregion

        #region Initialisation Methods

        // Start is called before the first frame update
        void Start()
        {
            if (stickyPopupModulePrefab == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] Did you forget to add a reference to your StickyPopupModule prefab on " + name + "?");
                #endif
            }
            else if (stickyInteractorBridge == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] Did you forget to add a reference to your StickyInteractorBridge from the scene to " + name + "?");
                #endif
            }

            else if (stickyPopupModulePrefab.NumberOfMessages < numMessagesRequired)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] The StickyPopupModule prefab (" + stickyPopupModulePrefab.name + "), needs a minimum of " + numMessagesRequired + " messages to work with this demo on " + name);
                #endif
            }
            else
            {
                stickyManager = StickyManager.GetOrCreateManager(gameObject.scene.handle);

                popupPrefabID = stickyManager.GetOrCreateGenericPool(stickyPopupModulePrefab);

                isInitialised = popupPrefabID != StickyManager.NoPrefabID;
            }
        }

        #endregion

        #region Update Methods
        void Update()
        {
            if (isPopupActive)
            {
                float gripValue = stickyInteractorBridge.HandGripValue;
                float triggerValue = stickyInteractorBridge.HandTriggerValue;

                activeStickyPopupModule.SetMessageValueText(gripMessage, S3DUtils.GetNumericString(gripValue, 2, false));
                activeStickyPopupModule.SetMessageValueText(triggerMessage, S3DUtils.GetNumericString(triggerValue, 2, false));
                activeStickyPopupModule.SetMessageValueText(infoMessage, stickyInteractorBridge.IsHandInputDeviceXRValid ? msgYes : msgNo);
            }
        }
        #endregion

        #region Public Methods


        /// <summary>
        /// This method is called automatically by StickyPopupModule when an item is clicked. 
        /// </summary>
        /// <param name="stickyPopupModule"></param>
        /// <param name="itemNumber"></param>
        /// <param name="stickyInteractive"></param>
        public void ClosePopup (StickyPopupModule stickyPopupModule, int itemNumber, StickyInteractive stickyInteractive)
        {
            string interactiveObject = stickyInteractive == null ? "unknown Interactive-enabled object" : stickyInteractive.name;

            Debug.Log("[DEBUG] Close popup on " + interactiveObject + " T:" + Time.time);

            // Close the popup after use - i.e. return it to the pool.
            if (stickyPopupModule != null) { stickyPopupModule.DestroyGenericObject(); }

            // Tell this script we no longer have an active popup
            activeStickyPopupModule = null;
            isPopupActive = false;
        }

        /// <summary>
        /// This should be automatically called by the interactive object you move the VR hands over the interactive object.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        public void ShowPopup (StickyInteractive stickyInteractive)
        {
            if (isInitialised && stickyInteractive != null && popupPrefabID != StickyManager.NoPrefabID && stickyInteractive.GetActivePopupID() == 0)
            {
                // Get the world-space position for the Popup based on the default relative local space offset setting 
                Vector3 _position = stickyInteractive.GetPopupPosition();

                // Setup the generic parameters
                S3DInstantiateGenericObjectParameters igParms = new S3DInstantiateGenericObjectParameters
                {
                    position = _position,
                    rotation = stickyInteractive.transform.rotation,
                    genericModulePrefabID = popupPrefabID,
                    //overrideAutoDestroy = true
                };

                StickyGenericModule stickyGenericModule = stickyManager.InstantiateGenericObject(ref igParms);

                if (stickyGenericModule != null)
                {
                    stickyGenericModule.despawnTime = maxPopupTime;

                    // Configure our extended functionality
                    activeStickyPopupModule = (StickyPopupModule)stickyGenericModule;

                    // Tell the popup which interactive-enabled object triggered it
                    activeStickyPopupModule.SetInteractiveObject(stickyInteractive);

                    // Tell the popup where it should be facing
                    if (playerCamera != null) { activeStickyPopupModule.SetCamera(playerCamera); }

                    // Tell the popup to call our method when the close button is clicked
                    activeStickyPopupModule.SetItemAction(1, ClosePopup);

                    gripMessage = activeStickyPopupModule.GetPopupMessageByIndex(0);
                    triggerMessage = activeStickyPopupModule.GetPopupMessageByIndex(1);
                    infoMessage = activeStickyPopupModule.GetPopupMessageByIndex(2);

                    activeStickyPopupModule.SetMessageLabelText(gripMessage, "Grip Value");
                    activeStickyPopupModule.SetMessageLabelText(triggerMessage, "Trigger Value");
                    activeStickyPopupModule.SetMessageLabelText(infoMessage, "Hand Device");

                    activeStickyPopupModule.ShowMessage(gripMessage);
                    activeStickyPopupModule.ShowMessage(triggerMessage);
                    activeStickyPopupModule.ShowMessage(infoMessage);

                    // Hide any remaining messages
                    for (int msgIdx = 3; msgIdx < activeStickyPopupModule.NumberOfMessages; msgIdx++)
                    {
                        activeStickyPopupModule.HideMessage(activeStickyPopupModule.GetPopupMessageByIndex(msgIdx));
                    }

                    isPopupActive = true;
                }
            }
        }

        #endregion
    }
}