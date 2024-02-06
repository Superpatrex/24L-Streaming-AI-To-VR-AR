using UnityEngine;
using System.Collections.Generic;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple script to cycle through a list of animations to play for an NPC or Player S3D
    /// character.
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// EXAMPLE: See the NPCLookAtDemo scene.
    /// SETUP
    /// 1. Add This script to a S3D Non-Player-Character (NPC) or Player in the scene
    /// 2. Get the name of the animation clip in your Animator Controller that will be replaced
    /// 3. Populate the list of animation clips that you'll be using 
    /// 4. 
    /// 5. On this component in the scene, set the Anim Action number.
    /// 6. Add a Player prefab to the scene
    /// 7. Add the player from the scene to this component on the NPC in the slot provided.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Anim Play List")]
    [DisallowMultipleComponent]
    public class SampleAnimPlayList : MonoBehaviour
    {
        #region Public Variables
        [Tooltip("If enabled, the Initialise() will be called as soon as Start() runs. This should be disabled if you are instantiating the component through code.")]
        public bool initialiseOnStart = true;
        [Tooltip("The zero-based index of the Animator Controller layer than contains the State to play.")]
        public int controllerLayerIndex = -1;
        [Tooltip("The name of the State within Animator Controller that will play the clip. e.g. Interact")]
        public string animatorStateName = null;
        [Tooltip("The original animation in the Animator Controller that will be replaced")]
        public AnimationClip originalClip = null;
        [Tooltip("The animations to cycle through")]
        public AnimationClip[] animationClips;
        #endregion

        #region Private Variables
        private StickyControlModule thisCharacter = null;
        private bool isInitialised = false;
        private int animatorStateId = 0;
        private AnimationClip currentClip = null;
        private int numClips = 0;
        private int currentClipIndex = -1;
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Public Methods

        public void Initialise()
        {
            thisCharacter = GetComponent<StickyControlModule>();

            // For the sake of the sample, do a bunch of error checking. In game code, you would probably
            // do all this checking on one line.
            if (thisCharacter == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleAnimPlayList - did you forget to attach this script to one of our non-player S3D characters?");
                #endif
            }
            else if (!thisCharacter.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleAnimPlayList - your character " + thisCharacter.name + " is not initialised");
                #endif
            }
            else if (string.IsNullOrEmpty(animatorStateName))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleAnimPlayList - please enter the name of the State in the Animator Controller that contains the clip you wish to replace. e.g. Interact");
                #endif
            }
            else if (!thisCharacter.IsAnimateEnabled)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleAnimPlayList - animate does not seem to be enabled, available, or configured on " + thisCharacter.name);
                #endif
            }
            else if (originalClip == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleAnimPlayList - you need to specify the original clip that will be replaced in the Animator Controller for " + thisCharacter.name);
                #endif
            }
            else
            {
                animatorStateId =  thisCharacter.GetAnimationStateId(animatorStateName);

                numClips = animationClips == null ? 0 : animationClips.Length;

                if (numClips < 1)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SampleAnimPlayList - you don't have any animation clips to play for " + thisCharacter.name);
                    #endif
                }

                currentClip = originalClip;

                isInitialised = numClips > 0;
            }
        }

        /// <summary>
        /// Play the animation clip in the list of animations specified.
        /// Index is zero-based.
        /// </summary>
        /// <param name="clipIndex"></param>
        public void PlayClip(int clipIndex)
        {
            if (isInitialised)
            {
                if (clipIndex >= 0 && clipIndex < numClips)
                {
                    currentClip = originalClip;

                    // Replace the current clip with the new clip in your Animator Controller
                    thisCharacter.ReplaceAnimationClip(ref currentClip, animationClips[clipIndex]);

                    // Play the clip. As an alternative method, you could trigger the state via the S3D Animate tab.
                    // See SampleNPCPlayAnim.cs for more details.
                    thisCharacter.PlayAnimationState(animatorStateId, controllerLayerIndex, 0.25f);
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("ERROR: SampleAnimPlayList - clip index (" + clipIndex + ") for " + thisCharacter.name + " is out-of-range. Valid values are between 0 and " + (numClips-1).ToString());
                }
                #endif
            }
        }

        /// <summary>
        /// Play the next clip in the sequence. Will auto-wrap to the start.
        /// </summary>
        public void PlayNextClip()
        {
            if (isInitialised)
            {
                currentClipIndex = (currentClipIndex + 1) % numClips;
                PlayClip(currentClipIndex);
            }
        }

        /// <summary>
        /// Play the previous clip in the sequence. Will auto-wrap to the end.
        /// </summary>
        public void PlayPreviousClip()
        {
            if (isInitialised)
            {
                currentClipIndex = (currentClipIndex - 1) % numClips;
                if (currentClipIndex < 0) { currentClipIndex = numClips - 1; }
                PlayClip(currentClipIndex);
            }
        }

        #endregion
    }
}