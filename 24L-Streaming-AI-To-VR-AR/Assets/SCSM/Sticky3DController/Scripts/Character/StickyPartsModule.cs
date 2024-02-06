using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [AddComponentMenu("Sticky3D Controller/Character/Sticky Parts Module")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    [RequireComponent(typeof(StickyControlModule))]
    public class StickyPartsModule : MonoBehaviour
    {
        #region Public Variables - General
        /// <summary>
        /// If enabled, the Initialise() will be called as soon as Start() runs. This should be disabled if you are
        /// instantiating the component through code.
        /// </summary>
        public bool initialiseOnStart = false;

        /// <summary>
        /// List of parts on the character
        /// </summary>
        public List<S3DPart> s3dPartList = null;

        #endregion

        #region Public Properties - General

        /// <summary>
        /// [READONLY]
        /// Has the module been initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// [READONLY]
        /// The number of S3DParts configured with this module
        /// </summary>
        public int NumberOfParts { get { return isInitialised ? numParts : (s3dPartList == null ? 0 : s3dPartList.Count); } }

        #endregion

        #region Private Variables
        private bool isInitialised = false;
        [System.NonSerialized] private StickyControlModule stickyControlModule = null;
        [SerializeField] private bool isS3DPartListExpanded = true;
        private int numParts = 0;

        #endregion

        #region Initialise Methods
        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Private or Internal Methods

        /// <summary>
        /// Enable or disable the active state of the transform's gameobject.
        /// Only update the state if it has changed.
        /// </summary>
        /// <param name="s3dPart"></param>
        /// <param name="isEnabled"></param>
        private void EnableOrDisablePart(S3DPart s3dPart, bool isEnabled)
        {
            if (s3dPart != null && s3dPart.partTransform != null)
            {
                // Only change the state if it needs changing
                if (s3dPart.partTransform.gameObject.activeSelf != isEnabled)
                {
                    s3dPart.partTransform.gameObject.SetActive(isEnabled);
                }
            }
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Attempt to disable all parts.
        /// </summary>
        public void DisableAllParts()
        {
            if (isInitialised)
            {
                for (int ptIdx = 0; ptIdx < numParts; ptIdx++)
                {
                    EnableOrDisablePart(s3dPartList[ptIdx], false);
                }
            }
        }

        /// <summary>
        /// Attempt to disable a part
        /// </summary>
        /// <param name="s3dPart"></param>
        public void DisablePart (S3DPart s3dPart)
        {
            EnableOrDisablePart(s3dPart, false);
        }

        /// <summary>
        /// Attempt to disable a part using the zero-based index of the part
        /// </summary>
        /// <param name="index"></param>
        public void DisablePartByIndex (int index)
        {
            EnableOrDisablePart(GetPartByIndex(index), false);
        }

        /// <summary>
        /// Attempt to disable a part using the unique guidHash of the part
        /// </summary>
        /// <param name="guidHash"></param>
        public void DisablePartByHash (int guidHash)
        {
            EnableOrDisablePart(GetPartByHash(guidHash), false);
        }

        /// <summary>
        /// Attempt to enable all parts.
        /// </summary>
        public void EnableAllParts()
        {
            if (isInitialised)
            {
                for (int ptIdx = 0; ptIdx < numParts; ptIdx++)
                {
                    EnableOrDisablePart(s3dPartList[ptIdx], true);
                }
            }
        }

        /// <summary>
        /// Attempt to enable a part
        /// </summary>
        /// <param name="s3dPart"></param>
        public void EnablePart (S3DPart s3dPart)
        {
            EnableOrDisablePart(s3dPart, true);
        }

        /// <summary>
        /// Attempt to enable a part using the zero-based index of the part
        /// </summary>
        /// <param name="index"></param>
        public void EnablePartByIndex (int index)
        {
            EnableOrDisablePart(GetPartByIndex(index), true);
        }

        /// <summary>
        /// Attempt to enable a part using the unique guidHash of the part
        /// </summary>
        /// <param name="guidHash"></param>
        public void EnablePartByHash (int guidHash)
        {
            EnableOrDisablePart(GetPartByHash(guidHash), true);
        }

        /// <summary>
        /// Get a S3DPart given the unique guidHash of the part.
        /// Always returns null if not initialised or guidHash is 0.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public S3DPart GetPartByHash (int guidHash)
        {
            if (guidHash == 0 || !isInitialised) { return null; }
            else
            {
                S3DPart s3dPart = null;
                S3DPart tempPart = null;

                for (int ptIdx = 0; ptIdx < numParts; ptIdx++)
                {
                    tempPart = s3dPartList[ptIdx];
                    if (tempPart != null && tempPart.guidHash == guidHash)
                    {
                        s3dPart = tempPart;
                        break;
                    }
                }

                return s3dPart;
            }
        }

        /// <summary>
        /// Get a S3DPart given the zero-based index in the list of parts.
        /// If always returns null if not initialised or the index is out of range.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public S3DPart GetPartByIndex (int index)
        {
            if (!isInitialised || index < 0 || index > numParts-1) { return null; }
            else
            {
                return s3dPartList[index];
            }
        }

        /// <summary>
        /// Get the zero-based index of a part with the given unique guidHash value.
        /// Always returns -1 if not initialised or guidHash is 0.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public int GetPartIndex (int guidHash)
        {
            if (guidHash == 0 || !isInitialised) { return -1; }
            else
            {
                S3DPart tempPart = null;
                int index = -1;

                for (int ptIdx = 0; ptIdx < numParts; ptIdx++)
                {
                    tempPart = s3dPartList[ptIdx];
                    if (tempPart != null && tempPart.guidHash == guidHash)
                    {
                        index = ptIdx;
                        break;
                    }
                }

                return index;
            }
        }

        /// <summary>
        /// Get the unique guidHash of a part given its zero-based index in the list of parts.
        /// Always returns 0 if not initialised, the index is out of range, or the part is null.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetPartGuidHash (int index)
        {
            if (!isInitialised || index < 0 || index > numParts-1) { return 0; }
            else
            {
                S3DPart tempPart = s3dPartList[index];
                
                if (tempPart == null) { return 0; }
                else { return tempPart.guidHash; }
            }
        }

        /// <summary>
        /// Initialise the StickyPartsModule at runtime. Has no effect if already initialised.
        /// </summary>
        public void Initialise()
        {
            if (!isInitialised)
            {
                stickyControlModule = GetComponent<StickyControlModule>();

                if (stickyControlModule != null)
                {
                    // keep compiler happy
                    if (isS3DPartListExpanded) { }

                    numParts = s3dPartList == null ? 0 : s3dPartList.Count;

                    ResetParts();

                    isInitialised = true;
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("[ERROR] StickPartsModule could not find the StickyControlModule which should be on the same gameobject"); }
                #endif
            }
        }

        /// <summary>
        /// Is the part enabled, given the zero-based index of the part in the list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsPartEnabledByIndex (int index)
        {
            S3DPart s3dPart = GetPartByIndex(index);

            return s3dPart == null ? false : s3dPart.IsPartEnabled;
        }

        /// <summary>
        /// Attempt to reset the parts to their states after the module was first initialised
        /// </summary>
        public void ResetParts()
        {
            // If trying to call before Initialise() is called, numParts will be 0.
            for (int ptIdx = 0; ptIdx < numParts; ptIdx++)
            {
                S3DPart s3dPart = s3dPartList[ptIdx];

                EnableOrDisablePart(s3dPart, s3dPart.enableOnStart);
            }
        }

        /// <summary>
        /// Toggle the part on or off, given the zero-based index of the part in the list.
        /// Will have no effect if not initialised or the index is out of range. You could call
        /// this from a Custom Input on the StickyInputModule to say take on/off a helmet.
        /// </summary>
        /// <param name="index"></param>
        public void ToggleEnablePartByIndex (int index)
        {
            S3DPart s3dPart = GetPartByIndex(index);

            if (s3dPart != null)
            {
                EnableOrDisablePart(s3dPart, !s3dPart.IsPartEnabled);
            }
        }

        /// <summary>
        /// Toggle all the parts on or off. See also ToggleEnablePartByIndex(..) to toggle
        /// individual parts. Has no effect if not initialised.
        /// </summary>
        public void ToggleEnableParts()
        {
            // If trying to call before Initialise() is called, numParts will be 0.
            for (int ptIdx = 0; ptIdx < numParts; ptIdx++)
            {
                S3DPart s3dPart = s3dPartList[ptIdx];

                EnableOrDisablePart(s3dPart, !s3dPart.IsPartEnabled);
            }
        }

        #endregion

    }
}
