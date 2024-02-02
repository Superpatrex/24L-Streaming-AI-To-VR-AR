using UnityEngine;
using System.Collections.Generic;
#if SSC_URP
using UnityEngine.Rendering.Universal;
#elif SSC_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Demo script to render stars in sky with built-in Render Pipeline.
    /// Currently does not work with LWRP or HDRP.
    /// URP requires Unity 2019.4+ and URP 7.3.1+.
    /// Supports one or two cameras.
    /// </summary>
    public class Celestials : MonoBehaviour
    {
        // To change:
        // 1. Delete the existing (default) Unity layer of SSC Celestials
        // 2. Change celestialsUnityLayer here
        // 3. Save this file
        // 4. Go back to the Unity editor and wait for it to recompile scripts
        // This will need to be done each time you import a new version of SSC.
        #if LANDSCAPE_BUILDER
        public static readonly int celestialsUnityLayer = 26;
        #else
        public static readonly int celestialsUnityLayer = 25;
        #endif

        #region Enumerations

        public enum EnvAmbientSource
        {
            Colour = 0,
            Gradient = 1,
            Skybox = 2
        }

        #endregion

        #region Public Variables
        public Camera camera1;
        public Camera camera2;
        public Color nightSkyColour = new Color(21f / 255f, 21f / 255f, 21f / 255f, 1f);

        [Tooltip("The material to be used to create each star")]
        public Material starMaterial;

        [Tooltip("A low poly mesh to be used to create each star")]
        public Mesh starMesh;

        public int numberOfStars = 1000;

        public float starSize = 2f;

        [Tooltip("Attempt to make a more randomised position for stars - especially for RefreshCelestials()")]
        public bool isStarfieldRandom = false;

        public bool initialiseOnAwake = true;

        public bool useHorizon = true;
        public EnvAmbientSource envAmbientSource = EnvAmbientSource.Colour;

        [Tooltip("By default the ambient sky colour will be set to the nightSkyColour")]
        public bool overrideAmbientColour = false;

        [Tooltip("If overriding the ambient colour, this is the ambient sky colour")]
        public Color ambientSkyColour = new Color(21f / 255f, 21f / 255f, 21f / 255f, 1f);

        [Tooltip("Near clip plane for the celestial camera(s). Reduce if planets start being clipped by camera")]
        [Range(0.0001f, 0.1f)] public float nearClipPlane = 0.1f;

        [Tooltip("Create the planet that are marked as hidden")]
        public bool isCreateHiddenPlanets = false;

        [Tooltip("List of planet or celestial objects")]
        public List<SSCCelestial> celestialList = new List<SSCCelestial>();
        #endregion

        #region Public Properties
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// By default, stars are randomised in a consistent way to recreate reproduceable results.
        /// However, they can be further randomised to make the starfield to be more random each time
        /// it is recreated during the same session.
        /// </summary>
        public bool IsStarfieldRandom { get { return isStarfieldRandom; } set { isStarfieldRandom = value; } }
        #endregion

        #region Private variables
        [System.NonSerialized] private Camera celestialsCamera1 = null;
        [System.NonSerialized] private Camera celestialsCamera2 = null;
        private bool isInitialised = false;
        [System.NonSerialized] private CombineInstance[] combineInstances;
        private bool isCamera2Initialised = false;
        private SSCRandom sscRandom = null;
        [System.NonSerialized] private MeshRenderer celestialMeshRenderer = null;
        [System.NonSerialized] private MeshRenderer starMRenderer = null;

        [System.NonSerialized] private Transform planetsTrfm = null;

        private float minCelestialCameraDistance = 0.2f;
        #endregion

        #region Initialisation Methods

        // Use this for initialization
        void Awake()
        {
            if (initialiseOnAwake) { Initialise(); }
        }

        /// <summary>
        /// Initialise the celestials camera(s).
        /// </summary>
        public void Initialise()
        {
            isInitialised = false;

            // Only add celestials camera if it doesn't already exist
            Transform celestialCamera1Trm = GetorCreateCamera("Celestials Camera 1", out celestialsCamera1);

            // Configure the celestials camera 1
            if (celestialCamera1Trm != null)
            {
                if (camera1 == null) { Debug.LogWarning("SSC Celestials - the (main) Camera1 is not set"); }
                else if (celestialsCamera1 == null) { Debug.LogWarning("SSC Celestials - did not create Celestials Camera 1"); }
                else if (IsVerifyStarSettings())
                {
                    ConfigCameras(celestialsCamera1, camera1, -100f);
                    BuildCelestials();                  

                    if (camera2 != null) { InitialiseCamera2(); }

                    // In the Unity Lighting editor for SRP, AmbientMode.Flat = Color and Trilight = Gradient
                    RenderSettings.ambientMode = envAmbientSource == EnvAmbientSource.Gradient ? UnityEngine.Rendering.AmbientMode.Trilight : envAmbientSource == EnvAmbientSource.Colour ? UnityEngine.Rendering.AmbientMode.Flat : UnityEngine.Rendering.AmbientMode.Skybox;

                    if (overrideAmbientColour)
                    {
                        RenderSettings.ambientSkyColor = ambientSkyColour;
                    }
                    else
                    {
                        RenderSettings.ambientSkyColor = nightSkyColour;
                    }                   

                    isInitialised = true;
                }
            }
        }

        #endregion

        #region Update Methods

        // Called during the physics update loop
        // Added in SSC 1.3.7 to make more compatible with
        // Sticky3D character when walking and looking left/right
        void FixedUpdate()
        {
            if (isInitialised)
            {
                UpdateCelestialsRotation();
            }
        }

        /// <summary>
        /// Changed from Update to LateUpdate() in SSC 1.3.3
        /// to overcome issue in a build when using S3D in first
        /// person and character movement in FixedUpdate.
        /// </summary>
        private void LateUpdate()
        {
            if (isInitialised)
            {
                UpdateCelestialsRotation();
            }
        }

        #endregion

        #region Private Methods

        private void BuildCelestials()
        {
            DestroyStarGameObject();
            CreateOrConfigureRandom();

            GameObject starsGameObject = CreateStars();

            if (starsGameObject != null)
            {
                CreatePlanets(starsGameObject);
            }
        }

        /// <summary>
        /// Calculate where the celestial (planet) should be placed relative to the celestials camera(s).
        /// </summary>
        /// <param name="sscCelestial"></param>
        /// <returns></returns>
        private Vector3 CalcCelestialPosition(SSCCelestial sscCelestial)
        {
            Vector3 objectPosition = sscCelestial.celestialToDirection * (minCelestialCameraDistance + sscCelestial.currentCelestialDistance);

            if (useHorizon && objectPosition.y < 0f) { objectPosition.y = -objectPosition.y; }

            // Cater for the celestials gameobject not being at 0,0,0
            objectPosition += transform.position;

            return objectPosition;
        }

        /// <summary>
        /// Configure an instance of SSCRandom, if it hasn't
        /// already been done in this session.
        /// </summary>
        private void CreateOrConfigureRandom()
        {
            if (sscRandom == null)
            {
                sscRandom = new SSCRandom();
                sscRandom.SetSeed(821997);
            }
        }

        private GameObject CreateStars()
        {
            GameObject starMeshGameObject = null;

            if (starMesh != null)
            {
                starMeshGameObject = new GameObject("SSC Stars");
                starMeshGameObject.transform.parent = transform;
                starMeshGameObject.transform.localPosition = Vector3.zero;
                starMeshGameObject.transform.localRotation = Quaternion.identity;
                starMeshGameObject.transform.localScale = Vector3.one;
                MeshFilter starMFilter = starMeshGameObject.AddComponent<MeshFilter>();
                starMRenderer = starMeshGameObject.AddComponent<MeshRenderer>();

                // Get the number of verts per star mesh
                int starMeshVerts = starMesh.vertices == null ? 0 : starMesh.vertices.Length;
                int totalVerts = 0;

                starMRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                starMRenderer.receiveShadows = false;

                starMeshGameObject.layer = celestialsUnityLayer;
                UnityEngine.Random.InitState(0);

                if (isStarfieldRandom) { sscRandom.SetSeed((int)Time.realtimeSinceStartup); }

                combineInstances = new CombineInstance[numberOfStars];
                Vector3 starPos;
                for (int i = 0; i < combineInstances.Length; i++)
                {
                    // Attempt to create a more randomised layout for the stars
                    if (isStarfieldRandom)
                    {
                        starPos = UnityEngine.Random.onUnitSphere * sscRandom.Range(1f, 5f);
                    }
                    else
                    {
                        starPos = UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(1f, 5f);
                    }
                    if (useHorizon && starPos.y < 0f) { starPos.y = -starPos.y; }
                    combineInstances[i].transform = Matrix4x4.TRS(starPos, Quaternion.identity, Vector3.one * 0.001f * starSize);
                    combineInstances[i].mesh = starMesh;
                    totalVerts += starMeshVerts;
                }

                starMFilter.sharedMesh = new Mesh();
                starMFilter.sharedMesh.name = "SSC Stars Mesh";
                // Check if there are more than 65535 verts
                if (totalVerts > ushort.MaxValue)
                {
                    starMFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                }
                starMFilter.sharedMesh.CombineMeshes(combineInstances);

                if (starMaterial != null) { starMRenderer.material = starMaterial; }

                starMeshGameObject.isStatic = true;
            }

            return starMeshGameObject;
        }

        /// <summary>
        /// Create a planet.
        /// Behaviour change in 1.3.7 Beta 6a (create hidden planets but disable them immediately).
        /// </summary>
        /// <returns></returns>
        private Transform CreatePlanet(SSCCelestial sscCelestial)
        {
            Transform planetTrfm = null;

            if (sscCelestial != null)
            {
                CreateOrConfigureRandom();

                // Validate the min/max size values
                sscCelestial.minSize = Mathf.Clamp(sscCelestial.minSize, 1, 20);
                sscCelestial.maxSize = Mathf.Clamp(sscCelestial.maxSize, sscCelestial.minSize, 20);

                sscCelestial.minDistance = Mathf.Clamp(sscCelestial.minDistance, 0f, 1f);
                sscCelestial.maxDistance = Mathf.Clamp(sscCelestial.maxDistance, sscCelestial.minDistance, 1f);

                // Get the random numbers before deciding to skip hidden (not created) planets.
                // This allows the other planet to still be rendered in their original locations.
                float planetScale = sscRandom.Range(sscCelestial.minSize, sscCelestial.maxSize) * 0.01f;
                sscCelestial.currentCelestialDistance = Mathf.Clamp(sscRandom.Range(sscCelestial.minDistance, sscCelestial.maxDistance + 0.01f), sscCelestial.minDistance, sscCelestial.maxDistance);

                // The min distance from the camera is ~0.2, so let the user select a -1.0 - 1.0 range,
                // but always add on the minimum camera distance. This should mostly avoid the planet being
                // clipped by the camera.

                sscCelestial.celestialToDirection = sscCelestial.isRandomPosition ? UnityEngine.Random.onUnitSphere : new Vector3(sscCelestial.positionX, sscCelestial.positionY, sscCelestial.positionZ);

                // Normalise the user input
                if (!sscCelestial.isRandomPosition)
                {
                    if (sscCelestial.celestialToDirection.sqrMagnitude < Mathf.Epsilon)
                    {
                        sscCelestial.celestialToDirection = new Vector3(0f, 0f, 1f);
                    }

                    sscCelestial.celestialToDirection.Normalize();
                }

                Vector3 planetPos = CalcCelestialPosition(sscCelestial);

                if (!sscCelestial.isHidden || isCreateHiddenPlanets)
                {
                    GameObject planetGO = null;

                    if (sscCelestial.celestialMesh == null)
                    {
                        planetGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                        Collider planetCollider;
                        if (planetGO.TryGetComponent(out planetCollider))
                        {
                            #if UNITY_EDITOR
                            DestroyImmediate(planetCollider);
                            #else
                            Destroy(planetCollider);
                            #endif
                        }
                    }
                    else
                    {
                        planetGO = new GameObject(sscCelestial.name);

                        MeshFilter mFilter = planetGO.AddComponent<MeshFilter>();
                        MeshRenderer mRenderer = planetGO.AddComponent<MeshRenderer>();

                        mFilter.mesh = sscCelestial.celestialMesh;
                        mRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                        mRenderer.receiveShadows = true;
                    }

                    if (!string.IsNullOrEmpty(sscCelestial.name)) { planetGO.name = sscCelestial.name; }
                    else { planetGO.name = "Unknown"; }

                    planetGO.layer = celestialsUnityLayer;
                    planetTrfm = planetGO.transform;

                    // Store the transform for easy access
                    sscCelestial.celestialTfrm = planetTrfm;

                    planetTrfm.SetPositionAndRotation(planetPos, Quaternion.identity);
                    planetTrfm.localScale *= planetScale;

                    // If the user has supplied a material attempt to update the planet's material
                    if (sscCelestial.celestialMaterial != null)
                    {
                        if (planetGO.TryGetComponent(out celestialMeshRenderer))
                        {
                            celestialMeshRenderer.material = sscCelestial.celestialMaterial;
                        }
                    }

                    if (sscCelestial.isFaceCamera1 && camera1 != null)
                    {
                        // Planet (A) to look at camera (B). Direction = (B-A).normalized
                        Vector3 _lookVector = camera1.transform.position - planetPos;
                        if (_lookVector != Vector3.zero)
                        {
                            planetTrfm.rotation = Quaternion.LookRotation(_lookVector.normalized);
                        }
                    }
                    else
                    {
                        planetTrfm.rotation = Quaternion.Euler(sscCelestial.rotation);
                    }

                    if (sscCelestial.isHidden) { planetGO.SetActive(false); }
                }
            }

            return planetTrfm;
        }

        /// <summary>
        /// Attempt to create the planets as a child of the stars
        /// </summary>
        /// <param name="starsGameObject"></param>
        private void CreatePlanets(GameObject starsGameObject)
        {
            if (starsGameObject != null)
            {
                GameObject planetsGameObject = new GameObject("SSC Planets");

                if (planetsGameObject != null)
                {
                    planetsTrfm = planetsGameObject.transform;

                    planetsTrfm.SetParent(starsGameObject.transform);
                    planetsTrfm.localPosition = Vector3.zero;
                    planetsTrfm.localRotation = Quaternion.identity;
                    planetsTrfm.localScale = Vector3.one;
                    planetsGameObject.layer = celestialsUnityLayer;

                    // Create from a list of planet prefabs
                    int numPlanets = celestialList == null ? 0 : celestialList.Count;

                    for (int cIdx = 0; cIdx < numPlanets; cIdx++)
                    {
                        Transform planet1Trfm = CreatePlanet(celestialList[cIdx]);

                        if (planet1Trfm != null)
                        {
                            planet1Trfm.SetParent(planetsTrfm);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Configure a celestial and display camera pair
        /// </summary>
        /// <param name="celestialsCamera"></param>
        /// <param name="displayCamera"></param>
        /// <param name="celestialsCameraDepth"></param>
        private void ConfigCameras(Camera celestialsCamera, Camera displayCamera, float celestialsCameraDepth)
        {
            if (celestialsCamera != null && displayCamera != null)
            {
                celestialsCamera.nearClipPlane = nearClipPlane;
                celestialsCamera.farClipPlane = 10f;
                celestialsCamera.depth = celestialsCameraDepth;
                celestialsCamera.clearFlags = envAmbientSource == EnvAmbientSource.Skybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
                celestialsCamera.backgroundColor = nightSkyColour;
                celestialsCamera.cullingMask = 1 << celestialsUnityLayer;

                celestialsCamera.fieldOfView = displayCamera.fieldOfView;
                // Set the celestials camera to use the same monitor or "display" as the display camera
                celestialsCamera.targetDisplay = displayCamera.targetDisplay;

                #if SSC_URP
                // Make the main camera an overlay camera (requires URP 7.3.1 or newer)
                var cameraDataCelestials = celestialsCamera.GetUniversalAdditionalCameraData();
                var cameraDataDisplay = displayCamera.GetUniversalAdditionalCameraData();
                if (cameraDataCelestials != null && cameraDataDisplay != null)
                {
                    cameraDataCelestials.renderType = CameraRenderType.Base;

                    // The display camera which renders the game, becomes an overlay camera
                    cameraDataDisplay.renderType = CameraRenderType.Overlay;
                    displayCamera.clearFlags = CameraClearFlags.Depth;

                    // Add the main overlay camera to the cameraStack list on the celestials camera
                    cameraDataCelestials.cameraStack.Add(displayCamera);
                }
                #elif SSC_HDRP
                var cameraDataCelestials = celestialsCamera.GetComponent<HDAdditionalCameraData>();
                var cameraDataDisplay = displayCamera.GetComponent<HDAdditionalCameraData>();
                if (cameraDataCelestials != null && cameraDataDisplay != null)
                {
                    cameraDataDisplay.clearColorMode = HDAdditionalCameraData.ClearColorMode.None;
                    //cameraDataDisplay.backgroundColorHDR = nightSkyColour;
                    cameraDataDisplay.volumeLayerMask = 0;
                    //cameraDataCelestials.clearColorMode = envAmbientSource == EnvAmbientSource.Skybox ? HDAdditionalCameraData.ClearColorMode.Sky : HDAdditionalCameraData.ClearColorMode.Color;
                    //cameraDataCelestials.backgroundColorHDR = nightSkyColour;
                    //cameraDataCelestials.volumeLayerMask = 0;
                    displayCamera.clearFlags = CameraClearFlags.Depth;
                }
                #else
                displayCamera.clearFlags = CameraClearFlags.Depth;
                #endif
            }
        }

        private void DestroyStarGameObject()
        {
            Transform starObj = transform.Find("SSC Stars");
            if (starObj != null) { DestroyImmediate(starObj.gameObject); }
        }

        /// <summary>
        /// Find the celestials camera in the scene or create a new one.
        /// This is a little simplistic and doesn't add the celestials camera
        /// if the parent transform already exists.
        /// </summary>
        /// <param name="cameraName"></param>
        /// <returns></returns>
        private Transform GetorCreateCamera(string cameraName, out Camera celestialsCamera)
        {
            celestialsCamera = null;

            Transform celestialCameraTrm = transform.Find(cameraName);
            if (celestialCameraTrm == null)
            {
                GameObject celestialsCameraObject = new GameObject(cameraName);

                if (celestialsCameraObject != null)
                {
                    celestialsCameraObject.transform.parent = transform;
                    celestialsCameraObject.transform.localPosition = Vector3.zero;
                    celestialsCamera = celestialsCameraObject.AddComponent<Camera>();
                    celestialCameraTrm = celestialsCameraObject.transform;
                }
            }
            else
            {
                celestialsCamera = celestialCameraTrm.GetComponent<Camera>();
            }

            return celestialCameraTrm;
        }

        private void UpdateCelestialsRotation()
        {
            if (camera1 != null)
            {
                celestialsCamera1.transform.rotation = camera1.transform.rotation;
                celestialsCamera1.fieldOfView = camera1.fieldOfView;
            }

            if (isCamera2Initialised)
            {
                celestialsCamera2.transform.rotation = camera2.transform.rotation;
                celestialsCamera2.fieldOfView = camera2.fieldOfView;
            }
        }

        /// <summary>
        /// Verify if the star settings look acceptable
        /// </summary>
        /// <returns></returns>
        private bool IsVerifyStarSettings()
        {
            bool isVerified = false;

            if (starMaterial == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("SSC Celestials - the star material is not set");
                #endif
            }
            else if (starMesh == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("SSC Celestials - the star mesh is not set");
                #endif
            }
            else if (starSize <= 0f)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("SSC Celestials - the star size must be greater than 0");
                #endif
            }
            else if (numberOfStars < 1)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("SSC Celestials - the number of stars must be greater than 0");
                #endif
            }
            else
            {
                isVerified = true;
            }

            return isVerified;
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Get a celestial (planet) by using its unique identifier.
        /// </summary>
        /// <param name="celestialId"></param>
        public SSCCelestial GetCelestialByID(int celestialId)
        {
            SSCCelestial celestial = null;

            if (celestialId != 0 && celestialList != null)
            {
                for (int cIdx = 0; cIdx < celestialList.Count; cIdx++)
                {
                    SSCCelestial _tempCelestial = celestialList[cIdx];
                    if (_tempCelestial != null && _tempCelestial.celestialId == celestialId)
                    {
                        celestial = _tempCelestial;
                        break;
                    }
                }
            }

            return celestial;
        }

        /// <summary>
        /// Get a celestial (planet) by using its zero-based index in the list
        /// </summary>
        /// <param name="index"></param>
        public SSCCelestial GetCelestialByIndex(int index)
        {
            int numCelestials = celestialList == null ? 0 : celestialList.Count;

            if (index >= 0 && index < numCelestials)
            {
                return celestialList[index];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get a celestial (planet) by using its case-sensitive name.
        /// WARNING: This will increase GC, so where possible, instead
        /// use either GetCelestialByIndex or GetCelestialById.
        /// </summary>
        /// <param name="celestialName"></param>
        /// <returns></returns>
        public SSCCelestial GetCelestialByName(string celestialName)
        {
            SSCCelestial celestial = null;

            int numCelestials = celestialList == null ? 0 : celestialList.Count;

            if (numCelestials > 0 && !string.IsNullOrEmpty(celestialName))
            {
                celestial = celestialList.Find(c => c.name == celestialName);
            }

            return celestial;
        }

        /// <summary>
        /// Get the relative distance between the celestial camera and the planet
        /// </summary>
        /// <param name="celestial"></param>
        /// <param name="forceRecalc">If position has been changed outside SSCCelestials, recalculate its distance</param>
        /// <returns></returns>
        public float GetCelestialDistance(SSCCelestial celestial, bool forceRecalc = false)
        {
            if (celestial != null && !celestial.isHidden)
            {
                if (!forceRecalc)
                {
                    return celestial.currentCelestialDistance;
                }
                else if (isInitialised && celestialsCamera1 != null)
                {
                    // Planet (A) Camera (B). Direction = (B-A).normalized
                    Vector3 _lookVector = celestialsCamera1.transform.position - celestial.celestialTfrm.position;
                    celestial.currentCelestialDistance = _lookVector.magnitude - minCelestialCameraDistance;
                    return celestial.currentCelestialDistance;
                }
                else if (isCamera2Initialised && celestialsCamera2 != null)
                {
                    // Planet (A) Camera (B). Direction = (B-A).normalized
                    Vector3 _lookVector = celestialsCamera2.transform.position - celestial.celestialTfrm.position;
                    celestial.currentCelestialDistance = _lookVector.magnitude - minCelestialCameraDistance;
                    return celestial.currentCelestialDistance;
                }
                else { return 0f; }
            }
            else { return 0f; }
        }

        /// <summary>
        /// Hide the planet baesd on its zero-based position in the list
        /// </summary>
        /// <param name="planetIndex"></param>
        public void HidePlanet (int planetIndex)
        {
            SSCCelestial planet = GetCelestialByIndex(planetIndex);

            if (planet != null && planet.CelestialTransform != null)
            {
                planet.CelestialTransform.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// If the stars have already been created, attempt to hide them.
        /// </summary>
        public void HideStars()
        {
            if (starMRenderer != null)
            {
                starMRenderer.enabled = false;
            }
        }

        /// <summary>
        /// Initialise the second camera
        /// </summary>
        public void InitialiseCamera2()
        {
            if (isInitialised)
            {
                // Only add celestials camera if it doesn't already exist
                Transform celestialCamera2Trm = GetorCreateCamera("Celestials Camera 2", out celestialsCamera2);

                if (celestialCamera2Trm != null && celestialsCamera2 != null)
                {
                    ConfigCameras(celestialsCamera2, camera2, -102f);
                    isCamera2Initialised = true;
                }
            }
        }

        /// <summary>
        /// Call this if you have changed any of the settings on the camera1 and/or camera2
        /// </summary>
        public void RefreshCameras()
        {
            if (IsInitialised)
            {
                ConfigCameras(celestialsCamera1, camera1, -100f);
                ConfigCameras(celestialsCamera2, camera2, -102f);
            }
        }

        /// <summary>
        /// Call this when changing the number of stars and/or planets.
        /// </summary>
        public void RefreshCelestials()
        {
            if (isInitialised && IsVerifyStarSettings())
            {
                BuildCelestials();
            }
        }

        /// <summary>
        /// Set the relative distance the celestial object (planet) is from the celestials camera(s).
        /// </summary>
        /// <param name="celestial"></param>
        /// <param name="relativeDistance">Value must be between 0.0 and 1.0</param>
        public void SetCelestialDistance (SSCCelestial celestial, float relativeDistance)
        {
            if (relativeDistance >= 0f && relativeDistance <= 1f && celestial != null && !celestial.isHidden)
            {
                celestial.currentCelestialDistance = relativeDistance;

                Vector3 celestialPos = CalcCelestialPosition(celestial);

                celestial.celestialTfrm.position = celestialPos;
            }
        }

        /// <summary>
        /// Set the number of stars. After calling this, call RefreshCelestials()
        /// for it to take effect.
        /// </summary>
        /// <param name="newNumberOfStars"></param>
        public void SetNumberOfStars (int newNumberOfStars)
        {
            if (numberOfStars > 0)
            {
                numberOfStars = newNumberOfStars;
            }
        }

        /// <summary>
        /// Show the planet baesd on its zero-based position in the list
        /// </summary>
        /// <param name="planetIndex"></param>
        public void ShowPlanet (int planetIndex)
        {
            SSCCelestial planet = GetCelestialByIndex(planetIndex);

            if (planet != null && planet.CelestialTransform != null)
            {
                planet.CelestialTransform.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// If the stars have already been created, but then hidden,
        /// attempt to show them.
        /// </summary>
        public void ShowStars()
        {
            if (starMRenderer != null)
            {
                starMRenderer.enabled = true;
            }
        }

        #endregion
    }
}