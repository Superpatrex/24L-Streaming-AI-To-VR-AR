using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// An interactive container that holds ammo for a weapon
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyMagazine : StickyInteractive, IStickyGravity
    {
        #region Public Variables

        #endregion

        #region Public Properties - General

        /// <summary>
        /// Get or set the amount of ammo currently in the magazine. It cannot exceed the Mag Capacity.
        /// </summary>
        public int AmmoCount { get { return ammoCount; } set { SetAmmoCount(value); } }

        /// <summary>
        /// [READONLY]
        /// Get the (common) array of ammo types.
        /// </summary>
        public S3DAmmoTypes AmmoTypes { get { return ammoTypes; } }

        /// <summary>
        /// Get or set the ammo type that can be placed in this magazine
        /// </summary>
        public S3DAmmo.AmmoType CompatibleAmmoType { get { return compatibleAmmoType; } set { SetCompatibleAmmoType(value); } }

        /// <summary>
        /// [READONLY]
        /// Get the ammo type that can be placed in this magazine as an integer.
        /// </summary>
        public int CompatibleAmmoTypeInt { get { return isInitialised ? compatibleAmmoTypeInt : (int)compatibleAmmoType; } }

        /// <summary>
        ///  Get or set disable regular colliders when equipped (attached) to a weapon
        /// </summary>
        public bool IsDisableRegularColOnEquip { get { return isDisableRegularColOnEquip; } set { isDisableRegularColOnEquip = value; } }

        /// <summary>
        /// [READONLY]
        /// Is the magazine empty?
        /// </summary>
        public bool IsEmpty { get { return ammoCount == 0; } }

        /// <summary>
        /// [READONLY]
        /// Is the magazine full of ammo?
        /// </summary>
        public bool IsFull { get { return ammoCount == magCapacity; } }

        /// <summary>
        /// [READONLY]
        /// Has the magazine been initialised?
        /// </summary>
        public bool IsMagazineInitialised { get { return isMagazineInitialised; } }

        /// <summary>
        /// Is this an interactive-enabled StickyMagazine?
        /// </summary>
        public override bool IsStickyMagazine { get { return true; } }

        /// <summary>
        /// Get or set the capacity of this magazine. If the Mag Capacity is reduced below the
        /// AmmoCount, the ammo will be reduced to fit in the magazine.
        /// </summary>
        public int MagCapacity { get { return magCapacity; } set { SetMagCapacity(value); } }

        /// <summary>
        /// Get or set the type of magazine. All magazines of the same type can be used with compatible weapons.
        /// </summary>
        public S3DMagType.MagType MagType { get { return magType; } set { SetMagType(value); } }

        /// <summary>
        /// [READONLY]
        /// Get the (common) array of mag types
        /// </summary>
        public S3DMagTypes MagTypes { get { return magTypes; } }

        #endregion

        #region Public Static Variables

        #endregion

        #region Protected Variables - Serialized

        /// <summary>
        /// The amount of ammo currently in the magazine. It cannot exceed the Mag Capacity.
        /// </summary>
        [SerializeField, Range(0, 100)] protected int ammoCount = 0;

        /// <summary>
        /// Reference to a common set of ammo types. All weapons and magazines should use the same scriptable object.
        /// </summary>
        [SerializeField] protected S3DAmmoTypes ammoTypes = null;

        /// <summary>
        /// The ammo type that can be placed in this magazine
        /// </summary>
        [SerializeField] protected S3DAmmo.AmmoType compatibleAmmoType = S3DAmmo.AmmoType.A;

        /// <summary>
        /// Disable regular colliders when equipped (attached) to a weapon
        /// </summary>
        [SerializeField] protected bool isDisableRegularColOnEquip = false;

        /// <summary>
        /// The amount of ammo the magazine can hold
        /// </summary>
        [SerializeField, Range(1, 100)] protected int magCapacity = 10;

        /// <summary>
        /// The type of magazine. All magazines of the same type can be used with compatible weapons.
        /// </summary>
        [SerializeField] protected S3DMagType.MagType magType = S3DMagType.MagType.A;

        /// <summary>
        /// Reference to a common set of magazine types. All weapons and magazines should use the same scriptable object.
        /// </summary>
        [SerializeField] protected S3DMagTypes magTypes = null;

        #endregion

        #region Protected Variables - General

        protected bool isMagazineInitialised = false;

        protected int compatibleAmmoTypeInt = -1;

        protected int magTypeInt = -1;

        #endregion

        #region Protected Variables - Editor

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [SerializeField] protected bool showGeneralSettingsInEditor = true;

        #endregion

        #region Public Delegates

        #endregion

        #region Update Methods

        //protected override void FixedUpdate()
        //{
        //    base.FixedUpdate();

        //    if (isMagazineInitialised)
        //    {

        //    }
        //}

        #endregion

        #region Protected Virtual Methods

        #endregion

        #region Events

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Drop the magazine
        /// </summary>
        /// <param name="stickyID"></param>
        public override void DropObject (int stickyID)
        {
            // This also invokes any drop events setup by the user
            base.DropObject(stickyID);

            if (isInitialised && isGrabbable)
            {
                if (isDisableRegularColOnGrab) { EnableNonTriggerColliders(); }
                if (isDisableTriggerColOnGrab) { EnableTriggerColliders(); }
            }

            if (isMagazineInitialised && isUseGravity)
            {
                SetUpRigidbody();
                RestoreRigidbodySettings();
            }
        }

        /// <summary>
        /// Initialise the magazine
        /// </summary>
        public override void Initialise()
        {
            base.Initialise();

            if (isInitialised)
            {
                //stickyManager = StickyManager.GetOrCreateManager();

                // Keep compiler happy
                if (showGeneralSettingsInEditor) { }

                ReInitialiseMagazine();

                isMagazineInitialised = true;
            }
        }

        /// <summary>
        /// Is the ammo that this magazine currently can hold, the same type as one from
        /// the compatibleAmmoTypes array? For performance reasons, the array is passed
        /// as integers rather than S3DAmmo.AmmoTypes.
        /// </summary>
        /// <param name="compatibleAmmoTypes"></param>
        /// <returns></returns>
        public bool IsAmmoCompatible (int[] compatibleAmmoTypes)
        {
            return compatibleAmmoTypes != null && System.Array.IndexOf(compatibleAmmoTypes, compatibleAmmoTypeInt) > -1;
        }

        /// <summary>
        /// Is this magazine compatible with the given magazine type?
        /// </summary>
        /// <param name="magTypeInt"></param>
        /// <returns></returns>
        public bool IsMagazineCompatible (int magTypeInt)
        {
            return magType == (S3DMagType.MagType)magTypeInt;
        }

        /// <summary>
        /// Is this magazine compatible with the given magazine type?
        /// </summary>
        /// <param name="magTypeInt"></param>
        /// <returns></returns>
        public bool IsMagazineCompatible (S3DMagType.MagType magType)
        {
            return this.magType == magType;
        }

        /// <summary>
        /// Re-initialise the magazine. Gets automatically called when Initialised.
        /// </summary>
        public virtual void ReInitialiseMagazine()
        {
            // If the base class hasn't already been initilised, this has no effect.
            if (isInitialised)
            {
                compatibleAmmoTypeInt = (int)compatibleAmmoType;

                if (ammoCount > magCapacity) { ammoCount = magCapacity; }
            }
        }

        /// <summary>
        /// Set the current amount of ammo in the magazine. It cannot exceed the Mag Capacity
        /// </summary>
        /// <param name="newAmmoCount"></param>
        public void SetAmmoCount (int newAmmoCount)
        {
            if (newAmmoCount < 0) { ammoCount = 0; }
            else if (newAmmoCount > magCapacity) { ammoCount = magCapacity; }
            else
            {
                ammoCount = newAmmoCount;
            }
        }

        /// <summary>
        /// Set the ammo type that can be placed in this magazine
        /// </summary>
        /// <param name="newAmmoType"></param>
        public void SetCompatibleAmmoType (S3DAmmo.AmmoType newAmmoType)
        {
            compatibleAmmoType = newAmmoType;
            compatibleAmmoTypeInt = (int)compatibleAmmoType;
        }

        /// <summary>
        /// Set the amount of ammo this magazine can hold. If it is less than the current Ammo Count,
        /// the ammo will be reduced to fit in the magazine.
        /// </summary>
        /// <param name="newMagCapacity"></param>
        public void SetMagCapacity (int newMagCapacity)
        {
            magCapacity = newMagCapacity < 1 ? 1 : newMagCapacity > 100 ? 100 : newMagCapacity;

            if (ammoCount > magCapacity) { ammoCount = magCapacity; }
        }

        /// <summary>
        /// Set the type of magazine. All magazines of the same type can be used with compatible weapons.
        /// </summary>
        /// <param name="newMagType"></param>
        public void SetMagType (S3DMagType.MagType newMagType)
        {
            magType = newMagType;
        }

        #endregion
    }
}