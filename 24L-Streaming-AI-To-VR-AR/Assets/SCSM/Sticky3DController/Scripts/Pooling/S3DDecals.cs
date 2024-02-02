using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A scriptable object that stores a list of StickyDecalModules.
    /// </summary>
    [CreateAssetMenu(fileName = "Sticky3D Decals", menuName = "Sticky3D/Decals")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class S3DDecals : ScriptableObject
    {
        #region Public Variables

        public List<StickyDecalModule> decalModuleList;

        #endregion

        #region Public Properties

        /// <summary>
        /// Get the number of StickyDecalModule slots in the list. NOTE: Some may be null
        /// </summary>
        public int NumberOfDecals { get { return decalModuleList == null ? 0 : decalModuleList.Count; } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the ID of the StickyDecalModule given the zero-based
        /// index position in the list. If no valid match is found, returns 0
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetDecalID (int index)
        {
            int numDecals = decalModuleList == null ? 0 : decalModuleList.Count;

            if (index < 0 || index >= numDecals) { return 0; }
            else { return decalModuleList[index].GetInstanceID(); }
        }

        #endregion
    }
}