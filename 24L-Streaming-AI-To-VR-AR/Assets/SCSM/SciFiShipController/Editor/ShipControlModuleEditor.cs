#define _SSC_SHOW_DRAG_MOMENTS
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(ShipControlModule))]
    public class ShipControlModuleEditor : Editor
    {
        #region Enumerations

        #endregion

        #region Custom Editor private variables

        private ShipControlModule shipControlModule = null;
        private Rigidbody shipRigidbody;

        private int index = 0;
        private List<Thruster> thrustersList = null;
        private List<GameObject> thrusterEffectsList = null;
        private int thrusterMoveDownPos = -1;
        private int thrusterInsertPos = -1;
        private int thrusterDeletePos = -1;
        private int numFilteredThrusters = 0;

        private int wingMoveDownPos = -1;
        private int wingInsertPos = -1;
        private int wingDeletePos = -1;

        private int controlSurfaceMoveDownPos = -1;
        private int controlSurfaceInsertPos = -1;
        private int controlSurfaceDeletePos = -1;

        private int weaponMoveDownPos = -1;
        private int weaponInsertPos = -1;
        private int weaponDeletePos = -1;
        private int firePosIndex = 0;

        private int damageRegionMoveDownPos = -1;
        private int damageRegionInsertPos = -1;
        private int damageRegionDeletePos = -1;

        private string[] thrusterForceUseStrings = { "None", "Forward Thrust", "Backward Thrust", "Upward Thrust", "Downward Thrust",
        "Rightward Thrust", "Leftward Thrust" };
        private int[] thrusterForceUseInts = { 0, 1, 2, 3, 4, 5, 6 };

        private string[] thrusterMomentUseStrings = { "None", "Positive Roll", "Negative Roll", "Positive Pitch", "Negative Pitch",
        "Positive Yaw", "Negative Yaw" };
        private int[] thrusterMomentUseInts = { 0, 1, 2, 3, 4, 5, 6 };

        private int[] damageRegionIndexInts = { -1 };
        private GUIContent[] damageRegionIndexContents = { new GUIContent("None") };

        // Used for StuckAction of RespawnOnPath
        private GUIContent[] pathContents = { new GUIContent("None") };

        private SSCManager sscManager = null;

        // Formatting and style variables
        private string txtColourName = "Black";
        private Color defaultTextColour = Color.black;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle foldoutStyleNoLabel;
        private static GUIStyle toggleCompactButtonStyleNormal = null;  // Small Toggle button. e.g. G(izmo) on/off
        private static GUIStyle toggleCompactButtonStyleToggled = null;
        private GUIStyle searchTextFieldStyle = null;
        private GUIStyle searchCancelButtonStyle = null;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        private float defaultSearchTextFieldWidth = 245f;
        private Color separatorColor = new Color();

        // Thruster list copy and paste
        [System.NonSerialized] private static List<Thruster> thrusterCopyBuffer = null;

        // Debugging
        private bool isDebuggingEnabled = false;
        private bool debugIsShowShipSpeedEnabled = false;
        private bool debugIsShowShipInput = false;
        private bool debugIsShowThrusters = false;
        private bool debugIsShowWeapons = false;
        private bool debugIsShowHealth = false;
        private ShipInput shipInput = new ShipInput();

        // Similar to isSceneDirtyRequired (SceneView variabled) but used for Inspector modifications.
        private bool isSceneModified = false;

        private string sscHelpPDF;

        #endregion

        #region Static Readonly Strings
        //private static readonly string destructRespawnWaring = "With Destruct Object AND respawning on at the same time, you will need to consider their timings.";
        #endregion

        #region SceneView Variables

        private bool isSceneDirtyRequired = false;

        private Quaternion sceneViewShipRotation = Quaternion.identity;
        private float handleDistanceScale = 1f;

        private bool isCentreOfMassSelected = false;
        private Vector3 centreOfMassHandlePosition = Vector3.zero;
        private Quaternion centreOfMassHandleRotation = Quaternion.identity;

        private Vector3 centreOfLiftHandlePosition = Vector3.zero;
        private Vector3 directionOfLiftHandleVector = Vector3.up;

        private Vector3 centreOfThrustHandlePosition = Vector3.zero;
        private Vector3 directionOfThrustHandleVector = Vector3.back;

        private Wing wingComponent;
        private Thruster thrusterComponent;
        private ControlSurface controlSurfaceComponent;
        private Weapon weaponComponent;
        private DamageRegion damageRegionCommponent;
        private Vector3 componentHandlePosition = Vector3.zero;
        private Vector3 gizmoPosition = Vector3.zero;
        private Quaternion componentHandleRotation = Quaternion.identity;
        private Quaternion fireDirectionRotation = Quaternion.identity;
        private Vector3 componentHandleScale = Vector3.one;
        private List<Vector3> weaponFirePositionList;
        private int numCompWeaponFirePositions;

        private Vector3 wingLengthDir = Vector3.right;
        private Vector3 wingWidthDir = Vector3.forward;

        private Vector3 controlSurfaceLengthDir = Vector3.right;
        private Vector3 controlSurfaceWidthDir = Vector3.forward;

        // Pale Green
        private Color weaponGizmoColour = new Color(152f/255f, 251f/255f, 152f/255f, 1f);

        // Yellow for Y axis rotation arc
        private Color weaponGizmoTurretYColour = new Color(1f, 0.92f, 0.016f, 0.7f);

        // Red for X axis rotation arc
        private Color weaponGizmoTurretXColour = new Color(1f, 0f, 0f, 0.4f);

        // Pastel Blue
        private Color damageRegionGizmoColour = new Color(186f/255f, 225f/255f, 255f/255f, 1f);

        #endregion

        #region GUIContent General

        private readonly static GUIContent headerContent = new GUIContent("<b>Ship Control Module</b>\n\nThis module enables you to implement ship behaviour on the object it is attached to.");
        private readonly static GUIContent[] tabTexts = { new GUIContent("Physics"), new GUIContent("Control"), new GUIContent("Thrusters"), new GUIContent("Aero"), new GUIContent("Combat") };
        private readonly static GUIContent gizmoBtnContent = new GUIContent("G", "Toggle gizmos on/off in the scene view");
        private readonly static GUIContent gizmoToggleBtnContent = new GUIContent("G", "Toggle gizmos and visualisations on/off for all items in the scene view");
        private readonly static GUIContent gizmoFindBtnContent = new GUIContent("F", "Find (select) in the scene view.");
        private readonly static GUIContent filterContent = new GUIContent("<b>Filter</b>", "Filter the items in the list by entering part of the item name");

        private readonly static GUIContent partProgressiveDamageContent = new GUIContent("Use Progressive Damage", "Whether this part's " +
            "performance is impacted by damage done to the ship.");
        private readonly static GUIContent partDamageRegionContent = new GUIContent("Damage Region", "Which damage region " +
            "this part belongs to.");
        private readonly static GUIContent partStartingHealthContent = new GUIContent("Starting Health", "The initial health value " +
            "of this part. This is the amount of damage that needs to be done to the damage region this part is associated with for " +
            "the part to reach its min performance.");

        private readonly static GUIContent gotoEffectFolderBtnContent = new GUIContent("F", "Find and highlight the sample Effects folder");
        private readonly static GUIContent gotoDestructsFolderBtnContent = new GUIContent("F", "Find and highlight the sample Destructs folder");
        private readonly static GUIContent gotoBeamFolderBtnContent = new GUIContent("F", "Find and highlight the sample Beam folder");
        private readonly static GUIContent gotoProjectileFolderBtnContent = new GUIContent("F", "Find and highlight the sample Projectiles folder");

        #endregion

        #region GUIContent Physics

        private readonly static GUIContent initialiseOnAwakeContent = new GUIContent(" Initialise On Awake", "If enabled, the " +
            "InitialiseShip() will be called as soon as Awake() runs for the ship. This should be disabled if you are " +
            "instantiating the ship through code.");
        private readonly static GUIContent turnOffAllGizmosContent = new GUIContent("Turn Off Gizmos", "Turn off all the gizmos in the scene view for this Ship.");
        private readonly static GUIContent physicsModelHeaderContent = new GUIContent("<b>Physics Model</b>\n\n" +
            "The physics model determines which options are available for ship control, as well as how physics for certain things " +
            "such as thrusters is handled. Select Physics-Based mode to create physically realistic ships and aircraft, or select " +
            "Arcade mode to replicate the behaviour of more fictitious craft.");
        private readonly static GUIContent shipPhysicsModelContent = new GUIContent(" Physics Model", "Determines which options are " +
            "available for ship control. In Physics-Based mode, everything is constrained to the most physically realistic values and " +
            "settings. Ships can only be turned using thrusters and control surfaces. In Arcade mode, some more unrealistic properties " +
            "and settings are exposed to allow for different gameplay experiences as well as more intuitive setup.");
        private readonly static GUIContent massHeaderContent = new GUIContent("<b>Mass</b>\n\n" +
            "The property of 'mass' determines how heavy the ship is (in kilograms). The point through which all of the mass of the ship " +
            "seems to act is known as the 'centre of mass' (CoM) and is indicated by the grey sphere in the scene view. When 'Set CoM " +
            "Manually' is disabled, the centre of mass will be automatically determined by Unity based on the position of any colliders " +
            "attached to the rigidbody. Otherwise, the centre of mass can be adjusted by modifying the value of 'Centre of Mass' in the " +
            "inspector or by selecting the grey sphere in the scene view and dragging it with the move tool. The 'Reset Centre Of Mass' " +
            "button resets the centre of mass to the position automatically determined by Unity.");
        private readonly static GUIContent massContent = new GUIContent(" Mass", "Mass of the ship in kilograms.");
        private readonly static GUIContent manualCentreOfMassContent = new GUIContent(" Set CoM Manually", "When enabled, the centre of mass " +
            "(the point through which all forces on the ship act) can be edited manually. When disabled, the centre of mass will be " +
            "positioned at the default position specified by Unity - this is determined by the ship's colliders.");
        private readonly static GUIContent centreOfMassContent = new GUIContent(" Centre of Mass", "The position of the centre of mass " +
            "in local space.");
        private readonly static GUIContent resetCentreOfMassButtonContent = new GUIContent("Reset Centre Of Mass", "Resets the centre of " +
            "mass to the default position specified by Unity - this is determined by the ship's colliders.");
        private readonly static GUIContent gravityHeaderContent = new GUIContent("<b>Gravity</b>\n\n" +
            "Acceleration changes the strength of gravity. Increasing it increases the pull of gravity. Direction " +
            "changes the direction in which gravity acts. This direction is indicated by the grey arrow in the scene view, and can be " +
            "adjusted by selecting the grey sphere and using the rotation tool.");
        private readonly static GUIContent gravitationalAccelerationContent = new GUIContent(" Acceleration", "The acceleration due to " +
            "gravity in metres per second squared. Earth gravity is approximately 9.81 m/s^2.");
        private readonly static GUIContent gravityDirectionContent = new GUIContent(" Direction", "The direction in which gravity acts on " +
            "the ship in world space.");
        private readonly static GUIContent arcadeMomentAccelerationHeaderContent = new GUIContent("<b>Pitch/Roll/Yaw Acceleration</b>\n\n" +
            "Use Pitch/Roll/Yaw Acceleration to determine how quickly the ship can turn on each axis.");
        private readonly static GUIContent arcadePitchAccelerationContent = new GUIContent(" Pitch Acceleration", "How fast the ship " +
            "accelerates when pitching up and down in degrees per second squared. Increasing this value will increase how fast the " +
            "ship can be pitched up and down by pilot and rotational flight assist inputs.");
        private readonly static GUIContent arcadeRollAccelerationContent = new GUIContent(" Roll Acceleration", "How fast the ship " +
            "accelerates when rolling left and right in degrees per second squared. Increasing this value will increase how fast the " +
            "ship can be rolled left and right by pilot and rotational flight assist inputs.");
        private readonly static GUIContent arcadeYawAccelerationContent = new GUIContent(" Yaw Acceleration", "How fast the ship " +
            "accelerates when turning left and right in degrees per second squared. Increasing this value will increase how fast the " +
            "ship can be turned left and right by pilot and rotational flight assist inputs.");
        private readonly static GUIContent arcadeTurnAccelerationHeaderContent = new GUIContent("<b>Turn Acceleration</b>\n\n" +
            "Use turn acceleration to add a more 'arcade' feel to ship movement. Flight Turn Acceleration adds force inputs to cause " +
            "the ship to move in the direction it is facing while in the air, while Ground Turn Acceleration does the same while the " +
            "ship is near the ground.");
        private readonly static GUIContent arcadeMaxFlightTurnAccContent = new GUIContent(" Flight Turn Acceleration", "How quickly the " +
            "ship accelerates in metres per second squared while in the air to move in the direction the ship is facing.");
        private readonly static GUIContent arcadeMaxGroundTurnAccContent = new GUIContent(" Ground Turn Acceleration", "How quickly the " +
            "ship accelerates in metres per second squared while near the ground to move in the direction the ship is facing.");

        #endregion

        #region GUIContent Control

        private readonly static GUIContent rotationalFlightAssistHeaderContent = new GUIContent("<b>Rotational Flight Assist</b>\n\n" +
            "Rotational Flight Assist helps a pilot to control a ship by applying axial inputs to oppose rotational velocity and slow down " +
            "spinning motion when the pilot releases the input on that axis.");
        private readonly static GUIContent rotationFlightAssistStrengthContent = new GUIContent(" RFA Strength", "Increasing this value will " +
            "increase how quickly rotational flight assist slows down spinning motions. Setting it to zero will disable rotational flight " +
            "assist entirely.");
        private readonly static GUIContent translationalFlightAssistHeaderContent = new GUIContent("<b>Translational Flight Assist</b>\n\n" +
            "Translational Flight Assist helps a pilot to control a ship by applying translational inputs to oppose movement on non-forward axes " +
            "when the pilot releases the input on those axes, which aligns the ship's velocity more with its forward direction, " +
            "making turning easier and more intuitive.");
        private readonly static GUIContent translationalFlightAssistStrengthContent = new GUIContent(" TFA Strength", "Increasing this value will " +
            "increase how quickly translational flight assist slows down movement in a particular direction. Setting it to zero will " +
            "disable translational flight assist entirely.");
        private readonly static GUIContent stabilityFlightAssistHeaderContent = new GUIContent("<b>Stability Flight Assist</b>\n\n" +
            "Stability Flight Assist helps a pilot to control a ship by applying rotational inputs to keep the ship stable at its " +
            "current orientation when the pilot releases rotational inputs.");
        private readonly static GUIContent stabilityFlightAssistStrengthContent = new GUIContent(" SFA Strength", "Increasing this value will " +
            "increase how rigidly stability flight assist keeps the ship stable. Setting it to zero will disable stability flight " +
            "assist entirely.");
        private readonly static GUIContent brakeFlightAssistHeaderContent = new GUIContent("<b>Brake Flight Assist</b>\n\n" +
            "Brake Flight Assist helps a pilot to slow a ship when the pilot releases the input on the forward or backward (z) axis. " +
            "At slow speeds it can bring a ship to a complete stop.");
        private readonly static GUIContent brakeFlightAssistStrengthContent = new GUIContent(" BFA Strength Z", "Increasing this value will increase how quickly brake flight assist slows down forward or backward movent when no forward or backward input is detected. [DEFAULT: 0]");
        private readonly static GUIContent brakeFlightMinSpeedContent = new GUIContent(" Min. Speed (m/s)", "The effective minimum speed at which the brake flight assist will operate. [DEFAULT -10m/s]");
        private readonly static GUIContent brakeFlightMaxSpeedContent = new GUIContent(" Max. Speed (m/s)", "The effective maximum speed at which the brake flight assist will operate. [DEFAULT +10m/s]");
        private readonly static GUIContent brakeFlightAssistStrengthXContent = new GUIContent(" BFA Strength X", "Increasing this value will increase how quickly brake flight assist slows down left or right movent when no left or right input is detected. [DEFAULT: 0]");
        private readonly static GUIContent brakeFlightAssistStrengthYContent = new GUIContent(" BFA Strength Y", "Increasing this value will increase how quickly brake flight assist slows down up or down movent when no up or down input is detected. [DEFAULT: 0]");
        private readonly static GUIContent limitPitchAndRollHeaderContent = new GUIContent("<b>Limit Pitch/Roll</b>\n\n" +
            "When Limit Pitch/Roll is enabled, the ship is limited to a range of pitches (as specified by the Max Pitch) and roll is " +
            "controlled by yaw input, between a small range of roll angles (as specified by Max Turn Roll). This " +
            "can either be constrained to the world upward direction (as would be used in an arcade airplane game) or constrained to the " +
            "ground surface (as would be used in a hover-racing game) by enabling Stick To Ground Surface.");
        private readonly static GUIContent limitPitchAndRollContent = new GUIContent(" Limit Pitch/Roll", "Enable this to constrain the " +
            "ship to a limited range of pitch and roll.");
        private readonly static GUIContent maxPitchContent = new GUIContent(" Max Pitch", "The maximum pitch in degrees that the ship is " +
            "allowed to attain.");
        private readonly static GUIContent pitchSpeedContent = new GUIContent(" Pitch Speed", "How fast the ship pitches in degrees per second.");
        private readonly static GUIContent maxTurnRollContent = new GUIContent(" Max Turn Roll", "The maximum roll in degrees that the " +
            "ship is allowed to attain.");
        private readonly static GUIContent turnRollSpeedContent = new GUIContent(" Turn Roll Speed", "How fast the ship rolls in degrees per second.");
        private readonly static GUIContent rollControlModeContent = new GUIContent(" Roll Control Mode", "How roll is controlled. When " +
            "Yaw Input is selected, roll is determined by the yaw input (turning left will make the ship roll left and vice versa). " +
            "When Strafe Input is selected, roll is determined by the strafe input (strafing left will make the ship roll left and " +
            "vice versa).");
        private readonly static GUIContent pitchRollMatchResponsivenessContent = new GUIContent(" Responsiveness", "How fast the ship " +
            "pitches/rolls to match the target pitch/roll (for instance the ground surface if Stick To Ground Surface is enabled).");
        private readonly static GUIContent stickToGroundSurfaceHeaderContent = new GUIContent("<b>Stick To Ground Surface</b>\n\n" +
            "When Stick To Ground Surface is enabled, the ship will orient itself and maintain a relatively constant distance to the " +
            "ground below it, as specified by the Target Distance.");
        private readonly static GUIContent avoidGroundSurfaceContent = new GUIContent(" Avoid Ground Surface", "Enable this to help the ship to avoid crashing into the ground below it");
        private readonly static GUIContent stickToGroundSurfaceContent = new GUIContent(" Stick To Ground Surface", "Enable this to allow " +
            "the ship to orient itself and maintain a relatively contanst distance to the ground below it.");
        private readonly static GUIContent useGroundMatchSmoothingContent = new GUIContent(" Ground Match Smoothing", "Whether the " +
            "movement used for matching the Target Distance from the ground is smoothed. Enable this to prevent the ship " +
            "from accelerating too quickly when near the Target Distance.");
        private readonly static GUIContent useGroundMatchLookAheadContent = new GUIContent(" Look Ahead", "Whether the ground " +
            "match algorithm 'looks ahead' to detect obstacles ahead of the ship.");
        private readonly static GUIContent orientUpInAirContent = new GUIContent(" Orient Upwards In Air", "Enable this to revert to the " +
            "default behaviour of Limit Pitch/Roll (constraining the ship to a limited range of pitch and roll, facing upwards) when " +
            "there isn't a detectable ground surface below the ship (i.e. when the ship is high up in the air).");
        private readonly static GUIContent targetGroundDistanceContent = new GUIContent(" Target Distance", "The distance the ship will " +
            "attempt to maintain to the ground surface below it.");
        private readonly static GUIContent targetGroundDistanceAboveContent = new GUIContent(" Target Above Distance", "The distance the ship will " +
            "attempt to maintain above the ground surface below it.");
        private readonly static GUIContent minGroundDistanceContent = new GUIContent(" Min Distance", "The min distance the ship will " +
            "attempt to maintain to the ground surface below it.");
        private readonly static GUIContent maxGroundDistanceContent = new GUIContent(" Max Distance", "The max distance the ship will " +
            "attempt to maintain to the ground surface below it.");
        private readonly static GUIContent maxGroundCheckDistanceContent = new GUIContent(" Max Check Distance", "The max distance the " +
            "ship will check below it to find the ground surface. When the ship is further than this distance away from the ground " +
            "surface it will either revert to the default behaviour of Limit Pitch/Roll (if Orient Up In Air is enabled) or it will " +
            "revert to standard six-degrees-of-freedom behaviour (if Orient Up In Air is disabled).");
        private readonly static GUIContent groundMatchResponsivenessContent = new GUIContent(" Responsiveness", "How responsive the " +
            "ship is to sudden changes in the distance to the ground. Increasing this value will allow the ship to match the Target " +
            "Distance more closely but may lead to a juddering effect if increased too much.");
        private readonly static GUIContent groundMatchDampingContent = new GUIContent(" Damping", "How much the up/down motion of " +
            "the ship is damped when attempting to match the Target Distance. Increasing this value will reduce overshoot but " +
            "may make the movement too rigid if increased too much.");
        private readonly static GUIContent maxGroundMatchAccelerationFactorContent = new GUIContent(" Max Acceleration", "The limit to " +
            "how quickly the ship can accelerate to maintain the Target Distance to the ground. Increasing this value will allow the " +
            "ship to match the Target Distance more closely but may look less natural.");
        private readonly static GUIContent centralMaxGroundMatchAccelerationFactorContent = new GUIContent(" Central Max Acceleration", "The " +
            "limit to how quickly the ship can accelerate to maintain the Target Distance to the ground (when at the Target Distance). " +
            "Increasing this value will allow the ship to match the Target Distance more closely but may look less natural.");
        private readonly static GUIContent groundNormalCalcModeContent = new GUIContent(" Ground Normal Calc.", "How the normal " +
            "direction (orientation) is determined. When Single Normal is selected, the single normal of each face of the ground " +
            "geometry is used. When Smoothed Normal is selected, the normals on each vertex of the face of the ground geometry are " +
            "blended together to give a smoothed normal, which is used instead. Smoothed Normal is more computationally expensive.");
        private readonly static GUIContent groundNormalHistoryLengthContent = new GUIContent(" Ground Normal History", "The number " +
            "of past frames (including this frame) used to average ground normals over time. Increase this value to make pitch and roll " +
            "movement smoother. Decrease this value to make pitch and roll movement more responsive to changes in the ground " + 
            "surface.");
        private readonly static GUIContent groundLayerMaskContent = new GUIContent(" Ground Layer Mask", "Only geometry in the specified " +
            "layers will be detected as being part of the ground surface.");
        private readonly static GUIContent inputControlHeaderContent = new GUIContent("<b>Input Control (2.5D)</b>\n\n" +
            "Limit an input axis to achieve 2.5D flight behaviour.");
        private readonly static GUIContent inputControlAxisContent = new GUIContent(" Input Control Axis", "The input axis to control or " + 
            "limit when simulating 2.5D flight.");
        private readonly static GUIContent inputControlLimitContent = new GUIContent(" Input Control Limit", "The target value of the x or y axis");
        private readonly static GUIContent inputControlMovingRigidnessContent = new GUIContent(" Input Moving Rigidness", "The rate at which force is applied to limit control");
        private readonly static GUIContent inputControlTurningRigidnessContent = new GUIContent(" Input Turning Rigidness", "The rate at which the ship turns to limit or correct the rotation");
        private readonly static GUIContent inputControlForwardAngleContent = new GUIContent(" Input Forward Angle", "Forward X angle for the plane the ship will fly in");
        private readonly static GUIContent physicsBasedMomentPowerHeaderContent = new GUIContent("<b>Pitch/Roll/Yaw Power</b>\n\n" +
            "Use Pitch/Roll/Yaw power to scale how much of the ship's thrust capabilities are allocated for turning on each axis. " +
            "NOTE: This behaviour will likely change in future versions.");
        private readonly static GUIContent rollPowerContent = new GUIContent(" Roll Power", "How much power of the ship's roll thrusters " +
            "is used to execute roll manoeuvres. Increasing this value will increase the roll speed and responsiveness of the ship.");
        private readonly static GUIContent pitchPowerContent = new GUIContent(" Pitch Power", "How much power of the ship's pitch thrusters " +
            "is used to execute pitching manoeuvres. Increasing this value will increase the pitch speed and responsiveness of the ship.");
        private readonly static GUIContent yawPowerContent = new GUIContent(" Yaw Power", "How much power of the ship's yaw thrusters " +
            "is used to execute yaw manoeuvres. Increasing this value will increase the yaw speed and responsiveness of the ship.");
        private readonly static GUIContent steeringThrusterPriorityLevelContent = new GUIContent(" Steering Thruster Priority", "How much steering inputs " +
            "are prioritised over lateral inputs for thrusters. A value of 0 means no prioritisation takes place, while a value of 1 will almost completely deactivate " +
            "relevant lateral thrusters whenever any opposing steering input at all is applied.");

        #endregion

        #region GUIContent Thrusters

        private readonly static GUIContent thrustersPhysicsBasedHeaderContent = new GUIContent("<b>Thrusters (Physics-Based)</b>\n\n" +
            "Thrusters are used to control the ship by adding forces to move the ship's position and torques to rotate the ship. " +
            "By specifying up to one force and up to two moments (primary and secondary) you can control what inputs activate each " +
            "thruster (you can auto-populate these by clicking the Auto-Populate Forces and Moments button). You can also specify an " +
            "effects gameObject which will have audio source and/or any particle effects etc. attached. They will be kept in sync with the actions of the " +
            "thruster.");
        private readonly static GUIContent thrustersArcadeHeaderContent = new GUIContent("<b>Thrusters (Arcade)</b>\n\n" +
            "Thrusters are used to control the ship by adding forces to move the ship's position. By specifying a force you can control " +
            "what inputs activate each thruster  (you can auto-populate this by clicking the Auto-Populate Forces and Moments button). " +
            "You can also specify an effects gameObject which will have audio source and/or any particle effects etc. attached. They will be kept in sync " +
            "with the actions of the thruster.");

        private readonly static GUIContent thrustersCentreOfThrustContent = new GUIContent(" Centre of Thrust", "Enable or disable centre of thrust and thrust direction gizmos in the scene view");
        private readonly static GUIContent thrusterSystemsContent = new GUIContent("Thruster Systems", "Power and Fuel Systems");
        private readonly static GUIContent thrusterSystemsStartedContent = new GUIContent("Thrusters Started", "Are the thruster systems online or in the process of coming online?");
        private readonly static GUIContent thrusterSystemStartupDurationContent = new GUIContent("Startup Duration", "The time, in seconds, it takes for the thrusters to fully come online");
        private readonly static GUIContent thrusterSystemShutdownDurationContent = new GUIContent("Shutdown Duration", "The time, in seconds, it takes for the thrusters to fully shutdown");
        private readonly static GUIContent useCentralFuelContent = new GUIContent("Central Fuel", "For thrusters, use a central fuel level, rather than fuel level per thruster.");
        private readonly static GUIContent centralFuelLevelContent = new GUIContent("Central Fuel Level", "The amount of fuel available to the whole ship - range 0.0 (empty) to 100.0 (full).");
        private readonly static GUIContent isThrusterFXStationaryContent = new GUIContent("FX when Stationary", "When the ship is enabled, but ship movement is disabled, should thrusters fire if they have Min FX Always On enabled?");

        private readonly static GUIContent thrusterCopyListContent = new GUIContent("C", "Copy list of thrusters into a copy buffer");
        private readonly static GUIContent thrusterPasteListContent = new GUIContent("P", "Overwrite (replace) all thrusters with those in the copy buffer. Effects Objects will need to be manually configured.");

        private readonly static GUIContent thrusterNameContent = new GUIContent("Name", "The name of the thruster.");
        private readonly static GUIContent thrusterForceUseContent = new GUIContent("Thrust Input", "Which input activates this thruster. " +
            "This determines what input/s are linked with this thruster (if you don't know how to use this, " +
            "the Auto-Populate Forces and Moments button can be used to calculate this automatically).");
        private readonly static GUIContent thrusterPrimaryMomentUseContent = new GUIContent("Primary Moment Input", "The rotational input " +
            "which is primarily used to activate this thruster. This determines what input/s are linked with this thruster (if " +
            "you don't know how to use this, the Auto-Populate Forces and Moments button can be used to calculate this automatically).");
        private readonly static GUIContent thrusterSecondaryMomentUseContent = new GUIContent("Secondary Moment Input", "The rotational input " +
            "which is secondarily used to activate this thruster. This determines what input/s are linked with this " +
            "thruster (if you don't know how to use this, the Auto-Populate Forces and Moments button can be used to calculate this " +
            "automatically).");
        private readonly static GUIContent thrusterMaxThrustContent = new GUIContent("Max Thrust (kN)", "The max thrust in kilonewtons " +
            "this thruster can generate.");
        private readonly static GUIContent thrusterThrottleContent = new GUIContent("Throttle", "The amount of available power being supplied to the thruster");
        private readonly static GUIContent thrusterRelativePositionContent = new GUIContent("Relative Position", "The position of the " +
            "thruster in local space.");
        private readonly static GUIContent thrusterThrustDirectionContent = new GUIContent("Thrust Direction", "The direction of thrust " +
            "of the thruster in local space.");
        private readonly static GUIContent thrusterEffectsObjectContent = new GUIContent("Effects Object", "The object which has the " +
            "effects that should be enabled when the thruster is used (e.g. sounds, particle effects, etc.) attached. Set the Volume of the " +
            "AudioSource to represent the maximum volume at full thrust.");
        private readonly static GUIContent thrustersAutoPopulateForcesMomentsButtonContent = new GUIContent("Auto-Populate Forces and Moments",
            "Automatically calculates the force, primary moment and secondary moment for each thruster.");
        private readonly static GUIContent thrusterMinPerformanceContent = new GUIContent("Min Performance", "The minimum possible " +
            "performance level of this thruster (i.e. what performance level the thruster will have when its health reaches zero). The " +
            "performance level affects how much thrust this thruster generates.");
        private readonly static GUIContent thrustersMinEffectsRateContent = new GUIContent("Minimum Effects Rate" , "The 0.0 to 1.0 value that " +
            "indicates the minimum normalised amount of any particle effects or audio sources that are applied when a non-zero thrust input is received for this thruster. " +
            "The default is 0 which will apply a linear particle emission rate or audio volume based on the amount of thrust input received. If the full particle emission " +
            "rate or audio volume should be applied when any input is received, set the value to 1.0.");

        private readonly static GUIContent isThrottleMinEffectsContent = new GUIContent("Throttle Min FX", "Does the amount of throttle available affect the Minimum Effects Rate?");
        private readonly static GUIContent isMinEffectsAlwaysOnContent = new GUIContent("Min FX Always On", "When Minimum Effects Rate > 0 and Throttle > 0 the effects fire when thruster input is 0. The Limit FX On Y and Z settings are still honoured when this is true.");
        private readonly static GUIContent thrusterLimitEffectsOnYContent = new GUIContent("Limit FX on Y Axis", "Limit when the effects are used for this thruster based on the speed of the ship along the local Y axis (up or down)");
        private readonly static GUIContent thrusterMinEffectsOnYContent = new GUIContent("Min. FX on Y Axis", "The minimum speed in m/s on the local y-axis the ship must be travelling at before the effects will activate");
        private readonly static GUIContent thrusterMaxEffectsOnYContent = new GUIContent("Max. FX on Y Axis", "The maximum speed in m/s on the local y-axis the ship can be travelling for the effects to be active.");
        private readonly static GUIContent thrusterLimitEffectsOnZContent = new GUIContent("Limit FX on Z Axis", "Limit when the effects are used for this thruster based on the speed of the ship along the local Z axis (forward or backward)");
        private readonly static GUIContent thrusterMinEffectsOnZContent = new GUIContent("Min. FX on Z Axis", "The minimum speed in m/s on the local z-axis the ship must be travelling at before the effects will activate");
        private readonly static GUIContent thrusterMaxEffectsOnZContent = new GUIContent("Max. FX on Z Axis", "The maximum speed in m/s on the local z-axis the ship can be travelling for the effects to be active.");
        private readonly static GUIContent thrusterRampUpDurationContent = new GUIContent("Throttle Up Time", "The length of time, in seconds, it takes the thruster to go from no thrust to maximum thrust. [Default = 0 or instant thrust increase]");
        private readonly static GUIContent thrusterRampDownDurationContent = new GUIContent("Throttle Down Time", "The length of time, in seconds, it takes the thruster to go from maximum thrust to no thrust. [Default = 0 or instant thrust decrease]");
        private readonly static GUIContent thrusterAlignBtnContent = new GUIContent("Align", "Automatically align the effect with the thruster relative position.");
        private readonly static GUIContent thrusterFuelLevelContent = new GUIContent("Fuel Level", "The amount of fuel available - range 0.0 (empty) to 100.0 (full)");
        private readonly static GUIContent thrusterFuelBurnRateContent = new GUIContent("Fuel Burn Rate", "The rate fuel is consumed per second. If rate is 0, fuel is unlimited");
        private readonly static GUIContent thrusterHeatLevelContent = new GUIContent("Heat Level", "The heat of the thruster or engine - range 0.0 (starting temp) to 100.0 (max temp). At 100, the thruster will produce no thrust.");
        private readonly static GUIContent thrusterHeatUpRateContent = new GUIContent("Heat Up Rate", "The rate heat is added per second. If rate is 0, the heat level never changes.");
        private readonly static GUIContent thrusterHeatDownRateContent = new GUIContent("Cool Down Rate", "The rate heat is removed per second. This is the rate the thruster cools when not in use. Has no effect if Heat Up Rate is 0.");
        private readonly static GUIContent thrusterOverHeatThresholdContent = new GUIContent("Overheat Threshold", "The heat level that the thruster will begin to overheat and start producing less thrust.");
        private readonly static GUIContent thrusterIsBurnoutOnMaxHeatContent = new GUIContent("Burnout on Max Heat", "When the thruster reaches max heat level of 100, will the thruster be inoperable until it is repaired?");


        #endregion

        #region GUIContent Aerodynamics

        private readonly static GUIContent atmosphericPropertiesHeaderContent = new GUIContent("<b>Medium Density</b>\n\n" +
            "The medium density property defines the density of the medium (in kilograms per cubic metre) the ship is travelling through " +
            "(generally in air). In less dense atmospheres this value should be lower and in more dense atmospheres this value should be " +
            "higher. At low altitudes in Earth's atmosphere the value is approximately 1.293 while in space (where there is virtually no " +
            "air) it should be set to zero to achieve realism. The medium density affects all aerodynamics (i.e. drag, lift, etc.).");
        private readonly static GUIContent mediumDensityContent = new GUIContent("Medium Density", "The density of the medium the " +
            "ship is travelling through.");
        private readonly static GUIContent dragPropertiesHeaderContent = new GUIContent("<b>Drag Properties</b>\n\n" +
            "The drag properties of the ship determine how the airflow around the ship affects the movement of the ship. After making any " +
            "mesh changes, click the Calculate Drag Properties to recalculate the internally stored drag properties of the ship. Then " +
            "use the Drag X/Y/Z Coefficients to alter how much drag the ship has on each axis. More streamlined axes of the ship should " +
            "have a lower drag coefficient while flatter axes should have a higher drag coefficient. In Arcade mode, you can also use " +
            "the Angular Drag Factor to alter how quickly angular drag will slow down any spinning motion and Disable Drag Moments to " +
            "prevent the ship from rotating due to moments caused by drag.");
        private readonly static GUIContent calculateDragPropertiesButtonContent = new GUIContent("Calculate Drag Properties", 
            "Calculates the drag properties (drag areas and centres of drag) of the ship. NOTE: This will re-enable all colliders. " +
            "You may want to adjust this after the calculation has been done");
        private readonly static GUIContent dragCoefficientXContent = new GUIContent("Drag X Coefficient", "The coefficient of drag " +
            "of the ship on the x-axis. Increasing the coefficient of the drag will increase the effect of drag.");
        private readonly static GUIContent dragCoefficientYContent = new GUIContent("Drag Y Coefficient", "The coefficient of drag " +
            "of the ship on the y-axis. Increasing the coefficient of the drag will increase the effect of drag.");
        private readonly static GUIContent dragCoefficientZContent = new GUIContent("Drag Z Coefficient", "The coefficient of drag " +
            "of the ship on the z-axis. Increasing the coefficient of the drag will increase the effect of drag.");
        private readonly static GUIContent angularDragFactorContent = new GUIContent("Angular Drag Factor", "How strong the effect of " +
            "angular drag is on the ship. Setting this to 1 will make it physically realistic.");
        private readonly static GUIContent disableDragMomentsContent = new GUIContent("Disable Drag Moments", "This will prevent drag " +
            "causing the ship to rotate.");

        private readonly static GUIContent dragMomentMultipliersContent = new GUIContent("Drag Moment Multipliers", "Multipliers for drag moments causing rotation along a local axis");
        private readonly static GUIContent dragXMomentMultiplierContent = new GUIContent(" Drag X Multiplier", "A multiplier for drag " + 
             "moments causing rotation along the local (pitch) x-axis. Decreasing this will make these moments weaker.");
        private readonly static GUIContent dragYMomentMultiplierContent = new GUIContent(" Drag Y Multiplier", "A multiplier for drag " +
             "moments causing rotation along the local (yaw) y-axis. Decreasing this will make these moments weaker.");
        private readonly static GUIContent dragZMomentMultiplierContent = new GUIContent(" Drag Z Multiplier", "A multiplier for drag " +
             "moments causing rotation along the local (roll) z-axis. Decreasing this will make these moments weaker.");

        private readonly static GUIContent aeroCentreOfLiftDirectionContent = new GUIContent("Centre of Lift/Direction","Enable or disable centre of lift and direction gizmos in the scene view. This will only be visible if you have wings.");

        private readonly static GUIContent wingsHeaderContent = new GUIContent("<b>Wings</b>\n\n" +
            "Wings allow you to simulate the effect of lift as air flows past a surface. Wings require the ship to be moving relative to " +
            "air around it in order to operate.");
        private readonly static GUIContent wingStallEffectContent = new GUIContent("Wing Stall Effect", "How much the effect of stalling " +
            "affects ship flight. Setting this to zero will make the effect of stalling very minimal.");
        private readonly static GUIContent wingNameContent = new GUIContent("Name", "Name of the wing. E.g. Front Wing, Tail Wing, Left Front Wing");
        private readonly static GUIContent angleOfAttackContent = new GUIContent("Angle Of Attack", "The angle of attack (inclination " +
            "above the x-z plane) of the wing in degrees.");
        private readonly static GUIContent wingSpanContent = new GUIContent("Length", "The span (length on the z-axis) of the wing " +
            "in metres.");
        private readonly static GUIContent wingChordContent = new GUIContent("Width", "The chord (width on the x-axis) of the wing " +
            "in metres.");
        private readonly static GUIContent wingRelativePositionContent = new GUIContent("Relative Position", "The position of the wing " +
            "in local space.");
        private readonly static GUIContent liftDirectionContent = new GUIContent("Lift Direction", "The direction of the lift force " +
            "of the wing in local space.");
        private readonly static GUIContent wingMinPerformanceContent = new GUIContent("Min Performance", "The minimum possible " +
            "performance level of this wing (i.e. what performance level the wing will have when its health reaches zero). The " +
            "performance level affects how much lift the wing generates.");

        private readonly static GUIContent controlSurfacesHeaderContent = new GUIContent("<b>Control Surfaces</b>\n\n" +
            "Control surfaces allow you to simulate the effect of moving parts changing the lift and drag properties of the ship " +
            "in order to control the movement of the ship. Control surfaces require the ship to be moving relative to air around it " +
            "in order to operate.");
        private readonly static GUIContent controlSurfaceTypeContent = new GUIContent("Type", "The type of control surface this is. " +
            "The type determines how the control surface moves and what inputs control it. Ailerons control the roll of the ship, and " +
            "are generally required to be placed symmetrically on opposite sides of the ship. Elevators control the pitch of the ship, " +
            "and should generally be placed behind the centre of mass of the ship. Rudders control the yaw of the ship and should " +
            "generally be placed in the middle of the ship (not to the left or to the right). Air brakes are used to slow the ship down.");
        private readonly static GUIContent controlSurfaceSpanContent = new GUIContent("Length", "The span of the control surface in " +
            "metres.");
        private readonly static GUIContent controlSurfaceChordContent = new GUIContent("Width", "The chord of the control surface in " +
            "metres.");
        private readonly static GUIContent controlSurfaceRelativePositionContent = new GUIContent("Relative Position", "The position " +
            "of the control surface in local space.");
        private readonly static GUIContent controlSurfaceMinPerformanceContent = new GUIContent("Min Performance", "The minimum possible " +
            "performance level of this control surface (i.e. what performance level the control surface will have when its health reaches zero). The " +
            "performance level affects how effective the control surface is.");
        //private readonly static GUIContent controlSurfaceRotationAxisContent = new GUIContent("Rotation Axis", "todo");

        private readonly static GUIContent arcadeUseBrakeComponentContent = new GUIContent("Use Brake Component", "Whether a brake " +
            "component is used.");
        private readonly static GUIContent arcadeBrakeStrengthContent = new GUIContent("Brake Strength", "The strength of the braking" +
            " force.");
        private readonly static GUIContent arcadeBrakeIgnoreMediumDensityContent = new GUIContent("Ignore Medium Density", "Whether " +
            "the strength of the brake force ignores the density of the medium the ship is in (assuming it to be a constant value " +
            "of one kilogram per cubic metre).");
        private readonly static GUIContent arcadeBrakeMinAccelerationContent = new GUIContent("Min Acceleration", "The minimum " +
            "braking acceleration (in metres per second) caused by the brake when the brake is fully engaged. Increase this " +
            "value to make the ship come to a stop more quickly at low speeds.");

        #endregion

        #region GUIContent Combat

        private readonly static GUIContent damageHeaderContent = new GUIContent("<b>Damage</b>\n\nDamage settings control under what " +
            "circumstances the ship is destroyed, and how the ship is affected up until that point.");

        private readonly static GUIContent shipDamageModelContent = new GUIContent("Damage Model", "Determines how damage is " +
            "calculated and applied to the ship. In Simple mode, the ship has a single health value. The only effects of damage are " +
            "visual until the health value reaches zero, at which point the ship is destroyed and optionally respawns. In " +
            "Progressive mode, as the ship takes damage the performance of parts is affected. In localised mode different parts can " +
            "be damaged independently of each other.");
        private readonly static GUIContent damageStartHealthContent = new GUIContent(" Starting Health", "How much 'health' the " +
            "ship has initially.");
        private readonly static GUIContent damageRegionInvincibleContent = new GUIContent(" Is Invincible", "When invincible, it will not take damage however its health can still be manually decreased.");
        private readonly static GUIContent collisionDamageResistanceContent = new GUIContent(" Col. Damage Resistance", "Value " +
            "indicating the resistance of the ship to damage caused by collisions. Increasing this value will decrease the amount of " +
            "damage caused to the ship by collisions.");
        private readonly static GUIContent useShieldingContent = new GUIContent(" Use Shielding", "Whether the main damage region uses " +
            "shielding. Up until a point, shielding protects the ship from damage (which can affect the performance of parts on the ship).");
        private readonly static GUIContent shieldingDamageThresholdContent = new GUIContent(" Damage Threshold", "Damage below this value " +
            "will not affect the shield or the ship's health while the shield is still active (i.e. until the shield has absorbed 'amount' " +
            "damage from damage events above the damage threshold).");
        private readonly static GUIContent shieldingAmountContent = new GUIContent(" Amount", "How much damage the shield can absorb before " +
            "it ceases to protect the ship from damage.");
        private readonly static GUIContent shieldingRechargeRateContent = new GUIContent(" Recharge Rate", "The rate per second that a shield will recharge (default = 0)");
        private readonly static GUIContent shieldingRechargeDelayContent = new GUIContent(" Recharge Delay", "The delay, in seconds, between when damage occurs to a shield and it begins to recharge.");

        private readonly static GUIContent shipDestroyEffectsObjectContent = new GUIContent(" Effects Object", "The particle and/or sound effect " +
            "prefab that will be instantiated when the ship is destroyed.");

        private readonly static GUIContent shipDestroyDestructObjectContent = new GUIContent(" Destruct Object", "The destruct prefab that breaks into fragments when the ship is destroyed.");
        private readonly static GUIContent damageRegionDestructObjectContent = new GUIContent(" Destruct Object", "The destruct prefab that breaks into fragments when the damage region's health reaches 0.");
        private readonly static GUIContent damageRegionChildTransformContent = new GUIContent(" Child Transform", "The child tranform of the ship that contains the mesh(es) for this local region. If set, it is disabled when the region's health reaches 0.");

        private readonly static GUIContent damageRegionEffectsObjectContent = new GUIContent(" Effects Object", "The particle and/or sound effect " +
            "prefab that will be instantiated when the damage region's health reaches 0.");

        private readonly static GUIContent damageRegionIsMoveEffectsObjectContent = new GUIContent(" Effect Follows Ship", "The particle and/or sound effect " +
            "will follow the damaged ship as it moves");

        private readonly static GUIContent damageRegionIsRadarEnabledContent = new GUIContent(" Visible to Radar", "Is this damage region visible to the radar system? It is only visible if the ship is also visible to radar.");

        private readonly static GUIContent useDamageMultipliersContent = new GUIContent(" Use Damage Multipliers", "Whether " +
            "damage type multipliers are used when calculating damage from projectiles.");
        private readonly static GUIContent useLocalisedDamageMultipliersContent = new GUIContent(" Local Multipliers", "Whether " +
            "damage type multipliers are localised (i.e. there are different sets of damage multipliers for each damage region of the ship).");
        private readonly static GUIContent typeADamageMultiplierContent = new GUIContent(" Damage Type A", "The relative amount of damage a Type A projectile will inflict on the ship.");
        private readonly static GUIContent typeBDamageMultiplierContent = new GUIContent(" Damage Type B", "The relative amount of damage a Type B projectile will inflict on the ship.");
        private readonly static GUIContent typeCDamageMultiplierContent = new GUIContent(" Damage Type C", "The relative amount of damage a Type C projectile will inflict on the ship.");
        private readonly static GUIContent typeDDamageMultiplierContent = new GUIContent(" Damage Type D", "The relative amount of damage a Type D projectile will inflict on the ship.");
        private readonly static GUIContent typeEDamageMultiplierContent = new GUIContent(" Damage Type E", "The relative amount of damage a Type E projectile will inflict on the ship.");
        private readonly static GUIContent typeFDamageMultiplierContent = new GUIContent(" Damage Type F", "The relative amount of damage a Type F projectile will inflict on the ship.");

        private readonly static GUIContent respawningHeaderContent = new GUIContent("<b>Respawning</b>\n\nRespawning settings " +
            "control what happens when the ship is destroyed.");
        private readonly static GUIContent respawningModeContent = new GUIContent("Respawning Mode", "How respawning happens.");
        private readonly static GUIContent respawnTimeContent = new GUIContent("Respawn Time", "How long the respawn process takes " +
            "(in seconds).");
        private readonly static GUIContent customRespawnPositionContent = new GUIContent("Respawn Position", "The position in world " +
            "space that the ship respawns from.");
        private readonly static GUIContent customRespawnRotationContent = new GUIContent("Respawn Rotation", "The rotation in world " +
            "space that the ship respawns with.");
        private readonly static GUIContent respawnVelocityContent = new GUIContent("Respawn Velocity", "The velocity in local " +
            "space that the ship respawns with.");
        private readonly static GUIContent respawningPathContent = new GUIContent("Respawn Path", "The Path from SSCManager in the scene. When the ship is respawned, it will respawn onto the closest point on the path.");

        private readonly static GUIContent collisionRespawnPositionDelayContent = new GUIContent("Col. Respawn Delay", "The time " +
            "(in seconds) between updates of the collision respawn position. Hence when the ship is destroyed by colliding with " +
            "something, the ship respawn position will be where the ship was between this time ago and twice this time ago. Only " +
            "relevant when respawningMode is set to RespawnAtLastPosition.");

        private readonly static GUIContent respawningStuckTimeContent = new GUIContent("Stuck Time", "The amount of time that needs to elapse before a stationary ship is considered stuck. When the value is 0, a stationary ship is never considered stuck.");
        private readonly static GUIContent respawningStuckTimeNeverContent = new GUIContent("Stuck Time (Never)", "The amount of time that needs to elapse before a stationary ship is considered stuck. When the value is 0, a stationary ship is never considered stuck.");
        private readonly static GUIContent respawningStuckSpeedThresholdContent = new GUIContent("Stuck Speed Threshold", "The maximum speed in metres per second the ship can be moving before it can be considered stuck. Default: 0.1 m/s");
        private readonly static GUIContent respawningStuckActionContent = new GUIContent("Stuck Action", "The action to take when the ship is deemed stationary or stuck.");
        private readonly static GUIContent respawningStuckCallbackContent = new GUIContent("callbackOnStuck", "Name of your custom method that is called when a ship gets stuck. This needs to be assigned in your code at runtime.");
        private readonly static GUIContent respawningStuckPathContent = new GUIContent("Stuck Respawn Path", "The Path from SSCManager in the scene. When the ship is stuck, it will respawn onto the closest point on the path.");

        private readonly static GUIContent applyControllerRumbleContent = new GUIContent(" Controller Rumble", "Whether controller rumble is applied to the ship by the ship control module.");
        private readonly static GUIContent minRumbleDamageContent = new GUIContent(" Min Rumble Damage", "The minimum amount of damage that will cause controller rumble.");
        private readonly static GUIContent maxRumbleDamageContent = new GUIContent(" Max Rumble Damage", "The amount of damage corresponding to maximum controller rumble.");
        private readonly static GUIContent damageRumbleTimeContent = new GUIContent(" Damage Rumble Time", "The time (in seconds) that a controller rumble event lasts for.");

        private readonly static GUIContent weaponsHeaderContent = new GUIContent("<b>Weapons</b>\n\n" + 
            "Weapons are used to allow the ship to fire projectiles. By specifying a firing button you can control which input " +
            "is used to activate each weapon. You can also specify the fire rate, direction and projectile for each weapon.");

        private readonly static GUIContent weaponUseWhenMovementDisabledContent = new GUIContent("Use When Movement Disabled", "When the ship is enabled, but movement is disabled, weapons and damage are updated");
        private readonly static GUIContent weaponNameContent = new GUIContent("Name", "The name of the weapon. E.g. Laser Cannon, Guided Missile Launcher, Rapid Fire Gun.");
        private readonly static GUIContent weaponTypeContent = new GUIContent("Type", "The type or style of weapon.");
        private readonly static GUIContent weaponRelativePositionContent = new GUIContent("Relative Position", "The position of the weapon " +
            "in local space relative to the pivot point of the ship.");
        private readonly static GUIContent weaponIsMultipleFirePositionsContent = new GUIContent("Multiple Fire Positions", "If this weapon has multiple cannons or barrels");
        private readonly static GUIContent weaponFirePositionContent = new GUIContent("Fire Position Offsets", "The positions of the cannon or barrel relative to the position of the weapon.");
        private readonly static GUIContent weaponFireDirectionContent = new GUIContent("Fire Direction", "The direction in which " +
            "the weapon fires projectiles in local ship space. +ve Z is fire forwards, -ve Z is fire backwards.");
        private readonly static GUIContent weaponProjectilePrefabContent = new GUIContent("Projectile Prefab", "Prefab template of the " +
            "projectiles fired by this weapon. Projectile prefabs need to have a Projectile Module script attached to them.");
        private readonly static GUIContent weaponBeamPrefabContent = new GUIContent("Beam Prefab", "Prefab template of the " +
            "beam fired by this weapon. Beam prefabs need to have a Beam Module script attached to them.");
        private readonly static GUIContent weaponReloadTimeContent = new GUIContent("Reload Time", "The minimum time (in seconds) between consecutive firings of the weapon.");
        private readonly static GUIContent weaponPowerUpTimeContent = new GUIContent("Power-up Time", "The minimum time (in seconds) between consecutive firings of the beam weapon.");
        private readonly static GUIContent weaponMaxRangeContent = new GUIContent("Max Range", "The maximum distance (in metres) the beam weapon can fire.");
        private readonly static GUIContent weaponRechargeTimeContent = new GUIContent("Recharge Time", "The time (in seconds) it takes the fully discharged weapon to reach maximum charge");
        private readonly static GUIContent weaponFiringButtonContent = new GUIContent("Firing Button", "The firing button or mechanism to use for this weapon. Auto-fire only works with Turrets when a target is selected.");
        private readonly static GUIContent weaponUnlimitedAmmoContent = new GUIContent("Unlimited Ammo", "Can this projectile weapon keep firing and never run out of ammunition?");
        private readonly static GUIContent weaponAmmunitionContent = new GUIContent("Ammunition", "The quantity of projectiles or ammunition available for this weapon.");
        private readonly static GUIContent weaponChargeAmountContent = new GUIContent("Charge Amount", "The amount of charge currently available for this weapon.");
        private readonly static GUIContent weaponIsAutoTargetingEnabledContent = new GUIContent("Auto Targeting", "When the Auto Targeting Module is attached, use this to indicate targets should be assigned to the weapon.");
        private readonly static GUIContent weaponTurretPivotYContent = new GUIContent("Turret Pivot Y", "The transform of the pivot point around which the turret turns on the local y-axis");
        private readonly static GUIContent weaponTurretPivotXContent = new GUIContent("Turret Pivot X", "The transform on which the barrel(s) or cannon(s) elevate up or down on the local x-axis");
        private readonly static GUIContent weaponTurretMinYContent = new GUIContent("Turret Min. Y", "The minimum angle on the local y-axis the turret can rotate to");
        private readonly static GUIContent weaponTurretMaxYContent = new GUIContent("Turret Max. Y", "The maximum angle on the local y-axis the turret can rotate to");
        private readonly static GUIContent weaponTurretMinXContent = new GUIContent("Turret Min. X", "The minimum angle on the local x-axis the turret can elevate to");
        private readonly static GUIContent weaponTurretMaxXContent = new GUIContent("Turret Max. X", "The maximum angle on the local x-axis the turret can elevate to");
        private readonly static GUIContent weaponTurretMoveSpeedContent = new GUIContent("Turret Move Speed","The rate at which the turret can rotate");
        private readonly static GUIContent weaponCheckLineOfSightContent = new GUIContent("Check Line Of Sight", "Whether the weapon checks line of sight before firing (in order to prevent friendly fire) each frame. " +
            "Since this uses raycasts it can lead to reduced performance.");
        private readonly static GUIContent weaponInaccuracyContent = new GUIContent("Turret Inaccuracy", "When inaccuracy is greater than 0, the turret may not aim at the optimum target position.");
        private readonly static GUIContent weaponTurretReturnToParkIntervalContent = new GUIContent("Turret Park Interval", "When greater than 0, the number of seconds a turret will wait, after losing a target, to begin returning to the original orientation.");

        private readonly static GUIContent weaponHeatLevelContent = new GUIContent("Heat Level", "The heat of the weapon - range 0.0 (starting temp) to 100.0 (max temp).");
        private readonly static GUIContent weaponHeatUpRateContent = new GUIContent("Heat Up Rate", "The rate heat is added per second for beam weapons. For projectile weapons, it is inversely proportional to the firing interval (reload time). If rate is 0, heat level never changes.");
        private readonly static GUIContent weaponHeatDownRateContent = new GUIContent("Cool Down Rate", "The rate heat is removed per second. This is the rate the weapon cools when not in use.");
        private readonly static GUIContent weaponOverHeatThresholdContent = new GUIContent("Overheat Threshold", "The heat level that the weapon will begin to overheat and start being less efficient.");
        private readonly static GUIContent weaponIsBurnoutOnMaxHeatContent = new GUIContent("Burnout on Max Heat", "When the weapon reaches max heat level of 100, will the weapon be inoperable until it is repaired?");

        private readonly static GUIContent damageRegionNameContent = new GUIContent(" Name", "The name of the damage region.");
        private readonly static GUIContent damageRegionRelativePositionContent = new GUIContent(" Relative Position", "Position of this " +
            "damage region in local space relative to the pivot point of the ship. Together with the size it determines what area the " +
            "damage region encapsulates. This is the area that damage must occur at to impact this damage region.");
        private readonly static GUIContent damageRegionSizeContent = new GUIContent(" Size", "Size of this damage region (in metres " +
            "cubed) in local space. Together with the relative position it determines what area the damage region encapsulates. This " +
            "is the area that damage must occur at to impact this damage region.");
        private readonly static GUIContent damageRegionStartHealthContent = new GUIContent(" Starting Health", "How much 'health' the " +
            "damage region has initially.");
        private readonly static GUIContent damageRegionCollisionDamageResistanceContent = new GUIContent(" Col. Damage Resistance", "Value " +
            "indicating the resistance of the damage region to damage caused by collisions. Increasing this value will decrease the amount of " +
            "damage caused to the damage region by collisions.");
        private readonly static GUIContent damageRegionUseShieldingContent = new GUIContent(" Use Shielding", "Whether this damage region uses " +
            "shielding. Up until a point, shielding protects the ship from damage (which can affect the performance of parts on the ship).");
        private readonly static GUIContent damageRegionShieldingDamageThresholdContent = new GUIContent(" Damage Threshold", "Damage below this value " +
            "will not affect the shield or the damage region's health while the shield is still active (i.e. until the shield has absorbed 'amount' " +
            "damage from damage events above the damage threshold).");
        private readonly static GUIContent damageRegionShieldingAmountContent = new GUIContent(" Amount", "How much damage the shield can absorb before " +
            "it ceases to protect the damage region from damage.");

        private readonly static GUIContent identificationHeaderContent = new GUIContent("<b>Identification</b>\n\n" + 
            "Configurable here or via code, identification can group ships into combat groups to help you identify them during gameplay.");
        private readonly static GUIContent factionIdContent = new GUIContent("Faction Id", "The faction or alliance the ship belongs to. This can be used to identify if a ship is friend or foe.  Neutral = 0.");
        private readonly static GUIContent squadronIdContent = new GUIContent("Squadron Id", "The (unique) squadron this ship is a member of. Do not place friendly and enemy ships in the same squadron.");
        private readonly static GUIContent isRadarEnabledContent = new GUIContent("Visible to Radar", "Is this ship visible to the radar system?");
        private readonly static GUIContent radarBlipSizeContent = new GUIContent("Radar Blip Size", "The relative size of the blip on the radar mini-map.");

        #endregion

        #region GUIConnent Debugging
        private readonly static GUIContent debugModeContent = new GUIContent("Debug Mode", "Use this to display the data about the ShipControlModule component at runtime in the editor.");
        private readonly static GUIContent debugNotSetContent = new GUIContent("-", "not set");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent("Is Initialised?");
        private readonly static GUIContent debugIsShipEnabledContent = new GUIContent("Is Ship Enabled?");
        private readonly static GUIContent debugIsMovementEnabledContent = new GUIContent("Is Movement Enabled?");
        private readonly static GUIContent debugIsVisbleToRadarEnabledContent = new GUIContent("Is Visible to Radar?");
        private readonly static GUIContent debugIsShipSpeedShownContent = new GUIContent("Show Ship Speed + Velo");
        private readonly static GUIContent debugShipSpeedContent = new GUIContent("Ship Speed km/h", "");
        private readonly static GUIContent debugShipVeloXContent = new GUIContent("Ship Velocity X m/s", "");
        private readonly static GUIContent debugShipVeloYContent = new GUIContent("Ship Velocity Y m/s", "");
        private readonly static GUIContent debugIsShowHealthContent = new GUIContent("Show Health");
        private readonly static GUIContent debugMainRegionContent = new GUIContent(" Main Damage Region");
        private readonly static GUIContent debugLocalRegionsContent = new GUIContent(" Local Damage Regions");
        private readonly static GUIContent debugRegionHealthContent = new GUIContent("  Region Health");
        private readonly static GUIContent debugUseShieldContent = new GUIContent("  Use Shield?");
        private readonly static GUIContent debugRegionShieldContent = new GUIContent("  Region Shield");
        private readonly static GUIContent debugIsShowThrustersContent = new GUIContent("Show Thrusters");
        private readonly static GUIContent debugIsThrusterSystemsStartedContent = new GUIContent(" Systems Status");
        private readonly static GUIContent debugMaxThrustContent = new GUIContent("  Max Thrust (kN)");
        private readonly static GUIContent debugThrustInputContent = new GUIContent("  Current Input");
        private readonly static GUIContent debugIsShowWeaponsContent = new GUIContent("Show Weapons");
        private readonly static GUIContent debugWeaponTypeContent = new GUIContent("   Weapon Type");
        private readonly static GUIContent debugWeaponIsLockedOnContent = new GUIContent("   Is Locked on Target");
        private readonly static GUIContent debugWeaponHasLoSContent = new GUIContent("   Has LoS");
        private readonly static GUIContent debugWeaponIsParkedContent = new GUIContent("   Is Parked");
        private readonly static GUIContent debugWeaponTargetGOContent = new GUIContent("   Target GameObject");
        private readonly static GUIContent debugNoTurretsContent = new GUIContent(" No Turret Weapons");

        private readonly static GUIContent debugIsShowShipInputContent = new GUIContent("Show Input");
        private readonly static GUIContent debugHorizontalContent = new GUIContent("Horizontal");
        private readonly static GUIContent debugVerticalContent = new GUIContent("Vertical");
        private readonly static GUIContent debugLongitudinalContent = new GUIContent("Longitudinal");
        private readonly static GUIContent debugPitchContent = new GUIContent("Pitch");
        private readonly static GUIContent debugYawContent = new GUIContent("Yaw");
        private readonly static GUIContent debugRollContent = new GUIContent("Roll");
        private readonly static GUIContent debugPrimaryFireContent = new GUIContent("Primary Fire");
        private readonly static GUIContent debugSecondaryFireContent = new GUIContent("Secondary Fire");
        private readonly static GUIContent debugDockingContent = new GUIContent("Docking");
        #endregion

        #region Serialized Properties   

        private SerializedProperty shipInstanceProp;

        // Physics
        private SerializedProperty shipPhysicsModelProp;
        private SerializedProperty manualCentreOfMassProp;
        private SerializedProperty showCOMGizmosInSceneViewProp;
        private SerializedProperty centreOfMassProp;

        // Control
        private SerializedProperty limitPitchAndRollProp;
        private SerializedProperty stickToGroundSurfaceProp;
        private SerializedProperty avoidGroundSurfaceProp;
        private SerializedProperty orientUpInAirProp;
        private SerializedProperty useGroundMatchSmoothingProp;
        private SerializedProperty targetGroundDistanceProp;
        private SerializedProperty minGroundDistanceProp;
        private SerializedProperty maxGroundDistanceProp;
        private SerializedProperty maxGroundCheckDistanceProp;
        private SerializedProperty maxGroundMatchAccFactorProp;
        private SerializedProperty centralMaxGroundMatchAccFactorProp;
        private SerializedProperty inputControlAxisProp;
        private SerializedProperty brakeFlightAssistStrengthProp;
        private SerializedProperty brakeFlightAssistMinSpeedProp;
        private SerializedProperty brakeFlightAssistMaxSpeedProp;

        // Thrusters
        private SerializedProperty thrustersProp;
        private SerializedProperty isThrusterSystemsStartedProp;
        private SerializedProperty thrusterSystemStartupDurationProp;
        private SerializedProperty thrusterSystemShutdownDurationProp;
        private SerializedProperty useCentralFuelProp;
        private SerializedProperty centralFuelLevelProp;
        private SerializedProperty thrusterProp;
        private SerializedProperty thrusterEffectObjectsProp;
        private SerializedProperty thrusterEffectObjProp;
        private SerializedProperty thrusterShowInEditorProp;
        private SerializedProperty thrusterIsThrusterSystemsExpandedProp;
        private SerializedProperty thrusterIsThrusterListExpandedProp;
        private SerializedProperty showCOTGizmosInSceneViewProp;
        private SerializedProperty thrusterShowGizmosInSceneViewProp;
        private SerializedProperty thrusterNameProp;
        private SerializedProperty forceUseProp;
        private SerializedProperty primaryMomentUseProp;
        private SerializedProperty secondaryMomentUseProp;
        private SerializedProperty thrusterMaxThrustProp;
        private SerializedProperty thrusterThrottleProp;
        private SerializedProperty thrusterRelativePositionProp;
        private SerializedProperty thrusterThrustDirectionProp;
        private SerializedProperty thrusterDamageRegionIndexProp;
        private SerializedProperty thrusterLimitEffectsOnZProp;
        private SerializedProperty thrusterMinEffectsOnZProp;
        private SerializedProperty thrusterMaxEffectsOnZProp;
        private SerializedProperty thrusterLimitEffectsOnYProp;
        private SerializedProperty thrusterMinEffectsOnYProp;
        private SerializedProperty thrusterMaxEffectsOnYProp;
        private SerializedProperty thrusterFuelLevelProp;
        private SerializedProperty thrusterHeatLevelProp;

        // Aerodynamics
        private SerializedProperty dragReferenceAreasProp;
        private SerializedProperty disableDragMomentsProp;
        private SerializedProperty dragCoefficientsProp;
        private SerializedProperty centreOfDragXMomentProp;
        private SerializedProperty centreOfDragYMomentProp;
        private SerializedProperty centreOfDragZMomentProp;
        private SerializedProperty showCOLGizmosInSceneViewProp;
        private SerializedProperty wingListProp;
        private SerializedProperty wingProp;
        private SerializedProperty wingDamageRegionIndexProp;
        private SerializedProperty wingNameProp;
        private SerializedProperty wingShowInEditorProp;
        private SerializedProperty wingIsWingListExpandedProp;
        private SerializedProperty wingShowGizmosInSceneViewProp;
        private SerializedProperty controlSurfaceListProp;
        private SerializedProperty controlSurfaceProp;
        private SerializedProperty controlSurfaceDamageRegionIndexProp;
        private SerializedProperty controlSurfaceShowInEditorProp;
        private SerializedProperty controlSurfaceIsControlSurfaceListExpandedProp;
        private SerializedProperty controlSurfaceShowGizmosInSceneViewProp;
        private SerializedProperty arcadeUseBrakeComponentProp;

        // Combat
        private SerializedProperty shipDamageModelProp;
        private SerializedProperty showDamageInEditorProp;
        private SerializedProperty damageRegionProp;
        private SerializedProperty respawningModeProp;
        private SerializedProperty collisionRespawnPositionDelayProp;
        private SerializedProperty respawningPathGUIDHashProp;
        private SerializedProperty respawningStuckTimeProp;
        private SerializedProperty respawningStuckActionProp;
        private SerializedProperty respawningStuckActionPathGUIDHashProp;
        private SerializedProperty useShieldingProp;
        private SerializedProperty shieldingRechargeRateProp;
        private SerializedProperty applyControllerRumbleProp;
        private SerializedProperty useDamageMultipliersProp;
        private SerializedProperty useLocalisedDamageMultipliersProp;
        //private SerializedProperty damageRegionDamageMultipliersProp;
        private SerializedProperty weaponListProp;
        private SerializedProperty weaponProp;
        private SerializedProperty weaponNameProp;
        private SerializedProperty weaponTypeProp;
        private SerializedProperty weaponFiringButtonProp;
        private SerializedProperty weaponDamageRegionIndexProp;
        private SerializedProperty weaponAmmunitionProp;
        private SerializedProperty weaponChargeAmountProp;
        private SerializedProperty weaponRechargeTimeProp;
        private SerializedProperty weaponIsAutoTargetingEnabledProp;
        private SerializedProperty weaponShowInEditorProp;
        private SerializedProperty weaponIsWeaponListExpandedProp;
        private SerializedProperty weaponShowGizmosInSceneViewProp;
        private SerializedProperty weaponFirePositionListProp;
        private SerializedProperty weaponIsMultipleFirePositionsProp;
        private SerializedProperty weaponturretPivotYProp;
        private SerializedProperty weaponRelativePositionProp;
        private SerializedProperty weaponFireDirectionProp;
        private SerializedProperty weaponProjectilePrefabProp;
        private SerializedProperty weaponBeamPrefabProp;
        private SerializedProperty weaponHeatLevelProp;
        private SerializedProperty weaponOverHeatThresholdProp;
        private SerializedProperty damageRegionListProp;
        private SerializedProperty damageRegionShowInEditorProp;
        private SerializedProperty damageRegionIsMainDamageRegionExpandedProp;
        private SerializedProperty damageRegionIsDamageRegionListExpandedProp;
        private SerializedProperty damageRegionShowGizmosInSceneViewProp;
        private SerializedProperty damageRegionNameProp;
        private SerializedProperty damageRegionEffectsObjectProp;
        private SerializedProperty damageRegionDestructObjectProp;
        private SerializedProperty damageRegionChildTransformProp;
        private SerializedProperty identityIsRadarEnabledProp;

        // Show Settings properties
        private SerializedProperty selectedTabIntProp;

        #endregion

        #region Events

        public void OnEnable()
        {
            shipControlModule = (ShipControlModule)target;

            // Only use if require scene view interaction
            #if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= SceneGUI;
            SceneView.duringSceneGui += SceneGUI;
            #else
            SceneView.onSceneGUIDelegate -= SceneGUI;
            SceneView.onSceneGUIDelegate += SceneGUI;
            #endif

            // Used in Richtext labels
            if (EditorGUIUtility.isProSkin) { txtColourName = "White"; defaultTextColour = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f); }

            // Keep compiler happy - can remove this later if it isn't required
            if (defaultTextColour.a > 0f) { }

            defaultEditorLabelWidth = 150f; // EditorGUIUtility.labelWidth;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            // Initialise properties
            shipInstanceProp = serializedObject.FindProperty("shipInstance");
            shipPhysicsModelProp = shipInstanceProp.FindPropertyRelative("shipPhysicsModel");
            shipDamageModelProp = shipInstanceProp.FindPropertyRelative("shipDamageModel");

            // Aerodynamics

            // Thrusters
            thrusterEffectObjectsProp = serializedObject.FindProperty("thrusterEffectObjects");

            // Selected tab property
            selectedTabIntProp = serializedObject.FindProperty("selectedTabInt");

            // Initialise lists
            // Get a reference to the thrusterList the current ship
            thrustersList = shipControlModule.shipInstance.thrusterList;
            thrusterEffectsList = new List<GameObject>();

            sscHelpPDF = SSCEditorHelper.GetHelpURL();

            // Reset GUIStyles
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            foldoutStyleNoLabel = null;
            searchTextFieldStyle = null;
            searchCancelButtonStyle = null;

            if (shipControlModule.editorSearchThrusterFilter == null) { shipControlModule.editorSearchThrusterFilter = string.Empty; }

            // Deselect all components in the scene view
            DeselectAllComponents();

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 1f) : Color.grey;
        }

        /// <summary>
        /// Called when the gameobject loses focus or Unity Editor enters/exits
        /// play mode
        /// </summary>
        void OnDestroy()
        {
            #if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= SceneGUI;
            #else
            SceneView.onSceneGUIDelegate -= SceneGUI;
            #endif

            // Always unhide Unity tools when losing focus on this gameObject
            Tools.hidden = false;
        }

        /// <summary>
        /// Gets called automatically 10 times per second
        /// Comment out if not required
        /// </summary>
        void OnInspectorUpdate()
        {
            // OnInspectorGUI() only registers events when the mouse is positioned over the custom editor window
            // This code forces OnInspectorGUI() to run every frame, so it registers events even when the mouse
            // is positioned over the scene view
            if (shipControlModule.allowRepaint) { Repaint(); }
        }

        #endregion

        #region Private Methods - Copy Paste

        /// <summary>
        /// Attempt to copy all the thrusters into the buffer
        /// </summary>
        private void CopyThrusterList()
        {
            if (thrustersList != null)
            {
                ValidateThrusterBuffer();

                int numThrusters = thrustersList.Count;

                if (numThrusters == 0)
                {
                    SSCEditorHelper.PromptGotIt("Copy Thrusters", "No thrusters to copy");
                    GUIUtility.ExitGUI();
                }
                else
                {
                    thrusterCopyBuffer.Clear();

                    // Perform a deep copy
                    thrusterCopyBuffer = thrustersList.ConvertAll(thruster => new Thruster(thruster));

                    int numCopied = thrusterCopyBuffer.Count;

                    Debug.Log("[INFO] Copied " + numCopied + " thrusters into the buffer");
                }
            }
        }

        /// <summary>
        /// Attempt to paste (overwrite/replace) all current thrusters with those in the copy buffer
        /// </summary>
        private void PasteThrusterList()
        {
            if (thrustersList != null)
            {
                ValidateThrusterBuffer();

                // Current number of thrusters
                int numThrusters = thrustersList.Count;
                bool isContinue = true;

                int numCpyThrusters = thrusterCopyBuffer.Count;

                if (numCpyThrusters == 0)
                {
                    SSCEditorHelper.PromptGotIt("Paste Thrusters", "No thrusters in the copy buffer");
                    GUIUtility.ExitGUI();
                }
                else if (numThrusters > 0)
                {
                    isContinue = SSCEditorHelper.PromptForContinue("Overwrite Thrusters", "Do you wish to delete the current thrusters and replace them with those from the copy buffer?");

                    if (!isContinue) { GUIUtility.ExitGUI(); }
                }
                
                if (isContinue)
                {
                    // Perform a deep copy
                    thrustersList = thrusterCopyBuffer.ConvertAll(thruster => new Thruster(thruster));

                    // Remove references to thruster effects objects
                    thrusterEffectsList.Clear();

                    int numPasted = thrustersList.Count;

                    // Add empty thruster FX references
                    for (int thIdx = 0; thIdx < numPasted; thIdx++)
                    {
                        thrusterEffectsList.Add(null);
                    }

                    if (thrustersList.Count == thrusterEffectsList.Count)
                    {
                        shipControlModule.shipInstance.thrusterList = thrustersList;
                        shipControlModule.thrusterEffectObjects = thrusterEffectsList.ToArray();

                        if (shipControlModule.IsInitialised)
                        {
                            shipControlModule.shipInstance.ReinitialiseThrusterVariables();
                            shipControlModule.shipInstance.ReinitialiseInputVariables();
                        }

                        Debug.Log("[INFO] Overwrote " + numThrusters + " thrusters with " + numPasted + " from the copy buffer");
                    }
                }
            }
        }

        private void ValidateThrusterBuffer()
        {
            if (thrusterCopyBuffer == null) { thrusterCopyBuffer = new List<Thruster>(20); }
        }

        #endregion

        #region Private Methods - General

        private void SceneGUI (SceneView sv)
        {
            if (shipControlModule != null && shipControlModule.gameObject.activeInHierarchy)
            {
                isSceneDirtyRequired = false;

                // Declare variables
                int numWings = shipControlModule.shipInstance.wingList == null ? 0 : shipControlModule.shipInstance.wingList.Count;
                int numThrusters = shipControlModule.shipInstance.thrusterList.Count;
                int numControlSurfaces = shipControlModule.shipInstance.controlSurfaceList == null ? 0 : shipControlModule.shipInstance.controlSurfaceList.Count;
                int numWeapons = shipControlModule.shipInstance.weaponList == null ? 0 : shipControlModule.shipInstance.weaponList.Count;

                int numDamageRegions = 0;
                if (shipControlModule.shipInstance.shipDamageModel == Ship.ShipDamageModel.Localised)
                {
                    numDamageRegions = shipControlModule.shipInstance.localisedDamageRegionList == null ? 0 : shipControlModule.shipInstance.localisedDamageRegionList.Count;
                }

                // Get the rotation of the ship in the scene
                sceneViewShipRotation = Quaternion.LookRotation(shipControlModule.transform.forward, shipControlModule.transform.up);

                //Quaternion shipInvRot = Quaternion.Inverse(shipControlModule.transform.rotation);

                using (new Handles.DrawingScope(Color.grey))
                {
                    #region Centre Of Mass and Gravity

                    // Get centre of mass handle position (we also use this for wings for physicsmodel=arcade so do regardless of gizmos visibility
                    centreOfMassHandlePosition = shipControlModule.transform.TransformPoint(shipControlModule.shipInstance.centreOfMass);

                    if (shipControlModule.shipInstance.showCOMGizmosInSceneView)
                    {
                        // Prevent gravity direction ever being the zero vector
                        if (shipControlModule.shipInstance.gravityDirection == Vector3.zero) { shipControlModule.shipInstance.gravityDirection = Vector3.down; }

                        // Get centre of mass handle rotation
                        centreOfMassHandleRotation = Quaternion.LookRotation(shipControlModule.shipInstance.gravityDirection, Vector3.forward);

                        // Draw a an arrow in the scene that is non-interactable
                        if (Event.current.type == EventType.Repaint)
                        {
                            Handles.ArrowHandleCap(0, centreOfMassHandlePosition, centreOfMassHandleRotation, 2f, EventType.Repaint);
                        }

                        if (shipControlModule.shipInstance.setCentreOfMassManually)
                        {
                            if (isCentreOfMassSelected)
                            {
                                // Choose which handle to draw based on which Unity tool is selected
                                if (Tools.current == Tool.Rotate)
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a rotation handle
                                    centreOfMassHandleRotation = Handles.RotationHandle(centreOfMassHandleRotation, centreOfMassHandlePosition);

                                    // Use the rotation handle to edit the direction of gravity
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(shipControlModule, "Rotate Gravity Direction");
                                        shipControlModule.shipInstance.gravityDirection = centreOfMassHandleRotation * Vector3.forward;
                                    }
                                }
                                else
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a movement handle
                                    centreOfMassHandlePosition = Handles.PositionHandle(centreOfMassHandlePosition, sceneViewShipRotation);

                                    // Use the position handle to edit the position of the centre of mass
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(shipControlModule, "Move Centre Of Mass");
                                        shipControlModule.shipInstance.centreOfMass = shipControlModule.transform.InverseTransformPoint(centreOfMassHandlePosition);
                                    }
                                }
                            }

                            // Allow the user to select/deselect the centre of mass
                            if (Handles.Button(centreOfMassHandlePosition, Quaternion.identity, 1f, 0.5f, Handles.SphereHandleCap))
                            {
                                if (isCentreOfMassSelected)
                                {
                                    DeselectAllComponents();
                                }
                                else
                                {
                                    DeselectAllComponents();
                                    isCentreOfMassSelected = true;
                                    // Hide Unity tools
                                    Tools.hidden = true;
                                }
                            }
                        }
                        else
                        {
                            isCentreOfMassSelected = false;

                            // Draw a sphere and arrow in the scene that are non-interactable
                            if (Event.current.type == EventType.Repaint)
                            {
                                // Only draw this sphere if we are not already drawing the clickable version
                                Handles.SphereHandleCap(0, centreOfMassHandlePosition, Quaternion.identity, 1f, EventType.Repaint);
                            }
                        }
                    }
                    #endregion
                }

                // If we change the colour here, we also need to set it in Tool.Scale code below)
                using (new Handles.DrawingScope(Color.cyan))
                {
                    #region Individual Wings

                    for (int wi = 0; wi < numWings; wi++)
                    {
                        wingComponent = shipControlModule.shipInstance.wingList[wi];

                        // Prevent lift direction ever being the zero vector
                        if (wingComponent.liftDirection == Vector3.zero) { wingComponent.liftDirection = Vector3.up; }

                        if (wingComponent.showGizmosInSceneView)
                        {
                            // Get component handle position
                            if (shipControlModule.shipInstance.shipPhysicsModel == Ship.ShipPhysicsModel.PhysicsBased)
                            {
                                componentHandlePosition = shipControlModule.transform.TransformPoint(wingComponent.relativePosition);
                            }
                            else { componentHandlePosition = centreOfMassHandlePosition; }

                            // Get component handle rotation
                            // TODO: This doesn't really seem all that correct... (specifying the 'up' direction?)
                            componentHandleRotation = Quaternion.LookRotation(shipControlModule.transform.TransformDirection(wingComponent.liftDirection), shipControlModule.transform.up);

                            // Get component handle scale
                            componentHandleScale.x = wingComponent.span;
                            componentHandleScale.y = 1f;
                            componentHandleScale.z = wingComponent.chord;

                            // Draw a an arrow in the scene that is non-interactable
                            if (Event.current.type == EventType.Repaint)
                            {
                                Handles.ArrowHandleCap(0, componentHandlePosition, componentHandleRotation, 1f, EventType.Repaint);
                            }

                            // Draw a rectangle outlining the wing's shape and size
                            Color transparentCyan = Color.cyan;
                            transparentCyan.a = 0.1f;
                            // The rectangle needs to be perpendicular to lift direction
                            Vector3 worldSpaceLiftDir = shipControlModule.transform.TransformDirection(wingComponent.liftDirection);
                            wingWidthDir = Vector3.Cross(Vector3.ProjectOnPlane(worldSpaceLiftDir, shipControlModule.transform.right), shipControlModule.transform.right).normalized;
                            wingLengthDir = Vector3.Cross(wingWidthDir, worldSpaceLiftDir).normalized;
                            Vector3[] wingVerts = new Vector3[4];
                            wingVerts[0] = componentHandlePosition - (wingLengthDir * wingComponent.span * 0.5f) - (wingWidthDir * wingComponent.chord * 0.5f);
                            wingVerts[1] = componentHandlePosition - (wingLengthDir * wingComponent.span * 0.5f) + (wingWidthDir * wingComponent.chord * 0.5f);
                            wingVerts[2] = componentHandlePosition + (wingLengthDir * wingComponent.span * 0.5f) + (wingWidthDir * wingComponent.chord * 0.5f);
                            wingVerts[3] = componentHandlePosition + (wingLengthDir * wingComponent.span * 0.5f) - (wingWidthDir * wingComponent.chord * 0.5f);
                            Handles.DrawSolidRectangleWithOutline(wingVerts, transparentCyan, Color.cyan);
                            //Debug.Log("wing " + Time.time);

                            if (wingComponent.selectedInSceneView)
                            {
                                // Choose which handle to draw based on which Unity tool is selected
                                if (Tools.current == Tool.Rotate)
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a rotation handle
                                    componentHandleRotation = Handles.RotationHandle(componentHandleRotation, componentHandlePosition);

                                    // Use the rotation handle to edit the direction of lift
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(shipControlModule, "Rotate Lift Direction");
                                        wingComponent.liftDirection = shipControlModule.transform.InverseTransformDirection(componentHandleRotation * Vector3.forward);
                                    }
                                }
                                else if (Tools.current == Tool.Scale)
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a scaling handle
                                    componentHandleScale = Handles.ScaleHandle(componentHandleScale, componentHandlePosition, sceneViewShipRotation, HandleUtility.GetHandleSize(componentHandlePosition));

                                    // ScaleHandle calls DoScaleHandle which chances the colour, so change it back - must match DrawScope above
                                    Handles.color = Color.cyan;

                                    // Use the scale handle to edit the width and length of the wing
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(shipControlModule, "Scale Wing");
                                        wingComponent.span = componentHandleScale.x;
                                        wingComponent.chord = componentHandleScale.z;
                                    }
                                }
                                else
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a movement handle
                                    componentHandlePosition = Handles.PositionHandle(componentHandlePosition, sceneViewShipRotation);

                                    // Use the position handle to edit the position of the wing
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(shipControlModule, "Move Wing");
                                        wingComponent.relativePosition = shipControlModule.transform.InverseTransformPoint(componentHandlePosition);
                                    }
                                }
                            }

                            // Allow the user to select/deselect the wing
                            if (Handles.Button(componentHandlePosition, Quaternion.identity, 0.5f, 0.25f, Handles.SphereHandleCap))
                            {
                                if (wingComponent.selectedInSceneView)
                                {
                                    DeselectAllComponents();
                                    wingComponent.showInEditor = false;
                                }
                                else
                                {
                                    DeselectAllComponents();
                                    wingComponent.selectedInSceneView = true;
                                    wingComponent.showInEditor = true;
                                    // Hide Unity tools
                                    Tools.hidden = true;
                                }
                            }
                        }
                    }

                    #endregion

                    #region Centre And Direction Of Lift

                    // Draw a sphere and an arrow in the scene that are non-interactable
                    if (shipControlModule.shipInstance.showCOLGizmosInSceneView && Event.current.type == EventType.Repaint)
                    {
                        if (shipControlModule.shipInstance.shipPhysicsModel == Ship.ShipPhysicsModel.PhysicsBased)
                        {
                            // Calculate average (weighted by wing area) of lift centre and direction 
                            centreOfLiftHandlePosition = Vector3.zero;
                            directionOfLiftHandleVector = Vector3.zero;
                            float wingsDenominator = 0f, wingArea = 0f;
                            for (int wi = 0; wi < numWings; wi++)
                            {
                                wingComponent = shipControlModule.shipInstance.wingList[wi];
                                wingArea = wingComponent.chord * wingComponent.span;
                                centreOfLiftHandlePosition += wingArea * wingComponent.relativePosition;
                                directionOfLiftHandleVector += wingArea * wingComponent.liftDirection.normalized;
                                wingsDenominator += wingArea;
                            }
                            if (wingsDenominator > 0f)
                            {
                                centreOfLiftHandlePosition /= wingsDenominator;
                                directionOfLiftHandleVector = (directionOfLiftHandleVector / wingsDenominator).normalized;
                            }
                            else { directionOfLiftHandleVector = Vector3.zero; }
                        }
                        else
                        {
                            // Centre of lift is always through the centre of mass
                            centreOfLiftHandlePosition = shipControlModule.shipInstance.centreOfMass;
                            // Calculate average (weighted by wing area) of lift direction 
                            float wingsDenominator = 0f, wingArea = 0f;
                            for (int wi = 0; wi < numWings; wi++)
                            {
                                wingComponent = shipControlModule.shipInstance.wingList[wi];
                                wingArea = wingComponent.chord * wingComponent.span;
                                directionOfLiftHandleVector += wingArea * wingComponent.liftDirection.normalized;
                                wingsDenominator += wingArea;
                            }
                            if (wingsDenominator > 0f)
                            {
                                directionOfLiftHandleVector = (directionOfLiftHandleVector / wingsDenominator).normalized;
                            }
                            else { directionOfLiftHandleVector = Vector3.zero; }
                        }

                        // Transform vectors into world space
                        centreOfLiftHandlePosition = shipControlModule.transform.TransformPoint(centreOfLiftHandlePosition);
                        directionOfLiftHandleVector = shipControlModule.transform.TransformDirection(directionOfLiftHandleVector);

                        // Only draw centre/direction of lift if net lift is produced
                        if (directionOfLiftHandleVector != Vector3.zero)
                        {
                            Handles.SphereHandleCap(0, centreOfLiftHandlePosition, Quaternion.identity, 0.75f, EventType.Repaint);
                            Handles.ArrowHandleCap(0, centreOfLiftHandlePosition, Quaternion.LookRotation(directionOfLiftHandleVector), 2f, EventType.Repaint);
                            Handles.DrawDottedLine(centreOfLiftHandlePosition - (directionOfLiftHandleVector * 100f),
                                centreOfLiftHandlePosition + (directionOfLiftHandleVector * 100f), 4f);
                        }
                    }

                    #endregion
                }

                if (shipControlModule.shipInstance.shipPhysicsModel == Ship.ShipPhysicsModel.PhysicsBased)
                {
                    // If we change the colour here, we also need to set it in Tool.Scale code below)
                    using (new Handles.DrawingScope(Color.yellow))
                    {
                        #region Individual Control Surfaces

                        for (int ci = 0; ci < numControlSurfaces; ci++)
                        {
                            controlSurfaceComponent = shipControlModule.shipInstance.controlSurfaceList[ci];

                            if (controlSurfaceComponent.showGizmosInSceneView)
                            {
                                // Get component handle position
                                componentHandlePosition = shipControlModule.transform.TransformPoint(controlSurfaceComponent.relativePosition);

                                // Get component handle scale - different control surfaces have different orientations
                                if (controlSurfaceComponent.type == ControlSurface.ControlSurfaceType.Aileron ||
                                    controlSurfaceComponent.type == ControlSurface.ControlSurfaceType.Elevator)
                                {
                                    // Ailerons and elevators
                                    componentHandleScale.x = controlSurfaceComponent.span;
                                    componentHandleScale.y = 1f;
                                    componentHandleScale.z = controlSurfaceComponent.chord;
                                }
                                else if (controlSurfaceComponent.type == ControlSurface.ControlSurfaceType.Rudder)
                                {
                                    // Rudders
                                    componentHandleScale.x = 1f;
                                    componentHandleScale.y = controlSurfaceComponent.span;
                                    componentHandleScale.z = controlSurfaceComponent.chord;
                                }
                                else
                                {
                                    // Air brakes
                                    componentHandleScale.x = controlSurfaceComponent.span;
                                    componentHandleScale.y = controlSurfaceComponent.chord;
                                    componentHandleScale.z = 1f;
                                }

                                // Draw a rectangle outlining the wing's shape and size
                                Color transparentYellow = Color.yellow;
                                transparentYellow.a = 0.1f;
                                bool drawFromEdge = true;
                                if (controlSurfaceComponent.type == ControlSurface.ControlSurfaceType.Aileron ||
                                    controlSurfaceComponent.type == ControlSurface.ControlSurfaceType.Elevator)
                                {
                                    controlSurfaceLengthDir = shipControlModule.transform.right;
                                    controlSurfaceWidthDir = shipControlModule.transform.forward;
                                    drawFromEdge = true;
                                }
                                else if (controlSurfaceComponent.type == ControlSurface.ControlSurfaceType.Rudder)
                                {
                                    controlSurfaceLengthDir = shipControlModule.transform.up;
                                    controlSurfaceWidthDir = shipControlModule.transform.forward;
                                    drawFromEdge = true;
                                }
                                else
                                {
                                    controlSurfaceLengthDir = shipControlModule.transform.right;
                                    controlSurfaceWidthDir = shipControlModule.transform.up;
                                    drawFromEdge = false;
                                }
                                Vector3[] controlSurfaceVerts = new Vector3[4];
                                float chordEdgeMultiplier1 = drawFromEdge ? 0f : 0.5f, chordEdgeMultiplier2 = drawFromEdge ? 1f : 0.5f;
                                controlSurfaceVerts[0] = componentHandlePosition - (controlSurfaceLengthDir * controlSurfaceComponent.span * 0.5f) + (controlSurfaceWidthDir * controlSurfaceComponent.chord * chordEdgeMultiplier1);
                                controlSurfaceVerts[1] = componentHandlePosition - (controlSurfaceLengthDir * controlSurfaceComponent.span * 0.5f) - (controlSurfaceWidthDir * controlSurfaceComponent.chord * chordEdgeMultiplier2);
                                controlSurfaceVerts[2] = componentHandlePosition + (controlSurfaceLengthDir * controlSurfaceComponent.span * 0.5f) - (controlSurfaceWidthDir * controlSurfaceComponent.chord * chordEdgeMultiplier2);
                                controlSurfaceVerts[3] = componentHandlePosition + (controlSurfaceLengthDir * controlSurfaceComponent.span * 0.5f) + (controlSurfaceWidthDir * controlSurfaceComponent.chord * chordEdgeMultiplier1);
                                Handles.DrawSolidRectangleWithOutline(controlSurfaceVerts, transparentYellow, Color.yellow);

                                if (controlSurfaceComponent.selectedInSceneView)
                                {
                                    // TODO: In future will require some sort of rotation for custom control surfaces
                                    // Choose which handle to draw based on which Unity tool is selected
                                    //if (Tools.current == Tool.Rotate && controlSurfaceComponent.type == ControlSurface.ControlSurfaceType.Custom)
                                    //{
                                    //    EditorGUI.BeginChangeCheck();

                                    //    // Draw a rotation handle
                                    //    componentHandleRotation = Handles.RotationHandle(componentHandleRotation, componentHandlePosition);

                                    //    // Use the rotation handle to edit the axis of rotation
                                    //    if (EditorGUI.EndChangeCheck())
                                    //    {
                                    //        isSceneDirtyRequired = true;
                                    //        //Undo.RecordObject(shipControlModule, "Rotate Control Surface Axis");
                                    //        //wingComponent.liftDirection = shipControlModule.transform.InverseTransformDirection(componentHandleRotation * Vector3.forward);
                                    //    }
                                    //}
                                    if (Tools.current == Tool.Scale)
                                    {
                                        EditorGUI.BeginChangeCheck();

                                        // Draw a scaling handle
                                        componentHandleScale = Handles.ScaleHandle(componentHandleScale, componentHandlePosition, sceneViewShipRotation, HandleUtility.GetHandleSize(componentHandlePosition));

                                        // ScaleHandle calls DoScaleHandle which chances the colour, so change it back - must match DrawScope above
                                        Handles.color = Color.yellow;

                                        // Use the scale handle to edit the width and length of the control surface
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            isSceneDirtyRequired = true;
                                            Undo.RecordObject(shipControlModule, "Scale Control Surface");
                                            // Different control surfaces have different orientations
                                            if (controlSurfaceComponent.type == ControlSurface.ControlSurfaceType.Aileron ||
                                                controlSurfaceComponent.type == ControlSurface.ControlSurfaceType.Elevator)
                                            {
                                                // Ailerons and elevators
                                                controlSurfaceComponent.span = componentHandleScale.x;
                                                controlSurfaceComponent.chord = componentHandleScale.z;
                                            }
                                            else if (controlSurfaceComponent.type == ControlSurface.ControlSurfaceType.Rudder)
                                            {
                                                // Rudders
                                                controlSurfaceComponent.span = componentHandleScale.y;
                                                controlSurfaceComponent.chord = componentHandleScale.z;
                                            }
                                            else
                                            {
                                                // Air brakes
                                                controlSurfaceComponent.span = componentHandleScale.x;
                                                controlSurfaceComponent.chord = componentHandleScale.y;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        EditorGUI.BeginChangeCheck();

                                        // Draw a movement handle
                                        componentHandlePosition = Handles.PositionHandle(componentHandlePosition, sceneViewShipRotation);

                                        // Use the position handle to edit the position of the control surface
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            isSceneDirtyRequired = true;
                                            Undo.RecordObject(shipControlModule, "Move Control Surface");
                                            controlSurfaceComponent.relativePosition = shipControlModule.transform.InverseTransformPoint(componentHandlePosition);
                                        }
                                    }
                                }

                                // Allow the user to select/deselect the control surface
                                if (Handles.Button(componentHandlePosition, Quaternion.identity, 0.5f, 0.25f, Handles.SphereHandleCap))
                                {
                                    if (controlSurfaceComponent.selectedInSceneView)
                                    {
                                        DeselectAllComponents();
                                        controlSurfaceComponent.showInEditor = false;
                                    }
                                    else
                                    {
                                        DeselectAllComponents();
                                        controlSurfaceComponent.selectedInSceneView = true;
                                        controlSurfaceComponent.showInEditor = true;
                                        // Hide Unity tools
                                        Tools.hidden = true;
                                    }
                                }
                            }
                        }

                        #endregion
                    }
                }

                #region Weapons
                using (new Handles.DrawingScope(weaponGizmoColour))
                {
                    for (int wpi = 0; wpi < numWeapons; wpi++)
                    {
                        weaponComponent = shipControlModule.shipInstance.weaponList[wpi];

                        // Prevent fire direction ever being a zero vector
                        if (weaponComponent.fireDirection == Vector3.zero) { weaponComponent.fireDirection = Vector3.forward; }

                        if (weaponComponent.showGizmosInSceneView)
                        {
                            componentHandlePosition = shipControlModule.transform.TransformPoint(weaponComponent.relativePosition);

                            // Get component handle rotation
                            componentHandleRotation = Quaternion.LookRotation(shipControlModule.transform.TransformDirection(weaponComponent.fireDirection), shipControlModule.transform.up);

                            handleDistanceScale = HandleUtility.GetHandleSize(componentHandlePosition);

                            // Draw a fire direction arrow in the scene that is non-interactable
                            if (Event.current.type == EventType.Repaint)
                            {
                                Handles.ArrowHandleCap(0, componentHandlePosition, componentHandleRotation, 1f, EventType.Repaint);

                                // Draw Fire Positions (non-interactable)
                                if (weaponComponent.isMultipleFirePositions)
                                {
                                    weaponFirePositionList = weaponComponent.firePositionList;
                                    numCompWeaponFirePositions = weaponComponent.firePositionList == null ? 0 : weaponComponent.firePositionList.Count;

                                    // Get base weapon world position (not accounting for relative fire position)
                                    Vector3 weaponWorldBasePosition = (shipControlModule.transform.rotation * weaponComponent.relativePosition) + shipControlModule.transform.position;

                                    // Get relative fire direction
                                    Vector3 weaponRelativeFireDirection = weaponComponent.fireDirection.normalized;

                                    Vector3 weaponRelativeFirePosition = Vector3.zero;

                                    for (int fposIdx = 0; fposIdx < numCompWeaponFirePositions; fposIdx++)
                                    {
                                        // Check if there are multiple fire positions
                                        if (weaponComponent.isMultipleFirePositions)
                                        {
                                            // Get relative fire position
                                            weaponRelativeFirePosition = weaponComponent.firePositionList[fposIdx];
                                        }

                                        // Get weapon world fire position
                                        Vector3 weaponWorldFirePosition = weaponWorldBasePosition + (shipControlModule.transform.rotation * weaponRelativeFirePosition);

                                        // Do not use transform.TransformPoint because it won't work correctly when the parent gameobject has scale not equal to 1,1,1.
                                        // TODO - fix turret code when ship is rotated
                                        if (weaponComponent.weaponType == Weapon.WeaponType.TurretProjectile && weaponComponent.turretPivotX != null)
                                        {
                                            Handles.SphereHandleCap(0, ((shipControlModule.transform.rotation *
                                                        weaponComponent.relativePosition) + (weaponComponent.turretPivotX.rotation * weaponFirePositionList[fposIdx])) +
                                                        shipControlModule.transform.position, componentHandleRotation, 0.2f, EventType.Repaint);

                                        }
                                        else
                                        {
                                            // v1.1.8 - does not work when ship is rotated
                                            //Handles.SphereHandleCap(0, ((shipControlModule.transform.rotation *
                                            //            weaponComponent.relativePosition) + weaponFirePositionList[fposIdx]) +
                                            //            shipControlModule.transform.position, componentHandleRotation, 0.2f, EventType.Repaint);

                                            // v1.1.9+
                                            Handles.SphereHandleCap(0, weaponWorldFirePosition, componentHandleRotation, 0.2f, EventType.Repaint);
                                        }
                                    }
                                }

                                // Draw firing arc for turrets
                                if (weaponComponent.weaponType == Weapon.WeaponType.TurretProjectile && weaponComponent.turretPivotX != null)
                                {
                                    // Horizontal rotation arc
                                    using (new Handles.DrawingScope(weaponGizmoTurretYColour))
                                    {
                                        Handles.DrawWireArc(componentHandlePosition, weaponComponent.turretPivotX.up, Quaternion.AngleAxis(weaponComponent.turretMinY, weaponComponent.turretPivotX.up) * weaponComponent.turretPivotX.forward, weaponComponent.turretMaxY + 360f - (weaponComponent.turretMinY + 360f), handleDistanceScale * 0.75f);
                                    }

                                    // Elevation arc
                                    using (new Handles.DrawingScope(weaponGizmoTurretXColour))
                                    {
                                        Handles.DrawSolidArc(componentHandlePosition, weaponComponent.turretPivotX.right * -1f, Quaternion.AngleAxis(weaponComponent.turretMinX, weaponComponent.turretPivotX.right * -1f) * weaponComponent.turretPivotX.forward, weaponComponent.turretMaxX + 360f - (weaponComponent.turretMinX + 360f), handleDistanceScale * 0.75f);
                                    }
                                }
                            }

                            if (weaponComponent.selectedInSceneView)
                            {
                                // Choose which handle to draw based on which Unity tool is selected
                                if (Tools.current == Tool.Rotate)
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a rotation handle
                                    componentHandleRotation = Handles.RotationHandle(componentHandleRotation, componentHandlePosition);

                                    // Use the rotation handle to edit the direction of thrust
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(shipControlModule, "Rotate Weapon Fire Direction");
                                       
                                        // The turret pivot y parent gameobject may be rotated
                                        if (weaponComponent.weaponType == Weapon.WeaponType.TurretProjectile && weaponComponent.turretPivotY != null)
                                        {
                                            //fireDirectionRotation = componentHandleRotation * weaponComponent.turretPivotY.rotation;
                                            fireDirectionRotation = componentHandleRotation;
                                        }
                                        else { fireDirectionRotation = componentHandleRotation; }

                                        weaponComponent.fireDirection = shipControlModule.transform.InverseTransformDirection(fireDirectionRotation * Vector3.forward);
                                    }
                                }
                                else if (Tools.current == Tool.Move || Tools.current == Tool.Transform)
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a movement handle
                                    componentHandlePosition = Handles.PositionHandle(componentHandlePosition, sceneViewShipRotation);

                                    // Use the position handle to edit the position of the weapon
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(shipControlModule, "Move Weapon");
                                        weaponComponent.relativePosition = shipControlModule.transform.InverseTransformPoint(componentHandlePosition);
                                    }
                                }
                            }

                            // Allow the user to select/deselect the weapon location in the scene view
                            if (Handles.Button(componentHandlePosition, Quaternion.identity, 0.5f, 0.25f, Handles.SphereHandleCap))
                            {
                                if (weaponComponent.selectedInSceneView)
                                {
                                    DeselectAllComponents();
                                    weaponComponent.showInEditor = false;
                                }
                                else
                                {
                                    DeselectAllComponents();
                                    shipControlModule.shipInstance.isWeaponListExpanded = false;
                                    ExpandList(shipControlModule.shipInstance.weaponList, false);
                                    weaponComponent.selectedInSceneView = true;
                                    weaponComponent.showInEditor = true;
                                    isSceneDirtyRequired = true;
                                    // Hide Unity tools
                                    Tools.hidden = true;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Local Damage Regions
                using (new Handles.DrawingScope(damageRegionGizmoColour))
                {
                    for (int dri = 0; dri < numDamageRegions; dri++)
                    {
                        damageRegionCommponent = shipControlModule.shipInstance.localisedDamageRegionList[dri];

                        if (damageRegionCommponent.showGizmosInSceneView)
                        {
                            componentHandlePosition = shipControlModule.transform.TransformPoint(damageRegionCommponent.relativePosition);

                            // Get component handle rotation (this could be simplier...)
                            componentHandleRotation = sceneViewShipRotation;

                            if (damageRegionCommponent.selectedInSceneView)
                            {
                                #if UNITY_2017_3_OR_NEWER
                                if (Tools.current == Tool.Move || Tools.current == Tool.Transform)
                                #else
                                if (Tools.current == Tool.Move)
                                #endif
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a movement handle
                                    componentHandlePosition = Handles.PositionHandle(componentHandlePosition, sceneViewShipRotation);

                                    // Use the position handle to edit the position of the local damage region
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(shipControlModule, "Move Local Damaage Region");
                                        damageRegionCommponent.relativePosition = shipControlModule.transform.InverseTransformPoint(componentHandlePosition);
                                    }
                                }
                                else if (Tools.current == Tool.Scale)
                                {
                                    EditorGUI.BeginChangeCheck();

                                    Vector3 scale = Handles.ScaleHandle(damageRegionCommponent.size, componentHandlePosition, componentHandleRotation, HandleUtility.GetHandleSize(componentHandlePosition));

                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(shipControlModule, "Resize Local Damaage Region");
                                        damageRegionCommponent.size = scale;
                                    }
                                }

                                using (new Handles.DrawingScope(Matrix4x4.TRS(componentHandlePosition, sceneViewShipRotation, Vector3.one)))
                                {
                                    Handles.DrawWireCube(Vector3.zero, damageRegionCommponent.size);
                                }
                            }

                            // Allow the user to select/deselect the damage region location in the scene view
                            if (Handles.Button(componentHandlePosition, Quaternion.identity, 0.5f, 0.25f, Handles.SphereHandleCap))
                            {
                                if (damageRegionCommponent.selectedInSceneView)
                                {
                                    DeselectAllComponents();
                                    damageRegionCommponent.showInEditor = false;
                                }
                                else
                                {
                                    DeselectAllComponents();
                                    damageRegionCommponent.selectedInSceneView = true;
                                    damageRegionCommponent.showInEditor = true;
                                    // Hide Unity tools
                                    Tools.hidden = true;
                                }
                            }

                        }
                    }
                }
                #endregion

                #region Thrusters
                using (new Handles.DrawingScope(Color.magenta))
                {
                    #region Individual Thrusters

                    for (int ti = 0; ti < numThrusters; ti++)
                    {
                        thrusterComponent = shipControlModule.shipInstance.thrusterList[ti];

                        // Prevent thrust direction ever being the zero vector
                        if (thrusterComponent.thrustDirection == Vector3.zero) { thrusterComponent.thrustDirection = Vector3.forward; }

                        if (thrusterComponent.showGizmosInSceneView)
                        {
                            // Get component handle position
                            if (shipControlModule.shipInstance.shipPhysicsModel == Ship.ShipPhysicsModel.PhysicsBased)
                            {
                                componentHandlePosition = shipControlModule.transform.TransformPoint(thrusterComponent.relativePosition);
                                gizmoPosition = componentHandlePosition;
                            }
                            else
                            {
                                componentHandlePosition = centreOfMassHandlePosition;
                                gizmoPosition = componentHandlePosition - shipControlModule.transform.TransformDirection(thrusterComponent.thrustDirection);
                            }

                            // Get component handle rotation
                            componentHandleRotation = Quaternion.LookRotation(shipControlModule.transform.TransformDirection(-thrusterComponent.thrustDirection), shipControlModule.transform.up);

                            // Draw an arrow in the scene that is non-interactable
                            if (Event.current.type == EventType.Repaint)
                            {
                                Handles.ArrowHandleCap(0, gizmoPosition, componentHandleRotation, 1f, EventType.Repaint);
                            }

                            if (thrusterComponent.selectedInSceneView)
                            {
                                // Choose which handle to draw based on which Unity tool is selected
                                if (Tools.current == Tool.Rotate)
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a rotation handle
                                    componentHandleRotation = Handles.RotationHandle(componentHandleRotation, componentHandlePosition);

                                    // Use the rotation handle to edit the direction of thrust
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(shipControlModule, "Rotate Thrust Direction");
                                        thrusterComponent.thrustDirection = shipControlModule.transform.InverseTransformDirection(componentHandleRotation * Vector3.back);
                                    }
                                }
                                else
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a movement handle
                                    componentHandlePosition = Handles.PositionHandle(componentHandlePosition, sceneViewShipRotation);

                                    // Use the position handle to edit the position of the the thruster
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(shipControlModule, "Move Thruster");
                                        thrusterComponent.relativePosition = shipControlModule.transform.InverseTransformPoint(componentHandlePosition);
                                    }
                                }
                            }

                            // Allow the user to select/deselect the thruster
                            if (Handles.Button(gizmoPosition, Quaternion.identity, 0.5f, 0.25f, Handles.SphereHandleCap))
                            {
                                if (thrusterComponent.selectedInSceneView)
                                {
                                    DeselectAllComponents();
                                    thrusterComponent.showInEditor = false;
                                }
                                else
                                {
                                    DeselectAllComponents();
                                    thrusterComponent.selectedInSceneView = true;
                                    thrusterComponent.showInEditor = true;
                                    // Hide Unity tools
                                    Tools.hidden = true;
                                }
                            }
                        }
                    }

                    #endregion

                    #region Centre And Direction Of Forward Thrust

                    // Draw a sphere and an arrow in the scene that are non-interactable
                    if (shipControlModule.shipInstance.showCOTGizmosInSceneView && Event.current.type == EventType.Repaint)
                    {
                        if (shipControlModule.shipInstance.shipPhysicsModel == Ship.ShipPhysicsModel.PhysicsBased)
                        {
                            // Calculate average (weighted by thrust force) of thrust centre and direction 
                            centreOfThrustHandlePosition = Vector3.zero;
                            directionOfThrustHandleVector = Vector3.zero;
                            float thrustersDenominator = 0f, thrustForce = 0f;
                            for (int ti = 0; ti < numThrusters; ti++)
                            {
                                thrusterComponent = shipControlModule.shipInstance.thrusterList[ti];
                                if (thrusterComponent.forceUse == 1)
                                {
                                    thrustForce = thrusterComponent.maxThrust;
                                    centreOfThrustHandlePosition += thrustForce * thrusterComponent.relativePosition;
                                    directionOfThrustHandleVector += thrustForce * thrusterComponent.thrustDirection.normalized;
                                    thrustersDenominator += thrustForce;
                                }
                            }
                            if (thrustersDenominator > 0f)
                            {
                                centreOfThrustHandlePosition /= thrustersDenominator;
                                directionOfThrustHandleVector = (directionOfThrustHandleVector / thrustersDenominator).normalized;
                            }
                            else { directionOfThrustHandleVector = Vector3.zero; }
                        }
                        else
                        {
                            // Centre of thrust is always through the centre of mass
                            centreOfThrustHandlePosition = shipControlModule.shipInstance.centreOfMass;
                            // Calculate average (weighted by thrust force) of thrust direction 
                            directionOfThrustHandleVector = Vector3.zero;
                            float thrustersDenominator = 0f, thrustForce = 0f;
                            for (int ti = 0; ti < numThrusters; ti++)
                            {
                                thrusterComponent = shipControlModule.shipInstance.thrusterList[ti];
                                if (thrusterComponent.forceUse == 1)
                                {
                                    thrustForce = thrusterComponent.maxThrust;
                                    directionOfThrustHandleVector += thrustForce * thrusterComponent.thrustDirection.normalized;
                                    thrustersDenominator += thrustForce;
                                }
                            }
                            if (thrustersDenominator > 0f)
                            {
                                directionOfThrustHandleVector = (directionOfThrustHandleVector / thrustersDenominator).normalized;
                            }
                            else { directionOfThrustHandleVector = Vector3.zero; }
                        }

                        // Transform vectors into world space
                        centreOfThrustHandlePosition = shipControlModule.transform.TransformPoint(centreOfThrustHandlePosition);
                        directionOfThrustHandleVector = shipControlModule.transform.TransformDirection(directionOfThrustHandleVector);

                        // Only draw centre/direction of thrust if net thrust is produced
                        if (directionOfThrustHandleVector != Vector3.zero)
                        {
                            Handles.SphereHandleCap(0, centreOfThrustHandlePosition, Quaternion.identity, 0.5f, EventType.Repaint);
                            Handles.ArrowHandleCap(0, centreOfThrustHandlePosition, Quaternion.LookRotation(-directionOfThrustHandleVector), 2f, EventType.Repaint);
                            Handles.DrawDottedLine(centreOfThrustHandlePosition - (directionOfThrustHandleVector * 100f),
                                centreOfThrustHandlePosition + (directionOfThrustHandleVector * 100f), 4f);
                        }
                    }

                    #endregion
                }
                #endregion

                #region Centre(s) Of Drag
                #if SSC_SHOW_DRAG_MOMENTS
                using (new Handles.DrawingScope(Color.red))
                {
                    // Draw a sphere in the scene that is non-interactable
                    if (Event.current.type == EventType.Repaint)
                    {
                        Handles.SphereHandleCap(0, shipControlModule.transform.TransformPoint(shipControlModule.shipInstance.centreOfDragXMoment), Quaternion.identity, 0.25f, EventType.Repaint);
                    }
                }

                using (new Handles.DrawingScope(Color.green))
                {
                    // Draw a sphere in the scene that is non-interactable
                    if (Event.current.type == EventType.Repaint)
                    {
                        Handles.SphereHandleCap(0, shipControlModule.transform.TransformPoint(shipControlModule.shipInstance.centreOfDragYMoment), Quaternion.identity, 0.25f, EventType.Repaint);
                    }
                }

                using (new Handles.DrawingScope(Color.blue))
                {
                    // Draw a sphere in the scene that is non-interactable
                    if (Event.current.type == EventType.Repaint)
                    {
                        Handles.SphereHandleCap(0, shipControlModule.transform.TransformPoint(shipControlModule.shipInstance.centreOfDragZMoment), Quaternion.identity, 0.25f, EventType.Repaint);
                    }
                }
                #endif
                #endregion

                if (isSceneDirtyRequired && !Application.isPlaying)
                {
                    isSceneDirtyRequired = false;
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
            else
            {
                // Always unhide Unity tools and deselect all components when the object is disabled
                Tools.hidden = false;
                DeselectAllComponents();
            }
        }

        /// <summary>
        /// Deselect all components in the scene view edit mode, and unhides the Unity tools
        /// </summary>
        private void DeselectAllComponents ()
        {
            // Set all components to not be selected

            isCentreOfMassSelected = false;

            // Avoid situation where ship is destroyed in play mode while it is selected.
            if (shipControlModule != null && shipControlModule.shipInstance != null)
            {
                int numThrusters = shipControlModule.shipInstance.thrusterList.Count;
                for (int ti = 0; ti < numThrusters; ti++)
                {
                    shipControlModule.shipInstance.thrusterList[ti].selectedInSceneView = false;
                }

                int numWings = shipControlModule.shipInstance.wingList == null ? 0 : shipControlModule.shipInstance.wingList.Count;
                for (int wi = 0; wi < numWings; wi++)
                {
                    shipControlModule.shipInstance.wingList[wi].selectedInSceneView = false;
                }

                int numControlSurfaces = shipControlModule.shipInstance.controlSurfaceList == null ? 0 : shipControlModule.shipInstance.controlSurfaceList.Count;
                for (int ci = 0; ci < numControlSurfaces; ci++)
                {
                    shipControlModule.shipInstance.controlSurfaceList[ci].selectedInSceneView = false;
                }

                int numWeapons = shipControlModule.shipInstance.weaponList == null ? 0 : shipControlModule.shipInstance.weaponList.Count;
                for (int wpi = 0; wpi < numWeapons; wpi++)
                {
                    shipControlModule.shipInstance.weaponList[wpi].selectedInSceneView = false;
                }

                int numDamageRegions = shipControlModule.shipInstance.localisedDamageRegionList == null ? 0 : shipControlModule.shipInstance.localisedDamageRegionList.Count;
                for (int dri = 0; dri < numDamageRegions; dri++)
                {
                    shipControlModule.shipInstance.localisedDamageRegionList[dri].selectedInSceneView = false;
                }
            }
            // Unhide Unity tools
            Tools.hidden = false;
        }

        /// <summary>
        /// Expand (show) or collapse (hide) all items in a list in the editor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="componentList"></param>
        /// <param name="isExpanded"></param>
        private void ExpandList<T>(List<T> componentList, bool isExpanded)
        {
            int numComponents = componentList == null ? 0 : componentList.Count;

            if (numComponents > 0)
            {
                System.Type compType = typeof(T);

                if (compType == typeof(Weapon))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as Weapon).showInEditor = isExpanded;
                    }
                }
                else if (compType == typeof(Thruster))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as Thruster).showInEditor = isExpanded;
                    }
                }
                else if (compType == typeof(Wing))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as Wing).showInEditor = isExpanded;
                    }
                }
                else if (compType == typeof(ControlSurface))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as ControlSurface).showInEditor = isExpanded;
                    }
                }
                else if (compType == typeof(DamageRegion))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as DamageRegion).showInEditor = isExpanded;
                    }
                }
            }
        }

        /// <summary>
        /// Toggle on/off the gizmos and visualisations of a list of components based
        /// on the state of the first component in the list.
        /// Optionally force the gizmos to be turned off.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="componentList"></param>
        /// <param name="forceOff"></param>
        private void ToggleGizmos<T>(List<T> componentList, bool forceOff = false)
        {
            int numComponents = componentList == null ? 0 : componentList.Count;

            if (numComponents > 0)
            {
                System.Type compType = typeof(T);

                if (compType == typeof(Weapon))
                {
                    // Examine the first component
                    bool showGizmos = (componentList[0] as Weapon).showGizmosInSceneView;

                    if (forceOff) { showGizmos = true; }

                    // Toggle gizmos and visualisations to opposite of first member
                    for (int cpi = 0; cpi <numComponents; cpi++)
                    {
                        (componentList[cpi] as Weapon).showGizmosInSceneView = !showGizmos;
                    }
                }
                else if (compType == typeof(Thruster))
                {
                    // Examine the first component
                    bool showGizmos = (componentList[0] as Thruster).showGizmosInSceneView;

                    if (forceOff) { showGizmos = true; }

                    // Toggle gizmos and visualisations to opposite of first member
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as Thruster).showGizmosInSceneView = !showGizmos;
                    }
                }
                else if (compType == typeof(Wing))
                {
                    // Examine the first component
                    bool showGizmos = (componentList[0] as Wing).showGizmosInSceneView;

                    if (forceOff) { showGizmos = true; }

                    // Toggle gizmos and visualisations to opposite of first member
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as Wing).showGizmosInSceneView = !showGizmos;
                    }
                }
                else if (compType == typeof(ControlSurface))
                {
                    // Examine the first component
                    bool showGizmos = (componentList[0] as ControlSurface).showGizmosInSceneView;

                    if (forceOff) { showGizmos = true; }

                    // Toggle gizmos and visualisations to opposite of first member
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as ControlSurface).showGizmosInSceneView = !showGizmos;
                    }
                }
                else if (compType == typeof(DamageRegion))
                {
                    // Examine the first component
                    bool showGizmos = (componentList[0] as DamageRegion).showGizmosInSceneView;

                    if (forceOff) { showGizmos = true; }

                    // Toggle gizmos and visualisations to opposite of first member
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as DamageRegion).showGizmosInSceneView = !showGizmos;
                    }
                }

                SceneView.RepaintAll();
            }
        }

        /// <summary>
        /// Draw a (F)ind button which will select the item in the scene view
        /// </summary>
        /// <param name="listProp"></param>
        /// <param name="showInEditorProp"></param>
        /// <param name="selectedInSceneViewProp"></param>
        /// <param name="showGizmoInSceneViewProp"></param>
        private void SelectItemInSceneViewButton<T>(List<T> componentList, SerializedProperty showInEditorProp, SerializedProperty selectedInSceneViewProp, SerializedProperty showGizmoInSceneViewProp)
        {
            if (GUILayout.Button(gizmoFindBtnContent, buttonCompact, GUILayout.MaxWidth(20f)))
            {
                serializedObject.ApplyModifiedProperties();
                DeselectAllComponents();
                ExpandList(componentList, false);
                serializedObject.Update();
                selectedInSceneViewProp.boolValue = true;
                showInEditorProp.boolValue = true;
                showGizmoInSceneViewProp.boolValue = true;
                // Hide Unity tools
                Tools.hidden = true;
            }
        }

        /// <summary>
        /// Draw the array of damage region multipliers for a DamageRegion of the ship.
        /// Includes support for Editor Undo/Redo.
        /// </summary>
        /// <param name="damageRegion"></param>
        private void DrawDamageMultipliers(DamageRegion damageRegion)
        {
            // Apply property changes
            serializedObject.ApplyModifiedProperties();
            // Make sure that the damage multipliers array is the correct size
            damageRegion.Initialise();
            // Read in the properties
            serializedObject.Update();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();
            for (int dTypeIdx = 0; dTypeIdx < 6; dTypeIdx++)
            {
                ProjectileModule.DamageType thisDamageType = ProjectileModule.DamageType.Default;
                GUIContent thisDamageTypeGUIContent = typeADamageMultiplierContent;
                switch (dTypeIdx)
                {
                    case 0:
                        thisDamageType = ProjectileModule.DamageType.TypeA;
                        thisDamageTypeGUIContent = typeADamageMultiplierContent;
                        break;
                    case 1:
                        thisDamageType = ProjectileModule.DamageType.TypeB;
                        thisDamageTypeGUIContent = typeBDamageMultiplierContent;
                        break;
                    case 2:
                        thisDamageType = ProjectileModule.DamageType.TypeC;
                        thisDamageTypeGUIContent = typeCDamageMultiplierContent;
                        break;
                    case 3:
                        thisDamageType = ProjectileModule.DamageType.TypeD;
                        thisDamageTypeGUIContent = typeDDamageMultiplierContent;
                        break;
                    case 4:
                        thisDamageType = ProjectileModule.DamageType.TypeE;
                        thisDamageTypeGUIContent = typeEDamageMultiplierContent;
                        break;
                    case 5:
                        thisDamageType = ProjectileModule.DamageType.TypeF;
                        thisDamageTypeGUIContent = typeFDamageMultiplierContent;
                        break;
                }

                float thisDamageMultiplier = damageRegion.GetDamageMultiplier(thisDamageType);
                EditorGUI.BeginChangeCheck();
                thisDamageMultiplier = EditorGUILayout.FloatField(thisDamageTypeGUIContent, thisDamageMultiplier);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(shipControlModule, "Modify Damage " + thisDamageType.ToString());
                    damageRegion.SetDamageMultiplier(thisDamageType, thisDamageMultiplier);
                }
            }
            // Read in the properties
            serializedObject.Update();
        }

        #endregion

        #region OnInspectorGUI

        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            #region Initialise

            shipControlModule.allowRepaint = false;
            isSceneModified = false;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;

            #endregion

            #region Configure Buttons and Styles

            // Set up rich text GUIStyles
            helpBoxRichText = new GUIStyle("HelpBox");
            helpBoxRichText.richText = true;

            labelFieldRichText = new GUIStyle("Label");
            labelFieldRichText.richText = true;

            buttonCompact = new GUIStyle("Button");
            buttonCompact.fontSize = 10;

            // Set up the toggle buttons styles
            if (toggleCompactButtonStyleNormal == null)
            {
                // Create a new button or else will effect the Button style for other buttons too
                toggleCompactButtonStyleNormal = new GUIStyle("Button");
                toggleCompactButtonStyleToggled = new GUIStyle(toggleCompactButtonStyleNormal);
                toggleCompactButtonStyleNormal.fontStyle = FontStyle.Normal;
                toggleCompactButtonStyleToggled.fontStyle = FontStyle.Bold;
                toggleCompactButtonStyleToggled.normal.background = toggleCompactButtonStyleToggled.active.background;
            }

            if (foldoutStyleNoLabel == null)
            {
                // When using a no-label foldout, don't forget to set the global
                // EditorGUIUtility.fieldWidth to a small value like 15, then back
                // to the original afterward.
                foldoutStyleNoLabel = new GUIStyle(EditorStyles.foldout);
                foldoutStyleNoLabel.fixedWidth = 0.01f;
            }

           if (searchTextFieldStyle == null)
            {
                #if UNITY_2022_3_OR_NEWER
                searchTextFieldStyle = new GUIStyle(EditorStyles.toolbarSearchField);
                #else
                searchTextFieldStyle = new GUIStyle(GUI.skin.FindStyle("ToolbarSeachTextField"));
                #endif

                searchTextFieldStyle.stretchHeight = false;
                searchTextFieldStyle.stretchWidth = false;
                searchTextFieldStyle.fontSize = 10;
                searchTextFieldStyle.fixedHeight = 16f;
                searchTextFieldStyle.fixedWidth = defaultSearchTextFieldWidth;
            }

            if (searchCancelButtonStyle == null)
            {
                #if UNITY_2022_3_OR_NEWER
                searchCancelButtonStyle =  new GUIStyle(SSCUtils.ReflectionGetPropertyValue<GUIStyle>(typeof(EditorStyles), "toolbarSearchFieldCancelButton", null, true, true));
                #else
                searchCancelButtonStyle = new GUIStyle(GUI.skin.FindStyle("ToolbarSeachCancelButton"));
                #endif
            }

            #endregion

            // Read in all the properties
            serializedObject.Update();

            #region Header Info and Buttons
            SSCEditorHelper.SSCVersionHeader(labelFieldRichText);
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            SSCEditorHelper.DrawSSCGetHelpButtons(buttonCompact);
            #endregion

            // Show a toolbar to allow the user to switch between viewing different areas
            selectedTabIntProp.intValue = GUILayout.Toolbar(selectedTabIntProp.intValue, tabTexts);

            #region Create Damage Menu Array
            if (shipDamageModelProp.intValue == (int)Ship.ShipDamageModel.Localised)
            {
                int localisedDamageRegionsCount = shipControlModule.shipInstance.localisedDamageRegionList.Count;
                damageRegionIndexInts = new int[localisedDamageRegionsCount + 1];
                damageRegionIndexContents = new GUIContent[localisedDamageRegionsCount + 1];

                // Create a list of indices and GUIContents to go with each damage region, so that users can select damage
                // regions for each part when using the localised damage model
                for (int i = 0; i < localisedDamageRegionsCount + 1; i++)
                {
                    damageRegionIndexInts[i] = i - 1;
                    if (i == 0) { damageRegionIndexContents[i] = new GUIContent("None"); }
                    else { damageRegionIndexContents[i] = new GUIContent(shipControlModule.shipInstance.localisedDamageRegionList[i - 1].name); }
                }
            }
            #endregion

            #region Physics

            if (selectedTabIntProp.intValue == 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if (shipInstanceProp != null)
                {
                    manualCentreOfMassProp = shipInstanceProp.FindPropertyRelative("setCentreOfMassManually");
                    showCOMGizmosInSceneViewProp = shipInstanceProp.FindPropertyRelative("showCOMGizmosInSceneView");

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("initialiseOnAwake"), initialiseOnAwakeContent);
                    if (GUILayout.Button(turnOffAllGizmosContent, buttonCompact, GUILayout.MaxWidth(120f)))
                    {
                        showCOMGizmosInSceneViewProp.boolValue = false;
                        // Centre of thrust
                        shipInstanceProp.FindPropertyRelative("showCOTGizmosInSceneView").boolValue = false;
                        // Centre of lift
                        shipInstanceProp.FindPropertyRelative("showCOLGizmosInSceneView").boolValue = false;

                        serializedObject.ApplyModifiedProperties();
                        // ToggleGizmos checks for null lists and ignores them
                        // Force the gizmos to be turned off.
                        Undo.RecordObject(shipControlModule, "Turn Gizmos Off");
                        ToggleGizmos(shipControlModule.shipInstance.thrusterList, true);
                        ToggleGizmos(shipControlModule.shipInstance.wingList, true);
                        ToggleGizmos(shipControlModule.shipInstance.controlSurfaceList, true);
                        ToggleGizmos(shipControlModule.shipInstance.weaponList, true);
                        ToggleGizmos(shipControlModule.shipInstance.localisedDamageRegionList, true);
                        serializedObject.Update();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.LabelField(physicsModelHeaderContent, helpBoxRichText);

                    EditorGUILayout.PropertyField(shipPhysicsModelProp, shipPhysicsModelContent);

                    EditorGUILayout.LabelField(massHeaderContent, helpBoxRichText);

                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("mass"), massContent);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(manualCentreOfMassProp, manualCentreOfMassContent);
                    // Show Gizmos button
                    if (showCOMGizmosInSceneViewProp.boolValue) { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleToggled, GUILayout.MaxWidth(22f))) { showCOMGizmosInSceneViewProp.boolValue = false; } }
                    else { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleNormal, GUILayout.MaxWidth(22f))) { showCOMGizmosInSceneViewProp.boolValue = true; } }
                    EditorGUILayout.EndHorizontal();

                    centreOfMassProp = shipInstanceProp.FindPropertyRelative("centreOfMass");
                    if (manualCentreOfMassProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(centreOfMassProp, centreOfMassContent);
                        if (GUILayout.Button(resetCentreOfMassButtonContent))
                        {
                            // Reset the centre of mass to what is calculated by colliders
                            shipRigidbody = shipControlModule.GetComponent<Rigidbody>();
                            if (shipRigidbody == null) { Debug.LogWarning("ERROR: The RigidBody is missing on " + shipControlModule.name + ". Cannot reset the Centre of Mass."); }
                            else
                            {
                                shipRigidbody.ResetCenterOfMass();
                                centreOfMassProp.vector3Value = shipRigidbody.centerOfMass;
                            }
                        }
                    }
                    else
                    {
                        // Reset the centre of mass to what is calculated by colliders
                        shipRigidbody = shipControlModule.GetComponent<Rigidbody>();
                        if (shipRigidbody != null)
                        {
                            shipRigidbody.ResetCenterOfMass();
                            centreOfMassProp.vector3Value = shipRigidbody.centerOfMass;
                        }
                    }

                    EditorGUILayout.LabelField(gravityHeaderContent, helpBoxRichText);

                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("gravitationalAcceleration"), gravitationalAccelerationContent);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("gravityDirection"), gravityDirectionContent);

                    if (shipPhysicsModelProp.intValue == (int)Ship.ShipPhysicsModel.Arcade)
                    {
                        EditorGUILayout.LabelField(arcadeMomentAccelerationHeaderContent, helpBoxRichText);

                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("arcadePitchAcceleration"), arcadePitchAccelerationContent);
                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("arcadeRollAcceleration"), arcadeRollAccelerationContent);
                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("arcadeYawAcceleration"), arcadeYawAccelerationContent);

                        EditorGUILayout.LabelField(arcadeTurnAccelerationHeaderContent, helpBoxRichText);

                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("arcadeMaxFlightTurningAcceleration"), arcadeMaxFlightTurnAccContent);
                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("arcadeMaxGroundTurningAcceleration"), arcadeMaxGroundTurnAccContent);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            #endregion

            #region Control

            else if (selectedTabIntProp.intValue == 1)
            {
                // Expand label width from 150 to 165 to cater for Stick To Ground Surface label.
                EditorGUIUtility.labelWidth = 165f;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                #region Flight Assist
                EditorGUILayout.LabelField(rotationalFlightAssistHeaderContent, helpBoxRichText);
                EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("rotationalFlightAssistStrength"), rotationFlightAssistStrengthContent);

                if (shipPhysicsModelProp.intValue == (int)Ship.ShipPhysicsModel.PhysicsBased)
                {
                    EditorGUILayout.LabelField(translationalFlightAssistHeaderContent, helpBoxRichText);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("translationalFlightAssistStrength"), translationalFlightAssistStrengthContent);

                    EditorGUILayout.LabelField(physicsBasedMomentPowerHeaderContent, helpBoxRichText);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("rollPower"), rollPowerContent);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("pitchPower"), pitchPowerContent);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("yawPower"), yawPowerContent);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("steeringThrusterPriorityLevel"), steeringThrusterPriorityLevelContent);
                }

                EditorGUILayout.LabelField(stabilityFlightAssistHeaderContent, helpBoxRichText);
                EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("stabilityFlightAssistStrength"), stabilityFlightAssistStrengthContent);
                #endregion

                #region Brake Flight Assist
                brakeFlightAssistStrengthProp = shipInstanceProp.FindPropertyRelative("brakeFlightAssistStrength");
                brakeFlightAssistMinSpeedProp = shipInstanceProp.FindPropertyRelative("brakeFlightAssistMinSpeed");
                brakeFlightAssistMaxSpeedProp = shipInstanceProp.FindPropertyRelative("brakeFlightAssistMaxSpeed");

                // Check for default setup for backward compatibility pre-v1.1.5
                if (brakeFlightAssistStrengthProp.floatValue == 0f && brakeFlightAssistMinSpeedProp.floatValue == 0f && brakeFlightAssistMaxSpeedProp.floatValue == 0f)
                {
                    brakeFlightAssistMinSpeedProp.floatValue = -10f;
                    brakeFlightAssistMaxSpeedProp.floatValue = 10f;
                }

                EditorGUILayout.LabelField(brakeFlightAssistHeaderContent, helpBoxRichText);
                EditorGUILayout.PropertyField(brakeFlightAssistStrengthProp, brakeFlightAssistStrengthContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(brakeFlightAssistMinSpeedProp, brakeFlightMinSpeedContent);
                if (EditorGUI.EndChangeCheck() && brakeFlightAssistMinSpeedProp.floatValue > brakeFlightAssistMaxSpeedProp.floatValue)
                {
                    brakeFlightAssistMaxSpeedProp.floatValue = brakeFlightAssistMinSpeedProp.floatValue;
                }
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(brakeFlightAssistMaxSpeedProp, brakeFlightMaxSpeedContent);
                if (EditorGUI.EndChangeCheck() && brakeFlightAssistMaxSpeedProp.floatValue < brakeFlightAssistMinSpeedProp.floatValue)
                {
                    brakeFlightAssistMinSpeedProp.floatValue = brakeFlightAssistMaxSpeedProp.floatValue;
                }

                EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("brakeFlightAssistStrengthX"), brakeFlightAssistStrengthXContent);
                EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("brakeFlightAssistStrengthY"), brakeFlightAssistStrengthYContent);

                #endregion

                #region Limit Pitch and Roll
                EditorGUILayout.LabelField(limitPitchAndRollHeaderContent, helpBoxRichText);

                limitPitchAndRollProp = shipInstanceProp.FindPropertyRelative("limitPitchAndRoll");
                EditorGUILayout.PropertyField(limitPitchAndRollProp, limitPitchAndRollContent);

                if (limitPitchAndRollProp.boolValue)
                {
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("maxPitch"), maxPitchContent);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("pitchSpeed"), pitchSpeedContent);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("maxTurnRoll"), maxTurnRollContent);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("turnRollSpeed"), turnRollSpeedContent);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("rollControlMode"), rollControlModeContent);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("pitchRollMatchResponsiveness"), pitchRollMatchResponsivenessContent);

                    #region Avoid or StickTo Ground Surface

                    // Avoid and StickTo cannot both be used at the same time.
                    avoidGroundSurfaceProp = shipInstanceProp.FindPropertyRelative("avoidGroundSurface");
                    stickToGroundSurfaceProp = shipInstanceProp.FindPropertyRelative("stickToGroundSurface");
                    orientUpInAirProp = shipInstanceProp.FindPropertyRelative("orientUpInAir");
                    useGroundMatchSmoothingProp = shipInstanceProp.FindPropertyRelative("useGroundMatchSmoothing");

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(avoidGroundSurfaceProp, avoidGroundSurfaceContent);
                    if (EditorGUI.EndChangeCheck() && avoidGroundSurfaceProp.boolValue && stickToGroundSurfaceProp.boolValue)
                    {
                        stickToGroundSurfaceProp.boolValue = false;
                        orientUpInAirProp.boolValue = false;
                        useGroundMatchSmoothingProp.boolValue = false;
                    }

                    if (avoidGroundSurfaceProp.boolValue) { SSCEditorHelper.InTechPreview(false); }

                    EditorGUILayout.LabelField(stickToGroundSurfaceHeaderContent, helpBoxRichText);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(stickToGroundSurfaceProp, stickToGroundSurfaceContent);
                    if (EditorGUI.EndChangeCheck() && avoidGroundSurfaceProp.boolValue && stickToGroundSurfaceProp.boolValue)
                    {
                        avoidGroundSurfaceProp.boolValue = false;
                    }

                    if (stickToGroundSurfaceProp.boolValue || avoidGroundSurfaceProp.boolValue)
                    {
                        // Find common properties for Avoid or StickTo Ground Surface
                        targetGroundDistanceProp = shipInstanceProp.FindPropertyRelative("targetGroundDistance");
                        maxGroundCheckDistanceProp = shipInstanceProp.FindPropertyRelative("maxGroundCheckDistance");

                        // Stick to Ground Surface
                        if (stickToGroundSurfaceProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(orientUpInAirProp, orientUpInAirContent);                            
                            EditorGUILayout.PropertyField(useGroundMatchSmoothingProp, useGroundMatchSmoothingContent);
                            EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("useGroundMatchLookAhead"), useGroundMatchLookAheadContent);
                            targetGroundDistanceProp.floatValue = EditorGUILayout.Slider(targetGroundDistanceContent, targetGroundDistanceProp.floatValue, 0f, maxGroundCheckDistanceProp.floatValue);
                            
                            if (shipControlModule.shipInstance.shipPhysicsModel == Ship.ShipPhysicsModel.Arcade && useGroundMatchSmoothingProp.boolValue)
                            {
                                minGroundDistanceProp = shipInstanceProp.FindPropertyRelative("minGroundDistance");
                                maxGroundDistanceProp = shipInstanceProp.FindPropertyRelative("maxGroundDistance");
                                minGroundDistanceProp.floatValue = EditorGUILayout.Slider(minGroundDistanceContent, minGroundDistanceProp.floatValue, 0f, targetGroundDistanceProp.floatValue);
                                maxGroundDistanceProp.floatValue = EditorGUILayout.Slider(maxGroundDistanceContent, maxGroundDistanceProp.floatValue, targetGroundDistanceProp.floatValue, maxGroundCheckDistanceProp.floatValue);
                            }
                        }
                        // Avoid Ground Surface
                        else
                        {
                            EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("useGroundMatchLookAhead"), useGroundMatchLookAheadContent);
                            targetGroundDistanceProp.floatValue = EditorGUILayout.Slider(targetGroundDistanceAboveContent, targetGroundDistanceProp.floatValue, 0f, maxGroundCheckDistanceProp.floatValue);
                        }

                        EditorGUILayout.PropertyField(maxGroundCheckDistanceProp, maxGroundCheckDistanceContent);
                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("groundMatchResponsiveness"), groundMatchResponsivenessContent);
                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("groundMatchDamping"), groundMatchDampingContent);

                        if (shipControlModule.shipInstance.shipPhysicsModel == Ship.ShipPhysicsModel.Arcade)
                        {
                            maxGroundMatchAccFactorProp = shipInstanceProp.FindPropertyRelative("maxGroundMatchAccelerationFactor");
                            if (useGroundMatchSmoothingProp.boolValue)
                            {
                                centralMaxGroundMatchAccFactorProp = shipInstanceProp.FindPropertyRelative("centralMaxGroundMatchAccelerationFactor");
                                centralMaxGroundMatchAccFactorProp.floatValue = EditorGUILayout.Slider(centralMaxGroundMatchAccelerationFactorContent, centralMaxGroundMatchAccFactorProp.floatValue, 0f, maxGroundMatchAccFactorProp.floatValue);
                            }
                            EditorGUILayout.PropertyField(maxGroundMatchAccFactorProp, maxGroundMatchAccelerationFactorContent);
                        }

                        // Work with both Avoid and StickToGroundSurface
                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("groundNormalCalculationMode"), groundNormalCalcModeContent);
                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("groundLayerMask"), groundLayerMaskContent);
                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("groundNormalHistoryLength"), groundNormalHistoryLengthContent);

                    }

                    #endregion

                    // TEST PARAMETERS

                    //EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("groundDerivOnMeasurement"), new GUIContent("Ground Deriv On M"));

                    //EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("rotProportional"), new GUIContent("Rot PID P Gain"));
                    //EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("rotIntegral"), new GUIContent("Rot PID I Gain"));
                    //EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("rotDerivative"), new GUIContent("Rot PID D Gain"));

                    //EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("groundProportional"), new GUIContent("Ground PID P Gain"));
                    //EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("groundIntegral"), new GUIContent("Ground PID I Gain"));
                    //EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("groundDerivative"), new GUIContent("Ground PID D Gain"));
                }
                #endregion

                #region Input Control - 2.5D flight
                EditorGUILayout.LabelField(inputControlHeaderContent, helpBoxRichText);
                inputControlAxisProp = shipInstanceProp.FindPropertyRelative("inputControlAxis");

                EditorGUILayout.PropertyField(inputControlAxisProp, inputControlAxisContent);
                if (inputControlAxisProp.intValue > 0)
                {
                    SSCEditorHelper.InTechPreview(false);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("inputControlLimit"), inputControlLimitContent);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("inputControlMovingRigidness"), inputControlMovingRigidnessContent);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("inputControlTurningRigidness"), inputControlTurningRigidnessContent);
                    if (inputControlAxisProp.intValue == 1)
                    {
                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("inputControlForwardAngle"), inputControlForwardAngleContent);
                    }
                }
                #endregion

                EditorGUILayout.EndVertical();

                // Set label width back to default
                EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            }

            #endregion

            #region Thrusters

            else if (selectedTabIntProp.intValue == 2)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if (shipControlModule.shipInstance.shipPhysicsModel == Ship.ShipPhysicsModel.PhysicsBased)
                {
                    EditorGUILayout.LabelField(thrustersPhysicsBasedHeaderContent, helpBoxRichText);
                }
                else
                {
                    EditorGUILayout.LabelField(thrustersArcadeHeaderContent, helpBoxRichText);
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(thrustersCentreOfThrustContent);
                showCOTGizmosInSceneViewProp = shipInstanceProp.FindPropertyRelative("showCOTGizmosInSceneView");
                // Show Gizmos button
                if (showCOTGizmosInSceneViewProp.boolValue) { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleToggled, GUILayout.MaxWidth(22f))) { showCOTGizmosInSceneViewProp.boolValue = false; } }
                else { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleNormal, GUILayout.MaxWidth(22f))) { showCOTGizmosInSceneViewProp.boolValue = true; } }
                EditorGUILayout.EndHorizontal();

                #region Thruster Systems and Fuel

                isThrusterSystemsStartedProp = shipInstanceProp.FindPropertyRelative("isThrusterSystemsStarted");
                useCentralFuelProp = shipInstanceProp.FindPropertyRelative("useCentralFuel");
                centralFuelLevelProp = shipInstanceProp.FindPropertyRelative("centralFuelLevel");

                thrusterIsThrusterSystemsExpandedProp = shipInstanceProp.FindPropertyRelative("isThrusterSystemsExpanded");

                EditorGUI.indentLevel += 1;
                EditorGUIUtility.fieldWidth = 15f;
                EditorGUILayout.BeginHorizontal();
                thrusterIsThrusterSystemsExpandedProp.boolValue = EditorGUILayout.Foldout(thrusterIsThrusterSystemsExpandedProp.boolValue, "", foldoutStyleNoLabel);
                EditorGUI.indentLevel -= 1;
                EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                EditorGUILayout.LabelField(thrusterSystemsContent);
                EditorGUILayout.EndHorizontal();

                if (thrusterIsThrusterSystemsExpandedProp.boolValue)
                {
                    thrusterSystemStartupDurationProp = shipInstanceProp.FindPropertyRelative("thrusterSystemStartupDuration");
                    thrusterSystemShutdownDurationProp = shipInstanceProp.FindPropertyRelative("thrusterSystemShutdownDuration");

                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(isThrusterSystemsStartedProp, thrusterSystemsStartedContent);
                    if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                    {
                        serializedObject.ApplyModifiedProperties();
                        // Flip the value to make sure it works in the editor at runtime
                        isThrusterSystemsStartedProp.boolValue = !isThrusterSystemsStartedProp.boolValue;
                        if (!isThrusterSystemsStartedProp.boolValue)
                        {
                            shipControlModule.StartupThrusterSystems(true);
                        }
                        else
                        {
                            shipControlModule.ShutdownThrusterSystems(true);
                        }
                        serializedObject.Update();
                    }

                    EditorGUILayout.PropertyField(thrusterSystemStartupDurationProp, thrusterSystemStartupDurationContent);
                    EditorGUILayout.PropertyField(thrusterSystemShutdownDurationProp, thrusterSystemShutdownDurationContent);

                    EditorGUILayout.PropertyField(useCentralFuelProp, useCentralFuelContent);
                    if (useCentralFuelProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(centralFuelLevelProp, centralFuelLevelContent);
                    }

                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("isThrusterFXStationary"), isThrusterFXStationaryContent);

                    GUILayout.EndVertical();
                }
                #endregion

                // Apply property changes before potential list changes
                serializedObject.ApplyModifiedProperties();

                #region Initialise lists

                thrusterEffectsList.Clear();
                if (shipControlModule.thrusterEffectObjects != null) { thrusterEffectsList.AddRange(shipControlModule.thrusterEffectObjects); }

                // Make sure the two lists are the same size
                bool hasListChanged = false;
                while (thrusterEffectsList.Count < thrustersList.Count) { thrusterEffectsList.Add(null); hasListChanged = true; }
                while (thrusterEffectsList.Count > thrustersList.Count) { thrusterEffectsList.RemoveAt(thrusterEffectsList.Count - 1); hasListChanged = true; }
                if (hasListChanged)
                {
                    shipControlModule.thrusterEffectObjects = thrusterEffectsList.ToArray();
                }

                #endregion

                int tInt = thrustersList.Count;

                #region Allow users to copy, paste, add and remove thrusters
                bool isAddThruster = false;
                bool isRemoveLastThruster = false;
                GUILayout.BeginHorizontal();

                EditorGUI.indentLevel += 1;
                EditorGUIUtility.fieldWidth = 15f;
                // Read in properties
                serializedObject.Update();
                thrusterIsThrusterListExpandedProp = shipInstanceProp.FindPropertyRelative("isThrusterListExpanded");
                EditorGUI.BeginChangeCheck();
                thrusterIsThrusterListExpandedProp.boolValue = EditorGUILayout.Foldout(thrusterIsThrusterListExpandedProp.boolValue, "", foldoutStyleNoLabel);            
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(shipControlModule.shipInstance.thrusterList, thrusterIsThrusterListExpandedProp.boolValue);
                    serializedObject.Update();
                }
                EditorGUI.indentLevel -= 1;
                EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                EditorGUILayout.LabelField("<color=" + txtColourName + "><b>Thrusters: " + numFilteredThrusters + " of " + tInt + "</b></color>", labelFieldRichText);
                if (GUILayout.Button(thrusterCopyListContent, GUILayout.Width(25f))) { CopyThrusterList(); }
                if (GUILayout.Button(thrusterPasteListContent, GUILayout.Width(25f)))
                {
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(shipControlModule, "Paste Thruster List");
                    PasteThrusterList();
                    isSceneModified = true;
                    serializedObject.Update();
                    
                }
                if (tInt > 0)
                {
                    if (GUILayout.Button(gizmoToggleBtnContent, GUILayout.MaxWidth(22f)))
                    {
                        serializedObject.ApplyModifiedProperties();
                        Undo.RecordObject(shipControlModule, "Toggle Thruster Gizmos");
                        ToggleGizmos(shipControlModule.shipInstance.thrusterList);
                        isSceneModified = true;
                        serializedObject.Update();
                    }
                }
                if (GUILayout.Button("+", GUILayout.Width(25f))) { isAddThruster = true; }
                if (GUILayout.Button("-", GUILayout.Width(25f)) && thrustersList.Count > 0) { isRemoveLastThruster = true; }
                GUILayout.EndHorizontal();
                #endregion

                SSCEditorHelper.DrawSearchFilterControl(filterContent, labelFieldRichText, searchTextFieldStyle, searchCancelButtonStyle, ref shipControlModule.editorSearchThrusterFilter);

                #region Thrusters List

                thrustersProp = shipInstanceProp.FindPropertyRelative("thrusterList");

                // Reset button variables
                thrusterMoveDownPos = -1;
                thrusterInsertPos = -1;
                thrusterDeletePos = -1;
                numFilteredThrusters = 0;

                int numThrusters = thrustersProp.arraySize;

                for (index = 0; index < numThrusters; index++)
                {
                    // Get the thruster from the array of thrusters
                    thrusterProp = thrustersProp.GetArrayElementAtIndex(index);

                    thrusterShowInEditorProp = thrusterProp.FindPropertyRelative("showInEditor");
                    thrusterShowGizmosInSceneViewProp = thrusterProp.FindPropertyRelative("showGizmosInSceneView");
                    thrusterNameProp = thrusterProp.FindPropertyRelative("name");

                    // Check if the list of Thrusters is being filtered. Ignore Locations not in the filter
                    if (!SSCEditorHelper.IsInSearchFilter(thrusterNameProp, shipControlModule.editorSearchThrusterFilter)) { continue; }
                    numFilteredThrusters++;

                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    #region Thruster Find/Move/Insert/Delete buttons
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel += 1;
                    thrusterShowInEditorProp.boolValue = EditorGUILayout.Foldout(thrusterShowInEditorProp.boolValue, (index + 1).ToString("00") + ": " + thrusterNameProp.stringValue);
                    EditorGUI.indentLevel -= 1;
                    // Find (select) in the scene
                    SelectItemInSceneViewButton(shipControlModule.shipInstance.thrusterList, thrusterShowInEditorProp, thrusterProp.FindPropertyRelative("selectedInSceneView"), thrusterShowGizmosInSceneViewProp);
                    // Show Gizmos button
                    if (thrusterShowGizmosInSceneViewProp.boolValue) { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleToggled, GUILayout.MaxWidth(22f))) { thrusterShowGizmosInSceneViewProp.boolValue = false; } }
                    else { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleNormal, GUILayout.MaxWidth(22f))) { thrusterShowGizmosInSceneViewProp.boolValue = true; } }
                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numThrusters > 1) { thrusterMoveDownPos = index; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { thrusterInsertPos = index; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { thrusterDeletePos = index; }

                    GUILayout.EndHorizontal();
                    #endregion

                    #region Thruster properties
                    if (thrusterShowInEditorProp.boolValue)
                    {
                        // Get the properties for this array item (thruster)
                        forceUseProp = thrusterProp.FindPropertyRelative("forceUse");
                        primaryMomentUseProp = thrusterProp.FindPropertyRelative("primaryMomentUse");
                        secondaryMomentUseProp = thrusterProp.FindPropertyRelative("secondaryMomentUse");
                        thrusterMaxThrustProp = thrusterProp.FindPropertyRelative("maxThrust");
                        thrusterThrottleProp = thrusterProp.FindPropertyRelative("throttle");
                        thrusterRelativePositionProp = thrusterProp.FindPropertyRelative("relativePosition");
                        thrusterThrustDirectionProp = thrusterProp.FindPropertyRelative("thrustDirection");
                        thrusterDamageRegionIndexProp = thrusterProp.FindPropertyRelative("damageRegionIndex");
                        thrusterLimitEffectsOnZProp = thrusterProp.FindPropertyRelative("limitEffectsOnZ");
                        thrusterMinEffectsOnZProp = thrusterProp.FindPropertyRelative("minEffectsOnZ");
                        thrusterMaxEffectsOnZProp = thrusterProp.FindPropertyRelative("maxEffectsOnZ");
                        thrusterLimitEffectsOnYProp = thrusterProp.FindPropertyRelative("limitEffectsOnY");
                        thrusterMinEffectsOnYProp = thrusterProp.FindPropertyRelative("minEffectsOnY");
                        thrusterMaxEffectsOnYProp = thrusterProp.FindPropertyRelative("maxEffectsOnY");
                        thrusterFuelLevelProp = thrusterProp.FindPropertyRelative("fuelLevel");
                        thrusterHeatLevelProp = thrusterProp.FindPropertyRelative("heatLevel");

                        // Get the property of this EffectsObject (GameObject) from the EffectObjects array
                        thrusterEffectObjProp = thrusterEffectObjectsProp.GetArrayElementAtIndex(index);

                        EditorGUILayout.PropertyField(thrusterNameProp, thrusterNameContent);
                        thrusterMaxThrustProp.floatValue = EditorGUILayout.FloatField(thrusterMaxThrustContent, thrusterMaxThrustProp.floatValue / 1000f) * 1000f;
                        EditorGUILayout.PropertyField(thrusterThrottleProp, thrusterThrottleContent);
                        if (shipPhysicsModelProp.intValue == (int)Ship.ShipPhysicsModel.PhysicsBased)
                        {
                            EditorGUILayout.PropertyField(thrusterRelativePositionProp, thrusterRelativePositionContent);
                        }
                        EditorGUILayout.PropertyField(thrusterThrustDirectionProp, thrusterThrustDirectionContent);
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(thrusterForceUseContent, GUILayout.Width(defaultEditorLabelWidth - 5f));
                        forceUseProp.intValue = EditorGUILayout.IntPopup(forceUseProp.intValue, thrusterForceUseStrings, thrusterForceUseInts);
                        GUILayout.EndHorizontal();
                        if (shipPhysicsModelProp.intValue == (int)Ship.ShipPhysicsModel.PhysicsBased)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(thrusterPrimaryMomentUseContent, GUILayout.Width(defaultEditorLabelWidth - 5f));
                            primaryMomentUseProp.intValue = EditorGUILayout.IntPopup(primaryMomentUseProp.intValue, thrusterMomentUseStrings, thrusterMomentUseInts);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(thrusterSecondaryMomentUseContent, GUILayout.Width(defaultEditorLabelWidth - 5f));
                            secondaryMomentUseProp.intValue = EditorGUILayout.IntPopup(secondaryMomentUseProp.intValue, thrusterMomentUseStrings, thrusterMomentUseInts);
                            GUILayout.EndHorizontal();
                        }

                        bool useProgressiveDamage = thrusterDamageRegionIndexProp.intValue > -1 && shipDamageModelProp.intValue != (int)Ship.ShipDamageModel.Simple;
                        if (shipDamageModelProp.intValue == (int)Ship.ShipDamageModel.Progressive)
                        {
                            useProgressiveDamage = EditorGUILayout.Toggle(partProgressiveDamageContent, useProgressiveDamage);
                            if (useProgressiveDamage) { thrusterDamageRegionIndexProp.intValue = 0; }
                            else { thrusterDamageRegionIndexProp.intValue = -1; }
                        }
                        else if (shipDamageModelProp.intValue == (int)Ship.ShipDamageModel.Localised)
                        {
                            thrusterDamageRegionIndexProp.intValue = EditorGUILayout.IntPopup(partDamageRegionContent, thrusterDamageRegionIndexProp.intValue, damageRegionIndexContents, damageRegionIndexInts);
                        }

                        if (useProgressiveDamage)
                        {
                            EditorGUILayout.PropertyField(thrusterProp.FindPropertyRelative("minPerformance"), thrusterMinPerformanceContent);
                            EditorGUILayout.PropertyField(thrusterProp.FindPropertyRelative("startingHealth"), partStartingHealthContent);
                        }

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(thrusterEffectsObjectContent, GUILayout.Width(defaultEditorLabelWidth - 60f));
                        if (GUILayout.Button(thrusterAlignBtnContent, toggleCompactButtonStyleNormal, GUILayout.MaxWidth(50f)))
                        {
                            if (thrusterEffectObjProp.objectReferenceValue != null)
                            {
                                // Position the thruster effect to match the thruster relative position and direction
                                Transform thrusterEffectsTransform = ((GameObject)thrusterEffectObjProp.objectReferenceValue).transform;

                                thrusterEffectsTransform.localPosition = thrusterRelativePositionProp.vector3Value;
                                thrusterEffectsTransform.localRotation = Quaternion.LookRotation(thrusterThrustDirectionProp.vector3Value * -1f, Vector3.up);
                            }
                        }
                        thrusterEffectObjProp.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField(thrusterEffectObjProp.objectReferenceValue, typeof(GameObject), true);
                        EditorGUILayout.EndHorizontal();
                        if (thrusterEffectObjProp.objectReferenceValue != null)
                        {
                            EditorGUILayout.PropertyField(thrusterProp.FindPropertyRelative("minEffectsRate"), thrustersMinEffectsRateContent);
                            EditorGUILayout.PropertyField(thrusterProp.FindPropertyRelative("isThrottleMinEffects"), isThrottleMinEffectsContent);
                            EditorGUILayout.PropertyField(thrusterProp.FindPropertyRelative("isMinEffectsAlwaysOn"), isMinEffectsAlwaysOnContent);

                            EditorGUILayout.PropertyField(thrusterLimitEffectsOnYProp, thrusterLimitEffectsOnYContent);
                            if (thrusterLimitEffectsOnYProp.boolValue)
                            {
                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(thrusterMinEffectsOnYProp, thrusterMinEffectsOnYContent);
                                if (EditorGUI.EndChangeCheck() && thrusterMinEffectsOnYProp.floatValue > thrusterMaxEffectsOnYProp.floatValue)
                                {
                                    thrusterMaxEffectsOnYProp.floatValue = thrusterMinEffectsOnYProp.floatValue;
                                }
                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(thrusterMaxEffectsOnYProp, thrusterMaxEffectsOnYContent);
                                if (EditorGUI.EndChangeCheck() && thrusterMinEffectsOnYProp.floatValue > thrusterMaxEffectsOnYProp.floatValue)
                                {
                                    thrusterMinEffectsOnYProp.floatValue = thrusterMaxEffectsOnYProp.floatValue;
                                }
                            }

                            EditorGUILayout.PropertyField(thrusterLimitEffectsOnZProp, thrusterLimitEffectsOnZContent);
                            if (thrusterLimitEffectsOnZProp.boolValue)
                            {
                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(thrusterMinEffectsOnZProp, thrusterMinEffectsOnZContent);
                                if (EditorGUI.EndChangeCheck() && thrusterMinEffectsOnZProp.floatValue > thrusterMaxEffectsOnZProp.floatValue)
                                {
                                    thrusterMaxEffectsOnZProp.floatValue = thrusterMinEffectsOnZProp.floatValue;
                                }
                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(thrusterMaxEffectsOnZProp, thrusterMaxEffectsOnZContent);
                                if (EditorGUI.EndChangeCheck() && thrusterMinEffectsOnZProp.floatValue > thrusterMaxEffectsOnZProp.floatValue)
                                {
                                    thrusterMinEffectsOnZProp.floatValue = thrusterMaxEffectsOnZProp.floatValue;
                                }
                            }
                        }
                        EditorGUILayout.PropertyField(thrusterProp.FindPropertyRelative("rampUpDuration"), thrusterRampUpDurationContent);
                        EditorGUILayout.PropertyField(thrusterProp.FindPropertyRelative("rampDownDuration"), thrusterRampDownDurationContent);

                        // When using a central fuel level for the whole ship, it individual thrusters draw from the same fuel level
                        if (!useCentralFuelProp.boolValue)
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(thrusterFuelLevelProp, thrusterFuelLevelContent);
                            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                            {
                                thrustersList[index].SetFuelLevel(thrusterFuelLevelProp.floatValue);
                            }
                        }
                        EditorGUILayout.PropertyField(thrusterProp.FindPropertyRelative("fuelBurnRate"), thrusterFuelBurnRateContent);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(thrusterHeatLevelProp, thrusterHeatLevelContent);
                        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                        {
                            thrustersList[index].SetHeatLevel(thrusterHeatLevelProp.floatValue);
                        }
                        EditorGUILayout.PropertyField(thrusterProp.FindPropertyRelative("heatUpRate"), thrusterHeatUpRateContent);
                        EditorGUILayout.PropertyField(thrusterProp.FindPropertyRelative("heatDownRate"), thrusterHeatDownRateContent);
                        EditorGUILayout.PropertyField(thrusterProp.FindPropertyRelative("overHeatThreshold"), thrusterOverHeatThresholdContent);
                        EditorGUILayout.PropertyField(thrusterProp.FindPropertyRelative("isBurnoutOnMaxHeat"), thrusterIsBurnoutOnMaxHeatContent);
                    }
                    #endregion

                    GUILayout.EndVertical();
                }
                #endregion

                #region Auto-Populate Forces and Moments

                if (GUILayout.Button(thrustersAutoPopulateForcesMomentsButtonContent))//, GUILayout.Width(220f)))
                {
                    // Get the centre of mass property
                    centreOfMassProp = shipInstanceProp.FindPropertyRelative("centreOfMass");

                    for (index = 0; index < numThrusters; index++)
                    {
                        // Get the thruster from the array of thrusters
                        thrusterProp = thrustersProp.GetArrayElementAtIndex(index);

                        // Get the properties for this array item (thruster)
                        thrusterThrustDirectionProp = thrusterProp.FindPropertyRelative("thrustDirection");
                        forceUseProp = thrusterProp.FindPropertyRelative("forceUse");
                        primaryMomentUseProp = thrusterProp.FindPropertyRelative("primaryMomentUse");
                        secondaryMomentUseProp = thrusterProp.FindPropertyRelative("secondaryMomentUse");
                        thrusterRelativePositionProp = thrusterProp.FindPropertyRelative("relativePosition");

                        // First, calculate the primary direction of thrust, and use this to assign the force use
                        // Determine which axis (x, y, z) has the greatest magnitude,
                        // then determine which direction thrust occurs along this axis
                        if (Mathf.Abs(thrusterThrustDirectionProp.vector3Value.x) > Mathf.Abs(thrusterThrustDirectionProp.vector3Value.y) &&
                            Mathf.Abs(thrusterThrustDirectionProp.vector3Value.x) > Mathf.Abs(thrusterThrustDirectionProp.vector3Value.z))
                        {
                            // X-axis has greatest magnitude
                            // Positive x-axis: rightward thrust
                            if (thrusterThrustDirectionProp.vector3Value.x >= 0) { forceUseProp.intValue = 5; }
                            // Negative x-axis: leftward thrust
                            else { forceUseProp.intValue = 6; }
                        }
                        else if (Mathf.Abs(thrusterThrustDirectionProp.vector3Value.y) > Mathf.Abs(thrusterThrustDirectionProp.vector3Value.z))
                        {
                            // Y-axis has greatest magnitude
                            // Positive y-axis: upward thrust
                            if (thrusterThrustDirectionProp.vector3Value.y >= 0) { forceUseProp.intValue = 3; }
                            // Negative y-axis: downward thrust
                            else { forceUseProp.intValue = 4; }
                        }
                        else if (Mathf.Abs(thrusterThrustDirectionProp.vector3Value.z) > Mathf.Abs(thrusterThrustDirectionProp.vector3Value.y))
                        {
                            // Z-axis has greatest magnitude
                            // Positive z-axis: forward thrust
                            if (thrusterThrustDirectionProp.vector3Value.z >= 0) { forceUseProp.intValue = 1; }
                            // Negative z-axis: backward thrust
                            else { forceUseProp.intValue = 2; }
                        }
                        else
                        {
                            // No axis has the greatest magnitude: none
                            forceUseProp.intValue = 0;
                        }

                        // Next, calculate the moment applied by the thrust direction
                        Vector3 momentAppliedByThruster = Vector3.Cross(thrusterRelativePositionProp.vector3Value - centreOfMassProp.vector3Value,
                            thrusterThrustDirectionProp.vector3Value);

                        // Calculate the primary direction of the moment, and use this to assign the primary moment use
                        // Determine which axis (x, y, z) has the greatest magnitude,
                        // then determine which direction the moment occurs on this axis
                        // Then remove this component from the moment and repeat this process again to find the secondary moment use
                        int thisMomentUse = 0;
                        for (int mu = 0; mu < 2; mu++)
                        {
                            if (Mathf.Abs(momentAppliedByThruster.x) > Mathf.Abs(momentAppliedByThruster.y) &&
                                Mathf.Abs(momentAppliedByThruster.x) > Mathf.Abs(momentAppliedByThruster.z))
                            {
                                // X-axis has greatest magnitude
                                // Positive x-axis: positive pitch
                                if (momentAppliedByThruster.x >= 0) { thisMomentUse = 3; }
                                // Negative x-axis: negative pitch
                                else { thisMomentUse = 4; }
                            }
                            else if (Mathf.Abs(momentAppliedByThruster.y) > Mathf.Abs(momentAppliedByThruster.z))
                            {
                                // Y-axis has greatest magnitude
                                // Positive y-axis: positive yaw
                                if (momentAppliedByThruster.y >= 0) { thisMomentUse = 5; }
                                // Negative y-axis: negative yaw
                                else { thisMomentUse = 6; }
                            }
                            else if (Mathf.Abs(momentAppliedByThruster.z) > Mathf.Abs(momentAppliedByThruster.y))
                            {
                                // Z-axis has greatest magnitude
                                // Positive z-axis: negative roll
                                if (momentAppliedByThruster.z >= 0) { thisMomentUse = 2; }
                                // Negative y-axis: positive roll
                                else { thisMomentUse = 1; }
                            }
                            else
                            {
                                // No axis has the greatest magnitude: none
                                thisMomentUse = 0;
                            }

                            if (mu == 0)
                            {
                                // The first time through the loop we want to assign the primary moment use
                                primaryMomentUseProp.intValue = thisMomentUse;
                                // Then remove this component from the moment, so we don't detect it on the next pass
                                if (thisMomentUse == 1 || thisMomentUse == 2) { momentAppliedByThruster.z = 0f; }
                                else if (thisMomentUse == 3 || thisMomentUse == 4) { momentAppliedByThruster.x = 0f; }
                                else if (thisMomentUse == 5 || thisMomentUse == 6) { momentAppliedByThruster.y = 0f; }
                                // If the primary moment use is none, the secondary moment use should also be none
                                else { secondaryMomentUseProp.intValue = 0; mu = 2; }
                            }
                            else
                            {
                                // The second time through the loop we want to assign the secondary moment use
                                secondaryMomentUseProp.intValue = thisMomentUse;
                            }
                        }
                    }
                }

                #endregion

                // Apply property changes before potential list changes
                serializedObject.ApplyModifiedProperties();

                #region Add/Remove/Insert

                // If we need to change the arrays, first need to get the latest copy
                if (isAddThruster || isRemoveLastThruster || thrusterDeletePos >= 0 || thrusterInsertPos >= 0 || thrusterMoveDownPos >= 0)
                {
                    GUI.FocusControl(null);
                    Undo.RecordObject(shipControlModule, "Modify Thruster List");

                    thrusterEffectsList.Clear();
                    if (shipControlModule.thrusterEffectObjects != null) { thrusterEffectsList.AddRange(shipControlModule.thrusterEffectObjects); }

                    // Don't permit multiple operations in the same pass
                    if (isAddThruster)
                    {
                        #region Arcade Add Thruster Direction

                        Vector3 newThrusterDirection = Vector3.forward;
                        int newThrusterForceUse = 1;

                        // When adding a new thruster in arcade mode, try to add it facing a different direction to existing thrusters
                        if (shipPhysicsModelProp.intValue == (int)Ship.ShipPhysicsModel.Arcade)
                        {
                            if (!thrustersList.Exists(t => t.thrustDirection.normalized == Vector3.forward))
                            {
                                newThrusterDirection = Vector3.forward;
                                newThrusterForceUse = 1;
                            }
                            else if (!thrustersList.Exists(t => t.thrustDirection.normalized == Vector3.back))
                            {
                                newThrusterDirection = Vector3.back;
                                newThrusterForceUse = 2;
                            }
                            else if (!thrustersList.Exists(t => t.thrustDirection.normalized == Vector3.up))
                            {
                                newThrusterDirection = Vector3.up;
                                newThrusterForceUse = 3;
                            }
                            else if (!thrustersList.Exists(t => t.thrustDirection.normalized == Vector3.down))
                            {
                                newThrusterDirection = Vector3.down;
                                newThrusterForceUse = 4;
                            }
                            else if (!thrustersList.Exists(t => t.thrustDirection.normalized == Vector3.right))
                            {
                                newThrusterDirection = Vector3.right;
                                newThrusterForceUse = 5;
                            }
                            else if (!thrustersList.Exists(t => t.thrustDirection.normalized == Vector3.left))
                            {
                                newThrusterDirection = Vector3.left;
                                newThrusterForceUse = 6;
                            }
                        }

                        #endregion

                        thrustersList.Add(new Thruster());
                        thrusterEffectsList.Add(null);
                        isAddThruster = false;

                        // Arcade mode: assign calculated thruster direction / force use
                        if (shipPhysicsModelProp.intValue == (int)Ship.ShipPhysicsModel.Arcade)
                        {
                            thrustersList[thrustersList.Count - 1].thrustDirection = newThrusterDirection;
                            thrustersList[thrustersList.Count - 1].forceUse = newThrusterForceUse;
                        }
                    }
                    else if (isRemoveLastThruster)
                    {
                        if (EditorUtility.DisplayDialog("Delete " + thrustersList[thrustersList.Count - 1].name + "?", thrustersList[thrustersList.Count - 1].name + " will be deleted\n\nThis action will remove the thruster from the list and cannot be undone.", "Delete Now", "Cancel"))
                        {
                            thrustersList.RemoveAt(thrustersList.Count - 1);
                            thrusterEffectsList.RemoveAt(thrusterEffectsList.Count - 1);
                            isRemoveLastThruster = false;
                        }
                    }
                    else if (thrusterMoveDownPos >= 0)
                    {
                        if (thrusterMoveDownPos > thrustersList.Count - 2)
                        {
                            thrustersList.Insert(0, new Thruster(thrustersList[thrusterMoveDownPos]));
                            thrustersList.RemoveAt(thrusterMoveDownPos + 1);
                            thrusterEffectsList.Insert(0, thrusterEffectsList[thrusterMoveDownPos]);
                            thrusterEffectsList.RemoveAt(thrusterMoveDownPos + 1);
                        }
                        else
                        {
                            thrustersList.Insert(thrusterMoveDownPos + 2, new Thruster(thrustersList[thrusterMoveDownPos]));
                            thrustersList.RemoveAt(thrusterMoveDownPos);
                            thrusterEffectsList.Insert(thrusterMoveDownPos + 2, thrusterEffectsList[thrusterMoveDownPos]);
                            thrusterEffectsList.RemoveAt(thrusterMoveDownPos);
                        }
                        thrusterMoveDownPos = -1;
                    }
                    else if (thrusterInsertPos >= 0)
                    {
                        thrustersList.Insert(thrusterInsertPos, new Thruster(thrustersList[thrusterInsertPos]));

                        thrusterEffectsList.Insert(thrusterInsertPos, thrusterEffectsList[thrusterInsertPos]);
                        thrusterInsertPos = -1;
                    }
                    else if (thrusterDeletePos >= 0)
                    {
                        // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and DeletePos is reset to -1.
                        int _deleteIndex = thrusterDeletePos;
                        if (EditorUtility.DisplayDialog("Delete " + thrustersList[thrusterDeletePos].name + "?", thrustersList[thrusterDeletePos].name + " will be deleted\n\nThis action will remove the thruster from the list and cannot be undone.", "Delete Now", "Cancel"))
                        {
                            thrustersList.RemoveAt(_deleteIndex);
                            thrusterEffectsList.RemoveAt(_deleteIndex);
                            thrusterDeletePos = -1;
                        }
                    }

                    // Convert the lists back to arrays and save them back to the ship control module class
                    shipControlModule.thrusterEffectObjects = thrusterEffectsList.ToArray();

                    #if UNITY_2019_3_OR_NEWER
                    // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                    if (!Application.isPlaying)
                    {
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                    GUIUtility.ExitGUI();
                    #endif

                    //Undo.RecordObject(shipControlModule, "Modify Thruster List");
                    isSceneModified = true;
                }

                #endregion

                // Read in the properties as the list may have changed
                serializedObject.Update();

                EditorGUILayout.EndVertical();
            }

            #endregion

            #region Aerodynamics

            else if (selectedTabIntProp.intValue == 3)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField(atmosphericPropertiesHeaderContent, helpBoxRichText);

                EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("mediumDensity"), mediumDensityContent);

                // Get properties
                dragReferenceAreasProp = shipInstanceProp.FindPropertyRelative("dragReferenceAreas");
                disableDragMomentsProp = shipInstanceProp.FindPropertyRelative("disableDragMoments");
                centreOfDragXMomentProp = shipInstanceProp.FindPropertyRelative("centreOfDragXMoment");
                centreOfDragYMomentProp = shipInstanceProp.FindPropertyRelative("centreOfDragYMoment");
                centreOfDragZMomentProp = shipInstanceProp.FindPropertyRelative("centreOfDragZMoment");

                EditorGUILayout.LabelField(dragPropertiesHeaderContent, helpBoxRichText);

                #region Calculate Drag Properties

                if (GUILayout.Button(calculateDragPropertiesButtonContent))
                {
                    // Set rotation to none
                    Quaternion originalShipRotation = shipControlModule.transform.rotation;
                    shipControlModule.transform.rotation = Quaternion.identity;

                    // Find all of the ship's mesh renderers
                    MeshRenderer[] allShipMeshRenderers = shipControlModule.GetComponentsInChildren<MeshRenderer>();

                    if (allShipMeshRenderers.Length > 0)
                    {
                        // Get the bounds of the ship according to renderers
                        Bounds shipBounds = allShipMeshRenderers[0].bounds;
                        int ci;
                        for (ci = 1; ci < allShipMeshRenderers.Length; ci++)
                        {
                            shipBounds.Encapsulate(allShipMeshRenderers[ci].bounds);
                        }

                        // Disable all original colliders and use a series of mesh colliders instead
                        Collider[] originalShipColliders = shipControlModule.GetComponentsInChildren<Collider>();
                        for (ci = 0; ci < originalShipColliders.Length; ci++)
                        {
                            originalShipColliders[ci].enabled = false;
                        }
                        MeshFilter[] shipMeshFilters = shipControlModule.GetComponentsInChildren<MeshFilter>();
                        // TODO: what if this is zero?
                        GameObject[] addedMeshColliderGameObjects = new GameObject[shipMeshFilters.Length];
                        for (ci = 0; ci < shipMeshFilters.Length; ci++)
                        {
                            addedMeshColliderGameObjects[ci] = new GameObject("Temp Mesh Collider");
                            addedMeshColliderGameObjects[ci].transform.position = shipMeshFilters[ci].transform.position;
                            addedMeshColliderGameObjects[ci].transform.localScale = shipMeshFilters[ci].transform.lossyScale;
                            addedMeshColliderGameObjects[ci].transform.rotation = shipMeshFilters[ci].transform.rotation;
                            MeshCollider mc = addedMeshColliderGameObjects[ci].AddComponent<MeshCollider>();
                            mc.sharedMesh = shipMeshFilters[ci].sharedMesh;
                        }

                        // Perform raycasts based on calculated ship bounds and hence calculate drag reference areas
                        float raycastSpacingX = shipBounds.size.x / 1000f;
                        float raycastSpacingY = shipBounds.size.y / 1000f;
                        float raycastSpacingZ = shipBounds.size.z / 1000f;
                        float raycastLength, xPos, yPos, zPos;
                        Vector3 startPos = Vector3.zero;
                        int raycastHitCount;

                        Vector3 dragReferenceAreasVector = Vector3.zero;

                        Vector3 worldSpaceShipPos = shipControlModule.transform.position;
                        Vector3 dragPosXSum = Vector3.zero, dragPosYSum = Vector3.zero, dragPosZSum = Vector3.zero;

                        // X-axis raycasts
                        raycastHitCount = 0;
                        raycastLength = shipBounds.size.x + 2f;
                        xPos = shipBounds.min.x - 1f;
                        for (yPos = shipBounds.min.y; yPos <= shipBounds.max.y; yPos += raycastSpacingY)
                        {
                            for (zPos = shipBounds.min.z; zPos <= shipBounds.max.z; zPos += raycastSpacingZ)
                            {
                                startPos.x = xPos;
                                startPos.y = yPos;
                                startPos.z = zPos;

                                if (Physics.Raycast(startPos, Vector3.right, raycastLength))
                                {
                                    raycastHitCount++;
                                    // Add weighted values to calculate the centre(s) of drag
                                    dragPosZSum.y += (yPos - worldSpaceShipPos.y) * raycastSpacingY * raycastSpacingZ;
                                    dragPosYSum.z += (zPos - worldSpaceShipPos.z) * raycastSpacingY * raycastSpacingZ;
                                }
                            }
                        }
                        dragReferenceAreasVector.x = raycastHitCount * raycastSpacingY * raycastSpacingZ;

                        // Y-axis raycasts
                        raycastHitCount = 0;
                        raycastLength = shipBounds.size.y + 2f;
                        yPos = shipBounds.min.y - 1f;
                        for (xPos = shipBounds.min.x; xPos <= shipBounds.max.x; xPos += raycastSpacingX)
                        {
                            for (zPos = shipBounds.min.z; zPos <= shipBounds.max.z; zPos += raycastSpacingZ)
                            {
                                startPos.x = xPos;
                                startPos.y = yPos;
                                startPos.z = zPos;

                                if (Physics.Raycast(startPos, Vector3.up, raycastLength))
                                {
                                    raycastHitCount++;
                                    // Add weighted values to calculate the centre(s) of drag
                                    dragPosZSum.x += (xPos - worldSpaceShipPos.x) * raycastSpacingX * raycastSpacingZ;
                                    dragPosXSum.z += (zPos - worldSpaceShipPos.z) * raycastSpacingX * raycastSpacingZ;
                                }
                            }
                        }
                        dragReferenceAreasVector.y = raycastHitCount * raycastSpacingX * raycastSpacingZ;

                        // Z-axis raycasts
                        raycastHitCount = 0;
                        raycastLength = shipBounds.size.z + 2f;
                        zPos = shipBounds.min.z - 1f;
                        for (xPos = shipBounds.min.x; xPos <= shipBounds.max.x; xPos += raycastSpacingX)
                        {
                            for (yPos = shipBounds.min.y; yPos <= shipBounds.max.y; yPos += raycastSpacingY)
                            {
                                startPos.x = xPos;
                                startPos.y = yPos;
                                startPos.z = zPos;

                                if (Physics.Raycast(startPos, Vector3.forward, raycastLength))
                                {
                                    raycastHitCount++;
                                    // Add weighted values to calculate the centre(s) of drag
                                    dragPosYSum.x += (xPos - worldSpaceShipPos.x) * raycastSpacingX * raycastSpacingY;
                                    dragPosXSum.y += (yPos - worldSpaceShipPos.y) * raycastSpacingX * raycastSpacingY;
                                }
                            }
                        }
                        dragReferenceAreasVector.z = raycastHitCount * raycastSpacingX * raycastSpacingY;

                        // Check that we found some mass on each axis
                        // If we did not, log a warning and set a "default" value of 1 to make sure no
                        // errors occur when simulating drag etc.
                        if (dragReferenceAreasVector.x == 0f)
                        {
                            dragReferenceAreasVector.x = 1f;
                            Debug.LogWarning("WARNING: No area found for this ship on the x-axis, so a default value of " +
                                "one square metre was set. Have you added a valid mesh for this ship (this is required to determine" +
                                "the drag properties)?");
                        }
                        if (dragReferenceAreasVector.y == 0f)
                        {
                            dragReferenceAreasVector.y = 1f;
                            Debug.LogWarning("WARNING: No area found for this ship on the y-axis, so a default value of " +
                                "one square metre was set. Have you added a valid mesh for this ship (this is required to determine" +
                                "the drag properties)?");
                        }
                        if (dragReferenceAreasVector.z == 0f)
                        {
                            dragReferenceAreasVector.z = 1f;
                            Debug.LogWarning("WARNING: No area found for this ship on the z-axis, so a default value of " +
                                "one square metre was set. Have you added a valid mesh for this ship (this is required to determine" +
                                "the drag properties)?");
                        }

                        // Calculate centre of drag (divide sum of radius * area by cumulative area summed over)
                        dragPosXSum.x = shipControlModule.shipInstance.centreOfMass.x;
                        dragPosXSum.y /= dragReferenceAreasVector.z;
                        dragPosXSum.z /= dragReferenceAreasVector.y;
                        dragPosYSum.x /= dragReferenceAreasVector.z;
                        dragPosYSum.y = shipControlModule.shipInstance.centreOfMass.y;
                        dragPosYSum.z /= dragReferenceAreasVector.x;
                        dragPosZSum.x /= dragReferenceAreasVector.y;
                        dragPosZSum.y /= dragReferenceAreasVector.x;
                        dragPosZSum.z = shipControlModule.shipInstance.centreOfMass.z;

                        // Set properties
                        dragReferenceAreasProp.vector3Value = dragReferenceAreasVector;
                        centreOfDragXMomentProp.vector3Value = dragPosXSum;
                        centreOfDragYMomentProp.vector3Value = dragPosYSum;
                        centreOfDragZMomentProp.vector3Value = dragPosZSum;

                        // TODO: What if the collider was originally disabled?
                        // Re-enable original colliders and remove added mesh colliders
                        for (ci = 0; ci < originalShipColliders.Length; ci++)
                        {
                            originalShipColliders[ci].enabled = true;
                        }
                        for (ci = 0; ci < addedMeshColliderGameObjects.Length; ci++)
                        {
                            DestroyImmediate(addedMeshColliderGameObjects[ci]);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("WARNING: No mesh renderers found for this ship, so no drag properties could be determined.");
                    }

                    // Set rotation back to original
                    shipControlModule.transform.rotation = originalShipRotation;
                }

                #endregion

                #region Drag Coeffients and multipliers
                dragCoefficientsProp = shipInstanceProp.FindPropertyRelative("dragCoefficients");
                Vector3 dragCoefficientsVector = dragCoefficientsProp.vector3Value;
                dragCoefficientsVector.x = EditorGUILayout.Slider(dragCoefficientXContent, dragCoefficientsVector.x, 0f, 2f);
                dragCoefficientsVector.y = EditorGUILayout.Slider(dragCoefficientYContent, dragCoefficientsVector.y, 0f, 2f);
                dragCoefficientsVector.z = EditorGUILayout.Slider(dragCoefficientZContent, dragCoefficientsVector.z, 0f, 2f);
                dragCoefficientsProp.vector3Value = dragCoefficientsVector;
                if (shipControlModule.shipInstance.shipPhysicsModel != Ship.ShipPhysicsModel.PhysicsBased)
                {
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("angularDragFactor"), angularDragFactorContent);   
                }

                EditorGUILayout.PropertyField(disableDragMomentsProp, disableDragMomentsContent);

                if (!disableDragMomentsProp.boolValue)
                {
                    EditorGUILayout.LabelField(dragMomentMultipliersContent);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("dragXMomentMultiplier"), dragXMomentMultiplierContent);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("dragYMomentMultiplier"), dragYMomentMultiplierContent);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("dragZMomentMultiplier"), dragZMomentMultiplierContent);
                }
                #endregion

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(aeroCentreOfLiftDirectionContent);
                showCOLGizmosInSceneViewProp = shipInstanceProp.FindPropertyRelative("showCOLGizmosInSceneView");
                // Show Gizmos button
                if (showCOLGizmosInSceneViewProp.boolValue) { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleToggled, GUILayout.MaxWidth(22f))) { showCOLGizmosInSceneViewProp.boolValue = false; } }
                else { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleNormal, GUILayout.MaxWidth(22f))) { showCOLGizmosInSceneViewProp.boolValue = true; } }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(wingsHeaderContent, helpBoxRichText);

                EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("wingStallEffect"), wingStallEffectContent);

                wingListProp = shipInstanceProp.FindPropertyRelative("wingList");
                if (wingListProp == null)
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    shipControlModule.shipInstance.wingList = new List<Wing>();
                    // Read in the properties
                    serializedObject.Update();
                }

                #region Add-Remove Wings Buttons

                // Reset button variables
                wingMoveDownPos = -1;
                wingInsertPos = -1;
                wingDeletePos = -1;

                int numWings = wingListProp.arraySize;
                GUILayout.BeginHorizontal();

                EditorGUI.indentLevel += 1;
                EditorGUIUtility.fieldWidth = 15f;
                wingIsWingListExpandedProp = shipInstanceProp.FindPropertyRelative("isWingListExpanded");
                EditorGUI.BeginChangeCheck();
                wingIsWingListExpandedProp.boolValue = EditorGUILayout.Foldout(wingIsWingListExpandedProp.boolValue, "", foldoutStyleNoLabel);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(shipControlModule.shipInstance.wingList, wingIsWingListExpandedProp.boolValue);
                    // Read in the properties
                    serializedObject.Update();
                }
                EditorGUI.indentLevel -= 1;
                EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                EditorGUILayout.LabelField("<color=" + txtColourName + "><b>Wings: " + numWings.ToString("00") + "</b></color>", labelFieldRichText);

                if (numWings > 0)
                {
                    if (GUILayout.Button(gizmoToggleBtnContent, GUILayout.MaxWidth(22f)))
                    {
                        serializedObject.ApplyModifiedProperties();
                        Undo.RecordObject(shipControlModule, "Toggle Wing Gizmos");
                        ToggleGizmos(shipControlModule.shipInstance.wingList);
                        // Read in any changes made in ToggleGizmos
                        serializedObject.Update();
                    }
                }
                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(shipControlModule, "Add Wing");
                    shipControlModule.shipInstance.wingList.Add(new Wing());
                    // Read in the properties
                    serializedObject.Update();

                    numWings = wingListProp.arraySize;
                    if (numWings > 0)
                    {
                        // Force new wing to be serialized in scene
                        wingProp = wingListProp.GetArrayElementAtIndex(numWings - 1);
                        wingShowInEditorProp = wingProp.FindPropertyRelative("showInEditor");
                        wingShowInEditorProp.boolValue = !wingShowInEditorProp.boolValue;
                        // Show the new wing
                        wingShowInEditorProp.boolValue = true;
                    }

                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (numWings > 0) { wingDeletePos = wingListProp.arraySize - 1; }
                }
                GUILayout.EndHorizontal();

                #endregion

                #region Wing List

                numWings = wingListProp.arraySize;
                for (index = 0; index < numWings; index++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    wingProp = wingListProp.GetArrayElementAtIndex(index);

                    wingNameProp = wingProp.FindPropertyRelative("name");
                    wingShowInEditorProp = wingProp.FindPropertyRelative("showInEditor");
                    wingShowGizmosInSceneViewProp = wingProp.FindPropertyRelative("showGizmosInSceneView");
                    wingDamageRegionIndexProp = wingProp.FindPropertyRelative("damageRegionIndex");

                    #region Wing Find/Move/Insert/Delete buttons

                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel += 1;
                    wingShowInEditorProp.boolValue = EditorGUILayout.Foldout(wingShowInEditorProp.boolValue, (index + 1).ToString("00") + " " + wingNameProp.stringValue);
                    EditorGUI.indentLevel -= 1;

                    // Find (select) in the scene
                    SelectItemInSceneViewButton(shipControlModule.shipInstance.wingList, wingShowInEditorProp, wingProp.FindPropertyRelative("selectedInSceneView"), wingShowGizmosInSceneViewProp);

                    // Show Gizmos button
                    if (wingShowGizmosInSceneViewProp.boolValue) { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleToggled, GUILayout.MaxWidth(22f))) { wingShowGizmosInSceneViewProp.boolValue = false; } }
                    else { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleNormal, GUILayout.MaxWidth(22f))) { wingShowGizmosInSceneViewProp.boolValue = true; } }

                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numWings > 1) { wingMoveDownPos = index; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { wingInsertPos = index; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { wingDeletePos = index; }
                    GUILayout.EndHorizontal();

                    #endregion

                    if (wingShowInEditorProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(wingNameProp, wingNameContent);
                        EditorGUILayout.PropertyField(wingProp.FindPropertyRelative("angleOfAttack"), angleOfAttackContent);
                        EditorGUILayout.PropertyField(wingProp.FindPropertyRelative("span"), wingSpanContent);
                        EditorGUILayout.PropertyField(wingProp.FindPropertyRelative("chord"), wingChordContent);
                        if (shipControlModule.shipInstance.shipPhysicsModel == Ship.ShipPhysicsModel.PhysicsBased)
                        {
                            EditorGUILayout.PropertyField(wingProp.FindPropertyRelative("relativePosition"), wingRelativePositionContent);
                        }
                        EditorGUILayout.PropertyField(wingProp.FindPropertyRelative("liftDirection"), liftDirectionContent);

                        bool useProgressiveDamage = wingDamageRegionIndexProp.intValue > -1 && shipDamageModelProp.intValue != (int)Ship.ShipDamageModel.Simple;
                        if (shipDamageModelProp.intValue == (int)Ship.ShipDamageModel.Progressive)
                        {
                            useProgressiveDamage = EditorGUILayout.Toggle(partProgressiveDamageContent, useProgressiveDamage);
                            if (useProgressiveDamage) { wingDamageRegionIndexProp.intValue = 0; }
                            else { wingDamageRegionIndexProp.intValue = -1; }
                        }
                        else if (shipDamageModelProp.intValue == (int)Ship.ShipDamageModel.Localised)
                        {
                            wingDamageRegionIndexProp.intValue = EditorGUILayout.IntPopup(partDamageRegionContent, wingDamageRegionIndexProp.intValue, damageRegionIndexContents, damageRegionIndexInts);
                        }

                        if (useProgressiveDamage)
                        {
                            EditorGUILayout.PropertyField(wingProp.FindPropertyRelative("minPerformance"), wingMinPerformanceContent);
                            EditorGUILayout.PropertyField(wingProp.FindPropertyRelative("startingHealth"), partStartingHealthContent);
                        }
                    }
                    GUILayout.EndVertical();
                }

                #endregion

                #region Move/Remove/Insert Wings

                if (wingDeletePos >= 0 || wingInsertPos >= 0 || wingMoveDownPos >= 0)
                {
                    GUI.FocusControl(null);
                    // Don't permit multiple operations in the same pass
                    if (wingMoveDownPos >= 0)
                    {
                        // Move down one position, or wrap round to start of list
                        if (wingMoveDownPos < wingListProp.arraySize - 1)
                        {
                            wingListProp.MoveArrayElement(wingMoveDownPos, wingMoveDownPos + 1);
                        }
                        else { wingListProp.MoveArrayElement(wingMoveDownPos, 0); }

                        wingMoveDownPos = -1;
                    }
                    else if (wingInsertPos >= 0)
                    {
                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        shipControlModule.shipInstance.wingList.Insert(wingInsertPos, new Wing(shipControlModule.shipInstance.wingList[wingInsertPos]));

                        serializedObject.Update();

                        // Hide original wing
                        wingListProp.GetArrayElementAtIndex(wingInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                        wingShowInEditorProp = wingListProp.GetArrayElementAtIndex(wingInsertPos).FindPropertyRelative("showInEditor");

                        // Force new wing to be serialized in scene
                        wingShowInEditorProp.boolValue = !wingShowInEditorProp.boolValue;

                        // Show inserted duplicate wing
                        wingShowInEditorProp.boolValue = true;

                        wingInsertPos = -1;
                    }
                    else if (wingDeletePos >= 0)
                    {
                        // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and wingDeletePos is reset to -1.
                        int _deleteIndex = wingDeletePos;
                        if (EditorUtility.DisplayDialog("Delete Wing " + (wingDeletePos + 1) + "?", "Wing " + (wingDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the wing from the list and cannot be undone.", "Delete Now", "Cancel"))
                        {
                            wingListProp.DeleteArrayElementAtIndex(_deleteIndex);
                            wingDeletePos = -1;
                        }
                    }

                    SceneView.RepaintAll();

                    #if UNITY_2019_3_OR_NEWER
                    // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                    serializedObject.ApplyModifiedProperties();
                    if (!Application.isPlaying)
                    {
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                    GUIUtility.ExitGUI();
                    #endif
                }

                #endregion

                // Control surfaces only available in physics-based mode
                if (shipControlModule.shipInstance.shipPhysicsModel == Ship.ShipPhysicsModel.PhysicsBased)
                {
                    // Reset button variables
                    controlSurfaceMoveDownPos = -1;
                    controlSurfaceInsertPos = -1;
                    controlSurfaceDeletePos = -1;

                    EditorGUILayout.LabelField(controlSurfacesHeaderContent, helpBoxRichText);

                    controlSurfaceListProp = shipInstanceProp.FindPropertyRelative("controlSurfaceList");
                    if (controlSurfaceListProp == null)
                    {
                        // Apply property changes
                        serializedObject.ApplyModifiedProperties();
                        shipControlModule.shipInstance.controlSurfaceList = new List<ControlSurface>();
                        // Read in the properties
                        serializedObject.Update();
                    }

                    #region Add-Remove Control Surfaces Buttons
                    int numControlSurfaces = controlSurfaceListProp.arraySize;
                    GUILayout.BeginHorizontal();

                    EditorGUI.indentLevel += 1;
                    EditorGUIUtility.fieldWidth = 15f;
                    controlSurfaceIsControlSurfaceListExpandedProp = shipInstanceProp.FindPropertyRelative("isControlSurfaceListExpanded");
                    EditorGUI.BeginChangeCheck();
                    controlSurfaceIsControlSurfaceListExpandedProp.boolValue = EditorGUILayout.Foldout(controlSurfaceIsControlSurfaceListExpandedProp.boolValue, "", foldoutStyleNoLabel);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        ExpandList(shipControlModule.shipInstance.controlSurfaceList, controlSurfaceIsControlSurfaceListExpandedProp.boolValue);
                        serializedObject.Update();
                    }
                    EditorGUI.indentLevel -= 1;
                    EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;

                    EditorGUILayout.LabelField("<color=" + txtColourName + "><b>Control Surfaces: " + numControlSurfaces.ToString("00") + "</b></color>", labelFieldRichText);
                    if (numControlSurfaces > 0)
                    {
                        if (GUILayout.Button(gizmoToggleBtnContent, GUILayout.MaxWidth(22f)))
                        {
                            serializedObject.ApplyModifiedProperties();
                            Undo.RecordObject(shipControlModule, "Toggle Control Surface Gizmos");
                            ToggleGizmos(shipControlModule.shipInstance.controlSurfaceList);
                            // Read in any changes made in ToggleGizmos
                            serializedObject.Update();
                        }
                    }
                    if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                    {
                        // Apply property changes
                        serializedObject.ApplyModifiedProperties();
                        shipControlModule.shipInstance.controlSurfaceList.Add(new ControlSurface());
                        // Read in the properties
                        serializedObject.Update();

                        numControlSurfaces = controlSurfaceListProp.arraySize;
                        if (numControlSurfaces > 0)
                        {
                            // Force new control surface to be serialized in scene
                            controlSurfaceProp = controlSurfaceListProp.GetArrayElementAtIndex(numControlSurfaces - 1);
                            controlSurfaceShowInEditorProp = controlSurfaceProp.FindPropertyRelative("showInEditor");
                            controlSurfaceShowInEditorProp.boolValue = !controlSurfaceShowInEditorProp.boolValue;
                            // Show the new Control Surface
                            controlSurfaceShowInEditorProp.boolValue = true;
                        }
                        SceneView.RepaintAll();
                    }
                    if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                    {
                        if (numControlSurfaces > 0) { controlSurfaceDeletePos = controlSurfaceListProp.arraySize - 1; }
                    }
                    GUILayout.EndHorizontal();

                    #endregion

                    #region Control Surfaces List

                    numControlSurfaces = controlSurfaceListProp.arraySize;
                    for (index = 0; index < numControlSurfaces; index++)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        controlSurfaceProp = controlSurfaceListProp.GetArrayElementAtIndex(index);

                        controlSurfaceShowInEditorProp = controlSurfaceProp.FindPropertyRelative("showInEditor");
                        controlSurfaceShowGizmosInSceneViewProp = controlSurfaceProp.FindPropertyRelative("showGizmosInSceneView");
                        controlSurfaceDamageRegionIndexProp = controlSurfaceProp.FindPropertyRelative("damageRegionIndex");
                        
                        #region Control Surface Find/Move/Insert/Delete buttons
                        GUILayout.BeginHorizontal();
                        EditorGUI.indentLevel += 1;
                        controlSurfaceShowInEditorProp.boolValue = EditorGUILayout.Foldout(controlSurfaceShowInEditorProp.boolValue, "Control Surface: " + (index + 1).ToString("00"));
                        EditorGUI.indentLevel -= 1;

                        // Find (select) in the scene
                        SelectItemInSceneViewButton(shipControlModule.shipInstance.controlSurfaceList, controlSurfaceShowInEditorProp, controlSurfaceProp.FindPropertyRelative("selectedInSceneView"), controlSurfaceShowGizmosInSceneViewProp);

                        // Show Gizmos button
                        if (controlSurfaceShowGizmosInSceneViewProp.boolValue) { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleToggled, GUILayout.MaxWidth(22f))) { controlSurfaceShowGizmosInSceneViewProp.boolValue = false; } }
                        else { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleNormal, GUILayout.MaxWidth(22f))) { controlSurfaceShowGizmosInSceneViewProp.boolValue = true; } }

                        // Move down button
                        if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numControlSurfaces > 1) { controlSurfaceMoveDownPos = index; }
                        // Create duplicate button
                        if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { controlSurfaceInsertPos = index; }
                        // Delete button
                        if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { controlSurfaceDeletePos = index; }
                        GUILayout.EndHorizontal();
                        #endregion

                        if (controlSurfaceShowInEditorProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(controlSurfaceProp.FindPropertyRelative("type"), controlSurfaceTypeContent);
                            EditorGUILayout.PropertyField(controlSurfaceProp.FindPropertyRelative("span"), controlSurfaceSpanContent);
                            EditorGUILayout.PropertyField(controlSurfaceProp.FindPropertyRelative("chord"), controlSurfaceChordContent);
                            EditorGUILayout.PropertyField(controlSurfaceProp.FindPropertyRelative("relativePosition"), controlSurfaceRelativePositionContent);
                            //EditorGUILayout.PropertyField(controlSurfaceProp.FindPropertyRelative("rotationAxis"), controlSurfaceRotationAxisContent);

                            bool useProgressiveDamage = controlSurfaceDamageRegionIndexProp.intValue > -1 && shipDamageModelProp.intValue != (int)Ship.ShipDamageModel.Simple;
                            if (shipDamageModelProp.intValue == (int)Ship.ShipDamageModel.Progressive)
                            {
                                useProgressiveDamage = EditorGUILayout.Toggle(partProgressiveDamageContent, useProgressiveDamage);
                                if (useProgressiveDamage) { controlSurfaceDamageRegionIndexProp.intValue = 0; }
                                else { controlSurfaceDamageRegionIndexProp.intValue = -1; }
                            }
                            else if (shipDamageModelProp.intValue == (int)Ship.ShipDamageModel.Localised)
                            {
                                controlSurfaceDamageRegionIndexProp.intValue = EditorGUILayout.IntPopup(partDamageRegionContent, controlSurfaceDamageRegionIndexProp.intValue, damageRegionIndexContents, damageRegionIndexInts);
                            }

                            if (useProgressiveDamage)
                            {
                                EditorGUILayout.PropertyField(controlSurfaceProp.FindPropertyRelative("minPerformance"), controlSurfaceMinPerformanceContent);
                                EditorGUILayout.PropertyField(controlSurfaceProp.FindPropertyRelative("startingHealth"), partStartingHealthContent);
                            }
                        }
                        GUILayout.EndVertical();
                    }

                    #endregion

                    #region Move/Remove/Insert Control Surfaces

                    if (controlSurfaceDeletePos >= 0 || controlSurfaceInsertPos >= 0 || controlSurfaceMoveDownPos >= 0)
                    {
                        GUI.FocusControl(null);

                        // Don't permit multiple operations in the same pass
                        if (controlSurfaceMoveDownPos >= 0)
                        {
                            // Move down one position, or wrap round to start of list
                            if (controlSurfaceMoveDownPos < controlSurfaceListProp.arraySize - 1)
                            {
                                controlSurfaceListProp.MoveArrayElement(controlSurfaceMoveDownPos, controlSurfaceMoveDownPos + 1);
                            }
                            else { controlSurfaceListProp.MoveArrayElement(controlSurfaceMoveDownPos, 0); }

                            controlSurfaceMoveDownPos = -1;
                        }
                        else if (controlSurfaceInsertPos >= 0)
                        {
                            // Apply property changes before potential list changes
                            serializedObject.ApplyModifiedProperties();

                            shipControlModule.shipInstance.controlSurfaceList.Insert(controlSurfaceInsertPos, new ControlSurface(shipControlModule.shipInstance.controlSurfaceList[controlSurfaceInsertPos]));

                            // Read in the properties as the list may have changed
                            serializedObject.Update();

                            // Hide original control surface
                            controlSurfaceListProp.GetArrayElementAtIndex(controlSurfaceInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                            controlSurfaceShowInEditorProp = controlSurfaceListProp.GetArrayElementAtIndex(controlSurfaceInsertPos).FindPropertyRelative("showInEditor");

                            // Force new control surface to be serialized in scene
                            controlSurfaceShowInEditorProp.boolValue = !controlSurfaceShowInEditorProp.boolValue;

                            // Show inserted duplicate control surface
                            controlSurfaceShowInEditorProp.boolValue = true;

                            controlSurfaceInsertPos = -1;
                        }
                        else if (controlSurfaceDeletePos >= 0)
                        {
                            // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and DeletePos is reset to -1.
                            int _deleteIndex = controlSurfaceDeletePos;
                            if (EditorUtility.DisplayDialog("Delete Control Surface " + (_deleteIndex + 1).ToString("00") + "?", "Control Surface " + (_deleteIndex + 1).ToString("00") + " will be deleted\n\nThis action will remove the control surface from the list and cannot be undone.", "Delete Now", "Cancel"))
                            {
                                controlSurfaceListProp.DeleteArrayElementAtIndex(_deleteIndex);
                                controlSurfaceDeletePos = -1;
                            }
                        }

                        SceneView.RepaintAll();

                        #if UNITY_2019_3_OR_NEWER
                        serializedObject.ApplyModifiedProperties();
                        // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                        if (!Application.isPlaying)
                        {
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }
                        GUIUtility.ExitGUI();
                        #endif
                    }

                    #endregion
                }
                // Generic braking component only available in arcade mode
                else
                {
                    arcadeUseBrakeComponentProp = shipInstanceProp.FindPropertyRelative("arcadeUseBrakeComponent");
                    EditorGUILayout.PropertyField(arcadeUseBrakeComponentProp, arcadeUseBrakeComponentContent);
                    if (arcadeUseBrakeComponentProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("arcadeBrakeStrength"), arcadeBrakeStrengthContent);
                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("arcadeBrakeIgnoreMediumDensity"), arcadeBrakeIgnoreMediumDensityContent);
                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("arcadeBrakeMinAcceleration"), arcadeBrakeMinAccelerationContent);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            #endregion

            #region Combat

            else if (selectedTabIntProp.intValue == 4)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                respawningModeProp = shipInstanceProp.FindPropertyRelative("respawningMode");
                
                #region Damage
                EditorGUILayout.LabelField(damageHeaderContent, helpBoxRichText);  

                GUILayout.BeginHorizontal();
                EditorGUI.indentLevel += 1;
                EditorGUIUtility.fieldWidth = 15f;
                showDamageInEditorProp = shipInstanceProp.FindPropertyRelative("showDamageInEditor");
                showDamageInEditorProp.boolValue = EditorGUILayout.Foldout(showDamageInEditorProp.boolValue, "", foldoutStyleNoLabel);
                EditorGUI.indentLevel -= 1;
                EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                EditorGUILayout.LabelField(shipDamageModelContent, GUILayout.Width(defaultEditorLabelWidth + 3f));
                EditorGUILayout.PropertyField(shipDamageModelProp, GUIContent.none);
                GUILayout.EndHorizontal();

                if (showDamageInEditorProp.boolValue)
                {
                    damageRegionIsMainDamageRegionExpandedProp = shipInstanceProp.FindPropertyRelative("isMainDamageExpanded");

                    // If we are using the localised damage model, this information is for the main damage region
                    if (shipControlModule.shipInstance.shipDamageModel == Ship.ShipDamageModel.Localised)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUI.indentLevel += 1;
                        EditorGUIUtility.fieldWidth = 15f;
                        damageRegionIsMainDamageRegionExpandedProp.boolValue = EditorGUILayout.Foldout(damageRegionIsMainDamageRegionExpandedProp.boolValue, GUIContent.none, foldoutStyleNoLabel);
                        EditorGUI.indentLevel -= 1;
                        EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                        EditorGUILayout.LabelField("<color=" + txtColourName + "><b>Main Damage Region</b></color>", labelFieldRichText);
                        GUILayout.EndHorizontal();
                    }
                    // When there is only a single (main) damage region - always show it
                    else if (!damageRegionIsMainDamageRegionExpandedProp.boolValue) { damageRegionIsMainDamageRegionExpandedProp.boolValue = true; }

                    damageRegionProp = shipInstanceProp.FindPropertyRelative("mainDamageRegion");
                    useDamageMultipliersProp = shipInstanceProp.FindPropertyRelative("useDamageMultipliers");

                    #region Main Damage Region
                    if (damageRegionIsMainDamageRegionExpandedProp.boolValue)
                    {
                        // Expand label width from 150 to 175.
                        EditorGUIUtility.labelWidth = 175f;

                        EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("startingHealth"), damageStartHealthContent);
                        EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("isInvincible"), damageRegionInvincibleContent);
                        useShieldingProp = damageRegionProp.FindPropertyRelative("useShielding");
                        EditorGUILayout.PropertyField(useShieldingProp, useShieldingContent);
                        if (useShieldingProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("shieldingDamageThreshold"), shieldingDamageThresholdContent);
                            EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("shieldingAmount"), shieldingAmountContent);

                            shieldingRechargeRateProp = damageRegionProp.FindPropertyRelative("shieldingRechargeRate");
                            EditorGUILayout.PropertyField(shieldingRechargeRateProp, shieldingRechargeRateContent);
                            if (shieldingRechargeRateProp.floatValue > 0f)
                            {
                                EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("shieldingRechargeDelay"), shieldingRechargeDelayContent);
                            }
                        }
                        EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("collisionDamageResistance"), collisionDamageResistanceContent);

                        applyControllerRumbleProp = shipInstanceProp.FindPropertyRelative("applyControllerRumble");
                        EditorGUILayout.PropertyField(applyControllerRumbleProp, applyControllerRumbleContent);

                        if (applyControllerRumbleProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("minRumbleDamage"), minRumbleDamageContent);
                            EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("maxRumbleDamage"), maxRumbleDamageContent);
                            EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("damageRumbleTime"), damageRumbleTimeContent);
                        }

                        damageRegionEffectsObjectProp = damageRegionProp.FindPropertyRelative("destructionEffectsObject");
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(shipDestroyEffectsObjectContent, GUILayout.Width(EditorGUIUtility.labelWidth - 28f));
                        if (GUILayout.Button(gotoEffectFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.effectsFolder, false, true); }
                        EditorGUILayout.PropertyField(damageRegionEffectsObjectProp, GUIContent.none);
                        GUILayout.EndHorizontal();

                        damageRegionDestructObjectProp = damageRegionProp.FindPropertyRelative("destructObject");
                        //if (damageRegionDestructObjectProp.objectReferenceValue != null && respawningModeProp.intValue != (int)Ship.RespawningMode.DontRespawn)
                        //{
                        //    EditorGUILayout.HelpBox(destructRespawnWaring, MessageType.Warning);
                        //}

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(shipDestroyDestructObjectContent, GUILayout.Width(EditorGUIUtility.labelWidth - 28f));
                        if (GUILayout.Button(gotoDestructsFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.destructFolder, false, true); }
                        EditorGUILayout.PropertyField(damageRegionDestructObjectProp, GUIContent.none);
                        GUILayout.EndHorizontal();

                        #region Main Damage Region Damage Multipliers

                        EditorGUILayout.PropertyField(useDamageMultipliersProp, useDamageMultipliersContent);

                        if (useDamageMultipliersProp.boolValue)
                        {
                            DrawDamageMultipliers(shipControlModule.shipInstance.mainDamageRegion);
                        }

                        #endregion

                        EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
                    }
                    #endregion

                    #region Localised Damage Regions
                    if (shipControlModule.shipInstance.shipDamageModel == Ship.ShipDamageModel.Localised)
                    {
                        // Expand label width from 150 to 175.
                        EditorGUIUtility.labelWidth = 175f;

                        damageRegionListProp = shipInstanceProp.FindPropertyRelative("localisedDamageRegionList");
                        if (damageRegionListProp == null)
                        {
                            // Apply property changes
                            serializedObject.ApplyModifiedProperties();
                            shipControlModule.shipInstance.localisedDamageRegionList = new List<DamageRegion>();
                            // Read in the properties
                            serializedObject.Update();
                        }

                        #region Add-Remove Damage Regions Buttons

                        // Reset button variables
                        damageRegionMoveDownPos = -1;
                        damageRegionInsertPos = -1;
                        damageRegionDeletePos = -1;

                        int numDamageRegions = damageRegionListProp.arraySize;
                        GUILayout.BeginHorizontal();

                        EditorGUI.indentLevel += 1;
                        EditorGUIUtility.fieldWidth = 15f;
                        damageRegionIsDamageRegionListExpandedProp = shipInstanceProp.FindPropertyRelative("isDamageListExpanded");
                        EditorGUI.BeginChangeCheck();
                        damageRegionIsDamageRegionListExpandedProp.boolValue = EditorGUILayout.Foldout(damageRegionIsDamageRegionListExpandedProp.boolValue, "", foldoutStyleNoLabel);
                        if (EditorGUI.EndChangeCheck())
                        {
                            serializedObject.ApplyModifiedProperties();
                            ExpandList(shipControlModule.shipInstance.localisedDamageRegionList, damageRegionIsDamageRegionListExpandedProp.boolValue);
                            // Read in the properties
                            serializedObject.Update();
                        }
                        EditorGUI.indentLevel -= 1;
                        EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;

                        EditorGUILayout.LabelField("<color=" + txtColourName + "><b>Local Damage Regions: " + numDamageRegions.ToString("00") + "</b></color>", labelFieldRichText);
                        if (numDamageRegions > 0)
                        {
                            if (GUILayout.Button(gizmoToggleBtnContent, GUILayout.MaxWidth(22f)))
                            {
                                serializedObject.ApplyModifiedProperties();
                                Undo.RecordObject(shipControlModule, "Toggle Damage Region Gizmos");
                                ToggleGizmos(shipControlModule.shipInstance.localisedDamageRegionList);
                                // Read in any changes made in ToggleGizmos
                                serializedObject.Update();
                            }
                        }
                        if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                        {
                            // Apply property changes
                            serializedObject.ApplyModifiedProperties();
                            shipControlModule.shipInstance.localisedDamageRegionList.Add(new DamageRegion());
                            // Read in the properties
                            serializedObject.Update();

                            numDamageRegions = damageRegionListProp.arraySize;
                            if (numDamageRegions > 0)
                            {
                                // Force new damage region to be serialized in scene
                                damageRegionProp = damageRegionListProp.GetArrayElementAtIndex(numDamageRegions - 1);
                                damageRegionShowInEditorProp = damageRegionProp.FindPropertyRelative("showInEditor");
                                damageRegionShowInEditorProp.boolValue = !damageRegionShowInEditorProp.boolValue;
                                // Show the new damage region
                                damageRegionShowInEditorProp.boolValue = true;
                            }

                            SceneView.RepaintAll();
                        }
                        if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                        {
                            if (numDamageRegions > 0) { damageRegionDeletePos = damageRegionListProp.arraySize - 1; }
                        }
                        GUILayout.EndHorizontal();

                        #endregion

                        #region Damage Region List

                        numDamageRegions = damageRegionListProp.arraySize;

                        useLocalisedDamageMultipliersProp = shipInstanceProp.FindPropertyRelative("useLocalisedDamageMultipliers");

                        if (numDamageRegions > 0 && useDamageMultipliersProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(useLocalisedDamageMultipliersProp, useLocalisedDamageMultipliersContent);
                        }

                        for (index = 0; index < numDamageRegions; index++)
                        {
                            GUILayout.BeginVertical(EditorStyles.helpBox);
                            damageRegionProp = damageRegionListProp.GetArrayElementAtIndex(index);

                            damageRegionNameProp = damageRegionProp.FindPropertyRelative("name");
                            damageRegionShowInEditorProp = damageRegionProp.FindPropertyRelative("showInEditor");
                            damageRegionShowGizmosInSceneViewProp = damageRegionProp.FindPropertyRelative("showGizmosInSceneView");

                            #region Damage Region Find/Move/Insert/Delete buttons

                            GUILayout.BeginHorizontal();
                            EditorGUI.indentLevel += 1;
                            damageRegionShowInEditorProp.boolValue = EditorGUILayout.Foldout(damageRegionShowInEditorProp.boolValue, (index + 1).ToString("00") + " " + damageRegionNameProp.stringValue);
                            EditorGUI.indentLevel -= 1;

                            // Find (select) in the scene
                            SelectItemInSceneViewButton(shipControlModule.shipInstance.localisedDamageRegionList, damageRegionShowInEditorProp, damageRegionProp.FindPropertyRelative("selectedInSceneView"), damageRegionShowGizmosInSceneViewProp);

                            // Show Gizmos button
                            if (damageRegionShowGizmosInSceneViewProp.boolValue) { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleToggled, GUILayout.MaxWidth(22f))) { damageRegionShowGizmosInSceneViewProp.boolValue = false; } }
                            else { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleNormal, GUILayout.MaxWidth(22f))) { damageRegionShowGizmosInSceneViewProp.boolValue = true; } }

                            // Move down button
                            if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numDamageRegions > 1) { damageRegionMoveDownPos = index; }
                            // Create duplicate button
                            if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { damageRegionInsertPos = index; }
                            // Delete button
                            if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { damageRegionDeletePos = index; }
                            GUILayout.EndHorizontal();

                            #endregion

                            if (damageRegionShowInEditorProp.boolValue)
                            {
                                EditorGUILayout.PropertyField(damageRegionNameProp, damageRegionNameContent);
                                EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("relativePosition"), damageRegionRelativePositionContent);
                                EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("size"), damageRegionSizeContent);
                                EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("startingHealth"), damageRegionStartHealthContent);
                                EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("isInvincible"), damageRegionInvincibleContent);
                                useShieldingProp = damageRegionProp.FindPropertyRelative("useShielding");
                                EditorGUILayout.PropertyField(useShieldingProp, damageRegionUseShieldingContent);
                                if (useShieldingProp.boolValue)
                                {
                                    EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("shieldingDamageThreshold"), damageRegionShieldingDamageThresholdContent);
                                    EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("shieldingAmount"), damageRegionShieldingAmountContent);

                                    shieldingRechargeRateProp = damageRegionProp.FindPropertyRelative("shieldingRechargeRate");
                                    EditorGUILayout.PropertyField(shieldingRechargeRateProp, shieldingRechargeRateContent);
                                    if (shieldingRechargeRateProp.floatValue > 0f)
                                    {
                                        EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("shieldingRechargeDelay"), shieldingRechargeDelayContent);
                                    }
                                }
                                EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("collisionDamageResistance"), damageRegionCollisionDamageResistanceContent);

                                damageRegionEffectsObjectProp = damageRegionProp.FindPropertyRelative("destructionEffectsObject");
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(damageRegionEffectsObjectContent, GUILayout.Width(EditorGUIUtility.labelWidth - 28f));
                                if (GUILayout.Button(gotoEffectFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.effectsFolder, false, true); }
                                EditorGUILayout.PropertyField(damageRegionEffectsObjectProp, GUIContent.none);
                                GUILayout.EndHorizontal();

                                if (damageRegionEffectsObjectProp.objectReferenceValue != null)
                                {
                                    EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("isMoveDestructionEffectsObject"), damageRegionIsMoveEffectsObjectContent);
                                }

                                damageRegionDestructObjectProp = damageRegionProp.FindPropertyRelative("destructObject");
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(damageRegionDestructObjectContent, GUILayout.Width(EditorGUIUtility.labelWidth - 28f));
                                if (GUILayout.Button(gotoDestructsFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.destructFolder, false, true); }
                                EditorGUILayout.PropertyField(damageRegionDestructObjectProp, GUIContent.none);
                                GUILayout.EndHorizontal();

                                damageRegionChildTransformProp = damageRegionProp.FindPropertyRelative("regionChildTransform");
                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(damageRegionChildTransformProp, damageRegionChildTransformContent);
                                if (EditorGUI.EndChangeCheck() && damageRegionChildTransformProp.objectReferenceValue != null)
                                {
                                    if (!((Transform)damageRegionChildTransformProp.objectReferenceValue).IsChildOf(shipControlModule.transform))
                                    {
                                        damageRegionChildTransformProp.objectReferenceValue = null;
                                        Debug.LogWarning("The transform to disable for this region must be a child of the parent ship gameobject or part of the prefab.");
                                    }
                                }

                                EditorGUILayout.PropertyField(damageRegionProp.FindPropertyRelative("isRadarEnabled"), damageRegionIsRadarEnabledContent);

                                #region Localised Damage Region damage multipliers

                                if (useDamageMultipliersProp.boolValue && useLocalisedDamageMultipliersProp.boolValue)
                                {
                                    DrawDamageMultipliers(shipControlModule.shipInstance.localisedDamageRegionList[index]);
                                }

                                #endregion
                            }
                            GUILayout.EndVertical();
                        }

                        #endregion

                        EditorGUIUtility.labelWidth = defaultEditorLabelWidth;

                        #region Move/Remove/Insert Damage Regions

                        if (damageRegionDeletePos >= 0 || damageRegionInsertPos >= 0 || damageRegionMoveDownPos >= 0)
                        {
                            GUI.FocusControl(null);

                            // Don't permit multiple operations in the same pass
                            if (damageRegionMoveDownPos >= 0)
                            {
                                // Move down one position, or wrap round to start of list
                                if (damageRegionMoveDownPos < damageRegionListProp.arraySize - 1)
                                {
                                    damageRegionListProp.MoveArrayElement(damageRegionMoveDownPos, damageRegionMoveDownPos + 1);
                                }
                                else { damageRegionListProp.MoveArrayElement(damageRegionMoveDownPos, 0); }

                                damageRegionMoveDownPos = -1;
                            }
                            else if (damageRegionInsertPos >= 0)
                            {
                                // Apply property changes before potential list changes
                                serializedObject.ApplyModifiedProperties();

                                shipControlModule.shipInstance.localisedDamageRegionList.Insert(damageRegionInsertPos, new DamageRegion(shipControlModule.shipInstance.localisedDamageRegionList[damageRegionInsertPos]));

                                serializedObject.Update();

                                // Hide original damage region
                                damageRegionListProp.GetArrayElementAtIndex(damageRegionInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                                damageRegionShowInEditorProp = damageRegionListProp.GetArrayElementAtIndex(damageRegionInsertPos).FindPropertyRelative("showInEditor");
                                // Generate a new hashcode for the duplicated DamageRegion
                                damageRegionListProp.GetArrayElementAtIndex(damageRegionInsertPos).FindPropertyRelative("guidHash").intValue = SSCMath.GetHashCodeFromGuid();

                                // Force new damage region to be serialized in scene
                                damageRegionShowInEditorProp.boolValue = !damageRegionShowInEditorProp.boolValue;

                                // Show inserted duplicate damage region
                                damageRegionShowInEditorProp.boolValue = true;

                                damageRegionInsertPos = -1;
                            }
                            else if (damageRegionDeletePos >= 0)
                            {
                                // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and damageRegionDeletePos is reset to -1.
                                int _deleteIndex = damageRegionDeletePos;

                                if (EditorUtility.DisplayDialog("Delete Damage Region " + (damageRegionDeletePos + 1) + "?", "Damage Region " + (damageRegionDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the damage region from the list and cannot be undone.", "Delete Now", "Cancel"))
                                {
                                    damageRegionListProp.DeleteArrayElementAtIndex(_deleteIndex);
                                    damageRegionDeletePos = -1;
                                }
                            }

                            SceneView.RepaintAll();

                            #if UNITY_2019_3_OR_NEWER
                            serializedObject.ApplyModifiedProperties();
                            // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                            if (!Application.isPlaying)
                            {
                                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                            }
                            GUIUtility.ExitGUI();
                            #endif
                        }

                        #endregion
                    }
                    #endregion
                }
                #endregion

                #region Respawning
                EditorGUILayout.LabelField(respawningHeaderContent, helpBoxRichText);
 
                collisionRespawnPositionDelayProp = shipInstanceProp.FindPropertyRelative("collisionRespawnPositionDelay");

                EditorGUILayout.PropertyField(respawningModeProp, respawningModeContent);

                if (respawningModeProp.intValue != (int)Ship.RespawningMode.DontRespawn)
                {
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("respawnTime"), respawnTimeContent);

                    if (respawningModeProp.intValue == (int)Ship.RespawningMode.RespawnAtLastPosition)
                    {
                        EditorGUILayout.PropertyField(collisionRespawnPositionDelayProp, collisionRespawnPositionDelayContent);
                    }
                    else if (respawningModeProp.intValue == (int)Ship.RespawningMode.RespawnAtSpecifiedPosition)
                    {
                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("customRespawnPosition"), customRespawnPositionContent);
                        EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("customRespawnRotation"), customRespawnRotationContent);
                    }
                    else if (respawningModeProp.intValue == (int)Ship.RespawningMode.RespawnOnPath)
                    {
                        if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(shipControlModule.gameObject.scene.handle); }
                        if (sscManager != null)
                        {
                            int numPaths = sscManager.pathDataList == null ? 0 : sscManager.pathDataList.Count;

                            if (numPaths > 0)
                            {
                                respawningPathGUIDHashProp = shipInstanceProp.FindPropertyRelative("respawningPathGUIDHash");

                                // Get the index or sequence order in the list of Paths.
                                int pathIdx = sscManager.pathDataList.FindIndex(p => p.guidHash == respawningPathGUIDHashProp.intValue);

                                if (pathContents.Length != numPaths + 1)
                                {
                                    System.Array.Resize(ref pathContents, numPaths + 1);
                                }

                                // Populate an array of path names
                                pathContents[0] = new GUIContent("None");
                                for (int pIdx = 0; pIdx < numPaths; pIdx++)
                                {
                                    pathContents[pIdx + 1] = new GUIContent(string.IsNullOrEmpty(sscManager.pathDataList[pIdx].name) ? "Path " + (pIdx + 1) : sscManager.pathDataList[pIdx].name);
                                }

                                int pathPopupSelectedIdx = EditorGUILayout.Popup(respawningPathContent, pathIdx < 0 ? 0 : pathIdx + 1, pathContents);

                                // Did the Path selection change?
                                if (pathPopupSelectedIdx - 1 != pathIdx)
                                {
                                    pathIdx = pathPopupSelectedIdx - 1;
                                    if (pathIdx < 0 || pathIdx >= numPaths)
                                    {
                                        respawningPathGUIDHashProp.intValue = 0;
                                    }
                                    else
                                    {
                                        respawningPathGUIDHashProp.intValue = sscManager.pathDataList[pathIdx].guidHash;
                                    }
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField("No Paths found in SSCManager in the scene");
                            }
                        }
                    }

                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("respawnVelocity"), respawnVelocityContent);
                }

                respawningStuckTimeProp = shipInstanceProp.FindPropertyRelative("stuckTime");
                if (respawningStuckTimeProp.floatValue == 0f) { EditorGUILayout.PropertyField(respawningStuckTimeProp, respawningStuckTimeNeverContent); }
                else
                {
                    EditorGUILayout.PropertyField(respawningStuckTimeProp, respawningStuckTimeContent);
                    EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("stuckSpeedThreshold"), respawningStuckSpeedThresholdContent);

                    respawningStuckActionProp = shipInstanceProp.FindPropertyRelative("stuckAction");

                    // When using RespawnMode and Respawn last position less x seconds, the stuck time needs to be less else we'll just keep getting stuck.
                    if (respawningStuckActionProp.intValue == (int)Ship.StuckAction.SameAsRespawningMode && respawningStuckTimeProp.floatValue > collisionRespawnPositionDelayProp.floatValue && respawningModeProp.intValue == (int)Ship.RespawningMode.RespawnAtLastPosition)
                    {
                        EditorGUILayout.HelpBox("Decrease the Stuck Time OR increase the Col. Respawn Delay to avoid always getting stuck.", MessageType.Warning, true);
                    }

                    EditorGUILayout.PropertyField(respawningStuckActionProp, respawningStuckActionContent);

                    if (respawningStuckActionProp.intValue == (int)Ship.StuckAction.InvokeCallback)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(respawningStuckCallbackContent, GUILayout.Width(defaultEditorLabelWidth));
                        if (shipControlModule.callbackOnStuck == null)
                        {
                            EditorGUILayout.LabelField("Not set (set this in your code)");
                        }
                        else
                        {
                            EditorGUILayout.LabelField(shipControlModule.callbackOnStuck.Method.Name);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else if (respawningStuckActionProp.intValue == (int)Ship.StuckAction.RespawnOnPath)
                    {
                        if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(shipControlModule.gameObject.scene.handle); }
                        if (sscManager != null)
                        {
                            int numPaths = sscManager.pathDataList == null ? 0 : sscManager.pathDataList.Count;

                            if (numPaths > 0)
                            {
                                respawningStuckActionPathGUIDHashProp = shipInstanceProp.FindPropertyRelative("stuckActionPathGUIDHash");

                                // Get the index or sequence order in the list of Paths.
                                int pathIdx = sscManager.pathDataList.FindIndex(p => p.guidHash == respawningStuckActionPathGUIDHashProp.intValue);

                                if (pathContents.Length != numPaths + 1)
                                {
                                    System.Array.Resize(ref pathContents, numPaths + 1);
                                }

                                // Populate an array of path names
                                pathContents[0] = new GUIContent("None");
                                for (int pIdx = 0; pIdx < numPaths; pIdx++)
                                {
                                    pathContents[pIdx + 1] = new GUIContent(string.IsNullOrEmpty(sscManager.pathDataList[pIdx].name) ? "Path " + (pIdx+1) : sscManager.pathDataList[pIdx].name);
                                }

                                int pathPopupSelectedIdx = EditorGUILayout.Popup(respawningStuckPathContent, pathIdx < 0 ? 0 : pathIdx + 1, pathContents);

                                // Did the Path selection change?
                                if (pathPopupSelectedIdx - 1 != pathIdx)
                                {
                                    pathIdx = pathPopupSelectedIdx - 1;
                                    if (pathIdx < 0 || pathIdx >= numPaths)
                                    {
                                        respawningStuckActionPathGUIDHashProp.intValue = 0;
                                    }
                                    else
                                    {
                                        respawningStuckActionPathGUIDHashProp.intValue = sscManager.pathDataList[pathIdx].guidHash;
                                    }
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField("No Paths found in SSCManager in the scene");
                            }
                        }
                    }
                }

                #endregion

                #region Weapons
                EditorGUILayout.LabelField(weaponsHeaderContent, helpBoxRichText);

                // Expand label width from 150 to 175.
                EditorGUIUtility.labelWidth = 175f;
                EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("useWeaponsWhenMovementDisabled"), weaponUseWhenMovementDisabledContent);
                EditorGUIUtility.labelWidth = defaultEditorLabelWidth;

                weaponListProp = shipInstanceProp.FindPropertyRelative("weaponList");
                if (weaponListProp == null)
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    shipControlModule.shipInstance.weaponList = new List<Weapon>();
                    isSceneModified = true;
                    // Read in the properties
                    serializedObject.Update();
                }

                // Reset button variables
                weaponMoveDownPos = -1;
                weaponInsertPos = -1;
                weaponDeletePos = -1;

                #region Add-Remove Weapons and Gizmos Buttons

                int numWeapons = weaponListProp.arraySize;
                GUILayout.BeginHorizontal();

                EditorGUI.indentLevel += 1;
                EditorGUIUtility.fieldWidth = 15f;
                weaponIsWeaponListExpandedProp = shipInstanceProp.FindPropertyRelative("isWeaponListExpanded");
                EditorGUI.BeginChangeCheck();
                weaponIsWeaponListExpandedProp.boolValue = EditorGUILayout.Foldout(weaponIsWeaponListExpandedProp.boolValue, "", foldoutStyleNoLabel);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(shipControlModule.shipInstance.weaponList, weaponIsWeaponListExpandedProp.boolValue);
                    // Read in the properties
                    serializedObject.Update();
                }
                EditorGUI.indentLevel -= 1;
                EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                EditorGUILayout.LabelField("<color=" + txtColourName + "><b>Weapons: " + numWeapons.ToString("00") + "</b></color>", labelFieldRichText);

                if (numWeapons > 0)
                {
                    if (GUILayout.Button(gizmoToggleBtnContent, GUILayout.MaxWidth(22f)))
                    {
                        serializedObject.ApplyModifiedProperties();
                        Undo.RecordObject(shipControlModule, "Toggle Weapon Gizmos");
                        ToggleGizmos(shipControlModule.shipInstance.weaponList);
                        serializedObject.Update();
                    }
                }

                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(shipControlModule, "Add Weapon");
                    shipControlModule.shipInstance.weaponList.Add(new Weapon());
                    isSceneModified = true;
                    // Read in the properties
                    serializedObject.Update();

                    numWeapons = weaponListProp.arraySize;
                    if (numWeapons > 0)
                    {
                        // Force new weapon to be serialized in scene
                        weaponProp = weaponListProp.GetArrayElementAtIndex(numWeapons - 1);
                        weaponShowInEditorProp = weaponProp.FindPropertyRelative("showInEditor");
                        weaponShowInEditorProp.boolValue = !weaponShowInEditorProp.boolValue;
                        // Show the new weapon
                        weaponShowInEditorProp.boolValue = true;
                    }
                }
                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (numWeapons > 0) { weaponDeletePos = weaponListProp.arraySize - 1; }
                }
                GUILayout.EndHorizontal();

                #endregion

                #region Weapon List

                numWeapons = weaponListProp.arraySize;
                for (index = 0; index < numWeapons; index++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    weaponProp = weaponListProp.GetArrayElementAtIndex(index);

                    #region Find Properties
                    weaponNameProp = weaponProp.FindPropertyRelative("name");
                    weaponTypeProp = weaponProp.FindPropertyRelative("weaponType");
                    weaponShowInEditorProp = weaponProp.FindPropertyRelative("showInEditor");
                    weaponShowGizmosInSceneViewProp = weaponProp.FindPropertyRelative("showGizmosInSceneView");
                    weaponDamageRegionIndexProp = weaponProp.FindPropertyRelative("damageRegionIndex");
                    weaponAmmunitionProp = weaponProp.FindPropertyRelative("ammunition");
                    weaponRechargeTimeProp = weaponProp.FindPropertyRelative("rechargeTime");
                    weaponChargeAmountProp = weaponProp.FindPropertyRelative("chargeAmount");
                    weaponFirePositionListProp = weaponProp.FindPropertyRelative("firePositionList");
                    weaponFireDirectionProp = weaponProp.FindPropertyRelative("fireDirection");
                    weaponRelativePositionProp = weaponProp.FindPropertyRelative("relativePosition");
                    #endregion

                    // If the fire position list is null, create the list and add a default position
                    if (weaponFirePositionListProp == null)
                    {
                        // Apply property changes
                        serializedObject.ApplyModifiedProperties();
                        shipControlModule.shipInstance.weaponList[index].firePositionList = new List<Vector3>(2);
                        shipControlModule.shipInstance.weaponList[index].firePositionList.Add(Vector3.zero);
                        isSceneModified = true;
                        // Read in the properties
                        serializedObject.Update();
                    }

                    #region Weapon Find/Move/Insert/Delete buttons
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel += 1;
                    weaponShowInEditorProp.boolValue = EditorGUILayout.Foldout(weaponShowInEditorProp.boolValue, (index + 1).ToString("00") + " " + weaponNameProp.stringValue);
                    EditorGUI.indentLevel -= 1;

                    // Find (select) in the scene
                    SelectItemInSceneViewButton(shipControlModule.shipInstance.weaponList, weaponShowInEditorProp, weaponProp.FindPropertyRelative("selectedInSceneView"), weaponShowGizmosInSceneViewProp);

                    // Show Gizmos button
                    if (weaponShowGizmosInSceneViewProp.boolValue) { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleToggled, GUILayout.MaxWidth(22f))) { weaponShowGizmosInSceneViewProp.boolValue = false; } }
                    else { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleNormal, GUILayout.MaxWidth(22f))) { weaponShowGizmosInSceneViewProp.boolValue = true; } }

                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numWeapons > 1) { weaponMoveDownPos = index; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { weaponInsertPos = index; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { weaponDeletePos = index; }
                    GUILayout.EndHorizontal();
                    #endregion

                    if (weaponShowInEditorProp.boolValue)
                    {
                        weaponFiringButtonProp = weaponProp.FindPropertyRelative("firingButton");
                        weaponIsMultipleFirePositionsProp = weaponProp.FindPropertyRelative("isMultipleFirePositions");
                        weaponProjectilePrefabProp = weaponProp.FindPropertyRelative("projectilePrefab");
                        weaponBeamPrefabProp = weaponProp.FindPropertyRelative("beamPrefab");
                        weaponIsAutoTargetingEnabledProp = weaponProp.FindPropertyRelative("isAutoTargetingEnabled");

                        EditorGUILayout.PropertyField(weaponNameProp, weaponNameContent);

                        int prevWeaponType = weaponTypeProp.intValue;
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(weaponTypeProp, weaponTypeContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            // If the weapon has changed from Projectile to Beam or vis versa, clear the old prefab and set defaults.
                            if (weaponTypeProp.intValue == Weapon.FixedBeamInt || weaponTypeProp.intValue == Weapon.TurretBeamInt)
                            {
                                weaponProjectilePrefabProp.objectReferenceValue = null;
                                // Currently beams have unlimited amno
                                weaponAmmunitionProp.intValue = -1;
                            }
                            else if (prevWeaponType == Weapon.FixedBeamInt || prevWeaponType == Weapon.TurretBeamInt)
                            {
                                weaponBeamPrefabProp.objectReferenceValue = null;
                            }
                        }

                        if (weaponTypeProp.intValue == Weapon.TurretBeamInt) { SSCEditorHelper.InTechPreview(false); }

                        if (weaponRelativePositionProp.vector3Value == Vector3.zero && (weaponTypeProp.intValue == Weapon.TurretProjectileInt || weaponTypeProp.intValue == Weapon.TurretBeamInt))
                        {
                            EditorGUILayout.HelpBox("For quick start setup, drag the Turret Pivot Y transform into the slot below", MessageType.Info);
                        }
                        EditorGUILayout.PropertyField(weaponRelativePositionProp, weaponRelativePositionContent);

                        #region Weapon Fire Positions
                        EditorGUILayout.PropertyField(weaponIsMultipleFirePositionsProp, weaponIsMultipleFirePositionsContent);
                        if (weaponIsMultipleFirePositionsProp.boolValue)
                        {
                            GUILayout.BeginVertical(EditorStyles.helpBox);
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(weaponFirePositionContent);
                            int numWeaponFirePositions = weaponFirePositionListProp.arraySize;
                            if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                            {
                                weaponFirePositionListProp.arraySize += 1;
                                numWeaponFirePositions = weaponFirePositionListProp.arraySize;
                            }
                            if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                            {
                                if (numWeaponFirePositions > 1)
                                {
                                    weaponFirePositionListProp.arraySize -= 1;
                                    numWeaponFirePositions--;
                                }
                            }
                            GUILayout.EndHorizontal();

                            for (firePosIndex = 0; firePosIndex < numWeaponFirePositions; firePosIndex++)
                            {
                                EditorGUILayout.PropertyField(weaponFirePositionListProp.GetArrayElementAtIndex(firePosIndex));
                            }
                            GUILayout.EndVertical();
                        }
                        #endregion

                        EditorGUILayout.PropertyField(weaponFireDirectionProp, weaponFireDirectionContent);

                        #region Beam or Projectile prefabs
                        if (weaponTypeProp.intValue == Weapon.FixedBeamInt || weaponTypeProp.intValue == Weapon.TurretBeamInt)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(weaponBeamPrefabContent, GUILayout.Width(defaultEditorLabelWidth - 28f));
                            if (GUILayout.Button(gotoBeamFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.beamsFolder, false, true); }
                            EditorGUILayout.PropertyField(weaponBeamPrefabProp, GUIContent.none);
                            GUILayout.EndHorizontal();
                            EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("reloadTime"), weaponPowerUpTimeContent);
                            EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("maxRange"), weaponMaxRangeContent);
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(weaponProjectilePrefabContent, GUILayout.Width(defaultEditorLabelWidth - 28f));
                            if (GUILayout.Button(gotoProjectileFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.projectilesFolder, false, true); }
                            EditorGUILayout.PropertyField(weaponProjectilePrefabProp, GUIContent.none);
                            GUILayout.EndHorizontal();
                            EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("reloadTime"), weaponReloadTimeContent);
                        }
                        #endregion

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(weaponFiringButtonProp, weaponFiringButtonContent);
                        if (EditorGUI.EndChangeCheck() && weaponFiringButtonProp.intValue == (int)Weapon.FiringButton.AutoFire && weaponTypeProp.intValue != Weapon.TurretProjectileInt && weaponTypeProp.intValue != Weapon.TurretBeamInt)
                        {
                            weaponFiringButtonProp.intValue = 0;
                            Debug.LogWarning("Only turrets support auto-fire");
                        }

                        // Check LoS only applies to auto-fire turrets
                        if (weaponFiringButtonProp.intValue == (int)Weapon.FiringButton.AutoFire && (weaponTypeProp.intValue == Weapon.TurretProjectileInt || weaponTypeProp.intValue == Weapon.TurretBeamInt))
                        {
                            EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("checkLineOfSight"), weaponCheckLineOfSightContent);
                        }

                        // Beams auto-recharge. Ammo applies to projectile weapons
                        if (weaponTypeProp.intValue == Weapon.FixedBeamInt || weaponTypeProp.intValue == Weapon.TurretBeamInt)
                        {
                            EditorGUILayout.PropertyField(weaponChargeAmountProp, weaponChargeAmountContent);
                            EditorGUILayout.PropertyField(weaponRechargeTimeProp, weaponRechargeTimeContent);
                        }
                        else
                        {
                            bool unlimitedAmmo = weaponAmmunitionProp.intValue < 0;
                            unlimitedAmmo = EditorGUILayout.Toggle(weaponUnlimitedAmmoContent, unlimitedAmmo);
                            if (unlimitedAmmo && weaponAmmunitionProp.intValue != -1) { weaponAmmunitionProp.intValue = -1; }
                            else if (!unlimitedAmmo)
                            {
                                if (weaponAmmunitionProp.intValue < 0) { weaponAmmunitionProp.intValue = 10; }
                                EditorGUILayout.PropertyField(weaponAmmunitionProp, weaponAmmunitionContent);
                            }
                        }

                        #region Weapon Heat

                        weaponHeatLevelProp = weaponProp.FindPropertyRelative("heatLevel");
                        weaponOverHeatThresholdProp = weaponProp.FindPropertyRelative("overHeatThreshold");

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(weaponHeatLevelProp, weaponHeatLevelContent);
                        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                        {
                            shipControlModule.shipInstance.weaponList[index].SetHeatLevel(weaponHeatLevelProp.floatValue);
                        }
                        EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("heatUpRate"), weaponHeatUpRateContent);
                        EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("heatDownRate"), weaponHeatDownRateContent);
                        EditorGUILayout.PropertyField(weaponOverHeatThresholdProp, weaponOverHeatThresholdContent);
                        EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("isBurnoutOnMaxHeat"), weaponIsBurnoutOnMaxHeatContent);

                        // Fix any backward compatibility issues
                        if (weaponOverHeatThresholdProp.floatValue == 0f) { weaponOverHeatThresholdProp.floatValue = 80f; }

                        #endregion

                        #region Weapon Damage
                        bool useProgressiveDamage = weaponDamageRegionIndexProp.intValue > -1 && shipDamageModelProp.intValue != (int)Ship.ShipDamageModel.Simple;
                        if (shipDamageModelProp.intValue == (int)Ship.ShipDamageModel.Progressive)
                        {
                            useProgressiveDamage = EditorGUILayout.Toggle(partProgressiveDamageContent, useProgressiveDamage);
                            if (useProgressiveDamage) { weaponDamageRegionIndexProp.intValue = 0; }
                            else { weaponDamageRegionIndexProp.intValue = -1; }
                        }
                        else if (shipDamageModelProp.intValue == (int)Ship.ShipDamageModel.Localised)
                        {
                            weaponDamageRegionIndexProp.intValue = EditorGUILayout.IntPopup(partDamageRegionContent, weaponDamageRegionIndexProp.intValue, damageRegionIndexContents, damageRegionIndexInts);
                        }

                        if (useProgressiveDamage)
                        {
                            EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("startingHealth"), partStartingHealthContent);
                        }
                        #endregion

                        #region Turret Weapons
                        if (weaponTypeProp.intValue == Weapon.TurretProjectileInt || weaponTypeProp.intValue == Weapon.TurretBeamInt)
                        {
                            EditorGUILayout.PropertyField(weaponIsAutoTargetingEnabledProp, weaponIsAutoTargetingEnabledContent);
                            weaponturretPivotYProp = weaponProp.FindPropertyRelative("turretPivotY");

                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(weaponturretPivotYProp, weaponTurretPivotYContent);
                            if (EditorGUI.EndChangeCheck())
                            {
                                // Copy the local space pivot point to the relative location for quick initial setup
                                if (weaponturretPivotYProp.objectReferenceValue != null)
                                {
                                    weaponRelativePositionProp.vector3Value =  shipControlModule.transform.InverseTransformPoint(((Transform)weaponturretPivotYProp.objectReferenceValue).position);

                                    // Set the default fire direction. The turret may not be facing forwards on the ship, so adjust the direction accordingly.
                                    Quaternion turretPivotYRot = ((Transform)weaponturretPivotYProp.objectReferenceValue).rotation;
                                    // Subtract the ship rotation (multiple by inverse)
                                    weaponFireDirectionProp.vector3Value = turretPivotYRot * Quaternion.Inverse(shipControlModule.transform.rotation) * Vector3.forward;

                                    //weaponFireDirectionProp.vector3Value = Quaternion.Euler(shipControlModule.transform.InverseTransformDirection(turretPivotYRot.eulerAngles)) * Vector3.forward;
                                }
                            }
                            EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("turretPivotX"), weaponTurretPivotXContent);
                            EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("turretMinY"), weaponTurretMinYContent);
                            EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("turretMaxY"), weaponTurretMaxYContent);
                            EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("turretMinX"), weaponTurretMinXContent);
                            EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("turretMaxX"), weaponTurretMaxXContent);
                            EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("turretMoveSpeed"), weaponTurretMoveSpeedContent);
                            EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("inaccuracy"), weaponInaccuracyContent);
                            EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("turretReturnToParkInterval"), weaponTurretReturnToParkIntervalContent);

                            //EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("target"), new GUIContent("Target"));
                        }
                        #endregion

                        #region Fixed Projectile Weapons
                        else if (weaponTypeProp.intValue == Weapon.FixedProjectileInt)
                        {
                            if (weaponProjectilePrefabProp.objectReferenceValue != null && ((ProjectileModule)weaponProjectilePrefabProp.objectReferenceValue).isKinematicGuideToTarget)
                            {
                                EditorGUILayout.PropertyField(weaponIsAutoTargetingEnabledProp, weaponIsAutoTargetingEnabledContent);
                            }
                            // turn off Auto Targeting if the projectile doesn't support it
                            else if (weaponIsAutoTargetingEnabledProp.boolValue)
                            {
                                weaponIsAutoTargetingEnabledProp.boolValue = false;
                            }
                        }
                        #endregion

                        #region Fixed Beam weapons
                        else if (weaponTypeProp.intValue == Weapon.FixedBeamInt)
                        {
                            //EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("checkLineOfSight"), weaponAmmunitionContent);
                        }
                        #endregion
                    }
                    GUILayout.EndVertical();
                }

                #endregion

                #region Move/Insert/Delete Weapons

                if (weaponDeletePos >= 0 || weaponInsertPos >= 0 || weaponMoveDownPos >= 0)
                {
                    GUI.FocusControl(null);

                    // Don't permit multiple operations in the same pass
                    if (weaponMoveDownPos >= 0)
                    {
                        // Move down one position, or wrap round to start of list
                        if (weaponMoveDownPos < weaponListProp.arraySize - 1)
                        {
                            weaponListProp.MoveArrayElement(weaponMoveDownPos, weaponMoveDownPos + 1);
                        }
                        else { weaponListProp.MoveArrayElement(weaponMoveDownPos, 0); }

                        weaponMoveDownPos = -1;
                    }
                    else if (weaponInsertPos >= 0)
                    {
                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        shipControlModule.shipInstance.weaponList.Insert(weaponInsertPos, new Weapon(shipControlModule.shipInstance.weaponList[weaponInsertPos]));

                        // Read all properties from the ShipControlModule
                        serializedObject.Update();

                        // Hide original weapon
                        weaponListProp.GetArrayElementAtIndex(weaponInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                        weaponShowInEditorProp = weaponListProp.GetArrayElementAtIndex(weaponInsertPos).FindPropertyRelative("showInEditor");

                        // Force new weapon to be serialized in scene
                        weaponShowInEditorProp.boolValue = !weaponShowInEditorProp.boolValue;

                        // Show inserted duplicate weapon
                        weaponShowInEditorProp.boolValue = true;

                        weaponInsertPos = -1;

                        isSceneModified = true;
                    }
                    else if (weaponDeletePos >= 0)
                    {
                        // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and weaponDeletePos is reset to -1.
                        int _deleteIndex = weaponDeletePos;
                        if (EditorUtility.DisplayDialog("Delete Weapon " + (weaponDeletePos + 1) + "?", "Weapon " + (weaponDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the weapon from the list and cannot be undone.", "Delete Now", "Cancel"))
                        {
                            weaponListProp.DeleteArrayElementAtIndex(_deleteIndex);
                            weaponDeletePos = -1;
                        }
                    }

                    #if UNITY_2019_3_OR_NEWER
                    serializedObject.ApplyModifiedProperties();
                    // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                    if (!Application.isPlaying)
                    {
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                    GUIUtility.ExitGUI();
                    #endif
                }

                #endregion

                #endregion

                #region Identification
                EditorGUILayout.LabelField(identificationHeaderContent, helpBoxRichText);
                identityIsRadarEnabledProp = shipInstanceProp.FindPropertyRelative("isRadarEnabled");
                EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("factionId"), factionIdContent);
                EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("squadronId"), squadronIdContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(identityIsRadarEnabledProp, isRadarEnabledContent);
                if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                {
                    if (identityIsRadarEnabledProp.boolValue) { shipControlModule.EnableRadar(); }
                    else { shipControlModule.DisableRadar(); }
                }
                EditorGUILayout.PropertyField(shipInstanceProp.FindPropertyRelative("radarBlipSize"), radarBlipSizeContent);
                #endregion

                EditorGUILayout.EndVertical();
            }

            #endregion

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            #region Mark Scene Dirty if required

            if (isSceneModified && !Application.isPlaying)
            {
                isSceneModified = false;
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            #endregion

            #region Debug Mode
            // NOTE: This is NOT performance optimised - can create GC issues and other performance overhead.
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && shipControlModule != null)
            {
                SSCEditorHelper.PerformanceImpact();

                float rightLabelWidth = 150f;
                bool isShipInitialised = shipControlModule.IsInitialised;

                #region Debugging - General

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(isShipInitialised ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsShipEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(shipControlModule.ShipIsEnabled() ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsMovementEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(shipControlModule.ShipMovementIsEnabled() ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsVisbleToRadarEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(shipControlModule.IsVisbleToRadar? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                debugIsShowShipSpeedEnabled = EditorGUILayout.Toggle(debugIsShipSpeedShownContent, debugIsShowShipSpeedEnabled);
                if (debugIsShowShipSpeedEnabled)
                {
                    Vector3 shipVelo = isShipInitialised ? shipControlModule.shipInstance.LocalVelocity : Vector3.zero;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugShipSpeedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    if (isShipInitialised)
                    {
                        // z-axis of Local space velocity. Convert to km/h
                        EditorGUILayout.LabelField((shipVelo.z * 3.6f).ToString("0.0") + " (m/s " + shipVelo.z.ToString("0.0") + ")", GUILayout.MaxWidth(rightLabelWidth));
                    }
                    else
                    {
                        EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugShipVeloXContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField((isShipInitialised ? shipVelo.x.ToString("0.0") : "-"), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugShipVeloYContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField((isShipInitialised ? shipVelo.y.ToString("0.0") : "-"), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();
                }

                #endregion

                #region Debugging - Health
                debugIsShowHealth = EditorGUILayout.Toggle(debugIsShowHealthContent, debugIsShowHealth);
                if (debugIsShowHealth)
                {
                    EditorGUILayout.LabelField(debugMainRegionContent);
    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugRegionHealthContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(isShipInitialised ? shipControlModule.shipInstance.HealthNormalised.ToString("0.0 %") : "---", GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugUseShieldContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(shipControlModule.shipInstance.mainDamageRegion.useShielding ? "Yes" : "---", GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugRegionShieldContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(isShipInitialised ? shipControlModule.shipInstance.mainDamageRegion.ShieldNormalised.ToString("0.0 %") : "---", GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    if (isShipInitialised && shipControlModule.shipInstance.shipDamageModel == Ship.ShipDamageModel.Localised)
                    {
                        EditorGUILayout.LabelField(debugLocalRegionsContent);

                        int numDamageRegions = shipControlModule.shipInstance.localisedDamageRegionList == null ? 0 : shipControlModule.shipInstance.localisedDamageRegionList.Count;

                        SSCEditorHelper.DrawUILine(separatorColor, 2, 6);

                        for (int drIdx = 0; drIdx < numDamageRegions; drIdx++)
                        {
                            DamageRegion damageRegion = shipControlModule.shipInstance.localisedDamageRegionList[drIdx];

                            if (damageRegion != null)
                            {
                                EditorGUILayout.LabelField(" " + (drIdx + 1).ToString("00") + " " + damageRegion.name);

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(debugRegionHealthContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                                EditorGUILayout.LabelField(isShipInitialised ? damageRegion.HealthNormalised.ToString("0.0 %") : "---", GUILayout.MaxWidth(rightLabelWidth));
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(debugUseShieldContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                                EditorGUILayout.LabelField(damageRegion.useShielding ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(debugRegionShieldContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                                EditorGUILayout.LabelField(isShipInitialised ? damageRegion.ShieldNormalised.ToString("0.0 %") : "---", GUILayout.MaxWidth(rightLabelWidth));
                                EditorGUILayout.EndHorizontal();

                                SSCEditorHelper.DrawUILine(separatorColor, 2, 6);
                            }
                        }
                    }
                }
                #endregion

                #region Debugging - Thrusters
                debugIsShowThrusters = EditorGUILayout.Toggle(debugIsShowThrustersContent, debugIsShowThrusters);

                if (debugIsShowThrusters)
                {
                    int numThrusters = shipControlModule.shipInstance == null ? 0 : shipControlModule.shipInstance.thrusterList == null ? 0 : shipControlModule.shipInstance.thrusterList.Count;

                    bool isThrustersEnabled = isShipInitialised && shipControlModule.ShipIsEnabled();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsThrusterSystemsStartedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    //EditorGUILayout.LabelField(shipControlModule.IsThrusterSystemsStarted ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.LabelField(shipControlModule.ThrusterSystemsStatus, GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    for (int thIdx = 0; thIdx < numThrusters; thIdx++)
                    {
                        Thruster thruster = shipControlModule.shipInstance.thrusterList[thIdx];

                        if (thruster != null)
                        {
                            EditorGUILayout.LabelField(" " + (thIdx+1).ToString("00") + " " + thruster.name);

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(debugMaxThrustContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                            EditorGUILayout.LabelField((thruster.maxThrust/1000f).ToString(), GUILayout.MaxWidth(rightLabelWidth));
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(debugThrustInputContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                            EditorGUILayout.LabelField(isThrustersEnabled ? thruster.currentInput.ToString("0.000") : "---", GUILayout.MaxWidth(rightLabelWidth));
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                #endregion

                #region Debugging - Weapons

                debugIsShowWeapons = EditorGUILayout.Toggle(debugIsShowWeaponsContent, debugIsShowWeapons);

                if (debugIsShowWeapons)
                {
                    int numWeapons = shipControlModule.NumberOfWeapons;
                    if (numWeapons > 0)
                    {
                        for (int wpIdx = 0; wpIdx < numWeapons; wpIdx++)
                        {
                            Weapon weapon = shipControlModule.shipInstance.weaponList[wpIdx];

                            if (weapon != null)
                            {                                
                                EditorGUILayout.LabelField(" " + (wpIdx + 1).ToString("00") + " " + weapon.name);

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(debugWeaponTypeContent, GUILayout.Width(defaultEditorLabelWidth - 3f));
                                EditorGUILayout.LabelField(weapon.weaponType.ToString());
                                EditorGUILayout.EndHorizontal();

                                if (weapon.IsTurretWeapon)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(debugWeaponIsLockedOnContent, GUILayout.Width(defaultEditorLabelWidth - 3f));
                                    EditorGUILayout.LabelField(weapon.isLockedOnTarget ? "Yes" : "No");
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(debugWeaponHasLoSContent, GUILayout.Width(defaultEditorLabelWidth - 3f));
                                    EditorGUILayout.LabelField(weapon.HasLineOfSight ? "Yes" : "No");
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(debugWeaponIsParkedContent, GUILayout.Width(defaultEditorLabelWidth - 3f));
                                    EditorGUILayout.LabelField(weapon.IsParked ? "Yes" : "No");
                                    EditorGUILayout.EndHorizontal();

                                    GameObject targetGO = weapon.GetTarget();

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(debugWeaponTargetGOContent, GUILayout.Width(defaultEditorLabelWidth - 3f));
                                    EditorGUILayout.LabelField(targetGO != null ? targetGO.name : "---");
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField(" " + (wpIdx + 1).ToString("00") + " Weapon is null!");
                            }

                            SSCEditorHelper.DrawUILine(separatorColor, 2, 6);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(debugNoTurretsContent);
                    }
                }
                #endregion

                #region Debugging - Ship Input

                debugIsShowShipInput = EditorGUILayout.Toggle(debugIsShowShipInputContent, debugIsShowShipInput);

                if (debugIsShowShipInput)
                {
                    shipControlModule.GetShipInput(shipInput);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugHorizontalContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(shipInput.horizontal.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugVerticalContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(shipInput.vertical.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                     EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugLongitudinalContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(shipInput.longitudinal.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugPitchContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(shipInput.pitch.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugYawContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(shipInput.yaw.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugRollContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(shipInput.roll.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugPrimaryFireContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(shipInput.primaryFire.ToString(), GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugSecondaryFireContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(shipInput.secondaryFire.ToString(), GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugDockingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(shipInput.dock.ToString(), GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();
                }

                #endregion
            }
            EditorGUILayout.EndVertical();
            #endregion

            shipControlModule.allowRepaint = true;
        }

        #endregion
    }
}