using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Place this script on a gameobject with a trigger collider to call your methods in other components
    /// in the scene when another collider enters or exits the trigger collider area.
    /// WARNING: Beware of using multiple colliders on the same gameobject as this component. This can lead
    /// to unwanted behaviour like getting Enter/Exit/Stay events when you least expect it.
    /// For example, a box non-trigger collider on this gameobject way cause a OnTriggerEnter event
    /// to fire when say a sphere trigger collider has been configured but an object with a trigger collider
    /// has entered the space of the box collider.
    /// The reason this occurs is that Unity creates a compound collider for all colliders on the same gameobject.
    /// The solution is to move the trigger collider (and this component) to a child gameobject.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Objects/Sticky Proximity")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class S3DProximity : MonoBehaviour
    {
        #region Public Variables
        public bool initialiseOnStart = false;

        /// <summary>
        /// Array of Unity Tags for objects that affect this collider area. If none are provided, all objects can affect this area. NOTE: All tags MUST exist.
        /// </summary>
        public string[] tags = new string[] { };

        /// <summary>
        /// Methods that get called when a collider enters the trigger area
        /// </summary>
        public S3DProximityEvt onEnterMethods = null;

        /// <summary>
        /// Methods that get called when a collider exits the trigger area
        /// </summary>
        public S3DProximityEvt onExitMethods = null;
        #endregion

        #region Private Variables
        private Collider proximityCollider = null;
        private bool isInitialised = false;
        private int numTags = 0;
        #endregion

        #region Initialise Methods
        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
            else
            {
                // Disable the collider
                proximityCollider = GetComponent<Collider>();

                if (proximityCollider != null && proximityCollider.isTrigger && proximityCollider.enabled)
                {
                    proximityCollider.enabled = false;
                }
            }
        }

        #endregion

        #region Events

        private void OnTriggerEnter(Collider other)
        {
            if (isInitialised && onEnterMethods != null && IsObjectTagMatched(other.gameObject))
            {
                // TODO find the bounds intersect point between trigger and other collider

                onEnterMethods.Invoke(Vector3.zero, other.gameObject.GetInstanceID(), false);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (isInitialised && onExitMethods != null && IsObjectTagMatched(other.gameObject))
            {
                onExitMethods.Invoke(Vector3.zero, other.gameObject.GetInstanceID(), false);
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

        #region Public Methods

        /// <summary>
        /// Call this to manually initialise the component
        /// </summary>
        public void Initialise()
        {
            if (!isInitialised)
            {
                proximityCollider = GetComponent<Collider>();

                if (proximityCollider == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("[ERROR] S3DProximity could not find a (trigger) collider component. Did you attach one to the " + name + " gameobject?");
                    #endif
                }
                else if (!proximityCollider.isTrigger)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("[ERROR] S3DProximity - the collider is not a trigger on " + name);
                    #endif
                }
                else
                {
                    ValidateTags();
                    isInitialised = true;

                    if (!proximityCollider.enabled) { proximityCollider.enabled = true; }
                }
            }
        }

        #endregion
    }
}