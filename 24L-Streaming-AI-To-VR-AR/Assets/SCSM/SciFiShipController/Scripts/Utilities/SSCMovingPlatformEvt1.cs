using UnityEngine;
using UnityEngine.Events;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// This allows the storage of method calls with parameters in SSCMovingPlatform.cs.
    /// We need to derive it from UnityEvents so that the UnityEvent is serializable
    /// in the inspector. When parameters are used, UnityEvent is not serializable
    /// without the use of this class. We can have up to 4 parameters.
    /// </summary>
    [System.Serializable]
    public class SSCMovingPlatformEvt1 : UnityEvent<bool, bool, int, Vector3>
    {
        // T0 bool - Is at start
        // T1 bool - Is at end
        // T2 int - Unity object InstanceID
        // T3 vector3 - FUTURE use
    }
}