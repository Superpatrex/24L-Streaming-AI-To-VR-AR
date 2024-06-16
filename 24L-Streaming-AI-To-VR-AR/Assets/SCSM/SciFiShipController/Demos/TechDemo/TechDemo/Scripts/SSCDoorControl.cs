using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Component to be used with the SSCDoorAnimator component to open, close or lock a door.
    /// Typically, this should be added to the gameobject that contains your door control panel model. 
    /// </summary>
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    [DisallowMultipleComponent]
    public class SSCDoorControl : MonoBehaviour
    {
        #region Public Variables

        [Tooltip("If you want to initialise at a particular point in your game code, set this off, otherwise set it on")]
        public bool isInitialiseOnStart = false;

        [Tooltip("The SSCDoorAnimator component that controls how the door(s) behave")]
        public SSCDoorAnimator sscDoorAnimator = null;

        [Tooltip("Array of zero-based door indexes from the SSCDoorAnimator to control")]
        public int[] doorIndexes;

        [Tooltip("This is the mesh renderer for the device or button(s) that open, close, or lock the door(s).")]
        public MeshRenderer panelMeshRenderer = null;

        [Tooltip("The zero-based index of the material on the mesh renderer which will be replaced when buttons are activated")]
        [Range(0, 9)] public int materialElementIndex = 0;

        [Tooltip("Will call the SSCDoorAnimator component to attempt to open all the doors when the Open button is selected")]
        public bool openDoorsOnSelect = true;

        [Tooltip("Will call the SSCDoorAnimator component to attempt to close all the doors when the Close button is selected")]
        public bool closeDoorsOnSelect = true;

        [Tooltip("Will call the SSCDoorAnimator component to attempt to lock or unlock all the doors when the Lock button is selected")]
        public bool lockDoorsOnSelect = true;

        [Header("Activation Materials")]
        public Material idleMaterial = null;
        public Material openMaterial = null;
        public Material closeMaterial = null;
        public Material lockMaterial = null;
        public Material lockedMaterial = null;
        #endregion

        #region Private Variables
        private bool isInitialised = false;
        private List<Material> materialList = null;
        private int numMaterials = 0;
        private int numDoors = 0;
        #endregion

        #region Initialisation Methods

        // Start is called before the first frame update
        void Start()
        {
            if (isInitialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check if the door is locked
        /// </summary>
        /// <returns></returns>
        private bool GetLockedStatus(int doorIndex)
        {
            return isInitialised && sscDoorAnimator != null && sscDoorAnimator.IsInitialised && sscDoorAnimator.IsDoorLockedByIndex(doorIndex);
        }

        /// <summary>
        /// Check if the first door is open
        /// </summary>
        /// <returns></returns>
        private bool GetIsOpen()
        {          
            return isInitialised && numDoors > 0 && sscDoorAnimator != null && sscDoorAnimator.IsInitialised && sscDoorAnimator.IsDoorOpenByIndex(doorIndexes[0]);
        }

        /// <summary>
        /// Attempt to switch or replace the material on the mesh renderer
        /// for this instance of the object in the scene.
        /// </summary>
        /// <param name="material"></param>
        private void SwitchMaterial(Material material)
        {
            if (isInitialised && material != null)
            {
                materialList[materialElementIndex] = material;

                if (materialElementIndex == 0)
                {
                    panelMeshRenderer.material = material;
                }
                else
                {
                    panelMeshRenderer.materials = materialList.ToArray();
                }
            }
        }

        #endregion

        #region Public API Member Methods

        /// <summary>
        /// Get the number of doors or sets of doors affect by this door control component
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfDoors()
        {
            return doorIndexes == null ? 0 : doorIndexes.Length;
        }

        /// <summary>
        /// Initialise the component. Either call this manually in code, or tick initialiseOnStart in the Unity Inspector
        /// </summary>
        public void Initialise()
        {
            // Only initialise once
            if (isInitialised) { return; }

            if (sscDoorAnimator == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] SSCDoorControl (" + name + ") could not find SSCDoorAnimator component");
                #endif
            }
            else if (panelMeshRenderer == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] SSCDoorControl could not find panelMeshRenderer");
                #endif
            }
            else if (materialElementIndex < 0 || materialElementIndex > 9)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] SSCDoorControl (" + name + ") - the material index is invalid");
                #endif
            }
            else
            {
                materialList = new List<Material>(2);
                panelMeshRenderer.GetMaterials(materialList);

                numMaterials = materialList == null ? 0 : materialList.Count;
                numDoors = GetNumberOfDoors();

                if (materialElementIndex >= numMaterials)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("[ERROR] SSCDoorControl (" + name + ") - the material index is invalid. Number of materials on renderer: " + numMaterials + ". Zero-based Index specified: " + materialElementIndex);
                    #endif
                }
                else if (numDoors < 1)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("[ERROR] SSCDoorControl (" + name + ") - you must specify at least 1 door index from the SSCDoorAnimator to control.");
                    #endif
                }
                else
                {
                    isInitialised = true;

                    UpdateLockStatus();
                }
            }
        }

        /// <summary>
        /// Select the close button
        /// </summary>
        public void SelectClose()
        {
            SelectClose(false);
        }

        /// <summary>
        /// Select the close button
        /// </summary>
        public void SelectClose(bool isCheckIfUnlocked)
        {
            if (isInitialised)
            {
                if (!isCheckIfUnlocked || sscDoorAnimator == null || sscDoorAnimator.IsAnyDoorUnlocked())
                {
                    SwitchMaterial(closeMaterial);
                }

                if (closeDoorsOnSelect && sscDoorAnimator != null)
                {
                    // Close all the doors being controlled by this button
                    for (int dIdx = 0; dIdx < numDoors; dIdx++)
                    {
                        sscDoorAnimator.CloseDoors(doorIndexes[dIdx]);
                    }
                }
            }
        }

        /// <summary>
        /// Select the lock button
        /// </summary>
        public void SelectLock()
        {
            if (isInitialised)
            {
                SwitchMaterial(lockMaterial);

                if (lockDoorsOnSelect && sscDoorAnimator != null)
                {
                    // Are the door(s) locked?
                    if (numDoors > 0 && GetLockedStatus(doorIndexes[0]))
                    {
                        // Unlock all the doors being controlled by this button
                        for (int dIdx = 0; dIdx < numDoors; dIdx++)
                        {
                            sscDoorAnimator.UnlockDoor(doorIndexes[dIdx]);
                        }

                        ShowUnlocked();
                    }
                    else
                    {
                        sscDoorAnimator.LockDoors();
                        ShowLocked();
                    }
                }
            }
        }

        /// <summary>
        /// Select the open button
        /// </summary>
        public void SelectOpen()
        {
            SelectOpen(false);
        }

        /// <summary>
        /// Select the open button if there are no doors or at least one is unlocked.
        /// </summary>
        /// <param name="isCheckIfUnlocked"></param>
        public void SelectOpen(bool isCheckIfUnlocked)
        {
            if (isInitialised)
            {
                if (!isCheckIfUnlocked || sscDoorAnimator == null || sscDoorAnimator.IsAnyDoorUnlocked())
                {
                    SwitchMaterial(openMaterial);
                }

                if (openDoorsOnSelect && sscDoorAnimator != null)
                {
                    // Open all the doors being controlled by this button
                    for (int dIdx = 0; dIdx < numDoors; dIdx++)
                    {
                        sscDoorAnimator.OpenDoors(doorIndexes[dIdx]);
                    }
                }
            }
        }

        /// <summary>
        /// Select the Open or Close button depending on if the doors
        /// are currently open. Will also check if any door is unlocked.
        /// </summary>
        public void SelectToggle()
        {
            if (isInitialised)
            {
                if (GetIsOpen()) { SelectClose(true); }
                else { SelectOpen(true); }
            }
        }

        /// <summary>
        /// Indicate that the door is locked
        /// </summary>
        public void ShowLocked()
        {
            if (isInitialised)
            {
                SwitchMaterial(lockedMaterial);
            }
        }

        /// <summary>
        /// Indicate that the door is unlocked
        /// </summary>
        public void ShowUnlocked()
        {
            if (isInitialised)
            {
                SwitchMaterial(idleMaterial);
            }
        }

        /// <summary>
        /// Check the first door and update the material to idle or locked
        /// </summary>
        public void UpdateLockStatus()
        {
            if (isInitialised)
            {
                if (GetLockedStatus(doorIndexes[0])) { ShowLocked(); }
                else { ShowUnlocked(); }
            }
        }

        #endregion

        #region Public API Static Methods

        /// <summary>
        /// Select the close button on a Door Control device 
        /// </summary>
        /// <param name="sscDoorControl"></param>
        public static void SelectClose (SSCDoorControl sscDoorControl)
        {
            if (sscDoorControl != null) { sscDoorControl.SelectClose(true); }
        }

        /// <summary>
        /// Select the lock button on a Door Control device 
        /// </summary>
        /// <param name="sscDoorControl"></param>
        public static void SelectLock (SSCDoorControl sscDoorControl)
        {
            if (sscDoorControl != null) { sscDoorControl.SelectLock(); }
        }

        /// <summary>
        /// Select the open button on a Door Control device 
        /// </summary>
        /// <param name="sscDoorControl"></param>
        public static void SelectOpen (SSCDoorControl sscDoorControl)
        {
            if (sscDoorControl != null) { sscDoorControl.SelectOpen(true); }
        }

        /// <summary>
        /// Select the Open or Close button on a Door Control device depending
        /// on if the doors are currently open.
        /// </summary>
        /// <param name="sscDoorControl"></param>
        public static void SelectToggle (SSCDoorControl sscDoorControl)
        {
            if (sscDoorControl != null) { sscDoorControl.SelectToggle(); }
        }

        /// <summary>
        /// Indicate that a door is locked on a Door Control device 
        /// </summary>
        /// <param name="sscDoorControl"></param>
        public static void ShowLocked(SSCDoorControl sscDoorControl)
        {
            if (sscDoorControl != null) { sscDoorControl.ShowLocked(); }
        }

        /// <summary>
        /// Indicate that a door is unlocked on a Door Control device 
        /// </summary>
        /// <param name="sscDoorControl"></param>
        public static void ShowUnlocked(SSCDoorControl sscDoorControl)
        {
            if (sscDoorControl != null) { sscDoorControl.ShowUnlocked(); }
        }

        #endregion
    }
}