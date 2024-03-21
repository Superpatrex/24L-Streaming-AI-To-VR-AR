using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MGAssets
{

    public class SpinningAround : MonoBehaviour
    {
        public bool isActive = true;

        public float factor = 1f;
        public Vector3 angularSpeed = new Vector3();

        public Vector3 worldPos = new Vector3();


        void Update()
        {
            //if (isActive) transform.Rotate(factor * angularSpeed * Time.deltaTime);

            if (isActive) transform.RotateAround(worldPos, angularSpeed, factor * angularSpeed.magnitude * Time.deltaTime);

        }
    }
}