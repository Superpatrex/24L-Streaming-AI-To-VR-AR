using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple script to attach to a character to show Equip, Grab or Stash
    /// options for an interactive-enabled object.
    /// SETUP:
    /// 1. Attach this to your S3D character parent gameobject which includes the StickyControlModule component.
    /// 2. Add the demos\prefabs\visuals\StickyPopup6.prefab to the stickyPopup1 slot on this component.
    /// 3. Enable "Initialise on Start" for this component.
    /// 4. Add an interactive-enabled object (like the S3D_P320M17 weapon) to the scene
    /// 5. On the interactive object, on the "Interactive" tab, ensure "Initialise on Start" is enabled.
    /// 6. On the "Interactive" tab, turn on "Is Activable".
    /// 7. Under "Is Activable", turn on "Auto Deactivate" and "Priority on Grab"
    /// 8. On the "Interactive" tab, turn on "Is Equippable"
    /// 9. Under "Is Equippable", set the Equip Offset and Rotation.
    /// 10. On the Sticky3D character, go to the "Events" tab, and add an event to "On Activated"
    /// 11. Drag the Sticky3D character into the Object slot for the new event.
    /// 12. For the "Function", select "SampleEquipGrabStashPopup", and "ShowPopup(StickyInteractive)"
    /// 13. Drag the interactive object (e.g. S3D_P320M17) from the scene into the StickInteractive slot for
    ///     the event Function parameter.
    /// NOTE: If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Equip Grab Stash Popup")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class SampleEquipGrabStashPopup : MonoBehaviour
    {
        #region Public Variables
        [Tooltip("If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the component is enabled through code.")]
        public bool initialiseOnStart = false;
        [Tooltip("The sticky popup prefab to display when activating, or engaging with, the interactive object")]
        public StickyPopupModule stickyPopup1 = null;

        #endregion

        #region Public Properties

        public bool IsInitialised { get { return isInitialised; } }

        #endregion

        #region Private Variables - General
        private StickyControlModule thisCharacter = null;
        private StickyManager stickyManager = null;
        private bool isInitialised = false;
        private int popupPrefabID = -1;
        #endregion

        #region Private Initialise Methods

        // Use this for initialization
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This is automatically called when the user clicks on an item in the options popup
        /// </summary>
        /// <param name="stickyPopupModule"></param>
        /// <param name="itemNumber"></param>
        /// <param name="stickyInteractive"></param>
        private void PopupActioned(StickyPopupModule stickyPopupModule, int itemNumber, StickyInteractive stickyInteractive)
        {
            // Close the popup after use - i.e. return it to the pool.
            if (stickyPopupModule != null) { stickyPopupModule.DestroyGenericObject(); }

            if (itemNumber == 2)
            {
                thisCharacter.StashItem(stickyInteractive);
            }
            else if (itemNumber == 3)
            {
                thisCharacter.GrabInteractive(stickyInteractive, false, false);
            }
            else if (itemNumber == 4)
            {
                thisCharacter.EquipItemNoReturn(stickyInteractive);
            }
        }

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Initialise this component. Has no effect if already initialised
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            stickyManager = StickyManager.GetOrCreateManager(gameObject.scene.handle);

            // For the sake of the sample, do a bunch of error checking. In game code, you would probably
            // do all this checking on one line.
            if (!gameObject.TryGetComponent(out thisCharacter))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleEquipGrabStashPopup - did you forget to attach this script to your player S3D character?");
                #endif
            }
            else if (!thisCharacter.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleEquipGrabStashPopup - your character " + thisCharacter.name + " is not initialised");
                #endif
            }
            else if (stickyPopup1 == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleEquipGrabStashPopup - " + thisCharacter.name + " is missing the StickyPopupModule prefab");
                #endif
            }
            else
            {
                popupPrefabID = stickyManager.GetOrCreateGenericPool(stickyPopup1);

                isInitialised = popupPrefabID != StickyManager.NoPrefabID;
            }
        }

        /// <summary>
        /// Hook this method up to the On Activated event of an interactive-enabled object.
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

                    stickyPopupModule.SetCamera(thisCharacter.lookCamera1);

                    // Determine which options are available for this interactive object
                    stickyPopupModule.SetItem(2, true, stickyInteractive.IsStashable);
                    stickyPopupModule.SetItem(3, true, stickyInteractive.IsGrabbable);
                    stickyPopupModule.SetItem(4, true, stickyInteractive.IsEquippable);

                    // Call a method when an item is selected by the player
                    stickyPopupModule.SetItemAction(1, PopupActioned);
                    stickyPopupModule.SetItemAction(2, PopupActioned);
                    stickyPopupModule.SetItemAction(3, PopupActioned);
                    stickyPopupModule.SetItemAction(4, PopupActioned);
                }
            }
        }

        #endregion
    }
}