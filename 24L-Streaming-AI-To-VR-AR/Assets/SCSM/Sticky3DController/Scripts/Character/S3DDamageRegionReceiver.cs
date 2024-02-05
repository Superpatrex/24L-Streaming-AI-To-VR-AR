using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A small component placed on a damage region transform, typically a humanoid bone,
    /// that allows it to receive hit damage and transmit that to the stickyControlModule.
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    [DisallowMultipleComponent]
    public class S3DDamageRegionReceiver : MonoBehaviour
    {
        /// <summary>
        /// Gets configured at runtime
        /// </summary>
        [HideInInspector, System.NonSerialized] public StickyControlModule stickyControlModule = null;

        /// <summary>
        /// Gets set at runtime when the reference to the character is set. This avoids a null check.
        /// </summary>
        [HideInInspector] public bool isValid = false;

        /// <summary>
        /// Gets set at runtime. The guidHash of the damage region
        /// </summary>
        [HideInInspector] public int damageRegionId = 0;
    }
}