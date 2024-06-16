using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CesiumForUnity;

public class CesiumGraphics : MonoBehaviour
{
    static Cesium3DTileset tileset;
    // Start is called before the first frame update
    void Start()
    {
        tileset = GetComponent<Cesium3DTileset>();

        // Set the Cesium graphics quality based on the value of the quality dropdown.
        SetCesiumGraphicsQuality();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void SetCesiumGraphicsQuality()
    {
        if (tileset != null && tileset.maximumScreenSpaceError != Settings.CesiumGraphicsQuality)
        {
            tileset.maximumScreenSpaceError = Settings.CesiumGraphicsQuality;
        }
    }
}
