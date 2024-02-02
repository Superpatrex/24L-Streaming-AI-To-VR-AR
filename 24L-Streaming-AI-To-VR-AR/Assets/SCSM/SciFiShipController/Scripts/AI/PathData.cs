// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
using System.Collections.Generic;
using UnityEngine;

namespace SciFiShipController
{
    /// <summary>
    /// Class to store path data. Should not include any path
    /// manupulation code. Typically this would be in SSManager or
    /// in a user-defined class.
    /// </summary>
    [System.Serializable]
    public class PathData
    {
        #region Public variables
        // IMPORTANT When changing this section update:
        // SetClassDefaults() and PathData(PathData pathData) clone constructor

        /// <summary>
        /// A user-defined optional name for the path
        /// </summary>
        public string name;

        /// <summary>
        /// Unique identifier of the path
        /// </summary>
        public int guidHash;

        /// <summary>
        /// Path distances are out of date and need to be
        /// refreshed by calling sscManager.RefreshPathDistances(..)
        /// </summary>
        public bool isDirty;

        /// <summary>
        /// An ordered list of Locations associated with this Path. If they are
        /// orphaned or unassigned and not in the SSCManager.locationDataList,
        /// the isUnassigned flag should be set on the Location.
        /// </summary>
        public List<PathLocationData> pathLocationDataList;

        /// <summary>
        /// Whether the Path is drawn in the scene view of the editor
        /// </summary>
        public bool showGizmosInSceneView;

        /// <summary>
        /// Whether the Path Location number are displayed in the scene
        /// when the SSCManager is selected
        /// </summary>
        public bool showPointNumberLabelsInScene;

        /// <summary>
        /// Whether the Location names are displayed in the scene
        /// when the SSCManager is selected
        /// </summary>
        public bool showPointNameLabelsInScene;

        /// <summary>
        /// Whether the cummulative distances are displayed in the scene
        /// when the SSCManager is selected
        /// </summary>
        public bool showDistancesInScene;

        /// <summary>
        /// Whether the path is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// Whether the Path Locations are shown as expanded in the inspector window of the editor
        /// There could be a lot of Locations in many Paths, so collapse by default
        /// </summary>
        public bool showLocationsInEditor;

        /// <summary>
        /// Is the Path's last Location joined to the first Location to complete
        /// a closed circuit?
        /// </summary>
        public bool isClosedCircuit;

        /// <summary>
        /// The colour of the lines drawn in the scene stored as RGBA floats
        /// </summary>
        public Vector4 pathLineColour;

        /// <summary>
        /// The distance to place a new Location on the Path away from the line of sight object
        /// </summary>
        [Range(0f, 50f)] public float locationDefaultNormalOffset;

        /// <summary>
        /// The total length of the Path. Call sscManager.RefreshPathDistances(..) to
        /// update.
        /// </summary>
        public float splineTotalDistance;

        /// <summary>
        /// The maximum height in up direction that SnapToMesh will use when looking for
        /// the heighest mesh. See SCCManager.SnapToMesh(..).
        /// </summary>
        public float snapMaxHeight;

        /// <summary>
        /// The lowest mesh in the up direction that SnapToMesh will use when looking for
        /// the heighest mesh. See SCCManager.SnapToMesh(..).
        /// </summary>
        public float snapMinHeight;

        #endregion

        #region Internal or Private variables

        /// <summary>
        /// [INTERNAL ONLY]
        /// When a Path is attached to ShipDockingStation as an entry or exit Path
        /// for a ShipDockingPoint, the LocationData must be initialised so that
        /// they can correctly move with the docking station at runtime.
        /// The same Path may be used multiple times on a ShipDockingStation.
        /// </summary>
        [System.NonSerialized] internal bool isDynamicPathInitialised = false;

        /// <summary>
        /// The world velocity of the path as a vector. Derived from the world velocity
        /// of the rigidbody that the path is attached to. This would typically be a mothership
        /// for a ShipDockingStation.
        /// </summary>
        internal Vector3 worldVelocity;

        /// <summary>
        /// [INTERNAL ONLY]
        /// The world angular velocity of the path as a vector. Derived from the world velocity
        /// of the rigidbody that the path is attached to. This would typically be a mothership
        /// for a ShipDockingStation.
        /// </summary>
        internal Vector3 worldAngularVelocity;

        /// <summary>
        /// [INTERNAL ONLY]
        /// When a path is attached to a moving ShipDockingStation this is the current position
        /// of that gameobject, else it is Vector3.zero.
        /// </summary>
        internal Vector3 anchorPoint;

        /// <summary>
        /// [INTERNAL ONLY]
        /// This is used to determine if this path has already been updated in the current frame.
        /// Used with ShipDockingStation to avoid updating the same Path (and Locations) multiple
        /// times.
        /// </summary>
        [System.NonSerialized] internal float updateSeqNumber;

        #endregion

        #region Class Constructors
        public PathData()
        {
            SetClassDefaults();
        }

        // Copy constructor
        public PathData(PathData pathData)
        {
            if (pathData == null) { SetClassDefaults(); }
            else
            {
                name = pathData.name;
                guidHash = pathData.guidHash;
                isDirty = pathData.isDirty;

                if (pathData.pathLocationDataList == null) { pathLocationDataList = new List<PathLocationData>(3); }
                else { pathLocationDataList = pathData.pathLocationDataList.ConvertAll(plocData => new PathLocationData(plocData)); }

                showInEditor = pathData.showInEditor;
                showLocationsInEditor = pathData.showLocationsInEditor;
                showGizmosInSceneView = pathData.showGizmosInSceneView;
                showPointNumberLabelsInScene = pathData.showPointNumberLabelsInScene;
                showPointNameLabelsInScene = pathData.showPointNameLabelsInScene;
                showDistancesInScene = pathData.showDistancesInScene;
                isClosedCircuit = pathData.isClosedCircuit;
                pathLineColour = pathData.pathLineColour;
                locationDefaultNormalOffset = pathData.locationDefaultNormalOffset;
                splineTotalDistance = pathData.splineTotalDistance;
                snapMinHeight = pathData.snapMinHeight;
                snapMaxHeight = pathData.snapMaxHeight;
                worldVelocity = pathData.worldVelocity;
                worldAngularVelocity = pathData.worldAngularVelocity;
                anchorPoint = pathData.anchorPoint;
            }
        }
        #endregion

        #region Initialisation
        private void SetClassDefaults()
        {
            name = string.Empty;
            // Get a unique GUID then convert it to a hash for efficient non-GC access.
            guidHash = SSCMath.GetHashCodeFromGuid();
            isDirty = false;

            // Assume at least 3 points in list
            pathLocationDataList = new List<PathLocationData>(3);
            showGizmosInSceneView = true;
            showPointNumberLabelsInScene = false;
            showPointNameLabelsInScene = false;
            showDistancesInScene = false;
            showInEditor = true;
            showLocationsInEditor = false;
            isClosedCircuit = false;
            pathLineColour = new Vector4(1f,0f,0f,1f);
            locationDefaultNormalOffset = 1f;
            splineTotalDistance = 0f;

            // Used with SSCManager.SnapToMesh(..)
            snapMinHeight = 0f;
            snapMaxHeight = 2000f;

            worldVelocity = Vector3.zero;
            worldAngularVelocity = Vector3.zero;
            anchorPoint = Vector3.zero;
        }
        #endregion

        #region Overrides

        /// <summary>
        /// PathData comparison
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) { return false; }
            else
            {
                return guidHash == ((PathData)obj).guidHash;
            }
        }

        public override int GetHashCode()
        {
            return guidHash;
        }

        #endregion
    }
}