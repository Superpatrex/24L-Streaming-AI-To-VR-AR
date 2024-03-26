using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class FixTransform : MonoBehaviour
{
    //public GameObject targetObject;
    //public InputAction fixTransform;
    //public InputAction startListening;
    //public UnityEvent voiceEnabled;
    //public UnityEvent voiceDisable;

    public GameObject hands;
    public GameObject camera;

    public GameObject ship;

    void Start()
    {
        FixCameraAndHands();
    }
    public void FixCameraAndHands()
    {
        // This fixes the issues with the camera and hands not being in the correct position
        hands.transform.position = ship.transform.position + new Vector3(0, -.18f, 4.5f);
        camera.transform.position = ship.transform.position + new Vector3(0, -.18f, 4.5f);

        // Parent the camera and hands to the ship
        hands.transform.SetParent(ship.transform, true);
        camera.transform.SetParent(ship.transform, true);
    }
  
}
