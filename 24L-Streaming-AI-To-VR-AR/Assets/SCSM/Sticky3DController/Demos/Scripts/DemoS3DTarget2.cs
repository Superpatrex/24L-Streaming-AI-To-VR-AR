using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Shows how Dynamic Objects can receive fire from a StickyWeapon.
    /// See Demos\Prefabs\DynamicObjects\StickyDynamicTarget1.prefab and
    /// Demos\Scripts\SampleHitDynamicObject.cs.
    /// WARNING
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    [RequireComponent(typeof(StickyDamageReceiver), typeof(StickyDynamicModule))]
    [DisallowMultipleComponent]
    public class DemoS3DTarget2 : MonoBehaviour
    {
        #region Public Variables
        [Range(0f, 0.99f)] public float accuracyRequired = 0.5f;
        public Vector3 targetDimensions = Vector3.one;
        public float health = 100f;
        #endregion

        #region Public Properties

        #endregion

        #region Public Static Variables

        #endregion

        #region Private Variables - General

        private StickyDynamicModule stickyDynamicModule = null;
        private StickyDamageReceiver stickyDamageReceiver = null;
        private StickyManager stickyManager = null;

        private float hitDistX, hitDistY, hitDistZ;
        private float halfDimX, halfDimY, halfDimZ;
        private float sideX, sideY, sideZ;

        private bool isInitialised = false;

        #endregion

        #region Initialisation Methods

        private void Start()
        {
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

            if (!TryGetComponent(out stickyDynamicModule))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR Demo3DTarget2 could not find attached StickyDynamicModule");
                #endif
            }
            else if (!TryGetComponent(out stickyDamageReceiver))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR Demo3DTarget2 could not find attached StickyDamageReceiver");
                #endif
            }
            else
            {
                stickyManager = StickyManager.GetOrCreateManager(gameObject.scene.handle);
                stickyDamageReceiver.callbackOnHit = TakeDamage;

                isInitialised = true;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Destroy decals (return to the pool) where projectiles hit, then
        /// destroy (return to the pool) the dynamic object.
        /// </summary>
        private void DestroyTarget()
        {
            if (isInitialised)
            {
                // Remove any decals first
                stickyManager.DestroyDecals(gameObject);

                Debug.Log("[DEBUG] " + name + " destroyed!!");

                stickyDynamicModule.DestroyDynamicObject();
            }
        }

        #endregion

        #region Callback methods

        /// <summary>
        /// This gets automatically called when a weapon fires at, and hits this target
        /// </summary>
        /// <param name="s3dObjectHitParameters"></param>
        private void TakeDamage (S3DObjectHitParameters objhitParms)
        {
            if (isInitialised)
            {
                bool isHitByBeam = objhitParms.beamPrefab != null;
                bool isHitByProjectile = objhitParms.projectilePrefab != null;

                Vector3 hitPoint = objhitParms.hitInfo.point;

                // Convert the hit into local space
                Vector3 pt = Quaternion.Inverse(transform.rotation) * (hitPoint - transform.position);

                // Was there a hit within the target zone?
                if ((pt.x < -sideX && pt.y > -hitDistY && pt.y < hitDistY && pt.z > -hitDistZ && pt.z < hitDistZ) ||
                    (pt.x > sideX && pt.y > -hitDistY && pt.y < hitDistY && pt.z > -hitDistZ && pt.z < hitDistZ) ||
                    (pt.y < -sideY && pt.x > -hitDistX && pt.x < hitDistX && pt.z > -hitDistZ && pt.z < hitDistZ) ||
                    (pt.y > sideY && pt.x > -hitDistX && pt.x < hitDistX && pt.z > -hitDistZ && pt.z < hitDistZ) ||
                    (pt.z < -sideZ && pt.y > -hitDistY && pt.y < hitDistY && pt.x > -hitDistX && pt.x < hitDistX) ||
                    (pt.z > sideZ && pt.y > -hitDistY && pt.y < hitDistY && pt.x > -hitDistX && pt.x < hitDistX)
                )
                {
                    if (isHitByProjectile)
                    {
                        //StickyProjectileModule projectileModulePrefab = objhitParms.projectilePrefab;

                        health -= objhitParms.damageAmount;

                        if (health < 0f) { health = 0f; }

                        //Debug.Log("[DEBUG] DemoS3DTarget2 ammoType: " + (S3DAmmo.AmmoType)objhitParms.ammoTypeInt + " damage: " + damageAmount + " T:" + Time.time);
                    }
                    else
                    {
                        
                    }


                    Debug.Log("Hit " + name + " within the target! T:" + Time.time);

                    if (health == 0f)
                    {
                        DestroyTarget();
                    }
                }
                else
                {
                    Debug.Log("Hit " + name + " but not within the target zone... T:" + Time.time);
                }
            }
        }

        #endregion
    }
}