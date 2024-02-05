using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The fitment type of a StickyMagazine. Magazines with the same
    /// type can fit the same weapon.
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    [System.Serializable]
    public class S3DMagType
    {
        #region Enumerations

        public enum MagType
        {
            A = 0,
            B = 1,
            C = 2,
            D = 3,
            E = 4,
            F = 5,
            G = 6,
            H = 7,
            I = 8,
            J = 9,
            K = 10,
            L = 11,
            M = 12,
            N = 13,
            O = 14,
            P = 15,
            Q = 16,
            R = 17,
            S = 18,
            T = 19,
            U = 20,
            V = 21,
            W = 22,
            X = 23,
            Y = 24,
            Z = 25
        }

        #endregion

        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        public MagType magType;

        public string magTypeName;

        #endregion

        #region Constructors
        public S3DMagType()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public S3DMagType(S3DMagType s3dMagType)
        {
            if (s3dMagType == null) { SetClassDefaults(); }
            else
            {
                magType = s3dMagType.magType;
                magTypeName = s3dMagType.magTypeName;
            }
        }

        #endregion

        #region Public API Methods - General

        public void SetClassDefaults()
        {
            magType = MagType.A;
            magTypeName = "A";
        }

        #endregion
    }
}