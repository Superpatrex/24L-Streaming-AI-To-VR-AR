using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A non-serializable class used to pass Foot IK data around at runtime
    /// </summary>
    public class S3DFootData
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        public AvatarIKGoal foot;

        public Vector3 currentFootIKPos;
        public Vector3 prevFootIKPos;
        public Quaternion currentFootIKRot;
        public Quaternion prevFootIKRot;
        public Vector3 prevFootIKRotLS;

        /// <summary>
        /// This is the world space position where the foot was in the previous LateUpdate.
        /// Default Vector3.zero.
        /// </summary>
        public Vector3 lastKnownFootPos;

        /// <summary>
        /// This is the world space rotation of the foot during the previous LateUpdate.
        /// Default Quaternion.identity.
        /// </summary>
        public Quaternion lastKnownFootRot;

        // mostly static
        public float footToSoleDist;

        /// <summary>
        /// The rotation of the foot, in local space, when the character is first initialised.
        /// </summary>
        public Quaternion startFootRotLS;

        #endregion

        #region Constructors
        public S3DFootData()
        {
            SetClassDefaults();
        }

        #endregion

        #region Public Member Methods
        public void SetClassDefaults()
        {
            foot = AvatarIKGoal.LeftFoot;

            currentFootIKPos = Vector3.zero;
            currentFootIKRot = Quaternion.identity;
            prevFootIKPos = Vector3.zero;
            prevFootIKRot = Quaternion.identity;
            prevFootIKRotLS = Vector3.zero;
            lastKnownFootPos = Vector3.zero;
            lastKnownFootRot = Quaternion.identity;

            footToSoleDist = 0f;
            startFootRotLS = Quaternion.identity;

            //currentFootIKWeight = 0f;
            //blendWeight = 0f;
        }

        #endregion
    }
}
