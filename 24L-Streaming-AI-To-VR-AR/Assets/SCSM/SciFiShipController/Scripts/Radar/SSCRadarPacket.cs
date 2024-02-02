using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This should always be a DATA-ONLY class.
    /// Used with the radar system to send RadarItem data to the centralised system.
    /// Although a subset of the SSCRadarItem, it was decided not to make this a base
    /// class for SSCRadarItem.
    /// </summary>
    public class SSCRadarPacket
    {
        #region Public variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// World space position in the scene of this item
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public Vector3 position;

        /// <summary>
        /// The current velocity of the item being tracked
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public Vector3 velocity;

        /// <summary>
        /// Is this item currently visible to radar queries?
        /// For example, when a ship has been destroyed or is respawning, this should
        /// be set to false.
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public bool isVisibleToRadar;

        /// <summary>
        /// The faction or alliance the item belongs to. This can be used to identify if an item is friend or foe.
        /// 0 = Neutral
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public int factionId;

        /// <summary>
        /// The squadron this item is a member of. Typically only applies to Ships
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public int squadronId;

        #endregion

        #region Class Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public SSCRadarPacket()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Create a packet to transmit based on the current data in a SSCRadarItem
        /// e.g. from a ship's data.
        /// </summary>
        /// <param name="sscRadarItem"></param>
        public SSCRadarPacket(SSCRadarItem sscRadarItem)
        {
            if (sscRadarItem == null) { SetClassDefaults(); }
            else
            {
                position = sscRadarItem.position;
                velocity = sscRadarItem.velocity;
                isVisibleToRadar = sscRadarItem.isVisibleToRadar;
                factionId = sscRadarItem.factionId;
                squadronId = sscRadarItem.squadronId;
            }
        }

        #endregion

        #region Private Methods
        private void SetClassDefaults()
        {
            position = Vector3.zero;
            velocity = Vector3.zero;
            isVisibleToRadar = true;
            factionId = 0; // Neutral
            squadronId = -1; // NOT SET
        }
        #endregion
    }
}