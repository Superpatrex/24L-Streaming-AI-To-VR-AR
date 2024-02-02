using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2015-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// [FUTURE - WIP]
    /// Class that handles LocalSpace floating origin.
    /// </summary>
    [RequireComponent(typeof(SystemSpaceManager))]
    public class FloatingOriginManager : MonoBehaviour
    {
        #region Public Variables

        /// <summary>
        /// The maximum distance LocalObserverPosition can be from the origin before ShiftFloatingOrigin is called.
        /// </summary>
        public float observerMaxDistance = 100f;

        /// <summary>
        /// If system space position is translated when the origin is reset.
        /// </summary>
        public bool moveSystemSpace = true;

        #endregion

        #region Private Variables

        // Position of the local observer (player or player average)
        private Vector3 localObserverPosition = Vector3.zero;

        // ObserverMaxDistance but squared (for faster distance comparison)
        private float observerMaxSqrDistance = 0f;

        // List of registered floating transform interfaces that will have Shift called when the Floating Origin shifts
        private List<IFloatingTransform> floatingTransforms = new List<IFloatingTransform>();

        // The local SystemSpace class, used for shifting the system position
        private SystemSpaceManager systemSpace;

        #endregion

        #region Initialisation Methods

        // Awake is called before the first frame update
        void Awake()
        {
            systemSpace = GetComponent<SystemSpaceManager>();

            // Square the observerMaxDistance
            observerMaxSqrDistance = observerMaxDistance * observerMaxDistance;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets LocalObserverPosition and checks if Floating Origin should be shifted.
        /// </summary>
        /// <param name="oPosition"></param>
        public void SetLocalObserverPosition(Vector3 oPosition)
        {
            localObserverPosition = oPosition;

            if (ObserverOutsideBounds())
            {
                ShiftFloatingOrigin();
            }
        }

        /// <summary>
        /// Registers a IFloatingTransform with the FloatingOrigin class so it has Shift called when the origin is updated.
        /// </summary>
        /// <param name="fTransform"></param>
        public void RegisterFloatingTransform(IFloatingTransform fTransform)
        {
            if (fTransform != null)
            {
                floatingTransforms.Add(fTransform);
            }
        }

        /// <summary>
        /// Removes the IFloatingTransform from the FloatingOrigin (use before destroying an object).
        /// </summary>
        /// <param name="fTransform"></param>
        public void RemoveFloatingTransform(IFloatingTransform fTransform)
        {
            floatingTransforms.Remove(fTransform);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if the LocalObserverPosition is further from the Origin than allowed.
        /// </summary>
        /// <returns></returns>
        private bool ObserverOutsideBounds()
        {
            return localObserverPosition.sqrMagnitude > observerMaxSqrDistance;
        }

        /// <summary>
        /// Resets the Floating Origin.
        /// </summary>
        private void ShiftFloatingOrigin()
        {
            // Calculate shiftDelta
            Vector3 shiftDelta = -localObserverPosition;

            // Apply shiftdelta to localObserverpos, and all registered floatingTransforms
            localObserverPosition = Vector3.zero;

            // Loop through all floating transforms and perform a shift on each of them
            int nFloatingTransforms = floatingTransforms.Count;
            IFloatingTransform fTransform;
            for (int i = 0; i < floatingTransforms.Count; i++)
            {
                fTransform = floatingTransforms[i];
                if (fTransform != null)
                {
                    fTransform.FloatingTransformShift(shiftDelta);
                }
            }

            if (moveSystemSpace && systemSpace != null)
            {
                // Apply shiftDelta to SystemSpace coords
                systemSpace.AddLocalDelta(-shiftDelta);
            }
        }

        #endregion
    }
}
