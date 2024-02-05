using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A collection of one of more blendshapes that make up a blink.
    /// </summary>
    [System.Serializable]
    public class S3DBlink
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// Unique identifier
        /// </summary>
        public int guidHash;

        /// <summary>
        /// List of emotion shapes that make up this emotion
        /// </summary>
        public List<S3DEmotionShape> emotionShapeList;

        /// <summary>
        /// The time, in seconds, the blink takes to occur.
        /// The average blink duration is between 100 and 400ms.
        /// </summary>
        [Range(0.1f, 0.4f)] public float blinkDuration;

        /// <summary>
        /// Should the eyes start blinking when this component is initialised?
        /// </summary>
        public bool isEnableOnInitialise;

        /// <summary>
        /// The minimum time, in seconds between each blink.
        /// A fast blink interval would be approx 2 seconds.
        /// </summary>
        [Range(1f, 20f)] public float minBlinkInterval;

        /// <summary>
        /// The minimum time, in seconds, between each blink.
        /// A long blink interval would be 20 seconds.
        /// </summary>
        [Range(1f, 20f)] public float maxBlinkInterval;

        /// <summary>
        /// A lower-quality simplified blink action that is more performant
        /// </summary>
        public bool isSimpleBlink;

        /// <summary>
        /// Editor only, is this emotion being previewed on the model?
        /// </summary>
        public bool isPreviewMode;

        /// <summary>
        /// Editor only, is this emotion expanded in the Inspector?
        /// </summary>
        public bool showInEditor;


        [System.NonSerialized] public float blinkIntervalTimer;


        [System.NonSerialized] public float blinkDurationTimer;

        /// <summary>
        /// Runtime caching of number of S3DEmotionShapes
        /// </summary>
        [System.NonSerialized] public int numShapes;

        /// <summary>
        /// Is the eye currently opening or closing?
        /// </summary>
        [System.NonSerialized] public bool isBlinking;

        #endregion

        #region Public Properties

        /// <summary>
        /// The unique identifier for the Blink
        /// </summary>
        public int GetBlinkId { get { return guidHash; } }

        #endregion

        #region Constructors

        public S3DBlink()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="blink"></param>
        public S3DBlink(S3DBlink blink)
        {
            if (blink == null) { SetClassDefaults(); }
            else
            {
                guidHash = blink.guidHash;

                if (blink.emotionShapeList == null) { this.emotionShapeList = new List<S3DEmotionShape>(2); }
                else { this.emotionShapeList = blink.emotionShapeList.ConvertAll(es => new S3DEmotionShape(es)); }

                // Only one emotion can be in preview mode on a model.
                isPreviewMode = false;

                isEnableOnInitialise = blink.isEnableOnInitialise;
                isSimpleBlink = blink.isSimpleBlink;

                showInEditor = blink.showInEditor;
            }
        }

        #endregion

        #region Public member Methods

        public void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            emotionShapeList = new List<S3DEmotionShape>(2);
            blinkDuration = 0.125f;
            minBlinkInterval = 2f;
            maxBlinkInterval = 10f;
            showInEditor = true;
            isPreviewMode = false;
            isEnableOnInitialise = true;
            isSimpleBlink = false;
        }

        #endregion
    }
}