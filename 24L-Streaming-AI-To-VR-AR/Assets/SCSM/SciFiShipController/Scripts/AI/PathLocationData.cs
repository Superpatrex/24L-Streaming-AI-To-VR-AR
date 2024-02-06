// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
using System.Collections.Generic;
using UnityEngine;

namespace SciFiShipController
{
    /// <summary>
    /// A location along a Path. This consists of a LocationData point
    /// and information about that Location unique to the current Path.
    /// </summary>
    [System.Serializable]
    public class PathLocationData
    {
        #region Public variables
        // IMPORTANT When changing this section update:
        // SetClassDefaults() and PathLocationData(PathLocationData pathLocationData) clone constructor

        /// <summary>
        /// Unique identifier of the path location
        /// </summary>
        public int guidHash;

        /// <summary>
        /// A reference to a locationData instance - typically within the
        /// sscManager.locationDataList
        /// </summary>
        public LocationData locationData;

        /// <summary>
        /// Used for the tangent of the bezier curve
        /// </summary>
        public Vector3 inControlPoint;

        /// <summary>
        /// Used for the tangent of the bezier curve
        /// </summary>
        public Vector3 outControlPoint;

        /// <summary>
        /// The initial or starting position of inControlPoint. Used for moveable
        /// Paths with ShipDockingStations at runtime.
        /// </summary>
        [System.NonSerialized] public Vector3 inInitialControlPoint;

        /// <summary>
        /// The initial or starting position of outControlPoint. Used for moveable
        /// Paths with ShipDockingStations at runtime.
        /// </summary>
        [System.NonSerialized] public Vector3 outInitialControlPoint;

        /// <summary>
        /// Is the inControl handle selected for editing in the scene?
        /// </summary>
        public bool inControlSelectedInSceneView;

        /// <summary>
        /// Is the outControl handle selected for editing in the scene?
        /// </summary>
        public bool outControlSelectedInSceneView;

        /// <summary>
        /// Whether the path location is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// The total spline distance from the start of the Path.
        /// If pathData.isClosedCircuit is enabled, the first PathLocationData
        /// point is set to the total distance of the Path. 
        /// </summary>
        public float distanceCumulative;

        /// <summary>
        /// The spline distance from the previous Location on the Path.
        /// If pathData.isClosedCircuit is enabled, on the first PathLocationData
        /// point, this is the distance from the last Location in the Path.
        /// </summary>
        public float distanceFromPreviousLocation;

        #endregion

        #region Class Constructors
        public PathLocationData()
        {
            SetClassDefaults();
        }

        // Copy constructor
        public PathLocationData(PathLocationData pathLocationData)
        {
            if (pathLocationData == null) { SetClassDefaults(); }
            else
            {
                guidHash = pathLocationData.guidHash;
                showInEditor = pathLocationData.showInEditor;
                if (pathLocationData.locationData == null) { locationData = new LocationData() { isUnassigned = true }; }
                else { locationData = new LocationData(pathLocationData.locationData); }
                inControlPoint = pathLocationData.inControlPoint;
                outControlPoint = pathLocationData.outControlPoint;
                inControlSelectedInSceneView = pathLocationData.inControlSelectedInSceneView;
                outControlSelectedInSceneView = pathLocationData.outControlSelectedInSceneView;
                distanceCumulative = pathLocationData.distanceCumulative;
                distanceFromPreviousLocation = pathLocationData.distanceFromPreviousLocation;
            }
        }

        #endregion

        #region Initialisation
        private void SetClassDefaults()
        {
            // Get a unique GUID then convert it to a hash for efficient non-GC access.
            guidHash = SSCMath.GetHashCodeFromGuid();
            showInEditor = true;
            // By default locations are unassigned - that is, not part of the sscManager.locationDataList.
            locationData = new LocationData() { isUnassigned = true };

            inControlPoint = Vector3.left;
            outControlPoint = Vector3.right;
            inControlSelectedInSceneView = false;
            outControlSelectedInSceneView = false;
            distanceCumulative = 0f;
            distanceFromPreviousLocation = 0f;
        }
        #endregion
    }
}