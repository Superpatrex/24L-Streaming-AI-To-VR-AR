using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class containing data for planets that are used in the Celestials demo component
    /// </summary>
    [System.Serializable]
    public class SSCCelestial
    {
        #region Public variables

        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// The name of the planet or celestial object.
        /// </summary>
        [Tooltip("The name of the planet or celestial object")]
        public string name;

        /// <summary>
        /// The minimum scaled size of the celestial
        /// </summary>
        [Tooltip("The minimum scaled size of the celestial")]
        [Range(1, 20), Min(1)] public int minSize;

        /// <summary>
        /// The maximum scaled size of the celestial
        /// </summary>
        [Tooltip("The maximum scaled size of the celestial")]
        [Range(1, 20), Min(1)] public int maxSize;

        [Tooltip("The minimum relative distance the celestial is from the camera")]
        [Range(0f, 1f)] public float minDistance;

        [Tooltip("The maximum relative distance the celestial is from the camera")]
        [Range(0f, 1f)] public float maxDistance;

        [Tooltip("Should the position of the planet be randomly generated")]
        public bool isRandomPosition;

        [Tooltip("The relative left or right position of the planet")]
        [Range(-1f, 1f)] public float positionX;

        [Tooltip("The relative up or down position of the planet")]
        [Range(-1f, 1f)] public float positionY;

        [Tooltip("The relative forward or back position of the planet")]
        [Range(-1f, 1f)] public float positionZ;

        [Tooltip("Rotation in degrees. Has no effect if Is Face Camera is enabled.")]
        public Vector3 rotation;

        [Tooltip("The optional material for the celestial object")]
        public Material celestialMaterial;

        [Tooltip("Optional mesh for the celestial object. Will default to a Unity sphere.")]
        public Mesh celestialMesh;

        [Tooltip("If you do not have a seamless texture, enabling this may hide the seams. Overrides the Rotation setting.")]
        public bool isFaceCamera1;

        [Tooltip("For testing purposes you might want to not render or create some celestial objects")]
        public bool isHidden;

        /// <summary>
        /// The unique identifier or guidHash for this planet.
        /// </summary>
        [HideInInspector] public int celestialId;

        #endregion

        #region Public Properties
        public Transform CelestialTransform { get { return celestialTfrm; } }
        #endregion

        #region Private Variables

        [System.NonSerialized] internal Transform celestialTfrm;

        /// <summary>
        /// This is the initial direction from the celestials camera to the
        /// celestial object (planet).
        /// </summary>
        [System.NonSerialized] internal Vector3 celestialToDirection;

        /// <summary>
        /// The current relative distance the celestial object (planet) is
        /// from the celestials camera.
        /// </summary>
        internal float currentCelestialDistance;

        #endregion

        #region Constructors

        // Class constructor
        public SSCCelestial()
        {
            SetClassDefaults();
        }

        #endregion

        #region Public Non-Static Methods

        public void SetClassDefaults()
        {
            this.name = "Planet1";
            minSize = 1;
            maxSize = 4;

            minDistance = 0f;
            maxDistance = 1f;

            isRandomPosition = true;

            celestialMaterial = null;

            isFaceCamera1 = false;
            isHidden = false;

            celestialMesh = null;

            if (celestialId == 0) { celestialId = SSCMath.GetHashCodeFromGuid(); }
        }

        #endregion
    }
}



