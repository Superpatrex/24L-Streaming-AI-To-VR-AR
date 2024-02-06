using System.Collections.Generic;
using UnityEngine;

// Sticky3D Control Module Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [System.Serializable]
    public class S3DAnimAction
    {
        #region Enumerations

        public enum StandardAction
        {
            Walk = 0,
            Sprint = 1,
            Strafe = 2,
            Jump = 3,
            Crouch = 4,
            Land = 5,
            Move = 6,
            Look = 7,
            Climb = 8,
            //Sit = 9,
            Custom = 99
        }

        public enum WeaponAction
        {
            Fire1 = 0,
            Fire2 = 1,
            Held = 3,
            Aim = 4,
            Reload1 = 10,
            Reload2 = 11,
            Dropped = 41,
            Equipped = 42,
            Socketed = 43,
            Stashed = 44,
            Custom = 99
        }

        public enum ParameterType
        {
            None = 0,
            Bool = 1,
            Trigger = 2,
            Float = 3,
            Integer = 4
        }

        /// <summary>
        /// Possible bool values that can be passed to a character animation controller.
        /// Typically indicate a state of the character.
        /// </summary>
        public enum ActionBoolValue
        {
            FixedValue = -1,
            IsGrounded = 0,
            IsStepping = 1,
            IsSteppingDown = 2,
            IsCrouching = 3,
            IsIdle = 5,
            IsJetPacking = 7,
            IsGroundedOrJetPacking = 8,
            IsGroundedOrClimbing = 9,
            IsGroundedOrClimbingOrSteppingDown = 10,
            IsGroundedOrSteppingDown = 12,
            IsWalking = 20,
            IsWalkingForward = 21,
            IsWalkingBackward = 22,
            IsWalkingOrStrafing = 23,
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
            IsLookUpOrDownWhileIdle = 226
        }

        /// <summary>
        /// Possible trigger values that can be passed to a character animation controller.
        /// These tend to be single events
        /// </summary>
        public enum ActionTriggerValue
        {
            Crouch = 0,
            Land = 1,
            Jump = 2
        }

        /// <summary>
        /// Possible integer values that can be passed to a character animation controller.
        /// </summary>
        public enum ActionIntegerValue
        {
            None = 0
        }

        /// <summary>
        /// Possible float values that can be passed to a character animation controller.
        /// These could be continuously changing values, like movement speed.
        /// </summary>
        public enum ActionFloatValue
        {
            FixedValue = -1,
            MovingSpeed = 0,
            MovingForwardSpeed = 1,
            MovingBackwardSpeed = 2,
            MovingForwardBackSpeed = 3,
            MovingDirectionX = 6,
            MovingDirectionY = 7,
            MovingDirectionZ = 8,
            MovingSpeedInvN = 9,
            MovingSpeedN = 10,
            CrouchSpeed = 12,
            CrouchAmount = 13,
            WalkSpeed = 14,
            SprintSpeed = 15,
            StrafeSpeed = 16,
            JumpSpeed = 17,
            ClimbSpeed = 18,
            SprintingSpeed = 25,
            StrafingSpeed = 40,
            StrafingRightSpeed = 41,
            StrafingLeftSpeed = 42,
            StrafingSpeedN = 43,
            TurningSpeed = 50,
            WalkingSpeed = 70,
            WalkingForwardSpeed = 71,
            WalkingBackwardSpeed = 72,
            MovementInputX = 210,
            MovementInputY = 211,
            MovementInputZ = 212,
            MovementInputMagnitude = 213,
            LookHorizontalInput = 221,
            LookVerticalInput = 222,
            LookZoomInput = 223,
            LookOrbitInput = 224
        }

        /// <summary>
        /// Possible bool values that can be passed to a weapon animation controller.
        /// </summary>
        public enum ActionWeaponBoolValue
        {
            FixedValue = -1,
            HasFired1 = 0,
            HasFired2 = 1,
            HasEmptyFired1 = 2,
            HasEmptyFired2 = 3,
            HasReloadStarted1 = 10,
            HasReloadStarted2 = 11,
            HasReloadFinished1 = 12,
            HasReloadFinished2 = 13,
            IsReloading1 = 20,
            IsReloading2 = 21,
            IsHeld = 30,
            IsAiming = 31
        }

        /// <summary>
        /// Possible float values that can be passed to a weapon animation controller.
        /// </summary>
        public enum ActionWeaponFloatValue
        {
            FixedValue = -1,
            ReloadDuration1 = 0,
            ReloadDuration2 = 1,
            ReloadDelay1 = 2,
            ReloadDelay2 = 3
        }

        /// <summary>
        /// Possible trigger values that can be passed to a weapon animation controller.
        /// </summary>
        public enum ActionWeaponTriggerValue
        {
            HasFired1 = 0,
            HasFired2 = 1,
            HasEmptyFired1 = 2,
            HasEmptyFired2 = 3,
            HasReloadStarted1 = 10,
            HasReloadStarted2 = 11,
            HasReloadFinished1 = 12,
            HasReloadFinished2 = 13
        }

        /// <summary>
        /// Possible integer values that can be passed to a weapon animation controller.
        /// </summary>
        public enum ActionWeaponIntegerValue
        {
            FixedValue = -1,
            None = 0
        }

        #endregion

        #region Public Static 

        public static int StandardActionWalkInt = (int)StandardAction.Walk;
        public static int StandardActionSprintInt = (int)StandardAction.Sprint;
        public static int StandardActionJumpInt = (int)StandardAction.Jump;
        public static int StandardActionCrouchInt = (int)StandardAction.Crouch;
        public static int StandardActionLandInt = (int)StandardAction.Land;
        public static int StandardActionMoveInt = (int)StandardAction.Move;
        public static int StandardActionCustomInt = (int)StandardAction.Custom;

        public static int WeaponActionFire1Int = (int)WeaponAction.Fire1;
        public static int WeaponActionFire2Int = (int)WeaponAction.Fire2;
        public static int WeaponActionAimInt = (int)WeaponAction.Aim;
        public static int WeaponActionReload1Int = (int)WeaponAction.Reload1;
        public static int WeaponActionReload2Int = (int)WeaponAction.Reload2;
        public static int WeaponActionDroppedInt = (int)WeaponAction.Dropped;
        public static int WeaponActionEquippedInt = (int)WeaponAction.Equipped;
        public static int WeaponActionSocketedInt = (int)WeaponAction.Socketed;
        public static int WeaponActionStashedInt = (int)WeaponAction.Stashed;
        public static int WeaponActionCustomInt = (int)WeaponAction.Custom;

        public static int ParameterTypeNoneInt = (int)ParameterType.None;
        public static int ParameterTypeBoolInt = (int)ParameterType.Bool;
        public static int ParameterTypeTriggerInt = (int)ParameterType.Trigger;
        public static int ParameterTypeFloatInt = (int)ParameterType.Float;
        public static int ParameterTypeIntegerInt = (int)ParameterType.Integer;

        public static int ActionFloatValueFixedInt = (int)ActionFloatValue.FixedValue;
        public static int ActionBoolValueFixedInt = (int)ActionBoolValue.FixedValue;

        public static int ActionWeaponFloatValueFixedInt = (int)ActionWeaponFloatValue.FixedValue;
        public static int ActionWeaponBoolValueFixedInt = (int)ActionWeaponBoolValue.FixedValue;

        #endregion

        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// This is the action that happens that causes the animation to take place.
        /// It helps you remember why you set it up. It only applies to characters.
        /// </summary>
        public StandardAction standardAction;

        /// <summary>
        /// This is the action that happens that causes the weapon animation to take place.
        /// It helps you remember why you set it up. It only applies to weapons.
        /// </summary>
        public WeaponAction weaponAction;

        public ParameterType parameterType;

        public int paramHashCode;

        public ActionBoolValue actionBoolValue;
        public ActionFloatValue actionFloatValue;
        public ActionTriggerValue actionTriggerValue;
        public ActionIntegerValue actionIntegerValue;

        public ActionWeaponBoolValue actionWeaponBoolValue;
        public ActionWeaponFloatValue actionWeaponFloatValue;
        public ActionWeaponTriggerValue actionWeaponTriggerValue;
        public ActionWeaponIntegerValue actionWeaponIntegerValue;

        /// <summary>
        /// A value that is used to multiple or change the value
        /// of the float value being passed to the animation controller.
        /// </summary>
        [Range(0.01f, 25f)] public float floatMultiplier;

        /// <summary>
        /// A fixed float value that can be passed to the animation controller.
        /// Typically used with a condition.
        /// </summary>
        [Range(-1f, 1f)] public float fixedFloatValue;

        /// <summary>
        /// A true or false value that can be passed to the animation controller.
        /// Typically uses with a condition.
        /// </summary>
        public bool fixedBoolValue;

        /// <summary>
        /// Currently used for weapon actions like dropped, equipped, socketed or stashed to trigger
        /// an immediate exit from an Animation Layer while holding a weapon.
        /// </summary>
        public bool isExitAction;

        /// <summary>
        /// Works with bool types. When the value is true, use false instead. When the value is false, use true instead.
        /// </summary>
        public bool isInvert;

        /// <summary>
        /// Works with bool custom anim actions to toggle the existing parameter value in the animator controller.
        /// Cannot be used with isInvert is true.
        /// </summary>
        public bool isToggle;

        /// <summary>
        /// Works with bool custom anim actions to reset to false after it has been sent to the animator controller.
        /// Has no effect if isToggle is true. [DEFAULT: True]
        /// </summary>
        public bool isResetCustomAfterUse;

        /// <summary>
        /// The damping applied to help smooth transitions, especially with Blend Trees. Currently only used for floats.
        /// For quick transitions to the new float value use a low damping value, for the slower transitions use more damping.
        /// </summary>
        [Range(0f, 1f)] public float damping;

        public List<S3DAnimCondition> s3dAnimConditionList;

        public bool showInEditor;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Hashed GUID code to uniquely identify an AnimAction instance.
        /// </summary>
        public int guidHash;

        #endregion

        #region Internal Variables
        [System.NonSerialized] internal int numConditions;

        // Internal only used with StandardAction and WeaponAction of Custom
        [System.NonSerialized] internal bool customActionBoolValue;
        [System.NonSerialized] internal float customActionFloatValue;
        [System.NonSerialized] internal bool customActionTriggerValue;
        [System.NonSerialized] internal int customActionIntegerValue;

        #endregion

        #region Constructors
        public S3DAnimAction()
        {
            SetClassDefaults();
        }

        public S3DAnimAction(S3DAnimAction s3dAnimAction)
        {
            if (s3dAnimAction == null) { SetClassDefaults(); }
            else
            {
                guidHash = s3dAnimAction.guidHash;
                showInEditor = s3dAnimAction.showInEditor;
                standardAction = s3dAnimAction.standardAction;
                weaponAction = s3dAnimAction.weaponAction;
                parameterType = s3dAnimAction.parameterType;
                paramHashCode = s3dAnimAction.paramHashCode;
                actionBoolValue = s3dAnimAction.actionBoolValue;
                actionFloatValue = s3dAnimAction.actionFloatValue;
                actionTriggerValue = s3dAnimAction.actionTriggerValue;
                actionIntegerValue = s3dAnimAction.actionIntegerValue;

                actionWeaponBoolValue = s3dAnimAction.actionWeaponBoolValue;
                actionWeaponFloatValue = s3dAnimAction.actionWeaponFloatValue;
                actionWeaponTriggerValue = s3dAnimAction.actionWeaponTriggerValue;
                actionWeaponIntegerValue = s3dAnimAction.actionWeaponIntegerValue;

                floatMultiplier = s3dAnimAction.floatMultiplier;
                fixedFloatValue = s3dAnimAction.fixedFloatValue;
                fixedBoolValue = s3dAnimAction.fixedBoolValue;

                isExitAction = s3dAnimAction.isExitAction;

                isInvert = s3dAnimAction.isInvert;
                // Is toggle is not compatible with isInvert.
                isToggle = isInvert ? false : s3dAnimAction.isToggle;
                isResetCustomAfterUse = s3dAnimAction.isResetCustomAfterUse;

                damping = s3dAnimAction.damping;

                if (s3dAnimAction.s3dAnimConditionList == null) { this.s3dAnimConditionList = new List<S3DAnimCondition>(2); }
                else { this.s3dAnimConditionList = s3dAnimAction.s3dAnimConditionList.ConvertAll(cdn => new S3DAnimCondition(cdn)); }
            }
        }

        #endregion

        #region Private Member Methods
        public virtual void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            showInEditor = true;
            standardAction = StandardAction.Walk;
            weaponAction = WeaponAction.Fire1;
            parameterType = ParameterType.None;
            paramHashCode = 0;
            // Default to first value in the list
            actionBoolValue = 0;
            actionFloatValue = 0;
            actionTriggerValue = 0;
            actionIntegerValue = 0;
            actionWeaponBoolValue = 0;
            actionWeaponFloatValue = 0;
            actionWeaponTriggerValue = 0;
            actionWeaponIntegerValue = 0;
            floatMultiplier = 1f;
            fixedFloatValue = 0f;
            fixedBoolValue = false;
            isInvert = false;
            isToggle = false;
            isResetCustomAfterUse = true;
            damping = 0.1f;
            s3dAnimConditionList = new List<S3DAnimCondition>(2);
        }

        #endregion

        #region Public Static Methods

        #endregion
    }
}