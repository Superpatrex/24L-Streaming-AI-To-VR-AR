using System;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// SSCRandom is a linear congruential generator.
    /// It is a renamed version of LBRandom from Landscape Builder
    /// Usage:
    /// SSCRandom sscRandom = new SSCRandom();
    /// // Set a seed value
    /// if (sscRandom != null)
    /// {
    ///     sscRandom.SetSeed(85.3f);
    ///     float randomNumber1 = sscRandom.Next();
    ///     float randomNumber2 = sscRandom.Next();
    ///     s3dRandom = null;
    /// }
    /// </summary>
    public class SSCRandom
    {
        #region Public variables and properties

        /// <summary>
        /// Returns the last generated random number.
        /// NOTE: This is NOT the values returned by the Range() methods
        /// </summary>
        public UInt32 GetLast { get { return currentInt; } }

        #endregion

        #region Private variables
        private UInt32 currentInt;

        #endregion

        #region Constructors

        public SSCRandom()
        {
            SetClassDefaults();
        }

        #endregion

        #region Public Non-Static Methods

        public void SetSeed(int seed)
        {
            // Must be > 0
            seed = seed < 0 ? -seed : (seed == 0 ? 1 : seed);

            currentInt = (UInt32)(((UInt64)seed * 279470273uL) % (UInt64)0xfffffffb);
        }

        public UInt32 Next()
        {
            currentInt = (UInt32)(((UInt64)currentInt * 279470273uL) % (UInt64)0xfffffffb);

            return currentInt;
        }

        /// <summary>
        /// Return values between min (inclusive) and max (exclusive)
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public float Range(float min, float max)
        {
            // Get a value between 0.0 and 0.999.
            // NOTE: We don't actually ever want 1.0f returned
            float nF = Normalised();

            return ((max - min) * nF) + min;
        }

        /// <summary>
        /// Return values between min (inclusive) and max (inclusive)
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public int Range(int min, int max)
        {
            // Get a value between 0.0 and 0.999.
            // NOTE: We don't actually ever want 1.0f returned
            float nF = Normalised();

            // Expand min/max range by subtracting 0.5 from min, and adding 0.5 to max.
            // This will ensure the min and max values have the same distribution as
            // values between min and max.
            // Fast round by adding 0.5 and casting to int - similar to Math.Floor()
            return (int)((float)(((max - min + 1) * nF) + min - 0.5f) + 0.5f);
        }

        /// <summary>
        /// Return a value between 0.000 (inclusive) and 1.000 (exclusive)
        /// i.e. max value will be 0.999.
        /// </summary>
        /// <returns></returns>
        public float Normalised()
        {
            currentInt = Next();

            return (float)(currentInt % 1000) / 1000f;
        }

        #endregion

        #region Private Non-static Methods

        private void SetClassDefaults()
        {
            currentInt = 0u;
        }

        #endregion
    }
}