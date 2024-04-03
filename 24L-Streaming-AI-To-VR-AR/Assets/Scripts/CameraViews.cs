using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraViews : MonoBehaviour
{
    public Camera PlaneCamera;
    public Camera SpectorCamera;
    public Camera ChatCamera;

    private static CameraViews ThisInstance;
    private Camera LastCamera;
    private Camera CurrentCamera;

    private float oRotation = -90;
    private float fRotation = -90;
    // Start is called before the first frame update
    void Start()
    {
        PlaneCamera.enabled = false;
        SpectorCamera.enabled = true;
        ChatCamera.enabled = false;
        CurrentCamera = SpectorCamera;
        LastCamera = PlaneCamera;
        ThisInstance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.F1))
        {
            CurrentCamera.enabled = false;
            CurrentCamera = SpectorCamera;
            CurrentCamera.enabled = true;
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.F2))
        {   
            CurrentCamera.enabled = false;
            CurrentCamera = PlaneCamera;
            CurrentCamera.enabled = true;
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.F3))
        {
            CurrentCamera.enabled = false;
            CurrentCamera = ChatCamera;
            CurrentCamera.enabled = true;
        }

        if (SpectorCamera.enabled)
        {

        }

        if (PlaneCamera.enabled)
        {
            if (UnityEngine.Input.GetKey(KeyCode.LeftArrow))
            {
                fRotation += 45f * (Time.deltaTime);
                if (fRotation > 0)
                    fRotation = 0;
            }

            if (UnityEngine.Input.GetKey(KeyCode.RightArrow))
            {
                fRotation -= 45f * (Time.deltaTime);
                if (fRotation < -180)
                    fRotation = -180;
            }

            //PlaneCamera.transform.position = Quaternion.Euler(0, fRotation, 0) * new Vector3(0, 1.5f, -3.5f);
            //PlaneCamera.transform.rotation = Quaternion.LookRotation(SpectorCamera.transform.position - PlaneCamera.transform.position + new Vector3(-2, 0, 0));
        }
    }
}
