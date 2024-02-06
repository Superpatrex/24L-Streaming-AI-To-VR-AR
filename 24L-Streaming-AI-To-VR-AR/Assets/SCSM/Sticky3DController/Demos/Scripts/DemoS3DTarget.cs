using UnityEngine;
using System.Collections.Generic;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple target that can be hit by a Demo S3D_Projectile.
    /// Currently only works with 1 unit cubes.
    /// For use with a beam or projectile StickyWeapon, see Demo3DTarget2.
    /// WARNING
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    public class DemoS3DTarget : MonoBehaviour
    {
        #region Public Variables
        [Range(0f,0.99f)] public float accuracyRequired = 0.5f;
        public Vector3 targetDimensions = Vector3.one;
        #endregion

        #region Private Variables
        private ContactPoint[] contactPoints;
        private float hitDistX, hitDistY, hitDistZ;
        private float halfDimX, halfDimY, halfDimZ;
        private float sideX, sideY, sideZ;
        #endregion

        #region Initialisation Methods

        private void Start()
        {
            // Pre-allocate contact array
            contactPoints = new ContactPoint[10];

            // Validate input data
            Vector3 dimensions = S3DMath.Abs(targetDimensions);

            halfDimX = dimensions.x * 0.5f;
            halfDimY = dimensions.y * 0.5f;
            halfDimZ = dimensions.z * 0.5f;

            // How close we need to be to centre of each face
            // before we apply additional force.
            hitDistX = (1f - accuracyRequired) * halfDimX;
            hitDistY = (1f - accuracyRequired) * halfDimY;
            hitDistZ = (1f - accuracyRequired) * halfDimZ;

            // Contact points are not exactly accurate so we
            // cannot just use the distance from the centre to the side.
            sideX = halfDimX - 0.001f;
            sideY = halfDimY - 0.001f;
            sideZ = halfDimZ - 0.001f;
        }

        #endregion

        #region Events

        // Check when a collider has hit the target
        private void OnCollisionEnter(Collision collision)
        {
            Rigidbody rb = collision.rigidbody;

            // check if it the object has a rigidbody
            if (rb != null)
            {
                // Did a projectile hit the target?
                DemoS3DProjectile demoS3DProjectile = rb.GetComponent<DemoS3DProjectile>();

                if (demoS3DProjectile != null)
                {
                    // Where did we hit the target?
                    int numContacts = collision.GetContacts(contactPoints);

                    if (numContacts > 0)
                    {
                        // Just look at the first point of contact
                        ContactPoint contactPoint = contactPoints[0];

                        // Convert into local space
                        Vector3 pt = Quaternion.Inverse(transform.rotation) * (contactPoint.point - transform.position);

                        // Was there a hit within the target zone?
                        if ((pt.x < -sideX && pt.y > -hitDistY && pt.y < hitDistY && pt.z > -hitDistZ && pt.z < hitDistZ) ||
                            (pt.x > sideX && pt.y > -hitDistY && pt.y < hitDistY && pt.z > -hitDistZ && pt.z < hitDistZ) ||
                            (pt.y < -sideY && pt.x > -hitDistX && pt.x < hitDistX && pt.z > -hitDistZ && pt.z < hitDistZ) ||
                            (pt.y > sideY && pt.x > -hitDistX && pt.x < hitDistX && pt.z > -hitDistZ && pt.z < hitDistZ) ||
                            (pt.z < -sideZ && pt.y > -hitDistY && pt.y < hitDistY && pt.x > -hitDistX && pt.x < hitDistX) ||
                            (pt.z > sideZ && pt.y > -hitDistY && pt.y < hitDistY && pt.x > -hitDistX && pt.x < hitDistX)
                        )
                        {
                            // Apply extra force
                            rb.AddForceAtPosition(demoS3DProjectile.transform.forward * 2000f, contactPoint.point);
                        }
                    }
                }
            }
        }

        #endregion
    }
}