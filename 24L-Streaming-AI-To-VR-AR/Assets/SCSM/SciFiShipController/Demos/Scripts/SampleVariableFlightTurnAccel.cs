using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Sample component to attach to a ship which dynamically updates the Flight Turn Acceleration
    /// of an Arcade ship based on its forward velocity.
    /// WARNING
    /// This is only sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own
    /// namespace. It is subject to change at any time and be overwritten by
    /// SSC updates.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Variable Flight Turn Acceleration")]
    [HelpURL("https://scsmmedia.com/ssc-documentation")]
    public class SampleVariableFlightTurnAccel : MonoBehaviour
    {
        #region Public Variables
        public bool initialiseOnStart = false;

        [Tooltip("The Flght Turn Acceleration at the minimum Speed Threshold")]
        [Range(0.1f, 1000f)] public float lowerFlightTurningAcceleration = 50f;

        [Tooltip("The Flght Turn Acceleration at or above the maximum Speed Threshold")]
        [Range(0.1f, 1000f)] public float upperFlightTurningAcceleration= 300f;

        [Tooltip("The minimum speed, in metres per second, that variable flght turn acceleration comes into effect")]
        [Range(0f, 100f)] public float minSpeedThreshold = 10f;

        [Tooltip("At or above this speed, in metres per second, the upper flight turn acceleration is applied")]
        [Range(0f, 1000f)] public float maxSpeedThreshold = 500f;

        [Tooltip("If this ship has an ShipAIInputModule, the minimum interval between Flight Turn Acceleration changes")]
        [Range(0f, 2f)] public float aiUpdateInterval = 0.5f;

        #endregion

        #region Private Variables - General
        private ShipControlModule shipControlModule = null;
        private ShipAIInputModule shipAIInputModule = null;
        private Ship ship = null;
        private bool isInitialised = false;
        private bool isAIShip = false;
        private float currentFlightTurnAcceleration = 0f;
        private float prevFlightTurnAcceleration = 0f;
        private float aiUpdateTimer = 0f;
        #endregion

        #region Private Initialise Methods

        // Start is called before the first frame update
        private void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Update Methods

        void Update()
        {
            if (isInitialised && shipControlModule.ShipMovementIsEnabled())
            {
                float forwardVelo = ship.LocalVelocity.z;

                // Ensure thresholds make sense
                if (maxSpeedThreshold < minSpeedThreshold) { minSpeedThreshold = maxSpeedThreshold; }
                else if (minSpeedThreshold > maxSpeedThreshold) { maxSpeedThreshold = minSpeedThreshold; }

                // Ensure flight turn acceleration limits make sense
                if (upperFlightTurningAcceleration < lowerFlightTurningAcceleration) { lowerFlightTurningAcceleration = upperFlightTurningAcceleration; }
                else if (lowerFlightTurningAcceleration > upperFlightTurningAcceleration) { upperFlightTurningAcceleration = lowerFlightTurningAcceleration; }

                if (forwardVelo <= minSpeedThreshold) { currentFlightTurnAcceleration = lowerFlightTurningAcceleration; }
                else if (forwardVelo >= maxSpeedThreshold) { currentFlightTurnAcceleration = upperFlightTurningAcceleration; }
                else
                {
                    // Get a 0.0 to 1.0 value for our forward velocity within the working range of the variable turn acceleration
                    float normalisedForwardVelo = SSCMath.Normalise(forwardVelo, minSpeedThreshold, maxSpeedThreshold);

                    // Smooth the effect at each end of the range
                    normalisedForwardVelo = SSCMath.EaseInOutCurve(normalisedForwardVelo);

                    currentFlightTurnAcceleration = lowerFlightTurningAcceleration + (normalisedForwardVelo * (upperFlightTurningAcceleration - lowerFlightTurningAcceleration));
                }

                if (currentFlightTurnAcceleration != prevFlightTurnAcceleration)
                {
                    ship.arcadeMaxFlightTurningAcceleration = currentFlightTurnAcceleration;

                    //Debug.Log("currentFlightTurnAcceleration: " + currentFlightTurnAcceleration);

                    // If this an AI Ship, check if we need to re-calc ship pararmeters.
                    if (isAIShip && shipAIInputModule.IsInitialised && shipAIInputModule.GetState() != AIState.idleStateID)
                    {
                        aiUpdateTimer += Time.deltaTime;

                        if (aiUpdateTimer > aiUpdateInterval)
                        {
                            aiUpdateTimer = 0f;
                            // WARNING: This is not performance optimised to run every frame.
                            shipAIInputModule.RecalculateShipParameters();
                        }
                    }
                }

                // Remember the last value used
                prevFlightTurnAcceleration = currentFlightTurnAcceleration;
            }
        }

        #endregion

        #region Private and Internal Methods

        #endregion

        #region Public Methods

        /// <summary>
        /// Call this from your code if initialiseOnStart is false
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            if (!TryGetComponent(out shipControlModule))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("SampleVariableFlightTurnAccel - could not find the player ship component - this script needs to be attached to your ShipControlModule for your player ship");
                #endif
            }
            else if (!shipControlModule.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("SampleVariableFlightTurnAccel - your player ship is not initialised");
                #endif
            }
            else
            {
                // Get a reference to the shipInstance script
                ship = shipControlModule.shipInstance;

                if (TryGetComponent(out shipAIInputModule))
                {
                    isAIShip = true;
                    #if UNITY_EDITOR
                    Debug.LogWarning("SampleVariableFlightTurnAccel is not optimised for AI Ships and may require performance testing and optimisation");
                    #endif
                }

                isInitialised = true;
            }
        }

        #endregion
    }
}