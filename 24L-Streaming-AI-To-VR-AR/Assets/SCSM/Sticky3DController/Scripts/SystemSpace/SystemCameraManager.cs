using UnityEngine;

// Copyright (c) 2015-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// [FUTURE - WIP]
    /// Class that moves a camera through a scale model of the solar system, syncronised to the movement of a local space camera.
    /// </summary>
    [RequireComponent(typeof(SystemSpaceManager))]
    public class SystemCameraManager : MonoBehaviour
    {
        #region Public Variables

        /// <summary>
        /// The parent object of the scale solar system model.
        /// </summary>
        public GameObject scaleSystemModel;

        /// <summary>
        /// The system camera that views the scale model of solar system.
        /// </summary>
        public Camera systemCamera;

        /// <summary>
        /// The local camera that is viewing the world.
        /// </summary>
        public Camera localCamera;

        #endregion

        #region Private Variables

        // The scene SystemSpaceManager
        private SystemSpaceManager systemSpaceManager;

        #endregion

        #region Update Methods

        // Awake is called before the first frame update
        void Awake()
        {
            // Enable the scale model of the solar system
            if (scaleSystemModel != null)
            {
                scaleSystemModel.SetActive(true);
            }

            // Retrieve the SystemSpaceManager
            systemSpaceManager = GetComponent<SystemSpaceManager>();

            UpdateTransform();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            UpdateTransform();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the position and rotation of the micro system camera based on system position and local camera transform.
        /// </summary>
        private void UpdateTransform()
        {
            if (systemSpaceManager != null && localCamera != null && systemCamera != null)
            {
                // Sets the position of the system camera
                systemCamera.transform.position = systemSpaceManager.SystemSpaceTransform(localCamera.transform.position);

                // Sets the rotation to the local camera rotation
                systemCamera.transform.rotation = localCamera.transform.rotation;

                // Sets the FOV
                systemCamera.fieldOfView = localCamera.fieldOfView;
            }
#if UNITY_EDITOR
            else { Debug.LogWarning("Missing Components!"); }
#endif
        }

        #endregion
    }
}
