using UnityEngine;

public class WalkingCamera : MonoBehaviour
{
    public bool allowCursor = true, startLocked = false, startUnlocked = false;
    public float mouseSens = 100, walkSpeed = 3;

    [Space]
    public bool followGround = true;
    public float height = 1f;
    public float clamp = 45;

    public KeyCode resetKey = KeyCode.Space;

    //
    void Start()
    {
        if (startLocked) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        else if (startUnlocked) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }
    //
    void Update()
    {

        //Cursor lock-unlock
        if (allowCursor && Input.GetKeyDown(KeyCode.Tab))
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        //

        //
        if (followGround)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity);
            if (hit.collider != null && hit.collider.gameObject != this.gameObject) transform.position = new Vector3(transform.position.x, height + hit.point.y, transform.position.z);
        }
        //

        //
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * Time.deltaTime * mouseSens, 0, Space.World);
            transform.Rotate(-Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSens, 0, 0, Space.Self);
        }
        //


        // Clamp Vertical Angle
        float angleX = transform.localRotation.eulerAngles.x;
        if (angleX > 180) angleX -= 360;
        else if (angleX < -180) angleX += 360;

        if (angleX > 70) transform.localRotation = Quaternion.Euler(70, transform.localRotation.eulerAngles.y, 0 /*transform.localRotation.eulerAngles.z*/);
        else if (angleX < -70) transform.localRotation = Quaternion.Euler(290, transform.localRotation.eulerAngles.y, 0 /*transform.localRotation.eulerAngles.z*/);
        //

        if (Input.GetKey(resetKey)) transform.rotation = Quaternion.identity;


        //Translation Movement
        if (Input.GetKey(KeyCode.W)) transform.Translate(Vector3.forward * Time.deltaTime * walkSpeed);
        if (Input.GetKey(KeyCode.S)) transform.Translate(Vector3.back * Time.deltaTime * walkSpeed);
        if (Input.GetKey(KeyCode.A)) transform.Translate(Vector3.left * Time.deltaTime * walkSpeed);
        if (Input.GetKey(KeyCode.D)) transform.Translate(Vector3.right * Time.deltaTime * walkSpeed);
        //
    }
    //






    ////////Quaternion ClampRotationAroundXAxis(Quaternion q)
    ////////{
    ////////    q.x /= q.w;
    ////////    q.y /= q.w;
    ////////    q.z /= q.w;
    ////////    q.w = 1.0f;

    ////////    float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
    ////////    angleX = Mathf.Clamp(angleX, -clamp, clamp);
    ////////    q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

    ////////    return q;
    ////////}

}
