using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple script to detect when another object has collided with this object.
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// Setup:
    /// 1. Add to a gameobject in the scene
    /// 2. Add a collider or trigger collider to the object
    /// 3. Add a Sticky3D Controller prefab to the scene
    /// 4. On S3D, on the Collider tab, tick "On Trigger Enter/Exit"
    /// 5. Run the scene and bump into the object with the S3D character.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Detect Collision")]
    public class SampleDetectCollision : MonoBehaviour
    {
        #region Public Variables
        public bool isCheckForPlayerTag = true;

        #endregion

        #region Private variables
        private const string playerTagName = "Player";

        #endregion

        #region Collision Events
        // NOTE: OnCollision events are only raised with non-Kinematic rigidbodies. S3D is a kinematic controller.

        private void OnCollisionEnter(Collision collision)
        {
            if (!isCheckForPlayerTag || (isCheckForPlayerTag && collision.gameObject.CompareTag(playerTagName)))
            {
                Debug.Log("[INFO] " + collision.transform.name + " has hit " + this.name + " at time: " + Time.time);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (!isCheckForPlayerTag || (isCheckForPlayerTag && collision.gameObject.CompareTag(playerTagName)))
            {
                Debug.Log("[INFO] " + collision.transform.name + " is still colliding with the collider of " + this.name + " at time: " + Time.time);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!isCheckForPlayerTag || (isCheckForPlayerTag && collision.gameObject.CompareTag(playerTagName)))
            {
                Debug.Log("[INFO] " + collision.transform.name + " has stopped colliding with the collider of " + this.name + " at time: " + Time.time);
            }
        }

        #endregion

        #region Trigger Events

        private void OnTriggerEnter(Collider other)
        {
            if (!isCheckForPlayerTag || (isCheckForPlayerTag && other.gameObject.CompareTag(playerTagName)))
            {
                Debug.Log("[INFO] " + other.name + " is entering the trigger collider of " + this.name + " at time: " + Time.time);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!isCheckForPlayerTag || (isCheckForPlayerTag && other.gameObject.CompareTag(playerTagName)))
            {
                Debug.Log("[INFO] " + other.name + " is staying inside the trigger collider of " + this.name + " at time: " + Time.time);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!isCheckForPlayerTag || (isCheckForPlayerTag && other.gameObject.CompareTag(playerTagName)))
            {
                Debug.Log("[INFO] " + other.name + " is leaving the trigger collider of " + this.name + " at time: " + Time.time);
            }
        }

        #endregion
    }
}