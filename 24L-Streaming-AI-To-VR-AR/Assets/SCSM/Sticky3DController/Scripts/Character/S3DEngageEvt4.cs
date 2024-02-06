using UnityEngine;
using UnityEngine.Events;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This allows the storage of method calls with parameters in StickyControlModule.cs.
    /// We need to derive it from UnityEvents so that the UnityEvent is serializable
    /// in the inspector. When parameters are used, UnityEvent is not serializable
    /// without the use of this class. We can have up to 4 parameters.
    /// </summary>
    [System.Serializable]
    public class S3DEngageEvt4 : UnityEvent<int, int, bool, Vector3>
    {
        // onPreStartAim, onPostStartAim, onPreStopAim, onPostStopAim
        // onPreStartHoldWeapon, onPostStopHoldWeapon
        // Param 1, StickyID
        // Param 2, weapon stickyInteractiveID
        // Param 3, FUTURE
        // Param 4, FUTURE
    }
}