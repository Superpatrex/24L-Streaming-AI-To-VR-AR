using System.Collections.Generic;
using UnityEngine;
#if SSC_ENTITIES
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Rendering;
#endif

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Sample prefab spawner to create something like an asteroid field. The DOTS option
    /// uses currently doesn't support colliders but has the benefit of random rotation.
    /// Data Oriented Technology Stack (DOTS) has the same requirements as DOTS with
    /// Projectiles in Sci-Fi Ship Controller. See the manual for more info on DOTS setup.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Obstacle Spawner")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleObstacleSpawner : MonoBehaviour
    {
        #region Public variables

        public bool initialiseOnAwake;

        [Tooltip("Experimental")]
        public bool useDOTS = false;
        public Transform[] obstaclePrefabs;

        public Vector3 obstacleFieldCentre = new Vector3(500f, 250f, 500f);
        public Vector3 obstacleFieldSize = new Vector3(5000f, 500f, 5000f);
        public int obstacles = 2500;
        public Vector3 obstacleScaleMin = Vector3.one * 5f;
        public Vector3 obstacleScaleMax = Vector3.one * 20f;

        #if SSC_ENTITIES
        public World asteroidWorld;
        public List<EntityArchetype> asteriodEntityArchetypeList;
        public List<Entity> asteriodPrefabEntityList;
        public AsteroidSystem asteroidSystem;
        #if UNITY_2022_2_OR_NEWER
        // blobAssetStore not requried for ECS 1.0 Entity creation
        #elif UNITY_2019_3_OR_NEWER || UNITY_ENTITIES_0_2_0_OR_NEWER
        private BlobAssetStore blobAssetStore;
        #endif
        #endif

        #endregion

        #region Private variables
        private List<Transform> obstacleTrmList;
        private List<Vector3> obstacleRotationDirectionList;
        private List<float> obstacleRotationRateList;
        private int numObstaclesToRotate = 0;
        #endregion

        #region Initialise Methods

        private void Awake()
        {
            if (initialiseOnAwake) { Initialise(); }
        }

        public void Initialise()
        {
            int numObstaclePrefabs = obstaclePrefabs == null ? 0 : obstaclePrefabs.Length;

            if (numObstaclePrefabs > 0)
            {
                float minX = obstacleFieldCentre.x - (obstacleFieldSize.x * 0.5f);
                float maxX = obstacleFieldCentre.x + (obstacleFieldSize.x * 0.5f);
                float minY = obstacleFieldCentre.y - (obstacleFieldSize.y * 0.5f);
                float maxY = obstacleFieldCentre.y + (obstacleFieldSize.y * 0.5f);
                float minZ = obstacleFieldCentre.z - (obstacleFieldSize.z * 0.5f);
                float maxZ = obstacleFieldCentre.z + (obstacleFieldSize.z * 0.5f);
                Vector3 newPosition = Vector3.zero;
                Vector3 newScale = Vector3.one;
                UnityEngine.Random.InitState(0);

                bool isDOTSConfigured = false;

                if (useDOTS) { isDOTSConfigured = this.ConvertPrefabsToEntities(); }
                else
                {
                    obstacleTrmList = new List<Transform>(obstacles);
                    obstacleRotationDirectionList = new List<Vector3>(obstacles);
                    obstacleRotationRateList = new List<float>(obstacles);
                }

                for (int i = 0; i < obstacles; i++)
                {
                    newPosition.x = UnityEngine.Random.Range(minX, maxX);
                    newPosition.y = UnityEngine.Random.Range(minY, maxY);
                    newPosition.z = UnityEngine.Random.Range(minZ, maxZ);

                    newScale = new Vector3(UnityEngine.Random.Range(obstacleScaleMin.x, obstacleScaleMax.x), UnityEngine.Random.Range(obstacleScaleMin.y, obstacleScaleMax.y), UnityEngine.Random.Range(obstacleScaleMin.z, obstacleScaleMax.z));

                    float rotationRate = UnityEngine.Random.Range(0.1f, 1.0f);
                    Vector3 rotationDirection = new Vector3(UnityEngine.Random.Range(0.01f, 1.0f), UnityEngine.Random.Range(0.01f, 1.0f), UnityEngine.Random.Range(0.01f, 1.0f));

                    // By design, if the DOTS option is enabled but DOTS is not configured it will render nothing in the scene.
                    if (useDOTS)
                    {
                        if (isDOTSConfigured)
                        {
                            #if SSC_ENTITIES
                            AsteroidSystem.CreateAsteroid(newPosition, Quaternion.identity, newScale, rotationRate, rotationDirection, asteriodPrefabEntityList[UnityEngine.Random.Range(0, numObstaclePrefabs - 1)]);
                            #endif
                        }
                    }
                    else
                    {
                        // select a prefab from the list and add it to the scene
                        Transform obstacleTrfm = Instantiate(obstaclePrefabs[UnityEngine.Random.Range(0, numObstaclePrefabs - 1)], newPosition, Quaternion.identity, transform);

                        if (obstacleTrfm != null)
                        {
                            obstacleTrfm.localScale = newScale;
                            obstacleTrmList.Add(obstacleTrfm);
                            obstacleRotationDirectionList.Add(rotationDirection);
                            obstacleRotationRateList.Add(rotationRate);
                            numObstaclesToRotate++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Convert the set of prefabs into Entities.
        /// Create an instance of the AsteroidSystem.
        /// </summary>
        /// <returns></returns>
        public bool ConvertPrefabsToEntities()
        {
            bool isConfigured = false;

            #if SSC_ENTITIES
            int numObstaclePrefabs = obstaclePrefabs == null ? 0 : obstaclePrefabs.Length;

            // Create empty lists with sufficient capacity
            asteriodEntityArchetypeList = new List<EntityArchetype>(numObstaclePrefabs > 0 ? numObstaclePrefabs : 1);
            asteriodPrefabEntityList = new List<Entity>(numObstaclePrefabs > 0 ? numObstaclePrefabs : 1);

            if (numObstaclePrefabs > 0 && useDOTS)
            {
                asteroidWorld = DOTSHelper.GetDefaultWorld();

                if (asteroidWorld != null)
                {
                    #region Construct Entity Archetypes

                    #if UNITY_2022_2_OR_NEWER
                    List<MeshRenderer> mRenList = new List<MeshRenderer>();
                    List<Mesh> meshList = new List<Mesh>();
                    List<Material> matList = new List<Material>();
                    EntityManager entityManager = asteroidWorld.EntityManager;

                    #elif UNITY_2019_3_OR_NEWER || UNITY_ENTITIES_0_2_0_OR_NEWER
                    blobAssetStore = new BlobAssetStore();
                    GameObjectConversionSettings conversionSettings = GameObjectConversionSettings.FromWorld(asteroidWorld, blobAssetStore);
                    #endif

                    // Reserve some space for colliders
                    List<Collider> colliderList = new List<Collider>(4);

                    for (int pfIdx = 0; pfIdx < numObstaclePrefabs; pfIdx++)
                    {
                        // NOTE: Currently, if there are Colliders on child gameobjects in the prefab, it doesn't render
                        // after being converted to an entity.

                        obstaclePrefabs[pfIdx].GetComponentsInChildren<Collider>(colliderList);

                        int numColliders = colliderList == null ? 0 : colliderList.Count;

                        for (int clIdx = 0; clIdx < numColliders; clIdx++)
                        {
                            Collider collider = colliderList[clIdx];
                            // Ignore colliders on the parent
                            if (collider.transform != obstaclePrefabs[pfIdx])
                            {
                                if (collider.enabled) { collider.enabled = false; }
                                //Debug.Log("[DEBUG] collider: " + collider.name);
                            }
                        }

                        //if (collider != null) { Debug.Log("[DEBUG] collider: " + collider.name); }


                        // WARNING: ConvertGameObjectHierarchy has been REMOVED in ECS 1.0 for U2022.2 and WILL NOT WORK...

                        #if UNITY_2022_2_OR_NEWER
                        // ECS 1.0
                        // Create the parent entity components
                        Entity prefabEntity = entityManager.CreateEntity();
                        
                        entityManager.AddComponent(prefabEntity, typeof(LocalTransform));
                        entityManager.AddComponent(prefabEntity, typeof(Asteroid));
                        entityManager.AddComponent(prefabEntity, typeof(PostTransformScale));

                        MeshRenderer mRen;
                        MeshFilter mFilter;

                        Transform prefabRootTransform = obstaclePrefabs[pfIdx];

                        // Is there a mesh renderer and filter on the prefab root gameobject?
                        if (prefabRootTransform.TryGetComponent(out mRen) && mRen.TryGetComponent(out mFilter))
                        {
                            // Create a RenderMeshDescription with named parameters.
                            var desc = new RenderMeshDescription(
                                shadowCastingMode: mRen.shadowCastingMode,
                                receiveShadows: mRen.receiveShadows);

                            // Create an array of mesh and material required for runtime rendering.
                            var renderMeshArray = new RenderMeshArray(mRen.sharedMaterials, new Mesh[] { mFilter.sharedMesh });

                            RenderMeshUtility.AddComponents
                            (
                                entity: prefabEntity,
                                entityManager: entityManager,
                                renderMeshDescription: desc,
                                renderMeshArray: renderMeshArray,
                                materialMeshInfo: MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0)
                            );
                        }
                        else
                        {
                            // TEST just add first mesh to parent
                            mRen = prefabRootTransform.GetComponentInChildren<MeshRenderer>();
                            if (mRen != null && mRen.TryGetComponent(out mFilter))
                            {
                                // Create a RenderMeshDescription with named parameters.
                                var desc = new RenderMeshDescription(
                                    shadowCastingMode: mRen.shadowCastingMode,
                                    receiveShadows: mRen.receiveShadows);

                                // Create an array of mesh and material required for runtime rendering.
                                var renderMeshArray = new RenderMeshArray(mRen.sharedMaterials, new Mesh[] { mFilter.sharedMesh });

                                RenderMeshUtility.AddComponents
                                (
                                    entity: prefabEntity,
                                    entityManager: entityManager,
                                    renderMeshDescription: desc,
                                    renderMeshArray: renderMeshArray,
                                    materialMeshInfo: MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0)
                                );
                            }
                        }

                        // Find non-recursive child transform (we could use GetComponentsInChildren but will add more complexity)
                        foreach (Transform childTfrm in prefabRootTransform)
                        {
                            // Is there a mesh renderer and filter on the prefab's child transform?
                            if (childTfrm.TryGetComponent(out mRen) && childTfrm.TryGetComponent(out mFilter))
                            {
                                Entity childEntity = entityManager.CreateEntity();
                                entityManager.AddComponentData(childEntity, new LocalTransform() { Position = childTfrm.localPosition, Rotation = childTfrm.localRotation, Scale = 1 });
                                entityManager.AddComponentData(childEntity, new Parent() { Value = prefabEntity });

                                // Create a RenderMeshDescription with named parameters.
                                var desc = new RenderMeshDescription(
                                    shadowCastingMode: mRen.shadowCastingMode,
                                    receiveShadows: mRen.receiveShadows);

                                // Create an array of mesh and material required for runtime rendering.
                                var renderMeshArray = new RenderMeshArray(mRen.sharedMaterials, new Mesh[] { mFilter.sharedMesh });

                                RenderMeshUtility.AddComponents
                                (
                                    entity: childEntity,
                                    entityManager: entityManager,
                                    renderMeshDescription: desc,
                                    renderMeshArray: renderMeshArray,
                                    materialMeshInfo: MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0)
                                );
                            }
                        }
                        
                        #elif UNITY_2019_3_OR_NEWER || UNITY_ENTITIES_0_2_0_OR_NEWER
                        // U2019.3 introduced Entities 0.2.0
                        Entity prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(obstaclePrefabs[pfIdx].gameObject, conversionSettings);
                        #else
                        // U2019.1, 2019.2 Entities 0.012 preview 33 to 0.1.1 preview.
                        Entity prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(obstaclePrefabs[pfIdx].gameObject, asteroidWorld);
                        #endif

                        asteriodPrefabEntityList.Add(prefabEntity);

                        // NOTE: We many only need one of these for all the Asteroid prefabs
                        #if UNITY_2022_2_OR_NEWER
                        ComponentType[] archetypeComponentTypeArray = new ComponentType[4];
                        #else
                        ComponentType[] archetypeComponentTypeArray = new ComponentType[4];
                        #endif

                        // With ConvertGameObjectHierarchy not sure if we need Translation and Rotation here given they are automatically added
                        #if UNITY_2022_2_OR_NEWER
                        archetypeComponentTypeArray[0] = typeof(LocalTransform);
                        archetypeComponentTypeArray[1] = typeof(PostTransformScale);
                        archetypeComponentTypeArray[2] = typeof(Asteroid);

                        // NOTE: Requires hybrid renderer
                        archetypeComponentTypeArray[3] = typeof(RenderMesh);
                        #else
                        // ECS 0.51
                        archetypeComponentTypeArray[0] = typeof(Translation);
                        archetypeComponentTypeArray[1] = typeof(Rotation);
                        archetypeComponentTypeArray[2] = typeof(Asteroid);

                        // NOTE: Requires hybrid renderer
                        archetypeComponentTypeArray[3] = typeof(RenderMesh);
                        #endif

                        EntityArchetype entityArcheType = asteroidWorld.EntityManager.CreateArchetype(archetypeComponentTypeArray);
                        asteriodEntityArchetypeList.Add(entityArcheType);
                    }

                    #endregion

                    // Initialise Asteroid system
                    if (asteroidSystem == null)
                    {
                        #if UNITY_2022_2_OR_NEWER
                        // ECS 1.0
                        asteroidSystem = asteroidWorld.GetOrCreateSystemManaged<AsteroidSystem>();
                        #else
                        // ECS 0.51
                        asteroidSystem = asteroidWorld.GetOrCreateSystem<AsteroidSystem>();
                        #endif
                    }
                }

                isConfigured = asteroidSystem != null;
            }
            #endif

            return isConfigured;
        }

        #endregion

        #region Event Methods

        private void Update()
        {
            if (useDOTS) { return; }
            else
            {
                float deltaTime = Time.deltaTime;

                // Rotate all the obstacles
                for (int aIdx = 0; aIdx < numObstaclesToRotate; aIdx++)
                {
                    obstacleTrmList[aIdx].rotation *= Quaternion.AngleAxis(obstacleRotationRateList[aIdx] * deltaTime * Mathf.Rad2Deg, obstacleRotationDirectionList[aIdx]);
                }
            }
        }

        #if SSC_ENTITIES && (UNITY_2019_3_OR_NEWER || UNITY_ENTITIES_0_2_0_OR_NEWER)
        private void OnDestroy()
        {
            // Dispose of the BlobAssetStore, else we're get a message:
            // A Native Collection has not been disposed, resulting in a memory leak.
            #if UNITY_2022_2_OR_NEWER
            //if (blobAssetStore.IsCreated) { blobAssetStore.Dispose(); }
            #elif SSC_ENTITIES && (UNITY_2019_3_OR_NEWER || UNITY_ENTITIES_0_2_0_OR_NEWER)
            if (blobAssetStore != null) { blobAssetStore.Dispose(); }
            #endif
        }
        #endif
        #endregion

    }
}
