using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Script to rotate a light on the y-axis. If an audiosource (and clip) is attached to the same
    /// gameobject, it will activate and deactivate at the same time as the light.
    /// If including an audiosource, turn off Play On Awake for the audiosource.
    /// Assumes original rotation y is between -359.999 and +359.999
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class BeaconLight : MonoBehaviour
    {
        #region Public Variables
        /// <summary>
        /// If enabled, the InitialiseBeacon() will be called as soon as Awake() runs. This should be disabled if you are
        /// instantiating the beacon through code.
        /// </summary>
        public bool initialiseOnAwake = false;
        // Turn on or off the beacon after initialisation at runtime, call TurnOn(..)
        public bool isOn = false;
        public float rotationsPerMinute = 30f;
        #endregion

        #region Private Variables
        private Light beacon = null;
        private AudioSource audioSource = null;
        private bool isInitialised = false;
        //private Quaternion rotation;
        //private float timer = 0f;
        private float currentRotationAngle = 0f;
        private float originalRotationAngle = 0f;
        private float fadeAudioTimer = 0f;
        private float fadeAudioDuration = 1f;
        private float fadeAudioStartVolume = 1f;
        private AnimationCurve audioFadeOutCurve;
        #endregion

        #region Initialisation Methods

        private void Awake()
        {
            beacon = GetComponent<Light>();

            if (beacon == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("BeaconLight on " + this.name + " could not find an attached Light component");
                #endif
            }
            else
            {
                if (initialiseOnAwake) { InitialiseBeacon(); }
                else { beacon.enabled = false; }
            }
        }

        /// <summary>
        /// Initialise the beacon and get ready to rotate the beacon
        /// </summary>
        public void InitialiseBeacon()
        {
            if (beacon != null)
            {
                beacon.enabled = isOn;

                // Get the original positive rotation angle
                originalRotationAngle = transform.rotation.eulerAngles.y;
                if (originalRotationAngle < 360f) { originalRotationAngle += 360f; }

                currentRotationAngle = originalRotationAngle;

                audioSource = GetComponent<AudioSource>();

                if (audioSource != null && audioSource.clip != null && audioSource.isActiveAndEnabled)
                {
                    if (isOn)
                    {
                        if (!audioSource.isPlaying) { audioSource.Play(); }
                    }
                    else if (audioSource.isPlaying) { audioSource.Stop(); }                    
                }

                audioFadeOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

                isInitialised = true;
            }
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            if (isInitialised && isOn)
            {
                // Based on the rotation speed, how far has the beacon rotated in the last frame?
                // Need to use per frame rotation because rotation speed may have changed

                float rotThisFrame = rotationsPerMinute / 60f * Time.deltaTime;

                // Discard the integral value
                rotThisFrame -= (float)System.Math.Floor(rotThisFrame);

                currentRotationAngle += rotThisFrame * 360f;

                if (currentRotationAngle >= 360f) { currentRotationAngle -= 360f; }

                transform.rotation = Quaternion.Euler(0f, currentRotationAngle, 0f);

                if (fadeAudioTimer > 0f)
                {
                    fadeAudioTimer -= Time.deltaTime;

                    if (fadeAudioTimer > 0f)
                    {
                        audioSource.volume = audioFadeOutCurve.Evaluate(1f - (fadeAudioTimer / fadeAudioDuration));
                    }
                    else if (audioSource != null && audioSource.clip != null && audioSource.isActiveAndEnabled && audioSource.isPlaying)
                    {
                        audioSource.Stop();
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// While the beacon light is still on, fade out the audio
        /// </summary>
        /// <param name="fadeDuration">Duration in seconds</param>
        public void FadeOutAudio (float fadeDuration)
        {
            if (isInitialised && audioSource != null && audioSource.clip != null && audioSource.isActiveAndEnabled && audioSource.isPlaying)
            {
                fadeAudioDuration = fadeDuration;

                fadeAudioTimer = fadeDuration;

                fadeAudioStartVolume = audioSource.volume;
            }           
        }

        /// <summary>
        /// Turn on (or off) the beacon
        /// </summary>
        /// <param name="turnOn"></param>
        public void TurnOn(bool turnOn)
        {
            if (isInitialised && beacon != null)
            {
                isOn = turnOn;

                bool isValidAudioClip = audioSource != null && audioSource.clip != null && audioSource.isActiveAndEnabled;

                // If required , turn off audio before light (should all be done in the same frame... but just incase)
                if (isValidAudioClip && !isOn && audioSource.isPlaying) { audioSource.Stop(); }

                beacon.enabled = turnOn;

                // If required, turn on audio after light
                if (isValidAudioClip && isOn && !audioSource.isPlaying) { audioSource.Play(); }

                // Reset rotation
                currentRotationAngle = originalRotationAngle;

                fadeAudioTimer = 0f;
            }
        }

        #endregion
    }
}