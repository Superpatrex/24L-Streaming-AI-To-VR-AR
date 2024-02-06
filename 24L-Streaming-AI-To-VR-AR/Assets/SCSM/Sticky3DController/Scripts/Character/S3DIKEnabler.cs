using UnityEngine;
// Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This component can be placed on the same gameobject as the Unity Animator script
    /// when it is a child of the Sticky3D Controller to redirect IK events to S3D.
    /// </summary>
    [DisallowMultipleComponent]
    public class S3DIKEnabler : MonoBehaviour
    {
        #region Private Variables
        private StickyControlModule stickyControlModule = null;
        private bool isInitialised = false;
        #endregion

        #region Initialisation
        private void Start()
        {
            stickyControlModule = GetComponentInParent<StickyControlModule>();

            isInitialised = stickyControlModule != null;
        }
        #endregion

        #region Events

        private void OnAnimatorIK(int layerIndex)
        {
            if (isInitialised) { stickyControlModule.OnAnimatorIK(layerIndex); }
        }

        private void OnAnimatorMove()
        {
            if (isInitialised) { stickyControlModule.OnAnimatorMove(); }
        }

        #endregion
    }
}