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
    public TMP_Dropdown ttsNameDropDown;
    public TMP_Dropdown qualityDropDown;
    public Scrollbar volumeSlider;

    // Public static variables for the settings
    public static float CesiumGraphicsQuality = 4f;
    public static int textToSpeechVoice = 5;
    public static float Volume = 1.0f;
    public static bool Tunneling = false;
    public static Settings Instance { get; private set; }

    // Private variables for the settings
    private static readonly int CesiumGraphicsQualityMax = 17;
    private static readonly int CesiumGraphicsQualityMin = 0;

    private static readonly string[] TTSNames = {
        "Skully",
        "Cael",
        "Cam",
        "Carl",
        "Cartoon Baby",
        "Charlie",
        "Cody",
        "Connor",
        "Cooper",
        "Disaffected",
        "Hollywood",
        "Overconfident",
        "Prospector",
        "Railey",
        "Rebecca",
        "Remi",
        "Rubie",
        "Trendy",
        "Vampire",
        "Maria",
        "British Butler",
        "Pirate",
        "Colin",
        "Rosie"
    };

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            //Destroy(gameObject);
        }
    }

    public void Start()
    {
        if (tunnelingToggle == null || ttsNameDropDown == null || qualityDropDown == null || volumeSlider == null)
        {
            Debug.LogError("One or more UI elements are not set in the inspector. Fix this before continuing.");
            throw new Exception("One or more UI elements are not set in the inspector. Fix this before continuing.");
        }

        if (ttsNameDropDown != null)
        {
            ttsNameDropDown.value = textToSpeechVoice;
        }
    }
    
    /// <summary>
    /// Set the Cesium graphics quality based on the value of the quality dropdown.
    /// Should only be powers of 2 between 0 and 16.
    /// </summary>
    public void SetCesiumGraphicsQuality()
    {
        int quality = (int)Math.Pow((double)2, (double)qualityDropDown.value);

        if (quality < CesiumGraphicsQualityMin || quality > CesiumGraphicsQualityMax)
        {
            Debug.Log("Invalid quality level. Setting to default value of 8.");
            CesiumGraphicsQuality = 8f;
        }
        else
        {
            CesiumGraphicsQuality = (float)quality;
        }

        Debug.Log("Cesium graphics quality set to " + CesiumGraphicsQuality + ".");
    }

    /// <summary>
    /// Sets whether the looking movement is using snap or continuous.
    /// </summary>
    public void SetTTSName()
    {
        textToSpeechVoice = ttsNameDropDown.value;
        PlayerPrefs.SetInt("TTSName", textToSpeechVoice);
        PlayerPrefs.Save();
        Debug.Log("Turn set to " + textToSpeechVoice + ".");
    }

    /// <summary>
    /// Set the global volume based on the value of the volume slider.
    /// </summary>
    public void SetVolume()
    {
        AudioListener.volume = volumeSlider.value;
        Volume = volumeSlider.value;
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

    public string GetTTSName()
    {
        return TTSNames[textToSpeechVoice];
    }

    public float GetCesiumGraphicsQuality()
    {
        return CesiumGraphicsQuality;
    }
}
