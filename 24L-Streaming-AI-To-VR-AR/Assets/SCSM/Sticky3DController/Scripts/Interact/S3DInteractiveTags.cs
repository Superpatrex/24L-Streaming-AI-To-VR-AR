using UnityEngine;
using System.Collections.Generic;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A scriptable object that stores an array of 32 interactive object tags.
    /// A 32-bit integer is used as a bitfield to store or query multiple tags.
    /// </summary>
    [CreateAssetMenu(fileName = "Sticky3D Interactive Tags", menuName = "Sticky3D/Interactive Tags")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class S3DInteractiveTags : ScriptableObject
    {
        #region Public Variables

        public string[] tags;

        #endregion

        #region Events

        private void OnEnable()
        {
            ValidateTags();
        }

        #endregion

        #region Public API Member Methods

        /// <summary>
        /// Find a tag using the name (description) of a tag. If it exists, add it to the given
        /// tagsMask and return the result.
        /// If the tag name cannot be found, return the original tagsMask. If there are duplicate
        /// tags with the same name, only add the first one.
        /// USAGE:
        /// int _mask = s3dInteractiveTags.AddToMask(0, "Rifle");
        /// _mask = s3dInteractiveTags.AddToMask(_mask, "Hand gun");
        /// WARNING: This may impact GC.
        /// </summary>
        /// <param name="tagsMask"></param>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public int AddToMask (int tagsMask, string tagName)
        {
            int tagMask = GetMask(tagName);
            int newTagsMask = tagsMask;

            // If the tagName exists, and the it is not already in the tagsMask supplied, add it.
            if (tagMask != 0 && ((tagMask & tagsMask) == 0))
            {
                // Add the tag to the existing mask
                newTagsMask = tagMask | tagsMask;
            }

            return newTagsMask;
        }

        /// <summary>
        /// Attempt to get the mask for a given interactive tag name.
        /// Returns 0 if none found. If there are duplicate tags then the first one is returned.
        /// Tag names are case-sensitive.
        /// WARNING: This may impact GC.
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public int GetMask (string tagName)
        {
            // Start with None
            int value = 0;

            for (int i = 0; i < 32; i++)
            {
                if (!string.IsNullOrEmpty(tags[i]) && tags[i] == tagName)
                {
                    value = 1 << i;
                    break;
                }
            }

            return value;
        }

        /// <summary>
        /// Get an array of tag names or descriptions.
        /// WARNING: This may impact GC.
        /// </summary>
        /// <param name="isIgnoreBlanks"></param>
        /// <returns></returns>
        public string[] GetTagNames(bool isIgnoreBlanks)
        {
            if (isIgnoreBlanks)
            {
                ValidateTags();
                List<string> tagList = new List<string>(32);

                int numTags = tags.Length;

                for (int i = 0; i < 32; i++)
                {
                    if (!string.IsNullOrEmpty(tags[i])) { tagList.Add(tags[i]); }
                }

                return tagList.ToArray();
            }
            else
            {
                return tags;
            }
        }

        /// <summary>
        /// If there are no tags, create a new array with 32 tags.
        /// Include a Default first tag.
        /// </summary>
        public void ValidateTags()
        {
            // When created, always add 32 tags
            if (tags == null)
            {
                tags = new string[32];

                // Always include a default tag
                tags[0] = "Default";

                for (int i = 1; i < 32; i++)
                {
                    tags[i] = string.Empty;
                }
            }
        }
        #endregion

        #region Public API Static Methods

        /// <summary>
        /// Get the first matching position in the array of tags, based on the provided bitmask.
        /// This is useful if you have a bitmask where only one value is selected.
        /// </summary>
        /// <param name="bitmask"></param>
        /// <returns></returns>
        public static int GetFirstPosition (int bitmask)
        {
            int index = 0;

            for (int idx = 0; idx < 32; idx++)
            {
                if (bitmask == (1 << idx))
                {
                    index = idx;
                    break;
                }
            }

            return index;
        }


        /// <summary>
        /// Check to see if an S3DInteractiveTags scriptable object looks valid.
        /// </summary>
        /// <param name="s3dAmmoTypes"></param>
        /// <returns></returns>
        public static bool IsInteractiveTagsValid(S3DInteractiveTags s3dInteractiveTags)
        {
            return s3dInteractiveTags != null && s3dInteractiveTags.tags != null && s3dInteractiveTags.tags.Length == 32;
        }

        #endregion
    }
}