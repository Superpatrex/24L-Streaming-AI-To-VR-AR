using UnityEngine;
using System.Collections.Generic;

// Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class to hold information about a footstep. For example, which sound to play
    /// and/or particle effect prefab to instantiate when character walks on a particular
    /// surface type.
    /// In the editor they are titled Surface Actions.
    /// </summary>
    [System.Serializable]
    public class S3DFootstep
    {
        #region Enumerations
        public enum Foot
        {
            Left = 0,
            Right = 1,
            Both = 2
        }
        #endregion

        #region Public Static
        public static int FootLeftInt = (int)Foot.Left;
        public static int FootRightInt = (int)Foot.Right;
        public static int FootBothInt = (int)Foot.Both;

        #endregion

        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        public Foot foot;

        /// <summary>
        /// List of S3DSurfaceType guidHash
        /// </summary>
        public List<int> surfaceTypeIntList;

        public List<S3DTerrainTexture> s3dTerrainTextureList;

        /// <summary>
        /// Minimum volume the audio clips are played at
        /// </summary>
        [Range(0.01f, 1f)] public float minVolume;
        /// <summary>
        /// Maximum volume the audio clips are played at
        /// </summary>
        [Range(0.01f, 1f)] public float maxVolume;
        /// <summary>
        /// Minimum pitch applied to the audio clips
        /// </summary>
        [Range(-3f, 3f)] public float minPitch;
        /// <summary>
        /// Maximum pitch applied ot the audio clips;
        /// </summary>
        [Range(-3f, 3f)] public float maxPitch;

        /// <summary>
        /// A list of potential clips to play.
        /// </summary>
        public List<AudioClip> audioclipList;

        public bool showInEditor;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Hashed GUID code to uniquely identify S3DFootstep instance.
        /// </summary>
        public int guidHash;

        #endregion

        #region Internal Variables
        [System.NonSerialized] internal int numAudioClips;
        [System.NonSerialized] internal int numSurfaceTypes;
        [System.NonSerialized] internal int numTerrainTextures;
        [System.NonSerialized] internal int[] terrainTextureIDArray;
        [System.NonSerialized] internal float[] terrainTextureWeightArray;

        #endregion

        #region Constructors
        public S3DFootstep()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy Constructor for S3DFootstep class
        /// </summary>
        /// <param name="s3dFootStep"></param>
        public S3DFootstep(S3DFootstep s3dFootStep)
        {
            if (s3dFootStep == null) { SetClassDefaults(); }
            else
            {
                foot = s3dFootStep.foot;
                guidHash = s3dFootStep.guidHash;
                showInEditor = s3dFootStep.showInEditor;

                surfaceTypeIntList = new List<int>(s3dFootStep.surfaceTypeIntList);

                minVolume = s3dFootStep.minVolume;
                maxVolume = s3dFootStep.maxVolume;
                minPitch = s3dFootStep.minPitch;
                maxPitch = s3dFootStep.maxPitch;

                // Audio clips don't have a new or copy constructor - only a Create(..) method.
                int numClips = s3dFootStep.audioclipList == null ? 0 : s3dFootStep.audioclipList.Count;

                audioclipList = new List<AudioClip>(numClips < 3 ? 3 : numClips);

                // Copy the existing audio clips into the new list
                for (int clpIdx = 0; clpIdx < numClips; clpIdx++)
                {
                    AudioClip audioClip = s3dFootStep.audioclipList[clpIdx];
                    if (audioClip != null)
                    {
                        audioclipList.Add(audioClip);
                    }
                }
            }
        }

        #endregion

        #region Private Member Methods
        public void SetClassDefaults()
        {
            foot = Foot.Both;
            guidHash = S3DMath.GetHashCodeFromGuid();
            showInEditor = true;

            surfaceTypeIntList = new List<int>(5);

            s3dTerrainTextureList = new List<S3DTerrainTexture>(2);

            minVolume = 0.7f;
            maxVolume = 1f;
            minPitch = 0.5f;
            maxPitch = 1.5f;

            audioclipList = new List<AudioClip>(3);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the zero-based index in the list of surface types using the ID or guidHash.
        /// Returns -1 if no match is gound
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public int GetSurfaceTypeIndex(int guidHash)
        {
            int numSurfaceTypes = surfaceTypeIntList == null ? 0 : surfaceTypeIntList.Count;

            if (numSurfaceTypes > 0)
            {
                // We using integers so they shouldn't incur GC in a FindIndex.
                return surfaceTypeIntList.FindIndex(st => st == guidHash);
            }
            else { return -1; }
        }

        #endregion
    }
}