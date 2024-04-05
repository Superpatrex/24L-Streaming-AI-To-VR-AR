using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class FixTransform : MonoBehaviour
{
    // Public Fields
    public GameObject hands;
    public GameObject camera;

    public GameObject ship;
    private float speed = 0.5f;
    //public GameObject head;
    //public

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        //FixCameraAndHands();
    }

    /// <summary>
    /// Fixes the Camera and Hands when the script is started
    /// </summary>
    public void Up()
    {
        Vector3 direction = ship.transform.TransformDirection(new Vector3(0, speed, 0f));
        hands.transform.position += direction;
        camera.transform.position += direction;
    }

    public void Down()
    {
        Vector3 direction = ship.transform.TransformDirection(new Vector3(0, -1 * speed, 0f));
        hands.transform.position += direction;
        camera.transform.position += direction;
    }

    public void Right()
    {
        Vector3 direction = ship.transform.TransformDirection(new Vector3(speed, 0f, 0f));
        hands.transform.position += direction;
        camera.transform.position += direction;
    }

    public void Left()
    {
        Vector3 direction = ship.transform.TransformDirection(new Vector3(-1 * speed, 0f, 0f));
        hands.transform.position += direction;
        camera.transform.position += direction;
    }

    public void Forward()
    {
        Vector3 direction = ship.transform.TransformDirection(new Vector3(0f, 0f, speed));
        hands.transform.position += direction;
        camera.transform.position += direction;
    }

    public void Backward()
    {
        Vector3 direction = ship.transform.TransformDirection(new Vector3(0f, 0f, -1 * speed));
        hands.transform.position += direction;
        camera.transform.position += direction;
    }
  
}
