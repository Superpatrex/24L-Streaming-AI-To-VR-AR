using UnityEngine;
using scsmmedia;

// Sticky3D Control Module Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace MyUniqueGame
{
    /// <summary>
    /// This demo script shows how to spawn pooled Mesh Text using a customised
    /// StickyGenericModule. This demo script is designed to work with in VR
    /// but it could be adapter (in your own code) to work with a non-VR character.
    /// WARNING: This is a DEMO script and is subject to change without notice during
    /// upgrades. This is just to show you how to do things in your own code.
    /// Setup:
    /// 1. Add this to an empty gameobject in the scene
    /// 2. Create a prefab using a SampleGenericTextModule
    /// 3. Add a Grabbable and Touchable interactive object to the scene. e.g. S3D_Lever1
    /// 4. Add the interactive-enabled object from the scene to this component
    /// 5. Add the prefab from the project folder to this component
    /// 6. On the interactive object in the scene, add an OnGrabbed event
    /// 7. Drag this component from the scene into the OnGrabbed event
    /// 8. Set the function for the OnGrabbed event to DemoS3DTextSpawner.GrabbedInteractiveObject
    /// 9. On the interactive object, add an OnDropped event
    /// 10. Drag this component from the scene into the OnDropped event
    /// 11. Set the function for the OnGrabbed event to DemoS3DTextSpawner.ReleasedInteractiveObject
    /// 12. On the interactive tab, set the popup offset to where you want the "Release" message to appear.
    ///     The "Grab" message will appear slightly above that.
    /// See also SampleGenericTextModule.cs.
    /// </summary>
    public class DemoS3DTextSpawner : MonoBehaviour
    {
        #region Public Variables

        [Tooltip("A SampleGenericTextModule prefab")]
        public GameObject textPrefab = null;

        [Tooltip("An interactive-enabled object in the scene that is Grabbable and Touchable")]
        public StickyInteractive stickyInteractive = null;
        #endregion

        #region Private Variables
        private StickyManager stickyManager = null;
        private int textObjectPrefabID = -1;
        private StickyGenericModule textModule = null;
        private Transform interactiveParent = null;
        private bool isInitialised = false;

        private readonly static string ReleaseText = "release";
        private readonly static string GrabText = "grab";
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            stickyManager = StickyManager.GetOrCreateManager(gameObject.scene.handle);

            if (stickyManager != null)
            {
                if (textPrefab != null && textPrefab.TryGetComponent(out textModule))
                {
                    textObjectPrefabID = stickyManager.GetOrCreateGenericPool(textModule);
                }
            }

            if (stickyInteractive != null)
            {
                interactiveParent = stickyInteractive.transform.parent;
            }

            isInitialised = interactiveParent != null && textObjectPrefabID >= 0;

        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get a SampleGenericTextModule from the pool and parent it to the interactive-enabled object.
        /// </summary>
        /// <param name="isRelease"></param>
        private void CreateText(bool isRelease)
        {
            if (isInitialised)
            {
                // Position the Release message at the popup position
                Vector3 popupPosition = stickyInteractive.GetPopupPosition();

                // Setup the generic parameters
                // Position Grab text to the left of the object
                S3DInstantiateGenericObjectParameters igParms = new S3DInstantiateGenericObjectParameters
                {
                    position = isRelease ? popupPosition : popupPosition + (interactiveParent.up * 0.1f),
                    rotation = interactiveParent.rotation,
                    genericModulePrefabID = textObjectPrefabID
                };

                StickyGenericModule stickyGenericModule = stickyManager.InstantiateGenericObject(ref igParms);

                if (stickyGenericModule != null)
                {
                    stickyGenericModule.transform.SetParent(interactiveParent);

                    // Configure our extended functionality
                    MyUniqueGame.SampleGenericTextModule genericTextModule = (MyUniqueGame.SampleGenericTextModule)stickyGenericModule;
                    genericTextModule.SetText(isRelease ? ReleaseText : GrabText);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The object was dropped.
        /// Call this from the OnDropped event of the interactive-enabled object
        /// </summary>
        public void ReleasedInteractiveObject()
        {
            CreateText(true);
        }

        /// <summary>
        /// The object has been grabbed, so spawn the mesh text nearby.
        /// Call this from the OnGrabbed event of the interactive-enabled object
        /// </summary>
        public void GrabbedInteractiveObject()
        {
            if (isInitialised)
            {
                CreateText(false);
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("DemoS3DTextSpawner GrabInteractiveObject - the component is not initialised.");
            }
            #endif
        }

        #endregion
    }
}