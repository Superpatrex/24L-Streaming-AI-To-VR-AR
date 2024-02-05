using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Sample ship spawner that leverages the in-build ShipSpawner class. In your project,
    /// you'll want to include "using SciFiShipController" namespace and write your own code.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Ship Spawner 1")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleShipSpawner1 : MonoBehaviour
    {
        #region Public variables

        [Header("Player")]
        public GameObject playerShip = null;

        [Header("Prefabs")]
        public ShipControlModule npcShipPrefab = null;

        #endregion

        #region Private variables

        #endregion

        #region Initialisation Methods

        void Awake()
        {
            // In this example we already have the player setup in the scene. You could instantiate it from a prefab.
            // We also use a camera and the ShipCameraModule for the player ship. You don't need to use our ShipCameraModule.
            if (playerShip == null) { Debug.LogWarning("Please supply a player from the scene"); }

            if (npcShipPrefab == null) { Debug.LogWarning("Please supply a Ship prefab for the non-player ships"); }
            else
            {
                Squadron squadron1 = new Squadron();
                if (squadron1 != null)
                {
                    squadron1.squadronName = "Blue Squadron";
                    squadron1.squadronId = 1;

                    // The squadron will be created at the location of the gameobject you place in the scene
                    squadron1.anchorPosition = this.transform.position;

                    // What types of ships are we spawning?
                    squadron1.shipPrefab = npcShipPrefab.gameObject;

                    // Optionally place the player from the scene at the head of the squadron
                    squadron1.playerShip = playerShip;

                    // Place the ships in a classic Vic (Vee) formation
                    squadron1.tacticalFormation = Squadron.TacticalFormation.Vic;

                    // Configure the formation layout
                    squadron1.rowsX = 1;    // Not used for Vic formations
                    squadron1.rowsZ = 5;
                    squadron1.rowsY = 2;    // 2 stacks of ships (one above the other)

                    // How far apart are the ships?
                    squadron1.offsetX = 50f;
                    squadron1.offsetZ = 50f;
                    squadron1.offsetY = 125f;

                    // Set the forwards direction of the squadron based on the rotation of the
                    // rotation of the gameobject it is attached to.
                    squadron1.fwdDirection = transform.rotation * Vector3.forward;

                    ShipSpawner shipSpawner = new ShipSpawner();
                    if (shipSpawner != null)
                    {
                        GameObject squadronGameObject = new GameObject(squadron1.squadronName);
                        if (squadronGameObject != null)
                        {
                            squadronGameObject.transform.SetParent(gameObject.transform);

                            // If you have your own AI ship component, it can be automatically added to the NPC ship prefab when it is spawned
                            bool addAIScriptIfMissing = false;
                            System.Type aiShipComponent = null;

                            //aiShipComponent = typeof(myAIShipClass);

                            if (shipSpawner.CreateSquadron(squadron1, squadronGameObject.transform, ShipDestroyCallBack, addAIScriptIfMissing ? aiShipComponent : null))
                            {
                                int numShips = squadron1.shipList == null ? 0 : squadron1.shipList.Count;

                                Debug.Log("[DEBUG] Number of ships added to " + squadron1.squadronName + ": " + numShips);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Private Member Methods

        private void ShipDestroyCallBack(Ship ship)
        {
            Debug.Log("Ship " + ship.shipId + " was destroyed in squadron " + ship.squadronId);
        }

        #endregion

    }
}