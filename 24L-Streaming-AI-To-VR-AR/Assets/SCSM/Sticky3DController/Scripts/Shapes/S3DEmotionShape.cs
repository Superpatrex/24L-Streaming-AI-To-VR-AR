using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Emotions can consist of one or more EmotionShapes. These point to a BlendShape
    /// on a model.
    /// </summary>
    [System.Serializable]
    public class S3DEmotionShape
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// Unique identifier
        /// </summary>
        public int guidHash;

        /// <summary>
        /// The guidHash or unique identifier of the S3DBlendShape
        /// </summary>
        public int s3dBlendShapeId;

        /// <summary>
        /// The blendshape name hashed.
        /// </summary>
        public int blendShapeNameHash;

        /// <summary>
        /// The normalised weight to be applied to the blendshape
        /// </summary>
        [Range(0f, 100f)] public float maxWeight;

        /// <summary>
        /// Is this emotion shape synched with the list of S3DBlendShapes on the character?
        /// NOTE: This does not mean the S3DBlendShape is valid for the model.
        /// </summary>
        public bool isSynced;

        #endregion

        #region Public Properties

        /// <summary>
        /// The unique identifier for the EmotionShape
        /// </summary>
        public int GetEmotionShapeId { get { return guidHash; } }

        #endregion

        #region Public Static Variables

        #endregion

        #region Constructors

        public S3DEmotionShape()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="emotionShape"></param>
        public S3DEmotionShape(S3DEmotionShape emotionShape)
        {
            if (emotionShape == null) { SetClassDefaults(); }
            else
            {
                guidHash = emotionShape.guidHash;
                s3dBlendShapeId = emotionShape.s3dBlendShapeId;
                blendShapeNameHash = emotionShape.blendShapeNameHash;
                maxWeight = emotionShape.maxWeight;
                isSynced = emotionShape.isSynced;
            }
        }

        #endregion

        #region Public member Methods

        public void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            s3dBlendShapeId = S3DEmotion.NoID;
            blendShapeNameHash = 0;
            maxWeight = 100f;
            isSynced = false;
        }

        #endregion
    }
}