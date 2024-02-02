using UnityEngine;

// Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [System.Serializable]
    public class S3DPart
    {
        #region Enumerations

        #endregion

        #region Public Static

        #endregion

        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        public int guidHash;
        public bool showInEditor;

        /// <summary>
        /// The child transform on the character for this part
        /// </summary>
        public Transform partTransform;

        /// <summary>
        /// Should the part be enabled (or disabled) when the module is initialised?
        /// </summary>
        public bool enableOnStart;
        #endregion

        #region Public Properties

        /// <summary>
        /// Is the parts transform enabled?
        /// </summary>
        public bool IsPartEnabled { get { return partTransform == null ? false : partTransform.gameObject.activeSelf; } }

        #endregion

        #region Internal Variables

        #endregion

        #region Constructors
        public S3DPart()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy Constructor for S3DPart class
        /// </summary>
        /// <param name="s3dPart"></param>
        public S3DPart(S3DPart s3dPart)
        {
            if (s3dPart == null) { SetClassDefaults(); }
            else
            {
                guidHash = s3dPart.guidHash;
                showInEditor = s3dPart.showInEditor;
                partTransform = s3dPart.partTransform;
                enableOnStart = s3dPart.enableOnStart;
            }
        }

        #endregion

        #region Private Member Methods
        public void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            showInEditor = true;
            partTransform = null;
            enableOnStart = true;
        }

        #endregion

        #region Public Member Methods

        #endregion
    }
}