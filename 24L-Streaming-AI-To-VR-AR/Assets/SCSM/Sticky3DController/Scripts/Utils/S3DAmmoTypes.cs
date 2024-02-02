using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A scriptable object that stores an array of 26 S3DAmmo (types).
    /// </summary>
    [CreateAssetMenu(fileName = "Sticky3D Ammo Types", menuName = "Sticky3D/Ammo Types")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class S3DAmmoTypes : ScriptableObject
    {
        #region Public Variables

        public S3DAmmo[] ammoTypes;

        #endregion

        #region public Methods

        public void ValidateTypes()
        {
            // When created, always add 26 types.
            if (ammoTypes == null)
            {
                ammoTypes = new S3DAmmo[26];

                for (int i = 0; i < 26; i++)
                {
                    S3DAmmo ammo = new S3DAmmo();
                    char letter = System.Convert.ToChar(((int)'A') + i);

                    ammo.ammoType = (S3DAmmo.AmmoType)i;
                    ammo.ammoName = "Ammo Type " + letter.ToString();

                    ammoTypes[i] = ammo;
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
        /// Typically only used in the editor, this returns all the Ammo Types in a scriptable object as an array of names.
        /// </summary>
        /// <param name="s3dAmmoTypes"></param>
        /// <param name="returnDefaultTypeIfEmpty"></param>
        /// <returns></returns>
        public static string[] GetAmmoTypeNames (S3DAmmoTypes s3dAmmoTypes, bool returnDefaultTypeIfEmpty)
        {
            int numAmmoTypes = s3dAmmoTypes == null || s3dAmmoTypes.ammoTypes == null ? 0 : s3dAmmoTypes.ammoTypes.Length;

            if (numAmmoTypes > 0)
            {
                string[] ammoTypeNames = new string[numAmmoTypes];

                for (int i = 0; i < numAmmoTypes; i++)
                {
                    S3DAmmo ammo = s3dAmmoTypes.ammoTypes[i];

                    ammoTypeNames[i] = ammo == null || string.IsNullOrEmpty(ammo.ammoName) ? ((S3DAmmo.AmmoType)i).ToString() : ammo.ammoName;
                }

                return ammoTypeNames;
            }
            else if (returnDefaultTypeIfEmpty)
            {
                return new string[] { "Ammo Type " + S3DAmmo.AmmoType.A.ToString() };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Check to see if an AmmoTypes scriptable object looks valid.
        /// </summary>
        /// <param name="s3dAmmoTypes"></param>
        /// <returns></returns>
        public static bool IsAmmoTypeValid (S3DAmmoTypes s3dAmmoTypes)
        {
            return s3dAmmoTypes != null && s3dAmmoTypes.ammoTypes != null && s3dAmmoTypes.ammoTypes.Length == 26;
        }

        #endregion
    }
}