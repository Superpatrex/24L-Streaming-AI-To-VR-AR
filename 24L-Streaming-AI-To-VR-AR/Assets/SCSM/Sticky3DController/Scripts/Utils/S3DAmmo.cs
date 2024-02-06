using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The type, caliber, and/or  of ammunition. e.g. 9mm, .22LR
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    [System.Serializable]
    public class S3DAmmo
    {
        #region Enumerations

        public enum AmmoType
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

        /// <summary>
        /// A to Z
        /// </summary>
        public AmmoType ammoType;

        /// <summary>
        /// The ammo name, e.g., 9mm, 9mm hollow point, .357 Mag, Pulse1
        /// </summary>
        public string ammoName;

        /// <summary>
        /// How much the Damage Amount of the projecile is multiplied when it hits an object or character.
        /// Default = 1 (no change).
        /// </summary>
        [Range(0f, 100f)] public float damageMultiplier = 1f;

        /// <summary>
        /// How much the force of impact is multiplied when the projectile hits an object or character.
        /// Default = 1 (no change).
        /// </summary>
        [Range(0f, 100f)] public float impactMultiplier = 1f;

        #endregion

        #region Public Properties

        #endregion

        #region Public Static Variables

        #endregion

        #region Private Variables - General

        #endregion

        #region Constructors
        public S3DAmmo()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public S3DAmmo(S3DAmmo s3dAmmo)
        {
            if (s3dAmmo == null) { SetClassDefaults(); }
            else
            {
                ammoType = s3dAmmo.ammoType;
                ammoName = s3dAmmo.ammoName;
                damageMultiplier = s3dAmmo.damageMultiplier;
                impactMultiplier = s3dAmmo.impactMultiplier;
            }
        }

        #endregion

        #region Private Member Methods



        #endregion

        #region Public API Methods - General

        public void SetClassDefaults()
        {
            ammoType = AmmoType.A;
            ammoName = "A";
            damageMultiplier = 1f;
            impactMultiplier = 1f;
        }

        #endregion

    }
}