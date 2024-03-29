using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using SciFiShipController;
using scsmmedia;
using System.Diagnostics.Tracing;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEditorInternal;
using UnityEditor;



public class ShowSettings : MonoBehaviour
{
    public GameObject targetObject;
    public InputAction changeSettings;
    public GameObject aircraft;
    public GameObject rayInteractor;
    public GameObject lever;
    public GameObject joystick;
    public GameObject leftHand;
    private PlayerInputModule playerInputModule = null;
    private ShipControlModule shipControlModule = null;
    private ShipInput shipInput = null;
    private ShipInput currentShipInput = new ShipInput();
    private List<Weapon> weapons;
    private bool isSettingsMenuActive = false;
  
    void OnEnable() // This OnEnable is called when the object that this script is attached to is enabled.
    {
        changeSettings.Enable();
    }

    void OnShowSettings()
    {

        if (!isSettingsMenuActive)
        {
            if (Settings.Tunneling){ Tunneling.volume.enabled = false; }

            playerInputModule = aircraft.GetComponent<PlayerInputModule>();
            if (playerInputModule != null && playerInputModule.IsInitialised)
            {
                shipControlModule = playerInputModule.GetShipControlModule;
                shipControlModule.GetShipInput(currentShipInput);  // Assign current ship data to currentShipInput object 

                if (shipControlModule == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SampleInputOverride - could not get the ShipControlModule. Did you attach the script to the player ship?");
                    #endif
                }
                else if (!shipControlModule.IsInitialised)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SampleInputOverride - is Initialise on Awake is enabled on the Physics tab of the ShipControlModule?");
                    #endif
                }
                else
                {
                    // Override the Longitudinal (foward/backward) axis
                    playerInputModule.isLongitudinalDataDiscarded = true;
                    // Re-initialise the DiscardData shipInput settings in the PlayerInputModule
                    // This is required after one or more of the [axis]DataDiscarded values are changed at runtime.
                    playerInputModule.ReinitialiseDiscardData();

                    // Create a new instance of the shipInput which we can send each frame to the ship.
                    shipInput = new ShipInput();
                    if (shipInput != null)
                    {
                        // Start by disabling everything. This helps to future-proof the code.
                        // We'll be telling the Ship you can discard anything else that we don't enable below.
                        shipInput.DisableAllData();

                        // When we send data, we will tell the ship we'll be sending Longitudinal data only.
                        shipInput.isLongitudinalDataEnabled = true;
                    }
                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("ERROR: SampleInputOverride - did you forget to attach to the PlayerInputModule? Also check if Initialise on Awake is enabled.");
            }
            #endif

            // Stop aircraft
            shipInput.longitudinal = 0f;
            shipControlModule.SendInput(shipInput);
            // Debug.Log(currentShipInput.longitudinal);

            isSettingsMenuActive = true;

            // Adjust hand position
            leftHand.transform.localEulerAngles = new Vector3(50, 0, 90);

            // Show ray interactor
            rayInteractor.SetActive(true);
            rayInteractor.transform.localEulerAngles = new Vector3(40, 0, 0);
            rayInteractor.GetComponent<XRInteractorLineVisual>().enabled = true;

            // Deactivate ability to move the aircraft and use weapons
            lever.GetComponent<StickyInteractive>().enabled = false;
            joystick.GetComponent<StickyInteractive>().enabled = false;
            weapons = shipControlModule.shipInstance.weaponList;
            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i].firingButton = 0;
            }

        }
        else
        {
            shipInput.longitudinal = currentShipInput.longitudinal;
            //Debug.Log(shipInput.longitudinal);
            shipControlModule.SendInput(shipInput);
            isSettingsMenuActive = false;

            // Apply settings to environment
            ApplySettings.SaveSettings();


            // Adjust hand position
            leftHand.transform.localEulerAngles = new Vector3(0, 0, 90);

            // Hide ray interactor
            rayInteractor.SetActive(false);

            // Activate ability to move the aircraft and use weapons
            lever.GetComponent<StickyInteractive>().enabled = true;
            joystick.GetComponent<StickyInteractive>().enabled = true;
            for (int i = 0; i < weapons.Count; i++)
            {
                switch (weapons[i].name)
                {
                    case "Left Gun":
                    case "Right Gun":
                        weapons[i].firingButton = (Weapon.FiringButton)1;
                        break;
                    case "Left Missile":
                    case "Right Missile":
                        weapons[i].firingButton = (Weapon.FiringButton)2;
                        break;
                }
            }
            
        }

    }

    void OnDisable()
    {
        changeSettings.Disable();
    }


    void Start()
    {
        changeSettings.started += context =>
        {
            targetObject.SetActive(!targetObject.activeSelf);
        };
        changeSettings.performed += context =>
        {
            targetObject.SetActive(!targetObject.activeSelf);
        };
        changeSettings.canceled += context =>
        {
            targetObject.SetActive(!targetObject.activeSelf);
        };
    }
}
