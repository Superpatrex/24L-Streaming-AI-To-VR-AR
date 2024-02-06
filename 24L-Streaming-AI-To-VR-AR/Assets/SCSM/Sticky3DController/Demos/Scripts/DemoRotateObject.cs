// Copyright (c) 2015-2022 SCSM Pty Ltd. All rights reserved.
using UnityEngine;

namespace scsmmedia
{
    /// <summary>
    /// Use in demo platform rotation scene(s) to help test character setup and configuration.
    /// Object needs to have a non-kinematic rigidbody to rotate or more.
    /// WARNING
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    public class DemoRotateObject : MonoBehaviour
    {
        #region Public Variables

        public float speed = 10f;
        public float angleSpeed = 20f;
        public bool useFixedUpdate = false;
        public bool setMeshCollidersToConvex = false;

        public bool logSpeed;

        #endregion

        #region Private Variables

        private Rigidbody rBody;
        private float timer = 0f;
        private bool isInitialised = false;

        #endregion

        #region Initialisation Methods

        // Awake is called before the first frame update
        void Awake()
        {
            rBody = GetComponent<Rigidbody>();

            if (rBody != null)
            {
                if (!rBody.isKinematic && setMeshCollidersToConvex)
                {
                    // Non kinematic rigidbodies don't support non-convex mesh colliders
                    MeshCollider[] meshColliders = GetComponentsInChildren<MeshCollider>();

                    int numColliders = meshColliders == null ? 0 : meshColliders.Length;

                    for (int cIdx = 0; cIdx < numColliders; cIdx++)
                    {
                        meshColliders[cIdx].convex = true;
                    }
                }

                isInitialised = !rBody.isKinematic;
            }
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        private void Update()
        {
            if (!useFixedUpdate && isInitialised) { RotateObject(); }
        }

        // Called once per physics tick
        private void FixedUpdate()
        {
            if (useFixedUpdate && isInitialised) { RotateObject(); }
        }

        private void RotateObject()
        {
            if (rBody.isKinematic) { return; }

            timer += Time.deltaTime;
            rBody.angularVelocity = Vector3.one * Mathf.Clamp(timer * 1f, 0f, angleSpeed) * Mathf.Deg2Rad;

            rBody.velocity = Vector3.forward * Mathf.Clamp(timer * 1f, 0f, speed);

            #if UNITY_EDITOR
            if (logSpeed)
            {
                Debug.Log(gameObject.name + " Travelling " + rBody.velocity.magnitude + " m/s");
            }
            #endif
        }

        #endregion
    }
}
