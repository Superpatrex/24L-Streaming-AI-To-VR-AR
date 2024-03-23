using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core3lb
{

    [Serializable]
    public class InputReferencesXR
    {
        public InputActionReference leftHand;
        public InputActionReference rightHand;
        //Add more Controller Types such as hands go here!
    }

    //Haptics is handled by the Player rig as it is an a PER Basis
    //Hand Tracking will be handed by the player rig as it is a PER Basis


    //PlayerRigBase.PlayerInput().GetInputBool(PlayerRigBase.PlayerInput().handTrigger, Input3lb.Controller.Left,Input3lb.InputRequest.Get);

        public class InputXR : MonoBehaviour
        {
            [CoreEmphasize]
            public MonoBehaviour inputOverride;
            public IOverrideInput overrideInputoverrideInput;
            //These are all PUBLIC so they can be subscribed to
            public static InputXR instance;
            [CoreHeader("XR Defined Buttons - All Required")]
            public InputReferencesXR grab;
            public InputReferencesXR move;
            public InputReferencesXR interact;
            public InputReferencesXR altInteract;
            public InputReferencesXR menu;


        public void Awake()
            {
                if(instance != null)
                {
                    CoreDebug.LogError("Two InputXR's detected deleting this one", gameObject);
                    Destroy(this);
                }
                instance = this;
                if(inputOverride)
                {
                    if (inputOverride.TryGetComponent(out IOverrideInput myOverride))
                    {
                        overrideInputoverrideInput = myOverride;
                    }
                    else
                    {
                        CoreDebug.LogError("Slotted Override Input is not an IOverrideInput");
                    }
                }              
            }

            public enum Controller
            {
                Left,
                Right,
                //More can be added here as they start left foot right foot so on
            }

            public enum Button
            {
                Grab,
                Interact,
                AltInteract,
                Menu,
                Move,
            }

            public enum InputRequest
            {
                GetDown,
                GetUp,
                Get,
            }

            ///ACTION STUFF
            ///
            [Space]
            [SerializeField]
            [Tooltip("Input action assets to affect when inputs are enabled or disabled.")]
            List<InputActionAsset> m_ActionAssets;

            public virtual bool GetBool(InputReferencesXR WhatRef, Controller which, InputRequest whatRequest)
            {
                return HandleReference(WhatRef, which, whatRequest);
            }     

            //Default Predefined Buttons - These are the most common Interact/Grab/Move/Menu
            public virtual bool GetInteract(Controller which)
            {
                if (overrideInputoverrideInput != null && overrideInputoverrideInput.IenableOverride)
                {
                    return overrideInputoverrideInput.GetInteract(which);
                }
            return HandleReference(interact, which, InputRequest.Get);
            }

            public virtual bool GetGrab(Controller which)
            {
                if (overrideInputoverrideInput != null && overrideInputoverrideInput.IenableOverride)
                {
                    return overrideInputoverrideInput.GetGrab(which);
                }
                return HandleReference(grab, which, InputRequest.Get);
            }

            public virtual bool GetAltInteract(Controller which)
            {
                if (overrideInputoverrideInput != null && overrideInputoverrideInput.IenableOverride)
                {
                    return overrideInputoverrideInput.GetAltInteract(which);
                }
                return HandleReference(altInteract, which, InputRequest.Get);
            }

            public virtual bool GetMove(Controller which)
            {
                if (overrideInputoverrideInput != null && overrideInputoverrideInput.IenableOverride)
                {
                    return overrideInputoverrideInput.GetGrab(which);
                }
                return HandleReference(move, which, InputRequest.Get);
            }

            public virtual bool GetMenu(Controller which)
            {
                if (overrideInputoverrideInput != null && overrideInputoverrideInput.IenableOverride)
                {
                    return overrideInputoverrideInput.GetGrab(which);
                }
                return HandleReference(menu, which, InputRequest.Get);
            }

            public virtual bool ButtonRequest(Button button, Controller which, InputRequest inputRequest)
            {
                switch (button)
                {
                    case Button.Grab:
                        return GetBool(grab, which, inputRequest);
                    case Button.Interact:
                        return GetBool(interact, which, inputRequest);
                    case Button.Menu:
                        return GetBool(menu, which, inputRequest);
                    case Button.Move:
                        return GetBool(move, which, inputRequest);
                    case Button.AltInteract:
                        return GetBool(altInteract, which, inputRequest);
                    default:
                        return false;
                }
            }

        public virtual bool GetButton(Button button, Controller which)
        {
            switch (button)
            {
                case Button.Grab:
                    return GetGrab(which);
                case Button.Interact:
                    return GetInteract(which);
                case Button.Menu:
                    return GetMenu(which);
                case Button.Move:
                    return GetMove(which);
                case Button.AltInteract:
                    return GetInteract(which);
                default:
                    return false;
            }
        }

        public virtual Vector2 GetAxis(InputReferencesXR WhatRef, Controller which)
            {
                return HandleAxis(WhatRef, which);
            }

            protected virtual Vector2 HandleAxis(InputReferencesXR WhatRef, Controller which)
            {
                Vector2 axis = Vector2.zero;
                InputActionReference holdRef = null;
                switch (which)
                {
                    case Controller.Right:
                        holdRef = WhatRef.rightHand;
                        break;
                    case Controller.Left:
                        holdRef = WhatRef.leftHand;
                        break;
                    default:
                        break;
                }
                return holdRef.action.ReadValue<Vector2>();
            }

            //Internal Handling of buttons if needed for developer
            public static bool HandleReference(InputReferencesXR WhatRef, Controller which, InputRequest whatRequest)
            {
                InputActionReference holdRef = null;
                switch (which)
                {
                    case Controller.Right:
                        holdRef = WhatRef.rightHand;
                        break;
                    case Controller.Left:
                        holdRef = WhatRef.leftHand;
                        break;
                    default:
                        break;
                }
                return HandleInputActions(holdRef, whatRequest);
            }

            //This is here to simplify if you have an action reference
            public static bool HandleInputActions(InputActionReference reference, InputRequest whatRequest)
            {
                switch (whatRequest)
                {
                    case InputRequest.GetDown:
                        return reference.action.WasPerformedThisFrame();
                    case InputRequest.GetUp:
                        return reference.action.WasReleasedThisFrame();
                    case InputRequest.Get:
                        return reference.action.IsPressed();
                    default:
                        break;
                }
                return false;
            }

            public static Vector2 GetVector2FromAction(InputActionReference actionReference)
            {
                return actionReference.action.ReadValue<Vector2>();
            }

            public static float GetFloatFromAction(InputActionReference actionReference)
            {
                return actionReference.action.ReadValue<float>();
            }

        /// <summary>
        /// Input action assets to affect when inputs are enabled or disabled.
        /// </summary>
        public List<InputActionAsset> actionAssets
            {
                get => m_ActionAssets;
                set => m_ActionAssets = value ?? throw new ArgumentNullException(nameof(value));
            }

            /// <summary>
            /// See <see cref="MonoBehaviour"/>.
            /// </summary>
            protected void OnEnable()
            {
                EnableInput();
            }

            /// <summary>
            /// See <see cref="MonoBehaviour"/>.
            /// </summary>
            protected void OnDisable()
            {
                DisableInput();
            }

            /// <summary>
            /// Enable all actions referenced by this component.
            /// </summary>
            /// <remarks>
            /// Unity will automatically call this function when this <see cref="InputActionManager"/> component is enabled.
            /// However, this method can be called to enable input manually, such as after disabling it with <see cref="DisableInput"/>.
            /// <br />
            /// Enabling inputs only enables the action maps contained within the referenced
            /// action map assets (see <see cref="actionAssets"/>).
            /// </remarks>
            /// <seealso cref="DisableInput"/>
            public void EnableInput()
            {
                if (m_ActionAssets == null)
                    return;

                foreach (var actionAsset in m_ActionAssets)
                {
                    if (actionAsset != null)
                    {
                        actionAsset.Enable();
                        foreach (var action in actionAssets)
                        {
                            action.Enable();
                        }

                    }
                }
            }

            /// <summary>
            /// Disable all actions referenced by this component.
            /// </summary>
            /// <remarks>
            /// This function will automatically be called when this <see cref="InputActionManager"/> component is disabled.
            /// However, this method can be called to disable input manually, such as after enabling it with <see cref="EnableInput"/>.
            /// <br />
            /// Disabling inputs only disables the action maps contained within the referenced
            /// action map assets (see <see cref="actionAssets"/>).
            /// </remarks>
            /// <seealso cref="EnableInput"/>
            public void DisableInput()
            {
                if (m_ActionAssets == null)
                    return;

                foreach (var actionAsset in m_ActionAssets)
                {
                    if (actionAsset != null)
                    {
                        actionAsset.Disable();
                    }
                }
            }
    }
}