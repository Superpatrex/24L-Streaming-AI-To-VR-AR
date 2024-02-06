#define _SSC_SHIP_DEBUG
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// For Math functions instead of Mathf
using System;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class containing data for a ship.
    /// </summary>
    [System.Serializable]
    public class Ship
    {
        #region Enumerations

        public enum ShipPhysicsModel
        {
            PhysicsBased = 10,
            Arcade = 20
        }

        public enum GroundNormalCalculationMode
        {
            SingleNormal = 10,
            SmoothedNormal = 20
            //AverageHeight = 30
        }

        public enum RollControlMode
        {
            YawInput = 10,
            StrafeInput = 20
        }

        public enum ShipDamageModel
        {
            Simple = 10,
            Progressive = 20,
            Localised = 30
        }

        public enum RespawningMode
        {
            DontRespawn = 10,
            RespawnAtOriginalPosition = 20,
            RespawnAtLastPosition = 30,
            RespawnAtSpecifiedPosition = 40,
            RespawnOnPath = 50
        }

        public enum StuckAction
        {
            DoNothing = 10,
            InvokeCallback = 20,
            RespawnOnPath = 30,
            SameAsRespawningMode = 50
        }

        public enum InputControlAxis
        {
            None = 0,
            X = 1,
            Y = 2
        }

        #endregion

        #region Public Variables and Properties

        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// If enabled, the InitialiseShip() will be called as soon as Awake() runs for the ship. This should be disabled if you are
        /// instantiating the ship through code.
        /// </summary>
        public bool initialiseOnAwake;

        /// <summary>
        /// What physics model the ship uses. If you modify this, call ReinitialiseShipPhysicsModel().
        /// Physics Based: Physics-based model. Physically realistic thrusters and aerodynamic devices are used to control the ship's motion.
        /// Arcade: Simplified physics model. Some physical effects are simplified and unrealistic effects can be used to enhance gameplay.
        /// </summary>
        public ShipPhysicsModel shipPhysicsModel;

        // Physical characteristics
        /// <summary>
        /// Mass of the ship in kilograms. If you modify this, call ReinitialiseMass() on the ShipControlModule.
        /// </summary>
        public float mass;
        /// <summary>
        /// Centre of mass of the ship in local space. If you modify this, call ReinitialiseMass() on the ShipControlModule.
        /// </summary>
        public Vector3 centreOfMass;
        /// <summary>
        /// If set to true, the centre of mass will be set to the value of centreOfMass. 
        /// Otherwise the centre of mass will be set to the unity default specified by the rigdbody.
        /// NOTE: This value will not affect anything if changed at runtime.
        /// </summary>
        public bool setCentreOfMassManually;

        /// <summary>
        /// Show the centre of mass gizmos in the scene view.
        /// </summary>
        public bool showCOMGizmosInSceneView;

        /// <summary>
        /// Show the centre of lift and lift direction gizmos in the scene view.
        /// </summary>
        public bool showCOLGizmosInSceneView;

        /// <summary>
        /// Show the centre of forwards thrust gizmos in the scene view.
        /// </summary>
        public bool showCOTGizmosInSceneView;

        /// <summary>
        /// List of all thruster components for this ship. If you modify this, call ReinitialiseThrusterVariables() and ReinitialiseInputVariables().
        /// </summary>
        public List<Thruster> thrusterList;

        /// <summary>
        /// When the ship is enabled, but ship movement is disabled, should thrusters fire
        /// if they have isMinEffectsAlwaysOn enabled?
        /// </summary>
        public bool isThrusterFXStationary;

        /// <summary>
        /// Are the thruster systems online or in the process of coming online?
        /// At runtime call shipInstance.StartupThrusterSystems() or ShutdownThrusterSystems()
        /// </summary>
        public bool isThrusterSystemsStarted;
        /// <summary>
        /// The time, in seconds, it takes for the thrusters to fully come online
        /// </summary>
        [Range(0f, 60f)] public float thrusterSystemStartupDuration;
        /// <summary>
        /// The time, in seconds, it takes for the thrusters to fully shutdown
        /// </summary>
        [Range(0f, 60f)] public float thrusterSystemShutdownDuration;
        /// <summary>
        /// For thrusters, use a central fuel level, rather than fuel level per thruster.
        /// </summary>
        public bool useCentralFuel;
        /// <summary>
        /// The amount of fuel available when useCentralFuel is true - range 0.0 (empty) to 100.0 (full).
        /// At runtime call shipInstance.SetFuelLevel(..)
        /// </summary>
        [Range(0f, 100f)] public float centralFuelLevel;
        /// <summary>
        /// List of all wing components for this ship. If you modify this, call ReinitialiseWingVariables().
        /// </summary>
        public List<Wing> wingList;
        /// <summary>
        /// List of all control surface components for this ship. If you modify this, call ReinitialiseInputVariables().
        /// </summary>
        public List<ControlSurface> controlSurfaceList;
        /// <summary>
        /// List of all weapon components for this ship. If you modify this, call ReinitialiseWeaponVariables().
        /// </summary>
        public List<Weapon> weaponList;
        /// <summary>
        /// When the ship is enabled, but movement is disabled, weapons and damage are updated
        /// </summary>
        public bool useWeaponsWhenMovementDisabled = false;
        /// <summary>
        /// [READ ONLY] The number of weapons on this ship
        /// </summary>
        public int NumberOfWeapons { get { return weaponList == null ? 0 : weaponList.Count; } }

        /// <summary>
        /// [Internal Use] Is the thruster systems section expanded in the SCM Editor?
        /// </summary>
        public bool isThrusterSystemsExpanded;
        /// <summary>
        /// [Internal Use] Is the thruster list expanded in the SCM Editor?
        /// </summary>
        public bool isThrusterListExpanded;
        /// <summary>
        /// [Internal Use] Is the wing list expanded in the SCM Editor?
        /// </summary>
        public bool isWingListExpanded;
        /// <summary>
        /// [Internal Use] Is the control surface list expanded in the SCM Editor?
        /// </summary>
        public bool isControlSurfaceListExpanded;
        /// <summary>
        /// [Internal Use] Is the weapon list expanded in the SCM Editor?
        /// </summary>
        public bool isWeaponListExpanded;
        /// <summary>
        /// [Internal Use] Is the main damage expanded in the SCM Editor?
        /// </summary>
        public bool isMainDamageExpanded;
        /// <summary>
        /// [Internal Use] Is the local damage list expanded in the SCM Editor?
        /// </summary>
        public bool isDamageListExpanded;
        /// <summary>
        /// Whether the Damge is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showDamageInEditor;

        // Aerodynamic properties
        /// <summary>
        /// The drag coefficients in the local x, y and z directions. Increasing the drag coefficients will increase drag.
        /// </summary>
        public Vector3 dragCoefficients;
        /// <summary>
        /// The projected areas (in square metres) of the ship in the local x, y and z planes.
        /// </summary>
        public Vector3 dragReferenceAreas;
        /// <summary>
        /// The centre of drag for moments causing rotation along the local x-axis.
        /// </summary>
        public Vector3 centreOfDragXMoment;
        /// <summary>
        /// The centre of drag for moments causing rotation along the local y-axis.
        /// </summary>
        public Vector3 centreOfDragYMoment;
        /// <summary>
        /// The centre of drag for moments causing rotation along the local z-axis.
        /// </summary>
        public Vector3 centreOfDragZMoment;
        /// <summary>
        /// Multiplier of the effect of angular drag. Only relevant when not in physics-based mode (when in this mode it is set to the physically realistic value of 1).
        /// </summary>
        [Range(0f, 4f)] public float angularDragFactor;
        /// <summary>
        /// When this is enabled, drag moments have no effect (i.e. drag cannot cause the ship to rotate). Only relevant when not in physics-based mode (when in this mode drag moments are always enabled).
        /// </summary>
        public bool disableDragMoments;
        /// <summary>
        /// A multiplier for drag moments causing rotation along the local (pitch) x-axis. Decreasing this will make these moments weaker.
        /// </summary>
        [Range(0f, 1f)] public float dragXMomentMultiplier;
        /// <summary>
        /// A multiplier for drag moments causing rotation along the local (yaw) y-axis. Decreasing this will make these moments weaker.
        /// </summary>
        [Range(0f, 1f)] public float dragYMomentMultiplier;
        /// <summary>
        /// A multiplier for drag moments causing rotation along the local (roll) z-axis. Decreasing this will make these moments weaker.
        /// </summary>
        [Range(0f, 1f)] public float dragZMomentMultiplier;

        /// <summary>
        /// How strong the effect of stalling is on wings (between 0 and 1). Higher values make the effect of stalling more prominent.
        /// </summary>
        [Range(0f, 1f)] public float wingStallEffect;

        /// <summary>
        /// Whether a braking component is enabled in arcade mode.
        /// </summary>
        public bool arcadeUseBrakeComponent;
        /// <summary>
        /// The strength of the brake force in arcade mode. Only relevant when not in physics-based mode and when 
        /// arcadeUseBrakeComponent is enabled.
        /// </summary>
        public float arcadeBrakeStrength;
        /// <summary>
        /// Whether the strength of the brake force ignores the density of the medium the ship is in (assuming it to be a constant
        /// value of one kilogram per cubic metre). Only relevant when not in physics-based mode and when arcadeUseBrakeComponent 
        /// is enabled.
        /// </summary>
        public bool arcadeBrakeIgnoreMediumDensity;
        /// <summary>
        /// The minimum braking acceleration (in metres per second) caused by the brake when the brake is fully engaged. Increase this
        /// value to make the ship come to a stop more quickly at low speeds. Only relevant when not in physics-based mode and when 
        /// arcadeUseBrakeComponent is enabled.
        /// </summary>
        public float arcadeBrakeMinAcceleration;

        // Input modulation
        /// <summary>
        /// Strength of the rotational flight assist. Set to zero to disable.
        /// </summary>
        [Range(0f, 10f)] public float rotationalFlightAssistStrength;
        /// <summary>
        /// Strength of the translational flight assist. Set to zero to disable.
        /// </summary>
        [Range(0f, 10f)] public float translationalFlightAssistStrength;
        /// <summary>
        /// Strength of the stability flight assist. Set to zero to disable.
        /// </summary>
        [Range(0f, 10f)] public float stabilityFlightAssistStrength;
        /// <summary>
        /// Strength of the brake flight assist. Set to zero to disable [Default = 0]
        /// Operates on Forward and Backward movements when there is no ship input.
        /// Is overridden in forwards direction when AutoCruise is enabled on PlayerInputModule.
        /// </summary>
        [Range(0f, 10f)] public float brakeFlightAssistStrength;
        /// <summary>
        /// The effective minimum speed in m/s that the brake flight assist will operate
        /// </summary>
        [Range(-1000f, 1000f)] public float brakeFlightAssistMinSpeed;
        /// <summary>
        /// The effective maximum speed in m/s that the brake flight assist will operate
        /// </summary>
        [Range(-1000f, 1000f)] public float brakeFlightAssistMaxSpeed;
        /// <summary>
        /// Strength of the brake flight assist on local x-axis. Set to zero to disable [Default = 0]
        /// Operates on left and right movements when there is no ship input.
        /// </summary>
        [Range(0f, 10f)] public float brakeFlightAssistStrengthX;
        /// <summary>
        /// Strength of the brake flight assist on local y-axis. Set to zero to disable [Default = 0]
        /// Operates on up and down movements when there is no ship input.
        /// </summary>
        [Range(0f, 10f)] public float brakeFlightAssistStrengthY;
        /// <summary>
        /// How much power of the ship's roll thrusters is used to execute roll maneuvers. 
        /// Increasing this value will increase the roll speed and responsiveness of the ship.
        /// </summary>
        [Range(0f, 1f)] public float rollPower;
        /// <summary>
        /// How much power of the ship's roll thrusters is used to execute pitching maneuvers. 
        /// Increasing this value will increase the pitch speed and responsiveness of the ship.
        /// </summary>
        [Range(0f, 1f)] public float pitchPower;
        /// <summary>
        /// How much power of the ship's roll thrusters is used to execute yaw maneuvers. 
        /// Increasing this value will increase the yaw speed and responsiveness of the ship.
        /// </summary>
        [Range(0f, 1f)] public float yawPower;
        /// <summary>
        /// How much steering inputs are prioritised over lateral inputs for thrusters.
        /// A value of 0 means no prioritisation takes place, while a value of 1 will almost completely deactivate relevant lateral
        /// thrusters whenever any opposing steering input at all is applied.
        /// </summary>
        [Range(0f, 1f)] public float steeringThrusterPriorityLevel;
        /// <summary>
        /// Whether pitch and roll is limited within a certain range.
        /// </summary>
        public bool limitPitchAndRoll;
        /// <summary>
        /// The maximum allowed pitch (in degrees). Only relevant when limitPitchAndRoll is set to true.
        /// </summary>
        [Range(0f, 75f)] public float maxPitch;
        /// <summary>
        /// The maximum allowed roll (in degrees) when turning. Only relevant when limitPitchAndRoll is set to true.
        /// </summary>
        [Range(0f, 75f)] public float maxTurnRoll;
        /// <summary>
        /// How quickly the ship pitches (in degrees per second) when pitch and roll is limited to a certain range. Only relevant when limitPitchAndRoll is set to true.
        /// </summary>
        [Range(10f, 90f)] public float pitchSpeed;
        /// <summary>
        /// How quickly the ship rolls (in degrees per second) when pitch and roll is limited to a certain range. Only relevant when limitPitchAndRoll is set to true.
        /// </summary>
        [Range(10f, 90f)] public float turnRollSpeed;
        /// <summary>
        /// The mode currently being used for controlling roll. Only relevant when limitPitchAndRoll is set to true. 
        /// Yaw Input: The ship's roll is dependent on the yaw input.
        /// Strafe Input: The ship's roll is dependent on the left-right strafe input.
        /// </summary>
        public RollControlMode rollControlMode;
        /// <summary>
        /// How quickly the ship pitches and rolls to match the target pitch and roll. Only relevant when limitPitchAndRoll is set to true.
        /// </summary>
        [Range(1f, 10f)] public float pitchRollMatchResponsiveness;
        /// <summary>
        /// Whether the ship will attempt to maintain a constant distance from the detectable ground surface.
        /// </summary>
        public bool stickToGroundSurface;
        /// <summary>
        /// Helps the ship to avoid crashing into the ground below it.
        /// It overrides vertical input by applying force when below the perpendicular target ground distance using a PID controller.
        /// </summary>
        public bool avoidGroundSurface;
        /// <summary>
        /// Whether the ship will orient itself upwards when a ground surface is not detectable.
        /// </summary>
        public bool orientUpInAir;
        /// <summary>
        /// The distance from the ground the ship will attempt to maintain. Only relevant when stickToGroundSurface is set to true.
        /// </summary>
        public float targetGroundDistance;
        /// <summary>
        /// Whether the movement used for matching the Target Distance from the ground is smoothed. Enable this to prevent the ship
        /// from accelerating too quickly when near the Target Distance. Only relevant when stickToGroundSurface is set to true.
        /// If you modify this, call ReinitialiseGroundMatchVariables().
        /// </summary>
        public bool useGroundMatchSmoothing;
        /// <summary>
        /// Whether the ground match algorithm "looks ahead" to detect obstacles ahead of the ship. 
        /// Only relevant when stickToGroundSurface is set to true.
        /// </summary>
        public bool useGroundMatchLookAhead;
        /// <summary>
        /// The minimum distance from the ground the ship will attempt to maintain. Only relevant when stickToGroundSurface and 
        /// useGroundMatchSmoothing is set to true.
        /// </summary>
        public float minGroundDistance;
        /// <summary>
        /// The minimum distance from the ground the ship will attempt to maintain. Only relevant when stickToGroundSurface and 
        /// useGroundMatchSmoothing is set to true.
        /// </summary>
        public float maxGroundDistance;
        /// <summary>
        /// The maximum distance to check for the ground from the bottom of the ship via raycast. 
        /// Only relevant when stickToGroundSurface is set to true.
        /// </summary>
        [Range(0f, 100f)] public float maxGroundCheckDistance;
        /// <summary>
        /// How responsive the ship is to sudden changes in the distance to the ground. 
        /// Increasing this value will allow the ship to match the Target Distance more closely but may lead to a juddering effect 
        /// if increased too much. Only relevant when stickToGroundSurface is set to true.
        /// If you modify this, call ReinitialiseGroundMatchVariables().
        /// </summary>
        [Range(1f, 100f)] public float groundMatchResponsiveness;
        /// <summary>
        /// How much the up/down motion of the ship is damped when attempting to match the Target Distance.
        /// Increasing this value will reduce overshoot but may make the movement too rigid if increased too much. 
        /// Only relevant when stickToGroundSurface is set to true.
        /// If you modify this, call ReinitialiseGroundMatchVariables().
        /// </summary>
        [Range(1f, 100f)] public float groundMatchDamping;
        /// <summary>
        /// The limit to how quickly the ship can accelerate to maintain the Target Distance to the ground. 
        /// Increasing this value will allow the ship to match the Target Distance more closely but may look less natural.
        /// If you modify this, call ReinitialiseGroundMatchVariables().
        /// </summary>
        [Range(1f, 4f)] public float maxGroundMatchAccelerationFactor;
        /// <summary>
        /// The limit (when at the Target Distance from the ground) to how quickly the ship can accelerate to maintain the Target 
        /// Distance to the ground. Increasing this value will allow the ship to match the Target Distance more closely but may 
        /// look less natural. Only relevant when stickToGroundSurface and useGroundMatchSmoothing is set to true. 
        /// If you modify this, call ReinitialiseGroundMatchVariables().
        /// </summary>
        public float centralMaxGroundMatchAccelerationFactor;
        /// <summary>
        /// How the normal direction (orientation) is determined. 
        /// When Single Normal is selected, the single normal of each face of the ground geometry is used. 
        /// When Smoothed Normal is selected, the normals on each vertex of the face of the ground geometry are blended together to give a smoothed normal, which is used instead. 
        /// Smoothed Normal is more computationally expensive.
        /// </summary>
        public GroundNormalCalculationMode groundNormalCalculationMode;
        /// <summary>
        /// The number of past frames (including this frame) used to average ground normals over. Increase this value to make pitch and roll
        /// movement smoother. Decrease this value to make pitch and roll movement more responsive to changes in the ground
        /// surface. Only relevant when stickToGroundSurface is set to true. 
        /// If you modify this, call ReinitialiseGroundMatchVariables().
        /// </summary>
        [Range(1, 50)] public int groundNormalHistoryLength;
        /// <summary>
        /// Layermask determining which layers will be detected as part of the ground surface when raycasted against.
        /// </summary>
        public LayerMask groundLayerMask;
        /// <summary>
        /// The input axis to be controlled or limited. This is used to simulate 2.5D flight.
        /// </summary>
        public InputControlAxis inputControlAxis;
        /// <summary>
        /// The target value of the inputControlAxis.
        /// </summary>
        public float inputControlLimit;
        /// <summary>
        /// The rate at which force is applied to limit control
        /// </summary>
        [Range(0.1f, 100f)] public float inputControlMovingRigidness;
        /// <summary>
        /// The rate at which the ship turns to limit or correct the rotation
        /// </summary>
        [Range(0.1f, 100f)] public float inputControlTurningRigidness;
        /// <summary>
        /// Forward X angle for the plane the ship will fly in
        /// </summary>
        [Range(0f, 180f)] public float inputControlForwardAngle;

        // Arcade physics properties
        /// <summary>
        /// How fast the ship accelerates when pitching up and down in degrees per second squared. 
        /// Increasing this value will increase how fast the ship can be pitched up and down by pilot and rotational flight assist inputs.
        /// </summary>
        [Range(0f, 500f)] public float arcadePitchAcceleration;
        /// <summary>
        /// How fast the ship accelerates when turning left and right in degrees per second squared. 
        /// Increasing this value will increase how fast the ship can be turned left and right by pilot and rotational flight assist inputs.
        /// </summary>
        [Range(0f, 500f)] public float arcadeYawAcceleration;
        /// <summary>
        /// How fast the ship accelerates when rolling left and right in degrees per second squared. 
        /// Increasing this value will increase how fast the ship can be rolled left and right by pilot and rotational flight assist inputs.
        /// </summary>
        [Range(0f, 500f)] public float arcadeRollAcceleration;
        /// <summary>
        /// How quickly the ship accelerates in metres per second squared while in the air to move in the direction the ship is facing.
        /// </summary>
        [Range(0f, 1000f)] public float arcadeMaxFlightTurningAcceleration;
        /// <summary>
        /// How quickly the ship accelerates in metres per second squared while near the ground to move in the direction the ship is facing.
        /// </summary>
        [Range(0f, 1000f)] public float arcadeMaxGroundTurningAcceleration;

        /// <summary>
        /// [INTERNAL USE ONLY] shouldn't be called directly, call SendInput in the ship control module instead
        /// </summary>
        public Vector3 pilotForceInput { private get; set; }
        /// <summary>
        /// [INTERNAL USE ONLY] shouldn't be called directly, call SendInput in the ship control module instead
        /// </summary>
        public Vector3 pilotMomentInput { private get; set; }
        /// <summary>
        /// [INTERNAL USE ONLY] shouldn't be called directly, call SendInput in the ship control module instead
        /// </summary>
        public bool pilotPrimaryFireInput { private get; set; }
        /// <summary>
        /// [INTERNAL USE ONLY] shouldn't be called directly, call SendInput in the ship control module instead
        /// </summary>
        public bool pilotSecondaryFireInput { private get; set; }

        /// <summary>
        /// The density (in kilograms per cubic metre) of the fluid the ship is travelling through (usually air).
        /// </summary>
        public float mediumDensity;
        /// <summary>
        /// The magnitude of the acceleration (in metres per second squared) due to gravity.
        /// </summary>
        public float gravitationalAcceleration;
        /// <summary>
        /// The direction in world space in which gravity acts upon the ship.
        /// </summary>
        public Vector3 gravityDirection;

        // TODO: Update comment when adding more damage models
        /// <summary>
        /// What damage model the ship uses.
        /// Simple: Simplistic damage model with a single health value. Effects of damage are only visual until the ship is destroyed.
        /// Progressive: More complex damage model. As the ship takes damage the performance of parts is affected.
        /// Localised: More complex damage model. Similar to progressive, but different parts can be damaged independently of each other.
        /// If you modify this, call ReinitialiseDamageRegionVariables().
        /// </summary>
        public ShipDamageModel shipDamageModel;

        /// <summary>
        /// Damage Region used in the simple and progressive damage models. 
        /// If you modify this, call ReinitialiseDamageRegionVariables().
        /// NOTE: Not required if just changing the Health or ShieldHealth
        /// </summary>
        public DamageRegion mainDamageRegion;
        /// <summary>
        /// List of Damage Regions used in the localised damage model. 
        /// If you modify this, call ReinitialiseDamageRegionVariables().
        /// NOTE: Not required if just changing the Health or ShieldHealth
        /// </summary>
        public List<DamageRegion> localisedDamageRegionList;

        /// <summary>
        /// Whether damage multipliers are used when calculating damage from projectiles.
        /// </summary>
        public bool useDamageMultipliers;
        /// <summary>
        /// Whether damage multipliers are localised (i.e. there are different sets of damage multipliers for each damage region
        /// of the ship). Only relevant when useDamageMultipliers is set to true and shipDamageModel is set to Localised.
        /// </summary>
        public bool useLocalisedDamageMultipliers;

        /// <summary>
        /// [READONLY]
        /// Normalised (0.0 – 1.0) value of the overall health of the ship. To get the actual health values, see the damageRegions of the ship.
        /// </summary>
        public float HealthNormalised
        {
            get
            {
                float _healthN = mainDamageRegion.startingHealth == 0f ? 0f : mainDamageRegion.Health / mainDamageRegion.startingHealth;
                if (_healthN > 1f) { return 1f; }
                else if (_healthN < 0f) { return 0f; }
                else { return _healthN; }
            }
        }

        /// <summary>
        /// How respawning happens. If you change this to RespawningMode.RespawnAtLastPosition, call ReinitialiseRespawnVariables().
        /// </summary>
        public RespawningMode respawningMode;
        /// <summary>
        /// How long the respawning process takes (in seconds). Only relevant when respawningMode is not set to DontRespawn.
        /// </summary>
        public float respawnTime;
        /// <summary>
        /// The time (in seconds) between updates of the collision respawn position. Hence when the ship is destroyed by colliding with
        /// something, the ship respawn position will be where the ship was between this time ago and twice this time ago. Only
        /// relevant when respawningMode is set to RespawnAtLastPosition.
        /// </summary>
        public float collisionRespawnPositionDelay;
        /// <summary>
        /// Where the ship respawns from in world space when respawningMode is set to RespawnFromSpecifiedPosition.
        /// </summary>
        public Vector3 customRespawnPosition;
        /// <summary>
        /// The rotation the ship respawns from in world space when respawningMode is set to RespawnFromSpecifiedPosition.
        /// </summary>
        public Vector3 customRespawnRotation;
        /// <summary>
        /// The velocity the ship respawns with in local space. Only relevant when respawningMode is not set to DontRespawn.
        /// </summary>
        public Vector3 respawnVelocity;
        /// <summary>
        /// guidHash code of the Path which the ship will be respawned on when respawningMode is RespawnOnPath.
        /// </summary>
        public int respawningPathGUIDHash;
        /// <summary>
        /// [READONLY] The current respawn position
        /// </summary>
        public Vector3 RespawnPosition { get { return currentRespawnPosition; } }

        /// <summary>
        /// Whether controller rumble is applied to the ship by the ship control module.
        /// </summary>
        public bool applyControllerRumble = false;
        /// <summary>
        /// The minimum amount of damage that will cause controller rumble.
        /// </summary>
        public float minRumbleDamage = 0f;
        /// <summary>
        /// The amount of damage corresponding to maximum controller rumble.
        /// </summary>
        public float maxRumbleDamage = 10f;
        /// <summary>
        /// The time (in seconds) that a controller rumble event lasts for.
        /// </summary>
        [Range(0.1f, 5f)] public float damageRumbleTime = 0.5f;

        /// <summary>
        /// The amount of time that needs to elapse before a stationary ship is considered stuck.
        /// When the value is 0, a stationary ship is never considered stuck.
        /// </summary>
        [Range(0f, 300)] public float stuckTime;
        /// <summary>
        /// The maximum speed in m/sec the ship can be moving before it can be considered stuck
        /// </summary>
        [Range(0f, 1f)] public float stuckSpeedThreshold;
        /// <summary>
        /// The action to take when the ship is deemed stationary or stuck.
        /// </summary>
        public StuckAction stuckAction;
        /// <summary>
        /// guidHash code of the Path which the ship will be respawned on when it is stuck
        /// and the StuckAction is RespawnOnPath.
        /// </summary>
        public int stuckActionPathGUIDHash;

        /// <summary>
        /// [READONLY] The world velocity of the ship as a vector. Derived from the velocity of the rigidbody.
        /// </summary>
        public Vector3 WorldVelocity { get { return worldVelocity; } }

        /// <summary>
        /// [READONLY] The local velocity of the ship as a vector. Derived from the velocity of the rigidbody.
        /// </summary>
        public Vector3 LocalVelocity { get { return localVelocity; } }

        /// <summary>
        /// [READONLY] The world angular velocity of the ship as a vector. Derived from the angular velocity of the rigidbody.
        /// </summary>
        public Vector3 WorldAngularVelocity { get { return worldAngularVelocity; } }

        /// <summary>
        /// [READONLY] The local angular velocity of the ship as a vector. Derived from the angular velocity of the rigidbody.
        /// </summary>
        public Vector3 LocalAngularVelocity { get { return localAngularVelocity; } }

        /// <summary>
        /// [READONLY] The pitch angle, in degrees, above or below the artificial horizon
        /// </summary>
        public float PitchAngle { get { return Vector3.SignedAngle(new Vector3(trfmFwd.x, 0f, trfmFwd.z), trfmFwd, Vector3.right) * Mathf.Sign(-Vector3.Dot(Vector3.forward, trfmFwd)); } }

        /// <summary>
        /// [READONLY] The roll angle, in degrees, left (-ve) or right (+ve)
        /// </summary>
        public float RollAngle { get { Vector3 _localUpNormal = trfmInvRot * Vector3.up; return -Mathf.Atan2(_localUpNormal.x, _localUpNormal.y) * Mathf.Rad2Deg; } }

        /// <summary>
        /// [READONLY] The rigidbody position of the ship as a vector. Derived from the position of the rigidbody. This is
        /// where the physics engine says the ship is.
        /// </summary>
        public Vector3 RigidbodyPosition { get { return rbodyPos; } }

        /// <summary>
        /// [READONLY] The rigidbody rotation of the ship as a vector. Derived from the rotation of the rigidbody. This is
        /// where the physics engine says the ship is rotated.
        /// </summary>
        public Quaternion RigidbodyRotation { get { return rbodyRot; } }

        /// <summary>
        /// [READONLY] The rigidbody inverse rotation of the ship as a vector. Derived from the rotation of the rigidbody. This is
        /// the inverse of where the physics engine says the ship is rotated.
        /// </summary>
        public Quaternion RigidbodyInverseRotation { get { return rbodyInvRot; } }

        /// <summary>
        /// [READONLY] The rigidbody forward direction of the ship as a vector. Derived from the rotation of the rigidbody. This is
        /// the direction the physics engine says the ship is facing.
        /// </summary>
        public Vector3 RigidbodyForward { get { return rbodyFwd; } }

        /// <summary>
        /// [READONLY] The rigidbody right direction of the ship as a vector. Derived from the rotation of the rigidbody. This is
        /// the direction the physics engine says the ship's right direction is facing.
        /// </summary>
        public Vector3 RigidbodyRight { get { return rbodyRight; } }

        /// <summary>
        /// [READONLY] The rigidbody up direction of the ship as a vector. Derived from the rotation of the rigidbody. This is
        /// the direction the physics engine says the ship's up direction is facing.
        /// </summary>
        public Vector3 RigidbodyUp { get { return rbodyUp; } }

        /// <summary>
        /// [READONLY] The position of the ship as a vector. Derived from the position of the transform. You should use 
        /// RigidbodyPosition instead if you are using the data for physics calculations.
        /// </summary>
        public Vector3 TransformPosition { get { return trfmPos; } }

        /// <summary>
        /// [READONLY] The forward direction of the ship in world space as a vector. Derived from the forward direction of the transform. 
        /// You should use RigidbodyForward instead if you are using the data for physics calculations.
        /// </summary>
        public Vector3 TransformForward { get { return trfmFwd; } }

        /// <summary>
        /// [READONLY] The right direction of the ship in world space as a vector. Derived from the right direction of the transform.
        /// You should use RigidbodyRight instead if you are using the data for physics calculations.
        /// </summary>
        public Vector3 TransformRight { get { return trfmRight; } }

        /// <summary>
        /// [READONLY] The up direction of the ship in world space as a vector. Derived from the up direction of the transform.
        /// You should use RigidbodyUp instead if you are using the data for physics calculations.
        /// </summary>
        public Vector3 TransformUp { get { return trfmUp; } }

        /// <summary>
        /// [READONLY] The rotation of the ship in world space as a quaternion. Derived from the rotation of the transform.
        /// You should use RigidbodyRotation instead if you are using the data for physics calculations.
        /// </summary>
        public Quaternion TransformRotation { get { return trfmRot; } }

        /// <summary>
        /// [READONLY] The inverse rotation of the ship in world space as a quaternion. Derived from the rotation of the transform.
        /// You should use RigidbodyInverseRotation instead if you are using the data for physics calculations.
        /// </summary>
        public Quaternion TransformInverseRotation { get { return trfmInvRot; } }

        /// <summary>
        /// [READONLY] Whether the ship is currently sticking to a ground surface.
        /// </summary>
        public bool IsGrounded { get { return stickToGroundSurfaceEnabled; } }

        /// <summary>
        /// [READONLY] The current normal of the target plane in world space. This is the upwards direction the ship will attempt to
        /// orient itself to if limit pitch and roll is enabled.
        /// </summary>
        public Vector3 WorldTargetPlaneNormal { get { return worldTargetPlaneNormal; } }

        /// <summary>
        /// The faction or alliance the ship belongs to. This can be used to identify if a ship is friend or foe.
        /// Default (neutral) is 0.
        /// </summary>
        public int factionId;

        /// <summary>
        /// The squadron this ship is a member of.
        /// </summary>
        public int squadronId;

        /// <summary>
        /// The relative size of the blip on the radar mini-map
        /// </summary>
        [Range(1, 5)] public byte radarBlipSize;

        /// <summary>
        /// [INTERNAL USE ONLY] Instead, call shipControlModule.EnableRadar() or DisableRadar().
        /// </summary>
        public bool isRadarEnabled;

        /// <summary>
        /// [READONLY] The number used by the SSCRadar system to identify this ship at a point in time.
        /// This should not be stored across frames and is updated as required by the system.
        /// </summary>
        public int RadarId { get { return radarItemIndex; } }

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] public SSCRadarPacket sscRadarPacket;

        /// <summary>
        /// Session-only transform InstanceID
        /// </summary>
        [System.NonSerialized] public int shipId;

        /// <summary>
        /// Session-only estimated maximum range of turret weapons
        /// </summary>
        [System.NonSerialized] public float estimatedMaxTurretRange;

        /// <summary>
        /// Session-only estimated maximum range of all weapons with isAutoTargetingEnabled = true
        /// </summary>
        [System.NonSerialized] public float estimatedMaxAutoTargetRange;

        #endregion

        #region Public Delegates

        public delegate void CallbackOnDamage(float mainDamageRegionHealth);
        public delegate void CallbackOnCameraShake(float shakeAmountNormalised);
        public delegate void CallbackOnWeaponFired(Ship ship, Weapon weapon);
        public delegate void CallbackOnWeaponNoAmmo(Ship ship, Weapon weapon);        

        /// <summary>
        /// The name of the custom method that is called immediately
        /// after damage has changed. Your method must take 1 float
        /// parameter. This should be a lightweight method to avoid
        /// performance issues. It could be used to update a HUD or
        /// take some other action.
        /// </summary>
        [NonSerialized] public CallbackOnDamage callbackOnDamage = null;

        /// <summary>
        /// Generally reserved for internal use by ShipCameraModule. If you use your own
        /// camera scripts, you can create a lightweight custom method and assign it at
        /// runtime so that this is called whenever camera shake data is available.
        /// </summary>
        [NonSerialized] public CallbackOnCameraShake callbackOnCameraShake = null;

        /// <summary>
        /// The name of the custom method that is called immediately after
        /// a weapon has fired.
        /// This should be a lightweight method to avoid performance issues that
        /// doesn't hold references the ship or weapon past the end of the current frame.
        /// </summary>
        [NonSerialized] public CallbackOnWeaponFired callbackOnWeaponFired = null;

        /// <summary>
        /// The name of the custom method that is called immediately after the weapon
        /// runs out of ammunition. Your method must take a ship and weapon parameter.
        /// This should be a lightweight method to avoid performance issues that
        /// doesn't hold references the ship or weapon past the end of the current frame.
        /// </summary>
        [NonSerialized] public CallbackOnWeaponNoAmmo callbackOnWeaponNoAmmo = null;
        #endregion

        #region Private and Internal Variables

        #region Private Variables - Cached
        private Vector3 worldVelocity;
        private Vector3 localVelocity;
        private Vector3 absLocalVelocity;
        private Vector3 worldAngularVelocity;
        private Vector3 localAngularVelocity;
        private Vector3 trfmUp;
        private Vector3 trfmFwd;
        private Vector3 trfmRight;
        private Vector3 trfmPos;
        private Vector3 rbodyPos;
        private Vector3 rbodyUp;
        private Vector3 rbodyFwd;
        private Vector3 rbodyRight;
        private Quaternion trfmRot;
        private Quaternion trfmInvRot;
        private Quaternion rbodyRot;
        private Quaternion rbodyInvRot;
        private Vector3 rBodyInertiaTensor;
        private bool shipPhysicsModelPhysicsBased;
        private bool shipDamageModelIsSimple;

        #endregion

        #region Private Variables - Timing related
        private float lastFixedUpdateTime = 0f;
        private Vector3 previousFrameWorldVelocity = Vector3.zero;
        internal float thrusterSystemStartupTimer = 0f;
        internal float thrusterSystemShutdownTimer = 0f;
        #endregion

        #region Private Variables - Cached input
        // Is each input being used on the ship?
        private bool longitudinalThrustInputUsed;
        private bool horizontalThrustInputUsed;
        private bool verticalThrustInputUsed;
        private bool pitchMomentInputUsed;
        private bool yawMomentInputUsed;
        private bool rollMomentInputUsed;
        #endregion

        #region Private Variables - Input
        private bool limitPitchAndRollEnabled = false;
        private bool stickToGroundSurfaceEnabled = false;
        private Vector3 forceInput = Vector3.zero;
        private Vector3 momentInput = Vector3.zero;
        private float pitchAmountInput = 0f, rollAmountInput = 0f;
        private float pitchAmount = 0f, rollAmount = 0f;
        private float maxFramePitchDelta, maxFrameRollDelta;
        private float targetLocalPitch, targetLocalRoll;
        private float yawVelocity;
        private float flightAssistValue;
        #endregion

        #region Private Variables - Thruster
        private Thruster thruster;
        private Vector3 thrustForce = Vector3.zero;
        #endregion

        #region Private Variables - Aerodynamics
        private float dragLength, dragWidth, dragHeight;
        private float quarticLengthXProj, quarticLengthYProj;
        private float quarticWidthYProj, quarticWidthZProj;
        private float quarticHeightXProj, quarticHeightZProj;
        private Vector3 localDragForce;
        private Vector3 localLiftForce;
        private Vector3 localInducedDragForce;
        private float liftForceMagnitude;
        private Wing wing;
        private Vector3 wingPlaneNormal = Vector3.right;
        private Vector3 localVeloInWingPlane = Vector3.zero;
        private float wingAngleOfAttack = 0f;
        private float angularDragMultiplier;
        private ControlSurface controlSurface;
        private float aerodynamicTrueAirspeed;
        private float liftCoefficient = 0f, phi0, phi1, phi2, phi3;
        private Vector3 controlSurfaceMovementAxis = Vector3.up;
        private float controlSurfaceInput = 0f, controlSurfaceAngle;
        private float controlSurfaceLiftDelta, controlSurfaceDragDelta;
        private Vector3 controlSurfaceRelativePosition;
        #endregion

        #region Private Variables - Gravity
        private Vector3 localGravityAcceleration;
        #endregion

        #region Private Variables - Arcade physics
        /// <summary>
        /// Note that this is actually an acceleration.
        /// </summary>
        private Vector3 arcadeMomentVector = Vector3.zero;
        /// <summary>
        /// Note that this is actually an acceleration.
        /// </summary>
        private Vector3 arcadeForceVector = Vector3.zero;
        private float arcadeBrakeForceMagnitude = 0f;
        private float arcadeBrakeForceMinMagnitude = 0f;
        private float maxGroundMatchAcceleration = 0f;
        private float centralMaxGroundMatchAcceleration = 0f;
        // Temp ground distance input required
        private float groundDistInput = 0f;
        #endregion

        #region Private Variables - Combat variables
        private Weapon weapon;
        private Vector3 weaponRelativeFirePosition = Vector3.zero;
        private Vector3 weaponRelativeFireDirection = Vector3.zero;
        private Vector3 weaponWorldBasePosition = Vector3.zero;
        private Vector3 weaponWorldFirePosition = Vector3.zero;
        private Vector3 weaponWorldFireDirection = Vector3.zero;
        private Vector3 currentRespawnPosition = Vector3.zero;
        private Quaternion currentRespawnRotation = Quaternion.identity;
        private Vector3 currentCollisionRespawnPosition = Vector3.zero;
        private Vector3 nextCollisionRespawnPosition = Vector3.zero;
        private Quaternion currentCollisionRespawnRotation = Quaternion.identity;
        private Quaternion nextCollisionRespawnRotation = Quaternion.identity;
        private float collisionRespawnPositionTimer = 0f;
        private bool lastDamageWasCollisionDamage = false;
        private int weaponFiringButtonInt;
        private int weaponPrimaryFiringInt, weaponSecondaryFiringInt, weaponAutoFiringInt;
        private int weaponTypeInt;
        private int lastDamageEventIndex = -1;
        private float damageRumbleAmountRequired = 0f;
        private float damageCameraShakeAmountRequired = 0f;

        // Damage Regions
        [System.NonSerialized] internal int numLocalisedDamageRegions;

        // Radar variables
        [System.NonSerialized] internal int radarItemIndex = -1;

        #endregion

        #region Private Variables - Collision
        
        /// <summary>
        /// Unordered unique set of colliders on objects that have been attached to this ship.
        /// These are ignored during collision events
        /// </summary>
        [NonSerialized] private HashSet<int> attachedCollisionColliders = new HashSet<int>();
        #endregion

        #region Private Variables - PID Controllers
        private PIDController pitchPIDController;
        private PIDController rollPIDController;
        private PIDController groundDistPIDController;
        private PIDController rFlightAssistPitchPIDController;
        private PIDController rFlightAssistYawPIDController;
        private PIDController rFlightAssistRollPIDController;
        private PIDController inputAxisForcePIDController;
        private PIDController inputAxisMomentPIDController;
        private PIDController sFlightAssistPitchPIDController;
        private PIDController sFlightAssistYawPIDController;
        private PIDController sFlightAssistRollPIDController;
        private float sFlightAssistTargetPitch = 0f;
        private float sFlightAssistTargetYaw = 0f;
        private float sFlightAssistTargetRoll = 0f;
        private bool sFlightAssistRotatingPitch = false;
        private bool sFlightAssistRotatingYaw = false;
        private bool sFlightAssistRotatingRoll = false;
        #endregion

        #region Private Variables - Normals
        private Vector3 worldTargetPlaneNormal = Vector3.up;
        private Vector3 localTargetPlaneNormal = Vector3.up;
        private float currentPerpendicularGroundDist = -1f;
        private MeshCollider groundMeshCollider;
        private MeshCollider cachedGroundMeshCollider;
        private Mesh cachedGroundMesh;
        private Vector3[] cachedGroundMeshNormals;
        private int[] cachedGroundMeshTriangles;
        private Vector3 groundNormal0;
        private Vector3 groundNormal1;
        private Vector3 groundNormal2;
        private Vector3 barycentricCoord;
        private Vector3[] groundNormalHistory;
        private int groundNormalHistoryIdx = 0;
        #endregion

        #region Private and Internal Variables - Misc

        private int componentIndex;
        private int componentListSize;

        private Ray raycastRay;
        private RaycastHit raycastHitInfo;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Added in 1.3.3 to support muzzle FX parenting.
        /// MAY NEED TO BE REMOVED IN THE FUTURE.
        /// WARNING: Not fully tested with destroying ships
        /// in the scene. Need to be careful it doesn't prevent
        /// garabage collection of ShipControlModule.
        /// Where possible - use the cached trfmXXX itema above.
        /// </summary>
        [System.NonSerialized] internal Transform shipTransform;

        private SSCManager sscManager = null;

        // Reusable lists - typically used with GetComponents or GetComponentsInChildren to avoid GC.
        //private List<ShipControlModule> tempShipControlModuleList;

        // Add additional force or boost
        private Vector3 boostDirection;
        private float boostTimer;
        private float boostForce;  // in newtons

        #endregion

        #endregion

        #region Constructors

        // Class constructor
        public Ship()
        {
            SetClassDefaults();
        }

        // Copy constructor
        public Ship (Ship ship)
        {
            if (ship == null) { SetClassDefaults(); }
            else
            {
                this.initialiseOnAwake = ship.initialiseOnAwake;
                this.shipPhysicsModel = ship.shipPhysicsModel;
                this.mass = ship.mass;
                this.centreOfMass = ship.centreOfMass;
                this.setCentreOfMassManually = ship.setCentreOfMassManually;
                this.showCOMGizmosInSceneView = ship.showCOMGizmosInSceneView;
                this.showCOLGizmosInSceneView = ship.showCOLGizmosInSceneView;
                this.showCOTGizmosInSceneView = ship.showCOTGizmosInSceneView;

                if (ship.thrusterList == null)
                {
                    this.thrusterList = new List<Thruster>(25);

                    // Create a default forwards-facing thruster
                    Thruster thruster = new Thruster();
                    if (thruster != null)
                    {
                        thruster.name = "Forward Thruster";
                        thruster.forceUse = 1;
                        thruster.primaryMomentUse = 0;
                        thruster.secondaryMomentUse = 0;
                        thruster.maxThrust = 100000;
                        thruster.thrustDirection = Vector3.forward;
                        thruster.relativePosition = Vector3.zero;
                        this.thrusterList.Add(thruster);
                    }
                }
                else { this.thrusterList = ship.thrusterList.ConvertAll(th => new Thruster(th)); }

                if (ship.wingList == null) { this.wingList = new List<Wing>(5); }
                else { this.wingList = ship.wingList.ConvertAll(wg => new Wing(wg)); }

                if (ship.controlSurfaceList == null) { this.controlSurfaceList = new List<ControlSurface>(10); }
                else { this.controlSurfaceList = ship.controlSurfaceList.ConvertAll(cs => new ControlSurface(cs)); }

                if (ship.weaponList == null) { this.weaponList = new List<Weapon>(10); }
                else { this.weaponList = ship.weaponList.ConvertAll(wp => new Weapon(wp)); }

                this.isThrusterSystemsExpanded = ship.isThrusterSystemsExpanded;
                this.isThrusterListExpanded = ship.isThrusterListExpanded;
                this.isWingListExpanded = ship.isWingListExpanded;
                this.isControlSurfaceListExpanded = ship.isControlSurfaceListExpanded;
                this.isWeaponListExpanded = ship.isWeaponListExpanded;
                this.isMainDamageExpanded = ship.isMainDamageExpanded;
                this.isDamageListExpanded = ship.isDamageListExpanded;
                this.showDamageInEditor = ship.showDamageInEditor;

                this.isThrusterFXStationary = ship.isThrusterFXStationary;
                this.isThrusterSystemsStarted = ship.isThrusterSystemsStarted;
                this.thrusterSystemStartupDuration = ship.thrusterSystemStartupDuration;
                this.thrusterSystemShutdownDuration = ship.thrusterSystemShutdownDuration;
                this.useCentralFuel = ship.useCentralFuel;
                this.centralFuelLevel = ship.centralFuelLevel;

                this.dragCoefficients = ship.dragCoefficients;
                this.dragReferenceAreas = ship.dragReferenceAreas;
                this.centreOfDragXMoment = ship.centreOfDragXMoment;
                this.centreOfDragYMoment = ship.centreOfDragYMoment;
                this.centreOfDragZMoment = ship.centreOfDragZMoment;
                this.angularDragFactor = ship.angularDragFactor;
                this.disableDragMoments = ship.disableDragMoments;
                this.dragXMomentMultiplier = ship.dragXMomentMultiplier;
                this.dragYMomentMultiplier = ship.dragYMomentMultiplier;
                this.dragZMomentMultiplier = ship.dragZMomentMultiplier;
                this.wingStallEffect = ship.wingStallEffect;

                this.arcadeUseBrakeComponent = ship.arcadeUseBrakeComponent;
                this.arcadeBrakeStrength = ship.arcadeBrakeStrength;
                this.arcadeBrakeIgnoreMediumDensity = ship.arcadeBrakeIgnoreMediumDensity;
                this.arcadeBrakeMinAcceleration = ship.arcadeBrakeMinAcceleration;

                this.rotationalFlightAssistStrength = ship.rotationalFlightAssistStrength;
                this.translationalFlightAssistStrength = ship.translationalFlightAssistStrength;
                this.brakeFlightAssistStrength = ship.brakeFlightAssistStrength;
                this.brakeFlightAssistMinSpeed = ship.brakeFlightAssistMinSpeed;
                this.brakeFlightAssistMaxSpeed = ship.brakeFlightAssistMaxSpeed;
                this.brakeFlightAssistStrengthX = ship.brakeFlightAssistStrengthX;
                this.brakeFlightAssistStrengthY = ship.brakeFlightAssistStrengthY;

                this.limitPitchAndRoll = ship.limitPitchAndRoll;
                this.maxPitch = ship.maxPitch;
                this.maxTurnRoll = ship.maxTurnRoll;
                this.pitchSpeed = ship.pitchSpeed;
                this.turnRollSpeed = ship.turnRollSpeed;
                this.rollControlMode = ship.rollControlMode;
                this.pitchRollMatchResponsiveness = ship.pitchRollMatchResponsiveness;
                this.stickToGroundSurface = ship.stickToGroundSurface;
                this.avoidGroundSurface = ship.avoidGroundSurface;
                this.useGroundMatchSmoothing = ship.useGroundMatchSmoothing;
                this.useGroundMatchLookAhead = ship.useGroundMatchLookAhead;
                this.orientUpInAir = ship.orientUpInAir;
                this.targetGroundDistance = ship.targetGroundDistance;
                this.minGroundDistance = ship.minGroundDistance;
                this.maxGroundDistance = ship.maxGroundDistance;
                this.maxGroundCheckDistance = ship.maxGroundCheckDistance;
                this.groundMatchResponsiveness = ship.groundMatchResponsiveness;
                this.groundMatchDamping = ship.groundMatchDamping;
                this.maxGroundMatchAccelerationFactor = ship.maxGroundMatchAccelerationFactor;
                this.centralMaxGroundMatchAccelerationFactor = ship.centralMaxGroundMatchAccelerationFactor;
                this.groundNormalCalculationMode = ship.groundNormalCalculationMode;
                this.groundNormalHistoryLength = ship.groundNormalHistoryLength;
                this.groundLayerMask = ship.groundLayerMask;

                this.inputControlAxis = ship.inputControlAxis;
                this.inputControlLimit = ship.inputControlLimit;
                this.inputControlForwardAngle = ship.inputControlForwardAngle;
                this.inputControlMovingRigidness = ship.inputControlMovingRigidness;
                this.inputControlTurningRigidness = ship.inputControlTurningRigidness;

                this.rollPower = ship.rollPower;
                this.pitchPower = ship.pitchPower;
                this.yawPower = ship.yawPower;
                this.steeringThrusterPriorityLevel = ship.steeringThrusterPriorityLevel;

                this.pilotMomentInput = ship.pilotMomentInput;
                this.pilotForceInput = ship.pilotForceInput;
                this.arcadePitchAcceleration = ship.arcadePitchAcceleration;
                this.arcadeYawAcceleration = ship.arcadeYawAcceleration;
                this.arcadeRollAcceleration = ship.arcadeRollAcceleration;
                this.arcadeMaxFlightTurningAcceleration = ship.arcadeMaxFlightTurningAcceleration;
                this.arcadeMaxGroundTurningAcceleration = ship.arcadeMaxGroundTurningAcceleration;

                this.mediumDensity = ship.mediumDensity;
                this.gravitationalAcceleration = ship.gravitationalAcceleration;
                this.gravityDirection = ship.gravityDirection;

                this.shipDamageModel = ship.shipDamageModel;
                if (ship.mainDamageRegion == null)
                {
                    this.mainDamageRegion = new DamageRegion();
                    this.mainDamageRegion.name = "Main Damage Region";
                }
                else { this.mainDamageRegion = new DamageRegion(ship.mainDamageRegion); }

                if (ship.localisedDamageRegionList == null) { this.localisedDamageRegionList = new List<DamageRegion>(10); }
                else { this.localisedDamageRegionList = ship.localisedDamageRegionList.ConvertAll(dr => new DamageRegion(dr)); }

                this.useDamageMultipliers = ship.useDamageMultipliers;
                this.useLocalisedDamageMultipliers = ship.useLocalisedDamageMultipliers;

                this.respawnTime = ship.respawnTime;
                this.collisionRespawnPositionDelay = ship.collisionRespawnPositionDelay;
                this.respawningMode = ship.respawningMode;
                this.customRespawnPosition = ship.customRespawnPosition;
                this.customRespawnRotation = ship.customRespawnRotation;
                this.respawnVelocity = ship.respawnVelocity;
                this.respawningPathGUIDHash = ship.respawningPathGUIDHash;
                this.minRumbleDamage = ship.minRumbleDamage;
                this.maxRumbleDamage = ship.maxRumbleDamage;
                this.stuckTime = ship.stuckTime;
                this.stuckSpeedThreshold = ship.stuckSpeedThreshold;
                this.stuckAction = ship.stuckAction;
                this.stuckActionPathGUIDHash = ship.stuckActionPathGUIDHash;
                this.factionId = ship.factionId;
                this.squadronId = ship.squadronId;

                this.isRadarEnabled = ship.isRadarEnabled;
                this.radarBlipSize = ship.radarBlipSize;
            }
        }

        #endregion

        #region Private and Internal Non-Static Methods

        /// <summary>
        /// [INTERNAL ONLY]
        /// Check if the thruster systems are starting up or shutting down.
        /// If the state has changed in this frame, like has started or shutdown, the method will return true.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="hasShutdown"></param>
        /// <param name="hasStarted"></param>
        /// <returns></returns>
        internal bool CheckThrusterSystems(float deltaTime, out bool hasStarted, out bool hasShutdown)
        {
            bool hasStateChanged = false;
            hasShutdown = false;
            hasStarted = false;

            // Is the system shutting down?
            if (thrusterSystemShutdownTimer > 0f)
            {
                thrusterSystemShutdownTimer += deltaTime;

                if (thrusterSystemShutdownTimer > thrusterSystemShutdownDuration)
                {
                    ShutdownThrusterSystems(true);
                    hasStateChanged = true;
                    hasShutdown = true;
                }
                else
                {
                    SetThrottleAllThrusters(1f - thrusterSystemShutdownTimer / thrusterSystemShutdownDuration);
                }
            }
            else if (thrusterSystemStartupTimer > 0f)
            {
                thrusterSystemStartupTimer += deltaTime;

                //Debug.Log("[DEBUG] Thruster System starting up: " + thrusterSystemStartupTimer);

                if (thrusterSystemStartupTimer > thrusterSystemStartupDuration)
                {
                    StartupThrusterSystems(true);
                    hasStateChanged = true;
                    hasStarted = true;
                }
                else
                {
                    SetThrottleAllThrusters(thrusterSystemStartupTimer / thrusterSystemStartupDuration);
                }
            }

            return hasStateChanged;
        }

        /// <summary>
        /// Reset the thruster systems variables and thruster throttle values.
        /// The systems might have been starting or shutting down when the ship
        /// was destroyed.
        /// Typically used when a ship is respawned.
        /// </summary>
        internal void ResetThrusterSystems()
        {
            // If the systems are starting or shutting down, reset all the
            // thruster throttle values.
            if (thrusterSystemStartupTimer > 0f || thrusterSystemShutdownTimer > 0f)
            {
                SetThrottleAllThrusters(isThrusterSystemsStarted ? 1f : 0f);
            }

            thrusterSystemShutdownTimer = 0f;
            thrusterSystemStartupTimer = 0f;
        }

        /// <summary>
        /// Returns the angle between two vectors projected into a specified plane, with a sign indicating the direction.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="planeNormal"></param>
        /// <returns></returns>
        private float SignedAngleInPlane(Vector3 from, Vector3 to, Vector3 planeNormal)
        {
            return Vector3.SignedAngle(Vector3.ProjectOnPlane(from, planeNormal), Vector3.ProjectOnPlane(to, planeNormal), planeNormal);
        }

        /// <summary>
        /// Returns a float number raised to an integer power.
        /// </summary>
        /// <param name="baseNum"></param>
        /// <param name="indexNum"></param>
        /// <returns></returns>
        private float IntegerPower(float baseNum, int powerNum)
        {
            // Start power calculation at power of zero instead of power of one, so that anything to the power of zero is one
            float resultNum = 1f;
            // Iteratively mutiply by the base number
            for (int i = 0; i < powerNum; i++) { resultNum *= baseNum; }
            return resultNum;
        }

        /// <summary>
        /// Returns the damped (averaged) normal.
        /// </summary>
        /// <param name="currentNormal"></param>
        /// <returns></returns>
        private Vector3 GetDampedGroundNormal()
        {
            Vector3 dampedNormal = Vector3.zero;

            // Loop through ground normal
            for (int hIdx = 0; hIdx < groundNormalHistoryLength; hIdx++)
            {
                // Optimise by setting each axis rather than creating new vectors
                dampedNormal.x += groundNormalHistory[hIdx].x;
                dampedNormal.y += groundNormalHistory[hIdx].y;
                dampedNormal.z += groundNormalHistory[hIdx].z;
            }

            dampedNormal.x *= 1f / groundNormalHistoryLength;
            dampedNormal.y *= 1f / groundNormalHistoryLength;
            dampedNormal.z *= 1f / groundNormalHistoryLength;

            return dampedNormal.normalized;
        }

        /// <summary>
        /// Update the normal history.
        /// </summary>
        /// <param name="currentNormal"></param>
        private void UpdateGroundNormalHistory(Vector3 currentNormal)
        {
            // Update the current element of the history with the current normal
            if (groundNormalHistoryLength > groundNormalHistoryIdx) { groundNormalHistory[groundNormalHistoryIdx] = currentNormal; }
            // Keeps track of which array position to replace. Avoids having to shuffle elements
            groundNormalHistoryIdx++;
            if (groundNormalHistoryIdx >= groundNormalHistoryLength) { groundNormalHistoryIdx = 0; }
        }

        /// <summary>
        /// Returns a damped max acceleration input.
        /// </summary>
        /// <param name="currentDisplacement"></param>
        /// <param name="targetDisplacement"></param>
        /// <param name="minDisplacement"></param>
        /// <param name="maxDisplacement"></param>
        /// <param name="minAcceleration"></param>
        /// <param name="maxAcceleration"></param>
        /// <param name="inputScalingFactor"></param>
        /// <param name="inputPowerFactor"></param>
        /// <returns></returns>
        private float DampedMaxAccelerationInput(float currentDisplacement, float targetDisplacement, float minDisplacement,
            float maxDisplacement, float minAcceleration, float maxAcceleration, float inputScalingFactor, float inputPowerFactor)
        {
            // Highest possible max acceleration input is max acceleration
            float maxAccelerationInput = maxAcceleration;

            // The max acceleration is determined by how close we are to the ground
            // At the min/max allowed distance from the ground the limit is infinity
            // At the target distance from the ground the limit is some given value

            if (currentDisplacement < targetDisplacement && currentDisplacement > minDisplacement)
            {
                // Reciprocal
                float xVal = (currentDisplacement - minDisplacement) / (targetDisplacement - minDisplacement);
                maxAccelerationInput = minAcceleration + (inputScalingFactor * ((1f / Mathf.Pow(xVal, inputPowerFactor)) - 1f));
            }
            else if (currentDisplacement > targetDisplacement && currentDisplacement < maxDisplacement)
            {
                // Reciprocal
                float xVal = (currentDisplacement - maxDisplacement) / (targetDisplacement - maxDisplacement);
                maxAccelerationInput = minAcceleration + (inputScalingFactor * ((1f / Mathf.Pow(xVal, inputPowerFactor)) - 1f));
            }

            // Highest possible max acceleration input is max acceleration
            return maxAccelerationInput < maxAcceleration ? maxAccelerationInput : maxAcceleration;
        }

        /// <summary>
        /// Applies a specified amount of damage to the ship at a specified position.
        /// </summary>
        /// <param name="damageAmount"></param>
        /// <param name="damagePosition"></param>
        /// <param name="isCollisionDamage"></param>
        private void ApplyDamage(float damageAmount, ProjectileModule.DamageType damageType, Vector3 damagePosition, bool adjustForCollisionResistance)
        {
            // Determine which damage region was hit
            bool[] regionDamagedArray = new bool[1];
            float[] actualDamageArray = new float[1];
            int damageArrayLengths = 1;
            if (shipDamageModel == ShipDamageModel.Localised)
            {
                damageArrayLengths = localisedDamageRegionList.Count + 1;
                regionDamagedArray = new bool[damageArrayLengths];
                actualDamageArray = new float[damageArrayLengths];
            }

            // Loop through all damage regions
            DamageRegion thisDamageRegion;
            float actualDamage = 0f;
            bool regionDamaged = false;            
            bool isShipInvincible = mainDamageRegion.isInvincible;

            // Get damage position in local space (calc once outside the loop)
            Vector3 localDamagePosition = trfmInvRot * (damagePosition - trfmPos);

            for (int i = 0; i < damageArrayLengths; i++)
            {
                // Get the current damage region we are iterating over, and work out whether it has been hit
                if (i == 0)
                {
                    // Main damage region
                    thisDamageRegion = mainDamageRegion;
                    // Main damage region is always hit unless it is invincible (which would make the whole ship invincible)
                    regionDamaged = !isShipInvincible;
                }
                else
                {
                    // Localised damage region
                    thisDamageRegion = localisedDamageRegionList[i - 1];

                    // If the main damage region is invincible, so are all the damage regions
                    if (isShipInvincible || thisDamageRegion.isInvincible) { regionDamaged = false; }
                    else
                    {
                        // Calculate whether the hit point is within the volume, and hence populate regionDamaged variable
                        // First, get damage position in local space
                        //localDamagePosition = trfmInvRot * (damagePosition - trfmPos);

                        // Then check whether the position is within the bounds of this damage region
                        regionDamaged = thisDamageRegion.IsHit(localDamagePosition);

                        // For some reason we need to slightly expand the damage region when the collider is exactly the same size.
                        //regionDamaged = localDamagePosition.x >= thisDamageRegion.relativePosition.x - (thisDamageRegion.size.x / 2) - 0.0001f &&
                        //    localDamagePosition.x <= thisDamageRegion.relativePosition.x + (thisDamageRegion.size.x / 2) + 0.0001f &&
                        //    localDamagePosition.y >= thisDamageRegion.relativePosition.y - (thisDamageRegion.size.y / 2) - 0.0001f &&
                        //    localDamagePosition.y <= thisDamageRegion.relativePosition.y + (thisDamageRegion.size.y / 2) + 0.0001f &&
                        //    localDamagePosition.z >= thisDamageRegion.relativePosition.z - (thisDamageRegion.size.z / 2) - 0.0001f &&
                        //    localDamagePosition.z <= thisDamageRegion.relativePosition.z + (thisDamageRegion.size.z / 2) + 0.0001f;
                    }
                }

                // Don't do any further calculations if the region wasn't hit
                if (regionDamaged)
                {
                    // Adjust for collision resistance - value passed in is just an impulse magnitude
                    if (adjustForCollisionResistance)
                    {
                        if (thisDamageRegion.collisionDamageResistance < 0.01f) { actualDamage = Mathf.Infinity; }
                        else { actualDamage = damageAmount / thisDamageRegion.collisionDamageResistance * 4f / mass; }
                    }
                    else { actualDamage = damageAmount; }

                    // Modify damage dealt based on relevant damage multipliers
                    if (useDamageMultipliers)
                    {
                        if (useLocalisedDamageMultipliers)
                        {
                            actualDamage *= thisDamageRegion.GetDamageMultiplier(damageType);
                        }
                        else
                        {
                            actualDamage *= mainDamageRegion.GetDamageMultiplier(damageType);
                        }
                    }

                    // Determine whether shielding is active for this damage region
                    if (thisDamageRegion.useShielding && thisDamageRegion.ShieldHealth > 0f)
                    {
                        // Set the shielding to active
                        regionDamaged = false;
                        // Only do damage to the shield if the damage amount is above the shielding threshold
                        if (actualDamage >= thisDamageRegion.shieldingDamageThreshold)
                        {
                            thisDamageRegion.ShieldHealth -= actualDamage;
                            thisDamageRegion.shieldRechargeDelayTimer = 0f;
                            // If this damage destroys the shielding entirely...
                            if (thisDamageRegion.ShieldHealth <= 0f)
                            {
                                // Get the residual damage value
                                actualDamage = -thisDamageRegion.ShieldHealth;
                                // Set the shielding to inactive
                                regionDamaged = true;
                                thisDamageRegion.ShieldHealth = -0.01f;
                            }
                        }
                    }

                    if (regionDamaged)
                    {
                        // Reduce health of damage region itself
                        thisDamageRegion.Health -= actualDamage;
                    }
                }
                // Added v1.2.7 - regions not damaged should have 0 damage
                else { actualDamage = 0f; }

                // Set calculated array variables
                actualDamageArray[i] = actualDamage;
                regionDamagedArray[i] = regionDamaged;
            }

            // Need to re-check this variable, as damage can destroy shielding and then use residual damage to affect
            // the damage region as well
            if (!shipDamageModelIsSimple && !isShipInvincible)
            {
                // Loop through each part list and reduce health of all parts in the regions that have been damaged

                // Compute an index shift for localised damage regions
                // Main damage region is index 0, so localised damage regions are a one-based indexing system
                // as opposed to the standard zero-based indexing system
                int drIndexShift = 0, drShiftedIndex, drIndex;
                if (shipDamageModel == ShipDamageModel.Localised) { drIndexShift = 1; }

                componentListSize = thrusterList.Count;
                for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                {
                    drIndex = thrusterList[componentIndex].damageRegionIndex;
                    drShiftedIndex = drIndex + drIndexShift;
                    if (drIndex > -1 && drShiftedIndex < damageArrayLengths && regionDamagedArray[drShiftedIndex])
                    {
                        thrusterList[componentIndex].Health -= actualDamageArray[drShiftedIndex];
                    }
                }

                componentListSize = wingList.Count;
                for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                {
                    drIndex = wingList[componentIndex].damageRegionIndex;
                    drShiftedIndex = drIndex + drIndexShift;
                    if (drIndex > -1 && drShiftedIndex < damageArrayLengths && regionDamagedArray[drShiftedIndex])
                    {
                        wingList[componentIndex].Health -= actualDamageArray[drShiftedIndex];
                    }
                }

                componentListSize = controlSurfaceList.Count;
                for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                {
                    drIndex = controlSurfaceList[componentIndex].damageRegionIndex;
                    drShiftedIndex = drIndex + drIndexShift;
                    if (drIndex > -1 && drShiftedIndex < damageArrayLengths && regionDamagedArray[drShiftedIndex])
                    {
                        controlSurfaceList[componentIndex].Health -= actualDamageArray[drShiftedIndex];
                    }
                }

                componentListSize = weaponList.Count;
                for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                {
                    drIndex = weaponList[componentIndex].damageRegionIndex;
                    drShiftedIndex = drIndex + drIndexShift;
                    if (drIndex > -1 && drShiftedIndex < damageArrayLengths && regionDamagedArray[drShiftedIndex])
                    {
                        weaponList[componentIndex].Health -= actualDamageArray[drShiftedIndex];
                    }
                }
            }

            // Damage callback
            if (callbackOnDamage != null) { callbackOnDamage(mainDamageRegion.Health); }

            // Add a damage event
            lastDamageEventIndex++;
            // Calculate rumble and camera shake required
            float rumbleDamageAmount = damageAmount;
            if (adjustForCollisionResistance)
            {
                rumbleDamageAmount /= mainDamageRegion.collisionDamageResistance * 2500f;
            }

            // Start camera shake as damageAmount with adjusted collision amount if applicable
            damageCameraShakeAmountRequired = rumbleDamageAmount;

            if (rumbleDamageAmount <= minRumbleDamage) { damageRumbleAmountRequired = 0f; }
            else if (rumbleDamageAmount >= maxRumbleDamage) { damageRumbleAmountRequired = 1f; }
            else { damageRumbleAmountRequired = (rumbleDamageAmount - minRumbleDamage) / (maxRumbleDamage - minRumbleDamage); }

            // Calculate camera shake required
            if (damageCameraShakeAmountRequired < 0f) { damageCameraShakeAmountRequired = 0f; }
            else if (damageCameraShakeAmountRequired > 1f) { damageCameraShakeAmountRequired = 1f; }
        }

        /// <summary>
        /// Get the index or 0-based position in the list of damage regions.
        /// Although typically used to return the index in the localisedDamageRegionList,
        /// if passed the mainDamageRegion, it will always return 0.
        /// If no matching regions are found, it will return -1.
        /// </summary>
        /// <param name="damageRegion"></param>
        /// <returns></returns>
        private int GetDamageRegionIndex(DamageRegion damageRegion)
        {
            int damageRegionIndex = -1;

            if (damageRegion.guidHash == mainDamageRegion.guidHash) { return 0; }
            else
            {
                int localisedDamageRegionListCount = localisedDamageRegionList == null ? 0 : localisedDamageRegionList.Count;

                for (int drIdx = 0; drIdx < localisedDamageRegionListCount; drIdx++)
                {
                    if (localisedDamageRegionList[drIdx].guidHash == damageRegion.guidHash) { damageRegionIndex = drIdx; break; }
                }
            }

            return damageRegionIndex;
        }

        /// <summary>
        /// Update the maximum estimated range for all turrets with auto targeting enabled
        /// </summary>
        internal void UpdateMaxTurretRange()
        {
            int numWeapons = weaponList == null ? 0 : weaponList.Count;

            float maxRange = 0f;

            for (int wpIdx = 0; wpIdx < numWeapons; wpIdx++)
            {
                Weapon weapon = weaponList[wpIdx];

                // Only process turrets with Auto Targeting enabled
                if (weapon != null && weapon.isAutoTargetingEnabled && (weapon.weaponTypeInt == Weapon.TurretProjectileInt || weapon.weaponTypeInt == Weapon.TurretBeamInt))
                {
                    if (weapon.estimatedRange > maxRange) { maxRange = weapon.estimatedRange; }
                }
            }

            estimatedMaxTurretRange = maxRange;
        }

        /// <summary>
        /// Update the maximum estimated range for all weapons with auto targeting enabled
        /// </summary>
        internal void UpdateMaxAutoTargetRange()
        {
            int numWeapons = weaponList == null ? 0 : weaponList.Count;

            float maxRange = 0f;

            for (int wpIdx = 0; wpIdx < numWeapons; wpIdx++)
            {
                Weapon weapon = weaponList[wpIdx];

                // Only process weapons with Auto Targeting enabled
                if (weapon != null && weapon.isAutoTargetingEnabled)
                {
                    if (weapon.estimatedRange > maxRange) { maxRange = weapon.estimatedRange; }
                }
            }

            estimatedMaxAutoTargetRange = maxRange;
        }

        /// <summary>
        /// This gets called from FixedUpdate in BeamModule. It performs the following:
        /// 1. Checks if the beam should be despawned
        /// 2. Moves the beam
        /// 3. Changes the length
        /// 4. Checks if it hits anything
        /// 5. Updates damage on what it hits
        /// 6. Instantiate effects object at hit point
        /// 7. Consumes weapon power
        /// It needs to be a member of the ship instance as it requires both ship and beam data.
        /// Assumes the beam linerenderer has useWorldspace enabled.
        /// </summary>
        /// <param name="beamModule"></param>
        internal void MoveBeam(BeamModule beamModule)
        {
            if (beamModule.isInitialised && beamModule.isBeamEnabled && beamModule.weaponIndex >= 0 && beamModule.firePositionIndex >= 0)
            {
                Weapon weapon = weaponList[beamModule.weaponIndex];
                SSCBeamItemKey beamItemKey = weapon.beamItemKeyList[beamModule.firePositionIndex];

                // Read once from the deltaTime property getter
                float _deltaTime = Time.deltaTime;

                if ((weapon.weaponTypeInt == Weapon.FixedBeamInt || weapon.weaponTypeInt == Weapon.TurretBeamInt) && beamItemKey.beamSequenceNumber == beamModule.itemSequenceNumber)
                {
                    weaponFiringButtonInt = (int)weapon.firingButton;

                    // For autofiring weapons, this will return false.
                    bool isReadyToFire = (pilotPrimaryFireInput && weaponFiringButtonInt == weaponPrimaryFiringInt) || (pilotSecondaryFireInput && weaponFiringButtonInt == weaponSecondaryFiringInt);

                    // If this is auto-firing turret check if it is ready to fire
                    if (weaponFiringButtonInt == weaponAutoFiringInt && weapon.weaponTypeInt == Weapon.TurretBeamInt)
                    {
                        // Is turret locked on target and in direct line of sight?
                        // NOTE: If check LoS is enabled, it will still fire if another enemy is between the turret and the weapon.target.
                        isReadyToFire = weapon.isLockedOnTarget && (!weapon.checkLineOfSight || WeaponHasLineOfSight(weapon));

                        // BUILD TEST
                        //isReadyToFire = !weapon.checkLineOfSight || WeaponHasLineOfSight(weapon);
                        // TODO - Is target in range?
                    }

                    // Should this beam be despawned or returned to the pool?
                    // a) No charge in weapon
                    // b) Weapon has no health
                    // c) Weapon has no performance
                    // d) user stopped firing and has fired for min time permitted
                    // e) has exceeded the maximum firing duration
                    if (weapon.chargeAmount <= 0f || weapon.health <= 0f || weapon.currentPerformance == 0f || (!isReadyToFire && beamModule.burstDuration > beamModule.minBurstDuration) || beamModule.burstDuration > beamModule.maxBurstDuration)
                    {
                        // Unassign the beam from this weapon's fire position
                        weapon.beamItemKeyList[beamModule.firePositionIndex] = new SSCBeamItemKey(-1, -1, 0);

                        beamModule.DestroyBeam();
                    }
                    else
                    {
                        // Move the beam start
                        // Calculate the World-space Fire Position (like when weapon is first fired)
                        // Note: That the ship could have moved and rotated, so we recalc here.
                        Vector3 _weaponWSBasePos = GetWeaponWorldBasePosition(weapon);
                        Vector3 _weaponWSFireDir = GetWeaponWorldFireDirection(weapon);
                        Vector3 _weaponWSFirePos = GetWeaponWorldFirePosition(weapon, _weaponWSBasePos, beamModule.firePositionIndex);

                        // Move the beam transform but keep the first LineRenderer position at 0,0,0
                        beamModule.transform.position = _weaponWSFirePos;

                        beamModule.transform.SetPositionAndRotation(_weaponWSFirePos, Quaternion.LookRotation(_weaponWSFireDir, rbodyUp));

                        // Calc length and end position
                        float _desiredLength = beamModule.burstDuration * beamModule.speed;

                        if (_desiredLength > weapon.maxRange) { _desiredLength = weapon.maxRange; }

                        Vector3 _endPosition = _weaponWSFirePos + (_weaponWSFireDir * _desiredLength);

                        // Check if it hit anything
                        if (Physics.Linecast(_weaponWSFirePos, _endPosition, out raycastHitInfo, ~0, QueryTriggerInteraction.Ignore))
                        {
                            // Adjust the end position to the hit point
                            _endPosition = raycastHitInfo.point;

                            // Update damage if it hit a ship or an object with a damage receiver
                            if (!BeamModule.CheckShipHit(raycastHitInfo, beamModule, _deltaTime))
                            {
                                // If it hit an object with a DamageReceiver script attached, take appropriate action like call a custom method
                                BeamModule.CheckObjectHit(raycastHitInfo, beamModule, _deltaTime);
                            }

                            // ISSUES - the effect may not be visible while firing if:
                            // 1) if the effect despawn time is less than the beam max burst duration (We have a warning in the BeamModule editor)
                            // 2) if the effect does not have looping enabled (this could be more expensive to check)

                            // Add or Move the effects object
                            if (beamModule.effectsObjectPrefabID >= 0)
                            {
                                if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(beamModule.sceneHandle); }

                                if (sscManager != null)
                                {
                                    // If the effect has not been spawned, do now
                                    if (beamModule.effectsItemKey.effectsObjectSequenceNumber == 0)
                                    {
                                        InstantiateEffectsObjectParameters ieParms = new InstantiateEffectsObjectParameters
                                        {
                                            effectsObjectPrefabID = beamModule.effectsObjectPrefabID,
                                            position = _endPosition,
                                            rotation = beamModule.transform.rotation
                                        };

                                        // Instantiate the hit effects
                                        if (sscManager.InstantiateEffectsObject(ref ieParms) != null)
                                        {
                                            if (ieParms.effectsObjectSequenceNumber > 0)
                                            {
                                                // Record the hit effect item key
                                                beamModule.effectsItemKey = new SSCEffectItemKey(ieParms.effectsObjectPrefabID, ieParms.effectsObjectPoolListIndex, ieParms.effectsObjectSequenceNumber);
                                            }
                                        }
                                    }
                                    // Move the existing effects object to the end of the beam
                                    else
                                    {
                                        // Currently we are not checking sequence number matching (for pooled effects) as it is faster and can
                                        // avoid doing an additional GetComponent().
                                        sscManager.MoveEffectsObject(beamModule.effectsItemKey, _endPosition, beamModule.transform.rotation, false);
                                    }
                                }
                            }
                        }
                        // The beam isn't hitting anything AND there is an active effects object
                        else if (beamModule.effectsObjectPrefabID >= 0 && beamModule.effectsItemKey.effectsObjectSequenceNumber > 0)
                        {
                            // Destroy the effects object or return it to the pool
                            if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(beamModule.sceneHandle); }

                            if (sscManager != null)
                            {
                                sscManager.DestroyEffectsObject(beamModule.effectsItemKey);
                            }
                        }

                        // Move the beam end (assumes world space)
                        // useWorldspace is enabled in beamModule.InitialiseBeam(..)
                        beamModule.lineRenderer.SetPosition(0, _weaponWSFirePos);
                        beamModule.lineRenderer.SetPosition(1, _endPosition);

                        // Consume weapon power. Consumes upt to 2x power when overheating.
                        if (weapon.rechargeTime > 0f) { weapon.chargeAmount -= _deltaTime * (weapon.heatLevel > weapon.overHeatThreshold ? 2f - weapon.currentPerformance : 1f) / beamModule.dischargeDuration; }

                        weapon.ManageHeat(_deltaTime, 1f);

                        // If we run out of power, de-activate it before weapon starts recharging
                        if (weapon.chargeAmount <= 0f)
                        {
                            weapon.chargeAmount = 0f;

                            // Unassign the beam from this weapon's fire position
                            weapon.beamItemKeyList[beamModule.firePositionIndex] = new SSCBeamItemKey(-1, -1, 0);

                            beamModule.DestroyBeam();
                        }
                    }
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR ship.MoveBeam has been called on the wrong beam. isInitialised: " + beamModule.isInitialised +
                  " isBeamEnabled: " + beamModule.isBeamEnabled + " weaponIndex: " + beamModule.weaponIndex + " firePositionIndex: " + beamModule.firePositionIndex); }
            #endif
        }

        private Vector3 GetWeaponWorldBasePosition(Weapon weapon)
        {
            return (trfmRight * weapon.relativePosition.x) +
                   (trfmUp * weapon.relativePosition.y) +
                   (trfmFwd * weapon.relativePosition.z) +
                   trfmPos;
        }

        private Vector3 GetWeaponWorldFireDirection(Weapon weapon)
        {
            // Get relative fire direction
            weaponRelativeFireDirection = weapon.fireDirectionNormalised;

            // If this is a turret, adjust relative fire direction based on turret rotation
            if (weapon.weaponTypeInt == Weapon.TurretProjectileInt || weapon.weaponTypeInt == Weapon.TurretBeamInt)
            {
                // (turret rotation less ship rotate) - (original turret rotation less ship rotation) + relative fire direction
                weaponRelativeFireDirection = (trfmInvRot * weapon.turretPivotX.rotation) * weapon.intPivotYInvRot * weaponRelativeFireDirection;
            }

            // Get weapon world fire direction
            return (trfmRight * weaponRelativeFireDirection.x) +
                   (trfmUp * weaponRelativeFireDirection.y) +
                   (trfmFwd * weaponRelativeFireDirection.z);
        }

        private Vector3 GetWeaponWorldFirePosition(Weapon weapon, Vector3 weaponWorldBasePosition, int firePositionIndex)
        {
            // Check if there are multiple fire positions
            if (weapon.isMultipleFirePositions)
            {
                // Get relative fire position
                weaponRelativeFirePosition = weapon.firePositionList[firePositionIndex];
            }
            else
            {
                // If there is only one fire position, relative position must be the zero vector
                weaponRelativeFirePosition.x = 0f;
                weaponRelativeFirePosition.y = 0f;
                weaponRelativeFirePosition.z = 0f;
            }
            // If this is a turret, adjust relative fire position based on turret rotation
            if (weapon.weaponTypeInt == Weapon.TurretProjectileInt || weapon.weaponTypeInt == Weapon.TurretBeamInt)
            {
                weaponRelativeFirePosition = (trfmInvRot * weapon.turretPivotX.rotation) * weaponRelativeFirePosition;
            }

            return weaponWorldBasePosition +
                    (trfmRight * weaponRelativeFirePosition.x) +
                    (trfmUp * weaponRelativeFirePosition.y) +
                    (trfmFwd * weaponRelativeFirePosition.z);
        }


        /// <summary>
        /// Begin to shut down the thrusters. Optionally override the shutdown duration.
        /// See also shipControlModule.ShutdownThrusterSystems(..)
        /// </summary>
        internal void ShutdownThrusterSystems (bool isInstantShutdown = false)
        {
            // Instant shutdown can be called even while in the process of shutting down.
            if (isThrusterSystemsStarted || isInstantShutdown)
            {
                // Perform an instant shutdown?
                if (isInstantShutdown || thrusterSystemShutdownDuration <= 0f)
                {
                    isThrusterSystemsStarted = false;
                    thrusterSystemShutdownTimer = 0f;
                    thrusterSystemStartupTimer = 0f;

                    SetThrottleAllThrusters(0f);
                }
                else
                {
                    // Activate the shutdown sequence

                    // if startup was in progress, begin at that point
                    if (thrusterSystemStartupTimer > 0f && thrusterSystemStartupDuration > 0f)
                    {
                        thrusterSystemShutdownTimer = (1f - (thrusterSystemStartupTimer / thrusterSystemStartupDuration)) * thrusterSystemShutdownDuration;
                    }
                    else
                    {
                        thrusterSystemShutdownTimer = 0.001f;
                    }

                    // Reset the startup timer so that shutdown can continue
                    thrusterSystemStartupTimer = 0f;
                }
            }
        }

        /// <summary>
        /// Begin to bring the thrusters online. Optionally, override the startup duration.
        /// As soon as the systems begin to start up, isThrusterSystemsStarted will be true.
        /// See also shipControlModule.StartupThrusterSystems(..)
        /// </summary>
        internal void StartupThrusterSystems (bool isInstantStartup = false)
        {
            // Instant startup can be called even while in the process of
            // starting up. This is actually how we trigger the end of the
            // startup process when the timer expires in CheckThrusterSystems(..).

            // Perform an instant startup?
            if (isInstantStartup || thrusterSystemStartupDuration <= 0f)
            {
                thrusterSystemShutdownTimer = 0f;
                thrusterSystemStartupTimer = 0f;

                SetThrottleAllThrusters(1f);
            }
            else
            {
                // Activate the startup sequence

                // if shutdown was in progress, begin at that point
                if (thrusterSystemShutdownTimer > 0f && thrusterSystemShutdownDuration > 0f)
                {
                    thrusterSystemStartupTimer = (1f - (thrusterSystemShutdownTimer / thrusterSystemShutdownDuration)) * thrusterSystemStartupDuration;
                }
                else
                {
                    thrusterSystemStartupTimer = 0.001f;
                }

                // Reset the shutdown timer so that startup can continue
                thrusterSystemShutdownTimer = 0f;
            }

            isThrusterSystemsStarted = true;
        }

        #endregion

        #region Public Non-Static Methods

        #region Set Class Defaults

        public void SetClassDefaults()
        {
            this.initialiseOnAwake = true;

            this.shipPhysicsModel = ShipPhysicsModel.Arcade;

            this.mass = 5000f;
            this.centreOfMass = Vector3.zero;
            this.setCentreOfMassManually = false;
            this.showCOMGizmosInSceneView = true;
            this.showCOLGizmosInSceneView = false;
            this.showCOTGizmosInSceneView = false;

            this.thrusterList = new List<Thruster>(25);

            // Create a default forwards-facing thruster
            Thruster thruster = new Thruster();
            if (thruster != null)
            {
                thruster.name = "Forward Thruster";
                thruster.forceUse = 1;
                thruster.primaryMomentUse = 0;
                thruster.secondaryMomentUse = 0;
                thruster.maxThrust = 100000;
                thruster.thrustDirection = Vector3.forward;
                thruster.relativePosition = Vector3.zero;
                this.thrusterList.Add(thruster);
            }
            isThrusterFXStationary = false;
            isThrusterSystemsStarted = true;
            thrusterSystemStartupDuration = 5f;
            thrusterSystemShutdownDuration = 5f;
            useCentralFuel = true;
            centralFuelLevel = 100f;

            this.wingList = new List<Wing>(5);
            this.controlSurfaceList = new List<ControlSurface>(10);
            this.weaponList = new List<Weapon>(10);

            // Expand editor lists by default
            isThrusterSystemsExpanded = true;
            isThrusterListExpanded = true;
            isWingListExpanded = true;
            isControlSurfaceListExpanded = true;
            isWeaponListExpanded = true;
            isDamageListExpanded = true;
            isMainDamageExpanded = true;
            showDamageInEditor = true;

            this.dragCoefficients = new Vector3(0.5f, 1f, 0.1f);
            this.dragReferenceAreas = new Vector3(10f, 20f, 5f);
            this.centreOfDragXMoment = Vector3.zero;
            this.centreOfDragYMoment = Vector3.zero;
            this.centreOfDragZMoment = Vector3.zero;
            this.angularDragFactor = 1f;
            this.disableDragMoments = false;
            this.dragXMomentMultiplier = 1f;
            this.dragYMomentMultiplier = 1f;
            this.dragZMomentMultiplier = 1f;
            this.wingStallEffect = 1f;

            this.arcadeUseBrakeComponent = true;
            this.arcadeBrakeStrength = 100f;
            this.arcadeBrakeIgnoreMediumDensity = false;
            this.arcadeBrakeMinAcceleration = 25f;

            this.rotationalFlightAssistStrength = 3f;
            this.translationalFlightAssistStrength = 3f;
            this.stabilityFlightAssistStrength = 0f;
            this.brakeFlightAssistStrength = 0f;
            this.brakeFlightAssistMinSpeed = -10f;
            this.brakeFlightAssistMaxSpeed = 10f;
            this.brakeFlightAssistStrengthX = 0f;
            this.brakeFlightAssistStrengthY = 0f;

            this.limitPitchAndRoll = false;
            this.maxPitch = 10f;
            this.maxTurnRoll = 15f;
            this.pitchSpeed = 30f;
            this.turnRollSpeed = 30f;
            this.rollControlMode = RollControlMode.YawInput;
            this.pitchRollMatchResponsiveness = 4f;
            this.stickToGroundSurface = false;
            this.avoidGroundSurface = false;
            this.useGroundMatchSmoothing = false;
            this.useGroundMatchLookAhead = false;
            this.orientUpInAir = false;
            this.targetGroundDistance = 5f;
            this.minGroundDistance = 2f;
            this.maxGroundDistance = 8f;
            this.maxGroundCheckDistance = 25f;
            this.groundMatchResponsiveness = 50f;
            this.groundMatchDamping = 50f;
            this.maxGroundMatchAccelerationFactor = 3f;
            this.centralMaxGroundMatchAccelerationFactor = 1f;
            this.groundNormalCalculationMode = GroundNormalCalculationMode.SmoothedNormal;
            this.groundNormalHistoryLength = 5;
            this.groundLayerMask = ~0;

            this.inputControlAxis = InputControlAxis.None;
            this.inputControlLimit = 0f;
            this.inputControlForwardAngle = 0f;
            this.inputControlMovingRigidness = 25f;
            this.inputControlTurningRigidness = 25f;

            this.rollPower = 1f;
            this.pitchPower = 1f;
            this.yawPower = 1f;
            this.steeringThrusterPriorityLevel = 0f;

            this.pilotMomentInput = Vector3.zero;
            this.pilotForceInput = Vector3.zero;
            this.arcadePitchAcceleration = 100f;
            this.arcadeYawAcceleration = 100f;
            this.arcadeRollAcceleration = 250f;
            this.arcadeMaxFlightTurningAcceleration = 500f;
            this.arcadeMaxGroundTurningAcceleration = 500f;

            this.mediumDensity = 1.293f;
            this.gravitationalAcceleration = 9.81f;
            this.gravityDirection = -Vector3.up;

            this.shipDamageModel = ShipDamageModel.Simple;
            this.mainDamageRegion = new DamageRegion();
            this.mainDamageRegion.name = "Main Damage Region";
            this.localisedDamageRegionList = new List<DamageRegion>(10);
            this.useDamageMultipliers = false;
            this.useLocalisedDamageMultipliers = false;
            this.respawnTime = 5f;
            this.collisionRespawnPositionDelay = 5f;
            this.respawningMode = RespawningMode.DontRespawn;
            this.customRespawnPosition = Vector3.zero;
            this.customRespawnRotation = Vector3.zero;
            this.respawnVelocity = Vector3.zero;
            this.respawningPathGUIDHash = 0;
            this.minRumbleDamage = 0f;
            this.maxRumbleDamage = 10f;
            this.stuckTime = 0f;
            this.stuckSpeedThreshold = 0.1f;
            this.stuckAction = StuckAction.DoNothing;
            this.stuckActionPathGUIDHash = 0;

            // By default, all ships on the same faction/side/alliance.
            this.factionId = 0;

            squadronId = -1; // NOT SET

            this.isRadarEnabled = false;
            this.radarBlipSize = 1;
        }

        #endregion

        #region Initialisation

        /// <summary>
        /// Initialise all necessary ship data.
        /// </summary>
        public void Initialise(Transform trfm)
        {
            // Store physics model as a boolean value to save looking up enumeration at runtime
            shipPhysicsModelPhysicsBased = shipPhysicsModel == ShipPhysicsModel.PhysicsBased;

            // New values, better for pipes
            pitchPIDController = new PIDController(0.5f, 0f, 0.1f);
            rollPIDController = new PIDController(0.5f, 0f, 0.1f);
            // For pitch and roll derivative on measurement is disabled so that
            // a) in limit pitch/roll mode we can use local-space pitch and roll measurements
            // b) if the target pitch/roll is quickly moving away from the measured pitch/roll the
            //    PID controller can use a large input to prevent it
            pitchPIDController.derivativeOnMeasurement = false;
            rollPIDController.derivativeOnMeasurement = false;
            ReinitialisePitchRollMatchVariables();

            // Create a new PID controller for ground match
            groundDistPIDController = new PIDController(0f, 0f, 0f);
            // For ground dist derivative on measurement is disabled so that if the target distance is quickly moving away from
            // the measured pitch/roll the PID controller can use a large input to prevent it
            groundDistPIDController.derivativeOnMeasurement = false;
            // Initialise ground dist PID controller variables
            ReinitialiseGroundMatchVariables();

            // Create new PID controllers for input axis control
            inputAxisForcePIDController = new PIDController(0f, 0f, 0f);
            inputAxisMomentPIDController = new PIDController(0f, 0f, 0f);
            inputAxisForcePIDController.derivativeOnMeasurement = false;
            inputAxisMomentPIDController.derivativeOnMeasurement = false;
            ReinitialiseInputControlVariables();

            // Create new PID controllers for stability flight assist
            sFlightAssistPitchPIDController = new PIDController(0f, 0f, 0f);
            sFlightAssistYawPIDController = new PIDController(0f, 0f, 0f);
            sFlightAssistRollPIDController = new PIDController(0f, 0f, 0f);
            ReinitialiseStabilityFlightAssistVariables();

            // Initialise ray for raycasting against the ground
            raycastRay = new Ray(Vector3.zero, Vector3.up);

            // Initialise thruster, wing, weapon and damage region variables
            ReinitialiseThrusterVariables();
            ReinitialiseWingVariables();
            ReinitialiseWeaponVariables();
            ReinitialiseDamageRegionVariables(true);

            // Cache the Weapon FiringButton enumeration values to avoid looking up the enumeration
            // Used in CalculateForceAndMoment(..)
            weaponPrimaryFiringInt = (int)Weapon.FiringButton.Primary;
            weaponSecondaryFiringInt = (int)Weapon.FiringButton.Secondary;
            weaponAutoFiringInt = (int)Weapon.FiringButton.AutoFire;

            // Initialise respawn data
            ReinitialiseRespawnVariables();

            // Initialise input data
            ReinitialiseInputVariables();

            // Get a ship ID integer from the transform ID
            shipId = trfm.GetInstanceID();

            // Initially there is no radar item assigned in the radar system
            radarItemIndex = -1;

            // Added in 1.3.3 for muzzle FX reparenting ONLY.
            // Use with caution as may need to be removed in the future if
            // causes issues like when destroying the ShipControlModule.
            shipTransform = trfm;

            StopBoost();
        }

        /// <summary>
        /// Re-initialises all variables needed when changing the ship physics model.
        /// Call after modifying shipPhysicsModel.
        /// </summary>
        public void ReinitialiseShipPhysicsModel()
        {
            // Store physics model as a boolean value to save looking up enumeration at runtime
            shipPhysicsModelPhysicsBased = shipPhysicsModel == ShipPhysicsModel.PhysicsBased;

            ReinitialisePitchRollMatchVariables();
            ReinitialiseGroundMatchVariables();
            ReinitialiseInputVariables();
            ReinitialiseInputControlVariables();
        }

        /// <summary>
        /// Re-initialises variables related to the stability flight assist.
        /// Call after modifying stabilityFlightAssistStrength.
        /// </summary>
        private void ReinitialiseStabilityFlightAssistVariables()
        {
            // Configure pitch PID controller
            sFlightAssistPitchPIDController.pGain = 0.025f * stabilityFlightAssistStrength;
            sFlightAssistPitchPIDController.iGain = 0.005f * stabilityFlightAssistStrength;
            sFlightAssistPitchPIDController.dGain = 0.01f * stabilityFlightAssistStrength;
            sFlightAssistPitchPIDController.SetInputLimits(-1f, 1f);

            // Configure yaw PID controller
            sFlightAssistYawPIDController.pGain = 0.025f * stabilityFlightAssistStrength;
            sFlightAssistYawPIDController.iGain = 0.005f * stabilityFlightAssistStrength;
            sFlightAssistYawPIDController.dGain = 0.01f * stabilityFlightAssistStrength;
            sFlightAssistYawPIDController.SetInputLimits(-1f, 1f);

            // Configure roll PID controller
            sFlightAssistRollPIDController.pGain = 0.025f * stabilityFlightAssistStrength;
            sFlightAssistRollPIDController.iGain = 0.005f * stabilityFlightAssistStrength;
            sFlightAssistRollPIDController.dGain = 0.01f * stabilityFlightAssistStrength;
            sFlightAssistRollPIDController.SetInputLimits(-1f, 1f);
        }

        /// <summary>
        /// Re-initialises variables related to pitch and roll match.
        /// </summary>
        private void ReinitialisePitchRollMatchVariables()
        {
            if (shipPhysicsModelPhysicsBased)
            {
                // Pitch and roll PID controllers are always constrained to ship physics
                pitchPIDController.SetInputLimits(-1f, 1f);
                rollPIDController.SetInputLimits(-1f, 1f);
            }
            else
            {
                // Pitch and roll PID controllers are not constrained to ship physics
                pitchPIDController.SetInputLimits(Mathf.NegativeInfinity, Mathf.Infinity);
                rollPIDController.SetInputLimits(Mathf.NegativeInfinity, Mathf.Infinity);
            }

            // Reset PID controllers
            pitchPIDController.ResetController();
            rollPIDController.ResetController();
        }

        /// <summary>
        /// Re-initialises variables related to ground distance match calculations.
        /// Call after modifying useGroundMatchSmoothing, groundMatchResponsiveness, groundMatchDamping, 
        /// maxGroundMatchAccelerationFactor, centralMaxGroundMatchAccelerationFactor or groundNormalHistoryLength.
        /// </summary>
        public void ReinitialiseGroundMatchVariables()
        {
            // Calculate height range constant (if we are using a min-max range for target distance)
            float groundMatchHeightRangeConstant = 10f;
            if (useGroundMatchSmoothing)
            {
                // Set the constant to the minimum of: The difference between the min and target distances and the difference
                // between the max and target distances
                groundMatchHeightRangeConstant = targetGroundDistance - minGroundDistance;
                if (groundMatchHeightRangeConstant > maxGroundDistance - targetGroundDistance)
                {
                    groundMatchHeightRangeConstant = maxGroundDistance - targetGroundDistance;
                }
            }

            // P-gain is proportional to responsiveness and inversely proportional to height range
            groundDistPIDController.pGain = 100f * groundMatchResponsiveness / groundMatchHeightRangeConstant;
            // I-gain is half of p-gain
            groundDistPIDController.iGain = 50f * groundMatchResponsiveness / groundMatchHeightRangeConstant;
            // D-gain is proportional to damping and inversely proportional to height range
            groundDistPIDController.dGain = 10f * groundMatchDamping / groundMatchHeightRangeConstant;

            if (shipPhysicsModelPhysicsBased)
            {
                // In physics-based mode, the forces must stay within the limits of the ship thrusters
                groundDistPIDController.SetInputLimits(-1f, 1f);
            }
            else
            {
                // In arcade mode, the forces must stay within a maximum acceleration
                maxGroundMatchAcceleration = Mathf.Pow(10f, maxGroundMatchAccelerationFactor);
                groundDistPIDController.SetInputLimits(-maxGroundMatchAcceleration, maxGroundMatchAcceleration);
                centralMaxGroundMatchAcceleration = Mathf.Pow(10f, centralMaxGroundMatchAccelerationFactor);
            }

            // Reset the PID Controller
            groundDistPIDController.ResetController();

            // Initialise ground normal history
            groundNormalHistory = new Vector3[groundNormalHistoryLength];
            for (int gnIdx = 0; gnIdx < groundNormalHistoryLength; gnIdx++) { groundNormalHistory[gnIdx] = trfmUp; }
        }

        /// <summary>
        /// Re-initialises variables related to Input Control for 2.5D flight.
        /// Call this after modifying inputControlAxis, inputControlMovingRigidness or inputControlTurningRigidness.
        /// </summary>
        public void ReinitialiseInputControlVariables()
        {
            if (shipPhysicsModelPhysicsBased)
            {
                // Set up the physics-based movement PID controller
                // 0.5, 0.1, 0.1 TODO tweak for physics-based
                inputAxisForcePIDController.pGain = inputControlMovingRigidness * 0.1f;
                inputAxisForcePIDController.iGain = inputControlMovingRigidness * 0.02f;
                inputAxisForcePIDController.dGain = inputControlMovingRigidness * 0.02f;

                // Set up the physics-base rotation PID controller
                // 10, 2, 2 TODO tweak for physics-based
                inputAxisMomentPIDController.pGain = inputControlTurningRigidness * 1f;
                inputAxisMomentPIDController.iGain = inputControlTurningRigidness * 0.2f;
                inputAxisMomentPIDController.dGain = inputControlTurningRigidness * 0.2f;

                // Set physical input limits
                inputAxisForcePIDController.SetInputLimits(-1f, 1f);
                inputAxisMomentPIDController.SetInputLimits(-1f, 1f);
            }
            else
            {
                // Set up the arcade movement PID controller
                inputAxisForcePIDController.pGain = inputControlMovingRigidness * 1f;
                inputAxisForcePIDController.iGain = inputControlMovingRigidness * 0.2f;
                inputAxisForcePIDController.dGain = inputControlMovingRigidness * 0.2f;

                // Set up the arcade rotation PID controller
                inputAxisMomentPIDController.pGain = inputControlTurningRigidness * 1f;
                inputAxisMomentPIDController.iGain = inputControlTurningRigidness * 0.2f;
                inputAxisMomentPIDController.dGain = inputControlTurningRigidness * 0.2f;

                // Set non-physical input limits
                inputAxisForcePIDController.SetInputLimits(Mathf.NegativeInfinity, Mathf.Infinity);
                inputAxisMomentPIDController.SetInputLimits(Mathf.NegativeInfinity, Mathf.Infinity);
            }

            inputAxisForcePIDController.ResetController();
            inputAxisMomentPIDController.ResetController();
        }

        /// <summary>
        /// Re-initialises variables related to thrusters. 
        /// Call after modifying thrusterList.
        /// </summary>
        public void ReinitialiseThrusterVariables()
        {
            // Initialise all thrusters
            componentListSize = thrusterList == null ? 0 : thrusterList.Count;
            for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
            {
                thrusterList[componentIndex].Initialise();
            }

            // If thruster systems are not online, immediately turn off all thrusters.
            if (!isThrusterSystemsStarted)
            {
                ShutdownThrusterSystems(true);
            }
        }

        /// <summary>
        /// Re-initialises variables related to wings.
        /// Call after modifying wingList.
        /// </summary>
        public void ReinitialiseWingVariables()
        {
            // Initialise all wings
            componentListSize = wingList == null ? 0 : wingList.Count;
            for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
            {
                wingList[componentIndex].Initialise();
            }
        }

        /// <summary>
        /// Re-initialises variables related to weapons.
        /// Call after modifying weaponList.
        /// </summary>
        public void ReinitialiseWeaponVariables()
        {
            // Initialise all weapons
            componentListSize = weaponList == null ? 0 : weaponList.Count;
            for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
            {
                weaponList[componentIndex].Initialise(trfmInvRot);
            }
        }

        /// <summary>
        /// Re-initialises variables related to damage regions.
        /// Call after modifying mainDamageRegion or localDamageRegionList (or modifying shipDamageModel).
        /// NOTE: Not required if just changing the Health or ShieldHealth
        /// </summary>
        public void ReinitialiseDamageRegionVariables(bool refreshAll = false)
        {
            // Initialise the main damage region
            mainDamageRegion.Initialise();

            numLocalisedDamageRegions = 0;

            // Only initialise localised damage regions if the ship damage model is localised
            if (shipDamageModel == ShipDamageModel.Localised || refreshAll)
            {
                // Initialise all localised damage regions
                numLocalisedDamageRegions = localisedDamageRegionList == null ? 0 : localisedDamageRegionList.Count;
                for (componentIndex = 0; componentIndex < numLocalisedDamageRegions; componentIndex++)
                {
                    localisedDamageRegionList[componentIndex].Initialise();
                }
            }
        }

        /// <summary>
        /// Re-initialises respawn variables using the current position and rotation of the ship.
        /// Also needs to be called after changing respawningMode to RespawningMode.RespawnAtLastPosition.
        /// </summary>
        public void ReinitialiseRespawnVariables()
        {
            // Initialise current respawn position/rotation
            currentRespawnPosition = trfmPos;
            currentRespawnRotation = trfmRot;
            if (respawningMode == RespawningMode.RespawnAtLastPosition)
            {
                currentCollisionRespawnPosition = currentRespawnPosition;
                currentCollisionRespawnRotation = currentRespawnRotation;
                nextCollisionRespawnPosition = currentRespawnPosition;
                nextCollisionRespawnRotation = currentRespawnRotation;
            }
        }

        /// <summary>
        /// Re-initialises variables related to ship inputs. 
        /// Call after modifying thrusterList or controlSurfaceList.
        /// Call after modifying the forceUse/primaryMomentUse/secondaryMomentUse of a thruster or the type of a control surface.
        /// </summary>
        public void ReinitialiseInputVariables()
        {
            // Initially assume that no inputs are used
            longitudinalThrustInputUsed = false;
            horizontalThrustInputUsed = false;
            verticalThrustInputUsed = false;
            pitchMomentInputUsed = false;
            yawMomentInputUsed = false;
            rollMomentInputUsed = false;

            if (shipPhysicsModelPhysicsBased)
            {
                // Loop through all thrusters to check force and moment inputs
                componentListSize = thrusterList.Count;
                for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                {
                    // Get the thruster we are concerned with
                    thruster = thrusterList[componentIndex];
                    // Check whether force inputs are used for this thruster
                    if (thruster.forceUse == 1 || thruster.forceUse == 2) { longitudinalThrustInputUsed = true; }
                    else if (thruster.forceUse == 3 || thruster.forceUse == 4) { verticalThrustInputUsed = true; }
                    else if (thruster.forceUse == 5 || thruster.forceUse == 6) { horizontalThrustInputUsed = true; }
                    // Check whether moment inputs are used for this thruster
                    if (thruster.primaryMomentUse == 1 || thruster.primaryMomentUse == 2) { rollMomentInputUsed = true; }
                    else if (thruster.primaryMomentUse == 3 || thruster.primaryMomentUse == 4) { pitchMomentInputUsed = true; }
                    else if (thruster.primaryMomentUse == 5 || thruster.primaryMomentUse == 6) { yawMomentInputUsed = true; }
                    if (thruster.secondaryMomentUse == 1 || thruster.secondaryMomentUse == 2) { rollMomentInputUsed = true; }
                    else if (thruster.secondaryMomentUse == 3 || thruster.secondaryMomentUse == 4) { pitchMomentInputUsed = true; }
                    else if (thruster.secondaryMomentUse == 5 || thruster.secondaryMomentUse == 6) { yawMomentInputUsed = true; }
                }

                // Loop through all control surfaces to check moment inputs and brake input
                componentListSize = controlSurfaceList.Count;
                for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                {
                    // Get the control surface we are concerned with
                    controlSurface = controlSurfaceList[componentIndex];
                    // Check whether moment inputs are used for this control surface
                    if (controlSurface.type == ControlSurface.ControlSurfaceType.Aileron) { rollMomentInputUsed = true; }
                    else if (controlSurface.type == ControlSurface.ControlSurfaceType.Elevator) { pitchMomentInputUsed = true; }
                    else if (controlSurface.type == ControlSurface.ControlSurfaceType.Rudder) { yawMomentInputUsed = true; }
                    else if (controlSurface.type == ControlSurface.ControlSurfaceType.AirBrake) { longitudinalThrustInputUsed = true; }
                }
            }
            else
            {
                // Loop through all thrusters to check force inputs
                componentListSize = thrusterList.Count;
                for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                {
                    // Get the thruster we are concerned with
                    thruster = thrusterList[componentIndex];
                    // Check whether force inputs are used for this thruster
                    if (thruster.forceUse == 1 || thruster.forceUse == 2) { longitudinalThrustInputUsed = true; }
                    else if (thruster.forceUse == 3 || thruster.forceUse == 4) { verticalThrustInputUsed = true; }
                    else if (thruster.forceUse == 5 || thruster.forceUse == 6) { horizontalThrustInputUsed = true; }
                }

                // In arcade mode, moment inputs only depend on the pitch/roll/yaw accelerations being non-zero
                pitchMomentInputUsed = arcadePitchAcceleration > 0.01f;
                yawMomentInputUsed = arcadeYawAcceleration > 0.01f;
                rollMomentInputUsed = arcadeRollAcceleration > 0.01f;

                // In arcade mode, brakes can count for longitudinal inputs
                if (arcadeUseBrakeComponent) { longitudinalThrustInputUsed = true; }
            }
        }

        #endregion

        #region Update Position And Movement Data

        /// <summary>
        /// Reset the cached velocity data. Typically called
        /// when the ship rigidbody is set to kinematic.
        /// </summary>
        public void ResetVelocityData()
        {
            worldVelocity = Vector3.zero;
            localVelocity = Vector3.zero;
            absLocalVelocity = Vector3.zero;
            worldAngularVelocity = Vector3.zero;
            localAngularVelocity = Vector3.zero;
        }

        /// <summary>
        /// Update position and movement data using data obtained from a transform and a rigidbody.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="rigidbody"></param>
        public void UpdatePositionAndMovementData(Transform transform, Rigidbody rigidbody)
        {
            // Update data obtained from transform
            trfmPos = transform.position;
            trfmFwd = transform.forward;
            trfmRight = transform.right;
            trfmUp = transform.up;
            trfmRot = transform.rotation;
            trfmInvRot = Quaternion.Inverse(trfmRot);

            // Update data obtained from rigidbody
            rbodyPos = rigidbody.position;
            rbodyRot = rigidbody.rotation;
            rbodyInvRot = Quaternion.Inverse(rbodyRot);
            rbodyFwd = rbodyRot * Vector3.forward;
            rbodyRight = rbodyRot * Vector3.right;
            rbodyUp = rbodyRot * Vector3.up;
            worldVelocity = rigidbody.velocity;
            worldAngularVelocity = rigidbody.angularVelocity;
            localVelocity = rbodyInvRot * worldVelocity;
            localAngularVelocity = rbodyInvRot * worldAngularVelocity;
            rBodyInertiaTensor = rigidbody.inertiaTensor;
        }

        #endregion

        #region Calculate Force and Moment

        /// <summary>
        /// Calculate the equivalent local resultant force and moment for the ship and store them in the supplied reference vectors.
        /// Should be called during FixedUpdate(), and have UpdatePositionAndMovementData() called prior to it.
        /// </summary>
        /// <param name="localResultantForce"></param>
        /// <param name="localResultantMoment"></param>
        public void CalculateForceAndMoment(ref Vector3 localResultantForce, ref Vector3 localResultantMoment)
        {
#if SSC_SHIP_DEBUG
            float initStartTime = Time.realtimeSinceStartup;
#endif

            // TODO: OPTIMISATIONS
            // Reduce number of function calls
            // Use less math functions
            // Remove variable declarations
            // Try not to use transform functions and vector3 functions
            // Do as much precalculation as possible i.e. store a bool for isPhysicsBased etc.

            #region Initialisation

            // Read from Time property getter once
            float _deltaTime = Time.deltaTime;

            // Remember the previous frame's world velocity
            previousFrameWorldVelocity = worldVelocity;

            // Reset local resultant force and moment
            localResultantForce = Vector3.zero;
            localResultantMoment = Vector3.zero;

            // Reset any inputs that aren't used (forceInput and momentInput variables used temporarily)
            forceInput.x = horizontalThrustInputUsed ? pilotForceInput.x : 0f;
            forceInput.y = verticalThrustInputUsed ? pilotForceInput.y : 0f;
            forceInput.z = longitudinalThrustInputUsed ? pilotForceInput.z : 0f;
            momentInput.x = pitchMomentInputUsed ? pilotMomentInput.x : 0f;
            momentInput.y = yawMomentInputUsed ? pilotMomentInput.y : 0f;
            momentInput.z = rollMomentInputUsed ? pilotMomentInput.z : 0f;
            pilotForceInput = forceInput;
            pilotMomentInput = momentInput;
            // Reset force and moment input
            forceInput = Vector3.zero;
            momentInput = Vector3.zero;

            // Reset arcade force and moment vectors
            arcadeForceVector = Vector3.zero;
            arcadeMomentVector = Vector3.zero;

            // Get the ship physics model
            shipPhysicsModelPhysicsBased = shipPhysicsModel == ShipPhysicsModel.PhysicsBased;
            // Get the ship damage model
            shipDamageModelIsSimple = shipDamageModel == ShipDamageModel.Simple;

            #endregion

#if SSC_SHIP_DEBUG
            float initEndTime = Time.realtimeSinceStartup;

            float groundStartTime = Time.realtimeSinceStartup;
#endif

            #region Get Ground Information

            if (limitPitchAndRoll)
            {
                if (stickToGroundSurface || avoidGroundSurface)
                {
                    // TODO: This shouldn't always be centre of the object
                    //raycastRay.origin = trfmPos + (trfmFwd * 3.5f);
                    raycastRay.origin = trfmPos;
                    // If we were on the ground last frame, raycast in the "down" direction indicated by the ground normal
                    if (stickToGroundSurfaceEnabled) { raycastRay.direction = -worldTargetPlaneNormal; }
                    // Otherwise raycast in the ship's down direction
                    else { raycastRay.direction = -trfmUp; }

                    // Perform the raycast
                    if (Physics.Raycast(raycastRay, out raycastHitInfo, maxGroundCheckDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
                    {
                        // Single normal, derived from raycast hit normal
                        // This is the backup for smoothed normals and average height methods
                        worldTargetPlaneNormal = raycastHitInfo.normal;

                        if (groundNormalCalculationMode == GroundNormalCalculationMode.SmoothedNormal)
                        {
                            // Only use smoothed normals if the ground is using a mesh collider
                            // Check for a valid number of triangles, as sometimes can return -1. This potentially happens when
                            // Unity simplifies the collider (called degenerate) to improve performance. 
                            if (raycastHitInfo.collider != null && raycastHitInfo.collider.GetType() == typeof(MeshCollider) && raycastHitInfo.triangleIndex >= 0)
                            {
                                // Get the collider as a mesh collider
                                groundMeshCollider = (MeshCollider)raycastHitInfo.collider;
                                if (groundMeshCollider != null)
                                {
                                    // Check if this mesh collider is the cached one - if it is, we can use
                                    // the previously stored data without having to look it up from the mesh
                                    if (groundMeshCollider != cachedGroundMeshCollider)
                                    {
                                        // If this mesh collider is not the cached one, cache the mesh data for next time
                                        cachedGroundMeshCollider = groundMeshCollider;
                                        // Get the shared mesh being used by the ground collider and check that it isn't null
                                        cachedGroundMesh = groundMeshCollider.sharedMesh;
                                        if (cachedGroundMesh != null)
                                        {
                                            // Get the normals and triangle indices arrays
                                            cachedGroundMeshNormals = cachedGroundMesh.normals;
                                            cachedGroundMeshTriangles = cachedGroundMesh.triangles;
                                        }
                                    }

                                    // Get the normals from the triangle we hit using the cached data
                                    groundNormal0 = cachedGroundMeshNormals[cachedGroundMeshTriangles[raycastHitInfo.triangleIndex * 3 + 0]];
                                    groundNormal1 = cachedGroundMeshNormals[cachedGroundMeshTriangles[raycastHitInfo.triangleIndex * 3 + 1]];
                                    groundNormal2 = cachedGroundMeshNormals[cachedGroundMeshTriangles[raycastHitInfo.triangleIndex * 3 + 2]];
                                    // Get the barycentric coordinate of the point we hit - this gives us information as to
                                    // where in the triangle this point is
                                    barycentricCoord = raycastHitInfo.barycentricCoordinate;
                                    // Interpolate between the three normals using the barycentric coordinate to 
                                    // get the smoothed normal - this is how smooth shading works
                                    worldTargetPlaneNormal = groundMeshCollider.transform.TransformDirection(
                                        barycentricCoord[0] * groundNormal0 +
                                        barycentricCoord[1] * groundNormal1 +
                                        barycentricCoord[2] * groundNormal2).normalized;
                                }
                            }
                        }
                        //else if (groundNormalCalculationMode == GroundNormalCalculationMode.AverageHeight)
                        //{
                        //    float halfRectangleWidth = 3.5f;
                        //    float halfRectangleLength = 5.5f;
                        //    bool raycastFRHit, raycastFLHit, raycastRRHit, raycastRLHit;
                        //    float nUsableNormals = 0f;
                        //    Vector3 raycastFRHitPoint = Vector3.zero;
                        //    Vector3 raycastFLHitPoint = Vector3.zero;
                        //    Vector3 raycastRRHitPoint = Vector3.zero;
                        //    Vector3 raycastRLHitPoint = Vector3.zero;
                        //    RaycastHit testRaycastHitInfo;
                        //    Ray testRaycastRay = new Ray();
                        //    Vector3 averageNormal = Vector3.zero;

                        //    // Front-right raycast
                        //    testRaycastRay.origin = trfmPos + (trfmRight * halfRectangleWidth) + (trfmFwd * halfRectangleLength);
                        //    testRaycastRay.direction = -trfmUp;
                        //    if (Physics.Raycast(testRaycastRay, out testRaycastHitInfo, maxGroundCheckDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
                        //    {
                        //        raycastFRHit = true;
                        //        raycastFRHitPoint = testRaycastHitInfo.point;
                        //    }
                        //    else { raycastFRHit = false; }

                        //    // Front-left raycast
                        //    testRaycastRay.origin = trfmPos - (trfmRight * halfRectangleWidth) + (trfmFwd * halfRectangleLength);
                        //    testRaycastRay.direction = -trfmUp;
                        //    if (Physics.Raycast(testRaycastRay, out testRaycastHitInfo, maxGroundCheckDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
                        //    {
                        //        raycastFLHit = true;
                        //        raycastFLHitPoint = testRaycastHitInfo.point;
                        //    }
                        //    else { raycastFLHit = false; }

                        //    // Rear-right raycast
                        //    testRaycastRay.origin = trfmPos + (trfmRight * halfRectangleWidth) - (trfmFwd * halfRectangleLength);
                        //    testRaycastRay.direction = -trfmUp;
                        //    if (Physics.Raycast(testRaycastRay, out testRaycastHitInfo, maxGroundCheckDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
                        //    {
                        //        raycastRRHit = true;
                        //        raycastRRHitPoint = testRaycastHitInfo.point;
                        //    }
                        //    else { raycastRRHit = false; }

                        //    // Rear-left raycast
                        //    testRaycastRay.origin = trfmPos - (trfmRight * halfRectangleWidth) - (trfmFwd * halfRectangleLength);
                        //    testRaycastRay.direction = -trfmUp;
                        //    if (Physics.Raycast(testRaycastRay, out testRaycastHitInfo, maxGroundCheckDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
                        //    {
                        //        raycastRLHit = true;
                        //        raycastRLHitPoint = testRaycastHitInfo.point;
                        //    }
                        //    else { raycastRLHit = false; }

                        //    if (raycastFLHit && raycastFRHit && raycastRRHit)
                        //    {
                        //        nUsableNormals += 1f;
                        //        averageNormal += Vector3.Cross(raycastRRHitPoint - raycastFRHitPoint, raycastFLHitPoint - raycastFRHitPoint).normalized;
                        //    }
                        //    if (raycastFRHit && raycastRRHit && raycastRLHit)
                        //    {
                        //        nUsableNormals += 1f;
                        //        averageNormal += Vector3.Cross(raycastRLHitPoint - raycastRRHitPoint, raycastFRHitPoint - raycastRRHitPoint).normalized;
                        //    }
                        //    if (raycastRRHit && raycastRLHit && raycastFLHit)
                        //    {
                        //        nUsableNormals += 1f;
                        //        averageNormal += Vector3.Cross(raycastFLHitPoint - raycastRLHitPoint, raycastRRHitPoint - raycastRLHitPoint).normalized;
                        //    }
                        //    if (raycastRLHit && raycastFLHit && raycastFRHit)
                        //    {
                        //        nUsableNormals += 1f;
                        //        averageNormal += Vector3.Cross(raycastFRHitPoint - raycastFLHitPoint, raycastRLHitPoint - raycastFLHitPoint).normalized;
                        //    }

                        //    if (nUsableNormals > 0f) { worldTargetPlaneNormal = (averageNormal / nUsableNormals).normalized; }
                        //}

                        // Calculate a normal averaged from the last few frames
                        UpdateGroundNormalHistory(worldTargetPlaneNormal);
                        worldTargetPlaneNormal = GetDampedGroundNormal();

                        // Get target plane normal in local space
                        localTargetPlaneNormal = trfmInvRot * worldTargetPlaneNormal;

                        // Calculate distance to the ground in a direction parallel to the ground normal
                        // (from the ground directly beneath the ship)
                        currentPerpendicularGroundDist = Vector3.Dot(trfmPos - raycastHitInfo.point, worldTargetPlaneNormal);

                        //Debug.DrawRay(raycastRay.origin, raycastRay.direction.normalized * maxGroundCheckDistance, Color.blue);

                        // If required, use look ahead to check if there is an obstacle ahead we will need to go over
                        if (useGroundMatchLookAhead)
                        {
                            float maxRaycastDistance;
                            RaycastHit secondaryRaycastHitInfo;

                            // Loop through a series of different look ahead times, starting at half a second away
                            for (float groundMatchLookAheadTime = 0.5f; groundMatchLookAheadTime > 0.01f; groundMatchLookAheadTime -= 0.1f)
                            {
                                // Calculate the corresponding look ahead distance
                                float groundMatchLookAheadDistance = groundMatchLookAheadTime * localVelocity.z;

                                // Calculate the raycast direction
                                raycastRay.direction = (-currentPerpendicularGroundDist * worldTargetPlaneNormal) + (groundMatchLookAheadDistance * trfmFwd);
                                // Calculate the required raycast length to go down the same distance as the standard ground check
                                maxRaycastDistance = Mathf.Sqrt((currentPerpendicularGroundDist * currentPerpendicularGroundDist) +
                                    (groundMatchLookAheadDistance * groundMatchLookAheadDistance)) * (maxGroundCheckDistance / currentPerpendicularGroundDist);

                                // Perform the raycast
                                if (Physics.Raycast(raycastRay, out secondaryRaycastHitInfo, maxRaycastDistance, groundLayerMask,
                                    QueryTriggerInteraction.Ignore))
                                {
                                    // Calculate the perpendicular ground distance (projected onto the target normal)
                                    float futurePerpendicularGroundDistance = Vector3.Dot(trfmPos - secondaryRaycastHitInfo.point, worldTargetPlaneNormal);
                                    // If the perpendicular ground distance we have calculated is LESS than the current 
                                    // perpendicular ground distance (i.e. for the ground beneath the ship), use this
                                    // new ground distance instead
                                    if (futurePerpendicularGroundDistance <= currentPerpendicularGroundDist)
                                    {
                                        currentPerpendicularGroundDist = futurePerpendicularGroundDistance;
                                        //Debug.DrawRay(raycastRay.origin, raycastRay.direction.normalized * maxRaycastDistance, Color.green);
                                        // Exit the loop
                                        break;
                                    }
                                    //else
                                    //{
                                    //    Debug.DrawRay(raycastRay.origin, raycastRay.direction.normalized * maxRaycastDistance, Color.red);
                                    //}
                                }
                            }
                        }

                        // Set relevant variables, to indicate to later code that we have found a ground surface
                        limitPitchAndRollEnabled = true;
                        stickToGroundSurfaceEnabled = true;
                    }
                    else
                    {
                        // If we were on the ground on the previous frame, and now we are not,
                        // reset the PID controller to prevent spikes when we go back onto the ground again
                        if (stickToGroundSurfaceEnabled) { groundDistPIDController.ResetController(); }

                        limitPitchAndRollEnabled = orientUpInAir;
                        stickToGroundSurfaceEnabled = false;
                        if (limitPitchAndRollEnabled)
                        {
                            worldTargetPlaneNormal = Vector3.up;
                            // Update ground normal history
                            UpdateGroundNormalHistory(worldTargetPlaneNormal);
                            // Get target plane normal in local space
                            localTargetPlaneNormal = trfmInvRot * worldTargetPlaneNormal;
                        }
                        else
                        {
                            // Update ground normal history
                            UpdateGroundNormalHistory(trfmUp);
                        }
                    }
                }
                else
                {
                    limitPitchAndRollEnabled = true;
                    stickToGroundSurfaceEnabled = false;
                    worldTargetPlaneNormal = Vector3.up;
                    // Get target plane normal in local space
                    localTargetPlaneNormal = trfmInvRot * worldTargetPlaneNormal;
                }
            }
            else
            {
                limitPitchAndRollEnabled = false;
                stickToGroundSurfaceEnabled = false;
            }

            #endregion

#if SSC_SHIP_DEBUG
            float groundEndTime = Time.realtimeSinceStartup;
#endif

            #region Gravity

            // Calculate acceleration due to gravity in local space (storing the value for use later)
            localGravityAcceleration = trfmInvRot * gravityDirection * gravitationalAcceleration;
            // Then add it to local resultant force vector (multiplying by mass to turn it into a force)
            localResultantForce += localGravityAcceleration * mass;

            #endregion

#if SSC_SHIP_DEBUG
            float inputStartTime = Time.realtimeSinceStartup;
#endif

            #region Calculate Final Inputs

            // Calculate final input

            // | Assists/limiters info |
            // Rotational flight assist:
            // - Always on
            // - If the pilot has released an axial input but we are still spinning on that axis,
            //   modify the input to slow down the spin
            // Translational flight assist:
            // - Always on
            // - If the pilot has released a translational input but we are still moving on that axis,
            //   modify the input to slow down the movement
            // - Is overriden on y-axis by stick to ground surface
            // - Currently does not apply on z-axis
            // - Isn't available in arcade mode (it is instead replaced by velocity match code)
            // Brake flight assist:
            // - Off by default
            // - Translational flight assist on z-axis only
            // - Only applies when no z-axis input
            // - Only applies when fwd/back movement is non-zero and not close to zero
            // - Can optionally applies on local x (left-right) and/or y axis (up-down)
            // - Can be overridden in forwards direction by AutoCruise on PlayerInputModule
            // Limit pitch and roll:
            // - Overrides x and z axes of rotational flight assist
            // - Causes y-axis rotational flight assist to apply on an axis normal to the target plane
            // Stick to ground surface
            // - Overrides y axis of pilot force input

            // Moment inputs
            // - Rotational flight assist

            if (limitPitchAndRollEnabled)
            {
                // Ground angle match assist: Calculate a target pitch and roll from the ground angle
                // Pitch is still affected by pilot input but is clamped between min and max pitch
                // Roll is controlled by yaw input or strafe input
                // Calculate target plane normal in local space

                // Calculate target pitch (relative to the ground normal) in degrees
                // This is always dependent on the pitch input
                pitchAmountInput = pilotMomentInput.x * maxPitch;

                // Calculate target roll (relative to the ground normal) in degrees
                // a) From the yaw input
                if (rollControlMode == RollControlMode.YawInput) { rollAmountInput = pilotMomentInput.y * maxTurnRoll; }
                // b) From the strafe input
                else { rollAmountInput = pilotForceInput.x * maxTurnRoll; }

                // Calculate the maximum possible change in pitch/roll for this frame
                maxFramePitchDelta = _deltaTime * pitchSpeed;
                maxFrameRollDelta = _deltaTime * turnRollSpeed;

                // Move the pitch amount towards the target pitch input
                if (pitchAmountInput >= pitchAmount - maxFramePitchDelta && pitchAmountInput <= pitchAmount + maxFramePitchDelta) { pitchAmount = pitchAmountInput; }
                else if (pitchAmountInput > pitchAmount) { pitchAmount += maxFramePitchDelta; }
                else { pitchAmount -= maxFramePitchDelta; }

                // Move the roll amount towards the target roll input
                if (rollAmountInput >= rollAmount - maxFrameRollDelta && rollAmountInput <= rollAmount + maxFrameRollDelta) { rollAmount = rollAmountInput; }
                else if (rollAmountInput > rollAmount) { rollAmount += maxFrameRollDelta; }
                else { rollAmount -= maxFrameRollDelta; }

                // Calculate target local-space roll and pitch in degrees
                targetLocalPitch = ((float)Math.Atan2(localTargetPlaneNormal.z, localTargetPlaneNormal.y) * Mathf.Rad2Deg) + pitchAmount;
                targetLocalRoll = ((float)Math.Atan2(localTargetPlaneNormal.x, localTargetPlaneNormal.y) * Mathf.Rad2Deg) + rollAmount;

                //Debug.Log("Target pitch: " + targetLocalPitch.ToString("00.00") + ", target roll: " + targetLocalRoll.ToString("00.00"));

                if (shipPhysicsModelPhysicsBased)
                {
                    // Use the PID controllers to calculate the required moment inputs on the x and z axes
                    momentInput.z = rollPIDController.RequiredInput(targetLocalRoll, 0f, _deltaTime);
                    momentInput.x = pitchPIDController.RequiredInput(targetLocalPitch, 0f, _deltaTime);
                    momentInput.y = 0f;
                }
                else
                {
                    // Use the PID controllers to calculate the required moments on the x and z axes
                    arcadeMomentVector.z = rollPIDController.RequiredInput(targetLocalRoll, 0f, _deltaTime) * 50f * pitchRollMatchResponsiveness;
                    arcadeMomentVector.x = pitchPIDController.RequiredInput(targetLocalPitch, 0f, _deltaTime) * 50f * pitchRollMatchResponsiveness;
                    arcadeMomentVector.y = 0f;
                    // Completely reset moment input here
                    momentInput = Vector3.zero;
                }

                // Apply typical yaw rotation with respect to ground normal instead of local y coordinate
                // TODO: Surely this line can be optimised...?
                yawVelocity = Vector3.Dot(Vector3.Project(localAngularVelocity * Mathf.Rad2Deg, localTargetPlaneNormal), localTargetPlaneNormal);
                if (shipPhysicsModelPhysicsBased)
                {
                    // Activate rotational flight assist if pilot releases an input 
                    // or if input is in direction opposing rotational velocity
                    if ((pilotMomentInput.y > 0f) == (yawVelocity < 0f) || pilotMomentInput.y == 0f)
                    {
                        // Calculate the flight assist value on the ground normal axis
                        flightAssistValue = -yawVelocity / 100f * rotationalFlightAssistStrength;
                        // Choose whichever value has the highest absolute value: The flight assist value or the pilot input value
                        if ((flightAssistValue > 0f ? flightAssistValue : -flightAssistValue) >
                            (pilotMomentInput.y > 0f ? pilotMomentInput.y : -pilotMomentInput.y))
                        {
                            momentInput += localTargetPlaneNormal * flightAssistValue;
                        }
                        else { momentInput += localTargetPlaneNormal * pilotMomentInput.y; }
                    }
                    else { momentInput += localTargetPlaneNormal * pilotMomentInput.y; }
                }
                else
                {
                    momentInput += localTargetPlaneNormal * pilotMomentInput.y;
                    // Activate arcade rotational flight assist if pilot releases an input 
                    // or if input is in direction opposing rotational velocity
                    if ((pilotMomentInput.y > 0f) == (yawVelocity < 0f) || pilotMomentInput.y == 0f)
                    {
                        // Calculate the flight assist value on the ground normal axis
                        arcadeMomentVector += localTargetPlaneNormal * -yawVelocity * rotationalFlightAssistStrength;
                    }
                }
            }
            else
            {
                #region Rotational flight assist: Input to counteract rotational velocity
                if (shipPhysicsModelPhysicsBased)
                {
                    // Activate rotational flight assist if pilot releases an input 
                    // or if input is in direction opposing rotational velocity
                    if ((pilotMomentInput.x > 0f) == (localAngularVelocity.x < 0f) || pilotMomentInput.x == 0f)
                    {
                        // Calculate the flight assist value on this axis
                        flightAssistValue = -localAngularVelocity.x * Mathf.Rad2Deg / 100f * rotationalFlightAssistStrength;
                        // Choose whichever value has the highest absolute value: The flight assist value or the pilot input value
                        if ((flightAssistValue > 0f ? flightAssistValue : -flightAssistValue) >
                            (pilotMomentInput.x > 0f ? pilotMomentInput.x : -pilotMomentInput.x)) { momentInput.x = flightAssistValue; }
                        else { momentInput.x = pilotMomentInput.x; }
                    }
                    else { momentInput.x = pilotMomentInput.x; }
                    if ((pilotMomentInput.y > 0f) == (localAngularVelocity.y < 0f) || pilotMomentInput.y == 0f)
                    {
                        // Calculate the flight assist value on this axis
                        flightAssistValue = -localAngularVelocity.y * Mathf.Rad2Deg / 100f * rotationalFlightAssistStrength;
                        // Choose whichever value has the highest absolute value: The flight assist value or the pilot input value
                        if ((flightAssistValue > 0f ? flightAssistValue : -flightAssistValue) >
                            (pilotMomentInput.y > 0f ? pilotMomentInput.y : -pilotMomentInput.y)) { momentInput.y = flightAssistValue; }
                        else { momentInput.y = pilotMomentInput.y; }
                    }
                    else { momentInput.y = pilotMomentInput.y; }
                    if ((pilotMomentInput.z > 0f) == (localAngularVelocity.z > 0f) || pilotMomentInput.z == 0f)
                    {
                        // Calculate the flight assist value on this axis
                        flightAssistValue = localAngularVelocity.z * Mathf.Rad2Deg / 100f * rotationalFlightAssistStrength;
                        // Choose whichever value has the highest absolute value: The flight assist value or the pilot input value
                        if ((flightAssistValue > 0f ? flightAssistValue : -flightAssistValue) >
                            (pilotMomentInput.z > 0f ? pilotMomentInput.z : -pilotMomentInput.z)) { momentInput.z = flightAssistValue; }
                        else { momentInput.z = pilotMomentInput.z; }
                    }
                    else { momentInput.z = pilotMomentInput.z; }
                }
                else
                {
                    // Activate arcade rotational flight assist if pilot releases an input 
                    // or if input is in direction opposing rotational velocity
                    momentInput = pilotMomentInput;
                    if ((pilotMomentInput.x > 0f) == (localAngularVelocity.x < 0f) || pilotMomentInput.x == 0f)
                    {
                        arcadeMomentVector.x = -localAngularVelocity.x * Mathf.Rad2Deg * rotationalFlightAssistStrength;
                    }
                    if ((pilotMomentInput.y > 0f) == (localAngularVelocity.y < 0f) || pilotMomentInput.y == 0f)
                    {
                        arcadeMomentVector.y = -localAngularVelocity.y * Mathf.Rad2Deg * rotationalFlightAssistStrength;
                    }
                    if ((pilotMomentInput.z > 0f) == (localAngularVelocity.z > 0f) || pilotMomentInput.z == 0f)
                    {
                        arcadeMomentVector.z = localAngularVelocity.z * Mathf.Rad2Deg * rotationalFlightAssistStrength;
                    }
                }
                #endregion
            }

            #region Stability Flight Assist
            if (stabilityFlightAssistStrength > 0f)
            {
                // TODO I need to optimise the calculation section

                #region Calculate Current Pitch, Yaw and Roll

                // TODO I really need to optimise this properly
                // Calculate current pitch of the ship
                Vector3 pitchProjectionPlaneNormal = Vector3.Cross(Vector3.up, TransformForward);
                Vector3 pitchProjectedUpDirection = Vector3.ProjectOnPlane(TransformUp, pitchProjectionPlaneNormal);
                float stabilityCurrentPitch = Vector3.SignedAngle(Vector3.up, pitchProjectedUpDirection, pitchProjectionPlaneNormal);
                // Measure pitch with respect to target pitch direction
                stabilityCurrentPitch -= sFlightAssistTargetPitch;
                if (stabilityCurrentPitch > 180f) { stabilityCurrentPitch -= 360f; }
                else if (stabilityCurrentPitch < -180f) { stabilityCurrentPitch += 360f; }

                // Calculate current yaw of the ship
                // TODO this can be massively optimised by just setting the y-value to zero
                Vector3 yawProjectionPlaneNormal = Vector3.up;
                Vector3 yawProjectedForwardDirection = Vector3.ProjectOnPlane(TransformForward, yawProjectionPlaneNormal);
                float stabilityCurrentYaw = Vector3.SignedAngle(Vector3.forward, yawProjectedForwardDirection, yawProjectionPlaneNormal);
                // Measure yaw with respect to target yaw direction
                stabilityCurrentYaw -= sFlightAssistTargetYaw;
                if (stabilityCurrentYaw > 180f) { stabilityCurrentYaw -= 360f; }
                else if (stabilityCurrentYaw < -180f) { stabilityCurrentYaw += 360f; }

                // Calculate current roll of the ship
                Vector3 rollProjectionPlaneNormal = TransformForward;
                rollProjectionPlaneNormal.y = 0f;
                Vector3 rollProjectedUpDirection = Vector3.ProjectOnPlane(TransformUp, rollProjectionPlaneNormal);
                float stabilityCurrentRoll = -Vector3.SignedAngle(Vector3.up, rollProjectedUpDirection, rollProjectionPlaneNormal);
                // Measure roll with respect to target roll direction
                stabilityCurrentRoll -= sFlightAssistTargetRoll;
                if (stabilityCurrentRoll > 180f) { stabilityCurrentRoll -= 360f; }
                else if (stabilityCurrentRoll < -180f) { stabilityCurrentRoll += 360f; }

                #endregion

                #region Update Target Pitch, Yaw and Roll Values

                // How this works: There are two (mutually exclusive) modes of operation:
                // 1. Setting target values for pitch/yaw/roll
                // 2. Applying the stability assist for pitch/yaw/roll
                // For example, if the target pitch is being set, this means that the stability assist will not be used
                // for pitch in that frame
                bool allowPitchStabilityAssist = true;
                bool allowYawStabilityAssist = true;
                bool allowRollStabilityAssist = true;

                // Check if the pilot is initiating a manoeuvre on any of the three axes via pilot input
                bool initiatingPitchManoeuvre = pilotMomentInput.x > 0.01f || pilotMomentInput.x < -0.01f;
                bool initiatingYawManoeuvre = pilotMomentInput.y > 0.01f || pilotMomentInput.y < -0.01f;
                bool initiatingRollManoeuvre = pilotMomentInput.z > 0.01f || pilotMomentInput.z < -0.01f;

                // If there is currently some pilot pitch/yaw/roll input...
                if (initiatingPitchManoeuvre || initiatingRollManoeuvre || initiatingYawManoeuvre)
                {
                    // ... set the target pitch/yaw/roll
                    sFlightAssistTargetPitch += stabilityCurrentPitch;
                    sFlightAssistPitchPIDController.ResetController();
                    sFlightAssistTargetYaw += stabilityCurrentYaw;
                    sFlightAssistYawPIDController.ResetController();
                    sFlightAssistTargetRoll += stabilityCurrentRoll;
                    sFlightAssistRollPIDController.ResetController();
                    allowPitchStabilityAssist = false;
                    allowYawStabilityAssist = false;
                    allowRollStabilityAssist = false;

                    // If a manouevre has been initiated on a given axis via pilot input, remember it
                    // We can hence know when we are rotating on a particular axis due to pilot input
                    // or due to drag moments etc and act accordingly
                    // Note that pitch can affect yaw and vice versa
                    if (initiatingPitchManoeuvre || initiatingYawManoeuvre)
                    {
                        sFlightAssistRotatingPitch = true; 
                        sFlightAssistRotatingYaw = true;
                    }
                    if (initiatingRollManoeuvre) { sFlightAssistRotatingRoll = true; }
                }
                // Otherwise...
                else
                {
                    // If there is a significant pitch assist input to correct a pitch manoeuvre...
                    if (sFlightAssistRotatingPitch && (momentInput.x > 0.3f || momentInput.x < -0.3f))
                    {
                        // ... set the target pitch
                        sFlightAssistTargetPitch += stabilityCurrentPitch;
                        sFlightAssistPitchPIDController.ResetController();
                        allowPitchStabilityAssist = false;
                    }
                    // When the pitch assist moment input has reduced enough, we can assume we have finished
                    // any pitch manoeuvres initiated by the pilot
                    else { sFlightAssistRotatingPitch = false; }
                    // If there is a significant yaw assist input to correct a yaw manoeuvre...
                    if (sFlightAssistRotatingYaw && (momentInput.y > 0.3f || momentInput.y < -0.3f))
                    {
                        // ... set the target yaw
                        sFlightAssistTargetYaw += stabilityCurrentYaw;
                        sFlightAssistYawPIDController.ResetController();
                        allowYawStabilityAssist = false;
                    }
                    // When the yaw assist moment input has reduced enough, we can assume we have finished
                    // any yaw manoeuvres initiated by the pilot
                    else { sFlightAssistRotatingYaw = false; }
                    // If there is a significant roll assist input to correct a roll manoeuvre...
                    if (sFlightAssistRotatingRoll && (momentInput.z > 0.3f || momentInput.z < -0.3f))
                    {
                        // ... set the target roll
                        sFlightAssistTargetRoll += stabilityCurrentRoll;
                        sFlightAssistRollPIDController.ResetController();
                        allowRollStabilityAssist = false;
                    }
                    // When the roll assist moment input has reduced enough, we can assume we have finished
                    // any roll manoeuvres initiated by the pilot
                    else { sFlightAssistRotatingRoll = false; }
                }

                #endregion

                #region Apply Stability Assist

                if (allowPitchStabilityAssist)
                {
                    // Get input from pitch PID controller and apply it
                    momentInput.x += sFlightAssistPitchPIDController.RequiredInput(0f, stabilityCurrentPitch, _deltaTime);
                }
                
                if (allowYawStabilityAssist)
                {
                    // Get input from yaw PID controller and apply it
                    momentInput.y += sFlightAssistYawPIDController.RequiredInput(0f, stabilityCurrentYaw, _deltaTime);
                }

                if (allowRollStabilityAssist)
                {
                    // Get input from roll PID controller and apply it
                    momentInput.z += sFlightAssistRollPIDController.RequiredInput(0f, stabilityCurrentRoll, _deltaTime);
                }

                #endregion
            }
            #endregion

            if (shipPhysicsModelPhysicsBased)
            {
                // Scale moment inputs in physics-based mode - they generally need to be a lot less than force inputs
                momentInput.x *= pitchPower;
                momentInput.y *= yawPower;
                momentInput.z *= rollPower;

                // Clamp moment inputs to be between -1 and 1. We don't do this in arcade mode so that moment assists/limits
                // can use more than the specified maximum moments if necessary
                if (momentInput.x > 1f) { momentInput.x = 1f; }
                else if (momentInput.x < -1f) { momentInput.x = -1f; }
                if (momentInput.y > 1f) { momentInput.y = 1f; }
                else if (momentInput.y < -1f) { momentInput.y = -1f; }
                if (momentInput.z > 1f) { momentInput.z = 1f; }
                else if (momentInput.z < -1f) { momentInput.z = -1f; }
            }

            // Force inputs
            // - Translational flight assist (physics-based only)
            // - Stick to ground surface (physics-based only: arcade equivalent is applied separately)

            if (shipPhysicsModelPhysicsBased)
            {
                // Translational flight assist: Input to counteract translational velocity

                // Activate translational flight assist if pilot releases an input 
                // or if input is in direction opposing velocity
                if ((pilotForceInput.x > 0f) == (localVelocity.x < 0f) || pilotForceInput.x == 0f)
                {
                    // Calculate the flight assist value on this axis
                    flightAssistValue = -localVelocity.x / 100f * translationalFlightAssistStrength;
                    if (flightAssistValue > 1f) { flightAssistValue = 1f; }
                    else if (flightAssistValue < -1f) { flightAssistValue = -1f; }
                    // Choose whichever value has the highest absolute value: The flight assist value or the pilot input value
                    if ((flightAssistValue > 0f ? flightAssistValue : -flightAssistValue) >
                        (pilotForceInput.x > 0f ? pilotForceInput.x : -pilotForceInput.x)) { forceInput.x = flightAssistValue; }
                    else { forceInput.x = pilotForceInput.x; }
                }
                else { forceInput.x = pilotForceInput.x; }

                if (stickToGroundSurfaceEnabled)
                {
                    // Get input from ground distance PID controller
                    forceInput.y = groundDistPIDController.RequiredInput(targetGroundDistance, currentPerpendicularGroundDist, _deltaTime);

                    // NOTE: This hasn't been tested yet in physics-model mode
                    if (avoidGroundSurface)
                    {
                        // Only override user input if the ship is going below the targetGroundDistance
                        forceInput.y = forceInput.y > 0f ? forceInput.y : pilotForceInput.y;
                    }
                }
                else
                {
                    // Activate translational flight assist if pilot releases an input 
                    // or if input is in direction opposing velocity
                    if ((pilotForceInput.y > 0f) == (localVelocity.y < 0f) || pilotForceInput.y == 0f)
                    {
                        // Calculate the flight assist value on this axis
                        flightAssistValue = -localVelocity.y / 100f * translationalFlightAssistStrength;
                        if (flightAssistValue > 1f) { flightAssistValue = 1f; }
                        else if (flightAssistValue < -1f) { flightAssistValue = -1f; }
                        // Choose whichever value has the highest absolute value: The flight assist value or the pilot input value
                        if ((flightAssistValue > 0f ? flightAssistValue : -flightAssistValue) >
                            (pilotForceInput.y > 0f ? pilotForceInput.y : -pilotForceInput.y)) { forceInput.y = flightAssistValue; }
                        else { forceInput.y = pilotForceInput.y; }
                    }
                    else { forceInput.y = pilotForceInput.y; }
                }

                // Force input on z-axis is always just pilot input
                forceInput.z = pilotForceInput.z;
            }
            else
            {
                forceInput.x = pilotForceInput.x;
                // In arcade mode, stick-to-ground force isn't achieved through thrusters
                // Currently this implementation won't allow for thrust pushing up/down from ground
                // to activate particle trails in non-physics-based mode...
                forceInput.y = stickToGroundSurfaceEnabled ? 0f : pilotForceInput.y;
                forceInput.z = pilotForceInput.z;
            }

            #region Brake flight assist
            // Pilot or AI releases input and ship is moving forwards or backwards.
            // z-axis is always on when strength > 0 for backward compatibility.
            // Can optionally apply on local x (left-right) and/or y axis (up-down)
            // Works within the speed range configured.
            // Does not make adjustments if velocity is very near zero.

            #region Brake Flight Assist Z-Axis
            if (brakeFlightAssistStrength > 0f && forceInput.z == 0f && localVelocity.z > brakeFlightAssistMinSpeed && localVelocity.z < brakeFlightAssistMaxSpeed && (localVelocity.z < -0.01f || localVelocity.z > 0.01f))
            {
                // Max brakeFlightAssistStrength is 10, so div by 10 (x 0.1)
                // Apply thrust in the opposite direction to current z-axis velocity
                flightAssistValue = (localVelocity.z < 0f ? brakeFlightAssistStrength : -brakeFlightAssistStrength) * 0.1f;

                // Dampen based on how close velo is to 0 using a x^2 curve when velo between -10 and +10 m/s.
                if (localVelocity.z > 0f && localVelocity.z < 10f) { flightAssistValue *= localVelocity.z / 10f; }
                else if (localVelocity.z < 0f && localVelocity.z > -10f) { flightAssistValue *= localVelocity.z / -10f; }

                // Clamp -1.0 to 1.0
                if (flightAssistValue > 1f) { flightAssistValue = 1f; }
                else if (flightAssistValue < -1f) { flightAssistValue = -1f; }

                forceInput.z = flightAssistValue;
            }
            #endregion

            #region Brake Flight Assist X-Axis
            if (brakeFlightAssistStrengthX > 0f && forceInput.x == 0f && localVelocity.x > brakeFlightAssistMinSpeed && localVelocity.x < brakeFlightAssistMaxSpeed && (localVelocity.x < -0.01f || localVelocity.x > 0.01f))
            {
                // Max brakeFlightAssistStrengthX is 10, so div by 10 (x 0.1)
                // Apply thrust in the opposite direction to current x-axis velocity
                flightAssistValue = (localVelocity.x < 0f ? brakeFlightAssistStrengthX : -brakeFlightAssistStrengthX) * 0.1f;

                // Dampen based on how close velo is to 0 using a x^2 curve when velo between -10 and +10 m/s.
                if (localVelocity.x > 0f && localVelocity.x < 10f) { flightAssistValue *= localVelocity.x / 10f; }
                else if (localVelocity.x < 0f && localVelocity.x > -10f) { flightAssistValue *= localVelocity.x / -10f; }

                // Clamp -1.0 to 1.0
                if (flightAssistValue > 1f) { flightAssistValue = 1f; }
                else if (flightAssistValue < -1f) { flightAssistValue = -1f; }

                forceInput.x = flightAssistValue;
            }
            #endregion

            #region Brake Flight Assist Y-Axis
            if (brakeFlightAssistStrengthY > 0f && forceInput.y == 0f && localVelocity.y > brakeFlightAssistMinSpeed && localVelocity.y < brakeFlightAssistMaxSpeed && (localVelocity.y < -0.01f || localVelocity.y > 0.01f))
            {
                // Max brakeFlightAssistStrengthY is 10, so div by 10 (x 0.1)
                // Apply thrust in the opposite direction to current y-axis velocity
                flightAssistValue = (localVelocity.y < 0f ? brakeFlightAssistStrengthY : -brakeFlightAssistStrengthY) * 0.1f;

                // Dampen based on how close velo is to 0 using a x^2 curve when velo between -10 and +10 m/s.
                if (localVelocity.y > 0f && localVelocity.y < 10f) { flightAssistValue *= localVelocity.y / 10f; }
                else if (localVelocity.y < 0f && localVelocity.y > -10f) { flightAssistValue *= localVelocity.y / -10f; }

                // Clamp -1.0 to 1.0
                if (flightAssistValue > 1f) { flightAssistValue = 1f; }
                else if (flightAssistValue < -1f) { flightAssistValue = -1f; }

                forceInput.y = flightAssistValue;
            }
            #endregion
            #endregion

            #region Input Control - 2.5D
            // TODO probably can significantly optimise the input control axis code

            if ((int)inputControlAxis > 0)
            {
                // Y axis for side-view OR top-down flight
                if ((int)inputControlAxis == 2)
                {
                    // Project the transform right direction onto the XZ plane
                    Vector3 flatTrfmRight = TransformRight;
                    flatTrfmRight.y = 0f;
                    // Project the transform forward direction onto the XZ plane...
                    Vector3 targetForward = TransformForward;
                    targetForward.y = 0f;
                    // ... then onto the plane of the ship's up and forwards directions while keeping it on the XZ plane
                    // To do this step we subtract the projection of this vector onto flatTrfmRight
                    targetForward -= (Vector3.Dot(targetForward, flatTrfmRight) / Vector3.Dot(flatTrfmRight, flatTrfmRight)) * flatTrfmRight;
                    // Measure the angle from this projected forwards direction (which will be the target) to the current forwards direction
                    float inputControlLimitAngleDelta = Vector3.SignedAngle(targetForward, TransformForward, TransformRight);

                    if (shipPhysicsModelPhysicsBased)
                    {
                        // Physics-based: Set the inputs to counteract any displacement from the target position/rotation
                        forceInput.y = inputAxisForcePIDController.RequiredInput(inputControlLimit, TransformPosition.y, _deltaTime);
                        momentInput.x = inputAxisMomentPIDController.RequiredInput(0f, inputControlLimitAngleDelta, _deltaTime);
                    }
                    else
                    {
                        // Arcade: Use arcade force/moment to counteract any displacement from the target position/rotation
                        arcadeForceVector.y += inputAxisForcePIDController.RequiredInput(inputControlLimit, TransformPosition.y, _deltaTime);
                        arcadeMomentVector.x += inputAxisMomentPIDController.RequiredInput(0f, inputControlLimitAngleDelta, _deltaTime);
                        // Set inputs to zero
                        forceInput.y = 0f;
                        momentInput.x = 0f;
                    }
                }
                // X axis for side-scroller-like flight
                else if ((int)inputControlAxis == 1)
                {
                    // Calculate the normal to the plane we want to remain in
                    Vector3 targetInputControlPlaneNormal = Quaternion.Euler(0f, inputControlForwardAngle, 0f) * Vector3.right;

                    // Project the transform up direction onto the YZ plane
                    Vector3 flatTrfmUp = TransformUp;
                    flatTrfmUp -= Vector3.Project(flatTrfmUp, targetInputControlPlaneNormal);
                    // Project the transform forward direction onto the YZ plane...
                    Vector3 targetForward = TransformForward;
                    targetForward -= Vector3.Project(targetForward, targetInputControlPlaneNormal);
                    // ... then onto the plane of the ship's right and forwards directions while keeping it on the YZ plane
                    // To do this step we subtract the projection of this vector onto flatTrfmUp
                    targetForward -= (Vector3.Dot(targetForward, flatTrfmUp) / Vector3.Dot(flatTrfmUp, flatTrfmUp)) * flatTrfmUp;
                    // Measure the angle from this projected forwards direction (which will be the target) to the current forwards direction
                    float inputControlLimitAngleDelta = Vector3.SignedAngle(targetForward, TransformForward, TransformUp);

                    if (shipPhysicsModelPhysicsBased)
                    {
                        // Physics-based: Set the inputs to counteract any displacement from the target position/rotation
                        forceInput.x = inputAxisForcePIDController.RequiredInput(inputControlLimit, TransformPosition.x, _deltaTime);
                        momentInput.y = inputAxisMomentPIDController.RequiredInput(0f, inputControlLimitAngleDelta, _deltaTime);
                    }
                    else
                    {
                        // Arcade: Use arcade force/moment to counteract any displacement from the target position/rotation
                        arcadeForceVector.x += inputAxisForcePIDController.RequiredInput(inputControlLimit, TransformPosition.x, _deltaTime);
                        arcadeMomentVector.y += inputAxisMomentPIDController.RequiredInput(0f, inputControlLimitAngleDelta, _deltaTime);
                        // Set inputs to zero
                        forceInput.x = 0f;
                        momentInput.y = 0f;
                    }
                }
            }
            #endregion

            #endregion

#if SSC_SHIP_DEBUG
            float inputEndTime = Time.realtimeSinceStartup;

            float thrusterStartTime = Time.realtimeSinceStartup;
#endif

            #region Thrusters

            if (thrusterList != null)
            {
                componentListSize = thrusterList.Count;
                for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                {
                    // Get the thruster we are concerned with
                    thruster = thrusterList[componentIndex];
                    // Calculate thruster input - for this we need to know what type of thruster
                    // this is so that we can read the right input/s
                    // First set thruster input to zero
                    thruster.currentInput = 0f;

                    // Thrusters with max heat (temperature) or with no fuel produce no thrust etc.
                    // Check heat before fuels, so that if we run out of fuel, thruster
                    // can still cool down.

                    if (thruster.heatLevel >= 100f || (useCentralFuel ? centralFuelLevel <= 0f : thruster.fuelLevel <= 0f))
                    {
                        // Check if we need to cool the thruster down
                        thruster.ManageHeat(_deltaTime);
                        continue;
                    }

                    // Then add inputs for force/moments to give a final input...
                    switch (thruster.forceUse)
                    {
                        // Not a force thruster
                        case 0: break;
                        // Forward thruster
                        case 1: thruster.currentInput += (forceInput.z > 0f ? forceInput.z : 0f); break;
                        // Backward thruster
                        case 2: thruster.currentInput += (forceInput.z < 0f ? -forceInput.z : 0f); break;
                        // Upward thruster
                        case 3: thruster.currentInput += (forceInput.y > 0f ? forceInput.y : 0f); break;
                        // Downward thruster
                        case 4: thruster.currentInput += (forceInput.y < 0f ? -forceInput.y : 0f); break;
                        // Rightward thruster
                        case 5: thruster.currentInput += (forceInput.x > 0f ? forceInput.x : 0f); break;
                        // Leftward thruster
                        case 6: thruster.currentInput += (forceInput.x < 0f ? -forceInput.x : 0f); break;
                        // Not a valid thruster type
                        #if UNITY_EDITOR
                        default: Debug.LogWarning("Ship - Invalid thruster force use: " + thruster.forceUse.ToString()); break;
                        #endif
                    }

                    // Only allow thrusters to affect rotation when using the physics-based model
                    if (shipPhysicsModelPhysicsBased)
                    {
                        // Addition of moment inputs can be negative i.e. if we need to rotate in the opposite direction, add weighting
                        // to tell this thruster not to activate

                        switch (thruster.primaryMomentUse)
                        {
                            // Not a moment thruster
                            case 0: break;
                            // Positive roll thruster
                            case 1: if (momentInput.z / rollPower >= -(1f - (steeringThrusterPriorityLevel * 0.9f))) { thruster.currentInput += momentInput.z; } else { thruster.currentInput = 0f; } break;
                            // Negative roll thruster
                            case 2: if (momentInput.z / rollPower <= (1f - (steeringThrusterPriorityLevel * 0.9f))) { thruster.currentInput -= momentInput.z; } else { thruster.currentInput = 0f; } break;
                            // Positive pitch thruster
                            case 3: if (momentInput.x / pitchPower >= -(1f - (steeringThrusterPriorityLevel * 0.9f))) { thruster.currentInput += momentInput.x; } else { thruster.currentInput = 0f; } break;
                            // Negative pitch thruster
                            case 4: if (momentInput.x / pitchPower <= (1f - (steeringThrusterPriorityLevel * 0.9f))) { thruster.currentInput -= momentInput.x; } else { thruster.currentInput = 0f; } break;
                            // Positive yaw thruster
                            case 5: if (momentInput.y / yawPower >= -(1f - (steeringThrusterPriorityLevel * 0.9f))) { thruster.currentInput += momentInput.y; } else { thruster.currentInput = 0f; } break;
                            // Negative yaw thruster
                            case 6: if (momentInput.y / yawPower <= (1f - (steeringThrusterPriorityLevel * 0.9f))) { thruster.currentInput -= momentInput.y; } else { thruster.currentInput = 0f; } break;
                            // Not a valid thruster type
                            #if UNITY_EDITOR
                            default: Debug.LogWarning("Ship - Invalid thruster moment use: " + thruster.primaryMomentUse.ToString()); break;
                            #endif
                        }
                        switch (thruster.secondaryMomentUse)
                        {
                            // Not a moment thruster
                            case 0: break;
                            // Positive roll thruster
                            case 1: thruster.currentInput += momentInput.z; break;
                            // Negative roll thruster
                            case 2: thruster.currentInput -= momentInput.z; break;
                            // Positive pitch thruster
                            case 3: thruster.currentInput += momentInput.x; break;
                            // Negative pitch thruster
                            case 4: thruster.currentInput -= momentInput.x; break;
                            // Positive yaw thruster
                            case 5: thruster.currentInput += momentInput.y; break;
                            // Negative yaw thruster
                            case 6: thruster.currentInput -= momentInput.y; break;
                            // Not a valid thruster type
                            #if UNITY_EDITOR
                            default: Debug.LogWarning("Ship - Invalid thruster moment use: " + thruster.secondaryMomentUse.ToString()); break;
                            #endif
                        }
                    }

                    // Calculate thrust force

                    // Allow for the thruster to be throttled up and down over time
                    // Input is clamped between 0 and 1 (can only act within physical constraints of thruster)
                    thruster.SmoothThrusterInput(_deltaTime);
                    thrustForce = thruster.thrustDirectionNormalised * thruster.maxThrust * thruster.currentInput;

                    if (useCentralFuel)
                    {
                        if (thruster.fuelBurnRate > 0f && thruster.currentInput > 0f)
                        {
                            // Burn fuel independently to the health level. A damaged thruster will burn the same
                            // amount of fuel but produce less thrust
                            SetFuelLevel(centralFuelLevel - (thruster.currentInput * thruster.fuelBurnRate * _deltaTime));
                        }
                    }
                    else
                    {
                        thruster.BurnFuel(_deltaTime);
                    }

                    thruster.ManageHeat(_deltaTime);

                    // Adjust for thruster damage
                    if (!shipDamageModelIsSimple && thruster.damageRegionIndex > -1)
                    {
                        thrustForce *= thruster.CurrentPerformance;
                    }

                    // Add calculated thrust force to local resultant force and moment
                    localResultantForce += thrustForce;
                    // Only allow thrusters to affect rotation when using the physics-based model
                    if (shipPhysicsModelPhysicsBased)
                    {
                        localResultantMoment += Vector3.Cross(thruster.relativePosition - centreOfMass, thrustForce);
                    }
                }
            }

            #endregion

#if SSC_SHIP_DEBUG
            float thrusterEndTime = Time.realtimeSinceStartup;

            float aeroStartTime = Time.realtimeSinceStartup;
#endif

            #region Boost
            // Is boost currently in operation?
            if (boostTimer > 0f)
            {
                boostTimer -= _deltaTime;
                localResultantForce += boostDirection * boostForce;
            }
            #endregion

            #region Aerodynamics

            // Dynamic viscosity of air at 25 deg C and 1 atm: 18.37 x 10^-6 Pa s
            // Density of air on earth at low altitudes: 1.293 kg/m^3
            // Aircraft head-on drag coefficient: ~0.05

            if (mediumDensity > 0f)
            {
                //float pDragStartTime = Time.realtimeSinceStartup;

                #region Profile Drag

                // Profile drag calculation

                // Calculate local drag force vector
                // Drag force magnitude = 0.5 * fluid density * velocity squared * drag coefficient * cross-sectional area
                localDragForce = 0.5f * mediumDensity * -localVelocity;
                localDragForce.x *= dragCoefficients.x * dragReferenceAreas.x * (localVelocity.x > 0 ? localVelocity.x : -localVelocity.x);
                localDragForce.y *= dragCoefficients.y * dragReferenceAreas.y * (localVelocity.y > 0 ? localVelocity.y : -localVelocity.y);
                localDragForce.z *= dragCoefficients.z * dragReferenceAreas.z * (localVelocity.z > 0 ? localVelocity.z : -localVelocity.z);
                // Add calculated drag force to local resultant force
                localResultantForce += localDragForce;

                if (!disableDragMoments)
                {
                    // Add calculated drag force to local resultant moment
                    // Centre of drag is precalculated separately for each axis

                    localResultantMoment.x += ((centreOfDragXMoment.y - centreOfMass.y) * localDragForce.z) * dragXMomentMultiplier;
                    localResultantMoment.x += ((centreOfDragXMoment.z - centreOfMass.z) * -localDragForce.y) * dragXMomentMultiplier;

                    localResultantMoment.y += ((centreOfDragYMoment.x - centreOfMass.x) * -localDragForce.z) * dragYMomentMultiplier;
                    localResultantMoment.y += ((centreOfDragYMoment.z - centreOfMass.z) * localDragForce.x) * dragYMomentMultiplier;

                    localResultantMoment.z += ((centreOfDragZMoment.x - centreOfMass.x) * localDragForce.y) * dragZMomentMultiplier;
                    localResultantMoment.z += ((centreOfDragZMoment.y - centreOfMass.y) * -localDragForce.x) * dragZMomentMultiplier;
                }

                #endregion

                //float pDragEndTime = Time.realtimeSinceStartup;

                //float aDragStartTime = Time.realtimeSinceStartup;

                #region Angular Profile Drag

                // Angular drag calculation

                if (angularDragFactor > 0f || shipPhysicsModelPhysicsBased)
                {
                    // TODO: Could the length, width and height be precalculated?
                    // Step 1: We are modelling the drag as if it is occuring on a rectangular prism,
                    // so calculate the hypothetical length, width and height of that prism from the given areas of each side
                    dragLength = (float)Math.Sqrt(dragReferenceAreas.x * dragReferenceAreas.y / dragReferenceAreas.z);
                    dragWidth = dragReferenceAreas.y / dragLength;
                    dragHeight = dragReferenceAreas.z / dragWidth;

                    // Step 2: Calculate the angular drag moment on each axis

                    // TODO: Re-comment this section
                    // TODO: Could these values be precalculated?
                    quarticLengthXProj = IntegerPower((dragLength / 2) + centreOfMass.z - centreOfDragYMoment.z, 4) + IntegerPower((dragLength / 2) - centreOfMass.z + centreOfDragYMoment.z, 4);
                    quarticLengthYProj = IntegerPower((dragLength / 2) + centreOfMass.z - centreOfDragXMoment.z, 4) + IntegerPower((dragLength / 2) - centreOfMass.z + centreOfDragXMoment.z, 4);
                    quarticWidthYProj = IntegerPower((dragWidth / 2) + centreOfMass.x - centreOfDragZMoment.x, 4) + IntegerPower((dragWidth / 2) - centreOfMass.x + centreOfDragZMoment.x, 4);
                    quarticWidthZProj = IntegerPower((dragWidth / 2) + centreOfMass.x - centreOfDragYMoment.x, 4) + IntegerPower((dragWidth / 2) - centreOfMass.x + centreOfDragYMoment.x, 4);
                    quarticHeightXProj = IntegerPower((dragHeight / 2) + centreOfMass.y - centreOfDragZMoment.y, 4) + IntegerPower((dragHeight / 2) - centreOfMass.y + centreOfDragZMoment.y, 4);
                    quarticHeightZProj = IntegerPower((dragHeight / 2) + centreOfMass.y - centreOfDragXMoment.y, 4) + IntegerPower((dragHeight / 2) - centreOfMass.y + centreOfDragXMoment.y, 4);
                    if (shipPhysicsModelPhysicsBased) { angularDragMultiplier = 1f; }
                    else { angularDragMultiplier = (float)Math.Pow(10f, angularDragFactor - 1f); }
                    // Formula: For a rotating rectangle with the pivot at one end:
                    // Drag moment = 1/8 * fluid density * angular velocity squared * height * drag coefficient * length to the power of four
                    // X-axis angular drag
                    localResultantMoment.x += mediumDensity / 8f * localAngularVelocity.x * localAngularVelocity.x * dragWidth *
                        dragCoefficients.y * quarticLengthYProj * (localAngularVelocity.x > 0f ? -1f : 1f) * angularDragMultiplier;
                    localResultantMoment.x += mediumDensity / 8f * localAngularVelocity.x * localAngularVelocity.x * dragWidth *
                        dragCoefficients.z * quarticHeightZProj * (localAngularVelocity.x > 0f ? -1f : 1f) * angularDragMultiplier;
                    // Y-axis angular drag
                    localResultantMoment.y += mediumDensity / 8f * localAngularVelocity.y * localAngularVelocity.y * dragHeight *
                        dragCoefficients.x * quarticLengthXProj * (localAngularVelocity.y > 0f ? -1f : 1f) * angularDragMultiplier;
                    localResultantMoment.y += mediumDensity / 8f * localAngularVelocity.y * localAngularVelocity.y * dragHeight *
                        dragCoefficients.z * quarticWidthZProj * (localAngularVelocity.y > 0f ? -1f : 1f) * angularDragMultiplier;
                    // Z-axis angular drag
                    localResultantMoment.z += mediumDensity / 8f * localAngularVelocity.z * localAngularVelocity.z * dragLength *
                        dragCoefficients.y * quarticWidthYProj * (localAngularVelocity.z > 0f ? -1f : 1f) * angularDragMultiplier;
                    localResultantMoment.z += mediumDensity / 8f * localAngularVelocity.z * localAngularVelocity.z * dragLength *
                        dragCoefficients.x * quarticHeightXProj * (localAngularVelocity.z > 0f ? -1f : 1f) * angularDragMultiplier;
                }

                #endregion

                //float aDragEndTime = Time.realtimeSinceStartup;

                //float wingsStartTime = Time.realtimeSinceStartup;

                // Calculate the "true airspeed" - this is just velocity in forwards direction
                aerodynamicTrueAirspeed = localVelocity.z;

                #region Wings

                // Wing simulation

                componentListSize = wingList == null ? 0 : wingList.Count;
                for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                {
                    // Get the wing we are concerned with
                    wing = wingList[componentIndex];
                    // Only calculate lift if some component of the velocity is in the negative airflow direction
                    if (aerodynamicTrueAirspeed > 0f)
                    {
                        //float aoaStartTime = Time.realtimeSinceStartup;

                        #region Angle of Attack

                        // OLD UNOPTIMISED CODE. TODO: Delete
                        ////Vector3 wingPlaneNormal = Vector3.Cross(wing.liftDirection, wing.airflowDirection);
                        //wingPlaneNormal = Vector3.Cross(wing.liftDirection, Vector3.back);
                        //localVeloInWingPlane = Vector3.ProjectOnPlane(localVelocity, wingPlaneNormal);
                        //// Is this the right direction?
                        ////float angleOfAttack = Vector3.SignedAngle(-wing.airflowDirection, localVeloInWingPlane, wingPlaneNormal) + wing.angleOfAttack;
                        //wingAngleOfAttack = Vector3.SignedAngle(Vector3.forward, localVeloInWingPlane, wingPlaneNormal) + wing.angleOfAttack;

                        // Project the local velocity into the plane containing the lift direction and the forwards (negative airflow) direction
                        // Take the cross product of lift direction and forwards direction to get the normal of the plane
                        // Normalise it for the dot product in the next step
                        //wingPlaneNormal = Vector3.Cross(wing.liftDirectionNormalised, Vector3.forward).normalized;
                        wingPlaneNormal.x = wing.liftDirectionNormalised.y;
                        wingPlaneNormal.y = -wing.liftDirectionNormalised.x;
                        wingPlaneNormal.z = 0f;
                        wingPlaneNormal.Normalize();
                        //localVeloInWingPlane = (localVelocity - (Vector3.Dot(localVelocity, wingPlaneNormal) * wingPlaneNormal)).normalized;
                        // Multiplty the dot product of the local velocity and the wing plane normal with the wing plane normal to 
                        // get the component of the local velocity in the direction of the wing plane normal, then subtract
                        // this vector from the local velocity to project it into the desired plane
                        // Then normalise the vector for use in the dot product of the next step
                        // NOTE: The dot product does not require a z component as the z component of wing plane normal will always be zero,
                        //       as it is perpendicular to the forwards direction
                        localVeloInWingPlane = (localVelocity - (((localVelocity.x * wingPlaneNormal.x) + (localVelocity.y * wingPlaneNormal.y)) * wingPlaneNormal)).normalized;

                        // The angle of attack of the wing is equal to:
                        // The angle between the lift direction and the velocity in the wing plane (computed via arccos of dot product) MINUS
                        // The angle between the lift direction and the forwards direction PLUS
                        // The angle of attack of the wing due to camber
                        // Resultant of the first thing minus the second gives the angle between forwards direction and velocity, with
                        // forwards above velocity resulting in positive numbers

                        wingAngleOfAttack = ((float)(Math.Acos(Mathf.Clamp((wing.liftDirectionNormalised.x * localVeloInWingPlane.x) +
                                                            (wing.liftDirectionNormalised.y * localVeloInWingPlane.y) +
                                                            (wing.liftDirectionNormalised.z * localVeloInWingPlane.z), -1f, 1f)) -
                                                        Math.Acos(Mathf.Clamp(wing.liftDirectionNormalised.z, -1f, 1f))) * Mathf.Rad2Deg) +
                                                        wing.angleOfAttack;

                        #endregion

                        //float aoaEndTime = Time.realtimeSinceStartup;

                        //float lcStartTime = Time.realtimeSinceStartup;

                        #region Lift Coefficient

                        phi0 = (20f * wingStallEffect) - 25f;
                        phi1 = 15f;
                        phi2 = 45f - (25f * wingStallEffect);
                        phi3 = 75f - (50f * wingStallEffect);

                        //// If angle of attack is not between the min and max wing angles, the wing will stall and not produce any lift
                        //if (angleOfAttack > -5f && angleOfAttack < 15f)
                        //{
                        //    // Approximation of lift coefficient, adapted from thin airfoil theory: 
                        //    // Lift coefficient = (m * angle of attack) + d
                        //    // Where m = max lift coefficient / (max wing angle - min wing angle) and d = -m * min wing angle
                        //    // Max lift coefficient = 1.5, min wing angle = -5 degrees, max wing angle = 15 degrees
                        //    float liftCoefficient = (0.075f * angleOfAttack) + 0.375f;

                        //}

                        // TODO: NEED A COMMENT FOR BELOW THEORY
                        // Also should look at whether I should just enable/disable stalling
                        // and just use predetermined values
                        // Also, possibly stall angle actually comes from chord (wing width)?

                        // Stall region
                        if (wingAngleOfAttack < phi0) { liftCoefficient = 0f; }
                        // Increasing lift region
                        else if (wingAngleOfAttack < phi1) { liftCoefficient = 1.6f * (wingAngleOfAttack - phi0) / (phi1 - phi0); }
                        // Maximum lift region
                        else if (wingAngleOfAttack < phi2) { liftCoefficient = 1.6f; }
                        // Falloff region
                        else if (wingAngleOfAttack < phi3) { liftCoefficient = 1.6f * (wingAngleOfAttack - phi3) / (phi2 - phi3); }
                        // Stall region
                        else { liftCoefficient = 0f; }

                        #endregion

                        //float lcEndTime = Time.realtimeSinceStartup;

                        //float lidStartTime = Time.realtimeSinceStartup;

                        #region Lift and Induced Drag

                        // Lift force magnitude = 0.5 * fluid density * velocity squared * planform wing area * coefficient of lift
                        liftForceMagnitude = 0.5f * mediumDensity * aerodynamicTrueAirspeed * aerodynamicTrueAirspeed * (wing.span * wing.chord * (float)Math.Cos(wingAngleOfAttack * Mathf.Deg2Rad)) * liftCoefficient;

                        // Adjust for wing damage
                        if (!shipDamageModelIsSimple && wing.damageRegionIndex > -1)
                        {
                            liftForceMagnitude *= wing.CurrentPerformance;
                        }

                        localLiftForce = liftForceMagnitude * wing.liftDirectionNormalised;
                        // Induced drag force magnitude = 2 * lift squared / (fluid density * velocity squared * pi * wingspan squared)
                        localInducedDragForce = 2f * liftForceMagnitude * liftForceMagnitude /
                            (mediumDensity * aerodynamicTrueAirspeed * aerodynamicTrueAirspeed * Mathf.PI * wing.span * wing.span) * Vector3.back;

                        #endregion

                        //float lidEndTime = Time.realtimeSinceStartup;

                        //float forceStartTime = Time.realtimeSinceStartup;

                        // Add calculated lift and induced drag forces to local resultant force and moment
                        localResultantForce += localLiftForce + localInducedDragForce;
                        if (shipPhysicsModelPhysicsBased)
                        {
                            localResultantMoment += Vector3.Cross(wing.relativePosition - centreOfMass, localLiftForce + localInducedDragForce);
                        }

                        //float forceEndTime = Time.realtimeSinceStartup;

                        //if (componentIndex == 0)
                        //{
                        //    Debug.Log("| Angle of attack: " + ((aoaEndTime - aoaStartTime) * 1000f).ToString("0.0000") + " ms | " +
                        //        "Lift coefficient: " + ((lcEndTime - lcStartTime) * 1000f).ToString("0.0000") + " ms | " +
                        //        "Lift and induced drag: " + ((lidEndTime - lidStartTime) * 1000f).ToString("0.0000") + " ms | " +
                        //        "Force and moment: " + ((forceEndTime - forceStartTime) * 1000f).ToString("0.0000") + " ms | ");
                        //}
                    }
                }

                #endregion

                //float wingsEndTime = Time.realtimeSinceStartup;

                //float controlStartTime = Time.realtimeSinceStartup;

                // Control surfaces only available in physics-based mode
                if (shipPhysicsModelPhysicsBased)
                {
                    #region Control Surfaces

                    // Control surface simulation

                    componentListSize = controlSurfaceList == null ? 0 : controlSurfaceList.Count;
                    for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                    {
                        // Get the control surface we are concerned with
                        controlSurface = controlSurfaceList[componentIndex];

                        #region Control Surface Type

                        switch (controlSurface.type)
                        {
                            case ControlSurface.ControlSurfaceType.Aileron:
                                // Aileron
                                controlSurfaceMovementAxis = Vector3.up;
                                controlSurfaceInput = momentInput.z * (controlSurface.relativePosition.x > 0f ? 1f : -1f);
                                break;
                            case ControlSurface.ControlSurfaceType.Elevator:
                                // Elevator
                                controlSurfaceMovementAxis = Vector3.up;
                                controlSurfaceInput = -momentInput.x;
                                break;
                            case ControlSurface.ControlSurfaceType.Rudder:
                                // Rudder
                                controlSurfaceMovementAxis = Vector3.right;
                                controlSurfaceInput = momentInput.y;
                                break;
                            case ControlSurface.ControlSurfaceType.AirBrake:
                                // Air brake
                                controlSurfaceMovementAxis = Vector3.zero;
                                controlSurfaceInput = forceInput.z < 0f ? -forceInput.z : 0f;
                                break;
#if UNITY_EDITOR
                            default:
                                // Custom (TODO)
                                Debug.Log("Ship - Custom control surfaces not supported yet.");
                                break;
#endif
                        }

                        #endregion

                        // When there is no input the control surface does not apply any force
                        if (controlSurfaceInput != 0f)
                        {
                            #region Calculate Lift and Drag Forces

                            // Calculate the angle the control surface is inclined at
                            // Absolute value of controlSurfaceInput is used as we only want magnitude - direction is added in a later step
                            controlSurfaceAngle = Mathf.PI / 2f * (controlSurfaceInput > 0f ? controlSurfaceInput : -controlSurfaceInput);
                            // Calculate the magnitude of the change (delta) in lift and drag caused by the inclination of the control surface
                            // This is essentially the change in planform area (for lift) and the change in profile area (for drag)
                            // multiplied by the usual aerodynamic formulae for each
                            // TODO: Should probably work out something mildly more accurate for lift coefficient (currently is just 1.6)
                            // TODO: Should probably have something for a drag coefficient as well (currently is just 2)
                            // Then convert the forces into vectors by multiplying by their directions
                            // TODO: Include relative position based on inclination
                            // - I think this means the position of the centre of the CS adjusted for where it has been rotated to?
                            if (controlSurfaceMovementAxis != Vector3.zero)
                            {
                                controlSurfaceLiftDelta = (1f - (float)Math.Cos(controlSurfaceAngle)) * 0.5f * mediumDensity *
                                    aerodynamicTrueAirspeed * aerodynamicTrueAirspeed * controlSurface.chord * controlSurface.span * 1.6f;
                            }
                            else { controlSurfaceLiftDelta = 0f; }

                            controlSurfaceDragDelta = (float)Math.Sin(controlSurfaceAngle) * 0.5f * mediumDensity *
                                aerodynamicTrueAirspeed * aerodynamicTrueAirspeed * controlSurface.chord * controlSurface.span * 2f;

                            // Adjust for control surface damage
                            if (!shipDamageModelIsSimple && controlSurface.damageRegionIndex > -1)
                            {
                                controlSurfaceLiftDelta *= controlSurface.CurrentPerformance;
                                controlSurfaceDragDelta *= controlSurface.CurrentPerformance;
                            }

                            localLiftForce = controlSurfaceLiftDelta * (controlSurfaceInput > 0f ? -1f : 1f) * controlSurfaceMovementAxis;
                            localDragForce = controlSurfaceDragDelta * (aerodynamicTrueAirspeed > 0f ? 1f : -1f) * Vector3.back;

                            #endregion

                            // Adjust relative position to take into account current inclination of control surface
                            controlSurfaceRelativePosition = controlSurface.relativePosition + (controlSurfaceMovementAxis *
                                controlSurface.chord * 0.5f * (float)Math.Sin(controlSurfaceAngle));

                            // Add calculated lift and drag forces to resultant force and moment
                            localResultantForce += localLiftForce + localDragForce;
                            localResultantMoment += Vector3.Cross(controlSurfaceRelativePosition - centreOfMass, localLiftForce + localDragForce);
                        }
                    }

                    #endregion
                }

                //float controlEndTime = Time.realtimeSinceStartup;

                //Debug.Log("| Profile Drag: " + ((pDragEndTime - pDragStartTime) * 1000f).ToString("0.0000") + " ms | " +
                //    "Angular Drag: " + ((aDragEndTime - aDragStartTime) * 1000f).ToString("0.0000") + " ms | " +
                //    "Wings: " + ((wingsEndTime - wingsStartTime) * 1000f).ToString("0.0000") + " ms | " +
                //    "Control Surfaces: " + ((controlEndTime - controlStartTime) * 1000f).ToString("0.0000") + " ms | ");
            }

            #endregion

#if SSC_SHIP_DEBUG
            float aeroEndTime = Time.realtimeSinceStartup;

            float arcadeStartTime = Time.realtimeSinceStartup;
#endif

            #region Arcade Physics Adjustments

            if (!shipPhysicsModelPhysicsBased)
            {
                // Apply moments to match pitch, yaw and roll inputs, as well as for various arcade adjustments
                // These include rotational flight assist and target pitch/roll moment
                // Multiplication by inertia tensor is used so that we can use acceleration parameters instead of force parameters
                localResultantMoment.x += ((momentInput.x * arcadePitchAcceleration) + arcadeMomentVector.x) * Mathf.Deg2Rad * rBodyInertiaTensor.x;
                localResultantMoment.y += ((momentInput.y * arcadeYawAcceleration) + arcadeMomentVector.y) * Mathf.Deg2Rad * rBodyInertiaTensor.y;
                localResultantMoment.z += ((momentInput.z * arcadeRollAcceleration) + arcadeMomentVector.z) * Mathf.Deg2Rad * rBodyInertiaTensor.z * -1f;

                // TODO: Currently this doesn't take into account the force already being applied by the ship,
                // calculated prior to this point
                // Should it do that?

                if (stickToGroundSurfaceEnabled)
                {
                    // Calculate the limits for input for ground matching
                    float maxGroundDistInput = maxGroundMatchAcceleration;
                    if (useGroundMatchSmoothing)
                    {
                        // Previous values (for last two arguments): 50f, 2f
                        maxGroundDistInput = DampedMaxAccelerationInput(currentPerpendicularGroundDist, targetGroundDistance,
                            minGroundDistance, maxGroundDistance, centralMaxGroundMatchAcceleration, maxGroundMatchAcceleration, 200f, 0.5f);
                    }

                    // Set input limits
                    groundDistPIDController.SetInputLimits(-maxGroundDistInput, maxGroundDistInput);

                    // Get input from ground distance PID controller
                    groundDistInput = groundDistPIDController.RequiredInput(targetGroundDistance, currentPerpendicularGroundDist, _deltaTime);

                    // Are we preventing an arcade model ship from going below the target (minimum) distance?
                    if (avoidGroundSurface)
                    {
                        // Only override user input if the ship is going below the targetGroundDistance
                        stickToGroundSurfaceEnabled = groundDistInput > 0f;
                    }
                }

                if (stickToGroundSurfaceEnabled)
                {
                    // Apply turning force counteracting velocity on x axis if pilot releases the input 
                    // or if input is in direction opposing velocity
                    if ((forceInput.x > 0f) == (localVelocity.x < 0f) || forceInput.x == 0f)
                    {
                        // The force also includes a component to counteract gravity on this axis
                        arcadeForceVector.x = -((localVelocity.x * 0.5f / _deltaTime) + localGravityAcceleration.x);
                    }
                    else { arcadeForceVector.x = 0f; }
                    // When we are sticking to the ground surface don't apply turning force on y axis,
                    // as this force should be primarily handled by the force keeping the ship at the
                    // target distance from the ground
                    arcadeForceVector.y = 0f;
                    // TODO: Maybe want to have an option to allow force to act on z as well (resulting in very arcade-like handling)
                    arcadeForceVector.z = 0f;

                    // Clamp the magnitude of the turning force vector so that the force applied does not exceed
                    // the maximum on-ground turning acceleration. Multiply by mass to convert the acceleration into a force
                    arcadeForceVector = Vector3.ClampMagnitude(arcadeForceVector, arcadeMaxGroundTurningAcceleration) * mass;

                    // Add a force acting in the direction of the ground normal to keep the ship at a target distance
                    // from the ground (the magnitude of the force is calculated by the associated PID controller)
                    arcadeForceVector += trfmInvRot * worldTargetPlaneNormal * groundDistInput * mass;
                }
                else
                {
                    // Apply turning force counteracting velocity on x and y axes if pilot releases the input 
                    // or if input is in direction opposing velocity
                    if ((forceInput.x > 0f) == (localVelocity.x < 0f) || forceInput.x == 0f)
                    {
                        // The force also includes a component to counteract gravity on this axis
                        arcadeForceVector.x += -((localVelocity.x * 0.5f / _deltaTime) + localGravityAcceleration.x);
                    }
                    if ((forceInput.y > 0f) == (localVelocity.y < 0f) || forceInput.y == 0f)
                    {
                        // The force also includes a component to counteract gravity on this axis
                        arcadeForceVector.y += -((localVelocity.y * 0.5f / _deltaTime) + localGravityAcceleration.y);
                    }
                    // Maybe want to have an option to allow force to act on z as well (resulting in very arcade-like handling)

                    // Clamp the magnitude of the turning force vector so that the force applied does not exceed
                    // the maximum in-flight turning acceleration. Multiply by mass to convert the acceleration into a force (f = ma)
                    arcadeForceVector = Vector3.ClampMagnitude(arcadeForceVector, arcadeMaxFlightTurningAcceleration) * mass;
                }

                // Braking component for arcade mode - active when moving forward and braking
                if (arcadeUseBrakeComponent && localVelocity.z > 0f && forceInput.z < 0f)
                {
                    // Calculate magnitude of braking force using drag formula
                    arcadeBrakeForceMagnitude = 0.5f * (arcadeBrakeIgnoreMediumDensity ? 1f : mediumDensity) * localVelocity.z * localVelocity.z * arcadeBrakeStrength * -forceInput.z;
                    // Calculate min braking force (f = ma)
                    arcadeBrakeForceMinMagnitude = arcadeBrakeMinAcceleration * mass;
                    // Make sure min braking force doesn't overcompensate and cause the ship to start moving backwards
                    if (arcadeBrakeForceMinMagnitude > localVelocity.z / _deltaTime * mass)
                    {
                        arcadeBrakeForceMinMagnitude = localVelocity.z / _deltaTime * mass;
                    }
                    // Add braking force or min braking force - whichever is larger
                    arcadeForceVector.z -= arcadeBrakeForceMagnitude > arcadeBrakeForceMinMagnitude ? arcadeBrakeForceMagnitude : arcadeBrakeForceMinMagnitude;
                }

                // Add the calculated arcade force to the local resultant force
                localResultantForce += arcadeForceVector;
            }

            #endregion

#if SSC_SHIP_DEBUG
            float arcadeEndTime = Time.realtimeSinceStartup;
#endif

            // Remember time of this fixed update
            lastFixedUpdateTime = Time.time;

#if SSC_SHIP_DEBUG
            //Debug.Log("| Init: " + ((initEndTime - initStartTime) * 1000f).ToString("0.0000") + " ms | " +
            //    "Ground: " + ((groundEndTime - groundStartTime) * 1000f).ToString("0.0000") + " ms | " +
            //    "Input: " + ((inputEndTime - inputStartTime) * 1000f).ToString("0.0000") + " ms | " +
            //    "Thrusters: " + ((thrusterEndTime - thrusterStartTime) * 1000f).ToString("0.0000") + " ms | " +
            //    "Aero: " + ((aeroEndTime - aeroStartTime) * 1000f).ToString("0.0000") + " ms | " +
            //    "Arcade: " + ((arcadeEndTime - arcadeStartTime) * 1000f).ToString("0.0000") + " ms | " +
            //    "Total: " + ((arcadeEndTime - initStartTime) * 1000f).ToString("0.0000") + " ms | ");
#endif
        }

        #endregion

        #region Update Weapons And Damage

        /// <summary>
        /// Update weapons and damage information for this ship. Should be called during Update(), and have 
        /// UpdatePositionAndMovementData() called prior to it.
        /// NOTE: We often hardcode the weaponType int rather than looking up the enum for the sake of performance.
        /// </summary>
        /// <param name="shipControllerManager"></param>
        public void UpdateWeaponsAndDamage(SSCManager shipControllerManager)
        {
            #region Initialisation

            float _deltaTime = Time.deltaTime;

            // Get the ship damage model
            shipDamageModelIsSimple = shipDamageModel == ShipDamageModel.Simple;

            // Update the local reference, which is used also in MoveBeam()
            if (sscManager == null) { sscManager = shipControllerManager; }

            #endregion

            #region Weapons

            componentListSize = weaponList == null ? 0 : weaponList.Count;
            for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
            {
                // Get the weapon we are concerned with
                weapon = weaponList[componentIndex];
                if (weapon == null) { continue; }

                // Check that the weapon has had a projectile ID or beam ID assigned
                // Assumes that the IDs have been correctly assigned based on the weaponType.
                if (weapon.projectilePrefabID >= 0 || weapon.beamPrefabID >= 0)
                {
                    // Lookup the enumeration once
                    weaponTypeInt = weapon.weaponTypeInt;
                    weaponFiringButtonInt = (int)weapon.firingButton;

                    // Does the weapon need to cool down??
                    weapon.ManageHeat(_deltaTime, 0f);

                    // If the weapon is not connected to a firing mechanism we can probably ignore the reload timer
                    // Weapons with no health or performance cannot aim, reload, move turrets, or fire.
                    if (weaponFiringButtonInt > 0 && weapon.health > 0f && weapon.currentPerformance > 0f)
                    {
                        if (weaponTypeInt == Weapon.TurretProjectileInt || weaponTypeInt == Weapon.TurretBeamInt) { weapon.MoveTurret(worldVelocity, false); }

                        // Does the beam weapon need charging?
                        if ((weaponTypeInt == Weapon.FixedBeamInt || weaponTypeInt == Weapon.TurretBeamInt) && weapon.rechargeTime > 0f && weapon.chargeAmount < 1f)
                        {
                            weapon.chargeAmount += _deltaTime * 1f / weapon.rechargeTime;
                            if (weapon.chargeAmount > 1f) { weapon.chargeAmount = 1f; }
                        }

                        // Check if this projectile weapon is reloading or not
                        // Check if this beam weapon is powering-up or not
                        if (weapon.reloadTimer > 0f)
                        {
                            // If reloading (or powering-up), update reload timer
                            weapon.reloadTimer -= _deltaTime;
                        }
                        // Check if this weapon is damaged or not
                        else if (shipDamageModelIsSimple || weapon.damageRegionIndex < 0 || weapon.Health > 0f)
                        {
                            // If not reloading and not damaged, check if we should fire the weapon or not
                            // For autofiring weapons, this will return false.
                            bool isReadyToFire = (pilotPrimaryFireInput && weaponFiringButtonInt == weaponPrimaryFiringInt) || (pilotSecondaryFireInput && weaponFiringButtonInt == weaponSecondaryFiringInt);

                            // If this is auto-firing turret check if it is ready to fire
                            if (weaponFiringButtonInt == weaponAutoFiringInt && (weaponTypeInt == Weapon.TurretProjectileInt || weaponTypeInt == Weapon.TurretBeamInt))
                            {
                                // Is turret locked on target and in direct line of sight?
                                // NOTE: If check LoS is enabled, it will still fire if another enemy is between the turret and the weapon.target.
                                isReadyToFire = weapon.isLockedOnTarget && (!weapon.checkLineOfSight || WeaponHasLineOfSight(weapon));

                                // TODO - Is target in range?

                            }

                            // If not reloading and not damaged, check if we should fire the weapon or not
                            if (isReadyToFire && weapon.ammunition != 0)
                            {
                                // ==============================================================================
                                // NOTE: We need to do most of these steps also in MoveBeam(..). So the commented
                                // out methods below called in MoveBeam(..)
                                // ==============================================================================

                                // Get base weapon world position (not accounting for relative fire position)
                                //weaponWorldBasePosition = GetWeaponWorldBasePosition(weapon);
                                weaponWorldBasePosition = (trfmRight * weapon.relativePosition.x) +
                                        (trfmUp * weapon.relativePosition.y) +
                                        (trfmFwd * weapon.relativePosition.z) +
                                        trfmPos;

                                // Get relative fire direction
                                weaponRelativeFireDirection = weapon.fireDirectionNormalised;
                                // If this is a turret, adjust relative fire direction based on turret rotation
                                if (weaponTypeInt == Weapon.TurretProjectileInt || weaponTypeInt == Weapon.TurretBeamInt)
                                {
                                    // (turret rotation less ship rotate) - (original turret rotation less ship rotation) + relative fire direction
                                    weaponRelativeFireDirection = (trfmInvRot * weapon.turretPivotX.rotation) * weapon.intPivotYInvRot * weaponRelativeFireDirection;
                                }

                                // Get weapon world fire direction
                                weaponWorldFireDirection = (trfmRight * weaponRelativeFireDirection.x) +
                                            (trfmUp * weaponRelativeFireDirection.y) +
                                            (trfmFwd * weaponRelativeFireDirection.z);

                                //weaponWorldFireDirection = GetWeaponWorldFireDirection(weapon);

                                // Start the firing cycle with no heat generated
                                float heatValue = 0f;
                                bool hasFired = false;

                                // Loop through all fire positions
                                int firePositionListCount = weapon.isMultipleFirePositions ? weapon.firePositionList.Count : 1;
                                for (int fp = 0; fp < firePositionListCount; fp++)
                                {
                                    // Only fire if there is unlimited ammunition (-1) or greater than 0 projectiles available
                                    if (weapon.ammunition != 0)
                                    {
                                        // If not unlimited ammo, decrement the quantity available
                                        if (weapon.ammunition > 0) { weapon.ammunition--; }

                                        // Check if there are multiple fire positions
                                        if (weapon.isMultipleFirePositions)
                                        {
                                            // Get relative fire position
                                            weaponRelativeFirePosition = weapon.firePositionList[fp];
                                        }
                                        else
                                        {
                                            // If there is only one fire position, relative position must be the zero vector
                                            weaponRelativeFirePosition.x = 0f;
                                            weaponRelativeFirePosition.y = 0f;
                                            weaponRelativeFirePosition.z = 0f;
                                        }
                                        // If this is a turret, adjust relative fire position based on turret rotation
                                        if (weaponTypeInt == Weapon.TurretProjectileInt || weaponTypeInt == Weapon.TurretBeamInt)
                                        {
                                            weaponRelativeFirePosition = (trfmInvRot * weapon.turretPivotX.rotation) * weaponRelativeFirePosition;
                                        }

                                        // Get weapon world fire position
                                        weaponWorldFirePosition = weaponWorldBasePosition +
                                            (trfmRight * weaponRelativeFirePosition.x) +
                                            (trfmUp * weaponRelativeFirePosition.y) +
                                            (trfmFwd * weaponRelativeFirePosition.z);

                                        //weaponWorldFirePosition = GetWeaponWorldFirePosition(weapon, weaponWorldBasePosition, weaponRelativeFirePosition);

                                        if (weaponTypeInt == Weapon.FixedBeamInt || weaponTypeInt == Weapon.TurretBeamInt)
                                        {
                                            // if the sequence number is 0, it means it is not active
                                            if (weapon.beamItemKeyList[fp].beamSequenceNumber == 0)
                                            {
                                                InstantiateBeamParameters ibParms = new InstantiateBeamParameters()
                                                {
                                                    beamPrefabID = weapon.beamPrefabID,
                                                    position = weaponWorldFirePosition,
                                                    fwdDirection = weaponWorldFireDirection,
                                                    upDirection = trfmUp,
                                                    shipId = shipId,
                                                    squadronId = squadronId,
                                                    weaponIndex = componentIndex,
                                                    firePositionIndex = fp,
                                                    beamSequenceNumber = 0,
                                                    beamPoolListIndex = -1
                                                };

                                                // Create a beam using the ship control Manager (SSCManager)
                                                // Pass InstantiateBeamParameters by reference so we can get the beam index and sequence number back
                                                BeamModule beamModule = shipControllerManager.InstantiateBeam(ref ibParms);
                                                if (beamModule != null)
                                                {
                                                    beamModule.callbackOnMove = MoveBeam;
                                                    // Retrieve the unique identifiers for the beam instance
                                                    weapon.beamItemKeyList[fp] = new SSCBeamItemKey(weapon.beamPrefabID, ibParms.beamPoolListIndex, ibParms.beamSequenceNumber);
                                                    // Immediately update the beam position (required for pooled beams that have previously been used)
                                                    MoveBeam(beamModule);

                                                    hasFired = true;

                                                    // Was a poolable muzzle fx spawned?
                                                    if (beamModule.muzzleEffectsItemKey.effectsObjectSequenceNumber > 0)
                                                    {
                                                        // Only get the transform if the muzzle EffectsModule has Is Reparented enabled.
                                                        Transform muzzleFXTrfm = shipControllerManager.GetEffectsObjectTransform(beamModule.muzzleEffectsItemKey.effectsObjectTemplateListIndex, beamModule.muzzleEffectsItemKey.effectsObjectPoolListIndex, true);

                                                        if (muzzleFXTrfm != null)
                                                        {
                                                            muzzleFXTrfm.SetParent(beamModule.transform);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Create a new projectile using the ship controller Manager (SSCManager)
                                            // Velocity is world velocity of ship plus relative velocity of weapon due to angular velocity
                                            InstantiateProjectileParameters ipParms = new InstantiateProjectileParameters()
                                            {
                                                projectilePrefabID = weapon.projectilePrefabID,
                                                position = weaponWorldFirePosition,
                                                fwdDirection = weaponWorldFireDirection,
                                                upDirection = trfmUp,
                                                weaponVelocity = worldVelocity + Vector3.Cross(worldAngularVelocity, weaponWorldFirePosition - trfmPos),
                                                gravity = gravitationalAcceleration,
                                                gravityDirection = gravityDirection,
                                                shipId = shipId,
                                                squadronId = squadronId,
                                                targetShip = weapon.isProjectileKGuideToTarget ? weapon.targetShip : null,
                                                targetGameObject = weapon.isProjectileKGuideToTarget ? weapon.target : null,
                                                targetguidHash = weapon.isProjectileKGuideToTarget ? weapon.targetguidHash : 0,
                                            };

                                            shipControllerManager.InstantiateProjectile(ref ipParms);

                                            // For now, assume projectile was instantiated
                                            // Heat value is inversely proportional to the firing interval (reload time)
                                            if (weapon.reloadTime > 0f) { heatValue += 1f / weapon.reloadTime; };

                                            hasFired = true;

                                            // Was a poolable muzzle fx spawned?
                                            if (ipParms.muzzleEffectsObjectPoolListIndex >= 0)
                                            {
                                                // Only get the transform if the muzzle EffectsModule has Is Reparented enabled.
                                                Transform muzzleFXTrfm = shipControllerManager.GetEffectsObjectTransform(ipParms.muzzleEffectsObjectPrefabID, ipParms.muzzleEffectsObjectPoolListIndex, true);

                                                if (muzzleFXTrfm != null)
                                                {
                                                    if (weaponTypeInt == Weapon.TurretProjectileInt)
                                                    {
                                                        muzzleFXTrfm.SetParent(weapon.turretPivotX);
                                                    }
                                                    else
                                                    {
                                                        // This may be risky as it could create a dependency that
                                                        // causes GC and/or circular reference issues when the ShipControlModule is destroyed.
                                                        // Might need a better method of doing this in the future.
                                                        muzzleFXTrfm.SetParent(shipTransform);
                                                    }
                                                }
                                            }
                                        }

                                        // Set reload timer to reload time
                                        weapon.reloadTimer = weapon.reloadTime;

                                        // Did we just run out of ammo?
                                        if (weapon.ammunition == 0 && callbackOnWeaponNoAmmo != null)
                                        {
                                            callbackOnWeaponNoAmmo.Invoke(this, weapon);
                                        }
                                    }
                                }

                                if (heatValue > 0f) { weapon.ManageHeat(_deltaTime, heatValue); }

                                // If the ship has a callback configured for the weapon, and it
                                // has fired at least one fire point, invoke the callback.
                                if (hasFired && callbackOnWeaponFired != null)
                                {
                                    callbackOnWeaponFired.Invoke(this, weapon);
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Damage

            if (!Destroyed())
            {
                if (respawningMode == RespawningMode.RespawnAtLastPosition)
                {
                    // Update current respawn position and rotation
                    currentRespawnPosition = trfmPos;
                    currentRespawnRotation = trfmRot;

                    // Increment collision respawn position timer
                    collisionRespawnPositionTimer += _deltaTime;
                    // When timer reaches the interval amount...
                    if (collisionRespawnPositionTimer > collisionRespawnPositionDelay)
                    {
                        // Update current respawn position/rotation (from what was next)
                        currentCollisionRespawnPosition = nextCollisionRespawnPosition;
                        currentCollisionRespawnRotation = nextCollisionRespawnRotation;
                        // Set next respawn position/rotation from current position/rotation
                        nextCollisionRespawnPosition = trfmPos;
                        nextCollisionRespawnRotation = trfmRot;
                        // Reset the timer
                        collisionRespawnPositionTimer = 0f;
                    }
                }
            }

            #endregion
        }

        #endregion

        #endregion

        #region Public API Methods

        #region Damage API Methods

        /// <summary>
        /// When attaching an object to the ship, call this method for each non-trigger collider.
        /// It is automatically called when using VR hands to avoid them colliding with the ship
        /// and causing damage.
        /// </summary>
        /// <param name="colliderToAttach"></param>
        public void AttachCollider (Collider colliderToAttach)
        {
            if (colliderToAttach != null && !colliderToAttach.isTrigger)
            {
                attachedCollisionColliders.Add(colliderToAttach.GetInstanceID());
            }
        }

        /// <summary>
        /// When attaching an object to the ship, call this method with an array of non-trigger collider.
        /// It is automatically called when using VR hands to avoid them colliding with the ship
        /// and causing damage. See also AttachCollider(..) and DetachCollider(..).
        /// </summary>
        /// <param name="collidersToAttach"></param>
        public void AttachColliders (Collider[] collidersToAttach)
        {
            int numAttachedColliders = collidersToAttach == null ? 0 : collidersToAttach.Length;

            for (int colIdx = 0; colIdx < numAttachedColliders; colIdx++)
            {
                Collider attachCollider = collidersToAttach[colIdx];
                if (attachCollider.enabled && !attachCollider.isTrigger) { AttachCollider(attachCollider); }
            }
        }

        /// <summary>
        /// When detaching or removing an object from the ship, call this method for each non-trigger collider.
        /// This is only required if it was first registered with AttachCollider(..).
        /// USAGE: DetachCollider (collider.GetInstanceID())
        /// </summary>
        /// <param name="colliderID"></param>
        public void DetachCollider (int colliderID)
        {
            attachedCollisionColliders.Remove(colliderID);
        }

        /// <summary>
        /// When detaching or removing an object from the ship, call this method with an array
        /// of non-trigger colliders. This is only required if they were first attached.
        /// </summary>
        /// <param name="collidersToDetach"></param>
        public void DetachColliders (Collider[] collidersToDetach)
        {
            int numAttachedColliders = collidersToDetach == null ? 0 : collidersToDetach.Length;

            for (int colIdx = 0; colIdx < numAttachedColliders; colIdx++)
            {
                Collider attachCollider = collidersToDetach[colIdx];
                if (attachCollider != null && attachCollider.enabled) { DetachCollider(attachCollider.GetInstanceID()); }
            }
        }

        /// <summary>
        /// Get the localised damage region with guidHash in the list of regions.
        /// The ship must be initialised.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public DamageRegion GetDamageRegion(int guidHash)
        {
            DamageRegion damageRegion = null;

            if (guidHash != 0 && shipDamageModel == ShipDamageModel.Localised)
            {
                // Attempt to find a matching guidHash
                for (int dmIdx = 0; dmIdx < numLocalisedDamageRegions; dmIdx++)
                {
                    if (localisedDamageRegionList[dmIdx].guidHash == guidHash)
                    {
                        damageRegion = localisedDamageRegionList[dmIdx]; break;
                    }
                }
            }

            return damageRegion;
        }

        /// <summary>
        /// Get the localised damage region with the zero-based index in the list of regions.
        /// The ship must be initialised.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DamageRegion GetDamageRegionByIndex(int index)
        {
            if (shipDamageModel == ShipDamageModel.Localised && index >= 0 && index < numLocalisedDamageRegions)
            {
                return localisedDamageRegionList[index];
            }
            else { return null; }
        }

        /// <summary>
        /// Get the zero-based index of a local damage region in the ship given the damage region name.
        /// Returns -1 if not found or the Damage Model is not Localised.
        /// Use this sparingly as it will incur garabage. Always declare the parameter as a static readonly variable.
        /// Usage:
        /// private static readonly string EngineDamageRegionName = "Engines";
        /// ..
        /// int drIdx = GetDamageRegionIndexByName(EngineDamageRegionName);
        /// DamageRegion damageRegion = GetDamageRegionByIndex(drIdx);
        /// </summary>
        /// <param name="damageRegionName"></param>
        /// <returns></returns>
        public int GetDamageRegionIndexByName(string damageRegionName)
        {
            if (shipDamageModel == ShipDamageModel.Localised && localisedDamageRegionList != null)
            {
                return localisedDamageRegionList.FindIndex(dr => dr.name == damageRegionName);
            }
            else { return -1; }
        }

        /// <summary>
        /// Get the world space central position of the localised damage region
        /// with the given guidHash. If the ship is not using the Localised damage model
        /// or a damage region with guidHash cannot be found, this returns the world space
        /// position of the ship.
        /// The ship must be initialised.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public Vector3 GetDamageRegionWSPosition(int guidHash)
        {
            DamageRegion damageRegion = GetDamageRegion(guidHash);
            if (damageRegion != null)
            {
                return (trfmRight * damageRegion.relativePosition.x) +
                       (trfmUp * damageRegion.relativePosition.y) +
                       (trfmFwd * damageRegion.relativePosition.z) +
                       trfmPos;
            }
            else { return TransformPosition; }
        }

        /// <summary>
        /// Get the world space central position of the damage region.
        /// The ship must be initialised.
        /// </summary>
        /// <param name="damageRegion"></param>
        /// <returns></returns>
        public Vector3 GetDamageRegionWSPosition(DamageRegion damageRegion)
        {
            return (trfmRight * damageRegion.relativePosition.x) +
                    (trfmUp * damageRegion.relativePosition.y) +
                    (trfmFwd * damageRegion.relativePosition.z) +
                    trfmPos;
        }

        /// <summary>
        /// Is this world space point on the ship, currently shielded?
        /// If the main damage region is invincible, it will always return true.
        /// If the ShipDamageModel is NOT Localised, the worldSpacePoint is ignored.
        /// </summary>
        /// <param name="worldSpacePoint"></param>
        /// <returns></returns>
        public bool HasActiveShield (Vector3 worldSpacePoint)
        {
            bool isShielded = false;

            if (mainDamageRegion.isInvincible) { isShielded = true; }
            else if (shipDamageModel != ShipDamageModel.Localised)
            {
                isShielded = mainDamageRegion.useShielding && mainDamageRegion.ShieldHealth > 0f;
            }
            else
            {
                // Determine which damage region was hit
                int numDamageRegions = localisedDamageRegionList.Count + 1;

                // Loop through all damage regions
                DamageRegion thisDamageRegion;

                // Get damage position in local space (calc once outside the loop)
                Vector3 localDamagePosition = trfmInvRot * (worldSpacePoint - trfmPos);

                for (int i = 0; i < numDamageRegions; i++)
                {
                    thisDamageRegion = i == 0 ? mainDamageRegion : localisedDamageRegionList[i - 1];

                    if (thisDamageRegion.useShielding && thisDamageRegion.ShieldHealth > 0f && thisDamageRegion.IsHit(localDamagePosition))
                    {
                        isShielded = true;
                        break;
                    }
                }
            }

            return isShielded;
        }

        /// <summary>
        /// Check if a world-space point is within the volume area of a damage region.
        /// </summary>
        /// <param name="damageRegion"></param>
        /// <param name="worldSpacePoint"></param>
        /// <returns></returns>
        public bool IsPointInDamageRegion (DamageRegion damageRegion, Vector3 worldSpacePoint)
        {
            return damageRegion == null ? false : damageRegion.IsHit(trfmInvRot * (worldSpacePoint - trfmPos));
        }

        /// <summary>
        /// Make the whole ship invincible to damage.
        /// For individual damageRegions change the isInvisible value on the localised region.
        /// </summary>
        public void MakeShipInvincible()
        {
            mainDamageRegion.isInvincible = true;
        }

        /// <summary>
        /// Make the whole ship vincible to damage. When hit, the ship or shields will take damage.
        /// For individual damageRegions change the isInvisible value on the localised region.
        /// </summary>
        public void MakeShipVincible()
        {
            mainDamageRegion.isInvincible = false;
        }

        /// <summary>
        /// Resets health data for the ship. Used when initialising and respawning the ship.
        /// </summary>
        /// <returns></returns>
        public void ResetHealth()
        {
            // Reset health value for the damage region
            mainDamageRegion.Health = mainDamageRegion.startingHealth;

            // Reset shield health value for the damage region
            mainDamageRegion.ShieldHealth = mainDamageRegion.shieldingAmount;

            mainDamageRegion.shieldRechargeDelayTimer = 0f;

            mainDamageRegion.isDestructObjectActivated = false;
            mainDamageRegion.isDestroyed = false;

            damageCameraShakeAmountRequired = 0f;

            if (!shipDamageModelIsSimple)
            {
                // Reset health value for each component

                componentListSize = thrusterList.Count;
                for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                {
                    thrusterList[componentIndex].Repair();
                }

                componentListSize = wingList.Count;
                for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                {
                    wingList[componentIndex].Health = wingList[componentIndex].startingHealth;
                }

                componentListSize = controlSurfaceList.Count;
                for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                {
                    controlSurfaceList[componentIndex].Health = controlSurfaceList[componentIndex].startingHealth;
                }

                componentListSize = weaponList.Count;
                for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                {
                    weaponList[componentIndex].Repair();
                }

                if (shipDamageModel == ShipDamageModel.Localised)
                {
                    // For each damage region:
                    // reset health value, shield health value, effects object or destruct instantiated flags, and child tranform

                    for (componentIndex = 0; componentIndex < numLocalisedDamageRegions; componentIndex++)
                    {
                        DamageRegion _damageRegion = localisedDamageRegionList[componentIndex];

                        _damageRegion.Health = localisedDamageRegionList[componentIndex].startingHealth;
                        _damageRegion.ShieldHealth = localisedDamageRegionList[componentIndex].shieldingAmount;
                        _damageRegion.shieldRechargeDelayTimer = 0f;
                        _damageRegion.isDestructionEffectsObjectInstantiated = false;
                        _damageRegion.isDestructObjectActivated = false;
                        _damageRegion.isDestroyed = false;
                        // if there is one, reactivate the transform for this region.
                        if (_damageRegion.regionChildTransform != null && !_damageRegion.regionChildTransform.gameObject.activeSelf)
                        {
                            _damageRegion.regionChildTransform.gameObject.SetActive(true);
                        }
                    }
                }
            }

            if (callbackOnDamage != null) { callbackOnDamage(mainDamageRegion.Health); }
        }

        /// <summary>
        /// Typically used when ShipDamageModel is Simple or Progressive, add health to the ship.
        /// If isAffectShield is true, and the health reaches the maximum configured, excess health will
        /// be applied to the shield for the specified DamageRegion. For Progressive Damage, health is
        /// also added to components that have Use Progressive Damage enabled.
        /// </summary>
        /// <param name="healthAmount"></param>
        /// <param name="isAffectShield"></param>
        public void AddHealth(float healthAmount, bool isAffectShield)
        {
            AddHealth(mainDamageRegion, healthAmount, isAffectShield);
        }

        /// <summary>
        /// Add health to a specific DamageRegion.
        /// If isAffectShield is true, and the health reaches the maximum configured, excess health will
        /// be applied to the shield for the specified DamageRegion. For Progressive Damage, health is
        /// also added to components that have Use Progressive Damage enabled.
        /// NOTE: -ve values are ignored. To incur damage use the ApplyNormalDamage or ApplyCollisionDamage
        /// API methods.
        /// </summary>
        /// <param name="damageRegion"></param>
        /// <param name="healthAmount"></param>
        /// <param name="isAffectShield"></param>
        public void AddHealth(DamageRegion damageRegion, float healthAmount, bool isAffectShield)
        {
            if (damageRegion != null && healthAmount > 0f)
            {
                float health = damageRegion.Health;
                if (health < 0f) { health = healthAmount; }
                else { health += healthAmount; }

                if (health > damageRegion.startingHealth)
                {
                    if (isAffectShield && damageRegion.useShielding)
                    {
                        float newShieldHealth = 0f;
                        // When shielding is -ve (e.g. -0.01 when it has been used up) set the shielding amount rather than adding it
                        if (damageRegion.ShieldHealth < 0f) { newShieldHealth = health - damageRegion.startingHealth; }
                        else { newShieldHealth = damageRegion.ShieldHealth + health - damageRegion.startingHealth; }

                        // Cap shielding to maximum permitted.
                        if (newShieldHealth > damageRegion.shieldingAmount) { newShieldHealth = damageRegion.shieldingAmount; }

                        damageRegion.ShieldHealth = newShieldHealth;
                    }

                    // Cap health to maximum permitted
                    health = damageRegion.startingHealth;
                }

                damageRegion.Health = health;

                // Not sure if this is correct... why do we do this??
                damageRegion.isDestructionEffectsObjectInstantiated = false;

                // Use !shipDamageModelIsSimple first to avoid always having to lookup the enumeration 
                if (!shipDamageModelIsSimple)
                {
                    int damageRegionIndex = GetDamageRegionIndex(damageRegion);

                    // Apply health to each of the components
                    if (damageRegionIndex >= 0)
                    {
                        // For Progressive, a damageRegionIndex of 0 on the component means Use Progressive Damage.
                        // If -1, the component doesn't use Progressive damage.
                        // For localised damage, the damageRegionIndex needs to match the damage region that health is being added to.

                        // Add health to Thrusters
                        componentListSize = thrusterList.Count;
                        for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                        {
                            // For Progressive, a region index of 0 means Use Progressive Damage.
                            if (thrusterList[componentIndex].damageRegionIndex == damageRegionIndex)
                            {
                                health = thrusterList[componentIndex].Health;
                                if (health < 0f) { health = healthAmount; }
                                else { health += healthAmount; }
                                if (health > thrusterList[componentIndex].startingHealth) { health = thrusterList[componentIndex].startingHealth; }

                                // Only set the health value once for a given thruster
                                thrusterList[componentIndex].Health = health;
                            }
                        }

                        //Add health to Wings
                        componentListSize = wingList.Count;
                        for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                        {
                            // For Progressive, a region index of 0 means Use Progressive Damage
                            if (wingList[componentIndex].damageRegionIndex == damageRegionIndex)
                            {
                                health = wingList[componentIndex].Health;
                                if (health < 0f) { health = healthAmount; }
                                else { health += healthAmount; }
                                if (health > wingList[componentIndex].startingHealth) { health = wingList[componentIndex].startingHealth; }

                                // Only set the health value once for a given wing
                                wingList[componentIndex].Health = health;
                            }
                        }

                        // Add health to Control Surfaces
                        componentListSize = controlSurfaceList.Count;
                        for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                        {
                            // For Progressive, a region index of 0 means Use Progressive Damage
                            if (controlSurfaceList[componentIndex].damageRegionIndex == damageRegionIndex)
                            {
                                health = controlSurfaceList[componentIndex].Health;
                                if (health < 0f) { health = healthAmount; }
                                else { health += healthAmount; }
                                if (health > controlSurfaceList[componentIndex].startingHealth) { health = controlSurfaceList[componentIndex].startingHealth; }

                                // Only set the health value once for a given Control Surface
                                controlSurfaceList[componentIndex].Health = health;
                            }
                        }

                        // Add health to Weapons
                        componentListSize = weaponList.Count;
                        for (componentIndex = 0; componentIndex < componentListSize; componentIndex++)
                        {
                            // For Progressive, a region index of 0 means Use Progressive Damage
                            if (weaponList[componentIndex].damageRegionIndex == damageRegionIndex)
                            {
                                health = weaponList[componentIndex].Health;
                                if (health < 0f) { health = healthAmount; }
                                else { health += healthAmount; }
                                if (health > weaponList[componentIndex].startingHealth) { health = weaponList[componentIndex].startingHealth; }

                                // Only set the health value once for a given Weapon
                                weaponList[componentIndex].Health = health;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies damage to the ship due to a collision. The damage is registered as "collision" damage,
        /// meaning that if it destroys the ship the ship will be respawned at a previously recorded respawn position.
        /// </summary>
        /// <param name="collisionInfo"></param>
        public void ApplyCollisionDamage (Collision collisionInfo)
        {
            int hitColliderID = collisionInfo.collider.GetInstanceID();

            // Avoid collision with colliders that have been registered with (attached to) this ship
            if (!attachedCollisionColliders.Contains(hitColliderID))
            {
                // Loop through contact points
                int numberOfContactPoints = collisionInfo.contactCount;
                float damageAtEachPoint = collisionInfo.impulse.magnitude / numberOfContactPoints;
                for (int c = 0; c < numberOfContactPoints; c++)
                {
                    //Debug.Log("[DEBUG] " + collisionInfo.collider.name + " hit " + collisionInfo.GetContact(c).thisCollider.name + " T:" + Time.time);

                    // Apply damage at each contact point
                    ApplyDamage(damageAtEachPoint, ProjectileModule.DamageType.Default, collisionInfo.GetContact(c).point, true);
                }

                // Remember that this was collision damage
                lastDamageWasCollisionDamage = true;
            }
        }

        /// <summary>
        /// Applies a specified amount of damage to the ship at a specified position. The damage is registered as "collision" damage,
        /// meaning that if it destroys the ship the ship will be respawned at a previously recorded respawn position.
        /// </summary>
        /// <param name="damageAmount"></param>
        /// <param name="damagePosition"></param>
        public void ApplyCollisionDamage(float damageAmount, Vector3 damagePosition)
        {
            ApplyDamage(damageAmount, ProjectileModule.DamageType.Default, damagePosition, false);
            // Remember that this was collision damage
            lastDamageWasCollisionDamage = true;
        }

        /// <summary>
        /// Applies a specified amount of damage to the ship at a specified position.
        /// </summary>
        /// <param name="damageAmount"></param>
        /// <param name="damagePosition"></param>
        public void ApplyNormalDamage(float damageAmount, ProjectileModule.DamageType damageType, Vector3 damagePosition)
        {
            ApplyDamage(damageAmount, damageType, damagePosition, false);
            // Remember that this was not collision damage
            lastDamageWasCollisionDamage = false;
        }

        /// <summary>
        /// Returns the index of the last damage event. When this value changes, a damage event has occurred.
        /// </summary>
        /// <returns></returns>
        public int LastDamageEventIndex() { return lastDamageEventIndex; }

        /// <summary>
        /// Returns the amount of rumble required due to the last damage event.
        /// </summary>
        /// <returns></returns>
        public float RequiredDamageRumbleAmount() { return damageRumbleAmountRequired; }

        /// <summary>
        /// Returns the amount of camera shake required due to the last damage event
        /// </summary>
        /// <returns></returns>
        public float RequiredCameraShakeAmount() { return damageCameraShakeAmountRequired; }
        #endregion

        #region General API Methods

        /// <summary>
        /// Returns an interpolated velocity in world space (akin to to the interpolated position/rotation on rigidbodies).
        /// </summary>
        public Vector3 GetInterpolatedWorldVelocity()
        {
            // Get the time in seconds since the last fixed update
            float timeSinceLastFixedUpdate = Time.time - lastFixedUpdateTime;
            // Interpolate between the world velocities of the last frame and the previous frame to get an interpolated world velocity
            return previousFrameWorldVelocity + (worldVelocity - previousFrameWorldVelocity) * (timeSinceLastFixedUpdate / Time.fixedDeltaTime);
        }

        /// <summary>
        /// Add temporary boost to the ship in a normalised local-space forceDirection
        /// which a force of forceAmount Newtons for a period of duration seconds.
        /// IMPORTANT: forceDirection must be normalised, otherwise you will get odd results.
        /// </summary>
        /// <param name="forceDirection"></param>
        /// <param name="forceAmount"></param>
        /// <param name="duration"></param>
        public void AddBoost(Vector3 forceDirection, float forceAmount, float duration)
        {
            boostDirection = forceDirection;
            boostForce = forceAmount;
            boostTimer = duration;
        }

        /// <summary>
        /// Immediately stop any boost that has been applied with AddBoost(..).
        /// </summary>
        public void StopBoost()
        {
            boostTimer = 0f;
        }

        #endregion

        #region Respawning API Methods

        /// <summary>
        /// Returns true if the ship has been destroyed.
        /// </summary>
        public bool Destroyed()
        {
            if (shipDamageModel == ShipDamageModel.Simple || shipDamageModel == ShipDamageModel.Progressive)
            {
                return mainDamageRegion.Health <= 0f;
            }
            else
            {
                return mainDamageRegion.Health <= 0f;
            }
        }

        /// <summary>
        /// Returns the position for the ship to respawn at.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetRespawnPosition()
        {
            if (respawningMode == RespawningMode.RespawnAtLastPosition)
            {
                if (lastDamageWasCollisionDamage) { return currentCollisionRespawnPosition; }
                else { return currentRespawnPosition; }
            }
            else if (respawningMode == RespawningMode.RespawnAtOriginalPosition)
            {
                return currentRespawnPosition;
            }
            else if (respawningMode == RespawningMode.RespawnAtSpecifiedPosition)
            {
                return customRespawnPosition;
            }
            else { return Vector3.zero; }
        }

        /// <summary>
        /// Returns the rotation for the ship to respawn with.
        /// </summary>
        /// <returns></returns>
        public Quaternion GetRespawnRotation()
        {
            if (respawningMode == RespawningMode.RespawnAtLastPosition)
            {
                if (lastDamageWasCollisionDamage) { return currentCollisionRespawnRotation; }
                else { return currentRespawnRotation; }
            }
            else if (respawningMode == RespawningMode.RespawnAtOriginalPosition)
            {
                return currentRespawnRotation;
            }
            else if (respawningMode == RespawningMode.RespawnAtSpecifiedPosition)
            {
                return Quaternion.Euler(customRespawnRotation);
            }
            else { return Quaternion.identity; }
        }

        #endregion

        #region Thruster API Methods

        /// <summary>
        /// Get the (central) fuel level of the ship. Fuel Level Range 0.0 (empty) to 100.0 (full).
        /// </summary>
        /// <returns></returns>
        public float GetFuelLevel()
        {
            return centralFuelLevel;
        }

        /// <summary>
        /// Get the fuel level of the Thruster based on the order it appears in the Thrusters tab
        /// in the ShipControlModule. Numbers begin at 1.
        /// Fuel Level Range 0.0 (empty) to 100.0 (full).
        /// </summary>
        /// <param name="thrusterNumber"></param>
        /// <returns></returns>
        public float GetFuelLevel (int thrusterNumber)
        {
            if (thrusterNumber > 0 && thrusterNumber <= thrusterList.Count)
            {
                return thrusterList[thrusterNumber - 1].fuelLevel;
            }
            else { return 0f; }
        }

        /// <summary>
        /// Get the heat level of the Thruster based on the order it appears in the Thrusters tab
        /// in the ShipControlModule. Numbers begin at 1.
        /// Heat Level Range 0.0 (min) to 100.0 (max).
        /// </summary>
        /// <param name="thrusterNumber"></param>
        /// <returns></returns>
        public float GetHeatLevel (int thrusterNumber)
        {
            if (thrusterNumber > 0 && thrusterNumber <= thrusterList.Count)
            {
                return thrusterList[thrusterNumber - 1].heatLevel;
            }
            else { return 0f; }
        }

        /// <summary>
        /// Get the overheating threshold of the Thruster based on the order it appears in the Thrusters tab
        /// in the ShipControlModule. Numbers begin at 1.
        /// </summary>
        /// <param name="thrusterNumber"></param>
        /// <returns></returns>
        public float GetOverheatingThreshold (int thrusterNumber)
        {
            if (thrusterNumber > 0 && thrusterNumber <= thrusterList.Count)
            {
                return thrusterList[thrusterNumber - 1].overHeatThreshold;
            }
            else { return 0f; }
        }

        /// <summary>
        /// Get the Maximum Thrust of the Thruster based on the order it appears in the Thrusters tab
        /// in the ShipControlModule. Numbers begin at 1. Values are returned in kilo Newtons.
        /// </summary>
        /// <param name="thrusterNumber"></param>
        /// <returns></returns>
        public float GetMaxThrust (int thrusterNumber)
        {
            if (thrusterNumber > 0 && thrusterNumber <= thrusterList.Count)
            {
                return thrusterList[thrusterNumber-1].maxThrust;
            }
            else { return 0f; }
        }

        /// <summary>
        /// Is the thruster heat level at or above the overheating threshold?
        /// Thruster Number is based on the order it appears in the Thrusters tab
        /// in the ShipControlModule. Numbers begin at 1.
        /// </summary>
        /// <param name="thrusterNumber"></param>
        /// <returns></returns>
        public bool IsThrusterOverheating (int thrusterNumber)
        {
            if (thrusterNumber > 0 && thrusterNumber <= thrusterList.Count)
            {
                return thrusterList[thrusterNumber - 1].IsThrusterOverheating();
            }
            else { return false; }
        }

        /// <summary>
        /// Repair the health of a thruster. Will also set the heat level to 0.
        /// Can be useful if a thruster has burnt out after being over heated.
        /// Thruster Number is based on the order it appears in the Thrusters tab
        /// in the ShipControlModule. Numbers begin at 1.
        /// </summary>
        /// <param name="thrusterNumber"></param>
        public void RepairThruster (int thrusterNumber)
        {
            if (thrusterNumber > 0 && thrusterNumber <= thrusterList.Count)
            {
                thrusterList[thrusterNumber - 1].Repair();
            }
        }

        /// <summary>
        /// Set the central fuel level for the whole ship. If useCentralFuel is false,
        /// use SetFuelLevel (thrusterNumber, new FuelLevel).
        /// </summary>
        /// <param name="newFuelLevel"></param>
        public void SetFuelLevel (float newFuelLevel)
        {
            // Clamp the fuel level
            if (newFuelLevel < 0f) { newFuelLevel = 0f; }
            else if (newFuelLevel > 100f) { newFuelLevel = 100f; }

            centralFuelLevel = newFuelLevel;
        }

        /// <summary>
        /// Set the fuel level of the Thruster based on the order it appears in the Thrusters tab
        /// in the ShipControlModule. Numbers begin at 1.
        /// Fuel Level Range 0.0 (empty) to 100.0 (full).
        /// </summary>
        /// <param name="thrusterNumber"></param>
        /// <returns></returns>
        public void SetFuelLevel (int thrusterNumber, float newFuelLevel)
        {
            if (thrusterNumber > 0 && thrusterNumber <= thrusterList.Count)
            { 
                thrusterList[thrusterNumber - 1].SetFuelLevel(newFuelLevel);
            }
        }

        /// <summary>
        /// Set the heat level of the Thruster based on the order it appears in the Thrusters tab
        /// in the ShipControlModule. Numbers begin at 1.
        /// Heat Level Range 0.0 (min) to 100.0 (max).
        /// </summary>
        /// <param name="thrusterNumber"></param>
        /// <returns></returns>
        public void SetHeatLevel (int thrusterNumber, float newHeatLevel)
        {
            if (thrusterNumber > 0 && thrusterNumber <= thrusterList.Count)
            {
                thrusterList[thrusterNumber - 1].SetHeatLevel(newHeatLevel);
            }
        }

        /// <summary>
        /// Set the Maximum Thrust of the Thruster based on the order it appears in the Thruster tab
        /// in the ShipControlModule. Numbers begin at 1. Values should be in kilo Newtons
        /// </summary>
        /// <param name="thrusterNumber"></param>
        /// <param name="newMaxThrustkN"></param>
        public void SetMaxThrust (int thrusterNumber, int newMaxThrustkN)
        {
            if (thrusterNumber > 0 && thrusterNumber <= thrusterList.Count)
            {
                thrusterList[thrusterNumber - 1].maxThrust = newMaxThrustkN * 1000f;
            }
        }

        /// <summary>
        /// Use this if you want fine-grained control over max thruster force, otherwise use SetMaxThrust.
        /// Set the Maximum Thrust of the Thruster based on the order it appears in the Thruster tab
        /// in the ShipControlModule. Numbers begin at 1. Values are in Newtons. 
        /// </summary>
        /// <param name="thrusterNumber"></param>
        /// <param name="newMaxThrustNewtons"></param>
        public void SetMaxThrustNewtons (int thrusterNumber, float newMaxThrustNewtons)
        {
            if (thrusterNumber > 0 && thrusterNumber <= thrusterList.Count)
            {
                thrusterList[thrusterNumber - 1].maxThrust = newMaxThrustNewtons;
            }
        }

        /// <summary>
        /// Set all thrusters to the same throttle value.
        /// Values are clamped to between 0.0 and 1.0.
        /// </summary>
        /// <param name="newThrottleValue"></param>
        public void SetThrottleAllThrusters (float newThrottleValue)
        {
            // Clamp values
            if (newThrottleValue < 0f) { newThrottleValue = 0f; }
            else if (newThrottleValue > 1f) { newThrottleValue = 1f; }

            int numThrusters = thrusterList == null ? 0 : thrusterList.Count;

            for (int thIdx = 0; thIdx < numThrusters; thIdx++)
            {
                thrusterList[thIdx].throttle = newThrottleValue;
            }
        }

        /// <summary>
        /// Set the throttle for a given thruster. Numbers begin at 1. Values should be between 0.0 and 1.0.
        /// </summary>
        /// <param name="thrusterNumber"></param>
        /// <param name="newThrottleValue"></param>
        public void SetThrusterThrottle (int thrusterNumber, float newThrottleValue)
        {
            if (thrusterNumber > 0 && thrusterNumber <= thrusterList.Count)
            {
                thrusterList[thrusterNumber - 1].throttle = newThrottleValue < 0f ? 0f : newThrottleValue > 1f ? 1f : newThrottleValue;
            }
        }

        #endregion

        #region Weapon API Methods

        /// <summary>
        /// Add a pre-configured weapon to the current list of weapons and initialise it for use
        /// if the ship has already been initialised.
        /// </summary>
        /// <param name="weaponToAdd"></param>
        public void AddWeapon (Weapon weaponToAdd)
        {
            if (weaponToAdd != null)
            {
                weaponList.Add(weaponToAdd);

                // If the ship has already been initialised, initialise the weapon.
                if (shipId != 0)
                {
                    weaponToAdd.Initialise(trfmInvRot);
                }
            }
        }

        /// <summary>
        /// Deactivate all beam weapons that are currently firing
        /// </summary>
        public void DeactivateBeams(SSCManager sscManager)
        {
            int numWeapons = NumberOfWeapons;

            for (int weaponIdx = 0; weaponIdx < numWeapons; weaponIdx++)
            {
                Weapon weapon = weaponList[weaponIdx];
                if (weapon != null && (weapon.weaponTypeInt == Weapon.FixedBeamInt || weaponTypeInt == Weapon.TurretBeamInt))
                {
                    int numBeams = weapon.beamItemKeyList == null ? 0 : weapon.beamItemKeyList.Count;

                    for (int bItemIdx = 0; bItemIdx < numBeams; bItemIdx++)
                    {
                        if (weapon.beamItemKeyList[bItemIdx].beamSequenceNumber > 0)
                        {
                            sscManager.DeactivateBeam(weapon.beamItemKeyList[bItemIdx]);
                        }

                        weapon.beamItemKeyList[bItemIdx] = new SSCBeamItemKey(-1, -1, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Get the zero-based index of a weapon on the ship given the weapon name. Returns -1 if not found.
        /// Use this sparingly as it will incur garabage. Always declare the parameter as a static readonly variable.
        /// Usage:
        /// private static readonly string WPNturret1Name = "Turret 1";
        /// ..
        /// int wpIdx = GetWeaponIndexByName(WPNturret1Name);
        /// </summary>
        /// <param name="weaponName"></param>
        /// <returns></returns>
        public int GetWeaponIndexByName(string weaponName)
        {
            if (weaponList != null) { return weaponList.FindIndex(wp => wp.name == weaponName); }
            else { return -1; }
        }

        /// <summary>
        /// Get the weapon on the ship from a zero-based index in the list of weapons.
        /// This will be 1 less than the number shown next to the weapon on the Combat tab.
        /// It validates the weaponIdx so if possible, don't call this every frame.
        /// </summary>
        /// <param name="weaponIdx"></param>
        /// <returns></returns>
        public Weapon GetWeaponByIndex(int weaponIdx)
        {
            if (weaponIdx >= 0 && weaponIdx < (weaponList == null ? 0 : weaponList.Count))
            {
                return weaponList[weaponIdx];
            }
            else { return null; }
        }

        /// <summary>
        /// Get the heat level of the Weapon with the zero-based index in list of weapons.
        /// Heat Level Range 0.0 (min) to 100.0 (max).
        /// </summary>
        /// <param name="weaponIdx"></param>
        /// <returns></returns>
        public float GetWeaponHeatLevel (int weaponIdx, float newHeatLevel)
        {
            if (weaponIdx >= 0 && weaponIdx < (weaponList == null ? 0 : weaponList.Count))
            {
                return weaponList[weaponIdx].heatLevel;
            }
            else { return 0f; }
        }

        /// <summary>
        /// Get the index in the weaponList of next weapon that has AutoTargetingEnabled.
        /// If no match is found, return -1. startIdx is the zero-based index in the
        /// weaponList to begin the search.
        /// </summary>
        /// <param name="startIdx"></param>
        /// <returns></returns>
        public int GetNextAutoTargetingWeaponIndex(int startIdx)
        {
            int foundWeaponIdx = -1;

            Weapon _tempWeapon = null;
            int numWeapons = weaponList == null ? 0 : weaponList.Count;
            if (startIdx >= 0 && startIdx < numWeapons)
            {
                // Use a for-loop rather than list.Find to avoid GC
                for (int weaponIdx = startIdx; weaponIdx < numWeapons; weaponIdx++)
                {
                    _tempWeapon = weaponList[weaponIdx];
                    if (_tempWeapon != null && _tempWeapon.isAutoTargetingEnabled)
                    {
                        foundWeaponIdx = weaponIdx;
                        break;
                    }
                }
            }
            return foundWeaponIdx;
        }

        /// <summary>
        /// Clears all targeting information for the weapon. This should be called if you do not
        /// know if the target is a ship or a gameobject.
        /// WARNING: For the sake of performance, does not validate weaponIdx.
        /// </summary>
        /// <param name="weaponIdx">zero-based index in list of weapons</param>
        public void ClearWeaponTarget(int weaponIdx)
        {
            if (weaponIdx >= 0) { weaponList[weaponIdx].ClearTarget(); }
        }

        /// <summary>
        /// Has the weapon with the zero-based index in list of weapons got a target
        /// assigned to it? The target could be a GameObject or a Ship.
        /// WARNING: For the sake of performance, does not validate weaponIdx.
        /// </summary>
        /// <param name="weaponIdx"></param>
        /// <returns></returns>
        public bool HasWeaponTarget(int weaponIdx)
        {
            if (weaponIdx >= 0) { return weaponList[weaponIdx].target != null; }
            else { return false; }
        }

        /// <summary>
        /// Set the heat level of the Weapon with the zero-based index in list of weapons.
        /// Heat Level Range 0.0 (min) to 100.0 (max).
        /// </summary>
        /// <param name="weaponIdx"></param>
        /// <returns></returns>
        public void SetWeaponHeatLevel (int weaponIdx, float newHeatLevel)
        {
            if (weaponIdx >= 0 && weaponIdx < (weaponList == null ? 0 : weaponList.Count))
            {
                weaponList[weaponIdx].SetHeatLevel(newHeatLevel);
            }
        }

        /// <summary>
        /// Sets the gameobject that the weapon will attempt to aim at. Currently only applies
        /// to Weapon.WeaponType.TurretProjectile and FixedProjectile weapons with guided projectiles.
        /// However, for the sake of performance, this method does not do any WeaponType validation.
        /// WARNING: This method will generate garbage. Where possible call
        /// SetWeaponTarget(int weaponIdx, GameObject target). If only the name is known,
        /// first call GetWeaponIndexByName(string weaponName) once in your Awake() routine.
        /// </summary>
        /// <param name="weaponName"></param>
        /// <param name="target"></param>
        public void SetWeaponTarget(string weaponName, GameObject target)
        {
            int wpIdx = GetWeaponIndexByName(weaponName);
            if (wpIdx >= 0)
            {
                // It is quicker not to check the weapon type and just set the target
                // Call SetTarget so it is configured correctly rather than setting the public variable
                weaponList[wpIdx].SetTarget(target);
            }
        }

        /// <summary>
        /// Sets the gameobject that the weapon will attempt to aim at. Currently only applies
        /// to Weapon.WeaponType.TurretProjectile and FixedProjectile weapons with guided projectiles.
        /// However, for the sake of performance, this method does not do any WeaponType validation.
        /// WARNING: For the sake of performance, does not validate weaponIdx.
        /// </summary>
        /// <param name="weaponIdx">zero-based index in list of weapons</param>
        /// <param name="target"></param>
        public void SetWeaponTarget(int weaponIdx, GameObject target)
        {
            // Currently don't check weapon type or number of weapons as this would incur overhead.
            if (weaponIdx >= 0)
            {
                // Call SetTarget so it is configured correctly rather than setting the public variable
                weaponList[weaponIdx].SetTarget(target);
            }
        }

        /// <summary>
        /// Sets the ship that the weapon will attempt to aim at. Currently only applies
        /// to Weapon.WeaponType.TurretProjectile and FixedProjectile weapons with guided projectiles.
        /// However, for the sake of performance, this method does not do any WeaponType validation.
        /// WARNING: For the sake of performance, does not validate weaponIdx.
        /// </summary>
        /// <param name="weaponIdx">zero-based index in list of weapons</param>
        /// <param name="targetShipControlModule"></param>
        public void SetWeaponTargetShip(int weaponIdx, ShipControlModule targetShipControlModule)
        {
            // Currently don't check weapon type or number of weapons as this would incur overhead.
            if (weaponIdx >= 0)
            {
                // Call SetTargetShip so it is configured correctly rather than setting the public variable
                weaponList[weaponIdx].SetTargetShip(targetShipControlModule);
            }
        }

        /// <summary>
        /// Sets the ship's damage region that the weapon will attempt to aim at. Currently only applies
        /// to Weapon.WeaponType.TurretProjectile and FixedProjectile weapons with guided projectiles.
        /// However, for the sake of performance, this method does not do any WeaponType validation.
        /// WARNING: For the sake of performance, does not validate weaponIdx.
        /// </summary>
        /// <param name="weaponIdx"></param>
        /// <param name="targetShipControlModule"></param>
        /// <param name="damageRegion"></param>
        public void SetWeaponTargetShipDamageRegion (int weaponIdx, ShipControlModule targetShipControlModule, DamageRegion damageRegion)
        {
            // Currently don't check weapon type or number of weapons as this would incur overhead.
            if (weaponIdx >= 0)
            {
                // Call SetTargetShipDamageRegion so it is configured correctly rather than setting the public variable
                weaponList[weaponIdx].SetTargetShipDamageRegion(targetShipControlModule, damageRegion);
            }
        }

        /// <summary>
        /// Performs a once-off change to the direction the weapon will fire based on a
        /// "target" position in world-space. The weapon will NOT stay locked onto this
        /// position.
        /// NOTE: Should NOT be used for Turrets. Use SetWeaponTarget(..) instead.
        /// WARNING: For the sake of performance, does not validate weaponIdx.
        /// </summary>
        /// <param name="weaponIdx">zero-based position in the list of weapons</param>
        /// <param name="aimAtWorldSpacePosition"></param>
        public void SetWeaponFireDirection(int weaponIdx, Vector3 aimAtWorldSpacePosition)
        {
            // Currently don't check weapon type or number of weapons as this would incur overhead.
            if (weaponIdx >= 0)
            {
                Weapon weapon = weaponList[weaponIdx];
                if (weapon != null)
                {
                    // Get base weapon world position (not accounting for relative fire position)
                    weaponWorldBasePosition = (trfmRight * weapon.relativePosition.x) +
                                            (trfmUp * weapon.relativePosition.y) +
                                            (trfmFwd * weapon.relativePosition.z) +
                                            trfmPos;

                    // Convert ws fire direction into local ship space
                    weapon.fireDirection = trfmInvRot * (aimAtWorldSpacePosition - weaponWorldBasePosition);
                    weapon.Initialise(trfmInvRot);
                }
            }
        }

        /// <summary>
        /// Returns whether a weapon has line of sight to the weapon's specified target (i.e. weapon.target).
        /// If directToTarget is set to true, will raycast directly from the weapon to the target.
        /// If directToTarget is set to false, will raycast in the direction the weapon is facing.
        /// This method will return true if the raycast hits:
        /// a) The weapon's specified target,
        /// b) An enemy ship (distinguished by faction ID) - even if it is not the target and anyEnemy is true,
        /// c) An object that isn't the target (if obstaclesBlockLineOfSight is set to false),
        /// d) Nothing.
        /// This method will return false if the raycast hits:
        /// a) A friendly ship (distinguished by faction ID),
        /// b) An object that isn't the target (if obstaclesBlockLineOfSight is set to true).
        /// c) An enemy ship that is not the target when anyEnemy is false
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="directToTarget"></param>
        /// <returns></returns>
        public bool WeaponHasLineOfSight (Weapon weapon, bool directToTarget = false, bool obstaclesBlockLineOfSight = true, bool anyEnemy = true)
        {
            return WeaponHasLineOfSight(weapon, weapon.target, directToTarget, obstaclesBlockLineOfSight, anyEnemy);
        }

        /// <summary>
        /// Returns whether a weapon has line of sight to a target.
        /// If directToTarget is set to true, will raycast directly from the weapon to the target.
        /// If directToTarget is set to false, will raycast in the direction the weapon is facing.
        /// This method will return true if the raycast hits:
        /// a) The target,
        /// b) An enemy ship (distingushed by faction ID) - even if it is not the target and anyEnemy is true,
        /// c) An object that isn't the target (if obstaclesBlockLineOfSight is set to false),
        /// d) Nothing.
        /// This method will return false if the raycast hits:
        /// a) A friendly ship (distinguished by faction ID),
        /// b) An object that isn't the target (if obstaclesBlockLineOfSight is set to true).
        /// c) An enemy ship that is not the target when anyEnemy is false
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="target"></param>
        /// <param name="directToTarget"></param>
        /// <returns></returns>
        public bool WeaponHasLineOfSight(Weapon weapon, GameObject target, bool directToTarget = false, bool obstaclesBlockLineOfSight = true, bool anyEnemy = true)
        {
            // Update the line-of-sight property
            weapon.UpdateLineOfSight(target, trfmPos, trfmRight, trfmUp, trfmFwd, trfmInvRot, factionId, 
                false, directToTarget, obstaclesBlockLineOfSight, anyEnemy);

            // Return the updated property
            return weapon.HasLineOfSight;
        }

        #endregion

        #endregion
    }
}
