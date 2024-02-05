using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Executes a simple state machine for determining state information for a "race" AI.
    /// This is a SAMPLE ONLY and may get modified in future releases. If you wish to use something
    /// similar in your own game create a new script in your own namespace to avoid it getting
    /// overwritten by Sci-Fi Ship Controller updates.
    /// If instantiated / added at runtime, also call Initialise().
    /// Paths are assigned per ship (as this script is attached to each AI Ship). However,
    /// they could be managed centrally and set via code.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Race AI")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    [RequireComponent(typeof(ShipAIInputModule))]
    public class SampleRace : MonoBehaviour
    {
        #region Public Variables

        /// <summary>
        /// If enabled, the Initialise() will be called as soon as Awake() runs. This should be disabled if you are
        /// instantiating the SampleRace through code.
        /// </summary>
        public bool initialiseOnAwake = false;

        /// <summary>
        /// Array of track path names.
        /// </summary>
        public string[] trackPathNames = { "Path 1 Name Here", "Path 2 Name Here" };

        /// <summary>
        /// Has the SampleRace been initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// Get the index of the Path in the 
        /// </summary>
        //public int CurrentRacePathIndex { get { return currentRacePathIndex; } }

        #endregion

        #region Private Variables

        private ShipAIInputModule shipAIInputModule;
        private SSCManager sscManager;
        private List<PathData> racePathsList;
        private int numPaths = 0;
        private int currentRacePathIndex = 0;
        private bool isInitialised = false;
        #endregion

        #region Initialise Methods

        private void Awake()
        {
            if (initialiseOnAwake) { Initialise(); }
        }

        /// <summary>
        /// If shipAIInputModule is not initialised, SampleRace will initialise it.
        /// </summary>
        public void Initialise()
        {
            // Don't attempt to re-initialise multiple times.
            if (isInitialised) { return; }

            // Get a reference to the ship AI input module attached to this gameobject
            shipAIInputModule = GetComponent<ShipAIInputModule>();
            // Initialise the ship AI (if it hasn't been initialised already)
            shipAIInputModule.Initialise();

            // Get a reference to the Ship Controller Manager instance
            sscManager = SSCManager.GetOrCreateManager(gameObject.scene.handle);

            if (sscManager != null)
            {
                // Initialise the list of race paths
                racePathsList = new List<PathData>();
                // Find the paths and add them to the list
                PathData racePath;
                for (int i = 0; i < trackPathNames.Length; i++)
                {
                    racePath = sscManager.GetPath(trackPathNames[i]);
                    if (racePath != null) { racePathsList.Add(racePath); }
                    #if UNITY_EDITOR
                    else { Debug.Log("Path not found: " + trackPathNames[i]); }
                    #endif
                }

                // cache the number of paths so we don't need to keep counting the list
                numPaths = racePathsList == null ? 0 : racePathsList.Count;

                // If it exists, assign the ship to follow the first Path
                AssignPath(0);

                isInitialised = true;
            }
        }

        #endregion

        #region Public Member Methods

        /// <summary>
        /// Set the ship to follow the 0-based Path included in trackPathNames.
        /// If no Path are found or the index is out of range, the Ship will be
        /// placed in the idle state.
        /// </summary>
        /// <param name="pathIndex"></param>
        public void AssignPath(int pathIndex)
        {
            // Assign the first race path
            if (pathIndex >= 0 && pathIndex < racePathsList.Count)
            {
                // Initialise the AI in the "move to" state
                shipAIInputModule.SetState(AIState.moveToStateID);

                currentRacePathIndex = pathIndex;
                shipAIInputModule.AssignTargetPath(racePathsList[currentRacePathIndex]);

                // Assign the has completed state action callback (this will then be called every time the
                // ship gets to the end of a path)
                shipAIInputModule.callbackCompletedStateAction = FinishedPathCallback;
            }
            else
            {
                shipAIInputModule.SetState(AIState.idleStateID);
                shipAIInputModule.callbackCompletedStateAction = null;
            }
        }

        /// <summary>
        /// Gets the distance from the start of the list of Paths.
        /// </summary>
        /// <returns></returns>
        public float GetDistanceFromStart()
        {
            float distanceFromStart = 0f;

            if (isInitialised && numPaths > 0)
            {
                // Add the paths already completed.
                // start at the second path (if there is one)
                for (int pIdx = 1; pIdx < numPaths; pIdx++)
                {
                    // Add the distance for the previous path
                    distanceFromStart += racePathsList[pIdx-1].splineTotalDistance;
                    // Exit the loop once we're up to the current path
                    if (currentRacePathIndex >= pIdx) { break; }
                }

                // Get the distance from the start of the current path
                PathData pathData = racePathsList[currentRacePathIndex];
                if (pathData != null)
                {
                    // Add the distance to the previous location on the path
                    int prevLocationIdx = shipAIInputModule.GetPreviousTargetPathLocationIndex();

                    if (prevLocationIdx >= 0)
                    {
                        PathLocationData prevPathLocationData = pathData.pathLocationDataList[prevLocationIdx];
                        if (prevPathLocationData != null)
                        {
                            // In a closed circuit the first Location contains the total distance for the Path.
                            if (prevLocationIdx > 0) { distanceFromStart += prevPathLocationData.distanceCumulative; }

                            // This is the location on the path the ship is heading towards
                            int nextLocationIdx = shipAIInputModule.GetCurrentTargetPathLocationIndex();

                            // Calc distance from the previous location
                            float tValue = shipAIInputModule.GetCurrentTargetPathTValue();

                            float deltaDistance = SSCManager.GetPathDistance(pathData, prevLocationIdx, nextLocationIdx) * tValue;
                            distanceFromStart += deltaDistance;
                        }
                    } 
                }
            }

            return distanceFromStart;
        }

        #endregion

        #region Callback Methods

        /// <summary>
        /// Function to be called when the ship has completed the current path.
        /// </summary>
        public void FinishedPathCallback (ShipAIInputModule shipAIInputModule)
        {
            // Increment the race path index
            currentRacePathIndex++;
            // If we have finished all of the paths loop back to the first one
            if (currentRacePathIndex >= racePathsList.Count) { currentRacePathIndex = 0; }
            // Get the next path and assign it to the ship AI
            shipAIInputModule.AssignTargetPath(racePathsList[currentRacePathIndex]);
        }

        #endregion
    }
}
