using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Attach to Collider that you want to receive damaage when projectiles or beams
    /// hit it. Configure the callback to call your method whenever it is hit.
    /// This is useful for objects in the scene that are not S3D characters - like
    /// your own stationary or moving objects.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Utilities/Sticky Damage Receiver")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyDamageReceiver : MonoBehaviour
    {
        #region Public Delegates
        public delegate void CallbackOnHit(S3DObjectHitParameters s3dObjectHitParameters);

        /// <summary>
        /// The name of the custom method that is called immediately
        /// after the object is hit by a projectile or beam. Your method must take 1
        /// parameter of type S3DObjectHitParameters. This should be 
        /// a lightweight method to avoid performance issues. It could be used to 
        /// take damage on non-S3D character assets in the scene.
        /// </summary>
        public CallbackOnHit callbackOnHit = null;
        #endregion
    }
}