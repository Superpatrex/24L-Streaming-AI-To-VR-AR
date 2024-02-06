using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class used for storing AI Data on a ship to be used by the state method.
    /// </summary>
    public class DemoFlyToLocationShipData : MonoBehaviour
    {
        public AIBehaviourInput.AIBehaviourType currentBehaviourType = AIBehaviourInput.AIBehaviourType.CustomSeekArrival;
    }
}
