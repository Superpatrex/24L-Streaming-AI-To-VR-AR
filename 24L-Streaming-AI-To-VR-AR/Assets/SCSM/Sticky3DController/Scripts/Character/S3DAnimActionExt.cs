using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// An extended version of S3DAnimAction currently used with
    /// S3DWeaponAnimSets. It's purpose is to store a parameter name,
    /// which is converted to a hashcode for use with an S3DAnimAction
    /// that gets applied to the character holding a weapon.
    /// </summary>
    [System.Serializable]
    public class S3DAnimActionExt : S3DAnimAction
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)
        
        /// <summary>
        /// The name of the parameter in the animation controller.
        /// This will be manually entered by the user.
        /// </summary>
        public string parameterName;

        #endregion

        #region Constructors
        public S3DAnimActionExt()
        {
            SetClassDefaults();
        }

        #endregion

        #region Public API Methods - General

        public override void SetClassDefaults()
        {
            base.SetClassDefaults();

            parameterName = string.Empty;
        }

        #endregion
    }
}