using System.Collections;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple script to attach to a player character to sit or stand from a sittable
    /// interactive-enebled object.
    /// SETUP:
    /// 1. Attach this to your S3D character parent gameobject which includes the StickyControlModule component.
    /// 2. Add an interactive component to your seat object (which must have a collider)
    /// 3. Enable IsSittable on the seat interactive object
    /// 4. Set the Sit Target Offset to be infront of the seat at ground level e.g. x: 0, y: 0, z: 0.5
    /// 5. At an On Hover Enter event to the interactive object
    /// 6. Drag in the character from the scene to the new event. Select the SampleSitActionPopup.ShowPopup function.
    /// 7. Drag the seat interactive object from the scene into the ShowPopup parameter slot.
    /// 8. On the character Sticky Input Module, add a Custom Input
    /// 9. Add a button (say Y key) to the Custom Input and add an event
    /// 10. Drag the seat interactive object from the scene into event
    /// 11. Select the SampleSitActionPopup.StandUp function.
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// NOTE: This has a dependency on SampleMoveTo.cs
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Sit Action Popup")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class SampleSitActionPopup : MonoBehaviour
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
        [Tooltip("Should look be disabled when the character sits down?")]
        public bool disableLook = true;
        [Tooltip("Should the character move with the reference frame when the character is sitting?")]
        public bool lockPositonToRefFrame = true;
        [Tooltip("The sticky popup prefab to display when hovering over the interactive object")]
        public StickyPopupModule stickyPopup1 = null;
        [Tooltip("The character max step up height during sitting procedure")]
        public float maxStepOffset = 0.2f;
        [Tooltip("How close the character must align to the seat rotation (in degrees) before sitting")]
        public float targetAngleThreshold = 2f;
        [Tooltip("How close the character must be to the seat (in metres) before rotating to sit down")]
        public float targetDistanceThreshold = 0.05f;
        [Tooltip("Only show one popup at a time for a character")]
        public bool singlePopupOnly = false;
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

        #endregion

        #region Public Delegates
        public delegate void OnSeated(SampleSitActionPopup sampleSitActionPopup);
        public delegate void OnStoodUp(SampleSitActionPopup sampleSitActionPopup);

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

        #region Private Variables - General
        [System.NonSerialized] private StickyControlModule thisCharacter = null;
        [System.NonSerialized] private StickyManager stickyManager = null;
        [System.NonSerialized] private SampleMoveTo sampleMoveTo = null;
        [System.NonSerialized] private StickyInteractive sittableInteractive = null;
        private bool isInitialised = false;
        private int sitAnimHashcode = 0;
        private int popupPrefabID = -1;
        private float sittingScaledHeight = 0f;
        private bool isStandingUpInProgress = false;
        [System.NonSerialized] private WaitForSeconds notifyCheckWait = null;
        private int sittingStateNameHash = 0;
        [System.NonSerialized] private IEnumerator waitingToSit = null;
        #endregion

        #region Private Initialise Methods

        // Use this for initialization
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Update Methods

        #endregion

        #region Private and Internal Methods - General


        private void EnableFullMovement()
        {
            if (scaleHeightWhenSitting) { thisCharacter.RestoreFullHeight(); }

            if (disableLook) { thisCharacter.EnableLookMovement(); }

            if (lockPositonToRefFrame) { thisCharacter.UnlockPosition(); }
            else { thisCharacter.EnableMovement(); }

            thisCharacter.SetLookFollowHead(false);
            thisCharacter.SetLookFollowHeadTP(false);

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
                    if (!thisCharacter.IsObstacle(spineOffset.y - 0.2f, 0f, thisCharacter.GetCurrentForward * -1f, checkBehindDist))
                    {
                        if (lockPositonToRefFrame) { thisCharacter.LockPosition(); }
                        else { thisCharacter.DisableMovement(); }
                        if (disableLook) { thisCharacter.DisableLookMovement(); }
                        thisCharacter.IsSitting = true;
                        thisCharacter.SetLookFollowHead(true);
                        thisCharacter.SetLookFollowHeadTP(true);
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
                        Debug.Log("SampleSitActionPopup - there is an obstacle in the way preventing " + name + " to sit down at time: " + Time.time);
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
        /// This is automatically called when the user clicks on an item in the sit action popup
        /// </summary>
        /// <param name="stickyPopupModule"></param>
        /// <param name="itemNumber"></param>
        /// <param name="stickyInteractive"></param>
        private void SitPopupActioned (StickyPopupModule stickyPopupModule, int itemNumber, StickyInteractive stickyInteractive)
        {
            // Close the popup after use - i.e. return it to the pool.
            if (stickyPopupModule != null) { stickyPopupModule.DestroyGenericObject(); }

            if (sampleMoveTo != null)
            {
                if (itemNumber == 2)
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
                else
                {
                    sampleMoveTo.callbackOnReachedMoveTarget -= SitMoveToCompleted;
                    sampleMoveTo.CancelMoveTo();
                }
            }
        }

        /// <summary>
        /// This is automatically called by SampleMoveTo when the character
        /// is facing in the correct direction, ready to sit down.
        /// </summary>
        private void SitRotateToCompleted()
        {
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

                if (sittableInteractive != null) { sittableInteractive.DeallocateSeat(); }

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
                Debug.LogWarning("ERROR: SampleSitActionPopup - did you forget to attach this script to your player S3D character?");
                #endif
            }
            else if (!thisCharacter.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitActionPopup - your character " + thisCharacter.name + " is not initialised");
                #endif
            }
            else if (!thisCharacter.IsAnimateEnabled)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitActionPopup - animate does not seem to be enabled, available, or configured on " + thisCharacter.name);
                #endif
            }
            else if (string.IsNullOrEmpty(sittingAnimParam))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitActionPopup - isSittingAnimParam on this component seems empty for " + thisCharacter.name + ". This should match the name in your animator controller.");
                #endif
            }
            else if (!thisCharacter.defaultAnimator.isHuman)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitActionPopup - " + thisCharacter.name + " does not seem to have a humanoid rig");
                #endif
            }
            else if (stickyPopup1 == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitActionPopup - " + thisCharacter.name + " is missing the StickyPopupModule prefab");
                #endif
            }
            else if (stickyManager != null)
            {
                if (thisCharacter.IsNPC())
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("WARNING: SampleSitActionPopup - " + thisCharacter.name + " is being used for an NPC. Typically, you should use SampleSitActionNPC.cs instead.");
                    #endif
                }

                popupPrefabID = stickyManager.GetOrCreateGenericPool(stickyPopup1);

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

                isInitialised = popupPrefabID != StickyManager.NoPrefabID && sampleMoveTo != null && sitAnimHashcode != 0;
            }
        }

        /// <summary>
        /// This allows you to override the scaled height of the character
        /// when scaleHeightWhenSitting is true.
        /// </summary>
        /// <param name="scaledHeight"></param>
        public void SetScaledSittingHeight (float scaledHeight)
        {
            // Only allow sensible values
            if (scaledHeight > 0.1f && scaledHeight < 5f)
            {
                sittingScaledHeight = scaledHeight;
            }
        }

        /// <summary>
        /// Hook this method up to the On Hover Enter event of a sittable interactive-enabled object.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        public void ShowPopup (StickyInteractive stickyInteractive)
        {
            // Ensure we have a valid pooled popup prefab, and an instance of it isn't already active (shown) for this seat.
            // Don't display if the character is already sitting down.
            if (isInitialised && stickyInteractive != null && popupPrefabID != StickyManager.NoPrefabID && !stickyInteractive.IsSeatAllocated && stickyInteractive.GetActivePopupID() == 0 && !thisCharacter.IsSitting)
            {
                if (!stickyInteractive.IsSittable)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SampleSitActionPopup.PopupShowSit - " + stickyInteractive.name + " is not Sittable");
                    #endif
                }
                else if (!singlePopupOnly || !thisCharacter.IsActivePopups())
                {
                    // Get the world-space position for the Popup based on the default relative local space offset setting 
                    Vector3 _position = stickyInteractive.GetPopupPosition();

                    // Setup the generic parameters
                    S3DInstantiateGenericObjectParameters igParms = new S3DInstantiateGenericObjectParameters
                    {
                        position = _position,
                        rotation = stickyInteractive.transform.rotation,
                        genericModulePrefabID = popupPrefabID
                    };

                    StickyGenericModule stickyGenericModule = stickyManager.InstantiateGenericObject(ref igParms);

                    if (stickyGenericModule != null)
                    {
                        // Configure our extended functionality
                        StickyPopupModule stickyPopupModule = (StickyPopupModule)stickyGenericModule;

                        // Tell the popup which interactive-enabled object triggered it
                        stickyPopupModule.SetInteractiveObject(stickyInteractive);

                        stickyPopupModule.SetCamera(thisCharacter.lookCamera1);

                        // Added S3D 1.1.0 Beta 21c
                        stickyPopupModule.SetCharacterIndirect(thisCharacter);

                        // To avoid passing strings, you could have them preset in the prefab.
                        stickyPopupModule.SetItem(1, "Cancel");
                        stickyPopupModule.SetItem(2, "Sit");
                        // We only need 2 of the 3 items from this prefab
                        stickyPopupModule.HideItem(3);

                        stickyPopupModule.SetItemAction(1, SitPopupActioned);
                        stickyPopupModule.SetItemAction(2, SitPopupActioned);
                    }
                }
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