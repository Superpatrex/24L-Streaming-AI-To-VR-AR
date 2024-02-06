using UnityEngine;
using System.Collections.Generic;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple script to have a Sticky3D NPC character play an animation clip when the
    /// player comes within range. It also shows how to replace an animation clip in
    /// your animator controller.
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// EXAMPLE: See the NPCLookAtDemo scene.
    /// SETUP
    /// 1. Add This script to a new CHILD GameObject or a S3D Non-Player-Character (NPC) in the scene (not the player)
    /// 2. Set the SphereCollider to be a trigger collider and set radius to say 5.
    /// 3. In your Animation Controller, add a Trigger parameter that triggers an animation.
    /// 4. On the StickyControlModule, create a custom trigger Anim Action on the Animate tab
    /// 5. On this component in the scene, set the Anim Action number.
    /// 6. Add a Player prefab to the scene
    /// 7. Add the player from the scene to this component on the NPC in the slot provided.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    [AddComponentMenu("Sticky3D Controller/Samples/NPC Play Anim")]
    [DisallowMultipleComponent]
    public class SampleNPCPlayAnim : MonoBehaviour
    {
        #region Public Variables
        public bool initialiseOnStart = true;
        [Tooltip("The S3D character to wave at")]
        public StickyControlModule player = null;
        [Tooltip("Does the player need to be in line-of-sight?")]
        public bool checkLOS = false;
        [Tooltip("The (Custom) Anim Action from the Animate tab of your NPC. 0 = Not Set")]
        public int animActionNumber = 0;
        [Tooltip("The original animation in the Animator Controller that will be replaced")]
        public AnimationClip originalClip = null;
        [Tooltip("The clip you want to play")]
        public AnimationClip animationClip = null;
        [Tooltip("The time, in seconds, between when the player enters the trigger area and when the animation plays")]
        [Range(0f, 10f)] public float delayTime = 0f;
        #endregion

        #region Private Variables
        private StickyControlModule thisCharacter = null;
        private Collider distanceCollider = null;
        private bool isInitialised = false;
        private S3DAnimAction s3dAnimAction = null;
        private AnimationClip currentClip = null;
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get eye level of the player, then check if there is line of sight
        /// from the eye level of this character to the player.
        /// </summary>
        /// <returns></returns>
        private bool IsPlayerInLoS()
        {
            return player != null && thisCharacter.IsInLineOfSight(player.GetWorldEyePosition(), true, false);
        }

        /// <summary>
        /// Replace the clip in the Animator Controller and notify the character we want to run the animation
        /// </summary>
        private void PlayAnimation()
        {
            // This is optional but it demonstrates how to replace a clip in your Animator Controller
            thisCharacter.ReplaceAnimationClip(ref currentClip, animationClip);

            // Here we use a Trigger to enable our action (however, we could also use a bool).
            if (s3dAnimAction.parameterType == S3DAnimAction.ParameterType.Trigger)
            {
                thisCharacter.SetCustomAnimActionTriggerValue(s3dAnimAction, true);
            }
        }

        #endregion

        #region Event Methods

        private void OnTriggerEnter(Collider other)
        {
            if (isInitialised)
            {
                // Is this the player and can we see them?
                if (player.IsColliderSelf(other) && (!checkLOS || IsPlayerInLoS()))
                {
                    if (delayTime > 0f) { Invoke("PlayAnimation", delayTime); }
                    else { PlayAnimation(); }
                }
            }
        }

        #endregion

        #region Public Methods

        public void Initialise()
        {
            distanceCollider = GetComponent<SphereCollider>();
            thisCharacter = GetComponentInParent<StickyControlModule>();

            // For the sake of the sample, do a bunch of error checking. In game code, you would probably
            // do all this checking on one line.
            if (thisCharacter == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleNPCPlayAnim - did you forget to attach this script to a child gameobject of one of our non-player S3D characters?");
                #endif
            }
            else if (!thisCharacter.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleNPCPlayAnim - your character " + thisCharacter.name + " is not initialised");
                #endif
            }
            else if (distanceCollider == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleNPCPlayAnim requires a Sphere collider");
                #endif
            }
            else if (!distanceCollider.isTrigger)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleNPCPlayAnim requires a trigger Sphere collider");
                #endif
            }
            else if (player == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleNPCPlayAnim requires a Sticky3D player to be added to this script");
                #endif
            }
            else if (animActionNumber < 1)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleNPCPlayAnim - you need to enter an Anim Action Number from the Animate tab of the character which will trigger the action");
                #endif
            }
            else if (!thisCharacter.IsAnimateEnabled)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleNPCPlayAnim - animate does not seem to be enabled, available, or configured on " + thisCharacter.name);
                #endif
            }
            else if (originalClip == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleNPCPlayAnim - you need to specify the original clip that will be replaced in the Animator Controller for " + thisCharacter.name);
                #endif
            }           
            else
            {
                int numAnimActions = thisCharacter.NumberOfAnimActions;

                if (animActionNumber > numAnimActions)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SampleNPCPlayAnim - The Anim Action Number from the Animate tab of the character is out of range. Number must be between 1 and " + numAnimActions.ToString());
                    #endif
                }
                else
                {
                    // Get the (custom) Anim Action
                    s3dAnimAction = thisCharacter.GetAnimActionByIndex(animActionNumber-1);

                    currentClip = originalClip;

                    isInitialised = s3dAnimAction != null;
                }
            }
        }


        #endregion
    }
}
