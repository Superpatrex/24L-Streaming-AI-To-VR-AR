using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [RequireComponent(typeof(ShipControlModule))]
    public class TechDemo2PlayerHUD : MonoBehaviour
    {
        #region Public Variables

        public ShipDisplayModule shipDisplayModule = null;

        #endregion

        #region Private Variables

        private ShipControlModule shipControlModule;
        private Weapon missileWeaponLeft = null;
        private Weapon missileWeaponRight = null;

        private DisplayGauge healthGauge = null;
        private DisplayGauge missileGauge = null;
        private bool isInitialised = false;

        #endregion

        #region Initialisation

        // Start is called before the first frame update
        void Start()
        {
            // Get a reference to the ship control module
            shipControlModule = GetComponent<ShipControlModule>();
            // Get references to the left and right weapons
            missileWeaponLeft = shipControlModule.shipInstance.GetWeaponByIndex(2);
            missileWeaponRight = shipControlModule.shipInstance.GetWeaponByIndex(3);

            // Get references to the gauges
            if (shipDisplayModule != null)
            {
                //// If the ship display module is not initialised, initialise it
                //if (!shipDisplayModule.IsInitialised) { shipDisplayModule.Initialise(); }
                // Get references to the gauges
                healthGauge = shipDisplayModule.GetDisplayGauge("Health Gauge");
                missileGauge = shipDisplayModule.GetDisplayGauge("Missile Gauge");
                isInitialised = true;
            }
        }

        #endregion

        #region Update

        // Update is called once per frame
        void Update()
        {
            // Check if the HUD is initialised and visible.
            if (isInitialised && shipDisplayModule.IsHUDShown)
            {
                if (healthGauge != null)
                {
                    // Set the value of the health gauge.
                    // On the HUD, the health gauge has "Value Affects Colour" enabled so the colour of the health
                    // bar gauge will automatically adjust based on the 0.0-1.0 value of the ship health.
                    shipDisplayModule.SetDisplayGaugeValue(healthGauge, shipControlModule.shipInstance.HealthNormalised);
                }

                if (missileGauge != null && missileWeaponLeft != null && missileWeaponRight != null)
                {
                    // Get the current reload charge of the missile weapons
                    float currentReloadCharge = 0f;
                    if (missileWeaponLeft.Health > 0f)
                    {
                        currentReloadCharge = 1f - (missileWeaponLeft.reloadTimer / missileWeaponLeft.reloadTime);
                    }
                    else if (missileWeaponRight.Health > 0f)
                    {
                        currentReloadCharge = 1f - (missileWeaponRight.reloadTimer / missileWeaponRight.reloadTime);
                    }
                    // Set the value of the missile gauge
                    shipDisplayModule.SetDisplayGaugeValue(missileGauge, currentReloadCharge);
                }
            }
        }

        #endregion
    }
}
