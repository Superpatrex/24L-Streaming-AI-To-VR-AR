using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2020-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A component that can be added to a StickyControlModule which replaces
    /// sets of animation clips on a character.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Character/Sticky Anim Replacer")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyAnimReplacer : MonoBehaviour
    {
        #region Enumerations

        #endregion

        #region Public Variables
        public bool initialiseOnStart = false;
        #endregion

        #region Public Properties

        /// <summary>
        /// Is this Sticky Zone initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// Get the number of animation clip sets for this Anim Replacer.
        /// </summary>
        public int NumAnimClipSets { get { return isInitialised ? numAnimClipSets : s3dAnimClipSetList == null ? 0 : s3dAnimClipSetList.Count; } }
        #endregion

        #region Internal or Private Variables
        private bool isInitialised = false;
        private StickyControlModule stickyControlModule = null;

        [SerializeField] private List<S3DAnimClipSet> s3dAnimClipSetList = null;

        /// <summary>
        /// Keeps a list of the animations that have been replaced at runtime
        /// </summary>
        private List<bool> isReplacedList = null;
        private int numAnimClipSets = 0;

        #endregion

        #region Initialisation Methods

        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Internal or Private Methods

        /// <summary>
        /// Replace the original animations with those specified in the Anim Clip Set scriptable object(s)
        /// </summary>
        private void OverrideAnimClips ()
        {
            // Loop though the sets of clips to override
            for (int animClipSetIdx = 0; animClipSetIdx < numAnimClipSets; animClipSetIdx++)
            {
                // Get the scriptable object.
                S3DAnimClipSet s3dAnimClipSet = s3dAnimClipSetList[animClipSetIdx];

                if (s3dAnimClipSet != null)
                {
                    int numAnimClipPairs = s3dAnimClipSet.animClipPairList == null ? 0 : s3dAnimClipSet.animClipPairList.Count;

                    // Loop through all the Clip Pairs in the Animation Clip Set scriptable object
                    for (int animClipPairIdx = 0; animClipPairIdx < numAnimClipPairs; animClipPairIdx++)
                    {
                        S3DAnimClipPair clipPair = s3dAnimClipSet.animClipPairList[animClipPairIdx];

                        stickyControlModule.ReplaceAnimationClipNoRef(clipPair.originalClip, clipPair.replacementClip);

                        //Debug.Log("[DEBUG] override clip " + clipPair.originalClip.name + " with " + clipPair.replacementClip.name);
                    }
                }
            }
        }

        /// <summary>
        /// Replace the current animations with the original Anim Clips specified in the Anim Clip Set scriptable object(s)
        /// </summary>
        private void RestoreAnimClips()
        {
            // Loop though the sets of clips to override
            for (int animClipSetIdx = 0; animClipSetIdx < numAnimClipSets; animClipSetIdx++)
            {
                // Get the scriptable object.
                S3DAnimClipSet s3dAnimClipSet = s3dAnimClipSetList[animClipSetIdx];

                if (s3dAnimClipSet != null)
                {
                    int numAnimClipPairs = s3dAnimClipSet.animClipPairList == null ? 0 : s3dAnimClipSet.animClipPairList.Count;

                    // Loop through all the Clip Pairs in the Animation Clip Set scriptable object
                    for (int animClipPairIdx = 0; animClipPairIdx < numAnimClipPairs; animClipPairIdx++)
                    {
                        S3DAnimClipPair clipPair = s3dAnimClipSet.animClipPairList[animClipPairIdx];

                        stickyControlModule.ReplaceAnimationClipNoRef(clipPair.originalClip, clipPair.originalClip);

                        //Debug.Log("[DEBUG] override clip " + clipPair.originalClip.name + " with " + clipPair.replacementClip.name);
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add animation clip set to the StickyAnimReplacer
        /// </summary>
        /// <param name="s3dAnimClipSet"></param>
        public void AddAnimClipSet (S3DAnimClipSet s3dAnimClipSet)
        {
            if (s3dAnimClipSetList == null) { s3dAnimClipSetList = new List<S3DAnimClipSet>(5); }

            if (s3dAnimClipSetList != null)
            {
                s3dAnimClipSetList.Add(s3dAnimClipSet);
                if (isInitialised) { isReplacedList.Add(false); }
                numAnimClipSets = s3dAnimClipSetList == null ? 0 : s3dAnimClipSetList.Count;
            }
        }

        /// <summary>
        /// Initialise the Sticky Anim Replacer. Call this if you haven’t enabled “Initialise On Start” in the editor.
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            if (!TryGetComponent(out stickyControlModule))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] StickyAnimReplacer.Initialise - could not find StickyControlModule component on " + name);
                #endif
            }
            else
            {
                // If the S3D module is not initialised and it probably didn't fail when first initialised,
                // try and initialise it now
                if (!stickyControlModule.IsInitialised && !stickyControlModule.initialiseOnAwake)
                {
                    stickyControlModule.Initialise();
                }

                if (!stickyControlModule.IsInitialised)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("[ERROR] StickyAnimReplacer.Initialise - the StickyControlModule component is not initialised on " + name);
                    #endif
                }
                else
                {
                    numAnimClipSets = s3dAnimClipSetList == null ? 0 : s3dAnimClipSetList.Count;

                    // Provide a capacity of at least 1.
                    isReplacedList = new List<bool>(numAnimClipSets + 1);

                    // Start with none being replaced.
                    for (int animClipIdx = 0; animClipIdx < numAnimClipSets; animClipIdx++)
                    {
                        isReplacedList.Add(false);
                    }

                    isInitialised = true;
                }
            }
        }

        /// <summary>
        /// Replace the animation on the character with those in the anim clip set using
        /// the number of the Animation Clip Set on the component.
        /// </summary>
        /// <param name="clipSetNumber"></param>
        public void ReplaceAnimClipSet (int clipSetNumber)
        {
            if (isInitialised)
            {
                if (clipSetNumber > 0 && clipSetNumber <= numAnimClipSets)
                {
                    // Get the scriptable object.
                    S3DAnimClipSet s3dAnimClipSet = s3dAnimClipSetList[clipSetNumber-1];

                    if (s3dAnimClipSet != null)
                    {
                        int numAnimClipPairs = s3dAnimClipSet.animClipPairList == null ? 0 : s3dAnimClipSet.animClipPairList.Count;

                        // Loop through all the Clip Pairs in the Animation Clip Set scriptable object
                        for (int animClipPairIdx = 0; animClipPairIdx < numAnimClipPairs; animClipPairIdx++)
                        {
                            S3DAnimClipPair clipPair = s3dAnimClipSet.animClipPairList[animClipPairIdx];

                            stickyControlModule.ReplaceAnimationClipNoRef(clipPair.originalClip, clipPair.replacementClip);

                            //Debug.Log("[DEBUG] override clip " + clipPair.originalClip.name + " with " + clipPair.replacementClip.name);
                        }

                        isReplacedList[clipSetNumber-1] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Restore the animation on the character with the original ones in the anim clip set using
        /// the number of the Animation Clip Set on the component.
        /// </summary>
        /// <param name="clipSetIndex"></param>
        public void RestoreAnimClipSet (int clipSetNumber)
        {
            if (isInitialised)
            {
                if (clipSetNumber > 0 && clipSetNumber <= numAnimClipSets)
                {
                    // Get the scriptable object.
                    S3DAnimClipSet s3dAnimClipSet = s3dAnimClipSetList[clipSetNumber-1];

                    if (s3dAnimClipSet != null)
                    {
                        int numAnimClipPairs = s3dAnimClipSet.animClipPairList == null ? 0 : s3dAnimClipSet.animClipPairList.Count;

                        // Loop through all the Clip Pairs in the Animation Clip Set scriptable object
                        for (int animClipPairIdx = 0; animClipPairIdx < numAnimClipPairs; animClipPairIdx++)
                        {
                            S3DAnimClipPair clipPair = s3dAnimClipSet.animClipPairList[animClipPairIdx];

                            stickyControlModule.ReplaceAnimationClipNoRef(clipPair.originalClip, clipPair.originalClip);

                            //Debug.Log("[DEBUG] override clip " + clipPair.originalClip.name + " with " + clipPair.replacementClip.name);
                        }

                        isReplacedList[clipSetNumber-1] = false;
                    }
                }
            }
        }

        /// <summary>
        /// Remove an animation clip set from the StickyAnimReplacer
        /// </summary>
        /// <param name="s3dAnimClipSet"></param>
        public void RemoveAnimClipSet (S3DAnimClipSet s3dAnimClipSet)
        {
            if (s3dAnimClipSetList != null && s3dAnimClipSet != null)
            {
                if (isInitialised)
                {
                    for (int animClipSetIdx = 0; animClipSetIdx < numAnimClipSets; animClipSetIdx++)
                    {
                        if (s3dAnimClipSetList[animClipSetIdx] == s3dAnimClipSet)
                        {
                            isReplacedList.RemoveAt(animClipSetIdx);
                            break;
                        }
                    }
                }
                s3dAnimClipSetList.Remove(s3dAnimClipSet);
            }
        }

        /// <summary>
        /// Remove the zero-based index of an Animation Clip Set assigned to this StickyAnimReplacer
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAnimClipSet (int index)
        {
            if (NumAnimClipSets > 0 && index >= 0 && index < NumAnimClipSets)
            {
                if (isInitialised)
                {
                    isReplacedList.RemoveAt(index);
                }
                s3dAnimClipSetList.RemoveAt(index);
            }
        }

        /// <summary>
        /// Replace or restore a set of animation clips on the character
        /// using the Animation Clip Set number on this component.
        /// </summary>
        /// <param name="clipSetIndex"></param>
        public void ToggleAnimClipSet (int clipSetNumber)
        {
            if (isInitialised && clipSetNumber > 0 && clipSetNumber <= numAnimClipSets)
            {
                if (isReplacedList[clipSetNumber-1])
                {
                    RestoreAnimClipSet(clipSetNumber);
                }
                else
                {
                    ReplaceAnimClipSet(clipSetNumber);
                }
            }
        }

        #endregion

    }
}