using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnUI : MonoBehaviour
{
    public GameObject targetObject;
    public InputAction activateUI;
    // public PlayerInput playerInput;



    // public void onActivateUI()
    // {
    //     ToggleObject();
    // }

    void OnEnable()
    {
        activateUI.Enable();
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
    }


    void ToggleObject()
    {
        targetObject.SetActive(!targetObject.activeSelf);
    }

  
}
