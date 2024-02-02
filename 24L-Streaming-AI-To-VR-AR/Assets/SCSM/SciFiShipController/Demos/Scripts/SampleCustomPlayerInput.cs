using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This simple sample script shows how to receive notification when a custom
    /// player input action is performed.
    /// Setup:
    /// 1. Add this script to an empty gameobject in your scene.
    /// 2. Name the new gameobject. e.g. ReceivePlayerInput
    /// 3. Add a ship with a ShipControlModule and PlayerInputModule component.
    /// 4. In the PlayerInputModule editor, add a custom player input.
    /// 5. Drag ReceivePlayerInput gameobject into the Callback method object
    /// 6. Change the "No Function" to "SampleCustomPlayerInput" (Dynamic Vector3, Int) "DoActionOne".
    /// 7. Set the player input for your selected Input Mode.
    /// Running:
    /// 1. Run the scene
    /// 2. Perform the action that you set up #7 above. E.g. press a key on the keyboard
    /// 3. Check the Unity console for the appropriate message
    /// This is only sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own namespace.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Custom Player Input")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleCustomPlayerInput : MonoBehaviour
    {
        #region Public Methods

        public void DoActionOne(Vector3 inputValue, int customPlayerInputEventType)
        {
            Debug.Log("Do action one with a input value of " + inputValue.x + " eventType: " + (CustomPlayerInput.CustomPlayerInputEventType)customPlayerInputEventType + " at time: " + Time.time);
        }


        public void DoActionTwo(Vector3 inputValue, int customPlayerInputEventType)
        {
            Debug.Log("Do action two with a input value of " + inputValue + " eventType: " + (CustomPlayerInput.CustomPlayerInputEventType)customPlayerInputEventType + " at time: " + Time.time);
        }

        #endregion
    }
}
