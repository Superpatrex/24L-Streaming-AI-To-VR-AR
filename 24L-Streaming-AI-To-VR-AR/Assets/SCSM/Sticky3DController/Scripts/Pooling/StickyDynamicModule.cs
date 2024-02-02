using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Dynamic objects are poolable physical objects in your scene that can react
    /// to changing gravity situations. They have configurable despawn options
    /// and can respond differently when at rest.
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyDynamicModule : StickyGenericModule, IStickyGravity
    {
        #region Enumerations

        #endregion

        #region Public Variables

        /// <summary>
        /// The ID number for this dynamic prefab (as assigned by the Sticky Manager in the scene).
        /// This is the index in the StickyManager dynamicTemplatesList.
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] public int dynamicObjectPrefabID = -1;

        #endregion

        #region Public Properties

        /// <summary>
        /// [READONLY] Get the current reference frame transform
        /// </summary>
        public Transform CurrentReferenceFrame { get { return currentReferenceFrame; } }

        /// <summary>
        /// Get or set the conditions under which this object despawns
        /// </summary>
        public StickyManager.DespawnCondition DespawnCondition { get { return despawnCondition; } set { SetDespawnCondition(value); } }

        /// <summary>
        /// Get or set how the rigidbodies are disabled when the object is considered to be Static.
        /// </summary>
        public StickyManager.DisableRBodyMode DisableRigidBodyMode { get { return disableRBodyMode; } set { SetDisableRigidBodyMode(value); } }

        /// <summary>
        /// [READONLY] Get the current speed, in metres per second, the Dynamic object is travelling in when in the Dynamic state.
        /// </summary>
        public float DynamicObjectSpeed { get { return isRBody ? rBody.velocity.magnitude : 0f; } }

        /// <summary>
        /// [READ ONLY] Get the estimated time, in seconds, before the dynamic object is despawned
        /// </summary>
        public float EstimatedDespawnTime { get { return IsDynamicModuleActivated && isModuleEnabled && despawnCondition ==  StickyManager.DespawnCondition.Time ? despawnTime - dynamicDespawnTimer : float.PositiveInfinity; } }

        /// <summary>
        /// Get or set the gravity in metres per second per second
        /// </summary>
        public float GravitationalAcceleration { get { return gravitationalAcceleration; } set { SetGravitationalAcceleration(value); } }

        /// <summary>
        /// Get or set the world space direction that gravity acts upon the dynamic object when GravityMode is Direction.
        /// </summary>
        public Vector3 GravityDirection { get { return gravityDirection; } set { SetGravityDirection(value); } }

        /// <summary>
        /// Get or set the method used to determine in which direction gravity is acting.
        /// </summary>
        public StickyManager.GravityMode GravityMode { get { return gravityMode; } set { SetGravityMode(value); } }

        /// <summary>
        /// [READONLY]
        /// Is there a rigidbody currently attached to the object?
        /// </summary>
        public bool HasRigidbody { get { return isRBody; } }

        /// <summary>
        /// [READONLY] Is the Dynamic object currently in the dynamic state (or is it static)
        /// </summary>
        public bool IsDynamic { get { return isDynamic; } }

        /// <summary>
        /// Is the dynamic module currently in use?
        /// </summary>
        public bool IsDynamicModuleActivated { get; protected set; }

        /// <summary>
        /// [READONLY] Has the module been initialised?
        /// </summary>
        public bool IsDynamicModuleInitialised { get { return isDynamicModuleInitialised; } }

        /// <summary>
        /// [READONLY]
        /// Is the dynamic object visible to the main camera? (CURRENTLY ALWAYS RETURNS TRUE)
        /// </summary>
        public bool IsObjectVisibleToCamera { get { return isObjectVisibleToCamera; } }

        /// <summary>
        /// [READONLY] Get the current rigidbody (if any).
        /// WARNING: Do not use this as a reference past the end of the frame.
        /// </summary>
        public Rigidbody ObjectRigidbody { get { return isRBody ? rBody : null; } }

        #endregion

        #region Public Static Variables

        #endregion

        #region Protected Variables - Serialized General

        /// <summary>
        /// How the rigidbodies are disabled when the object is considered to be Static.
        /// "Destroy" removes the rigidbody component and adds it back in as needed - best performance for unmoving objects
        /// "Set As Kinematic" sets the rigidbody to kinematic - half/half performance
        /// "Don't Disable" uses Unity default rigidbody behaviour where rigidbodies go into sleep mode until something collides with them
        /// - best for objects being moved
        /// "Destroy" is best for objects far away from each other, and "Don't Disable" is best for objects close to each other
        /// </summary>
        [SerializeField] protected StickyManager.DisableRBodyMode disableRBodyMode = StickyManager.DisableRBodyMode.Destroy;

        /// <summary>
        /// The conditions under which this object despawns
        /// </summary>
        [SerializeField] protected StickyManager.DespawnCondition despawnCondition = StickyManager.DespawnCondition.DontDespawn;

        /// <summary>
        /// If the object has been unmoving for more than the maximum time, set the object as static
        /// and "disable" the rigidbody according to the "Disable Rigidbody Mode".
        /// </summary>
        [SerializeField, Range(0.1f, 30f)] protected float maxTimeUnmoving = 1f;

        /// <summary>
        /// When the speed (in metres per second) in any direction falls below this value, the object is considered to have stopped moving
        /// </summary>
        [SerializeField, Range(0f, 1f)] protected float unmovingVelocity = 0.1f;

        /// <summary>
        /// Start in Static mode rather than Dynamic.
        /// This is NOT compatible with disableRBodyMode == DisableRBodyMode.Destroy
        /// </summary>
        [SerializeField] protected bool isStartStatic = false;

        /// <summary>
        /// Wait until the object is not being rendered by the camera before being despawned
        /// NOTE: This has not been implemented yet
        /// </summary>
        [SerializeField] protected bool waitUntilNotRenderedToDespawn = false;

        /// <summary>
        /// Wait until the object is set to static before being despawned
        /// </summary>
        [SerializeField] protected bool waitUntilStaticToDespawn = true;

        #endregion

        #region Protected Variables - Serialized Gravity

        /// <summary>
        /// The amount of angular drag the object has
        /// </summary>
        [SerializeField] protected float angularDrag = 0.05f;

        /// <summary>
        /// The amount of drag the object has. A solid block of metal would be 0.001, while a feather would be 10.
        /// </summary>
        [SerializeField] protected float drag = 0.01f;

        /// <summary>
        /// The rigidbody collision detection mode
        /// </summary>
        [SerializeField] protected CollisionDetectionMode collisionDetection = CollisionDetectionMode.Discrete;

        /// <summary>
        /// The gravity in metres per second per second
        /// </summary>
        [SerializeField] protected float gravitationalAcceleration = 9.81f;

        /// <summary>
        /// The world space direction that gravity acts upon the dynamic object when GravityMode is Direction.
        /// </summary>
        [SerializeField] protected Vector3 gravityDirection = Vector3.down;

        /// <summary>
        /// The method used to determine in which direction gravity is acting.
        /// </summary>
        [SerializeField] protected StickyManager.GravityMode gravityMode = StickyManager.GravityMode.UnityPhysics;

        /// <summary>
        /// The rigidbody interpolation
        /// </summary>
        [SerializeField] protected RigidbodyInterpolation interpolation = RigidbodyInterpolation.None;

        /// <summary>
        /// This is the mass of the object
        /// </summary>
        [SerializeField] protected float mass = 1f;

        /// <summary>
        /// Initial or default reference frame transform the object will stick to when Use Gravity is enabled and Gravity Mode is ReferenceFrame.
        /// </summary>
        [SerializeField] protected Transform initialReferenceFrame;

        /// <summary>
        /// Object is effected by gravity
        /// </summary>
        [SerializeField] protected bool isUseGravity = true;

        #endregion

        #region Private and Protected Variables - General

        protected int disableRBodyModeInt = -1;
        protected int despawnConditionInt = -1;
        protected float dynamicDespawnTimer = 0f;
        protected bool isDespawned = true;
        protected bool isDynamic = false;
        protected bool isDynamicModuleInitialised = false;
        protected bool isObjectVisibleToCamera = true;
        protected bool isRBody = false;
        [System.NonSerialized] Rigidbody rBody = null;
        protected float sqrUnmovingVelocity;
        protected float timeUnmoving;

        private S3DInstantiateGenericObjectParameters igParms;

        #endregion

        #region Protected Variables - Gravity

        protected int gravityModeInt = StickyManager.GravityModeDirectionInt;
        protected float defaultGravitationalAcceleration = 0f;
        protected Vector3 defaultGravityDirection = Vector3.down;

        // The current world position and rotation
        protected Vector3 currentWorldPosition = Vector3.zero;
        protected Quaternion currentWorldRotation = Quaternion.identity;

        protected Transform currentReferenceFrame = null;
        protected Transform previousReferenceFrame = null;
        protected int currentReferenceFrameId = -1;

        // The current position and rotation of the reference frame in world space
        protected Vector3 currentReferenceFramePosition = Vector3.zero;
        protected Quaternion currentReferenceFrameRotation = Quaternion.identity;
        protected Vector3 currentReferenceFrameUp = Vector3.up;
        // The current position and rotation relative to the reference frame
        protected Vector3 currentRelativePosition = Vector3.zero;
        protected Quaternion currentRelativeRotation = Quaternion.identity;

        /// <summary>
        /// This enables inheritted classes to call ApplyGravity in their own
        /// FixedUpate AFTER base.FixedUpdate runs.
        /// </summary>
        protected bool isDelayApplyGravity = false;

        #endregion

        #region Public Delegates

        #endregion

        #region Private Initialise Methods

        #endregion

        #region Update Methods

        private void FixedUpdate()
        {
            if (!isModuleEnabled || !IsDynamicModuleActivated) { return; }

            #region Disabling dynamic
            // Determine what to do when the object stops moving
            if (isDynamic && !isDespawned && isRBody && disableRBodyModeInt != StickyManager.DisableRBodyModeDontDisableInt)
            {
                // Check if the object is currently "unmoving"
                if (rBody.velocity.sqrMagnitude < sqrUnmovingVelocity)
                {
                    // If it isn't moving increment the unmoving timer
                    timeUnmoving += Time.deltaTime;

                    if (timeUnmoving > maxTimeUnmoving)
                    {
                        // If it has been unmoving for more than the maximum time set the object as static
                        SetStatic();
                        // Reset the timer
                        timeUnmoving = 0f;
                    }
                }
                else
                {
                    // If it is moving reset the unmoving timer
                    timeUnmoving = 0f;
                }
            }
            #endregion

            #region Dynamic Gravity

            if (isDynamic && !isDelayApplyGravity) { ApplyGravity(); }

            #endregion

            #region Despawn based on elapsed time
            if (despawnConditionInt == StickyManager.DespawnConditionTimeInt)
            {
                // Increment the despawn timer
                dynamicDespawnTimer += Time.deltaTime;

                if (!isDespawned && (!waitUntilNotRenderedToDespawn || !isObjectVisibleToCamera))
                {
                    // Only despawn if a given time has elapsed
                    if (dynamicDespawnTimer > despawnTime)
                    {
                        // Despawn the object
                        DestroyDynamicObject();
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Apply Gravity if required.
        /// This should always be called from FixedUpdate()
        /// </summary>
        protected virtual void ApplyGravity()
        {
            if (isUseGravity && isRBody)
            {
                // No need to do anything if Unity Physics gravity is in use.
                if (gravityModeInt == StickyManager.GravityModeDirectionInt)
                {
                    // Add gravity based on world space gravity direction, mass of the object, and the fixed time step.
                    // Uses ForceMode.Force
                    rBody.AddForce(gravityDirection * gravitationalAcceleration * mass);
                }
                else if (gravityModeInt == StickyManager.GravityModeRefFrameInt && currentReferenceFrameId != 0)
                {
                    // If the reference frame has been destroyed, clear it.
                    if (currentReferenceFrame == null) { SetCurrentReferenceFrame(null); }
                    else
                    {
                        //if (isMovementDataStale) { UpdatePositionAndMovementData(); }

                        //CalculateFromPreviousFrame();

                        /// TODO - Option to Move and rotate this interactive object as the reference frame moves and rotates


                        // Apply gravity relative to the reference frame Down direction.
                        rBody.AddForce(-currentReferenceFrameUp * gravitationalAcceleration * mass);
                    }
                }
            }
        }

        /// <summary>
        /// [UNTESTED]
        /// </summary>
        /// <param name="isEnabled"></param>
        protected virtual void EnableOrDisableDynamicObject (bool isEnabled)
        {
            if (isRBody)
            {
                if (isEnabled && !isModuleEnabled && isDynamic)
                {
                    rBody.isKinematic = false;
                    rBody.collisionDetectionMode = collisionDetection;
                }
                else if (!isEnabled)
                {
                    rBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    rBody.isKinematic = true;
                }

                rBody.detectCollisions = isEnabled;
            }
        }


        protected virtual void SetDynamic()
        {
            // Disable the static flags, enable the rigidbody
            if (disableRBodyModeInt == StickyManager.DisableRBodyModeDestroyInt)
            {
                SetUpRigidbody();
            }
            else if (disableRBodyModeInt == StickyManager.DisableRBodyModeSetAsKinematicInt)
            {
                if (isRBody)
                {
                    rBody.isKinematic = false;
                    rBody.collisionDetectionMode = collisionDetection;
                }
            }
            // Set the state of isDynamic
            isDynamic = true;
        }

        /// <summary>
        /// Set up a rigidbody
        /// </summary>
        protected virtual void SetUpRigidbody()
        {
            if (rBody == null && !TryGetComponent(out rBody))
            {
                rBody = gameObject.AddComponent<Rigidbody>();
            }

            rBody.mass = mass;
            rBody.drag = drag;
            rBody.angularDrag = angularDrag;
            rBody.interpolation = interpolation;
            rBody.collisionDetectionMode = collisionDetection;
            rBody.useGravity = isUseGravity && gravityModeInt == StickyManager.GravityModeUnityInt;

            isRBody = rBody != null;
        }

        protected virtual void SetStatic()
        {
            // Enable the static flags and disable the rigidbody

            if (disableRBodyModeInt == StickyManager.DisableRBodyModeDestroyInt)
            {
                // Destroy may not happen in the current frame. If SetDynamic is called immediately after SetStatic,
                // like on activation when isStartStatic is true, SetDynamic may add the rigidbody only to have it
                // immediately destroyed.
                if (isRBody)
                {
                    Destroy(rBody);
                    rBody = null;
                    isRBody = false;
                }
            }
            else if (disableRBodyModeInt == StickyManager.DisableRBodyModeSetAsKinematicInt)
            {
                if (isRBody)
                {
                    rBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    rBody.isKinematic = true;
                }
            }
            // Set the state of isDynamic
            isDynamic = false;
        }

        #endregion

        #region Events

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Activate the instance of the StickyDynamicModule by turning it on and get it ready for use.
        /// This is automatically called from the StickyManager pooling system whenever it is spawned (instantiated) in the scene.
        /// </summary>
        /// <param name="idParms"></param>
        /// <returns></returns>
        public virtual uint ActivateDyanmicObject (ref S3DInstantiateDynamicObjectParameters idParms)
        {
            // The module should be initialised already, but just in case this is called outside StickyManager,
            // it will check first.
            if (!isDynamicModuleInitialised) { InitialiseDynamicModule(); }

            // Store the index to the DynamicObjectTemplate from the StickyManager dynamicObjectTemplatesList
            this.dynamicObjectPrefabID = idParms.dynamicObjectPrefabID;

            // We want to control despawn time ourselves rather than via the StickyGenericModule
            //despawnTime = -1;

            // Reset despawn timer
            dynamicDespawnTimer = 0f;

            despawnConditionInt = (int)despawnCondition;
            disableRBodyModeInt = (int)disableRBodyMode;
            gravityModeInt = (int)gravityMode;

            isObjectVisibleToCamera = true;

            // Initialise dynamic module
            timeUnmoving = 0f;

            if (isDespawned)
            {
                if (isRBody)
                {
                    rBody.velocity = Vector3.zero;
                    rBody.angularVelocity = Vector3.zero;
                }

                isDespawned = false;
            }

            // Initialise generic object module
            // Currently igParms are required to be set for generic object Initialise(..).
            igParms.genericObjectPoolListIndex = -1;
            // We want to control despawn time ourselves rather than via the StickyGenericModule
            igParms.overrideAutoDestroy = true;
            Initialise(igParms);

            if (isUseGravity)
            {
                SetDynamic();
            }
            else { SetStatic(); }

            IsDynamicModuleActivated = true;

            return itemSequenceNumber;
        }

        /// <summary>
        /// [INCOMPLETE]
        /// </summary>
        public virtual void DestroyDynamicObject()
        {
            IsDynamicModuleActivated = false;
            isDespawned = true;

            if (isRBody)
            {
                rBody.velocity = Vector3.zero;
                rBody.angularVelocity = Vector3.zero;
            }

            DestroyGenericObject();
        }

        /// <summary>
        /// Temporarily disable or pause the module. Typically called automatically from
        /// stickyManager.PauseDynamicObjects().
        /// </summary>
        public override void DisableModule()
        {
            EnableOrDisableDynamicObject(false);
            base.DisableModule();
        }

        /// <summary>
        /// Re-enable or unpause the module. Typically called automatically from
        /// stickyManager.ResumeDynamicObjects().
        /// </summary>
        public override void EnableModule()
        {
            EnableOrDisableDynamicObject(true);
            base.EnableModule();
        }

        /// <summary>
        /// The module must always be initialised AND activated before it can be used.
        /// It will only be initialised once, while it can be activated and deactivated
        /// multiple times.
        /// Once initialised, this will always return true. 
        /// </summary>
        /// <returns></returns>
        public virtual bool InitialiseDynamicModule()
        {
            if (isDynamicModuleInitialised) { return true; }
            else
            {
                // Calculate square of unmovingVelocity so that at runtime we can use the 
                // vector3.sqrMagnitude function instead of the slower vector3.magnitude to compare velocities
                sqrUnmovingVelocity = unmovingVelocity * unmovingVelocity;

                despawnConditionInt = (int)despawnCondition;
                disableRBodyModeInt = (int)disableRBodyMode;
                gravityModeInt = (int)gravityMode;

                // Remember initial gravity settings. See also ResetGravity().
                defaultGravitationalAcceleration = gravitationalAcceleration;
                defaultGravityDirection = gravityDirection;

                if (disableRBodyModeInt != StickyManager.DisableRBodyModeDestroyInt)
                {
                    SetUpRigidbody();
                }

                if (isStartStatic)
                {
                    // If starting static, set the object as static
                    SetStatic();
                }
                else
                {
                    // Else set the object as dynamic
                    SetDynamic();
                }

                if (gravityModeInt == StickyManager.GravityModeRefFrameInt)
                {
                    SetCurrentReferenceFrame(initialReferenceFrame);
                }

                isDynamicModuleInitialised = true;

                return isDynamicModuleInitialised;
            }
        }

        public void SetDespawnCondition (StickyManager.DespawnCondition newDespawnCondition)
        {
            despawnCondition = newDespawnCondition;
            despawnConditionInt = (int)despawnCondition;
        }

        /// <summary>
        /// Set how the rigidbodies are disabled when the object is considered to be Static.
        /// </summary>
        /// <param name="newDisableRBodyMode"></param>
        public void SetDisableRigidBodyMode (StickyManager.DisableRBodyMode newDisableRBodyMode)
        {
            disableRBodyMode = newDisableRBodyMode;
            disableRBodyModeInt = (int)disableRBodyMode;
        }


        #endregion

        #region Public API Methods - Gravity

        /// <summary>
        /// Reset gravity to default (starting) values.
        /// Typically gets called automatically.
        /// </summary>
        public void ResetGravity()
        {
            gravitationalAcceleration = defaultGravitationalAcceleration;
            gravityDirection = defaultGravityDirection;
        }

        /// <summary>
        /// Attempt to restore the current reference frame, to the initial or default setting.
        /// This is automatically called when exiting a StickyZone.
        /// </summary>
        public virtual void RestoreDefaultReferenceFrame()
        {
            SetCurrentReferenceFrame(initialReferenceFrame);
        }

        /// <summary>
        /// Attempt to restore the previous reference frame that was being used before what is
        /// currently set. NOTE: We do not support nesting.
        /// </summary>
        public virtual void RestorePreviousReferenceFrame()
        {
            SetCurrentReferenceFrame(previousReferenceFrame);
        }

        /// <summary>
        /// Sets the current reference frame.
        /// </summary>
        /// <param name="newReferenceFrame"></param>
        public virtual void SetCurrentReferenceFrame (Transform newReferenceFrame)
        {
            if (newReferenceFrame != null)
            {
                // Only update if the reference frame has changed
                if (currentReferenceFrameId != newReferenceFrame.GetHashCode())
                {
                    previousReferenceFrame = currentReferenceFrame;

                    // Calculate the new relative position and rotation
                    // The INTENT is to maintain the same forward direction of the interactive object
                    // as it crosses reference frame boundaries. These reference object may
                    // appear on be on a similar plane but be rotated say 90, 180 or 270 deg.
                    // We also, if possible want the character up direction to match the normal
                    // or new reference object.                    

                    // NOTE: CURRENTLY DOESN'T WORK WTIH SCALED MESHES
                    //currentRelativePosition = newReferenceFrame.InverseTransformPoint(transform.position);
                    currentReferenceFramePosition = Quaternion.Inverse(newReferenceFrame.rotation) * (transform.position - newReferenceFrame.position);

                    // Project the relative rotation into the new reference frame
                    currentRelativeRotation = Quaternion.Inverse(newReferenceFrame.rotation) * transform.rotation;

                    // Set the current reference frame transform
                    currentReferenceFrame = newReferenceFrame;

                    // Update reference frame data
                    currentReferenceFramePosition = currentReferenceFrame.position;
                    currentReferenceFrameRotation = currentReferenceFrame.rotation;
                    currentReferenceFrameUp = currentReferenceFrame.up;

                    // GetHashCode seems to return same value as GetInstanceID()
                    // GetHashCode is faster than GetInstanceID() when doing comparisons.
                    currentReferenceFrameId = currentReferenceFrame.GetHashCode();
                }
            }
            else
            {
                // Calculate the new relative position and rotation
                currentRelativePosition = Vector3.zero;
                currentRelativeRotation = Quaternion.identity;

                // Currently we do not have a reference frame assigned
                currentReferenceFrame = null;

                // Update reference frame data
                currentReferenceFramePosition = Vector3.zero;
                currentReferenceFrameRotation = Quaternion.identity;
                currentReferenceFrameUp = Vector3.up;

                currentReferenceFrameId = 0;
            }
        }

        /// <summary>
        /// Set the gravity in metres per second per second.
        /// </summary>
        /// <param name="newAcceleration">0 or greater. Earth gravity 9.81.</param>
        public void SetGravitationalAcceleration (float newAcceleration)
        {
            gravitationalAcceleration = newAcceleration >= 0 ? gravitationalAcceleration : -newAcceleration;
        }

        /// <summary>
        /// Set the world space direction that gravity acts upon the dynamic object when GravityMode is Direction. 
        /// </summary>
        /// <param name="newDirection"></param>
        public void SetGravityDirection (Vector3 newDirection)
        {
            if (newDirection.sqrMagnitude < Mathf.Epsilon)
            {
                gravityDirection = Vector3.forward;
            }
            else
            {
                gravityDirection = newDirection.normalized;
            }
        }

        /// <summary>
        /// Set the method used to determine in which direction gravity is acting.
        /// </summary>
        /// <param name="newGravityMode"></param>
        public void SetGravityMode (StickyManager.GravityMode newGravityMode)
        {
            gravityMode = newGravityMode;
            gravityModeInt = (int)gravityMode;
        }

        /// <summary>
        /// Change the way reference frames are determined.
        /// </summary>
        /// <param name="newRefUpdateType"></param>
        public void SetReferenceUpdateType (StickyControlModule.ReferenceUpdateType newRefUpdateType)
        {
            #if UNITY_EDITOR
            Debug.LogWarning("StickyDynamicModule - SetReferenceUpdateType is currently not implemented");
            #endif
        }

        #endregion

    }
}