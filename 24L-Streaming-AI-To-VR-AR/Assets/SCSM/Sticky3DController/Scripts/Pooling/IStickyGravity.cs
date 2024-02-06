using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Interface for objects that implement dynamic gravity
    /// </summary>
    public interface IStickyGravity
    {
        #region Public Properties

        /// <summary>
        /// [READONLY] Get the current reference frame transform
        /// </summary>
        Transform CurrentReferenceFrame { get; }

        /// <summary>
        /// Get or set the gravity in metres per second per second
        /// </summary>
        float GravitationalAcceleration { get; set; }

        /// <summary>
        /// Get or set the world space direction that gravity acts upon the dynamic object when GravityMode is Direction.
        /// </summary>
        Vector3 GravityDirection { get; set; }

        /// <summary>
        /// Get or set the method used to determine in which direction gravity is acting.
        /// </summary>
        StickyManager.GravityMode GravityMode { get; set; }

        /// <summary>
        /// [READONLY]
        /// Is there a rigidbody currently attached to the object?
        /// </summary>
        bool HasRigidbody { get; }

        /// <summary>
        /// [READONLY]
        /// Get the rigidbody attached to the object (if any)
        /// </summary>
        Rigidbody ObjectRigidbody { get; }

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Reset gravity to default (starting) values.
        /// Typically gets called automatically when required.
        /// </summary>
        void ResetGravity();

        /// <summary>
        /// Attempt to restore the current reference frame, to the initial or default setting.
        /// This is automatically called when exiting a StickyZone.
        /// </summary>
        void RestoreDefaultReferenceFrame();

        /// <summary>
        /// Attempt to restore the previous reference frame that was being used before what is
        /// currently set. NOTE: We do not support nesting.
        /// </summary>
        void RestorePreviousReferenceFrame();

        /// <summary>
        /// Sets the current reference frame.
        /// </summary>
        /// <param name="newReferenceFrame"></param>
        void SetCurrentReferenceFrame (Transform newReferenceFrame);

        /// <summary>
        /// Change the way reference frames are determined.
        /// </summary>
        /// <param name="newRefUpdateType"></param>
        void SetReferenceUpdateType (StickyControlModule.ReferenceUpdateType newRefUpdateType);

        #endregion

    }
}