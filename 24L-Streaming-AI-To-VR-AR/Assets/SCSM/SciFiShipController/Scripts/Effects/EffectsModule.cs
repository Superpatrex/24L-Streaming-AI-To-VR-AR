using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Implement a particle and/or sound effect when something is hit, damaged or destroyed.
    /// This can include multiple child particle systems and/or an audio source attached to the parent gameobject. 
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Object Components/Effects Module")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class EffectsModule : MonoBehaviour
    {
        #region Public Variables and Properties

        /// <summary>
        /// Whether pooling is used when spawning effects objects of this type.
        /// Currently we don't support changing this at runtime.
        /// </summary>
        public bool usePooling = false;
        /// <summary>
        /// The starting size of the pool.
        /// </summary>
        public int minPoolSize = 5;
        /// <summary>
        /// The maximum allowed size of the pool.
        /// </summary>
        public int maxPoolSize = 100;

        /// <summary>
        /// The effects object will be automatically despawned after this amount of time (in seconds) has elapsed.
        /// </summary>
        public float despawnTime = 3f;

        /// <summary>
        /// If an audio source is included, the volume can be optionally set at runtime to the default volume.
        /// </summary>
        [Range(0f, 1f)] public float defaultVolume = 1f;

        /// <summary>
        /// Does this object get parented to another object when activated? If so,
        /// it will be reparented to the pool transform after use.
        /// </summary>
        public bool isReparented = false;

        /// <summary>
        /// Is the effects module enabled to emit particles and play an audio clip?
        /// See also EnableEffects() and DisableEffects().
        /// </summary>
        public bool IsEffectsModuleEnabled { get { return isEffectsModuleEnabled; } }

        ///// <summary>
        ///// The ID number for this effects object prefab (as assigned by the Ship Controller Manager in the scene).
        ///// This is the index in the SSCManager effectsObjectTemplatesList
        ///// [INTERNAL USE ONLY]
        ///// </summary>
        //public int effectsObjectPrefabID;

        #endregion

        #region Private Variables
        private AudioSource audioSource = null;
        private AudioClip audioClip = null;
        private List<ParticleSystem> shurikenParticlesList = null;
        private int numShurikenParticles = 0;
        private bool isEffectsModuleEnabled = true;
        private bool isAudioSourcePaused = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used to determine uniqueness
        /// </summary>
        [System.NonSerialized] internal uint itemSequenceNumber;

        /// <summary>
        /// [INTERNAL ONLY]
        /// If isReparented is true, this is the original parent
        /// transform used in the pooling system.
        /// </summary>
        [System.NonSerialized] internal Transform poolParentTrfm;

        #endregion

        #region Internal Static Variables
        internal static uint nextSequenceNumber = 1;
        internal readonly static string destroyMethodName = "DestroyEffectsObject";
        #endregion

        #region Internal Methods

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Get a reference to the audio source and cache it, if it hasn't already been done.
        /// NOTE: For performance reasons, the audio source must be on the parent gameobject.
        /// </summary>
        /// <returns></returns>
        internal AudioSource GetAudioSource()
        {
            if (audioSource == null) { audioSource = GetComponent<AudioSource>(); }

            return audioSource;
        }

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Get a reference to the audio clip. Cache the audio source and audio clip if they
        /// are not already cached.
        /// </summary>
        /// <returns></returns>
        internal AudioClip GetAudioClip()
        {
            if (audioSource == null) { audioSource = GetComponent<AudioSource>(); }

            // If the audioClip is not defined, attempt to cache it (if it exists)
            if (audioClip == null && audioSource != null) { audioClip = audioSource.clip; }

            return audioClip;
        }

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Replace the existing clip with a new one
        /// </summary>
        /// <param name="newAudioClip"></param>
        internal void SetAudioClip(AudioClip newAudioClip)
        {
            if (audioSource != null)
            {
                audioSource.clip = newAudioClip;
                audioClip = audioSource.clip;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check to see if the EffectsModule has an AudioSource attached.
        /// </summary>
        /// <returns></returns>
        public bool HasAudioSource()
        {
            if (audioSource != null) { return true; }
            else if (itemSequenceNumber != 0u) { return false; }
            else
            {
                return TryGetComponent(out audioSource);
            }
        }

        /// <summary>
        /// Check to see if the EffectsModule has one or more ParticleSystems
        /// </summary>
        /// <returns></returns>
        public bool HasParticleSystems()
        {
            if (numShurikenParticles > 0) { return true; }
            else if (itemSequenceNumber != 0u) { return false; }
            else
            {
                ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();

                return (particleSystems == null ? 0 : particleSystems.Length) > 0f;
            }
        }

        /// <summary>
        /// Initialises the effects object. Play the optional audio clip if there is one.
        /// Returns the unique identifier for this EffectsObject instance.
        /// </summary>
        public uint InitialiseEffectsObject ()
        {
            // Check for an audiosource attached to the same gameobject in the prefab.
            // For performance reasons, don't search child objects.
            if (audioSource == null) { audioSource = GetComponent<AudioSource>(); }

            // If the audioClip is not defined, attempt to cache it (if it exists)
            if (audioClip == null && audioSource != null) { audioClip = audioSource.clip; }

            // If there is a valid clip, play it.
            if (audioClip != null && audioSource.isActiveAndEnabled && !audioSource.isPlaying) { audioSource.Play(); }

            isAudioSourcePaused = false;

            IncrementSequenceNumber();

            if (usePooling)
            {
                if (isReparented) { poolParentTrfm = transform.parent; }

                // If pooling, cache the list of particle systems for this effect
                if (shurikenParticlesList == null)
                {
                    shurikenParticlesList = new List<ParticleSystem>();
                    shurikenParticlesList.AddRange(GetComponentsInChildren<ParticleSystem>());
                    numShurikenParticles = shurikenParticlesList == null ? 0 : shurikenParticlesList.Count;
                }

                ParticleSystem particleSystem = null;

                // Start all particle systems that haven't started automatically on awake
                for (int spIdx = 0; spIdx < numShurikenParticles; spIdx++)
                {
                    particleSystem = shurikenParticlesList[spIdx];
                    if (particleSystem != null && !particleSystem.isPlaying) { particleSystem.Play(true); }
                }
            }

            // After a given amount of time, automatically destroy this effects object
            Invoke(destroyMethodName, despawnTime);

            return itemSequenceNumber;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Makes this EffectsModule unique from all others that have gone before them.
        /// This is called every time the effect is Initialised. 
        /// </summary>
        internal void IncrementSequenceNumber()
        {
            itemSequenceNumber = nextSequenceNumber++;
            // if sequence number needs to be wrapped, do so to a high-ish number that is unlikely to be in use 
            if (nextSequenceNumber > uint.MaxValue - 100) { nextSequenceNumber = 100000; }
        }

        /// <summary>
        /// Removes the effects object. How this is done depends on what system is being used (i.e. pooling etc.).
        /// If pooling is enabled, and there is an audio clip still playing, stop it.
        /// </summary>
        internal void DestroyEffectsObject()
        {
            //Debug.Log("[DEBUG] EffectsModule.DestroyEffectsObject T: " + Time.time);

            if (usePooling)
            {
                // If there is a clip and it is still playing, stop it
                if (audioClip != null && audioSource.isActiveAndEnabled && audioSource.isPlaying)
                {
                    audioSource.Stop();
                    isAudioSourcePaused = false;
                }

                ParticleSystem particleSystem = null;

                // Stop all particle systems
                for (int spIdx = 0; spIdx < numShurikenParticles; spIdx++)
                {
                    particleSystem = shurikenParticlesList[spIdx];
                    if (particleSystem != null && particleSystem.isPlaying) { shurikenParticlesList[spIdx].Stop(true); }
                }

                // Deactivate the effects object
                gameObject.SetActive(false);

                if (isReparented && poolParentTrfm != null)
                {
                    //Debug.Log("[DEBUG] EffectsModule.DestroyEffectsObject and reparent " + name + " T: " + Time.time);
                    transform.SetParent(poolParentTrfm);
                }
            }
            else
            {
                // Destroy the effects object
                Destroy(gameObject);
            }
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Re-enable an Effect after it has been disabled with
        /// DisableEffects(). See also SSCManager.ResumeEffectsObjects().
        /// NOTE: Only takes action if IsEffectsModuleEnabled is false.
        /// </summary>
        public void EnableEffects()
        {
            if (!isEffectsModuleEnabled)
            {
                // If there is a clip and it is still playing, unpause it
                if (audioClip != null && audioSource.isActiveAndEnabled && isAudioSourcePaused)
                {
                    audioSource.UnPause();
                    isAudioSourcePaused = false;
                }

                ParticleSystem particleSystem = null;

                // Play (resume) all paused particle systems
                for (int spIdx = 0; spIdx < numShurikenParticles; spIdx++)
                {
                    particleSystem = shurikenParticlesList[spIdx];
                    if (particleSystem != null && particleSystem.isPaused) { shurikenParticlesList[spIdx].Play(true); }
                }

                isEffectsModuleEnabled = true;
            }
        }

        /// <summary>
        /// Useful when you want to pause the action in a game.
        /// Should always be called BEFORE setting Time.timeScale to 0.
        /// See also SSCManager.PauseEffectsObjects().
        /// NOTE: Only takes action if IsEffectsModuleEnabled is true.
        /// </summary>
        public void DisableEffects()
        {
            if (isEffectsModuleEnabled)
            {
                // If there is a clip and it is still playing, pause it
                if (audioClip != null && audioSource.isActiveAndEnabled && audioSource.isPlaying)
                {
                    audioSource.Pause();
                    isAudioSourcePaused = true;
                }

                ParticleSystem particleSystem = null;

                // Pause all particle systems
                for (int spIdx = 0; spIdx < numShurikenParticles; spIdx++)
                {
                    particleSystem = shurikenParticlesList[spIdx];
                    if (particleSystem != null && particleSystem.isPlaying) { shurikenParticlesList[spIdx].Pause(true); }
                }

                isEffectsModuleEnabled = false;
            }  
        }

        #endregion
    }
}
