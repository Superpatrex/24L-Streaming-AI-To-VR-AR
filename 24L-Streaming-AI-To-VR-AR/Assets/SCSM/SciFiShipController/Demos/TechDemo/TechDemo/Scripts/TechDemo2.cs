using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Demo gameplay controller for Sci-Fi Ship Controller Tech Demo #2.
    /// This script contains a number of techniques that could be used in developing
    /// a game with Sci-Fi Ship Controller. Its purpose is to help you write
    /// your own game with our API. For example it shows:
    /// 1. How AI ships can be spawned
    /// 2. How AI ships can be docked and undocked from a ShipDockingStation
    /// 3. How Radar can be used to find and allocate targets to AI ships and weapons
    /// 4. How gameplay can be paused and resumed
    /// 5. How a mission can be broken up in several phases or stages
    /// 6. How AI ships can follow a path to a destination then take another action
    /// 7. How a second Point of Interest camera could be used on a second monitor
    /// 8. How to add a Heads-up Display
    /// 9. How AI ships can be undocked from a capital ship
    /// 10. How to use Damage Regions for health and weapon targeting
    /// </summary>
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class TechDemo2 : MonoBehaviour
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
        public GameQuality gameQuality = GameQuality.High;
        public bool limitScreenToHD = true;
        public bool limitFrameRate = true;

        [Header("Unity Layers")]
        public int smallShipsUnityLayer = 27;

        [Header("Factions")]
        public int friendlyFactionID = 1;
        public int enemyFactionID = 2;

        [Header("Cameras")]
        public ShipCameraModule shipCameraModule1;
        public SampleChangeCameraView changeCameraView;
        public ShipCameraModule pointOfInterestCamera;
        public bool usePOICamera = true;
        public bool isSwitchDisplays = false;

        [Header("Player 1 Ship")]
        public ShipControlModule player1Ship;
        public int player1SquadronId = 10;

        [Header("Friendly Squadron A")]
        public GameObject squadronAGameObject;
        public ShipControlModule squadronAShipPrefab;
        public int minDockingIndexA = 0;
        public int maxDockingIndexA = 0;
        /// <summary>
        /// Friendly squadron A ID.
        /// </summary>
        public int squadronAId = 1;

        [Header("Friendly Squadron B")]
        public GameObject squadronBGameObject;
        public ShipControlModule squadronBShipPrefab;
        public int minDockingIndexB = 0;
        public int maxDockingIndexB = 0;
        /// <summary>
        /// Friendly squadron B ID.
        /// </summary>
        public int squadronBId = 2;

        [Header("Enemy Squadron C")]
        public GameObject squadronCGameObject;
        public ShipControlModule squadronCShipPrefab;
        public int minDockingIndexC = 0;
        public int maxDockingIndexC = 0;
        /// <summary>
        /// Enemy base squadron ID.
        /// </summary>
        public int squadronCId = 3;

        [Header("Enemy Squadron D")]
        // These ships are in the capital ship hangar
        public GameObject squadronDGameObject;
        public ShipControlModule squadronDShipPrefab;
        public int minDockingIndexD = 0;
        public int maxDockingIndexD = 0;
        /// <summary>
        /// Enemy air squadron ID.
        /// </summary>
        public int squadronDId = 4;

        [Header("Enemy Capital Ship (E)")]
        public ShipControlModule enemyCapitalShip;
        /// <summary>
        /// Enemy capital ship squadron ID.
        /// </summary>
        public int squadronEId = 5;
        /// <summary>
        /// The delay time between initial undockings of the ships in the capital ship hangar
        /// </summary>
        public float capitalUndockingDelay = 1f;

        [Header("Enemy Base (F)")]
        public GameObject enemyTurretsGameObject;
        /// <summary>
        /// Enemy base turrets squadron ID.
        /// </summary>
        public int squadronFId = 6;
        public float enemyTurretTargetRadius = 25f;
        public float enemyBaseRadius = 1000f;

        [Header("Other")]
        public int otherFactionID = 100;

        [Header("Location Names")]
        public string friendlyBaseLocationName = "Location Name Here";
        public string enemyBaseLocationName = "Location Name Here";
        public string S2RespawnLocationName = "Location Name Here";

        [Header("Path Names")]
        public string enemyBaseExitPathName = "Path Name Here";
        public string friendlyApproachPathName = "Path Name Here";
        public string diversionPath1Name = "Path Name Here";

        [Header("Docking Stations")]
        /// <summary>
        /// Docking Station for friendly fighters at the friendly base
        /// </summary>
        public ShipDockingStation friendlyBaseDockingStation = null;
        /// <summary>
        /// The delay time between initial undockings of the friendly ships
        /// </summary>
        public float friendlyUndockingDelay = 1f;
        /// <summary>
        /// Docking Station for enemy fighters at the enemy base
        /// </summary>
        public ShipDockingStation enemyBaseDockingStation1 = null;
        /// <summary>
        /// Docking Station where the capital ship is docked
        /// </summary>
        public ShipDockingStation enemyBaseDockingStation2 = null;

        [Header("Demo Sections")]
        /// <summary>
        /// When the player or any friendly ship gets within this distance of the enemy base, section 2
        /// of the demo will be triggered.
        /// </summary>
        public float section2Dist = 8000f;

        [Header("Misc")]
        public MeshFilter gameplayArea = null;
        public GameObject landscapeParent = null;
        public Celestials celestials = null;
        public ShipDisplayModule shipDisplayModule = null;
        [Range(0f, 1f)]
        public float cockpitViewHeight = 0.75f;

        public bool isGamePaused { get; private set; }

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

        #endregion

        #region Private variables

        private SSCManager sscManager = null;
        private SSCRadar sscRadar = null;

        private List<ShipAIInputModule> squadronAShips;
        private int squadronAShipsCount = 0;
        private List<ShipAIInputModule> squadronBShips;
        private int squadronBShipsCount = 0;
        private List<ShipAIInputModule> squadronCShips;
        private int squadronCShipsCount = 0;
        private List<ShipAIInputModule> squadronDShips;
        private int squadronDShipsCount = 0;

        // A list of ships that are currently paused
        private List<ShipAIInputModule> pausedAIShips;

        private List<SurfaceTurretModule> enemySurfaceTurrets;
        private int enemySurfaceTurretsCount = 0;

        private LocationData friendlyBaseLocation;
        private LocationData enemyBaseLocation;
        private LocationData section2PlayerRespawnLocation;
        private PathData friendlyApproachPath;
        private PathData enemyBaseExitPath;
        private PathData diversionPath1;

        private List<SSCRadarBlip> sscRadarBlipsList = null;
        private int sscRadarBlipsListCount = 0;

        private SSCRadarQuery squadronARadarQuery = null;
        private SSCRadarQuery squadronBRadarQuery = null;
        private SSCRadarQuery squadronCRadarQuery = null;
        private SSCRadarQuery squadronDRadarQuery = null;

        private int[] friendlyFactions = new int[0];
        private int[] enemyFactions = new int[0];

        private ShipDockingStation capitalshipDockingStation = null;

        /// <summary>
        /// 0 - Start of demo.
        /// 1 - Player and friendly AI ships flying to enemy base.
        /// 2 - Attack on enemy base.
        /// 3 - Capital ship gets ready for departure
        /// 4 - Attack on enemy capital ship.
        /// 5 - Launch Squadron D from Capital Ship
        /// </summary>
        private int demoSectionIndex = 0;

        private bool isSmallShipsLayerDefined = false;

        private Bounds gameplayAreaBounds;

        private bool isPOICameraDisplayed = false;
        private bool isPOIwithSquadronD = false;
        private bool isBridgeAttackEnabled = false;

        private scsmmedia.MusicController musicController = null;

        // UI and HUD variables
        private EventSystem eventSystem;
        private Color colourNonSelectedBtnText;
        private Color colourSelectedBtnText;
        private Color colourDefaultBorder;
        private Color colourSelectedBorder;

        private bool isHUDFirstTimeShown = true;
        private DisplayMessage section1Message = null;
        private DisplayMessage section2Message = null;
        private DisplayMessage section3Message = null;
        private int prevNumShieldGeneratorsDestroyed = 0;

        private scsmmedia.SCSMString tempDisplayMessageString;

        #endregion

        #region Initialisation Methods

        // Use this for initialization
        void Start()
        {   
            isGamePaused = false;

            if (limitScreenToHD) { SSCUtils.MaxScreenHD(); }

            // Get or create the SSC manager and radar components
            sscManager = SSCManager.GetOrCreateManager();
            sscRadar = SSCRadar.GetOrCreateRadar();

            #region Unity Layers
            // Small ships are added to a Unity Layer
            // The Capital Ship's AI will ignore small ships with obstacle avoidance
            // We check it in the demo because we don't have control over who has and has not added
            // the Unity Layer in the editor.

            string smallShipsLayerName = LayerMask.LayerToName(smallShipsUnityLayer);

            isSmallShipsLayerDefined = !string.IsNullOrEmpty(smallShipsLayerName) && smallShipsLayerName.ToLower() == "small ships";

            #if UNITY_EDITOR
            if (!isSmallShipsLayerDefined)
            {
                string msg = "Please configure a Unity Layer called 'Small Ships'. By default the TechDemo2 script uses Layer 27";
                Debug.LogWarning("[ERROR] TechDemo2 - " + msg);
                UnityEditor.EditorUtility.DisplayDialog("TechDemo Config", msg, "Got it!");
            }
            #endif
           
            #endregion

            #region Player Initialisation

            // Set the player's faction and squadron ID
            player1Ship.shipInstance.factionId = friendlyFactionID;
            player1Ship.shipInstance.squadronId = player1SquadronId;
            // Add the player's ship to the Small Ships Unity Layer so that the Capital ship
            // ignore it for obstacle avoidance
            if (isSmallShipsLayerDefined) { player1Ship.gameObject.layer = smallShipsUnityLayer; }

            // Set up the game play area based on the area defined by a simple cube in the scene
            // The cube should have the renderer and collider disabled. The collider remains so that
            // it can be visualised in the editor for design purposes.
            if (gameplayArea != null && gameplayArea.mesh != null)
            {
                // Typically we'd use the MeshRenderer bounds but that is not available
                // so will need to use the mesh and the scaled value
                gameplayAreaBounds = gameplayArea.mesh.bounds;
                // This works because we have a 1x1x1 cube
                gameplayAreaBounds.size = gameplayArea.transform.localScale;
            }
            else
            {
                gameplayAreaBounds = new Bounds();
            }

            #if UNITY_EDITOR
            if (player1Ship.shipInstance.respawningMode == Ship.RespawningMode.DontRespawn)
            {
                Debug.LogWarning("Tech Demo - The Player ship for this demo needs to be automatically respawned. See the Combat tab.");
            }
            #endif

            // This enables us to turn off the Heads up display when the ship is destroyed
            // We could also do things like update scores or end the mission etc.
            player1Ship.callbackOnDestroy = OnPlayerDestroyed;

            // This enables us to turn the HUD on again after the ship has been respawned
            player1Ship.callbackOnRespawn = OnPlayerRespawnCallback;

            #endregion

            #region Docking Station Initialisation
            // Initialise Docking Stations
            if (friendlyBaseDockingStation != null && !friendlyBaseDockingStation.IsInitialised) { friendlyBaseDockingStation.Initialise(true); }
            if (enemyBaseDockingStation1 != null && !enemyBaseDockingStation1.IsInitialised) { enemyBaseDockingStation1.Initialise(true); }
            if (enemyBaseDockingStation2 != null && !enemyBaseDockingStation2.IsInitialised) { enemyBaseDockingStation2.Initialise(true); }
            // The Capital Ship is also a DockingStation but is setup with the ship below
            #endregion

            #region Squadrons Initialisation

            // Initialise the lists of ships
            squadronAShips = new List<ShipAIInputModule>();
            squadronBShips = new List<ShipAIInputModule>();
            squadronCShips = new List<ShipAIInputModule>();
            squadronDShips = new List<ShipAIInputModule>();

            // Initialise the squadrons
            InitialiseSquadron(squadronAGameObject, squadronAShipPrefab, friendlyBaseDockingStation, minDockingIndexA,
                maxDockingIndexA, squadronAShips, friendlyFactionID, squadronAId);
            InitialiseSquadron(squadronBGameObject, squadronBShipPrefab, friendlyBaseDockingStation, minDockingIndexB,
                maxDockingIndexB, squadronBShips, friendlyFactionID, squadronBId);
            InitialiseSquadron(squadronCGameObject, squadronCShipPrefab, enemyBaseDockingStation1, minDockingIndexC,
                maxDockingIndexC, squadronCShips, enemyFactionID, squadronCId);

            // Store how many ships are in each squadron
            squadronAShipsCount = squadronAShips.Count;
            squadronBShipsCount = squadronBShips.Count;
            squadronCShipsCount = squadronCShips.Count;

            #endregion

            #region Enemy Base Initialisation

            // Initialise the list of enemy turrets
            enemySurfaceTurrets = new List<SurfaceTurretModule>();

            if (enemyTurretsGameObject != null)
            {
                // Find all the enemy surface turrets
                SurfaceTurretModule[] surfaceTurrets = enemyTurretsGameObject.GetComponentsInChildren<SurfaceTurretModule>();
                int surfaceTurretsLength = surfaceTurrets.Length;
                // Loop through all of the turrets
                SurfaceTurretModule surfaceTurretModuleInstance;
                for (int i = 0; i < surfaceTurretsLength; i++)
                {
                    surfaceTurretModuleInstance = surfaceTurrets[i];
                    surfaceTurretModuleInstance.isVisibleToRadar = true;
                    surfaceTurretModuleInstance.autoCreateLocation = false;
                    // Set the faction and squadron IDs for the surface turret
                    surfaceTurretModuleInstance.factionId = enemyFactionID;
                    surfaceTurretModuleInstance.squadronId = squadronFId;
                    // Set up notification when the turret is destroyed
                    surfaceTurretModuleInstance.callbackOnDestroy = OnSurfaceTurretDestroyed;
                    // Initialise the turret in manual fire mode (so that it initially will do nothing)
                    // Will also add to radar as RadarItemType.GameObject
                    surfaceTurretModuleInstance.Initialise();
                    surfaceTurretModuleInstance.SetManualFire();
                    // Squadron Id is not usually used with a surface turret. So set it now.
                    //sscRadar.SetSquardronId(surfaceTurretModuleInstance.RadarId, squadronFId);
                    // Add it to the list
                    enemySurfaceTurrets.Add(surfaceTurretModuleInstance);
                }
            }

            // Store how many enemy surface turrets there are
            enemySurfaceTurretsCount = enemySurfaceTurrets.Count;

            #endregion

            #region Enemy Capital Ship Initialisation

            if (enemyCapitalShip != null)
            {
                // Set the faction and squadron ID of the enemy capital ship
                enemyCapitalShip.shipInstance.factionId = enemyFactionID;
                enemyCapitalShip.shipInstance.squadronId = squadronEId;

                enemyCapitalShip.callbackOnDestroy = OnMissionSuccess;

                // The capital ship is initialised above when the ship docking station (SSCHanger1) it is docked
                // with, is initialised. The docking component is also initialised by the docking station
                // as it was assiged to the docking point in the editor.

                // This will cache the AI component on the ShipControlModule and return it here
                ShipAIInputModule enemyCapitalShipAI = enemyCapitalShip.GetShipAIInputModule();
                if (enemyCapitalShipAI != null)
                {
                    // If the Unity Layer for small ships is setup correctly, exclude these
                    // from Capital Ship obstacle avoidance.
                    if (isSmallShipsLayerDefined && SSCUtils.IsInLayerMask(smallShipsUnityLayer, enemyCapitalShipAI.obstacleLayerMask))
                    {
                        // Exclude Small Ships if not already excluded
                        enemyCapitalShipAI.obstacleLayerMask -= 1 << smallShipsUnityLayer;
                    }
                    enemyCapitalShipAI.Initialise();
                }

                // Initialise the docking station which is part of the capital ship
                // This is where the enemy SquadronD fighters are launched from after the capital ship takes off.
                capitalshipDockingStation = enemyCapitalShip.GetComponent<ShipDockingStation>();
                if (capitalshipDockingStation != null && !capitalshipDockingStation.IsInitialised)
                {
                    capitalshipDockingStation.Initialise(true);
                    InitialiseSquadron(squadronDGameObject, squadronDShipPrefab, capitalshipDockingStation, minDockingIndexD,
                        maxDockingIndexD, squadronDShips, enemyFactionID, squadronDId);
                    squadronDShipsCount = squadronDShips.Count;
                }

                // Disabling the ship will also make it invisible to radar
                enemyCapitalShip.DisableShip(false);
            }

            #endregion

            #region Locations Initialisation

            // Get the locations using their names from the SSC Manager
            friendlyBaseLocation = sscManager.GetLocation(friendlyBaseLocationName);
            if (friendlyBaseLocation != null)
            {
                // Set the friendly base location to an unused faction ID
                friendlyBaseLocation.factionId = otherFactionID;
            }
            enemyBaseLocation = sscManager.GetLocation(enemyBaseLocationName);
            if (enemyBaseLocation != null)
            {
                // Set the enemy base location to an unused faction ID
                enemyBaseLocation.factionId = otherFactionID;
            }
            section2PlayerRespawnLocation = sscManager.GetLocation(S2RespawnLocationName);

            #endregion

            #region Paths Initialisation

            // Get the paths using their names from the SSC Manager
            friendlyApproachPath = sscManager.GetPath(friendlyApproachPathName);
            enemyBaseExitPath = sscManager.GetPath(enemyBaseExitPathName);
            diversionPath1 = sscManager.GetPath(diversionPath1Name);

            // We might not be using this any more
            if (enemyBaseExitPath != null) { }

            #endregion

            #region Initialise Radar Query Data

            // Initialise squadron radar queries
            squadronARadarQuery = new SSCRadarQuery();
            squadronBRadarQuery = new SSCRadarQuery();
            squadronCRadarQuery = new SSCRadarQuery();
            squadronDRadarQuery = new SSCRadarQuery();

            // Initialise a list of radar results
            sscRadarBlipsList = new List<SSCRadarBlip>();

            // Create int lists for groups of factions/squadrons that we will use repeatedly
            friendlyFactions = new int[] { friendlyFactionID };
            enemyFactions = new int[] { enemyFactionID };

            #endregion

            #region Point Of Interest Camera
            if (pointOfInterestCamera != null)
            {
                DisablePointOfInterestCamera();
            }
            #endregion

            #region Change Camera View

            #if UNITY_EDITOR
            if (changeCameraView == null) { Debug.LogWarning("ERROR: TechDemo - ChangeCameraView is not defined in the Cameras section in the inspector"); }
            #endif

            #endregion

            #region Quality Settings

            // Start up validation
            #if UNITY_EDITOR
            if (landscapeParent == null) { Debug.LogWarning("Tech Demo - cannot find landscape parent gameobject."); }
            #endif

            SetGameQuality();
            #endregion

            #region Heads-Up Display (HUD)
            if (shipDisplayModule != null)
            {
                // We would typically call shipDisplayModule.HideHUD() after configuring it here,
                // but due to how we play the scene for 0.5 of a second to allow the camera to reposition
                // that will not work. So make sure things all are turned off in the HUD when the scene
                // first starts. We use a bool isHUDFirstTimeShown to reanable things in UnpauseGame().

                // Before shipDisplayModule is initialised, we can simply update the variables directly
                shipDisplayModule.showActiveDisplayReticle = false;
                shipDisplayModule.showAirspeed = false;
                shipDisplayModule.showAltitude = false;

                shipDisplayModule.Initialise();
                int guidHashReticle = shipDisplayModule.GetDisplayReticleGuidHash(2);
                shipDisplayModule.ChangeDisplayReticle(guidHashReticle);

                // Pre-create display messages (by design they are not shown when first added)
                // We could have also set these up in the editor.

                // Add a message telling the player to fly to the enemy base
                section1Message = shipDisplayModule.AddMessage("Section 1 Message", "<b>Objective</b>: Fly to the enemy base.");
                section1Message.scrollSpeed = 0.75f;
                section1Message.isScrollFullscreen = true;
                shipDisplayModule.SetDisplayMessageOffset(section1Message, 0f, 0.53f);
                shipDisplayModule.SetDisplayMessageSize(section1Message, 0.5f, 0.1f);
                shipDisplayModule.SetDisplayMessageScrollDirection(section1Message, DisplayMessage.ScrollDirectionLR);
                shipDisplayModule.SetDisplayMessageTextFontSize(section1Message, false, 1, 20);

                // Create new message with the attributes of section1Message
                section2Message = shipDisplayModule.CopyDisplayMessage(section1Message, "Section 2 Message");
                // We can modify attributes directly before calling AddMessage or we can call SetDisplayMessage[...]() methods
                // after adding it - like was done with section1Message above.
                section2Message.messageString = "<b>Objective</b>: Destroy the enemy turrets (0/4).";
                shipDisplayModule.AddMessage(section2Message);

                section3Message = shipDisplayModule.CopyDisplayMessage(section1Message, "Section 3 Message");
                section3Message.messageString = "<b>Objective</b>: Destroy the shield generators of the enemy capital ship (0/4).";
                shipDisplayModule.AddMessage(section3Message);

                // Create Display Targets at runtime (by design they are not shown when first added)
                // Display Targets should not have overlaying factions or squadrons. That is, each DisplayTarget
                // should be limited to a unique group or category of radar items. The same faction or squadron
                // should not appear in multiple DisplayTargets.
                // Create an enemy "target"
                guidHashReticle = shipDisplayModule.GetDisplayReticleGuidHash("SSCUITarget1");
                DisplayTarget displayTarget = shipDisplayModule.AddTarget(guidHashReticle);
                displayTarget.factionsToInclude = enemyFactions;
                // Enemy ship squadrons
                displayTarget.squadronsToInclude = new int[] { squadronCId, squadronDId, squadronEId };
                shipDisplayModule.AddTargetSlots(displayTarget, 2);
                shipDisplayModule.SetDisplayTargetReticleColour(displayTarget, Color.red);

                // Create an enemy turret target
                guidHashReticle = shipDisplayModule.GetDisplayReticleGuidHash("SSCUITarget2");
                displayTarget = shipDisplayModule.AddTarget(guidHashReticle);
                displayTarget.factionsToInclude = enemyFactions;
                // Enemy turret squadron
                displayTarget.squadronsToInclude = new int[] { squadronFId };
                shipDisplayModule.AddTargetSlots(displayTarget, 2);
                shipDisplayModule.SetDisplayTargetReticleColour(displayTarget, Color.red);

                // Create a friendly "target"
                guidHashReticle = shipDisplayModule.GetDisplayReticleGuidHash("SSCUITarget4");
                displayTarget = shipDisplayModule.AddTarget(guidHashReticle);
                displayTarget.isTargetable = false;
                displayTarget.factionsToInclude = friendlyFactions;
                displayTarget.squadronsToInclude = new int[] { squadronAId, squadronBId };
                shipDisplayModule.SetDisplayTargetReticleColour(displayTarget, Color.blue);

                shipDisplayModule.HideDisplayMessages();
                shipDisplayModule.HideDisplayGauges();
                shipDisplayModule.HideDisplayTargets();
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: TechDemo - ShipDisplayModule is not defined in the Misc section in the inspector"); }
            #endif
            #endregion

            #region Initialise Menu
            #if UNITY_EDITOR
            if (menuPanel == null) { Debug.LogWarning("ERROR: TechDemo UI menuPanel is not configured"); }
            if (qualityPanel == null) { Debug.LogWarning("ERROR: TechDemo UI qualityPanel is not configured"); }
            if (resumeButton == null) { Debug.LogWarning("ERROR: TechDemo resumeButton is not configured"); }
            if (startButton == null) { Debug.LogWarning("ERROR: TechDemo startButton is not configured"); }
            if (restartButton == null) { Debug.LogWarning("ERROR: TechDemo restartButton is not configured"); }
            if (quitButton == null) { Debug.LogWarning("ERROR: TechDemo quitButton is not configured"); }
            if (qualityLowButton == null) { Debug.LogWarning("ERROR: TechDemo qualityLowButton is not configured"); }
            if (qualityMedButton == null) { Debug.LogWarning("ERROR: TechDemo qualityMedButton is not configured"); }
            if (qualityHighButton == null) { Debug.LogWarning("ERROR: TechDemo qualityHighButton is not configured"); }
            if (resumeButtonText == null) { Debug.LogWarning("ERROR: TechDemo resumeButtonText is not configured"); }
            if (startButtonText == null) { Debug.LogWarning("ERROR: TechDemo startButtonText is not configured"); }
            if (restartButtonText == null) { Debug.LogWarning("ERROR: TechDemo restartButtonText is not configured"); }
            if (quitButtonText == null) { Debug.LogWarning("ERROR: TechDemo quitButtonText is not configured"); }
            if (qualityLowButtonText == null) { Debug.LogWarning("ERROR: TechDemo qualityLowButtonText is not configured"); }
            if (qualityMedButtonText == null) { Debug.LogWarning("ERROR: TechDemo qualityMedButtonText is not configured"); }
            if (qualityHighButtonText == null) { Debug.LogWarning("ERROR: TechDemo qualityHighButtonText is not configured"); }
            if (qualityLowBorderImg == null) { Debug.LogWarning("ERROR: TechDemo qualityLowBorderImg is not configured"); }
            if (qualityMedBorderImg == null) { Debug.LogWarning("ERROR: TechDemo qualityMedBorderImg is not configured"); }
            if (qualityHighBorderImg == null) { Debug.LogWarning("ERROR: TechDemo qualityHighBorderImg is not configured"); }
            if (missionOverPanel == null) { Debug.LogWarning("ERROR: TechDemo missionOverPanel is not configured"); }
            if (missionSuccessTitle == null) { Debug.LogWarning("ERROR: TechDemo missionSuccessTitle is not configured"); }
            if (missionSuccessSubTitle == null) { Debug.LogWarning("ERROR: TechDemo missionSuccessSubTitle is not configured"); }
            if (missionFailedTitle == null) { Debug.LogWarning("ERROR: TechDemo missionFailedTitle is not configured"); }
            if (missionFailedSubTitle == null) { Debug.LogWarning("ERROR: TechDemo missionFailedSubTitle is not configured"); }
            #endif

            eventSystem = EventSystem.current;

            colourNonSelectedBtnText = new Color(245f / 255f, 245f / 255f, 245f / 255f, 1f);
            colourSelectedBtnText = new Color(168f / 255f, 168f / 255f, 227f / 255f, 1f);
            colourDefaultBorder = new Color(72f / 255f, 72f / 255f, 188f / 255f, 40f/255f);

            // White with the same alpha a original border colour
            colourSelectedBorder = new Color(1f, 1f, 1f, 40f / 255f);

            StartCoroutine(ShowStart());

            #endregion

            #region Music Controller Initialisation
            musicController = GetComponent<scsmmedia.MusicController>();
            if (musicController != null) { musicController.Initialise(); }
            #endregion
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            // Game is paused/unpaused from a Custom Player Input on the Player Input Module
            // which calls TogglePause().
            if (!isGamePaused)
            {
                // Check if player is outside gameplay area
                if (!gameplayAreaBounds.Contains(player1Ship.shipInstance.TransformPosition))
                {
                    MissionFailed();
                }

                if (demoSectionIndex == 0)
                {
                    #region Demo Section 0
                    
                    if (!isHUDFirstTimeShown)
                    {
                        InitiateDemoSection1();
                        demoSectionIndex = 1;
                    }

                    #endregion
                }
                else if (demoSectionIndex == 1)
                {
                    #region Demo Section 1

                    // As soon as the player or a friendly AI ship gets close enough to the enemy base, initiate demo section 2
                    if (enemyBaseLocation != null)
                    {
                        bool initiateDemoSection2 = false;

                        // Check the player ship
                        if (player1Ship != null)
                        {
                            if (Vector3.SqrMagnitude(player1Ship.transform.position - enemyBaseLocation.position) < section2Dist * section2Dist)
                            {
                                initiateDemoSection2 = true;
                            }
                        }

                        // Check the squadron A ships
                        if (!initiateDemoSection2 && squadronAShips != null && squadronAShipsCount > 0)
                        {
                            for (int i = 0; i < squadronAShipsCount; i++)
                            {
                                if (Vector3.SqrMagnitude(squadronAShips[i].transform.position - enemyBaseLocation.position) < section2Dist * section2Dist)
                                {
                                    initiateDemoSection2 = true; i = squadronAShipsCount;
                                }
                            }
                        }

                        // Check the squadron B ships
                        if (!initiateDemoSection2 && squadronBShips != null && squadronAShipsCount > 0)
                        {
                            for (int i = 0; i < squadronAShipsCount; i++)
                            {
                                if (Vector3.SqrMagnitude(squadronAShips[i].transform.position - enemyBaseLocation.position) < section2Dist * section2Dist)
                                {
                                    initiateDemoSection2 = true; i = squadronBShipsCount;
                                }
                            }
                        }

                        if (initiateDemoSection2)
                        {
                            InitiateDemoSection2();
                            demoSectionIndex = 2;
                        }
                    }

                    #endregion
                }
                else if (demoSectionIndex == 2)
                {
                    #region Demo Section 2

                    // Have all the enemy surface turrets been destroyed?
                    if (enemySurfaceTurretsCount == 0)
                    {
                        InitiateDemoSection3();
                        demoSectionIndex = 3;
                    }

                    #endregion
                }
                else if (demoSectionIndex == 4)
                {
                    #region Demo Section 4
                    if (enemyCapitalShip != null && enemyCapitalShip.shipInstance.LocalVelocity.z > 25f)
                    {
                        InitiateDemoSection5();
                    }
                    #endregion
                }
                else if (demoSectionIndex == 5)
                {
                    #region Demo Section 5

                    #endregion
                }

                if (demoSectionIndex >= 3)
                {
                    #region Demo Sections 3-5

                    if (enemyCapitalShip != null)
                    {
                        // If we have not yet brought down the shields for the bridge, check if we should
                        if (enemyCapitalShip.shipInstance.localisedDamageRegionList[7].useShielding)
                        {
                            // Count the number of shield generators (their damage regions) with zero health
                            int currentNumShieldGeneratorsDestroyed = 0;
                            if (enemyCapitalShip.shipInstance.localisedDamageRegionList[3].Health <= 0f) { currentNumShieldGeneratorsDestroyed++; }
                            if (enemyCapitalShip.shipInstance.localisedDamageRegionList[4].Health <= 0f) { currentNumShieldGeneratorsDestroyed++; }
                            if (enemyCapitalShip.shipInstance.localisedDamageRegionList[5].Health <= 0f) { currentNumShieldGeneratorsDestroyed++; }
                            if (enemyCapitalShip.shipInstance.localisedDamageRegionList[6].Health <= 0f) { currentNumShieldGeneratorsDestroyed++; }

                            // Bring down the shield if the health of all the shield generator regions is zero
                            if (currentNumShieldGeneratorsDestroyed == 4)
                            {
                                enemyCapitalShip.shipInstance.localisedDamageRegionList[7].useShielding = false;
                                isBridgeAttackEnabled = true;

                                // We want to see the player attacking the capital ship bridge for the end-game scenario
                                if (isPOICameraDisplayed)
                                {
                                    SwitchPointOfInterestCamera(enemyCapitalShip);
                                }

                                // Update the objective message
                                if (tempDisplayMessageString == null) { tempDisplayMessageString = new scsmmedia.SCSMString(100); }
                                tempDisplayMessageString.Set("<b>Objective</b>: Destroy the bridge of the enemy capital ship.");
                                shipDisplayModule.SetDisplayMessageText(section3Message, tempDisplayMessageString.ToString());
                            }
                            else if (currentNumShieldGeneratorsDestroyed > prevNumShieldGeneratorsDestroyed)
                            {
                                // If we have destroyed one or more shield generators in the last frame, update the objective message
                                if (tempDisplayMessageString == null) { tempDisplayMessageString = new scsmmedia.SCSMString(100); }
                                tempDisplayMessageString.Set("<b>Objective</b>: Destroy the shield generators of the enemy capital ship (");
                                tempDisplayMessageString.Add(currentNumShieldGeneratorsDestroyed);
                                tempDisplayMessageString.Add("/4).");
                                shipDisplayModule.SetDisplayMessageText(section3Message, tempDisplayMessageString.ToString());
                                // Remember the number of shield generators we have destroyed
                                prevNumShieldGeneratorsDestroyed = currentNumShieldGeneratorsDestroyed;
                            }
                        }
                        // Else if the shields are already down, check if the bridge has been destroyed
                        else if (enemyCapitalShip.shipInstance.localisedDamageRegionList[7].Health <= 0f)
                        {
                            // If the bridge has been destroyed, destroy the capital ship
                            enemyCapitalShip.shipInstance.mainDamageRegion.useShielding = false;
                            enemyCapitalShip.shipInstance.ApplyNormalDamage(1000f, ProjectileModule.DamageType.Default, Vector3.zero);
                        }
                    }

                    #endregion
                }
            }
        }

        #endregion

        #region Private Member Methods

        /// <summary>
        /// Method to be invoked for undocking squadrons A and B.
        /// </summary>
        private void InitiallyUndockSquadronsAAndB ()
        {
            // Undock 1 ship from squadron A
            UndockSquadronShips(squadronAShips, 1);
            // Undock 1 ship from squadron B
            UndockSquadronShips(squadronBShips, 1);
        }

        /// <summary>
        /// Method to be invoked for undocking squadron D ships
        /// from the capital ship hangar bay.
        /// Switch the Point of Interest camera on second monitor (if available)
        /// to one of the enemy ships in Capital Ship Docking bay.
        /// </summary>
        private void InitiallyUndockSquadronD()
        {
            // Undock 1 ship from squadron D
            UndockSquadronShips(squadronDShips, 1);

            // This method is called multiple times, so only switch PoI camera once
            // This only works in a standalone build as second monitor support is simulated
            // in the editor game window.
            // Don't switch the PoI camera if the player is already attacking the capital ship bridge - we want to see that...
            if (isPOICameraDisplayed && !isBridgeAttackEnabled && !isPOIwithSquadronD && squadronDShipsCount > 1)
            {
                isPOIwithSquadronD = true;

                // Get the second squadron D ship
                ShipAIInputModule shipAIInputModuleInstance = squadronDShips[1];
                if (shipAIInputModuleInstance != null && shipAIInputModuleInstance.IsInitialised)
                {
                    ShipControlModule shipControlModuleInstance = shipAIInputModuleInstance.GetShipControlModule;

                    if (shipControlModuleInstance != null)
                    {
                        SwitchPointOfInterestCamera(shipControlModuleInstance);
                    }
                }
            }
        }

        /// <summary>
        /// Undocks shipsToUndock ships from shipsList (of the ships that are currently docked).
        /// Sets up a callback method for Squadron D from the capital ship.
        /// </summary>
        /// <param name="shipsList"></param>
        /// <param name="shipsToUndock"></param>
        private void UndockSquadronShips(List<ShipAIInputModule> shipsList, int shipsToUndock)
        {
            // Keep track of how many ships we have undocked in this function
            int shipsUndocked = 0;
            // Loop through all of the ships in this squadron
            ShipDocking shipDockingInstance;
            int shipsListCount = shipsList.Count;
            for (int i = 0; i < shipsListCount; i++)
            {
                // Check the docking state of this ship
                shipDockingInstance = shipsList[i].GetComponent<ShipDocking>();
                if (shipDockingInstance != null)
                {
                    if (shipDockingInstance.GetState() == ShipDocking.DockingState.Docked)
                    {
                        // Get notified when the docking state changes so we can adjust flight behaviour
                        if (shipsList[i].GetShip.squadronId == squadronDId)
                        {
                            shipDockingInstance.callbackOnStateChange = OnChangeDockingStateSquadronD;
                        }

                        // If this ship is docked, set the ship to undocking
                        shipDockingInstance.SetState(ShipDocking.DockingState.Undocking);
                        // Record that we have undocked this ship
                        shipsUndocked++;
                    }
                }
                // Once we have undocked enough ships, exit the loop
                if (shipsUndocked >= shipsToUndock)
                {
                    i = shipsListCount;
                }
            }
        }

        /// <summary>
        /// Create and initialises a squadron of AI ships in the idle state.
        /// </summary>
        /// <param name="squadronParentGameObject"></param>
        /// <param name="squadronShipPrefab"></param>
        /// <param name="squadronSpawnPoints"></param>
        private void InitialiseSquadron(GameObject squadronParentGameObject, ShipControlModule squadronShipPrefab,
            ShipDockingStation squadronDockingStation, int minDockingIndex, int maxDockingIndex,
            List<ShipAIInputModule> squadronShipList, int factionID, int squadronID)
        {
            if (squadronParentGameObject != null && squadronShipPrefab != null && squadronAShips != null &&
                squadronDockingStation != null && squadronDockingStation.shipDockingPointList != null &&
                squadronDockingStation.shipDockingPointList.Count > minDockingIndex &&
                squadronDockingStation.shipDockingPointList.Count > maxDockingIndex)
            {
                // Loop through the docking points
                for (int i = minDockingIndex; i <= maxDockingIndex; i++)
                {
                    // Instantiate the ship at the origin
                    GameObject shipGameObjectInstance = Object.Instantiate(squadronShipPrefab.gameObject,
                        Vector3.zero, Quaternion.identity);
                    // Append an index to the name
                    shipGameObjectInstance.name += " " + (i - minDockingIndex + 1).ToString();
                    // Parent the ship to the squadron transform
                    shipGameObjectInstance.transform.parent = squadronParentGameObject.transform;

                    // Add the ship to the Small Ships Unity Layer so that the Capital ship
                    // ignore it for obstacle avoidance
                    if (isSmallShipsLayerDefined) { shipGameObjectInstance.layer = smallShipsUnityLayer; }

                    // Get the ship control module instance
                    ShipControlModule shipControlModuleInstance = shipGameObjectInstance.GetComponent<ShipControlModule>();
                    // Get the ship AI input module instance
                    ShipAIInputModule shipAIInputModuleInstance = shipGameObjectInstance.GetComponent<ShipAIInputModule>();
                    // Get the ship docking instance
                    ShipDocking shipDockingInstance = shipGameObjectInstance.GetComponent<ShipDocking>();
                    if (shipControlModuleInstance != null && shipAIInputModuleInstance != null)
                    {
                        // Add the ship to the list
                        squadronShipList.Add(shipAIInputModuleInstance);

                        // Initialise the ship
                        shipControlModuleInstance.InitialiseShip();
                        shipAIInputModuleInstance.Initialise();
                        // Start with the ships in the idle state
                        if (shipAIInputModuleInstance.IsInitialised)
                        {
                            shipAIInputModuleInstance.SetState(AIState.idleStateID);
                        }
                        // Set the faction ID of the ship
                        shipControlModuleInstance.shipInstance.factionId = factionID;
                        // Set the squadron ID of the ship
                        shipControlModuleInstance.shipInstance.squadronId = squadronID;
                        // Set up callbacks
                        shipControlModuleInstance.callbackOnRespawn = OnRespawnCallback;
                        shipAIInputModuleInstance.callbackCompletedStateAction = CompletedStateActionCallback;

                        // In this game, ships start docked
                        if (shipDockingInstance != null)
                        {
                            if (!shipDockingInstance.IsInitialised) { shipDockingInstance.Initialise(); }
                            // Dock the ship with the correct docking point - this also sets its position
                            squadronDockingStation.AssignShipToDockingPoint(shipControlModuleInstance, shipDockingInstance, i);
                            shipDockingInstance.SetState(ShipDocking.DockingState.Docked);
                        }

                        // Set the spawn point of the ship
                        if (shipControlModuleInstance.shipInstance.squadronId != squadronID)
                        {
                            shipControlModuleInstance.shipInstance.customRespawnPosition = shipGameObjectInstance.transform.position;
                            shipControlModuleInstance.shipInstance.respawningMode = Ship.RespawningMode.RespawnAtSpecifiedPosition;
                        }
                        // Make the ship visible to radar
                        shipControlModuleInstance.EnableRadar();
                    }
                }
            }
        }

        /// <summary>
        /// Assigns targets for an entire squadron.
        /// </summary>
        /// <param name="squadronShipList"></param>
        /// <param name="squadronShipsCount"></param>
        /// <param name="radarQueryCentre"></param>
        /// <param name="radarQueryRange"></param>
        /// <param name="radarQueryFactionID"></param>
        /// <param name="radarQueryFactionsToExclude"></param>
        private void AssignSquadronTargets(SSCRadarQuery squadronRadarQuery, List<ShipAIInputModule> squadronShipList,
            int squadronShipsCount, Vector3 radarQueryCentre, float radarQueryRange, int[] radarQueryFactionsToInclude,
            int[] radarQueryFactionsToExclude, int[] radarQuerySquadronsToInclude, int[] radarQuerySquadronsToExclude,
            SSCRadarQuery.QuerySortOrder querySortOrder)
        {
            if (squadronShipList != null && squadronShipsCount > 0 && squadronRadarQuery != null)
            {
                // Set up radar query
                SetSquadronTargets(squadronRadarQuery, radarQueryCentre, radarQueryRange, radarQueryFactionsToInclude,
                    radarQueryFactionsToExclude, radarQuerySquadronsToInclude, radarQuerySquadronsToExclude, querySortOrder);
                // Execute the query
                sscRadar.GetRadarResults(squadronRadarQuery, sscRadarBlipsList);
                sscRadarBlipsListCount = sscRadarBlipsList.Count;

                if (sscRadarBlipsListCount > 0)
                {
                    // Loop through the ships of the specified squadron
                    int radarResultIndex = 0;
                    for (int shipIndex = 0; shipIndex < squadronShipsCount; shipIndex++)
                    {
                        // Get the radar result and assign it as this ship's target
                        squadronShipList[shipIndex].SetState(AIState.dogfightStateID);
                        // TODO: This won't work if the target is not a ship...
                        squadronShipList[shipIndex].AssignTargetShip(sscRadarBlipsList[radarResultIndex].shipControlModule);
                        // Increment radar result index
                        radarResultIndex++;
                        if (radarResultIndex >= sscRadarBlipsListCount) { radarResultIndex = 0; }
                    }
                }
                else
                {
                    Debug.Log("TechDemo2.cs AssignSquadronTargets: could not assign targets as none match the provided criteria.");
                }
            }
        }

        /// <summary>
        /// Sets up the radar query for a squadron, so that the correct targets can be found when we need them.
        /// </summary>
        /// <param name="squadronRadarQuery"></param>
        /// <param name="radarQueryCentre"></param>
        /// <param name="radarQueryRange"></param>
        /// <param name="radarQueryFactionsToInclude"></param>
        /// <param name="radarQueryFactionsToExclude"></param>
        /// <param name="radarQuerySquadronsToInclude"></param>
        /// <param name="radarQuerySquadronsToExclude"></param>
        private void SetSquadronTargets(SSCRadarQuery squadronRadarQuery, Vector3 radarQueryCentre, float radarQueryRange, 
            int[] radarQueryFactionsToInclude, int[] radarQueryFactionsToExclude, 
            int[] radarQuerySquadronsToInclude, int[] radarQuerySquadronsToExclude, SSCRadarQuery.QuerySortOrder querySortOrder)
        {
            if (squadronRadarQuery != null)
            {
                // Set up radar query - this will be used for assigning individual targets when we need them
                squadronRadarQuery.centrePosition = radarQueryCentre;
                squadronRadarQuery.range = radarQueryRange;
                squadronRadarQuery.factionId = -1;
                squadronRadarQuery.factionsToInclude = radarQueryFactionsToInclude;
                squadronRadarQuery.factionsToExclude = radarQueryFactionsToExclude;
                squadronRadarQuery.squadronsToInclude = radarQuerySquadronsToInclude;
                squadronRadarQuery.squadronsToExclude = radarQuerySquadronsToExclude;
                squadronRadarQuery.is3DQueryEnabled = false;
                squadronRadarQuery.querySortOrder = querySortOrder;
            }
        }

        /// <summary>
        /// Assigns a new target to an AI ship.
        /// </summary>
        /// <param name="shipAIInputModuleInstance"></param>
        /// <param name="shipControlModuleInstance"></param>
        /// <param name="squadronRadarQuery"></param>
        private void AssignNewTarget (ShipAIInputModule shipAIInputModuleInstance, ShipControlModule shipControlModuleInstance, 
            SSCRadarQuery squadronRadarQuery)
        {
            if (shipAIInputModuleInstance != null && shipControlModuleInstance != null && squadronRadarQuery != null)
            {
                // Execute the query
                sscRadar.GetRadarResults(squadronRadarQuery, sscRadarBlipsList);
                sscRadarBlipsListCount = sscRadarBlipsList.Count;

                if (sscRadarBlipsListCount > 0)
                {
                    // Choose a random radar blip
                    int chosenRadarBlipIndex = 0;
                    if ((int)squadronRadarQuery.querySortOrder == SSCRadarQuery.querySortOrderNoneInt)
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

                    if (chosenRadarBlip.radarItemType == SSCRadarItem.RadarItemType.AIShip || 
                        chosenRadarBlip.radarItemType == SSCRadarItem.RadarItemType.PlayerShip)
                    {
                        // Chosen target is a ship, so attack it with the dogfight state
                        shipAIInputModuleInstance.SetState(AIState.dogfightStateID);
                        shipAIInputModuleInstance.AssignTargetShip(chosenRadarBlip.shipControlModule);
                    }
                    else if (chosenRadarBlip.radarItemType == SSCRadarItem.RadarItemType.GameObject)
                    {
                        GameObject chosenRadarBlipGameObject = chosenRadarBlip.itemGameObject;
                        if (chosenRadarBlipGameObject != null)
                        {
                            SurfaceTurretModule surfaceTurretModuleInstance = chosenRadarBlipGameObject.GetComponent<SurfaceTurretModule>();
                            if (surfaceTurretModuleInstance != null)
                            {
                                // Chosen target is a turret, so attack it with the strafing run state
                                shipAIInputModuleInstance.SetState(AIState.strafingRunStateID);
                                shipAIInputModuleInstance.AssignTargetPosition(surfaceTurretModuleInstance.TransformPosition);
                                shipAIInputModuleInstance.AssignSurfaceTurretsToEvade(enemySurfaceTurrets);
                                shipAIInputModuleInstance.AssignTargetRadius(enemyTurretTargetRadius);
                            }
                            else
                            {
                                Debug.LogWarning("TechDemo2.cs AssignNewTarget: surface turret module instance is null.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("TechDemo2.cs AssignNewTarget: chosen radar blip gameobject is null.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("TechDemo2.cs AssignNewTarget: Radar blip has wrong type.");
                    }
                }
                else
                {
                    Debug.LogWarning("TechDemo2.cs AssignNewTarget: could not assign target as none match the provided criteria.");
                }
            }
            else
            {
                Debug.LogWarning("TechDemo2.cs AssignNewTarget: squadron radar query is null.");
            }
        }

        /// <summary>
        /// Assigns a new path to an AI ship.
        /// </summary>
        private void AssignNewPath (ShipAIInputModule shipAIInputModuleInstance, PathData newPathToFollow)
        {
            if (shipAIInputModuleInstance != null && newPathToFollow != null)
            {
                // Set the ship's state to the "move to" state
                shipAIInputModuleInstance.SetState(AIState.moveToStateID);
                // Set the target path to the new path
                shipAIInputModuleInstance.AssignTargetPath(newPathToFollow);
            }
        }

        /// <summary>
        /// Gives an AI ship new instructions.
        /// </summary>
        /// <param name="shipControlModuleInstance"></param>
        /// <param name="shipAIInputModuleInstance"></param>
        /// <param name="completedStateAction"></param>
        private void GetNewInstructions (ShipControlModule shipControlModuleInstance, 
            ShipAIInputModule shipAIInputModuleInstance, ShipDocking shipDockingInstance, bool completedStateAction, bool respawned)
        {
            if (shipControlModuleInstance != null && shipAIInputModuleInstance != null && shipDockingInstance != null)
            {
                int squadronId = shipControlModuleInstance.shipInstance.squadronId;

                if (respawned && !(demoSectionIndex >= 2 && (squadronId == squadronAId || squadronId == squadronBId)))
                {
                    #region Respawn Instructions

                    if (squadronId == squadronAId)
                    {
                        // Recently respawned. Needs to reset to docked position, then initiate undocking process.
                        friendlyBaseDockingStation.AssignShipToDockingPoint(shipControlModuleInstance, shipDockingInstance);
                        shipDockingInstance.SetState(ShipDocking.DockingState.Docked);
                        shipDockingInstance.SetState(ShipDocking.DockingState.Undocking);
                    }
                    else if (squadronId == squadronBId)
                    {
                        // Recently respawned. Needs to reset to docked position, then initiate undocking process.
                        friendlyBaseDockingStation.AssignShipToDockingPoint(shipControlModuleInstance, shipDockingInstance);
                        shipDockingInstance.SetState(ShipDocking.DockingState.Docked);
                        shipDockingInstance.SetState(ShipDocking.DockingState.Undocking);
                    }
                    else if (squadronId == squadronCId)
                    {
                        // Recently respawned. Needs to reset to docked position, then initiate undocking process.
                        enemyBaseDockingStation1.AssignShipToDockingPoint(shipControlModuleInstance, shipDockingInstance);
                        shipDockingInstance.SetState(ShipDocking.DockingState.Docked);
                        shipDockingInstance.SetState(ShipDocking.DockingState.Undocking);
                    }
                    else if (squadronId == squadronDId)
                    {
                        // If we have configured the capital ship hangar ship's to respawn at original location, don't attempt to launch them from the hangar dock again.
                        // At the moment, the initial setting for AI ships is overridden in InitialiseSquadron(..).
                        if (shipControlModuleInstance.shipInstance.respawningMode == Ship.RespawningMode.RespawnAtOriginalPosition)
                        {
                            // Recently respawned. Needs to reset to docked position, then initiate undocking process.
                            capitalshipDockingStation.AssignShipToDockingPoint(shipControlModuleInstance, shipDockingInstance);
                            shipDockingInstance.SetState(ShipDocking.DockingState.Docked);
                            shipDockingInstance.SetState(ShipDocking.DockingState.Undocking);
                        }
                        else
                        {
                            shipDockingInstance.SetState(ShipDocking.DockingState.NotDocked);
                        }
                    }

                    #endregion
                }
                else
                {
                    if (demoSectionIndex == 1)
                    {
                        #region Demo Section 1

                        if (squadronId == squadronAId)
                        {
                            #region Squadron A

                            // Finished undocking. Needs to follow the approach path.
                            AssignNewPath(shipAIInputModuleInstance, friendlyApproachPath);

                            #endregion
                        }
                        else if (squadronId == squadronBId)
                        {
                            #region Squadron B

                            // Finished undocking. Needs to follow the approach path.
                            AssignNewPath(shipAIInputModuleInstance, friendlyApproachPath);

                            #endregion
                        }

                        #endregion
                    }
                    else
                    {
                        #region Demo Sections 2-5

                        // For demo section 2, targets should be prioritised by distance to the enemy base
                        // For demo sections 3-5, targets should be prioritised by distance to the enemy capital ship
                        bool centreQueryAroundCapitalShip = demoSectionIndex > 2 && enemyCapitalShip != null;

                        if (squadronId == squadronAId)
                        {
                            #region Squadron A

                            // Get the current AI state
                            int currentStateID = shipAIInputModuleInstance.GetState();

                            if (currentStateID == AIState.moveToStateID)
                            {
                                PathData lastTargetPath = shipAIInputModuleInstance.GetTargetPath();

                                if (lastTargetPath == friendlyApproachPath || lastTargetPath == diversionPath1)
                                {
                                    // Finished following path. Need to get a target.
                                    AssignNewTarget(shipAIInputModuleInstance, shipControlModuleInstance, squadronARadarQuery);
                                }
                                else
                                {
                                    // Finished undocking. Need to follow approach path.
                                    AssignNewPath(shipAIInputModuleInstance, friendlyApproachPath);
                                }
                            }
                            else
                            {
                                // Destroyed the target. Need to get a new target.
                                if (centreQueryAroundCapitalShip) { squadronARadarQuery.centrePosition = enemyCapitalShip.transform.position; }
                                AssignNewTarget(shipAIInputModuleInstance, shipControlModuleInstance, squadronARadarQuery);
                            }

                            #endregion
                        }
                        else if (squadronId == squadronBId)
                        {
                            #region Squadron B

                            // Get the current AI state
                            int currentStateID = shipAIInputModuleInstance.GetState();

                            if (currentStateID == AIState.moveToStateID)
                            {
                                PathData lastTargetPath = shipAIInputModuleInstance.GetTargetPath();

                                if (lastTargetPath == friendlyApproachPath || lastTargetPath == diversionPath1)
                                {
                                    // Finished following path. Need to get a target.
                                    AssignNewTarget(shipAIInputModuleInstance, shipControlModuleInstance, squadronBRadarQuery);
                                }
                                else
                                {
                                    // Finished undocking. Need to follow approach path.
                                    AssignNewPath(shipAIInputModuleInstance, friendlyApproachPath);
                                }
                            }
                            else
                            {
                                // Destroyed the target, or else finished the strafing run. Need to get a new target.
                                if (centreQueryAroundCapitalShip) { squadronBRadarQuery.centrePosition = enemyCapitalShip.transform.position; }
                                AssignNewTarget(shipAIInputModuleInstance, shipControlModuleInstance, squadronBRadarQuery);
                            }

                            #endregion
                        }
                        else if (squadronId == squadronCId)
                        {
                            #region Squadron C

                            // Finished undocking, or else destroyed the target. Need to get a new target.
                            if (centreQueryAroundCapitalShip) { squadronCRadarQuery.centrePosition = enemyCapitalShip.transform.position; }
                            AssignNewTarget(shipAIInputModuleInstance, shipControlModuleInstance, squadronCRadarQuery);

                            #endregion
                        }
                        else if (squadronId == squadronDId)
                        {
                            #region Squadron D

                            // Finished undocking, or else destroyed the target. Need to get a new target.
                            if (centreQueryAroundCapitalShip) { squadronDRadarQuery.centrePosition = enemyCapitalShip.transform.position; }
                            AssignNewTarget(shipAIInputModuleInstance, shipControlModuleInstance, squadronDRadarQuery);

                            #endregion
                        }

                        #endregion
                    }
                }
            }
        }

        /// <summary>
        /// Will set the quality of the game based on the current
        /// gameQuality setting.
        /// This assumes the Ship AI Input Module is already initialised on
        /// all AI ships.
        /// </summary>
        private void SetGameQuality()
        {
            bool isHigh = gameQuality == GameQuality.High;
            bool isMedium = gameQuality == GameQuality.Medium;

            #region Terrain Settings
            if (landscapeParent != null)
            {
                Terrain[] terrains = landscapeParent.GetComponentsInChildren<Terrain>();
                int numTerrains = terrains == null ? 0 : terrains.Length;
                
                for (int tIdx = 0; tIdx < numTerrains; tIdx++)
                {
                    Terrain terrain = terrains[tIdx];
                    if (terrain != null && terrain.terrainData != null)
                    {
                        terrain.heightmapPixelError = isHigh ? 1f : isMedium ? 5f : 10f;
                        terrain.basemapDistance = isHigh ? 5000f : isMedium ? 3000f : 1000f;

                        #if UNITY_2019_1_OR_NEWER
                        terrain.shadowCastingMode = isHigh ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                        #else
                        terrain.castShadows = isHigh ? true : false;
                        #endif                      
                    }
                }
            }
            #endregion

            #region Lights

            if (friendlyBaseDockingStation != null)
            {
                Light[] friendlyBaseLights = friendlyBaseDockingStation.GetComponentsInChildren<Light>();
                int numLights = friendlyBaseLights == null ? 0 : friendlyBaseLights.Length;

                for (int lgtIdx = 0; lgtIdx < numLights; lgtIdx++)
                {
                    Light light = friendlyBaseLights[lgtIdx];
                    if (light != null)
                    {
                        if (light.type == LightType.Spot) { light.enabled = isHigh ? true : false; }
                        if (light.type == LightType.Point) { light.enabled = isHigh || isMedium ? true : false; }
                    }
                }
            }

            #endregion

            #region Particle Effects

            SetThrusterEffectQuality(squadronAShips);
            SetThrusterEffectQuality(squadronBShips);
            SetThrusterEffectQuality(squadronCShips);
            SetThrusterEffectQuality(squadronDShips);

            if (player1Ship != null)
            {
                if (isHigh || isMedium) { player1Ship.EnableThrusterEffects("Glow"); }
                else { player1Ship.DisableThrusterEffects("Glow"); }
                player1Ship.ReinitialiseThrusterEffects();
            }

            #endregion

            #region General
            // Override the target frame rate by not using the monitor refresh rate (vSync = 1)
            // Some devices can have a high monitor refresh rate like 120 or 144.
            QualitySettings.vSyncCount = 0;
            if (limitFrameRate) { Application.targetFrameRate = isHigh ? 60 : 30; }

            #endregion
        }

        /// <summary>
        /// Turn on or off some thruster particle effects based on the gameQuality level.
        /// This assumes the Ship AI Input Module is already initialised on all AI ships.
        /// For this demo we want to not use particle systems that produce a Glow Effect
        /// unless the gameQuality is High.
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
                        // with "Glow" in their name.
                        if (isHigh)
                        {
                            ship.EnableThrusterEffects("Glow");
                        }
                        else
                        {
                            ship.DisableThrusterEffects("Glow");
                        }

                        ship.ReinitialiseThrusterEffects();
                    }
                }
            }
        }

        /// <summary>
        /// Check to see if the Point of Interest camera is used on the second monitor (if there is one).
        /// Currently turning it off doesn't work due to a Unity limitation with additional displays....
        /// Only enable if Game Quality is High.
        /// NOTE: This only works in a standalone build as second monitor support is simulated in the editor Game view.
        /// In the editor Display.displays will always return 1. So as a workaround, go to Game view, click
        /// Add Tab->Game view and move it to the second monitor. Then, on second Game view, set Display to Display 2.
        /// In the editor, with 2+ Game views, Maximise On Play will have no effect.
        /// </summary>
        private void CheckPointOfInterestCamera()
        {
            // Point of interest camera is only available on standalone and in the editor.
            // It currently does not work in a UWP Win10 build. It is not supported on Xbox.
            // It makes no sense on android.
            #if (!UNITY_STANDALONE_OSX && !UNITY_STANDALONE_WIN && !UNITY_EDITOR)
            usePOICamera = false;
            #endif

            if (usePOICamera && !isPOICameraDisplayed && pointOfInterestCamera != null && gameQuality == GameQuality.High)
            {
                // Multiple displays can be activated (once) in a build and then cannot be de-activated for the current session
                if (SSCUtils.VerifyTargetDisplay(2, true))
                {
                    if (!pointOfInterestCamera.gameObject.activeSelf) { pointOfInterestCamera.gameObject.SetActive(true); }

                    // Switch the displays (monitors) around.
                    if (isSwitchDisplays)
                    {
                        pointOfInterestCamera.SetCameraTargetDisplay(1);
                        if (shipCameraModule1 != null) { shipCameraModule1.SetCameraTargetDisplay(2); }
                        if (shipDisplayModule != null) { shipDisplayModule.SetCanvasTargetDisplay(2); }
                        if (sscRadar != null) { sscRadar.SetCanvasTargetDisplay(2); }

                        if (player1Ship != null)
                        {
                            PlayerInputModule playerInputModule = player1Ship.GetComponent<PlayerInputModule>();
                            if (playerInputModule != null) { playerInputModule.SetTargetDisplay(2); }
                        }

                        if (menuPanel != null) { menuPanel.transform.parent.GetComponent<Canvas>().targetDisplay = 1; }
                    }

                    // Ensure the POI camera show stars
                    if (celestials != null && celestials.IsInitialised)
                    {
                        celestials.camera2 = pointOfInterestCamera.GetComponent<Camera>();
                        celestials.InitialiseCamera2();

                        if (isSwitchDisplays)
                        {
                            // We've updated the target displays for the two cameras, so need
                            // to refresh celestials.
                            celestials.RefreshCameras();
                        }
                    }

                    isPOICameraDisplayed = true;
                }
                else { DisablePointOfInterestCamera(); }
            }
            else { DisablePointOfInterestCamera(); }
        }

        /// <summary>
        /// Stop the PoI camera from rendering
        /// </summary>
        private void DisablePointOfInterestCamera()
        {
           if (pointOfInterestCamera != null)
           {
                pointOfInterestCamera.DisableCamera();
                if (pointOfInterestCamera.gameObject.activeSelf)
                {
                    pointOfInterestCamera.gameObject.SetActive(false);
                }
           }
        }

        /// <summary>
        /// Switch the Point of View camera to either the capital ship or one of the ships that were launched from the capital ship
        /// </summary>
        /// <param name="shipToWatch"></param>
        private void SwitchPointOfInterestCamera(ShipControlModule shipToWatch)
        {
            if (isPOICameraDisplayed && shipToWatch != null)
            {
                int _shipId = shipToWatch.shipInstance.shipId;

                if (_shipId == enemyCapitalShip.shipInstance.shipId)
                {
                    // The PoI camera will track along beside the capital ship and watch the action
                    pointOfInterestCamera.targetOffset = new Vector3(-600f, 400f, 0f);
                    pointOfInterestCamera.cameraRotationMode = ShipCameraModule.CameraRotationMode.AimAtTarget;
                    pointOfInterestCamera.lockToTargetPosition = false;
                    pointOfInterestCamera.lockToTargetRotation = false;
                }
                else
                {
                    // A ship from the capital ship hangar.
                    pointOfInterestCamera.targetOffset = new Vector3(0f, 2.5f, -9f);
                    pointOfInterestCamera.cameraRotationMode = ShipCameraModule.CameraRotationMode.FollowTargetRotation;
                    pointOfInterestCamera.lockToTargetPosition = true;
                    pointOfInterestCamera.lockToTargetRotation = true;
                }

                pointOfInterestCamera.SetTarget(shipToWatch);
            }
        }

        #endregion

        #region UI Methods

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
        /// </summary>
        /// <returns></returns>
        private IEnumerator ShowStart()
        {
            ShowMenu(false);

            // While the PlayerCamera is repositioning we need to prevent player ship moving
            if (player1Ship != null) { player1Ship.DisableShipMovement(); }

            yield return new WaitForSeconds(0.5f);
            PauseGame();
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

        #region Pause Game Methods

        /// <summary>
        /// Pause the game by:
        /// 1. Pausing the camera
        /// 2. Pausing the player ship
        /// 3. Pausing all AI squadrons
        /// 4. Pausing the Capital Ship if it is escaping
        /// </summary>
        private void PauseGame()
        {
            // Pause Cameras (assumes the camera is enabled) - this may not always be true...
            // Disable camera movement, making sure to perform camera movement for this frame
            if (shipCameraModule1 != null && shipCameraModule1.target != null)
            {
                shipCameraModule1.MoveCamera();
                shipCameraModule1.DisableCamera();
            }

            if (isPOICameraDisplayed)
            {
                if (pointOfInterestCamera.target != null) { pointOfInterestCamera.MoveCamera(); }
                pointOfInterestCamera.DisableCamera();
            }

            // Pause ships
            // Assume the player ship is always enabled. This could be a problem
            // if the ship is respawning...
            if (player1Ship != null && player1Ship.IsInitialised) { player1Ship.DisableShip(false); }

            // If this is the first time paused, reserve enough capacity for all the AI Ships
            int numSquadronAShips = squadronAShips == null ? 0 : squadronAShips.Count;
            int numSquadronBShips = squadronBShips == null ? 0 : squadronBShips.Count;
            int numSquadronCShips = squadronCShips == null ? 0 : squadronCShips.Count;
            int numSquadronDShips = squadronDShips == null ? 0 : squadronDShips.Count;

            if (pausedAIShips == null) { pausedAIShips = new List<ShipAIInputModule>(numSquadronAShips + numSquadronBShips + numSquadronCShips + numSquadronDShips); }

            AddSquadronToPausedList(numSquadronAShips, squadronAShips);
            AddSquadronToPausedList(numSquadronBShips, squadronBShips);
            AddSquadronToPausedList(numSquadronCShips, squadronCShips);
            AddSquadronToPausedList(numSquadronDShips, squadronDShips);

            // If the Capital Ship is moving, need to also disable it
            if (demoSectionIndex >= 4 && enemyCapitalShip != null && enemyCapitalShip.IsInitialised && enemyCapitalShip.ShipIsEnabled())
            {
                enemyCapitalShip.DisableShip(false);
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

            // turn off the radar mini-map
            if (sscRadar != null) { sscRadar.HideUI(); }

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
        /// 3. Unpausing the Capital Ship if it is escaping
        /// 4. Unpausing all AI squadrons and check if any are still docked in a hanger
        /// 5. Unpausing the camera
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

            // Unpause ships
            if (player1Ship != null && player1Ship.IsInitialised) { player1Ship.EnableShip(false, false); }

            // If the capital ship was taking off or escaping, re-enable it
            if (demoSectionIndex == 4 && enemyCapitalShip != null && enemyCapitalShip.IsInitialised)
            {
                enemyCapitalShip.EnableShip(false, false);
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
                            shipControlModule.EnableShip(false, false);

                            // Check if any AI ships are still docked in one of the hangers.
                            if (shipControlModule.GetShipDocking(false).GetStateInt() == ShipDocking.dockedInt)
                            {
                                // Re-dock the ship after EnableShip(..) was called.
                                shipControlModule.GetShipDocking(false).SetState(ShipDocking.DockingState.Docked);
                            }
                        }
                    }
                }
            }

            // Enable the cameras
            if (shipCameraModule1 != null) { shipCameraModule1.EnableCamera(); }

            if (isPOICameraDisplayed) { pointOfInterestCamera.EnableCamera(); }

            // Unpause game music (if any)
            if (musicController != null) { musicController.ResumeMusic(); }

            AudioListener.pause = false;
         
            if (shipDisplayModule != null)
            {
                // Due to a delayed startup procedure in the TechDemo, we need to
                // re-enable certain items on the HUD here.
                if (isHUDFirstTimeShown)
                {
                    isHUDFirstTimeShown = false;
                    // Show the active reticle. This will also turn on the heads-up display.
                    shipDisplayModule.ShowDisplayReticle();
                    shipDisplayModule.ShowAltitude();
                    shipDisplayModule.ShowAirSpeed();
                    shipDisplayModule.ShowDisplayGauges();
                }
                else
                {
                    shipDisplayModule.ShowHUD();
                }

                // If we have the menu and HUD on the second monitor, we don't want to centre the cursor
                // on the first monitor.
                if (!(isPOICameraDisplayed && isSwitchDisplays)) { shipDisplayModule.CentreCursor(); }
            }

            // turn on the radar mini-map
            if (sscRadar != null) { sscRadar.ShowUI(); }

            if (numPauseAIShips > 0) { pausedAIShips.Clear(); }
            isGamePaused = false;
        }

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

        #endregion

        #region Callback Methods

        /// <summary>
        /// Callback for when an AI ship completes a state action.
        /// </summary>
        /// <param name="shipAIInputModuleInstance"></param>
        public void CompletedStateActionCallback(ShipAIInputModule shipAIInputModuleInstance)
        {
            if (shipAIInputModuleInstance != null)
            {
                ShipControlModule shipControlModuleInstance = shipAIInputModuleInstance.GetShipControlModule;
                GetNewInstructions(shipControlModuleInstance, shipAIInputModuleInstance, shipControlModuleInstance == null ? null : shipControlModuleInstance.GetShipDocking(false), true, false);
            }
        }

        /// <summary>
        /// Callback for when an AI ship respawns.
        /// </summary>
        /// <param name="shipControlModuleInstance"></param>
        /// <param name="shipAIInputModuleInstance"></param>
        public void OnRespawnCallback(ShipControlModule shipControlModuleInstance, ShipAIInputModule shipAIInputModuleInstance)
        {
            if (shipControlModuleInstance != null)
            {
                GetNewInstructions(shipControlModuleInstance, shipAIInputModuleInstance, shipControlModuleInstance.GetShipDocking(false), false, true);
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
            }
        }

        /// <summary>
        /// Callback for when a surface turret is destroyed in the enemy base
        /// </summary>
        /// <param name="surfaceTurretModuleInstance"></param>
        public void OnSurfaceTurretDestroyed(SurfaceTurretModule surfaceTurretModuleInstance)
        {
            if (surfaceTurretModuleInstance != null)
            {
                for (int i = 0; i < enemySurfaceTurretsCount; i++)
                {
                    if (enemySurfaceTurrets[i] != null && enemySurfaceTurrets[i].GetInstanceID() == surfaceTurretModuleInstance.GetInstanceID())
                    {
                        // Remove the turret from the list
                        enemySurfaceTurrets.RemoveAt(i);
                        enemySurfaceTurretsCount--;
                        break;
                    }
                }

                // Update the objective message
                if (tempDisplayMessageString == null) { tempDisplayMessageString = new scsmmedia.SCSMString(100); }
                tempDisplayMessageString.Set("<b>Objective</b>: Destroy the enemy turrets (");
                tempDisplayMessageString.Add(4 - enemySurfaceTurretsCount);
                tempDisplayMessageString.Add("/4).");
                shipDisplayModule.SetDisplayMessageText(section2Message, tempDisplayMessageString.ToString());
            }
        }

        /// <summary>
        /// Called when a Squadron D ship (from the capital ship) changes it's docking state.
        /// We are using this to change the ship behaviour while undocking or in normal flight.
        /// </summary>
        /// <param name="shipDocking"></param>
        /// <param name="shipControlModule"></param>
        /// <param name="shipAIInputModule"></param>
        /// <param name="prevState"></param>
        public void OnChangeDockingStateSquadronD(ShipDocking shipDocking, ShipControlModule shipControlModule, ShipAIInputModule shipAIInputModule, ShipDocking.DockingState prevState)
        {
            int currentState = shipDocking.GetStateInt();

            if (currentState == ShipDocking.notDockedInt)
            {
                // Configure ship for normal flight 

                shipAIInputModule.movementAlgorithm = ShipAIInputModule.AIMovementAlgorithm.PlanarFlightBanking;

                // Lift off thruster
                shipControlModule.shipInstance.thrusterList[1].maxThrust = 10000f;

                // Left/Right thrusters
                shipControlModule.shipInstance.thrusterList[3].maxThrust = 0f;
                shipControlModule.shipInstance.thrusterList[4].maxThrust = 0f;

                // Reverse thruster
                shipControlModule.shipInstance.thrusterList[5].maxThrust = 10000f;

                // In normal flight, help the ship face the direction it is flying
                shipControlModule.shipInstance.arcadeMaxFlightTurningAcceleration = 300f;
            }
            else
            {
                // Configure ship for a docking procedure with the capital ship

                shipAIInputModule.movementAlgorithm = ShipAIInputModule.AIMovementAlgorithm.Full3DFlight;

                // Lift off thruster
                shipControlModule.shipInstance.thrusterList[1].maxThrust = 600000f;

                // Left/Right thrusters
                shipControlModule.shipInstance.thrusterList[3].maxThrust = 2000000f;
                shipControlModule.shipInstance.thrusterList[4].maxThrust = 2000000f;

                // Reverse thruster
                shipControlModule.shipInstance.thrusterList[5].maxThrust = 200000f;

                // When ship starts undocking the Flight Turn Acceleration should be 0
                shipControlModule.shipInstance.arcadeMaxFlightTurningAcceleration = 0f;
            }
        }

        /// <summary>
        /// Check to see if the Capital Ship has reached the end of the Hanger 1 exit path.
        /// If it has, the Player has failed to destroy it.
        /// </summary>
        /// <param name="shipDocking"></param>
        /// <param name="shipControlModule"></param>
        /// <param name="shipAIInputModule"></param>
        /// <param name="prevState"></param>
        public void OnMissionFailure(ShipDocking shipDocking, ShipControlModule shipControlModule, ShipAIInputModule shipAIInputModule, ShipDocking.DockingState prevState)
        {
            // Has the Capital ship reached the end of the hanger 1 exit path?
            if (shipDocking.GetStateInt() == ShipDocking.notDockedInt)
            {
                shipAIInputModule.SetState(AIState.idleStateID);

                MissionFailed();
            }
        }

        /// <summary>
        /// When the Capital Ship is destroyed, the mission has been successful.
        /// </summary>
        /// <param name="ship"></param>
        public void OnMissionSuccess(Ship ship)
        {
            // Destroy any ships are still docked in the capital ship
            for (int i = 0; i < squadronDShipsCount; i++)
            {
                ShipAIInputModule shipAIInputModuleInstance = squadronDShips[i];
                if (shipAIInputModuleInstance != null)
                {
                    ShipControlModule aiShipControlModuleInstance = shipAIInputModuleInstance.GetShipControlModule;

                    // Prevent squadron D (which launch from the capital ship) from respawning
                    aiShipControlModuleInstance.shipInstance.respawningMode = Ship.RespawningMode.DontRespawn;

                    ShipDocking shipDockingInstance = aiShipControlModuleInstance.GetShipDocking();
                    if (shipDockingInstance.IsInitialised && shipDockingInstance.GetStateInt() != ShipDocking.notDockedInt)
                    {
                        // Undock the AI ship from the now destroyed capital ship
                        shipDockingInstance.SetState(ShipDocking.DockingState.NotDocked);

                        // Destroy the squadron D ships which are probably docked or undocking from the capital ship when it explodes.
                        // Immediately apply an excessive amount of damage
                        aiShipControlModuleInstance.shipInstance.ApplyNormalDamage(1000f, ProjectileModule.DamageType.Default, Vector3.zero);
                    }
                }
            }

            // Give user a chance to see or hear any relevant explosions and the destruction of the capital ship.
            Invoke("MissionCompleted", 8f);
        }

        #endregion

        #region Public Member Methods

        #region Initiate Demo Section Methods

        /// <summary>
        /// Initiates section 1 of tech demo 2. Starts the friendly ships flying towards the enemy base from Hanger2.
        /// </summary>
        public void InitiateDemoSection1 ()
        {
            // Set squadron targets for squadrons A, B and C
            if (enemyBaseLocation != null)
            {
                // Set friendly squadron A to attack the enemy turrets
                SetSquadronTargets(squadronARadarQuery, enemyBaseLocation.position, enemyBaseRadius, null, null,
                    new int[] { squadronFId }, null, SSCRadarQuery.QuerySortOrder.None);

                // Set friendly squadron B to attack squadron C
                SetSquadronTargets(squadronBRadarQuery, enemyBaseLocation.position, enemyBaseRadius, null, null,
                    new int[] { squadronCId }, null, SSCRadarQuery.QuerySortOrder.None);

                // Set enemy squadron C to attack squadrons A and B and the player
                SetSquadronTargets(squadronCRadarQuery, enemyBaseLocation.position, 10000f, null, null,
                    new int[] { squadronAId, squadronBId, player1SquadronId }, null, SSCRadarQuery.QuerySortOrder.DistanceAsc3D);
            }

            // Invoke undocking methods to undock ships at different times
            for (int i = 0; i < Mathf.Max(squadronAShipsCount, squadronBShipsCount); i++)
            {
                Invoke("InitiallyUndockSquadronsAAndB", i*friendlyUndockingDelay + 1f);
            }

            // Tell the player to fly to the enemy base
            // NOTE: This currently makes msg appear just before HUD is disabled (not ideal...) 
            shipDisplayModule.ShowDisplayMessage(section1Message);
        }

        /// <summary>
        /// Initiates section 2 of tech demo 2.
        /// Sets all of the enemy surface turrets to auto fire mode,
        /// undocks squadron C and sets the player/friendly respawn point.
        /// </summary>
        public void InitiateDemoSection2 ()
        {
            if (enemySurfaceTurrets != null)
            {
                // Loop through all of the enemy surface turrets
                for (int i = 0; i < enemySurfaceTurretsCount; i++)
                {
                    // Set each turret to auto fire mode
                    enemySurfaceTurrets[i].SetAutoFire();
                }
            }

            // Undock all of the ships in squadron C
            if (enemyBaseDockingStation1 != null)
            {
                if (squadronCShips != null && squadronCShipsCount > 0)
                {
                    // Loop through all of the ships in squadron C
                    ShipDocking shipDockingInstance;
                    for (int i = 0; i < squadronCShipsCount; i++)
                    {
                        // Set the ship to undocking
                        shipDockingInstance = squadronCShips[i].GetComponent<ShipDocking>();
                        if (shipDockingInstance != null)
                        {
                            shipDockingInstance.SetState(ShipDocking.DockingState.Undocking);
                        }
                    }
                }
            }

            // Set the player respawn point
            player1Ship.shipInstance.respawningMode = Ship.RespawningMode.RespawnAtSpecifiedPosition;
            player1Ship.shipInstance.customRespawnPosition = section2PlayerRespawnLocation.position;
            player1Ship.shipInstance.customRespawnRotation = Vector3.up * -90f;

            // Set the respawn point for squadrons A and B

            if (squadronAShips != null && squadronAShipsCount > 0)
            {
                // Loop through all of the ships in squadron A
                ShipAIInputModule shipAIInputModuleInstance;
                for (int i = 0; i < squadronAShipsCount; i++)
                {
                    shipAIInputModuleInstance = squadronAShips[i];

                    if (shipAIInputModuleInstance != null)
                    {
                        // Set the spawn point of the ship
                        shipAIInputModuleInstance.GetShipControlModule.shipInstance.customRespawnPosition = section2PlayerRespawnLocation.position - (Vector3.forward * i * 20f);
                        shipAIInputModuleInstance.GetShipControlModule.shipInstance.customRespawnRotation = Vector3.up * -90f;
                        shipAIInputModuleInstance.GetShipControlModule.shipInstance.respawningMode = Ship.RespawningMode.RespawnAtSpecifiedPosition;
                    }
                }
            }

            if (squadronBShips != null && squadronBShipsCount > 0)
            {
                // Loop through all of the ships in squadron B
                ShipAIInputModule shipAIInputModuleInstance;
                for (int i = 0; i < squadronBShipsCount; i++)
                {
                    shipAIInputModuleInstance = squadronBShips[i];

                    if (shipAIInputModuleInstance != null)
                    {
                        // Set the spawn point of the ship
                        shipAIInputModuleInstance.GetShipControlModule.shipInstance.customRespawnPosition = section2PlayerRespawnLocation.position + (Vector3.forward * i * 20f);
                        shipAIInputModuleInstance.GetShipControlModule.shipInstance.customRespawnRotation = Vector3.up * -90f;
                        shipAIInputModuleInstance.GetShipControlModule.shipInstance.respawningMode = Ship.RespawningMode.RespawnAtSpecifiedPosition;
                    }
                }
            }

            // Tell the player to attack the enemy turrets
            shipDisplayModule.HideDisplayMessage(section1Message);
            shipDisplayModule.ShowDisplayMessage(section2Message);
        }

        /// <summary>
        /// Initiates section 3 of tech demo 2.
        /// Capital ship gets ready for departure
        /// </summary>
        public void InitiateDemoSection3 ()
        {
            if (enemyCapitalShip != null)
            {
                ShipDocking enemyCapitalShipDocking = enemyCapitalShip.GetShipDocking();
                ShipAIInputModule enemyCapitalShipAI = enemyCapitalShip.GetShipAIInputModule();

                if (enemyCapitalShipAI != null && enemyCapitalShipDocking != null)
                {
                    enemyCapitalShip.EnableShip(false, true);

                    enemyCapitalShipDocking.callbackOnStateChange = OnMissionFailure;

                    // Open the docking station bay doors
                    if (enemyBaseDockingStation2 != null)
                    {
                        SSCDoorAnimator doorAnim = enemyBaseDockingStation2.GetComponent<SSCDoorAnimator>();
                        if (doorAnim != null)
                        {
                            doorAnim.OpenSpeed = 0.2f;
                            doorAnim.OpenDoors();

                            Invoke("CapitalShipTakeOff", 6f);
                        }
                    }
                }
            }

            if (squadronAShips != null && squadronAShipsCount > 0)
            {
                // Loop through all of the ships in squadron A
                ShipAIInputModule shipAIInputModuleInstance;
                for (int i = 0; i < squadronAShipsCount; i++)
                {
                    shipAIInputModuleInstance = squadronAShips[i];

                    if (shipAIInputModuleInstance != null)
                    {
                        // Assign them to follow the first diversion path
                        AssignNewPath(shipAIInputModuleInstance, diversionPath1);

                        // Set them to respawn at their last position
                        shipAIInputModuleInstance.GetShipControlModule.shipInstance.respawningMode = Ship.RespawningMode.RespawnAtLastPosition;
                    }
                }
            }

            if (squadronBShips != null && squadronBShipsCount > 0)
            {
                // Loop through all of the ships in squadron B
                ShipAIInputModule shipAIInputModuleInstance;
                for (int i = 0; i < squadronBShipsCount; i++)
                {
                    shipAIInputModuleInstance = squadronBShips[i];

                    if (shipAIInputModuleInstance != null)
                    {
                        // Assign them to follow the first diversion path
                        AssignNewPath(shipAIInputModuleInstance, diversionPath1);

                        // Set them to respawn at their last position
                        shipAIInputModuleInstance.GetShipControlModule.shipInstance.respawningMode = Ship.RespawningMode.RespawnAtLastPosition;
                    }
                }
            }

            if (squadronCShips != null && squadronCShipsCount > 0)
            {
                // Loop through all of the ships in squadron C
                ShipAIInputModule shipAIInputModuleInstance;
                for (int i = 0; i < squadronCShipsCount; i++)
                {
                    shipAIInputModuleInstance = squadronCShips[i];

                    if (shipAIInputModuleInstance != null)
                    {
                        // Assign them to follow the first diversion path
                        AssignNewPath(shipAIInputModuleInstance, diversionPath1);

                        // Set them to respawn at their last position
                        shipAIInputModuleInstance.GetShipControlModule.shipInstance.respawningMode = Ship.RespawningMode.RespawnAtLastPosition;
                    }
                }
            }

            // Set the player to respawn at their last position
            player1Ship.shipInstance.respawningMode = Ship.RespawningMode.RespawnAtLastPosition;

            // Tell the player to attack the enemy capital ship
            shipDisplayModule.HideDisplayMessage(section2Message);
            shipDisplayModule.ShowDisplayMessage(section3Message);
        }

        /// <summary>
        /// Squadrons A, B and Player to engage capital Ship.
        /// </summary>
        public void InitiateDemoSection4()
        {
            // Capital Ship uses on-board weapons and radar to defend itself
            // This is controlled by Auto-Targeting component attached to Capital Ship.
            // Capital Ship weapons are set to Auto-Targeting

            // Squadrons A and B engage Capital Ship

            // Set friendly squadron A to attack squadron D (the enemy capital ship squadron)
            SetSquadronTargets(squadronARadarQuery, enemyBaseLocation.position, 10000f, null, null,
                new int[] { squadronCId, squadronDId }, null, SSCRadarQuery.QuerySortOrder.None);

            // Set friendly squadron B to attack squadron D (the enemy capital ship squadron)
            SetSquadronTargets(squadronBRadarQuery, enemyBaseLocation.position, 10000f, null, null,
                new int[] { squadronDId }, null, SSCRadarQuery.QuerySortOrder.None);

            // if enabled on the second display, switch the Point of Interest camera to the capital ship
            if (isPOICameraDisplayed)
            {
                if (pointOfInterestCamera.target == null && enemyCapitalShip != null) { pointOfInterestCamera.SetTarget(enemyCapitalShip); }
            }
        }

        /// <summary>
        /// Launch Squadron D from Capital Ship
        /// Have squadron D engage (friendly) squadrons A and B
        /// </summary>
        public void InitiateDemoSection5()
        {
            // Launch Squadron D from Capital Ship
            if (enemyCapitalShip != null)
            {
                // Invoke undocking methods to undock ships at different times from the capital ship hangar bay.
                for (int i = 0; i < squadronDShipsCount; i++)
                {
                    Invoke("InitiallyUndockSquadronD", i * capitalUndockingDelay + 1f);
                }

                // Set enemy squadron D to attack squadrons A and B and the player
                // TODO this should do some sort of priority using distance to the capital ship
                SetSquadronTargets(squadronDRadarQuery, enemyBaseLocation.position, 10000f, null, null,
                    new int[] { squadronAId, squadronBId, player1SquadronId }, null, SSCRadarQuery.QuerySortOrder.DistanceAsc3D);
            }

            demoSectionIndex = 5;
        }

        /// <summary>
        /// Enable Capital Ship and undock from Hanger1
        /// </summary>
        public void CapitalShipTakeOff()
        {
            if (enemyCapitalShip != null)
            {
                ShipDocking enemyCapitalShipDocking = enemyCapitalShip.GetShipDocking();
                ShipAIInputModule enemyCapitalShipAI = enemyCapitalShip.GetShipAIInputModule();

                if (enemyCapitalShipAI != null && enemyCapitalShipDocking != null)
                {
                    enemyCapitalShip.EnableShip(false, true);

                    enemyCapitalShipDocking.SetState(ShipDocking.DockingState.Undocking);

                    InitiateDemoSection4();
                    demoSectionIndex = 4;
                }
            }
        }

        #endregion

        #region Camera Methods

        /// <summary>
        /// Cycles the camera view for the tech demo 2 player camera.
        /// </summary>
        public void TechDemoCycleCameraView (Vector3 inputValue, int customPlayerInputEventType)
        {
            if (changeCameraView != null)
            {
                // Cycle the camera view
                changeCameraView.CycleCameraView(inputValue, customPlayerInputEventType);

                // Update the HUD depending on whether the new view is inside or outside the cockpit
                int newCameraViewIndex = changeCameraView.GetCurrentCameraViewIndex();
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
            if (!isPOICameraDisplayed) { CheckPointOfInterestCamera(); }
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

        #region Public Teleport Test Methods

        /// <summary>
        /// Can be called by setting up a Custom Player Input on the Player Input Module.
        /// This is some test code which basically gets the friendly ships to the citidel
        /// instantly, and surprises the enemy. Made really to help test the capital ship
        /// attack. We left it here just to demonstrate how you can teleport ships.
        /// </summary>
        public void TeleportCheat()
        {
            if (demoSectionIndex == 1)
            {
                Vector3 deltaTP = enemyBaseLocation.position + (Vector3.up * 450f) + (Vector3.right * 750f) + (Vector3.forward) - player1Ship.shipInstance.TransformPosition;
                player1Ship.TelePort(deltaTP, false);

                InitiateDemoSection2();
                demoSectionIndex = 2;

                for (int i = 0; i < squadronAShipsCount; i++)
                {
                    squadronAShips[i].TelePort(deltaTP, false);
                    CompletedStateActionCallback(squadronAShips[i]);
                }
                for (int i = 0; i < squadronBShipsCount; i++)
                {
                    squadronBShips[i].TelePort(deltaTP, false);
                    CompletedStateActionCallback(squadronBShips[i]);
                }

                sscManager.TelePortProjectiles(deltaTP);
            }
        }

        #endregion
    }
}
