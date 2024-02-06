using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This simple sample script shows how to override input that gets sent from the
    /// ShipAIInputModule to the ShipControlModule of an NPC ship.
    /// This script overrides the primary fire mechanism. This script will control
    /// when the primary weapons on the AI ship, rather than using something like
    /// the AutoTargetingModule.
    /// SETUP:
    /// 1. Attach a SampleInputAIOverride component to an AI (NPC) ship.
    /// 2. Tick InitialiseOnStart or call Initialise() from your game code.
    /// WARNING
    /// This is only sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own
    /// namespace. It is subject to change at any time and be overwritten by
    /// SSC updates.
    /// </summary>
    public class SampleInputAIOverride : MonoBehaviour
    {
        #region Public Variables
        public bool initialiseOnStart = false;
        #endregion

        #region Private Variables
        private ShipAIInputModule shipAIInputModule = null;
        private ShipControlModule shipControlModule = null;
        private bool isInitialised = false;
        private ShipInput shipInput = null;
        #endregion

        #region Initialisation Methods

        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determine if the AI Ship should attempt to fire all primary
        /// weapons.
        /// </summary>
        /// <returns></returns>
        private bool ShouldAttemptToFireNow()
        {
            // Fire the primary weapon(s) as often as they are able to

            // Add your firing logic here.

            return true;
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            if (isInitialised)
            {
                // We only need to update the primaryFire as all other
                // input is being handled by the ShipAIInputModule.
                shipInput.primaryFire = ShouldAttemptToFireNow();

                shipControlModule.SendInput(shipInput);
            }
        }

        #endregion

        #region Public Methods

        public void Initialise()
        {
            shipAIInputModule = GetComponent<ShipAIInputModule>();

            if (shipAIInputModule == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] SampleInputAIOverride.Initialise() could not find the AI module. Did you attach this script to a ShipAIInputModule?");
                #endif
            }
            else
            {
                // If the AI hasn't been initialised, do it now
                if (!shipAIInputModule.IsInitialised) { shipAIInputModule.Initialise(); }

                if (shipAIInputModule.IsInitialised)
                {
                    shipControlModule = shipAIInputModule.GetShipControlModule;

                    if (shipControlModule != null)
                    {
                        // Override the primary fire input on the AI Module
                        shipAIInputModule.isPrimaryFireDataDiscarded = true;
                        // Re-initialise the DiscardData shipInput settings in the ShipAIInputModule
                        // This is required after one or more of the [axis]DataDiscarded values are changed at runtime.
                        shipAIInputModule.ReinitialiseDiscardData();

                        // Create an instance of ShipInput which we can send to the ship.
                        shipInput = new ShipInput();

                        // Start by disabling everything. This helps to future-proof the code.
                        // We'll be telling the Ship you can discard anything else that we don't enable below.
                        shipInput.DisableAllData();

                        // When we send data, we will tell the ship we'll be sending Primary Fire data only.
                        shipInput.isPrimaryFireDataEnabled = true;

                        Debug.Log("SampleInputAIOverride initialised");

                        isInitialised = true;
                    }
                }
            }          
        }

        #endregion
    }
}