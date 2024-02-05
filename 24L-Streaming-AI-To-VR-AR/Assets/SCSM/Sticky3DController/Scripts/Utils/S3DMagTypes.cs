using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A scriptable object that stores an array of 26 S3DMagTypes.
    /// </summary>
    [CreateAssetMenu(fileName = "Sticky3D Mag Types", menuName = "Sticky3D/Magazine Types")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class S3DMagTypes : ScriptableObject
    {
        #region Public Variables

        public S3DMagType[] magTypes;

        #endregion

        #region public Methods

        public void ValidateTypes()
        {
            // When created, always add 26 types.
            if (magTypes == null)
            {
                magTypes = new S3DMagType[26];

                for (int i = 0; i < 26; i++)
                {
                    S3DMagType magType = new S3DMagType();
                    char letter = System.Convert.ToChar(((int)'A') + i);

                    magType.magType = (S3DMagType.MagType)i;
                    magType.magTypeName = "Mag Type " + letter.ToString();

                    magTypes[i] = magType;
                }
            }
        }

        #endregion

        #region Events

        private void OnEnable()
        {
            ValidateTypes();
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Typically only used in the editor, this returns all the Mag Types in a scriptable object as an array of names.
        /// </summary>
        /// <param name="s3dMagTypes"></param>
        /// <param name="returnDefaultTypeIfEmpty"></param>
        /// <returns></returns>
        public static string[] GetMagTypeNames (S3DMagTypes s3dMagTypes, bool returnDefaultTypeIfEmpty)
        {
            int numMagTypes = s3dMagTypes == null || s3dMagTypes.magTypes == null ? 0 : s3dMagTypes.magTypes.Length;

            if (numMagTypes > 0)
            {
                string[] magTypeNames = new string[numMagTypes];

                for (int i = 0; i < numMagTypes; i++)
                {
                    S3DMagType magType = s3dMagTypes.magTypes[i];

                    magTypeNames[i] = magType == null || string.IsNullOrEmpty(magType.magTypeName) ? ((S3DMagType.MagType)i).ToString() : magType.magTypeName;
                }

                return magTypeNames;
            }
            else if (returnDefaultTypeIfEmpty)
            {
                return new string[] { "Mag Type " + S3DMagType.MagType.A.ToString() };
            }
            else
            {
                return null;
            }
        }

        #endregion

    }
}