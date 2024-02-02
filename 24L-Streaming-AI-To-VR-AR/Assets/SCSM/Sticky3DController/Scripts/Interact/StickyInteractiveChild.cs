using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This small component, can be used to make child non-trigger colliders of an
    /// interactive-enabled object discoverable in the scene.
    /// To avoid using this component, add the non-trigger colliders to the parent
    /// gameobject which contains the StickyInteractive component OR add a rigidbody
    /// to the parent gameobject.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Objects/Sticky Interactive Child")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyInteractiveChild : MonoBehaviour
    {
        #region Public Variables
        public StickyInteractive stickyInteractive = null;

        #endregion

        #region Private Variables

        #endregion

        #region Initialise Methods

        #if UNITY_EDITOR
        // Start is called before the first frame update
        void Start()
        {
            if (stickyInteractive == null)
            {
                Debug.LogWarning("ERROR The StickyInteractiveChild on " + gameObject.name + " does not have a reference to its parent StickyInteractive component");
            }

            // We "could" also check for a collider but that will just consume more time when scene loads
        }
        #endif

        #endregion

    }
}