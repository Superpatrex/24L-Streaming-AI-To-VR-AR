using System.Collections;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple script to attach to an NPC character to sit or stand from a sittable
    /// interactive-enebled object.
    /// SETUP:
    /// 1. Attach this to your S3D NPC character parent gameobject which includes the StickyControlModule component.
    /// 2. Add an interactive component to your seat object (which must have a collider)
    /// 3. Enable IsSittable on the seat interactive object
    /// 4. Set the Sit Target Offset to be infront of the seat at ground level e.g. x: 0, y: 0, z: 0.5
    /// 5. Drag the seat interactive object from the scene into the slot on this component
    /// 6. From your own code, get a reference to this component and call BeginSitAction()
    ///    OR use a StickyProximity component on the seat to trigger the BeginSitAction.
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// NOTE: This has a dependency on SampleMoveTo.cs
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Sit Action NPC")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class SampleSitActionNPC : MonoBehaviour
    {
        #region Public Variables
        [Tooltip("If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the component is enabled through code.")]
        public bool initialiseOnStart = false;
        [Tooltip("The name of the bool parameter in the Animator Controller used for sitting")]
        public string sittingAnimParam = "isSitting";
        [Tooltip("The time between the player decides to stand up and when they can move again")]
        [Range(0f, 5f)] public float sitToStandDelay = 1.2f;
        [Tooltip("The distance we look behind the character for any obstacles above the base of the spine bone")]
        [Range(0.1f, 1f)] public float checkBehindDist = 0.34f;
        [Tooltip("Should the character move with the reference frame when the character is sitting?")]
        public bool lockPositonToRefFrame = true;
        [Tooltip("The character max step up height during sitting procedure")]
        public float maxStepOffset = 0.2f;
        [Tooltip("How close the character must align to the seat rotation (in degrees) before sitting")]
        public float targetAngleThreshold = 2f;
        [Tooltip("How close the character must be to the seat (in metres) before rotating to sit down")]
        public float targetDistanceThreshold = 0.05f;
        [Tooltip("When sitting, should the character main collider be scaled down. By default this is knee height so they can sit close to a table or bench")]
        public bool scaleHeightWhenSitting = false;
        [Tooltip("If callback notifications are configured, should they be called when stand or sit actions complete?")]
        public bool isGetNotifications = false;
        [Range(0.01f, 1f), Tooltip("How often to check for notifications")] public float actionCheckInterval = 0.2f;
        [Tooltip("The name of the animator State which is running when the character has finished sitting down")]
        public string sittingStateName = "Sitting";
        [Tooltip("The zero-based animation layer that contains the Sitting state")]
        public int sittingAnimationLayer = 0;
        [Tooltip("The maximum time, in seconds, an action notification will wait before giving up")]
        public float notificationTimeout = 10f;
        #endregion

        #region Public Properties
        public bool IsInitialised { get { return isInitialised; } }

        public StickyControlModule ThisNPC { get { return thisCharacter; } }

        #endregion

        #region Public Static Variables

        #endregion

        #region Private Variables - General
        [System.NonSerialized] private StickyControlModule thisCharacter = null;
        [System.NonSerialized] private StickyManager stickyManager = null;
        [System.NonSerialized] private SampleMoveTo sampleMoveTo = null;
        [SerializeField] private StickyInteractive sittableInteractive = null;
        private bool isInitialised = false;
        private int sitAnimHashcode = 0;
        private bool isStandingUpInProgress = false;
        private float sittingScaledHeight = 0f;
        [System.NonSerialized] private WaitForSeconds notifyCheckWait = null;
        private int sittingStateNameHash = 0;
        [System.NonSerialized] private IEnumerator waitingToSit = null;
        #endregion

        #region Public Delegates
        public delegate void OnSeated(SampleSitActionNPC sampleSitActionNPC);
        public delegate void OnStoodUp(SampleSitActionNPC sampleSitActionNPC);

        /// <summary>
        /// The name of your custom method that is called immediately after the character
        /// has finished sitting down.
        /// </summary>
        [System.NonSerialized] public OnSeated onSeated = null;

        /// <summary>
        /// The name of your custom method that is called immediately after the character
        /// has stood up from a sitting position.
        /// </summary>
        [System.NonSerialized] public OnStoodUp onStoodUp = null;

        #endregion

        #region Private Initialise Methods

        // Use this for initialization
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Update Methods

        void Update()
        {
            
        }

        #endregion

        #region Private and Internal Methods - General

        private void EnableFullMovement()
        {
            if (scaleHeightWhenSitting) { thisCharacter.RestoreFullHeight(); }

            if (lockPositonToRefFrame) { thisCharacter.UnlockPosition(); }
            else { thisCharacter.EnableMovement(); }

            // Prevent the character from moving or rotating based on its
            // last movement or rotation action.
            thisCharacter.StopMoving();

            if (isStandingUpInProgress)
            {
                isStandingUpInProgress = false;

                // If configured, call the dev-supplied external method.
                if (isGetNotifications && onStoodUp != null)
                {
                    onStoodUp.Invoke(this);
                }
            }
        }

        /// <summary>
        /// If configured, send a notification when the sit action has completed.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForSitdown()
        {
            bool hasCompleted = false;
            float timeOut = notificationTimeout;

            while (!hasCompleted && timeOut > 0f)
            {
                yield return notifyCheckWait;
                timeOut -= actionCheckInterval;

                hasCompleted = thisCharacter.defaultAnimator.GetCurrentAnimatorStateInfo(sittingAnimationLayer).shortNameHash == sittingStateNameHash;
            }

            if (hasCompleted && onSeated != null)
            {
                onSeated.Invoke(this);
            }
        }

        /// <summary>
        /// Attempt to make the character sit down
        /// </summary>
        private void SitDown()
        {
            if (isInitialised && !thisCharacter.IsSitting)
            {
                Vector3 spineOffset = Vector3.zero;
                if (thisCharacter.GetBoneBottomOffset(HumanBodyBones.Spine, ref spineOffset))
                {
                    // Make sure there is space for the character to sit down.
                    // As an enhancement, in your own code, you could check to make sure there was something to sit on.
                    Vector3 back = thisCharacter.GetCurrentForward * -1f;
                    if (back.sqrMagnitude < Mathf.Epsilon) { back = thisCharacter.transform.forward * -1f; }

                    if (!thisCharacter.IsObstacle(spineOffset.y - 0.2f, 0f, back, checkBehindDist))
                    {
                        if (lockPositonToRefFrame) { thisCharacter.LockPosition(); }
                        else { thisCharacter.DisableMovement(); }
                        thisCharacter.IsSitting = true;
                        thisCharacter.defaultAnimator.SetBool(sitAnimHashcode, true);

                        if (scaleHeightWhenSitting)
                        {
                            if (sittingScaledHeight == 0) { thisCharacter.ScaleToKneeHeight(); }
                            else { thisCharacter.ScaleToTemporaryHeight(sittingScaledHeight); }
                        }

                        if (isGetNotifications)
                        {
                            waitingToSit = WaitForSitdown();
                            StartCoroutine(waitingToSit);
                        }

                    }
                    #if UNITY_EDITOR
                    else
                    {
                        thisCharacter.StopMoving();
                        Debug.Log("SampleSitActionNPC - there is an obstacle in the way preventing " + name + " to sit down at time: " + Time.time);
                    }
                    #endif
                }
            }
        }

        /// <summary>
        /// This is automatically called by SampleMoveTo when the character
        /// reaches the target postion.
        /// </summary>
        private void SitMoveToCompleted()
        {
            if (sampleMoveTo != null && sittableInteractive != null)
            {
                sampleMoveTo.callbackOnReachedMoveTarget -= SitMoveToCompleted;

                sampleMoveTo.targetAngleThreshold = targetAngleThreshold;
                sampleMoveTo.RotateTo(sittableInteractive.transform, Vector3.zero);
                // Get notified when the character is ready to sit down
                sampleMoveTo.callbackOnReachedRotateTarget += SitRotateToCompleted;
            }
        }

        /// <summary>
        /// This is automatically called by SampleMoveTo when the character
        /// is facing in the correct direction, ready to sit down.
        /// </summary>
        private void SitRotateToCompleted()
        {
            sampleMoveTo.callbackOnReachedRotateTarget -= SitRotateToCompleted;
            SitDown();
        }

        #endregion

        #region Events

        private void OnDisable()
        {
            if (sampleMoveTo != null)
            {
                sampleMoveTo.callbackOnReachedMoveTarget -= SitMoveToCompleted;
                sampleMoveTo.callbackOnReachedRotateTarget -= SitRotateToCompleted;
            }
        }

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Attempt to sit on the interactive seat set in the editor.
        /// See also BeginSitAction (StickyInteractive stickyInteractive).
        /// </summary>
        public void BeginSitAction()
        {
            BeginSitAction(sittableInteractive);
        }

        /// <summary>
        /// Attempt to move the NPC near the interactive seat, then sit down.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        public void BeginSitAction (StickyInteractive stickyInteractive)
        {
            if (isInitialised && sampleMoveTo != null)
            {
                if (stickyInteractive != null)
                {
                    // Keep track of the interative-enabled object
                    sittableInteractive = stickyInteractive;

                    sittableInteractive.AllocateSeat();

                    sampleMoveTo.targetDistanceThreshold = targetDistanceThreshold;
                    // Setting the step height lower than the seat can prevent the character climbing across the seat
                    sampleMoveTo.maxStepOffset = maxStepOffset;
                    sampleMoveTo.MoveToWithOffset(stickyInteractive.transform, stickyInteractive.sitTargetOffset);
                    // Get notified when we have moved the character to be in front of the object
                    sampleMoveTo.callbackOnReachedMoveTarget += SitMoveToCompleted;
                }
                else
                {
                    sampleMoveTo.callbackOnReachedMoveTarget -= SitMoveToCompleted;
                    sampleMoveTo.CancelMoveTo();
                }
            }
        }

        /// <summary>
        /// During the initial phases of sitting, it is possible to cancel the sit action.
        /// When sitting, call StandUp() instead.
        /// </summary>
        public void CancelSitAction ()
        {
            if (isInitialised && !thisCharacter.IsSitting)
            {
                if (waitingToSit != null) { StopCoroutine(waitingToSit); }

                // If is in the moveTo or rotateTo phase
                if (sampleMoveTo.IsMoving) { sampleMoveTo.CancelMoveTo(); }
                else if (sampleMoveTo.IsRotating) { sampleMoveTo.CancelRotateTo(); }

                sampleMoveTo.callbackOnReachedMoveTarget -= SitMoveToCompleted;
                sampleMoveTo.callbackOnReachedRotateTarget -= SitRotateToCompleted;

                sittableInteractive.DeallocateSeat();

                isStandingUpInProgress = false;
            }
        }

        /// <summary>
        /// Initialise this component. Has no effect if already initialised
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            stickyManager = StickyManager.GetOrCreateManager(gameObject.scene.handle);

            // For the sake of the sample, do a bunch of error checking. In game code, you would probably
            // do all this checking on one line.
            if (!gameObject.TryGetComponent(out thisCharacter))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitActionNPC - did you forget to attach this script to your player S3D character?");
                #endif
            }
            else if (!thisCharacter.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitActionNPC - your character " + thisCharacter.name + " is not initialised");
                #endif
            }
            else if (!thisCharacter.IsAnimateEnabled)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitActionNPC - animate does not seem to be enabled, available, or configured on " + thisCharacter.name);
                #endif
            }
            else if (string.IsNullOrEmpty(sittingAnimParam))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitActionNPC - isSittingAnimParam on this component seems empty for " + thisCharacter.name + ". This should match the name in your animator controller.");
                #endif
            }
            else if (!thisCharacter.defaultAnimator.isHuman)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitActionNPC - " + thisCharacter.name + " does not seem to have a humanoid rig");
                #endif
            }
            else if (!thisCharacter.IsNPC())
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitActionNPC - " + thisCharacter.name + " this character is not marked as an NPC under the General Move Settings ");
                #endif
            }
            else if (stickyManager != null)
            {
                if (!gameObject.TryGetComponent(out sampleMoveTo))
                {
                    sampleMoveTo = gameObject.AddComponent<SampleMoveTo>();
                }

                sitAnimHashcode = thisCharacter.VerifyAnimParameter(sittingAnimParam, S3DAnimAction.ParameterType.Bool);

                if (isGetNotifications)
                {
                    notifyCheckWait = new WaitForSeconds(actionCheckInterval);
                    sittingStateNameHash = Animator.StringToHash(sittingStateName);
                }

                isInitialised = sampleMoveTo != null && sitAnimHashcode != 0;
            }
        }

        /// <summary>
        /// This allows you to override the scaled height of the character
        /// when scaleHeightWhenSitting is true.
        /// </summary>
        /// <param name="scaledHeight"></param>
        public void SetScaledSittingHeight(float scaledHeight)
        {
            // Only allow sensible values
            if (scaledHeight > 0.1f && scaledHeight < 5f)
            {
                sittingScaledHeight = scaledHeight;
            }
        }

        /// <summary>
        /// Attempt to make the character stand up
        /// </summary>
        public void StandUp()
        {
            if (isInitialised)
            {
                if (thisCharacter.IsSitting)
                {
                    if (waitingToSit != null) { StopCoroutine(waitingToSit); }

                    thisCharacter.IsSitting = false;
                    isStandingUpInProgress = true;
                    // We could create an event on the animation, however, just as easy
                    // to do it here with a delay.
                    Invoke("EnableFullMovement", sitToStandDelay);
                    thisCharacter.defaultAnimator.SetBool(sitAnimHashcode, false);
                    sittableInteractive.DeallocateSeat();
                }
                else { CancelSitAction(); }
            }
        }

        #endregion
    }
}