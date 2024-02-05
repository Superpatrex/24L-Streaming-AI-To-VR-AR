using UnityEngine;

// Copyright (c) 2015-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class to transmit XR input data to the character controller
    /// </summary>
    public class CharacterInputXR
    {
        #region Public Variables and properties

        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        public Vector3 hmdPosition;
        public float leftHandGrip;
        public Vector3 leftHandPosition;
        public Quaternion leftHandRotation;
        public float leftHandTrigger;
        public float rightHandGrip;
        public Vector3 rightHandPosition;
        public Quaternion rightHandRotation;
        public float rightHandTrigger;

        #endregion

        #region Class Constructor

        public CharacterInputXR()
        {
            SetClassDefaults();
        }

        #endregion

        #region Public Member Methods

        public void SetClassDefaults()
        {
            hmdPosition = Vector3.zero;
            leftHandGrip = 0f;
            leftHandPosition = Vector3.zero;
            leftHandRotation = Quaternion.identity;
            leftHandTrigger = 0f;
            rightHandGrip = 0f;
            rightHandPosition = Vector3.zero;
            rightHandRotation = Quaternion.identity;
            rightHandTrigger = 0f;
        }

        #endregion
    }
}