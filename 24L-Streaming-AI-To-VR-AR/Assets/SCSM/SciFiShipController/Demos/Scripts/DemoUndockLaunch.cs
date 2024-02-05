using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Demo script used in Docking Demo scene to launch the player ship with the undocking catapult.
    /// WARNING: This is a DEMO script and is subject to change without notice during
    /// upgrades. This is just to show you how to do things in your own code.
    /// 1. The script should be attached to the player ship.
    /// 2. The LaunchShip() needs to be called from a new PlayerCustomInput (using the L button)
    /// </summary>
    public class DemoUndockLaunch : MonoBehaviour
    {
        #region Public variables
        public ShipDisplayModule shipDisplayModule = null;

        #endregion

        #region Private variables
        private ShipControlModule playerShip = null;
        private ShipDocking shipDocking = null;
        private PlayerInputModule playerInputModule = null;
        private bool isInitialised = false;
        private DisplayMessage countdownDisplayMessage = null;
        private DisplayMessage launchDisplayMessage = null;
        private int countDown;
        private string countDownMethodName = "CountDown";
        private CustomPlayerInput customPlayerInput = null;

        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            playerShip = GetComponent<ShipControlModule>();
            playerInputModule = GetComponent<PlayerInputModule>();

            if (playerShip == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("DemoUndockLaunch - could not find the player ship component - this script needs to be attached to your ShipControlModule for your player ship");
                #endif
            }
            else if (playerInputModule == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("DemoUndockLaunch - could not find the PlayerInputModule component - this component needs to be attached to your ShipControlModule for your player ship");
                #endif
            }
            else if (shipDisplayModule == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("DemoUndockLaunch - could not find the HUD - please add the ShipDisplayModule from the scene to the slot provided");
                #endif
            }
            else
            {
                shipDocking = playerShip.GetShipDocking(true);

                if (shipDocking == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("DemoUndockLaunch - could not find the ShipDocking component - this script needs to be attached to your ShipControlModule for your player ship");
                    #endif
                }
                else if (shipDocking.GetStateInt() != ShipDocking.dockedInt)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("DemoUndockLaunch - on the player ship, the ShipDocking Initial Docking State must be set to Docked, or else this demo will not work...");
                    #endif
                }
                else
                {
                    // Setup the messages
                    countDown = 3;

                    // This is in case the DemoUndockLaunch script runs before ShipDisplayModule.Start() event.
                    if (!shipDisplayModule.IsInitialised) { shipDisplayModule.Initialise(); }

                    countdownDisplayMessage = shipDisplayModule.AddMessage("Countdown Msg", countDown.ToString());

                    // Add message a bottom of screen to tell user what to do
                    launchDisplayMessage = shipDisplayModule.AddMessage("Launch Msg", "Press L to launch when ready");
                    if (launchDisplayMessage != null)
                    {
                        shipDisplayModule.SetDisplayMessageOffset(launchDisplayMessage, 0f, -0.9f);
                        shipDisplayModule.SetDisplayMessageBackgroundColour(launchDisplayMessage, new Color(0f, 0f, 0.5f, 0.3f));
                        shipDisplayModule.ShowDisplayMessageBackground(launchDisplayMessage);
                        shipDisplayModule.ShowDisplayMessage(launchDisplayMessage);
                    }

                    // In code, add the input for the button press (assume Direct Keyboard)
                    customPlayerInput = new CustomPlayerInput();

                    if (customPlayerInput != null)
                    {
                        customPlayerInput.inputAxisMode = PlayerInputModule.InputAxisMode.SingleAxis;
                        customPlayerInput.isButton = true;
                        customPlayerInput.isButtonEnabled = true;

                        // Here we use Direct Keyboard but could also setup Unity Input System, Oculus, VIVE, Rewired etc.
                        customPlayerInput.dkmPositiveKeycode = KeyCode.L;

                        // Tell SSC to call our method when the button is pressed.
                        customPlayerInput.customPlayerInputEvt = new CustomPlayerInputEvt();
                        customPlayerInput.customPlayerInputEvt.AddListener(delegate { LaunchShip(); });

                        // Add the custom player input to the list in the PlayerInputModule
                        playerInputModule.customPlayerInputList.Add(customPlayerInput);

                        // We have modified the Custom Player Inputs, so we need to reinitilise them.
                        playerInputModule.ReinitialiseCustomPlayerInput();
                    }

                    isInitialised = true;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Cleanup
        /// </summary>
        private void OnDestroy()
        {
            // Remove any listeners we added
            if (customPlayerInput != null && customPlayerInput.customPlayerInputEvt != null)
            {
                customPlayerInput.customPlayerInputEvt.RemoveAllListeners();
            }
        }

        #endregion

        #region Private Methods

        private void CountDown()
        {
            countDown--;

            if (countDown > 0)
            {
                shipDisplayModule.SetDisplayMessageText(countdownDisplayMessage, countDown.ToString());
            }
            else
            {
                // Hey, we're ready to launch!
                shipDisplayModule.HideDisplayMessage(countdownDisplayMessage);

                // If we want to leave the hangar we should also undock from the docking point
                // so other ships can use it, otherwise we could just set the state to NotDocked.
                // e.g. shipDocking.SetState(ShipDocking.DockingState.NotDocked);
                if (shipDocking.shipDockingStation != null)
                {
                    // Use the docking point in the hangar the player is docked with
                    if (shipDocking.DockingPointId >= 0)
                    {
                        shipDocking.shipDockingStation.UnDockShip(shipDocking.DockingPointId);
                        shipDocking.shipDockingStation.UnassignDockingPoint(shipDocking.DockingPointId);
                    }
                }
            } 
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Count down and then launch the ship using the catapult
        /// </summary>
        public void LaunchShip()
        {
            // If we've already started to countdown, avoid calling it again
            if (isInitialised && countDown > 2)
            {
                shipDisplayModule.HideDisplayMessage(launchDisplayMessage);
                shipDisplayModule.ShowDisplayMessage(countdownDisplayMessage);

                // Count down with HUD messages
                Invoke(countDownMethodName, 1f);
                Invoke(countDownMethodName, 2f);
                Invoke(countDownMethodName, 3f);

                // Undock in CountDown() when countDown reaches 0.
            }
        }

        #endregion
    }
}
