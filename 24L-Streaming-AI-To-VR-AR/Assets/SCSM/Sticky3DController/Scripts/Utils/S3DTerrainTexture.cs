using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [System.Serializable]
    public class S3DTerrainTexture
    {
        #region Public Properties
        /// <summary>
        /// The ID or hash of the albedo name of the texture
        /// </summary>
        public int ID { get { return albedoNameHash; } }

        /// <summary>
        /// The name of the albedo texture in the terrain layer
        /// </summary>
        public string AlbedoName { get { return albedoName; } set { albedoName = value; albedoNameHash = string.IsNullOrEmpty(value) ? 0 : S3DMath.GetHashCode(value); } }
        
        /// <summary>
        /// Minimum weight of terrain texture (0.01 to 1.0) at terrain position to register as a hit
        /// </summary>
        public float MinWeight { get { return minWeight; } set { minWeight = value; } }

        #endregion

        #region Private Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)
        [SerializeField] private int albedoNameHash;
        [SerializeField] private string albedoName;
        [SerializeField] [Range(0.01f,1f)] private float minWeight;
        #endregion

        #region Constructors
        public S3DTerrainTexture()
        {
            SetClassDefaults();
        }
        #endregion

        #region Private Member Methods
        private void SetClassDefaults()
        {
            albedoNameHash = 0;
            albedoName = string.Empty;
            minWeight = 0.5f;
        }

        #endregion

        #region Public Methods
        public void RefreshID()
        {
            albedoNameHash = string.IsNullOrEmpty(albedoName) ? 0 : S3DMath.GetHashCode(albedoName);
        }
        #endregion
    }
}