using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Simple component that can be added to any non-SSC object to make it targetable (visible on radar).
    /// For a more comprehensive solution, see DestructibleObjectModule.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Object Components/Targetable")]
    [DisallowMultipleComponent]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SSCTargetable : MonoBehaviour
    {
        #region Public Variables
        public bool initialiseOnStart = false;

        [Tooltip("How much health the object has initially")]
        public float startingHealth = 100f;

        [Tooltip("Should the object be destroyed if health falls to 0?")]
        public bool destroyOnNoHealth = true;

        [Tooltip("When first initialised, should this object be visible to radar?")]
        public bool isVisibleToRadarOnStart = true;

        [Tooltip("The relative size of the blip on the radar mini-map. Must be between 1 and 5 inclusive.")]
        [Range(1, 5)] public byte radarBlipSize = 1;

        /// <summary>
        /// The faction or alliance the object belongs to. This can be used to identify if a object is friend or foe.  Neutral = 0.
        /// </summary>
        [SerializeField] private int factionId = 0;

        /// <summary>
        /// Although normally representing a squadron of ships, this can be used on a gameobjects to group it with other things in your scene
        /// </summary>
        [SerializeField] private int squadronId = -1;

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

                if (isInitialised && health == 0f && destroyOnNoHealth)
                {
                    Destroy(gameObject);
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

        #endregion

        #region Private Variables
        private bool isInitialised = false;
        private DamageReceiver damageReceiver = null;
        private SSCRadar sscRadar = null;
        private bool isRadarEnabled = false;
        private float health = 0f;
        private int radarItemIndex = -1;
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Events

        private void OnDestroy()
        {
            EnableOrDisableRadar(false);
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

        #endregion

        #region Public Virtual and Protected API Methods

        /// <summary>
        /// Add health to the object. To incur damage use the ApplyDamage(..).
        /// </summary>
        /// <param name="healthAmount"></param>
        public virtual void AddHealth (float healthAmount)
        {
            if (healthAmount > 0f)
            {
                if (health < 0f) { health = healthAmount; }
                else { health += healthAmount; }

                if (health > startingHealth)
                {
                    // Cap health to maximum permitted
                    health = startingHealth;
                }
            }
        }

        /// <summary>
        /// Apply damage to the object.
        /// </summary>
        /// <param name="damageAmount"></param>
        public virtual void ApplyDamage (float damageAmount)
        {
            if (damageAmount > 0f)
            {
                // Reduce health of object itself
                Health -= damageAmount;
            }
        }

        /// <summary>
        /// This routine is called by our damage receiver when a projectile or beam hits our object
        /// </summary>
        /// <param name="callbackOnObjectHitParameters"></param>
        public virtual void ApplyDamage (CallbackOnObjectHitParameters callbackOnObjectHitParameters)
        {
            if (callbackOnObjectHitParameters.hitInfo.transform != null)
            {
                ProjectileModule projectile = callbackOnObjectHitParameters.projectilePrefab;

                if (projectile != null)
                {
                    ApplyDamage(projectile.damageAmount);
                }
                // Should have been a beam weapon that fired at the object
                else
                {
                    BeamModule beam = callbackOnObjectHitParameters.beamPrefab;

                    if (beam != null)
                    {
                        ApplyDamage(callbackOnObjectHitParameters.damageAmount);
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
        /// Initialises the SSCTargetable. If you wish to override this in a child (inherited)
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
                // Make sure this is NOT a ship or surface turret
                ShipControlModule shipControlModule = GetComponent<ShipControlModule>();
                SurfaceTurretModule surfaceTurretModule = GetComponent<SurfaceTurretModule>();
                DestructibleObjectModule destructibleObjectModule = GetComponent<DestructibleObjectModule>();

                if (shipControlModule != null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SSCTargetable.Initialise() - this component is not compatible with ships. Instead, use Identification on the Combat tab of " + name);
                    #endif
                }
                else if (surfaceTurretModule != null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SSCTargetable.Initialise() - this component is not compatible with surface turrrets. Instead, use Is Visible to Radar on the Surface Turret Module of " + name);
                    #endif
                }
                else if (destructibleObjectModule != null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SSCTargetable.Initialise() - this component is not compatible with DestructibleObjectModule. Instead, use Visible to Radar on the DestructibleObjectModule of " + name);
                    #endif
                }
                else
                {
                    damageReceiver = GetComponent<DamageReceiver>();
                    sscRadar = SSCRadar.GetOrCreateRadar();

                    if (damageReceiver == null) { damageReceiver = gameObject.AddComponent<DamageReceiver>(); }

                    if (damageReceiver == null)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("ERROR: SSCTargetable.Initialise() - could not find or add DamageReceiver component to " + name);
                        #endif
                    }
                    else
                    {
                        health = startingHealth;

                        // Get notified when we take damage
                        damageReceiver.callbackOnHit = ApplyDamage;

                        EnableOrDisableRadar(isVisibleToRadarOnStart);

                        isInitialised = true;
                    }
                }
            }
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Disable radar for this object. If you want to change the visibility to other radar consumers,
        /// consider calling SetRadarVisibility(..) rather than disabling the radar and (later)
        /// calling EnableRadar() again. This will be automatically called by when the object is destroyed.
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
        /// Reset the health of the object back to initial values
        /// </summary>
        public void ResetHealth()
        {
            // Reset health value for the object
            health = startingHealth;
        }

        /// <summary>
        /// If radar is enabled for this object, set its visibility to radar.
        /// </summary>
        /// <param name="isVisible"></param>
        public void SetRadarVisibility(bool isVisible)
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
        public void SetSquadronId(int newSquadronId)
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
        public void SetFactionId(int newFactionId)
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

        #endregion
    }
}