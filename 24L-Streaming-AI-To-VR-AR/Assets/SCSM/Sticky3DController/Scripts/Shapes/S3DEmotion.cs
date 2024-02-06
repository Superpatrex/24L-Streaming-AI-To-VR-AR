using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A collection of one of more blendshapes that make up an emotion.
    /// </summary>
    [System.Serializable]
    public class S3DEmotion
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// Unique identifier
        /// </summary>
        public int guidHash;

        /// <summary>
        /// The descriptive name of the emotion
        /// </summary>
        public string emotionName;

        /// <summary>
        /// List of emotion shapes that make up this emotion
        /// </summary>
        public List<S3DEmotionShape> emotionShapeList;

        /// <summary>
        /// The time, in seconds, it takes the emotion to reach its full strength.
        /// </summary>
        [Range(0f, 2f)] public float fadeInDuration;

        /// <summary>
        /// The time, in seconds, it takes the emotion to stop affecting the character.
        /// </summary>
        [Range(0f, 2f)] public float fadeOutDuration;

        /// <summary>
        /// If this emotion is triggered by an enter reaction, hold long should it
        /// be fully active before it begins to fade out.
        /// </summary>
        [Range(0f, 60f)] public float holdEnterDuration;

        /// <summary>
        /// If this emotion is triggered by an exit reaction, hold long should it
        /// be fully active before it begins to fade out.
        /// </summary>
        [Range(0f, 60f)] public float holdExitDuration;

        /// <summary>
        /// If this emotion is triggered by a stay reaction, hold long should it
        /// be fully active before it begins to fade out.
        /// </summary>
        [Range(0f, 60f)] public float holdStayDuration;

        /// <summary>
        /// Editor only, is this emotion being previewed on the model?
        /// </summary>
        public bool isPreviewMode;

        /// <summary>
        /// Is this emotion randomly used while a speech audio clip is playing?
        /// </summary>
        public bool isVoxEmotion;

        /// <summary>
        /// The emotion's anger intensity ranging from fear (-1) to anger (1).
        /// Default is neither fear nor anger (0)
        /// </summary>
        [Range(-1f, 1f)] public float angerIntensity;

        /// <summary>
        /// The emotion's joy intensity, ranging from sad (-1) to joy (1).
        /// Default is neither sad nor joy (0).
        /// </summary>
        [Range(-1f, 1f)] public float joyIntensity;

        /// <summary>
        /// The emotion's surprise intensity, ranging from anticipation (-1) to surprise (1).
        /// Default is neither anticipation nor surprise (0).
        /// </summary>
        [Range(-1f, 1f)] public float surpriseIntensity;

        /// <summary>
        /// The emotion's trust intensity, ranging from Disgust (-1) to trust (1).
        /// Default is neither disgust nor trust (0).
        /// </summary>
        [Range(-1f, 1f)] public float trustIntensity;

        /// <summary>
        /// Editor only, is this emotion expanded in the Inspector?
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// Is the emotion currently enabled?
        /// </summary>
        [System.NonSerialized] public bool isEmotionEnabled;

        [System.NonSerialized] public IEnumerator fadeInEnumerator;
        [System.NonSerialized] public IEnumerator fadeOutEnumerator;
        [System.NonSerialized] public IEnumerator fadeInOutEnumerator;

        /// <summary>
        /// The normalised amount the emotion is faded in or out
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
        /// Used to denote that an emotion, emotionshape or blendshape is not set.
        /// </summary>
        public static int NoID = 0;

        /// <summary>
        /// The minimum fade-in our fade-out time, in seconds, for an emotion (10ms)
        /// </summary>
        public static readonly float MinFadeInOutDuration = 0.01f;

        #endregion

        #region Public Properties

        /// <summary>
        /// The unique identifier for the Emotion
        /// </summary>
        public int GetEmotionId { get { return guidHash; } }

        /// <summary>
        /// Get the hashed value of the emotionName. If no name, will return 0.
        /// </summary>
        public int GetEmotionNameHash { get { return string.IsNullOrEmpty(emotionName) ? 0 : S3DMath.GetHashCode(emotionName); } }

        /// <summary>
        /// Is this emotion only friendly?
        /// </summary>
        public bool IsFriendly { get { return angerIntensity == 0f && (joyIntensity > 0f || surpriseIntensity > 0f || trustIntensity > 0f) && joyIntensity >= 0f && surpriseIntensity >= 0f && trustIntensity >= 0f; } }

        /// <summary>
        /// Is this emotion hostile? Typically used in response to a hostile character? ie. A foe, enemy and one with a different factionId.
        /// </summary>
        public bool IsHostile { get { return angerIntensity == 0f && joyIntensity == 0f && surpriseIntensity == 0f && trustIntensity == 0f; } }

        /// <summary>
        /// Is the emotion neutral? ie are all the intensities set to 0?
        /// </summary>
        public bool IsNeutral { get { return angerIntensity == 0f && joyIntensity == 0f && surpriseIntensity == 0f && trustIntensity == 0f; } }

        /// <summary>
        /// Is this emotion neutral or friendly?
        /// </summary>
        public bool IsNeutralorFriendly { get { return angerIntensity == 0f && joyIntensity >= 0f && surpriseIntensity >= 0f && trustIntensity >= 0f; } }

        #endregion

        #region Protected Variables

        #endregion

        #region Delegates

        //internal delegate void CallbackOnFadedOut(S3DEmotion emotion);

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Singlecast delegate to internally get notified when an emotion finishes fading out.
        /// </summary>
        //internal CallbackOnFadedOut callbackOnFadedOut;

        #endregion

        #region Constructors

        public S3DEmotion()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="emotion"></param>
        public S3DEmotion (S3DEmotion emotion)
        {
            if (emotion == null) { SetClassDefaults(); }
            else
            {
                guidHash = emotion.guidHash;
                emotionName = emotion.emotionName;
                fadeInDuration = emotion.fadeInDuration;
                fadeOutDuration = emotion.fadeOutDuration;
                holdEnterDuration = emotion.holdEnterDuration;
                holdExitDuration = emotion.holdExitDuration;
                holdStayDuration = emotion.holdStayDuration;
                isVoxEmotion = emotion.isVoxEmotion;

                if (emotion.emotionShapeList == null) { this.emotionShapeList = new List<S3DEmotionShape>(2); }
                else { this.emotionShapeList = emotion.emotionShapeList.ConvertAll(es => new S3DEmotionShape(es)); }

                // Off by default
                isPreviewMode = false;

                angerIntensity = emotion.angerIntensity;
                joyIntensity = emotion.joyIntensity;
                surpriseIntensity = emotion.surpriseIntensity;
                trustIntensity = emotion.trustIntensity;

                showInEditor = emotion.showInEditor;
            }
        }

        #endregion

        #region Public Member Methods

        public virtual void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            emotionShapeList = new List<S3DEmotionShape>(2);
            emotionName = "[emotion name]";
            showInEditor = true;
            isPreviewMode = false;
            fadeInDuration = 0.3f;
            fadeOutDuration = 0.1f;
            holdEnterDuration = 0f;
            holdExitDuration = 0f;
            holdStayDuration = 0f;
            isVoxEmotion = false;
            angerIntensity = 0f;
            joyIntensity = 0f;
            surpriseIntensity = 0;
            trustIntensity = 0;

            isEmotionEnabled = false;
        }

        #endregion
    }
}