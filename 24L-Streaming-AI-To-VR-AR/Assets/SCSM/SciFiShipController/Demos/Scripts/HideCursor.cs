using System.Collections;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Sample script hide/show the mouse pointer due to mouse (in)activity.
    /// Drop it onto a gameobject in the scene.
    /// </summary>
    public class HideCursor : MonoBehaviour
    {
        #region Public varibles and properties
        
        #region Cursor variables
        public float hideCursorTime = 3f;
        private bool isCursorVisible = true;
        private float cursorTimer = 0f;

        // Switch to using the New Input System if it is available
        #if SSC_UIS
        private Vector2 currentMousePosition = Vector2.zero;
        private Vector2 lastMousePosition = Vector2.zero;
        #else
        private Vector3 currentMousePosition = Vector3.zero;
        private Vector3 lastMousePosition = Vector3.zero;
        #endif
        #endregion

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            #region Show or Hide Cursor
            // After x seconds of inactivity, hide the (mouse) cursor
            // Use the New Input System if it is available
            #if SSC_UIS
            currentMousePosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            #else
            currentMousePosition = Input.mousePosition;
            #endif            
            if (isCursorVisible)
            {
                cursorTimer += Time.deltaTime;
                // If use has move the mouse, reset the timer
                if (lastMousePosition != currentMousePosition) { lastMousePosition = currentMousePosition; cursorTimer = 0f; }
                // After hideCursorTime secs, hide it
                else if (cursorTimer > hideCursorTime) { ShowCursor(false); }
            }
            // Check if mouse has moved (does user wish to click on something?)
            else if (lastMousePosition != currentMousePosition)
            {
                lastMousePosition = currentMousePosition;
                ShowCursor(true);
            }
            #endregion
        }

        #endregion

        #region Private Methods


        #endregion

        #region Public Methods

        /// <summary>
        /// Show, or hide the cursor.
        /// NOTE: This will sometimes fail to turn off the cursor in the editor
        /// Game View when it doesn't have focus, but will work fine in a build.
        /// </summary>
        /// <param name="isVisible"></param>
        public void ShowCursor(bool isVisible)
        {
            Cursor.visible = isVisible;
            isCursorVisible = isVisible;
            if (isVisible) { cursorTimer = 0f; }
        }

        #endregion
    }
}