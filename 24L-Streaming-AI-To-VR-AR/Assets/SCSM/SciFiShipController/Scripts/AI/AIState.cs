using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class used for AI states.
    /// </summary>
    public class AIState
    {
        #region Public Enumerations

        public enum BehaviourCombiner
        {
            /// <summary>
            /// Chooses the first non-zero behaviour in the list.
            /// </summary>
            PriorityOnly = 10,
            /// <summary>
            /// Loops through the non-zero behaviours in the list (in order) and has a probability specified by weighting 
            /// of choosing each one.
            /// </summary>
            PrioritisedDithering = 20,
            /// <summary>
            /// Uses a weighted combination of all the non-zero behaviours in the list.
            /// </summary>
            WeightedAverage = 30
        }

        #endregion

        #region Public Non-Static Variables

        /// <summary>
        /// The name of the state.
        /// </summary>
        public string name;
        /// <summary>
        /// The ID number of the state.
        /// </summary>
        public int id;
        /// <summary>
        /// The method called by the state when it is the current state for a ship.
        /// </summary>
        public ShipAIInputModule.CallbackStateMethod callbackStateMethod;
        /// <summary>
        /// The method used by the state for combining behaviours.
        /// </summary>
        public BehaviourCombiner behaviourCombiner;

        #endregion

        #region Private Static Variables

        private static bool isInitialising = false;
        private static bool isInitialised = false;

        private static List<AIState> aiStatesList;

        #endregion

        #region Public Static Readonly Variables

        /// <summary>
        /// The state ID number for the Idle state. The Idle state has no required inputs.
        /// While in the Idle state, the AI ship will remain stationary.
        /// </summary>
        public static readonly int idleStateID = 0;
        /// <summary>
        /// The state ID number for the Move To state. The Move To state takes the following required inputs: 
        /// TargetPath / TargetLocation / TargetPosition. It also takes the following optional inputs: ShipsToEvade.
        /// While in the Move To state, the AI ship will either follow TargetPath, or if that is null, move towards
        /// TargetLocation, or if that is null, move towards TargetPosition. It will also evade the targeting regions
        /// of up to 5 ships in the ShipsToEvade list. The state action is set as completed when the ship is within
        /// the ship radius of TargetPosition / TargetLocation or it if reaches the end of TargetPath.
        /// </summary>
        public static readonly int moveToStateID = 1;
        /// <summary>
        /// The state ID number for the Dogfight state. The Dogfight state takes the following required inputs: TargetShip.
        /// While in the Dogfight state, the AI ship will attack TargetShip, whilst also trying to evade TargetShip if 
        /// TargetShip ends up behind it. The state action is set as completed when TargetShip is destroyed.
        /// </summary>
        public static readonly int dogfightStateID = 2;
        /// <summary>
        /// The state ID number for the Docking state. The Docking state takes the following required inputs: 
        /// TargetPosition, TargetRotation, TargetRadius, TargetDistance, TargetAngularDistance, TargetVelocity.
        /// While in the Docking state, the AI ship will move directly towards TargetPosition (a position moving with velocity 
        /// TargetVelocity) and (when it gets within TargetRadius) attempt to match TargetRotation. The state action is set as 
        /// completed once the ship is within TargetDistance of TargetPosition and TargetAngularDistance of TargetRotation. 
        /// </summary>
        public static readonly int dockingStateID = 3;
        /// <summary>
        /// The state ID number for the Strafing Run state. The Strafing Run state takes the following required inputs: 
        /// TargetLocation / TargetPosition, TargetRadius. It also takes the following optional inputs: SurfaceTurretsToEvade.
        /// While in the Strafing Run state, the AI ship will move directly towards TargetLocation / TargetPosition until it gets
        /// within TargetRadius. Then it will move past and away from TargetLocation / TargetPosition until it escapes the
        /// TargetRadius, at which point it will set the state action as completed.
        /// </summary>
        public static readonly int strafingRunStateID = 4;

        #endregion

        #region Constructors

        // Class constructor
        public AIState (string stateName, int stateID, ShipAIInputModule.CallbackStateMethod stateMethod, BehaviourCombiner stateBehaviourCombiner)
        {
            this.name = stateName;
            this.id = stateID;
            this.callbackStateMethod = stateMethod;
            this.behaviourCombiner = stateBehaviourCombiner;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// The state method for the Idle state.
        /// </summary>
        /// <param name="stateMethodParameters"></param>
        private static void IdleState(AIStateMethodParameters stateMethodParameters)
        {
            stateMethodParameters.aiBehaviourInputsList[0].behaviourType = AIBehaviourInput.AIBehaviourType.CustomIdle;
            stateMethodParameters.aiBehaviourInputsList[0].weighting = 1f;
        }

        /// <summary>
        /// The state method for the Move To state.
        /// </summary>
        /// <param name="stateMethodParameters"></param>
        private static void MoveToState (AIStateMethodParameters stateMethodParameters)
        {
            // Priority #1: Obstacle avoidance
            stateMethodParameters.aiBehaviourInputsList[0].behaviourType = AIBehaviourInput.AIBehaviourType.CustomObstacleAvoidance;
            stateMethodParameters.aiBehaviourInputsList[0].weighting = 1f;

            // Priority #2: Evade the targeting regions of a list of ships
            int shipsToEvadeCount = 0;
            if (stateMethodParameters.shipsToEvade != null)
            {
                shipsToEvadeCount = stateMethodParameters.shipsToEvade.Count;
                // Limit ships to evade to a maximum of 5
                if (shipsToEvadeCount > 5) { shipsToEvadeCount = 5; }
                // Loop over all the ships to evade
                Ship shipToEvade;
                for (int i = 0; i < shipsToEvadeCount; i++)
                {
                    shipToEvade = stateMethodParameters.shipsToEvade[i];
                    if (shipToEvade != null && !shipToEvade.Destroyed())
                    {
                        stateMethodParameters.aiBehaviourInputsList[1 + i].behaviourType = AIBehaviourInput.AIBehaviourType.CustomUnblockCone;
                        stateMethodParameters.aiBehaviourInputsList[1 + i].targetPosition = stateMethodParameters.shipsToEvade[i].TransformPosition;
                        stateMethodParameters.aiBehaviourInputsList[1 + i].targetForwards = stateMethodParameters.shipsToEvade[i].TransformForward;
                        stateMethodParameters.aiBehaviourInputsList[1 + i].targetFOVAngle = 5f;
                        //stateMethodParameters.aiBehaviourInputsList[1 + i].weighting = 0.2f;
                        stateMethodParameters.aiBehaviourInputsList[1 + i].weighting = 1f / shipsToEvadeCount;

                        //stateMethodParameters.aiBehaviourInputsList[1 + i].behaviourType = AIBehaviourInput.AIBehaviourType.CustomUnblockCylinder;
                        //stateMethodParameters.aiBehaviourInputsList[1 + i].targetPosition = shipToEvade.TransformPosition;
                        //stateMethodParameters.aiBehaviourInputsList[1 + i].targetForwards = shipToEvade.TransformForward;
                        //stateMethodParameters.aiBehaviourInputsList[1 + i].targetRadius = stateMethodParameters.shipAIInputModule.shipRadius;
                        //stateMethodParameters.aiBehaviourInputsList[1 + i].weighting = 0.2f;
                    }
                }
            }

            // Priority #3: Follow a path / move to a location / move to a position
            if (stateMethodParameters.targetPath != null)
            {
                stateMethodParameters.aiBehaviourInputsList[shipsToEvadeCount + 1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomFollowPath;
                stateMethodParameters.aiBehaviourInputsList[shipsToEvadeCount + 1].targetPath = stateMethodParameters.targetPath;
                stateMethodParameters.aiBehaviourInputsList[shipsToEvadeCount + 1].weighting = 1f;
            }
            else if (stateMethodParameters.targetLocation != null)
            {
                stateMethodParameters.aiBehaviourInputsList[shipsToEvadeCount + 1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomSeekArrival;
                stateMethodParameters.aiBehaviourInputsList[shipsToEvadeCount + 1].targetPosition = stateMethodParameters.targetLocation.position;
                stateMethodParameters.aiBehaviourInputsList[shipsToEvadeCount + 1].weighting = 1f;

                // If distance to target location is less than ship radius, set the state action as completed
                if ((stateMethodParameters.shipControlModule.shipInstance.TransformPosition - stateMethodParameters.targetLocation.position).sqrMagnitude 
                    < stateMethodParameters.shipAIInputModule.shipRadius)
                {
                    stateMethodParameters.shipAIInputModule.SetHasCompletedStateAction(true);
                }
            }
            else
            {
                stateMethodParameters.aiBehaviourInputsList[shipsToEvadeCount + 1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomSeekArrival;
                stateMethodParameters.aiBehaviourInputsList[shipsToEvadeCount + 1].targetPosition = stateMethodParameters.targetPosition;
                stateMethodParameters.aiBehaviourInputsList[shipsToEvadeCount + 1].weighting = 1f;

                // If distance to target position is less than ship radius, set the state action as completed
                if ((stateMethodParameters.shipControlModule.shipInstance.TransformPosition - stateMethodParameters.targetPosition).sqrMagnitude
                    < stateMethodParameters.shipAIInputModule.shipRadius)
                {
                    stateMethodParameters.shipAIInputModule.SetHasCompletedStateAction(true);
                }
            }
        }

        /// <summary>
        /// The state method for the Dogfight state.
        /// </summary>
        /// <param name="stateMethodParameters"></param>
        private static void DogfightState (AIStateMethodParameters stateMethodParameters)
        {
            if (stateMethodParameters.targetShip != null)
            {
                // Pre-calculation
                Vector3 fromTargetShipVector = stateMethodParameters.shipControlModule.shipInstance.TransformPosition - stateMethodParameters.targetShip.TransformPosition;
                float distToTargetShip = fromTargetShipVector.magnitude;
                float approxPursueInterceptionTime = stateMethodParameters.shipControlModule.shipInstance.WorldVelocity.sqrMagnitude > 0.01f ? 
                    distToTargetShip / stateMethodParameters.shipControlModule.shipInstance.WorldVelocity.magnitude : 1000f;
                float approxEvadeInterceptionTime = stateMethodParameters.targetShip.WorldVelocity.sqrMagnitude > 0.01f ?
                    distToTargetShip / stateMethodParameters.targetShip.WorldVelocity.magnitude : 1000f;

                // Priority #1: Obstacle avoidance
                stateMethodParameters.aiBehaviourInputsList[0].behaviourType = AIBehaviourInput.AIBehaviourType.CustomObstacleAvoidance;
                stateMethodParameters.aiBehaviourInputsList[0].weighting = 1f;

                // Priority #2: Evade target ship's targeting region
                stateMethodParameters.aiBehaviourInputsList[1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomUnblockCone;
                stateMethodParameters.aiBehaviourInputsList[1].targetPosition = stateMethodParameters.targetShip.TransformPosition;
                stateMethodParameters.aiBehaviourInputsList[1].targetForwards = stateMethodParameters.targetShip.TransformForward;
                stateMethodParameters.aiBehaviourInputsList[1].targetFOVAngle = 5f;
                stateMethodParameters.aiBehaviourInputsList[1].weighting = 1f;

                // Priority #3: Evade target ship (if we are "in front" of the target ship and within 3 seconds evade interception time)
                if (Vector3.Dot(fromTargetShipVector, stateMethodParameters.targetShip.TransformForward) > 0f &&
                    Vector3.Dot(fromTargetShipVector, stateMethodParameters.shipControlModule.shipInstance.TransformForward) > 0f &&
                    approxEvadeInterceptionTime < 3f)
                {
                    stateMethodParameters.aiBehaviourInputsList[2].behaviourType = AIBehaviourInput.AIBehaviourType.CustomFlee;
                    stateMethodParameters.aiBehaviourInputsList[2].targetPosition = stateMethodParameters.targetShip.TransformPosition;
                    stateMethodParameters.aiBehaviourInputsList[2].targetVelocity = stateMethodParameters.targetShip.WorldVelocity;
                    stateMethodParameters.aiBehaviourInputsList[2].weighting = 1f;
                }

                // Priority #4: Pursue/seek target ship
                if (approxPursueInterceptionTime > 3f && approxPursueInterceptionTime < 10f)
                {
                    stateMethodParameters.aiBehaviourInputsList[3].behaviourType = AIBehaviourInput.AIBehaviourType.CustomPursuitArrival;
                }
                else
                {
                    stateMethodParameters.aiBehaviourInputsList[3].behaviourType = AIBehaviourInput.AIBehaviourType.CustomSeekMovingArrival;
                }
                stateMethodParameters.aiBehaviourInputsList[3].targetPosition = stateMethodParameters.targetShip.TransformPosition;
                stateMethodParameters.aiBehaviourInputsList[3].targetVelocity = stateMethodParameters.targetShip.WorldVelocity;
                stateMethodParameters.aiBehaviourInputsList[3].useTargetingAccuracy = true;
                stateMethodParameters.aiBehaviourInputsList[3].weighting = 1f;

                // Set the state action as completed once the target ship is destroyed
                // TODO: should possibly choose different action if the target ship is destroyed?
                if (stateMethodParameters.targetShip.Destroyed())
                {
                    stateMethodParameters.shipAIInputModule.SetHasCompletedStateAction(true);
                }
            }
            else
            {
                // Fallback: If the target ship is null (which it shouldn't be) simply seek the target position with obstacle avoidance

                // Priority #1: Obstacle avoidance
                stateMethodParameters.aiBehaviourInputsList[0].behaviourType = AIBehaviourInput.AIBehaviourType.CustomObstacleAvoidance;
                stateMethodParameters.aiBehaviourInputsList[0].weighting = 1f;

                // Priority #2: Seek target position
                stateMethodParameters.aiBehaviourInputsList[1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomSeek;
                stateMethodParameters.aiBehaviourInputsList[1].targetPosition = stateMethodParameters.targetPosition;
                stateMethodParameters.aiBehaviourInputsList[1].weighting = 1f;
            }
        }

        /// <summary>
        /// The state method for the Docking state.
        /// </summary>
        /// <param name="stateMethodParameters"></param>
        private static void DockingState(AIStateMethodParameters stateMethodParameters)
        {
            float sqrPosDelta = (stateMethodParameters.shipControlModule.shipInstance.TransformPosition - stateMethodParameters.targetPosition).sqrMagnitude;
            float sqrRadius = stateMethodParameters.targetRadius * stateMethodParameters.targetRadius;

            // Priority #1: Obstacle avoidance
            // Only activates when outside of the target radius
            if (sqrPosDelta > sqrRadius)
            {
                stateMethodParameters.aiBehaviourInputsList[0].behaviourType = AIBehaviourInput.AIBehaviourType.CustomObstacleAvoidance;
                //stateMethodParameters.aiBehaviourInputsList[0].weighting = 1f;
            }

            // Priority #2: Docking
            stateMethodParameters.aiBehaviourInputsList[1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomDock;
            stateMethodParameters.aiBehaviourInputsList[1].targetPosition = stateMethodParameters.targetPosition;
            stateMethodParameters.aiBehaviourInputsList[1].targetForwards = stateMethodParameters.targetRotation * Vector3.forward;
            stateMethodParameters.aiBehaviourInputsList[1].targetUp = stateMethodParameters.targetRotation * Vector3.up;
            stateMethodParameters.aiBehaviourInputsList[1].targetRadius = stateMethodParameters.targetRadius;
            stateMethodParameters.aiBehaviourInputsList[1].targetVelocity = stateMethodParameters.targetVelocity;
            stateMethodParameters.aiBehaviourInputsList[1].targetTime = stateMethodParameters.targetTime;
            stateMethodParameters.aiBehaviourInputsList[1].weighting = 1f;

            // If we reach the following conditions, set the state action as completed:
            // - Within target distance metres of target position
            // - Within target angular distance degrees of the target rotation
            float angleDelta = Quaternion.Angle(stateMethodParameters.shipControlModule.shipInstance.TransformRotation, stateMethodParameters.targetRotation);
            if (sqrPosDelta <= stateMethodParameters.targetDistance * stateMethodParameters.targetDistance && 
                angleDelta <= stateMethodParameters.targetAngularDistance)
            {
                stateMethodParameters.shipAIInputModule.SetHasCompletedStateAction(true);
            }
        }

        /// <summary>
        /// The state method for the Strafing Run state.
        /// </summary>
        /// <param name="stateMethodParameters"></param>
        private static void StrafingRunState(AIStateMethodParameters stateMethodParameters)
        {
            // Priority #1: Obstacle avoidance
            stateMethodParameters.aiBehaviourInputsList[0].behaviourType = AIBehaviourInput.AIBehaviourType.CustomObstacleAvoidance;
            stateMethodParameters.aiBehaviourInputsList[0].weighting = 1f;

            // Priority #2: Evade the targeting regions of a list of ships
            int surfaceTurretsToEvadeCount = 0;
            if (stateMethodParameters.surfaceTurretsToEvade != null)
            {
                surfaceTurretsToEvadeCount = stateMethodParameters.surfaceTurretsToEvade.Count;
                // Limit surface turrets to evade to a maximum of 5
                if (surfaceTurretsToEvadeCount > 5) { surfaceTurretsToEvadeCount = 5; }
                // Loop over all the ships to evade
                SurfaceTurretModule surfaceTurretToEvade;
                for (int i = 0; i < surfaceTurretsToEvadeCount; i++)
                {
                    surfaceTurretToEvade = stateMethodParameters.surfaceTurretsToEvade[i];
                    if (surfaceTurretToEvade != null && (surfaceTurretToEvade.weapon.Health > 0f || !surfaceTurretToEvade.isDestroyOnNoHealth))
                    {
                        // CURRENT VERSION: TODO FIX TRANSFORM FORWARD
                        //stateMethodParameters.aiBehaviourInputsList[1 + i].behaviourType = AIBehaviourInput.AIBehaviourType.CustomUnblockCone;
                        //stateMethodParameters.aiBehaviourInputsList[1 + i].targetPosition = stateMethodParameters.surfaceTurretsToEvade[i].TransformPosition;
                        //stateMethodParameters.aiBehaviourInputsList[1 + i].targetForwards = stateMethodParameters.surfaceTurretsToEvade[i].TransformForward;
                        //stateMethodParameters.aiBehaviourInputsList[1 + i].targetFOVAngle = 5f;
                        //stateMethodParameters.aiBehaviourInputsList[1 + i].weighting = 0.2f;

                        // What is this?
                        //stateMethodParameters.aiBehaviourInputsList[1 + i].behaviourType = AIBehaviourInput.AIBehaviourType.CustomUnblockCylinder;
                        //stateMethodParameters.aiBehaviourInputsList[1 + i].targetPosition = shipToEvade.TransformPosition;
                        //stateMethodParameters.aiBehaviourInputsList[1 + i].targetForwards = shipToEvade.TransformForward;
                        //stateMethodParameters.aiBehaviourInputsList[1 + i].targetRadius = stateMethodParameters.shipAIInputModule.shipRadius;
                        //stateMethodParameters.aiBehaviourInputsList[1 + i].weighting = 0.2f;
                    }
                }
            }

            // Get the direction and distance to the target position
            Vector3 fromTargetPositionVector = Vector3.zero;
            if (stateMethodParameters.targetLocation != null)
            {
                fromTargetPositionVector = stateMethodParameters.shipControlModule.shipInstance.TransformPosition - stateMethodParameters.targetLocation.position;
            }
            else
            {
                fromTargetPositionVector = stateMethodParameters.shipControlModule.shipInstance.TransformPosition - stateMethodParameters.targetPosition;
            }
            float distToTargetPosition = fromTargetPositionVector.magnitude;

            // Get the current state stage index
            int currentStateStageIndex = stateMethodParameters.shipAIInputModule.GetCurrentStateStageIndex();

            if (currentStateStageIndex == 0)
            {
                // Stage 1: Going towards the target position

                // Priority #3: Move to a location / move to a position
                if (stateMethodParameters.targetLocation != null)
                {
                    stateMethodParameters.aiBehaviourInputsList[surfaceTurretsToEvadeCount + 1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomSeek;
                    // Target position is moved towards our ship by the target radius, to prevent obstacle
                    // avoidance from picking up the target object
                    stateMethodParameters.aiBehaviourInputsList[surfaceTurretsToEvadeCount + 1].targetPosition =
                        stateMethodParameters.targetLocation.position + (fromTargetPositionVector / distToTargetPosition * stateMethodParameters.targetRadius);
                    stateMethodParameters.aiBehaviourInputsList[surfaceTurretsToEvadeCount + 1].useTargetingAccuracy = true;
                    stateMethodParameters.aiBehaviourInputsList[surfaceTurretsToEvadeCount + 1].weighting = 1f;
                }
                else
                {
                    stateMethodParameters.aiBehaviourInputsList[surfaceTurretsToEvadeCount + 1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomSeek;
                    // Target position is moved towards our ship by the target radius, to prevent obstacle
                    // avoidance from picking up the target object
                    stateMethodParameters.aiBehaviourInputsList[surfaceTurretsToEvadeCount + 1].targetPosition =
                        stateMethodParameters.targetPosition + (fromTargetPositionVector / distToTargetPosition * stateMethodParameters.targetRadius);
                    stateMethodParameters.aiBehaviourInputsList[surfaceTurretsToEvadeCount + 1].useTargetingAccuracy = true;
                    stateMethodParameters.aiBehaviourInputsList[surfaceTurretsToEvadeCount + 1].weighting = 1f;
                }

                // If we get within the target radius, go to stage 2
                if (distToTargetPosition < stateMethodParameters.targetRadius)
                {
                    stateMethodParameters.shipAIInputModule.SetCurrentStateStageIndex(1);
                }
            }
            else if (currentStateStageIndex == 1)
            {
                // Stage 2: Going away from the target position

                // Priority #3: Move away from a location / move away from a position
                if (stateMethodParameters.targetLocation != null)
                {
                    stateMethodParameters.aiBehaviourInputsList[surfaceTurretsToEvadeCount + 1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomSeek;
                    stateMethodParameters.aiBehaviourInputsList[surfaceTurretsToEvadeCount + 1].targetPosition =
                        stateMethodParameters.targetLocation.position + Vector3.Reflect(-fromTargetPositionVector / distToTargetPosition * stateMethodParameters.targetRadius * 2f, Vector3.up);
                    stateMethodParameters.aiBehaviourInputsList[surfaceTurretsToEvadeCount + 1].weighting = 1f;
                }
                else
                {
                    stateMethodParameters.aiBehaviourInputsList[surfaceTurretsToEvadeCount + 1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomFlee;
                    stateMethodParameters.aiBehaviourInputsList[surfaceTurretsToEvadeCount + 1].targetPosition =
                        stateMethodParameters.targetPosition + Vector3.Reflect(-fromTargetPositionVector / distToTargetPosition * stateMethodParameters.targetRadius * 2f, Vector3.up);
                    stateMethodParameters.aiBehaviourInputsList[surfaceTurretsToEvadeCount + 1].weighting = 1f;
                }

                // If we get outside the target radius, set the state action as completed
                if (distToTargetPosition > stateMethodParameters.targetRadius)
                {
                    stateMethodParameters.shipAIInputModule.SetHasCompletedStateAction(true);
                }
            }
        }

        #endregion

        #region Public Static API Methods

        public static void Initialise ()
        {
            if (isInitialised || isInitialising) { return; }
            else
            {
                isInitialising = true;

                // Create the AI States List with the number of elements equal to the number of predefined states
                aiStatesList = new List<AIState>(5);
                // Add the predefined states
                AddState("Idle", IdleState, BehaviourCombiner.PriorityOnly);
                AddState("Move To", MoveToState, BehaviourCombiner.PrioritisedDithering);
                AddState("Dogfight", DogfightState, BehaviourCombiner.PriorityOnly);
                AddState("Docking", DockingState, BehaviourCombiner.PriorityOnly);
                AddState("Strafing Run", StrafingRunState, BehaviourCombiner.PrioritisedDithering);

                isInitialising = false;
                isInitialised = true;
            }
        }

        /// <summary>
        /// Adds a new AI state with a given name and state method. Returns the ID of the new state.
        /// </summary>
        /// <param name="newStateName"></param>
        /// <param name="newStateMethod"></param>
        /// <returns></returns>
        public static int AddState (string newStateName, ShipAIInputModule.CallbackStateMethod newStateMethod, BehaviourCombiner newStateBehaviourCombiner = BehaviourCombiner.PriorityOnly)
        {
            // Allow AddState to be called during Initialise()
            if (isInitialised || isInitialising)
            {
                // Create a new state with the given name and method
                // The ID is set to what its index in the list is going to be
                AIState newAIState = new AIState(newStateName, aiStatesList.Count, newStateMethod, newStateBehaviourCombiner);
                // Add the state to the list
                aiStatesList.Add(newAIState);
                // Return the ID of the newly created state
                return newAIState.id;
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("AIState.AddState - AIState is not initialised. Please call AIState.Initialise() first.");
                #endif
                return -1;
            }
        }

        /// <summary>
        /// Returns the AI state with the corresponding state ID.
        /// </summary>
        /// <param name="stateID"></param>
        /// <returns></returns>
        public static AIState GetState (int stateID)
        {
            // First check that the supplied ID is valid
            if (stateID >= 0 && stateID < aiStatesList.Count)
            {
                return aiStatesList[stateID];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to return the state name. Use with caution as this
        /// will generate GC. When comparing in code, use:
        /// if (stateID == AIState.moveToState) etc instead to avoid GC.
        /// </summary>
        /// <param name="stateID"></param>
        /// <returns></returns>
        public static string GetStateName (int stateID)
        {
            AIState aiState = GetState(stateID);

            if (aiState != null) { return string.IsNullOrEmpty(aiState.name) ? "Unnamed State" : aiState.name; }
            else { return "State is null"; }
        }

        #endregion

        #region Public Member API Methods

        public override string ToString()
        {
            return name + " id: " + id.ToString();
        }

        #endregion
    }
}
