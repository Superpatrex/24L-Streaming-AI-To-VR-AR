using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This component, currently in Technical Preview enables a VR hand on a Sticky3D character
    /// to touch interactive-enabled objects in the scene.
    /// It does this by sending trigger collider events to the StickyXRInteractor component.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Character/Sticky XR Touch")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyXRTouch : MonoBehaviour
    {
        #region Public Variables
        [Tooltip("The Sticky XR Interactor component for this hand")]
        public StickyXRInteractor stickyXRInteractor = null;

        #endregion

        #region Private Variables
        private bool isInitialised = false;
        [System.NonSerialized] private Collider handCollider;
        [System.NonSerialized] private StickyInteractive stickyInteractive = null;
        [System.NonSerialized] private StickyInteractiveChild stickyInteractiveChild = null;
        //private int handColliderID = 0;
        private StickyControlModule stickyControlModule = null;
        private int stickyID = 0;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            if (stickyXRInteractor == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: The StickyXRTouch component on " + name + " requires a referene to the stickyXRInteractor");
                #endif
            }
            else if (!TryGetComponent<Collider>(out handCollider))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: The StickyXRTouch component requires a trigger collider on " + name);
                #endif
            }
            else if (stickyXRInteractor.stickyControlModule == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: The StickyXRTouch component on " + name + " cannot find the Sticky Control Module reference on its stickyXRInteractor.");
                #endif
            }
            else
            {
                stickyControlModule = stickyXRInteractor.stickyControlModule;
                stickyID = stickyControlModule.StickyID;

                //handColliderID = handCollider.GetInstanceID();

                isInitialised = handCollider.isTrigger;
            }
        }

        #region Event Methods


        private void OnTriggerEnter(Collider other)
        {
            if (!isInitialised) { return; }
            // Ignore any colliders registered with the character.
            else if (!stickyControlModule.IsColliderSelf(other))
            {
                if (!other.TryGetComponent(out stickyInteractive))
                {
                    if (other.TryGetComponent(out stickyInteractiveChild))
                    {
                        stickyInteractive = stickyInteractiveChild.stickyInteractive;
                    }
                }

                if (stickyInteractive != null)
                {
                    if (stickyInteractive.IsTouchable && !stickyXRInteractor.CheckHeldInteractive())
                    {
                        Vector3 handColliderPos = handCollider.bounds.center;

                        Vector3 hitPoint = other.ClosestPointOnBounds(handColliderPos);
                        Vector3 hitNormal = (handColliderPos - hitPoint).normalized;

                        stickyXRInteractor.TouchInteractive(stickyInteractive, hitPoint, hitNormal);

                        //Debug.Log("[DEBUG] StickyXRTouch OnTriggerEnter " + name + " is touching " + other.name + " T:" + Time.time);
                    }
                }
            }
        }


        private void OnTriggerExit(Collider other)
        {
            if (!isInitialised) { return; }
            // Ignore any colliders registered with the character
            else if (!stickyControlModule.IsColliderSelf(other))
            {
                if (!other.TryGetComponent(out stickyInteractive))
                {
                    if (other.TryGetComponent(out stickyInteractiveChild))
                    {
                        stickyInteractive = stickyInteractiveChild.stickyInteractive;
                    }
                }

                if (stickyInteractive != null)
                {
                    if (stickyInteractive.IsTouchable)
                    {
                        stickyXRInteractor.StopTouchingInteractive(stickyInteractive);

                        //Debug.Log("[DEBUG] StickyXRTouch OnTriggerExit " + name + " other: " + other.name + " T:" + Time.time);
                    }
                }
            }
            
        }

        #endregion
    }
}