using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Add this sample script to a Sticky3D Controller gameobject to set
    /// a look target for the head.
    /// WARNING
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Head IK Target")]
    [DisallowMultipleComponent]
    public class SampleHeadIKTarget : MonoBehaviour
    {
        #region Public Variables
        public bool isInitialiseOnStart = false;
        public Transform lookAtTarget = null;
        #endregion

        #region Private Variables
        private bool isInitialised = false;
        private StickyControlModule stickyControlModule = null;
        #endregion

        #region Initialisation Methods

        private void Start()
        {
            if (isInitialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Private Methods


        #endregion

        #region Public Methods

        /// <summary>
        /// Get things ready. Call this from your game code if you don't have isInitialiseOnStart set.
        /// </summary>
        public void Initialise()
        {
            if (!isInitialised)
            {
                stickyControlModule = GetComponent<StickyControlModule>();

                if (stickyControlModule != null)
                {
                    if (stickyControlModule == null)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("ERROR: SampleHeadIKTarget - did you forget to attach this script to a S3D characters?");
                        #endif
                    }
                    else if (!stickyControlModule.IsInitialised)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("ERROR: SampleHeadIKTarget - your character " + stickyControlModule.name + " is not initialised");
                        #endif
                    }
                    else if (!stickyControlModule.IsHeadIKEnabled)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("ERROR: SampleHeadIKTarget - Head IK on " + stickyControlModule.name + " is not enabled and ready for use. Check Animate tab on the S3D character.");
                        #endif
                    }
                    else
                    {
                        if (lookAtTarget != null)
                        {
                            stickyControlModule.SetHeadIKTarget(lookAtTarget, Vector3.zero);
                        }

                        isInitialised = true;
                    }
                }
            }
        }

        #endregion
    }
}