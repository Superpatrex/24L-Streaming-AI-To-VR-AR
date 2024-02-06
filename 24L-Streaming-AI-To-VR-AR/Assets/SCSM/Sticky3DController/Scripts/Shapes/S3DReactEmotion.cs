using System.Collections;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// An emotion, referenced by its Emotion Id (hash) for use in a Reaction.
    /// </summary>
    [System.Serializable]
    public class S3DReactEmotion
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// Unique identifier
        /// </summary>
        public int guidHash;

        /// <summary>
        /// The guidHash or unique identifier of the S3DEmotion
        /// </summary>
        public int s3dEmotionId;

        /// <summary>
        /// The emotion name hashed.
        /// </summary>
        public int emotionNameHash;

        /// <summary>
        /// Is this emotion synched with the list of S3DEmotions on the StickyShapeModule Emotion tab?
        /// </summary>
        public bool isSynced;

        [System.NonSerialized] public IEnumerator fadeOutEnumerator;
        [System.NonSerialized] public IEnumerator fadeInOutEnumerator;
        [System.NonSerialized] public int reactStageInt;

        #endregion

        #region Public Properties

        /// <summary>
        /// The unique identifier for the S3DReactEmotion
        /// </summary>
        public int GetReactEmotionId { get { return guidHash; } }

        #endregion

        #region Constructors

        public S3DReactEmotion()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="reactEmotion"></param>
        public S3DReactEmotion(S3DReactEmotion reactEmotion)
        {
            if (reactEmotion == null) { SetClassDefaults(); }
            else
            {
                guidHash = reactEmotion.guidHash;
                s3dEmotionId = reactEmotion.s3dEmotionId;
                emotionNameHash = reactEmotion.emotionNameHash;
                isSynced = reactEmotion.isSynced;
            }
        }

        #endregion

        #region Public member Methods

        public void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            s3dEmotionId = S3DEmotion.NoID;
            emotionNameHash = 0;
            isSynced = false;
        }

        #endregion
    }
}