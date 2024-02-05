using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    public class AIStateMethodParameters
    {
        #region Public Variables

        /// <summary>
        /// The list of AI behaviour inputs.
        /// </summary>
        public List<AIBehaviourInput> aiBehaviourInputsList;
        /// <summary>
        /// The Ship Control Module instance of this AI ship. The ship instance can be obtained using shipControlModule.shipInstance.
        /// </summary>
        public ShipControlModule shipControlModule;
        /// <summary>
        /// The Ship AI Input Module instance of this AI ship.
        /// </summary>
        public ShipAIInputModule shipAIInputModule;

        /// <summary>
        /// The target position vector of this AI ship.
        /// </summary>
        public Vector3 targetPosition;
        /// <summary>
        /// The target rotation of this AI ship.
        /// </summary>
        public Quaternion targetRotation;
        /// <summary>
        /// The target location of this AI ship.
        /// </summary>
        public LocationData targetLocation;
        /// <summary>
        /// The target path of this AI ship.
        /// </summary>
        public PathData targetPath;
        /// <summary>
        /// The target ship of this AI ship.
        /// </summary>
        public Ship targetShip;
        /// <summary>
        /// The list of ships to evade for this AI ship.
        /// </summary>
        public List<Ship> shipsToEvade;
        /// <summary>
        /// The list of surface turrets to evade for this AI ship.
        /// </summary>
        public List<SurfaceTurretModule> surfaceTurretsToEvade;
        /// <summary>
        /// The target radius of this AI ship.
        /// </summary>
        public float targetRadius;
        /// <summary>
        /// The target distance of this AI ship.
        /// </summary>
        public float targetDistance;
        /// <summary>
        /// The target angular distance of this AI ship.
        /// </summary>
        public float targetAngularDistance;
        /// <summary>
        /// The target velocity of this AI ship.
        /// </summary>
        public Vector3 targetVelocity;
        /// <summary>
        /// The target time of this AI ship.
        /// </summary>
        public float targetTime;

        #endregion

        #region Constructors

        // Constructor
        public AIStateMethodParameters (List<AIBehaviourInput> behaviourInputsList, ShipControlModule shipControlModuleInstance, ShipAIInputModule shipAIInputModuleInstance)
        {
            this.aiBehaviourInputsList = behaviourInputsList;
            this.shipControlModule = shipControlModuleInstance;
            this.shipAIInputModule = shipAIInputModuleInstance;
            this.targetPosition = Vector3.zero;
            this.targetRotation = Quaternion.identity;
            this.targetLocation = null;
            this.targetPath = null;
            this.targetShip = null;
            this.shipsToEvade = null;
            this.surfaceTurretsToEvade = null;
            this.targetRadius = 0f;
            this.targetDistance = 0f;
            this.targetAngularDistance = 0f;
            this.targetVelocity = Vector3.zero;
            this.targetTime = 0f;
        }

        #endregion
    }
}
