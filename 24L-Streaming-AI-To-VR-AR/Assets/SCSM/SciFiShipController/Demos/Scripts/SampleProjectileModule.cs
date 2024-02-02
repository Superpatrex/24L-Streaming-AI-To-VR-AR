using UnityEngine;
using SciFiShipController;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace MyUniqueGame
{
    /// <summary>
    /// Demo script used to show how to create your own custom ProjectileModule component.
    /// WARNING: This is a DEMO script and is subject to change without notice during
    /// upgrades. This is just to show you how to do things in your own code.
    /// 1. The script should be attached to your projectile gameobject. Typically the
    ///    the projectile will be child object. See our ProjectileModule prefabs.
    /// 2. Create a prefab
    /// 3. Use the prefab on ship and surface turret weapons.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Projectile Module")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleProjectileModule : ProjectileModule
    {
        #region Public Variables
        [Range(0f, 1.0f)] public float sampleDrag = 0.2f;

        #endregion

        #region Private Variables

        #endregion

        #region Override Methods

        /// <summary>
        /// Generally you won't need to override this method
        /// </summary>
        /// <param name="ipParms"></param>
        public override void InitialiseProjectile(InstantiateProjectileParameters ipParms)
        {
            // If you need to override InitialisProject, generally call the base version first.
            base.InitialiseProjectile(ipParms);

            // Do your stuff here
            Debug.Log("SampleProjectileModule overriding InitialiseProjectile T:" + Time.time);
        }

        /// <summary>
        /// Change how your non-guided projectile moves
        /// </summary>
        protected override void CalcPositionAndVelocity()
        {
            // Generally we always want to update thisFramePosition and optionally velocity
            // Here we are going to modify speed and velocity and allow the base method to handle the rest.
            // This will slow the projectile down as it travels.
            speed -= sampleDrag * 100f * Time.deltaTime;

            // Prevent projectile going backward
            if (speed < startSpeed * 0.1f) 
            {
                speed = startSpeed * 0.1f;
                // The projectile will automatically be despawned
                despawnTimer = despawnTime;
            }

            // Set a new velocity
            velocity = transform.forward * speed;

            // In this sample we still want to do most of the standard calculations
            base.CalcPositionAndVelocity();
        }

        /// <summary>
        /// You can write your own collision detection code for projectiles
        /// </summary>
        /// <returns></returns>
        protected override bool CheckCollision()
        {
            // You custom code can go here

            // In this sample we don't really need this method but include it here to demonstrate

            return base.CheckCollision();
        }


        #endregion
    }
}