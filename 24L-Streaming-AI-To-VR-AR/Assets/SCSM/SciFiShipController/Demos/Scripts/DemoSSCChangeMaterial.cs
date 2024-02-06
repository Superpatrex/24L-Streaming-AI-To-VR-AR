using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Simple demo script to change 1, 2 or 3 materials of an object, with alternate
    /// materials from 3 material arrays.
    /// We use it to change lift control panel materials in TechDemo3.
    /// WARNING
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next SSC update.
    /// This script has NOT been performance tested...
    /// </summary>
    public class DemoSSCChangeMaterial : MonoBehaviour
    {
        #region Public Variables
        public MeshRenderer objectMeshRenderer = null;

        [Header("Group1 Material")]
        [Range(0, 9)] public int group1MatElementIndex = 0;
        public Material[] group1Materials;

        [Header("Group2 Material")]
        [Range(0, 9)] public int group2MatElementIndex = 1;
        public Material[] group2Materials;

        [Header("Group3 Material")]
        [Range(0, 9)] public int group3MatElementIndex = 2;
        public Material[] group3Materials;
        #endregion

        #region Private Variables
        private bool isInitialised = false;
        private Material[] materialAray;
        private int numMaterialsOnObject = 0;
        private bool[] isGroupValid;
        private int[] currentGroupMatIndex;
        private int[] numMaterialsArray;
        #endregion

        #region Initialise Methods

        // Start is called before the first frame update
        void Start()
        {
            if (objectMeshRenderer == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR DemoSSCChangeMaterial - could not find mesh renderer for " + name);
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
                    Debug.LogWarning("ERROR DemoSSCChangeMaterial - could not find any materials on the mesh renderer (" + objectMeshRenderer.name + ") for " + name);
                    #endif
                }
                // Are there any materials to change?
                else if (numMaterialsArray != null && numMaterialsArray.Length == 3 && (numMaterialsArray[0] > 0 || numMaterialsArray[1] > 0 || numMaterialsArray[2] > 0))
                {
                    isGroupValid = new bool[] { false, false, false };

                    ValidateGroup(1);
                    ValidateGroup(2);
                    ValidateGroup(3);

                    // At least one of the groups must be valid
                    if (isGroupValid[0] || isGroupValid[1] && isGroupValid[2])
                    {
                        currentGroupMatIndex = new int[] { 0, 0, 0 };

                        isInitialised = true;
                    }
                }
            }
        }

        private bool ValidateGroup(int groupNumber)
        {
            int groupMatElementIndex = groupNumber == 1 ? group1MatElementIndex : groupNumber == 2 ? group2MatElementIndex : group3MatElementIndex;

            isGroupValid[groupNumber - 1] = numMaterialsArray[groupNumber - 1] > 0 && groupMatElementIndex >= 0 && groupMatElementIndex < numMaterialsOnObject;

            if (!isGroupValid[groupNumber - 1] && numMaterialsArray[groupNumber - 1] > 0 && (groupMatElementIndex < 0 || groupMatElementIndex > numMaterialsOnObject - 1))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR DemoSSCChangeMaterial - group" + groupNumber + "MatElementIndex for " + name + " must be in the range 0 to " + (numMaterialsOnObject - 1).ToString());
                #endif
            }

            return isGroupValid[groupNumber - 1];
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get the next material for this groups
        /// </summary>
        /// <param name="groupNumber"></param>
        private void GetGroupNextMaterial(int groupNumber, bool isReverseOrder)
        {
            if (isInitialised && groupNumber > 0 && groupNumber < 4 && objectMeshRenderer != null && isGroupValid[groupNumber - 1])
            {
                if (isReverseOrder)
                {
                    // Get the previous material in the list we are cycling through
                    --currentGroupMatIndex[groupNumber - 1];
                    // If before the start of the array of materials to change, go to the end
                    if (currentGroupMatIndex[groupNumber - 1] < 0) { currentGroupMatIndex[groupNumber - 1] = numMaterialsArray[groupNumber - 1] - 1; }
                }
                else
                {

                    // Get the next material in the list we are cycling through
                    ++currentGroupMatIndex[groupNumber - 1];
                    // If past the end of the array of materials to change, go back to the start
                    if (currentGroupMatIndex[groupNumber - 1] > numMaterialsArray[groupNumber - 1] - 1) { currentGroupMatIndex[groupNumber - 1] = 0; }
                }

                Material mat = groupNumber == 1 ? group1Materials[currentGroupMatIndex[groupNumber - 1]] :
                               groupNumber == 2 ? group2Materials[currentGroupMatIndex[groupNumber - 1]] :
                               group3Materials[currentGroupMatIndex[groupNumber - 1]];

                SwitchMaterial(groupNumber, mat);
            }
        }

        /// <summary>
        /// Attempt to switch or replace the material on the mesh renderer
        /// for this instance of the object in the scene.
        /// </summary>
        /// <param name="material"></param>
        private void SwitchMaterial(int groupNumber, Material material)
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
        /// Replace the current material for Group 1 with the zero-based material element in the array.
        /// </summary>
        /// <param name="materialElementIndex"></param>
        public void GetGroup1Material (int materialElementIndex)
        {
            if (isInitialised && materialElementIndex >= 0 && materialElementIndex < numMaterialsArray[0] && isGroupValid[0])
            {
                currentGroupMatIndex[0] = materialElementIndex;
                SwitchMaterial(1, group1Materials[materialElementIndex]);
            }
        }

        /// <summary>
        /// Replace the current material for Group 2 with the zero-based material element in the array.
        /// </summary>
        /// <param name="materialElementIndex"></param>
        public void GetGroup2Material (int materialElementIndex)
        {
            if (isInitialised && materialElementIndex >= 0 && materialElementIndex < numMaterialsArray[1] && isGroupValid[1])
            {
                currentGroupMatIndex[1] = materialElementIndex;
                SwitchMaterial(2, group2Materials[materialElementIndex]);
            }
        }

        /// <summary>
        /// Replace the current material for Group 3 with the zero-based material element in the array.
        /// </summary>
        /// <param name="materialElementIndex"></param>
        public void GetGroup3Material(int materialElementIndex)
        {
            if (isInitialised && materialElementIndex >= 0 && materialElementIndex < numMaterialsArray[2] && isGroupValid[2])
            {
                currentGroupMatIndex[2] = materialElementIndex;
                SwitchMaterial(3, group1Materials[materialElementIndex]);
            }
        }

        /// <summary>
        /// Replace the current material for Group 1 with the next one in the array.
        /// </summary>
        public void GetNextGroup1Material()
        {
            GetGroupNextMaterial(1, false);
        }

        /// <summary>
        /// Replace the current material for Group 2 with the next one in the array.
        /// </summary>
        public void GetNextGroup2Material()
        {
            GetGroupNextMaterial(2, false);
        }

        /// <summary>
        /// Replace the current material for Group 3 with the next one in the array.
        /// </summary>
        public void GetNextGroup3Material()
        {
            GetGroupNextMaterial(3, false);
        }

        /// <summary>
        /// Replace the current material for Group 1 with the previous one in the array.
        /// </summary>
        public void GetPreviousGroup1Material()
        {
            GetGroupNextMaterial(1, true);
        }

        /// <summary>
        /// Replace the current material for Group 2 with the previous one in the array.
        /// </summary>
        public void GetPreviousGroup2Material()
        {
            GetGroupNextMaterial(2, true);
        }

        /// <summary>
        /// Replace the current material for Group 3 with the previous one in the array.
        /// </summary>
        public void GetPreviousGroup3Material()
        {
            GetGroupNextMaterial(3, true);
        }

        #endregion
    }
}