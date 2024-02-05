using UnityEngine;

// Sticky3D Controller Copyright (c) 2019-2021 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This simple sample script shows how to receive notification when a custom
    /// input action is performed.
    /// Setup:
    /// 1. Add this script to an empty gameobject in your scene.
    /// 2. Name the new gameobject. e.g. ReceivePlayerInput
    /// 3. Add a character with a StickyControlModule and StickyInputModule component.
    /// 4. In the StickyInputModule editor, add a custom input.
    /// 5. Drag ReceivePlayerInput gameobject into the Callback method object
    /// 6. Change the "No Function" to "SampleCustomInput" (Dynamic Vector3, Int) "DoActionOne".
    /// 7. Set the player input for your selected Input Mode.
    /// Running:
    /// 1. Run the scene
    /// 2. Perform the action that you set up #7 above. E.g. press a key on the keyboard
    /// 3. Check the Unity console for the appropriate message
    /// This is only sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own namespace.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Receive Custom Input")]
    public class SampleCustomInput : MonoBehaviour
    {
        #region Public Methods

        public void DoActionOne(Vector3 inputValue, int customInputEventType)
        {
            Debug.Log("Do action one with a input value of " + inputValue.x + " eventType: " + (CustomInput.CustomInputEventType)customInputEventType + " at time: " + Time.time);
        }


        public void DoActionTwo(Vector3 inputValue, int customPlayerInputEventType)
        {
            Debug.Log("Do action two with a input value of " + inputValue + " eventType: " + (CustomInput.CustomInputEventType)customPlayerInputEventType + " at time: " + Time.time);
        }

        #endregion
    }
}