using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Class containing data for a sticky damage region.
    /// </summary>
    [System.Serializable]
    public class S3DDamageRegion
    {
        #region Public Enumerations

        public enum DamageType
        {
            /// <summary>
            /// Damage from beams or projectiles of this type will be unaffected by character damage multipliers
            /// i.e. the amount of damage done to the character will be identical to damageAmount.
            /// </summary>
            Default = 0,
            TypeA = 100,
            TypeB = 105,
            TypeC = 110,
            TypeD = 115,
            TypeE = 120,
            TypeF = 125,
        }

        #endregion

        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// The name of this damage region.
        /// </summary>
        public string name;
        /// <summary>
        /// When invincible, it will not take damage however its health can still be manually decreased.
        /// </summary>
        public bool isInvincible;

        /// <summary>
        /// The starting health value of this damage region. Range 0.0 to 1.0.
        /// Displayed in the editor as 0.0 - 100.0
        /// </summary>
        [Range(0f, 1f)] public float startingHealth;

        /// <summary>
        /// Whether this damage region uses shielding. Up until a point, shielding protects the damage region from damage 
        /// (which can affect the performance of parts associated with the damage region).
        /// </summary>
        public bool useShielding;
        /// <summary>
        /// Damage below this value will not affect the shield or the damage region's health while the shield is still active
        /// (i.e. until the shield has absorbed damage more than or equal to the shieldingAmount value from damage events above the 
        /// damage threshold). Only relevant if useShielding is enabled.
        /// </summary>
        public float shieldingDamageThreshold;
        /// <summary>
        /// How much damage the shield can absorb before it ceases to protect the damage region from damage.  Only relevant if 
        /// useShielding is enabled.
        /// </summary>
        public float shieldingAmount;

        /// <summary>
        /// When useShielding is true, this is the rate per second that a shield will recharge (default = 0)
        /// </summary>
        [Range(0f, 100f)] public float shieldingRechargeRate;

        /// <summary>
        /// When useShielding is true, and shieldingRechargeRate is greater than 0, this is the delay, in seconds,
        /// between when damage occurs to a shield and it begins to recharge.
        /// </summary>
        [Range(0f, 300f)] public float shieldingRechargeDelay;

        /// <summary>
        /// Value indicating the resistance of the damage region to damage caused by collisions. Increasing this value will decrease the amount
        /// of damage caused to the damage region by collisions.
        /// </summary>
        [Range(0f, 100f)] public float collisionDamageResistance;

        /// <summary>
        /// The bone, transform and colliders associated with this damage region (if any).
        /// For some unknown reason, Unity always creates a serialised instance of this
        /// in the editor. The human bone will default to Hips.
        /// So, just to be sure this isn't Unity version specific, we'll always create them.
        /// </summary>
        public S3DHumanBonePersist s3dHumanBonePersist;

        /// <summary>
        /// The InstanceID of the collider. Set at runtime. 0 = unset.
        /// </summary>
        public int colliderId;

        /// <summary>
        /// Whether the damage region is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// Whether the damage region multipliers are show as expanded in the inspector window of the editor
        /// </summary>
        public bool showMultipliersInEditor;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Hashed GUID code to uniquely identify a damage region on a character. Used instead of the
        /// name to avoid GC when comparing two damage regions.
        /// </summary>
        public int guidHash;

        #endregion

        #region Public Properties

        /// <summary>
        /// Get the unique ID of the damage region. Will return 0 if not set.
        /// </summary>
        public int DamageRegionID { get { return guidHash; } }

        /// <summary>
        /// Get or set the current health value of this damage region.
        /// Returns values between 0.0 and 100.0.
        /// </summary>
        public float Health
        {
            get { return health * 100f; }
            set { SetHealth(value); }
        }

        /// <summary>
        /// [READONLY]
        /// Normalised (0.0 – 1.0) value of the health of the damage region.
        /// </summary>
        public float HealthNormalised
        {
            get { return health; }
        }

        /// <summary>
        /// The current health value of this damage region's shield.
        /// Max value = shieldingAmount.
        /// A value below 0 will indicate it is inactive.
        /// </summary>
        public float ShieldHealth
        {
            get { return shieldHealth; }
            set { SetShieldHealth(value); }
        }

        /// <summary>
        /// [READONLY]
        /// Normalised (0.0 – 1.0) value of the shield for this damage region. If useShielding is false, it will
        /// always return 0. If the shield is inactive, it will also return 0.
        /// </summary>
        public float ShieldNormalised
        {
            get
            {
                float _shieldN = !useShielding || shieldingAmount == 0f ? 0f : shieldHealth / shieldingAmount;
                if (_shieldN > 1f) { return 1f; }
                else if (_shieldN < 0f) { return 0f; }
                else { return _shieldN; }
            }
        }

        #endregion

        #region Public Static Variables

        #endregion

        #region Private or Internal Variables - General

        /// <summary>
        /// The array of damage multipliers for this region.
        /// The relative amount of damage a Type A-F projectile or beam will inflict on the character.
        /// When the Damage Type is Default, the damage multipliers are ignored.
        /// </summary>
        [SerializeField] private float[] damageMultipliersArray;

        /// <summary>
        /// 0.0 - 1.0 value of the current damage region health
        /// </summary>
        [System.NonSerialized] private float health;

        /// <summary>
        /// 0.0 - 1.0 value of the current damage region shield health
        /// </summary>
        [System.NonSerialized] private float shieldHealth;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Flag for when the region destroy "event" has been actioned after health reaches 0
        /// </summary>
        [System.NonSerialized] internal bool isDestroyed;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] internal float shieldRechargeDelayTimer;

        #endregion

        #region Class constructors

        public S3DDamageRegion()
        {
            SetClassDefaults();
        }

        // Copy constructor
        public S3DDamageRegion(S3DDamageRegion damageRegion)
        {
            if (damageRegion == null) { SetClassDefaults(); }
            else
            {
                this.name = damageRegion.name;
                this.isInvincible = damageRegion.isInvincible;
                this.startingHealth = damageRegion.startingHealth;
                this.Health = damageRegion.Health;
                this.useShielding = damageRegion.useShielding;
                this.shieldingDamageThreshold = damageRegion.shieldingDamageThreshold;
                this.shieldingAmount = damageRegion.shieldingAmount;
                this.ShieldHealth = damageRegion.ShieldHealth;
                this.shieldingRechargeRate = damageRegion.shieldingRechargeRate;
                this.shieldingRechargeDelay = damageRegion.shieldingRechargeDelay;
                this.collisionDamageResistance = damageRegion.collisionDamageResistance;
                this.s3dHumanBonePersist = new S3DHumanBonePersist(damageRegion.s3dHumanBonePersist);
                this.showInEditor = damageRegion.showInEditor;
                this.showMultipliersInEditor = damageRegion.showMultipliersInEditor;
                this.guidHash = damageRegion.guidHash;
                this.Initialise();
            }
        }

        #endregion

        #region Private and Internal Methods - General

        internal void CheckShieldRecharge()
        {
            // Is shield recharging enabled?
            if (health > 0 && useShielding && shieldingRechargeRate > 0)
            {
                // Can the shield be recharged?
                if (shieldingRechargeDelay > shieldRechargeDelayTimer)
                {
                    shieldRechargeDelayTimer += Time.deltaTime;
                }
                else
                {
                    shieldHealth += shieldingRechargeRate * Time.deltaTime;

                    if (shieldHealth > shieldingAmount) { shieldHealth = shieldingAmount; }
                }
            }
        }

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Initialises data for the damage region. This does some precalculation to allow for performance improvements.
        /// </summary>
        public void Initialise()
        {
            // Check that the damage multipliers array exists
            if (damageMultipliersArray == null)
            {
                damageMultipliersArray = new float[] { 1f, 1f, 1f, 1f, 1f, 1f };
            }
            else
            {
                // Check that the damage multipliers array is of the correct length
                int damageMultipliersArrayLength = damageMultipliersArray.Length;
                if (damageMultipliersArrayLength != 6)
                {
                    // If it is not the correct length, resize it
                    // Convert the array into a list
                    List<float> tempDamageMultipliersList = new List<float>();
                    tempDamageMultipliersList.AddRange(damageMultipliersArray);
                    if (damageMultipliersArrayLength > 6)
                    {
                        // If we have too many items in the array, remove some
                        for (int i = damageMultipliersArrayLength; i > 6; i--) { tempDamageMultipliersList.RemoveAt(i - 1); }
                    }
                    else
                    {
                        // If we don't have enough items in the array, add some
                        for (int i = damageMultipliersArrayLength; i < 6; i++) { tempDamageMultipliersList.Add(1f); }
                    }
                    // Convert the list back into an array
                    damageMultipliersArray = tempDamageMultipliersList.ToArray();
                }
            }

            isDestroyed = false;
        }

        public void SetClassDefaults()
        {
            this.name = "Damage Region";
            this.isInvincible = false;
            this.startingHealth = 1f;
            this.Health = 100f;
            this.useShielding = false;
            this.shieldingDamageThreshold = 10f;
            this.shieldingAmount = 100f;
            this.ShieldHealth = 100f;
            this.shieldingRechargeRate = 0f;
            this.shieldingRechargeDelay = 10f;
            this.collisionDamageResistance = 10f;
            this.showInEditor = true;
            this.showMultipliersInEditor = false;
            this.s3dHumanBonePersist = new S3DHumanBonePersist();
            this.damageMultipliersArray = new float[] { 1f, 1f, 1f, 1f, 1f, 1f };

            // Get a unique GUID then convert it to a hash for efficient non-GC access.
            guidHash = S3DMath.GetHashCodeFromGuid();

            this.Initialise();
        }

        /// <summary>
        /// Returns the damage multiplier for damageType.
        /// The relative amount of damage a Type A-F projectile or beam will inflict on the character.
        /// When the Damage Type is Default, the damage multipliers are ignored.
        /// </summary>
        /// <param name="damageType"></param>
        /// <returns></returns>
        public float GetDamageMultiplier(DamageType damageType)
        {
            switch ((int)damageType)
            {
                // Hardcoded int values for performance
                // Default = 0
                case 0: return 1f;
                // Type A = 100
                case 100: return damageMultipliersArray[0];
                // Type B = 105
                case 105: return damageMultipliersArray[1];
                // Type C = 110
                case 110: return damageMultipliersArray[2];
                // Type D = 115
                case 115: return damageMultipliersArray[3];
                // Type E = 120
                case 120: return damageMultipliersArray[4];
                // Type F = 125
                case 125: return damageMultipliersArray[5];
                // Default case
                default: return 1f;
            }
        }

        /// <summary>
        /// Reset the health of the damage region back to its initial state
        /// </summary>
        public void ResetHealth()
        {
            health = startingHealth;
            shieldHealth = useShielding ? 1f : 0f;
        }

        /// <summary>
        /// Sets the damage multiplier for damageType to damageMultiplier.
        /// </summary>
        /// <param name="damageType"></param>
        /// <param name="damageMultiplier"></param>
        public void SetDamageMultiplier(DamageType damageType, float damageMultiplier)
        {
            switch ((int)damageType)
            {
                // Hardcoded int values for performance
                // Type A = 100
                case 100: damageMultipliersArray[0] = damageMultiplier; break;
                // Type B = 105
                case 105: damageMultipliersArray[1] = damageMultiplier; break;
                // Type C = 110
                case 110: damageMultipliersArray[2] = damageMultiplier; break;
                // Type D = 115
                case 115: damageMultipliersArray[3] = damageMultiplier; break;
                // Type E = 120
                case 120: damageMultipliersArray[4] = damageMultiplier; break;
                // Type F = 125
                case 125: damageMultipliersArray[5] = damageMultiplier; break;
            }
        }

        /// <summary>
        /// Set the health of the damage  region
        /// </summary>
        /// <param name="newHealthValue"></param>
        public void SetHealth (float newHealthValue)
        {
            if (newHealthValue < 0.0001f) { health = 0f; }
            else if (newHealthValue > 100f) { health = 1f; }
            else { health = newHealthValue / 100f; }
        }

        /// <summary>
        /// Set the shield health of the damage region.
        /// Setting the value to less than 0 will indicate it is inactive.
        /// The max value is clamped at shieldAmount.
        /// </summary>
        /// <param name="newHealthValue"></param>
        public void SetShieldHealth (float newHealthValue)
        {
            if (newHealthValue < 0f) { shieldHealth = -0.1f; }
            else if (newHealthValue > shieldingAmount) { shieldHealth = shieldingAmount; }
            else { shieldHealth = newHealthValue; }
        }

        #endregion
    }
}