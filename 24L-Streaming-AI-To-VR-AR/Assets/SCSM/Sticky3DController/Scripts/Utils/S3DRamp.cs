using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    public class S3DRamp
    {
        #region Public Variables
        /// <summary>
        /// The input can be in the range 0.0 to +/-1.0 or -1.0 to 1.0.
        /// </summary>
        [Range(-1f,1f)] public float currentInput;
        [Range(0f, 30f)] public float rampUpDuration;
        [Range(0f, 30f)] public float rampDownDuration;

        #endregion

        #region Private Variables

        // NOTE: Internal values are stored as +ve even
        // though the value being smoothed could be +ve or -ve
        private float smoothTimer;
        private float rampTargetDuration;
        private float deltaAmount;
        private float currentTargetInput;
        private float previousInput;
        private float startInput;
        private bool isRampDown;
        private bool isStartInputNegative;
        #endregion

        #region Public Constructors

        public S3DRamp()
        {
            SetDefaults();
        }

        #endregion

        #region Private Methods

        private void SetDefaults()
        {
            currentInput = 0f;
            rampUpDuration = 0f;
            rampDownDuration = 0f;
            Reset();
        }

        #endregion

        #region Public Methods

        public void Reset()
        {
            smoothTimer = 0f;
            rampTargetDuration = 0f;
            deltaAmount = 0f;
            currentTargetInput = 0f;
            previousInput = 0f;
            startInput = 0f;
            isStartInputNegative = false;
            isRampDown = false;
        }

        /// <summary>
        /// Smooth or dampen the currentInput value based on user settings.
        /// </summary>
        /// <param name="lastFrameTime"></param>
        public void SmoothInput(float lastFrameTime)
        {
            // Remember the sign
            bool isNegative = currentInput < 0f;

            // Convert to a +ve number
            if (isNegative) { currentInput = -currentInput; }

            // If current input is 0 but the startInput was -ve, the result should be 0 or -ve.
            if (currentInput == 0f && isStartInputNegative) { isNegative = true; }

            // Clamp 0.0-1.0
            currentInput = currentInput < 0f ? 0f : currentInput > 1f ? 1f : currentInput;

            // If the ramp up time is not set, simply return the target value
            if (rampUpDuration > 0f || rampDownDuration > 0f)
            {
                // Are we still trying to get to the same target value?
                if (currentInput != currentTargetInput)
                {
                    // Target value has changed
                    currentTargetInput = currentInput;

                    // Are we starting at a -ve number?
                    isStartInputNegative = previousInput < 0f;
                    startInput = previousInput < 0f ? -previousInput : previousInput;

                    smoothTimer = 0f;

                    // What is the delta between original value and target value?
                    //deltaAmount = currentTargetInput - previousInput;
                    deltaAmount = currentTargetInput - startInput;

                    // Are we ramping up or down?
                    isRampDown = deltaAmount < 0f;

                    // What is the proportional ramp up duration between original value and target value?
                    if (isRampDown)
                    {
                        deltaAmount *= -1f;
                        rampTargetDuration = deltaAmount * rampDownDuration;
                    }
                    else
                    {
                        rampTargetDuration = deltaAmount * rampUpDuration;
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
                    currentInput = isRampDown ? startInput - deltaAmount * rampValue : startInput + deltaAmount * rampValue;
                }
            }

            // Re-apply the sign
            if (currentInput > 0f && isNegative) { currentInput = -currentInput; }

            previousInput = currentInput;
        }

        #endregion
    }
}