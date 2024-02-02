using UnityEngine;
using UnityEngine.Events;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This allows the storage of method calls with parameters in StickyInteractive.cs.
    /// We need to derive it from UnityEvents so that the UnityEvent is serializable
    /// in the inspector. When parameters are used, UnityEvent is not serializable
    /// without the use of this class. We can have up to 4 parameters.
    /// </summary>
    [System.Serializable]
    public class S3DInteractiveEvt5 : UnityEvent<int, Vector3, Vector3, Vector3>
    {
        // onReadableValueChanged
        // Param 1, StickyInteractiveID
        // Param 2, Current Value: x: left - right -1.0 to 1.0, y: FUTURE, z: back - forward -1.0 to 1.0
        // Param 3, Previous Value: x: left - right -1.0 to 1.0, y: FUTURE, z: back - forward -1.0 to 1.0 
        // Param 4, FUTURE 
    }
}
