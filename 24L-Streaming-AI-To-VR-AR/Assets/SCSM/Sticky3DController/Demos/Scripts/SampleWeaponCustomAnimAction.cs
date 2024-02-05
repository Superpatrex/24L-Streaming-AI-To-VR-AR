using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Sample script to have a StickyWeapon run an Animation Action in code when
    /// a weapon fires.
    /// A simpler setup for this scenario is just to use the Fire1 Weapon Action
    /// without having to write any code. However, here we want to show how to do
    /// things in code.
    /// Setup:
    /// 1. On the StickyWeapon, go to the Weapon tab and expand Animate Settings
    /// 2. Link in your weapon Animator
    /// 3. Under Weapon Actions, add an Animate Action.
    /// 4. Set the Weapon Action to "Custom"
    /// 5. Assuming your animator controller has a Trigger parameter called "Fire",
    ///    set the Parameter Type to "Trigger", and select the "Parameter Name"
    ///    from the dropdown list.
    /// 6. The "Trigger Value" should now say "User game code"
    /// 7. Add this component to the StickyWeapon.
    /// 8. Set the "Anim Action Number" to the Custom Animate Action added above.
    /// 9. Run the scene and fire the weapon.
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    [AddComponentMenu("Sticky3D Controller/Samples/Weapon Custom Anim Action")]
    [DisallowMultipleComponent]
    public class SampleWeaponCustomAnimAction : MonoBehaviour
    {
        #region Public Variables
        public bool initialiseOnStart = true;

        [Tooltip("The (Custom) Anim Action from the Animate tab of your StickyWeapon. 0 = Not Set")]
        public int animActionNumber = 0;
        #endregion

        #region Private Variables - General
        private StickyWeapon stickyWeapon = null;
        private bool isInitialised = false;
        private S3DAnimAction s3dAnimAction = null;
        #endregion

        #region Private Initialise Methods

        // Use this for initialization
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Private and Internal Methods - General

        /// <summary>
        /// This is automatically called by the weapon when it fires
        /// </summary>
        /// <param name="stickyWeapon"></param>
        /// <param name="weaponButtonNumber"></param>
        private void WeaponHasFired (StickyWeapon stickyWeapon, int weaponButtonNumber)
        {
            if (isInitialised && (weaponButtonNumber == 1 || weaponButtonNumber == 2))
            {
                #if UNITY_EDITOR
                // Uncomment to output to the Unity console
                //Debug.Log("Weapon (" + name + ") has fired on the " + (weaponButtonNumber == 1 ? "Primary" : "Secondary") + " firing mechansim");
                #endif

                // Here we use a Trigger to enable our action (however, we could also use a bool).
                // See also SetCustomAnimActionBoolValue(..) and SetCustomAnimActionFloatValue(..).
                if (s3dAnimAction.parameterType == S3DAnimAction.ParameterType.Trigger)
                {
                    stickyWeapon.SetCustomAnimActionTriggerValue(s3dAnimAction, true);
                }
            }
        }

        #endregion

        #region Public API Methods - General

        public void Initialise()
        {
            if (!TryGetComponent(out stickyWeapon))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleWeaponCustomAnimAction - did you forget to attach this script to StickyWeapon?");
                #endif
            }
            else
            {
                if (!stickyWeapon.IsWeaponInitialised)
                {
                    stickyWeapon.Initialise();
                }

                int numAnimActions = stickyWeapon.NumberOfAnimActions;

                if (!stickyWeapon.IsAnimateEnabled)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SampleWeaponCustomAnimAction - Animate is not enabled on the weapon (" + name + ")");
                    #endif
                }
                if (animActionNumber > numAnimActions)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SampleWeaponCustomAnimAction - The Anim Action Number from the Animate tab of the weapon is out of range. Number must be between 1 and " + numAnimActions.ToString());
                    #endif
                }
                else
                {
                    // Get the (custom) Anim Action
                    s3dAnimAction = stickyWeapon.GetAnimActionByIndex(animActionNumber - 1);

                    if (s3dAnimAction == null)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("ERROR: SampleWeaponCustomAnimAction - could not find the custom animation action using AnimActionNumber " + animActionNumber + " on the weapon (" + name + ")");
                        #endif
                    }
                    else
                    {
                        // Get notified when the weapon fires
                        stickyWeapon.callbackOnWeaponFired = WeaponHasFired;

                        isInitialised = true;
                    }
                }      
            }
        }

        #endregion
    }
}