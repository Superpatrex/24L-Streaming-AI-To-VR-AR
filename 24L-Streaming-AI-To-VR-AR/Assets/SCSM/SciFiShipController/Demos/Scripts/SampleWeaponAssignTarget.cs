using UnityEngine;
using SciFiShipController;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipControllerSample
{
    /// <summary>
    /// Sample script to assign targets to turret weapons on a ship at runtime.
    /// This is only a code segment to demonstrate how API calls could be used in
    /// your own code. Place it on an empty gameobject in the scene to see how
    /// it works.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Weapon Assign Target")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleWeaponAssignTarget : MonoBehaviour
    {
        #region Public variables
        public bool initialiseOnAwake = true;

        [Header("Player Ship Configuration")]
        public ShipControlModule playerShip = null;
        public string[] NameOfTurrets = { };

        [Header("Enemy Ship Configuration")]
        public ShipControlModule enemyShip = null;

        #endregion

        #region Private variables
        private int[] turretIndexArray;

        #endregion

        #region Initialisation Methods
        void Awake()
        {
            if (initialiseOnAwake) { Initialise(); }
        }

        public void Initialise()
        {
            if (playerShip == null) { Debug.LogWarning("Please specify a player ship from the scene."); }
            else if (enemyShip == null) { Debug.LogWarning("Please specify an enemy ship from the scene."); }
            else if (NameOfTurrets == null || NameOfTurrets.Length < 1)
            {
                Debug.LogWarning("Please provide at least 1 weapon turret name from the player ship which will target the enemy ship");
            }
            else
            {
                int numTurrets = NameOfTurrets == null ? 0 : NameOfTurrets.Length;

                turretIndexArray = new int[numTurrets];

                // As we are only assigning a target to a weapon, we don't need to wait for the ship to be initialised.
                if (turretIndexArray != null)
                {
                    // As a best-practice pre-fetch the weapon indexes to reduce garbage collection in the future
                    for (int tNameIdx = 0; tNameIdx < numTurrets; tNameIdx++)
                    {
                        // Ignore turrets without a name both for input and within the list of weapons on a ship
                        if (string.IsNullOrEmpty(NameOfTurrets[tNameIdx])) { turretIndexArray[tNameIdx] = -1; }
                        else { turretIndexArray[tNameIdx] = playerShip.shipInstance.GetWeaponIndexByName(NameOfTurrets[tNameIdx]); }

                        // Assign the initial target. Later in your code you can also call this method to assign other targets
                        playerShip.shipInstance.SetWeaponTarget(turretIndexArray[tNameIdx], enemyShip.gameObject);
                    }
                }
            }
        }

        #endregion

    }
}