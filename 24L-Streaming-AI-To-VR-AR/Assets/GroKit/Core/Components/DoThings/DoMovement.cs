using System.Collections;
using UnityEngine;

namespace Core3lb
{
    public class DoMovement : MonoBehaviour
    {
        public float moveSpeed = 2;
        Coroutine moveTween;
        public float rotateSpeed = 2;
        Coroutine rotateTween;

        public void _MoveObjectHere(Transform WhereTo)
        {
            if(moveTween != null)
            {
                StopCoroutine(moveTween);

            }
            moveTween = StartCoroutine(MoveToTarget(WhereTo.position));
        }

        public void _ChangeRotateSpeed(float chg)
        {
            rotateSpeed = chg;
        }

        public void _ChangeMoveSpeed(float chg)
        {
            moveSpeed = chg;
        }

        //World Move BY

        public void _MoveByX(float x)
        {
            if (moveTween != null)
            {
                StopCoroutine(moveTween);

            }
            moveTween = StartCoroutine(MoveToTarget(transform.position + Vector3.right * x));
        }

        public void _MoveByY(float y)
        {
            if (moveTween != null)
            {
                StopCoroutine(moveTween);
            }
            moveTween = StartCoroutine(MoveToTarget(transform.position + Vector3.up * y));
        }

        public void _MoveByZ(float z)
        {
            if (moveTween != null)
            {
                StopCoroutine(moveTween);
            }
            moveTween = StartCoroutine(MoveToTarget(transform.position + Vector3.forward * z));
        }

        //Local Move By

        public void _LocalMoveByX(float x)
        {
            if (moveTween != null)
            {
                StopCoroutine(moveTween);

            }
            moveTween = StartCoroutine(MoveToTarget(transform.position + transform.TransformDirection(Vector3.right * x)));
        }

        public void _LocalMoveByY(float y)
        {
            if (moveTween != null)
            {
                StopCoroutine(moveTween);
            }
            moveTween = StartCoroutine(MoveToTarget(transform.position + transform.TransformDirection(Vector3.up * y)));
        }

        public void _LocalMoveByZ(float z)
        {
            if (moveTween != null)
            {
                StopCoroutine(moveTween);
            }
            moveTween = StartCoroutine(MoveToTarget(transform.position + transform.TransformDirection(Vector3.forward * z)));
        }

        //Local Move To

        public void _LocalMoveToX(float x)
        {
            if (moveTween != null)
            {
                StopCoroutine(moveTween);

            }
            Vector3 holder = transform.localPosition;
            holder.x = x;
            moveTween = StartCoroutine(LocalMoveToTarget(holder));
        }

        public void _LocalMoveToY(float y)
        {
            if (moveTween != null)
            {
                StopCoroutine(moveTween);
            }
            Vector3 holder = transform.localPosition;
            holder.y = y;
            moveTween = StartCoroutine(LocalMoveToTarget(holder));
        }

        public void _LocalMoveToZ(float z)
        {
            if (moveTween != null)
            {
                StopCoroutine(moveTween);
            }
            Vector3 holder = transform.localPosition;
            holder.z = z;
            moveTween = StartCoroutine(LocalMoveToTarget(holder));
        }

        //Rotation

        public void _RotateObjectHere(Quaternion targetRotation)
        {
            if (rotateTween != null)
            {
                StopCoroutine(rotateTween);
            }
            rotateTween = StartCoroutine(RotateToTarget(targetRotation));
        }

        //World Rotate By

        public void _RotateByX(float angle)
        {
            if (rotateTween != null)
            {
                StopCoroutine(rotateTween);
            }
            rotateTween = StartCoroutine(RotateToTarget(transform.rotation * Quaternion.Euler(angle, 0, 0)));
        }

        public void _RotateByY(float angle)
        {
            if (rotateTween != null)
            {
                StopCoroutine(rotateTween);
            }
            rotateTween = StartCoroutine(RotateToTarget(transform.rotation * Quaternion.Euler(0, angle, 0)));
        }

        public void _RotateByZ(float angle)
        {
            if (rotateTween != null)
            {
                StopCoroutine(rotateTween);
            }
            rotateTween = StartCoroutine(RotateToTarget(transform.rotation * Quaternion.Euler(0, 0, angle)));
        }

        //World Rotate To

        public void _RotateToX(float angle)
        {
            if (rotateTween != null)
            {
                StopCoroutine(rotateTween);
            }
            Vector3 holder = transform.rotation.eulerAngles;
            holder.x = angle;
            rotateTween = StartCoroutine(RotateToTarget(Quaternion.Euler(holder)));
        }

        public void _RotateToY(float angle)
        {
            if (rotateTween != null)
            {
                StopCoroutine(rotateTween);
            }
            Vector3 holder = transform.rotation.eulerAngles;
            holder.y = angle;
            rotateTween = StartCoroutine(RotateToTarget(Quaternion.Euler(holder)));
        }

        public void _RotateToZ(float angle)
        {
            if (rotateTween != null)
            {
                StopCoroutine(rotateTween);
            }
            Vector3 holder = transform.rotation.eulerAngles;
            holder.z = angle;
            rotateTween = StartCoroutine(RotateToTarget(Quaternion.Euler(holder)));
        }

        //Local Rotate To

        public void _LocalRotateToX(float angle)
        {
            if (rotateTween != null)
            {
                StopCoroutine(rotateTween);
            }
            Vector3 holder = transform.localRotation.eulerAngles;
            holder.x = angle;
            rotateTween = StartCoroutine(LocalRotateToTarget(Quaternion.Euler(holder)));
        }

        public void _LocalRotateToY(float angle)
        {
            if (rotateTween != null)
            {
                StopCoroutine(rotateTween);
            }
            Vector3 holder = transform.localRotation.eulerAngles;
            holder.y = angle;
            rotateTween = StartCoroutine(LocalRotateToTarget(Quaternion.Euler(holder)));
        }

        public void _LocalRotateToZ(float angle)
        {
            if (rotateTween != null)
            {
                StopCoroutine(rotateTween);
            }
            Vector3 holder = transform.localRotation.eulerAngles;
            holder.z = angle;
            rotateTween = StartCoroutine(LocalRotateToTarget(Quaternion.Euler(holder)));
        }

        //To Targets

        private IEnumerator MoveToTarget(Vector3 targetPosition)
        {
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, (moveSpeed * 5) * Time.deltaTime);
                yield return null;
            }
        }

        private IEnumerator RotateToTarget(Quaternion targetRotation)
        {
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * 10 * Time.deltaTime);
                yield return null;
            }
        }

        private IEnumerator LocalMoveToTarget(Vector3 targetPosition)
        {
            while (Vector3.Distance(transform.localPosition, targetPosition) > 0.01f)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, (moveSpeed * 5) * Time.deltaTime);
                yield return null;
            }
        }

        private IEnumerator LocalRotateToTarget(Quaternion targetRotation)
        {
            while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.01f)
            {
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, rotateSpeed * 10 * Time.deltaTime);
                yield return null;
            }
        }
    }
}
