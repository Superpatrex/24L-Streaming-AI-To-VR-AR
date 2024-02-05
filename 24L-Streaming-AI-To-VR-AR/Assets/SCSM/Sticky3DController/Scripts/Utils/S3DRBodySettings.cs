using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class for storing rigidbody settings.
    /// Always check isSaved before using values to restore settings to a rigidbody.
    /// When settings become stale, set isSaved back to false.
    /// </summary>
    public class S3DRBodySettings
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// Indicates if the settings have been updated
        /// </summary>
        public bool isSaved;

        public CollisionDetectionMode rBodyColDectionMode = CollisionDetectionMode.Discrete;
        public float rBodyDrag = 0f;
        public float rBodyAngularDrag = 0f;
        public bool rBodyIsKinematic = false;
        public bool rBodyUseGravity = false;

        #endregion

        #region Constructors
        public S3DRBodySettings()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public S3DRBodySettings (S3DRBodySettings s3dRBodySettings)
        {
            if (s3dRBodySettings == null) { SetClassDefaults(); }
            else
            {
                isSaved = s3dRBodySettings.isSaved;

                rBodyColDectionMode = s3dRBodySettings.rBodyColDectionMode;
                rBodyDrag = s3dRBodySettings.rBodyDrag;
                rBodyAngularDrag = s3dRBodySettings.rBodyAngularDrag;
                rBodyIsKinematic = s3dRBodySettings.rBodyIsKinematic;
                rBodyUseGravity = s3dRBodySettings.rBodyUseGravity;
            }
        }

        #endregion

        #region Public API Methods - General

        public void SetClassDefaults()
        {
            isSaved = false;

            rBodyColDectionMode = CollisionDetectionMode.Discrete;
            rBodyDrag = 0f;
            rBodyAngularDrag = 0f;
            rBodyIsKinematic = false;
            rBodyUseGravity = false;
        }

        /// <summary>
        /// Restore the current rigidbody settings.
        /// You should always check if isSaved is true before
        /// calling this as values may have become stale.
        /// </summary>
        /// <param name="rBody"></param>
        public void RestoreSettings (Rigidbody rBody)
        {
            if (rBody != null)
            {
                // If currently kinematic and setting to non-kinematic,
                // apply kinematic BEFORE setting the collisionDectionMode,
                // as that new mode may not be compatible with kinematic.
                if (rBody.isKinematic && !rBodyIsKinematic)
                {
                    rBody.isKinematic = rBodyIsKinematic;
                    rBody.collisionDetectionMode = rBodyColDectionMode;
                    rBody.drag = rBodyDrag;
                    rBody.angularDrag = rBodyAngularDrag;
                    rBody.useGravity = rBodyUseGravity;
                }
                else
                {
                    rBody.collisionDetectionMode = rBodyColDectionMode;
                    rBody.drag = rBodyDrag;
                    rBody.angularDrag = rBodyAngularDrag;
                    rBody.isKinematic = rBodyIsKinematic;
                    rBody.useGravity = rBodyUseGravity;
                }
            }
        }

        /// <summary>
        /// Save the current rigidbody settings
        /// </summary>
        /// <param name="rBody"></param>
        public void SaveSettings (Rigidbody rBody)
        {
            if (rBody != null)
            {
                rBodyColDectionMode = rBody.collisionDetectionMode;
                rBodyDrag = rBody.drag;
                rBodyAngularDrag = rBody.angularDrag;
                rBodyIsKinematic = rBody.isKinematic;
                rBodyUseGravity = rBody.useGravity;

                isSaved = true;
            }
        }


        #endregion

    }
}