using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This is a very simple sample script to show how a Sticky3D character can
    /// be moved via code. It is NOT meant to be a replacement for a full NPC AI
    /// system. It doesn't include any kind of path finding or obstacle avoidance.
    /// For Teleporting - see stickyControlModule.TelePort(..).
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Move To")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class SampleMoveTo : MonoBehaviour
    {
        #region Public Variables


        public bool initialiseOnAwake = true;

        [Tooltip("How close to the target is considered to be at the target")]
        public float targetDistanceThreshold = 0.01f;

        [Tooltip("How close to the target rotation is considered to be facing the target direction")]
        public float targetAngleThreshold = 0.5f;

        [Tooltip("When the character is closer than this distance, the move speed will be reduced. Minimum value is 0.01.")]
        [Range(0.01f, 1f)] public float moveDistanceThreshold = 0.05f;

        [Tooltip("Angles below this threshold will reduce the turning speed. The value needs to be more than 10")]
        [Range(10f, 120f)] public float turnAngleThreshold = 60f;

        [Tooltip("The character max step up height during move or rotate procedure")]
        public float maxStepOffset = 0.5f;

        #endregion

        #region Public Properties

        public bool IsMoving { get { return isMoving; } }

        public bool IsRotating { get { return isRotating; } }

        /// <summary>
        /// When moving, the current distance to the target.
        /// </summary>
        public float DistanceToTarget { get { return isInitialised && isMoving ? Mathf.Sqrt(distanceToTargetSqr) : 0f; } }

        #endregion

        #region Public Static Variables

        #endregion

        #region Private Variables - General
        private bool isInitialised = false;
        [System.NonSerialized] private StickyControlModule thisCharacter = null;
        [System.NonSerialized] private StickyInputModule stickyInputModule = null;
        private bool isPlayer = false;
        private bool isMoving = false;
        private bool isRotating = false;
        private bool wasPlayerInputEnabled = false;
        private bool wasFreeLookEnabled = false;
        private Vector3 targetPosition = Vector3.zero;
        [System.NonSerialized] private Transform targetTfm = null;
        private Vector3 targetOffset = Vector3.zero;
        private Vector3 targetDirection = Vector3.forward;
        private Quaternion targetRotationLS = Quaternion.identity;
        private CharacterInput characterInput = null;
        private float initialMaxStepOffset = 0f;
        private float distanceToTargetSqr = 0;
        #endregion

        #region Public Delegates
        public delegate void CallbackOnReachedMoveTarget();
        public delegate void CallbackOnReachedRotateTarget();

        /// <summary>
        /// Set this to a custom method to receive notification when the target
        /// position has been reached.
        /// </summary>
        public CallbackOnReachedMoveTarget callbackOnReachedMoveTarget;

        /// <summary>
        /// Set this to a custom method to receive notificationw when the target
        /// rotation or direction to face has been reached.
        /// </summary>
        public CallbackOnReachedRotateTarget callbackOnReachedRotateTarget;

        #endregion

        #region Private Initialise Methods

        private void Awake()
        {
            if (initialiseOnAwake) { Initialise(); }
        }

        #endregion

        #region Update Methods

        void Update()
        {
            if (isInitialised)
            {
                if (isMoving)
                {
                    if (targetTfm != null) { targetPosition = targetTfm.position + (targetTfm.rotation * targetOffset); }
                    MoveToward();
                }
                else if (isRotating)
                {
                    if (targetTfm != null)
                    {
                        // Add the local space rotation to the world space rotation. Then get the world space direction
                        targetDirection = targetTfm.rotation * targetRotationLS * Vector3.forward;

                        if (targetDirection == Vector3.zero) { targetDirection = Vector3.forward; }
                    }
                    RotateToward();
                }
            }
        }

        #endregion

        #region Private and Internal Methods - General

        /// <summary>
        /// Get a progressive amount of look.
        /// turnAngleThreshold should be >= 10.
        /// </summary>
        /// <param name="localAngleAdjustmentY"></param>
        /// <returns></returns>
        private float GetLookAmount(float localAngleAdjustmentY)
        {
            // +-20 degrees gives full turn value of 1.
            float amount = localAngleAdjustmentY / turnAngleThreshold;

            // Clamp -1.0 to 1.0
            if (amount < -1f) { amount = -1f; }
            else if (amount > 1f) { amount = 1f; }

            return amount;
        }

        /// <summary>
        /// Attempt to move toward the target destination.
        /// Stop moving when near the target position.
        /// </summary>
        private void MoveToward()
        {
            Vector3 dirToTarget = targetPosition - thisCharacter.GetCurrentPosition;
            distanceToTargetSqr = dirToTarget.sqrMagnitude;

            if (distanceToTargetSqr < targetDistanceThreshold * targetDistanceThreshold)
            {
                CancelMoveTo();

                if (callbackOnReachedMoveTarget != null) { callbackOnReachedMoveTarget.Invoke(); }
            }
            else
            {
                if (distanceToTargetSqr < Mathf.Epsilon) { dirToTarget = thisCharacter.GetCurrentForward; }
                else { dirToTarget.Normalize(); }

                // Project the target direction onto the characters x-z plane
                Vector3 targetFwd = Vector3.ProjectOnPlane(dirToTarget, thisCharacter.GetCurrentUp);

                // Calc just the y-axis local component
                float localAngleAdjustmentY = Vector3.SignedAngle(thisCharacter.GetCurrentForward, targetFwd, thisCharacter.GetCurrentUp);

                characterInput.horizontalLook = GetLookAmount(localAngleAdjustmentY);

                // Walk slower when the distance is less
                float moveAmount = distanceToTargetSqr / (moveDistanceThreshold * moveDistanceThreshold);

                characterInput.verticalMove = moveAmount > 1f ? 1f : moveAmount;

                thisCharacter.SendInput(characterInput);
            }
        }

        /// <summary>
        /// Configure the S3D character so we can override movement
        /// </summary>
        private void OverrideMovement()
        {
            // Is this a player character which might have sticky input module enabled?
            if (isPlayer)
            {
                if (stickyInputModule != null)
                {
                    // Remember if input is enabled
                    wasPlayerInputEnabled = stickyInputModule.IsInitialised && stickyInputModule.IsInputEnabled;
                    stickyInputModule.DisableInput(true);

                    wasFreeLookEnabled = thisCharacter.IsLookFreeLookEnabled;

                    if (wasFreeLookEnabled ) { thisCharacter.SetFreeLook(false); }
                }
            }
            else
            {
                wasPlayerInputEnabled = false;
            }

            initialMaxStepOffset = thisCharacter.maxStepOffset;
            thisCharacter.maxStepOffset = maxStepOffset;

            characterInput.SetClassDefaults();
        }

        /// <summary>
        /// Restore the S3D config we changed
        /// </summary>
        private void RestoreSettings()
        {
            // Cancel any movement input for NPC or Player
            characterInput.horizontalLook = 0f;
            characterInput.horizontalMove = 0f;
            characterInput.verticalMove = 0f;
            characterInput.sprint = false;
            thisCharacter.ResetInputDamping();
            thisCharacter.SendInput(characterInput);

            if (isPlayer)
            {
                if (wasPlayerInputEnabled)
                {
                    stickyInputModule.EnableInput();
                }

                if (wasFreeLookEnabled) { thisCharacter.SetFreeLook(true); }
            }

            thisCharacter.maxStepOffset = initialMaxStepOffset;
        }

        /// <summary>
        /// Attempt to rotate to face the target direction
        /// </summary>
        private void RotateToward()
        {
            // Calc just the y-axis local component
            float localAngleAdjustmentY = Vector3.SignedAngle(thisCharacter.GetCurrentForward, targetDirection, thisCharacter.GetCurrentUp);

            if (localAngleAdjustmentY > -targetAngleThreshold && localAngleAdjustmentY < targetAngleThreshold)
            {
                CancelRotateTo();

                if (callbackOnReachedRotateTarget != null) { callbackOnReachedRotateTarget.Invoke(); }
            }
            else
            {
                characterInput.horizontalLook = GetLookAmount(localAngleAdjustmentY);

                thisCharacter.SendInput(characterInput);
            }
        }

        #endregion

        #region Events

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Cancel the MoveTo operation
        /// </summary>
        public void CancelMoveTo()
        {
            if (isMoving)
            {
                targetTfm = null;
                targetPosition = Vector3.zero;
                targetOffset = Vector3.zero;
                RestoreSettings();
                isMoving = false;
                isRotating = false;
            }
        }

        /// <summary>
        /// Cancel the RotateTo operation
        /// </summary>
        public void CancelRotateTo()
        {
            if (isRotating)
            {
                targetTfm = null;
                targetPosition = Vector3.zero;
                targetOffset = Vector3.zero;
                targetDirection = Vector3.forward;
                RestoreSettings();
                isRotating = false;
                isMoving = false;
            }
        }

        /// <summary>
        /// Use this for initialisation if initialiseOnAwake is false.
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            // For the sake of the sample, do a bunch of error checking. In game code, you would probably
            // do all this checking on one line.
            if (!gameObject.TryGetComponent(out thisCharacter))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleMoveTo - did you forget to attach this script to your player S3D character?");
                #endif
            }
            else if (!thisCharacter.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleMoveTo - your character " + thisCharacter.name + " is not initialised");
                #endif
            }
            else
            {
                isPlayer = !thisCharacter.IsNPC() && TryGetComponent(out stickyInputModule);

                characterInput = new CharacterInput();

                isInitialised = true;
            }
        }


        /// <summary>
        /// Attempt to move toward a world space position.
        /// </summary>
        /// <param name="moveToPosition"></param>
        public void MoveTo (Vector3 moveToPosition)
        {
            if (isInitialised)
            {
                targetPosition = moveToPosition;
                targetOffset = Vector3.zero;
                OverrideMovement();
                isMoving = true;
                isRotating = false;
            }
        }

        /// <summary>
        /// Attempt to move toward a transform position
        /// </summary>
        /// <param name="moveToTransform"></param>
        public void MoveTo (Transform moveToTransform)
        {
            if (isInitialised)
            {
                targetTfm = moveToTransform;
                targetOffset = Vector3.zero;

                if (targetTfm != null)
                {
                    OverrideMovement();
                    isMoving = true;
                    isRotating = false;
                }
                else { CancelMoveTo(); }
            }
        }

        /// <summary>
        /// Attempt to move toward a transform position with a local space offset.
        /// </summary>
        /// <param name="moveToTransform"></param>
        /// <param name="offset"></param>
        public void MoveToWithOffset (Transform moveToTransform, Vector3 offset)
        {
            if (isInitialised)
            {
                targetTfm = moveToTransform;
                targetOffset = offset;

                if (targetTfm != null)
                {
                    OverrideMovement();
                    isMoving = true;
                    isRotating = false;
                }
                else { CancelMoveTo(); }
            }
        }

        /// <summary>
        /// Attempt to face the given world space direction
        /// </summary>
        /// <param name="rotation"></param>
        public void RotateTo (Vector3 direction)
        {
            if (isInitialised)
            {
                targetTfm = null;

                if (direction == Vector3.zero) { targetDirection = Vector3.forward; }
                else { targetDirection = direction; }

                OverrideMovement();
                isRotating = true;
                isMoving = false;
            }
        }

        /// <summary>
        /// Attempt to face the given transform direction with the relative offset rotation
        /// expressed in Euler angles (degrees)
        /// </summary>
        /// <param name="rotateToTransform"></param>
        /// <param name="offsetRotation"></param>
        public void RotateTo (Transform rotateToTransform, Vector3 offsetRotation)
        {
            if (isInitialised)
            {
                targetTfm = rotateToTransform;

                if (targetTfm != null)
                {
                    targetRotationLS = Quaternion.Euler(offsetRotation);
                }
                else
                {
                    targetDirection = Quaternion.Euler(offsetRotation) * Vector3.forward;

                    if (targetDirection == Vector3.zero) { targetDirection = Vector3.forward; }
                }

                OverrideMovement();
                isRotating = true;
                isMoving = false;
            }
        }

        /// <summary>
        /// If moving, attempt to sprint toward the target
        /// </summary>
        public void Sprint()
        {
            if (isInitialised && isMoving)
            {
                characterInput.sprint = true;
            }
        }

        /// <summary>
        /// If moving, attempt to walk toward the target
        /// </summary>
        public void Walk()
        {
            if (isInitialised && isMoving)
            {
                characterInput.sprint = false;
            }
        }

        #endregion
    }
}