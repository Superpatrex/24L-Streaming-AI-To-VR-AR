using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class containing data for an AI Behaviour Input.
    /// </summary>
    public class AIBehaviourInput
    {
        #region Enumerations

        /// <summary>
        /// The type of behaviour. Multiple behaviours can
        /// be combined together.
        /// </summary>
        public enum AIBehaviourType
        {
            /// <summary>
            /// Comes to a complete stop.
            /// Required inputs: Weighting.
            /// </summary>
            Idle = 0,
            /// <summary>
            /// Moves directly towards target position.
            /// Required inputs: Target position, weighting.
            /// Optional inputs: Use targeting accuracy.
            /// </summary>
            Seek = 1,
            /// <summary>
            /// Moves directly away from target position.
            /// Required inputs: Target position, weighting.
            /// </summary>
            Flee = 2,
            /// <summary>
            /// Moves towards the future position of an object currently at target position moving with a 
            /// velocity of target velocity.
            /// Required inputs: Target position, target velocity, weighting.
            /// Optional inputs: Use targeting accuracy.
            /// </summary>
            Pursuit = 3,
            /// <summary>
            /// Moves away from the future position of an object currently at target position moving with a 
            /// velocity of target velocity.
            /// Required inputs: Target position, target velocity, weighting.
            /// </summary>
            Evasion = 4,
            /// <summary>
            /// Moves directly towards target position, slowing down when nearing target position to come to a 
            /// complete stop upon reaching it.
            /// Required inputs: Target position, weighting.
            /// Optional inputs: Use targeting accuracy.
            /// </summary>
            SeekArrival = 5,
            /// <summary>
            /// Moves directly towards target position, changing speed when nearing the target position to 
            /// match target velocity upon reaching it.
            /// Required inputs: Target position, target velocity, weighting.
            /// Optional inputs: Use targeting accuracy.
            /// </summary>
            SeekMovingArrival = 6,
            /// <summary>
            /// Moves towards the future position of an object currently at target position moving with a 
            /// velocity of target velocity, changing speed when nearing the target position to match 
            /// target velocity upon reaching it.
            /// Required inputs: Target position, target velocity, weighting.
            /// Optional inputs: Use targeting accuracy.
            /// </summary>
            PursuitArrival = 7,
            //Avoid = 11,
            //Follow = 12,
            //BlockCylinder = 16,
            //BlockCone = 17,
            /// <summary>
            /// Moves out of an imaginary cylinder. The cylinder starts at the target position, stretches out 
            /// infinitely in the direction of target forwards and has a radius of target radius. If the ship 
            /// is not in the cylinder, returns a zero output.
            /// Required inputs: Target position, target forwards, target radius, weighting.
            /// </summary>
            UnblockCylinder = 19,
            /// <summary>
            /// Moves out of an imaginary cone. The cone starts at the target position, stretches out 
            /// infinitely in the direction of target forwards, and the angle between its central axis and its 
            /// edges is target FOV angle. If the ship is not in the cone, returns a zero output.
            /// Required inputs: Target position, target forwards, target FOV angle, weighting.
            /// </summary>
            UnblockCone = 20,
            /// <summary>
            /// Takes preventative action to avoid obstacles. If the ship does need to take preventative 
            /// action, returns a zero output.
            /// Required inputs: Weighting.
            /// </summary>
            ObstacleAvoidance = 22,
            //Wander = 25,
            /// <summary>
            /// Moves onto and then follows the target path.
            /// Required inputs: Target path, weighting.
            /// Optional inputs: Use targeting accuracy.
            /// </summary>
            FollowPath = 28,
            /// <summary>
            /// Moves directly towards target position and (when it gets within target radius) attempts to match orientation 
            /// of target forwards and target up. Target velocity indicates the velocity of the target position (set it to
            /// Vector3.zero if it is not moving). Target time indicates the time it will attempt to take to move from the target
            /// radius to the target position.
            /// Required inputs: Target position, target forwards, target up, target radius, target velocity, target time, weighting.
            /// </summary>
            Dock = 31,
            CustomIdle = 200,
            CustomSeek = 201,
            CustomFlee = 202,
            CustomPursuit = 203,
            CustomEvasion = 204,
            CustomSeekArrival = 205,
            CustomSeekMovingArrival = 206,
            CustomPursuitArrival = 207,
            //CustomFollow = 211,
            //CustomAvoid = 212,
            //CustomBlockCylinder = 216,
            //CustomBlockCone = 217,
            CustomUnblockCylinder = 219,
            CustomUnblockCone = 220,
            CustomObstacleAvoidance = 222,
            //CustomWander = 225
            CustomFollowPath = 228,
            CustomDock = 231
        }

        #endregion

        #region Public Variables

        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)
        /// <summary>
        /// The type of behaviour to set this behaviour input with.
        /// </summary>
        public AIBehaviourType behaviourType;
        /// <summary>
        /// The Ship instance for this AI ship.
        /// </summary>
        public Ship shipInstance;
        /// <summary>
        /// The Ship Control Module instance for this AI ship.
        /// </summary>
        public ShipControlModule shipControlModuleInstance;
        /// <summary>
        /// The Ship AI Input Module instance for this AI ship.
        /// </summary>
        public ShipAIInputModule shipAIInputModuleInstance;

        /// <summary>
        /// The relative weighting of this behaviour input. The specific use of this is determined by the behaviour combiner
        /// used by the current state.
        /// If the behaviour combiner is Priority Only, behaviour inputs with a nonzero weighting will be used while behaviour 
        /// inputs with a zero weighting will be skipped.
        /// If the behaviour combiner is Prioritised Dithering, the weighting specifies the probability that the behaviour input
        /// will be used (instead of being skipped). For example, if the weighting is 0.7, there is a 70% chance of the behaviour
        /// input being used and a 30% chance of it being skipped.
        /// If the behaviour combiner is Weighted Average, the weighting specifies how much weighting will be given to this
        /// behaviour input when all of the behaviour inputs are summed together to obtain the combined behaviour input.
        /// </summary>
        public float weighting;
        /// <summary>
        /// The target position provided to this behaviour input.
        /// </summary>
        public Vector3 targetPosition;
        /// <summary>
        /// The target path provided to this behaviour input.
        /// </summary>
        public PathData targetPath;
        /// <summary>
        /// The target velocity provided to this behaviour input.
        /// </summary>
        public Vector3 targetVelocity;
        /// <summary>
        /// The target forwards direction provided to this behaviour input. NOTE: This must be a normalised vector.
        /// </summary>
        public Vector3 targetForwards;
        /// <summary>
        /// The target up direction provided to this behaviour input. NOTE: This must be a normalised vector.
        /// </summary>
        public Vector3 targetUp;
        /// <summary>
        /// The target radius (in metres) provided to this behaviour input.
        /// </summary>
        public float targetRadius;
        /// <summary>
        /// The target Field Of View (FOV) angle (in degrees) provided to this behaviour input. NOTE: This is the angle for
        /// "one side" of the field of view, i.e. the angle is half of the entire field of view.
        /// </summary>
        public float targetFOVAngle;
        /// <summary>
        /// The target time (in seconds) provided to this behaviour input.
        /// </summary>
        public float targetTime;
        /// <summary>
        /// Whether targeting accuracy should be taken into account by this behaviour input.
        /// </summary>
        public bool useTargetingAccuracy;

        #endregion

        #region Private Static Variables

        // Steering behaviour variables
        private static Vector3 headingVector;
        private static float headingVectorMagnitude;
        private static float headingVectorSqrMagnitude;
        private static Vector3 headingVectorNormalised;
        private static float desiredSpeed;
        private static Vector3 currentWanderDirection;
        private static Ray OARay = new Ray(Vector3.zero, Vector3.forward);
        private static RaycastHit OARaycastHit;
        private static Rigidbody OARaycastHitRigidbody;

        #endregion

        #region Class Constructors

        // Class constructor #1
        public AIBehaviourInput()
        {
            SetClassDefaults();
        }

        // Class constructor #2
        public AIBehaviourInput(ShipControlModule ourShipControlModule, ShipAIInputModule ourShipAI)
        {
            SetClassDefaults();
            this.shipInstance = ourShipControlModule.shipInstance;
            this.shipControlModuleInstance = ourShipControlModule;
            this.shipAIInputModuleInstance = ourShipAI;
        }

        // Copy constructor
        public AIBehaviourInput (AIBehaviourInput behaviourInput)
        {
            if (behaviourInput == null) { SetClassDefaults(); }
            else
            {
                this.behaviourType = behaviourInput.behaviourType;
                this.shipInstance = behaviourInput.shipInstance;
                this.shipControlModuleInstance = behaviourInput.shipControlModuleInstance;
                this.shipAIInputModuleInstance = behaviourInput.shipAIInputModuleInstance;
                this.weighting = behaviourInput.weighting;
                this.targetPosition = behaviourInput.targetPosition;
                this.targetPath = behaviourInput.targetPath;
                this.targetVelocity = behaviourInput.targetVelocity;
                this.targetForwards = behaviourInput.targetForwards;
                this.targetUp = behaviourInput.targetUp;
                this.targetRadius = behaviourInput.targetRadius;
                this.targetFOVAngle = behaviourInput.targetFOVAngle;
                this.targetTime = behaviourInput.targetTime;
                this.useTargetingAccuracy = behaviourInput.useTargetingAccuracy;
            }
        }

        #endregion

        #region Public Member Methods

        public void SetClassDefaults()
        {
            behaviourType = AIBehaviourType.Idle;
            shipInstance = null;
            shipControlModuleInstance = null;
            shipAIInputModuleInstance = null;
            weighting = 0f;
            targetPosition = Vector3.zero;
            targetPath = null;
            targetVelocity = Vector3.zero;
            targetForwards = Vector3.forward;
            targetUp = Vector3.up;
            targetRadius = 0f;
            targetFOVAngle = 0f;
            targetTime = 0f;
            useTargetingAccuracy = false;
        }

        /// <summary>
        /// Clears the behaviour-dependent settings of a behaviour input.
        /// </summary>
        public void ClearBehaviourInput ()
        {
            // Set the weighting to zero
            weighting = 0f;
            // Set the behaviour input parameters to "zero" values
            // Set the target position to Vector3.zero
            targetPosition.x = 0f;
            targetPosition.y = 0f;
            targetPosition.z = 0f;
            // Set the target path to null
            targetPath = null;
            // Set the target velocity to zero
            targetVelocity.x = 0f;
            targetVelocity.y = 0f;
            targetVelocity.z = 0f;
            // Set the target forwards to zero
            targetForwards.x = 0f;
            targetForwards.y = 0f;
            targetForwards.z = 0f;
            // Set the target up to zero
            targetUp.x = 0f;
            targetUp.y = 0f;
            targetUp.z = 0f;
            // Set the target radius to zero
            targetRadius = 0f;
            // Set the target FOV angle to zero
            targetFOVAngle = 0f;
            // Set the target time to zero
            targetTime = 0f;
        }

        #endregion

        #region Public Static Methods

        #region Helper Functions

        /// <summary>
        /// Calculates an approximate time for the ship to "catch up to" another ship. Used in look ahead times.
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetVelocity"></param>
        /// <returns></returns>
        private static float ApproxInterceptionTime(Vector3 ourPosition, Vector3 ourVelocity, Vector3 targetPosition)
        {
            float ourSpeed = ourVelocity.magnitude;
            return (targetPosition - ourPosition).magnitude / (ourSpeed < 0.1f ? 0.1f : ourSpeed);

            // Returns Infinity when magnitude is 0.
            //return (targetPosition - ourPosition).magnitude / ourVelocity.magnitude;

            // Implementation #1
            //return (targetPosition - shipControlModule.shipInstance.TransformPosition).magnitude / Mathf.Max((targetVelocity - shipControlModule.shipInstance.WorldVelocity).magnitude, 0.1f);

            // Implementation #2
            //Vector3 relativePos = targetPosition - shipControlModule.shipInstance.TransformPosition;
            //if (Vector3.Dot(relativePos, shipControlModule.shipInstance.WorldVelocity) > 0f)
            //{
            //    Vector3 relativeVelo = shipControlModule.shipInstance.WorldVelocity - targetVelocity;
            //    float dotProduct = Vector3.Dot(relativePos, relativeVelo / relativeVelo.sqrMagnitude);
            //    return dotProduct > 0f ? dotProduct : 0f;
            //}
            //else { return 0f; }
        }

        /// <summary>
        /// Returns whether two objects (approximated as spheres) within given position, velocity and radius will collide
        /// within a given look ahead time.
        /// </summary>
        /// <param name="object1Position"></param>
        /// <param name="object1Velocity"></param>
        /// <param name="object2Position"></param>
        /// <param name="object2Velocity"></param>
        /// <param name="object1Radius"></param>
        /// <param name="object2Radius"></param>
        /// <param name="lookAheadTime"></param>
        /// <returns></returns>
        public static bool OnCollisionCourse(Vector3 object1Position, Vector3 object1Velocity, float object1Radius, Vector3 object2Position, Vector3 object2Velocity, float object2Radius, float lookAheadTime)
        {
            // Calculate relative position and velocity
            Vector3 relativePosition = object2Position - object1Position;
            Vector3 relativeVelocity = object2Velocity - object1Velocity;
            // Calculate maximum distance between objects required for a collision
            float collisionDistance = object1Radius + object2Radius;

            // Check if the objects are currently colliding
            if (relativePosition.magnitude < collisionDistance) { return true; }

            // Check if the objects will collide at the look ahead time
            if ((relativePosition + relativeVelocity * lookAheadTime).magnitude < collisionDistance) { return true; }

            // Check if the objects will collide between now and the look ahead time
            float closestApproachTime = -Vector3.Dot(relativeVelocity, relativePosition) / Vector3.Dot(relativeVelocity, relativeVelocity);
            if (closestApproachTime > 0f && closestApproachTime < lookAheadTime)
            {
                if ((relativePosition + relativeVelocity * closestApproachTime).magnitude < collisionDistance) { return true; }
                else { return false; }
            }
            else { return false; }
        }

        /// <summary>
        /// Performs a sweep test of sweepType along sweepRay to a maximum distance of sweepMaxDistance.
        /// sweepRay.direction must be normalised.
        /// Sweep types are: 0: No sweep test. 1: Raycast. 2: Spherecast. 3: Rigidbody sweep test.
        /// </summary>
        /// <param name="behaviourInput"></param>
        /// <param name="sweepType"></param>
        /// <param name="sweepRay"></param>
        /// <param name="sweepRaycastHit"></param>
        /// <param name="sweepMaxDistance"></param>
        /// <returns></returns>
        private static bool PerformSweep (AIBehaviourInput behaviourInput, int sweepType, Ray sweepRay, 
            ref RaycastHit sweepRaycastHit, float sweepMaxDistance)
        {
            bool sweepDidHit = false;
            switch (sweepType)
            {
                case 0:
                    // No sweep test
                    sweepDidHit = false;
                    break;
                case 1:
                    // Raycast
                    sweepDidHit = Physics.Raycast(sweepRay, out sweepRaycastHit, sweepMaxDistance,
                        behaviourInput.shipAIInputModuleInstance.obstacleLayerMask);
                    break;
                case 2:
                    // Spherecast
                    sweepRay.origin -= (sweepRay.direction * behaviourInput.shipAIInputModuleInstance.shipRadius);
                    sweepDidHit = Physics.SphereCast(sweepRay, behaviourInput.shipAIInputModuleInstance.shipRadius, out sweepRaycastHit,
                        sweepMaxDistance + behaviourInput.shipAIInputModuleInstance.shipRadius,
                        behaviourInput.shipAIInputModuleInstance.obstacleLayerMask);
                    break;
                case 3:
                    // Rigidbody sweep test
                    sweepDidHit = behaviourInput.shipControlModuleInstance.ShipRigidbody.SweepTest(sweepRay.direction, 
                        out sweepRaycastHit, sweepMaxDistance);
                    break;

            }

            return sweepDidHit;
        }

        /// <summary>
        /// Calculates the maximum speed an AI ship can fly at in order to reach a target position and velocity from its
        /// current position and velocity in world space.
        /// </summary>
        /// <param name="turnEndPosition"></param>
        /// <param name="turnEndVelocity"></param>
        /// <param name="behaviourInput"></param>
        /// <returns></returns>
        public static float CalculateMaxTurnSpeed (Vector3 turnTargetPosition, Vector3 turnTargetVelocity,
            AIBehaviourInput behaviourInput)
        {
            // TODO need to take into account following cases:
            // 1. Target velocity = 0 - then just use constant radius curve from start radius
            // 2. Weird curves:
            //    a) Dot product of start and target velocity is negative
            //    b) Dot product is positive

            float maxTurnSpeed = 0f;

            // Calculate the vector from the start position to the target position
            Vector3 startToTarget = turnTargetPosition - behaviourInput.shipInstance.TransformPosition;
            // Calculate the square distance between the start and target points
            float startToTargetSqrDistance = startToTarget.sqrMagnitude;

            // Calculate the angle from the startToTarget vector to the turnStartVelocity vector
            float startAngle = Vector3.Angle(startToTarget, behaviourInput.shipInstance.WorldVelocity) * Mathf.Deg2Rad;
            // Calculate the turn start radius
            float turnStartRadius = Mathf.Sqrt(startToTargetSqrDistance / (2f * (1f - Mathf.Cos(2f * startAngle))));

            if (turnTargetVelocity.sqrMagnitude > 0.01f)
            {
                // Case 1: Target velocity is nonzero

                // Calculate the angle from the startToTarget vector to the turnTargetVelocity vector
                float targetAngle = Vector3.Angle(startToTarget, turnTargetVelocity) * Mathf.Deg2Rad;
                // Calculate the turn end (target) radius
                float turnEndRadius = Mathf.Sqrt(startToTargetSqrDistance / (2f * (1f - Mathf.Cos(2f * targetAngle))));

                // Calculate the maximum speed along the curve to reach the target
                maxTurnSpeed = behaviourInput.shipAIInputModuleInstance.MaxSpeedAlongCurve(turnStartRadius,
                    turnEndRadius, Mathf.Sqrt(startToTargetSqrDistance), behaviourInput.shipInstance.IsGrounded);
            }
            else
            {
                // Case 2: Target velocity is zero

                // Calculate the maximum speed along the curve to reach the target
                maxTurnSpeed = behaviourInput.shipAIInputModuleInstance.MaxSpeedAlongCurve(turnStartRadius,
                    turnStartRadius, Mathf.Sqrt(startToTargetSqrDistance), behaviourInput.shipInstance.IsGrounded);
            }

            return maxTurnSpeed;
        }

        #endregion

        #region Set Behaviour Methods

        /// <summary>
        /// Sets a behaviour output to an "idle" behaviour output.
        /// Required inputs: Weighting.
        /// </summary>
        /// <param name="behaviourInput"></param>
        /// <param name="behaviourOutput"></param>
        public static void SetIdleBehaviourOutput(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput)
        {
            // Desired heading is current forwards direction
            behaviourOutput.heading = behaviourInput.shipInstance.TransformForward;
            // Desired velocity is zero
            behaviourOutput.velocity = Vector3.zero;
            // Target is not set
            behaviourOutput.target = Vector3.zero;
            behaviourOutput.setTarget = true;
            // No desired upwards orientation (?)
            behaviourOutput.up = Vector3.zero;
            // Never use targeting accuracy
            behaviourOutput.useTargetingAccuracy = false;
        }

        /// <summary>
        /// Sets a behaviour output to a "seek" behaviour output.
        /// Required inputs: Target position, weighting.
        /// </summary>
        /// <param name="behaviourInput"></param>
        /// <param name="behaviourOutput"></param>
        public static void SetSeekBehaviourOutput(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput)
        {
            // Desired heading is towards the target position
            headingVectorNormalised = (behaviourInput.targetPosition - behaviourInput.shipInstance.TransformPosition).normalized;
            behaviourOutput.heading = headingVectorNormalised;
            // Desired velocity is max speed in direction of desired heading
            behaviourOutput.velocity = headingVectorNormalised * behaviourInput.shipAIInputModuleInstance.maxSpeed;
            // Target is the target position
            behaviourOutput.target = behaviourInput.targetPosition;
            behaviourOutput.setTarget = true;
            // No desired upwards orientation
            behaviourOutput.up = Vector3.zero;
            // Whether we use targeting accuracy depends on behaviour input settings
            behaviourOutput.useTargetingAccuracy = behaviourInput.useTargetingAccuracy;
        }

        /// <summary>
        /// Sets a behaviour output to a "flee" behaviour output.
        /// Required inputs: Target position, weighting.
        /// </summary>
        /// <param name="behaviourInput"></param>
        /// <param name="behaviourOutput"></param>
        public static void SetFleeBehaviourOutput(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput)
        {
            // Desired heading is away from the target position
            headingVectorNormalised = (behaviourInput.shipInstance.TransformPosition - behaviourInput.targetPosition).normalized;
            behaviourOutput.heading = headingVectorNormalised;
            // Desired velocity is max speed in direction of desired heading
            behaviourOutput.velocity = headingVectorNormalised * behaviourInput.shipAIInputModuleInstance.maxSpeed;
            // Target is not set
            behaviourOutput.target = Vector3.zero;
            behaviourOutput.setTarget = true;
            // No desired upwards orientation
            behaviourOutput.up = Vector3.zero;
            // Never use targeting accuracy
            behaviourOutput.useTargetingAccuracy = false;
        }

        /// <summary>
        /// Sets a behaviour output to a "pursuit" behaviour output.
        /// Required inputs: Target position, target velocity, weighting.
        /// </summary>
        /// <param name="behaviourInput"></param>
        /// <param name="behaviourOutput"></param>
        public static void SetPursuitBehaviourOutput(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput)
        {
            // Desired heading is towards the predicted target position
            //headingVectorNormalised = (behaviourInput.targetPosition + (behaviourInput.targetVelocity * PredictionIntervalTime(behaviourInput.targetPosition, behaviourInput.targetVelocity))
            //    - behaviourInput.shipInstance.TransformPosition).normalized;
            headingVectorNormalised = (behaviourInput.targetPosition + (behaviourInput.targetVelocity *
                ApproxInterceptionTime(behaviourInput.shipInstance.TransformPosition, behaviourInput.shipInstance.WorldVelocity,
                behaviourInput.targetPosition)) - behaviourInput.shipInstance.TransformPosition).normalized;
            behaviourOutput.heading = headingVectorNormalised;
            // Desired velocity is max speed in direction of desired heading
            behaviourOutput.velocity = headingVectorNormalised * behaviourInput.shipAIInputModuleInstance.maxSpeed;
            // Target is the target position
            behaviourOutput.target = behaviourInput.targetPosition;
            behaviourOutput.setTarget = true;
            // No desired upwards orientation
            behaviourOutput.up = Vector3.zero;
            // Whether we use targeting accuracy depends on behaviour input settings
            behaviourOutput.useTargetingAccuracy = behaviourInput.useTargetingAccuracy;
        }

        /// <summary>
        /// Sets a behaviour output to an "evasion" behaviour output.
        /// Required inputs: Target position, target velocity, weighting.
        /// </summary>
        /// <param name="behaviourInput"></param>
        /// <param name="behaviourOutput"></param>
        public static void SetEvasionBehaviourOutput(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput)
        {
            // Desired heading is away from the predicted target position
            headingVectorNormalised = (behaviourInput.shipInstance.TransformPosition - behaviourInput.targetPosition -
                (behaviourInput.targetVelocity * ApproxInterceptionTime(behaviourInput.shipInstance.TransformPosition,
                behaviourInput.shipInstance.WorldVelocity, behaviourInput.targetPosition))).normalized;
            behaviourOutput.heading = headingVectorNormalised;
            // Desired velocity is max speed in direction of desired heading
            behaviourOutput.velocity = headingVectorNormalised * behaviourInput.shipAIInputModuleInstance.maxSpeed;
            // Target is not set
            behaviourOutput.target = Vector3.zero;
            behaviourOutput.setTarget = true;
            // No desired upwards orientation
            behaviourOutput.up = Vector3.zero;
            // Never use targeting accuracy
            behaviourOutput.useTargetingAccuracy = false;
        }

        ///// <summary>
        ///// Sets a behaviour input to an "avoid" behaviour input for a fixed target position
        ///// </summary>
        ///// <param name="behaviourInput"></param>
        //public static void SetAvoidInputBehaviour(AIBehaviourInput behaviourInput)
        //{
        //    behaviourInput.velocityOutput = Vector3.zero;
        //    behaviourInput.weighting = 0f;
        //}

        ///// <summary>
        ///// Sets a behaviour input to an "follow" behaviour input for a moving target
        ///// Follow should probably store some kind of vector3 offset or distance at which to
        ///// follow the target.
        ///// </summary>
        ///// <param name="behaviourInput"></param>
        ///// <param name="targetPosition"></param>
        ///// <param name="targetVelocity"></param>
        ///// <param name="behaviourWeighting"></param>
        //public static void SetFollowInputBehaviour(AIBehaviourInput behaviourInput)
        //{
        //    behaviourInput.velocityOutput = Vector3.zero;
        //    behaviourInput.weighting = 0f;
        //}

        /// <summary>
        /// Sets a behaviour output to a "seek arrival" behaviour output for a fixed target position.
        /// Required inputs: Target position, weighting.
        /// </summary>
        /// <param name="behaviourInput"></param>
        /// <param name="behaviourOutput"></param>
        public static void SetSeekArrivalBehaviourOutput(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput)
        {
            // Desired heading is towards the target position
            headingVector = behaviourInput.targetPosition - behaviourInput.shipInstance.TransformPosition;
            headingVectorMagnitude = headingVector.magnitude;
            headingVectorNormalised = headingVector / headingVector.magnitude;
            behaviourOutput.heading = headingVectorNormalised;
            // Desired velocity is generally max speed in direction of desired heading, but decreases when nearing the target
            //behaviourInput.velocityOutput = headingVectorNormalised * (float)System.Math.Sqrt(2f * behaviourInput.shipAIInputModuleInstance.decelerationRate * headingVectorMagnitude);
            behaviourOutput.velocity = headingVectorNormalised * behaviourInput.shipAIInputModuleInstance.MaxSpeedFromBrakingDistance(0f, headingVectorMagnitude, behaviourInput.shipInstance.LocalVelocity.normalized);
            // Re-clamp velocity magnitude to max speed if needed
            if (behaviourOutput.velocity.sqrMagnitude > behaviourInput.shipAIInputModuleInstance.maxSpeed * behaviourInput.shipAIInputModuleInstance.maxSpeed)
            {
                behaviourOutput.velocity = headingVectorNormalised * behaviourInput.shipAIInputModuleInstance.maxSpeed;
            }
            // Target is the target position
            behaviourOutput.target = behaviourInput.targetPosition;
            behaviourOutput.setTarget = true;
            // No desired upwards orientation
            behaviourOutput.up = Vector3.zero;
            // Whether we use targeting accuracy depends on behaviour input settings
            behaviourOutput.useTargetingAccuracy = behaviourInput.useTargetingAccuracy;
        }

        /// <summary>
        /// Sets a behaviour output to a "seek arrival" behaviour output for a moving target position.
        /// Required inputs: Target position, target velocity, weighting.
        /// </summary>
        /// <param name="behaviourInput"></param>
        /// <param name="behaviourOutput"></param>
        public static void SetSeekMovingArrivalBehaviourOutput(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput)
        {
            // Desired heading is towards the target position
            headingVector = behaviourInput.targetPosition - behaviourInput.shipInstance.TransformPosition;
            headingVectorMagnitude = headingVector.magnitude;
            headingVectorNormalised = headingVector / headingVector.magnitude;
            behaviourOutput.heading = headingVectorNormalised;
            // Desired velocity is generally max speed in direction of desired heading, but decreases when nearing the target
            //behaviourInput.velocityOutput = behaviourInput.targetVelocity + (headingVectorNormalised * (float)System.Math.Sqrt(2f * behaviourInput.shipAIInputModuleInstance.decelerationRate * headingVectorMagnitude));
            // Braking distance code

            // TODO probably convert > to >= (but only when algorithm is fixed)

            // Check whether target velocity and heading are in the same direction
            float targetDotHeading = Vector3.Dot(behaviourInput.targetVelocity, headingVectorNormalised);
            if (targetDotHeading > 0f)
            {
                // Target velocity and heading are in the same direction
                // TODO optimise code
                // Split target velocity into two components - that in the direction of the heading and whatever is left over
                Vector3 targetVelocityHeadingComponent = Vector3.Project(behaviourInput.targetVelocity, headingVectorNormalised);
                Vector3 targetVelocityOtherComponent = behaviourInput.targetVelocity - targetVelocityHeadingComponent;
                // Use braking distance code to determine required velocity in direction of heading, then add the
                // the other component of the target velocity to it
                // TODO what if heading vector is in opposite direction to target velocity?
                behaviourOutput.velocity = targetVelocityOtherComponent + (headingVectorNormalised * behaviourInput.shipAIInputModuleInstance.MaxSpeedFromBrakingDistance(targetVelocityHeadingComponent.magnitude, headingVectorMagnitude, behaviourInput.shipInstance.LocalVelocity.normalized));
            }
            else
            {
                // Target velocity and heading are in opposite directions
                // TODO optimise code
                // Split target velocity into two components - that in the direction of the heading and whatever is left over
                Vector3 targetVelocityHeadingComponent = Vector3.Project(behaviourInput.targetVelocity, headingVectorNormalised);
                Vector3 targetVelocityOtherComponent = behaviourInput.targetVelocity - targetVelocityHeadingComponent;
                // Use braking distance code to determine required velocity in direction of heading, then add the
                // the other component of the target velocity to it
                //behaviourOutput.velocity = targetVelocityOtherComponent;
                behaviourOutput.velocity = targetVelocityOtherComponent + (headingVectorNormalised * behaviourInput.shipAIInputModuleInstance.MaxSpeedFromBrakingDistance(targetVelocityHeadingComponent.magnitude, headingVectorMagnitude, behaviourInput.shipInstance.LocalVelocity.normalized));
            }

            // Calculate max speed the ship can travel at while still making the turn
            float maxAllowedSpeed = CalculateMaxTurnSpeed(behaviourInput.targetPosition, behaviourInput.targetVelocity, behaviourInput);
            // Clamp to ship AI max speed
            if (maxAllowedSpeed > behaviourInput.shipAIInputModuleInstance.maxSpeed) { maxAllowedSpeed = behaviourInput.shipAIInputModuleInstance.maxSpeed; }

            // Clamp velocity magnitude to max allowed speed
            if (behaviourOutput.velocity.sqrMagnitude > maxAllowedSpeed * maxAllowedSpeed)
            {
                behaviourOutput.velocity = headingVectorNormalised * maxAllowedSpeed;
            }

            // Target is the target position
            behaviourOutput.target = behaviourInput.targetPosition;
            behaviourOutput.setTarget = true;
            // No desired upwards orientation
            behaviourOutput.up = Vector3.zero;
            // Whether we use targeting accuracy depends on behaviour input settings
            behaviourOutput.useTargetingAccuracy = behaviourInput.useTargetingAccuracy;
        }

        /// <summary>
        /// Sets a behaviour output to a "pursuit arrival" behaviour output.
        /// Required inputs: Target position, target velocity, weighting.
        /// </summary>
        /// <param name="behaviourInput"></param>
        /// <param name="behaviourOutput"></param>
        public static void SetPursuitArrivalBehaviourOutput(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput)
        {
            // Target is the predicted target position
            behaviourOutput.target = behaviourInput.targetPosition + (behaviourInput.targetVelocity * ApproxInterceptionTime(behaviourInput.shipInstance.TransformPosition,
                behaviourInput.shipInstance.WorldVelocity, behaviourInput.targetPosition));
            behaviourOutput.setTarget = true;
            // Desired heading is towards the predicted target position
            headingVector = behaviourOutput.target - behaviourInput.shipInstance.TransformPosition;
            headingVectorMagnitude = headingVector.magnitude;
            headingVectorNormalised = headingVector / headingVector.magnitude;
            behaviourOutput.heading = headingVectorNormalised;
            // Desired velocity is generally max speed in direction of desired heading, but decreases when nearing the target
            // For "pursue arrival" behaviour, "zero" velocity is the target's velocity
            //behaviourInput.velocityOutput = behaviourInput.targetVelocity + (headingVectorNormalised * (float)System.Math.Sqrt(2f * behaviourInput.shipAIInputModuleInstance.decelerationRate * headingVectorMagnitude));
            // Braking distance code

            // Check whether target velocity and heading are in the same direction
            float targetDotHeading = Vector3.Dot(behaviourInput.targetVelocity, headingVectorNormalised);
            if (targetDotHeading > 0f)
            {
                // Target velocity and heading are in the same direction
                // TODO optimise code

                // Split target velocity into two components - that in the direction of the heading and whatever is left over
                Vector3 targetVelocityHeadingComponent = Vector3.Project(behaviourInput.targetVelocity, headingVectorNormalised);
                Vector3 targetVelocityOtherComponent = behaviourInput.targetVelocity - targetVelocityHeadingComponent;
                // Use braking distance code to determine required velocity in direction of heading, then add the
                // the other component of the target velocity to it
                // TODO what if heading vector is in opposite direction to target velocity?
                // MaxSpeedFromBraking(..) can return zero which results in velocityOutput being Vector3.Zero
                behaviourOutput.velocity = targetVelocityOtherComponent + (headingVectorNormalised * behaviourInput.shipAIInputModuleInstance.MaxSpeedFromBrakingDistance(targetVelocityHeadingComponent.magnitude, headingVectorMagnitude, behaviourInput.shipInstance.LocalVelocity.normalized));
            }
            else
            {
                // Target velocity and heading are in opposite directions
                // TODO optimise code

                // Split target velocity into two components - that in the direction of the heading and whatever is left over
                Vector3 targetVelocityHeadingComponent = Vector3.Project(behaviourInput.targetVelocity, headingVectorNormalised);
                Vector3 targetVelocityOtherComponent = behaviourInput.targetVelocity - targetVelocityHeadingComponent;
                // Use braking distance code to determine required velocity in direction of heading, then add the
                // the other component of the target velocity to it
                // TODO what if heading vector is in opposite direction to target velocity?
                // MaxSpeedFromBraking(..) can return zero which results in velocityOutput being Vector3.Zero
                //behaviourOutput.velocity = targetVelocityOtherComponent;
                behaviourOutput.velocity = targetVelocityOtherComponent + (headingVectorNormalised * behaviourInput.shipAIInputModuleInstance.MaxSpeedFromBrakingDistance(targetVelocityHeadingComponent.magnitude, headingVectorMagnitude, behaviourInput.shipInstance.LocalVelocity.normalized));
            }

            // Calculate max speed the ship can travel at while still making the turn
            float maxAllowedSpeed = CalculateMaxTurnSpeed(behaviourInput.targetPosition, behaviourInput.targetVelocity, behaviourInput);
            // Clamp to ship AI max speed
            if (maxAllowedSpeed > behaviourInput.shipAIInputModuleInstance.maxSpeed) { maxAllowedSpeed = behaviourInput.shipAIInputModuleInstance.maxSpeed; }

            // Clamp velocity magnitude to max allowed speed
            if (behaviourOutput.velocity.sqrMagnitude > maxAllowedSpeed * maxAllowedSpeed)
            {
                behaviourOutput.velocity = headingVectorNormalised * maxAllowedSpeed;
            }

            // No desired upwards orientation
            behaviourOutput.up = Vector3.zero;
            // Whether we use targeting accuracy depends on behaviour input settings
            behaviourOutput.useTargetingAccuracy = behaviourInput.useTargetingAccuracy;
        }

        /// <summary>
        /// Sets a behaviour output to a "unblock cylinder" behaviour output.
        /// Required inputs: Target position, target forwards, target radius, weighting.
        /// </summary>
        /// <param name="behaviourInput"></param>
        /// <param name="behaviourOutput"></param>
        public static void SetUnblockCylinderBehaviourOutput(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput)
        {
            // First check whether we are within the "blocking" region
            // This region is defined as being within a cylinder of targetRadius along the targetForwards direction from targetPosition
            float fwdProjectionAmount = Vector3.Dot(behaviourInput.shipInstance.TransformPosition - behaviourInput.targetPosition, behaviourInput.targetForwards);
            if (fwdProjectionAmount > 0f)
            {
                Vector3 fwdProjection = fwdProjectionAmount * behaviourInput.targetForwards;
                // Heading vector is from closest point on centre of cylinder to current position
                headingVector = behaviourInput.shipInstance.TransformPosition - (behaviourInput.targetPosition + fwdProjection);
                headingVectorSqrMagnitude = headingVector.sqrMagnitude;
                if (headingVectorSqrMagnitude < behaviourInput.targetRadius * behaviourInput.targetRadius)
                {
                    // If we are within the cylinder, set an output to vacate the region
                    behaviourOutput.heading = headingVector / (float)System.Math.Sqrt(headingVectorSqrMagnitude);
                    behaviourOutput.velocity = behaviourOutput.heading * behaviourInput.shipAIInputModuleInstance.maxSpeed;

                    float exitAngle = 30f * Mathf.Deg2Rad;

                    headingVectorNormalised = (behaviourOutput.heading * Mathf.Cos(exitAngle)) + (behaviourInput.targetForwards * Mathf.Sin(exitAngle));
                    behaviourOutput.heading = headingVectorNormalised;
                }
                else
                {
                    // If we are not within the cylinder, no output is needed for this behaviour
                    behaviourOutput.heading = Vector3.zero;
                    behaviourOutput.velocity = Vector3.zero;
                }
            }
            else
            {
                // If we are not within the cone, no output is needed for this behaviour
                behaviourOutput.heading = Vector3.zero;
                behaviourOutput.velocity = Vector3.zero;
            }
            // Target is not set
            behaviourOutput.target = Vector3.zero;
            behaviourOutput.setTarget = true;
            // No desired upwards orientation
            behaviourOutput.up = Vector3.zero;
            // Never use targeting accuracy
            behaviourOutput.useTargetingAccuracy = false;
        }

        /// <summary>
        /// Sets a behaviour output to a "unblock cone" behaviour output.
        /// Required inputs: Target position, target forwards, target FOV angle, weighting.
        /// </summary>
        /// <param name="behaviourInput"></param>
        /// <param name="behaviourOutput"></param>
        public static void SetUnblockConeBehaviourOutput(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput)
        {
            // First check whether we are within the "blocking" region
            // This region is defined as being within a cone of targetFOVAngle along the targetForwards direction from targetPosition
            float fwdProjectionAmount = Vector3.Dot(behaviourInput.shipInstance.TransformPosition - behaviourInput.targetPosition, behaviourInput.targetForwards);
            if (fwdProjectionAmount > 0f)
            {
                Vector3 fwdProjection = fwdProjectionAmount * behaviourInput.targetForwards;
                // Heading vector is from closest point on centre of cylinder to current position
                headingVector = behaviourInput.shipInstance.TransformPosition - (behaviourInput.targetPosition + fwdProjection);
                headingVectorMagnitude = headingVector.magnitude;
                if ((float)System.Math.Atan(headingVectorMagnitude / fwdProjection.magnitude) * Mathf.Rad2Deg < behaviourInput.targetFOVAngle)
                {
                    // If we are within the cone, set an output to vacate the region
                    behaviourOutput.heading = headingVector / headingVectorMagnitude;
                    //behaviourInput.velocityOutput = behaviourInput.headingOutput * behaviourInput.shipAIInputModuleInstance.maxSpeed;
                    behaviourOutput.velocity = behaviourInput.shipInstance.WorldVelocity.normalized * behaviourInput.shipAIInputModuleInstance.maxSpeed;

                    float exitAngle = (behaviourInput.targetFOVAngle + 30f) * Mathf.Deg2Rad;

                    headingVectorNormalised = (behaviourOutput.heading * Mathf.Cos(exitAngle)) + (behaviourInput.targetForwards * Mathf.Sin(exitAngle));
                    behaviourOutput.heading = headingVectorNormalised;
                }
                else
                {
                    // If we are not within the cone, no output is needed for this behaviour
                    behaviourOutput.heading = Vector3.zero;
                    behaviourOutput.velocity = Vector3.zero;
                }
            }
            else
            {
                // If we are not within the cone, no output is needed for this behaviour
                behaviourOutput.heading = Vector3.zero;
                behaviourOutput.velocity = Vector3.zero;
            }
            // Target is not set
            behaviourOutput.target = Vector3.zero;
            behaviourOutput.setTarget = true;
            // No desired upwards orientation
            behaviourOutput.up = Vector3.zero;
            // Never use targeting accuracy
            behaviourOutput.useTargetingAccuracy = false;
        }

        /// <summary>
        /// Sets a behaviour output to a "obstacle avoidance" behaviour output.
        /// Required inputs: Weighting.
        /// </summary>
        /// <param name="behaviourInput"></param>
        /// <param name="behaviourOutput"></param>
        public static void SetObstacleAvoidanceBehaviourOutput(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput)
        {
            #region Step 1: Preliminary Checks

            // Sweep variables - these are used since we will be changing the sweep type based on obstacle avoidance quality
            // Whether the sweep hit anything
            bool sweepDidHit = false;
            // The type of sweep:
            // 0 - No sweep, 1 - raycast, 2 - spherecast, 3 - rigidbody sweep test
            // NOTE: Rigidbody sweep test is not used if the obstacle layers are not everything 
            // or the default raycast layers (everything but the ignore raycast layer)
            int sweepType = 0;

            // Choose which type of sweep test to perform based on quality
            switch (behaviourInput.shipAIInputModuleInstance.obstacleAvoidanceQuality)
            {
                case ShipAIInputModule.AIObstacleAvoidanceQuality.Off:
                    // No obstacle avoidance - don't perform a sweep
                    sweepType = 0;
                    break;
                case ShipAIInputModule.AIObstacleAvoidanceQuality.Low:
                    // Low quality - use a spherecast
                    sweepType = 2;
                    break;
                case ShipAIInputModule.AIObstacleAvoidanceQuality.Medium:
                    // Medium quality - use a spherecast
                    sweepType = 2;
                    break;
                case ShipAIInputModule.AIObstacleAvoidanceQuality.High:
                    // High quality - use a rigidbody sweep test (unless we are using a custom layer selection)
                    if (behaviourInput.shipAIInputModuleInstance.obstacleLayerMask == Physics.AllLayers ||
                    behaviourInput.shipAIInputModuleInstance.obstacleLayerMask == Physics.DefaultRaycastLayers)
                    {
                        sweepType = 3;
                    }
                    // If we are using a custom layer selection, revert to using a spherecast
                    else { sweepType = 2; }
                    break;
                default:
                    // Default to a spherecast
                    sweepType = 2;
                    break;
            }

            // Calculate the position of the centre of the ship, shifted forward by the specified amount
            // This is to avoid issues where colliders near the front of the ship can interfere with obstacle avoidance
            Vector3 shiftedShipCentrePosition = behaviourInput.shipInstance.RigidbodyPosition +
                (behaviourInput.shipInstance.RigidbodyForward * behaviourInput.shipAIInputModuleInstance.raycastStartOffsetZ);

            // Find the last target position
            Vector3 worldSpaceTargetPosition = behaviourInput.shipAIInputModuleInstance.GetLastBehaviourInputTarget();            

            // If the last target position is actually set (i.e. it isn't Vector3.zero), check if there is an obstacle
            // between us and the target position
            if (worldSpaceTargetPosition.sqrMagnitude > 0.01 || sweepType == 0)
            {
                // Set sweep test origin and direction
                OARay.direction = (worldSpaceTargetPosition - behaviourInput.shipInstance.RigidbodyPosition);
                OARay.origin = behaviourInput.shipInstance.RigidbodyPosition +
                    (OARay.direction * behaviourInput.shipAIInputModuleInstance.raycastStartOffsetZ);
                float distanceToCurrentTarget = (worldSpaceTargetPosition - OARay.origin).magnitude;
                OARay.direction /= distanceToCurrentTarget;

                sweepDidHit = PerformSweep(behaviourInput, sweepType, OARay, ref OARaycastHit, distanceToCurrentTarget);

                // DEBUGGING
                //if (!sweepDidHit) { Debug.DrawRay(OARay.origin, OARay.direction * distanceToCurrentTarget, Color.green); }
                //else { Debug.DrawRay(OARay.origin, OARay.direction * distanceToCurrentTarget, Color.red); }

                // If the sweep from the ship to the target position did not hit, assume that we do not
                // need to take corrective action (i.e. that no obstacle avoidance is required)
                if (!sweepDidHit)
                {
                    // Set the behaviour output to a "null" output
                    behaviourOutput.heading = Vector3.zero;
                    behaviourOutput.up = Vector3.zero;
                    behaviourOutput.velocity = Vector3.zero;
                    behaviourOutput.target = Vector3.zero;
                    behaviourOutput.setTarget = false;
                    return;
                }
            }

            #endregion

            #region Step 2: Precalculate Obstacle Avoidance Information

            // TODO this should be determined by ship maneuverability
            float OALookAheadTime = 4f;
            //float OALookAheadTime = behaviourInput.shipAIInputModuleInstance.ObstacleAvoidanceLookAheadTime();

            // Get the current speed of the ship in the forwards direction
            float currentSpeed = behaviourInput.shipInstance.LocalVelocity.z > 0f ? behaviourInput.shipInstance.LocalVelocity.z : 0f;
            // Calculate how far we should look ahead
            // This is the maximum of OALookAheadTime seconds ahead and ship radius * 5
            float OALookAheadDistance = currentSpeed * OALookAheadTime;
            if (OALookAheadDistance < behaviourInput.shipAIInputModuleInstance.shipRadius * 5f) { OALookAheadDistance = behaviourInput.shipAIInputModuleInstance.shipRadius * 5f; }
            float distToObstacle = 0f;

            #endregion

            #region Step 3: Determine If Action Is Required

            // Determine if we need to take action to avoid an obstacle
            // There are three instances in which this could occur:
            // a) There is an obstacle blocking the "forwards" direction of the ship
            // b) There is an obstacle blocking the future "forwards" direction of the ship
            // c) [NOT USED CURRENTLY] There is an obstacle blocking the "velocity" direction of the ship

            bool forwardsDirectionBlocked = false;
            bool futureForwardsDirectionBlocked = false;
            
            // First check the "forwards" direction

            // Set sweep test origin and direction
            OARay.origin = shiftedShipCentrePosition;
            OARay.direction = behaviourInput.shipInstance.RigidbodyForward;
            sweepDidHit = PerformSweep(behaviourInput, sweepType, OARay, ref OARaycastHit, OALookAheadDistance);

            if (sweepDidHit)
            {
                OARaycastHitRigidbody = OARaycastHit.rigidbody;
                if (OARaycastHitRigidbody != null)
                {
                    // If the object hit was a rigidbody, we need to check if we are on a collision course with it
                    forwardsDirectionBlocked = OnCollisionCourse(shiftedShipCentrePosition,
                        behaviourInput.shipInstance.WorldVelocity, behaviourInput.shipAIInputModuleInstance.shipRadius, OARaycastHitRigidbody.position,
                        OARaycastHitRigidbody.velocity, behaviourInput.shipAIInputModuleInstance.shipRadius, OALookAheadTime);
                }
                else { forwardsDirectionBlocked = true; distToObstacle = OARaycastHit.distance; }
            }

            // DEBUGGING (TODO REMOVE)
            //if (forwardsDirectionBlocked) { Debug.DrawRay(OARay.origin, OARay.direction * OALookAheadDistance, Color.red); }
            //else { Debug.DrawRay(OARay.origin, OARay.direction * OALookAheadDistance, Color.green); }

            // If the "forwards" direction is not blocked, check the future "forwards" direction
            if (!forwardsDirectionBlocked)
            {
                // Set up a ray to check the future "forwards" direction of the ship
                // i.e. the "forwards" direction that will be reached in the future assuming
                // we maintain a constant angular velocity

                // Set sweep test origin and direction
                float angularVeloLookAheadTime = OALookAheadTime * 0.25f * Mathf.Rad2Deg;
                OARay.origin = shiftedShipCentrePosition;
                OARay.direction = Quaternion.Euler(behaviourInput.shipInstance.WorldAngularVelocity * angularVeloLookAheadTime) * behaviourInput.shipInstance.RigidbodyForward;
                sweepDidHit = PerformSweep(behaviourInput, sweepType, OARay, ref OARaycastHit, OALookAheadDistance);

                if (sweepDidHit)
                {
                    OARaycastHitRigidbody = OARaycastHit.rigidbody;
                    if (OARaycastHitRigidbody != null)
                    {
                        // If the object hit was a rigidbody, we need to check if we are on a collision course with it
                        futureForwardsDirectionBlocked = OnCollisionCourse(shiftedShipCentrePosition,
                            behaviourInput.shipInstance.WorldVelocity, behaviourInput.shipAIInputModuleInstance.shipRadius, OARaycastHitRigidbody.position,
                            OARaycastHitRigidbody.velocity, behaviourInput.shipAIInputModuleInstance.shipRadius, OALookAheadTime);
                    }
                    else { futureForwardsDirectionBlocked = true; distToObstacle = OARaycastHit.distance; }
                }

                // DEBUGGING
                //if (futureForwardsDirectionBlocked) { Debug.DrawRay(OARay.origin, OARay.direction * OALookAheadDistance, Color.red); }
                //else { Debug.DrawRay(OARay.origin, OARay.direction * OALookAheadDistance, Color.green); }
            }

            #endregion

            #region Step 4: Find Valid Path

            // If step 3 determined that we need to take action to avoid an obstacle,
            // examine other possible directions we could move in to avoid the obstacle. Then
            // determine what ship input we need to move in the chosen direction. Otherwise if
            // step 3 did not determine that we need to take action, set the input of this behaviour
            // to a "null" input.

            // TODO IMPROVEMENTS:
            // - "Whisker" method - move away from obstacles on each side
            // - Use improved braking/cornering algoriths

            // Check if either the forwards direction or the future forwards direction are blocked
            if (forwardsDirectionBlocked || futureForwardsDirectionBlocked)
            {
                // Search for an alternative route
                bool foundViableRoute = false;
                float raycastAngle = 10f;
                OARay.origin = shiftedShipCentrePosition;

                // Check if the forwards direction of the ship is not blocked
                // (if it isn't blocked we can use that)
                if (!forwardsDirectionBlocked)
                {
                    behaviourOutput.heading = behaviourInput.shipInstance.RigidbodyForward;
                    behaviourOutput.velocity = behaviourInput.shipInstance.RigidbodyForward * behaviourInput.shipAIInputModuleInstance.maxSpeed;
                    foundViableRoute = true;
                }

                // TODO: Should use pitch/yaw/roll acceleration instead of 100f
                // TODO: Should probably use newly derived curve speed calculations to calculate speeds
                float averageAngularVelocity = 100f * Mathf.Deg2Rad * distToObstacle / currentSpeed * 0.5f;

                // Calculate an order for raycasting based on the passed in target position
                // Directions closer to the target position are evaluated first
                int numberOfAvailableDirections = 4;
                Vector3[] XYPlaneRayDirections = new Vector3[4];
                Vector3 localSpaceTargetPosition = behaviourInput.shipInstance.RigidbodyInverseRotation * worldSpaceTargetPosition;
                // Is the target position more towards the right than the left?
                bool localTargetXPositive = localSpaceTargetPosition.x >= 0f;
                // Is the target position more towards up than down?
                bool localTargetYPositive = localSpaceTargetPosition.y >= 0f;
                // If the ship is grounded, we can only (reliably) use left and right directions to avoid obstacles
                if (behaviourInput.shipInstance.IsGrounded)
                {
                    // Simply order the X-directions if we are grounded
                    XYPlaneRayDirections[0] = behaviourInput.shipInstance.RigidbodyRight * (localTargetXPositive ? 1f : -1f);
                    XYPlaneRayDirections[1] = behaviourInput.shipInstance.RigidbodyRight * (localTargetXPositive ? -1f : 1f);
                    // Two directions available for obstacle avoidance
                    numberOfAvailableDirections = 2;
                }
                // Otherwise if the ship is not grounded, we can use all four directions
                // Is the target position more along the x-axis than the y-axis...
                else if ((localTargetXPositive ? localSpaceTargetPosition.x : -localSpaceTargetPosition.x) >
                    (localTargetYPositive ? localSpaceTargetPosition.y : -localSpaceTargetPosition.y))
                {
                    // Local space target position is more along x-axis than y-axis
                    // X-directions are first and last
                    XYPlaneRayDirections[0] = behaviourInput.shipInstance.RigidbodyRight * (localTargetXPositive ? 1f : -1f);
                    XYPlaneRayDirections[3] = behaviourInput.shipInstance.RigidbodyRight * (localTargetXPositive ? -1f : 1f);
                    // Y-directions are second and third
                    XYPlaneRayDirections[1] = behaviourInput.shipInstance.RigidbodyUp * (localTargetYPositive ? 1f : -1f);
                    XYPlaneRayDirections[2] = behaviourInput.shipInstance.RigidbodyUp * (localTargetYPositive ? -1f : 1f);
                    // Four directions available for obstacle avoidance
                    numberOfAvailableDirections = 4;
                }
                // ... or is the target position more along the y-axis than the x-axis?
                else
                {
                    // Local space target position is more along y-axis than x-axis
                    // Y-directions are first and last
                    XYPlaneRayDirections[0] = behaviourInput.shipInstance.RigidbodyUp * (localTargetYPositive ? 1f : -1f);
                    XYPlaneRayDirections[3] = behaviourInput.shipInstance.RigidbodyUp * (localTargetYPositive ? -1f : 1f);
                    // X-directions are second and third
                    XYPlaneRayDirections[1] = behaviourInput.shipInstance.RigidbodyRight * (localTargetXPositive ? 1f : -1f);
                    XYPlaneRayDirections[2] = behaviourInput.shipInstance.RigidbodyRight * (localTargetXPositive ? -1f : 1f);
                    // Four directions available for obstacle avoidance
                    numberOfAvailableDirections = 4;
                }

                float sineRaycastAngle, cosineRaycastAngle, maxTurnVelocity, neededHorizontalDistance;

                // Loop through increasing raycast angles in order to find a viable route
                while (!foundViableRoute)
                {
                    // Take the sine and cosine of the raycast angle to use for calculating the direction vector of the sweep
                    sineRaycastAngle = Mathf.Sin(raycastAngle * Mathf.Deg2Rad);
                    cosineRaycastAngle = Mathf.Cos(raycastAngle * Mathf.Deg2Rad);

                    if (raycastAngle < 89f)
                    {
                        // TODO: Should also check the flight acceleration etc.
                        neededHorizontalDistance = distToObstacle * (sineRaycastAngle / cosineRaycastAngle);
                        maxTurnVelocity = averageAngularVelocity * (neededHorizontalDistance * neededHorizontalDistance + distToObstacle * distToObstacle) / (2f * neededHorizontalDistance);
                    }
                    else
                    {
                        // TODO maybe have some sort of min speed?
                        maxTurnVelocity = 10f;
                    }

                    if (maxTurnVelocity > behaviourInput.shipAIInputModuleInstance.maxSpeed || float.IsNaN(maxTurnVelocity) || 
                        float.IsInfinity(maxTurnVelocity)) { maxTurnVelocity = behaviourInput.shipAIInputModuleInstance.maxSpeed; }

                    // Choose which type of sweep test to perform based on quality
                    switch (behaviourInput.shipAIInputModuleInstance.obstacleAvoidanceQuality)
                    {
                        case ShipAIInputModule.AIObstacleAvoidanceQuality.Off:
                            // No obstacle avoidance - don't perform a sweep
                            sweepType = 0;
                            break;
                        case ShipAIInputModule.AIObstacleAvoidanceQuality.Low:
                            // Low quality - use a raycast
                            sweepType = 1;
                            break;
                        case ShipAIInputModule.AIObstacleAvoidanceQuality.Medium:
                            // Medium quality - use a raycast
                            sweepType = 1;
                            break;
                        case ShipAIInputModule.AIObstacleAvoidanceQuality.High:
                            // High quality - use a spherecast
                            sweepType = 2;
                            break;
                        default:
                            // Default to a raycast
                            sweepType = 1;
                            break;
                    }

                    // Loop through the raycast directions in order of directions closest to furthest from the target position
                    for (int i = 0; i < numberOfAvailableDirections; i++)
                    {
                        // Set raycast direction
                        OARay.direction = (behaviourInput.shipInstance.RigidbodyForward * cosineRaycastAngle) +
                            (XYPlaneRayDirections[i] * sineRaycastAngle);

                        // Set sweep test origin and direction
                        OARay.origin = shiftedShipCentrePosition;
                        OARay.direction = (behaviourInput.shipInstance.RigidbodyForward * cosineRaycastAngle) +
                            (XYPlaneRayDirections[i] * sineRaycastAngle);
                        sweepDidHit = PerformSweep(behaviourInput, sweepType, OARay, ref OARaycastHit, OALookAheadDistance);

                        if (!sweepDidHit)
                        {
                            // The sweep did not return a hit, hence we should go in this direction
                            //Debug.DrawRay(OARay.origin, OARay.direction * OALookAheadDistance, Color.green);
                            // Heading is in direction of the ray we cast, 
                            // velocity is calculated turn velocity in direction of heading
                            behaviourOutput.heading = OARay.direction;
                            behaviourOutput.velocity = OARay.direction * maxTurnVelocity;
                            //if (givenHorizontalDistance > neededHorizontalDistance)
                            //{
                            //    behaviourInput.velocity = OARay.direction * behaviourInput.shipAIInputModuleInstance.maxSpeed;
                            //}
                            //else
                            //{
                            //    //behaviourInput.velocity = XYPlaneRayDirections[i] * behaviourInput.shipAIInputModuleInstance.maxSpeed;
                            //}
                            // We have found a viable route, so break out of the loop
                            foundViableRoute = true; break;
                        }
                        //else { Debug.DrawRay(OARay.origin, OARay.direction * OALookAheadDistance, Color.red); }
                    }

                    // Increment the raycast angle
                    raycastAngle += 20f;
                    if (raycastAngle > 90.1f)
                    {
                        behaviourOutput.heading = behaviourInput.shipInstance.RigidbodyForward * -1f;
                        behaviourOutput.velocity = behaviourInput.shipInstance.RigidbodyForward * -behaviourInput.shipAIInputModuleInstance.maxSpeed;
                        break;
                    }
                }

                // Up and target are not set
                behaviourOutput.up = Vector3.zero;
                behaviourOutput.target = Vector3.zero;
                behaviourOutput.setTarget = false;
            }
            else
            {
                // If it was determined that no action is required, set the behaviour output to a "null" output
                behaviourOutput.heading = Vector3.zero;
                behaviourOutput.up = Vector3.zero;
                behaviourOutput.velocity = Vector3.zero;
                behaviourOutput.target = Vector3.zero;
                behaviourOutput.setTarget = false;
            }

            // Never use targeting accuracy
            behaviourOutput.useTargetingAccuracy = false;

            #endregion
        }

        /// <summary>
        /// Sets a behaviour output to a "follow path" behaviour output.
        /// Required inputs: Target path, weighting.
        /// </summary>
        /// <param name="behaviourInput"></param>
        /// <param name="behaviourOutput"></param>
        public static void SetFollowPathBehaviourOutput(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput)
        {
            // Check that location data list is valid
            if (behaviourInput.targetPath != null && behaviourInput.targetPath.pathLocationDataList != null)
            {
                // TODO do something about non-closed circuits
                // TODO check that path functions are successful i.e. return true. If they do not return true,
                // do something...

                // Get the current target path index
                int currentTargetPathIndex = behaviourInput.shipAIInputModuleInstance.GetCurrentTargetPathLocationIndex();

                // TODO: if the current index is -1, try and find the closest path point
                if (currentTargetPathIndex < 0) { currentTargetPathIndex = SSCManager.GetNextPathLocationIndex(behaviourInput.targetPath, -1, true); }
                // Try and find the current path point
                int targetPathLocationCount = behaviourInput.targetPath.pathLocationDataList.Count;
                if (currentTargetPathIndex < 0 || targetPathLocationCount < 2)
                {
                    // Default to facing forward and stopping if no valid Path Locations
                    behaviourOutput.heading = Vector3.forward;
                    behaviourOutput.up = Vector3.zero;
                    behaviourOutput.velocity = Vector3.zero;
                    behaviourOutput.target = Vector3.zero;
                    behaviourOutput.setTarget = true;
                }
                else
                {
                    // Get the indices of the first and last path locations
                    int firstTargetPathLocationIdx = SSCManager.GetFirstAssignedLocationIdx(behaviourInput.targetPath);
                    int lastTargetPathLocationIdx = SSCManager.GetLastAssignedLocationIdx(behaviourInput.targetPath);

                    // Validate Path Data
                    if (firstTargetPathLocationIdx == lastTargetPathLocationIdx || firstTargetPathLocationIdx < 0 || lastTargetPathLocationIdx < 0)
                    {
                        // Default to facing forward and stopping if no valid Path Locations
                        behaviourOutput.heading = Vector3.forward;
                        behaviourOutput.up = Vector3.zero;
                        behaviourOutput.velocity = Vector3.zero;
                        behaviourOutput.target = Vector3.zero;
                        behaviourOutput.setTarget = true;
                    }
                    else
                    {
                        #region Set Up Quality Settings

                        // The (exact) number of iterations used in the future speed look ahead loop
                        int speedLookAheadIterations = 5;
                        // The (approximate) number of iterations used in the GetFurtherPointOnPathData function
                        int furtherPathPointCalculationIterations = 5;

                        switch (behaviourInput.shipAIInputModuleInstance.pathFollowingQuality)
                        {
                            // Low quality
                            case ShipAIInputModule.AIPathFollowingQuality.VeryLow:
                                speedLookAheadIterations = 1;
                                furtherPathPointCalculationIterations = 1;
                                break;
                            case ShipAIInputModule.AIPathFollowingQuality.Low:
                                speedLookAheadIterations = 3;
                                furtherPathPointCalculationIterations = 3;
                                break;
                            // Medium quality
                            case ShipAIInputModule.AIPathFollowingQuality.Medium:
                                speedLookAheadIterations = 5;
                                furtherPathPointCalculationIterations = 4;
                                break;
                            // High quality
                            case ShipAIInputModule.AIPathFollowingQuality.High:
                                speedLookAheadIterations = 10;
                                furtherPathPointCalculationIterations = 5;
                                break;
                        }

                        #endregion

                        #region Update Current Waypoint

                        // If this path is not a closed circuit, the current target path location index is not allowed
                        // to be the first assigned location index (only the previous index can refer to the first point)
                        if (!behaviourInput.targetPath.isClosedCircuit && currentTargetPathIndex == firstTargetPathLocationIdx)
                        {
                            // If it is the first index, set it to the next one
                            currentTargetPathIndex = SSCManager.GetNextPathLocationIndex(behaviourInput.targetPath, currentTargetPathIndex, true);
                        }

                        // Check if we have gone "past" the current path point i.e. crossed the plane of its tangent
                        Vector3 pathTangent = Vector3.zero;
                        SSCMath.GetPathTangent(behaviourInput.targetPath, currentTargetPathIndex, 0f, ref pathTangent);

                        if (Vector3.Dot(behaviourInput.shipInstance.TransformPosition -
                            behaviourInput.targetPath.pathLocationDataList[currentTargetPathIndex].locationData.position, pathTangent) > 0f)
                        {
                            // Don't try and go to the next point if this is not a closed circuit and this point is the last point
                            if (!behaviourInput.targetPath.isClosedCircuit && currentTargetPathIndex == lastTargetPathLocationIdx)
                            {
                                // Instead set the state action as completed
                                behaviourInput.shipAIInputModuleInstance.SetHasCompletedStateAction(true);
                            }
                            else
                            {
                                // Increment the target path index (Get next valid Path Location)
                                int nextTargetPathIndex = SSCManager.GetNextPathLocationIndex(behaviourInput.targetPath, currentTargetPathIndex,
                                    behaviourInput.targetPath.isClosedCircuit);
                                // If we didn't find another Location, keep same target. Not sure what else to do...
                                if (nextTargetPathIndex >= 0)
                                {
                                    currentTargetPathIndex = nextTargetPathIndex;
                                }
                            }
                        }
                        // Set the new target path index
                        behaviourInput.shipAIInputModuleInstance.SetCurrentTargetPathLocationIndex(currentTargetPathIndex);

                        #endregion

                        // Get the previous path index
                        int lastTargetPathIndex = SSCManager.GetPreviousPathLocationIndex(behaviourInput.targetPath, currentTargetPathIndex, true);

                        // TODO eventually will need to get from path data
                        float pathRadius = 5f;

                        #region Find Closest Point On Path

                        Vector3 closestPointOnPath = Vector3.zero;
                        float closestPointOnPathTValue = 0f;
                        Vector3 closestPointOnPathTangent = Vector3.zero;
                        Vector3 closestPointOnPathNormal = Vector3.zero;
                        Vector3 closestPointOnPathBinormal = Vector3.zero;
                        float closestPointOnPathCurvature = 0f;
                        // Find the position and t-value of the closest point on the path
                        SSCMath.FindClosestPointOnPath(behaviourInput.targetPath, lastTargetPathIndex,
                            behaviourInput.shipInstance.TransformPosition, ref closestPointOnPath, ref closestPointOnPathTValue);
                        // Get the tangent, normal and binormal information
                        SSCMath.GetPathFrenetData(behaviourInput.targetPath, lastTargetPathIndex, closestPointOnPathTValue,
                            ref closestPointOnPathTangent, ref closestPointOnPathNormal, ref closestPointOnPathBinormal);
                        // Get the curvature of this point
                        // If the ship is sticking to a ground surface, calculate the curvature projected into the plane
                        // of the ground surface. This way speed calculation will only care about changes in the path
                        // perpendicular to the ground surface
                        if (behaviourInput.shipInstance.IsGrounded)
                        {
                            SSCMath.GetPathCurvatureInPlane(behaviourInput.targetPath, lastTargetPathIndex, closestPointOnPathTValue,
                                behaviourInput.shipInstance.WorldTargetPlaneNormal, ref closestPointOnPathCurvature);
                        }
                        // Otherwise just calculate curvature normally
                        else
                        {
                            SSCMath.GetPathCurvature(behaviourInput.targetPath, lastTargetPathIndex, closestPointOnPathTValue,
                                ref closestPointOnPathCurvature);
                        }
                        // Calculate the distance from the ship to the path
                        // Subtract the projection onto the tangent (to get vector rejection)
                        // This gives the position of the ship projected onto a vector normal to the path
                        // TODO optimise
                        Vector3 projectedShipPosition = behaviourInput.shipInstance.TransformPosition -
                            Vector3.Project(behaviourInput.shipInstance.TransformPosition - closestPointOnPath, closestPointOnPathTangent);
                        // If the ship is sticking to a ground surface, project the projected ship position
                        // into the plane of that ground surface
                        // This way we will only care about aspects of the path in the plane of the ground surface
                        if (behaviourInput.shipInstance.IsGrounded)
                        {
                            // TODO optimise
                            projectedShipPosition = Vector3.ProjectOnPlane(projectedShipPosition - closestPointOnPath,
                                behaviourInput.shipInstance.WorldTargetPlaneNormal) + closestPointOnPath;
                        }

                        // Measure the distance (perpendicular to the path tangent) from the projected ship position to the path
                        float distanceToPath = Vector3.Distance(projectedShipPosition, closestPointOnPath);
                        // Compare the distance to the path radius to compute whether we are currenly within the bounds of the path
                        bool withinPathRadius = distanceToPath < pathRadius;

                        // Set the time value between the last point and the next point on the path.
                        behaviourInput.shipAIInputModuleInstance.SetCurrentTargetPathTValue(closestPointOnPathTValue);

                        #endregion

                        #region Steering

                        // TODO DEBUG LINES
                        //Debug.DrawLine(behaviourInput.shipInstance.TransformPosition, closestPointOnPath, Color.green);
                        //Debug.DrawRay(closestPointOnPath, (projectedShipPosition - closestPointOnPath).normalized * pathRadius, Color.red);
                        //Debug.DrawRay(closestPointOnPath + (closestPointOnPathTangent * 0.1f), (projectedShipPosition - closestPointOnPath).normalized * distanceToPath, Color.yellow);

                        // Look ahead a given distance along the path and use that point on the path to inform our heading
                        // Get the current speed of the ship in the forwards direction
                        float currentSpeed = behaviourInput.shipInstance.LocalVelocity.z > 0f ? behaviourInput.shipInstance.LocalVelocity.z : 0f;
                        // Calculate the distance to look ahead for steering
                        // TODO improve algorithm, don't use hardcoded values (account for maneuverability), optimise
                        float steerLookAheadTime = Mathf.Lerp(0.2f, 0.4f, 1f - (closestPointOnPathCurvature * 500f));
                        float steerLookAheadDistance = steerLookAheadTime * currentSpeed;
                        // If we are on the path, minimum steer look ahead distance is proportional to distance from the path
                        // TODO NOTE ONLY: first is * 20f, second is * 5f
                        if (withinPathRadius && steerLookAheadDistance < distanceToPath * 15f)
                        {
                            steerLookAheadDistance = distanceToPath * 15f;
                        }
                        // If we are not on the path, minimum steer look ahead distance is proportional to path radius
                        else if (!withinPathRadius && steerLookAheadDistance < pathRadius * 15f)
                        {
                            steerLookAheadDistance = pathRadius * 15f;
                        }
                        // Look ahead distance must be at least the ship's assumed diameter
                        if (steerLookAheadDistance < behaviourInput.shipAIInputModuleInstance.shipRadius * 2f)
                        {
                            steerLookAheadDistance = behaviourInput.shipAIInputModuleInstance.shipRadius * 2f;
                        }
                        // Find the point that distance along the path
                        Vector3 steerTargetPathPoint = Vector3.zero;
                        float steerTargetPathPointTValue = 0f;
                        Vector3 steerTargetPathTangent = Vector3.zero;
                        float steerTargetPathPointCurvature = 0f;
                        int steerTargetLastTargetPathIndex = 0;
                        // TODO change "5" based on quality
                        SSCMath.GetFurtherPointOnPathData(behaviourInput.targetPath, lastTargetPathIndex, closestPointOnPathTValue,
                            steerLookAheadDistance, 5, ref steerTargetPathPoint, ref steerTargetPathPointCurvature,
                            ref steerTargetLastTargetPathIndex, ref steerTargetPathPointTValue);

                        // Get the tangent of the steer path point
                        SSCMath.GetPathTangent(behaviourInput.targetPath, steerTargetLastTargetPathIndex, steerTargetPathPointTValue, ref steerTargetPathTangent);
                        // Find the point to use for the target output point
                        // This will be some point at or further than the steer target path point
                        // How far ahead of the steer target path point it is is determined by ship speed and path curvature
                        // TODO optimise
                        float bhTargetLookAheadDistance = Mathf.Lerp(0.25f, 0.5f, 1f - (closestPointOnPathCurvature * 500f)) * currentSpeed;
                        // Get the target point
                        Vector3 bhTargetPathPoint = Vector3.zero;
                        float bhTargetPathPointTValue = 0f;
                        float bhTargetPathPointCurvature = 0f;
                        int bhLastTargetPathIndex = 0;
                        // TODO change "5" based on quality
                        SSCMath.GetFurtherPointOnPathData(behaviourInput.targetPath, steerTargetLastTargetPathIndex, steerTargetPathPointTValue,
                            bhTargetLookAheadDistance, 5, ref bhTargetPathPoint, ref bhTargetPathPointCurvature,
                            ref bhLastTargetPathIndex, ref bhTargetPathPointTValue);

                        // If the ship is sticking to a ground surface, project the steer target point and target point
                        // into the plane of that ground surface
                        // This way we will only care about aspects of the path in the plane of the ground surface
                        if (behaviourInput.shipInstance.IsGrounded)
                        {
                            // TODO optimise
                            steerTargetPathPoint = Vector3.ProjectOnPlane(steerTargetPathPoint - behaviourInput.shipInstance.TransformPosition,
                                behaviourInput.shipInstance.WorldTargetPlaneNormal) + behaviourInput.shipInstance.TransformPosition;
                            bhTargetPathPoint = Vector3.ProjectOnPlane(bhTargetPathPoint - behaviourInput.shipInstance.TransformPosition,
                                behaviourInput.shipInstance.WorldTargetPlaneNormal) + behaviourInput.shipInstance.TransformPosition;
                        }

                        // Calculate heading from the point on the path we found
                        float distToSteerTargetPoint = (steerTargetPathPoint - behaviourInput.shipInstance.TransformPosition).magnitude;
                        behaviourOutput.heading = (steerTargetPathPoint - behaviourInput.shipInstance.TransformPosition) / distToSteerTargetPoint;
                        // Assign the calculated target point
                        behaviourOutput.target = bhTargetPathPoint;
                        behaviourOutput.setTarget = true;
                        // No desired upwards orientation
                        behaviourOutput.up = Vector3.zero;

                        // TODO DEBUG LINE
                        //Debug.DrawLine(behaviourInput.shipInstance.TransformPosition, steerTargetPathPoint, Color.magenta);
                        //Debug.DrawRay(behaviourInput.shipInstance.TransformPosition, behaviourInput.shipInstance.WorldTargetPlaneNormal * 10f, Color.gray);

                        #endregion

                        #region Current Speed

                        // Calculate the radius of the curve that we want to be turning through at the steer target point
                        // We set this to the radius of the curve at the closest point on the path so that we match our
                        // speed to the curve correctly where we are currently
                        float curveEndingRadius = 1f / closestPointOnPathCurvature;
                        // Calculate the effective radius of the curve that we are currently turning through
                        // This is measured in the same plane of the curve at the closest point on the curve
                        float effectiveAngularVelocity = 1f;
                        if (behaviourInput.shipInstance.IsGrounded)
                        {
                            // TODO optimise
                            effectiveAngularVelocity = Vector3.Dot(behaviourInput.shipInstance.WorldAngularVelocity,
                                behaviourInput.shipInstance.WorldTargetPlaneNormal);

                            // TODO need to work out correct sign
                            if (effectiveAngularVelocity < 0f) { effectiveAngularVelocity = -effectiveAngularVelocity; }
                        }
                        else
                        {
                            // TODO optimise
                            effectiveAngularVelocity = Vector3.Dot(behaviourInput.shipInstance.WorldAngularVelocity,
                                closestPointOnPathBinormal);
                            // TODO need to work out correct sign
                            if (effectiveAngularVelocity < 0f) { effectiveAngularVelocity = -effectiveAngularVelocity; }
                        }
                        float curveStartingRadius = effectiveAngularVelocity > 0f ? (currentSpeed / effectiveAngularVelocity) : 10000000f;

                        // Prevents case of zero curve starting radius
                        if (curveStartingRadius < 0.1f) { curveStartingRadius = 10000000f; }

                        // Calculate a maximum speed based on the current path curvature
                        // This assumes the ideal case: That we are following the path exactly
                        // TODO: Maybe dist to steer target point should be * 0.5?
                        float currentTargetSpeed = behaviourInput.shipAIInputModuleInstance.MaxSpeedAlongCurve(curveStartingRadius,
                            curveEndingRadius, distToSteerTargetPoint, behaviourInput.shipInstance.IsGrounded);
                        // Calculate the speed required based on our actual position relative to the path
                        if (!withinPathRadius)
                        {
                            // Since we are outside the bounds of the path, we need to rejoin the path
                            // Radius of curvature requires an angle and a distance it is over
                            // Distance is the distance to the steer target point
                            // Calculate the angle for the "in" curve and the "out" curve
                            float turnAngle1 = (float)System.Math.Acos(Vector3.Dot(behaviourInput.shipInstance.WorldVelocity.normalized,
                                steerTargetPathPoint - behaviourInput.shipInstance.TransformPosition) / distToSteerTargetPoint);
                            float turnAngle2 = (float)System.Math.Acos(Vector3.Dot(steerTargetPathTangent,
                                steerTargetPathPoint - behaviourInput.shipInstance.TransformPosition) / distToSteerTargetPoint);
                            // Calculate the radius for the "in" curve and the "out" curve
                            float turnRadius1 = distToSteerTargetPoint / (2f * (float)System.Math.Sin(turnAngle1));
                            float turnRadius2 = distToSteerTargetPoint / (2f * (float)System.Math.Sin(turnAngle2));
                            // Calculate the maximum turn speed for the "in" curve and the out "curve"
                            float turnSpeed1 = behaviourInput.shipAIInputModuleInstance.MaxSpeedAlongConstantRadiusCurve(turnRadius1, behaviourInput.shipInstance.IsGrounded);
                            float turnSpeed2 = behaviourInput.shipAIInputModuleInstance.MaxSpeedAlongConstantRadiusCurve(turnRadius2, behaviourInput.shipInstance.IsGrounded);
                            // Update the current target speed accordingly
                            if (turnSpeed1 < currentTargetSpeed) { currentTargetSpeed = turnSpeed1; }
                            if (turnSpeed2 < currentTargetSpeed) { currentTargetSpeed = turnSpeed2; }
                        }

                        #endregion

                        #region Future Speed

                        // Declare variables for use
                        Vector3 pointOnPath = Vector3.zero;
                        float tValue = 0f;
                        float newTValue = 0f;
                        int newLastTargetPathIndex = 0;
                        float pathCurvature = 0f;

                        // Calculate total look-ahead distance based on stopping distance
                        // POSSIBLE BUG: Probably LocalVelocity should be normalised as a parameter for BrakingDistance(...)
                        float lookAheadDistance = behaviourInput.shipAIInputModuleInstance.BrakingDistance(currentSpeed, 0.1f, behaviourInput.shipInstance.LocalVelocity);
                        // Choose the distance increment size based on the user-specified path following quality
                        float distanceIncrement = lookAheadDistance / speedLookAheadIterations;
                        if (distanceIncrement < 0.001f) { distanceIncrement = 0.001f; }
                        float totalObservedDistance = distanceIncrement;
                        // Remember the curvature of the closest point on the path as the last path point curvature
                        float lastPathCurvature = closestPointOnPathCurvature;
                        // Start at the closest point on the path
                        tValue = closestPointOnPathTValue;
                        // Iterate over the path at regular distance intervals to find what speed we should be doing right now
                        int penultimateTargetPathLocationIdx = SSCManager.GetPreviousPathLocationIndex(behaviourInput.targetPath, lastTargetPathLocationIdx, false);
                        while (totalObservedDistance < lookAheadDistance + 0.001f)
                        {
                            // If this is not a closed ciruit, don't go past the last path point
                            if (!behaviourInput.targetPath.isClosedCircuit &&
                                (lastTargetPathIndex == lastTargetPathLocationIdx ||
                                (lastTargetPathIndex == penultimateTargetPathLocationIdx && newTValue > 0.999f)))
                            {
                                totalObservedDistance = lookAheadDistance + 1f;
                            }
                            else
                            {
                                // Get the path data at the new point on the path
                                SSCMath.GetFurtherPointOnPathData(behaviourInput.targetPath, lastTargetPathIndex, tValue, distanceIncrement,
                                    furtherPathPointCalculationIterations, ref pointOnPath, ref pathCurvature, ref newLastTargetPathIndex,
                                    ref newTValue);
                                if (behaviourInput.shipInstance.IsGrounded)
                                {
                                    // Project the curvature into the ground plane if we are on the ground
                                    // This isn't technically accurate (since the ground plane could have changed by the time that
                                    // we reach that point on the track) but does improve performance significantly
                                    SSCMath.GetPathCurvatureInPlane(behaviourInput.targetPath, newLastTargetPathIndex, newTValue,
                                        behaviourInput.shipInstance.WorldTargetPlaneNormal, ref pathCurvature);
                                }
                                // Calculate the required speed from the curvature (and the rate of change of curvature)
                                float maxCurveSpeed = behaviourInput.shipAIInputModuleInstance.MaxSpeedAlongChangingRadiusCurve(
                                1f / lastPathCurvature, 1f / pathCurvature, distanceIncrement);
                                // ORIGINAL CODE
                                //float maxCurrentSpeed = behaviourInput.shipAIInputModuleInstance.MaxSpeedFromBrakingDistance(maxCurveSpeed, totalObservedDistance - distanceIncrement);
                                // 31/03/2020 CODE - Takes into account distance to path for braking
                                float maxCurrentSpeed = behaviourInput.shipAIInputModuleInstance.MaxSpeedFromBrakingDistance(maxCurveSpeed, totalObservedDistance - distanceIncrement + distanceToPath, behaviourInput.shipInstance.LocalVelocity.normalized);
                                // If the required current speed is lower than our current target speed, update our current target speed
                                if (maxCurrentSpeed < currentTargetSpeed) { currentTargetSpeed = maxCurrentSpeed; }
                                // Set the path data for the next point on the path
                                lastTargetPathIndex = newLastTargetPathIndex;
                                tValue = newTValue;
                                // Increment the observed distance
                                totalObservedDistance += distanceIncrement;
                                // Remember the last value of the path curvature
                                lastPathCurvature = pathCurvature;
                            }
                        }

                        // Current target speed is not allowed to exceed the set max speed
                        if (currentTargetSpeed > behaviourInput.shipAIInputModuleInstance.maxSpeed)
                        {
                            currentTargetSpeed = behaviourInput.shipAIInputModuleInstance.maxSpeed;
                        }

                        #endregion

                        // Set velocity input from the heading and the target speed we calculated
                        behaviourOutput.velocity = behaviourOutput.heading * currentTargetSpeed;

                        // Add the velocity of the path
                        Vector3 pathVelocity = Vector3.zero;
                        SSCMath.GetPathVelocity(behaviourInput.targetPath,
                            behaviourInput.shipControlModuleInstance.shipInstance.TransformPosition, ref pathVelocity);
                        behaviourOutput.velocity += pathVelocity;
                    }
                }
            }
            else
            {
                // Default to facing forward and stopping if the path is invalid
                behaviourOutput.heading = Vector3.forward;
                behaviourOutput.up = Vector3.zero;
                behaviourOutput.velocity = Vector3.zero;
                behaviourOutput.target = Vector3.zero;
                behaviourOutput.setTarget = true;
            }

            // Whether we use targeting accuracy depends on behaviour input settings
            behaviourOutput.useTargetingAccuracy = behaviourInput.useTargetingAccuracy;
        }

        /// <summary>
        /// Sets a behaviour output to an "dock" behaviour output.
        /// Required inputs: Target position, target forwards, target up, target radius, target velocity, weighting.
        /// </summary>
        /// <param name="behaviourInput"></param>
        /// <param name="behaviourOutput"></param>
        public static void SetDockBehaviourOutput(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput)
        {
            // Do precalculation for heading vector
            headingVector = behaviourInput.targetPosition - behaviourInput.shipControlModuleInstance.shipInstance.TransformPosition;
            headingVectorSqrMagnitude = headingVector.sqrMagnitude;
            headingVectorMagnitude = Mathf.Sqrt(headingVectorSqrMagnitude);
            headingVectorNormalised = headingVector / headingVectorMagnitude;

            if (headingVectorMagnitude > behaviourInput.targetRadius)
            {
                // When outside the target radius...
                // Desired velocity is generally max speed in direction of desired heading, but decreases when nearing the target
                behaviourOutput.velocity = headingVectorNormalised * behaviourInput.shipAIInputModuleInstance.MaxSpeedFromBrakingDistance(0f, headingVectorMagnitude * 0.5f, behaviourInput.shipInstance.LocalVelocity.normalized);
                // Re-clamp velocity magnitude to max speed if needed
                if (behaviourOutput.velocity.sqrMagnitude > behaviourInput.shipAIInputModuleInstance.maxSpeed * behaviourInput.shipAIInputModuleInstance.maxSpeed)
                {
                    behaviourOutput.velocity = headingVectorNormalised * behaviourInput.shipAIInputModuleInstance.maxSpeed;
                }
                // Add target velocity to output velocity to adjust for moving target
                behaviourOutput.velocity += behaviourInput.targetVelocity;
                // Target is the target position, but shifted towards the ship by the target radius
                // This is done to prevent obstacle avoidance being triggered
                behaviourOutput.target = behaviourInput.targetPosition - (headingVectorNormalised * behaviourInput.targetRadius);
                behaviourOutput.setTarget = true;
                //Debug.DrawRay(behaviourInput.shipControlModuleInstance.shipInstance.TransformPosition, behaviourOutput.velocity, Color.red);
            }
            else
            {
                // When inside the target radius...
                // Desired velocity is towards the target position

                // Minimum speed is so that it would take five times the target time to reach the target position from the target radius
                float minSpeed = 1f;

                if (behaviourInput.targetTime > 0f)
                {
                    minSpeed = behaviourInput.targetRadius / (behaviourInput.targetTime * 5f);
                }
                if (minSpeed < 1f) { minSpeed = 1f; }
                // Maximum speed depends on the braking distance
                // NOTE: Currently requires a Reverse Thruster.
                float maxSpeed = behaviourInput.shipAIInputModuleInstance.MaxSpeedFromBrakingDistance(0f, headingVectorMagnitude * 0.5f, behaviourInput.shipInstance.LocalVelocity.normalized);

                // Target speed is proportional to the square distance to the target
                float speedMultiplier = 1f;
                if (behaviourInput.targetTime > 0f)
                {
                    // Adjust speed multiplier so that the time to move from target radius to position at which min speed is reached
                    // will be approximately the target time
                    speedMultiplier = 1f / (behaviourInput.targetRadius * ((1f / (behaviourInput.targetTime * minSpeed)) - 1f));
                }
                // NOTE: Previously the below was linear instead of quadratic
                // v = m*x^2
                float targetSpeed = speedMultiplier * headingVectorMagnitude *  headingVectorMagnitude;

                // ATTEMPTED IMPROVEMENT #1: Slow down if the angle to the target rotation is too great
                //float angleDelta = Quaternion.Angle(behaviourInput.shipControlModuleInstance.shipInstance.TransformRotation, Quaternion.LookRotation(behaviourInput.targetForwards, behaviourInput.targetUp));
                //float maxAngleDelta = Mathf.InverseLerp(1f, 10f, headingVectorMagnitude / behaviourInput.targetRadius);
                //float targetAngleDelta = Mathf.InverseLerp(0f, 5f, headingVectorMagnitude / behaviourInput.targetRadius);
                //targetSpeed *= Mathf.InverseLerp(targetAngleDelta, maxAngleDelta, angleDelta);

                // ATTEMPTED IMPROVEMENT #2: Redirect velocity if we are going off-course
                float zFactor = 1f;
                float veloAngleDelta = Vector3.Dot(headingVectorNormalised, behaviourInput.shipControlModuleInstance.shipInstance.WorldVelocity) / behaviourInput.shipControlModuleInstance.shipInstance.WorldVelocity.magnitude * Mathf.Rad2Deg;
                float maxVeloAngleDelta = Mathf.InverseLerp(5f, 30f, headingVectorMagnitude / behaviourInput.targetRadius);
                float targetVeloAngleDelta = Mathf.InverseLerp(0f, 10f, headingVectorMagnitude / behaviourInput.targetRadius);
                zFactor = Mathf.InverseLerp(targetVeloAngleDelta, maxVeloAngleDelta, veloAngleDelta);
                behaviourOutput.velocity = headingVectorNormalised;       

                Vector3 rVector = behaviourInput.shipControlModuleInstance.shipInstance.WorldVelocity -
                    Vector3.Project(behaviourInput.shipControlModuleInstance.shipInstance.WorldVelocity, headingVectorNormalised);
                behaviourOutput.velocity += rVector.normalized * zFactor;
                // Normalise the vector - but if the vector is zero, just set it to the heading vector
                if (behaviourOutput.velocity.sqrMagnitude > Mathf.Epsilon) { behaviourOutput.velocity.Normalize(); }
                else { behaviourOutput.velocity = headingVectorNormalised; }

                // Clamp target speed between min and max speeds
                if (targetSpeed < minSpeed) { targetSpeed = minSpeed; }
                else if (targetSpeed > maxSpeed) { targetSpeed = maxSpeed; }

                behaviourOutput.velocity *= targetSpeed;

                // Add target velocity to output velocity to adjust for moving target
                behaviourOutput.velocity += behaviourInput.targetVelocity;
                // Target is the target position
                behaviourOutput.target = behaviourInput.targetPosition;
                behaviourOutput.setTarget = true;
                //Debug.DrawRay(behaviourInput.shipControlModuleInstance.shipInstance.TransformPosition, behaviourOutput.velocity, Color.green);
            }

            // Calculate interpolation float: 0 is inside target radius, 1 is outside 2 * target radius
            float interpolationValue = (headingVectorMagnitude / behaviourInput.targetRadius) - 1f;
            if (interpolationValue < 0f) { interpolationValue = 0f; }
            else if (interpolationValue > 1f) { interpolationValue = 1f; }

            // Desired heading and up directions are interpolated around the target radius
            // Inside the target radius:
            // - Desired heading is target forwards direction
            // - Desired up is target upwards direction
            // Outside the target radius:
            // - Desired heading is towards the target position
            // - Desired up direction is vector rejection of target up direction onto heading vector
            behaviourOutput.heading = Vector3.Slerp(behaviourInput.targetForwards, (headingVector + behaviourInput.targetVelocity).normalized, interpolationValue);
            behaviourOutput.up = behaviourInput.targetUp - (Vector3.Project(behaviourOutput.up, headingVector) * interpolationValue);

            //Debug.DrawLine(behaviourInput.shipControlModuleInstance.shipInstance.TransformPosition, behaviourOutput.target, Color.blue);
            //Debug.DrawLine(behaviourOutput.target, behaviourInput.targetPosition, Color.cyan);

            //Debug.DrawLine(behaviourInput.shipControlModuleInstance.shipInstance.TransformPosition, behaviourInput.targetPosition, Color.grey);

            // Never use targeting accuracy
            behaviourOutput.useTargetingAccuracy = false;
        }

        #endregion

        #endregion
    }
}

