using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class to transmit input data to the character controller
    /// </summary>
    public class CharacterInput
    {
        #region Public Variables and properties

        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        public float horizontalMove;
        public float verticalMove;
        public bool jump;
        public bool sprint;
        public bool crouch;
        /// <summary>
        /// When crouch input is detected, crouching will start or stop
        /// </summary>
        public bool crouchIsToggled;
        public bool jetpack;
        public bool switchLook;
        public float horizontalLook;
        public float verticalLook;
        public Quaternion xrLook;
        public float zoomLook;
        public float orbitLook;
        public bool leftFire1;
        public bool leftFire2;
        public bool rightFire1;
        public bool rightFire2;
        #endregion

        #region Class Constructor

        public CharacterInput ()
        {
            SetClassDefaults();
        }

        #endregion

        #region Public Member Methods

        public void SetClassDefaults()
        {
            horizontalMove = 0f;
            verticalMove = 0f;
            jump = false;
            sprint = false;
            crouch = false;
            crouchIsToggled = false;
            jetpack = false;
            switchLook = false;
            horizontalLook = 0f;
            verticalLook = 0f;
            xrLook = Quaternion.identity;
            zoomLook = 0f;
            orbitLook = 0f;
            leftFire1 = false;
            leftFire2 = false;
            rightFire1 = false;
            rightFire2 = false;
        }

        #endregion
    }
}