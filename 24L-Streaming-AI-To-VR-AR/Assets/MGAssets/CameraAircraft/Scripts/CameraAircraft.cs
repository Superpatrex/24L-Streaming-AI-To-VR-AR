using UnityEngine;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////// - CameraAircraft Script - Version 1.1.190802 - Created by Maloke Games 2019 - Visit us here: https://maloke.itch.io/ 
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////
////// This script is a very basic camera movement used to demonstrate how the Aircraft HUD GUI works
////// It does not contains proper collision nor physics but feel free to modify and use it as you wish!
//////
////// Controls: W-S (Pitch), A-D (Roll), Q-E (Yaw), R-F (Lift), T-Space (Reset Attitude), Y (Toogle Sound), Shift-Ctrl (Faster/Slower speed)
//////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
public class CameraAircraft : MonoBehaviour
{
    public static CameraAircraft current;

    public bool isActive = true, cursorStartLocked = false, turbulence = true;
    public float pitchFactor = 1, rollFactor = 1, yawFactor = 1, thrust = 1, lift = 1;

    public AudioSource audioSource;
    public AudioClip audioClip;
    public float audioPitch = 1;

    public float boostFactor = 1f, brakeFactor = 1f;
    float boost = 1f, brake = 1f;

    float lastThrust;
    bool lastTurbulence = false;

    ////////////////// Inicialization
    void Awake()
    {
        //Application.targetFrameRate = 60;
        if (audioSource == null) audioSource.GetComponent<AudioSource>();
    }
    void Start() { if(cursorStartLocked) Cursor.lockState = CursorLockMode.Locked; else Cursor.lockState = CursorLockMode.None; }
    void OnEnable() { current = this; lastThrust = thrust; lastTurbulence = turbulence; }
    //////////////////

    //////////////////// Cursor Lock and Sound On-Off Control
    void Update()
    {
        //Return if control is not activated
        if (!isActive) return;

        //Cursor lock-unlock with Tab key
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Cursor.lockState != CursorLockMode.Locked) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            else { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
            if (audioSource != null && audioClip != null) audioSource.PlayOneShot(audioClip);
        }
        //

        //Enable or Disable Sound
        if (Input.GetKeyDown(KeyCode.Y) && audioSource != null) { audioSource.mute = !audioSource.mute; if (audioClip != null) audioSource.PlayOneShot(audioClip); }
        //
    }
    ////////////////////

    ////////////////////////////////////// Aircraft Control
    void FixedUpdate() //Update()
    {
        //Return if control is not activated
        if (!isActive) return;

        //Reset Aircraft Attitude
        if (Input.GetKey(KeyCode.T) || Input.GetMouseButtonDown(2)) transform.localRotation = Quaternion.identity;
        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButtonDown(1)) transform.localEulerAngles = new Vector3( 0, transform.localEulerAngles.y, 0);
        //

        //Boost or Brake
        if (Input.GetKey(KeyCode.LeftShift)) boost = 2; else boost = 1;
        if (Input.GetKey(KeyCode.LeftControl)) brake = .25f; else brake = 1;
        //

        //Thrust
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            if (thrust != 0) { lastThrust = thrust; thrust = 0; lastTurbulence = turbulence; turbulence = false; }
            else { thrust = lastThrust; turbulence = lastTurbulence; }
        }
        if (Input.GetKeyDown(KeyCode.Comma)) thrust -= 1;
        if (Input.GetKeyDown(KeyCode.Period )) thrust += 1;
        //

        //Sound
        if (audioSource != null) audioSource.pitch = audioPitch + ( (boost == 1)? 0 : +0.5f) + ( (brake == 1)? 0: -0.5f * audioPitch);
        //

        //Aircraft Thrust
        transform.Translate(Vector3.forward * Time.fixedDeltaTime * (thrust * 5) * boost * boostFactor * brake * brakeFactor);// * (turbulence ? Random.Range(0.995f, 1.005f) : 1));
        //

        //Lift
        transform.Translate(((Input.GetKey(KeyCode.R) ? 1 : 0) - (Input.GetKey(KeyCode.F) ? 1 : 0)) * Vector3.up * Time.fixedDeltaTime * (lift * 5) * boost * boostFactor * brake * brakeFactor); //Up and Down (Drones+Helicopters)
        //

        //Mouse Control (Only if cursor is locked)
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            transform.Rotate(
                Input.GetAxis("Mouse Y") * Time.fixedDeltaTime * (pitchFactor * 100) * boost * brake * 2,
                0, //Input.GetAxis("Mouse X") * Time.fixedDeltaTime * (yawFactor * 100) * boost * brake,
                -Input.GetAxis("Mouse X") * Time.fixedDeltaTime * (rollFactor * 100) * boost * brake / 2,
                Space.Self);
        }
        //

        //Keyboard Control
        transform.Rotate(
              ((Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0) + (turbulence ?   Random.Range(-0.05f, 0.05f) : 0)) * Time.fixedDeltaTime * (pitchFactor * 100) * boost * brake,
              ((Input.GetKey(KeyCode.E) ? 1 : 0) - (Input.GetKey(KeyCode.Q) ? 1 : 0) + (turbulence ?     Random.Range(-0.1f, 0.1f) : 0)) * Time.fixedDeltaTime * (yawFactor   * 100) * boost * brake,
              ((Input.GetKey(KeyCode.A) ? 1 : 0) - (Input.GetKey(KeyCode.D) ? 1 : 0) + (turbulence ? Random.Range(-0.125f, 0.125f) : 0)) * Time.fixedDeltaTime * (rollFactor  * 100) * boost * brake,
              Space.Self);
        //
    }
    //////////////////////////////////////
}
//