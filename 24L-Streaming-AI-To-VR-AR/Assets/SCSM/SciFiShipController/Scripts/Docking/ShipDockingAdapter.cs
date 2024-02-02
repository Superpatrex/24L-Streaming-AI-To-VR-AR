using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// A ship docking adapter is a location on a ship that allows it to dock
    /// with a Ship Docking Point on a Ship Docking Station.
    /// A Ship Docking component can have 1 or more Docking Adapters.
    /// NOTE: Currently only 1 adapter per ship is permitted. This may be
    /// expanded in the future.
    /// </summary>
    [System.Serializable]
    public class ShipDockingAdapter
    {
        #region Enumerations

        #endregion

        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// Local position relative to the Ship
        /// </summary>
        public Vector3 relativePosition;

        /// <summary>
        /// The direction the adapter is facing relative to the Ship. Default is down (y = -1).
        /// A +ve Z value is forwards, and -ve Z value is backwards.
        /// </summary>
        public Vector3 relativeDirection;

        /// <summary>
        /// Whether the docking adapter is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// Whether the docking adapter is shown as selected in the scene view of the editor.
        /// </summary>
        public bool selectedInSceneView;

        /// <summary>
        /// Whether the gizmos for this docking adapter are shown in the scene view of the editor.
        /// </summary>
        public bool showGizmosInSceneView;

        #endregion

        #region Constructors
        public ShipDockingAdapter()
        {
            SetClassDefaults();
        }

        public ShipDockingAdapter(ShipDockingAdapter shipDockingAdapter)
        {
            if (shipDockingAdapter == null) { SetClassDefaults(); }
            else
            {
                relativePosition = shipDockingAdapter.relativePosition;
                relativeDirection = shipDockingAdapter.relativeDirection;
            }
        }

        #endregion

        #region Private Member Methods
        public void SetClassDefaults()
        {
            relativePosition = Vector3.zero;
            relativeDirection = Vector3.down;
            showInEditor = false;
            selectedInSceneView = false;
            showGizmosInSceneView = false;
        }
        #endregion
    }
}