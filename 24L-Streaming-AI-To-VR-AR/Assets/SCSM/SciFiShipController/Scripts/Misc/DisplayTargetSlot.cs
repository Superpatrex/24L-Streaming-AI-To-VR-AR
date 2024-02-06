using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This class is used for each instance or copy of the same DisplayTarget.
    /// Each DisplayTarget has a list of these called displayTargetSlotList.
    /// </summary>
    public class DisplayTargetSlot
    {
        #region Public Variables
        public SSCRadarItemKey radarItemKey;

        /// <summary>
        /// The Display Target's normalised offset between the left (-1) and the right (1) from the centre (0) of the screen.
        /// At runtime call shipDisplayModule.SetDisplayTargetOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float offsetX;

        /// <summary>
        /// The Display Target's normalised offset between the bottom (-1) and the top (1) from the centre (0) of the screen.
        /// At runtime call shipDisplayModule.SetDisplayTargetOffset(..)
        /// </summary>
        [Range(-1f, 1f)] public float offsetY;

        public bool showTargetSlot;

        #endregion

        #region Private or Internal Variables and Properties

        internal float fixedWeaponTargetScore;

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

        public DisplayTargetSlot()
        {
            radarItemKey = new SSCRadarItemKey() { radarItemIndex = -1, radarItemSequenceNumber = 0 };
            offsetX = 0f;
            offsetY = 0f;
            showTargetSlot = false;
            fixedWeaponTargetScore = -1;
        }

        #endregion
    }
}
