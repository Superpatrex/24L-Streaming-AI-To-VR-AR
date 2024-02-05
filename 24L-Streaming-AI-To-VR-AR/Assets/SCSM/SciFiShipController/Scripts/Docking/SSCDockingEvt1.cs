using UnityEngine;
using UnityEngine.Events;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This allows the storage of method calls with parameters in ShipDockingStation.cs and ShipDocking.cs
    /// Param 1 - ShipDockingStationID
    /// Param 2 - shipId
    /// Param 3 - Docking Station Docking Point Number (1 - n)
    /// Param 4 - FUTURE Expansion
    /// We need to derive it from UnityEvents so that the UnityEvent is serializable
    /// in the inspector. When parameters are used, UnityEvent is not serializable
    /// without the use of this class. We can have up to 4 parameters.
    /// </summary>
    [System.Serializable]
    public class SSCDockingEvt1 : UnityEvent<int, int, int, Vector3>
    {
        // ShipDockingStation onPreUndock, onPostDocked
        // ShipDocking onPostDockingHover, onPostUndockingHover
        // Param 1 - ShipDockingStationID
        // Param 2 - shipId
        // Param 3 - Docking Station - Docking Point Number (1 - n)
        // Param 4 - FUTURE Expansion
    }
}