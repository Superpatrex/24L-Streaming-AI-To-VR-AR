using UnityEngine;

namespace Core3lb
{

    public class PositionerSpatial : PositionerBase
    {
        [Header("-Position-")]
        public bool randomisePosition;
        [CoreShowIf("randomisePosition")]
        [SerializeField] bool useBox = false;

        [CoreHideIf("useBox")]
        [SerializeField] float spawnRadius;
        [CoreHideIf("useBox")]
        [SerializeField] bool isCircle = false;
        [CoreHideIf("useBox")]
        [SerializeField] bool cannotGoNeg;

        [CoreShowIf("useBox")]
        [SerializeField] BoxCollider myCollider;

        [CoreShowIf("randomiseRotation")]
        public bool yOnly;

        [CoreToggleHeader("OverlapDetection")]
        public bool useOverlapDetection;
        [CoreShowIf("useOverlapDetection")]
        public float obstacleCheckRadius = 3f;
        [CoreShowIf("useOverlapDetection")]
        public int maxAttempts = 10;
        [CoreReadOnly]
        [CoreShowIf("useOverlapDetection")]
        bool inOverlap;

        public override Vector3 WhatPosition(Transform whereNow)
        {
            if (randomisePosition)
            {
                Vector3 holder;
                if (useBox)
                {
                    holder = GetFromCollider();
                }
                else
                {
                    holder = GetCirclePoint();
                }
                if (useOverlapDetection)
                {
                    if (!inOverlap)
                    {
                        holder = OverlapChecker(holder);
                    }
                }
                return holder;
            }
            else
            {
                return whereNow.position;
            }
        }

        public Vector3 OverlapChecker(Vector3 where)
        {
            bool validPosition = false;
            int attempts = 0;
            Vector3 position = where;
            inOverlap = true;
            while (!validPosition && attempts < maxAttempts)
            {
                // Increase our spawn attempts
                attempts++;
                position = WhatPosition(transform);
                // Pick a random position
                validPosition = true;
                // Collect all colliders within our Obstacle Check Radius
                Collider[] colliders = Physics.OverlapSphere(position, obstacleCheckRadius);
                // Go through each collider collected
                foreach (Collider col in colliders)
                {
                    validPosition = false;
                }
            }
            inOverlap = false;
            return position;
        }

        public override Quaternion WhatRotation(Transform whereNow)
        {
            if (randomiseRotation)
            {
                Quaternion holder;
                if (yOnly)
                {
                    holder = Quaternion.Euler(new Vector3(whereNow.rotation.x, Random.Range(0, 360), whereNow.rotation.z));
                }
                else
                {
                    holder = Random.rotation;
                }
                return holder;
            }
            else
            {
                return whereNow.rotation;
            }
        }

        Vector3 GetCirclePoint()
        {
            Vector3 pos;
            if (cannotGoNeg)
            {
                var holder = Random.insideUnitSphere;
                holder.y = Mathf.Abs(holder.y);
                pos = holder * spawnRadius + transform.position;
            }
            else
            {
                pos = Random.insideUnitSphere * spawnRadius + transform.position;
            }

            if (isCircle)
            {
                pos.y = transform.position.y;
            }
            return pos;
        }

        Vector3 GetFromCollider()
        {
            return myCollider.GetRandomPointInsideCollider();
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            UnityEditor.Handles.color = Color.yellow;
            if (!isCircle)
            {

                Gizmos.DrawWireSphere(transform.position, spawnRadius);
            }
            else
            {
                UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, spawnRadius);
            }

        }
#endif
    }
}