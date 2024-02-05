using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class containing data for a Sticky Display's recticle.
    /// This is the aiming device in the heads-up display
    /// </summary>
    [System.Serializable]
    public class S3DDisplayReticle
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
        public S3DDisplayReticle()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// S3DDisplayReticle copy constructor
        /// </summary>
        /// <param name="displayReticle"></param>
        public S3DDisplayReticle(S3DDisplayReticle displayReticle)
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
            guidHash = S3DMath.GetHashCodeFromGuid();
        }

        #endregion
    }
}