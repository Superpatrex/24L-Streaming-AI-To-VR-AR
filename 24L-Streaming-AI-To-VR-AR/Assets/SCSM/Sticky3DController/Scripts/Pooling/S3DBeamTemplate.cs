using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The template used by the pooling system to describe a unique type of StickyBeamModule.
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class S3DBeamTemplate
    {
        public StickyBeamModule s3dBeamModulePrefab;
        public int instanceID;
        public int currentPoolSize;
        public List<GameObject> s3dBeamPool;

        // Class constructor
        public S3DBeamTemplate (StickyBeamModule prefab, int id)
        {
            this.s3dBeamModulePrefab = prefab;
            this.instanceID = id;
            this.currentPoolSize = 0;
            this.s3dBeamPool = null;
        }
    }
}