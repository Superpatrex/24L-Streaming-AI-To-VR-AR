using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class ThrottleChange : MonoBehaviour
{
    
    public InputAction adjustThrottle;
    public GameObject ship;
    public float [] values = {0.0f, 30.0f, 60.0f, 30.0f, -10.0f};
    private int i = 0;
    void Update()
    {
        
    }

    void OnEnable()
    {
        adjustThrottle.Enable();
    }

    void Start()
    {
        Debug.Log("Throttle changed to " + values[i % values.Length]);

        adjustThrottle.started += context => {
            //this.gameObject.transform.rotation = ship.transform.rotation * Quaternion.Euler(values[++i % values.Length], 0, 0f);            
            Debug.Log("Throttle changed to " + values[i % values.Length]);
        };
        adjustThrottle.performed += context => {
            this.gameObject.transform.rotation = ship.transform.rotation * Quaternion.Euler(values[++i % values.Length], 0, 0f); 
            Debug.Log("Throttle changed to " + values[i % values.Length]);           
        };
        adjustThrottle.canceled += context => {
            //this.gameObject.transform.rotation = ship.transform.rotation * Quaternion.Euler(values[++i % values.Length], 0, 0f);            
            Debug.Log("Throttle changed to " + values[i % values.Length]);
        };
    }
}