using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Used in GravityShooterDemo scene to fire a rigidbody projectile from a character.
    /// Setup:
    /// 1. Add this script to a StickyControlModule gameobject in the scene
    /// 2. Add S3D_Projectile1 prefab from Demos\Prefabs to the slot provided
    /// WARNING
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    public class DemoS3DFireProjectile : MonoBehaviour
    {
        #region Public Variables
        public DemoS3DProjectile demoProjectile = null;
        public float projectileSpeed = 100f;

        /// <summary>
        /// The minimum time (in seconds) between consecutive firings of the weapon.
        /// </summary>
        [Range(0.05f, 10f)] public float reloadTime = 0.1f;

        /// <summary>
        /// Offset from the feet
        /// </summary>
        public Vector3 firePositionOffset = Vector3.zero;
        #endregion

        #region Private Variables
        private StickyControlModule stickyControlModule = null;
        private bool isInitialised = false;

        /// <summary>
        /// The time (in seconds) until this weapon can fire again.
        /// </summary>
        private float reloadTimer = 0f;
        #endregion

        #region Initialisation Methods

        // Start is called before the first frame update
        void Start()
        {
            stickyControlModule = GetComponent<StickyControlModule>();

            if (stickyControlModule != null) { isInitialised = true; }
        }

        #endregion

        #region Public Methods

        public void FireProjectile()
        {
            reloadTimer += Time.deltaTime;

            // Is weapon ready to fire?
            if (reloadTimer >= reloadTime)
            {
                reloadTimer = 0f;

                if (demoProjectile != null && isInitialised && stickyControlModule.IsInitialised)
                {
                    Vector3 firePosition = stickyControlModule.GetCurrentOffsetFromBottom(firePositionOffset);
                    Vector3 fireDirection = stickyControlModule.GetWorldLookDirection();

                    // In first person, the pitch up and down is controlled by the camera look direction.
                    // Add on the local y-axis offset.
                    firePosition += stickyControlModule.GetCurrentUp * fireDirection.y;

                    DemoS3DProjectile projectile = Instantiate(demoProjectile, firePosition, Quaternion.LookRotation(fireDirection, stickyControlModule.GetCurrentUp));

                    if (projectile != null)
                    {
                        projectile.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0f, 0f, projectileSpeed));
                    }
                }
            }
        }

        #endregion

    }
}