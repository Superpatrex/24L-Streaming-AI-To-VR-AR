using UnityEngine;
using UnityEngine.InputSystem;

namespace Core3lb
{
    public class DEBUG_InputActions : MonoBehaviour
    {
       public InputActionReference myAction;
       public bool forceEnableAction;
        public bool forceEnableActionSet;
        public InputActionAsset myAsset;

        public void Update()
        {
            if(forceEnableActionSet)
            {
                myAsset.Enable();
                //
            }
            if(forceEnableAction)
            {
                myAction.action.Enable();
            }
            Debug.LogError(myAction.name + " is Enabled? " + myAction.action.enabled);
            Debug.LogError(myAction.name + " isPressed " + myAction.action.IsPressed());
        }
    }
}
