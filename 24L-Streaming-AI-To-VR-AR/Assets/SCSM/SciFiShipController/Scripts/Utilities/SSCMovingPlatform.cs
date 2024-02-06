using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class for a moving platform.
    /// Similar to the StickyMovingPlatform found in Sticky3D Controller (a character controller by SCSM)
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Utilities/Moving Platform")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SSCMovingPlatform : MonoBehaviour
    {
        #region Enumerations
        public enum MoveUpdateType
        {
            Update = 0,
            FixedUpdate = 1
        }
        #endregion

        #region Public Variables

        /// <summary>
        /// If enabled, Initialise() will be called as soon as Awake() runs. This should be disabled if you want to control
        /// when the SSC Moving Platform is enabled through code.
        /// </summary>
        public bool initialiseOnAwake = false;

        /// <summary>
        /// The update loop or timing to use for moving and/or rotating the platform.
        /// At runtime call SetMoveUpdateType() to change it.
        /// </summary>
        public MoveUpdateType moveUpdateType = MoveUpdateType.Update;

        /// <summary>
        /// Whether the platform moves.
        /// </summary>
        public bool move = true;

        /// <summary>
        /// Use positions relative to the initial gameobject position, rather than
        /// absolute world space positions.
        /// </summary>
        public bool useRelativePositions = false;

        /// <summary>
        /// List of positions the platform will move to (in order). Call Initialise() if you modify this.
        /// </summary>
        public List<Vector3> positions = new List<Vector3>(new Vector3[] { Vector3.zero, Vector3.forward * 5f });

        /// <summary>
        /// Average movement speed of the platform in metres per second.
        /// To update this while the platform is moving, call UpdateAverageMoveSpeed(..).
        /// </summary>
        public float averageMoveSpeed = 5f;

        /// <summary>
        /// The time the platform waits at the first position.
        /// </summary>
        public float startWaitTime = 0f;

        /// <summary>
        /// The time the platform waits at the last position
        /// </summary>
        public float endWaitTime = 0f;

        /// <summary>
        /// The "profile" of the platform's movement. Use this to make the movement more or less smooth.
        /// Call RefreshInverseCurve() after changing this at runtime.
        /// </summary>
        public AnimationCurve movementProfile = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        /// <summary>
        /// The maximum time it takes the platform to come to a stop when smoothStop is used with StopPlatform() 
        /// </summary>
        [Range(0f, 10f)] public float smoothStopTime = 2f;

        /// <summary>
        /// The maximum time it takes the platform to come to resume normal speed when smoothStart is used with StartPlatform() 
        /// </summary>
        [Range(0f, 10f)] public float smoothStartTime = 2f;

        /// <summary>
        /// Whether the platform rotates.
        /// </summary>
        public bool rotate = false;

        /// <summary>
        /// The starting rotation of the platform.
        /// </summary>
        public Vector3 startingRotation = Vector3.zero;

        /// <summary>
        /// The axis of rotation of the platform.
        /// </summary>
        public Vector3 rotationAxis = Vector3.up;

        /// <summary>
        /// The rotational speed of the platform in degrees per second.
        /// </summary>
        public float rotationSpeed = 180f;

        /// <summary>
        /// The audio source containing the clip to play for the platform.
        /// Must be a child of the platform gameobject.
        /// Call ResetAudioSettings() if changed at runtime.
        /// </summary>
        public AudioSource platformAudio = null;

        /// <summary>
        /// Overall volume of sound for the platform
        /// </summary>
        [Range(0f, 1f)] public float overallAudioVolume = 0.5f;

        /// <summary>
        /// The sound that is played while the platform is moving
        /// </summary>
        public AudioClip inTransitAudioClip = null;

        /// <summary>
        /// The sound that is played when the platform arrives at the first position.
        /// This does not play when it is first initialised.
        /// </summary>
        public AudioClip audioArrivedStartClip = null;

        /// <summary>
        /// The relative volume the arrived start audio clip is played
        /// </summary>
        [Range(0f, 1f)] public float audioArrivedStartVolume = 1f;

        /// <summary>
        /// The sound that is played when the platform arrives at the last position
        /// </summary>
        public AudioClip audioArrivedEndClip = null;

        /// <summary>
        /// The relative volume the arrived end audio clip is played
        /// </summary>
        [Range(0f, 1f)] public float audioArrivedEndVolume = 1f;

        /// <summary>
        /// These are triggered by a moving platform when it arrives at the start position
        /// </summary>
        public SSCMovingPlatformEvt1 onArriveStart = null;

        /// <summary>
        /// These are triggered by a moving platform when it arrives at the end position
        /// </summary>
        public SSCMovingPlatformEvt1 onArriveEnd = null;

        /// <summary>
        /// These are triggered by a moving platform when it departs from the start position
        /// </summary>
        public SSCMovingPlatformEvt1 onDepartStart = null;

        /// <summary>
        /// These are triggered by a moving platform when it departs from the end position
        /// </summary>
        public SSCMovingPlatformEvt1 onDepartEnd = null;

        #endregion Public Variables

        #region Public Properties

        /// <summary>
        /// The unique identifier of this platform during this session
        /// </summary>
        public int PlatformId { get { return platformId; } }

        /// <summary>
        /// If Move is enabled, is the platform waiting at the starting position? 
        /// </summary>
        public bool IsWaitingAtStartPosition { get { return isInitialised && move && isWaitingAtStartPosition; } }

        /// <summary>
        /// If Move is enabled, is the platform waiting at the end or last position? 
        /// </summary>
        public bool IsWaitingAtEndPosition { get { return isInitialised && move && isWaitingAtEndPosition; } }

        #endregion

        #region Private Variables

        /// <summary>
        /// Is the platform ready for use?
        /// </summary>
        private bool isInitialised = false;

        private int platformId = -1;

        private bool isMoveFixedUpdate = false;

        /// <summary>
        /// World space origin for relative positions.
        /// </summary>
        private Vector3 originPosition = Vector3.zero;

        /// <summary>
        /// The rotation of the platform at the origin, used to determine
        /// relative offset positions when useRelativePositions is true.
        /// </summary>
        private Quaternion originRotation = Quaternion.identity;

        /// <summary>
        /// The number of positions in the array.
        /// </summary>
        private int numPositions = 0;

        /// <summary>
        /// The index of the position the platform was last at
        /// </summary>
        private int lastPositionIndex = 0;

        /// <summary>
        /// The index of the position the platform should move towards.
        /// </summary>
        private int nextPositionIndex = 0;

        /// <summary>
        /// The last worldspace position the platform visited.
        /// </summary>
        private Vector3 lastPosition = Vector3.zero;

        /// <summary>
        /// The worldspace position the platform is now moving towards.
        /// </summary>
        private Vector3 nextPosition = Vector3.zero;

        /// <summary>
        /// The total time it will take to travel from the last position to the next position.
        /// </summary>
        private float travelTimeToNextPosition = 0f;

        /// <summary>
        /// The total distance from the last position to the next position.
        /// </summary>
        private float travelDistToNextPosition = 0f;

        /// <summary>
        /// The time elapsed since the platform left the last position.
        /// </summary>
        private float travelTimer = 0f;

        /// <summary>
        /// The time elaspsed since the platform reached the last position.
        /// </summary>
        private float waitTimer = 0f;

        /// <summary>
        /// Used with smooth start/stop
        /// </summary>
        private float smoothTimer = 0f;

        /// <summary>
        /// Is the platform stopped between start and end positions?
        /// </summary>
        private bool isStoppedInTransit = false;

        /// <summary>
        /// Only applies if manually stopped with StopPlatform(..)
        /// </summary>
        private bool isSmoothStopEnabled = false;

        /// <summary>
        /// Only applies if manually started with StartPlatform(..).
        /// </summary>
        private bool isSmoothStartEnabled = false;

        /// <summary>
        /// The current rate or acceleration (or decceleration)
        /// when smoothly stopping or starting
        /// </summary>
        private float smoothAcceleration = 0f;

        private float smoothInitialSpeed = 0f;

        private Vector3 smoothInitialPosition = Vector3.zero;
        private float smoothDistanceToTravel = 0f;

        /// <summary>
        /// Whether we are currently waiting at the first position.
        /// </summary>
        private bool isWaitingAtStartPosition = false;

        /// <summary>
        /// Whether we are currently waiting at the last position.
        /// </summary>
        private bool isWaitingAtEndPosition = false;

        /// <summary>
        /// Can audio clips be played?
        /// </summary>
        private bool isAudioAvailable = false;

        /// <summary>
        /// The optional rigidbody attached to the platform
        /// </summary>
        private Rigidbody rBody = null;

        /// <summary>
        /// Is there a rigidbody attached to the platform?
        /// </summary>
        private bool isRigidBody = false;

        /// <summary>
        /// Is the plaftorm currently attempting to reverse its
        /// direction of travel?
        /// </summary>
        private bool isReversingDirection = false;

        /// <summary>
        /// The current world space position of the platform
        /// </summary>
        private Vector3 currentWorldPosition = Vector3.zero;

        /// <summary>
        /// The world space position of the platform in the last frame
        /// </summary>
        private Vector3 previousWorldPosition = Vector3.zero;

        /// <summary>
        /// The current world space rotation of the platform
        /// </summary>
        private Quaternion currentWorldRotation = Quaternion.identity;

        /// <summary>
        /// Inverts time and distance so we can find the current time
        /// from a distance between points.
        /// </summary>
        private AnimationCurve invMovementProfile = null;

        #endregion Private Variables

        #region Awake and Update Methods

        // Awake is called before the first frame update
        void Awake()
        {
            // Initialise the platform
            if (initialiseOnAwake) { Initialise(); }
        }

        private void Update()
        {
            if (!isMoveFixedUpdate && isInitialised && !isStoppedInTransit)
            {
                UpdatePlatform();
            }
        }

        private void FixedUpdate()
        {
            if (isMoveFixedUpdate && isInitialised && !isStoppedInTransit)
            {
                UpdatePlatform();
            }
        }

        #endregion Awake and Update Methods

        #region Private Member Methods

        private void UpdatePlatform()
        {
            // Movement
            if (move && numPositions > 1 && Time.deltaTime > 0f)
            {
                previousWorldPosition = currentWorldPosition;

                if (isSmoothStopEnabled || isSmoothStartEnabled)
                {
                    // Increment the smooth movement timer
                    smoothTimer += Time.deltaTime;

                    // Calculate the distance we need to be displaced from the initial position
                    float dist = smoothInitialSpeed * smoothTimer + (0.5f * smoothAcceleration * (smoothTimer * smoothTimer));

                    // Move towards the next position
                    Vector3 direction = (nextPosition - lastPosition).normalized;
                    currentWorldPosition = smoothInitialPosition + direction * dist;

                    // Smoothly move the platform
                    MovePlatform();

                    if (isSmoothStartEnabled)
                    {
                        // Smooth start

                        // Calculate the current curve time 
                        float currentCurveTime = CalculateCurrentCurveTime();
                        // Calculate the current curve speed
                        float currentCurveSpeed = CalculateCurveSpeed(currentCurveTime);
                        // Calculate our current speed
                        float currentPlatformSpeed = smoothAcceleration * smoothTimer;
                        //Debug.Log("Platform: " + currentPlatformSpeed.ToString("0.000") + " m/s, curve: " + currentCurveSpeed.ToString("0.000") + " m/s");
                        // If our current speed is greater than or equal to the current curve speed, switch to normal movement
                        if (currentPlatformSpeed >= currentCurveSpeed)
                        {
                            isSmoothStartEnabled = false;
                            travelTimer = currentCurveTime;
                        }
                    }
                    else
                    {
                        // Smooth stop

                        if (smoothTimer > smoothStopTime || dist >= smoothDistanceToTravel)
                        {
                            move = false;
                            isSmoothStopEnabled = false;

                            // Switch the direction platform is moving if required
                            if (isReversingDirection)
                            {
                                ChangeDirection();

                                isReversingDirection = false;

                                // If smooth stop, assume a smooth start.
                                StartPlatform(true);
                            }
                        }
                    }
                }
                else if (isWaitingAtStartPosition)
                {
                    // Increment the waiting timer
                    waitTimer += Time.deltaTime;

                    // Check if the timer has exceeded the waiting time
                    if (waitTimer > startWaitTime)
                    {
                        // Stop waiting
                        isWaitingAtStartPosition = false;

                        PlayInTransitAudio();

                        if (onDepartStart != null) { onDepartStart.Invoke(true, false, platformId, Vector3.zero); }
                    }
                }
                else if (isWaitingAtEndPosition)
                {
                    // Increment the waiting timer
                    waitTimer += Time.deltaTime;

                    // Check if the timer has exceeded the waiting time
                    if (waitTimer > endWaitTime)
                    {
                        // Stop waiting
                        isWaitingAtEndPosition = false;

                        PlayInTransitAudio();

                        if (onDepartEnd != null) { onDepartEnd.Invoke(false, true, platformId, Vector3.zero); }
                    }
                }
                else
                {
                    // Increment the movement timer
                    travelTimer += Time.deltaTime;

                    // Check if the timer has exceeded the travel time
                    if (travelTimer > travelTimeToNextPosition)
                    {
                        // Start moving towards the next position
                        GoToNextPosition();
                    }
                    else
                    {
                        // Move the platform to the correct position using the movement profile
                        float movementProfileValue = movementProfile.Evaluate(travelTimer / travelTimeToNextPosition);

                        currentWorldPosition = lastPosition + (nextPosition - lastPosition) * movementProfileValue;

                        MovePlatform();

                        if (isAudioAvailable && platformAudio.isPlaying && platformAudio.isActiveAndEnabled)
                        {
                            platformAudio.volume = overallAudioVolume * movementProfileValue;
                        }
                    }
                }
            }

            // Rotation
            if (rotate)
            {
                // If we have a rigidbody and this is being called from FixedUpdate(), rotate the rigidbody rather than the transform.
                if (isRigidBody && isMoveFixedUpdate)
                {
                    currentWorldRotation = Quaternion.Euler(rotationAxis.normalized * rotationSpeed * Time.deltaTime) * rBody.rotation;
                    rBody.MoveRotation(currentWorldRotation);
                }
                else
                {
                    currentWorldRotation = Quaternion.Euler(rotationAxis.normalized * rotationSpeed * Time.deltaTime) * transform.rotation;
                    transform.rotation = currentWorldRotation;
                }
            }
        }

        /// <summary>
        /// Change the last/next positions around (and do nothing else)
        /// </summary>
        private void ChangeDirection()
        {
            int oldNextPosIndex = nextPositionIndex;
            int oldLastPosIndex = lastPositionIndex;

            Vector3 oldNextPos = nextPosition;
            Vector3 oldLastPos = lastPosition;

            nextPositionIndex = oldLastPosIndex;
            lastPositionIndex = oldNextPosIndex;
            nextPosition = oldLastPos;
            lastPosition = oldNextPos;
        }

        /// <summary>
        /// Does all the necessary calculations to set up the platform to go to the next position in the array.
        /// </summary>
        private void GoToNextPosition()
        {
            // Increment the next position index, and calculate the last position index
            nextPositionIndex++;
            lastPositionIndex = nextPositionIndex - 1;
            // Loop round to the beginning of the list if necessary
            nextPositionIndex %= numPositions;

            // Retrieve the positions from the array
            if (useRelativePositions)
            {
                // Adjust for relative positions
                lastPosition = originPosition + (originRotation * positions[lastPositionIndex]);
                nextPosition = originPosition + (originRotation * positions[nextPositionIndex]);
            }
            else
            {
                lastPosition = positions[lastPositionIndex];
                nextPosition = positions[nextPositionIndex];
            }

            // Calculate the travel distance between the two positions
            travelDistToNextPosition = (nextPosition - lastPosition).magnitude;
            // Calculate the travel time between the two positions
            travelTimeToNextPosition = averageMoveSpeed == 0f ? float.MaxValue : travelDistToNextPosition / averageMoveSpeed;
            
            // Reset the timers
            waitTimer = 0f;
            travelTimer = 0f;
            
            if (lastPositionIndex == 0)
            {
                // Start waiting if we have a wait timer for the first position
                if (startWaitTime > 0.001f)
                {
                    isWaitingAtStartPosition = true;
                    StopInTransitAudio();
                    PlayStartAudio();

                    if (onArriveStart != null) { onArriveStart.Invoke(true, false, platformId, Vector3.zero); }
                }
                else
                {
                    // Arrive at the first position, then immediately depart
                    if (onArriveStart != null) { onArriveStart.Invoke(true, false, platformId, Vector3.zero); }
                    if (onDepartStart != null) { onDepartStart.Invoke(true, false, platformId, Vector3.zero); }
                }
            }
            else if (lastPositionIndex == numPositions - 1)
            {
                // Start waiting if we have a wait timer for the last position
                if (endWaitTime > 0.001f)
                {
                    isWaitingAtEndPosition = true;
                    StopInTransitAudio();
                    PlayEndAudio();

                    if (onArriveEnd != null) { onArriveEnd.Invoke(false, true, platformId, Vector3.zero); }
                }
                else
                {
                    // Arrive at the last position, then immediately depart
                    if (onArriveEnd != null) { onArriveEnd.Invoke(false, true, platformId, Vector3.zero); }
                    if (onDepartEnd != null) { onDepartEnd.Invoke(false, true, platformId, Vector3.zero); }
                }
            }
        }

        private void MovePlatform()
        {
            // If we have a rigidbody and this is being called from FixedUpdate(), move the rigidbody rather than the transform.
            if (isRigidBody && isMoveFixedUpdate)
            {
                rBody.MovePosition(currentWorldPosition);
            }
            else
            {
                transform.position = currentWorldPosition;
            }
        }

        /// <summary>
        /// Attempt to play the InTransist audio clip
        /// </summary>
        private void PlayInTransitAudio()
        {
            if (isAudioAvailable && inTransitAudioClip != null)
            {
                platformAudio.clip = inTransitAudioClip;

                if (!platformAudio.isPlaying && platformAudio.isActiveAndEnabled) { platformAudio.Play(); }
            }
        }

        private void StopInTransitAudio()
        {
            if (isAudioAvailable && inTransitAudioClip != null)
            {
                platformAudio.volume = 0f;
                if (platformAudio.isActiveAndEnabled && platformAudio.isPlaying) { platformAudio.Stop(); }
            }
        }

        private void PlayStartAudio()
        {
            if (isAudioAvailable && audioArrivedStartClip != null)
            {
                platformAudio.volume = overallAudioVolume; 
                platformAudio.PlayOneShot(audioArrivedStartClip, audioArrivedStartVolume);
            }
        }

        private void PlayEndAudio()
        {
            if (isAudioAvailable && audioArrivedEndClip != null)
            {
                platformAudio.volume = overallAudioVolume;
                platformAudio.PlayOneShot(audioArrivedEndClip, audioArrivedEndVolume);
            }
        }

        /// <summary>
        /// Calculates the time value for the current position in the movement profile curve.
        /// </summary>
        /// <returns></returns>
        private float CalculateCurrentCurveTime ()
        {
            // Position variables are in world space
            float distBetweenPositions = (nextPosition - lastPosition).magnitude;
            float distFromLast = (currentWorldPosition - lastPosition).magnitude;

            // Calculate normalised value how far between last and next position 
            float distanceTravelledN = distBetweenPositions != 0f ? distFromLast / distBetweenPositions : 0f;

            // Conduct binary search to find at what time this normalised value occurs at
            float minTime = 0f;
            float maxTime = 1f;
            float timeEstimate = distanceTravelledN;
            float errorMargin = 0.01f;
            // Iterate a maximum of 10 times (for reasonable accuracy but not too high a cost)
            int maxSearchIterations = 10;
            for (int i = 0; i < maxSearchIterations; i++)
            {
                // Evaluate the curve at our current estimate
                float estimateValue = movementProfile.Evaluate(timeEstimate);
                // Check if our estimate is within the specified error margin
                float currentError = estimateValue - distanceTravelledN;
                if ((currentError > 0f && currentError < errorMargin) || (currentError < 0f && currentError > -errorMargin))
                {
                    // If our estimate is within the specified error margin, stop the search
                    i = maxSearchIterations;
                }
                else
                {
                    // If the evaluated value is too high, update our max time value
                    if (estimateValue > distanceTravelledN) { maxTime = timeEstimate; }
                    // If the evaluated value is too low, update our min time value
                    else { minTime = timeEstimate; }
                    // Calculate a new time estimate halfway between the min and max time values
                    timeEstimate = (minTime + maxTime) * 0.5f;
                }
            }

            // Return the best estimate for the time scaled by travel time to next position
            return timeEstimate * travelTimeToNextPosition;
        }

        /// <summary>
        /// Calculates the approximate "speed" (the time derivative) of the movement profile curve at the specified time value.
        /// </summary>
        private float CalculateCurveSpeed (float currentCurveTime)
        {
            // Half of distance in t-direction to measure derivative over
            float tDist = 0.005f;
            // Calculate times to evaluate curve at
            float t1 = 0f;
            float t2 = 1f;
            if (currentCurveTime < tDist) { t1 = 0f; t2 = 2f * tDist; }
            else if (currentCurveTime > 1 - tDist) { t1 = 1 - (2f * tDist); t2 = 1f;  }
            else { t1 = currentCurveTime - tDist; t2 = currentCurveTime + tDist; }

            // Evaluate curve and measure derivative
            float curveDerivative = ((movementProfile.Evaluate(t2) - movementProfile.Evaluate(t1)) * travelDistToNextPosition) /
                (2f * tDist * travelTimeToNextPosition);

            // Return derivative scaled by travel time to next position
            return curveDerivative / travelTimeToNextPosition;
        }

        #endregion Private Member Methods

        #region Public Member API Methods

        /// <summary>
        /// If the platform is travelling away from the start, stop it (using
        /// smooth stop if requested), then start it (using smooth start if
        /// requested), and move to the first or starting position.
        /// </summary>
        /// <param name="useSmoothStartStop"></param>
        public void CallToStartPosition (bool useSmoothStartStop)
        {
            if (IsDestinationEnd())
            {
                // Check if already at starting position
                if (isWaitingAtStartPosition)
                {
                    // waiting at start so reset waiting timer to give more time to get onto platform
                    waitTimer = 0f;
                }
                else
                {
                    //Debug.Log("[DEBUG] SSCMovingPlatform.CallToStartPosition T:" + Time.time);
                    isReversingDirection = true;
                    StopPlatform(useSmoothStartStop);
                }
            }
            else if (isWaitingAtEndPosition)
            {
                // Get the lift moving if still waiting at the other end.
                // Too bad if someone or something is trying to get onto the platform...
                // Could have a mininum wait time if need be.
                waitTimer = endWaitTime;
            }
        }

        /// <summary>
        /// Initialises and resets the platform. Call this if you modify the positions list or if you want to reset the platform
        /// back to its original position and rotation.
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            rBody = GetComponent<Rigidbody>();
            isRigidBody = rBody != null;

            platformId = this.GetInstanceID();

            if (isRigidBody)
            {
                // Kinematic does not support ContinuousDynamic.
                if (rBody.collisionDetectionMode == CollisionDetectionMode.ContinuousDynamic || rBody.collisionDetectionMode == CollisionDetectionMode.Continuous)
                {
                    //rBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    rBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                }

                // If user hasn't selected an interpolation mode, select Interpolate.
                // NOTE: Other rigidbodies around the character will also need to use the same to avoid jitter.
                if (rBody.interpolation == RigidbodyInterpolation.None)
                {
                    rBody.interpolation = RigidbodyInterpolation.Interpolate;
                }

                // Set up the rigidbody
                rBody.isKinematic = true;
                rBody.detectCollisions = true;
            }

            SetMoveUpdateType(moveUpdateType);

            currentWorldPosition = transform.position;
            currentWorldRotation = transform.rotation;

            previousWorldPosition = currentWorldPosition;

            SetOrigin(currentWorldPosition);

            // Used with relative positions
            originRotation = currentWorldRotation;

            // Check how many positions there are in the list
            numPositions = positions.Count;
            if (numPositions > 1)
            {
                // Set the position of the platform to the first position in the list.
                nextPositionIndex = 0;

                // Audio won't be played as ResetAudioSettings() is called later in Initialise().
                // Typically you don't want a first posiiton audio clip to play when we first start.
                GoToNextPosition();
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SSCMovingPlatform.Initialise - at least two positions are required for a moving platform.");
                #endif
            }

            // Set the position of the platform back to its starting rotation
            currentWorldRotation = Quaternion.Euler(startingRotation);
            transform.rotation = currentWorldRotation;

            ResetAudioSettings();

            RefreshInverseCurve();

            isInitialised = true;
        }

        /// <summary>
        /// If the platform is moving, is it travelling toward the end position?
        /// If the destination is the starting point, then this will return false.
        /// </summary>
        /// <returns></returns>
        public bool IsDestinationEnd()
        {
            return lastPositionIndex < nextPositionIndex;
        }

        /// <summary>
        /// Set the world space origin of the platform. When useRelativePositions is true, all positions are relative to the origin.
        /// </summary>
        public void SetOrigin(Vector3 worldSpaceOrigin)
        {
            originPosition = worldSpaceOrigin;
        }

        /// <summary>
        /// Call this after changing the MoveProfile at runtime.
        /// </summary>
        public void RefreshInverseCurve()
        {
            invMovementProfile = SSCUtils.InverseAnimCurve(movementProfile);
        }

        /// <summary>
        /// Call this when you wish to remove any custom event listeners, like
        /// after creating them in code and then destroying the object.
        /// You could add this to your game play OnDestroy code.
        /// </summary>
        public void RemoveListeners()
        {
            if (isInitialised)
            {
                if (onArriveStart != null) { onArriveStart.RemoveAllListeners(); }
                if (onArriveEnd != null) { onArriveEnd.RemoveAllListeners(); }
                if (onDepartStart != null) { onDepartStart.RemoveAllListeners(); }
                if (onDepartEnd != null) { onDepartEnd.RemoveAllListeners(); }
            }
        }

        /// <summary>
        /// Call after changing audio source
        /// </summary>
        public void ResetAudioSettings()
        {
            isAudioAvailable = false;

            if (platformAudio != null)
            {
                platformAudio.volume = overallAudioVolume;
                isAudioAvailable = true;
            }
        }

        /// <summary>
        /// Set the update loop used to move and/or rotate the platform
        /// </summary>
        /// <param name="newMoveUpdateType"></param>
        public void SetMoveUpdateType(MoveUpdateType newMoveUpdateType)
        {
            moveUpdateType = newMoveUpdateType;

            isMoveFixedUpdate = moveUpdateType == MoveUpdateType.FixedUpdate;
        }

        /// <summary>
        /// Start the platform either instantly or smoothly.
        /// It will be ignored if the platform is already starting.
        /// </summary>
        /// <param name="isSmoothStart"></param>
        public void StartPlatform (bool isSmoothStart)
        {
            // Ensure platform is not in the process of starting, and that it is in fact not moving.
            if (!isSmoothStartEnabled && !move)
            {
                isSmoothStopEnabled = false;
                isSmoothStartEnabled = isSmoothStart;

                // Calculate the travel distance between the two positions
                travelDistToNextPosition = (nextPosition - lastPosition).magnitude;
                // Calculate the travel time between the two positions
                travelTimeToNextPosition = averageMoveSpeed == 0f ? float.MaxValue : travelDistToNextPosition / averageMoveSpeed;

                if (isSmoothStart && smoothStartTime > 0f)
                {
                    // Smooth Start

                    // Recalculate travel timer
                    travelTimer = CalculateCurrentCurveTime();
                    // Set initial values for smooth movement
                    smoothTimer = 0f;
                    smoothAcceleration = averageMoveSpeed / smoothStartTime;
                    smoothInitialPosition = currentWorldPosition;
                    smoothInitialSpeed = 0f;

                    move = true;
                }
                else
                {
                    // Instant start (works for linear curves only)

                    // Are we re-starting part way between locations?
                    if (travelTimer != 0f || (!isWaitingAtStartPosition && !isWaitingAtEndPosition))
                    {
                        // Recalculate travel timer
                        travelTimer = CalculateCurrentCurveTime();
                    }

                    move = true;
                }

                // No longer waiting at start or end
                waitTimer = 0f;
                isWaitingAtStartPosition = false;
                isWaitingAtEndPosition = false;
            }
        }

        /// <summary>
        /// Stop the platform either instantly or smoothly before the next position.
        /// Will be ignored if already stopping.
        /// </summary>
        /// <param name="isSmoothStop"></param>
        public void StopPlatform (bool isSmoothStop)
        {
            // Ensure platform is not in the process of stopping, and that it is in fact moving.
            if (!isSmoothStopEnabled && move)
            {
                isSmoothStartEnabled = false;
                isSmoothStopEnabled = isSmoothStop;

                if (isSmoothStop && !isWaitingAtStartPosition && !isWaitingAtEndPosition && smoothStopTime > 0f)
                {
                    smoothTimer = 0f;
                    // deceleration (d) = (finalSpeed - initialSpeed) / time

                    // Get current speed
                    smoothInitialSpeed = (currentWorldPosition - previousWorldPosition).magnitude / Time.deltaTime;

                    // Consider the distance to the next position and make sure we don't overshoot it.
                    float stoppingTime = travelTimeToNextPosition - travelTimer < smoothStopTime ? travelTimeToNextPosition - travelTimer : smoothStopTime;

                    // If we're already at or past the next position, stop now
                    if (stoppingTime <= 0f)
                    {
                        move = false;
                    }
                    else
                    {
                        // Assume constant deceleration, so average speed is mean of initial and final speed

                        // distance to stop = t * ((init speed + final speed) / 2)
                        smoothDistanceToTravel = stoppingTime * (smoothInitialSpeed / 2f);

                        //Debug.Log("[DEBUG] Distance To Stop: " + smoothDistanceToTravel);

                        // Acceleration = - initSpeed / time (assuming constant deceleration and final speed is 0)
                        smoothAcceleration = -smoothInitialSpeed / stoppingTime;

                        smoothInitialPosition = currentWorldPosition;
                    }
                }
                // Instant stop
                else { move = false; }

                isSmoothStopEnabled = move;

                // Switch the direction platform is moving if required
                // If smooth
                if (isReversingDirection && !isSmoothStopEnabled)
                {
                    ChangeDirection();

                    isReversingDirection = false;

                    //Debug.Log("[DEBUG] Stopped platform instantly - now should reverse the direction and start (smoothstart: " + isSmoothStartEnabled + ")");

                    // If smooth stop, assume a smooth start.
                    StartPlatform(false);
                }
            }
        }

        /// <summary>
        /// Teleport the platform to new location by moving by an amount in the x, y and z directions.
        /// This could be useful if changing the origin or centre of your world to compensate for float-point error. 
        /// </summary>
        /// <param name="delta"></param>
        public void TelePort(Vector3 delta)
        {
            transform.position += delta;

            currentWorldPosition = transform.position;

            SetOrigin(originPosition += delta);

            // For non-relative positions, move them all
            if (!useRelativePositions)
            {
                lastPosition += delta;
                nextPosition += delta;

                numPositions = positions.Count;
                for (int pIdx = 0; pIdx < numPositions; pIdx++)
                {
                    positions[pIdx] += delta;
                }
            }
        }

        /// <summary>
        /// Update the current average move speed. Useful when the platform is already moving
        /// and you want to adjust the speed.
        /// </summary>
        /// <param name="newMoveSpeed"></param>
        public void UpdateAverageMoveSpeed(float newMoveSpeed)
        {
            // Calc normalised value how far between last and next position 
            float distanceTravelledN = travelTimeToNextPosition == 0f ? 0f : travelTimer / travelTimeToNextPosition;

            averageMoveSpeed = newMoveSpeed;

            // Calculate the travel distance between the two positions
            travelDistToNextPosition = (nextPosition - lastPosition).magnitude;
            // Calculate the travel time between the two positions
            travelTimeToNextPosition = averageMoveSpeed == 0f ? float.MaxValue : travelDistToNextPosition / averageMoveSpeed;

            // Adjust travel timer
            travelTimer = travelTimeToNextPosition * distanceTravelledN;
        }

        #endregion Public Member Methods
    }
}
