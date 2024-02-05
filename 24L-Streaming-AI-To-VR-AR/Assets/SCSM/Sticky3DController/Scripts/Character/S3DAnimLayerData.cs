// Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
using UnityEngine;

namespace scsmmedia
{
    /// <summary>
    /// A non-serializable runtime class to keep track of animator Layer attributes.
    /// Not to be confused with S3DAnimLayer struct in S3DUtils.cs.
    /// </summary>
    public class S3DAnimLayerData
    {
        #region Public variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// Zero-based layer number in the animator
        /// </summary>
        public int layerIndex;

        /// <summary>
        /// The weight of the layer
        /// </summary>
        [Range(0f, 1f)] public float blendWeight;

        /// <summary>
        /// Is the animator layer currently blending in?
        /// </summary>
        public bool isBlendingIn;

        /// <summary>
        /// Is the animator layer currently blending out?
        /// </summary>
        public bool isBlendingOut;

        /// <summary>
        /// The time, in seconds, that the layer will take to blend in
        /// </summary>
        public float blendInDuration;

        /// <summary>
        /// The time, in seconds, that the layer will take to blend out
        /// </summary>
        public float blendOutDuration;

        #endregion

        #region Public Delegates

        public delegate void CallBackOnBlendedIn(S3DAnimLayerData animLayerData, int layerIndex);
        public delegate void CallBackOnBlendedOut(S3DAnimLayerData animLayerData, int layerIndex);

        /// <summary>
        /// The name of your custom method that is called immediately after the blend in operation has completed.
        /// </summary>
        [System.NonSerialized] public CallBackOnBlendedIn callBackOnBlendedIn;

        /// <summary>
        /// The name of your custom method that is called immediately after the blend out operation has completed.
        /// </summary>
        [System.NonSerialized] public CallBackOnBlendedOut callBackOnBlendedOut;

        #endregion

        #region Constructors

        public S3DAnimLayerData()
        {
            SetClassDefaults();
        }

        #endregion

        #region Public Methods
        public void SetClassDefaults()
        {
            layerIndex = 0;
            blendWeight = 1f;
            isBlendingIn = false;
            isBlendingOut = false;
            blendInDuration = 1f;
            blendOutDuration = 1f;
        }

        #endregion
    }
}