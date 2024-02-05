using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Enable and disable, in a sequence 1, 2 or 3 groups of renderers. In each group,
    /// renderers will turn on and off one at a time. An example could be a set of "lights"
    /// (using emitting materials) that chase across a landing pad.
    /// We use it as a landing pad indicator in TechDemo4.
    /// WARNING
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next SSC update.
    /// This script has NOT been performance tested...
    /// </summary>
    [HelpURL("https://scsmmedia.com/ssc-documentation")]
    public class DemoSSCSequenceRenderers : MonoBehaviour
    {
        #region Public Variables


        public bool isInitialiseOnStart = false;

        [Header("Group 1")]
        [Range(0f, 10f)] public float group1OnDuration = 0.2f;
        [Range(0f, 10f)] public float group1Interval = 0f;
        [Range(0f, 30f)] public float group1ActivateDelay = 0f;
        [Range(0f, 30f)] public float group1DeactivateDelay = 0f;
        public bool group1StartOn = false;
        public MeshRenderer[] group1Renderers;

        [Header("Group 2")]
        [Range(0f, 10f)] public float group2OnDuration = 0.2f;
        [Range(0f, 10f)] public float group2Interval = 0f;
        [Range(0f, 30f)] public float group2ActivateDelay = 0f;
        [Range(0f, 30f)] public float group2DeactivateDelay = 0f;
        public bool group2StartOn = false;
        public MeshRenderer[] group2Renderers;

        [Header("Group 3")]
        [Range(0f, 10f)] public float group3OnDuration = 0.2f;
        [Range(0f, 10f)] public float group3Interval = 0f;
        [Range(0f, 30f)] public float group3ActivateDelay = 0f;
        [Range(0f, 30f)] public float group3DeactivateDelay = 0f;
        public bool group3StartOn = false;
        public MeshRenderer[] group3Renderers;

        #endregion

        #region Public Properties

        #endregion

        #region Public Static Variables

        #endregion

        #region Private Variables
        private bool isInitialised = false;
        private bool[] isGroupActive;
        private bool[] isGroupRendering;
        private bool[] isGroupValid;
        private int[] numRenderersArray;
        private int[] rendererIndex;
        private float[] groupTimer;
        #endregion

        #region Private Initialise Methods

        // Use this for initialization
        private void Start()
        {
            if (isInitialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Update Methods

        void Update()
        {
            if (isInitialised)
            {
                // Loop through all the groups
                for (int groupNumber = 1; groupNumber < 4; groupNumber++)
                {
                    // Is this group active?
                    if (isGroupActive[groupNumber - 1])
                    {
                        CheckStatus(groupNumber);
                    }
                }
            }
        }

        #endregion

        #region Private and Internal Methods

        /// <summary>
        /// Check if a renderer should be on or off in the group.
        /// Assumes the groupNumber is valid.
        /// Assumes the group is active.
        /// </summary>
        /// <param name="groupNumber"></param>
        private void CheckStatus (int groupNumber)
        {
            float groupOnDuration = groupNumber == 1 ? group1OnDuration : groupNumber == 2 ? group2OnDuration : group3OnDuration;
            float groupInterval = groupNumber == 1 ? group1Interval : groupNumber == 2 ? group2Interval : group3Interval;

            int groupIdx = groupNumber - 1;

            groupTimer[groupIdx] += Time.deltaTime;

            if (isGroupRendering[groupIdx])
            {
                if (groupTimer[groupIdx] > groupOnDuration)
                {
                    groupTimer[groupIdx] = 0f;

                    DisableRenderer(groupNumber);
                }
            }
            else
            {
                if (groupTimer[groupIdx] > groupInterval)
                {
                    groupTimer[groupIdx] = 0f;
                    EnableRenderer(groupNumber);
                }
            }
        }

        /// <summary>
        /// Invoke this method with a delay
        /// </summary>
        private void DisableGroup1()
        {
            EnableGroup(1, false);
        }

        /// <summary>
        /// Invoke this method with a delay
        /// </summary>
        private void DisableGroup2()
        {
            EnableGroup(2, false);
        }

        /// <summary>
        /// Invoke this method with a delay
        /// </summary>
        private void DisableGroup3()
        {
            EnableGroup(3, false);
        }

        /// <summary>
        /// Disable the current renderer in the group
        /// Assumes the group is valid.
        /// </summary>
        /// <param name="groupNumber"></param>
        private void DisableRenderer(int groupNumber)
        {
            int numRenderersInGroup = numRenderersArray[groupNumber - 1];

            MeshRenderer[] groupRenderers = groupNumber == 1 ? group1Renderers : groupNumber == 2 ? group2Renderers : group3Renderers;

            int currentRenderer = rendererIndex[groupNumber - 1];

            if (currentRenderer >= 0 && currentRenderer < numRenderersInGroup)
            {
                groupRenderers[currentRenderer].enabled = false;
                isGroupRendering[groupNumber - 1] = false;
            }
        }

        /// <summary>
        /// Disable all renderers in a group.
        /// Assumes the groupNumber is valid
        /// </summary>
        /// <param name="groupNumber"></param>
        private void DisableRenderersAll(int groupNumber)
        {
            int numRenderersInGroup = numRenderersArray[groupNumber - 1];

            MeshRenderer[] groupRenderers = groupNumber == 1 ? group1Renderers : groupNumber == 2 ? group2Renderers : group3Renderers;

            for (int r = 0; r < numRenderersInGroup; r++)
            {
                MeshRenderer meshRenderer = groupRenderers[r];

                if (meshRenderer != null)
                {
                    meshRenderer.enabled = false;
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
            if (isInitialised && groupNumber > 0 && groupNumber < 4 && isGroupValid[groupNumber-1])
            {
                if (isEnabled) { EnableRenderer(groupNumber); }
                else { DisableRenderersAll(groupNumber); }

                // Remember if the group is active or inactive
                isGroupActive[groupNumber - 1] = isEnabled;

                // Groups that are active start in the rendering state.
                isGroupRendering[groupNumber - 1] = isEnabled;

                // Reset the group timer
                groupTimer[groupNumber - 1] = 0f;
            }
        }

        /// <summary>
        /// Invoke this method with a delay
        /// </summary>
        private void EnableGroup1()
        {
            EnableGroup(1, true);
        }

        /// <summary>
        /// Invoke this method with a delay
        /// </summary>
        private void EnableGroup2()
        {
            EnableGroup(2, true);
        }

        // Invoke this method with a delay
        private void EnableGroup3()
        {
            EnableGroup(3, true);
        }

        /// <summary>
        /// Enable the next renderer in the sequence for a group.
        /// Assumes the group number and the group is valid.
        /// </summary>
        /// <param name="groupNumber"></param>
        private void EnableRenderer(int groupNumber)
        {
            int groupIdx = groupNumber - 1;

            int numRenderersInGroup = numRenderersArray[groupIdx];

            // Get the next renderer
            rendererIndex[groupIdx]++;

            // Do we need to loop round to the start again?
            if (rendererIndex[groupIdx] >= numRenderersInGroup)
            {
                rendererIndex[groupIdx] = 0;
            }

            // Get the renderers for this group
            MeshRenderer[] groupRenderers = groupNumber == 1 ? group1Renderers : groupNumber == 2 ? group2Renderers : group3Renderers;

            // Get the renderer we wish to enable
            MeshRenderer mRen = groupRenderers[rendererIndex[groupIdx]];

            if (mRen != null)
            {
                mRen.enabled = true;
                isGroupRendering[groupIdx] = true;
            }
        }

        private bool ValidateGroup(int groupNumber)
        {
            isGroupValid[groupNumber - 1] = false;

            if (numRenderersArray != null)
            {
                isGroupValid[groupNumber - 1] = numRenderersArray[groupNumber - 1] > 0;
            }

            return isGroupValid[groupNumber - 1];
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Called automatically if isInitialiseOnStart is true. Otherwise, call it from your own game code.
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            isGroupActive = new bool[] { group1StartOn, group2StartOn, group3StartOn };
            isGroupRendering = new bool[] { group1StartOn, group2StartOn, group3StartOn };
            isGroupValid = new bool[] { false, false, false };

            // Current position in the array of renderers for each group (not set)
            rendererIndex = new int[] { -1, -1, -1 };

            groupTimer = new float[] { 0f, 0f, 0f };

            numRenderersArray = new int[]
                                     { group1Renderers == null ? 0 : group1Renderers.Length,
                                       group2Renderers == null ? 0 : group2Renderers.Length,
                                       group3Renderers == null ? 0 : group3Renderers.Length };

            // Check which groups have valid renderers
            ValidateGroup(1);
            ValidateGroup(2);
            ValidateGroup(3);

            // Are there any any valid groups?
            if (isGroupValid != null && isGroupValid.Length == 3 && (isGroupValid[0] || isGroupValid[1] || isGroupValid[2]))
            {
                isInitialised = true;

                // Start by disabling all renderers
                if (isGroupValid[0])
                {
                    DisableRenderersAll(1);
                    
                    if (group1StartOn) { TurnGroupOn(1); }
                }

                if (isGroupValid[1])
                {
                    DisableRenderersAll(2);

                    if (group2StartOn) { TurnGroupOn(2); }
                }

                if (isGroupValid[2])
                {
                    DisableRenderersAll(3);

                    if (group3StartOn) { TurnGroupOn(3); }
                }
            }
        }

        /// <summary>
        /// Start the group rendering. Valid numbers are between 1 and 3.
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
        /// Stop the group rendering. Valid numbers are between 1 and 3.
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