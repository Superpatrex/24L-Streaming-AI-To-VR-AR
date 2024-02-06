using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This sample script shows how you can use a custom player input action to change the camera view for a ship.
    /// Setup:
    /// 1. Add this script to your camera (it must have a Ship Camera Module attached)
    /// 2. On your Player Input Module, add a new custom player input
    /// 3. Configure the button to the button you want to press to cycle the camera views
    /// 4. In the gameobject slot, select the object this script is attached to (the camera)
    /// 5. Choose the "SampleChangeCameraView.CycleCameraView" option from the function dropdown (where it initially says "No Function")
    /// 6. Configure the settings of the three camera views as required
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Change Camera View")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    [RequireComponent(typeof(ShipCameraModule))]
    public class SampleChangeCameraView : MonoBehaviour
    {
        #region Public Variables

        #region Camera View 1 Variables

        [Header("Camera View 1")]
        /// <summary>
        /// The target offset coordinates of camera view 1.
        /// </summary>
        public ShipCameraModule.TargetOffsetCoordinates targetOffsetCoords1 = ShipCameraModule.TargetOffsetCoordinates.TargetRotation;
        /// <summary>
        /// The target offset of camera view 1.
        /// </summary>
        public Vector3 targetOffset1 = new Vector3(0f, 0.75f, 4.25f);
        /// <summary>
        /// Whether camera view 1 is locked to the target position.
        /// </summary>
        public bool lockToTargetPos1 = true;
        /// <summary>
        /// The move speed of camera view 1.
        /// </summary>
        public float moveSpeed1 = 15f;
        /// <summary>
        /// Whether camera view 1 is locked to the target rotation.
        /// </summary>
        public bool lockToTargetRot1 = true;
        /// <summary>
        /// The turn speed of camera view 1.
        /// </summary>
        public float turnSpeed1 = 15f;
        /// <summary>
        /// The camera rotation mode of camera view 1.
        /// </summary>
        public ShipCameraModule.CameraRotationMode cameraRotationMode1 = ShipCameraModule.CameraRotationMode.FollowTargetRotation;
        /// <summary>
        /// The follow velocity threshold of camera view 1.
        /// </summary>
        public float followVelocityThreshold1 = 10f;
        /// <summary>
        /// Whether camera view 1 is oriented upwards.
        /// </summary>
        public bool orientUpwards1 = false;

        #endregion

        #region Camera View 2 Variables

        [Header("Camera View 2")]
        /// <summary>
        /// The target offset coordinates of camera view 2.
        /// </summary>
        public ShipCameraModule.TargetOffsetCoordinates targetOffsetCoords2 = ShipCameraModule.TargetOffsetCoordinates.TargetRotation;
        /// <summary>
        /// The target offset of camera view 2.
        /// </summary>
        public Vector3 targetOffset2 = new Vector3(0f, 2f, -10f);
        /// <summary>
        /// Whether camera view 2 is locked to the target position.
        /// </summary>
        public bool lockToTargetPos2 = false;
        /// <summary>
        /// The move speed of camera view 2.
        /// </summary>
        public float moveSpeed2 = 15f;
        /// <summary>
        /// Whether camera view 2 is locked to the target rotation.
        /// </summary>
        public bool lockToTargetRot2 = false;
        /// <summary>
        /// The turn speed of camera view 2.
        /// </summary>
        public float turnSpeed2 = 15f;
        /// <summary>
        /// The camera rotation mode of camera view 2.
        /// </summary>
        public ShipCameraModule.CameraRotationMode cameraRotationMode2 = ShipCameraModule.CameraRotationMode.FollowVelocity;
        /// <summary>
        /// The follow velocity threshold of camera view 2.
        /// </summary>
        public float followVelocityThreshold2 = 10f;
        /// <summary>
        /// Whether camera view 2 is oriented upwards.
        /// </summary>
        public bool orientUpwards2 = false;

        #endregion

        #region Camera View 3 Variables

        [Header("Camera View 3")]
        /// <summary>
        /// The target offset coordinates of camera view 3.
        /// </summary>
        public ShipCameraModule.TargetOffsetCoordinates targetOffsetCoords3 = ShipCameraModule.TargetOffsetCoordinates.TargetRotation;
        /// <summary>
        /// The target offset of camera view 3.
        /// </summary>
        public Vector3 targetOffset3 = new Vector3(0f, 3f, -20f);
        /// <summary>
        /// Whether camera view 3 is locked to the target position.
        /// </summary>
        public bool lockToTargetPos3 = false;
        /// <summary>
        /// The move speed of camera view 3.
        /// </summary>
        public float moveSpeed3 = 15f;
        /// <summary>
        /// Whether camera view 3 is locked to the target rotation.
        /// </summary>
        public bool lockToTargetRot3 = false;
        /// <summary>
        /// The turn speed of camera view 3.
        /// </summary>
        public float turnSpeed3 = 15f;
        /// <summary>
        /// The camera rotation mode of camera view 3.
        /// </summary>
        public ShipCameraModule.CameraRotationMode cameraRotationMode3 = ShipCameraModule.CameraRotationMode.FollowVelocity;
        /// <summary>
        /// The follow velocity threshold of camera view 3.
        /// </summary>
        public float followVelocityThreshold3 = 10f;
        /// <summary>
        /// Whether camera view 3 is oriented upwards.
        /// </summary>
        public bool orientUpwards3 = false;

        #endregion

        #endregion

        #region Private Variables

        private ShipCameraModule shipCameraModule;
        private int currentCameraViewIndex = 0;

        #endregion

        #region Initialisation

        // Awake is called before the first frame update
        void Awake()
        {
            // Get a reference to the ship camera module
            shipCameraModule = GetComponent<ShipCameraModule>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Cycles the camera view.
        /// </summary>
        public void CycleCameraView (Vector3 inputValue, int customPlayerInputEventType)
        {
            // Cycle the current camera view index to the next view.
            currentCameraViewIndex = (currentCameraViewIndex + 1) % 3;

            SetCurrentView(currentCameraViewIndex);
        }

        /// <summary>
        /// Gets the current camera view index.
        /// 0 - camera view 1.
        /// 1 - camera view 2.
        /// 2 - camera view 3.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentCameraViewIndex ()
        {
            return currentCameraViewIndex;
        }

        /// <summary>
        /// Gets the current camera view index.
        /// 0 - camera view 1.
        /// 1 - camera view 2.
        /// 2 - camera view 3.
        /// </summary>
        /// <returns></returns>
        public int GetNextCameraViewIndex ()
        {
            return (currentCameraViewIndex + 1) % 3;
        }

        /// <summary>
        /// Set the current view using the zero-based index
        /// 0 - camera view 1.
        /// 1 - camera view 2.
        /// 2 - camera view 3.
        /// </summary>
        /// <param name="viewIndex"></param>
        public void SetCurrentView(int viewIndex)
        {
            if (viewIndex >= 0 && viewIndex < 3 && shipCameraModule != null)
            {
                currentCameraViewIndex = viewIndex;

                if (currentCameraViewIndex == 0)
                {
                    // Camera view 1
                    // Set the camera parameters to match camera view 1
                    shipCameraModule.targetOffsetCoordinates = targetOffsetCoords1;
                    shipCameraModule.targetOffset = targetOffset1;
                    shipCameraModule.lockToTargetPosition = lockToTargetPos1;
                    shipCameraModule.moveSpeed = moveSpeed1;
                    shipCameraModule.lockToTargetRotation = lockToTargetRot1;
                    shipCameraModule.turnSpeed = turnSpeed1;
                    shipCameraModule.cameraRotationMode = cameraRotationMode1;
                    shipCameraModule.followVelocityThreshold = followVelocityThreshold1;
                    shipCameraModule.orientUpwards = orientUpwards1;
                }
                else if (currentCameraViewIndex == 1)
                {
                    // Camera view 2
                    // Set the camera parameters to match camera view 2
                    shipCameraModule.targetOffsetCoordinates = targetOffsetCoords2;
                    shipCameraModule.targetOffset = targetOffset2;
                    shipCameraModule.lockToTargetPosition = lockToTargetPos2;
                    shipCameraModule.moveSpeed = moveSpeed2;
                    shipCameraModule.lockToTargetRotation = lockToTargetRot2;
                    shipCameraModule.turnSpeed = turnSpeed2;
                    shipCameraModule.cameraRotationMode = cameraRotationMode2;
                    shipCameraModule.followVelocityThreshold = followVelocityThreshold2;
                    shipCameraModule.orientUpwards = orientUpwards2;
                }
                else
                {
                    // Camera view 3
                    // Set the camera parameters to match camera view 3
                    shipCameraModule.targetOffsetCoordinates = targetOffsetCoords3;
                    shipCameraModule.targetOffset = targetOffset3;
                    shipCameraModule.lockToTargetPosition = lockToTargetPos3;
                    shipCameraModule.moveSpeed = moveSpeed3;
                    shipCameraModule.lockToTargetRotation = lockToTargetRot3;
                    shipCameraModule.turnSpeed = turnSpeed3;
                    shipCameraModule.cameraRotationMode = cameraRotationMode3;
                    shipCameraModule.followVelocityThreshold = followVelocityThreshold3;
                    shipCameraModule.orientUpwards = orientUpwards3;
                }
            }
        }

        #endregion
    }
}
