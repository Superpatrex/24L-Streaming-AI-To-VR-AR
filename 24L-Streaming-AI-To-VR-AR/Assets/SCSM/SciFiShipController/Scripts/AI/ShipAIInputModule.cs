using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [AddComponentMenu("Sci-Fi Ship Controller/Ship Components/Ship AI Input Module")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    [RequireComponent(typeof(ShipControlModule))]
    [DisallowMultipleComponent]
    public class ShipAIInputModule : MonoBehaviour
    {
        #region Public Enumerations

        public enum AIMovementAlgorithm
        {
            //LeftAndRightOnly = 10,
            //LeftAndRightStrafeOnly = 11,
            PlanarFlight = 20,
            PlanarFlightBanking = 25,
            Full3DFlight = 30
        }

        public enum AIObstacleAvoidanceQuality
        {
            Off = 5,
            Low = 10,
            Medium = 15,
            High = 20
        }

        public enum AIPathFollowingQuality
        {
            VeryLow = 5,
            Low = 10,
            Medium = 15,
            High = 20
        }

        public enum AIStateActionInfo
        {
            /// <summary>
            /// The current state is a custom state.
            /// </summary>
            Custom = 0,
            /// <summary>
            /// The current state is Idle. The current state action is idling.
            /// </summary>
            Idle = 5,
            /// <summary>
            /// The current state is Move To. The current state action is moving towards TargetPosition.
            /// </summary>
            MoveToSeekPosition = 10,
            /// <summary>
            /// The current state is Move To. The current state action is moving towards TargetLocation.
            /// </summary>
            MoveToSeekLocation = 11,
            /// <summary>
            /// The current state is Move To. The current state action is following TargetPath.
            /// </summary>
            MoveToFollowPath = 12,
            /// <summary>
            /// The current state is Dogfight. The current state action is attacking TargetShip.
            /// </summary>
            DogfightAttackShip = 20,
            /// <summary>
            /// The current state is Docking. The current state action is moving towards TargetPosition and TargetRotation.
            /// </summary>
            Docking = 25,
            /// <summary>
            /// The current state is Strafing Run. The current state action is moving towards and attacking TargetPosition
            /// before moving away from TargetPosition when within TargetRadius.
            /// </summary>
            StrafingRun = 30
        }

        #endregion

        #region Public Variables and Properties

        /// <summary>
        /// If enabled, the Initialise() will be called as soon as Awake() runs. This should be disabled if you are
        /// instantiating the ShipAIInputModule through code.
        /// </summary>
        public bool initialiseOnAwake = false;

        /// <summary>
        /// Can the main update loop perform calculations and send input to the ship?
        /// </summary>
        public bool IsAIEnabled { get { return isAIEnabled; } set { EnableOrDisableAI(value); } }

        /// <summary>
        /// Will the main update loop perform calculations and send input to the ship as soon
        /// as it is initialised? [Default: true]
        /// </summary>
        public bool isEnableAIOnInitialise = true;

        /// <summary>
        /// Has the AI module been initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// The algorithm used for calculating AI movement.
        /// </summary>
        public AIMovementAlgorithm movementAlgorithm = AIMovementAlgorithm.PlanarFlightBanking;

        /// <summary>
        /// The quality of obstacle avoidance for this AI ship. Lower quality settings will improve performance.
        /// </summary>
        public AIObstacleAvoidanceQuality obstacleAvoidanceQuality = AIObstacleAvoidanceQuality.Medium;

        /// <summary>
        /// Layermask determining which layers will be detected as obstacles when raycasted against. Exclude layers that
        /// you don't want the AI ship to try and avoid using obstacle avoidance.
        /// </summary>
        public LayerMask obstacleLayerMask = Physics.AllLayers;

        /// <summary>
        /// The starting offset for obstacle avoidance raycasts on the z-axis. Increase this value to move the
        /// starting point for obstacle avoidance raycasts forward and hence avoid detecting collisions with
        /// frontally-placed colliders within the ship itself.
        /// </summary>
        public float raycastStartOffsetZ = 0f;

        /// <summary>
        /// The quality of path following for this AI ship. Lower quality settings will improve performance.
        /// </summary>
        public AIPathFollowingQuality pathFollowingQuality = AIPathFollowingQuality.Medium;

        /// <summary>
        /// The max speed for the ship in metres per second.
        /// </summary>
        public float maxSpeed = 1000f;
        /// <summary>
        /// The supposed radius of the ship (approximated as a sphere) used for obstacle avoidance.
        /// </summary>
        public float shipRadius = 5f;

        /// <summary>
        /// The accuracy of the ship at shooting at a target. A value of 1 is perfect accuracy, while a value of 0
        /// is the lowest accuracy.
        /// </summary>
        [Range(0f, 1f)] public float targetingAccuracy = 1f;

        /// <summary>
        /// The maximum angle of the ship to the target at which it will fire.
        /// </summary>
        public float fireAngle = 10f;

        /// <summary>
        /// The maximum bank angle (in degrees) the ship should bank at while turning.
        /// Only relevant when movementAlgorithm is set to PlanarFlightBanking.
        /// </summary>
        [Range(10f, 90f)] public float maxBankAngle = 30f;
        /// <summary>
        /// The turning angle (in degrees) to the target position at which the AI will bank at the maxBankAngle. Lower values
        /// will result in the AI banking at a steeper angle for lower turning angles.
        /// Only relevant when movementAlgorithm is set to PlanarFlightBanking.
        /// </summary>
        [Range(5f, 90f)] public float maxBankTurnAngle = 15f;
        /// <summary>
        /// The maximum pitch angle (in degrees) that the AI is able to use to pitch towards the target position.
        /// Only relevant when movementAlgorithm is set to PlanarFlight or PlanarFlightBanking.
        /// </summary>
        [Range(5f, 90f)] public float maxPitchAngle = 90f;
        /// <summary>
        /// Only use pitch to steer when the ship is within the threshold (in degrees) of the correct yaw/roll angle.
        /// Only relevant when movementAlgorithm is set to Full3DFlight.
        /// </summary>
        [Range(10f, 90f)] public float turnPitchThreshold = 30f;
        /// <summary>
        /// When turning, will the ship favour yaw (i.e. turning using yaw then pitching) or roll (i.e. turning using roll
        /// then pitching) to achieve the turn? Lower values will favour yaw while higher values will favour roll.
        /// Only relevant when movementAlgorithm is set to Full3DFlight.
        /// </summary>
        [Range(0f, 1f)] public float rollBias = 0.5f;

        // Used for Debugging in the Editor
        public Vector3 DesiredLocalVelocity { get { return desiredLocalVelocity; } }
        public Vector3 CurrentLocalVelocity { get { return currentLocalVelocity; } }
        public ShipInput GetShipInput { get { return shipInput; } }

        /// <summary>
        /// Get a reference to the ShipControlModule component attached to this Ship AI Input Module.
        /// This is only available if the Ship AI Input Module is initialised. If not, it will return null.
        /// </summary>
        public ShipControlModule GetShipControlModule { get { if (isInitialised) { return shipControlModule; } else { return null; } } }

        /// <summary>
        /// Get a reference to the Ship instance which is part of an initialised ShipControlModule.
        /// If the Ship AI Input Module or Ship Control Module are not initialised, it will return null.
        /// </summary>
        public Ship GetShip { get { if (isInitialised) { if (shipControlModule != null && shipControlModule.IsInitialised) { return shipControlModule.shipInstance; } else { return null; } } else { return null; } } }
        
        /// <summary>
        /// Get the identity of the ship this AI module is attached to. It will return 0 if the ship is not initialised.
        /// </summary>
        public int GetShipId { get { if (isInitialised) { if (shipControlModule != null && shipControlModule.IsInitialised) { return shipControlModule.shipInstance.shipId; } else { return 0; } } else { return 0; } } }

        #endregion

        #region Public Data Discard variables (Advanced)
        /// <summary>
        /// Should we use or discard data from the horizontal axis?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isHorizontalDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the vertical axis?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isVerticalDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the longitudinal axis?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isLongitudinalDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the pitch axis?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isPitchDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the yaw axis?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isYawDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the roll axis?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isRollDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the primaryFire button?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isPrimaryFireDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the secondaryFire button?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isSecondaryFireDataDiscarded;
        /// <summary>
        /// Should we use or discard data from the dock button?
        /// Call ReinitialiseDiscardData() after modifying this at runtime.
        /// </summary>
        public bool isDockDataDiscarded;
        #endregion

        #region Public Delegates

        public delegate void CallbackCustomIdleBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        public delegate void CallbackCustomSeekBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        public delegate void CallbackCustomFleeBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        public delegate void CallbackCustomPursuitBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        public delegate void CallbackCustomEvasionBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        public delegate void CallbackCustomSeekArrivalBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        public delegate void CallbackCustomSeekMovingArrivalBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        public delegate void CallbackCustomPursuitArrivalBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        //public delegate void CallbackCustomFollowBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        //public delegate void CallbackCustomAvoidBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        //public delegate void CallbackCustomBlockCylinderBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        //public delegate void CallbackCustomBlockConeBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        public delegate void CallbackCustomUnblockCylinderBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        public delegate void CallbackCustomUnblockConeBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        public delegate void CallbackCustomObstacleAvoidanceBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        public delegate void CallbackCustomFollowPathBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);
        public delegate void CallbackCustomDockBehaviour(AIBehaviourInput behaviourInput, AIBehaviourOutput behaviourOutput);

        public delegate void CallbackStateMethod(AIStateMethodParameters stateMethodParameters);
        public delegate void CallbackCompletedStateAction(ShipAIInputModule shipAIInputModule);

        public delegate void CallbackOnStateChange(ShipAIInputModule shipAIInputModule, int currentStateId, int previousStateId);

        // These callback methods allow a game developer to supply a custom method (delegate) that gets called instead
        // of the default behaviour.

        /// <summary>
        /// The name of the developer-supplied custom method that is called when the AIBehaviourType is "CustomIdle".
        /// </summary>
        public CallbackCustomIdleBehaviour callbackCustomIdleBehaviour = null;
        /// <summary>
        /// The name of the developer-supplied custom method that is called when the AIBehaviourType is "CustomSeek".
        /// </summary>
        public CallbackCustomSeekBehaviour callbackCustomSeekBehaviour = null;
        /// <summary>
        /// The name of the developer-supplied custom method that is called when the AIBehaviourType is "CustomFlee".
        /// </summary>
        public CallbackCustomFleeBehaviour callbackCustomFleeBehaviour = null;
        /// <summary>
        /// The name of the developer-supplied custom method that is called when the AIBehaviourType is "CustomPursuit".
        /// </summary>
        public CallbackCustomPursuitBehaviour callbackCustomPursuitBehaviour = null;
        /// <summary>
        /// The name of the developer-supplied custom method that is called when the AIBehaviourType is "CustomEvasion".
        /// </summary>
        public CallbackCustomEvasionBehaviour callbackCustomEvasionBehaviour = null;
        /// <summary>
        /// The name of the developer-supplied custom method that is called when the AIBehaviourType is "CustomSeekArrival".
        /// </summary>
        public CallbackCustomSeekArrivalBehaviour callbackCustomSeekArrivalBehaviour = null;
        /// <summary>
        /// The name of the developer-supplied custom method that is called when the AIBehaviourType is "CustomSeekMovingArrival".
        /// </summary>
        public CallbackCustomSeekMovingArrivalBehaviour callbackCustomSeekMovingArrivalBehaviour = null;
        /// <summary>
        /// The name of the developer-supplied custom method that is called when the AIBehaviourType is "CustomPursuitArrival".
        /// </summary>
        public CallbackCustomPursuitArrivalBehaviour callbackCustomPursuitArrivalBehaviour = null;
        ///// <summary>
        ///// The name of the developer-supplied custom method that is called when the AIBehaviourType is "CustomAvoid".
        ///// </summary>
        //public CallbackCustomAvoidBehaviour callbackCustomAvoidBehaviour = null;
        ///// <summary>
        ///// The name of the developer-supplied custom method that is called when the AIBehaviourType is "CustomFollow".
        ///// </summary>
        //public CallbackCustomFollowBehaviour callbackCustomFollowBehaviour = null;
        /// <summary>
        /// The name of the developer-supplied custom method that is called when the AIBehaviourType is "CustomUnblockCylinder".
        /// </summary>
        public CallbackCustomUnblockCylinderBehaviour callbackCustomUnblockCylinderBehaviour = null;
        /// <summary>
        /// The name of the developer-supplied custom method that is called when the AIBehaviourType is "CustomUnblockCone".
        /// </summary>
        public CallbackCustomUnblockConeBehaviour callbackCustomUnblockConeBehaviour = null;
        /// <summary>
        /// The name of the developer-supplied custom method that is called when the AIBehaviourType is "CustomObstacleAvoidance".
        /// </summary>
        public CallbackCustomObstacleAvoidanceBehaviour callbackCustomObstacleAvoidanceBehaviour = null;
        /// <summary>
        /// The name of the developer-supplied custom method that is called when the AIBehaviourType is "CustomFollowPath".
        /// </summary>
        public CallbackCustomFollowPathBehaviour callbackCustomFollowPathBehaviour = null;
        /// <summary>
        /// The name of the developer-supplied custom method that is called when the AIBehaviourType is "CustomDock".
        /// </summary>
        public CallbackCustomDockBehaviour callbackCustomDockBehaviour = null;

        /// <summary>
        /// The name of the developer-supplied custom method that is called when the current state action has been completed.
        /// Must have 1 parameter of type ShipAIInputModule.
        /// </summary>
        public CallbackCompletedStateAction callbackCompletedStateAction = null;

        /// <summary>
        /// The name of the developer-supplied custom method that gets called whenever the state changes.
        /// Must have 3 parameters of type: ShipAIInputModule, int (currentStateId), and int (previousStateId).
        /// </summary>
        public CallbackOnStateChange callbackOnStateChange = null;

        #endregion

        #region Private Variables

        private ShipControlModule shipControlModule;
        private bool isInitialised = false;

        /// <summary>
        /// When false, prevents the main update loop from performing calculations
        /// or sending input to the ship.
        /// </summary>
        private bool isAIEnabled = false;

        // State variables
        private Vector3 targetPosition = Vector3.zero;
        private Quaternion targetRotation = Quaternion.identity;
        private LocationData targetLocation = null;
        private PathData targetPath = null;
        private int currentTargetPathLocationIndex = -1;
        private int prevTargetPathLocationIndex = -1;
        private float currentTargetPathTValue = 0f;
        private Ship targetShip = null;
        private List<Ship> shipsToEvade = null;
        private List<SurfaceTurretModule> surfaceTurretsToEvade = null;
        private float targetRadius = 10f;
        private float targetDistance = 1f;
        private float targetAngularDistance = 1f;
        private Vector3 targetVelocity = Vector3.zero;
        private float targetTime = 0f;
        private bool hasCompletedStateAction = false;
        private int currentStateStageIndex = 0;

        private ShipInput shipInput;

        private List<AIBehaviourInput> behaviourInputsList;
        private List<AIBehaviourOutput> behaviourOutputsList;
        private int behavioursListCount = 10;
        private AIBehaviourOutput combinedBehaviourOutput;
        private Vector3 lastBehaviourInputTarget = Vector3.zero;

        private AIState currentState = null;
        private AIStateMethodParameters stateMethodParameters;

        private PIDController pitchPIDController;
        private PIDController yawPIDController;
        private PIDController rollPIDController;

        private PIDController verticalPIDController;
        private PIDController horizontalPIDController;
        private PIDController longitudinalPIDController;

        private float targetRoll = 0f, targetYaw = 0f, targetPitch = 0f;
        private float currentRoll = 0f, currentYaw = 0f, currentPitch = 0f;

        private float sinTheta = 0f;

        private List<int> targetingWeaponIdxList;

        // Input calculation variables
        private Vector3 desiredHeadingFlat = Vector3.zero;
        private float desiredHeadingFlatMagnitude = 0f;
        private Vector3 desiredHeadingLocalSpace = Vector3.zero;
        private Vector3 desiredHeadingLocalSpaceXZPlane = Vector3.zero;
        private Vector3 desiredUpLocalSpace = Vector3.zero;
        private Vector3 desiredUpLocalSpaceXYPlane = Vector3.zero;
        private Vector3 desiredUpLocalSpaceYZPlane = Vector3.zero;
        private Vector3 shipForwardFlat = Vector3.zero;
        private Vector3 desiredLocalVelocity = Vector3.zero;
        private Vector3 currentLocalVelocity = Vector3.zero;

        // Characteristics of the ship
        // Physical characteristics
        private float shipMaxFlightTurnAcceleration = 100f;
        private float shipMaxGroundTurnAcceleration = 100f;
        private float shipMaxAngularAcceleration = 100f;
        private float shipMaxBrakingConstantDecelerationZ = 100f;
        private float shipMaxBrakingEffectiveDragCoefficientZ = 0f;
        private float shipMaxBrakingConstantDecelerationX = 100f;
        private float shipMaxBrakingConstantDecelerationY = 100f;
        // Combat characteristics
        private float primaryFireProjectileSpeed = 0f;
        private float secondaryFireProjectileSpeed = 0f;
        private float primaryFireProjectileDespawnTime = 0f;
        private float secondaryFireProjectileDespawnTime = 0f;
        private bool primaryFireUsesTurrets = false;
        private bool secondaryFireUsesTurrets = false;
        private Vector3 primaryFireWeaponDirection = Vector3.forward;
        private Vector3 secondaryFireWeaponDirection = Vector3.forward;
        private Vector3 primaryFireWeaponRelativePosition = Vector3.zero;
        private Vector3 secondaryFireWeaponRelativePosition = Vector3.zero;

        #if UNITY_EDITOR
        private bool logStateNullWarning = true;
        #endif

        #endregion

        // Use this for initialization
        void Awake()
        {
            if (initialiseOnAwake) { Initialise(); }
        }

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            // Only do any calculations if we have initialised and the ship is enabled
            if (isInitialised && isAIEnabled && shipControlModule.ShipIsEnabled())
            {
                // Clear behaviour inputs list
                for (int i = 0; i < behavioursListCount; i++) { behaviourInputsList[i].ClearBehaviourInput(); }

                // Update position and movement data (so that all data is current)
                shipControlModule.shipInstance.UpdatePositionAndMovementData(transform, shipControlModule.ShipRigidbody);

                #region Calculate Combined Behaviour Input

                // Call state method to get prioritised list of behaviour inputs
                // First check that current state and callback method are not null
                if (currentState != null && currentState.callbackStateMethod != null)
                {
                    // Update state method parameters
                    stateMethodParameters.targetPosition = targetPosition;
                    stateMethodParameters.targetRotation = targetRotation;
                    stateMethodParameters.targetLocation = targetLocation;
                    stateMethodParameters.targetPath = targetPath;
                    stateMethodParameters.targetShip = targetShip;
                    stateMethodParameters.shipsToEvade = shipsToEvade;
                    stateMethodParameters.surfaceTurretsToEvade = surfaceTurretsToEvade;
                    stateMethodParameters.targetRadius = targetRadius;
                    stateMethodParameters.targetDistance = targetDistance;
                    stateMethodParameters.targetAngularDistance = targetAngularDistance;
                    stateMethodParameters.targetVelocity = targetVelocity;
                    stateMethodParameters.targetTime = targetTime;
                    // Call state method
                    currentState.callbackStateMethod(stateMethodParameters);
                    #if UNITY_EDITOR
                    // Reset logging condition
                    logStateNullWarning = true;
                    #endif
                }
                #if UNITY_EDITOR
                else if (currentState == null)
                {
                    if (logStateNullWarning)
                    {
                        Debug.LogWarning("ERROR: AI state is null on " + gameObject.name + ". Make sure that when calling " +
                            "SetState() you pass in a valid state ID.");
                        logStateNullWarning = false;
                    }
                }
                else
                {
                    if (logStateNullWarning)
                    {
                        Debug.LogWarning("ERROR: AI state method is null on " + gameObject.name + ". If you are using a custom state, " +
                            "make sure to set the callbackStateMethod to the custom state method you have written.");
                        logStateNullWarning = false;
                    }
                }
                #endif

                // Combine behaviour inputs
                CombineBehaviourInputs(combinedBehaviourOutput, behaviourInputsList, behaviourOutputsList,
                    shipControlModule.shipInstance.TransformPosition, shipControlModule.shipInstance.WorldVelocity);

                // If the target (the actual world-space position the behaviour output is aiming for) is set, remember
                // it as the new latest target
                if (combinedBehaviourOutput.setTarget)
                {
                    lastBehaviourInputTarget = combinedBehaviourOutput.target;
                }

                // If the current state action has been completed, call the relevant callback (if it has been assigned)
                if (hasCompletedStateAction && callbackCompletedStateAction != null)
                {
                    callbackCompletedStateAction(this);
                }

                #endregion

                #region Calculate Ship Input

                #region Rotational Input

                // Calculate data common to the different algorithms
                // In the future, 2D algorithms may not use all of this data, so may need to split it up

                // Create flattened equivalent of combinedBehaviourOutput.heading
                desiredHeadingFlat = combinedBehaviourOutput.heading;
                desiredHeadingFlat.y = 0f;
                desiredHeadingFlatMagnitude = desiredHeadingFlat.magnitude;
                // We divide by this later, so we need to make sure this is non-zero
                if (desiredHeadingFlatMagnitude < 0.01f) { desiredHeadingFlatMagnitude = 0.01f; }

                // Check whether an up direction was specified
                bool upDirectionSpecified = combinedBehaviourOutput.up.sqrMagnitude > 0.01f;

                // Calculate desired up direction in local space
                if (movementAlgorithm == AIMovementAlgorithm.PlanarFlight || movementAlgorithm == AIMovementAlgorithm.PlanarFlightBanking)
                {
                    // Desired up for planar flight is world up direction
                    desiredUpLocalSpace = shipControlModule.shipInstance.TransformInverseRotation * Vector3.up;

                    // Calculate desired up projected into XY and YZ planes
                    desiredUpLocalSpaceXYPlane = desiredUpLocalSpace;
                    desiredUpLocalSpaceXYPlane.z = 0f;
                    desiredUpLocalSpaceYZPlane = desiredUpLocalSpace;
                    desiredUpLocalSpaceYZPlane.x = 0f;

                    // Normalise vectors
                    desiredUpLocalSpace.Normalize();
                    desiredUpLocalSpaceXYPlane.Normalize();
                    desiredUpLocalSpaceYZPlane.Normalize();
                }
                else if (movementAlgorithm == AIMovementAlgorithm.Full3DFlight)
                {
                    // For full 3D flight we want to turn in the following manner:
                    // - Roll so that the current heading is "up" or "down" in local space (not to the left or to the right)
                    // - Then pitch to match the current heading
                    // - The two actions above are performed simultaneously, but an "up" direction for the ship is calculated
                    //   as if they would be performed one after the other

                    // First, transform heading into ship local space
                    desiredHeadingLocalSpace = shipControlModule.shipInstance.TransformInverseRotation * combinedBehaviourOutput.heading;

                    if (!upDirectionSpecified)
                    {
                        // No up direction provided, so calculate it based on heading
                        // Project heading onto local XY plane by removing z component
                        // This will become the desired up projected into the XY plane
                        desiredUpLocalSpaceXYPlane = desiredHeadingLocalSpace;
                        desiredUpLocalSpaceXYPlane.z = 0f;
                        // Then minus the component of the XY-projected vector in the direction of the original heading
                        // to calculate desired up direction
                        desiredUpLocalSpace = desiredUpLocalSpaceXYPlane - (desiredHeadingLocalSpace * Vector3.Dot(desiredHeadingLocalSpace, desiredUpLocalSpaceXYPlane));
                        // Calculate desired up projected into YZ plane
                        desiredUpLocalSpaceYZPlane = desiredUpLocalSpace;
                        desiredUpLocalSpaceYZPlane.x = 0f;

                        // The above calculation for the desired up direction will always produce an up direction such that the 
                        // target is above the ship (in terms of pitch angle). This is usually desirable, but sometimes the 
                        // target will be just a small angle below the ship, which would require the ship to roll 180 degrees to meet
                        // the target. So the code below allows the up direction to be flipped if the following conditions are met:
                        // 1. The desired heading is below the ship 
                        if (desiredHeadingLocalSpace.y < 0f)
                        {
                            desiredUpLocalSpace *= -1f;
                            desiredUpLocalSpaceXYPlane *= -1f;
                            desiredUpLocalSpaceYZPlane *= -1f;
                        }
                    }
                    else
                    {
                        // Up direction was provided, so use that
                        desiredUpLocalSpace = shipControlModule.shipInstance.TransformInverseRotation * combinedBehaviourOutput.up;
                        // Project into local XY and YZ planes
                        desiredUpLocalSpaceXYPlane = desiredUpLocalSpace;
                        desiredUpLocalSpaceXYPlane.z = 0f;
                        desiredUpLocalSpaceYZPlane = desiredUpLocalSpace;
                        desiredUpLocalSpaceYZPlane.x = 0f;
                    }

                    // Normalise up direction vectors
                    desiredUpLocalSpace.Normalize();
                    desiredUpLocalSpaceXYPlane.Normalize();
                    desiredUpLocalSpaceYZPlane.Normalize();

                    // Calculate desired heading projected into XZ plane
                    // TODO: Originally this wasn't normalised - but I think it should be
                    // I need to check what effect it has
                    desiredHeadingLocalSpaceXZPlane = desiredHeadingLocalSpace;
                    desiredHeadingLocalSpaceXZPlane.y = 0f;
                    desiredHeadingLocalSpaceXZPlane.Normalize();
                }

                // Calculate a flattened version of the ship forwards vector
                shipForwardFlat = shipControlModule.shipInstance.TransformForward;
                shipForwardFlat.y = 0f;

                if (movementAlgorithm == AIMovementAlgorithm.PlanarFlight || movementAlgorithm == AIMovementAlgorithm.PlanarFlightBanking)
                {
                    // Ship pitch calculations
                    // Calculate the sine of the pitch delta angle between our current up direction and our desired up direction
                    sinTheta = Vector3.Cross(desiredUpLocalSpaceYZPlane, Vector3.up).x;
                    // Clamp sinTheta between -1 and 1
                    if (sinTheta > 1f) { sinTheta = 1f; }
                    else if (sinTheta < -1f) { sinTheta = -1f; }
                    // Use arcsin to determine the actual angle
                    currentPitch = Mathf.Asin(sinTheta) * Mathf.Rad2Deg;
                    // Target pitch is based on y-value of desired heading
                    targetPitch = Mathf.Atan(-combinedBehaviourOutput.heading.y / desiredHeadingFlatMagnitude) * Mathf.Rad2Deg;
                    // Limit the pitch to within the provided constraints
                    if (targetPitch < -maxPitchAngle) { targetPitch = -maxPitchAngle; }
                    else if (targetPitch > maxPitchAngle) { targetPitch = maxPitchAngle; }

                    // Ship yaw calculations
                    // Calculate the sine of the yaw delta angle between our desired forward direction and our current forward direction
                    sinTheta = Vector3.Cross(desiredHeadingFlat / desiredHeadingFlatMagnitude, shipForwardFlat).y;
                    // Clamp sinTheta between -1 and 1
                    if (sinTheta > 1f) { sinTheta = 1f; }
                    else if (sinTheta < -1f) { sinTheta = -1f; }
                    // Use arcsin to determine the actual angle
                    currentYaw = Mathf.Asin(sinTheta) * Mathf.Rad2Deg;
                    // If heading is in opposite direction to ship forwards, adjust angle (as arcsine will give wrong angle)
                    if (Vector3.Dot(desiredHeadingFlat, shipForwardFlat) < 0f)
                    {
                        currentYaw = currentYaw > 0f ? 180f - currentYaw : currentYaw - 180f;
                    }
                    // Target yaw is zero (as it is measured relative to the desired pitch)
                    targetYaw = 0f;

                    // Calculate the sine of the roll delta angle between our current up direction and our desired up direction
                    sinTheta = Vector3.Cross(Vector3.up, desiredUpLocalSpaceXYPlane).z;
                    // Clamp sinTheta between -1 and 1
                    if (sinTheta > 1f) { sinTheta = 1f; }
                    else if (sinTheta < -1f) { sinTheta = -1f; }
                    // Use arcsin to determine the actual angle
                    currentRoll = Mathf.Asin(sinTheta) * Mathf.Rad2Deg;
                    if (movementAlgorithm == AIMovementAlgorithm.PlanarFlight)
                    {
                        // Target roll is zero (as it is measured relative to the desired pitch)
                        targetRoll = 0f;
                    }
                    else if (movementAlgorithm == AIMovementAlgorithm.PlanarFlightBanking)
                    {
                        // Target roll delta should be based on our current yaw
                        //targetRoll = -currentYaw * 0.2f;

                        targetRoll = (-currentYaw / maxBankTurnAngle) * maxBankAngle;
                        if (targetRoll < -maxBankAngle) { targetRoll = -maxBankAngle; }
                        else if (targetRoll > maxBankAngle) { targetRoll = maxBankAngle; }
                    }
                }
                else
                {
                    // Only do yaw calculations if there is some bias towards yaw,
                    // OR if we have a specified up direction (since then we will want to yaw no matter what)
                    if (upDirectionSpecified || rollBias < 1f)
                    {
                        // Calculate the sine of the yaw delta angle between our current forward direction and our desired forward direction
                        sinTheta = Vector3.Cross(desiredHeadingLocalSpaceXZPlane, Vector3.forward).y;
                        // Clamp sinTheta between -1 and 1
                        if (sinTheta > 1f) { sinTheta = 1f; }
                        else if (sinTheta < -1f) { sinTheta = -1f; }
                        // Use arcsin to determine the actual angle
                        currentYaw = Mathf.Asin(sinTheta) * Mathf.Rad2Deg;
                        // If heading is in opposite direction to ship forwards, adjust angle (as arcsine will give wrong angle)
                        if (desiredHeadingLocalSpaceXZPlane.z < 0f)
                        {
                            currentYaw = currentYaw > 0f ? 180f - currentYaw : -180f - currentYaw;
                        }
                    }
                    // Target yaw is zero (as it is measured relative to the desired yaw)
                    targetYaw = 0f;

                    // Only do roll calculations if there is some bias towards roll,
                    // OR if we have a specified up direction (since then we will want to roll no matter what)
                    if (upDirectionSpecified || rollBias > 0f)
                    {
                        // Calculate the sine of the roll delta angle between our current up direction and our desired up direction
                        sinTheta = Vector3.Cross(Vector3.up, desiredUpLocalSpaceXYPlane).z;
                        // Clamp sinTheta between -1 and 1
                        if (sinTheta > 1f) { sinTheta = 1f; }
                        else if (sinTheta < -1f) { sinTheta = -1f; }
                        // Use arcsin to determine the actual angle
                        currentRoll = Mathf.Asin(sinTheta) * Mathf.Rad2Deg;
                        // If up direction is in opposite direction to ship upwards, adjust angle (as arcsine will give wrong angle)
                        if (desiredUpLocalSpaceXYPlane.y < 0f)
                        {
                            currentRoll = currentRoll > 0f ? 180f - currentRoll : -180f - currentRoll;
                        }
                    }
                    // Target roll is zero (as it is measured relative to the desired pitch)
                    targetRoll = 0f;

                    // Decide whether we will use pitch to steer (this is only needed if we have no specified up direction)
                    // Here we will also choose whether we will use roll or yaw to steer with (we only want to use one at a time
                    // when there is no specified up direction)
                    bool usePitchToSteer = true;
                    if (!upDirectionSpecified)
                    {
                        // Bias: 0-1: 0 = full yaw, 1 = full roll, 0.5 = no bias
                        bool chooseRollToSteer = false;
                        float currentTurningValue = 0f;

                        // If roll bias is zero, always choose yaw to steer with
                        if (rollBias < 0.001f) { chooseRollToSteer = false; }
                        // If roll bias is one, always choose roll to steer with
                        else if (rollBias > 0.999f) { chooseRollToSteer = true; }
                        else
                        {
                            // Otherwise, calculate a bias value
                            float KValue = 0.082085f * Mathf.Exp(5f * rollBias);
                            // Choose roll or yaw: Whichever has the shortest angle to turn through
                            // (adjusted by roll bias)
                            if (Mathf.Abs(currentYaw) * KValue > Mathf.Abs(currentRoll)) { chooseRollToSteer = true; }
                            else { chooseRollToSteer = false; }
                        }

                        // Roll was chosen to steer with
                        if (chooseRollToSteer) { currentYaw = 0f; currentTurningValue = currentRoll; }
                        // Yaw was chosen to steer with
                        else { currentRoll = 0f; currentTurningValue = currentYaw; }

                        // Only use pitch to steer when we are within turnPitchThreshold degrees of the correct yaw/roll angle
                        usePitchToSteer = currentTurningValue > -turnPitchThreshold && currentTurningValue < turnPitchThreshold;
                    }

                    // Only use pitch to steer when we are within turnPitchThreshold degrees of the correct yaw/roll angle
                    // OR if we have a specified up direction (since then we will want to pitch no matter what)
                    if (usePitchToSteer)
                    {
                        // Ship pitch calculations
                        // Calculate the sine of the pitch delta angle between our current up direction and our desired up direction
                        sinTheta = Vector3.Cross(desiredUpLocalSpaceYZPlane, Vector3.up).x;
                        // Clamp sinTheta between -1 and 1
                        if (sinTheta > 1f) { sinTheta = 1f; }
                        else if (sinTheta < -1f) { sinTheta = -1f; }
                        // Use arcsin to determine the actual angle
                        currentPitch = Mathf.Asin(sinTheta) * Mathf.Rad2Deg;
                        // If the up direction is in opposite direction to ship upwards, adjust angle (as arcsine will give wrong angle)
                        if (desiredUpLocalSpaceYZPlane.y < 0f)
                        {
                            currentPitch = currentPitch > 0f ? 180f - currentPitch : -180f - currentPitch;
                        }
                        // If the heading is in opposite direction to forwards, and we have no specified up direction
                        // OR if the up direction is in opposite direction to ship upwards, and we have a specified up direction,
                        // flip pitch 180 degrees. This is because:
                        // - If we have no specified up direction (we auto-generated one) and the heading is behind us,
                        //   we actually need to flip over with pitch in order to go in the correct direction
                        // - If we have a specified up direction and the up direction is below us, we want to use roll
                        //   instead of pitch to achieve the desired up direction. So we need to flip the pitch,
                        //   since it will be flipped again once we complete the roll.
                        if ((desiredHeadingLocalSpace.z < 0f && !upDirectionSpecified) || 
                            (desiredUpLocalSpaceYZPlane.y < 0f && upDirectionSpecified))
                        {
                            // Adding 180 degrees is because we always want to pitch up not down
                            currentPitch = currentPitch > 0f ? currentPitch - 180f : currentPitch + 180f;
                        }
                        // OLD CODE
                        //// Otherwise, if the up direction is in opposite direction to ship upwards, and we have
                        //// a specified up direction, adjust angle (as arcsine will give wrong angle)
                        //if (desiredUpLocalSpaceYZPlane.y < 0f && upDirectionSpecified)
                        //{
                        //    // Something new to try... adjust angle first!
                        //    currentPitch = currentPitch > 0f ? 180f - currentPitch : -180f - currentPitch;
                        //    // CURRENT BEST
                        //    currentPitch = currentPitch > 0f ? currentPitch - 180f : currentPitch + 180f;
                        //}
                        // Target pitch is zero (as it is measured relative to the desired pitch)
                        targetPitch = 0f;
                    }
                    else
                    {
                        currentPitch = 0f;
                        targetPitch = 0f;
                    }
                }

                // Always calculate yaw input from PID controller
                shipInput.yaw = yawPIDController.RequiredInput(targetYaw, currentYaw, Time.deltaTime);

                if (shipControlModule.shipInstance.IsGrounded)
                {
                    // If ship is grounded, set pitch and roll input to zero
                    shipInput.pitch = 0f;
                    shipInput.roll = 0f;
                }
                else
                {
                    // If ship isn't grounded, calculate pitch and roll input from PID controllers
                    shipInput.pitch = pitchPIDController.RequiredInput(targetPitch, currentPitch, Time.deltaTime);
                    shipInput.roll = rollPIDController.RequiredInput(targetRoll, currentRoll, Time.deltaTime);
                }

                #endregion

                #region Translational Input

                // Transform steering vectors into local space
                desiredLocalVelocity = shipControlModule.shipInstance.TransformInverseRotation * combinedBehaviourOutput.velocity;
                currentLocalVelocity = shipControlModule.shipInstance.TransformInverseRotation * shipControlModule.shipInstance.WorldVelocity;

                shipInput.horizontal = horizontalPIDController.RequiredInput(desiredLocalVelocity.x, currentLocalVelocity.x, Time.deltaTime);
                shipInput.vertical = verticalPIDController.RequiredInput(desiredLocalVelocity.y, currentLocalVelocity.y, Time.deltaTime);
                shipInput.longitudinal = longitudinalPIDController.RequiredInput(desiredLocalVelocity.z, currentLocalVelocity.z, Time.deltaTime);

                #endregion

                #region Weapons Input

                // TODO remove when satisfied
                //// Only fire if there is a target, it is in range and within the fire angle
                //if (targetShip != null && IsTargetInRange())
                //{
                //    shipInput.primaryFire = Mathf.Abs(currentYaw - targetYaw) < fireAngle && Mathf.Abs(currentPitch - targetPitch) < fireAngle;
                //    shipInput.secondaryFire = shipInput.primaryFire;
                //}
                //else
                //{
                //    shipInput.primaryFire = false;
                //    shipInput.secondaryFire = false;
                //}

                // Default - don't fire
                shipInput.primaryFire = false;
                shipInput.secondaryFire = false;

                // Only fire if there is a target
                if (targetShip != null)
                {
                    Vector3 weaponFirePosition = Vector3.zero;
                    Vector3 weaponFireVelocity = Vector3.forward;

                    // Check if we fired the primary weapon if it would hit the target ship
                    weaponFirePosition = shipControlModule.shipInstance.TransformPosition +
                        (shipControlModule.shipInstance.TransformRotation * primaryFireWeaponRelativePosition);
                    // If using turrets, always fire
                    if (primaryFireUsesTurrets)
                    {
                        shipInput.primaryFire = true;
                    }
                    // If not using turrets, projectiles will be fired from weapon direction
                    else
                    {
                        weaponFireVelocity = (shipControlModule.shipInstance.TransformRotation * primaryFireWeaponDirection) * primaryFireProjectileSpeed;
                        // TODO ship radius - how do we get this for the other ship?
                        // (currently just uses 5 * radius of our ship)
                        shipInput.primaryFire = AIBehaviourInput.OnCollisionCourse(shipControlModule.shipInstance.TransformPosition,
                            shipControlModule.shipInstance.WorldVelocity + weaponFireVelocity, 0f, targetShip.TransformPosition,
                            targetShip.WorldVelocity, shipRadius * 5f, primaryFireProjectileDespawnTime);
                    }

                    // Check if we fired the secondary weapon if it would hit the target ship
                    weaponFirePosition = shipControlModule.shipInstance.TransformPosition +
                        (shipControlModule.shipInstance.TransformRotation * secondaryFireWeaponRelativePosition);
                    // If using turrets, always fire
                    if (secondaryFireUsesTurrets)
                    {
                        shipInput.secondaryFire = true;
                    }
                    // If not using turrets, projectiles will be fired from weapon direction
                    else
                    {
                        weaponFireVelocity = (shipControlModule.shipInstance.TransformRotation * secondaryFireWeaponDirection) * secondaryFireProjectileSpeed;
                        // TODO ship radius - how do we get this for the other ship?
                        // (currently just uses 5 * radius of our ship)
                        shipInput.secondaryFire = AIBehaviourInput.OnCollisionCourse(shipControlModule.shipInstance.TransformPosition,
                            shipControlModule.shipInstance.WorldVelocity + weaponFireVelocity, 0f, targetShip.TransformPosition,
                            targetShip.WorldVelocity, shipRadius * 5f, secondaryFireProjectileDespawnTime);
                    }
                }

                // Strafing run with target as a position
                if (currentState.id == AIState.strafingRunStateID)
                {
                    Vector3 weaponFirePosition = Vector3.zero;
                    Vector3 weaponFireVelocity = Vector3.forward;

                    // Check if we fired the primary weapon if it would hit the target
                    weaponFirePosition = shipControlModule.shipInstance.TransformPosition +
                        (shipControlModule.shipInstance.TransformRotation * primaryFireWeaponRelativePosition);
                    // If using turrets, always fire
                    if (primaryFireUsesTurrets)
                    {
                        shipInput.primaryFire = true;
                    }
                    // If not using turrets, projectiles will be fired from weapon direction
                    else
                    {
                        weaponFireVelocity = (shipControlModule.shipInstance.TransformRotation * primaryFireWeaponDirection) * primaryFireProjectileSpeed;
                        // TODO target radius - how do we get this for the target?
                        // (currently just uses 5 * radius of our ship)
                        shipInput.primaryFire = AIBehaviourInput.OnCollisionCourse(shipControlModule.shipInstance.TransformPosition,
                            shipControlModule.shipInstance.WorldVelocity + weaponFireVelocity, 0f, targetPosition,
                            Vector3.zero, shipRadius * 5f, primaryFireProjectileDespawnTime);
                    }

                    // Check if we fired the secondary weapon if it would hit the target
                    weaponFirePosition = shipControlModule.shipInstance.TransformPosition +
                        (shipControlModule.shipInstance.TransformRotation * secondaryFireWeaponRelativePosition);
                    // If using turrets, always fire
                    if (secondaryFireUsesTurrets)
                    {
                        shipInput.secondaryFire = true;
                    }
                    // If not using turrets, projectiles will be fired from weapon direction
                    else
                    {
                        weaponFireVelocity = (shipControlModule.shipInstance.TransformRotation * secondaryFireWeaponDirection) * secondaryFireProjectileSpeed;
                        // TODO target radius - how do we get this for the target?
                        // (currently just uses 5 * radius of our ship)
                        shipInput.secondaryFire = AIBehaviourInput.OnCollisionCourse(shipControlModule.shipInstance.TransformPosition,
                            shipControlModule.shipInstance.WorldVelocity + weaponFireVelocity, 0f, targetPosition,
                            Vector3.zero, shipRadius * 5f, secondaryFireProjectileDespawnTime);
                    }
                }

                #endregion

                #endregion

                #region Send Calculated Ship Input

                // Send the calculated input to the ship
                shipControlModule.SendInput(shipInput);

                #endregion
            }
        }

        #endregion

        #region Private Methods

        #region Private Methods - General

        /// <summary>
        /// Enable or disable the AI. This is different from setting the state to idle.
        /// This will prevent the update loop from performing any calculations or
        /// sending input to the ship.
        /// </summary>
        /// <param name="isEnable"></param>
        private void EnableOrDisableAI(bool isEnable)
        {
            isAIEnabled = isEnable;
        }

        #endregion

        #region Private Methods - Old Steering Wander Behaviour

        ///// <summary>
        ///// Adds a weighted desired steering/heading from the "wander" behaviour.
        ///// </summary>
        ///// <param name="targetPosition"></param>
        ///// <param name="desiredHeadingVector"></param>
        ///// <param name="desiredUpVector"></param>
        ///// <param name="steeringVector"></param>
        ///// <param name="behaviourWeighting"></param>
        //private void AddWander(float wanderStrength, float wanderRate, ref Vector3 desiredHeadingVector, ref Vector3 desiredUpVector,
        //    ref Vector3 steeringVector, float behaviourWeighting)
        //{
        //    currentWanderDirection += UnityEngine.Random.onUnitSphere * wanderRate * Time.deltaTime;
        //    currentWanderDirection *= wanderStrength / currentWanderDirection.magnitude;

        //    headingVector = shipControlModule.shipInstance.TransformRotation * currentWanderDirection;
        //    headingVector += 10f * shipControlModule.shipInstance.TransformForward;

        //    // Desired heading is towards the target position
        //    headingVectorNormalised = headingVector.normalized;
        //    desiredHeadingVector += headingVectorNormalised * behaviourWeighting;
        //    // No desired upwards orientation
        //    // Steering vector is desired velocity minus current velocity, desired velocity is in the direction of desired heading
        //    //steeringVector += ((maxSpeed * headingVectorNormalised) - shipControlModule.shipInstance.WorldVelocity) * behaviourWeighting;
        //    steeringVector += headingVectorNormalised * 10f * behaviourWeighting;
        //}

        #endregion

        #region Private Methods - Set Behaviour

        /// <summary>
        /// Sets an AIBehaviourOutput using a specified AIBehaviourInput. If the resulting AIBehaviourOutput has
        /// use targeting accuracy enabled, will apply targeting accuracy to the heading.
        /// </summary>
        /// <param name="aiBehaviourInput"></param>
        /// <param name="aiBehaviourOutput"></param>
        private void SetBehaviourOutput(AIBehaviourInput aiBehaviourInput, AIBehaviourOutput aiBehaviourOutput)
        {
            switch (aiBehaviourInput.behaviourType)
            {
                case AIBehaviourInput.AIBehaviourType.Idle:
                    AIBehaviourInput.SetIdleBehaviourOutput(aiBehaviourInput, aiBehaviourOutput);
                    break;
                case AIBehaviourInput.AIBehaviourType.Seek:
                    AIBehaviourInput.SetSeekBehaviourOutput(aiBehaviourInput, aiBehaviourOutput);
                    break;
                case AIBehaviourInput.AIBehaviourType.Flee:
                    AIBehaviourInput.SetFleeBehaviourOutput(aiBehaviourInput, aiBehaviourOutput);
                    break;
                case AIBehaviourInput.AIBehaviourType.Pursuit:
                    AIBehaviourInput.SetPursuitBehaviourOutput(aiBehaviourInput, aiBehaviourOutput);
                    break;
                case AIBehaviourInput.AIBehaviourType.Evasion:
                    AIBehaviourInput.SetEvasionBehaviourOutput(aiBehaviourInput, aiBehaviourOutput);
                    break;
                case AIBehaviourInput.AIBehaviourType.SeekArrival:
                    AIBehaviourInput.SetSeekArrivalBehaviourOutput(aiBehaviourInput, aiBehaviourOutput);
                    break;
                case AIBehaviourInput.AIBehaviourType.SeekMovingArrival:
                    AIBehaviourInput.SetSeekMovingArrivalBehaviourOutput(aiBehaviourInput, aiBehaviourOutput);
                    break;
                case AIBehaviourInput.AIBehaviourType.PursuitArrival:
                    AIBehaviourInput.SetPursuitArrivalBehaviourOutput(aiBehaviourInput, aiBehaviourOutput);
                    break;
                //case AIBehaviourInput.AIBehaviourType.Follow:
                //    AIBehaviourInput.SetFollowInputBehaviour(aiBehaviourInput);
                //    break;
                //case AIBehaviourInput.AIBehaviourType.Avoid:
                //    AIBehaviourInput.SetAvoidInputBehaviour(aiBehaviourInput);
                //    break;
                case AIBehaviourInput.AIBehaviourType.UnblockCylinder:
                    AIBehaviourInput.SetUnblockCylinderBehaviourOutput(aiBehaviourInput, aiBehaviourOutput);
                    break;
                case AIBehaviourInput.AIBehaviourType.UnblockCone:
                    AIBehaviourInput.SetUnblockConeBehaviourOutput(aiBehaviourInput, aiBehaviourOutput);
                    break;
                case AIBehaviourInput.AIBehaviourType.ObstacleAvoidance:
                    AIBehaviourInput.SetObstacleAvoidanceBehaviourOutput(aiBehaviourInput, aiBehaviourOutput);
                    break;
                case AIBehaviourInput.AIBehaviourType.FollowPath:
                    AIBehaviourInput.SetFollowPathBehaviourOutput(aiBehaviourInput, aiBehaviourOutput);
                    break;
                case AIBehaviourInput.AIBehaviourType.Dock:
                    AIBehaviourInput.SetDockBehaviourOutput(aiBehaviourInput, aiBehaviourOutput);
                    break;
                case AIBehaviourInput.AIBehaviourType.CustomIdle:
                    if (callbackCustomIdleBehaviour != null) { callbackCustomIdleBehaviour(aiBehaviourInput, aiBehaviourOutput); }
                    else { AIBehaviourInput.SetIdleBehaviourOutput(aiBehaviourInput, aiBehaviourOutput); }
                    break;
                case AIBehaviourInput.AIBehaviourType.CustomSeek:
                    if (callbackCustomSeekBehaviour != null) { callbackCustomSeekBehaviour(aiBehaviourInput, aiBehaviourOutput); }
                    else { AIBehaviourInput.SetSeekBehaviourOutput(aiBehaviourInput, aiBehaviourOutput); }
                    break;
                case AIBehaviourInput.AIBehaviourType.CustomFlee:
                    if (callbackCustomFleeBehaviour != null) { callbackCustomFleeBehaviour(aiBehaviourInput, aiBehaviourOutput); }
                    else { AIBehaviourInput.SetFleeBehaviourOutput(aiBehaviourInput, aiBehaviourOutput); }
                    break;
                case AIBehaviourInput.AIBehaviourType.CustomPursuit:
                    if (callbackCustomPursuitBehaviour != null) { callbackCustomPursuitBehaviour(aiBehaviourInput, aiBehaviourOutput); }
                    else { AIBehaviourInput.SetPursuitBehaviourOutput(aiBehaviourInput, aiBehaviourOutput); }
                    break;
                case AIBehaviourInput.AIBehaviourType.CustomEvasion:
                    if (callbackCustomEvasionBehaviour != null) { callbackCustomEvasionBehaviour(aiBehaviourInput, aiBehaviourOutput); }
                    else { AIBehaviourInput.SetEvasionBehaviourOutput(aiBehaviourInput, aiBehaviourOutput); }
                    break;
                case AIBehaviourInput.AIBehaviourType.CustomSeekArrival:
                    if (callbackCustomSeekArrivalBehaviour != null) { callbackCustomSeekArrivalBehaviour(aiBehaviourInput, aiBehaviourOutput); }
                    else { AIBehaviourInput.SetSeekArrivalBehaviourOutput(aiBehaviourInput, aiBehaviourOutput); }
                    break;
                case AIBehaviourInput.AIBehaviourType.CustomSeekMovingArrival:
                    if (callbackCustomSeekMovingArrivalBehaviour != null) { callbackCustomSeekMovingArrivalBehaviour(aiBehaviourInput, aiBehaviourOutput); }
                    else { AIBehaviourInput.SetSeekMovingArrivalBehaviourOutput(aiBehaviourInput, aiBehaviourOutput); }
                    break;
                case AIBehaviourInput.AIBehaviourType.CustomPursuitArrival:
                    if (callbackCustomPursuitArrivalBehaviour != null) { callbackCustomPursuitArrivalBehaviour(aiBehaviourInput, aiBehaviourOutput); }
                    else { AIBehaviourInput.SetPursuitArrivalBehaviourOutput(aiBehaviourInput, aiBehaviourOutput); }
                    break;
                //case AIBehaviourInput.AIBehaviourType.CustomFollow:
                //    if (callbackCustomFollowBehaviour != null) { callbackCustomFollowBehaviour(aiBehaviourInput); }
                //    else { AIBehaviourInput.SetFollowInputBehaviour(aiBehaviourInput); }
                //    break;
                //case AIBehaviourInput.AIBehaviourType.CustomAvoid:
                //    if (callbackCustomAvoidBehaviour != null) { callbackCustomAvoidBehaviour(aiBehaviourInput); }
                //    else { AIBehaviourInput.SetAvoidInputBehaviour(aiBehaviourInput); }
                //    break;
                case AIBehaviourInput.AIBehaviourType.CustomUnblockCylinder:
                    if (callbackCustomUnblockCylinderBehaviour != null) { callbackCustomUnblockCylinderBehaviour(aiBehaviourInput, aiBehaviourOutput); }
                    else { AIBehaviourInput.SetUnblockCylinderBehaviourOutput(aiBehaviourInput, aiBehaviourOutput); }
                    break;
                case AIBehaviourInput.AIBehaviourType.CustomUnblockCone:
                    if (callbackCustomUnblockConeBehaviour != null) { callbackCustomUnblockConeBehaviour(aiBehaviourInput, aiBehaviourOutput); }
                    else { AIBehaviourInput.SetUnblockConeBehaviourOutput(aiBehaviourInput, aiBehaviourOutput); }
                    break;
                case AIBehaviourInput.AIBehaviourType.CustomObstacleAvoidance:
                    if (callbackCustomObstacleAvoidanceBehaviour != null) { callbackCustomObstacleAvoidanceBehaviour(aiBehaviourInput, aiBehaviourOutput); }
                    else { AIBehaviourInput.SetObstacleAvoidanceBehaviourOutput(aiBehaviourInput, aiBehaviourOutput); }
                    break;
                case AIBehaviourInput.AIBehaviourType.CustomFollowPath:
                    if (callbackCustomFollowPathBehaviour != null) { callbackCustomFollowPathBehaviour(aiBehaviourInput, aiBehaviourOutput); }
                    else { AIBehaviourInput.SetFollowPathBehaviourOutput(aiBehaviourInput, aiBehaviourOutput); }                   
                    break;
                case AIBehaviourInput.AIBehaviourType.CustomDock:
                    if (callbackCustomDockBehaviour != null) { callbackCustomDockBehaviour(aiBehaviourInput, aiBehaviourOutput); }
                    else { AIBehaviourInput.SetDockBehaviourOutput(aiBehaviourInput, aiBehaviourOutput); }
                    break;
                default:
                    AIBehaviourInput.SetIdleBehaviourOutput(aiBehaviourInput, aiBehaviourOutput);
                    break;
            }

            // If the resulting AIBehaviourOutput has use targeting accuracy enabled, apply it to the heading
            if (aiBehaviourOutput.useTargetingAccuracy && targetingAccuracy < 1f)
            {
                // Calculate maximum amount of deviation using target accuracy
                float maxHeadingDeviation = (1f - targetingAccuracy) * 0.1f;

                // Generate two vectors perpendicular to the heading vector
                // TODO would like to ensure continuity - maybe do some check of x and z components of heading?
                Vector3 perpendicularV1 = Vector3.Cross(aiBehaviourOutput.heading, Vector3.up);
                Vector3 perpendicularV2 = Vector3.Cross(aiBehaviourOutput.heading, perpendicularV1);

                // Get the current game time
                float currentGameTime = Time.time;

                // Use the game time to generate two multipliers for the perpendicular components using sine functions
                // TODO want to choose better mathematical functions. Aim is:
                // 1. Maximise seeming randomness of movement
                // 2. Allow the components to both reach zero simultaneously at irregular intervals
                float v1Component = maxHeadingDeviation * Mathf.Sin(currentGameTime);
                float v2Component = maxHeadingDeviation * Mathf.Sin(currentGameTime * 1.13f);

                // Add components to heading
                aiBehaviourOutput.heading += (perpendicularV1 * v1Component) + (perpendicularV2 * v2Component);

                // Re-normalise heading
                aiBehaviourOutput.heading.Normalize();
            }
        }

        #endregion

        #region Private Methods - Combine Behaviour Inputs

        /// <summary>
        /// Combines a list of behaviour inputs into a single output, calculating each output in turn.
        /// </summary>
        /// <param name="combinedBehaviourOutput"></param>
        /// <param name="behaviourInputs"></param>
        /// <param name="behaviourOutputs"></param>
        /// <param name="shipWorldPosition"></param>
        /// <param name="shipWorldVelocity"></param>
        public void CombineBehaviourInputs (AIBehaviourOutput combinedBehaviourOutput, List<AIBehaviourInput> behaviourInputs, 
            List<AIBehaviourOutput> behaviourOutputs, Vector3 shipWorldPosition, Vector3 shipWorldVelocity)
        {
            // Reset the combined input to a "blank" behaviour input
            combinedBehaviourOutput.heading = Vector3.zero;
            combinedBehaviourOutput.up = Vector3.zero;
            combinedBehaviourOutput.velocity = Vector3.zero;
            combinedBehaviourOutput.target = Vector3.zero;
            combinedBehaviourOutput.setTarget = true;

            // If the currentState is not set, get out quickly
            if (currentState == null) { return; }

            // TODO - REMOVE TEST CODE - for NaN desiredLocalVelocity
            //float tempvalue = 0f;
            //int lastBhIdx = -1;

            // Loop through behaviour inputs list
            AIBehaviourInput behaviourInput;
            AIBehaviourOutput behaviourOutput;
            float totalWeighting = 0f;
            for (int i = 0; i < behavioursListCount; i++)
            {
                // Get the current behaviour input and output
                behaviourInput = behaviourInputs[i];
                behaviourOutput = behaviourOutputs[i];
                // Only calculate behaviours that have a non-zero weighting
                if (behaviourInput.weighting > 0f)
                {
                    // Calculate behaviour output
                    SetBehaviourOutput(behaviourInput, behaviourOutput);

                    // Only use behaviours that have a non-zero output
                    if (behaviourOutput.heading.sqrMagnitude > 0.01f)
                    {
                        // TEST CODE for NaN desiredLocalVelocity
                        //tempvalue += bh.headingOutput.sqrMagnitude;
                        //lastBhIdx = i;

                        if (currentState.behaviourCombiner == AIState.BehaviourCombiner.PriorityOnly)
                        {
                            // Priority only - first non-zero behaviour is set as the output
                            combinedBehaviourOutput.heading = behaviourOutput.heading;
                            combinedBehaviourOutput.up = behaviourOutput.up;
                            combinedBehaviourOutput.velocity = behaviourOutput.velocity;
                            combinedBehaviourOutput.target = behaviourOutput.target;
                            combinedBehaviourOutput.setTarget = behaviourOutput.setTarget;
                            // Skip all the rest of the behaviours
                            i = behavioursListCount;

                        }
                        else if (currentState.behaviourCombiner == AIState.BehaviourCombiner.PrioritisedDithering)
                        {
                            // Prioritised dithering - first non-zero behaviour allowed by probability check is set as the output
                            // TODO: Should probably add another parameter (dither probability) instead of just repurposing weighting
                            if (UnityEngine.Random.Range(0f, 1f) < behaviourInput.weighting)
                            {
                                combinedBehaviourOutput.heading = behaviourOutput.heading;
                                combinedBehaviourOutput.up = behaviourOutput.up;
                                combinedBehaviourOutput.velocity = behaviourOutput.velocity;
                                combinedBehaviourOutput.target = behaviourOutput.target;
                                combinedBehaviourOutput.setTarget = behaviourOutput.setTarget;
                                // Skip all the rest of the behaviours
                                i = behavioursListCount;
                            }
                        }
                        else
                        {
                            // Weighted average - output is set as weighted average of all non-zero behaviours
                            // Add weighted heading and up vectors to total
                            combinedBehaviourOutput.heading += behaviourOutput.heading * behaviourInput.weighting;
                            combinedBehaviourOutput.up += behaviourOutput.up * behaviourInput.weighting;
                            // Add weighted velocity delta to total
                            combinedBehaviourOutput.velocity += (behaviourOutput.velocity - shipWorldVelocity) * behaviourInput.weighting;
                            // Add weighted target delta to total
                            combinedBehaviourOutput.target += (behaviourOutput.target - shipWorldPosition) * behaviourInput.weighting;
                            // Add weighting to total
                            totalWeighting += behaviourInput.weighting;
                        }
                    }
                }
            }

            //TEST CODE TO CHECK for NaN on desiredLocalVelocity
            //if (combinedBehaviourInput.velocityOutput == Vector3.zero)
            //{
            //    //if (lastBhIdx == 3)
            //    //{
            //    //    bh = behaviourInputs[3];
            //    //    Debug.Log("[DEBUG] NaN alert on " + gameObject.name + " targetVelocity:" + bh.targetVelocity + " velocityOutput:" + bh.velocityOutput + " headingOutput:" + bh.headingOutput + " btype: " + bh.behaviourType);
            //    //}

            //    Debug.Log("[DEBUG] NaN alert on " + gameObject.name + " tempvalue: " + tempvalue + ", velocityOutput: " + combinedBehaviourInput.velocityOutput + " combiner: " + currentState.behaviourCombiner + " lastBhIdx: " + lastBhIdx);
            //}

            if (currentState.behaviourCombiner == AIState.BehaviourCombiner.WeightedAverage &&
                totalWeighting > 0f)
            {
                // Divide totals by total weighting
                combinedBehaviourOutput.heading /= totalWeighting;
                combinedBehaviourOutput.up /= totalWeighting;
                combinedBehaviourOutput.velocity /= totalWeighting;
                // Add ship world velocity back to behaviour input velocity
                combinedBehaviourOutput.velocity += shipWorldVelocity;
                // Add ship world position back to behaviour input target
                combinedBehaviourOutput.target += shipWorldPosition;
                combinedBehaviourOutput.setTarget = false;
                // Normalise inputs
                combinedBehaviourOutput.NormaliseOutputs();
            }
        }

        #endregion

        #region Private Methods - Target Methods

        /// <summary>
        /// Sets a list of weapons that can be assigned a target. This should be called
        /// if weapon characterists of the ship are changed at runtime. The weapon position
        /// in the ship weaponList are cached in a reusable list to save on GC and improve
        /// performance.
        /// </summary>
        private void SetTargetingWeaponList()
        {
            if (shipControlModule != null && shipControlModule.shipInstance != null)
            {
                int numWeapons = shipControlModule.shipInstance.weaponList == null ? 0 : shipControlModule.shipInstance.weaponList.Count;
                if (targetingWeaponIdxList == null) { targetingWeaponIdxList = new List<int>(numWeapons); }
                else { targetingWeaponIdxList.Clear(); }

                Weapon weapon = null;

                if (targetingWeaponIdxList != null)
                {
                    for (int wpIdx = 0; wpIdx < numWeapons; wpIdx++)
                    {
                        weapon = shipControlModule.shipInstance.weaponList[wpIdx];
                        if (weapon != null)
                        {
                            // Does this look like a weapon that can be assigned a target? (i.e. is it a turret or does
                            // it use guided projectiles, and also does it not have auto targeting enabled, which overrides everything)
                            if (((weapon.weaponType == Weapon.WeaponType.TurretProjectile && weapon.turretPivotY != null && weapon.turretPivotX != null)
                                || weapon.isProjectileKGuideToTarget) && weapon.projectilePrefab != null && !weapon.isAutoTargetingEnabled)
                            {
                                targetingWeaponIdxList.Add(wpIdx);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Public Member API Methods

        #region Enable, Disable AI

        /// <summary>
        /// Prevent the main update loop from performing calculations or sending data to the ship.
        /// This is different from setting the state to AIState.idleStateID.
        /// </summary>
        public void DisableAI()
        {
            EnableOrDisableAI(false);
        }

        /// <summary>
        /// Enable the main update loop to perform calculations and send input to the ship.
        /// </summary>
        public void EnableAI()
        {
            EnableOrDisableAI(true);
        }

        #endregion

        #region Initialisation / Precalculation API Methods

        /// <summary>
        /// Initialises the Ship AI Input Module.
        /// </summary>
        public void Initialise()
        {
            // Don't run if already initialised.
            if (isInitialised) { return; }

            // Find the ship control module that we will send input to
            shipControlModule = GetComponent<ShipControlModule>();
            // If the ship control module has not been initialised yet, initialise it
            shipControlModule.InitialiseShip();

            // Create a new ShipInput instance
            shipInput = new ShipInput();

            // Initialise PID controllers
            pitchPIDController = new PIDController(0.05f, 0f, 0.025f);
            pitchPIDController.SetInputLimits(-1f, 1f);
            yawPIDController = new PIDController(0.05f, 0f, 0.025f);
            yawPIDController.SetInputLimits(-1f, 1f);
            rollPIDController = new PIDController(0.05f, 0f, 0.025f);
            rollPIDController.SetInputLimits(-1f, 1f);
            verticalPIDController = new PIDController(0.1f, 0.05f, 0f);
            verticalPIDController.SetInputLimits(-1f, 1f);
            horizontalPIDController = new PIDController(0.1f, 0.05f, 0f);
            horizontalPIDController.SetInputLimits(-1f, 1f);
            longitudinalPIDController = new PIDController(1f, 0.05f, 0f);
            longitudinalPIDController.SetInputLimits(-1f, 1f);

            SetTargetingWeaponList();

            // Set up behaviour inputs and outputs
            behaviourInputsList = new List<AIBehaviourInput>(behavioursListCount);
            behaviourOutputsList = new List<AIBehaviourOutput>(behavioursListCount);
            for (int i = 0; i < behavioursListCount; i++)
            {
                behaviourInputsList.Add(new AIBehaviourInput(shipControlModule, this));
                behaviourOutputsList.Add(new AIBehaviourOutput());
            }
            combinedBehaviourOutput = new AIBehaviourOutput();

            // Initialise AI State data if it hasn't already been initialised
            AIState.Initialise();
            // Set initial AI state to Idle
            SetState(AIState.idleStateID);
            // Initialise state method parameters with behaviour inputs list and our ship
            stateMethodParameters = new AIStateMethodParameters(behaviourInputsList, shipControlModule, this);

            // Recalculate ship parameters
            RecalculateShipParameters();

            ReinitialiseDiscardData();

            isInitialised = true;

            // Should the main update loop start calculating when module is initialised?
            isAIEnabled = isEnableAIOnInitialise;
        }

        /// <summary>
        /// Recalculates the parameters for the AI's "model" of the ship. Should be called if any of the ship's characteristics
        /// are modified.
        /// </summary>
        public void RecalculateShipParameters ()
        {
            #region Calculate Movement Parameters

            if (shipControlModule.shipInstance.shipPhysicsModel == Ship.ShipPhysicsModel.Arcade)
            {
                #region Arcade

                // Max turning acceleration is based on max flight and ground turning acceleration
                shipMaxFlightTurnAcceleration = shipControlModule.shipInstance.arcadeMaxFlightTurningAcceleration;
                shipMaxGroundTurnAcceleration = shipControlModule.shipInstance.arcadeMaxGroundTurningAcceleration;
                // Max angular acceleration is based on arcade yaw and pitch acceleration
                // Currently I just use the minimum of the two accelerations
                //shipMaxAngularAcceleration = (shipControlModule.shipInstance.arcadeYawAcceleration +
                //    shipControlModule.shipInstance.arcadePitchAcceleration) * 0.375f;
                shipMaxAngularAcceleration = shipControlModule.shipInstance.arcadeYawAcceleration < shipControlModule.shipInstance.arcadePitchAcceleration ?
                    shipControlModule.shipInstance.arcadeYawAcceleration : shipControlModule.shipInstance.arcadePitchAcceleration;

                // Max braking constant deceleration is based on thrusters and arcade brake min acceleration
                shipMaxBrakingConstantDecelerationX = 0f;
                shipMaxBrakingConstantDecelerationY = 0f;
                shipMaxBrakingConstantDecelerationZ = 0f;
                // Loop through the list of thrusters
                int thrusterListCount = shipControlModule.shipInstance.thrusterList.Count;
                Thruster thrusterComponent;
                int LRTurningThrustersCount = 0;
                int UDTurningThrustersCount = 0;
                float LRTurningThrust = 0f;
                float UDTurningThrust = 0f;
                float upBrakingThrust = 0f;
                float downBrakingThrust = 0f;
                float rightBrakingThrust = 0f;
                float leftBrakingThrust = 0f;
                for (int thrusterIndex = 0; thrusterIndex < thrusterListCount; thrusterIndex++)
                {
                    thrusterComponent = shipControlModule.shipInstance.thrusterList[thrusterIndex];
                    // Find any braking thrusters
                    if (thrusterComponent.forceUse == 2)
                    {
                        shipMaxBrakingConstantDecelerationZ += thrusterComponent.maxThrust *
                            -thrusterComponent.thrustDirectionNormalised.z / shipControlModule.shipInstance.mass;
                    }
                    // Find any up/down turning thrusters
                    else if (thrusterComponent.forceUse == 3)
                    {
                        // Up thruster
                        UDTurningThrust += thrusterComponent.maxThrust * thrusterComponent.thrustDirectionNormalised.y;
                        UDTurningThrustersCount++;

                        // Up thrusters can be used to brake on Y axis
                        upBrakingThrust += thrusterComponent.maxThrust * thrusterComponent.thrustDirectionNormalised.y;
                    }
                    else if (thrusterComponent.forceUse == 4)
                    {
                        // Down thruster
                        UDTurningThrust += thrusterComponent.maxThrust * -thrusterComponent.thrustDirectionNormalised.y;
                        UDTurningThrustersCount++;

                        // Down thrusters can be used to brake on Y axis
                        downBrakingThrust += thrusterComponent.maxThrust * -thrusterComponent.thrustDirectionNormalised.y;
                    }
                    // Find any left/right turning thrusters
                    else if (thrusterComponent.forceUse == 5)
                    {
                        // Right thruster
                        LRTurningThrust += thrusterComponent.maxThrust * thrusterComponent.thrustDirectionNormalised.x;
                        LRTurningThrustersCount++;

                        // Right thrusters can be used to brake on X axis
                        rightBrakingThrust += thrusterComponent.maxThrust * thrusterComponent.thrustDirectionNormalised.x;
                    }
                    else if (thrusterComponent.forceUse == 6)
                    {
                        // Left thruster
                        LRTurningThrust += thrusterComponent.maxThrust * -thrusterComponent.thrustDirectionNormalised.x;
                        LRTurningThrustersCount++;

                        // Left thrusters can be used to brake on X axis
                        rightBrakingThrust += thrusterComponent.maxThrust * -thrusterComponent.thrustDirectionNormalised.x;
                    }
                }

                // Max braking constant deceleration X is taken from the minimum braking force in each direction 
                if (rightBrakingThrust >= leftBrakingThrust)
                {
                    shipMaxBrakingConstantDecelerationX = rightBrakingThrust / shipControlModule.shipInstance.mass;
                }
                else
                {
                    shipMaxBrakingConstantDecelerationX = leftBrakingThrust / shipControlModule.shipInstance.mass;
                }

                // Max braking constant deceleration Y is taken from the minimum braking force in each direction 
                if (upBrakingThrust >= downBrakingThrust)
                {
                    shipMaxBrakingConstantDecelerationY = upBrakingThrust / shipControlModule.shipInstance.mass;
                }
                else
                {
                    shipMaxBrakingConstantDecelerationY = downBrakingThrust / shipControlModule.shipInstance.mass;
                }

                // Max braking effective drag coefficient is based on arcade brake strength
                if (shipControlModule.shipInstance.arcadeUseBrakeComponent)
                {
                    shipMaxBrakingConstantDecelerationZ += shipControlModule.shipInstance.arcadeBrakeMinAcceleration;
                    shipMaxBrakingEffectiveDragCoefficientZ = shipControlModule.shipInstance.arcadeBrakeStrength * 0.5f;
                    if (!shipControlModule.shipInstance.arcadeBrakeIgnoreMediumDensity)
                    {
                        shipMaxBrakingEffectiveDragCoefficientZ *= shipControlModule.shipInstance.mediumDensity;
                    }
                    // Need to divide drag coefficient by mass to get acceleration
                    shipMaxBrakingEffectiveDragCoefficientZ /= shipControlModule.shipInstance.mass;
                }
                else { shipMaxBrakingEffectiveDragCoefficientZ = 0f; }

                // Add any up/down/left/right turning thrust found to flight turn acceleration
                if (UDTurningThrustersCount + LRTurningThrustersCount > 0)
                {
                    shipMaxFlightTurnAcceleration += ((UDTurningThrust + LRTurningThrust) / 
                        (UDTurningThrustersCount + LRTurningThrustersCount)) / shipControlModule.shipInstance.mass;
                }
                // Add any left/right turning thrust found to ground turn acceleration
                if (UDTurningThrustersCount > 0)
                {
                    shipMaxGroundTurnAcceleration += (LRTurningThrust / LRTurningThrustersCount) / shipControlModule.shipInstance.mass;
                }

                // Make sure flight and ground turn accelerations are a minimum of 50 m/s^2
                if (shipMaxFlightTurnAcceleration < 50f) { shipMaxFlightTurnAcceleration = 50f; }
                if (shipMaxGroundTurnAcceleration < 50f) { shipMaxGroundTurnAcceleration = 50f; }

                #endregion
            }
            else
            {
                #region Physics-Based

                // TODO: will probably be able to calculate the following parameters
                // (shipMaxFlightTurnAcceleration, shipMaxGroundTurnAcceleration, shipMaxAngularAcceleration)
                // more accurately after we have done the physics-based update
                // shipMaxBrakingConstantDeceleration, shipMaxBrakingEffectiveDragCoefficient are probably correct already though

                // Loop through the list of thrusters
                int thrusterListCount = shipControlModule.shipInstance.thrusterList.Count;
                Thruster thrusterComponent;
                shipMaxBrakingConstantDecelerationX = 0f;
                shipMaxBrakingConstantDecelerationY = 0f;
                shipMaxBrakingConstantDecelerationZ = 0f;
                float shipMaxVerticalAcceleration = 0f;
                float shipMaxHorizontalAcceleration = 0f;
                float shipMaxPitchAngularAcceleration = 0f;
                float shipMaxYawAngularAcceleration = 0f;
                float shipMaxRollAngularAcceleration = 0f;
                float shipAveragePitchThrottleTime = 0f;
                float shipAverageYawThrottleTime = 0f;
                float shipAverageRollThrottleTime = 0f;
                float upBrakingThrust = 0f;
                float downBrakingThrust = 0f;
                float rightBrakingThrust = 0f;
                float leftBrakingThrust = 0f;
                for (int thrusterIndex = 0; thrusterIndex < thrusterListCount; thrusterIndex++)
                {
                    thrusterComponent = shipControlModule.shipInstance.thrusterList[thrusterIndex];
                    // Max braking constant deceleration is based on reverse thrusters
                    if (thrusterComponent.forceUse == 2)
                    {
                        shipMaxBrakingConstantDecelerationZ += thrusterComponent.maxThrust * 
                            -thrusterComponent.thrustDirectionNormalised.z / shipControlModule.shipInstance.mass;
                    }
                    else if (thrusterComponent.forceUse == 3)
                    {
                        shipMaxVerticalAcceleration += thrusterComponent.maxThrust *
                            thrusterComponent.thrustDirectionNormalised.y / shipControlModule.shipInstance.mass;

                        // Up thrusters can be used to brake on Y axis
                        upBrakingThrust += thrusterComponent.maxThrust * thrusterComponent.thrustDirectionNormalised.y;
                    }
                    else if (thrusterComponent.forceUse == 4)
                    {
                        shipMaxVerticalAcceleration += thrusterComponent.maxThrust *
                            -thrusterComponent.thrustDirectionNormalised.y / shipControlModule.shipInstance.mass;

                        // Down thrusters can be used to brake on Y axis
                        downBrakingThrust += thrusterComponent.maxThrust * -thrusterComponent.thrustDirectionNormalised.y;
                    }
                    else if (thrusterComponent.forceUse == 5)
                    {
                        shipMaxHorizontalAcceleration += thrusterComponent.maxThrust *
                            thrusterComponent.thrustDirectionNormalised.x / shipControlModule.shipInstance.mass;

                        // Right thrusters can be used to brake on X axis
                        rightBrakingThrust += thrusterComponent.maxThrust * thrusterComponent.thrustDirectionNormalised.x;
                    }
                    else if (thrusterComponent.forceUse == 6)
                    {
                        shipMaxHorizontalAcceleration += thrusterComponent.maxThrust *
                            -thrusterComponent.thrustDirectionNormalised.x / shipControlModule.shipInstance.mass;

                        // Left thrusters can be used to brake on X axis
                        leftBrakingThrust += thrusterComponent.maxThrust * -thrusterComponent.thrustDirectionNormalised.x;
                    }

                    // The thruster angular acceleration is equal to the torque caused by this thurster divided by the moment of inertia on this axis
                    Vector3 thrusterAngularAcceleration = Vector3.Cross(thrusterComponent.relativePosition - shipControlModule.shipInstance.centreOfMass,
                            thrusterComponent.maxThrust * thrusterComponent.thrustDirectionNormalised) / 
                            Mathf.Abs(Vector3.Dot(shipControlModule.ShipRigidbody.inertiaTensor, thrusterComponent.thrustDirectionNormalised));

                    // Pitch thrusters
                    if (thrusterComponent.primaryMomentUse == 3 || thrusterComponent.secondaryMomentUse == 3)
                    {
                        shipMaxPitchAngularAcceleration += thrusterAngularAcceleration.x;
                        // Weight the average throttle time by the angular acceleration of the thruster
                        shipAveragePitchThrottleTime += (thrusterComponent.rampUpDuration > thrusterComponent.rampDownDuration ?
                            thrusterComponent.rampUpDuration : thrusterComponent.rampDownDuration) * thrusterAngularAcceleration.x;
                    }
                    else if (thrusterComponent.primaryMomentUse == 4 || thrusterComponent.secondaryMomentUse == 4)
                    {
                        shipMaxPitchAngularAcceleration -= thrusterAngularAcceleration.x;
                        // Weight the average throttle time by the angular acceleration of the thruster
                        shipAveragePitchThrottleTime += (thrusterComponent.rampUpDuration > thrusterComponent.rampDownDuration ?
                            thrusterComponent.rampUpDuration : thrusterComponent.rampDownDuration) * -thrusterAngularAcceleration.x;
                    }
                    // Yaw thrusters
                    if (thrusterComponent.primaryMomentUse == 5 || thrusterComponent.secondaryMomentUse == 5)
                    {
                        shipMaxYawAngularAcceleration += thrusterAngularAcceleration.y;
                        // Weight the average throttle time by the angular acceleration of the thruster
                        shipAverageYawThrottleTime += (thrusterComponent.rampUpDuration > thrusterComponent.rampDownDuration ?
                            thrusterComponent.rampUpDuration : thrusterComponent.rampDownDuration) * thrusterAngularAcceleration.y;
                    }
                    else if (thrusterComponent.primaryMomentUse == 6 || thrusterComponent.secondaryMomentUse == 6)
                    {
                        shipMaxYawAngularAcceleration -= thrusterAngularAcceleration.y;
                        // Weight the average throttle time by the angular acceleration of the thruster
                        shipAverageYawThrottleTime += (thrusterComponent.rampUpDuration > thrusterComponent.rampDownDuration ?
                            thrusterComponent.rampUpDuration : thrusterComponent.rampDownDuration) * -thrusterAngularAcceleration.y;
                    }
                    // Roll thrusters
                    if (thrusterComponent.primaryMomentUse == 1 || thrusterComponent.secondaryMomentUse == 1)
                    {
                        shipMaxRollAngularAcceleration -= thrusterAngularAcceleration.z;
                        // Weight the average throttle time by the angular acceleration of the thruster
                        shipAverageRollThrottleTime += (thrusterComponent.rampUpDuration > thrusterComponent.rampDownDuration ?
                            thrusterComponent.rampUpDuration : thrusterComponent.rampDownDuration) * -thrusterAngularAcceleration.z;
                    }
                    else if (thrusterComponent.primaryMomentUse == 2 || thrusterComponent.secondaryMomentUse == 2)
                    {
                        shipMaxRollAngularAcceleration += thrusterAngularAcceleration.z;
                        // Weight the average throttle time by the angular acceleration of the thruster
                        shipAverageRollThrottleTime += (thrusterComponent.rampUpDuration > thrusterComponent.rampDownDuration ?
                            thrusterComponent.rampUpDuration : thrusterComponent.rampDownDuration) * thrusterAngularAcceleration.z;
                    }
                }
                // Angular accelerations need to be scaled by the allocated pitch/roll/yaw power
                shipMaxPitchAngularAcceleration *= shipControlModule.shipInstance.pitchPower;
                shipMaxRollAngularAcceleration *= shipControlModule.shipInstance.rollPower;
                shipMaxYawAngularAcceleration *= shipControlModule.shipInstance.yawPower;
                // Max flight turn acceleration is based on left/right/up/down thrusters
                // Max ground turn acceleration is based on left/right thrusters
                shipMaxFlightTurnAcceleration = (shipMaxHorizontalAcceleration + shipMaxVerticalAcceleration) * 0.5f;
                shipMaxGroundTurnAcceleration = shipMaxHorizontalAcceleration;
                // Max angular acceleration is based on pitch and yaw thrusters
                // Currently I just use the minimum of the two accelerations
                //shipMaxAngularAcceleration = (shipMaxPitchAngularAcceleration + shipMaxYawAngularAcceleration) * 0.375f;
                shipMaxAngularAcceleration = shipMaxPitchAngularAcceleration < shipMaxYawAngularAcceleration ?
                    shipMaxPitchAngularAcceleration : shipMaxYawAngularAcceleration;
                // Calculate average throttle up/down time for thrusters on each rotational axis
                if (shipMaxPitchAngularAcceleration > 0f) { shipAveragePitchThrottleTime /= shipMaxPitchAngularAcceleration; }
                if (shipMaxYawAngularAcceleration > 0f) { shipAverageYawThrottleTime /= shipMaxYawAngularAcceleration; }
                if (shipMaxRollAngularAcceleration > 0f) { shipAverageRollThrottleTime /= shipMaxRollAngularAcceleration; }

                // Set individual input limits for the PID rotation controllers
                pitchPIDController.useIndividualInputLimits = true;
                pitchPIDController.SetIndividualInputLimits(-1f, 1f, -1f, 1f, -2f, 2f);
                yawPIDController.useIndividualInputLimits = true;
                yawPIDController.SetIndividualInputLimits(-1f, 1f, -1f, 1f, -2f, 2f);
                rollPIDController.useIndividualInputLimits = true;
                rollPIDController.SetIndividualInputLimits(-1f, 1f, -1f, 1f, -2f, 2f);
                // Calculate proportional derivative parameters for the PID rotation controllers
                pitchPIDController.pGain = 0.05f;
                pitchPIDController.dGain = 2f * Mathf.Sqrt(pitchPIDController.pGain * 0.5f / shipMaxPitchAngularAcceleration) *
                    (1f + shipAveragePitchThrottleTime);
                yawPIDController.pGain = 0.05f;
                yawPIDController.dGain = 2f * Mathf.Sqrt(yawPIDController.pGain * 0.5f / shipMaxYawAngularAcceleration) *
                    (1f + shipAverageYawThrottleTime);
                rollPIDController.pGain = 0.05f;
                rollPIDController.dGain = 2f * Mathf.Sqrt(rollPIDController.pGain * 0.5f / shipMaxRollAngularAcceleration) *
                    (1f + shipAverageRollThrottleTime);

                //Debug.Log("Pitch... P-Gain set to " + pitchPIDController.pGain.ToString("0.0000") + ". D-Gain set to " + pitchPIDController.dGain.ToString("0.0000"));
                //Debug.Log("Roll.... P-Gain set to " + rollPIDController.pGain.ToString("0.0000") + ". D-Gain set to " + rollPIDController.dGain.ToString("0.0000"));
                //Debug.Log("Yaw..... P-Gain set to " + yawPIDController.pGain.ToString("0.0000") + ". D-Gain set to " + yawPIDController.dGain.ToString("0.0000"));

                // Max braking constant deceleration X is taken from the minimum braking force in each direction 
                if (rightBrakingThrust >= leftBrakingThrust)
                {
                    shipMaxBrakingConstantDecelerationX = rightBrakingThrust / shipControlModule.shipInstance.mass;
                }
                else
                {
                    shipMaxBrakingConstantDecelerationX = leftBrakingThrust / shipControlModule.shipInstance.mass;
                }

                // Max braking constant deceleration Y is taken from the minimum braking force in each direction 
                if (upBrakingThrust >= downBrakingThrust)
                {
                    shipMaxBrakingConstantDecelerationY = upBrakingThrust / shipControlModule.shipInstance.mass;
                }
                else
                {
                    shipMaxBrakingConstantDecelerationY = downBrakingThrust / shipControlModule.shipInstance.mass;
                }

                // Max braking effective drag coefficient is based on air brake control surfaces
                shipMaxBrakingEffectiveDragCoefficientZ = 0f;
                // Loop through the list of control surfaces
                int controlSurfaceListCount = shipControlModule.shipInstance.controlSurfaceList.Count;
                ControlSurface controlSurfaceComponent;
                for (int controlSurfaceIndex = 0; controlSurfaceIndex < controlSurfaceListCount; controlSurfaceIndex++)
                {
                    controlSurfaceComponent = shipControlModule.shipInstance.controlSurfaceList[controlSurfaceIndex];
                    if (controlSurfaceComponent.type == ControlSurface.ControlSurfaceType.AirBrake)
                    {
                        shipMaxBrakingEffectiveDragCoefficientZ += 0.5f * shipControlModule.shipInstance.mediumDensity *
                            controlSurfaceComponent.chord * controlSurfaceComponent.span * 2f / shipControlModule.shipInstance.mass;
                    }
                }

                #endregion
            }

            #endregion

            #region Calculate Combat Parameters

            /// TODO - consider beam weapons....

            // Set some default values in case no relevant weapons are found
            primaryFireProjectileSpeed = 0f;
            primaryFireProjectileDespawnTime = 0f;
            primaryFireUsesTurrets = false;
            secondaryFireProjectileSpeed = 0f;
            secondaryFireProjectileDespawnTime = 0f;
            secondaryFireUsesTurrets = false;

            // Loop through the list of weapons
            int weaponListCount = shipControlModule.shipInstance.weaponList.Count;
            Weapon weaponComponent;
            ProjectileModule weaponProjectilePrefab;
            for (int weaponIndex = 0; weaponIndex < weaponListCount; weaponIndex++)
            {
                weaponComponent = shipControlModule.shipInstance.weaponList[weaponIndex];
                // Check that the weapon has a valid projectile prefab
                weaponProjectilePrefab = weaponComponent.projectilePrefab;
                if (weaponProjectilePrefab != null)
                {
                    if (weaponComponent.firingButton == Weapon.FiringButton.Primary)
                    {
                        // Primary fire input weapons
                        // We want to find the weapon with the fastest projectile speed
                        if (weaponProjectilePrefab.startSpeed > primaryFireProjectileSpeed)
                        {
                            primaryFireProjectileSpeed = weaponProjectilePrefab.startSpeed;
                            primaryFireProjectileDespawnTime = weaponProjectilePrefab.despawnTime;
                            primaryFireUsesTurrets = weaponComponent.weaponType == Weapon.WeaponType.TurretProjectile;
                            //primaryFireUsesTurrets = weaponComponent.IsTurretWeapon;
                            primaryFireWeaponDirection = weaponComponent.fireDirectionNormalised;
                            primaryFireWeaponRelativePosition = weaponComponent.relativePosition;
                        }
                    }
                    else if (weaponComponent.firingButton == Weapon.FiringButton.Secondary)
                    {
                        // Seconday fire input weapons
                        // We want to find the weapon with the fastest projectile speed
                        if (weaponProjectilePrefab.startSpeed > secondaryFireProjectileSpeed)
                        {
                            secondaryFireProjectileSpeed = weaponProjectilePrefab.startSpeed;
                            secondaryFireProjectileDespawnTime = weaponProjectilePrefab.despawnTime;
                            secondaryFireUsesTurrets = weaponComponent.weaponType == Weapon.WeaponType.TurretProjectile;
                            //secondaryFireUsesTurrets = weaponComponent.IsTurretWeapon;
                            secondaryFireWeaponDirection = weaponComponent.fireDirectionNormalised;
                            secondaryFireWeaponRelativePosition = weaponComponent.relativePosition;
                        }
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// Resets the ship's PID Controllers. Call this if you manually modify the ship's velocity or angular velocity.
        /// </summary>
        public void ResetPIDControllers ()
        {
            // Reset all PID controllers
            pitchPIDController.ResetController();
            yawPIDController.ResetController();
            rollPIDController.ResetController();
            verticalPIDController.ResetController();
            horizontalPIDController.ResetController();
            longitudinalPIDController.ResetController();
        }

        #endregion

        #region Physics API Methods

        /// <summary>
        /// Calculates the maxmimum speed for a ship along a curve.
        /// </summary>
        /// <param name="curveStartingRadius"></param>
        /// <param name="curveEndingRadius"></param>
        /// <param name="curveLength"></param>
        /// <returns></returns>
        public float MaxSpeedAlongCurve (float curveStartingRadius, float curveEndingRadius, float curveLength, bool isGrounded)
        {
            // Calculate the limiting speed based on maximum centripetal acceleration
            float curveStartSpeed = MaxSpeedAlongConstantRadiusCurve(curveStartingRadius, isGrounded);
            float curveEndSpeed = MaxSpeedAlongConstantRadiusCurve(curveEndingRadius, isGrounded);
            // Take into account the time we have to slow down before the middle of the curve
            curveEndSpeed = MaxSpeedFromBrakingDistance(curveEndSpeed, curveLength * 0.5f, Vector3.forward);
            // Take the minimum of curve start and end speeds as the limiting speed based on maximum centripetal acceleration
            float accelerationLimitedSpeed = curveStartSpeed < curveEndSpeed ? curveStartSpeed : curveEndSpeed;
            // Calculate the limiting speed based on maximum angular acceleration
            float angularAccelerationLimitedSpeed = Mathf.Infinity;
            // Only calculate an angular acceleration limited speed if the starting and ending radii are different
            // Otherwise there would be no need for any angular acceleration
            if (curveStartingRadius != curveEndingRadius)
            {
                angularAccelerationLimitedSpeed = MaxSpeedAlongChangingRadiusCurve(curveStartingRadius, curveEndingRadius, curveLength);
            }
            // Return the minimum value of acceleration limited and angular acceleration limited speeds
            return accelerationLimitedSpeed < angularAccelerationLimitedSpeed ? accelerationLimitedSpeed : angularAccelerationLimitedSpeed;
        }

        /// <summary>
        /// Calculates the maximum speed for a ship along a curve of constant radius.
        /// </summary>
        /// <param name="curveRadius"></param>
        /// <returns></returns>
        public float MaxSpeedAlongConstantRadiusCurve (float curveRadius, bool isGrounded)
        {
            // Calculate the limiting speed based on maximum centripetal acceleration
            return isGrounded ? (float)System.Math.Sqrt(shipMaxGroundTurnAcceleration * curveRadius) : (float)System.Math.Sqrt(shipMaxFlightTurnAcceleration * curveRadius);
        }

        /// <summary>
        /// Calculates the maximum speed for a ship along a curve of changing radius.
        /// </summary>
        /// <param name="curveStartingRadius"></param>
        /// <param name="curveEndingRadius"></param>
        /// <param name="curveLength"></param>
        /// <returns></returns>
        public float MaxSpeedAlongChangingRadiusCurve (float curveStartingRadius, float curveEndingRadius, float curveLength)
        {
            // Avoid possible divide by zero errors
            if (curveStartingRadius < 0.01f) { curveStartingRadius = 0.01f; }
            if (curveEndingRadius < 0.01f) { curveEndingRadius = 0.01f; }
            // Assumes that the velocity through the curve remains constant, and calculates the maximum this velocity
            // can be given the maximum angular acceleration of the ship
            float squareVelocity = (curveStartingRadius * shipMaxAngularAcceleration * Mathf.Deg2Rad * curveLength) / ((curveStartingRadius / curveEndingRadius) - 1f);
            return Mathf.Sqrt(squareVelocity > 0f ? squareVelocity : -squareVelocity);
        }

        /// <summary>
        /// Calculates the maximum current speed for a ship given a target speed at a target distance away from its current position,
        /// along a particular (normalised) local velocity direction.
        /// </summary>
        /// <param name="targetSpeed"></param>
        /// <param name="targetDistance"></param>
        /// <param name="localVeloDir">Must be normalised!</param>
        /// <returns></returns>
        public float MaxSpeedFromBrakingDistance (float targetSpeed, float targetDistance, Vector3 localVeloDir)
        {
            // Calculate braking constant deceleration weighted by movement direction
            float shipMaxBrakingConstantDeceleration =
                (localVeloDir.x > 0f ? localVeloDir.x : -localVeloDir.x) * shipMaxBrakingConstantDecelerationX +
                (localVeloDir.y > 0f ? localVeloDir.y : -localVeloDir.y) * shipMaxBrakingConstantDecelerationY +
                (localVeloDir.z > 0f ? localVeloDir.z : -localVeloDir.z) * shipMaxBrakingConstantDecelerationZ;

            if (localVeloDir.sqrMagnitude < Mathf.Epsilon)
            {
                // If the ship is not moving, there will no movement direction vector, so weight all of the braking
                // direction components equally (0.578 = 1/sqrt(3))
                shipMaxBrakingConstantDeceleration = 0.578f * 
                    (shipMaxBrakingConstantDecelerationX + shipMaxBrakingConstantDecelerationY + shipMaxBrakingConstantDecelerationZ);
            }

            // Calculate braking effective drag coefficient weighted by movement direction
            float shipMaxBrakingEffectiveDragCoefficient =
                (localVeloDir.z > 0f ? localVeloDir.z : -localVeloDir.z) * shipMaxBrakingEffectiveDragCoefficientZ;

            if (shipMaxBrakingEffectiveDragCoefficient > 0f) // Test code for 1.2.7 Beta 3a+ to avoid div by 0 error (NaN result)
            {
                // When shipMaxBrakingEffectiveDragCoefficient = 0, then below results in a NaN due to div by 0 error.

                // u = sqrt((e^(2*d*cd) * (a + cd*v^2) - a) / cd)
                return (float)System.Math.Sqrt(((float)System.Math.Exp(2f * targetDistance * shipMaxBrakingEffectiveDragCoefficient) *
                    (shipMaxBrakingConstantDeceleration + (shipMaxBrakingEffectiveDragCoefficient * targetSpeed * targetSpeed)) -
                    shipMaxBrakingConstantDeceleration) / shipMaxBrakingEffectiveDragCoefficient);
            }
            else
            {
                // v^2 - u^2 = 2*a*d => u = sqrt(v^2 - 2*a*d)
                // NOTE: 2*a*d is positive in the code as deceleration is the negative of acceleration
                //return (float)System.Math.Sqrt((targetSpeed * targetSpeed) + (2f * shipMaxBrakingConstantDeceleration * targetDistance));

                float _maxSpeed = (float)System.Math.Sqrt((targetSpeed * targetSpeed) + (2f * shipMaxBrakingConstantDeceleration * targetDistance));

                if (_maxSpeed == 0f && targetDistance > 0.001f)
                {
                    Debug.LogWarning("ERROR MaxSpeedFromBrakingDistance - targetSpeed: " + targetSpeed + " targetDistance: " + targetDistance + " shipMaxBrakingConstantDeceleration: " + shipMaxBrakingConstantDeceleration + " on " + transform.name + " T:" + Time.time);
                    //Debug.LogWarning("X: " + shipMaxBrakingConstantDecelerationX + " Y: " + shipMaxBrakingConstantDecelerationY + " Z: " + shipMaxBrakingConstantDecelerationZ);
                    return 1f;
                }
                else { return _maxSpeed; }
            }
        }

        /// <summary>
        /// Calculates the distance required to slow down from the current speed to the target speed,
        /// along a particular (normalised) local velocity direction.
        /// </summary>
        /// <param name="currentSpeed"></param>
        /// <param name="targetSpeed"></param>
        /// <param name="localVeloDir">Must be normalised!</param>
        /// <returns></returns>
        public float BrakingDistance (float currentSpeed, float targetSpeed, Vector3 localVeloDir)
        {
            // Calculate braking constant deceleration weighted by movement direction
            float shipMaxBrakingConstantDeceleration =
                (localVeloDir.x > 0f ? localVeloDir.x : -localVeloDir.x) * shipMaxBrakingConstantDecelerationX +
                (localVeloDir.y > 0f ? localVeloDir.y : -localVeloDir.y) * shipMaxBrakingConstantDecelerationY +
                (localVeloDir.z > 0f ? localVeloDir.z : -localVeloDir.z) * shipMaxBrakingConstantDecelerationZ;

            // Potentially this should be using localVeloDir.sqrMagnitude < Mathf.Epsilon
            if (localVeloDir == Vector3.zero)
            {
                // If the ship is not moving, there will no movement direction vector, so weight all of the braking
                // direction components equally (0.578 = 1/sqrt(3))
                shipMaxBrakingConstantDeceleration = 0.578f *
                    (shipMaxBrakingConstantDecelerationX + shipMaxBrakingConstantDecelerationY + shipMaxBrakingConstantDecelerationZ);
            }

            // Calculate braking effective drag coefficient weighted by movement direction
            float shipMaxBrakingEffectiveDragCoefficient =
                (localVeloDir.z > 0f ? localVeloDir.z : -localVeloDir.z) * shipMaxBrakingEffectiveDragCoefficientZ;

            if (shipMaxBrakingEffectiveDragCoefficient > 0f)
            {
                // d = ln((a + cd*u^2)/(a + cd*v^2)) / (2 * cd)
                return (float)System.Math.Log((shipMaxBrakingConstantDeceleration + shipMaxBrakingEffectiveDragCoefficient * currentSpeed * currentSpeed) /
                    (shipMaxBrakingConstantDeceleration + shipMaxBrakingEffectiveDragCoefficient * targetSpeed * targetSpeed)) /
                    (2f * shipMaxBrakingEffectiveDragCoefficient);
            }
            else
            {
                // v^2 - u^2 = 2*a*d => d = (v^2 - u^2) / (2*a)
                return (currentSpeed * currentSpeed - targetSpeed * targetSpeed) / (2f * shipMaxBrakingConstantDeceleration);
            }
        }

        #endregion

        #region Behaviour Input Information API Methods

        /// <summary>
        /// Returns the last position to be designated as the target position by the chosen AI behaviour input.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetLastBehaviourInputTarget () { return lastBehaviourInputTarget; }

        #endregion

        #region Assign API Methods

        /// <summary>
        /// Assigns a target path for this AI ship, to be used by the current state.
        /// Sets the current target path location index to the second point or the first point if there is no second point.
        /// </summary>
        /// <param name="target"></param>
        public void AssignTargetPath (PathData pathData)
        {
            targetPath = pathData;
            // Attempt to set the target path location index
            if (pathData != null && pathData.pathLocationDataList != null)
            {
                prevTargetPathLocationIndex = SSCManager.GetFirstAssignedLocationIdx(pathData);
                currentTargetPathLocationIndex = SSCManager.GetNextPathLocationIndex(pathData, prevTargetPathLocationIndex, false);
            }
            // Set the target location and position to nothing
            targetLocation = null;
            targetPosition = Vector3.zero;
        }

        /// <summary>
        /// Assigns a target path for this AI ship, to be used by the current state.
        /// Set the previous and next locations along the path, and the normalised distance between the two locations
        /// where the ship will join the path.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="previousPathLocationIndex"></param>
        /// <param name="nextPathLocationIndex"></param>
        /// <param name="targetPathTValue"></param>
        public void AssignTargetPath (PathData pathData, int previousPathLocationIndex, int nextPathLocationIndex, float targetPathTValue)
        {
            targetPath = pathData;
            // Attempt to set the target path location index
            if (pathData != null && pathData.pathLocationDataList != null)
            {
                prevTargetPathLocationIndex = previousPathLocationIndex;
                currentTargetPathLocationIndex = nextPathLocationIndex;
                currentTargetPathTValue = targetPathTValue;
            }
            // Set the target location and position to nothing
            targetLocation = null;
            targetPosition = Vector3.zero;
        }

        /// <summary>
        /// Assigns a target location for this AI ship, to be used by the current state.
        /// </summary>
        /// <param name="target"></param>
        public void AssignTargetLocation(LocationData locationData)
        {
            targetLocation = locationData;
            // Set the target path and position to nothing
            targetPath = null;
            targetPosition = Vector3.zero;
        }

        /// <summary>
        /// Assigns a target position for this AI ship, to be used by the current state.
        /// </summary>
        /// <param name="targetPositionVector"></param>
        public void AssignTargetPosition (Vector3 targetPositionVector)
        {
            targetPosition = targetPositionVector;
            // Set the target path and location to nothing
            targetLocation = null;
            targetPath = null;
        }

        /// <summary>
        /// Assigns a target rotation for this AI ship, to be used by the current state.
        /// </summary>
        /// <param name="targetRotationQuaternion"></param>
        public void AssignTargetRotation(Quaternion targetRotationQuaternion)
        {
            targetRotation = targetRotationQuaternion;
        }

        /// <summary>
        /// Assigns a target ship for this AI ship, to be used by the current state.
        /// </summary>
        public void AssignTargetShip (ShipControlModule targetShipControlModule)
        {
            if (targetShipControlModule != null)
            {
                targetShip = targetShipControlModule.shipInstance;

                #if UNITY_EDITOR

                if (!targetShipControlModule.IsInitialised)
                {
                    Debug.LogWarning("ShipAIInputModule.AssignTargetShip: Target ship " + targetShipControlModule.gameObject.name +
                        " being assigned to " + gameObject.name + " is not initialised. Some AI features may not work" +
                        " as expected.");
                }

                #endif 

                // Set any applicable weapons to also target this ship
                int numTargettingWeapons = targetingWeaponIdxList == null ? 0 : targetingWeaponIdxList.Count;
                for (int wpIdx = 0; wpIdx < numTargettingWeapons; wpIdx++)
                {
                    Weapon weapon = shipControlModule.shipInstance.weaponList[targetingWeaponIdxList[wpIdx]];
                    if (weapon != null)
                    {
                        weapon.SetTargetShip(targetShipControlModule);

                        //if (targetShipControlModule != null) { weapon.SetTarget(targetShipControlModule.gameObject); }
                        //else { weapon.SetTarget(null); }
                    }
                }
            }
            else { targetShip = null; }
        }

        /// <summary>
        /// Assigns a list of ships to evade for this ship, to be used by the current state.
        /// </summary>
        /// <param name="shipsToEvadeList"></param>
        public void AssignShipsToEvade (List<Ship> shipsToEvadeList)
        {
            // Sets it to a reference of the list
            shipsToEvade = shipsToEvadeList;
        }

        /// <summary>
        /// Assigns a list of surface turrets to evade for this ship, to be used by the current state.
        /// </summary>
        /// <param name="surfaceTurretsToEvadeList"></param>
        public void AssignSurfaceTurretsToEvade(List<SurfaceTurretModule> surfaceTurretsToEvadeList)
        {
            // Sets it to a reference of the list
            surfaceTurretsToEvade = surfaceTurretsToEvadeList;
        }

        /// <summary>
        /// Assigns a target radius for this ship, to be used by the current state.
        /// </summary>
        /// <param name="targetRadius"></param>
        public void AssignTargetRadius(float targetRadius)
        {
            this.targetRadius = targetRadius;
        }

        /// <summary>
        /// Assigns a target distance for this ship, to be used by the current state.
        /// </summary>
        /// <param name="targetDistance"></param>
        public void AssignTargetDistance(float targetDistance)
        {
            this.targetDistance = targetDistance;
        }

        /// <summary>
        /// Assigns a target angular distance for this ship, to be used by the current state.
        /// </summary>
        /// <param name="targetAngularDistance"></param>
        public void AssignTargetAngularDistance(float targetAngularDistance)
        {
            this.targetAngularDistance = targetAngularDistance;
        }

        /// <summary>
        /// Assigns a target time for this ship, to be used by the current state.
        /// </summary>
        /// <param name="targetTime"></param>
        public void AssignTargetTime (float targetTime)
        {
            this.targetTime = targetTime;
        }

        /// <summary>
        /// Assigns a target velocity for this ship, to be used by the current state.
        /// </summary>
        /// <param name="targetVelocity"></param>
        public void AssignTargetVelocity(Vector3 targetVelocity)
        {
            this.targetVelocity = targetVelocity;
        }

        /// <summary>
        /// Sets the current index of the location of the target path the AI ship will head towards.
        /// If the index value has changed, also updates the Previous Target Path Location Index.
        /// </summary>
        /// <param name="newTargetPathLocationIndex"></param>
        public void SetCurrentTargetPathLocationIndex (int newTargetPathLocationIndex)
        {
            if (newTargetPathLocationIndex != currentTargetPathLocationIndex)
            {
                prevTargetPathLocationIndex = currentTargetPathLocationIndex;
            }
            currentTargetPathLocationIndex = newTargetPathLocationIndex;
        }

        /// <summary>
        /// Sets the current index of the location of the target path the AI ship will head towards.
        /// If the index value has changed, also update the Previous Target Path Location Index.
        /// Also sets the time value between the previous location and the current (next) location.
        /// The time value should be between 0.0 and 1.0
        /// </summary>
        /// <param name="newTargetPathLocationIndex"></param>
        /// <param name="newTargetPathLocationTValue"></param>
        public void SetCurrentTargetPathLocationIndex(int newTargetPathLocationIndex, float newTargetPathLocationTValue)
        {
            if (newTargetPathLocationIndex != currentTargetPathLocationIndex)
            {
                prevTargetPathLocationIndex = currentTargetPathLocationIndex;
            }
            currentTargetPathLocationIndex = newTargetPathLocationIndex;
            currentTargetPathTValue = newTargetPathLocationTValue;
        }

        /// <summary>
        /// Sets the previous index of the location of the target path the AI ship is heading away from.
        /// </summary>
        /// <param name="newTargetPathLocationIndex"></param>
        public void SetPreviousTargetPathLocationIndex(int newTargetPathLocationIndex)
        {
            prevTargetPathLocationIndex = newTargetPathLocationIndex;
        }

        /// <summary>
        /// Set the time value between the previous Target Path Location
        /// and the current (next) Location along the Path. The TValue
        /// should be between 0.0 and 1.0.
        /// </summary>
        /// <param name="tValue"></param>
        public void SetCurrentTargetPathTValue(float tValue)
        {
            currentTargetPathTValue = tValue;
        }

        /// <summary>
        /// Sets the current stage index for the current state. This (zero-based) index is used to keep track of what stage
        /// the AI ship has reached in the current state. Typically, this should only be set from inside a state method.
        /// </summary>
        /// <returns></returns>
        public void SetCurrentStateStageIndex(int newStateStageIndex)
        {
            currentStateStageIndex = newStateStageIndex;
        }

        #endregion

        #region Get Target API Methods

        /// <summary>
        /// Gets the currently assigned target path (if any).
        /// </summary>
        /// <returns></returns>
        public PathData GetTargetPath()
        {
            return targetPath;
        }

        /// <summary>
        /// Gets the currently assigned target position (if any).
        /// A returned value of Vector3.Zero indicates it is unassigned.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetTargetPosition()
        {
            return targetPosition;
        }

        /// <summary>
        /// Gets the currently assigned target rotation (if any).
        /// </summary>
        /// <returns></returns>
        public Quaternion GetTargetRotation()
        {
            return targetRotation;
        }

        /// <summary>
        /// Gets the currently assigned target location (if any).
        /// </summary>
        /// <returns></returns>
        public LocationData GetTargetLocation()
        {
            return targetLocation;
        }

        /// <summary>
        /// Gets the currently assigned target ship (if any).
        /// </summary>
        /// <returns></returns>
        public Ship GetTargetShip()
        {
            return targetShip;
        }

        /// <summary>
        /// Gets the currently assigned list of ships to evade (if any).
        /// </summary>
        /// <returns></returns>
        public List<Ship> GetShipsToEvade ()
        {
            return shipsToEvade;
        }

        /// <summary>
        /// Gets the currently assigned list of surface turrets to evade (if any).
        /// </summary>
        /// <returns></returns>
        public List<SurfaceTurretModule> GetSurfaceTurretsToEvade()
        {
            return surfaceTurretsToEvade;
        }

        /// <summary>
        /// Gets the currently assigned target radius.
        /// </summary>
        /// <returns></returns>
        public float GetTargetRadius()
        {
            return targetRadius;
        }

        /// <summary>
        /// Gets the currently assigned target distance.
        /// </summary>
        /// <returns></returns>
        public float GetTargetDistance()
        {
            return targetDistance;
        }

        /// <summary>
        /// Gets the currently assigned target angular distance.
        /// </summary>
        /// <returns></returns>
        public float GetTargetAngularDistance()
        {
            return targetAngularDistance;
        }

        /// <summary>
        /// Gets the currently assigned target time.
        /// </summary>
        /// <returns></returns>
        public float GetTargetTime()
        {
            return targetTime;
        }

        /// <summary>
        /// Gets the currently assigned target velocity.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetTargetVelocity()
        {
            return targetVelocity;
        }

        /// <summary>
        /// Gets the current index of the location of the target path the AI ship will head towards.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentTargetPathLocationIndex()
        {
            return currentTargetPathLocationIndex;
        }

        /// <summary>
        /// Gets the previous index of the location of the target path the AI ship is heading away from.
        /// </summary>
        /// <returns></returns>
        public int GetPreviousTargetPathLocationIndex()
        {
            return prevTargetPathLocationIndex;
        }

        /// <summary>
        /// Get the time value between the previous Target Path Location
        /// and the current (next) Location along the Path. The TValue
        /// should be between 0.0 and 1.0.
        /// </summary>
        /// <returns></returns>
        public float GetCurrentTargetPathTValue()
        {
            return currentTargetPathTValue;
        }

        /// <summary>
        /// Gets the current stage index for the current state. This (zero-based) index is used to keep track of what stage
        /// the AI ship has reached in the current state.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentStateStageIndex()
        {
            return currentStateStageIndex;
        }

        /// <summary>
        /// Returns an enumeration indicating what the current state action for this AI ship is.
        /// </summary>
        /// <returns></returns>
        public AIStateActionInfo GetCurrentStateAction ()
        {
            // Get the current state ID
            int currentStateId = currentState.id;

            // Set the AI state action info accordingly
            AIStateActionInfo aiStateActionInfo = AIStateActionInfo.Custom;
            if (currentStateId == AIState.idleStateID)
            {
                aiStateActionInfo = AIStateActionInfo.Idle;
            }
            else if (currentStateId == AIState.moveToStateID)
            {
                if (targetPath != null) { aiStateActionInfo = AIStateActionInfo.MoveToFollowPath; }
                else if (targetLocation != null) { aiStateActionInfo = AIStateActionInfo.MoveToSeekLocation; }
                else { aiStateActionInfo = AIStateActionInfo.MoveToSeekPosition; }
            }
            else if (currentStateId == AIState.dogfightStateID)
            {
                aiStateActionInfo = AIStateActionInfo.DogfightAttackShip;
            }
            else if (currentStateId == AIState.dockingStateID)
            {
                aiStateActionInfo = AIStateActionInfo.Docking;
            }
            else if (currentStateId == AIState.strafingRunStateID)
            {
                aiStateActionInfo = AIStateActionInfo.StrafingRun;
            }
            else
            {
                aiStateActionInfo = AIStateActionInfo.Custom;
            }

            return aiStateActionInfo;
        }

        #endregion

        #region Input API Methods

        /// <summary>
        /// Re-initialise (set) the shipInput based on the is[axis/button]DataDiscard field settings.
        /// Must be called after each change to any of those fields/variables.
        /// </summary>
        public void ReinitialiseDiscardData()
        {
            shipInput.isHorizontalDataEnabled = !isHorizontalDataDiscarded;
            shipInput.isVerticalDataEnabled = !isVerticalDataDiscarded;
            shipInput.isLongitudinalDataEnabled = !isLongitudinalDataDiscarded;
            shipInput.isPitchDataEnabled = !isPitchDataDiscarded;
            shipInput.isYawDataEnabled = !isYawDataDiscarded;
            shipInput.isRollDataEnabled = !isRollDataDiscarded;
            shipInput.isPrimaryFireDataEnabled = !isPrimaryFireDataDiscarded;
            shipInput.isSecondaryFireDataEnabled = !isSecondaryFireDataDiscarded;
            shipInput.isDockDataEnabled = !isDockDataDiscarded;
        }

        #endregion

        #region State API Methods

        /// <summary>
        /// Sets the current state for this AI ship using the given state ID.
        /// </summary>
        /// <param name="stateID"></param>
        public void SetState (int newStateID)
        {
            int prevStateId = GetState();

            // Get state with the corresponding state ID and set it to the current state object
            currentState = AIState.GetState(newStateID);
            // State action starts as uncompleted
            hasCompletedStateAction = false;
            // Current state stage index starts as zero
            currentStateStageIndex = 0;

            #if UNITY_EDITOR
            // Raise a warning if the state retrieved is null
            if (currentState == null)
            {
                Debug.LogWarning("ERROR: ShipAIInputModule.SetState: " + newStateID + " is not a valid state ID.");
            }
            #endif

            // If required, send a notification of the state change to the developer-supplied custom method
            if (callbackOnStateChange != null) { callbackOnStateChange.Invoke(this, GetState(), prevStateId); }
        }

        /// <summary>
        /// Returns the state ID of the current state for this AI ship.
        /// Returns -1 if the currentState is not set.
        /// To get the instance of the state call
        /// AIState.GetState(shipAIInputMdoule.GetState())
        /// </summary>
        /// <param name="stateID"></param>
        public int GetState ()
        {
            // Return the current state object's state ID
            return currentState != null ? currentState.id : -1;
        }

        /// <summary>
        /// Returns whether the state action has been completed yet.
        /// </summary>
        /// <returns></returns>
        public bool HasCompletedStateAction () { return hasCompletedStateAction; }

        /// <summary>
        /// Set whether the state action has been completed yet. Typically this should only be called from within a state method.
        /// </summary>
        /// <param name="isCompleted"></param>
        public void SetHasCompletedStateAction (bool isCompleted = true) { hasCompletedStateAction = isCompleted; }

        #endregion

        #region Movement API Methods

        // IMPORTANT: If you're looking to fly an AI ship to somewhere look at the
        // SetState(..) and AssignTarget[Path | Location | Position | Ship] API methods
        // Also read the chapter on Ship AI System in the manual.

        /// <summary>
        /// Teleport the (AI) ship to a new location by moving by an amount
        /// in the x, y and z directions. This could be useful if changing
        /// the origin or centre of your world to compensate for float-point
        /// error.
        /// NOTE: This does not alter the current Respawn position.
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="resetVelocity"></param>
        public void TelePort(Vector3 delta, bool resetVelocity)
        {
            // Remeber current situation of the ship
            bool isMovementEnabled = false;

            if (isInitialised) { shipControlModule.DisableShipMovement(); isMovementEnabled = true; }

            // if the TargetPosition is set, update it
            if (targetPosition.x != 0f || targetPosition.y != 0f || targetPosition.z != 0f)
            {
                targetPosition += delta;
            }

            transform.position += delta;
            //shipControlModule.ShipRigidbody.MovePosition(transform.position);

            // If movement was enabled, re-enable it
            if (isMovementEnabled)
            {
                shipControlModule.EnableShipMovement(resetVelocity);
                shipControlModule.shipInstance.UpdatePositionAndMovementData(transform, shipControlModule.ShipRigidbody);
            }
        }

        #endregion

        #endregion
    }
}
