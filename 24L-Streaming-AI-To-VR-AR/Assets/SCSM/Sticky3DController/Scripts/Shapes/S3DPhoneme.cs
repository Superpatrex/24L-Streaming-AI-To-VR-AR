using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// An abstraction of speech sound.
    /// Pronounced: phon-eme
    /// </summary>
    [System.Serializable]
    public class S3DPhoneme
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// Unique identifier
        /// </summary>
        public int guidHash;

        /// <summary>
        /// The name or title of the phoneme
        /// </summary>
        public string phonemeName;

        /// <summary>
        /// List of emotion shapes that make up the viseme for the phoneme
        /// </summary>
        public List<S3DEmotionShape> emotionShapeList;

        /// <summary>
        /// The time, in seconds, the phoneme will be active on the character.
        /// A typical short phoneme is 40-80ms.
        /// </summary>
        [Range(0.01f, 0.4f)] public float duration;

        /// <summary>
        /// The number of times the phoneme would be spoken in a sequence of 100 phonemes.
        /// </summary>
        [Range(0, 100)] public int frequency;

        /// <summary>
        /// Editor only, is this phoneme being previewed on the model?
        /// </summary>
        public bool isPreviewMode;

        /// <summary>
        /// Editor only, is this phoneme expanded in the Inspector?
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// The normalised commulative frequency based on the sum of all frequencies.
        /// See CalcFrequencyDistribution().
        /// </summary>
        [System.NonSerialized] public float frequencyNormalised;

        /// <summary>
        /// Is the phoneme currently enabled?
        /// </summary>
        [System.NonSerialized] public bool isPhonemeEnabled;

        [System.NonSerialized] public IEnumerator fadeInEnumerator;
        [System.NonSerialized] public IEnumerator fadeOutEnumerator;
        [System.NonSerialized] public IEnumerator fadeInOutEnumerator;

        /// <summary>
        /// The normalised amount the phoneme is faded in or out
        /// </summary>
        [System.NonSerialized] public float fadeProgress;

        [System.NonSerialized] public bool isFadingIn;

        [System.NonSerialized] public bool isFadingOut;


        /// <summary>
        /// Runtime caching of number of S3DEmotionShapes
        /// </summary>
        [System.NonSerialized] public int numShapes;

        #endregion

        #region Public Static Variables

        /// <summary>
        /// The minimum runtime (duration), in seconds, for a phoneme (10ms)
        /// </summary>
        public static readonly float MinDuration = 0.01f;

        #endregion

        #region Public Properties

        /// <summary>
        /// The unique identifier for the phoneme
        /// </summary>
        public int GetPhonemeId { get { return guidHash; } }

        #endregion

        #region Protected Variables

        #endregion

        #region Constructors

        public S3DPhoneme()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="phoneme"></param>
        public S3DPhoneme(S3DPhoneme phoneme)
        {
            if (phoneme == null) { SetClassDefaults(); }
            else
            {
                guidHash = phoneme.guidHash;
                phonemeName = phoneme.phonemeName;

                if (phoneme.emotionShapeList == null) { this.emotionShapeList = new List<S3DEmotionShape>(2); }
                else { this.emotionShapeList = phoneme.emotionShapeList.ConvertAll(es => new S3DEmotionShape(es)); }

                duration = phoneme.duration;
                frequency = phoneme.frequency;

                // Off by default
                isPreviewMode = false;

                showInEditor = phoneme.showInEditor;
            }
        }

        #endregion

        #region Public member Methods

        public virtual void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            emotionShapeList = new List<S3DEmotionShape>(1);
            phonemeName = "A";
            showInEditor = true;
            isPreviewMode = false;
            duration = 0.080f;
            frequency = 1;

            isPhonemeEnabled = false;
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Cummulative Distribution Function (CDF).
        /// </summary>
        /// <param name="phonemeList"></param>
        public static void CalcFrequencyDistribution (List<S3DPhoneme> phonemeList)
        {
            int numPhonemes = phonemeList == null ? 0 : phonemeList.Count;

            float totalFrequency = 0f;

            // Get the frequency for all phonemes
            for (int pIdx = 0; pIdx < numPhonemes; pIdx++)
            {
                S3DPhoneme phoneme = phonemeList[pIdx];
                if (phoneme != null)
                {
                    totalFrequency += phoneme.frequency < 0 ? 1 : phoneme.frequency;
                }
            }

            float cummulativeFrequency = 0f;

            // Set the normalised cumulative frequency distribution
            for (int pIdx = 0; pIdx < numPhonemes; pIdx++)
            {
                S3DPhoneme phoneme = phonemeList[pIdx];
                if (phoneme != null)
                {
                    float freqN = (float)phoneme.frequency / totalFrequency;
                    cummulativeFrequency += freqN;
                    phoneme.frequencyNormalised = cummulativeFrequency;
                }
            }
        }

        #endregion
    }
}