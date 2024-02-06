// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
using System.Collections.Generic;
using UnityEngine;

namespace SciFiShipController
{
    /// <summary>
    /// Class to store location data. Should not include any location
    /// manupulation code. Typically this would be in SSManager or
    /// in a user-defined class. See also
    /// SSCManager.AppendLocation(..)
    /// </summary>
    [System.Serializable]
    public class LocationData
    {
        #region Public variables
        // IMPORTANT When changing this section update:
        // SetClassDefaults() and LocationData(LocationData locationData) clone constructor

        /// <summary>
        /// A user-defined optional name for the location e.g. Enemy Base
        /// </summary>
        public string name;

        /// <summary>
        /// The position in worldspace of the location
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// The initial or starting position of Location. Used for moveable
        /// Paths with ShipDockingStations at runtime.
        /// </summary>
        [System.NonSerialized] public Vector3 initialPosition;

        /// <summary>
        /// Unique identifier of the location
        /// </summary>
        public int guidHash;

        /// <summary>
        /// Whether the location is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// Whether the location is shown as selected in the scene view of the editor.
        /// </summary>
        public bool selectedInSceneView;

        /// <summary>
        /// Whether the gizmos for this location are shown in the scene view of the editor
        /// </summary>
        public bool showGizmosInSceneView;

        /// <summary>
        /// [INTERNAL USE ONLY] Indicates the location is not a member of the SSCManager
        /// locationDataList. This can occur when an empty slot is created in a Path. 
        /// </summary>
        public bool isUnassigned;

        /// <summary>
        /// The faction or alliance the Location belongs to. This can be used to identify if a Location is friend or foe.
        /// Default (neutral) is 0
        /// </summary>
        public int factionId;

        /// <summary>
        /// The relative size of the blip for this Location on the Radar mini-map
        /// </summary>
        [Range(1,5)] public byte radarBlipSize;

        /// <summary>
        /// [INTERNAL USE ONLY] Instead, call sscManager.EnableRadar(..) or DisableRadar(..).
        /// </summary>
        public bool isRadarEnabled;

        /// <summary>
        /// [READONLY] The number used by the SSCRadar system to identify this Location at a point in time.
        /// This should not be stored across frames and is updated as required by the system.
        /// </summary>
        public int RadarId { get { return radarItemIndex; } }

        #endregion

        #region Private variables
        internal int radarItemIndex;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Use with sscManager.AddLocation(gameObject)
        /// and sscManager.GetLocationByGameObjectID(id)
        /// </summary>
        internal int gameObjectInstanceId;
        #endregion

        #region Class Constructors
        public LocationData()
        {
            SetClassDefaults();
        }

        // Copy constuctor
        public LocationData(LocationData locationData)
        {
            if (locationData == null) { SetClassDefaults(); }
            else
            {
                name = locationData.name;
                position = locationData.position;
                guidHash = locationData.guidHash;
                showInEditor = locationData.showInEditor;
                selectedInSceneView = locationData.selectedInSceneView;
                showGizmosInSceneView = locationData.showGizmosInSceneView;
                isUnassigned = locationData.isUnassigned;
                isRadarEnabled = locationData.isRadarEnabled;
                factionId = locationData.factionId;
                radarBlipSize = locationData.radarBlipSize;
            }
        }
        #endregion

        #region Initialisation
        private void SetClassDefaults()
        {
            name = string.Empty;
            // Get a unique GUID then convert it to a hash for efficient non-GC access.
            guidHash = SSCMath.GetHashCodeFromGuid();
            position = Vector3.zero;
            showInEditor = true;
            selectedInSceneView = false;
            showGizmosInSceneView = true;
            isUnassigned = false;
            isRadarEnabled = false;
            radarItemIndex = -1;
            factionId = 0;
            radarBlipSize = 1;
            gameObjectInstanceId = 0;
        }
        #endregion
    }
}