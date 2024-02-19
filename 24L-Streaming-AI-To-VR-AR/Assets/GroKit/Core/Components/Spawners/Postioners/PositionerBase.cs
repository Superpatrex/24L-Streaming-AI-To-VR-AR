using UnityEngine;

namespace Core3lb
{
    public class PositionerBase : MonoBehaviour
    {
        [Header("-Rotation-")]
        public bool randomiseRotation;

        public virtual Vector3 WhatPosition(Transform whereNow = null)
        {
            return transform.position;
        }
        public virtual Quaternion WhatRotation(Transform whereNow = null)
        {
            if(randomiseRotation)
            {
                return Random.rotation;
            }
            return transform.rotation;
        }
    }
}
