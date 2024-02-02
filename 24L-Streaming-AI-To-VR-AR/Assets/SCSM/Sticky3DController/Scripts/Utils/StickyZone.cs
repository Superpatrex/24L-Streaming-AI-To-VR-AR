using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A component that works with a trigger collider to override setting on a StickyControlModule
    /// when it enters the zone area.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Character/Sticky Zone")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyZone : MonoBehaviour
    {
        #region Enumerations


        #endregion

        #region Public Variables
        public bool initialiseOnStart = false;
        /// <summary>
        /// Does this zone override the current Sticky3D Controller with it's own?
        /// </summary>
        public bool overrideReferenceFrame = false;
        public Transform referenceTransform = null;
        /// <summary>
        /// Should the initial or default Reference Frame transform be
        /// restored when the Sticky3D Controller exits the zone?
        /// </summary>
        public bool isRestoreDefaultRefTransformOnExit = true;

        /// <summary>
        /// Should the previous Reference Frame transform be
        /// restored when the Sticky3D Controller exits the zone?
        /// If the previous is null, the initial or default one is
        /// restored.
        /// </summary>
        public bool isRestorePreviousRefTransformOnExit = false;

        /// <summary>
        /// If enabled, when entering the zone area, set Look to First Person on the Sticky3D Controller, by turning off Third Person.
        /// </summary>
        public bool overrideLookFirstPerson = false;
        /// <summary>
        /// If enabled, when entering the zone area, set Look to Third Person on the Sticky3D Controller.
        /// </summary>
        public bool overrideLookThirdPerson = false;

        /// <summary>
        /// Does the zone override the gravity of Sticky3D Controllers entering it?
        /// </summary>
        public bool overrideGravity = false;
        /// <summary>
        /// If overridden, the gravitational acceleration to apply to Sticky3D Controllers entering the zone.
        /// </summary>
        public float gravitationalAcceleration = 9.81f;

        /// <summary>
        /// Does the zone override animation clips?
        /// </summary>
        public bool overrideAnimClips = false;

        /// <summary>
        /// Should the original clips be restored when the Sticky3D Controller exits the zone?
        /// </summary>
        public bool isRestorePreviousAnimClipsOnExit = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Is this Sticky Zone initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// Get the number of animation clip sets currently assigned to this Sticky Zone
        /// </summary>
        public int NumAnimClipSets { get { return isInitialised ? numAnimClipSets : s3dAnimClipSetList == null ? 0 : s3dAnimClipSetList.Count; } }
        #endregion

        #region Internal or Private Variables
        private bool isInitialised = false;
        private Collider[] potentialZoneColliders = null;

        [SerializeField] private List<S3DAnimClipSet> s3dAnimClipSetList = null;
        private int numAnimClipSets = 0;

        /// <summary>
        /// An optional array of factionIds that will limit which characters
        /// this zone applies to.
        /// </summary>
        [SerializeField] private int[] factionsToFilter;
        private int numFactionsToFilter = 0;

        /// <summary>
        /// An optional array of modelIds that will limit which characters
        /// this zone applies to.
        /// </summary>
        [SerializeField] private int[] modelsToFilter;
        private int numModelsToFilter = 0;

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
        /// Does this zone apply to this Sticky3D character?
        /// </summary>
        /// <param name="stickyControlModule"></param>
        /// <returns></returns>
        private bool IsApplyZone(StickyControlModule stickyControlModule)
        {
            bool isApplyZone = false;

            if (stickyControlModule.isReactToStickyZonesEnabled)
            {
                // Moving the stickyControlModule outside the Array.IndexOf reduces GC
                int factionId = stickyControlModule.factionId;
                int modelId = stickyControlModule.modelId;

                // Avoid GC by using Array.IndexOf( ) > -1 rather than Array.Exists(..)
                if (
                (numFactionsToFilter < 1 || System.Array.IndexOf(factionsToFilter, factionId) > -1) &&
                (numModelsToFilter < 1 || System.Array.IndexOf(modelsToFilter, modelId) > -1) 
                )
                {
                    isApplyZone = true;
                }
            }

            return isApplyZone;
        }

        /// <summary>
        /// Replace the original animations with those specified in the Anim Clip Set scriptable object(s)
        /// </summary>
        /// <param name="stickyControlModule"></param>
        private void OverrideAnimClips(StickyControlModule stickyControlModule)
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
        /// <param name="stickyControlModule"></param>
        private void RestoreAnimClips(StickyControlModule stickyControlModule)
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

        #region Event Methods

        private void OnTriggerEnter(Collider other)
        {
            // Is anything being overridden?
            if (isInitialised && (overrideReferenceFrame || overrideLookFirstPerson || overrideLookThirdPerson || overrideGravity || (overrideAnimClips && numAnimClipSets > 0)))
            {
                // Is this a Sticky3D Controller entering the collider zone?
                StickyControlModule stickyControlModule = other.GetComponent<StickyControlModule>();

                if (stickyControlModule != null && IsApplyZone(stickyControlModule))
                {
                    //Debug.Log("[DEBUG] StickyZone " + stickyControlModule.name + " entered " + name + " ref type: " + stickyControlModule.referenceUpdateType);

                    if (overrideReferenceFrame &&
                        (stickyControlModule.refUpdateTypeInt == StickyControlModule.RefUpdateTypeManualInt ||
                         stickyControlModule.refUpdateTypeInt == StickyControlModule.RefUpdateTypeAutoFirstInt)
                       )
                    {
                        //Debug.Log("[DEBUG] Enter Zone " + gameObject.name + " is overriding Ref. Frame for " + stickyControlModule.name + " T:" + Time.time);
                        stickyControlModule.SetCurrentReferenceFrame(referenceTransform);                       
                    }

                    // If override Look first or third person is enabled, and the camera is not set to that mode, override it.
                    if ((stickyControlModule.isThirdPerson && overrideLookFirstPerson) || (!stickyControlModule.isThirdPerson && overrideLookThirdPerson))
                    {
                        stickyControlModule.ToggleFirstThirdPerson();
                    }

                    if (overrideGravity)
                    {
                        stickyControlModule.SetGravitationalAcceleration(gravitationalAcceleration);
                    }

                    if (overrideAnimClips && numAnimClipSets > 0)
                    {
                        //Debug.Log("[DEBUG] StickyZone OnTrigger enter " + other.name);
                        OverrideAnimClips(stickyControlModule);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Is anything being overridden?
            if (isInitialised && ((overrideReferenceFrame && (isRestoreDefaultRefTransformOnExit || isRestorePreviousRefTransformOnExit)) || (overrideAnimClips && numAnimClipSets > 0 && isRestorePreviousAnimClipsOnExit)))
            {
                // Is this a Sticky3D Controller exiting the collider zone?
                StickyControlModule stickyControlModule = other.GetComponent<StickyControlModule>();

                if (stickyControlModule != null && IsApplyZone(stickyControlModule))
                {
                    if (overrideReferenceFrame &&
                       (stickyControlModule.refUpdateTypeInt == StickyControlModule.RefUpdateTypeManualInt ||
                        stickyControlModule.refUpdateTypeInt == StickyControlModule.RefUpdateTypeAutoFirstInt)
                       )
                    {
                        if (isRestoreDefaultRefTransformOnExit)
                        {
                            //Debug.Log("[DEBUG] Exit Zone " + gameObject.name + " is restoring Ref. Frame for " + stickyControlModule.name + " T:" + Time.time);
                            stickyControlModule.RestoreDefaultReferenceFrame();
                        }
                        else if (isRestorePreviousRefTransformOnExit)
                        {
                            //Debug.Log("[DEBUG] Exit Zone " + gameObject.name + " is restoring Ref. Frame for " + stickyControlModule.name + " collider: " + other.GetType().Name + " T:" + Time.time);
                            stickyControlModule.RestorePreviousReferenceFrame();
                        }
                    }

                    if (overrideAnimClips && numAnimClipSets > 0 && isRestorePreviousAnimClipsOnExit)
                    {
                        //Debug.Log("[DEBUG] StickyZone OnTrigger exit " + other.name);
                        RestoreAnimClips(stickyControlModule);
                    }
                }
            }
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Add animation clip set to the Sticky Zone
        /// </summary>
        /// <param name="s3dAnimClipSet"></param>
        public void AddAnimClipSet(S3DAnimClipSet s3dAnimClipSet)
        {
            if (s3dAnimClipSetList == null) { s3dAnimClipSetList = new List<S3DAnimClipSet>(5); }

            if (s3dAnimClipSetList != null)
            {
                s3dAnimClipSetList.Add(s3dAnimClipSet);
                numAnimClipSets = s3dAnimClipSetList == null ? 0 : s3dAnimClipSetList.Count;
            }
        }

        /// <summary>
        /// Remove an animation clip set from the Sticky Zone
        /// </summary>
        /// <param name="s3dAnimClipSet"></param>
        public void RemoveAnimClipSet(S3DAnimClipSet s3dAnimClipSet)
        {
            if (s3dAnimClipSetList != null && s3dAnimClipSet != null)
            {
                s3dAnimClipSetList.Remove(s3dAnimClipSet);
            }
        }

        /// <summary>
        /// Remove the zero-based index of a Animation Clip Set assigned to this Sticky Zone
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAnimClipSet(int index)
        {
            if (NumAnimClipSets > 0 && index >= 0 && index < NumAnimClipSets)
            {
                s3dAnimClipSetList.RemoveAt(index);
            }
        }

        /// <summary>
        /// Initialise the Sticky Zone
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            // Find all the colliders attached to this object
            potentialZoneColliders = GetComponents<Collider>();

            numAnimClipSets = s3dAnimClipSetList == null ? 0 : s3dAnimClipSetList.Count;
            numFactionsToFilter = factionsToFilter == null ? 0 : factionsToFilter.Length;
            numModelsToFilter = modelsToFilter == null ? 0 : modelsToFilter.Length;

            int numColliders = 0;
            if (potentialZoneColliders != null)
            {
                // Loop through them to find potential zone colliders
                numColliders = potentialZoneColliders.Length;
                if (numColliders > 0)
                {
                    for (int i = 0; i < numColliders; i++)
                    {
                        // Check if the collider is a trigger type
                        if (potentialZoneColliders[i] != null && potentialZoneColliders[i].isTrigger)
                        {
                            // Stop iterating if we find a suitable zone collider
                            isInitialised = true;
                            i = numColliders;
                        }
                    }
                }
            }

            #if UNITY_EDITOR
            if (!isInitialised)
            {
                if (numColliders > 0)
                {
                    Debug.LogWarning("[ERROR] StickyZone the collider must be a trigger");
                }
                else
                {
                    Debug.LogWarning("[ERROR] StickyZone could not a collider component. Did you attach one to this gameobject?");
                }
            }
            #endif
        }

        /// <summary>
        /// Set a new Factions Filter
        /// </summary>
        /// <param name="newFactionsFilter"></param>
        public void SetFactionsFilter(int[] newFactionsFilter)
        {
            factionsToFilter = newFactionsFilter;
            numFactionsToFilter = factionsToFilter == null ? 0 : factionsToFilter.Length;
        }

        /// <summary>
        /// Set a new Models Filter
        /// </summary>
        /// <param name="newModelsFilter"></param>
        public void SetModelsFilter(int[] newModelsFilter)
        {
            modelsToFilter = newModelsFilter;
            numModelsToFilter = modelsToFilter == null ? 0 : modelsToFilter.Length;
        }

        #endregion
    }
}

