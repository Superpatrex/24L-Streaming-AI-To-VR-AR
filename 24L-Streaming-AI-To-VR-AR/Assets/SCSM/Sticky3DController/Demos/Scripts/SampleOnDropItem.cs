// Copyright (c) 2015-2022 SCSM Pty Ltd. All rights reserved.
using UnityEngine;

namespace scsmmedia
{
    /// <summary>
    /// Sample component that can be added to a StickyInteractive-enabled object in the scene
    /// that takes action when an item is dropped.
    /// The sample uses Unity physics gravity by default but can use other forms of gravity
    /// by setting "Use Gravity" on the interactive-enabled object.
    /// NOTE: This should NOT be used with Weapons or Magazines which already support Drop natively.
    /// Setup:
    /// 1. Add a StickyInteractive component to an object in the scene
    /// 2. On the StickyInteractive component, tick Initialise on Start, IsGrabbable, and
    ///    configure the primary hold position, and optionally tick IsTouchable.
    /// 3. On the StickyInteractive component, add two events for the OnDropped.
    /// 4. Drag this gameobject into each event
    /// 5. On the first event, set the function to "StickyInteractive.EnableNonTriggerColliders"
    /// 6. On the second event, set the function to "SampleOnDropItem.DropItem".
    /// 7. If IsTouchable is enabled, on the S3D player character, add a custom input, button, Alpha 1. Add event,
    ///    PlayerJaneSuited (or name of player), function: StickyControlModule.SetRightHandIKTargetLookAtInteractive
    /// 8. On the S3D player character, add a custom input, button, key G. Add event,
    ///    PlayerJaneSuited (or name of player), function: StickyControlModule.ToggleHoldInteractive
    /// 9. Optionally try enabling "Use Gravity" on the interactive object.
    /// WARNING
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/On Drop Item")]
    [DisallowMultipleComponent]
    public class SampleOnDropItem : MonoBehaviour
    {
        #region Public Variables
        
        #endregion

        #region Private Variables
        private StickyInteractive stickyInteractive = null;
        private bool isInitialised = false;
        #endregion

        #region Initialise Methods

        private void Start()
        {
            if (TryGetComponent(out stickyInteractive))
            {
                isInitialised = true;
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR SampleOnDropItem could not find an attached StickyInteractive component on " + name); }
            #endif
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Call this from the OnDropped event on a StickyInteractive-enabled object
        /// </summary>
        public void DropItem()
        {
            if (isInitialised)
            {
                if (stickyInteractive.IsUseGravity)
                {
                    stickyInteractive.RestoreGravity();
                }
                else if (stickyInteractive.HasRigidbody)
                {
                    stickyInteractive.RestoreRigidbodySettings();

                    // Turn on interpolate as we are close to the camera
                    stickyInteractive.ObjectRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                }
                else
                {
                    // Add a non-kinematic rigidbody
                    stickyInteractive.AddRigidbody (false);

                    Rigidbody rb = stickyInteractive.ObjectRigidbody;

                    if (rb != null)
                    {
                        rb.mass = stickyInteractive.Mass;
                        // Use the in-built Unity Physics gravity.
                        rb.useGravity = true;
                        rb.interpolation = RigidbodyInterpolation.Interpolate;
                    }
                }
            }
        }

        #endregion
    }
}