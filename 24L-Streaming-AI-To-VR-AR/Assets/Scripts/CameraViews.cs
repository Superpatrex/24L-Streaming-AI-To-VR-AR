using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraViews : MonoBehaviour
{
    public Camera PlaneCamera;
    public Camera SpectorCamera;

    private static CameraViews ThisInstance;
    private Camera LastCamera;
    private Camera CurrentCamera;
    // Start is called before the first frame update
    void Start()
    {
        PlaneCamera.enabled = false;
        SpectorCamera.enabled = true;
        CurrentCamera = SpectorCamera;
        LastCamera = PlaneCamera;
        ThisInstance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
        {
            CurrentCamera.enabled = false;
            CurrentCamera = SpectorCamera;
            CurrentCamera.enabled = true;
        }
        else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
        {   
            CurrentCamera.enabled = false;
            CurrentCamera = PlaneCamera;
            CurrentCamera.enabled = true;
        }
    }
}
