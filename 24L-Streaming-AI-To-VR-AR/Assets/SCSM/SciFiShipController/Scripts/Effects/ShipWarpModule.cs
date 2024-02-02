using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// A warp drive-like effect that works with a shipControlModule.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Misc/Ship Warp Module")]
    [HelpURL("https://scsmmedia.com/ssc-documentation")]
    public class ShipWarpModule : MonoBehaviour
    {
        #region Enumerations

        public enum EnvAmbientSource
        {
            Colour = 0,
            Gradient = 1,
            Skybox = 2
        }

        #endregion

        #region Public Variables

        public bool initialiseOnStart = false;

        /// <summary>
        /// Allow the user of Custom Player inputs during warp
        /// </summary>
        public bool allowCustomInputs = false;

        public Color nightSkyColour = new Color(21f / 255f, 21f / 255f, 21f / 255f, 1f);

        [Tooltip("By default the ambient sky colour will be set to the nightSkyColour")]
        public bool overrideAmbientColour = false;

        [Tooltip("If overriding the ambient colour, this is the ambient sky colour")]
        public Color ambientSkyColour = new Color(21f / 255f, 21f / 255f, 21f / 255f, 1f);

        /// <summary>
        /// These methods are called immediately after the camera settings have been changed
        /// </summary>
        public SSCCameraSettingsEvt1 onChangeCameraSettings = null;

        /// <summary>
        /// These methods get called immediately before Engage() is executed.
        /// </summary>
        public UnityEvent onPreEngageWarp = null;

        /// <summary>
        /// These methods get called immediately after Engage() is executed.
        /// </summary>
        public UnityEvent onPostEngageWarp = null;

        /// <summary>
        /// These methods get called immediately before Disengage() is executed.
        /// </summary>
        public UnityEvent onPreDisengageWarp = null;

        /// <summary>
        /// These methods get called immediately after Disengage() is executed.
        /// </summary>
        public UnityEvent onPostDisengageWarp = null;

        #endregion

        #region Public Properties - General

        /// <summary>
        /// Get or set the source of the ambient light. Colour, Gradient or Skybox.
        /// </summary>
        public EnvAmbientSource EnvironmentAmbientSource { get { return envAmbientSource; } set { SetEnvironmentAmbientSource(value); } }

        /// <summary>
        /// Is the module initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// Is warp currently engaged?
        /// </summary>
        public bool IsWarpEngaged { get { return isWarpEngaged; } }

        /// <summary>
        /// If greater than zero, the time, in seconds, that warp will automatically disengage.
        /// </summary>
        public float MaxWarpDuration { get { return maxWarpDuration; } set { SetMaxWarpDuration(value); } }

        #endregion

        #region Public Properties - Camera

        /// <summary>
        /// The zero-based index of the currently used ShipCameraSetttings.
        /// Will return - 1 if th module is not initialised, or there are no active camera settings applied.
        /// </summary>
        public int CurrentCameraSettingsIndex { get { return isInitialised ? currentCameraSettingsIndex : -1; } }

        /// <summary>
        /// If there are optional camera settings configured, apply the first one when warp is engaged.
        /// </summary>
        public bool IsApplyCameraSettingsOnEngage { get { return isApplyCameraSettingsOnEngage; } set { isApplyCameraSettingsOnEngage = value; } }


        #endregion

        #region Public Static Variables

        #endregion

        #region Protected Variables - General

        /// <summary>
        /// The source of the ambient light. Colour, Gradient or Skybox
        /// </summary>
        [SerializeField] protected EnvAmbientSource envAmbientSource = EnvAmbientSource.Colour;

        /// <summary>
        /// If greater than zero, the time, in seconds, that warp will automatically disengage.
        /// </summary>
        [SerializeField, Range(0f, 300f)] protected float maxWarpDuration = 0f;

        /// <summary>
        /// The offset, in local space, from warp fx is from the position of the ship
        /// </summary>
        [SerializeField] protected Vector3 offsetFromShip = new Vector3(0f, 0f, 40f);

        protected bool isInitialised = false;
        protected bool isSavedShipSettings = false;
        protected bool isWarpEngaged = false;
        protected SSCManager sscManager = null;

        protected float savedGravitationalAcceleration = 0f;
        protected bool savedIsPlayerInputEnabled = false;

        protected SSCRandom sscRandom;
        protected float warpEngagedTimer = 0f;

        #endregion

        #region Protected Variables - Ship

        /// <summary>
        /// The amount of proportional thrust to apply to forward thrusters when warp is engaged.
        /// </summary>
        [SerializeField, Range(0f, 1f)] protected float shipForwardThrust = 0.5f;

        /// <summary>
        /// The module used to control the player ship
        /// </summary>
        [SerializeField] protected ShipControlModule shipControlModule = null;

        /// <summary>
        /// The minimum interval, in seconds, between ship shake incidents
        /// </summary>
        [SerializeField, Range(0.1f, 10f)] protected float minShakeInterval = 0.5f;

        /// <summary>
        /// The maximum interval, in seconds, between ship shake incidents
        /// </summary>
        [SerializeField, Range(0.1f, 10f)] protected float maxShakeInterval = 3f;

        /// <summary>
        /// The maximum strength of the ship shake. Smaller numbers are better.
        /// This can be overridden by calling ShakeShip(duration,strength)
        /// </summary>
        [SerializeField, Range(0.005f, 0.5f)] protected float maxShakeStrength = 0.05f;

        /// <summary>
        /// The maximum duration, in seconds, the ship will shake per incident.
        /// This can be overridden by calling ShakeShip(duration,strength).
        /// </summary>
        [SerializeField, Range(0.1f, 5f)] protected float maxShakeDuration = 0.2f;

        /// <summary>
        /// The maximum angle, in degrees, the ship can pitch down.
        /// </summary>
        [SerializeField, Range(0f, 15f)] protected float maxShipPitchDown = 1f;

        /// <summary>
        /// The maximum time, in seconds, the ship will take to pitch up and down
        /// </summary>
        [SerializeField, Range(0f, 20f)] protected float maxShipPitchDuration = 5f;

        /// <summary>
        /// The maximum angle, in degrees, the ship can pitch up.
        /// </summary>
        [SerializeField, Range(0f, 15f)] protected float maxShipPitchUp = 1f;

        /// <summary>
        /// The curve used to evaluate the amount of pitch over the pitch duration of each pitch incident.
        /// </summary>
        [SerializeField] protected AnimationCurve shipPitchCurve = GetDefaultPitchCurve();

        /// <summary>
        /// The maximum angle, in degrees, the ship can roll left or right.
        /// </summary>
        [SerializeField, Range(0f, 30f)] protected float maxShipRollAngle = 3f;

        /// <summary>
        /// The maximum time, in seconds, the ship will take to roll from left to right
        /// </summary>
        [SerializeField, Range(0f, 20f)] protected float maxShipRollDuration = 5f;

        /// <summary>
        /// The curve used to evaluate the amount of roll over the roll duration of each roll incident.
        /// </summary>
        [SerializeField] protected AnimationCurve shipRollCurve = GetDefaultRollCurve();

        protected bool isPlayerShip = false;

        [NonSerialized] protected PlayerInputModule playerInputModule = null;
        [NonSerialized] protected Ship ship = null;
        [NonSerialized] protected Rigidbody shipRBody = null;

        protected Vector3 shipOriginPos = Vector3.zero;
        protected Quaternion shipOriginRot = Quaternion.identity;

        protected bool isShaking = false;
        protected float shakeStrength = 1f;
        protected float shakeDuration = 0f;
        protected float shakeShipTimer = 0f;
        protected float shakeIntervalTimer = 0f;

        protected float targetShipPitchAngle = 0f;
        protected float currentShipPitchAngle = 0f;
        protected float previousShipPitchAngle = 0f;
        protected float targetShipPitchDuration = 0f;
        protected float shipPitchTimer = 0f;

        protected float targetShipRollAngle = 0f;
        protected float currentShipRollAngle = 0f;
        protected float previousShipRollAngle = 0f;
        protected float targetShipRollDuration = 0f;
        protected float shipRollTimer = 0f;

        protected Vector3 currentShipPosOffset = Vector3.zero;
        protected Quaternion currentShipRot = Quaternion.identity;

        /// <summary>
        /// A list of forward thruster settings for isMinEffectsAlwaysOn
        /// </summary>
        [NonSerialized] protected readonly List<bool> isMinEffectsAlwaysOnList = new List<bool>();
        protected int numThrusters = 0;
        protected int numFwdThrusters = 0;
        protected bool savedIsThrusterFXStationary = false;

        #endregion

        #region Protected Variables - Camera

        /// <summary>
        /// If there are optional camera settings configured, apply the first one when warp is engaged.
        /// </summary>
        [SerializeField] protected bool isApplyCameraSettingsOnEngage = false;

        /// <summary>
        /// The module used to control the player ship camera
        /// </summary>
        [SerializeField] protected ShipCameraModule shipCameraModule = null;

        /// <summary>
        /// A list of optional ShipCameraSettings for switching between camera settings when warp is engaged.
        /// </summary>
        [SerializeField] protected List<ShipCameraSettings> shipCameraSettingsList = new List<ShipCameraSettings>();

        [NonSerialized] protected Camera camera1;

        protected int currentCameraSettingsIndex = -1;
        protected int currentCameraSettingsHash = 0;

        #endregion

        #region Protected Variables - FX

        /// <summary>
        /// The maximum interval, in seconds, between sound fx when warp is engaged
        /// </summary>
        [SerializeField, Range(0.1f, 30f)] protected float maxSoundInterval = 5f;

        [Tooltip("The child particle system used to generate the inner or centre particles for the FX")]
        [SerializeField] protected ParticleSystem innerParticleSystem = null;

        /// <summary>
        /// Is the sound effects currently paused. New new sounds will play until it is unpaused.
        /// </summary>
        [SerializeField] protected bool isSoundFXPaused = false;

        /// <summary>
        /// Is the volume randomised between 50 percent of the EffectsModule default volume, and the default volume?
        /// </summary>
        [SerializeField] protected bool isSoundIntervalRandomised = true;

        [Tooltip("The child particle system used to generate the outer particles for the FX")]
        [SerializeField] protected ParticleSystem outerParticleSystem = null;

        /// <summary>
        /// The local space relative offset from the ship used when instantiating Sound FX.
        /// </summary>
        [SerializeField] protected Vector3 soundFXOffset = Vector3.zero;

        /// <summary>
        /// A set of SoundFX that are randomly selected while warp is engaged.
        /// </summary>
        [SerializeField] protected SSCSoundFXSet sscSoundFXSet = null;
        
        protected int[] soundEffectsPrefabIDs = null;

        protected float soundFXIntervalTimer = 0;

        #endregion

        #region Protected and Public Variables - Editor

        [SerializeField] protected int selectedTabInt = 0;
        [HideInInspector] public bool allowRepaint = false;

        #endregion

        #region Public Delegates

        #endregion

        #region Protected Initialise Methods

        // Use this for initialization
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Update Methods

        private void Update()
        {
            if (isWarpEngaged)
            {
                float dTime = Time.deltaTime;
                warpEngagedTimer += dTime;

                if (maxWarpDuration > 0f && warpEngagedTimer >= maxWarpDuration)
                {
                    DisengageWarp();
                }
                else
                {
                    UpdateParticleSystems(dTime);
                    UpdateSound(dTime);
                }
            }
        }

        private void FixedUpdate()
        {
            if (isWarpEngaged)
            {
                float dTime = Time.fixedDeltaTime;
                bool isMoveShip = false;

                UpdateRollShip(dTime, ref isMoveShip);
                UpdatePitchShip(dTime, ref isMoveShip);
                UpdateShakeShip(dTime, ref isMoveShip);

                if (isMoveShip) { MoveShip(); }
            }
        }

        #endregion

        #region Protected Virtual Methods - General

        protected virtual void UpdateRenderSettings()
        {
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
        }

        #endregion

        #region Protected virtual Methods - Ship

        /// <summary>
        /// Attempt to configure the ship so that warp actions can be applied to it.
        /// </summary>
        protected virtual bool ConfigureShipForWarp()
        {
            bool isConfigured = false;

            if (shipControlModule == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] SampleWarpFX.ConfigureShipForWarp - no ship defined");
                #endif
            }
            else if (!shipControlModule.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] SampleWarpFX.ConfigureShipForWarp - " + shipControlModule.name + " is not initialised");
                #endif
            }
            else
            {
                ship = shipControlModule.shipInstance;

                if (shipRBody == null) { shipRBody = shipControlModule.ShipRigidbody; }

                numThrusters = ship.thrusterList.Count;

                // Save and turn off gravity
                savedGravitationalAcceleration = ship.gravitationalAcceleration;
                ship.gravitationalAcceleration = 0f;

                // Save and enable thrusters if required
                savedIsThrusterFXStationary = ship.isThrusterFXStationary;
                ship.isThrusterFXStationary = shipForwardThrust > 0f;

                StopShipMovement();

                numFwdThrusters = 0;

                if (shipForwardThrust > 0f)
                {
                    // Count the forward thrusters and record some of their settings
                    for (int thIdx = 0; thIdx < numThrusters; thIdx++)
                    {
                        Thruster thruster = ship.thrusterList[thIdx];

                        // Remember the setting so that it can be restored when disengaging warp.
                        isMinEffectsAlwaysOnList.Add(thruster.isMinEffectsAlwaysOn);

                        // forceUse: 1 = Forwards
                        if (thruster.forceUse == 1)
                        {
                            numFwdThrusters++;

                            // Enable the forward thruster fx to fire while the ship is not moving
                            thruster.isMinEffectsAlwaysOn = true;

                            thruster.currentInput = shipForwardThrust;
                        }
                        else
                        {
                            // Turn off non-forward thrusters 
                            thruster.isMinEffectsAlwaysOn = false;
                            thruster.currentInput = 0f;
                        }
                    }
                }

                shipOriginPos = shipRBody.position;
                shipOriginRot = shipRBody.rotation;

                isSavedShipSettings = true;
                isConfigured = true;
            }

            return isConfigured;
        }

        /// <summary>
        /// Get a random shake interval, in seconds.
        /// </summary>
        /// <returns></returns>
        protected virtual float GetShakeInterval()
        {
            if (isInitialised)
            {
                return sscRandom.Range(minShakeInterval, maxShakeInterval);
            }
            else { return 2f; }
        }

        /// <summary>
        /// Get a random pitch down angle in degrees.
        /// </summary>
        /// <returns></returns>
        protected virtual float GetShipPitchDownAngle()
        {
            if (isInitialised && maxShipPitchDown > 0f)
            {
                return sscRandom.Range(maxShipPitchDown * 0.1f, maxShipPitchDown);
            }
            else { return 0f; }
        }

        /// <summary>
        /// Get a random pitch duration in seconds
        /// </summary>
        /// <returns></returns>
        protected virtual float GetShipPitchTargetDuration()
        {
            if (isInitialised && maxShipPitchDuration > 0f)
            {
                return sscRandom.Range(maxShipPitchDuration * 0.5f, maxShipPitchDuration);
            }
            else { return 0f; }
        }

        /// <summary>
        /// Get a random pitch up angle in degrees.
        /// </summary>
        /// <returns></returns>
        protected virtual float GetShipPitchUpAngle()
        {
            if (isInitialised && maxShipPitchUp > 0f)
            {
                return sscRandom.Range(maxShipPitchUp * 0.1f, maxShipPitchUp);
            }
            else { return 0f; }
        }

        /// <summary>
        /// Get a random roll angle in degrees.
        /// </summary>
        /// <returns></returns>
        protected virtual float GetShipRollTargetAngle()
        {
            if (isInitialised && maxShipRollAngle > 0f)
            {
                return sscRandom.Range(maxShipRollAngle * 0.1f, maxShipRollAngle);
            }
            else { return 0f; }
        }

        /// <summary>
        /// Get a random roll duration in seconds
        /// </summary>
        /// <returns></returns>
        protected virtual float GetShipRollTargetDuration()
        {
            if (isInitialised && maxShipRollDuration > 0f)
            {
                return sscRandom.Range(maxShipRollDuration * 0.5f, maxShipRollDuration);
            }
            else { return 0f; }
        }

        /// <summary>
        /// Attempt to update the ship's rigidbody position and rotation
        /// </summary>
        protected virtual void MoveShip()
        {
            if (isInitialised)
            {
                // When the ship is kinematic, we need to move the RigidBody.
                shipRBody.MovePosition(shipOriginPos + (currentShipRot * currentShipPosOffset));
                shipRBody.MoveRotation(currentShipRot);
            }
        }

        /// <summary>
        /// Attempt to return ship settings to pre-warp values
        /// </summary>
        protected virtual void RestoreShipSettings()
        {
            if (isInitialised && isSavedShipSettings && shipControlModule != null)
            {
                ship.gravitationalAcceleration = savedGravitationalAcceleration;
                ship.isThrusterFXStationary = savedIsThrusterFXStationary;

                // Restore the thruster settings
                if (shipForwardThrust > 0f)
                {
                    // Restore the original settings for the forward thrusters
                    for (int thIdx = 0; thIdx < numThrusters; thIdx++)
                    {
                        Thruster thruster = ship.thrusterList[thIdx];

                        // Restore the original settings
                        thruster.isMinEffectsAlwaysOn = isMinEffectsAlwaysOnList[thIdx];

                        thruster.currentInput = 0f;
                    }
                }

                if (isPlayerShip && playerInputModule != null)
                {
                    if (savedIsPlayerInputEnabled)
                    {
                        playerInputModule.EnableInput();
                        savedIsPlayerInputEnabled = false;
                    }
                }

                // Return ship to original position
                shipRBody.MovePosition(shipOriginPos);
                shipRBody.MoveRotation(shipOriginRot);

                isSavedShipSettings = false;
            }
        }

        /// <summary>
        /// This is what moves the ship while it is shaking.
        /// </summary>
        /// <param name="deltaTime"></param>
        protected virtual void ShipVibration(float deltaTime)
        {
            //Vector3 shipCurrentPos = shipRBody.position;
            //Quaternion shipCurrentRot = shipRBody.rotation;

            // We will always loose the first frame but that's ok as we
            // don't want to update the rigidbody just to change it again in the
            // same frame when StopShipShake() is called.

            float timeFactor = 1f;

            // Reduce strength over time of the shake
            timeFactor = (shakeDuration - (shakeDuration - shakeShipTimer)) / shakeDuration;

            // Set the minimum to be 50% of the shake strength
            timeFactor = 0.5f + (timeFactor * 0.5f);

            currentShipPosOffset = UnityEngine.Random.insideUnitCircle * (shakeStrength * timeFactor);
        }

        /// <summary>
        /// Attempt to start shaking the ship
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="strength"></param>
        protected virtual void ShakeShip(float duration, float strength)
        {
            if (isWarpEngaged && duration > 0f && strength > 0f)
            {
                isShaking = true;
                shakeStrength = strength;
                shakeDuration = duration;
                shakeShipTimer = duration;
                shakeIntervalTimer = 0f;
            }
            else
            {
                StopShipShake();
            }
        }

        /// <summary>
        /// Shake the ship after initial delay in seconds, with the
        /// current maxShakeDuration and maxShakeStrength.
        /// </summary>
        /// <param name="delayTime"></param>
        public void ShakeShipDelayed(float delayTime)
        {
            if (delayTime > 0f)
            {
                Invoke("ShakeShip", delayTime);
            }
            else { ShakeShip(maxShakeDuration, maxShakeStrength); }
        }

        /// <summary>
        /// Stop the ship from shaking
        /// </summary>
        protected virtual void StopShipShake()
        {
            isShaking = false;
            shakeShipTimer = 0f;
        }

        /// <summary>
        /// Pitch the ship up or down using the pitch timer.
        /// </summary>
        /// <param name="dTime"></param>
        /// <param name="isMoveShip"></param>
        protected virtual void UpdatePitchShip(float dTime, ref bool isMoveShip)
        {
            if (targetShipPitchAngle != 0f && (maxShipPitchDown > 0f || maxShipPitchUp > 0f))
            {
                currentShipPitchAngle = Mathf.Lerp(previousShipPitchAngle, targetShipPitchAngle, shipPitchCurve.Evaluate((targetShipPitchDuration - shipPitchTimer) / targetShipPitchDuration));

                // Rotate around ship local right axis (pitch)
                currentShipRot = (isMoveShip ? currentShipRot : shipOriginRot) * Quaternion.AngleAxis(currentShipPitchAngle, Vector3.right);

                shipPitchTimer -= dTime;

                // Pitch Down
                if (targetShipPitchAngle > 0f)
                {
                    // If pitching down and is almost at target, start pitching up
                    if (currentShipPitchAngle > 0f && shipPitchTimer <= 0f)
                    {
                        // Get a -ve target angle, to start pitching up
                        targetShipPitchAngle = maxShipPitchUp > 0f ? -GetShipPitchUpAngle() : 0f;
                        targetShipPitchDuration = GetShipPitchTargetDuration();
                        shipPitchTimer = targetShipPitchDuration;
                        previousShipPitchAngle = currentShipPitchAngle;
                    }
                }
                // Pitch Up
                else
                {
                    // If pitching up and is almost at target, start pitching down
                    if (currentShipPitchAngle < 0f && shipPitchTimer <= 0f)
                    {
                        // Get a +ve target angle, to start pitching down
                        targetShipPitchAngle = maxShipPitchDown > 0f ? GetShipPitchDownAngle() : 0f;
                        targetShipPitchDuration = GetShipPitchTargetDuration();
                        shipPitchTimer = targetShipPitchDuration;
                        previousShipPitchAngle = currentShipPitchAngle;
                    }
                }

                isMoveShip = true;
            }
        }

        /// <summary>
        /// Roll the ship left or right using the roll timer
        /// </summary>
        /// <param name="dTime"></param>
        /// <param name="isMoveShip"></param>
        protected virtual void UpdateRollShip(float dTime, ref bool isMoveShip)
        {
            if (maxShipRollAngle > 0f && targetShipRollAngle != 0f)
            {
                currentShipRollAngle = Mathf.Lerp(previousShipRollAngle, targetShipRollAngle, shipRollCurve.Evaluate((targetShipRollDuration - shipRollTimer) / targetShipRollDuration));

                // Rotate around ship local forward axis (roll)
                currentShipRot = shipOriginRot * Quaternion.AngleAxis(currentShipRollAngle, Vector3.forward);

                shipRollTimer -= dTime;

                // Roll left
                if (targetShipRollAngle > 0f)
                {
                    // If rolling left and is almost at target, start rolling right
                    if (currentShipRollAngle > 0f && shipRollTimer <=0f)
                    {
                        // Get a -ve target angle, to start rolling right
                        targetShipRollAngle = -GetShipRollTargetAngle();
                        targetShipRollDuration = GetShipRollTargetDuration();
                        shipRollTimer = targetShipRollDuration;
                        previousShipRollAngle = currentShipRollAngle;
                    }
                }
                // Roll right
                else
                {
                    // If rolling right and is almost at target, start rolling left
                    if (currentShipRollAngle < 0f && shipRollTimer <= 0f)
                    {
                        // Get a +ve target angle, to start rolling left
                        targetShipRollAngle = GetShipRollTargetAngle();
                        targetShipRollDuration = GetShipRollTargetDuration();
                        shipRollTimer = targetShipRollDuration;
                        previousShipRollAngle = currentShipRollAngle;
                    }
                }

                isMoveShip = true;
            }
        }

        /// <summary>
        /// Shake the ship using the interval timer
        /// </summary>
        /// <param name="dTime"></param>
        /// <param name="isMoveShip"></param>
        protected virtual void UpdateShakeShip(float dTime, ref bool isMoveShip)
        {
            if (isShaking)
            {
                // Check if shaking should stop
                shakeShipTimer -= dTime;
                if (shakeShipTimer <= 0f)
                {
                    StopShipShake();
                    shakeIntervalTimer = GetShakeInterval();
                }
                else
                {
                    ShipVibration(dTime);
                    isMoveShip = true;
                }
            }
            // Are we waiting for the next shake incident to occur?
            else if (shakeIntervalTimer > 0f)
            {
                shakeIntervalTimer -= dTime;

                if (shakeIntervalTimer <= 0f)
                {
                    // Start the next shake incident
                    shakeIntervalTimer = 0f;
                    UpdateShakeDuration();
                    UpdateShakeStrength();
                    ShakeShip();
                }
            }
        }

        /// <summary>
        /// Randomly set a new shake duration
        /// </summary>
        protected virtual void UpdateShakeDuration()
        {
            if (isInitialised)
            {
                shakeDuration = sscRandom.Range(maxShakeDuration * 0.1f, maxShakeDuration);
            }
        }

        /// <summary>
        /// Randomly set a new shake strength
        /// </summary>
        protected virtual void UpdateShakeStrength()
        {
            if (isInitialised)
            {
                shakeStrength = sscRandom.Range(maxShakeStrength * 0.1f, maxShakeStrength);
            }
        }

        #endregion

        #region Protected Virtual Methods - Camera
        
        protected virtual bool ConfigureCamera (Camera camera)
        {
            bool isSuccessful = false;

            if (camera != null)
            {
                camera.clearFlags = envAmbientSource == EnvAmbientSource.Skybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
                camera.backgroundColor = nightSkyColour;

                camera1 = camera;

                isSuccessful = true;
            }

            return isSuccessful;
        }

        #endregion

        #region Protected Virtual Methods - FX

        /// <summary>
        /// Attempt to enable or disable the Particle FX
        /// </summary>
        /// <param name="isEnable"></param>
        protected virtual void EnableOrDisableParticleFX(bool isEnable)
        {
            if (isInitialised)
            {
                if (innerParticleSystem != null)
                {
                    if (isEnable)
                    {
                        innerParticleSystem.gameObject.SetActive(true);
                    }
                    else if (innerParticleSystem.gameObject.activeSelf)
                    {
                        // Stop emmitting particles before disabling
                        if (innerParticleSystem.IsAlive(true))
                        {
                            innerParticleSystem.Stop(true);
                        }

                        innerParticleSystem.gameObject.SetActive(false);
                    }
                }

                if (outerParticleSystem != null)
                {
                    if (isEnable)
                    {
                        outerParticleSystem.gameObject.SetActive(true);
                    }
                    else if (outerParticleSystem.gameObject.activeSelf)
                    {
                        // Stop emmitting particles before disabling
                        if (outerParticleSystem.IsAlive(true))
                        {
                            outerParticleSystem.Stop(true);
                        }

                        outerParticleSystem.gameObject.SetActive(false);
                    }
                }
            }
        }


        /// <summary>
        /// Get a random sound fx interval, in seconds.
        /// </summary>
        /// <returns></returns>
        protected virtual float GetSoundFXInterval()
        {
            if (isInitialised)
            {
                return sscRandom.Range(maxSoundInterval * 0.2f, maxSoundInterval);
            }
            else { return 5f; }
        }

        /// <summary>
        /// Attempt to pause or unpause the Sound FX feature.
        /// Currently this does not stop any sound FX that have been instantiated.
        /// </summary>
        /// <param name="isPause"></param>
        protected virtual void PauseOrUnPauseSoundFX(bool isPause)
        {
            if (isPause)
            {
                // A possible enhancement is to actual stop any current sound FX
                // from playing.
                if (!isSoundFXPaused)
                {
                    isSoundFXPaused = true;
                }
            }
            else
            {
                isSoundFXPaused = false;
            }
        }

        /// <summary>
        /// Attempt to play a random sound and return the estimated duration for that sound.
        /// </summary>
        protected virtual float PlaySoundFX()
        {
            float estimatedDuration = 0f;

            if (isInitialised)
            {
                int numEffects = sscSoundFXSet != null ? sscSoundFXSet.NumberOfEffects : 0;

                if (numEffects > 0)
                {
                    int prefabIDArrayIndex = sscRandom.Range(0, numEffects-1);
                    int effectPrefabID = soundEffectsPrefabIDs[prefabIDArrayIndex];

                    if (effectPrefabID == SSCManager.NoPrefabID)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("ShipWarpModule.PlaySoundFX() - item " + (prefabIDArrayIndex+1) + " in " + sscSoundFXSet.name + " does not have a template registered with SSCManager.");
                        #endif
                    }
                    else
                    {
                        // Get the original prefab used to create the pool of sound fx
                        EffectsModule effectsModule = sscManager.GetEffectsObjectPrefab(effectPrefabID);

                        // Calculate the volume to use for this sound FX.
                        float defaultVolume = effectsModule.defaultVolume;
                        float soundFXVolume = isSoundIntervalRandomised ? sscRandom.Range(defaultVolume * 0.5f, defaultVolume) : defaultVolume;

                        // As the sound should be pooled, assume the despawn time has been configured to match the audio clip length
                        estimatedDuration = effectsModule.despawnTime;

                        InstantiateSoundFXParameters sfxParms = new InstantiateSoundFXParameters()
                        {
                            effectsObjectPrefabID = effectPrefabID,
                            position = shipOriginPos + (currentShipRot * (currentShipPosOffset + soundFXOffset)),
                            volume = soundFXVolume
                        };

                        sscManager.InstantiateSoundFX(sfxParms, null);
                    }
                }
            }

            return estimatedDuration;
        }

        /// <summary>
        /// Override this method to update the particle systems at runtime during the Update() loop
        /// </summary>
        /// <param name="dTime"></param>
        protected virtual void UpdateParticleSystems(float dTime)
        {

        }

        /// <summary>
        /// If required, count down until the next sound should be played,
        /// then play it.
        /// </summary>
        /// <param name="dTime"></param>
        protected virtual void UpdateSound(float dTime)
        {
            if (!isSoundFXPaused && soundFXIntervalTimer > 0f)
            {
                soundFXIntervalTimer -= dTime;

                if (soundFXIntervalTimer <= 0f)
                {
                    float soundDuration = PlaySoundFX();

                    soundFXIntervalTimer = GetSoundFXInterval() + soundDuration;
                }
            }
        }

        /// <summary>
        /// Ensure the particlesystem is a child of the warp module gameobject.
        /// Deactivate it when verified.
        /// </summary>
        /// <param name="particleSystem"></param>
        /// <returns></returns>
        protected virtual bool VerifyParticleSystem(ParticleSystem particleSystem)
        {
            bool isVerified = false;

            if (particleSystem != null && particleSystem.transform.IsChildOf(transform))
            {
                particleSystem.gameObject.SetActive(false);

                isVerified = true;
            }

            return isVerified;
        }

        #endregion

        #region Protected and Internal Methods - General

        #endregion

        #region Protected and Internal Methods - Ship

        /// <summary>
        /// Get the number of forward thrusters on the ship
        /// </summary>
        /// <returns></returns>
        protected int GetNumForwardThrusters()
        {
            int numFwdThrusters = 0;

            if (isInitialised && numThrusters > 0)
            {
                for (int thIdx = 0; thIdx < numThrusters; thIdx++)
                {
                    // forceUse: 1 = Forwards
                    if (ship.thrusterList[thIdx].forceUse == 1)
                    {
                        numFwdThrusters++;
                    }
                }
            }

            return numFwdThrusters;
        }

        /// <summary>
        /// Shake the ship for maxShakeDuration seconds which maxShakeStrength or force.
        /// If the ship is not configured for warp or the duration and/or strength are 0 or less,
        /// StopShipShake() will be automatically called and the inputs ignored.
        /// </summary>
        protected void ShakeShip()
        {
            ShakeShip(maxShakeDuration, maxShakeStrength);
        }

        #endregion

        #region Protected and Internal Methods - Camera

        /// <summary>
        /// Attempt to get the next zero-based index in the list of ship camera set.
        /// NOTE: The slot might not contain a valid ShipCameraSettings ScriptableObject.
        /// </summary>
        /// <returns></returns>
        protected int GetNextCameraSettingsIndex()
        {
            int nextSettingIndex = -1;

            if (isInitialised)
            {
                int numSettingSlots = shipCameraSettingsList.Count;

                if (numSettingSlots > 0)
                {
                    // If not set yet, start in the first slot
                    if (currentCameraSettingsIndex < 0) { nextSettingIndex = 0; }
                    else
                    {
                        nextSettingIndex = (currentCameraSettingsIndex + 1) % numSettingSlots;
                    }
                }
            }

            return nextSettingIndex;
        }

        #endregion

        #region Protected and Internal Methods - FX

        /// <summary>
        /// Attempt to create all (sound) EffectsModule pools with the SSCManager in the scene.
        /// </summary>
        protected void ReinitialiseSoundFX()
        {
            int numEffects = sscSoundFXSet != null ? sscSoundFXSet.NumberOfEffects : 0;

            if (numEffects > 0)
            {
                soundEffectsPrefabIDs = new int[numEffects];

                if (sscManager == null)
                {
                    //Debug.Log("[DEBUG] ShipWarpModule.ReinitialiseSoundFX() - GetorCreateManager...");
                    sscManager = SSCManager.GetOrCreateManager(gameObject.scene.handle);
                }

                if (sscManager != null)
                {
                    sscManager.CreateEffectsPools(sscSoundFXSet, soundEffectsPrefabIDs);
                }
            }
            else
            {
                soundEffectsPrefabIDs = null;
            }
        }

        #endregion

        #region Events

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Attempt to disengage warp fx for the ship
        /// </summary>
        public virtual void DisengageWarp()
        {
            if (isWarpEngaged)
            {
                if (onPreDisengageWarp != null) { onPreDisengageWarp.Invoke(); }

                warpEngagedTimer = 0f;

                CancelInvoke("ShakeShip");

                EnableOrDisableParticleFX(false);
                RestoreShipSettings();

                shipControlModule.StopThrusterEffects();

                shakeIntervalTimer = 0f;
                shipRollTimer = 0f;
                soundFXIntervalTimer = 0f;
                isWarpEngaged = false;

                if (onPostDisengageWarp != null) { onPostDisengageWarp.Invoke(); }
            }
        }

        /// <summary>
        /// Attempt to engage warp fx for the ship
        /// </summary>
        public virtual void EngageWarp()
        {
            if (!isWarpEngaged)
            {
                if (onPreEngageWarp != null) { onPreEngageWarp.Invoke(); }

                if (ConfigureShipForWarp())
                {
                    // Update the position and rotation of the warp FX, based on the position of the ship.
                    transform.SetPositionAndRotation(ship.TransformPosition + (ship.TransformRotation * offsetFromShip), ship.TransformRotation);

                    EnableOrDisableParticleFX(true);

                    UpdateShakeDuration();
                    UpdateShakeStrength();

                    shakeIntervalTimer = 0f;
                    warpEngagedTimer = 0f;

                    targetShipPitchAngle = maxShipPitchDown > 0f ? GetShipPitchDownAngle() : GetShipPitchUpAngle();
                    // Start pitching one way from level flight. If only up OR down is used, the duration will be full duration (rather than half)
                    targetShipPitchDuration = GetShipPitchTargetDuration() * (maxShipPitchDown > 0f && maxShipPitchUp > 0f ? 0.5f : 1f);
                    previousShipPitchAngle = 0f;
                    shipPitchTimer = targetShipPitchDuration;

                    targetShipRollAngle = GetShipRollTargetAngle();
                    currentShipRot = shipOriginRot;

                    // Start by rolling one way from level flight.
                    targetShipRollDuration = GetShipRollTargetDuration() * 0.5f;
                    shipRollTimer = targetShipRollDuration;
                    previousShipRollAngle = 0f;

                    if (shipCameraModule != null)
                    {
                        shipCameraModule.EnableCamera();
                    }

                    if (isApplyCameraSettingsOnEngage)
                    {
                        CycleCameraSettings();
                    }

                    ShakeShipDelayed(minShakeInterval);

                    soundFXIntervalTimer = GetSoundFXInterval();

                    isWarpEngaged = true;

                    if (onPostEngageWarp != null) { onPostEngageWarp.Invoke(); }
                }
            }
        }

        /// <summary>
        /// Initialise the Warp Effect
        /// </summary>
        public virtual void Initialise()
        {
            if (isInitialised) { return; }
            else
            {
                UpdateRenderSettings();

                if (shipCameraModule == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("[ERROR] SampleWarpFX - ship camera module is not setup");
                    #endif
                }
                else if (ConfigureCamera(shipCameraModule.GetCamera1) && VerifyParticleSystem(innerParticleSystem) && VerifyParticleSystem(outerParticleSystem))
                {
                    DisableParticleFX();

                    // If the ship hasn't been initialised yet, do it now.
                    if (shipControlModule != null && !shipControlModule.IsInitialised)
                    {
                        shipControlModule.InitialiseShip();
                    }

                    sscRandom = new SSCRandom();
                    // Set the seed to an arbitary prime number (but it could be anything really)
                    sscRandom.SetSeed(16729);

                    ReinitialiseSoundFX();

                    isInitialised = true;
                }
            }         
        }

        /// <summary>
        /// Update or create pooled sound effects with SSCManager in the scene.
        /// </summary>
        public virtual void RefreshManager()
        {
            ReinitialiseSoundFX();
        }

        /// <summary>
        /// Attempt to set the environment ambient source (0: Colour, 1: Gradient, 2: Skybox)
        /// </summary>
        /// <param name="envAmbientSourceInt"></param>
        public void SetEnvironmentAmbientSource (int envAmbientSourceInt)
        {
            if (envAmbientSource >= 0 && envAmbientSourceInt < 3)
            {
                SetEnvironmentAmbientSource((EnvAmbientSource)envAmbientSourceInt);
            }
        }

        /// <summary>
        /// Set the environment ambient source (Colour, Gradient or Skybox).
        /// Will also refresh camera settings.
        /// </summary>
        /// <param name="newEnvAmbientSource"></param>
        public virtual void SetEnvironmentAmbientSource (EnvAmbientSource newEnvAmbientSource)
        {
            envAmbientSource = newEnvAmbientSource;

            UpdateRenderSettings();

            if (camera1 != null)
            {
                ConfigureCamera(camera1);
            }
        }

        /// <summary>
        /// Set the time, in seconds, that warp will automatically disengage. If it is set to 0,
        /// it will not automatically disengage.
        /// </summary>
        /// <param name="newMaxWarpDuration"></param>
        public void SetMaxWarpDuration (float newMaxWarpDuration)
        {
            if (newMaxWarpDuration >= 0)
            {
                maxWarpDuration = newMaxWarpDuration;
            }
        }

        /// <summary>
        /// Attempt to stop the ship from moving.
        /// </summary>
        public virtual void StopShipMovement()
        {
            if (shipControlModule != null && shipControlModule.IsInitialised)
            {
                // If this is a player ship, prevent player input
                if (isPlayerShip || playerInputModule != null || shipControlModule.TryGetComponent(out playerInputModule))
                {
                    isPlayerShip = true;

                    // Save and disable player input (optionally allow custom player inputs)
                    // Check if it has already been saved when input is enabled.
                    if (!savedIsPlayerInputEnabled)
                    {
                        savedIsPlayerInputEnabled = playerInputModule.IsInputEnabled;
                    }

                    if (savedIsPlayerInputEnabled)
                    {
                        playerInputModule.DisableInput(allowCustomInputs);
                    }
                }

                shipControlModule.shipInstance.StopBoost();
                shipControlModule.DisableShipMovement();

                // Re-enable collision detection so that particles don't enter the ship
                shipControlModule.ShipRigidbody.detectCollisions = true;
            }
        }

        /// <summary>
        /// Attempt to toggle warp on and off.
        /// </summary>
        public void ToggleWarp()
        {
            if (isInitialised)
            {
                if (isWarpEngaged) { DisengageWarp(); }
                else { EngageWarp(); }
            }
        }

        #endregion

        #region Public API Methods - Ship

        /// <summary>
        /// Return the default pitch animation curve
        /// </summary>
        /// <returns></returns>
        public static AnimationCurve GetDefaultPitchCurve()
        {
            return AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        /// <summary>
        /// Return the default roll animation curve
        /// </summary>
        /// <returns></returns>
        public static AnimationCurve GetDefaultRollCurve()
        {
            return AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        /// <summary>
        /// Attempt to set the maxShakeInterval
        /// </summary>
        /// <param name="newMaxShakeInterval"></param>
        public void SetMaxShakeInterval (float newMaxShakeInterval)
        {
            if (newMaxShakeInterval >= 0.1f)
            {
                maxShakeInterval = newMaxShakeInterval;
            }
        }

        /// <summary>
        /// Attempt to set the minShakeInterval
        /// </summary>
        /// <param name="newMinShakeInterval"></param>
        public void SetMinShakeInterval (float newMinShakeInterval)
        {
            if (newMinShakeInterval >= 0.1f)
            {
                minShakeInterval = newMinShakeInterval;
            }
        }

        /// <summary>
        /// Attempt to set the maxShakeStrength
        /// </summary>
        /// <param name="newMaxShakeStrength"></param>
        public void SetMaxShakeStrength (float newMaxShakeStrength)
        {
            if (newMaxShakeStrength >= 0.005f)
            {
                maxShakeStrength = newMaxShakeStrength;
            }
        }

        /// <summary>
        /// Attempt to set the maxShakeDuration
        /// </summary>
        /// <param name="newMaxShakeDuration"></param>
        public void SetMaxShakeDuration (float newMaxShakeDuration)
        {
            if (newMaxShakeDuration >= 0.1f)
            {
                maxShakeDuration = newMaxShakeDuration;
            }
        }

        /// <summary>
        /// Set the ship being used with the Ship Warp Module
        /// </summary>
        /// <param name="newShipControlModule"></param>
        public void SetShipControlModule (ShipControlModule newShipControlModule)
        {
            shipControlModule = newShipControlModule;
        }

        /// <summary>
        /// Attempt to set the shipPitchCurve
        /// </summary>
        /// <param name="newAnimationCurve"></param>
        public void SetShipPitchCurve (AnimationCurve newAnimationCurve)
        {
            if (newAnimationCurve == null)
            {
                shipPitchCurve = GetDefaultPitchCurve();
            }
            else
            {
                shipPitchCurve = newAnimationCurve;
            }
        }

        /// <summary>
        /// Attempt to set the maxShipPitchDown angle
        /// </summary>
        /// <param name="newMaxShipPitchDown"></param>
        public void SetMaxShipPitchDown (float newMaxShipPitchDown)
        {
            if (newMaxShipPitchDown >= 0f && newMaxShipPitchDown < 90f)
            {
                maxShipPitchDown = newMaxShipPitchDown;
            }
        }

        /// <summary>
        /// Attempt to set the maximum time, in seconds, the ship will take to pitch up and down
        /// </summary>
        /// <param name="newMaxShipPitchDuration"></param>
        public void SetMaxShipPitchDuration (float newMaxShipPitchDuration)
        {
            if (newMaxShipPitchDuration >= 0f)
            {
                maxShipPitchDuration = newMaxShipPitchDuration;
            }
        }

        /// <summary>
        /// Attempt to set the maxShipPitchUp angle
        /// </summary>
        /// <param name="newMaxShipPitchUp"></param>
        public void SetMaxShipPitchUp (float newMaxShipPitchUp)
        {
            if (newMaxShipPitchUp >= 0f && newMaxShipPitchUp < 90f)
            {
                maxShipPitchUp = newMaxShipPitchUp;
            }
        }

        /// <summary>
        /// Attempt to set the maxShipRollAngle
        /// </summary>
        /// <param name="newMaxShipPitchDown"></param>
        public void SetMaxShipRollAngle (float newMaxShipRollAngle)
        {
            if (newMaxShipRollAngle >= 0f && newMaxShipRollAngle < 90f)
            {
                maxShipRollAngle = newMaxShipRollAngle;
            }
        }

        /// <summary>
        /// Attempt to set the maxShipDuration
        /// </summary>
        /// <param name="newMaxShipRollDuration"></param>
        public void SetMaxShipRollDuration (float newMaxShipRollDuration)
        {
            if (newMaxShipRollDuration >= 0f)
            {
                maxShipRollDuration = newMaxShipRollDuration;
            }
        }

        /// <summary>
        /// Attempt to set the shipRollCurve
        /// </summary>
        /// <param name="newAnimationCurve"></param>
        public void SetShipRollCurve (AnimationCurve newAnimationCurve)
        {
            if (newAnimationCurve == null)
            {
                shipRollCurve = GetDefaultRollCurve();
            }
            else
            {
                shipRollCurve = newAnimationCurve;
            }
        }

        #endregion

        #region Public API Methods - Camera

        /// <summary>
        /// Attempt to cycle through a list of camera settings.
        /// It will skip over any empty settings slots and will
        /// not apply the same settings twice in a row.
        /// </summary>
        public void CycleCameraSettings()
        {
            if (isInitialised && shipCameraModule != null)
            {
                int numCamSettings = shipCameraSettingsList.Count;

                int prevCameraSettingsIndex = currentCameraSettingsIndex;

                // Transverse the list of settings a maximum of once
                for (int iterations = 0; iterations < numCamSettings; iterations++)
                {
                    int settingIdx = GetNextCameraSettingsIndex();

                    // Is the index valid for the list of camera settings?
                    if (settingIdx >= 0 && settingIdx < numCamSettings)
                    {
                        currentCameraSettingsIndex = settingIdx;

                        ShipCameraSettings camSettings = shipCameraSettingsList[settingIdx];

                        if (camSettings != null)
                        {
                            // Make sure we're not just trying to re-apply the same settings
                            if (camSettings.GetHashCode() != currentCameraSettingsHash)
                            {
                                currentCameraSettingsHash = camSettings.GetHashCode();
                                shipCameraModule.ApplyCameraSettings(camSettings);

                                if (onChangeCameraSettings != null) { onChangeCameraSettings.Invoke(prevCameraSettingsIndex+1, currentCameraSettingsIndex+1, false); }
                            }
                            break;
                        }
                        else
                        {
                            // This settings slot is empty, so look for another one
                            continue;
                        }
                    }
                    else
                    {
                        // No valid setting index found, so exit the loop
                        break;
                    }
                }

                #if UNITY_EDITOR
                if (numCamSettings == 0)
                {
                    Debug.LogWarning("ShipWarpModule.CycleCameraSettings() - No Camera Settings found on the Camera Tab");
                }
                #endif
            }
        }

        /// <summary>
        /// Get the current ship camera module (if any)
        /// </summary>
        /// <returns></returns>
        public ShipCameraModule GetShipCameraModule()
        {
            return shipCameraModule;
        }

        /// <summary>
        /// Attempt to set the ship camera module used by the Warp FX
        /// </summary>
        /// <param name="newShipCameraModule"></param>
        public void SetShipCameraModule (ShipCameraModule newShipCameraModule)
        {
            shipCameraModule = newShipCameraModule;
        }

        #endregion

        #region Public API Methods - FX

        /// <summary>
        /// Attempt to disable the particle systems.
        /// </summary>
        public virtual void DisableParticleFX()
        {
            EnableOrDisableParticleFX(false);
        }

        /// <summary>
        /// Attempt to pause the sound FX feature.
        /// </summary>
        public void PauseSoundFX()
        {
            PauseOrUnPauseSoundFX(true);
        }

        /// <summary>
        /// Attempt to update the InnerParticleSystem with another ParticleSystem.
        /// </summary>
        /// <param name="newParticleSystem"></param>
        public void SetInnerParticleSystem (ParticleSystem newParticleSystem)
        {
            if (VerifyParticleSystem(newParticleSystem))
            {
                innerParticleSystem = newParticleSystem;
            }
            else { innerParticleSystem = null; }
        }

        /// <summary>
        /// Attempt to set the maxSoundInterval
        /// </summary>
        /// <param name="newMaxSoundInterval"></param>
        public void SetMaxSoundInterval (float newMaxSoundInterval)
        {
            if (newMaxSoundInterval >= 0.1f)
            {
                maxSoundInterval = newMaxSoundInterval;
            }
        }

        /// <summary>
        /// Attempt to update the OuterParticleSystem with another ParticleSystem.
        /// </summary>
        /// <param name="newParticleSystem"></param>
        public void SetOuterParticleSystem (ParticleSystem newParticleSystem)
        {
            if (VerifyParticleSystem(newParticleSystem))
            {
                outerParticleSystem = newParticleSystem;
            }
            else { outerParticleSystem = null; }
        }

        /// <summary>
        /// Attempt to update the Sound FX Set.
        /// </summary>
        /// <param name="newSoundFXSet"></param>
        public void SetSoundFXSet (SSCSoundFXSet newSoundFXSet)
        {
            if (newSoundFXSet != null)
            {
                sscSoundFXSet = newSoundFXSet;

                if (isInitialised)
                {                
                    ReinitialiseSoundFX();
                }
            }
            else
            {
                sscSoundFXSet = null;
            }
        }

        /// <summary>
        /// Attempt to unpause the sound FX feature.
        /// </summary>
        public void UnpauseSoundFX()
        {
            PauseOrUnPauseSoundFX(false);
        }

        #endregion

        #region Public API Methods - Events

        /// <summary>
        /// Call this when you wish to remove any custom event listeners, like
        /// after creating them in code and then destroying the object.
        /// You could add this to your game play OnDestroy code.
        /// </summary>
        public virtual void RemoveListeners()
        {
            if (isInitialised)
            {
                if (onChangeCameraSettings != null) { onChangeCameraSettings.RemoveAllListeners(); }
                if (onPreEngageWarp != null) { onPreEngageWarp.RemoveAllListeners(); }
                if (onPostEngageWarp != null) { onPostEngageWarp.RemoveAllListeners(); }
                if (onPreDisengageWarp != null) { onPreDisengageWarp.RemoveAllListeners(); }
                if (onPostDisengageWarp != null) { onPostDisengageWarp.RemoveAllListeners(); }
            }
        }

        #endregion
    }
}