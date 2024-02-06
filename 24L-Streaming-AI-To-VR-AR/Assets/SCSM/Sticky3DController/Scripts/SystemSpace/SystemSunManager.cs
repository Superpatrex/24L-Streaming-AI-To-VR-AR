using UnityEngine;

// Copyright (c) 2015-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// [FUTURE - WIP]
    /// Class that rotates the local sun directional light to emulate light from the scale model of the solar system.
    /// </summary>
    [RequireComponent(typeof(SystemSpaceManager))]
    public class SystemSunManager : MonoBehaviour
    {
        #region Public Variables

        /// <summary>
        /// The transform of the sun in the scale system model.
        /// </summary>
        public Transform scaleSunTransform;

        /// <summary>
        /// The directional light that simulates the sun locally.
        /// </summary>
        public Light sunDirectionalLight;

        /// <summary>
        /// The position from which sun direction is calculated (perhaps the camera of the local player).
        /// </summary>
        public Transform localSunObserver;

        #endregion

        #region Private Variables

        // The scene SystemSpaceManager
        private SystemSpaceManager systemSpaceManager;

        #endregion

        #region Initialisation Methods

        // Awake is called before the first frame update
        void Awake()
        {
            // Retrieve the SystemSpaceManager
            systemSpaceManager = GetComponent<SystemSpaceManager>();

            UpdateTransform();
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            UpdateTransform();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the rotation of the local sun light based on systemPosition and the sun's position
        /// </summary>
        private void UpdateTransform()
        {
            if (systemSpaceManager != null && sunDirectionalLight != null)
            {
                // Calculate the direction vector from the sun to the observation point
                Vector3 forward = systemSpaceManager.SystemSpaceTransform(localSunObserver.position) - scaleSunTransform.position;

                // Set the rotation of the sun directional light to point in the forward direction
                sunDirectionalLight.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            }
#if UNITY_EDITOR
            else { Debug.LogWarning("Missing Components!"); }
#endif
        }

        #endregion
    }
}
