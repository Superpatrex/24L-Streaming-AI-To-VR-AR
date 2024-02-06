using System.Collections.Generic;
using UnityEngine;

// Sticky3D Control Module Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Eye data for use with StickyShapesModule and humanoid characters
    /// </summary>
    [System.Serializable]
    public class S3DEye
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        public bool isLeftEye;

        /// <summary>
        /// Eye bone transform on the humanoid model.
        /// Required for eye movement.
        /// </summary>
        public Transform eyeTfrm;

        public bool isTfrmValid;

        [System.NonSerialized] public Quaternion lastRotLS;

        #endregion

        #region Constructors

        public S3DEye ()
        {
            SetClassDefaults();
        }

        #endregion

        #region Private Member Methods
        public void SetClassDefaults()
        {
            isLeftEye = false;
            eyeTfrm = null;
            isTfrmValid = false;
        }

        #endregion
    }
}