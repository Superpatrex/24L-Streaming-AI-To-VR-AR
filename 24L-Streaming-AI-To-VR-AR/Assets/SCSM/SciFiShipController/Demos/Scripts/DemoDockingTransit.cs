using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Demo script that uses your pre-configured Docking Station for an AI Ship to undock,
    /// fly along a path, then dock with another Docking Station.
    /// This is designed to work with TechDemo4scene2 but a similar script could be adapted
    /// to work with your scenario.
    /// 1. Add this component to a gameobject in the scene
    /// 2. Add two Docking Stations to your scene
    /// 3. Configure a Ship Docking Point on each Ship Docking Station
    /// 4. Drag each of the Ship Docking Stations from the scene into the slots provided on this component
    /// 5. Add a NPC ship with ShipDocking and ShipAIInputModules attached
    /// 6. On one of the Docking Stations, drag the NPC ship into the Ship Docking Station Point (Assign Ship)
    /// 7. In the scene, ensure there is a SSCManager.
    /// 8. Create a path between station 1 and station 2 (leave a gap between the start/end of the path and the stations)
    /// 9. Click (I)nsert button on path to create a duplicate of the first path and rename it
    /// 10. Reverse the direction of the new path using the reverse (<->) button
    /// 11. Add the names of the paths to this component in the inspector
    /// 12. On the ShipDocking component (on the NPC ship), set the Initial Docking State to Docked.
    /// 13. On the ShipDocking component (on the NPC ship), optionally set the Auto Undock Time.
    /// WARNING: This is a DEMO script and is subject to change without notice during
    /// upgrades. This is just to show you how to do things in your own code and namespace.
    /// </summary>
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class DemoDockingTransit : MonoBehaviour
    {
        #region Public Variables

        public bool initialiseOnStart = false;

        public ShipDockingStation shipDockingStation1 = null;
        public ShipDockingStation shipDockingStation2 = null;

        public string departTransitPathName = "Depart Transit Path";
        public string returnTransitPathName = "Return Transit Path";

        public ShipAIInputModule.AIObstacleAvoidanceQuality obsAvoidWhenDocking = ShipAIInputModule.AIObstacleAvoidanceQuality.Off;
        public ShipAIInputModule.AIObstacleAvoidanceQuality obsAvoidWhenNoDocking = ShipAIInputModule.AIObstacleAvoidanceQuality.Medium;

        #endregion

        #region Public Properties
        public bool IsInitialised { get { return isInitialised; } }
        #endregion

        #region Public Static Variables

        #endregion

        #region Private Variables - General

        private bool isInitialised = false;
        private SSCManager sscManager = null;
        private PathData departTransitPath = null;
        private PathData returnTransitPath = null;
        private List<ShipAIInputModule> aiShips = null;

        #endregion

        #region Public Delegates

        #endregion

        #region Private Initialise Methods

        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Update Methods

        #endregion

        // Create additional regions for specific Private method groups
        #region Private and Internal Methods - General

        private void ConfigureAssignedShips (ShipDockingStation shipDockingStation)
        {
            int numDockingPoints = shipDockingStation.NumberOfDockingPoints;

            for (int dpIdx = 0; dpIdx < numDockingPoints; dpIdx++)
            {
                ShipControlModule shipControlModule = shipDockingStation.GetAssignedShip(dpIdx);
                if (shipControlModule != null)
                {
                    // Tell the AI ship to take action when it has finished following a path
                    ShipAIInputModule shipAIInputModule = shipControlModule.GetShipAIInputModule(true);
                    if (shipAIInputModule != null && shipAIInputModule.IsInitialised)
                    {
                        shipAIInputModule.callbackCompletedStateAction = CompletedStateActionCallback;

                        // Remember this ship
                        aiShips.Add(shipAIInputModule);
                    }

                    // The ship should already know about it's docking point component, as it was discovered
                    // when the ship was assigned to the docking point in ShipDockingStation.Initialise().
                    ShipDocking shipDocking = shipControlModule.GetShipDocking(false);

                    // Get notified when a ship changes it's docking state (Docking, Docked, Undocking, Undocked) 
                    if (shipDocking != null)
                    {
                        shipDocking.callbackOnStateChange = OnDockingStateChanged;
                    }
                }
            }
        }


        #endregion

        #region Events

        #endregion

        #region Call back notifications

        /// <summary>
        /// Take action when a ship finished it's current action
        /// </summary>
        /// <param name="shipAIInputModule"></param>
        public void CompletedStateActionCallback (ShipAIInputModule shipAIInputModule)
        {
            if (shipAIInputModule != null)
            {
                // Ww are only interested in knowing when the ship is not docking, undocking or docked.
                // i.e. Has just finished undocking OR has reached the end of the depart or return path.
                ShipControlModule shipControlModule = shipAIInputModule.GetShipControlModule;
                if (shipControlModule.ShipIsNotDocked())
                {
                    PathData pathToFollow = shipAIInputModule.GetTargetPath();

                    // Was the ship following the depart path?
                    if (pathToFollow != null && pathToFollow.Equals(departTransitPath))
                    {
                        shipDockingStation2.AssignShipToDockingPoint(shipControlModule, shipControlModule.GetShipDocking(false));
                        shipDockingStation2.DockShip(shipControlModule);

                    }
                    // Was the ship following the return path?
                    else if (pathToFollow != null && pathToFollow.Equals(returnTransitPath))
                    {
                        shipDockingStation1.AssignShipToDockingPoint(shipControlModule, shipControlModule.GetShipDocking(false));
                        shipDockingStation1.DockShip(shipControlModule);
                    }
                    else
                    {
                        // Which station was it assigned to?
                        if (shipDockingStation1.IsShipAssigned(shipControlModule.GetShipId))
                        {
                            pathToFollow = departTransitPath;
                            shipDockingStation1.UnassignShip(shipControlModule.GetShipId);
                        }
                        else
                        {
                            // Assume assigned to second station
                            pathToFollow = returnTransitPath;
                            shipDockingStation2.UnassignShip(shipControlModule.GetShipId);
                        }

                        // Tell the ship to follow the appropriate path
                        shipAIInputModule.AssignTargetPath(pathToFollow);
                        shipAIInputModule.SetState(AIState.moveToStateID);
                    }

                }
            }
        }

        /// <summary>
        /// Gets called automatically when a ship's docking state changes.
        /// (Docking, Docked, Undocking, Undocked)
        /// </summary>
        /// <param name="shipDocking"></param>
        /// <param name="shipControlModule"></param>
        /// <param name="shipAIInputModule"></param>
        /// <param name="previousDockingState"></param>
        public void OnDockingStateChanged (ShipDocking shipDocking, ShipControlModule shipControlModule, ShipAIInputModule shipAIInputModule, ShipDocking.DockingState previousDockingState)
        {
            int dockingStateInt = shipDocking.GetStateInt();

            // Set the obstacle avoidance quality based on the docking state.
            if (shipAIInputModule != null)
            {
                if (dockingStateInt == ShipDocking.notDockedInt)
                {
                    shipAIInputModule.obstacleAvoidanceQuality = obsAvoidWhenNoDocking;
                }
                else
                {
                    shipAIInputModule.obstacleAvoidanceQuality = obsAvoidWhenDocking;
                }
            }
        }

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// If the ship is waiting (docked) at a station, attempt to get it
        /// to travel to the other station.
        /// </summary>
        /// <param name="dockingPointNumber"></param>
        public void DepartStation (ShipControlModule shipControlModule)
        {
            // Is ship docked?
            if (isInitialised && shipControlModule != null && shipControlModule.ShipIsDocked())
            {
                // Now determine if it is docked with one of our stations
                int dockingPointIdx = shipDockingStation1.GetDockingPointIndex(shipControlModule.GetShipId);

                if (dockingPointIdx != ShipDockingStation.NoDockingPoint)
                {
                    // Ship is assigned to Station 1.
                    DepartStation1(dockingPointIdx+1);
                }
                else
                {
                    // Ship is not docked with Station 1, so check station 2.
                    dockingPointIdx = shipDockingStation2.GetDockingPointIndex(shipControlModule.GetShipId);

                    // Is ship assigned to Station 2?
                    if (dockingPointIdx != ShipDockingStation.NoDockingPoint)
                    {
                        // Ship is assigned to Station 2.
                        DepartStation2(dockingPointIdx+1);
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to get the ship (if any) to undock and begin travelling to Station 2.
        /// Assume the ship is an AI or AI-assisted ship.
        /// </summary>
        /// <param name="dockingPointNumber"></param>
        public void DepartStation1 (int dockingPointNumber)
        {
            if (isInitialised)
            {
                shipDockingStation1.UnDockShip(dockingPointNumber - 1);
            } 
        }

        /// <summary>
        /// Attempt to get the ship (if any) to undock and begin travelling to Station 1.
        /// Assume the ship is an AI or AI-assisted ship.
        /// </summary>
        /// <param name="dockingPointNumber"></param>
        public void DepartStation2 (int dockingPointNumber)
        {
            if (isInitialised)
            {
                shipDockingStation2.UnDockShip(dockingPointNumber - 1);
            }
        }

        /// <summary>
        /// Initialise this demo component.
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            // Get or create a new manager in the current scene
            sscManager = SSCManager.GetOrCreateManager(gameObject.scene.handle);

            departTransitPath = sscManager.GetPath(departTransitPathName);
            returnTransitPath = sscManager.GetPath(returnTransitPathName);

            aiShips = new List<ShipAIInputModule>(5);

            if (shipDockingStation1 == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("DemoDockingTransit - could not find ShipDockingStation1 - did you add it as a reference from the scene in the slot provided?");
                #endif
            }
            else if (shipDockingStation2 == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("DemoDockingTransit - could not find ShipDockingStation2 - did you add it as a reference from the scene in the slot provided?");
                #endif
            }
            else if (!shipDockingStation1.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: DemoDockingTransit " + shipDockingStation1.name + " should have Initialise On Awake enabled for this demo");
                #endif
            }
            else if (!shipDockingStation2.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: DemoDockingTransit " + shipDockingStation2.name + " should have Initialise On Awake enabled for this demo");
                #endif
            }
            else if (departTransitPath == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("DemoDockingTransit - could not find departTransitPath (" + departTransitPathName + ")" );
                #endif
            }
            else if (returnTransitPath == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("DemoDockingTransit - could not find returnTransitPath (" + returnTransitPathName + ")" );
                #endif
            }
            else
            {
                ConfigureAssignedShips(shipDockingStation1);
                ConfigureAssignedShips(shipDockingStation2);

                isInitialised = true;
            }
        }


        #endregion

    }
}