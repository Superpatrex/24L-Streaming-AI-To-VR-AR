using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class to transmit input data to a weapon
    /// </summary>
    public class WeaponInput
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        public bool fire1;
        public bool fire2;

        #endregion

        #region Class Constructor

        public WeaponInput()
        {
            SetClassDefaults();
        }

        #endregion

        #region Public Member Methods

        public void SetClassDefaults()
        {
            fire1 = false;
            fire2 = false;
        }

        #endregion
    }
}