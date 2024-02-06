using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Demo script to cycle through two or more materials on an object.
    /// WARNING
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// This script has NOT been performance tested...
    /// </summary>
    public class DemoS3DCycleMaterials : MonoBehaviour
    {
        #region Public Variables
        public MeshRenderer objectMeshRenderer = null;

        [Header("Group1 Material")]
        [Range(0,9)] public int group1MatElementIndex = 0;
        [Range(0.01f, 10f)] public float group1SwitchInterval = 0.1f;
        [Range(0f, 30f)] public float group1ActivateDelay = 0f;
        [Range(0f, 30f)] public float group1DeactivateDelay = 0f;
        public bool group1StartOn = false;
        public Material[] group1Materials;

        [Header("Group2 Material")]
        [Range(0, 9)] public int group2MatElementIndex = 1;
        [Range(0.01f, 10f)] public float group2SwitchInterval = 0.1f;
        [Range(0f, 30f)] public float group2ActivateDelay = 0f;
        [Range(0f, 30f)] public float group2DeactivateDelay = 0f;
        public bool group2StartOn = false;
        public Material[] group2Materials;

        [Header("Group3 Material")]
        [Range(0, 9)] public int group3MatElementIndex = 2;
        [Range(0.01f, 10f)] public float group3SwitchInterval = 0.1f;
        [Range(0f, 30f)] public float group3ActivateDelay = 0f;
        [Range(0f, 30f)] public float group3DeactivateDelay = 0f;
        public bool group3StartOn = false;
        public Material[] group3Materials;

        #endregion

        #region Private Variables
        private bool isInitialised = false;
        private Material[] materialAray;
        private int numMaterialsOnObject = 0;
        private bool[] isGroupActive;
        private bool[] isGroupValid;
        private int[] currentGroupMatIndex;
        private int[] numMaterialsArray;
        private float[] groupTimer;

        #endregion

        #region Initialise Methods

        // Start is called before the first frame update
        void Start()
        {
            if (objectMeshRenderer == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR Demo3DCycleMaterials - could not find mesh renderer for " + name);
                #endif
            }
            else
            {
                materialAray = objectMeshRenderer.materials;

                numMaterialsOnObject = materialAray == null ? 0 : materialAray.Length;

                numMaterialsArray = new int[]
                                         { group1Materials == null ? 0 : group1Materials.Length,
                                       group2Materials == null ? 0 : group2Materials.Length,
                                       group3Materials == null ? 0 : group3Materials.Length };

                if (numMaterialsOnObject < 1)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR Demo3DCycleMaterials - could not find any materials on the mesh renderer (" + objectMeshRenderer.name + ") for " + name);
                    #endif
                }
                // Are there any materials to switch?
                else if (numMaterialsArray != null && numMaterialsArray.Length == 3 && (numMaterialsArray[0] > 0 || numMaterialsArray[1] > 0 || numMaterialsArray[2] > 0))
                {
                    isGroupValid = new bool[] { false, false, false };

                    ValidateGroup(1);
                    ValidateGroup(2);
                    ValidateGroup(3);

                    // At least one of the groups must be valid
                    if (isGroupValid[0] || isGroupValid[1] && isGroupValid[2])
                    {

                        groupTimer = new float[] { 0f, 0f, 0f };
                        currentGroupMatIndex = new int[] { 0, 0, 0 };
                        isGroupActive = new bool[] { group1StartOn, group2StartOn, group3StartOn };

                        isInitialised = true;

                        EnableGroup(1, group1StartOn);
                        EnableGroup(2, group2StartOn);
                        EnableGroup(3, group3StartOn);
                    }
                }
            }
        }


        private bool ValidateGroup(int groupNumber)
        {
            int groupMatElementIndex = groupNumber == 1 ? group1MatElementIndex : groupNumber == 2 ? group2MatElementIndex : group3MatElementIndex;

            isGroupValid[groupNumber-1] = numMaterialsArray[groupNumber - 1] > 0 && groupMatElementIndex >= 0 && groupMatElementIndex < numMaterialsOnObject;

            if (!isGroupValid[groupNumber - 1] && numMaterialsArray[groupNumber - 1] > 0 && (groupMatElementIndex < 0 || groupMatElementIndex > numMaterialsOnObject - 1))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR Demo3DCycleMaterials - group" + groupNumber + "MatElementIndex for " + name + " must be in the range 0 to " + (numMaterialsOnObject - 1).ToString());
                #endif
            }

            return isGroupValid[groupNumber - 1];
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
                        CheckSwitchingStatus(groupNumber);
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check if we need to switch any materials
        /// </summary>
        /// <param name="groupNumber"></param>
        private void CheckSwitchingStatus(int groupNumber)
        {
            float groupFlashInterval = groupNumber == 1 ? group1SwitchInterval : groupNumber == 2 ? group2SwitchInterval : group3SwitchInterval;

            if (groupFlashInterval > 0f && isGroupValid[groupNumber-1])
            {
                // If past the interval time, get the next material
                if (groupTimer[groupNumber-1] > groupFlashInterval)
                {
                    groupTimer[groupNumber-1] = 0f;

                    // Get the next material in the list we are cycling through
                    ++currentGroupMatIndex[groupNumber-1];
                    // If past the end of the array of materials to change, go back to the start
                    if (currentGroupMatIndex[groupNumber-1] > numMaterialsArray[groupNumber-1] - 1) { currentGroupMatIndex[groupNumber-1] = 0; }

                    Material mat = groupNumber == 1 ? group1Materials[currentGroupMatIndex[groupNumber - 1]] :
                                   groupNumber == 2 ? group2Materials[currentGroupMatIndex[groupNumber - 1]] :
                                   group3Materials[currentGroupMatIndex[groupNumber - 1]];

                    SwitchMaterial(groupNumber, mat);
                }
                else
                {
                    // Increment the time for this group
                    groupTimer[groupNumber - 1] += Time.deltaTime;
                }
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
        /// Enable or disable the group to switch materials
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="isEnabled"></param>
        private void EnableGroup(int groupNumber, bool isEnabled)
        {
            if (isInitialised && groupNumber > 0 && groupNumber < 4 && isGroupValid[groupNumber-1])
            {
                // Remember if the group is active or inactive
                isGroupActive[groupNumber - 1] = isEnabled;

                // Reset the group timer
                groupTimer[groupNumber - 1] = 0f;
            }
        }

        /// <summary>
        /// Attempt to switch or replace the material on the mesh renderer
        /// for this instance of the object in the scene.
        /// </summary>
        /// <param name="material"></param>
        private void SwitchMaterial (int groupNumber, Material material)
        {
            if (isInitialised && groupNumber > 0 && groupNumber < 4 && material != null && objectMeshRenderer != null)
            {
                // Get the zero-based material element index we are going to replace on the renderer.
                int materialElementIndex = groupNumber == 1 ? group1MatElementIndex : groupNumber == 2 ? group2MatElementIndex : group3MatElementIndex;

                materialAray[materialElementIndex] = material;

                objectMeshRenderer.materials = materialAray;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the group switching materials. Valid numbers are between 1 and 3.
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
        /// Stop the group switching materials. Valid numbers are between 1 and 3.
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