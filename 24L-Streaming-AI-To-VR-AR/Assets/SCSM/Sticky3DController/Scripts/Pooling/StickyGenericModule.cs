using UnityEngine;

// Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This component is used with the Sticky Manager to make an object in your scene poolable. Instead of
    /// instantiating and destroying an object multiple times, the object is pooled and enabled or
    /// disabled as required.
    /// See also Demos\scripts\SampleGenericTextModule.cs and SamplePopupOptions.cs
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Objects/Sticky Generic Module")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyGenericModule : MonoBehaviour, IStickyPoolable
    {
        #region Public Enumerations

        #endregion

        #region Public Variables

        [Tooltip("The starting size of the pool")]
        /// <summary>
        /// The starting size of the pool.
        /// </summary>
        public int minPoolSize = 5;

        [Tooltip("The maximum allowed size of the pool.")]
        /// <summary>
        /// The maximum allowed size of the pool.
        /// </summary>
        public int maxPoolSize = 100;

        [Tooltip("The object will be automatically despawned after this amount of time (in seconds) has elapsed.")]
        /// <summary>
        /// The object will be automatically despawned after this amount of time (in seconds) has elapsed.
        /// If the value is less than 0, it won't automatically despawn.
        /// </summary>
        public float despawnTime = 3f;

        [Tooltip("Does this object get parented to another object when activated? If so, it will be reparented to the pool transform after use.")]
        /// <summary>
        /// Does this object get parented to another object when activated? If so,
        /// it will be reparented to the pool transform after use.
        /// </summary>
        public bool isReparented = false;

        #endregion

        #region Public Variables - Editor

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [HideInInspector] public bool allowRepaint = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// [READONLY] Is the Module initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// Is the Module enabled?
        /// See also EnableModule() and DisableModule().
        /// </summary>
        public bool IsModuleEnabled { get { return isModuleEnabled; } }

        #endregion

        #region Private or Internal Variables
        protected bool isInitialised = false;
        protected bool isModuleEnabled = true;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used to determine uniqueness
        /// </summary>
        [System.NonSerialized] internal uint itemSequenceNumber;

        /// <summary>
        /// [INTERNAL ONLY]
        /// If isReparented is true, this is the original parent
        /// transform used in the pooling system.
        /// </summary>
        [System.NonSerialized] internal Transform poolParentTrfm;
        #endregion

        #region Internal Static Variables
        internal static uint nextSequenceNumber = 1;
        internal readonly static string destroyMethodName = "DestroyGenericObject";
        #endregion

        #region Private or internal Methods

        /// <summary>
        /// Makes this StickyGenericModule unique from all others that have gone before them.
        /// This is called every time the effect is Initialised. 
        /// </summary>
        internal void IncrementSequenceNumber()
        {
            itemSequenceNumber = nextSequenceNumber++;
            // if sequence number needs to be wrapped, do so to a high-ish number that is unlikely to be in use 
            if (nextSequenceNumber > uint.MaxValue - 100) { nextSequenceNumber = 100000; }
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Return the Generic Module to the pool managed by
        /// StickyManager. If this object was parented to another
        /// transform when it was activated, reparent it to the original
        /// pool.
        /// </summary>
        protected virtual void ReturnToPool()
        {
            isInitialised = false;

            // Prevent any Invokes from trying to run after it is returned to the pool.
            // For example, it may be "destroyed" before the despawn time.
            CancelInvoke();

            // Deactivate the module object
            gameObject.SetActive(false);

            if (isReparented && poolParentTrfm != null)
            {
                transform.SetParent(poolParentTrfm);
            }
        }

        #endregion

        #region Public Virtual API Methods

        /// <summary>
        /// Destroy (deactivate) the Generic Module. Returns it to the pool.
        /// </summary>
        public virtual void DestroyGenericObject()
        {
            ReturnToPool();
        }

        /// <summary>
        /// Temporarily disable or pause the module. Typically called automatically from
        /// stickyManager.PauseGenericObjects().
        /// </summary>
        public virtual void DisableModule()
        {
            if (isModuleEnabled)
            {
                isModuleEnabled = false;
            }
        }

        /// <summary>
        /// Re-enable or unpause the module. Typically called automatically from
        /// stickyManager.ResumeGenericObjects().
        /// </summary>
        public virtual void EnableModule()
        {
            if (!isModuleEnabled)
            {
                isModuleEnabled = true;
            }
        }

        /// <summary>
        /// Initialise the module after it has been activated in the pool managed by StickyManager.
        /// Currently igParms are required to be set for generic object Initialise(..), however we
        /// still pass them in for future expansion.
        /// </summary>
        /// <param name="igParms"></param>
        /// <returns></returns>
        public virtual uint Initialise (S3DInstantiateGenericObjectParameters igParms)
        {
            IncrementSequenceNumber();

            if (isReparented) { poolParentTrfm = transform.parent; }

            // After a given amount of time, automatically destroy this object
            // i.e. Return it to the pool.
            if (despawnTime > 0f && !igParms.overrideAutoDestroy) { Invoke(destroyMethodName, despawnTime); }

            isInitialised = true;

            return itemSequenceNumber;
        }

        #endregion

        #region Public General API Methods

        /// <summary>
        /// Get a local space position on the module, given a world space position
        /// (converts a world space position to a local space position on the transform)
        /// </summary>
        /// <param name="wsPosition"></param>
        /// <returns></returns>
        public Vector3 GetLocalPosition (Vector3 wsPosition)
        {
            return Quaternion.Inverse(transform.rotation) * (wsPosition - transform.position);
        }

        #endregion
    }
}