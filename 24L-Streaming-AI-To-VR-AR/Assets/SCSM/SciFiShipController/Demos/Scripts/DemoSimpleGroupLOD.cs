using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// A demo script that can be attached to a parent gameobject to enable or disable
    /// the child renderers at a given distance from the camera.
    /// </summary>
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class DemoSimpleGroupLOD : MonoBehaviour
    {
        #region Public Variables

        [Tooltip("If this is false, you need to call Initialise() in your code")]
        public bool initialiseOnStart = false;

        [Tooltip("How often, in seconds, the distance to the camera is checked")]
        public float updateInterval = 1f;

        [Tooltip("If child objects can be destroyed, enable this.")]
        public bool checkNullObjects = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Is the component initialised? Unless the camera has been destroyed,
        /// it should also have a valid camera when true.
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        #endregion

        #region Public Static Variables

        #endregion

        #region Private Variables - General

        [SerializeField] private float maxXZDistance = 50f;
        [SerializeField] private float maxYDistance = 100f;
        [SerializeField] private Camera playerCamera;

        private float sqrMaxXZDistance;
        private float sqrMaxYDistance;
        private float currentSqrXZDistance;
        private float currentSqrYDistance;
        private Vector3 meshToCameraOffset = Vector3.zero;
        private Vector3 playerCameraPos;

        private List<MeshRenderer> meshRendererList;
        private Bounds meshBounds;
        private bool isInitialised = false;
        private int numMeshRenderers = 0;
        private float updateIntervalTimer = 0f;

        #endregion

        #region Private Initialise Methods

        // Use this for initialization
        void Start()
        {
            if (initialiseOnStart) { Iniitalise(); }
        }

        #endregion

        #region Update Methods

        void Update()
        {
            if (!isInitialised) { return; }

            // Increment the update interval timer
            updateIntervalTimer += Time.deltaTime;

            // Do an update if the update interval time has elapsed
            if (updateIntervalTimer >= updateInterval)
            {
                if (playerCamera != null)
                {
                    RenderChildObjects(IsRenderMesh());
                }

                // Reset the update interval timer
                updateIntervalTimer = 0f;
            }
        }

        #endregion

        #region Private and Internal Methods - General

        /// <summary>
        /// Decide whether or not to render the mesh
        /// </summary>
        /// <returns></returns>
        private bool IsRenderMesh()
        {
            playerCameraPos = playerCamera.transform.position;

            // Determine how far away from the mesh bounds we are on each axis
            // Get vector from centre of bounds to player camera position
            meshToCameraOffset = playerCameraPos - meshBounds.center;
            // Take absolute value of offset, then minus half of bounds size from it
            meshToCameraOffset.x = meshToCameraOffset.x > 0f ? (meshToCameraOffset.x - meshBounds.extents.x) : 
                (-meshToCameraOffset.x - meshBounds.extents.x);
            // Discard negative values (they are inside of the bounds)
            meshToCameraOffset.x = meshToCameraOffset.x > 0f ? meshToCameraOffset.x : 0f;
            // Take absolute value of offset, then minus half of bounds size from it
            meshToCameraOffset.y = meshToCameraOffset.y > 0f ? (meshToCameraOffset.y - meshBounds.extents.y) :
                (-meshToCameraOffset.y - meshBounds.extents.y);
            // Discard negative values (they are inside of the bounds)
            meshToCameraOffset.y = meshToCameraOffset.y > 0f ? meshToCameraOffset.y : 0f;
            // Take absolute value of offset, then minus half of bounds size from it
            meshToCameraOffset.z = meshToCameraOffset.z > 0f ? (meshToCameraOffset.z - meshBounds.extents.z) :
                (-meshToCameraOffset.z - meshBounds.extents.z);
            // Discard negative values (they are inside of the bounds)
            meshToCameraOffset.z = meshToCameraOffset.z > 0f ? meshToCameraOffset.z : 0f;

            // Get square distances to edge of mesh bounds
            currentSqrXZDistance = (meshToCameraOffset.x * meshToCameraOffset.x) + (meshToCameraOffset.z * meshToCameraOffset.z);
            currentSqrYDistance = (meshToCameraOffset.y * meshToCameraOffset.y);

            // Compare distances to max distances to determine whether or not mesh should be rendered
            return currentSqrXZDistance < sqrMaxXZDistance && currentSqrYDistance < sqrMaxYDistance;
        }

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Initialise this component. Has no effect if already initialised.
        /// </summary>
        public void Iniitalise()
        {
            if (isInitialised) { return; }

            meshRendererList = new List<MeshRenderer>(10);

            GetComponentsInChildren(true, meshRendererList);

            numMeshRenderers = meshRendererList.Count;

            // Get the bounds for all the child objects
            for (int mrIdx = 0; mrIdx < numMeshRenderers; mrIdx++)
            {
                meshBounds.Encapsulate(meshRendererList[mrIdx].bounds);
            }

            // Choose a random starting time for timer
            updateIntervalTimer = UnityEngine.Random.Range(0f, updateInterval);

            // Pre-calculate square distances
            sqrMaxXZDistance = maxXZDistance * maxXZDistance;
            sqrMaxYDistance = maxYDistance * maxYDistance;

            isInitialised = numMeshRenderers > 0 && playerCamera != null;
        }

        /// <summary>
        /// Enable or disable rendering of child objects
        /// </summary>
        /// <param name="isRender"></param>
        public void RenderChildObjects (bool isRender)
        {
            for (int mrIdx = 0; mrIdx < numMeshRenderers; mrIdx++)
            {
                MeshRenderer mRen = meshRendererList[mrIdx];

                if (!checkNullObjects || mRen != null)
                {
                    mRen.enabled = isRender;
                }
            }
        }

        /// <summary>
        /// Used to set the camera at runtime.
        /// </summary>
        /// <param name="newPlayerCamera"></param>
        public void SetCamera (Camera newPlayerCamera)
        {
            playerCamera = newPlayerCamera;
            isInitialised = numMeshRenderers > 0 && playerCamera != null;

            if (isInitialised) { RenderChildObjects(IsRenderMesh()); }
        }

        #endregion

    }
}