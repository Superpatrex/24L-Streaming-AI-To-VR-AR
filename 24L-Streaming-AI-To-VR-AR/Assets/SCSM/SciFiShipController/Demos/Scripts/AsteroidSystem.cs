#if SSC_ENTITIES
using System;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Rendering;
#if UNITY_2022_2_OR_NEWER
using Unity.Entities.Graphics;
#endif

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [Serializable]
    #if UNITY_2020_3_OR_NEWER
    public partial struct Asteroid : IComponentData
    #else
    public struct Asteroid : IComponentData
    #endif
    {
        public float rotationRate;
        public float3 rotationDirection;
    }

    /// <summary>
    /// A DOTS system for creating and rotating asteroids. When Unity.Physics (for DOTS)
    /// is installed, box colliders are added which can be hit by Projectiles with DOTS enabled.
    /// NOTE: Ships currently don't detect Entities in a scene.
    /// Unity 2020.3 LTS or newer requires Entities 0.51-preview.32+
    /// </summary>
    #if UNITY_2020_3_OR_NEWER
    public partial class AsteroidSystem : SystemBase
    #elif UNITY_2020_1_OR_NEWER
    public class AsteroidSystem : SystemBase
    #else
    public class AsteroidSystem : JobComponentSystem
    #endif
    {

        #region Public variables

        #endregion

        #region Private variables
        private static World asteroidWorld;
        private static EntityQuery asteroidEntityQuery;
        private static EntityManager entityManager;
        private static ComponentType compTypeAsteroid;

        #endregion

        #region Structures

        [BurstCompile]
        private struct RotationJob : IJobChunk
        {
            public float deltaTime;

            #if UNITY_2022_2_OR_NEWER
            // ECS 1.0
            public ComponentTypeHandle<LocalTransform> transformType;
            [ReadOnly] public ComponentTypeHandle<Asteroid> asteroidType;
            #elif UNITY_2020_1_OR_NEWER
            // Renamed from ArchetypeChunkComponentType to ComponentTypeHandle in Entities 0.12.0
            public ComponentTypeHandle<Rotation> rotationType;
            [ReadOnly] public ComponentTypeHandle<Asteroid> asteroidType;
            #else
            public ArchetypeChunkComponentType<Rotation> rotationType;
            [ReadOnly] public ArchetypeChunkComponentType<Asteroid> asteroidType;
            #endif

            #if UNITY_2022_2_OR_NEWER
            // ECS 1.0
            public void Execute (in ArchetypeChunk archetypeChunk, int unfilteredChunkIndex, bool useEnabledMask, in Unity.Burst.Intrinsics.v128 chunkEnabledMask)
            {
                NativeArray<LocalTransform> chunkTransforms = archetypeChunk.GetNativeArray(ref transformType);
                var chunkAsteriods = archetypeChunk.GetNativeArray(ref asteroidType);

                for (int i = 0; i < archetypeChunk.Count; i++)
                {
                    LocalTransform localTransform = chunkTransforms[i];
                    Asteroid asteroid = chunkAsteriods[i];

                    quaternion newQuaternion = math.mul(math.normalize(localTransform.Rotation), quaternion.AxisAngle(asteroid.rotationDirection, asteroid.rotationRate * deltaTime));

                    localTransform.Rotation = newQuaternion;

                    chunkTransforms[i] = localTransform;
                }
            }
            #else
            // ECS 0.51

            public void Execute(ArchetypeChunk archetypeChunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkRotations = archetypeChunk.GetNativeArray(rotationType);
                var chunkAsteriods = archetypeChunk.GetNativeArray(asteroidType);

                for (int i = 0; i < archetypeChunk.Count; i++)
                {
                    Rotation rotation = chunkRotations[i];
                    Asteroid asteroid = chunkAsteriods[i];

                    quaternion newQuaternion = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(asteroid.rotationDirection, asteroid.rotationRate * deltaTime));

                    chunkRotations[i] = new Rotation { Value = newQuaternion };
                }
            }
            #endif
        }

        #endregion

        #region Event Methods
        protected override void OnCreate()
        {
            //base.OnCreate();

            // Get the world that will "hold" the entities.
            if (asteroidWorld == null)
            {
                asteroidWorld = DOTSHelper.GetDefaultWorld();
            }

            if (asteroidWorld != null) { entityManager = asteroidWorld.EntityManager; }

            // Cache the component type to avoid ComponentType.op_Implicit in CreateAsteroid(..)
            compTypeAsteroid = typeof(Asteroid);

            // Asteroid entity query code
            // Get all the entities with a Translation, Rotation and Asteroid components
            #if UNITY_2022_2_OR_NEWER
            // ECS 1.0
            asteroidEntityQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(LocalTransform), compTypeAsteroid }
            });
            #else
            // ECS 0.51
            asteroidEntityQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Translation), typeof(Rotation), compTypeAsteroid }
            });
            #endif
        }

        #endregion

        #region Update Methods

        // OnUpdate runs on the main thread.
        #if UNITY_2020_1_OR_NEWER
        protected override void OnUpdate()
        #else
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        #endif
        {

            #if UNITY_2022_2_OR_NEWER
            // ECS 1.0
            // Rotation component is read-write because we want to update the rotation of the asteroids each frame
            var localTransformType = GetComponentTypeHandle<LocalTransform>();
            // Asteroid data is read-only
            var asteroidType = GetComponentTypeHandle<Asteroid>(true);
            #elif UNITY_2020_1_OR_NEWER
            // Replace GetArchetypeChunkComponentType<T>() with GetComponentTypeHandle<T>() in Entities 0.12.0
            // Rotation component is read-write because we want to update the rotation of the asteroids each frame
            var rotationType = GetComponentTypeHandle<Rotation>();
            // Asteroid data is read-only
            var asteroidType = GetComponentTypeHandle<Asteroid>(true);
            #else
            // Rotation component is read-write because we want to update the rotation of the asteroids each frame
            var rotationType = GetArchetypeChunkComponentType<Rotation>();
            // Asteroid data is read-only
            var asteroidType = GetArchetypeChunkComponentType<Asteroid>(true);
            #endif

            RotationJob rotationJob = new RotationJob()
            {
                #if UNITY_2022_2_OR_NEWER
                deltaTime = SystemAPI.Time.DeltaTime,
                #elif UNITY_2019_3_OR_NEWER
                deltaTime = Time.DeltaTime,
                #else
                deltaTime = Time.deltaTime,
                #endif

                #if UNITY_2022_2_OR_NEWER
                transformType = localTransformType,
                #else
                rotationType = rotationType,
                #endif

                asteroidType = asteroidType
            };

            #if UNITY_2020_1_OR_NEWER
            this.Dependency = rotationJob.ScheduleParallel(asteroidEntityQuery, this.Dependency);
            #else
            return rotationJob.Schedule(asteroidEntityQuery, inputDeps);
            #endif
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Create a new asteroid as an entity in the scene.
        /// Currently has no concept of a collider etc.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <param name="rotationRate"></param>
        /// <param name="rotationDirection"></param>
        /// <param name="prefabEntity"></param>
        public static void CreateAsteroid(Vector3 position, Quaternion rotation, Vector3 scale, float rotationRate, Vector3 rotationDirection, Entity prefabEntity)
        {
            // EntityManager is now a struct (was a class). Gets created and destroyed at same time as the World.
            if (asteroidWorld.IsCreated)
            {
                Entity entity = entityManager.Instantiate(prefabEntity);

                // Add an Asteroid component if it wasn't attached to the prefab that was converted
                if (!entityManager.HasComponent(entity, compTypeAsteroid))
                {
                    entityManager.AddComponent(entity, compTypeAsteroid);
                }
               
                #if UNITY_2022_2_OR_NEWER
                // ECS 1.0
                // Add pre-rendering scale component for ECS 1.0
                if (!entityManager.HasComponent(entity, typeof(PostTransformScale)))
                {
                    entityManager.AddComponent(entity, typeof(PostTransformScale));
                }
                #else
                // Add a scale component in ECS 0.51
                if (!entityManager.HasComponent(entity, typeof(NonUniformScale)))
                {
                    entityManager.AddComponent(entity, typeof(NonUniformScale));
                }
                #endif

                #if SSC_PHYSICS
                // Add a unique Unity Physics box collider
                if (entityManager.HasComponent(entity, typeof(Unity.Physics.PhysicsCollider)))
                {
                    entityManager.RemoveComponent<Unity.Physics.PhysicsCollider>(entity);
                }
                var collider = Unity.Physics.BoxCollider.Create(new Unity.Physics.BoxGeometry { Center = float3.zero, Size = (float3)scale, Orientation = quaternion.identity, BevelRadius = 0.1f });
                entityManager.AddComponentData(entity, new Unity.Physics.PhysicsCollider { Value = collider });
                #endif

                // Set their position, rotation and asteroid properties
                // U2022.2+ uses ECS 1.0.0-prev.15.
                #if UNITY_2022_2_OR_NEWER
                // Set position from world origin
                LocalTransform localTrfm = entityManager.GetComponentData<LocalTransform>(entity);
                localTrfm.Position = (float3)position;
                localTrfm.Rotation = (quaternion)rotation;
                localTrfm.Scale = 1;
                entityManager.SetComponentData<LocalTransform>(entity, localTrfm);
                entityManager.SetComponentData(entity, new PostTransformScale() { Value = float3x3.Scale((float3)scale) });
                #else
                // ECS 0.51
                entityManager.SetComponentData(entity, new Translation() { Value = (float3)position });
                entityManager.SetComponentData(entity, new Rotation() { Value = (quaternion)rotation });
                entityManager.SetComponentData(entity, new NonUniformScale() { Value = (float3)scale });
                #endif

                entityManager.SetComponentData(entity, new Asteroid() { rotationRate = rotationRate, rotationDirection = (float3)rotationDirection });
            }
        }

        #endregion
    }
}
#endif