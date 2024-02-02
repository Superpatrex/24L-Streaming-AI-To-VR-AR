using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This class contains a replacement animation clip for an original clip in an
    /// animator controller. See also S3DAnimClipSet.
    /// </summary>
    [System.Serializable]
    public class S3DAnimClipPair
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)
        public int guidHash;
        public AnimationClip originalClip;
        public AnimationClip replacementClip;
        #endregion

        #region Constructors
        public S3DAnimClipPair()
        {
            SetClassDefaults();
        }
        #endregion

        #region Private Member Methods
        public void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            originalClip = null;
            replacementClip = null;
        }

        #endregion
    }
}