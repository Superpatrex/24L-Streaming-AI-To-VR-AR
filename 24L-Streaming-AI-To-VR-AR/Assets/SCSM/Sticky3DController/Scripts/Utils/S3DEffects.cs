using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This is a runtime data class to hold particle systems which are attached to or children of a gameobject.
    /// They are currently used for the character JetPack system.
    /// Not to be confused with StickyEffectsModule which is used when something is hit, damaged or destroyed.
    /// </summary>
    public class S3DEffects
    {
        #region Public Static variables
        /// <summary>
        /// The amount of input required before the effect will have any effect.
        /// This can be useful with controllers which can produce small values
        /// when the user is not moving them (simulates a dead-zone).
        /// It can also avoid effects "firing" for very small inputs.
        /// </summary>
        [Range(0f, 0.1f)] public static float inputThreshold = 0.005f;
        #endregion

        #region Public Variables

        /// <summary>
        /// The 0.0-1.0 value that indicates the minimum normalised amount of any particle effects that are
        /// applied when a non-zero input is received. Default is 0. If the full particle emission
        /// rate should be applied when any input is received, set the value to 1.0.
        /// </summary>
        [Range(0f, 1f)] public float minEffectsRate;

        #endregion

        #region Private Variables
        private bool initialised;
        private float currentInput;
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

        #region Constructors

        public S3DEffects()
        {
            initialised = false;
            currentInput = 0f;
            minEffectsRate = 0f;
        }

        #endregion

        #region Private or Internal Methods
        /// <summary>
        /// If initialised restore the ParticleSystems to their original values. This is typically
        /// used when we want to reinitialise the system.
        /// </summary>
        internal void RestoreInitialValues()
        {
            if (initialised)
            {
                // Restore Particle Systems to original values
                numShurikenParticles = shurikenParticles == null ? 0 : shurikenParticles.Length;

                for (int index = 0; index < numShurikenParticles; index++)
                {
                    if (shurikenParticles[index] != null)
                    {
                        shurikenEmissionModule = shurikenParticles[index].emission;
                        shurikenMainModule = shurikenParticles[index].main;

                        shurikenEmissionModule.rateOverTimeMultiplier = shurikenParticleInitialRateMultipliers[index];
                        shurikenMainModule.startSpeedMultiplier = shurikenParticleInitialSpeedMultipliers[index];
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public void Initialise(GameObject gameObject)
        {
            // Find all the attached shuriken particle systems
            List<ParticleSystem> shurikenParticlesList = new List<ParticleSystem>();
            shurikenParticlesList.AddRange(gameObject.GetComponentsInChildren<ParticleSystem>());
            shurikenParticles = shurikenParticlesList.ToArray();

            // Get the initial rate over time / speed multipliers - this is necessary as we want to always set the multiplier
            // to this initial value multiplied by the current input
            numShurikenParticles = shurikenParticles == null ? 0 : shurikenParticles.Length;
            shurikenParticleInitialRateMultipliers = new float[numShurikenParticles];
            shurikenParticleInitialSpeedMultipliers = new float[numShurikenParticles];
            for (int index = 0; index < numShurikenParticles; index++)
            {
                if (shurikenParticles[index] != null)
                {
                    shurikenEmissionModule = shurikenParticles[index].emission;
                    shurikenMainModule = shurikenParticles[index].main;

                    // Record their initial values
                    shurikenParticleInitialRateMultipliers[index] = shurikenEmissionModule.rateOverTimeMultiplier;
                    shurikenParticleInitialSpeedMultipliers[index] = shurikenMainModule.startSpeedMultiplier;

                    // Start them at zero
                    shurikenEmissionModule.rateOverTimeMultiplier = 0f;
                    shurikenMainModule.startSpeedMultiplier = 0f;
                }
            }

            initialised = true;
        }

        /// <summary>
        /// Pauses the particle effects.
        /// </summary>
        public void Pause()
        {
            // Update shuriken particle systems
            numShurikenParticles = shurikenParticles == null ? 0 : shurikenParticles.Length;
            for (int index = 0; index < numShurikenParticles; index++)
            {
                shurikenParticles[index].Pause();
            }
        }

        /// <summary>
        /// Plays the particle effects (resuming them if they were previously paused).
        /// </summary>
        public void Play()
        {
            // Update shuriken particle systems
            numShurikenParticles = shurikenParticles == null ? 0 : shurikenParticles.Length;
            for (int index = 0; index < numShurikenParticles; index++)
            {
                shurikenParticles[index].Play();
            }
        }

        /// <summary>
        /// Stop particle effects.
        /// </summary>
        public void Stop()
        {
            // Update shuriken particle systems
            numShurikenParticles = shurikenParticles == null ? 0 : shurikenParticles.Length;
            for (int index = 0; index < numShurikenParticles; index++)
            {
                // Cannot just do Stop() as previous state could have been Pause()
                shurikenParticles[index].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public void UpdateEffects(float input)
        {
            currentInput = input < inputThreshold ? 0f : input;

            if (currentInput > 0f)
            {
                // If user wants a minimum particle emission update accordingly.
                currentInput = currentInput < minEffectsRate ? minEffectsRate : currentInput;
                // Clamp to 1.0
                if (currentInput > 1f) { currentInput = 1f; }
            }

            // Update shuriken particle systems
            numShurikenParticles = shurikenParticles == null ? 0 : shurikenParticles.Length;
            for (int index = 0; index < numShurikenParticles; index++)
            {
                shurikenEmissionModule = shurikenParticles[index].emission;
                shurikenMainModule = shurikenParticles[index].main;

                if (currentInput > 0f)
                {
                    shurikenEmissionModule.rateOverTimeMultiplier = shurikenParticleInitialRateMultipliers[index] * currentInput;
                    shurikenMainModule.startSpeedMultiplier = shurikenParticleInitialSpeedMultipliers[index] * currentInput;
                }
                else
                {
                    shurikenEmissionModule.rateOverTimeMultiplier = 0f;
                    shurikenMainModule.startSpeedMultiplier = 0f;
                }
            }
        }

        #endregion
    }
}