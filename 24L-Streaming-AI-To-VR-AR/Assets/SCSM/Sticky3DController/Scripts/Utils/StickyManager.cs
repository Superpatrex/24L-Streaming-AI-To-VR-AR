using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [AddComponentMenu("Sticky3D Controller/Utilities/Sticky Manager")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    [DisallowMultipleComponent]
    public class StickyManager : MonoBehaviour
    {
        #region Enumerations

        /// <summary>
        /// The method used to disable a rigidbody
        /// </summary>
        public enum DisableRBodyMode
        {
            Destroy = 0,
            SetAsKinematic = 1,
            DontDisable = 2
        }

        /// <summary>
        /// The condition determined when an object should be despawned
        /// or returned to the pool.
        /// </summary>
        public enum DespawnCondition
        {
            Time = 0,
            DontDespawn = 1
        }

        /// <summary>
        /// The method used to determine in which direction gravity is acting.
        /// </summary>
        public enum GravityMode
        {
            Direction = 0,
            ReferenceFrame = 1,
            UnityPhysics = 2
        }

        #endregion

        #region Public Static Variables
        //public readonly static string displayPanelName = "DisplayPanel";

        public static readonly int DisableRBodyModeDestroyInt = (int)DisableRBodyMode.Destroy;
        public static readonly int DisableRBodyModeSetAsKinematicInt = (int)DisableRBodyMode.SetAsKinematic;
        public static readonly int DisableRBodyModeDontDisableInt = (int)DisableRBodyMode.DontDisable;

        public static readonly int DespawnConditionTimeInt = (int)DespawnCondition.Time;
        public static readonly int DespawnConditionDontDespawnInt = (int)DespawnCondition.DontDespawn;

        public static readonly int GravityModeDirectionInt = (int)GravityMode.Direction;
        public static readonly int GravityModeRefFrameInt = (int)GravityMode.ReferenceFrame;
        public static readonly int GravityModeUnityInt = (int)GravityMode.UnityPhysics;

        /// <summary>
        /// Used to denote that a prefab is not pooled.
        /// See GetorCreateGenericPool(..).
        /// </summary>
        public static int NoPrefabID = -1;

        #endregion

        #region Public Variables


        #endregion

        #region Public Properties - General

        /// <summary>
        /// [READONLY] Is the StickyManager initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// [READONLY] Are all the beams in the scene currently paused?
        /// See also PauseBeams() and ResumeBeams()
        /// </summary>
        public bool IsBeamObjectsPaused { get { return isBeamsPaused; } }

        /// <summary>
        /// [READONLY] Are all the generic objects in the scene currently paused?
        /// See also PauseGenericObjects() and ResumeGenericObjects()
        /// </summary>
        public bool IsGenericObjectsPaused { get { return isGenericObjectsPaused; } }

        /// <summary>
        /// [READONLY] Get the list of the Beam Templates. To modify, use the public API Methods provided.
        /// </summary>
        public List<S3DBeamTemplate> BeamTemplatesList { get { { return beamTemplatesList; } } }

        /// <summary>
        /// [READONLY] Get the list of the Decal Templates. To modify, use the public API Methods provided.
        /// </summary>
        public List<S3DDecalTemplate> DecalTemplatesList { get { { return decalTemplatesList; } } }

        /// <summary>
        /// [READONLY] Get the list of the Dynamic Templates. To modify, use the public API Methods provided.
        /// </summary>
        public List<S3DDynamicObjectTemplate> DynamicObjectTemplatesList { get { { return dynamicObjectTemplatesList; } } }

        /// <summary>
        /// [READONLY] Get the list of the Effects Templates. To modify, use the public API Methods provided.
        /// </summary>
        public List<S3DEffectsObjectTemplate> EffectsObjectTemplatesList { get { { return effectsObjectTemplatesList; } } }

        /// <summary>
        /// [READONLY] Get the list of the Generic Object Templates. To modify, use the public API Methods provided.
        /// </summary>
        public List<S3DGenericObjectTemplate> GenericObjectTemplatesList { get { return genericObjectTemplatesList; } }

        /// <summary>
        /// [READONLY] Get the list of the Projectil Templates. To modify, use the public API Methods provided.
        /// </summary>
        public List<S3DProjectileTemplate> ProjectileTemplatesList { get { { return projectileTemplatesList; } } }

        /// <summary>
        /// [READONLY] The number of (unique) StickyBeamModule prefabs in the BeamTemplateList
        /// </summary>
        public int NumberOfBeamTemplates { get { return numBeamTemplates; } }

        #endregion

        #region Public Properties - Ammo

        /// <summary>
        /// [READONLY (Public)]
        /// Get the (common) array of ammo types.
        /// </summary>
        public S3DAmmoTypes AmmoTypes { get { return ammoTypes; } internal set { SetAmmoTypes(value); } }

        #endregion

        #region Public Delegates

        #endregion

        #region Private Variables - General

        //private static StickyManager currentManager = null;
        private static List<GameObject> tempGameObjectList = new List<GameObject>();
        private static List<StickyManager> managerList = new List<StickyManager>(2);
        private static int numberManagers = 0;

        internal int sceneHandle = 0;

        private bool isInitialised = false;
        private bool isInitialising = false;

        /// <summary>
        /// The list of beam modules added to the pool at start-up. Beams can also be added via code.
        /// </summary>
        [SerializeField] private List<StickyBeamModule> startupBeamModuleList = null;

        /// <summary>
        /// The list of decal sets added to the pool at start-up. Decals can also be added via code.
        /// Each set can contain multiple StickyDecalModule prefabs.
        /// </summary>
        [SerializeField] private List<S3DDecals> startupDecalsList = null;

        /// <summary>
        /// The list of dynamic modules added to the pool at start-up. Dynamic objects can also be added via code.
        /// </summary>
        [SerializeField] private List<StickyDynamicModule> startupDynamicModuleList = null;
        /// <summary>
        /// The list of effects modules added to the pool at start-up. Effects can also be added via code.
        /// </summary>
        [SerializeField] private List<StickyEffectsModule> startupEffectsModuleList = null;
        /// <summary>
        /// The list of modules added to the pool at start-up. Modules can also be added via code.
        /// </summary>
        [SerializeField] private List<StickyGenericModule> startupGenericModuleList = null;
        /// <summary>
        /// The list of projectile modules added to the pool at start-up. Projectile objects can also be added via code.
        /// </summary>
        [SerializeField] private List<StickyProjectileModule> startupProjectileModuleList = null;

        /// <summary>
        /// A reusable array of RaycastHit structs used in CheckCharacterHit() and CheckObjectHit().
        /// </summary>
        [System.NonSerialized] private RaycastHit[] raycastHitInfoArray;

        /// <summary>
        /// A random class for selecting prefabIDs from a list. e.g. Decals.
        /// </summary>
        [System.NonSerialized] private S3DRandom s3dRandomPrefab;

        #endregion

        #region Private Variables - Ammo

        /// <summary>
        /// Reference to a common set of ammo types. All weapons and magazines should use the same scriptable object.
        /// </summary>
        [SerializeField] private S3DAmmoTypes ammoTypes = null;

        private bool isAmmoTypesValid = false;

        #endregion

        #region Private Variables - Beams
        private List<S3DBeamTemplate> beamTemplatesList;
        private int numBeamTemplates = 0;
        private S3DBeamTemplate beamTemplate;
        private GameObject beamGameObjectInstance;
        private bool isBeamsPaused = false;
        private Transform beamPooledTrfm = null;
        #endregion

        #region Private Variables - Decals
        private List<S3DDecalTemplate> decalTemplatesList;
        private S3DDecalTemplate decalTemplate;
        private GameObject decalGameObjectInstance;
        private bool isDecalsPaused = false;
        private Transform decalPooledTrfm = null;
        private List<StickyDecalModule> tempDecalList;
        #endregion

        #region Private Variables - Dynamic Objects
        private List<S3DDynamicObjectTemplate> dynamicObjectTemplatesList;
        private S3DDynamicObjectTemplate dynamicObjectTemplate;
        private GameObject dynamicObjectGameObjectInstance;
        private bool isDynamicObjectsPaused = false;
        private Transform dynamicPooledTrfm = null;
        #endregion

        #region Private Variables - Effects Objects
        private List<S3DEffectsObjectTemplate> effectsObjectTemplatesList;
        private S3DEffectsObjectTemplate effectsObjectTemplate;
        private GameObject effectsObjectGameObjectInstance;
        private bool isEffectsObjectsPaused = false;
        private Transform effectsPooledTrfm = null;
        #endregion

        #region Private Variables - Generic Objects
        private List<S3DGenericObjectTemplate> genericObjectTemplatesList;
        private S3DGenericObjectTemplate genericObjectTemplate;
        private GameObject genericObjectGameObjectInstance;
        private bool isGenericObjectsPaused = false;
        private Transform genericPooledTrfm = null;
        #endregion

        #region Private Variables - Projectiles
        private List<S3DProjectileTemplate> projectileTemplatesList;
        private S3DProjectileTemplate projectileTemplate;
        private GameObject projectileGameObjectInstance;
        private bool isProjectilesPaused = false;
        private Transform projectilePooledTrfm = null;
        #endregion

        #region Events

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
        }

        #endregion

        #region Startup Methods

        /// <summary>
        /// Cater for the situation where the StickyManager is manually added to the
        /// scene in the editor and Startup Generic Modules are added via the editor.
        /// </summary>
        private void Start()
        {           
            if (!isInitialised && !isInitialising)
            {
                Initialise();
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Attempt to add a StickyManager to the a scene
        /// </summary>
        /// <param name="sceneHandle"></param>
        /// <returns></returns>
        private static StickyManager AddStickyManager(int sceneHandle)
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
                                Debug.LogWarning("[ERROR] StickyManager.AddStickyManager() - scene " + _scene.name + " is NOT loaded");
                            }
                            #endif
                            break;
                        }
                    }
                }
            }

            GameObject newManagerGameObject = new GameObject("Sticky Manager");
            newManagerGameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            newManagerGameObject.transform.SetParent(null);
            StickyManager _manager = newManagerGameObject.AddComponent<StickyManager>();
            _manager.sceneHandle = _manager.gameObject.scene.handle;
            return _manager;
        }

        #endregion

        #region Private and Internal Methods - Beams

        /// <summary>
        /// Adds a new beam template to the list.
        /// </summary>
        /// <param name="stickyBeamPrefab"></param>
        /// <param name="beamTransformID"></param>
        /// <returns></returns>
        private int AddBeamTemplate(StickyBeamModule stickyBeamPrefab, int beamTransformID)
        {
            // Create a new beam template for this prefab
            beamTemplate = new S3DBeamTemplate(stickyBeamPrefab, beamTransformID);
            // Add the new beam template to the end of the list
            beamTemplatesList.Add(beamTemplate);
            // Get the index of the new beam template
            int beamTemplateIndex = beamTemplatesList.Count - 1;

            numBeamTemplates = beamTemplatesList.Count;

            // Initialise beam pool with capacity of minimum pool size
            beamTemplate.currentPoolSize = beamTemplate.s3dBeamModulePrefab.minPoolSize;
            beamTemplate.s3dBeamPool = new List<GameObject>(beamTemplate.currentPoolSize);
            // Create the objects in the pool
            for (int i = 0; i < beamTemplate.currentPoolSize; i++)
            {
                // Instantiate the beam object
                beamGameObjectInstance = Instantiate(beamTemplate.s3dBeamModulePrefab.gameObject);
                // Set the object's parent to be the manager
                beamGameObjectInstance.transform.SetParent(beamPooledTrfm);

                // Pre-assign the StickyManager so we don't need to check it each time we call stickyBeamModule.Initialise()
                StickyBeamModule _stickyBeamModule;
                if (beamGameObjectInstance.TryGetComponent(out _stickyBeamModule))
                {
                    _stickyBeamModule.stickyManager = this;
                }

                // Set the object to inactive
                beamGameObjectInstance.SetActive(false);
                // Add the object to the list of pooled objects
                beamTemplate.s3dBeamPool.Add(beamGameObjectInstance);
            }

            return beamTemplateIndex;
        }

        /// <summary>
        /// Deactivate a beam
        /// </summary>
        /// <param name="beamItemKey"></param>
        internal void DeactivateBeam (S3DBeamItemKey beamItemKey)
        {
            if (beamItemKey.beamPoolListIndex >= 0 && beamItemKey.beamSequenceNumber != 0)
            {
                if (beamItemKey.beamTemplatesListIndex >= 0)
                {
                    S3DBeamTemplate _beamTemplate = beamTemplatesList[beamItemKey.beamTemplatesListIndex];

                    if (_beamTemplate != null && _beamTemplate.s3dBeamModulePrefab != null)
                    {
                        GameObject pmGO = _beamTemplate.s3dBeamPool[beamItemKey.beamPoolListIndex];
                        if (pmGO.activeInHierarchy)
                        {
                            StickyBeamModule beamModule = pmGO.GetComponent<StickyBeamModule>();

                            // Verify we have the correct beam
                            if (beamModule != null && beamModule.itemSequenceNumber == beamItemKey.beamSequenceNumber)
                            {
                                beamModule.DestroyBeam();
                            }
                        }
                    }
                }
            }
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

                // Pause or unpause all active Pooled Beams
                for (int ptIdx = 0; ptIdx < numBeamTemplates; ptIdx++)
                {
                    S3DBeamTemplate _beamTemplate = beamTemplatesList[ptIdx];
                    if (_beamTemplate != null && _beamTemplate.s3dBeamModulePrefab != null)
                    {
                        // Examine each of the beams in the pool
                        for (int pmIdx = 0; pmIdx < _beamTemplate.currentPoolSize; pmIdx++)
                        {
                            GameObject pmGO = _beamTemplate.s3dBeamPool[pmIdx];
                            if (pmGO.activeInHierarchy)
                            {
                                if (isPause) { pmGO.GetComponent<IStickyPoolable>().DisableModule(); }
                                else { pmGO.GetComponent<IStickyPoolable>().EnableModule(); }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Private and Internal Methods - Decal Modules

        /// <summary>
        /// Adds a new decal template to the list.
        /// </summary>
        /// <param name="decalPrefab"></param>
        /// <param name="decalTransformID"></param>
        private int AddDecalTemplate (StickyDecalModule decalPrefab, int decalTransformID)
        {
            // Create a new decal template for this prefab
            decalTemplate = new S3DDecalTemplate(decalPrefab, decalTransformID);
            // Add the new decal template to the end of the list
            decalTemplatesList.Add(decalTemplate);
            // Get the index of the new decal template
            int decalTemplateIndex = decalTemplatesList.Count - 1;

            // Initialise decal pool with capacity of minimum pool size
            decalTemplate.currentPoolSize = decalTemplate.s3dDecalModulePrefab.minPoolSize;
            decalTemplate.s3dDecalPool = new List<GameObject>(decalTemplate.currentPoolSize);
            // Create the objects in the pool
            for (int i = 0; i < decalTemplate.currentPoolSize; i++)
            {
                // Instantiate the decal
                decalGameObjectInstance = Instantiate(decalTemplate.s3dDecalModulePrefab.gameObject);
                // Set the object's parent to be the manager
                decalGameObjectInstance.transform.SetParent(decalPooledTrfm);

                // Prefetch the decal mesh verts and disable collider
                StickyDecalModule _stickyDecalModule;
                if (decalGameObjectInstance.TryGetComponent(out _stickyDecalModule))
                {
                    _stickyDecalModule.ValidateDecal();
                }

                // Set the object to inactive
                decalGameObjectInstance.SetActive(false);
                // Add the object to the list of pooled objects
                decalTemplate.s3dDecalPool.Add(decalGameObjectInstance);
            }

            return decalTemplateIndex;
        }

        /// <summary>
        /// Destroy the decal (return it to the pool).
        /// </summary>
        /// <param name="decalItemKey"></param>
        internal void DestroyDecal(S3DDecalItemKey decalItemKey)
        {
            // Is this effect pooled?
            if (decalItemKey.decalPoolListIndex >= 0 && decalItemKey.decalSequenceNumber != 0)
            {
                if (decalItemKey.decalTemplatesListIndex >= 0)
                {
                    // Ensure we have a valid StickyDecal template in the pooling system
                    S3DDecalTemplate _decalTemplate = decalTemplatesList[decalItemKey.decalTemplatesListIndex];

                    if (_decalTemplate != null && _decalTemplate.s3dDecalModulePrefab != null)
                    {
                        GameObject pmGO = _decalTemplate.s3dDecalPool[decalItemKey.decalPoolListIndex];
                        if (pmGO.activeInHierarchy)
                        {
                            StickyDecalModule _decalModule = pmGO.GetComponent<StickyDecalModule>();

                            // Verify we have the correct StickyDecal within the pool.
                            // This may not always be true. e.g., when an object is returned to the pool sooner than expected.
                            if (_decalModule != null && _decalModule.itemSequenceNumber == decalItemKey.decalSequenceNumber)
                            {
                                _decalModule.CancelInvoke(StickyDecalModule.destroyMethodName);
                                _decalModule.DestroyDecal();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// See also PauseDecals() and ResumeDecals() in Public API.
        /// Pause or unpause all Pooled decals in the scene
        /// </summary>
        /// <param name="isPause"></param>
        private void PauseDecals(bool isPause)
        {
            if (isInitialised)
            {
                isDecalsPaused = isPause;

                int numDecalTemplates = decalTemplatesList == null ? 0 : decalTemplatesList.Count;

                // Pause or unpause all active Pooled Decal Modules
                for (int eotIdx = 0; eotIdx < numDecalTemplates; eotIdx++)
                {
                    S3DDecalTemplate _decalTemplate = decalTemplatesList[eotIdx];
                    if (_decalTemplate != null && _decalTemplate.s3dDecalModulePrefab != null)
                    {
                        // Examine each of the effect module in the pool
                        for (int pmIdx = 0; pmIdx < _decalTemplate.currentPoolSize; pmIdx++)
                        {
                            GameObject emGO = _decalTemplate.s3dDecalPool[pmIdx];
                            if (emGO.activeInHierarchy)
                            {
                                if (isPause) { emGO.GetComponent<IStickyPoolable>().DisableModule(); }
                                else { emGO.GetComponent<IStickyPoolable>().EnableModule(); }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Private and Internal Methods - Dynamic Modules

        /// <summary>
        /// Adds a new dynamic object template to the list.
        /// </summary>
        /// <param name="dynamicObjectPrefab"></param>
        /// <param name="dynamicObjectTransformID"></param>
        private int AddDynamicObjectTemplate (StickyDynamicModule dynamicObjectPrefab, int dynamicObjectTransformID)
        {
            // Create a new dynamic object template for this prefab
            dynamicObjectTemplate = new S3DDynamicObjectTemplate(dynamicObjectPrefab, dynamicObjectTransformID);
            // Add the new dynamic object template to the end of the list
            dynamicObjectTemplatesList.Add(dynamicObjectTemplate);
            // Get the index of the new dynamic object template
            int dynamicObjectTemplateIndex = dynamicObjectTemplatesList.Count - 1;

            // Initialise dynamic pool with capacity of minimum pool size
            dynamicObjectTemplate.currentPoolSize = dynamicObjectTemplate.s3dDynamicModulePrefab.minPoolSize;
            dynamicObjectTemplate.s3dDynamicObjectPool = new List<GameObject>(dynamicObjectTemplate.currentPoolSize);
            // Create the objects in the pool
            for (int i = 0; i < dynamicObjectTemplate.currentPoolSize; i++)
            {
                // Instantiate the dynamic object
                dynamicObjectGameObjectInstance = Instantiate(dynamicObjectTemplate.s3dDynamicModulePrefab.gameObject);
                // Set the object's parent to be the manager
                dynamicObjectGameObjectInstance.transform.SetParent(dynamicPooledTrfm);
                // Set the object to inactive
                dynamicObjectGameObjectInstance.SetActive(false);
                // Add the object to the list of pooled objects
                dynamicObjectTemplate.s3dDynamicObjectPool.Add(dynamicObjectGameObjectInstance);
            }

            return dynamicObjectTemplateIndex;
        }

        /// <summary>
        /// Destroy the dynamic Object (return it to the pool).
        /// </summary>
        /// <param name="dynamicItemKey"></param>
        internal void DestroyDynamicObject(S3DDynamicItemKey dynamicItemKey)
        {
            // Is this dynamic pooled?
            if (dynamicItemKey.dynamicObjectPoolListIndex >= 0 && dynamicItemKey.dynamicObjectSequenceNumber != 0)
            {
                if (dynamicItemKey.dynamicObjectTemplatesListIndex >= 0)
                {
                    // Ensure we have a valid StickyDynamicModule template in the pooling system
                    S3DDynamicObjectTemplate _dynamicTemplate = dynamicObjectTemplatesList[dynamicItemKey.dynamicObjectTemplatesListIndex];

                    if (_dynamicTemplate != null && _dynamicTemplate.s3dDynamicModulePrefab != null)
                    {
                        GameObject pmGO = _dynamicTemplate.s3dDynamicObjectPool[dynamicItemKey.dynamicObjectPoolListIndex];
                        if (pmGO.activeInHierarchy)
                        {
                            StickyDynamicModule _dynamicModule = pmGO.GetComponent<StickyDynamicModule>();

                            // Verify we have the correct dynamic object within the pool.
                            // This may not always be true. e.g., when an object is returned to the pool sooner than expected.
                            if (_dynamicModule != null && _dynamicModule.itemSequenceNumber == dynamicItemKey.dynamicObjectSequenceNumber)
                            {
                                _dynamicModule.CancelInvoke(StickyDynamicModule.destroyMethodName);
                                _dynamicModule.DestroyDynamicObject();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// See also PauseDynamicObjects() and ResumeDynamicObjects() in Public API.
        /// Pause or unpause all Pooled dynamic objects in the scene
        /// </summary>
        /// <param name="isPause"></param>
        private void PauseDynamicObjects(bool isPause)
        {
            if (isInitialised)
            {
                isDynamicObjectsPaused = isPause;

                int numDynamicTemplates = dynamicObjectTemplatesList == null ? 0 : dynamicObjectTemplatesList.Count;

                // Pause or unpause all active Pooled Dynamic Modules
                for (int eotIdx = 0; eotIdx < numDynamicTemplates; eotIdx++)
                {
                    S3DDynamicObjectTemplate _dynamicTemplate = dynamicObjectTemplatesList[eotIdx];
                    if (_dynamicTemplate != null && _dynamicTemplate.s3dDynamicModulePrefab != null)
                    {
                        // Examine each of the effect module in the pool
                        for (int pmIdx = 0; pmIdx < _dynamicTemplate.currentPoolSize; pmIdx++)
                        {
                            GameObject emGO = _dynamicTemplate.s3dDynamicObjectPool[pmIdx];
                            if (emGO.activeInHierarchy)
                            {
                                if (isPause) { emGO.GetComponent<IStickyPoolable>().DisableModule(); }
                                else { emGO.GetComponent<IStickyPoolable>().EnableModule(); }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Private and Internal Methods - Effects Modules

        /// <summary>
        /// Adds a new effects object template to the list.
        /// </summary>
        /// <param name="effectsObjectPrefab"></param>
        /// <param name="effectsObjectTransformID"></param>
        private int AddEffectsObjectTemplate (StickyEffectsModule effectsObjectPrefab, int effectsObjectTransformID)
        {
            // Create a new effects object template for this prefab
            effectsObjectTemplate = new S3DEffectsObjectTemplate(effectsObjectPrefab, effectsObjectTransformID);
            // Add the new effects object template to the end of the list
            effectsObjectTemplatesList.Add(effectsObjectTemplate);
            // Get the index of the new effects object template
            int effectsObjectTemplateIndex = effectsObjectTemplatesList.Count - 1;

            // Initialise effects pool with capacity of minimum pool size
            effectsObjectTemplate.currentPoolSize = effectsObjectTemplate.s3dEffectsModulePrefab.minPoolSize;
            effectsObjectTemplate.s3dEffectsObjectPool = new List<GameObject>(effectsObjectTemplate.currentPoolSize);
            // Create the objects in the pool
            for (int i = 0; i < effectsObjectTemplate.currentPoolSize; i++)
            {
                // Instantiate the effects object
                effectsObjectGameObjectInstance = Instantiate(effectsObjectTemplate.s3dEffectsModulePrefab.gameObject);
                // Set the object's parent to be the manager
                effectsObjectGameObjectInstance.transform.SetParent(effectsPooledTrfm);
                // Set the object to inactive
                effectsObjectGameObjectInstance.SetActive(false);
                // Add the object to the list of pooled objects
                effectsObjectTemplate.s3dEffectsObjectPool.Add(effectsObjectGameObjectInstance);
            }

            return effectsObjectTemplateIndex;
        }

        /// <summary>
        /// Destroy the effects Object (return it to the pool).
        /// </summary>
        /// <param name="effectItemKey"></param>
        internal void DestroyEffectsObject(S3DEffectItemKey effectItemKey)
        {
            // Is this effect pooled?
            if (effectItemKey.effectsObjectPoolListIndex >= 0 && effectItemKey.effectsObjectSequenceNumber != 0)
            {
                if (effectItemKey.effectsObjectTemplatesListIndex >= 0)
                {
                    // Ensure we have a valid StickyEffectsObject template in the pooling system
                    S3DEffectsObjectTemplate _effectsTemplate = effectsObjectTemplatesList[effectItemKey.effectsObjectTemplatesListIndex];

                    if (_effectsTemplate != null && _effectsTemplate.s3dEffectsModulePrefab != null)
                    {
                        GameObject pmGO = _effectsTemplate.s3dEffectsObjectPool[effectItemKey.effectsObjectPoolListIndex];
                        if (pmGO.activeInHierarchy)
                        {
                            StickyEffectsModule _effectsModule = pmGO.GetComponent<StickyEffectsModule>();

                            // Verify we have the correct StickyEffectsObject within the pool.
                            // This may not always be true. e.g., when an object is returned to the pool sooner than expected.
                            if (_effectsModule != null && _effectsModule.itemSequenceNumber == effectItemKey.effectsObjectSequenceNumber)
                            {
                                _effectsModule.CancelInvoke(StickyEffectsModule.destroyMethodName);
                                _effectsModule.DestroyEffectsObject();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determine if the effects item is current. If it has been despawned OR respawned again it will return false.
        /// </summary>
        /// <param name="effectItemKey"></param>
        /// <returns></returns>
        internal bool IsEffectsObjectCurrent (S3DEffectItemKey effectItemKey)
        {
            bool isValid = false;

            // Is this effect pooled AND do we have a reference to the item in the list of pooled prefab instances?
            if (effectItemKey.effectsObjectPoolListIndex >= 0 && effectItemKey.effectsObjectTemplatesListIndex >= 0)
            {
                // Get the template
                S3DEffectsObjectTemplate _effectsTemplate = effectsObjectTemplatesList[effectItemKey.effectsObjectTemplatesListIndex];

                // Does the template and template prefab look ok?
                if (_effectsTemplate != null && _effectsTemplate.s3dEffectsModulePrefab != null)
                {
                    // Get the actual gamobject
                    GameObject pmGO = _effectsTemplate.s3dEffectsObjectPool[effectItemKey.effectsObjectPoolListIndex];

                    // If it still active (not despawned) AND has a matching sequence number, it will be valid
                    isValid = pmGO.activeInHierarchy && pmGO.GetComponent<StickyEffectsModule>().itemSequenceNumber == effectItemKey.effectsObjectSequenceNumber;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Change the transform position and rotation of an EffectsObject.
        /// It is faster to not validate sequence numbers for pooled effects.
        /// </summary>
        /// <param name="effectItemKey"></param>
        /// <param name="newPosition"></param>
        /// <param name="newRotation"></param>
        /// <param name="isValidateSequenceNumber"></param>
        internal bool MoveEffectsObject(S3DEffectItemKey effectItemKey, Vector3 newPosition, Quaternion newRotation, bool isValidateSequenceNumber)
        {
            bool isEffectActiveAndValid = false;

            // Is this effect pooled?
            if (effectItemKey.effectsObjectPoolListIndex >= 0 && effectItemKey.effectsObjectSequenceNumber != 0)
            {
                // Do we have a reference to the item in the list of pooled prefab instances?
                if (effectItemKey.effectsObjectTemplatesListIndex >= 0)
                {
                    // Get the template
                    S3DEffectsObjectTemplate _effectsTemplate = effectsObjectTemplatesList[effectItemKey.effectsObjectTemplatesListIndex];

                    // Does the template and template prefab look ok?
                    if (_effectsTemplate != null && _effectsTemplate.s3dEffectsModulePrefab != null)
                    {
                        GameObject pmGO = _effectsTemplate.s3dEffectsObjectPool[effectItemKey.effectsObjectPoolListIndex];
                        if (pmGO.activeInHierarchy)
                        {
                            if (!isValidateSequenceNumber || pmGO.GetComponent<StickyEffectsModule>().itemSequenceNumber == effectItemKey.effectsObjectSequenceNumber)
                            {
                                pmGO.transform.SetPositionAndRotation(newPosition, newRotation);
                                isEffectActiveAndValid = true;
                            }
                        }
                    }
                }
            }

            return isEffectActiveAndValid;
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// See also PauseEffectsObjects() and ResumeEffectsObjects() in Public API.
        /// Pause or unpause all Pooled effects objects in the scene
        /// </summary>
        /// <param name="isPause"></param>
        private void PauseEffectsObjects(bool isPause)
        {
            if (isInitialised)
            {
                isEffectsObjectsPaused = isPause;

                int numEffectsTemplates = effectsObjectTemplatesList == null ? 0 : effectsObjectTemplatesList.Count;

                // Pause or unpause all active Pooled Effects Modules
                for (int eotIdx = 0; eotIdx < numEffectsTemplates; eotIdx++)
                {
                    S3DEffectsObjectTemplate _effectsTemplate = effectsObjectTemplatesList[eotIdx];
                    if (_effectsTemplate != null && _effectsTemplate.s3dEffectsModulePrefab != null)
                    {
                        // Examine each of the effect module in the pool
                        for (int pmIdx = 0; pmIdx < _effectsTemplate.currentPoolSize; pmIdx++)
                        {
                            GameObject emGO = _effectsTemplate.s3dEffectsObjectPool[pmIdx];
                            if (emGO.activeInHierarchy)
                            {
                                if (isPause) { emGO.GetComponent<IStickyPoolable>().DisableModule(); }
                                else { emGO.GetComponent<IStickyPoolable>().EnableModule(); }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Private and Internal Methods - Generic Objects

        /// <summary>
        /// Adds a new generic object template to the list.
        /// </summary>
        /// <param name="genericObjectPrefab"></param>
        /// <param name="genericObjectTransformID"></param>
        /// <returns></returns>
        private int AddGenericObjectTemplate(StickyGenericModule genericObjectPrefab, int genericObjectTransformID)
        {
            // Create a new generic object template for this prefab
            genericObjectTemplate = new S3DGenericObjectTemplate(genericObjectPrefab, genericObjectTransformID);
            // Add the new generic object template to the end of the list
            genericObjectTemplatesList.Add(genericObjectTemplate);
            // Get the index of the new generic object template
            int genericObjectTemplateIndex = genericObjectTemplatesList.Count - 1;

            // Initialise generic pool with capacity of minimum pool size
            genericObjectTemplate.currentPoolSize = genericObjectTemplate.s3dGenericModulePrefab.minPoolSize;
            genericObjectTemplate.s3dGenericObjectPool = new List<GameObject>(genericObjectTemplate.currentPoolSize);
            // Create the objects in the pool
            for (int i = 0; i < genericObjectTemplate.currentPoolSize; i++)
            {
                // Instantiate the generic object
                genericObjectGameObjectInstance = Instantiate(genericObjectTemplate.s3dGenericModulePrefab.gameObject);
                // Set the object's parent to be the manager
                genericObjectGameObjectInstance.transform.SetParent(genericPooledTrfm);
                // Set the object to inactive
                genericObjectGameObjectInstance.SetActive(false);
                // Add the object to the list of pooled objects
                genericObjectTemplate.s3dGenericObjectPool.Add(genericObjectGameObjectInstance);
            }

            return genericObjectTemplateIndex;
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// See also PauseGenericObjects() and ResumeGenericObjects() in Public API.
        /// Pause or unpause all pooled generic objects in the scene.
        /// </summary>
        /// <param name="isPause"></param>
        private void PauseGenericObjects(bool isPause)
        {
            if (isInitialised)
            {
                isGenericObjectsPaused = isPause;

                int numGenericTemplates = genericObjectTemplatesList == null ? 0 : genericObjectTemplatesList.Count;

                // Pause or unpause all active Pooled Generic Modules
                for (int gotIdx = 0; gotIdx < numGenericTemplates; gotIdx++)
                {
                    S3DGenericObjectTemplate _genericTemplate = genericObjectTemplatesList[gotIdx];
                    if (_genericTemplate != null && _genericTemplate.s3dGenericModulePrefab != null)
                    {
                        // Examine each of the generic modules in the pool
                        for (int gmIdx = 0; gmIdx < _genericTemplate.currentPoolSize; gmIdx++)
                        {
                            GameObject emGo = _genericTemplate.s3dGenericObjectPool[gmIdx];
                            if (emGo.activeInHierarchy)
                            {
                                if (isPause) { emGo.GetComponent<IStickyPoolable>().DisableModule(); }
                                else { emGo.GetComponent<IStickyPoolable>().EnableModule(); }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Private and Internal Methods - Projectile Modules

        /// <summary>
        /// Adds a new projectile template to the list.
        /// </summary>
        /// <param name="stickyProjectilePrefab"></param>
        /// <param name="projectileTransformID"></param>
        /// <returns></returns>
        private int AddProjectileTemplate(StickyProjectileModule stickyProjectilePrefab, int projectileTransformID)
        {
            // Create a new projectile template for this prefab
            projectileTemplate = new S3DProjectileTemplate(stickyProjectilePrefab, projectileTransformID);
            // Add the new projectile template to the end of the list
            projectileTemplatesList.Add(projectileTemplate);
            // Get the index of the new projectile template
            int projectileTemplateIndex = projectileTemplatesList.Count - 1;

            // Initialise projectile pool with capacity of minimum pool size
            projectileTemplate.currentPoolSize = projectileTemplate.s3dProjectileModulePrefab.minPoolSize;
            projectileTemplate.s3dProjectilePool = new List<GameObject>(projectileTemplate.currentPoolSize);

            // Populate an array of pooled decal module prefabIDs.
            projectileTemplate.numberOfDecals = stickyProjectilePrefab.s3dDecals == null ? 0 : stickyProjectilePrefab.s3dDecals.NumberOfDecals;

            if (projectileTemplate.numberOfDecals > 0)
            {
                projectileTemplate.decalPrefabIDs = new int[projectileTemplate.numberOfDecals];
                CreateDecalPools(stickyProjectilePrefab.s3dDecals, projectileTemplate.decalPrefabIDs);
            }
            //else
            //{
            //    CreateDecalPools(stickyProjectilePrefab.s3dDecals);
            //}

            // Create the objects in the pool
            for (int i = 0; i < projectileTemplate.currentPoolSize; i++)
            {
                // Instantiate the projectile object
                projectileGameObjectInstance = Instantiate(projectileTemplate.s3dProjectileModulePrefab.gameObject);
                // Set the object's parent to be the manager
                projectileGameObjectInstance.transform.SetParent(projectilePooledTrfm);

                // Pre-assign the StickyManager so we don't need to check it each time we call stickyProjectileModule.Initialise()
                // Pre-assign the default decals
                StickyProjectileModule _stickyProjectileModule;
                if (projectileGameObjectInstance.TryGetComponent(out _stickyProjectileModule))
                {
                    _stickyProjectileModule.stickyManager = this;
                    _stickyProjectileModule.numberOfDecals = projectileTemplate.numberOfDecals;
                }

                // Set the object to inactive
                projectileGameObjectInstance.SetActive(false);
                // Add the object to the list of pooled objects
                projectileTemplate.s3dProjectilePool.Add(projectileGameObjectInstance);
            }

            return projectileTemplateIndex;
        }

        /// <summary>
        /// [INTERNAL ONLY]
        /// See also PauseProjectiles() and ResumeProjectiles() in Public API
        /// Pause or unpause all Pooled Projectiles in the scene
        /// </summary>
        /// <param name="isPause"></param>
        private void PauseProjectiles(bool isPause)
        {
            if (isInitialised)
            {
                isProjectilesPaused = isPause;

                int numProjectileTemplates = projectileTemplatesList == null ? 0 : projectileTemplatesList.Count;

                // Pause or unpause all active Pooled Projectiles
                for (int ptIdx = 0; ptIdx < numProjectileTemplates; ptIdx++)
                {
                    S3DProjectileTemplate _projectileTemplate = projectileTemplatesList[ptIdx];
                    if (_projectileTemplate != null && _projectileTemplate.s3dProjectileModulePrefab != null)
                    {
                        // Examine each of the projectiles in the pool
                        for (int pmIdx = 0; pmIdx < _projectileTemplate.currentPoolSize; pmIdx++)
                        {
                            GameObject pmGO = _projectileTemplate.s3dProjectilePool[pmIdx];
                            if (pmGO.activeInHierarchy)
                            {
                                if (isPause) { pmGO.GetComponent<IStickyPoolable>().DisableModule(); }
                                else { pmGO.GetComponent<IStickyPoolable>().EnableModule(); }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Public API Static Methods

        /// <summary>
        /// Attempt to find the first StickyManager in the scene.
        /// Typically you should be using GetOrCreateManager().
        /// </summary>
        /// <returns></returns>
        public static StickyManager FindFirstManager()
        {
            #if UNITY_2022_2_OR_NEWER
            return GameObject.FindFirstObjectByType<StickyManager>();
            #else
            return GameObject.FindObjectOfType<StickyManager>();
            #endif
        }

        /// <summary>
        /// Returns the current Sticky Manager instance for this scene. If one does not already exist, a new one is created.
        /// If the manager is not initialised, it will be initialised.
        /// For multi-additive scenes, pass in the current scene handle e.g., gameObject.scene.handle.
        /// </summary>
        /// <returns></returns>
        public static StickyManager GetOrCreateManager (int sceneHandle = 0)
        {
            StickyManager _manager = null;

            // Attempt to find the handle in the list of current StickyManagers
            for (int mIdx = numberManagers-1; mIdx >= 0; mIdx--)
            {
                StickyManager _tempManager = managerList[mIdx];
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

            // If there wasn't a manager in the list, see if we can find one in the indicated scene.
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

                        // Assume StickyManager is always a root level object
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
                    _manager = AddStickyManager(sceneHandle);
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
                Debug.LogWarning("StickyManager GetOrCreateManager() Warning: Could not find or create manager, so returned null.");
            }
            #endif
             
            return _manager;


            //// Check whether we have already found a manager for this scene
            //if (currentManager == null)
            //{
            //    // Otherwise, check whether this scene already has a manager
            //    currentManager = FindFirstManager();

            //    if (currentManager == null)
            //    {
            //        // If this scene does not already have a manager, create one
            //        GameObject newManagerGameObject = new GameObject("Sticky Manager");
            //        newManagerGameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            //        newManagerGameObject.transform.SetParent(null);
            //        currentManager = newManagerGameObject.AddComponent<StickyManager>();
            //    }
            //}

            //if (currentManager != null)
            //{
            //    // Initialise the manager if it hasn't already been initialised
            //    if (!currentManager.isInitialised) { currentManager.Initialise(); }
            //}
            //#if UNITY_EDITOR
            //// If currentManager is still null, log a warning to the console
            //else
            //{
            //    Debug.LogWarning("StickyManager GetOrCreateManager() Warning: Could not find or create manager, so returned null.");
            //}
            //#endif

            //return currentManager;
        }

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Initialise the Sticky Manager
        /// </summary>
        public void Initialise()
        {
            if (isInitialising) { return; }

            isInitialising = true;

            #region Beams
            // Initialise the beam templates list with an initial capacity of 5 beam types
            beamTemplatesList = new List<S3DBeamTemplate>(5);

            // Beam child transforms
            beamPooledTrfm = S3DUtils.GetOrCreateChildTransform(transform, "BeamPooled");
            #endregion

            #region Decals
            // Initialise the decal templates list with an initial capacity of 10 decal types
            decalTemplatesList = new List<S3DDecalTemplate>(10);

            // Decal child transforms
            decalPooledTrfm = S3DUtils.GetOrCreateChildTransform(transform, "DecalPooled");

            // Intialise a temporary list of decals for short-term use
            tempDecalList = new List<StickyDecalModule>(10);
            #endregion

            #region Dynamic Objects
            // Initialise the dynamic object templates list with an initial capacity of 10 dynamic object types
            dynamicObjectTemplatesList = new List<S3DDynamicObjectTemplate>(10);

            // Dynamic Objects child transforms
            dynamicPooledTrfm = S3DUtils.GetOrCreateChildTransform(transform, "DynamicPooled");
            #endregion

            #region Effects Objects
            // Initialise the effects object templates list with an initial capacity of 25 effects object types
            effectsObjectTemplatesList = new List<S3DEffectsObjectTemplate>(25);

            // Effects Objects child transforms
            effectsPooledTrfm = S3DUtils.GetOrCreateChildTransform(transform, "EffectPooled");
            #endregion

            #region Generic Objects
            // Initialise the genric object templates list with an initial capacity of 25 generic object types
            genericObjectTemplatesList = new List<S3DGenericObjectTemplate>(25);

            // Generic object child transforms
            genericPooledTrfm = S3DUtils.GetOrCreateChildTransform(transform, "GenericPooled");
            #endregion

            #region Projectiles
            // Initialise the projectile templates list with an initial capacity of 5 projectile types
            projectileTemplatesList = new List<S3DProjectileTemplate>(5);

            // Projectile child transforms
            projectilePooledTrfm = S3DUtils.GetOrCreateChildTransform(transform, "ProjectilePooled");
            #endregion

            #region Intialise temp Raycast variables
            // Temp reusable hit array
            raycastHitInfoArray = new RaycastHit[20];
            #endregion

            #region Random number generators

            s3dRandomPrefab = new S3DRandom();
            s3dRandomPrefab.SetSeed(4451);

            #endregion

            #region Ammo Types
            isAmmoTypesValid = ammoTypes != null;

            #endregion

            isInitialised = true;

            #region Start-up Beam, Dynamic, Effects, Projectiles, and Generic modules

            if (startupBeamModuleList != null)
            {
                // Add startup beam modules to the pooled objects
                for (int bmIdx = 0; bmIdx < startupBeamModuleList.Count; bmIdx++)
                {
                    StickyBeamModule stickyBeamModule = startupBeamModuleList[bmIdx];

                    if (stickyBeamModule != null)
                    {
                        GetOrCreateBeamPool(stickyBeamModule);
                    }
                }
            }

            if (startupDecalsList != null)
            {
                // Loop through each set of decals
                for (int dcsIdx = 0; dcsIdx < startupDecalsList.Count; dcsIdx++)
                {
                    S3DDecals s3dDecals = startupDecalsList[dcsIdx];

                    if (s3dDecals != null)
                    {
                        CreateDecalPools(s3dDecals);
                    }
                }
            }

            if (startupDynamicModuleList != null)
            {
                // Add startup dynamic modules to the pooled objects
                for (int dmIdx = 0; dmIdx < startupDynamicModuleList.Count; dmIdx++)
                {
                    StickyDynamicModule stickyDynamicModule = startupDynamicModuleList[dmIdx];

                    if (stickyDynamicModule != null)
                    {
                        GetOrCreateDynamicPool(stickyDynamicModule);
                    }
                }
            }

            if (startupEffectsModuleList != null)
            {
                // Add startup effects modules to the pooled objects
                for (int emIdx = 0; emIdx < startupEffectsModuleList.Count; emIdx++)
                {
                    StickyEffectsModule stickyEffectsModule = startupEffectsModuleList[emIdx];

                    if (stickyEffectsModule != null)
                    {
                        GetOrCreateEffectsPool(stickyEffectsModule);
                    }
                }
            }

            if (startupProjectileModuleList != null)
            {
                // Add startup beam modules to the pooled objects
                for (int pmIdx = 0; pmIdx < startupProjectileModuleList.Count; pmIdx++)
                {
                    StickyProjectileModule stickyProjectileModule = startupProjectileModuleList[pmIdx];

                    if (stickyProjectileModule != null)
                    {
                        GetOrCreateProjectilePool(stickyProjectileModule);
                    }
                }
            }

            if (startupGenericModuleList != null)
            {
                // Add startup modules to the pooled objects
                for (int gmIdx = 0; gmIdx < startupGenericModuleList.Count; gmIdx++)
                {
                    StickyGenericModule stickyGenericModule = startupGenericModuleList[gmIdx];

                    if (stickyGenericModule != null)
                    {
                        GetOrCreateGenericPool(stickyGenericModule);
                    }
                }
            }

            #endregion

            isInitialising = false;
        }

        /// <summary>
        /// This should be automatically called when a weapon is initialised
        /// or weapon.ReInitialiseWeapon() is called.
        /// Update
        /// - beamPrefabId
        /// - beamPrefab.effectsObjectPrefabID
        /// - muzzleEffectsObjectPrefabIDs
        /// - projectilePrefabID
        /// - projectilePrefab.effectsObjectPrefabID
        /// - spentCartridgePrefabId
        /// </summary>
        /// <param name="stickyWeapon"></param>
        public void ReinitialiseWeaponPoolItems (StickyWeapon stickyWeapon)
        {
            #region Beam Weapon
            if (stickyWeapon.IsBeamWeapon)
            {
                if (stickyWeapon.beamPrefab != null)
                {
                    // Get the transform instance ID for this beam prefab
                    int beamTransformID = stickyWeapon.beamPrefab.transform.GetInstanceID();
                    // Search the beam template list to see if we already have a 
                    // beam prefab with the same instance ID
                    int beamTemplateIndex = beamTemplatesList.FindIndex(p => p.instanceID == beamTransformID);

                    if (beamTemplateIndex == -1)
                    {
                        // If no match was found, create a new beam template for this prefab
                        stickyWeapon.beamPrefabID = AddBeamTemplate(stickyWeapon.beamPrefab, beamTransformID);
                    }
                    else
                    {
                        // Save the beam template index in the weapon
                        stickyWeapon.beamPrefabID = beamTemplateIndex;
                    }

                    // Check if the beam has a hit effects object
                    if (stickyWeapon.beamPrefab.effectsObject != null)
                    {
                        // Get the transform instance ID for this effects object prefab
                        int effectsObjectTransformID = stickyWeapon.beamPrefab.effectsObject.transform.GetInstanceID();
                        // Search the effects object templates list to see if we already have an 
                        // effects object prefab with the same instance ID
                        int effectsObjectTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsObjectTransformID);

                        if (effectsObjectTemplateIndex == -1)
                        {
                            // If no match was found, create a new effects object template for this prefab
                            stickyWeapon.beamPrefab.effectsObjectPrefabID = AddEffectsObjectTemplate(stickyWeapon.beamPrefab.effectsObject, effectsObjectTransformID);
                        }
                        else
                        {
                            // Save the effect template index in the beam
                            stickyWeapon.beamPrefab.effectsObjectPrefabID = effectsObjectTemplateIndex;
                        }
                    }
                    // No default hit effects object for this beam
                    else { stickyWeapon.beamPrefab.effectsObjectPrefabID = NoPrefabID; }

                    stickyWeapon.EstimatedRange = stickyWeapon.MaxRange;
                }
                else
                {
                    stickyWeapon.beamPrefabID = NoPrefabID;
                }
            }
            #endregion

            #region Projectile Weapon (Standard or Raycast)
            else if (stickyWeapon.IsProjectileWeapon)
            {
                if (stickyWeapon.projectilePrefab != null)
                {
                    // Get the transform instance ID for this projectile prefab
                    int projectileTransformID = stickyWeapon.projectilePrefab.transform.GetInstanceID();
                    // Search the projectile template list to see if we already have a 
                    // projectile prefab with the same instance ID
                    int projectileTemplateIndex = projectileTemplatesList.FindIndex(p => p.instanceID == projectileTransformID);

                    if (projectileTemplateIndex == -1)
                    {
                        // If no match was found, create a new projectile template for this prefab
                        stickyWeapon.projectilePrefabID = AddProjectileTemplate(stickyWeapon.projectilePrefab, projectileTransformID);
                    }
                    else
                    {
                        // Save the projectile template index in the weapon
                        stickyWeapon.projectilePrefabID = projectileTemplateIndex;
                    }

                    // Check if the projectile has a hit effects object
                    if (stickyWeapon.projectilePrefab.effectsObject != null)
                    {
                        // Get the transform instance ID for this effects object prefab
                        int effectsObjectTransformID = stickyWeapon.projectilePrefab.effectsObject.transform.GetInstanceID();
                        // Search the effects object templates list to see if we already have an 
                        // effects object prefab with the same instance ID
                        int effectsObjectTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsObjectTransformID);

                        if (effectsObjectTemplateIndex == -1)
                        {
                            // If no match was found, create a new effects object template for this prefab
                            stickyWeapon.projectilePrefab.effectsObjectPrefabID = AddEffectsObjectTemplate(stickyWeapon.projectilePrefab.effectsObject, effectsObjectTransformID);
                        }
                        else
                        {
                            // Save the effect template index in the projectile
                            stickyWeapon.projectilePrefab.effectsObjectPrefabID = effectsObjectTemplateIndex;
                        }
                    }
                    // No default hit effects object for this projectile
                    else { stickyWeapon.projectilePrefab.effectsObjectPrefabID = NoPrefabID; }
                }
                else
                {
                    stickyWeapon.projectilePrefabID = NoPrefabID;
                }
            }
            #endregion

            #region Muzzle Effects

            // Loop through the array of Muzzle EffectsModules configured in the editor
            // or at runtime by the game dev.
            if (stickyWeapon.NumberMuzzleEffects1 > 0)
            {
                StickyEffectsModule[] muzzleEffectsObjects = stickyWeapon.MuzzleEffects1;
                int numValidated = 0;

                // Loop throught the array to get the number of validate FX.
                // This avoids having to create a List and converting it to an array.
                for (int mFXIdx = 0; mFXIdx < stickyWeapon.NumberMuzzleEffects1; mFXIdx++)
                {
                    StickyEffectsModule effectsObject = muzzleEffectsObjects[mFXIdx];

                    if (effectsObject != null)
                    {
                        // Do any other validation required here

                        numValidated++;
                    }
                }

                // Allocate the array
                stickyWeapon.muzzleEffectsObject1PrefabIDs = new int[numValidated];

                // Find matching template effects or add new ones
                for (int mFXIdx = 0; mFXIdx < stickyWeapon.NumberMuzzleEffects1; mFXIdx++)
                {
                    StickyEffectsModule effectsObject = muzzleEffectsObjects[mFXIdx];

                    if (effectsObject != null)
                    {
                        // Do any other validation required here

                        // Get the transform instance ID for this effects object prefab
                        int effectsObjectTransformID = effectsObject.transform.GetInstanceID();
                        // Search the effects object templates list to see if we already have an 
                        // effects object prefab with the same instance ID
                        int effectsObjectTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsObjectTransformID);

                        if (effectsObjectTemplateIndex == -1)
                        {
                            // If no match was found, create a new effects object template for this prefab
                            //effectsObject.effectsObjectPrefabID = AddEffectsObjectTemplate(effectsObject, effectsObjectTransformID);
                            stickyWeapon.muzzleEffectsObject1PrefabIDs[mFXIdx] = AddEffectsObjectTemplate(effectsObject, effectsObjectTransformID);
                        }
                        else
                        {
                            // Save the effect template index in the array of validated muzzle FX for this weapon
                            stickyWeapon.muzzleEffectsObject1PrefabIDs[mFXIdx] = effectsObjectTemplateIndex;
                        }
                    }
                }

                stickyWeapon.NumberMuzzleEffects1Valid = numValidated;
            }

            #endregion

            #region Reloadable

            // Get or create pools for prefabs used for reloadable weapons
            if (stickyWeapon.IsReloadable)
            {
                StickyEffectsModule reloadSoundFX = stickyWeapon.ReloadSoundFX1;
                stickyWeapon.reloadSoundFXPrefabID1 = reloadSoundFX == null ? NoPrefabID : GetOrCreateSoundFXPool(reloadSoundFX);

                reloadSoundFX = stickyWeapon.ReloadSoundFX2;
                stickyWeapon.reloadSoundFXPrefabID2 = reloadSoundFX == null ? NoPrefabID : GetOrCreateSoundFXPool(reloadSoundFX);

                reloadSoundFX = stickyWeapon.ReloadEquipSoundFX1;
                stickyWeapon.reloadEquipSoundFXPrefabID1 = reloadSoundFX == null ? NoPrefabID : GetOrCreateSoundFXPool(reloadSoundFX);

                reloadSoundFX = stickyWeapon.ReloadEquipSoundFX2;
                stickyWeapon.reloadEquipSoundFXPrefabID2 = reloadSoundFX == null ? NoPrefabID : GetOrCreateSoundFXPool(reloadSoundFX);
            }

            #endregion

            #region Spent Cartridges
            if (stickyWeapon.spentCartridgePrefab != null)
            {
                // Get the transform instance ID for this dynamic prefab
                int dynamicTransformID = stickyWeapon.spentCartridgePrefab.transform.GetInstanceID();
                // Search the dynamic template list to see if we already have a 
                // dynamic prefab with the same instance ID
                int dynamicTemplateIndex = dynamicObjectTemplatesList.FindIndex(p => p.instanceID == dynamicTransformID);

                if (dynamicTemplateIndex == -1)
                {
                    // If no match was found, create a new dynamic template for this prefab
                    stickyWeapon.spentCartridgePrefabID = AddDynamicObjectTemplate(stickyWeapon.spentCartridgePrefab, dynamicTransformID);
                }
                else
                {
                    // Save the dynamic template index in the weapon
                    stickyWeapon.spentCartridgePrefabID = dynamicTemplateIndex;
                }
            }
            else
            {
                stickyWeapon.spentCartridgePrefabID = StickyManager.NoPrefabID;
            }

            #endregion
        }

        #endregion

        #region Public API Methods - Ammo

        /// <summary>
        /// Get the Damage Multiplier for an ammo type.
        /// </summary>
        /// <param name="ammoType"></param>
        /// <returns></returns>
        public float GetAmmoDamageMultiplier (S3DAmmo.AmmoType ammoType)
        {
            return GetAmmoDamageMultiplier((int)ammoType);
        }

        /// <summary>
        /// Get the Damage Multiplier for an ammo type.
        /// </summary>
        /// <param name="ammoTypeInt"></param>
        /// <returns></returns>
        public float GetAmmoDamageMultiplier (int ammoTypeInt)
        {
            if (isAmmoTypesValid && ammoTypeInt >= 0 && ammoTypeInt < 26)
            {
                return ammoTypes.ammoTypes[ammoTypeInt].damageMultiplier;
            }
            else { return 1f; }
        }

        /// <summary>
        /// Get the (force) impact multiplier for an ammo type.
        /// </summary>
        /// <param name="ammoType"></param>
        /// <returns></returns>
        public float GetAmmoImpactMultiplier (S3DAmmo.AmmoType ammoType)
        {
            return GetAmmoImpactMultiplier((int)ammoType);
        }

        /// <summary>
        /// Get the (force) impact multiplier for an ammo type.
        /// </summary>
        /// <param name="ammoTypeInt"></param>
        /// <returns></returns>
        public float GetAmmoImpactMultiplier (int ammoTypeInt)
        {
            if (isAmmoTypesValid && ammoTypeInt >= 0 && ammoTypeInt < 26)
            {
                return ammoTypes.ammoTypes[ammoTypeInt].impactMultiplier;
            }
            else { return 1f; }
        }

        /// <summary>
        /// Set or update the list of (common) ammo types.
        /// Generally only used Internally. You would need a good
        /// reason to have to use this in your code. If in doubt, ask us.
        /// </summary>
        /// <param name="newAmmoTypes"></param>
        public void SetAmmoTypes (S3DAmmoTypes newAmmoTypes)
        {
            if (S3DAmmoTypes.IsAmmoTypeValid(newAmmoTypes))
            {
                ammoTypes = newAmmoTypes;
                isAmmoTypesValid = true;
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR StickyManager.SetAmmoTypes the newAmmoTypes parameter is invalid"); }
            #endif
        }

        #endregion

        #region Public API Memeber Methods - Beams

        /// <summary>
        /// Returns the prefab for a beam module given its beam prefab ID.
        /// </summary>
        /// <param name="beamPrefabID"></param>
        /// <returns></returns>
        public StickyBeamModule GetBeamPrefab (int beamPrefabID)
        {
            // Check that a valid beam prefab ID was supplied
            if (beamPrefabID < numBeamTemplates)
            {
                return beamTemplatesList[beamPrefabID].s3dBeamModulePrefab;
            }
            else { return null; }
        }

        /// <summary>
        /// Get the beam pool for this prefab or create a new pool if one does not already exist.
        /// Return the beamPrefabID for the pool or StickyManager.NoPrefabID (-1) if it fails.
        /// See also InstantiateBeam(igParms).
        /// </summary>
        /// <param name="stickyBeamModule"></param>
        /// <returns></returns>
        public int GetOrCreateBeamPool (StickyBeamModule stickyBeamPrefab)
        {
            int beamTemplateIndex = NoPrefabID;

            if (stickyBeamPrefab != null)
            {
                // Get the transform instance ID for this beam module prefab
                int beamTransformID = stickyBeamPrefab.transform.GetInstanceID();
                // Search the beam templates list to see if we already have an 
                // beam prefab with the same instance ID
                beamTemplateIndex = beamTemplatesList.FindIndex(e => e.instanceID == beamTransformID);

                if (beamTemplateIndex == NoPrefabID)
                {
                    // If no match was found, create a new beam template for this prefab
                    beamTemplateIndex = AddBeamTemplate(stickyBeamPrefab, beamTransformID);
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: stickyManager.GetorCreateBeamPool - the stickyBeamPrefab is null");
            }
            #endif

            return beamTemplateIndex;
        }

        /// <summary>
        /// Get the beam pool for this prefab or create a new pool if one does not already exist.
        /// Return the beamPrefabID for the pool or StickyManager.NoPrefID (-1) if it fails.
        /// GetorCreateBeamPool (StickyBeamModule beamPrefab) is slightly faster.
        /// See also InstantiateBeam(igParms).
        /// </summary>
        /// <param name="beamPrefab"></param>
        /// <returns></returns>
        public int GetOrCreateBeamPool (GameObject beamPrefab)
        {
            int beamTemplateIndex = NoPrefabID;
            StickyBeamModule stickyBeamModule = null;

            if (beamPrefab == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: stickyManager.GetOrCreateBeamPool - the beamPrefab is null");
                #endif
            }
            else if (beamPrefab.TryGetComponent(out stickyBeamModule))
            {
                // Get the transform instance ID for this beam module prefab
                int beamTransformID = stickyBeamModule.transform.GetInstanceID();
                // Search the beam templates list to see if we already have an 
                // beam prefab with the same instance ID
                beamTemplateIndex = beamTemplatesList.FindIndex(e => e.instanceID == beamTransformID);

                if (beamTemplateIndex == NoPrefabID)
                {
                    // If no match was found, create a new beam template for this prefab
                    beamTemplateIndex = AddBeamTemplate(stickyBeamModule, beamTransformID);
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: stickyManager.GetOrCreateBeamPool - " + beamPrefab.name + " does not include a StickyBeamModule component");
            }
            #endif
            
            return beamTemplateIndex;
        }

        /// <summary>
        /// Instantiates the beam with ID beamPrefabID at the position specified in world space and with the forwards
        /// and up directions specified.
        /// (ibParms.beamPrefabID is the ID sent back to each weapon after calling weapon.ReInitialiseWeapon() method).
        /// </summary>
        /// <param name="ibParms"></param>
        /// <returns></returns>
        public StickyBeamModule InstantiateBeam (ref S3DInstantiateBeamParameters ibParms)
        {
            StickyBeamModule _stickyBeamModule = null;

            if (isInitialised)
            {
                // Check that a valid ID has been provided
                if (ibParms.beamPrefabID > StickyManager.NoPrefabID && ibParms.beamPrefabID < numBeamTemplates)
                {
                    // Get the beam template using its ID (this is simply the index of it in the beam template list)
                    beamTemplate = beamTemplatesList[ibParms.beamPrefabID];

                    // Find the first inactive beam in the pool
                    if (beamTemplate.s3dBeamPool == null)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("StickyManager InstantiateBeam() s3dBeamPool is null. Please Report.");
                        #endif
                    }
                    else
                    {
                        int firstInactiveIndex = beamTemplate.s3dBeamPool.FindIndex(g => !g.activeInHierarchy);

                        if (firstInactiveIndex == -1)
                        {
                            // All beams in the pool are currently active
                            // So if we want to get a new one, we'll need to add a new one to the pool
                            // First check if we have reached the max pool size
                            if (beamTemplate.currentPoolSize < beamTemplate.s3dBeamModulePrefab.maxPoolSize)
                            {
                                // If we are still below the max pool size, add a new beam instance to the pool
                                // Instantiate the beam with the correct position and rotation
                                beamGameObjectInstance = Instantiate(beamTemplate.s3dBeamModulePrefab.gameObject,
                                    ibParms.position, Quaternion.LookRotation(ibParms.fwdDirection, ibParms.upDirection));
                                // Set the beam's parent to be the manager beam transform
                                beamGameObjectInstance.transform.SetParent(beamPooledTrfm);
                                // Initialise the beam
                                if (beamGameObjectInstance.TryGetComponent(out _stickyBeamModule))
                                {
                                    // Pre-assign the StickyManager so we don't need to check it each time we call stickyBeamModule.Initialise()
                                    _stickyBeamModule.stickyManager = this;
                                    ibParms.beamSequenceNumber = _stickyBeamModule.Initialise(ibParms);
                                    ibParms.beamPoolListIndex = beamTemplate.currentPoolSize;
                                    // Add the beam to the list of pooled beams
                                    beamTemplate.s3dBeamPool.Add(beamGameObjectInstance);
                                    // Update the current pool size counter
                                    beamTemplate.currentPoolSize++;
                                }
                            }
                        }
                        else
                        {
                            // Get the beam
                            beamGameObjectInstance = beamTemplate.s3dBeamPool[firstInactiveIndex];
                            // Position the beam
                            beamGameObjectInstance.transform.SetPositionAndRotation(ibParms.position, Quaternion.LookRotation(ibParms.fwdDirection, ibParms.upDirection));
                            // Set the beam to active
                            beamGameObjectInstance.SetActive(true);
                            // Initialise the beam
                            if (beamGameObjectInstance.TryGetComponent(out _stickyBeamModule))
                            {
                                ibParms.beamSequenceNumber = _stickyBeamModule.Initialise(ibParms);
                                ibParms.beamPoolListIndex = firstInactiveIndex;
                            }
                        }
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    // If an invalid ID is provided, log a warning
                    Debug.LogWarning("StickyManager InstantiateBeam() Warning: Provided beamPrefabID was invalid (" + ibParms.beamPrefabID + ").");
                }
                #endif

            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("StickyManager InstantiateBeam() Warning: Method was called before the manager was initialised.");
            }
            #endif

            return _stickyBeamModule;
        }

        /// <summary>
        /// Pause all Pooled Beams in the scene. This is useful when
        /// pausing your game.
        /// </summary>
        public void PauseBeams()
        {
            PauseBeams(true);
        }

        /// <summary>
        /// Resume all Pooled beams in the scene. This is useful when
        /// unpausing your game.
        /// </summary>
        public void ResumeBeams()
        {
            PauseBeams(false);
        }

        #endregion

        #region Public API Member Methods - Decal Modules

        /// <summary>
        /// Create any Decal pools that have not already been created using
        /// a set of StickyDecalModule prefabs.
        /// </summary>
        /// <param name="s3dDecals"></param>
        public void CreateDecalPools (S3DDecals s3dDecals)
        {
            int numDecals = s3dDecals == null || s3dDecals.decalModuleList == null ? 0 : s3dDecals.decalModuleList.Count;

            for (int dcIdx = 0; dcIdx < numDecals; dcIdx++)
            {
                GetOrCreateDecalPool(s3dDecals.decalModuleList[dcIdx]);
            }
        }

        /// <summary>
        /// Create any Decal pools that have not already been created using
        /// a set of StickyDecalModule prefabs.
        /// Populate a pre-created array of S3DDecalTemplate prefabIDs. The
        /// length of decalPrefabIDs must match number of decal slots.
        /// </summary>
        /// <param name="s3dDecals"></param>
        /// <param name="decalPrefabIDs"></param>
        /// <returns>False if mismatch between decals and prefabID array</returns>
        public bool CreateDecalPools (S3DDecals s3dDecals, int[] decalPrefabIDs)
        {
            int numDecals = s3dDecals == null || s3dDecals.decalModuleList == null ? 0 : s3dDecals.decalModuleList.Count;

            int numDecalPrefabIDs = decalPrefabIDs == null ? 0 : decalPrefabIDs.Length;

            if (numDecals != numDecalPrefabIDs) { return false; }
            else
            {
                for (int dcIdx = 0; dcIdx < numDecals; dcIdx++)
                {
                    decalPrefabIDs[dcIdx] = GetOrCreateDecalPool(s3dDecals.decalModuleList[dcIdx]);
                }

                return true;
            }
        }

        /// <summary>
        /// Destroy (return to pool) any decals that are children of the given gameobject.
        /// </summary>
        /// <param name="decalParent"></param>
        public void DestroyDecals (GameObject decalParent)
        {
            if (isInitialised && decalParent != null)
            {
                decalParent.GetComponentsInChildren(true,tempDecalList);

                int numDealModules = tempDecalList.Count;

                for (int dmIdx = 0; dmIdx < numDealModules; dmIdx++)
                {
                    tempDecalList[dmIdx].DestroyDecal();
                }

                tempDecalList.Clear();
            }
        }

        /// <summary>
        /// Returns the prefab for a decal (object) module given its decal prefab ID.
        /// </summary>
        /// <param name="decalPrefabID"></param>
        /// <returns></returns>
        public StickyDecalModule GetDecalPrefab (int decalPrefabID)
        {
            // Check that a valid beam prefab ID was supplied
            if (decalPrefabID < decalTemplatesList.Count)
            {
                return decalTemplatesList[decalPrefabID].s3dDecalModulePrefab;
            }
            else { return null; }
        }

        /// <summary>
        /// Get the decal pool for this prefab or create a new pool if one does not already exist.
        /// Return the decalPrefabID for the pool or StickyManager.NoPrefabID (-1) if it fails.
        /// See also InstantiateDecal(idParms).
        /// </summary>
        /// <param name="stickyDecalModule"></param>
        /// <returns></returns>
        public int GetOrCreateDecalPool (StickyDecalModule decalPrefab)
        {
            int decalTemplateIndex = NoPrefabID;

            if (decalPrefab != null)
            {
                // Get the transform instance ID for this decal module prefab
                int decalTransformID = decalPrefab.transform.GetInstanceID();
                // Search the decal templates list to see if we already have an 
                // decal prefab with the same instance ID
                decalTemplateIndex = decalTemplatesList.FindIndex(e => e.instanceID == decalTransformID);

                if (decalTemplateIndex == NoPrefabID)
                {
                    // If no match was found, create a new decal template for this prefab
                    decalTemplateIndex = AddDecalTemplate(decalPrefab, decalTransformID);
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: stickyManager.GetorCreateDecalPool - the stickyDecalPrefab is null");
            }
            #endif

            return decalTemplateIndex;
        }

        /// <summary>
        /// Get the decal pool for this prefab or create a new pool if one does not already exist.
        /// Return the decalPrefabID for the pool or StickyManager.NoPrefID (-1) if it fails.
        /// GetorCreateDecalPool (StickyDecalModule decalPrefab) is slightly faster.
        /// See also InstantiateDecal(idParms).
        /// </summary>
        /// <param name="decalPrefab"></param>
        /// <returns></returns>
        public int GetOrCreateDecalPool (GameObject decalPrefab)
        {
            int decalTemplateIndex = NoPrefabID;
            StickyDecalModule stickyDecalModule = null;

            if (decalPrefab == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: stickyManager.GetOrCreateDecalPool - the decalPrefab is null");
                #endif
            }
            else if (decalPrefab.TryGetComponent(out stickyDecalModule))
            {
                // Get the transform instance ID for this decal module prefab
                int decalTransformID = stickyDecalModule.transform.GetInstanceID();
                // Search the decal templates list to see if we already have an 
                // decal prefab with the same instance ID
                decalTemplateIndex = decalTemplatesList.FindIndex(e => e.instanceID == decalTransformID);

                if (decalTemplateIndex == NoPrefabID)
                {
                    // If no match was found, create a new decal template for this prefab
                    decalTemplateIndex = AddDecalTemplate(stickyDecalModule, decalTransformID);
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: stickyManager.GetOrCreateDecalPool - " + decalPrefab.name + " does not include a StickyDecalModule component");
            }
            #endif
            
            return decalTemplateIndex;
        }

        /// <summary>
        /// Returns the decalPrefabID for a decal template from a list of default decals associated
        /// with a projectile. If the projectilePrebID is not valid, or there are no matching deal
        /// module pools, the decalPrefabID will return StickyManager.NoPrefabID (-1).
        /// </summary>
        /// <param name="projectilePrefabID">The index in the list of projectile templates</param>
        /// <returns></returns>
        public int GetProjectileDecalPrefabID (int projectilePrefabID)
        {
            if (projectilePrefabID >= 0 && projectilePrefabID < projectileTemplatesList.Count)
            {
                S3DProjectileTemplate projectileTemplate = projectileTemplatesList[projectilePrefabID];

                if (projectileTemplate.numberOfDecals > 0)
                {
                    if (projectileTemplate.numberOfDecals == 1)
                    {
                        return projectileTemplate.decalPrefabIDs[0];
                    }
                    else
                    {
                        return projectileTemplate.decalPrefabIDs[s3dRandomPrefab.Range(0, projectileTemplate.numberOfDecals - 1)];
                    }
                }
                else { return NoPrefabID; }
            }
            else { return NoPrefabID; }
        }

        /// <summary>
        /// Get the random zero-based index from the number of prefabs supplied.
        /// If sticky manager is not initialised or the number is less than 1,
        /// this will return -1.
        /// </summary>
        /// <param name="numberOfPrefabs"></param>
        /// <returns></returns>
        public int GetRandomPrefabIndex (int numberOfPrefabs)
        {
            if (numberOfPrefabs < 1 || !isInitialised) { return -1; }
            else { return numberOfPrefabs == 1 ? 0 : s3dRandomPrefab.Range(0, numberOfPrefabs - 1); }
        }

        /// <summary>
        /// Instantiates the decal with ID decalPrefabID at the position specified in world space
        /// (decalPrefabID is the ID sent back in idParms).
        /// </summary>
        /// <param name="ieParms"></param>
        /// <returns></returns>
        public StickyDecalModule InstantiateDecal (ref S3DInstantiateDecalParameters idParms)
        {
            StickyDecalModule _decalModule = null;

            if (isInitialised)
            {
                // Check that a valid ID has been provided
                if (idParms.decalPrefabID >= 0 && idParms.decalPrefabID < decalTemplatesList.Count)
                {
                    // Get the decal template using its ID (this is just the index of it in the decal template list)
                    decalTemplate = decalTemplatesList[idParms.decalPrefabID];

                    // Currently decals are only instantiated as normal Unity gameobjects

                    // Find the first inactive (pooled) decal
                    if (decalTemplate.s3dDecalPool == null)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("StickyManager InstantiateDecal() decalPool is null. PLEASE REPORT");
                        #endif
                    }
                    else
                    {
                        // Randomise the rotation
                        idParms.rotation *= Quaternion.AngleAxis(s3dRandomPrefab.Range(0f, 360f), Vector3.forward);

                        int firstInactiveIndex = decalTemplate.s3dDecalPool.FindIndex(e => !e.activeInHierarchy);
                        if (firstInactiveIndex == -1)
                        {
                            // All decals in the pool are currently active
                            // So if we want to get a new one, we'll need to add a new one to the pool
                            // First check if we have reached the max pool size
                            if (decalTemplate.currentPoolSize < decalTemplate.s3dDecalModulePrefab.maxPoolSize)
                            {
                                // If we are still below the max pool size, add a new decal instance to the pool
                                // Instantiate the decal with the correct position and rotation
                                decalGameObjectInstance = Instantiate(decalTemplate.s3dDecalModulePrefab.gameObject,
                                    idParms.position, idParms.rotation);
                                // Set the object's parent to be the manager
                                decalGameObjectInstance.transform.SetParent(decalPooledTrfm);
                                // Initialise the decal
                                _decalModule = decalGameObjectInstance.GetComponent<StickyDecalModule>();
                                // Prefetch the mesh verts and disable collider
                                _decalModule.ValidateDecal();
                                idParms.decalSequenceNumber = _decalModule.Initialise(idParms);
                                idParms.decalPoolListIndex = decalTemplate.currentPoolSize;
                                // Add the object to the list of pooled objects
                                decalTemplate.s3dDecalPool.Add(decalGameObjectInstance);
                                // Update the current pool size counter
                                decalTemplate.currentPoolSize++;
                            }
                        }
                        else
                        {
                            // Get the decal
                            decalGameObjectInstance = decalTemplate.s3dDecalPool[firstInactiveIndex];
                            // Position the object
                            decalGameObjectInstance.transform.SetPositionAndRotation(idParms.position, idParms.rotation);
                            // Set the object to active
                            decalGameObjectInstance.SetActive(true);
                            // Initialise the decal
                            _decalModule = decalGameObjectInstance.GetComponent<StickyDecalModule>();
                            idParms.decalSequenceNumber = _decalModule.Initialise(idParms);
                            idParms.decalPoolListIndex = firstInactiveIndex;
                        }

                        #region Check Overlap
                        // If overlap is restricted, check if the decal should be placed here
                        if (idParms.decalSequenceNumber > 0 && _decalModule.overlapAmount < 1f)
                        {
                            bool shouldPlace = true;

                            float overlayFactor = 1f - _decalModule.overlapAmount;

                            /// TODO - Fix this for S3D characters. Currently it only works because we don't check if it hit a trigger collider
                            /// on a non-character or a character.
                            /// For now, just assume there are no trigger colliders close by.
                            for (int vtIdx = 0; vtIdx < _decalModule.numVerts; vtIdx++)
                            {
                                // The z-axis is artifically offset slightly behind the decal in stickyDecalModule.CacheMeshVerts().
                                Vector3 vertPosLS = _decalModule.meshVerts[vtIdx];
                                // Adjust edges inward based on permitted overlap amount. Keep z the same.
                                vertPosLS.x *= overlayFactor;
                                vertPosLS.y *= overlayFactor;

                                Vector3 vertPosWS = idParms.position + (idParms.rotation * vertPosLS);

                                // Raycast from slighty behind the decal (see CacheMeshVerts) to slightly infront of the decal.
                                // Distance = z-axis vert offset (0.01) + small amount (0.005) + the decal normal offset (0.0005)
                                if (!Physics.Raycast(vertPosWS, idParms.fwdDirection, 0.0155f, idParms.collisionMaskLayerInt, QueryTriggerInteraction.Collide))
                                {
                                    shouldPlace = false;
                                    break;
                                }
                            }

                            // If the decal overlaps the object, remove it.
                            if (!shouldPlace)
                            {
                                //Debug.Log("[DEBUG] don't place " + _decalModule.name + " T:" + Time.time);
                                idParms.decalSequenceNumber = 0;
                                idParms.decalPoolListIndex = -1;

                                _decalModule.DestroyDecal();

                                _decalModule = null;
                            }
                        }
                        #endregion
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    // If an invalid ID is provided, log a warning
                    Debug.LogWarning("StickyManager InstantiateDecal() Warning: Provided decalPrefabID was invalid (" + idParms.decalPrefabID + ").");
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("StickyManager InstantiateDecal() Warning: Method was called before the manager was initialised.");
            }
            #endif

            return _decalModule;
        }

        /// <summary>
        /// Pause all Pooled Decals in the scene
        /// </summary>
        public void PauseDecals()
        {
            PauseDecals(true);
        }

        /// <summary>
        /// Resume all Pooled Decals in the scene
        /// </summary>
        public void ResumeDecals()
        {
            PauseDecals(false);
        }

        #endregion

        #region Public API Member Methods - Dynamic (Object) Modules

        /// <summary>
        /// Returns the prefab for a dynamic (object) module given its dynamic prefab ID.
        /// </summary>
        /// <param name="dynamicPrefabID"></param>
        /// <returns></returns>
        public StickyDynamicModule GetDynamicPrefab (int dynamicPrefabID)
        {
            // Check that a valid beam prefab ID was supplied
            if (dynamicPrefabID < dynamicObjectTemplatesList.Count)
            {
                return dynamicObjectTemplatesList[dynamicPrefabID].s3dDynamicModulePrefab;
            }
            else { return null; }
        }

        /// <summary>
        /// Get the dynamic object pool for this prefab or create a new pool if one does not already exist.
        /// Return the dynamicPrefabID for the pool or StickyManager.NoPrefabID (-1) if it fails.
        /// See also InstantiateDynamicObject(idParms).
        /// </summary>
        /// <param name="stickyDynamicModule"></param>
        /// <returns></returns>
        public int GetOrCreateDynamicPool (StickyDynamicModule stickyDynamicPrefab)
        {
            int dynamicTemplateIndex = NoPrefabID;

            if (stickyDynamicPrefab != null)
            {
                // Get the transform instance ID for this dynamic module prefab
                int beamTransformID = stickyDynamicPrefab.transform.GetInstanceID();
                // Search the dynamic object templates list to see if we already have an 
                // dynamic prefab with the same instance ID
                dynamicTemplateIndex = dynamicObjectTemplatesList.FindIndex(e => e.instanceID == beamTransformID);

                if (dynamicTemplateIndex == NoPrefabID)
                {
                    // If no match was found, create a new dynamic template for this prefab
                    dynamicTemplateIndex = AddDynamicObjectTemplate(stickyDynamicPrefab, beamTransformID);
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: stickyManager.GetorCreateDynamicPool - the stickyDynamicPrefab is null");
            }
            #endif

            return dynamicTemplateIndex;
        }

        /// <summary>
        /// Get the dynamic pool for this prefab or create a new pool if one does not already exist.
        /// Return the dynamicPrefabID for the pool or StickyManager.NoPrefID (-1) if it fails.
        /// GetOrCreateDynamicPool (StickyDynamicModule dynamicPrefab) is slightly faster.
        /// See also InstantiateDynamic(igParms).
        /// </summary>
        /// <param name="dynamicPrefab"></param>
        /// <returns></returns>
        public int GetOrCreateDynamicPool (GameObject dynamicPrefab)
        {
            int dynamicTemplateIndex = NoPrefabID;
            StickyDynamicModule stickyDynamicModule = null;

            if (dynamicPrefab == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: stickyManager.GetOrCreateDynamicPool - the dynamicPrefab is null");
                #endif
            }
            else if (dynamicPrefab.TryGetComponent(out stickyDynamicModule))
            {
                // Get the transform instance ID for this dynamic module prefab
                int dynamicTransformID = stickyDynamicModule.transform.GetInstanceID();
                // Search the dynamic templates list to see if we already have an 
                // dynamic prefab with the same instance ID
                dynamicTemplateIndex = dynamicObjectTemplatesList.FindIndex(e => e.instanceID == dynamicTransformID);

                if (dynamicTemplateIndex == NoPrefabID)
                {
                    // If no match was found, create a new dynamic template for this prefab
                    dynamicTemplateIndex = AddDynamicObjectTemplate(stickyDynamicModule, dynamicTransformID);
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: stickyManager.GetOrCreateDynamicPool - " + dynamicPrefab.name + " does not include a StickyDynamicModule component");
            }
            #endif
            
            return dynamicTemplateIndex;
        }

        /// <summary>
        /// Instantiate a dynamic object in the scene using an instance from the pool if possible.
        /// </summary>
        /// <param name="idParms"></param>
        /// <returns></returns>
        public StickyDynamicModule InstantiateDynamicObject (ref S3DInstantiateDynamicObjectParameters idParms)
        {
            StickyDynamicModule _dynamicModule = null;

            if (isInitialised)
            {
                // Check that a valid ID has been provided
                if (idParms.dynamicObjectPrefabID >= 0 && idParms.dynamicObjectPrefabID < dynamicObjectTemplatesList.Count)
                {
                    // Get the dynamic object template using its ID (this is just the index of it in the dynamic object template list)
                    dynamicObjectTemplate = dynamicObjectTemplatesList[idParms.dynamicObjectPrefabID];

                    // Currently dynamic objects are only instantiated as normal Unity gameobjects

                    // Find the first inactive (pooled) dynamic object
                    if (dynamicObjectTemplate.s3dDynamicObjectPool == null)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("StickyManager InstantiateDynamicObject() dynamicObjectPool is null. PLEASE REPORT");
                        #endif
                    }
                    else
                    {
                        int firstInactiveIndex = dynamicObjectTemplate.s3dDynamicObjectPool.FindIndex(e => !e.activeInHierarchy);
                        if (firstInactiveIndex == -1)
                        {
                            // All dynamic objects in the pool are currently active
                            // So if we want to get a new one, we'll need to add a new one to the pool
                            // First check if we have reached the max pool size
                            if (dynamicObjectTemplate.currentPoolSize < dynamicObjectTemplate.s3dDynamicModulePrefab.maxPoolSize)
                            {
                                // If we are still below the max pool size, add a new dynamic object instance to the pool
                                // Instantiate the dynamic object with the correct position and rotation
                                dynamicObjectGameObjectInstance = Instantiate(dynamicObjectTemplate.s3dDynamicModulePrefab.gameObject,
                                    idParms.position, idParms.rotation);
                                // Set the object's parent to be the manager
                                dynamicObjectGameObjectInstance.transform.SetParent(dynamicPooledTrfm);
                                // Initialise the dynamic object
                                _dynamicModule = dynamicObjectGameObjectInstance.GetComponent<StickyDynamicModule>();

                                if (_dynamicModule.InitialiseDynamicModule())
                                {
                                    idParms.dynamicObjectSequenceNumber = _dynamicModule.ActivateDyanmicObject(ref idParms);
                                    idParms.dynamicObjectPoolListIndex = dynamicObjectTemplate.currentPoolSize;
                                    // Add the object to the list of pooled objects
                                    dynamicObjectTemplate.s3dDynamicObjectPool.Add(dynamicObjectGameObjectInstance);
                                    // Update the current pool size counter
                                    dynamicObjectTemplate.currentPoolSize++;
                                }
                            }
                        }
                        else
                        {
                            // Get the dynamic object
                            dynamicObjectGameObjectInstance = dynamicObjectTemplate.s3dDynamicObjectPool[firstInactiveIndex];
                            // Position the object
                            dynamicObjectGameObjectInstance.transform.SetPositionAndRotation(idParms.position, idParms.rotation);
                            // Set the object to active
                            dynamicObjectGameObjectInstance.SetActive(true);
                            // Initialise the dynamic object
                            _dynamicModule = dynamicObjectGameObjectInstance.GetComponent<StickyDynamicModule>();
                            if (_dynamicModule.InitialiseDynamicModule())
                            {
                                idParms.dynamicObjectSequenceNumber = _dynamicModule.ActivateDyanmicObject(ref idParms);
                                idParms.dynamicObjectPoolListIndex = firstInactiveIndex;
                            }
                        }
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    // If an invalid ID is provided, log a warning
                    Debug.LogWarning("StickyManager InstantiateDynamicObject() Warning: Provided dynamicObjectPrefabID was invalid (" + idParms.dynamicObjectPrefabID + ").");
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("StickyManager InstantiateDynamicObject() Warning: Method was called before the manager was initialised.");
            }
            #endif

            return _dynamicModule;
        }

        /// <summary>
        /// Pause all Pooled Dynamic Objects in the scene
        /// </summary>
        public void PauseDynamicObjects()
        {
            PauseDynamicObjects(true);
        }

        /// <summary>
        /// Resume all Pooled Dynamic Objects in the scene
        /// </summary>
        public void ResumeDynamicObjects()
        {
            PauseDynamicObjects(false);
        }

        #endregion

        #region Public API Member Methods - Effects (Object) Modules

        /// <summary>
        /// Create any Effects pools that have not already been created using
        /// a set of StickyEffectsModule prefabs.
        /// Populate a pre-created array of S3DEffectsTemplate prefabIDs. The
        /// length of effectsPrefabIDs must match number of effects slots.
        /// </summary>
        public bool CreateEffectsPools (S3DEffectsSet s3dEffectsSet, int[] effectsPrefabIDs)
        {
            int numEffects = s3dEffectsSet == null || s3dEffectsSet.effectsModuleList == null ? 0 : s3dEffectsSet.effectsModuleList.Count;

            int numEffectsPrefabIDs = effectsPrefabIDs == null ? 0 : effectsPrefabIDs.Length;

            if (numEffects != numEffectsPrefabIDs) { return false; }
            else
            {
                for (int dcIdx = 0; dcIdx < numEffects; dcIdx++)
                {
                    effectsPrefabIDs[dcIdx] = GetOrCreateEffectsPool(s3dEffectsSet.effectsModuleList[dcIdx]);

                    #if UNITY_EDITOR
                    if (effectsPrefabIDs[dcIdx] == NoPrefabID)
                    {
                        Debug.LogWarning("ERROR " + s3dEffectsSet.name + ", item " + (dcIdx+1).ToString("00") + ", does not contain a valid StickyEffectsModule prefab.");
                    }
                    #endif
                }

                return true;
            }
        }

        /// <summary>
        /// Returns the prefab for an effects (object) module given its effects prefab ID.
        /// </summary>
        /// <param name="effectsPrefabID"></param>
        /// <returns></returns>
        public StickyEffectsModule GetEffectsPrefab (int effectsPrefabID)
        {
            // Check that a valid effects prefab ID was supplied
            if (effectsPrefabID < effectsObjectTemplatesList.Count)
            {
                return effectsObjectTemplatesList[effectsPrefabID].s3dEffectsModulePrefab;
            }
            else { return null; }
        }

        /// <summary>
        /// Get the effects object pool for this prefab or create a new pool if one does not already exist.
        /// Return the effectsPrefabID for the pool or StickyManager.NoPrefabID (-1) if it fails.
        /// See also InstantiateEffectsObject(ieParms).
        /// </summary>
        /// <param name="stickyEffectsModule"></param>
        /// <returns></returns>
        public int GetOrCreateEffectsPool (StickyEffectsModule effectsPrefab)
        {
            int effectsTemplateIndex = NoPrefabID;

            if (effectsPrefab != null)
            {
                // Get the transform instance ID for this effects module prefab
                int effectsTransformID = effectsPrefab.transform.GetInstanceID();
                // Search the effects object templates list to see if we already have an 
                // effects prefab with the same instance ID
                effectsTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsTransformID);

                if (effectsTemplateIndex == NoPrefabID)
                {
                    // If no match was found, create a new effects template for this prefab
                    effectsTemplateIndex = AddEffectsObjectTemplate(effectsPrefab, effectsTransformID);
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: stickyManager.GetorCreateEffectsPool - the StickyEffectsModule Prefab is null");
            }
            #endif

            return effectsTemplateIndex;
        }

        /// <summary>
        /// Get the effects pool for this prefab or create a new pool if one does not already exist.
        /// Return the effectsPrefabID for the pool or StickyManager.NoPrefID (-1) if it fails.
        /// GetorCreateEffectsPool (StickyEffectsModule effectsPrefab) is slightly faster.
        /// See also InstantiateEffects(igParms).
        /// </summary>
        /// <param name="effectsPrefab"></param>
        /// <returns></returns>
        public int GetOrCreateEffectsPool (GameObject effectsPrefab)
        {
            int effectsTemplateIndex = NoPrefabID;
            StickyEffectsModule stickyEffectsModule = null;

            if (effectsPrefab == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: stickyManager.GetOrCreateEffectsPool - the effectsPrefab is null");
                #endif
            }
            else if (effectsPrefab.TryGetComponent(out stickyEffectsModule))
            {
                // Get the transform instance ID for this effects module prefab
                int effectsTransformID = stickyEffectsModule.transform.GetInstanceID();
                // Search the effects templates list to see if we already have an 
                // effects prefab with the same instance ID
                effectsTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsTransformID);

                if (effectsTemplateIndex == NoPrefabID)
                {
                    // If no match was found, create a new effects template for this prefab
                    effectsTemplateIndex = AddEffectsObjectTemplate(stickyEffectsModule, effectsTransformID);
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: stickyManager.GetOrCreateEffectsPool - " + effectsPrefab.name + " does not include a StickyEffectsModule component");
            }
            #endif
            
            return effectsTemplateIndex;
        }

        /// <summary>
        /// Get the Sound FX object pool for this prefab or create a new pool if one does not already exist.
        /// Return the effectsPrefabID for the pool or StickyManager.NoPrefabID (-1) if it fails.
        /// See also InstantiateSoundFX(sfxParms).
        /// </summary>
        /// <param name="stickyEffectsModule"></param>
        /// <returns></returns>
        public int GetOrCreateSoundFXPool (StickyEffectsModule effectsPrefab)
        {
            int effectsTemplateIndex = NoPrefabID;

            if (effectsPrefab != null)
            {
                // Verify that it is a SoundFX type of Effects Module
                if (effectsPrefab.ModuleEffectsType == StickyEffectsModule.EffectsType.SoundFX)
                {
                    // Get the transform instance ID for this effects module prefab
                    int effectsTransformID = effectsPrefab.transform.GetInstanceID();
                    // Search the effects object templates list to see if we already have an 
                    // effects prefab with the same instance ID
                    effectsTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsTransformID);

                    if (effectsTemplateIndex == NoPrefabID)
                    {
                        // If no match was found, create a new effects template for this prefab
                        effectsTemplateIndex = AddEffectsObjectTemplate(effectsPrefab, effectsTransformID);
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("ERROR: stickyManager.GetOrCreateSoundFXPool - the stickyEffectsPrefab must have Effects Type of Sound FX");
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: stickyManager.GetOrCreateSoundFXPool - the stickyEffectsPrefab is null");
            }
            #endif

            return effectsTemplateIndex;
        }

        /// <summary>
        /// Get the Sound FX pool for this prefab or create a new pool if one does not already exist.
        /// Return the effectsPrefabID for the pool or StickyManager.NoPrefID (-1) if it fails.
        /// GetorCreateSoundFXPool (StickyEffectsModule effectsPrefab) is slightly faster.
        /// See also InstantiateSoundFX(sfxParms).
        /// </summary>
        /// <param name="effectsPrefab"></param>
        /// <returns></returns>
        public int GetOrCreateSoundFXPool (GameObject effectsPrefab)
        {
            int effectsTemplateIndex = NoPrefabID;
            StickyEffectsModule stickyEffectsModule = null;

            if (effectsPrefab == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: stickyManager.GetOrCreateSoundFXPool - the effectsPrefab is null");
                #endif
            }
            else if (effectsPrefab.TryGetComponent(out stickyEffectsModule))
            {
                // Verify that it is a SoundFX type of Effects Module
                if (stickyEffectsModule.ModuleEffectsType == StickyEffectsModule.EffectsType.SoundFX)
                {
                    // Get the transform instance ID for this effects module prefab
                    int effectsTransformID = stickyEffectsModule.transform.GetInstanceID();
                    // Search the effects templates list to see if we already have an 
                    // effects prefab with the same instance ID
                    effectsTemplateIndex = effectsObjectTemplatesList.FindIndex(e => e.instanceID == effectsTransformID);

                    if (effectsTemplateIndex == NoPrefabID)
                    {
                        // If no match was found, create a new effects template for this prefab
                        effectsTemplateIndex = AddEffectsObjectTemplate(stickyEffectsModule, effectsTransformID);
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("ERROR: stickyManager.GetOrCreateSoundFXPool - the stickyEffectsPrefab must have Effects Type of Sound FX");
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: stickyManager.GetOrCreateSoundFXPool - " + effectsPrefab.name + " does not include a StickyEffectsModule component");
            }
            #endif
            
            return effectsTemplateIndex;
        }

        /// <summary>
        /// Instantiates the effects object with ID effectsObjectPrefabID at the position specified in world space
        /// (effectsObjectPrefabID is the ID sent back in ieParms).
        /// </summary>
        /// <param name="ieParms"></param>
        /// <returns></returns>
        public StickyEffectsModule InstantiateEffectsObject (ref S3DInstantiateEffectsObjectParameters ieParms)
        {
            StickyEffectsModule _effectsModule = null;

            if (isInitialised)
            {
                // Check that a valid ID has been provided
                if (ieParms.effectsObjectPrefabID >= 0 && ieParms.effectsObjectPrefabID < effectsObjectTemplatesList.Count)
                {
                    // Get the effects object template using its ID (this is just the index of it in the effects object template list)
                    effectsObjectTemplate = effectsObjectTemplatesList[ieParms.effectsObjectPrefabID];

                    // Currently effects objects are only instantiated as normal Unity gameobjects

                    // Find the first inactive (pooled) effects object
                    if (effectsObjectTemplate.s3dEffectsObjectPool == null)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("StickyManager InstantiateEffectsObject() effectsObjectPool is null. PLEASE REPORT");
                        #endif
                    }
                    else
                    {
                        int firstInactiveIndex = effectsObjectTemplate.s3dEffectsObjectPool.FindIndex(e => !e.activeInHierarchy);
                        if (firstInactiveIndex == -1)
                        {
                            // All effects objects in the pool are currently active
                            // So if we want to get a new one, we'll need to add a new one to the pool
                            // First check if we have reached the max pool size
                            if (effectsObjectTemplate.currentPoolSize < effectsObjectTemplate.s3dEffectsModulePrefab.maxPoolSize)
                            {
                                // If we are still below the max pool size, add a new effects object instance to the pool
                                // Instantiate the effects object with the correct position and rotation
                                effectsObjectGameObjectInstance = Instantiate(effectsObjectTemplate.s3dEffectsModulePrefab.gameObject,
                                    ieParms.position, ieParms.rotation);
                                // Set the object's parent to be the manager
                                effectsObjectGameObjectInstance.transform.SetParent(effectsPooledTrfm);
                                // Initialise the effects object
                                _effectsModule = effectsObjectGameObjectInstance.GetComponent<StickyEffectsModule>();
                                ieParms.effectsObjectSequenceNumber = _effectsModule.Initialise(ieParms);
                                ieParms.effectsObjectPoolListIndex = effectsObjectTemplate.currentPoolSize;
                                // Add the object to the list of pooled objects
                                effectsObjectTemplate.s3dEffectsObjectPool.Add(effectsObjectGameObjectInstance);
                                // Update the current pool size counter
                                effectsObjectTemplate.currentPoolSize++;
                            }
                        }
                        else
                        {
                            // Get the effects object
                            effectsObjectGameObjectInstance = effectsObjectTemplate.s3dEffectsObjectPool[firstInactiveIndex];
                            // Position the object
                            effectsObjectGameObjectInstance.transform.SetPositionAndRotation(ieParms.position, ieParms.rotation);
                            // Set the object to active
                            effectsObjectGameObjectInstance.SetActive(true);
                            // Initialise the effects object
                            _effectsModule = effectsObjectGameObjectInstance.GetComponent<StickyEffectsModule>();
                            ieParms.effectsObjectSequenceNumber = _effectsModule.Initialise(ieParms);
                            ieParms.effectsObjectPoolListIndex = firstInactiveIndex;
                        }
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    // If an invalid ID is provided, log a warning
                    Debug.LogWarning("StickyManager InstantiateEffectsObject() Warning: Provided effectsObjectPrefabID was invalid (" + ieParms.effectsObjectPrefabID + ").");
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("StickyManager InstantiateEffectsObject() Warning: Method was called before the manager was initialised.");
            }
            #endif

            return _effectsModule;
        }

        /// <summary>
        /// Instantiate a pooled StickyEffectsModule which contains an audio source. If audioClip is null,
        /// the existing one (if there is one) attached to the prefab will be used.
        /// See also GetorCreateEffectsPool(..).
        /// </summary>
        /// <param name="sfxParms"></param>
        /// <param name="audioClip"></param>
        public StickyEffectsModule InstantiateSoundFX (ref S3DInstantiateSoundFXParameters sfxParms, AudioClip audioClip)
        {
            StickyEffectsModule _effectsModule = null;

            if (isInitialised)
            {
                if (sfxParms.effectsObjectPrefabID >= 0 && sfxParms.effectsObjectPrefabID < effectsObjectTemplatesList.Count)
                {
                    // Get the effects object template using its ID (this is just the index of it in the effects object template list)
                    effectsObjectTemplate = effectsObjectTemplatesList[sfxParms.effectsObjectPrefabID];

                    // Find the first inactive effects object
                    if (effectsObjectTemplate.s3dEffectsObjectPool == null)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("StickyManager InstantiateSoundFX() effectsObjectPool is null. PLEASE REPORT");
                        #endif
                    }
                    else
                    {
                        int firstInactiveIndex = effectsObjectTemplate.s3dEffectsObjectPool.FindIndex(e => !e.activeInHierarchy);
                        if (firstInactiveIndex == -1)
                        {
                            // All effects objects in the pool are currently active
                            // So if we want to get a new one, we'll need to add a new one to the pool
                            // First check if we have reached the max pool size
                            if (effectsObjectTemplate.currentPoolSize < effectsObjectTemplate.s3dEffectsModulePrefab.maxPoolSize)
                            {
                                // If we are still below the max pool size, add a new effects object instance to the pool
                                // Instantiate the effects object with the correct position and rotation
                                effectsObjectGameObjectInstance = Instantiate(effectsObjectTemplate.s3dEffectsModulePrefab.gameObject,
                                    sfxParms.position, Quaternion.identity);
                                // Set the object's parent to be the manager
                                effectsObjectGameObjectInstance.transform.SetParent(effectsPooledTrfm);
                                firstInactiveIndex = effectsObjectTemplate.currentPoolSize;
                                // Add the object to the list of pooled objects
                                effectsObjectTemplate.s3dEffectsObjectPool.Add(effectsObjectGameObjectInstance);
                                // Update the current pool size counter
                                effectsObjectTemplate.currentPoolSize++;                                     
                            }
                        }
                        else
                        {
                            // Get the effects object
                            effectsObjectGameObjectInstance = effectsObjectTemplate.s3dEffectsObjectPool[firstInactiveIndex];
                            // Position the object
                            effectsObjectGameObjectInstance.transform.SetPositionAndRotation(sfxParms.position, Quaternion.identity);
                            // Set the object to active
                            effectsObjectGameObjectInstance.SetActive(true);
                        }

                        if (firstInactiveIndex >= 0)
                        {
                            // Initialise the effects object
                            _effectsModule = effectsObjectGameObjectInstance.GetComponent<StickyEffectsModule>();

                            AudioSource audioSource = _effectsModule.GetAudioSource();

                            if (audioSource != null)
                            {
                                // Replace the clip?
                                if (audioClip != null)
                                {
                                    _effectsModule.SetAudioClip(audioClip);
                                }

                                // Override the prefab volume?
                                if (sfxParms.volume > 0f && sfxParms.volume <= 1f) { audioSource.volume = sfxParms.volume; }

                                sfxParms.effectsObjectSequenceNumber = _effectsModule.Initialise(sfxParms);
                                sfxParms.effectsObjectPoolListIndex = firstInactiveIndex;
                            }
                        }
                    }
                }
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("StickyManager InstantiateSoundFX() Warning: Method was called before the manager was initialised.");
            }
            #endif

            return _effectsModule;
        }

        /// <summary>
        /// Pause all Pooled Effects Objects in the scene
        /// </summary>
        public void PauseEffectsObjects()
        {
            PauseEffectsObjects(true);
        }

        /// <summary>
        /// Resume all Pooled Effects Objects in the scene
        /// </summary>
        public void ResumeEffectsObjects()
        {
            PauseEffectsObjects(false);
        }

        #endregion

        #region Public API Member Methods - Generic Objects

        /// <summary>
        /// Returns the prefab for a generic module given its generic prefab ID.
        /// </summary>
        /// <param name="genericPrefabID"></param>
        /// <returns></returns>
        public StickyGenericModule GetGenericPrefab (int genericPrefabID)
        {
            // Check that a valid beam prefab ID was supplied
            if (genericPrefabID < genericObjectTemplatesList.Count)
            {
                return genericObjectTemplatesList[genericPrefabID].s3dGenericModulePrefab;
            }
            else { return null; }
        }

        /// <summary>
        /// Get the generic pool for this prefab or create a new pool if one does not already exist.
        /// Return the genericObjectPrefabID for the pool or StickyManager.NoPrefabID (-1) if it fails.
        /// See also InstantiateGenericObject(igParms).
        /// </summary>
        /// <param name="genericObjectPrefab"></param>
        /// <returns></returns>
        public int GetOrCreateGenericPool (StickyGenericModule genericObjectPrefab)
        {
            int genericObjectTemplateIndex = NoPrefabID;

            if (genericObjectPrefab != null)
            {
                // Get the transform instance ID for this generic module prefab
                int genericObjectTransformID = genericObjectPrefab.transform.GetInstanceID();
                // Search the generic object templates list to see if we already have an 
                // generic object prefab with the same instance ID
                genericObjectTemplateIndex = genericObjectTemplatesList.FindIndex(e => e.instanceID == genericObjectTransformID);

                if (genericObjectTemplateIndex == NoPrefabID)
                {
                    // If no match was found, create a new generic object template for this prefab
                    genericObjectTemplateIndex = AddGenericObjectTemplate(genericObjectPrefab, genericObjectTransformID);
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: stickyManager.GetorCreateGenericPool - the genericObjectPrefab is null");
            }
            #endif

            return genericObjectTemplateIndex;
        }

        /// <summary>
        /// Get the generic pool for this prefab or create a new pool if one does not already exist.
        /// Return the genericObjectPrefabID for the pool or StickyManager.NoPrefID (-1) if it fails.
        /// GetorCreateGenericPool (StickyGenericModule genericObjectPrefab) is slightly faster.
        /// See also InstantiateGenericObject(igParms).
        /// </summary>
        /// <param name="genericObjectPrefab"></param>
        /// <returns></returns>
        public int GetOrCreateGenericPool (GameObject genericObjectPrefab)
        {
            int genericObjectTemplateIndex = NoPrefabID;
            StickyGenericModule genericModule = null;

            if (genericObjectPrefab == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: stickyManager.GetorCreateGenericPool - the genericObjectPrefab is null");
                #endif
            }
            else if (genericObjectPrefab.TryGetComponent(out genericModule))
            {
                // Get the transform instance ID for this generic module prefab
                int genericObjectTransformID = genericModule.transform.GetInstanceID();
                // Search the generic object templates list to see if we already have an 
                // generic object prefab with the same instance ID
                genericObjectTemplateIndex = genericObjectTemplatesList.FindIndex(e => e.instanceID == genericObjectTransformID);

                if (genericObjectTemplateIndex == NoPrefabID)
                {
                    // If no match was found, create a new generic object template for this prefab
                    genericObjectTemplateIndex = AddGenericObjectTemplate(genericModule, genericObjectTransformID);
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: stickyManager.GetorCreateGenericPool - " + genericObjectPrefab.name + " does not include a StickyGenericModule component");
            }
            #endif
            
            return genericObjectTemplateIndex;
        }

        /// <summary>
        /// Instantiates the generic object with ID genericModulePrefabID at the position specified in world space
        /// </summary>
        /// <param name="igParms"></param>
        /// <returns></returns>
        public StickyGenericModule InstantiateGenericObject (ref S3DInstantiateGenericObjectParameters igParms)
        {
            StickyGenericModule _stickyGenericModule = null;

            if (isInitialised)
            {
                // Check that a valid ID has been provided
                if (igParms.genericModulePrefabID > StickyManager.NoPrefabID && igParms.genericModulePrefabID < genericObjectTemplatesList.Count)
                {
                    // Get the generic object template using its ID (this is just the index of it in the generic object template list)
                    genericObjectTemplate = genericObjectTemplatesList[igParms.genericModulePrefabID];

                    // Find the first inactive generic object in the pool
                    if (genericObjectTemplate.s3dGenericObjectPool == null)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("StickyManager InstantiateGenericObject() s3dGenericObjectPool is null. PLEASE REPORT");
                        #endif
                    }
                    else
                    {
                        int firstInactiveIndex = genericObjectTemplate.s3dGenericObjectPool.FindIndex(g => !g.activeInHierarchy);

                        if (firstInactiveIndex == -1)
                        {
                            // All generic objects in the pool are currently active
                            // So if we want to get a new one, we'll need to add a new one to the pool
                            // First check if we have reached the max pool size
                            if (genericObjectTemplate.currentPoolSize < genericObjectTemplate.s3dGenericModulePrefab.maxPoolSize)
                            {
                                // If we are still below the max pool size, add a new generic object instance to the pool
                                // Instantiate the generic object with the correct position and rotation
                                genericObjectGameObjectInstance = Instantiate(genericObjectTemplate.s3dGenericModulePrefab.gameObject,
                                    igParms.position, igParms.rotation);
                                // Set the object's parent to be the manager generic pool transform
                                genericObjectGameObjectInstance.transform.SetParent(genericPooledTrfm);
                                // Initialise the generic object
                                if (genericObjectGameObjectInstance.TryGetComponent(out _stickyGenericModule))
                                {
                                    igParms.genericObjectSequenceNumber = _stickyGenericModule.Initialise(igParms);
                                    igParms.genericObjectPoolListIndex = genericObjectTemplate.currentPoolSize;
                                    // Add the object to the list of pooled objects
                                    genericObjectTemplate.s3dGenericObjectPool.Add(genericObjectGameObjectInstance);
                                    // Update the current pool size counter
                                    genericObjectTemplate.currentPoolSize++;
                                }
                            }
                        }
                        else
                        {
                            // Get the generic object
                            genericObjectGameObjectInstance = genericObjectTemplate.s3dGenericObjectPool[firstInactiveIndex];
                            // Position the object
                            genericObjectGameObjectInstance.transform.SetPositionAndRotation(igParms.position, igParms.rotation);
                            // Set the object to active
                            genericObjectGameObjectInstance.SetActive(true);
                            // Initialise the generic object
                            if (genericObjectGameObjectInstance.TryGetComponent(out _stickyGenericModule))
                            {
                                igParms.genericObjectSequenceNumber = _stickyGenericModule.Initialise(igParms);
                                igParms.genericObjectPoolListIndex = firstInactiveIndex;
                            }
                        }
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    // If an invalid ID is provided, log a warning
                    Debug.LogWarning("StickyManager InstantiateGenericObject() Warning: Provided genericModulePrefabID was invalid (" +
                          igParms.genericModulePrefabID + ").");
                }
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("StickyManager InstantiateGenericObject() Warning: Method was called before the manager was initialised.");
            }
            #endif

            return _stickyGenericModule;
        }

        /// <summary>
        /// Pause all pooled Generic Objects in the scene. This is useful when
        /// pausing your game.
        /// </summary>
        public void PauseGenericObjects()
        {
            PauseGenericObjects(true);
        }

        /// <summary>
        /// Resume all pooled Generic Objects in the scene. This is useful when
        /// unpausing your game.
        /// </summary>
        public void ResumeGenericObjects()
        {
            PauseGenericObjects(false);
        }

        #endregion

        #region Public API Member Methods - Object Detection

        /// <summary>
        /// Has a Sticky3D character been hit by a projectile?
        /// Applies damage and impact force if hit.
        /// Invokes callbackOnHit if configured on hit character.
        /// </summary>
        /// <param name="fromPosition"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <param name="collisionLayerMask"></param>
        /// <param name="projectile"></param>
        /// <param name="skipFastCheck"></param>
        /// <returns></returns>
        public bool CheckCharacterHitByProjectile
        (
            Vector3 fromPosition, Vector3 direction, float distance,
            LayerMask collisionLayerMask, out RaycastHit hitInfo,
            StickyProjectileModule projectile,
            bool skipFastCheck
        )
        {
            bool isHit = false;

            hitInfo = new RaycastHit();

            // Do a quick test to see if there is anything between the two world space positions
            if (skipFastCheck || Physics.Raycast(fromPosition, direction, out hitInfo, distance, collisionLayerMask, QueryTriggerInteraction.Collide))
            {
                // Get all hits between fromPosition position and the distance away in the direction given. Include triggers.
                // NOTE: RaycastNonAlloc, like other 3D multi-hit physics item, returns an UNSORTED array.
                Ray ray = new Ray(fromPosition, direction);

                // There is no direct access to the underlying Raycast(from, direction, results, distance, laymask, queryTriggerInteraction).
                int numHits = Physics.RaycastNonAlloc(ray, raycastHitInfoArray, distance, collisionLayerMask, QueryTriggerInteraction.Collide);

                if (numHits > 1) { S3DUtils.SortHitsAsc(raycastHitInfoArray, numHits); }

                StickyControlModule stickyControlModule;

                for (int hIdx = 0; hIdx < numHits; hIdx++)
                {
                    hitInfo = raycastHitInfoArray[hIdx];

                    Rigidbody hitRigidbody = hitInfo.rigidbody;

                    if (hitRigidbody != null)
                    {
                        Collider hitCollider = hitInfo.collider;
                        S3DDamageRegionReceiver damageRegionReceiver;
                        int hitDamageRegionId = 0;

                        // Is it a Sticky3D character?
                        bool isCharacterHit = hitRigidbody.TryGetComponent(out stickyControlModule);

                        // If the character has multiple damage regions, and we just hit the main character
                        // capsule collider, ignore it.
                        if (isCharacterHit)
                        {
                            if (stickyControlModule.NumberOfDamageRegions > 1)
                            {
                                // Skip the character's main capsule collider if there are additional damage regions
                                if (hitCollider.GetInstanceID() == stickyControlModule.StickyID) { continue; }
                                else if (hitCollider.TryGetComponent(out damageRegionReceiver) && damageRegionReceiver.isValid)
                                {
                                    stickyControlModule = damageRegionReceiver.stickyControlModule;
                                    hitDamageRegionId = damageRegionReceiver.damageRegionId;
                                }
                                else { isCharacterHit = false; }
                            }
                            else
                            {
                                hitDamageRegionId = stickyControlModule.GetMainDamageRegionID();
                            }
                        }
                        // I don't think this is necessary as the hit seems to always occur on the main character rigidbody
                        // even when there are ragdoll rigidbodies attached to bone colliders.
                        /// TODO - This might be too expensive for beams which could be continously hitting a character every frame
                        else if (hitCollider.TryGetComponent(out damageRegionReceiver) && damageRegionReceiver.isValid)
                        {
                            stickyControlModule = damageRegionReceiver.stickyControlModule;
                            hitDamageRegionId = damageRegionReceiver.damageRegionId;
                            isCharacterHit = true;
                        }

                        // Did we hit a character?
                        if (isCharacterHit)
                        {
                            // Did we just hit the character that fired the projectile? If so, ignore this hit.
                            if (stickyControlModule.StickyID == projectile.sourceStickyId) { continue; }
                            // Check that we didn't hit a collider on a character that should be ignored
                            else if (!stickyControlModule.IsColliderHittableByWeapon(hitInfo.collider)) { continue; }

                            float damageValue = projectile.damageAmount * GetAmmoDamageMultiplier(projectile.ammoTypeInt);
                            float impactForceValue = projectile.impactForce * GetAmmoImpactMultiplier(projectile.ammoTypeInt);

                            // Apply damage to the Sticky character
                            stickyControlModule.ApplyNormalDamage(hitDamageRegionId, damageValue, projectile.damageType, hitInfo.point);

                            // Apply impact force to the Sticky character
                            stickyControlModule.AddForceAtPosition(direction, impactForceValue, 1f, hitInfo.point);

                            // If required, call the custom method
                            if (stickyControlModule.callbackOnHit != null)
                            {
                                // Create a struct with the necessary parameters
                                S3DCharacterHitParameters s3dCharacterHitParameters = new S3DCharacterHitParameters
                                {
                                    hitInfo = hitInfo,
                                    stickyControlModule = stickyControlModule,
                                    projectilePrefab = GetProjectilePrefab(projectile.projectilePrefabID),
                                    beamPrefab = null,
                                    damageAmount = damageValue,
                                    damageTypeInt = (int)projectile.damageType,
                                    impactForce = impactForceValue,
                                    ammoTypeInt = projectile.ammoTypeInt,
                                    sourceStickyId = projectile.sourceStickyId,
                                    sourceFactionId = projectile.sourceFactionId,
                                    sourceModelId = projectile.sourceModelId
                                };
                                // Call the custom callback
                                stickyControlModule.callbackOnHit(s3dCharacterHitParameters);
                            }

                            isHit = true;
                        }

                        // We hit an object with a rigidbody so exit
                        break;
                    }
                    else if (!hitInfo.collider.isTrigger)
                    {
                        // We hit a collider, but it wasn't a character and it wasn't a trigger collider so exit.
                        break;
                    }
                } 
            }

            return isHit;
        }

        /// <summary>
        /// Has a beam hit?
        /// 1) A Sticky3D character?
        /// 2) A rigidbody with StickyDamageReceiver attached?
        /// 3) A non-trigger collider with a StickyDamageReceiver attached?
        /// 4) A regular non-trigger collider
        /// If hit:
        /// 1) Applies damage and invokes callbackOnHit on hit character (if any)
        /// 2) Applies damage to StickyDamageReceiver and invokes callbackOnHit on hit receiver (if any)
        /// </summary>
        /// <param name="fromPosition"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <param name="collisionLayerMask"></param>
        /// <param name="hitInfo"></param>
        /// <param name="beamModule"></param>
        /// <param name="hitDuration"></param>
        /// <param name="skipFastCheck"></param>
        /// <returns></returns>
        public bool CheckObjectHitByBeam
        (
            Vector3 fromPosition, Vector3 direction, float distance,
            LayerMask collisionLayerMask, out RaycastHit hitInfo,
            StickyBeamModule beamModule, float hitDuration,
            bool skipFastCheck
        )
        {
            bool isHit = false;

            hitInfo = new RaycastHit();

            // Do a quick test to see if there is anything between the start and end of the beam
            if (skipFastCheck || Physics.Raycast(fromPosition, direction, out hitInfo, distance, collisionLayerMask, QueryTriggerInteraction.Collide))
            {
                // Was the first thing we hit a non-trigger collider?
                isHit = !skipFastCheck && !hitInfo.collider.isTrigger;

                int numHits = isHit ? 1 : 0;

                // If this was a regular non-trigger collider, no need to do any more raycasts
                if (isHit)
                {
                    raycastHitInfoArray[0] = hitInfo;
                    //Debug.Log("[DEBUG] check obj hit by beam: " + isHit + " skipFastCheck: " + skipFastCheck + " fastHit: " + hitInfo.collider.name + " T:" + Time.time);
                }
                else
                {
                    // Get all hits between fromPosition position and the distance away in the direction given. Include triggers.
                    // NOTE: RaycastNonAlloc, like other 3D multi-hit physics item, returns an UNSORTED array.
                    Ray ray = new Ray(fromPosition, direction);

                    // There is no direct access to the underlying Raycast(from, direction, results, distance, laymask, queryTriggerInteraction).
                    numHits = Physics.RaycastNonAlloc(ray, raycastHitInfoArray, distance, collisionLayerMask, QueryTriggerInteraction.Collide);
                }

                if (numHits > 1) { S3DUtils.SortHitsAsc(raycastHitInfoArray, numHits); }

                StickyControlModule stickyControlModule;
                StickyDamageReceiver stickyDamageReceiver;

                for (int hIdx = 0; hIdx < numHits; hIdx++)
                {
                    hitInfo = raycastHitInfoArray[hIdx];

                    Rigidbody hitRigidbody = hitInfo.rigidbody;
                    Collider hitCollider = hitInfo.collider;

                    if (hitRigidbody != null)
                    {
                        S3DDamageRegionReceiver damageRegionReceiver;
                        int hitDamageRegionId = 0;

                        // Is it a Sticky3D character?
                        bool isCharacterHit = hitRigidbody.TryGetComponent(out stickyControlModule);

                        // If the character has multiple damage regions, and we just hit the main character
                        // capsule collider, ignore it.
                        if (isCharacterHit)
                        {
                            if (stickyControlModule.NumberOfDamageRegions > 1)
                            {
                                // Skip the character's main capsule collider if there are additional damage regions
                                if (hitCollider.GetInstanceID() == stickyControlModule.StickyID) { continue; }
                                else if (hitCollider.TryGetComponent(out damageRegionReceiver) && damageRegionReceiver.isValid)
                                {
                                    stickyControlModule = damageRegionReceiver.stickyControlModule;
                                    hitDamageRegionId = damageRegionReceiver.damageRegionId;
                                }
                                else { isCharacterHit = false; }
                            }
                            else
                            {
                                hitDamageRegionId = stickyControlModule.GetMainDamageRegionID();
                            }
                        }
                        // I don't think this is necessary as the hit seems to always occur on the main character rigidbody
                        // even when there are ragdoll rigidbodies attached to bone colliders.
                        // NOTE: This might be too expensive for beams which could be continously hitting a character every frame
                        //else if (hitCollider.TryGetComponent(out damageRegionReceiver) && damageRegionReceiver.isValid)
                        //{
                        //    stickyControlModule = damageRegionReceiver.stickyControlModule;
                        //    hitDamageRegionId = damageRegionReceiver.damageRegionId;
                        //    isCharacterHit = true;
                        //}

                        // Is it a Sticky3D character?
                        if (isCharacterHit)
                        {
                            // Did we just hit the character that fired the beam? If so, ignore this hit.
                            if (stickyControlModule.StickyID == beamModule.sourceStickyId) { continue; }
                            // Check that we didn't hit a collider on a character that should be ignored
                            else if (!stickyControlModule.IsColliderHittableByWeapon(hitCollider)) { continue; }

                            float damageAmount = beamModule.damageRate * hitDuration;

                            // Apply damage to the Sticky character
                            stickyControlModule.ApplyNormalDamage(hitDamageRegionId, damageAmount, beamModule.damageType, hitInfo.point);

                            // Apply impact force to the Sticky character
                            //stickyControlModule.AddForceAtPosition(direction, impactForceValue, 1f, hitInfo.point);

                            // If required, call the custom method
                            if (stickyControlModule.callbackOnHit != null)
                            {
                                // Create a struct with the necessary parameters
                                S3DCharacterHitParameters s3dCharacterHitParameters = new S3DCharacterHitParameters
                                {
                                    hitInfo = hitInfo,
                                    stickyControlModule = stickyControlModule,
                                    projectilePrefab = null,
                                    beamPrefab = GetBeamPrefab(beamModule.beamPrefabID),
                                    damageAmount = damageAmount,
                                    damageTypeInt = (int)beamModule.damageType,
                                    impactForce = 0f,
                                    ammoTypeInt = -1,
                                    sourceStickyId = beamModule.sourceStickyId,
                                    sourceFactionId = beamModule.sourceFactionId,
                                    sourceModelId = beamModule.sourceModelId
                                };
                                // Call the custom callback
                                stickyControlModule.callbackOnHit(s3dCharacterHitParameters);
                            }

                            isHit = true;
                            break;
                        }
                        // Is a damage receiver attached to a rigidbody?
                        else if (hitRigidbody.TryGetComponent(out stickyDamageReceiver) && !hitCollider.isTrigger)
                        {
                            if (stickyDamageReceiver.callbackOnHit != null)
                            {
                                // Create a struct with the necessary parameters
                                S3DObjectHitParameters s3dObjectHitParameters = new S3DObjectHitParameters
                                {
                                    hitInfo = hitInfo,
                                    projectilePrefab = null,
                                    beamPrefab = GetBeamPrefab(beamModule.beamPrefabID),
                                    damageAmount = beamModule.damageRate * hitDuration,
                                    damageTypeInt = (int)beamModule.damageType,
                                    impactForce = 0f,
                                    ammoTypeInt = -1,
                                    sourceStickyId = beamModule.sourceStickyId,
                                    sourceFactionId = beamModule.sourceFactionId,
                                    sourceModelId = beamModule.sourceModelId
                                };
                                // Call the custom callback
                                stickyDamageReceiver.callbackOnHit(s3dObjectHitParameters);
                            }

                            isHit = true;
                            break;
                        }
                    }
                    
                    // Is it a regular object?
                    if (!hitCollider.isTrigger)
                    {
                        // Does this non-trigger collider have a damage receiver attached to it?
                        if (hitCollider.TryGetComponent(out stickyDamageReceiver))
                        {
                            if (stickyDamageReceiver.callbackOnHit != null)
                            {
                                // Create a struct with the necessary parameters
                                S3DObjectHitParameters callbackOnObjectHitParameters = new S3DObjectHitParameters
                                {
                                    hitInfo = hitInfo,
                                    projectilePrefab = null,
                                    beamPrefab = GetBeamPrefab(beamModule.beamPrefabID),
                                    damageAmount = beamModule.damageRate * hitDuration,
                                    damageTypeInt = (int)beamModule.damageType,
                                    impactForce = 0f,
                                    ammoTypeInt = -1,
                                    sourceStickyId = beamModule.sourceStickyId,
                                    sourceFactionId = beamModule.sourceFactionId,
                                    sourceModelId = beamModule.sourceModelId
                                };
                                // Call the custom callback
                                stickyDamageReceiver.callbackOnHit(callbackOnObjectHitParameters);
                            }
                        }

                        isHit = true;
                        break;
                    }
                }
            }

            return isHit;
        }

        /// <summary>
        /// Has a projectile hit?
        /// 1) A Sticky3D character?
        /// 2) A rigidbody with StickyDamageReceiver attached?
        /// 3) A rigidbody with StickyDynamicModule attached?
        /// 4) A non-trigger collider with a StickyDamageReceiver attached?
        /// 5) A regular non-trigger collider
        /// If hit:
        /// 1) Apply damage and force and invoke callbackOnHit on hit character (if any)
        /// 2) Apply force on dynamic module (if any)
        /// 3) Apply damage and invoke callbackOnHit on hit damage receiver (if any)
        /// </summary>
        /// <param name="fromPosition"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <param name="collisionLayerMask"></param>
        /// <param name="hitInfo"></param>
        /// <param name="sourceStickyID">Populate for Projectile Raycast weapons</param>
        /// <param name="projectile"></param>
        /// <param name="skipFastCheck"></param>
        /// <returns></returns>
        public bool CheckObjectHitByProjectile
        (
            Vector3 fromPosition, Vector3 direction, float distance,
            LayerMask collisionLayerMask, out RaycastHit hitInfo, int sourceStickyID,
            StickyProjectileModule projectile, bool skipFastCheck
        )
        {
            bool isHit = false;

            hitInfo = new RaycastHit();

            // Do a quick test to see if there is anything between the two world space positions
            if (skipFastCheck || Physics.Raycast(fromPosition, direction, out hitInfo, distance, collisionLayerMask, QueryTriggerInteraction.Collide))
            {
                // Was the first thing we hit a non-trigger collider?
                isHit = !skipFastCheck && !hitInfo.collider.isTrigger;

                int numHits = isHit ? 1 : 0;

                // If this was a regular non-trigger collider, no need to do any more raycasts
                if (isHit)
                {
                    raycastHitInfoArray[0] = hitInfo;

                    //Debug.Log("[DEBUG] CheckObjectHitByProjectile hit regular collider: " + hitInfo.transform.name + " T:" + Time.time);
                }
                else
                {
                    // Get all hits between fromPosition position and the distance away in the direction given. Include triggers.
                    // NOTE: RaycastNonAlloc, like other 3D multi-hit physics item, returns an UNSORTED array.
                    Ray ray = new Ray(fromPosition, direction);

                    // There is no direct access to the underlying Raycast(from, direction, results, distance, laymask, queryTriggerInteraction).
                    numHits = Physics.RaycastNonAlloc(ray, raycastHitInfoArray, distance, collisionLayerMask, QueryTriggerInteraction.Collide);
                }

                if (numHits > 1) { S3DUtils.SortHitsAsc(raycastHitInfoArray, numHits); }

                StickyControlModule stickyControlModule;              
                StickyDamageReceiver stickyDamageReceiver;
                StickyDynamicModule stickyDynamicModule;

                for (int hIdx = 0; hIdx < numHits; hIdx++)
                {
                    hitInfo = raycastHitInfoArray[hIdx];

                    Rigidbody hitRigidbody = hitInfo.rigidbody;
                    Collider hitCollider = hitInfo.collider;

                    if (hitRigidbody != null)
                    {
                        S3DDamageRegionReceiver damageRegionReceiver;
                        int hitDamageRegionId = 0;

                        // Is it a Sticky3D character?
                        bool isCharacterHit = hitRigidbody.TryGetComponent(out stickyControlModule);

                        // If the character has multiple damage regions, and we just hit the main character
                        // capsule collider, ignore it.
                        if (isCharacterHit)
                        {
                            if (stickyControlModule.NumberOfDamageRegions > 1)
                            {
                                // Skip the character's main capsule collider if there are additional damage regions
                                if (hitCollider.GetInstanceID() == stickyControlModule.StickyID) { continue; }
                                else if (hitCollider.TryGetComponent(out damageRegionReceiver) && damageRegionReceiver.isValid)
                                {
                                    stickyControlModule = damageRegionReceiver.stickyControlModule;
                                    hitDamageRegionId = damageRegionReceiver.damageRegionId;

                                    //Debug.Log("[DEBUG] Hit damage region " + damageRegionReceiver.name + " on " + stickyControlModule.name + ". T:" + Time.time);
                                }
                                else { isCharacterHit = false; }
                            }
                            else
                            {
                                hitDamageRegionId = stickyControlModule.GetMainDamageRegionID();
                            }
                        }
                        // I don't think this is necessary as the hit seems to always occur on the main character rigidbody
                        // even when there are ragdoll rigidbodies attached to bone colliders.
                        else if (hitCollider.TryGetComponent(out damageRegionReceiver) && damageRegionReceiver.isValid)
                        {
                            stickyControlModule = damageRegionReceiver.stickyControlModule;
                            hitDamageRegionId = damageRegionReceiver.damageRegionId;
                            isCharacterHit = true;
                            //Debug.Log("[DEBUG] Hit rigidbody " + hitRigidbody.name + " with a damage region receiver on " + stickyControlModule.name + " with a ragdoll.");
                        }

                        //Debug.Log("[DEBUG] hIdx " + hIdx + " " + hitInfo.transform.name + " collider: " + hitCollider.name + " isCharacterHit: " + isCharacterHit + " T: " + Time.time);

                        if (isCharacterHit)
                        {
                            // Did we just hit the character that fired the projectile? If so, ignore this hit.
                            if (sourceStickyID != 0) { if (stickyControlModule.StickyID == sourceStickyID) { continue; } }
                            else if (stickyControlModule.StickyID == projectile.sourceStickyId) { continue; }
                            // Check that we didn't hit a collider on a character that should be ignored
                            else if (!stickyControlModule.IsColliderHittableByWeapon(hitCollider)) { continue; }

                            float damageValue = projectile.damageAmount * GetAmmoDamageMultiplier(projectile.ammoTypeInt);
                            float impactForceValue = projectile.impactForce * GetAmmoImpactMultiplier(projectile.ammoTypeInt);

                            // Apply damage to the Sticky character
                            stickyControlModule.ApplyNormalDamage(hitDamageRegionId, damageValue, projectile.damageType, hitInfo.point);

                            // Apply impact force to the Sticky character
                            stickyControlModule.AddForceAtPosition(direction, impactForceValue, 1f, hitInfo.point);

                            // If required, call the custom method
                            if (stickyControlModule.callbackOnHit != null)
                            {
                                // Create a struct with the necessary parameters
                                S3DCharacterHitParameters s3dCharacterHitParameters = new S3DCharacterHitParameters
                                {
                                    hitInfo = hitInfo,
                                    stickyControlModule = stickyControlModule,
                                    projectilePrefab = GetProjectilePrefab(projectile.projectilePrefabID),
                                    beamPrefab = null,
                                    damageAmount = damageValue,
                                    damageTypeInt = (int)projectile.damageType,
                                    impactForce = impactForceValue,
                                    ammoTypeInt = projectile.ammoTypeInt,
                                    sourceStickyId = projectile.sourceStickyId,
                                    sourceFactionId = projectile.sourceFactionId,
                                    sourceModelId = projectile.sourceModelId
                                };
                                // Call the custom callback
                                stickyControlModule.callbackOnHit(s3dCharacterHitParameters);
                            }

                            isHit = true;
                            break;
                        }
                        else if (!hitCollider.isTrigger)
                        {
                            // A StickyDynamicModule can also have a StickyDamageReceiver attached. e.g. StickyDynamicTarget1.prefab

                            bool isDynAndOrDmgRvrHit = false;

                            // Is a dynamic module that is not in the static state
                            if (hitRigidbody.TryGetComponent(out stickyDynamicModule) && stickyDynamicModule.IsDynamic)
                            {
                                float impactForceValue = projectile.impactForce * GetAmmoImpactMultiplier(projectile.ammoTypeInt);

                                hitRigidbody.AddForceAtPosition(direction * impactForceValue, hitInfo.point);
                                isDynAndOrDmgRvrHit = true;
                            }
                            
                            // Is a damage receiver attached to a rigidbody?
                            if (hitRigidbody.TryGetComponent(out stickyDamageReceiver) && !hitCollider.isTrigger)
                            {
                                if (stickyDamageReceiver.callbackOnHit != null)
                                {
                                    // Create a struct with the necessary parameters
                                    S3DObjectHitParameters callbackOnObjectHitParameters = new S3DObjectHitParameters
                                    {
                                        hitInfo = hitInfo,
                                        projectilePrefab = GetProjectilePrefab(projectile.projectilePrefabID),
                                        beamPrefab = null,
                                        damageAmount = projectile.damageAmount * GetAmmoDamageMultiplier(projectile.ammoTypeInt),
                                        damageTypeInt = (int)projectile.damageType,
                                        impactForce = projectile.impactForce * GetAmmoImpactMultiplier(projectile.ammoTypeInt),
                                        ammoTypeInt = projectile.ammoTypeInt,
                                        sourceStickyId = projectile.sourceStickyId,
                                        sourceFactionId = projectile.sourceFactionId,
                                        sourceModelId = projectile.sourceModelId
                                    };
                                    // Call the custom callback
                                    stickyDamageReceiver.callbackOnHit(callbackOnObjectHitParameters);
                                }

                                isDynAndOrDmgRvrHit = true;
                            }

                            if (isDynAndOrDmgRvrHit)
                            {
                                isHit = true;
                                break;
                            }
                        }
                    }

                    // Is it a regular object
                    if (!hitCollider.isTrigger)
                    {
                        // Does this non-trigger collider have a damage receiver attached to it?
                        if (hitCollider.TryGetComponent(out stickyDamageReceiver))
                        {
                            if (stickyDamageReceiver.callbackOnHit != null)
                            {
                                // Create a struct with the necessary parameters
                                S3DObjectHitParameters callbackOnObjectHitParameters = new S3DObjectHitParameters
                                {
                                    hitInfo = hitInfo,
                                    projectilePrefab = GetProjectilePrefab(projectile.projectilePrefabID),
                                    beamPrefab = null,
                                    damageAmount = projectile.damageAmount * GetAmmoDamageMultiplier(projectile.ammoTypeInt),
                                    damageTypeInt = (int)projectile.damageType,
                                    impactForce = projectile.impactForce * GetAmmoImpactMultiplier(projectile.ammoTypeInt),
                                    ammoTypeInt = projectile.ammoTypeInt,
                                    sourceStickyId = projectile.sourceStickyId,
                                    sourceFactionId = projectile.sourceFactionId,
                                    sourceModelId = projectile.sourceModelId
                                };
                                // Call the custom callback
                                stickyDamageReceiver.callbackOnHit(callbackOnObjectHitParameters);
                            }
                        }

                        isHit = true;
                        break;
                    }
                }
            }

            return isHit;
        }

        /// <summary>
        /// Get the point where we hit a non-trigger collider or an object, or a Sticky3D character.
        /// If nothing was hit, return the point at the distance in the desired direction.
        /// If isWeapon is true, on characters, only weapon hittable trigger colliders will be detected.
        /// </summary>
        /// <param name="fromPosition">world space position to start from</param>
        /// <param name="direction">world space direction</param>
        /// <param name="distance"></param>
        /// <param name="collisionLayerMask"></param>
        /// <param name="sourceStickyID">The Sticky3D character that wants to perform the check. Else set to 0.</param>
        /// <param name="isWeapon"></param>
        /// <returns></returns>
        public Vector3 GetHitOrMaxPoint
        (
            Vector3 fromPosition, Vector3 direction, float distance,
            LayerMask collisionLayerMask, int sourceStickyID,
            bool isWeapon
        )
        {
            RaycastHit hitInfo = new RaycastHit();
            bool isHit = false;
            Vector3 hitPoint = Vector3.zero;

            // Do a quick test to see if there is anything between the two world space positions
            if (Physics.Raycast(fromPosition, direction, out hitInfo, distance, collisionLayerMask, QueryTriggerInteraction.Collide))
            {
                // Was the first thing we hit a non-trigger collider?
                isHit = !hitInfo.collider.isTrigger;

                int numHits = isHit ? 1 : 0;

                // If this was a regular non-trigger collider, no need to do any more raycasts
                if (isHit)
                {
                    raycastHitInfoArray[0] = hitInfo;
                }
                else
                {
                    // Get all hits between fromPosition position and the distance away in the direction given. Include triggers.
                    // NOTE: RaycastNonAlloc, like other 3D multi-hit physics item, returns an UNSORTED array.
                    Ray ray = new Ray(fromPosition, direction);

                    // There is no direct access to the underlying Raycast(from, direction, results, distance, laymask, queryTriggerInteraction).
                    numHits = Physics.RaycastNonAlloc(ray, raycastHitInfoArray, distance, collisionLayerMask, QueryTriggerInteraction.Collide);
                }

                if (numHits > 1) { S3DUtils.SortHitsAsc(raycastHitInfoArray, numHits); }

                for (int hIdx = 0; hIdx < numHits; hIdx++)
                {
                    hitInfo = raycastHitInfoArray[hIdx];

                    Collider hitCollider = hitInfo.collider;

                    if (!hitCollider.isTrigger)
                    {
                        isHit = true;
                        hitPoint = hitInfo.point;
                        break;
                    }
                    else
                    {
                        Rigidbody hitRigidbody = hitInfo.rigidbody;
                        StickyControlModule stickyControlModule;

                        // Did we hit a S3D character?
                        if (hitRigidbody != null && hitRigidbody.TryGetComponent(out stickyControlModule))
                        {
                            // Did we just hit the character that wants to perform the check? If so, ignore this hit.
                            if (sourceStickyID != 0 && stickyControlModule.StickyID == sourceStickyID) { continue; }
                            // If this is weapon-related, check that the collider on the character we hit should be detected by weapon fire.
                            else if (isWeapon && !stickyControlModule.IsColliderHittableByWeapon(hitCollider)) { continue; }

                            isHit = true;
                            hitPoint = hitInfo.point;
                            break;
                        }
                    }
                }
            }

            if (!isHit)
            {
                // Didn't hit anything in range, so just get a point at the furtherest distance in world space.
                hitPoint = fromPosition + (direction * distance);
            }

            return hitPoint;
        }

        /// <summary>
        /// Get the max world space point given a original point, a direction, and a distance from the starting point.
        /// </summary>
        /// <param name="fromPosition"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public Vector3 GetMaxPoint (Vector3 fromPosition, Vector3 direction, float distance)
        {
            return fromPosition + ((direction.sqrMagnitude < Mathf.Epsilon ? Vector3.forward : direction.normalized) * distance);
        }

        #endregion

        #region Public API Member Methods - Projectile Modules

        /// <summary>
        /// Returns the prefab for a projectile module given its projectile prefab ID.
        /// </summary>
        /// <param name="projectilePrefabID"></param>
        /// <returns></returns>
        public StickyProjectileModule GetProjectilePrefab (int projectilePrefabID)
        {
            // Check that a valid projectile prefab ID was supplied
            if (projectilePrefabID < projectileTemplatesList.Count)
            {
                return projectileTemplatesList[projectilePrefabID].s3dProjectileModulePrefab;
            }
            else { return null; }
        }

        /// <summary>
        /// Get the projectile pool for this prefab or create a new pool if one does not already exist.
        /// Return the projectilePrefabID for the pool or StickyManager.NoPrefabID (-1) if it fails.
        /// See also InstantiateProjectile(ipParms).
        /// </summary>
        /// <param name="stickyProjectileModule"></param>
        /// <returns></returns>
        public int GetOrCreateProjectilePool (StickyProjectileModule stickyProjectilePrefab)
        {
            int projectileTemplateIndex = NoPrefabID;

            if (stickyProjectilePrefab != null)
            {
                // Get the transform instance ID for this projectile module prefab
                int projectileTransformID = stickyProjectilePrefab.transform.GetInstanceID();
                // Search the projectile templates list to see if we already have an 
                // projectile prefab with the same instance ID
                projectileTemplateIndex = projectileTemplatesList.FindIndex(e => e.instanceID == projectileTransformID);

                if (projectileTemplateIndex == NoPrefabID)
                {
                    // If no match was found, create a new projectile template for this prefab
                    projectileTemplateIndex = AddProjectileTemplate(stickyProjectilePrefab, projectileTransformID);
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: stickyManager.GetorCreateProjectilePool - the stickyProjectilePrefab is null");
            }
            #endif

            return projectileTemplateIndex;
        }

        /// <summary>
        /// Get the projectile pool for this prefab or create a new pool if one does not already exist.
        /// Return the projectilePrefabID for the pool or StickyManager.NoPrefID (-1) if it fails.
        /// GetorCreateProjectilePool (StickyProjectileModule projectilePrefab) is slightly faster.
        /// See also InstantiateProjectile(igParms).
        /// </summary>
        /// <param name="projectilePrefab"></param>
        /// <returns></returns>
        public int GetOrCreateProjectilePool (GameObject projectilePrefab)
        {
            int projectileTemplateIndex = NoPrefabID;
            StickyProjectileModule stickyProjectileModule = null;

            if (projectilePrefab == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: stickyManager.GetOrCreateProjectilePool - the projectilePrefab is null");
                #endif
            }
            else if (projectilePrefab.TryGetComponent(out stickyProjectileModule))
            {
                // Get the transform instance ID for this projectile module prefab
                int projectileTransformID = stickyProjectileModule.transform.GetInstanceID();
                // Search the projectile templates list to see if we already have an 
                // projectile prefab with the same instance ID
                projectileTemplateIndex = projectileTemplatesList.FindIndex(e => e.instanceID == projectileTransformID);

                if (projectileTemplateIndex == NoPrefabID)
                {
                    // If no match was found, create a new projectile template for this prefab
                    projectileTemplateIndex = AddProjectileTemplate(stickyProjectileModule, projectileTransformID);
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: stickyManager.GetOrCreateProjectilePool - " + projectilePrefab.name + " does not include a StickyProjectileModule component");
            }
            #endif
            
            return projectileTemplateIndex;
        }

        /// <summary>
        /// Instantiates the projectile with ID projectilePrefabID at the position specified in world space and with the forwards
        /// and up directions specified.
        /// (ipParms.projectilePrefabID is the ID sent back to each weapon after calling weapon.ReInitialiseWeapon() method).
        /// (ipParms.effectsObjectPrefabID is ignored as an input as it is looked up in this method).
        /// </summary>
        /// <param name="ipParms"></param>
        /// <returns></returns>
        public StickyProjectileModule InstantiateProjectile (ref S3DInstantiateProjectileParameters ipParms)
        {
            StickyProjectileModule _stickyProjectileModule = null;

            if (isInitialised)
            {
                // Check that a valid ID has been provided
                if (ipParms.projectilePrefabID > StickyManager.NoPrefabID && ipParms.projectilePrefabID < projectileTemplatesList.Count)
                {
                    // Get the projectile template using its ID (this is simply the index of it in the projectile template list)
                    projectileTemplate = projectileTemplatesList[ipParms.projectilePrefabID];

                    // Get the regular hit effects prefab ID. This is the index in list of effect templates.
                    ipParms.effectsObjectPrefabID = projectileTemplate.s3dProjectileModulePrefab.effectsObjectPrefabID;

                    // Find the first inactive projectile in the pool
                    if (projectileTemplate.s3dProjectilePool == null)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("StickyManager InstantiateProjectile() s3dProjectilePool is null. Please Report.");
                        #endif
                    }
                    else
                    {
                        int firstInactiveIndex = projectileTemplate.s3dProjectilePool.FindIndex(g => !g.activeInHierarchy);

                        if (firstInactiveIndex == -1)
                        {
                            // All projectiles in the pool are currently active
                            // So if we want to get a new one, we'll need to add a new one to the pool
                            // First check if we have reached the max pool size
                            if (projectileTemplate.currentPoolSize < projectileTemplate.s3dProjectileModulePrefab.maxPoolSize)
                            {
                                // If we are still below the max pool size, add a new projectile instance to the pool
                                // Instantiate the projectile with the correct position and rotation
                                projectileGameObjectInstance = Instantiate(projectileTemplate.s3dProjectileModulePrefab.gameObject,
                                    ipParms.position, Quaternion.LookRotation(ipParms.fwdDirection, ipParms.upDirection) * projectileTemplate.modelRotation);
                                // Set the projectile's parent to be the manager projectile transform
                                projectileGameObjectInstance.transform.SetParent(projectilePooledTrfm);
                                // Initialise instance of the projectile
                                if (projectileGameObjectInstance.TryGetComponent(out _stickyProjectileModule))
                                {
                                    // Pre-assign the StickyManager so we don't need to check it each time we call stickyProjectileModule.Initialise()
                                    _stickyProjectileModule.stickyManager = this;
                                    ipParms.projectileSequenceNumber = _stickyProjectileModule.Initialise(ipParms);
                                    ipParms.projectilePoolListIndex = projectileTemplate.currentPoolSize;
                                    // Add the projectile to the list of pooled projectiles
                                    projectileTemplate.s3dProjectilePool.Add(projectileGameObjectInstance);
                                    // Update the current pool size counter
                                    projectileTemplate.currentPoolSize++;
                                }
                            }
                        }
                        else
                        {
                            // Get the projectile
                            projectileGameObjectInstance = projectileTemplate.s3dProjectilePool[firstInactiveIndex];
                            // Position the projectile
                            projectileGameObjectInstance.transform.SetPositionAndRotation(ipParms.position, Quaternion.LookRotation(ipParms.fwdDirection, ipParms.upDirection) * projectileTemplate.modelRotation);
                            // Set the projectile to active
                            projectileGameObjectInstance.SetActive(true);
                            // Initialise the instance of the projectile
                            if (projectileGameObjectInstance.TryGetComponent(out _stickyProjectileModule))
                            {
                                ipParms.projectileSequenceNumber = _stickyProjectileModule.Initialise(ipParms);
                                ipParms.projectilePoolListIndex = firstInactiveIndex;
                            }
                        }
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    // If an invalid ID is provided, log a warning
                    Debug.LogWarning("StickyManager InstantiateProjectile() Warning: Provided projectilePrefabID was invalid (" + ipParms.projectilePrefabID + ").");
                }
                #endif

            }
            #if UNITY_EDITOR
            else
            {
                // If the method is called before the manager is initialised, log a warning
                Debug.LogWarning("StickyManager InstantiateProjectile() Warning: Method was called before the manager was initialised.");
            }
            #endif

            return _stickyProjectileModule;
        }

        /// <summary>
        /// Pause all Pooled Projectiles in the scene. This is useful when
        /// pausing your game.
        /// </summary>
        public void PauseProjectiles()
        {
            PauseProjectiles(true);
        }

        /// <summary>
        /// Resume all Pooled projectiles in the scene. This is useful when
        /// unpausing your game.
        /// </summary>
        public void ResumeProjectiles()
        {
            PauseProjectiles(false);
        }

        #endregion
    }

    #region Public Structures

    /// <summary>
    /// Parameters structure for use with stickyManager.InstantiateBeam(..).
    /// struct members subject to change without notice.
    /// TODO - need to identify the weapon...
    /// </summary>
    public struct S3DInstantiateBeamParameters
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
        /// S3D character that fired the beam, else 0
        /// </summary>
        public int stickyId;
        /// <summary>
        /// The faction or alliance of the character that fired the beam belongs to.
        /// </summary>
        public int factionId;
        /// <summary>
        /// The type, category, or model of the character that fired the beam.
        /// </summary>
        public int modelId;
        /// <summary>
        /// The zero-based index of the fire position offset on the weapon that fired the beam.
        /// If there are no position offsets, it will be ignored.
        /// </summary>
        public int firePositionOffsetIndex;
        /// <summary>
        /// This is the index in the StickyManager effectsObjectTemplatesList.
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
    /// Parameters structure for use with stickyManager.InstantiateDecal(..).
    /// struct members subject to change without notice.
    /// </summary>
    public struct S3DInstantiateDecalParameters
    {
        #region Public variables

        // The decal template index
        public int decalPrefabID;

        // The position where the decal will be instantiated
        public Vector3 position;

        // The rotation of the decal when instantiated
        public Quaternion rotation;

        // The direction the decal is facing
        public Vector3 fwdDirection;

        /// <summary>
        /// The collision LayerMask sent as an integer
        /// </summary>
        public int collisionMaskLayerInt;

        /// <summary>
        /// Return value of the zero-based index in the current pool
        /// Returns -1 if non-pooled.
        /// </summary>
        public int decalPoolListIndex;
        /// <summary>
        /// Return value of the unique number for the decal instance
        /// </summary>
        public uint decalSequenceNumber;

        #endregion
    }

    /// <summary>
    /// Parameters structure for use with stickyManager.InstantiateDynamicObject(..).
    /// struct members subject to change without notice.
    /// </summary>
    public struct S3DInstantiateDynamicObjectParameters
    {
        #region Public variables

        // The dynamic object template index
        public int dynamicObjectPrefabID;

        // The position where the dynamic object will be instantiated
        public Vector3 position;

        // The rotation of the dynamic object when instantiated
        public Quaternion rotation;

        /// <summary>
        /// Return value of the zero-based index in the current pool
        /// Returns -1 if non-pooled.
        /// </summary>
        public int dynamicObjectPoolListIndex;
        /// <summary>
        /// Return value of the unique number for the dynamic instance
        /// </summary>
        public uint dynamicObjectSequenceNumber;

        #endregion
    }

    /// <summary>
    /// Parameters structure for use with stickyManager.InstantiateEffectsObject(..).
    /// struct members subject to change without notice.
    /// </summary>
    public struct S3DInstantiateEffectsObjectParameters
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
    /// Parameters structure for use with stickyManager.InstantiateSoundFX(..).
    /// struct members subject to change without notice.
    /// </summary>
    public struct S3DInstantiateSoundFXParameters
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
    /// Parameters structure for use with stickyManager.InstantiateGenericObject(..).
    /// struct members subject to change without notice.
    /// </summary>
    public struct S3DInstantiateGenericObjectParameters
    {
        #region Public variables

        /// <summary>
        /// The generic module template index
        /// </summary>
        public int genericModulePrefabID;

        /// <summary>
        /// The position where the generic object will be instantiated
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// The rotation of the generic object when instantiated
        /// </summary>
        public Quaternion rotation;

        /// <summary>
        /// Return value of the zero-based index in the current pool
        /// Returns -1 if non-pooled.
        /// </summary>
        public int genericObjectPoolListIndex;
        /// <summary>
        /// Return value of the unique number for the generic module instance
        /// </summary>
        public uint genericObjectSequenceNumber;
        /// <summary>
        /// Set this to true if you want to use the despawnTime (set to >= 0)
        /// but don't want the StickyGenericModule to invoke the DestroyGenericObject()
        /// after the despawnTime has lapsed.
        /// </summary>
        public bool overrideAutoDestroy;

        #endregion
    }

    /// <summary>
    /// Parameters structure for use with stickyManager.InstantiateProjectile(..).
    /// struct members subject to change without notice.
    /// </summary>
    public struct S3DInstantiateProjectileParameters
    {
        #region Public Variables
        
        /// <summary>
        /// The projectile template index
        /// </summary>
        public int projectilePrefabID;

        // The position where the projectile object will be instantiated
        public Vector3 position;

        /// <summary>
        /// The rotation of the projectile when instantiated
        /// </summary>
        //public Quaternion rotation;

        /// <summary>
        /// The world space direction the projectile is fired in
        /// </summary>
        public Vector3 fwdDirection;

        /// <summary>
        /// The world space up direction of the projectile
        /// </summary>
        public Vector3 upDirection;

        /// <summary>
        /// Current velocity of the weapon that fired the projectile.
        /// It could also be the velocity of the character holding the weapon.
        /// </summary>
        public Vector3 weaponVelocity;

        /// <summary>
        /// S3D character that fired the projectile, else 0
        /// </summary>
        public int stickyId;

        /// <summary>
        /// The faction or alliance of the character that fired the projectile belongs to.
        /// </summary>
        public int factionId;

        /// <summary>
        /// The type, category, or model of the character that fired the projectile.
        /// </summary>
        public int modelId;

        /// <summary>
        /// This is the index in the StickyManager effectsObjectTemplatesList.
        /// </summary>
        public int effectsObjectPrefabID;

        /// <summary>
        /// The collision LayerMask sent as an integer
        /// </summary>
        public int collisionMaskLayerInt;

        /// <summary>
        /// The 0-25 (A-Z) ammo type that was fired from the weapon
        /// </summary>
        public int ammoTypeInt;

        /// <summary>
        /// Return value of the zero-based index in the current pool
        /// Returns -1 if non-pooled.
        /// </summary>
        public int projectilePoolListIndex;

        /// <summary>
        /// Return value of the unique number for the dynamic instance
        /// </summary>
        public uint projectileSequenceNumber;
        #endregion
    }

    /// <summary>
    /// A struct used to uniquely identify a Sticky Beam Module
    /// </summary>
    public struct S3DBeamItemKey
    {
        #region Public variables

        /// <summary>
        /// -1 = unset
        /// </summary>
        public int beamTemplatesListIndex;

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
        public S3DBeamItemKey(int templateId, int poolId, uint sequenceNumber)
        {
            beamTemplatesListIndex = templateId;
            beamPoolListIndex = poolId;
            beamSequenceNumber = sequenceNumber;
        }
        #endregion
    }

    /// <summary>
    /// A struct used to uniquely identity a StickyDecalModule
    /// </summary>
    public struct S3DDecalItemKey
    {
        #region Public variables

        /// <summary>
        /// -1 = unset
        /// </summary>
        public int decalTemplatesListIndex;

        /// <summary>
        /// -1 = unset
        /// </summary>
        public int decalPoolListIndex;

        /// <summary>
        /// 0 = unset
        /// </summary>
        public uint decalSequenceNumber;
        #endregion

        #region Constructor
        public S3DDecalItemKey(int templateId, int poolId, uint sequenceNumber)
        {
            decalTemplatesListIndex = templateId;
            decalPoolListIndex = poolId;
            decalSequenceNumber = sequenceNumber;
        }

        #endregion
    }

    /// <summary>
    /// A struct used to uniquely identity a StickyDynamicModule
    /// </summary>
    public struct S3DDynamicItemKey
    {
        #region Public variables

        /// <summary>
        /// -1 = unset
        /// </summary>
        public int dynamicObjectTemplatesListIndex;

        /// <summary>
        /// -1 = unset
        /// </summary>
        public int dynamicObjectPoolListIndex;

        /// <summary>
        /// 0 = unset
        /// </summary>
        public uint dynamicObjectSequenceNumber;
        #endregion

        #region Constructor
        public S3DDynamicItemKey(int templateId, int poolId, uint sequenceNumber)
        {
            dynamicObjectTemplatesListIndex = templateId;
            dynamicObjectPoolListIndex = poolId;
            dynamicObjectSequenceNumber = sequenceNumber;
        }

        #endregion
    }

    /// <summary>
    /// A struct used to uniquely identity a StickyEffectsModule
    /// </summary>
    public struct S3DEffectItemKey
    {
        #region Public variables

        /// <summary>
        /// -1 = unset
        /// </summary>
        public int effectsObjectTemplatesListIndex;

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
        public S3DEffectItemKey(int templateId, int poolId, uint sequenceNumber)
        {
            effectsObjectTemplatesListIndex = templateId;
            effectsObjectPoolListIndex = poolId;
            effectsObjectSequenceNumber = sequenceNumber;
        }

        #endregion

        #region Public Methods

        public void Reset()
        {
            effectsObjectTemplatesListIndex = -1;
            effectsObjectPoolListIndex = -1;
            effectsObjectSequenceNumber = 0;
        }

        #endregion
    }

    /// <summary>
    /// A struct used to uniquely identity a StickyGenericModule
    /// </summary>
    public struct S3DGenericItemKey
    {
        #region Public variables

        /// <summary>
        /// -1 = unset
        /// </summary>
        public int genericObjectTemplatesListIndex;

        /// <summary>
        /// -1 = unset
        /// </summary>
        public int genericObjectPoolListIndex;

        /// <summary>
        /// 0 = unset
        /// </summary>
        public uint genericObjectSequenceNumber;
        #endregion

        #region Constructor
        public S3DGenericItemKey(int templateId, int poolId, uint sequenceNumber)
        {
            genericObjectTemplatesListIndex = templateId;
            genericObjectPoolListIndex = poolId;
            genericObjectSequenceNumber = sequenceNumber;
        }

        #endregion
    }

    #endregion
}