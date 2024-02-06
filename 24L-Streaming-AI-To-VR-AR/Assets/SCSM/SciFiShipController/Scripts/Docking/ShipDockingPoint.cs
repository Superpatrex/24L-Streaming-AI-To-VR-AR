using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This class is used with a ShipDockingStation to describe where ships can
    /// dock and undock on a station.
    /// </summary>
    [System.Serializable]
    public class ShipDockingPoint
    {
        #region Enumerations

        #endregion

        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// Local position relative to the ShipDockingStation
        /// </summary>
        public Vector3 relativePosition;

        /// <summary>
        /// Location rotation relative to the ShipDockingStation.
        /// Stored in degrees.
        /// </summary>
        public Vector3 relativeRotation;

        /// <summary>
        /// Whether the docking point is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// Whether the docking point is shown as selected in the scene view of the editor.
        /// </summary>
        public bool selectedInSceneView;

        /// <summary>
        /// Whether the gizmos for this docking point are shown in the scene view of the editor.
        /// </summary>
        public bool showGizmosInSceneView;

        /// <summary>
        /// The optional guidHash which identifies the entry path a ship can take to dock at this docking point.
        /// int guidHash = sscManager.GetPath(pathName)
        /// </summary>
        public int guidHashEntryPath;

        /// <summary>
        /// The optional guidHash which identifies the exit path a ship can take to depart from this docking point.
        /// int guidHash = sscManager.GetPath(pathName)
        /// </summary>
        public int guidHashExitPath;

        /// <summary>
        /// This is the optimum height above the docking point in the relative up direction, a ship hovers
        /// before departing or arriving.
        /// </summary>
        [Range(0f, 1000f)] public float hoverHeight;

        /// <summary>
        /// The ship currently docked, docking, or undocking from this point.
        /// </summary>
        public ShipControlModule dockedShip;

        #endregion

        #region Constructors
        public ShipDockingPoint()
        {
            SetClassDefaults();
        }

        public ShipDockingPoint(ShipDockingPoint shipDockingPoint)
        {
            if (shipDockingPoint == null) { SetClassDefaults(); }
            else
            {
                relativePosition = shipDockingPoint.relativePosition;
                relativeRotation = shipDockingPoint.relativeRotation;
                guidHashEntryPath = shipDockingPoint.guidHashEntryPath;
                guidHashExitPath = shipDockingPoint.guidHashExitPath;
                hoverHeight = shipDockingPoint.hoverHeight;
            }
        }

        #endregion

        #region Private Member Methods
        public void SetClassDefaults()
        {
            relativePosition = Vector3.zero;
            relativeRotation = Vector3.zero;
            guidHashEntryPath = 0;
            guidHashExitPath = 0;
            showInEditor = false;
            selectedInSceneView = false;
            showGizmosInSceneView = false;
            hoverHeight = 10f;
        }
        #endregion
    }
}