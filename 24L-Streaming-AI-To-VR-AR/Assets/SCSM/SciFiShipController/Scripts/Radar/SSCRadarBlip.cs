using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// A struct used to transmit data from the radar system to a script that
    /// has run a query requesting information for the purposes of situation awareness.
    /// Generally this data only persists for a single frame.
    /// </summary>
    /// [Unity.VisualScripting.Inspectable]
    public struct SSCRadarBlip
    {
        #region Public variables
        // Vector3 = 12 bytes which is the same as 3 x float.

        /// [Unity.VisualScripting.Inspectable]
        public SSCRadarItem.RadarItemType radarItemType;

        /// <summary>
        /// World space position in the scene of this item
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public Vector3 wsPosition;

        /// <summary>
        /// The squared 3D distance from the centre of the radar collector.
        /// To get the actual distance Mathf.Sqrt(sscRadarBlip.distanceSqr3D).
        /// Where possible use the squared distance for better performance.
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public float distanceSqr3D;

        /// <summary>
        /// The squared 2D distance from the centre of the radar collector.
        /// To get the actual distance Mathf.Sqrt(sscRadarBlip.distanceSqr2D)
        /// Where possible use the squared distance for better performance.
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public float distanceSqr2D;

        /// <summary>
        /// Reference to the gameobject in the scene when radarItemType is GameObject 
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public GameObject itemGameObject;

        /// <summary>
        /// Reference to a ship in the scene when radarItemType is AIShip or PlayerShip
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public ShipControlModule shipControlModule;

        /// <summary>
        /// The faction or alliance the item belongs to. This can be used to identify if an item is friend or foe.
        /// factionId of 0 is everyone is on the same side or neutral
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public int factionId;

        /// <summary>
        /// The squadron this item is a member of. Typically only applies to Ships.
        /// Default: -1 (not set)
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public int squadronId;

        /// <summary>
        /// Unique identifier used for RadarItemTypes:
        /// Location, GameObject, Path (future). Default value is 0.
        /// GameObject types default this to gameObject.GetHashCode()
        /// Can use sscManager.GetLocation(guidHash) to retrieve the LocationData.
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public int guidHash;

        /// <summary>
        /// The relative size of the blip on the radar mini-map
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public byte blipSize;

        #endregion

        #region Private or Internal variables
        internal int radarItemIndex;
        internal uint radarItemSequenceNumber;
        #endregion
    }
}