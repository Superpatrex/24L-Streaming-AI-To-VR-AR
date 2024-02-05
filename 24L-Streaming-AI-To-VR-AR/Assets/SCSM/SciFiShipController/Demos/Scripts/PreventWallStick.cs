using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This will nudge a the ship away from a collider (like a wall) if it is
    /// constant contact with it for a specified duration. Attach this component
    /// to a ShipControlModule.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PreventWallStick : MonoBehaviour
    {
        #region Public variables
        public float maxStickTime = 0.25f;
        public float wallPushVelocity = 5f;
        #endregion

        #region Private variables
        private float wallStickTimer = 0f;
        private int totalTouchingColliders = 0;
        private Rigidbody rBody;
        #endregion

        #region Initialisation Methods
        void Awake()
        {
            rBody = GetComponent<Rigidbody>();
        }
        #endregion

        #region Event Methods
        private void OnCollisionStay(Collision collision)
        {
            // If we are touching other colliders for too long, push us away from them
            wallStickTimer += Time.deltaTime;
            if (wallStickTimer > maxStickTime && rBody != null)
            {
                #if UNITY_2018_3_OR_NEWER
                rBody.AddForce(collision.GetContact(0).normal * wallPushVelocity, ForceMode.VelocityChange);
                #else
                ContactPoint[] contacts = collision.contacts;
                int numContactPoints = contacts == null ? 0 : contacts.Length;
                if (numContactPoints > 0)
                {
                    rBody.AddForce(contacts[0].normal * wallPushVelocity, ForceMode.VelocityChange);
                }
                #endif
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Every time a collider enters, increment the number of colliders we are touching
            totalTouchingColliders++;
        }

        private void OnCollisionExit(Collision collision)
        {
            // Every time a collider exits, decrement the number of colliders we are touching
            totalTouchingColliders--;
            if (totalTouchingColliders < 1)
            {
                if (totalTouchingColliders < 0)
                {
                    totalTouchingColliders = 0;
                    #if UNITY_EDITOR
                    Debug.Log("[DEBUG] Colliders < 0");
                    #endif
                }
                wallStickTimer = 0f;
            }
        }
        #endregion
    }
}
