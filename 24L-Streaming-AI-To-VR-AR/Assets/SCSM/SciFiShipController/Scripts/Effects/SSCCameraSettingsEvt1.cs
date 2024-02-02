using UnityEngine;
using UnityEngine.Events;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This allows the storage of method calls with parameters in ShipWarpModule.cs.
    /// We need to derive it from UnityEvents so that the UnityEvent is serializable
    /// in the inspector. When parameters are used, UnityEvent is not serializable
    /// without the use of this class. We can have up to 4 parameters.
    /// </summary>
    [System.Serializable]
    public class SSCCameraSettingsEvt1 : UnityEvent<int, int, bool>
    {
        // onCameraSettingsChange
        // Param 1, Prev CammeraSettings index (1,2,3... OR 0 not set)
        // Param 2, New CammeraSettings index (1,2,3... OR 0 not set)
        // Param 3, future use
    }
}