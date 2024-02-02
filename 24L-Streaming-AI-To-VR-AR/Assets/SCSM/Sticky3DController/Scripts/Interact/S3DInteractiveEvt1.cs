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
    public class S3DInteractiveEvt1 : UnityEvent<Vector3, Vector3, int, int>
    {
        // onEnterHover, onGrabbed, onPostGrabbed, onTouched
        // Param 1, Vector3 hit point
        // Param 2, Vector3 hit normal
        // Param 3, StickyInteractiveID
        // Param 4, StickyID
    }
}

