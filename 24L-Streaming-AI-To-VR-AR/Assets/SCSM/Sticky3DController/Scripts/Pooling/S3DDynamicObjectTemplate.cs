﻿using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class containing data for a dynamic object template. Used by StickyManager.
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class S3DDynamicObjectTemplate
    {
        public StickyDynamicModule s3dDynamicModulePrefab;
        public int instanceID;
        public int currentPoolSize;
        public List<GameObject> s3dDynamicObjectPool;

        // Class constructor
        public S3DDynamicObjectTemplate(StickyDynamicModule prefab, int id)
        {
            this.s3dDynamicModulePrefab = prefab;
            this.instanceID = id;
            this.currentPoolSize = 0;
            this.s3dDynamicObjectPool = null;
        }
    }
}