using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Sample script to show reading values from an interactive lever.
    /// This example is for VR but you could also use something similar in a non-VR project.
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// Setup:
    /// 1. Setup your scene for VR using the instructions in the manual.
    /// 2. Add the Demos\Prefabs\Props\S3D_Lever1 prefab to the scene
    /// 3. Add a world-space canvas with a Text control
    /// 4. Set the canvas scale to 0.0001, 0.0001, 0.0001.
    /// 5. Set the canvas event camera to the S3D character XR Camera.
    /// 6. Move the canvas and Text so that it can be seen by the player
    /// 6. Create an empty gameobject called "Sample Lever Value"
    /// 7. Add this component tothe empty gameobject
    /// 8. In the scene, expand the S3D_Lever1 gameobject and locate the "Lever" child object
    /// 9. Drag that child object, which contains the StickyInteractive component into this component
    /// 10. Drag the UI Text component into this component
    /// 11. In the scene, expand the S3D_Lever1 gameobject and locate the "Lever" child object and go to Events tab
    /// 12. Add a new On Readable Value Changed event
    /// 13. Drag the "Sample Lever Value" gameobject into the "Object" field of the event
    /// 14. 
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/VR Lever Value")]
    [DisallowMultipleComponent]
    public class SampleLeverValue : MonoBehaviour
    {
        #region Public Variables
        public bool initialiseOnStart = false;

        public StickyInteractive interactiveLever = null;

        public UnityEngine.UI.Text leverText = null;

        [Tooltip("Does the lever move left to right, rather than forward and back?")]
        public bool isLeftRightLever = false;
        #endregion

        #region Private Variables
        private bool isInitialised = false;

        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Public Methods

        public void Initialise()
        {
            if (interactiveLever == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleLeverValue - did you forget to add a Lever prefab to the scene, then drag in the child interactive object?");
                #endif
            }
            else if (leverText == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleLeverValue - setup a simple world space canvas with a Text component, and reference it in the leverText slot provided.");
                #endif
            }
            else
            {
                isInitialised = true;
            }
        }

        #endregion

        #region Public Callback Methods

        /// <summary>
        /// This is automatically called by Sticky3D when you hook it up to the lever's OnReadableValueChanged event
        /// on the interactive-enabled object.
        /// </summary>
        /// <param name="stickyInteractiveID"></param>
        /// <param name="currentValue"></param>
        /// <param name="previousValue"></param>
        /// <param name="notused"></param>
        public void OnLeverChanged (int stickyInteractiveID, Vector3 currentValue, Vector3 previousValue, Vector3 notused)
        {
            if (isInitialised)
            {
                leverText.text = System.Math.Round(isLeftRightLever ? currentValue.x : currentValue.z, 2).ToString("0.00");

                //Debug.Log("OnLeverChanged  stickyInteractiveID: " + stickyInteractiveID + " prev: " + previousValue + " current: " + currentValue);
            }
        }

        #endregion
    }
}