using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Sample script to show how to override the thruster particle effects based on the speed
    /// of the ship. Attach to the parent of a ship prefab. This assumes that particle effect
    /// attached to thrusters have Loop enabled.
    /// Attach this script to the parent gameobject of your ship or prefab.
    /// This is only a sample to demonstrate how API calls could be used in your own code.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Thruster FX Override")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleThrusterFXOverride : MonoBehaviour
    {
        #region Public Variables
        public ParticleSystem[] thrusterParticleFX;

        // Disable thruster particle fx emission if the ship speed is greater
        public float speedToDisable = 2f;
        public float initialInterval = 3f;
        public float updateInterval = 0.5f;
        #endregion

        #region Private Variables
        private ShipControlModule shipControlModule;
        private int numParticleFX = 0;
        private float sqrSpeedToDisable = 0f;
        private List<AudioSource> audioSourceList;

        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            shipControlModule = GetComponent<ShipControlModule>();
            if (shipControlModule != null) { Initialise(); }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("SampleThrusterFXOverride - could not find ShipControlModule - which should be attached to the same gameobject");
            }
            #endif
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Call this when the number of thrusterParticleFXs in the array have changed 
        /// </summary>
        public void Initialise()
        {
            CancelInvoke();
            numParticleFX = thrusterParticleFX == null ? 0 : thrusterParticleFX.Length;
            sqrSpeedToDisable = speedToDisable * speedToDisable;

            if (numParticleFX > 0)
            {
                if (audioSourceList == null) { audioSourceList = new List<AudioSource>(numParticleFX); }
                else { audioSourceList.Clear(); }

                // Find the audio source (if any) attached to the thrusterFX
                for (int psIdx = 0; psIdx < numParticleFX; psIdx++)
                {
                    ParticleSystem particleSystem = thrusterParticleFX[psIdx];
                    if (particleSystem != null)
                    {
                        AudioSource audioSource = particleSystem.GetComponent<AudioSource>();
                        // If there is no audioSource attached to this thruster fx, add a null
                        if (audioSource == null) { audioSourceList.Add(null); }
                        else { audioSourceList.Add(audioSource); }
                    }
                }

                InvokeRepeating("CheckThrusterFX", initialInterval, updateInterval);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check each of the thruster particle systems. Rather than disable the particle suddenly,
        /// disable looping. When travelling less than the speedToDisable, turn Loop back on and
        /// begin playing.
        /// </summary>
        private void CheckThrusterFX()
        {
            if (shipControlModule != null)
            {
                Vector3 worldVelocity = shipControlModule.shipInstance.WorldVelocity;
                // Get square magnitude to avoid square root
                float sqrMagnitude = worldVelocity.x * worldVelocity.x + worldVelocity.y * worldVelocity.y + worldVelocity.z * worldVelocity.z;

                // Disable thruster particle fx emission if the ship speed is greater
                bool disableFX = sqrMagnitude > sqrSpeedToDisable;

                // numParticleFX is set once in Start().
                for (int psIdx = 0; psIdx < numParticleFX; psIdx++)
                {
                    ParticleSystem particleSystem = thrusterParticleFX[psIdx];
                    if (particleSystem != null)
                    {
                        ParticleSystem.MainModule mainModule = particleSystem.main;

                        if (mainModule.loop)
                        {
                            if (disableFX)
                            {
                                mainModule.loop = false;
                                AudioSource audioSource = audioSourceList[psIdx];
                                // Check if there is a valid audiosource for this thrusterfx
                                // NOTE: stopping the audiosource will have no effect, because SSC will start playing the clip again
                                if (audioSource != null && audioSource.clip != null && audioSource.isActiveAndEnabled) { audioSource.mute = true; }
                            }
                        }
                        else if (!disableFX)
                        {
                            mainModule.loop = true;
                            if (particleSystem.isStopped) { particleSystem.Play(true); }

                            AudioSource audioSource = audioSourceList[psIdx];
                            // Check if there is a valid audiosource for this thrusterfx
                            if (audioSource != null && audioSource.clip != null && audioSource.isActiveAndEnabled) { audioSource.mute = false; }

                        }
                    }
                }
            }
        }

        #endregion
    }
}