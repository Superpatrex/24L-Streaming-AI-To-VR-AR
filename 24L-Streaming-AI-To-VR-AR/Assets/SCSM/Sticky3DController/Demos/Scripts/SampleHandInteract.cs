using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Sample script to demonstrate a Sticky3D character (S3D) interacting with an object in the scene using Hand IK.
    /// Setup:
    /// 1. Add a Sticky3D player character to your test scene (eg PlayerJaneSuited)
    /// 2. Add the StickyDisplay1 prefab from Demos\Prefabs\Visuals to the scene
    /// 3. Create an empty gameobject in the scene and rename it SampleInteract.
    /// 4. Add this component the the empty gameobject
    /// 5. On the Hand Interact component, tick "Initialise on Start"
    /// 6. Add the S3D player from the scene to this component
    /// 7. Add the Sticky3D Display Module from the scene to this component
    /// 8. On the Sticky3D player's Sticky Input Module, add a Custom Input
    /// 9. Configure a Interactive button on the Custom Input e.g. "I" if InputMode is DirectKeyboard
    /// 10. Add a new Callback method to the Custom Input and drag the SampleInteract gameobject into the slot
    /// 11. For the Function, select SampleHandInteract, ToggleInteractive().
    /// 12. Add another Custom Input
    /// 13. Configure a Touch or Target button on the Custom Input e.g. Mouse0 if InputMode is DirectKeyboard
    /// 14. Add a new Callback method to the Custom Input and drag the Sticky3D player gameobject into the slot
    /// 15. For the Function, select StickyControlModule, SetRightHandIKTargetLookingAtInteractive().
    /// 16. Add another Custom Input
    /// 17. Configure a Interactive button on the Custom Input e.g. "G" if InputMode is DirectKeyboard
    /// 18. Add a new Callback method to the Custom Input and drag the Sticky3D player gameobject into the slot
    /// 19. For the Function, select StickyControlModule, ToggleHoldInteractive(bool). Leave the bool unticked.
    /// 20. Add a small object to the scene (could be a small cube, ball, cup etc)
    /// 21. Set the object approx 1 metre off the ground
    /// 22. Add a StickyInteractive component to the object
    /// 23. Tick "Initialise on Start", "Is Grabbable", "Parent on Grab", and "Is Touchable"
    /// Trouble Shooting:
    /// 1. If you can grab items but not reach out and touch them ensure the Animator Controller has IK Pass enabled
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Hand Interact")]
    [DisallowMultipleComponent]
    public class SampleHandInteract : MonoBehaviour
    {
        #region Public Variables
        public bool initialiseOnStart = false;

        [Tooltip("Automatically turn on Interactive during initialisation")]
        public bool autoEnableOnStart = false;

        [Tooltip("The S3D player character in the scene")]
        public StickyControlModule stickyPlayer = null;
        [Tooltip("The Sticky Display Module in the scene - used to select the object with the Reticle")]
        public StickyDisplayModule stickyDisplayModule = null;

        public float maxReachDistance = 5f;

        [Tooltip("Should the text be displayed and update on the screen?")]
        public bool showText = true;

        [Tooltip("Use Head IK with Third Person")]
        public bool useHeadIK = false;
        #endregion

        #region Private Static Strings
        private readonly static string interactiveOffText = "<b>Interactive (I)</b>: OFF";
        private readonly static string interativeOnText = "<b>Interactive (I)</b>: ON";
        private readonly static string lookingAtNothingText = "<b>Looking At</b>: ";
        private readonly static string targetNothingText = "<b>Target</b>: ";
        private readonly static string keysText = "<b>Keys</b>: Grab (G)";

        #endregion

        #region Private Variables
        private bool isInitialised = false;
        private S3DDisplayMessage dmInteractiveActive = null;
        private S3DDisplayMessage dmLookingAt = null;
        private S3DDisplayMessage dmTarget = null;
        private S3DDisplayMessage dmKeys = null;
        // Can the character grab objects by pointing at them and clicking on them?
        private bool isInteractive = false;
        private StickyInteractive lookingAtObject = null;
        private int lookingAtObjectId = StickyInteractive.NoID;
        private Color32 defaultReticleColour;
        private Color32 lookingAtReticleColour;
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Private Methods

        // Make sure the Sticky Display Module will work with this sample
        private bool ConfigureDisplayModule()
        {
            bool isConfigured = false;

            if (stickyDisplayModule.GetNumberDisplayReticles < 1)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleHandInteract - " + stickyDisplayModule.name + " must have at least one Display Reticle");
                #endif
            }
            else
            {
                // Make sure we can move the reticle when user moves the pointer on the screen
                stickyDisplayModule.lockDisplayReticleToCursor = true;

                stickyDisplayModule.autoHideCursor = false;

                if (!stickyDisplayModule.IsInitialised) { stickyDisplayModule.Initialise(); }

                if (stickyDisplayModule.IsInitialised)
                {
                    stickyDisplayModule.HideCursor();

                    if (showText)
                    {
                        // Add messages in code (could also pre-setup these in the editor)
                        dmInteractiveActive = stickyDisplayModule.AddMessage("Interactive Enabled", interactiveOffText);
                        stickyDisplayModule.SetDisplayMessageOffset(dmInteractiveActive, 0f, -0.59f);
                        stickyDisplayModule.SetDisplayMessageSize(dmInteractiveActive, 0.98f, 0.2f);
                        stickyDisplayModule.SetDisplayMessageScrollDirection(dmInteractiveActive, S3DDisplayMessage.ScrollDirectionNone);
                        stickyDisplayModule.SetDisplayMessageTextFontSize(dmInteractiveActive, true, 1, 50);
                        stickyDisplayModule.SetDisplayMessageTextAlignment(dmInteractiveActive, TextAnchor.MiddleLeft);
                        stickyDisplayModule.ShowDisplayMessage(dmInteractiveActive);

                        dmLookingAt = stickyDisplayModule.CopyDisplayMessage(dmInteractiveActive, "Looking At Object");
                        stickyDisplayModule.AddMessage(dmLookingAt);
                        stickyDisplayModule.SetDisplayMessageOffset(dmLookingAt, 0f, -0.69f);
                        stickyDisplayModule.SetDisplayMessageText(dmLookingAt, lookingAtNothingText);
                        stickyDisplayModule.ShowDisplayMessage(dmLookingAt);

                        dmTarget = stickyDisplayModule.CopyDisplayMessage(dmInteractiveActive, "Target Object");
                        stickyDisplayModule.AddMessage(dmTarget);
                        stickyDisplayModule.SetDisplayMessageOffset(dmTarget, 0f, -0.79f);
                        stickyDisplayModule.SetDisplayMessageText(dmTarget, targetNothingText);
                        stickyDisplayModule.ShowDisplayMessage(dmTarget);

                        dmKeys = stickyDisplayModule.CopyDisplayMessage(dmInteractiveActive, "Keys");
                        stickyDisplayModule.AddMessage(dmKeys);
                        stickyDisplayModule.SetDisplayMessageOffset(dmKeys, 0f, -0.89f);
                        stickyDisplayModule.SetDisplayMessageText(dmKeys, keysText);
                        stickyDisplayModule.ShowDisplayMessage(dmKeys);
                    }

                    // Remmeber the colour that was set in the editor
                    defaultReticleColour = stickyDisplayModule.activeDisplayReticleColour;

                    // Set the look at colour to green
                    lookingAtReticleColour = new Color32(0, 255, 0, defaultReticleColour.a);

                    isConfigured = true;
                }
            }

            return isConfigured;
        }

        #endregion

        #region Public Methods

        public void Initialise()
        {
            if (stickyPlayer == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleHandInteract - did you forget to add a Sticky3D (player) character from the scene?");
                #endif
            }
            else if (!stickyPlayer.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleHandInteract - your character " + stickyPlayer.name + " is not initialised");
                #endif
            }
            if (stickyDisplayModule == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleHandInteract - did you forget to add a StickyDisplayModule from the scene?");
                #endif
            }
            else if (ConfigureDisplayModule())
            {
                // Get notified when the character changes the item they wish to interact with
                if (showText) { stickyPlayer.callbackOnChangeInteractiveTarget = OnInteractiveTargetChanged; }

                isInitialised = true;

                if (autoEnableOnStart) { ToggleInteractive(); }
            }
        }

        /// <summary>
        /// Toggle the ability for the character to grab, select or touch things in the scene
        /// </summary>
        public void ToggleInteractive()
        {
            if (isInitialised)
            {
                // Can we enable interactive?
                if (!isInteractive)
                {
                    isInteractive = stickyDisplayModule.lockDisplayReticleToCursor;

                    if (isInteractive)
                    {
                        // Get notified when the character changes what they are looking at
                        stickyPlayer.callbackOnChangeLookAtInteractive = OnInteractiveLookAtChanged;

                        stickyPlayer.EnableHandIK(true);
                        stickyPlayer.EnableLookInteractive();

                        // If we're using Head IK, enable it
                        if (useHeadIK)
                        {
                            // We are using the look at point in update to set
                            // the head ik target, so make sure we get the data.
                            stickyPlayer.SetUpdateLookingAtPoint(true);

                            // When the character is not moving, get the head to face
                            // the direction the character should be looking.
                            stickyPlayer.SetHeadIKLookAtInteractive(true);

                            //stickyPlayer.ResetHeadIK();
                            stickyPlayer.EnableHeadIK(true);
                        }
                    }
                }
                else
                {
                    isInteractive = false;
                    stickyPlayer.DisableHandIK(true);
                    stickyPlayer.DisableLookInteractive();

                    if (useHeadIK) { stickyPlayer.DisableHeadIK(true); }

                    // Stop getting notified when the character changes what they are looking at
                    stickyPlayer.callbackOnChangeLookAtInteractive = null;
                }

                if (showText)
                {
                    stickyDisplayModule.SetDisplayMessageText(dmInteractiveActive, isInteractive ? interativeOnText : interactiveOffText);
                }
            }
        }

        #endregion

        #region Public Callback Methods

        /// <summary>
        /// This method is automatically called by S3D when the player changes what they are looking at
        /// WARNING: This can impact GC when the on-screen UI text is updated.
        /// </summary>
        /// <param name="stickyControlModule"></param>
        /// <param name="oldLookAtObject"></param>
        /// <param name="newLookAtObject"></param>
        public void OnInteractiveLookAtChanged(StickyControlModule stickyControlModule, StickyInteractive oldLookAtObject, StickyInteractive newLookAtObject)
        {
            lookingAtObject = newLookAtObject;
            lookingAtObjectId = newLookAtObject == null ? StickyInteractive.NoID : newLookAtObject.StickyInteractiveID;

            if (lookingAtObjectId == StickyInteractive.NoID)
            {
                if (showText)
                {
                    stickyDisplayModule.SetDisplayMessageText(dmLookingAt, lookingAtNothingText);
                }
                // Change the aiming reticle colour to indicate when the character is NOT looking at a Sticky Interactive object
                stickyDisplayModule.SetDisplayReticleColour(defaultReticleColour);
            }
            else
            {
                if (showText)
                {
                    stickyDisplayModule.SetDisplayMessageText(dmLookingAt, lookingAtNothingText + " " + lookingAtObject.name);
                }
                // Change the aiming reticle colour to indicate when the character is looking at a Sticky Interactive object
                stickyDisplayModule.SetDisplayReticleColour(lookingAtReticleColour);
            }
        }

        /// <summary>
        /// This method is automatically called by S3D when the player changes the interactive-enabled object they are targeting.
        /// WARNING: This can impact GC when the on-screen UI text is updated.
        /// </summary>
        /// <param name="stickyControlModule"></param>
        /// <param name="oldTarget"></param>
        /// <param name="newTarget"></param>
        public void OnInteractiveTargetChanged(StickyControlModule stickyControlModule, StickyInteractive oldTarget, StickyInteractive newTarget)
        {
            if (showText)
            {
                if (newTarget == null)
                {
                    stickyDisplayModule.SetDisplayMessageText(dmTarget, targetNothingText);
                }
                else
                {
                    stickyDisplayModule.SetDisplayMessageText(dmTarget, targetNothingText + " " + newTarget.name);
                }
            }
        }

        #endregion
    }
}