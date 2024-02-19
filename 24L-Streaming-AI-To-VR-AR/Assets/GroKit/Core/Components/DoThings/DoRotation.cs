using UnityEngine;

namespace Core3lb
{
    public class DoRotation : MonoBehaviour
    {
        public bool isRotating;
        public Vector3 rotateDirection;
        public float rotateSpeed;

        public void FixedUpdate()
        {
            if(isRotating)
            {
                transform.Rotate(rotateDirection * rotateSpeed);
            }
        }

        public void _SetSpeed(float speed)
        {
            rotateSpeed = speed;
        }

        public void _AddSpeed(float speed)
        {
            rotateSpeed += speed;
        }

        public void _ToggleRotate()
        {
            isRotating = !isRotating; 
        }

        public void _SetRotate(bool chg)
        {
            isRotating = chg;
        }
    }
}
