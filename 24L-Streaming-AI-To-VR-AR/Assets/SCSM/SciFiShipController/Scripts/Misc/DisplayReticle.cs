using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class containing data for a Ship Display's recticle.
    /// This is the aiming device in the heads-up display
    /// </summary>
    [System.Serializable]
    public class DisplayReticle
    {
        #region Public variables

        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// The sprite (texture) that will be displayed in the HUD
        /// </summary>
        public Sprite primarySprite;

        /// <summary>
        /// Whether the reticle is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// Hashed GUID code to uniquely identify a reticle.
        /// [INTERNAL USE ONLY]
        /// </summary>
        public int guidHash;

        #endregion

        #region Private variables

        #endregion

        #region Constructors
        public DisplayReticle()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// DisplayReticle copy constructor
        /// </summary>
        /// <param name="displayReticle"></param>
        public DisplayReticle(DisplayReticle displayReticle)
        {
            if (displayReticle == null) { SetClassDefaults(); }
            else
            {
                primarySprite = displayReticle.primarySprite;
                showInEditor = displayReticle.showInEditor;
                guidHash = displayReticle.guidHash;
            }
        }
        #endregion

        #region Public Member Methods

        /// <summary>
        /// Set the defaults values for this class
        /// </summary>
        public void SetClassDefaults()
        {
            primarySprite = null;
            showInEditor = false;
            guidHash = SSCMath.GetHashCodeFromGuid();
        }

        #endregion
    }
}