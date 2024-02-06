using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;

// Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(StickyControlModule))]
    public class StickyControlModuleEditor : Editor
    {
        #region Custom Editor private variables
        private StickyControlModule stickyControlModule;
        private bool isStylesInitialised = false;
        private bool isSceneModified = false;
        // Formatting and style variables
        private string txtColourName = "Black";
        private Color defaultTextColour = Color.black;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle headingFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle foldoutStyleNoLabel;
        private GUIStyle toggleCompactButtonStyleNormal = null;  // Small Toggle button. e.g. G(izmo) on/off
        private GUIStyle toggleCompactButtonStyleToggled = null;

        private Color separatorColor = new Color();
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        private bool isDebuggingEnabled = false;
        private bool isShowVolume = true;
        private bool isShowHeadIKDirection = false;
        private bool isBoneDebugging = false;
        private bool isDamageRegionDebugging = false;
        private bool isShowGroundedIndicator = false;
        private Color groundedIndicatorColour = new Color();

        private int s3dFootstepMoveDownPos = -1;
        private int s3dFootstepInsertPos = -1;
        private int s3dFootstepDeletePos = -1;
        private bool isFootstepClipPlaying = false;

        private string[] surfaceTypeArray;
        private int s3dFootstepSurfaceTypeDeletePos = -1;
        private int s3dFootstepTerrainTextureDeletePos = -1;
        private int s3dFootstepAudioClipDeletePos = -1;

        private int s3dAnimActionMoveDownPos = -1;
        private int s3dAnimActionInsertPos = -1;
        private int s3dAnimActionDeletePos = -1;

        private int s3dAnimConditionMoveDownPos = -1;
        private int s3dAnimConditionInsertPos = -1;
        private int s3dAnimConditionDeletePos = -1;

        // Parameters for default character animator
        private List<S3DAnimParm> animParamsBoolList;
        private List<S3DAnimParm> animParamsTriggerList;
        private List<S3DAnimParm> animParamsFloatList;
        private List<S3DAnimParm> animParamsIntegerList;

        private string[] animParamBoolNames;
        private string[] animParamTriggerNames;
        private string[] animParamFloatNames;
        private string[] animParamIntegerNames;

        private List<S3DAnimLayer> animLayerList;
        private string[] animLayerNames;
        private List<S3DAnimTrans> animTransList;
        private string[] animTransNames;

        // Parameters for left and right hand VR animators
        private List<S3DAnimParm> animParamsFloatLHVRList;
        private List<S3DAnimParm> animParamsFloatRHVRList;

        private string[] animParamFloatLHVRNames;
        private string[] animParamFloatRHVRNames;

        [System.NonSerialized] private Transform headTrfm = null;
        [System.NonSerialized] private Transform leftFootTrfm = null;
        [System.NonSerialized] private Transform rightFootTrfm = null;

        private bool isHumanoid = false;

        private string[] interactiveTagNames = null;

        private int s3dRagdollBoneMoveDownPos = -1;
        private int s3dRagdollBoneInsertPos = -1;
        private int s3dRagdollBoneDeletePos = -1;

        private int s3dDmRgnMoveDownPos = -1;
        private int s3dDmRgnInsertPos = -1;
        private int s3dDmRgnDeletePos = -1;

        private int s3dEquipPtMoveDownPos = -1;
        private int s3dEquipPtInsertPos = -1;
        private int s3dEquipPtDeletePos = -1;
        [System.NonSerialized] private List<S3DHumanBone> rigHumanBoneList;

        private bool isDebugMoveExpanded = false;
        private bool isDebugLookExpanded = false;
        private bool isDebugCollideExpanded = false;
        private bool isDebugAnimateExpanded = false;
        private bool isDebugEngageExpanded = false;

        #endregion

        #region Static Strings

        #endregion

        #region SceneView Variables

        private bool isSceneDirtyRequired = false;

        private Quaternion sceneViewStickyRot = Quaternion.identity;
        private Vector3 componentHandlePosition = Vector3.zero;
        private Quaternion componentHandleRotation = Quaternion.identity;
        private float relativeHandleSize = 1f;

        // SceneView Hand variables
        private bool isLeftHandSelected = false;
        private bool isRightHandSelected = false;
        private Transform leftHandTrfm = null;
        private Transform rightHandTrfm = null;
        private Color fadedGizmoColour;

        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("This module enables you to control your character");
        private readonly static GUIContent[] tabTexts = { new GUIContent("Move"), new GUIContent("Look"), new GUIContent("Collide"), new GUIContent("Jet Pack"), new GUIContent("Animate"), new GUIContent("Engage") };

        #endregion

        #region GUIContent - General
        private readonly static GUIContent initialiseOnAwakeContent = new GUIContent(" Initialise on Awake", "If enabled, Initialise() will be called as soon as Awake() runs. This should be disabled if you want to control when the Sticky Control Module is enabled through code.");
        #endregion

        #region GUIContent - Move General
        private readonly static GUIContent isMoveGeneralExpandedContent = new GUIContent(" General Move Settings");
        private readonly static GUIContent isNPCContent = new GUIContent(" Non-Player Character", "Is this a non-player character (NPC)? Set when you want to drive input via code.");
        private readonly static GUIContent walkSpeedContent = new GUIContent(" Walk Speed", "The speed at which the character can walk in metres per second");
        private readonly static GUIContent sprintSpeedContent = new GUIContent(" Sprint Speed", "The speed at which the character can sprint or run in metres per second");
        private readonly static GUIContent strafeSpeedContent = new GUIContent(" Strafe Speed", "The speed at which the character can strafe left or right in metres per second");
        private readonly static GUIContent jumpSpeedContent = new GUIContent(" Jump Speed", "The initial speed added to the character when jumping in metres per second");
        private readonly static GUIContent jumpDelayContent = new GUIContent(" Jump Delay", "The number of seconds the character delays jumping upward. This is useful when a jump animation includes an initial movement when the feet are still on the ground.");
        private readonly static GUIContent crouchSpeedContent = new GUIContent(" Crouch Speed", "The speed at which the character can crouch or stand from a crouch");
        private readonly static GUIContent maxAccelerationContent = new GUIContent(" Max Acceleration", "The maximum character movement acceleration in metres per second per second");
        private readonly static GUIContent allowMovementInAirContent = new GUIContent(" Allow Movement in Air", "Whether movement is allowed while in the air (while jumping)");
        private readonly static GUIContent allowSprintBackwardContent = new GUIContent(" Allow Sprint Backward", "Can the character sprint or run backward?");
        private readonly static GUIContent gravitationalAccelerationContent = new GUIContent(" Gravity", "The gravitational acceleration, in metres per second per second, that acts downward for the character");
        private readonly static GUIContent arcadeFallMultiplierContent = new GUIContent(" Arcade Fall Multiplier", "This can give your character an arcade-like feel when falling due to gravity. Values > 0 will seem a little retro while values less than 0 will make them parachute or float to the ground.");
        private readonly static GUIContent maxStepOffsetContent = new GUIContent(" Max Step Offset", "The maximum height of an object the character can step up onto or over without jumping. It must be less than half the height of the character.");
        private readonly static GUIContent stepUpSpeedContent = new GUIContent(" Step Up Speed", "The speed at which the character rises up a step");
        private readonly static GUIContent stepUpBiasContent = new GUIContent(" Step Up Bias", "Dynamically changes the step-up speed while sprinting up steps. [DEFAULT: 1.0]");
        private readonly static GUIContent maxSlopeAngleContent = new GUIContent(" Max Slope Angle", "The maximum slope, in degrees, that the character can walk up");
        private readonly static GUIContent alignToGroundNormalContent = new GUIContent(" Align to Ground Normal", "Will the character's up direction attempt to align with the ground normal? If enabled, Vertical Rotation Rate will apply.");
        private readonly static GUIContent verticalRotationRateContent = new GUIContent(" Vertical Rotation Rate", "How quickly, in degrees per second, the character will attempt to match the target Up direction. Used for reference frame normal and ground normal matching.");
        private readonly static GUIContent turnRotationRateContent = new GUIContent(" Turn Rotation Rate", "How quickly, in degrees per second, the character will attempt to match the Free Look camera direction.");
        private readonly static GUIContent stuckTimeContent = new GUIContent(" Stuck Time", "The amount of time that needs to elapse before a stationary character is considered stuck. When the value is 0, a stationary character is never considered stuck.");
        private readonly static GUIContent stuckSpeedThresholdContent = new GUIContent(" Stuck Speed Threshold", "The maximum speed in m/sec the character can be moving before it can be considered stuck");
        private readonly static GUIContent moveUpdateTypeContent = new GUIContent(" Move Update Type", "The update loop or timing to use for moving the character.");
        #endregion

        #region GUIContent - Move Climbing
        private readonly static GUIContent isClimbingEnabledContent = new GUIContent(" Climbing", "Is the character able to climb walls?");
        private readonly static GUIContent climbSpeedContent = new GUIContent(" Climb Speed", "The speed at which the character can climb in metres per second");
        private readonly static GUIContent minClimbSlopeAngleContent = new GUIContent(" Min Slope Angle", "The minimum slope, in degrees, that the character can climb. Has no effect if less than the general walkable Min Slope Angle.");
        private readonly static GUIContent maxClimbSlopeAngleContent = new GUIContent(" Max Slope Angle", "The maximum slope, in degrees, that the character can climb");
        private readonly static GUIContent maxGrabDistanceContent = new GUIContent(" Max Grab Distance", "The maximum distance in-front of the character that they can reach out to grab a climbable surface when not climbing.");
        private readonly static GUIContent climbFaceSurfaceRateContent = new GUIContent(" Face Surface Rate", "The rate at which the character turns to face the surface they are climbing");
        private readonly static GUIContent climbTopDetectionContent = new GUIContent(" Top Detection", "Detect when the characters shoulders have reached the top of a climbable object. Shoulder height is set on the Collide tab.");
        private readonly static GUIContent climbableLayerMaskContent = new GUIContent(" Climbable Layer Mask", "Climbable obstacles or structures with a collider need to be in one of these layers.");

        #endregion

        #region GUIContent - Move Foot Steps
        private readonly static GUIContent isFootStepsEnabledContent = new GUIContent(" Footsteps", "Is the character using footstep sounds and/or effects?");
        private readonly static GUIContent s3dSurfacesContent = new GUIContent(" Known Surface Types", "A shared Scriptable Object in the Project containing a list of common surface types");
        private readonly static GUIContent leftFootContent = new GUIContent(" Left Foot", "The (child) left foot transform of the character");
        private readonly static GUIContent rightFootContent = new GUIContent(" Right Foot", "The (child) right foot transform of the character");
        private readonly static GUIContent footStepsUseMoveSpeedContent = new GUIContent(" Use Move Speed", "Rather than using foot placement, use the character moving speed");
        private readonly static GUIContent footStepWalkFrequencyContent = new GUIContent(" Walk Frequency", "This controls the relative foot step frequency assuming the walk speed is 1.0");
        private readonly static GUIContent footStepSprintFrequencyContent = new GUIContent(" Sprint Frequency", "This controls the relative foot step frequency assuming the sprint speed is 1.0");
        private readonly static GUIContent footStepsAudioContent = new GUIContent(" Audio Source", "The audio source containing the clips to play when the footsteps are used. Must be a child of the character gameobject.");
        private readonly static GUIContent footStepsNewAudioContent = new GUIContent("New", "Create a new child audio source on the character");
        private readonly static GUIContent footStepsListenContent = new GUIContent("L", "Listen to the clip play once. Press again to stop (any clip) playing.");
        private readonly static GUIContent footStepsDefaultClipContent = new GUIContent(" Default Footstep Sound", "The default footstep sound when the walking surface is unknown");
        private readonly static GUIContent footStepsVolumeContent = new GUIContent(" Overall Footstep Volume", "The overall volume of footsteps");
        private readonly static GUIContent fsMinVolumeContent = new GUIContent(" Min Volume", "The relative minimum volume of the audio clips");
        private readonly static GUIContent fsMaxVolumeContent = new GUIContent(" Max Volume", "The relative maximum volume of the audio clips");
        private readonly static GUIContent fsMinPitchContent = new GUIContent(" Min Pitch", "The minimum pitch of the audio clips");
        private readonly static GUIContent fsMaxPitchContent = new GUIContent(" Max Pitch", "The maximum pitch of the audio clips");
        private readonly static GUIContent fsMinWeightContent = new GUIContent(" Min Weight", "Minimum weight of terrain texture (0.01 to 1.0) at terrain position to register as a hit");

        #endregion

        #region GUIContent - Identification
        private readonly static GUIContent isIdentificationExpandedContent = new GUIContent(" Identification Settings");
        private readonly static GUIContent identFactionIdContent = new GUIContent(" Faction ID", "The faction or alliance the character belongs to. This can be used to identify if a character is friend or foe. Neutral = 0.");
        private readonly static GUIContent identModelIdContent = new GUIContent(" Model ID", "The type, category, or model of the character. Can be useful if you have a group of characters with similar attributes. For your own models, use numbers above 100 as numbers 1 to 99 are reserved for Sticky3D models.");

        #endregion

        #region GUIContent - Look General
        private readonly static GUIContent lookOnInitialiseContent = new GUIContent(" Look on Initialise", "Is look enabled when the module is first initialised? This will only take effect if the Look Camera is configured.");
        private readonly static GUIContent isThirdPersonContent = new GUIContent(" Third Person", "Is this in 3rd person controller mode?");
        private readonly static GUIContent isFreeLookContent = new GUIContent(" Free Look", "Is the free look mode enabled?");
        private readonly static GUIContent isLookGeneralExpandedContent = new GUIContent(" General Look Settings");
        private readonly static GUIContent lookTransformContent = new GUIContent(" Look Transform", "The parent transform used for look direction. Anything that should have its position or orientation modified by look direction should probably be parented to this transform");
        private readonly static GUIContent lookCamera1FirstPersonContent = new GUIContent(" Look Camera", "The main first person camera which is a child of the controller");
        private readonly static GUIContent lookCamera1FirstPersonNewContent = new GUIContent("New", "Create a new first person camera as a child of the controller");
        private readonly static GUIContent lookCameraAlignContent = new GUIContent("Align", "Set the Camera Offset using the camera's current position relative to the character in the scene");
        private readonly static GUIContent lookCamera1ThirdPersonContent = new GUIContent(" Look Camera", "The main third person camera which should not be a child of, or attached to, the controller");
        private readonly static GUIContent isAutoFirstPersonCameraHeightContent = new GUIContent(" Auto Camera Height", "Automatically adjust the 1st person camera to the average eye height, based on the height of the player");
        private readonly static GUIContent isLookCameraFollowHeadContent = new GUIContent(" Follow Head Position", "If the relative head bone position changes, the first person camera will move relative to it.");
        private readonly static GUIContent lookHorizontalSpeedContent = new GUIContent(" Horizontal Speed", "The speed or rate the character can look left or right");
        private readonly static GUIContent lookHorizontalDampingContent = new GUIContent(" Horizontal Damping", "The amount of damping applied when starting or stopping to look left or right");
        private readonly static GUIContent lookVerticalSpeedContent = new GUIContent(" Vertical Speed", "The speed or rate the character can look up or down");
        private readonly static GUIContent lookVerticalDampingContent = new GUIContent(" Vertical Damping", "The amount of damping applied when starting or stopping to look up or down");
        private readonly static GUIContent lookPitchUpLimitContent = new GUIContent(" Pitch Up Limit", "The pitch limit for look upward direction in degrees");
        private readonly static GUIContent lookPitchDownLimitContent = new GUIContent(" Pitch Down Limit", "The pitch limit for look downward direction in degrees");
        private readonly static GUIContent lookCameraOffsetContent = new GUIContent(" Camera Offset", "The camera offset or distance from the character when in third person mode");
        private readonly static GUIContent lookFocusOffsetContent = new GUIContent(" Focus Offset", "The local space point on the character where the third person camera focuses relative to the origin or pivot point of the character prefab");
        private readonly static GUIContent lookMoveSpeedContent = new GUIContent(" Camera Move Speed", "The speed at which the Third Person camera can adjust to the optimal position");
        private readonly static GUIContent lookZoomDurationContent = new GUIContent(" Zoom Duration", "The time, in seconds, to zoom fully in or out");
        private readonly static GUIContent lookUnzoomDelayContent = new GUIContent(" Unzoom Delay", "The delay, in seconds, before zoom starts to return to the non-zoomed position");
        private readonly static GUIContent lookUnzoomedFoVContent = new GUIContent(" Unzoomed FoV", "The camera field-of-view when no zoom is applied to the first person camera [Default 60]");
        private readonly static GUIContent lookZoomedFoVContent = new GUIContent(" Zoomed FoV", "The camera field-of-view when the first person camera is fully zoomed in [Default 10]");
        private readonly static GUIContent lookZoomOutFactorContent = new GUIContent(" Zoom Out Factor", "In third person, the relative amount the camera can zoom out. [Default 1]");
        private readonly static GUIContent lookOrbitDurationContent = new GUIContent(" Orbit Duration", "The time, in seconds, to fully orbit the character");
        private readonly static GUIContent lookUnorbitDelayContent = new GUIContent(" Unorbit Delay", "The delay, in seconds, before orbiting camera starts to return to the default position");
        private readonly static GUIContent lookOrbitDampingContent = new GUIContent(" Orbit Damping", "The amount of damping applied when starting or stopping camera orbit in third person");
        private readonly static GUIContent lookOrbitMinAngleContent = new GUIContent(" Orbit Min Angle", "The minimum anti-clockwise angle to rotate the camera around the character");
        private readonly static GUIContent lookOrbitMaxAngleContent = new GUIContent(" Orbit Max Angle", "The maximum clockwise angle to rotate the camera around the character");
        private readonly static GUIContent lookShowCursorContent = new GUIContent(" Show Cursor", "Show the screen cursor or mouse pointer");
        private readonly static GUIContent lookAutoHideCursorContent = new GUIContent(" Auto-hide Cursor", "Automatically hide the screen cursor or mouse pointer after it has been stationary for a fixed period of time. Automatically show the cursor if the mouse if moved.");
        private readonly static GUIContent lookHideCursorTimeContent = new GUIContent(" Hide Cursor Time", "The number of seconds to wait until after the cursor has not moved before hiding it");
        private readonly static GUIContent lookMaxLoSFoVContent = new GUIContent(" Max LoS Field-of-View", "Maximum Line-of-Sight Field-of-View");
        private readonly static GUIContent lookMaxShakeStrengthContent = new GUIContent(" Max Shake Strength", "The maximum strength of the third person camera shake. Smaller numbers are better.");
        private readonly static GUIContent lookMaxShakeDurationContent = new GUIContent(" Max Shake Duration", "The maximum duration (in seconds) the third person camera will shake per incident.");
        private readonly static GUIContent lookUpdateTypeContent = new GUIContent(" Update Type", "The update loop or timing to use for moving or rotate the camera.");
        //private readonly static GUIContent lookFocusToleranceContent = new GUIContent(" Focus Tolerance", "The distance the focus point can move before the third person camera reacts");
        #endregion

        #region GUIContent - Look Clip Objects
        private readonly static GUIContent clipObjectsContent = new GUIContent(" Clip Objects", "Adjust the camera position to attempt to avoid the camera flying through objects between the character and the camera. This has performance overhead, so disable if not needed.");
        private readonly static GUIContent minClipMoveSpeedContent = new GUIContent(" Minimum Move Speed", "The minimum speed the camera will move to avoid flying through objects between the character and the camera. High values make clipping more effective. Lower values will make it smoother.");
        private readonly static GUIContent clipMinDistanceContent = new GUIContent(" Minimum Distance", "The minimum distance the camera can be from the character position.");
        private readonly static GUIContent clipMinOffsetXContent = new GUIContent(" Minimum Offset X", "The minimum offset on the x-axis the camera can be from the character when object clipping. This should be less than or equal to the Camera Offset X value.");
        private readonly static GUIContent clipMinOffsetYContent = new GUIContent(" Minimum Offset Y", "The minimum offset on the y-axis the camera can be from the character when object clipping. This should be less than or equal to the Camera Offset Y value.");
        private readonly static GUIContent clipResponsivenessContent = new GUIContent(" Responsiveness", "The responsiveness to changes in the clipping distance. When 1.0, it will depend on the Min Move Speed and the Camera Move Speed. Reducing the responsiveness can stabilise clipping when the character is moving erratically or on a vehicle that is.");
        private readonly static GUIContent clipObjectMaskContent = new GUIContent(" Clip Object Layers", "Only attempt to clip objects in the following Unity Layers");
        #endregion

        #region GUIContent - Look Interactive
        private readonly static GUIContent isLookInteractiveEnabledContent = new GUIContent(" Interactive", "Can the character see or detect objects with a StickyInteractive component?");
        private readonly static GUIContent lookMaxInteractiveDistanceContent = new GUIContent(" Max Distance", "When Look Interactive is enabled, how far away from the camera can the character see objects with a StickyInteractive component?");
        private readonly static GUIContent lookInteractiveLockToCameraContent = new GUIContent(" Lock to Camera", "Instead of using the mouse or cursor position, always look in the direction the camera is facing");
        private readonly static GUIContent lookInteractiveLayerMaskContent = new GUIContent(" Layer Mask", "The non-trigger colliders and characters in these Unity layers can be seen when Look Interactive is enabled");
        private readonly static GUIContent isUpdateLookingAtPointContent = new GUIContent(" Update Looking Point", "When true, GetLookingAtPoint is updated. This is the point in world space where the user is currently aiming or targeting. It could be an interactive-enabled object.");
        #endregion

        #region GUIContent - Look Sockets
        private readonly static GUIContent isLookSocketsEnabledContent = new GUIContent(" Sockets", "Can the character see or detect objects with a StickySocket component?");
        private readonly static GUIContent lookMaxSocketDistanceContent = new GUIContent(" Max Distance", "When Look Sockets is enabled, how far away from the camera can the character see objects with a StickySocket component?");
        private readonly static GUIContent lookSocketLockToCameraContent = new GUIContent(" Lock to Camera", "Instead of using the mouse or cursor position, always look in the direction the camera is facing");
        private readonly static GUIContent lookSocketLayerMaskContent = new GUIContent(" Layer Mask", "The trigger colliders in these Unity layers can be seen when Look Sockets is enabled");
        private readonly static GUIContent socketActiveMaterialContent = new GUIContent(" Active Material", "The (custom) material used to highlight a socket in the scene. If not set, one will be created at runtime.");
        private readonly static GUIContent isLookSocketAutoShowContent = new GUIContent(" Auto Show", "Attempt to automatically show (and hide) the highlighter when looking a socket");
        #endregion

        #region GUIConent - Look VR
        private readonly static GUIContent isLookVRContent = new GUIContent(" Look VR Mode", "Is the VR mode enabled? This only works when Sticky Input Module is set to UnityXR.");
        private readonly static GUIContent isMatchHumanHeightVRContent = new GUIContent(" Match Human Height", "When Look VR is enabled, the character Height will be modified to match the approximate height of the human player based on the starting head-mounted device position above the floor.");
        private readonly static GUIContent isRoomScaleVRContent = new GUIContent(" Room Scale", "The VR Head Mounted Device (HMD) drives character motion.");
        private readonly static GUIContent humanPostureVRContent = new GUIContent(" Human Posture", "The posture or starting position of the human player when wearing a VR head-mounted device.");
        private readonly static GUIContent isSnapTurnVRContent = new GUIContent(" Snap Turn", "When enabled with UnityXR input, left and right turn will be incremented by the Snap Turn Amount");
        private readonly static GUIContent snapTurnDegreesContent = new GUIContent(" Snap Turn Amount", "The number of degrees turned with each snap movement.");
        private readonly static GUIContent snapTurnIntervalTimeContent = new GUIContent(" Snap Turn Interval", "The minimum amount of time required between snap turns");

        #endregion

        #region GUIContent - Collide
        private readonly static GUIContent heightContent = new GUIContent(" Height", "The height of the character collider in metres");
        private readonly static GUIContent radiusContent = new GUIContent(" Radius", "The radius of the character collider in metres");
        private readonly static GUIContent pivotToCentreOffsetYContent = new GUIContent(" Pivot to Centre Y", "The distance, in the up direction, from the pivot point to the centre of the model. If the pivot point is at the feet, this will be half the height.");
        private readonly static GUIContent shoulderHeightContent = new GUIContent(" Shoulder Height", "The height of the shoulders. Men typically have a shoulder height 0.82 x height, while females are 0.81 x the height.");
        private readonly static GUIContent shoulderHeightCalcContent = new GUIContent("Calc", "Calculate the shoulder height based on the height of the character");
        private readonly static GUIContent crouchHeightNormalisedContent = new GUIContent(" Crouch Height", "The height of the character when crouching");
        private readonly static GUIContent maxSweepIterationsContent = new GUIContent(" Max Sweep Iterations", "The maximum number of sweep iterations allowed per frame");
        private readonly static GUIContent sweepToleranceContent = new GUIContent(" Sweep Tolerance", "The tolerance allowed for sweeps and grounded checks in metres");
        private readonly static GUIContent collisionLayerMaskContent = new GUIContent(" Collision Layer Mask", "The layer mask used for collision testing for the character");
        private readonly static GUIContent isOnTriggerEnterContent = new GUIContent(" On Trigger Enter/Exit", "Call OnTriggerEnter or Exit when character enters or exits a Trigger Collider. This, or Trigger Collider, must be enabled if using StickyZones.");
        private readonly static GUIContent isOnTriggerStayContent = new GUIContent(" On Trigger Stay", "Call OnTriggerStay EVERY FRAME while the character is inside a Trigger Collider");
        private readonly static GUIContent isTriggerColliderEnabledContent = new GUIContent(" Trigger Collider", "Rather than disabling the capsule collider at runtime, it is converted to a trigger collider so that it can be detected by raycasts. Character will be pushed by other moving objects that have colliders.");
        private readonly static GUIContent isReactToStickyZonesEnabledContent = new GUIContent(" React to Sticky Zones", "When entering or exiting a StickyZone, allow configuration changes based on zone settings");
        private readonly static GUIContent interactionLayerMaskContent = new GUIContent(" Interaction Layer Mask", "The character will attempt to interact with dynamic rigidbodies in these layers. Default: Nothing");
        private readonly static GUIContent referenceFrameLayerMaskContent = new GUIContent(" Reference Layer Mask", "The Unity Layers used to test if the object under the character is a suitable Reference Frame");
        private readonly static GUIContent referenceUpdateTypeContent = new GUIContent(" Reference Update Type", "Determines how the reference frame is updated. Manual = via your code or a StickyZone. See manual for details.");
        private readonly static GUIContent initialReferenceFrameContent = new GUIContent(" Initial Ref Frame", "Initial or default reference frame transform the character will stick to.");
        private readonly static GUIContent isUseRBodyReferenceFrameContent = new GUIContent(" Use RBody Ref Frame", "When Reference Update Type is Automatic or AutoFirst, when detecting a collider that is attached to or a child of a rigidbody, the reference frame will be set to the transform of the rigidbody rather than the transform of the collider.");

        #endregion

        #region GUIContent - JetPack
        private readonly static GUIContent isJetPackAvailableContent = new GUIContent(" Is Available", "Is the Jet Pack feature selectable by the player?");
        private readonly static GUIContent isJetPackEnabledContent = new GUIContent(" Is Enabled", "Is the Jet Pack currently engaged?");
        private readonly static GUIContent jetPackFuelLevelContent = new GUIContent(" Fuel Level", "The amount of fuel available to power the Jet Pack");
        private readonly static GUIContent jetPackFuelBurnRateContent = new GUIContent(" Fuel Burn Rate", "The rate fuel is consumed per second. If rate is 0, fuel is unlimited");
        private readonly static GUIContent jetPackSpeedContent = new GUIContent(" Max Speed", "Maximum speed the jet pack can propel the character");
        private readonly static GUIContent jetPackMaxAccelerationContent = new GUIContent(" Max Acceleration", "The maximum jet pack acceleration in metres per second per second.");
        private readonly static GUIContent jetPackDampingContent = new GUIContent(" Damping Force", "The inertia damping force applied to slow down movement when no input is received");
        private readonly static GUIContent jetPackAudioContent = new GUIContent(" Audio Source", "The audio source containing the clip to play when the jetpack is used. Must be a child of the character gameobject.");
        private readonly static GUIContent jetPackNewAudioContent = new GUIContent("New", "Create a new child jetpack audio source on the character");
        private readonly static GUIContent jetPackRampUpDurationContent = new GUIContent(" Ramp Up Duration", "The number of seconds it takes for this jet pack to go from minimum to maximum power.");
        private readonly static GUIContent jetPackRampDownDurationContent = new GUIContent(" Ramp Down Duration", "The number of seconds it takes for this jet pack to go from maximum to minimum power.");
        private readonly static GUIContent jetPackHealthContent = new GUIContent(" Health of Jet Pack", "Health value of the jet pack. 0 = no health, 100 = full health. A damage jet pack will burn the same amount of fuel but will produce less thrust.");
        private readonly static GUIContent jetPackThrustersContent = new GUIContent(" Thrusters", "Expand to show or configure the particle effects used when the jet pack is in use");
        private readonly static GUIContent jetPackThrusterFwdContent = new GUIContent(" Push Forward", "The parent gameobject for the backward-facing (particle) thruster effect for the jet pack. Must be a child of the character gameobject.");
        private readonly static GUIContent jetPackThrusterBackContent = new GUIContent(" Push Backward", "The parent gameobject for the forward-facing (particle) thruster effect for the jet pack. Must be a child of the character gameobject.");
        private readonly static GUIContent jetPackThrusterUpContent = new GUIContent(" Push Up", "The parent gameobject for the downward-facing (particle) thruster effect for the jet pack. Must be a child of the character gameobject.");
        private readonly static GUIContent jetPackThrusterDownContent = new GUIContent(" Push Down", "The parent gameobject for the upward-facing (particle) thruster effect for the jet pack. Must be a child of the character gameobject.");
        private readonly static GUIContent jetPackThrusterRightContent = new GUIContent(" Push Right", "The parent gameobject for the left-facing (particle) thruster effect for the jet pack. Must be a child of the character gameobject.");
        private readonly static GUIContent jetPackThrusterLeftContent = new GUIContent(" Push Left", "The parent gameobject for the right-facing (particle) thruster effect for the jet pack. Must be a child of the character gameobject.");
        private readonly static GUIContent jetPackMinEffectsRateContent = new GUIContent(" Min Effects Rate", "The 0.0-1.0 value that indicates the minimum normalised amount of any particle effects that are applied when a non-zero jet pack input is received. Default is 0. If the full particle emission rate should be applied when any input is received, set the value to 1.0.");
        private readonly static GUIContent jetPackAlignBtnContent = new GUIContent("Auto", "Automatically position the effect.");
        #endregion

        #region GUIContent - Animate General
        private readonly static GUIContent aaExportJsonContent = new GUIContent("Export Json", "Export Animation Actions to a json file");
        private readonly static GUIContent aaImportJsonContent = new GUIContent("Import Json", "Import (and overwrite) Animation Actions from a json file. Does not change the Animator Controller.");
        private readonly static GUIContent aaRefreshParamsContent = new GUIContent(" Refresh", "Refresh the Parameter from the Animator Controller");
        private readonly static GUIContent defaultAnimatorContent = new GUIContent(" Animator", "The animator that will control animations for this character");
        #endregion

        #region GUIContent - Animate Aim IK
        private readonly static GUIContent aimIKContent = new GUIContent(" Aim IK", "Inverse Kinematic settings used when aiming a weapon");
        private readonly static GUIContent isAimIKWhenNotAimingContent = new GUIContent(" When Not Aiming", "If a weapon is held, will it always attempt to face the target using Aim IK? Otherwise, it will only do this when weapon IsAiming is true.");
        private readonly static GUIContent aimIKCameraOffsetTPSContent = new GUIContent(" Aim Camera Offset TPS", "The camera offset or distance from the character when in third person weapon aiming mode.");
        private readonly static GUIContent aimIKAnimIKPassLayerIndexContent = new GUIContent(" Anim IK Pass Layer", "The zero-based layer in the animator controller that has the IK Pass enabled for Aim IK. If this layer (not layer 0) has an avatar mask for fingers, it needs to be above the layer than includes the character aim animation. e.g., aim ik layer = 1 and layer with animations is 2.");
        private readonly static GUIContent aimIKDownLimitContent = new GUIContent(" Aim Down Limit", "The maximum rotation, in degrees, each bone for Aim IK can pitch down.");
        private readonly static GUIContent aimIKUpLimitContent = new GUIContent(" Aim Up Limit", "The maximum rotation, in degrees, each bone for Aim IK can pitch up.");
        private readonly static GUIContent aimIKLeftLimitContent = new GUIContent(" Aim Left Limit", "The maximum rotation, in degrees, the held weapon can point left.");
        private readonly static GUIContent aimIKRightLimitContent = new GUIContent(" Aim Right Limit", "The maximum rotation, in degrees, the held weapon can point right.");
        private readonly static GUIContent aimIkTurnRotationRateContent = new GUIContent(" Aim Turn-Look Rate", "How quickly, in degrees per second, the character will attempt to turn (or look) toward the aim target.");
        private readonly static GUIContent aimIKBoneWeightFPSContent = new GUIContent(" Bone Weight FPS", "The overall bone weight when holding a weapon in first-person. Adjust this if the bones pitch up or down at a different rate to the first-person camera. This can be most noticeable when holding a weapon but not aiming it and Free Look is disabled.");
        private readonly static GUIContent aimIKBonesContent = new GUIContent(" Aim Bones", "The (spine) bones used when aiming at a target");

        #endregion

        #region GUIContent - Animate Foot IK
        private readonly static GUIContent isFootIKContent = new GUIContent(" Foot IK", "Are the feet placed on the ground using Inverse Kinematics for Humanoid rigged characters?");
        private readonly static GUIContent footIKAnimIKPassLayerIndexContent = new GUIContent(" Anim IK Pass Layer", "The zero-based layer in the animator controller that has the IK Pass enabled for Foot IK");
        private readonly static GUIContent footIKCurveParmsContent = new GUIContent(" Foot IK Weight Curve Parameters", "The names of the float parameters used for foot IK blending");
        private readonly static GUIContent footIKLCurveParmContent = new GUIContent("  Left Foot Weight param", "The name of the float parameter in the animator controller used for left foot IK blending");
        private readonly static GUIContent footIKRCurveParmContent = new GUIContent("  Right Foot Weight param", "The name of the float parameter in the animator controller used for right foot IK blending");
        private readonly static GUIContent footIKBodyMoveSpeedYContent = new GUIContent(" Body Move Speed Y", "The rate at which the character's y-axis position is adjusted for foot IK placement");
        private readonly static GUIContent footIKSlopeReactRateContent = new GUIContent(" Slope React Rate", "The rate at which the foot rotates to match the slope");
        private readonly static GUIContent footIKSlopeToleranceContent = new GUIContent(" Slope Tolerance", "The minimum slope that is required before the feet will be rotated to match the slope under the character. This can allow the character to use foot rotations from the animation when standing or moving on relatively flat surfaces.");
        private readonly static GUIContent footIKToeDistanceContent = new GUIContent(" Toe Distance", "The distance in metres, in the forward direction, from the foot bone to the end of the toes or the front of the foot.");
        private readonly static GUIContent footIKPositionOnlyContent = new GUIContent(" Adjust Position Only", "Only adjust the position of the feet. Do not modify the rotation.");
        private readonly static GUIContent footIKMaxInwardRotationXContent = new GUIContent(" Max Foot Inward Roll", "The maximum rotation, in degrees, the foot can roll or rotate inward");
        private readonly static GUIContent footIKMaxOutwardRotationXContent = new GUIContent(" Max Foot Outward Roll", "The maximum rotation, in degrees, the foot can roll or rotate outward");
        private readonly static GUIContent footIKMaxPitchZContent = new GUIContent(" Max Foot Pitch", "The maximum rotation, in degrees, the foot can pitch forward or back");
        #endregion

        #region GUIContent - Animate Head IK
        private readonly static GUIContent isHeadIKContent = new GUIContent(" Head IK", "The head can follow a target within defined constaints");
        private readonly static GUIContent headIKAnimIKPassLayerIndexContent = new GUIContent(" Anim IK Pass Layer", "The zero-based layer in the animator controller that has the IK Pass enabled for Head IK");
        private readonly static GUIContent headIKMoveMaxSpeedContent = new GUIContent(" Head Move Speed", "The rate at which the character's head turns toward the target position");
        private readonly static GUIContent headIKMoveDampingContent = new GUIContent(" Head Move Damping", "The amount of damping to apply when starting or stopping to turn the character's head toward a target position.");
        private readonly static GUIContent headIKLookDownLimitContent = new GUIContent(" Look Down Limit", "The maximum rotation, in degrees, the head can be tilted down");
        private readonly static GUIContent headIKLookUpLimitContent = new GUIContent(" Look Up Limit", "The maximum rotation, in degrees, the head can be tilted up");
        private readonly static GUIContent headIKLookLeftLimitContent = new GUIContent(" Look Left Limit", "The maximum rotation, in degrees, the head can look left");
        private readonly static GUIContent headIKLookRightLimitContent = new GUIContent(" Look Right Limit", "The maximum rotation, in degrees, the head can look right");
        private readonly static GUIContent headIKEyesWeightContent = new GUIContent(" Eyes Weight", "How much the eyes are used to look towards the target");
        private readonly static GUIContent headIKHeadWeightContent = new GUIContent(" Head Weight", "How much the head is used to look towards the target");
        private readonly static GUIContent headIKBodyWeightContent = new GUIContent(" Body Weight", "How much the body is used to look towards the target");
        private readonly static GUIContent headIKLookAtEyesContent = new GUIContent(" Look at Eyes", "When the Head IK target is a character, look toward their eyes");
        private readonly static GUIContent headIKLookAtInteractiveContent = new GUIContent(" Look at Interactive", "When look interactive and Update Looking Point are enabled in the Look tab, while the character is stationary, should the head face either the interactive enabled object being looked at, or in the direction the character is looking?");
        private readonly static GUIContent headIKWhenClimbingContent = new GUIContent(" When Climbing", "If Head IK is enabled, the head (also) turns towards the target while climbing");
        private readonly static GUIContent headIKAdjustForVelocityContent = new GUIContent(" Adjust for Velocity", "When the character or the target is moving quickly this may help the head adjust quickly to the rapidly changing positions [DEFAULT: OFF]");
        private readonly static GUIContent headIKWhenMovementDisabledContent = new GUIContent(" When Move Disabled", "If Head IK is enabled, the head can turn towards the target while the character movement is disabled. For example, when the character is seated.");
        private readonly static GUIContent headIKConsiderBehindContent = new GUIContent(" Consider Behind", "Will the character's look direction be affected if the target is behind them?");

        #endregion

        #region GUIContent - Animate Hand IK
        private readonly static GUIContent isHandIKContent = new GUIContent(" Hand IK", "Are the hands moved using Inverse Kinematics for Humanoid rigged characters?");
        private readonly static GUIContent isHandIKLeftContent = new GUIContent(" Left Hand");
        private readonly static GUIContent isHandIKRightContent = new GUIContent(" Right Hand");
        private readonly static GUIContent handIKAnimIKPassLayerIndexContent = new GUIContent(" Anim IK Pass Layer", "The zero-based layer in the animator controller that has the IK Pass enabled for Hand IK");
        private readonly static GUIContent handIKMoveMaxSpeedContent = new GUIContent(" Hand Move Speed", "The maximum rate at which the character's hands move toward the target position.");
        private readonly static GUIContent handIKLeftRadiusContent = new GUIContent(" Hand Radius", "How close an object must be to a hand before it is considered to be touching it.");
        private readonly static GUIContent handIKLeftHandSettingsContent = new GUIContent("  Constraint Settings", "Determines which axis are used to constrain hand bone rotation");
        private readonly static GUIContent handIKRightHandSettingsContent = new GUIContent("  Constraint Settings", "Determines which axis are used to constrain hand bone rotation");
        private readonly static GUIContent handIKZoneColourContent = new GUIContent(" Gizmo Colour", "The colour of the gizmos shown in the scene view at runtime");
        private readonly static GUIContent handIKWhenMovementDisabledContent = new GUIContent(" When Move Disabled", "The hands can move while the character movement is disabled. For example, when the character is seated or stationary.");
        private readonly static GUIContent leftHandPalmOffsetContent = new GUIContent("  Palm Offset", "The local space offset the palm, or centre, of the hand is from the left hand bone position");
        private readonly static GUIContent leftHandPalmRotationContent = new GUIContent("  Palm Rotation", "The local space left hand palm rotation from the hand bone stored in degrees. The palm normal, stored as a rotation.");
        private readonly static GUIContent handIKLHMaxInRotContent = new GUIContent("  Max In Rotation", "The Hand IK left hand (wrist) max rotation (to the right) in degrees");
        private readonly static GUIContent handIKLHMaxOutRotContent = new GUIContent("  Max Out Rotation", "The Hand IK left hand (wrist) max rotation (to the left) in degrees");
        private readonly static GUIContent handIKLHMaxUpRotContent = new GUIContent("  Max Up Rotation", "The Hand IK left hand (wrist) max rotation up in degrees");
        private readonly static GUIContent handIKLHMaxDownRotContent = new GUIContent("  Max Down Rotation", "The Hand IK left hand (wrist) max rotation down in degrees");
        private readonly static GUIContent handIKLHInwardLimitContent = new GUIContent("  Inward Limit", "The Hand IK left hand inward movement (to the right) limit in degrees. Has no effect when aiming a weapon.");
        private readonly static GUIContent handIKLHOutwardLimitContent = new GUIContent("  Outward Limit", "The Hand IK left hand outward movement (to the left) limit in degrees. Currently the left hand cannot reach behind the character. Has no effect when aiming a weapon.");
        private readonly static GUIContent handIKLHMaxReachDistContent = new GUIContent("  Max Reach Dist.", "The maximum distance the left hand will attempt to reach for an object.");
        private readonly static GUIContent handIKLHElbowHintContent = new GUIContent("  Elbow Hint", "Left Elbow hint transform for Hand IK. Should be approx 0.25 directly behind elbow.");
        private readonly static GUIContent rightHandPalmOffsetContent = new GUIContent("  Palm Offset", "The local space offset the palm, or centre, of the hand is from the right hand bone position");
        private readonly static GUIContent rightHandPalmRotationContent = new GUIContent("  Palm Rotation", "The local space right hand palm rotation from the hand bone stored in degrees. The palm normal, stored as a rotation.");
        private readonly static GUIContent handIKRHMaxInRotContent = new GUIContent("  Max In Rotation", "The Hand IK right hand (wrist) max rotation (to the right) in degrees");
        private readonly static GUIContent handIKRHMaxOutRotContent = new GUIContent("  Max Out Rotation", "The Hand IK right hand (wrist) max rotation (to the left) in degrees");
        private readonly static GUIContent handIKRHMaxUpRotContent = new GUIContent("  Max Up Rotation", "The Hand IK right hand (wrist) max rotation up in degrees");
        private readonly static GUIContent handIKRHMaxDownRotContent = new GUIContent("  Max Down Rotation", "The Hand IK right hand (wrist) max rotation down in degrees");
        private readonly static GUIContent handIKRHInwardLimitContent = new GUIContent("  Inward Limit", "The Hand IK right hand inward movement (to the left) limit in degrees. Has no effect when aiming a weapon.");
        private readonly static GUIContent handIKRHOutwardLimitContent = new GUIContent("  Outward Limit", "The Hand IK right hand outward movement (to the right) limit in degrees. Currently the right hand cannot reach behind the character. Has no effect when aiming a weapon.");
        private readonly static GUIContent handIKRHMaxReachDistContent = new GUIContent("  Max Reach Dist.", "The maximum distance the right hand will attempt to reach for an object.");
        private readonly static GUIContent handIKRHElbowHintContent = new GUIContent("  Elbow Hint", "Right Elbow hint transform for Hand IK. Should be approx 0.25 directly behind elbow.");


        #endregion

        #region GUIContent - Animate Hand VR

        private readonly static GUIContent isHandVRContent = new GUIContent(" Hand VR", "Are the hands animated using VR controller input? The Input Mode must be UnityXR.");
        private readonly static GUIContent handVRLHAnimatorContent = new GUIContent(" Left Hand Animator", "The animator used to animate a Virtual Reality left hand");
        private readonly static GUIContent handVRRHAnimatorContent = new GUIContent(" Right Hand Animator", "The animator used to animate a Virtual Reality left hand");

        private readonly static GUIContent handVRGripParmNameContent = new GUIContent("  Grip Parameter", "The (float) parameter name from the animation controller for the grip action");
        private readonly static GUIContent handVRTriggerParmNameContent = new GUIContent("  Trigger Parameter", "The (float) parameter name from the animation controller for the trigger action");

        #endregion

        #region GUIContent - Animate Ragdoll
        private readonly static GUIContent ragdollContent = new GUIContent(" Ragdoll", "Ragdoll settings");
        private readonly static GUIContent ragdollGenerateContent = new GUIContent("Generate", "Add ragdoll features to this character");
        private readonly static GUIContent ragdollGetBonesContent = new GUIContent("Get Bones", "Attempt to automatically find the bones to be used for the ragdoll");
        private readonly static GUIContent ragdollRemoveContent = new GUIContent("Remove", "Remove (delete) ragdoll features from this character");
        private readonly static GUIContent ragdollTestContent = new GUIContent("Test", "Test the ragdoll at runtime");
        private readonly static GUIContent rdBoneTransformContent = new GUIContent("Bone Transform", "The transform of a bone in the humanoid rig");
        #endregion

        #region GUIContent - Animate Root Motion
        private readonly static GUIContent isRootMotionContent = new GUIContent(" Root Motion", "Is Animation Root Motion used to influence character position and rotation?");
        private readonly static GUIContent isRootMotionTurningContent = new GUIContent(" Anim Drives Turning", "Do animations drive turn left or right? Input can be sent to the animator controller via Anim Actions, and S3D reads the rotation from the avatar.");
        private readonly static GUIContent rootMotionIdleThresholdContent = new GUIContent(" Idle Threshold", "When root motion is enabled, velocity below this level will be considered idle. Some idle animations move the character around a little. Increasing this value will make IsIdle more stable. Default: 0.0001.");
        #endregion

        #region GUIContent - Animate Actions
        private readonly static GUIContent animateActionsContent = new GUIContent(" Actions", "The animate actions that interact with your animation controller");
        private readonly static GUIContent aaStandardActionContent = new GUIContent(" Standard Action", "This is the action that happens that causes the animation to take place. It helps you remember why you set it up.");
        private readonly static GUIContent aaParmTypeContent = new GUIContent(" Parameter Type", "The type of animation parameter, if any, used with this action");
        private readonly static GUIContent aaParmNamesContent = new GUIContent(" Parameter Name", "The parameter name from the animation controller that applies to this action");
        private readonly static GUIContent aaBoolValueContent = new GUIContent(" Bool Value", "The realtime value from the Sticky3D Controller that will be sent to the model's animation controller");
        private readonly static GUIContent aaFloatValueContent = new GUIContent(" Float Value", "The realtime value from the Sticky3D Controller that will be sent to the model's animation controller");
        private readonly static GUIContent aaTriggerValueContent = new GUIContent(" Trigger Value", "The realtime value from the Sticky3D Controller that will be sent to the model's animation controller");
        private readonly static GUIContent aaIntegerValueContent = new GUIContent(" Integer Value", "The realtime value from the Sticky3D Controller that will be sent to the model's animation controller");
        private readonly static GUIContent aaFloatMultiplierContent = new GUIContent(" Float Multiplier", "A value that is used to multiple or change the value of the float value being passed to the animation controller. Can speed up or slow down an animation.");
        private readonly static GUIContent aaFloatFixedValueContent = new GUIContent(" Fixed Value", "The fixed float value being passed to the animation controller.");
        private readonly static GUIContent aaBoolFixedValueContent = new GUIContent(" Fixed Value", "True or False being passed to the animation controller.");
        private readonly static GUIContent aaDampingContent = new GUIContent(" Damping", "The damping applied to help smooth transitions, especially with Blend Trees. Currently only used for floats. For quick transitions to the new float value use a low damping value, for the slower transitions use more damping.");
        private readonly static GUIContent aaCustomValueContent = new GUIContent("Custom Input or user game code");
        private readonly static GUIContent aaIsResetCustomAfterUseContent = new GUIContent(" Reset After Use", "Works with bool custom anim actions to reset to false after it has been sent to the animator controller. Has no effect if Toggle is true. [Default: True]");
        private readonly static GUIContent aaIsInvertContent = new GUIContent(" Invert", "When the value is true, use false instead. When the value is false, use true instead. Not compatible with Toggle");
        private readonly static GUIContent aaIsToggleContent = new GUIContent(" Toggle", "Works with bool custom anim actions to toggle the existing parameter value in the animator controller. Not compatible with Invert or Reset After Use.");
        private readonly static GUIContent aaTransNamesContent = new GUIContent(" Transition Name", "The transition name from the animation controller that applies to this action");
        private readonly static GUIContent aaNoneContent = new GUIContent(" None Available", "No data available");
        private readonly static GUIContent conditionsInfoContent = new GUIContent("Data is only sent to your animator when the conditions are true");
        #endregion

        #region GUIContent - Engage
        private readonly static GUIContent isEngageGeneralExpandedContent = new GUIContent(" General Engage Settings");
        private readonly static GUIContent isEngageDamageRegionsExpandedContent = new GUIContent(" Damage Region Settings");
        private readonly static GUIContent isEngageEventsExpandedContent = new GUIContent(" Event Settings");
        private readonly static GUIContent isEngageEquipExpandedContent = new GUIContent(" Equip Settings");
        private readonly static GUIContent isEngageRespawnExpandedContent = new GUIContent(" Respawn Settings");
        private readonly static GUIContent isEngageStashExpandedContent = new GUIContent(" Stash Settings");
        private readonly static GUIContent storeMaxSelectableInSceneContent = new GUIContent(" Max Selectable in Scene", "The number of Interactive-enabled object that can be selected in the scene at the same time.");
        private readonly static GUIContent engageColourContent = new GUIContent(" Engage Colour", "The colour associated with engaging with an interactive-enabled object in the scene. Typically used by the StickyDisplayModule reticle when is hovering over an object. See Events, On Look At Changed");
        private readonly static GUIContent nonengageColourContent = new GUIContent(" Non-engage Colour", "The colour associated with not engaging with an interactive-enabled object in the scene. Typically used by the StickyDisplayModule reticle when not hovering over an object. See Events, On Look At Changed");
        private readonly static GUIContent interactiveTagsContent = new GUIContent(" Interactive Tags", "The Scriptable Object containing a list of 32 tags. You should only need one per project. They help to identify which interactive objects can be added to particular Equip Points. To create custom tags, in the Project pane, click Create->Sticky3D->Interactive Tags.");
        private readonly static GUIContent lassoSpeedContent = new GUIContent(" Lasso Speed", "The rate at which an interactive object is pulled toward or pushed away from a character when interacting with objects. Currently works with socketed objects. When 0, objects are instantly snapped into position.");

        private readonly static GUIContent healthContent = new GUIContent(" Character Health", "Overall health value of the character. 0 = no health, 100 = full health. Health will affect the speed the character can move.");
        private readonly static GUIContent damageRegionGenerateContent = new GUIContent("Generate Humanoid Regions", "Attempt to generate humanoid damage regions.");
        private readonly static GUIContent damageRegionRemoveContent = new GUIContent("Remove Regions", "Attempt to remove (delete) the humanoid damage regions.");
        private readonly static GUIContent damageRegionNameContent = new GUIContent(" Name", "The name of this damage region.");
        private readonly static GUIContent damageRegionStartHealthContent = new GUIContent(" Starting Health", "The starting health value of this damage region.");
        private readonly static GUIContent damageRegionInvincibleContent = new GUIContent(" Is Invincible", "When invincible, it will not take damage however its health can still be manually decreased.");
        private readonly static GUIContent damageRegionUseShieldingContent = new GUIContent(" Use Shielding", "Whether this damage region uses shielding. Up until a point, shielding protects the damage region from damage.");
        private readonly static GUIContent damageRegionShieldingDamageThresholdContent = new GUIContent(" Damage Threshold", "Damage below this value will not affect the shield or the damage region's health while the shield is still active (i.e. until the shield has absorbed damage more than or equal to the Shielding Amount value from damage events above the damage threshold).");
        private readonly static GUIContent damageRegionShieldingAmountContent = new GUIContent(" Shield Amount", "How much damage the shield can absorb before it ceases to protect the damage region from damage.");
        private readonly static GUIContent damageRegionMultiplierContent = new GUIContent(" Damage Multipliers");
        private readonly static GUIContent damageRegionMultiplierDescContent = new GUIContent("The relative amount of damage a Type A-F projectile or beam will inflict on the character. When the Damage Type is Default, the damage multipliers are ignored.");
        private readonly static GUIContent typeADamageMultiplierContent = new GUIContent(" Damage Type A", "The relative amount of damage a Type A projectile or beam will inflict on the character.");
        private readonly static GUIContent typeBDamageMultiplierContent = new GUIContent(" Damage Type B", "The relative amount of damage a Type B projectile or beam will inflict on the character.");
        private readonly static GUIContent typeCDamageMultiplierContent = new GUIContent(" Damage Type C", "The relative amount of damage a Type C projectile or beam will inflict on the character.");
        private readonly static GUIContent typeDDamageMultiplierContent = new GUIContent(" Damage Type D", "The relative amount of damage a Type D projectile or beam will inflict on the character.");
        private readonly static GUIContent typeEDamageMultiplierContent = new GUIContent(" Damage Type E", "The relative amount of damage a Type E projectile or beam will inflict on the character.");
        private readonly static GUIContent typeFDamageMultiplierContent = new GUIContent(" Damage Type F", "The relative amount of damage a Type F projectile or beam will inflict on the character.");
        private readonly static GUIContent shieldingRechargeRateContent = new GUIContent(" Recharge Rate", "The rate per second that a shield will recharge (default = 0)");
        private readonly static GUIContent shieldingRechargeDelayContent = new GUIContent(" Recharge Delay", "The delay, in seconds, between when damage occurs to a shield and it begins to recharge.");
        private readonly static GUIContent damageRegionCollisionDamageResistanceContent = new GUIContent(" Col. Damage Resistance", "Value indicating the resistance of the damage region to damage caused by collisions. Increasing this value will decrease the amount of damage caused to the damage region by collisions.");
        private readonly static GUIContent damageRegionColliderContent = new GUIContent(" Collider", "The collider used for this damage region");
        private readonly static GUIContent damageRegionTransformContent = new GUIContent(" Bone Transform", "The transform used for this damage region");
        private readonly static GUIContent onInitialisedEvtDelayContent = new GUIContent("  On Initialised Event Delay", "The number of seconds to delay firing the onInitialised event methods after the S3D character has been initialised.");
        private readonly static GUIContent onInitialisedContent = new GUIContent("On Initialised", "These are triggered by a S3D character after the character is initialised");
        private readonly static GUIContent onDestroyedContent = new GUIContent("On Destroyed", "These are triggered when the character is destroyed or when it reaches 0 health.");
        private readonly static GUIContent onInteractLookAtChangedContent = new GUIContent("On Look At Changed", "These are triggered by a S3D character when they start or stop looking at an interactive-enabled object in the scene.");
        private readonly static GUIContent onPreStartAimContent = new GUIContent("On Pre Start Aim", "These are triggered by a S3D character immediately before they start aiming a weapon");
        private readonly static GUIContent onPostStartAimContent = new GUIContent("On Post Start Aim", "These are triggered by a S3D character immediately after they start aiming a weapon");
        private readonly static GUIContent onPreStopAimContent = new GUIContent("On Pre Stop Aim", "These are triggered by a S3D character immediately before they stop aiming a weapon");
        private readonly static GUIContent onPostStopAimContent = new GUIContent("On Post Stop Aim", "These are triggered by a S3D character immediately after they stop aiming a weapon");
        private readonly static GUIContent onPreStartHoldWeaponContent = new GUIContent("On Pre Start Hold Weapon", "These are triggered by a S3D character immediately before they start holding a weapon");
        private readonly static GUIContent onPostStopHoldWeaponContent = new GUIContent("On Post Stop Hold Weapon", "These are triggered by a S3D character immediately after they stop holding a weapon");
        private readonly static GUIContent onRespawnedContent = new GUIContent("On Respawned", "These are triggered when the character has just been respawned");
        private readonly static GUIContent onRespawningContent = new GUIContent("On Respawning", "These are triggered when the character is about to be respawned");

        private readonly static GUIContent startLivesContent = new GUIContent(" Start Lives", "The number of the lives the character has when it is initialised");
        private readonly static GUIContent maxLivesContent = new GUIContent(" Max Lives", "The maximum number of lives that the character can have.");
        private readonly static GUIContent unlimitedLivesContent = new GUIContent(" Unlimited Lives", "The character never runs out of lives");
        private readonly static GUIContent respawnModeContent = new GUIContent(" Respawn Mode", "How respawning happens if the character has unlimited or at least 1 spare life");
        private readonly static GUIContent respawnTimeContent = new GUIContent(" Respawn Time", "How long the respawning process takes (in seconds). Only relevant when Respawn Mode is not set to Dont Respawn or Ragdoll on Destroy is enabled.");
        private readonly static GUIContent customRespawnPositionContent = new GUIContent(" Respawn Position", "Where the character respawns from in world space when Respawn Mode is set to Respawn From Specified Position.");
        private readonly static GUIContent customRespawnRotationContent = new GUIContent(" Respawn Rotation", "The Euler rotation angles the character respawns from in world space when Respawn Mode is set to Respawn From Specified Position.");
        private readonly static GUIContent isDropHeldOnDestroyContent = new GUIContent(" Drop Held on Destroy", "Should the character attempt to drop any held interactive objects when health reaches 0?");
        private readonly static GUIContent isDropEquipOnDestroyContent = new GUIContent(" Drop Equip on Destroy", "Should the character attempt to drop any equipped interactive objects when health reaches 0?");
        private readonly static GUIContent isDropStashOnDestroyContent = new GUIContent(" Drop Stash on Destroy", "Should the character attempt to drop any stashed interactive objects when health reaches 0?");
        private readonly static GUIContent isRagdollOnDestroyContent = new GUIContent(" Ragdoll on Destroy", "Should the ragdoll be enabled when health reaches 0?");

        private readonly static GUIContent engageEquipContent = new GUIContent("For placing visible inactive interactive objects on the body of a character. Typically, items will be parented to a humanoid bone.");
        private readonly static GUIContent equipPointNameContent = new GUIContent(" Name", "A description of the location or purpose for the equip point. e.g., right weapon holster");
        private readonly static GUIContent equipPointTransformContent = new GUIContent(" Parent Transform", "The transform, typically a bone, that will the objects will be parented to.");
        private readonly static GUIContent equipPointRelativeOffsetContent = new GUIContent(" Relative Offset", "The local space offset from the parent transform.");
        private readonly static GUIContent equipPointRelativeRotationContent = new GUIContent(" Relative Rotation", "The local space rotation, in Euler angles, from the parent transform.");
        private readonly static GUIContent equipPointGetBoneContent = new GUIContent("B", "Get the transform of a humanoid bone on this character");
        private readonly static GUIContent equipPointMaxItemsContent = new GUIContent(" Max Items", "The maximum number of items to equip at this point");
        private readonly static GUIContent minEquipDropDistanceContent = new GUIContent(" Min Drop Distance", "The minimum distance on the local x or z axis an equipped object can be dropped from the character capsule collider");
        private readonly static GUIContent maxEquipDropDistanceContent = new GUIContent(" Max Drop Distance", "The maximum distance on the local x or z axis an equipped object can be dropped from the character capsule collider");
        private readonly static GUIContent equipPointPermittedTagsContent = new GUIContent(" Permitted Tags", "The interactive-enabled objects that are permitted to be attached to this Equip Point. See the S3DInteractiveTags scriptableobject.");


        private readonly static GUIContent stashParentContent = new GUIContent(" Stash Parent", "The child transform under which all items are stashed");
        private readonly static GUIContent minStashDropDistanceContent = new GUIContent(" Min Drop Distance", "The minimum distance on the local x or z axis a stashed object can be dropped from the character capsule collider");
        private readonly static GUIContent maxStashDropDistanceContent = new GUIContent(" Max Drop Distance", "The maximum distance on the local x or z axis a stashed object can be dropped from the character capsule collider");

        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent(" Debug Mode", "Use this to troubleshoot the data at runtime in the editor.");
        private readonly static GUIContent debugIsMoveExpandedContent = new GUIContent(" Debug Move", "Use this to troubleshoot move data at runtime in the editor.");
        private readonly static GUIContent debugIsLookExpandedContent = new GUIContent(" Debug Look", "Use this to troubleshoot look data at runtime in the editor.");
        private readonly static GUIContent debugIsCollideExpandedContent = new GUIContent(" Debug Collide", "Use this to troubleshoot collide data at runtime in the editor.");
        private readonly static GUIContent debugIsAnimateExpandedContent = new GUIContent(" Debug Animate", "Use this to troubleshoot animate data at runtime in the editor.");
        private readonly static GUIContent debugIsEngageExpandedContent = new GUIContent(" Debug Engage", "Use this to troubleshoot engage data at runtime in the editor.");
        private readonly static GUIContent debugShowVolumeContent = new GUIContent(" Show Volume", "Draw the approximate volume of the character in the scene view when in Play mode");
        private readonly static GUIContent debugShowGroundedContent = new GUIContent(" Show Grounded", "Draw a disc when the character is grounded in the scene view when in Play mode");
        private readonly static GUIContent debugShowHeadIKDirectionContent = new GUIContent(" Show Head IK", "Draw a line indicating the direction the Head IK is using in the scene view when in Play mode");
        private readonly static GUIContent debugNotSetContent = new GUIContent("--", "not set");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent(" Is Initialised?");
        private readonly static GUIContent debugIsLookEnabledContent = new GUIContent(" Is Look Enabled?");
        private readonly static GUIContent debugIsLookMovementEnabledContent = new GUIContent(" Is Look Movement Enabled?");
        private readonly static GUIContent debugIsMovementEnabledContent = new GUIContent(" Is Movement Enabled?");
        private readonly static GUIContent debugIsPositionLockedContent = new GUIContent(" Is Position Locked?");
        private readonly static GUIContent debugIsGroundedContent = new GUIContent(" Is Grounded?");
        private readonly static GUIContent debugIsSteppingContent = new GUIContent(" Is Stepping?");
        private readonly static GUIContent debugIsWalkingContent = new GUIContent(" Is Walking?");
        private readonly static GUIContent debugIsSprintingContent = new GUIContent(" Is Sprinting?");
        private readonly static GUIContent debugIsStrafingLeftContent = new GUIContent(" Is Strafing Left?");
        private readonly static GUIContent debugIsStrafingRightContent = new GUIContent(" Is Strafing Right?");
        private readonly static GUIContent debugIsCrouchingContent = new GUIContent(" Is Crouching?");
        private readonly static GUIContent debugIsClimbingContent = new GUIContent(" Is Climbing?");
        private readonly static GUIContent debugCurrentHeightContent = new GUIContent(" Current Height");
        private readonly static GUIContent debugCrouchAmountContent = new GUIContent(" Crouch Amount");
        private readonly static GUIContent debugIsThirdPersonContent = new GUIContent(" Is 3rd Person?");
        private readonly static GUIContent debugIsFreeLookContent = new GUIContent(" Is Free Look?");
        private readonly static GUIContent debugSpeedContent = new GUIContent(" Speed (km/h)");
        private readonly static GUIContent debugTurningSpeedContent = new GUIContent(" Turning Speed");
        private readonly static GUIContent debugZoomAmountContent = new GUIContent(" Zoom Amount");
        private readonly static GUIContent debugGroundSlopeContent = new GUIContent(" Last Ground Slope");
        private readonly static GUIContent debugIsLookFixedUpdateContent = new GUIContent(" Is Look Fixed Update?");
        private readonly static GUIContent debugIsLockCamToWorldPosContent = new GUIContent(" Is Lock Cam World Pos?");
        private readonly static GUIContent debugIsLockCamToWorldRotContent = new GUIContent(" Is Lock Cam World Rot?");

        private readonly static GUIContent debugIsLookFollowHeadContent = new GUIContent(" Is Look Follow Head?");
        private readonly static GUIContent debugIsLookFollowHeadTPContent = new GUIContent(" Is Look Follow Head TP?");
        private readonly static GUIContent debugIsLookInteractiveEnabledContent = new GUIContent(" Is Look Interactive?");
        private readonly static GUIContent debugIsLookingAtInteractiveContent = new GUIContent("  Looking At");
        private readonly static GUIContent debugIsLookingAtPointContent = new GUIContent("  Focusing On");
        private readonly static GUIContent debugIsLookSocketsEnabledContent = new GUIContent(" Is Look Sockets?");
        private readonly static GUIContent debugNumSelectedInteractiveContent = new GUIContent(" Selected Interactives");
        private readonly static GUIContent debugLHInteractiveContent = new GUIContent(" Held in Left Hand");
        private readonly static GUIContent debugRHInteractiveContent = new GUIContent(" Held in Right Hand");
        private readonly static GUIContent debugNumStashedItemsContent = new GUIContent(" Num Stashed Items");

        private readonly static GUIContent debugIsLookVREnabledContent = new GUIContent(" Is Look VR Enabled?");

        private readonly static GUIContent debugIsAnimateEnabledContent = new GUIContent(" Is Animate Enabled?");

        private readonly static GUIContent debugIsAimEnabledContent = new GUIContent(" Is Aim Enabled?");

        private readonly static GUIContent debugIsHeadIKEnabledContent = new GUIContent(" Is Head IK Enabled?");
        private readonly static GUIContent debugHeadIKTargetTrfmContent = new GUIContent("  Target Transform");
        private readonly static GUIContent debugHeadIKTargetPosContent = new GUIContent("  Target Position");

        private readonly static GUIContent debugIsHandIKEnabledContent = new GUIContent(" Is Hand IK Enabled?");
        private readonly static GUIContent debugCurrentMaxHandIKWeightContent = new GUIContent("  Current Max IK Weight");
        private readonly static GUIContent debugLeftHandIKTargetTrfmContent = new GUIContent("  LH Target Transform");
        private readonly static GUIContent debugLeftHandIKTargetPosContent = new GUIContent("  LH Target Position");
        private readonly static GUIContent debugLeftHandIKPrevPosContent = new GUIContent("  LH Previous Position");
        private readonly static GUIContent debugRightHandIKTargetTrfmContent = new GUIContent("  RH Target Transform");
        private readonly static GUIContent debugRightHandIKTargetPosContent = new GUIContent("  RH Target Position");
        private readonly static GUIContent debugRightHandIKPrevPosContent = new GUIContent("  RH Previous Position");

        private readonly static GUIContent debugIsFootIKEnabledContent = new GUIContent(" Is Foot IK Enabled?");
        private readonly static GUIContent debugLeftFootIKPosContent = new GUIContent("  Left Foot IK Pos");
        private readonly static GUIContent debugRightFootIKPosContent = new GUIContent("  Right Foot IK Pos");

        private readonly static GUIContent debugCurrentReferenceFrameContent = new GUIContent(" Current Reference Frame");
        private readonly static GUIContent debugBoneDebuggingContent = new GUIContent(" Bone Debugging", "Show some bone-related values");

        private readonly static GUIContent debugAnimatorNotSetContent = new GUIContent("  Animator not configured");
        private readonly static GUIContent debugLeftFootPosWSContent = new GUIContent("  Left Foot WS Position");
        private readonly static GUIContent debugLeftFootToSoleContent = new GUIContent("  Left Foot to Sole");
        private readonly static GUIContent debugRightFootPosWSContent = new GUIContent("  Right Foot WS Position");
        private readonly static GUIContent debugRightFootToSoleContent = new GUIContent("  Right Foot to Sole");
        private readonly static GUIContent debugHeadNameContent = new GUIContent("  Head Bone");

        private readonly static GUIContent debugDmRegDebuggingContent = new GUIContent(" Damage Region Debugging", "Show some damage region-related values");
        private readonly static GUIContent debugDmRegLivesContent = new GUIContent("  Lives");
        private readonly static GUIContent debugDmRegHealthContent = new GUIContent("  Health");
        private readonly static GUIContent debugDmRegNameContent = new GUIContent("  Name");
        private readonly static GUIContent debugDmRegShieldHealthContent = new GUIContent("  Shield Health");
        #endregion

        #region Serialized Properties - General
        private SerializedProperty selectedTabIntProp;
        #endregion

        #region Serialized Properties - Move General
        private SerializedProperty initialiseOnAwakeProp;
        private SerializedProperty isNPCProp;
        private SerializedProperty isMoveGeneralExpandedProp;
        private SerializedProperty walkSpeedProp;
        private SerializedProperty sprintSpeedProp;
        private SerializedProperty strafeSpeedProp;
        private SerializedProperty jumpSpeedProp;
        private SerializedProperty jumpDelayProp;
        private SerializedProperty crouchSpeedProp;
        private SerializedProperty maxAccelerationProp;
        private SerializedProperty maxSlopeAngleProp;
        private SerializedProperty allowMovementInAirProp;
        private SerializedProperty allowSprintBackwardProp;
        private SerializedProperty gravitationalAccelerationProp;
        private SerializedProperty arcadeFallMultiplierProp;
        private SerializedProperty maxStepOffsetProp;
        private SerializedProperty stepUpSpeedProp;
        private SerializedProperty stepUpBiasProp;
        private SerializedProperty alignToGroundNormalProp;
        private SerializedProperty verticalRotationRateProp;
        private SerializedProperty turnRotationRateProp;
        private SerializedProperty stuckTimeProp;
        private SerializedProperty stuckSpeedThresholdProp;
        private SerializedProperty moveUpdateTypeProp;
        #endregion

        #region Serialized Properties - Move Climbing
        private SerializedProperty isClimbingEnabledProp;
        private SerializedProperty isClimbingExpandedProp;
        private SerializedProperty climbSpeedProp;
        private SerializedProperty minClimbSlopeAngleProp;
        private SerializedProperty maxClimbSlopeAngleProp;
        private SerializedProperty maxGrabDistanceProp;
        private SerializedProperty climbFaceSurfaceRateProp;
        private SerializedProperty climbTopDetectionProp;
        private SerializedProperty climbableLayerMaskProp;
        #endregion

        #region Serialized Properties - Move Footsteps
        private SerializedProperty isFootStepsEnabledProp;
        private SerializedProperty s3dSurfacesProp;
        private SerializedProperty leftFootProp;
        private SerializedProperty rightFootProp;
        private SerializedProperty footStepsUseMoveSpeedProp;
        private SerializedProperty footStepsAudioProp;
        private SerializedProperty footStepsDefaultClipProp;
        private SerializedProperty footStepsVolumeProp;
        private SerializedProperty footStepWalkFrequencyProp;
        private SerializedProperty footStepSprintFrequencyProp;
        private SerializedProperty isS3DFootstepExpandedProp;
        private SerializedProperty isS3DFootstepListExpandedProp;
        private SerializedProperty s3dFootstepListProp;
        private SerializedProperty s3dFSShowInEditorProp;
        private SerializedProperty s3dFSMinVolumeProp;
        private SerializedProperty s3dFSMaxVolumeProp;
        private SerializedProperty s3dFSMinPitchProp;
        private SerializedProperty s3dFSMaxPitchProp;
        private SerializedProperty s3dFootstepProp;
        private SerializedProperty s3dFSSurfaceTypeListProp;
        private SerializedProperty s3dFSSurfaceTypeProp;
        private SerializedProperty s3dFSTerrainTextureListProp;
        private SerializedProperty s3dFSTerrainTextureProp;
        private SerializedProperty s3dFSAudioClipListProp;
        private SerializedProperty s3dFSAudioClipProp;

        #endregion

        #region Serialized Properties - Look General
        private SerializedProperty lookOnInitialiseProp;
        private SerializedProperty isThirdPersonProp;
        private SerializedProperty isFreeLookProp;
        private SerializedProperty isLookVRProp;
        private SerializedProperty isLookGeneralExpandedProp;
        private SerializedProperty lookFirstPersonTransform1Prop;
        private SerializedProperty lookFirstPersonCamera1Prop;
        private SerializedProperty lookThirdPersonCamera1Prop;
        private SerializedProperty showLookCamOffsetGizmosInSceneViewProp;
        private SerializedProperty showLookFocusOffsetGizmosInSceneViewProp;
        private SerializedProperty isAutoFirstPersonCameraHeightProp;
        private SerializedProperty isLookCameraFollowHeadProp;
        private SerializedProperty lookVerticalSpeedProp;
        private SerializedProperty lookHorizontalSpeedProp;
        private SerializedProperty lookHorizontalDampingProp;
        private SerializedProperty lookVerticalDampingProp;
        private SerializedProperty lookPitchUpLimitProp;
        private SerializedProperty lookPitchDownLimitProp;
        private SerializedProperty lookCameraOffsetProp;
        private SerializedProperty lookFocusOffsetProp;
        private SerializedProperty lookMoveSpeedProp;
        private SerializedProperty lookShowCursorProp;
        private SerializedProperty lookAutoHideCursorProp;
        private SerializedProperty lookHideCursorTimeProp;
        private SerializedProperty lookZoomDurationProp;
        private SerializedProperty lookUnzoomDelayProp;
        private SerializedProperty lookUnzoomedFoVProp;
        private SerializedProperty lookZoomedFoVProp;
        private SerializedProperty lookZoomOutFactorProp;
        private SerializedProperty lookOrbitDurationProp;
        private SerializedProperty lookUnorbitDelayProp;
        private SerializedProperty lookOrbitDampingProp;
        private SerializedProperty lookOrbitMinAngleProp;
        private SerializedProperty lookOrbitMaxAngleProp;
        private SerializedProperty lookMaxLoSFoVProp;
        private SerializedProperty lookMaxShakeStrengthProp;
        private SerializedProperty lookMaxShakeDurationProp;
        private SerializedProperty lookUpdateTypeProp;
        //private SerializedProperty lookFocusToleranceProp;
        #endregion

        #region Serialized Properties - Look Clip Objects
        private SerializedProperty clipObjectsProp;
        private SerializedProperty isLookClipObjectsExpandedProp;
        private SerializedProperty minClipMoveSpeedProp;
        private SerializedProperty clipMinDistanceProp;
        private SerializedProperty clipMinOffsetXProp;
        private SerializedProperty clipMinOffsetYProp;
        private SerializedProperty clipResponsivenessProp;
        private SerializedProperty clipObjectMaskProp;
        #endregion

        #region Serialized Properties - Look Interactive
        private SerializedProperty isLookInteractiveExpandedProp;
        private SerializedProperty isLookInteractiveEnabledProp;
        private SerializedProperty lookMaxInteractiveDistanceProp;
        private SerializedProperty lookInteractiveLockToCameraProp;
        private SerializedProperty lookInteractiveLayerMaskProp;
        private SerializedProperty isUpdateLookingAtPointProp;
        #endregion

        #region Serialized Properties - Look Sockets
        private SerializedProperty isLookSocketsExpandedProp;
        private SerializedProperty isLookSocketsEnabledProp;
        private SerializedProperty lookMaxSocketDistanceProp;
        private SerializedProperty lookSocketLockToCameraProp;
        private SerializedProperty lookSocketLayerMaskProp;
        private SerializedProperty socketActiveMaterialProp;
        private SerializedProperty isLookSocketAutoShowProp;
        #endregion

        #region Serialized Properties - Look VR
        private SerializedProperty isLookVRExpandedProp;
        private SerializedProperty isMatchHumanHeightVRProp;
        private SerializedProperty isRoomScaleVRProp;
        private SerializedProperty humanPostureVRProp;
        private SerializedProperty isSnapTurnVRProp;
        private SerializedProperty snapTurnDegreesProp;
        private SerializedProperty snapTurnIntervalTimeProp;
        #endregion

        #region Serialized Properties - Collide
        private SerializedProperty heightProp;
        private SerializedProperty radiusProp;
        private SerializedProperty pivotToCentreOffsetYProp;
        private SerializedProperty shoulderHeightProp;
        private SerializedProperty crouchHeightNormalisedProp;
        private SerializedProperty maxSweepIterationsProp;
        private SerializedProperty sweepToleranceProp;
        private SerializedProperty isOnTriggerEnterEnabledProp;
        private SerializedProperty isOnTriggerStayEnabledProp;
        private SerializedProperty isTriggerColliderEnabledProp;
        private SerializedProperty collisionLayerMaskProp;
        private SerializedProperty isReactToStickyZonesEnabledProp;
        private SerializedProperty interactionLayerMaskProp;
        private SerializedProperty referenceFrameLayerMaskProp;
        private SerializedProperty referenceUpdateTypeProp;
        #endregion

        #region Serialized Properties - JetPack
        private SerializedProperty isJetPackAvailableProp;
        private SerializedProperty isJetPackEnabledProp;
        private SerializedProperty jetPackFuelLevelProp;
        private SerializedProperty jetPackFuelBurnRateProp;
        private SerializedProperty jetPackSpeedProp;
        private SerializedProperty jetPackMaxAccelerationProp;
        private SerializedProperty jetPackDampingProp;
        private SerializedProperty jetPackAudioProp;
        private SerializedProperty jetPackRampUpDurationProp;
        private SerializedProperty jetPackRampDownDurationProp;
        private SerializedProperty jetPackHealthProp;

        private SerializedProperty isJetPackThrusterListExpandedProp;
        private SerializedProperty jetPackThrusterFwdProp;
        private SerializedProperty jetPackThrusterBackProp;
        private SerializedProperty jetPackThrusterUpProp;
        private SerializedProperty jetPackThrusterDownProp;
        private SerializedProperty jetPackThrusterRightProp;
        private SerializedProperty jetPackThrusterLeftProp;
        private SerializedProperty jetPackMinEffectsRateProp;

        #endregion

        #region Serialized Properties - Animate
        private SerializedProperty defaultAnimatorProp;

        // Aim IK
        private SerializedProperty isAimIKWhenNotAimingProp;
        private SerializedProperty aimIKCameraOffsetTPSProp;
        private SerializedProperty aimIKAnimIKPassLayerIndexProp;
        private SerializedProperty aimBonesProp;
        private SerializedProperty isAimIKExpandedProp;
        private SerializedProperty aimIKDownLimitProp;
        private SerializedProperty aimIKUpLimitProp;
        private SerializedProperty aimIKLeftLimitProp;
        private SerializedProperty aimIKRightLimitProp;
        private SerializedProperty aimIkTurnRotationRateProp;
        private SerializedProperty aimIKBoneWeightFPSProp;
        private SerializedProperty isAimBonesExpandedProp;    

        // Foot IK
        private SerializedProperty isFootIKProp;
        private SerializedProperty footIKAnimIKPassLayerIndexProp;
        private SerializedProperty footIKBodyMoveSpeedYProp;
        private SerializedProperty footIKSlopeReactRateProp;
        private SerializedProperty footIKSlopeToleranceProp;
        private SerializedProperty footIKToeDistanceProp;
        private SerializedProperty footIKPositionOnlyProp;
        private SerializedProperty footIKMaxInwardRotationXProp;
        private SerializedProperty footIKMaxOutwardRotationXProp;
        private SerializedProperty footIKMaxPitchZProp;
        private SerializedProperty paramHashLFootIKWeightCurveProp;
        private SerializedProperty paramHashRFootIKWeightCurveProp;
        private SerializedProperty isFootIKExpandedProp;

        // Ragdoll
        private SerializedProperty isRagdollExpandedProp;
        private SerializedProperty ragdollBoneListProp;
        private SerializedProperty s3dRagdollBoneProp;
        private SerializedProperty rdBoneGUIDHashProp;
        private SerializedProperty rdBoneTransformProp;
        private SerializedProperty rdBoneProp;

        // Root Motion
        private SerializedProperty isRootMotionProp;
        private SerializedProperty isRootMotionExpandedProp;
        private SerializedProperty isRootMotionTurningProp;
        private SerializedProperty rootMotionIdleThresholdProp;

        // Head IK
        private SerializedProperty isHeadIKProp;
        private SerializedProperty headIKAnimIKPassLayerIndexProp;
        private SerializedProperty isHeadIKExpandedProp;
        private SerializedProperty headIKMoveMaxSpeedProp;
        private SerializedProperty headIKMoveDampingProp;
        private SerializedProperty headIKLookDownLimitProp;
        private SerializedProperty headIKLookUpLimitProp;
        private SerializedProperty headIKLookLeftLimitProp;
        private SerializedProperty headIKLookRightLimitProp;
        private SerializedProperty headIKEyesWeightProp;
        private SerializedProperty headIKHeadWeightProp;
        private SerializedProperty headIKBodyWeightProp;
        private SerializedProperty headIKLookAtEyesProp;
        private SerializedProperty headIKLookAtInteractiveProp;
        private SerializedProperty headIKAdjustForVelocityProp;
        private SerializedProperty headIKWhenClimbingProp;
        private SerializedProperty headIKWhenMovementDisabledProp;
        private SerializedProperty headIKConsiderBehindProp;

        // Hand IK
        private SerializedProperty isHandIKProp;
        private SerializedProperty handIKAnimIKPassLayerIndexProp;
        private SerializedProperty handIKMoveMaxSpeedProp;
        private SerializedProperty isHandIKExpandedProp;
        private SerializedProperty isLeftHandIKExpandedProp;
        private SerializedProperty isRightHandIKExpandedProp;
        private SerializedProperty handIKWhenMovementDisabledProp;
        private SerializedProperty handZoneGizmoColourProp;

        private SerializedProperty leftHandRadiusProp;
        private SerializedProperty leftHandPalmOffsetProp;
        private SerializedProperty leftHandPalmRotationProp;
        private SerializedProperty handIKLHElbowHintProp;
        private SerializedProperty handIKLHMaxInRotProp;
        private SerializedProperty handIKLHMaxOutRotProp;
        private SerializedProperty handIKLHMaxUpRotProp;
        private SerializedProperty handIKLHMaxDownRotProp;
        private SerializedProperty handIKLHInwardLimitProp;
        private SerializedProperty handIKLHOutwardLimitProp;
        private SerializedProperty handIKLHMaxReachDistProp;

        private SerializedProperty rightHandPalmOffsetProp;
        private SerializedProperty rightHandPalmRotationProp;
        private SerializedProperty handIKRHElbowHintProp;
        private SerializedProperty handIKRHMaxInRotProp;
        private SerializedProperty handIKRHMaxOutRotProp;
        private SerializedProperty handIKRHMaxUpRotProp;
        private SerializedProperty handIKRHMaxDownRotProp;
        private SerializedProperty handIKRHInwardLimitProp;
        private SerializedProperty handIKRHOutwardLimitProp;
        private SerializedProperty handIKRHMaxReachDistProp;

        // Hand VR
        private SerializedProperty isHandVRProp;
        private SerializedProperty isHandVRExpandedProp;
        private SerializedProperty handVRLHAnimatorProp;
        private SerializedProperty handVRRHAnimatorProp;

        private SerializedProperty showLHGizmosInSceneViewProp;
        private SerializedProperty showRHGizmosInSceneViewProp;

        // Animation Actions
        private SerializedProperty s3dAAListProp;
        private SerializedProperty isAnimActionsExpandedProp;
        private SerializedProperty isS3DAnimActionListExpandedProp;
        private SerializedProperty s3dAnimActionProp;
        private SerializedProperty s3dAAShowInEditorProp;
        private SerializedProperty s3dAAstandardActionProp;
        private SerializedProperty s3dAAParamTypeProp;
        private SerializedProperty s3dAAParamHashCodeProp;
        private SerializedProperty s3dAAValueProp;
        private SerializedProperty s3dAAIsInvertProp;
        private SerializedProperty s3dAAIsToggleProp;
        private SerializedProperty s3dACListProp;
        private SerializedProperty s3dAnimConditionProp;
        private SerializedProperty s3dACShowInEditorProp;
        private SerializedProperty s3dACConditionTypeProp;
        private SerializedProperty s3dACActionConditionProp;
        #endregion

        #region Serialized Properties - Engage
        private SerializedProperty isEngageGeneralExpandedProp;
        private SerializedProperty isEngageDmRgnExpandedProp;
        private SerializedProperty isEngageDmRgnListExpandedProp;
        private SerializedProperty isEngageEquipExpandedProp;
        private SerializedProperty isEngageEventsExpandedProp;
        private SerializedProperty isEngageRespawnExpandedProp;
        private SerializedProperty isEngageStashExpandedProp;
        private SerializedProperty storeMaxSelectableInSceneProp;
        private SerializedProperty engageColourProp;
        private SerializedProperty nonengageColourProp;
        private SerializedProperty interactiveTagsProp;
        private SerializedProperty lassoSpeedProp;
        private SerializedProperty healthProp;
        private SerializedProperty damageRegionListProp;
        private SerializedProperty damageRegionProp;
        private SerializedProperty damageRegionShowInEditorProp;
        private SerializedProperty damageRegionShowMultipliersInEditorProp;
        private SerializedProperty startingHealthProp;
        private SerializedProperty useShieldingProp;
        private SerializedProperty damageRegionNameProp;
        private SerializedProperty damageRegionColliderProp;
        private SerializedProperty damageRegionTransformProp;
        private SerializedProperty damageRegionBonePersistProp;
        private SerializedProperty shieldingRechargeRateProp;
        private SerializedProperty startLivesProp;
        private SerializedProperty maxLivesProp;
        private SerializedProperty respawnModeProp;
        private SerializedProperty respawnTimeProp;
        private SerializedProperty customRespawnPositionProp;
        private SerializedProperty customRespawnRotationProp;
        private SerializedProperty isDropHeldOnDestroyProp;
        private SerializedProperty isDropEquipOnDestroyProp;
        private SerializedProperty isDropStashOnDestroyProp;
        private SerializedProperty isRagdollOnDestroyProp;

        private SerializedProperty onInitialisedEvtDelayProp;
        private SerializedProperty onInitialisedProp;
        private SerializedProperty onDestroyedProp;
        private SerializedProperty onInteractLookAtChangedProp;
        private SerializedProperty onPreStartAimProp;
        private SerializedProperty onPostStartAimProp;
        private SerializedProperty onPreStopAimProp;
        private SerializedProperty onPostStopAimProp;
        private SerializedProperty onPreStartHoldWeaponProp;
        private SerializedProperty onPostStopHoldWeaponProp;
        private SerializedProperty onRespawnedProp;
        private SerializedProperty onRespawningProp;

        private SerializedProperty isEngageEquipPtListExpandedProp;
        private SerializedProperty equipPointListProp;
        private SerializedProperty equipPointProp;
        private SerializedProperty equipPointNameProp;
        private SerializedProperty equipPointMaxItemsProp;
        private SerializedProperty equipPointPermittedTagsProp;
        private SerializedProperty equipPointParentTransformProp;
        private SerializedProperty equipPointRelativeOffsetProp;
        private SerializedProperty equipPointRelativeRotationProp;
        private SerializedProperty equipPointShowInEditorProp;
        private SerializedProperty equipPtShowGizmoInSceneViewProp;
        private SerializedProperty equipPtIsSelectedInSceneViewProp;
        private SerializedProperty minEquipDropDistanceProp;
        private SerializedProperty maxEquipDropDistanceProp;

        private SerializedProperty stashParentProp;
        private SerializedProperty minStashDropDistanceProp;
        private SerializedProperty maxStashDropDistanceProp;
        #endregion

        #region Serialized Properties - Engage Identification
        private SerializedProperty factionIdProp;
        private SerializedProperty modelIdProp;
        private SerializedProperty isIdenficationExpandedProp;

        #endregion

        #region Events

        public void OnEnable()
        {
            stickyControlModule = (StickyControlModule)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 1f) : Color.grey;
            groundedIndicatorColour = new Color(0f, 1f, 0f, 0.5f);

            // Reset GUIStyles
            isStylesInitialised = false;
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            foldoutStyleNoLabel = null;

            // Used in Richtext labels
            if (EditorGUIUtility.isProSkin) { txtColourName = "White"; defaultTextColour = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f); }

            // Keep compiler happy - can remove this later if it isn't required
            if (defaultTextColour.a > 0f) { }

            if (!Application.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                stickyControlModule.InitialiseEditorEssentials();
            }

            SceneView.duringSceneGui -= SceneGUI;
            SceneView.duringSceneGui += SceneGUI;

            stickyControlModule.ReinitialiseDamage();

            interactiveTagNames = null;

            #region Find Properties - General
            selectedTabIntProp = serializedObject.FindProperty("selectedTabInt");
            #endregion

            #region Find Properties - Move General
            initialiseOnAwakeProp = serializedObject.FindProperty("initialiseOnAwake");
            isNPCProp = serializedObject.FindProperty("isNPC");
            isMoveGeneralExpandedProp = serializedObject.FindProperty("isMoveGeneralExpanded");
            walkSpeedProp = serializedObject.FindProperty("walkSpeed");
            sprintSpeedProp = serializedObject.FindProperty("sprintSpeed");
            strafeSpeedProp = serializedObject.FindProperty("strafeSpeed");
            jumpSpeedProp = serializedObject.FindProperty("jumpSpeed");
            jumpDelayProp = serializedObject.FindProperty("jumpDelay");
            crouchSpeedProp = serializedObject.FindProperty("crouchSpeed");
            maxAccelerationProp = serializedObject.FindProperty("maxAcceleration");
            maxSlopeAngleProp = serializedObject.FindProperty("maxSlopeAngle");
            allowMovementInAirProp = serializedObject.FindProperty("allowMovementInAir");
            allowSprintBackwardProp = serializedObject.FindProperty("allowSprintBackward");
            gravitationalAccelerationProp = serializedObject.FindProperty("gravitationalAcceleration");
            arcadeFallMultiplierProp = serializedObject.FindProperty("arcadeFallMultiplier");
            maxStepOffsetProp = serializedObject.FindProperty("maxStepOffset");
            stepUpSpeedProp = serializedObject.FindProperty("stepUpSpeed");
            stepUpBiasProp = serializedObject.FindProperty("stepUpBias");
            alignToGroundNormalProp = serializedObject.FindProperty("alignToGroundNormal");
            verticalRotationRateProp = serializedObject.FindProperty("verticalRotationRate");
            turnRotationRateProp = serializedObject.FindProperty("turnRotationRate");
            stuckTimeProp = serializedObject.FindProperty("stuckTime");
            stuckSpeedThresholdProp = serializedObject.FindProperty("stuckSpeedThreshold");
            moveUpdateTypeProp = serializedObject.FindProperty("moveUpdateType");
            #endregion

            #region Find Properties - Move Climbing
            isClimbingEnabledProp = serializedObject.FindProperty("isClimbingEnabled");
            isClimbingExpandedProp = serializedObject.FindProperty("isClimbingExpanded");
            climbSpeedProp = serializedObject.FindProperty("climbSpeed");
            minClimbSlopeAngleProp = serializedObject.FindProperty("minClimbSlopeAngle");
            maxClimbSlopeAngleProp = serializedObject.FindProperty("maxClimbSlopeAngle");
            maxGrabDistanceProp = serializedObject.FindProperty("maxGrabDistance");
            climbFaceSurfaceRateProp = serializedObject.FindProperty("climbFaceSurfaceRate");
            climbTopDetectionProp = serializedObject.FindProperty("climbTopDetection");
            climbableLayerMaskProp = serializedObject.FindProperty("climbableLayerMask");
            #endregion

            #region Find Properties - Move Footsteps
            isFootStepsEnabledProp = serializedObject.FindProperty("isFootStepsEnabled");
            s3dSurfacesProp = serializedObject.FindProperty("s3dSurfaces");
            leftFootProp = serializedObject.FindProperty("leftFoot");
            rightFootProp = serializedObject.FindProperty("rightFoot");
            footStepsUseMoveSpeedProp = serializedObject.FindProperty("footStepsUseMoveSpeed");
            footStepsAudioProp = serializedObject.FindProperty("footStepsAudio");
            footStepsDefaultClipProp = serializedObject.FindProperty("footStepsDefaultClip");
            footStepsVolumeProp = serializedObject.FindProperty("footStepsVolume");
            footStepWalkFrequencyProp = serializedObject.FindProperty("footStepWalkFrequency");
            footStepSprintFrequencyProp = serializedObject.FindProperty("footStepSprintFrequency");
            isS3DFootstepListExpandedProp = serializedObject.FindProperty("isS3DFootstepListExpanded");
            isS3DFootstepExpandedProp = serializedObject.FindProperty("isS3DFootstepExpanded");
            s3dFootstepListProp = serializedObject.FindProperty("s3dFootstepList");
            #endregion

            #region Find Properties - Look General
            isThirdPersonProp = serializedObject.FindProperty("isThirdPerson");
            isFreeLookProp = serializedObject.FindProperty("isFreeLook");
            lookOnInitialiseProp = serializedObject.FindProperty("lookOnInitialise");
            isLookGeneralExpandedProp = serializedObject.FindProperty("isLookGeneralExpanded");
            lookFirstPersonTransform1Prop = serializedObject.FindProperty("lookFirstPersonTransform1");
            lookFirstPersonCamera1Prop = serializedObject.FindProperty("lookFirstPersonCamera1");
            lookThirdPersonCamera1Prop = serializedObject.FindProperty("lookThirdPersonCamera1");
            showLookCamOffsetGizmosInSceneViewProp = serializedObject.FindProperty("showLookCamOffsetGizmosInSceneView");
            showLookFocusOffsetGizmosInSceneViewProp = serializedObject.FindProperty("showLookFocusOffsetGizmosInSceneView");
            isAutoFirstPersonCameraHeightProp = serializedObject.FindProperty("isAutoFirstPersonCameraHeight");
            isLookCameraFollowHeadProp = serializedObject.FindProperty("isLookCameraFollowHead");
            lookVerticalSpeedProp = serializedObject.FindProperty("lookVerticalSpeed");
            lookHorizontalSpeedProp = serializedObject.FindProperty("lookHorizontalSpeed");
            lookVerticalDampingProp = serializedObject.FindProperty("lookVerticalDamping");
            lookHorizontalDampingProp = serializedObject.FindProperty("lookHorizontalDamping");
            lookPitchUpLimitProp = serializedObject.FindProperty("lookPitchUpLimit");
            lookPitchDownLimitProp = serializedObject.FindProperty("lookPitchDownLimit");
            lookCameraOffsetProp = serializedObject.FindProperty("lookCameraOffset");
            lookFocusOffsetProp = serializedObject.FindProperty("lookFocusOffset");
            lookMoveSpeedProp = serializedObject.FindProperty("lookMoveSpeed");
            lookUnzoomDelayProp = serializedObject.FindProperty("lookUnzoomDelay");
            lookZoomDurationProp = serializedObject.FindProperty("zoomDuration");
            lookUnzoomedFoVProp = serializedObject.FindProperty("lookUnzoomedFoV");
            lookZoomedFoVProp = serializedObject.FindProperty("lookZoomedFoV");
            lookZoomOutFactorProp = serializedObject.FindProperty("lookZoomOutFactor");
            lookOrbitDurationProp = serializedObject.FindProperty("orbitDuration");
            lookUnorbitDelayProp = serializedObject.FindProperty("lookUnorbitDelay");
            lookOrbitDampingProp = serializedObject.FindProperty("lookOrbitDamping");
            lookOrbitMinAngleProp = serializedObject.FindProperty("lookOrbitMinAngle");
            lookOrbitMaxAngleProp = serializedObject.FindProperty("lookOrbitMaxAngle");
            lookShowCursorProp = serializedObject.FindProperty("lookShowCursor");
            lookAutoHideCursorProp = serializedObject.FindProperty("lookAutoHideCursor");
            lookHideCursorTimeProp = serializedObject.FindProperty("lookHideCursorTime");
            lookMaxLoSFoVProp = serializedObject.FindProperty("lookMaxLoSFoV");
            lookMaxShakeStrengthProp = serializedObject.FindProperty("lookMaxShakeStrength");
            lookMaxShakeDurationProp = serializedObject.FindProperty("lookMaxShakeDuration");
            lookUpdateTypeProp = serializedObject.FindProperty("lookUpdateType");
            //lookFocusToleranceProp = serializedObject.FindProperty("lookFocusTolerance");
            #endregion

            #region Find Properties - Look Clip Objects
            clipObjectsProp = serializedObject.FindProperty("clipObjects");
            isLookClipObjectsExpandedProp = serializedObject.FindProperty("isLookClipObjectsExpanded");
            clipMinDistanceProp = serializedObject.FindProperty("clipMinDistance");
            clipMinOffsetXProp = serializedObject.FindProperty("clipMinOffsetX");
            clipMinOffsetYProp = serializedObject.FindProperty("clipMinOffsetY");
            clipResponsivenessProp = serializedObject.FindProperty("clipResponsiveness");
            minClipMoveSpeedProp = serializedObject.FindProperty("minClipMoveSpeed");
            clipObjectMaskProp = serializedObject.FindProperty("clipObjectMask");
            #endregion

            #region Find Properties - Look Interactive
            isLookInteractiveExpandedProp = serializedObject.FindProperty("isLookInteractiveExpanded");
            isLookInteractiveEnabledProp = serializedObject.FindProperty("isLookInteractiveEnabled");
            lookMaxInteractiveDistanceProp = serializedObject.FindProperty("lookMaxInteractiveDistance");
            lookInteractiveLockToCameraProp = serializedObject.FindProperty("lookInteractiveLockToCamera");
            lookInteractiveLayerMaskProp = serializedObject.FindProperty("lookInteractiveLayerMask");
            isUpdateLookingAtPointProp = serializedObject.FindProperty("isUpdateLookingAtPoint");
            #endregion

            #region Find Properties - Look Sockets
            isLookSocketsExpandedProp = serializedObject.FindProperty("isLookSocketsExpanded");
            isLookSocketsEnabledProp = serializedObject.FindProperty("isLookSocketsEnabled");
            lookMaxSocketDistanceProp = serializedObject.FindProperty("lookMaxSocketDistance");
            lookSocketLockToCameraProp = serializedObject.FindProperty("lookSocketLockToCamera");
            lookSocketLayerMaskProp = serializedObject.FindProperty("lookSocketLayerMask");
            socketActiveMaterialProp = serializedObject.FindProperty("socketActiveMaterial");
            isLookSocketAutoShowProp = serializedObject.FindProperty("isLookSocketAutoShow");
            #endregion

            #region Find Properties - Look VR
            isLookVRProp = serializedObject.FindProperty("isLookVR");
            isLookVRExpandedProp = serializedObject.FindProperty("isLookVRExpanded");
            isMatchHumanHeightVRProp = serializedObject.FindProperty("isMatchHumanHeightVR");
            isRoomScaleVRProp = serializedObject.FindProperty("isRoomScaleVR");
            humanPostureVRProp = serializedObject.FindProperty("humanPostureVR");
            isSnapTurnVRProp = serializedObject.FindProperty("isSnapTurnVR");
            snapTurnDegreesProp = serializedObject.FindProperty("snapTurnDegrees");
            snapTurnIntervalTimeProp = serializedObject.FindProperty("snapTurnIntervalTime");
            #endregion

            #region Find Properties - Collide
            heightProp = serializedObject.FindProperty("height");
            radiusProp = serializedObject.FindProperty("radius");
            pivotToCentreOffsetYProp = serializedObject.FindProperty("pivotToCentreOffsetY");
            shoulderHeightProp = serializedObject.FindProperty("shoulderHeight");
            crouchHeightNormalisedProp = serializedObject.FindProperty("crouchHeightNormalised");
            maxSweepIterationsProp = serializedObject.FindProperty("maxSweepIterations");
            sweepToleranceProp = serializedObject.FindProperty("sweepTolerance");
            collisionLayerMaskProp = serializedObject.FindProperty("collisionLayerMask");
            interactionLayerMaskProp = serializedObject.FindProperty("interactionLayerMask");
            referenceFrameLayerMaskProp = serializedObject.FindProperty("referenceFrameLayerMask");
            isOnTriggerEnterEnabledProp = serializedObject.FindProperty("isOnTriggerEnterEnabled");
            isOnTriggerStayEnabledProp = serializedObject.FindProperty("isOnTriggerStayEnabled");
            isTriggerColliderEnabledProp = serializedObject.FindProperty("isTriggerColliderEnabled");
            isReactToStickyZonesEnabledProp = serializedObject.FindProperty("isReactToStickyZonesEnabled");

            referenceUpdateTypeProp = serializedObject.FindProperty("referenceUpdateType");
            #endregion

            #region Find Properties - Jet Pack
            isJetPackAvailableProp = serializedObject.FindProperty("isJetPackAvailable");
            isJetPackEnabledProp = serializedObject.FindProperty("isJetPackEnabled");
            jetPackFuelLevelProp = serializedObject.FindProperty("jetPackFuelLevel");
            jetPackFuelBurnRateProp = serializedObject.FindProperty("jetPackFuelBurnRate");
            jetPackSpeedProp = serializedObject.FindProperty("jetPackSpeed");
            jetPackMaxAccelerationProp = serializedObject.FindProperty("jetPackMaxAcceleration");
            jetPackDampingProp = serializedObject.FindProperty("jetPackDamping");
            jetPackAudioProp = serializedObject.FindProperty("jetPackAudio");
            jetPackRampUpDurationProp = serializedObject.FindProperty("jetPackRampUpDuration");
            jetPackRampDownDurationProp = serializedObject.FindProperty("jetPackRampDownDuration");
            jetPackHealthProp = serializedObject.FindProperty("jetPackHealth");

            isJetPackThrusterListExpandedProp = serializedObject.FindProperty("isJetPackThrusterListExpanded");
            jetPackThrusterFwdProp = serializedObject.FindProperty("jetPackThrusterFwd");
            jetPackThrusterBackProp = serializedObject.FindProperty("jetPackThrusterBack");
            jetPackThrusterUpProp = serializedObject.FindProperty("jetPackThrusterUp");
            jetPackThrusterDownProp = serializedObject.FindProperty("jetPackThrusterDown");
            jetPackThrusterRightProp = serializedObject.FindProperty("jetPackThrusterRight");
            jetPackThrusterLeftProp = serializedObject.FindProperty("jetPackThrusterLeft");
            jetPackMinEffectsRateProp = serializedObject.FindProperty("jetPackMinEffectsRate");
            #endregion

            #region Find Properties - Animate
            defaultAnimatorProp = serializedObject.FindProperty("defaultAnimator");

            // Aim IK
            aimIKCameraOffsetTPSProp = serializedObject.FindProperty("aimIKCameraOffsetTPS");
            isAimIKWhenNotAimingProp = serializedObject.FindProperty("isAimIKWhenNotAiming");
            aimIKAnimIKPassLayerIndexProp = serializedObject.FindProperty("aimIKAnimIKPassLayerIndex");
            aimIkTurnRotationRateProp = serializedObject.FindProperty("aimIkTurnRotationRate");
            aimIKBoneWeightFPSProp = serializedObject.FindProperty("aimIKBoneWeightFPS");
            aimBonesProp = serializedObject.FindProperty("aimBones");
            isAimIKExpandedProp = serializedObject.FindProperty("isAimIKExpanded");
            isAimBonesExpandedProp = serializedObject.FindProperty("isAimBonesExpanded");
            aimIKDownLimitProp = serializedObject.FindProperty("aimIKDownLimit");
            aimIKUpLimitProp = serializedObject.FindProperty("aimIKUpLimit");
            aimIKLeftLimitProp = serializedObject.FindProperty("aimIKLeftLimit");
            aimIKRightLimitProp = serializedObject.FindProperty("aimIKRightLimit");

            // Foot IK
            isFootIKProp = serializedObject.FindProperty("isFootIK");
            footIKAnimIKPassLayerIndexProp = serializedObject.FindProperty("footIKAnimIKPassLayerIndex");
            paramHashLFootIKWeightCurveProp = serializedObject.FindProperty("paramHashLFootIKWeightCurve");
            paramHashRFootIKWeightCurveProp = serializedObject.FindProperty("paramHashRFootIKWeightCurve");
            footIKBodyMoveSpeedYProp = serializedObject.FindProperty("footIKBodyMoveSpeedY");
            footIKSlopeReactRateProp = serializedObject.FindProperty("footIKSlopeReactRate");
            footIKSlopeToleranceProp = serializedObject.FindProperty("footIKSlopeTolerance");
            footIKToeDistanceProp = serializedObject.FindProperty("footIKToeDistance");
            footIKPositionOnlyProp = serializedObject.FindProperty("footIKPositionOnly");
            footIKMaxInwardRotationXProp = serializedObject.FindProperty("footIKMaxInwardRotationX");
            footIKMaxOutwardRotationXProp = serializedObject.FindProperty("footIKMaxOutwardRotationX");
            footIKMaxPitchZProp = serializedObject.FindProperty("footIKMaxPitchZ");
            isFootIKExpandedProp = serializedObject.FindProperty("isFootIKExpanded");

            // Head IK
            isHeadIKProp = serializedObject.FindProperty("isHeadIK");
            headIKAnimIKPassLayerIndexProp = serializedObject.FindProperty("headIKAnimIKPassLayerIndex");
            isHeadIKExpandedProp = serializedObject.FindProperty("isHeadIKExpanded");
            headIKMoveMaxSpeedProp = serializedObject.FindProperty("headIKMoveMaxSpeed");
            headIKMoveDampingProp = serializedObject.FindProperty("headIKMoveDamping");
            headIKLookDownLimitProp = serializedObject.FindProperty("headIKLookDownLimit");
            headIKLookUpLimitProp = serializedObject.FindProperty("headIKLookUpLimit");
            headIKLookLeftLimitProp = serializedObject.FindProperty("headIKLookLeftLimit");
            headIKLookRightLimitProp = serializedObject.FindProperty("headIKLookRightLimit");
            headIKEyesWeightProp = serializedObject.FindProperty("headIKEyesWeight");
            headIKHeadWeightProp = serializedObject.FindProperty("headIKHeadWeight");
            headIKBodyWeightProp = serializedObject.FindProperty("headIKBodyWeight");
            headIKLookAtEyesProp = serializedObject.FindProperty("headIKLookAtEyes");
            headIKLookAtInteractiveProp = serializedObject.FindProperty("headIKLookAtInteractive");
            headIKAdjustForVelocityProp = serializedObject.FindProperty("headIKAdjustForVelocity");
            headIKWhenClimbingProp = serializedObject.FindProperty("headIKWhenClimbing");
            headIKWhenMovementDisabledProp = serializedObject.FindProperty("headIKWhenMovementDisabled");
            headIKConsiderBehindProp = serializedObject.FindProperty("headIKConsiderBehind");

            // Hand IK
            isHandIKProp = serializedObject.FindProperty("isHandIK");
            handIKAnimIKPassLayerIndexProp = serializedObject.FindProperty("handIKAnimIKPassLayerIndex");
            handIKMoveMaxSpeedProp = serializedObject.FindProperty("handIKMoveMaxSpeed");
            isHandIKExpandedProp = serializedObject.FindProperty("isHandIKExpanded");
            isLeftHandIKExpandedProp = serializedObject.FindProperty("isLeftHandIKExpanded");
            isRightHandIKExpandedProp = serializedObject.FindProperty("isRightHandIKExpanded");
            handZoneGizmoColourProp = serializedObject.FindProperty("handZoneGizmoColour");
            handIKWhenMovementDisabledProp = serializedObject.FindProperty("handIKWhenMovementDisabled");
            leftHandRadiusProp = serializedObject.FindProperty("leftHandRadius");
            leftHandPalmOffsetProp = serializedObject.FindProperty("leftHandPalmOffset");
            leftHandPalmRotationProp = serializedObject.FindProperty("leftHandPalmRotation");
            handIKLHElbowHintProp = serializedObject.FindProperty("handIKLHElbowHint");
            handIKLHMaxInRotProp = serializedObject.FindProperty("handIKLHMaxInRot");
            handIKLHMaxOutRotProp = serializedObject.FindProperty("handIKLHMaxOutRot");
            handIKLHMaxUpRotProp = serializedObject.FindProperty("handIKLHMaxUpRot");
            handIKLHMaxDownRotProp = serializedObject.FindProperty("handIKLHMaxDownRot");
            handIKLHInwardLimitProp = serializedObject.FindProperty("handIKLHInwardLimit");
            handIKLHOutwardLimitProp = serializedObject.FindProperty("handIKLHOutwardLimit");
            handIKLHMaxReachDistProp = serializedObject.FindProperty("handIKLHMaxReachDist");
            rightHandPalmOffsetProp = serializedObject.FindProperty("rightHandPalmOffset");
            rightHandPalmRotationProp = serializedObject.FindProperty("rightHandPalmRotation");
            handIKRHElbowHintProp = serializedObject.FindProperty("handIKRHElbowHint");
            handIKRHMaxInRotProp = serializedObject.FindProperty("handIKRHMaxInRot");
            handIKRHMaxOutRotProp = serializedObject.FindProperty("handIKRHMaxOutRot");
            handIKRHMaxUpRotProp = serializedObject.FindProperty("handIKRHMaxUpRot");
            handIKRHMaxDownRotProp = serializedObject.FindProperty("handIKRHMaxDownRot");
            handIKRHInwardLimitProp = serializedObject.FindProperty("handIKRHInwardLimit");
            handIKRHOutwardLimitProp = serializedObject.FindProperty("handIKRHOutwardLimit");
            handIKRHMaxReachDistProp = serializedObject.FindProperty("handIKRHMaxReachDist");
            showLHGizmosInSceneViewProp = serializedObject.FindProperty("showLHGizmosInSceneView");
            showRHGizmosInSceneViewProp = serializedObject.FindProperty("showRHGizmosInSceneView");

            // Hand VR
            isHandVRProp = serializedObject.FindProperty("isHandVR");
            isHandVRExpandedProp = serializedObject.FindProperty("isHandVRExpanded");
            handVRLHAnimatorProp = serializedObject.FindProperty("leftHandAnimator");
            handVRRHAnimatorProp = serializedObject.FindProperty("rightHandAnimator");

            // Ragdoll
            isRagdollExpandedProp = serializedObject.FindProperty("isRagdollExpanded");
            ragdollBoneListProp = serializedObject.FindProperty("ragdollBoneList");

            // Root Motion
            isRootMotionProp = serializedObject.FindProperty("isRootMotion");
            isRootMotionTurningProp = serializedObject.FindProperty("isRootMotionTurning");
            rootMotionIdleThresholdProp = serializedObject.FindProperty("rootMotionIdleThreshold");
            isRootMotionExpandedProp = serializedObject.FindProperty("isRootMotionExpanded");

            // Animation Actions
            s3dAAListProp = serializedObject.FindProperty("s3dAnimActionList");
            isAnimActionsExpandedProp = serializedObject.FindProperty("isAnimActionsExpanded");
            isS3DAnimActionListExpandedProp = serializedObject.FindProperty("isS3DAnimActionListExpanded");
            #endregion

            #region Find Properties - Engage
            isEngageGeneralExpandedProp = serializedObject.FindProperty("isEngageGeneralExpanded");
            isEngageDmRgnExpandedProp = serializedObject.FindProperty("isEngageDamageRegionsExpanded");
            isEngageDmRgnListExpandedProp = serializedObject.FindProperty("isEngageDamageRegionListExpanded");
            isEngageEquipExpandedProp = serializedObject.FindProperty("isEngageEquipExpanded");
            isEngageEventsExpandedProp = serializedObject.FindProperty("isEngageEventsExpanded");
            isEngageRespawnExpandedProp = serializedObject.FindProperty("isEngageRespawnExpanded");
            isEngageStashExpandedProp = serializedObject.FindProperty("isEngageStashExpanded");
            storeMaxSelectableInSceneProp = serializedObject.FindProperty("storeMaxSelectableInScene");
            engageColourProp = serializedObject.FindProperty("engageColour");
            nonengageColourProp = serializedObject.FindProperty("nonengageColour");
            interactiveTagsProp = serializedObject.FindProperty("interactiveTags");
            lassoSpeedProp = serializedObject.FindProperty("lassoSpeed");
            healthProp = serializedObject.FindProperty("health");
            damageRegionListProp = serializedObject.FindProperty("damageRegionList");
            onInitialisedEvtDelayProp = serializedObject.FindProperty("onInitialisedEvtDelay");
            onInitialisedProp = serializedObject.FindProperty("onInitialised");
            onDestroyedProp = serializedObject.FindProperty("onDestroyed");
            onInteractLookAtChangedProp = serializedObject.FindProperty("onInteractLookAtChanged");
            onPreStartAimProp = serializedObject.FindProperty("onPreStartAim");
            onPostStartAimProp = serializedObject.FindProperty("onPostStartAim");
            onPreStopAimProp = serializedObject.FindProperty("onPreStopAim");
            onPostStopAimProp = serializedObject.FindProperty("onPostStopAim");

            onPreStartHoldWeaponProp = serializedObject.FindProperty("onPreStartHoldWeapon");
            onPostStopHoldWeaponProp = serializedObject.FindProperty("onPostStopHoldWeapon");

            onRespawnedProp = serializedObject.FindProperty("onRespawned");
            onRespawningProp = serializedObject.FindProperty("onRespawning");

            equipPointListProp = serializedObject.FindProperty("equipPointList");
            isEngageEquipPtListExpandedProp = serializedObject.FindProperty("isEngageEquipPointListExpanded");
            minEquipDropDistanceProp = serializedObject.FindProperty("minEquipDropDistance");
            maxEquipDropDistanceProp = serializedObject.FindProperty("maxEquipDropDistance");

            stashParentProp = serializedObject.FindProperty("stashParent");
            minStashDropDistanceProp = serializedObject.FindProperty("minStashDropDistance");
            maxStashDropDistanceProp = serializedObject.FindProperty("maxStashDropDistance");

            #endregion

            #region Find Properties - Engage Identification
            factionIdProp = serializedObject.FindProperty("factionId");
            modelIdProp = serializedObject.FindProperty("modelId");
            isIdenficationExpandedProp = serializedObject.FindProperty("isIdenficationExpanded");
            #endregion

            #region Find Properties - Engage Respawn
            startLivesProp = serializedObject.FindProperty("startLives");
            maxLivesProp = serializedObject.FindProperty("maxLives");
            respawnModeProp = serializedObject.FindProperty("respawnMode");
            respawnTimeProp = serializedObject.FindProperty("respawnTime");
            customRespawnPositionProp = serializedObject.FindProperty("customRespawnPosition");
            customRespawnRotationProp = serializedObject.FindProperty("customRespawnRotation");
            isDropHeldOnDestroyProp = serializedObject.FindProperty("isDropHeldOnDestroy");
            isDropEquipOnDestroyProp = serializedObject.FindProperty("isDropEquipOnDestroy");
            isDropStashOnDestroyProp = serializedObject.FindProperty("isDropStashOnDestroy");
            isRagdollOnDestroyProp = serializedObject.FindProperty("isRagdollOnDestroy");

            #endregion

            RefreshAnimatorParameters();
            RefreshVRAnimatorParameters();
            //RefreshAnimatorTransitions();

            RefreshSurfaceTypes();

            isFootstepClipPlaying = false;

            // force refresh
            headTrfm = null;
            leftFootTrfm = null;
            rightHandTrfm = null;

            // Deselect all components in the scene view
            DeselectAllComponents();

            if (rigHumanBoneList == null) { rigHumanBoneList = new List<S3DHumanBone>(); }

            RefreshBones();

            //S3DUtils.ReflectionOutputMethods(typeof(Animator), false, true, true);
            //S3DUtils.ReflectionOutputProperties(typeof(Animator), true, true);
        }

        /// <summary>
        /// Called when the gameobject loses focus
        /// </summary>
        private void OnDestroy()
        {
            SceneView.duringSceneGui -= SceneGUI;

            // Always unhide Unity tools when losing focus on this gameObject
            Tools.hidden = false;
        }

        // Called when the gameobject loses focus or enters/exits playmode
        private void OnDisable()
        {
            if (isFootstepClipPlaying)
            {
                StickyEditorHelper.StopAllAudioClips();
                isFootstepClipPlaying = false;
            }

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
            if (stickyControlModule.allowRepaint) { Repaint(); }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Configure bone colliders. These are used for the Ragdoll AND damage regions.
        /// They are configured by default disabled as Triggers.
        /// At runtime, during initialisation, if there are matching damage regions,
        /// they will be enabled as triggers.
        /// At runtime, when ragdoll is enabled, they are enabled as non-trigger colliders.
        /// </summary>
        /// <param name="bodyBone"></param>
        /// <param name="parentBone"></param>
        /// <param name="childBone"></param>
        /// <param name="isAddEnd"></param>
        /// <param name="colliderType"></param>
        /// <param name="colRadiusFactor"></param>
        private Collider ConfigureBoneCollider
        (
            S3DHumanBonePersist bodyBone, S3DHumanBonePersist parentBone, S3DHumanBonePersist childBone,
            bool isAddEnd, System.Type colliderType, float colRadiusFactor
        )
        {
            CapsuleCollider capCollider;
            BoxCollider boxCollider;
            
            if (bodyBone == null)
            {
                Debug.LogWarning("[ERROR] ConfigureBoneCollider - could not find " + bodyBone + " bone on " + stickyControlModule.name);
                return null;
            }
            else
            {
                Transform boneTfrm = bodyBone.boneTransform;

                if (boneTfrm != null)
                {
                    float stickyRadius = stickyControlModule.radius;
                    GameObject boneGO = boneTfrm.gameObject;

                    // The rig model may be scaled (eg. 100, 100, 100)
                    float boneScale = boneTfrm.lossyScale.x;

                    #region Capsule Collider
                    if (colliderType == typeof(CapsuleCollider))
                    {
                        if (!boneTfrm.TryGetComponent(out capCollider))
                        {
                            capCollider = Undo.AddComponent<CapsuleCollider>(boneGO);

                            // Create in disabled state
                            capCollider.enabled = false;
                            // This gets changed to non-trigger if ragdoll is enabled at runtime
                            capCollider.isTrigger = true;

                            if (bodyBone.bone == HumanBodyBones.Head)
                            {
                                capCollider.radius = stickyRadius * 0.40f / boneScale;
                                capCollider.height = capCollider.radius * 3f / boneScale;

                                Transform hipsBoneTransform = stickyControlModule.GetRagdollBoneTransform(HumanBodyBones.Hips);

                                // If the ragdoll isn't configured, attempt to find the hips bone.
                                if (hipsBoneTransform == null) { stickyControlModule.defaultAnimator.GetBoneTransform(HumanBodyBones.Hips); }

                                if (hipsBoneTransform != null && bodyBone.boneTransform != null)
                                {
                                    int axis;
                                    float distance;
                                    S3DMath.LongestAxis(S3DUtils.GetLocalSpacePosition(bodyBone.boneTransform, hipsBoneTransform.position), out axis, out distance);

                                    Vector3 capCentre = Vector3.zero;
                                    capCentre[axis] = distance > 0f ? -capCollider.radius : capCollider.radius;

                                    capCollider.center = capCentre;
                                }
                            }
                            else if (childBone != null)
                            {
                                // If this bone has a child, 
                                int axis;
                                float distance;
                                stickyControlModule.GetBoneLength(boneTfrm, childBone.boneTransform, out axis, out distance);

                                capCollider.radius = (distance < 0f ? -distance : distance) * colRadiusFactor / boneScale;

                                // If this is the last arm or leg bone, add an extra distance to cater for the hand or foot (which we don't know the length of)
                                capCollider.height = ((distance < 0f ? -distance : distance) + (isAddEnd ? capCollider.radius : 0f)) / boneScale;
                                Vector3 capCentre = Vector3.zero;
                                capCentre[axis] = capCollider.height * 0.5f;
                                capCollider.center = capCentre;
                            }
                        }

                        bodyBone.boneCollider = capCollider;
                    }
                    #endregion

                    #region Box Collider
                    else if (colliderType == typeof(BoxCollider))
                    {
                        if (!boneTfrm.TryGetComponent(out boxCollider))
                        {
                            if (childBone == null || parentBone == null)
                            {
                                if (bodyBone.bone == HumanBodyBones.Hips)
                                {
                                    boxCollider = Undo.AddComponent<BoxCollider>(boneGO);

                                    // Create in disabled state
                                    boxCollider.enabled = false;
                                    // This gets changed to non-trigger if ragdoll is enabled at runtime
                                    boxCollider.isTrigger = true;

                                    boxCollider.size = (new Vector3(stickyRadius * 2f * colRadiusFactor, 0.2f, stickyRadius * 0.6f)) / boneScale;

                                    bodyBone.boneCollider = boxCollider;
                                }
                            }
                            else
                            {
                                boxCollider = Undo.AddComponent<BoxCollider>(boneGO);

                                // Create in disabled state
                                boxCollider.enabled = false;
                                // This gets changed to non-trigger if ragdoll is enabled at runtime
                                boxCollider.isTrigger = true;

                                int axis;
                                float distance;
                                stickyControlModule.GetBoneLength(parentBone.boneTransform, childBone.boneTransform, out axis, out distance);

                                // Use the radius factor to change the width of the box collider.
                                // Assume the z (depth) of the box collider is less than the radius as the character is likely to be thinner than it is wide.
                                Vector3 colliderSize = (new Vector3(stickyRadius * 2f * colRadiusFactor, distance, stickyRadius * 0.6f)) / boneScale;

                                if (bodyBone.bone == HumanBodyBones.Chest)
                                {
                                    // Allow space for the hips collider
                                    colliderSize.y -= 0.2f / boneScale;
                                    boxCollider.center = new Vector3(0f, 0.1f / boneScale, 0f);
                                }

                                boxCollider.size = colliderSize;

                                bodyBone.boneCollider = boxCollider;
                            }
                        }
                        else
                        {
                            bodyBone.boneCollider = boxCollider;
                        }
                    }

                    #endregion
                }

                return bodyBone.boneCollider;
            }
        }

        /// <summary>
        /// Configure each ragdoll bone.
        /// Add and configure rigidbody, joint and collider.
        /// </summary>
        /// <param name="bodyBone"></param>
        /// <param name="parentBone"></param>
        /// <param name="childBone"></param>
        /// <param name="isAddEnd"></param>
        /// <param name="colliderType"></param>
        /// <param name="colRadiusFactor"></param>
        /// <param name="twistAxis"></param>
        /// <param name="swingAxis"></param>
        /// <param name="twistMinLimit"></param>
        /// <param name="twistMaxLimit"></param>
        /// <param name="swingLimit"></param>
        /// <param name="normalisedMass"></param>
        private void ConfigureRagdollBone
        (
            S3DHumanBonePersist bodyBone, S3DHumanBonePersist parentBone, S3DHumanBonePersist childBone, bool isAddEnd,
            System.Type colliderType, float colRadiusFactor, Vector3 twistAxis, Vector3 swingAxis,
            float twistMinLimit, float twistMaxLimit, float swingLimit, float normalisedMass
        )
        {
            CharacterJoint joint = null;
            Transform boneTfrm;
            Rigidbody rb;

            if (bodyBone == null)
            {
                Debug.LogWarning("[ERROR] CreateRagdoll - could not find " + bodyBone + " bone on " + stickyControlModule.name);
            }
            else
            {
                boneTfrm = bodyBone.boneTransform;

                float stickyMass = stickyControlModule.CharacterRigidBody.mass;

                bool isHips = bodyBone.bone == HumanBodyBones.Hips;

                if (boneTfrm != null)
                {
                    GameObject boneGO = boneTfrm.gameObject;

                    ConfigureBoneCollider (bodyBone, parentBone, childBone, isAddEnd, colliderType, colRadiusFactor);

                    // The hips don't need a character joint.
                    if (isHips)
                    {
                        if (!boneTfrm.TryGetComponent(out rb))
                        {
                            rb = Undo.AddComponent<Rigidbody>(boneGO);
                        }

                        if (rb != null)
                        {
                            // Create disabled (non-dyanmic)
                            rb.isKinematic = true;
                            rb.mass = normalisedMass * stickyMass;
                        }
                    }
                    else
                    {
                        if (!boneTfrm.TryGetComponent(out joint))
                        {
                            joint = Undo.AddComponent<CharacterJoint>(boneGO);
                        }

                        if (boneTfrm.TryGetComponent(out rb))
                        {
                            // Create disabled (non-dynamic)
                            rb.isKinematic = true;
                            rb.mass = normalisedMass * stickyMass;
                        }

                        #region Configure the joint
                        if (joint != null)
                        {
                            // Prevent issues spawning inside geometry
                            joint.enablePreprocessing = false;

                            joint.anchor = Vector3.zero;

                            if (parentBone != null)
                            {
                                // Set the twist axis
                                joint.axis = S3DMath.LongestAxisDirection(boneTfrm.InverseTransformDirection(twistAxis));

                                // Set the swing axis
                                joint.swingAxis = S3DMath.LongestAxisDirection(boneTfrm.InverseTransformDirection(swingAxis));

                                joint.connectedBody = parentBone.boneTransform.GetComponent<Rigidbody>();

                                SoftJointLimit jointLimit = new SoftJointLimit();
                                // Set contact distance to auto-calculated
                                jointLimit.contactDistance = 0;

                                jointLimit.limit = twistMinLimit;
                                joint.lowTwistLimit = jointLimit;
                                jointLimit.limit = twistMaxLimit;
                                joint.highTwistLimit = jointLimit;
                                jointLimit.limit = swingLimit;
                                joint.swing1Limit = jointLimit;
                                jointLimit.limit = 0f;
                                joint.swing2Limit = jointLimit;
                            }
                        }
                        #endregion
                    }
                }
            }
        }

        /// <summary>
        /// Set up rich text GUIStyles
        /// </summary>
        private void ConfigureButtonsAndStyles()
        {
            if (!isStylesInitialised)
            {
                helpBoxRichText = new GUIStyle("HelpBox");
                helpBoxRichText.richText = true;

                labelFieldRichText = new GUIStyle("Label");
                labelFieldRichText.richText = true;

                headingFieldRichText = new GUIStyle(UnityEditor.EditorStyles.miniLabel);
                headingFieldRichText.richText = true;
                headingFieldRichText.normal.textColor = helpBoxRichText.normal.textColor;

                // Overide default styles
                EditorStyles.foldout.fontStyle = FontStyle.Bold;

                // When using a no-label foldout, don't forget to set the global
                // EditorGUIUtility.fieldWidth to a small value like 15, then back
                // to the original afterward.
                foldoutStyleNoLabel = new GUIStyle(EditorStyles.foldout);
                foldoutStyleNoLabel.fixedWidth = 0.01f;

                buttonCompact = new GUIStyle("Button");
                buttonCompact.fontSize = 10;

                // Create a new button or else will effect the Button style for other buttons too
                toggleCompactButtonStyleNormal = new GUIStyle("Button");
                toggleCompactButtonStyleToggled = new GUIStyle(toggleCompactButtonStyleNormal);
                toggleCompactButtonStyleNormal.fontStyle = FontStyle.Normal;
                toggleCompactButtonStyleToggled.fontStyle = FontStyle.Bold;
                toggleCompactButtonStyleToggled.normal.background = toggleCompactButtonStyleToggled.active.background;

                isStylesInitialised = true;
            }
        }

        /// <summary>
        /// Attempt to create humanoid damage regions based on a pre-determined set of bones
        /// </summary>
        private void CreateDamageRegions()
        {
            if (!stickyControlModule.IsValidHumanoid(true))
            {
                StickyEditorHelper.PromptGotIt("Create Damage Regions", "Sorry, your character does not seem to be configured as a humanoid. Check the Animator on the Animate tab, and the model Rig settings.");
            }
            else
            {
                Animator animator = stickyControlModule.defaultAnimator;

                S3DHumanBonePersist hipsBone = new S3DHumanBonePersist(HumanBodyBones.Hips, animator.GetBoneTransform(HumanBodyBones.Hips));
                S3DHumanBonePersist chestBone = new S3DHumanBonePersist(HumanBodyBones.Chest, animator.GetBoneTransform(HumanBodyBones.Chest));
                S3DHumanBonePersist headBone = new S3DHumanBonePersist(HumanBodyBones.Head, animator.GetBoneTransform(HumanBodyBones.Head));
                S3DHumanBonePersist leftUpperArmBone = new S3DHumanBonePersist(HumanBodyBones.LeftUpperArm, animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));
                S3DHumanBonePersist leftLowerArmBone = new S3DHumanBonePersist(HumanBodyBones.LeftLowerArm, animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
                S3DHumanBonePersist leftHandBone = new S3DHumanBonePersist(HumanBodyBones.LeftHand, animator.GetBoneTransform(HumanBodyBones.LeftHand));
                S3DHumanBonePersist rightUpperArmBone = new S3DHumanBonePersist(HumanBodyBones.RightUpperArm, animator.GetBoneTransform(HumanBodyBones.RightUpperArm));
                S3DHumanBonePersist rightLowerArmBone = new S3DHumanBonePersist(HumanBodyBones.RightLowerArm, animator.GetBoneTransform(HumanBodyBones.RightLowerArm));
                S3DHumanBonePersist rightHandBone = new S3DHumanBonePersist(HumanBodyBones.RightHand, animator.GetBoneTransform(HumanBodyBones.RightHand));
                S3DHumanBonePersist leftUpperLegBone = new S3DHumanBonePersist(HumanBodyBones.LeftUpperLeg, animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg));
                S3DHumanBonePersist leftLowerLegBone = new S3DHumanBonePersist(HumanBodyBones.LeftLowerLeg, animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg));
                S3DHumanBonePersist leftFootBone = new S3DHumanBonePersist(HumanBodyBones.LeftFoot, animator.GetBoneTransform(HumanBodyBones.LeftFoot));
                S3DHumanBonePersist rightUpperLegBone = new S3DHumanBonePersist(HumanBodyBones.RightUpperLeg, animator.GetBoneTransform(HumanBodyBones.RightUpperLeg));
                S3DHumanBonePersist rightLowerLegBone = new S3DHumanBonePersist(HumanBodyBones.RightLowerLeg, animator.GetBoneTransform(HumanBodyBones.RightLowerLeg));
                S3DHumanBonePersist rightFootBone = new S3DHumanBonePersist(HumanBodyBones.RightFoot, animator.GetBoneTransform(HumanBodyBones.RightFoot));

                Undo.SetCurrentGroupName("Create Damage Regions");
                int undoGroup = UnityEditor.Undo.GetCurrentGroup();
                Undo.RecordObject(stickyControlModule, string.Empty);

                S3DDamageRegion damageRegion;

                damageRegion = stickyControlModule.GetOrCreateDamageRegion(HumanBodyBones.Hips);
                if (damageRegion != null)
                {
                    damageRegion.s3dHumanBonePersist.boneCollider = ConfigureBoneCollider(hipsBone, null, null, false, typeof(BoxCollider), 0.7f);
                }

                damageRegion = stickyControlModule.GetOrCreateDamageRegion(HumanBodyBones.Chest);
                if (damageRegion != null)
                {
                    damageRegion.s3dHumanBonePersist.boneCollider = ConfigureBoneCollider(chestBone, hipsBone, headBone, false, typeof(BoxCollider), 0.7f);
                }

                damageRegion = stickyControlModule.GetOrCreateDamageRegion(HumanBodyBones.Head);
                if (damageRegion != null)
                {
                    damageRegion.s3dHumanBonePersist.boneCollider = ConfigureBoneCollider(headBone, chestBone, null, false, typeof(CapsuleCollider), 1f);
                }

                damageRegion = stickyControlModule.GetOrCreateDamageRegion(HumanBodyBones.LeftUpperArm);
                if (damageRegion != null)
                {
                    damageRegion.s3dHumanBonePersist.boneCollider = ConfigureBoneCollider(leftUpperArmBone, chestBone, leftLowerArmBone, false, typeof(CapsuleCollider), 0.25f);
                }

                damageRegion = stickyControlModule.GetOrCreateDamageRegion(HumanBodyBones.LeftLowerArm);
                if (damageRegion != null)
                {
                    damageRegion.s3dHumanBonePersist.boneCollider = ConfigureBoneCollider(leftLowerArmBone, leftUpperArmBone, leftHandBone, true, typeof(CapsuleCollider), 0.25f);
                }
                damageRegion = stickyControlModule.GetOrCreateDamageRegion(HumanBodyBones.RightUpperArm);
                if (damageRegion != null)
                {
                    damageRegion.s3dHumanBonePersist.boneCollider = ConfigureBoneCollider(rightUpperArmBone, chestBone, rightLowerArmBone, false, typeof(CapsuleCollider), 0.25f);
                }

                damageRegion = stickyControlModule.GetOrCreateDamageRegion(HumanBodyBones.RightLowerArm);
                if (damageRegion != null)
                {
                    damageRegion.s3dHumanBonePersist.boneCollider = ConfigureBoneCollider(rightLowerArmBone, rightUpperArmBone, rightHandBone, true, typeof(CapsuleCollider), 0.25f);
                }

                damageRegion = stickyControlModule.GetOrCreateDamageRegion(HumanBodyBones.LeftUpperLeg);
                if (damageRegion != null)
                {
                    damageRegion.s3dHumanBonePersist.boneCollider = ConfigureBoneCollider(leftUpperLegBone, hipsBone, leftLowerLegBone, false, typeof(CapsuleCollider), 0.3f);
                }

                damageRegion = stickyControlModule.GetOrCreateDamageRegion(HumanBodyBones.LeftLowerLeg);
                if (damageRegion != null)
                {
                    damageRegion.s3dHumanBonePersist.boneCollider = ConfigureBoneCollider(leftLowerLegBone, leftUpperLegBone, leftFootBone, true, typeof(CapsuleCollider), 0.25f);
                }

                damageRegion = stickyControlModule.GetOrCreateDamageRegion(HumanBodyBones.RightUpperLeg);
                if (damageRegion != null)
                {
                    damageRegion.s3dHumanBonePersist.boneCollider = ConfigureBoneCollider(rightUpperLegBone, hipsBone, rightLowerLegBone, false, typeof(CapsuleCollider), 0.3f);
                }

                damageRegion = stickyControlModule.GetOrCreateDamageRegion(HumanBodyBones.RightLowerLeg);
                if (damageRegion != null)
                {
                    damageRegion.s3dHumanBonePersist.boneCollider = ConfigureBoneCollider(rightLowerLegBone, rightUpperLegBone, rightFootBone, true, typeof(CapsuleCollider), 0.25f);
                }

                ExpandList(stickyControlModule.DamageRegionList, false);

                Undo.CollapseUndoOperations(undoGroup);
            }
        }

        /// <summary>
        /// Attempt to configure ragdoll components for this character.
        /// This assumes character is in T-Pose.
        /// </summary>
        private void CreateRagdoll()
        {
            List<S3DHumanBonePersist> bones = stickyControlModule.RagdollBoneList;

            int numBones = bones == null ? 0 : bones.Count;
            int undoGroup = 0;

            if (numBones > 0)
            {
                S3DHumanBonePersist hipsBone = stickyControlModule.GetRagdollBone(HumanBodyBones.Hips);

                if (hipsBone != null && hipsBone.boneTransform != null)
                {
                    float totalRelativeMass = 0f;
                    Transform hipTfrm = hipsBone.boneTransform;

                    Undo.SetCurrentGroupName("Create Ragdoll");
                    undoGroup = UnityEditor.Undo.GetCurrentGroup();
                    Undo.RecordObject(stickyControlModule, string.Empty);

                    // Get all the bones we need
                    S3DHumanBonePersist chestBone = stickyControlModule.GetRagdollBone(HumanBodyBones.Chest);
                    S3DHumanBonePersist headBone = stickyControlModule.GetRagdollBone(HumanBodyBones.Head);

                    S3DHumanBonePersist leftUpperLegBone = stickyControlModule.GetRagdollBone(HumanBodyBones.LeftUpperLeg);
                    S3DHumanBonePersist rightUpperLegBone = stickyControlModule.GetRagdollBone(HumanBodyBones.RightUpperLeg);
                    S3DHumanBonePersist leftLowerLegBone = stickyControlModule.GetRagdollBone(HumanBodyBones.LeftLowerLeg);
                    S3DHumanBonePersist rightLowerLegBone = stickyControlModule.GetRagdollBone(HumanBodyBones.RightLowerLeg);
                    S3DHumanBonePersist leftFootBone = stickyControlModule.GetRagdollBone(HumanBodyBones.LeftFoot);
                    S3DHumanBonePersist rightFootBone = stickyControlModule.GetRagdollBone(HumanBodyBones.RightFoot);

                    S3DHumanBonePersist leftUpperArmBone = stickyControlModule.GetRagdollBone(HumanBodyBones.LeftUpperArm);
                    S3DHumanBonePersist rightUpperArmBone = stickyControlModule.GetRagdollBone(HumanBodyBones.RightUpperArm);
                    S3DHumanBonePersist leftLowerArmBone = stickyControlModule.GetRagdollBone(HumanBodyBones.LeftLowerArm);
                    S3DHumanBonePersist rightLowerArmBone = stickyControlModule.GetRagdollBone(HumanBodyBones.RightLowerArm);
                    S3DHumanBonePersist leftHandBone = stickyControlModule.GetRagdollBone(HumanBodyBones.LeftHand);
                    S3DHumanBonePersist rightHandBone = stickyControlModule.GetRagdollBone(HumanBodyBones.RightHand);

                    // This assumes character is in T-Pose...
                    Vector3 hipFwdWS = hipTfrm.TransformDirection(Vector3.forward);
                    Vector3 hipRightWS = hipTfrm.TransformDirection(Vector3.right);
                    Vector3 hipUpWS = hipTfrm.TransformDirection(Vector3.up);

                    // Set the relative mass of each bone
                    hipsBone.relativeMass = 1f; totalRelativeMass += hipsBone.relativeMass;
                    if (chestBone != null) { chestBone.relativeMass = 1f; totalRelativeMass += chestBone.relativeMass; }
                    if (headBone != null) { headBone.relativeMass = 0.4f; totalRelativeMass += headBone.relativeMass; }
                    if (leftUpperLegBone != null) { leftUpperLegBone.relativeMass = 0.6f; totalRelativeMass += leftUpperLegBone.relativeMass; }
                    if (rightUpperLegBone != null) { rightUpperLegBone.relativeMass = 0.6f; totalRelativeMass += rightUpperLegBone.relativeMass; }
                    if (leftLowerLegBone != null) { leftLowerLegBone.relativeMass = 0.6f; totalRelativeMass += leftLowerLegBone.relativeMass; }
                    if (rightLowerLegBone != null) { rightLowerLegBone.relativeMass = 0.6f; totalRelativeMass += rightLowerLegBone.relativeMass; }
                    if (leftUpperArmBone != null) { leftUpperArmBone.relativeMass = 0.4f; totalRelativeMass += leftUpperArmBone.relativeMass; }
                    if (rightUpperArmBone != null) { rightUpperArmBone.relativeMass = 0.4f; totalRelativeMass += rightUpperArmBone.relativeMass; }
                    if (leftLowerArmBone != null) { leftLowerArmBone.relativeMass = 0.4f; totalRelativeMass += leftLowerArmBone.relativeMass; }
                    if (rightLowerArmBone != null) { rightLowerArmBone.relativeMass = 0.4f; totalRelativeMass += rightLowerArmBone.relativeMass; }

                    ConfigureRagdollBone(hipsBone, null, null, false, typeof(BoxCollider), 0.7f, Vector3.zero, Vector3.zero, 0f, 0f, 0f, S3DMath.Normalise(hipsBone.relativeMass, 0f, totalRelativeMass));
                    ConfigureRagdollBone(chestBone, hipsBone, headBone, false, typeof(BoxCollider), 0.7f, hipRightWS, hipFwdWS, -20f, 20f, 10f, S3DMath.Normalise(chestBone.relativeMass, 0f, totalRelativeMass));
                    ConfigureRagdollBone(headBone, chestBone, null, false, typeof(CapsuleCollider), 1f, hipRightWS, hipFwdWS, -40f, 25f, 25f, S3DMath.Normalise(headBone.relativeMass, 0f, totalRelativeMass));

                    ConfigureRagdollBone(leftUpperLegBone, hipsBone, leftLowerLegBone, false, typeof(CapsuleCollider), 0.3f, hipRightWS, hipFwdWS, -20f, 70f, 30f, S3DMath.Normalise(leftUpperLegBone.relativeMass, 0f, totalRelativeMass));
                    ConfigureRagdollBone(rightUpperLegBone, hipsBone, rightLowerLegBone, false, typeof(CapsuleCollider), 0.3f, hipRightWS, hipFwdWS, -20f, 70f, 30f, S3DMath.Normalise(rightUpperLegBone.relativeMass, 0f, totalRelativeMass));
                    ConfigureRagdollBone(leftLowerLegBone, leftUpperLegBone, leftFootBone, true, typeof(CapsuleCollider), 0.25f, hipRightWS, hipFwdWS, -80f, 0f, 0f, S3DMath.Normalise(leftLowerLegBone.relativeMass, 0f, totalRelativeMass));
                    ConfigureRagdollBone(rightLowerLegBone, rightUpperLegBone, rightFootBone, true, typeof(CapsuleCollider), 0.25f, hipRightWS, hipFwdWS, -80f, 0f, 0f, S3DMath.Normalise(rightLowerLegBone.relativeMass, 0f, totalRelativeMass));

                    ConfigureRagdollBone(leftUpperArmBone, chestBone, leftLowerArmBone, false, typeof(CapsuleCollider), 0.25f, hipUpWS, hipFwdWS, -70f, 10f, 50f, S3DMath.Normalise(leftUpperArmBone.relativeMass, 0f, totalRelativeMass));
                    ConfigureRagdollBone(rightUpperArmBone, chestBone, rightLowerArmBone, false, typeof(CapsuleCollider), 0.25f, hipUpWS, hipFwdWS, -70f, 10f, 50f, S3DMath.Normalise(rightUpperArmBone.relativeMass, 0f, totalRelativeMass));
                    ConfigureRagdollBone(leftLowerArmBone, leftUpperArmBone, leftHandBone, true, typeof(CapsuleCollider), 0.25f, hipFwdWS, hipUpWS, -90f, 0f, 0f, S3DMath.Normalise(leftLowerArmBone.relativeMass, 0f, totalRelativeMass));
                    ConfigureRagdollBone(rightLowerArmBone, rightUpperArmBone, rightHandBone, true, typeof(CapsuleCollider), 0.25f, hipFwdWS, hipUpWS, -90f, 0f, 0f, S3DMath.Normalise(rightLowerArmBone.relativeMass, 0f, totalRelativeMass));

                    Undo.CollapseUndoOperations(undoGroup);
                }
                else
                {
                    Debug.LogWarning("Sticky3D CreateRagdoll failed because could not find Hips (root) bone. Click Get Bones and try again");
                }
            }

        }

        /// <summary>
        ///  Deselect all components in the scene view edit mode, and unhides the Unity tools
        /// </summary>
        private void DeselectAllComponents()
        {
            // Set all components to not be selected

            // Avoid situation where character is destroyed in play mode while it is selected.
            if (stickyControlModule != null)
            {
                int numEquipPoints = stickyControlModule.NumberOfEquipPoints;

                for (int eqIdx = 0; eqIdx < numEquipPoints; eqIdx++)
                {
                    stickyControlModule.EquipPointList[eqIdx].selectedInSceneView = false;
                }
            }

            isLeftHandSelected = false;
            isRightHandSelected = false;

            // Unhide Unity tools
            Tools.hidden = false;
        }

        /// <summary>
        /// Draw an audio clip with listen button and name and tooltip
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="audioClipContent"></param>
        private void DrawAudioClip(SerializedProperty audioClip, GUIContent audioClipContent)
        {
            GUILayout.BeginHorizontal();
#if UNITY_2019_1_OR_NEWER
            EditorGUILayout.LabelField(audioClipContent, GUILayout.Width(defaultEditorLabelWidth - 23f));
#else
            EditorGUILayout.LabelField(audioClipContent, GUILayout.Width(defaultEditorLabelWidth - 28f));
#endif
            if (GUILayout.Button(footStepsListenContent, buttonCompact, GUILayout.MaxWidth(20f)))
            {
                if (isFootstepClipPlaying)
                {
                    StickyEditorHelper.StopAllAudioClips();
                    isFootstepClipPlaying = false;
                }
                else if (audioClip.objectReferenceValue != null)
                {
                    StickyEditorHelper.PlayAudioClip((AudioClip)audioClip.objectReferenceValue, 0, false);
                    isFootstepClipPlaying = true;
                }
            }
            EditorGUILayout.PropertyField(audioClip, GUIContent.none);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a damage region in the inspector
        /// </summary>
        /// <param name="s3dDamageRegionProp"></param>
        /// <param name="isMainDamageRegion"></param>
        /// <param name="damageRegionIndex"></param>
        /// <param name="numberOfDamageRegions"></param>
        private void DrawDamageRegion(SerializedProperty s3dDamageRegionProp, int damageRegionIndex, int numberOfDamageRegions)
        {
            EditorGUILayout.BeginVertical("HelpBox");

            damageRegionNameProp = s3dDamageRegionProp.FindPropertyRelative("name");
            damageRegionShowInEditorProp = s3dDamageRegionProp.FindPropertyRelative("showInEditor");
            //damageRegionShowGizmosInSceneViewProp = s3dDamageRegionProp.FindPropertyRelative("showGizmosInSceneView");

            if (damageRegionIndex == 0)
            {
                StickyEditorHelper.DrawS3DFoldout(damageRegionShowInEditorProp, new GUIContent((damageRegionIndex+1).ToString("00 ") + damageRegionNameProp.stringValue), foldoutStyleNoLabel, defaultEditorFieldWidth);
            }
            else
            {
                GUILayout.BeginHorizontal();

                StickyEditorHelper.DrawS3DFoldout(damageRegionShowInEditorProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                EditorGUILayout.LabelField((damageRegionIndex + 1).ToString("00 ") + damageRegionNameProp.stringValue);
                // Move down button
                if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numberOfDamageRegions > 1) { s3dDmRgnMoveDownPos = damageRegionIndex; }
                // Create duplicate button
                if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { s3dDmRgnInsertPos = damageRegionIndex; }
                // Delete button
                if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dDmRgnDeletePos = damageRegionIndex; }
                GUILayout.EndHorizontal();
            }

            if (damageRegionShowInEditorProp.boolValue)
            {
                if (damageRegionIndex > 0)
                {
                    EditorGUILayout.PropertyField(damageRegionNameProp, damageRegionNameContent);
                }

                // health is stored internally as 0-1 value
                startingHealthProp = s3dDamageRegionProp.FindPropertyRelative("startingHealth");
                damageRegionShowMultipliersInEditorProp = s3dDamageRegionProp.FindPropertyRelative("showMultipliersInEditor");

                EditorGUI.BeginChangeCheck();
                startingHealthProp.floatValue = EditorGUILayout.Slider(damageRegionStartHealthContent, startingHealthProp.floatValue * 100f, 0f, 100f) / 100f;
                if (EditorGUI.EndChangeCheck() && damageRegionIndex == 0)
                {
                    serializedObject.ApplyModifiedProperties();
                    stickyControlModule.SetHealth(startingHealthProp.floatValue * 100f);
                    serializedObject.Update();
                }

                EditorGUILayout.PropertyField(s3dDamageRegionProp.FindPropertyRelative("isInvincible"), damageRegionInvincibleContent);
                useShieldingProp = s3dDamageRegionProp.FindPropertyRelative("useShielding");
                EditorGUILayout.PropertyField(useShieldingProp, damageRegionUseShieldingContent);
                if (useShieldingProp.boolValue)
                {
                    EditorGUILayout.PropertyField(s3dDamageRegionProp.FindPropertyRelative("shieldingDamageThreshold"), damageRegionShieldingDamageThresholdContent);
                    EditorGUILayout.PropertyField(s3dDamageRegionProp.FindPropertyRelative("shieldingAmount"), damageRegionShieldingAmountContent);

                    shieldingRechargeRateProp = s3dDamageRegionProp.FindPropertyRelative("shieldingRechargeRate");
                    EditorGUILayout.PropertyField(shieldingRechargeRateProp, shieldingRechargeRateContent);
                    if (shieldingRechargeRateProp.floatValue > 0f)
                    {
                        EditorGUILayout.PropertyField(s3dDamageRegionProp.FindPropertyRelative("shieldingRechargeDelay"), shieldingRechargeDelayContent);
                    }
                }
                EditorGUILayout.PropertyField(s3dDamageRegionProp.FindPropertyRelative("collisionDamageResistance"), damageRegionCollisionDamageResistanceContent);

                // Damage region collider
                if (damageRegionIndex > 0)
                {
                    damageRegionBonePersistProp = s3dDamageRegionProp.FindPropertyRelative("s3dHumanBonePersist");
                    damageRegionColliderProp = damageRegionBonePersistProp.FindPropertyRelative("boneCollider");
                    damageRegionTransformProp = damageRegionBonePersistProp.FindPropertyRelative("boneTransform");

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(damageRegionTransformContent, GUILayout.Width(defaultEditorLabelWidth));
                    if (damageRegionTransformProp.objectReferenceValue == null)
                    {
                        EditorGUILayout.LabelField(debugNotSetContent);
                    }
                    else
                    {
                        EditorGUILayout.LabelField(new GUIContent(((Transform)damageRegionTransformProp.objectReferenceValue).name));
                    }
                    GUILayout.EndHorizontal();

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(damageRegionColliderProp, damageRegionColliderContent);
                    if (EditorGUI.EndChangeCheck() && damageRegionColliderProp.objectReferenceValue != null)
                    {
                        if (!((Collider)damageRegionColliderProp.objectReferenceValue).transform.IsChildOf(stickyControlModule.transform))
                        {
                            damageRegionColliderProp.objectReferenceValue = null;
                            Debug.LogWarning("The collider for damage region (" + damageRegionNameProp.stringValue + ") must be a child of the Sticky3D Controller gameobject or part of the prefab on " + stickyControlModule.name);
                        }
                    }
                }

                // If this is the main damage region and there are other damage regions,
                // give user option to copy damage multiplier settings to all other regions.
                if (damageRegionIndex == 0 && numberOfDamageRegions > 1)
                {
                    GUILayout.BeginHorizontal();
                    StickyEditorHelper.DrawS3DFoldout(damageRegionShowMultipliersInEditorProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                    EditorGUILayout.LabelField(damageRegionMultiplierContent);
                    if (GUILayout.Button("Copy > All", buttonCompact, GUILayout.MaxWidth(68f)))
                    {
                        serializedObject.ApplyModifiedProperties();
                        stickyControlModule.CopyDamageMultipliersToAll(stickyControlModule.GetDamageRegionByIndex(damageRegionIndex));
                        serializedObject.Update();
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    StickyEditorHelper.DrawS3DFoldout(damageRegionShowMultipliersInEditorProp, damageRegionMultiplierContent, foldoutStyleNoLabel, defaultEditorFieldWidth);
                }

                if (damageRegionShowMultipliersInEditorProp.boolValue)
                {
                    StickyEditorHelper.DrawInformationLabel(damageRegionMultiplierDescContent, 3f);

                    DrawDamageMultipliers(damageRegionIndex);
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw the array of damage region multipliers for a DamageRegion of the character.
        /// Includes support for Editor Undo/Redo.
        /// </summary>
        /// <param name="damageRegionIndex"></param>
        private void DrawDamageMultipliers(int damageRegionIndex)
        {
            S3DDamageRegion damageRegion = stickyControlModule.GetDamageRegionByIndex(damageRegionIndex);

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
                S3DDamageRegion.DamageType thisDamageType = S3DDamageRegion.DamageType.Default;
                GUIContent thisDamageTypeGUIContent = typeADamageMultiplierContent;
                switch (dTypeIdx)
                {
                    case 0:
                        thisDamageType = S3DDamageRegion.DamageType.TypeA;
                        thisDamageTypeGUIContent = typeADamageMultiplierContent;
                        break;
                    case 1:
                        thisDamageType = S3DDamageRegion.DamageType.TypeB;
                        thisDamageTypeGUIContent = typeBDamageMultiplierContent;
                        break;
                    case 2:
                        thisDamageType = S3DDamageRegion.DamageType.TypeC;
                        thisDamageTypeGUIContent = typeCDamageMultiplierContent;
                        break;
                    case 3:
                        thisDamageType = S3DDamageRegion.DamageType.TypeD;
                        thisDamageTypeGUIContent = typeDDamageMultiplierContent;
                        break;
                    case 4:
                        thisDamageType = S3DDamageRegion.DamageType.TypeE;
                        thisDamageTypeGUIContent = typeEDamageMultiplierContent;
                        break;
                    case 5:
                        thisDamageType = S3DDamageRegion.DamageType.TypeF;
                        thisDamageTypeGUIContent = typeFDamageMultiplierContent;
                        break;
                }

                float thisDamageMultiplier = damageRegion.GetDamageMultiplier(thisDamageType);
                EditorGUI.BeginChangeCheck();
                thisDamageMultiplier = EditorGUILayout.FloatField(thisDamageTypeGUIContent, thisDamageMultiplier);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(stickyControlModule, "Modify Damage " + thisDamageType.ToString());
                    damageRegion.SetDamageMultiplier(thisDamageType, thisDamageMultiplier);
                }
            }
            // Read in the properties
            serializedObject.Update();
        }

        /// <summary>
        /// Draw equip point settings in the inspector
        /// </summary>
        private void DrawEngageEquipSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            StickyEditorHelper.DrawS3DFoldout(isEngageEquipExpandedProp, isEngageEquipExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (isEngageEquipExpandedProp.boolValue)
            {
                StickyEditorHelper.DrawInformationLabel(engageEquipContent);

                #region Equip General Settings
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(minEquipDropDistanceProp, minEquipDropDistanceContent);
                if (EditorGUI.EndChangeCheck())
                {
                    if (minEquipDropDistanceProp.floatValue > maxEquipDropDistanceProp.floatValue)
                    {
                        maxEquipDropDistanceProp.floatValue = minEquipDropDistanceProp.floatValue;
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(maxEquipDropDistanceProp, maxEquipDropDistanceContent);
                if (EditorGUI.EndChangeCheck())
                {
                    if (maxEquipDropDistanceProp.floatValue < minEquipDropDistanceProp.floatValue)
                    {
                        minEquipDropDistanceProp.floatValue = maxEquipDropDistanceProp.floatValue;
                    }
                }

                #endregion

                #region Add-Remove Equip Points

                int numEquipPoints = equipPointListProp.arraySize;

                // Reset button variables
                s3dEquipPtMoveDownPos = -1;
                s3dEquipPtInsertPos = -1;
                s3dEquipPtDeletePos = -1;

                GUILayout.BeginHorizontal();

                EditorGUI.indentLevel += 1;
                EditorGUIUtility.fieldWidth = 15f;
                EditorGUI.BeginChangeCheck();
                isEngageEquipPtListExpandedProp.boolValue = EditorGUILayout.Foldout(isEngageEquipPtListExpandedProp.boolValue, "", foldoutStyleNoLabel);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(stickyControlModule.EquipPointList, isEngageEquipPtListExpandedProp.boolValue);
                    // Read in the properties
                    serializedObject.Update();
                }
                EditorGUI.indentLevel -= 1;

                EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                EditorGUILayout.LabelField("<color=" + txtColourName + ">Equip Points: " + numEquipPoints.ToString("00") + "</color>", labelFieldRichText);

                if (GUILayout.Button("G", GUILayout.MaxWidth(30f)))
                {
                    bool isEnableGizmos = false;
                    // Toggle show Gizmos in scene for all equip points
                    for (int eqIdx = 0; eqIdx < numEquipPoints; eqIdx++)
                    {
                        equipPointProp = equipPointListProp.GetArrayElementAtIndex(eqIdx);
                        equipPtShowGizmoInSceneViewProp = equipPointProp.FindPropertyRelative("showGizmosInSceneView");
                        equipPtIsSelectedInSceneViewProp = equipPointProp.FindPropertyRelative("selectedInSceneView");

                        // Toggle based on the status of the first equip point
                        if (eqIdx == 0) { isEnableGizmos = !equipPtShowGizmoInSceneViewProp.boolValue; }

                        // If selected in scene, unselect it.
                        equipPtIsSelectedInSceneViewProp.boolValue = false;
                        equipPtShowGizmoInSceneViewProp.boolValue = isEnableGizmos;
                    }
                }
                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(stickyControlModule, "Add Equip Point");
                    stickyControlModule.AddEquipPoint(new S3DEquipPoint());
                    ExpandList(stickyControlModule.EquipPointList, false);
                    isSceneModified = true;
                    // Read in the properties
                    serializedObject.Update();

                    numEquipPoints = equipPointListProp.arraySize;
                    if (numEquipPoints > 0)
                    {
                        // Force new Equip Point to be serialized in scene
                        equipPointProp = equipPointListProp.GetArrayElementAtIndex(numEquipPoints - 1);
                        equipPointShowInEditorProp = equipPointProp.FindPropertyRelative("showInEditor");
                        equipPointShowInEditorProp.boolValue = !equipPointShowInEditorProp.boolValue;
                        // Show the new Equip Point
                        equipPointShowInEditorProp.boolValue = true;
                    }
                }
                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (numEquipPoints > 0) { s3dEquipPtDeletePos = equipPointListProp.arraySize - 1; }
                }

                GUILayout.EndHorizontal();

                #endregion

                #region Draw list of Equip Points

                for (int eqIdx = 0; eqIdx < numEquipPoints; eqIdx++)
                {
                    equipPointProp = equipPointListProp.GetArrayElementAtIndex(eqIdx);
                    equipPointShowInEditorProp = equipPointProp.FindPropertyRelative("showInEditor");
                    equipPointNameProp = equipPointProp.FindPropertyRelative("equipPointName");
                    equipPointMaxItemsProp = equipPointProp.FindPropertyRelative("maxItems");
                    equipPointPermittedTagsProp = equipPointProp.FindPropertyRelative("permittedTags");
                    equipPointParentTransformProp = equipPointProp.FindPropertyRelative("parentTransform");
                    equipPointRelativeOffsetProp = equipPointProp.FindPropertyRelative("relativeOffset");
                    equipPointRelativeRotationProp = equipPointProp.FindPropertyRelative("relativeRotation");
                    equipPtShowGizmoInSceneViewProp = equipPointProp.FindPropertyRelative("showGizmosInSceneView");
                    equipPtIsSelectedInSceneViewProp = equipPointProp.FindPropertyRelative("selectedInSceneView");

                    EditorGUILayout.BeginVertical("HelpBox");

                    GUILayout.BeginHorizontal();
                    StickyEditorHelper.DrawS3DFoldout(equipPointShowInEditorProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                    EditorGUILayout.LabelField((eqIdx + 1).ToString("00 ") + equipPointNameProp.stringValue);
                    // Find (select) in the scene
                    bool isSelected = equipPtIsSelectedInSceneViewProp.boolValue;
                    SelectItemInSceneViewButton(ref isSelected, equipPtShowGizmoInSceneViewProp);
                    // Only update if it has changed
                    if (isSelected != equipPtIsSelectedInSceneViewProp.boolValue)
                    {
                        equipPtIsSelectedInSceneViewProp.boolValue = isSelected;
                    }
                    // Toggle selection in scene view on/off
                    StickyEditorHelper.DrawGizmosButton(equipPtShowGizmoInSceneViewProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled);
                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numEquipPoints > 1) { s3dEquipPtMoveDownPos = eqIdx; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { s3dEquipPtInsertPos = eqIdx; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dEquipPtDeletePos = eqIdx; }
                    GUILayout.EndHorizontal();

                    if (equipPointShowInEditorProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(equipPointNameProp, equipPointNameContent);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.LabelField(equipPointTransformContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
                        if (GUILayout.Button(equipPointGetBoneContent, buttonCompact, GUILayout.MaxWidth(20f)))
                        {
                            // Apply property changes
                            serializedObject.ApplyModifiedProperties();

                            Transform transform = equipPointParentTransformProp.objectReferenceValue as Transform;

                            // Create a drop down list of all the locations
                            GenericMenu dropdown = new GenericMenu();
                            dropdown.AddItem(new GUIContent("None"), transform == null, UpdateEngageEquipPointTransform, new Vector2Int(0, eqIdx));

                            if (stickyControlModule.IsValidHumanoid())
                            {
                                if (rigHumanBoneList.Count == 0) { S3DUtils.GetHumanBones(stickyControlModule.defaultAnimator, rigHumanBoneList); }    

                                int selectedTransformID = transform == null ? 0 : transform.GetInstanceID();

                                for (int bnIdx = 0; bnIdx < rigHumanBoneList.Count; bnIdx++)
                                {
                                    S3DHumanBone humanBone = rigHumanBoneList[bnIdx];
                                    string tempBoneName = humanBone.bone.ToString();

                                    dropdown.AddItem(new GUIContent(tempBoneName), selectedTransformID == humanBone.boneTransform.GetInstanceID() , UpdateEngageEquipPointTransform, new Vector2Int(humanBone.guidHash, eqIdx));
                                }
                            }

                            dropdown.ShowAsContext();
                            SceneView.RepaintAll();

                            serializedObject.Update();
                        }
                        EditorGUILayout.PropertyField(equipPointParentTransformProp, GUIContent.none);
                        if (EditorGUI.EndChangeCheck() && equipPointParentTransformProp.objectReferenceValue != null)
                        {
                            if (!((Transform)equipPointParentTransformProp.objectReferenceValue).IsChildOf(stickyControlModule.transform))
                            {
                                equipPointParentTransformProp.objectReferenceValue = null;
                                Debug.LogWarning("The Equip Point parent transform must be a child of the parent Sticky3D Controller gameobject or part of the prefab.");
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(equipPointRelativeOffsetContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
                        if (GUILayout.Button(StickyEditorHelper.resetBtnSmlContent, buttonCompact, GUILayout.MaxWidth(20f)))
                        {
                            equipPointRelativeOffsetProp.vector3Value = Vector3.zero;
                        }
                        EditorGUILayout.PropertyField(equipPointRelativeOffsetProp, GUIContent.none);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(equipPointRelativeRotationContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
                        if (GUILayout.Button(StickyEditorHelper.resetBtnSmlContent, buttonCompact, GUILayout.MaxWidth(20f)))
                        {
                            equipPointRelativeRotationProp.vector3Value = Vector3.zero;
                        }
                        EditorGUILayout.PropertyField(equipPointRelativeRotationProp, GUIContent.none);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.PropertyField(equipPointMaxItemsProp, equipPointMaxItemsContent);

                        if (interactiveTagNames == null)
                        {
                            if (interactiveTagsProp.objectReferenceValue != null)
                            {
                                interactiveTagNames = ((S3DInteractiveTags)interactiveTagsProp.objectReferenceValue).GetTagNames(true);
                            }
                            else
                            {
                                interactiveTagNames = new string[] { "Default" };
                            }
                        }

                        equipPointPermittedTagsProp.intValue = EditorGUILayout.MaskField(equipPointPermittedTagsContent, equipPointPermittedTagsProp.intValue, interactiveTagNames);
                    }

                    EditorGUILayout.EndVertical();


                }

                #endregion

                #region Move/Insert/Delete Equip Points

                if (s3dEquipPtDeletePos >= 0 || s3dEquipPtInsertPos >= 0 || s3dEquipPtMoveDownPos >= 0)
                {
                    GUI.FocusControl(null);
                    // Don't permit multiple operations in the same pass
                    if (s3dEquipPtMoveDownPos >= 0)
                    {
                        if (equipPointListProp.arraySize > 2)
                        {
                            // Move down one position, or wrap round to 1st position in list
                            if (s3dEquipPtMoveDownPos < equipPointListProp.arraySize - 1)
                            {
                                equipPointListProp.MoveArrayElement(s3dEquipPtMoveDownPos, s3dEquipPtMoveDownPos + 1);
                            }
                            else { equipPointListProp.MoveArrayElement(s3dEquipPtMoveDownPos, 0); }
                        }
                        s3dEquipPtMoveDownPos = -1;
                    }
                    else if (s3dEquipPtInsertPos >= 0)
                    {
                        // NOTE: Undo doesn't work with Insert.

                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        S3DEquipPoint insertedEquipPoint = new S3DEquipPoint(stickyControlModule.EquipPointList[s3dEquipPtInsertPos]);
                        insertedEquipPoint.showInEditor = true;
                        // Generate a new hashcode for the duplicated Equip Point
                        insertedEquipPoint.guidHash = S3DMath.GetHashCodeFromGuid();

                        stickyControlModule.EquipPointList.Insert(s3dEquipPtInsertPos, insertedEquipPoint);

                        // Read all properties from the Sticky Controller
                        serializedObject.Update();

                        // Hide original Equip Point
                        equipPointListProp.GetArrayElementAtIndex(s3dEquipPtInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                        equipPointShowInEditorProp = equipPointListProp.GetArrayElementAtIndex(s3dEquipPtInsertPos).FindPropertyRelative("showInEditor");

                        // Force new action to be serialized in scene
                        equipPointShowInEditorProp.boolValue = !equipPointShowInEditorProp.boolValue;

                        // Show inserted duplicate Equip Point
                        equipPointShowInEditorProp.boolValue = true;

                        s3dEquipPtInsertPos = -1;

                        isSceneModified = true;
                    }
                    else if (s3dEquipPtDeletePos >= 0)
                    {
                        // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and DeletePos is reset to -1.
                        int _deleteIndex = s3dEquipPtDeletePos;

                        if (EditorUtility.DisplayDialog("Delete Equip Point " + (s3dEquipPtDeletePos + 1) + "?", "Equip Point " + (s3dEquipPtDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the Equip Point from the list and cannot be undone.", "Delete Now", "Cancel"))
                        {
                            equipPointListProp.DeleteArrayElementAtIndex(_deleteIndex);
                            s3dEquipPtDeletePos = -1;
                        }
                    }
                }


                #endregion

            }
        }

        /// <summary>
        /// Dropdown menu callback method used when a bone is selected
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateEngageEquipPointTransform (object obj)
        {
            // The menu data is passed as Vector2Int. But it could be passed as say a Vector3Int
            // if more data is required.
            // Vector2Int.x is S3DHumanBone guidHash
            // Vector2Int.y is the zero-based Equip Point
            if (obj != null && obj.GetType() == typeof(Vector2Int))
            {
                Vector2Int objData = (Vector2Int)obj;

                // Find the selected bone (and transform)
                if (objData.y < stickyControlModule.NumberOfEquipPoints)
                {
                    if (objData.x == 0)
                    {
                        // If "None" is selected from the list, x is 0.
                        serializedObject.ApplyModifiedProperties();
                        Undo.RecordObject(stickyControlModule, "Set Equip Point Transform");
                        stickyControlModule.SetEquipPointTransform(objData.y, null);
                        serializedObject.Update();
                    }
                    else
                    {
                        for (int bnIdx = 0; bnIdx < rigHumanBoneList.Count; bnIdx++)
                        {
                            S3DHumanBone humanBone = rigHumanBoneList[bnIdx];
                            if (humanBone.guidHash == objData.x)
                            {
                                if (humanBone.isValid && humanBone.boneTransform.IsChildOf(stickyControlModule.transform))
                                {
                                    serializedObject.ApplyModifiedProperties();
                                    Undo.RecordObject(stickyControlModule, "Set Equip Point Transform");
                                    stickyControlModule.SetEquipPointTransform(objData.y, humanBone.boneTransform);
                                    serializedObject.Update();
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw events in the inspector
        /// </summary>
        private void DrawEngageEventSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            StickyEditorHelper.DrawS3DFoldout(isEngageEventsExpandedProp, isEngageEventsExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (isEngageEventsExpandedProp.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(onInitialisedEvtDelayProp, onInitialisedEvtDelayContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyControlModule.SetOnInitialisedEvtDelay(onInitialisedEvtDelayProp.floatValue);
                }

                EditorGUILayout.PropertyField(onInitialisedProp, onInitialisedContent);
                EditorGUILayout.PropertyField(onDestroyedProp, onDestroyedContent);
                EditorGUILayout.PropertyField(onInteractLookAtChangedProp, onInteractLookAtChangedContent);

                EditorGUILayout.PropertyField(onPreStartAimProp, onPreStartAimContent);
                EditorGUILayout.PropertyField(onPostStartAimProp, onPostStartAimContent);
                EditorGUILayout.PropertyField(onPreStopAimProp, onPreStopAimContent);
                EditorGUILayout.PropertyField(onPostStopAimProp, onPostStopAimContent);

                EditorGUILayout.PropertyField(onPreStartHoldWeaponProp, onPreStartHoldWeaponContent);
                EditorGUILayout.PropertyField(onPostStopHoldWeaponProp, onPostStopHoldWeaponContent);

                EditorGUILayout.PropertyField(onRespawningProp, onRespawningContent);
                EditorGUILayout.PropertyField(onRespawnedProp, onRespawnedContent);
            }
        }

        /// <summary>
        /// Draw the interactive tags scriptable object in the inspector
        /// </summary>
        private void DrawEngageInteractiveTags()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(interactiveTagsProp, interactiveTagsContent);
            if (EditorGUI.EndChangeCheck())
            {
                // Force the names to be repopulated
                serializedObject.ApplyModifiedProperties();
                interactiveTagNames = null;
                serializedObject.Update();
            }

            if (interactiveTagNames == null)
            {
                if (interactiveTagsProp.objectReferenceValue != null)
                {
                    interactiveTagNames = ((S3DInteractiveTags)interactiveTagsProp.objectReferenceValue).GetTagNames(true);
                }
                else
                {
                    interactiveTagNames = new string[] { "Default" };
                }
            }
        }

        /// <summary>
        /// Draw the lasso settings in the inspector
        /// </summary>
        private void DrawEngageLassoSettings()
        {
            EditorGUILayout.PropertyField(lassoSpeedProp, lassoSpeedContent);
        }

        /// <summary>
        /// Draw the identification settings in the inspector
        /// </summary>
        private void DrawIdentificationSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            StickyEditorHelper.DrawS3DFoldout(isIdenficationExpandedProp, isIdentificationExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);
            if (isIdenficationExpandedProp.boolValue)
            {
                EditorGUILayout.PropertyField(factionIdProp, identFactionIdContent);
                EditorGUILayout.PropertyField(modelIdProp, identModelIdContent);
            }
        }

        /// <summary>
        /// Draw the look interactive settings in the inspector
        /// </summary>
        private void DrawLookInteractiveSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            GUILayout.BeginHorizontal();
            StickyEditorHelper.DrawS3DFoldout(isLookInteractiveExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
            EditorGUILayout.LabelField(isLookInteractiveEnabledContent, GUILayout.Width(155f));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isLookInteractiveEnabledProp, GUIContent.none);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                if (isLookInteractiveEnabledProp.boolValue)
                {
                    stickyControlModule.EnableLookInteractive();
                }
                else
                {
                    stickyControlModule.DisableLookInteractive();
                }
                    
            }
            GUILayout.EndHorizontal();

            if (isLookInteractiveEnabledProp.boolValue && isLookInteractiveExpandedProp.boolValue)
            {
                EditorGUILayout.PropertyField(lookInteractiveLayerMaskProp, lookInteractiveLayerMaskContent);

                // When Look VR is enabled, Max Distance is set on the StickyXRInteractor component of each hand
                if (!isLookVRProp.boolValue)
                {
                    EditorGUILayout.PropertyField(lookMaxInteractiveDistanceProp, lookMaxInteractiveDistanceContent);
                    EditorGUILayout.PropertyField(lookInteractiveLockToCameraProp, lookInteractiveLockToCameraContent);
                    EditorGUILayout.PropertyField(isUpdateLookingAtPointProp, isUpdateLookingAtPointContent);
                }
            }
        }

        /// <summary>
        /// Draw the look sockets settings in the inspector
        /// </summary>
        private void DrawLookSocketsSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            GUILayout.BeginHorizontal();
            StickyEditorHelper.DrawS3DFoldout(isLookSocketsExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
            EditorGUILayout.LabelField(isLookSocketsEnabledContent, GUILayout.Width(155f));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isLookSocketsEnabledProp, GUIContent.none);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                if (isLookInteractiveEnabledProp.boolValue)
                {
                    stickyControlModule.EnableLookSockets();
                }
                else
                {
                    stickyControlModule.DisableLookSockets();
                }
                    
            }
            GUILayout.EndHorizontal();

            if (isLookSocketsEnabledProp.boolValue && isLookSocketsExpandedProp.boolValue)
            {
                EditorGUILayout.PropertyField(lookSocketLayerMaskProp, lookSocketLayerMaskContent);

                // When Look VR is enabled, [TODO] Max Distance is set on the StickyXRInteractor component of each hand
                if (!isLookVRProp.boolValue)
                {
                    EditorGUILayout.PropertyField(lookMaxSocketDistanceProp, lookMaxSocketDistanceContent);
                    EditorGUILayout.PropertyField(lookSocketLockToCameraProp, lookSocketLockToCameraContent);
                }

                EditorGUILayout.PropertyField(socketActiveMaterialProp, socketActiveMaterialContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(isLookSocketAutoShowProp, isLookSocketAutoShowContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyControlModule.SetLookSocketAutoShow(isLookSocketAutoShowProp.boolValue);
                }
            }
        }

        /// <summary>
        /// Draw a drop down list of animator parameters
        /// </summary>
        /// <param name="paramList"></param>
        /// <param name="paramNames"></param>
        /// <param name="hashCodeProperty"></param>
        /// <param name="labelContent"></param>
        /// <param name="isHandVR"></param>
        private void DrawParameterSelection(List<S3DAnimParm> paramList, string[] paramNames, SerializedProperty hashCodeProperty, GUIContent labelContent, bool isHandVR = false)
        {
            if (paramNames == null) { if (isHandVR) { RefreshVRAnimatorParameters(); } else { RefreshAnimatorParameters(); } }

            if (paramNames == null)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(labelContent, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(aaNoneContent, GUILayout.Width(defaultEditorLabelWidth - 8f));
                GUILayout.EndHorizontal();
            }
            else
            {
                int paramIdx = paramList.FindIndex(p => p.hashCode == hashCodeProperty.intValue);

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(labelContent, GUILayout.Width(defaultEditorLabelWidth - 1f));

                EditorGUI.BeginChangeCheck();
                paramIdx = EditorGUILayout.Popup(paramIdx, paramNames);
                if (EditorGUI.EndChangeCheck())
                {
                    // The parameter list and the name array should be in synch. See RefreshAnimatorParameters()
                    if (paramIdx < paramList.Count)
                    {
                        hashCodeProperty.intValue = paramList[paramIdx].hashCode;
                    }
                }

                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Draw the respawn properties in the inspector
        /// </summary>
        private void DrawRespawnSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            StickyEditorHelper.DrawS3DFoldout(isEngageRespawnExpandedProp, isEngageRespawnExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (isEngageRespawnExpandedProp.boolValue)
            {
                bool unlimtiedLives = startLivesProp.intValue < 0;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(unlimitedLivesContent, GUILayout.Width(defaultEditorLabelWidth - 3f));
                unlimtiedLives = EditorGUILayout.Toggle(unlimtiedLives);
                EditorGUILayout.EndHorizontal();

                if (unlimtiedLives && startLivesProp.intValue != -1) { startLivesProp.intValue = -1; }
                else if (!unlimtiedLives)
                {
                    #region Start Lives
                    if (startLivesProp.intValue < 0)
                    {
                        int newValue = 0;
                        startLivesProp.intValue = newValue;

                        // Verify it AFTER first setting it else it could remain at -1.
                        serializedObject.ApplyModifiedProperties();
                        stickyControlModule.SetStartLives(newValue);
                        serializedObject.Update();
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(startLivesProp, startLivesContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            serializedObject.ApplyModifiedProperties();
                            stickyControlModule.SetStartLives(startLivesProp.intValue);
                            serializedObject.Update();
                        }
                    }
                    #endregion

                    #region Max Lives
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(maxLivesProp, maxLivesContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        stickyControlModule.SetMaxLives(maxLivesProp.intValue);
                        serializedObject.Update();
                    }
                    #endregion
                }

                #region Respawn Mode

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(respawnModeProp, respawnModeContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    serializedObject.ApplyModifiedProperties();
                    stickyControlModule.SetRespawnMode((StickyControlModule.RespawningMode)respawnModeProp.intValue);
                    serializedObject.Update();
                }

                EditorGUILayout.PropertyField(respawnTimeProp, respawnTimeContent);

                if (respawnModeProp.intValue == (int)(StickyControlModule.RespawningMode.RespawnAtSpecifiedPosition))
                {
                    EditorGUILayout.PropertyField(customRespawnPositionProp, customRespawnPositionContent);
                    EditorGUILayout.PropertyField(customRespawnRotationProp, customRespawnRotationContent);
                }

                #endregion

                #region Drop on Destroy

                EditorGUILayout.PropertyField(isDropHeldOnDestroyProp, isDropHeldOnDestroyContent);
                EditorGUILayout.PropertyField(isDropEquipOnDestroyProp, isDropEquipOnDestroyContent);
                EditorGUILayout.PropertyField(isDropStashOnDestroyProp, isDropStashOnDestroyContent);

                #endregion

                EditorGUILayout.PropertyField(isRagdollOnDestroyProp, isRagdollOnDestroyContent);
            }
        }

        /// <summary>
        /// Draw the stash properties in the inspector
        /// </summary>
        private void DrawStashEngageSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            StickyEditorHelper.DrawS3DFoldout(isEngageStashExpandedProp, isEngageStashExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (isEngageStashExpandedProp.boolValue)
            {
                #region Stash Parent
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(stashParentContent, GUILayout.Width(defaultEditorLabelWidth - 53f));
                if (GUILayout.Button(StickyEditorHelper.btnNewContent, buttonCompact, GUILayout.MaxWidth(50f)) && stashParentProp.objectReferenceValue == null)
                {
                    serializedObject.ApplyModifiedProperties();

                    Undo.SetCurrentGroupName("Add Stash");
                    int undoGroup = UnityEditor.Undo.GetCurrentGroup();
                    Undo.RecordObject(stickyControlModule, string.Empty);

                    GameObject stashGameObject = new GameObject("Stash");
                    Undo.RegisterCreatedObjectUndo(stashGameObject, string.Empty);
                    stashGameObject.transform.SetParent(stickyControlModule.transform, false);
                    stickyControlModule.SetStashParent(stashGameObject.transform);
                    Undo.CollapseUndoOperations(undoGroup);

                    // Should be non-scene objects but is required to force being set as dirty
                    EditorUtility.SetDirty(stickyControlModule);

                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.PropertyField(stashParentProp, GUIContent.none);
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    stickyControlModule.StashParent = stashParentProp.objectReferenceValue as Transform;
                    serializedObject.Update();
                }

                #endregion

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(minStashDropDistanceProp, minStashDropDistanceContent);
                if (EditorGUI.EndChangeCheck())
                {
                    if (minStashDropDistanceProp.floatValue > maxStashDropDistanceProp.floatValue)
                    {
                        maxStashDropDistanceProp.floatValue = minStashDropDistanceProp.floatValue;
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(maxStashDropDistanceProp, maxStashDropDistanceContent);
                if (EditorGUI.EndChangeCheck())
                {
                    if (maxStashDropDistanceProp.floatValue < minStashDropDistanceProp.floatValue)
                    {
                        minStashDropDistanceProp.floatValue = maxStashDropDistanceProp.floatValue;
                    }
                }

            }
        }

        /// <summary>
        /// Draw a toggle button and update the variable when button is pressed
        /// </summary>
        /// <param name="toggleVariable"></param>
        private void DrawToggleButton(ref bool toggleVariable)
        {
            if (GUILayout.Button(StickyEditorHelper.gizmoToggleBtnContent, GUILayout.MaxWidth(22f)))
            {
                serializedObject.ApplyModifiedProperties();
                toggleVariable = !toggleVariable;
                SceneView.RepaintAll();
                // Read in any changes made in here
                serializedObject.Update();
            }
        }

        private void DrawTransitionSelection()
        {
            if (animTransNames == null) { RefreshAnimatorTransitions(); }

            if (animTransNames == null)
            {
                GUILayout.BeginHorizontal();
#if UNITY_2019_1_OR_NEWER
                EditorGUILayout.LabelField(aaTransNamesContent, GUILayout.Width(defaultEditorLabelWidth - 3f));
#else
                EditorGUILayout.LabelField(aaTransNamesContent, GUILayout.Width(defaultEditorLabelWidth - 8f));
#endif
                EditorGUILayout.LabelField(aaNoneContent, GUILayout.Width(defaultEditorLabelWidth - 8f));
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                int transIdx = 0;
                EditorGUILayout.LabelField(aaTransNamesContent, GUILayout.Width(defaultEditorLabelWidth - 3f));
                transIdx = EditorGUILayout.Popup(transIdx, animTransNames);
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Draw a thruster effects property in the editor
        /// </summary>
        /// <param name="thrusterProp"></param>
        /// <param name="thrusterGUIContent"></param>
        private void DrawJetPackThrusterEffect(SerializedProperty thrusterProp, GUIContent thrusterGUIContent)
        {
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
#if UNITY_2019_1_OR_NEWER
            EditorGUILayout.LabelField(thrusterGUIContent, GUILayout.Width(defaultEditorLabelWidth - 53f));
#else
            EditorGUILayout.LabelField(thrusterGUIContent, GUILayout.Width(defaultEditorLabelWidth - 58f));
#endif
            if (GUILayout.Button(jetPackAlignBtnContent, buttonCompact, GUILayout.Width(50f)) && thrusterProp.objectReferenceValue != null)
            {
                // Get the centre of the prefab
                Transform characterTfm = stickyControlModule.transform;

                Vector3 characterCentre = characterTfm.position + (characterTfm.up * stickyControlModule.pivotToCentreOffsetY);
                Transform effectTfm = ((GameObject)thrusterProp.objectReferenceValue).transform;

                Vector3 newPosition = Vector3.zero;
                Quaternion newRotation = Quaternion.identity;

                bool updateTransform = true;

                // Get offset slightly behind character
                Vector3 backOffset = -characterTfm.forward * (stickyControlModule.radius + 0.01f);

                switch (thrusterProp.name)
                {
                    case "jetPackThrusterFwd":
                        // faces backward, pushes forward
                        newRotation = Quaternion.LookRotation(-characterTfm.forward);
                        // Default position is slightly behind centre back
                        newPosition = characterCentre + backOffset;
                        break;
                    case "jetPackThrusterBack":
                        // faces forward, pushes backward
                        newRotation = Quaternion.LookRotation(characterTfm.forward);
                        // Default position is slightly infront of centre front
                        newPosition = characterCentre + (characterTfm.forward * (stickyControlModule.radius + 0.01f));
                        break;
                    case "jetPackThrusterUp":
                        // faces down, pushes up
                        newRotation = Quaternion.LookRotation(-characterTfm.up);
                        // Default position 0.2 below centre
                        newPosition = characterCentre + (characterTfm.up * -0.2f) + backOffset;
                        break;
                    case "jetPackThrusterDown":
                        // faces up, pushes down
                        newRotation = Quaternion.LookRotation(characterTfm.up);
                        // Default position 0.2 above centre
                        newPosition = characterCentre + (characterTfm.up * 0.2f) + backOffset;
                        break;
                    case "jetPackThrusterRight":
                        // faces left - pushes to the right
                        newRotation = Quaternion.LookRotation(Vector3.Cross(characterTfm.up, -characterTfm.forward));
                        // Default position half radius to the left of centre
                        newPosition = characterCentre + (Vector3.Cross(characterTfm.up, -characterTfm.forward) * stickyControlModule.radius * 0.5f) + backOffset;
                        break;
                    case "jetPackThrusterLeft":
                        // faces right - pushes to the left
                        newRotation = Quaternion.LookRotation(Vector3.Cross(characterTfm.up, characterTfm.forward));
                        // Default position half radius to the right of centre
                        newPosition = characterCentre + (Vector3.Cross(characterTfm.up, characterTfm.forward) * stickyControlModule.radius * 0.5f) + backOffset;
                        break;
                    default:
                        updateTransform = false;
                        break;
                }

                if (updateTransform)
                {
                    Undo.RecordObject(effectTfm, "JetPack Effect Position");
                    effectTfm.SetPositionAndRotation(newPosition, newRotation);
                }
            }
            EditorGUILayout.PropertyField(thrusterProp, GUIContent.none);
            if (EditorGUI.EndChangeCheck() && thrusterProp.objectReferenceValue != null)
            {
                if (!((GameObject)thrusterProp.objectReferenceValue).transform.IsChildOf(stickyControlModule.transform))
                {
                    thrusterProp.objectReferenceValue = null;
                    Debug.LogWarning("The jet pack thruster effects gameobject must be a child of the parent Sticky3D Controller gameobject or part of the prefab.");
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a ragdoll bone in the inspector
        /// </summary>
        /// <param name="s3dBoneProp"></param>
        /// <param name="boneIndex"></param>
        /// <param name="numBones"></param>
        /// <param name="movePos"></param>
        /// <param name="insertPos"></param>
        /// <param name="deletePos"></param>
        private void DrawRagdollBone(SerializedProperty s3dBoneProp, int boneIndex, int numBones, ref int movePos, ref int insertPos, ref int deletePos)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            rdBoneProp = s3dBoneProp.FindPropertyRelative("bone");

            HumanBodyBones humanBone = (HumanBodyBones)rdBoneProp.intValue;

            rdBoneTransformProp = s3dBoneProp.FindPropertyRelative("boneTransform");

            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(humanBone.ToString(), GUILayout.Width(100f));
            EditorGUILayout.PropertyField(rdBoneTransformProp, GUIContent.none);

            // Move down button
            if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numBones > 1) { movePos = boneIndex; }
            // Create duplicate button
            if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { insertPos = boneIndex; }
            // Delete button
            if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { deletePos = boneIndex; }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
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

                if (compType == typeof(S3DAnimAction))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as S3DAnimAction).showInEditor = isExpanded;
                    }
                }
                else if (compType == typeof(S3DFootstep))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as S3DFootstep).showInEditor = isExpanded;
                    }
                }
                else if (compType == typeof(S3DDamageRegion))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as S3DDamageRegion).showInEditor = isExpanded;
                    }
                }
                else if (compType == typeof(S3DEquipPoint))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as S3DEquipPoint).showInEditor = isExpanded;
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to get and populate the list of humaniod bones used for ragdoll
        /// </summary>
        private void GetRagdollBones()
        {
            if (stickyControlModule.IsValidHumanoid(false))
            {
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(stickyControlModule, "Get Ragdoll Bones");
                stickyControlModule.ConfigureRagdollBones();
                serializedObject.Update();

                // Force persistance of changes to the scene
                s3dRagdollBoneProp = ragdollBoneListProp.GetArrayElementAtIndex(0);
                rdBoneGUIDHashProp = s3dRagdollBoneProp.FindPropertyRelative("guidHash");
                int hashValue = rdBoneGUIDHashProp.intValue;
                rdBoneGUIDHashProp.intValue = 0;
                rdBoneGUIDHashProp.intValue = hashValue;
            }
            else
            {
                Debug.LogWarning("ERROR StickyControlModuleEditor cannot get ragdoll bones - this character does not have a humanoid rig or there is no animator");
            }
        }

        /// <summary>
        /// Is this a Custom "standard" action? If so, it will get it's value from code like a Custom Input
        /// in StickyInputModule.
        /// </summary>
        /// <param name="standardActionProp"></param>
        /// <returns></returns>
        private bool IsCustomAction(SerializedProperty standardActionProp)
        {
            return standardActionProp.intValue == (int)S3DAnimAction.StandardAction.Custom;
        }

        /// <summary>
        /// Check if this is a humanoid and refresh key bone transforms.
        /// (Currently only feet and head).
        /// </summary>
        private void RefreshBones()
        {
            if (stickyControlModule != null && stickyControlModule.defaultAnimator != null && stickyControlModule.defaultAnimator.runtimeAnimatorController != null && stickyControlModule.IsValidHumanoid(false))
            {
                isHumanoid = true;

                if (leftFootTrfm == null) { leftFootTrfm = stickyControlModule.defaultAnimator.GetBoneTransform(HumanBodyBones.LeftFoot); }
                if (rightFootTrfm == null) { rightFootTrfm = stickyControlModule.defaultAnimator.GetBoneTransform(HumanBodyBones.RightFoot); }
                if (headTrfm == null) { headTrfm = stickyControlModule.defaultAnimator.GetBoneTransform(HumanBodyBones.Head); }
            }
            else
            {
                isHumanoid = false;
                leftFootTrfm = null;
                rightFootTrfm = null;
                headTrfm = null;
            }
        }

        /// <summary>
        /// This lets us modify and display things in the scene view
        /// See also OnDrawGizmosSelected in StickyControlModule.cs
        /// </summary>
        /// <param name="sv"></param>
        private void SceneGUI(SceneView sv)
        {
            if (stickyControlModule != null && stickyControlModule.gameObject.activeInHierarchy)
            {
                isSceneDirtyRequired = false;

                // IMPORTANT: Do not use transform.TransformPoint or InverseTransformPoint because they won't work correctly
                // when the parent gameobject has scale not equal to 1,1,1.

                // Get the rotation of the character in the scene
                sceneViewStickyRot = Quaternion.LookRotation(stickyControlModule.transform.forward, stickyControlModule.transform.up);

                #region When Debugging is enabled
                if (isDebuggingEnabled && (isShowVolume || isShowGroundedIndicator || isShowHeadIKDirection) && EditorApplication.isPlaying)
                {
                    // Use local space rather than world space for the volume wire cube.
                    // This allows the handle to rotate with the character.
                    using (new Handles.DrawingScope(Color.yellow, stickyControlModule.transform.localToWorldMatrix))
                    {
                        // Show the volume occupied by the character in play mode.
                        // Currently only an approximation using a rectangular prism to represent the capsule.
                        if (isShowVolume)
                        {
                            Vector3 _scaledSize = stickyControlModule.ScaledSize;

                            Handles.DrawWireCube(new Vector3(0f, _scaledSize.y / stickyControlModule.height * pivotToCentreOffsetYProp.floatValue, 0f), stickyControlModule.ScaledSize);
                        }

                        // Show a line indicating the direction the head ik is looking
                        if (isShowHeadIKDirection && stickyControlModule.IsHeadIKEnabled && headTrfm != null)
                        {
                            // The headIKTargetPos is where the character wants to look. The Previous is where it IS looking.
                            Vector3 _headIKPreviousPos = stickyControlModule.GetHeadIKPreviousPosition();

                            Handles.DrawLine(stickyControlModule.GetLocalPosition(headTrfm.position), stickyControlModule.GetLocalPosition(_headIKPreviousPos));
                        }

                        if (isShowGroundedIndicator && stickyControlModule.IsGrounded)
                        {
                            using (new Handles.DrawingScope(groundedIndicatorColour))
                            {
                                Handles.DrawSolidDisc(stickyControlModule.GetLocalPosition(stickyControlModule.GetCurrentBottom()), stickyControlModule.transform.up, stickyControlModule.ScaledSize.x / 2f);
                            }
                        }
                    }
                }
                #endregion

                #region Look

                // Show where the Camera Offset would be 
                if (stickyControlModule.ShowLookCamOffsetGizmosInSceneView && stickyControlModule.isThirdPerson)
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        using (new Handles.DrawingScope(Color.green))
                        {
                            componentHandlePosition = stickyControlModule.transform.position + (stickyControlModule.transform.rotation * stickyControlModule.lookCameraOffset);
                            Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), componentHandlePosition, Quaternion.identity, 0.05f, EventType.Repaint);
                        }
                    }
                }
                
                // Show where the Focus Offset would be 
                if (stickyControlModule.ShowLookFocusOffsetGizmosInSceneView && stickyControlModule.isThirdPerson)
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        using (new Handles.DrawingScope(Color.blue))
                        {
                            componentHandlePosition = stickyControlModule.transform.position + (stickyControlModule.transform.rotation * stickyControlModule.lookFocusOffset);
                            Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), componentHandlePosition, Quaternion.identity, 0.05f, EventType.Repaint);
                        }
                    }
                }

                #endregion

                #region Hands
                if (stickyControlModule.IsEditorMode && stickyControlModule.IsHandIKEnabled)
                {
                    // Get the hand transform if we don't have them yet
                    if (leftHandTrfm == null) { leftHandTrfm = stickyControlModule.GetLeftHandTransform; }
                    if (rightHandTrfm == null) { rightHandTrfm = stickyControlModule.GetRightHandTransform; }

                    #region Left Hand
                    if (leftHandTrfm != null && stickyControlModule.ShowLHGizmosInSceneView)
                    {
                        // Get component handle position and rotation
                        componentHandlePosition = stickyControlModule.GetLeftHandPalmPosition();                        
                        componentHandleRotation = stickyControlModule.GetLeftHandPalmRotation();

                        // Use a fixed size rather than one that changes with scene view camera distance
                        relativeHandleSize = 0.1f;
                        //relativeHandleSize = HandleUtility.GetHandleSize(componentHandlePosition);
                        fadedGizmoColour = stickyControlModule.HandZoneGizmoColour;

                        // If this hand is not selected, show it a little more transparent
                        if (!isLeftHandSelected)
                        {
                            fadedGizmoColour.a *= 0.65f;
                            if (fadedGizmoColour.a < 0.1f) { fadedGizmoColour.a = stickyControlModule.HandZoneGizmoColour.a; }
                        }

                        // Draw point in the scene that is non-interactable
                        if (Event.current.type == EventType.Repaint)
                        {
                            using (new Handles.DrawingScope(fadedGizmoColour))
                            {
                                // Forwards direction of the palm of the hand
                                // NOTE: For the left hand we flip the direction of the gizmo to make it look correct in the editor.
                                //Handles.ArrowHandleCap(GUIUtility.GetControlID(FocusType.Passive), componentHandlePosition, componentHandleRotation * Quaternion.Euler(0f, 180f, 0f), relativeHandleSize, EventType.Repaint);
                                
                                // DON'T FLIP LH for now - not consistant results for multiple rigs...
                                Handles.ArrowHandleCap(GUIUtility.GetControlID(FocusType.Passive), componentHandlePosition, componentHandleRotation, relativeHandleSize, EventType.Repaint);

                                // Draw the up direction with a sphere on top of a line
                                Quaternion upRotation = componentHandleRotation * Quaternion.Euler(270f, 0f, 0f);
                                Handles.DrawLine(componentHandlePosition, componentHandlePosition + upRotation * (Vector3.forward * relativeHandleSize));
                                Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), componentHandlePosition + (upRotation * (Vector3.forward * relativeHandleSize)), upRotation, 0.3f * relativeHandleSize, EventType.Repaint);
                            }
                        }

                        using (new Handles.DrawingScope(stickyControlModule.HandZoneGizmoColour))
                        {
                            if (isLeftHandSelected)
                            {
                                // Choose which handle to draw based on which Unity tool is selected
                                if (Tools.current == Tool.Rotate)
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a rotation handle
                                    componentHandleRotation = Handles.RotationHandle(componentHandleRotation, componentHandlePosition);

                                    // Use the rotation handle to edit the palm normal
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(stickyControlModule, "Rotate Left Palm Point");

                                        stickyControlModule.SetLeftHandPalmRotation((Quaternion.Inverse(leftHandTrfm.rotation) * componentHandleRotation).eulerAngles);
                                    }
                                }

                                if (Tools.current == Tool.Move || Tools.current == Tool.Transform)
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a movement handle
                                    componentHandlePosition = Handles.PositionHandle(componentHandlePosition, componentHandleRotation);

                                    // Use the position handle to edit the position of the local palm point
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(stickyControlModule, "Move Left Hand Centre");
                                        stickyControlModule.SetLeftHandPalmOffset(S3DUtils.GetLocalSpacePosition(leftHandTrfm, componentHandlePosition));
                                    }
                                }
                            }
                        }

                        using (new Handles.DrawingScope(fadedGizmoColour))
                        {
                            SceneViewSelectButton(componentHandlePosition, 0.04f, ref isLeftHandSelected);
                        }
                    }
                    #endregion

                    #region Right Hand
                    if (rightHandTrfm != null && stickyControlModule.ShowRHGizmosInSceneView)
                    {
                        // Get component handle position and rotation
                        componentHandlePosition = stickyControlModule.GetRightHandPalmPosition();
                        componentHandleRotation = stickyControlModule.GetRightHandPalmRotation();

                        // Use a fixed size rather than one that changes with scene view camera distance
                        relativeHandleSize = 0.1f;
                        //relativeHandleSize = HandleUtility.GetHandleSize(componentHandlePosition);
                        fadedGizmoColour = stickyControlModule.HandZoneGizmoColour;

                        // If this hand is not selected, show it a little more transparent
                        if (!isRightHandSelected)
                        {
                            fadedGizmoColour.a *= 0.65f;
                            if (fadedGizmoColour.a < 0.1f) { fadedGizmoColour.a = stickyControlModule.HandZoneGizmoColour.a; }
                        }

                        // Draw point in the scene that is non-interactable
                        if (Event.current.type == EventType.Repaint)
                        {
                            using (new Handles.DrawingScope(fadedGizmoColour))
                            {
                                // Forwards direction of the palm of the hand
                                Handles.ArrowHandleCap(GUIUtility.GetControlID(FocusType.Passive), componentHandlePosition, componentHandleRotation, relativeHandleSize, EventType.Repaint);

                                // Draw the up direction with a sphere on top of a line
                                Quaternion upRotation = componentHandleRotation * Quaternion.Euler(270f, 0f, 0f);
                                Handles.DrawLine(componentHandlePosition, componentHandlePosition + upRotation * (Vector3.forward * relativeHandleSize));
                                Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), componentHandlePosition + (upRotation * (Vector3.forward * relativeHandleSize)), upRotation, 0.3f * relativeHandleSize, EventType.Repaint);
                            }
                        }

                        using (new Handles.DrawingScope(stickyControlModule.HandZoneGizmoColour))
                        {
                            if (isRightHandSelected)
                            {
                                // Choose which handle to draw based on which Unity tool is selected
                                if (Tools.current == Tool.Rotate)
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a rotation handle
                                    componentHandleRotation = Handles.RotationHandle(componentHandleRotation, componentHandlePosition);

                                    // Use the rotation handle to edit the palm normal
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(stickyControlModule, "Rotate Right Palm Point");

                                        stickyControlModule.SetRightHandPalmRotation((Quaternion.Inverse(rightHandTrfm.rotation) * componentHandleRotation).eulerAngles);
                                    }
                                }

                                if (Tools.current == Tool.Move || Tools.current == Tool.Transform)
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a movement handle
                                    componentHandlePosition = Handles.PositionHandle(componentHandlePosition, componentHandleRotation);

                                    // Use the position handle to edit the position of the local palm point
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(stickyControlModule, "Move Right Hand Centre");
                                        stickyControlModule.SetRightHandPalmOffset(S3DUtils.GetLocalSpacePosition(rightHandTrfm, componentHandlePosition));
                                    }
                                }
                            }
                        }

                        using (new Handles.DrawingScope(fadedGizmoColour))
                        {
                            SceneViewSelectButton(componentHandlePosition, 0.04f, ref isRightHandSelected);
                        }
                    }
                    #endregion
                }
                #endregion

                #region Engage Equip Points

                using (new Handles.DrawingScope(stickyControlModule.equipPointGizmoColour))
                {
                    for (int eqIdx = 0; eqIdx < stickyControlModule.NumberOfEquipPoints; eqIdx++)
                    {
                        S3DEquipPoint equipPoint = stickyControlModule.EquipPointList[eqIdx];
                        if (equipPoint != null && equipPoint.parentTransform != null)
                        {
                            fadedGizmoColour = stickyControlModule.equipPointGizmoColour;
                            Vector3 localScale = equipPoint.parentTransform.localScale;

                            // If this is not the selected Equip Point, show it a little more transparent
                            if (!equipPoint.selectedInSceneView)
                            {
                                fadedGizmoColour.a *= 0.65f;
                                if (fadedGizmoColour.a < 0.1f) { fadedGizmoColour.a = stickyControlModule.equipPointGizmoColour.a; }
                            }

                            if (equipPoint.showGizmosInSceneView)
                            {
                                // Get component handle position
                                componentHandlePosition = stickyControlModule.GetEquipPointPosition(equipPoint);
                                //componentHandlePosition = equipPoint.parentTransform.TransformPoint(new Vector3(equipPoint.relativeOffset.x / localScale.x, equipPoint.relativeOffset.y / localScale.y, equipPoint.relativeOffset.z / localScale.z));

                                //relativeHandleSize = HandleUtility.GetHandleSize(componentHandlePosition);

                                // Get component handle rotation
                                componentHandleRotation = stickyControlModule.GetEquipPointRotation(equipPoint);

                                // Draw point in the scene that is non-interactable
                                if (Event.current.type == EventType.Repaint)
                                {
                                    using (new Handles.DrawingScope(fadedGizmoColour))
                                    {
                                        // Forwards direction of the equip point
                                        Handles.ArrowHandleCap(GUIUtility.GetControlID(FocusType.Passive), componentHandlePosition, componentHandleRotation, 0.2f, EventType.Repaint);

                                        // Draw the up direction with a sphere on top of a line
                                        Quaternion upRotation = componentHandleRotation * Quaternion.Euler(270f, 0f, 0f);
                                        Handles.DrawLine(componentHandlePosition, componentHandlePosition + upRotation * (Vector3.forward * 0.2f));
                                        Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), componentHandlePosition + (upRotation * (Vector3.forward * 0.2f)), upRotation, 0.05f, EventType.Repaint);
                                    }
                                }

                                if (equipPoint.selectedInSceneView)
                                {
                                    // Choose which handle to draw based on which Unity tool is selected
                                    if (Tools.current == Tool.Rotate)
                                    {
                                        EditorGUI.BeginChangeCheck();

                                        // Draw a rotation handle
                                        componentHandleRotation = Handles.RotationHandle(componentHandleRotation, componentHandlePosition);

                                        // Use the rotation handle to edit the docking point direction
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            isSceneDirtyRequired = true;
                                            Undo.RecordObject(stickyControlModule, "Rotate Equip Point");

                                            equipPoint.relativeRotation = (Quaternion.Inverse(equipPoint.parentTransform.rotation) * componentHandleRotation).eulerAngles;   
                                        }
                                    }
                                    else if (Tools.current == Tool.Move || Tools.current == Tool.Transform)
                                    {
                                        EditorGUI.BeginChangeCheck();

                                        // Draw a movement handle
                                        componentHandlePosition = Handles.PositionHandle(componentHandlePosition, equipPoint.parentTransform.rotation);

                                        // Use the position handle to edit the position of the weapon
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            isSceneDirtyRequired = true;
                                            Undo.RecordObject(stickyControlModule, "Move Equip Point");
                                            // Don't use InverseTransformPoint as it has issues with scaled transforms
                                            equipPoint.relativeOffset = S3DUtils.GetLocalSpacePosition(equipPoint.parentTransform, componentHandlePosition);
                                        }
                                    }
                                }

                                // If the equip point is not selected it will be faded, else it will be the original colour.
                                using (new Handles.DrawingScope(fadedGizmoColour))
                                {
                                    // Allow the user to select/deselect the equip point in the scene view
                                    if (Handles.Button(componentHandlePosition, Quaternion.identity, 0.05f, 0.01f, Handles.SphereHandleCap))
                                    {
                                        if (equipPoint.selectedInSceneView)
                                        {
                                            DeselectAllComponents();
                                            equipPoint.showInEditor = false;
                                        }
                                        else
                                        {
                                            DeselectAllComponents();
                                            stickyControlModule.IsEquipListExpanded = false;
                                            ExpandList(stickyControlModule.EquipPointList, false);
                                            equipPoint.selectedInSceneView = true;
                                            equipPoint.showInEditor = true;
                                            isSceneDirtyRequired = true;
                                            // Hide Unity tools
                                            Tools.hidden = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

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
        /// Draw a selectable button in the scene view
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="isSelected"></param>
        private void SceneViewSelectButton(Vector3 pos, float buttonRadius, ref bool isSelected)
        {
            // Allow the user to select/deselect the hand location in the scene view
            if (Handles.Button(pos, Quaternion.identity, buttonRadius, buttonRadius * 0.5f, Handles.SphereHandleCap))
            {
                if (isSelected)
                {
                    DeselectAllComponents();
                }
                else
                {
                    DeselectAllComponents();
                    isSelected = true;
                    // Hide Unity tools
                    Tools.hidden = true;
                }
            }
        }

        /// <summary>
        /// Fetch the current paramaters from the character animator
        /// </summary>
        private void RefreshAnimatorParameters()
        {
            if (animParamsBoolList == null) { animParamsBoolList = new List<S3DAnimParm>(10); }
            else { animParamsBoolList.Clear(); }
            if (animParamsTriggerList == null) { animParamsTriggerList = new List<S3DAnimParm>(10); }
            else { animParamsTriggerList.Clear(); }
            if (animParamsFloatList == null) { animParamsFloatList = new List<S3DAnimParm>(10); }
            else { animParamsFloatList.Clear(); }
            if (animParamsIntegerList == null) { animParamsIntegerList = new List<S3DAnimParm>(10); }
            else { animParamsIntegerList.Clear(); }

            if (stickyControlModule.defaultAnimator != null)
            {
                // Synch the list of parameters from the animator controller with the array of names used in the editor

                // Populate the lists from the animator
                S3DAnimParm.GetParameterList(stickyControlModule.defaultAnimator, animParamsBoolList, S3DAnimAction.ParameterType.Bool);
                S3DAnimParm.GetParameterList(stickyControlModule.defaultAnimator, animParamsTriggerList, S3DAnimAction.ParameterType.Trigger);
                S3DAnimParm.GetParameterList(stickyControlModule.defaultAnimator, animParamsFloatList, S3DAnimAction.ParameterType.Float);
                S3DAnimParm.GetParameterList(stickyControlModule.defaultAnimator, animParamsIntegerList, S3DAnimAction.ParameterType.Integer);

                // Get arrays of names for use in the popup controls
                animParamBoolNames = S3DAnimParm.GetParameterNames(animParamsBoolList);
                animParamTriggerNames = S3DAnimParm.GetParameterNames(animParamsTriggerList);
                animParamFloatNames = S3DAnimParm.GetParameterNames(animParamsFloatList);
                animParamIntegerNames = S3DAnimParm.GetParameterNames(animParamsIntegerList);
            }
            else
            {
                animParamBoolNames = null;
                animParamTriggerNames = null;
                animParamFloatNames = null;
                animParamIntegerNames = null;
            }
        }

        /// <summary>
        /// Fetch the current paramaters from the left and right hand VR animators
        /// </summary>
        private void RefreshVRAnimatorParameters()
        {
            if (animParamsFloatLHVRList == null) { animParamsFloatLHVRList = new List<S3DAnimParm>(10); }
            else { animParamsFloatLHVRList.Clear(); }
            if (animParamsFloatRHVRList == null) { animParamsFloatRHVRList = new List<S3DAnimParm>(10); }
            else { animParamsFloatRHVRList.Clear(); }

            if (stickyControlModule.leftHandAnimator != null)
            {
                // Synch the list of parameters from the animator controller with the array of names used in the editor

                // Populate the lists from the animator
                S3DAnimParm.GetParameterList(stickyControlModule.leftHandAnimator, animParamsFloatLHVRList, S3DAnimAction.ParameterType.Float);

                // Get arrays of names for use in the popup controls
                animParamFloatLHVRNames = S3DAnimParm.GetParameterNames(animParamsFloatLHVRList);
            }
            else
            {
                animParamFloatLHVRNames = null;
            }

            if (stickyControlModule.rightHandAnimator != null)
            {
                // Synch the list of parameters from the animator controller with the array of names used in the editor

                // Populate the lists from the animator
                S3DAnimParm.GetParameterList(stickyControlModule.rightHandAnimator, animParamsFloatRHVRList, S3DAnimAction.ParameterType.Float);

                // Get arrays of names for use in the popup controls
                animParamFloatRHVRNames = S3DAnimParm.GetParameterNames(animParamsFloatRHVRList);
            }
            else
            {
                animParamFloatRHVRNames = null;
            }
        }

        /// <summary>
        /// Fetch the current transitions from the character animator
        /// </summary>
        private void RefreshAnimatorTransitions()
        {
            if (stickyControlModule.defaultAnimator != null)
            {
                if (stickyControlModule.defaultAnimator != null && stickyControlModule.defaultAnimator.isActiveAndEnabled)
                {

                    if (animLayerList == null) { animLayerList = new List<S3DAnimLayer>(10); }
                    else { animLayerList.Clear(); }
                    if (animTransList == null) { animTransList = new List<S3DAnimTrans>(10); }
                    else { animTransList.Clear(); }

                    //stickyControlModule.defaultAnimator.runtimeAnimatorController.animationClips[0].

                    AnimatorController animController = (AnimatorController)stickyControlModule.defaultAnimator.runtimeAnimatorController;

                    // Loop through all the layers in the Animator
                    for(int lyIdx = 0; lyIdx < animController.layers.Length; lyIdx++)
                    {
                        var _layer = animController.layers[lyIdx];

                        animLayerList.Add(new S3DAnimLayer(lyIdx, _layer.name));

                        //Debug.Log("[DEBUG] Anim Layer: " + _layer.name);

                        // Loop through each state
                        foreach (var _state in _layer.stateMachine.states)
                        {
                            for(int trIdx = 0; trIdx <  _state.state.transitions.Length; trIdx++)
                            {
                                var _transition = _state.state.transitions[trIdx];

                                // If there is a user-defined name use that.
                                // Otherwise describe the source and destination of the transition.
                                string _name = _transition.name;
                                if (string.IsNullOrEmpty(_name))
                                {
                                    _name += _state.state.name + "->";

                                    if (_transition.destinationState != null)
                                    {
                                        _name += _transition.destinationState.name;
                                    }
                                    else if (_transition.destinationStateMachine != null)
                                    {
                                        _name += _transition.destinationStateMachine.name;
                                    }
                                    else if (_transition.isExit) { _name += "Exit"; }
                                    else { _name += "unknown"; }
                                }

                                animTransList.Add(new S3DAnimTrans(lyIdx, _transition.GetHashCode(), _name));

                                //Debug.Log("[DEBUG] Anim State: " + _name);
                            }
                        }
                    }

                    animTransNames = S3DAnimTrans.GetTransitionNames(animTransList);
                }
                else
                {
                    animTransNames = null;
                }
            }
            else
            {
                animTransNames = null;
            }
        }

        /// <summary>
        /// Refresh the array of SurfaceTypes from the Scriptable Object (if there is one assigned)
        /// </summary>
        private void RefreshSurfaceTypes()
        {
            surfaceTypeArray = S3DSurfaces.GetNameArray((S3DSurfaces)s3dSurfacesProp.objectReferenceValue, true);
        }

        /// <summary>
        /// If there isn't a Ragdoll collider for this same damage region bone, remove the damage region collider.
        /// </summary>
        /// <param name="humanBodyBone"></param>
        /// <param name="isRemoveColliderOnly">Only attempt to remove the collider. Don't remove the region itself</param>
        private void RemoveDamageRegion(HumanBodyBones humanBodyBone, bool isRemoveColliderOnly)
        {
            S3DDamageRegion damageRegion = stickyControlModule.GetDamageRegion(humanBodyBone);

            if (damageRegion != null)
            {
                S3DHumanBonePersist humanBonePersist = damageRegion.s3dHumanBonePersist;
                if (humanBonePersist != null && humanBonePersist.boneTransform != null)
                {
                    Collider boneCollider = humanBonePersist.boneCollider;

                    if (boneCollider != null)
                    {
                        bool isRemoveCollider = true;

                        // Check for ragdoll collider
                        List<S3DHumanBonePersist> bones = stickyControlModule.RagdollBoneList;

                        int numRagdollBones = bones == null ? 0 : bones.Count;

                        if (numRagdollBones > 0)
                        {
                            S3DHumanBonePersist ragdollBonePersist = stickyControlModule.GetRagdollBone(humanBodyBone);

                            // If there is a ragdoll collider for this bone, don't remove the collider
                            isRemoveCollider = !(ragdollBonePersist != null && ragdollBonePersist.boneCollider != null);
                        }

                        if (isRemoveCollider)
                        {
                            Undo.DestroyObjectImmediate(boneCollider);
                        }
                    }

                    if (!isRemoveColliderOnly) { stickyControlModule.RemoveDamageRegion(damageRegion); }
                }
            }
        }

        /// <summary>
        /// Attempt to remove the humanoid damage regions based on a pre-determined set of bones
        /// </summary>
        private void RemoveDamageRegions()
        {

            Undo.SetCurrentGroupName("Remove Damage Regions");
            int undoGroup = UnityEditor.Undo.GetCurrentGroup();
            Undo.RecordObject(stickyControlModule, string.Empty);

            RemoveDamageRegion(HumanBodyBones.Hips, false);
            RemoveDamageRegion(HumanBodyBones.Head, false);
            RemoveDamageRegion(HumanBodyBones.Chest, false);
            RemoveDamageRegion(HumanBodyBones.LeftUpperArm, false);
            RemoveDamageRegion(HumanBodyBones.LeftLowerArm, false);
            RemoveDamageRegion(HumanBodyBones.RightUpperArm, false);
            RemoveDamageRegion(HumanBodyBones.RightLowerArm, false);
            RemoveDamageRegion(HumanBodyBones.LeftUpperLeg, false);
            RemoveDamageRegion(HumanBodyBones.LeftLowerLeg, false);
            RemoveDamageRegion(HumanBodyBones.RightUpperLeg, false);
            RemoveDamageRegion(HumanBodyBones.RightLowerLeg, false);

            Undo.CollapseUndoOperations(undoGroup);
        }

        /// <summary>
        /// Attempt to remove ragdoll components from bones
        /// TODO - check damage regions before removing colliders
        /// TODO - check VR hands before removing rigidbody
        /// </summary>
        private void RemoveRagdoll()
        {
            List<S3DHumanBonePersist> bones = stickyControlModule.RagdollBoneList;

            int numBones = bones == null ? 0 : bones.Count;
            CharacterJoint joint = null;
            S3DHumanBonePersist s3dBone;
            Transform boneTfrm;
            Rigidbody rb;
            CapsuleCollider capCollider;
            BoxCollider boxCollider;

            Undo.SetCurrentGroupName("Remove Ragdoll");
            int undoGroup = UnityEditor.Undo.GetCurrentGroup();
            Undo.RecordObject(stickyControlModule, string.Empty);

            for (int bIdx = 0; bIdx < numBones; bIdx++)
            {
                s3dBone = bones[bIdx];
                boneTfrm = s3dBone.boneTransform;
                if (boneTfrm != null)
                {
                    if (s3dBone.bone == HumanBodyBones.Hips)
                    {
                        if (boneTfrm.TryGetComponent(out rb))
                        {
                            Undo.DestroyObjectImmediate(rb);

                            if (boneTfrm.TryGetComponent(out boxCollider))
                            {
                                Undo.DestroyObjectImmediate(boxCollider);
                            }
                        }
                    }
                    else if (boneTfrm.TryGetComponent(out joint))
                    {
                        Undo.DestroyObjectImmediate(joint);

                        // Only destroy rigidbody if a character joint was also found.
                        // This could still be a little risky for things like VR hands etc.
                        // Potentially could check the layer
                        if (boneTfrm.TryGetComponent(out rb))
                        {
                            Undo.DestroyObjectImmediate(rb);
                        }

                        // Don't remove any colliders from hands or feet.
                        if (s3dBone.bone != HumanBodyBones.LeftHand && s3dBone.bone != HumanBodyBones.RightHand &&
                            s3dBone.bone != HumanBodyBones.LeftFoot && s3dBone.bone != HumanBodyBones.RightFoot)
                        {
                            // Don't remove if a damage region exists for this bone
                            if (stickyControlModule.GetDamageRegion(s3dBone.bone) == null)
                            {
                                if (boneTfrm.TryGetComponent(out capCollider))
                                {
                                    Undo.DestroyObjectImmediate(capCollider);
                                }

                                if (boneTfrm.TryGetComponent(out boxCollider))
                                {
                                    Undo.DestroyObjectImmediate(boxCollider);
                                }
                            }
                        }
                    }
                }
            }

            Undo.CollapseUndoOperations(undoGroup);
        }


        /// <summary>
        /// Draw a (F)ind button which will select the item in the scene view
        /// </summary>
        /// <param name="isSelectedInSceneView"></param>
        /// <param name="showGizmoInSceneViewProp"></param>
        private void SelectItemInSceneViewButton(ref bool isSelectedInSceneView, SerializedProperty showGizmoInSceneViewProp)
        {
            if (GUILayout.Button(StickyEditorHelper.gizmoFindBtnContent, buttonCompact, GUILayout.MaxWidth(20f), GUILayout.MinHeight(18f)))
            {
                serializedObject.ApplyModifiedProperties();
                DeselectAllComponents();
                serializedObject.Update();
                isSelectedInSceneView = true;
                showGizmoInSceneViewProp.boolValue = true;
                // Hide Unity tools
                Tools.hidden = true;
            }
        }

        #endregion

        #region OnInspectorGUI
        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            #region Initialise
            stickyControlModule.allowRepaint = false;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
            isSceneModified = false;
            #endregion

            ConfigureButtonsAndStyles();

            // Read in all the properties
            serializedObject.Update();

            #region Header Info and Buttons
            StickyEditorHelper.DrawStickyVersionLabel(labelFieldRichText);

            EditorGUILayout.LabelField(headerContent, helpBoxRichText);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            StickyEditorHelper.DrawGetHelpButtons(buttonCompact);

            int prevTab = selectedTabIntProp.intValue;

            // Show a toolbar to allow the user to switch between viewing different areas
            selectedTabIntProp.intValue = GUILayout.Toolbar(selectedTabIntProp.intValue, tabTexts);

            // When switching tabs, disable focus on previous control
            if (prevTab != selectedTabIntProp.intValue) { GUI.FocusControl(null); }

            EditorGUILayout.EndVertical();
            #endregion

            #region Movement tab
            if (selectedTabIntProp.intValue == 0)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                #region Movement General Properties
                EditorGUILayout.PropertyField(initialiseOnAwakeProp, initialiseOnAwakeContent);

                StickyEditorHelper.DrawS3DFoldout(isMoveGeneralExpandedProp, isMoveGeneralExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

                if (isMoveGeneralExpandedProp.boolValue)
                {
                    EditorGUILayout.PropertyField(isNPCProp, isNPCContent);
                    EditorGUILayout.PropertyField(walkSpeedProp, walkSpeedContent);
                    EditorGUILayout.PropertyField(sprintSpeedProp, sprintSpeedContent);
                    EditorGUILayout.PropertyField(strafeSpeedProp, strafeSpeedContent);
                    EditorGUILayout.PropertyField(jumpSpeedProp, jumpSpeedContent);
                    EditorGUILayout.PropertyField(jumpDelayProp, jumpDelayContent);
                    EditorGUILayout.PropertyField(crouchSpeedProp, crouchSpeedContent);
                    EditorGUILayout.PropertyField(maxAccelerationProp, maxAccelerationContent);
                    EditorGUILayout.PropertyField(maxStepOffsetProp, maxStepOffsetContent);
                    EditorGUILayout.PropertyField(stepUpSpeedProp, stepUpSpeedContent);
                    EditorGUILayout.PropertyField(stepUpBiasProp, stepUpBiasContent);
                    EditorGUILayout.PropertyField(maxSlopeAngleProp, maxSlopeAngleContent);
                    EditorGUILayout.PropertyField(alignToGroundNormalProp, alignToGroundNormalContent);
                    EditorGUILayout.PropertyField(verticalRotationRateProp, verticalRotationRateContent);
                    EditorGUILayout.PropertyField(turnRotationRateProp, turnRotationRateContent);
                    EditorGUILayout.PropertyField(allowMovementInAirProp, allowMovementInAirContent);
                    EditorGUILayout.PropertyField(allowSprintBackwardProp, allowSprintBackwardContent);
                    EditorGUILayout.PropertyField(gravitationalAccelerationProp, gravitationalAccelerationContent);
                    EditorGUILayout.PropertyField(arcadeFallMultiplierProp, arcadeFallMultiplierContent);

                    EditorGUILayout.PropertyField(stuckTimeProp, stuckTimeContent);
                    if (stuckTimeProp.floatValue > 0f)
                    {
                        EditorGUILayout.PropertyField(stuckSpeedThresholdProp, stuckSpeedThresholdContent);
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(moveUpdateTypeProp, moveUpdateTypeContent);
                    if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                    {
                        stickyControlModule.SetMoveUpdateType((StickyControlModule.MoveUpdateType)moveUpdateTypeProp.intValue);
                    }

                    // Warn user if MoveUpdateType looks incorrect
                    if (isRootMotionProp.boolValue && !stickyControlModule.IsMoveFixedUpdate)
                    {
                        EditorGUILayout.HelpBox("If using Root Motion on Animate tab, movement needs to be done in Fixed Update", MessageType.Warning);
                    }
                }
                #endregion

                #region Climbing
                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

                GUILayout.BeginHorizontal();
                StickyEditorHelper.DrawS3DFoldout(isClimbingExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                #if UNITY_2019_3_OR_NEWER
                EditorGUILayout.LabelField(isClimbingEnabledContent, GUILayout.Width(155f));
                #else
                EditorGUILayout.LabelField(isClimbingEnabledContent, GUILayout.Width(152f));
                #endif
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(isClimbingEnabledProp, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    if (EditorApplication.isPlaying)
                    {
                        serializedObject.ApplyModifiedProperties();

                        if (isClimbingEnabledProp.boolValue)
                        {
                            stickyControlModule.EnableClimbing();
                        }
                        else
                        {
                            stickyControlModule.DisableClimbing();
                        }

                        serializedObject.Update();
                    }
                    else if (isClimbingEnabledProp.boolValue)
                    {
                        // Validate Climbing settings
                        if (minClimbSlopeAngleProp.floatValue < maxSlopeAngleProp.floatValue)
                        {
                            minClimbSlopeAngleProp.floatValue = maxSlopeAngleProp.floatValue;
                        }
                        if (maxClimbSlopeAngleProp.floatValue < minClimbSlopeAngleProp.floatValue)
                        {
                            maxClimbSlopeAngleProp.floatValue = minClimbSlopeAngleProp.floatValue;
                        }
                    }
                }
                GUILayout.EndHorizontal();

                if (isClimbingEnabledProp.boolValue && isClimbingExpandedProp.boolValue)
                {
                    StickyEditorHelper.InTechPreview(true);

                    #region Climbing General Properties

                    EditorGUILayout.PropertyField(climbSpeedProp, climbSpeedContent);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(minClimbSlopeAngleProp, minClimbSlopeAngleContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (minClimbSlopeAngleProp.floatValue < maxSlopeAngleProp.floatValue)
                        {
                            minClimbSlopeAngleProp.floatValue = maxSlopeAngleProp.floatValue;
                        }
                        if (maxClimbSlopeAngleProp.floatValue < minClimbSlopeAngleProp.floatValue)
                        {
                            maxClimbSlopeAngleProp.floatValue = minClimbSlopeAngleProp.floatValue;
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(maxClimbSlopeAngleProp, maxClimbSlopeAngleContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (maxClimbSlopeAngleProp.floatValue < maxSlopeAngleProp.floatValue)
                        {
                            maxClimbSlopeAngleProp.floatValue = maxSlopeAngleProp.floatValue;
                        }
                        if (minClimbSlopeAngleProp.floatValue > maxClimbSlopeAngleProp.floatValue)
                        {
                            minClimbSlopeAngleProp.floatValue = maxClimbSlopeAngleProp.floatValue;
                        }
                    }

                    EditorGUILayout.PropertyField(climbFaceSurfaceRateProp, climbFaceSurfaceRateContent);
                    EditorGUILayout.PropertyField(maxGrabDistanceProp, maxGrabDistanceContent);
                    EditorGUILayout.PropertyField(climbTopDetectionProp, climbTopDetectionContent);
                    EditorGUILayout.PropertyField(climbableLayerMaskProp, climbableLayerMaskContent);

                    #endregion
                }

                #endregion

                #region Footsteps
                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

                GUILayout.BeginHorizontal();
                StickyEditorHelper.DrawS3DFoldout(isS3DFootstepExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                #if UNITY_2019_3_OR_NEWER
                EditorGUILayout.LabelField(isFootStepsEnabledContent, GUILayout.Width(155f));
                #else
                EditorGUILayout.LabelField(isFootStepsEnabledContent, GUILayout.Width(152f));
                #endif
                EditorGUILayout.PropertyField(isFootStepsEnabledProp, GUIContent.none);
                GUILayout.EndHorizontal();

                if (isFootStepsEnabledProp.boolValue && isS3DFootstepExpandedProp.boolValue)
                {
                    #region Footstep General Properties
                    EditorGUILayout.PropertyField(footStepsUseMoveSpeedProp, footStepsUseMoveSpeedContent);

                    if (!footStepsUseMoveSpeedProp.boolValue)
                    {
                        // Left Foot
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(leftFootProp, leftFootContent);
                        if (EditorGUI.EndChangeCheck() && leftFootProp.objectReferenceValue != null)
                        {
                            if (!((Transform)leftFootProp.objectReferenceValue).IsChildOf(stickyControlModule.transform))
                            {
                                leftFootProp.objectReferenceValue = null;
                                Debug.LogWarning("The left foot transform must be a child of the parent Sticky3D Controller gameobject or part of the prefab.");
                            }
                        }

                        // Right Foot
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(rightFootProp, rightFootContent);
                        if (EditorGUI.EndChangeCheck() && rightFootProp.objectReferenceValue != null)
                        {
                            if (!((Transform)rightFootProp.objectReferenceValue).IsChildOf(stickyControlModule.transform))
                            {
                                rightFootProp.objectReferenceValue = null;
                                Debug.LogWarning("The right foot transform must be a child of the parent Sticky3D Controller gameobject or part of the prefab.");
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(footStepWalkFrequencyProp, footStepWalkFrequencyContent);
                        EditorGUILayout.PropertyField(footStepSprintFrequencyProp, footStepSprintFrequencyContent);
                    }

                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginHorizontal();
                    #if UNITY_2019_1_OR_NEWER
                    EditorGUILayout.LabelField(footStepsAudioContent, GUILayout.Width(defaultEditorLabelWidth - 53f));
                    #else
                    EditorGUILayout.LabelField(footStepsAudioContent, GUILayout.Width(defaultEditorLabelWidth - 58f));
                    #endif

                    if (GUILayout.Button(footStepsNewAudioContent, buttonCompact, GUILayout.Width(50f)) && footStepsAudioProp.objectReferenceValue == null)
                    {
                        serializedObject.ApplyModifiedProperties();

                        Undo.SetCurrentGroupName("New Audio Source");
                        int undoGroup = UnityEditor.Undo.GetCurrentGroup();

                        Undo.RecordObject(stickyControlModule, string.Empty);

                        GameObject audioGameObject = new GameObject("Audio");
                        if (audioGameObject != null)
                        {
                            Undo.RegisterCreatedObjectUndo(audioGameObject, string.Empty);
                            AudioSource _newAudioSource = Undo.AddComponent(audioGameObject, typeof(AudioSource)) as AudioSource;
                            _newAudioSource.playOnAwake = false;
                            _newAudioSource.maxDistance = 25f;
                            _newAudioSource.spatialBlend = 1f;
                            stickyControlModule.footStepsAudio = _newAudioSource;

                            audioGameObject.transform.SetParent(stickyControlModule.transform, false);
                        }

                        Undo.CollapseUndoOperations(undoGroup);

                        // Should be non-scene objects but is required to force being set as dirty
                        EditorUtility.SetDirty(stickyControlModule);

                        GUIUtility.ExitGUI();
                    }

                    EditorGUILayout.PropertyField(footStepsAudioProp, GUIContent.none);

                    GUILayout.EndHorizontal();

                    if (EditorGUI.EndChangeCheck() && footStepsAudioProp.objectReferenceValue != null)
                    {
                        if (!((AudioSource)footStepsAudioProp.objectReferenceValue).transform.IsChildOf(stickyControlModule.transform))
                        {
                            footStepsAudioProp.objectReferenceValue = null;
                            Debug.LogWarning("The foot steps audio source transform must be a child of the parent Sticky3D Controller gameobject or part of the prefab.");
                        }
                    }

                    DrawAudioClip(footStepsDefaultClipProp, footStepsDefaultClipContent);
                    //EditorGUILayout.PropertyField(footStepsDefaultClipProp, footStepsDefaultClipContent);

                    EditorGUILayout.PropertyField(footStepsVolumeProp, footStepsVolumeContent);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(s3dSurfacesProp, s3dSurfacesContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        RefreshSurfaceTypes();
                    }
                    #endregion

                    #region Check if s3dFootstepList is null
                    // Checking the property for being NULL doesn't check if the list is actually null.
                    if (stickyControlModule.s3dFootstepList == null)
                    {
                        // Apply property changes
                        serializedObject.ApplyModifiedProperties();
                        stickyControlModule.s3dFootstepList = new List<S3DFootstep>(10);
                        isSceneModified = true;
                        // Read in the properties
                        serializedObject.Update();
                    }
                    #endregion

                    #region Add-Remove S3DFootsteps
                    int numFootSteps = s3dFootstepListProp.arraySize;

                    // Reset button variables
                    s3dFootstepMoveDownPos = -1;
                    s3dFootstepInsertPos = -1;
                    s3dFootstepDeletePos = -1;

                    GUILayout.BeginHorizontal();

                    EditorGUI.indentLevel += 1;
                    EditorGUIUtility.fieldWidth = 15f;
                    EditorGUI.BeginChangeCheck();
                    isS3DFootstepListExpandedProp.boolValue = EditorGUILayout.Foldout(isS3DFootstepListExpandedProp.boolValue, "", foldoutStyleNoLabel);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        ExpandList(stickyControlModule.s3dFootstepList, isS3DFootstepListExpandedProp.boolValue);
                        // Read in the properties
                        serializedObject.Update();
                    }
                    EditorGUI.indentLevel -= 1;

                    EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                    //EditorGUILayout.LabelField("<color=" + txtColourName + ">Surface Actions: " + numFootSteps.ToString("00") + "</color>", labelFieldRichText);
                    EditorGUILayout.LabelField("Surface Actions: " + numFootSteps.ToString("00"), labelFieldRichText);

                    if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                    {
                        // Apply property changes
                        serializedObject.ApplyModifiedProperties();
                        Undo.RecordObject(stickyControlModule, "Add Surface Action");
                        stickyControlModule.s3dFootstepList.Add(new S3DFootstep());
                        ExpandList(stickyControlModule.s3dFootstepList, false);
                        isSceneModified = true;
                        // Read in the properties
                        serializedObject.Update();

                        numFootSteps = s3dFootstepListProp.arraySize;
                        if (numFootSteps > 0)
                        {
                            // Force new FootStep to be serialized in scene
                            s3dFootstepProp = s3dFootstepListProp.GetArrayElementAtIndex(numFootSteps - 1);
                            s3dFSShowInEditorProp = s3dFootstepProp.FindPropertyRelative("showInEditor");
                            s3dFSShowInEditorProp.boolValue = !s3dFSShowInEditorProp.boolValue;
                            // Show the new Footstep
                            s3dFSShowInEditorProp.boolValue = true;
                        }
                    }
                    if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                    {
                        if (numFootSteps > 0) { s3dFootstepDeletePos = s3dFootstepListProp.arraySize - 1; }
                    }

                    GUILayout.EndHorizontal();
                    #endregion

                    #region S3DFootstep List
                    numFootSteps = s3dFootstepListProp.arraySize;

                    for (int fsIdx = 0; fsIdx < numFootSteps; fsIdx++)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        s3dFootstepProp = s3dFootstepListProp.GetArrayElementAtIndex(fsIdx);

                        #region Get Properties for the Footstep surface
                        s3dFSShowInEditorProp = s3dFootstepProp.FindPropertyRelative("showInEditor");
                        #endregion

                        #region Footstep Move/Insert/Delete buttons
                        GUILayout.BeginHorizontal();
                        EditorGUI.indentLevel += 1;
                        s3dFSShowInEditorProp.boolValue = EditorGUILayout.Foldout(s3dFSShowInEditorProp.boolValue, "Surface Action " + (fsIdx + 1).ToString("00"));
                        EditorGUI.indentLevel -= 1;

                        // Move down button
                        if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numFootSteps > 1) { s3dFootstepMoveDownPos = fsIdx; }
                        // Create duplicate button
                        if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { s3dFootstepInsertPos = fsIdx; }
                        // Delete button
                        if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dFootstepDeletePos = fsIdx; }
                        GUILayout.EndHorizontal();
                        #endregion

                        if (s3dFSShowInEditorProp.boolValue)
                        {
                            #region General Footstep Properties
                            s3dFSMinVolumeProp = s3dFootstepProp.FindPropertyRelative("minVolume");
                            s3dFSMaxVolumeProp = s3dFootstepProp.FindPropertyRelative("maxVolume");
                            s3dFSMinPitchProp = s3dFootstepProp.FindPropertyRelative("minPitch");
                            s3dFSMaxPitchProp = s3dFootstepProp.FindPropertyRelative("maxPitch");

                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(s3dFSMinVolumeProp, fsMinVolumeContent);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (s3dFSMinVolumeProp.floatValue > s3dFSMaxVolumeProp.floatValue) { s3dFSMaxVolumeProp.floatValue = s3dFSMinVolumeProp.floatValue; }
                            }
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(s3dFSMaxVolumeProp, fsMaxVolumeContent);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (s3dFSMaxVolumeProp.floatValue < s3dFSMinVolumeProp.floatValue) { s3dFSMinVolumeProp.floatValue = s3dFSMaxVolumeProp.floatValue; }
                            }
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(s3dFSMinPitchProp, fsMinPitchContent);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (s3dFSMinPitchProp.floatValue > s3dFSMaxPitchProp.floatValue) { s3dFSMaxPitchProp.floatValue = s3dFSMinPitchProp.floatValue; }
                            }
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(s3dFSMaxPitchProp, fsMaxPitchContent);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (s3dFSMaxPitchProp.floatValue < s3dFSMinPitchProp.floatValue) { s3dFSMinPitchProp.floatValue = s3dFSMaxPitchProp.floatValue; }
                            }

                            #endregion

                            #region Mesh Surface Types
                            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
                            if (s3dSurfacesProp.objectReferenceValue == null)
                            {
                                EditorGUILayout.LabelField(" Mesh Surface Types", GUILayout.Width(130f));
                                EditorGUILayout.HelpBox("Configure Known Surface Types to use this feature", MessageType.Info);
                            }
                            else
                            {
                                #region Check if SurfaceType List is null
                                // Checking the property for being NULL doesn't check if the list is actually null.
                                if (stickyControlModule.s3dFootstepList[fsIdx].surfaceTypeIntList == null)
                                {
                                    // Apply property changes
                                    serializedObject.ApplyModifiedProperties();
                                    stickyControlModule.s3dFootstepList[fsIdx].surfaceTypeIntList = new List<int>(5);
                                    isSceneModified = true;
                                    // Read in the properties
                                    serializedObject.Update();
                                }
                                #endregion

                                #region Add or Remove Footstep Surface Types
                                s3dFootstepSurfaceTypeDeletePos = -1;
                                s3dFSSurfaceTypeListProp = s3dFootstepProp.FindPropertyRelative("surfaceTypeIntList");
                                int numFSSurfaceTypes = s3dFSSurfaceTypeListProp.arraySize;

                                GUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(" Mesh Surface Types", GUILayout.Width(130f));

                                if (GUILayout.Button("+", buttonCompact, GUILayout.Width(20f)) && numFSSurfaceTypes < 9)
                                {
                                    // Apply property changes
                                    serializedObject.ApplyModifiedProperties();
                                    Undo.RecordObject(stickyControlModule, "Add Surface Type");
                                    stickyControlModule.s3dFootstepList[fsIdx].surfaceTypeIntList.Add(0);
                                    isSceneModified = true;
                                    // Read in the properties
                                    serializedObject.Update();

                                    numFSSurfaceTypes = s3dFSSurfaceTypeListProp.arraySize;
                                }
                                if (GUILayout.Button("-", buttonCompact, GUILayout.Width(20f)))
                                {
                                    if (numFSSurfaceTypes > 0) { s3dFootstepSurfaceTypeDeletePos = s3dFSSurfaceTypeListProp.arraySize - 1; }
                                }

                                GUILayout.EndHorizontal();
                                #endregion

                                #region Surface Type List

                                for (int fsSurfaceTypeIdx = 0; fsSurfaceTypeIdx < numFSSurfaceTypes; fsSurfaceTypeIdx++)
                                {
                                    s3dFSSurfaceTypeProp = s3dFSSurfaceTypeListProp.GetArrayElementAtIndex(fsSurfaceTypeIdx);
                                    if (s3dFSSurfaceTypeProp != null)
                                    {
                                        GUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField(" " + (fsSurfaceTypeIdx + 1) + ".", GUILayout.Width(20f));

                                        // Find the index of the surface type using the guidHash of the type.
                                        int surfaceTypeIndex = 0;

                                        if (s3dSurfacesProp.objectReferenceValue != null)
                                        {
                                            surfaceTypeIndex = ((S3DSurfaces)s3dSurfacesProp.objectReferenceValue).GetSurfaceTypeIndex(s3dFSSurfaceTypeProp.intValue);

                                            // Cater for the "Not Set" item at the top of the list
                                            surfaceTypeIndex++;
                                        }

                                        EditorGUI.BeginChangeCheck();
                                        surfaceTypeIndex = EditorGUILayout.Popup(surfaceTypeIndex, surfaceTypeArray);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            if (surfaceTypeIndex <= 0) { s3dFSSurfaceTypeProp.intValue = 0; }
                                            else
                                            {
                                                S3DSurfaces s3dSurfaces = ((S3DSurfaces)s3dSurfacesProp.objectReferenceValue);

                                                s3dFSSurfaceTypeProp.intValue = s3dSurfaces == null ? 0 : s3dSurfaces.GetSurfaceTypeID(surfaceTypeIndex - 1);
                                            }
                                        }

                                        if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dFootstepSurfaceTypeDeletePos = fsSurfaceTypeIdx; }
                                        GUILayout.EndHorizontal();
                                    }
                                }
                                #endregion

                                #region Delete Surface Type
                                if (s3dFootstepSurfaceTypeDeletePos >= 0)
                                {
                                    GUI.FocusControl(null);
                                    s3dFSSurfaceTypeListProp.DeleteArrayElementAtIndex(s3dFootstepSurfaceTypeDeletePos);
                                    s3dFootstepSurfaceTypeDeletePos = -1;

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

                            #region Terrain Textures
                            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

                            #region Check if TerrainTexture list is null
                            // Checking the property for being NULL doesn't check if the list is actually null.
                            if (stickyControlModule.s3dFootstepList[fsIdx].s3dTerrainTextureList == null)
                            {
                                // Apply property changes
                                serializedObject.ApplyModifiedProperties();
                                stickyControlModule.s3dFootstepList[fsIdx].s3dTerrainTextureList = new List<S3DTerrainTexture>(2);
                                isSceneModified = true;
                                // Read in the properties
                                serializedObject.Update();
                            }
                            #endregion

                            #region Add or Remove Footstep Terrain Textures
                            s3dFootstepTerrainTextureDeletePos = -1;
                            s3dFSTerrainTextureListProp = s3dFootstepProp.FindPropertyRelative("s3dTerrainTextureList");
                            int numFSTerrainTextures = s3dFSTerrainTextureListProp.arraySize;

                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(" Terrain Textures", GUILayout.Width(130f));

                            if (GUILayout.Button("+", buttonCompact, GUILayout.Width(20f)) && numFSTerrainTextures < 9)
                            {
                                // Apply property changes
                                serializedObject.ApplyModifiedProperties();
                                Undo.RecordObject(stickyControlModule, "Add S3D Terrain Tex");
                                stickyControlModule.s3dFootstepList[fsIdx].s3dTerrainTextureList.Add(new S3DTerrainTexture());
                                isSceneModified = true;
                                // Read in the properties
                                serializedObject.Update();

                                numFSTerrainTextures = s3dFSTerrainTextureListProp.arraySize;
                            }
                            if (GUILayout.Button("-", buttonCompact, GUILayout.Width(20f)))
                            {
                                if (numFSTerrainTextures > 0) { s3dFootstepTerrainTextureDeletePos = s3dFSTerrainTextureListProp.arraySize - 1; }
                            }

                            GUILayout.EndHorizontal();

                            #endregion

                            #region Terrain Texture List
                            for (int fsTerrainTexIdx = 0; fsTerrainTexIdx < numFSTerrainTextures; fsTerrainTexIdx++)
                            {
                                s3dFSTerrainTextureProp = s3dFSTerrainTextureListProp.GetArrayElementAtIndex(fsTerrainTexIdx);
                                if (s3dFSTerrainTextureProp != null)
                                {
                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(" " + (fsTerrainTexIdx + 1) + ".", GUILayout.Width(20f));
                                    EditorGUI.BeginChangeCheck();
                                    EditorGUILayout.PropertyField(s3dFSTerrainTextureProp.FindPropertyRelative("albedoName"), GUIContent.none);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        // WARNING: This does not fire when an UNDO occurs, so the hash for the name is now incorrect.
                                        serializedObject.ApplyModifiedProperties();
                                        stickyControlModule.s3dFootstepList[fsIdx].s3dTerrainTextureList[fsTerrainTexIdx].RefreshID();
                                        serializedObject.Update();
                                    }
                                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dFootstepTerrainTextureDeletePos = fsTerrainTexIdx; }
                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(" ", GUILayout.Width(18f));
                                    EditorGUILayout.LabelField(fsMinWeightContent, GUILayout.Width(80f));
                                    EditorGUILayout.PropertyField(s3dFSTerrainTextureProp.FindPropertyRelative("minWeight"), GUIContent.none);
                                    GUILayout.EndHorizontal();
                                }
                            }
                            #endregion

                            #region Delete Terrain Texture
                            if (s3dFootstepTerrainTextureDeletePos >= 0)
                            {
                                GUI.FocusControl(null);
                                s3dFSTerrainTextureListProp.DeleteArrayElementAtIndex(s3dFootstepTerrainTextureDeletePos);
                                s3dFootstepTerrainTextureDeletePos = -1;

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

                            #region Audio Clips
                            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
                            #region Check if AudioClip List is null
                            // Checking the property for being NULL doesn't check if the list is actually null.
                            if (stickyControlModule.s3dFootstepList[fsIdx].audioclipList == null)
                            {
                                // Apply property changes
                                serializedObject.ApplyModifiedProperties();
                                stickyControlModule.s3dFootstepList[fsIdx].audioclipList = new List<AudioClip>(3);
                                isSceneModified = true;
                                // Read in the properties
                                serializedObject.Update();
                            }
                            #endregion

                            #region Add or Remove Footstep AudioClips
                            s3dFootstepAudioClipDeletePos = -1;
                            s3dFSAudioClipListProp = s3dFootstepProp.FindPropertyRelative("audioclipList");
                            int numFSAudioClips = s3dFSAudioClipListProp.arraySize;

                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(" Audio Clips", GUILayout.Width(75f));

                            if (GUILayout.Button("+", buttonCompact, GUILayout.Width(20f)) && numFSAudioClips < 9)
                            {
                                // Apply property changes
                                serializedObject.ApplyModifiedProperties();
                                Undo.RecordObject(stickyControlModule, "Add Audio Clip");
                                stickyControlModule.s3dFootstepList[fsIdx].audioclipList.Add(null);
                                isSceneModified = true;
                                // Read in the properties
                                serializedObject.Update();

                                numFSAudioClips = s3dFSAudioClipListProp.arraySize;
                            }
                            if (GUILayout.Button("-", buttonCompact, GUILayout.Width(20f)))
                            {
                                if (numFSAudioClips > 0) { s3dFootstepAudioClipDeletePos = s3dFSAudioClipListProp.arraySize - 1; }
                            }

                            GUILayout.EndHorizontal();

                            #endregion

                            #region AudioClip List

                            for (int fsAudClipIdx = 0; fsAudClipIdx < numFSAudioClips; fsAudClipIdx++)
                            {
                                s3dFSAudioClipProp = s3dFSAudioClipListProp.GetArrayElementAtIndex(fsAudClipIdx);
                                if (s3dFSAudioClipProp != null)
                                {
                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(" " + (fsAudClipIdx + 1) + ".", GUILayout.Width(20f));

                                    if (GUILayout.Button(footStepsListenContent, buttonCompact, GUILayout.MaxWidth(20f)))
                                    {
                                        if (isFootstepClipPlaying)
                                        {
                                            StickyEditorHelper.StopAllAudioClips();
                                            isFootstepClipPlaying = false;
                                        }
                                        else if (s3dFSAudioClipProp.objectReferenceValue != null)
                                        {
                                            StickyEditorHelper.PlayAudioClip((AudioClip)s3dFSAudioClipProp.objectReferenceValue, 0, false);
                                            isFootstepClipPlaying = true;
                                        }
                                    }

                                    EditorGUILayout.PropertyField(s3dFSAudioClipProp, GUIContent.none);
                                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dFootstepAudioClipDeletePos = fsAudClipIdx; }
                                    GUILayout.EndHorizontal();
                                }
                            }

                            #endregion

                            #region Delete Audio Clip
                            if (s3dFootstepAudioClipDeletePos >= 0)
                            {
                                GUI.FocusControl(null);
                                s3dFSAudioClipListProp.DeleteArrayElementAtIndex(s3dFootstepAudioClipDeletePos);
                                s3dFootstepAudioClipDeletePos = -1;

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
                        }

                        GUILayout.EndVertical();
                    }
                    #endregion

                    #region Move/Insert/Delete S3DFootstep
                    if (s3dFootstepDeletePos >= 0 || s3dFootstepInsertPos >= 0 || s3dFootstepMoveDownPos >= 0)
                    {
                        GUI.FocusControl(null);
                        // Don't permit multiple operations in the same pass
                        if (s3dFootstepMoveDownPos >= 0)
                        {
                            // Move down one position, or wrap round to start of list
                            if (s3dFootstepMoveDownPos < s3dFootstepListProp.arraySize - 1)
                            {
                                s3dFootstepListProp.MoveArrayElement(s3dFootstepMoveDownPos, s3dFootstepMoveDownPos + 1);
                            }
                            else { s3dFootstepListProp.MoveArrayElement(s3dFootstepMoveDownPos, 0); }

                            s3dFootstepMoveDownPos = -1;
                        }
                        else if (s3dFootstepInsertPos >= 0)
                        {
                            // NOTE: Undo doesn't work with Insert.

                            // Apply property changes before potential list changes
                            serializedObject.ApplyModifiedProperties();

                            S3DFootstep insertedFootstep = new S3DFootstep(stickyControlModule.s3dFootstepList[s3dFootstepInsertPos]);
                            insertedFootstep.showInEditor = true;
                            // Generate a new hashcode for the duplicated Footstep
                            insertedFootstep.guidHash = S3DMath.GetHashCodeFromGuid();

                            stickyControlModule.s3dFootstepList.Insert(s3dFootstepInsertPos, insertedFootstep);

                            // Read all properties from the Sticky Controller
                            serializedObject.Update();

                            // Hide original Footstep
                            s3dFootstepListProp.GetArrayElementAtIndex(s3dFootstepInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                            s3dFSShowInEditorProp = s3dFootstepListProp.GetArrayElementAtIndex(s3dFootstepInsertPos).FindPropertyRelative("showInEditor");

                            // Force new action to be serialized in scene
                            s3dFSShowInEditorProp.boolValue = !s3dFSShowInEditorProp.boolValue;

                            // Show inserted duplicate Footstep
                            s3dFSShowInEditorProp.boolValue = true;

                            s3dFootstepInsertPos = -1;

                            isSceneModified = true;
                        }
                        else if (s3dFootstepDeletePos >= 0)
                        {
                            // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and DeletePos is reset to -1.
                            int _deleteIndex = s3dFootstepDeletePos;

                            if (EditorUtility.DisplayDialog("Delete Surface " + (s3dFootstepDeletePos + 1) + "?", "Surface " + (s3dFootstepDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the Surface from the list and cannot be undone.", "Delete Now", "Cancel"))
                            {
                                s3dFootstepListProp.DeleteArrayElementAtIndex(_deleteIndex);
                                s3dFootstepDeletePos = -1;
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
                }

                #endregion

                EditorGUILayout.EndVertical();
            }
            #endregion

            #region Look tab
            else if (selectedTabIntProp.intValue == 1)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                EditorGUILayout.PropertyField(lookOnInitialiseProp, lookOnInitialiseContent);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(isThirdPersonProp, isThirdPersonContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyControlModule.ToggleFirstThirdPerson();
                }

                StickyEditorHelper.DrawS3DFoldout(isLookGeneralExpandedProp, isLookGeneralExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

                if (isLookGeneralExpandedProp.boolValue)
                {
                    #region Look Third Person
                    if (isThirdPersonProp.boolValue)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(isFreeLookProp, isFreeLookContent);
                        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                        {
                            stickyControlModule.SetFreeLook(isFreeLookProp.boolValue);
                        }

                        if (isLookVRProp.boolValue) { EditorGUILayout.HelpBox("VR is currently only supported in First Person mode", MessageType.Error); }

                        // Third Person Camera (the transform is always the transform of the camera)
                        EditorGUI.BeginChangeCheck();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(lookCamera1ThirdPersonContent, GUILayout.Width(defaultEditorLabelWidth - 46f));
                        if (GUILayout.Button(lookCameraAlignContent, GUILayout.Width(40f)))
                        {
                            if (lookThirdPersonCamera1Prop.objectReferenceValue != null)
                            {
                                Transform _camTrfm = ((Camera)lookThirdPersonCamera1Prop.objectReferenceValue).transform;

                                lookCameraOffsetProp.vector3Value = stickyControlModule.GetLocalPosition(_camTrfm.position);

                                // If freelook is enabled, we need to reset orbit etc to avoid camera going to the wrong position
                                if (stickyControlModule.IsLookFreeLookEnabled && EditorApplication.isPlaying)
                                {
                                    stickyControlModule.ResetFreeLook();
                                }
                            }
                        }
                        EditorGUILayout.PropertyField(lookThirdPersonCamera1Prop, GUIContent.none);
                        GUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck() && lookThirdPersonCamera1Prop.objectReferenceValue != null)
                        {
                            if (((Camera)lookThirdPersonCamera1Prop.objectReferenceValue).transform.IsChildOf(stickyControlModule.transform))
                            {
                                lookThirdPersonCamera1Prop.objectReferenceValue = null;
                                Debug.LogWarning("For Third Person, the camera must not be a child of the parent Sticky3D Controller gameobject or part of the prefab.");
                            }
                        }

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(lookCameraOffsetContent, GUILayout.Width(defaultEditorLabelWidth - 26f));
                        StickyEditorHelper.DrawGizmosButton(showLookCamOffsetGizmosInSceneViewProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled);                        
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(lookCameraOffsetProp, GUIContent.none);
                        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                        {
                            stickyControlModule.GetCameraOffsetDistance();
                        }
                        GUILayout.EndHorizontal();

                        if (lookCameraOffsetProp.vector3Value == Vector3.zero && lookThirdPersonCamera1Prop.objectReferenceValue != null)
                        {
                            EditorGUILayout.HelpBox("When third person is used at runtime, the Camera Offset must not be 0,0,0", MessageType.Warning);
                        }

                        GUILayout.BeginHorizontal();
                        #if UNITY_2019_1_OR_NEWER
                        EditorGUILayout.LabelField(lookFocusOffsetContent, GUILayout.Width(defaultEditorLabelWidth - 26f));
                        #else
                        EditorGUILayout.LabelField(lookFocusOffsetContent, GUILayout.Width(defaultEditorLabelWidth - 31f));
                        #endif
                        StickyEditorHelper.DrawGizmosButton(showLookFocusOffsetGizmosInSceneViewProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled);                        
                        EditorGUILayout.PropertyField(lookFocusOffsetProp, GUIContent.none);
                        GUILayout.EndHorizontal();

                        EditorGUILayout.PropertyField(lookHorizontalSpeedProp, lookHorizontalSpeedContent);
                        EditorGUILayout.PropertyField(lookHorizontalDampingProp, lookHorizontalDampingContent);
                        EditorGUILayout.PropertyField(lookVerticalSpeedProp, lookVerticalSpeedContent);
                        EditorGUILayout.PropertyField(lookVerticalDampingProp, lookVerticalDampingContent);

                        if (isFreeLookProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(lookPitchUpLimitProp, lookPitchUpLimitContent);
                            EditorGUILayout.PropertyField(lookPitchDownLimitProp, lookPitchDownLimitContent);
                        }

                        EditorGUILayout.PropertyField(lookMoveSpeedProp, lookMoveSpeedContent);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(lookShowCursorProp, lookShowCursorContent);
                        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                        {
                            if (lookShowCursorProp.boolValue) { stickyControlModule.ShowCursor(); }
                            else { stickyControlModule.HideCursor(); }
                        }

                        EditorGUILayout.PropertyField(lookAutoHideCursorProp, lookAutoHideCursorContent);
                        if (lookAutoHideCursorProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(lookHideCursorTimeProp, lookHideCursorTimeContent);
                        }

                        EditorGUILayout.PropertyField(lookZoomDurationProp, lookZoomDurationContent);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(lookUnzoomDelayProp, lookUnzoomDelayContent);
                        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                        {
                            stickyControlModule.SetLookUnzoomDelay(lookUnzoomDelayProp.floatValue);
                        }

                        EditorGUILayout.PropertyField(lookZoomOutFactorProp, lookZoomOutFactorContent);

                        EditorGUILayout.PropertyField(lookOrbitDurationProp, lookOrbitDurationContent);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(lookUnorbitDelayProp, lookUnorbitDelayContent);
                        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                        {
                            stickyControlModule.SetLookUnorbitDelay(lookUnzoomDelayProp.floatValue);
                        }

                        EditorGUILayout.PropertyField(lookOrbitDampingProp, lookOrbitDampingContent);
                        EditorGUILayout.PropertyField(lookOrbitMinAngleProp, lookOrbitMinAngleContent);
                        EditorGUILayout.PropertyField(lookOrbitMaxAngleProp, lookOrbitMaxAngleContent);

                        EditorGUILayout.PropertyField(lookMaxLoSFoVProp, lookMaxLoSFoVContent);

                        EditorGUILayout.PropertyField(lookMaxShakeStrengthProp, lookMaxShakeStrengthContent);

                        if (lookMaxShakeStrengthProp.floatValue > 0f)
                        {
                            EditorGUILayout.PropertyField(lookMaxShakeDurationProp, lookMaxShakeDurationContent);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(lookUpdateTypeProp, lookUpdateTypeContent);
                        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                        {
                            stickyControlModule.SetLookUpdateType((StickyControlModule.LookUpdateType)lookUpdateTypeProp.intValue);
                        }

                        //EditorGUILayout.PropertyField(lookFocusToleranceProp, lookFocusToleranceContent);
                    }

                    #endregion

                    #region Look First Person
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(isFreeLookProp, isFreeLookContent);
                        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                        {
                            stickyControlModule.SetFreeLook(isFreeLookProp.boolValue);
                        }

                        // First Person has a Camera and a tranform that can be rotated (they could be on the same gameobject)
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(lookFirstPersonTransform1Prop, lookTransformContent);
                        if (EditorGUI.EndChangeCheck() && lookFirstPersonTransform1Prop.objectReferenceValue != null)
                        {
                            if (!((Transform)lookFirstPersonTransform1Prop.objectReferenceValue).IsChildOf(stickyControlModule.transform))
                            {
                                lookFirstPersonTransform1Prop.objectReferenceValue = null;
                                Debug.LogWarning("For First Person, the look transform must be a child of the parent Sticky3D Controller gameobject or part of the prefab.");
                            }
                            // If the camera hasn't been setup, assume it is a child of the look transform
                            else if (lookFirstPersonCamera1Prop.objectReferenceValue == null)
                            {
                                // Find the first camera
                                lookFirstPersonCamera1Prop.objectReferenceValue = ((Transform)lookFirstPersonTransform1Prop.objectReferenceValue).GetComponentInChildren<Camera>();
                            }
                        }

                        EditorGUI.BeginChangeCheck();
                        GUILayout.BeginHorizontal();
                        #if UNITY_2019_1_OR_NEWER
                        EditorGUILayout.LabelField(lookCamera1FirstPersonContent, GUILayout.Width(defaultEditorLabelWidth - 53f));
                        #else
                        EditorGUILayout.LabelField(lookCamera1FirstPersonContent, GUILayout.Width(defaultEditorLabelWidth - 58f));
                        #endif
                        if (GUILayout.Button(lookCamera1FirstPersonNewContent, buttonCompact, GUILayout.MaxWidth(50f)) && lookFirstPersonCamera1Prop.objectReferenceValue == null)
                        {
                            serializedObject.ApplyModifiedProperties();

                            if (isLookVRProp.boolValue && stickyControlModule.IsLookVRAvailable)
                            {
                                #if SCSM_XR && SSC_UIS

                                Undo.SetCurrentGroupName("New XR First Person Camera");
                                int undoGroup = UnityEditor.Undo.GetCurrentGroup();

                                Undo.RecordObject(stickyControlModule, string.Empty);

                                GameObject camOffsetGameObject = new GameObject("XR Camera Offset");
                                GameObject camGameObject = new GameObject("XR Camera");

                                if (camOffsetGameObject != null && camGameObject != null)
                                {
                                    Undo.RegisterCreatedObjectUndo(camOffsetGameObject, string.Empty);
                                    Undo.RegisterCreatedObjectUndo(camGameObject, string.Empty);

                                    Camera _newCamera = Undo.AddComponent(camGameObject, typeof(Camera)) as Camera;
                                    _newCamera.tag = "MainCamera";
                                    stickyControlModule.lookFirstPersonCamera1 = _newCamera;

                                    // NOTE: We process XR HMD data ourselves as we have our own "rig".
                                    //// Distinguish between the old XR TrackPoseDriver and the one with the new Unity Input System.
                                    //var _tposeDriver = Undo.AddComponent(camGameObject, typeof(UnityEngine.InputSystem.XR.TrackedPoseDriver)) as UnityEngine.InputSystem.XR.TrackedPoseDriver;

                                    //if (_tposeDriver != null)
                                    //{
                                    //    _tposeDriver.trackingType = UnityEngine.InputSystem.XR.TrackedPoseDriver.TrackingType.RotationOnly;

                                    //    UnityEngine.InputSystem.InputAction _tposRotation = new UnityEngine.InputSystem.InputAction(null, UnityEngine.InputSystem.InputActionType.Value);
                                    //    if (_tposRotation != null)
                                    //    {
                                    //        _tposRotation.expectedControlType = "Quaternion";
                                    //        _tposeDriver.rotationAction = _tposRotation;
                                    //    }
                                    //}

                                    camOffsetGameObject.transform.SetParent(stickyControlModule.transform, false);
                                    camGameObject.transform.SetParent(camOffsetGameObject.transform, false);
                                    stickyControlModule.SetLookFirstPerson(_newCamera, camGameObject.transform, true);
                                    // Turn off auto camera height - assume this is controlled by the VR rig / pose driver.
                                    //stickyControlModule.isAutoFirstPersonCameraHeight = false;

                                    // Look for an existing AudioListener
                                    AudioListener _audioListener = stickyControlModule.GetComponentInChildren<AudioListener>();
                                    if (_audioListener == null)
                                    {
                                        // Didn't find one, so add an AudioListener now
                                        _audioListener = Undo.AddComponent(camGameObject, typeof(AudioListener)) as AudioListener;
                                    }

                                    Undo.CollapseUndoOperations(undoGroup);
                                }

                                #endif
                            }
                            else
                            {
                                // Create a new first person camera - assume not using UnityXR (VR)
                                Undo.SetCurrentGroupName("New First Person Camera");
                                int undoGroup = UnityEditor.Undo.GetCurrentGroup();

                                Undo.RecordObject(stickyControlModule, string.Empty);

                                GameObject camGameObject = new GameObject("Camera");
                                if (camGameObject != null)
                                {
                                    Undo.RegisterCreatedObjectUndo(camGameObject, string.Empty);
                                    Camera _newCamera = Undo.AddComponent(camGameObject, typeof(Camera)) as Camera;
                                    _newCamera.tag = "MainCamera";
                                    stickyControlModule.lookFirstPersonCamera1 = _newCamera;

                                    camGameObject.transform.SetParent(stickyControlModule.transform, false);
                                    stickyControlModule.SetLookFirstPerson(_newCamera, camGameObject.transform, true);
                                    stickyControlModule.isAutoFirstPersonCameraHeight = true;
                                    stickyControlModule.SetLookCameraHeight();

                                    // Look for an existing AudioListener
                                    AudioListener _audioListener = stickyControlModule.GetComponentInChildren<AudioListener>();
                                    if (_audioListener == null)
                                    {
                                        // Didn't find one, so add an AudioListener now
                                        _audioListener = Undo.AddComponent(camGameObject, typeof(AudioListener)) as AudioListener;
                                    }
                                }

                                Undo.CollapseUndoOperations(undoGroup);
                            }

                            // Should be non-scene objects but is required to force being set as dirty
                            EditorUtility.SetDirty(stickyControlModule);

                            GUIUtility.ExitGUI();

                            //serializedObject.Update();
                        }
                        EditorGUILayout.PropertyField(lookFirstPersonCamera1Prop, GUIContent.none);
                        GUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck() && lookFirstPersonCamera1Prop.objectReferenceValue != null)
                        {
                            if (!((Camera)lookFirstPersonCamera1Prop.objectReferenceValue).transform.IsChildOf(stickyControlModule.transform))
                            {
                                lookFirstPersonCamera1Prop.objectReferenceValue = null;
                                Debug.LogWarning("For First Person, the camera must be a child of the parent Sticky3D Controller gameobject or part of the prefab.");
                            }
                            // If look transform hasn't been setup up, assume it is the camera transform
                            else if (lookFirstPersonTransform1Prop.objectReferenceValue == null)
                            {
                                lookFirstPersonTransform1Prop.objectReferenceValue = ((Camera)lookFirstPersonCamera1Prop.objectReferenceValue).transform;
                            }
                        }

                        EditorGUILayout.PropertyField(isAutoFirstPersonCameraHeightProp, isAutoFirstPersonCameraHeightContent);
                        EditorGUILayout.PropertyField(isLookCameraFollowHeadProp, isLookCameraFollowHeadContent);
                        EditorGUILayout.PropertyField(lookHorizontalSpeedProp, lookHorizontalSpeedContent);
                        EditorGUILayout.PropertyField(lookHorizontalDampingProp, lookHorizontalDampingContent);
                        EditorGUILayout.PropertyField(lookVerticalSpeedProp, lookVerticalSpeedContent);
                        EditorGUILayout.PropertyField(lookVerticalDampingProp, lookVerticalDampingContent);
                        EditorGUILayout.PropertyField(lookPitchUpLimitProp, lookPitchUpLimitContent);
                        EditorGUILayout.PropertyField(lookPitchDownLimitProp, lookPitchDownLimitContent);

                        EditorGUILayout.PropertyField(lookAutoHideCursorProp, lookAutoHideCursorContent);
                        if (lookAutoHideCursorProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(lookHideCursorTimeProp, lookHideCursorTimeContent);
                        }

                        EditorGUILayout.PropertyField(lookZoomDurationProp, lookZoomDurationContent);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(lookUnzoomDelayProp, lookUnzoomDelayContent);
                        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                        {
                            stickyControlModule.SetLookUnzoomDelay(lookUnzoomDelayProp.floatValue);
                        }

                        EditorGUILayout.PropertyField(lookZoomedFoVProp, lookZoomedFoVContent);
                        EditorGUILayout.PropertyField(lookUnzoomedFoVProp, lookUnzoomedFoVContent);
                        EditorGUILayout.PropertyField(lookMaxLoSFoVProp, lookMaxLoSFoVContent);

                        if (!isLookVRProp.boolValue)
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(lookUpdateTypeProp, lookUpdateTypeContent);
                            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                            {
                                stickyControlModule.SetLookUpdateType((StickyControlModule.LookUpdateType)lookUpdateTypeProp.intValue);
                            }
                        }
                    }
                    #endregion
                }

                #region Clip Objects
                if (isThirdPersonProp.boolValue)
                {                  
                    StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

                    GUILayout.BeginHorizontal();
                    StickyEditorHelper.DrawS3DFoldout(isLookClipObjectsExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                    #if UNITY_2019_3_OR_NEWER
                    EditorGUILayout.LabelField(clipObjectsContent, GUILayout.Width(155f));
                    #else
                    EditorGUILayout.LabelField(clipObjectsContent, GUILayout.Width(152f));
                    #endif
                    EditorGUILayout.PropertyField(clipObjectsProp, GUIContent.none);
                    GUILayout.EndHorizontal();

                    if (clipObjectsProp.boolValue && isLookClipObjectsExpandedProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(minClipMoveSpeedProp, minClipMoveSpeedContent);
                        EditorGUILayout.PropertyField(clipMinDistanceProp, clipMinDistanceContent);
                        EditorGUILayout.PropertyField(clipMinOffsetXProp, clipMinOffsetXContent);
                        EditorGUILayout.PropertyField(clipMinOffsetYProp, clipMinOffsetYContent);
                        EditorGUILayout.PropertyField(clipResponsivenessProp, clipResponsivenessContent);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(clipObjectMaskContent, GUILayout.Width(defaultEditorLabelWidth - 58f));
                        if (GUILayout.Button(StickyEditorHelper.resetBtnContent, buttonCompact, GUILayout.MaxWidth(50f)))
                        {
                            serializedObject.ApplyModifiedProperties();
                            Undo.RecordObject(stickyControlModule, "Reset Clip Mask");
                            stickyControlModule.ResetClipObjectMask();
                            GUIUtility.ExitGUI();
                            return;
                        }
                        EditorGUILayout.PropertyField(clipObjectMaskProp, GUIContent.none);
                        EditorGUILayout.EndHorizontal();

                        // When first added or if user attempts to set to Nothing, reset to defaults.
                        if (clipObjectMaskProp.intValue == 0)
                        {
                            serializedObject.ApplyModifiedProperties();
                            Undo.RecordObject(stickyControlModule, "Reset Clip Mask");
                            stickyControlModule.ResetClipObjectMask();
                            GUIUtility.ExitGUI();
                            return;
                        }
                    }
                }
                #endregion

                DrawLookInteractiveSettings();

                #region Look VR
                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

                GUILayout.BeginHorizontal();
                StickyEditorHelper.DrawS3DFoldout(isLookVRExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                #if UNITY_2019_3_OR_NEWER
                EditorGUILayout.LabelField(isLookVRContent, GUILayout.Width(155f));
                #else
                EditorGUILayout.LabelField(isLookVRContent, GUILayout.Width(152f));
                #endif
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(isLookVRProp, GUIContent.none);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyControlModule.SetLookVR(isLookVRProp.boolValue);
                }
                GUILayout.EndHorizontal();

                if (isLookVRProp.boolValue && isLookVRExpandedProp.boolValue)
                {
                    if (!stickyControlModule.IsLookVRAvailable)
                    {
                        EditorGUILayout.HelpBox("VR requires Unity XR to be setup for a first-person, non-NPC player. See the Sticky Input Module section of the manual for details", MessageType.Error);
                    }
                    else
                    {
                        if (!isFreeLookProp.boolValue)
                        {
                            EditorGUILayout.HelpBox("VR requires first person Free Look", MessageType.Error);
                        }

                        StickyEditorHelper.InTechPreview(true);
                    }

                    EditorGUILayout.PropertyField(isMatchHumanHeightVRProp, isMatchHumanHeightVRContent);

                    if (!isMatchHumanHeightVRProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(humanPostureVRProp, humanPostureVRContent);
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(isRoomScaleVRProp, isRoomScaleVRContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        // If SnapTurnVR is enabled, turn it off
                        if (isRoomScaleVRProp.boolValue && isSnapTurnVRProp.boolValue)
                        { 
                            if (EditorApplication.isPlaying)
                            {
                                serializedObject.ApplyModifiedProperties();
                                stickyControlModule.DisableSnapTurnVR();
                                serializedObject.Update();
                            }
                            else
                            {
                                isSnapTurnVRProp.boolValue = false;
                            }
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(isSnapTurnVRProp, isSnapTurnVRContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (EditorApplication.isPlaying)
                        {
                            if (isSnapTurnVRProp.boolValue) { stickyControlModule.EnableSnapTurnVR(); }
                            else { stickyControlModule.DisableSnapTurnVR(); }
                        }
                        
                        // If Room Scale VR is on, turn it off
                        if (isSnapTurnVRProp.boolValue && isRoomScaleVRProp.boolValue) { isRoomScaleVRProp.boolValue = false; }
                    }

                    if (isSnapTurnVRProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(snapTurnDegreesProp, snapTurnDegreesContent);
                        EditorGUILayout.PropertyField(snapTurnIntervalTimeProp, snapTurnIntervalTimeContent); 
                    }
                }

                #endregion

                DrawLookSocketsSettings();
                //GUILayoutUtility.GetRect(1f, 2f);

                GUILayout.EndVertical();
            }
            #endregion

            #region Collide tab
            else if (selectedTabIntProp.intValue == 2)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(heightProp, heightContent);
                if (EditorGUI.EndChangeCheck() && heightProp.floatValue < 0.01f)
                {
                    // Prevent height being 0, -ve, or small value
                    heightProp.floatValue = 0.01f;
                }
                EditorGUILayout.PropertyField(radiusProp, radiusContent);
                EditorGUILayout.PropertyField(pivotToCentreOffsetYProp, pivotToCentreOffsetYContent);

                #region Shoulder Height
                GUILayout.BeginHorizontal();
                #if UNITY_2019_1_OR_NEWER
                EditorGUILayout.LabelField(shoulderHeightContent, GUILayout.Width(defaultEditorLabelWidth - 53f));
                #else
                EditorGUILayout.LabelField(shoulderHeightContent, GUILayout.Width(defaultEditorLabelWidth - 58f));
                #endif
                if (GUILayout.Button(shoulderHeightCalcContent, buttonCompact, GUILayout.Width(50f)))
                {
                    GUI.FocusControl(null);
                    shoulderHeightProp.floatValue = stickyControlModule.CalculateShoulderHeight();
                }
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(shoulderHeightProp, GUIContent.none);
                if (EditorGUI.EndChangeCheck() && shoulderHeightProp.floatValue < 0.01f)
                {
                    // Prevent shoulder height being 0, -ve, or small value
                    shoulderHeightProp.floatValue = 0.01f;
                }
                GUILayout.EndHorizontal();
                #endregion

                // Let user adjust crouch height in metres
                crouchHeightNormalisedProp.floatValue = EditorGUILayout.Slider(crouchHeightNormalisedContent, crouchHeightNormalisedProp.floatValue * heightProp.floatValue, heightProp.floatValue * 0.1f, heightProp.floatValue * 0.9f) / heightProp.floatValue;
                EditorGUILayout.PropertyField(maxSweepIterationsProp, maxSweepIterationsContent);
                EditorGUILayout.PropertyField(sweepToleranceProp, sweepToleranceContent);
                EditorGUILayout.PropertyField(collisionLayerMaskProp, collisionLayerMaskContent);
                EditorGUILayout.PropertyField(interactionLayerMaskProp, interactionLayerMaskContent);
                EditorGUILayout.PropertyField(isOnTriggerEnterEnabledProp, isOnTriggerEnterContent);
                EditorGUILayout.PropertyField(isOnTriggerStayEnabledProp, isOnTriggerStayContent);
                EditorGUILayout.PropertyField(isTriggerColliderEnabledProp, isTriggerColliderEnabledContent);
                EditorGUILayout.PropertyField(isReactToStickyZonesEnabledProp, isReactToStickyZonesEnabledContent);

                #region Reference Frame
                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
                EditorGUILayout.PropertyField(referenceFrameLayerMaskProp, referenceFrameLayerMaskContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(referenceUpdateTypeProp, referenceUpdateTypeContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyControlModule.SetReferenceUpdateType((StickyControlModule.ReferenceUpdateType)referenceUpdateTypeProp.intValue);
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("initialReferenceFrame"), initialReferenceFrameContent);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isUseRBodyReferenceFrame"), isUseRBodyReferenceFrameContent);

                EditorGUILayout.EndVertical();

                // When first added or if user attempts to set to Nothing, reset to defaults.
                if (referenceFrameLayerMaskProp.intValue == 0)
                {
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(stickyControlModule, "Reset Reference Layer Mask");
                    stickyControlModule.ResetReferenceFrameLayerMask();
                    GUIUtility.ExitGUI();
                    return;
                }
                #endregion
            }
            #endregion

            #region Jet Pack tab
            else if (selectedTabIntProp.intValue == 3)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.PropertyField(isJetPackAvailableProp, isJetPackAvailableContent);
                EditorGUILayout.PropertyField(isJetPackEnabledProp, isJetPackEnabledContent);
                EditorGUILayout.PropertyField(jetPackFuelLevelProp, jetPackFuelLevelContent);
                EditorGUILayout.PropertyField(jetPackFuelBurnRateProp, jetPackFuelBurnRateContent);
                EditorGUILayout.PropertyField(jetPackSpeedProp, jetPackSpeedContent);
                EditorGUILayout.PropertyField(jetPackMaxAccelerationProp, jetPackMaxAccelerationContent);
                EditorGUILayout.PropertyField(jetPackDampingProp, jetPackDampingContent);
                EditorGUILayout.PropertyField(jetPackRampUpDurationProp, jetPackRampUpDurationContent);
                EditorGUILayout.PropertyField(jetPackRampDownDurationProp, jetPackRampDownDurationContent);

                #region JetPack AudioSource
                GUILayout.BeginHorizontal();
                #if UNITY_2019_1_OR_NEWER
                EditorGUILayout.LabelField(jetPackAudioContent, GUILayout.Width(defaultEditorLabelWidth - 53f));
                #else
                EditorGUILayout.LabelField(jetPackAudioContent, GUILayout.Width(defaultEditorLabelWidth - 58f));
                #endif

                if (GUILayout.Button(jetPackNewAudioContent, buttonCompact, GUILayout.Width(50f)) && jetPackAudioProp.objectReferenceValue == null)
                {
                    serializedObject.ApplyModifiedProperties();

                    Undo.SetCurrentGroupName("New Audio Source");
                    int undoGroup = UnityEditor.Undo.GetCurrentGroup();

                    Undo.RecordObject(stickyControlModule, string.Empty);

                    GameObject audioGameObject = new GameObject("JetPack");
                    if (audioGameObject != null)
                    {
                        Undo.RegisterCreatedObjectUndo(audioGameObject, string.Empty);
                        AudioSource _newAudioSource = Undo.AddComponent(audioGameObject, typeof(AudioSource)) as AudioSource;
                        _newAudioSource.playOnAwake = false;
                        _newAudioSource.maxDistance = 25f;
                        _newAudioSource.spatialBlend = 1f;
                        _newAudioSource.loop = true;
                        _newAudioSource.volume = 0.5f;
                        stickyControlModule.jetPackAudio = _newAudioSource;

                        audioGameObject.transform.SetParent(stickyControlModule.transform, false);
                        // Assumes height, radius and pivotToCentreOffsetY have already been set
                        // Places the jetpack audio at 75% of character height
                        audioGameObject.transform.localPosition += audioGameObject.transform.up * (stickyControlModule.pivotToCentreOffsetY + stickyControlModule.height * 0.25f);
                        // Place it slightly behind the character
                        audioGameObject.transform.localPosition += audioGameObject.transform.forward * (stickyControlModule.radius * -1.1f);
                    }

                    Undo.CollapseUndoOperations(undoGroup);

                    // Should be non-scene objects but is required to force being set as dirty
                    EditorUtility.SetDirty(stickyControlModule);

                    GUIUtility.ExitGUI();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(jetPackAudioProp, GUIContent.none);
                if (EditorGUI.EndChangeCheck() && jetPackAudioProp.objectReferenceValue != null)
                {
                    if (!((AudioSource)jetPackAudioProp.objectReferenceValue).transform.IsChildOf(stickyControlModule.transform))
                    {
                        jetPackAudioProp.objectReferenceValue = null;
                        Debug.LogWarning("The jet pack audio source transform must be a child of the parent Sticky3D Controller gameobject or part of the prefab.");
                    }
                }

                GUILayout.EndHorizontal();
                #endregion

                // health is stored internally as 0-1 value
                jetPackHealthProp.floatValue = EditorGUILayout.Slider(jetPackHealthContent, jetPackHealthProp.floatValue * 100f, 0f, 100f) / 100f;

                EditorGUI.indentLevel += 1;
                isJetPackThrusterListExpandedProp.boolValue = EditorGUILayout.Foldout(isJetPackThrusterListExpandedProp.boolValue, jetPackThrustersContent);
                EditorGUI.indentLevel -= 1;

                if (isJetPackThrusterListExpandedProp.boolValue)
                {
                    EditorGUILayout.PropertyField(jetPackMinEffectsRateProp, jetPackMinEffectsRateContent);

                    DrawJetPackThrusterEffect(jetPackThrusterFwdProp, jetPackThrusterFwdContent);
                    DrawJetPackThrusterEffect(jetPackThrusterBackProp, jetPackThrusterBackContent);
                    DrawJetPackThrusterEffect(jetPackThrusterUpProp, jetPackThrusterUpContent);
                    DrawJetPackThrusterEffect(jetPackThrusterDownProp, jetPackThrusterDownContent);
                    DrawJetPackThrusterEffect(jetPackThrusterRightProp, jetPackThrusterRightContent);
                    DrawJetPackThrusterEffect(jetPackThrusterLeftProp, jetPackThrusterLeftContent);
                }

                EditorGUILayout.EndVertical();
            }

            #endregion

            #region Animate tab
            else if (selectedTabIntProp.intValue == 4)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                #region Animate General
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(defaultAnimatorProp, defaultAnimatorContent);
                if (EditorGUI.EndChangeCheck())
                {
                    RefreshAnimatorParameters();
                    //RefreshAnimatorTransitions();
                }
                #endregion

                StickyEditorHelper.DrawUILine(separatorColor);

                if (defaultAnimatorProp.objectReferenceValue != null)
                {
                    #region Aim IK

                    StickyEditorHelper.DrawS3DFoldout(isAimIKExpandedProp, aimIKContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

                    if (isAimIKExpandedProp.boolValue)
                    {
                        StickyEditorHelper.InTechPreview(true);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(isAimIKWhenNotAimingProp, isAimIKWhenNotAimingContent);
                        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                        {
                            serializedObject.ApplyModifiedProperties();
                            stickyControlModule.SetAimIKWhenNotAiming(isAimIKWhenNotAimingProp.boolValue);
                            serializedObject.Update();
                        }

                        EditorGUILayout.PropertyField(aimIKCameraOffsetTPSProp, aimIKCameraOffsetTPSContent);
                        EditorGUILayout.PropertyField(aimIKAnimIKPassLayerIndexProp, aimIKAnimIKPassLayerIndexContent);
                        EditorGUILayout.PropertyField(aimIKDownLimitProp, aimIKDownLimitContent);
                        EditorGUILayout.PropertyField(aimIKUpLimitProp, aimIKUpLimitContent);
                        EditorGUILayout.PropertyField(aimIKLeftLimitProp, aimIKLeftLimitContent);
                        EditorGUILayout.PropertyField(aimIKRightLimitProp, aimIKRightLimitContent);
                        EditorGUILayout.PropertyField(aimIkTurnRotationRateProp, aimIkTurnRotationRateContent);
                        EditorGUILayout.PropertyField(aimIKBoneWeightFPSProp, aimIKBoneWeightFPSContent);

                        int _prevArraySize = aimBonesProp.arraySize;
                        StickyEditorHelper.DrawHorizontalGap(2f);
                        StickyEditorHelper.DrawArray(aimBonesProp, isAimBonesExpandedProp, aimIKBonesContent, 70f, "Bone", buttonCompact, foldoutStyleNoLabel, defaultEditorFieldWidth);

                        if (aimBonesProp.arraySize > _prevArraySize)
                        {
                            serializedObject.ApplyModifiedProperties();
                            stickyControlModule.AimBones[aimBonesProp.arraySize - 1].SetClassDefaults();
                            serializedObject.Update();
                        }
                        else if (aimBonesProp.arraySize < _prevArraySize)
                        {
                            serializedObject.ApplyModifiedProperties();
                            stickyControlModule.RefreshAimBoneTransforms();
                            serializedObject.Update();
                        }

                        StickyEditorHelper.DrawInformationLabel(new GUIContent("NOTE: Shoulder bones will have no effect with First Person aiming"), 15f);
                    }

                    StickyEditorHelper.DrawHorizontalGap(2f);

                    #endregion

                    StickyEditorHelper.DrawUILine(separatorColor);

                    #region Head IK
                    GUILayout.BeginHorizontal();
                    StickyEditorHelper.DrawS3DFoldout(isHeadIKExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                    GUILayout.Label(isHeadIKContent, GUILayout.Width(defaultEditorLabelWidth - 22f));
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(isHeadIKProp, GUIContent.none);
                    GUILayout.EndHorizontal();
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();

                        if (EditorApplication.isPlaying)
                        {
                            // Toggle isHeadIK so Enable/DisableHeadIK can detect the change.
                            stickyControlModule.EditorInternalOnlyHeadIK();

                            if (isHeadIKProp.boolValue)
                            {
                                stickyControlModule.EnableHeadIK(true);
                            }
                            else
                            {
                                stickyControlModule.DisableHeadIK(true);
                            }
                        }
                        else
                        {
                            stickyControlModule.IKCheckEnabler();
                        }
                        serializedObject.Update();
                    }

                    if (isHeadIKExpandedProp.boolValue && isHeadIKProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(headIKAnimIKPassLayerIndexProp, headIKAnimIKPassLayerIndexContent);
                        EditorGUILayout.PropertyField(headIKMoveMaxSpeedProp, headIKMoveMaxSpeedContent);
                        EditorGUILayout.PropertyField(headIKMoveDampingProp, headIKMoveDampingContent);
                        EditorGUILayout.PropertyField(headIKLookDownLimitProp, headIKLookDownLimitContent);
                        EditorGUILayout.PropertyField(headIKLookUpLimitProp, headIKLookUpLimitContent);
                        EditorGUILayout.PropertyField(headIKLookLeftLimitProp, headIKLookLeftLimitContent);
                        EditorGUILayout.PropertyField(headIKLookRightLimitProp, headIKLookRightLimitContent);
                        EditorGUILayout.PropertyField(headIKEyesWeightProp, headIKEyesWeightContent);
                        EditorGUILayout.PropertyField(headIKHeadWeightProp, headIKHeadWeightContent);
                        EditorGUILayout.PropertyField(headIKBodyWeightProp, headIKBodyWeightContent);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(headIKLookAtEyesProp, headIKLookAtEyesContent);
                        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                        {
                            stickyControlModule.SetHeadIKLookAtEyes(headIKLookAtEyesProp.boolValue);
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(headIKLookAtInteractiveProp, headIKLookAtInteractiveContent);
                        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                        {
                            stickyControlModule.SetHeadIKLookAtInteractive(headIKLookAtInteractiveProp.boolValue);
                        }

                        EditorGUILayout.PropertyField(headIKAdjustForVelocityProp, headIKAdjustForVelocityContent);
                        EditorGUILayout.PropertyField(headIKWhenClimbingProp, headIKWhenClimbingContent);
                        EditorGUILayout.PropertyField(headIKWhenMovementDisabledProp, headIKWhenMovementDisabledContent);
                        EditorGUILayout.PropertyField(headIKConsiderBehindProp, headIKConsiderBehindContent);
                    }

                    #endregion

                    StickyEditorHelper.DrawUILine(separatorColor);

                    #region Hand IK
                    GUILayout.BeginHorizontal();
                    StickyEditorHelper.DrawS3DFoldout(isHandIKExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                    GUILayout.Label(isHandIKContent, GUILayout.Width(defaultEditorLabelWidth - 22f));
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(isHandIKProp, GUIContent.none);
                    GUILayout.EndHorizontal();
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();

                        if (EditorApplication.isPlaying)
                        {
                            // Toggle isHandIK so Enable/DisableHandIK can detect the change.
                            stickyControlModule.EditorInternalOnlyHandIK();

                            if (isHandIKProp.boolValue)
                            {
                                stickyControlModule.EnableHandIK(true);
                            }
                            else
                            {
                                stickyControlModule.DisableHandIK(true);
                            }
                        }
                        else
                        {
                            stickyControlModule.IKCheckEnabler();

                            if (isHandIKProp.boolValue)
                            {
                                stickyControlModule.InitialiseEditorEssentials();
                            }
                        }
                        serializedObject.Update();
                    }

                    if (isHandIKExpandedProp.boolValue && isHandIKProp.boolValue)
                    {
                        StickyEditorHelper.InTechPreview(false);

                        EditorGUILayout.PropertyField(handIKAnimIKPassLayerIndexProp, handIKAnimIKPassLayerIndexContent);
                        EditorGUILayout.PropertyField(handIKMoveMaxSpeedProp, handIKMoveMaxSpeedContent);

                        EditorGUILayout.PropertyField(leftHandRadiusProp, handIKLeftRadiusContent);

                        EditorGUILayout.PropertyField(handZoneGizmoColourProp, handIKZoneColourContent);

                        EditorGUILayout.PropertyField(handIKWhenMovementDisabledProp, handIKWhenMovementDisabledContent);

                        #region Left Hand
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(" ", GUILayout.Width(1f));
                        StickyEditorHelper.DrawS3DFoldout(isLeftHandIKExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                        GUILayout.Label(isHandIKLeftContent);
                        // Find (select) in the scene
                        SelectItemInSceneViewButton(ref isLeftHandSelected, showLHGizmosInSceneViewProp);
                        // Toggle selection in scene view on/off
                        StickyEditorHelper.DrawGizmosButton(showLHGizmosInSceneViewProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled);
                        GUILayout.EndHorizontal();

                        if (isLeftHandIKExpandedProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(leftHandPalmOffsetProp, leftHandPalmOffsetContent);
                            EditorGUILayout.PropertyField(leftHandPalmRotationProp, leftHandPalmRotationContent);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("leftHandConstraintSettings"), handIKLeftHandSettingsContent);
                            EditorGUILayout.PropertyField(handIKLHMaxInRotProp, handIKLHMaxInRotContent);
                            EditorGUILayout.PropertyField(handIKLHMaxOutRotProp, handIKLHMaxOutRotContent);
                            EditorGUILayout.PropertyField(handIKLHMaxUpRotProp, handIKLHMaxUpRotContent);
                            EditorGUILayout.PropertyField(handIKLHMaxDownRotProp, handIKLHMaxDownRotContent);
                            EditorGUILayout.PropertyField(handIKLHInwardLimitProp, handIKLHInwardLimitContent);
                            EditorGUILayout.PropertyField(handIKLHOutwardLimitProp, handIKLHOutwardLimitContent);

                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(handIKLHMaxReachDistProp, handIKLHMaxReachDistContent);
                            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                            {
                                stickyControlModule.SetLeftHandMaxReachDistance(handIKLHMaxReachDistProp.floatValue);
                            }

                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(handIKLHElbowHintProp, handIKLHElbowHintContent);
                            if (EditorGUI.EndChangeCheck())
                            {
                                serializedObject.ApplyModifiedProperties();
                                stickyControlModule.SetLeftHandElbowHint((Transform)handIKLHElbowHintProp.objectReferenceValue);
                                serializedObject.Update();
                            }
                        }
                        #endregion

                        #region Right Hand
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(" ", GUILayout.Width(1f));
                        StickyEditorHelper.DrawS3DFoldout(isRightHandIKExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                        GUILayout.Label(isHandIKRightContent);
                        // Find (select) in the scene
                        SelectItemInSceneViewButton(ref isRightHandSelected, showRHGizmosInSceneViewProp);
                        // Toggle selection in scene view on/off
                        StickyEditorHelper.DrawGizmosButton(showRHGizmosInSceneViewProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled);
                        GUILayout.EndHorizontal();

                        if (isRightHandIKExpandedProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(rightHandPalmOffsetProp, rightHandPalmOffsetContent);
                            EditorGUILayout.PropertyField(rightHandPalmRotationProp, rightHandPalmRotationContent);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("rightHandConstraintSettings"), handIKRightHandSettingsContent);
                            EditorGUILayout.PropertyField(handIKRHMaxInRotProp, handIKRHMaxInRotContent);
                            EditorGUILayout.PropertyField(handIKRHMaxOutRotProp, handIKRHMaxOutRotContent);
                            EditorGUILayout.PropertyField(handIKRHMaxUpRotProp, handIKRHMaxUpRotContent);
                            EditorGUILayout.PropertyField(handIKRHMaxDownRotProp, handIKRHMaxDownRotContent);
                            EditorGUILayout.PropertyField(handIKRHInwardLimitProp, handIKRHInwardLimitContent);
                            EditorGUILayout.PropertyField(handIKRHOutwardLimitProp, handIKRHOutwardLimitContent);

                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(handIKRHMaxReachDistProp, handIKRHMaxReachDistContent);
                            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                            {
                                stickyControlModule.SetRightHandMaxReachDistance(handIKRHMaxReachDistProp.floatValue);
                            }

                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(handIKRHElbowHintProp, handIKRHElbowHintContent);
                            if (EditorGUI.EndChangeCheck())
                            {
                                serializedObject.ApplyModifiedProperties();
                                stickyControlModule.SetRightHandElbowHint((Transform)handIKRHElbowHintProp.objectReferenceValue);
                                serializedObject.Update();
                            }
                        }
                        #endregion
                    }

                    #endregion

                    StickyEditorHelper.DrawUILine(separatorColor);

                    #region Foot IK
                    GUILayout.BeginHorizontal();
                    StickyEditorHelper.DrawS3DFoldout(isFootIKExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                    GUILayout.Label(isFootIKContent, GUILayout.Width(defaultEditorLabelWidth-22f));
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(isFootIKProp, GUIContent.none);
                    GUILayout.EndHorizontal();
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();

                        if (EditorApplication.isPlaying)
                        {
                            // Toggle isFootIK so Enable/DisableFootIK can detect the change.
                            stickyControlModule.EditorInternalOnlyFootIK();

                            if (isFootIKProp.boolValue)
                            {
                                //Undo.RecordObject(stickyControlModule, "Enable Foot IK");
                                stickyControlModule.EnableFootIK();
                            }
                            else
                            {
                                //Undo.RecordObject(stickyControlModule, "Diable Foot IK");
                                stickyControlModule.DisableFootIK();
                            }
                        }
                        else
                        {
                            stickyControlModule.IKCheckEnabler();
                        }
                        serializedObject.Update();
                    }

                    if (isFootIKExpandedProp.boolValue && isFootIKProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(footIKAnimIKPassLayerIndexProp, footIKAnimIKPassLayerIndexContent);
                        EditorGUILayout.PropertyField(footIKBodyMoveSpeedYProp, footIKBodyMoveSpeedYContent);
                        EditorGUILayout.PropertyField(footIKPositionOnlyProp, footIKPositionOnlyContent);
                        if (!footIKPositionOnlyProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(footIKMaxInwardRotationXProp, footIKMaxInwardRotationXContent);
                            EditorGUILayout.PropertyField(footIKMaxOutwardRotationXProp, footIKMaxOutwardRotationXContent);
                            EditorGUILayout.PropertyField(footIKMaxPitchZProp, footIKMaxPitchZContent);
                            EditorGUILayout.PropertyField(footIKSlopeReactRateProp, footIKSlopeReactRateContent);
                            EditorGUILayout.PropertyField(footIKSlopeToleranceProp, footIKSlopeToleranceContent);
                            EditorGUILayout.PropertyField(footIKToeDistanceProp, footIKToeDistanceContent);
                        }
                        EditorGUILayout.LabelField(footIKCurveParmsContent);
                        DrawParameterSelection(animParamsFloatList, animParamFloatNames, paramHashLFootIKWeightCurveProp, footIKLCurveParmContent);
                        DrawParameterSelection(animParamsFloatList, animParamFloatNames, paramHashRFootIKWeightCurveProp, footIKRCurveParmContent);
                    }
                    #endregion

                    StickyEditorHelper.DrawUILine(separatorColor);

                    #region Ragdoll

                    StickyEditorHelper.DrawS3DFoldout(isRagdollExpandedProp, ragdollContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

                    if (isRagdollExpandedProp.boolValue)
                    {
                        StickyEditorHelper.InTechPreview(true);

                        if (!isHumanoid)
                        {
                            EditorGUILayout.HelpBox("This feature only applies to characters with a Humanoid rig and Animator", MessageType.Error);
                        }

                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button(ragdollGetBonesContent, buttonCompact))
                        {                            
                            GetRagdollBones();
                        }

                        if (GUILayout.Button(ragdollGenerateContent, buttonCompact))
                        {
                            CreateRagdoll();
                        }

                        if (GUILayout.Button(ragdollRemoveContent, buttonCompact))
                        {
                            RemoveRagdoll();
                        }

                        if (GUILayout.Button(ragdollTestContent, buttonCompact))
                        {
                            if (EditorApplication.isPlaying)
                            {
                                stickyControlModule.IsRagdoll = !stickyControlModule.IsRagdoll;
                            }
                            else
                            {
                                StickyEditorHelper.PromptGotIt("Ragdoll", "To test the ragdoll, you need to be in play mode");
                            }
                        }

                        GUILayout.EndHorizontal();

                        StickyEditorHelper.DrawHorizontalGap(2f);

                        #region Add-Remove Ragdoll Bones
                        int numRagdollBones = ragdollBoneListProp.arraySize;
                        // Reset button variables
                        s3dRagdollBoneMoveDownPos = -1;
                        s3dRagdollBoneInsertPos = -1;
                        s3dRagdollBoneDeletePos = -1;

                        GUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField("<color=" + txtColourName + ">Ragdoll Bones: " + numRagdollBones.ToString("00") + "</color>", labelFieldRichText);

                        if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                        {
                            // Apply property changes
                            serializedObject.ApplyModifiedProperties();
                            Undo.RecordObject(stickyControlModule, "Add Ragdoll Bone");
                            if (stickyControlModule.RagdollBoneList == null) { stickyControlModule.RagdollBoneList = new List<S3DHumanBonePersist>(); }
                            stickyControlModule.RagdollBoneList.Add(new S3DHumanBonePersist());
                            isSceneModified = true;
                            // Read in the properties
                            serializedObject.Update();

                            numRagdollBones = ragdollBoneListProp.arraySize;
                            if (numRagdollBones > 0)
                            {
                                // Force new bone to be serialized in scene
                                s3dRagdollBoneProp = ragdollBoneListProp.GetArrayElementAtIndex(numRagdollBones - 1);
                                rdBoneGUIDHashProp = s3dRagdollBoneProp.FindPropertyRelative("guidHash");
                                int hashValue = rdBoneGUIDHashProp.intValue;
                                rdBoneGUIDHashProp.intValue = 0;
                                rdBoneGUIDHashProp.intValue = hashValue;
                            }
                        }
                        if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                        {
                            if (numRagdollBones > 0) { s3dRagdollBoneDeletePos = ragdollBoneListProp.arraySize - 1; }
                        }

                        GUILayout.EndHorizontal();

                        #endregion

                        #region Ragdoll Bone List                       

                        for (int rbIdx = 0; rbIdx < numRagdollBones; rbIdx++)
                        {
                            DrawRagdollBone(ragdollBoneListProp.GetArrayElementAtIndex(rbIdx), rbIdx, numRagdollBones, ref s3dRagdollBoneMoveDownPos, ref s3dRagdollBoneInsertPos, ref s3dRagdollBoneDeletePos);
                        }

                        #endregion

                        #region Move/Insert/Delete Bones

                        if (s3dRagdollBoneDeletePos >= 0 || s3dRagdollBoneInsertPos >= 0 || s3dRagdollBoneMoveDownPos >= 0)
                        {
                            GUI.FocusControl(null);
                            // Don't permit multiple operations in the same pass
                            if (s3dRagdollBoneMoveDownPos >= 0)
                            {
                                // Move down one position, or wrap round to start of list
                                if (s3dRagdollBoneMoveDownPos < ragdollBoneListProp.arraySize - 1)
                                {
                                    ragdollBoneListProp.MoveArrayElement(s3dRagdollBoneMoveDownPos, s3dRagdollBoneMoveDownPos + 1);
                                }
                                else { ragdollBoneListProp.MoveArrayElement(s3dRagdollBoneMoveDownPos, 0); }

                                s3dRagdollBoneMoveDownPos = -1;
                            }
                            else if (s3dRagdollBoneInsertPos >= 0)
                            {
                                // NOTE: Undo doesn't work with Insert.

                                // Apply property changes before potential list changes
                                serializedObject.ApplyModifiedProperties();

                                S3DHumanBonePersist insertedRagdollBone = new S3DHumanBonePersist(stickyControlModule.RagdollBoneList[s3dRagdollBoneInsertPos]);
                                // Generate a new hashcode for the duplicated RagdollBone
                                insertedRagdollBone.guidHash = S3DMath.GetHashCodeFromGuid();

                                stickyControlModule.RagdollBoneList.Insert(s3dRagdollBoneInsertPos, insertedRagdollBone);

                                // Read all properties from the Sticky Controller
                                serializedObject.Update();
                                rdBoneGUIDHashProp = ragdollBoneListProp.GetArrayElementAtIndex(s3dRagdollBoneInsertPos).FindPropertyRelative("guidHash");

                                // Force new action to be serialized in scene
                                int hashValue = rdBoneGUIDHashProp.intValue;
                                rdBoneGUIDHashProp.intValue = 0;
                                rdBoneGUIDHashProp.intValue = hashValue;

                                s3dRagdollBoneInsertPos = -1;

                                isSceneModified = true;
                            }
                            else if (s3dRagdollBoneDeletePos >= 0)
                            {
                                // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and DeletePos is reset to -1.
                                int _deleteIndex = s3dRagdollBoneDeletePos;

                                if (EditorUtility.DisplayDialog("Delete Ragdoll Bone " + (s3dRagdollBoneDeletePos + 1) + "?", "Ragdoll Bone " + (s3dRagdollBoneDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the ragdoll bone from the list and cannot be undone.", "Delete Now", "Cancel"))
                                {
                                    ragdollBoneListProp.DeleteArrayElementAtIndex(_deleteIndex);
                                    s3dRagdollBoneDeletePos = -1;
                                }
                            }

                            serializedObject.ApplyModifiedProperties();
                            // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                            if (!Application.isPlaying)
                            {
                                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                            }
                            GUIUtility.ExitGUI();
                        }

                        #endregion
                    }

                    #endregion

                    StickyEditorHelper.DrawUILine(separatorColor);

                    #region Root Motion

                    GUILayout.BeginHorizontal();
                    StickyEditorHelper.DrawS3DFoldout(isRootMotionExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                    GUILayout.Label(isRootMotionContent, GUILayout.Width(defaultEditorLabelWidth - 22f));
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(isRootMotionProp, GUIContent.none);
                    GUILayout.EndHorizontal();
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();

                        if (isRootMotionProp.boolValue)
                        {
                            stickyControlModule.EnableRootMotion();
                        }
                        else
                        {
                            stickyControlModule.DisableRootMotion();
                        }

                        serializedObject.Update();
                    }

                    if (isRootMotionExpandedProp.boolValue && isRootMotionProp.boolValue)
                    {
                        StickyEditorHelper.InTechPreview(false);

                        EditorGUILayout.PropertyField(isRootMotionTurningProp, isRootMotionTurningContent);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(rootMotionIdleThresholdProp, rootMotionIdleThresholdContent);
                        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                        {
                            serializedObject.ApplyModifiedProperties();
                            stickyControlModule.SetRootMotionIdleThreshold(rootMotionIdleThresholdProp.floatValue);
                            serializedObject.Update();
                        }

                        // Warn user if MoveUpdateType isn't FixedUpdate
                        if (!stickyControlModule.IsMoveFixedUpdate)
                        {
                            EditorGUILayout.HelpBox("Root Motion requires movement to be done in Fixed Update (See Move tab)", MessageType.Warning);
                        }
                    }

                    #endregion
                }

                #region Hand VR

                StickyEditorHelper.DrawUILine(separatorColor);

                GUILayout.BeginHorizontal();
                StickyEditorHelper.DrawS3DFoldout(isHandVRExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                GUILayout.Label(isHandVRContent, GUILayout.Width(defaultEditorLabelWidth - 22f));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(isHandVRProp, GUIContent.none);
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();

                    if (EditorApplication.isPlaying)
                    {
                        // Toggle isHandVR so Enable/DisableHandVR can detect the change.
                        stickyControlModule.EditorInternalOnlyHandVR();

                        if (isHandVRProp.boolValue)
                        {
                            stickyControlModule.EnableHandVR();
                        }
                        else
                        {
                            stickyControlModule.DisableHandVR();
                        }
                    }
                    else
                    {
                        if (isHandVRProp.boolValue)
                        {
                            stickyControlModule.InitialiseEditorEssentials();
                        }
                    }
                    serializedObject.Update();
                }

                if (isHandVRExpandedProp.boolValue && isHandVRProp.boolValue)
                {
                    #if SCSM_XR && SSC_UIS
                    
                    #else
                    EditorGUILayout.HelpBox("Hand VR requires Unity 2020.3+ and Unity XR - See Help for details", MessageType.Error);
                    #endif

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(handVRLHAnimatorProp, handVRLHAnimatorContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        RefreshVRAnimatorParameters();
                    }

                    if (handVRLHAnimatorProp.objectReferenceValue != null)
                    {
                        s3dAnimActionProp = serializedObject.FindProperty("leftHandVRGrip");
                        s3dAAParamHashCodeProp = s3dAnimActionProp.FindPropertyRelative("paramHashCode");                        

                        DrawParameterSelection(animParamsFloatLHVRList, animParamFloatLHVRNames, s3dAAParamHashCodeProp, handVRGripParmNameContent, true);

                        s3dAnimActionProp = serializedObject.FindProperty("leftHandVRTrigger");
                        s3dAAParamHashCodeProp = s3dAnimActionProp.FindPropertyRelative("paramHashCode");

                        DrawParameterSelection(animParamsFloatLHVRList, animParamFloatLHVRNames, s3dAAParamHashCodeProp, handVRTriggerParmNameContent, true);
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(handVRRHAnimatorProp, handVRRHAnimatorContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        RefreshVRAnimatorParameters();
                    }

                    if (handVRRHAnimatorProp.objectReferenceValue != null)
                    {
                        s3dAnimActionProp = serializedObject.FindProperty("rightHandVRGrip");
                        s3dAAParamHashCodeProp = s3dAnimActionProp.FindPropertyRelative("paramHashCode");

                        DrawParameterSelection(animParamsFloatRHVRList, animParamFloatRHVRNames, s3dAAParamHashCodeProp, handVRGripParmNameContent, true);

                        s3dAnimActionProp = serializedObject.FindProperty("rightHandVRTrigger");
                        s3dAAParamHashCodeProp = s3dAnimActionProp.FindPropertyRelative("paramHashCode");

                        DrawParameterSelection(animParamsFloatRHVRList, animParamFloatRHVRNames, s3dAAParamHashCodeProp, handVRTriggerParmNameContent, true);
                    }
                }

                #endregion

                #region Animation Actions

                StickyEditorHelper.DrawUILine(separatorColor);
                StickyEditorHelper.DrawS3DFoldout(isAnimActionsExpandedProp, animateActionsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

                if (isAnimActionsExpandedProp.boolValue)
                {
                    #region Check if s3dAnimActionList is null
                    // Checking the property for being NULL doesn't check if the list is actually null.
                    if (stickyControlModule.s3dAnimActionList == null)
                    {
                        // Apply property changes
                        serializedObject.ApplyModifiedProperties();
                        stickyControlModule.s3dAnimActionList = new List<S3DAnimAction>(10);
                        isSceneModified = true;
                        // Read in the properties
                        serializedObject.Update();
                    }
                    #endregion

                    #region Import - Export JSON
                    StickyEditorHelper.DrawHorizontalGap(2f);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(aaExportJsonContent, buttonCompact, GUILayout.Width(100f)))
                    {
                        string exportPath = EditorUtility.SaveFilePanel("Save Animation Actions", "Assets", stickyControlModule.name + "_animation_actions", "json");

                        if (stickyControlModule.SaveAnimateDataAsJson(exportPath))
                        {
                            // Check if path is in Project folder
                            if (exportPath.Contains(Application.dataPath))
                            {
                                // Get the folder to highlight in the Project folder
                                string folderPath = StickyEditorHelper.GetAssetFolderFromFilePath(exportPath);

                                // Get the json file in the Project folder
                                exportPath = "Assets" + exportPath.Replace(Application.dataPath, "");
                                AssetDatabase.ImportAsset(exportPath);

                                StickyEditorHelper.HighlightFolderInProjectWindow(folderPath, true, true);
                            }
                            Debug.Log("StickyControlModule Animation Actions exported to " + exportPath);
                        }
                        GUIUtility.ExitGUI();
                    }
                    if (GUILayout.Button(aaImportJsonContent, buttonCompact, GUILayout.Width(100f)))
                    {
                        string importPath = string.Empty, importFileName = string.Empty;

                        bool isContinue = stickyControlModule.s3dAnimActionList != null;

                        isContinue = stickyControlModule.s3dAnimActionList.Count < 1 || StickyEditorHelper.PromptForContinue("Overwriting Animation Actions?", "This will replace existing Animation Actions and conditions.\n\n Are you sure?");

                        if (isContinue && StickyEditorHelper.GetFilePathFromUser("Import Animation Actions", StickySetup.s3dFolder, new string[] { "JSON", "json" }, false, ref importPath, ref importFileName))
                        {
                            List<S3DAnimAction> importedAnimActionList = stickyControlModule.ImportAnimateDataFromJson(importPath, importFileName);

                            int numImportedAnimActions = importedAnimActionList == null ? 0 : importedAnimActionList.Count;

                            if (numImportedAnimActions > 0)
                            {
                                serializedObject.ApplyModifiedProperties();
                                Undo.RecordObject(stickyControlModule, "Import Animation Actions");

                                if (stickyControlModule.s3dAnimActionList != null)
                                {
                                    stickyControlModule.s3dAnimActionList.Clear();
                                }
                                stickyControlModule.s3dAnimActionList.AddRange(importedAnimActionList);
                                serializedObject.Update();
                            }
                        }

                        GUIUtility.ExitGUI();
                    }
                    GUILayout.EndHorizontal();
                    StickyEditorHelper.DrawHorizontalGap(2f);
                    #endregion

                    #region Add-Remove AnimActions

                    int numAnimActions = s3dAAListProp.arraySize;

                    // Reset button variables
                    s3dAnimActionMoveDownPos = -1;
                    s3dAnimActionInsertPos = -1;
                    s3dAnimActionDeletePos = -1;

                    GUILayout.BeginHorizontal();

                    EditorGUI.indentLevel += 1;
                    EditorGUIUtility.fieldWidth = 15f;
                    EditorGUI.BeginChangeCheck();
                    isS3DAnimActionListExpandedProp.boolValue = EditorGUILayout.Foldout(isS3DAnimActionListExpandedProp.boolValue, "", foldoutStyleNoLabel);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        ExpandList(stickyControlModule.s3dAnimActionList, isS3DAnimActionListExpandedProp.boolValue);
                        // Read in the properties
                        serializedObject.Update();
                    }
                    EditorGUI.indentLevel -= 1;

                    EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                    EditorGUILayout.LabelField("<color=" + txtColourName + ">Animation Actions: " + numAnimActions.ToString("00") + "</color>", labelFieldRichText);

                    if (GUILayout.Button(aaRefreshParamsContent, GUILayout.MaxWidth(80f)))
                    {
                        stickyControlModule.gameObject.SetActive(false);
                        stickyControlModule.gameObject.SetActive(true);
                        RefreshAnimatorParameters();

                        // NOTE: This Refresh Button will not always be visible (like when there is no default character animator)
                        if (isHandVRProp.boolValue) { RefreshVRAnimatorParameters(); }

                        //RefreshAnimatorTransitions();
                    }

                    if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                    {
                        // Apply property changes
                        serializedObject.ApplyModifiedProperties();
                        Undo.RecordObject(stickyControlModule, "Add Animate Action");
                        stickyControlModule.s3dAnimActionList.Add(new S3DAnimAction());
                        ExpandList(stickyControlModule.s3dAnimActionList, false);
                        isSceneModified = true;
                        // Read in the properties
                        serializedObject.Update();

                        numAnimActions = s3dAAListProp.arraySize;
                        if (numAnimActions > 0)
                        {
                            // Force new AnimAction to be serialized in scene
                            s3dAnimActionProp = s3dAAListProp.GetArrayElementAtIndex(numAnimActions - 1);
                            s3dAAShowInEditorProp = s3dAnimActionProp.FindPropertyRelative("showInEditor");
                            s3dAAShowInEditorProp.boolValue = !s3dAAShowInEditorProp.boolValue;
                            // Show the new AnimAction
                            s3dAAShowInEditorProp.boolValue = true;
                        }
                    }
                    if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                    {
                        if (numAnimActions > 0) { s3dAnimActionDeletePos = s3dAAListProp.arraySize - 1; }
                    }

                    GUILayout.EndHorizontal();

                    #endregion

                    #region Anim Action List
                    numAnimActions = s3dAAListProp.arraySize;

                    for (int aaIdx = 0; aaIdx < numAnimActions; aaIdx++)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        s3dAnimActionProp = s3dAAListProp.GetArrayElementAtIndex(aaIdx);

                        #region Get Properties for the Animate Action
                        s3dAAShowInEditorProp = s3dAnimActionProp.FindPropertyRelative("showInEditor");
                        s3dAAstandardActionProp = s3dAnimActionProp.FindPropertyRelative("standardAction");

                        #endregion

                        #region AnimAction Move/Insert/Delete buttons
                        GUILayout.BeginHorizontal();
                        EditorGUI.indentLevel += 1;
                        s3dAAShowInEditorProp.boolValue = EditorGUILayout.Foldout(s3dAAShowInEditorProp.boolValue, "Animate Action " + (aaIdx + 1).ToString("00") + (s3dAAShowInEditorProp.boolValue ? "" : " - (" + (S3DAnimAction.StandardAction)s3dAAstandardActionProp.intValue + ")"));
                        EditorGUI.indentLevel -= 1;

                        // Move down button
                        if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numAnimActions > 1) { s3dAnimActionMoveDownPos = aaIdx; }
                        // Create duplicate button
                        if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { s3dAnimActionInsertPos = aaIdx; }
                        // Delete button
                        if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dAnimActionDeletePos = aaIdx; }
                        GUILayout.EndHorizontal();
                        #endregion

                        if (s3dAAShowInEditorProp.boolValue)
                        {
                            s3dAAParamTypeProp = s3dAnimActionProp.FindPropertyRelative("parameterType");
                            s3dAAParamHashCodeProp = s3dAnimActionProp.FindPropertyRelative("paramHashCode");

                            EditorGUILayout.PropertyField(s3dAAstandardActionProp, aaStandardActionContent);

                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(s3dAAParamTypeProp, aaParmTypeContent);
                            if (EditorGUI.EndChangeCheck())
                            {
                                s3dAAParamHashCodeProp.intValue = 0;
                            }

                            #region Parameters
                            if (s3dAAParamTypeProp.intValue == S3DAnimAction.ParameterTypeBoolInt)
                            {
                                DrawParameterSelection(animParamsBoolList, animParamBoolNames, s3dAAParamHashCodeProp, aaParmNamesContent);
                                s3dAAIsInvertProp = s3dAnimActionProp.FindPropertyRelative("isInvert");
                                s3dAAIsToggleProp = s3dAnimActionProp.FindPropertyRelative("isToggle");
                                if (IsCustomAction(s3dAAstandardActionProp))
                                {
                                    EditorGUI.BeginChangeCheck();
                                    EditorGUILayout.PropertyField(s3dAAIsInvertProp, aaIsInvertContent);
                                    if (EditorGUI.EndChangeCheck() && s3dAAIsInvertProp.boolValue && s3dAAIsToggleProp.boolValue)
                                    {
                                        s3dAAIsToggleProp.boolValue = false;
                                    }

                                    if (!s3dAAIsInvertProp.boolValue)
                                    {
                                        EditorGUILayout.PropertyField(s3dAAIsToggleProp, aaIsToggleContent);
                                    }

                                    if (!s3dAAIsToggleProp.boolValue)
                                    {
                                        EditorGUILayout.PropertyField(s3dAnimActionProp.FindPropertyRelative("isResetCustomAfterUse"), aaIsResetCustomAfterUseContent);
                                    }
                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(aaBoolValueContent, GUILayout.Width(defaultEditorLabelWidth));
                                    EditorGUILayout.LabelField(aaCustomValueContent, GUILayout.MinWidth(defaultEditorFieldWidth - 8f));
                                    GUILayout.EndHorizontal();
                                }
                                else
                                {
                                    s3dAAValueProp = s3dAnimActionProp.FindPropertyRelative("actionBoolValue");
                                    if (s3dAAValueProp.intValue != S3DAnimAction.ActionBoolValueFixedInt)
                                    {
                                        EditorGUILayout.PropertyField(s3dAAIsInvertProp, aaIsInvertContent);
                                    }
                                    EditorGUILayout.PropertyField(s3dAAValueProp, aaBoolValueContent);

                                    // Is this a fixed bool value the user is sending to the animation controller?
                                    if (s3dAAValueProp.intValue == S3DAnimAction.ActionBoolValueFixedInt)
                                    {
                                        EditorGUILayout.PropertyField(s3dAnimActionProp.FindPropertyRelative("fixedBoolValue"), aaBoolFixedValueContent);
                                    }
                                }
                            }
                            else if (s3dAAParamTypeProp.intValue == S3DAnimAction.ParameterTypeTriggerInt)
                            {
                                DrawParameterSelection(animParamsTriggerList, animParamTriggerNames, s3dAAParamHashCodeProp, aaParmNamesContent);
                                if (IsCustomAction(s3dAAstandardActionProp))
                                {
                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(aaTriggerValueContent, GUILayout.Width(defaultEditorLabelWidth));
                                    EditorGUILayout.LabelField(aaCustomValueContent, GUILayout.MinWidth(defaultEditorFieldWidth - 8f));
                                    GUILayout.EndHorizontal();
                                }
                                else
                                {
                                    s3dAAValueProp = s3dAnimActionProp.FindPropertyRelative("actionTriggerValue");
                                    EditorGUILayout.PropertyField(s3dAAValueProp, aaTriggerValueContent);
                                }
                            }
                            else if (s3dAAParamTypeProp.intValue == S3DAnimAction.ParameterTypeFloatInt)
                            {
                                DrawParameterSelection(animParamsFloatList, animParamFloatNames, s3dAAParamHashCodeProp, aaParmNamesContent);
                                if (IsCustomAction(s3dAAstandardActionProp))
                                {
                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(aaFloatValueContent, GUILayout.Width(defaultEditorLabelWidth));
                                    EditorGUILayout.LabelField(aaCustomValueContent, GUILayout.MinWidth(defaultEditorFieldWidth - 8f));
                                    GUILayout.EndHorizontal();
                                    EditorGUILayout.PropertyField(s3dAnimActionProp.FindPropertyRelative("floatMultiplier"), aaFloatMultiplierContent);
                                    //EditorGUILayout.PropertyField(s3dAnimActionProp.FindPropertyRelative("damping"), aaBlendRateContent);
                                }
                                else
                                {
                                    s3dAAValueProp = s3dAnimActionProp.FindPropertyRelative("actionFloatValue");
                                    EditorGUILayout.PropertyField(s3dAAValueProp, aaFloatValueContent);

                                    // Is this a fixed float value the user is sending to the animation controller?
                                    if (s3dAAValueProp.intValue == S3DAnimAction.ActionFloatValueFixedInt)
                                    {
                                        EditorGUILayout.PropertyField(s3dAnimActionProp.FindPropertyRelative("fixedFloatValue"), aaFloatFixedValueContent);
                                    }
                                    else
                                    {
                                        EditorGUILayout.PropertyField(s3dAnimActionProp.FindPropertyRelative("floatMultiplier"), aaFloatMultiplierContent);
                                    }
                                    EditorGUILayout.PropertyField(s3dAnimActionProp.FindPropertyRelative("damping"), aaDampingContent);
                                }
                            }
                            else if (s3dAAParamTypeProp.intValue == S3DAnimAction.ParameterTypeIntegerInt)
                            {
                                DrawParameterSelection(animParamsIntegerList, animParamIntegerNames, s3dAAParamHashCodeProp, aaParmNamesContent);
                                if (IsCustomAction(s3dAAstandardActionProp))
                                {
                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(aaIntegerValueContent, GUILayout.Width(defaultEditorLabelWidth));
                                    EditorGUILayout.LabelField(aaCustomValueContent, GUILayout.MinWidth(defaultEditorFieldWidth - 8f));
                                    GUILayout.EndHorizontal();
                                }
                                else
                                {
                                    // Currently we don't have any integer realtime values in S3D
                                }
                            }
                            #endregion

                            #region Transistions

                            //DrawTransitionSelection();

                            #endregion

                            #region Conditions

                            s3dACListProp = s3dAnimActionProp.FindPropertyRelative("s3dAnimConditionList");

                            #region Check if s3dAnimConditionList is null
                            // Checking the property for being NULL doesn't check if the list is actually null.
                            if (stickyControlModule.s3dAnimActionList[aaIdx].s3dAnimConditionList == null)
                            {
                                // Apply property changes
                                serializedObject.ApplyModifiedProperties();
                                stickyControlModule.s3dAnimActionList[aaIdx].s3dAnimConditionList = new List<S3DAnimCondition>(2);
                                isSceneModified = true;
                                // Read in the properties
                                serializedObject.Update();
                            }
                            #endregion

                            #region Add-Remove Animation Conditions

                            int numAnimConditions = s3dACListProp.arraySize;

                            // Reset button variables
                            s3dAnimConditionMoveDownPos = -1;
                            s3dAnimConditionInsertPos = -1;
                            s3dAnimConditionDeletePos = -1;

                            GUILayout.BeginHorizontal();

                            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                            EditorGUILayout.LabelField(" Conditions: " + numAnimConditions.ToString("00"), labelFieldRichText);

                            if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                            {
                                // Apply property changes
                                serializedObject.ApplyModifiedProperties();
                                Undo.RecordObject(stickyControlModule, "Add Animate Condition");
                                stickyControlModule.s3dAnimActionList[aaIdx].s3dAnimConditionList.Add(new S3DAnimCondition());
                                isSceneModified = true;
                                // Read in the properties
                                serializedObject.Update();

                                numAnimConditions = s3dACListProp.arraySize;
                                if (numAnimConditions > 0)
                                {
                                    // Force new AnimCondition to be serialized in scene
                                    s3dAnimConditionProp = s3dACListProp.GetArrayElementAtIndex(numAnimConditions - 1);
                                    s3dACShowInEditorProp = s3dAnimConditionProp.FindPropertyRelative("showInEditor");
                                    s3dACShowInEditorProp.boolValue = !s3dACShowInEditorProp.boolValue;
                                    // Show the new AnimCondition
                                    s3dACShowInEditorProp.boolValue = true;
                                }
                            }
                            if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                            {
                                if (numAnimConditions > 0) { s3dAnimConditionDeletePos = s3dACListProp.arraySize - 1; }
                            }

                            GUILayout.EndHorizontal();

                            #endregion

                            #region Anim Condition List
                            numAnimConditions = s3dACListProp.arraySize;

                            if (numAnimConditions > 0)
                            {
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(" ", GUILayout.Width(1f));
                                EditorGUILayout.LabelField(conditionsInfoContent, EditorStyles.wordWrappedLabel);
                                GUILayout.EndHorizontal();
                            }

                            for (int acIdx = 0; acIdx < numAnimConditions; acIdx++)
                            {
                                s3dAnimConditionProp = s3dACListProp.GetArrayElementAtIndex(acIdx);

                                #region Get Properties for the Animate Condition
                                s3dACShowInEditorProp = s3dAnimConditionProp.FindPropertyRelative("showInEditor");
                                s3dACConditionTypeProp = s3dAnimConditionProp.FindPropertyRelative("conditionType");
                                s3dACActionConditionProp = s3dAnimConditionProp.FindPropertyRelative("actionCondition");
                                #endregion

                                #region Condition and  Move/Insert/Delete buttons
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField((acIdx + 1).ToString(" 00"), GUILayout.Width(25f));
                                EditorGUILayout.PropertyField(s3dACConditionTypeProp, GUIContent.none, GUILayout.MaxWidth(60f));
                                EditorGUILayout.PropertyField(s3dACActionConditionProp, GUIContent.none);

                                // Move down button
                                if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numAnimConditions > 1) { s3dAnimConditionMoveDownPos = acIdx; }
                                // Create duplicate button
                                if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { s3dAnimConditionInsertPos = acIdx; }
                                // Delete button
                                if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dAnimConditionDeletePos = acIdx; }
                                GUILayout.EndHorizontal();
                                #endregion
                            }

                            #endregion

                            #region Move/Insert/Delete Anim Conditions

                            if (s3dAnimConditionDeletePos >= 0 || s3dAnimConditionInsertPos >= 0 || s3dAnimConditionMoveDownPos >= 0)
                            {
                                GUI.FocusControl(null);
                                // Don't permit multiple operations in the same pass
                                if (s3dAnimConditionMoveDownPos >= 0)
                                {
                                    // Move down one position, or wrap round to start of list
                                    if (s3dAnimConditionMoveDownPos < s3dACListProp.arraySize - 1)
                                    {
                                        s3dACListProp.MoveArrayElement(s3dAnimConditionMoveDownPos, s3dAnimConditionMoveDownPos + 1);
                                    }
                                    else { s3dACListProp.MoveArrayElement(s3dAnimConditionMoveDownPos, 0); }

                                    s3dAnimConditionMoveDownPos = -1;
                                }
                                else if (s3dAnimConditionInsertPos >= 0)
                                {
                                    // NOTE: Undo doesn't work with Insert.

                                    // Apply property changes before potential list changes
                                    serializedObject.ApplyModifiedProperties();

                                    stickyControlModule.s3dAnimActionList[aaIdx].s3dAnimConditionList.Insert(s3dAnimConditionInsertPos, new S3DAnimCondition(stickyControlModule.s3dAnimActionList[aaIdx].s3dAnimConditionList[s3dAnimConditionInsertPos]));

                                    // Read all properties from the Sticky Controller
                                    serializedObject.Update();

                                    // Hide original Animate Condition
                                    s3dACShowInEditorProp = s3dACListProp.GetArrayElementAtIndex(s3dAnimConditionInsertPos).FindPropertyRelative("showInEditor");

                                    // Force new condition to be serialized in scene
                                    s3dACShowInEditorProp.boolValue = !s3dACShowInEditorProp.boolValue;

                                    // Show inserted duplicate dockingPoint
                                    s3dACShowInEditorProp.boolValue = true;

                                    s3dAnimConditionInsertPos = -1;

                                    isSceneModified = true;
                                }
                                else if (s3dAnimConditionDeletePos >= 0)
                                {
                                    // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and DeletePos is reset to -1.
                                    int _deleteIndex = s3dAnimConditionDeletePos;

                                    if (EditorUtility.DisplayDialog("Delete Animate Condition " + (s3dAnimConditionDeletePos + 1) + "?", "Animate Condition " + (s3dAnimConditionDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the Animate Condition from the list and cannot be undone.", "Delete Now", "Cancel"))
                                    {
                                        s3dACListProp.DeleteArrayElementAtIndex(_deleteIndex);
                                        s3dAnimConditionDeletePos = -1;
                                    }
                                }

                                serializedObject.ApplyModifiedProperties();
                                // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                                if (!Application.isPlaying)
                                {
                                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                                }
                                GUIUtility.ExitGUI();
                            }

                            #endregion

                            #endregion
                        }

                        GUILayout.EndVertical();
                    }

                    #endregion

                    #region Move/Insert/Delete Anim Actions

                    if (s3dAnimActionDeletePos >= 0 || s3dAnimActionInsertPos >= 0 || s3dAnimActionMoveDownPos >= 0)
                    {
                        GUI.FocusControl(null);
                        // Don't permit multiple operations in the same pass
                        if (s3dAnimActionMoveDownPos >= 0)
                        {
                            // Move down one position, or wrap round to start of list
                            if (s3dAnimActionMoveDownPos < s3dAAListProp.arraySize - 1)
                            {
                                s3dAAListProp.MoveArrayElement(s3dAnimActionMoveDownPos, s3dAnimActionMoveDownPos + 1);
                            }
                            else { s3dAAListProp.MoveArrayElement(s3dAnimActionMoveDownPos, 0); }

                            s3dAnimActionMoveDownPos = -1;
                        }
                        else if (s3dAnimActionInsertPos >= 0)
                        {
                            // NOTE: Undo doesn't work with Insert.

                            // Apply property changes before potential list changes
                            serializedObject.ApplyModifiedProperties();

                            S3DAnimAction insertedAnimAction = new S3DAnimAction(stickyControlModule.s3dAnimActionList[s3dAnimActionInsertPos]);
                            insertedAnimAction.showInEditor = true;
                            // Generate a new hashcode for the duplicated AnimAction
                            insertedAnimAction.guidHash = S3DMath.GetHashCodeFromGuid();

                            //stickyControlModule.s3dAnimActionList.Insert(s3dAnimActionInsertPos, new S3DAnimAction(stickyControlModule.s3dAnimActionList[s3dAnimActionInsertPos]));
                            stickyControlModule.s3dAnimActionList.Insert(s3dAnimActionInsertPos, insertedAnimAction);

                            // Read all properties from the Sticky Controller
                            serializedObject.Update();

                            // Hide original Animate Action
                            s3dAAListProp.GetArrayElementAtIndex(s3dAnimActionInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                            s3dAAShowInEditorProp = s3dAAListProp.GetArrayElementAtIndex(s3dAnimActionInsertPos).FindPropertyRelative("showInEditor");

                            // Force new action to be serialized in scene
                            s3dAAShowInEditorProp.boolValue = !s3dAAShowInEditorProp.boolValue;

                            // Show inserted duplicate AnimAction
                            s3dAAShowInEditorProp.boolValue = true;

                            s3dAnimActionInsertPos = -1;

                            isSceneModified = true;
                        }
                        else if (s3dAnimActionDeletePos >= 0)
                        {
                            // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and DeletePos is reset to -1.
                            int _deleteIndex = s3dAnimActionDeletePos;

                            if (EditorUtility.DisplayDialog("Delete Animate Action " + (s3dAnimActionDeletePos + 1) + "?", "Animate Action " + (s3dAnimActionDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the Animate Action from the list and cannot be undone.", "Delete Now", "Cancel"))
                            {
                                s3dAAListProp.DeleteArrayElementAtIndex(_deleteIndex);
                                s3dAnimActionDeletePos = -1;
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
                }
                StickyEditorHelper.DrawHorizontalGap(2f);
                #endregion

                EditorGUILayout.EndVertical();
            }

            #endregion

            #region Engage tab
            else if (selectedTabIntProp.intValue == 5)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                #region General Engage Settings

                StickyEditorHelper.DrawS3DFoldout(isEngageGeneralExpandedProp, isEngageGeneralExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

                if (isEngageGeneralExpandedProp.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    // health is stored internally as 0-1 value
                    healthProp.floatValue = EditorGUILayout.Slider(healthContent, healthProp.floatValue * 100f, 0f, 100f) / 100f;
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        stickyControlModule.SetHealth(healthProp.floatValue * 100f);
                        stickyControlModule.GetMainDamageRegion().startingHealth = healthProp.floatValue;
                        serializedObject.Update();
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(storeMaxSelectableInSceneProp, storeMaxSelectableInSceneContent);
                    if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                    {
                        stickyControlModule.SetStoreMaxSelectableInScene(storeMaxSelectableInSceneProp.intValue, false);
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(engageColourProp, engageColourContent);
                    if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                    {
                        stickyControlModule.SetEngageColour(engageColourProp.colorValue);
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(nonengageColourProp, nonengageColourContent);
                    if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                    {
                        stickyControlModule.SetNonEngageColour(nonengageColourProp.colorValue);
                    }

                    DrawEngageInteractiveTags();
                    DrawEngageLassoSettings();
                }
                #endregion

                #region Damage Regions

                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

                StickyEditorHelper.DrawS3DFoldout(isEngageDmRgnExpandedProp, isEngageDamageRegionsExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

                if (isEngageDmRgnExpandedProp.boolValue)
                {
                    StickyEditorHelper.InTechPreview(true);

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(damageRegionGenerateContent, buttonCompact))
                    {
                        CreateDamageRegions();
                    }
                    if (GUILayout.Button(damageRegionRemoveContent, buttonCompact))
                    {
                        RemoveDamageRegions();
                    }
                    EditorGUILayout.EndHorizontal();

                    #region Add-Remove Damage Regions

                    int numDamageRegions = damageRegionListProp.arraySize;

                    // Reset button variables
                    s3dDmRgnMoveDownPos = -1;
                    s3dDmRgnInsertPos = -1;
                    s3dDmRgnDeletePos = -1;

                    GUILayout.BeginHorizontal();

                    EditorGUI.indentLevel += 1;
                    EditorGUIUtility.fieldWidth = 15f;
                    EditorGUI.BeginChangeCheck();
                    isEngageDmRgnListExpandedProp.boolValue = EditorGUILayout.Foldout(isEngageDmRgnListExpandedProp.boolValue, "", foldoutStyleNoLabel);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        ExpandList(stickyControlModule.DamageRegionList, isEngageDmRgnListExpandedProp.boolValue);
                        // Read in the properties
                        serializedObject.Update();
                    }
                    EditorGUI.indentLevel -= 1;

                    EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                    EditorGUILayout.LabelField("<color=" + txtColourName + ">Damage Regions: " + numDamageRegions.ToString("00") + "</color>", labelFieldRichText);

                    if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                    {
                        // Apply property changes
                        serializedObject.ApplyModifiedProperties();
                        Undo.RecordObject(stickyControlModule, "Add Damage Region");
                        stickyControlModule.AddDamageRegion(new S3DDamageRegion());
                        ExpandList(stickyControlModule.DamageRegionList, false);
                        isSceneModified = true;
                        // Read in the properties
                        serializedObject.Update();

                        numDamageRegions = damageRegionListProp.arraySize;
                        if (numDamageRegions > 1)
                        {
                            // Force new Damage Region to be serialized in scene
                            damageRegionProp = damageRegionListProp.GetArrayElementAtIndex(numDamageRegions - 1);
                            damageRegionShowInEditorProp = damageRegionProp.FindPropertyRelative("showInEditor");
                            damageRegionShowInEditorProp.boolValue = !damageRegionShowInEditorProp.boolValue;
                            // Show the new Damage Region
                            damageRegionShowInEditorProp.boolValue = true;
                        }
                    }
                    if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                    {
                        // Must always be a main damage region
                        if (numDamageRegions > 1) { s3dDmRgnDeletePos = damageRegionListProp.arraySize - 1; }
                    }

                    GUILayout.EndHorizontal();
                    #endregion

                    // Draw list damage regions (main will always be drawn)
                    for (int dmRgnIdx = 0; dmRgnIdx < numDamageRegions; dmRgnIdx++)
                    {
                        DrawDamageRegion(damageRegionListProp.GetArrayElementAtIndex(dmRgnIdx), dmRgnIdx, numDamageRegions);
                    }

                    #region Move/Insert/Delete Damage Regions

                    // We cannot move/insert/delete at the main damage region position in the list
                    if (s3dDmRgnDeletePos > 0 || s3dDmRgnInsertPos > 0 || s3dDmRgnMoveDownPos > 0)
                    {
                        GUI.FocusControl(null);
                        // Don't permit multiple operations in the same pass
                        if (s3dDmRgnMoveDownPos > 0)
                        {
                            if (damageRegionListProp.arraySize > 2)
                            {
                                // Move down one position, or wrap round to 2nd position in list after Main damage region
                                if (s3dDmRgnMoveDownPos < damageRegionListProp.arraySize - 1)
                                {
                                    damageRegionListProp.MoveArrayElement(s3dDmRgnMoveDownPos, s3dDmRgnMoveDownPos + 1);
                                }
                                else { damageRegionListProp.MoveArrayElement(s3dDmRgnMoveDownPos, 1); }
                            }
                            s3dDmRgnMoveDownPos = -1;
                        }
                        else if (s3dDmRgnInsertPos > 0)
                        {
                            // NOTE: Undo doesn't work with Insert.

                            // Apply property changes before potential list changes
                            serializedObject.ApplyModifiedProperties();

                            S3DDamageRegion insertedDamageRegion = new S3DDamageRegion(stickyControlModule.DamageRegionList[s3dDmRgnInsertPos]);
                            insertedDamageRegion.showInEditor = true;
                            // Generate a new hashcode for the duplicated Damage Region
                            insertedDamageRegion.guidHash = S3DMath.GetHashCodeFromGuid();

                            stickyControlModule.DamageRegionList.Insert(s3dDmRgnInsertPos, insertedDamageRegion);

                            // Read all properties from the Sticky Controller
                            serializedObject.Update();

                            // Hide original Damage Region
                            damageRegionListProp.GetArrayElementAtIndex(s3dDmRgnInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                            damageRegionShowInEditorProp = damageRegionListProp.GetArrayElementAtIndex(s3dDmRgnInsertPos).FindPropertyRelative("showInEditor");

                            // Force new action to be serialized in scene
                            damageRegionShowInEditorProp.boolValue = !damageRegionShowInEditorProp.boolValue;

                            // Show inserted duplicate Damage Region
                            damageRegionShowInEditorProp.boolValue = true;

                            s3dDmRgnInsertPos = -1;

                            isSceneModified = true;
                        }
                        else if (s3dDmRgnDeletePos > 0)
                        {
                            // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and DeletePos is reset to -1.
                            int _deleteIndex = s3dDmRgnDeletePos;

                            if (EditorUtility.DisplayDialog("Delete Damage Region " + (s3dDmRgnDeletePos + 1) + "?", "Damage Region " + (s3dDmRgnDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the Damage Region from the list and cannot be undone.", "Delete Now", "Cancel"))
                            {
                                damageRegionListProp.DeleteArrayElementAtIndex(_deleteIndex);
                                s3dDmRgnDeletePos = -1;
                            }
                        }
                    }


                    #endregion
                }

                #endregion

                DrawEngageEquipSettings();

                DrawEngageEventSettings();

                DrawIdentificationSettings();

                DrawRespawnSettings();

                DrawStashEngageSettings();

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
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && stickyControlModule != null)
            {
                float rightLabelWidth = 175f;

                StickyEditorHelper.PerformanceImpact();

                isShowVolume = EditorGUILayout.Toggle(debugShowVolumeContent, isShowVolume);
                isShowGroundedIndicator = EditorGUILayout.Toggle(debugShowGroundedContent, isShowGroundedIndicator);
                isShowHeadIKDirection = EditorGUILayout.Toggle(debugShowHeadIKDirectionContent, isShowHeadIKDirection);

                #region Debug - Move

                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
                isDebugMoveExpanded = StickyEditorHelper.DrawS3DFoldout(isDebugMoveExpanded, debugIsMoveExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

                if (isDebugMoveExpanded)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsInitialised ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsMovementEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsMovementEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsPositionLockedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsPositionLocked ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsGroundedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsGrounded ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsSteppingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsStepping ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsWalkingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsWalking ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsSprintingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsSprinting ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsStrafingLeftContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsStrafingLeft ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsStrafingRightContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsStrafingRight ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsCrouchingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsCrouching ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsClimbingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsClimbing ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugCurrentHeightContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.ScaledSize.y.ToString("0.00"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugCrouchAmountContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.CrouchAmount.ToString("0.00"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugSpeedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    if (stickyControlModule.IsInitialised)
                    {
                        // z-axis of Local space velocity relative to the object it is walking on. Convert to km/h
                        float characterVelocity = stickyControlModule.GetCurrentLocalVelocity.z;
                        //if (characterVelocity < 0) { characterVelocity = -characterVelocity; }

                        EditorGUILayout.LabelField((characterVelocity * 3.6f).ToString("0.0") + " (m/s " + characterVelocity.ToString("0.0") + ")", GUILayout.MaxWidth(rightLabelWidth));
                    }
                    else
                    {
                        EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugTurningSpeedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.TurningSpeed.ToString("0.00"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugGroundSlopeContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.GetLastGroundSlope.ToString("0.00"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();
                }
                #endregion

                #region Debug - Look

                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
                isDebugLookExpanded = StickyEditorHelper.DrawS3DFoldout(isDebugLookExpanded, debugIsLookExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

                if (isDebugLookExpanded)
                {
                    #region Look General

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsThirdPersonContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsLookThirdPersonEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsFreeLookContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsLookFreeLookEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsLookEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsLookEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsLookFollowHeadContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsLookCameraFollowHead ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsLookFollowHeadTPContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsLookCameraFollowHeadTP ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsLookMovementEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsLookMovementEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugZoomAmountContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.ZoomAmount.ToString("0.000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsLookFixedUpdateContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsLookFixedUpdate ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsLockCamToWorldPosContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsLockCamToWorldPos ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsLockCamToWorldRotContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsLockCamToWorldRot ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    #endregion

                    #region Debug - Look Interactive
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsLookInteractiveEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsLookInteractiveEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    if (stickyControlModule.IsLookInteractiveEnabled)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(debugIsLookingAtInteractiveContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                        EditorGUILayout.LabelField(stickyControlModule.GetLookingAtInteractiveId() == StickyInteractive.NoID ? "--" : stickyControlModule.GetLookingAtInteractive().name, GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(debugIsLookingAtPointContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                        EditorGUILayout.LabelField(StickyEditorHelper.GetVector3Text(stickyControlModule.GetLookingAtPoint, 1), GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();
                    }

                    #endregion

                    #region Debug - Look VR
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsLookVREnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsLookVREnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();
                    #endregion

                    #region Debug - Look Sockets
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsLookSocketsEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsLookSocketsEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();
                    #endregion
                }
                #endregion

                #region Debug - Collide
                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
                isDebugCollideExpanded = StickyEditorHelper.DrawS3DFoldout(isDebugCollideExpanded, debugIsCollideExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

                if (isDebugCollideExpanded)
                {
                    #region Debug - Reference Frame
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugCurrentReferenceFrameContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.GetCurrentReferenceFrame == null ? "--" : stickyControlModule.GetCurrentReferenceFrame.name, GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();
                    #endregion
                }
                #endregion

                #region Debug - Animate

                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
                isDebugAnimateExpanded = StickyEditorHelper.DrawS3DFoldout(isDebugAnimateExpanded, debugIsAnimateExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

                if (isDebugAnimateExpanded)
                {
                    #region Debug - Animate General

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsAnimateEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsAnimateEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    #endregion

                    #region Debug - Aim IK
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsAimEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsAimAtTarget ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    #endregion

                    #region Debug - Head IK
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsHeadIKEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsHeadIKEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    if (stickyControlModule.IsHeadIKEnabled)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(debugHeadIKTargetTrfmContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                        EditorGUILayout.LabelField(stickyControlModule.GetHeadIKTarget() == null ? "--" : stickyControlModule.GetHeadIKTarget().name, GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(debugHeadIKTargetPosContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                        EditorGUILayout.LabelField(StickyEditorHelper.GetVector3Text(stickyControlModule.GetHeadIKTargetPosition(), 3), GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();
                    }
                    #endregion

                    #region Debug - Hand IK
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsHandIKEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsHandIKEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    if (stickyControlModule.IsHandIKEnabled)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(debugCurrentMaxHandIKWeightContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                        EditorGUILayout.LabelField(stickyControlModule.CurrentMaxHandIKWeight.ToString("0.00"), GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(debugLeftHandIKTargetTrfmContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                        EditorGUILayout.LabelField(stickyControlModule.GetLeftHandIKTarget() == null ? "--" : stickyControlModule.GetLeftHandIKTarget().name, GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(debugLeftHandIKTargetPosContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                        EditorGUILayout.LabelField(StickyEditorHelper.GetVector3Text(stickyControlModule.GetLeftHandIKTargetPosition(), 3), GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(debugLeftHandIKPrevPosContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                        EditorGUILayout.LabelField(StickyEditorHelper.GetVector3Text(stickyControlModule.GetLeftHandIKPreviousPosition(), 3), GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(debugRightHandIKTargetTrfmContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                        EditorGUILayout.LabelField(stickyControlModule.GetRightHandIKTarget() == null ? "--" : stickyControlModule.GetRightHandIKTarget().name, GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(debugRightHandIKTargetPosContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                        EditorGUILayout.LabelField(StickyEditorHelper.GetVector3Text(stickyControlModule.GetRightHandIKTargetPosition(), 3), GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(debugRightHandIKPrevPosContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                        EditorGUILayout.LabelField(StickyEditorHelper.GetVector3Text(stickyControlModule.GetRightHandIKPreviousPosition(), 3), GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();
                    }

                    #endregion

                    #region Debug - Foot IK
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsFootIKEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.IsFootIKEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    if (stickyControlModule.IsFootIKEnabled)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(debugLeftFootIKPosContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                        EditorGUILayout.LabelField(StickyEditorHelper.GetVector3Text(stickyControlModule.GetLeftFootIKPosition(), 3), GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(debugRightFootIKPosContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                        EditorGUILayout.LabelField(StickyEditorHelper.GetVector3Text(stickyControlModule.GetRightFootIKPosition(), 3), GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();
                    }

                    #endregion

                    #region Debug - Bones
                    isBoneDebugging = EditorGUILayout.Toggle(debugBoneDebuggingContent, isBoneDebugging);

                    if (isBoneDebugging)
                    {
                        if (stickyControlModule.defaultAnimator != null && stickyControlModule.defaultAnimator.runtimeAnimatorController != null)
                        {
                            RefreshBones();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(debugLeftFootPosWSContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                            if (leftFootTrfm != null)
                            {
                                EditorGUILayout.LabelField(StickyEditorHelper.GetVector3Text(leftFootTrfm.position, 3), GUILayout.MaxWidth(rightLabelWidth));
                            }
                            else { EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth)); }
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(debugRightFootPosWSContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                            if (rightFootTrfm != null)
                            {
                                EditorGUILayout.LabelField(StickyEditorHelper.GetVector3Text(rightFootTrfm.position, 3), GUILayout.MaxWidth(rightLabelWidth));
                            }
                            else { EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth)); }
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(debugLeftFootToSoleContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                            if (leftFootTrfm != null)
                            {
                                EditorGUILayout.LabelField(stickyControlModule.defaultAnimator.leftFeetBottomHeight.ToString("0.000"), GUILayout.MaxWidth(rightLabelWidth));
                            }
                            else { EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth)); }
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(debugRightFootToSoleContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                            if (leftFootTrfm != null)
                            {
                                EditorGUILayout.LabelField(stickyControlModule.defaultAnimator.rightFeetBottomHeight.ToString("0.000"), GUILayout.MaxWidth(rightLabelWidth));
                            }
                            else { EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth)); }
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(debugHeadNameContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                            if (headTrfm != null)
                            {
                                EditorGUILayout.LabelField(headTrfm.name, GUILayout.MaxWidth(rightLabelWidth));
                            }
                            else { EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth)); }
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.LabelField(debugAnimatorNotSetContent);
                        }
                    }
                    #endregion
                }
                #endregion

                #region Debug - Engage

                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
                isDebugEngageExpanded = StickyEditorHelper.DrawS3DFoldout(isDebugEngageExpanded, debugIsEngageExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

                if (isDebugEngageExpanded)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugNumSelectedInteractiveContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.GetNumberSelectedInteractive().ToString(), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    #region Held Objects

                    StickyInteractive stickyInteractive = stickyControlModule.LeftHandInteractive;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugLHInteractiveContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyInteractive == null ? "--" : stickyInteractive.name, GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    stickyInteractive = stickyControlModule.RightHandInteractive;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugRHInteractiveContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyInteractive == null ? "--" : stickyInteractive.name, GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    #endregion

                    #region Stashed Objects

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugNumStashedItemsContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                    EditorGUILayout.LabelField(stickyControlModule.NumberOfStashItems.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    #endregion

                    #region Debug - Damage Regions

                    isDamageRegionDebugging = EditorGUILayout.Toggle(debugDmRegDebuggingContent, isDamageRegionDebugging);

                    if (isDamageRegionDebugging && stickyControlModule.IsInitialised)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(debugDmRegLivesContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                        EditorGUILayout.LabelField(stickyControlModule.Lives.ToString("0"), GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();

                        for (int drIdx = 0; drIdx < stickyControlModule.NumberOfDamageRegions; drIdx++)
                        {
                            S3DDamageRegion damageRegion = stickyControlModule.GetDamageRegionByIndex(drIdx);

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(debugDmRegNameContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                            EditorGUILayout.LabelField(damageRegion.name, GUILayout.MaxWidth(rightLabelWidth));
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(debugDmRegHealthContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                            EditorGUILayout.LabelField(damageRegion.Health.ToString("0.0"), GUILayout.MaxWidth(rightLabelWidth));
                            EditorGUILayout.EndHorizontal();

                            string shieldHealthTxt = damageRegion.useShielding ? (damageRegion.ShieldHealth < 0f ? "Down" : damageRegion.ShieldHealth.ToString("0.0")) : "--";

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(debugDmRegShieldHealthContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                            EditorGUILayout.LabelField(shieldHealthTxt, GUILayout.MaxWidth(rightLabelWidth));
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.EndVertical();
                        }
                    }

                    #endregion
                }
                #endregion
            }
            EditorGUILayout.EndVertical();
            #endregion

            stickyControlModule.allowRepaint = true;
        }

        #endregion
    }
}