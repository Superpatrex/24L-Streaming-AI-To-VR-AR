using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A poolable decal used when something is hit or damaged. It should have a single child quad mesh.
    /// </summary> 
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyDecalModule : StickyGenericModule
    {
        #region Public Variables

        /// <summary>
        /// The time, in seconds that the decal will fade from view to prevent
        /// it popping out when it despawns. This should be less than the despawn time.
        /// </summary>
        [Range(0f, 10f)] public float fadeOutTime = 3f;

        /// <summary>
        /// The time, in seconds, between each update to the decal material when fading out of view.
        /// </summary>
        [Range(0.01f, 0.5f)] public float fadeOutFrequency = 0.1f;

        /// <summary>
        /// The amount of overlap permitted when placing near the edge of objects.
        /// </summary>
        [Range(0f, 1f)] public float overlapAmount = 1f;

        /// <summary>
        /// The ID number for this decal prefab (as assigned by the Sticky Manager in the scene).
        /// This is the index in the StickyManager decalTemplatesList.
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] public int decalPrefabID = -1;

        [System.NonSerialized] public bool isVertsCached = false;
        [System.NonSerialized] public bool isMaterialCached = false;

        /// <summary>
        /// The scaled vert positions in local space on the mesh.
        /// </summary>
        [System.NonSerialized] public Vector3[] meshVerts;
        [System.NonSerialized] public int numVerts = 0;
        [System.NonSerialized] public Material decalMaterial = null;

        #endregion

        #region Public Properties

        #endregion

        #region Internal Static Variables
        internal readonly static string decalFadeOutMethodName = "FadeOut";
        #endregion

        #region Private and Protected Variables - General
        private S3DInstantiateGenericObjectParameters igParms;

        protected float initialMaterialAlpha = 0f;
        protected Color fadeMaterialColour;
        protected float startFadeOutAfter = 0f;
        protected float fadeTimer = 0;

        #endregion

        #region Protected Methods - General

        /// <summary>
        /// Cache the mesh verts and disable collider if it has one.
        /// This is likely impact GC but should only be performed once
        /// when the object is first added to the pool.
        /// </summary>
        protected void CacheMeshVerts()
        {
            if (!isVertsCached)
            {
                // Get the first child mesh
                MeshFilter mFilter = GetComponentInChildren<MeshFilter>();

                if (mFilter == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: The StickyDecalModule (" + name + ") is missing the child MeshFilter for the Quad");
                    #endif
                }
                else if (mFilter.mesh != null)
                {
                    // Check for a collider as these are required on the demo prefabs that
                    // ship with Sticky3D to pass publishering validation.
                    Collider dCollider;
                    if (mFilter.TryGetComponent(out dCollider))
                    {
                        #if UNITY_EDITOR
                        DestroyImmediate(dCollider);
                        #else
                        Destroy(dCollider);
                        #endif
                    }

                    meshVerts = mFilter.mesh.vertices;

                    numVerts = meshVerts == null ? 0 : meshVerts.Length;

                    if (numVerts == 4)
                    {
                        Vector3 localScale = mFilter.transform.localScale;

                        // Scale the verts so they are in local space metres rather than normalised positions
                        for (int vtIdx = 0; vtIdx < numVerts; vtIdx++)
                        {
                            Vector3 vert = meshVerts[vtIdx];
                           
                            vert.x = vert.x * localScale.x;
                            vert.y = vert.y * localScale.y;
                            // Offset the corners of the quad slightly behind the decal mesh.
                            // This reduces chances of physics hit failure AND reduces calcs required when testing
                            // the corners of the decal.
                            vert.z = (vert.z * localScale.z) - 0.01f;

                            meshVerts[vtIdx] = vert;
                        }

                        isVertsCached = true;
                    }
                    #if UNITY_EDITOR
                    else
                    {
                        Debug.LogWarning("ERROR: The StickyDecalModule (" + name + ") is expecting a Quad mesh with 4 verts but instead " + mFilter.mesh.name + " has " + numVerts + " verts");
                    }
                    #endif
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("ERROR: The StickyDecalModule (" + name + ") is missing the child mesh (Quad)");
                }
                #endif
            }
        }

        /// <summary>
        /// Cache the material for the decal. It is assumed that there is only
        /// a single renderer with a single material.
        /// </summary>
        protected void CacheMaterial()
        {
            // Get the first renderer for this decal (assumes only 1)
            Renderer renderer = GetComponentInChildren<Renderer>(true);

            if (renderer != null)
            {
                // Get the material for this instance of the decal in the scene
                decalMaterial = renderer.material;

                if (decalMaterial != null)
                {
                    isMaterialCached = true;

                    /// TODO - cater for HDRP decal shader

                    fadeMaterialColour = decalMaterial.color;

                    initialMaterialAlpha = fadeMaterialColour.a;

                    startFadeOutAfter = despawnTime - fadeOutTime;
                    if (startFadeOutAfter < 0f) { startFadeOutAfter = 0f; }
                }
            }
        }

        /// <summary>
        /// Fade the decal from view over time. See Initialise() where it is
        /// invoked every x seconds.
        /// </summary>
        protected void FadeOut()
        {
            fadeTimer += fadeOutFrequency;

            if (fadeOutTime - fadeTimer > 0f)
            {
                fadeMaterialColour.a = (1f - (fadeTimer / fadeOutTime)) * initialMaterialAlpha;
                decalMaterial.color = fadeMaterialColour;
            }
        }

        #endregion

        #region Events

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Removes the Decal (returns it to the pool)
        /// </summary>
        public virtual void DestroyDecal()
        {
            if (isMaterialCached && fadeOutTime > 0f && despawnTime > 0.1f)
            {
                CancelInvoke(decalFadeOutMethodName);
            }

            DestroyGenericObject();
        }

        public virtual uint Initialise (S3DInstantiateDecalParameters idParms)
        {
            // Store the index to the DecalTemplate from the StickyManager decalTemplatesList
            this.decalPrefabID = idParms.decalPrefabID;

            // Initialise generic object module
            // Currently igParms are required to be set for generic object Initialise(..).
            igParms.genericObjectPoolListIndex = -1;
            Initialise(igParms);

            // If required, reset the decal material alpha value.
            if (isMaterialCached && fadeOutTime > 0f && despawnTime > fadeOutFrequency)
            {
                /// TODO - cater for HDRP decal shader

                fadeMaterialColour.a = initialMaterialAlpha;
                decalMaterial.color = fadeMaterialColour;
                fadeTimer = 0f;

                InvokeRepeating(decalFadeOutMethodName, startFadeOutAfter, fadeOutFrequency);
            }

            return itemSequenceNumber;
        }

        /// <summary>
        /// Validate the decal by caching mesh verts and disabling the collider if it has one.
        /// This is called automatically when decals are added to the pool by StickyManager.
        /// Cache the material so that it can be faded if required.
        /// </summary>
        public virtual void ValidateDecal()
        {
            CacheMeshVerts();
            CacheMaterial();
        }

        #endregion
    }
}