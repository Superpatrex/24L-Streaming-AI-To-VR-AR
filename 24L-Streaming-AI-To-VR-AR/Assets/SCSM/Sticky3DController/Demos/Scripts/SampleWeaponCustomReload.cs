using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple sample script to attach to a weapon to perform a custom reload action
    /// when the weapon runs out of ammo.
    /// NOTE: If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Weapon Custom Reload")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class SampleWeaponCustomReload : MonoBehaviour
    {
        #region Public Variables

        #endregion

        #region Private Variables - General
        private bool isInitialised = false;
        #endregion

        #region Private Initialise Methods

        // Use this for initialization
        void Start()
        {
            Initialise();
        }

        #endregion

        #region Private Methods

        private void Initialise()
        {
            // We don't need to cache the weapon, as it will get
            // passed in again in the callback.
            StickyWeapon stickyWeapon = null;

            if (!TryGetComponent(out stickyWeapon))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR SampleWeaponCustomReload could not find a StickyWeapon component attached to " + name);
                #endif
            }
            else
            {
                // Although not strickly necessary in this scenario, we'll Initialise the weapon
                // if it is not already initialised.
                if (!stickyWeapon.IsInitialised) { stickyWeapon.Initialise(); }

                stickyWeapon.callbackOnWeaponNoAmmo = WeaponHasNoAmmo;
                isInitialised = true;
            }
        }

        /// <summary>
        /// This is automatically called by the weapon when it runs out of ammo
        /// </summary>
        /// <param name="stickyWeapon"></param>
        /// <param name="weaponButtonNumber"></param>
        private void WeaponHasNoAmmo (StickyWeapon stickyWeapon, int weaponButtonNumber)
        {
            if (isInitialised && (weaponButtonNumber == 1 || weaponButtonNumber == 2))
            {
                Debug.Log("Weapon (" + name + ") has run out of ammo on the " + (weaponButtonNumber == 1 ? "Primary" : "Secondary") + " firing mechansim");

                // Simply call Manual Reload
                // Instead you could do any number of more fancy reload "things".
                stickyWeapon.Reload(weaponButtonNumber);
            }
        }

        #endregion
    }
}