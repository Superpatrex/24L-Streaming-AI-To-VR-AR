using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Settings : MonoBehaviour
{
    // Public SerailzieField variables for the UI elements
    public Toggle tunnelingToggle;
    public TMP_Dropdown turnDropDown;
    public TMP_Dropdown qualityDropDown;
    public Scrollbar volumeSlider;

    // Public static variables for the settings
    public static int CesiumGraphicsQuality = 8;
    public static string Turn = "Snap";
    public static float Volume = 1.0f;
    public static bool Tunneling = false;

    // Private variables for the settings
    private static readonly int CesiumGraphicsQualityMax = 16;
    private static readonly int CesiumGraphicsQualityMin = 0;

    public void Start()
    {
        if (tunnelingToggle == null || turnDropDown == null || qualityDropDown == null || volumeSlider == null)
        {
            Debug.LogError("One or more UI elements are not set in the inspector. Fix this before continuing.");
            throw new Exception("One or more UI elements are not set in the inspector. Fix this before continuing.");
        }
    }
    
    /// <summary>
    /// Set the Cesium graphics quality based on the value of the quality dropdown.
    /// Should only be powers of 2 between 0 and 16.
    /// </summary>
    public void SetCesiumGraphicsQuality()
    {
        int quality = (int)Math.Pow((double)2, (double)qualityDropDown.value);

        if (quality <= CesiumGraphicsQualityMin || quality >= CesiumGraphicsQualityMax)
        {
            Debug.Log("Invalid quality level. Setting to default value of 8.");
            CesiumGraphicsQuality = 8;
        }
        else
        {
            CesiumGraphicsQuality = quality;
        }

        Debug.Log("Cesium graphics quality set to " + CesiumGraphicsQuality + ".");
    }

    /// <summary>
    /// Sets whether the looking movement is using snap or continuous.
    /// </summary>
    public void SetTurn()
    {
        Turn = turnDropDown.value == 0 ? "Snap" : "Continuous";
        Debug.Log("Turn set to " + Turn + ".");
    }

    /// <summary>
    /// Set the global volume based on the value of the volume slider.
    /// </summary>
    public void SetVolume()
    {
        AudioListener.volume = volumeSlider.value;
        Debug.Log("Volume set to " + AudioListener.volume + ".");
    }

    /// <summary>
    /// Set whether tunneling is enabled or disabled.
    /// </summary>
    public void SetTunneling()
    {
        Tunneling = tunnelingToggle.isOn;
        Debug.Log("Tunneling is " + (Tunneling ? "enabled" : "disabled") + ".");
    }
}
