using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// A demo script that can be attached to an object to enable or disable the renderer at a given distance from the camera.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class DemoSimpleLOD : MonoBehaviour
    {
        public float updateInterval = 1f;
        private float updateIntervalTimer = 0f;

        /// <summary>
        /// Use SetCamera(newCamera) at runtime.
        /// </summary>
        public Camera playerCamera;

        private Vector3 playerCameraPos;

        public float maxXZDistance = 50f;
        public float maxYDistance = 100f;
        private float sqrMaxXZDistance;
        private float sqrMaxYDistance;
        private float currentSqrXZDistance;
        private float currentSqrYDistance;
        private Vector3 meshToCameraOffset = Vector3.zero;

        private MeshRenderer meshRenderer;
        private Bounds meshBounds;
        private bool isInitialised = false;

        // Use this for initialization
        void Awake()
        {
            // Find the mesh renderer attached to this object
            if (TryGetComponent(out meshRenderer))
            {
                // Get the bounds of the mesh
                meshBounds = meshRenderer.bounds;

                // Choose a random starting time for timer
                updateIntervalTimer = UnityEngine.Random.Range(0f, updateInterval);

                // Pre-calculate square distances
                sqrMaxXZDistance = maxXZDistance * maxXZDistance;
                sqrMaxYDistance = maxYDistance * maxYDistance;

                isInitialised = playerCamera != null;
            }
        }

        // Update is called once per frame
        void Update ()
        {
            if (!isInitialised) { return; }

            // Increment the update interval timer
            updateIntervalTimer += Time.deltaTime;

            // Do an update if the update interval time has elapsed
            if (updateIntervalTimer >= updateInterval)
            {
                if (playerCamera != null)
                {
                    // Decide whether or not to render the mesh
                    bool renderMesh = false;
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
                    renderMesh = currentSqrXZDistance < sqrMaxXZDistance && currentSqrYDistance < sqrMaxYDistance;

                    // If the render setting does not match what we have decided, change it
                    if (meshRenderer.enabled != renderMesh) { meshRenderer.enabled = renderMesh; }
                }

                // Reset the update interval timer
                updateIntervalTimer = 0f;
            }
        }

        /// <summary>
        /// Used to set the camera at runtime.
        /// </summary>
        /// <param name="newPlayerCamera"></param>
        public void SetCamera (Camera newPlayerCamera)
        {
            playerCamera = newPlayerCamera;
            isInitialised = playerCamera != null;
        }
    }
}
