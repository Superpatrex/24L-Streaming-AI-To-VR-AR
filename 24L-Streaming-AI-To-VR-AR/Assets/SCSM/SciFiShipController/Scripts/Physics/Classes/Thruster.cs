using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class containing data for a thruster.
    /// </summary>
    [System.Serializable]
    public class Thruster
    {
        #region Public variables

        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// The name of the thruster.
        /// </summary>
        public string name;

        /// <summary>
        /// The maximum thrust force of this thruster in newtons.
        /// </summary>
        public float maxThrust;

        /// <summary>
        /// The amount of available power being supplied to the thruster.
        /// </summary>
        [Range(0f, 1f)] public float throttle;

        /// <summary>
        /// Position of the thruster in local space relative to the pivot point of the ship. This is the position where the thrust force will be applied at.
        /// </summary>
        public Vector3 relativePosition;
        /// <summary>
        /// The local space direction of thrust provided by the thruster. If you modify this, call Initialise().
        /// </summary>
        public Vector3 thrustDirection;

        /// <summary>
        /// The current thruster input (from 0 to 1).
        /// </summary>
        public float currentInput;
        /// <summary>
        /// The use of this thruster in terms of force. This defines what inputs control the thruster. 0 - none. 1 - forwards thrust. 2 - backwards thrust. 3 - upwards thrust. 4 - downwards thrust. 5 - rightwards thrust. 6 - leftwards thrust.
        /// If you modify this, call ReinitialiseInputVariables() on the ship this thruster is attached to.
        /// </summary>
        public int forceUse;
        /// <summary>
        /// The primary use of this thruster in terms of a moment (turning force). This defines what inputs control the thruster. 0 - none. 1 - positive roll. 2 - negative roll. 3 - positive pitch. 4 - negative pitch. 5 - positive yaw. 6 - negative yaw.
        /// If you modify this, call ReinitialiseInputVariables() on the ship this thruster is attached to.
        /// </summary>
        public int primaryMomentUse;
        /// <summary>
        /// The secondary use of this thruster in terms of a moment (turning force). This defines what inputs control the thruster. 0 - none. 1 - positive roll. 2 - negative roll. 3 - positive pitch. 4 - negative pitch. 5 - positive yaw. 6 - negative yaw.
        /// If you modify this, call ReinitialiseInputVariables() on the ship this thruster is attached to.
        /// </summary>
        public int secondaryMomentUse;

        /// <summary>
        /// The index of the damage region this thruster is associated with. When the damage model of the ship is set to simple, this 
        /// is irrelevant. A negative value means it is associated with no damage region (so the thruster's performance will not be 
        /// affected by damage). When the damage model of the ship is set to progressive, a value of zero means it is 
        /// associated with the main damage region. When the damage model of the ship is set to localised, a zero or positive value
        /// indicates which damage region it is associated with (using a zero-based indexing system).
        /// </summary>
        public int damageRegionIndex;

        /// <summary>
        /// When minEffectsRate > 0 and throttle > 0 the effects fire when thruster input is 0.
        /// The limitEffectsOnY and Z settings are still honoured when this is true.
        /// </summary>
        public bool isMinEffectsAlwaysOn;

        /// <summary>
        /// The minimum (i.e. when its health reaches zero) performance level of this thruster. The performance level affects how much
        /// thrust is produced by this thruster. At a performance level of one it produces the maxThrust value. At a performance level of
        /// zero it produces no thrust.
        /// </summary>
        [Range(0f, 1f)] public float minPerformance;

        /// <summary>
        /// The starting health value of this thruster.
        /// </summary>
        public float startingHealth;

        /// <summary>
        /// The 0.0-1.0 value that indicates the minimum normalised amount of any particle or sound effects that are
        /// applied when a non-zero thrustinput is received for this thruster. Default is 0. If the full
        /// particle emission rate should be applied when any input is received, set the value to 1.0.
        /// </summary>
        [Range(0f, 1f)] public float minEffectsRate;

        /// <summary>
        /// Does the amount of throttle available affect the minEffectsRate?
        /// </summary>
        public bool isThrottleMinEffects;

        /// <summary>
        /// Limit when the effects are used for this thruster based on the speed of the
        /// ship along the local Z axis (forward or backward)
        /// </summary>
        public bool limitEffectsOnZ;

        /// <summary>
        /// The minimum speed in m/s on the local z-axis the ship must be travelling at before
        /// the effects will activate
        /// </summary>
        [Range(-5000f, 5000f)] public float minEffectsOnZ;

        /// <summary>
        /// The maximum speed in m/s on the local z-axis the ship can be travelling for the
        /// the effects to be active
        /// </summary>
        [Range(-5000f, 5000f)] public float maxEffectsOnZ;

        /// <summary>
        /// Limit when the effects are used for this thruster based on the speed of the
        /// ship along the local Y axis (up or down)
        /// </summary>
        public bool limitEffectsOnY;

        /// <summary>
        /// The minimum speed in m/s on the local y-axis the ship must be travelling at before
        /// the effects will activate
        /// </summary>
        [Range(-5000f, 5000f)] public float minEffectsOnY;

        /// <summary>
        /// The maximum speed in m/s on the local y-axis the ship can be travelling for the
        /// the effects to be active
        /// </summary>
        [Range(-5000f, 5000f)] public float maxEffectsOnY;

        /// <summary>
        /// Whether the thruster is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// Whether the thruster node is shown as selected in the scene view of the editor.
        /// </summary>
        public bool selectedInSceneView;

        /// <summary>
        /// Whether the gizmos for this thruster are shown in the scene view of the editor
        /// </summary>
        public bool showGizmosInSceneView;

        /// <summary>
        /// The number of seconds it takes for this thruster to go from minimum to maximum power.
        /// Also known as Throttle Up Time.
        /// </summary>
        [Range(0f,30f)] public float rampUpDuration;

        /// <summary>
        /// The number of seconds it takes for this thruster to go from maximum to minimum power.
        /// </summary>
        [Range(0f, 30f)] public float rampDownDuration;

        /// <summary>
        /// The amount of fuel available - range 0.0 (empty) to 100.0 (full).
        /// At runtime call either thruster.SetFuelLevel(..) or shipInstance.SetFuelLevel(..)
        /// </summary>
        [Range(0f, 100f)] public float fuelLevel;

        /// <summary>
        /// The rate fuel is consumed per second. If rate is 0, fuel is unlimited
        /// </summary>
        [Range(0f, 20f)] public float fuelBurnRate;

        /// <summary>
        /// The heat of the thruster or engine - range 0.0 (starting temp) to 100.0 (max temp).
        /// At runtime call either thruster.SetHeatLevel(..) or shipInstance.SetHeatLevel(..)
        /// </summary>
        [Range(0f, 100f)] public float heatLevel;

        /// <summary>
        /// The rate heat is added per second. If rate is 0, heat level never changes.
        /// </summary>
        [Range(0f, 20f)] public float heatUpRate;

        /// <summary>
        /// The rate heat is removed per second. This is the rate the thruster cools when not in use.
        /// </summary>
        [Range(0f, 20f)] public float heatDownRate;

        /// <summary>
        /// The heat level that the thruster will begin to overheat and start producing less thrust.
        /// </summary>
        [Range(50f, 100f)] public float overHeatThreshold;

        /// <summary>
        /// When the thruster reaches max heat level of 100, will the thruster be inoperable
        /// until it is repaired?
        /// </summary>
        public bool isBurnoutOnMaxHeat;

        #endregion

        #region Public Properties

        /// <summary>
        /// The current performance level of this thruster (determined by the Health value). The performance level affects how much 
        /// thrust is produced by this thruster. At a performance level of one it produces the maxThrust value. At a performance level 
        /// of zero it produces no thrust. The value will zero if the fuelLevel is 0.
        /// </summary>
        public float CurrentPerformance { get; private set; }

        /// <summary>
        /// Current fuel level - range 0.0 (empty) to 100.0 (full)
        /// </summary>
        public float FuelLevel { get { return fuelLevel; } }

        /// <summary>
        /// Current heat level - range 0.0 (starting temp) to 100.0 (max temp).
        /// </summary>
        public float HeatLevel { get { return heatLevel; } }

        /// <summary>
        /// The current health value of this thruster. 
        /// </summary>
        public float Health
        {
            get { return health; }
            set
            {
                // Update the health value
                health = value;

                // Update the current performance value
                UpdateThrusterPerformance();
            }
        }

        /// <summary>
        /// The normalised local space direction of thrust provided by the thruster.
        /// </summary>
        public Vector3 thrustDirectionNormalised { get; private set; }

        #endregion

        #region Private variables

        private float health;

        private float previousInput;
        private float throttleDeltaAmount;   // the amount to throttle down by
        private float startInput;           // used with 
        private float currentTargetInput;
        private float smoothTimer;
        private float rampTargetDuration;
        private bool isRampDown;

        #endregion

        #region Constructors

        // Class constructor
        public Thruster()
        {
            SetClassDefaults();
        }

        // Copy constructor
        public Thruster(Thruster thruster)
        {
            if (thruster == null) { SetClassDefaults(); }
            else
            {
                this.name = thruster.name;
                this.maxThrust = thruster.maxThrust;
                this.throttle = thruster.throttle;
                this.relativePosition = thruster.relativePosition;
                this.thrustDirection = thruster.thrustDirection;
                this.currentInput = thruster.currentInput;
                this.forceUse = thruster.forceUse;
                this.primaryMomentUse = thruster.primaryMomentUse;
                this.secondaryMomentUse = thruster.secondaryMomentUse;
                this.damageRegionIndex = thruster.damageRegionIndex;
                this.minPerformance = thruster.minPerformance;
                this.startingHealth = thruster.startingHealth;
                this.Health = thruster.Health;
                this.showInEditor = thruster.showInEditor;
                this.selectedInSceneView = thruster.selectedInSceneView;
                this.showGizmosInSceneView = thruster.showGizmosInSceneView;
                this.minEffectsRate = thruster.minEffectsRate;
                this.isThrottleMinEffects = thruster.isThrottleMinEffects;
                this.isMinEffectsAlwaysOn = thruster.isMinEffectsAlwaysOn;
                this.limitEffectsOnZ = thruster.limitEffectsOnZ;
                this.minEffectsOnZ = thruster.minEffectsOnZ;
                this.maxEffectsOnZ = thruster.maxEffectsOnZ;
                this.limitEffectsOnY = thruster.limitEffectsOnY;
                this.minEffectsOnY = thruster.minEffectsOnY;
                this.maxEffectsOnY = thruster.maxEffectsOnY;
                this.rampUpDuration = thruster.rampUpDuration;
                this.rampDownDuration = thruster.rampDownDuration;
                this.fuelLevel = thruster.fuelLevel;
                this.fuelBurnRate = thruster.fuelBurnRate;
                this.heatLevel = thruster.heatLevel;
                this.heatUpRate = thruster.heatUpRate;
                this.heatDownRate = thruster.heatDownRate;
                this.overHeatThreshold = thruster.overHeatThreshold;
                this.isBurnoutOnMaxHeat = thruster.isBurnoutOnMaxHeat;
                this.Initialise();
            }
        }

        #endregion

        #region Private or Internal Methods

        /// <summary>
        /// Reset thruster ramp up/down varibles.
        /// Used with SmoothThrusterInput(..)
        /// </summary>
        private void ResetThrusterDamping()
        {
            smoothTimer = 0f;
            rampTargetDuration = 0f;
            throttleDeltaAmount = 0f;
            currentTargetInput = 0f;
            previousInput = 0f;
            startInput = 0f;
            isRampDown = false;
        }

        /// <summary>
        /// Check if we need to burn fuel on this thruster. If there is no fuel
        /// available or the burn rate is 0, then do nothing.
        /// </summary>
        /// <param name="dTime"></param>
        internal void BurnFuel(float dTime)
        {
            if (fuelLevel > 0f && fuelBurnRate > 0f && currentInput > 0f)
            {
                // Burn fuel independently to the health level. A damaged thruster will burn the same
                // amount of fuel but produce less thrust
                SetFuelLevel(fuelLevel - (currentInput * fuelBurnRate * dTime));
            }
        }

        /// <summary>
        /// Check if we need to change the heat level on this thruster. If
        /// heat up rate is 0, then do nothing.
        /// </summary>
        /// <param name="dTime"></param>
        internal void ManageHeat (float dTime)
        {
            if (heatUpRate > 0f)
            {
                // Heat or cool thruster independently to the health level.
                if (currentInput > 0f)
                {
                    // Heating up
                    if (heatLevel < 100f)
                    {
                        SetHeatLevel(heatLevel + (currentInput * heatUpRate * dTime));
                    }
                }
                // Only cool down if not burnt out
                else if (heatDownRate > 0f && (!isBurnoutOnMaxHeat || heatLevel < 100f))
                {
                    // Cooling down
                    SetHeatLevel(heatLevel - (heatDownRate * dTime));
                }
            }
        }

        #endregion

        #region Public Non-Static Methods

        public void SetClassDefaults()
        {
            this.name = "Thruster";
            this.maxThrust = 100000f;
            this.throttle = 1f;
            this.relativePosition = Vector3.zero;
            this.thrustDirection = Vector3.forward;
            this.currentInput = 0f;
            this.forceUse = 1;
            this.primaryMomentUse = 0;
            this.secondaryMomentUse = 0;
            this.damageRegionIndex = -1;
            this.minPerformance = 0.25f;
            this.startingHealth = 100f;
            this.Health = 100f;
            this.showInEditor = true;
            this.selectedInSceneView = false;
            this.showGizmosInSceneView = true;
            this.minEffectsRate = 0f;
            this.isThrottleMinEffects = false;
            this.isMinEffectsAlwaysOn = false;
            this.limitEffectsOnZ = false;
            this.minEffectsOnZ = -5000f;
            this.maxEffectsOnZ = 5000f;
            this.limitEffectsOnY = false;
            this.minEffectsOnY = -5000f;
            this.maxEffectsOnY = 5000f;
            this.rampUpDuration = 0f;
            this.rampDownDuration = 0f;
            this.fuelLevel = 100f;
            this.fuelBurnRate = 0f;
            this.heatLevel = 0f;
            this.heatUpRate = 0f;
            this.heatDownRate = 2f;
            this.overHeatThreshold = 80f;
            this.isBurnoutOnMaxHeat = false;
            this.Initialise();
        }

        /// <summary>
        /// Initialises data for the thruster. This does some precalculation to allow for performance improvements.
        /// Call after modifying thrustDirection.
        /// </summary>
        public void Initialise()
        {
            // Calculate normalised vectors
            thrustDirectionNormalised = thrustDirection.normalized;

            // Reset thruster ramp up/down varibles
            ResetThrusterDamping();

            UpdateThrusterPerformance();
        }

        /// <summary>
        /// Is the thruster heat level at or above the over heating threshold?
        /// </summary>
        /// <returns></returns>
        public bool IsThrusterOverheating()
        {
            return heatLevel >= overHeatThreshold;
        }

        /// <summary>
        /// Reset the heat level to 0 and reset health
        /// </summary>
        public void Repair()
        {
            ResetThrusterDamping();

            heatLevel = 0f;

            // This will also update the current performance
            Health = startingHealth;
        }

        /// <summary>
        /// Set the new fuel level available to this thruster.
        /// Range 0.0 (empty) to 100.0 (full).
        /// </summary>
        /// <param name="newFuelLevel"></param>
        public void SetFuelLevel (float newFuelLevel)
        {
            if (newFuelLevel < 0f) { newFuelLevel = 0f; }
            else if (newFuelLevel > 100f) { newFuelLevel = 100f; }

            // Only update the thruster performance if we need to
            if (newFuelLevel != fuelLevel && (fuelLevel == 0f || newFuelLevel == 0f))
            {
                fuelLevel = newFuelLevel;
                UpdateThrusterPerformance();
            }
            else
            {
                fuelLevel = newFuelLevel;
            }

            if (fuelLevel == 0f) { ResetThrusterDamping(); }
        }

        /// <summary>
        /// Set the new heat level on this thruster.
        /// Range 0.0 (min) to 100.0 (max).
        /// </summary>
        /// <param name="newHeatLevel"></param>
        public void SetHeatLevel(float newHeatLevel)
        {
            if (newHeatLevel < 0f) { newHeatLevel = 0f; }
            else if (newHeatLevel > 100f) { newHeatLevel = 100f; }

            // Only update the thruster performance if we need to
            // Update when:
            // a) heatLevel will be equal or above the overheat threshold
            // b) heatLevel will fallen below the overheat threshold
            // c) AND it has changed
            if (newHeatLevel != heatLevel && (newHeatLevel >= overHeatThreshold || (newHeatLevel < overHeatThreshold && heatLevel >= overHeatThreshold)))
            {
                heatLevel = newHeatLevel;
                UpdateThrusterPerformance();
            }
            else
            {
                heatLevel = newHeatLevel;
            }

            if (heatLevel == 100f) { ResetThrusterDamping(); }
        }

        /// <summary>
        /// Smooth or dampen the thruster's currentInput value based on user settings.
        /// Throttle up and down time can be set independently for each thruster.
        /// </summary>
        /// <param name="lastFrameTime"></param>
        public void SmoothThrusterInput(float lastFrameTime)
        {
            // Clamp 0.0-1.0
            currentInput = currentInput < 0f ? 0f : currentInput > 1f ? 1f : currentInput;

            // Apply throttle level
            if (throttle < 1f && throttle >= 0f) { currentInput *= throttle; }

            // If the ramp up time is not set, simply return the target value
            if (rampUpDuration > 0f || rampDownDuration > 0f)
            {
                // Are we still trying to get to the same target value?
                if (currentInput != currentTargetInput)
                {
                    // Target value has changed
                    currentTargetInput = currentInput;
                    startInput = previousInput;
                    smoothTimer = 0f;

                    // What is the delta between original value and target value?
                    throttleDeltaAmount = currentTargetInput - previousInput;

                    // Are we ramping up or down?
                    isRampDown = throttleDeltaAmount < 0f;

                    // What is the proportional ramp up duration between original value and target value?
                    if (isRampDown)
                    {
                        throttleDeltaAmount *= -1f;
                        rampTargetDuration = throttleDeltaAmount * rampDownDuration;
                    }
                    else
                    {
                        rampTargetDuration = throttleDeltaAmount * rampUpDuration;
                    }
                }

                // Do we need to ramp up/down?
                if (smoothTimer < rampTargetDuration && rampTargetDuration >= 0f)
                {
                    smoothTimer += lastFrameTime;

                    // Get the point on the throttle up curve
                    // y = (e ^ x/a) - 1, where a = duration (in seconds) and x = elapsed time
                    float rampValue = (float)System.Math.Pow(System.Math.E, smoothTimer / rampTargetDuration) - 1f;

                    // If close to the target value, set equal to the target value
                    if (rampValue > 0.999f) { rampValue = 1f; smoothTimer = rampTargetDuration; }

                    // When throttling up/down, increase/reduce the amount by adding/subtracting the proportion to be reduced from the
                    // original throttle input amount when the timer started.
                    currentInput = isRampDown ? startInput - throttleDeltaAmount * rampValue  : startInput + throttleDeltaAmount * rampValue;

                    previousInput = currentInput;
                }
            }
        }

        /// <summary>
        /// Update the CurrentPerformance value of this thruster. It gets automatically
        /// called when the Health is changed and when fuel or heat levels change.
        /// </summary>
        public void UpdateThrusterPerformance()
        {
            // Update the current performance value
            if (fuelLevel > 0f && heatLevel < 100f)
            {  
                CurrentPerformance = health / startingHealth;
                if (heatUpRate > 0f && heatLevel >= overHeatThreshold)
                {
                    CurrentPerformance *= 1f - SSCMath.Normalise(heatLevel, overHeatThreshold, 100f);
                }
                CurrentPerformance = CurrentPerformance > minPerformance ? CurrentPerformance : minPerformance;
                CurrentPerformance = CurrentPerformance < 1f ? CurrentPerformance : 1f;
            }
            else { CurrentPerformance = 0f; }
        }

        #endregion
    }
}
