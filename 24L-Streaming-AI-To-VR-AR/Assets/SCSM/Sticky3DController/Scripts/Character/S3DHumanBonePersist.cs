using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Bone data used with a humanoid rig. This includes
    /// more persistent data than S3DHumanBone.
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    [System.Serializable]
    public class S3DHumanBonePersist
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        [HideInInspector] public int guidHash;
        public HumanBodyBones bone;
        public Transform boneTransform;
        public Collider boneCollider;
        [System.NonSerialized] public float relativeMass;

        /// <summary>
        /// The class instance is valid when it has a boneTransform, boneCollider, and the guidHash is set.
        /// </summary>
        [System.NonSerialized] public bool isValid;
        #endregion

        #region Public Properties

        #endregion

        #region Public Static Variables

        #endregion

        #region Private Variables - General

        #endregion

        #region Constructors
        public S3DHumanBonePersist()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Constructor currently used for ragdoll
        /// </summary>
        /// <param name="humanBone"></param>
        /// <param name="transform"></param>
        public S3DHumanBonePersist(HumanBodyBones humanBone, Transform transform)
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            isValid = false;
            bone = humanBone;
            boneTransform = transform;
            boneCollider = null;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="s3dHumanBone"></param>
        public S3DHumanBonePersist(S3DHumanBonePersist s3dHumanBone)
        {
            if (s3dHumanBone == null) { SetClassDefaults(); }
            else
            {
                bone = s3dHumanBone.bone;
                boneTransform = s3dHumanBone.boneTransform;
                boneCollider = s3dHumanBone.boneCollider;
                guidHash = s3dHumanBone.guidHash;
                isValid = s3dHumanBone.isValid;
                relativeMass = s3dHumanBone.relativeMass;
            }
        }

        #endregion

        #region Private Member Methods
        public void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            bone = HumanBodyBones.Hips;
            boneTransform = null;
            boneCollider = null;
            isValid = false;
            relativeMass = 1f;
        }

        #endregion

        #region Public API Methods - General

        #endregion
    }
}