using UnityEngine;
using System.Collections.Generic;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This scriptable object can contain a list of animation clip pairs used typically with
    /// Sticky Zones. When a character moves into a zone, the origianl clips are replaced with
    /// other animation clips. See also StickyZone and S3DAnimClipPair.
    /// </summary>
    [CreateAssetMenu(fileName = "Sticky3D Anim Clip Set", menuName = "Sticky3D/Anim Clip Set")]
    public class S3DAnimClipSet : ScriptableObject
    {
        #region Public Variables
        public List<S3DAnimClipPair> animClipPairList;

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the zero-based index in the list of Anim Clip Pairs using the ID or guidHash.
        /// Returns -1 if no match is found.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public int GetAnimClipPairIndex(int guidHash)
        {
            int animClipPairIndex = -1;
            int numAnimClipPairs = animClipPairList == null ? 0 : animClipPairList.Count;

            // We're not using a FindIndex because we want to avoid GC
            for (int stIdx = 0; stIdx < numAnimClipPairs; stIdx++)
            {
                S3DAnimClipPair s3dAnimClipPair = animClipPairList[stIdx];
                if (s3dAnimClipPair.guidHash == guidHash)
                {
                    animClipPairIndex = stIdx;
                    break;
                }
            }

            return animClipPairIndex;
        }

        /// <summary>
        /// Get the ID or guidHash of the Anim Clip Pair given the zero-based
        /// index position in the list. If no valid match is found, returns 0
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetAnimClipPairID(int index)
        {
            int numAnimClipPairs = animClipPairList == null ? 0 : animClipPairList.Count;

            if (index < 0 || index >= numAnimClipPairs) { return 0; }
            else { return animClipPairList[index].guidHash; }
        }

        #endregion
    }
}