using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [System.Serializable]
    public class S3DSurfaceType
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)
        public int guidHash;
        public string surfaceName;
        public S3DDecals damageDecals;
        #endregion

        #region Constructors
        public S3DSurfaceType()
        {
            SetClassDefaults();
        }
        #endregion

        #region Private Member Methods
        public void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            surfaceName = string.Empty;
            damageDecals = null;
        }

        #endregion
    }
}