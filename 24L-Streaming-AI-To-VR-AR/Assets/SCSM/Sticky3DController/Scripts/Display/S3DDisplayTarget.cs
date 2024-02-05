using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class containing data for a Sticky Display's target.
    /// This is used to track target objects on the heads-up display.
    /// </summary>
    [System.Serializable]
    public class S3DDisplayTarget
    {
        #region Public Static variables
        public static readonly int MAX_DISPLAYTARGET_SLOTS = 10;
        #endregion

        #region Public variables

        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// Whether the target is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// The guidHash of the displayed reticle used for this target
        /// </summary>
        public int guidHashDisplayReticle;

        /// <summary>
        /// The width, in pixels of the target panel. This is scaled with the panel.
        /// Assumes default HUD width is 1920.
        /// </summary>
        public int width;

        /// <summary>
        /// The height, in pixels of the target panel. This is scaled with the panel.
        /// Assumes default HUD height is 1080.
        /// </summary>
        public int height;

        /// <summary>
        /// Hashed GUID code to uniquely identify a target.
        /// [INTERNAL USE ONLY]
        /// </summary>
        public int guidHash;

        /// <summary>
        /// Show(or hide) the target. At runtime use stickyDisplayModule.ShowDisplayTarget() or HideDisplayTarget().
        /// </summary>
        public bool showTarget;

        /// <summary>
        /// Colour of the target reticle sprite.
        /// At runtime call stickyDisplayModule.SetDisplayTargetReticleColour(..).
        /// </summary>
        public Color reticleColour;

        /// <summary>
        /// The maximum number of these DisplayTargets that can be shown on the
        /// HUD at any one time. Should not exceed MAX_DISPLAYTARGET_SLOTS.
        /// To change the number of slots at runtime call stickyDisplayModule.AddTargetSlots(..)
        /// </summary>
        [Range(1, 10)] public int maxNumberOfTargets;

        #endregion

        #region Private or Internal variables and properties - not serialized

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used for reticle brightness
        /// </summary>
        internal S3DColour baseReticleColour;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Stores a list of the current active display targets
        /// </summary>
        internal List<S3DDisplayTargetSlot> displayTargetSlotList;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        internal bool isInitialised;

        #endregion

        #region Constructors
        public S3DDisplayTarget()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// DisplayReticle copy constructor
        /// </summary>
        /// <param name="displayTarget"></param>
        public S3DDisplayTarget(S3DDisplayTarget displayTarget)
        {
            if (displayTarget == null) { SetClassDefaults(); }
            else
            {
                guidHashDisplayReticle = displayTarget.guidHashDisplayReticle;
                showInEditor = displayTarget.showInEditor;
                guidHash = displayTarget.guidHash;
                showTarget = displayTarget.showTarget;
                width = displayTarget.width;
                height = displayTarget.height;
                reticleColour = new Color(displayTarget.reticleColour.r, displayTarget.reticleColour.g, displayTarget.reticleColour.b, displayTarget.reticleColour.a);

                maxNumberOfTargets = displayTarget.maxNumberOfTargets;
            }
        }
        #endregion

        #region Public Member Methods

        /// <summary>
        /// Set the defaults values for this class
        /// </summary>
        public void SetClassDefaults()
        {
            guidHashDisplayReticle = 0; // Unassigned
            showInEditor = false;
            guidHash = S3DMath.GetHashCodeFromGuid();
            showTarget = false; // off by default
            width = 64;
            height = 64;
            reticleColour = Color.red;
            maxNumberOfTargets = 1;
            displayTargetSlotList = new List<S3DDisplayTargetSlot>(10);
        }

        #endregion
    }
}