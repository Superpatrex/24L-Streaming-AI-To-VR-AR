using UnityEngine;
using UnityEngine.Events;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This allows the storage of method calls with parameters in CustomPlayerInput.cs.
    /// We need to derive it from UnityEvents so that the UnityEvent is serializable
    /// in the inspector. When parameters are used, UnityEvent is not serializable
    /// without the use of this class. We can have up to 4 parameters.
    /// </summary>
    [System.Serializable]
    public class CustomPlayerInputEvt : UnityEvent<Vector3, int>
    {
        // T0 float is the value
        // T1 int CustomPlayerInput.CustomPlayerInputEventType
    }
}