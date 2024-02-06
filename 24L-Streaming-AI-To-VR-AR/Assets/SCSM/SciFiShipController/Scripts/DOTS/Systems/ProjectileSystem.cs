#if SSC_ENTITIES
using System;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    #region ProjectileComponent

    [Serializable]
    public struct Projectile : IComponentData
    {
        public float3 velocity;
        public float damageAmount;
        public float despawnTime;
        public float despawnTimer;
        public byte useGravity; // 0 = no, 1 = yes
        public float gravity;
        public float3 gravityDirection;
        public float speed;
        public float3 fwdDirection;
        public float3 upDirection;
        public int projectilePrefabID;
        public int effectsObjectPrefabID;
        public int shieldEffectsObjectPrefabID;
        public int sourceShipId;
        public int sourceSquadronId;
        public int damageTypeInt;
        public int sceneHandle;
        public int _tempIdx;
    }

    #endregion

    #region ProjectileSystem

    /// <summary>
    /// ProjectileSystem is used to create and move projectiles with the
    /// Data-Orientated Tech Stack (DOTS). It uses C# Jobs, Entities and
    /// the Burst compiler.The default SimulationSystemGroup in Entities
    /// 0.0.12 preview 30 runs at end of Update rather than FixedUpdate.
    /// So disable auto creation and create and update it manually from
    /// SSCManager.
    /// U2019.1 - Entities 0.0.12-preview.30
    /// U2019.2 - Entities 0.1.1 preview
    /// U2019.3 - Entities 0.2.0 preview.18
    /// U2019.4 - Entities 0.5.2 preview.4
    /// U2020.1 - Entities 0.12.0 (untested)
    /// U2020.3 - Entities 0.51.0 preview.32+ (SSC 1.3.4 and earlier Entities 0.17.0-preview.42)
    /// U2022.2 - Entities 1.0.0-pre.15 (SSC 1.3.8+ WIP)
    /// </summary>
    [DisableAutoCreation]
    #if UNITY_2020_3_OR_NEWER
    public partial class ProjectileSystem : SystemBase
    #elif UNITY_2020_1_OR_NEWER
    public class ProjectileSystem : SystemBase
    #else
    public class ProjectileSystem : JobComponentSystem
    #endif
    {
        #region Public Properties

        // For testing only
        public static int GetTotalProjectiles { get; private set; }

        #endregion

        #region Private Variables
        // EntityQuery aka ComponentGroup pre-ECS 0.27
        private static EntityQuery projectileEntityQuery;

        private static EntityManager entityManager;

        private static ComponentType compTypeProjectile;

        private Vector3 pos, velo, frameMovement;

        private Entity projectileEntity;
        private int nProjectiles;

        private NativeArray<RaycastHit> raycastResults;
        private NativeArray<RaycastCommand> raycastCommands;
        private RaycastCommand raycastCommand;

        private NativeList<Entity> entitiesToDestroy;
        private RaycastHit hitInfo;

        private SSCManager sscManager;

        #if SSC_PHYSICS
        private Unity.Physics.Systems.BuildPhysicsWorld buildPhysicsWorld;
        private NativeArray<Unity.Physics.RaycastHit> physicsraycastResults;
        private int numPhysicsRaycastHits;
        private int numPhysicsBodies;
        //private NativeList<Unity.Physics.RaycastHit> physicsraycastResultList;
        #endif

        #endregion

        #region Projectile Movement Job

        [BurstCompile]
        struct ProjectileMoveJob : IJobChunk
        {
            public float deltaTime;

            #if UNITY_2022_2_OR_NEWER
            // ECS 1.0
            public ComponentTypeHandle<LocalTransform> transformType;
            public ComponentTypeHandle<Projectile> projectileType;
            #elif UNITY_2020_1_OR_NEWER
            // Renamed from ArchetypeChunkComponentType to ComponentTypeHandle in Entities 0.12.0
            public ComponentTypeHandle<Translation> translationType;
            public ComponentTypeHandle<Projectile> projectileType;
            #else
            public ArchetypeChunkComponentType<Translation> translationType;
            public ArchetypeChunkComponentType<Projectile> projectileType;
            #endif

            #if UNITY_2022_2_OR_NEWER
            // ECS 1.0
            public void Execute (in ArchetypeChunk archetypeChunk, int unfilteredChunkIndex, bool useEnabledMask, in Unity.Burst.Intrinsics.v128 chunkEnabledMask)
            #else
            // ECS 0.51
            public void Execute(ArchetypeChunk archetypeChunk, int chunkIndex, int firstEntityIndex)
            #endif
            {
                #if UNITY_2022_2_OR_NEWER
                // ECS 1.0
                NativeArray<LocalTransform> chunkTransforms = archetypeChunk.GetNativeArray(ref transformType);
                NativeArray<Projectile> chunkProjectiles = archetypeChunk.GetNativeArray(ref projectileType);
                int numPositions = chunkTransforms.Length;
                #else
                // ECS 0.51
                NativeArray<Translation> chunkTranslations = archetypeChunk.GetNativeArray(translationType);
                NativeArray<Projectile> chunkProjectiles = archetypeChunk.GetNativeArray(projectileType);
                int numPositions = chunkTranslations.Length;
                #endif

                for (int i = 0; i < numPositions; i++)
                {
                    Projectile projectile = chunkProjectiles[i];
                    #if UNITY_2022_2_OR_NEWER
                    // ECS 1.0
                    LocalTransform localTransform = chunkTransforms[i];
                    #else
                    // ECS 0.51
                    Translation position = chunkTranslations[i];
                    #endif

                    // Use Gravity?
                    if (projectile.useGravity == (byte)1)
                    {
                        projectile.velocity += projectile.gravity * deltaTime * projectile.gravityDirection;
                    }

                    // Update our current position using our velocity and frame time
                    #if UNITY_2022_2_OR_NEWER
                    // ECS 1.0
                    localTransform.Position += projectile.velocity * deltaTime;
                    chunkTransforms[i] = localTransform;
                    #else
                    // ECS 0.51
                    position.Value += projectile.velocity * deltaTime;
                    chunkTranslations[i] = position;
                    #endif

                    // Update the timer so that in the OnUpdate we can destroy the entity when required.
                    projectile.despawnTimer += deltaTime;

                    chunkProjectiles[i] = projectile;

                    // Update our current rotation using our velocity (currently not required)
                    //rotation.Value = quaternion.LookRotationSafe(movement.Velocity, new float3(0f, 1f, 0f));
                }
            }
        }

        [BurstCompile]
        struct ProjectileTelePortJob: IJobChunk
        {
            public float3 deltaPosition;

            #if UNITY_2022_2_OR_NEWER
            // ECS 1.0
            public ComponentTypeHandle<LocalTransform> transformType;
            #elif UNITY_2020_1_OR_NEWER
            // Renamed from ArchetypeChunkComponentType to ComponentTypeHandle in Entities 0.12.0
            public ComponentTypeHandle<Translation> translationType;
            #else
            public ArchetypeChunkComponentType<Translation> translationType;
            #endif

            #if UNITY_2022_2_OR_NEWER
            // ECS 1.0
            public void Execute (in ArchetypeChunk archetypeChunk, int unfilteredChunkIndex, bool useEnabledMask, in Unity.Burst.Intrinsics.v128 chunkEnabledMask)
            #else
            // ECS 0.51
            public void Execute(ArchetypeChunk archetypeChunk, int chunkIndex, int firstEntityIndex)
            #endif
            {
                #if UNITY_2022_2_OR_NEWER
                // ECS 1.0
                NativeArray<LocalTransform> chunkTransforms = archetypeChunk.GetNativeArray(ref transformType);

                for (int i = 0; i < chunkTransforms.Length; i++)
                {
                    LocalTransform localTransform = chunkTransforms[i];
                    localTransform.Position += deltaPosition;
                    chunkTransforms[i] = localTransform;
                }
                #else
                // ECS 0.51
                NativeArray<Translation> chunkTranslations = archetypeChunk.GetNativeArray(translationType);

                for (int i = 0; i < chunkTranslations.Length; i++)
                {
                    Translation translation = chunkTranslations[i];
                    translation.Value += deltaPosition;
                    chunkTranslations[i] = translation;
                }
                #endif
            }
        }

        #endregion

        #region Physics Raycast Job
        #if SSC_PHYSICS

        /// <summary>
        /// Parallel job for casting rays from projectiles using Unity.Physics
        /// </summary>
        [BurstCompile]
        struct ProjectilePhysicsRayJob : IJobForEach<Translation, Projectile>
        {
            public NativeArray<Unity.Physics.RaycastHit> raycastResultsInJob;
            [ReadOnly] public Unity.Physics.CollisionWorld collisionWorldInJob;
            [ReadOnly] public float deltaTime;

            public void Execute([ReadOnly] ref Translation position, ref Projectile projectile)
            {
                Unity.Physics.RaycastInput raycastInput = new Unity.Physics.RaycastInput
                {
                    Start = position.Value,                    
                    End = position.Value + (projectile.velocity * deltaTime),
                    Filter = Unity.Physics.CollisionFilter.Default
                };

                if (collisionWorldInJob.CastRay(raycastInput, out Unity.Physics.RaycastHit hit))
                {
                    if (hit.RigidBodyIndex == 0) { hit.Position = new float3(1f, 1f, 1f); }

                    raycastResultsInJob[projectile._tempIdx] = hit;
                }
                else
                {
                    // Dummy hit
                    Unity.Physics.RaycastHit raycastHit = new Unity.Physics.RaycastHit();
                    raycastHit.RigidBodyIndex = -1;
                    raycastResultsInJob[projectile._tempIdx] = raycastHit;
                }
            }
        }

        #endif
        #endregion

        #region Event Methods

        protected override void OnCreate()
        {
            // Get the current entity manager. If it doesn't exist, create one.
            entityManager = SSCManager.sscWorld.EntityManager;

            // Cache the component type to avoid ComponentType.op_Implicit in CreateProjectile(..)
            compTypeProjectile = typeof(Projectile);

            // Projectile entity query code
            // Get all the entities with a Translation and Projectile component
            projectileEntityQuery = GetEntityQuery(new EntityQueryDesc
            {
                #if UNITY_2022_2_OR_NEWER
                All = new ComponentType[] { typeof(LocalTransform), compTypeProjectile }
                #else
                All = new ComponentType[] { typeof(Translation), typeof(Rotation), compTypeProjectile }
                #endif
            });

            // Initialise raycast command
            #if UNITY_2022_2_OR_NEWER
            // ECS 1.0 - TODO: Check if Default QueryParameters is what we want
            raycastCommand = new RaycastCommand(Vector3.zero, Vector3.forward, QueryParameters.Default, 1f);
            #else
            // ECS 0.51
            raycastCommand = new RaycastCommand(Vector3.zero, Vector3.forward, 1f);
            #endif

            // Initialise entities to destroy list
            entitiesToDestroy = new NativeList<Entity>(Allocator.Persistent);

            sscManager = SSCManager.GetOrCreateManager();

            #if SSC_PHYSICS
            if (!DOTSHelper.GetBuildPhysicsWorld(SSCManager.sscWorld, ref buildPhysicsWorld))
            {
                #if UNITY_EDITOR
                Debug.Log("ERROR: ProjectileSystem.OnCreate() - could not get physicsworld - PLEASE REPORT");
                #endif
            }
            else
            {
                // Create empty resultset. Note the difference between Physics and UnityEngine RaycastHit.
                //physicsraycastResultList = new NativeList<Unity.Physics.RaycastHit>(Allocator.Persistent);
            }
            #endif
        }

        protected override void OnDestroy()
        {
            // Dispose of the entities to destroy native list
            entitiesToDestroy.Dispose();

            #if SSC_PHYSICS
            if (physicsraycastResults.IsCreated) { physicsraycastResults.Dispose(); }
            #endif
        }

        #endregion

        #region Update Methods

        // OnUpdate runs on the main thread.
        #if UNITY_2020_1_OR_NEWER
        protected override void OnUpdate()
        #else
        protected override JobHandle OnUpdate(JobHandle jobHandle)
        #endif
        {
            //int nProjectiles = 0;
            nProjectiles = 0;

            #region Physics Updates
            #if SSC_PHYSICS
            // Ensure physics world updates first
            #if UNITY_2020_1_OR_NEWER
            // FinalJobHandle deprecated in Unity.Physics 0.4.0-preview.5. Replaced with GetOutputDependency().
            this.Dependency = JobHandle.CombineDependencies(this.Dependency, buildPhysicsWorld.GetOutputDependency());
            #else
            jobHandle = JobHandle.CombineDependencies(jobHandle, buildPhysicsWorld.FinalJobHandle);
            #endif
            #endif
            #endregion

            // Get all of the projectile entities in the scene
            // wrap in using statement to automatically dispose after use
            using (NativeArray<Entity> nativeArray = projectileEntityQuery.ToEntityArray(Allocator.TempJob))
            {
                nProjectiles = nativeArray.Length;

                // for testing only
                GetTotalProjectiles = nProjectiles;
            }

            // Gather the types of the components that we want to manipulate
            #if UNITY_2022_2_OR_NEWER
            // ECS 1.0
            ComponentTypeHandle<LocalTransform> transformType = GetComponentTypeHandle<LocalTransform>();
            ComponentTypeHandle<Projectile> projectileType = GetComponentTypeHandle<Projectile>();
            #elif UNITY_2020_1_OR_NEWER
            // Renamed from ArchetypeChunkComponentType to ComponentTypeHandle in Entities 0.12.0
            // Replace GetArchetypeChunkComponentType<T>() with GetComponentTypeHandle<T>()
            ComponentTypeHandle<Translation> positionType = GetComponentTypeHandle<Translation>();
            ComponentTypeHandle<Rotation> rotationType = GetComponentTypeHandle<Rotation>();
            ComponentTypeHandle<Projectile> projectileType = GetComponentTypeHandle<Projectile>();
            #else
            ArchetypeChunkComponentType<Translation> positionType = GetArchetypeChunkComponentType<Translation>();
            ArchetypeChunkComponentType<Rotation> rotationType = GetArchetypeChunkComponentType<Rotation>();
            ArchetypeChunkComponentType<Projectile> projectileType = GetArchetypeChunkComponentType<Projectile>();
            #endif

            // Get all the chunks (segments of data) that match this query (that have Translation and Projectile components)
            // The chunks will only contain our projectile entities.
            #if UNITY_2022_2_OR_NEWER
            // ECS 1.0
            NativeArray<ArchetypeChunk> chunks = projectileEntityQuery.ToArchetypeChunkArray(Allocator.TempJob);
            #else
            // ECS 0.51
            NativeArray<ArchetypeChunk> chunks = projectileEntityQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            #endif

            // Set up the command and result buffers
            raycastCommands = new NativeArray<RaycastCommand>(nProjectiles, Allocator.TempJob);
            raycastResults = new NativeArray<RaycastHit>(nProjectiles, Allocator.TempJob);

            // Get delta time once

            // This system is updated from FixedUpdate, so use PhysX time rather than ECS timing.
            float deltaTime = UnityEngine.Time.fixedDeltaTime;
            
            //#if UNITY_2022_2_OR_NEWER
            //float deltaTime = SystemAPI.Time;
            //#elif UNITY_2019_3_OR_NEWER
            //float deltaTime = Time.DeltaTime;
            //#else
            //float deltaTime = Time.deltaTime;
            //#endif

            #region Raycast Job

            // TODO - investigate using a job to populate the raycastCommands NativeArray
            // Iterate through all projectile entities
            int raycastIndex = 0;
            // Loop through the chunks
            int chunksLength = chunks == null ? 0 : chunks.Length;
            for (int chunkIndex = 0; chunkIndex < chunksLength; chunkIndex++)
            {
                // Get the current chunk
                ArchetypeChunk chunk = chunks[chunkIndex];

                #if UNITY_2022_2_OR_NEWER
                // ECS 1.0
                // Get an array of the LocalTransform components from the entities (that match the projectileEntityQuery) within this chunk
                NativeArray<LocalTransform> localTransformComponents = chunk.GetNativeArray(ref transformType);
                // Get an array of the projectile components from the entities within this chunk
                NativeArray<Projectile> projectileComponents = chunk.GetNativeArray(ref projectileType);
                #else
                // ECS 0.51
                // Get an array of the position components from the entities (that match the projectileEntityQuery) within this chunk
                NativeArray<Translation> positionComponents = chunk.GetNativeArray(positionType);
                // Get an array of the projectile components from the entities within this chunk
                NativeArray<Projectile> projectileComponents = chunk.GetNativeArray(projectileType);
                #endif

                // Loop through the entities in this chunk
                int chunkSize = chunk == null ? 0 : chunk.Count;
                for (int entityIndex = 0; entityIndex < chunkSize; entityIndex++)
                {
                    // Get the position of this projectile
                    #if UNITY_2022_2_OR_NEWER
                    // ECS 1.0
                    pos = localTransformComponents[entityIndex].Position;
                    #else
                    // ECS 0.51
                    pos = positionComponents[entityIndex].Value;
                    #endif
                    // Get the velocity of this projectile
                    velo = projectileComponents[entityIndex].velocity;

                    // Calculate raycast data
                    raycastCommand.from = pos;
                    raycastCommand.direction = velo;
                    raycastCommand.distance = (velo * deltaTime).magnitude;

                    // Set raycast data
                    raycastCommands[raycastIndex] = raycastCommand;

                    // Used in the Unity.Physics raycast job
                    Projectile _projectile = projectileComponents[entityIndex];
                    _projectile._tempIdx = raycastIndex;
                    projectileComponents[entityIndex] = _projectile;

                    //Debug.Log("tempidx: " + _projectile._tempIdx + " actual: " + raycastIndex);

                    // Increment the raycast index - we're doing things this way to make sure that
                    // in the second loop the indices all match up correctly
                    raycastIndex++;
                }
            }

            // Schedule the batch of raycasts
            // TODO - in Entities 12+, not sure if this a parallel job...
            JobHandle handle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, 1, default(JobHandle));

            // Wait for the batch processing job to complete
            handle.Complete();

            #endregion

            #region Unity.Physics Raycast Job
            #if SSC_PHYSICS
            physicsraycastResults = new NativeArray<Unity.Physics.RaycastHit>(nProjectiles, Allocator.TempJob);

            if (physicsraycastResults.IsCreated)
            {
                
                #if UNITY_2020_1_OR_NEWER
                // Check if this is parallel...
                new ProjectilePhysicsRayJob()
                {
                    collisionWorldInJob = buildPhysicsWorld.PhysicsWorld.CollisionWorld,
                    raycastResultsInJob = physicsraycastResults,
                    deltaTime = deltaTime
                }.Schedule(this, this.Dependency).Complete();
                #else
                new ProjectilePhysicsRayJob()
                {
                    collisionWorldInJob = buildPhysicsWorld.PhysicsWorld.CollisionWorld,
                    raycastResultsInJob = physicsraycastResults,
                    deltaTime = deltaTime
                }.Schedule(this, jobHandle).Complete();
                #endif

                numPhysicsRaycastHits = physicsraycastResults.Length;
                numPhysicsBodies = buildPhysicsWorld.PhysicsWorld.CollisionWorld.NumBodies;
            }
            else
            {
                numPhysicsRaycastHits = 0;
                numPhysicsBodies = 0;
            }
            #endif
            #endregion

            #region Process Raycasts for collision and despawn old projectiles
            
            // Get an archetype for projectiles
            #if UNITY_2020_1_OR_NEWER
            // Renamed from ArchetypeChunkEntityType to EntityTypeHandle in Entities 0.12.0
            // Replace GetArchetypeChunkEntityType() with GetEntityTypeHandle()
            EntityTypeHandle projectileChunkEntityArchetype = GetEntityTypeHandle();
            #else
            ArchetypeChunkEntityType projectileChunkEntityArchetype = GetArchetypeChunkEntityType();
            #endif

            // Iterate through all projectile entities again
            // Reset raycastIndex
            raycastIndex = 0;
            Collider other;
            for (int chunkIndex = 0; chunkIndex < chunksLength; chunkIndex++)
            {
                // Get the current chunk of (projectile) entities
                ArchetypeChunk chunk = chunks[chunkIndex];

                // Get a native array of the entities in this chunk
                NativeArray<Entity> chunkEntities = chunk.GetNativeArray(projectileChunkEntityArchetype);

                #if UNITY_2022_2_OR_NEWER
                NativeArray<Projectile> projectileComponents = chunk.GetNativeArray(ref projectileType);
                NativeArray<LocalTransform> raytransformComponents = chunk.GetNativeArray(ref transformType);
                #else
                // Get arrays of the projectile and rotations components from the entities within this chunk
                NativeArray<Projectile> projectileComponents = chunk.GetNativeArray(projectileType);
                NativeArray<Rotation> rotationComponents = chunk.GetNativeArray(rotationType);
                #endif

                // Loop through the entities in this chunk
                int chunkSize = chunk == null ? 0 : chunk.Count;
                for (int entityIndex = 0; entityIndex < chunkSize; entityIndex++)
                {
                    Projectile projectile = projectileComponents[entityIndex];
                    #if UNITY_2022_2_OR_NEWER
                    // ECS 1.0
                    LocalTransform rayLocalTransform = raytransformComponents[entityIndex];
                    #else
                    // ECS 0.51
                    Rotation rotation = rotationComponents[entityIndex];
                    #endif

                    #region Process Legacy Physics Raycast results
                    hitInfo = raycastResults[raycastIndex];
                    other = hitInfo.collider;

                    // If raycastResults[raycastIndex].collider == null there was no hit
                    if (other != null)
                    {

                        bool isShieldHit = false;
                        ShipControlModule shipControlModule = null;

                        // Do we need to check for ship shield hits?
                        if (projectile.shieldEffectsObjectPrefabID >= 0 && ProjectileModule.CheckShipHit(hitInfo, projectile.damageAmount, (ProjectileModule.DamageType)projectile.damageTypeInt, projectile.sourceShipId, projectile.sourceSquadronId, projectile.projectilePrefabID, out shipControlModule))
                        {
                            isShieldHit = shipControlModule.shipInstance.HasActiveShield(hitInfo.point);
                        }
                        // No shield effects so perform a regular CheckShipHit
                        else if (projectile.shieldEffectsObjectPrefabID < 0 && ProjectileModule.CheckShipHit(hitInfo, projectile.damageAmount, (ProjectileModule.DamageType)projectile.damageTypeInt, projectile.sourceShipId, projectile.sourceSquadronId, projectile.projectilePrefabID))
                        {
                            // No need to do anything else here
                        }
                        else
                        {
                            // If it hit an object with a DamageReceiver script attached, take appropriate action like call a custom method
                            ProjectileModule.CheckObjectHit(hitInfo, projectile.damageAmount, (ProjectileModule.DamageType)projectile.damageTypeInt, projectile.sourceShipId, projectile.sourceSquadronId, projectile.projectilePrefabID);
                        }

                        // OLD CODE pre v1.3.5
                        // Determine if it has hit a ship
                        //if (!ProjectileModule.CheckShipHit(hitInfo, projectile.damageAmount, (ProjectileModule.DamageType)projectile.damageTypeInt, projectile.sourceShipId, projectile.sourceSquadronId, projectile.projectilePrefabID))
                        //{
                        //    // If it hit an object with a DamageReceiver script attached, take appropriate action like call a custom method
                        //    ProjectileModule.CheckObjectHit(hitInfo, projectile.damageAmount, (ProjectileModule.DamageType)projectile.damageTypeInt, projectile.sourceShipId, projectile.sourceSquadronId, projectile.projectilePrefabID);
                        //}

                        // If required, use a shield EffectsObject.
                        if (isShieldHit && projectile.shieldEffectsObjectPrefabID >= 0)
                        {
                            if (sscManager != null)
                            {
                                InstantiateEffectsObjectParameters ieParms = new InstantiateEffectsObjectParameters
                                {
                                    effectsObjectPrefabID = projectile.shieldEffectsObjectPrefabID,
                                    position = hitInfo.point + (hitInfo.normal * 0.0005f),
                                    rotation = Quaternion.LookRotation(-hitInfo.normal),
                                };

                                // For projectiles we don't need to get the effectsObject key from ieParms.
                                sscManager.InstantiateEffectsObject(ref ieParms);
                            }
                        }
                        else if (!isShieldHit && projectile.effectsObjectPrefabID >= 0 && sscManager != null)
                        {
                            InstantiateEffectsObjectParameters ieParms = new InstantiateEffectsObjectParameters
                            {
                                effectsObjectPrefabID = projectile.effectsObjectPrefabID,
                                position = hitInfo.point,
                                #if UNITY_2022_2_OR_NEWER
                                // ECS 1.0
                                rotation = rayLocalTransform.Rotation
                                #else
                                // ECS 0.51
                                rotation = rotation.Value
                                #endif                            
                            };

                            sscManager.InstantiateEffectsObject(ref ieParms);
                        }

                        // Mark this (projectile) entity to be destroyed
                        entitiesToDestroy.Add(chunkEntities[entityIndex]);
                    }
                    // Should this entity be despawned? Use and "else" so we don't try to destroy it twice
                    else
                    {
                        // We "could" create a job with a CommandBuffer which would queue all the DestroyEntity requests and then run
                        // it at the end of the job on the main thread, that just creates more overhead. It "might" be
                        // faster with Burst if there were a zillion projectiles but doing it directly on the main thread
                        // is simplier and probably just as performant.

                        // Is the projectile past it's use-by date?
                        if (projectile.despawnTimer + deltaTime > projectile.despawnTime)
                        {
                            entitiesToDestroy.Add(chunkEntities[entityIndex]);
                        }

                        // If the Entity is not being destroyed, we "could" update the despawnTimer here on the main thread
                        // using the following example code, however, we can do that more efficently in the projectileJob
                        // below. Example code: entityManager.SetComponentData(chunkEntities[entityIndex], projectile);
                    }
                    #endregion

                    #region Process Unity.Physics Raycast results
                    #if SSC_PHYSICS
                    if (numPhysicsRaycastHits > 0)
                    {
                        Unity.Physics.RaycastHit raycastHit = physicsraycastResults[projectile._tempIdx];

                        if (raycastHit.RigidBodyIndex >= 0 && raycastHit.RigidBodyIndex < numPhysicsBodies)
                        {
                            // If the surface normal vector has a length, we must have hit something
                            if (math.abs(math.lengthsq(raycastHit.SurfaceNormal)) > 0f)
                            {
                                //Debug.Log("[DEBUG] PhysicsHit: " + raycastHit.Position.ToString() + " RigidBodyIndex: " + raycastHit.RigidBodyIndex);

                                if (sscManager != null && projectile.effectsObjectPrefabID >= 0)
                                {
                                    InstantiateEffectsObjectParameters ieParms = new InstantiateEffectsObjectParameters
                                    {
                                        effectsObjectPrefabID = projectile.effectsObjectPrefabID,
                                        position = raycastHit.Position,
                                        rotation = rotation.Value
                                    };

                                    sscManager.InstantiateEffectsObject(ref ieParms);
                                }

                                // Mark this (projectile) entity to be destroyed
                                entitiesToDestroy.Add(chunkEntities[entityIndex]);

                                // Potentially destroy the object that was hit.
                                //entitiesToDestroy.Add(buildPhysicsWorld.PhysicsWorld.CollisionWorld.Bodies[raycastHit.RigidBodyIndex].Entity);
                            }
                        }
                    }
                    #endif
                    #endregion

                    // Increment the raycast index - we're doing things this way to make sure that
                    // in the second loop the indices all match up correctly
                    // TODO: Need to check that this works - it might not work if, for instance, the chunks
                    // get populated in a different order each time
                    raycastIndex++;
                }
            }

            // It is faster to destroy a native array of projectiles, than one at a time
            entityManager.DestroyEntity(entitiesToDestroy.AsArray());
            entitiesToDestroy.Clear();

            #endregion

            // Dispose of the native array
            if (raycastResults.IsCreated) { raycastResults.Dispose(); }
            if (raycastCommands.IsCreated) { raycastCommands.Dispose(); }
            if (chunks.IsCreated) { chunks.Dispose(); }

            #region Unity.Physics Raycast Cleanup
            #if SSC_PHYSICS
            if (physicsraycastResults.IsCreated)
            {
                physicsraycastResults.Dispose();
            }
            #endif
            #endregion

            #region Parallel Job to move the Projectiles
            // Create a new IJobChunk projectile move job, passing in the current frame time as
            // an argument along with the chunk component types.
            // Notice that we cannot used cached chunk component types.
            ProjectileMoveJob projectileMoveJob = new ProjectileMoveJob
            {
                deltaTime = deltaTime,

                #if UNITY_2022_2_OR_NEWER
                transformType = GetComponentTypeHandle<LocalTransform>(),
                projectileType = GetComponentTypeHandle<Projectile>()
                #elif UNITY_2020_1_OR_NEWER
                // Replace GetArchetypeChunkComponentType<T>() with GetComponentTypeHandle<T>() in Entities 0.12.0
                translationType = GetComponentTypeHandle<Translation>(),
                projectileType = GetComponentTypeHandle<Projectile>()
                #else
                translationType = GetArchetypeChunkComponentType<Translation>(),
                projectileType = GetArchetypeChunkComponentType<Projectile>()
                #endif
            };

            // Schedule the parallel projectile move job
            #if UNITY_2020_1_OR_NEWER
            this.Dependency = projectileMoveJob.ScheduleParallel(projectileEntityQuery, this.Dependency);
            #else
            return projectileMoveJob.Schedule(projectileEntityQuery, jobHandle);
            #endif

            #endregion
        }

        #endregion

        #region Private and Internal Methods

        /// <summary>
        /// Teleport (move) the projectiles a particular amount on x,y,z axes.
        /// </summary>
        /// <param name="deltaPosition"></param>
        internal void TelePortProjectiles(Vector3 deltaPosition)
        {
            // Create a new job
            ProjectileTelePortJob projectileTelePortJob = new ProjectileTelePortJob
            {
                deltaPosition = deltaPosition,

                #if UNITY_2022_2_OR_NEWER
                // ECS 1.0
                transformType = GetComponentTypeHandle<LocalTransform>()
                #elif UNITY_2020_1_OR_NEWER
                // Replace GetArchetypeChunkComponentType<T>() with GetComponentTypeHandle<T>() in Entities 0.12.0
                translationType = GetComponentTypeHandle<Translation>()
                #else
                translationType = GetArchetypeChunkComponentType<Translation>()
                #endif
            };

            // Schedule the parallel teleport job
            #if UNITY_2020_1_OR_NEWER
            this.Dependency = projectileTelePortJob.ScheduleParallel(projectileEntityQuery, this.Dependency);
            #else
            JobHandle jobHandle = projectileTelePortJob.Schedule(projectileEntityQuery);
            #endif
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Create a new projectile entity in the scene, based on a prefab that has already been converted
        /// from a gameobject prefab to an entity.
        /// Add a Projectile component if it wasn't already attached to the original gameobject prefab.
        /// Update the array of all projectile entities.
        /// It "might" be better to pass the ProjectModule instance reference which would simplify
        /// maintenance.
        /// NOTE: This method runs on the main thread.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="weaponVelocity"></param>
        /// <param name="startFwdDirection"></param>
        /// <param name="startUpDirection"></param>
        /// <param name="startSpeed"></param>
        /// <param name="deltaTime"></param>
        /// <param name="useGravity"></param>
        /// <param name="gravity"></param>
        /// <param name="gravityDirection"></param>
        /// <param name="damageAmount"></param>
        /// <param name="despawnTime"></param>
        /// <param name="projectilePrefabID"></param>
        /// <param name="effectsObjectPrefabID"></param>
        /// <param name="shieldEffectsObjectPrefabID"></param>
        /// <param name="shipId"></param>
        /// <param name="squadronId"></param>
        /// <param name="damageTypeInt"></param>
        /// <param name="sceneHandle"></param>
        /// <param name="projectilePrefabEntity"></param>        
        public static void CreateProjectile
        (
            Vector3 position,
            float3 weaponVelocity,
            float3 startFwdDirection,
            float3 startUpDirection,
            float startSpeed,
            float deltaTime,
            bool useGravity,
            float gravity,
            float3 gravityDirection,
            float damageAmount,
            float despawnTime,
            int projectilePrefabID,
            int effectsObjectPrefabID,
            int shieldEffectsObjectPrefabID,
            int shipId,
            int squadronId,
            int damageTypeInt,
            int sceneHandle,
            Entity projectilePrefabEntity
        )
        {
            // The prefab is converted to an entity when the ProjectileTemplate is created.
            // Translation, Rotation, and RenderMesh components are automatically added to the input projectPrefabEntity
            // when it is converted from the gameobject prefab. So, we don't need to add them here.

            Entity entity = entityManager.Instantiate(projectilePrefabEntity);

            // Add a Projectile component if it wasn't on the template prefab
            if (!entityManager.HasComponent(entity, compTypeProjectile))
            {
                entityManager.AddComponent(entity, compTypeProjectile);
            }

            //Debug.Log("[DEBUG] has projectilemodule: " + entityManager.HasComponent(entity, typeof(ProjectileModule)));
            //Debug.Log("[DEBUG] has RenderMesh: " + entityManager.HasComponent(entity, typeof(Unity.Rendering.RenderMesh)));

            float3 _velocity = startFwdDirection * startSpeed;

            // Set their worldspace position, rotation and projectile properties
            // Shift the position forward by the weapon velocity, so that projectiles don't ever end up behind the ship
            #if UNITY_2022_2_OR_NEWER
            // Set position from world origin
            LocalTransform localTrfm = entityManager.GetComponentData<LocalTransform>(entity);
            localTrfm.Position = (float3)position + (_velocity * deltaTime);
            localTrfm.Rotation = quaternion.LookRotation(startFwdDirection, startUpDirection);
            localTrfm.Scale = 1;
            entityManager.SetComponentData<LocalTransform>(entity, localTrfm);
            #else
            entityManager.SetComponentData(entity, new Translation() { Value = (float3)position + (_velocity * deltaTime) });
            entityManager.SetComponentData(entity, new Rotation() { Value = quaternion.LookRotation(startFwdDirection, startUpDirection) });
            #endif

            entityManager.SetComponentData(entity, new Projectile()
            {
                // Initialise the velocity based on the forwards direction
                // The forwards direction should have been set correctly prior to enabling the object
                velocity = _velocity + weaponVelocity,

                // Store the current speed and forwards direction incase we want to change
                // these if gravity is being applied to the projectile
                useGravity = useGravity ? (byte)1 : (byte)0,
                gravity = gravity,
                gravityDirection = gravityDirection,
                damageAmount = damageAmount,
                despawnTime = despawnTime,
                despawnTimer = 0f,
                speed = startSpeed,
                fwdDirection = startFwdDirection,
                upDirection = startUpDirection,
                projectilePrefabID = projectilePrefabID,
                effectsObjectPrefabID = effectsObjectPrefabID,
                shieldEffectsObjectPrefabID = shieldEffectsObjectPrefabID,
                sourceShipId = shipId,
                sourceSquadronId = squadronId,
                damageTypeInt = damageTypeInt,
                sceneHandle = sceneHandle,
                _tempIdx = -1
            }
            );
        }

        #endregion
    }

    #endregion
}
#endif
