using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Executes a simple state machine for determining state information for an "evade" AI.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Evade AI")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    [RequireComponent(typeof(ShipAIInputModule))]
    public class SampleEvade : MonoBehaviour
    {
        #region Public Variables

        /// <summary>
        /// The name of the path this AI ship should attempt to follow.
        /// </summary>
        public string pathName = "Path Name Here";

        /// <summary>
        /// The list of ships this AI ship should evade.
        /// </summary>
        public List<ShipControlModule> shipsToEvade = new List<ShipControlModule>();

        #endregion

        #region Private Variables

        private ShipAIInputModule shipAIInputModule;
        private SSCManager sscManager;
        private PathData flightPath;
        private List<Ship> shipsToEvadeAIList;

        #endregion

        #region Initialise Methods

        // Use this for initialization
        void Awake()
        {
            // Get a reference to the ship AI input module attached to this gameobject
            shipAIInputModule = GetComponent<ShipAIInputModule>();
            // Initialise the ship AI (if it hasn't been initialised already)
            shipAIInputModule.Initialise();

            // Get a reference to the Ship Controller Manager instance
            sscManager = SSCManager.GetOrCreateManager(gameObject.scene.handle);
            // Find the Path
            flightPath = sscManager.GetPath(pathName);

            #if UNITY_EDITOR
            if (flightPath == null) { Debug.Log("Path not found: " + pathName); }
            #endif

            // Initialise the AI in the "move to" state
            shipAIInputModule.SetState(AIState.moveToStateID);
            // Set the target path to the flight path
            shipAIInputModule.AssignTargetPath(flightPath);
            shipAIInputModule.SetCurrentTargetPathLocationIndex(1, 0f);
            // Set the list of ships to evade
            shipsToEvadeAIList = new List<Ship>();
            for (int i = 0; i < shipsToEvade.Count; i++)
            {
                if (shipsToEvade[i] != null) { shipsToEvadeAIList.Add(shipsToEvade[i].shipInstance); }
            }
            shipAIInputModule.AssignShipsToEvade(shipsToEvadeAIList);
        }

        #endregion
    }
}
