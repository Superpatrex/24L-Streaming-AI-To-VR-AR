using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple sample script to attach to a S3D player character to modify the camera offset
    /// when a weapon is held in third-person. It uses the onPreStartHoldWeapon and
    /// onPostStopHoldWeapon events. This shows how to call APIs when starting to
    /// hold or drop a weapon.
    /// NOTE: If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// Setup
    /// 1. Add this component to your player Sticky3D character in the scene
    /// 2. Enable "Is Initialise On Start"
    /// 3. On the Events tab of the Sticky Control Module add an event to onPreStartHoldWeapon
    /// 4. Drag the StickyControlModule into the Object field
    /// 5. Set the Function to SampleWeaponGrabTPS OnPreStartHoldWeapon
    /// 6. On the Events tab of the Sticky Control Module add an event to onPostStopHoldWeapon
    /// 7. Drag the StickyControlModule into the Object field
    /// 8. Set the Function to SampleWeaponGrabTPS OnPostStopHoldWeapon
    /// 9. Run the scene switch to third person
    /// 10. Pickup a weapon in the scene
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Weapon Grab TPS")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class SampleWeaponGrabTPS : MonoBehaviour
    {
        #region Public Variables

        public bool isInitialiseOnStart = false;

        [Tooltip("Third person camera offset when holding (but not aiming) a weapon")]
        public Vector3 cameraOffsetTPS = new Vector3(0.6f, 1.7f, -1f);

        [Tooltip("Local space point on the character where the third person camera focuses relative to the origin or pivot point of the character prefab when holding (but not aiming) a weapon.")]
        public Vector3 focusOffsetTPS = new Vector3(0.3f, 1.6f, 0.3f);

        #endregion

        #region Private Variables - General

        private StickyControlModule stickyControlModule = null;
        private bool isInitialised = false;

        private Vector3 savedCameraOffset = Vector3.zero;
        private Vector3 savedFocusOffset = Vector3.zero;

        #endregion

        #region Private Initialise Methods

        // Use this for initialization
        private void Start()
        {
            if (isInitialiseOnStart) { Initialise(); }
        }

        /// <summary>
        /// Call this from your own game code if isInitialiseOnStart is false.
        /// </summary>
        public void Initialise()
        {
            if (!TryGetComponent(out stickyControlModule))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR SampleWeaponGrabTPS could not find a StickyControlModule component attached to " + name);
                #endif
            }
            else if (stickyControlModule.IsNPC())
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleWeaponGrabTPS - your character " + stickyControlModule.name + " must be a player, not an NPC.");
                #endif
            }
            else if (!stickyControlModule.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleWeaponGrabTPS - your character " + stickyControlModule.name + " is not initialised");
                #endif
            }
            else
            {
                isInitialised = true;
            }
        }

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// This gets called automatically when you hook up the Function in the On Pre Start Hold Weapon event for the character.
        /// We could use a method with no parameters but for completeness we'll show how to discover the data passsed in.
        /// </summary>
        /// <param name="stickyID"></param>
        /// <param name="interactiveID"></param>
        /// <param name="notUsed"></param>
        /// <param name="futureV3"></param>
        public void OnPreStartHoldWeapon(int stickyID, int interactiveID, bool notUsed, Vector3 futureV3)
        {
            if (!isInitialised) { return; }

            // The interactiveID is the ID of the interactive-enabled weapon that is about to be grabbed.

            // If not already holding a weapon, remember the original third-person camera settings
            if (!stickyControlModule.IsRightHandHoldingWeapon && !stickyControlModule.IsLeftHandHoldingWeapon)
            {
                savedCameraOffset = stickyControlModule.lookCameraOffset;
                savedFocusOffset = stickyControlModule.lookFocusOffset;
            }

            // Modify the third person camera offsets
            stickyControlModule.SetLookCameraOffsetTP(cameraOffsetTPS);
            stickyControlModule.SetLookCamerFocusOffsetTP(focusOffsetTPS);
        }

        /// <summary>
        /// This gets called automatically when you hook up the Function in the On Post Stop Hold Weapon event for the character.
        /// We could use a method with no parameters but for completeness we'll show how to discover the data passsed in.
        /// </summary>
        /// <param name="stickyID"></param>
        /// <param name="interactiveID"></param>
        /// <param name="notUsed"></param>
        /// <param name="futureV3"></param>
        public void OnPostStopHoldWeapon(int stickyID, int interactiveID, bool notUsed, Vector3 futureV3)
        {
            if (!isInitialised) { return; }

            // The interactiveID is the ID of the interactive-enabled weapon that was just dropped, equipped or stashed.

            // If no longer holding a weapon, restore the original settings.
            if (!stickyControlModule.IsRightHandHoldingWeapon && !stickyControlModule.IsLeftHandHoldingWeapon)
            {
                stickyControlModule.SetLookCameraOffsetTP(savedCameraOffset);
                stickyControlModule.SetLookCamerFocusOffsetTP(savedFocusOffset);
            }
        }

        #endregion

    }
}