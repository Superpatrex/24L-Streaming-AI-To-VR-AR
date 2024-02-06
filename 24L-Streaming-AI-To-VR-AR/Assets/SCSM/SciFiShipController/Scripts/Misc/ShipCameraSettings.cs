using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// A ScriptableObject that can contain settings for a ShipCameraModule.
    /// By design it does not include the target ship.
    /// Currently used by ShipWarpModule.
    /// </summary>
    [CreateAssetMenu(fileName = "Ship Camera Settings", menuName = "Sci-Fi Ship Controller/Ship Camera Settings")]
    [HelpURL("https://scsmmedia.com/ssc-documentation")]
    public class ShipCameraSettings : ScriptableObject
    {
        #region Public Variables - General

        /// <summary>
        /// The offset from the target (in local space) for the camera to aim for.
        /// </summary>
        public Vector3 targetOffset = Vector3.zero;

        /// <summary>
        /// The coordinate system used to interpret the target offset.
        /// CameraRotation: The target offset is relative to the rotation of the camera.
        /// TargetRotation: The target offset is relative to the rotation of the target.
        /// TargetRotationFlat: The target offset is relative to the flat rotation of the target.
        /// World: The target offset is relative to the world coordinate system.
        /// </summary>
        public ShipCameraModule.TargetOffsetCoordinates targetOffsetCoordinates = ShipCameraModule.TargetOffsetCoordinates.CameraRotation;

        /// <summary>
        /// If enabled, the camera will stay locked to the optimal camera position.
        /// </summary>
        public bool lockToTargetPosition = false;
        /// <summary>
        /// How quickly the camera moves towards the optimal camera position. Only relevant when lockToTargetPosition is disabled.
        /// </summary>
        [Range(1f, 100f)] public float moveSpeed = 15f;

        /// <summary>
        /// Damp or modify the target position offset based upon the ship pitch and yaw inputs
        /// </summary>
        public bool targetOffsetDamping = false;
        /// <summary>
        /// The rate at which Target Offset Y is modified by ship pitch input. Higher values are more responsive.
        /// </summary>
        [Range(0.01f, 1f)] public float dampingPitchRate = 0.25f;
        /// <summary>
        /// The rate at which the Target Offset Y returns to normal when there is no ship pitch input. Higher values are more responsive.
        /// </summary>
        [Range(0.01f, 1f)] public float dampingPitchGravity = 0.25f;
        /// <summary>
        /// The rate at which Target Offset X is modified by ship yaw input. Higher values are more responsive.
        /// </summary>
        [Range(0.01f, 1f)] public float dampingYawRate = 0.25f;
        /// <summary>
        /// The rate at which the Target Offset X returns to normal when there is no ship yaw input. Higher values are more responsive.
        /// </summary>
        [Range(0.01f, 1f)] public float dampingYawGravity = 0.25f;
        /// <summary>
        /// The damping maximum pitch Target Offset Up (y-axis)
        /// </summary>
        public float dampingMaxPitchOffsetUp = 2f;
        /// <summary>
        /// The damping maximum pitch Target Offset Down (y-axis)
        /// </summary>
        public float dampingMaxPitchOffsetDown = -2f;
        /// <summary>
        /// The damping maximum yaw Target Offset right (x-axis)
        /// </summary>
        public float dampingMaxYawOffsetRight = 2f;
        /// <summary>
        /// The damping maximum yaw Target Offset left (x-axis)
        /// </summary>
        public float dampingMaxYawOffsetLeft = -2f;

        /// <summary>
        /// If enabled, the camera will stay locked to the optimal camera rotation.
        /// </summary>
        public bool lockToTargetRotation = false;
        /// <summary>
        /// How quickly the camera turns towards the optimal camera rotation. Only relevant when lockToTargetRotation is disabled.
        /// </summary>
        [Range(1f, 100f)] public float turnSpeed = 15f;
        /// <summary>
        /// When cameraRotationMode is Aim At Target, enabling this will enable the camera to track the target
        /// without moving in the scene.
        /// </summary>
        public bool lockCameraPosition = false;
        /// <summary>
        /// How the camera rotation is determined.
        /// FollowVelocity: The camera rotates to face in the direction the ship is moving in.
        /// FollowTargetRotation: The camera rotates to face the direction the ship is facing in.
        /// AimAtTarget: The camera rotates to face towards the ship itself.
        /// </summary>
        public ShipCameraModule.CameraRotationMode cameraRotationMode = ShipCameraModule.CameraRotationMode.FollowTargetRotation;
        /// <summary>
        /// Below this velocity (in metres per second) the forwards direction of the target will be followed instead of the velocity.
        /// Only relevant when cameraRotationMode is set to FollowVelocity or TopDownFollowVelocity.
        /// </summary>
        public float followVelocityThreshold = 10f;
        /// <summary>
        /// If enabled, the camera will orient with respect to the world up direction rather than the target's up direction.
        /// </summary>
        public bool orientUpwards = false;
        /// <summary>
        /// The rotation of the camera. Only relevant when cameraRotationMode is set to Fixed.
        /// </summary>
        public Vector3 cameraFixedRotation = Vector3.zero;

        /// <summary>
        /// When the camera position/rotation is updated.
        /// FixedUpdate: The update occurs during FixedUpdate. Recommended for rigidbodies with Interpolation set to None.
        /// LateUpdate: The update occurs during LateUpdate. Recommended for rigidbodies with Interpolation set to Interpolate.
        /// Automatic: When the update occurs is automatically determined.
        /// </summary>
        public ShipCameraModule.CameraUpdateType updateType = ShipCameraModule.CameraUpdateType.Automatic;

        /// <summary>
        /// The maximum strength of the camera shake. Smaller numbers are better.
        /// This can be overridden by calling shipCameraModule.ShakeCamera(duration,strength)
        /// If modifying at runtime, you must call ReinitialiseTargetVariables().
        /// </summary>
        [Range(0f, 0.5f)] public float maxShakeStrength = 0f;

        /// <summary>
        /// The maximum duration (in seconds) the camera will shake per incident.
        /// This can be overridden by calling ShakeCamera(duration,strength).
        /// If modifying at runtime, you must call ReinitialiseTargetVariables().
        /// </summary>
        [Range(0.1f, 5f)] public float maxShakeDuration = 0.2f;

        #endregion

        #region Public Variables - Object Clipping

        /// <summary>
        /// Adjust the camera position to attempt to avoid the camera flying through objects between the ship and the camera.
        /// </summary>
        public bool clipObjects = false;

        /// <summary>
        /// The minimum speed the camera will move to avoid flying through objects between the ship and the camera.
        /// High values make clipping more effective. Lower values will make it smoother.
        /// Currently this has no effect if Lock to Target Position is enabled.
        /// </summary>
        [Range(1f, 100f)] public float minClipMoveSpeed = 10f;

        /// <summary>
        /// When clipObjects is true, the minimum distance the camera can be from the Ship (target) position.
        /// Typically this is the spheric radius of the ship. If the ship has colliders that do not overlay the
        /// target position, this value should be set, else set to 0 to improve performance.
        /// </summary>
        [Range(0f, 1000f)] public float clipMinDistance = 0f;

        /// <summary>
        /// The minimum offset on the x-axis, in metres, the camera can be from the Ship (target) when object clipping. This should be less than or equal to the Target Offset X value. 
        /// </summary>
        [Range(0f, 50f)] public float clipMinOffsetX = 0f;

        /// <summary>
        /// The minimum offset on the y-axis, in metres, the camera can be from the Ship (target) when object clipping. This should be less than or equal to the Target Offset Y value.
        /// </summary>
        [Range(0f, 50)] public float clipMinOffsetY = 0f;

        /// <summary>
        /// Clip objects in the selected Unity Layers.
        /// Start with Nothing (0) and call ResetClipObjectSettings()
        /// </summary>
        public LayerMask clipObjectMask = 0;

        #endregion

        #region Public Variables - Zoom

        /// <summary>
        /// In ShipCameraModule this is SerializeField private.
        /// </summary>
        public bool isZoomEnabled = false;

        /// <summary>
        /// The time, in seconds, to zoom fully in or out
        /// </summary>
        [Range(0.1f, 20f)] public float zoomDuration = 3f;

        /// <summary>
        /// The delay, in seconds, before zoom starts to return to the non-zoomed position
        /// </summary>
        [Range(0f, 3600f)] public float unzoomDelay = 0f;

        /// <summary>
        /// The camera field-of-view when no zoom is applied
        /// </summary>
        [Range(20f, 85f)] public float unzoomedFoV = 60f;

        /// <summary>
        /// The camera field-of-view when the camera is fully zoomed in.
        /// </summary>
        [Range(1f, 50f)] public float zoomedInFoV = 10f;

        /// <summary>
        /// The camera field-of-view when the camera is fully zoomed out.
        /// </summary>
        [Range(40f, 150f)] public float zoomedOutFoV = 90f;

        /// <summary>
        /// The amount of damping applied when starting or stopping camera zoom
        /// </summary>
        [Range(0f, 1f)] public float zoomDamping = 0.1f;

        #endregion
    }
}