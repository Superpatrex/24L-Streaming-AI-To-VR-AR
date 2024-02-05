using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class containing data for a decal template. Used by StickyManager.
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class S3DDecalTemplate
    {
        public StickyDecalModule s3dDecalModulePrefab;
        public int instanceID;
        public int currentPoolSize;
        public List<GameObject> s3dDecalPool;

        // Class constructor
        public S3DDecalTemplate(StickyDecalModule prefab, int id)
        {
            this.s3dDecalModulePrefab = prefab;
            this.instanceID = id;
            this.currentPoolSize = 0;
            this.s3dDecalPool = null;
        }
    }
}