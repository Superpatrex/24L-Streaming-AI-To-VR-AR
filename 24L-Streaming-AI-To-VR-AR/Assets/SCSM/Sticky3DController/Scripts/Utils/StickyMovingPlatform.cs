using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class for a moving platform.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Utilities/Sticky Moving Platform")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyMovingPlatform : MonoBehaviour
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
        /// when the Sticky Moving Platform is enabled through code.
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
        /// The time the platform waits at each position.
        /// </summary>
        public float waitTime = 0f;

        /// <summary>
        /// The "profile" of the platform's movement. Use this to make the movement more or less smooth.
        /// </summary>
        public AnimationCurve movementProfile = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

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

        #endregion Public Variables

        #region Public Properties

        /// <summary>
        /// If Move is enabled, is the platform waiting at one of the end points? 
        /// </summary>
        public bool IsWaitingAtPosition { get { return isInitialised && move && isWaitingAtPosition; } }

        #endregion

        #region Private Variables

        /// <summary>
        /// Is the platform ready for use?
        /// </summary>
        private bool isInitialised = false;

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
        /// The index of the position the platform should move towards.
        /// </summary>
        private int nextPositionIndex = 0;

        /// <summary>
        /// The last position the platform visited.
        /// </summary>
        private Vector3 lastPosition = Vector3.zero;

        /// <summary>
        /// The position the platform is now moving towards.
        /// </summary>
        private Vector3 nextPosition = Vector3.zero;

        /// <summary>
        /// The total time it will take to travel from the last position to the next position.
        /// </summary>
        private float travelTimeToNextPosition = 0f;

        /// <summary>
        /// The time elapsed since the platform left the last position.
        /// </summary>
        private float travelTimer = 0f;

        /// <summary>
        /// The time elaspsed since the platform reached the last position.
        /// </summary>
        private float waitTimer = 0f;

        /// <summary>
        /// Whether we are currently waiting at the last position.
        /// </summary>
        private bool isWaitingAtPosition = false;

        /// <summary>
        /// The optional rigidbody attached to the platform
        /// </summary>
        private Rigidbody rBody = null;

        /// <summary>
        /// Is there a rigidbody attached to the platform?
        /// </summary>
        private bool isRigidBody = false;

        /// <summary>
        /// The current world space position of the platform
        /// </summary>
        private Vector3 currentWorldPosition = Vector3.zero;

        /// <summary>
        /// The current world space rotation of the platform
        /// </summary>
        private Quaternion currentWorldRotation = Quaternion.identity;

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
            if (!isMoveFixedUpdate && isInitialised)
            {
                UpdatePlatform();
            }
        }

        private void FixedUpdate()
        {
            if (isMoveFixedUpdate && isInitialised)
            {
                UpdatePlatform();
            }
        }

        #endregion Awake and Update Methods

        #region Private Member Methods

        /// <summary>
        /// Move or rotate the platform
        /// </summary>
        private void UpdatePlatform()
        {
            // Movement
            if (move && numPositions > 1)
            {
                if (isWaitingAtPosition)
                {
                    // Increment the waiting timer
                    waitTimer += Time.deltaTime;

                    // Check if the timer has exceeded the waiting time
                    if (waitTimer > waitTime)
                    {
                        // Stop waiting
                        isWaitingAtPosition = false;
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
                        currentWorldPosition = lastPosition + (nextPosition - lastPosition) * movementProfile.Evaluate(travelTimer / travelTimeToNextPosition);

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
        /// Does all the necessary calculations to set up the platform to go to the next position in the array.
        /// </summary>
        private void GoToNextPosition()
        {
            // Increment the next position index, and calculate the last position index
            nextPositionIndex++;
            int lastPositionIndex = nextPositionIndex - 1;
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

            // Calculate the travel time between the two positions
            travelTimeToNextPosition = averageMoveSpeed == 0f ? float.MaxValue : (nextPosition - lastPosition).magnitude / averageMoveSpeed;
            // Reset the timers
            waitTimer = 0f;
            travelTimer = 0f;
            // Start waiting if we have a wait timer
            if (waitTime > 0.001f) { isWaitingAtPosition = true; }
        }

        #endregion Private Member Methods

        #region Public Member API Methods

        /// <summary>
        /// Initialises and resets the platform. Call this if you modify the positions list or if you want to reset the platform
        /// back to its original position and rotation.
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            rBody = GetComponent<Rigidbody>();
            isRigidBody = rBody != null;

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

            SetOrigin(currentWorldPosition);

            // Used with relative positions
            originRotation = currentWorldRotation;

            // Check how many positions there are in the list
            numPositions = positions.Count;
            if (numPositions > 1)
            {
                // Set the position of the platform to the first position in the list.
                nextPositionIndex = 0;
                GoToNextPosition();
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleMovingPlatform.Initialise - at least two positions are required for a moving platform.");
                #endif
            }

            // Set the position of the platform back to its starting rotation
            currentWorldRotation = Quaternion.Euler(startingRotation);
            transform.rotation = currentWorldRotation;

            isInitialised = true;
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
        /// Set the world space origin of the platform. When useRelativePositions is true, all positions are relative to the origin.
        /// </summary>
        public void SetOrigin(Vector3 worldSpaceOrigin)
        {
            originPosition = worldSpaceOrigin;
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

            // Calculate new time to travel to next position from last.
            travelTimeToNextPosition = averageMoveSpeed == 0f ? float.MaxValue : (nextPosition - lastPosition).magnitude / averageMoveSpeed;

            // Adjust travel timer
            travelTimer = travelTimeToNextPosition * distanceTravelledN;
        }

        #endregion Public Member Methods
    }
}
