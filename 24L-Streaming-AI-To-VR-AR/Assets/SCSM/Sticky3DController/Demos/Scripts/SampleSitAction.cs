using System.Collections;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple script to attach to a character to toggle and action (sitting) on or off.
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// See also SampleSitActionPopup.cs and SampleSitActionNPC.cs.
    /// SETUP:
    /// 1. Attach this to your S3D character parent gameobject which includes the StickyControlModule component.
    /// 2. On the Sticky Input Module, add a new Custom Input.
    /// 3. Configure the button for the Custom Input e.g. T on the keyboard
    /// 4. Add a callback method to the Custom Input. Drag in the player under "Runtime Only"
    /// 5. Change the Function to SampleSitAction.ToggleSit()
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Sit Action")]
    [DisallowMultipleComponent]
    public class SampleSitAction : MonoBehaviour
    {
        #region Public Variables
        [Tooltip("If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the component is enabled through code. Default: true for backward compatibility")]
        public bool initialiseOnStart = true;
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

        #region Private Variables
        [System.NonSerialized] private StickyControlModule thisCharacter = null;
        private bool isInitialised = false;
        private int sitAnimHashcode = 0;
        private bool isSitting = false;
        private float sittingScaledHeight = 0f;
        private bool isStandingUpInProgress = false;
        [System.NonSerialized] private WaitForSeconds notifyCheckWait = null;
        private int sittingStateNameHash = 0;
        [System.NonSerialized] private IEnumerator waitingToSit = null;
        #endregion

        #region Public Delegates
        public delegate void OnSeated(SampleSitAction sampleSitAction);
        public delegate void OnStoodUp(SampleSitAction sampleSitAction);

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

        #region Initialisation Methods
        // Start is called before the first frame update
        private void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Private Methods

        private void EnableFullMovement()
        {
            if (scaleHeightWhenSitting) { thisCharacter.RestoreFullHeight(); }

            if (disableLook) { thisCharacter.EnableLookMovement(); }

            if (lockPositonToRefFrame) { thisCharacter.UnlockPosition(); }
            else { thisCharacter.EnableMovement(); }

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

        #endregion

        #region Public Methods

        /// <summary>
        /// Attempt to initialise the component
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            thisCharacter = GetComponent<StickyControlModule>();

            // For the sake of the sample, do a bunch of error checking. In game code, you would probably
            // do all this checking on one line.
            if (thisCharacter == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitAction - did you forget to attach this script to your player S3D character?");
                #endif
            }
            else if (!thisCharacter.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitAction - your character " + thisCharacter.name + " is not initialised");
                #endif
            }
            else if (!thisCharacter.IsAnimateEnabled)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitAction - animate does not seem to be enabled, available, or configured on " + thisCharacter.name);
                #endif
            }
            else if (string.IsNullOrEmpty(sittingAnimParam))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitAction - isSittingAnimParam on this component seems empty for " + thisCharacter.name + ". This should match the name in your animator controller.");
                #endif
            }
            else if (!thisCharacter.defaultAnimator.isHuman)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleSitAction - " + thisCharacter.name + " does not seem to have a humanoid rig");
                #endif
            }
            else
            {
                sitAnimHashcode = thisCharacter.VerifyAnimParameter(sittingAnimParam, S3DAnimAction.ParameterType.Bool);

                if (sitAnimHashcode != 0)
                {
                    if (isGetNotifications)
                    {
                        notifyCheckWait = new WaitForSeconds(actionCheckInterval);
                        sittingStateNameHash = Animator.StringToHash(sittingStateName);
                    }

                    isInitialised = true;
                }
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
        /// Attempt to make the character sit down
        /// </summary>
        public void SitDown()
        {
            if (isInitialised && !isSitting)
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
                        isSitting = true;
                        thisCharacter.defaultAnimator.SetBool(sitAnimHashcode, isSitting);

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
                        Debug.Log("SampleSitAction - there is an obstacle in the way preventing " + name + " to sit down at time: " + Time.time);
                    }
                    #endif
                }
            }
        }

        /// <summary>
        /// Attempt to make the character stand up
        /// </summary>
        public void StandUp()
        {
            if (isInitialised && isSitting)
            {
                if (waitingToSit != null) { StopCoroutine(waitingToSit); }

                isSitting = false;
                isStandingUpInProgress = true;
                // We could create an event on the animation, however, just as easy
                // to do it here with a delay.
                Invoke("EnableFullMovement", sitToStandDelay);
                thisCharacter.defaultAnimator.SetBool(sitAnimHashcode, isSitting);
            }
        }

        /// <summary>
        /// Toggle sitting. For a player character, call this from
        /// a Custom Input on the S3D Sticky Input Module.
        /// </summary>
        public void ToggleSit()
        {
            if (isInitialised)
            {
                if (!isSitting) { SitDown(); }
                else { StandUp(); }               
            }
        }

        #endregion
    }
}