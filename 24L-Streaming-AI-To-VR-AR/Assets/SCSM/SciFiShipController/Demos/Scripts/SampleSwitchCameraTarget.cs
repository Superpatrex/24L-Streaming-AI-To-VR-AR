using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This sample is not meant to be added directly to your game. Instead, it is an example
    /// of what you might write for your specific needs. This code is subject to change with
    /// each release of Sci-Fi Ship Controller.
    /// This sample uses the ShipCameraModule and allows the player to switch between the
    /// initial camera setup in your scene, and other target ship (Key: T).
    /// It can also switch to the next squadron (key: Y).
    /// It uses the Legacy Unity Input System for to check for the T and Y keys.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Switch Camera Target")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleSwitchCameraTarget : MonoBehaviour
    {

        #region Public Variables
        public ShipCameraModule shipCameraModule;
        public DemoControlModule demoControlModule;

        // This is the squadron which contains the alternate camera target
        // Valid squadron IDs are 0 or greater.
        public int squadronId = -1;

        #endregion

        #region Private Variables
        private ShipControlModule originalShipControlModule;
        private Vector3 originalCameraTargetOffset;
        private int shipIdx = 0;

        #endregion

        #region Initialisation Methods

        // Use this for initialization
        void Awake()
        {
            if (shipCameraModule == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("SampleSwitchCameraTarget - a ShipCameraModule has not be assigned from the scene");
                #endif
            }
            else if (demoControlModule == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("SampleSwitchCameraTarget - a DemoControlModule has not be assigned from the scene");
                #endif
            }
            else
            {
                originalShipControlModule = shipCameraModule.GetTarget();
                originalCameraTargetOffset = shipCameraModule.targetOffset;
            }

            #if !ENABLE_LEGACY_INPUT_MANAGER && UNITY_2019_2_OR_NEWER
            Debug.LogWarning("ERROR: SampleSwitchCameraTarget - This sample uses keyboard input from Legacy Input System which is NOT enabled in this project.");
            // Keep compiler happy
            if (shipIdx == 0) { }
            #endif
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            if (shipCameraModule != null && demoControlModule != null)
            {
                #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER
                // Check for Y - switch squadrons
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    squadronId = demoControlModule.GetNextSquadronWithShips(squadronId);
                    shipIdx = 0;
                    // If we got a valid squadron, get the first valid ship in that squadron
                    if (squadronId >= 0)
                    {
                        ShipControlModule shipControlModule = demoControlModule.GetNextShip(squadronId, shipIdx);
                        if (shipControlModule != null)
                        {
                            // When switching squadrons, get the camera offset from the squadron
                            Squadron squadron = demoControlModule.squadronList.Find(sq => sq.squadronId == squadronId);
                            if (squadron != null) { shipCameraModule.targetOffset = squadron.cameraTargetOffset; }

                            SetNewTarget(shipControlModule);
                            shipIdx++;
                        }
                    }
                }

                // Check for T - switch ship
                if (Input.GetKeyDown(KeyCode.T))
                {
                    ShipControlModule shipControlModule = demoControlModule.GetNextShip(squadronId, shipIdx);
                    if (shipControlModule != null)
                    {
                        SetNewTarget(shipControlModule);
                        shipIdx++;
                    }
                }
                #endif

                // Auto re-assign original if current is destroyed
                if (shipCameraModule.target == null)
                {
                    // The original ship (probably a player ship) may not be in the same squadron as the
                    // last target. This ensures that the next time user switches ships, it will be using the
                    // correct squadron and the camera target offset will be correct.
                    if (originalShipControlModule != null && originalShipControlModule.shipInstance != null)
                    {
                        squadronId = originalShipControlModule.shipInstance.squadronId;
                    }
                    shipCameraModule.targetOffset = originalCameraTargetOffset;
                    SetNewTarget(originalShipControlModule);
                    shipIdx = 0;
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Set a new target for the camera
        /// </summary>
        /// <param name="shipControlModule"></param>
        private void SetNewTarget(ShipControlModule shipControlModule)
        {
            shipCameraModule.SetTarget(shipControlModule);
            if (shipCameraModule.target != null)
            {
                shipCameraModule.ReinitialiseTargetVariables();
            }
        }

        #endregion
    }
}