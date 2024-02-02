using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class containing data for a damage region.
    /// </summary>
    [System.Serializable]
    public class DamageRegion
    {
        #region Public variables and properties

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
        /// Position of this damage region in local space relative to the pivot point of the ship.
        /// </summary>
        public Vector3 relativePosition;
        /// <summary>
        /// Size of this damage region (in metres cubed) in local space.
        /// </summary>
        public Vector3 size;

        /// <summary>
        /// The starting health value of this damage region.
        /// </summary>
        public float startingHealth;

        private float health;
        /// <summary>
        /// The current health value of this damage region.
        /// NOTE: Health can fall below zero if health is low
        /// when damage is applied.
        /// </summary>
        public float Health
        {
            get { return health; }
            set
            {
                // Update the health value
                health = value;
            }
        }

        /// <summary>
        /// [READONLY]
        /// Normalised (0.0 – 1.0) value of the health of the damage region.
        /// </summary>
        public float HealthNormalised
        {
            get
            {
                float _healthN = startingHealth == 0f ? 0f : health / startingHealth;
                if (_healthN > 1f) { return 1f; }
                else if (_healthN < 0f) { return 0f; }
                else { return _healthN; }
            }
        }

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

        private float shieldHealth;
        /// <summary>
        /// The current health value of this damage region's shield.
        /// When a shield is destroyed, its value is set to -0.01.
        /// </summary>
        public float ShieldHealth
        {
            get { return shieldHealth; }
            set
            {
                // Update the health value
                shieldHealth = value;
            }
        }

        /// <summary>
        /// [READONLY]
        /// Normalised (0.0 – 1.0) value of the shield for this damage region. If useShielding is false, it will
        /// always return 0.
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
        /// Whether the damage region is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// Whether the damage region is shown as selected in the scene view of the editor.
        /// </summary>
        public bool selectedInSceneView;

        /// <summary>
        /// Whether the gizmos for this damage region are shown in the scene view of the editor.
        /// </summary>
        public bool showGizmosInSceneView;

        /// <summary>
        /// The sound or particle FX used when region is destroyed (if this is the main damage region, this will occur when the
        /// ship is destroyed). It should automatically be despawned when the whole ship is despawned.
        /// If you modify this, call ReinitialiseShipProjectilesAndEffects() on the ShipControlModule.
        /// </summary>
        public EffectsModule destructionEffectsObject;

        /// <summary>
        /// Should the destruction EffectsObject move as the ship moves?
        /// </summary>
        public bool isMoveDestructionEffectsObject;

        /// <summary>
        /// This is used when you want pre-build fragments of the ship to explode out from the ship position when it is destroyed.
        /// If you modify this, call UpdateDestructObjects() on the ShipControlModule.
        /// </summary>
        public DestructModule destructObject;

        /// <summary>
        /// The child transform of the ship that contains the mesh(es) for this local region. If set, it is disabled when the region's health reaches 0.
        /// Setting this can also assist other weapons in the scene with determining Line-of-Sight to a damage region.
        /// NOTE: It is ignored for the mainRegion.
        /// </summary>
        public Transform regionChildTransform;

        /// <summary>
        /// [INTERNAL USE ONLY] Instead, call shipControlModule.EnableRadar(damageRegion) or DisableRadar(damageRegion).
        /// </summary>
        public bool isRadarEnabled;

        /// <summary>
        /// [READONLY] The number used by the SSCRadar system to identify this ship's damage region at a point in time.
        /// This should not be stored across frames and is updated as required by the system.
        /// </summary>
        public int RadarId { get { return radarItemIndex; } }

        /// <summary>
        /// The ID number for this damage region's destruction effects object prefab (as assigned by the Ship Controller Manager in the scene).
        /// This is the index in the SSCManager effectsObjectTemplatesList.
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] public int effectsObjectPrefabID;

        /// <summary>
        /// Flag for whether the destruction effects object has been instantiated.
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] public bool isDestructionEffectsObjectInstantiated;

        /// <summary>
        /// Hashed GUID code to uniquely identify a damage region on a ship. Used instead of the
        /// name to avoid GC when comparing two damage regions.
        /// [INTERNAL USE ONLY]
        /// </summary>
        public int guidHash;

        #endregion

        #region Private or Internal variables

        /// <summary>
        /// The array of damage multipliers for this ship.
        /// </summary>
        [SerializeField] private float[] damageMultipliersArray;

        /// <summary>
        /// This identifies the destructionEffectsObject instance that may have been instantiated.
        /// </summary>
        [System.NonSerialized] internal SSCEffectItemKey destructionEffectItemKey;

        /// <summary>
        /// The ID number for this damage region's destruct prefab (as assigned by the SSCManager in the scene).
        /// This is the index in the SSCManager destructTemplateList.
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] internal int destructObjectPrefabID;

        /// <summary>
        /// Flag for whether the destruct object has been activated.
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] internal bool isDestructObjectActivated;

        /// <summary>
        /// This identifies the destruct object instance that may have been instantiated.
        /// </summary>
        [System.NonSerialized] internal SSCDestructItemKey destructItemKey;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// Flag for when the region destroy "event" has been actioned after health reaches 0
        /// </summary>
        [System.NonSerialized] internal bool isDestroyed;

        // Radar variables
        [System.NonSerialized] internal int radarItemIndex;
        [System.NonSerialized] internal SSCRadarPacket sscRadarPacket;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] internal float shieldRechargeDelayTimer;

        #endregion

        #region Class constructors

        public DamageRegion()
        {
            SetClassDefaults();
        }

        // Copy constructor
        public DamageRegion(DamageRegion damageRegion)
        {
            if (damageRegion == null) { SetClassDefaults(); }
            else
            {
                this.name = damageRegion.name;
                this.isInvincible = damageRegion.isInvincible;
                this.relativePosition = damageRegion.relativePosition;
                this.size = damageRegion.size;
                this.startingHealth = damageRegion.startingHealth;
                this.Health = damageRegion.Health;
                this.useShielding = damageRegion.useShielding;
                this.shieldingDamageThreshold = damageRegion.shieldingDamageThreshold;
                this.shieldingAmount = damageRegion.shieldingAmount;
                this.ShieldHealth = damageRegion.ShieldHealth;
                this.shieldingRechargeRate = damageRegion.shieldingRechargeRate;
                this.shieldingRechargeDelay = damageRegion.shieldingRechargeDelay;
                this.collisionDamageResistance = damageRegion.collisionDamageResistance;
                this.showInEditor = damageRegion.showInEditor;
                this.selectedInSceneView = damageRegion.selectedInSceneView;
                this.showGizmosInSceneView = damageRegion.showGizmosInSceneView;
                this.destructionEffectsObject = damageRegion.destructionEffectsObject;
                this.isMoveDestructionEffectsObject = damageRegion.isMoveDestructionEffectsObject;
                this.destructObject = damageRegion.destructObject;
                this.regionChildTransform = damageRegion.regionChildTransform;
                this.isRadarEnabled = damageRegion.isRadarEnabled;
                //this.effectsObjectPrefabID = damageRegion.effectsObjectPrefabID;
                //this.isDestructionEffectsObjectInstantiated = damageRegion.isDestructionEffectsObjectInstantiated;
                this.guidHash = damageRegion.guidHash;
                this.Initialise();
            }
        }

        #endregion

        #region Internal Non-Static Methods

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

        #region Public Non-Static Methods

        public void SetClassDefaults()
        {
            this.name = "Damage Region";
            this.isInvincible = false;
            this.relativePosition = Vector3.zero;
            this.size = Vector3.one * 10f;
            this.startingHealth = 100f;
            this.Health = 100f;
            this.useShielding = false;
            this.shieldingDamageThreshold = 10f;
            this.shieldingAmount = 100f;
            this.ShieldHealth = 100f;
            this.shieldingRechargeRate = 0f;
            this.shieldingRechargeDelay = 10f;
            this.collisionDamageResistance = 10f;
            this.showInEditor = true;
            this.selectedInSceneView = false;
            this.showGizmosInSceneView = true;
            this.destructionEffectsObject = null;
            this.isMoveDestructionEffectsObject = false;
            this.destructObject = null;
            this.regionChildTransform = null;
            this.isRadarEnabled = false;
            //this.effectsObjectPrefabID = -1;
            //this.isDestructionEffectsObjectInstantiated = false;
            this.damageMultipliersArray = new float[] { 1f, 1f, 1f, 1f, 1f, 1f };

            // Get a unique GUID then convert it to a hash for efficient non-GC access.
            guidHash = SSCMath.GetHashCodeFromGuid();

            this.Initialise();
        }

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

            effectsObjectPrefabID = -1;
            isDestructionEffectsObjectInstantiated = false;
            destructionEffectItemKey = new SSCEffectItemKey(-1, -1, 0);

            destructObjectPrefabID = -1;
            isDestructObjectActivated = false;
            destructItemKey = new SSCDestructItemKey(-1, -1, 0);

            isDestroyed = false;

            radarItemIndex = -1;
        }

        /// <summary>
        /// Returns the damage multiplier for damageType.
        /// </summary>
        /// <param name="damageType"></param>
        /// <returns></returns>
        public float GetDamageMultiplier (ProjectileModule.DamageType damageType)
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
        /// Is the hitPoint within the bounds of the region?
        /// Use this when calculating if a projectile or beam hits
        /// the damage region. See also ship.IsPointInDamageRegion(damageRegion, worldSpacePoint).
        /// USAGE: damageRegion.IsHit(ship.TransformInverseRotation * (damagePositionWS - ship.TransformPosition) )
        /// </summary>
        /// <param name="hitPoint"></param>
        /// <returns></returns>
        public bool IsHit (Vector3 localDamagePosition)
        {
            // Then check whether the position is within the bounds or volume of this damage region
            // For some reason we need to slightly expand the damage region when the collider is exactly the same size.
            return localDamagePosition.x >= relativePosition.x - (size.x / 2) - 0.0001f &&
                localDamagePosition.x <= relativePosition.x + (size.x / 2) + 0.0001f &&
                localDamagePosition.y >= relativePosition.y - (size.y / 2) - 0.0001f &&
                localDamagePosition.y <= relativePosition.y + (size.y / 2) + 0.0001f &&
                localDamagePosition.z >= relativePosition.z - (size.z / 2) - 0.0001f &&
                localDamagePosition.z <= relativePosition.z + (size.z / 2) + 0.0001f;
        }

        /// <summary>
        /// Sets the damage multiplier for damageType to damageMultiplier.
        /// </summary>
        /// <param name="damageType"></param>
        /// <param name="damageMultiplier"></param>
        public void SetDamageMultiplier(ProjectileModule.DamageType damageType, float damageMultiplier)
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

        #endregion
    }
}
