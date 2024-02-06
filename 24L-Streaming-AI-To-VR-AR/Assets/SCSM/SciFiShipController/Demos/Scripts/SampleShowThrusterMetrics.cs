using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Sample script to show how thruster metrics can be added to a Ship Display Module (HUD) at runtime.
    /// This sample shows how you can:
    /// 1. Create HUD gauges at runtime
    /// 2. Show ship fuel level for the central fuel level or the first thruster on a HUD gauge
    /// 3. Show ship heat level for the first thruster on a HUD gauge
    /// 4. Show an alert message when the first thruster is overheating.
    /// WARNING: This is only sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own
    /// namespace.
    /// SETUP:
    /// 1. Add Prefabs\Visuals\HUD1 prefab to the scene
    /// 2. Add player ship from Prefabs\Ships to the scene (ones that don't have NPC in the name)
    /// 3. Remove or disable the default Main Camera
    /// 4. Add Prefabs\Environment\PlayerCamera prefab to the scene
    /// 5. Hook up the player ship to the PlayerCamera
    /// 6. Add this script to the player ship
    /// 7. Hook up the HUD1 prefab to the slot on this component
    /// 8. Hook up the SSCUIFilled sprite from the Textures/HUD folder to the slot on this component
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Show Thruster Metrics")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleShowThrusterMetrics : MonoBehaviour
    {

        #region Public Variables and Properties

        [Tooltip("Add the HUD from the scene")]
        public ShipDisplayModule shipDisplayModule;

        [Tooltip("Add the SSCUIFilled sprite from the Textures/HUD folder")]
        public Sprite levelGaugeSprite;

        public bool updateMetrics = true;

        [Header("Shared Level Colours")]
        public Color lowColour = Color.green;
        public Color mediumColour = new Color(1f, 0.5f, 0f, 1f);
        public Color highColour = Color.red;

        #endregion

        #region Private Variables
        private bool isInitialised = false;
        private ShipControlModule playerShipControlModule = null;
        private Ship playerShipInstance = null;
        private DisplayMessage thrusterCriticalMsg = null;
        private DisplayGauge fuelGauge = null;
        private DisplayGauge heatGauge = null;
        
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            #region Initialise Player Ship
            playerShipControlModule = GetComponent<ShipControlModule>();

            bool isShipInitialised = false;

            if (playerShipControlModule != null)
            {
                // If Initialise On Awake is not ticking on the ship, initialise the ship now.
                if (!playerShipControlModule.IsInitialised) { playerShipControlModule.InitialiseShip(); }

                // Ship must be initialised before accessing the shipInstance.
                if (playerShipControlModule.IsInitialised)
                {
                    playerShipInstance = playerShipControlModule.shipInstance;
                    isShipInitialised = true;
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("[ERROR] SampleShowThrusterMetrics.Start() could not find the ship! Did you attach this script to you Player ship (ShipControlModule)?");
            }
            #endif
            #endregion

            #region Initialise HUD
            bool isHUDSetup = false;
            if (shipDisplayModule != null)
            {
                // If Initialise on Start is not ticked on the HUD, do it now.
                if (!shipDisplayModule.IsInitialised) { shipDisplayModule.Initialise(); }

                if (shipDisplayModule.IsInitialised)
                {
                    #if UNITY_EDITOR
                    if (levelGaugeSprite == null)
                    {
                        Debug.LogWarning("[ERROR] SampleShowThrusterMetrics.Start() the levelGaugeSprite is not set. Add SSCUIFilled from the Textures/HUD folder.");
                    }
                    #endif

                    heatGauge = shipDisplayModule.AddGauge("HeatLevel", "Temp");
                    if (heatGauge != null)
                    {
                        // Set some of the Health gauge attributes
                        shipDisplayModule.SetDisplayGaugeForegroundSprite(heatGauge, levelGaugeSprite);
                        shipDisplayModule.SetDisplayGaugeSize(heatGauge, 0.1f, 0.025f);
                        shipDisplayModule.SetDisplayGaugeOffset(heatGauge, -0.55f, 0.82f);
                        
                        if (isShipInitialised)
                        {
                            // Use the overheating value of the thruster to set the medium colour value point on the gauge.
                            shipDisplayModule.SetDisplayGaugeMediumColourValue(heatGauge, playerShipInstance.GetOverheatingThreshold(1) / 100f);

                            // Get the current heat level of our first thruster.
                            shipDisplayModule.SetDisplayGaugeValue(heatGauge, playerShipInstance.GetHeatLevel(1) / 100f);
                        }

                        shipDisplayModule.SetDisplayGaugeValueAffectsColourOn(heatGauge, lowColour, mediumColour, highColour);
                        shipDisplayModule.ShowDisplayGauge(heatGauge);

                        // Create a fuel gauge using a copy of the heat gauge created above
                        fuelGauge = shipDisplayModule.CopyDisplayGauge(heatGauge, "FuelLevel");
                        if (fuelGauge != null)
                        {
                            fuelGauge.gaugeString = "Fuel";
                            shipDisplayModule.AddGauge(fuelGauge);
                            shipDisplayModule.SetDisplayGaugeOffset(fuelGauge, 0.55f, 0.82f);
                            // Reverse the gauge colours as we want critical to be when fuel is low.
                            shipDisplayModule.SetDisplayGaugeValueAffectsColourOn(fuelGauge, highColour, mediumColour, lowColour);
                            shipDisplayModule.SetDisplayGaugeMediumColourValue(fuelGauge, 0.25f);
                            shipDisplayModule.ShowDisplayGauge(fuelGauge);
                        }

                        // Create a critical warning message for the thruster overheating
                        thrusterCriticalMsg = shipDisplayModule.AddMessage("CriticalMessage", "ALERT: Engines Critical");

                        if (thrusterCriticalMsg != null)
                        {
                            shipDisplayModule.SetDisplayMessageOffset(thrusterCriticalMsg, 0f, 0.93f);
                            shipDisplayModule.SetDisplayMessageSize(thrusterCriticalMsg, 1f, 0.03f);
                            shipDisplayModule.SetDisplayMessageTextColour(thrusterCriticalMsg, Color.red);
                        }

                        isHUDSetup = fuelGauge != null && thrusterCriticalMsg != null;
                    }
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("[ERROR] SampleShowThrusterMetrics.Start() the ShipDisplayModule is not set. Add the Prefabs/Visuals/HUD1 prefab to the scene, then drag it on to this script which should be attached to your player ship.");
            }
            #endif
            #endregion

            isInitialised = isHUDSetup && isShipInitialised;
        }

        #endregion

        #region Update Methods
        // Update is called once per frame
        void Update()
        {
            if (isInitialised && updateMetrics && shipDisplayModule.IsHUDShown)
            {
                // Update Thruster temperature on the HUD
                shipDisplayModule.SetDisplayGaugeValue(heatGauge, playerShipInstance.GetHeatLevel(1) / 100f);

                // Update fuel level
                // Check to see if "Central Fuel" is enabled on the Thruster tab
                if (playerShipInstance.useCentralFuel)
                {
                    // Use the Central fuel level for the whole ship
                    shipDisplayModule.SetDisplayGaugeValue(fuelGauge, playerShipInstance.GetFuelLevel() / 100f);
                }
                else
                {
                    // Use the fuel level for the first thruster
                    shipDisplayModule.SetDisplayGaugeValue(fuelGauge, playerShipInstance.GetFuelLevel(1) / 100f);
                }

                // Check to see if we need to show or hide the overheating alert message.
                if (playerShipInstance.IsThrusterOverheating(1))
                {
                    if (!thrusterCriticalMsg.showMessage) { shipDisplayModule.ShowDisplayMessage(thrusterCriticalMsg); }
                }
                else
                {
                    if (thrusterCriticalMsg.showMessage) { shipDisplayModule.HideDisplayMessage(thrusterCriticalMsg); }
                }
            }
        }

        #endregion

        #region Public Methods

        public void StartMetrics()
        {
            updateMetrics = true;
        }

        public void StopMetrics()
        {
            updateMetrics = false;
        }

        #endregion
    }
}