using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public static int CesiumGraphicsQuality = 8;
    public static string Turn = "Snap";
    public static float Volume = 1.0f;
    public static bool Tunneling = false;

    private static readonly int CesiumGraphicsQualityMax = 16;
    private static readonly int CesiumGraphicsQualityMin = 0;
    
    public static void SetCesiumGraphicsQuality(int quality)
    {
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

    public static void SetTurn(string turn)
    {
        Turn = turn;
        Debug.Log("Turn set to " + turn + ".");
    }

    public static void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        Debug.Log("Volume set to " + volume + ".");
    }

    public static void SetTunneling(bool tunneling)
    {
        Tunneling = tunneling;
        Debug.Log("Tunneling is " + (Tunneling ? "enabled" : "disabled") + ".");
    }
}
