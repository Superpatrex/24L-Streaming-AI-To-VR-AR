using UnityEngine;

// Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Small opitional component added to the feet of a character to detect collisions with the ground
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    public class StickyFoot : MonoBehaviour
    {
        #region Private Variables
        private Collider footCollider;
        private StickyControlModule stickyControlModule = null;
        private bool isInitialised = false;
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            if (!TryGetComponent(out footCollider))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: StickyFoot on " + gameObject.name + " requires a Trigger Collider so that it can detect contact with the ground");
                #endif
            }
            else if (!footCollider.isTrigger)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: StickyFoot - the Collider on " + gameObject.name + " must be a Trigger Collider to record contact with the ground");
                #endif
            }
            else
            {
                Rigidbody rb = footCollider.attachedRigidbody;
                if (rb != null)
                {
                    stickyControlModule = rb.GetComponent<StickyControlModule>();

                    isInitialised = stickyControlModule != null;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Called when the foot collider hits a non-trigger collider
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (isInitialised && !stickyControlModule.IsColliderSelf(other))
            {
                stickyControlModule.PlayFootStep();
            }
        }

        #endregion
    }
}