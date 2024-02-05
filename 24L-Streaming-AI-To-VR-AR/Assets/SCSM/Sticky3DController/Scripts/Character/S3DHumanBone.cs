using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Weighted human bone used with a humanoid rig
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    [System.Serializable]
    public class S3DHumanBone
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        [HideInInspector] public int guidHash;
        public HumanBodyBones bone;

        /// <summary>
        /// The amount of influence this bone has in calculation.
        /// </summary>
        [Tooltip("The amount of influence this bone has in calculation.")]
        [Range(0f,1f)] public float weight;

        /// <summary>
        /// Is this a paired bone. E.g. If LeftShoulder, assume the pair is the RightShoulder.
        /// </summary>
        [Tooltip("Is this a paired bone. E.g. If LeftShoulder, assume the pair is the RightShoulder.")]
        public bool isPaired;

        /// <summary>
        /// When aiming a weapon in FPS, the bone may pitch up and down in the wrong direction.
        /// When the local bone rotation is modified in OnAnimatorIK it may need to be flipped.
        /// This isn't required in LateUpdate().
        /// </summary>
        [Tooltip("If required, is the pitch (when aiming a weapon) flipped?")]
        public bool isFlipPitch;

        [System.NonSerialized] public Transform boneTransform;
        [System.NonSerialized] public Transform boneTransformTwin;
        [System.NonSerialized] public HumanBodyBones boneTwin;
        [System.NonSerialized] public bool isValid;
        [System.NonSerialized] public bool isTwinValid;
        /// <summary>
        /// The current pitch angle being applied to this bone for aiming. This is used for calculation in
        /// AimBoneAtTarget() in StickyControlModule.
        /// </summary>
        [System.NonSerialized] public float currentPitchAngle;
        #endregion

        #region Public Properties

        #endregion

        #region Public Static Variables

        #endregion

        #region Private Variables - General

        #endregion

        #region Constructors
        public S3DHumanBone()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="s3dHumanBone"></param>
        public S3DHumanBone (S3DHumanBone s3dHumanBone)
        {
            if (s3dHumanBone == null) { SetClassDefaults(); }
            else
            {
                bone = s3dHumanBone.bone;
                weight = s3dHumanBone.weight;
                isPaired = s3dHumanBone.isPaired;
                boneTransform = s3dHumanBone.boneTransform;
                guidHash = s3dHumanBone.guidHash;
                isValid = s3dHumanBone.isValid;
                isTwinValid = s3dHumanBone.isTwinValid;
                isFlipPitch = s3dHumanBone.isFlipPitch;
            }
        }

        #endregion

        #region Private Member Methods
        public void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            bone = HumanBodyBones.Spine;
            weight = 1f;
            isPaired = false;
            boneTransform = null;
            isValid = false;
            isTwinValid = false;
            currentPitchAngle = 0f;
            isFlipPitch = false;
        }

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Get the twin or paired bone of a given bone. Returns true if a matching
        /// bone is found, otherwise false.
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="twin"></param>
        /// <returns></returns>
        public static bool GetTwin (HumanBodyBones bone, ref HumanBodyBones twin)
        {
            bool isPair = true;

            twin = HumanBodyBones.Hips;

            switch (bone)
            {
                case HumanBodyBones.LeftShoulder: twin = HumanBodyBones.RightShoulder; break;
                case HumanBodyBones.RightShoulder: twin = HumanBodyBones.LeftShoulder; break;
                case HumanBodyBones.LeftUpperArm: twin = HumanBodyBones.RightUpperArm; break;
                case HumanBodyBones.RightUpperArm: twin = HumanBodyBones.LeftUpperArm; break;
                case HumanBodyBones.LeftLowerArm: twin = HumanBodyBones.RightLowerArm; break;
                case HumanBodyBones.RightLowerArm: twin = HumanBodyBones.LeftLowerArm; break;
                case HumanBodyBones.LeftUpperLeg: twin = HumanBodyBones.RightUpperLeg; break;
                case HumanBodyBones.RightUpperLeg: twin = HumanBodyBones.LeftUpperLeg; break;
                case HumanBodyBones.LeftLowerLeg: twin = HumanBodyBones.RightLowerLeg; break;
                case HumanBodyBones.RightLowerLeg: twin = HumanBodyBones.LeftLowerLeg; break;
                case HumanBodyBones.LeftHand: twin = HumanBodyBones.RightHand; break;
                case HumanBodyBones.RightHand: twin = HumanBodyBones.LeftHand; break;
                case HumanBodyBones.LeftFoot: twin = HumanBodyBones.RightFoot; break;
                case HumanBodyBones.RightFoot: twin = HumanBodyBones.LeftFoot; break;
                case HumanBodyBones.LeftToes: twin = HumanBodyBones.RightToes; break;
                case HumanBodyBones.RightToes: twin = HumanBodyBones.LeftToes; break;
                case HumanBodyBones.LeftEye: twin = HumanBodyBones.RightEye; break;
                case HumanBodyBones.RightEye: twin = HumanBodyBones.LeftEye; break;
                default: isPair = false; break;
            }

            return isPair;
        }

        #endregion
    }
}