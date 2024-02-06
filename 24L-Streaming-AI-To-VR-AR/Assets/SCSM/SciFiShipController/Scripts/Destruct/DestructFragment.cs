using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Used in the destruct module to keep track of object fragments
    /// </summary>
    public class DestructFragment
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        public bool isDespawned;
        public bool isPaused;
        public MeshRenderer mRen;
        public Rigidbody rBody;
        public bool isDynamic;
        public Vector3 originalLocalPosition;
        public Quaternion originalLocalRotation;
        public float timeUnmoving;
        public bool isObjectVisible;
        public float mass;

        #endregion

        #region Constructors
        public DestructFragment()
        {
            SetClassDefaults();
        }

        #endregion

        #region Public Member Methods

        /// <summary>
        /// Set the defaults values for this class
        /// </summary>
        public void SetClassDefaults()
        {
            isDespawned = false;
            isPaused = false;
            mRen = null;
            rBody = null;
            isDynamic = false;
            originalLocalPosition = Vector3.zero;
            originalLocalRotation = Quaternion.identity;
            timeUnmoving = 0f;
            isObjectVisible = false;
            mass = 1f;
        }

        #endregion
    }
}
