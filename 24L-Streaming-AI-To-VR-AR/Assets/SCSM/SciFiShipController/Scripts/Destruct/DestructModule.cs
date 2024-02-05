using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This module enables you to manage a prefab as it breaks into fragments.
    /// TODO: consider using simple capule or box colliders on fragments
    /// TODO: deal with proximityTrigger (should be per fragment or whole object)
    /// TODO: destructFragment.isObjectVisible isn't being set
    /// TODO: set explosion direction based on hit normal.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Object Components/Destruct Module")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class DestructModule : MonoBehaviour
    {
        #region Enumerations

        public enum DisableRigidbodyMode
        {
            Destroy = 0,
            SetAsKinematic = 1,
            DontDisable = 2
        }

        public enum DespawnCondition
        {
            Time = 0,
            DontDespawn = 1
            //DistanceFromOrigin
        }

        #endregion

        #region Public Variables

        /// <summary>
        /// Should the explosion occur immediately the scene is started or the module is instantiated?
        /// This should be disabled if used with a pooling system.
        /// </summary>
        public bool isExplodeOnStart = false;

        /// <summary>
        /// Whether pooling is used when spawning destruct objects of this type.
        /// Currently we don't support changing this at runtime.
        /// </summary>
        public bool usePooling = true;
        /// <summary>
        /// The starting size of the pool.
        /// </summary>
        public int minPoolSize = 5;
        /// <summary>
        /// The maximum allowed size of the pool.
        /// </summary>
        public int maxPoolSize = 100;

        /// <summary>
        /// Add rigidbodies to the fragments in the prefab
        /// </summary>
        public bool isAddRigidBodiesEnabled = false;

        /// <summary>
        /// Add mesh colliders to the fragments in the prefab
        /// </summary>
        public bool isAddMeshCollidersEnabled = false;

        /// <summary>
        /// The default effective range of the blast
        /// </summary>
        public float explosionRadius = 5f;

        /// <summary>
        /// The default power of the blast
        /// </summary>
        public float explosionPower = 100f;

        /// <summary>
        /// When the speed in any direction of the fragment falls below this value, the fragment is considered to have stopped moving
        /// </summary>
        public float unmovingVelocity = 0.1f;

        /// <summary>
        /// This is the total mass of all the fragments
        /// </summary>
        public float mass = 1f;

        /// <summary>
        /// This may be more accurate when there the is a lot of variation between the size of each fragment.
        /// This method is slower during the initial configuration phase and may affect performance of non-pooled modules.
        /// </summary>
        public bool isCalcMassByBounds = false;

        /// <summary>
        /// The amount of drag the fragments have. A solid block of metal would be 0.001, while a feather would be 10.
        /// </summary>
        public float drag = 0.01f;

        /// <summary>
        /// The amount of angular drag the fragments have
        /// </summary>
        public float angularDrag = 0.05f;

        /// <summary>
        /// Fragments are effected by gravity
        /// </summary>
        public bool useGravity = true;

        /// <summary>
        /// The rigidbody interpolation
        /// </summary>
        public RigidbodyInterpolation interpolation = RigidbodyInterpolation.None;

        /// <summary>
        /// The rigidbody collision detection mode
        /// </summary>
        public CollisionDetectionMode collisionDetection = CollisionDetectionMode.Discrete;

        /// <summary>
        /// If a fragment has been unmoving for more than the maximum time, set the object as static
        /// and "disable" the rigidbody according to the "Disable Rigidbody Mode".
        /// </summary>
        public float maxTimeUnmoving = 1f;

        /// <summary>
        /// Start in Static mode rather than Dynamic.
        /// This is NOT compatible with disableRigidbodyMode == DisableRigidbodyMode.Destroy
        /// </summary>
        public bool isStartStatic = false;

        /// <summary>
        /// After this time (in seconds), the destruct object is automatically despawned or removed from the scene.
        /// </summary>
        public float despawnTime = 5f;

        // How the rigidbodies are disabled when the object is considered to be Static.
        // "Destroy" removes the rigidbody component and adds it back in as needed - best performance for unmoving objects
        // "Set As Kinematic" sets the rigidbody to kinematic - half/half performance
        // "Don't Disable" uses Unity default rigidbody behaviour where rigidbodies go into sleep mode until something collides with them
        // - best for objects being moved
        // "Destroy" is best for objects far away from each other, and "Don't Disable" is best for objects close to each other
        public DisableRigidbodyMode disableRigidbodyMode = DisableRigidbodyMode.Destroy;

        // The conditions under which this object despawns
        public DespawnCondition despawnCondition = DespawnCondition.DontDespawn;

        /// <summary>
        /// Wait until the fragment is not being rendered by the camera before being despawned
        /// NOTE: This has not been implemented yet
        /// </summary>
        public bool waitUntilNotRenderedToDespawn = false;

        /// <summary>
        /// Wait until the fragment is set to static before being despawned
        /// </summary>
        public bool waitUntilStaticToDespawn = true;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [HideInInspector] public bool allowRepaint = false;
        #endregion

        #region Public Properties

        /// <summary>
        /// Has the module been initially configured?
        /// </summary>
        public bool IsInitialised { get; private set; }

        /// <summary>
        /// Is the destruct module currently in use?
        /// </summary>
        public bool IsActivated { get; private set; }

        /// <summary>
        /// [READONLY] Is the destruct module currently ready for use, or has it been paused?
        /// </summary>
        public bool IsDestructEnabled { get { return isDestructEnabled; } }


        public float EstimatedDespawnTime { get { return IsActivated && isDestructEnabled && despawnCondition == DespawnCondition.Time ? despawnTimer - despawnTime : float.PositiveInfinity;  } }

        #endregion

        #region Private and Internal Variables
        [System.NonSerialized] private List<DestructFragment> destructFragmentList = null;
        [System.NonSerialized] private MeshRenderer[] meshRenderers = null;

        private int numFragments = 0;
        private int numActiveFragments = 0;
        private float sqrUnmovingVelocity;
        private bool isDynamic = false;
        //private SphereCollider proximityTrigger;
        private float despawnTimer = 0f;

        /// <summary>
        /// Is the destruct module currently running (ready of use), or has it been paused?
        /// </summary>
        private bool isDestructEnabled = true;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used to determine uniqueness
        /// </summary>
        [System.NonSerialized] internal uint itemSequenceNumber;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Use with pooling which gets set in Activate(..)
        /// </summary>
        [System.NonSerialized] internal int destructPoolListIndex;

        #endregion

        #region Internal Static Variables
        internal static uint nextSequenceNumber = 1;
        #endregion

        #region Initialise Methods

        // Start is called before the first frame update
        void Start()
        {
            InitialiseDestruct();

            if (isExplodeOnStart)
            {
                InstantiateDestructParameters dstParms = new InstantiateDestructParameters
                {
                    position = transform.position,
                    rotation = transform.rotation,
                    explosionPowerFactor = 1f,
                    explosionRadiusFactor = 1f
                };

                ActivateModule(-1);
                Explode(dstParms);
            }
        }

        /// <summary>
        /// The module must always be initialised AND activated before it can be used.
        /// It should only be initialised once, while it can be activated and deactivated
        /// multiple times.
        /// </summary>
        internal bool InitialiseDestruct()
        {
            if (IsInitialised) { return true; }

            // Assume non-pooling. This gets set when Activate(..) is called.
            destructPoolListIndex = -1;

            // Calculate square of unmovingVelocity so that at runtime we can use the 
            // vector3.sqrMagnitude function instead of the slower vector3.magnitude to compare velocities
            sqrUnmovingVelocity = unmovingVelocity * unmovingVelocity;

            meshRenderers = GetComponentsInChildren<MeshRenderer>();

            numFragments = meshRenderers == null ? 0 : meshRenderers.Length;

            // When first initialsed, all fragments are active.
            numActiveFragments = numFragments;

            if (numFragments > 0)
            {
                destructFragmentList = new List<DestructFragment>(numFragments);

                if (destructFragmentList != null)
                {
                    int numFragmentsAdded = 0;
                    GameObject fragmentGO = null;

                    #region Calculate Total Bounds

                    float totalVolume = 0f;

                    // This will calculate the combined volume of all fragments as if they were laid out side by side.
                    // It uses bounds to calculate volume rather than the actual volume of the fragment.
                    if (isCalcMassByBounds)
                    {
                        for (int fIdx = 0; fIdx < numFragments; fIdx++)
                        {
                            MeshRenderer meshRenderer = meshRenderers[fIdx];
                            Bounds _fragmentBounds = meshRenderer.bounds;

                            totalVolume += _fragmentBounds.size.x * _fragmentBounds.size.y * _fragmentBounds.size.z;
                        }
                    }
                    #endregion

                    #region Populate the list of destructFragments.
                    for (int fIdx = 0; fIdx < numFragments; fIdx++)
                    {
                        MeshRenderer meshRenderer = meshRenderers[fIdx];
                        DestructFragment destructFragment = new DestructFragment();

                        if (destructFragment != null)
                        {
                            fragmentGO = meshRenderer.gameObject;
                            destructFragment.originalLocalPosition = fragmentGO.transform.localPosition;
                            destructFragment.originalLocalRotation = fragmentGO.transform.localRotation;

                            destructFragment.mRen = meshRenderer;
                            destructFragment.isObjectVisible = meshRenderer.isVisible;

                            // Calculate fragment mass
                            if (isCalcMassByBounds && totalVolume > 0f)
                            {
                                Bounds _fragmentBounds = meshRenderer.bounds;
                                destructFragment.mass = (_fragmentBounds.size.x * _fragmentBounds.size.y * _fragmentBounds.size.z) / totalVolume;

                                //Debug.Log("[DEBUG] fragment mass: " + destructFragment.mass);
                            }
                            else { destructFragment.mass = mass / numFragments; }

                            // Add a mesh collider if required
                            if (isAddMeshCollidersEnabled)
                            {
                                MeshCollider meshCollider = meshRenderer.GetComponent<MeshCollider>();
                                if (meshCollider == null)
                                {
                                    meshCollider = fragmentGO.AddComponent<MeshCollider>();
                                }

                                if (meshCollider != null)
                                {
                                    // Required for non-kinematic mesh colliders
                                    meshCollider.convex = true;
                                }
                            }

                            Rigidbody rBody = meshRenderer.GetComponent<Rigidbody>();

                            // Add a rigid body if required
                            if (isAddRigidBodiesEnabled && rBody == null)
                            {
                                rBody = fragmentGO.AddComponent<Rigidbody>();
                            }

                            if (rBody != null)
                            {
                                destructFragment.rBody = rBody;
                            }

                            if (disableRigidbodyMode != DisableRigidbodyMode.Destroy)
                            {
                                SetUpRigidbody(destructFragment, useGravity, drag, angularDrag, interpolation, collisionDetection);
                            }

                            if (isStartStatic)
                            {
                                // If starting static, set the object as static
                                SetStatic(destructFragment);
                            }
                            else
                            {
                                // Else set the object as dynamic
                                SetDynamic(destructFragment);
                            }

                            destructFragmentList.Add(destructFragment);
                            numFragmentsAdded++;
                        }
                    }
                    #endregion

                    IsInitialised = numFragmentsAdded == numFragments;
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ERROR: DestructMoudle.Initialise() - could not create a new list of fragments. PLEASE REPORT"); }
                #endif
            }

            return IsInitialised;
        }

        /// <summary>
        /// INCOMPLETE - should turn off renderers etc
        /// </summary>
        internal void DeactivateModule()
        {
            IsActivated = false;
        }

        /// <summary>
        /// Turn the destruct module on and get it ready for use
        /// </summary>
        internal uint ActivateModule(int poolIndex)
        {
            if (InitialiseDestruct())
            {
                IncrementSequenceNumber();

                // Reset despawn timer
                despawnTimer = 0f;

                if (usePooling)
                {
                    for (int fIdx = 0; fIdx < numFragments; fIdx++)
                    {
                        DestructFragment destructFragment = destructFragmentList[fIdx];

                        if (destructFragment != null)
                        {
                            destructFragment.timeUnmoving = 0f;

                            if (destructFragment.isDespawned)
                            {
                                Transform transform = destructFragment.mRen == null ? null : transform = destructFragment.mRen.transform;

                                // Reset the rigidbody
                                Rigidbody rBody = destructFragment.rBody;

                                if (rBody != null && transform != null)
                                {
                                    // Reset position and rotation
                                    // At this point it may be Kinematic and should be only moved from
                                    // FixedUpdate with rBody.MovePosition(..) and rBody.MoveRotation(..). However,
                                    // as we're about to turn off Kinematic so we should be ok... maybe.
                                    transform.localPosition = destructFragment.originalLocalPosition;
                                    transform.localRotation = destructFragment.originalLocalRotation;

                                    rBody.velocity = Vector3.zero;
                                    rBody.angularVelocity = Vector3.zero;
                                }

                                if (transform != null)
                                {
                                    transform.gameObject.SetActive(true);
                                }

                                destructFragment.isDespawned = false;
                            }
                        }
                    }

                    numActiveFragments = numFragments;

                    destructPoolListIndex = poolIndex;
                }
                else { destructPoolListIndex = -1; }

                SetDynamicAll();
                isDestructEnabled = true;
                IsActivated = true;

                return itemSequenceNumber;
            }
            else { return 0; }
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            if (!isDestructEnabled || !IsActivated) { return; }

            #region Disabling dynamic
            // Determine what to do when a fragment stops moving
            if (disableRigidbodyMode != DisableRigidbodyMode.DontDisable)
            {
                for (int fIdx = 0; fIdx < numFragments; fIdx++)
                {
                    DestructFragment destructFragment = destructFragmentList[fIdx];
                    if (destructFragment != null && destructFragment.isDynamic && !destructFragment.isDespawned && destructFragment.rBody != null)
                    {
                        // Check if the object is currently "unmoving"
                        if (destructFragment.rBody.velocity.sqrMagnitude < sqrUnmovingVelocity)
                        {
                            // If it isn't moving increment the unmoving timer
                            destructFragment.timeUnmoving += Time.deltaTime;

                            if (destructFragment.timeUnmoving > maxTimeUnmoving)
                            {
                                // If it has been unmoving for more than the maximum time set the object as static
                                SetStatic(destructFragment);
                                // Reset the timer
                                destructFragment.timeUnmoving = 0f;
                            }
                        }
                        else
                        {
                            // If it is moving reset the unmoving timer
                            destructFragment.timeUnmoving = 0f;
                        }
                    }
                }
            }
            #endregion

            #region Despawn based on elapsed time
            if (despawnCondition == DespawnCondition.Time)
            {
                // Increment the despawn timer
                despawnTimer += Time.deltaTime;

                for (int fIdx = 0; fIdx < numFragments; fIdx++)
                {
                    DestructFragment destructFragment = destructFragmentList[fIdx];

                    // If needed wait until it isn't rendered to despawn
                    if (destructFragment != null && !destructFragment.isDespawned && (!waitUntilNotRenderedToDespawn || !destructFragment.isObjectVisible))
                    {
                        // If needed to wait until it is static to despawn
                        if (!waitUntilStaticToDespawn || !destructFragment.isDynamic || (disableRigidbodyMode == DisableRigidbodyMode.DontDisable && destructFragment.rBody.IsSleeping()))
                        {
                            // Only despawn if a given time has elapsed
                            if (despawnTimer > despawnTime)
                            {
                                // Despawn the fragment
                                Despawn(destructFragment, fIdx);
                            }
                        }
                    }
                }
            }
            #endregion

            //// Despawn based on distance
            //else if (despawnCondition == DespawnCondition.DistanceFromOrigin)
            //{
            //    // If needed wait until it isn't rendered to despawn
            //    if (!waitUntilNotRenderedToDespawn || !objectVisible)
            //    {
            //        // If needed wait until it is static to despawn
            //        if (!waitUntilStaticToDespawn || !isDynamic || (disableRigidbodyMode == DisableRigidbodyMode.DontDisable && rBody.IsSleeping()))
            //        {
            //            // Only despawn if it is more than a given distance from its starting point
            //            if (Vector3.Distance(origin, transform.position) > despawnDistance)
            //            {
            //                // Despawn the object
            //                Despawn();
            //            }
            //        }
            //    }
            //}
        }

        #endregion

        #region Private and Internal Methods

        /// <summary>
        /// Makes this DestructModule unique from all others that have gone before them.
        /// This is called every time beam is Activated. 
        /// </summary>
        internal void IncrementSequenceNumber()
        {
            itemSequenceNumber = nextSequenceNumber++;
            // if sequence number needs to be wrapped, do so to a high-ish number that is unlikely to be in use 
            if (nextSequenceNumber > uint.MaxValue - 100) { nextSequenceNumber = 100000; }
        }

        /// <summary>
        /// CURRENTLY NOT IMPLEMENTED
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            // If the proximity trigger collider is triggered when the object is static...
            if (disableRigidbodyMode != DisableRigidbodyMode.DontDisable && !isDynamic)
            {
                // Check whether the other collider has a rigidbody attached and is moving.
                Rigidbody otherRBody = other.attachedRigidbody;
                if (otherRBody != null && otherRBody.velocity.sqrMagnitude > sqrUnmovingVelocity)
                {
                    SetDynamicAll();
                }
            }
        }

        // Use OnBecameVisible and OnBecameInvisible events to track object visibility
        // INCOMPLETE - currently only works if there is a mesh renderer attached to the
        // parent gameobject.
        private void OnBecameVisible()
        {
            //Debug.Log("[DEBUG] Become visible T:" + Time.time);
            //objectVisible = true;
        }

        private void OnBecameInvisible()
        {
            //objectVisible = false;
            //Debug.Log("[DEBUG] Become invisible T:" + Time.time);
        }

        private void SetDynamicAll()
        {
            for (int fIdx = 0; fIdx < numFragments; fIdx++)
            {
                DestructFragment destructFragment = destructFragmentList[fIdx];
                if (destructFragment != null) { SetDynamic(destructFragment); }
            }
        }

        private void SetDynamic(DestructFragment destructFragment)
        {
            // Disable the static flags, enable the rigidbody and disable the proximity trigger
            //if (proximityTrigger != null) { proximityTrigger.enabled = false; }
            if (disableRigidbodyMode == DisableRigidbodyMode.Destroy)
            {
                SetUpRigidbody(destructFragment, useGravity, drag, angularDrag, interpolation, collisionDetection);
            }
            else if (disableRigidbodyMode == DisableRigidbodyMode.SetAsKinematic)
            {
                if (destructFragment.rBody != null)
                {
                    destructFragment.rBody.isKinematic = false;
                    destructFragment.rBody.collisionDetectionMode = collisionDetection;
                }
            }
            // Set the state of isDynamic
            destructFragment.isDynamic = true;
        }

        private void SetStaticAll()
        {
            for (int fIdx = 0; fIdx < numFragments; fIdx++)
            {
                DestructFragment destructFragment = destructFragmentList[fIdx];
                if (destructFragment != null) { SetStatic(destructFragment); }
            }
        }

        private void SetStatic(DestructFragment destructFragment)
        {
            // Enable the static flags (except for batching), disable the rigidbody and enable the proximity trigger
            // NOTE: batching and proximity trigger are not implemented yet.
            //if (proximityTrigger != null) { proximityTrigger.enabled = true; }

            if (disableRigidbodyMode == DisableRigidbodyMode.Destroy)
            {
                // Destroy may not happen in the current frame. If SetDynamic is called immediately after SetStatic,
                // like on activation when isStartStatic is true, SetDynamic may add the rigidbody only to have it
                // immediately destroyed.
                if (destructFragment.rBody != null) { Destroy(destructFragment.rBody); destructFragment.rBody = null; }
            }
            else if (disableRigidbodyMode == DisableRigidbodyMode.SetAsKinematic)
            {
                if (destructFragment.rBody != null)
                {
                    destructFragment.rBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    destructFragment.rBody.isKinematic = true;
                }
            }
            // Set the state of isDynamic
            destructFragment.isDynamic = false;
        }

        // Set up a rigidbody for the fragment
        private void SetUpRigidbody(DestructFragment destructFragment, bool rUseGravity, float rDrag, float rAngularDrag, RigidbodyInterpolation rInterpolation, CollisionDetectionMode rCollisionDetection)
        {
            if (destructFragment != null && destructFragment.mRen != null)
            {
                destructFragment.rBody = destructFragment.mRen.GetComponent<Rigidbody>();
                if (destructFragment.rBody == null) { destructFragment.rBody = destructFragment.mRen.gameObject.AddComponent<Rigidbody>(); }
                destructFragment.rBody.mass = destructFragment.mass;
                destructFragment.rBody.drag = rDrag;
                destructFragment.rBody.angularDrag = rAngularDrag;
                destructFragment.rBody.interpolation = rInterpolation;
                destructFragment.rBody.collisionDetectionMode = rCollisionDetection;
                destructFragment.rBody.useGravity = rUseGravity;
            }
        }

        /// <summary>
        /// Set up a spherecollider and return that spherecollider
        /// CURENTLY NOT FULLY IMPLEMENTED - set for the whole object not per fragment
        /// </summary>
        /// <param name="proximity"></param>
        /// <returns></returns>
        private SphereCollider SetUpProximityTrigger(float proximity)
        {
            // Set up a "proximity trigger" collider: a sphere collider of a defined radius with "isTrigger" enabled
            SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            // Radius needs to be scaled down by the scale of the object to be a correct proximity measurement
            Vector3 objectScale = transform.lossyScale;
            trigger.radius = proximity / Mathf.Max(objectScale.x, objectScale.y, objectScale.z);
            return trigger;
        }

        /// <summary>
        /// If no more fragements to despawn, this then calls Despawn().
        /// </summary>
        /// <param name="destructFragment"></param>
        /// <param name="fragmentIndex"></param>
        private void Despawn(DestructFragment destructFragment, int fragmentIndex)
        {
            if (usePooling)
            { 
                // Deactivate the fragment
                destructFragment.isDespawned = true;
                destructFragment.mRen.gameObject.SetActive(false);
            }
            else
            {
                Destroy(destructFragment.mRen.gameObject);
                destructFragmentList.RemoveAt(fragmentIndex);
                numFragments--;
            }

            numActiveFragments--;

            if (numActiveFragments < 1)
            {
                Despawn();
            }
        }

        // Despawn the object
        private void Despawn()
        {
            if (usePooling && destructPoolListIndex >= 0)
            {
                IsActivated = false;
                // Return it to the pool
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// UNTESTED
        /// </summary>
        /// <param name="isEnabled"></param>
        private void EnableOrDisableDestruct(bool isEnabled)
        {     
            if (IsActivated)
            {
                for (int fIdx = 0; fIdx < numFragments; fIdx++)
                {
                    DestructFragment destructFragment = destructFragmentList[fIdx];

                    // Fragments that are already despawned and those without a rigidbody do not need to be paused
                    if (destructFragment != null && !destructFragment.isDespawned && destructFragment.rBody != null)
                    {
                        // If enabling and this fragement was paused, unpause it now
                        if (isEnabled && destructFragment.isPaused && destructFragment.isDynamic)
                        {
                            destructFragment.rBody.isKinematic = false;
                            destructFragment.rBody.collisionDetectionMode = collisionDetection;
                        }
                        else if (!isEnabled)
                        {
                            destructFragment.rBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                            destructFragment.rBody.isKinematic = true;
                        }

                        destructFragment.rBody.detectCollisions = isEnabled;
                        destructFragment.isPaused = !isEnabled;
                    }
                }
            }

            isDestructEnabled = isEnabled;
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Disable or pause the Destruct module
        /// </summary>
        public void DisableDestruct()
        {
            EnableOrDisableDestruct(false);
        }

        /// <summary>
        /// Enable or unpause the Destruct module
        /// </summary>
        public void EnableDestruct()
        {
            EnableOrDisableDestruct(true);
        }

        /// <summary>
        /// Explode or begin the destruction process
        /// </summary>
        /// <param name="dstParms"></param>
        public void Explode(InstantiateDestructParameters dstParms)
        {
            if (IsActivated)
            {
                for (int fIdx = 0; fIdx < numFragments; fIdx++)
                {
                    DestructFragment destructFragment = destructFragmentList[fIdx];

                    if (destructFragment != null)
                    {
                        if (destructFragment.rBody != null)
                        {
                            destructFragment.rBody.AddExplosionForce(explosionPower * dstParms.explosionPowerFactor, dstParms.position, explosionRadius * dstParms.explosionRadiusFactor);
                        }
                    }
                }
            }
        }

        #endregion
    }
}