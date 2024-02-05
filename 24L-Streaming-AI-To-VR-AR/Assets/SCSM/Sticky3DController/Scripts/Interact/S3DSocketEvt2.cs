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
    public class S3DSocketEvt2 : UnityEvent<int, int, int>
    {
        // FUTURE
        // Param 1, SocketID
        // Param 2, StickyID
        // Param 3, StickyInteractiveID
        // Param 4
    }
}