using System.Collections;
using UnityEngine;

namespace MGAssets
{
    public class CameraMove : MonoBehaviour
    {
        [Tooltip("Leave it empty for automatically getting the MainCamera")]
        public GameObject cameraGO;


        [Space]
        public bool translate = true;
        public bool rotate = true;

        [Space]
        public Vector3 positionOffSet;
        public Vector3 rotationOffSet;


        [Space]
        public bool smooth = false;
        public float translateTime = 0.05f;
        public float rotateTime = 0.10f;
        Vector3 translateVel;



        // Initialization
        void OnEnable() 
        { 
            if (!cameraGO) cameraGO = Camera.main.gameObject;
            translateVel = Vector3.zero;

            StopAllCoroutines();
            StartCoroutine("UpdateCam");
        }
        //



        //////////////////////////////////////////////////////////////////////////////////////// Update Camera Position
        IEnumerator UpdateCam()
        {
            while (true)
            {
                // Moves and Rotates the Camera-GameObject without smoothing (Instant Position)
                if (!smooth)
                {
                    if (translate) cameraGO.transform.position = transform.position + positionOffSet.x * transform.right + positionOffSet.y * transform.up + positionOffSet.z * transform.forward;
                    if (rotate) cameraGO.transform.rotation = transform.rotation * Quaternion.Euler(rotationOffSet);
                }
                else
                {
                    // Moves and Rotates the Camera-GameObject with smoothing
                    if (translate)
                    {
                        if (translateTime == 0) cameraGO.transform.position = transform.position + positionOffSet.x * transform.right + positionOffSet.y * transform.up + positionOffSet.z * transform.forward;
                        else if (translateTime != 0) cameraGO.transform.position = Vector3.SmoothDamp(cameraGO.transform.position, transform.position + positionOffSet.x * transform.right + positionOffSet.y * transform.up + positionOffSet.z * transform.forward, ref translateVel, translateTime * Time.fixedDeltaTime, Mathf.Infinity);
                    }

                    if (rotate)
                    {
                        if (rotateTime == 0) cameraGO.transform.rotation = transform.rotation * Quaternion.Euler(rotationOffSet);
                        else if (rotateTime != 0) cameraGO.transform.rotation = Quaternion.RotateTowards(cameraGO.transform.rotation, transform.rotation * Quaternion.Euler(rotationOffSet), 100f / rotateTime * Time.fixedDeltaTime);
                    }
                    //
                }

                yield return new WaitForFixedUpdate();
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////// Update Camera Position


    }
}