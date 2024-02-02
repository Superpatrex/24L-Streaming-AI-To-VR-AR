using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class containing data for an effects object template. Used by StickyManager.
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class S3DEffectsObjectTemplate
    {
        public StickyEffectsModule s3dEffectsModulePrefab;
        public int instanceID;
        public int currentPoolSize;
        public List<GameObject> s3dEffectsObjectPool;

        // Class constructor
        public S3DEffectsObjectTemplate(StickyEffectsModule prefab, int id)
        {
            this.s3dEffectsModulePrefab = prefab;
            this.instanceID = id;
            this.currentPoolSize = 0;
            this.s3dEffectsObjectPool = null;
        }
    }
}