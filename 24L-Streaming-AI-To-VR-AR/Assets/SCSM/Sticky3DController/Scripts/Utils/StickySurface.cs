using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This component is placed on an object so that Sticky3D can identify
    /// what type of surface it is. Place on all surfaces mesh colliders that
    /// you want your character to use a non-default footstep action or to
    /// for non-default damage decals when hit by a projectile.
    /// </summary>
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickySurface : MonoBehaviour
    {
        #region Public Variables
        /// <summary>
        /// Reference to a list of common surface types
        /// </summary>
        public S3DSurfaces s3dSurfaces = null;
        /// <summary>
        /// The guidHash for the Surface Type from the S3DSurfaces scriptable object
        /// </summary>
        public int guidHashSurfaceType = 0;
        /// <summary>
        /// Is this object a Unity terrain rather than a regular mesh?
        /// </summary>
        public bool isTerrain = false;
        #endregion

        #region Public Properties

        public bool HasDamageDecals { get { return GetNumberOfDamageDecals() > 0; } }

        #endregion

        #region Private Variables
        [System.NonSerialized] private Terrain terrain = null;
        [System.NonSerialized] private TerrainData tData = null;
        [System.NonSerialized] private Vector3 worldPosition;
        [System.NonSerialized] private TerrainLayer[] terrainLayers = null;
        [System.NonSerialized] private int numTerrainLayers = 0;
        [System.NonSerialized] private List<int> terrainTextureNameHashList = null;
        [System.NonSerialized] private StickyManager stickyManager = null;

        #endregion

        #region Public Decal Methods

        /// <summary>
        /// Get a random decal prefab from the list of damage decals (if any).
        /// Returns true if one is populated in the out parameter, else will return false.
        /// </summary>
        /// <returns></returns>
        public bool GetDamageDecalPrefab (out StickyDecalModule stickyDecalModule)
        {
            bool isSuccess = false;
            int numDecals = GetNumberOfDamageDecals();
            stickyDecalModule = null;

            if (numDecals > 0)
            {
                S3DSurfaceType surfaceType = s3dSurfaces.GetSurfaceType(guidHashSurfaceType);

                if (numDecals == 1)
                {
                    stickyDecalModule = surfaceType.damageDecals.decalModuleList[0];
                    isSuccess = stickyDecalModule != null;
                }
                else
                {
                    if (stickyManager == null) { stickyManager = StickyManager.GetOrCreateManager(gameObject.scene.handle); }

                    int decalIndex = stickyManager.GetRandomPrefabIndex(numDecals);

                    if (decalIndex >= 0)
                    {
                        stickyDecalModule = surfaceType.damageDecals.decalModuleList[decalIndex];
                        isSuccess = stickyDecalModule != null;
                    }
                }
            }

            return isSuccess;
        }

        /// <summary>
        /// Get the number of damage decals for this sticky surface
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfDamageDecals()
        {        
            if (guidHashSurfaceType != 0 && s3dSurfaces != null)
            {
                S3DSurfaceType surfaceType = s3dSurfaces.GetSurfaceType(guidHashSurfaceType);

                if (surfaceType == null) { return 0; }
                else
                {
                    return surfaceType.damageDecals == null ? 0 : surfaceType.damageDecals.NumberOfDecals;
                }
            }
            else { return 0; }
        }

        #endregion

        #region Public Terrain Methods

        /// <summary>
        /// Cache the Terrain object and fetch textures.
        /// WARNING: This may incur GC so will only update the details the first time it runs.
        /// </summary>
        public void GetTerrainDetails()
        {
            // Cache terrain on first-time use.
            // Avoid Start() to we don't need to call that for all mesh objects
            // with a StickySurface component.
            if (isTerrain && terrain == null)
            {
                terrain = GetComponent<Terrain>();
                if (terrain != null)
                {
                    tData = terrain.terrainData;
                    // Cached position of the terrain
                    worldPosition = transform.position;

                    // Retrrieve the terrain layers that hold the Alphamap Textures
                    terrainLayers = tData.terrainLayers;
                    numTerrainLayers = terrainLayers == null ? 0 : terrainLayers.Length;

                    terrainTextureNameHashList = new List<int>(numTerrainLayers < 1 ? 1 : numTerrainLayers);

                    for (int txIdx = 0; txIdx < numTerrainLayers; txIdx++)
                    {
                        TerrainLayer terrainLayer = terrainLayers[txIdx];
                        if (terrainLayer != null)
                        {
                            if (terrainLayer.diffuseTexture != null)
                            {
                                // Convert the texture name into a hash code
                                terrainTextureNameHashList.Add(S3DMath.GetHashCode(terrainLayer.diffuseTexture.name));
                            }
                            else
                            {
                                terrainTextureNameHashList.Add(0);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the ID (hash) of the dominate texture's name at the world space position.
        /// It assumes the wsPoint is within the bounds of the terrain.
        /// If no dominate texture is found, returns a guidHash of 0.
        /// </summary>
        /// <param name="wsPoint"></param>
        /// <param name="minWeight"></param>
        /// <returns></returns>
        public int GetTextureAtPosition(Vector3 wsPoint, float minWeight)
        {
            int texNameHash = 0;

            GetTerrainDetails();

            if (tData != null)
            {
                // Convert point into a normalised position on the terrain
                float xPosN = Mathf.InverseLerp(worldPosition.x, worldPosition.x + tData.size.x, wsPoint.x);
                float zPosN = Mathf.InverseLerp(worldPosition.z, worldPosition.z + tData.size.z, wsPoint.z);

                // Retrieve alphamap metadata
                int alphamapWidth = tData.alphamapWidth;
                int alphamapHeight = tData.alphamapHeight;
                int numberAlphaMapLayers = tData.alphamapLayers;

                if (numberAlphaMapLayers > 0 && numTerrainLayers > 0)
                {
                    // Get position of point in alphamap
                    int alphaX = Mathf.FloorToInt(xPosN * (alphamapWidth - 1f));
                    int alphaZ = Mathf.FloorToInt(zPosN * (alphamapHeight - 1f));

                    // Get the alphamap layers for this point in the terrain
                    // WARNING - this could cause GC - needs checking
                    float[,,] alphamap = tData.GetAlphamaps(alphaX, alphaZ, 1, 1);

                    if (alphamap != null)
                    {
                        float maxWeight = 0f;
                        int maxLayer = -1;

                        for (int l = 0; l < numberAlphaMapLayers; l++)
                        {
                            // The array is very small so this should be fast
                            if (alphamap[0, 0, l] > maxWeight) { maxLayer = l; maxWeight = alphamap[0, 0, l]; }
                        }

                        if (maxLayer >= 0 && maxLayer < numTerrainLayers && maxWeight >= minWeight)
                        {
                            // Set the ID or hash of the dominate texture's name
                            texNameHash = terrainTextureNameHashList[maxLayer];
                        }
                    }
                }
            }

            return texNameHash;
        }

        /// <summary>
        /// Does the world-space position contain any of the textures with their indicated minimum weights?
        /// It assumes the wsPoint is within the bounds of the terrain.
        /// </summary>
        /// <param name="wsPoint"></param>
        /// <param name="terrainTextureIDs"></param>
        /// <param name="terrainTextureMinWeights"></param>
        /// <returns></returns>
        public bool IsTextureAtPosition(Vector3 wsPoint, int[] terrainTextureIDs, float[] terrainTextureMinWeights)
        {
            bool isTextureMatch = false;

            GetTerrainDetails();

            if (tData != null)
            {
                // Convert point into a normalised position on the terrain
                float xPosN = Mathf.InverseLerp(worldPosition.x, worldPosition.x + tData.size.x, wsPoint.x);
                float zPosN = Mathf.InverseLerp(worldPosition.z, worldPosition.z + tData.size.z, wsPoint.z);

                // Retrieve alphamap metadata
                int alphamapWidth = tData.alphamapWidth;
                int alphamapHeight = tData.alphamapHeight;
                int numberAlphaMapLayers = tData.alphamapLayers;

                if (numberAlphaMapLayers > 0 && numTerrainLayers > 0)
                {
                    // Get position of point in alphamap
                    int alphaX = Mathf.FloorToInt(xPosN * (alphamapWidth - 1f));
                    int alphaZ = Mathf.FloorToInt(zPosN * (alphamapHeight - 1f));

                    // Get the alphamap layers for this point in the terrain
                    // WARNING - this could cause GC - needs checking
                    float[,,] alphamap = tData.GetAlphamaps(alphaX, alphaZ, 1, 1);

                    if (alphamap != null)
                    {
                        for (int l = 0; l < numberAlphaMapLayers; l++)
                        {
                            // Get the hashed name of one of the textures at this position in the terrain
                            int texNameHash = terrainTextureNameHashList[l];

                            // Is it in the array of textures we're looking for?
                            int index = System.Array.IndexOf(terrainTextureIDs, texNameHash);

                            if (index >= 0)
                            {
                                // Check the relative weight of this texture at this position
                                if (alphamap[0, 0, l] >= terrainTextureMinWeights[index])
                                {
                                    isTextureMatch = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return isTextureMatch;
        }

        #endregion
    }
}