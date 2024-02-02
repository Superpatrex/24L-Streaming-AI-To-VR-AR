using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class for implementing proportional-integral-derivative (PID) controllers.
    /// d is uesd for physics-based scenarios where there could be overshoot or correction.
    /// </summary>
    [System.Serializable]
    public class S3DPIDController
    {
        #region Public Variables

        /// <summary>
        /// Proportional (P) gain.
        /// </summary>
        public float pGain;
        /// <summary>
        /// Integral (I) gain.
        /// </summary>
        public float iGain;
        /// <summary>
        /// Derivative (D) gain.
        /// </summary>
        public float dGain;
        /// <summary>
        /// Whether input over time is used for the derivative (D) term instead of proportional (P) over time.
        /// Enabling this gets rid of the effect known as "derivative kick" where changes in the target value
        /// cause huge spikes in the calculated input.
        /// </summary>
        public bool derivativeOnMeasurement;
        /// <summary>
        /// Whether limits are used for the proportional (P), integral (I) and derivative (D) components individually.
        /// </summary>
        public bool useIndividualInputLimits;

        #endregion

        #region Private Variables

        /// <summary>
        /// Proportional (P) term.
        /// </summary>
        private float proportional;
        /// <summary>
        /// Integral (I) term.
        /// </summary>
        private float integral;
        /// <summary>
        /// Derivative (D) term.
        /// </summary>
        private float derivative;
        /// <summary>
        /// Proportional (P) term multiplied by p-gain value.
        /// </summary>
        private float proportionalTimesPGain;
        /// <summary>
        /// Integral (I) term multiplied by i-gain value.
        /// </summary>
        private float integralTimesIGain;
        /// <summary>
        /// Derivative (D) term multiplied by d-gain value.
        /// </summary>
        private float derivativeTimesDGain;
        /// <summary>
        /// The previous value of the proportional (P) term.
        /// </summary>
        private float previousProportional;
        /// <summary>
        /// The previous system value.
        /// </summary>
        private float previousValue;
        /// <summary>
        /// Whether this is the first update since this the system was last initialised or reset.
        /// </summary>
        private bool firstUpdate;
        /// <summary>
        /// The minimum allowed input value.
        /// </summary>
        private float minInput;
        /// <summary>
        /// The maximum allowed input value.
        /// </summary>
        private float maxInput;

        /// <summary>
        /// The minimum allowed proportional times p-gain value.
        /// </summary>
        private float minProportionalInput;
        /// <summary>
        /// The maximum allowed proportional times p-gain value.
        /// </summary>
        private float maxProportionalInput;
        /// <summary>
        /// The minimum allowed integral times i-gain value.
        /// </summary>
        private float minIntegralInput;
        /// <summary>
        /// The maximum allowed integral times i-gain value.
        /// </summary>
        private float maxIntegralInput;
        /// <summary>
        /// The minimum allowed derivative times d-gain value.
        /// </summary>
        private float minDerivativeInput;
        /// <summary>
        /// The maximum allowed derivative times d-gain value.
        /// </summary>
        private float maxDerivativeInput;

        private float requiredInput;

        #endregion

        #region Constructors

        // Class Constructor
        public S3DPIDController(float kp, float ki, float kd)
        {
            this.pGain = kp;
            this.iGain = ki;
            this.dGain = kd;
            this.derivativeOnMeasurement = false;
            this.useIndividualInputLimits = false;
            this.firstUpdate = true;
            this.minInput = Mathf.NegativeInfinity;
            this.maxInput = Mathf.Infinity;
            this.minProportionalInput = Mathf.NegativeInfinity;
            this.maxProportionalInput = Mathf.Infinity;
            this.minIntegralInput = Mathf.NegativeInfinity;
            this.maxIntegralInput = Mathf.Infinity;
            this.minDerivativeInput = Mathf.NegativeInfinity;
            this.maxDerivativeInput = Mathf.Infinity;
            this.ResetController();
        }

        #endregion

        #region Public Non-Static Methods

        /// <summary>
        /// Calculates the required input to move from the system from currentValue to targetValue.
        /// lastFrameTime should be set to the time since this function was last called.
        /// </summary>
        /// <param name="targetValue"></param>
        /// <param name="actualValue"></param>
        /// <param name="lastFrameTime"></param>
        /// <returns></returns>
        public float RequiredInput(float targetValue, float currentValue, float lastFrameTime)
        {
            // Implements the basic PID algorithm
            // First check that the target value passed in is not NaN (as this can cause long-lasting issues)
            if (float.IsNaN(targetValue)) { targetValue = 0f; }
            // Calculate the delta to the target value
            proportional = targetValue - currentValue;
            // Multiply by iGain here instead of at the return line, to allow tuning parameters on the fly
            // Otherwise when we change iGain the entire I term would change dramatically
            integralTimesIGain += proportional * lastFrameTime * iGain;
            // Clamp the integral term into the allowed input range
            if (integralTimesIGain < minInput) { integralTimesIGain = minInput; }
            else if (integralTimesIGain > maxInput) { integralTimesIGain = maxInput; }
            // Don't measure derivative on first system update
            if (!firstUpdate)
            {
                if (derivativeOnMeasurement)
                {
                    // Calculate how quickly the value is changing, then take the negative of it
                    // This works due to the following:
                    // P = target - current
                    // dP/dt = d(target)/dt - d(current)/dt
                    // Hence if we assume target is unchanging then...
                    // dP/dt = -d(current)/dt
                    // This eliminates spikes occuring when the target value changes
                    derivative = (previousValue - currentValue) / lastFrameTime;
                }
                else
                {
                    // Calculate how quickly the proportional value is changing
                    derivative = (proportional - previousProportional) / lastFrameTime;
                }
            }
            else { derivative = 0f; firstUpdate = false; }
            // Multiply the terms by their respective gains
            // We don't multiply by iGain here as we have already done it
            proportionalTimesPGain = proportional * pGain;
            derivativeTimesDGain = derivative * dGain;
            // If necessary, clamp the individual terms
            if (useIndividualInputLimits)
            {
                // Clamp the proportional term into the required range
                if (proportionalTimesPGain < minProportionalInput) { proportionalTimesPGain = minProportionalInput; }
                else if (proportionalTimesPGain > maxProportionalInput) { proportionalTimesPGain = maxProportionalInput; }
                // Clamp the integral term into the required range
                if (integralTimesIGain < minIntegralInput) { integralTimesIGain = minIntegralInput; }
                else if (integralTimesIGain > maxIntegralInput) { integralTimesIGain = maxIntegralInput; }
                // Clamp the derivative term into the required range
                if (derivativeTimesDGain < minDerivativeInput) { derivativeTimesDGain = minDerivativeInput; }
                else if (derivativeTimesDGain > maxDerivativeInput) { derivativeTimesDGain = maxDerivativeInput; }
            }
            // Calculate the required input
            requiredInput = proportionalTimesPGain + integralTimesIGain + derivativeTimesDGain;
            // Clamp the required input into the allowed input range
            if (requiredInput < minInput) { requiredInput = minInput; }
            else if (requiredInput > maxInput) { requiredInput = maxInput; }
            // Store this frame's values of the proportional and current value for use the next time this function is called
            previousProportional = proportional;
            previousValue = currentValue;
            // Return the calculated value
            return requiredInput;
        }

        /// <summary>
        /// Sets the input limits of the controller.
        /// </summary>
        /// <param name="minInputLimit"></param>
        /// <param name="maxInputLimit"></param>
        public void SetInputLimits(float minInputLimit, float maxInputLimit)
        {
            minInput = minInputLimit;
            maxInput = maxInputLimit;
        }

        /// <summary>
        /// Set the individual input limits of the controller.
        /// </summary>
        /// <param name="minProportionalInputLimit"></param>
        /// <param name="maxProportionalInputLimit"></param>
        /// <param name="minIntegralInputLimit"></param>
        /// <param name="maxIntegralInputLimit"></param>
        /// <param name="minDerivativeInputLimit"></param>
        /// <param name="maxDerivativeInputLimit"></param>
        public void SetIndividualInputLimits(float minProportionalInputLimit, float maxProportionalInputLimit,
            float minIntegralInputLimit, float maxIntegralInputLimit,
            float minDerivativeInputLimit, float maxDerivativeInputLimit)
        {
            minProportionalInput = minProportionalInputLimit;
            maxProportionalInput = maxProportionalInputLimit;
            minIntegralInput = minIntegralInputLimit;
            maxIntegralInput = maxIntegralInputLimit;
            minDerivativeInput = minDerivativeInputLimit;
            maxDerivativeInput = maxDerivativeInputLimit;
        }

        /// <summary>
        /// Resets state values of the controller.
        /// </summary>
        public void ResetController()
        {
            // Reset state values. Integral accumulates over time and previousProportional/previousValue store previous values,
            // so they need to be reset.
            integralTimesIGain = 0f;
            previousProportional = 0f;
            previousValue = 0f;
            // Remember that we have reset the controller for the next update
            firstUpdate = true;
        }

        #endregion

    }
}