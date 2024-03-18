using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class SpawnUI : MonoBehaviour
{
    public GameObject targetObject;
    public InputAction activateUI;
    public InputAction startListening;
    public UnityEvent voiceEnabled;
    public UnityEvent voiceDisable;

    public GameObject hands;
    public GameObject camera;

    public GameObject ship;

    void OnEnable()
    {
        activateUI.Enable();
        startListening.Enable();
    }

    void OnDisable()
    {
        activateUI.Disable();
    }

    
    void Start()
    {
        activateUI.started += context => {
            targetObject.SetActive(!targetObject.activeSelf);
        };
        activateUI.performed += context => {
            targetObject.SetActive(!targetObject.activeSelf);
        };
        activateUI.canceled += context => {
            targetObject.SetActive(!targetObject.activeSelf);
        };

        startListening.started += context => {
            voiceEnabled.Invoke();
        };
        startListening.performed += context => {
            voiceEnabled.Invoke();
        };
        startListening.canceled += context => {
            voiceEnabled.Invoke();
        };

        FixCameraAndHands();
    }

    public void FixCameraAndHands()
    {
        // This fixes the issues with the camera and hands not being in the correct position
        hands.transform.position = ship.transform.position + new Vector3(0, 0, 4.3f);
        camera.transform.position = ship.transform.position + new Vector3(0, 0, 4.3f);
    }
  
}