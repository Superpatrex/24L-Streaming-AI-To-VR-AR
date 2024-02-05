using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Attach a DamageReceiver component to an object in your scene, then use your
    /// own code to process the damage received after being hit by a projectile or
    /// (laser) beam.
    /// This is only sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own
    /// namespace.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Object Damage")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    [RequireComponent(typeof(SciFiShipController.DamageReceiver))]
    public class SampleObjectDamage : MonoBehaviour
    {
        #region Public Variables
        public float health = 100;
        #endregion

        #region Initialisation Methods

        void Start()
        {
            DamageReceiver damageReceiver = GetComponent<DamageReceiver>();
            if (damageReceiver != null)
            {
                damageReceiver.callbackOnHit = TakeDamage;
            }
        }

        #endregion

        #region Public Member Methods

        /// <summary>
        /// This routine is called by Sci-Fi Ship Controller when a projectile hits it.
        /// </summary>
        /// <param name="callbackOnObjectHitParameters"></param>
        public void TakeDamage(CallbackOnObjectHitParameters callbackOnObjectHitParameters)
        {
            var projectile = callbackOnObjectHitParameters.projectilePrefab;
            
            if (projectile != null)
            {
                health -= projectile.damageAmount;

                // Uncomment if you want to debug in the editor
                //#if UNITY_EDITOR
                //Debug.Log("Projectile: " + projectile.name + " hit " + gameObject.name +  " with damage amount of " + projectile.damageAmount + " Health: " + health);
                //#endif
            }
            // must have been a beam weapon that fired at the object
            else
            {
                // if we don't need to know what hit the object, simply reduce the health
                health -= callbackOnObjectHitParameters.damageAmount;

                // Uncomment if you want to debug in the editor
                //#if UNITY_EDITOR
                //var beam = callbackOnObjectHitParameters.beamPrefab;
                //Debug.Log("Beam: " + beam.name + " hit " + gameObject.name +  " with damage amount of " + callbackOnObjectHitParameters.damageAmount + " Health: " + health);
                //#endif
            }

            if (health < 0f)
            {
                Destroy(gameObject);
            }
        }

        #endregion
    }
}