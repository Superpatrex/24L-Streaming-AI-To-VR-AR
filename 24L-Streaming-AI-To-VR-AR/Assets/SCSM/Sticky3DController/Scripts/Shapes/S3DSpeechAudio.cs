using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A speech audio clip used to play phoneme blendshapes
    /// on a character using the StickyShapeModule.
    /// </summary>
    [System.Serializable]
    public class S3DSpeechAudio
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// Unique identifier
        /// </summary>
        public int guidHash;

        /// <summary>
        /// Audio clip that holds the sound of the character speaking
        /// </summary>
        public AudioClip audioClip;

        /// <summary>
        /// Should the emotion immediately start fading out when
        /// there is silence in the audio and isEmotionsIgnoreSilence is false?
        /// Emotions may never reach their maximum set weight, however, it may
        /// look a little more random.
        /// </summary>
        public bool isEmotionFadeOnSilence;

        /// <summary>
        /// The minimum interval between emotions playing when
        /// isEmotionsIgnoreSilence is true.
        /// </summary>
        [Range(0f, 5f)] public float emotionMinInterval;

        /// <summary>
        /// The maximum interval between emotions playing when
        /// isEmotionsIgnoreSilence is true.
        /// </summary>
        [Range(0f, 5f)] public float emotionMaxInterval;

        /// <summary>
        /// Do emotions with isVOXEmotion enabled, play through silent sections of the audio clip?
        /// </summary>
        public bool isEmotionsIgnoreSilence;

        /// <summary>
        /// Volume below this level will be considered as silence
        /// </summary>
        [Range(0f, 0.5f)] public float silenceThreshold;

        /// <summary>
        /// Editor only, is this speech audio clip being previewed on the model?
        /// </summary>
        public bool isPreviewMode;

        public bool showInEditor;

        /// <summary>
        /// The speed to play back the phonemes, relative to
        /// the phoneme timings set on the character.
        /// </summary>
        [Range(0.05f, 5f)] public float speechSpeed;

        /// <summary>
        /// The relative playback volume
        /// </summary>
        [Range(0f, 1f)] public float volume;

        /// <summary>
        /// The timer used to determine the next emotion should be played
        /// when isEmotionsIgnoreSilence is true.
        /// </summary>
        [System.NonSerialized] public float intervalTimer;

        /// <summary>
        /// The guidHash of the S3DReact (if any) that is playing this speechAudio.
        /// Not Set = 0.
        /// </summary>
        [System.NonSerialized] public int playedByReactId;

        #endregion

        #region Public Properties

        /// <summary>
        /// The unique identifier for the this Speech Audio.
        /// </summary>
        public int GetSpeechAudioId { get { return guidHash; } }

        /// <summary>
        /// Get the name, if any, of the audio clip
        /// </summary>
        public string GetClipName { get { return audioClip == null ? "** No Clip **" : audioClip.name; } }

        /// <summary>
        /// Get the hashed value of the audio clip. If no name, will return 0.
        /// </summary>
        public int GetClipNameHash { get { return audioClip == null ? 0 : S3DMath.GetHashCode(audioClip.name); } }

        #endregion

        #region Public Static Variables

        /// <summary>
        /// The minimum speed the phonemes are played at, relative
        /// to the phoneme duration.
        /// </summary>
        public static readonly float MinSpeechSpeed = 0.05f;

        /// <summary>
        /// Used to denote that S3DSpeechAudio or S3DReactSpeechAudio is not set.
        /// </summary>
        public static int NoID = 0;

        #endregion

        #region Protected Variables - General

        protected int numSamples = 0;

        [System.NonSerialized] protected float[] samples;

        #endregion

        #region Constructors

        public S3DSpeechAudio ()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="s3dSpeechAudio"></param>
        public S3DSpeechAudio(S3DSpeechAudio s3dSpeechAudio)
        {
            if (s3dSpeechAudio == null) { SetClassDefaults(); }
            else
            {
                guidHash = s3dSpeechAudio.guidHash;
                showInEditor = s3dSpeechAudio.showInEditor;
                audioClip = s3dSpeechAudio.audioClip;
                speechSpeed = s3dSpeechAudio.speechSpeed;
                volume = s3dSpeechAudio.volume;
                silenceThreshold = s3dSpeechAudio.silenceThreshold;
                emotionMinInterval = s3dSpeechAudio.emotionMinInterval;
                emotionMaxInterval = s3dSpeechAudio.emotionMaxInterval;
                isEmotionsIgnoreSilence = s3dSpeechAudio.isEmotionsIgnoreSilence;
                isEmotionFadeOnSilence = s3dSpeechAudio.isEmotionFadeOnSilence;
                isPreviewMode = false;

                playedByReactId = 0;
            }
        }

        #endregion

        #region Public member methods

        /// <summary>
        /// Attempt to read the audio sample data into a buffer.
        /// </summary>
        /// <returns></returns>
        public bool GetAudioData()
        {
            bool isSuccessful = false;
            numSamples = 0;

            if (IsValid())
            {
                try
                {
                    // Audio must be decompressed.
                    if (audioClip.loadType == AudioClipLoadType.DecompressOnLoad) // && audioClip.loadState == AudioDataLoadState.Loaded)
                    {
                        samples = new float[audioClip.samples * audioClip.channels];
                        audioClip.GetData(samples, 0);
                        numSamples = samples.Length;
                        isSuccessful = numSamples > 0;
                    }
                    else if (audioClip.loadType == AudioClipLoadType.CompressedInMemory)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("[ERROR] S3DSpeechAudio GetAudioData -  cannot read smaples from a compressed file " + audioClip.name);
                        #endif
                    }
                    else if (audioClip.loadType == AudioClipLoadType.Streaming)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("[ERROR] S3DSpeechAudio GetAudioData -  cannot read smaples from a streaming file " + audioClip.name);
                        #endif
                    }
                }
                catch (System.Exception ex)
                {
                    // An exception is NOT raised if GetData(samples) is called on compressed
                    // or streaming audio.
                    // Expose the error in the runtime log - not just in the editor.
                    Debug.LogWarning("[ERROR] S3DSpeechAudio GetAudioData " + ex.Message);
                }
            }

            return isSuccessful;
        }

        /// <summary>
        /// Attempt to get the max volume of all channels in the audio clip at a time in the clip.
        /// </summary>
        /// <param name="offsetTime"></param>
        /// <returns></returns>
        public float GetMaxVolume (float offsetTime)
        {
            float vol = 0f, v;

            if (numSamples > 0)
            {
                int channels = audioClip.channels;
                float pos = offsetTime * audioClip.frequency * channels;

                int sampleIndex = Mathf.RoundToInt(pos);

                if (pos < numSamples)
                {
                    for (int ch = 0; ch < channels; ch++)
                    {
                        v = samples[sampleIndex + ch];

                        // Get the absolute value as the range can be -1.0 to 1.0
                        if (v < 0f) { v = -v; }

                        if (v > vol) { vol = v; }
                    }
                }
            }

            return vol;
        }

        /// <summary>
        /// Attempt to get the volume of the audio clip at a time in the clip.
        /// </summary>
        /// <param name="offsetTime"></param>
        /// <returns></returns>
        public float GetVolume (float offsetTime)
        {
            return GetMaxVolume(offsetTime);
        }

        /// <summary>
        /// Is this speech audio valid?
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return audioClip != null;
        }

        public void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            audioClip = null;
            isPreviewMode = false;
            showInEditor = true;
            speechSpeed = 1f;
            volume = 1f;
            silenceThreshold = 0.1f;
            emotionMinInterval = 1f;
            emotionMaxInterval = 4f;
            isEmotionsIgnoreSilence = true;
            isEmotionFadeOnSilence = false;

            playedByReactId = 0;
        }

        #endregion
    }
}