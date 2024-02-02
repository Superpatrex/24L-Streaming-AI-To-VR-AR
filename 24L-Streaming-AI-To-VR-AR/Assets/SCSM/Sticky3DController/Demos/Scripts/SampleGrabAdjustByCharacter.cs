using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple sample script to attach to an interactive-enabled object like a weapon.
    /// It can be used when different characters have different rig setups and may
    /// incorrectly rotate objects when they are grabbed.
    /// 1. On your Sticky3D characters, go to the Engage tab and give it a Model ID.
    ///    (each character of the same type should have the same Model ID).
    /// 2. Add this component to your interactive-enabled object.
    /// 3. Add one of more model numbers to this component
    /// 4. Add the same number of positions and/or rotation adjustments (1 for each different Model ID)
    /// 5. On the Grabbable interactive object, go to the "Events" tab and add an On Post Grabbed event
    /// 6. Drag the interactive object gameobject into the Event Object slot
    /// 7. Set the Function for the Event to SampleGrabAdjustByCharacter (Dynamic) OnGrabbed.
    /// NOTE:
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    [AddComponentMenu("Sticky3D Controller/Samples/Grab Adjust by Character")]
    [DisallowMultipleComponent]
    public class SampleGrabAdjustByCharacter : MonoBehaviour
    {
        #region Public Variables

        [Tooltip("The different character model IDs that you wish to modify grab position and/or rotation")]
        public int[] models;

        [Tooltip("The matching position adjustments for the models. If none are required, leave this as an array of 0")]
        public Vector3[] positionAdjustments;

        [Tooltip("The matching rotation adjustments for the models. If none are required, leave this as an array of 0")]
        public Vector3[] rotationAdjustments;

        #endregion

        #region Private Variables - General

        private StickyInteractive stickyInteractive = null;
        private bool isInitialised = false;
        private int numModels = 0;
        private int numPositionAdjustments = 0;
        private int numRotationAdjustments = 0;

        #endregion

        #region Private Initialise Methods

        // Use this for initialization
        void Start()
        {
            if (TryGetComponent(out stickyInteractive))
            {
                numModels = models == null ? 0 : models.Length;
                numPositionAdjustments = positionAdjustments == null ? 0 : positionAdjustments.Length;
                numRotationAdjustments = rotationAdjustments == null ? 0 : rotationAdjustments.Length;

                if (numModels < 1)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SampleGrabAdjustByCharacter - there needs to be at least 1 Sticky3D character model number in " + name);
                    #endif
                }
                else if (numPositionAdjustments > 0 && numModels != numPositionAdjustments)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SampleGrabAdjustByCharacter - You need to have same number of Models AND Position adjustments OR set the array size to 0 and use only Rotations on " + name);
                    #endif
                }
                else if (numRotationAdjustments > 0 && numModels != numRotationAdjustments)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SampleGrabAdjustByCharacter - You need to have same number of Models AND Rotation adjustments OR set the array size to 0 and use only Positions on " + name);
                    #endif
                }
                else if (numPositionAdjustments == 0 && numRotationAdjustments == 0)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SampleGrabAdjustByCharacter - You need to have same number of Models AND (Position or Rotation or both) adjustments on " + name);
                    #endif
                }
                else
                {
                    isInitialised = true;
                }
            }
        }

        #endregion

        #region Private and Internal Methods - General

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// This gets called automatically when you hook up the Function in the On Post Grabbed event for the interactive-enabled object
        /// </summary>
        /// <param name="handHoldPosition"></param>
        /// <param name="handHoldNormal"></param>
        /// <param name="stickyInteractiveID"></param>
        /// <param name="stickyID"></param>
        public void OnPostGrabbed (Vector3 handHoldPosition, Vector3 handHoldNormal, int stickyInteractiveID, int stickyID)
        {
            if (isInitialised)
            {
                // If this is a weapon we can find who is holding it.
                if (stickyInteractive.IsStickyWeapon)
                {
                    // Get the StickyWeapon component
                    StickyWeapon stickyWeapon = (StickyWeapon)stickyInteractive;

                    // Get the character tha grabbed the weapon
                    StickyControlModule stickyControlModule = stickyWeapon.Sticky3DCharacter;

                    if (stickyControlModule != null)
                    {
                        // Get the Model ID for this character
                        int characterModelId = stickyControlModule.modelId;

                        // Find any matching adjustments we want to apply to this character model type
                        int modelsIdx = System.Array.FindIndex(models, m => m == characterModelId);

                        if (modelsIdx >= 0 && modelsIdx < numModels)
                        {
                            // Apply the transformations
                            if (numPositionAdjustments > modelsIdx)
                            {
                                Vector3 adjPos = positionAdjustments[modelsIdx];
                                Vector3 localScale = stickyWeapon.transform.localScale;
                                adjPos.x *= localScale.x;
                                adjPos.y *= localScale.y;
                                adjPos.z *= localScale.z;

                                stickyWeapon.transform.localPosition += adjPos;
                            }

                            if (numRotationAdjustments > modelsIdx)
                            {
                                stickyWeapon.transform.localRotation *= Quaternion.Euler(rotationAdjustments[modelsIdx]);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}