using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Demo to spawn an AI Ship and head towards the first target location
    /// using a custom AIState and custom behaviour.
    /// Ship prefabs should NOT contain a PlayerInputModule.
    /// Optionally use a Ship that is already in the scene rather than spawning
    /// a new ship from a prefab.
    /// This is only sample to demonstrate how API calls could be used in
    /// your own code.
    /// </summary>
    public class DemoFlyToLocation : MonoBehaviour
    {
        #region Public variables

        /// <summary>
        /// The prefab from the project of the ship that will be instantiated
        /// for squadron members. See also ShipSpawner.cs
        /// </summary>
        public GameObject shipPrefab;

        // This enables an existing Ship GameObject in the scene
        // to be used instead of the prefab.
        public bool isShipAlreadyInScene = false;

        public DemoLocation flyToLocation;
        public AIBehaviourInput.AIBehaviourType primaryBehaviourType = AIBehaviourInput.AIBehaviourType.CustomSeekArrival;
        // This may be set for each location or globally like in this demo script. It could also be hard coded
        // into the CustomArrival method. We are added it here for testing.
        public float minArrivalSpeed = 30f;
        public int squadronId = -1;
        public int factionId = -1;
        public bool isHealthDisplayed = false;

        #endregion

        #region Private variables
        private Canvas canvas;
        private UnityEngine.UI.Text uiTextHealthLabel;
        private UnityEngine.UI.Text uiTextHealthValue;
        private ShipControlModule shipControlModule;
        private ShipAIInputModule shipAIInputModule;
        private Vector3 firstLocationPosition;

        //private AIBehaviourInput.AIBehaviourType currentBehaviourType = AIBehaviourInput.AIBehaviourType.CustomSeekArrival;

        #endregion

        #region Initialisation Methods

        void Awake()
        {
            // We could just instantiate the prefab but ShipSpawner can
            // help out with a few other things too.
            ShipSpawner shipSpawner = new ShipSpawner();
            if (shipSpawner != null)
            {
                if (isShipAlreadyInScene)
                {
                    if (shipPrefab == null) { Debug.LogWarning("DemoFlyToLocation - no gameobject was supplied"); }
                    else
                    {
                        shipControlModule = shipPrefab.GetComponent<ShipControlModule>();
                    }
                }
                else
                {
                    shipControlModule = shipSpawner.CreateShip(shipPrefab, transform.position, transform.rotation);
                }
                if (shipControlModule != null)
                {
                    // Update squadron-related fields in the ship
                    if (shipControlModule.shipInstance != null)
                    {
                        shipControlModule.shipInstance.factionId = factionId;
                        shipControlModule.shipInstance.squadronId = squadronId;

                        // Tell the ship to call our custom method immediately before the ship is destroyed
                        shipControlModule.callbackOnDestroy = ShipDestroyed;

                        // We assume the ShipAIInputModule was attached to the prefab and AwakeOnInitialise was disabled.
                        shipAIInputModule = shipControlModule.GetComponent<ShipAIInputModule>();
                        if (shipAIInputModule != null)
                        {
                            if (!shipAIInputModule.IsInitialised) { shipAIInputModule.Initialise(); }

                            if (shipAIInputModule.IsInitialised)
                            {
                                DemoFlyToLocationShipData shipData = shipAIInputModule.GetComponent<DemoFlyToLocationShipData>();
                                if (shipData == null) { shipData = shipAIInputModule.gameObject.AddComponent<DemoFlyToLocationShipData>(); }

                                // Add a custom state and set it as the current state
                                shipAIInputModule.SetState(AIState.AddState("Demo Custom State", DemoFlyToLocationState, AIState.BehaviourCombiner.PriorityOnly));

                                firstLocationPosition = flyToLocation.transform.position;
                                shipAIInputModule.AssignTargetPosition(firstLocationPosition);
                                
                                if (primaryBehaviourType == AIBehaviourInput.AIBehaviourType.CustomSeekArrival)
                                {
                                    // Whenever the CustomArrival behaviour is used call our DemoArrivalBehaviour(...) method
                                    // rather than the standard ShipAIInputModule Arrival behaviour.
                                    shipAIInputModule.callbackCustomSeekArrivalBehaviour = DemoArrivalBehaviour;
                                }
                                else { shipAIInputModule.callbackCustomSeekArrivalBehaviour = null; }

                                // Set the current AI behaviour type
                                shipData.currentBehaviourType = primaryBehaviourType;
                            }
                        }
                        #if UNITY_EDITOR
                        else
                        {
                            throw new MissingComponentException(shipPrefab.name + " prefab is missing the ShipAIInputModule");
                        }
                        #endif

                        if (isHealthDisplayed)
                        {
                            SampleShowShipHealth shipHealth = GetComponent<SampleShowShipHealth>();
                            if (shipHealth == null) { shipHealth = gameObject.AddComponent<SampleShowShipHealth>(); }
                            if (shipHealth != null)
                            {
                                shipHealth.shipControlModule = shipControlModule;
                                shipHealth.isHealthDisplayed = true;
                                shipHealth.Initialise();
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Public Member Methods

        /// <summary>
        /// Method that gets called from ShipControlModule immediately before the AI Ship is destroyed.
        /// This is configured in the Awake method above
        /// </summary>
        /// <param name="ship"></param>
        public void ShipDestroyed(Ship ship)
        {
            // Go back to the start and fly to the first location
            if (ship.respawningMode == Ship.RespawningMode.RespawnAtOriginalPosition)
            {
                ship.ResetHealth();
                if (shipAIInputModule != null && shipAIInputModule.IsInitialised) { shipAIInputModule.AssignTargetPosition(firstLocationPosition); }
            }
        }

        /// <summary>
        /// Demo state method.
        /// </summary>
        /// <param name="stateMethodParameters"></param>
        public void DemoFlyToLocationState (AIStateMethodParameters stateMethodParameters)
        {
            // Check AI Ship Data to find what behaviour type should be used
            DemoFlyToLocationShipData shipData = shipAIInputModule.GetComponent<DemoFlyToLocationShipData>();

            if (shipData != null)
            {
                // Configure a single behaviour with that behaviour type
                stateMethodParameters.aiBehaviourInputsList[0].behaviourType = shipData.currentBehaviourType;
                stateMethodParameters.aiBehaviourInputsList[0].targetPosition = stateMethodParameters.targetPosition;
                stateMethodParameters.aiBehaviourInputsList[0].weighting = 1f;
            }
            #if UNITY_EDITOR
            else
            {
                throw new MissingComponentException(shipAIInputModule.name + " prefab is missing the DemoFlyToLocationShipData component");
            }
            #endif
        }

        /// <summary>
        /// Method that gets called from ShipAIModule when one of the AIBehaviours is CustomArrival.
        /// This is configured in the Awake method above
        /// </summary>
        /// <param name="behaviourInput"></param>
        /// <param name="behaviourOutput"></param>
        public void DemoArrivalBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput)
        {
            // We should already have a reference to the NPC ship that was spawned in Awake().
            if (shipControlModule != null && shipAIInputModule != null)
            {
                // Desired heading is towards the target position
                Vector3 shipPosition = shipControlModule.shipInstance.TransformPosition;

                Vector3 headingVector = behaviourInput.targetPosition - shipPosition;
                // Distance between 2 positions is SQRT( (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z) )
                float headingVectorMagnitude = (float)System.Math.Sqrt(headingVector.x * headingVector.x + headingVector.y * headingVector.y + headingVector.z * headingVector.z);
                Vector3 headingVectorNormalised = headingVector / headingVectorMagnitude;
                behaviourOutput.heading = headingVectorNormalised;

                // If deceleration rate is not greater than zero, set to some arbitary rate
                //float decelerationRate = shipAIInputModule.decelerationRate > 0f ? shipAIInputModule.decelerationRate : 10f;

                //behaviourOutput.velocityOutput = headingVectorNormalised * (float)System.Math.Sqrt((minArrivalSpeed * minArrivalSpeed) + (2f * decelerationRate * headingVectorMagnitude));

                // Calculate max speed we can do now while still being able to slow down to the correct speed upon arrival
                behaviourOutput.velocity = headingVectorNormalised * shipAIInputModule.MaxSpeedFromBrakingDistance(minArrivalSpeed, headingVectorMagnitude, behaviourInput.shipInstance.LocalVelocity.normalized);

                // Target output is the target ship's position
                behaviourOutput.target = shipPosition;
                behaviourOutput.setTarget = true;
                // No desired up direction
                behaviourOutput.up = Vector3.zero;
            }
        }

        #endregion
    }
}

