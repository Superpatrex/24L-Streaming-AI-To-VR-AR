using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Collection of common maths routines used in SSC.
    /// </summary>
    public class SSCMath
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

        #endregion

        #region Bezier Curve Maths

        #region Bezier Path variables

        private static Vector3 thisPointOnPath = Vector3.zero;
        private static Vector3 previousPointOnPath = Vector3.zero;

        // NOTES
        // p0 is the first point, p1 is its control point (direction at p0 is towards p1)
        // p3 is the second point, p2 is its control point (direction at p3 is away from p2)
        // Bezier curve point: B = (1 - t)^3 * p0 + 3 * (1 - t)^2 * t + p1 + 3 * (1 - t) * t^2 * p2 + t^3 * p3
        // First derivative: B' = 3 * (1 - t)^2 * (p1 - p0) + 6 * (1 - t) * t * (p2 - p1) + 3 * t^2 * (p3 - p2)
        // Second derivative: B" = 6 * (1 - t) * (p2 - 2*p1 + p0) + 6 * t * (p3 - 2*p2 + p1)
        // Curvature: K = len(B' x B") / len(B')^3

        #endregion

        // TODO: Look at all function names / variable names, make sure they are all consistent with each other

        #region Public Static Member API Methods

        #region Basic Functions

        /// <summary>
        /// Gets the point on the path given the index of the last path point and the t-value.
        /// The pointOnPath variable is set to the point on the path.
        /// If the return value is true, the operation was successful.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="tValue"></param>
        /// <param name="pointOnPath"></param>
        /// <returns></returns>
        public static bool GetPointOnPath(PathData pathData, int lastPathPointIndex, float tValue, ref Vector3 pointOnPath)
        {
            // Check that the path is valid
            if (pathData != null && pathData.pathLocationDataList != null)
            {
                // Check how many points there are in the path
                int pathDataLocationListCount = pathData.pathLocationDataList.Count;
                if (pathDataLocationListCount < 1)
                {
                    // No points in the path, so this is not a valid path
                    return false;
                }
                else if (pathDataLocationListCount == 1)
                {
                    // Check if the single point is assigned to a valid Location
                    if (pathData.pathLocationDataList[0].locationData.isUnassigned) { return false; }
                    else
                    {
                        // Only one point in the path, so simply return that point
                        pointOnPath = pathData.pathLocationDataList[0].locationData.position;
                        return true;
                    }
                }
                else if (lastPathPointIndex < pathDataLocationListCount)
                {
                    // Multiple points in the path, so perform the algorithm
                    // Get the index of the next (assigned) path point
                    // A potential optimisation would be to pass in the already determined pathDataLocationListCount...
                    int nextPathPointIndex = SSCManager.GetNextPathLocationIndex(pathData, lastPathPointIndex, true);

                    // If there is no next assigned point, return false
                    if (nextPathPointIndex < 0) { return false; }
                    else
                    {
                        // Call the internal algorithm function
                        GetPointOnPathInternal(pathData, lastPathPointIndex, nextPathPointIndex, tValue, ref pointOnPath);
                        return true;
                    }
                }
                else { return false; }
            }
            else { return false; }
        }

        /// <summary>
        /// Calculates the tangent, normal and binormal of the path given the index of the last path point and the t-value.
        /// The pathTangent variable is set to the (normalised) tangent of the path.
        /// The pathNormal variable is set to the (normalised) normal of the path.
        /// The pathBinormal variable is set to the (normalised) binormal of the path.
        /// If the return value is true, the operation was successful.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="tValue"></param>
        /// <param name="pathTangent"></param>
        /// <param name="pathNormal"></param>
        /// <param name="pathBinormal"></param>
        /// <returns></returns>
        public static bool GetPathFrenetData (PathData pathData, int lastPathPointIndex, float tValue, ref Vector3 pathTangent,
            ref Vector3 pathNormal, ref Vector3 pathBinormal)
        {
            // Check that the path is valid
            if (pathData != null && pathData.pathLocationDataList != null)
            {
                // Check how many points there are in the path
                int pathDataLocationListCount = pathData.pathLocationDataList.Count;
                if (pathDataLocationListCount <= 1)
                {
                    // One or no points in the path, so this is not a valid path for curvature to be determined
                    return false;
                }
                else if (lastPathPointIndex < pathDataLocationListCount)
                {
                    // Multiple points in the path, so perform the algorithm
                    // Get the index of the next path point
                    int nextPathPointIndex = SSCManager.GetNextPathLocationIndex(pathData, lastPathPointIndex, true);

                    // If there is no next assigned point, return false
                    if (nextPathPointIndex < 0) { return false; }
                    else
                    {
                        // Call the internal algorithm function to obtain the first and second derivatives 
                        // (which will be in the directions of the tangent and the normal respectively)
                        GetPathFirstDerivativeInternal(pathData, lastPathPointIndex, nextPathPointIndex, tValue, ref pathTangent);
                        GetPathSecondDerivativeInternal(pathData, lastPathPointIndex, nextPathPointIndex, tValue, ref pathNormal);
                        // Normalise the calculated tangent
                        pathTangent /= (float)System.Math.Sqrt((pathTangent.x * pathTangent.x) +
                            (pathTangent.y * pathTangent.y) + (pathTangent.z * pathTangent.z));
                        // Normalise the calculated normal
                        pathNormal /= (float)System.Math.Sqrt((pathNormal.x * pathNormal.x) +
                            (pathNormal.y * pathNormal.y) + (pathNormal.z * pathNormal.z));
                        // Binormal is simply the cross product of the tangent and the normal
                        // There is no need to normalise it, as the cross product of two orthogonal vectors of unit
                        // length is itself of unit length
                        // TODO optimise
                        pathBinormal = Vector3.Cross(pathTangent, pathNormal);
                        return true;
                    }
                }
                else { return false; }
            }
            else { return false; }
        }

        /// <summary>
        /// Calculates the tangent of the path given the index of the last path point and the t-value.
        /// The pathTangent variable is set to the (normalised) tangent of the path.
        /// If the return value is true, the operation was successful.
        /// NOTE: If you also want the normal and/or the binormal, call GetPathFrenetData instead (as it gets all
        /// three values at once more quickly).
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="tValue"></param>
        /// <param name="pathTangent"></param>
        /// <returns></returns>
        public static bool GetPathTangent(PathData pathData, int lastPathPointIndex, float tValue, ref Vector3 pathTangent)
        {
            // Check that the path is valid
            if (pathData != null && pathData.pathLocationDataList != null)
            {
                // Check how many points there are in the path
                int pathDataLocationListCount = pathData.pathLocationDataList.Count;
                if (pathDataLocationListCount <= 1)
                {
                    // One or no points in the path, so this is not a valid path for curvature to be determined
                    return false;
                }
                else if (lastPathPointIndex < pathDataLocationListCount)
                {
                    // Multiple points in the path, so perform the algorithm
                    // Get the index of the next path point
                    int nextPathPointIndex = SSCManager.GetNextPathLocationIndex(pathData, lastPathPointIndex, true);

                    // If there is no next assigned point, return false
                    if (nextPathPointIndex < 0) { return false; }
                    else
                    {
                        // Call the internal algorithm function to obtain the first derivative (which will be in the 
                        // direction of the tangent)
                        GetPathFirstDerivativeInternal(pathData, lastPathPointIndex, nextPathPointIndex, tValue, ref pathTangent);
                        // Normalise the calculated tangent
                        pathTangent /= (float)System.Math.Sqrt((pathTangent.x * pathTangent.x) +
                            (pathTangent.y * pathTangent.y) + (pathTangent.z * pathTangent.z));
                        return true;
                    }
                }
                else { return false; }
            }
            else { return false; }
        }

        /// <summary>
        /// Calculates the Frenet normal of the path given the index of the last path point and the t-value.
        /// The pathNormal variable is set to the (normalised) Frenet normal of the path.
        /// If the return value is true, the operation was successful.
        /// NOTE: If you also want the tangent and/or the binormal, call GetPathFrenetData instead (as it gets all
        /// three values at once more quickly).
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="tValue"></param>
        /// <param name="pathNormal"></param>
        /// <returns></returns>
        public static bool GetPathNormal(PathData pathData, int lastPathPointIndex, float tValue, ref Vector3 pathNormal)
        {
            // Check that the path is valid
            if (pathData != null && pathData.pathLocationDataList != null)
            {
                // Check how many points there are in the path
                int pathDataLocationListCount = pathData.pathLocationDataList.Count;
                if (pathDataLocationListCount <= 1)
                {
                    // One or no points in the path, so this is not a valid path for curvature to be determined
                    return false;
                }
                else if (lastPathPointIndex < pathDataLocationListCount)
                {
                    // Multiple points in the path, so perform the algorithm
                    // Get the index of the next path point
                    int nextPathPointIndex = SSCManager.GetNextPathLocationIndex(pathData, lastPathPointIndex, true);

                    // If there is no next assigned point, return false
                    if (nextPathPointIndex < 0) { return false; }
                    else
                    {
                        // Call the internal algorithm function to obtain the second derivative (which will be in the
                        // direction of the normal)
                        GetPathSecondDerivativeInternal(pathData, lastPathPointIndex, nextPathPointIndex, tValue, ref pathNormal);
                        // Normalised the calculated normal
                        pathNormal /= (float)System.Math.Sqrt((pathNormal.x * pathNormal.x) +
                            (pathNormal.y * pathNormal.y) + (pathNormal.z * pathNormal.z));
                        return true;
                    }
                }
                else { return false; }
            }
            else { return false; }
        }

        /// <summary>
        /// Calculates the curvature of the path given the index of the last path point and the t-value.
        /// The curvature is the reciprocal of the radius of curvature (i.e. radius of curvature = 1 / curvature).
        /// The pathCurvature variable is set to the radius of curvature.
        /// If the return value is true, the operation was successful.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="tValue"></param>
        /// <param name="pathCurvature"></param>
        /// <returns></returns>
        public static bool GetPathCurvature(PathData pathData, int lastPathPointIndex, float tValue, ref float pathCurvature)
        {
            // Check that the path is valid
            if (pathData != null && pathData.pathLocationDataList != null)
            {
                // Check how many points there are in the path
                int pathDataLocationListCount = pathData.pathLocationDataList.Count;
                if (pathDataLocationListCount <= 1)
                {
                    // One or no points in the path, so this is not a valid path for curvature to be determined
                    return false;
                }
                else if (lastPathPointIndex < pathDataLocationListCount)
                {
                    // Multiple points in the path, so perform the algorithm
                    // Get the index of the next path point
                    int nextPathPointIndex = SSCManager.GetNextPathLocationIndex(pathData, lastPathPointIndex, true);

                    // If there is no next assigned point, return false
                    if (nextPathPointIndex < 0) { return false; }
                    else
                    {
                        // Call the internal algorithm function
                        GetPathCurvatureInternal(pathData, lastPathPointIndex, nextPathPointIndex, tValue, ref pathCurvature);
                        return true;
                    }
                }
                else { return false; }
            }
            else { return false; }
        }

        /// <summary>
        /// Calculates the curvature of the path (in a given plane defined by the plane normal) given the index of the 
        /// last path point and the t-value.
        /// The curvature is the reciprocal of the radius of curvature (i.e. radius of curvature = 1 / curvature).
        /// The vector planeNormal passed in must be normalised.
        /// The pathCurvature variable is set to the radius of curvature.
        /// If the return value is true, the operation was successful.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="tValue"></param>
        /// <param name="pathCurvature"></param>
        /// <returns></returns>
        public static bool GetPathCurvatureInPlane(PathData pathData, int lastPathPointIndex, float tValue, Vector3 planeNormal, ref float pathCurvature)
        {
            // Check that the path is valid
            if (pathData != null && pathData.pathLocationDataList != null)
            {
                // Check how many points there are in the path
                int pathDataLocationListCount = pathData.pathLocationDataList.Count;
                if (pathDataLocationListCount <= 1)
                {
                    // One or no points in the path, so this is not a valid path for curvature to be determined
                    return false;
                }
                else if (lastPathPointIndex < pathDataLocationListCount)
                {
                    // Multiple points in the path, so perform the algorithm
                    // Get the index of the next path point
                    int nextPathPointIndex = SSCManager.GetNextPathLocationIndex(pathData, lastPathPointIndex, true);

                    // If there is no next assigned point, return false
                    if (nextPathPointIndex < 0) { return false; }
                    else
                    {
                        // Call the internal algorithm function
                        GetPathCurvatureInPlaneInternal(pathData, lastPathPointIndex, nextPathPointIndex, tValue, planeNormal, ref pathCurvature);
                        return true;
                    }
                }
                else { return false; }
            }
            else { return false; }
        }

        /// <summary>
        /// Calculates the velocity of a point on the path, given the position of the point on the path.
        /// The velocity variable is set to the velocity of the point.
        /// If the return value is true, the operation was successful.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="pointOnPath"></param>
        /// <param name="velocity"></param>
        /// <returns></returns>
        public static bool GetPathVelocity (PathData pathData, Vector3 pointOnPath, ref Vector3 velocity)
        {
            // Check that the path is valid
            if (pathData != null)
            {
                // The path anchorPoint is typically a ShipDockingStation's current position.
                // The velocity consists of two components:
                // 1. The component due to the velocity of the path
                // 2. The component due to the angular velocity of the path
                velocity = pathData.worldVelocity + Vector3.Cross(pathData.worldAngularVelocity, pointOnPath - pathData.anchorPoint);
                return true;
            }
            else { return false; }
        }

        #endregion

        #region Complex Functions

        /// <summary>
        /// Gets the position and t-value of the closest point on the path to the target point, given the last path point index.
        /// The closestPointOnPath variable is set to the position of the closest point.
        /// The closestPointOnPathTValue variable is set to the t-value of the closest point.
        /// If the return value is true, the operation was successful.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="targetPoint"></param>
        /// <param name="closestPointOnPath"></param>
        /// <param name="closestPointOnPathTValue"></param>
        /// <returns></returns>
        public static bool FindClosestPointOnPath (PathData pathData, int lastPathPointIndex, Vector3 targetPoint, ref Vector3 closestPointOnPath, ref float closestPointOnPathTValue)
        {
            // Check that the path is valid
            if (pathData != null && pathData.pathLocationDataList != null)
            {
                // Check how many points there are in the path
                int pathDataLocationListCount = pathData.pathLocationDataList.Count;
                if (pathDataLocationListCount < 1)
                {
                    // No points in the path, so this is not a valid path
                    return false;
                }
                else if (pathDataLocationListCount == 1)
                {
                    // Only one point in the path, so simply return 0 and the (only) Location
                    closestPointOnPath = pathData.pathLocationDataList[0].locationData.position;
                    closestPointOnPathTValue = 0f;
                    return true;
                }
                else if (lastPathPointIndex < pathDataLocationListCount)
                {
                    // Multiple points in the path, so perform the algorithm

                    // Get the index of the next path point
                    int nextPathPointIndex = SSCManager.GetNextPathLocationIndex(pathData, lastPathPointIndex, true);

                    // If there is no next assigned point, return false
                    if (nextPathPointIndex < 0) { return false; }
                    else
                    {
                        // First, do a rough check to find the neighbourhood of t-values we want to look at
                        // Iterate through a number of evenly spaced t-values and find the closest one
                        int roughTIterations = 5;
                        float tValueIncrementSize = 1f / roughTIterations;
                        closestPointOnPathTValue = 0f;
                        float currentTValue = 0f;
                        float closestPointOnPathSqrDist = Mathf.Infinity;
                        float thisPointSqrDist;
                        for (int i = 0; i <= roughTIterations; i++)
                        {
                            // Find the path point associated with this t-value
                            GetPointOnPathInternal(pathData, lastPathPointIndex, nextPathPointIndex, currentTValue, ref thisPointOnPath);
                            // Calculate the square of the distance from the this path point to the target point
                            thisPointSqrDist = (thisPointOnPath.x - targetPoint.x) * (thisPointOnPath.x - targetPoint.x) +
                                (thisPointOnPath.y - targetPoint.y) * (thisPointOnPath.y - targetPoint.y) +
                                (thisPointOnPath.z - targetPoint.z) * (thisPointOnPath.z - targetPoint.z);
                            // Compare the square distance with the current closest point
                            if (thisPointSqrDist < closestPointOnPathSqrDist)
                            {
                                // If this point is closer than the current closest point, set it as the new closest point
                                closestPointOnPath = thisPointOnPath;
                                closestPointOnPathTValue = currentTValue;
                                closestPointOnPathSqrDist = thisPointSqrDist;
                            }
                            // Increment the t-value
                            currentTValue += tValueIncrementSize;
                        }

                        // Next, use a binary search to more accurately determine the t-value
                        // Binary search algorithm:
                        // Start with an initial search window size of half the rough T iteration size
                        // Each iteration, search the path points at (t - search window size) and (t + search window size)
                        // with "t" being the current t-value
                        // - If either of these points is a closer path point, replace t with the new closest t-value
                        // - Otherwise halve the search window size
                        // Stop searching when the search window reaches a certain threshold size

                        float tSearchWindowSize = tValueIncrementSize * 0.5f;
                        // TODO specify externally
                        float tAccuracy = 0.0001f;

                        while (tSearchWindowSize > tAccuracy)
                        {
                            // Initially assume we will not find a closer path point
                            bool foundCloserPathPoint = false;
                            float newClosestTValue = closestPointOnPathTValue;

                            // Find the path point slightly further ahead of this t-value
                            currentTValue = closestPointOnPathTValue + tSearchWindowSize;
                            if (currentTValue > 1f) { currentTValue = 1f; }
                            GetPointOnPathInternal(pathData, lastPathPointIndex, nextPathPointIndex, currentTValue, ref thisPointOnPath);
                            // Calculate the square of the distance from this path point to the target point
                            thisPointSqrDist = (thisPointOnPath.x - targetPoint.x) * (thisPointOnPath.x - targetPoint.x) +
                                (thisPointOnPath.y - targetPoint.y) * (thisPointOnPath.y - targetPoint.y) +
                                (thisPointOnPath.z - targetPoint.z) * (thisPointOnPath.z - targetPoint.z);
                            // Compare the square distance with the current closest point
                            if (thisPointSqrDist < closestPointOnPathSqrDist)
                            {
                                // If this point is closer than the current closest point, set it as the new closest point
                                newClosestTValue = currentTValue;
                                closestPointOnPath = thisPointOnPath;
                                closestPointOnPathSqrDist = thisPointSqrDist;
                                foundCloserPathPoint = true;
                            }

                            // Find the path point slightly further behind this t-value
                            currentTValue = closestPointOnPathTValue - tSearchWindowSize;
                            if (currentTValue < 0f) { currentTValue = 0f; }
                            GetPointOnPathInternal(pathData, lastPathPointIndex, nextPathPointIndex, currentTValue, ref thisPointOnPath);
                            /// Calculate the square of the distance from the this path point to the target point
                            thisPointSqrDist = (thisPointOnPath.x - targetPoint.x) * (thisPointOnPath.x - targetPoint.x) +
                                (thisPointOnPath.y - targetPoint.y) * (thisPointOnPath.y - targetPoint.y) +
                                (thisPointOnPath.z - targetPoint.z) * (thisPointOnPath.z - targetPoint.z);
                            // Compare the square distance with the current closest point
                            if (thisPointSqrDist < closestPointOnPathSqrDist)
                            {
                                // If this point is closer than the current closest point, set it as the new closest point
                                newClosestTValue = currentTValue;
                                closestPointOnPath = thisPointOnPath;
                                closestPointOnPathSqrDist = thisPointSqrDist;
                                foundCloserPathPoint = true;
                            }

                            if (!foundCloserPathPoint)
                            {
                                // If neither of the path points we checked were closer then the closest path point,
                                // halve the search window size
                                tSearchWindowSize *= 0.5f;
                            }
                            else
                            {
                                // If we found a new closest path point, replace the old closest t-value with the new one
                                closestPointOnPathTValue = newClosestTValue;
                            }
                        }

                        return true;
                    }
                }
                else { return false; }
            }
            else { return false; }
        }

        /// <summary>
        /// Gets the position, t-value, lastPathPointIndex of the closest point on the path to the target point.
        /// The closestPointOnPath variable is set to the position of the closest point.
        /// The closestPointOnPathTValue variable is set to the t-value of the closest point.
        /// The lastPathPointIndex variable is set to the previous PathLocationData index on the path.
        /// If the return value is true, the operation was successful.
        /// NOTE: This should only be used if you do not know the lastPathPointIndex closest to the targetPoint.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="targetPoint"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="closestPointOnPath"></param>
        /// <param name="closestPointOnPathTValue"></param>
        /// <returns></returns>
        public static bool FindClosestPointOnPath(PathData pathData, Vector3 targetPoint, ref Vector3 closestPointOnPath, ref float closestPointOnPathTValue, ref int lastPathPointIndex)
        {
            // Check how many points there are in the path
            int pathDataLocationListCount = pathData != null && pathData.pathLocationDataList != null ? pathData.pathLocationDataList.Count : 0;

            if (pathDataLocationListCount < 1) { return false; }
            else if (pathDataLocationListCount == 1)
            {
                // Only one point in the path, so simply return 0 and the (only) Location
                closestPointOnPath = pathData.pathLocationDataList[0].locationData.position;
                closestPointOnPathTValue = 0f;
                return true;
            }
            else
            {
                float sqrMinDistance = float.MaxValue;
                int closestPathPointIndex = -1;

                // Find the closest user defined Location

                // Start loop with the index of the next assigned Location in the Path
                for (int currentPathPointIndex = SSCManager.GetNextPathLocationIndex(pathData, -1, false); currentPathPointIndex >= 0 && currentPathPointIndex < pathDataLocationListCount;)
                {
                    Vector3 locationPosition = pathData.pathLocationDataList[currentPathPointIndex].locationData.position;

                    float sqrDistToLocation = (locationPosition.x - targetPoint.x) * (locationPosition.x - targetPoint.x) +
                                              (locationPosition.y - targetPoint.y) * (locationPosition.y - targetPoint.y) +
                                              (locationPosition.z - targetPoint.z) * (locationPosition.z - targetPoint.z);

                    // Is this the closest Location to the target so far?
                    if (sqrDistToLocation < sqrMinDistance)
                    {
                        sqrMinDistance = sqrDistToLocation;
                        closestPathPointIndex = currentPathPointIndex;
                    }

                    // Get the index of the next assigned Location in the Path
                    currentPathPointIndex = SSCManager.GetNextPathLocationIndex(pathData, currentPathPointIndex, false);
                }

                // Did we find a closest Location?
                if (closestPathPointIndex >= 0)
                {
                    // Get the Locations either side of the closest one
                    int prevPathPointIndex = SSCManager.GetPreviousPathLocationIndex(pathData, closestPathPointIndex, pathData.isClosedCircuit);
                    int nextPathPointIndex = SSCManager.GetNextPathLocationIndex(pathData, closestPathPointIndex, pathData.isClosedCircuit);

                    // Check which Location is the closest
                    float sqrDistToPrevious = float.MaxValue;
                    float sqrDistToNext = float.MaxValue;

                    if (prevPathPointIndex >= 0 && prevPathPointIndex < pathDataLocationListCount)
                    {
                        Vector3 locationPosition = pathData.pathLocationDataList[prevPathPointIndex].locationData.position;

                        sqrDistToPrevious = (locationPosition.x - targetPoint.x) * (locationPosition.x - targetPoint.x) +
                                            (locationPosition.y - targetPoint.y) * (locationPosition.y - targetPoint.y) +
                                            (locationPosition.z - targetPoint.z) * (locationPosition.z - targetPoint.z);
                    }

                    if (nextPathPointIndex >= 0 && nextPathPointIndex < pathDataLocationListCount)
                    {
                        Vector3 locationPosition = pathData.pathLocationDataList[nextPathPointIndex].locationData.position;

                        sqrDistToNext = (locationPosition.x - targetPoint.x) * (locationPosition.x - targetPoint.x) +
                                        (locationPosition.y - targetPoint.y) * (locationPosition.y - targetPoint.y) +
                                        (locationPosition.z - targetPoint.z) * (locationPosition.z - targetPoint.z);
                    }

                    if (sqrDistToPrevious < sqrDistToNext) { lastPathPointIndex = prevPathPointIndex; }
                    else { lastPathPointIndex = closestPathPointIndex; }

                    return FindClosestPointOnPath(pathData, lastPathPointIndex, targetPoint, ref closestPointOnPath, ref closestPointOnPathTValue);
                }
                else { return false; }
            }
        }

        /// <summary>
        /// Gets the position, curvature and float path point index of a point on the path that is added distance ahead of a 
        /// given other point on the path, specified by the index of the last path point and the t-value, and using approximately
        /// approximateIterations iterations.
        /// The newPointOnPath variable is set to the position of the calculated point.
        /// The pathCurvature variable is set to the curvature of the path at the calculated point.
        /// The newPointOnPathLastPathPointIndex variable is set to the index of the last path point before the calculated point.
        /// The newPointOnPathTValue variable is set to the t-value of the calculated point.
        /// If the return value is true, the operation was successful.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="tValue"></param>
        /// <param name="addedDistance"></param>
        /// <param name="newPointOnPath"></param>
        /// <param name="pathCurvature"></param>
        /// <param name="newPointOnPathLastPathPointIndex"></param>
        /// <param name="newPointOnPathTValue"></param>
        /// <returns></returns>
        public static bool GetFurtherPointOnPathData (PathData pathData, int lastPathPointIndex, float tValue, float addedDistance, 
            int approximateIterations, ref Vector3 newPointOnPath, ref float pathCurvature, ref int newPointOnPathLastPathPointIndex, 
            ref float newPointOnPathTValue)
        {
            // Check that the path is valid
            if (pathData != null && pathData.pathLocationDataList != null)
            {
                // Check how many points there are in the path
                int pathDataLocationListCount = pathData.pathLocationDataList.Count;
                if (pathDataLocationListCount <= 1)
                {
                    // One or no points in the path, so this is not a valid path for distance to be added
                    return false;
                }
                else if (lastPathPointIndex < pathDataLocationListCount)
                {
                    // Multiple points in the path, so perform the algorithm

                    if (addedDistance > Mathf.Epsilon)
                    {
                        // Step 1: Determine which two points this new point on the path will be between
                        // There are two possible scenarios:
                        // - Case 1: The new point on the path is between the last point and the next point
                        // - Case 2: The new point on the path is somewhere after the next point

                        // Get the index of the next path point
                        int nextPathPointIndex = SSCManager.GetNextPathLocationIndex(pathData, lastPathPointIndex, true);

                        // If there is no next assigned point, return false
                        if (nextPathPointIndex < 0) { return false; }
                        else
                        {
                            // TODO allow the distance from the current point on the path to the next path point to be passed in
                            // and also passed out, to save calculation
                            bool reachedEndOfPath = false;
                            // For the first point, calculate the distance from the current point on the path to the next path point
                            // using 10 line segments
                            float distanceToNextPathPoint = 0f;
                            float startTValue = tValue;
                            GetDistanceBetweenTValuesInternal(pathData, lastPathPointIndex, nextPathPointIndex, startTValue, 1f, 10, ref distanceToNextPathPoint);
                            // If the added distance is greater than the distance to the next path point, iteratively search
                            // For the two path points the added distance point will be between
                            while (distanceToNextPathPoint < addedDistance)
                            {
                                // If this path is not a closed circuit, don't allow going past the last path point
                                if (!pathData.isClosedCircuit && 
                                    (nextPathPointIndex == SSCManager.GetLastAssignedLocationIdx(pathData) || 
                                    nextPathPointIndex == SSCManager.GetFirstAssignedLocationIdx(pathData)))
                                {
                                    distanceToNextPathPoint = addedDistance + 1f;
                                    reachedEndOfPath = true;
                                }
                                else
                                {
                                    // Subtract the distance to this point from the added distance
                                    addedDistance -= distanceToNextPathPoint;
                                    // Recaclulate the last and next path points
                                    lastPathPointIndex = SSCManager.GetNextPathLocationIndex(pathData, lastPathPointIndex, true);
                                    nextPathPointIndex = SSCManager.GetNextPathLocationIndex(pathData, lastPathPointIndex, true);
                                    // Get the distance between the new last and next path points
                                    distanceToNextPathPoint = pathData.pathLocationDataList[nextPathPointIndex].distanceFromPreviousLocation;
                                    // Start t-value for all later path points is always zero
                                    startTValue = 0f;
                                }
                            }

                            if (!reachedEndOfPath)
                            {
                                // Step 2: Determine the new point on the path at the given distance
                                // Calculate (roughly) how much to increment the t-value each time, based on a given number of segments to use
                                // Then iteratively calculate the length from each point to the next, until the total length exceeds the 
                                // added distance. Then interpolate between this point and the last point to get the new point on the path

                                // Calculate the t-value we expect the new point on the path to have
                                // (using the assumption that t-values are linear)
                                float expectedTValue = startTValue + ((1f - startTValue) * (addedDistance / distanceToNextPathPoint));

                                // Assume that we will go about as far as the expected t-value (from the start t-value),
                                // and calculate a t-value increment size that will reach that t-value in the given number of iterations
                                float tValueIncrementSize = (expectedTValue - startTValue) / approximateIterations;
                                // Don't allow t-increment sizes less than 0.001
                                if (tValueIncrementSize < 0.001f) { tValueIncrementSize = 0.001f; }

                                // Get the initial point on the path and the t-value of the next point on the path
                                GetPointOnPathInternal(pathData, lastPathPointIndex, nextPathPointIndex, startTValue, ref previousPointOnPath);
                                float thisPointOnPathTValue = startTValue + tValueIncrementSize;

                                // Loop through until we find the new point on the path
                                bool foundNewPointOnPath = false;
                                float totalDistance = 0f;
                                while (!foundNewPointOnPath)
                                {
                                    // Get the next point on the path
                                    GetPointOnPathInternal(pathData, lastPathPointIndex, nextPathPointIndex, thisPointOnPathTValue, ref thisPointOnPath);
                                    // Calculate the distance from the previous path point to this path point
                                    float distanceBetweenPointsOnPath = (float)System.Math.Sqrt(
                                        (thisPointOnPath.x - previousPointOnPath.x) * (thisPointOnPath.x - previousPointOnPath.x) +
                                        (thisPointOnPath.y - previousPointOnPath.y) * (thisPointOnPath.y - previousPointOnPath.y) +
                                        (thisPointOnPath.z - previousPointOnPath.z) * (thisPointOnPath.z - previousPointOnPath.z));
                                    // Add this distance to the total distance
                                    totalDistance += distanceBetweenPointsOnPath;
                                    // Compare the new total distance to the added distance
                                    if (totalDistance > addedDistance)
                                    {
                                        // If we have now gone further than the added distance, we know that the new point on the path will
                                        // be somewhere between the previous path point and this path point.
                                        // So simply linearly interpolate t-values between the two path points to give the t-value
                                        // of the new point on the path
                                        newPointOnPathTValue = thisPointOnPathTValue - (tValueIncrementSize *
                                            ((totalDistance - addedDistance) / distanceBetweenPointsOnPath));
                                        foundNewPointOnPath = true;
                                    }
                                    else
                                    {
                                        // We have not yet gone further than the added distance, so we are still iterating
                                        // Increment the t-value
                                        thisPointOnPathTValue += tValueIncrementSize;
                                        if (thisPointOnPathTValue > 1f)
                                        {
                                            // If we have now gone further than the end of this section of the path (past the next path point),
                                            // we know that the new point on the path will be somewhere between this path point and the end of this
                                            // section of the path.
                                            // So simply linearly interpolate t-values between this path point and the end of this section of the path
                                            // to give the t-value of the new point on the path
                                            newPointOnPathTValue = 1f + ((1f - (thisPointOnPathTValue - tValueIncrementSize)) *
                                                ((distanceToNextPathPoint - addedDistance) / (distanceBetweenPointsOnPath - totalDistance + distanceToNextPathPoint)));
                                            foundNewPointOnPath = true;
                                        }
                                        else
                                        {
                                            // Set this path point to be the previous path point
                                            previousPointOnPath = thisPointOnPath;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // If we have reached the end of the path, simply use the last path point
                                nextPathPointIndex = SSCManager.GetLastAssignedLocationIdx(pathData);
                                lastPathPointIndex = SSCManager.GetPreviousPathLocationIndex(pathData, nextPathPointIndex, false);
                                newPointOnPathTValue = 1f;
                            }

                            // Get the extra point on path data - position, curvature, last path point index
                            GetPointOnPathInternal(pathData, lastPathPointIndex, nextPathPointIndex, newPointOnPathTValue, ref newPointOnPath);
                            GetPathCurvatureInternal(pathData, lastPathPointIndex, nextPathPointIndex, newPointOnPathTValue, ref pathCurvature);
                            newPointOnPathLastPathPointIndex = lastPathPointIndex;

                            return true;
                        }
                    }
                    else
                    {
                        // Added distance is 0, so simply return the passed in path point
                        newPointOnPathLastPathPointIndex = lastPathPointIndex;
                        newPointOnPathTValue = tValue;
                        // Get the index of the next path point
                        int nextPathPointIndex = SSCManager.GetNextPathLocationIndex(pathData, lastPathPointIndex, true);
                        // Get the point on the path and the curvature
                        GetPointOnPathInternal(pathData, lastPathPointIndex, nextPathPointIndex, newPointOnPathTValue, ref newPointOnPath);
                        GetPathCurvatureInternal(pathData, lastPathPointIndex, nextPathPointIndex, newPointOnPathTValue, ref pathCurvature);
                        return true;
                    }
                }
                else { return false; }
            }
            else { return false; }
        }

        #endregion

        #region Path Computation Functions

        /// <summary>
        /// Calculates the distance between two (consecutive) path points given the index of the last path point and the number of
        /// line segments to use during the calculation. More line segments will give a more accurate distance.
        /// The distanceBetweenPathPoints variable is set to the distance between the path points.
        /// If the return value is true, the operation was successful.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="pathSegments"></param>
        /// <param name="distanceBetweenPathPoints"></param>
        /// <returns></returns>
        public static bool GetDistanceBetweenPathPoints(PathData pathData, int lastPathPointIndex, int lineSegments, ref float distanceBetweenPathPoints)
        {
            // Check that the path is valid
            if (pathData != null && pathData.pathLocationDataList != null)
            {
                // Check how many points there are in the path
                int pathDataLocationListCount = pathData.pathLocationDataList.Count;
                if (pathDataLocationListCount <= 1)
                {
                    // One or no points in the path, so this is not a valid path for distance to be calculated
                    return false;
                }
                else if (lastPathPointIndex < pathDataLocationListCount)
                {
                    // Multiple points in the path, so perform the algorithm
                    // Get the index of the next path point
                    int nextPathPointIndex = SSCManager.GetNextPathLocationIndex(pathData, lastPathPointIndex, true);

                    // If there is no next assigned point, return false
                    if (nextPathPointIndex < 0) { return false; }
                    else
                    {
                        // Call the internal algorithm function
                        GetDistanceBetweenTValuesInternal(pathData, lastPathPointIndex, nextPathPointIndex, 0f, 1f, lineSegments, ref distanceBetweenPathPoints);

                        return true;
                    }
                }
                else { return false; }
            }
            else { return false; }
        }

        #endregion

        #endregion

        #region (Internal) Private Static Member Methods

        // Path variables
        private static Vector3 pathPoint0 = Vector3.zero;
        private static Vector3 pathPoint1 = Vector3.zero;
        private static Vector3 pathPoint2 = Vector3.zero;
        private static Vector3 pathPoint3 = Vector3.zero;
        private static Vector3 thisPathPointFirstDerivative = Vector3.zero;
        private static Vector3 thisPathPointSecondDerivative = Vector3.zero;

        /// <summary>
        /// Calculates the point on the path given the index of the last path point, the index of the next path point and the t-value.
        /// The pointOnPath variable is set to the point on the path.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="nextPathPointIndex"></param>
        /// <param name="tValue"></param>
        /// <param name="pointOnPath"></param>
        private static void GetPointOnPathInternal (PathData pathData, int lastPathPointIndex, int nextPathPointIndex, float tValue, 
            ref Vector3 pointOnPath)
        {
            // Get path points
            pathPoint0 = pathData.pathLocationDataList[lastPathPointIndex].locationData.position;
            pathPoint3 = pathData.pathLocationDataList[nextPathPointIndex].locationData.position;
            // Get control points
            pathPoint1 = pathData.pathLocationDataList[lastPathPointIndex].outControlPoint;
            pathPoint2 = pathData.pathLocationDataList[nextPathPointIndex].inControlPoint;
            // Calculate new path point
            // Bezier curve point: B = (1 - t)^3 * p0 + 3 * (1 - t)^2 * t + p1 + 3 * (1 - t) * t^2 * p2 + t^3 * p3
            // Do it component-wise to improve performance
            pointOnPath.x = (1f - tValue) * (1f - tValue) * (1f - tValue) * pathPoint0.x +
                            3f * (1f - tValue) * (1f - tValue) * tValue * pathPoint1.x +
                            3f * (1f - tValue) * tValue * tValue * pathPoint2.x +
                            tValue * tValue * tValue * pathPoint3.x;
            pointOnPath.y = (1f - tValue) * (1f - tValue) * (1f - tValue) * pathPoint0.y +
                            3f * (1f - tValue) * (1f - tValue) * tValue * pathPoint1.y +
                            3f * (1f - tValue) * tValue * tValue * pathPoint2.y +
                            tValue * tValue * tValue * pathPoint3.y;
            pointOnPath.z = (1f - tValue) * (1f - tValue) * (1f - tValue) * pathPoint0.z +
                            3f * (1f - tValue) * (1f - tValue) * tValue * pathPoint1.z +
                            3f * (1f - tValue) * tValue * tValue * pathPoint2.z +
                            tValue * tValue * tValue * pathPoint3.z;
        }

        /// <summary>
        /// Calculates the first derivative of the path given the index of the last path point, the index of the next path point and the t-value.
        /// The pathFirstDerivative variable is set to the first derivative of the path.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="nextPathPointIndex"></param>
        /// <param name="tValue"></param>
        /// <param name="pointOnPath"></param>
        private static void GetPathFirstDerivativeInternal (PathData pathData, int lastPathPointIndex, int nextPathPointIndex, float tValue,
            ref Vector3 pathFirstDerivative)
        {
            // Get path points
            pathPoint0 = pathData.pathLocationDataList[lastPathPointIndex].locationData.position;
            pathPoint3 = pathData.pathLocationDataList[nextPathPointIndex].locationData.position;
            // Get control points
            pathPoint1 = pathData.pathLocationDataList[lastPathPointIndex].outControlPoint;
            pathPoint2 = pathData.pathLocationDataList[nextPathPointIndex].inControlPoint;
            // Calculate new path point
            // First derivative: B' = 3 * (1 - t)^2 * (p1 - p0) + 6 * (1 - t) * t * (p2 - p1) + 3 * t^2 * (p3 - p2)
            // Do it component-wise to improve performance
            pathFirstDerivative.x = 3f * (1f - tValue) * (1f - tValue) * (pathPoint1.x - pathPoint0.x) +
                                    6f * (1f - tValue) * tValue * (pathPoint2.x - pathPoint1.x) +
                                    3f * tValue * tValue * (pathPoint3.x - pathPoint2.x);
            pathFirstDerivative.y = 3f * (1f - tValue) * (1f - tValue) * (pathPoint1.y - pathPoint0.y) +
                                    6f * (1f - tValue) * tValue * (pathPoint2.y - pathPoint1.y) +
                                    3f * tValue * tValue * (pathPoint3.y - pathPoint2.y);
            pathFirstDerivative.z = 3f * (1f - tValue) * (1f - tValue) * (pathPoint1.z - pathPoint0.z) +
                                    6f * (1f - tValue) * tValue * (pathPoint2.z - pathPoint1.z) +
                                    3f * tValue * tValue * (pathPoint3.z - pathPoint2.z);
        }

        /// <summary>
        /// Calculates the second derivative of the path given the index of the last path point, the index of the next path point and the t-value.
        /// The pathSecondDerivative variable is set to the first derivative of the path.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="nextPathPointIndex"></param>
        /// <param name="tValue"></param>
        /// <param name="pointOnPath"></param>
        private static void GetPathSecondDerivativeInternal (PathData pathData, int lastPathPointIndex, int nextPathPointIndex, float tValue,
            ref Vector3 pathSecondDerivative)
        {
            // Get path points
            pathPoint0 = pathData.pathLocationDataList[lastPathPointIndex].locationData.position;
            pathPoint3 = pathData.pathLocationDataList[nextPathPointIndex].locationData.position;
            // Get control points
            pathPoint1 = pathData.pathLocationDataList[lastPathPointIndex].outControlPoint;
            pathPoint2 = pathData.pathLocationDataList[nextPathPointIndex].inControlPoint;
            // Calculate new path point
            // Second derivative: B" = 6 * (1 - t) * (p2 - 2*p1 + p0) + 6 * t * (p3 - 2*p2 + p1)
            // Do it component-wise to improve performance
            pathSecondDerivative.x = 6f * (1f - tValue) * (pathPoint2.x - 2f * pathPoint1.x + pathPoint0.x) +
                                     6f * tValue * (pathPoint3.x - 2f * pathPoint2.x + pathPoint1.x);
            pathSecondDerivative.y = 6f * (1f - tValue) * (pathPoint2.y - 2f * pathPoint1.y + pathPoint0.y) +
                                     6f * tValue * (pathPoint3.y - 2f * pathPoint2.y + pathPoint1.y);
            pathSecondDerivative.z = 6f * (1f - tValue) * (pathPoint2.z - 2f * pathPoint1.z + pathPoint0.z) +
                                     6f * tValue * (pathPoint3.z - 2f * pathPoint2.z + pathPoint1.z);
        }

        /// <summary>
        /// Calculates the radius of curvature of a point on the path given the index of the last path point, 
        /// the index of the next path point and the t-value.
        /// The pointPathCurvature variable is set to the radius of curvature.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="tValue"></param>
        /// <param name="pathPointCurvature"></param>
        /// <returns></returns>
        private static void GetPathCurvatureInternal (PathData pathData, int lastPathPointIndex, int nextPathPointIndex, 
            float tValue, ref float pathPointCurvature)
        {
            // Get path points
            pathPoint0 = pathData.pathLocationDataList[lastPathPointIndex].locationData.position;
            pathPoint3 = pathData.pathLocationDataList[nextPathPointIndex].locationData.position;
            // Get control points
            pathPoint1 = pathData.pathLocationDataList[lastPathPointIndex].outControlPoint;
            pathPoint2 = pathData.pathLocationDataList[nextPathPointIndex].inControlPoint;
            // Calculate new path point
            // Curvature: K = len(B' x B") / len(B')^3
            // TODO optimise
            // Get first and second derivatives
            GetPathFirstDerivativeInternal(pathData, lastPathPointIndex, nextPathPointIndex, tValue, ref thisPathPointFirstDerivative);
            GetPathSecondDerivativeInternal(pathData, lastPathPointIndex, nextPathPointIndex, tValue, ref thisPathPointSecondDerivative);
            float firstDerivativeMagnitude = (float)System.Math.Sqrt((thisPathPointFirstDerivative.x * thisPathPointFirstDerivative.x) +
                (thisPathPointFirstDerivative.y * thisPathPointFirstDerivative.y) +
                (thisPathPointFirstDerivative.z * thisPathPointFirstDerivative.z));
            // Calculate curvature
            pathPointCurvature = Vector3.Cross(thisPathPointFirstDerivative, thisPathPointSecondDerivative).magnitude / 
                (firstDerivativeMagnitude * firstDerivativeMagnitude * firstDerivativeMagnitude);
        }

        /// <summary>
        /// Calculates the radius of curvature (in a given plane defined by the plane normal) of a point on the path 
        /// given the index of the last path point, the index of the next path point and the t-value.
        /// The vector planeNormal passed in must be normalised.
        /// The pointPathCurvature variable is set to the radius of curvature.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="tValue"></param>
        /// <param name="pathPointCurvature"></param>
        /// <returns></returns>
        private static void GetPathCurvatureInPlaneInternal(PathData pathData, int lastPathPointIndex, int nextPathPointIndex, 
            float tValue, Vector3 planeNormal, ref float pathPointCurvature)
        {
            // Get path points
            pathPoint0 = pathData.pathLocationDataList[lastPathPointIndex].locationData.position;
            pathPoint3 = pathData.pathLocationDataList[nextPathPointIndex].locationData.position;
            // Get control points
            pathPoint1 = pathData.pathLocationDataList[lastPathPointIndex].outControlPoint;
            pathPoint2 = pathData.pathLocationDataList[nextPathPointIndex].inControlPoint;
            // Calculate new path point
            // Curvature: K = len(B' x B") / len(B')^3
            // Get first and second derivatives
            GetPathFirstDerivativeInternal(pathData, lastPathPointIndex, nextPathPointIndex, tValue, ref thisPathPointFirstDerivative);
            GetPathSecondDerivativeInternal(pathData, lastPathPointIndex, nextPathPointIndex, tValue, ref thisPathPointSecondDerivative);
            // Project derivatives into the plane defined by the given plane normal
            // Projection of a onto b = a.b / (|a|*|b|) * b/|b|
            // In this case the magnitude of the plane normal is 1
            thisPathPointFirstDerivative = Vector3.ProjectOnPlane(thisPathPointFirstDerivative, planeNormal);
            thisPathPointSecondDerivative = Vector3.ProjectOnPlane(thisPathPointSecondDerivative, planeNormal);
            float firstDerivativeMagnitude = thisPathPointFirstDerivative.magnitude;
            // Calculate curvature (TODO optimise)
            pathPointCurvature = Vector3.Cross(thisPathPointFirstDerivative, thisPathPointSecondDerivative).magnitude /
                (firstDerivativeMagnitude * firstDerivativeMagnitude * firstDerivativeMagnitude);
        }

        /// <summary>
        /// Calculates the distance between two points on the given the index of the last path point, the index of the next path point, 
        /// the t-value of the start point on the path, the t-value of the end point of the path and the number of
        /// line segments to use during the calculation. More line segments will give a more accurate distance.
        /// The distanceBetweenTValues variable is set to the distance between the two points on the path.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="lastPathPointIndex"></param>
        /// <param name="nextPathPointIndex"></param>
        /// <param name="startTValue"></param>
        /// <param name="endTValue"></param>
        /// <param name="lineSegments"></param>
        /// <param name="distanceBetweenTValues"></param>
        private static void GetDistanceBetweenTValuesInternal (PathData pathData, int lastPathPointIndex, int nextPathPointIndex, 
            float startTValue, float endTValue, int lineSegments, ref float distanceBetweenTValues)
        {
            // Calculate how much to increment the t-value by for each iteration to achieve the specified number of line segments
            float tValueIncrementSize = (endTValue - startTValue) / lineSegments;
            float currentTValue = startTValue;
            distanceBetweenTValues = 0f;
            // Get the first path point
            GetPointOnPathInternal(pathData, lastPathPointIndex, nextPathPointIndex, currentTValue, ref thisPointOnPath);
            // Iterate through a number of evenly spaced t-values and add the lengths of the line segments together
            for (int i = 0; i < lineSegments; i++)
            {
                // Increment the t-value, and set the previous path point
                currentTValue += tValueIncrementSize;
                previousPointOnPath = thisPointOnPath;
                // Find the path point associated with this t-value
                GetPointOnPathInternal(pathData, lastPathPointIndex, nextPathPointIndex, currentTValue, ref thisPointOnPath);
                // Calculate the distance to this path point from the last point, and add it to the total distance
                distanceBetweenTValues += (float)System.Math.Sqrt(
                    (thisPointOnPath.x - previousPointOnPath.x) * (thisPointOnPath.x - previousPointOnPath.x) +
                    (thisPointOnPath.y - previousPointOnPath.y) * (thisPointOnPath.y - previousPointOnPath.y) +
                    (thisPointOnPath.z - previousPointOnPath.z) * (thisPointOnPath.z - previousPointOnPath.z));
            }
        }

        #endregion

        #endregion

        #region General Curve Methods

        /// <summary>
        /// Evaluate an ease in-out curve.
        /// Time (t) is clamped between 0.0 and 1.0
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static float EaseInOutCurve(float t)
        {
            t = t < 0f ? 0f : t > 1f ? 1f : t;

            // f2(x) = x^2 / (x^2 + (1-x)^2)
            return (t * t) / ( (t * t) + ((1 - t) * (1 - t)) );
        }

        /// <summary>
        /// Evaluate an ease in-out curve.
        /// Time (t) and strength are clamped between 0.0 and 1.0
        /// Stength = 0.0 - linear, Stength 1.0 = full ease-out.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="strength"></param>
        /// <returns></returns>
        public static float EaseInOutCurve(float t, float strength)
        {
            t = t < 0f ? 0f : t > 1f ? 1f : t;
            strength = strength < 0f ? 0f : (strength > 1f ? 1f : strength);

            return (1 - strength) * t + strength * EaseInOutCurve(t);
        }

        /// <summary>
        /// Evaluate an ease in-out curve with a smoother (slower) start and end.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static float EaseInOutCurve3X(float t)
        {
            t = t < 0f ? 0f : t > 1f ? 1f : t;

            // f3(x) = x^3 / (x^3 + (1-x)^3)
            return (t * t * t) / ((t * t * t) + ((1 - t) * (1 - t) * (1 - t)));
        }

        /// <summary>
        /// Evaluate an ease in-out curve with a smoother (slower) start and end.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static float EaseInOutCurve4X(float t)
        {
            t = t < 0f ? 0f : t > 1f ? 1f : t;

            // f4(x) = 4^3 / (x^4 + (1-x)^4)
            return (t * t * t * t) / ((t * t * t * t) + ((1 - t) * (1 - t) * (1 - t) * (1 - t)));
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
        /// Normalise the value "x" to return values between 0 and 1
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

        #endregion

        #region Power Methods

        // Faster version of Mathf.Pow for integer exponents
        public static float IntPow(float num, int pow)
        {
            if (pow != 0)
            {
                float ans = num;
                for (int i = 1; i < pow; i++)
                {
                    ans *= num;
                }
                return ans;
            }
            else { return 1f; }
        }

        // Faster version of Mathf.Pow for integer exponents and bases
        public static int IntPow(int num, int pow)
        {
            if (pow != 0)
            {
                int ans = num;
                for (int i = 1; i < pow; i++)
                {
                    ans *= num;
                }
                return ans;
            }
            else { return 1; }
        }

        #endregion

        #region Static Shape and Point Math Methods

        private static bool IsInPolygon(List<Vector2> points, Vector2 sample)
        {
            bool isInPolygon = false;

            int j = points.Count - 1;

            for (int i = 0; i < points.Count; j = i++)
            {
                if (((points[i].y <= sample.y && sample.y < points[j].y) || (points[j].y <= sample.y && sample.y < points[i].y)) &&
                   (sample.x < (points[j].x - points[i].x) * (sample.y - points[i].y) / (points[j].y - points[i].y) + points[i].x))
                    isInPolygon = !isInPolygon;
            }

            return isInPolygon;
        }

        /// <summary>
        /// Is the sample point inside the quad which has points p1, p2, p3 and p4?
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static bool IsInQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector3 sample)
        {
            return IsInTriangle(p1, p2, p3, sample) || IsInTriangle(p4, p2, p3, sample);
        }

        /// <summary>
        /// Is the sample point inside the triangle which has points: p1,p2 & p3
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static bool IsInTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 sample)
        {
            bool halfPlaneSide1 = HalfPlaneSideSign(sample, p1, p2) < 0f;
            bool halfPlaneSide2 = HalfPlaneSideSign(sample, p2, p3) < 0f;
            bool halfPlaneSide3 = HalfPlaneSideSign(sample, p3, p1) < 0f;

            return ((halfPlaneSide1 == halfPlaneSide2) && (halfPlaneSide2 == halfPlaneSide3));
        }


        public static float HalfPlaneSideSign(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return (p1.x - p3.x) * (p2.z - p3.z) - (p2.x - p3.x) * (p1.z - p3.z);
        }

        /// <summary>
        /// Allows for a "thickness" of each triangle to be specified to allow for error
        /// </summary>
        /// <param name="sp1"></param>
        /// <param name="sp2"></param>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static float SquareDistanceToSide(Vector3 sp1, Vector3 sp2, Vector3 sample)
        {
            float squareSideLength = PlanarSquareDistance(sp1, sp2);
            float dotProduct = ((sample.x - sp1.x) * (sp2.x - sp1.x) + (sample.z - sp1.z) * (sp2.z - sp1.z)) / squareSideLength;
            if (dotProduct < 0)
            {
                return PlanarSquareDistance(sample, sp1);
            }
            else if (dotProduct <= 1)
            {
                return PlanarSquareDistance(sample, sp1) - dotProduct * dotProduct * squareSideLength;
            }
            else
            {
                return PlanarSquareDistance(sample, sp2);
            }
        }

        /// <summary>
        /// Square distance calculation ignoring y distance
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static float PlanarSquareDistance(Vector3 p1, Vector3 p2)
        {
            // Basically pythagoras but without y and without final square root
            return (((p1.x - p2.x) * (p1.x - p2.x)) + ((p1.z - p2.z) * (p1.z - p2.z)));
        }

        /// <summary>
        /// Find the closest central spline point
        /// </summary>
        /// <param name="splinePoints"></param>
        /// <param name="pointToMatch"></param>
        /// <returns></returns>
        public static int FindClosestPoint(Vector3[] splinePoints, Vector3 pointToMatch)
        {
            float sqrDist = 0f;
            float closestSqrDist = Mathf.Infinity;
            int closestPoint = 0;

            if (splinePoints != null)
            {
                for (int i = 0; i < splinePoints.Length; i++)
                {
                    sqrDist = PlanarSquareDistance(splinePoints[i], pointToMatch);
                    if (sqrDist < closestSqrDist) { closestSqrDist = sqrDist; closestPoint = i; }
                }
            }

            return closestPoint;
        }

        /// <summary>
        /// Find closest consecutive path point to this one
        /// </summary>
        /// <param name="splinePoints"></param>
        /// <param name="pointToMatch"></param>
        /// <param name="consecutiveTo"></param>
        /// <returns></returns>
        public static int FindClosestConsecutivePoint(Vector3[] splinePoints, Vector3 pointToMatch, int consecutiveTo)
        {
            int closestPoint = 0;

            if (splinePoints != null)
            {
                // Check if the consecutive points exist
                bool c1Exists = consecutiveTo - 1 >= 0;
                bool c2Exists = splinePoints.Length > consecutiveTo + 1;
                if (c1Exists && c2Exists)
                {
                    // Compare the distances to both of the consecutive points, return the closest point
                    if (PlanarSquareDistance(splinePoints[consecutiveTo - 1], pointToMatch) < PlanarSquareDistance(splinePoints[consecutiveTo + 1], pointToMatch))
                    {
                        closestPoint = consecutiveTo - 1;
                    }
                    else { closestPoint = consecutiveTo + 1; }
                }
                // Return any point that exists
                else if (c1Exists) { closestPoint = consecutiveTo - 1; }
                else if (c2Exists) { closestPoint = consecutiveTo + 1; }
            }

            return closestPoint;
        }

        /// <summary>
        /// Find furthest consecutive path point to this one
        /// </summary>
        /// <param name="splinePoints"></param>
        /// <param name="pointToMatch"></param>
        /// <param name="consecutiveTo"></param>
        /// <returns></returns>
        public static int FindFurthestConsecutivePoint(Vector3[] splinePoints, Vector3 pointToMatch, int consecutiveTo)
        {
            int closestPoint = 0;

            if (splinePoints != null)
            {
                // Check if the consecutive points exist
                bool c1Exists = consecutiveTo - 1 >= 0;
                bool c2Exists = splinePoints.Length > consecutiveTo + 1;
                if (c1Exists && c2Exists)
                {
                    // Compare the distances to both of the consecutive points, return the furthest point
                    if (PlanarSquareDistance(splinePoints[consecutiveTo - 1], pointToMatch) < PlanarSquareDistance(splinePoints[consecutiveTo + 1], pointToMatch))
                    {
                        closestPoint = consecutiveTo + 1;
                    }
                    else { closestPoint = consecutiveTo - 1; }
                }
                // Return any point that exists
                else if (c1Exists) { closestPoint = consecutiveTo - 1; }
                else if (c2Exists) { closestPoint = consecutiveTo + 1; }
            }

            return closestPoint;
        }

        #endregion

    }
}