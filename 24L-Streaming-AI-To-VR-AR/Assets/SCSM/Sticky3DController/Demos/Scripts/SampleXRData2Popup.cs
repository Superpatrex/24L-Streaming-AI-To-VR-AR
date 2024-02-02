using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Sample script which shows a popup in VR when you hover over an interactive-enabled 
    /// object. This example, shows XR status data.
    /// Setup:
    /// 1. In the scene add an interactive-enabled object
    /// 2. In the scene add an empty gameobject and add this script to it
    /// 3. From the project pane, add the Demos -> Prefabs -> Visuals -> StickyPopup5 prefab to this script in the scene.
    /// 6. On your interactive-enabled object, on the Events tab, add a new On Hover Enter event
    /// 7. Drag this script gameobject, from the scene into the new event
    /// 8. Set the Function to be SampleXRData2Popup.ShowPopup(StickyInteractive).
    /// 9. Drag the interactive object from the scene into the "(None) StickyInteractive" parameter slot.
    /// 10. On the interactive object, set the popup offset to say y = 0.25 (adjust this as required)
    /// 11. When playing the game, start looking at (pointing at) the interactive object with one of the hand controllers.
    /// NOTE:
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/XR Data2 Popup")]
    public class SampleXRData2Popup : MonoBehaviour
    {

        #region Enumerations

        public enum DisplayOption2
        {
            HeightCalibrated,
            HMD_AxisX,
            HMD_AxisY,
            HMD_AxisZ
        }

        public enum DisplayOption3
        {
            HMD_AxisX,
            HMD_AxisY,
            HMD_AxisZ,
            HMDValid,
            VRRefreshRate,
            XRInitCount
        }

        #endregion

        #region Public Variables

        [Tooltip("This is your popup prefab. e.g. Demos -> Prefabs -> Visuals -> StickyPopup5")]
        public StickyPopupModule stickyPopupModulePrefab;

        [Tooltip("A StickyInputModule component for your Sticky3D character XR rig")]
        public StickyInputModule stickyInputModule;

        [Tooltip("The camera where the popup should face")]
        public Camera playerCamera = null;

        [Tooltip("When enabled, in VR, show this data in the status popup")]
        public DisplayOption2 displayOption2 = DisplayOption2.HeightCalibrated;

        [Tooltip("When enabled, in VR, show this data in the status popup")]
        public DisplayOption3 displayOption3 = DisplayOption3.HMDValid;

        [Tooltip("Override the time the popup remains visible before automatically being returned to the pool")]
        [Range(5f, 300f)] public float maxPopupTime = 20f;

        #endregion

        #region Private Variables

        private bool isInitialised = false;
        private StickyManager stickyManager = null;
        private StickyControlModule stickyControlModule = null;
        private int popupPrefabID = StickyManager.NoPrefabID;
        private int numMessagesRequired = 3;
        /// <summary>
        /// The popup that is currently being shown in the scene.
        /// </summary>
        private StickyPopupModule activeStickyPopupModule = null;
        private bool isPopupActive = false;
        private S3DPopupMessage displayMsg3 = null;
        private S3DPopupMessage initialisedXRMsg = null;
        private S3DPopupMessage displayMsg2 = null;

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
            if (stickyInputModule == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] Did you forget to add a reference to your Sticky3D character in the scene on " + name + "?");
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

                stickyControlModule = stickyInputModule.GetStickyControlModule(true);

                isInitialised = popupPrefabID != StickyManager.NoPrefabID;
            }
        }

        #endregion

        #region Update Methods
        void Update()
        {
            if (isPopupActive)
            {
                #if SCSM_XR && SSC_UIS
                bool isXRInitialised = stickyInputModule.IsInitialisedXR;
                int xrCount = stickyInputModule.InitialisedXRCount;
                bool isHMDValid = stickyInputModule.IsHMDInputDeviceXRValid;
                float refreshRate = UnityEngine.XR.XRDevice.refreshRate;
                Vector3 hmdPos = stickyInputModule.GetCharacterInputXR.hmdPosition;
                #else
                bool isXRInitialised = false;
                bool isHMDValid = false;
                float refreshRate = 0f;
                int xrCount = 0;
                Vector3 hmdPos = Vector3.zero;
                #endif

                activeStickyPopupModule.SetMessageValueText(initialisedXRMsg, isXRInitialised ? msgYes : msgNo);

                string msgText = string.Empty;

                switch (displayOption2)
                {
                    case DisplayOption2.HMD_AxisX:
                        msgText = S3DUtils.GetNumericString(hmdPos.x, 3, false);
                        break;
                    case DisplayOption2.HMD_AxisY:
                        msgText = S3DUtils.GetNumericString(hmdPos.y, 3, false);
                        break;
                    case DisplayOption2.HMD_AxisZ:
                        msgText = S3DUtils.GetNumericString(hmdPos.z, 3, false);
                        break;
                    case DisplayOption2.HeightCalibrated:
                        msgText = stickyControlModule.IsHeightCalibratedVR ? msgYes : msgNo;
                        break;
                }

                activeStickyPopupModule.SetMessageValueText(displayMsg2, msgText);

                switch (displayOption3)
                {
                    case DisplayOption3.HMD_AxisX:
                        msgText = S3DUtils.GetNumericString(hmdPos.x, 3, false);
                        break;
                    case DisplayOption3.HMD_AxisY:
                        msgText = S3DUtils.GetNumericString(hmdPos.y, 3, false);
                        break;
                    case DisplayOption3.HMD_AxisZ:
                        msgText = S3DUtils.GetNumericString(hmdPos.z, 3, false);
                        break;
                    case DisplayOption3.HMDValid:
                        msgText = isHMDValid ? msgYes : msgNo;
                        break;
                    case DisplayOption3.VRRefreshRate:
                        msgText = S3DUtils.GetNumericString(refreshRate, 1, false);
                        break;
                    case DisplayOption3.XRInitCount:
                        msgText = S3DUtils.GetNumericString(xrCount, 0, false);
                        break;
                }

                activeStickyPopupModule.SetMessageValueText(displayMsg3, msgText);
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

                    initialisedXRMsg = activeStickyPopupModule.GetPopupMessageByIndex(0);
                    displayMsg2 = activeStickyPopupModule.GetPopupMessageByIndex(1);
                    displayMsg3 = activeStickyPopupModule.GetPopupMessageByIndex(2);

                    string msgString = string.Empty;

                    activeStickyPopupModule.SetMessageLabelText(initialisedXRMsg, "XR Initialised");

                    switch (displayOption2)
                    {
                        case DisplayOption2.HMD_AxisX:
                            msgString = "HMD X";
                            break;
                        case DisplayOption2.HMD_AxisY:
                            msgString = "HMD Y";
                            break;
                        case DisplayOption2.HMD_AxisZ:
                            msgString = "HMD Z";
                            break;
                        case DisplayOption2.HeightCalibrated:
                            msgString = "Height Calibrated";
                            break;
                    }

                    activeStickyPopupModule.SetMessageLabelText(displayMsg2, msgString);
     
                    switch (displayOption3)
                    {
                        case DisplayOption3.HMD_AxisX:
                            msgString = "HMD X";
                            break;
                        case DisplayOption3.HMD_AxisY:
                            msgString = "HMD Y";
                            break;
                        case DisplayOption3.HMD_AxisZ:
                            msgString = "HMD Z";
                            break;
                        case DisplayOption3.HMDValid:
                            msgString = "HMD Valid";
                            break;
                        case DisplayOption3.VRRefreshRate:
                            msgString = "Refresh Rate";
                            break;
                        case DisplayOption3.XRInitCount:
                            msgString = "XR Count";
                            break;
                    }

                    activeStickyPopupModule.SetMessageLabelText(displayMsg3, msgString);

                    activeStickyPopupModule.ShowMessage(initialisedXRMsg);
                    activeStickyPopupModule.ShowMessage(displayMsg2);
                    activeStickyPopupModule.ShowMessage(displayMsg3);

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