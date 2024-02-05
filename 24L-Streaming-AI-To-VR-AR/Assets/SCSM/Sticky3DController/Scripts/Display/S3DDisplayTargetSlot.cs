using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This class is used for each instance or copy of the same S3DDisplayTarget.
    /// Each S3DDisplayTarget has a list of these called displayTargetSlotList.
    /// </summary>
    public class S3DDisplayTargetSlot
    {
        #region Public Variables
        public bool showTargetSlot;

        /// <summary>
        /// The Display Target's normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.
        /// At runtime call stickyDisplayModule.SetDisplayTargetOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float offsetX;

        /// <summary>
        /// The Display Target's normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.
        /// At runtime call stickyDisplayModule.SetDisplayTargetOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float offsetY;

        #endregion

        #region Private or Internal Variables and Properties

        /// <summary>
        /// [INTERNAL ONLY]
        /// Once initialised, the RectTransform of the target slot panel
        /// </summary>
        internal RectTransform CachedTargetPanel { get; set; }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Once initialised, the Reticle Image component
        /// </summary>
        internal UnityEngine.UI.Image CachedImgComponent { get; set; }

        #endregion

        #region Constructors

        public S3DDisplayTargetSlot()
        {
            offsetX = 0f;
            offsetY = 0f;
            showTargetSlot = false;
        }

        #endregion
    }
}