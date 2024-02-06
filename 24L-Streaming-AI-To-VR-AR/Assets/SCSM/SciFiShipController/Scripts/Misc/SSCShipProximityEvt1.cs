using UnityEngine;
using UnityEngine.Events;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This allows the storage of method calls with parameters in SSCProxity.cs.
    /// We need to derive it from UnityEvents so that the UnityEvent is serializable
    /// in the inspector. When parameters are used, UnityEvent is not serializable
    /// without the use of this class. We can have up to 4 parameters.
    /// </summary>
    [System.Serializable]
    public class SSCShipProximityEvt1 : UnityEvent<int, int, int, int>
    {
        // onEnterMethods, onExitMethods
        // Param 1, ShipId
        // Param 2, FactionId
        // Param 3, SquadronId
        // Param 4, InstanceId of the SSCShipProximity component in the scene
    }
}