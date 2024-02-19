using UnityEngine;

namespace Core3lb
{
    public class FollowTarget : MonoBehaviour
    {
        public Transform target;
        [CoreHeader("Settings")]
        Vector3 positionOffset;
        Quaternion rotationOffset = Quaternion.identity;
        public bool instantFollow = false;
        public bool followOnStart = false;
        [CoreShowIf("followOnStart")]
        public bool offsetPosOnStart = false;
        [CoreShowIf("followOnStart")]
        public bool offsetRotOnStart = false;


        [CoreToggleHeader("Position")]
        public bool followPosition = true;
        [CoreShowIf("followPosition")]
        public bool followX = true;
        [CoreShowIf("followPosition")]
        public bool followY = true;
        [CoreShowIf("followPosition"),]
        public bool followZ = true;
        [CoreShowIf("followPosition"),]
        public float positionTime = .5f;
        [CoreToggleHeader("Rotation")]
        public bool followRotation = true;
        [CoreShowIf("followRotation")]
        public bool followXrot = true;
        [CoreShowIf("followRotation")]
        public bool followYrot = true;
        [CoreShowIf("followRotation")]
        public bool followZrot = true;
        [CoreShowIf("followRotation")]
        public float rotationSpeed = 5.0f;
        [Tooltip("Z Will be foward for look at")]
        public bool lookAtTarget = false;


        private bool isFollowing = false;
        private Vector3 velocity = Vector3.zero;
        private Quaternion targetRotation;

        public virtual void Start()
        {
            if (followOnStart)
            {
                if (offsetPosOnStart)
                {
                    _StartFollowWithOffset();
                }
                else
                {
                    _StartFollow();
                }

            }
        }

        void Update()
        {
            if (isFollowing)
            {
                transform.position = PositionSolver();
                transform.rotation = RotationSolver();
            }
        }

        public virtual Vector3 PositionSolver()
        {
            if (!followPosition)
            {
                return transform.position;
            }

            Vector3 targetPosition = target.position + positionOffset;

            // Apply the followX, followY, followZ constraints
            if (!followX) targetPosition.x = transform.position.x;
            if (!followY) targetPosition.y = transform.position.y;
            if (!followZ) targetPosition.z = transform.position.z;

            if (instantFollow)
            {
                return targetPosition;
            }
            else
            {
                return Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, positionTime);
            }
        }

        public virtual Quaternion RotationSolver()
        {
            Quaternion result;
            if (!followRotation)
            {
                return transform.rotation;
            }
            if (lookAtTarget)
            {
                Vector3 direction = target.position - transform.position;
                result = Quaternion.LookRotation(direction);
            }
            else
            {
                if(rotationOffset == Quaternion.identity)
                {
                    result = target.rotation;
                }
                else
                {
                    result = target.rotation * rotationOffset;
                }

            }
            // Apply the followX, followY, followZ constraints
            Vector3 eulerResult = result.eulerAngles;
            Vector3 currentEuler = transform.rotation.eulerAngles;

            result = Quaternion.Euler(
                followXrot ? eulerResult.x : currentEuler.x,
                followYrot ? eulerResult.y : currentEuler.y,
                followZrot ? eulerResult.z : currentEuler.z
            );

            if (instantFollow)
            {
                return result;
            }
            else
            {
                return Quaternion.Slerp(transform.rotation, result, Time.deltaTime * rotationSpeed);
            }
        }

        public void _ChangeTarget(Transform myTarget)
        {
            target = myTarget;
        }

        [CoreButton]
        public void _StartFollow()
        {
            isFollowing = true;
        }
        [CoreButton]
        public void _StopFollow()
        {
            isFollowing = false;
        }
        [CoreButton]
        public void _StartFollowWithOffset()
        {
            positionOffset = transform.position - target.position;
            if(offsetRotOnStart)
            {
                rotationOffset = Quaternion.Inverse(target.rotation) * transform.rotation;
            }
            else
            {
                rotationOffset = Quaternion.identity;
            }

            _StartFollow();
        }




        private void RotateTowardsTarget()
        {
            targetRotation = target.rotation * rotationOffset;

        }

        private void LookAtTarget()
        {
            Vector3 direction = transform.position - target.position;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;
        }
        
        //CHECK THIS LATER!
        //private void LookAtTargetInstant()
        //{
        //    Vector3 direction = transform.position - target.position;

        //    Quaternion lookRotation = Quaternion.LookRotation(direction);
        //    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        //}


        private void SetRotation()
        {
            transform.rotation = target.rotation * rotationOffset;
        }
    }
}