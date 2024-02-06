using UnityEngine;

// Copyright (c) 2015-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// [FUTURE - WIP]
    /// Class that keeps track of system space.
    /// </summary>
    public class SystemSpaceManager : MonoBehaviour
    {
        #region Public Variables

        /// <summary>
        /// Each system space unit is equal to this many local space units.
        /// </summary>
        public float systemSpaceScale = 1f;

        /// <summary>
        /// Initial position of the local origin in system space.
        /// </summary>
        public Vector3 initialSystemPosition = Vector3.zero;

        #endregion

        #region Private Variables

        // Position of the local origin in system space
        private Vector3 systemPosition;

        #endregion

        #region Initialisation Methods

        // Awake is called before the first frame update
        void Awake()
        {
            systemPosition = initialSystemPosition;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the position of the local origin in system space.
        /// </summary>
        /// <param name="position"></param>
        public void SetSystemPosition(Vector3 position)
        {
            systemPosition = position;
        }

        /// <summary>
        /// Gets the position of the local origin in system space.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetSystemPosition()
        {
            return systemPosition;
        }

        /// <summary>
        /// Moves system position by a local space vector (accounting for unit scale)
        /// </summary>
        /// <param name="lDelta"></param>
        public void AddLocalDelta(Vector3 lDelta)
        {
            systemPosition += lDelta / systemSpaceScale;
        }

        /// <summary>
        /// Transforms a local position into system space
        /// </summary>
        /// <param name="localPosition"></param>
        /// <returns></returns>
        public Vector3 SystemSpaceTransform(Vector3 localPosition)
        {
            return systemPosition + (localPosition / systemSpaceScale);
        }

        #endregion
    }
}
