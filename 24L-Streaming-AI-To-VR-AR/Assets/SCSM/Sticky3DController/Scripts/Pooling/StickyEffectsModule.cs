using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// [INCOMPLETE]
    /// Implement a particle and/or sound effect when something is hit, damaged or destroyed.
    /// This can include multiple child particle systems and/or an audio source attached to the parent gameobject. 
    /// </summary>
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyEffectsModule : StickyGenericModule
    {
        #region Enumerations

        public enum EffectsType
        {
            Default = 0,
            SoundFX = 5
        }

        #endregion

        #region Public Variables

        /// <summary>
        /// The ID number for this effects prefab (as assigned by the Sticky Manager in the scene).
        /// This is the index in the StickyManager effectsTemplatesList.
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] public int effectsObjectPrefabID = -1;

        #endregion

        #region Public Properties

        /// <summary>
        /// Get or set the module EffectsType. Most effects modules will have the Default type.
        /// Sound FX is for items that specifically require a Sound FX rather than a general StickyEffectsModule
        /// </summary>
        public EffectsType ModuleEffectsType { get { return effectsType; } set { effectsType = value; } }

        #endregion

        #region Public Static Variables

        #endregion

        #region Private and Protected Variables - General

        /// <summary>
        /// Most effects modules will have the Default type.
        /// Sound FX is for items that specifically require a Sound FX rather than a general StickyEffectsModule
        /// </summary>
        [SerializeField] protected EffectsType effectsType = EffectsType.Default;

        private S3DInstantiateGenericObjectParameters igParms;

        [System.NonSerialized] protected AudioSource audioSource = null;
        [System.NonSerialized] protected AudioClip audioClip = null;
        [System.NonSerialized] protected List<ParticleSystem> shurikenParticlesList = null;
        [System.NonSerialized] protected int numShurikenParticles = 0;
        [System.NonSerialized] protected bool isEffectsModuleEnabled = true;
        [System.NonSerialized] protected bool isAudioSourcePaused = false;

        #endregion

        #region Public Delegates

        #endregion

        #region Private Initialise Methods

        #endregion

        #region Update Methods

        #endregion

        #region Private and Internal Methods - General

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

        #region Events

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Removes the effects object. If there is an audio clip still playing, stop it.
        /// </summary>
        public virtual void DestroyEffectsObject()
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

            DestroyGenericObject();
        }

        /// <summary>
        /// Initialise the instance of the StickyEffectsModule.
        /// This is automatically called from the StickyManager pooling system whenever it is spawned (instantiated) in the scene.
        /// </summary>
        /// <param name="ieParms"></param>
        /// <returns></returns>
        public virtual uint Initialise (S3DInstantiateEffectsObjectParameters ieParms)
        {
            // Store the index to the EffectsObjectTemplate from the StickyManager effectsObjectTemplatesList
            this.effectsObjectPrefabID = ieParms.effectsObjectPrefabID;

            // Initialise generic object module
            // Currently igParms are required to be set for generic object Initialise(..).
            igParms.genericObjectPoolListIndex = -1;
            Initialise(igParms);

            // Initial effects module
            // Check for an audiosource attached to the same gameobject in the prefab.
            // For performance reasons, don't search child objects.
            if (audioSource == null) { audioSource = GetComponent<AudioSource>(); }

            // If the audioClip is not defined, attempt to cache it (if it exists)
            if (audioClip == null && audioSource != null) { audioClip = audioSource.clip; }

            // If there is a valid clip, play it.
            if (audioClip != null && audioSource.isActiveAndEnabled && !audioSource.isPlaying) { audioSource.Play(); }

            isAudioSourcePaused = false;

            // Cache the list of particle systems for this effect
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

            return itemSequenceNumber;
        }

        /// <summary>
        /// Initialise the instance of the StickyEffectsModule.
        /// This is automatically called from the StickyManager pooling system whenever it is spawned (instantiated) in the scene.
        /// </summary>
        /// <param name="sfxParms"></param>
        /// <returns></returns>
        public virtual uint Initialise (S3DInstantiateSoundFXParameters sfxParms)
        {
            // Store the index to the EffectsObjectTemplate from the StickyManager effectsObjectTemplatesList
            this.effectsObjectPrefabID = sfxParms.effectsObjectPrefabID;

            // Initialise generic object module
            // Currently igParms are required to be set for generic object Initialise(..).
            igParms.genericObjectPoolListIndex = -1;
            Initialise(igParms);

            // Initial effects module
            // Check for an audiosource attached to the same gameobject in the prefab.
            // For performance reasons, don't search child objects.
            if (audioSource == null) { audioSource = GetComponent<AudioSource>(); }

            // If the audioClip is not defined, attempt to cache it (if it exists)
            if (audioClip == null && audioSource != null) { audioClip = audioSource.clip; }

            // If there is a valid clip, play it.
            if (audioClip != null && audioSource.isActiveAndEnabled && !audioSource.isPlaying) { audioSource.Play(); }

            isAudioSourcePaused = false;

            return itemSequenceNumber;
        }

        #endregion

    }
}