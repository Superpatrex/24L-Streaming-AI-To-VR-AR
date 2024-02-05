using UnityEngine;

// Sticky3D Control Module Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [System.Serializable]
    public class S3DAnimCondition
    {
        #region Enumerations

        public enum ActionCondition
        {
            IsGrounded = 0,
            IsStepping = 1,
            IsCrouching = 3,
            IsIdle = 5,
            IsJetPacking = 7,
            IsWalking = 20,
            IsWalkingForward = 21,
            IsWalkingBackward = 22,
            IsWalkingOrStrafing = 23,
            IsWalkingOrSprinting = 24,
            IsSprinting = 30,
            IsStrafing = 40,
            IsStrafingRight = 41,
            IsStrafingLeft = 42,
            HasLanded = 50,
            HasJumped = 51,
            IsClimbing = 60,
            IsClimbingAtTop = 61,
            IsSprintInput = 200,
            IsStrafeInput = 201,
            IsWalkInput = 202,
            IsLookLeftInput = 220,
            IsLookRightInput = 221,
            IsLookDownInput = 222,
            IsLookUpInput = 223,
            IsLookWhileIdle = 224,
            IsLookLeftOrRightWhileIdle = 225,
            IsLookUpOrDownWhileIdle = 226,
            IsWeaponInAnyHand = 300,
            IsWeaponInLeftHand = 301,
            IsWeaponInRightHand = 302,
            IsMagazineInLeftHand = 303,
            IsMagazineInRightHand = 304
        }

        public enum ActionWeaponCondition
        {
            HasFired1 = 0,
            HasFired2 = 1,
            HasEmptyFired1 = 2,
            HasEmptyFired2 = 3,
            HasReloadStarted1 = 10,
            HasReloadStarted2 = 11,
            HasReloadFinished1 = 12,
            HasReloadFinished2 = 13,
            IsHeld = 16,
            IsReloading1 = 20,
            IsReloading2 = 21,
            IsReloadUnequipping1 = 22,
            IsReloadUnequipping2 = 23,
            IsReloadEquipping1 = 24,
            IsReloadEquipping2 = 25
        }

        public enum ConditionType
        {
            AND = 0,
            NOT = 1
        }

        #endregion

        #region Public Static 
        public static int ConditionTypeAndInt = (int)ConditionType.AND;
        public static int ConditionTypeNotInt = (int)ConditionType.NOT;
        #endregion

        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        public ActionCondition actionCondition;
        public ActionWeaponCondition actionWeaponCondition;
        public ConditionType conditionType;

        public bool showInEditor;

        #endregion

        #region Constructors
        public S3DAnimCondition()
        {
            SetClassDefaults();
        }

        public S3DAnimCondition(S3DAnimCondition s3dAnimCondition)
        {
            if (s3dAnimCondition == null) { SetClassDefaults(); }
            else
            {
                actionCondition = s3dAnimCondition.actionCondition;
                actionWeaponCondition = s3dAnimCondition.actionWeaponCondition;
                conditionType = s3dAnimCondition.conditionType;
                showInEditor = s3dAnimCondition.showInEditor;
            }
        }

        #endregion

        #region Private Member Methods
        public void SetClassDefaults()
        {
            showInEditor = true;
            actionCondition = 0;
            actionWeaponCondition = 0;
            conditionType = ConditionType.AND;
        }

        #endregion
    }
}