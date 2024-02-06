using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Used in CottageCameraDemo scene to show a scenario of switch between gameplay
    /// camera control and S3D camera control (there are many other possible options).
    /// We use the StickyMovingPlatform to move the main camera for convience in this
    /// demo. This is simply to signify that something other than the S3D character is
    /// controlling the camera and general gameplay.
    /// Once the user has control they can toggle between 3rd and 1st Person with "V" key,
    /// which is configured in StickyInputModule.
    /// WARNING
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    public class DemoS3DCottageCamera : MonoBehaviour
    {
        #region Public Variables
        public Camera mainGamePlayCamera = null;
        public StickyControlModule playerCharacter = null;

        #endregion

        #region Private Variables
        private bool isInitialised = false;
        private StickyMovingPlatform s3dPlatform = null;
        private StickyInputModule playerInputModule = null;
        private bool playerHasControl = false;
        #endregion

        #region Initialisation Methods

        // Start is called before the first frame update
        void Start()
        {
            if (mainGamePlayCamera == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: Cannot find the main gameplay camera. Ensure it is added to the mainGamePlayCamera slot in this script");
                #endif
            }
            else if (playerCharacter == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: Cannot find the Sticky3D Controller. Did you add a S3D character to the scene, then drag it into the script slot provided?");
                #endif
            }
            else
            {
                s3dPlatform = mainGamePlayCamera.GetComponent<StickyMovingPlatform>();
                playerInputModule = playerCharacter.GetComponent<StickyInputModule>();

                if (s3dPlatform == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: Cannot find the StickyMovingPlatform component which should be attached to the mainGamePlayCamera for this demo.");
                    #endif
                }
                else if (playerInputModule == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: There does not seem to be a StickyInputModule attached to your Sticky3D Controller.");
                    #endif
                }
                else
                {
                    // Start with no wait time so camera starts moving immediately
                    s3dPlatform.waitTime = 0f;

                    // After the camera starts moving, set the wait time so that we know when
                    // the platform has reached the end of it's journey towards the cottage.
                    Invoke("SetPlatformWaitTime", 1f);

                    // Make sure the S3D controller and input modules are initialised
                    if (!playerCharacter.IsInitialised) { playerCharacter.Initialise(); }
                    if (!playerInputModule.IsInitialised) { playerInputModule.Initialise(); }

                    // Prevent user input for the player
                    playerInputModule.DisableInput(false);

                    // Turn off Player cameras
                    playerCharacter.DisableLook();

                    isInitialised = true;
                }
            }
        }

        #endregion

        #region Update Methods
        // Update is called once per frame
        void Update()
        {
            if (isInitialised)
            {
                if (!playerHasControl && s3dPlatform.IsWaitingAtPosition)
                {
                    // Switch control to the Sticky3D Controller

                    // We could turn off the main gameplay or menu camera
                    //mainGamePlayCamera.enabled = false;

                    // Turn off the platform movement
                    s3dPlatform.move = false;

                    playerCharacter.lookCameraOffset = new Vector3(0f, 1.8f, -2f);

                    // In this demo, we going to use the main camera as also our 3rd person camera.
                    // However, we don't want to cut directly to the 3rd person camera position,
                    // instead, we want to blend towards the offset position.
                    playerCharacter.SetLookThirdPerson(mainGamePlayCamera, true, false);

                    // Permit the user to control the character
                    playerInputModule.EnableInput();

                    playerHasControl = true;
                }
            }
        }

        #endregion

        #region Private Methods

        private void SetPlatformWaitTime()
        {
            s3dPlatform.waitTime = 5f;
        }

        #endregion
    }
}