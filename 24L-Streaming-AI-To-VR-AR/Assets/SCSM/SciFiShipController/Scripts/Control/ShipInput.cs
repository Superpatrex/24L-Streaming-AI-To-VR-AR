using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class containing ship input parameters.
    /// </summary>
    public class ShipInput
    {
        #region Public Variables - Input Data
        /// <summary>
        /// X-axis translational input. Range -1 to +1. Positive values result in rightward input, negative values result in leftward input.
        /// </summary>
        public float horizontal;
        /// <summary>
        /// Y-axis translational input. Range -1 to +1. Positive values result in upward input, negative values result in downward input.
        /// </summary>
        public float vertical;
        /// <summary>
        /// Z-axis translational input. Range -1 to +1. Positive values result in forward input, negative values result in backward input.
        /// </summary>
        public float longitudinal;

        /// <summary>
        /// X-axis rotational input. Range -1 to +1. Positive values result in downwards pitching input, negative values result in upwards pitching input.
        /// </summary>
        public float pitch;
        /// <summary>
        /// Y-axis rotational input. Range -1 to +1. Positive values result in rightwards turning input, negative values result in leftwards turning input.
        /// </summary>
        public float yaw;
        /// <summary>
        /// Z-axis rotational input. Range -1 to +1. Positive values result in rightwards rolling input, negative values result in leftwards rolling input.
        /// </summary>
        public float roll;

        /// <summary>
        /// Primary fire input. Boolean value.
        /// </summary>
        public bool primaryFire;
        /// <summary>
        /// Secondary fire input. Boolean value.
        /// </summary>
        public bool secondaryFire;
        
        /// <summary>
        /// Dock action input. Boolean value. Works when an initialised ShipDocking script is attached to an enabled ShipControlModule.
        /// This will trigger a docking action which will depend on which DockingState the ship is already in.
        /// </summary>
        public bool dock;

        #endregion

        #region Public Variables - Input Enablement (Advanced)
        // Generally you'll leave these all enabled. When you want
        // to update some axis in code while allowing the PlayerInputModule
        // to control everything else, you can just enable the axis
        // that you want to update and call shipControlModule.SendInput(shipInput).

        /// <summary>
        /// Should we use or discard data in the horizontal field? 
        /// </summary>
        public bool isHorizontalDataEnabled;
        /// <summary>
        /// Should we use or discard data in the vertical field?
        /// </summary>
        public bool isVerticalDataEnabled;
        /// <summary>
        /// Should we use or discard data in the longitudinal field?
        /// </summary>
        public bool isLongitudinalDataEnabled;
        /// <summary>
        /// Should we use or discard data in the pitch field?
        /// </summary>
        public bool isPitchDataEnabled;
        /// <summary>
        /// Should we use or discard data in the yaw field?
        /// </summary>
        public bool isYawDataEnabled;
        /// <summary>
        /// Should we use or discard data in the roll field?
        /// </summary>
        public bool isRollDataEnabled;
        /// <summary>
        /// Should we use or discard data in the primaryFire field?
        /// </summary>
        public bool isPrimaryFireDataEnabled;
        /// <summary>
        /// Should we use or discard data in the secondaryFire field?
        /// </summary>
        public bool isSecondaryFireDataEnabled;
        /// <summary>
        /// Should we use or discard data in the dock field?
        /// </summary>
        public bool isDockDataEnabled;

        #endregion

        #region Class Constructor
        public ShipInput()
        {
            // Input data fields
            horizontal = 0f;
            vertical = 0f;
            longitudinal = 0f;
            pitch = 0f;
            yaw = 0f;
            roll = 0f;
            primaryFire = false;
            secondaryFire = false;
            dock = false;
            // Update Enablement fields
            EnableAllData();
        }
        #endregion

        #region Public API Methods

        /// <summary>
        /// A quick way to set all is[axisname]DataEnabled fields to true
        /// </summary>
        public void EnableAllData()
        {
            isHorizontalDataEnabled = true;
            isVerticalDataEnabled = true;
            isLongitudinalDataEnabled = true;
            isPitchDataEnabled = true;
            isYawDataEnabled = true;
            isRollDataEnabled = true;
            isPrimaryFireDataEnabled = true;
            isSecondaryFireDataEnabled = true;
            isDockDataEnabled = true;
        }

        /// <summary>
        /// A quick way to set all is[axisname]DataEnabled fields to false
        /// </summary>
        public void DisableAllData()
        {
            isHorizontalDataEnabled = false;
            isVerticalDataEnabled = false;
            isLongitudinalDataEnabled = false;
            isPitchDataEnabled = false;
            isYawDataEnabled = false;
            isRollDataEnabled = false;
            isPrimaryFireDataEnabled = false;
            isSecondaryFireDataEnabled = false;
            isDockDataEnabled = false;
        }

        #endregion
    }
}
