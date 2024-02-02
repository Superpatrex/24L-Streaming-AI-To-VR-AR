using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Data class to record meta-data about an items position etc. within an environment
    /// for the SSCRadar system. This data could be represented as a series of blips on
    /// a minimap or radar display. There is a one-to-one relationship between the SSCRadarItem
    /// and a gameobject or entity in the scene.
    /// Typically, this data is broadcast (sent) from a ship to the central radar system
    /// using SSCRadarPacket which contains a subset of this class.
    /// Some fields like radarItemType rarely change so aren't transmitted with each update.
    /// </summary>
    /// [Unity.VisualScripting.Inspectable]
    public class SSCRadarItem
    {
        #region Enumerations

        /// <summary>
        /// Location = LocationData
        /// Path = PathData
        /// </summary>
        public enum RadarItemType
        {
            AIShip = 0,
            PlayerShip = 1,
            Location = 2,
            // Path = 3,
            GameObject = 4,
            ShipDamageRegion = 5,
            Custom = 999
        }
        #endregion

        #region Public variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        public RadarItemType radarItemType;

        /// <summary>
        /// World space position in the scene of this item
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// The current velocity of the item being tracked
        /// </summary>
        public Vector3 velocity;

        /// <summary>
        /// Is this item currently visible to radar queries?
        /// For example, when a ship has been destroyed or is respawning, this should
        /// be set to false.
        /// </summary>
        public bool isVisibleToRadar;

        /// <summary>
        /// The faction or alliance the item belongs to. This can be used to identify if an item is friend or foe.
        /// Default (neutral) is 0
        /// </summary>
        public int factionId;

        /// <summary>
        /// The squadron this item is a member of. Typically only applies to Ships.
        /// Default is -1 (not set)
        /// </summary>
        public int squadronId;

        /// <summary>
        /// Unique identifier used for RadarItemTypes:
        /// Location, GameObject, ShipDamageRegion, Path (future). Default value is 0.
        /// GameObject types default this to gameObject.GetHashCode()
        /// </summary>
        public int guidHash;

        /// <summary>
        /// Reference to the gameobject in the scene when radarItemType is GameObject 
        /// </summary>
        public GameObject itemGameObject;

        /// <summary>
        /// Reference to a ship in the scene when radarItemType is AIShip or PlayerShip
        /// </summary>
        public ShipControlModule shipControlModule;

        /// <summary>
        /// The relative size of the blip on the radar mini-map
        /// Must be between 1 and 5 inclusive.
        /// </summary>
        public byte blipSize;

        #endregion

        #region Internal Variables

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used to determine uniqueness
        /// </summary>
        internal uint itemSequenceNumber;

        #endregion

        #region Internal Static Variables
        internal static int RadarItemTypeLocationInt = (int)RadarItemType.Location;
        internal static int RadarItemTypeGameObjectInt = (int)RadarItemType.GameObject;
        internal static uint nextSequenceNumber = 1;
        #endregion

        #region Class Constructors

        public SSCRadarItem()
        {
            SetClassDefaults();
        }

        public SSCRadarItem(SSCRadarItem sscRadarItem)
        {
            if (sscRadarItem == null) { SetClassDefaults(); }
            else
            {
                radarItemType = sscRadarItem.radarItemType;
                position = sscRadarItem.position;
                velocity = sscRadarItem.velocity;
                isVisibleToRadar = sscRadarItem.isVisibleToRadar;
                factionId = sscRadarItem.factionId;
                squadronId = sscRadarItem.squadronId;
                guidHash = sscRadarItem.guidHash;
                itemGameObject = sscRadarItem.itemGameObject;
                shipControlModule = sscRadarItem.shipControlModule;
                blipSize = sscRadarItem.blipSize;
                itemSequenceNumber = sscRadarItem.itemSequenceNumber;
            }
        }

        #endregion

        #region Internal Member Methods
 
        /// <summary>
        /// Makes this radarItem unique from all others that have gone before them.
        /// This is called every time radarItem is enabled. 
        /// </summary>
        internal void IncrementSequenceNumber()
        {
            itemSequenceNumber = nextSequenceNumber++;
            // if sequence number needs to be wrapped, do so to a high-ish number that is unlikely to be in use 
            if (nextSequenceNumber > uint.MaxValue - 100) { nextSequenceNumber = 100000; }
        }

        #endregion

        #region Public Member Methods

        public void SetClassDefaults()
        {
            radarItemType = RadarItemType.Custom;
            position = Vector3.zero;
            velocity = Vector3.zero;
            isVisibleToRadar = true;
            // Default factionId of 0 is everyone is on the same side (neutral)
            factionId = 0;
            squadronId = -1; // NOT SET
            guidHash = 0;
            shipControlModule = null;
            itemGameObject = null;
            blipSize = 1;
            itemSequenceNumber = nextSequenceNumber++;

            // if sequence number needs to be wrapped, do so to a high-ish number that is unlikely to be in use 
            if (nextSequenceNumber > uint.MaxValue - 100) { nextSequenceNumber = 100000; }
        }

        #endregion

    }
}