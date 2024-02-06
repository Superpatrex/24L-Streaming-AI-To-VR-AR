using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    public class ThrusterEffects : MonoBehaviour
    {
        #region Public variables
        /// <summary>
        /// The amount of input required before the effect will have any effect.
        /// This can be useful with controllers which can produce small values
        /// when the user is not moving them (simulates a dead-zone).
        /// It can also avoid thruster effects "firing" for very small inputs.
        /// </summary>
        [Range(0f, 0.1f)] public static float inputThreshold = 0.005f;
        #endregion
        
        #region Private variables - General
        private bool initialised = false;
        private int index = 0;
        private float currentThrusterInput = 0f;
        #endregion

        #region Private shuriken particle variables
        // Arrays of shuriken particle systems and their initial rate over time multipliers
        private ParticleSystem[] shurikenParticles;
        private float[] shurikenParticleInitialRateMultipliers;
        private float[] shurikenParticleInitialSpeedMultipliers;

        private int numShurikenParticles = 0;

        private ParticleSystem.EmissionModule shurikenEmissionModule;
        private ParticleSystem.MainModule shurikenMainModule;
        #endregion

        #region Private AudioSource variables
        // Audio sources for clips
        private AudioSource[] audioSources;
        private int numAudioSources = 0;
        private AudioSource audioSource;
        private float maxVolume = 1f;

        #endregion

        #region Private or Internal Methods

        /// <summary>
        /// [INTERNAL ONLY]
        /// Clear thruster effects data
        /// </summary>
        internal void Clear()
        {
            shurikenParticles = null;
            shurikenParticleInitialRateMultipliers = null;
            shurikenParticleInitialSpeedMultipliers = null;
            audioSources = null;
            numAudioSources = 0;
            numShurikenParticles = 0;
            currentThrusterAudioVolume = 0f;
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        internal float currentThrusterAudioVolume = 0f;

        /// <summary>
        /// If initialised restore the ParticleSystems to their original values. This is typically
        /// used when we want to reinitialise the system.
        /// </summary>
        protected virtual void RestoreInitialValues()
        {
            if (initialised)
            {
                // Restore Particle Systems to original values
                numShurikenParticles = shurikenParticles == null ? 0 : shurikenParticles.Length;

                for (index = 0; index < numShurikenParticles; index++)
                {
                    if (shurikenParticles[index] != null)
                    {
                        shurikenEmissionModule = shurikenParticles[index].emission;
                        shurikenMainModule = shurikenParticles[index].main;

                        shurikenEmissionModule.rateOverTimeMultiplier = shurikenParticleInitialRateMultipliers[index];
                        shurikenMainModule.startSpeedMultiplier = shurikenParticleInitialSpeedMultipliers[index];
                    }
                }

                // Restore Audio Sources to original volumes
                numAudioSources = audioSources == null ? 0 : audioSources.Length;

                for (index = 0; index < numAudioSources; index++)
                {
                    audioSources[index].volume = maxVolume;
                }
            }
        }

        #endregion

        #region Public Virtual Member Methods

        /// <summary>
        /// Uses the given thruster input value to update all thruster effects systems i.e. particle systems.
        /// minEffectsRate must be between 0.0 and 1.0. This is the minimum particle system emission rate
        /// proportional to the maximum emission rate.
        /// Assumes thruster is not null.
        /// </summary>
        /// <param name="thruster"></param>
        /// <param name="shipLocalVelocity"></param>
        public virtual void UpdateThrusterInput (Thruster thruster, Vector3 shipLocalVelocity)
        {
            if (initialised)
            {
                // We only want to update the thruster effect systems when we receive input from this function
                // This way we can update this less frequently etc. to improve performance

                // Check for maxThrust > 0. If no thrust, then no FX or audio.
                float thrusterInput = thruster.maxThrust * thruster.throttle > 0f ? thruster.currentInput * thruster.CurrentPerformance : 0f;
                bool isFXLimited = false;

                // Check for inputThreshold
                currentThrusterInput = thrusterInput < inputThreshold ? 0f : thrusterInput;

                // Check if ship is travelling too fast or two slow on Z-axis for the thruster to be active
                if (currentThrusterInput > 0f && thruster.limitEffectsOnZ)
                {
                    if (shipLocalVelocity.z < thruster.minEffectsOnZ || shipLocalVelocity.z > thruster.maxEffectsOnZ)
                    {
                        currentThrusterInput = 0f;
                        isFXLimited = true;
                    }
                }

                // Check if ship is travelling too fast or two slow on Y-axis for the thruster to be active
                if (currentThrusterInput > 0f && thruster.limitEffectsOnY)
                {
                    if (shipLocalVelocity.y < thruster.minEffectsOnY || shipLocalVelocity.y > thruster.maxEffectsOnY)
                    {
                        currentThrusterInput = 0f;
                        isFXLimited = true;
                    }
                }

                if (currentThrusterInput > 0f || (!isFXLimited && thruster.isMinEffectsAlwaysOn && thruster.throttle > 0f))
                {
                    // If user wants a minimum particle emission or audio volume, update accordingly.
                    currentThrusterInput = currentThrusterInput < thruster.minEffectsRate ? thruster.minEffectsRate * (thruster.isThrottleMinEffects ? thruster.throttle : 1f) : currentThrusterInput;
                    // Clamp to 1.0
                    if (currentThrusterInput > 1f) { currentThrusterInput = 1f; }
                }

                // Update shuriken particle systems
                numShurikenParticles = shurikenParticles == null ? 0 : shurikenParticles.Length;
                for (index = 0; index < numShurikenParticles; index++)
                {
                    ParticleSystem pSystem = shurikenParticles[index];

                    shurikenEmissionModule = pSystem.emission;
                    shurikenMainModule = pSystem.main;

                    if (currentThrusterInput > 0f)
                    {
                        // +SMS v1.3.6 Beta 1j. Used with isMinEffectsAlwaysOn and ship.isThrusterFXStationary
                        if (thruster.isMinEffectsAlwaysOn && pSystem.isPaused) { pSystem.Play(true); }

                        shurikenEmissionModule.rateOverTimeMultiplier = shurikenParticleInitialRateMultipliers[index] * currentThrusterInput;
                        shurikenMainModule.startSpeedMultiplier = shurikenParticleInitialSpeedMultipliers[index] * currentThrusterInput;
                    }
                    else
                    {
                        shurikenEmissionModule.rateOverTimeMultiplier = 0f;
                        shurikenMainModule.startSpeedMultiplier = 0f;
                    }
                }

                // Update Audio Sources
                // See also shipControlModule.GetThrusterAudioVolume(..)
                currentThrusterAudioVolume = currentThrusterInput > 0f ? currentThrusterInput * maxVolume : 0f;

                // numAudioSources is set in Initialise()
                for (index = 0; index < numAudioSources; index++)
                {
                    audioSource = audioSources[index];

                    if (currentThrusterInput > 0f)
                    {
                        // +SMS v1.3.6 Beta 1j. Used with isMinEffectsAlwaysOn and ship.isThrusterFXStationary
                        if (thruster.isMinEffectsAlwaysOn && audioSource.mute) { audioSource.mute = false; }

                        //audioSource.volume = currentThrusterInput * maxVolume;
                        audioSource.volume = currentThrusterAudioVolume;
                        if (!audioSource.isPlaying && audioSource.isActiveAndEnabled) { audioSource.Play(); }
                    }
                    else { audioSource.volume = 0f; }
                }
            }
        }

        /// <summary>
        /// Pauses the thruster effects.
        /// </summary>
        public virtual void Pause()
        {
            // Update shuriken particle systems
            numShurikenParticles = shurikenParticles == null ? 0 : shurikenParticles.Length;
            for (index = 0; index < numShurikenParticles; index++)
            {
                shurikenParticles[index].Pause();
            }

            Mute(true);
        }

        /// <summary>
        /// Stop thruster effects and set sound volume to 0.
        /// </summary>
        public virtual void Stop()
        {
            // Update shuriken particle systems
            numShurikenParticles = shurikenParticles == null ? 0 : shurikenParticles.Length;
            for (index = 0; index < numShurikenParticles; index++)
            {
                // Cannot just do Stop() as previous state could have been Pause()
                shurikenParticles[index].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            // Turn off volume.
            // numAudioSources is set in Initialise()
            for (index = 0; index < numAudioSources; index++)
            {
                audioSource = audioSources[index];
                audioSource.volume = 0f;
                if (audioSource.isActiveAndEnabled && audioSource.isPlaying) { audioSource.Stop(); }
            }
        }

        /// <summary>
        /// Plays the thruster effects (resuming them if they were previously paused).
        /// </summary>
        public virtual void Play()
        {
            // Update shuriken particle systems
            numShurikenParticles = shurikenParticles == null ? 0 : shurikenParticles.Length;
            for (index = 0; index < numShurikenParticles; index++)
            {
                shurikenParticles[index].Play();
            }

            UnMute(true);
        }

        /// <summary>
        /// Set the max volume of the thruster effects audio.
        /// </summary>
        /// <param name="newMaxVolume"></param>
        public virtual void SetMaxVolume(float newMaxVolume)
        {
            maxVolume = newMaxVolume < 0f ? 0f : newMaxVolume > 1f ? 1f : newMaxVolume;
        }

        /// <summary>
        /// Mutes the sound on all active audio sources on the thruster. This is automatically
        /// called in Pause().
        /// </summary>
        public virtual void Mute(bool isPauseSound)
        {
            // numAudioSources is set in Initialise()
            for (index = 0; index < numAudioSources; index++)
            {
                audioSource = audioSources[index];
                audioSource.mute = true;
                if (isPauseSound && audioSource.isPlaying && audioSource.isActiveAndEnabled) { audioSource.Pause(); }
            }
        }

        /// <summary>
        /// UnMutes the sound on all active audio sources on the thruster. This is automatically
        /// called in Play().
        /// </summary>
        /// <param name="isUnPauseSound"></param>
        public virtual void UnMute(bool isUnPauseSound)
        {
            // numAudioSources is set in Initialise()
            for (index = 0; index < numAudioSources; index++)
            {
                audioSource = audioSources[index];
                if (isUnPauseSound && !audioSource.isPlaying && audioSource.isActiveAndEnabled) { audioSource.Play(); }

                audioSource.mute = false;
            }
        }

        /// <summary>
        /// Initialise thruster effects.
        /// NOTE: This will increase Garbage Collection, so use sparingly.
        /// Ignores disabled AudioSources
        /// The audio max volume is set to the current volume of the audio source.
        /// </summary>
        public virtual void Initialise ()
        {
            // If this is being re-initialised, like when effects have been added, removed or disabled,
            // we need to reset the Particle Systems and Audio Sources to original values first.
            if (initialised) { RestoreInitialValues(); }

            // Find all the attached shuriken particle systems
            List<ParticleSystem> shurikenParticlesList = new List<ParticleSystem>();
            shurikenParticlesList.AddRange(GetComponentsInChildren<ParticleSystem>());
            shurikenParticles = shurikenParticlesList.ToArray();

            // Get the initial rate over time / speed multipliers - this is necessary as we want to always set the multiplier
            // to this initial value multiplied by the current thruster input
            numShurikenParticles = shurikenParticles == null ? 0 : shurikenParticles.Length;
            shurikenParticleInitialRateMultipliers = new float[numShurikenParticles];
            shurikenParticleInitialSpeedMultipliers = new float[numShurikenParticles];
            for (index = 0; index < numShurikenParticles; index++)
            {
                if (shurikenParticles[index] != null)
                {
                    shurikenParticleInitialRateMultipliers[index] = shurikenParticles[index].emission.rateOverTimeMultiplier;
                    shurikenParticleInitialSpeedMultipliers[index] = shurikenParticles[index].main.startSpeedMultiplier;
                }
            }

            // Find all the attached enabled audiosources
            List <AudioSource> audioSourceList = new List<AudioSource>(2);
            // For some reason when includeInactive is false, it still returns them.
            GetComponentsInChildren(false, audioSourceList);

            // As an optimisation remove any invalid or non-configured sources
            // We need to loop through the list and potentially have to count the items twice, but it will be faster when updating.
            numAudioSources = audioSourceList == null ? 0 : audioSourceList.Count;
            for (index = numAudioSources- 1; index >= 0; index--)
            {
                if (audioSourceList[index].clip == null || !audioSourceList[index].gameObject.activeSelf) { audioSourceList.RemoveAt(index); }
                else { maxVolume = audioSourceList[index].volume; }
            }

            audioSources = audioSourceList.ToArray();
            numAudioSources = audioSources == null ? 0 : audioSources.Length;

            initialised = true;
        }

        #endregion
    }
}
