using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple script to play an animation state, starting a normalised time from the beginning of the clip.
    /// This component can be added to a StickyControlModule or an empty gameobject in the scene.
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Anim Play with Offset")]
    [DisallowMultipleComponent]
    public class SampleAnimPlayWithOffset : MonoBehaviour
    {
        #region Public Variables

        [Tooltip("If this component is attached to the parent gameobject of a S3D character, it will auto-populate this field")]
        public StickyControlModule stickyControlModule = null;

        [Tooltip("If enabled, the Initialise() will be called as soon as Start() runs. This should be disabled if you are instantiating the component through code.")]
        public bool initialiseOnStart = true;

        [Tooltip("The zero-based index of the Animator Controller layer than contains the State to play.")]
        public int controllerLayerIndex = -1;

        [Tooltip("The name of the State within Animator Controller that will play the clip. e.g. Action")]
        public string animatorStateName = null;

        [Tooltip("The normalised transistion (0.0 to 1.0), between the current animation state and the new one")]
        [Range(0f, 1f)] public float transitionNormalised = 0.2f;

        #endregion

        #region Private Variables
        
        private bool isInitialised = false;
        private int animatorStateId = 0;

        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialise this component
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            // For the sake of the sample, do a bunch of error checking. In game code, you would probably
            // do all this checking on one line.
            if (stickyControlModule == null && !TryGetComponent(out stickyControlModule))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleAnimPlayWithOffset - did you forget to attach this script to one of our player S3D characters?");
                #endif
            }
            else if (!stickyControlModule.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleAnimPlayWithOffset - your character " + stickyControlModule.name + " is not initialised");
                #endif
            }
            else if (string.IsNullOrEmpty(animatorStateName))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleAnimPlayWithOffset - please enter the name of the State in the Animator Controller that contains the clip you wish to play. e.g. Action");
                #endif
            }
            else if (!stickyControlModule.IsAnimateEnabled)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleAnimPlayWithOffset - animate does not seem to be enabled, available, or configured on " + stickyControlModule.name);
                #endif
            }
            else
            {
                animatorStateId = stickyControlModule.GetAnimationStateId(animatorStateName);

                isInitialised = animatorStateId != 0;
            }
        }

        /// <summary>
        /// Play an animation state, starting at a normalised offset from the beginning of the clip.
        /// 0.0 is start from the beginning. 0.9 is start near the end.
        /// </summary>
        /// <param name="offsetNormalised">Range 0.0 to 1.0</param>
        public void PlayAnimationWithOffset (float offsetNormalised)
        {
            if (isInitialised)
            {
                // Play the clip, starting a normalised time from the beginning.

                if (offsetNormalised < 0f) { offsetNormalised = 0f; }
                else if (offsetNormalised > 1f) { offsetNormalised = 1f; }

                stickyControlModule.PlayAnimationStateWithOffset(animatorStateId, controllerLayerIndex, transitionNormalised, offsetNormalised);
            }
        }

        #endregion
    }
}