using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Executes a simple state machine for determining state information for a "chase" AI.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Chase AI")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    [RequireComponent(typeof(ShipAIInputModule))]
    public class SampleChase : MonoBehaviour
    {
        #region Public Variables

        /// <summary>
        /// The ship this AI ship should chase.
        /// </summary>
        public ShipControlModule targetShip;

        #endregion

        #region Private Variables

        private ShipAIInputModule shipAIInputModule;

        #endregion

        #region Initialise Methods

        // Use this for initialization
        void Awake()
        {
            // Get a reference to the ship AI input module attached to this gameobject
            shipAIInputModule = GetComponent<ShipAIInputModule>();
            // Initialise the ship AI (if it hasn't been initialised already)
            shipAIInputModule.Initialise();

            // Initialise the AI in the "dogfight" state
            shipAIInputModule.SetState(AIState.dogfightStateID);
            // Initialise the ship AI (if it hasn't been initialised already)
            targetShip.InitialiseShip();
            // Set the target ship to the provided target ship
            shipAIInputModule.AssignTargetShip(targetShip);
        }

        #endregion
    }
}
