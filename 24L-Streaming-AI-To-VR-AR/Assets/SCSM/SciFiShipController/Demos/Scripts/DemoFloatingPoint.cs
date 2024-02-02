using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Simple demo that acts a bit like a floating point error manager.
    /// It is currently used in the Demos\scenes\Demo Floating Point scene.
    /// See also Demos\Scripts\SampleTelePortWorld which moves Paths and AI Ships.
    /// NOTE: This is NOT designed to solve all FP error issues.
    /// This is only a sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own
    /// namespace.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Floating Point")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class DemoFloatingPoint : MonoBehaviour
    {
        #region Public Variables
        public ShipControlModule playerShip;
        public ShipCameraModule shipCamera;
        public float maxMoveDistance = 50f;
        public GameObject environment = null;
        #endregion

        #region Private Variables - General
        private bool isInitialised = false;

        private Vector3 startPosition = Vector3.zero;
        private bool isShipTeleportRequired = false;
        private bool isCameraTeleportRequired = false;
        private Vector3 deltaShipMove = Vector3.zero;
        private Vector3 deltaCameraMove = Vector3.zero;
        private float maxDistSqr = 0f;
        private SSCManager sscManager = null;
        #endregion

        #region Private Initialise Methods

        // Start is called before the first frame update
        void Start()
        {
            if (playerShip != null && shipCamera != null && environment != null)
            {
                startPosition = playerShip.shipInstance.TransformPosition;

                maxDistSqr = maxMoveDistance * maxMoveDistance;

                sscManager = SSCManager.GetOrCreateManager(gameObject.scene.handle);

                isInitialised = true;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Move the player ship (and any other associated movable objects)
        /// Do NOT include the SSC camera here.
        /// NOTE: Currently doesn't move particle FX or sounds
        /// </summary>
        /// <param name="deltaMove"></param>
        private void TelePortMovableObjects (Vector3 deltaTP)
        {
            // Move any paths in the scene
            //sscManager.MoveLocations(myPath, Time.deltaTime, deltaTP, Quaternion.identity);

            // For AI ships, teleport the Ship AI Input Module rather than the ShipControlModule
            //shipAIInputModule.TelePort(deltaTP, false);

            playerShip.TelePort(deltaTP, false);

            sscManager.TelePortProjectiles(deltaTP);
            sscManager.TelePortBeams(deltaTP);
        }

        /// <summary>
        /// Move static objects in the scene.
        /// We have a single directional light so probably don't need to move that
        /// </summary>
        /// <param name="deltaTP"></param>
        private void TelePortStaticObjects (Vector3 deltaTP)
        {
            environment.transform.position -= deltaTP;
        }

        #endregion

        #region Update Methods

        private void FixedUpdate()
        {
            if (!isInitialised) { return; }

            deltaShipMove = playerShip.shipInstance.TransformPosition - startPosition;

            if (deltaShipMove.sqrMagnitude >= maxDistSqr)
            {
                deltaCameraMove = deltaShipMove;
                isCameraTeleportRequired = true;
                isShipTeleportRequired = true;
            }

            if (isShipTeleportRequired)
            {
                TelePortMovableObjects(-deltaShipMove);
                isShipTeleportRequired = false;
            }
        }

        private void LateUpdate()
        {
            if (!isInitialised) { return; }

            if (isCameraTeleportRequired)
            {
                shipCamera.TelePort(-deltaCameraMove);
                isCameraTeleportRequired = false;

                // Move the world
                TelePortStaticObjects(deltaCameraMove);
            }
        }

        #endregion

    }
}