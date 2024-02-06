using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// A struct used to uniquely identify a SSCRadarItem
    /// </summary>
    /// [Unity.VisualScripting.Inspectable]
    public struct SSCRadarItemKey
    {
        #region Public variables
        /// [Unity.VisualScripting.Inspectable]
        public int radarItemIndex;
        /// [Unity.VisualScripting.Inspectable]
        public uint radarItemSequenceNumber;
        #endregion
    }
}
