using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [System.Serializable]
    public class S3DBlendShape
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// Unique identifier (Id)
        /// </summary>
        public int guidHash;

        /// <summary>
        /// The skinned mesh renderer that contains the blendshape
        /// </summary>
        public SkinnedMeshRenderer skinnedMeshRenderer;

        /// <summary>
        /// The zero-based index of the blendshape on the skinned mesh renederer
        /// </summary>
        public int blendShapeIndex;

        /// <summary>
        /// The blendshape name hashed.
        /// </summary>
        public int blendShapeNameHash;

        /// <summary>
        /// Does the blendShapeNameHash match the name of the blendshape
        /// on skinned mesh renderer at the blendShapeIndex.
        /// </summary>
        public bool isNameMatched;

        /// <summary>
        /// Is this a valid blendshape that is ready for use?
        /// </summary>
        public bool isValid;

        #endregion

        #region Public Properties

        #endregion

        #region Public Static Variables

        #endregion

        #region Constructors

        public S3DBlendShape()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="blendShape"></param>
        public S3DBlendShape(S3DBlendShape blendShape)
        {
            if (blendShape == null) { SetClassDefaults(); }
            else
            {
                guidHash = blendShape.guidHash;
                skinnedMeshRenderer = blendShape.skinnedMeshRenderer;
                blendShapeIndex = blendShape.blendShapeIndex;
                blendShapeNameHash = blendShape.blendShapeNameHash;
                isNameMatched = blendShape.isNameMatched;
                isValid = blendShape.isValid;
            }
        }

        #endregion

        #region Public member Methods

        public void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            skinnedMeshRenderer = null;
            blendShapeIndex = -1;
            blendShapeNameHash = 0;
            isNameMatched = false;
            isValid = false;
        }

        #endregion

    }
}