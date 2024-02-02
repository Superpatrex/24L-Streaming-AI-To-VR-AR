using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A scriptable object that stores a list of StickyEffectsModules.
    /// </summary>
    [CreateAssetMenu(fileName = "Sticky3D Effects Set", menuName = "Sticky3D/Effects Set")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class S3DEffectsSet : ScriptableObject
    {
        #region Public Variables

        public List<StickyEffectsModule> effectsModuleList;

        #endregion

        #region Public Properties

        /// <summary>
        /// Get the number of StickyEffectsModule slots in the list. NOTE: Some may be null
        /// </summary>
        public int NumberOfEffects { get { return effectsModuleList == null ? 0 : effectsModuleList.Count; } }

        #endregion
    }
}