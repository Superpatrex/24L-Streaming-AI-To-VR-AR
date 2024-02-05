using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The template used by the pooling system to describe a unique type of StickyProjectileModule.
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class S3DProjectileTemplate
    {
        public StickyProjectileModule s3dProjectileModulePrefab;
        public int instanceID;
        public int currentPoolSize;
        public List<GameObject> s3dProjectilePool;
        public Quaternion modelRotation;
        public int numberOfDecals;
        public int[] decalPrefabIDs;

        // Class constructor
        public S3DProjectileTemplate(StickyProjectileModule prefab, int id)
        {
            this.s3dProjectileModulePrefab = prefab;
            this.instanceID = id;
            this.currentPoolSize = 0;
            this.s3dProjectilePool = null;
            // Cache the model rotation offset
            this.modelRotation = Quaternion.Euler(prefab.modelRotationOffset);
            this.numberOfDecals = 0;
        }
    }
}