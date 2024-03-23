using UnityEngine;

namespace Core3lb
{
    public class Facing : MonoBehaviour
    {
        public Transform faceTo;
        public bool facePlayer = true;
        public bool isFacing = true;
        public float turnSpeed = 1.0f;

        public bool scaleToDistance;
        public float ScaleMultiplier = 0.5f;
        private Vector3 initialScale;
        Vector3 scaleTo;

        void Awake()
        {
            initialScale = transform.localScale;
        }

        private void FixedUpdate()
        {
            if (isFacing)
            {
                FaceObject();
            }
            if (scaleToDistance)
            {
                ScaleObject();
            }
        }

        void FaceObject()
        {
            Vector3 targetPosition;
            if (facePlayer && Camera.main)
            {
                targetPosition = Camera.main.transform.position;
            }
            else
            {
                targetPosition = faceTo.transform.position;
            }
            targetPosition.y = transform.position.y; // Keep the object's y-position constant
            var targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }

        void ScaleObject()
        {
            if (facePlayer && Camera.main)
            {
                scaleTo = Camera.main.transform.position;
            }
            else
            {
                scaleTo = faceTo.transform.position;
            }
            float distance = Vector3.Distance(scaleTo, transform.position);
            transform.localScale = Mathf.Sqrt(distance) * ScaleMultiplier * initialScale;
        }

        public void _FaceTo(GameObject loc)
        {
            faceTo = loc.transform;
            isFacing = true;
        }

        public void _FacingToggle()
        {
            isFacing = !isFacing;
        }

        public void _FacingOff()
        {
            isFacing = false;
        }

        public void _FacingOn()
        {
            isFacing = true;
        }
    }
}