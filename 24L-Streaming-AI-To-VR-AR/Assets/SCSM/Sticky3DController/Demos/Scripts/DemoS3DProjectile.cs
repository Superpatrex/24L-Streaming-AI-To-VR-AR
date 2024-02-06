using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Used in GravityShooterDemo scene. Simple component used when firing projectiles
    /// with a collider and a rigidbody.
    /// WARNING
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class DemoS3DProjectile : MonoBehaviour
    {
        #region Public Varaibles
        /// <summary>
        /// The projectile will be automatically despawned after this amount of time (in seconds) has elapsed.
        /// </summary>
        public float despawnTime = 3f;

        #endregion

        #region Private Variables

        private float despawnTimer = 0f;

        #endregion

        #region Update Methods

        private void Update()
        {
            despawnTimer += Time.deltaTime;

            if (despawnTimer > despawnTime)
            {
                DestroyProjectile();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Destroy this projectile
        /// </summary>
        public void DestroyProjectile()
        {
            Destroy(gameObject);
        }

        #endregion
    }
}
