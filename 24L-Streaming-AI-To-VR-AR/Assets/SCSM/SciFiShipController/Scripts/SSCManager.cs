using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if SSC_ENTITIES
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Rendering;
#if UNITY_2022_2_OR_NEWER
using Unity.Entities.Graphics;
#endif
#endif

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [AddComponentMenu("Sci-Fi Ship Controller/Managers/Ship Controller Manager")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SSCManager : MonoBehaviour
    {
        #region Public Variables and Properties

        /// <summary>
        /// [READONLY] Are all the projectiles in the scene currently paused?
        /// See also PauseProjectiles() and ResumeProjectiles()
        /// </summary>
        public bool IsProjectilesPaused { get { return isProjectilesPaused; } }

        /// <summary>
        /// [READONLY] Are all the beams in the scene currently paused?
        /// See also PauseBeams() and ResumeBeams()
        /// </summary>
        public bool IsBeamsPaused { get { return isBeamsPaused; } }

        /// <summary>
        /// [READONLY] Are all the destructs in the scene currently paused?
        /// See also PauseDestructs() and ResumeDestructs()
        /// </summary>
        public bool IsDestructsPaused { get { return isDestructsPaused; } }

        /// <summary>
        /// [READONLY] Are all the effects objects in the scene currently paused?
        /// See also PauseEffectsObjects() and ResumeEffectsObjects()
        /// </summary>
        public bool IsEffectsObjectsPaused { get { return isEffectsObjectsPaused; } }

        public List<ProjectileTemplate> ProjectileTemplatesList { get { return projectileTemplatesList; } }
        public List<BeamTemplate> BeamTemplatesList { get { return beamTemplateList; } }
        public List<DestructTemplate> DestructTemplatesList { get { return destructTemplateList; } }
        public List<EffectsObjectTemplate> EffectsObjectTemplatesList { get { return effectsObjectTemplatesList; } }

        // Path data variables
        public List<PathData> pathDataList;
        public bool isPathDataListExpanded;
        public int pathDataActiveGUIDHash = -1; // The active path (there can only be one active)
        public string editorSearchPathFilter;
        [Range(1f, 100f)] public float pathDisplayResolution = 10f; // Default to approx. 10 metre-segments
        [Range(0.05f, 1f)] public float pathPrecision = 0.5f;

        // Location data variables
        public List<LocationData> locationDataList;
        [Range(0f,50f)] public float locationDefaultNormalOffset = 1f;
        public bool isLocationDataListExpanded;
        public bool allowRepaint = false;
        [Range(1f,1000f)] public float findZoomDistance = 50f;
        public string editorSearchLocationFilter;

        // Options variables
        public bool isAutosizeLocationGizmo = true;
        public Color locationGizmoColour = new Color(1f, 0.92f, 0.016f, 0.6f);
        public Color defaultPathControlGizmoColour = new Color(1f, 0f, 0f, 0.8f);
        [Range(1f, 50f)] public float defaultPathControlOffset = 10f;

        /// <summary>
        /// [INTERNAL USE ONLY] Instead, call sscManager.EnableRadar() or DisableRadar().
        /// </summary>
        public bool isRadarEnabled;

        #endregion

        #region Public Static Variables
        /// <summary>
        /// Used to denote that a prefab is not pooled.
        /// See GetorCreateEffectsPool(..).
        /// </summary>
        public static int NoPrefabID = -1;

        #endregion

        #region Public Delegates

        /// <summary>
        /// Optional callback for use with kinematic guided projectiles. Stored in SSCManager to
        /// avoid having one delegate for every projectile in the scene.
        /// </summary>
        /// <param name="projectileModule"></param>
        public delegate void CallbackProjectileMoveTo(ProjectileModule projectileModule);

        /// <summary>
        /// The name of the custom method that is called when a kinematic guided projectile is being moved.
        /// Your method must take 1 parameter of type ProjectileModule. It MUST correctly update the Velocity
        /// property AND transform.position on the projectile. Optionally it can update tranform.rotation.
        /// This should be a lightweight method to avoid performance issues as it is typically called
        /// each FixedUpdate.
        /// </summary>
        public CallbackProjectileMoveTo callbackProjectileMoveTo = null;

        #endregion

        #region Private Variables

        private List<ProjectileTemplate> projectileTemplatesList;
        private ProjectileTemplate projectileTemplate;
        private GameObject projectileGameObjectInstance;

        private List<BeamTemplate> beamTemplateList;
        private BeamTemplate beamTemplate;
        private GameObject beamGameObjectInstance;

        private List<DestructTemplate> destructTemplateList;
        private DestructTemplate destructTemplate;
        private GameObject destructGameObjectInstance;

        private List<EffectsObjectTemplate> effectsObjectTemplatesList;
        private EffectsObjectTemplate effectsObjectTemplate;
        private GameObject effectsObjectGameObjectInstance;

        private static List<GameObject> tempGameObjectList = new List<GameObject>();
        private static List<SSCManager> managerList = new List<SSCManager>(2);
        private static int numberManagers = 0;
        //private static SSCManager currentManager = null;
        internal int sceneHandle = 0;
        private bool isInitialised = false;

        private Transform projectilePooledTrfm = null;
        private Transform projectileNonPooledTrfm = null;
        private Transform beamPooledTrfm = null;
        private Transform beamNonPooledTrfm = null;
        private Transform destructPooledTrfm = null;
        private Transform destructNonPooledTrfm = null;
        private Transform effectsPooledTrfm = null;
        private Transform effectsNonPooledTrfm = null;

        private bool isBeamsPaused = false;
        private bool isDestructsPaused = false;
        private bool isProjectilesPaused = false;
        private bool isEffectsObjectsPaused = false;

        /// <summary>
        /// This is used to get a quick estimate of a Path Section length
        /// or distance. See CalcPathSegments(..).
        /// </summary>
        private int numPathSegmentsForEstimate = 8;

        /// <summary>
        /// The Path segment length used for maximum accuracy or precision.
        /// </summary>
        private float maxPathSegmentPrecisionLength = 0.5f;

        private SSCRadar sscRadar = null;
     
        #endregion

        #if SSC_ENTITIES

        public static ProjectileSystem projectileSystem;
        public static EntityManager entityManager;
        public static World sscWorld;
        #if UNITY_2022_2_OR_NEWER
        // BlobAssetStore not required for ECS 1.0 Entity creation
        #elif UNITY_2019_3_OR_NEWER || UNITY_ENTITIES_0_2_0_OR_NEWER
        public static BlobAssetStore blobAssetStore;
        #endif

        /// <summary>
        /// This is only set when there are weapons that use DOTS-enabled projectile modules
        /// </summary>
        public bool isProjectileSytemUpdateRequired = false;

        #endif

        #region Public Methods

        /// <summary>
        /// Initialises the Ship Controller Manager instance.
        /// </summary>
        public void Initialise ()
        {
            // Initialise the projectile templates list with an initial capacity of 10 projectile types
            projectileTemplatesList = new List<ProjectileTemplate>(10);

            // Initialise the beam templates list with an initial capacity of 10 beam types
            beamTemplateList = new List<BeamTemplate>(10);

            // Initialise the destruct templates list with an initial capacity of 10 destruct types
            destructTemplateList = new List<DestructTemplate>(10);

            // Initialise the effects object templates list with an initial capacity of 25 effects object types
            effectsObjectTemplatesList = new List<EffectsObjectTemplate>(25);

            // AI data
            if (locationDataList == null) { locationDataList = new List<LocationData>(10); }
            if (pathDataList == null) { pathDataList = new List<PathData>(5); }

            // Beam child transforms
            beamPooledTrfm = SSCUtils.GetOrCreateChildTransform(transform, "BeamPooled");
            beamNonPooledTrfm = SSCUtils.GetOrCreateChildTransform(transform, "BeamNonPooled");

            // Destruct child transforms
            destructPooledTrfm = SSCUtils.GetOrCreateChildTransform(transform, "DestructPooled");
            destructNonPooledTrfm = SSCUtils.GetOrCreateChildTransform(transform, "DestructNonPooled");

            // Projectile child transforms
            projectilePooledTrfm = SSCUtils.GetOrCreateChildTransform(transform, "ProjectilePooled");
            projectileNonPooledTrfm = SSCUtils.GetOrCreateChildTransform(transform, "ProjectileNonPooled");

            // Effects object child transforms
            effectsPooledTrfm = SSCUtils.GetOrCreateChildTransform(transform, "EffectPooled");
            effectsNonPooledTrfm = SSCUtils.GetOrCreateChildTransform(transform, "EffectNonPooled");

            #if SSC_ENTITIES
            // NOTE: GetDefaultWorld() may fail in the editor if the scene hasn't been run in this session.
            if (Application.isPlaying)
            {
                // Get the world that will "hold" the entities.
                if (sscWorld == null)
                {
                    sscWorld = DOTSHelper.GetDefaultWorld();
                }

                #if UNITY_2022_2_OR_NEWER
                // blobAssetStore not required for ECS 1.0 Entity creation

                #elif UNITY_2019_3_OR_NEWER || UNITY_ENTITIES_0_2_0_OR_NEWER
                if (blobAssetStore == null)
                {
                    blobAssetStore = new BlobAssetStore();
                }
                #endif

                if (sscWorld == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("SSCManager.Initialise - could not get entities World. Please Play the scene once to fix. If problem persists PLEASE REPORT"); 
                    #endif
                    return;
                }
                #if UNITY_2022_2_OR_NEWER
                // ECS 1.0
                //else if (!blobAssetStore.IsCreated)
                //{
                //    #if UNITY_EDITOR
                //    Debug.LogWarning("SSCManager.Initialise - could not create BlobAssetStore - PLEASE REPORT"); 
                //    #endif
                //    return;
                //}
                #elif UNITY_2019_3_OR_NEWER || UNITY_ENTITIES_0_2_0_OR_NEWER
                // ECS 0.51
                else if (blobAssetStore == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("SSCManager.Initialise - could not create BlobAssetStore - PLEASE REPORT"); 
                    #endif
                    return;
                }
                #endif
                else
                {
                    // Create the ProjectileSystem manually so we can update it from FixedUpdate().
                    if (projectileSystem == null)
                    {
                        #if UNITY_2022_2_OR_NEWER
                        // ECS 1.0
                        projectileSystem = sscWorld.GetOrCreateSystemManaged<ProjectileSystem>();
                        #else
                        projectileSystem = sscWorld.GetOrCreateSystem<ProjectileSystem>();
                        // ECS 0.51
                        #endif
                    }
                    if (projectileSystem != null)
                    {
                        // Get the current entity manager. If it doesn't exist, create one.
                        entityManager = sscWorld.EntityManager;
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("SSCManager.Initialise - could not create ProjectileSystem() - PLEASE REPORT"); }
                    #endif
                }
            }
            #endif

            isInitialised = true;

            // Check to see if any locations are enabled for radar
            // NOTE: This must appear AFTER isInitialised = true.
            int numLocations = locationDataList == null ? 0 : locationDataList.Count;
            for (int lIdx = 0; lIdx < numLocations; lIdx++)
            {
                if (locationDataList[lIdx].isRadarEnabled)
                {
                    if (sscRadar == null) { sscRadar = SSCRadar.GetOrCreateRadar(); }
                    if (sscRadar != null) { EnableRadar(locationDataList[lIdx]); }
                }
            }
        }

        /// <summary>
        /// Updates the projectiles and effects objects lists in the Ship Controller Manager given a ship instance, and updates
        /// the IDs for: 
        /// - Each weapon's projectile
        /// - Each projectile's effects object
        /// - Each damage region's effects object
        /// </summary>
        /// <param name="weaponList"></param>
        public void UpdateProjectilesAndEffects (Ship shipInstance)
        {
            if (isInitialised)
            {
                if (shipInstance != null)
                {
                    if (shipInstance.weaponList != null)
                    {
                        // Loop through the list of weapons
                        int weaponListCount = shipInstance.weaponList.Count;
                        for (int w = 0; w < weaponListCount; w++)
                        {
                            Weapon weapon = shipInstance.weaponList[w];

                            // Only process projectile weapons
                            if (weapon.IsProjectileWeapon)
                            {
                                if (weapon.projectilePrefab != null)
                                {
                                    UpdateWeaponProjectileAndEffects(weapon);
                                }
                                #if UNITY_EDITOR
                                else
                                {
                                    // If the projectile prefab is null, log a warning
                                    Debug.LogWarning("SSCManager UpdateProjectilesAndEffects() Warning: Projectile for a weapon (" 
                                        + weapon.name + ") was null. " + "This weapon will not be able to fire projectiles.");
                                }
                                #endif
                            }
                        }
                    }
                    #if UNITY_EDITOR
                    else
                    {
                        // If the weapon list is null, log a warning
                        Debug.LogWarning("SSCManager UpdateProjectilesAndEffects() Warning: Weapon list was null.");
                    }
                    #endif

                    if (shipInstance.mainDamageRegion != null && shipInstance.localisedDamageRegionList != null)
                    {
                        int localisedDamageRegionListCount = shipInstance.localisedDamageRegionList.Count;

                        // Loop through all the damage regions of this ship
                        DamageRegion damageRegion;
                        for (int d = 0; d < localisedDamageRegionListCount + 1; d++)
                        {
                            if (d == 0) { damageRegion = shipInstance.mainDamageRegion; }
                            else { damageRegion = shipInstance.localisedDamageRegionList[d - 1]; }

                            // Check if the damage region has an effects object
                            if (damageRegion != null && damageRegion.destructionEffectsObject != null)
                            {
                                // Get the transform instance ID for this effects object prefab
                                int effectsObjectTransformID = damageRegion.destructionEffectsObject.transform.GetInstanceID();
                                // Search the effects object templates list to see if we already have an 
                                // effects object prefab with the same instance ID
                                int effectsObjectTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsObjectTransformID);

                                if (effectsObjectTemplateIndex == -1)
                                {
                                    // If no match was found, create a new effects object template for this prefab
                                    damageRegion.effectsObjectPrefabID = AddEffectsObjectTemplate(damageRegion.destructionEffectsObject, effectsObjectTransformID);
                                }
                                else
                                {
                                    // Save the effect template index in the damage region
                                    damageRegion.effectsObjectPrefabID = effectsObjectTemplateIndex;
                                }
                            }
                        }
                    }
                    #if UNITY_EDITOR
                    else
                    {
                        // If the weapon list is null, log a warning
                        Debug.LogWarning("SSCManager UpdateProjectilesAndEffects() Warning: main damage region or localised damage region list is null");
                    }
                    #endif
                }
                #if UNITY_EDITOR
                else
                {
                    // If any of the damage region variables are null, log a warning
                    Debug.LogWarning("SSCManager UpdateProjectilesAndEffects() Warning: shipInstance is null.");
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("SSCManager UpdateProjectilesAndEffects() Warning: Method was called before the manager was initialised.");
            }
            #endif
        }

        /// <summary>
        /// Updates the beams and effects objects lists in the Ship Controller Manager given a ship instance, and updates
        /// the ids for:
        /// - Each weapon's beam
        /// - Each beam's effects object
        /// </summary>
        /// <param name="shipInstance"></param>
        public void UpdateBeamsAndEffects (Ship shipInstance)
        {
            if (isInitialised)
            {
                if (shipInstance != null)
                {
                    if (shipInstance.weaponList != null)
                    {
                        // Loop through the list of weapons
                        int weaponListCount = shipInstance.weaponList.Count;

                        for (int w = 0; w < weaponListCount; w++)
                        {
                            Weapon weapon = shipInstance.weaponList[w];

                            // Only process beam weapons
                            if (weapon.IsBeamWeapon)
                            {
                                if (weapon.beamPrefab != null)
                                {
                                    UpdateWeaponBeamAndEffects(weapon);
                                }
                                #if UNITY_EDITOR
                                else
                                {
                                    // If the beam prefab is null, log a warning
                                    Debug.LogWarning("SSCManager UpdateBeamsAndEffects() Warning: Beam for a weapon ("
                                        + weapon.name + ") was null. " + "This weapon will not be able to fire beams.");
                                }
                                #endif
                            }
                        }
                    }
                    #if UNITY_EDITOR
                    else
                    {
                        // If the weapon list is null, log a warning
                        Debug.LogWarning("SSCManager UpdateBeamsAndEffects() Warning: Weapon list was null.");
                    }
                    #endif
                }
                #if UNITY_EDITOR
                else
                {
                    // If any of the damage region variables are null, log a warning
                    Debug.LogWarning("SSCManager UpdateBeamsAndEffects() Warning: ship instance is null");
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("SSCManager UpdateBeamsAndEffects() Warning: Method was called before the manager was initialised.");
            }
            #endif
        }

        /// <summary>
        /// Updates the beams and effects objects lists in the Ship Controller Manager given a surface turret instance, and updates
        /// the ids for:
        /// - The weapon's beam
        /// - Each beam's effects object
        /// </summary>
        /// <param name="surfaceTurretModule"></param>
        public void UpdateBeamsAndEffects (SurfaceTurretModule surfaceTurretModule)
        {
            if (isInitialised)
            {
                if (surfaceTurretModule != null)
                {
                    if (surfaceTurretModule.weapon != null)
                    {
                        // Only process beam weapons
                        if (surfaceTurretModule.weapon.IsBeamWeapon)
                        {
                            if (surfaceTurretModule.weapon.beamPrefab != null)
                            {
                                UpdateWeaponBeamAndEffects(surfaceTurretModule.weapon);
                            }
                            #if UNITY_EDITOR
                            else
                            {
                                // If the beam prefab is null, log a warning
                                Debug.LogWarning("SSCManager UpdateBeamsAndEffects() Warning: Beam for a weapon ("
                                    + surfaceTurretModule.weapon.name + ") was null. " + "This weapon will not be able to fire beams.");
                            }
                            #endif
                        }
                    }
                    #if UNITY_EDITOR
                    else
                    {
                        // If the weapon is null, log an error
                        Debug.LogWarning("SSCManager UpdateBeamsAndEffects() ERROR: SurfaceTurretModule Weapon was null.");
                    }
                    #endif
                }
                #if UNITY_EDITOR
                else
                {
                    // If surfaceTurretModule variables is null, log a warning
                    Debug.LogWarning("SSCManager UpdateBeamsAndEffects() Warning: surfaceTurretModule is null");
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("SSCManager UpdateBeamsAndEffects() Warning: Method was called before the manager was initialised.");
            }
            #endif
        }

        /// <summary>
        /// Updates the DestructModule objects lists in the Ship Controller Manager given a Ship instance
        /// </summary>
        /// <param name="surfaceTurretModule"></param>
        public void UpdateDestructObjects(Ship shipInstance)
        {
            if (isInitialised)
            {
                if (shipInstance != null)
                {
                    if (shipInstance.mainDamageRegion != null && shipInstance.localisedDamageRegionList != null)
                    {
                        int localisedDamageRegionListCount = shipInstance.localisedDamageRegionList.Count;

                        // Loop through all the damage regions of this ship
                        DamageRegion damageRegion;
                        for (int d = 0; d < localisedDamageRegionListCount + 1; d++)
                        {
                            if (d == 0) { damageRegion = shipInstance.mainDamageRegion; }
                            else { damageRegion = shipInstance.localisedDamageRegionList[d - 1]; }

                            // Check if the damage region has a destruct object
                            if (damageRegion != null && damageRegion.destructObject != null)
                            {
                                if (damageRegion.destructObject != null)
                                {
                                    // Get the transform instance ID for this destruct prefab
                                    int destructTransformID = damageRegion.destructObject.transform.GetInstanceID();
                                    // Search the destruct templates list to see if we already have a destruct prefab with the same instance ID
                                    int destructTemplateIndex = destructTemplateList.FindIndex(e => e.instanceID == destructTransformID);

                                    if (destructTemplateIndex == -1)
                                    {
                                        // If no match was found, create a new destruct template for this prefab
                                        damageRegion.destructObjectPrefabID = AddDestructTemplate(damageRegion.destructObject, destructTransformID);
                                    }
                                    else
                                    {
                                        // Save the destruct template index in the surface turret
                                        damageRegion.destructObjectPrefabID = destructTemplateIndex;
                                    }
                                }
                                else { damageRegion.destructObjectPrefabID = -1; }

                                damageRegion.isDestructObjectActivated = false;
                            }
                        }
                    }
                    #if UNITY_EDITOR
                    else
                    {
                        // If the weapon list is null, log a warning
                        Debug.LogWarning("SSCManager UpdateDestructObjects(shipInstance) Warning: main damage region or localised damage region list is null");
                    }
                    #endif
                }
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("SSCManager UpdateDestructObjects(shipInstance) Warning: Method was called before the manager was initialised.");
            }
            #endif
        }

        /// <summary>
        /// Updates the DestructModule objects lists in the Ship Controller Manager given a SurfaceTurretModule instance
        /// </summary>
        /// <param name="surfaceTurretModule"></param>
        public void UpdateDestructObjects(SurfaceTurretModule surfaceTurretModule)
        {
            if (isInitialised)
            {
                if (surfaceTurretModule.destructObject != null)
                {
                    // Get the transform instance ID for this destruct prefab
                    int destructTransformID = surfaceTurretModule.destructObject.transform.GetInstanceID();
                    // Search the destruct templates list to see if we already have a destruct prefab with the same instance ID
                    int destructTemplateIndex = destructTemplateList.FindIndex(e => e.instanceID == destructTransformID);

                    if (destructTemplateIndex == -1)
                    {
                        // If no match was found, create a new destruct template for this prefab
                        surfaceTurretModule.destructObjectPrefabID = AddDestructTemplate(surfaceTurretModule.destructObject, destructTransformID);
                    }
                    else
                    {
                        // Save the destruct template index in the surface turret
                        surfaceTurretModule.destructObjectPrefabID = destructTemplateIndex;
                    }
                }
                else { surfaceTurretModule.destructObjectPrefabID = -1; }

                surfaceTurretModule.isDestructObjectActivated = false;
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("SSCManager UpdateDestructObjects(SurfaceTurretModule surfaceTurretModule) Warning: Method was called before the manager was initialised.");
            }
            #endif
        }

        /// <summary>
        /// Updates the DestructModule objects lists in the Ship Controller Manager given a DestructibleObjectModule instance
        /// </summary>
        /// <param name="destructibleObjectModule"></param>
        public void UpdateDestructObjects(DestructibleObjectModule destructibleObjectModule)
        {
            if (isInitialised)
            {
                if (destructibleObjectModule.destructObject != null)
                {
                    // Get the transform instance ID for this destruct prefab
                    int destructTransformID = destructibleObjectModule.destructObject.transform.GetInstanceID();
                    // Search the destruct templates list to see if we already have a destruct prefab with the same instance ID
                    int destructTemplateIndex = destructTemplateList.FindIndex(e => e.instanceID == destructTransformID);

                    if (destructTemplateIndex == -1)
                    {
                        // If no match was found, create a new destruct template for this prefab
                        destructibleObjectModule.destructObjectPrefabID = AddDestructTemplate(destructibleObjectModule.destructObject, destructTransformID);
                    }
                    else
                    {
                        // Save the destruct template index in the surface turret
                        destructibleObjectModule.destructObjectPrefabID = destructTemplateIndex;
                    }
                }
                else { destructibleObjectModule.destructObjectPrefabID = -1; }

                destructibleObjectModule.isDestructObjectActivated = false;
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("SSCManager UpdateDestructObjects(DestructibleObjectModule destructibleObjectModule) Warning: Method was called before the manager was initialised.");
            }
            #endif
        }

        /// <summary>
        /// Updates the projectiles and effects objects lists in the Ship Controller Manager given a SurfaceTurretModule instance,
        /// and updates the IDs for: 
        /// - The weapon's projectile
        /// - The projectile's effects object
        /// - The damage effects object
        /// </summary>
        /// <param name="surfaceTurretModule"></param>
        public void UpdateProjectilesAndEffects (SurfaceTurretModule surfaceTurretModule)
        {
            if (isInitialised)
            {
                if (surfaceTurretModule != null && surfaceTurretModule.weapon != null)
                {
                    // Update projectile weapon (if used)
                    if (surfaceTurretModule.weapon.IsProjectileWeapon)
                    {
                        if (surfaceTurretModule.weapon.projectilePrefab != null)
                        {
                            UpdateWeaponProjectileAndEffects(surfaceTurretModule.weapon);
                        }
                        #if UNITY_EDITOR
                        else
                        {
                            // If the projectile prefab is null, log a warning
                            Debug.LogWarning("SSCManager UpdateProjectilesAndEffects() Warning: Projectile for a surfaceTurretModule ("
                                + surfaceTurretModule.name + ") was null. " + "This weapon will not be able to fire projectiles.");
                        }
                        #endif
                    }

                    if (surfaceTurretModule.destructionEffectsObject != null)
                    {
                        // Get the transform instance ID for this effects object prefab
                        int effectsObjectTransformID = surfaceTurretModule.destructionEffectsObject.transform.GetInstanceID();
                        // Search the effects object templates list to see if we already have an 
                        // effects object prefab with the same instance ID
                        int effectsObjectTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsObjectTransformID);

                        if (effectsObjectTemplateIndex == -1)
                        {
                            // If no match was found, create a new effects object template for this prefab
                            surfaceTurretModule.effectsObjectPrefabID = AddEffectsObjectTemplate(surfaceTurretModule.destructionEffectsObject, effectsObjectTransformID);
                        }
                        else
                        {
                            // Save the effect template index in the surface turret
                            surfaceTurretModule.effectsObjectPrefabID = effectsObjectTemplateIndex;
                        }
                    }
                    else { surfaceTurretModule.effectsObjectPrefabID = -1; }

                    surfaceTurretModule.isDestructionEffectsObjectInstantiated = false;
                }
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("SSCManager UpdateProjectilesAndEffects(SurfaceTurretModule surfaceTurretModule) Warning: Method was called before the manager was initialised.");
            }
            #endif
        }

        /// <summary>
        /// Updates the effects objects lists in the Ship Controller Manager given a DestructibleObjectModule instance,
        /// and updates the IDs for: 
        /// - The damage effects object
        /// </summary>
        /// <param name="destructibleObjectModule"></param>
        public void UpdateEffects (DestructibleObjectModule destructibleObjectModule)
        {
            if (isInitialised)
            {
                if (destructibleObjectModule != null)
                {
                    if (destructibleObjectModule.destructionEffectsObject != null)
                    {
                        // Get the transform instance ID for this effects object prefab
                        int effectsObjectTransformID = destructibleObjectModule.destructionEffectsObject.transform.GetInstanceID();
                        // Search the effects object templates list to see if we already have an 
                        // effects object prefab with the same instance ID
                        int effectsObjectTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsObjectTransformID);

                        if (effectsObjectTemplateIndex == -1)
                        {
                            // If no match was found, create a new effects object template for this prefab
                            destructibleObjectModule.effectsObjectPrefabID = AddEffectsObjectTemplate(destructibleObjectModule.destructionEffectsObject, effectsObjectTransformID);
                        }
                        else
                        {
                            // Save the effect template index in the surface turret
                            destructibleObjectModule.effectsObjectPrefabID = effectsObjectTemplateIndex;
                        }
                    }
                    else { destructibleObjectModule.effectsObjectPrefabID = -1; }

                    destructibleObjectModule.isDestructionEffectsObjectInstantiated = false;
                }
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("SSCManager UpdateEffects(DestructibleObjectModule destructibleObjectModule) Warning: Method was called before the manager was initialised.");
            }
            #endif
        }

        /// <summary>
        /// Instantiates the beam with ID beamPrefabID at the position specified in world space and with the forwards
        /// and up directions specified.
        /// (ibParms.beamPrefabID is the ID sent back to each weapon after calling the UpdateBeamsAndEffects() method).
        /// ibParms.effectsObjectPrefabID is ignored as it is looked up in this method.
        /// </summary>
        /// <param name="ibParms"></param>
        public BeamModule InstantiateBeam(ref InstantiateBeamParameters ibParms)
        {
            BeamModule beamModule = null;

            if (isInitialised)
            {
                if (ibParms.beamPrefabID >= 0 && ibParms.beamPrefabID < beamTemplateList.Count)
                {
                    // Get the beam template using its ID (this is simply the index of it in the beam template list)
                    beamTemplate = beamTemplateList[ibParms.beamPrefabID];

                    // Get the effects prefab ID. This is the index in list of effect templates.
                    ibParms.effectsObjectPrefabID = beamTemplate.beamPrefab.effectsObjectPrefabID;

                    if (beamTemplate.beamPrefab.usePooling)
                    {
                        // If we are using pooling, find the first inactive beam
                        if (beamTemplate.beamPoolList == null)
                        {
                            #if UNITY_EDITOR
                            Debug.LogWarning("SSCManager InstantiateBeam() beamPoolList is null. We do not support changing to usePooling for a beam at runtime.");
                            #endif
                        }
                        else
                        {
                            int firstInactiveIndex = beamTemplate.beamPoolList.FindIndex(p => !p.activeInHierarchy);
                            if (firstInactiveIndex == -1)
                            {
                                // All beams in the pool are currently active
                                // So if we want to get a new one, we'll need to add a new one to the pool
                                // First check if we have reached the max pool size
                                if (beamTemplate.currentPoolSize < beamTemplate.beamPrefab.maxPoolSize)
                                {
                                    // If we are still below the max pool size, add a new beam instance to the pool
                                    // Instantiate the beam object with the correct position and rotation
                                    beamGameObjectInstance = Instantiate(beamTemplate.beamPrefab.gameObject,
                                        ibParms.position, Quaternion.LookRotation(ibParms.fwdDirection, ibParms.upDirection));
                                    // Set the object's parent to be the manager
                                    beamGameObjectInstance.transform.SetParent(beamPooledTrfm);
                                    // Initialise the beam
                                    beamModule = beamGameObjectInstance.GetComponent<BeamModule>();
                                    // Remember which scene this beam instance is being created in
                                    beamModule.sceneHandle = sceneHandle;
                                    ibParms.beamSequenceNumber = beamModule.InitialiseBeam(ibParms);
                                    // Add the object to the list of pooled objects
                                    beamTemplate.beamPoolList.Add(beamGameObjectInstance);
                                    // Set the beam id to the last index position in the list
                                    ibParms.beamPoolListIndex = beamTemplate.currentPoolSize;
                                    // Update the current pool size counter
                                    beamTemplate.currentPoolSize++;
                                }
                            }
                            else
                            {
                                // Get the beam object
                                beamGameObjectInstance = beamTemplate.beamPoolList[firstInactiveIndex];
                                // Position the object
                                beamGameObjectInstance.transform.SetPositionAndRotation(ibParms.position, Quaternion.LookRotation(ibParms.fwdDirection, ibParms.upDirection));
                                // Set the object to active
                                beamGameObjectInstance.SetActive(true);
                                // Initialise the beam
                                beamModule = beamGameObjectInstance.GetComponent<BeamModule>();
                                ibParms.beamSequenceNumber = beamModule.InitialiseBeam(ibParms);
                                ibParms.beamPoolListIndex = firstInactiveIndex;
                            }
                        }
                    }
                    else
                    {
                        // If we are not using pooling, simply instantiate the beam
                        beamGameObjectInstance = Instantiate(beamTemplate.beamPrefab.gameObject,
                                    ibParms.position, Quaternion.LookRotation(ibParms.fwdDirection, ibParms.upDirection));
                        // Set the object's parent to be the manager
                        beamGameObjectInstance.transform.SetParent(beamNonPooledTrfm);
                        // Initialise the beam
                        beamModule = beamGameObjectInstance.GetComponent<BeamModule>();
                        // Remember which scene this beam instance is being created in
                        beamModule.sceneHandle = sceneHandle;
                        ibParms.beamSequenceNumber = beamModule.InitialiseBeam(ibParms);
                        // We are not using pooling so reset the index to -1.
                        ibParms.beamPoolListIndex = -1;
                    }

                    // Spawn a muzzle effects object if there is one
                    // NOTE: Currently it doesn't move with ships
                    if (beamTemplate.beamPrefab.muzzleEffectsObjectPrefabID >= 0)
                    {
                        Quaternion _objRotation = Quaternion.LookRotation(ibParms.fwdDirection, ibParms.upDirection);

                        InstantiateEffectsObjectParameters ieParms = new InstantiateEffectsObjectParameters
                        {
                            effectsObjectPrefabID = beamTemplate.beamPrefab.muzzleEffectsObjectPrefabID,
                            position = ibParms.position + (_objRotation * beamTemplate.beamPrefab.muzzleEffectsOffset),
                            rotation = _objRotation
                        };

                        // Instantiate the muzzle effects
                        if (InstantiateEffectsObject(ref ieParms) != null)
                        {
                            if (ieParms.effectsObjectSequenceNumber > 0)
                            {
                                // Record the muzzle effect item key
                                beamModule.muzzleEffectsItemKey = new SSCEffectItemKey(ieParms.effectsObjectPrefabID, ieParms.effectsObjectPoolListIndex, ieParms.effectsObjectSequenceNumber);
                            }
                        }
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    // If an invalid ID is provided, log a warning
                    Debug.LogWarning("SSCManager InstantiateBeam() Warning: Provided beamPrefabID was invalid. " +
                        "To get the correct ID, call UpdateBeamsAndEffects() for this ship or surface turret, and use the " +
                        "beamPrefabID populated for each weapon.");
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("SSCManager InstantiateBeam() Warning: Method was called before the manager was initialised.");
            }
            #endif

            return beamModule;
        }

        /// <summary>
        /// Instantiates the destruct object with ID destructPrefabID at the position specified in world space
        /// (destructPrefabID is the ID sent back to each SurfaceTurret or Ship after calling the UpdateProjectilesAndDestruct() method).
        /// If non-pooled and isExplodeOnStart is true, the power and radius will always be the prefab defaults.
        /// </summary>
        /// <param name="projectilePrefabID"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public DestructModule InstantiateDestruct (ref InstantiateDestructParameters dstParms)
        {
            DestructModule _destructModule = null;

            if (isInitialised)
            {
                // Check that a valid ID has been provided
                if (dstParms.destructPrefabID >= 0 && dstParms.destructPrefabID < destructTemplateList.Count)
                {
                    // Get the destruct object template using its ID (this is just the index of it in the destruct object template list)
                    destructTemplate = destructTemplateList[dstParms.destructPrefabID];

                    // Currently destruct objects are only instantiated as normal Unity gameobjects with optional pooling
                    if (destructTemplate.destructPrefab.usePooling)
                    {
                        // If we are using pooling, find the first inactive destruct object
                        if (destructTemplate.destructPoolList == null)
                        {
                            #if UNITY_EDITOR
                            Debug.LogWarning("SSCManager InstantiateDestruct() destructPoolList is null. We do not support changing to usePooling for an destruct object at runtime.");
                            #endif
                        }
                        else
                        {
                            int firstInactiveIndex = destructTemplate.destructPoolList.FindIndex(e => !e.activeInHierarchy);
                            if (firstInactiveIndex == -1)
                            {
                                // All destruct objects in the pool are currently active
                                // So if we want to get a new one, we'll need to add a new one to the pool
                                // First check if we have reached the max pool size
                                if (destructTemplate.currentPoolSize < destructTemplate.destructPrefab.maxPoolSize)
                                {
                                    // Always disable isExplodeOnStart when pooling is enabled
                                    destructTemplate.destructPrefab.isExplodeOnStart = false;

                                    // If we are still below the max pool size, add a new destruct object instance to the pool
                                    // Instantiate the destruct object with the correct position and rotation
                                    destructGameObjectInstance = Instantiate(destructTemplate.destructPrefab.gameObject,
                                        dstParms.position, dstParms.rotation);
                                    // Set the object's parent to be the manager
                                    destructGameObjectInstance.transform.SetParent(destructPooledTrfm);
                                    // Initialise the destruct object
                                    _destructModule = destructGameObjectInstance.GetComponent<DestructModule>();

                                    if (_destructModule.InitialiseDestruct())
                                    {
                                        dstParms.destructSequenceNumber = _destructModule.ActivateModule(destructTemplate.currentPoolSize);
                                        dstParms.destructPoolListIndex = destructTemplate.currentPoolSize;
                                        // Add the object to the list of pooled objects
                                        destructTemplate.destructPoolList.Add(destructGameObjectInstance);
                                        // Update the current pool size counter
                                        destructTemplate.currentPoolSize++;
                                        _destructModule.Explode(dstParms);
                                    }
                                }
                            }
                            else
                            {
                                // Get the destruct object
                                destructGameObjectInstance = destructTemplate.destructPoolList[firstInactiveIndex];
                                // Position the object
                                destructGameObjectInstance.transform.SetPositionAndRotation(dstParms.position, dstParms.rotation);
                                // Set the object to active
                                destructGameObjectInstance.SetActive(true);
                                // Initialise the destruct object
                                _destructModule = destructGameObjectInstance.GetComponent<DestructModule>();

                                if (_destructModule.InitialiseDestruct())
                                {
                                    dstParms.destructSequenceNumber = _destructModule.ActivateModule(firstInactiveIndex);
                                    dstParms.destructPoolListIndex = firstInactiveIndex;
                                    _destructModule.Explode(dstParms);
                                }
                            }
                        }
                    }
                    else
                    {
                        // If we are not using pooling, simply instantiate the effect
                        destructGameObjectInstance = Instantiate(destructTemplate.destructPrefab.gameObject,
                                    dstParms.position, dstParms.rotation);
                        // Set the object's parent to be the manager
                        destructGameObjectInstance.transform.SetParent(destructNonPooledTrfm);
                        _destructModule = destructGameObjectInstance.GetComponent<DestructModule>();

                        // Custom settings is dstParms will only be used if isExplodeOnStart not enabled.
                        if (!_destructModule.isExplodeOnStart)
                        {
                            // Initialise the destruct object                          
                            if (_destructModule.InitialiseDestruct())
                            {
                                dstParms.destructSequenceNumber = _destructModule.ActivateModule(-1);
                                dstParms.destructPoolListIndex = -1;
                                _destructModule.Explode(dstParms);
                            }
                        }
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    // If an invalid ID is provided, log a warning
                    Debug.LogWarning("SSCManager InstantiateDestruct() Warning: Provided destructPrefabID was invalid (" + dstParms.destructPrefabID +
                        "). To get the correct ID, call UpdateProjectilesAndDestruct(..) for the ship or SurfaceTurret, and use the " +
                        "destructPrefabID populated for each projectile / damage region.");
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("SSCManager InstantiateDestruct() Warning: Method was called before the manager was initialised.");
            }
            #endif

            return _destructModule;
        }

        [System.Obsolete("This method will be removed in a future version. Please use InstantiateProjectile (InstantiateProjectileParameters ipParms).")]
        public void InstantiateProjectile(int projectilePrefabID, Vector3 position, Vector3 fwdDirection, Vector3 upDirection, Vector3 weaponVelocity, float gravity, Vector3 gravityDirection, int shipId, int squadronId)
        {
            InstantiateProjectileParameters ipParms = new InstantiateProjectileParameters()
            {
                projectilePrefabID = projectilePrefabID,
                position = position,
                fwdDirection = fwdDirection,
                upDirection = upDirection,
                weaponVelocity = weaponVelocity,
                gravity = gravity,
                gravityDirection = gravityDirection,
                shipId = shipId,
                squadronId = squadronId
            };

            InstantiateProjectile(ref ipParms);
        }

        /// <summary>
        /// Instantiates the projectile with ID projectilePrefabID at the position specified in world space and with the forwards
        /// and up directions specified
        /// (ipParms.projectilePrefabID is the ID sent back to each weapon after calling the UpdateProjectilesAndEffects() method).
        /// ipParms.effectsObjectPrefabID is ignored as it is looked up in this method.
        /// </summary>
        /// <param name="ipParms"></param>
        public void InstantiateProjectile (ref InstantiateProjectileParameters ipParms)
        {
            if (isInitialised)
            {
                // Muzzle FX hasn't been spawned yet.
                ipParms.muzzleEffectsObjectPrefabID = -1;
                ipParms.muzzleEffectsObjectPoolListIndex = -1;                

                // Check that a valid ID has been provided
                if (ipParms.projectilePrefabID >= 0 && ipParms.projectilePrefabID < projectileTemplatesList.Count)
                {
                    // Get the projectile template using its ID (this is simply the index of it in the projectile template list)
                    projectileTemplate = projectileTemplatesList[ipParms.projectilePrefabID];

                    // Get the regular destruct effects prefab ID. This is the index in list of effect templates.
                    ipParms.effectsObjectPrefabID = projectileTemplate.projectilePrefab.effectsObjectPrefabID;

                    // Get the shield hit destruct effects prefab ID. This is the index in list of effect templates.
                    ipParms.shieldEffectsObjectPrefabID = projectileTemplate.projectilePrefab.shieldEffectsObjectPrefabID;

                    // Next, check how we need to instantiate this projectile
                    // DOTS/ECS Implementation
                    /// TODO - DOTS include shieldEffectsObjectPrefabID
                    if (projectileTemplate.projectilePrefab.useECS)
                    {
                        #if SSC_ENTITIES
                        // Now spawn a projectile entity at the player's position, with some forward velocity
                        ProjectileSystem.CreateProjectile
                        (
                            ipParms.position,
                            new float3(ipParms.weaponVelocity),
                            new float3(ipParms.fwdDirection),
                            new float3(ipParms.upDirection),                          
                            projectileTemplate.projectilePrefab.startSpeed,
                            Time.fixedDeltaTime,
                            projectileTemplate.projectilePrefab.useGravity,
                            ipParms.gravity,
                            new float3(ipParms.gravityDirection),
                            projectileTemplate.projectilePrefab.damageAmount,
                            projectileTemplate.projectilePrefab.despawnTime,
                            projectileTemplate.projectilePrefab.projectilePrefabID,
                            ipParms.effectsObjectPrefabID,
                            ipParms.shieldEffectsObjectPrefabID,
                            ipParms.shipId,
                            ipParms.squadronId,
                            (int)projectileTemplate.projectilePrefab.damageType,
                            sceneHandle,
                            projectileTemplate.projectilePrefabEntity
                        );
                        #endif
                    }
                    else if (projectileTemplate.projectilePrefab.usePooling)
                    {
                        // If we are using pooling, find the first inactive projectile
                        if (projectileTemplate.projectilePool == null)
                        {
                            #if UNITY_EDITOR
                            Debug.LogWarning("SSCManager InstantiateProjectile() projectilePool is null. We do not support changing to usePooling for a projectile at runtime.");
                            #endif
                        }
                        else
                        {                           
                            int firstInactiveIndex = projectileTemplate.projectilePool.FindIndex(p => !p.activeInHierarchy);
                            if (firstInactiveIndex == -1)
                            {
                                // All projectiles in the pool are currently active
                                // So if we want to get a new one, we'll need to add a new one to the pool
                                // First check if we have reached the max pool size
                                if (projectileTemplate.currentPoolSize < projectileTemplate.projectilePrefab.maxPoolSize)
                                {
                                    // If we are still below the max pool size, add a new projectile instance to the pool
                                    // Instantiate the projectile object with the correct position and rotation
                                    projectileGameObjectInstance = Instantiate(projectileTemplate.projectilePrefab.gameObject,
                                        ipParms.position, Quaternion.LookRotation(ipParms.fwdDirection, ipParms.upDirection));
                                    // Set the object's parent to be the manager
                                    projectileGameObjectInstance.transform.SetParent(projectilePooledTrfm);
                                    ProjectileModule _projectile = projectileGameObjectInstance.GetComponent<ProjectileModule>();
                                    // Remember which scene this projectile instance is being created in
                                    _projectile.sceneHandle = sceneHandle;
                                    // Initialise the projectile                                  
                                    _projectile.InitialiseProjectile(ipParms);
                                    // Add the object to the list of pooled objects
                                    projectileTemplate.projectilePool.Add(projectileGameObjectInstance);
                                    // Update the current pool size counter
                                    projectileTemplate.currentPoolSize++;
                                }
                            }
                            else
                            {
                                // Get the projectile object
                                projectileGameObjectInstance = projectileTemplate.projectilePool[firstInactiveIndex];
                                // Position the object
                                projectileGameObjectInstance.transform.SetPositionAndRotation(ipParms.position, Quaternion.LookRotation(ipParms.fwdDirection, ipParms.upDirection));
                                // Set the object to active
                                projectileGameObjectInstance.SetActive(true);
                                // Initialise the projectile
                                projectileGameObjectInstance.GetComponent<ProjectileModule>().InitialiseProjectile(ipParms);
                            }
                        }
                    }
                    else
                    {
                        // If we are not using DOTS or pooling, simply instantiate the projectile
                        projectileGameObjectInstance = Instantiate(projectileTemplate.projectilePrefab.gameObject,
                                    ipParms.position, Quaternion.LookRotation(ipParms.fwdDirection, ipParms.upDirection));
                        // Set the object's parent to be the manager
                        projectileGameObjectInstance.transform.SetParent(projectileNonPooledTrfm);

                        ProjectileModule _projectile = projectileGameObjectInstance.GetComponent<ProjectileModule>();
                        // Remember which scene this projectile instance is being created in
                        _projectile.sceneHandle = sceneHandle;
                        // Initialise the projectile                                  
                        _projectile.InitialiseProjectile(ipParms);
                    }

                    // Spawn a muzzle effects object if there is one
                    // NOTE: Currently it doesn't move with the ship
                    // TODO - add the muzzle ieParms.effectsObjectPoolListIndex to ipParms
                    // Set ipParms as ref so we can parent the FX to the weapon. It will only work for pooled muzzle fx.
                    if (projectileTemplate.projectilePrefab.muzzleEffectsObjectPrefabID >= 0)
                    {
                        Quaternion _objRotation = Quaternion.LookRotation(ipParms.fwdDirection, ipParms.upDirection);

                        InstantiateEffectsObjectParameters ieParms = new InstantiateEffectsObjectParameters
                        {
                            effectsObjectPrefabID = projectileTemplate.projectilePrefab.muzzleEffectsObjectPrefabID,
                            position = ipParms.position + (_objRotation * projectileTemplate.projectilePrefab.muzzleEffectsOffset),
                            rotation = _objRotation
                        };

                        // For projectiles we don't need to get the effectsObject key from ieParms.
                        InstantiateEffectsObject(ref ieParms);

                        // Return the spawned muzzle FX back to the caller so it can be parented (if required).
                        // If it is not spawned or is not pooled, the value will be -1.
                        // Need both the (template) PrefabID and the index in the pool.
                        ipParms.muzzleEffectsObjectPrefabID = ieParms.effectsObjectPrefabID;
                        ipParms.muzzleEffectsObjectPoolListIndex = ieParms.effectsObjectPoolListIndex;                        
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    // If an invalid ID is provided, log a warning
                    Debug.LogWarning("SSCManager InstantiateProjectile() Warning: Provided projectilePrefabID was invalid. " +
                        "To get the correct ID, call UpdateProjectilesAndEffects() for this ship, and use the " +
                        "projectilePrefabID populated for each weapon.");
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("SSCManager InstantiateProjectile() Warning: Method was called before the manager was initialised.");
            }
            #endif
        }

        /// <summary>
        /// Instantiates the effects object with ID effectsObjectPrefabID at the position specified in world space
        /// (effectsObjectPrefabID is the ID sent back to each projectile / damage region after calling the UpdateProjectilesAndEffects() method).
        /// </summary>
        /// <param name="effectsObjectPrefabID"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        [System.Obsolete("This method will be removed in a future version. Please use InstantiateEffectsObject (InstantiateEffectsObjectParameters ieParms).")]
        public void InstantiateEffectsObject(int effectsObjectPrefabID, Vector3 position, Quaternion rotation)
        {
            InstantiateEffectsObjectParameters ieParms = new InstantiateEffectsObjectParameters
            {
                effectsObjectPrefabID = effectsObjectPrefabID,
                position = position,
                rotation = rotation
            };

            InstantiateEffectsObject(ref ieParms);
        }

        /// <summary>
        /// Instantiates the effects object with ID effectsObjectPrefabID at the position specified in world space
        /// (effectsObjectPrefabID is the ID in ieParms sent back to each projectile / damage region after calling the UpdateProjectilesAndEffects() method).
        /// </summary>
        /// <param name="ieParms"></param>
        /// <returns></returns>
        public EffectsModule InstantiateEffectsObject (ref InstantiateEffectsObjectParameters ieParms)
        {
            EffectsModule _effectsModule = null;

            if (isInitialised)
            {
                // Check that a valid ID has been provided
                if (ieParms.effectsObjectPrefabID >= 0 && ieParms.effectsObjectPrefabID < effectsObjectTemplatesList.Count)
                {
                    // Get the effects object template using its ID (this is just the index of it in the effects object template list)
                    effectsObjectTemplate = effectsObjectTemplatesList[ieParms.effectsObjectPrefabID];

                    // Currently effects objects are only instantiated as normal Unity gameobjects with optional pooling
                    if (effectsObjectTemplate.effectsObjectPrefab.usePooling)
                    {
                        // If we are using pooling, find the first inactive effects object
                        if (effectsObjectTemplate.effectsObjectPool == null)
                        {
                            #if UNITY_EDITOR
                            Debug.LogWarning("SSCManager InstantiateEffectsObject() effectsObjectPool is null. We do not support changing to usePooling for an effects object at runtime.");
                            #endif
                        }
                        else
                        {
                            int firstInactiveIndex = effectsObjectTemplate.effectsObjectPool.FindIndex(e => !e.activeInHierarchy);
                            if (firstInactiveIndex == -1)
                            {
                                // All effects objects in the pool are currently active
                                // So if we want to get a new one, we'll need to add a new one to the pool
                                // First check if we have reached the max pool size
                                if (effectsObjectTemplate.currentPoolSize < effectsObjectTemplate.effectsObjectPrefab.maxPoolSize)
                                {
                                    // If we are still below the max pool size, add a new effects object instance to the pool
                                    // Instantiate the effects object with the correct position and rotation
                                    effectsObjectGameObjectInstance = Instantiate(effectsObjectTemplate.effectsObjectPrefab.gameObject,
                                        ieParms.position, ieParms.rotation);
                                    // Set the object's parent to be the manager
                                    effectsObjectGameObjectInstance.transform.SetParent(effectsPooledTrfm);
                                    // Initialise the effects object
                                    _effectsModule = effectsObjectGameObjectInstance.GetComponent<EffectsModule>();
                                    ieParms.effectsObjectSequenceNumber = _effectsModule.InitialiseEffectsObject();
                                    ieParms.effectsObjectPoolListIndex = effectsObjectTemplate.currentPoolSize;
                                    // Add the object to the list of pooled objects
                                    effectsObjectTemplate.effectsObjectPool.Add(effectsObjectGameObjectInstance);
                                    // Update the current pool size counter
                                    effectsObjectTemplate.currentPoolSize++;
                                }
                            }
                            else
                            {
                                // Get the effects object
                                effectsObjectGameObjectInstance = effectsObjectTemplate.effectsObjectPool[firstInactiveIndex];
                                // Position the object
                                effectsObjectGameObjectInstance.transform.SetPositionAndRotation(ieParms.position, ieParms.rotation);
                                // Set the object to active
                                effectsObjectGameObjectInstance.SetActive(true);
                                // Initialise the effects object
                                _effectsModule = effectsObjectGameObjectInstance.GetComponent<EffectsModule>();
                                ieParms.effectsObjectSequenceNumber = _effectsModule.InitialiseEffectsObject();
                                ieParms.effectsObjectPoolListIndex = firstInactiveIndex;
                            }
                        }
                    }
                    else
                    {
                        // If we are not using DOTS or pooling, simply instantiate the effect
                        effectsObjectGameObjectInstance = Instantiate(effectsObjectTemplate.effectsObjectPrefab.gameObject,
                                    ieParms.position, ieParms.rotation);
                        // Set the object's parent to be the manager
                        effectsObjectGameObjectInstance.transform.SetParent(effectsNonPooledTrfm);
                        // Initialise the effects object
                        _effectsModule = effectsObjectGameObjectInstance.GetComponent<EffectsModule>();
                        ieParms.effectsObjectSequenceNumber = _effectsModule.InitialiseEffectsObject();
                        ieParms.effectsObjectPoolListIndex = -1;
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    // If an invalid ID is provided, log a warning
                    Debug.LogWarning("SSCManager InstantiateEffectsObject() Warning: Provided effectsObjectPrefabID was invalid (" + ieParms.effectsObjectPrefabID +
                        "). To get the correct ID, call UpdateProjectilesAndEffects(..) for the ship or SurfaceTurret, and use the " +
                        "effectsObjectPrefabID populated for each projectile / damage region.");
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("SSCManager InstantiateEffectsObject() Warning: Method was called before the manager was initialised.");
            }
            #endif

            return _effectsModule;
        }

        #endregion

        #region Private and Internal Methods

        /// <summary>
        /// Adds a new projectile template to the list. Returns the projectile template index.
        /// </summary>
        /// <param name="projectilePrefab"></param>
        /// <param name="projectileTransformID"></param>
        private int AddProjectileTemplate (ProjectileModule projectilePrefab, int projectileTransformID)
        {
            // Create a new projectile template for this prefab
            projectileTemplate = new ProjectileTemplate(projectilePrefab, projectileTransformID);
            // Add the new projectile template to the end of the list
            projectileTemplatesList.Add(projectileTemplate);
            // Get the index of the new projectile template
            int projectileTemplateIndex = projectileTemplatesList.Count - 1;

            // Don't need to do anything else for non-pooling.                                

            // If we are using pooling for this projectile, set up the pool
            if (projectileTemplate.projectilePrefab.usePooling)
            {
                // Initialise projectile pool with capacity of minimum pool size
                projectileTemplate.currentPoolSize = projectileTemplate.projectilePrefab.minPoolSize;
                projectileTemplate.projectilePool = new List<GameObject>(projectileTemplate.currentPoolSize);
                // Create the objects in the pool
                for (int i = 0; i < projectileTemplate.currentPoolSize; i++)
                {
                    // Instantiate the projectile object
                    projectileGameObjectInstance = Instantiate(projectileTemplate.projectilePrefab.gameObject);
                    // Set the object's parent to be the manager
                    projectileGameObjectInstance.transform.SetParent(projectilePooledTrfm);
                    // Set the object to inactive
                    projectileGameObjectInstance.SetActive(false);

                    ProjectileModule _projectile = projectileGameObjectInstance.GetComponent<ProjectileModule>();
                    // Remember which scene this projectile instance is being created in
                    _projectile.sceneHandle = sceneHandle;

                    // Add the object to the list of pooled objects
                    projectileTemplate.projectilePool.Add(projectileGameObjectInstance);
                }
            }
            #if SSC_ENTITIES
            // If the projectile module uses DOTS/ECS then (currently) it requires updating
            // with FixedUpdate() from within SSCManager. NOTE: There can be 0, 1 or more projectile prefabs
            // with DOTS enabled. See also Initialise ().
            else if (projectileTemplate.projectilePrefab.useECS)
            {
                isProjectileSytemUpdateRequired = true;
            }
            #endif

            // When a new ProjectileTemplate is added to projectileTemplatesList, store the
            // index of the ProjectileTemplate in the ProjectileModule attached to the ProjectileTemplate.
            // This is used with Projectile FX when we know the ProjectileModule but not the parent ProjectileTemplate.
            if (projectileTemplate.projectilePrefab != null)
            {
                projectileTemplate.projectilePrefab.projectilePrefabID = projectileTemplateIndex;
            }

            return projectileTemplateIndex;
        }

        /// <summary>
        /// Adds a new beam template to the list. Returns the beam template index.
        /// </summary>
        /// <param name="beamPrefab"></param>
        /// <param name="beamTransformID"></param>
        /// <returns></returns>
        private int AddBeamTemplate (BeamModule beamPrefab, int beamTransformID)
        {
            // Create a new beam template for this prefab
            beamTemplate = new BeamTemplate(beamPrefab, beamTransformID);
            // Add the new beam template to the end of the list
            beamTemplateList.Add(beamTemplate);
            // Get the index of the new beam template
            int beamTemplateIndex = beamTemplateList.Count - 1;

            // Don't need to do anything else for non-pooling.                                

            // If we are using pooling for this beam, set up the pool
            if (beamTemplate.beamPrefab.usePooling)
            {
                // Initialise beam pool with capacity of minimum pool size
                beamTemplate.currentPoolSize = beamTemplate.beamPrefab.minPoolSize;
                beamTemplate.beamPoolList = new List<GameObject>(beamTemplate.currentPoolSize);
                // Create the objects in the pool
                for (int i = 0; i < beamTemplate.currentPoolSize; i++)
                {
                    // Instantiate the beam object
                    beamGameObjectInstance = Instantiate(beamTemplate.beamPrefab.gameObject);
                    // Set the object's parent to be the manager
                    beamGameObjectInstance.transform.SetParent(beamPooledTrfm);
                    // Set the object to inactive
                    beamGameObjectInstance.SetActive(false);
                    // Remember which scene this beam instance is being created in
                    beamGameObjectInstance.GetComponent<BeamModule>().sceneHandle = sceneHandle;

                    // Add the object to the list of pooled objects
                    beamTemplate.beamPoolList.Add(beamGameObjectInstance);
                }
            }

            // When a new BeamTemplate is added to beamTemplateList, store the
            // index of the BeamTemplate in the BeamModule attached to the BeamTemplate.
            // This is used with Beam FX when we know the BeamModule but not the parent BeamTemplate.
            if (beamTemplate.beamPrefab != null)
            {
                beamTemplate.beamPrefab.beamPrefabID = beamTemplateIndex;
            }

            return beamTemplateIndex;
        }

        /// <summary>
        /// Adds a new destruct template to the list.
        /// TODO: consider initialising when first adding to the pool
        /// TODO: turn off isExplodeOnStart
        /// </summary>
        /// <param name="destructPrefab"></param>
        /// <param name="destructTransformID"></param>
        private int AddDestructTemplate (DestructModule destructPrefab, int destructTransformID)
        {
            // Create a new destruct template for this prefab
            destructTemplate = new DestructTemplate(destructPrefab, destructTransformID);
            // Add the new destruct object template to the end of the list
            destructTemplateList.Add(destructTemplate);
            // Get the index of the new destruct object template
            int destructTemplateIndex = destructTemplateList.Count - 1;

            // Don't need to do anything else for non-pooling.                                

            // If we are using pooling for this Destruct module, set up the pool
            if (destructTemplate.destructPrefab.usePooling)
            {
                // Always disable isExplodeOnStart when using pooling
                destructTemplate.destructPrefab.isExplodeOnStart = false;

                // Initialise destruct pool with capacity of minimum pool size
                destructTemplate.currentPoolSize = destructTemplate.destructPrefab.minPoolSize;
                destructTemplate.destructPoolList = new List<GameObject>(destructTemplate.currentPoolSize);
                // Create the objects in the pool
                for (int i = 0; i < destructTemplate.currentPoolSize; i++)
                {
                    // Instantiate the destruct object
                    destructGameObjectInstance = Instantiate(destructTemplate.destructPrefab.gameObject);
                    // Set the object's parent to be the manager
                    destructGameObjectInstance.transform.SetParent(destructPooledTrfm);
                    // Set the object to inactive
                    destructGameObjectInstance.SetActive(false);
                    // Add the object to the list of pooled objects
                    destructTemplate.destructPoolList.Add(destructGameObjectInstance);
                }
            }

            return destructTemplateIndex;
        }

        /// <summary>
        /// Adds a new effects object template to the list.
        /// </summary>
        /// <param name="effectsObjectPrefab"></param>
        /// <param name="effectsObjectTransformID"></param>
        private int AddEffectsObjectTemplate (EffectsModule effectsObjectPrefab, int effectsObjectTransformID)
        {
            // Create a new effects object template for this prefab
            effectsObjectTemplate = new EffectsObjectTemplate(effectsObjectPrefab, effectsObjectTransformID);
            // Add the new effects object template to the end of the list
            effectsObjectTemplatesList.Add(effectsObjectTemplate);
            // Get the index of the new effects object template
            int effectsObjectTemplateIndex = effectsObjectTemplatesList.Count - 1;

            // Don't need to do anything else for non-pooling.                                

            // If we are using pooling for this effects module, set up the pool
            if (effectsObjectTemplate.effectsObjectPrefab.usePooling)
            {
                // Initialise effects pool with capacity of minimum pool size
                effectsObjectTemplate.currentPoolSize = effectsObjectTemplate.effectsObjectPrefab.minPoolSize;
                effectsObjectTemplate.effectsObjectPool = new List<GameObject>(effectsObjectTemplate.currentPoolSize);
                // Create the objects in the pool
                for (int i = 0; i < effectsObjectTemplate.currentPoolSize; i++)
                {
                    // Instantiate the effects object
                    effectsObjectGameObjectInstance = Instantiate(effectsObjectTemplate.effectsObjectPrefab.gameObject);
                    // Set the object's parent to be the manager
                    effectsObjectGameObjectInstance.transform.SetParent(effectsPooledTrfm);
                    // Set the object to inactive
                    effectsObjectGameObjectInstance.SetActive(false);
                    // Add the object to the list of pooled objects
                    effectsObjectTemplate.effectsObjectPool.Add(effectsObjectGameObjectInstance);
                }
            }

            return effectsObjectTemplateIndex;
        }

        /// <summary>
        /// Find an existing projectile template for this weapon, or add a new one.
        /// Find an existing effects template for this weapon, or add a new one.
        /// Update the estimatedRange of the weapon.
        /// </summary>
        /// <param name="weapon"></param>
        private void UpdateWeaponProjectileAndEffects(Weapon weapon)
        {
            // Get the transform instance ID for this projectile prefab
            int projectileTransformID = weapon.projectilePrefab.transform.GetInstanceID();
            // Search the projectile templates list to see if we already have a 
            // projectile prefab with the same instance ID
            int projectileTemplateIndex = projectileTemplatesList.FindIndex(p => p.instanceID == projectileTransformID);

            if (projectileTemplateIndex == -1)
            {
                // If no match was found, create a new projectile template for this prefab
                weapon.projectilePrefabID = AddProjectileTemplate(weapon.projectilePrefab, projectileTransformID);

                // Check if the projectile has a destruction effects object
                if (weapon.projectilePrefab.effectsObject != null)
                {
                    // Get the transform instance ID for this effects object prefab
                    int effectsObjectTransformID = weapon.projectilePrefab.effectsObject.transform.GetInstanceID();
                    // Search the effects object templates list to see if we already have an 
                    // effects object prefab with the same instance ID
                    int effectsObjectTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsObjectTransformID);

                    if (effectsObjectTemplateIndex == -1)
                    {
                        // If no match was found, create a new effects object template for this prefab
                        weapon.projectilePrefab.effectsObjectPrefabID = AddEffectsObjectTemplate(weapon.projectilePrefab.effectsObject, effectsObjectTransformID);
                    }
                    else
                    {
                        // Save the effect template index in the projectile
                        weapon.projectilePrefab.effectsObjectPrefabID = effectsObjectTemplateIndex;
                    }
                }
                // No destruction effects object for this projectile
                else { weapon.projectilePrefab.effectsObjectPrefabID = -1; }

                // Check if the projectile has a destruction shield effects object
                if (weapon.projectilePrefab.shieldEffectsObject != null)
                {
                    // Get the transform instance ID for this effects object prefab
                    int effectsObjectTransformID = weapon.projectilePrefab.shieldEffectsObject.transform.GetInstanceID();
                    // Search the effects object templates list to see if we already have an 
                    // effects object prefab with the same instance ID
                    int effectsObjectTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsObjectTransformID);

                    if (effectsObjectTemplateIndex == -1)
                    {
                        // If no match was found, create a new shield effects object template for this prefab
                        weapon.projectilePrefab.shieldEffectsObjectPrefabID = AddEffectsObjectTemplate(weapon.projectilePrefab.shieldEffectsObject, effectsObjectTransformID);
                    }
                    else
                    {
                        // Save the effect template index in the projectile
                        weapon.projectilePrefab.shieldEffectsObjectPrefabID = effectsObjectTemplateIndex;
                    }
                }
                // No destruction shield effects object for this projectile
                else { weapon.projectilePrefab.shieldEffectsObjectPrefabID = -1; }

                // Check if the projectile has a muzzle effects object
                if (weapon.projectilePrefab.muzzleEffectsObject != null)
                {
                    // Get the transform instance ID for this effects object prefab
                    int effectsObjectTransformID = weapon.projectilePrefab.muzzleEffectsObject.transform.GetInstanceID();
                    // Search the effects object templates list to see if we already have an 
                    // effects object prefab with the same instance ID
                    int effectsObjectTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsObjectTransformID);

                    if (effectsObjectTemplateIndex == -1)
                    {
                        // If no match was found, create a new effects object template for this prefab
                        weapon.projectilePrefab.muzzleEffectsObjectPrefabID = AddEffectsObjectTemplate(weapon.projectilePrefab.muzzleEffectsObject, effectsObjectTransformID);
                    }
                    else
                    {
                        // Save the effect template index in the projectile
                        weapon.projectilePrefab.muzzleEffectsObjectPrefabID = effectsObjectTemplateIndex;
                    }
                }
                // No muzzle effects object for this projectile
                else { weapon.projectilePrefab.muzzleEffectsObjectPrefabID = -1; }
            }
            else
            {
                // Save the projectile template index in the weapon
                weapon.projectilePrefabID = projectileTemplateIndex;
            }

            weapon.estimatedRange = weapon.projectilePrefab.estimatedRange;

            weapon.isProjectileKGuideToTarget = weapon.projectilePrefab.isKinematicGuideToTarget;
        }

        /// <summary>
        /// Find an existing beam template for this weapon, or add a new one.
        /// Find an existing effects template for this weapon, or add a new one.
        /// Update the estimated range of the weapon.
        /// </summary>
        /// <param name="weapon"></param>
        private void UpdateWeaponBeamAndEffects(Weapon weapon)
        {
            // Get the transform instance ID for this beam prefab
            int beamTransformID = weapon.beamPrefab.transform.GetInstanceID();
            // Search the beam templates list to see if we already have a 
            // beam prefab with the same instance ID
            int beamTemplateIndex = beamTemplateList.FindIndex(p => p.instanceID == beamTransformID);

            if (beamTemplateIndex == -1)
            {
                // If no match was found, create a new beam template for this prefab
                weapon.beamPrefabID = AddBeamTemplate(weapon.beamPrefab, beamTransformID);

                // Check if the beam has a hit effects object
                if (weapon.beamPrefab.effectsObject != null)
                {
                    // Get the transform instance ID for this effects object prefab
                    int effectsObjectTransformID = weapon.beamPrefab.effectsObject.transform.GetInstanceID();
                    // Search the effects object templates list to see if we already have an 
                    // effects object prefab with the same instance ID
                    int effectsObjectTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsObjectTransformID);

                    if (effectsObjectTemplateIndex == -1)
                    {
                        // If no match was found, create a new effects object template for this prefab
                        weapon.beamPrefab.effectsObjectPrefabID = AddEffectsObjectTemplate(weapon.beamPrefab.effectsObject, effectsObjectTransformID);
                    }
                    else
                    {
                        // Save the effect template index in the beam
                        weapon.beamPrefab.effectsObjectPrefabID = effectsObjectTemplateIndex;
                    }
                }
                // No hit effects object for this beam
                else { weapon.beamPrefab.effectsObjectPrefabID = -1; }

                // Check if the beam has a muzzle effects object
                if (weapon.beamPrefab.muzzleEffectsObject != null)
                {
                    // Get the transform instance ID for this effects object prefab
                    int effectsObjectTransformID = weapon.beamPrefab.muzzleEffectsObject.transform.GetInstanceID();
                    // Search the effects object templates list to see if we already have an 
                    // effects object prefab with the same instance ID
                    int effectsObjectTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsObjectTransformID);

                    if (effectsObjectTemplateIndex == -1)
                    {
                        // If no match was found, create a new effects object template for this prefab
                        weapon.beamPrefab.muzzleEffectsObjectPrefabID = AddEffectsObjectTemplate(weapon.beamPrefab.muzzleEffectsObject, effectsObjectTransformID);
                    }
                    else
                    {
                        // Save the effect template index in the beam
                        weapon.beamPrefab.muzzleEffectsObjectPrefabID = effectsObjectTemplateIndex;
                    }
                }
                // No muzzle effects object for this beam
                else { weapon.beamPrefab.muzzleEffectsObjectPrefabID = -1; }
            }
            else
            {
                // Save the beam template index in the weapon
                weapon.beamPrefabID = beamTemplateIndex;
            }

            weapon.estimatedRange = weapon.maxRange;
        }

        /// <summary>
        /// Given an initial Path section length, calculate the number of segments to use to get an
        /// Path distance. Currently it uses a global pathAccuracy value.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="prevPathPointIndex"></param>
        /// <param name="initialSectionDistance"></param>
        /// <returns></returns>
        private int CalcPathSegments(PathData pathData, int prevPathPointIndex, float initialSectionDistance)
        {
            int numSegments = 1;
            float sectionDistance = initialSectionDistance;
            // If the length of the Path section is unknown, first do a estimate with only a few segments
            if (initialSectionDistance <= 0f)
            {
                if (!SSCMath.GetDistanceBetweenPathPoints(pathData, prevPathPointIndex, numPathSegmentsForEstimate, ref sectionDistance))
                {
                    // If this failed, there is not much we can do
                    return numPathSegmentsForEstimate;
                }
            }

            // Round up. Path Accuracy is between 0.05 and 1.0
            numSegments = (int)(0.5f + (pathPrecision * sectionDistance / maxPathSegmentPrecisionLength));

            if (numSegments < numPathSegmentsForEstimate) { numSegments = numPathSegmentsForEstimate; }
            return numSegments;
        }

        /// <summary>
        /// Deactivate a beam
        /// </summary>
        /// <param name="beamItemKey"></param>
        internal void DeactivateBeam(SSCBeamItemKey beamItemKey)
        {
            // Is this beam pooled?
            if (beamItemKey.beamPoolListIndex >= 0 && beamItemKey.beamSequenceNumber != 0)
            {
                if (beamItemKey.beamTemplateListIndex >= 0)
                {
                    BeamTemplate _beamTemplate = beamTemplateList[beamItemKey.beamTemplateListIndex];

                    if (_beamTemplate != null && _beamTemplate.beamPrefab != null && _beamTemplate.beamPrefab.usePooling)
                    {
                        GameObject pmGO = _beamTemplate.beamPoolList[beamItemKey.beamPoolListIndex];
                        if (pmGO.activeInHierarchy)
                        {
                            BeamModule beamModule = pmGO.GetComponent<BeamModule>();

                            // Verify we have the correct beam
                            if (beamModule != null && beamModule.itemSequenceNumber == beamItemKey.beamSequenceNumber)
                            {
                                beamModule.DestroyBeam();
                            }                           
                        }
                    }
                }
            }
            // Is this an active beam non-pooled beam?
            else if (beamItemKey.beamSequenceNumber != 0 && beamNonPooledTrfm != null)
            {
                BeamModule[] _beamModules = beamNonPooledTrfm.GetComponentsInChildren<BeamModule>(false);
                int _numBeamModules = _beamModules == null ? 0 : _beamModules.Length;

                for (int bmIdx = 0; bmIdx < _numBeamModules; bmIdx++)
                {
                    BeamModule beamModule = _beamModules[bmIdx];
                    // Verify we have the correct beam
                    if (beamModule.itemSequenceNumber == beamItemKey.beamSequenceNumber)
                    {
                        beamModule.DestroyBeam();
                    }
                }
            }
        }

        /// <summary>
        /// Destroy the effects Object or return it to the pool.
        /// </summary>
        /// <param name="effectItemKey"></param>
        internal void DestroyEffectsObject(SSCEffectItemKey effectItemKey)
        {
            // Is this effect pooled?
            if (effectItemKey.effectsObjectPoolListIndex >= 0 && effectItemKey.effectsObjectSequenceNumber != 0)
            {
                if (effectItemKey.effectsObjectTemplateListIndex >= 0)
                {
                    EffectsObjectTemplate _effectsTemplate = effectsObjectTemplatesList[effectItemKey.effectsObjectTemplateListIndex];

                    if (_effectsTemplate != null && _effectsTemplate.effectsObjectPrefab != null && _effectsTemplate.effectsObjectPrefab.usePooling)
                    {
                        GameObject pmGO = _effectsTemplate.effectsObjectPool[effectItemKey.effectsObjectPoolListIndex];
                        if (pmGO.activeInHierarchy)
                        {
                            EffectsModule _effectsModule = pmGO.GetComponent<EffectsModule>();

                            // Verify we have the correct effects object
                            if (_effectsModule != null && _effectsModule.itemSequenceNumber == effectItemKey.effectsObjectSequenceNumber)
                            {
                                _effectsModule.CancelInvoke(EffectsModule.destroyMethodName);
                                _effectsModule.DestroyEffectsObject();
                            }
                        }
                    }
                }
            }
            // Is this a non-pooled effect?
            else if (effectItemKey.effectsObjectSequenceNumber != 0 && effectsNonPooledTrfm != null)
            {
                EffectsModule[] _effectsModules = effectsNonPooledTrfm.GetComponentsInChildren<EffectsModule>(false);
                int _numEffectsModules = _effectsModules == null ? 0 : _effectsModules.Length;

                for (int emIdx = 0; emIdx < _numEffectsModules; emIdx++)
                {
                    EffectsModule _effectsModule = _effectsModules[emIdx];
                    // Verify we have the correct effects object
                    if (_effectsModule.itemSequenceNumber == effectItemKey.effectsObjectSequenceNumber)
                    {
                        _effectsModule.CancelInvoke(EffectsModule.destroyMethodName);
                        _effectsModule.DestroyEffectsObject();
                    }
                }
            }
        }

        /// <summary>
        /// Return the transform for an instance of an EffectsModule in an effects pool.
        /// If checkIsReparented is true, it will return null if IsReparented is false.
        /// </summary>
        /// <param name="effectsObjectPrefabID"></param>
        /// <param name="effectsObjectPoolListIndex"></param>
        /// <param name="checkIsReparented"></param>
        /// <returns></returns>
        internal Transform GetEffectsObjectTransform(int effectsObjectPrefabID, int effectsObjectPoolListIndex, bool checkIsReparented)
        {
            if (effectsObjectPrefabID >= 0 && effectsObjectPoolListIndex >= 0)
            {
                if (checkIsReparented)
                {
                    effectsObjectTemplate = effectsObjectTemplatesList[effectsObjectPrefabID];

                    if (effectsObjectTemplate.effectsObjectPrefab.isReparented)
                    {
                        return effectsObjectTemplate.effectsObjectPool[effectsObjectPoolListIndex].transform;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return effectsObjectTemplatesList[effectsObjectPrefabID].effectsObjectPool[effectsObjectPoolListIndex].transform;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Change the transform position and rotation of an EffectsObject.
        /// It is faster to not validate sequence numbers for pooled effects.
        /// </summary>
        /// <param name="effectItemKey"></param>
        /// <param name="newPosition"></param>
        /// <param name="newRotation"></param>
        /// <param name="isValidateSequenceNumber"></param>
        internal bool MoveEffectsObject(SSCEffectItemKey effectItemKey, Vector3 newPosition, Quaternion newRotation, bool isValidateSequenceNumber)
        {
            bool isEffectActiveAndValid = false;

            // Is this effect pooled?
            if (effectItemKey.effectsObjectPoolListIndex >= 0 && effectItemKey.effectsObjectSequenceNumber != 0)
            {
                if (effectItemKey.effectsObjectTemplateListIndex >= 0)
                {
                    EffectsObjectTemplate _effectsTemplate = effectsObjectTemplatesList[effectItemKey.effectsObjectTemplateListIndex];

                    if (_effectsTemplate != null && _effectsTemplate.effectsObjectPrefab != null && _effectsTemplate.effectsObjectPrefab.usePooling)
                    {
                        GameObject pmGO = _effectsTemplate.effectsObjectPool[effectItemKey.effectsObjectPoolListIndex];
                        if (pmGO.activeInHierarchy)
                        {
                            if (!isValidateSequenceNumber || pmGO.GetComponent<EffectsModule>().itemSequenceNumber == effectItemKey.effectsObjectSequenceNumber)
                            {
                                pmGO.transform.SetPositionAndRotation(newPosition, newRotation);
                                isEffectActiveAndValid = true;
                            }
                        }
                    }
                }
            }
            // Is this a non-pooled effect?
            else if (effectItemKey.effectsObjectSequenceNumber != 0 && effectsNonPooledTrfm != null)
            {
                EffectsModule[] _effectsModules = effectsNonPooledTrfm.GetComponentsInChildren<EffectsModule>(false);
                int _numEffectsModules = _effectsModules == null ? 0 : _effectsModules.Length;

                for (int emIdx = 0; emIdx < _numEffectsModules; emIdx++)
                {
                    EffectsModule _effectsModule = _effectsModules[emIdx];
                    // Verify we have the correct beam
                    if (_effectsModule.itemSequenceNumber == effectItemKey.effectsObjectSequenceNumber)
                    {
                        _effectsModule.transform.SetPositionAndRotation(newPosition, newRotation);
                        isEffectActiveAndValid = true;
                        break;
                    }
                }
            }

            return isEffectActiveAndValid;
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// See also PauseBeams() and ResumeBeams() in Public API
        /// Pause or unpause all Pooled, and Non-Pooled Beams in the scene
        /// </summary>
        /// <param name="isPause"></param>
        private void PauseBeams(bool isPause)
        {
            if (isInitialised)
            {
                isBeamsPaused = isPause;

                int numBeamTemplates = beamTemplateList == null ? 0 : beamTemplateList.Count;

                // Pause or unpause all active Pooled Beams
                for (int ptIdx = 0; ptIdx < numBeamTemplates; ptIdx++)
                {
                    BeamTemplate _beamTemplate = beamTemplateList[ptIdx];
                    if (_beamTemplate != null && _beamTemplate.beamPrefab != null && _beamTemplate.beamPrefab.usePooling)
                    {
                        // Examine each of the beams in the pool
                        for (int pmIdx = 0; pmIdx < _beamTemplate.currentPoolSize; pmIdx++)
                        {
                            GameObject pmGO = _beamTemplate.beamPoolList[pmIdx];
                            if (pmGO.activeInHierarchy)
                            {
                                if (isPause) { pmGO.GetComponent<BeamModule>().DisableBeam(); }
                                else { pmGO.GetComponent<BeamModule>().EnableBeam(); }
                            }
                        }
                    }
                }

                // Pause or unpause all active Non-Pooled Beams (they should probably all be active in the heierarchy)
                if (beamNonPooledTrfm != null)
                {
                    BeamModule[] _beamModules = beamNonPooledTrfm.GetComponentsInChildren<BeamModule>(false);
                    int _numBeamModules = _beamModules == null ? 0 : _beamModules.Length;

                    for (int pmIdx = 0; pmIdx < _numBeamModules; pmIdx++)
                    {
                        if (isPause) { _beamModules[pmIdx].DisableBeam(); }
                        else { _beamModules[pmIdx].EnableBeam(); }
                    }
                }
            }
        }

        /// <summary>
        /// [INTERNAL ONLY] - UNTESTED
        /// See also PauseDestructs() and ResumeDestructs() in Public API
        /// Pause or unpause all Pooled, and Non-Pooled Destruct Modules in the scene
        /// </summary>
        /// <param name="isPause"></param>
        private void PauseDestructModules(bool isPause)
        {
            if (isInitialised)
            {
                isDestructsPaused = isPause;

                int numDestructTemplates = destructTemplateList == null ? 0 : destructTemplateList.Count;

                // Pause or unpause all active Pooled Destructs
                for (int dtIdx = 0; dtIdx < numDestructTemplates; dtIdx++)
                {
                    DestructTemplate _destructTemplate = destructTemplateList[dtIdx];
                    if (_destructTemplate != null && _destructTemplate.destructPrefab != null && _destructTemplate.destructPrefab.usePooling)
                    {
                        // Examine each of the destructs in the pool
                        for (int pmIdx = 0; pmIdx < _destructTemplate.currentPoolSize; pmIdx++)
                        {
                            GameObject pmGO = _destructTemplate.destructPoolList[pmIdx];
                            if (pmGO.activeInHierarchy)
                            {
                                if (isPause) { pmGO.GetComponent<DestructModule>().DisableDestruct(); }
                                else { pmGO.GetComponent<DestructModule>().EnableDestruct(); }
                            }
                        }
                    }
                }

                // Pause or unpause all active Non-Pooled Destructs (they should probably all be active in the heierarchy)
                if (destructNonPooledTrfm != null)
                {
                    DestructModule[] _destructModules = destructNonPooledTrfm.GetComponentsInChildren<DestructModule>(false);
                    int _numDestructModules = _destructModules == null ? 0 : _destructModules.Length;

                    for (int pmIdx = 0; pmIdx < _numDestructModules; pmIdx++)
                    {
                        if (isPause) { _destructModules[pmIdx].DisableDestruct(); }
                        else { _destructModules[pmIdx].EnableDestruct(); }
                    }
                }
            }
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// See also PauseProjectiles() and ResumeProjectiles() in Public API
        /// Pause or unpause all DOTS, Pooled, and Non-Pooled Projectiles in the scene
        /// </summary>
        /// <param name="isPause"></param>
        private void PauseProjectiles(bool isPause)
        {
            if (isInitialised)
            {
                // This will also stop (or resume) DOTS projectileSystem.Update() being called if applicable
                isProjectilesPaused = isPause;

                int numProjectileTemplates = projectileTemplatesList == null ? 0 : projectileTemplatesList.Count;

                // Pause or unpause all active Pooled Projectiles
                for (int ptIdx = 0; ptIdx < numProjectileTemplates; ptIdx++)
                {
                    ProjectileTemplate _projectileTemplate = projectileTemplatesList[ptIdx];
                    if (_projectileTemplate != null && _projectileTemplate.projectilePrefab != null && _projectileTemplate.projectilePrefab.usePooling)
                    {
                        // Examine each of the projectiles in the pool
                        for (int pmIdx = 0; pmIdx < _projectileTemplate.currentPoolSize; pmIdx++)
                        {
                            GameObject pmGO = _projectileTemplate.projectilePool[pmIdx];
                            if (pmGO.activeInHierarchy)
                            {
                                if (isPause) { pmGO.GetComponent<ProjectileModule>().DisableProjectile(); }
                                else { pmGO.GetComponent<ProjectileModule>().EnableProjectile(); }
                            }
                        }
                    }
                }

                // Pause or unpause all active Non-Pooled Projectiles (they should probably all be active in the heierarchy)
                if (projectileNonPooledTrfm != null)
                {
                    ProjectileModule[] _projectileModules = projectileNonPooledTrfm.GetComponentsInChildren<ProjectileModule>(false);
                    int _numProjectileModules = _projectileModules == null ? 0 : _projectileModules.Length;

                    for (int pmIdx = 0; pmIdx < _numProjectileModules; pmIdx++)
                    {
                        if (isPause) { _projectileModules[pmIdx].DisableProjectile(); }
                        else { _projectileModules[pmIdx].EnableProjectile(); }
                    }
                }
            }
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// See also PauseEffectsObjects() and ResumeEffectsObjects() in Public API.
        /// Pause or unpause all pooled and non-pooled effects objects in the scene.
        /// </summary>
        /// <param name="isPause"></param>
        private void PauseEffectsObjects(bool isPause)
        {
            if (isInitialised)
            {
                isEffectsObjectsPaused = isPause;

                int numEffectsTemplates = effectsObjectTemplatesList == null ? 0 : effectsObjectTemplatesList.Count;

                // Pause or unpause all active Pooled Effects
                for (int eotIdx = 0; eotIdx < numEffectsTemplates; eotIdx++)
                {
                    EffectsObjectTemplate  _effectsTemplate = effectsObjectTemplatesList[eotIdx];
                    if (_effectsTemplate != null && _effectsTemplate.effectsObjectPrefab != null && _effectsTemplate.effectsObjectPrefab.usePooling)
                    {
                        // Examine each of the effects modules in the pool
                        for (int emIdx = 0; emIdx < _effectsTemplate.currentPoolSize; emIdx++)
                        {
                            GameObject emGo = _effectsTemplate.effectsObjectPool[emIdx];
                            if (emGo.activeInHierarchy)
                            {
                                if (isPause) { emGo.GetComponent<EffectsModule>().DisableEffects(); }
                                else { emGo.GetComponent<EffectsModule>().EnableEffects(); }
                            }
                        }
                    }
                }

                // Pause or unpause all active Non-Pooled Effects (they should probably all be active in the heierarchy)
                if (effectsNonPooledTrfm != null)
                {
                    EffectsModule[] _effectsModules = effectsNonPooledTrfm.GetComponentsInChildren<EffectsModule>(false);
                    int _numEffectsModules = _effectsModules == null ? 0 : _effectsModules.Length;

                    for (int emIdx = 0; emIdx < _numEffectsModules; emIdx++)
                    {
                        if (isPause) { _effectsModules[emIdx].DisableEffects(); }
                        else { _effectsModules[emIdx].EnableEffects(); }
                    }
                }
            }
        }


        /// <summary>
        /// Select or unselect all the Locations in a Path.
        /// Always unselect in/out Controls for a valid Path Location
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="isSelected"></param>
        private void SelectPathLocations(PathData pathData, bool isSelected)
        {
            if (pathData != null)
            {
                int numLocations = pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;
                PathLocationData pathLocationData = null;
                LocationData locationData = null;
                for (int idx = 0; idx < numLocations; idx++)
                {
                    pathLocationData = pathData.pathLocationDataList[idx];
                    locationData = pathLocationData == null ? null : pathLocationData.locationData;
                    if (locationData != null)
                    {
                        locationData.selectedInSceneView = isSelected;
                        pathLocationData.inControlSelectedInSceneView = false;
                        pathLocationData.outControlSelectedInSceneView = false;
                    }
                }
            }
        }

        //// For testing only
        //#if SSC_ENTITIES
        //void OnGUI()
        //{
        //    GUI.Label(new Rect(10, 10, 100, 20), ProjectileSystem.GetTotalProjectiles.ToString());

        //    int numTemplates = projectileTemplatesList == null ? 0 : projectileTemplatesList.Count;
        //    if (numTemplates > 0) { GUI.Label(new Rect(10, 30, 100, 20), projectileTemplatesList[0].currentPoolSize.ToString()); }
        //}
        //#endif

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Attempt to add a SSCManager to the current scene
        /// </summary>
        /// <param name="sceneHandle"></param>
        /// <returns></returns>
        private static SSCManager AddSSCManager(int sceneHandle)
        {
            if (sceneHandle != 0)
            {
                Scene activeScene = SceneManager.GetActiveScene();

                // Check to see if this is the active scene.
                // If not, attempt to activate the correct scene.
                if (activeScene.handle != sceneHandle)
                {
                    int numScenes = SceneManager.sceneCount;

                    for (int sIdx = 0; sIdx < numScenes; sIdx++)
                    {
                        Scene _scene = SceneManager.GetSceneAt(sIdx);

                        if (_scene.handle == sceneHandle)
                        {
                            if (_scene.isLoaded)
                            {
                                SceneManager.SetActiveScene(_scene);
                            }
                            #if UNITY_EDITOR
                            else
                            {
                                Debug.LogWarning("[ERROR] SSCManager.AddSSCManager() - scene " + _scene.name + " is NOT loaded");
                            }
                            #endif
                            break;
                        }
                    }
                }
            }

            GameObject newManagerGameObject = new GameObject("SSC Manager");
            newManagerGameObject.transform.position = Vector3.zero;
            newManagerGameObject.transform.parent = null;
            SSCManager _manager = newManagerGameObject.AddComponent<SSCManager>();
            _manager.sceneHandle = _manager.gameObject.scene.handle;
            return _manager;
        }

        #endregion

        #region Events

        /// <summary>
        /// This gets called when SSCManager is selected in hierarchy and
        /// scene view is visable both at design time and runtime.
        /// This draws the Path lines in the scene. Locations and Path Location
        /// tangents and control points are drawn in SSCManagerEditor.SceneGUI(..).
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            int numPaths = pathDataList == null ? 0 : pathDataList.Count;
            int _numPathSegments = 4;

            // Remember current colour
            Color gizmosColour = Gizmos.color;

            // Ensure the length of each segment is sensible
            float _segmentLength = pathDisplayResolution < 0.1f ? 0.1f : pathDisplayResolution;

            // Loop through all paths
            for (int pIdx = 0; pIdx < numPaths; pIdx++)
            {
                PathData pathData = pathDataList[pIdx];
                if (pathData != null && pathData.showGizmosInSceneView)
                {
                    int numLocations = pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

                    if (numLocations > 1)
                    {
                        int firstIdx = GetNextPathLocationIndex(pathData, -1, false);
                        int prevIdx = firstIdx;
                        int nextIdx = -1;
                        int lastAssignedIdx = -1; // Used with closed circuit

                        int numSections = 0;

                        // There needs to be at least 2 locations to join together
                        if (firstIdx >= 0 && firstIdx < numLocations - 1)
                        {
                            Vector3 from = pathData.pathLocationDataList[firstIdx].locationData.position;
                            Vector3 to = Vector3.zero;

                            Vector3 segmentFrom, segmentTo = Vector3.zero;

                            Gizmos.color = pathData.pathLineColour;

                            // Loop through all the Locations in the path
                            for (prevIdx = firstIdx; prevIdx < numLocations-1;)
                            {
                                nextIdx = GetNextPathLocationIndex(pathData, prevIdx, false);

                                if (nextIdx < 0) { break; }
                                else
                                {
                                    // Remember last assigned Location for closed circuit
                                    lastAssignedIdx = nextIdx;

                                    PathLocationData pathLlocationData = pathData.pathLocationDataList[nextIdx];
                                    to = pathLlocationData.locationData.position;

                                    // Round up and have min 4 segments per section of the Path
                                    _numPathSegments = (int)(0.5f + pathLlocationData.distanceFromPreviousLocation / _segmentLength);
                                    if (_numPathSegments < 4) { _numPathSegments = 4; }

                                    segmentFrom = from;
                                    for (int sgIdx = 1; sgIdx < _numPathSegments + 1; sgIdx++)
                                    {
                                        if (SSCMath.GetPointOnPath(pathData, prevIdx, (float)sgIdx/_numPathSegments , ref segmentTo))
                                        {
                                            Gizmos.DrawLine(segmentFrom, segmentTo);
                                            segmentFrom = segmentTo;
                                        }
                                    }

                                    numSections++;
                                    from = to;
                                    prevIdx = nextIdx;
                                }
                            }

                            if (pathData.isClosedCircuit && numSections > 1)
                            {
                                // Draw the last section between the last point and the first to complete the circuit.
                                segmentFrom = to;

                                // Round up and have min 4 segments per section of the Path
                                _numPathSegments = (int)(0.5f + pathData.pathLocationDataList[firstIdx].distanceFromPreviousLocation / _segmentLength);
                                if (_numPathSegments < 4) { _numPathSegments = 4; }

                                for (int sgIdx = 1; sgIdx < _numPathSegments + 1; sgIdx++)
                                {
                                    if (SSCMath.GetPointOnPath(pathData, lastAssignedIdx, (float)sgIdx / _numPathSegments, ref segmentTo))
                                    {
                                        Gizmos.DrawLine(segmentFrom, segmentTo);
                                        segmentFrom = segmentTo;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // Reset colour back to original
            Gizmos.color = gizmosColour;
        }

        private void OnDestroy()
        {
            // Remove this manager from the static list of managers
            for (int mIdx = numberManagers - 1; mIdx >= 0; mIdx--)
            {
                if (managerList[mIdx] = this)
                {
                    managerList.RemoveAt(mIdx);
                    numberManagers = managerList.Count;
                }
            }

            #if SSC_ENTITIES && UNITY_2022_2_OR_NEWER
            // blobAssetStore not used in ECS 1.0 entity creation
            //if (blobAssetStore.IsCreated) { blobAssetStore.Dispose(); }
            #elif SSC_ENTITIES && (UNITY_2019_3_OR_NEWER || UNITY_ENTITIES_0_2_0_OR_NEWER)
            if (blobAssetStore != null) { blobAssetStore.Dispose(); }
            #endif
        }

        #endregion

        #region FixedUpdate
#if SSC_ENTITIES
        private void FixedUpdate()
        {
            // Currently the (Job)ComponentSystem's Update occurs late in the Update
            // cycle rather than in FixedUpdate. This may change in the future but for
            // now ProjectileSystem is manually created in Initialise() then updated
            // here where at least one Projectile prefab has DOTS/ECS enabled.
            if (isProjectileSytemUpdateRequired && !isProjectilesPaused && projectileSystem != null)
            {
                projectileSystem.Update();
            }
        }
#endif
        #endregion

        #region Public API Static Methods

        /// <summary>
        /// Returns the current Ship Controller Manager instance for this scene. If one does not already exist, a new one is created.
        /// If the manager is not initialised, it will be initialised.
        /// For multi-additive scenes, pass in the current scene handle e.g., gameObject.scene.handle.
        /// </summary>
        /// <param name="sceneHandle"></param>
        /// <returns></returns>
        public static SSCManager GetOrCreateManager (int sceneHandle = 0)
        {            
            SSCManager _manager = null;

            // Attempt to find the handle in the list of current SSCManagers
            for (int mIdx = numberManagers-1; mIdx >= 0; mIdx--)
            {
                SSCManager _tempManager = managerList[mIdx];
                if (_tempManager != null)
                {
                    // If no scene specified or this is the scene, set the manager
                    if (sceneHandle == 0 || _tempManager.sceneHandle == sceneHandle)
                    {
                        _manager = _tempManager;
                        break;
                    }
                }
                else
                {
                    // If, for some reason this manager is null, remove this slot from the list
                    managerList.RemoveAt(mIdx);
                    numberManagers--;
                }
            }

            // If there wasn't a manager in the list, see if we can find
            // one in the indicated scene.
            if (_manager == null)
            {
                int numScenes = SceneManager.sceneCount;

                for (int sIdx = 0; sIdx < numScenes; sIdx++)
                {
                    if (sceneHandle == 0 || SceneManager.GetSceneAt(sIdx).handle == sceneHandle)
                    {
                        // Get root gameobjects in the correct scene.
                        Scene _scene = SceneManager.GetSceneAt(sIdx);
                        _scene.GetRootGameObjects(tempGameObjectList);

                        int numRootGO = tempGameObjectList.Count;

                        // Assume SSCManager is always a root level object
                        for (int rIdx = 0; rIdx < numRootGO; rIdx++)
                        {
                            if (tempGameObjectList[rIdx].TryGetComponent(out _manager))
                            {
                                if (_manager.sceneHandle == 0) { _manager.sceneHandle = sceneHandle == 0 ? _scene.handle : sceneHandle; }
                                managerList.Add(_manager);
                                numberManagers = managerList.Count;
                                break;
                            }
                        }

                        break;
                    }

                    tempGameObjectList.Clear();
                }

                // If this scene does not already have a manager, create one
                if (_manager == null)
                {
                    _manager = AddSSCManager(sceneHandle);
                    managerList.Add(_manager);
                    numberManagers = managerList.Count;
                }
                //else { Debug.Log("[DEBUG] Found manager"); }
            }

            if (_manager != null)
            {
                // Initialise the manager if it hasn't already been initialised
                if (!_manager.isInitialised) { _manager.Initialise(); }
            }
            #if UNITY_EDITOR
            // If _manager is still null, log a warning to the console
            else
            {
                Debug.LogWarning("SSCManager GetOrCreateManager() Warning: Could not find or create manager, so returned null.");
            }
            #endif

            return _manager;


            //else
            //{
            //    // Default behaviour without multi-scene support like SSC 1.3.7 and earlier.

            //    // Check whether we have already found a manager for this scene
            //    if (currentManager == null)
            //    {
            //        // Otherwise, check whether this scene already has a manager
            //        currentManager = GameObject.FindObjectOfType<SSCManager>();

            //        // If this scene does not already have a manager, create one
            //        if (currentManager == null)
            //        {
            //            currentManager = AddSSCManager();
            //        }
            //    }

            //    if (currentManager != null)
            //    {
            //        // Initialise the manager if it hasn't already been initialised
            //        if (!currentManager.isInitialised) { currentManager.Initialise(); }
            //    }
            //    #if UNITY_EDITOR
            //    // If currentManager is still null, log a warning to the console
            //    else
            //    {
            //        Debug.LogWarning("SSCManager GetOrCreateManager() Warning: Could not find or create manager, so returned null.");
            //    }
            //    #endif

            //    return currentManager;
            //}
        }

        /// <summary>
        /// This method returns the index of the first assigned Location on a Path or -1 if
        /// the path is null, there are no path points, or there are no assigned path points.
        /// Locations on a Path can be assigned to a Location in the scene, or they
        /// can be empty or "unassigned". These unassigned locations are typically ignored for things like
        /// path following.
        /// </summary>
        /// <param name="pathData"></param>
        /// <returns></returns>
        public static int GetFirstAssignedLocationIdx(PathData pathData)
        {
            // Default to no first assigned location
            int firstLocationIdx = -1;

            if (pathData != null && pathData.pathLocationDataList != null)
            {
                int numPathLocations = pathData.pathLocationDataList.Count;

                if (numPathLocations > 0)
                {
                    PathLocationData tempPathLocationData = null;

                    for (int plIdx = 0; plIdx < numPathLocations; plIdx++)
                    {
                        tempPathLocationData = pathData.pathLocationDataList[plIdx];
                        if (tempPathLocationData == null || tempPathLocationData.locationData == null || tempPathLocationData.locationData.isUnassigned) { continue; }
                        else { firstLocationIdx = plIdx; break; }
                    }
                }
            }
            return firstLocationIdx;
        }

        /// <summary>
        /// This method returns the index of the last assigned Location on a Path or -1 if
        /// the path is null, there are no path points, or there are no assigned path points.
        /// Locations on a Path can be assigned to a Location in the scene, or they
        /// can be empty or "unassigned". These unassigned locations are typically ignored for things like
        /// path following.
        /// </summary>
        /// <param name="pathData"></param>
        /// <returns></returns>
        public static int GetLastAssignedLocationIdx(PathData pathData)
        {
            // Default to no first assigned location
            int lastLocationIdx = -1;

            if (pathData != null && pathData.pathLocationDataList != null)
            {
                int numPathLocations = pathData.pathLocationDataList.Count;

                if (numPathLocations > 0)
                {
                    PathLocationData tempPathLocationData = null;

                    for (int plIdx = numPathLocations - 1; plIdx >= 0; plIdx--)
                    {
                        tempPathLocationData = pathData.pathLocationDataList[plIdx];
                        if (tempPathLocationData == null || tempPathLocationData.locationData == null || tempPathLocationData.locationData.isUnassigned) { continue; }
                        else { lastLocationIdx = plIdx; break; }
                    }
                }
            }
            return lastLocationIdx;
        }

        /// <summary>
        /// Get the next Location that has been assigned to a Path, based on the 0-based
        /// index position in list of PathLocationData items. May return null if no next found.
        /// Wraps to start if isWrapEnabled is true.
        /// If currentIdx = -1, it will attempt to find the first assigned Location.
        /// e.g. LocationData locationData = GetNextLocation(pathData, prevIdx, false);
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="currentIdx"></param>
        /// <param name="isWrapEnabled"></param>
        /// <returns></returns>
        public static LocationData GetNextLocation(PathData pathData, int currentIdx, bool isWrapEnabled)
        {
            if (pathData == null || pathData.pathLocationDataList == null) { return null; }
            else
            {
                int numPathLocations = pathData.pathLocationDataList.Count;

                if (currentIdx < -1 || currentIdx > numPathLocations - 1) { return null; }
                // If this is the last Location and isWrapEnbled is false, there is no next Location
                else if (currentIdx == numPathLocations - 1 && !isWrapEnabled) { return null; }
                else
                {
                    // Start at the current position
                    int plIdx = currentIdx;
                    LocationData nextLocationData = null;
                    PathLocationData tempPathLocationData = null;

                    // If starting before the beginning of the Path we can iterate over the whole Path
                    // otherwise don't include the current position.
                    int numIterations = currentIdx < 0 ? numPathLocations : numPathLocations - 1;

                    // Loop through the Path a max of 1 time.
                    for (int i = 0; i < numIterations; i++)
                    {
                        plIdx++;
                        // If this is the last position, should we wrap around to the start?
                        if (plIdx >= numPathLocations)
                        {
                            if (!isWrapEnabled) { break; }
                            else { plIdx = 0; }
                        }

                        tempPathLocationData = pathData.pathLocationDataList[plIdx];
                        if (tempPathLocationData == null || tempPathLocationData.locationData == null || tempPathLocationData.locationData.isUnassigned) { continue; }
                        else { nextLocationData = tempPathLocationData.locationData; break; }
                    }
                    return nextLocationData;
                }
            }
        }

        /// <summary>
        /// Get the next PathLocation index that has been assigned to a Path, based on the 0-based
        /// index position in list of PathLocationData items. May return -1 if no next found.
        /// Wraps to start if isWrapEnabled is true.
        /// If currentIdx = -1, it will attempt to find the first assigned Location.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="currentIdx"></param>
        /// <param name="isWrapEnabled"></param>
        /// <returns></returns>
        public static int GetNextPathLocationIndex(PathData pathData, int currentIdx, bool isWrapEnabled)
        {
            if (pathData == null || pathData.pathLocationDataList == null) { return -1; }
            else
            {
                int numPathLocations = pathData.pathLocationDataList.Count;

                if (currentIdx < -1 || currentIdx > numPathLocations - 1) { return -1; }
                // If this is the last Location and isWrapEnabled is false, there is no next Location
                else if (currentIdx == numPathLocations - 1 && !isWrapEnabled) { return -1; }
                else
                {
                    // Start at the current position
                    int plIdx = currentIdx;
                    int nextLocationIndex = -1;
                    PathLocationData tempPathLocationData = null;

                    // If starting before the beginning of the Path we can iterate over the whole Path
                    // otherwise don't include the current position.
                    int numIterations = currentIdx < 0 ? numPathLocations : numPathLocations - 1;

                    // Loop through the Path a max of 1 time.
                    for (int i = 0; i < numIterations; i++)
                    {
                        plIdx++;
                        // If this is the last position, should we wrap around to the start?
                        if (plIdx >= numPathLocations)
                        {
                            if (!isWrapEnabled) { break; }
                            else { plIdx = 0; }
                        }

                        tempPathLocationData = pathData.pathLocationDataList[plIdx];
                        if (tempPathLocationData == null || tempPathLocationData.locationData == null || tempPathLocationData.locationData.isUnassigned) { continue; }
                        else { nextLocationIndex = plIdx; break; }
                    }
                    return nextLocationIndex;
                }
            }
        }

        /// <summary>
        /// Get the previous Location that has been assigned to a Path, based on the 0-based
        /// index position in list of PathLocationData items. May return null if no previous found.
        /// Wraps to end if isWrapEnabled is true.
        /// If currentIdx == number of Locations in Path, the last assigned Location will be returned.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="currentIdx"></param>
        /// <param name="isWrapEnabled"></param>
        /// <returns></returns>
        public static LocationData GetPreviousLocation(PathData pathData, int currentIdx, bool isWrapEnabled)
        {
            if (pathData == null || pathData.pathLocationDataList == null) { return null; }
            else
            {
                int numPathLocations = pathData.pathLocationDataList.Count;

                // It is possible to start 1 past the end of the Path so that the Last Location can be returned.
                if (currentIdx < 0 || currentIdx > numPathLocations) { return null; }
                // If this is the first Location and isWrapEnabled is false, there is no previous Location
                else if (currentIdx == 0 && !isWrapEnabled) { return null; }
                else
                {
                    // Start at the current position
                    int plIdx = currentIdx;
                    LocationData prevLocationData = null;
                    PathLocationData tempPathLocationData = null;

                    // If starting past the end of the Path we can iterate over the whole Path
                    // otherwise don't include the current position.
                    int numIterations = currentIdx >= numPathLocations ? numPathLocations : numPathLocations - 1;

                    // Loop through the Path a max of 1 time.
                    for (int i = 0; i < numIterations; i++)
                    {
                        --plIdx;
                        // If this is the first position, should we wrap around to the last?
                        if (plIdx < 0)
                        {
                            if (!isWrapEnabled) { break; }
                            else { plIdx = numPathLocations - 1; }
                        }

                        tempPathLocationData = pathData.pathLocationDataList[plIdx];
                        if (tempPathLocationData == null || tempPathLocationData.locationData == null || tempPathLocationData.locationData.isUnassigned) { continue; }
                        else { prevLocationData = tempPathLocationData.locationData; break; }
                    }
                    return prevLocationData;
                }
            }
        }

        /// <summary>
        /// Get the previous PathLocation index that has been assigned to a Path, based on the 0-based
        /// index position in list of PathLocationData items. May return -1 if no previous found.
        /// Wraps to end if isWrapEnabled is true.
        /// If currentIdx == number of Locations in Path, the last assigned Location will be returned.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="currentIdx"></param>
        /// <param name="isWrapEnabled"></param>
        /// <returns></returns>
        public static int GetPreviousPathLocationIndex(PathData pathData, int currentIdx, bool isWrapEnabled)
        {
            if (pathData == null || pathData.pathLocationDataList == null) { return -1; }
            else
            {
                int numPathLocations = pathData.pathLocationDataList.Count;

                // It is possible to start 1 past the end of the Path so that the Last Location can be returned.
                if (currentIdx < 0 || currentIdx > numPathLocations) { return -1; }
                // If this is the first Location and isWrapEnabled is false, there is no previous Location
                else if (currentIdx == 0 && !isWrapEnabled) { return -1; }
                else
                {
                    // Start at the current position
                    int plIdx = currentIdx;
                    int prevLocationIdx = -1;
                    PathLocationData tempPathLocationData = null;

                    // If starting past the end of the Path we can iterate over the whole Path
                    // otherwise don't include the current position.
                    int numIterations = currentIdx >= numPathLocations ? numPathLocations : numPathLocations - 1;

                    // Loop through the Path a max of 1 time.
                    for (int i = 0; i < numIterations; i++)
                    {
                        --plIdx;
                        // If this is the first position, should we wrap around to the last?
                        if (plIdx < 0)
                        {
                            if (!isWrapEnabled) { break; }
                            else { plIdx = numPathLocations - 1; }
                        }

                        tempPathLocationData = pathData.pathLocationDataList[plIdx];
                        if (tempPathLocationData == null || tempPathLocationData.locationData == null || tempPathLocationData.locationData.isUnassigned) { continue; }
                        else { prevLocationIdx = plIdx; break; }
                    }
                    return prevLocationIdx;
                }
            }
        }

        /// <summary>
        /// Import a json file from disk and return as PathData
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static PathData ImportPathDataFromJson(string folderPath, string fileName)
        {
            PathData pathData = null;

            if (!string.IsNullOrEmpty(folderPath) && !string.IsNullOrEmpty(fileName))
            {
                try
                {
                    string filePath = System.IO.Path.Combine(folderPath, fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        string jsonText = System.IO.File.ReadAllText(filePath);

                        pathData = new PathData();
                        int pathGuidHash = pathData.guidHash;

                        JsonUtility.FromJsonOverwrite(jsonText, pathData);

                        if (pathData != null)
                        {
                            // make hash code unique
                            pathData.guidHash = pathGuidHash;

                            int numPathLocations = pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

                            // Give PathLocationData unique hash codes
                            for (int lIdx = 0; lIdx < numPathLocations; lIdx++)
                            {
                                pathData.pathLocationDataList[lIdx].guidHash = SSCMath.GetHashCodeFromGuid();
                            }
                        }
                    }
                    #if UNITY_EDITOR
                    else
                    {
                        Debug.LogWarning("ERROR: Import PataData. Could not find file at " + filePath);
                    }
                    #endif
                }
                catch (System.Exception ex)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SSCManager - could not import path data from: " + folderPath + " PLEASE REPORT - " + ex.Message);
                    #else
                    // Keep compiler happy
                    if (ex != null) { }
                    #endif
                }
            }

            return pathData;
        }

        /// <summary>
        /// Save the PathData to a json file on disk.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="filePath"></param>
        public static bool SavePathDataAsJson(PathData pathData, string filePath)
        {
            bool isSuccessful = false;

            if (pathData != null && !string.IsNullOrEmpty(filePath))
            {
                try
                {
                    string jsonPathData = JsonUtility.ToJson(pathData);

                    if (!string.IsNullOrEmpty(jsonPathData) && !string.IsNullOrEmpty(filePath))
                    {
                        System.IO.File.WriteAllText(filePath, jsonPathData);
                        isSuccessful = true;
                    }
                }
                catch (System.Exception ex)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SSCManager - could not export: " + pathData.name + " PLEASE REPORT - " + ex.Message);
                    #else
                    // Keep compiler happy
                    if (ex != null) { }
                    #endif
                }
            }

            return isSuccessful;
        }

        /// <summary>
        /// Get the Path distance between two Locations. If the Location index positions in the Path
        /// are invalid, the method will return 0. If pathLocationIndex2 is less than pathLocationIndex1,
        /// assume the path wraps around to the start again.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="pathLocationIndex1"></param>
        /// <param name="pathLocationIndex2"></param>
        /// <returns></returns>
        public static float GetPathDistance(PathData pathData, int pathLocationIndex1, int pathLocationIndex2)
        {
            float deltaDistance = 0f;

            if (pathData != null && pathLocationIndex1 != pathLocationIndex2)
            {
                int numPathLocations = pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

                // Check locationData indexes are valid
                if (pathLocationIndex1 >= 0 && pathLocationIndex1 < numPathLocations && pathLocationIndex2 >= 0 && pathLocationIndex2 < numPathLocations)
                {
                    PathLocationData pathLocationData1 = pathData.pathLocationDataList[pathLocationIndex1];
                    PathLocationData pathLocationData2 = pathData.pathLocationDataList[pathLocationIndex2];

                    // Check Locations are valid
                    if (pathLocationData1 != null && pathLocationData2 != null)
                    {
                        // Check if locations are first Path points. If this is a closed circuit, the first Path point will store the total distance of the Path in distanceCumulative.
                        if (pathLocationIndex2 > pathLocationIndex1)
                        {
                            deltaDistance = (pathLocationIndex2 == 0 ? 0f : pathLocationData2.distanceCumulative) - (pathLocationIndex1 == 0 ? 0f : pathLocationData1.distanceCumulative);
                        }
                        else
                        {
                            // Distance to the end of the path + distance from start of path to second Location
                            deltaDistance = pathData.splineTotalDistance - (pathLocationIndex1 == 0 ? 0f : pathLocationData1.distanceCumulative) + (pathLocationIndex2 == 0 ? 0f : pathLocationData2.distanceCumulative);
                        }
                    }
                }
            }

            return deltaDistance;
        }

        #endregion

        #region Public API Member Methods - Beam, Destruct, Projectile and Effects Objects

        /// <summary>
        /// Get the effects pool for this prefab or create a new pool if one does not already exist.
        /// Return the effectsObjectPrefabID for the pool or SSCManager.NoPrefabID.
        /// See also InstantiateEffectsObject(ieParms), and InstantiateSoundFX(..).
        /// </summary>
        /// <param name="effectsObjectPrefab"></param>
        /// <returns></returns>
        public int GetorCreateEffectsPool (EffectsModule effectsObjectPrefab)
        {
            int effectsObjectTemplateIndex = NoPrefabID;

            if (effectsObjectPrefab != null)
            {
                if (effectsObjectPrefab.usePooling)
                {
                    // Get the transform instance ID for this effects object prefab
                    int effectsObjectTransformID = effectsObjectPrefab.transform.GetInstanceID();
                    // Search the effects object templates list to see if we already have an 
                    // effects object prefab with the same instance ID
                    effectsObjectTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsObjectTransformID);

                    if (effectsObjectTemplateIndex == NoPrefabID)
                    {
                        // If no match was found, create a new effects object template for this prefab
                        effectsObjectTemplateIndex = AddEffectsObjectTemplate(effectsObjectPrefab, effectsObjectTransformID);
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("ERROR: sscManager.GetorCreateEffectsPool - Use Pooling is not enabled on " + effectsObjectPrefab.gameObject.name);
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: sscManager.GetorCreateEffectsPool - the effectsObjectPrefab is null");
            }
            #endif

            // return the effectsObjectPrefabID
            return effectsObjectTemplateIndex;
        }

        /// <summary>
        /// Create any Effects pools that have not already been created using
        /// a set of EffectsModule prefabs.
        /// Populate a pre-created array of EffectsTemplate prefabIDs. The
        /// length of effectsPrefabIDs must match number of effects slots.
        /// </summary>
        public bool CreateEffectsPools (SSCSoundFXSet sscEffectsSet, int[] effectsPrefabIDs)
        {
            int numEffects = sscEffectsSet == null || sscEffectsSet.effectsModuleList == null ? 0 : sscEffectsSet.effectsModuleList.Count;

            int numEffectsPrefabIDs = effectsPrefabIDs == null ? 0 : effectsPrefabIDs.Length;

            if (numEffects != numEffectsPrefabIDs) { return false; }
            else
            {
                for (int dcIdx = 0; dcIdx < numEffects; dcIdx++)
                {
                    effectsPrefabIDs[dcIdx] = GetorCreateEffectsPool(sscEffectsSet.effectsModuleList[dcIdx]);

                    #if UNITY_EDITOR
                    if (effectsPrefabIDs[dcIdx] == NoPrefabID)
                    {
                        Debug.LogWarning("ERROR SSCManager.CreateEffectsPools(..) " + sscEffectsSet.name + ", item " + (dcIdx+1).ToString("00") + ", does not contain a valid EffectsModule prefab.");
                    }
                    #endif
                }

                return true;
            }
        }

        /// <summary>
        /// Returns the prefab for a beam given its beam prefab ID.
        /// </summary>
        /// <param name="beamPrefabID"></param>
        /// <returns></returns>
        public BeamModule GetBeamPrefab (int beamPrefabID)
        {
            // Check that a valid beam prefab ID was supplied
            if (beamPrefabID >= 0 && beamPrefabID < beamTemplateList.Count)
            {
                return beamTemplateList[beamPrefabID].beamPrefab;
            }
            else { return null; }
        }

        /// <summary>
        /// Returns the prefab for an EffectsObject given its effects prefab ID.
        /// </summary>
        /// <param name="effectsObjectPrefabID"></param>
        /// <returns></returns>
        public EffectsModule GetEffectsObjectPrefab (int effectsObjectPrefabID)
        {
            // Check that a valid effects object prefab ID was supplied
            if (effectsObjectPrefabID >= 0 && effectsObjectPrefabID < effectsObjectTemplatesList.Count)
            {
                return effectsObjectTemplatesList[effectsObjectPrefabID].effectsObjectPrefab;
            }
            else { return null; }
        }

        /// <summary>
        /// Pause all Pooled, and Non-Pooled Beams in the scene
        /// </summary>
        public void PauseBeams()
        {
            PauseBeams(true);
        }

        /// <summary>
        /// Resume all Pooled, and Non-Pooled beams in the scene
        /// </summary>
        public void ResumeBeams()
        {
            PauseBeams(false);
        }

        /// <summary>
        /// Pause all Pooled, and Non-Pooled Destruct objects in the scene
        /// </summary>
        public void PauseDestructs()
        {
            PauseDestructModules(true);
        }

        /// <summary>
        /// Resume all Pooled, and Non-Pooled Destruct objects in the scene
        /// </summary>
        public void ResumeDestructs()
        {
            PauseDestructModules(false);
        }

        /// <summary>
        /// Returns the prefab for a projectile given its projectile prefab ID.
        /// </summary>
        /// <param name="projectileTemplateIndex"></param>
        /// <returns></returns>
        public ProjectileModule GetProjectilePrefab (int projectilePrefabID)
        {
            // Check that a valid projectile prefab ID was supplied
            if (projectilePrefabID < projectileTemplatesList.Count)
            {
                return projectileTemplatesList[projectilePrefabID].projectilePrefab;
            }
            else { return null; }
        }

        /// <summary>
        /// Pause all DOTS, Pooled, and Non-Pooled Projectiles in the scene
        /// </summary>
        public void PauseProjectiles()
        {
            PauseProjectiles(true);
        }

        /// <summary>
        /// Resume all DOTS, Pooled, and Non-Pooled Projectiles in the scene
        /// </summary>
        public void ResumeProjectiles()
        {
            PauseProjectiles(false);
        }

        /// <summary>
        /// Pause all Pooled and Non-Pooled Effects Objects in the scene
        /// </summary>
        public void PauseEffectsObjects()
        {
            PauseEffectsObjects(true);
        }

        /// <summary>
        /// Resume all Pooled and Non-Pooled Effects Objects in the scene
        /// </summary>
        public void ResumeEffectsObjects()
        {
            PauseEffectsObjects(false);
        }

        /// <summary>
        /// Instantiate a pooled EffectsModule which contains an audio source. If audioClip is null,
        /// the existing one (if there is one) attached to the prefab will be used.
        /// See also GetorCreateEffectsPool(..).
        /// </summary>
        /// <param name="sfxParms"></param>
        /// <param name="audioClip"></param>
        public void InstantiateSoundFX (InstantiateSoundFXParameters sfxParms, AudioClip audioClip)
        {
            if (isInitialised)
            {
                if (sfxParms.effectsObjectPrefabID >= 0 && sfxParms.effectsObjectPrefabID < effectsObjectTemplatesList.Count)
                {
                    // Get the effects object template using its ID (this is just the index of it in the effects object template list)
                    effectsObjectTemplate = effectsObjectTemplatesList[sfxParms.effectsObjectPrefabID];

                    if (effectsObjectTemplate.effectsObjectPrefab.usePooling)
                    {
                        // If we are using pooling, find the first inactive effects object
                        if (effectsObjectTemplate.effectsObjectPool == null)
                        {
                            #if UNITY_EDITOR
                            Debug.LogWarning("SSCManager InstantiateSoundFX() effectsObjectPool is null. We do not support changing to usePooling for an effects object at runtime.");
                            #endif
                        }
                        else
                        {
                            EffectsModule _effectsModule = null;

                            int firstInactiveIndex = effectsObjectTemplate.effectsObjectPool.FindIndex(e => !e.activeInHierarchy);
                            if (firstInactiveIndex == -1)
                            {
                                // All effects objects in the pool are currently active
                                // So if we want to get a new one, we'll need to add a new one to the pool
                                // First check if we have reached the max pool size
                                if (effectsObjectTemplate.currentPoolSize < effectsObjectTemplate.effectsObjectPrefab.maxPoolSize)
                                {
                                    // If we are still below the max pool size, add a new effects object instance to the pool
                                    // Instantiate the effects object with the correct position and rotation
                                    effectsObjectGameObjectInstance = Instantiate(effectsObjectTemplate.effectsObjectPrefab.gameObject,
                                        sfxParms.position, Quaternion.identity);
                                    // Set the object's parent to be the manager
                                    effectsObjectGameObjectInstance.transform.SetParent(effectsPooledTrfm);
                                    firstInactiveIndex = effectsObjectTemplate.currentPoolSize;
                                    // Add the object to the list of pooled objects
                                    effectsObjectTemplate.effectsObjectPool.Add(effectsObjectGameObjectInstance);
                                    // Update the current pool size counter
                                    effectsObjectTemplate.currentPoolSize++;                                     
                                }
                            }
                            else
                            {
                                // Get the effects object
                                effectsObjectGameObjectInstance = effectsObjectTemplate.effectsObjectPool[firstInactiveIndex];
                                // Position the object
                                effectsObjectGameObjectInstance.transform.SetPositionAndRotation(sfxParms.position, Quaternion.identity);
                                // Set the object to active
                                effectsObjectGameObjectInstance.SetActive(true);
                            }

                            if (firstInactiveIndex >= 0)
                            {
                                // Initialise the effects object
                                _effectsModule = effectsObjectGameObjectInstance.GetComponent<EffectsModule>();

                                AudioSource audioSource = _effectsModule.GetAudioSource();

                                if (audioSource != null)
                                {
                                    // Replace the clip?
                                    if (audioClip != null)
                                    {
                                        _effectsModule.SetAudioClip(audioClip);
                                    }

                                    // Override the prefab volume?
                                    if (sfxParms.useDefaultVolume) { audioSource.volume = _effectsModule.defaultVolume; }
                                    else if (sfxParms.volume > 0f && sfxParms.volume <= 1f) { audioSource.volume = sfxParms.volume; }

                                    _effectsModule.InitialiseEffectsObject();

                                    //sfxParms.effectsObjectSequenceNumber = _effectsModule.InitialiseEffectsObject();
                                    //sfxParms.effectsObjectPoolListIndex = firstInactiveIndex;
                                }
                            }
                        }
                    }
                }
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("SSCManager InstantiateSoundFX() Warning: Method was called before the manager was initialised.");
            }
            #endif
        }

        /// <summary>
        /// Play an audio clip at the specified world-space position at a volume.
        /// This uses the pooled EffectsModules created with GetOrCreateEffectsPool(..).
        /// See also InstantiateSoundFX(..).
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="audioPosition"></param>
        /// <param name="clipVolume"></param>
        public void PlaySoundFX (int sfxObjectPrefabID, AudioClip audioClip, Vector3 audioPosition, float clipVolume)
        {
            // Check to see if the sound fx pool was created ok and the clip is not null.
            if (sfxObjectPrefabID >= 0 && audioClip != null)
            {
                // Play the sound clip using a pool EffectsModule
                InstantiateSoundFXParameters sfxParms = new InstantiateSoundFXParameters()
                {
                    effectsObjectPrefabID = sfxObjectPrefabID,
                    position = audioPosition,
                    volume = clipVolume
                };
                InstantiateSoundFX(sfxParms, audioClip);
            }
        }

        /// <summary>
        /// Teleport or instantly move all active beams by an amount
        /// in the x, y and z directions. This could be useful if changing
        /// the origin or centre of your world to compensate for float-point
        /// error.
        /// </summary>
        /// <param name="delta"></param>
        public void TelePortBeams(Vector3 delta)
        {
            int numBeamTemplates = beamTemplateList == null ? 0 : beamTemplateList.Count;

            // Teleport all active Pooled Beams
            for (int ptIdx = 0; ptIdx < numBeamTemplates; ptIdx++)
            {
                BeamTemplate _beamTemplate = beamTemplateList[ptIdx];
                if (_beamTemplate != null && _beamTemplate.beamPrefab != null && _beamTemplate.beamPrefab.usePooling)
                {
                    // Examine each of the beams in the pool
                    for (int pmIdx = 0; pmIdx < _beamTemplate.currentPoolSize; pmIdx++)
                    {
                        GameObject pmGO = _beamTemplate.beamPoolList[pmIdx];
                        if (pmGO.activeInHierarchy)
                        {
                            pmGO.transform.position += delta;
                            BeamModule beamModule = pmGO.GetComponent<BeamModule>();

                            if (beamModule.isInitialised && beamModule.isBeamEnabled)
                            {
                                beamModule.transform.position += delta;
                                beamModule.lineRenderer.SetPosition(0, beamModule.lineRenderer.GetPosition(0) + delta);
                                beamModule.lineRenderer.SetPosition(1, beamModule.lineRenderer.GetPosition(1) + delta);
                            }
                        }
                    }
                }
            }

            // Teleport all active Non-Pooled Beams (they should probably all be active in the heierarchy)
            if (beamNonPooledTrfm != null)
            {
                BeamModule[] _beamModules = beamNonPooledTrfm.GetComponentsInChildren<BeamModule>(false);
                int _numBeamModules = _beamModules == null ? 0 : _beamModules.Length;

                for (int pmIdx = 0; pmIdx < _numBeamModules; pmIdx++)
                {
                    BeamModule beamModule = _beamModules[pmIdx];

                    if (beamModule.isInitialised && beamModule.isBeamEnabled)
                    {
                        beamModule.transform.position += delta;
                        beamModule.lineRenderer.SetPosition(0, beamModule.lineRenderer.GetPosition(0) + delta);
                        beamModule.lineRenderer.SetPosition(1, beamModule.lineRenderer.GetPosition(1) + delta);
                    }
                }
            }
        }

        /// <summary>
        /// Teleport or instantly move all active projectiles by an amount
        /// in the x, y and z directions. This could be useful if changing
        /// the origin or centre of your world to compensate for float-point
        /// error.
        /// </summary>
        /// <param name="delta"></param>
        public void TelePortProjectiles(Vector3 delta)
        {
            //if (isInitialised)
            {
                int numProjectileTemplates = projectileTemplatesList == null ? 0 : projectileTemplatesList.Count;

                // Teleport all active Pooled Projectiles
                for (int ptIdx = 0; ptIdx < numProjectileTemplates; ptIdx++)
                {
                    ProjectileTemplate _projectileTemplate = projectileTemplatesList[ptIdx];
                    if (_projectileTemplate != null && _projectileTemplate.projectilePrefab != null && _projectileTemplate.projectilePrefab.usePooling)
                    {
                        // Examine each of the projectiles in the pool
                        for (int pmIdx = 0; pmIdx < _projectileTemplate.currentPoolSize; pmIdx++)
                        {
                            GameObject pmGO = _projectileTemplate.projectilePool[pmIdx];
                            if (pmGO.activeInHierarchy)
                            {
                                pmGO.transform.position += delta;
                                pmGO.GetComponent<ProjectileModule>().LastFramePosition += delta;
                                pmGO.GetComponent<ProjectileModule>().ThisFramePosition += delta;
                            }
                        }
                    }
                }

                // Teleport all active Non-Pooled Projectiles (they should probably all be active in the heierarchy)
                if (projectileNonPooledTrfm != null)
                {
                    ProjectileModule[] _projectileModules = projectileNonPooledTrfm.GetComponentsInChildren<ProjectileModule>(false);
                    int _numProjectileModules = _projectileModules == null ? 0 : _projectileModules.Length;

                    for (int pmIdx = 0; pmIdx < _numProjectileModules; pmIdx++)
                    {
                        _projectileModules[pmIdx].transform.position += delta;
                        _projectileModules[pmIdx].LastFramePosition += delta;
                        _projectileModules[pmIdx].ThisFramePosition += delta;
                    }
                }

                // Teleport DOTS projectiles
                #if SSC_ENTITIES
                // Are their potentially any DOTS projectiles to teleport?
                if (isProjectileSytemUpdateRequired && !isProjectilesPaused && projectileSystem != null)
                {
                    projectileSystem.TelePortProjectiles(delta);
                }
                #endif
            }
        }

        #endregion

        #region Public API Member Methods - Paths and Locations

        /// <summary>
        /// Add a new Location in the scene. The Location can then be added to one or more Paths.
        /// Locations can be used with SSC AI and can appear on radar.
        /// </summary>
        /// <param name="locationData"></param>
        /// <returns></returns>
        public bool AddLocation (LocationData locationData)
        {
            if (locationDataList == null) { locationDataList = new List<LocationData>(10); }
            if (locationDataList != null && locationData != null)
            {
                locationData.isUnassigned = false;
                locationDataList.Add(locationData);
                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// Same as calling AppendLocation(wsPosition, autoGenerateName)
        /// </summary>
        /// <param name="wsPosition"></param>
        /// <param name="autoGenerateName"></param>
        /// <returns></returns>
        public LocationData AddLocation (Vector3 wsPosition, bool autoGenerateName)
        {
            return AppendLocation(wsPosition, autoGenerateName);
        }

        /// <summary>
        /// Add a new Location in the scene. The Location can then be added to one or more Paths.
        /// Locations can be used with SSC AI and can appear on radar.
        /// If autoGenerateName is true, GC will be impacted.
        /// </summary>
        /// <param name="wsPosition"></param>
        /// <param name="autoGenerateName"></param>
        /// <returns></returns>
        public LocationData AppendLocation(Vector3 wsPosition, bool autoGenerateName)
        {
            LocationData locationData = new LocationData()
            {
                position = wsPosition
            };

            if (locationDataList == null) { locationDataList = new List<LocationData>(10); }
            if (locationDataList != null)
            {
                locationData.isUnassigned = false;
                if (autoGenerateName) { locationData.name = "Location " + (locationDataList.Count+1).ToString(); }
                locationDataList.Add(locationData);
            }

            return locationData;
        }

        /// <summary>
        /// Add a new Location in the scene and add it to the Path.
        /// If autoGenerateName is true, GC will be impacted.
        /// refreshPath will update the path distances.
        /// If adding many Locations, set refreshPath to false and call RefreshPathDistances(..)
        /// for each Path manually.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="wsPosition"></param>
        /// <param name="autoGenerateName"></param>
        /// <param name="refreshPath"></param>
        /// <returns></returns>
        public LocationData AppendLocation(PathData pathData, Vector3 wsPosition, bool autoGenerateName, bool refreshPath)
        {
            LocationData locationData = AppendLocation(wsPosition, false);

            if (locationData != null && pathData != null)
            {
                if (pathData.pathLocationDataList == null) { pathData.pathLocationDataList = new List<PathLocationData>(10); }
                if (pathData.pathLocationDataList != null)
                {
                    PathLocationData pathLocationData = new PathLocationData();
                    if (pathLocationData != null)
                    {
                        if (autoGenerateName)
                        {
                            // Give the new location a default name
                            locationData.name = (string.IsNullOrEmpty(pathData.name) ? "unknown" : pathData.name) + " " + (pathData.pathLocationDataList.Count+1).ToString();
                        }
                        pathLocationData.locationData = locationData;
                        pathLocationData.locationData.isUnassigned = false;
                        // Set the default tangent control points

                        // Get the last assigned point on the Path before the new Location is added
                        int prevLocationIdx = GetPreviousPathLocationIndex(pathData, pathData.pathLocationDataList.Count, pathData.isClosedCircuit);

                        if (prevLocationIdx >= 0)
                        {
                            LocationData prevLocation = pathData.pathLocationDataList[prevLocationIdx].locationData;

                            // Get the midpoint between the previous point and the new one
                            float midPointX = (wsPosition.x + prevLocation.position.x) / 2f;
                            float midPointY = (wsPosition.y + prevLocation.position.y) / 2f;
                            float midPointZ = (wsPosition.z + prevLocation.position.z) / 2f;
                            pathLocationData.inControlPoint.x = midPointX;
                            pathLocationData.inControlPoint.y = midPointY;
                            pathLocationData.inControlPoint.z = midPointZ;
                            
                            pathLocationData.outControlPoint = wsPosition + ((wsPosition - prevLocation.position) / 2f);

                            // Attempt to automatically fix control points on first point when adding second point
                            if (prevLocationIdx == 0)
                            {
                                PathLocationData firstLocationData = pathData.pathLocationDataList[prevLocationIdx];
                                if (firstLocationData != null)
                                {
                                    firstLocationData.outControlPoint = pathLocationData.inControlPoint;
                                    firstLocationData.inControlPoint = firstLocationData.locationData.position + ((firstLocationData.locationData.position - pathLocationData.inControlPoint).normalized * defaultPathControlOffset);
                                }
                            }
                        }
                        else
                        {
                            // default to left 2m and right 2m from position
                            pathLocationData.inControlPoint = wsPosition;
                            pathLocationData.inControlPoint.x -= defaultPathControlOffset;
                            pathLocationData.outControlPoint = wsPosition;
                            pathLocationData.outControlPoint.x += defaultPathControlOffset;
                        }

                        pathData.pathLocationDataList.Add(pathLocationData);

                        if (refreshPath) { RefreshPathDistances(pathData); }
                        else { pathData.isDirty = true; }
                    }
                }
            }

            return locationData;
        }

        /// <summary>
        /// Add a Location to the end of the Path by placing at the last Location on the Path.
        /// Deselects the last Location and selects the new Location.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="autoGenerateName"></param>
        /// <param name="refreshPath"></param>
        /// <returns></returns>
        public LocationData AppendLocation(PathData pathData, bool autoGenerateName, bool refreshPath)
        {
            LocationData locationData = null;

            int numPathPoints = pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

            if (numPathPoints > 0)
            {
                // Get the last assigned Location in the Path
                int lastIdx = GetPreviousPathLocationIndex(pathData, numPathPoints, false);

                if (lastIdx >= 0)
                {
                    PathLocationData lastPathLocationData = pathData.pathLocationDataList[lastIdx];

                    // Add a new Location to the end of the Path (don't refresh the Path before Control points are updated)
                    locationData = AppendLocation(pathData, lastPathLocationData.locationData.position, autoGenerateName, false);
                    if (locationData != null)
                    {
                        // Deselect the last Location
                        lastPathLocationData.locationData.selectedInSceneView = false;
                        lastPathLocationData.inControlSelectedInSceneView = false;
                        lastPathLocationData.outControlSelectedInSceneView = false;

                        locationData.selectedInSceneView = true;
                        PathLocationData newPathLocationData = pathData.pathLocationDataList[pathData.pathLocationDataList.Count-1];
                        newPathLocationData.inControlPoint = lastPathLocationData.inControlPoint;
                        newPathLocationData.outControlPoint = lastPathLocationData.outControlPoint;

                        if (refreshPath) { RefreshPathDistances(pathData); }
                        else { pathData.isDirty = true; }
                    }
                }
            }

            return locationData;
        }

        /// <summary>
        /// Add a new Location in the scene. The Location can then be added to one or more Paths.
        /// Locations can be used with SSC AI.
        /// </summary>
        /// <param name="locationGameObject"></param>
        /// <returns></returns>
        public LocationData AppendLocation(GameObject locationGameObject)
        {
            if (locationGameObject != null)
            {
                LocationData locationData = new LocationData()
                {
                    position = locationGameObject.transform.position,
                    // The gameobject instance id is not serialized
                    gameObjectInstanceId = locationGameObject.GetInstanceID()
                };

                if (locationDataList == null) { locationDataList = new List<LocationData>(10); }
                if (locationDataList != null)
                {
                    locationData.isUnassigned = false;
                    locationData.name = locationGameObject.name;
                    locationDataList.Add(locationData);
                }
                return locationData;
            }
            else { return null; }
        }

        /// <summary>
        /// Add a path to the scene. Alternatively, use CreatePath().
        /// </summary>
        /// <param name="newPath"></param>
        public void AddPath(PathData newPath)
        {
            if (newPath != null) { pathDataList.Add(newPath); }
            #if UNITY_EDITOR
            else { Debug.LogWarning("SSCManager.AddPath - the PathData parameter is null"); }
            #endif
        }

        /// <summary>
        /// Create a new path and return the reference to the PathData.
        /// </summary>
        /// <returns></returns>
        public PathData CreatePath()
        {
            PathData newPath = new PathData();
            if (newPath != null) { pathDataList.Add(newPath); }
            return newPath;
        }

        /// <summary>
        /// Delete a location and remove from radar if required
        /// </summary>
        /// <param name="locationData"></param>
        public void DeleteLocation(LocationData locationData)
        {
            if (locationData != null)
            {
                // De-select all locations
                int numLocations = locationDataList == null ? 0 : locationDataList.Count;

                for (int idx = 0; idx < numLocations; idx++)
                {
                    if (locationDataList[idx] != null) { locationDataList[idx].selectedInSceneView = false; }
                }

                locationData.selectedInSceneView = true;

                // Do we need to tell the radar system to stop tracking this Location?
                if (isInitialised && locationData.isRadarEnabled && locationData.RadarId >= 0 && sscRadar != null && sscRadar.IsInitialised)
                {
                    sscRadar.RemoveItem(locationData.RadarId);
                    locationData.isRadarEnabled = false;
                    locationData.radarItemIndex = -1;
                }

                DeleteSelectedLocations(true);
            }
        }

        /// <summary>
        /// Insert a new Location before the zero-based point on the Path.
        /// Has no effect if there are not no points in the Path.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="insertIndex"></param>
        /// <param name="autoGenerateName"></param>
        /// <param name="refreshPath"></param>
        /// <returns></returns>
        public LocationData InsertLocationAt (PathData pathData, int insertIndex, bool autoGenerateName, bool refreshPath)
        {
            LocationData locationData = null;
            LocationData prevLocation = null;
            LocationData nextLocation = null;

            if (pathData != null)
            {
                if (pathData.pathLocationDataList == null) { pathData.pathLocationDataList = new List<PathLocationData>(10); }

                int numPathLocations = pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

                if (numPathLocations > 0)
                {
                    // If attempting to insert past the end of the path, simply append.
                    if (insertIndex > numPathLocations)
                    {
                        locationData = AppendLocation(pathData, autoGenerateName, refreshPath);
                    }
                    else if (insertIndex >= 0)
                    {
                        // Get the prev assigned point on the Path
                        prevLocation = GetPreviousLocation(pathData, insertIndex, pathData.isClosedCircuit);
                        // The "next" location will be the insert point
                        nextLocation = pathData.pathLocationDataList[insertIndex].locationData;

                        if (prevLocation == null || nextLocation == null)
                        {
                            // Insert at beginning of path
                            locationData = AppendLocation(pathData.pathLocationDataList[0].locationData.position, false);
                        }
                        else
                        {
                            // Get the midpoint between the previous point and the next one
                            Vector3 wsPosition = new Vector3();
                            wsPosition.x = (nextLocation.position.x + prevLocation.position.x) / 2f;
                            wsPosition.y = (nextLocation.position.y + prevLocation.position.y) / 2f;
                            wsPosition.z = (nextLocation.position.z + prevLocation.position.z) / 2f;

                            locationData = AppendLocation(wsPosition, false);
                        }
                    }

                    // If we created a Location, now insert it into the Path
                    if (locationData != null)
                    {
                        // Unselect all locations on the path
                        SelectPathLocations(pathData, false);

                        PathLocationData pathLocationData = new PathLocationData();

                        if (pathLocationData != null)
                        {
                            if (autoGenerateName)
                            {
                                // Give the new location a default (inserted) name
                                locationData.name = (string.IsNullOrEmpty(pathData.name) ? "unknown" : pathData.name) + " " + insertIndex.ToString() + "a";
                            }

                            pathLocationData.locationData = locationData;
                            pathLocationData.locationData.isUnassigned = false;

                            Vector3 wsPosition = locationData.position;

                            // Set the default tangent control points
                            if (prevLocation != null)
                            {
                                // Get the midpoint between the previous point and the new one
                                float midPointX = (wsPosition.x + prevLocation.position.x) / 2f;
                                float midPointY = (wsPosition.y + prevLocation.position.y) / 2f;
                                float midPointZ = (wsPosition.z + prevLocation.position.z) / 2f;
                                pathLocationData.inControlPoint.x = midPointX;
                                pathLocationData.inControlPoint.y = midPointY;
                                pathLocationData.inControlPoint.z = midPointZ;

                                pathLocationData.outControlPoint = wsPosition + ((wsPosition - prevLocation.position) / 2f);
                            }
                            else if (nextLocation != null)
                            {
                                // Get the midpoint between the insert point and the new one
                                float midPointX = (wsPosition.x + nextLocation.position.x) / 2f;
                                float midPointY = (wsPosition.y + nextLocation.position.y) / 2f;
                                float midPointZ = (wsPosition.z + nextLocation.position.z) / 2f;
                                pathLocationData.inControlPoint.x = midPointX;
                                pathLocationData.inControlPoint.y = midPointY;
                                pathLocationData.inControlPoint.z = midPointZ;

                                pathLocationData.outControlPoint = wsPosition + ((wsPosition - nextLocation.position) / 2f);
                            }
                            else
                            {
                                // default to left 2m and right 2m from position
                                pathLocationData.inControlPoint = wsPosition;
                                pathLocationData.inControlPoint.x -= defaultPathControlOffset;
                                pathLocationData.outControlPoint = wsPosition;
                                pathLocationData.outControlPoint.x += defaultPathControlOffset;
                            }

                            if (insertIndex < numPathLocations)
                            {
                                pathData.pathLocationDataList.Insert(insertIndex, pathLocationData);
                            }
                            else { pathData.pathLocationDataList.Add(pathLocationData); }

                            if (refreshPath) { RefreshPathDistances(pathData); }
                            else { pathData.isDirty = true; }
                        }
                        else { locationData = null; }
                    }
                }
            }
            return locationData;
        }

        /// <summary>
        /// Update the Location's position. This also updates tangent control points for
        /// any Paths that the Location is a member of.
        /// refreshPath will update the path distances for all affected Paths.
        /// If updating many Locations, set refreshPath to false and call RefreshPathDistances(..)
        /// for each Path manually.
        /// If enabled for radar, update the radar item.
        /// </summary>
        /// <param name="locationData"></param>
        /// <param name="wsPosition"></param>
        /// <param name="refreshPath"></param>
        public void UpdateLocation(LocationData locationData, Vector3 wsPosition, bool refreshPath)
        {
            Vector3 currentPosition = locationData.position;
            Vector3 deltaPosition = wsPosition - currentPosition;
            bool isPathAffected = false;

            // Update the Location before updating Paths so that it is correct
            // when Path Distances are refreshed.
            locationData.position = wsPosition;

            // Check to see if radar is enabled, and update the wsPosition
            // NOTE: This hasn't been fully tested...
            if (locationData.isRadarEnabled && locationData.RadarId >= 0 && isInitialised && sscRadar != null && sscRadar.IsInitialised)
            {
                sscRadar.SetItemPosition(locationData.radarItemIndex, wsPosition);
            }

            int numPaths = pathDataList == null ? 0 : pathDataList.Count;
            for (int pIdx = 0; pIdx < numPaths; pIdx++)
            {
                PathData _pathData = pathDataList[pIdx];
                if (_pathData != null)
                {
                    isPathAffected = false;
                    List<PathLocationData> _pathLocationList = _pathData.pathLocationDataList;
                    int numLocationsInList = _pathLocationList == null ? 0 : _pathLocationList.Count;

                    for (int lIdx = 0; lIdx < numLocationsInList; lIdx++)
                    {
                        // Get the location within the Path
                        PathLocationData _pathLocation = _pathLocationList[lIdx];

                        // If this Location in the Path is the same one being updated, then updated the control points
                        if (_pathLocation != null && _pathLocation.locationData != null && _pathLocation.locationData.guidHash == locationData.guidHash)
                        {
                            _pathLocation.inControlPoint += deltaPosition;
                            _pathLocation.outControlPoint += deltaPosition;
                            isPathAffected = true;
                        }
                    }

                    if (isPathAffected) { if (refreshPath) { RefreshPathDistances(_pathData); } else { _pathData.isDirty = true; } }
                }
            }
        }

        /// <summary>
        /// Set the initial or starting positions of Locations and control points along a dynamic (moveable) Path
        /// at runtime. Currently used with ShipDockingStation entry and exit paths that are attached
        /// to a mothership.
        /// NOTE: Assumes the Locations in each Path are not members of any other Paths in the scene.
        /// Paths will only be initialised once.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="dockingStationPosition"></param>
        /// <param name="offsetPosition">The offset from the original position of the docking station when the paths where setup in the scene</param>
        /// <param name="deltaRotation">The amount the docking station has been rotated after the paths were setup in the scene</param>
        /// <param name="pathVelocity"></param>
        /// <param name="pathAngularVelocity"></param>
        public void InitialiseLocations (PathData pathData, Vector3 dockingStationPosition, Vector3 offsetPosition, Quaternion deltaRotation, Vector3 pathVelocity, Vector3 pathAngularVelocity)
        {
            // Only update the locations if they Path hasn't already been updated.
            // This prevents the Location position from potentially being offset from the original position
            // multiple times.
            if (pathData != null && !pathData.isDynamicPathInitialised)
            {
                int numLocationsInList = pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

                pathData.isDynamicPathInitialised = true;
                pathData.anchorPoint = dockingStationPosition;
                pathData.worldVelocity = pathVelocity;
                pathData.worldAngularVelocity = pathAngularVelocity;

                for (int lIdx = 0; lIdx < numLocationsInList; lIdx++)
                {
                    PathLocationData _pathLocationData = pathData.pathLocationDataList[lIdx];
                    LocationData _locationData = GetLocation(_pathLocationData.locationData.guidHash);
                    if (_locationData != null)
                    {
                        // Add the position offset, then rotate the result around the pivot point (position) of the Docking Station
                        _locationData.initialPosition = (deltaRotation * (_locationData.position + offsetPosition - dockingStationPosition)) + dockingStationPosition;
                        _locationData.position = _locationData.initialPosition;

                        // Keep PathLocation and Location's in sync
                        _pathLocationData.locationData.position = _locationData.position;

                        // Add the position offset to the control points, then rotate the result around the pivot point (position) of the Docking Station
                        _pathLocationData.inInitialControlPoint = (deltaRotation * (_pathLocationData.inControlPoint + offsetPosition - dockingStationPosition)) + dockingStationPosition;
                        _pathLocationData.outInitialControlPoint = (deltaRotation * (_pathLocationData.outControlPoint + offsetPosition - dockingStationPosition)) + dockingStationPosition;

                        _pathLocationData.inControlPoint = _pathLocationData.inInitialControlPoint;
                        _pathLocationData.outControlPoint = _pathLocationData.outInitialControlPoint;
                    }
                }
            }
        }

        /// <summary>
        /// Designed specifically for runtime movement of a Path, it assumes the Locations of the Path are NOT members of any other Path.
        /// Position and Rotation deltas are from the CURRENT positions and rotations. Works similar to TelePort API methods.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="updateSeqNumber"></param>
        /// <param name="deltaPosition">Difference between CURRENT position and new position</param>
        /// <param name="deltaRotation">Difference between CURRENT rotation and new rotation</param>
        public void MoveLocations (PathData pathData, float updateSeqNumber, Vector3 deltaPosition, Quaternion deltaRotation)
        {
            // Check if path is valid and hasn't already been updated this frame
            if (pathData != null && pathData.updateSeqNumber != updateSeqNumber)
            {
                // Mark this path updated in this frame
                pathData.updateSeqNumber = updateSeqNumber;

                int numLocationsInList = pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

                for (int lIdx = 0; lIdx < numLocationsInList; lIdx++)
                {
                    PathLocationData _pathLocationData = pathData.pathLocationDataList[lIdx];
                    LocationData _locationData = GetLocation(_pathLocationData.locationData.guidHash);

                    if (_locationData != null)
                    {
                        _locationData.position = deltaRotation * (_locationData.position + deltaPosition);

                        // Keep PathLocation and Location's in sync
                        _pathLocationData.locationData.position = _locationData.position;
                        _pathLocationData.inControlPoint = deltaRotation * (_pathLocationData.inControlPoint + deltaPosition);
                        _pathLocationData.outControlPoint = deltaRotation * (_pathLocationData.outControlPoint + deltaPosition);
                    }
                }
            }
        }

        /// <summary>
        /// Designed specifically for runtime movement of a Path, it assumes the Locations of the Path are NOT members
        /// of any other Path.
        /// Position and Rotation deltas are from the INITIAL or starting positions and rotations.
        /// See also InitialiseLocations(..).
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="updateSeqNumber">Typically Time.time, it is used to determine if the Path has already been updated this frame</param>
        /// <param name="dockingStationPosition"></param>
        /// <param name="deltaPosition">Difference between initial or starting position and new position</param>
        /// <param name="deltaRotation">Difference between initial or starting rotation and new rotation</param>
        /// <param name="pathVelocity"></param>
        /// <param name="pathAngularVelocity"></param>
        public void MoveLocations(PathData pathData, float updateSeqNumber, Vector3 dockingStationPosition, Vector3 deltaPosition, Quaternion deltaRotation, Vector3 pathVelocity, Vector3 pathAngularVelocity)
        {
            // Check if path is valid and hasn't already been updated this frame
            if (pathData != null && pathData.updateSeqNumber != updateSeqNumber)
            {
                // Mark this path updated in this frame
                pathData.updateSeqNumber = updateSeqNumber;
                pathData.anchorPoint = dockingStationPosition;
                pathData.worldVelocity = pathVelocity;
                pathData.worldAngularVelocity = pathAngularVelocity;

                int numLocationsInList = pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

                for (int lIdx = 0; lIdx < numLocationsInList; lIdx++)
                {
                    PathLocationData _pathLocationData = pathData.pathLocationDataList[lIdx];
                    LocationData _locationData = GetLocation(_pathLocationData.locationData.guidHash);
                    if (_locationData != null)
                    {
                        _locationData.position = (deltaRotation * (_locationData.initialPosition + deltaPosition - dockingStationPosition)) + dockingStationPosition;

                        // Keep PathLocation and Location's in sync
                        _pathLocationData.locationData.position = _locationData.position;
                        _pathLocationData.inControlPoint = (deltaRotation * (_pathLocationData.inInitialControlPoint + deltaPosition - dockingStationPosition)) + dockingStationPosition;
                        _pathLocationData.outControlPoint = (deltaRotation * (_pathLocationData.outInitialControlPoint + deltaPosition - dockingStationPosition)) + dockingStationPosition;
                    }
                }
            }
        }

        /// <summary>
        /// Given a Location in a Path, being moved, update all other selected Locations in the same Path at the same time.
        /// NOTE: As this uses delegates, it may incur some GC overhead.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="locationDataMoved"></param>
        /// <param name="wsPosition"></param>
        /// <param name="refreshPath"></param>
        public void MoveLocations(PathData pathData, LocationData locationDataMoved, Vector3 wsPosition, bool refreshPath)
        {
            if (pathData != null && pathData.pathLocationDataList != null && locationDataMoved != null)
            {
                // Does the Location exist at least once in the Path?
                if (pathData.pathLocationDataList.Exists(loc => loc.locationData != null && loc.locationData.guidHash == locationDataMoved.guidHash))
                {
                    int numPathLocations = pathData.pathLocationDataList.Count;
                    Vector3 deltaPosition = wsPosition - locationDataMoved.position;
                    bool isPathAffected = false;
                    for (int plIdx = 0; plIdx < numPathLocations; plIdx++)
                    {
                        PathLocationData pathLocationData = pathData.pathLocationDataList[plIdx];
                        if (pathLocationData != null && pathLocationData.locationData != null && pathLocationData.locationData.selectedInSceneView && !pathLocationData.locationData.isUnassigned)
                        {
                            // Move Location
                            pathLocationData.locationData.position += deltaPosition;
                            // Update Control Points
                            pathLocationData.inControlPoint += deltaPosition;
                            pathLocationData.outControlPoint += deltaPosition;
                            isPathAffected = true;
                        }
                    }

                    if (isPathAffected) { if (refreshPath) { RefreshPathDistances(pathData); } else { pathData.isDirty = true; } }
                }
            }
        }

        /// <summary>
        /// Given a Location being moved, update all other selected Locations in the scene at the same time.
        /// If they are members of Paths, update those Paths.
        /// </summary>
        /// <param name="locationDataMoved"></param>
        /// <param name="wsPosition"></param>
        /// <param name="refreshPaths"></param>
        public void MoveLocations(LocationData locationDataMoved, Vector3 wsPosition, bool refreshPaths)
        {
            if (locationDataMoved != null)
            {
                int numInList = locationDataList == null ? 0 : locationDataList.Count;

                Vector3 deltaPosition = wsPosition - locationDataMoved.position;

                // Update other selected Locations
                for (int lIdx = 0; lIdx < numInList; lIdx++)
                {
                    LocationData locationData = locationDataList[lIdx];

                    if (locationData != null && locationData.selectedInSceneView && locationData.guidHash != locationDataMoved.guidHash)
                    {
                        UpdateLocation(locationData, locationData.position + deltaPosition, false);
                    }
                }

                // Update the position of the Location being dragged in the scene
                UpdateLocation(locationDataMoved, wsPosition, false);

                if (refreshPaths)
                {
                    // Refresh any stale Path distances
                    int numPaths = pathDataList == null ? 0 : pathDataList.Count;

                    for (int pIdx = 0; pIdx < numPaths; pIdx++)
                    {
                        if (pathDataList[pIdx].isDirty) { RefreshPathDistances(pathDataList[pIdx]); }
                    }
                }
            }
        }

        /// <summary>
        /// Delete all the selected Locations, including any Location slots associated
        /// with a Path.
        /// refreshPath will update the path distances for all affected Paths.
        /// </summary>
        public void DeleteSelectedLocations(bool refreshPath)
        {
            int numInList = locationDataList == null ? 0 : locationDataList.Count;
            bool isPathAffected = false;

            // Loop backwards through the Locations
            for (int idx = numInList - 1; idx >= 0; idx--)
            {
                LocationData locationData = locationDataList[idx];
                if (locationData != null && locationData.selectedInSceneView)
                {
                    // Remove any Location slots in Paths
                    // Loop through all the paths
                    int numPathsInList = pathDataList == null ? 0 : pathDataList.Count;
                    for (int pIdx = 0; pIdx < numPathsInList; pIdx++)
                    {
                        PathData _pathData = pathDataList[pIdx];
                        int numPathLocationsInList = _pathData != null && _pathData.pathLocationDataList != null ? _pathData.pathLocationDataList.Count : 0;

                        isPathAffected = false;

                        // Loop backwards through the list and remove Location slots where there is a matching Location
                        for (int plIdx = numPathLocationsInList-1; plIdx >= 0; plIdx--)
                        {
                            PathLocationData _pathLocationData = _pathData.pathLocationDataList[plIdx];
                            if (_pathLocationData != null && _pathLocationData.locationData != null && _pathLocationData.locationData.guidHash == locationData.guidHash)
                            {
                                // Remove the Path Location slot
                                _pathLocationData.locationData = null;
                                _pathData.pathLocationDataList.RemoveAt(plIdx);
                                isPathAffected = true;
                            }
                        }

                        if (isPathAffected) { if (refreshPath) { RefreshPathDistances(_pathData); } else { _pathData.isDirty = true; } }
                    }
                    // Delete the Location
                    locationData = null;
                    locationDataList.RemoveAt(idx);
                }
            }
        }

        /// <summary>
        /// Attempt to snap selected Locations on a Path to the closes mesh within the mesh Layer Mask.
        /// The location is placed with an offset from mesh. This 'snap' distance is the pathData.locationDefaultNormalOffset.
        /// It can be set at runtime or in the SSCManager editor.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="upDirection">The up direction from the mesh being snapped to</param>
        /// <param name="meshLayerMask">e.g. meshLayerMask = ~0;</param>
        /// <param name="refreshPath"></param>
        public void SnapPathSelectedLocationsToMesh(PathData pathData, Vector3 upDirection, LayerMask meshLayerMask, bool refreshPath)
        {
            bool isPathAffected = false;
            int numPathLocations = pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

            PathLocationData tempPathLocationData = null;
            Vector3 locPosition;
            Vector3 offsetVector = upDirection * pathData.locationDefaultNormalOffset;
            Ray raycastRay = new Ray(Vector3.zero, Vector3.down);
            RaycastHit raycastHitInfo;
            float maxCheckDistance = pathData.snapMaxHeight - pathData.snapMinHeight;

            raycastRay.direction = -upDirection;

            for (int lIdx = 0; lIdx < numPathLocations; lIdx++)
            {
                // Only process assigned and selected Locations in the Path
                tempPathLocationData = pathData.pathLocationDataList[lIdx];
                if (tempPathLocationData != null && tempPathLocationData.locationData != null &&
                    !tempPathLocationData.locationData.isUnassigned && tempPathLocationData.locationData.selectedInSceneView)
                {
                    locPosition = tempPathLocationData.locationData.position;
                    raycastRay.origin = locPosition + (upDirection * 0.001f);

                    if (Physics.Raycast(raycastRay, out raycastHitInfo, maxCheckDistance, meshLayerMask, QueryTriggerInteraction.Ignore))
                    {
                        // Update the actual location, not just the reference attached to the Path.
                        LocationData locationData = GetLocation(tempPathLocationData.locationData.guidHash);

                        Vector3 newPosition = raycastHitInfo.point + offsetVector;

                        // Ensure the new Location position is within acceptable limits
                        if (newPosition.y >= pathData.snapMinHeight && newPosition.y <= pathData.snapMaxHeight)
                        {
                            isPathAffected = true;
                            UpdateLocation(locationData, newPosition, false);
                        }
                    }
                }
            }

            if (isPathAffected) { if (refreshPath) { RefreshPathDistances(pathData); } else { pathData.isDirty = true; } }
        }

        /// <summary>
        /// Attempt to snap the Path locations to a mesh above or below the current location. The location is
        /// placed with an offset from mesh. This 'snap' distance is the pathData.locationDefaultNormalOffset.
        /// It can be set at runtime or in the SSCManager editor.
        /// The method checks for a mesh between pathData.snapMinHeight and snapMaxHeight in the upDirection.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="upDirection">The up direction from the mesh being snapped to</param>
        /// <param name="meshLayerMask">e.g. meshLayerMask = ~0;</param>
        /// <param name="refreshPath"></param>
        public void SnapPathToMesh(PathData pathData, Vector3 upDirection, LayerMask meshLayerMask, bool refreshPath)
        {
            if (pathData != null)
            {
                bool isPathAffected = false;
                int numPathLocations = pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

                PathLocationData tempPathLocationData = null;
                Vector3 locPosition;
                Vector3 rayOrigin = Vector3.zero + (upDirection * pathData.snapMaxHeight);
                Vector3 offsetVector = upDirection * pathData.locationDefaultNormalOffset;
                Ray raycastRay = new Ray(rayOrigin, Vector3.down);
                RaycastHit raycastHitInfo;
                float maxCheckDistance = pathData.snapMaxHeight - pathData.snapMinHeight;

                raycastRay.direction = -upDirection;

                for (int lIdx = 0; lIdx < numPathLocations; lIdx++)
                {
                    tempPathLocationData = pathData.pathLocationDataList[lIdx];
                    if (tempPathLocationData == null || tempPathLocationData.locationData == null || tempPathLocationData.locationData.isUnassigned) { continue; }
                    else
                    {
                        isPathAffected = true;
                        locPosition = tempPathLocationData.locationData.position;
                        rayOrigin.x = locPosition.x;
                        rayOrigin.z = locPosition.z;
                        raycastRay.origin = rayOrigin;

                        if (Physics.Raycast(raycastRay, out raycastHitInfo, maxCheckDistance, meshLayerMask, QueryTriggerInteraction.Ignore))
                        {
                            // Update the actual location, not just the reference attached to the Path.
                            LocationData locationData = GetLocation(tempPathLocationData.locationData.guidHash);

                            UpdateLocation(locationData, raycastHitInfo.point + offsetVector, false);
                        }
                    }
                }

                if (isPathAffected) { if (refreshPath) { RefreshPathDistances(pathData); } else { pathData.isDirty = true; } }
            }
        }

        /// <summary>
        /// Attempt to snap the Location to a mesh above or below the current position. The location is
        /// placed with an offset from mesh. This 'snap' distance is the offset.
        /// </summary>
        /// <param name="locationData"></param>
        /// <param name="offset">The distance 'above' the mesh the Location should be placed</param>
        /// <param name="snapMinHeight">The minimum height of the mesh to check for</param>
        /// <param name="snapMaxHeight">The maximum height of the mesh to check for</param>
        /// <param name="upDirection">The up direction from the mesh being snapped to</param>
        /// <param name="meshLayerMask">e.g. meshLayerMask = ~0;</param>
        /// <param name="refreshPaths">Refresh the lengths of all Paths that contain this Location</param>
        public void SnapLocationToMesh(LocationData locationData, float offset, float snapMinHeight, float snapMaxHeight, Vector3 upDirection, LayerMask meshLayerMask, bool refreshPaths)
        {
            if (locationData != null)
            {
                Vector3 rayOrigin = Vector3.zero + (upDirection * snapMaxHeight);
                Vector3 offsetVector = upDirection * offset;
                Ray raycastRay = new Ray(rayOrigin, Vector3.down);
                RaycastHit raycastHitInfo;

                Vector3 locPosition = locationData.position;
                rayOrigin.x = locPosition.x;
                rayOrigin.z = locPosition.z;
                raycastRay.origin = rayOrigin;

                if (Physics.Raycast(raycastRay, out raycastHitInfo, snapMaxHeight - snapMinHeight, meshLayerMask, QueryTriggerInteraction.Ignore))
                {
                    UpdateLocation(locationData, raycastHitInfo.point + offsetVector, refreshPaths);
                }
            }
        }

        /// <summary>
        /// Get the first Location on a given Path
        /// </summary>
        /// <param name="pathData"></param>
        /// <returns></returns>
        public LocationData GetFirstLocation(PathData pathData)
        {
            // We use GetLocation(..) to ensure the path point contains an actual Location
            if (pathData == null || pathData.pathLocationDataList == null || locationDataList == null || pathData.pathLocationDataList.Count < 1 || locationDataList.Count < 1) { return null; }
            else { return GetLocation(pathData.pathLocationDataList[0].locationData.guidHash); }
        }

        /// <summary>
        /// Get the last Location on a given Path
        /// </summary>
        /// <param name="pathData"></param>
        /// <returns></returns>
        public LocationData GetLastLocation(PathData pathData)
        {
            int numLocations = pathData == null || pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

            // We use GetLocation(..) to ensure the path point contains an actual Location
            if (numLocations < 1) { return null; }
            else { return GetLocation(pathData.pathLocationDataList[numLocations-1].locationData.guidHash); }
        }

        /// <summary>
        /// Get a Location given the unique guidHash code of the Location
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public LocationData GetLocation(int guidHash)
        {
            if (locationDataList == null) { return null; }
            else { return locationDataList.Find(loc => loc.guidHash == guidHash); }
        }

        /// <summary>
        /// Get the first Location given the name of the Location. locationName is not case sensitive.
        /// If the locationName parameter is empty or null, no Location will be returned even if
        /// there are Locations without a name.
        /// NOTE: When possible, use GetLocation(guidHash).
        /// </summary>
        /// <param name="locationName"></param>
        /// <returns></returns>
        public LocationData GetLocation(string locationName)
        {
            if (locationDataList == null || string.IsNullOrEmpty(locationName)) { return null; }
            else { return locationDataList.Find(loc => !string.IsNullOrEmpty(loc.name) && loc.name.ToLower() == locationName.ToLower()); }
        }

        /// <summary>
        /// If a location was added with AppendLocation(gameObject), it can also be retrieved at runtime with
        /// this method by passing in the gameobject.GetInstanceID().
        /// </summary>
        /// <param name="gameObjectInstanceID"></param>
        /// <returns></returns>
        public LocationData GetLocationByGameObjectID(int gameObjectInstanceID)
        {
            LocationData locationData = null;
            LocationData tempLocationData = null;
            int _numLocations = locationDataList == null ? 0 : locationDataList.Count;

            if (_numLocations > 0 && gameObjectInstanceID != 0)
            {
                for (int lIdx = 0; lIdx < _numLocations; lIdx++)
                {
                    tempLocationData = locationDataList[lIdx];
                    if (tempLocationData != null && tempLocationData.gameObjectInstanceId == gameObjectInstanceID)
                    {
                        locationData = tempLocationData;
                        break;
                    }
                }
            }

            return locationData;
        }

        /// <summary>
        /// Get a Path given the unique guidHash code of the Path
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public PathData GetPath(int guidHash)
        {
            if (pathDataList == null) { return null; }
            else { return pathDataList.Find(p => p.guidHash == guidHash); }
        }

        /// <summary>
        /// Get the first Path given the name of the Path. pathName is not case sensitive.
        /// If the pathName parameter is empty or null, no Path will be returned even if
        /// there are Paths without a name.
        /// NOTE: When possible, use GetPath(guidHash).
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public PathData GetPath(string pathName)
        {
            if (pathDataList == null || string.IsNullOrEmpty(pathName)) { return null; }
            else { return pathDataList.Find(p => !string.IsNullOrEmpty(p.name) && p.name.ToLower() == pathName.ToLower()); }
        }

        /// <summary>
        /// Attempt to reverse the direction of a path
        /// </summary>
        /// <param name="pathData"></param>
        public void ReversePath (PathData pathData)
        {
            if (pathData != null && pathData.pathLocationDataList != null)
            {
                int numPathLocations = pathData.pathLocationDataList.Count;

                PathLocationData pathLocationData = null;

                if (numPathLocations > 1)
                {
                    pathData.pathLocationDataList.Reverse();

                    RefreshPathDistances(pathData);

                    // Reverse the control points
                    for (int plIdx = 0; plIdx < numPathLocations; plIdx++)
                    {
                        pathLocationData = pathData.pathLocationDataList[plIdx];

                        if (pathLocationData != null && pathLocationData.locationData != null && !pathLocationData.locationData.isUnassigned)
                        {
                            // Reverse Control Points
                            Vector3 inControlPoint = pathLocationData.inControlPoint;
                            pathLocationData.inControlPoint = pathLocationData.outControlPoint;
                            pathLocationData.outControlPoint = inControlPoint;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recalculate the distances that are stored in the PathLocationData instances.
        /// </summary>
        /// <param name="pathData"></param>
        public void RefreshPathDistances(PathData pathData)
        {
            if (pathData != null && pathData.pathLocationDataList != null)
            {
                int numLocationsInList = pathData.pathLocationDataList.Count;

                PathLocationData pathLocationData = null;
                float cumulativeDistance = 0f;

                if (numLocationsInList > 0)
                {
                    pathLocationData = pathData.pathLocationDataList[0];
                    pathLocationData.distanceCumulative = 0f;
                    pathLocationData.distanceFromPreviousLocation = 0f;
                    float sectionDistance = 0f;

                    int numSegments = numPathSegmentsForEstimate;

                    for (int plIdx = 1; plIdx < numLocationsInList; plIdx++)
                    {
                        pathLocationData = pathData.pathLocationDataList[plIdx];

                        // If the length of the Path section is unknown, first do a estimate with only a few segments,
                        // else get the number of segments based on the previous length of the Path section.
                        numSegments = CalcPathSegments(pathData, plIdx - 1, pathLocationData.distanceFromPreviousLocation);

                        if (SSCMath.GetDistanceBetweenPathPoints(pathData, plIdx-1, numSegments, ref sectionDistance))
                        {
                            cumulativeDistance += sectionDistance;
                            pathLocationData.distanceCumulative = cumulativeDistance;
                            pathLocationData.distanceFromPreviousLocation = sectionDistance;
                        }
                        else
                        {
                            pathLocationData.distanceCumulative = 0f;
                            pathLocationData.distanceFromPreviousLocation = 0f;
                        }
                    }

                    // Deal with closed circuit
                    // You "might" want a "circuit" consisting of just 2 points.
                    // Most circuits should have at least 3 points.
                    if (pathData.isClosedCircuit && numLocationsInList > 1)
                    {
                        pathLocationData = pathData.pathLocationDataList[0];

                        numSegments = CalcPathSegments(pathData, numLocationsInList - 1, pathLocationData.distanceFromPreviousLocation);

                        if (SSCMath.GetDistanceBetweenPathPoints(pathData, numLocationsInList - 1, numSegments, ref sectionDistance))
                        {
                            cumulativeDistance += sectionDistance;
                            pathLocationData.distanceCumulative = cumulativeDistance;
                            pathLocationData.distanceFromPreviousLocation = sectionDistance;
                        }
                    }
                }

                pathData.splineTotalDistance = cumulativeDistance;
                pathData.isDirty = false;
            }
        }

        #endregion

        #region Public API Member Methods - Radar

        /// <summary>
        /// Enable radar for a stationary Location if SSCManager is initialised.
        /// </summary>
        /// <param name="locationData"></param>
        public void EnableRadar(LocationData locationData)
        {
            if (locationData != null && isInitialised)
            {
                // Not assigned in the radar system
                locationData.radarItemIndex = -1;

                if (sscRadar == null) { sscRadar = SSCRadar.GetOrCreateRadar(); }

                if (sscRadar != null)
                {
                    SSCRadarItem sscRadarItem = new SSCRadarItem();
                    sscRadarItem.radarItemType = SSCRadarItem.RadarItemType.Location;
                    sscRadarItem.isVisibleToRadar = true;
                    sscRadarItem.guidHash = locationData.guidHash;
                    sscRadarItem.position = locationData.position;
                    sscRadarItem.factionId = locationData.factionId;
                    sscRadarItem.squadronId = -1; // NOT SET
                    sscRadarItem.blipSize = locationData.radarBlipSize;

                    // Create a packet to be used to send data to the radar system
                    //SSCRadarPacket sscRadarPacket = new SSCRadarPacket();

                    locationData.radarItemIndex = sscRadar.AddItem(sscRadarItem);

                    locationData.isRadarEnabled = true;
                }
            }
        }

        /// <summary>
        /// The Location will no longer be visible in the radar system.
        /// If you want to change the visibility to other
        /// radar consumers, consider changing the radar item data rather
        /// than disabling the radar and (later) calling EnableRadar again.
        /// </summary>
        public void DisableRadar(LocationData locationData)
        {
            if (isInitialised)
            {
                if (locationData.isRadarEnabled && sscRadar != null)
                {
                    sscRadar.RemoveItem(locationData.RadarId);
                }
                locationData.isRadarEnabled = false;
                locationData.radarItemIndex = -1;
            }
        }

        #endregion
    }

    #region ProjectileTemplate

    /// <summary>
    /// Class containing data for a projectile template. Used by SSCManager.
    /// </summary>
    public class ProjectileTemplate
    {
        public ProjectileModule projectilePrefab;
        public int instanceID;
        public int currentPoolSize;
        public List<GameObject> projectilePool;
        #if SSC_ENTITIES
        public EntityArchetype projectileEntityArchetype;
        public Entity projectilePrefabEntity;
        #endif

        // Class constructor
        // SSCManager must be initialised before this is called when DOTS/ECS is used.
        // When ECS 1.0, Entities are procedurally created.
        // When ECS <1.0, Entities uses ConvertGameObjectHierarchy(..).
        public ProjectileTemplate (ProjectileModule prefab, int id)
        {
            this.projectilePrefab = prefab;
            this.instanceID = id;
            this.currentPoolSize = 0;
            this.projectilePool = null;

            #if SSC_ENTITIES
            #region Construct Entity Archetype

            if (prefab.useECS)
            {
                // WARNING: ConvertGameObjectHierarchy has been REMOVED in ECS 1.0 for U2022.2 and WILL NOT WORK...

                World defaultWorld = SSCManager.sscWorld;
                EntityManager entityManager = SSCManager.entityManager;

                #if UNITY_2022_2_OR_NEWER
                // ECS 1.0   
                projectilePrefabEntity = entityManager.CreateEntity();

                entityManager.AddComponent(projectilePrefabEntity, typeof(LocalTransform));
                entityManager.AddComponent(projectilePrefabEntity, typeof(Projectile));
                entityManager.AddComponent(projectilePrefabEntity, typeof(PostTransformScale));

                MeshRenderer mRen;
                MeshFilter mFilter;

                Transform prefabRootTransform = projectilePrefab.transform;

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
                        entity: projectilePrefabEntity,
                        entityManager: entityManager,
                        renderMeshDescription: desc,
                        renderMeshArray: renderMeshArray,
                        materialMeshInfo: MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0)
                    );
                }
                
                #elif UNITY_2019_3_OR_NEWER || UNITY_ENTITIES_0_2_0_OR_NEWER
                // U2019.3 introduced Entities 0.2.0
                GameObjectConversionSettings conversionSettings = GameObjectConversionSettings.FromWorld(defaultWorld, SSCManager.blobAssetStore);
                projectilePrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab.gameObject, conversionSettings);
                #else
                // U2019.1, 2019.2 Entities 0.012 preview 33 to 0.1.1 preview.
                projectilePrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab.gameObject, defaultWorld);
                #endif

                #if UNITY_2022_2_OR_NEWER
                ComponentType[] archetypeComponentTypeArray = new ComponentType[4];
                #else
                ComponentType[] archetypeComponentTypeArray = new ComponentType[4];
                #endif

                // With ConvertGameObjectHierarchy not sure if we need Translation and Rotation here given they are automatically added
                #if UNITY_2022_2_OR_NEWER
                archetypeComponentTypeArray[0] = typeof(LocalTransform);
                archetypeComponentTypeArray[1] = typeof(Projectile);
                archetypeComponentTypeArray[2] = typeof(PostTransformScale);
                archetypeComponentTypeArray[3] = typeof(RenderMesh);
                #else
                // ECS 0.51
                archetypeComponentTypeArray[0] = typeof(Translation);
                archetypeComponentTypeArray[1] = typeof(Rotation);
                archetypeComponentTypeArray[2] = typeof(Projectile);

                // NOTE: Requires hybrid renderer, has been tested using ECS 0.0.12 preview-30 and Hybrid Renderer 0.0.1 preview-10
                archetypeComponentTypeArray[3] = typeof(RenderMesh);
                #endif
                
                this.projectileEntityArchetype = entityManager.CreateArchetype(archetypeComponentTypeArray);
            }
            #endregion
            #endif
        }
    }

    #endregion

    #region BeamTemplate

    /// <summary>
    /// Class containing data for a beam template. Used by SSCManager.
    /// </summary>
    public class BeamTemplate
    {
        public BeamModule beamPrefab;
        public int instanceID;
        public int currentPoolSize;
        public List<GameObject> beamPoolList;
        
        // Class constructor
        public BeamTemplate (BeamModule prefab, int id)
        {
            beamPrefab = prefab;
            instanceID = id;
            currentPoolSize = 0;
            beamPoolList = null;
        }
    }

    #endregion

    #region DestructTemplate

    /// <summary>
    /// Class containing data for a destruct template. Used by SSCManager.
    /// </summary>
    public class DestructTemplate
    {
        public DestructModule destructPrefab;
        public int instanceID;
        public int currentPoolSize;
        public List<GameObject> destructPoolList;
        
        // Class constructor
        public DestructTemplate (DestructModule prefab, int id)
        {
            destructPrefab = prefab;
            instanceID = id;
            currentPoolSize = 0;
            destructPoolList = null;
        }
    }

    #endregion

    #region EffectsObjectTemplate

    /// <summary>
    /// Class containing data for an effects object template. Used by SSCManager.
    /// </summary>
    public class EffectsObjectTemplate
    {
        public EffectsModule effectsObjectPrefab;
        public int instanceID;
        public int currentPoolSize;
        public List<GameObject> effectsObjectPool;

        // Class constructor
        public EffectsObjectTemplate (EffectsModule prefab, int id)
        {
            this.effectsObjectPrefab = prefab;
            this.instanceID = id;
            this.currentPoolSize = 0;
            this.effectsObjectPool = null;
        }
    }

    #endregion

    #region Public Structures

    /// <summary>
    /// Parameters structure for use with sscManager.InstantiateProjectile(..).
    /// struct members subject to change without notice.
    /// </summary>
    public struct InstantiateProjectileParameters
    {
        #region Public Variables
        /// <summary>
        /// The projectile template index
        /// </summary>
        public int projectilePrefabID;
        /// <summary>
        /// The world space position this projectile is fired from
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// The world space direction the profile is fired in
        /// </summary>
        public Vector3 fwdDirection;
        /// <summary>
        /// The up direction of the projectile
        /// </summary>
        public Vector3 upDirection;
        /// <summary>
        /// Current velocity of the weapon that fired the projectile (which could include
        /// the world velocity of the ship the weapon is attached to.
        /// </summary>
        public Vector3 weaponVelocity;
        /// <summary>
        /// Gravitational acceleration affecting the projectile in m/s^2
        /// </summary>
        public float gravity;
        /// <summary>
        /// Direction in worldspace that gravity is acting on the projectile 
        /// </summary>
        public Vector3 gravityDirection;
        /// <summary>
        /// Ship that fired the projectile, else 0
        /// </summary>
        public int shipId;
        /// <summary>
        /// Squadron of the ship that fired the projectile, else -1.
        /// </summary>
        public int squadronId;
        /// <summary>
        /// This is the index in the SSCManager effectsObjectTemplatesList for the regular destruct FX
        /// </summary>
        public int effectsObjectPrefabID;
        /// <summary>
        /// This is the index in the SSCManager effectsObjectTemplatesList for the hit shield destruct FX
        /// </summary>
        public int shieldEffectsObjectPrefabID;
        /// <summary>
        /// The ship to target for guided projectiles
        /// </summary>
        public ShipControlModule targetShip;
        /// <summary>
        /// The gameobject to target for guided projectiles
        /// </summary>
        public GameObject targetGameObject;
        /// <summary>
        /// The unique guidHash for the target. Currently only used for ship damage regions.
        /// If unset value is 0.
        /// </summary>
        public int targetguidHash;
        /// <summary>
        /// Return the spawned muzzle FX pooled template EffectsModule PrefabID.
        /// Returns -1 if non-pooled or not spawned.
        /// </summary>
        public int muzzleEffectsObjectPrefabID;
        /// <summary>
        /// Return the spawned muzzle FX of the zero-based index in the current pool
        /// Returns -1 if non-pooled or not spawned.
        /// </summary>
        public int muzzleEffectsObjectPoolListIndex;
        #endregion
    }

    /// <summary>
    /// Parameters structure for use with sscManager.InstantiateBeam(..).
    /// struct members subject to change without notice.
    /// </summary>
    public struct InstantiateBeamParameters
    {
        #region Public Variables
        /// <summary>
        /// The beam template index
        /// </summary>
        public int beamPrefabID;
        /// <summary>
        /// The world space position this beam is fired from
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// The world space direction the beam is fired in
        /// </summary>
        public Vector3 fwdDirection;
        /// <summary>
        /// The up direction of the beam
        /// </summary>
        public Vector3 upDirection;
        /// <summary>
        /// Ship that fired the beam, else 0
        /// </summary>
        public int shipId;
        /// <summary>
        /// Squadron of the ship that fired the beam, else -1.
        /// </summary>
        public int squadronId;
        /// <summary>
        /// The zero-based index of the weapon on the ship that fired the beam
        /// </summary>
        public int weaponIndex;
        /// <summary>
        /// The zero-based index of the fire position on the weapon that fired the beam
        /// </summary>
        public int firePositionIndex;
        /// <summary>
        /// This is the index in the SSCManager effectsObjectTemplatesList.
        /// </summary>
        public int effectsObjectPrefabID;
        /// <summary>
        /// Return value of the zero-based index in the current pool
        /// Returns -1 if non-pooled.
        /// </summary>
        public int beamPoolListIndex;
        /// <summary>
        /// Return value of the unique number for the beam instance
        /// </summary>
        public uint beamSequenceNumber;
        #endregion
    }

    /// <summary>
    /// Parameters structure for use with sscManager.InstantiateDestruct(..).
    /// struct members subject to change without notice.
    /// </summary>
    public struct InstantiateDestructParameters
    {
        #region Public variables

        // The destruct object template index
        public int destructPrefabID;

        // The position where the destruct object will be instantiated
        public Vector3 position;

        // The rotation of the destruct object when instantiated
        public Quaternion rotation;

        /// <summary>
        /// A 0 to 1 multiplier of the explosionPower of the Destruct Module
        /// </summary>
        public float explosionPowerFactor;

        /// <summary>
        /// A 0 to 1 multiplier of the explosionRadius of the Destruct Module
        /// </summary>
        public float explosionRadiusFactor;

        /// <summary>
        /// Return value of the zero-based index in the current pool
        /// Returns -1 if non-pooled.
        /// </summary>
        public int destructPoolListIndex;
        /// <summary>
        /// Return value of the unique number for the destruct instance
        /// </summary>
        public uint destructSequenceNumber;

        #endregion
    }

    /// <summary>
    /// Parameters structure for use with sscManager.InstantiateEffectsObject(..).
    /// struct members subject to change without notice.
    /// </summary>
    public struct InstantiateEffectsObjectParameters
    {
        #region Public variables

        // The effects object template index
        public int effectsObjectPrefabID;

        // The position where the effects object will be instantiated
        public Vector3 position;

        // The rotation of the effects object when instantiated
        public Quaternion rotation;

        /// <summary>
        /// Return value of the zero-based index in the current pool
        /// Returns -1 if non-pooled.
        /// </summary>
        public int effectsObjectPoolListIndex;
        /// <summary>
        /// Return value of the unique number for the effects instance
        /// </summary>
        public uint effectsObjectSequenceNumber;

        #endregion
    }

    /// <summary>
    /// Parameters structure for use with sscManager.InstantiateSoundFX(..).
    /// struct members subject to change without notice.
    /// </summary>
    public struct InstantiateSoundFXParameters
    {
        #region Public variables

        // The effects object template index
        public int effectsObjectPrefabID;

        // The position where the sound will be instantiated
        public Vector3 position;

        /// <summary>
        /// If the volume is > 0, it will override the volume of the prefab
        /// </summary>
        public float volume;

        /// <summary>
        /// If true, the volume will be set to the EffectModule.defaultVolume.
        /// </summary>
        public bool useDefaultVolume;

        #endregion
    }

    /// <summary>
    /// A struct used to uniquely identify a Beam Module
    /// </summary>
    public struct SSCBeamItemKey
    {
        #region Public variables

        /// <summary>
        /// -1 = unset
        /// </summary>
        public int beamTemplateListIndex;

        /// <summary>
        /// -1 = unset
        /// </summary>
        public int beamPoolListIndex;

        /// <summary>
        /// 0 = unset
        /// </summary>
        public uint beamSequenceNumber;
        #endregion

        #region Constructor
        public SSCBeamItemKey(int templateId, int poolId, uint sequenceNumber)
        {
            beamTemplateListIndex = templateId;
            beamPoolListIndex = poolId;
            beamSequenceNumber = sequenceNumber;
        }
        #endregion
    }

    /// <summary>
    /// A struct used to uniquely identify a Destruct Module
    /// </summary>
    public struct SSCDestructItemKey
    {
        #region Public variables

        /// <summary>
        /// -1 = unset
        /// </summary>
        public int destructTemplateListIndex;

        /// <summary>
        /// -1 = unset
        /// </summary>
        public int destructPoolListIndex;

        /// <summary>
        /// 0 = unset
        /// </summary>
        public uint destructSequenceNumber;
        #endregion

        #region Constructor
        public SSCDestructItemKey(int templateId, int poolId, uint sequenceNumber)
        {
            destructTemplateListIndex = templateId;
            destructPoolListIndex = poolId;
            destructSequenceNumber = sequenceNumber;
        }

        #endregion
    }

    /// <summary>
    /// A struct used to uniquely identity an EffectsModule
    /// </summary>
    public struct SSCEffectItemKey
    {
        #region Public variables
        
        /// <summary>
        /// -1 = unset
        /// </summary>
        public int effectsObjectTemplateListIndex;

        /// <summary>
        /// -1 = unset
        /// </summary>
        public int effectsObjectPoolListIndex;

        /// <summary>
        /// 0 = unset
        /// </summary>
        public uint effectsObjectSequenceNumber;
        #endregion

        #region Constructor
        public SSCEffectItemKey(int templateId, int poolId, uint sequenceNumber)
        {
            effectsObjectTemplateListIndex = templateId;
            effectsObjectPoolListIndex = poolId;
            effectsObjectSequenceNumber = sequenceNumber;
        }

        #endregion
    }

    #endregion
}
