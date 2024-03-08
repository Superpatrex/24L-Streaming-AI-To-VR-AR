using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class Tunneling : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        if (Settings.Tunneling)
        {
            // Enable volume component to show vignette
            gameObject.GetComponent<Volume>().enabled = true;
        }
        else
        {
            gameObject.GetComponent<Volume>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
