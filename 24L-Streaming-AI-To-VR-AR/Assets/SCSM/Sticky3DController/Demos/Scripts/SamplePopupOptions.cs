using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Sample script to demonstrate a popup option list appearing when the character moves the cursor
    /// over an interactive-enabled object.
    /// Setup:
    /// 1. Add this script to an empty gameobject in the scene
    /// 2. Rename the empty gameobject PopupOptions
    /// 3. Add StickyPopup1 from the Demos\Prefabs\Visuals folder to the StickyPopupPrefab slot
    /// 4. Add a interactive-enabled object to the scene. e.g. Demos\Prefabs\S3D_Briefcase1
    /// 5. On the interactive object, add a On Hover Enter event
    /// 6. Drag in the PopupOptions gameobect into the Hover Enter event
    /// 7. Set the Function to SamplePopupOptions > ShowPopup(StickyInteractive)
    /// 8. Drag the interactive object (e.g. S3D_Briefcase1) into the slot provided in the event
    /// 9. Configure (or add) a StickyDisplayModule in the scene to Lock Reticle to Cursor 
    /// 10. Ensure you have a UI EventSystem in the scene.
    /// 11. Drag your Sticky3D character into the slot provided on this component.
    /// WARNING: This is a DEMO script and is subject to change without notice during
    /// upgrades. This is just to show you how to do things in your own code.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Popup Options")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class SamplePopupOptions : MonoBehaviour
    {
        #region Public Variables
        public StickyPopupModule stickyPopupPrefab = null;
        public StickyControlModule s3dPlayer = null;
        #endregion

        #region Private Variables - General
        private bool isInitialised = false;
        private StickyManager stickyManager = null;
        private int popupPrefabID = 0;
        #endregion

        #region Private Initialise Methods

        // Use this for initialization
        void Awake()
        {
            stickyManager = StickyManager.GetOrCreateManager(gameObject.scene.handle);

            if (stickyPopupPrefab != null && stickyManager != null)
            {
                popupPrefabID = stickyManager.GetOrCreateGenericPool(stickyPopupPrefab);

                #if UNITY_EDITOR
                if (s3dPlayer == null || s3dPlayer.IsNPC())
                {
                    Debug.LogWarning("SamplePopupOptions - if you do not add a Sticky3D player character, the popup will not face the player.");
                }
                #endif

                isInitialised = true;
            }
        }

        #endregion

        #region Public Methods - General

        /// <summary>
        /// This method is called automatically by StickyPopupModule when an item is clicked. 
        /// </summary>
        /// <param name="stickyPopupModule"></param>
        /// <param name="itemNumber"></param>
        /// <param name="stickyInteractive"></param>
        public void ItemClicked (StickyPopupModule stickyPopupModule, int itemNumber, StickyInteractive stickyInteractive)
        {
            string interactiveObject = stickyInteractive == null ? "unknown Interactive-enabled object" : stickyInteractive.name;

            Debug.Log("[DEBUG] item " + itemNumber + " clicked on " + interactiveObject + " T:" + Time.time);

            // Close the popup after use - i.e. return it to the pool.
            if (stickyPopupModule != null) { stickyPopupModule.DestroyGenericObject(); }
        }

        /// <summary>
        /// Show the popup for this interactive-enabled object. It is automatically called by the Interactive-enabled onHoverEnter
        /// event. Only show a popup if it is being pooled and it is not already active over the interactive-enabled object.
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
                    genericModulePrefabID = popupPrefabID
                };

                StickyGenericModule stickyGenericModule = stickyManager.InstantiateGenericObject(ref igParms);

                if (stickyGenericModule != null)
                {
                    // Configure our extended functionality
                    StickyPopupModule stickyPopupModule = (StickyPopupModule)stickyGenericModule;

                    // Tell the popup which interactive-enabled object triggered it
                    stickyPopupModule.SetInteractiveObject(stickyInteractive);

                    if (s3dPlayer != null) { stickyPopupModule.SetCamera(s3dPlayer.lookCamera1); }

                    // Our StickyPopup1 prefab has 3 items or option buttons. So set them here
                    // To avoid passing strings, you could have them preset in the prefab.
                    stickyPopupModule.SetItem(1, "Option A");
                    stickyPopupModule.SetItem(2, "Option B");
                    // Hide and disable item 3
                    stickyPopupModule.SetItem(3, "Option C", true, false);

                    // We could also Hide or Show items without calling SetItem
                    stickyPopupModule.ShowItem(3);

                    // Tell the popup to call our method when an item is clicked
                    stickyPopupModule.SetItemAction(1, ItemClicked);
                    stickyPopupModule.SetItemAction(2, ItemClicked);
                    stickyPopupModule.SetItemAction(3, ItemClicked);
                }
            }
        }

        #endregion
    }
}