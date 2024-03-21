using UnityEngine;

namespace MGAssets
{

    public class Spinning : MonoBehaviour
    {
        public bool isActive = true;

        public float factor = 1f;
        public Vector3 angularSpeed = new Vector3();

        void Update()
        {
            if (isActive) transform.Rotate(factor * angularSpeed * Time.deltaTime);
        }
    }
}