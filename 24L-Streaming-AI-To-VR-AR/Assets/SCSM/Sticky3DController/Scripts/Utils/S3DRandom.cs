using System;

namespace scsmmedia
{
    /// <summary>
    /// S3DRandom is a linear congruential generator.
    /// It is a renamed version of LBRandom from Landscape Builder
    /// Usage:
    /// S3DRandom s3dRandom = new S3DRandom();
    /// // Set a seed value
    /// if (s3dRandom != null)
    /// {
    ///     s3dRandom.SetSeed(85.3f);
    ///     float randomNumber1 = s3dRandom.Next();
    ///     float randomNumber2 = s3dRandom.Next();
    ///     s3dRandom = null;
    /// }
    /// </summary>
    public class S3DRandom
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

        public S3DRandom()
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
        /// Return a value 0.0 1o 1.0 inclusive.
        /// </summary>
        /// <returns></returns>
        public float Range0to1()
        {
            float f = Range(0f, 1.001f);
            return f > 0.999f ? 1f : f;
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
        /// Return a random sign of -1 or 1
        /// </summary>
        /// <returns></returns>
        public float Sign()
        {
            if (Normalised() < 0.5f) { return -1f; }
            else { return 1f; }
        }

        /// <summary>
        /// Return a random sign of -1 or 1
        /// </summary>
        /// <returns></returns>
        public int SignInt()
        {
            if (Normalised() < 0.5f) { return -1; }
            else { return 1; }
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
