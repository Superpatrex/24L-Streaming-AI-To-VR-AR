using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class containing data for a Ship Display's target.
    /// This is used to track target ships or objects on the heads-up display
    /// </summary>
    [System.Serializable]
    public class DisplayTarget
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
        /// Show(or hide) the target. At runtime use shipDisplayModule.ShowDisplayTarget() or HideDisplayTarget().
        /// </summary>
        public bool showTarget;

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
        /// Colour of the target reticle sprite.
        /// At runtime call shipDisplayModule.SetDisplayTargetReticleColour(..).
        /// </summary>
        public Color reticleColour;

        /// <summary>
        /// [OPTIONAL] An array of factionIds to include.
        /// If you define it for one DisplayTarget, you must configure it on all DisplayTargets.
        /// </summary>
        public int[] factionsToInclude;

        /// <summary>
        /// [OPTIONAL] An array of squadronIds to include.
        /// If you define it for one DisplayTarget, you must configure it on all DisplayTargets.
        /// </summary>
        public int[] squadronsToInclude;

        /// <summary>
        /// Can this DisplayTarget be assigned to a weapon?
        /// </summary>
        public bool isTargetable;

        /// <summary>
        /// Hashed GUID code to uniquely identify a target.
        /// [INTERNAL USE ONLY]
        /// </summary>
        public int guidHash;

        /// <summary>
        /// The maximum number of these DisplayTargets that can be shown on the
        /// HUD at any one time. Should not exceed MAX_DISPLAYTARGET_SLOTS.
        /// To change the number of slots at runtime call shipDisplayModule.AddTargetSlots(..)
        /// </summary>
        [Range(1, 10)] public int maxNumberOfTargets;

        #endregion

        #region Private or Internal variables and properties - not serialized

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used for reticle brightness
        /// </summary>
        internal SSCColour baseReticleColour;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Stores a list of the current active display targets
        /// </summary>
        internal List<DisplayTargetSlot> displayTargetSlotList;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        internal bool isInitialised;

        #endregion

        #region Constructors
        public DisplayTarget()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// DisplayReticle copy constructor
        /// </summary>
        /// <param name="displayTarget"></param>
        public DisplayTarget(DisplayTarget displayTarget)
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

                if (displayTarget.factionsToInclude == null) { factionsToInclude = null; }
                else { factionsToInclude = displayTarget.factionsToInclude.Clone() as int[]; }

                if (displayTarget.squadronsToInclude == null) { squadronsToInclude = null; }
                else { squadronsToInclude = displayTarget.squadronsToInclude.Clone() as int[]; }

                isTargetable = displayTarget.isTargetable;
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
            guidHash = SSCMath.GetHashCodeFromGuid();
            showTarget = false; // off by default
            width = 64;
            height = 64;
            reticleColour = Color.red;
            factionsToInclude = null;
            squadronsToInclude = null;
            isTargetable = true;
            maxNumberOfTargets = 1;
            displayTargetSlotList = new List<DisplayTargetSlot>(10);
        }

        //public override string ToString()
        //{
            
        //}

        #endregion
    }
}