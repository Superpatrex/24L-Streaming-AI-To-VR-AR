using UnityEngine;

// Copyright (c) 2018-2021 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A non-serializable class used to pass Hand IK data around at runtime
    /// </summary>
    public class S3DHandData
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)
        
        public AvatarIKGoal hand;

        /// <summary>
        /// The approximate position on the character in local space at idle.
        /// i.e. with hands hanging down by the characters side.
        /// </summary>
        public Vector3 handIdlePositionLS;

        /// <summary>
        /// The approximate amount the hand is rotated at idle
        /// </summary>
        public Quaternion handIdleRotationLS;

        /// <summary>
        /// The target world-space position the hand should be reaching toward
        /// </summary>
        public Vector3 targetHandIKPos;

        /// <summary>
        /// The target world-space rotation the hand should be reaching toward
        /// </summary>
        public Quaternion targetHandIKRot;

        /// <summary>
        /// Local space offset from the target transform the hand should be reaching toward
        /// </summary>
        public Vector3 targetHandIKPosOffset;

        /// <summary>
        /// Local space rotation from the target rotation the hand should be reaching toward
        /// This is stored in degrees.
        /// </summary>
        public Vector3 targetHandIKRotMod;

        /// <summary>
        /// Previous Hand IK world space position
        /// </summary>
        public Vector3 prevHandIKPos;
        
        /// <summary>
        /// Previous Hand IK rotation
        /// </summary>
        public Quaternion prevHandIKRot;
        
        /// <summary>
        /// Current weight being applied to the hand ik position. This becomes the
        /// maximum weight that can be used for hand ik rotation.
        /// </summary>
        public float currentHandIKWeight;

        /// <summary>
        /// The actual Hand IK world space position we want to set. This is where
        /// we want the hand to be but it could be beyond the reach of the hand.
        /// This may NOT be the current hand position.
        /// </summary>
        public Vector3 currentHandIKPos;

        /// <summary>
        /// The actual Hand IK rotation we want to set
        /// </summary>
        public Quaternion currentHandIKRot;

        /// <summary>
        /// This is the world space position where the hand was in the previous LateUpdate.
        /// Default Vector3.zero.
        /// </summary>
        public Vector3 lastKnownHandPos;

        /// <summary>
        /// This is the world space rotation during the previous LateUpdate.
        /// Default Quaternion.identity.
        /// </summary>
        public Quaternion lastKnownHandRot;

        /// <summary>
        /// The world space direction to the target from the hand idle position.
        /// It is not normalised so can be used to get the distance using magitude or sqrMagnitude.
        /// Default is Vector3.forward
        /// </summary>
        public Vector3 idleDirToTarget;

        /// <summary>
        /// The world space direction to the target from the current palm position.
        /// It is not normalised so can be used to get the distance using magitude or sqrMagnitude.
        /// Default is Vector3.forward
        /// </summary>
        public Vector3 palmDirToTarget;

        public float blendWeight;

        /// <summary>
        /// Is the hand currently touching an interactive-enabled object with IsTouchable enabled?
        /// </summary>
        public bool isTouching;
        #endregion

        #region Constructors
        public S3DHandData ()
        {
            SetClassDefaults();
        }

        #endregion

        #region Private Member Methods
        public void SetClassDefaults()
        {
            hand = AvatarIKGoal.LeftHand;
            handIdlePositionLS = Vector3.zero;
            handIdleRotationLS = Quaternion.identity;
            targetHandIKPos = Vector3.zero;
            targetHandIKRot = Quaternion.identity;
            targetHandIKPosOffset = Vector3.zero;
            targetHandIKRotMod = Vector3.zero;
            prevHandIKPos = Vector3.zero;
            prevHandIKRot = Quaternion.identity;
            currentHandIKWeight = 0f;
            currentHandIKPos = Vector3.zero;
            currentHandIKRot = Quaternion.identity;
            lastKnownHandPos = Vector3.zero;
            lastKnownHandRot = Quaternion.identity;
            idleDirToTarget = Vector3.forward;
            palmDirToTarget = Vector3.forward;
            blendWeight = 0f;
            isTouching = false;
        }

        #endregion
    }
}