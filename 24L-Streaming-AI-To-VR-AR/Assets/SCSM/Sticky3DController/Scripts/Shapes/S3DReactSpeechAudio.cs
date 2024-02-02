using System.Collections;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A speech audio, which can play phonemes, referenced by its SpeechAudioID (hash),
    /// which can be used in a Reaction.
    /// </summary>
    [System.Serializable]
    public class S3DReactSpeechAudio
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// Unique identifier
        /// </summary>
        public int guidHash;

        /// <summary>
        /// The guidHash or unique identifier of the S3DSpeechAudio
        /// </summary>
        public int s3dSpeechAudioId;

        /// <summary>
        /// The speech audio clip name hashed.
        /// </summary>
        public int speechAudioNameHash;

        /// <summary>
        /// Is this speech audio synched with the list of S3DSpeechAudio on the StickyShapeModule Phonemes tab?
        /// </summary>
        public bool isSynced;

        //[System.NonSerialized] public IEnumerator playEnumerator;
        [System.NonSerialized] public int reactStageInt;

        #endregion

        #region Public Properties

        /// <summary>
        /// The unique identifier for the S3DReactSpeechAudio
        /// </summary>
        public int GetReactSpeechAudioId { get { return guidHash; } }

        #endregion

        #region Constructors

        public S3DReactSpeechAudio()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="s3dSpeechAudio"></param>
        public S3DReactSpeechAudio(S3DReactSpeechAudio s3dReactSpeechAudio)
        {
            if (s3dReactSpeechAudio == null) { SetClassDefaults(); }
            else
            {
                guidHash = s3dReactSpeechAudio.guidHash;
                s3dSpeechAudioId = s3dReactSpeechAudio.s3dSpeechAudioId;
                isSynced = s3dReactSpeechAudio.isSynced;
                speechAudioNameHash = s3dReactSpeechAudio.speechAudioNameHash;
            }
        }

        #endregion

        #region Public member Methods

        public void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            s3dSpeechAudioId = S3DSpeechAudio.NoID;
            speechAudioNameHash = 0;
            isSynced = false;
        }

        #endregion
    }
}