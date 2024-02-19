
using UnityEngine;

namespace Core3lb
{
    public class AttachToView : MonoBehaviour
    {

        public Transform playerController;
        public Transform headCamera;
        Vector3 currentPosition;
        public float distanceCheck = 5;
        public float smoothTime = 0.1f;

        void Start()
        {
            transform.parent = playerController;
            transform.localPosition = new Vector3(0, 0, 0);
            transform.localRotation = new Quaternion(0, 0, 0, 0);
            transform.localScale = new Vector3(1, 1, 1);
        }

        private void FixedUpdate()
        {
            if (Vector3.Distance(currentPosition, headCamera.eulerAngles) > distanceCheck)
            {
                currentPosition = new Vector3(transform.eulerAngles.x, headCamera.transform.eulerAngles.y, transform.eulerAngles.z);

                Quaternion targetRotation = Quaternion.Euler(currentPosition);
                float step = smoothTime * Time.fixedDeltaTime;

                // Gradually rotate the object towards the target rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, step);
            }
        }
    }
}