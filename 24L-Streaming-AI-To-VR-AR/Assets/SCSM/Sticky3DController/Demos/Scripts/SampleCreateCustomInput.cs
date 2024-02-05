using UnityEngine;
using scsmmedia;

// Sticky3D Controller Copyright (c) 2018 - 2022 SCSM Pty Ltd. All rights reserved.
namespace MyUniqueGame
{
    /// <summary>
    /// Sample script to show how to add custom inputs at runtime in your code.
    /// This sample assumes the (old) Input Manager is enabled and there is a mouse
    /// attached to the computer. However, this method could easily be adapted to any
    /// supported input mode.
    /// SETUP:
    /// 1. Add a demo Sticky3D character player prefab to your scene.
    /// 2. Attach this script to your S3D player.
    /// 3. Run the scene and click the left mouse button
    /// 4. Check the Unity console for an output message.
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Create Custom Input")]
    [DisallowMultipleComponent]
    public class SampleCreateCustomInput : MonoBehaviour
    {
        #region Public Variables
        [Tooltip("If enabled, the Initialise() will be called as soon as Start() runs. This should be disabled if you are instantiating the component through code.")]
        public bool initialiseOnStart = true;
        #endregion

        #region Private Variables
        private StickyControlModule thisCharacter = null;
        private StickyInputModule stickyInputModule = null;
        private bool isInitialised = false;
        private CustomInput customInput = null;
        #endregion

        #region Initialise Methods
        private void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }
        #endregion

        #region Public Methods
        public void Initialise()
        {
            // Run only once
            if (isInitialised) { return; }

            thisCharacter = GetComponent<StickyControlModule>();
            stickyInputModule = GetComponent<StickyInputModule>();
            customInput = new CustomInput();

            // For the sake of the sample, do a bunch of error checking. In game code, you would probably
            // do all this checking on one line.
            if (thisCharacter == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleCreateCustomInput - could not find StickyControlModule on " + name + ". Did you forget to attach this script to one of one of our player S3D characters?");
                #endif
            }
            else if (!thisCharacter.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleCreateCustomInput - your character " + thisCharacter.name + " is not initialised");
                #endif
            }
            else if (stickyInputModule == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleCreateCustomInput - could not find StickyInputModule on " + name + ". Did you forget to attach this script to one of our player S3D characters?");
                #endif
            }
            else if (customInput != null)
            {
                // Create the new event
                customInput.customInputEvt = new CustomInputEvt();
                
                if (customInput.customInputEvt != null)
                {
                    // You could call a method in this class with: delegate { MyMethod(myParameters); }
                    // You could call a static member of another class in your game with: delegate { MyOtherClass.MyMethod(myParameters); }
                    // You could call a member of another class instance in your game with: delegate { myOtherClassInstance.MyMethod(myParameters); }
                    // Here we call a static member in another class.
                    customInput.customInputEvt.AddListener(delegate { SampleCreateCustomInputGameClass.PerformAction(thisCharacter); });
                }

                customInput.canBeHeldDown = false;
                customInput.isButton = true;
                customInput.isButtonEnabled = true;

                if (stickyInputModule.inputMode == StickyInputModule.InputMode.DirectKeyboard)
                {
                    // Assume we're using the legacy input system for keyboard and mouse input
                    #if ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_2_OR_NEWER

                    // Configure left mouse click
                    customInput.dkmPositiveKeycode = KeyCode.Mouse0;

                    #endif
                }

                // Add the custom input to the list
                stickyInputModule.customInputList.Add(customInput);

                // Reinitialise custom input so the custom input(s) we have added take effect
                stickyInputModule.ReinitialiseCustomInput();

                isInitialised = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// Example of some other game play code
    /// </summary>
    public class SampleCreateCustomInputGameClass
    {
        /// <summary>
        /// This is called automatically from the S3D when the user click the appropriate button on a keyboard or gamepad etc.
        /// For convenience here, we've added it as a static class, but that is not a requirement.
        /// </summary>
        /// <param name="stickyControlModule"></param>
        public static void PerformAction(StickyControlModule stickyControlModule)
        {
            Debug.Log("Take some action at Time: " + Time.time);   
        }
    }
}