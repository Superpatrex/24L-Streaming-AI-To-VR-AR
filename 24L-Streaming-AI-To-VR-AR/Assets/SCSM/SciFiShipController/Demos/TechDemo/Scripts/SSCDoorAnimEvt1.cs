using UnityEngine;
using UnityEngine.Events;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// This allows the storage of method calls with parameters in SSCDoorAnimator.cs.
    /// We need to derive it from UnityEvents so that the UnityEvent is serializable
    /// in the inspector. When parameters are used, UnityEvent is not serializable
    /// without the use of this class. We can have up to 4 parameters.
    /// </summary>
    [System.Serializable]
    public class SSCDoorAnimEvt1 : UnityEvent<int, int, Vector3>
    {
        // T0 int - Unity SSCDoorAnimator script object InstanceID
        // T1 int - zero-based index of the (first) door being acted upon
        // T2 vector3 - FUTURE use
    }
}