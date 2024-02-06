using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Demo script to cycle (flash) one or more renderers on/off.
    /// WARNING
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    public class DemoS3DCycleRenderers : MonoBehaviour
    {
        #region Public Variables
        [Range(0f, 10f)] public float group1FlashInterval = 0f;
        [Range(0f, 10f)] public float group2FlashInterval = 0f;
        [Range(0f, 10f)] public float group3FlashInterval = 0f;

        [Range(0f, 30f)] public float group1ActivateDelay = 0f;
        [Range(0f, 30f)] public float group2ActivateDelay = 0f;
        [Range(0f, 30f)] public float group3ActivateDelay = 0f;

        [Range(0f, 30f)] public float group1DeactivateDelay = 0f;
        [Range(0f, 30f)] public float group2DeactivateDelay = 0f;
        [Range(0f, 30f)] public float group3DeactivateDelay = 0f;

        public bool group1StartOn = false;
        public bool group2StartOn = false;
        public bool group3StartOn = false;

        public MeshRenderer[] group1Renderers;
        public MeshRenderer[] group2Renderers;
        public MeshRenderer[] group3Renderers;
        #endregion

        #region Private Variables
        private bool isInitialised = false;
        private bool[] isGroupActive;
        private bool[] isGroupRendering;
        private int[] numRenderersArray;
        private float[] groupTimer;

        #endregion

        #region Initialise Methods

        // Start is called before the first frame update
        void Start()
        {
            isGroupActive = new bool[] { group1StartOn, group2StartOn, group3StartOn };
            isGroupRendering = new bool[] { group1StartOn, group2StartOn, group3StartOn };

            groupTimer = new float[] { 0f, 0f, 0f };

            numRenderersArray = new int[]
                                     { group1Renderers == null ? 0 : group1Renderers.Length,
                                       group2Renderers == null ? 0 : group2Renderers.Length,
                                       group3Renderers == null ? 0 : group3Renderers.Length };
            
            // Check which groups have valid renderers



            // Are there any renderers?
            if (numRenderersArray != null && numRenderersArray.Length == 3 && (numRenderersArray[0] > 0 || numRenderersArray[1] > 0 || numRenderersArray[2] > 0))
            {
                isInitialised = true;

                EnableGroup(1, group1StartOn);
                EnableGroup(2, group2StartOn);
                EnableGroup(3, group3StartOn);
            }
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            if (isInitialised)
            {
                // Loop through all the groups
                for (int groupNumber = 1; groupNumber < 4; groupNumber++)
                {
                    // Is this group active?
                    if (isGroupActive[groupNumber-1])
                    {
                        CheckFlashingStatus(groupNumber);
                    }
                }
            }
        }

        #endregion

        #region Private Methods


        private void CheckFlashingStatus(int groupNumber)
        {
            float groupFlashInterval = groupNumber == 1 ? group1FlashInterval : groupNumber == 2 ? group2FlashInterval : group3FlashInterval;

            if (groupFlashInterval > 0f)
            {
                // If past the interval time, toggle the renderers on/off
                if (groupTimer[groupNumber-1] > groupFlashInterval)
                {
                    groupTimer[groupNumber-1] = 0f;

                    // Toggle the render
                    isGroupRendering[groupNumber-1] = !isGroupRendering[groupNumber-1];

                    MeshRenderer[] meshRenderers = groupNumber == 1 ? group1Renderers : groupNumber == 2 ? group2Renderers : group3Renderers;

                    EnableRenderers(meshRenderers, numRenderersArray[groupNumber-1], isGroupRendering[groupNumber-1]);
                }
                else
                {
                    // Increment the time for this group
                    groupTimer[groupNumber - 1] += Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// Enable or disable the renderers in a group
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="isEnabled"></param>
        private void EnableGroup (int groupNumber, bool isEnabled)
        {
            if (isInitialised && groupNumber > 0 && groupNumber < 4)
            {
                int numRenderersInGroup = numRenderersArray[groupNumber - 1];

                if (groupNumber == 1) { EnableRenderers(group1Renderers, numRenderersInGroup, isEnabled); }
                else if (groupNumber == 2) { EnableRenderers(group2Renderers, numRenderersInGroup, isEnabled); }
                else if (groupNumber == 3) { EnableRenderers(group3Renderers, numRenderersInGroup, isEnabled); }

                // Remember if the group is active or inactive
                isGroupActive[groupNumber - 1] = isEnabled;

                // Groups that are active start in the rendering state.
                isGroupRendering[groupNumber - 1] = isEnabled;

                // Reset the group timer
                groupTimer[groupNumber - 1] = 0f;
            }
        }

        /// <summary>
        /// Call this method with a delay
        /// </summary>
        private void DisableGroup1()
        {
            EnableGroup(1, false);
        }

        /// <summary>
        /// Call this method with a delay
        /// </summary>
        private void DisableGroup2()
        {
            EnableGroup(2, false);
        }

        /// <summary>
        /// Call this method with a delay
        /// </summary>
        private void DisableGroup3()
        {
            EnableGroup(3, false);
        }

        /// <summary>
        /// Call this method with a delay
        /// </summary>
        private void EnableGroup1()
        {
            EnableGroup(1, true);
        }

        /// <summary>
        /// Call this method with a delay
        /// </summary>
        private void EnableGroup2()
        {
            EnableGroup(2, true);
        }

        // Call this method with a delay
        private void EnableGroup3()
        {
            EnableGroup(3, true);
        }

        /// <summary>
        /// Enable or disable a group of renderers
        /// </summary>
        /// <param name="groupRenderers"></param>
        /// <param name="numRenderers"></param>
        /// <param name="isEnable"></param>
        private void EnableRenderers(MeshRenderer[] groupRenderers, int numRenderers, bool isEnabled)
        {
            for (int r = 0; r < numRenderers; r++)
            {
                MeshRenderer meshRenderer = groupRenderers[r];

                if (meshRenderer != null)
                {
                    meshRenderer.enabled = isEnabled;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Turn the group of renderers on. Valid numbers are between 1 and 3.
        /// </summary>
        /// <param name="groupNumber"></param>
        public void TurnGroupOn(int groupNumber)
        {
            if (groupNumber == 1 && group1ActivateDelay > 0f)
            {
                Invoke("EnableGroup1", group1ActivateDelay);
            }
            else if (groupNumber == 2 && group2ActivateDelay > 0f)
            {
                Invoke("EnableGroup2", group2ActivateDelay);
            }
            else if (groupNumber == 3 && group3ActivateDelay > 0f)
            {
                Invoke("EnableGroup3", group3ActivateDelay);
            }
            else
            { 
                EnableGroup(groupNumber, true);
            }
        }

        /// <summary>
        /// Turn the group of renderers off. Valid numbers are between 1 and 3.
        /// </summary>
        /// <param name="groupNumber"></param>
        public void TurnGroupOff(int groupNumber)
        {
            if (groupNumber == 1 && group1DeactivateDelay > 0f)
            {
                Invoke("DisableGroup1", group1DeactivateDelay);
            }
            else if (groupNumber == 2 && group2DeactivateDelay > 0f)
            {
                Invoke("DisableGroup2", group2DeactivateDelay);
            }
            else if (groupNumber == 3 && group3DeactivateDelay > 0f)
            {
                Invoke("DisableGroup3", group3DeactivateDelay);
            }
            else
            {
                EnableGroup(groupNumber, false);
            }
        }

        #endregion

    }
}