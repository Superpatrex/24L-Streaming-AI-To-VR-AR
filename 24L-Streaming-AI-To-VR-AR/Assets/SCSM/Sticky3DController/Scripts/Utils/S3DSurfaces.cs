using UnityEngine;
using System.Collections.Generic;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CreateAssetMenu(fileName = "Sticky3D Surfaces", menuName = "Sticky3D/Surfaces")]
    public class S3DSurfaces : ScriptableObject
    {
        #region Public Variables
        public List<S3DSurfaceType> surfaceTypeList;

        #endregion

        #region Editor Methods

        #if UNITY_EDITOR
        /// <summary>
        /// Get an array of the surface types. Generates garbage so use sparingly.
        /// </summary>
        /// <param name="s3dSurfaces"></param>
        /// <param name="includeNotSet"></param>
        /// <returns></returns>
        public static string[] GetNameArray(S3DSurfaces s3dSurfaces, bool includeNotSet = true)
        {
            int numTypes = s3dSurfaces == null || s3dSurfaces.surfaceTypeList == null ? 0 : s3dSurfaces.surfaceTypeList.Count;

            if (numTypes < 1)
            {
                return includeNotSet ? new string[] { "Not Set" } : null;
            }
            else
            {
                List<string> surfaceTypeNameList = new List<string>(numTypes + 1);

                if (includeNotSet) { surfaceTypeNameList.Add("Not Set"); }

                var surfaceTypeList = s3dSurfaces.surfaceTypeList;

                for (int stIdx = 0; stIdx < numTypes; stIdx++)
                {
                    string _name = surfaceTypeList[stIdx].surfaceName;
                    surfaceTypeNameList.Add(string.IsNullOrEmpty(_name) ? "No name surface" : _name);
                }

                return surfaceTypeNameList.ToArray();
            }
        }

        #endif

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the SurfaceType given the guidHash of that item.
        /// Returns null if no match is found.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public S3DSurfaceType GetSurfaceType (int guidHash)
        {
            int surfaceTypeIndex = GetSurfaceTypeIndex(guidHash);

            if (surfaceTypeIndex < 0) { return null; }
            else { return surfaceTypeList[surfaceTypeIndex]; }
        }

        /// <summary>
        /// Get the zero-based index in the list of surface types using the ID or guidHash.
        /// Returns -1 if no match is found.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public int GetSurfaceTypeIndex (int guidHash)
        {
            int surfaceTypeIndex = -1;
            int numSurfaceTypes = surfaceTypeList == null ? 0 : surfaceTypeList.Count;

            // We're not using a FindIndex because we want to avoid GC
            for (int stIdx = 0; stIdx < numSurfaceTypes; stIdx++)
            {
                S3DSurfaceType s3dSurfaceType = surfaceTypeList[stIdx];
                if (s3dSurfaceType.guidHash == guidHash)
                {
                    surfaceTypeIndex = stIdx;
                    break;
                }
            }

            return surfaceTypeIndex;
        }

        /// <summary>
        /// Get the ID or guidHash of the SurfaceType given the zero-based
        /// index position in the list. If no valid match is found, returns 0
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetSurfaceTypeID(int index)
        {
            int numSurfaceTypes = surfaceTypeList == null ? 0 : surfaceTypeList.Count;

            if (index < 0 || index >= numSurfaceTypes) { return 0; }
            else { return surfaceTypeList[index].guidHash; }
        }

        #endregion
    }
}