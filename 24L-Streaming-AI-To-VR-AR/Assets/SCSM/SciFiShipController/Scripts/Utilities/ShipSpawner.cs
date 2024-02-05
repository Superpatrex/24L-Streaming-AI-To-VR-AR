using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// We "could" make the ShipSpawner behave much like the SSCManager or even
    /// integrate it into the SSCManager. It could be converted over pretty easily...
    /// </summary>
    public class ShipSpawner
    {
        #region Enumerations

        #endregion

        #region Public variables


        #endregion

        #region Private variables

        #endregion

        #region Public Methods

        /// <summary>
        /// Create a squadon of ships.
        /// If notification when a ship is about to be destroyed is required, pass in the name
        /// of the custom method to call, else set callbackOnDestroy to null.
        /// Optionally add an AI script component to each ship
        /// </summary>
        /// <param name="squadron"></param>
        /// <param name="parentTrfm"></param>
        /// <param name="callbackOnDestroy"></param>
        /// <param name="aiComponentType"></param>
        /// <returns></returns>
        public bool CreateSquadron(Squadron squadron, Transform parentTrfm, ShipControlModule.CallbackOnDestroy callbackOnDestroy, System.Type aiComponentType = null)
        {
            bool isSuccesful = false;

            // Use the rules within the squadron to instantiate the ships in the scene.
            if (squadron != null)
            {
                // Verify the ship prefab
                if (squadron.shipPrefab != null && squadron.shipPrefab.GetComponent<ShipControlModule>() != null)
                {
                    Vector3 upDirection = Vector3.up;
                    List<Vector3> shipPositions = new List<Vector3>(50);

                    #region Build list of ship positions

                    Vector3 shipPosition = Vector3.zero;

                    // The default location for the player in the squadron.
                    int playerIdx = 0;

                    if (squadron.tacticalFormation == Squadron.TacticalFormation.Vic)
                    {
                        // The V-formation is at 45 degrees if x and z offsets are the same.
                        // Only the number of rows on z and y axis are considered

                        // The bottom "layer" of ships starts at the y position of the squadron
                        float currentY = squadron.anchorPosition.y;
                        float currentX = squadron.anchorPosition.x;

                        // Loop through all the (stacked) x,z lines on the y-axis
                        for (int y = 0; y < squadron.rowsY; y++)
                        {
                            // The first row of ships starts at the anchor position
                            float currentZ = squadron.anchorPosition.z;

                            // Loop through all the lines of ships on the z-axis
                            for (int z = 0; z < squadron.rowsZ; z++)
                            {
                                shipPosition.y = currentY;
                                shipPosition.z = currentZ;

                                // The head of the v is always the anchor position of the formation + the y-offset
                                if (z == 0)
                                {
                                    shipPosition.x = currentX;
                                    shipPositions.Add(shipPosition);
                                }
                                else
                                {
                                    // Each ship is 45 degrees to the left and right behind the previous ship(s)

                                    // Right ship
                                    shipPosition.x = currentX + (z * squadron.offsetX);
                                    shipPositions.Add(shipPosition);

                                    // Left ship
                                    shipPosition.x = currentX + (z * -squadron.offsetX);
                                    shipPositions.Add(shipPosition);
                                }

                                currentZ -= squadron.offsetZ;
                            }

                            currentY += squadron.offsetY;
                        }
                    }
                    else if (squadron.tacticalFormation == Squadron.TacticalFormation.Wedge)
                    {
                        // Wedge is like a filled in Vic (vee) formation
                        // squadron.rowsX is not used for Wedge

                        // The bottom "layer" of ships starts at the y position of the squadron
                        float currentY = squadron.anchorPosition.y;
                        float currentX, currentZ;

                        // Loop through all the (stacked) x,z lines on the y-axis
                        for (int y = 0; y < squadron.rowsY; y++)
                        {
                            // The first row of ships starts at the anchor position
                            currentZ = squadron.anchorPosition.z;

                            // Loop through all the lines of ships on the z-axis
                            for (int z = 0; z < squadron.rowsZ; z++)
                            {
                                shipPosition.y = currentY;
                                shipPosition.z = currentZ;

                                // Each row along x-axis will have z ships

                                // If the zero-based row number is even it will have an odd number of ships
                                if (((z >> 1) << 1) == z)
                                {
                                    // Odd rows have a ship in the centre of the row.
                                    // Ships start on the left and are populated along the tow to the right
                                    currentX = squadron.anchorPosition.x - (z + 1) / 2 * squadron.offsetX;

                                    for (int x = 0; x <= z; x++)
                                    {
                                        shipPosition.x = currentX;
                                        shipPositions.Add(shipPosition);
                                        currentX += squadron.offsetX;
                                    }
                                }
                                else
                                {
                                    // Odd rows are offset half the offset x distance to the right and left of the centre
                                    currentX = squadron.anchorPosition.x - ((float)z / 2f * squadron.offsetX);

                                    for (int x = 0; x <= z; x++)
                                    {
                                        shipPosition.x = currentX;
                                        shipPositions.Add(shipPosition);
                                        currentX += squadron.offsetX;
                                    }
                                }

                                currentZ -= squadron.offsetZ;
                            }

                            currentY += squadron.offsetY;
                        }
                    }
                    else if (squadron.tacticalFormation == Squadron.TacticalFormation.LeftEchelon || squadron.tacticalFormation == Squadron.TacticalFormation.RightEchelon)
                    {
                        // Left or right offset
                        float offsetX = squadron.tacticalFormation == Squadron.TacticalFormation.LeftEchelon ? -squadron.offsetX : squadron.offsetX;

                        // The bottom "layer" of ships starts at the y position of the squadron
                        float currentY = squadron.anchorPosition.y;

                        // Loop through all the (stacked) x,z lines on the y-axis
                        for (int y = 0; y < squadron.rowsY; y++)
                        {
                            // The first row of ships starts at the anchor position
                            float currentZ = squadron.anchorPosition.z;
                            shipPosition.y = currentY;

                            // Loop through all the lines of ships on the z-axis
                            for (int z = 0; z < squadron.rowsZ; z++)
                            {
                                float currentX = squadron.anchorPosition.x;

                                // Lines are populated from left to right
                                for (int x = 0; x < squadron.rowsX; x++)
                                {
                                    // Each ship is angled to the left/right behind the previous ship.
                                    // If x and z offsets are the same the angle is 45 degrees
                                    shipPosition.x = currentX;
                                    //shipPosition.y = currentY;
                                    shipPosition.z = currentZ - (squadron.offsetX * x);
                                    shipPositions.Add(shipPosition);

                                    currentX += offsetX;
                                }

                                currentZ -= squadron.offsetZ;
                            }

                            currentY += squadron.offsetY;
                        }
                    }
                    else if (squadron.tacticalFormation == Squadron.TacticalFormation.Line)
                    {
                        playerIdx = Mathf.FloorToInt(squadron.rowsX / 2);
                        
                        float extentX = (squadron.rowsX - 1) * squadron.offsetX;
                        //float extentY = (squadron.rowsY - 1) * squadron.offsetY;
                        //float extentZ = (squadron.rowsZ - 1) * squadron.offsetZ;

                        // The bottom "layer" of ships starts at the y position of the squadron
                        float currentY = squadron.anchorPosition.y;

                        // Loop through all the (stacked) x,z lines on the y-axis
                        for (int y = 0; y < squadron.rowsY; y++)
                        {
                            // The first row of ships starts at the anchor position
                            float currentZ = squadron.anchorPosition.z;

                            // Loop through all the lines of ships on the z-axis
                            for (int z = 0; z < squadron.rowsZ; z++)
                            {
                                float currentX = squadron.anchorPosition.x - (extentX/2f);

                                // Lines are populated from left to right
                                for (int x = 0; x < squadron.rowsX; x++)
                                {
                                    shipPosition.x = currentX;
                                    shipPosition.y = currentY;
                                    shipPosition.z = currentZ;
                                    shipPositions.Add(shipPosition);

                                    currentX += squadron.offsetX;
                                }

                                currentZ -= squadron.offsetZ;
                            }

                            currentY += squadron.offsetY;
                        }
                    }
                    else if (squadron.tacticalFormation == Squadron.TacticalFormation.Column || squadron.tacticalFormation == Squadron.TacticalFormation.StaggeredColumn)
                    {
                        playerIdx = Mathf.FloorToInt(squadron.rowsX / 2);

                        // The anchor point for a column or staggered column is at the bottom left (a Line's anchor point is bottom centre)
                        bool isStaggered = squadron.tacticalFormation == Squadron.TacticalFormation.StaggeredColumn;

                        // The bottom "layer" of ships starts at the y position of the squadron
                        float currentY = squadron.anchorPosition.y;

                        // Loop through all the (stacked) x,z lines on the y-axis
                        for (int y = 0; y < squadron.rowsY; y++)
                        {
                            // The first row of ships starts at the anchor position
                            float currentZ = squadron.anchorPosition.z;

                            // Loop through all the lines of ships on the z-axis
                            for (int z = 0; z < squadron.rowsZ; z++)
                            {
                                // Unlike Line formations, columns start at the anchor point
                                float currentX = squadron.anchorPosition.x;

                                // For staggered formation, check if this row on z-axis is odd?
                                if (isStaggered && ((z >> 1) << 1) == z) { currentX += (squadron.offsetX * squadron.rowsX); }

                                // Lines are populated from left to right
                                for (int x = 0; x < squadron.rowsX; x++)
                                {
                                    shipPosition.x = currentX;
                                    shipPosition.y = currentY;
                                    shipPosition.z = currentZ;
                                    shipPositions.Add(shipPosition);

                                    currentX += squadron.offsetX;
                                }

                                currentZ -= squadron.offsetZ;
                            }

                            currentY += squadron.offsetY;
                        }

                    }
                    #endregion

                    #region Instantiate ships
                    int numShips = shipPositions == null ? 0 : shipPositions.Count;

                    // If a player ship is not being placed at the head of the squadron, reset its postion
                    // so a regular shipPrefab will be instantiated for all squadron members.
                    if (squadron.playerShip == null) { playerIdx = -1; }

                    // Calculate the rotation once outside the loop
                    Quaternion rotation = Quaternion.LookRotation(squadron.fwdDirection, upDirection);

                    for (int spIdx = 0; spIdx < numShips; spIdx++)
                    {
                        Vector3 rotatedPosition = (squadron.anchorPosition + (rotation * (shipPositions[spIdx] - squadron.anchorPosition)));

                        // Are we moving the player ship?
                        if (spIdx == playerIdx)
                        {
                            squadron.playerShip.transform.SetPositionAndRotation(rotatedPosition, rotation);
                        }
                        else
                        {
                            GameObject shipGameObjectInstance = Object.Instantiate(squadron.shipPrefab, rotatedPosition, rotation);

                            if (shipGameObjectInstance != null)
                            {
                                // Set the object's parent if one is supplied
                                if (parentTrfm != null) { shipGameObjectInstance.transform.SetParent(parentTrfm); }

                                // uncomment for testing in editor
                                //shipGameObjectInstance.name += spIdx;

                                // Add the ship to the squadron
                                squadron.shipList.Add(shipGameObjectInstance.transform.GetInstanceID());

                                // Initialise the ship
                                ShipControlModule shipControlModule = shipGameObjectInstance.GetComponent<ShipControlModule>();

                                if (shipControlModule != null)
                                {
                                    // Set the name of the custom callback method.
                                    shipControlModule.callbackOnDestroy = callbackOnDestroy;

                                    if (!shipControlModule.IsInitialised) { shipControlModule.InitialiseShip(); }

                                    // Update squadron-related fields in the ship
                                    if (shipControlModule.shipInstance != null)
                                    {
                                        shipControlModule.shipInstance.factionId = squadron.factionId;
                                        shipControlModule.shipInstance.squadronId = squadron.squadronId;
                                    }
                                }

                                // Optionally add an AI Script with default values
                                if (aiComponentType != null)
                                {
                                    if (shipGameObjectInstance.GetComponent(aiComponentType) == null) { shipGameObjectInstance.AddComponent(aiComponentType); }
                                }
                            }
                        }
                    }

                    isSuccesful = true;
                    #endregion

                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ERROR: " + squadron.squadronName + " squadron does not have a valid ship prefab. The prefab needs to have a ShipControlModule attached to the parent gameobject."); }
                #endif
            }

            // FUTURE - Optionally create ships using DOTS



            return isSuccesful;
        }

        /// <summary>
        /// Create a ship from a prefab. Optionally supply a parent transform, a
        /// callback method for when the ship is destroyed, and an AI script to attach.
        /// </summary>
        /// <param name="shipPrefab"></param>
        /// <param name="shipPosition"></param>
        /// <param name="shipRotation"></param>
        /// <param name="parentTrfm"></param>
        /// <param name="callbackOnDestroy"></param>
        /// <param name="aiComponentType"></param>
        /// <returns></returns>
        public ShipControlModule CreateShip(GameObject shipPrefab, Vector3 shipPosition, Quaternion shipRotation, Transform parentTrfm = null, ShipControlModule.CallbackOnDestroy callbackOnDestroy = null, System.Type aiComponentType = null)
        {
            ShipControlModule shipControlModule = null;

            GameObject shipGameObjectInstance = Object.Instantiate(shipPrefab, shipPosition, shipRotation);

            if (shipGameObjectInstance != null)
            {
                // Set the object's parent if one is supplied
                if (parentTrfm != null) { shipGameObjectInstance.transform.SetParent(parentTrfm); }

                shipControlModule = shipGameObjectInstance.GetComponent<ShipControlModule>();

                if (shipControlModule != null)
                {
                    // Set the name of the custom callback method.
                    shipControlModule.callbackOnDestroy = callbackOnDestroy;

                    if (!shipControlModule.IsInitialised) { shipControlModule.InitialiseShip(); }
                }

                // Optionally add an AI Script with default values
                if (aiComponentType != null)
                {
                    if (shipGameObjectInstance.GetComponent(aiComponentType) == null) { shipGameObjectInstance.AddComponent(aiComponentType); }
                }
            }

            return shipControlModule;
        }

        #endregion

        #region Private Methods


        #endregion
    }
}