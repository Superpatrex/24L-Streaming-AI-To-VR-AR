using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This module lets you configure blend shapes, typically, for facial expressions. 
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Character/Sticky Shapes Module")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyShapesModule : MonoBehaviour
    {
        #region Public Variables - General

        /// <summary>
        /// If enabled, the Initialise() will be called as soon as Start() runs. This should be disabled if you are
        /// instantiating the component through code.
        /// </summary>
        public bool initialiseOnStart = false;

        #endregion

        #region Public Variables - Editor

        /// <summary>
        /// Remember which tabs etc were shown in the editor
        /// </summary>
        [HideInInspector] public int selectedTabInt = 0;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [HideInInspector] public bool allowRepaint = false;

        #endregion

        #region Public Variables - Eye Movement

        /// <summary>
        /// The local minimum angle to move the eye to the right
        /// </summary>
        [Range(0.1f, 30f)] public float eyeMoveMinX = -5f;

        /// <summary>
        /// The local maximum angle to move the eye to the left
        /// </summary>
        [Range(0.1f, 30f)] public float eyeMoveMaxX = 5f;

        /// <summary>
        /// The local minimum angle to move the eye downward
        /// </summary>
        [Range(0.1f, 30f)] public float eyeMoveMinY = -4f;

        /// <summary>
        /// The local maximum angle to move the eye upward
        /// </summary>
        [Range(0.1f, 30f)] public float eyeMoveMaxY = 2f;

        /// <summary>
        /// When Eye Movement is enabled, the minimum time, in seconds, the character is gazing in the same direction.
        /// </summary>
        [Range(0f, 10f)] public float eyeMoveMinGazeDuration = 0.1f;

        /// <summary>
        /// When Eye Movement is enabled, the maximum time, in seconds, the character is gazing in the same direction.
        /// </summary>
        [Range(0f, 10f)] public float eyeMoveMaxGazeDuration = 2f;

        /// <summary>
        /// Human eyes can turn at around 700 degrees per second! However, this is over very short distances.
        /// </summary>
        [Range(1f, 100f)] public float eyeMoveSpeed = 10f;

        #endregion

        #region Public Variables - Engage

        /// <summary>
        /// The number of seconds, after initialisation, that reactions will not be triggered
        /// by a character entering or exiting the area. This can be useful if you do not want
        /// a character within the collider area to immediately trigger reactions when the
        /// component is initialised.
        /// </summary>
        [Range(0f, 30f)] public float reactNoNotifyDuration = 0f;

        #endregion

        #region Public Properties - General

        /// <summary>
        /// Get the left eye
        /// </summary>
        public S3DEye GetLeftEye { get { return leftEye; } }

        /// <summary>
        /// Get the right eye
        /// </summary>
        public S3DEye GetRightEye { get { return rightEye; } }

        /// <summary>
        /// Get or set if the component is attached to a non-Sticky3D Controller.
        /// i.e., it is using a 3rd party character controller.
        /// </summary>
        public bool IsNonS3DController { get { return isNonS3DController; } set { SetIsNonS3DController(value); } }

        /// <summary>
        /// Is this shape module initialised and ready for use?
        /// </summary>
        public bool IsShapesModuleInitialised { get { return isInitialised; } }

        /// <summary>
        /// Get the current list of skinned mesh renderers that contain blendshapes.
        /// </summary>
        public List<SkinnedMeshRenderer> GetSkinnedMeshRendererList { get { return skinnedMRenList; } }

        #endregion

        #region Public Properties - Blendshapes

        /// <summary>
        /// Get the current list of blendshapes. See also GetBlendShapesFromModel(..).
        /// </summary>
        public List<S3DBlendShape> GetBlendShapeList { get { return blendShapeList; } }

        /// <summary>
        /// Get the last number of blendshapes deemed valid.
        /// </summary>
        public int NumValidBlendShapes { get { return numValidBlendShapes; } }

        #endregion

        #region Public Properties - Emotions

        /// <summary>
        /// Get the blink emotion
        /// </summary>
        public S3DBlink GetBlink { get { return blink; } }

        /// <summary>
        /// Get the current list of emotions.
        /// </summary>
        public List<S3DEmotion> GetEmotionList { get { return emotionList; } }

        /// <summary>
        /// Is the blinking action currently enabled?
        /// </summary>
        public bool IsBlinkingEnabled { get { return isBlinkingEnabled; } }

        /// <summary>
        /// Is eye movement currently enabled?
        /// </summary>
        public bool IsEyeMovementEnabled { get { return isEyeMoveEnabled; } }

        /// <summary>
        /// Get the number of emotions on the Emotions tab.
        /// </summary>
        public int NumberOfEmotions { get { return isInitialised ? numEmotions : emotionList == null ? 0 : emotionList.Count; } }

        #endregion

        #region Public Properties - Phonemes

        /// <summary>
        /// Get the current list of phonemes.
        /// </summary>
        public List<S3DPhoneme> GetPhonemeList { get { return phonemeList; } }

        /// <summary>
        /// Get the current list of speech audio clips
        /// </summary>
        public List<S3DSpeechAudio> GetSpeechAudioList { get { return speechAudioList; } }

        #endregion

        #region Public Properties - Engage

        /// <summary>
        /// Get the current list of reactions.
        /// </summary>
        public List<S3DReact> GetReactList { get { return reactList; } }

        /// <summary>
        /// Is the character ready to react to others with emotions or speech?
        /// </summary>
        public bool IsReactingEnabled { get { return isReactingEnabled; } }

        /// <summary>
        /// Is the character ready to react?
        /// </summary>
        public bool IsReactInitialised { get { return isReactInitialised; } }

        /// <summary>
        /// Get or set the proximity collider. It must be on a child gameobject within the prefab.
        /// </summary>
        public Collider ProximityCollider { get { return proximityCollider; } set { SetProximityCollider(value); }  }

        /// <summary>
        /// Get or set the distance the character reacts to others nearby.
        /// Assumes the collider is a sphere or box collider.
        /// </summary>
        public float ReactDistance { get { return reactDistance; } set { SetReactDistance(value); } }

        #endregion

        #region Public Static Variables

        #endregion

        #region Private Variables

        [NonSerialized] private List<SkinnedMeshRenderer> tempSMRenList = new List<SkinnedMeshRenderer>(1);

        #endregion

        #region Protected Variables - Editor Only

        [SerializeField] protected bool isSkinnedMRenListExpanded = true;
        [SerializeField] protected bool isBlendShapeListExpanded = true;
        [SerializeField] protected bool isEyeMovementExpanded = true;
        [SerializeField] protected bool isEmotionListExpanded = true;
        [SerializeField] protected bool isAudioSettingsExpanded = true;
        [SerializeField] protected bool isPhonemeListExpanded = true;
        [SerializeField] protected bool isSpeechAudioListExpanded = true;
        [SerializeField] protected bool isReactSettingsExpanded = true;
        [SerializeField] protected bool isReactListExpanded = true;


        #endregion

        #region Protected Variables - General

        /// <summary>
        /// The character is NOT using a Sticky3DController.
        /// i.e., it is using a 3rd party character controller.
        /// </summary>
        [SerializeField] protected bool isNonS3DController = false;

        protected bool isInitialised = false;

        /// <summary>
        /// Is the module currently paused?
        /// </summary>
        protected bool isShapesPaused = false;

        /// <summary>
        /// The animator (if any) on a non-S3D character
        /// </summary>
        [NonSerialized] Animator nonS3DAnimator = null;

        protected S3DRandom s3dRandom = null;

        protected StickyControlModule stickyControlModule = null;

        /// <summary>
        /// Left eye on the humanoid model. Required for eye movement.
        /// </summary>
        [SerializeField] protected S3DEye leftEye = null;

        /// <summary>
        /// Right eye on the humanoid model. Required for eye movement.
        /// </summary>
        [SerializeField] protected S3DEye rightEye = null;

        /// <summary>
        /// The list of skinned mesh renderers that contain blendshapes on the model
        /// </summary>
        [SerializeField] protected List<SkinnedMeshRenderer> skinnedMRenList = new List<SkinnedMeshRenderer>(2);

        #endregion

        #region Protected Variables - Blend Shapes

        /// <summary>
        /// The list of blendshapes on the model
        /// </summary>
        [SerializeField] protected List<S3DBlendShape> blendShapeList = new List<S3DBlendShape>(10);

        /// <summary>
        /// The last number of blendshapes deemed valid. See ValidateBlendShapes(..).
        /// </summary>
        protected int numValidBlendShapes = 0;

        #endregion

        #region Protected Variables - Blink

        protected bool isBlinkingEnabled;

        #endregion

        #region Protected Variables - Eye Movement

        protected bool isEyeMoveEnabled;

        protected bool isEyesMovingToTarget = false;

        protected float eyeMoveGazeIntervalTimer = 0f;

        protected Vector2 eyeMoveTargetAngle = Vector2.zero;

        /// <summary>
        /// Should the eyes start moving when this component is initialised?
        /// </summary>
        [SerializeField] protected bool isEyeMoveEnableOnInit;

        #endregion

        #region Protected Variables - Emotions

        /// <summary>
        /// The blink configuration for this character
        /// </summary>
        [SerializeField] protected S3DBlink blink = new S3DBlink();

        /// <summary>
        /// The list of emotions to be used for this character
        /// </summary>
        [SerializeField] protected List<S3DEmotion> emotionList = new List<S3DEmotion>(2);

        [NonSerialized] protected int numEmotions = 0;

        [NonSerialized] protected bool isSpeechEmotionPlaying = false;

        /// <summary>
        /// The emotion that is currently enabled while the speech audio clip is playing.
        /// NOTE: This can be null during silent patches of the audio clip or if no
        /// emotions have isVOXEmotion enabled.
        /// </summary>
        [NonSerialized] protected S3DEmotion speechEmotionPlaying = null;

        /// <summary>
        /// The list of S3DEmotion IDs (1-based indexes) randomly selected when a speech auto clip is playing.
        /// </summary>
        [NonSerialized] protected List<int> voxEmotionIdList= new List<int>();

        /// <summary>
        /// A temporary reuseable list which can be used to store emotions.
        /// </summary>
        [NonSerialized] protected List<S3DEmotion> tempEmotionList = new List<S3DEmotion>();

        #endregion

        #region Protected Variables - Phonemes

        /// <summary>
        /// The audio source used to play speech autoclips
        /// </summary>
        [SerializeField] protected AudioSource phonemeAudioSource = null;

        /// <summary>
        /// The list of phonemes to be used for this character
        /// </summary>
        [SerializeField] protected List<S3DPhoneme> phonemeList = new List<S3DPhoneme>(10);

        /// <summary>
        /// The list of speech audio clips used for this character
        /// </summary>
        [SerializeField] protected List<S3DSpeechAudio> speechAudioList = new List<S3DSpeechAudio>(2);

        /// <summary>
        /// Is a speech audio clip currently playing?
        /// </summary>
        [NonSerialized] protected bool isSpeechPlaying = false;

        [NonSerialized] protected bool isSpeechPhonemePlaying = false;

        [NonSerialized] protected int numPhonemes = 0;

        /// <summary>
        /// The phoneme that is currently enabled while the speech audio clip is playing.
        /// NOTE: This can be null during silent patches of the audio clip.
        /// </summary>
        [NonSerialized] protected S3DPhoneme speechPhonemePlaying = null;

        /// <summary>
        /// The speech audio currently playing
        /// </summary>
        [NonSerialized] protected S3DSpeechAudio currentSpeechAudio = null;

        #endregion

        #region Protected Variables - Engage (React)

        /// <summary>
        /// Is the character ready to react to others with emotions or speech?
        /// </summary>
        protected bool isReactingEnabled = false;

        /// <summary>
        /// Is the character ready to react?
        /// </summary>
        protected bool isReactInitialised = false;

        [NonSerialized] protected int numReactions = 0;

        /// <summary>
        /// The distance at which the character starts to react to others in the scene
        /// </summary>
        [SerializeField, Range(1f, 100f)] protected float reactDistance = 3f;

        /// <summary>
        /// Should the character get ready to react when this component is initialised?
        /// </summary>
        [SerializeField] protected bool isReactEnableOnInit = false;

        /// <summary>
        /// The trigger collider used to detect nearby characters. It must be on a child gameobject within the prefab.
        /// </summary>
        [SerializeField] protected Collider proximityCollider = null;

        /// <summary>
        /// The list of reactions to be used for this character
        /// </summary>
        [SerializeField] protected List<S3DReact> reactList = new List<S3DReact>(2);

        [NonSerialized] protected int numReacts = 0;

        /// <summary>
        /// A unique array of characters that have entered the proximity area.
        /// </summary>
        [NonSerialized] protected HashSet<StickyControlModule> nearbyCharacters = new HashSet<StickyControlModule>();

        #endregion

        #region Public Delegates

        #endregion

        #region Private Initialise Methods

        // Use this for initialization
        private void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Update Methods

        protected virtual void LateUpdate()
        {
            if (isInitialised && !isShapesPaused)
            {
                float dTime = Time.deltaTime;

                if (isBlinkingEnabled)
                {
                    BlinkCycle(dTime);
                }

                if (isEyeMoveEnabled)
                {
                    MoveEyes(dTime);
                }

                if (isSpeechPlaying)
                {
                    UpdateSpeech(dTime);
                }
            }
        }

        #endregion

        #region Protected and Internal Methods - General

        /// <summary>
        /// Pause or Unpause all activity on the module
        /// </summary>
        /// <param name="isPaused"></param>
        protected virtual void PauseOrUnPause(bool isPaused)
        {
            // Only action if changing state
            if (isShapesPaused == isPaused) { return; }

            if (isPaused)
            {
                isShapesPaused = true;
            }
            else
            {
                isShapesPaused = false;
            }
        }

        #endregion

        #region Protected and Internal Methods - Blink

        /// <summary>
        /// Conmence a blink cycle. This starts with a randomly selected
        /// interval at which the eyes are typically open.
        /// </summary>
        protected virtual void BeginBlinkCycle()
        {
            blink.isBlinking = false;
            blink.blinkDurationTimer = 0f;
            blink.blinkIntervalTimer = s3dRandom.Range(blink.minBlinkInterval, blink.maxBlinkInterval);
            SetBlinkValue(0f);
        }

        /// <summary>
        /// Begin to blink the eyes after the interval between blinks.
        /// </summary>
        /// <param name="isSimpleBlink"></param>
        protected virtual void BeginBlinkEyes(bool isSimpleBlink)
        {
            blink.blinkIntervalTimer = 0f;
            blink.isBlinking = true;
            blink.blinkDurationTimer = blink.blinkDuration;

            SetBlinkValue(isSimpleBlink ? 1f : 0f);
        }

        /// <summary>
        /// Overrideable blink cycle
        /// </summary>
        /// <param name="dTime"></param>
        protected virtual void BlinkCycle (float dTime)
        {
            if (blink.isBlinking)
            {
                blink.blinkDurationTimer -= dTime;

                if (blink.blinkDurationTimer <=  0f)
                {
                    BeginBlinkCycle();
                }
            }
            else
            {
                blink.blinkIntervalTimer -= dTime;

                if (blink.blinkIntervalTimer <= 0f)
                {
                    BeginBlinkEyes(blink.isSimpleBlink);
                }
                else if (!blink.isSimpleBlink)
                {
                    float hDuration = blink.blinkDuration / 2f;

                    if (blink.blinkIntervalTimer < hDuration)
                    {
                        // Closing eyes
                        SetBlinkValue(Mathf.Lerp(0, 1f, blink.blinkIntervalTimer / hDuration));
                    }
                    else
                    {
                        // Opening eyes
                        SetBlinkValue(Mathf.Lerp(1f, 0f, (blink.blinkIntervalTimer - hDuration) / hDuration));
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to start or stop blinking
        /// </summary>
        /// <param name="isEnabled"></param>
        protected void EnableOrDisableBlinking(bool isEnabled)
        {
            // Only change state if it is required
            if (isEnabled == isBlinkingEnabled) { return; }

            blink.numShapes = blink.emotionShapeList.Count;

            if (isEnabled)
            {               
                isBlinkingEnabled = true;

                BeginBlinkCycle();
            }
            else
            {
                blink.isBlinking = false;
                isBlinkingEnabled = false;
            }
        }

        #endregion

        #region Protected and Internal Methods - Eye Movement

        /// <summary>
        /// Attempt to start or stop eye movement
        /// </summary>
        /// <param name="isEnabled"></param>
        protected void EnableOrDisableEyeMove(bool isEnabled)
        {
            // Only change state if it is required
            if (isEnabled == isEyeMoveEnabled) { return; }

            if (isEnabled)
            {
                if (!leftEye.isTfrmValid)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: StickyShapesModule - cannot enable eye movement on " + name + " as the left eye transform is missing or invalid. See the General tab.");
                    #endif
                }
                else if (!rightEye.isTfrmValid)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: StickyShapesModule - cannot enable eye movement on " + name + " as the right eye transform is missing or invalid. See the General tab.");
                    #endif
                }
                else
                {
                    isEyeMoveEnabled = true;
                    BeginMoveEyesCycle();
                }
            }
            else
            {
                leftEye.eyeTfrm.rotation = Quaternion.identity;

                isEyeMoveEnabled = false;
            }
        }

        /// <summary>
        /// Start a new eye movement cycle
        /// </summary>
        protected void BeginMoveEyesCycle()
        {
            // Get new target angle
            eyeMoveTargetAngle.x = s3dRandom.Range(eyeMoveMinX, eyeMoveMaxX);
            eyeMoveTargetAngle.y = s3dRandom.Range(eyeMoveMinY, eyeMoveMaxY);
            isEyesMovingToTarget = true;
            eyeMoveGazeIntervalTimer = 0f;
        }

        /// <summary>
        /// Overrideable method to randomly move eyes
        /// </summary>
        /// <param name="dTime"></param>
        protected virtual void MoveEyes(float dTime)
        {
            if (isEyesMovingToTarget)
            {
                Vector3 eyeRotOffset = Vector3.zero;

                // If eyes have z-axis forward on character, rotate around y-axis for left-right eye movement.
                // If eyes have z-axis forward on character, rotate around x-axis for up-down eye movement.
                Quaternion targetRotLS = Quaternion.Euler(-eyeMoveTargetAngle.y, eyeMoveTargetAngle.x, 0f);

                /// TODO - slerp or add some damping

                // Use the last rotation rather than the current rotation
                Quaternion newRotLS = Quaternion.RotateTowards(leftEye.lastRotLS, targetRotLS, eyeMoveSpeed * dTime);

                // Remember the last rotation
                leftEye.lastRotLS = newRotLS;
                rightEye.lastRotLS = newRotLS;

                // Are we close to the target?
                if (Quaternion.Dot(newRotLS, targetRotLS) > 0.9999f)
                {
                    isEyesMovingToTarget = false;
                    eyeMoveGazeIntervalTimer = s3dRandom.Range(eyeMoveMinGazeDuration, eyeMoveMaxGazeDuration);
                }
            }
            else
            {
                eyeMoveGazeIntervalTimer -= dTime;

                if (eyeMoveGazeIntervalTimer <= 0f)
                {
                    BeginMoveEyesCycle();
                }
            }

            // Update the rotation each frame to overwrite animations
            leftEye.eyeTfrm.localRotation = leftEye.lastRotLS;
            rightEye.eyeTfrm.localRotation = rightEye.lastRotLS;
        }

        #endregion

        #region Protected and Internal Methods - Emotions

        /// <summary>
        /// Attempt to enable or disable an emotion and all it's emotionShapes.
        /// </summary>
        /// <param name="emotion"></param>
        /// <param name="isEnabled"></param>
        /// <param name="isInstant"></param>
        protected virtual void EnableOrDisableEmotion (S3DEmotion emotion, bool isEnabled, bool isInstant)
        {
            // Only change state if it is required
            if (emotion.isEmotionEnabled == isEnabled && !(isEnabled && emotion.isFadingOut && emotion.isEmotionEnabled)) { return; }

            // Update the cached number of shapes so it can be used in SetEmotionValue(..)
            emotion.numShapes = emotion.emotionShapeList.Count;

            StopEmotionFadeRoutines(emotion);

            if (isEnabled)
            {
                emotion.isEmotionEnabled = true;

                // Here is where we'd start to fade-in...
                if (isInstant) { SetEmotionValue(emotion, 1f); }
                else
                {
                    emotion.fadeInEnumerator = FadeInEmotion(emotion);
                    StartCoroutine(emotion.fadeInEnumerator);
                }

                #if UNITY_EDITOR
                emotion.isPreviewMode = true;
                #endif
            }
            else
            {
                // Here is where we'd start to fade-out
                if (isInstant)
                {
                    emotion.isEmotionEnabled = false;
                    SetEmotionValue(emotion, 0f);
                }
                else
                {
                    emotion.fadeOutEnumerator = FadeOutEmotion(emotion);
                    StartCoroutine(emotion.fadeOutEnumerator);
                }

                #if UNITY_EDITOR
                emotion.isPreviewMode = false;
                #endif
            }
        }

        /// <summary>
        /// Attempt to fade in an emotion
        /// </summary>
        /// <param name="emotion"></param>
        /// <returns></returns>
        protected IEnumerator FadeInEmotion(S3DEmotion emotion)
        {
            emotion.isFadingIn = true;
            if (emotion.fadeInDuration == 0) { emotion.fadeInDuration = 0.01f; }

            float startTime = Time.time;
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.02f);

            float timer = emotion.fadeInDuration;

            // Reverse the fade
            if (emotion.isFadingOut)
            {
                timer = emotion.fadeInDuration * (1f - emotion.fadeProgress);
            }
            else
            {
                emotion.fadeProgress = 0f;
            }

            emotion.isFadingOut = false;

            while (timer >= 0f)
            {
                timer -= Time.deltaTime;
                emotion.fadeProgress = (emotion.fadeInDuration - timer) / emotion.fadeInDuration;
                SetEmotionValue(emotion, emotion.fadeProgress);
                yield return waitForSeconds;
            }

            emotion.isFadingIn = false;

            yield return null;
        }

        /// <summary>
        /// Attempt to fade out an emotion
        /// </summary>
        /// <param name="emotion"></param>
        /// <returns></returns>
        protected IEnumerator FadeOutEmotion(S3DEmotion emotion)
        {
            emotion.isFadingOut = true;
            if (emotion.fadeOutDuration == 0) { emotion.fadeOutDuration = 0.01f; }

            float startTime = Time.time;
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.02f);

            float timer = emotion.fadeOutDuration;

            // Reverse the fade
            if (emotion.isFadingIn)
            {
                timer = emotion.fadeOutDuration * emotion.fadeProgress;
            }
            else
            {
                emotion.fadeProgress = 0f;
            }

            emotion.isFadingIn = false;

            while (timer >= 0f)
            {
                timer -= Time.deltaTime;
                emotion.fadeProgress = timer / emotion.fadeOutDuration;
                SetEmotionValue(emotion, emotion.fadeProgress);
                yield return waitForSeconds;
            }

            emotion.isFadingOut = false;
            emotion.isEmotionEnabled = false;
            emotion.isPreviewMode = false;

            if (isSpeechEmotionPlaying)
            {
                speechEmotionPlaying = null;
                isSpeechEmotionPlaying = false;
            }

            yield return null;
        }

        /// <summary>
        /// Attempt to fade and emotion in, then out.
        /// Typically used when playing a speech audio.
        /// </summary>
        /// <param name="emotion"></param>
        /// <returns></returns>
        protected IEnumerator FadeInOutEmotion(S3DEmotion emotion, float playSpeed)
        {
            // Avoid div0
            if (emotion.fadeInDuration < S3DEmotion.MinFadeInOutDuration) { emotion.fadeInDuration = S3DEmotion.MinFadeInOutDuration; }
            if (emotion.fadeOutDuration < S3DEmotion.MinFadeInOutDuration) { emotion.fadeOutDuration = S3DEmotion.MinFadeInOutDuration; }

            // playSpeed should never be 0.
            float timer = (emotion.fadeInDuration + emotion.fadeOutDuration) / playSpeed;

            float startTime = Time.time;
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.02f);

            // Reverse the fade
            if (emotion.isFadingOut)
            {
                // I think this should be the same as if starting from FadeIn
                timer = emotion.fadeInDuration + emotion.fadeOutDuration - (emotion.fadeInDuration * emotion.fadeProgress);
            }
            else if (emotion.isFadingIn)
            {
                timer = emotion.fadeInDuration + emotion.fadeOutDuration - (emotion.fadeInDuration * emotion.fadeProgress);
            }
            else           
            {
                emotion.fadeProgress = 0f;
            }

            emotion.isFadingIn = true;
            emotion.isFadingOut = false;

            while (timer >= 0f)
            {
                timer -= Time.deltaTime;

                // Switch to fade out now?
                if (timer <= emotion.fadeOutDuration)
                {
                    emotion.isFadingIn = false;
                    emotion.isFadingOut = true;
                    emotion.fadeProgress = 0f;
                }

                if (emotion.isFadingIn)
                {
                    emotion.fadeProgress = 1f - ((timer - emotion.fadeOutDuration) / emotion.fadeInDuration);
                }
                else
                {
                    emotion.fadeProgress = timer / emotion.fadeOutDuration;
                }

                SetEmotionValue(emotion, emotion.fadeProgress);

                yield return waitForSeconds;
            }

            emotion.isFadingIn = false;
            emotion.isFadingOut = false;
            emotion.isEmotionEnabled = false;
            emotion.isPreviewMode = false;
            isSpeechEmotionPlaying = false;
            speechEmotionPlaying = null;

            yield return null;
        }

        /// <summary>
        /// Attempt to fade in, then out a emotion when isPlay is true. Otherwise, attempt to fade
        /// out the emotion.
        /// </summary>
        /// <param name="emotion"></param>
        /// <param name="isPlay"></param>
        /// <param name="playSpeed"></param>
        protected virtual void PlayOrStopEmotion(S3DEmotion emotion, bool isPlay, float playSpeed)
        {
            StopEmotionFadeRoutines(emotion);

            // Update the cached number of shapes so it can be used in SetEmotionValue(..)
            emotion.numShapes = emotion.emotionShapeList.Count;

            if (isPlay)
            {
                emotion.isEmotionEnabled = true;
                isSpeechEmotionPlaying = true;

                emotion.fadeInOutEnumerator = FadeInOutEmotion(emotion, playSpeed);
                StartCoroutine(emotion.fadeInOutEnumerator);

                #if UNITY_EDITOR
                emotion.isPreviewMode = isPlay;
                #endif
            }
            else
            {
                StopEmotion(emotion);
            }
        }

        /// <summary>
        /// Attempt to find and play the next randomly selected emotion.
        /// Used with speechAudio clips. See UpdateSpeech(..).
        /// </summary>
        protected virtual void PlayNextEmotion(float speechSpeed)
        {
            int emotionNumber = 0;

            int numVOXEmotions = voxEmotionIdList.Count;

            if (numVOXEmotions == 1)
            {
                emotionNumber = voxEmotionIdList[0];
            }
            else if (numVOXEmotions > 0)
            {
                // TODO - ignore invalid emotions ??

                emotionNumber = voxEmotionIdList[s3dRandom.Range(0, numVOXEmotions-1)];
            }

            if (emotionNumber > 0)
            {
                speechEmotionPlaying = GetEmotionByNumber(emotionNumber);

                if (speechEmotionPlaying != null)
                {
                    isSpeechEmotionPlaying = true;

                    PlayOrStopEmotion(speechEmotionPlaying, true, speechSpeed);
                }
            }
        }

        /// <summary>
        /// Attempt to set the weighted values for all the blendshapes defined in the emotion.
        /// </summary>
        /// <param name="emotion"></param>
        /// <param name="emotionValue"></param>
        protected void SetEmotionValue (S3DEmotion emotion, float emotionValue)
        {
            if (isInitialised)
            {
                // Clamp input value 0.0 to 1.0
                if (emotionValue < 0f) { emotionValue = 0f; }
                else if (emotionValue > 1f) { emotionValue = 1f; }

                // Loop through all the S3DEmotionShapes for this emotion,
                // and assign the value proportionally to the MaxWeight of that shape.
                for (int bsIdx = 0; bsIdx < emotion.numShapes; bsIdx++)
                {
                    S3DEmotionShape emotionShape = emotion.emotionShapeList[bsIdx];

                    // Does the emotionShape have a corresponding S3DBlendShape?
                    if (emotionShape.isSynced)
                    {
                        S3DBlendShape blendShape = GetBlendShape(emotionShape.s3dBlendShapeId);

                        if (blendShape.isValid)
                        {
                            blendShape.skinnedMeshRenderer.SetBlendShapeWeight(blendShape.blendShapeIndex, emotionValue * emotionShape.maxWeight);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If an emotion is already fading in or out or playing,
        /// immediately stop that.
        /// NOTE: This does not update the isFadingIn or Out variables.
        /// </summary>
        /// <param name="emotion"></param>
        protected void StopEmotionFadeRoutines(S3DEmotion emotion)
        {
            // If already fading in, stop doing that
            if (emotion.fadeInEnumerator != null)
            {
                StopCoroutine(emotion.fadeInEnumerator);
            }

            // If already fading out, stop doing that
            if (emotion.fadeOutEnumerator != null)
            {
                StopCoroutine(emotion.fadeOutEnumerator);
            }

            // If already playing (fading in and out), stop doing that
            if (emotion.fadeInOutEnumerator != null)
            {
                StopCoroutine(emotion.fadeInOutEnumerator);
            }
        }

        #endregion

        #region Protected and Internal Methods - Phonemes

        /// <summary>
        /// Attempt to enable or disable a phoneme and all it's emotionShapes.
        /// </summary>
        /// <param name="phoneme"></param>
        /// <param name="isEnabled"></param>
        /// <param name="isInstant"></param>
        protected virtual void EnableOrDisablePhoneme (S3DPhoneme phoneme, bool isEnabled, bool isInstant)
        {
            // Only change state if it is required
            if (phoneme.isPhonemeEnabled == isEnabled && !(isEnabled && phoneme.isFadingOut && phoneme.isPhonemeEnabled)) { return; }

            phoneme.numShapes = phoneme.emotionShapeList.Count;

            StopPhonemeFadeRoutines(phoneme);

            if (isEnabled)
            {
                phoneme.isPhonemeEnabled = true;

                // Here is where we'd start to fade-in...
                if (isInstant) { SetPhonemeValue(phoneme, 1f); }
                else
                {
                    phoneme.fadeInEnumerator = FadeInPhoneme(phoneme);
                    StartCoroutine(phoneme.fadeInEnumerator);
                }

                #if UNITY_EDITOR
                phoneme.isPreviewMode = true;
                #endif
            }
            else
            {
                // Here is where we'd start to fade-out
                if (isInstant)
                {
                    phoneme.isPhonemeEnabled = false;
                    SetPhonemeValue(phoneme, 0f);
                }
                else
                {
                    phoneme.fadeOutEnumerator = FadeOutPhoneme(phoneme);
                    StartCoroutine(phoneme.fadeOutEnumerator);
                }

                #if UNITY_EDITOR
                phoneme.isPreviewMode = false;
                #endif
            }
        }

        /// <summary>
        /// Attempt to fade in a phoneme
        /// </summary>
        /// <param name="phoneme"></param>
        /// <returns></returns>
        protected IEnumerator FadeInPhoneme(S3DPhoneme phoneme)
        {
            phoneme.isFadingIn = true;
            if (phoneme.duration == 0) { phoneme.duration = S3DPhoneme.MinDuration; }

            float startTime = Time.time;
            float fadeInDuration = phoneme.duration / 2f;
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.02f);

            float timer = fadeInDuration;

            // Reverse the fade
            if (phoneme.isFadingOut)
            {
                timer = fadeInDuration * (1f - phoneme.fadeProgress);
            }
            else
            {
                phoneme.fadeProgress = 0f;
            }

            phoneme.isFadingOut = false;

            while (timer >= 0f)
            {
                timer -= Time.deltaTime;
                phoneme.fadeProgress = (fadeInDuration - timer) / fadeInDuration;
                SetPhonemeValue(phoneme, phoneme.fadeProgress);
                yield return waitForSeconds;
            }

            phoneme.isFadingIn = false;

            yield return null;
        }

        /// <summary>
        /// Attempt to fade out a phoneme
        /// </summary>
        /// <param name="phoneme"></param>
        /// <returns></returns>
        protected IEnumerator FadeOutPhoneme(S3DPhoneme phoneme)
        {
            phoneme.isFadingOut = true;
            if (phoneme.duration == 0) { phoneme.duration = S3DPhoneme.MinDuration; }

            float startTime = Time.time;
            float fadeOutDuration = phoneme.duration / 2f;
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.02f);

            float timer = fadeOutDuration;

            // Reverse the fade
            if (phoneme.isFadingIn)
            {
                timer = fadeOutDuration * phoneme.fadeProgress;
            }
            else
            {
                phoneme.fadeProgress = 0f;
            }

            phoneme.isFadingIn = false;

            while (timer >= 0f)
            {
                timer -= Time.deltaTime;
                phoneme.fadeProgress = timer / fadeOutDuration;
                SetPhonemeValue(phoneme, phoneme.fadeProgress);
                yield return waitForSeconds;
            }

            phoneme.isFadingOut = false;
            phoneme.isPhonemeEnabled = false;
            phoneme.isPreviewMode = false;

            if (isSpeechPhonemePlaying)
            {
                speechPhonemePlaying = null;
                isSpeechPhonemePlaying = false;
            }

            yield return null;
        }

        /// <summary>
        /// Attempt to fade in, then out a phoneme.
        /// Typically used when playing a speech audio.
        /// </summary>
        /// <param name="phoneme"></param>
        /// <returns></returns>
        protected IEnumerator FadeInOutPhoneme(S3DPhoneme phoneme, float playSpeed)
        {
            // playSpeed should never be 0.
            float timer = (phoneme.duration > S3DPhoneme.MinDuration ? phoneme.duration : S3DPhoneme.MinDuration) / playSpeed;

            float startTime = Time.time;
            float fadeInDuration = timer / 2f;
            float fadeOutDuration = timer - fadeInDuration;
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.02f);

            // Reverse the fade
            if (phoneme.isFadingOut)
            {
                //timer = fadeInDuration * (1f - phoneme.fadeProgress) + fadeOutDuration;
                // I think this should be the same as if starting from FadeIn
                timer = fadeInDuration + fadeOutDuration - (fadeInDuration * phoneme.fadeProgress);
            }
            else if (phoneme.isFadingIn)
            {
                timer = fadeInDuration + fadeOutDuration - (fadeInDuration * phoneme.fadeProgress);
            }
            else           
            {
                phoneme.fadeProgress = 0f;
            }

            phoneme.isFadingIn = true;
            phoneme.isFadingOut = false;

            while (timer >= 0f)
            {
                timer -= Time.deltaTime;

                // Switch to fade out now?
                if (timer <= fadeOutDuration)
                {
                    phoneme.isFadingIn = false;
                    phoneme.isFadingOut = true;
                    phoneme.fadeProgress = 0f;
                }

                if (phoneme.isFadingIn)
                {
                    phoneme.fadeProgress = 1f - ((timer - fadeOutDuration) / fadeInDuration);
                }
                else
                {
                    phoneme.fadeProgress = timer / fadeOutDuration;
                }

                SetPhonemeValue(phoneme, phoneme.fadeProgress);
                yield return waitForSeconds;
            }

            phoneme.isFadingIn = false;
            phoneme.isFadingOut = false;
            phoneme.isPhonemeEnabled = false;
            phoneme.isPreviewMode = false;
            isSpeechPhonemePlaying = false;
            speechPhonemePlaying = null;

            yield return null;
        }

        /// <summary>
        /// Attempt to find and play the next randomly selected phoneme.
        /// Used with speechAudio clips. See UpdateSpeech(..).
        /// </summary>
        protected virtual void PlayNextPhoneme(float speechSpeed)
        {
            int phonemeNumber = 0;

            if (numPhonemes == 1)
            {
                phonemeNumber = 1;
            }
            else if (numPhonemes > 0)
            {
                // Use cumulative distribution
                float gcfValue = s3dRandom.Range0to1();

                phonemeNumber = 1;

                // Find the first cummulative distribution value (0.0 to 1.0)
                // that is equal to or greater than the cummulative value.
                for (int phIdx = 0; phIdx < numPhonemes; phIdx++)
                {
                    if (phonemeList[phIdx].frequencyNormalised >= gcfValue)
                    {
                        phonemeNumber = phIdx + 1;
                        break;
                    }
                }
            }

            if (phonemeNumber > 0)
            {
                speechPhonemePlaying = GetPhonemeByNumber(phonemeNumber);

                if (speechPhonemePlaying != null)
                {
                    isSpeechPhonemePlaying = true;
                    PlayOrStopPhoneme(speechPhonemePlaying, true, speechSpeed);
                }
            }
        }

        /// <summary>
        /// Attempt to fade in, then out a phoneme when isPlay is true. Otherwise, attempt to fade
        /// out the phoneme.
        /// </summary>
        /// <param name="phoneme"></param>
        /// <param name="isPlay"></param>
        /// <param name="playSpeed"></param>
        protected virtual void PlayOrStopPhoneme(S3DPhoneme phoneme, bool isPlay, float playSpeed)
        {
            StopPhonemeFadeRoutines(phoneme);

            phoneme.numShapes = phoneme.emotionShapeList.Count;

            if (isPlay)
            {
                phoneme.isPhonemeEnabled = true;
                isSpeechPhonemePlaying = true;

                phoneme.fadeInOutEnumerator = FadeInOutPhoneme(phoneme, playSpeed);
                StartCoroutine(phoneme.fadeInOutEnumerator);

                #if UNITY_EDITOR
                phoneme.isPreviewMode = isPlay;
                #endif
            }
            else
            {
                StopPhoneme(phoneme);
            }
        }

        /// <summary>
        /// WIP - TODO - implement non-instant stop for reactions.
        /// Attempt to play or stop playing a speech audio along with the
        /// phonemes and/or emotions.
        /// Assmumes is initialised and speechAudio is not null.
        /// </summary>
        /// <param name="speechAudio"></param>
        /// <param name="isPlay"></param>
        /// <param name="isInstant">CURRENTLY ALWAYS INSTANT STOP</param>
        protected virtual void PlayOrStopSpeechAudio (S3DSpeechAudio speechAudio, bool isPlay, bool isInstant)
        {
            if (isInitialised)
            {
                if (speechAudio != null && speechAudio.IsValid() && phonemeAudioSource != null)
                {
                    phonemeAudioSource.clip = speechAudio.audioClip;
                    phonemeAudioSource.volume = speechAudio.volume;

                    // Ensure the audiosource is ready
                    if (phonemeAudioSource.isActiveAndEnabled)
                    {
                        if (isPlay)
                        {
                            if (!phonemeAudioSource.isPlaying && speechAudio.GetAudioData())
                            {
                                S3DPhoneme.CalcFrequencyDistribution(phonemeList);

                                phonemeAudioSource.Play();
                                currentSpeechAudio = speechAudio;
                                currentSpeechAudio.isPreviewMode = true;
                                currentSpeechAudio.intervalTimer = 0f;
                                isSpeechPlaying = true;                                
                            }
                            else
                            {
                                // Cannot play this clip
                                speechAudio.playedByReactId = 0;
                            }
                        }
                        else
                        {
                            // If the audio clip has stopped playing because it
                            // came to the end, or was stopped in some other dev code,
                            // it might not be still playing.
                            if (phonemeAudioSource.isPlaying)
                            {
                                // Stop audio from playing
                                phonemeAudioSource.Stop();
                            }

                            // Stop phoneme from playing
                            if (isSpeechPhonemePlaying)
                            {
                                StopPhonemeInstantly(speechPhonemePlaying);
                                speechPhonemePlaying = null;
                                isSpeechPhonemePlaying = false;
                            }

                            // Stop emotion from playing
                            if (isSpeechEmotionPlaying)
                            {
                                StopEmotionInstantly(speechEmotionPlaying);
                                speechEmotionPlaying = null;
                                isSpeechEmotionPlaying = false;
                            }

                            currentSpeechAudio.isPreviewMode = false;
                            currentSpeechAudio.intervalTimer = 0f;
                            currentSpeechAudio = null;
                            isSpeechPlaying = false;

                            if (speechAudio.playedByReactId != 0)
                            {
                                // Retreive the reaction that started this speech Audio
                                S3DReact reaction = GetReaction(speechAudio.playedByReactId);

                                if (reaction != null && speechAudio.guidHash != 0)
                                {
                                    if (reaction.isEnterReactEnabled)
                                    {
                                        // Is this a speechAudio that just stopped playing from an enter reaction?
                                        if (reaction.enabledEnterReactSpeechAudio != null && reaction.enabledEnterReactSpeechAudio.s3dSpeechAudioId == speechAudio.guidHash)
                                        {
                                            //Debug.Log("[DEBUG] (Reaction) PlayOrStopSpeechAudio (stop) " + speechAudio.GetClipName + " for Enter reaction: " + reaction.reactName + " T:" + Time.time);
                                            FinaliseReaction(reaction, reaction.enabledEnterReactSpeechAudio, speechAudio);
                                        }
                                    }

                                    if (reaction.isExitReactEnabled)
                                    {
                                        // Is this a speechAudio that just stopped playing from an exit reaction?
                                        if (reaction.enabledExitReactSpeechAudio != null && reaction.enabledExitReactSpeechAudio.s3dSpeechAudioId == speechAudio.guidHash)
                                        {
                                            FinaliseReaction(reaction, reaction.enabledExitReactSpeechAudio, speechAudio);
                                        }
                                    }
                                }

                                speechAudio.playedByReactId = 0;
                            }
                        }
                    }
                    else
                    {
                        // Cannot play this clip
                        speechAudio.playedByReactId = 0;
                    }
                }
                #if UNITY_EDITOR
                else if (phonemeAudioSource == null)
                {
                    Debug.LogWarning("StickyShapeModule PlayOrStopSpeechAudio could not be actioned on " + name + " because the Audio Source is missing. Check the Phonetic Settings.");
                }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("StickyShapeModule PlayOrStopSpeechAudio could not be actioned on " + name + " because the module has not been initialised."); }
            #endif
        }

        /// <summary>
        /// Attempt to set the weighted values for all the blendshapes defined in the phoneme.
        /// </summary>
        /// <param name="phoneme"></param>
        /// <param name="phonemeValue"></param>
        protected void SetPhonemeValue (S3DPhoneme phoneme, float phonemeValue)
        {
            if (isInitialised)
            {
                // Clamp input value 0.0 to 1.0
                if (phonemeValue < 0f) { phonemeValue = 0f; }
                else if (phonemeValue > 1f) { phonemeValue = 1f; }

                // Loop through all the S3DEmotionShapes for this emotion,
                // and assign the value proportionally to the MaxWeight of that shape.
                for (int bsIdx = 0; bsIdx < phoneme.numShapes; bsIdx++)
                {
                    S3DEmotionShape emotionShape = phoneme.emotionShapeList[bsIdx];

                    // Does the emotionShape have a corresponding S3DBlendShape?
                    if (emotionShape.isSynced)
                    {
                        S3DBlendShape blendShape = GetBlendShape(emotionShape.s3dBlendShapeId);

                        if (blendShape.isValid)
                        {
                            blendShape.skinnedMeshRenderer.SetBlendShapeWeight(blendShape.blendShapeIndex, phonemeValue * emotionShape.maxWeight);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If a phoneme is already fading in or out or playing,
        /// immediately stop that.
        /// NOTE: This does not update the isFadingIn or Out variables.
        /// </summary>
        /// <param name="phoneme"></param>
        protected void StopPhonemeFadeRoutines(S3DPhoneme phoneme)
        {
            // If already fading in, stop doing that
            if (phoneme.fadeInEnumerator != null)
            {
                StopCoroutine(phoneme.fadeInEnumerator);
            }

            // If already fading out, stop doing that
            if (phoneme.fadeOutEnumerator != null)
            {
                StopCoroutine(phoneme.fadeOutEnumerator);
            }

            // If already fading in or out, stop doing that
            if (phoneme.fadeInOutEnumerator != null)
            {
                StopCoroutine(phoneme.fadeInOutEnumerator);
            }
        }

        /// <summary>
        /// Read the speech audio clip and play an appropriate phoneme
        /// </summary
        /// <param name="dTime">frame DeltaTime</param>
        protected virtual void UpdateSpeech(float dTime)
        {
            if (phonemeAudioSource.isPlaying)
            {
                float audioTime = phonemeAudioSource.time;

                float vol = currentSpeechAudio.GetMaxVolume(audioTime);
                bool isSilent = vol <= currentSpeechAudio.silenceThreshold;

                // If not silent, and the previous phoneme has finished playing,
                // find and start playing another phoneme.
                // NOTE: This looks better than immediately starting to fade out
                // a phoneme that is still fading in when a silent patch is reached.
                if (!isSpeechPhonemePlaying && !isSilent)
                {
                    PlayNextPhoneme(currentSpeechAudio.speechSpeed > S3DSpeechAudio.MinSpeechSpeed ? currentSpeechAudio.speechSpeed : S3DSpeechAudio.MinSpeechSpeed);
                }

                if (isSpeechEmotionPlaying)
                {
                    // If at or below the speech volume threshold, fadeout the emotion
                    // NOTE: This is NOT the default setting. Emotions may never reach their maximum set weight.
                    if (isSilent && currentSpeechAudio.isEmotionFadeOnSilence && !currentSpeechAudio.isEmotionsIgnoreSilence && !speechEmotionPlaying.isFadingOut)
                    {
                        StopEmotion(speechEmotionPlaying);
                    }
                }
                else
                {
                    if (currentSpeechAudio.isEmotionsIgnoreSilence)
                    {
                        // Set the interval if it hasn't already been set since the last
                        // emotion finished playing or one hasn't been played yet.
                        if (currentSpeechAudio.intervalTimer <= 0f)
                        {
                            currentSpeechAudio.intervalTimer = s3dRandom.Range(currentSpeechAudio.emotionMinInterval, currentSpeechAudio.emotionMaxInterval);
                        }
                        else
                        {
                            currentSpeechAudio.intervalTimer -= dTime;

                            // When we finish the countdown, set to 0 so that
                            // we run PlayNextEmotion(..).
                            if (currentSpeechAudio.intervalTimer <= 0f)
                            {
                                currentSpeechAudio.intervalTimer = 0f;
                            }
                        }
                    }

                    if ((!isSilent && !currentSpeechAudio.isEmotionsIgnoreSilence) || (currentSpeechAudio.isEmotionsIgnoreSilence && currentSpeechAudio.intervalTimer == 0f))
                    {
                        PlayNextEmotion(currentSpeechAudio.speechSpeed > S3DSpeechAudio.MinSpeechSpeed ? currentSpeechAudio.speechSpeed : S3DSpeechAudio.MinSpeechSpeed);
                    }
                }
            }
            else
            {                
                currentSpeechAudio.intervalTimer = 0f;
                StopSpeechAudio(currentSpeechAudio);
            }
        }

        #endregion

        #region Protected and Internal Methods - React

        /// <summary>
        /// Enable or disable all reactions
        /// </summary>
        /// <param name="isEnabled"></param>
        /// <param name="isInstant"></param>
        protected void EnableOrDisableReacting(bool isEnabled, bool isInstant)
        {
            // Only change state if it is required
            if (isEnabled == isReactingEnabled) { return; }

            if (isEnabled)
            {
                bool isS3DTriggerEnabled = false;

                if (!isNonS3DController && stickyControlModule != null)
                {
                    // S3D characters require "Trigger Collider" to be enabled on the Collide tab
                    isS3DTriggerEnabled = stickyControlModule.isTriggerColliderEnabled;

                    if (!isS3DTriggerEnabled)
                    {
                        Debug.LogWarning("[ERROR] StickyShapeModule.EnableOrDisableReacting - " + name + " can only react to others if Trigger Collider is enabled on the Collide tab of the StickyControlModule.");
                    }
                }

                isReactingEnabled = !isNonS3DController || isS3DTriggerEnabled;
            }
            else
            {
                isReactingEnabled = false;
            }
        }

        /// <summary>
        /// Attempt to Start or stop a reaction stage.
        /// reactionStageInt 0 = Enter, 1 = Stay, 2 = Exit
        /// Could include ReactEmotions and/or ReactSpeechAudio and/or ReactEvent (FUTURE).
        /// </summary>
        /// <param name="react"></param>
        /// <param name="isEnabled"></param>
        /// <param name="isInstant"></param>
        /// <param name="reactionStageInt"></param>
        protected virtual void EnableOrDisableReaction (S3DReact react, bool isEnabled, bool isInstant, int reactionStageInt)
        {
            if (react != null)
            {
                // Only change state if it is required
                if (reactionStageInt == S3DReact.ReactStageEnterInt)
                {
                    if (isEnabled == react.isEnterReactEnabled) { return; }
                }
                else if (reactionStageInt == S3DReact.ReactStageExitInt)
                {
                    if (isEnabled == react.isExitReactEnabled) { return; }
                }
                else if (reactionStageInt == S3DReact.ReactStageStayInt)
                {
                    if (isEnabled == react.isStayReactEnabled) { return; }
                }

                // If disabling and the whole S3DReact is already disabled, there is nothing to do
                if (!isEnabled && !react.isReactEnabled) { return; }

                // NOTE: ReactTo rules are processed in CharacterEnter(..)

                if (isEnabled)
                {
                    #region Enable
                    if (reactionStageInt == S3DReact.ReactStageEnterInt)
                    {
                        #region Enter Emotion
                        S3DReactEmotion _reactEmotion = null;

                        // When in previewmode, ignore (most) reaction rules - just select an emotion and enable it

                        // Fetch an emotion randomly from the list of enter emotions
                        int emotionIdx = react.numEnterEmotions == 1 ? 0 : react.numEnterEmotions == 0 ? -1 : s3dRandom.Range(0, react.numEnterEmotions-1);

                        if (emotionIdx >= 0)
                        {
                            _reactEmotion = react.enterReactEmotionList[emotionIdx];

                            // Remember the stage this applies to so that we can
                            // set if the stage is enabled or disabled
                            _reactEmotion.reactStageInt = reactionStageInt;

                            PlayReaction(react, _reactEmotion);
                        }
                        #endregion

                        #region Enter Speech Audio

                        S3DReactSpeechAudio _reactSpeechAudio = null;

                        // Fetch an speech audio randomly from the list of enter speech audios
                        int speechAudioIdx = react.numEnterSpeechAudios == 1 ? 0 : react.numEnterSpeechAudios == 0 ? -1 : s3dRandom.Range(0, react.numEnterSpeechAudios - 1);

                        if (speechAudioIdx >= 0)
                        {
                            _reactSpeechAudio = react.enterReactSpeechAudioList[speechAudioIdx];

                            _reactSpeechAudio.reactStageInt = reactionStageInt;

                            PlayReaction(react, _reactSpeechAudio);
                        }

                        #endregion

                        #region Post-Enter Event (No Emotions or SpeechAudio)
                        // This only fires here if no emotions or speech audio are played.
                        if (emotionIdx < 0 && speechAudioIdx < 0)
                        {
                            if (react.onPostEnter != null)
                            {
                                react.onPostEnter.Invoke(react.stickyID, react.reactedToStickyID, react.reactedToModelId, react.reactedToFriendOrFoe);
                            }
                            react.isEnterReactEnabled = false;
                        }

                        #endregion
                    }
                    else if (reactionStageInt == S3DReact.ReactStageStayInt)
                    {
                        #region Stay Emotion (FUTURE)

                        #endregion

                        #region Enter Speech Audio (FUTURE)

                        #endregion

                        #region Enter Event (FUTURE)

                        #endregion
                    }
                    else if (reactionStageInt == S3DReact.ReactStageExitInt)
                    {
                        #region Exit Emotion
                        S3DReactEmotion _reactEmotion = null;

                        // When in previewmode, ignore (most) reaction rules - just select an emotion and enable it

                        // Fetch an emotion randomly from the list of enter emotions
                        int emotionIdx = react.numExitEmotions == 1 ? 0 : react.numExitEmotions == 0 ? -1 : s3dRandom.Range(0, react.numExitEmotions-1);

                        if (emotionIdx >= 0)
                        {
                            _reactEmotion = react.exitReactEmotionList[emotionIdx];

                            // Remember the stage this applies to so that we can
                            // set if the stage is enabled or disabled
                            _reactEmotion.reactStageInt = reactionStageInt;

                            PlayReaction(react, _reactEmotion);
                        }
                        #endregion

                        #region Exit Speech Audio

                        S3DReactSpeechAudio _reactSpeechAudio = null;

                        // Fetch a speech audio randomly from the list of exit speech audios
                        int speechAudioIdx = react.numExitSpeechAudios == 1 ? 0 : react.numExitSpeechAudios == 0 ? -1 : s3dRandom.Range(0, react.numExitSpeechAudios - 1);

                        if (speechAudioIdx >= 0)
                        {
                            _reactSpeechAudio = react.exitReactSpeechAudioList[speechAudioIdx];

                            _reactSpeechAudio.reactStageInt = reactionStageInt;

                            PlayReaction(react, _reactSpeechAudio);
                        }

                        #endregion

                        #region Post-Exit Event (No Emotions or SpeechAudio)
                        // This only fires here if no emotions or speech audio are played.
                        if (emotionIdx < 0 && speechAudioIdx < 0)
                        {
                            if (react.onPostExit != null)
                            {
                                react.onPostExit.Invoke(react.stickyID, react.reactedToStickyID, react.reactedToModelId, react.reactedToFriendOrFoe);
                            }

                            react.isExitReactEnabled = false;
                        }
                        #endregion
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: EnableOrDisableReaction on " + name + " - Invalid reactionStageInt (" + reactionStageInt + ")"); }
                    #endif
                    #endregion
                }
                else
                {
                    #region Disable

                    // Attempt to retrieve and stop the emotion in use for this reaction.
                    S3DEmotion emotion;
                    S3DSpeechAudio speechAudio;
                    bool isEmotionEnabled = false;
                    if (reactionStageInt == S3DReact.ReactStageEnterInt)
                    {
                        #region Enter Emotion
                        if (GetEnabledEmotion(react.enabledEnterReactEmotion, out emotion))
                        {
                            if (isInstant)
                            {
                                StopEmotionInstantly(emotion);
                                react.enabledEnterReactEmotion = null;
                            }
                            else
                            {
                                StopEmotion(emotion);
                                // This is not instant, so maybe need some way of getting notified when has finished fading out.
                                isEmotionEnabled = true;
                            }
                        }
                        else
                        {
                            react.enabledEnterReactEmotion = null;
                        }
                        #endregion

                        #region Enter Speech Audio
                        if (GetPlayingSpeechAudio(react.enabledEnterReactSpeechAudio, out speechAudio))
                        {
                            StopSpeechAudio(speechAudio);
                        }
                        react.enabledEnterReactSpeechAudio = null;

                        #endregion

                        react.isEnterReactEnabled = isEmotionEnabled;
                    }
                    else if (reactionStageInt == S3DReact.ReactStageStayInt)
                    {
                        #region Stay Emotion (FUTURE)

                        #endregion

                        #region Enter Speech Audio (FUTURE)

                        #endregion

                        react.isStayReactEnabled = isEmotionEnabled;
                    }
                    else if (reactionStageInt == S3DReact.ReactStageExitInt)
                    {
                        #region Stay Emotion
                        if (GetEnabledEmotion(react.enabledExitReactEmotion, out emotion))
                        {
                            if (isInstant)
                            {
                                StopEmotionInstantly(emotion);
                                react.enabledExitReactEmotion = null;
                            }
                            else
                            {
                                StopEmotion(emotion);
                                // This is not instant, so maybe need some way of getting notified when has finished fading out.
                                isEmotionEnabled = true;
                            }
                        }
                        else
                        {
                            react.enabledExitReactEmotion = null;
                        }

                        #endregion

                        #region Exit Speech Audio
                        if (GetPlayingSpeechAudio(react.enabledExitReactSpeechAudio, out speechAudio))
                        {
                            StopSpeechAudio(speechAudio);
                        }
                        react.enabledExitReactSpeechAudio = null;

                        #endregion

                        react.isExitReactEnabled = isEmotionEnabled;
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: EnableOrDisableReaction on " + name + " - Invalid reactionStageInt (" + reactionStageInt + ")"); }
                    #endif
                    #endregion
                }

                //Debug.Log("[DEBUG] EnableOrDisableReaction() - isEnabled: " + isEnabled + " stage: " + (S3DReact.ReactionStage)reactionStageInt + " T:" + Time.time);

                // Update the state of the S3DReact instance
                react.isReactEnabled = react.isEnterReactEnabled || react.isExitReactEnabled || react.isStayReactEnabled;

                if (!react.isReactEnabled)
                {
                    // Reset the reactedTo values after they have been used.
                    react.reactedToStickyID = 0;
                    react.reactedToModelId = 0;
                    react.reactedToFriendOrFoe = 0;
                }
            }
        }

        /// <summary>
        /// Disable the proximity collider
        /// </summary>
        private void DisableProximityCollider()
        {
            if (proximityCollider != null && proximityCollider.isTrigger && proximityCollider.enabled)
            {
                proximityCollider.enabled = false;
            }
        }

        /// <summary>
        /// Attempt to fade an emotion from a reaction in, then out.
        /// </summary>
        /// <param name="react"></param>
        /// <param name="reactEmotion"></param>
        /// <param name="emotion"></param>
        /// <returns></returns>
        protected IEnumerator FadeInOutReaction(S3DReact react, S3DReactEmotion reactEmotion, S3DEmotion emotion)
        {
            // Avoid div0
            if (emotion.fadeInDuration < S3DEmotion.MinFadeInOutDuration) { emotion.fadeInDuration = S3DEmotion.MinFadeInOutDuration; }
            if (emotion.fadeOutDuration < S3DEmotion.MinFadeInOutDuration) { emotion.fadeOutDuration = S3DEmotion.MinFadeInOutDuration; }

            float holdDuration = GetEmotionHoldDuration(emotion, reactEmotion.reactStageInt);

            float timer = emotion.fadeInDuration;

            float startTime = Time.time;
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.02f);

            // Reverse the fade
            if (emotion.isFadingOut)
            {
                timer = emotion.fadeInDuration * emotion.fadeProgress;
                emotion.fadeProgress = 1f - emotion.fadeProgress;
            }
            // Already partially faded in
            else if (emotion.isFadingIn)
            {
                timer = emotion.fadeInDuration * (1f - emotion.fadeProgress);
            }
            else
            {
                emotion.fadeProgress = 0f;
            }

            // Fade in phase
            emotion.isFadingIn = true;
            emotion.isFadingOut = false;

            //Debug.Log("[DEBUG] Fade reaction emotion in T:" + Time.time);

            while (timer >= 0f)
            {
                timer -= Time.deltaTime;
                emotion.fadeProgress = (emotion.fadeInDuration - timer) / emotion.fadeInDuration;
                SetEmotionValue(emotion, emotion.fadeProgress);
                yield return waitForSeconds;
            }

            //Debug.Log("[DEBUG] Hold reaction emotion T:" + Time.time);

            yield return new WaitForSeconds(holdDuration);

            //Debug.Log("[DEBUG] Fade reaction emotion out T:" + Time.time);

            // Fading out phase
            emotion.isFadingOut = true;
            emotion.fadeProgress = 0f;
            timer = emotion.fadeOutDuration;

            while (timer >= 0f)
            {
                timer -= Time.deltaTime;
                emotion.fadeProgress = timer / emotion.fadeOutDuration;
                SetEmotionValue(emotion, emotion.fadeProgress);

                yield return waitForSeconds;
            }

            FinaliseReaction(react, reactEmotion, emotion);
            //Debug.Log("[DEBUG] Finished reaction emotion T:" + Time.time);

            yield return null;
        }

        /// <summary>
        /// Attempt to fade out a reaction
        /// </summary>
        /// <param name="react"></param>
        /// <param name="reactEmotion"></param>
        /// <param name="emotion"></param>
        /// <returns></returns>
        protected IEnumerator FadeOutReaction(S3DReact react, S3DReactEmotion reactEmotion, S3DEmotion emotion)
        {
            bool wasFadingOut = emotion.isFadingOut;

            emotion.isFadingOut = true;
            if (emotion.fadeOutDuration == 0) { emotion.fadeOutDuration = 0.01f; }

            float startTime = Time.time;
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.02f);

            float timer = emotion.fadeOutDuration;

            // Reverse the fade
            if (emotion.isFadingIn)
            {
                timer = emotion.fadeOutDuration * emotion.fadeProgress;
            }
            else if (wasFadingOut)
            {
                // Maybe we were already fading out when this was called
                timer = emotion.fadeOutDuration * (1f - emotion.fadeProgress);
            }
            else
            {
                emotion.fadeProgress = 0f;
            }

            emotion.isFadingIn = false;

            while (timer >= 0f)
            {
                timer -= Time.deltaTime;
                emotion.fadeProgress = timer / emotion.fadeOutDuration;
                SetEmotionValue(emotion, emotion.fadeProgress);
                yield return waitForSeconds;
            }

            FinaliseReaction(react, reactEmotion, emotion);

            yield return null;
        }

        /// <summary>
        /// Just before a reaction finishes playing, we need to reset variables to make sure things
        /// are in the correct state. Assumes parameters are not null.
        /// </summary>
        /// <param name="react"></param>
        /// <param name="reactEmotion"></param>
        /// <param name="emotion"></param>
        protected void FinaliseReaction(S3DReact react, S3DReactEmotion reactEmotion, S3DEmotion emotion)
        {
            emotion.isFadingIn = false;
            emotion.isFadingOut = false;
            emotion.isEmotionEnabled = false;
            emotion.isPreviewMode = false;

            // Check to see if there is also a speechAudio playing for this reaction stage.
            // If so, the reaction stage will not be disabled.
            bool isReactSpeechAudioPlaying = IsReactSpeechAudioPlaying(react, reactEmotion.reactStageInt);

            if (isSpeechEmotionPlaying && speechEmotionPlaying.guidHash == emotion.guidHash)
            {
                speechEmotionPlaying = null;
                isSpeechEmotionPlaying = false;
            }

            if (reactEmotion.reactStageInt == S3DReact.ReactStageEnterInt)
            {
                react.enabledEnterReactEmotion = null;
                react.isEnterReactEnabled = isReactSpeechAudioPlaying;

                #if UNITY_EDITOR
                react.isEnterPreviewMode = isReactSpeechAudioPlaying;
                #endif

                if (!react.isEnterReactEnabled && react.onPostEnter != null)
                {
                    react.onPostEnter.Invoke(react.stickyID, react.reactedToStickyID, react.reactedToModelId, react.reactedToFriendOrFoe);
                }
            }
            else if (reactEmotion.reactStageInt == S3DReact.ReactStageExitInt)
            {
                react.enabledExitReactEmotion = null;
                react.isExitReactEnabled = isReactSpeechAudioPlaying;

                #if UNITY_EDITOR
                react.isExitPreviewMode = isReactSpeechAudioPlaying;
                #endif

                if (!react.isExitReactEnabled && react.onPostExit != null)
                {
                    react.onPostExit.Invoke(react.stickyID, react.reactedToStickyID, react.reactedToModelId, react.reactedToFriendOrFoe);
                }
            }
            else if (reactEmotion.reactStageInt == S3DReact.ReactStageStayInt)
            {
                react.enabledStayReactEmotion = null;
                react.isStayReactEnabled = isReactSpeechAudioPlaying;

                #if UNITY_EDITOR
                react.isStayPreviewMode = isReactSpeechAudioPlaying;
                #endif
            }

            // Update the state of the S3DReact instance
            react.isReactEnabled = react.isEnterReactEnabled || react.isExitReactEnabled || react.isStayReactEnabled;

            if (!react.isReactEnabled)
            {
                // Reset the reactedTo values after they have been used.
                react.reactedToStickyID = 0;
                react.reactedToModelId = 0;
                react.reactedToFriendOrFoe = 0;
            }
        }

        /// <summary>
        /// When a speechAudio is stopped for a reaction stage, we need to reset some variables.
        /// </summary>
        /// <param name="react"></param>
        /// <param name="reactSpeechAudio"></param>
        /// <param name="speechAudio"></param>
        protected void FinaliseReaction(S3DReact react, S3DReactSpeechAudio reactSpeechAudio, S3DSpeechAudio speechAudio)
        {
            // Check to see if there is also an emotion for this reaction stage playing
            // Don't disable the reaction if there is.
            bool isReactEmotionEnabled = IsReactEmotionPlaying(react, reactSpeechAudio.reactStageInt); 

            if (reactSpeechAudio.reactStageInt == S3DReact.ReactStageEnterInt)
            {
                react.enabledEnterReactSpeechAudio = null;
                react.isEnterReactEnabled = isReactEmotionEnabled;

                #if UNITY_EDITOR
                react.isEnterPreviewMode = isReactEmotionEnabled;
                #endif

                if (!react.isEnterReactEnabled && react.onPostEnter != null)
                {
                    react.onPostEnter.Invoke(react.stickyID, react.reactedToStickyID, react.reactedToModelId, react.reactedToFriendOrFoe);
                }
            }
            else if (reactSpeechAudio.reactStageInt == S3DReact.ReactStageExitInt)
            {
                react.enabledExitReactSpeechAudio = null;
                react.isExitReactEnabled = isReactEmotionEnabled;

                #if UNITY_EDITOR
                react.isExitPreviewMode = isReactEmotionEnabled;
                #endif

                if (!react.isExitReactEnabled && react.onPostExit != null)
                {
                    react.onPostExit.Invoke(react.stickyID, react.reactedToStickyID, react.reactedToModelId, react.reactedToFriendOrFoe);
                }
            }
            else if (reactSpeechAudio.reactStageInt == S3DReact.ReactStageStayInt)
            {
                react.enabledStayReactSpeechAudio = null;
                react.isStayReactEnabled = isReactEmotionEnabled;

                #if UNITY_EDITOR
                react.isStayPreviewMode = isReactEmotionEnabled;
                #endif
            }

            // Update the state of the S3DReact instance
            react.isReactEnabled = react.isEnterReactEnabled || react.isExitReactEnabled || react.isStayReactEnabled;

            if (!react.isReactEnabled)
            {
                // Reset the reactedTo values after they have been used.
                react.reactedToStickyID = 0;
                react.reactedToModelId = 0;
                react.reactedToFriendOrFoe = 0;
            }
        }

        /// <summary>
        /// Is the reactEmotion (if any) for a reaction stage currently playing (enabled)?
        /// </summary>
        /// <param name="react"></param>
        /// <param name="reactStageInt"></param>
        /// <returns></returns>
        protected bool IsReactEmotionPlaying(S3DReact react, int reactStageInt)
        {
            S3DEmotion emotion = null;

            if (reactStageInt == S3DReact.ReactStageEnterInt) { emotion = GetEmotion(react.enabledEnterReactEmotion); }
            else if (reactStageInt == S3DReact.ReactStageExitInt) { emotion = GetEmotion(react.enabledExitReactEmotion); }
            if (reactStageInt == S3DReact.ReactStageStayInt) { emotion = GetEmotion(react.enabledStayReactEmotion); }

            return emotion != null && emotion.isEmotionEnabled;
        }

        /// <summary>
        /// Checks to see if the speechAudio for this stage of the reaction is playing.
        /// If it is set as playing, but has stopped, it will update the status.
        /// </summary>
        /// <param name="react"></param>
        /// <param name="reactStageInt"></param>
        protected bool IsReactSpeechAudioPlaying(S3DReact react, int reactStageInt)
        {
            bool isPlaying = false;

            S3DSpeechAudio speechAudio = null;

            if (reactStageInt == S3DReact.ReactStageEnterInt) { speechAudio = GetSpeechAudio(react.enabledEnterReactSpeechAudio); }
            else if (reactStageInt == S3DReact.ReactStageExitInt) { speechAudio = GetSpeechAudio(react.enabledExitReactSpeechAudio); }
            if (reactStageInt == S3DReact.ReactStageStayInt) { speechAudio = GetSpeechAudio(react.enabledStayReactSpeechAudio); }

            if (speechAudio != null && phonemeAudioSource != null && isSpeechPlaying && speechAudio.audioClip != null)
            {
                if (phonemeAudioSource.isPlaying)
                {
                    isPlaying = true;
                }
                else
                {
                    // Maybe it has stopped already

                    if (reactStageInt == S3DReact.ReactStageEnterInt) { react.enabledEnterReactSpeechAudio = null; }
                    else if (reactStageInt == S3DReact.ReactStageExitInt) { react.enabledExitReactSpeechAudio = null; }
                    if (reactStageInt == S3DReact.ReactStageStayInt) { react.enabledStayReactSpeechAudio = null; }

                    isSpeechPlaying = false;
                }
            }

            return isPlaying;
        }

        /// <summary>
        /// Play (with fade-in) or stop (with fade-out) a reactEmotion
        /// Assumes react and reactEmotion are not null.
        /// </summary>
        /// <param name="react"></param>
        /// <param name="reactEmotion"></param>
        /// <param name="isPlay"></param>
        protected virtual void PlayOrStopReaction (S3DReact react, S3DReactEmotion reactEmotion, bool isPlay)
        {
            S3DEmotion emotion = GetEmotion(reactEmotion);

            if (!reactEmotion.isSynced)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR: PlayOrStopReaction - (" + (S3DReact.ReactionStage)reactEmotion.reactStageInt + ") reactEmotion for reaction (" + react.reactName + ") on " + name + " is not syned with Emotions tab");
                #endif
            }
            else if (emotion == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR: PlayOrStopReaction - (" + (S3DReact.ReactionStage)reactEmotion.reactStageInt + ") could not find emotion for reaction (" + react.reactName + ") on " + name);
                #endif
            }
            else
            {
                // Update the cached number of shapes so it can be used in SetEmotionValue(..)
                emotion.numShapes = emotion.emotionShapeList.Count;

                StopReactEmotionFadeRoutines(reactEmotion, emotion);

                // When playing a reaction, the emotion should not also be in preview
                // as this reaction could have been triggered by preview mode on the Reaction.
                emotion.isPreviewMode = false;

                if (isPlay)
                {
                    emotion.isEmotionEnabled = true;

                    if (reactEmotion.reactStageInt == S3DReact.ReactStageEnterInt)
                    {
                        // Remember currently active emotion
                        react.enabledEnterReactEmotion = reactEmotion;

                        react.isEnterReactEnabled = true;

                        #if UNITY_EDITOR
                        react.isEnterPreviewMode = isPlay;
                        #endif
                    }
                    else if (reactEmotion.reactStageInt == S3DReact.ReactStageExitInt)
                    {
                        // Remember currently active emotion
                        react.enabledExitReactEmotion = reactEmotion;
                        react.isExitReactEnabled = true;

                        #if UNITY_EDITOR
                        react.isExitPreviewMode = isPlay;
                        #endif
                    }
                    else if (reactEmotion.reactStageInt == S3DReact.ReactStageStayInt)
                    {
                        // Remember currently active emotion
                        react.enabledStayReactEmotion = reactEmotion;
                        react.isStayReactEnabled = true;

                        #if UNITY_EDITOR
                        react.isStayPreviewMode = isPlay;
                        #endif
                    }

                    reactEmotion.fadeInOutEnumerator = FadeInOutReaction(react, reactEmotion, emotion);
                    StartCoroutine(reactEmotion.fadeInOutEnumerator);
                }
                else
                {
                    reactEmotion.fadeOutEnumerator = FadeOutReaction(react, reactEmotion, emotion);
                    StartCoroutine(reactEmotion.fadeOutEnumerator);
                }
            }
        }

        /// <summary>
        /// Play or stop a reactSpeechAudio.
        /// Assumes react and reactSpeechAudio are not null.
        /// </summary>
        /// <param name="react"></param>
        /// <param name="reactSpeechAudio"></param>
        /// <param name="isPlay"></param>
        protected virtual void PlayOrStopReaction(S3DReact react, S3DReactSpeechAudio reactSpeechAudio, bool isPlay)
        {
            S3DSpeechAudio speechAudio = GetSpeechAudio(reactSpeechAudio);

            if (!reactSpeechAudio.isSynced)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR: PlayOrStopReaction - (" + (S3DReact.ReactionStage)reactSpeechAudio.reactStageInt + ") reactSpeechAudio for reaction (" + react.reactName + ") on " + name + " is not syned with Phonemes tab");
                #endif
            }
            else if (speechAudio == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR: PlayOrStopReaction - (" + (S3DReact.ReactionStage)reactSpeechAudio.reactStageInt + ") could not find speechAudio for reaction (" + react.reactName + ") on " + name);
                #endif
            }
            else
            {
                StopReactSpeechAudioFadeRoutines(reactSpeechAudio, speechAudio);

                // When playing a reaction, the speechaudio should not also be in preview
                // as this reaction could have been triggered by preview mode on the Reaction.
                speechAudio.isPreviewMode = false;

                if (isPlay)
                {
                    if (speechAudio.audioClip != null)
                    {
                        //speechAudio.isSpeechAudioEnabled = true;

                        if (reactSpeechAudio.reactStageInt == S3DReact.ReactStageEnterInt)
                        {
                            // Remember currently active speechAudio
                            react.enabledEnterReactSpeechAudio = reactSpeechAudio;
                            react.isEnterReactEnabled = true;

                            #if UNITY_EDITOR
                            react.isEnterPreviewMode = isPlay;
                            #endif
                        }
                        else if (reactSpeechAudio.reactStageInt == S3DReact.ReactStageExitInt)
                        {
                            // Remember currently active speechAudio
                            react.enabledExitReactSpeechAudio = reactSpeechAudio;
                            react.isExitReactEnabled = true;

                            #if UNITY_EDITOR
                            react.isExitPreviewMode = isPlay;
                            #endif
                        }
                        else if (reactSpeechAudio.reactStageInt == S3DReact.ReactStageStayInt)
                        {
                            // Remember currently active speechAudio
                            react.enabledStayReactSpeechAudio = reactSpeechAudio;
                            react.isStayReactEnabled = true;

                            #if UNITY_EDITOR
                            react.isStayPreviewMode = isPlay;
                            #endif
                        }

                        // Remember what started this speechAudio
                        speechAudio.playedByReactId = react.guidHash;

                        PlayOrStopSpeechAudio(speechAudio, isPlay, false);
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("[ERROR: PlayOrStopReaction - speechAudio for " + (S3DReact.ReactionStage)reactSpeechAudio.reactStageInt + " reaction (" + react.reactName + ") on " + name + " has no audioClip"); }
                    #endif
                }
                else
                {
                    PlayOrStopSpeechAudio(speechAudio, isPlay, false);

                    /// TODO - when react Speech audio stop completely, then set.
                    /// Currently this assumes it stops instantly
                    FinaliseReaction(react, reactSpeechAudio, speechAudio);
                } 
            }
        }

        /// <summary>
        /// Process all the Enter emotions in each Reaction
        /// </summary>
        /// <param name="stickyID">StickyID of this character</param>
        /// <param name="reactToStickyID">StickyID of the other character we are reacting to</param>
        /// <param name="friendOrFoe"></param>
        /// <param name="reactStageInt"></param>
        /// <param name="otherModelId"></param>
        protected void ReactToNearby(int stickyID, int reactToStickyID, int friendOrFoe, int reactStageInt, int otherModelId)
        {
            for (int reIdx = 0; reIdx < numReactions; reIdx++)
            {
                S3DReact reaction = reactList[reIdx];

                if (reaction != null)
                {
                    bool isReact = false;

                    if (!reaction.isReactEnabled)
                    {
                        // Start react to nobody
                        reaction.reactedToStickyID = 0;
                        reaction.reactedToModelId = 0;
                        reaction.reactedToFriendOrFoe = 0;
                    }

                    if (reaction.reactToInt == S3DReact.ReactToAnyInt) { isReact = true; }
                    else if (reaction.reactToInt == S3DReact.ReactToFriendlyInt) { isReact = friendOrFoe >= 0; }
                    else if (reaction.reactToInt == S3DReact.ReactToFriendOnlyInt) { isReact = friendOrFoe > 0; }
                    else if (reaction.reactToInt == S3DReact.ReactToFoeInt) { isReact = friendOrFoe < 0; }
                    else if (reaction.reactToInt == S3DReact.ReactToNeutralOnlyInt) { isReact = friendOrFoe == 0; }

                    // Avoid GC by using Array.IndexOf( ) > -1 rather than Array.Exists(..)
                    isReact = isReact && (reaction.numModelsToInclude < 1 || System.Array.IndexOf(reaction.modelsToInclude, otherModelId) > -1);

                    if (isReact)
                    {
                        // Check if this reaction is already in use
                        // We do the check here to avoid alerting on other characters we weren't going to react to anyway.
                        if (reaction.isReactEnabled)
                        {
                            // In the future we may be able do say an enter AND exit reaction to the same character.
                            // Or maybe stop an enter reaction if an exit reaction is required for the same character.

                            #if UNITY_EDITOR
                            Debug.Log("[INFO] " + reaction.reactName + " on " + name + " is already reacting to StickyID: " + reaction.reactedToStickyID + " (Model Id: " + reaction.reactedToModelId + ") and therefore cannot react to " + reactToStickyID + " (Model Id: " + otherModelId + ")");
                            #endif

                            // Stop processing this reaction if we cannot react to this nearby character.
                            continue;
                        }

                        reaction.stickyID = stickyID;
                        reaction.reactedToStickyID = reactToStickyID;
                        reaction.reactedToModelId = otherModelId;
                        reaction.reactedToFriendOrFoe = friendOrFoe;

                        if (reactStageInt == S3DReact.ReactStageEnterInt)
                        {
                            if (reaction.onPreEnter != null)
                            {
                                reaction.onPreEnter.Invoke(stickyID, reactToStickyID, friendOrFoe, otherModelId);
                            }
                            PlayEnterReaction(reaction);
                        }
                        else if (reactStageInt == S3DReact.ReactStageExitInt)
                        {
                            if (reaction.onPreExit != null)
                            {
                                reaction.onPreExit.Invoke(stickyID, reactToStickyID, friendOrFoe, otherModelId);
                            }
                            PlayExitReaction(reaction);
                        }
                        //else if (reactStageInt == S3DReact.ReactStageStayInt) { PlayStayReaction(reaction); }
                    }
                }
            }
        }

        /// <summary>
        /// Delay the ReactToNearby - used to avoid startup issues like slurred speech.
        /// This will automatically be stopped by Unity if the object is destroyed.
        /// </summary>
        /// <param name="stickyID">StickyID of this character</param>
        /// <param name="reactToStickyID">StickyID of the other character we are reacting to</param>
        /// <param name="friendOrFoe"></param>
        /// <param name="reactStageInt"></param>
        /// <param name="otherModelId"></param>
        /// <returns></returns>
        protected IEnumerator ReactToNearbyDelayed(int stickyID, int reactToStickyID, int friendOrFoe, int reactStageInt, int otherModelId)
        {
            yield return null;

            ReactToNearby(stickyID, reactToStickyID, friendOrFoe, reactStageInt, otherModelId);
        }

        /// <summary>
        /// Refresh or repopulate the appropriate list of emotions for the given stage of the reaction.
        /// </summary>
        /// <param name="reaction"></param>
        /// <param name="reactionStage"></param>
        protected void RefreshAutoEmotionList(S3DReact reaction, S3DReact.ReactionStage reactionStage)
        {
            List<S3DReactEmotion> autoList = reactionStage == S3DReact.ReactionStage.Enter ? reaction.enterReactEmotionList : (reactionStage == S3DReact.ReactionStage.Exit ? reaction.enterReactEmotionList : null);

            autoList.Clear();

            tempEmotionList.Clear();

            if (GetEmotionsByIntensity(tempEmotionList, reaction.reactTo))
            {
                int numAutoEmotions = tempEmotionList.Count;

                for (int emIdx = 0; emIdx < numAutoEmotions; emIdx++)
                {
                    autoList.Add(new S3DReactEmotion() { s3dEmotionId = tempEmotionList[emIdx].guidHash, isSynced = true });
                }
            }
        }

        /// <summary>
        /// Call this when you wish to remove any custom event listeners, like
        /// after creating them in code and then destroying the object.
        /// You could add this to your game play OnDestroy code.
        /// </summary>
        protected void RemoveListeners(S3DReact reaction)
        {
            if (reaction.onPreEnter != null) { reaction.onPreEnter.RemoveAllListeners(); }
            if (reaction.onPostEnter != null) { reaction.onPostEnter.RemoveAllListeners(); }
            if (reaction.onPreExit != null) { reaction.onPreExit.RemoveAllListeners(); }
            if (reaction.onPostExit != null) { reaction.onPostExit.RemoveAllListeners(); }
        }

        /// <summary>
        /// Attempt to resync the react emotions with the emotions from those
        /// defined on the Emotions tab.
        /// </summary>
        /// <param name="reactEmotionList"></param>
        private void ResyncReactEmotions(List<S3DReactEmotion> reactEmotionList)
        {
            int numReactEmotions = reactEmotionList == null ? 0 : reactEmotionList.Count;
            int emotionId = 0;

            for (int reEmIdx = 0; reEmIdx < numReactEmotions; reEmIdx++)
            {
                S3DReactEmotion reactEmotion = reactEmotionList[reEmIdx];

                if (reactEmotion != null)
                {
                    // Get emotion hashed name from the S3DEmotion being referenced from this react emotion.
                    int nameHash = GetEmotionNameHash(reactEmotion.s3dEmotionId);

                    if (nameHash == 0)
                    {
                        // The S3DEmotion guidHash doesn't exist, so attempt to find with a matching emotion name
                        emotionId = GetEmotionIdByNameHash(reactEmotion.emotionNameHash);

                        reactEmotion.isSynced = emotionId != 0;

                        if (reactEmotion.isSynced)
                        {
                            reactEmotion.s3dEmotionId = emotionId;
                        }
                    }
                    else
                    {
                        // Found the emotion. Does the nameHash in the emotion match the S3DEmotion?
                        if (reactEmotion.emotionNameHash != nameHash)
                        {
                            // The user may have changed the name text of the emotion.
                            // Should be safe to update the hashed name here.
                            reactEmotion.emotionNameHash = nameHash;
                        }

                        reactEmotion.isSynced = true;
                    }
                }
            }
        }

        /// <summary>
        /// If a reactEmotion is already playing, immediately stop that.
        /// If an emotion is already fading in or out or playing,
        /// immediately stop that too.
        /// NOTE: This does not update the isFadingIn or Out variables.
        /// </summary>
        /// <param name="emotion"></param>
        protected void StopReactEmotionFadeRoutines(S3DReactEmotion reactEmotion, S3DEmotion emotion)
        {
            StopEmotionFadeRoutines(emotion);

            // If already playing (fading in and out), stop doing that
            if (reactEmotion.fadeInOutEnumerator != null)
            {
                StopCoroutine(reactEmotion.fadeInOutEnumerator);
            }

            // If already fading out, stop doing that
            if (reactEmotion.fadeOutEnumerator != null)
            {
                StopCoroutine(reactEmotion.fadeOutEnumerator);
            }
        }

        /// <summary>
        /// If a reactSpeechAudio is already playing, immediately stop that.
        /// </summary>
        /// <param name="reactSpeechAudio"></param>
        /// <param name="speechAudio"></param>
        protected void StopReactSpeechAudioFadeRoutines(S3DReactSpeechAudio reactSpeechAudio, S3DSpeechAudio speechAudio)
        {
            // If already playing, stop doing that
            //if (reactSpeechAudio.playEnumerator != null)
            //{
            //    StopCoroutine(reactSpeechAudio.playEnumerator);
            //}
        }

        #endregion

        #region Events

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Attempt to get the animator on a character that does not use the Sticky3D Controller component.
        /// </summary>
        /// <returns></returns>
        public Animator GetNonS3DAnimator()
        {
            if (isNonS3DController)
            {
                if (nonS3DAnimator == null && !gameObject.TryGetComponent(out nonS3DAnimator))
                {
                    return null;
                }
                else { return nonS3DAnimator; }

            }
            else { return null; }
        }

        /// <summary>
        /// Attempt to populate the list with skinned mesh renderers that contain blendshapes
        /// </summary>
        /// <param name="smRendererList"></param>
        /// <returns></returns>
        public bool GetSkinnedMeshRenderers (List<SkinnedMeshRenderer> smRendererList)
        {
            if (smRendererList == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetSkinnedMeshRenderers on " + name + " expects an empty list");
                #endif
                return false;
            }
            else
            {
                smRendererList.Clear();
                tempSMRenList.Clear();
                gameObject.GetComponentsInChildren(true, tempSMRenList);
                int numMRen = tempSMRenList.Count;

                for (int sIdx = 0; sIdx < numMRen; sIdx++)
                {
                    SkinnedMeshRenderer sMRen = smRendererList[sIdx];

                    Mesh mesh = sMRen.sharedMesh;

                    if (mesh != null && mesh.blendShapeCount > 0)
                    {
                        smRendererList.Add(sMRen);
                    }
                }

                tempSMRenList.Clear();
                return true;
            }
        }

        /// <summary>
        /// Get the StickyControlModule attached to this StickyShapesModule.
        /// </summary>
        /// <returns></returns>
        public StickyControlModule GetStickyControlModule()
        {
            if (isInitialised) { return stickyControlModule; }
            else if (!gameObject.TryGetComponent(out stickyControlModule))
            {
                Debug.LogWarning("ERROR StickyShapesModule.GetStickyControlModule() - could not find attached StickyControlModule on " + name);
                return null;
            }
            else { return stickyControlModule; }
        }

        /// <summary>
        /// Initialise the Sticky Shapes Module. Has no effect if called multiple times.
        /// </summary>
        public virtual void Initialise()
        {
            if (isInitialised)
            {
                return;
            }
            else if (!isNonS3DController && !gameObject.TryGetComponent(out stickyControlModule))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: StickyShapeModule on " + name + " could not find the StickyControlModule.");
                #endif
            }
            else if (!isNonS3DController && !stickyControlModule.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: StickyControlModule attached to StickyShapeModule on " + name + " is not initialised ");
                #endif
            }
            else if (!isNonS3DController && !stickyControlModule.IsAnimateEnabled && stickyControlModule.defaultAnimator == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: StickyControlModule attached to StickyShapeModule on " + name + " is not initialised ");
                #endif
            }
            else
            {
                // Keep compiler happy
                if (isSkinnedMRenListExpanded && isBlendShapeListExpanded && isEyeMovementExpanded && isEmotionListExpanded &&
                isPhonemeListExpanded && isAudioSettingsExpanded && isSpeechAudioListExpanded && isReactSettingsExpanded &&
                isReactListExpanded) { }

                if (skinnedMRenList == null) { skinnedMRenList = new List<SkinnedMeshRenderer>(2); }
                if (skinnedMRenList.Count < 1)
                {
                    GetSkinnedMeshRenderers(skinnedMRenList);
                }

                if (blendShapeList == null) { blendShapeList = new List<S3DBlendShape>(10); }

                if (skinnedMRenList.Count > 0 && blendShapeList.Count < 1)
                {
                    GetBlendShapesFromModel(blendShapeList);
                }

                if (isNonS3DController && GetNonS3DAnimator() == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("StickyShapeModule could not find an animator on the " + name + " non-S3D character.");
                    #endif
                }

                isBlinkingEnabled = false;

                s3dRandom = new S3DRandom();
                s3dRandom.SetSeed(GetInstanceID());

                isSpeechPlaying = false;

                ReinitialiseEmotions();
                ReinitialisePhonemes();

                isInitialised = true;

                bool hasChanged;
                ValidateBlendShapes(out hasChanged);

                ReinitialiseEyes();
                RefreshBlink();

                ReinitialiseReactions();

                if (blink.isEnableOnInitialise) { StartBlinking(); }
                if (isEyeMoveEnableOnInit) { StartEyeMovement(); }

                if (isReactEnableOnInit)
                {
                    if (reactNoNotifyDuration > 0f)
                    {
                        DisableProximityCollider();
                        Invoke("StartReacting", reactNoNotifyDuration);
                    }
                    else { StartReacting(); }
                }
                else
                {
                    DisableProximityCollider();
                }
            }
        }

        /// <summary>
        /// Attempt to pause the module
        /// </summary>
        public void PauseShapes()
        {
            if (isInitialised)
            {
                PauseOrUnPause(true);
            }
        }

        /// <summary>
        /// Attempt to reinitialise the eyes and validate them.
        /// </summary>
        public void ReinitialiseEyes()
        {
            bool canValidateEyes = false;

            if (!isInitialised && !isNonS3DController && !gameObject.TryGetComponent(out stickyControlModule))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR StickyShapesModule.ReinitialiseEyes - could not find attached StickyControlModule on " + name);
                #endif
            }
            else if (!isNonS3DController && stickyControlModule.defaultAnimator == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR StickyShapesModule.ReinitialiseEyes - could not find the Animator on " + name + ". Check the Animate tab on the StickyControlModule.");
                #endif
            }
            else if (isNonS3DController && ((isInitialised && nonS3DAnimator == null) || (!isInitialised && GetNonS3DAnimator() == null)))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR StickyShapesModule.ReinitialiseEyes - could not find the Animator on " + name + ". Add an Animator or call SetIsNonS3DController(..) in your code.");
                #endif
            }
            else
            {
                canValidateEyes = true;
            }

            if (leftEye == null) { leftEye = new S3DEye(); }
            if (rightEye == null) { rightEye = new S3DEye(); }

            if (canValidateEyes)
            {
                ValidateEye(leftEye, HumanBodyBones.LeftEye);
                ValidateEye(rightEye, HumanBodyBones.RightEye);
            }

            // Reset temp values
            leftEye.lastRotLS = leftEye.isTfrmValid ? leftEye.eyeTfrm.localRotation : Quaternion.identity;
            rightEye.lastRotLS = rightEye.isTfrmValid ? rightEye.eyeTfrm.localRotation : Quaternion.identity;
        }

        /// <summary>
        /// Set if this component is attached to a non-Sticky3D Controller.
        /// i.e., it is using a 3rd party character controller.
        /// </summary>
        /// <param name="isNotUsingS3DController"></param>
        public void SetIsNonS3DController (bool isNotAttached)
        {
            isNonS3DController = isNotAttached;
        }

        /// <summary>
        /// Attempt to set the animator for this humanoid character.
        /// You might need to set this manually if the Animator component
        /// is not on the same gameobject as the StickyShapesModule.
        /// </summary>
        /// <param name="_animator"></param>
        public void SetNonS3DAnimator (Animator _animator)
        {
            if (isNonS3DController)
            {
                if (_animator != null)
                {
                    if (_animator.transform.IsChildOf(transform))
                    {
                        nonS3DAnimator = _animator;
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("[ERROR] StickyShapesModule.SetNon3DAnimator " + _animator.name + " animator must be a child of " + gameObject.name); }
                    #endif
                }
                else { nonS3DAnimator = null; }
            }
        }

        /// <summary>
        /// Attempt to unpause the module
        /// </summary>
        public void UnpauseShapes()
        {
            if (isInitialised)
            {
                PauseOrUnPause(false);
            }
        }

        /// <summary>
        /// Attempt to validate an eye bone on a humanoid character.
        /// </summary>
        /// <param name="eye"></param>
        /// <param name="eyeBone"></param>
        public void ValidateEye (S3DEye eye, HumanBodyBones eyeBone)
        {
            if (!isNonS3DController)
            {
                if (eye.eyeTfrm == null)
                {
                    eye.eyeTfrm = stickyControlModule.GetBoneTransform(eyeBone);
                }
                eye.isTfrmValid = eye.eyeTfrm != null && eye.eyeTfrm.IsChildOf(stickyControlModule.transform);
            }
            else
            {
                

                if (nonS3DAnimator != null && eye.eyeTfrm == null)
                {
                    eye.eyeTfrm = nonS3DAnimator.GetBoneTransform(eyeBone);
                }
                eye.isTfrmValid = eye.eyeTfrm != null && eye.eyeTfrm.IsChildOf(transform);
            }
        }

        #endregion

        #region Public API Methods - Blendshapes

        /// <summary>
        /// Attempt to get a blendshape from the current list
        /// </summary>
        /// <param name="blendShapeId"></param>
        /// <returns></returns>
        public S3DBlendShape GetBlendShape (int blendShapeId)
        {
            S3DBlendShape blendShape = null;

            int numBShapes = blendShapeList.Count;

            for (int bsIdx = 0; bsIdx < numBShapes; bsIdx++)
            {
                if (blendShapeList[bsIdx].guidHash == blendShapeId)
                {
                    blendShape = blendShapeList[bsIdx];
                    break;
                }
            }

            return blendShape;
        }

        /// <summary>
        /// Attempt to get the BlendShapeId by supplying a hashed name
        /// of a blendshape.
        /// </summary>
        /// <param name="blendShapeNameHash"></param>
        /// <returns></returns>
        public int GetBlendShapeIdByNameHash (int blendShapeNameHash)
        {
            int blendShapeId = 0;

            int numBShapes = blendShapeList.Count;

            for (int bsIdx = 0; bsIdx < numBShapes; bsIdx++)
            {
                if (blendShapeList[bsIdx].blendShapeNameHash == blendShapeNameHash)
                {
                    blendShapeId = blendShapeList[bsIdx].guidHash;
                    break;
                }
            }

            return blendShapeId;
        }

        /// <summary>
        /// Attempt to find the unique BlendShapeId that matches the blendshape name.
        /// </summary>
        /// <param name="blendShapeName"></param>
        /// <returns></returns>
        public int GetBlendShapeId (string blendShapeName)
        {
            int blendShapeId = S3DEmotion.NoID;

            int numBlendShapes = blendShapeList.Count;

            if (!string.IsNullOrEmpty(blendShapeName))
            {
                int nameHash = S3DMath.GetHashCode(blendShapeName);

                for (int bsIdx = 0; bsIdx < numBlendShapes; bsIdx++)
                {
                    S3DBlendShape blendShape = blendShapeList[bsIdx];

                    if (blendShapeList[bsIdx].blendShapeNameHash == nameHash)
                    {
                        blendShapeId = blendShape.guidHash;
                    }
                }
            }

            return blendShapeId;
        }

        /// <summary>
        /// Attempt to get the blendshape hashed name using the blendShapeId.
        /// If not, return 0.
        /// </summary>
        /// <param name="blendShapeId"></param>
        /// <returns></returns>
        public int GetBlendShapeNameHash (int blendShapeId)
        {
            S3DBlendShape blendShape = GetBlendShape(blendShapeId);

            return blendShape == null ? 0 : blendShape.blendShapeNameHash;
        }

        /// <summary>
        /// Attempt to populate the list with blendshapes from the model.
        /// </summary>
        /// <param name="shapeList"></param>
        /// <returns></returns>
        public bool GetBlendShapesFromModel (List<S3DBlendShape> shapeList)
        {
            int numSMRen = skinnedMRenList.Count;

            if (shapeList == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetBlendShapesFromModel on " + name + " expects an empty list");
                #endif
                return false;
            }
            else if (numSMRen < 1)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetBlendShapesFromModel on " + name + " - there are no skinned mesh renderers. See the General tab or call GetSkinnedMeshRenderers().");
                #endif
                return false;
            }
            else
            {
                shapeList.Clear();

                // Loop through the skinned mesh renderers that contain blendshapes
                for (int sIdx = 0; sIdx < numSMRen; sIdx++)
                {
                    SkinnedMeshRenderer sMRen = skinnedMRenList[sIdx];

                    if (sMRen != null)
                    {
                        Mesh mesh = sMRen.sharedMesh;

                        if (mesh != null && mesh.blendShapeCount > 0)
                        {
                            int numBShapes = mesh.blendShapeCount;

                            for (int bsIdx=0; bsIdx < numBShapes; bsIdx++)
                            {
                                shapeList.Add(new S3DBlendShape() { skinnedMeshRenderer = sMRen, blendShapeIndex = bsIdx, blendShapeNameHash = S3DMath.GetHashCode(mesh.GetBlendShapeName(bsIdx)), isNameMatched = true });
                            }
                        }
                    }
                }
                
                return true;
            }
        }

        /// <summary>
        /// Attempt to populate the list with blendshape names from the model.
        /// </summary>
        /// <param name="nameList"></param>
        /// <returns></returns>
        public bool GetBlendShapeNamesFromModel (List<string> nameList)
        {
            int numSMRen = skinnedMRenList.Count;

            if (nameList == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetBlendShapeNamesFromModel on " + name + " expects an empty list");
                #endif
                return false;
            }
            else if (numSMRen < 1)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetBlendShapeNamesFromModel on " + name + " - there are no skinned mesh renderers. See the General tab or call GetSkinnedMeshRenderers().");
                #endif
                return false;
            }
            else
            {
                nameList.Clear();

                // Loop through the skinned mesh renderers that contain blendshapes
                for (int sIdx = 0; sIdx < numSMRen; sIdx++)
                {
                    SkinnedMeshRenderer sMRen = skinnedMRenList[sIdx];

                    if (sMRen != null)
                    {
                        Mesh mesh = sMRen.sharedMesh;

                        if (mesh != null && mesh.blendShapeCount > 0)
                        {
                            int numBShapes = mesh.blendShapeCount;

                            for (int bsIdx=0; bsIdx < numBShapes; bsIdx++)
                            {
                                nameList.Add(mesh.GetBlendShapeName(bsIdx));
                            }
                        }
                    }
                }
                
                return true;
            }
        }

        /// <summary>
        /// Attempt to validate the list of blendshapes against the ones on the model.
        /// This includes checking that the hashed names match the blendshape names on the model.
        /// The hasChanged parm will be true if any of the S3DBlendShapes are changed
        /// as a result of the validation.
        /// </summary>
        /// <param name="hasChanged"></param>
        public void ValidateBlendShapes (out bool hasChanged)
        {
            hasChanged = false;
            int numBShapes = blendShapeList.Count;
            numValidBlendShapes = 0;

            for (int bsIdx = 0; bsIdx < numBShapes; bsIdx++)
            {
                S3DBlendShape blendShape = blendShapeList[bsIdx];

                Mesh mesh = blendShape.skinnedMeshRenderer == null ? null : blendShape.skinnedMeshRenderer.sharedMesh;

                if (mesh != null && mesh.blendShapeCount > blendShape.blendShapeIndex)
                {
                    string modelBlendShapeName = mesh.GetBlendShapeName(blendShape.blendShapeIndex);

                    bool isMatchingName = S3DMath.GetHashCode(modelBlendShapeName) == blendShape.blendShapeNameHash;

                    if (isMatchingName != blendShape.isNameMatched)
                    {
                        blendShape.isNameMatched = isMatchingName;
                        hasChanged = true;
                    }

                    // Currently the only criteria is that the name matches.
                    if (blendShape.isNameMatched != blendShape.isValid)
                    {
                        blendShape.isValid = isMatchingName;
                        hasChanged = true;
                    }

                    if (blendShape.isValid) { numValidBlendShapes++; }
                }
                else
                {
                    // No mesh found or out of range blendShapeIndex
                    blendShape.isValid = false;
                    hasChanged = true;
                }
            }
        }

        #endregion

        #region Public API Methods - Blink

        /// <summary>
        /// Attempt to set the blink value, in the range 0.0 to 1.0
        /// for all the blendshapes associated with this emotion.
        /// </summary>
        /// <param name="blinkValue"></param>
        public void SetBlinkValue (float blinkValue)
        {
            if (isInitialised)
            {
                // Clamp input value 0.0 to 1.0
                if (blinkValue < 0f) { blinkValue = 0f; }
                else if (blinkValue > 1f) { blinkValue = 1f; }

                // Loop through all the S3DEmotionShapes for this blink emotion,
                // and assign the value proportionally to the MaxWeight of that shape.
                for (int bsIdx = 0; bsIdx < blink.numShapes; bsIdx++)
                {
                    S3DEmotionShape emotionShape = blink.emotionShapeList[bsIdx];

                    // Does the emotionShape have a corresponding S3DBlendShape?
                    if (emotionShape.isSynced)
                    {
                        S3DBlendShape blendShape = GetBlendShape(emotionShape.s3dBlendShapeId);

                        if (blendShape.isValid)
                        {
                            //Debug.Log("[DEBUG] emotionShape maxWeight: " + emotionShape.maxWeight + " blendshape index: " + blendShape.blendShapeIndex);

                            blendShape.skinnedMeshRenderer.SetBlendShapeWeight(blendShape.blendShapeIndex, blinkValue * emotionShape.maxWeight);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to start blinking
        /// </summary>
        public void StartBlinking()
        {
            if (isInitialised)
            {
                EnableOrDisableBlinking(true);
            }
        }

        /// <summary>
        /// Attempt to stop blinking
        /// </summary>
        public void StopBlinking()
        {
            if (isInitialised)
            {
                EnableOrDisableBlinking(false);
            }
        }

        /// <summary>
        /// Refresh validation state of blink emotions and blendshapes.
        /// Call after making any changes to the emotions or blendshapes
        /// associate with blinking.
        /// </summary>
        public void RefreshBlink()
        {
            if (blink == null) { blink = new S3DBlink(); }

            blink.numShapes = blink.emotionShapeList.Count;

            for (int bsIdx = 0; bsIdx < blink.numShapes; bsIdx++)
            {
                S3DEmotionShape emotionShape = blink.emotionShapeList[bsIdx];

                S3DBlendShape blendShape = GetBlendShape(emotionShape.s3dBlendShapeId);

                if (blendShape == null || !blendShape.isNameMatched)
                {
                    emotionShape.isSynced = false;
                }
            }
        }

        /// <summary>
        /// Attempt to resync all blink blendshapes with the blendshapes from those
        /// defined on the BlendShapes tab.
        /// </summary>
        public void ResyncBlinkShapes()
        {
            if (!isInitialised) { RefreshBlink(); }

            int blendShapeId = 0;

            for (int bsIdx = 0; bsIdx < blink.numShapes; bsIdx++)
            {
                S3DEmotionShape emotionShape = blink.emotionShapeList[bsIdx];

                if (emotionShape != null)
                {
                    // Get blendshape hashed name from the S3DBlendShape being referenced from this emotion shape.
                    int nameHash = GetBlendShapeNameHash(emotionShape.s3dBlendShapeId);

                    if (nameHash == 0)
                    {
                        // The S3DBlendShape guidHash doesn't exist, so attempt to find with a matching blendshape name
                        blendShapeId = GetBlendShapeIdByNameHash(emotionShape.blendShapeNameHash);

                        emotionShape.isSynced = blendShapeId != 0;

                        if (emotionShape.isSynced)
                        {
                            emotionShape.s3dBlendShapeId = blendShapeId;
                        }
                    }
                    else
                    {
                        // Found the blendshape. Does the nameHash in the emotion match the S3DBlendShape?
                        emotionShape.isSynced = emotionShape.blendShapeNameHash == nameHash;
                    }
                }
            }
        }

        #endregion

        #region Public API Methods - Eye Movement

        /// <summary>
        /// Attempt to start eye movement
        /// </summary>
        public void StartEyeMovement()
        {
            if (isInitialised)
            {
                EnableOrDisableEyeMove(true);
            }
        }

        /// <summary>
        /// Attempt to stop eye movement
        /// </summary>
        public void StopEyeMovement()
        {
            if (isInitialised)
            {
                EnableOrDisableEyeMove(false);
            }
        }

        #endregion

        #region Public API Methods - Emotions

        /// <summary>
        /// Attempt to add a new emotion to the list
        /// </summary>
        /// <param name="emotion"></param>
        public void AddEmotion (S3DEmotion emotion)
        {
            if (!isInitialised && emotionList == null) { emotionList = new List<S3DEmotion>(2); }

            if (isSpeechPlaying) { StopSpeechAudio(currentSpeechAudio); }

            emotionList.Add(emotion);

            ReinitialiseEmotions();
        }

        /// <summary>
        /// Get an emotion given the emotionId or the guidHash.
        /// </summary>
        /// <param name="emotionId"></param>
        /// <returns></returns>
        public S3DEmotion GetEmotion (int emotionId)
        {
            S3DEmotion emotion = null;

            int _numEmotions = isInitialised ? numEmotions : emotionList.Count;

            for (int emIdx = 0; emIdx < _numEmotions; emIdx++)
            {
                if (emotionList[emIdx].guidHash == emotionId)
                {
                    emotion = emotionList[emIdx];
                    break;
                }
            }

            return emotion;
        }

        /// <summary>
        /// Get an emotion given a reactEmotion.
        /// </summary>
        /// <param name="reactEmotion"></param>
        /// <returns></returns>
        public S3DEmotion GetEmotion (S3DReactEmotion reactEmotion)
        {
            return reactEmotion == null ? null : GetEmotion(reactEmotion.s3dEmotionId);
        }

        /// <summary>
        /// Attempt to get an emotion using the emotionName.
        /// Use sparingly as it could impact GC.
        /// </summary>
        /// <param name="emotionName"></param>
        /// <returns></returns>
        public S3DEmotion GetEmotionByName (string emotionName)
        {
            S3DEmotion emotion = null;
            if (!isInitialised) { numEmotions = emotionList.Count; }

            if (!string.IsNullOrEmpty(emotionName))
            {
                for (int emIdx = 0; emIdx < numEmotions; emIdx++)
                {
                    if (emotionList[emIdx].emotionName == emotionName)
                    {
                        emotion = emotionList[emIdx];
                        break;
                    }
                }
            }

            return emotion;
        }

        /// <summary>
        /// Attempt to get an emotion according the numeric order
        /// it appears in the list on the Emotion tab.
        /// </summary>
        /// <param name="emotionNumber"></param>
        /// <returns></returns>
        public S3DEmotion GetEmotionByNumber (int emotionNumber)
        {
            if (emotionNumber > 0 && emotionNumber <= (isInitialised ? numEmotions : emotionList.Count))
            {
                return emotionList[emotionNumber - 1];
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] StickyShapesModule.GetEmotionByNumber emotionNumber must be between 1 and " + (isInitialised ? numEmotions : emotionList.Count));
                #endif
                return null;
            }
        }

        /// <summary>
        /// Attempt to get the emotion hashed name using the EmotionId.
        /// If not, return 0.
        /// </summary>
        /// <param name="emotionId"></param>
        /// <returns></returns>
        public int GetEmotionNameHash (int emotionId)
        {
            S3DEmotion s3dEmotion = GetEmotion(emotionId);

            return s3dEmotion == null ? 0 : s3dEmotion.GetEmotionNameHash;
        }

        /// <summary>
        /// Attempt to get the EmotionId by supplying a hashed name of a emotion.
        /// If emotionNameHash parameter is 0, will return the EmotionId of the
        /// first emotion without an emotionName.
        /// </summary>
        /// <param name="emotionNameHash"></param>
        /// <returns></returns>
        public int GetEmotionIdByNameHash (int emotionNameHash)
        {
            int emotionId = 0;

            int _numEmotions = isInitialised ? numEmotions : emotionList.Count;

            for (int emIdx = 0; emIdx < _numEmotions; emIdx++)
            {
                if (emotionList[emIdx].GetEmotionNameHash == emotionNameHash)
                {
                    emotionId = emotionList[emIdx].guidHash;
                    break;
                }
            }

            return emotionId;
        }

        /// <summary>
        /// Attempt to populate the list with emotion names from the Emotions tab.
        /// </summary>
        /// <param name="nameList"></param>
        /// <returns></returns>
        public bool GetEmotionNames (List<string> nameList)
        {
            int _numEmotions = isInitialised ? numEmotions : emotionList.Count;

            if (nameList == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetEmotionNames on " + name + " expects an empty list");
                #endif
                return false;
            }
            else if (_numEmotions < 1)
            {
                return false;
            }
            else
            {
                nameList.Clear();

                for (int emIdx = 0; emIdx < _numEmotions; emIdx++)
                {
                    S3DEmotion s3DEmotion = emotionList[emIdx];

                    nameList.Add(string.IsNullOrEmpty(s3DEmotion.emotionName) ? "** No Name **" : s3DEmotion.emotionName);
                }

                return true;
            }
        }

        /// <summary>
        /// Get a subset from the Emotions tab based on their intensity and the type
        /// of character they may be reacting to.
        /// The supplied list must be non-null, otherwise the method will return false.
        /// Any items in the list will be overwritten.
        /// </summary>
        /// <param name="emotionSubsetList"></param>
        /// <returns></returns>
        public bool GetEmotionsByIntensity (List<S3DEmotion> emotionSubsetList, S3DReact.ReactTo reactingTo)
        {
            bool isSuccessful = false;

            if (emotionSubsetList != null)
            {
                emotionSubsetList.Clear();

                int reactToInt = (int)reactingTo;

                if (reactToInt != S3DReact.ReactToNobodyInt)
                {
                    if (!isInitialised) { numEmotions = emotionList == null ? 0 : emotionList.Count; }

                    bool isInclude = false;

                    for (int emIdx = 0; emIdx < numEmotions; emIdx++)
                    {
                        S3DEmotion emotion = emotionList[emIdx];
                        if (emotion != null)
                        {
                            isInclude = false;

                            if (reactToInt == S3DReact.ReactToAnyInt)
                            {
                                isInclude = true;
                            }
                            else if (reactToInt == S3DReact.ReactToFriendlyInt)
                            {
                                isInclude = emotion.IsNeutralorFriendly;
                            }
                            else if (reactToInt == S3DReact.ReactToFriendOnlyInt)
                            {
                                isInclude = emotion.IsFriendly;
                            }
                            else if (reactToInt == S3DReact.ReactToFoeInt)
                            {
                                isInclude = emotion.IsHostile;
                            }
                            else if (reactToInt == S3DReact.ReactToNeutralOnlyInt)
                            {
                                isInclude = emotion.IsNeutral;
                            }

                            if (isInclude) { emotionSubsetList.Add(emotion); }
                        }
                    }
                }

                isSuccessful = true;
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("[ERROR] GetEmotionsByIntensity the subset list supplied was null"); }
            #endif

            return isSuccessful;
        }

        /// <summary>
        /// Get the appropriate hold duration for a Reaction stage.
        /// WARNING: For the sake of performance, assumes the emotion is not null.
        /// </summary>
        /// <param name="emotion"></param>
        /// <param name="reactionStageInt"></param>
        /// <returns></returns>
        public float GetEmotionHoldDuration (S3DEmotion emotion, int reactionStageInt)
        {
            if (reactionStageInt == S3DReact.ReactStageEnterInt) { return emotion.holdEnterDuration; }
            else if (reactionStageInt == S3DReact.ReactStageExitInt) { return emotion.holdExitDuration; }
            else if (reactionStageInt == S3DReact.ReactStageStayInt) { return emotion.holdStayDuration; }
            else { return 0; }
        }

        /// <summary>
        /// Get an enable (in use) emotion, given a reactEmotion.
        /// </summary>
        /// <param name="reactEmotion"></param>
        /// <param name="emotion"></param>
        /// <returns></returns>
        public bool GetEnabledEmotion (S3DReactEmotion reactEmotion, out S3DEmotion emotion)
        {
            emotion = GetEmotion(reactEmotion);

            if (emotion != null && emotion.isEmotionEnabled) { return true; }
            else { emotion = null; return false; }
        }

        /// <summary>
        /// Is the emotion enabled (in use)?
        /// </summary>
        /// <param name="emotion"></param>
        /// <returns></returns>
        public bool IsEmotionEnabled (S3DEmotion emotion)
        {
            return emotion != null && emotion.isEmotionEnabled;
        }

        /// <summary>
        /// Is the emotion enabled (in use) for this reactEmotion?
        /// </summary>
        /// <param name="reactEmotion"></param>
        /// <returns></returns>
        public bool IsEmotionEnabled (S3DReactEmotion reactEmotion)
        {
            return reactEmotion != null && IsEmotionEnabled(GetEmotion(reactEmotion.s3dEmotionId));
        }

        /// <summary>
        /// Play an emotion at the regular speed according to the numeric order
        /// it appears in the list on the Emotions tab.
        /// </summary>
        /// <param name="emotionNumber"></param>
        public void PlayEmotion (int emotionNumber)
        {
            PlayOrStopEmotion(GetEmotionByNumber(emotionNumber), true, 1f);
        }

        /// <summary>
        /// Play an emotion at the regular speed.
        /// </summary>
        /// <param name="emotion"></param>
        public void PlayEmotion (S3DEmotion emotion)
        {
            if (!isShapesPaused && emotion != null)
            {
                PlayOrStopEmotion(emotion, true, 1f);
            }
        }

        /// <summary>
        /// Refresh the list which contains the emotions randomly selected
        /// when speech audio is playing.
        /// Emotions are stored here as the EmotionNumber or 1-based index
        /// in the list of emotions on the Emotions tab.
        /// </summary>
        public void RefreshVOXEmotions()
        {
            voxEmotionIdList.Clear();            

            for (int emIdx = 0; emIdx < numEmotions; emIdx++)
            {
                S3DEmotion emotion = emotionList[emIdx];
                if (emotion != null && emotion.isVoxEmotion && emotion.guidHash != 0)
                {
                    voxEmotionIdList.Add(emIdx+1);
                }
            }
        }

        /// <summary>
        /// Reinitialise the list of emotions. Call this if manually adding to or deleting from
        /// the list of emotions.
        /// </summary>
        public void ReinitialiseEmotions()
        {
            if (emotionList == null) { emotionList = new List<S3DEmotion>(2); }

            numEmotions = emotionList.Count;

            RefreshVOXEmotions();
        }

        /// <summary>
        /// Attempt to resync all emotion shapes with the blendshapes from those
        /// defined on the BlendShapes tab.
        /// </summary>
        public void ResyncEmotions()
        {
            if (!isInitialised) { ReinitialiseEmotions(); }

            int blendShapeId = 0;

            for (int emIdx = 0; emIdx < numEmotions; emIdx++)
            {
                S3DEmotion emotion = emotionList[emIdx];

                if (emotion != null)
                {
                    int numEmotionShapes = emotion.emotionShapeList.Count;

                    for (int esIdx = 0; esIdx < numEmotionShapes; esIdx++)
                    {
                        S3DEmotionShape emotionShape = emotion.emotionShapeList[esIdx];

                        if (emotionShape != null)
                        {
                            // Get blendshape hashed name from the S3DBlendShape being referenced from this emotion shape.
                            int nameHash = GetBlendShapeNameHash(emotionShape.s3dBlendShapeId);

                            if (nameHash == 0)
                            {
                                // The S3DBlendShape guidHash doesn't exist, so attempt to find with a matching blendshape name
                                blendShapeId = GetBlendShapeIdByNameHash(emotionShape.blendShapeNameHash);

                                emotionShape.isSynced = blendShapeId != 0;

                                if (emotionShape.isSynced)
                                {
                                    emotionShape.s3dBlendShapeId = blendShapeId;
                                }
                            }
                            else
                            {
                                // Found the blendshape. Does the nameHash in the emotion match the S3DBlendShape?
                                emotionShape.isSynced = emotionShape.blendShapeNameHash == nameHash;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to start an emotion
        /// </summary>
        /// <param name="emotion"></param>
        public void StartEmotion (S3DEmotion emotion)
        {
            if (!isShapesPaused && emotion != null)
            {
                EnableOrDisableEmotion(emotion, true, false);
            }
        }

        /// <summary>
        /// Attempt to start the first emotion with the given name.
        /// Use sparingly as it could impact GC.
        /// Where possible use StartEmotion(s3dEmotion).
        /// See also GetEmotionByName(emotionName).
        /// </summary>
        /// <param name="emotionName"></param>
        public void StartEmotion (string emotionName)
        {
            StartEmotion(GetEmotionByName(emotionName));
        }

        /// <summary>
        /// Attempt to start an emotion in the numeric order it
        /// appears in the list on the Emotion tab.
        /// </summary>
        /// <param name="emotionNumber"></param>
        public void StartEmotion (int emotionNumber)
        {
            StartEmotion(GetEmotionByNumber(emotionNumber));
        }

        /// <summary>
        /// Attempt to start an emotion instantly in the numeric order it
        /// appears in the list on the Emotion tab.
        /// </summary>
        /// <param name="emotionNumber"></param>
        public void StartEmotionInstantly (int emotionNumber)
        {
            if (!isShapesPaused)
            {
                StartEmotionInstantly(GetEmotionByNumber(emotionNumber));
            }
        }

        /// <summary>
        /// Attempt to start an emotion instantly in the numeric order it
        /// appears in the list on the Emotion tab.
        /// </summary>
        /// <param name="emotion"></param>
        public void StartEmotionInstantly (S3DEmotion emotion)
        {
            if (!isShapesPaused)
            {
                if (emotion != null) { EnableOrDisableEmotion(emotion, true, true); }
            }
        }

        /// <summary>
        /// Attempt to stop an emotion
        /// </summary>
        /// <param name="emotion"></param>
        public void StopEmotion (S3DEmotion emotion)
        {
            if (!isShapesPaused && emotion != null)
            {
                EnableOrDisableEmotion(emotion, false, false);
            }
        }

        /// <summary>
        /// Attempt to stop the first emotion with the given name.
        /// Use sparingly as it could impact GC.
        /// Where possible use StopEmotion(s3dEmotion).
        /// See also GetEmotionByName(emotionName).
        /// </summary>
        /// <param name="emotionName"></param>
        public void StopEmotion (string emotionName)
        {
            StopEmotion(GetEmotionByName(emotionName));
        }

        /// <summary>
        /// Attempt to stop an emotion in the numeric order it
        /// appears in the list on the Emotion tab.
        /// </summary>
        /// <param name="emotionNumber"></param>
        public void StopEmotion(int emotionNumber)
        {
            StopEmotion(GetEmotionByNumber(emotionNumber));
        }

        /// <summary>
        /// Attempt to stop an emotion instantly.
        /// </summary>
        /// <param name="emotion"></param>
        public void StopEmotionInstantly (S3DEmotion emotion)
        {
            if (!isShapesPaused)
            {
                if (emotion != null) { EnableOrDisableEmotion(emotion, false, true); }
            }
        }

        /// <summary>
        /// Attempt to stop an emotion instantly in the numeric order it
        /// appears in the list on the Emotion tab.
        /// </summary>
        /// <param name="emotionNumber"></param>
        public void StopEmotionInstantly (int emotionNumber)
        {
            if (!isShapesPaused)
            {
                StopEmotionInstantly(GetEmotionByNumber(emotionNumber));
            }           
        }

        #endregion

        #region Public API Methods - Phonemes

        /// <summary>
        /// Add a phoneme to the list on the Phoneme tab.
        /// </summary>
        /// <param name="phoneme"></param>
        public void AddPhoneme (S3DPhoneme phoneme)
        {
            if (phoneme != null)
            {
                phonemeList.Add(phoneme);
                ReinitialisePhonemes();
            }
        }

        /// <summary>
        /// Get the current audioSource used to play the speech audioclips
        /// </summary>
        /// <returns></returns>
        public AudioSource GetPhonemeAudioSource()
        {
            return phonemeAudioSource;
        }

        /// <summary>
        /// Attempt to get a phoneme using the phonemeName.
        /// Use sparingly as it could impact GC.
        /// </summary>
        /// <param name="phonemeName"></param>
        /// <returns></returns>
        public S3DPhoneme GetPhonemeByName (string phonemeName)
        {
            S3DPhoneme phoneme = null;
            int numPhonemes = phonemeList.Count;

            if (!string.IsNullOrEmpty(phonemeName))
            {
                for (int phIdx = 0; phIdx < numPhonemes; phIdx++)
                {
                    if (phonemeList[phIdx].phonemeName == phonemeName)
                    {
                        phoneme = phonemeList[phIdx];
                        break;
                    }
                }
            }

            return phoneme;
        }

        /// <summary>
        /// Attempt to get a phoneme according to the numeric order
        /// it appears in the list on the Phonetics tab.
        /// </summary>
        /// <param name="phonemeNumber"></param>
        /// <returns></returns>
        public S3DPhoneme GetPhonemeByNumber (int phonemeNumber)
        {
            if (phonemeNumber > 0 && phonemeNumber <= phonemeList.Count)
            {
                return phonemeList[phonemeNumber - 1];
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] StickyShapesModule.GetPhonemeByNumber phonemeNumber must be between 1 and " + phonemeList.Count);
                #endif
                return null;
            }
        }

        /// <summary>
        /// Get a playing (in use) speechAudio, given a reactSpeechAudio.
        /// if it is found, but no longer playing. Set the currentSpeechAudio to null and return false.
        /// </summary>
        /// <param name="reactSpeechAudio"></param>
        /// <param name="speechAudio"></param>
        /// <returns></returns>
        public bool GetPlayingSpeechAudio (S3DReactSpeechAudio reactSpeechAudio, out S3DSpeechAudio speechAudio)
        {
            speechAudio = GetSpeechAudio(reactSpeechAudio);

            if (speechAudio != null && currentSpeechAudio != null && speechAudio.guidHash == currentSpeechAudio.guidHash)
            {
                if (speechAudio.IsValid() && phonemeAudioSource != null && phonemeAudioSource.isActiveAndEnabled && phonemeAudioSource.isPlaying)
                {
                    return true;
                }
                else
                {
                    speechAudio = null;
                    currentSpeechAudio = null;
                    return false;
                }
            }
            else { speechAudio = null; return false; }
        }

        /// <summary>
        /// Attempt to get a speechAudio given the Id (guidHash).
        /// </summary>
        /// <param name="speechAudioId"></param>
        /// <returns></returns>
        public S3DSpeechAudio GetSpeechAudio (int speechAudioId)
        {
            S3DSpeechAudio speechAudio = null;

            if (speechAudioId != 0)
            {
                int _numSpeechAudios = speechAudioList.Count;

                for (int saIdx = 0; saIdx < _numSpeechAudios; saIdx++)
                {
                    S3DSpeechAudio _speechAudio = speechAudioList[saIdx];

                    if (_speechAudio != null && _speechAudio.guidHash == speechAudioId)
                    {
                        speechAudio = _speechAudio;
                        break;
                    }
                }
            }

            return speechAudio;
        }

        /// <summary>
        /// Attempt to get a speechAudio give a S3DReactSpeechAudio.
        /// </summary>
        /// <param name="reactSpeechAudio"></param>
        /// <returns></returns>
        public S3DSpeechAudio GetSpeechAudio (S3DReactSpeechAudio reactSpeechAudio)
        {
            if (reactSpeechAudio != null)
            {
                return GetSpeechAudio(reactSpeechAudio.s3dSpeechAudioId);
            }
            else { return null; }
        }

        /// <summary>
        /// Attempt to get a speechAudio according to the numeric order
        /// it appears in the list on the Phonetics tab.
        /// </summary>
        /// <param name="speechAudioNumber"></param>
        /// <returns></returns>
        public S3DSpeechAudio GetSpeechAudioByNumber (int speechAudioNumber)
        {
            if (speechAudioNumber > 0 && speechAudioNumber <= speechAudioList.Count)
            {
                return speechAudioList[speechAudioNumber - 1];
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] StickyShapesModule.GetSpeechAudioByNumber speechAudioNumber must be between 1 and " + speechAudioList.Count);
                #endif
                return null;
            }
        }

        /// <summary>
        /// Attempt to get the SpeechAudioId by supplying a hashed name of a speechAudio clip.
        /// If clipNameHash parameter is 0, will return the SpeechAudioId of the
        /// first speechAudio without an audio clip.
        /// </summary>
        /// <param name="clipNameHash"></param>
        /// <returns></returns>
        public int GetSpeechAudioIdByNameHash (int clipNameHash)
        {
            int speechAudioId = 0;

            int _numSpeechAudios = speechAudioList.Count;

            for (int saIdx = 0; saIdx < _numSpeechAudios; saIdx++)
            {
                if (speechAudioList[saIdx].GetClipNameHash == clipNameHash)
                {
                    speechAudioId = speechAudioList[saIdx].guidHash;
                    break;
                }
            }

            return speechAudioId;
        }

        /// <summary>
        /// Attempt to populate the list with speech audio names from the Phonemes tab.
        /// </summary>
        /// <param name="nameList"></param>
        /// <returns></returns>
        public bool GetSpeechAudioNames (List<string> nameList)
        {
            int _numSpeechAudios = speechAudioList.Count;

            if (nameList == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetSpeechAudioNames on " + name + " expects an empty list");
                #endif
                return false;
            }
            else if (_numSpeechAudios < 1)
            {
                return false;
            }
            else
            {
                nameList.Clear();

                for (int saIdx = 0; saIdx < _numSpeechAudios; saIdx++)
                {
                    //S3DSpeechAudio speechAudio = speechAudioList[saIdx];
                    //nameList.Add(speechAudio.audioClip == null ? "** No Clip **" : speechAudio.audioClip.name);

                    nameList.Add(speechAudioList[saIdx].GetClipName);                    
                }

                return true;
            }
        }


        /// <summary>
        /// Play a phoneme at the regular speed according to the numeric order
        /// it appears in the list on the Phonetics tab.
        /// </summary>
        /// <param name="phonemeNumber"></param>
        public void PlayPhoneme (int phonemeNumber)
        {
            PlayOrStopPhoneme(GetPhonemeByNumber(phonemeNumber), true, 1f);
        }

        /// <summary>
        /// Play a phoneme at the regular speed.
        /// </summary>
        /// <param name="phoneme"></param>
        public void PlayPhoneme (S3DPhoneme phoneme)
        {
            if (!isShapesPaused && phoneme != null)
            {
                PlayOrStopPhoneme(phoneme, true, 1f);
            }
        }

        /// <summary>
        /// Attempt to play a speech audio clip (along with the phonemes and/or emotions)
        /// in the numeric order it appears in the list on the Phonetics tab.
        /// </summary>
        /// <param name="phonemeNumber"></param>
        public void PlaySpeechAudio (int speechAudioNumber)
        {
            PlaySpeechAudio(GetSpeechAudioByNumber(speechAudioNumber));
        }

        /// <summary>
        /// Attempt to play a speech audio clip along with the phonemes and/or emotions.
        /// </summary>
        /// <param name="speechAudio"></param>
        public void PlaySpeechAudio (S3DSpeechAudio speechAudio)
        {
            if (!isShapesPaused && speechAudio != null)
            {
                PlayOrStopSpeechAudio(speechAudio, true, true);
            }
        }

        /// <summary>
        /// Reinitialise the list of Phonemes.
        /// </summary>
        public void ReinitialisePhonemes()
        {
            if (phonemeList == null) { phonemeList = new List<S3DPhoneme>(10); }

            numPhonemes = phonemeList.Count;
        }

        /// <summary>
        /// Attempt to resync all phoneme shapes with the blendshapes from those
        /// defined on the BlendShapes tab.
        /// </summary>
        public void ResyncPhonemes()
        {
            int numPhonemes = phonemeList.Count;
            int blendShapeId = 0;

            for (int phIdx = 0; phIdx < numPhonemes; phIdx++)
            {
                S3DPhoneme phoneme = phonemeList[phIdx];

                if (phoneme != null)
                {
                    int numEmotionShapes = phoneme.emotionShapeList.Count;

                    for (int esIdx = 0; esIdx < numEmotionShapes; esIdx++)
                    {
                        S3DEmotionShape emotionShape = phoneme.emotionShapeList[esIdx];

                        if (emotionShape != null)
                        {
                            // Get blendshape hashed name from the S3DBlendShape being referenced from this emotion shape in the phoneme.
                            int nameHash = GetBlendShapeNameHash(emotionShape.s3dBlendShapeId);

                            if (nameHash == 0)
                            {
                                // The S3DBlendShape guidHash doesn't exist, so attempt to find with a matching blendshape name
                                blendShapeId = GetBlendShapeIdByNameHash(emotionShape.blendShapeNameHash);

                                emotionShape.isSynced = blendShapeId != 0;

                                if (emotionShape.isSynced)
                                {
                                    emotionShape.s3dBlendShapeId = blendShapeId;
                                }
                            }
                            else
                            {
                                // Found the blendshape. Does the nameHash in the emotion match the S3DBlendShape?
                                emotionShape.isSynced = emotionShape.blendShapeNameHash == nameHash;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set the audioSource used to play the speech audioclips
        /// </summary>
        /// <returns></returns>
        public void SetPhonemeAudioSource (AudioSource newAudioSource)
        {
            phonemeAudioSource = newAudioSource;
        }

        /// <summary>
        /// Attempt to start a phoneme
        /// </summary>
        /// <param name="phoneme"></param>
        public void StartPhoneme (S3DPhoneme phoneme)
        {
            if (!isShapesPaused && phoneme != null)
            {
                EnableOrDisablePhoneme(phoneme, true, false);
            }
        }

        /// <summary>
        /// Attempt to start the first phoneme with the given name.
        /// Use sparingly as it could impact GC.
        /// Where possible use StartPhoneme(s3dPhoneme).
        /// See also GetPhonemeByName(emotionName).
        /// </summary>
        /// <param name="phonemeName"></param>
        public void StartPhoneme (string phonemeName)
        {
            StartPhoneme(GetPhonemeByName(phonemeName));
        }

        /// <summary>
        /// Attempt to start a phoneme in the numeric order it
        /// appears in the list on the Phonetics tab.
        /// </summary>
        /// <param name="phonemeNumber"></param>
        public void StartPhoneme (int phonemeNumber)
        {
            StartPhoneme(GetPhonemeByNumber(phonemeNumber));
        }

        /// <summary>
        /// Attempt to start a phoneme instantly in the numeric order it
        /// appears in the list on the Phonetics tab.
        /// </summary>
        /// <param name="phonemeNumber"></param>
        public void StartPhonemeInstantly (int phonemeNumber)
        {
            if (!isShapesPaused)
            {
                StartPhonemeInstantly(GetPhonemeByNumber(phonemeNumber));
            }
        }

        /// <summary>
        /// Attempt to start a phoneme instantly in the numeric order it
        /// appears in the list on the Phonetics tab.
        /// </summary>
        /// <param name="phoneme"></param>
        public void StartPhonemeInstantly (S3DPhoneme phoneme)
        {
            if (!isShapesPaused)
            {
                if (phoneme != null) { EnableOrDisablePhoneme(phoneme, true, true); }
            }
        }

        /// <summary>
        /// Attempt to stop a phoneme
        /// </summary>
        /// <param name="phoneme"></param>
        public void StopPhoneme (S3DPhoneme phoneme)
        {
            if (!isShapesPaused && phoneme != null)
            {
                EnableOrDisablePhoneme(phoneme, false, false);
            }
        }

        /// <summary>
        /// Attempt to stop the first phoneme with the given name.
        /// Use sparingly as it could impact GC.
        /// Where possible use StopPhoneme(s3dPhoneme).
        /// See also GetPhonemeByName(phonemeName).
        /// </summary>
        /// <param name="phonemeName"></param>
        public void StopPhoneme (string phonemeName)
        {
            StopPhoneme(GetPhonemeByName(phonemeName));
        }

        /// <summary>
        /// Attempt to stop a phoneme in the numeric order it
        /// appears in the list on the Phonetics tab.
        /// </summary>
        /// <param name="phonemeNumber"></param>
        public void StopPhoneme (int phonemeNumber)
        {
            StopPhoneme(GetPhonemeByNumber(phonemeNumber));
        }

        /// <summary>
        /// Attempt to stop a phoneme instantly in the numeric order it
        /// appears in the list on the Phonetics tab.
        /// </summary>
        /// <param name="phoneme"></param>
        public void StopPhonemeInstantly (S3DPhoneme phoneme)
        {
            if (!isShapesPaused)
            {
                if (phoneme != null) { EnableOrDisablePhoneme(phoneme, false, true); }
            }
        }

        /// <summary>
        /// Attempt to stop a phoneme instantly in the numeric order it
        /// appears in the list on the Phonetics tab.
        /// </summary>
        /// <param name="phoneNumber"></param>
        public void StopPhonemeInstantly (int phoneNumber)
        {
            if (!isShapesPaused)
            {
                StopPhonemeInstantly(GetPhonemeByNumber(phoneNumber));
            }
        }

        /// <summary>
        /// Attempt to stop a speech audio clip from playing based on the numeric
        /// order it appears in the list on the Phonetics tab.
        /// </summary>
        /// <param name="speechAudioNumber"></param>
        public void StopSpeechAudio (int speechAudioNumber)
        {
            if (!isShapesPaused)
            {
                StopSpeechAudio(GetSpeechAudioByNumber(speechAudioNumber));
            }
        }

        /// <summary>
        /// Attempt to stop a speech audio from playing.
        /// </summary>
        /// <param name="speechAudio"></param>
        public void StopSpeechAudio (S3DSpeechAudio speechAudio)
        {
            if (!isShapesPaused && speechAudio != null)
            {
                PlayOrStopSpeechAudio(speechAudio, false, false);
            }
        }

        /// <summary>
        /// Attempt to stop a speech audio from playing.
        /// </summary>
        /// <param name="speechAudio"></param>
        public void StopSpeechAudioInstantly (S3DSpeechAudio speechAudio)
        {
            if (!isShapesPaused && speechAudio != null)
            {
                PlayOrStopSpeechAudio(speechAudio, false, true);
            }
        }

        #endregion

        #region Public API Methods - Engage (React)

        /// <summary>
        /// A S3D character has been detected nearby entering the proximity area.
        /// Nearby characters must be initialised to be considered.
        /// </summary>
        /// <param name="nearbyCharacter"></param>
        public void CharacterEnter (StickyControlModule nearbyCharacter)
        {
            if (isInitialised && isReactingEnabled && !isShapesPaused && nearbyCharacter != null)
            {
                if (nearbyCharacter.IsInitialised)
                {
                    if (nearbyCharacters.Add(nearbyCharacter))
                    {
                        int friendOrFoe = stickyControlModule.IsFriendOrFoe(nearbyCharacter.factionId);

                        //Debug.Log("[DEBUG] " + nearbyCharacter.name + " is near " + name + " " + S3DUtils.GetFriendOrFoe(friendOrFoe));

                        if (Time.time < 0.01f)
                        {
                            StartCoroutine(ReactToNearbyDelayed(stickyControlModule.StickyID, nearbyCharacter.StickyID, friendOrFoe, S3DReact.ReactStageEnterInt, nearbyCharacter.modelId));
                        }
                        else { ReactToNearby(stickyControlModule.StickyID, nearbyCharacter.StickyID, friendOrFoe, S3DReact.ReactStageEnterInt, nearbyCharacter.modelId); }
                    }
                }
            }
        }

        /// <summary>
        /// A nearby character has departed the proximity area.
        /// Characters must be initialised to be considered.
        /// </summary>
        /// <param name="nearbyCharacter"></param>
        public void CharacterExit (StickyControlModule nearbyCharacter)
        {
            if (isInitialised && nearbyCharacter != null)
            {
                // If the shapes module is initialised, always remove
                // characters when they exit the proximity area.
                if (nearbyCharacters.Remove(nearbyCharacter) && nearbyCharacter.IsInitialised)
                {
                    if (isReactingEnabled && !isShapesPaused)
                    {
                        int friendOrFoe = stickyControlModule.IsFriendOrFoe(nearbyCharacter.factionId);

                        //Debug.Log("[DEBUG] " + nearbyCharacter.name + " is no longer near " + name + " " + S3DUtils.GetFriendOrFoe(friendOrFoe));

                        if (Time.time < 0.01f)
                        {
                            StartCoroutine(ReactToNearbyDelayed(stickyControlModule.StickyID, nearbyCharacter.StickyID, friendOrFoe, S3DReact.ReactStageExitInt, nearbyCharacter.modelId));
                        }
                        else { ReactToNearby(stickyControlModule.StickyID, nearbyCharacter.StickyID, friendOrFoe, S3DReact.ReactStageExitInt, nearbyCharacter.modelId); }
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to add a new reaction to the list
        /// </summary>
        /// <param name="react"></param>
        public void AddReaction (S3DReact react)
        {
            if (!isReactInitialised && reactList == null) { reactList = new List<S3DReact>(2); }

            //if (isSpeechPlaying) { StopSpeechAudio(currentSpeechAudio); }

            reactList.Add(react);

            ReinitialiseReactions();
        }

        /// <summary>
        /// Attempt to get a reaction using the unique guidHash or ID
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public S3DReact GetReaction (int guidHash)
        {
            S3DReact react = null;
            if (!isInitialised) { numReactions = reactList.Count; }

            if (guidHash != 0)
            {
                for (int reIdx = 0; reIdx < numReactions; reIdx++)
                {
                    if (reactList[reIdx].guidHash == guidHash)
                    {
                        react = reactList[reIdx];
                        break;
                    }
                }
            }

            return react;
        }

        /// <summary>
        /// Attempt to get a reaction using the reactName.
        /// Use sparingly as it could impact GC.
        /// </summary>
        /// <param name="reactName"></param>
        /// <returns></returns>
        public S3DReact GetReactionByName (string reactName)
        {
            S3DReact react = null;
            if (!isInitialised) { numReactions = reactList.Count; }

            if (!string.IsNullOrEmpty(reactName))
            {
                for (int reIdx = 0; reIdx < numReactions; reIdx++)
                {
                    if (reactList[reIdx].reactName == reactName)
                    {
                        react = reactList[reIdx];
                        break;
                    }
                }
            }

            return react;
        }

        /// <summary>
        /// Attempt to get a reaction according the numeric order
        /// it appears in the list on the Engage tab under Reactions.
        /// </summary>
        /// <param name="reactNumber"></param>
        /// <returns></returns>
        public S3DReact GetReactionByNumber (int reactNumber)
        {
            if (reactNumber > 0 && reactNumber <= (isReactInitialised ? numReactions : reactList.Count))
            {
                return reactList[reactNumber - 1];
            }
            else { return null; }
        }

        /// <summary>
        /// Attempt to play an enter reaction.
        /// </summary>
        /// <param name="react"></param>
        public void PlayEnterReaction (S3DReact react)
        {
            if (isInitialised && isReactingEnabled && !isShapesPaused && react != null)
            {
                EnableOrDisableReaction(react, true, false, S3DReact.ReactStageEnterInt);
            }
        }

        /// <summary>
        /// Attempt to play the first enter reaction with the given name.
        /// Use sparingly as it could impact GC.
        /// Where possible use PlayEnterReaction(s3dReact).
        /// See also GetReactionByName(reactName).
        /// </summary>
        /// <param name="reactName"></param>
        public void PlayEnterReaction (string reactName)
        {
            PlayEnterReaction(GetReactionByName(reactName));
        }

        /// <summary>
        /// Attempt to play an enter reaction (in the numeric order it
        /// appears in the list of enter reactions on the Engage tab)
        /// when another comes nearby.
        /// </summary>
        /// <param name="reactNumber"></param>
        public void PlayEnterReaction (int reactNumber)
        {
            PlayEnterReaction(GetReactionByNumber(reactNumber));
        }

        /// <summary>
        /// Attempt to play an exit reaction.
        /// </summary>
        /// <param name="react"></param>
        public void PlayExitReaction (S3DReact react)
        {
            if (isInitialised && isReactingEnabled && !isShapesPaused && react != null)
            {
                EnableOrDisableReaction(react, true, false, S3DReact.ReactStageExitInt);
            }
        }

        /// <summary>
        /// Attempt to play the first exit reaction with the given name.
        /// Use sparingly as it could impact GC.
        /// Where possible use PlayExitReaction(s3dReact).
        /// See also GetReactionByName(reactName).
        /// </summary>
        /// <param name="reactName"></param>
        public void PlayExitReaction (string reactName)
        {
            PlayExitReaction(GetReactionByName(reactName));
        }

        /// <summary>
        /// Attempt to play an enter reaction (in the numeric order it
        /// appears in the list of enter reactions on the Engage tab)
        /// when another departs from nearby.
        /// </summary>
        /// <param name="reactNumber"></param>
        public void PlayExitReaction (int reactNumber)
        {
            PlayExitReaction(GetReactionByNumber(reactNumber));
        }

        /// <summary>
        /// Play an emotional reaction
        /// </summary>
        /// <param name="reaction"></param>
        /// <param name="reactEmotion"></param>
        public void PlayReaction (S3DReact reaction, S3DReactEmotion reactEmotion)
        {
            if (!isShapesPaused && reaction != null && reactEmotion != null)
            {
                // This is what starts the fade in/out of the emotion
                PlayOrStopReaction(reaction, reactEmotion, true);
            }
        }

        /// <summary>
        /// Play a speech audio reaction
        /// </summary>
        /// <param name="reaction"></param>
        /// <param name="reactSpeechAudio"></param>
        public void PlayReaction (S3DReact reaction, S3DReactSpeechAudio reactSpeechAudio)
        {
            if (!isShapesPaused && reaction != null && reactSpeechAudio != null)
            {
                // This is what starts the fade in/out of the speech audio
                PlayOrStopReaction(reaction, reactSpeechAudio, true);
            }
        }

        /// <summary>
        /// Prepare the character to start reacting to others in the scene.
        /// See also StartReacting() and StopReacting(..).
        /// </summary>
        public void ReinitialiseReactions()
        {
            if (reactList == null) { reactList = new List<S3DReact>(2); }

            numReactions = reactList.Count;

            if (proximityCollider != null)
            {
                StickyShapesProximity shapesProximity = null;

                // If proximity component doesn't exist at runtime, add it.
                if (!proximityCollider.TryGetComponent(out shapesProximity))
                {
                    proximityCollider.gameObject.AddComponent(typeof(StickyShapesProximity));

                    Debug.LogWarning("StickyShapesModule.ReinitialiseReact() - Adding missing StickyShapesProximity to " + proximityCollider.name + " on " + name);
                }

                // On S3D characters, ensure weapons ignore this trigger collider
                if (!isNonS3DController && stickyControlModule != null)
                {
                    stickyControlModule.RegisterWeaponNonHitCollider(proximityCollider);
                }

                // Initialise the proximity script that is attached to the child trigger collider
                if (shapesProximity != null)
                {
                    shapesProximity.Initialise(this);
                }

                for (int reIdx = 0; reIdx < numReactions; reIdx++)
                {
                    S3DReact reaction = reactList[reIdx];
                    if (reaction != null)
                    {
                        // Populate list based on emotion intensities from Emotion tab
                        // and the React To setting.
                        if (reaction.isAutoEnterEmotions)
                        {
                            RefreshAutoEmotionList(reaction, S3DReact.ReactionStage.Enter);
                        }

                        reaction.CacheData();
                    }
                }

                isReactInitialised = shapesProximity != null;
            }
            else
            {
                isReactInitialised = false;
            }
        }

        /// <summary>
        /// Call this when you wish to remove any custom event listeners, like
        /// after creating them in code and then destroying the object.
        /// You could add this to your game play OnDestroy code.
        /// </summary>
        public void RemoveListeners()
        {
            if (isInitialised)
            {
                for (int reIdx = 0; reIdx < numReactions; reIdx++)
                {
                    S3DReact reaction = reactList[reIdx];
                    if (reaction != null) { RemoveListeners(reaction); }
                }
            }
        }

        /// <summary>
        /// Attempt to resync all react emotions with the emotions from those
        /// defined on the Emotions tab.
        /// </summary>
        public void ResyncReactions()
        {
            if (!isInitialised)
            {
               ReinitialiseEmotions();

                if (reactList == null) { reactList = new List<S3DReact>(2); }
                numReactions = reactList.Count;
            }

            for (int reIdx=0; reIdx < numReactions; reIdx++)
            {
                S3DReact s3dReact = reactList[reIdx];

                if (s3dReact != null)
                {
                    ResyncReactEmotions(s3dReact.enterReactEmotionList);

                }
            }
        }

        /// <summary>
        /// Set the proximity trigger collider for this shapes module.
        /// </summary>
        /// <param name="proxCollider"></param>
        public void SetProximityCollider (Collider proxCollider)
        {
            if (proxCollider == null) { proximityCollider = null; }
            else
            {
                Transform proxTrfm = proxCollider.transform;

                // Collider cannot be a component on same gameobject
                if (proxTrfm.GetHashCode() == transform.GetHashCode() || !proxTrfm.IsChildOf(transform))
                {
                    proximityCollider = null;
                    Debug.LogWarning("[ERROR] The Shapes Module proximity collider must be on a child object of " + gameObject.name);
                }
                // Only update if it has changed
                else if (proxCollider.GetHashCode() != proximityCollider.GetHashCode())
                {
                    proximityCollider = proxCollider;
                }
            }
        }

        /// <summary>
        /// Set the distance the character starts to react to others in the scene
        /// </summary>
        /// <param name="newDistance"></param>
        public void SetReactDistance (float newDistance)
        {
            reactDistance = newDistance;

            if (isReactInitialised || proximityCollider != null)
            {
                System.Type _colType = proximityCollider.GetType();

                if (_colType == typeof(SphereCollider))
                {
                    ((SphereCollider)proximityCollider).radius = reactDistance;
                }
                else if (_colType == typeof(BoxCollider))
                {
                    ((BoxCollider)proximityCollider).size = new Vector3(reactDistance * 2f, reactDistance * 2f, reactDistance * 2f);
                }
                else
                {
                    Debug.LogWarning("[WARNING] StickyShapeModule SetReactDistance - Could not adjust Proxmity Collider on " + name + " as it not a sphere or box collider. Adjust the collider manually");
                }
            }
        }

        /// <summary>
        /// Start reacting to others in the scene
        /// </summary>
        public void StartReacting()
        {
            if (isReactInitialised)
            {
                EnableOrDisableReacting(true, true);
            }
        }

        /// <summary>
        /// Stop reacting to others in the scene
        /// </summary>
        /// <param name="isInstant"></param>
        public void StopReacting (bool isInstant)
        {
            if (isReactInitialised)
            {
                EnableOrDisableReacting(false, isInstant);
            }
        }

        /// <summary>
        /// Attempt to stop a reaction that was triggered when
        /// another came nearby.
        /// </summary>
        /// <param name="react"></param>
        public void StopEnterReaction (S3DReact react)
        {
            if (!isShapesPaused && react != null)
            {
                EnableOrDisableReaction(react, false, false, S3DReact.ReactStageEnterInt);
            }
        }

        /// <summary>
        /// Attempt to stop the first reaction with the given name
        /// that was triggered when another came nearby.
        /// Use sparingly as it could impact GC.
        /// Where possible use StopEnterReaction(s3dReact).
        /// See also GetReactionByName(reactName).
        /// </summary>
        /// <param name="reactName"></param>
        public void StopEnterReaction (string reactName)
        {
            StopEnterReaction(GetReactionByName(reactName));
        }

        /// <summary>
        /// Attempt to stop a reaction (in the numeric order it
        /// appears in the list of reactions on the Engage tab)
        /// that was triggered when another came nearby.
        /// </summary>
        /// <param name="reactNumber"></param>
        public void StopEnterReaction (int reactNumber)
        {
            StopEnterReaction(GetReactionByNumber(reactNumber));
        }

        /// <summary>
        /// Attempt to stop a reaction instantly that was triggered
        /// when another came nearby.
        /// </summary>
        /// <param name="react"></param>
        public void StopEnterReactionInstantly (S3DReact react)
        {
            if (isInitialised && isReactingEnabled && !isShapesPaused && react != null)
            {
                EnableOrDisableReaction(react, false, true, S3DReact.ReactStageEnterInt);
            }
        }

        /// <summary>
        /// Attempt to stop a reaction instantly (in the numeric order it
        /// appears in the list of reactions on the Engage tab) that was
        /// triggered by another coming nearby.
        /// </summary>
        /// <param name="reactNumber"></param>
        public void StopEnterReactionInstantly (int reactNumber)
        {
            if (!isShapesPaused)
            {
                StopEnterReactionInstantly(GetReactionByNumber(reactNumber));
            }           
        }

        /// <summary>
        /// Attempt to stop a reaction that was triggered when
        /// another departed from nearby.
        /// </summary>
        /// <param name="react"></param>
        public void StopExitReaction (S3DReact react)
        {
            if (!isShapesPaused && react != null)
            {
                EnableOrDisableReaction(react, false, false, S3DReact.ReactStageExitInt);
            }
        }

        /// <summary>
        /// Attempt to stop the first reaction with the given name
        /// that was triggered when another departed from nearby.
        /// Use sparingly as it could impact GC.
        /// Where possible use StopExitReaction(s3dReact).
        /// See also GetReactionByName(reactName).
        /// </summary>
        /// <param name="reactName"></param>
        public void StopExitReaction (string reactName)
        {
            StopExitReaction(GetReactionByName(reactName));
        }

        /// <summary>
        /// Attempt to stop a reaction (in the numeric order it
        /// appears in the list of reactions on the Engage tab)
        /// that was triggered when another departs from nearby.
        /// </summary>
        /// <param name="reactNumber"></param>
        public void StopExitReaction (int reactNumber)
        {
            StopExitReaction(GetReactionByNumber(reactNumber));
        }

        /// <summary>
        /// Attempt to stop a reaction instantly that was triggered
        /// when another departs from nearby.
        /// </summary>
        /// <param name="react"></param>
        public void StopExitReactionInstantly (S3DReact react)
        {
            if (isInitialised && isReactingEnabled && !isShapesPaused && react != null)
            {
                EnableOrDisableReaction(react, false, true, S3DReact.ReactStageExitInt);
            }
        }

        /// <summary>
        /// Attempt to stop a reaction instantly (in the numeric order it
        /// appears in the list of reactions on the Engage tab) that was
        /// triggered by another departs from nearby.
        /// </summary>
        /// <param name="reactNumber"></param>
        public void StopExitReactionInstantly (int reactNumber)
        {
            if (!isShapesPaused)
            {
                StopExitReactionInstantly(GetReactionByNumber(reactNumber));
            }           
        }


        #endregion
    }
}