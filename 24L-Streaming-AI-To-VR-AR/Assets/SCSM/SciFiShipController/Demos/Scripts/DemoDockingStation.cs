using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Demo script that uses your pre-configured Docking Station to undock, then re-dock
    /// AI Ships.
    /// WARNING: This is a DEMO script and is subject to change without notice during
    /// upgrades. This is just to show you how to do things in your own code.
    /// It assumes:
    /// 1. There is a least 1 AI ship configured and assigned to a docking point
    /// 2. There is a least 1 Exit Path configured
    /// If and Entry Path is configured the AI Ship(s) will attempt to undock,
    /// fly along the exit path, then fly back along the entry path and
    /// re-dock with the Ship Docking Station.
    /// </summary>
    public class DemoDockingStation : MonoBehaviour
    {
        #region Private Variables
        private ShipDockingStation shipDockingStation = null;
        #endregion

        #region Initialisation Methods

        // Start is called before the first frame update
        void Start()
        {
            if (!TryGetComponent(out shipDockingStation))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("DemoDockingStation - could not find ShipDockingStation component - this script needs to be attached to your ShipDockingStation");
                #endif
            }
            else
            {
                // if the ship docking station hasn't been initialised on wake, do it now
                if (!shipDockingStation.IsInitialised) { shipDockingStation.Initialise(true); }

                int numDockingPoints = shipDockingStation.NumberOfDockingPoints;

                for (int dpIdx = 0; dpIdx < numDockingPoints; dpIdx++)
                {
                    ShipControlModule shipControlModule = shipDockingStation.GetAssignedShip(dpIdx);
                    if (shipControlModule != null)
                    {
                        // Tell the AI ship to take action when it has finished following the exit path
                        ShipAIInputModule shipAIInputModule = shipControlModule.GetShipAIInputModule(true);
                        if (shipAIInputModule != null && shipAIInputModule.IsInitialised)
                        {
                            shipAIInputModule.callbackCompletedStateAction = CompletedStateActionCallback;
                        }
                    }
                }

                // Now that things are initialised, attempt to undock any ships
                Invoke("UndockShips", 3f);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Undock all the AI docked ships at this Ship Docking Station
        /// </summary>
        private void UndockShips()
        {
            int numDockingPoints = shipDockingStation.NumberOfDockingPoints;

            for (int dpIdx = 0; dpIdx < numDockingPoints; dpIdx++)
            {
                // Check if this is a AI or AI-assisted ship
                ShipControlModule shipControlModule = shipDockingStation.GetAssignedShip(dpIdx);
                // We have already checked for an AI component in Start(), so now we just need to check if there is one attached (forceCheck can be false).
                if (shipControlModule != null && shipControlModule.GetShipAIInputModule(false) != null)
                {
                    shipDockingStation.UnDockShip(dpIdx);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Take action when a ship finished it's current action
        /// </summary>
        /// <param name="shipAIInputModule"></param>
        public void CompletedStateActionCallback(ShipAIInputModule shipAIInputModule)
        {
            //Debug.Log("[DEBUG] DEMO finished action for " + shipAIInputModule.name + " T: " + Time.time);

            if (shipAIInputModule != null)
            {
                // Only dock the ship if it has just become undocked at the end of exit path
                ShipControlModule shipControlModule = shipAIInputModule.GetShipControlModule;
                if (shipControlModule != null && shipControlModule.ShipIsNotDocked())
                {
                    shipDockingStation.DockShip(shipAIInputModule.GetShipControlModule);
                }                
            }
        }

        #endregion
    }
}
