using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if SCSM_S3D
using scsmmedia;
#endif

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Setup for Sticky3D Controller integration:
    /// 1. Add Sticky3DController\Demos\Prefabs\Characters\NPC_Bob into the scene under "Characters"
    /// 2. Add Sticky3DController\Demos\Prefabs\Characters\PlayerJaneSuited into the scene under "Characters"
    /// 3. Add Sticky3DController\Demos\Prefabs\ThirdPersonCamera into the scene under "Characters"
    /// 4. Set the PlayerJaneSuited transform position to -66, 28, -218
    /// 5. Set the NPC_Bob transform position to 10, 10, 10
    /// 6. Add the NPC_Bob from the scene to Services Assistant slot
    /// 7. Add the PlayerJaneSuited from the scene to Pilot Controller slot
    /// 8. Add Sticky3DController\Demos\AnimClipSets\Sticky3D Demo Sit Set1 to the Player Shuttle Anim Clip Set slot
    /// 9. Add Sticky3DController\Demos\AnimClipSets\Sticky3D Demo Sit Set5 to the Assistant Anim Clip Set slot
    /// 10. Add Sticky3DController\Demos\Models\Characters\s3d_bob\s3d_bob_interact to Default Interact Animation
    /// 11. Add Sticky3DController\Demos\Animations\s3d_jane_gestures\s3d_jane_wave5 to Services NPC Wave Animation
    /// 12. On the PlayerJaneSuited in the scene, on the Look tab, add the ThirdPersonCamera from "Characters" to the "Look Camera"
    /// 13. On the PlayerJaneSuited in the scene, on the StickyPartsModule, untick "Initialise On Start"
    /// 14. On the NPC_Bob in the scene, on the Collide tab, set Reference Update Type to "Auto First"
    /// 15. On this component, turn off "Use Walk Thru"
    /// 16. Optionally Save Scene as "techdemo3scene2" to avoid it being overwritten on the next SSC update
    /// WARNING: Currently uses Sticky3D when changing cameras for Celestials (stars)
    /// </summary>
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class TechDemo3 : MonoBehaviour
    {
        #region Enumerations
        public enum GameQuality
        {
            Low = 0,
            Medium = 1,
            High = 2
        }

        #endregion

        #region Public Variables

        [Header("General Settings")]
        public Celestials celestials = null;
        [Tooltip("The chair in the services room")]
        public GameObject assistantChair = null;
        [Tooltip("Ladder for getting into Hawk cockpit")]
        public GameObject ladder1 = null;
        [Tooltip("Enable the walk through camera. On by default when Sticky3D is not installed.")]
        public bool useWalkThru = false;
        [Tooltip("Main directional light")]
        public Light mainLight = null;
        
        public GameQuality gameQuality = GameQuality.High;
        public bool limitScreenToHD = true;
        public bool limitFrameRate = true;

        [Header("Space Port Lighting")]
        public Light entranceLight1 = null;
        public Light entranceLight2 = null;
        public Light servicesLight1 = null;
        public Light servicesLight2 = null;
        public Light bottomLiftLight1 = null;
        public Light corridorLeftLight1 = null;
        public Light corridorRightLight1 = null;
        public Light topLift1Light1 = null;
        public Light topLift1Light2 = null;

        [Header("Player Shuttle Ship")]
        public ShipControlModule playerShuttle = null;
        public ShipCameraModule shipCamera = null;
        public GameObject shuttleSideDoorControl = null;
        [Tooltip("The chair in the shuttle where the character starts")]
        public GameObject shuttlePilotChair = null;
        [Tooltip("The flashing light on the console")]
        public GameObject dockingButton = null;
        public Light shuttleCockpitLight = null;
        public Light shuttleCargoBayLight = null;

        [Header("Player Hawk Ship")]
        public ShipControlModule playerHawk = null;
        public GameObject hawkStaticColliders = null;
        public MeshCollider hawkMeshCollider = null;
        public BoxCollider hawkCanopyCollider = null;
        public SSCDoorAnimator hawkCanopyDoor = null;
        public SSCDoorProximity hawkCanopyDoorProximity = null;
        public Light hawkCockpitLight = null;
        [Tooltip("Always show the full Hawk HUD, even on Low Quality")]
        public bool hawkFullHUD = true;

        [Header("Sound FX")]
        public EffectsModule soundPoolPrefab = null;
        public AudioClip dockingSoundClip = null;
        public AudioClip attackExplosion = null;
        public AudioClip shuttleAmbientClip = null;
        public AudioClip spacePortAmbientClip = null;

        [Header("Heads Up Display")]
        public ShipDisplayModule shipDisplayModule = null;
        [Range(0f, 1f)] public float cockpitViewHeight = 0.75f;

        [Header("Attack Items - General")]
        public BeaconLight beaconLight = null;
        public SSCLightStrobe liftStrobeLight1 = null;
        public SSCLightStrobe liftStrobeLight2 = null;
        public Vector3 battleCentralPos = Vector3.up * 120f;
        public GameObject shipsParentGameObject = null;

        [Header("Attack Items - Friendly")]
        public ShipControlModule friendlyShipPrefab = null;
        public int numFriendlyShip = 5;
        public float friendlyShipSpawnDist = 25f;
        public int numFriendlyRespawns = 3;
        [Range(0f, 1f)] public float friendlyAccuracy = 0.75f;
        private int friendlyFactionID = 1;
        private int playerSquadronID = 1;
        private int friendlySquadronID = 2;
        private int barrelsSquadronID = 3;

        [Header("Attack Items - Enemy")]
        public ShipControlModule enemyShip1Prefab = null;
        public int numEnemyShip1 = 5;
        public ShipControlModule enemyShip2Prefab = null;
        public int numEnemyShip2 = 5;
        public float enemyShipSpawnDist = 500f;
        public ShipControlModule enemyCapitalShipPrefab = null;
        public Vector3[] capitalShipSpawnPoints = null;
        public Vector3[] capitalShipSpawnRots = null;
        public EffectsModule sparksEffectsObjectPrefab = null;
        public Vector3[] sparkFXPoints = null;
        public int numEnemyRespawns = 3;
        [Range(0f, 1f)] public float enemyAccuracy = 0.5f;
        private int enemyFactionID = 2;
        private int enemySquadron1ID = 4;
        private int enemySquadron2ID = 5;
        private int enemyCapitalSquadronID = 6;

        [Header("Attack Items - AI")]
        public string friendlyIdlePathName = "Path Name Here";
        public float friendlyIdleSpeed = 50f;
        public string enemyIdlePathName = "Path Name Here";
        public float enemyIdleSpeed = 50f;
        public List<string> friendlyEscapePathNames = new List<string>();
        public List<string> enemyEscapePathNames = new List<string>();
        public float pursuitSpeed = 100f;
        public float strafingRunSpeed = 100f;

        [Header("SpacePort Doors")]
        public SSCDoorAnimator lift1BottomDoors = null;
        public SSCDoorAnimator lift1OuterDoors = null;
        public SSCDoorAnimator lift2BottomDoors = null;
        public GameObject lift1DoorControl = null;
        public SSCDoorAnimator outerDockingBayDoor1 = null;
        public SSCDoorProximity outerDockingBayDoorProximity = null;
        public SSCDoorAnimator[] delayedUnlockDoors = null;

        [Header("SpacePort Lift")]
        public GameObject lift1Control = null;
        public SSCMovingPlatform lift1MovingPlatform = null;
        public Light lift1CallLight = null;
        public GameObject lift1SafetyCabinet = null;
        public DemoSSCCycleMaterials lift1Barrier = null;

        [Header("Menu")]
        public GameObject menuPanel = null;
        public GameObject qualityPanel = null;
        public Button resumeButton = null;
        public Button startButton = null;
        public Button restartButton = null;
        public Button quitButton = null;
        public Button qualityLowButton = null;
        public Button qualityMedButton = null;
        public Button qualityHighButton = null;
        public Text resumeButtonText = null;
        public Text startButtonText = null;
        public Text restartButtonText = null;
        public Text quitButtonText = null;
        public Text qualityLowButtonText = null;
        public Text qualityMedButtonText = null;
        public Text qualityHighButtonText = null;
        public Image qualityLowBorderImg = null;
        public Image qualityMedBorderImg = null;
        public Image qualityHighBorderImg = null;
        public GameObject missionOverPanel = null;
        public GameObject missionSuccessTitle = null;
        public GameObject missionSuccessSubTitle = null;
        public GameObject missionFailedTitle = null;
        public GameObject missionFailedSubTitle = null;

        [Header("Walk Thru")]
        [Tooltip("The camera used when Sticky3D Controller is not installed")]
        public Camera walkThruCamera1 = null;
        public ShipCameraModule walkThruCamera2 = null;
        public ShipControlModule cameraShip = null;
        public string walkThruPath1Name = "Shuttle Fly Thru Path";
        public string walkThruPath2Name = "Space port Fly Thru Entrance";
        public string walkThruPath3Name = "Space port Fly Thru Services";
        public string walkThruPath4Name = "Space port Fly Thru Call Lift";
        public string walkThruPath5Name = "Space port Fly Thru Enter Lift";
        public string walkThruPath6Name = "Space port Fly Thru Top Lift";
        public SSCProximity cockPitEntryProximity = null;

        [Header("Sticky3D Controller Setup")]
        [Tooltip("Used when Sticky3D Controller is in project to modify character behaviour")]
        public GameObject[] stickyZones = null;
        [Tooltip("Used when Sticky3D Controller is in project. To assign to stickyZones.")]
        public Transform[] stickyZoneRefFrames = null;
        [Tooltip("Used when Sticky3D Controller is in project. Assign the Demo Sit Set5 here")]
        public ScriptableObject playerShuttleAnimClipSet = null;
        [Tooltip("Used when Sticky3D Controller is in project. Assign the Demo Sit Set5 here")]
        public ScriptableObject assistantAnimClipSet = null;
        [Tooltip("Used when Sticky3D Controller is in project, this is the Bob Interact animation")]
        public AnimationClip defaultInteractAnimation = null;
        [Tooltip("Used when Sticky3D Controller is in project, this is the animation used to attract the player")]
        public AnimationClip servicesNPCWaveAnimation = null;
        #endregion

        #region Public Properties
        public bool isGamePaused { get; private set; }
        #endregion

        #region Character Controller variables
        [Header("Sticky3D Characters")]
        [Tooltip("Used when Sticky3D Controller is in project add PlayerJaneSuited character here")]
        public GameObject pilotController = null;

        [Tooltip("Used when Sticky3D Controller is in project add the NPC_Rod character here")]
        public GameObject servicesAssistant = null;

        #endregion

        #region Private Variables - General

        private bool isInitialised = false;
        private bool attackStarted = false;
        private bool bombingRunsStarted = false;
        private bool isUpdateHawkHUDMetrics = false;
        private bool isInHawkCockpit = false;
        private SSCDoorAnimator sscShuttleDoorAnimator = null;
        private int soundeffectsObjectPrefabID = -1;
        private int sparksEffectsObjectPrefabID = -1;
        private float sparksTimer = 0f;
        private float sparksInterval = 5f;
        private int numSparkFXPoints = 0;
        private Weapon playerHawkLMissiles = null;
        private Weapon playerHawkRMissiles = null;
        private SSCRandom sparksRandom = null;
        private SSCManager sscManager = null;
        private SSCRadar sscRadar = null;
        private DisplayGauge dockedIndicator = null;
        private DisplayGauge fuelLevelGauge = null;
        private DisplayGauge heatLevelGauge = null;
        private DisplayGauge healthGauge = null;
        private DisplayGauge missilesGauge = null;
        private DisplayGauge shieldsGauge = null;
        private DisplayGauge launchGauge = null;
        private DisplayGauge gearGauge = null;
        private DisplayGauge enemyGauge = null;
        private DisplayMessage startDockingMessage = null;
        private DisplayMessage dockingInProgressMessage = null;
        private DisplayMessage exitShuttleMessage = null;
        private DisplayMessage findServiceDeskMessage = null;
        private DisplayMessage getToDeckMessage = null;
        private DisplayMessage getToHawkMessage = null;
        private DisplayMessage getToHawkMessageWithHelmet = null;
        private PlayerInputModule shuttlePlayerInputModule = null;
        private PlayerInputModule hawkPlayerInputModule = null;
        private AutoTargetingModule hawkPlayerAutoTargetingModule = null;
        private ShipDocking shipDocking = null;
        private SampleChangeCameraView changeShipCameraView = null;

        private LocationData hawkLandingLocation = null;

        private SSCDoorControl sscDoorControlShuttleSideDoors = null;
        private SSCDoorControl sscDoorControlLift1Doors = null;

        private DemoSSCChangeMaterial lift1ControlChangeMaterial = null;

        private List<SSCRadarBlip> sscRadarBlipsList = null;
        private int sscRadarBlipsListCount = 0;
        private SSCRadarQuery friendlyRadarQuery = null;
        private SSCRadarQuery enemyRadarQuery = null;

        private List<ShipAIInputModule> friendlyShips = null;
        private List<ShipAIInputModule> enemyType1Ships = null;
        private List<ShipAIInputModule> enemyType2Ships = null;
        private List<ShipAIInputModule> enemyCapitalShips = null;

        /// <summary>
        /// List of available friendly ShipIds
        /// </summary>
        private List<int> availableFriendlyShips = null;
        /// <summary>
        /// List of available enemy ShipIds 
        /// </summary>
        private List<int> availableEnemyShips = null;

        private int numEnemyRemaining = 0;
        private int numEnemyCapitalShips = 0;

        private SSCRandom aiRandom = null;

        private PathData friendlyIdlePath;
        private PathData enemyIdlePath;
        private List<PathData> friendlyEscapePaths = null;
        private List<PathData> enemyEscapePaths = null;

        // A list of ships that are currently paused
        private List<ShipAIInputModule> pausedAIShips;

        // Use to calculate new position of walk thru path in shuttle
        private Vector3 shuttleOriginalPos = Vector3.zero;
        private Quaternion shuttleOriginalRot = Quaternion.identity;

        // UI variables
        private EventSystem eventSystem;
        private Color colourNonSelectedBtnText;
        private Color colourSelectedBtnText;
        private Color colourDefaultBorder;
        private Color colourSelectedBorder;

        private AudioSource ambientAudioSource = null;
        private scsmmedia.MusicController musicController = null;

        private WaitForSeconds fadeUpLightWait = null;
        private System.Type hdLightType = null;

        private bool isURP = false;
        private bool isHDRP = false;

        #endregion

        #region Private Variables - Walk Thru
        private int walkThruSectionNumber = 0;
        private ShipAIInputModule cameraShipAI = null;
        private bool isWalkThruInitialised = false;
        private PathData walkThruPath1 = null;
        private PathData walkThruPath2 = null;
        private PathData walkThruPath3 = null;
        private PathData walkThruPath4 = null;
        private PathData walkThruPath5 = null;
        private PathData walkThruPath6 = null;
        #endregion

        #region Private Sticky3D Controller variables (if present)
        #if SCSM_S3D
        private StickyControlModule s3dPlayer = null;
        private StickyInputModule s3dPlayerInput = null;
        private int s3dPlayerInteractStateId = 0;
        private SampleSitAction sampleSitActionPlayer = null;
        private StickyControlModule s3dServiceAssistant = null;
        private int servicesAssistantInteractStateId = 0;
        private StickyInteractive shuttleSideDoorControlInteractive = null;
        private StickyInteractive shuttleDockingButtonInteractive = null;
        private StickyInteractive lift1DoorControlInteractive = null;
        private StickyInteractive lift1ControlInteractive = null;
        private StickyInteractive lift1SafetyCabinetInteractive = null;
        private StickyPartsModule s3dPlayerParts = null;
        private CustomInput s3dciMouseClick = null;
        private CustomInput s3dciSitting = null;
        private CustomInput s3dciPauseGame = null;
        private Color32 defaultReticleColour;
        private Color32 lookingAtReticleColour;
        private bool isPlayerEnabledOnPause = false;
        #endif
        #endregion

        #region Initialisation Methods

        // Start is called before the first frame update
        void Start()
        {
            #region Check for key components

            ambientAudioSource = GetComponent<AudioSource>();

            if (playerShuttle == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: TechDemo3 could not find shuttle player ship");
                #endif
            }
            else if (playerHawk == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: TechDemo3 could not find hawk player ship");
                #endif
            }
            else if (shipCamera == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: TechDemo3 could not find ship player camera");
                #endif
            }
            else if (ambientAudioSource == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: TechDemo3 could not find AudioSource component on " + gameObject.name);
                #endif
            }
            else
            #endregion

            {
                isGamePaused = false;

                if (limitScreenToHD) { SSCUtils.MaxScreenHD(); }

                isURP = SSCUtils.IsURP(false);

                if (!isURP) { isHDRP = SSCUtils.IsHDRP(false); }

                ConfigurePlayerShips();

                // Get or create the SSC manager component
                sscManager = SSCManager.GetOrCreateManager();

                #region Configure Radar

                sscRadar = SSCRadar.GetOrCreateRadar();

                // Initialise a list of radar results
                sscRadarBlipsList = new List<SSCRadarBlip>();

                // Create a landing location for the player ship, and make it appear
                // on Hawk radar HUD to help orientate the player.
                hawkLandingLocation = new LocationData()
                {
                    name = "PlayerLandingLocation",
                    factionId = 0,
                    position = playerHawk != null ? playerHawk.transform.position : Vector3.zero,
                    isRadarEnabled = true,
                    radarBlipSize = 5,
                    showGizmosInSceneView = true
                };

                if (hawkLandingLocation != null)
                {
                    sscManager.AddLocation(hawkLandingLocation);
                }

                // The Radar UI mini-map which appears when the pilot is in the Hawk,
                // will query the area around the Hawk as it is flown. This will also
                // determine who appears as friend, foe or neutral on the UI mini-map.
                if (playerHawk != null)
                {
                    sscRadar.FollowShip(playerHawk);
                }

                #endregion

                #region Initialise Paths

                friendlyIdlePath = sscManager.GetPath(friendlyIdlePathName);
                enemyIdlePath = sscManager.GetPath(enemyIdlePathName);

                if (friendlyEscapePathNames != null && friendlyEscapePathNames.Count > 0)
                {
                    int numFriendlyEscapePaths = friendlyEscapePathNames.Count;
                    friendlyEscapePaths = new List<PathData>(numFriendlyEscapePaths);
                    for (int i = 0; i < numFriendlyEscapePaths; i++)
                    {
                        friendlyEscapePaths.Add(sscManager.GetPath(friendlyEscapePathNames[i]));
                    }
                }
                else
                {
                    Debug.LogWarning("ERROR: TechDemo3 no friendly escape paths defined");
                }

                if (enemyEscapePathNames != null && enemyEscapePathNames.Count > 0)
                {
                    int numEnemyEscapePaths = enemyEscapePathNames.Count;
                    enemyEscapePaths = new List<PathData>(numEnemyEscapePaths);
                    for (int i = 0; i < numEnemyEscapePaths; i++)
                    {
                        enemyEscapePaths.Add(sscManager.GetPath(enemyEscapePathNames[i]));
                    }
                }
                else
                {
                    Debug.LogWarning("ERROR: TechDemo3 no enemy escape paths defined");
                }

                #endregion

                // Initialise all ships
                InitialiseAttackShips();

                // Setup custom scene-specific FX.
                ConfigureEffectsPools();

                ConfigureHUD();

                sscShuttleDoorAnimator = playerShuttle.GetComponentInChildren<SSCDoorAnimator>();

                #if UNITY_EDITOR
                if (sscShuttleDoorAnimator == null)
                {
                    Debug.LogWarning("ERROR: TechDemo3 could not find SSCDoorAnimator component in Shuttle");
                }
                #endif

                // Some doors may have a SSC Door Proximity component. If the player is inside the proximity when
                // the game starts the trigger enter and exit events will fire. For these doors, the SSC Door Animator
                // component should have the doors locked, but the Proximity component should not automatically unlock them.
                // Instead, we'll unlock these doors after the scene starts.
                if (delayedUnlockDoors != null)
                {
                    // On slower computers we need to give enough time for all the Awake methods
                    // in the scene to be called.
                    Invoke("DelayedUnlockDoors", 1.0f);
                }

                #if SCSM_S3D
                if (useWalkThru)
                {
                    ConfigureNoStickyController();
                    ConfigureServicesAssistantNPC();
                }
                else
                {
                    ConfigureStick3DCharacters();
                    ConfigureStickyInteractive();
                    // Give the characters a chance to initialise over a few frames and become grounded,
                    // set their reference frame etc.
                    Invoke("InitialiseStickyZones", 0.1f);
                }
                
                if (sscShuttleDoorAnimator != null) { sscShuttleDoorAnimator.LockDoors(); }

                #else
                useWalkThru = true;
                ConfigureNoStickyController();
                #endif

                isInitialised = shuttlePlayerInputModule != null && shipDisplayModule != null && sscManager != null && shipDocking != null;

                UpdateGauges();

                SetGameQuality();

                InitialiseMenu();

                if (celestials != null)
                {
                    // Refresh the celestials cameras after the scene first renders
                    Invoke("SetSceneCamera", 0.01f);
                }

                System.GC.Collect();

                ShowMenu(false);

                // When Sticky3D player is sitting in the shuttle,
                // give it some time to sit down.
                Invoke("ShowStart", useWalkThru ? 0.25f : 1.5f);

                #region Music Controller Initialisation
                musicController = GetComponent<scsmmedia.MusicController>();
                if (musicController != null) { musicController.Initialise(); }
                #endregion
            }
        }

        /// <summary>
        /// Check panels and buttons are configured in the menu
        /// </summary>
        private void InitialiseMenu()
        {
            #region Initialise Menu
            #if UNITY_EDITOR
            if (menuPanel == null) { Debug.LogWarning("ERROR: TechDemo3UI menuPanel is not configured"); }
            if (qualityPanel == null) { Debug.LogWarning("ERROR: TechDemo3UI qualityPanel is not configured"); }
            if (resumeButton == null) { Debug.LogWarning("ERROR: TechDemo3resumeButton is not configured"); }
            if (startButton == null) { Debug.LogWarning("ERROR: TechDemo3startButton is not configured"); }
            if (restartButton == null) { Debug.LogWarning("ERROR: TechDemo3restartButton is not configured"); }
            if (quitButton == null) { Debug.LogWarning("ERROR: TechDemo3quitButton is not configured"); }
            if (qualityLowButton == null) { Debug.LogWarning("ERROR: TechDemo3qualityLowButton is not configured"); }
            if (qualityMedButton == null) { Debug.LogWarning("ERROR: TechDemo3qualityMedButton is not configured"); }
            if (qualityHighButton == null) { Debug.LogWarning("ERROR: TechDemo3qualityHighButton is not configured"); }
            if (resumeButtonText == null) { Debug.LogWarning("ERROR: TechDemo3resumeButtonText is not configured"); }
            if (startButtonText == null) { Debug.LogWarning("ERROR: TechDemo3startButtonText is not configured"); }
            if (restartButtonText == null) { Debug.LogWarning("ERROR: TechDemo3restartButtonText is not configured"); }
            if (quitButtonText == null) { Debug.LogWarning("ERROR: TechDemo3quitButtonText is not configured"); }
            if (qualityLowButtonText == null) { Debug.LogWarning("ERROR: TechDemo3qualityLowButtonText is not configured"); }
            if (qualityMedButtonText == null) { Debug.LogWarning("ERROR: TechDemo3qualityMedButtonText is not configured"); }
            if (qualityHighButtonText == null) { Debug.LogWarning("ERROR: TechDemo3qualityHighButtonText is not configured"); }
            if (qualityLowBorderImg == null) { Debug.LogWarning("ERROR: TechDemo3qualityLowBorderImg is not configured"); }
            if (qualityMedBorderImg == null) { Debug.LogWarning("ERROR: TechDemo3qualityMedBorderImg is not configured"); }
            if (qualityHighBorderImg == null) { Debug.LogWarning("ERROR: TechDemo3qualityHighBorderImg is not configured"); }
            if (missionOverPanel == null) { Debug.LogWarning("ERROR: TechDemo3missionOverPanel is not configured"); }
            if (missionSuccessTitle == null) { Debug.LogWarning("ERROR: TechDemo3missionSuccessTitle is not configured"); }
            if (missionSuccessSubTitle == null) { Debug.LogWarning("ERROR: TechDemo3missionSuccessSubTitle is not configured"); }
            if (missionFailedTitle == null) { Debug.LogWarning("ERROR: TechDemo3 missionFailedTitle is not configured"); }
            if (missionFailedSubTitle == null) { Debug.LogWarning("ERROR: TechDemo3 missionFailedSubTitle is not configured"); }
            #endif

            eventSystem = EventSystem.current;

            colourNonSelectedBtnText = new Color(245f / 255f, 245f / 255f, 245f / 255f, 1f);
            colourSelectedBtnText = new Color(168f / 255f, 168f / 255f, 227f / 255f, 1f);
            colourDefaultBorder = new Color(72f / 255f, 72f / 255f, 188f / 255f, 40f/255f);

            // White with the same alpha a original border colour
            colourSelectedBorder = new Color(1f, 1f, 1f, 40f / 255f);

            #endregion
        }

        #endregion

        #region Update Methods
       
        // Update is called once per frame
        void Update()
        {
            if (isInitialised && !isGamePaused)
            {
                #region Update Space Port FX
                // Show sparks around the space port when the attack has started but not when the play is flying the Hawk.
                if (attackStarted && !isInHawkCockpit && sparksEffectsObjectPrefabID >= 0 && numSparkFXPoints > 0)
                {
                    sparksTimer += Time.deltaTime;
                    if (sparksTimer > sparksInterval)
                    {
                        InstantiateEffectsObjectParameters ieParms = new InstantiateEffectsObjectParameters
                        {
                            effectsObjectPrefabID = sparksEffectsObjectPrefabID,
                            position = sparkFXPoints[sparksRandom.Range(0, numSparkFXPoints-1)]
                        };
                        sscManager.InstantiateEffectsObject(ref ieParms);

                        sparksTimer = 0f;
                    }
                }
                #endregion

                // This only occurs when game quality is medium or high and player is in the Hawk
                if (isUpdateHawkHUDMetrics)
                {
                    UpdateGauges();
                }

                #region Update Walk Thru
                if (isWalkThruInitialised)
                {
                    if (walkThruSectionNumber == 1)
                    {
                        WalkThruUpdateSection1();
                    }
                    else if (walkThruSectionNumber == 8)
                    {
                        WalkThruUpdateSection8();
                    }
                    else if (walkThruSectionNumber == 10)
                    {
                        WalkThruUpdateSection10();
                    }
                }
                #endregion

                #region Check if we need to update Capital Ship shields
                if (attackStarted && isInHawkCockpit)
                {
                    CheckCapitalShipHealth();
                }
                #endregion
            }
        }

        #endregion

        #region Events

        private void OnDestroy()
        {
            #if SCSM_S3D
            
            // Remove listeners
            if (lift1DoorControlInteractive != null)
            {
                lift1DoorControlInteractive.RemoveListeners();
            }

            if (lift1ControlInteractive != null)
            {
                lift1ControlInteractive.RemoveListeners();
            }

            if (shuttleSideDoorControlInteractive != null)
            {
                shuttleSideDoorControlInteractive.RemoveListeners();
            }

            if (lift1SafetyCabinetInteractive != null)
            {
                lift1SafetyCabinetInteractive.RemoveListeners();
            }

            #endif
        }

        #endregion

        #region Private Methods - Sticky3D Controller

        #if SCSM_S3D
        private void CameraChanged(int stickyID, Camera oldCamera, Camera newCamera, bool isThirdPersonCamera)
        {
            //Debug.Log("[DEBUG] camera changed. From: " + oldCamera.name + " to " + newCamera.name + ". Is third person? " + isThirdPersonCamera);

            if (celestials != null)
            {
                celestials.camera1 = newCamera;
                celestials.RefreshCameras();
            }
        }

        /// <summary>
        /// If Sticky3D Controller is in the project, add the StickyZones at runtime to avoid having to include
        /// S3D in the SSC TechDemo3.
        /// WARNING: The string name compares probably incur GC.
        /// </summary>
        private void InitialiseStickyZones()
        {
            int numS3DZones = stickyZones == null ? 0 : stickyZones.Length;
            int numS3DRefFrames = stickyZoneRefFrames == null ? 0 : stickyZoneRefFrames.Length;

            if (numS3DZones != numS3DRefFrames)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: TechDemo3 the number of sticky zone reference frames (" + numS3DRefFrames + ") must match the number of sticky zones (" + numS3DZones + ").");
                #endif
            }
            else
            {
                for (int znIdx = 0; znIdx < numS3DZones; znIdx++)
                {
                    GameObject go = stickyZones[znIdx];
                    if (go != null)
                    {
                        StickyZone stickyZone = go.GetComponent<StickyZone>();
                        if (stickyZone == null) { stickyZone = go.AddComponent<StickyZone>(); }

                        if (stickyZone != null)
                        {
                            // Default settings for zones in Tech Demo3
                            stickyZone.referenceTransform = stickyZoneRefFrames[znIdx];
                            stickyZone.overrideReferenceFrame = true;
                            stickyZone.isRestoreDefaultRefTransformOnExit = false;
                            stickyZone.isRestorePreviousRefTransformOnExit = false;
                            stickyZone.overrideLookFirstPerson = false;
                            stickyZone.overrideGravity = false;
                            stickyZone.overrideAnimClips = false;
                            stickyZone.initialiseOnStart = true;

                            if (go.name.Equals("StickyZonePortA") || go.name.Equals("ShuttleDoorStickyZone"))
                            {
                                stickyZone.overrideLookThirdPerson = true;
                            }
                            else if (go.name.Contains("Lift"))
                            {
                                // When the S3D character steps off the lift, restore the previous reference transform
                                stickyZone.isRestorePreviousRefTransformOnExit = true;
                            }
                            else if (go.name.Equals("StickyZoneServicesChairBob"))
                            {
                                // Configure the zone to replace the default sit animations for Bob.
                                stickyZone.overrideAnimClips = true;
                                stickyZone.isRestorePreviousAnimClipsOnExit = true;
                                stickyZone.overrideReferenceFrame = false;
                                if (servicesAssistant != null)
                                {
                                    s3dServiceAssistant = servicesAssistant.GetComponent<StickyControlModule>();
                                    if (s3dServiceAssistant != null && assistantAnimClipSet != null)
                                    {
                                        // This only applies to assistant Bob
                                        stickyZone.SetModelsFilter(new int[] { s3dServiceAssistant.modelId });

                                        stickyZone.AddAnimClipSet((S3DAnimClipSet)assistantAnimClipSet);
                                    }
                                    #if UNITY_EDITOR
                                    else
                                    {
                                        Debug.LogWarning("[ERROR] TechDemo3 - could not configure or find Assistant Anim Clip Set");
                                    }
                                    #endif
                                }
                            }
                            else if (go.name.Equals("StickyZoneShuttlePilotChair"))
                            {
                                // Configure the zone to replace the default sit animations for Player Jane Suited.
                                stickyZone.overrideAnimClips = true;
                                stickyZone.isRestorePreviousAnimClipsOnExit = true;
                                stickyZone.overrideReferenceFrame = false;
                                if (s3dPlayer != null && playerShuttleAnimClipSet != null)
                                {
                                    // This should only apply to the player character
                                    stickyZone.SetModelsFilter(new int[] { s3dPlayer.modelId });

                                    stickyZone.AddAnimClipSet((S3DAnimClipSet)playerShuttleAnimClipSet);
                                }
                                #if UNITY_EDITOR
                                else
                                {
                                    Debug.LogWarning("[ERROR] TechDemo3 - could not configure or find Player Shuttle Anim Clip Set");
                                }
                                #endif
                            }

                            // We start the game with some of the zone gameobjects disabled so that characters that
                            // are inside the trigger collider zones don't pre-maturely trigger a OnTriggerEnter
                            // event before the zones are initialised.
                            if (!stickyZone.gameObject.activeSelf) { stickyZone.gameObject.SetActive(true); }

                            if (!stickyZone.IsInitialised) { stickyZone.Initialise(); }
                        }
                    }
                }

                // Force garbage collection so that we don't get an unexpect GC event
                // while the game is running.
                System.GC.Collect();
            }
        }

        /// <summary>
        /// If Sticky3D Controller is in the project, configure the characters.
        /// The majority of these things could be manually configured in the Unity editor,
        /// however, as we're using default prefabs that come with Sticky3D Controller, we
        /// need to ensure things will work correctly with Tech Demo 3 in Sci-Fi Ship Controller.
        /// It also demonstrates how to do things in code.
        /// </summary>
        private void ConfigureStick3DCharacters()
        {
            #region Main Player
            if (pilotController != null)
            {
                // Ensure the walk through camera is disabled
                ConfigureWalkThrough(false);

                // If Sticky3D Controller asset is installed, check if the pilot is a S3D character
                s3dPlayer = pilotController.GetComponent<StickyControlModule>();
                if (s3dPlayer != null)
                {
                    // The player was not placed correctly in the shuttle and didn't get a reference frame
                    if (!s3dPlayer.IsInitialised)
                    {
                        pilotController.transform.position = playerShuttle.transform.position + (playerShuttle.transform.up * 0.5f);
                        s3dPlayer.Initialise();
                    }

                    #region Parts Setup
                    // At start, turn off helmet, visor, and jetpack
                    s3dPlayerParts = pilotController.GetComponent<StickyPartsModule>();
                    if (s3dPlayerParts != null)
                    {
                        s3dPlayerParts.Initialise();
                        s3dPlayerParts.DisableAllParts();
                        s3dPlayer.DisableJetPackAvailability();
                    }
                    #endregion

                    #region Camera and Look Setup
                    // Help avoid the camera clipping through walls etc.
                    s3dPlayer.clipObjects = true;
                    s3dPlayer.clipMinDistance = 0.7f;
                    s3dPlayer.clipResponsiveness = 0.95f;

                    s3dPlayer.callbackOnCameraChange = CameraChanged;

                    if (celestials) { celestials.camera1 = s3dPlayer.lookCamera1; }

                    // Restrict camera vertical movement when in shuttle pilot seat 
                    s3dPlayer.lookPitchUpLimit = 25f;
                    s3dPlayer.lookPitchDownLimit = 35f;

                    // turn off auto hide on S3D as we'll control this with the SSC HUD.
                    s3dPlayer.lookAutoHideCursor = false;

                    // The prefab may have a modified nearClipPlane. See set to work with this scene.
                    s3dPlayer.lookFirstPersonCamera1.nearClipPlane = 0.3f;

                    #endregion

                    #region Climbing Setup
                    // Set the climb speed to match the s3d_jane_suited_climbtop3 animation
                    // which is designed to work with the SSCLadder1.
                    s3dPlayer.climbSpeed = 0.75f;
                    s3dPlayer.climbTopDetection = true;
                    #endregion

                    #region Head and Foot IK Setup

                    s3dPlayer.headIKMoveDamping = 0.1f;

                    s3dPlayer.EnableHeadIK(true);

                    // turn off foot ik for now
                    s3dPlayer.DisableFootIK();

                    #endregion

                    #region Setup the SSC HUD for use with the Sticky character.
                    // We could use the StickyDisplayModule instead. However, we'll use the
                    // SSC HUD as we want to use it later in the Hawk ship for targeting.
                    if (shipDisplayModule != null)
                    {
                        shipDisplayModule.lockDisplayReticleToCursor = true;

                        int guidHashReticle = shipDisplayModule.GetDisplayReticleGuidHash(1);
                        shipDisplayModule.ChangeDisplayReticle(guidHashReticle);

                        // Remmeber the colour that was set in the editor
                        defaultReticleColour = shipDisplayModule.activeDisplayReticleColour;

                        // Set the look at colour to green
                        lookingAtReticleColour = new Color32(0, 255, 0, defaultReticleColour.a);

                        shipDisplayModule.ShowDisplayReticle();

                        shipDisplayModule.HideCursor();
                    }
                    #endregion

                    #region Interactive and Hand IK Setup
                    // Get notified when the character changes what they are looking at
                    s3dPlayer.callbackOnChangeLookAtInteractive = OnInteractiveLookAtChanged;

                    // Set how close the character camera must be to see an interactive-enabled object
                    s3dPlayer.lookMaxInteractiveDistance = 3f;

                    // Let the character select a interactive-enabled object in the scene
                    // e.g. the docking button in the shuttle.
                    // Allow for the Call Elevator button to remain selected while the player
                    // is wanting to say put on their helment (which requires 2 selectables at the same time).
                    s3dPlayer.SetStoreMaxSelectableInScene(2, true);

                    // Hand IK setup (we could just set these things in the prefab...)
                    // Currently both hands use the left hand radius.
                    s3dPlayer.leftHandRadius = 0.08f;
                    s3dPlayer.SetLeftHandPalmOffset(new Vector3(-0.012f, 0.07f, 0.01f));
                    //s3dPlayer.SetLeftHandPalmRotation(new Vector3(0f, 315f, 270f));
                    s3dPlayer.SetRightHandPalmOffset(new Vector3(0.012f, 0.07f, 0.01f));
                    //s3dPlayer.SetRightHandPalmRotation(new Vector3(0f, 45f, 90f));

                    s3dPlayer.EnableHandIK(true);
                    s3dPlayer.EnableLookInteractive();
                    #endregion

                    #region Configure Sitting setup

                    if (s3dPlayer.IsAnimateEnabled)
                    {
                        s3dPlayerInteractStateId = s3dPlayer.GetAnimationStateId("Interact");
                    }

                    // Add a SampleSitAction component if one doesn't already exist on the player.
                    sampleSitActionPlayer = pilotController.GetComponent<SampleSitAction>();

                    if (sampleSitActionPlayer == null) { sampleSitActionPlayer = pilotController.AddComponent<SampleSitAction>(); }

                    if (sampleSitActionPlayer != null)
                    {
                        sampleSitActionPlayer.disableLook = false;
                    }

                    if (shuttlePilotChair != null && sampleSitActionPlayer != null)
                    {
                        // Move (TelePort) the player to be infront of the pilot chair.
                        Vector3 newPosition = shuttlePilotChair.transform.position + (shuttlePilotChair.transform.forward * 0.55f);
                        s3dPlayer.TelePort(newPosition, shuttlePilotChair.transform.rotation, true);

                        // Attempt to have the Player sit in the shuttle pilot's chair
                        Invoke("SitShuttlePilot", 1f);
                    }

                    #endregion

                    #region Configure the S3D input
                    // This could all be setup in the editor but this demonstrates how to do it at runtime.
                    // And by default, S3D is not included with SSC.
                    s3dPlayerInput = s3dPlayer.GetComponent<StickyInputModule>();

                    if (s3dPlayerInput != null)
                    {
                        s3dciMouseClick = new CustomInput();
                        s3dciSitting = new CustomInput();
                        s3dciPauseGame = new CustomInput();

                        // We want to disable user input at the start to avoid character rotating before they sit down.
                        s3dPlayerInput.DisableInput(true);

                        if (s3dciMouseClick != null && s3dciSitting != null)
                        {
                            s3dciMouseClick.customInputEvt = new CustomInputEvt();
                            s3dciSitting.customInputEvt = new CustomInputEvt();
                            s3dciPauseGame.customInputEvt = new CustomInputEvt();

                            // Add the listeners
                            if (s3dciMouseClick.customInputEvt != null && s3dciSitting.customInputEvt != null &&
                                s3dciPauseGame.customInputEvt != null &&
                                shuttlePlayerInputModule != null)
                            {
                                // One or more listeners can be added to the same custom input event.
                                // They will be executed in the order they are added when the user presses
                                // the appropriate button on the keyboard or gamepad
                                s3dciMouseClick.customInputEvt.AddListener(delegate { s3dPlayer.EngageLookingAtInteractive(false); });

                                // Allow the player to sit on the bench and stand up while in the services room
                                s3dciSitting.customInputEvt.AddListener(delegate { sampleSitActionPlayer.ToggleSit(); });

                                // Menu inputs for the character
                                s3dciPauseGame.customInputEvt.AddListener(delegate { TogglePause(); });
                            }

                            // Configure the buttons
                            s3dciMouseClick.canBeHeldDown = false;
                            s3dciMouseClick.isButton = true;
                            s3dciMouseClick.isButtonEnabled = true;

                            s3dciSitting.canBeHeldDown = false;
                            s3dciSitting.isButton = true;
                            s3dciSitting.isButtonEnabled = true;

                            s3dciPauseGame.canBeHeldDown = false;
                            s3dciPauseGame.isButton = true;
                            s3dciPauseGame.isButtonEnabled = true;

                            if (s3dPlayerInput.inputMode == StickyInputModule.InputMode.DirectKeyboard)
                            {
                                // Assume we're using the legacy input system for keyboard and mouse input
                                #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER

                                // Configure left mouse click for interactive-enabled object targeting (door control panels)
                                s3dciMouseClick.dkmPositiveKeycode = KeyCode.Mouse0;
                                // I key for docking shuttle
                                //s3dciDocking.dkmPositiveKeycode = KeyCode.I;
                                //// U key for undocking shuttle
                                //s3dciUndocking.dkmPositiveKeycode = KeyCode.U;
                                // T key to toggle sitting and standing
                                s3dciSitting.dkmPositiveKeycode = KeyCode.T;

                                // Menu ESC toggle game pause
                                s3dciPauseGame.dkmPositiveKeycode = KeyCode.Escape;

                                // If using UIS in future we could configure them here
                                #elif SSC_UIS

                                #endif
                            }

                            // Add the custom inputs to the list
                            s3dPlayerInput.customInputList.Add(s3dciMouseClick);
                            //s3dPlayerInput.customInputList.Add(s3dciDocking);
                            //s3dPlayerInput.customInputList.Add(s3dciUndocking);
                            s3dPlayerInput.customInputList.Add(s3dciSitting);
                            s3dPlayerInput.customInputList.Add(s3dciPauseGame);

                            // Reinitialise custom input so the custom input(s) we have added take effect
                            s3dPlayerInput.ReinitialiseCustomInput();
                        }
                    }

                    #endregion
                }
            }
            else
            {
                ConfigureWalkThrough(true);
            }
            #endregion

            ConfigureServicesAssistantNPC();
        }

        /// <summary>
        /// This is the NPC behind the Services desk in the spaceport
        /// </summary>
        private void ConfigureServicesAssistantNPC()
        {
            if (servicesAssistant != null)
            {
                // If Sticky3D Controller asset is installed, check if the services assistant is a S3D character
                s3dServiceAssistant = servicesAssistant.GetComponent<StickyControlModule>();
                if (s3dServiceAssistant != null)
                {
                    if (!s3dServiceAssistant.IsInitialised) { s3dServiceAssistant.Initialise(); }

                    if (s3dServiceAssistant.IsAnimateEnabled)
                    {
                        servicesAssistantInteractStateId = s3dServiceAssistant.GetAnimationStateId("Interact");
                    }

                    // Sit the NPC at the services desk
                    if (assistantChair != null)
                    {
                        // Move (TelePort) the NPC to be infront of the chair.
                        Vector3 newPosition = assistantChair.transform.position + (assistantChair.transform.forward * 0.6f);
                        s3dServiceAssistant.TelePort(newPosition, assistantChair.transform.rotation, true);

                        // Add a SampleSitAction component if one doesn't already exist on the NPC.
                        SampleSitAction sampleSitAction = servicesAssistant.GetComponent<SampleSitAction>();

                        if (sampleSitAction == null) { sampleSitAction = servicesAssistant.AddComponent<SampleSitAction>(); }

                        // Attempt to have the NPC sit on the chair
                        if (sampleSitAction != null)
                        {
                            // The space port is not moving, so we can just disable movement
                            sampleSitAction.lockPositonToRefFrame = false;
                            sampleSitAction.Invoke("SitDown", 1.2f);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add and configure any StickyInteractive components required in the scene.
        /// Typically this would all be setup in the editor, but this shows how to do it
        /// at runtime in code.
        /// </summary>
        private void ConfigureStickyInteractive()
        {
            #region Lift Door Control
            // Configure the control panel that allows the player to open the doors to the upper deck lift.
            if (lift1DoorControl != null)
            {
                // When using the door control panel, stop the doors opening automatically when they are unlocked
                SSCDoorProximity sscDoorProximity = lift1BottomDoors.gameObject.GetComponentInChildren<SSCDoorProximity>();
                if (sscDoorProximity != null)
                {
                    sscDoorProximity.isOpenDoorsOnEntry = false;
                    sscDoorProximity.isCloseDoorsOnExit = true;
                }

                // If the component hasn't been added, do it now
                if (lift1DoorControlInteractive == null)
                {
                    lift1DoorControlInteractive = lift1DoorControl.GetComponent<StickyInteractive>();
                    if (lift1DoorControlInteractive == null) { lift1DoorControlInteractive = lift1DoorControl.AddComponent<StickyInteractive>(); }
                }

                if (lift1DoorControlInteractive != null)
                {
                    lift1DoorControlInteractive.Initialise();
                    lift1DoorControlInteractive.SetIsTouchable(true);
                    lift1DoorControlInteractive.handHold1Offset = new Vector3(0f, 0.1f, 0.05f);
                    lift1DoorControlInteractive.handHold1Rotation = new Vector3(10f, 180f, 90f);

                    // The door control has a 3-button panel to give the player some visual feedback
                    sscDoorControlLift1Doors = lift1DoorControl.GetComponent<SSCDoorControl>();

                    if (sscDoorControlLift1Doors != null)
                    {
                        sscDoorControlLift1Doors.Initialise();

                        // Hook up a listener to open the doors
                        // See also OnDestroy() which removes the listeners
                        lift1DoorControlInteractive.onTouched = new S3DInteractiveEvt1();
                        lift1DoorControlInteractive.onTouched.AddListener(delegate { SSCDoorControl.SelectOpen(sscDoorControlLift1Doors); });

                        // Get notified when the player stops touching the door control buttons
                        // This will stop the character reaching for the button
                        lift1DoorControlInteractive.onStoppedTouching = new S3DInteractiveEvt2();
                        lift1DoorControlInteractive.onStoppedTouching.AddListener(DoorControlStopTouching);
                    }
                }
            }
            #endregion

            #region Lift Control
            // Configure the control panel that allows the player call the lift to the upper deck.
            if (lift1Control != null)
            {
                // If the component hasn't been added, do it now
                if (lift1ControlInteractive == null)
                {
                    lift1ControlInteractive = lift1Control.GetComponent<StickyInteractive>();
                    if (lift1ControlInteractive == null) { lift1ControlInteractive = lift1Control.AddComponent<StickyInteractive>(); }
                }

                if (lift1ControlInteractive != null && lift1MovingPlatform != null)
                {
                    // Get the component that will replace the control panel material
                    lift1ControlChangeMaterial = lift1Control.GetComponent<DemoSSCChangeMaterial>();

                    lift1ControlInteractive.Initialise();
                    lift1ControlInteractive.SetIsSelectable(true);

                    // See also OnDestroy() which removes the listeners
                    lift1ControlInteractive.onSelected = new S3DInteractiveEvt3();
                    lift1ControlInteractive.onUnselected = new S3DInteractiveEvt2();

                    // Switch the material on the call lift control so user can see that they have selected it
                    if (lift1ControlChangeMaterial != null)
                    {
                        lift1ControlInteractive.onSelected.AddListener(delegate { lift1ControlChangeMaterial.GetGroup1Material(1); });
                        lift1ControlInteractive.onUnselected.AddListener(delegate { lift1ControlChangeMaterial.GetGroup1Material(0); });
                    }

                    // Call the lift
                    lift1ControlInteractive.onSelected.AddListener(delegate { lift1MovingPlatform.CallToStartPosition(true); });
                }
            }
            #endregion

            #region Lift1 Safety Cabinet
            // Configure the cabinet handles to put on the player helmet at the top of Lift1
            if (lift1SafetyCabinet != null && s3dPlayerParts != null)
            {
                // If the component hasn't been added, do it now
                if (lift1SafetyCabinetInteractive == null)
                {
                    lift1SafetyCabinetInteractive = lift1SafetyCabinet.GetComponent<StickyInteractive>();
                    if (lift1SafetyCabinetInteractive == null) { lift1SafetyCabinetInteractive = lift1SafetyCabinet.AddComponent<StickyInteractive>(); }
                }

                if (lift1SafetyCabinetInteractive != null)
                {
                    lift1SafetyCabinetInteractive.Initialise();
                    lift1SafetyCabinetInteractive.SetIsSelectable(true);
                    // Make this object clickable rather than select and unselectable.
                    lift1SafetyCabinetInteractive.SetIsAutoUnselect(true);

                    // See also OnDestroy() which removes the listeners
                    lift1SafetyCabinetInteractive.onSelected = new S3DInteractiveEvt3();

                    // Toggle putting on and taking off the helmet.
                    // Head IK is enabled when the helmet is off.
                    // Unlock (or lock) the outer doors on the top deck of the space port.
                    lift1SafetyCabinetInteractive.onSelected.AddListener(delegate
                    {
                        s3dPlayerParts.ToggleEnablePartByIndex(2);
                        if (s3dPlayerParts.IsPartEnabledByIndex(2))
                        {
                            s3dPlayer.DisableHeadIK(false);
                            if (lift1OuterDoors != null) { lift1OuterDoors.UnlockDoors(); }
                        }
                        else
                        {
                            s3dPlayer.EnableHeadIK(true);
                            if (lift1OuterDoors != null) { lift1OuterDoors.LockDoors(); }
                        }
                    }
                    );
                }
            }

            #endregion

            #region Shuttle Side Door Control
            // Configure the control panel that allows the player to exit the shuttle into the space port
            if (shuttleSideDoorControl != null)
            {
                // If the component hasn't been added, do it now
                if (shuttleSideDoorControlInteractive == null)
                {
                    shuttleSideDoorControlInteractive = shuttleSideDoorControl.GetComponent<StickyInteractive>();
                    if (shuttleSideDoorControlInteractive == null) { shuttleSideDoorControlInteractive = shuttleSideDoorControl.AddComponent<StickyInteractive>(); }
                }

                if (shuttleSideDoorControlInteractive != null)
                {
                    shuttleSideDoorControlInteractive.Initialise();
                    shuttleSideDoorControlInteractive.SetIsTouchable(true);
                    shuttleSideDoorControlInteractive.handHold1Offset = new Vector3(0f, 0.1f, 0.05f);
                    shuttleSideDoorControlInteractive.handHold1Rotation = new Vector3(10f, 180f, 90f);

                    // The door control has a 3-button panel to give the player some visual feedback
                    sscDoorControlShuttleSideDoors = shuttleSideDoorControl.GetComponent<SSCDoorControl>();

                    if (sscDoorControlShuttleSideDoors != null)
                    {
                        sscDoorControlShuttleSideDoors.Initialise();

                        // Hook up a listener to open the doors
                        // See also OnDestroy() which removes the listeners
                        shuttleSideDoorControlInteractive.onTouched = new S3DInteractiveEvt1();
                        // We use a delegate as we are calling a method that doesn't use the event parameters.
                        shuttleSideDoorControlInteractive.onTouched.AddListener(delegate { SSCDoorControl.SelectOpen(sscDoorControlShuttleSideDoors); });

                        // These should really test if the door is open first but we're a little lazy...
                        shuttleSideDoorControlInteractive.onTouched.AddListener(delegate { StartSpacePortAmbientFX(); });
                        shuttleSideDoorControlInteractive.onTouched.AddListener(delegate { LightingEnterSpaceport(); });

                        // Get notified when the player stops touching the door control buttons
                        // This will stop the character reaching for the button
                        shuttleSideDoorControlInteractive.onStoppedTouching = new S3DInteractiveEvt2();
                        shuttleSideDoorControlInteractive.onStoppedTouching.AddListener(DoorControlStopTouching);

                    }
                }
            }
            #endregion

            #region Shuttle Console Dock Button
            if (dockingButton != null)
            {
                // If the component hasn't been added, do it now
                if (shuttleDockingButtonInteractive == null)
                {
                    shuttleDockingButtonInteractive = dockingButton.GetComponent<StickyInteractive>();
                    if (shuttleDockingButtonInteractive == null) { shuttleDockingButtonInteractive = dockingButton.AddComponent<StickyInteractive>(); }
                }

                if (shuttleDockingButtonInteractive != null)
                {
                    shuttleDockingButtonInteractive.Initialise();
                    shuttleDockingButtonInteractive.SetIsSelectable(true);

                    shuttleDockingButtonInteractive.onSelected = new S3DInteractiveEvt3();
                    // We don't need a delegate method here as we have a method with parameters that
                    // matches the event parameters.
                    shuttleDockingButtonInteractive.onSelected.AddListener(OnInterativeButtonSelected);
                }
            }

            #endregion
        }

        /// <summary>
        /// Attempt to have the Player sit in the shuttle pilot's chair
        /// </summary>
        private void SitShuttlePilot()
        {
            if (sampleSitActionPlayer != null)
            {
                sampleSitActionPlayer.SitDown();
                // Re-enable input so player can look around while seated
                s3dPlayerInput.EnableInput();
                // Start in 3rd person, switch to first person to sit down
                if (s3dPlayer.IsLookThirdPersonEnabled) { s3dPlayer.ToggleFirstThirdPerson(); }

                s3dPlayer.SetFreeLook(true);

                // We are using the look at point in update to set
                // the head ik target, so make sure we get the data.
                s3dPlayer.SetUpdateLookingAtPoint(true);

                s3dPlayer.SetHeadIKLookAtInteractive(true);

                s3dPlayer.EnableHeadIK(true);
            }
        }

        /// <summary>
        /// Attempt to have the Player stand up from the shuttle pilot's chair
        /// </summary>
        private void StandupShuttlePilot()
        {
            if (sampleSitActionPlayer != null)
            {
                sampleSitActionPlayer.StandUp();

                // Make sure we're in third person mode
                if (!s3dPlayer.IsLookThirdPersonEnabled) { s3dPlayer.ToggleFirstThirdPerson(); }

                // Make sure character can reach the shuttle door control
                s3dPlayer.SetRightHandMaxReachDistance(0.5f);
            }
        }

        #endif
        #endregion

        #region Private Methods - General

        /// <summary>
        /// Sometimes the enemy will attempt to attack the space port.
        /// </summary>
        /// <param name="shipAIInputModuleInstance"></param>
        /// <param name="shipControlModuleInstance"></param>
        /// <param name="squadronRadarQuery"></param>
        private bool AttemptAttackSpaceport (ShipAIInputModule shipAIInputModuleInstance, ShipControlModule shipControlModuleInstance)
        {
            bool isTargetAssigned = false;

            // Only attack the space port sometimes
            if (aiRandom != null && aiRandom.Range(0f, 1f) > 0.7f)
            {
                //int squadronId = shipControlModuleInstance.shipInstance.squadronId;
                int shipId = shipControlModuleInstance.shipInstance.shipId;

                // Execute the query
                sscRadar.GetRadarResults(enemyRadarQuery, sscRadarBlipsList);
                sscRadarBlipsListCount = sscRadarBlipsList.Count;

                if (sscRadarBlipsListCount > 0)
                {
                    // Choose a random radar blip
                    int chosenRadarBlipIndex = 0;
                    if ((int)enemyRadarQuery.querySortOrder == SSCRadarQuery.querySortOrderNoneInt)
                    {
                        // If there was no sort order for the query, choose completely at random
                        chosenRadarBlipIndex = UnityEngine.Random.Range(0, sscRadarBlipsListCount);
                    }
                    else
                    {
                        // If there was a specified sort order for the query, make sure we pick one of the top three results
                        UnityEngine.Random.Range(0, sscRadarBlipsListCount < 3 ? sscRadarBlipsListCount : 3);
                    }
                    SSCRadarBlip chosenRadarBlip = sscRadarBlipsList[chosenRadarBlipIndex];

                    if (chosenRadarBlip.radarItemType == SSCRadarItem.RadarItemType.GameObject)
                    {
                        GameObject chosenRadarBlipGameObject = chosenRadarBlip.itemGameObject;
                        if (chosenRadarBlipGameObject != null)
                        {
                            // Chosen target is a gameobject, so attack it with the strafing run state
                            shipAIInputModuleInstance.SetState(AIState.strafingRunStateID);
                            shipAIInputModuleInstance.AssignTargetPosition(chosenRadarBlip.wsPosition);
                            shipAIInputModuleInstance.AssignTargetRadius(150f);
                            shipAIInputModuleInstance.maxSpeed = strafingRunSpeed;

                            // Ensure this ship is not in the available list
                            availableEnemyShips.Remove(shipId);

                            //Debug.Log("[DEBUG] " + shipControlModuleInstance.name + " is attacking " + chosenRadarBlipGameObject + " at T:" + Time.time);

                            isTargetAssigned = true;
                        }
                        #if UNITY_EDITOR
                        else
                        {
                            Debug.LogWarning("TechDemo3.cs AttemptAttackSpaceport: chosen radar blip gameobject is null.");
                        }
                        #endif
                    }
                    #if UNITY_EDITOR
                    else
                    {
                        Debug.LogWarning("TechDemo3.cs AttemptAttackSpaceport: Radar blip has wrong type (" + chosenRadarBlip.radarItemType + ")");
                    }
                    #endif
                }
            }

            return isTargetAssigned;
        }

        /// <summary>
        /// Attempts to pair a ship with another ship. If successful, one ship will then pursue the other ship.
        /// </summary>
        /// <param name="shipAIInputModuleInstance"></param>
        /// <param name="shipControlModuleInstance"></param>
        /// <returns></returns>
        private void AttemptPairing (ShipAIInputModule shipAIInputModuleInstance, ShipControlModule shipControlModuleInstance)
        {
            // TODO need to make sure that when ships are destroyed they are removed from the relevant lists

            if (shipAIInputModuleInstance != null && shipControlModuleInstance != null)
            {
                bool pairingSuccessful = false;
                ShipAIInputModule friendlyShipAIInputModule = null;
                ShipAIInputModule enemyShipAIInputModule = null;

                // Check whether this ship is a friendly ship or an enemy ship
                bool isFriendlyShip = shipControlModuleInstance.shipInstance.factionId == friendlyFactionID;

                if (isFriendlyShip)
                {
                    friendlyShipAIInputModule = shipAIInputModuleInstance;
                    int thisShipListIndex = availableFriendlyShips.FindIndex(a => a == shipControlModuleInstance.GetShipId);

                    // If this is a friendly ship, look for an enemy ship to pair it with
                    if (availableEnemyShips != null && availableEnemyShips.Count > 0)
                    {
                        // Pick the first enemy ship from the list (as this has theoretically been in the queue for the longest)
                        int pairedShipId = availableEnemyShips[0];
                        // Find the paired ship in the lists of enemy ships
                        for (int i = 0; i < numEnemyShip1; i++)
                        {
                            if (enemyType1Ships[i].GetShipControlModule.GetShipId == pairedShipId)
                            {
                                enemyShipAIInputModule = enemyType1Ships[i];
                            }
                        }
                        if (enemyShipAIInputModule == null)
                        {
                            for (int i = 0; i < numEnemyShip2; i++)
                            {
                                if (enemyType2Ships[i].GetShipControlModule.GetShipId == pairedShipId)
                                {
                                    enemyShipAIInputModule = enemyType2Ships[i];
                                }
                            }
                        }
                        // Only continue if we managed to find the paired ship
                        if (enemyShipAIInputModule != null)
                        {
                            // Remove this ship from the list of available friendly ships (if it has been added)
                            if (thisShipListIndex != -1) { availableFriendlyShips.RemoveAt(thisShipListIndex); }
                            // Remove the enemy ship we found from the list of available enemy ships 
                            availableEnemyShips.RemoveAt(0);

                            // Record that pairing was successful
                            pairingSuccessful = true;
                        }
                    }
                    else
                    {
                        // If no enemy ship was found to pair it with, follow the friendly idle path
                        // Essentially, we want to wait for an enemy ship to be available to pair with us
                        shipAIInputModuleInstance.SetState(AIState.moveToStateID);
                        shipAIInputModuleInstance.AssignTargetPath(friendlyIdlePath);
                        shipAIInputModuleInstance.maxSpeed = friendlyIdleSpeed;
                        // Add this ship to the list of available friendly ships (if it hasn't already been added)
                        if (thisShipListIndex == -1) { availableFriendlyShips.Add(shipControlModuleInstance.GetShipId); }
                    }
                }
                else
                {
                    // If this is an enemy ship, look for a friendly ship to pair it with
                    enemyShipAIInputModule = shipAIInputModuleInstance;
                    int thisShipListIndex = availableEnemyShips.FindIndex(a => a == shipControlModuleInstance.GetShipId);

                    if (availableFriendlyShips != null && availableFriendlyShips.Count > 0)
                    {
                        // Pick the first friendly ship from the list (as this has theoretically been in the queue for the longest)
                        int pairedShipId = availableFriendlyShips[0];
                        // Find the paired ship in the list of friendly ships
                        for (int i = 0; i < numFriendlyShip; i++)
                        {
                            if (friendlyShips[i].GetShipControlModule.GetShipId == pairedShipId)
                            {
                                friendlyShipAIInputModule = friendlyShips[i];
                            }
                        }
                        // Only continue if we managed to find the paired ship
                        if (friendlyShipAIInputModule != null)
                        {
                            // Remove this ship from the list of available enemy ships (if it has been added)
                            if (thisShipListIndex != -1) { availableEnemyShips.RemoveAt(thisShipListIndex); }
                            // Remove the friendly ship we found from the list of available friendly ships 
                            availableFriendlyShips.RemoveAt(0);

                            // Record that pairing was successful
                            pairingSuccessful = true;
                        }
                    }
                    else
                    {
                        // If no friendly ship was found to pair it with, attempt a strafing run (if bombing has started)
                        // Essentially, we want to wait for an friendly ship to be availabe to pair with us
                        if (!bombingRunsStarted || !AttemptAttackSpaceport(shipAIInputModuleInstance, shipControlModuleInstance))
                        {
                            // Otherwise follow the enemy idle path
                            shipAIInputModuleInstance.SetState(AIState.moveToStateID);
                            shipAIInputModuleInstance.AssignTargetPath(enemyIdlePath);
                            shipAIInputModuleInstance.maxSpeed = enemyIdleSpeed;

                            // Add this ship to the list of available enemy ships (if it hasn't already been added)
                            if (thisShipListIndex == -1) { availableEnemyShips.Add(shipControlModuleInstance.GetShipId); }
                        }
                    }
                }

                // If we found two ships to pair together...
                if (pairingSuccessful && friendlyShipAIInputModule != null && enemyShipAIInputModule != null)
                {
                    // Randomly choose whether the friendly ship will be pursuing the enemy ship (or the other way around)
                    bool friendlyShipPursuing = aiRandom.Normalised() > 0.5f;

                    // One ship is pursuing, one ship is escaping
                    if (friendlyShipPursuing)
                    {
                        friendlyShipAIInputModule.SetState(AIState.dogfightStateID);
                        friendlyShipAIInputModule.AssignTargetShip(enemyShipAIInputModule.GetShipControlModule);
                        friendlyShipAIInputModule.maxSpeed = pursuitSpeed;
                        enemyShipAIInputModule.SetState(AIState.moveToStateID);
                        int enemyEscapePathsCount = enemyEscapePaths.Count;
                        int enemyEscapePathIndex = aiRandom.Range(0, enemyEscapePathsCount-1);
                        enemyShipAIInputModule.AssignTargetPath(enemyEscapePaths[enemyEscapePathIndex]);
                        enemyShipAIInputModule.AssignShipsToEvade(new List<Ship>(new Ship[]{ friendlyShipAIInputModule.GetShipControlModule.shipInstance }));
                        enemyShipAIInputModule.maxSpeed = pursuitSpeed;
                    }
                    else
                    {
                        enemyShipAIInputModule.SetState(AIState.dogfightStateID);
                        enemyShipAIInputModule.AssignTargetShip(friendlyShipAIInputModule.GetShipControlModule);
                        enemyShipAIInputModule.maxSpeed = pursuitSpeed;
                        friendlyShipAIInputModule.SetState(AIState.moveToStateID);
                        int friendlyEscapePathsCount = friendlyEscapePaths.Count;
                        int friendlyEscapePathIndex = aiRandom.Range(0, friendlyEscapePathsCount);
                        friendlyShipAIInputModule.AssignTargetPath(friendlyEscapePaths[0]);
                        friendlyShipAIInputModule.AssignShipsToEvade(new List<Ship>(new Ship[] { enemyShipAIInputModule.GetShipControlModule.shipInstance }));
                        friendlyShipAIInputModule.maxSpeed = pursuitSpeed;
                    }
                }
            }
        }
        
        /// <summary>
        /// Check if each of the capital ships is destroyed and the state of the shield generators. If all
        /// shield generators on a capital ship have been destroyed, lower the shield on the bridge.
        /// </summary>
        private void CheckCapitalShipHealth()
        {
            // Loop through all the capital ships
            for (int i = 0; i < numEnemyCapitalShips; i++)
            {
                ShipAIInputModule enemyShip = enemyCapitalShips[i];
                if (enemyShip != null)
                {
                    ShipControlModule enemyShipCM = enemyShip.GetShipControlModule;
                    if (enemyShipCM != null && enemyShipCM.ShipIsEnabled() && !enemyShipCM.shipInstance.Destroyed())
                    {
                        // If the bridge is still shielded, it means the shield generators haven't been destroyed
                        if (enemyShipCM.shipInstance.localisedDamageRegionList[7].useShielding)
                        {
                            // Count the number of shield generators (their damage regions) with zero health
                            int currentNumShieldGeneratorsDestroyed = 0;
                            if (enemyShipCM.shipInstance.localisedDamageRegionList[3].Health <= 0f) { currentNumShieldGeneratorsDestroyed++; }
                            if (enemyShipCM.shipInstance.localisedDamageRegionList[4].Health <= 0f) { currentNumShieldGeneratorsDestroyed++; }
                            if (enemyShipCM.shipInstance.localisedDamageRegionList[5].Health <= 0f) { currentNumShieldGeneratorsDestroyed++; }
                            if (enemyShipCM.shipInstance.localisedDamageRegionList[6].Health <= 0f) { currentNumShieldGeneratorsDestroyed++; }

                            // Bring down the shield if the health of all the shield generator regions is zero
                            if (currentNumShieldGeneratorsDestroyed == 4)
                            {
                                enemyShipCM.shipInstance.localisedDamageRegionList[7].useShielding = false;
                            }
                        }
                        // Else if the shields are already down, check if the bridge has been destroyed
                        else if (enemyShipCM.shipInstance.localisedDamageRegionList[7].Health <= 0f)
                        {
                            // If the bridge has been destroyed, destroy the capital ship
                            enemyShipCM.shipInstance.mainDamageRegion.useShielding = false;
                            enemyShipCM.shipInstance.ApplyNormalDamage(1000f, ProjectileModule.DamageType.Default, Vector3.zero);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Configure custom EffectsModules we use for scene-specific FX
        /// </summary>
        private void ConfigureEffectsPools()
        {
            // Used for playing a sound FX 
            if (soundPoolPrefab != null)
            {
                if (sscManager != null)
                {
                    soundeffectsObjectPrefabID = sscManager.GetorCreateEffectsPool(soundPoolPrefab);
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: TechDemo3 - the soundPoolPrefab has not been added in the inspector.");
            }
            #endif

            if (sparksEffectsObjectPrefab != null)
            {
                sparksEffectsObjectPrefabID = sscManager.GetorCreateEffectsPool(sparksEffectsObjectPrefab);

                // Cache the number of places the sparks can be randomly instantiated
                numSparkFXPoints = sparkFXPoints == null ? 0 : sparkFXPoints.Length;
                sparksRandom = new SSCRandom();
                sparksRandom.SetSeed(7252221);
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: TechDemo3 - the sparksEffectsObjectPrefab has not been added in the inspector.");
            }
            #endif
        }

        /// <summary>
        /// Configure the heads-up display
        /// </summary>
        private void ConfigureHUD()
        {
            if (shipDisplayModule != null)
            {
                // Use the HUD to show indicators when the shuttle is docked
                dockedIndicator = shipDisplayModule.GetDisplayGauge("Docked Indicator");

                // Gauges
                fuelLevelGauge = shipDisplayModule.GetDisplayGauge("Fuel Level");
                heatLevelGauge = shipDisplayModule.GetDisplayGauge("Heat Level");
                healthGauge = shipDisplayModule.GetDisplayGauge("Health");
                missilesGauge = shipDisplayModule.GetDisplayGauge("Missiles");
                shieldsGauge = shipDisplayModule.GetDisplayGauge("Shields");
                launchGauge = shipDisplayModule.GetDisplayGauge("Launch");
                gearGauge = shipDisplayModule.GetDisplayGauge("Gear");
                enemyGauge = shipDisplayModule.GetDisplayGauge("Enemy");

                if (enemyGauge != null) { enemyGauge.gaugeMaxValue = numEnemyShip1 + numEnemyShip2; }

                // Find the message to tell player to dock the shuttle with the space port
                startDockingMessage = shipDisplayModule.GetDisplayMessage("Start docking");
                shipDisplayModule.ShowDisplayMessage(startDockingMessage);

                // This message is displayed when useFlyThru is enabled and the shuttle is docking
                dockingInProgressMessage = shipDisplayModule.GetDisplayMessage("Docking in Progress");

                // Find the message to tell player to leave the shuttle via the side door
                exitShuttleMessage = shipDisplayModule.GetDisplayMessage("Exit Shuttle");

                // Find the message to tell player to find the services room
                findServiceDeskMessage = shipDisplayModule.GetDisplayMessage("Get to services");

                // Find the message we can use in the services room to tell the player to take the lift to the top deck
                getToDeckMessage = shipDisplayModule.GetDisplayMessage("Get to deck");

                // Find the messages used at the top of lift1. With Helmet is used with Stick3D when user hasn't put on helmet
                // my selecting the handles of the safety cabinet.
                getToHawkMessage = shipDisplayModule.GetDisplayMessage("Get to Hawk");
                getToHawkMessageWithHelmet = shipDisplayModule.GetDisplayMessage("Get to Hawk with Helmet");

                // Create Display Targets at runtime (by design they are not shown when first added)
                // Display Targets should not have overlaying factions or squadrons. That is, each DisplayTarget
                // should be limited to a unique group or category of radar items. The same faction or squadron
                // should not appear in multiple DisplayTargets.
                
                // Create an enemy "target"
                int guidHashReticle = shipDisplayModule.GetDisplayReticleGuidHash("SSCUIAim3");
                DisplayTarget displayTarget = shipDisplayModule.AddTarget(guidHashReticle);
                displayTarget.factionsToInclude = new int[] { enemyFactionID };
                shipDisplayModule.AddTargetSlots(displayTarget, 2);
                shipDisplayModule.SetDisplayTargetReticleColour(displayTarget, Color.red);

                // Create a friendly "target"
                guidHashReticle = shipDisplayModule.GetDisplayReticleGuidHash("SSCUIAim3");
                displayTarget = shipDisplayModule.AddTarget(guidHashReticle);
                displayTarget.isTargetable = false;
                displayTarget.factionsToInclude = new int[] { friendlyFactionID };
                shipDisplayModule.SetDisplayTargetReticleColour(displayTarget, Color.blue);

                shipDisplayModule.HideHeading();

                #if SCSM_S3D
                shipDisplayModule.Initialise();
                #endif
                //shipDisplayModule.HideCursor();

            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: TechDemo3- the shipDisplayModule (HUD) has not been added in the inspector.");
            }
            #endif
        }

        /// <summary>
        /// Setup the scene for when there is no integration with Sticky Control Module
        /// </summary>
        private void ConfigureNoStickyController()
        {
            #if SCSM_S3D
            if (pilotController != null)
            {
                s3dPlayer = pilotController.GetComponent<StickyControlModule>();
                if (s3dPlayer != null)
                {
                    s3dPlayer.DisableCharacter(false);
                }

                pilotController.gameObject.SetActive(false);
            }

            #endif

            // Make sure we can get out the shuttle side doors
            if (shuttleSideDoorControl != null && sscShuttleDoorAnimator != null)
            {
                // The door control has a 3-button panel to give the player some visual feedback
                sscDoorControlShuttleSideDoors = shuttleSideDoorControl.GetComponent<SSCDoorControl>();

                if (sscDoorControlShuttleSideDoors != null)
                {
                    sscDoorControlShuttleSideDoors.Initialise();
                }
                
                sscShuttleDoorAnimator.UnlockDoor(0);
            }

            // Sticky3D characters can activate the doors using the SSCDoorControl panel. But without this, we should
            // just open the doors, once they are unlocked, using the Door Proximity component with the trigger collider.
            if (lift1BottomDoors != null)
            {
                SSCDoorProximity sscDoorProximity = lift1BottomDoors.gameObject.GetComponentInChildren<SSCDoorProximity>();
                if (sscDoorProximity != null)
                {
                    sscDoorProximity.isOpenDoorsOnEntry = true;
                    sscDoorProximity.isCloseDoorsOnExit = true;
                }
            }

            // When Sticky3D is not being used, we still might want to simulate clicking the
            // call lift button during the walk through.
            if (lift1Control != null && lift1ControlChangeMaterial == null)
            {
                // Get the component that will replace the control panel material
                lift1ControlChangeMaterial = lift1Control.GetComponent<DemoSSCChangeMaterial>();

                TurnOffLiftCallButton();
            }

            // There is no S3D character to put on a helmet, so just unlock the outer doors at top of lift1
            if (lift1OuterDoors != null) { lift1OuterDoors.UnlockDoors(); }

            if (startDockingMessage != null && shipDisplayModule != null)
            {
                shipDisplayModule.SetDisplayMessageText(startDockingMessage, "(I)nitiate docking procedure");
            }

            if (playerShuttle != null && playerShuttle.IsInitialised)
            {
                shuttleOriginalPos = playerShuttle.shipInstance.TransformPosition;
                shuttleOriginalRot = playerShuttle.shipInstance.TransformRotation;
            }

            ConfigureWalkThrough(true);
        }

        /// <summary>
        /// Destroy the shuttle the player arrived in
        /// </summary>
        private void DestroyShuttle()
        {
            if (playerShuttle != null && playerShuttle.IsInitialised)
            {
                playerShuttle.shipInstance.mainDamageRegion.Health = 0;
            }
        }

        /// <summary>
        /// Get the AIShipInputModule using only the Ship script reference
        /// </summary>
        /// <param name="shipInstance"></param>
        /// <returns></returns>
        private ShipAIInputModule FindAIShip(Ship shipInstance)
        {
            ShipAIInputModule shipAIInputModule = null;

            int factionId = shipInstance.factionId;
            int squadronId = shipInstance.squadronId;
            int shipId = shipInstance.shipId;

            // Look in the appropriate list
            if (factionId == friendlyFactionID)
            {
                for (int i = 0; i < numFriendlyShip; i++)
                {
                    ShipAIInputModule friendlyShip = friendlyShips[i];
                    if (friendlyShip != null)
                    {
                        // Is this the ship we're looking for?
                        if (friendlyShip.GetShipId == shipId)
                        {
                            shipAIInputModule = friendlyShip;
                            break;
                        }
                    }
                }
            }
            else if (factionId == enemyFactionID)
            {
                if (squadronId == enemySquadron1ID)
                {
                    for (int i = 0; i < numEnemyShip1; i++)
                    {
                        ShipAIInputModule enemyShip = enemyType1Ships[i];
                        if (enemyShip != null)
                        {
                            if (enemyShip.GetShipId == shipId)
                            {
                                shipAIInputModule = enemyShip;
                                break;
                            }
                        }
                    }
                }
                else if (squadronId == enemySquadron2ID)
                {
                    for (int i = 0; i < numEnemyShip2; i++)
                    {
                        ShipAIInputModule enemyShip = enemyType2Ships[i];
                        if (enemyShip != null)
                        {
                            if (enemyShip.GetShipId == shipId)
                            {
                                shipAIInputModule = enemyShip;
                                break;
                            }
                        }
                    }
                }
            }


            return shipAIInputModule;
        }

        /// <summary>
        /// Spawns in and initialises the ships for the attack.
        /// </summary>
        private void InitialiseAttackShips ()
        {
            #region Set Up Radar Queries

            // Set up friendly radar query
            friendlyRadarQuery = new SSCRadarQuery();
            friendlyRadarQuery.centrePosition = battleCentralPos;
            friendlyRadarQuery.range = 10000f;
            friendlyRadarQuery.factionId = enemyFactionID;
            friendlyRadarQuery.factionsToInclude = null;
            friendlyRadarQuery.factionsToExclude = null;
            friendlyRadarQuery.squadronsToInclude = null;
            friendlyRadarQuery.squadronsToExclude = null;
            friendlyRadarQuery.is3DQueryEnabled = true;
            friendlyRadarQuery.querySortOrder = SSCRadarQuery.QuerySortOrder.DistanceAsc3D;

            // Set up enemy radar query
            enemyRadarQuery = new SSCRadarQuery();
            enemyRadarQuery.centrePosition = battleCentralPos;
            enemyRadarQuery.range = 10000f;
            enemyRadarQuery.factionId = friendlyFactionID;
            enemyRadarQuery.factionsToInclude = null;
            enemyRadarQuery.factionsToExclude = null;
            enemyRadarQuery.squadronsToInclude = new int[] { barrelsSquadronID };
            enemyRadarQuery.squadronsToExclude = null;
            enemyRadarQuery.is3DQueryEnabled = true;
            enemyRadarQuery.querySortOrder = SSCRadarQuery.QuerySortOrder.None;

            #endregion

            #region Spawn in and Initialise Ships

            // Initialise random generator
            aiRandom = new SSCRandom();
            aiRandom.SetSeed(0);

            // Initialise lists of all the ships
            friendlyShips = new List<ShipAIInputModule>(numFriendlyShip);
            enemyType1Ships = new List<ShipAIInputModule>(numEnemyShip1);
            enemyType2Ships = new List<ShipAIInputModule>(numEnemyShip2);
            enemyCapitalShips = new List<ShipAIInputModule>(capitalShipSpawnPoints != null ? capitalShipSpawnPoints.Length : 0);

            // Initialise lists of available ships
            availableFriendlyShips = new List<int>(numFriendlyShip);
            availableEnemyShips = new List<int>(numEnemyShip1 + numEnemyShip2);

            ShipDocking shipDocking;

            // Spawn in friendly ships
            if (friendlyShipPrefab != null && numFriendlyShip > 0)
            {
                for (int i = 0; i < numFriendlyShip; i++)
                {
                    // Instantiate the ship at the origin
                    GameObject shipGameObjectInstance = Object.Instantiate(friendlyShipPrefab.gameObject,
                        Vector3.zero, Quaternion.identity);
                    // Append an index to the name
                    shipGameObjectInstance.name += " " + (i + 1).ToString();
                    // Parent the ship to the ships gameobject
                    if (shipsParentGameObject != null)
                    {
                        shipGameObjectInstance.transform.SetParent(shipsParentGameObject.transform);
                    }

                    // If there is a docking component on the prefab, remove it as we don't need it in this demo
                    if (shipGameObjectInstance.TryGetComponent(out shipDocking))
                    {
                        Destroy(shipDocking);
                    }

                    // Position the ship correctly
                    float rotY = 360f * ((float)i / numFriendlyShip);
                    shipGameObjectInstance.transform.position = battleCentralPos +
                        (Quaternion.Euler(0f, rotY, 0f) * Vector3.forward * friendlyShipSpawnDist);
                    // Rotate the ship correctly
                    shipGameObjectInstance.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

                    // Get the ship control module instance
                    ShipControlModule shipControlModuleInstance = shipGameObjectInstance.GetComponent<ShipControlModule>();
                    // Get the ship AI input module instance
                    ShipAIInputModule shipAIInputModuleInstance = shipGameObjectInstance.GetComponent<ShipAIInputModule>();
                    // Initialise the ship
                    shipControlModuleInstance.InitialiseShip();
                    shipAIInputModuleInstance.Initialise();
                    // Set the ship to the idle state
                    if (shipAIInputModuleInstance.IsInitialised)
                    {
                        shipAIInputModuleInstance.SetState(AIState.idleStateID);
                    }
                    // Set the faction/squadron ID of the ship
                    shipControlModuleInstance.shipInstance.factionId = friendlyFactionID;
                    shipControlModuleInstance.shipInstance.squadronId = friendlySquadronID;

                    // Set the spawn point of the ship
                    shipControlModuleInstance.shipInstance.customRespawnPosition = shipGameObjectInstance.transform.position;
                    shipControlModuleInstance.shipInstance.respawningMode = Ship.RespawningMode.RespawnAtSpecifiedPosition;

                    // Stuck action
                    shipControlModuleInstance.shipInstance.stuckTime = 2f;
                    shipControlModuleInstance.shipInstance.stuckSpeedThreshold = 0.1f;
                    shipControlModuleInstance.shipInstance.stuckAction = Ship.StuckAction.InvokeCallback;

                    // Make the ship visible to radar
                    shipControlModuleInstance.EnableRadar();

                    // Set the accuracy of the ship
                    shipAIInputModuleInstance.targetingAccuracy = friendlyAccuracy;

                    // Set up callbacks
                    shipControlModuleInstance.callbackOnRespawn = OnRespawnCallback;
                    shipControlModuleInstance.callbackOnStuck = OnShipStuck;
                    shipControlModuleInstance.callbackOnDestroy = OnAIShipDestroyed;
                    shipAIInputModuleInstance.callbackCompletedStateAction = CompletedStateActionCallback;

                    // Disable the ship until the attack starts
                    shipControlModuleInstance.DisableShip(true);

                    // To improve obstacle avoidance increase the ship's radius
                    // NOTE: This also affects path following so don't make it too large
                    if (shipAIInputModuleInstance.shipRadius < 10f)
                    {
                        shipAIInputModuleInstance.shipRadius = 10f;
                    }

                    // Store this ship in a list for future reference
                    friendlyShips.Add(shipAIInputModuleInstance);
                    availableFriendlyShips.Add(shipControlModuleInstance.GetShipId);
                }
            }

            // Spawn in enemy attack ships
            if (enemyShip1Prefab != null && numEnemyShip1 > 0)
            {
                for (int i = 0; i < numEnemyShip1; i++)
                {
                    // Instantiate the ship at the origin
                    GameObject shipGameObjectInstance = Object.Instantiate(enemyShip1Prefab.gameObject,
                        Vector3.zero, Quaternion.identity);
                    // Append an index to the name
                    shipGameObjectInstance.name += " " + (i + 1).ToString();
                    // Parent the ship to the ships gameobject
                    if (shipsParentGameObject != null)
                    {
                        shipGameObjectInstance.transform.SetParent(shipsParentGameObject.transform);
                    }

                    // If there is a docking component on the prefab, remove it as we don't need it in this demo
                    if (shipGameObjectInstance.TryGetComponent(out shipDocking))
                    {
                        Destroy(shipDocking);
                    }

                    // Position the ship correctly
                    float rotY = 360f * ((float)(i) / (numEnemyShip1 + numEnemyShip2));
                    shipGameObjectInstance.transform.position = battleCentralPos +
                        (Quaternion.Euler(0f, rotY, 0f) * Vector3.forward * enemyShipSpawnDist) + (Vector3.up * 200f);
                    // Rotate the ship correctly
                    shipGameObjectInstance.transform.rotation = Quaternion.Euler(0f, rotY + 180f, 0f);

                    // Get the ship control module instance
                    ShipControlModule shipControlModuleInstance = shipGameObjectInstance.GetComponent<ShipControlModule>();
                    // Get the ship AI input module instance
                    ShipAIInputModule shipAIInputModuleInstance = shipGameObjectInstance.GetComponent<ShipAIInputModule>();
                    // Initialise the ship
                    shipControlModuleInstance.InitialiseShip();
                    shipAIInputModuleInstance.Initialise();
                    // Set the ship to the idle state
                    if (shipAIInputModuleInstance.IsInitialised)
                    {
                        shipAIInputModuleInstance.SetState(AIState.idleStateID);
                    }
                    // Set the faction/squadron ID of the ship
                    shipControlModuleInstance.shipInstance.factionId = enemyFactionID;
                    shipControlModuleInstance.shipInstance.squadronId = enemySquadron1ID;

                    // Set the spawn point of the ship
                    shipControlModuleInstance.shipInstance.customRespawnPosition = shipGameObjectInstance.transform.position;
                    shipControlModuleInstance.shipInstance.respawningMode = Ship.RespawningMode.RespawnAtSpecifiedPosition;

                    // Stuck action
                    shipControlModuleInstance.shipInstance.stuckTime = 2f;
                    shipControlModuleInstance.shipInstance.stuckSpeedThreshold = 0.1f;
                    shipControlModuleInstance.shipInstance.stuckAction = Ship.StuckAction.InvokeCallback;

                    // Make the ship visible to radar
                    shipControlModuleInstance.EnableRadar();

                    // Set the accuracy of the ship
                    shipAIInputModuleInstance.targetingAccuracy = enemyAccuracy;

                    // Set up callbacks
                    shipControlModuleInstance.callbackOnRespawn = OnRespawnCallback;
                    shipControlModuleInstance.callbackOnStuck = OnShipStuck;
                    shipControlModuleInstance.callbackOnDestroy = OnAIShipDestroyed;
                    shipAIInputModuleInstance.callbackCompletedStateAction = CompletedStateActionCallback;

                    // Disable the ship until the attack starts
                    shipControlModuleInstance.DisableShip(true);

                    // To improve obstacle avoidance increase the ship's radius
                    // NOTE: This also affects path following so don't make it too large
                    if (shipAIInputModuleInstance.shipRadius < 7f)
                    {
                        shipAIInputModuleInstance.shipRadius = 7f;
                    }

                    // Store this ship in a list for future reference
                    enemyType1Ships.Add(shipAIInputModuleInstance);
                    availableEnemyShips.Add(shipControlModuleInstance.GetShipId);
                }
            }
            if (enemyShip2Prefab != null && numEnemyShip2 > 0)
            {
                for (int i = 0; i < numEnemyShip2; i++)
                {
                    // Instantiate the ship at the origin
                    GameObject shipGameObjectInstance = Object.Instantiate(enemyShip2Prefab.gameObject,
                        Vector3.zero, Quaternion.identity);
                    // Append an index to the name
                    shipGameObjectInstance.name += " " + (i + 1).ToString();
                    // Parent the ship to the ships gameobject
                    if (shipsParentGameObject != null)
                    {
                        shipGameObjectInstance.transform.SetParent(shipsParentGameObject.transform);
                    }

                    // If there is a docking component on the prefab, remove it as we don't need it in this demo
                    if (shipGameObjectInstance.TryGetComponent(out shipDocking))
                    {
                        Destroy(shipDocking);
                    }

                    // Position the ship correctly
                    float rotY = 360f * ((float)(i + numEnemyShip1) / (numEnemyShip1 + numEnemyShip2));
                    shipGameObjectInstance.transform.position = battleCentralPos +
                        (Quaternion.Euler(0f, rotY, 0f) * Vector3.forward * enemyShipSpawnDist) + (Vector3.up * 200f);
                    // Rotate the ship correctly
                    shipGameObjectInstance.transform.rotation = Quaternion.Euler(0f, rotY + 180f, 0f);

                    // Get the ship control module instance
                    ShipControlModule shipControlModuleInstance = shipGameObjectInstance.GetComponent<ShipControlModule>();
                    // Get the ship AI input module instance
                    ShipAIInputModule shipAIInputModuleInstance = shipGameObjectInstance.GetComponent<ShipAIInputModule>();
                    // Initialise the ship
                    shipControlModuleInstance.InitialiseShip();
                    shipAIInputModuleInstance.Initialise();
                    // Set the ship to the idle state
                    if (shipAIInputModuleInstance.IsInitialised)
                    {
                        shipAIInputModuleInstance.SetState(AIState.idleStateID);
                    }
                    // Set the faction/squadron ID of the ship
                    shipControlModuleInstance.shipInstance.factionId = enemyFactionID;
                    shipControlModuleInstance.shipInstance.squadronId = enemySquadron2ID;

                    // Set the spawn point of the ship
                    shipControlModuleInstance.shipInstance.customRespawnPosition = shipGameObjectInstance.transform.position;
                    shipControlModuleInstance.shipInstance.respawningMode = Ship.RespawningMode.RespawnAtSpecifiedPosition;

                    // Stuck action
                    shipControlModuleInstance.shipInstance.stuckTime = 2f;
                    shipControlModuleInstance.shipInstance.stuckSpeedThreshold = 0.1f;
                    shipControlModuleInstance.shipInstance.stuckAction = Ship.StuckAction.InvokeCallback;

                    // Make the ship visible to radar
                    shipControlModuleInstance.EnableRadar();

                    // Set the accuracy of the ship
                    shipAIInputModuleInstance.targetingAccuracy = enemyAccuracy;

                    // Set up callbacks
                    shipControlModuleInstance.callbackOnRespawn = OnRespawnCallback;
                    shipControlModuleInstance.callbackOnStuck = OnShipStuck;
                    shipControlModuleInstance.callbackOnDestroy = OnAIShipDestroyed;
                    shipAIInputModuleInstance.callbackCompletedStateAction = CompletedStateActionCallback;

                    // Disable the ship until the attack starts
                    shipControlModuleInstance.DisableShip(true);

                    // Store this ship in a list for future reference
                    enemyType2Ships.Add(shipAIInputModuleInstance);
                    availableEnemyShips.Add(shipControlModuleInstance.GetShipId);
                }
            }

            // Spawn in enemy capital ships
            if (enemyCapitalShipPrefab != null && capitalShipSpawnPoints != null && capitalShipSpawnPoints.Length > 0)
            {
                for (int i = 0; i < capitalShipSpawnPoints.Length; i++)
                {
                    // Instantiate the ship at the origin
                    GameObject shipGameObjectInstance = Object.Instantiate(enemyCapitalShipPrefab.gameObject,
                        Vector3.zero, Quaternion.identity);
                    // Append an index to the name
                    shipGameObjectInstance.name += " " + (i + 1).ToString();
                    // Parent the ship to the ships gameobject
                    if (shipsParentGameObject != null)
                    {
                        shipGameObjectInstance.transform.parent = shipsParentGameObject.transform;
                    }

                    // Position the ship correctly
                    shipGameObjectInstance.transform.position = capitalShipSpawnPoints[i];
                    // Rotate the ship correctly
                    shipGameObjectInstance.transform.rotation = Quaternion.Euler(capitalShipSpawnRots[i]);

                    // Get the ship control module instance
                    ShipControlModule shipControlModuleInstance = shipGameObjectInstance.GetComponent<ShipControlModule>();
                    // Get the ship AI input module instance
                    ShipAIInputModule shipAIInputModuleInstance = shipGameObjectInstance.GetComponent<ShipAIInputModule>();
                    // Initialise the ship
                    shipControlModuleInstance.InitialiseShip();
                    shipAIInputModuleInstance.Initialise();
                    // Set the ship to the idle state
                    if (shipAIInputModuleInstance.IsInitialised)
                    {
                        shipAIInputModuleInstance.SetState(AIState.idleStateID);
                    }
                    // Set the faction ID and squadron ID of the ship
                    shipControlModuleInstance.shipInstance.factionId = enemyFactionID;
                    shipControlModuleInstance.shipInstance.squadronId = enemyCapitalSquadronID;

                    // In the battle, movement will be disabled on the capital ships, but
                    // we still want the turrets to fire at the friendly ships (including the player)
                    shipControlModuleInstance.shipInstance.useWeaponsWhenMovementDisabled = true;

                    // Adjust the turret weapons
                    int numWeapons = shipControlModuleInstance.NumberOfWeapons;

                    for (int wpIdx = 0; wpIdx < numWeapons; wpIdx++)
                    {
                        Weapon weapon = shipControlModuleInstance.shipInstance.GetWeaponByIndex(wpIdx);
                        if (weapon != null && weapon.IsTurretWeapon && weapon.isAutoTargetingEnabled)
                        {
                            // Give the friendly ships a better chance of not getting hit by the turrets
                            weapon.turretReturnToParkInterval = 3f;
                            weapon.reloadTime = 0.7f;
                            weapon.inaccuracy = 0.5f;
                        }
                    }

                    // Disable respawning
                    shipControlModuleInstance.shipInstance.respawningMode = Ship.RespawningMode.DontRespawn;

                    // Disable the ship until the attack starts
                    shipControlModuleInstance.DisableShip(true);

                    // Store this ship in a list for future reference
                    enemyCapitalShips.Add(shipAIInputModuleInstance);
                }
            }

            #endregion
        }

        /// <summary>
        /// Given a ShipId, check if the ship is a capital ship
        /// </summary>
        /// <param name="shipID"></param>
        /// <returns></returns>
        private bool IsCapitalShip(int shipId)
        {
            bool isCapitalShip = false;

            int numEnemyCapitalShips = enemyCapitalShips == null ? 0 : enemyCapitalShips.Count;

            for (int i = 0; i < numEnemyCapitalShips; i++)
            {
                if (enemyCapitalShips[i] != null && enemyCapitalShips[i].GetShipId == shipId)
                {
                    isCapitalShip = true;
                    break;
                }
            }

            return isCapitalShip;
        }

        /// <summary>
        /// Play an audioclip at the specified world-space position at a volume.
        /// This uses the pooled EffectsModules created during initialisation.
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="audioPosition"></param>
        /// <param name="clipVolume"></param>
        private void PlaySoundFX(AudioClip audioClip, Vector3 audioPosition, float clipVolume)
        {
            // Check to see if the sound fx pool was created ok and the clip is not null.
            if (soundeffectsObjectPrefabID >= 0 && audioClip != null)
            {
                // Play the sound clip using a pool EffectsModule
                InstantiateSoundFXParameters sfxParms = new InstantiateSoundFXParameters()
                {
                    effectsObjectPrefabID = soundeffectsObjectPrefabID,
                    position = audioPosition,
                    volume = clipVolume
                };
                sscManager.InstantiateSoundFX(sfxParms, audioClip);
            }
        }

        /// <summary>
        /// Will set the quality of the game based on the current
        /// gameQuality setting.
        /// This assumes the Ship AI Input Module is already initialised on
        /// all AI ships.
        /// NOTE: In a real game we'd probably use baked lighting where possible
        /// to improve overall performance.
        /// </summary>
        private void SetGameQuality()
        {
            bool isHigh = gameQuality == GameQuality.High;
            bool isMedium = gameQuality == GameQuality.Medium;
            bool isLow = !isHigh && !isMedium;

            ReinitialiseLighting(isLow, isMedium, isHigh);

            #region Particle Effects

            // Sparks - How often are they instantiated
            sparksInterval = isHigh ? 5f : isMedium ? 8f : 30f;

            // Enemy 1 ships (Vectras) have Glow and Emission FX that may be too
            // heavy for some hardware.
            SetThrusterEffectQuality(enemyType1Ships);
            // Captial ship has Liftoff thruster FX.
            SetThrusterEffectQuality(enemyCapitalShips);

            #endregion

            #region General
            // Override the target frame rate by not using the monitor refresh rate (vSync = 1)
            // Some devices can have a high monitor refresh rate like 120 or 144.
            QualitySettings.vSyncCount = 0;
            if (limitFrameRate) { Application.targetFrameRate = isHigh || isMedium ? 60 : 30; }

            // TESTING ONLY
            //if (limitFrameRate) { Application.targetFrameRate = 60; }
            #endregion

            #region Ship Setup

            if (playerHawk != null && playerHawk.IsInitialised)
            {
                // NOTE: HUD gauges get turned on/off in HawkReadyForTakeOff()
                playerHawk.shipInstance.useCentralFuel = true;

                Thruster firstThruster = playerHawk.shipInstance.thrusterList[0];

                // Fuel consumption only happens on Medium and High
                firstThruster.fuelBurnRate = isLow ? 0f : 0.1f;

                // Engine heating only happens on Medium and High
                firstThruster.heatUpRate = isLow ? 0f : 0.6f;
                firstThruster.heatDownRate = 2f;

                // Look up the weapons. Use the zero-based index to avoid GC with the weapon name
                if (playerHawkLMissiles == null) { playerHawkLMissiles = playerHawk.shipInstance.GetWeaponByIndex(2); }
                if (playerHawkRMissiles == null) { playerHawkRMissiles = playerHawk.shipInstance.GetWeaponByIndex(3); }

                if (playerHawkLMissiles != null && playerHawkRMissiles != null)
                {
                    // Low Quality has unlimited ammo and doesn't display the gauges in the HUD
                    playerHawkLMissiles.ammunition = isLow ? -1 : 40;
                    playerHawkRMissiles.ammunition = isLow ? -1 : 40;

                    if (missilesGauge != null)
                    {
                        // When the in Medium or High Quality, left + right missile ammo is 80
                        missilesGauge.gaugeMaxValue = 80;
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// Turn on or off some thruster particle effects based on the gameQuality level.
        /// This assumes the Ship AI Input Module is already initialised on all AI ships.
        /// For this demo we want to not use particle systems that produce a Glow Effect
        /// unless the gameQuality is High.
        /// The Vectra ships have Glow child thruster effects.
        /// </summary>
        /// <param name="squadronAIList"></param>
        private void SetThrusterEffectQuality(List<ShipAIInputModule> squadronAIList)
        {
            bool isHigh = gameQuality == GameQuality.High;

            int numSquadronAIShips = squadronAIList == null ? 0 : squadronAIList.Count;

            for (int i = 0; i < numSquadronAIShips; i++)
            {
                ShipAIInputModule aiShip = squadronAIList[i];

                // If the AI ship module has been initialised, we can get the ship control module
                // without performing another GetComponent().
                if (aiShip != null && aiShip.IsInitialised)
                {
                    ShipControlModule ship = aiShip.GetShipControlModule;
                    if (ship != null && ship.IsInitialised)
                    {
                        // Look for, and enable/disable particle systems on a gameobject
                        // with "Glow" or "Emission" in their name.
                        if (isHigh)
                        {
                            ship.EnableThrusterEffects("Glow");
                            ship.EnableThrusterEffects("Emission");
                            ship.EnableThrusterEffects("Liftoff");
                        }
                        else
                        {
                            ship.DisableThrusterEffects("Glow");
                            ship.DisableThrusterEffects("Emission");
                            ship.DisableThrusterEffects("Liftoff");
                        }

                        ship.ReinitialiseThrusterEffects();
                    }
                }
            }
        }

        /// <summary>
        /// Invoked when the shuttle has docked.
        /// Also called when the attack begins to improve performance.
        /// </summary>
        private void StopAmbientAudio()
        {
            if (ambientAudioSource.clip != null && ambientAudioSource.isPlaying)
            {
                ambientAudioSource.Stop();
            }
        }

        #endregion

        #region Private and internal Methods - Lighting

        /// <summary>
        /// Fade up the main direct light as the player crosses the top deck
        /// of the space port. This helps to illuminate the other ships.
        /// The current downside is that the planet in the celestials become
        /// shiny.
        /// </summary>
        /// <returns></returns>
        private IEnumerator FadeUpDirectLight()
        {
            yield return null;

            if (mainLight != null)
            {
                // Only create once to avoid GC
                if (fadeUpLightWait == null) { fadeUpLightWait = new WaitForSeconds(0.02f); }

                float targetIntensity = isHDRP ? 0.05f : 0.8f;

                while (mainLight.intensity < targetIntensity)
                {
                    yield return fadeUpLightWait;

                    if (isHDRP)
                    {
                        SSCUtils.HDLightIntensityMultiply(hdLightType, mainLight, 1.02f);
                    }
                    else
                    {
                        mainLight.intensity += 0.001f;
                    }
                }
            }
        }

        /// <summary>
        /// Reinitialise all the point lights
        /// </summary>
        private void ReinitialiseLighting(bool isLowQuality, bool isMediumQuality, bool isHighQuality)
        {
            #if UNITY_EDITOR
            if (QualitySettings.pixelLightCount < 2)
            {
                Debug.LogWarning("TechDemo3 requires at least 2 pixel lights to provide the minimum lighting required. Check your Quality Settings.");
            }
            #endif

            if (servicesLight2 != null) { servicesLight2.enabled = isHighQuality; }

            if (isHDRP)
            {
                hdLightType = SSCUtils.GetHDLightDataType(true);

                if (hdLightType != null)
                {
                    // When a built-in scene is converted to HDRP, the lights get a default 600 Lumen value.. which seems much too bright for our setup
                    if (bottomLiftLight1 != null) { SSCUtils.SetHDLightIntensity(hdLightType, bottomLiftLight1, 0.3f); }
                    if (lift1CallLight != null)
                    {
                        // move the light a bit further away from the control panel to avoid glare in HDRP
                        lift1CallLight.transform.position += Vector3.up * 0.01f;
                        SSCUtils.SetHDLightIntensity(hdLightType, lift1CallLight, 0.001f);
                    }

                    if (entranceLight1 != null) { SSCUtils.SetHDLightIntensity(hdLightType, entranceLight1, 0.3f); }
                    if (entranceLight2 != null) { SSCUtils.SetHDLightIntensity(hdLightType, entranceLight2, 0.3f); }
                    if (servicesLight1 != null) { SSCUtils.SetHDLightIntensity(hdLightType, servicesLight1, 0.4f); }
                    if (servicesLight2 != null) { SSCUtils.SetHDLightIntensity(hdLightType, servicesLight2, 0.4f); }
                    if (corridorLeftLight1 != null) { SSCUtils.SetHDLightIntensity(hdLightType, corridorLeftLight1, 0.2f); }
                    if (corridorRightLight1 != null) { SSCUtils.SetHDLightIntensity(hdLightType, corridorRightLight1, 0.2f); }
                    if (topLift1Light1 != null) { SSCUtils.SetHDLightIntensity(hdLightType, topLift1Light1, 0.1f); }
                    if (topLift1Light2 != null) { SSCUtils.SetHDLightIntensity(hdLightType, topLift1Light2, 0.1f); }
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ERROR: TechDemo3 could not configure lights for High Definition Render Pipeline"); }
                #endif
            }
            else
            {
                // URP and built-in lighting

                if (bottomLiftLight1 != null) { bottomLiftLight1.intensity = 0.7f; }

                if (lift1CallLight != null)
                {
                    // When the point light is very close to the lift call control, the colour
                    // is washed out on URP and HDRP. Making the intensity of the light very low, fixes this.
                    lift1CallLight.intensity = isURP ? 0.01f : 0.7f;
                }
            }

            SetLightShadows(bottomLiftLight1, isLowQuality, isMediumQuality, isHighQuality);
            SetLightShadows(entranceLight1, isLowQuality, isMediumQuality, isHighQuality);
            SetLightShadows(entranceLight2, isLowQuality, isMediumQuality, isHighQuality);
            SetLightShadows(servicesLight1, isLowQuality, isMediumQuality, isHighQuality);
            SetLightShadows(servicesLight2, isLowQuality, isMediumQuality, isHighQuality);
            SetLightShadows(corridorLeftLight1, isLowQuality, isMediumQuality, isHighQuality);
            SetLightShadows(corridorRightLight1, isLowQuality, isMediumQuality, isHighQuality);
            SetLightShadows(topLift1Light1, isLowQuality, isMediumQuality, isHighQuality);
            SetLightShadows(topLift1Light2, isLowQuality, isMediumQuality, isHighQuality);

            SetLightImportance(shuttleCockpitLight, true);
            SetLightImportance(shuttleCargoBayLight, true);
            SetLightImportance(entranceLight1, false);
            SetLightImportance(entranceLight2, false);
            SetLightImportance(servicesLight1, false);
            SetLightImportance(servicesLight2, false);
            SetLightImportance(bottomLiftLight1, false);
            SetLightImportance(corridorLeftLight1, false);
            SetLightImportance(corridorRightLight1, false);
            SetLightImportance(topLift1Light1, false);
            SetLightImportance(topLift1Light2, false);
            SetLightImportance(hawkCockpitLight, false);

            // Turn hawk cockpit light off at the start of the game
            LightingHawkCockpit(false);
        }

        // When entering the space port, adjust the lighting
        private void LightingEnterSpaceport()
        {
            SetLightImportance(shuttleCargoBayLight, false);
            SetLightImportance(entranceLight2, true);

            if (gameQuality == GameQuality.Low)
            {
                // We really need the second point light, even on Low.
                SetLightImportance(servicesLight1, true);
            }
            else
            {
                // Make sure the wall poster behind the assistance is well lit.
                SetLightImportance(servicesLight1, true);
            }
        }

        /// <summary>
        /// Called when the player exits the outer doors at the top of lift1
        /// and begins walking across the top deck toward the Hawk.
        /// </summary>
        private void LightingExitOuterDoors()
        {
            // Completely turn off unneeded lighting
            if (lift1CallLight != null) { lift1CallLight.enabled = false; }
            if (entranceLight1 != null) { entranceLight1.enabled = false; }
            if (entranceLight2 != null) { entranceLight2.enabled = false; }
            if (servicesLight1 != null) { servicesLight1.enabled = false; }
            if (bottomLiftLight1 != null) { bottomLiftLight1.enabled = false; }

            SetLightImportance(servicesLight2, false);
            SetLightImportance(corridorLeftLight1, false);
            SetLightImportance(corridorRightLight1, false);
            SetLightImportance(topLift1Light2, false);

            LightingHawkCockpit(true);
        }

        /// <summary>
        /// Called from SSCDoor2_Lift1_Access SSCDoorAnimator onOpening event
        /// </summary>
        public void LightingExitServices()
        {
            SetLightImportance(servicesLight1, false);
            SetLightImportance(entranceLight2, false);
            SetLightImportance(bottomLiftLight1, true);

            // Highlight the lift1 control panel
            if (lift1CallLight != null) { lift1CallLight.enabled = true; }
        }

        /// <summary>
        /// Enable or disable the cockpit light in the player Hawk ship on the top deck
        /// </summary>
        /// <param name="isEnabled"></param>
        private void LightingHawkCockpit(bool isEnabled)
        {
            if (hawkCockpitLight != null)
            {
                hawkCockpitLight.enabled = isEnabled;

                if (isEnabled)
                {
                    SetLightImportance(hawkCockpitLight, gameQuality != GameQuality.Low);
                }
            }
        }

        /// <summary>
        /// Set the importance of a light. In Unity 2019.4 with realtime point lights, Unity often
        /// seems to pick the wrong "important" light, and our signage is often not illuminated.
        /// </summary>
        /// <param name="light"></param>
        /// <param name="isImportant"></param>
        private void SetLightImportance(Light light, bool isImportant, bool isAuto = false)
        {
            if (light != null)
            {
                light.renderMode = isAuto ? LightRenderMode.Auto : isImportant ? LightRenderMode.ForcePixel : LightRenderMode.ForceVertex;
            }
        }

        /// <summary>
        /// Configure the light based on the game quality level
        /// </summary>
        /// <param name="light"></param>
        /// <param name="isLow"></param>
        /// <param name="isMedium"></param>
        /// <param name="isHigh"></param>
        private void SetLightShadows(Light light, bool isLow, bool isMedium, bool isHigh)
        {
            if (light != null)
            {
                light.shadows = isLow ? LightShadows.None : isMedium ? LightShadows.Hard : LightShadows.Soft;
                light.shadowStrength = 0.15f;
            }
        }

        #endregion

        #region Private Methods - Player ships

        /// <summary>
        /// Configure the player ships (Shuttle and the Hawk) for the mini-game.
        /// Assume player Hawk and shuttle have InitialiseOnAwake already set in the editor.
        /// </summary>
        private void ConfigurePlayerShips()
        {
            // Get some references
            shuttlePlayerInputModule = playerShuttle.GetComponent<PlayerInputModule>();
            hawkPlayerInputModule = playerHawk.GetComponent<PlayerInputModule>();
            hawkPlayerAutoTargetingModule = playerHawk.GetComponent<AutoTargetingModule>();
            changeShipCameraView = shipCamera.GetComponent<SampleChangeCameraView>();

            #if UNITY_EDITOR
            if (shuttlePlayerInputModule == null)
            {
                Debug.LogWarning("ERROR: TechDemo3 could not find PlayerInputModule component on player ship: " + playerShuttle.name);
            }

            if (hawkPlayerAutoTargetingModule == null)
            {
                Debug.LogWarning("ERROR: TechDemo3 could not find AutoTargetingModule component on player ship: " + playerHawk.name);
            }

            if (hawkPlayerInputModule == null)
            {
                Debug.LogWarning("ERROR: TechDemo3 could not find PlayerInputModule component on player ship: " + playerHawk.name);
            }

            if (changeShipCameraView == null)
            {
                Debug.LogWarning("ERROR: TechDemo3 could not find SampleChangeCameraView component on " + shipCamera.name);
            }
            #endif

            shipDocking = playerShuttle.GetShipDocking(true);

            if (shipDocking != null)
            {
                shipDocking.callbackOnStateChange = OnDockingStateChanged;
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: TechDemo3 could not find ShipDocking component on shuttle ship");
            }
            #endif

            // Identify the player ships so we don't get attacked by friendly ships
            playerHawk.shipInstance.factionId = friendlyFactionID;
            playerShuttle.shipInstance.factionId = friendlyFactionID;
            playerHawk.shipInstance.squadronId = playerSquadronID;

            // To begin with, both ships shouldn't be visible to (enemy) radar.
            playerHawk.DisableRadar();
            playerShuttle.DisableRadar();

            // This enables us to turn off the Heads up display when the Hawk is destroyed
            // We could also do things like update scores or end the mission etc.
            playerHawk.callbackOnDestroy = OnPlayerDestroyed;

            // This enables us to turn the HUD on again after the player Hawk ship has been respawned
            playerHawk.callbackOnRespawn = OnPlayerRespawnCallback;

            // Prevent the ship being moved by flying debris
            playerHawk.ShipRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        /// <summary>
        /// Called from HawkCockpitEntry()
        /// </summary>
        private void HawkPrepareForTakeOff()
        {
            if (playerHawk != null)
            {
                playerHawk.ShipRigidbody.constraints = RigidbodyConstraints.None;

                playerHawk.EnableShip(true, true);

                float delayTakeoff = 1f;

                // Flicker and turn on the SSC HUD
                if (shipDisplayModule != null)
                {
                    shipDisplayModule.lockDisplayReticleToCursor = false;
                    // Switch to the cross-hairs reticle
                    int guidHashReticle = shipDisplayModule.GetDisplayReticleGuidHash(0);
                    shipDisplayModule.ChangeDisplayReticle(guidHashReticle);

                    // Reset the reticle back to the centre of the screen
                    shipDisplayModule.SetDisplayReticleOffset(Vector2.zero);

                    shipDisplayModule.HideDisplayMessages();
                    shipDisplayModule.ShowOverlay();
                    shipDisplayModule.ShowHeading();
                    shipDisplayModule.ShowAttitude();
                    shipDisplayModule.FlickerOn(shipDisplayModule.flickerDefaultDuration);

                    // Delay takeoff for the duration that the HUD is flickering
                    delayTakeoff = shipDisplayModule.flickerDefaultDuration;

                    // TODO I've modified this
                    //shipDisplayModule.HideCursor();
                    shipDisplayModule.ShowCursor();
                }

                Invoke("HawkReadyForTakeOff", delayTakeoff);
            }
        }

        /// <summary>
        /// Permit the player to control the Hawk and engage the enemy ships
        /// </summary>
        private void HawkReadyForTakeOff()
        {
            if (playerHawk != null && hawkPlayerInputModule != null)
            {
                // Enable Hawk Input and permit to take off
                hawkPlayerInputModule.EnableInput();

                if (shipDisplayModule != null)
                {
                    shipDisplayModule.sourceShip = playerHawk;
                    shipDisplayModule.ShowDisplayReticle();
                    shipDisplayModule.ShowAirSpeed();
                    shipDisplayModule.ShowAltitude();

                    // Only show Hawk metric on HUD if Medium or High quality UNLESS overridden in inspector
                    // See also SetGameQuality()
                    isUpdateHawkHUDMetrics = hawkFullHUD || gameQuality != GameQuality.Low;

                    if (isUpdateHawkHUDMetrics)
                    {
                        shipDisplayModule.ShowDisplayGauge(fuelLevelGauge);
                        shipDisplayModule.ShowDisplayGauge(heatLevelGauge);                        
                        shipDisplayModule.ShowDisplayGauge(healthGauge);
                        
                        if (gameQuality != GameQuality.Low)
                        {
                            shipDisplayModule.ShowDisplayGauge(missilesGauge);
                            shipDisplayModule.ShowDisplayGauge(shieldsGauge);
                            shipDisplayModule.ShowDisplayGauge(launchGauge);
                            shipDisplayModule.ShowDisplayGauge(gearGauge);
                            shipDisplayModule.ShowDisplayGauge(enemyGauge);
                        }
                    }
                }

                // Apply some up and forward thrust to help the player take off
                playerHawk.shipInstance.AddBoost(new Vector3(0f,1f,0.1f), 300000f, 5f);

                // Raise landing gear


                if (hawkPlayerAutoTargetingModule != null) { hawkPlayerAutoTargetingModule.Initialise(); }

                // Give the player a chance to take off, then make them visible to enemy radar
                Invoke("HawkDelayedRadar", 5f);
            }
        }

        /// <summary>
        /// Make the player ship visible to radar and vulnerable to enemy attack.
        /// Make the player ship vincible to damage.
        /// </summary>
        private void HawkDelayedRadar()
        {
            playerHawk.shipInstance.MakeShipVincible();
            //playerHawk.shipInstance.mainDamageRegion.shieldingDamageThreshold = 10f;

            // Make the player visible to radar and vulnerable to enemy attack
            playerHawk.EnableRadar();

            if (sscRadar != null) { sscRadar.ShowUI(); }
        }

        #endregion

        #region Private UI Methods

        /// <summary>
        /// Show or hide the Menu in the scene.
        /// Enable the cursor when the menu is shown
        /// Disable the cursor immediately when the menu is disabled.
        /// </summary>
        /// <param name="isVisible"></param>
        private void ShowMenu(bool isVisible)
        {
            if (menuPanel != null) { menuPanel.SetActive(isVisible); }
            if (shipDisplayModule != null)
            {
                if (isVisible || !shipDisplayModule.autoHideCursor) { shipDisplayModule.ShowCursor(); } else { shipDisplayModule.HideCursor(); }
            }
        }

        /// <summary>
        /// Show or hide the Quality settings in the scene.
        /// Note, this is a child of the Menu panel.
        /// </summary>
        /// <param name="isVisible"></param>
        private void ShowQuality(bool isVisible)
        {
            if (isVisible) { HighlightQuality(); }
            if (qualityPanel != null) { qualityPanel.SetActive(isVisible); }
        }

        /// <summary>
        /// Highlight the button border of the current game quality
        /// </summary>
        private void HighlightQuality()
        {
            if (qualityLowButton != null && qualityMedButton != null && qualityHighButton != null)
            {
                if (gameQuality == GameQuality.Low)
                {
                    qualityLowBorderImg.color = colourSelectedBorder;
                    qualityMedBorderImg.color = colourDefaultBorder;
                    qualityHighBorderImg.color = colourDefaultBorder;
                }
                else if (gameQuality == GameQuality.Medium)
                {
                    qualityLowBorderImg.color = colourDefaultBorder;
                    qualityMedBorderImg.color = colourSelectedBorder;
                    qualityHighBorderImg.color = colourDefaultBorder;
                }
                else
                {
                    qualityLowBorderImg.color = colourDefaultBorder;
                    qualityMedBorderImg.color = colourDefaultBorder;
                    qualityHighBorderImg.color = colourSelectedBorder;
                }
            }
        }

        /// <summary>
        /// Show or hide the Game Over panel and text in the scene
        /// </summary>
        /// <param name="isVisible"></param>
        /// <param name="isMissionSuccessful"></param>
        private void ShowGameOver(bool isVisible, bool isMissionSuccessful)
        {
            if (missionOverPanel != null)
            {
                if (isVisible)
                {
                    if (isMissionSuccessful)
                    {
                        if (missionSuccessTitle != null) { missionSuccessTitle.SetActive(true); }
                        if (missionSuccessSubTitle != null) { missionSuccessSubTitle.SetActive(true); }
                        if (missionFailedTitle != null) { missionFailedTitle.SetActive(false); }
                        if (missionFailedSubTitle != null) { missionFailedSubTitle.SetActive(false); }
                    }
                    else
                    {
                        if (missionSuccessTitle != null) { missionSuccessTitle.SetActive(false); }
                        if (missionSuccessSubTitle != null) { missionSuccessSubTitle.SetActive(false); }
                        if (missionFailedTitle != null) { missionFailedTitle.SetActive(true); }
                        if (missionFailedSubTitle != null) { missionFailedSubTitle.SetActive(true); }
                    }
                }
                missionOverPanel.SetActive(isVisible);
            }
        }

        /// <summary>
        /// Show or hide a button
        /// </summary>
        /// <param name="button"></param>
        /// <param name="isVisible"></param>
        private void ShowButton(Button button, bool isVisible)
        {
            if (button != null) { button.gameObject.SetActive(isVisible); }
        }

        /// <summary>
        /// Set the focus to a button in the UI
        /// </summary>
        /// <param name="button"></param>
        private void SetButtonFocus(Button button)
        {
            if (button != null && eventSystem != null)
            {
                // If the button was already selected when button was last enabled,
                // when it is made active again it doesn't appear in it's selected state (grey)
                // However, the object is actually selected. So unselect it, then reselect it.
                eventSystem.SetSelectedGameObject(null);
                eventSystem.SetSelectedGameObject(button.gameObject);
            }
        }

        /// <summary>
        /// The button has been selected or deselected.
        /// UI.Text is stored as a reference in the scene to avoid having to do
        /// a GetComponentsInChildren.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="text"></param>
        /// <param name="isSelected"></param>
        private void SelectButton(Button button, Text text, bool isSelected)
        {
            if (button != null && text != null)
            {
                text.color = isSelected ? colourSelectedBtnText : colourNonSelectedBtnText;
            }
        }

        /// <summary>
        /// Wait until the player camera is in the correct position,
        /// pause updates, then show the Start options.
        /// Invoked from Start() when scene loads.
        /// </summary>
        /// <returns></returns>
        //private System.Collections.IEnumerator ShowStart()
        private void ShowStart()
        {
            ShowMenu(false);

            PauseGame();

            // Configure ambient sound while in shuttle
            ambientAudioSource.clip = shuttleAmbientClip;
            ambientAudioSource.volume = 0.1f;
            // Lower priority
            ambientAudioSource.priority = 64;

            ShowButton(resumeButton, false);
            ShowButton(startButton, true);
            ShowButton(restartButton, false);
            ShowButton(quitButton, true);
            ShowQuality(true);
            ShowMenu(true);
            SetButtonFocus(startButton);
            System.GC.Collect();
        }

        /// <summary>
        /// The mission failed so pause game and tell the user
        /// </summary>
        private void MissionFailed()
        {
            PauseGame();
            ShowGameOver(true, false);
            ShowButton(resumeButton, false);
            ShowButton(quitButton, true);
            SetButtonFocus(restartButton);
        }

        /// <summary>
        /// The mission was successful, so pause game and tell the user
        /// </summary>
        private void MissionCompleted()
        {
            PauseGame();
            ShowGameOver(true, true);
            ShowButton(resumeButton, false);
            ShowButton(quitButton, true);
            SetButtonFocus(restartButton);
        }

        #endregion

        #region Private Game Pause Methods

        /// <summary>
        /// Add all the AI Ships in a squadron to the list of ships to pause and unpause
        /// </summary>
        /// <param name="numSquadronAIShips"></param>
        /// <param name="squadronAIList"></param>
        private void AddSquadronToPausedList(int numSquadronAIShips, List<ShipAIInputModule> squadronAIList)
        {
            for (int i = 0; i < numSquadronAIShips; i++)
            {
                ShipAIInputModule aiShip = squadronAIList[i];

                if (aiShip != null && aiShip.IsInitialised)
                {
                    ShipControlModule ship = aiShip.GetShipControlModule;
                    if (ship != null && ship.IsInitialised)
                    {
                        // Only record ships that are either respawning or are currently enabled.
                        if (ship.IsRespawning)
                        {
                            pausedAIShips.Add(aiShip);
                            ship.PauseRespawning();
                        }
                        else if (ship.ShipIsEnabled())
                        {
                            pausedAIShips.Add(aiShip);
                            ship.DisableShip(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Pause the game by:
        /// 1. Pausing the camera
        /// 2. Pausing the player ship
        /// 3. Pausing all AI squadrons
        /// 4. Pausing the Capital Ship if it is in the scene
        /// 5. Pause the Sticky3D player
        /// 6. Pause the Sticky3D assistant
        /// </summary>
        private void PauseGame()
        {
            #if SCSM_S3D
            if (s3dPlayer != null && !useWalkThru)
            {
                // If look is enabled, so must be the player character
                isPlayerEnabledOnPause = s3dPlayer.IsLookEnabled;
                if (isPlayerEnabledOnPause)
                {
                    s3dPlayer.PauseCharacter();
                }
            }
            if (s3dServiceAssistant != null)
            {
                s3dServiceAssistant.DisableCharacter();
            }
            #endif

            // Assume the player ship is always enabled.
            if (!attackStarted && playerShuttle != null && playerShuttle.IsInitialised)
            {
                playerShuttle.DisableShip(false);

                if (useWalkThru) { shuttlePlayerInputModule.DisableInput(false); }
            }

            // If this is the first time paused, reserve enough capacity for all the AI Ships
            int numFriendlyShips = friendlyShips == null ? 0 : friendlyShips.Count;
            int numEnemyType1Ships = enemyType1Ships == null ? 0 : enemyType1Ships.Count;
            int numEnemyType2Ships = enemyType2Ships == null ? 0 : enemyType2Ships.Count;
            int numEnemyCapitalShips = enemyCapitalShips == null ? 0 : enemyCapitalShips.Count;

            if (pausedAIShips == null) { pausedAIShips = new List<ShipAIInputModule>(numFriendlyShips + numEnemyType1Ships + numEnemyType2Ships + numEnemyCapitalShips); }

            AddSquadronToPausedList(numFriendlyShips, friendlyShips);
            AddSquadronToPausedList(numEnemyType1Ships, enemyType1Ships);
            AddSquadronToPausedList(numEnemyType2Ships, enemyType2Ships);
            AddSquadronToPausedList(numEnemyCapitalShips, enemyCapitalShips);

            // Pause ambient audio
            if (ambientAudioSource.clip != null && ambientAudioSource.isPlaying)
            {
                ambientAudioSource.Pause();
            }

            // Pause game music (if any)
            if (musicController != null) { musicController.PauseMusic(); }
            AudioListener.pause = true;

            // Pause Beams, Destructs, Projectiles and effects
            sscManager.PauseBeams();
            sscManager.PauseDestructs();
            sscManager.PauseProjectiles();
            sscManager.PauseEffectsObjects();

            // turn off the heads-up display
            if (shipDisplayModule != null) { shipDisplayModule.HideHUD(); }

            // Change the UI
            ShowButton(resumeButton, true);
            ShowButton(startButton, false);
            ShowButton(restartButton, true);
            ShowButton(quitButton, true);
            ShowQuality(false);
            ShowMenu(true);
            SetButtonFocus(resumeButton);

            Time.timeScale = 0f;

            isGamePaused = true;
        }

        /// <summary>
        /// Set the game to be unpaused during the next frame.
        /// When timeScale has been set to 0, ships and camera modules
        /// will error with NaN if we attempt to set the timeScale to
        /// non-zero and then immediately try to calculate moment and
        /// force.
        /// </summary>
        private IEnumerator UnPauseGameNextFrame()
        {
            Time.timeScale = 1f;
            yield return new WaitForEndOfFrame();
            ShowMenu(false);
            UnPauseGame();
        }

        /// <summary>
        /// Unpause the game by:
        /// 1. Unpausing beams, destructs, projectiles and effects objects
        /// 2. Unpausing the player ship
        /// 3. Unpausing the Capital Ship if it is in the scene
        /// 4. Unpausing all AI squadrons
        /// 5. Unpausing the camera
        /// 6. Unpause the Sticky3D assistant
        /// 7. Unpause the Sticky3D player
        /// NOTE: We don't want to do this in the same frame
        /// that timeScale was changed from 0 to say 1.0.
        /// </summary>
        private void UnPauseGame()
        {
            // Unpause beams, destructs, projectiles and effects
            sscManager.ResumeBeams();
            sscManager.ResumeDestructs();
            sscManager.ResumeProjectiles();
            sscManager.ResumeEffectsObjects();

            if (!attackStarted && playerShuttle != null && playerShuttle.IsInitialised)
            {
                playerShuttle.EnableShip(false, false);
            }

            int numPauseAIShips = pausedAIShips == null ? 0 : pausedAIShips.Count;

            for (int i = 0; i < numPauseAIShips; i++)
            {
                ShipAIInputModule aiShip = pausedAIShips[i];

                if (aiShip != null && aiShip.IsInitialised)
                {
                    ShipControlModule shipControlModule = aiShip.GetShipControlModule;
                    if (shipControlModule != null && shipControlModule.IsInitialised)
                    {
                        if (shipControlModule.IsRespawning) { shipControlModule.ResumeRespawning(); }
                        else
                        {
                            if (attackStarted)
                            {
                                shipControlModule.EnableShip(false, false);

                                // Is this a capital ship?
                                if (IsCapitalShip(shipControlModule.GetShipId))
                                {
                                    shipControlModule.DisableShipMovement();
                                    shipControlModule.ShipRigidbody.detectCollisions = true;
                                }

                                // Check if any AI ships are still docked in one of the hangers.
                                // Currently we don't really need this for TechDemo3.
                                if (shipControlModule.GetShipDocking(false).GetStateInt() == ShipDocking.dockedInt)
                                {
                                    // Re-dock the ship after EnableShip(..) was called.
                                    shipControlModule.GetShipDocking(false).SetState(ShipDocking.DockingState.Docked);
                                }
                            }
                        }
                    }
                }
            }

            // Unpause game music (if any)
            if (musicController != null) { musicController.ResumeMusic(); }

            // Unpause ambient audio if required
            // Probably should also check if still in shuttle...
            if (ambientAudioSource.clip != null && !attackStarted)
            {
                ambientAudioSource.Play();
            }

            AudioListener.pause = false;

            #if SCSM_S3D
            if (s3dServiceAssistant != null) { s3dServiceAssistant.EnableCharacter(false); }
            if (s3dPlayer != null && isPlayerEnabledOnPause) { s3dPlayer.EnableCharacter(false); }
            #endif

            if (useWalkThru && playerShuttle != null && playerShuttle.IsInitialised && !playerShuttle.shipInstance.Destroyed() && shuttlePlayerInputModule != null)
            {
                // Allow custom inputs only from the shuttle while in walk thru mode.
                shuttlePlayerInputModule.EnableInput();
                shuttlePlayerInputModule.DisableInput(true);
            }

            if (shipDisplayModule != null)
            {
                shipDisplayModule.ShowHUD();
                shipDisplayModule.HideCursor();
            }

            if (numPauseAIShips > 0) { pausedAIShips.Clear(); }

            isGamePaused = false;
        }

        #endregion

        #region Private Walk Thru Member Methods

        /// <summary>
        /// Configure the walk through camera and turn it on or off.
        /// There are 2 walk thru cameras. A "normal" Unity camera and
        /// a SSC camera. The SSC camera is configured to follow a camera "ship"
        /// which can follow a path etc.
        /// </summary>
        /// <param name="isEnabled"></param>
        private void ConfigureWalkThrough(bool isEnabled)
        {
            /// Set up a camera walk through for the scene
            if (walkThruCamera1 != null)
            {
                if (isEnabled)
                {
                    if (cameraShip != null && walkThruCamera2 != null && sscManager != null)
                    {
                        walkThruCamera2.SetTarget(cameraShip);

                        cameraShipAI = cameraShip.GetShipAIInputModule(true);

                        walkThruPath1 = sscManager.GetPath(walkThruPath1Name);
                        walkThruPath2 = sscManager.GetPath(walkThruPath2Name);
                        walkThruPath3 = sscManager.GetPath(walkThruPath3Name);
                        walkThruPath4 = sscManager.GetPath(walkThruPath4Name);
                        walkThruPath5 = sscManager.GetPath(walkThruPath5Name);
                        walkThruPath6 = sscManager.GetPath(walkThruPath6Name);

                        #if UNITY_EDITOR
                        if (cameraShipAI == null) { Debug.LogWarning("TechDemo3 - " + cameraShip.name + " needs a Ship AI Input Module"); }
                        if (walkThruPath1 == null) { Debug.LogWarning("TechDemo3 - could not find a path in SSCManager called " + walkThruPath1Name); }
                        if (walkThruPath2 == null) { Debug.LogWarning("TechDemo3 - could not find a path in SSCManager called " + walkThruPath2Name); }
                        if (walkThruPath3 == null) { Debug.LogWarning("TechDemo3 - could not find a path in SSCManager called " + walkThruPath3Name); }
                        if (walkThruPath4 == null) { Debug.LogWarning("TechDemo3 - could not find a path in SSCManager called " + walkThruPath4Name); }
                        if (walkThruPath5 == null) { Debug.LogWarning("TechDemo3 - could not find a path in SSCManager called " + walkThruPath5Name); }
                        if (walkThruPath6 == null) { Debug.LogWarning("TechDemo3 - could not find a path in SSCManager called " + walkThruPath6Name); }
                        #endif

                        if (cameraShipAI != null) { cameraShipAI.Initialise(); }

                        // We want to move the path inside the shuttle during gameplay (ie make it dynamic), so we need to initialise it (for movement).
                        sscManager.InitialiseLocations(walkThruPath1, shuttleOriginalPos, Vector3.zero, Quaternion.identity, Vector3.zero, Vector3.zero);

                        cameraShip.DisableShip(true);

                        isWalkThruInitialised = cameraShipAI != null && walkThruPath1 != null && walkThruPath2 != null && walkThruPath3 != null &&
                                                walkThruPath4 != null && walkThruPath5 != null && walkThruPath6 != null && cockPitEntryProximity != null;
                    }

                    // For walk thru, when the cameraShip is disabled inside the canopy door proximity sphere collider,
                    // it will close the Hawk canopy, which is not what we want.
                    if (hawkCanopyDoorProximity != null) { hawkCanopyDoorProximity.isCloseDoorsOnExit = false; }

                    // Set the start position of the camera
                    if (shuttlePilotChair != null)
                    {
                        // Move the camera to the pilot chair.
                        Vector3 newPosition = shuttlePilotChair.transform.position + (shuttlePilotChair.transform.up * 1.2f);

                        walkThruCamera1.transform.SetPositionAndRotation(newPosition, shuttlePilotChair.transform.rotation);

                        // In the shuttle while docking, we want the camera to be locked to the pilot chair.
                        walkThruCamera1.transform.SetParent(shuttlePilotChair.transform);
                    }

                    if (celestials != null) { celestials.camera1 = walkThruCamera1; celestials.RefreshCameras(); }

                    if (shipDisplayModule != null)
                    {
                        shipDisplayModule.SetCamera(walkThruCamera1);
                        shipDisplayModule.Initialise();
                        shipDisplayModule.HideCursor();
                    }
                }

                // Turn camera on/off
                walkThruCamera1.enabled = isEnabled;
                walkThruCamera1.gameObject.SetActive(isEnabled);

            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR TechDemo3 - could not find WalkThru Camera, check this component in the scene");
            }
            #endif
        }

        /// <summary>
        /// Walk (fly) from the pilot seat to the shuttle side door
        /// </summary>
        private void WalkThruStartSection2()
        {
            if (isWalkThruInitialised)
            {
                Vector3 deltaPathPos = playerShuttle.transform.position - shuttleOriginalPos;
                Quaternion deltaPathRot = playerShuttle.transform.rotation * Quaternion.Inverse(shuttleOriginalRot);

                // Move the path locations to match the new position of the shuttle. Although the shuttle is not
                // a mothership (with a docking station module etc), we can use the same API to move the locations.
                sscManager.MoveLocations(walkThruPath1, Time.time, playerShuttle.transform.position, deltaPathPos, deltaPathRot, Vector3.zero, Vector3.zero);

                cameraShip.EnableShip(false, true);

                LocationData locationData = sscManager.GetFirstLocation(walkThruPath1);

                if (locationData != null)
                {
                    // Move our AI-driven "ship" to the start of the path inside the shuttle
                    cameraShip.TelePort(locationData.position, walkThruCamera1.transform.rotation, true);

                    // Move the camera that will be following our camera "ship".
                    walkThruCamera2.MoveTo(walkThruCamera1.transform.position, walkThruCamera1.transform.rotation.eulerAngles);
                }

                cameraShipAI.SetState(AIState.moveToStateID);
                cameraShipAI.AssignTargetPath(walkThruPath1);
                cameraShipAI.callbackCompletedStateAction = WalkThruFinishedPath1;
                walkThruSectionNumber = 2;

                walkThruCamera1.enabled = false;
                walkThruCamera1.gameObject.SetActive(false);

                // Move the non-ship camera back under the original gameobject so it isn't attached to the shuttle
                walkThruCamera1.transform.SetParent(walkThruCamera2.transform.parent);

                walkThruCamera2.StartCamera();
                walkThruCamera2.EnableCamera();

                // Tell the stars background we've changed cameras
                if (celestials != null) { celestials.camera1 = walkThruCamera2.GetCamera1; }

                // Tell the HUD we've changed cameras
                if (shipDisplayModule != null)
                {
                    shipDisplayModule.SetCamera(walkThruCamera2.GetCamera1);
                }
            }
        }

        /// <summary>
        /// Open the shuttle side door
        /// Start "walking" towards the services room
        /// </summary>
        private void WalkThruStartSection3()
        {
            SSCDoorControl.SelectOpen(sscDoorControlShuttleSideDoors);

            StartSpacePortAmbientFX();
            LightingEnterSpaceport();

            if (shipDisplayModule != null)
            {
                shipDisplayModule.HideDisplayGauge(dockedIndicator);
                shipDisplayModule.HideDisplayMessage(exitShuttleMessage);
                shipDisplayModule.ShowDisplayMessage(findServiceDeskMessage);
            }
            cameraShipAI.maxSpeed = 1.3f;

            cameraShipAI.SetState(AIState.moveToStateID);
            cameraShipAI.AssignTargetPath(walkThruPath2);
            cameraShipAI.callbackCompletedStateAction = WalkThruFinishedPath2;

            walkThruSectionNumber = 3; 
        }

        /// <summary>
        /// We have reached the services room
        /// </summary>
        private void WalkThruStartSection4()
        {
            cameraShipAI.SetState(AIState.moveToStateID);
            cameraShipAI.AssignTargetPath(walkThruPath3);
            cameraShipAI.callbackCompletedStateAction = WalkThruFinishedPath3;

            walkThruSectionNumber = 4;
        }

        /// <summary>
        /// We have reached the services assistant area
        /// </summary>
        private void WalkThruStartSection5()
        {
            cameraShipAI.maxSpeed = 1.3f;
            cameraShipAI.SetState(AIState.moveToStateID);
            cameraShipAI.AssignTargetPath(walkThruPath4);
            cameraShipAI.callbackCompletedStateAction = WalkThruFinishedPath4;

            walkThruSectionNumber = 5;
        }

        /// <summary>
        /// We have reached the call lift area
        /// </summary>
        private void WalkThruStartSection6()
        {
            // Call the lift
            if (lift1ControlChangeMaterial != null) { lift1ControlChangeMaterial.GetGroup1Material(1); }
            if (lift1MovingPlatform != null) { lift1MovingPlatform.CallToStartPosition(true); }

            walkThruSectionNumber = 6;
        }

        /// <summary>
        /// The lift has arrived and now the "player" needs to take the lift
        /// to the upper deck.
        /// </summary>
        private void WalkThruStartSection7()
        {
            cameraShipAI.maxSpeed = 1.3f;
            cameraShipAI.SetState(AIState.moveToStateID);
            cameraShipAI.AssignTargetPath(walkThruPath5);
            cameraShipAI.callbackCompletedStateAction = WalkThruFinishedPath5;

            walkThruSectionNumber = 7;
        }

        /// <summary>
        /// Ride the lift to the upper deck.
        /// </summary>
        private void WalkThruStartSection8()
        {
            // Switch cameras
            walkThruCamera1.transform.SetPositionAndRotation(walkThruCamera2.transform.position, walkThruCamera2.transform.rotation);

            cameraShip.DisableShip(true);
            walkThruCamera2.StopCamera();

            // parent camera to lift
            walkThruCamera1.transform.SetParent(lift1MovingPlatform.transform);

            // Start the camera rendering
            walkThruCamera1.enabled = true;
            walkThruCamera1.gameObject.SetActive(true);

            if (shipDisplayModule != null) { shipDisplayModule.SetCamera(walkThruCamera1); }
            if (celestials != null) { celestials.camera1 = walkThruCamera1; }

            walkThruSectionNumber = 8;
        }

        /// <summary>
        /// Arrived at the top of the lift. This is called each time lift1 gets
        /// to the top. It is configured in the editor for the lift moving platform (SSCLiftPlate1).
        /// It needs to be public so it can be configured in the editor.
        /// Head towards the Hawk ship.
        /// See also StartBombingRuns() which makes the camera ship move more rapidly.
        /// </summary>
        public void WalkThruStartSection9()
        {
            if (useWalkThru && walkThruSectionNumber == 8)
            {
                LocationData locationData = sscManager.GetFirstLocation(walkThruPath6);

                if (locationData != null)
                {
                    cameraShip.EnableShip(false, true);

                    // Move our AI-driven "ship" to the start of the path inside the shuttle
                    cameraShip.TelePort(locationData.position, walkThruCamera1.transform.rotation, true);

                    // Move the camera that will be following our camera "ship".
                    walkThruCamera2.MoveTo(walkThruCamera1.transform.position, walkThruCamera1.transform.rotation.eulerAngles);

                    // Move the camera slowly between the top of lift and the outer doors.
                    // Give the outer doors enough time to open, especially on slower hardware - which can present path following issues
                    cameraShipAI.maxSpeed = 0.5f;
                    cameraShipAI.SetState(AIState.moveToStateID);
                    cameraShipAI.AssignTargetPath(walkThruPath6);
                    cameraShipAI.callbackCompletedStateAction = WalkThruFinishedPath6;
                    walkThruSectionNumber = 9;

                    walkThruCamera1.enabled = false;
                    walkThruCamera1.gameObject.SetActive(false);

                    // Move the non-ship camera back under the original gameobject so it isn't attached to the shuttle
                    walkThruCamera1.transform.SetParent(walkThruCamera2.transform.parent);

                    walkThruCamera2.StartCamera();
                    walkThruCamera2.EnableCamera();

                    // Tell the stars background we've changed cameras
                    if (celestials != null) { celestials.camera1 = walkThruCamera2.GetCamera1; }

                    // Tell the HUD we've changed cameras
                    if (shipDisplayModule != null) { shipDisplayModule.SetCamera(walkThruCamera2.GetCamera1); }
                }
            }
        }

        /// <summary>
        /// At the ladder of the Hawk
        /// </summary>
        private void WalkThruStartSection10()
        {
            // Switch cameras
            walkThruCamera1.transform.SetPositionAndRotation(walkThruCamera2.transform.position, walkThruCamera2.transform.rotation);

            cameraShip.DisableShip(true);
            walkThruCamera2.StopCamera();

            // Start the camera rendering
            walkThruCamera1.enabled = true;
            walkThruCamera1.gameObject.SetActive(true);

            if (shipDisplayModule != null)
            {
                shipDisplayModule.SetCamera(walkThruCamera1);
                shipDisplayModule.HideCursor();
            }

            if (celestials != null) { celestials.camera1 = walkThruCamera1; }

            walkThruSectionNumber = 10;
        }

        /// <summary>
        /// "Player" is seated in shuttle pilot seat, waiting for docking
        /// to be completed.
        /// </summary>
        private void WalkThruUpdateSection1()
        {
            Vector3 lookDirection = (assistantChair.transform.position - walkThruCamera1.transform.position).normalized;

            // Keep the camera relatively flat or aligned with the shuttle
            lookDirection = Vector3.ProjectOnPlane(lookDirection, playerShuttle.shipInstance.TransformUp);

            Quaternion lookRot = Quaternion.LookRotation(lookDirection, playerShuttle.shipInstance.TransformUp);

            walkThruCamera1.transform.rotation = Quaternion.RotateTowards(walkThruCamera1.transform.rotation, lookRot, 0.2f);
        }

        /// <summary>
        /// Rotate the "player" as they travel up the lift
        /// </summary>
        private void WalkThruUpdateSection8()
        {
            Quaternion lookRot = Quaternion.LookRotation(Vector3.back, Vector3.up);

            walkThruCamera1.transform.rotation = Quaternion.RotateTowards(walkThruCamera1.transform.rotation, lookRot, 0.5f);
        }

        /// <summary>
        /// Climb the ladder into the Hawk.
        /// Check if the camera is near the Hawk cockpit and ready for entry.
        /// </summary>
        private void WalkThruUpdateSection10()
        {
            Vector3 _camPosition = walkThruCamera1.transform.position;

            // Have we reached the top of the ladder?
            if (_camPosition.y > 35.2f)
            {
                // Move toward the cockpit at the top of the ladder.
                _camPosition += walkThruCamera1.transform.forward * Time.deltaTime * 1.5f;
            }
            else { _camPosition.y += Time.deltaTime * 0.8f; }

            walkThruCamera1.transform.position = _camPosition;

            // Face towards the cockpit entry point
            Quaternion lookRot = Quaternion.LookRotation(ladder1.transform.forward * -1f, Vector3.up);

            walkThruCamera1.transform.rotation = Quaternion.RotateTowards(walkThruCamera1.transform.rotation, lookRot, 0.3f);

            // Check if the camera is near the Hawk cockpit and ready for entry.
            // At this point we're not using a moving rigidbody so check if the camera is within the trigger area.
            if (cockPitEntryProximity.ProximityRegion.Contains(_camPosition))
            {
                HawkCockpitEntry();
                cockPitEntryProximity.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Public Walk Thru Member Methods

        public void WalkThruFinishedPath1(ShipAIInputModule shipAIInputModule)
        {
            cameraShipAI.SetState(AIState.idleStateID);
            cameraShipAI.callbackCompletedStateAction = null;
            WalkThruStartSection3();
        }

        public void WalkThruFinishedPath2(ShipAIInputModule shipAIInputModule)
        {
            cameraShipAI.SetState(AIState.idleStateID);
            cameraShipAI.callbackCompletedStateAction = null;
            WalkThruStartSection4();
        }

        public void WalkThruFinishedPath3(ShipAIInputModule shipAIInputModule)
        {
            cameraShipAI.SetState(AIState.idleStateID);
            cameraShipAI.callbackCompletedStateAction = null;
            WalkThruStartSection5();
        }

        public void WalkThruFinishedPath4(ShipAIInputModule shipAIInputModule)
        {
            cameraShipAI.SetState(AIState.idleStateID);
            cameraShipAI.callbackCompletedStateAction = null;
            WalkThruStartSection6();
        }

        public void WalkThruFinishedPath5(ShipAIInputModule shipAIInputModule)
        {
            cameraShipAI.SetState(AIState.idleStateID);
            cameraShipAI.callbackCompletedStateAction = null;
            WalkThruStartSection8();
        }

        public void WalkThruFinishedPath6(ShipAIInputModule shipAIInputModule)
        {
            cameraShipAI.SetState(AIState.idleStateID);
            cameraShipAI.callbackCompletedStateAction = null;
            WalkThruStartSection10();
        }

        #endregion

        #region Public General Member Methods

        /// <summary>
        /// When Sticky3D is in the project, the NPC at the Services desk,
        /// signals that the player should approach the desk.
        /// See also StopLookingAtServiceAssistant().
        /// </summary>
        public void CallPlayerToServicesDesk()
        {
            #if SCSM_S3D
            if (isInitialised && s3dServiceAssistant != null && s3dServiceAssistant.IsInitialised)
            {
                // Configure the Waving animation for the NPC behind the services desk
                if (defaultInteractAnimation != null && servicesNPCWaveAnimation != null)
                {
                    if (!s3dServiceAssistant.IsHeadIKEnabled) { s3dServiceAssistant.EnableHeadIK(true); }

                    // Let the NPC look at the player when the NPC is seated and movement is disabled.
                    s3dServiceAssistant.headIKWhenMovementDisabled = true;
                    s3dServiceAssistant.headIKMoveMaxSpeed = 0.7f;

                    // Tell the NPC to look at the player
                    if (useWalkThru && isWalkThruInitialised)
                    {
                        s3dServiceAssistant.SetHeadIKTarget(walkThruCamera2.transform, Vector3.zero, true, false);
                    }
                    else
                    {
                        s3dServiceAssistant.SetHeadIKTarget(s3dPlayer.transform, Vector3.zero, true, true);
                    }

                    s3dServiceAssistant.ReplaceAnimationClipNoRef(defaultInteractAnimation, servicesNPCWaveAnimation);
                    s3dServiceAssistant.PlayAnimationState(servicesAssistantInteractStateId, -1);

                    // Get the player to look towards the services assistant with a slight delay
                    //Invoke("PlayerLookAtServicesAssistant", 1f);
                }
            }
            #endif
        }

        /// <summary>
        /// This is called automatically just after the scene starts.
        /// </summary>
        public void DelayedUnlockDoors()
        {
            int numDoorsToUnlock = delayedUnlockDoors == null ? 0 : delayedUnlockDoors.Length;

            for (int drIdx = 0; drIdx < numDoorsToUnlock; drIdx++)
            {
                SSCDoorAnimator doorAnimator = delayedUnlockDoors[drIdx];
                if (doorAnimator != null) { doorAnimator.UnlockDoors(); }
            }
        }

        /// <summary>
        /// Commence the docking procedure.
        /// You could call this code or added these individual items from this method
        /// to events on a custom player input in the editor.
        /// See also UndockShuttle().
        /// </summary>
        public void DockShuttle()
        {
            shuttlePlayerInputModule.EnableAIDocking();
            shuttlePlayerInputModule.DisableInput(false);
            shipDisplayModule.HideDisplayMessage(startDockingMessage);

            if (useWalkThru)
            {
                shipDisplayModule.ShowDisplayMessage(dockingInProgressMessage);
                if (dockingButton != null) { Destroy(dockingButton); }

                walkThruSectionNumber = isWalkThruInitialised ? 1 : 0;
            }
            else
            {
                shipDisplayModule.ShowDisplayMessage(exitShuttleMessage);
            }
            
            UpdateGauges();
        }

        /// <summary>
        /// Commence the undocking procedure.
        /// You could call this code or added these individual items from this method
        /// to events on a custom player input in the editor.
        /// See also DockShuttle().
        /// </summary>
        public void UndockShuttle()
        {
            shuttlePlayerInputModule.EnableAIUndocking();
            shuttlePlayerInputModule.DisableInput(false);
            UpdateGauges();
        }

        /// <summary>
        /// Display a message to the user to get to their ship on the upper deck.
        /// Remind them if they have forgotten to put on their helmet (Stick3D only).
        /// This is triggered by attempting to go through the outer doors at the top
        /// of lift1.
        /// </summary>
        public void GetToShipMessage()
        {
            shipDisplayModule.HideDisplayMessage(getToDeckMessage);

            #if SCSM_S3D
            if (useWalkThru || s3dPlayerParts.IsPartEnabledByIndex(2))
            {
                shipDisplayModule.HideDisplayMessage(getToHawkMessageWithHelmet);
                shipDisplayModule.ShowDisplayMessage(getToHawkMessage);
            }
            else
            {
                shipDisplayModule.HideDisplayMessage(getToHawkMessage);
                shipDisplayModule.ShowDisplayMessage(getToHawkMessageWithHelmet);
            }
            #else
            shipDisplayModule.ShowDisplayMessage(getToHawkMessage);
            #endif
        }

        /// <summary>
        /// Switch the character to the Hawk ship
        /// </summary>
        public void HawkCockpitEntry()
        {
            if (isInitialised && playerHawk != null && shipCamera != null)
            {
                // If on, turn off the HUD
                if (shipDisplayModule != null)
                {
                    shipDisplayModule.HideDisplayGauges();

                    // Ensure the HUD turns off the instance cockpit entry begins
                    shipDisplayModule.isHideHUDWithFlicker = false;
                    
                    shipDisplayModule.HideCursor();
                    shipDisplayModule.HideHUD();
                }

                // Set ship camera to Hawk
                shipCamera.SetTarget(playerHawk);

                // Set ship camera to cockpit view
                changeShipCameraView.SetCurrentView(0);

                // Move the ladder away from the hawk to avoid issues with the convex ship collider
                if (ladder1 != null) { ladder1.transform.position += ladder1.transform.rotation * (Vector3.forward * 3f); }

                // Switch Hawk from static to mesh convex colliders for flight
                if (hawkStaticColliders != null && hawkMeshCollider != null)
                {
                    // Turn off the static primitive colliders which are used when stationary on the deck of the space port
                    if (hawkStaticColliders.activeSelf) { hawkStaticColliders.SetActive(false); }

                    // Turn off the collider on the Hawk canopy and adjust material transparency
                    if (hawkCanopyCollider != null)
                    {
                        MeshRenderer canopyRM = hawkCanopyCollider.GetComponent<MeshRenderer>();
                        if (canopyRM != null)
                        {
                            // set the non-share material instance on the canopy
                            // NOTE: We may need to switch it back when user selects one of the
                            // 2 outside camera views while in the Hawk otherwise it will look
                            // like there is no canopy.
                            canopyRM.material.color = Color.clear;
                        }

                        hawkCanopyCollider.enabled = false;
                    }

                    // Turn on the convex mesh collider for the hawk
                    // This assume there is no active character in the cockpit seat.
                    if (!hawkMeshCollider.enabled)
                    {
                        hawkMeshCollider.convex = true;
                        hawkMeshCollider.enabled = true;

                        playerHawk.ShipRigidbody.ResetCenterOfMass();
                        playerHawk.ReinitialiseMass();
                    }
                }

                if (useWalkThru)
                {
                    walkThruSectionNumber = 11;

                    // Turn off the walk thru camera currently in use
                    walkThruCamera1.enabled = false;
                    walkThruCamera1.gameObject.SetActive(false);
                }
                else
                {
                    // Disable Sticky3D character
                    #if SCSM_S3D
                    if (s3dPlayer != null )
                    {
                        // Gracefully disable the character
                        s3dPlayer.DisableCharacter(true);

                        // Turn off the character
                        s3dPlayer.gameObject.SetActive(false);
                    }

                    #endif
                }

                // Enable Hawk Camera to move
                shipCamera.EnableCamera();

                // Get the stars to rotate with the ship camera rather than the Sticky3D character
                if (celestials != null && celestials.IsInitialised)
                {
                    celestials.camera1 = shipCamera.GetCamera1;
                    celestials.RefreshCameras();
                }

                // Assign the Hawk camera to the HUD so that Targets get shown
                // in the correct position on the screen.
                if (shipDisplayModule != null)
                {
                    shipDisplayModule.SetCamera(shipCamera.GetCamera1);
                }

                // Render the Hawk Camera
                shipCamera.StartCamera();

                // Lower the Hawk canopy
                if (hawkCanopyDoor != null) { hawkCanopyDoor.CloseDoors(); }

                isInHawkCockpit = true;

                if (beaconLight != null) { beaconLight.FadeOutAudio(2f); }

                // Wait a few seconds while the canopy closes
                Invoke("HawkPrepareForTakeOff", 3f);
            }
        }

        /// <summary>
        /// The player has passed through the outer doors on the top deck.
        /// Called from SSC Proximity on the TriggerBombingRuns gameobject. 
        /// </summary>
        public void PastOuterDoors()
        {
            // Once we're through the outer doors, "run" across the top deck toward the Hawk
            if (!bombingRunsStarted)
            {
                if (useWalkThru && isWalkThruInitialised)
                {
                    cameraShipAI.maxSpeed = 3f;
                    walkThruCamera2.moveSpeed = 6f;
                }

                // Disable Sticky3D assistant
                // No need to have them animating etc when out of view
                #if SCSM_S3D
                if (s3dServiceAssistant != null )
                {
                    // Gracefully disable the character
                    s3dServiceAssistant.DisableCharacter(true);

                    // Turn off the character
                    s3dServiceAssistant.gameObject.SetActive(false);
                }
                #endif

                LightingExitOuterDoors();

                StartCoroutine(FadeUpDirectLight());
            }
        }

        /// <summary>
        /// Shortly after the scene first renders we want to display the correct camera.
        /// This is called automatically.
        /// </summary>
        public void SetSceneCamera()
        {
            if (celestials.camera1 == null) { celestials.camera1 = Camera.main; }

            celestials.RefreshCameras();
        }

        /// <summary>
        /// Start the attack on the Spaceport.
        /// See the SSCProximity component on TriggerAttack gameobject in the scene.
        /// </summary>
        public void StartAttack ()
        {
            if (!attackStarted)
            {
                // Stop current ambient audio to improve performance
                StopAmbientAudio();

                // Turn off lights in the shuttle
                if (shuttleCockpitLight != null) { shuttleCockpitLight.enabled = false; }
                if (shuttleCargoBayLight != null) { shuttleCargoBayLight.enabled = false; }

                // Rotating light on top deck
                if (beaconLight != null) { beaconLight.TurnOn(true); }

                // Strobe lights at top of each lift
                if (liftStrobeLight1 != null) { liftStrobeLight1.TurnOn(); }
                if (liftStrobeLight2 != null) { liftStrobeLight2.TurnOn(); }

                // Unlock lift doors
                if (lift1BottomDoors != null) { lift1BottomDoors.UnlockDoors(); }
                if (lift2BottomDoors != null) { lift2BottomDoors.UnlockDoors(); }

                // Lock outer bay door where shuttle docked to prevent re-entry to shuttle when attack starts
                if (outerDockingBayDoor1 != null) { outerDockingBayDoor1.LockDoors(); }
                if (outerDockingBayDoorProximity != null) { outerDockingBayDoorProximity.isUnlockDoorsOnEntry = false; }

                // Update lift door control panels after the doors have been unlocked
                if (sscDoorControlLift1Doors != null) { sscDoorControlLift1Doors.UpdateLockStatus(); }

                // The lift barrier emissive material flashing can bleed light through the services
                // room doors. It also doesn't need to be on all the time. Start On is not enabled in the editor.
                if (lift1Barrier != null) { lift1Barrier.TurnGroupOn(1); }

                #region Explosion and camera shake on Sticky3D

                Vector3 explosionPosition = Vector3.zero;

                if (useWalkThru && isWalkThruInitialised)
                {
                    walkThruCamera2.ShakeCamera(0.5f, 0.2f);
                    // Set the explosion sound 1m infront of the camera "ship" for maximum effect
                    explosionPosition = cameraShip.shipInstance.TransformPosition + cameraShip.shipInstance.TransformForward;
                }
                else
                {
                    #if SCSM_S3D
                    if (s3dPlayer != null)
                    {
                        s3dPlayer.ShakeCamera(0.5f, 0.2f);
                        // Set the explosion sound 1m infront of the character for maximum effect
                        explosionPosition = s3dPlayer.GetCurrentTop() + s3dPlayer.GetCurrentForward;

                        //s3dPlayer.headIKAdjustForVelocity = true;
                    }
                    #else
                    {
                        // Fallback position of explosion
                        if (lift1BottomDoors != null)
                        {
                            explosionPosition = lift1BottomDoors.transform.position + lift1BottomDoors.transform.forward;
                        }                
                    }
                    #endif
                }

                // Ensure the guages are turned off
                if (shipDisplayModule != null)
                {
                    shipDisplayModule.HideDisplayGauges();
                    shipDisplayModule.HideDisplayMessages();

                    // Guide the player toward the lift so that they get to the upper deck of the space port
                    // COMMENTED OUT TO TEST COCKPIT ENTRY
                    if (getToDeckMessage != null) { shipDisplayModule.ShowDisplayMessage(getToDeckMessage); }
                }

                PlaySoundFX(attackExplosion, explosionPosition, 1f);

                #endregion

                #region Enable Ships

                // Enable and set initial targets for the friendly ships
                if (friendlyShipPrefab != null && numFriendlyShip > 0)
                {
                    for (int i = 0; i < numFriendlyShip; i++)
                    {
                        ShipAIInputModule friendlyShip = friendlyShips[i];
                        if (friendlyShip != null)
                        {
                            ShipControlModule friendlyShipCM = friendlyShip.GetShipControlModule;
                            if (friendlyShipCM != null)
                            {
                                friendlyShipCM.EnableShip(true, true);
                            }
                        }
                    }
                }

                // Enable and set initial targets for the type 1 enemy ships
                if (enemyShip1Prefab != null && numEnemyShip1 > 0)
                {
                    for (int i = 0; i < numEnemyShip1; i++)
                    {
                        ShipAIInputModule enemyShip = enemyType1Ships[i];
                        if (enemyShip != null)
                        {
                            ShipControlModule enemyShipCM = enemyShip.GetShipControlModule;
                            if (enemyShipCM != null)
                            {
                                enemyShipCM.EnableShip(true, true);
                            }
                        }
                    }
                }

                // Enable and set initial targets for the type 2 enemy ships
                if (enemyShip2Prefab != null && numEnemyShip2 > 0)
                {
                    for (int i = 0; i < numEnemyShip2; i++)
                    {
                        ShipAIInputModule enemyShip = enemyType2Ships[i];
                        if (enemyShip != null)
                        {
                            ShipControlModule enemyShipCM = enemyShip.GetShipControlModule;
                            if (enemyShipCM != null)
                            {
                                enemyShipCM.EnableShip(true, true);
                            }
                        }
                    }
                }

                // Enable the enemy capital ships
                numEnemyCapitalShips = enemyCapitalShips != null ? enemyCapitalShips.Count : 0;
                if (enemyCapitalShipPrefab != null && numEnemyCapitalShips > 0)
                {
                    for (int i = 0; i < numEnemyCapitalShips; i++)
                    {
                        ShipAIInputModule enemyShip = enemyCapitalShips[i];
                        if (enemyShip != null)
                        {
                            ShipControlModule enemyShipCM = enemyShip.GetShipControlModule;
                            if (enemyShipCM != null)
                            {
                                enemyShipCM.EnableShip(true, true);
                                enemyShipCM.DisableShipMovement();
                                enemyShipCM.ShipRigidbody.detectCollisions = true;
                            }
                        }
                    }
                }

                numEnemyRemaining = numEnemyShip1 + numEnemyShip2;

                #endregion

                #region Pair Ships

                // Pair all friendly ships with an enemy ship
                if (friendlyShipPrefab != null && numFriendlyShip > 0)
                {
                    for (int i = 0; i < numFriendlyShip; i++)
                    {
                        ShipAIInputModule friendlyShip = friendlyShips[i];
                        if (friendlyShip != null)
                        {
                            ShipControlModule friendlyShipCM = friendlyShip.GetShipControlModule;
                            if (friendlyShipCM != null)
                            {
                                AttemptPairing(friendlyShip, friendlyShipCM);
                            }
                        }
                    }
                }

                // Set remaining enemy ships to follow waiting routines
                if (enemyShip1Prefab != null && numEnemyShip1 > 0)
                {
                    for (int i = 0; i < numEnemyShip1; i++)
                    {
                        ShipAIInputModule enemyShip = enemyType1Ships[i];
                        if (enemyShip != null)
                        {
                            ShipControlModule enemyShipCM = enemyShip.GetShipControlModule;
                            if (enemyShipCM != null)
                            {
                                int thisShipListIndex = availableEnemyShips.FindIndex(a => a == enemyShipCM.GetShipId);
                                if (thisShipListIndex != -1) { AttemptPairing(enemyShip, enemyShipCM); }
                            }
                        }
                    }
                }
                if (enemyShip2Prefab != null && numEnemyShip2 > 0)
                {
                    for (int i = 0; i < numEnemyShip2; i++)
                    {
                        ShipAIInputModule enemyShip = enemyType2Ships[i];
                        if (enemyShip != null)
                        {
                            ShipControlModule enemyShipCM = enemyShip.GetShipControlModule;
                            if (enemyShipCM != null)
                            {
                                int thisShipListIndex = availableEnemyShips.FindIndex(a => a == enemyShipCM.GetShipId);
                                if (thisShipListIndex != -1) { AttemptPairing(enemyShip, enemyShipCM); }
                            }
                        }
                    }
                }

                #endregion

                // Perform a delayed destruction of the shuttle,
                // removing any chance of the player escaping
                Invoke("DestroyShuttle", 2f);

                attackStarted = true;

                // Debug.Log("Pause editor...");
                //UnityEditor.EditorApplication.isPaused = true;
            }
        }

        /// <summary>
        /// Start the bombing runs on the fuel drums.
        /// This gets called from the TriggerBombingRuns SSCProximity component
        /// at the top of the Lift1 outer doors.
        /// </summary>
        public void StartBombingRuns ()
        {
            if (!bombingRunsStarted)
            {
                bombingRunsStarted = true;

                // We no longer need the lift barrier flashing on/off
                if (lift1Barrier != null) { lift1Barrier.TurnGroupOff(1); }
            }
        }

        /// <summary>
        /// When the player opens the shuttle doors start playing the ambient background sounds.
        /// See also StopAmbientAudio()
        /// </summary>
        public void StartSpacePortAmbientFX()
        {
            if (spacePortAmbientClip != null && !ambientAudioSource.isPlaying)
            {
                ambientAudioSource.clip = spacePortAmbientClip;
                ambientAudioSource.volume = 0.3f;
                ambientAudioSource.Play();
            }
        }

        /// <summary>
        /// Stop the assistant looking at the player. This is called from the Lift1 access doors when
        /// the player opens the doors that lead from the services area to the bottom of Lift1.
        /// </summary>
        public void StopAssistantLookingAtPlayer()
        {
            #if SCSM_S3D
            if (isInitialised && s3dServiceAssistant != null && s3dServiceAssistant.IsInitialised)
            {
                // Stop assistant looking at Player
                s3dServiceAssistant.DisableHeadIK(true);
            }
            #endif
        }

        /// <summary>
        /// This gets triggered when the player has chatted with the service
        /// assistant and is now walking away (hopefully toward to the Lift Deck door control panel).
        /// See the SSCProximity component on TriggerAttack gameobject in the scene.
        /// </summary>
        public void StopLookingAtServiceAssistant()
        {
            #if SCSM_S3D
            if (isInitialised && s3dServiceAssistant != null && s3dServiceAssistant.IsInitialised)
            {
                // Stop assistant looking at Player
                s3dServiceAssistant.DisableHeadIK(true);

                // Leave Head IK enabled, just stop player looking at anything right now 
                if (!useWalkThru && s3dPlayer != null) { s3dPlayer.SetHeadIKTarget(null); }
            }
            #endif
        }

        /// <summary>
        /// Turn off the Lift Call button in the scene.
        /// For walk thru, also checks if the "player" is waiting for the lift.
        /// </summary>
        public void TurnOffLiftCallButton()
        {           
            if (lift1ControlChangeMaterial != null)
            {
                #if SCSM_S3D
                if (!useWalkThru && lift1ControlInteractive != null && s3dPlayer != null)
                {
                    // The lift control is a Selectable S3D Interactive-enabled object. The selected
                    // state is tracked by individual S3D characters rather than the object itself.
                    // Therefore we need to tell the S3D character than it has been unselected.
                    // If the button was not selected, this will have no effect.
                    s3dPlayer.UnselectInteractive(lift1ControlInteractive);
                }
                else
                {
                    // Turn off the control display. i.e. change the material to the off material
                    lift1ControlChangeMaterial.GetGroup1Material(0);
                }
                #else
                // Turn off the control display. i.e. change the material to the off material
                lift1ControlChangeMaterial.GetGroup1Material(0);
                #endif

                if (useWalkThru && isWalkThruInitialised && walkThruSectionNumber == 6)
                {
                    WalkThruStartSection7();
                }
            }
        }

        /// <summary>
        /// Update the status of HUD gauges when on the Shuttle or in the Hawk
        /// </summary>
        public void UpdateGauges()
        {
            if (isInHawkCockpit && isUpdateHawkHUDMetrics)
            {
                // Assume using the Central fuel level for the whole ship
                shipDisplayModule.SetDisplayGaugeValue(fuelLevelGauge, playerHawk.shipInstance.GetFuelLevel() / 100f);

                // Update Thruster temperature on the HUD
                shipDisplayModule.SetDisplayGaugeValue(heatLevelGauge, playerHawk.shipInstance.GetHeatLevel(1) / 100f);

                // Update ship overall health level on the HUD
                shipDisplayModule.SetDisplayGaugeValue(healthGauge, playerHawk.shipInstance.HealthNormalised);

                // Missiles, Shield, Launch and Gear gauges only are displayed when game quality is medium or high

                shipDisplayModule.SetDisplayGaugeValue(missilesGauge, (playerHawkLMissiles.ammunition + playerHawkRMissiles.ammunition) / missilesGauge.gaugeMaxValue);
                shipDisplayModule.SetDisplayGaugeValue(shieldsGauge, playerHawk.shipInstance.mainDamageRegion.ShieldNormalised);

                // Update how may enemy fighters remain
                shipDisplayModule.SetDisplayGaugeValue(enemyGauge, numEnemyRemaining / enemyGauge.gaugeMaxValue);
            }
            else if (isInitialised && dockedIndicator != null)
            {
                // Is the ship docked with the space port?
                shipDisplayModule.SetDisplayGaugeValue(dockedIndicator, shipDocking.GetStateInt() == ShipDocking.dockedInt ? 1f : 0f);
            }
        }

        #endregion

        #region Public Camera Member Methods

        /// <summary>
        /// Cycles the camera view for the tech demo 3 Hawk player camera.
        /// </summary>
        public void TechDemoCycleCameraView(Vector3 inputValue, int customPlayerInputEventType)
        {
            if (changeShipCameraView != null)
            {
                // Cycle the camera view
                changeShipCameraView.CycleCameraView(inputValue, customPlayerInputEventType);

                // Update the HUD depending on whether the new view is inside or outside the cockpit
                int newCameraViewIndex = changeShipCameraView.GetCurrentCameraViewIndex();
                if (newCameraViewIndex == 0)
                {
                    // Inside cockpit
                    shipDisplayModule.SetTargetsViewportSize(1f, cockpitViewHeight);
                    shipDisplayModule.SetTargetsViewportOffset(0f, (1f - cockpitViewHeight) / 2f);
                }
                else
                {
                    // Outside cockpit
                    shipDisplayModule.SetTargetsViewportSize(1f, 1f);
                    shipDisplayModule.SetTargetsViewportOffset(0f, 0f);
                }
            }
        }


        #endregion

        #region Public Menu Member Methods

        /// <summary>
        /// Toggle Pause on/off. When using (new) Unity Input, like say on Xbox,
        /// this can be called as a CustomInput from PlayerInputModule.
        /// </summary>
        public void TogglePause()
        {
            if (!isGamePaused) { PauseGame(); }
            else { StartCoroutine(UnPauseGameNextFrame()); }
        }

        /// <summary>
        /// This is hooked up to the Resume Button in the UI
        /// </summary>
        public void ResumeGame()
        {
            // When this is called from clicking on the UI,
            // we need to wait until the next frame before processing
            // the unpause.
            StartCoroutine(UnPauseGameNextFrame());
        }

       /// <summary>
        /// This is hooked up to the Start Button in the UI
        /// </summary>
        public void StartGame()
        {
            SetGameQuality();
            ShowMenu(false);
            // Make the cargo bay light "important"
            SetLightImportance(shuttleCargoBayLight, true);
            StartCoroutine(UnPauseGameNextFrame());
        }

        /// <summary>
        /// This is hooked up to the Restart Button in the UI
        /// </summary>
        public void RestartGame()
        {
            // Reset the timescale. When the scene loads the PlayerCamera will Awake before
            // Start in the script runs. If timeScale is still 0, we'll get errors with the PlayerCamera script,
            // because DisableCamera hasn't been called.
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Called when the Quit button is clicked on screen.
        /// </summary>
        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        /// <summary>
        /// This is hooked up to Resume button EventTrigger in the UI
        /// </summary>
        /// <param name="isSelected"></param>
        public void ResumeSelected(bool isSelected)
        {
            SelectButton(resumeButton, resumeButtonText, isSelected);
        }

        /// <summary>
        /// This is hooked up to Start button EventTrigger in the UI
        /// </summary>
        /// <param name="isSelected"></param>
        public void StartSelected(bool isSelected)
        {
            SelectButton(startButton, startButtonText, isSelected);
        }

        /// <summary>
        /// This is hooked up to Restart button EventTrigger in the UI
        /// </summary>
        /// <param name="isSelected"></param>
        public void RestartSelected(bool isSelected)
        {
            SelectButton(restartButton, restartButtonText, isSelected);
        }

        /// <summary>
        /// This is hooked up to Quit button EventTrigger in the UI
        /// </summary>
        /// <param name="isSelected"></param>
        public void QuitSelected(bool isSelected)
        {
            SelectButton(quitButton, quitButtonText, isSelected);
        }

        /// <summary>
        /// This is hooked up to Quality Low button EventTrigger in the UI
        /// </summary>
        /// <param name="isSelected"></param>
        public void QualityLowSelected(bool isSelected)
        {
            SelectButton(qualityLowButton, qualityLowButtonText, isSelected);
        }

        /// <summary>
        /// This is hooked up to Quality Medium button EventTrigger in the UI
        /// </summary>
        /// <param name="isSelected"></param>
        public void QualityMedSelected(bool isSelected)
        {
            SelectButton(qualityMedButton, qualityMedButtonText, isSelected);
        }

        /// <summary>
        /// This is hooked up to Quality High button EventTrigger in the UI
        /// </summary>
        /// <param name="isSelected"></param>
        public void QualityHighSelected(bool isSelected)
        {
            SelectButton(qualityHighButton, qualityHighButtonText, isSelected);
        }

        /// <summary>
        /// This is hooked up to the Quality Low, Med, High buttons in the UI
        /// This method doesn't change the game quality (that happens only when
        /// the game is started or restarted). The menu changes what the value
        /// should be when the game starts.
        /// </summary>
        /// <param name="quality"></param>
        public void UpdateGameQualitySetting(int quality)
        {
            // UI events cannot pass enumerations so we use a matching integer.
            gameQuality = (GameQuality)quality;
            HighlightQuality();
        }

        #endregion

        #region Public Callback Member Methods

        /// <summary>
        /// Callback for when an AI ship completes a state action.
        /// </summary>
        /// <param name="shipAIInputModuleInstance"></param>
        public void CompletedStateActionCallback(ShipAIInputModule shipAIInputModuleInstance)
        {
            if (shipAIInputModuleInstance != null)
            {
                ShipControlModule shipControlModuleInstance = shipAIInputModuleInstance.GetShipControlModule;

                if (shipControlModuleInstance != null)
                {
                    // Attempt new pairing
                    AttemptPairing(shipAIInputModuleInstance, shipControlModuleInstance);
                }
            }
        }

        /// <summary>
        /// Callback for when an AI ship is destroyed
        /// </summary>
        /// <param name="shipInstance"></param>
        public void OnAIShipDestroyed(Ship shipInstance)
        {
            // Do nothing if the ship is going to be respawned
            if (shipInstance.respawningMode != Ship.RespawningMode.DontRespawn) { return; }

            int factionId = shipInstance.factionId;
            int squadronId = shipInstance.squadronId;
            int shipId = shipInstance.shipId;

            ShipAIInputModule shipAIInputModuleInstance = FindAIShip(shipInstance);

            if (factionId == friendlyFactionID)
            {
                // update relevant lists of friendly ships when ships are destroyed
                if (squadronId == friendlySquadronID)
                {
                    if (shipAIInputModuleInstance != null)
                    {
                        friendlyShips.Remove(shipAIInputModuleInstance);
                        numFriendlyShip--;
                    }

                    availableFriendlyShips.Remove(shipId);
                }
            }
            else if (factionId == enemyFactionID)
            {
                // update relevant lists of enemy ships when ships are destroyed
                if (squadronId == enemySquadron1ID)
                {
                    if (shipAIInputModuleInstance != null)
                    {
                        enemyType1Ships.Remove(shipAIInputModuleInstance);
                        numEnemyShip1--;
                    }
                }
                else if (squadronId == enemySquadron2ID)
                {
                    if (shipAIInputModuleInstance != null)
                    {
                        enemyType2Ships.Remove(shipAIInputModuleInstance);
                        numEnemyShip2--;
                    }
                }

                availableEnemyShips.Remove(shipId);
                numEnemyRemaining--;

                // Currently don't seem to be able to find/destroy last 2 ships
                if (numEnemyRemaining < 1)
                {
                    MissionCompleted();
                }
            }
        }

        /// <summary>
        /// This method is automatically called by SSC when the docking state has changed. We asked SSC to notify us in ConfigurePlayerShips().
        /// </summary>
        /// <param name="shipDocking"></param>
        /// <param name="shipControlModule"></param>
        /// <param name="shipAIInputModule"></param>
        /// <param name="previousDockingState"></param>
        public void OnDockingStateChanged(ShipDocking shipDocking, ShipControlModule shipControlModule, ShipAIInputModule shipAIInputModule, ShipDocking.DockingState previousDockingState)
        {
            int dockingStateInt = shipDocking.GetStateInt();

            if (dockingStateInt == ShipDocking.dockedInt && previousDockingState == ShipDocking.DockingState.Docking)
            {
                // The shuttle has just docked with the space station
                if (soundeffectsObjectPrefabID >= 0 && shipDocking.shipDockingStation != null)
                {
                    // Get the docking point
                    ShipDockingPoint shipDockingPoint = shipDocking.shipDockingStation.GetDockingPoint(shipDocking.DockingPointId);

                    if (shipDockingPoint != null)
                    {
                        // Get the docking point in world space
                        Vector3 dockingPointWS = shipDocking.shipDockingStation.GetDockingPointPositionWS(shipDockingPoint);

                        // Play the docking sound using a pool EffectsModule
                        PlaySoundFX(dockingSoundClip, dockingPointWS, 0.5f);
                    }

                    // Unlock the shuttle side doors
                    if (sscShuttleDoorAnimator != null) { sscShuttleDoorAnimator.UnlockDoor(0); }

                    // Update the lock status on the on the door control panel
                    if (sscDoorControlShuttleSideDoors != null) { sscDoorControlShuttleSideDoors.UpdateLockStatus(); }

                    // Tell player to get to the services desk
                    if (shipDisplayModule != null)
                    {
                        if (useWalkThru)
                        {
                            shipDisplayModule.HideDisplayMessage(dockingInProgressMessage);
                            shipDisplayModule.ShowDisplayMessage(exitShuttleMessage);
                        }
                        else
                        {
                            shipDisplayModule.HideDisplayMessage(exitShuttleMessage);
                            shipDisplayModule.ShowDisplayMessage(findServiceDeskMessage);
                        }
                    }

                    #if SCSM_S3D
                    if (s3dPlayer != null)
                    {
                        // Once the shuttle has docked we no longer need this option,
                        // so turn it off to improve performance.
                        //s3dPlayer.headIKAdjustForVelocity = false;
                    }
                    #endif

                    if (useWalkThru)
                    {
                        if (isWalkThruInitialised)
                        {
                            WalkThruStartSection2();
                        }
                        #if UNITY_EDITOR
                        else { Debug.LogWarning("TechDemo 3 - Walk Thru has not been intialised."); }
                        #endif
                    }

                    // Stop the shuttle "engines" after a short delay. Must be careful not to start
                    // another ambient clip before this is called otherwise it will stop
                    // the "new" ambient clip.
                    Invoke("StopAmbientAudio", 2f);
                }

                UpdateGauges();
            }
        }

        /// <summary>
        /// Callback for when the Player ship respawns
        /// </summary>
        /// <param name="shipControlModuleInstance"></param>
        /// <param name="shipAIInputModuleInstance"></param>
        public void OnPlayerRespawnCallback(ShipControlModule shipControlModuleInstance, ShipAIInputModule shipAIInputModuleInstance)
        {
            // Is the player ship currently using the HUD?
            if (shipDisplayModule.IsSourceShip(shipControlModuleInstance))
            {
                shipDisplayModule.ShowHUD();
                if (sscRadar != null) { sscRadar.ShowUI(); }
            }
        }

        /// <summary>
        /// Callback for when the player ship is destroyed.
        /// Turn off player ship HUD
        /// </summary>
        /// <param name="shipInstance"></param>
        public void OnPlayerDestroyed(Ship shipInstance)
        {
            // Is the player ship currently using the HUD?
            if (shipDisplayModule.IsSourceShip(shipInstance))
            {
                shipDisplayModule.HideHUD();
                if (sscRadar != null) { sscRadar.HideUI(); }
            }
        }

        /// <summary>
        /// Callback for when ship gets stuck
        /// </summary>
        /// <param name="shipControlModule"></param>
        public void OnShipStuck (ShipControlModule shipControlModule)
        {
            // Destroy the ship next frame
            shipControlModule.shipInstance.mainDamageRegion.Health = 0;
        }

        /// <summary>
        /// Callback for when an AI ship respawns.
        /// Once the player is in the Hawk cockpit, the number of respawns for AI ships are limited.
        /// After a ship cannot be respawned, the next time it losses all health, only OnAIShipDestroyed(..) will be called.
        /// </summary>
        /// <param name="shipAIInputModuleInstance"></param>
        public void OnRespawnCallback(ShipControlModule shipControlModuleInstance, ShipAIInputModule shipAIInputModuleInstance)
        {
            if (shipControlModuleInstance != null && shipAIInputModuleInstance != null)
            {
                int factionId = shipControlModuleInstance.shipInstance.factionId;
                int squadronId = shipControlModuleInstance.shipInstance.squadronId;

                // Call get new target function with correct parameters
                if (factionId == friendlyFactionID)
                {
                    // If this is the nth time the friendly ship is respawned, don't allow it to be respawned again.
                    if (isInHawkCockpit && shipControlModuleInstance.NumberOfRespawns >= numFriendlyRespawns)
                    {
                        shipControlModuleInstance.shipInstance.respawningMode = Ship.RespawningMode.DontRespawn;
                    }

                    // Attempt new pairing
                    AttemptPairing(shipAIInputModuleInstance, shipControlModuleInstance);
                }
                else if (factionId == enemyFactionID)
                {
                    // If this is the nth time the enemy ship is respawned, don't allow it to be respawned again
                    if (isInHawkCockpit && shipControlModuleInstance.NumberOfRespawns >= numEnemyRespawns)
                    {
                        shipControlModuleInstance.shipInstance.respawningMode = Ship.RespawningMode.DontRespawn;
                    }

                    // Attempt new pairing
                    AttemptPairing(shipAIInputModuleInstance, shipControlModuleInstance);
                }
            }
        }

        #endregion

        #region Public Sticky3D Member Methods

        #if SCSM_S3D

        /// <summary>
        /// This is called when the player stops touching a door control panel.
        /// It will stop the character reaching for the control panel.
        /// </summary>
        /// <param name="stickyInteractiveID"></param>
        /// <param name="stickyID"></param>
        public void DoorControlStopTouching(int stickyInteractiveID, int stickyID)
        {
            // The player has stopped touching the interactive door control panel, so stop trying to reach for it.
            if (stickyID == s3dPlayer.StickyID && stickyInteractiveID == s3dPlayer.GetRightHandIKTargetInteractiveID())
            {
                s3dPlayer.SetRightHandIKTargetInteractive(null, false, false);
            }
        }

        /// <summary>
        /// This gets automatically called by the S3D character when it changes what objects it is looking at in the scene.
        /// We added this via a Listener in ConfigureStick3DCharacters().
        /// </summary>
        /// <param name="stickyControlModule"></param>
        /// <param name="oldLookAtObject"></param>
        /// <param name="newLookAtObject"></param>
        public void OnInteractiveLookAtChanged(StickyControlModule stickyControlModule, StickyInteractive oldLookAtObject, StickyInteractive newLookAtObject)
        {
            if (shipDisplayModule != null)
            {
                // Change the aiming reticle colour to indicate when the character is looking at a Sticky Interactive object
                shipDisplayModule.SetDisplayReticleColour(newLookAtObject == null ? defaultReticleColour : lookingAtReticleColour);
            }
        }

        /// <summary>
        /// This is called automatically by Sticky3D when the user selects the "button" in the scene.
        /// We added this via a Listener in ConfigureStickyInteractive().
        /// </summary>
        /// <param name="stickyInteractiveID"></param>
        /// <param name="stickyID"></param>
        /// <param name="selectedStoreItemID"></param>
        public void OnInterativeButtonSelected(int stickyInteractiveID, int stickyID, int selectedStoreItemID)
        {
            // Make sure this was the player who clicked the button (and not some random NPC)
            if (stickyID == s3dPlayer.StickyID)
            {
                // Check which interactive "button" was selected.
                if (dockingButton != null && shuttleDockingButtonInteractive != null && stickyInteractiveID == shuttleDockingButtonInteractive.StickyInteractiveID)
                {
                    // Remove the listeners before destroying the component
                    shuttleDockingButtonInteractive.RemoveListeners();

                    // Tell the character to unselect the button that was just selected by the player.
                    // We don't want the character to be retaining this after we destroy it below.
                    s3dPlayer.UnselectInteractiveByStoreItemID(selectedStoreItemID);

                    // Remove the light, interactive-enabled object and collider from the scene
                    // as we won't need it again in this demo
                    Destroy(dockingButton);

                    DockShuttle();
                    StandupShuttlePilot();

                    // When the character is moving rapidly with the shuttle, the head
                    // may not follow where the user is pointing. This option attempts
                    // to overcome this.
                    //s3dPlayer.headIKAdjustForVelocity = true;
                }
            }
        }

        public void PlayerLookAtServicesAssistant()
        {
            if (s3dPlayer != null && s3dServiceAssistant != null)
            {
                s3dPlayer.headIKMoveMaxSpeed = 0.9f;
                // If the player is sitting, make sure Head IK still works.
                s3dPlayer.headIKWhenMovementDisabled = true;
                s3dPlayer.SetHeadIKTarget(s3dServiceAssistant.transform, Vector3.zero, false, true);
            }
        }

        #endif
        #endregion
    }
}