using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The template used by the pooling system to describe a unique type of StickyGenericModule.
    /// </summary>
    public class S3DGenericObjectTemplate
    {
        public StickyGenericModule s3dGenericModulePrefab;
        public int instanceID;
        public int currentPoolSize;
        public List<GameObject> s3dGenericObjectPool;

        // Class constructor
        public S3DGenericObjectTemplate(StickyGenericModule prefab, int id)
        {
            this.s3dGenericModulePrefab = prefab;
            this.instanceID = id;
            this.currentPoolSize = 0;
            this.s3dGenericObjectPool = null;
        }
    }
}