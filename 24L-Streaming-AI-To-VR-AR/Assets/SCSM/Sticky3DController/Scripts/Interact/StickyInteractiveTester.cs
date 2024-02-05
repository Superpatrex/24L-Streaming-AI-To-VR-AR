using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This component can be used during development to quickly test if interaction
    /// is happening in the editor at runtime. It is NOT designed to be deployed with
    /// your final project.
    /// Add it to any empty gameobject in your scene.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Utilities/Sticky Interactive Tester")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyInteractiveTester : MonoBehaviour
    {
        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            // Remind developer that this is still in the scene
            Debug.Log("StickyInteractiveTester is on the " + transform.name + " gameobject. Do not to forget to remove it, and the events your added to objects, after testing.");
        }
        #endregion

        #region Public Generic Methods

        /// <summary>
        /// Call this from any event in Sticky3D
        /// </summary>
        public void GenericMethod()
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.GenericMethod called at " + Time.time + " on " + name);
        }

        /// <summary>
        /// Call this from any On Activated event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnActivated()
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnActivated called at " + Time.time + " on " + name);
        }

        /// <summary>
        /// Call this from any On Deactivated event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnDeactivated()
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.Deactivated called at " + Time.time + " on " + name);
        }

        /// <summary>
        /// Call this from any On Dropped event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnDropped()
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnDropped called at " + Time.time + " on " + name);
        }

        /// <summary>
        /// Call this from any On Grabbed event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnGrabbed()
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnGrabbed called at " + Time.time + " on " + name);
        }

        /// <summary>
        /// Call this from any On Hover Enter event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnHoverEnter()
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnHoverEnter called at " + Time.time + " on " + name);
        }

        /// <summary>
        /// Call this from any On Hover Exit event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnHoverExit()
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnHoverExit called at " + Time.time + " on " + name);
        }

        /// <summary>
        /// Call this from any On Post Grabbed event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnPostGrabbed()
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnPostGrabbed called at " + Time.time + " on " + name);
        }

        /// <summary>
        /// Call this from any On Post Stashed event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnPostStashed()
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnPostStashed called at " + Time.time + " on " + name);
        }

        /// <summary>
        /// Call this from any On Readable Value Changed event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnReadableValueChanged()
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnReadableValueChanged called at " + Time.time + " on " + name);
        }

        /// <summary>
        /// Call this from any On Selected event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnSelected()
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnSelected called at " + Time.time + " on " + name);
        }

        /// <summary>
        /// Call this from any On Touched event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnTouched()
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnTouched called at " + Time.time + " on " + name);
        }

        /// <summary>
        /// Call this from any On Stopped Touching event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnStoppedTouching()
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnStoppedTouching called at " + Time.time + " on " + name);
        }

        /// <summary>
        /// Call this from any On Unselected event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnUnselected()
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnUnselected called at " + Time.time + " on " + name);
        }

        #endregion

        #region Public Specific Methods

        /// <summary>
        /// Call this from any On Activated event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnActivated (int stickyInteractiveID, int stickyID, Vector3 future1, Vector3 future2)
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnActivated called at " + Time.time + " StickyInteractiveID: " + stickyInteractiveID + " StickyID: " + stickyID + " on " + name);
        }

        /// <summary>
        /// Call this from any On Deactivated event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnDeactivated (int stickyInteractiveID, int stickyID)
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnDeactivated called at " + Time.time + " StickyInteractiveID: " + stickyInteractiveID + " StickyID: " + stickyID + " on " + name);
        }

        /// <summary>
        /// Call this from any On Dropped event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnDropped (int stickyInteractiveID, int stickyID)
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnDropped called at " + Time.time + " StickyInteractiveID: " + stickyInteractiveID + " StickyID: " + stickyID + " on " + name);
        }

        /// <summary>
        /// Call this from any On Grabbed event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnGrabbed (Vector3 hitPoint, Vector3 hitNormal, int stickyInteractiveID, int stickyID)
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnGrabbed called at " + Time.time +
            " Hit Point: " + hitPoint + " Hit Normal: " + hitNormal +
            " StickyInteractiveID: " + stickyInteractiveID + " StickyID: " + stickyID + " on " + name);
        }

        /// <summary>
        /// Call this from any On Hover Enter event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnHoverEnter (Vector3 hitPoint, Vector3 hitNormal, int stickyInteractiveID, int stickyID)
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnHoverEnter called at " + Time.time + 
            " Hit Point: " + hitPoint + " Hit Normal: " + hitNormal +
            " StickyInteractiveID: " + stickyInteractiveID + " StickyID: " + stickyID + " on " + name);
        }

        /// <summary>
        /// Call this from any On Hover Exit event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnHoverExit (int stickyInteractiveID, int stickyID)
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnHoverExit called at " + Time.time + " StickyInteractiveID: " + stickyInteractiveID + " StickyID: " + stickyID + " on " + name);
        }

        /// <summary>
        /// Call this from any On Post Grabbed event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnPostGrabbed (Vector3 hitPoint, Vector3 hitNormal, int stickyInteractiveID, int stickyID)
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnPostGrabbed called at " + Time.time +
            " Hit Point: " + hitPoint + " Hit Normal: " + hitNormal +
            " StickyInteractiveID: " + stickyInteractiveID + " StickyID: " + stickyID + " on " + name);
        }

        /// <summary>
        /// Call this from any On Post Stashed event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnPostStashed(int stickyInteractiveID, int stickyID, int storeItemID, Vector3 futureValue)
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnPostStashed called at " + Time.time +
            " StickyInteractiveID: " + stickyInteractiveID + " StickyID: " + stickyID +
            " StoreItemID: " + storeItemID + " on " + name
            );
        }

        /// <summary>
        /// Call this from any On Readable Value Changed event on an object containing an StickyInteractive component.
        /// </summary>
        /// <param name="stickyInteractiveID"></param>
        /// <param name="currentValue"></param>
        /// <param name="previousValue"></param>
        /// <param name="notused"></param>
        public void OnReadableValueChanged (int stickyInteractiveID, Vector3 currentValue, Vector3 previousValue, Vector3 notused)
        {
            Debug.Log("StickyInteractiveTester.OnReadableValueChanged stickyInteractiveID: " + stickyInteractiveID + " previous: " + previousValue + " current: " + currentValue + " on " + name);
        }

        /// <summary>
        /// Call this from any On Selected event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnSelected(int stickyInteractiveID, int stickyID, int storeItemID)
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnSelected called at " + Time.time +
            " StickyInteractiveID: " + stickyInteractiveID + " StickyID: " + stickyID +
            " StoreItemID: " + storeItemID + " on " + name);
        }

        /// <summary>
        /// Call this from any On Touched event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnTouched (Vector3 hitPoint, Vector3 hitNormal, int stickyInteractiveID, int stickyID)
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnTouched called at " + Time.time +
            " Hit Point: " + hitPoint + " Hit Normal: " + hitNormal +
            " StickyInteractiveID: " + stickyInteractiveID + " StickyID: " + stickyID + " on " + name);
        }

        /// <summary>
        /// Call this from any On Stopped Touching event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnStoppedTouching(int stickyInteractiveID, int stickyID)
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnStoppedTouching called at " + Time.time + " StickyInteractiveID: " + stickyInteractiveID + " StickyID: " + stickyID + " on " + name);
        }

        /// <summary>
        /// Call this from any On Unselected event on an object containing an StickyInteractive component.
        /// </summary>
        public void OnUnselected (int stickyInteractiveID, int stickyID)
        {
            Debug.Log("[DEBUG] StickyInteractiveTester.OnUnselected called at " + Time.time + " StickyInteractiveID: " + stickyInteractiveID + " StickyID: " + stickyID + " on " + name);
        }

        #endregion

    }
}