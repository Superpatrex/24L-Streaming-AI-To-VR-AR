using UnityEngine;
using UnityEngine.Events;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This allows the storage of method calls with parameters in StickyControlModule.cs.
    /// We need to derive it from UnityEvents so that the UnityEvent is serializable
    /// in the inspector. When parameters are used, UnityEvent is not serializable
    /// without the use of this class. We can have up to 4 parameters.
    /// </summary>
    [System.Serializable]
    public class S3DReactEvt1 : UnityEvent<int, int, int, int>
    {
        // onReactPreEnter, onReactPreExit, onReactPostExit
        // Param 1, StickyID
        // Param 2, ReactToStickyID
        // Param 3, FriendOrFoe (Friend = 1, Neurtral = 0, Foe = -1)
        // Param 4, OtherModelId
    }
}