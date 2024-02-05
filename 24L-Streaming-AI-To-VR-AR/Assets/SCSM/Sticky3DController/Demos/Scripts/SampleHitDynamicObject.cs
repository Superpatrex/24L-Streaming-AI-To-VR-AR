using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple script to demonstrate how to instantiate a dynamic object and
    /// destroy it (return it to the pool) when it has taken too much damage.
    /// Setup:
    /// 1. Add a character to the scene that is configured to grab and shoot a weapon.
    /// 2. Add a grabbable StickyWeapon to your scene.
    /// 3. Create an empty gameobject in the scene and attach this script.
    /// 4. Move the gameobject to where you want your target to be spawned.
    /// 5. On SampleHitDynamicObject, enable InitialiseOnStart
    /// 6. Add in the Demos\Prefabs\DynamicObjects\StickyDynamicTarget1 prefab.
    /// 7. Run the scene and start shooting at it. After enough hits, it will be destroyed.                                                                                                                                                                                  
    /// NOTE:
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Hit Dynamic Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class SampleHitDynamicObject : MonoBehaviour
    {
        #region Public Variables
        [Tooltip("If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the component is enabled through code.")]
        public bool initialiseOnStart = false;

        [Tooltip("Add Demos -> Prefabs -> DynamicObjects -> StickyDynamicTarget1")]
        public StickyDynamicModule demoDynamicTargetPrefab = null;

        #endregion

        #region Public Properties

        #endregion

        #region Private Variables - General
        private bool isInitialised = false;
        private StickyManager stickyManager = null;
        private int dynamicPrefabID = StickyManager.NoPrefabID;
        #endregion

        #region Private Initialise Methods

        // Use this for initialization
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
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
            if (demoDynamicTargetPrefab == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleHitDynamicObject - did you forget to add the StickyDynamicTarget prefab from the Demos, Prefabs, Dynamic Objects, folder?");
                #endif
            }
            else
            {
                dynamicPrefabID = stickyManager.GetOrCreateDynamicPool(demoDynamicTargetPrefab);

                // This is an overly simplified example of just creating a single item in the scene
                // which the player can fire at.
                if (dynamicPrefabID != StickyManager.NoPrefabID)
                {
                    // Instantiate the pooled dynamic object    

                    S3DInstantiateDynamicObjectParameters idParms = new S3DInstantiateDynamicObjectParameters()
                    {
                        dynamicObjectPrefabID = dynamicPrefabID,
                        position = transform.position,
                        rotation = transform.rotation
                    };

                    StickyDynamicModule stickyDynamicModule = stickyManager.InstantiateDynamicObject(ref idParms);

                    isInitialised = stickyDynamicModule != null;
                }
            }
        }

        #endregion

    }
}