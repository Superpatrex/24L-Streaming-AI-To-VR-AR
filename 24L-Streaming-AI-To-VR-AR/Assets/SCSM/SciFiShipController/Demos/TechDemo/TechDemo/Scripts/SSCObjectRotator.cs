using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This component is used to rotate an object around the y and x axis using
    /// two pivot points. Typically the x axis mesh will be a a child of the y-axis
    /// mesh so that that both meshes can rotate around the y-axis.
    /// </summary>
    public class SSCObjectRotator : MonoBehaviour
    {
        #region Enumerations
        public enum UpdateType
        {
            Update = 0,
            FixedUpdate = 1,
            LateUpdate = 2
        }
        #endregion

        #region Public variables
        public bool initialiseOnAwake = true;
        public float startDelay = 0f;
        public UpdateType updateType = UpdateType.FixedUpdate;
        public Transform pivotY = null;
        public Transform pivotX = null;
        public float startYRotation = 0f;
        public float endYRotation = 180f;
        public float rotationYSpeed = 5f;
        public float startXRotation = 0f;
        public float endXRotation = 45f;
        public float rotationXSpeed = 2f;

        #endregion

        #region Private variables

        private Vector3 currentRot;
        private float currentYRot;
        private float currentXRot;
        private float yDirection = 1f;
        private float xDirection = 1f;
        private int updateTypeInt;
        private int updateTypeUpdateInt = (int)UpdateType.Update;
        private int updateTypeFixedInt = (int)UpdateType.FixedUpdate;
        private int updateTypeLateInt = (int)UpdateType.LateUpdate;
        private bool isInitialised = false;
        private bool isRotatingY = false;
        private bool isRotatingX = false;
        private float startCountdownTimer = 0f;

        #endregion

        #region Private Initialise methods
        // Start is called before the first frame update
        void Awake()
        {
            if (initialiseOnAwake) { Initialise(); }
        }
        #endregion

        #region Update Methods

        // Update is called once per frame
        private void Update()
        {
            if (isInitialised && updateTypeInt == updateTypeUpdateInt)
            {
                RotateObject();
            }
        }

        private void FixedUpdate()
        {
            if (isInitialised && updateTypeInt == updateTypeFixedInt)
            {
                RotateObject();
            }
        }

        private void LateUpdate()
        {
            if (isInitialised && updateTypeInt == updateTypeLateInt)
            {
                RotateObject();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This should only be called if the script is initialised (has valid pivot gameobjects)
        /// </summary>
        private void RotateObject()
        {
            // If there is a start delay, decrement the timer.
            if (startCountdownTimer > 0f)
            {
                startCountdownTimer -= Time.deltaTime;

                if (startCountdownTimer <= 0f)
                {
                    startCountdownTimer = 0f;
                    isRotatingX = true;
                    isRotatingY = true;
                }
            }

            if (isRotatingY)
            {
                // Rotate at x degrees per second around Y axis
                pivotY.localRotation = Quaternion.RotateTowards(pivotY.localRotation,
                    Quaternion.Euler(0f, endYRotation, 0f), rotationYSpeed * Time.deltaTime);

                float fixedEulerAngleY = pivotY.localRotation.eulerAngles.y > 180f ? pivotY.localRotation.eulerAngles.y - 360f : pivotY.localRotation.eulerAngles.y;

                // Clamping - clockwise rotation
                if (yDirection == 1f && fixedEulerAngleY >= endYRotation)
                {
                    currentRot.Set(pivotY.localEulerAngles.x, endYRotation, pivotY.localEulerAngles.z);
                    pivotY.localRotation = Quaternion.Euler(currentRot);
                    isRotatingY = false;
                }
                // Clamping anti-clockwise rotation
                else if (yDirection == -1f && fixedEulerAngleY <= endYRotation)
                {
                    currentRot.Set(pivotY.localEulerAngles.x, endYRotation, pivotY.localEulerAngles.z);
                    pivotY.localRotation = Quaternion.Euler(currentRot);
                    isRotatingY = false;
                }
            }

            if (isRotatingX)
            {
                // Rotate at x degrees per second around Y axis
                pivotX.localRotation = Quaternion.RotateTowards(pivotX.localRotation,
                    Quaternion.Euler(endXRotation,0f, 0f), rotationXSpeed * Time.deltaTime);

                float fixedEulerAngleX = pivotX.localRotation.eulerAngles.x > 180f ? pivotX.localRotation.eulerAngles.x - 360f : pivotX.localRotation.eulerAngles.x;

                // Clamping - clockwise rotation
                if (xDirection == 1f && fixedEulerAngleX >= endXRotation)
                {
                    currentRot.Set(endXRotation, pivotX.localEulerAngles.y, pivotX.localEulerAngles.z);
                    pivotX.localRotation = Quaternion.Euler(currentRot);
                    isRotatingX = false;
                }
                // Clamping anti-clockwise rotation
                else if (xDirection == -1f && fixedEulerAngleX <= endXRotation)
                {
                    currentRot.Set(endXRotation, pivotX.localEulerAngles.y, pivotX.localEulerAngles.z);
                    pivotX.localRotation = Quaternion.Euler(currentRot);
                    isRotatingX = false;
                }
            }
        }

        #endregion

        #region Public API Methods

        public void Initialise()
        {
            #if UNITY_EDITOR
            if (pivotY == null) { Debug.LogWarning("SSCObjectRotator Y-axis (horizontal) pivot transform is not defined"); }
            if (pivotX == null) { Debug.LogWarning("SSCObjectRotator X-axis (vertical) pivot transform is not defined"); }
            #endif

            // Avoid having to constantly look up the enumeration
            updateTypeInt = (int)updateType;

            yDirection = startYRotation < endYRotation ? 1f : -1f;
            xDirection = startXRotation < endXRotation ? 1f : -1f;

            currentRot = Vector3.zero;

            if (pivotY != null)
            {
                currentRot.Set(pivotY.localEulerAngles.x, startYRotation, pivotY.localEulerAngles.z);
                pivotY.localRotation = Quaternion.Euler(currentRot);
            }

            if (pivotX != null)
            {
                // In Unity down on X-axis is +ve.
                currentRot.Set(startXRotation, pivotX.localEulerAngles.y, pivotX.localEulerAngles.z);
                pivotX.localRotation = Quaternion.Euler(currentRot);
            }

            isInitialised = pivotY != null && pivotX != null;

            if (isInitialised)
            {
                if (startDelay > 0f)
                {
                    startCountdownTimer = startDelay;
                }
                else
                {
                    isRotatingX = true;
                    isRotatingY = true;
                }
            }
        }

        /// <summary>
        /// Change the updateType at runtime
        /// </summary>
        /// <param name="newUpdateType"></param>
        public void ChangeUpdateType(UpdateType newUpdateType)
        {
            updateType = newUpdateType;
            updateTypeInt = (int)updateType;
        }

        #endregion
    }
}
