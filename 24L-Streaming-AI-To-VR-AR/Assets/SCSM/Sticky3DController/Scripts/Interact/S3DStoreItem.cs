using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// An item that can be selected or stored with a character.
    /// </summary>
    [System.Serializable]
    public class S3DStoreItem
    {
        #region Enumerations

        #endregion

        #region Public Static

        /// <summary>
        /// Denotes the S3DStoreItem is invalid or not available
        /// </summary>
        public static int NoStoreItem = 0;
        #endregion

        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// A reference to a interactive-enabled object in the scene
        /// </summary>
        public StickyInteractive stickyInteractive;

        /// <summary>
        /// Keep a quick lookup to the StickyInteractiveID in case the StickyInteractive
        /// component become null.
        /// </summary>
        public int stickyInteractiveID;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Hashed GUID code to uniquely identify an S3DStoreItem instance.
        /// </summary>
        public int guidHash;

        #endregion

        #region Public Properties

        /// <summary>
        /// Is there a StickyWeapon associated with this storeitem?
        /// </summary>
        public bool IsStickyWeapon { get { return stickyInteractive != null && stickyInteractive.IsStickyWeapon; } }

        /// <summary>
        /// [READONLY]
        /// Unique identifier for this item
        /// </summary>
        public int StoreItemID { get { return guidHash; } }

        #endregion

        #region Internal Variables

        #endregion

        #region Constructors
        public S3DStoreItem()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public S3DStoreItem(S3DStoreItem s3dStoreItem)
        {
            if (s3dStoreItem == null) { SetClassDefaults(); }
            else
            {
                guidHash = s3dStoreItem.guidHash;
                stickyInteractive = s3dStoreItem.stickyInteractive;
                stickyInteractiveID = s3dStoreItem.stickyInteractiveID;
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// S3DStoreItem comparison
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) { return false; }
            else
            {
                return guidHash == ((S3DStoreItem)obj).guidHash;
            }
        }

        public override int GetHashCode()
        {
            return guidHash;
        }

        #endregion

        #region public Member Methods
        public void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            stickyInteractive = null;
            stickyInteractiveID = StickyManager.NoPrefabID;
        }

        #endregion
    }
}