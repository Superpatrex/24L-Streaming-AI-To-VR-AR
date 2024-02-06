using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This sample shows how you can:
    /// 1. Create HUD gauges at runtime
    /// 2. Show ship health on a HUD gauge
    /// 3. Show ship shield value on a HUD gauge
    /// 4. Shows how to get a weapon by its name
    /// 5. Show beam weapon charge on a HUD gauge
    /// 6. Show how to display health of local damage region on a HUD gauge
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
    /// 7. Configure a beam weapon on the player ship
    /// 8. Optionally configure one or two local damage regions on the ship Combat tab.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Show Ship Metrics")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleShowShipMetrics : MonoBehaviour
    {
        #region Public Variables and Properties

        [Tooltip("Add the HUD from the scene")]
        public ShipDisplayModule shipDisplayModule;

        [Tooltip("Add the SSCUIFilled sprite from the Textures/HUD folder")]
        public Sprite healthGaugeSprite;

        [Tooltip("Add the SSCUIFilled sprite from the Textures/HUD folder")]
        public Sprite shieldGaugeSprite;

        [Tooltip("The name of the beam weapon from the player ship - see Combat tab")]
        public string beamWeaponName;

        [Tooltip("(Optional) show health of a local damage region e.g. Engines")]
        public string localDamageRegionName1;

        [Tooltip("(Optional) show health of a local damage region e.g. Cockpit")]
        public string localDamageRegionName2;

        [Header("Shared Health Colours")]
        public Color lowColour = Color.red;
        public Color mediumColour = new Color(1f, 0.5f, 0f, 1f);
        public Color highColour = Color.green;

        #endregion

        #region Private Variables
        private bool isInitialised = false;
        private bool isWeaponSetup = false;
        private ShipControlModule playerShip = null;
        private DisplayGauge healthGauge = null;
        private DisplayGauge shieldGauge = null;
        private DisplayGauge weaponPowerGauge = null;
        private DisplayGauge damageRegion1Gauge = null;
        private DisplayGauge damageRegion2Gauge = null;
        private Weapon beamWeapon = null;
        private DamageRegion localDamageRegion1 = null;
        private DamageRegion localDamageRegion2 = null;
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            #region Initialise Player Ship
            playerShip = GetComponent<ShipControlModule>();

            bool isShipInitialised = false;

            if (playerShip != null)
            {
                // If Initialise On Awake is not ticking on the ship, initialise the ship now.
                if (!playerShip.IsInitialised) { playerShip.InitialiseShip(); }

                // Ship must be initialised before accessing the shipInstance.
                if (playerShip.IsInitialised)
                {
                    isShipInitialised = true;

                    // Get the beam weapon
                    int wpIdx = playerShip.shipInstance.GetWeaponIndexByName(beamWeaponName);
                    beamWeapon = playerShip.shipInstance.GetWeaponByIndex(wpIdx);

                    if (beamWeapon == null)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("[ERROR] SampleShowShipMetrics.Start() could not find the weapon by the name supplied: " + (string.IsNullOrEmpty(beamWeaponName) ? "(weapon name not given)" : beamWeaponName));
                        #endif
                    }
                    else if (beamWeapon.weaponTypeInt != Weapon.FixedBeamInt)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("[ERROR] SampleShowShipMetrics.Start() the weapon (" + beamWeaponName + " is not a FixedBeam weapon.");
                        #endif
                        beamWeapon = null;
                    }
                    else
                    {
                        isWeaponSetup = true;
                    }
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("[ERROR] SampleShowShipMetrics.Start() could not find the ship! Did you attach this script to you Player ship (ShipControlModule)?");
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
                    if (healthGaugeSprite == null)
                    {
                        Debug.LogWarning("[ERROR] SampleShowShipMetrics.Start() the healthGaugeSprite is not set. Add SSCUIFilled from the Textures/HUD folder.");
                    }
                    #endif

                    healthGauge = shipDisplayModule.AddGauge("Health", "Health");
                    if (healthGauge != null)
                    {
                        // Set some of the Health gauge attributes
                        shipDisplayModule.SetDisplayGaugeForegroundSprite(healthGauge,healthGaugeSprite);
                        shipDisplayModule.SetDisplayGaugeSize(healthGauge, 0.1f, 0.03f);
                        shipDisplayModule.SetDisplayGaugeOffset(healthGauge, -0.89f, 0.75f);
                        shipDisplayModule.SetDisplayGaugeValueAffectsColourOn(healthGauge, lowColour, mediumColour, highColour);
                        shipDisplayModule.ShowDisplayGauge(healthGauge);

                        if (shieldGaugeSprite != null)
                        {
                            shieldGauge = shipDisplayModule.CopyDisplayGauge(healthGauge, "Shield");
                            shieldGauge.gaugeString = "Shields";
                            shipDisplayModule.AddGauge(shieldGauge);
                            shipDisplayModule.SetDisplayGaugeOffset(shieldGauge, -0.89f, 0.65f);
                            shipDisplayModule.ShowDisplayGauge(shieldGauge);
                        }

                        if (isWeaponSetup)
                        {
                            // Copy the Health gauge and update some attributes for the weapon power gauge
                            weaponPowerGauge = shipDisplayModule.CopyDisplayGauge(healthGauge, "weaponPower");
                            weaponPowerGauge.gaugeString = "Charge";
                            shipDisplayModule.AddGauge(weaponPowerGauge);
                            shipDisplayModule.SetDisplayGaugeOffset(weaponPowerGauge, -0.89f, shieldGauge == null ? 0.65f : 0.55f);
                            shipDisplayModule.SetDisplayGaugeForegroundColour(weaponPowerGauge, Color.red);
                            shipDisplayModule.ShowDisplayGauge(weaponPowerGauge);
                        }

                        if (isShipInitialised)
                        {
                            // If there is a matching local damage region, create a health gauge for it
                            if (!string.IsNullOrEmpty(localDamageRegionName1))
                            {
                                int drIdx = playerShip.shipInstance.GetDamageRegionIndexByName(localDamageRegionName1);
                                localDamageRegion1 = playerShip.shipInstance.GetDamageRegionByIndex(drIdx);

                                damageRegion1Gauge = shipDisplayModule.AddGauge("DM1", localDamageRegionName1);
                                if (damageRegion1Gauge != null)
                                {
                                    shipDisplayModule.SetDisplayGaugeForegroundSprite(damageRegion1Gauge, healthGaugeSprite);
                                    shipDisplayModule.SetDisplayGaugeSize(damageRegion1Gauge, 0.1f, 0.03f);
                                    shipDisplayModule.SetDisplayGaugeOffset(damageRegion1Gauge, -0.89f, shieldGauge == null ? 0.55f : 0.45f);
                                    shipDisplayModule.SetDisplayGaugeValueAffectsColourOn(damageRegion1Gauge, lowColour, mediumColour, highColour);
                                    shipDisplayModule.ShowDisplayGauge(damageRegion1Gauge);
                                }
                            }

                            // If there is a matching local damage region, create a health gauge for it
                            if (!string.IsNullOrEmpty(localDamageRegionName2))
                            {
                                int drIdx = playerShip.shipInstance.GetDamageRegionIndexByName(localDamageRegionName2);
                                localDamageRegion2 = playerShip.shipInstance.GetDamageRegionByIndex(drIdx);

                                damageRegion2Gauge = shipDisplayModule.AddGauge("DM2", localDamageRegionName2);
                                if (damageRegion2Gauge != null)
                                {
                                    shipDisplayModule.SetDisplayGaugeForegroundSprite(damageRegion2Gauge, healthGaugeSprite);
                                    shipDisplayModule.SetDisplayGaugeSize(damageRegion2Gauge, 0.1f, 0.03f);
                                    shipDisplayModule.SetDisplayGaugeOffset(damageRegion2Gauge, -0.89f, shieldGauge == null ? 0.45f : 0.35f);
                                    shipDisplayModule.SetDisplayGaugeValueAffectsColourOn(damageRegion2Gauge, lowColour, mediumColour, highColour);
                                    shipDisplayModule.ShowDisplayGauge(damageRegion2Gauge);
                                }
                            }
                        }

                        isHUDSetup = true;
                    }
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("[ERROR] SampleShowShipMetrics.Start() the ShipDisplayModule is not set. Add the Prefabs/Visuals/HUD1 prefab to the scene, then drag it on to this script which should be attached to your player ship.");
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
            if (isInitialised)
            {
                // Update Health on the HUD
                shipDisplayModule.SetDisplayGaugeValue(healthGauge, playerShip.shipInstance.HealthNormalised);

                if (shieldGauge != null) { shipDisplayModule.SetDisplayGaugeValue(shieldGauge, playerShip.shipInstance.mainDamageRegion.ShieldNormalised); }

                // Update Weapon Charge on HUD
                if (isWeaponSetup) { shipDisplayModule.SetDisplayGaugeValue(weaponPowerGauge, beamWeapon.chargeAmount); }

                // Update the health of the damage regions on the HUD
                if (damageRegion1Gauge != null && localDamageRegion1 != null)
                {
                    shipDisplayModule.SetDisplayGaugeValue(damageRegion1Gauge, localDamageRegion1.HealthNormalised);
                }

                // Update the health of the damage regions on the HUD
                if (damageRegion2Gauge != null && localDamageRegion2 != null)
                {
                    shipDisplayModule.SetDisplayGaugeValue(damageRegion2Gauge, localDamageRegion2.HealthNormalised);
                }
            }
        }

        #endregion
    }
}