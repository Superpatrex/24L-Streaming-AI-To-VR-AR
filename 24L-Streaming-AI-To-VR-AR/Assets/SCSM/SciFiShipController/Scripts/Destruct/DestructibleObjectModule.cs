using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This module can be used to trigger a DestructModule when the health of the object reaches 0.
    /// For a simplified solution, see SSCTargetable.
    /// IMPORTANT: If you want a ship, a ship’s damage regions, or a surface turret to take damage,
    /// use the features included in the Ship Control Module or the Surface Turret Module. This
    /// module is to be used with regular gameobjects that don’t include those components.
    /// Examples could include buildings or destructible props.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Object Components/Destructive Object Module")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SciFiShipController.DamageReceiver))]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class DestructibleObjectModule : MonoBehaviour
    {
        #region Public Variables
        public bool initialiseOnStart = false;

        /// <summary>
        /// How much health the object has initially
        /// </summary>
        public float startingHealth = 100f;

        /// <summary>
        /// Whether this object uses shielding. Up until a point, shielding protects the object from damage 
        /// </summary>
        public bool useShielding = false;
        /// <summary>
        /// Damage below this value will not affect the shield or the object's health while the shield is still active 
        /// (i.e. until the shield has absorbed damage more than or equal to the shieldingAmount value from damage events above the 
        /// damage threshold). Only relevant if useShielding is enabled.
        /// </summary>
        public float shieldingDamageThreshold = 10f;
        /// <summary>
        /// How much damage the shield can absorb before it ceases to protect the object from damage.  Only relevant if 
        /// useShielding is enabled.
        /// </summary>
        public float shieldingAmount = 100f;

        /// <summary>
        /// When useShielding is true, this is the rate per second that a shield will recharge (default = 0)
        /// </summary>
        [Range(0f, 100f)] public float shieldingRechargeRate = 0f;

        /// <summary>
        /// When useShielding is true, and shieldingRechargeRate is greater than 0, this is the delay, in seconds,
        /// between when damage occurs to a shield and it begins to recharge.
        /// </summary>
        [Range(0f, 300f)] public float shieldingRechargeDelay = 10f;

        /// <summary>
        /// The sound or particle FX used when object is destroyed
        /// If you modify this, call ReinitialiseEffects().
        /// </summary>
        public EffectsModule destructionEffectsObject;

        /// <summary>
        /// The offset in the forward direction, from the objects gameobject, that the destruction effect is instantiated.
        /// </summary>
        public Vector3 destructionEffectsOffset = Vector3.zero;

        /// <summary>
        /// This is used when you want pre-build fragments of the object to explode out from the object position when it is destroyed.
        /// If you modify this, call ReinitialiseDestructObjects().
        /// </summary>
        public DestructModule destructObject = null;

        /// <summary>
        /// The offset in the forward direction, from the objects gameobject, that the destruct module is instantiated.
        /// </summary>
        public Vector3 destructObjectOffset = Vector3.zero;

        /// <summary>
        /// The relative size of the blip on the radar mini-map
        /// Must be between 1 and 5 inclusive.
        /// </summary>
        [Range(1, 5)] public byte radarBlipSize = 1;

        #endregion

        #region Public Properties

        /// <summary>
        /// The current health value of this object.
        /// </summary>
        public float Health
        {
            get { return health; }
            set
            {
                // Update the health value
                health = value < 0f ? 0f : value;

                if (isInitialised && health == 0f)
                {
                    DestructObject();
                }
            }
        }

        /// <summary>
        /// [READONLY]
        /// Normalised (0.0 – 1.0) value of the health of the object.
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
        /// [READONLY] Has the module been initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// [READONLY] The number used by the SSCRadar system to identify this object at a point in time.
        /// This should not be stored across frames and is updated as required by the system.
        /// </summary>
        public int RadarId { get { return radarItemIndex; } }

        /// <summary>
        /// The current health value of this object's shield.
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
        /// Normalised (0.0 – 1.0) value of the shield for this object. If useShielding is false, it will
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

        #endregion

        #region Private or Internal variables

        private bool isInitialised = false;
        private DamageReceiver damageReceiver = null;

        private float health;
        private float shieldHealth;

        /// <summary>
        /// Whether damage type multipliers are used when calculating damage from projectiles.
        /// </summary>
        [SerializeField] private bool useDamageMultipliers = false;

        /// <summary>
        /// The array of damage multipliers for this object.
        /// </summary>
        [SerializeField] private float[] damageMultipliersArray;

        /// <summary>
        /// [INTERNAL USE ONLY] Instead, call EnableRadar() or DisableRadar().
        /// </summary>
        [SerializeField] private bool isRadarEnabled = false;

        /// <summary>
        /// The faction or alliance the object belongs to. This can be used to identify if a object is friend or foe.  Neutral = 0.
        /// </summary>
        [SerializeField] private int factionId = 0;

        /// <summary>
        /// Although normally representing a squadron of ships, this can be used on a gameobjects to group it with other things in your scene
        /// </summary>
        [SerializeField] private int squadronId = -1;

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
        /// Flag for when the object destroy "event" has been actioned after health reaches 0
        /// </summary>
        [System.NonSerialized] internal bool isDestroyed = false;

        [System.NonSerialized] private SSCManager sscManager = null;

        // Radar variables
        [System.NonSerialized] private SSCRadar sscRadar = null;
        [System.NonSerialized] internal int radarItemIndex = -1;
        //[System.NonSerialized] internal SSCRadarPacket sscRadarPacket;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] internal float shieldRechargeDelayTimer;

        /// <summary>
        /// The ID number for this object's destruction effects object prefab (as assigned by the Ship Controller Manager in the scene).
        /// This is the index in the SSCManager effectsObjectTemplatesList.
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] public int effectsObjectPrefabID;

        /// <summary>
        /// Flag for whether the destruction effects object has been instantiated.
        /// [INTERNAL USE ONLY]
        /// </summary>
        [System.NonSerialized] public bool isDestructionEffectsObjectInstantiated;

        #endregion

        #region Initialise methods
        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Update Methods

        private void Update()
        {
            if (isInitialised)
            {
                CheckShieldRecharge();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Add or remove this gameobject from radar.
        /// </summary>
        /// <param name="isEnabled"></param>
        private void EnableOrDisableRadar(bool isEnabled)
        {
            if (isEnabled)
            {
                if (isInitialised && radarItemIndex == -1)
                {
                    // Create as a RadarItemType.GameObject
                    if (sscRadar == null) { sscRadar = SSCRadar.GetOrCreateRadar(); }
                    if (sscRadar != null) { radarItemIndex = sscRadar.EnableRadar(gameObject, transform.position, factionId, squadronId, 0, radarBlipSize); }

                    isRadarEnabled = radarItemIndex >= 0;
                }
            }
            else
            {
                if (isInitialised && isRadarEnabled && radarItemIndex >= 0)
                {
                    sscRadar.DisableRadar(radarItemIndex);
                }
                isRadarEnabled = false;
            }
        }

        /// <summary>
        /// Recharge the shield if required
        /// </summary>
        private void CheckShieldRecharge()
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

        #region Public API Methods

        /// <summary>
        /// Apply damage to the object. If Use Damage Multipliers is enabled, you can optionally pass in the DamageTye.
        /// </summary>
        /// <param name="damageAmount"></param>
        /// <param name="damageType"></param>
        public void ApplyDamage (float damageAmount, ProjectileModule.DamageType damageType = ProjectileModule.DamageType.Default)
        {
            float actualDamage = damageAmount;
            bool objectDamaged = true;

            // Modify damage dealt based on relevant damage multipliers
            if (useDamageMultipliers)
            {
                actualDamage *= GetDamageMultiplier(damageType);
            }

            // Determine whether shielding is active for this object
            if (useShielding && shieldHealth > 0f)
            {
                // Set the shielding to active
                objectDamaged = false;
                // Only do damage to the shield if the damage amount is above the shielding threshold
                if (actualDamage >= shieldingDamageThreshold)
                {
                    shieldHealth -= actualDamage;
                    shieldRechargeDelayTimer = 0f;
                    // If this damage destroys the shielding entirely...
                    if (shieldHealth <= 0f)
                    {
                        // Get the residual damage value
                        actualDamage = -shieldHealth;
                        // Set the shielding to inactive
                        objectDamaged = true;
                        shieldHealth = -0.01f;
                    }
                }
            }

            if (objectDamaged)
            {
                // Reduce health of object itself
                Health -= actualDamage;
            }
        }

        /// <summary>
        /// This routine is called by our damage receiver when a projectile or beam hits our object
        /// </summary>
        /// <param name="callbackOnObjectHitParameters"></param>
        public void ApplyDamage (CallbackOnObjectHitParameters callbackOnObjectHitParameters)
        {
            if (callbackOnObjectHitParameters.hitInfo.transform != null)
            {
                ProjectileModule projectile = callbackOnObjectHitParameters.projectilePrefab;

                if (projectile != null)
                {
                    ApplyDamage(projectile.damageAmount, projectile.damageType);                   
                }
                // Should have been a beam weapon that fired at the object
                else
                {
                    BeamModule beam = callbackOnObjectHitParameters.beamPrefab;

                    if (beam != null)
                    {
                        ApplyDamage(callbackOnObjectHitParameters.damageAmount, beam.damageType);
                    }
                    else
                    {
                        // if we don't need to know what hit the object, simply reduce the health
                        ApplyDamage(callbackOnObjectHitParameters.damageAmount);
                    }
                }
            }
        }

        /// <summary>
        /// Add health to the object. If isAffectShield is true, and the health reaches the maximum
        /// configured, excess health will be applied to the shield.
        /// To incur damage use the ApplyDamage(..).
        /// </summary>
        /// <param name="healthAmount"></param>
        /// <param name="isAffectShield"></param>
        public void AddHealth (float healthAmount, bool isAffectShield)
        {
            if (healthAmount > 0f)
            {
                if (health < 0f) { health = healthAmount; }
                else { health += healthAmount; }

                if (health > startingHealth)
                {
                    if (isAffectShield && useShielding)
                    {
                        float newShieldHealth = 0f;
                        // When shielding is -ve (e.g. -0.01 when it has been used up) set the shielding amount rather than adding it
                        if (shieldHealth < 0f) { newShieldHealth = health - startingHealth; }
                        else { newShieldHealth = shieldHealth + health - startingHealth; }

                        // Cap shielding to maximum permitted.
                        if (newShieldHealth > shieldingAmount) { newShieldHealth = shieldingAmount; }

                        shieldHealth = newShieldHealth;
                    }

                    // Cap health to maximum permitted
                    health = startingHealth;
                }
            }
        }

        /// <summary>
        /// Disable radar for this object. If you want to change the visibility to other radar consumers,
        /// consider calling SetRadarVisibility(..) rather than disabling the radar and (later)
        /// calling EnableRadar() again. This will be automatically called by DestructObject().
        /// </summary>
        public void DisableRadar()
        {
            EnableOrDisableRadar(false);
        }

        // Enable radar for this object. It will be visible on radar to others in the scene.
        public void EnableRadar()
        {
            EnableOrDisableRadar(true);
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
        /// Reinitialises variables required for Destruct Module.
        /// Call after modifying the destructObject.
        /// </summary>
        public void ReinitialiseDestructObjects()
        {
            if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(gameObject.scene.handle); }
            if (sscManager != null)
            {
                // Initialise destruct modules
                sscManager.UpdateDestructObjects(this);
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("DestructibleObjectModule.ReinitialiseDestructObjects Warning: could not find SSCManager to update effects.");
            }
            #endif
        }

        /// <summary>
        /// Reinitialises variables required for effects of the Destructible Object Module.
        /// Call after modifying any effect data for this module.
        /// </summary>
        public void ReinitialiseEffects()
        {
            if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(gameObject.scene.handle); }
            if (sscManager != null)
            {
                // Initialise effects objects
                sscManager.UpdateEffects(this);
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("DestructibleObjectModule.ReinitialiseEffects Warning: could not find SSCManager to update effects.");
            }
            #endif
        }

        /// <summary>
        /// Reset the health of the object back to initial values
        /// </summary>
        public void ResetHealth()
        {
            // Reset health value for the object
            health = startingHealth;

            // Reset shield health value for the object
            shieldHealth = shieldingAmount;

            shieldRechargeDelayTimer = 0f;

            isDestructObjectActivated = false;
            isDestroyed = false;
        }

        /// <summary>
        /// Sets the damage multiplier for damageType to damageMultiplier.
        /// </summary>
        /// <param name="damageType"></param>
        /// <param name="damageMultiplier"></param>
        public void SetDamageMultiplier (ProjectileModule.DamageType damageType, float damageMultiplier)
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
        /// If radar is enabled for this object, set its visibility to radar.
        /// </summary>
        /// <param name="isVisible"></param>
        public void SetRadarVisibility (bool isVisible)
        {
            if (isInitialised && isRadarEnabled && radarItemIndex >= 0)
            {
                sscRadar.SetVisibility(radarItemIndex, isVisible);
            }
        }

        /// <summary>
        /// Set the Squadron Id for the object. If radar is enabled, this will also update the radar.
        /// </summary>
        /// <param name="newSquadronId"></param>
        public void SetSquadronId (int newSquadronId)
        {
            if (squadronId != newSquadronId)
            {
                squadronId = newSquadronId;

                // Do we need to update the radar item?
                if (isInitialised && isRadarEnabled && radarItemIndex >= 0)
                {
                    sscRadar.SetSquardronId(radarItemIndex, squadronId);
                }
            }
        }

        /// <summary>
        /// Set the Faction Id for the object. If radar is enabled, this will also update the radar.
        /// </summary>
        /// <param name="newFactionId"></param>
        public void SetFactionId (int newFactionId)
        {
            if (factionId != newFactionId)
            {
                factionId = newFactionId;

                // Do we need to update the radar item?
                if (isInitialised && isRadarEnabled && radarItemIndex >= 0)
                {
                    sscRadar.SetFactionId(radarItemIndex, factionId);
                }
            }
        }

        /// <summary>
        /// Verify that the damage multiplier array is correctly sized
        /// </summary>
        public void VerifyMultiplierArray()
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
        }

        #endregion

        #region Public Virtual and Protected API Methods

        /// <summary>
        /// Initialises the DestructibleObjectModule. If you wish to override this in a child (inherited)
        /// class you almost always will want to call the base method first.
        /// public override void Initialise()
        /// {
        ///     base.Initialise();
        ///     // Do stuff here
        /// }
        /// </summary>
        public virtual void Initialise()
        {
            if (!isInitialised)
            {
                damageReceiver = GetComponent<DamageReceiver>();

                if (damageReceiver == null) { damageReceiver = gameObject.AddComponent<DamageReceiver>(); }

                if (damageReceiver == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: DestructibleOjectModule.Initialise() - could not find or add DamageReceiver component to " + name);
                    #endif
                }
                else
                {
                    VerifyMultiplierArray();

                    ResetHealth();

                    // Get a reference to the Ship Controller Manager instance
                    sscManager = SSCManager.GetOrCreateManager(gameObject.scene.handle);

                    // Not sure if we need these or just the ReinitialiseEffects()
                    effectsObjectPrefabID = -1;
                    isDestructionEffectsObjectInstantiated = false;
                    destructionEffectItemKey = new SSCEffectItemKey(-1, -1, 0);

                    // Not sure if we need these or just the ReinitialiseDestructObjects()
                    destructObjectPrefabID = -1;
                    isDestructObjectActivated = false;
                    destructItemKey = new SSCDestructItemKey(-1, -1, 0);

                    // Initialise effects objects
                    ReinitialiseEffects();

                    // Initialise destruct modules
                    ReinitialiseDestructObjects();

                    // Get notified when the object is hit by a projectile or beam
                    damageReceiver.callbackOnHit = ApplyDamage;

                    isInitialised = true;
                }

                if (isInitialised)
                {
                    EnableOrDisableRadar(isRadarEnabled);
                }
            }
        }

        /// <summary>
        /// Destroys the object.
        /// If you wish to override this in a child (inherited) class you almost
        /// always will want to call the base after doing your actions.
        /// public override void DestructObject()
        /// {
        ///     // Do stuff here
        ///     base.DestructObject();
        /// }
        /// </summary>
        public virtual void DestructObject()
        {
            if (isInitialised)
            {
                if (isRadarEnabled) { EnableOrDisableRadar(false); }

                #region Instantiate the destruction effects prefab
                if (destructionEffectsObject != null)
                {
                    InstantiateEffectsObjectParameters ieParms = new InstantiateEffectsObjectParameters
                    {
                        effectsObjectPrefabID = effectsObjectPrefabID,
                        position = transform.position + (transform.rotation * destructionEffectsOffset),
                        rotation = transform.rotation
                    };

                    sscManager.InstantiateEffectsObject(ref ieParms);
                }
                #endregion

                #region Instantiate the destruct module prefab
                if (!isDestructObjectActivated && destructObjectPrefabID >= 0 && destructObject != null)
                {
                    // Turn off all colliders. As we are going to destroy the gameobject,
                    // we can simply deactivate it.
                    gameObject.SetActive(false);

                    // Instantiate the region destruct prefab
                    InstantiateDestructParameters dstParms = new InstantiateDestructParameters
                    {
                        destructPrefabID = destructObjectPrefabID,
                        position = transform.position + (transform.rotation * destructObjectOffset),
                        rotation = transform.rotation,
                        explosionPowerFactor = 1f,
                        explosionRadiusFactor = 1f
                    };

                    // Keep track of the DestructModule instance that was instantiated for this object
                    // In our case, we don't really need this as we're going to destroy this script. However,
                    // it may be useful if we want to do something else in the future.
                    if (sscManager.InstantiateDestruct(ref dstParms) != null)
                    {
                        destructItemKey = new SSCDestructItemKey(destructObjectPrefabID, dstParms.destructPoolListIndex, dstParms.destructSequenceNumber);
                    }
                    
                    isDestructObjectActivated = true;
                }
                #endregion

                // Destroy the original object
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Destroys the object after a delay period in seconds
        /// </summary>
        /// <param name="delayTime"></param>
        public virtual void DestructObjectDelayed (float delayDuration)
        {
            if (delayDuration > 0f)
            {
                Invoke("DestructObject", delayDuration);
            }
            else { DestructObject(); }
        }

        #endregion

    }
}