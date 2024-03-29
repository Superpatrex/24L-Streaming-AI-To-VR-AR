using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ApplySettings : MonoBehaviour
{
    public GameObject settingsMenu;
    
    public static void SaveSettings()
    {
    
        CesiumGraphics.SetCesiumGraphicsQuality();
        Tunneling.SetTunneling();
    
    }
}
