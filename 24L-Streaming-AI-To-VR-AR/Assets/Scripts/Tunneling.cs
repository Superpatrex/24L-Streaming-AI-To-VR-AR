using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class Tunneling : MonoBehaviour
{
    public static Volume volume;

    // Start is called before the first frame update
    void Start()
    {
        volume = gameObject.GetComponent<Volume>();
        SetTunneling();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void SetTunneling()
    {
        if (Settings.Tunneling)
        {
            // Enable volume component to show vignette
            volume.enabled = true;
        }
        else
        {
            volume.enabled = false;
        }
    }
}
