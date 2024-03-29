using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SyncSettings : MonoBehaviour
{
    public GameObject volumeSlider;
    public GameObject qualityDropDown;
    // Start is called before the first frame update
    void Start()
    {
        TMP_Dropdown dd = qualityDropDown.GetComponent<TMP_Dropdown>();
        switch (Settings.CesiumGraphicsQuality)
        {
            case 1:
                dd.value = 0;
                break;
            case 2:
                dd.value = 1;
                break;
            case 4:
                dd.value = 2;
                break;
            case 8:
                dd.value = 3;
                break;
            case 16:
                dd.value = 3;
                break;
        }
        Scrollbar vs = volumeSlider.GetComponent<Scrollbar>();
        vs.value = Settings.Volume;
    }
}
