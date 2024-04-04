using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class FixTransform : MonoBehaviour
{
    // Public Fields
    public GameObject hands;
    public GameObject camera;

    //public GameObject ship;
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
        hands.transform.position = hands.transform.position + new Vector3(0, .1f, 0f);
        camera.transform.position = camera.transform.position + new Vector3(0, .1f, 0f);
    }

    public void Down()
    {
        hands.transform.position = hands.transform.position + new Vector3(0, -.1f, 0f);
        camera.transform.position = camera.transform.position + new Vector3(0, -.1f, 0f);
    }

    public void Right()
    {
        hands.transform.position = hands.transform.position + new Vector3(.1f, 0f, 0f);
        camera.transform.position = camera.transform.position + new Vector3(.1f, 0f, 0f);
    }

    public void Left()
    {
        hands.transform.position = hands.transform.position + new Vector3(-.1f, 0f, 0f);
        camera.transform.position = camera.transform.position + new Vector3(-.1f, 0f, 0f);
    }

    public void Forward()
    {
        hands.transform.position = hands.transform.position + new Vector3(0f, 0f, .1f);
        camera.transform.position = camera.transform.position + new Vector3(0f, 0f, .1f);
    }

    public void Backward()
    {
        hands.transform.position = hands.transform.position + new Vector3(0f, 0f, -.1f);
        camera.transform.position = camera.transform.position + new Vector3(0f, 0f, -.1f);
    }
  
}
