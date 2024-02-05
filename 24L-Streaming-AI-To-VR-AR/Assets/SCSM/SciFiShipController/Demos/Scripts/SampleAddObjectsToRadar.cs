using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This simple sample script adds GameObjects to radar. If you hit them with projectiles
    /// or beam from a ship they will be destroyed and removed from radar.
    /// NOTE: This script does not show or query the radar.
    /// SETUP:
    /// 1. Create an empty GameObject in the scene
    /// 2. Add this script to the new GameObject
    /// This is only sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own
    /// namespace.
    /// See also: SampleHUDTargets.cs.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Add Objects To Radar")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleAddObjectsToRadar : MonoBehaviour
    {
        #region Public Variables and Properties
        // A faction of 0 means neutral.
        public int factionId = 2;

        // Usually squadrons only apply to ships, but you can use them to group objects together too.
        // -1 means the squadron is not set.
        public int squadronId = -1;

        public bool incrementFactionId = false;
        [Range(1, 10)] public int numberOfEnemy = 5;
        public float distanceApart = 25f;
        #endregion

        #region Private Variables
        private SSCRadar sscRadar = null;
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            #region Create some "enemy" and added them to radar
            sscRadar = SSCRadar.GetOrCreateRadar();

            if (sscRadar != null)
            {
                // Start from the gameobject position
                Vector3 locationWS = transform.position;

                // Create some "enemy"
                for (int eIdx = 0; eIdx < numberOfEnemy; eIdx++)
                {
                    GameObject enemyGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                    if (enemyGO != null)
                    {
                        enemyGO.name = "Enemy " + (eIdx + 1).ToString();
                        enemyGO.transform.position = locationWS + (transform.right * distanceApart * (eIdx + 1));
                        enemyGO.transform.localScale = Vector3.one * 3f;
                        enemyGO.GetComponent<Renderer>().material.color = new Color(1f, 1f / (eIdx + 2), 1f / (eIdx + 2));

                        // Let's get notified when our "enemy" is hit
                        DamageReceiver damageReceiver = enemyGO.AddComponent<DamageReceiver>();
                        damageReceiver.callbackOnHit = TakeDamage;

                        // Tell the radar system about this enemy gameobject
                        sscRadar.EnableRadar(enemyGO, enemyGO.transform.position, factionId, -1, 0, 5);

                        // Should each enemy get a different faction id?
                        if (incrementFactionId) { factionId++; }
                    }
                }
            }
            #endregion
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// This routine is called by Sci-Fi Ship Controller when a projectile or beam hits our enemy
        /// </summary>
        /// <param name="callbackOnObjectHitParameters"></param>
        public void TakeDamage(CallbackOnObjectHitParameters callbackOnObjectHitParameters)
        {
            // Rather than taking damage, immediately our "enemy" is hit, we will destroy it.
            if (callbackOnObjectHitParameters.hitInfo.transform != null)
            {
                // Get the radar item by using the hashcode of the GameObject
                // Then tell the radar system to stop tracking this item.
                sscRadar.RemoveItem(sscRadar.GetRadarItemIndexByHash(callbackOnObjectHitParameters.hitInfo.transform.gameObject.GetHashCode()));

                // Now we can destroy the actual gameobject
                Destroy(callbackOnObjectHitParameters.hitInfo.transform.gameObject);
            }
        }
        #endregion
    }
}
