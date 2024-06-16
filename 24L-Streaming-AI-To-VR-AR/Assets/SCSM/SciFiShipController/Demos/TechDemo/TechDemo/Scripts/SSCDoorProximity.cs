using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Component to be used with the SSCDoorAnimator component to trigger when doors are locked or unlocked.
    /// Place it on a gameobject with a single trigger collider.
    /// Either the gameobject of this component, or the item that enters the trigger area must have a rigidbody
    /// attached, otherwise the OnTriggerEnter/Exit/Stay will not be called.
    /// The rigidbody can be set to isKinematic.
    /// WARNING: Beware of using multiple colliders on the same gameobject as this component. This can lead
    /// to unwanted behaviour like getting Enter/Exit/Stay events when you least expect it.
    /// For example, a box non-trigger collider on this gameobject way cause a OnTriggerEnter event
    /// to fire when say a sphere trigger collider has been configured but an object with a trigger collider
    /// has entered the space of the box collider.
    /// The reason this occurs is that Unity creates a compound collider for all colliders on the same gameobject.
    /// The solution is to move the trigger collider (and this component) to a child gameobject.
    /// </summary>
    public class SSCDoorProximity : MonoBehaviour
    {
        #region Public Variables

        public SSCDoorAnimator sscDoorAnimator;

        [Tooltip("Array of zero-based door indexes from the SSCDoorAnimator to control")]
        public int[] doorIndexes;

        [Tooltip("Should the door(s) be unlocked when an object enters the collider area?")]
        public bool isUnlockDoorsOnEntry = true;

        [Tooltip("Should the door(s) be openned when an object enters the collider area? Will have no effect on locked doors.")]
        public bool isOpenDoorsOnEntry = true;

        [Tooltip("Should the door(s) be closed when an object exits the collider area? Will have no effect on locked doors.")]
        public bool isCloseDoorsOnExit = true;

        [Tooltip("Should the door(s) be locked when an object exits the collider area?")]
        public bool isLockDoorsOnExit = true;

        [Tooltip("Array of Unity Tags for objects that affect this collider area. If none are provided, all objects can affect this area. NOTE: All tags MUST exist.")]
        public string[] tags = new string[] {};

        #endregion

        #region Private Variables
        private Collider proximityCollider = null;
        private bool isInitialised = false;
        private int numTags = 0;

        #endregion

        #region Initialisation Methods

        // Start is called before the first frame update
        void Start()
        {
            if (sscDoorAnimator != null)
            {
                // Find the first trigger collider
                Collider[] colliders = GetComponents<Collider>();

                foreach (Collider collider in colliders)
                {
                    if (collider.isTrigger)
                    {
                        proximityCollider = collider;
                        break;
                    }
                }

                if (proximityCollider != null)
                {
                    ValidateTags();
                    isInitialised = true;
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("[ERROR] SSCDoorProximity could not find a (trigger) collider component. Did you attach one to this gameobject?");
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("[ERROR] SSCDoorProximity could not find SSCDoorAnimator component for " + gameObject.name);
            }
            #endif
        }

        /// <summary>
        /// Removes any empty or null tags. NOTE: May increase GC so don't use each frame.
        /// </summary>
        private void ValidateTags()
        {
            numTags = tags == null ? 0 : tags.Length;

            if (numTags > 0)
            {
                List<string> tagList = new List<string>(tags);

                for (int tgIdx = numTags - 1; tgIdx >= 0; tgIdx--)
                {
                    // Remove invalid tag
                    if (string.IsNullOrEmpty(tagList[tgIdx])) { tagList.RemoveAt(tgIdx); }
                }

                // If there were invalid entries, update the array
                if (tagList.Count != numTags)
                {
                    tags = tagList.ToArray();
                    numTags = tags == null ? 0 : tags.Length;
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Does the gameobject have a tag that matches the array configured by the user.
        /// Will return true if a match is found OR there are no tags configured.
        /// </summary>
        /// <param name="objectGameObject"></param>
        /// <returns></returns>
        private bool IsObjectTagMatched(GameObject objectGameObject)
        {
            if (!isInitialised) { return false; }

            if (objectGameObject == null) { return false; }
            else if (numTags < 1) { return true; }          
            else
            {
                bool isMatch = false;
                for (int tgIdx = 0; tgIdx < numTags; tgIdx++)
                {
                    if (objectGameObject.CompareTag(tags[tgIdx]))
                    {
                        isMatch = true;
                        break;
                    }
                }
                return isMatch;
            }
        }

        #endregion

        #region Event Methods

        private void OnTriggerEnter(Collider other)
        {
            if (isInitialised)
            {
                int numDoors = GetNumberOfDoors();

                if (numDoors > 0 && IsObjectTagMatched(other.gameObject))
                {
                    //Debug.Log("[DEBUG] trigger enter " + proximityCollider.name + " other: " + other.name + " other tag: " + other.gameObject.tag + " T:" + Time.time);

                    for (int dIdx = 0; dIdx < numDoors; dIdx++)
                    {
                        int doorNumber = doorIndexes[dIdx];

                        if (isUnlockDoorsOnEntry)
                        {
                            sscDoorAnimator.UnlockDoor(doorNumber);
                        }

                        if (isOpenDoorsOnEntry)
                        {
                            sscDoorAnimator.OpenDoors(doorNumber);
                        }
                    }
                }
            }

            //Debug.Log("[DEBUG] trigger enter " + proximityCollider.name + " other: " + other.name + " T:" + Time.time);
        }

        private void OnTriggerStay(Collider other)
        {
            if (isInitialised)
            {
                //Debug.Log("[DEBUG] trigger stay " + proximityCollider.name + " other: " + other.name + " T:" + Time.time);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (isInitialised)
            {
                int numDoors = GetNumberOfDoors();

                if (numDoors > 0 && IsObjectTagMatched(other.gameObject))
                {
                    //Debug.Log("[DEBUG] trigger exit " + proximityCollider.name + " other: " + other.name + " T:" + Time.time);

                    for (int dIdx = 0; dIdx < numDoors; dIdx++)
                    {
                        int doorNumber = doorIndexes[dIdx];

                        if (isCloseDoorsOnExit)
                        {
                            sscDoorAnimator.CloseDoors(doorNumber);
                        }

                        if (isLockDoorsOnExit)
                        {
                            sscDoorAnimator.LockDoor(doorNumber);
                        }
                    }
                }
            }

            //Debug.Log("[DEBUG] trigger exit " + proximityCollider.name + " other: " + other.name + " T:" + Time.time);
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Get the number of doors or sets of doors affect by this proximity component
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfDoors()
        {
            return doorIndexes == null ? 0 : doorIndexes.Length;
        }

        #endregion
    }
}