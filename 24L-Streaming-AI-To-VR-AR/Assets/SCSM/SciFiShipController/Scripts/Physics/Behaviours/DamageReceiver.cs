using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Attach to Collider that you want to receive damaage when SSC projectiles
    /// or beams hit it. Configure the callback to call your method whenever it is hit.
    /// This is useful for objects in the scene that are not Ships - like your own
    /// stationary or moving objects.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Object Components/Damage Receiver")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class DamageReceiver : MonoBehaviour
    {
        #region Public Delegates
        public delegate void CallbackOnHit(CallbackOnObjectHitParameters callbackOnObjectHitParameters);

        /// <summary>
        /// The name of the custom method that is called immediately
        /// after the object is hit by a projectile or beam. Your method must take 1
        /// parameter of type CallbackOnObjectHitParameters. This should be 
        /// a lightweight method to avoid performance issues. It could be used to 
        /// take damage on non-Sci-Fi Ship Controller assets in the scene.
        /// </summary>
        public CallbackOnHit callbackOnHit = null;
        #endregion
    }

    #region Public Structures

    /// <summary>
    /// Paramaters structure for CallbackOnHit (callback for DamageReceiver).
    /// We do not recommend keeping references to any fields within this structure.
    /// Use them in one frame, then discard them.
    /// </summary>
    public struct CallbackOnObjectHitParameters
    {
        /// <summary>
        /// Hit information for the raycast hit against the object.
        /// </summary>
        public RaycastHit hitInfo;
        /// <summary>
        /// Prefab for the projectile that hit the object.
        /// </summary>
        public ProjectileModule projectilePrefab;
        /// <summary>
        /// Prefab for the beam that hit the object
        /// </summary>
        public BeamModule beamPrefab;
        /// <summary>
        /// Amount of damage done by the projectile or beam.
        /// </summary>
        public float damageAmount;
        /// <summary>
        /// The squadron ID of the ship that fired the projectile or beam.
        /// </summary>
        public int sourceSquadronId;
    };

    #endregion
}