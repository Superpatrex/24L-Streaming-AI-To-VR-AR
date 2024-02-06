using UnityEngine;

// Copyright (c) 2015-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// Collection of common maths routines used in S3D.
    public class S3DMath
    {
        #region Static values
        public static readonly float e = 2.718281828459041f;
        #endregion

        #region Hash Code Maths

        /// <summary>
        /// Returns a more deterministic hashcode from a new System GUID.
        /// </summary>
        /// <returns></returns>
        public static int GetHashCodeFromGuid()
        {
            return GetHashCode(System.Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Based on concept from corefx/src/Common/src/System/Text/StringOrCharArray.cs
        /// in .NET Core. This should be a little more deterministic than the standard
        /// GetHashCode from .NET. As this doesn't use pointers it should also be thread safe.
        /// </summary>
        /// <param name="stringToHash"></param>
        /// <returns></returns>
        public static int GetHashCode(string stringToHash)
        {
            // The hash values can overflow the max values of int. This will
            // avoid getting an overflowexception. Values should just wrap around.
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < stringToHash.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ stringToHash[i];
                    if (i == stringToHash.Length - 1) { break; }
                    hash2 = ((hash2 << 5) + hash2) ^ stringToHash[i + 1];
                }

                // 1566083941 is a Mersenne prime Mp = 2^n - 1
                return hash1 + (hash2 * 1566083941);
            }
        }

        /// <summary>
        /// Get an array of hashed codes from an array of strings.
        /// </summary>
        /// <param name="stringsToHash"></param>
        /// <returns></returns>
        public static int[] GetHashCodes (string[] stringsToHash)
        {
            int numStrings = stringsToHash == null ? 0 : stringsToHash.Length;

            if (numStrings > 0)
            {
                int[] hashedArray = new int[numStrings];
                for (int sIdx = 0; sIdx < numStrings; sIdx++) { hashedArray[sIdx] = GetHashCode(stringsToHash[sIdx]); }
                return hashedArray;
            }
            else
            {
                return new int[0];
            }
        }

        #endregion

        #region General Maths

        /// <summary>
        /// Return the Vector2 as absolute (+ve) values.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 Abs(Vector2 v)
        {
            return new Vector2(v.x < 0 ? -v.x : v.x, v.y < 0 ? -v.y : v.y);
        }

        /// <summary>
        /// Return the Vector3 as absolute (+ve) values.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 Abs(Vector3 v)
        {
            return new Vector3(v.x < 0 ? -v.x : v.x, v.y < 0 ? -v.y : v.y, v.z < 0 ? -v.z : v.z);
        }

        /// <summary>
        /// Clamp a Vector2 to return x and y values between 0.0 and 1.0
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 Clamp(Vector2 v)
        {
            return new Vector2(v.x < 0f ? 0f : v.x > 1f ? 1f : v.x, v.y < 0f ? 0f : v.y > 1f ? 1f : v.y);
        }

        /// <summary>
        /// Clamp a Vector2 to return x and y values between the Min and Max values specified
        /// </summary>
        /// <param name="v"></param>
        /// <param name="xMin"></param>
        /// <param name="xMax"></param>
        /// <param name="yMin"></param>
        /// <param name="yMax"></param>
        /// <returns></returns>
        public static Vector2 Clamp(Vector2 v, float xMin, float xMax, float yMin, float yMax)
        {
            return new Vector2(v.x < xMin ? xMin : v.x > xMax ? xMax : v.x, v.y < yMin ? yMin : v.y > yMax ? yMax : v.y);
        }

        /// <summary>
        /// Apply damping to a float targetValue.
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="targetValue"></param>
        /// <param name="deltaTime"></param>
        /// <param name="damping"></param>
        /// <returns></returns>
        public static float DampValue(float currentValue, float targetValue, float deltaTime, float damping)
        {
            if (damping <= 0f) { return targetValue; }
            else if (damping >= 1f || deltaTime < 0.0001f || currentValue == targetValue) { return currentValue; }
            else
            {
                return Mathf.Lerp(currentValue, targetValue, 1f - Mathf.Pow(damping, deltaTime));
            }
        }

        /// <summary>
        /// Apply damping to a vector3 targetValue
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="targetValue"></param>
        /// <param name="deltaTime"></param>
        /// <param name="damping"></param>
        /// <returns></returns>
        public static Vector3 DampValue(Vector3 currentValue, Vector3 targetValue, float deltaTime, float damping)
        {
            if (damping <= 0f) { return targetValue; }
            else if (damping > 1f || deltaTime < 0.0001f) { return currentValue; }
            else if (currentValue == targetValue) { return targetValue; }
            else
            {
                // exp range is between -50 and -15 * dT.
                //float dampedValue = (35f * damping) - 50f;
                float exp = 1f - Mathf.Exp(((35f * damping) - 50f) * deltaTime);

                return new Vector3(Mathf.Lerp(currentValue.x, targetValue.x, exp), Mathf.Lerp(currentValue.y, targetValue.y, exp), Mathf.Lerp(currentValue.z, targetValue.z, exp));
            }
        }

        /// <summary>
        /// Given a maximum range of -b to +b, calculate what number, when multiplied by x, would
        /// give a range of -1.0 to 1.0.
        /// For example when b = 1.5, and x = 0.75, the returned value would be 0.3333.
        /// 0.75 / 1.5 * (1f / 1.5f)
        /// NOTE: This is NOT a mathematic term and NOT related to Inverse Normal Distribution.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float InvNormalise(float x, float b)
        {
            if (x < -b) { return -1f; }
            else if (x > b) { return 1f; }
            else { return x / b * (1f / b); }
        }

        /// <summary>
        /// Get the maximum absolute value of a Vector2 x and y values.
        /// e.g. v.x = -1.3, v.y = 0.5 return 1.3.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float MaxAbs(Vector2 v)
        {
            float f1 = v.x < 0 ? -v.x : v.x;
            float f2 = v.y < 0 ? -v.y : v.y;

            if (f1 > f2) { return f1; }
            else { return f2; }
        }

        /// <summary>
        /// Get the maximum absolute value of a Vector3 x, y and z values.
        /// e.g. v.x = -1.3, v.y = 0.5, v.z = 1.2 return 1.3.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float MaxAbs(Vector3 v)
        {
            // Note: we can reference x as v[0], y as v[1], and z as v[2].
            int axis = 0;
            Vector3 absV = Abs(v);
            if (absV[1] > absV[0]) { axis = 1; }
            if (absV[2] > absV[axis]) { axis = 2; }
            return absV[axis];
        }

        /// <summary>
        /// NLerp is a poor man's version of Slerp. Cheaper but faster than Slerp.
        /// Essentially it is a normalised version of Lerp yet has a spheric-like path.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 NLerp(Vector3 v1, Vector3 v2, float t)
        {
            if (t < 0f) { t = 0f; }
            else if (t > 1f) { t = 1f; }

            return new Vector3(v1.x + (v2.x-v1.x) * t, v1.y + (v2.y - v1.y) * t, v1.z + (v2.z - v1.z) * t).normalized;
        }

        /// <summary>
        /// Return the normalised direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Vector3 Normalise(Vector3 direction)
        {
            if (direction.sqrMagnitude < Vector3.kEpsilon)
            {
                return Vector3.forward;
            }
            else
            {
                return direction.normalized;
            }
        }

        /// <summary>
        /// Given a maximum range of -b to +b, find the -1.0 t0 1.0 value of x within that range.
        /// For example when b = 1.5, and x = 0.75, the returned value would be 0.5 because
        /// 0.75 * (1f / 1.5f). Return value is clamped at -1.0 and 1.0.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Normalise(float x, float b)
        {
            if (x < -b) { return -1f; }
            else if (x > b) { return 1f; }
            else { return x * (1f / b); }
        }

        /// <summary>
        /// Normalise the value "x" to return values between 0.0 and 1.0
        /// given the potential range between "a" and "b"
        /// If "b" less than or equal to "a" this funtion will always return 0
        /// </summary>
        /// <param name="x"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Normalise(float x, float a, float b)
        {
            if (b <= a) { return 0f; }
            else { return ((x - a) * (1f / (b - a))); }
        }

        /// <summary>
        /// Return the normalised direction from v1 to v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3 Normalise(Vector3 v1, Vector3 v2)
        {
            Vector3 direction = v2 - v1;

            if (direction.sqrMagnitude < Vector3.kEpsilon)
            {
                return Vector3.forward;
            }
            else
            {
                return direction.normalized;
            }
        }


        /// <summary>
        /// Return the normalised direction from v1 to v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="vForward">Default forward if v2 is equal to or very near v1</param>
        /// <returns></returns>
        public static Vector3 Normalise(Vector3 v1, Vector3 v2, Vector3 vForward)
        {
            Vector3 direction = v2 - v1;

            if (direction.sqrMagnitude < Vector3.kEpsilon)
            {
                return vForward;
            }
            else
            {
                return direction.normalized;
            }
        }

        /// <summary>
        /// Get the relative (local space) rotation from point 1 to point 2.
        /// [NOT YET TESTED]
        /// </summary>
        /// <param name="wsFromRotation">World space from point rotation</param>
        /// <param name="wsFromPoint">World space from point</param>
        /// <param name="wsToPoint">World space to point</param>
        /// <returns></returns>
        public static Quaternion RotateFromTo (Quaternion wsFromRotation, Vector3 wsFromPoint, Vector3 wsToPoint)
        {
            Vector3 direction = Normalise(wsFromPoint,  wsToPoint);

            // Local Space (relative) direction from "FromPoint" to "ToPoint"
            direction = Quaternion.Inverse(wsFromRotation) * direction;

            return Quaternion.LookRotation(direction);
        }

        /// <summary>
        /// Determine the longest axis of a vector.
        /// axis x = 0, y = 1, z = 2
        /// </summary>
        /// <param name="v"></param>
        /// <param name="axis"></param>
        /// <param name="distance"></param>
        public static void LongestAxis(Vector3 v, out int axis, out float distance)
        {
            // Note: we can reference x as v[0], y as v[1], and z as v[2].
            axis = 0;
            Vector3 absV = Abs(v);
            if (absV[1] > absV[0]) { axis = 1; }
            if (absV[2] > absV[axis]) { axis = 2; }
            distance = v[axis];
        }

        /// <summary>
        /// Get a direction based on the longest axis.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 LongestAxisDirection(Vector3 v)
        {
            Vector3 axisDirection = Vector3.zero;
            int axis = 0;
            float distance = 0f;
            LongestAxis(v, out axis, out distance);
            axisDirection[axis] = distance > 0f ? 1f : -1f;

            return axisDirection;
        }

        /// <summary>
        /// Get the smallest axis of a vector.
        /// axis x = 0, y = 1, z = 2
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static int SmallestAxis(Vector3 v)
        {
            int axis = 0;
            Vector3 absV = Abs(v);
            if (absV[1] < absV[0]) { axis = 1; }
            if (absV[2] < absV[axis]) { axis = 2; }
            return axis;
        }

        /// <summary>
        /// Get the largetst axis of a vector.
        /// axis x = 0, y = 1, z = 2
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static int LargestAxis(Vector3 v)
        {
            int axis = 0;
            Vector3 absV = Abs(v);
            if (absV[1] > absV[0]) { axis = 1; }
            if (absV[2] > absV[axis]) { axis = 2; }
            return axis;
        }

        /// <summary>
        /// The angle in degrees between a and b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float S3DDeltaAngle (float a, float b)
        {
            float v = b - a;

            if (v < Mathf.Epsilon && v > -Mathf.Epsilon) { v = 0f; }
            else
            {
                while (v > 360f) { v -= 360f; }
                while (v < 0f) { v += 360f; }

                // Convert into range -180 to +180
                if (v > 180f) { v -= 360f; }
            }

            return v;
        }

        /// <summary>
        /// Lerp between 2 angles in degrees. Always return the result
        /// as a Euler angle (0.0 to < 360.0 degrees).
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static float S3DLerpEuler (float a, float b, float t)
        {
            float v;

            if (t < Mathf.Epsilon) { v = a; }
            else if (t > 1f - Mathf.Epsilon) { v = b; }
            // Flip the lerp direction if required
            else if (a > b) { return S3DLerpEuler(b, a, t < 0f ? 1f : t > 1f ? 0f : 1f - t); }
            else
            {
                float delta = b - a;
                while (delta > 360f) { delta -= 360f; }
                while (delta < 0f) { delta += 360f; }

                // Convert into range -180 to +180
                if (delta > 180f) { delta -= 360f; }

                v = a + delta * (t < 0f ? 0f : t > 1f ? 1f : t);
            }

            // Convert into Euler 0.0 to <360.0 range
            while (v > 360f) { v -= 360f; }
            while (v < 0f) { v += 360f; }

            //Debug.Log("[DEBUG] LerpEuler a: " + a + " b: " + b + " t: " + t + " value: " + v);

            return v;
        }

        /// <summary>
        /// Using a hermite polynomial (s-curve), return a smoothed value
        /// between a and b.
        /// NEEDS MORE TESTING
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static float S3DSmoothStep(float a, float b, float t)
        {
            // Clamp the time between 0.0 and 1.0
            if (t < 0f) { t = 0; }
            else if (t > 1f) { t = 1f; }

            // Flip range
            if (b < a)
            {
                float c = b;
                a = b;
                b = c;

                t = 1f - t;
            }

            float x = (t - a) / (b - a);

            // Clamp
            if (x < 0f) { x = 0f; }
            else if (x > 1f) { x = 1f; }

            // Kevin Perlin variant
            return x * x * x * (x * (6.0f * x - 15.0f) + 10.0f);
        }

        /// <summary>
        /// Get the pitch (forward or back) of a slope normal, relative to the up direction of another object.
        /// All directions are in world space.
        /// </summary>
        /// <param name="fwdWS"></param>
        /// <param name="upWS"></param>
        /// <param name="rightWS"></param>
        /// <param name="slopeNormalWS"></param>
        /// <returns></returns>
        public static float SlopePitch (Vector3 fwdWS, Vector3 upWS, Vector3 rightWS, Vector3 slopeNormalWS)
        {
            return Vector3.SignedAngle(upWS, Normalise(Vector3.Project(slopeNormalWS, rightWS), slopeNormalWS), fwdWS);
        }

        #endregion
    }
}