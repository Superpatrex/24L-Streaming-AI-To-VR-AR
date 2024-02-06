using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Toggle tunnelingToggle;
    public Dropdown turnDropDown;
    public Dropdown qualityDropDown;
    public Slider volumeSlider;
    public static int CesiumGraphicsQuality = 8;
    public static string Turn = "Snap";
    public static float Volume = 1.0f;
    public static bool Tunneling = false;

    private static readonly int CesiumGraphicsQualityMax = 16;
    private static readonly int CesiumGraphicsQualityMin = 0;
    
    public void SetCesiumGraphicsQuality()
    {
        int quality = qualityDropDown.value;

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

    public void SetTurn()
    {
        Turn = turnDropDown.value == 0 ? "Snap" : "Smooth";
        Debug.Log("Turn set to " + Turn + ".");
    }

    public void SetVolume()
    {
        AudioListener.volume = volumeSlider.value;
        Debug.Log("Volume set to " + AudioListener.volume + ".");
    }

    public void SetTunneling()
    {
        Tunneling = tunnelingToggle.isOn;
        Debug.Log("Tunneling is " + (Tunneling ? "enabled" : "disabled") + ".");
    }
}
