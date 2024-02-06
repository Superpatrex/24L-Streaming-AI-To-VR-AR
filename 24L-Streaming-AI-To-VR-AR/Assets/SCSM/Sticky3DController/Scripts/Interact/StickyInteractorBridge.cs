using UnityEngine;
#if SCSM_XR && SSC_UIS
using UnityEngine.XR;
using System.Collections.Generic;
#endif

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This component, currently in Technical Preview, enables a non-Sticky3D object
    /// or hand to interact with Sticky Interactive objects.
    /// For example, it could be used with Sci-Fi Ship Controller XR hands to touch
    /// or grab a Sticky Interactive-enabled object.
    /// Features will be added to this component as new scenarios come to light.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Utilities/Sticky Interactor Bridge")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyInteractorBridge : MonoBehaviour
    {
        #region Enumerations

        #endregion

        #region Public Static Variables

        #endregion

        #region Public Variables - General
        /// <summary>
        /// If enabled, the Initialise() will be called as soon as Start() runs. This should be disabled if you are
        /// instantiating the component through code.
        /// </summary>
        public bool initialiseOnStart = false;

        #endregion

        #region Public Variables - Grab

        /// <summary>
        /// If Can Grab, is true, and the hand is currently grabbing an object,
        /// if it moves outside the collider's range, it will automatically be dropped.
        /// This really only makes scene for interactive-enabled objects that are not
        /// parented on Grab.
        /// </summary>
        public bool isAutoDrop = true;

        /// <summary>
        /// If Can Grab, is true, will the component attempt to automatically
        /// grab an interactive-enabled Grabbable object when it is in range?
        /// </summary>
        public bool isAutoGrab = true;
        #endregion
        
        #region Public Variables - Animate Hand VR

        /// <summary>
        /// The animator used to animate a Virtual Reality hand
        /// </summary>
        public Animator handAnimator = null;
        #endregion

        #region Public Properties - General

        /// <summary>
        /// [READONLY]
        /// Get the current grip value from the VR hand controller
        /// </summary>
        public float HandGripValue { get { return isInitialised ? (isLeftHand ? currentCharInputXR.leftHandGrip : currentCharInputXR.rightHandGrip) : 0f; } }

        /// <summary>
        /// [READONLY]
        /// Get the current trigger value from the VR hand controller
        /// </summary>
        public float HandTriggerValue { get { return isInitialised ? (isLeftHand ? currentCharInputXR.leftHandTrigger : currentCharInputXR.rightHandTrigger) : 0f; } }

        /// <summary>
        /// [READONLY]
        /// Has the module been initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// [READONLY]
        /// Can this item touch an interactive-enabled Touchable object?
        /// </summary>
        public bool IsCanTouch { get { return isCanTouch; } }

        /// <summary>
        /// [READONLY]
        /// Can this item grab an interactive-enabled Grabbable object?
        /// </summary>
        public bool IsCanGrab { get { return isCanGrab; } }

        /// <summary>
        /// [READONLY]
        /// Is the XR hand input device valid? If this component is not initialised,
        /// or UnityXR is not configured, or the hand controller device is not
        /// connected, this property will return false.
        /// </summary>
        public bool IsHandInputDeviceXRValid
        {
            get
            {
                if (!isInitialised) { return false; }
                #if SCSM_XR && SSC_UIS
                return handInputDeviceXR.isValid;
                #else
                return false;
                #endif
            }
        }

        /// <summary>
        /// [READONLY]
        /// Is hand animate enabled? This is only true when a runtimeAnimatorController
        /// is available at runtime for the Animator component.
        /// </summary>
        public bool IsHandVRAnimateEnabled { get { return isHandVRAnimateEnabled; } }

        /// <summary>
        /// [READONLY]
        /// Is this component currently holding an interactive-enabled Grabbable object?
        /// </summary>
        public bool IsHeld { get { return isInitialised && heldInteractiveId != StickyInteractive.NoID; } }

        /// <summary>
        /// [READONLY]
        /// Is this component currently touching an interactive-enabled Touchable object?
        /// </summary>
        public bool IsTouching { get { return isInitialised && isTouching && touchingInteractiveId != StickyInteractive.NoID; } }

        #endregion

        #region Public Variables and Properties - Internal Only

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [HideInInspector] public bool allowRepaint = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool showGeneralSettingsInEditor = true;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool showInteractiveSettingsInEditor = true;

        #endregion

        #region Private Variables - General
        protected bool isInitialised = false;

        #endregion

        #region Protected Variables - Activate

        /// <summary>
        /// Can this item activate an interactive-enabled Activable object?
        /// </summary>
        [SerializeField] protected bool isCanActivate = false;
        #endregion

        #region Protected Variables - Grab

        /// <summary>
        /// Can this item (automatically) grab an interactive-enabled Grabbable object?
        /// </summary>
        [SerializeField] protected bool isCanGrab = false;

        /// <summary>
        /// The z-axis (forward) should point in the direction of the palm is facing.
        /// This is the palm “normal”. The x-axis (right) is towards the direction of the
        /// index finger and, for the right hand, the y-axis (up) is in a thumbs-up direction.
        /// </summary>
        [SerializeField] protected Transform palmTransform = null;

        /// <summary>
        /// [INTERNAL ONLY] The item being held by this component
        /// </summary>
        [System.NonSerialized] protected StickyInteractive heldInteractive = null;
        /// <summary>
        /// [INTERNAL ONLY] The ID of the object being held by this component
        /// </summary>
        protected int heldInteractiveId = 0;

        /// <summary>
        /// [INTERNAL ONLY] The ID of interactive object in contact with that hasn't been grabbed
        /// </summary>
        [System.NonSerialized] protected StickyInteractive contactInteractive = null;

        /// <summary>
        /// [INTERNAL ONLY] The ID of interactive object in contact with that hasn't been grabbed
        /// </summary>
        protected int contactInteractiveId = 0;

        /// <summary>
        /// [INTERNAL ONLY] Is the interactive object being held at
        /// the secondary hand hold position with this component?
        /// </summary>
        //private bool isHeldSecondaryHandHold = false;

        /// <summary>
        /// This is the transform created at runtime as a child of the palmTransform,
        /// that a grabbable interactive-enabled object uses to follow the hand.
        /// It is used when the isParentOnGrab is NOT enabled on the object.
        /// </summary>
        [System.NonSerialized] protected Transform heldTargetTfrm = null;

        #endregion

        #region Protected Variables - Animate Hand VR

        [SerializeField] protected bool isLeftHand = false;
        [SerializeField] protected S3DAnimAction handVRGrip = null;
        [SerializeField] protected S3DAnimAction handVRTrigger = null;

        // Editor only
        [SerializeField] protected bool isHandVRExpanded = true;

        protected CharacterInputXR currentCharInputXR = null;
        protected CharacterInputXR previousCharInputXR = null;

        #if SCSM_XR && SSC_UIS
        // Quest2 HeldInHand, TrackedDevice, Controller, Left
        protected static InputDeviceCharacteristics leftHandCharacteristicsXR = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller;
        protected static InputDeviceCharacteristics rightHandCharacteristicsXR = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller;
        
        // Device-based input - will at some point convert to UnityEngine.InputSystem.InputDevice handInputDeviceUIS
        [System.NonSerialized] protected UnityEngine.XR.InputDevice handInputDeviceXR;
        
        #endif

        /// <summary>
        /// [INTERNAL ONLY] Is Hand animate enabled at runtime?
        /// </summary>
        protected bool isHandVRAnimateEnabled = false;

        #endregion

        #region Protected Variables - Touch

        /// <summary>
        /// Can this item touch an interactive-enabled Touchable object?
        /// </summary>
        [SerializeField] protected bool isCanTouch = false;

        [System.NonSerialized] protected Collider touchCollider;
        [System.NonSerialized] protected StickyInteractive stickyInteractive = null;
        [System.NonSerialized] protected StickyInteractiveChild stickyInteractiveChild = null;

        [System.NonSerialized] protected StickyInteractive touchingInteractive = null;
        protected int touchingInteractiveId = 0;
        protected bool isTouching = false;

        #endregion

        #region Initialise Methods
        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Protected or Internal Methods

        /// <summary>
        /// Attempt to activate (or deactivate) an interactive-enabled object
        /// </summary>
        /// <param name="stickyInteractive"></param>
        protected virtual void ActivateInteractive(StickyInteractive stickyInteractive, bool isActivate)
        {
            if (isInitialised && stickyInteractive != null && stickyInteractive.IsActivable)
            {
                if (isActivate)
                {
                    if (isCanActivate) { stickyInteractive.ActivateObject(0); }
                }
                else
                {
                    stickyInteractive.DeactivateObject(0);
                }
            }
        }

        /// <summary>
        /// Check if there is an item being held by this component.
        /// If the item being held was destroyed, return false.
        /// </summary>
        /// <returns>true if held, otherwise false</returns>
        protected virtual bool CheckHeldInteractive()
        {
            if (!isInitialised) { return false; }
            else if (heldInteractiveId != StickyInteractive.NoID)
            {
                // Check that it hasn't been destroyed
                if (heldInteractive == null)
                {
                    if (heldTargetTfrm != null) { Destroy(heldTargetTfrm.gameObject); }

                    // Cannot find it so assume this component is no longer holding it
                    heldInteractiveId = StickyInteractive.NoID;
                    //isHeldSecondaryHandHold = false;

                    return false;
                }
                else
                {
                    return true;
                }
            }
            else { return false; }
        }

        /// <summary>
        /// Check if an interactive-enabled object is being touched by this component.
        /// If the item being touched was destroyed, return false.
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckTouchInteractive()
        {
            if (!isInitialised) { return false; }
            else if (touchingInteractiveId != StickyInteractive.NoID)
            {
                // Check that it hasn't been destroyed
                if (touchingInteractive == null)
                {
                    isTouching = false;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else { return false; }
        }

        /// <summary>
        /// Calculate the desired position and rotation of the object to be held.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        /// <param name="isSecondaryHandHold"></param>
        /// <param name="wsPosition"></param>
        /// <param name="wsRotation"></param>
        protected virtual void GetHoldPositionAndRotation (StickyInteractive stickyInteractive, bool isSecondaryHandHold, out Vector3 wsPosition, out Quaternion wsRotation)
        {
            // Get the palm of the hand's relative position and rotation to the hand bone.
            wsPosition = GetHandPalmPosition();
            wsRotation = GetHandPalmRotation();

            Vector3 handHoldLocalRotEuler = isSecondaryHandHold ? stickyInteractive.handHold2Rotation : stickyInteractive.handHold1Rotation;

            // We want the object's hand hold rotation to point towards the palm.
            Quaternion handHoldLocalRot = Quaternion.Euler(handHoldLocalRotEuler);

            wsRotation *= Quaternion.Inverse(handHoldLocalRot);

            // Subtract the hand hold offset
            wsPosition -= wsRotation * stickyInteractive.GetHandHoldLocalOffset(isSecondaryHandHold);
        }

        /// <summary>
        /// Attempt to parent an interactive-enabled object to this component.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        /// <param name="handTrfm"></param>
        /// <param name="isSecondaryHandHold"></param>
        /// <returns></returns>
        protected virtual bool ParentIteractive(StickyInteractive stickyInteractive, Transform handTrfm, bool isSecondaryHandHold)
        {
            bool isParented = false;

            if (handTrfm != null)
            {
                // Set the world space position and rotation before parenting to avoid issues with character rig scaling.
                Vector3 wsPosition;
                Quaternion wsRotation;

                GetHoldPositionAndRotation(stickyInteractive, isSecondaryHandHold, out wsPosition, out wsRotation);

                stickyInteractive.transform.SetPositionAndRotation(wsPosition, wsRotation);

                // Use instead of transform.SetParent() as we may need to remember the original
                // transform for when the object is dropped.
                stickyInteractive.SetObjectParent(handTrfm);

                isParented = true;
            }

            return isParented;
        }

        /// <summary>
        /// Animate the XR hand models for the character based on input from the hand controller
        /// </summary>
        protected void XRAnimateHand()
        {
            if (isHandVRAnimateEnabled)
            {
                if (handVRGrip != null && handVRGrip.paramHashCode != 0)
                {
                    handAnimator.SetFloat(handVRGrip.paramHashCode, isLeftHand ? currentCharInputXR.leftHandGrip : currentCharInputXR.rightHandGrip);
                }

                if (handVRTrigger != null && handVRTrigger.paramHashCode != 0)
                {
                    handAnimator.SetFloat(handVRTrigger.paramHashCode, isLeftHand ? currentCharInputXR.leftHandTrigger : currentCharInputXR.rightHandTrigger);
                }
            }
        }

        #endregion

        #region Protected or Internal Methods - XR

        /// <summary>
        /// Try to enable or disable hand VR animate at runtime
        /// </summary>
        /// <param name="isEnabled"></param>
        protected void EnableOrDisableHandVRAnimate(bool isEnabled)
        {
            isHandVRAnimateEnabled = isEnabled;

            if (isEnabled)
            {
                if (handAnimator == null)
                {
                    isHandVRAnimateEnabled = false;

                    #if UNITY_EDITOR
                    Debug.LogWarning("StickyInteractorBridge could not enable animate because " + (isLeftHand ? "left" : "right") + " Animator is not set under Hand Animate Settings");
                    #endif
                }
                else if (handAnimator.runtimeAnimatorController == null)
                {
                    isHandVRAnimateEnabled = false;

                    #if UNITY_EDITOR
                    Debug.LogWarning("StickyInteractorBridge could not enable animate because there is no controller set on the Animator component of " + handAnimator.name);
                    #endif
                }
            }
        }

        #if SCSM_XR && SSC_UIS

        /// <summary>
        /// Check if the player is attempting to release (drop) a grabbable object when held
        /// and isAutoDrop is false by releasing the grip button.
        /// </summary>
        protected void XRCheckDrop()
        {
            // Is there an object held and isAutoDrop is not enabled?
            if (isCanGrab && !isAutoDrop && heldInteractiveId != StickyInteractive.NoID)
            {
                if ((isLeftHand && currentCharInputXR.leftHandGrip < 0.2f) || (!isLeftHand && currentCharInputXR.rightHandGrip < 0.2f))
                {
                    DropInteractive();
                }
            }
        }

        /// <summary>
        /// Check if the user is attempting to grab an interactive object that is not auto-grab.
        /// Contact is determined by OnTriggerEnter and OnTriggerExit.
        /// </summary>
        protected void XRCheckGrab()
        {
            // Is this potentially grabble when user presses the grip button on the controller?
            if (isCanGrab && !isAutoGrab && contactInteractiveId != StickyInteractive.NoID)
            {
                // Is an object already being held or was the contact interactive object destroyed
                if (heldInteractiveId != StickyInteractive.NoID || contactInteractive == null)
                {
                    contactInteractiveId = StickyInteractive.NoID;
                    contactInteractive = null;
                }
                // Is the hand in contact with the object and the player is pressing the Grip?
                else if ((isLeftHand && currentCharInputXR.leftHandGrip > 0.7f) || (!isLeftHand && currentCharInputXR.rightHandGrip > 0.7f))
                {
                    //Debug.Log("[DEBUG] attempt to grab with left + " + isLeftHand + " T:" + Time.time);

                    GrabInteractive(contactInteractive, false);
                }
            }
        }

        /// <summary>
        /// XR Devices may not get connected in the first few frames when the scene loads,
        /// so get notified when XR devices get connected.
        /// </summary>
        /// <param name="inputDevice"></param>
        protected void XRDeviceConnected(UnityEngine.XR.InputDevice inputDevice)
        {
            if (isLeftHand && (inputDevice.characteristics & leftHandCharacteristicsXR) == leftHandCharacteristicsXR)
            {
                handInputDeviceXR = inputDevice;
                //Debug.Log("[DEBUG] Left Device connected! name: " + inputDevice.name + " T:" + Time.time);
            }
            else if (!isLeftHand && (inputDevice.characteristics & rightHandCharacteristicsXR) == rightHandCharacteristicsXR)
            {
                handInputDeviceXR = inputDevice;
                //Debug.Log("[DEBUG] Right Device connected! name: " + inputDevice.name + " T:" + Time.time);
            }
        }

        /// <summary>
        /// Attempt to get the (already connected) hand controller
        /// </summary>
        protected void XRGetDevice()
        {
            List<InputDevice> xrInputDevices = new List<InputDevice>(1);

            if (isLeftHand)
            {
                InputDevices.GetDevicesWithCharacteristics(leftHandCharacteristicsXR, xrInputDevices);
            }
            else
            {
                InputDevices.GetDevicesWithCharacteristics(rightHandCharacteristicsXR, xrInputDevices);
            }

            if (xrInputDevices.Count > 0) { handInputDeviceXR = xrInputDevices[0]; }
        }

        #endif

        #endregion

        #region Event Methods

        protected void OnEnable()
        {
            #if SCSM_XR && SSC_UIS

            // XR Devices may not get connected in the first few frames when the scene loads,
            // so get notified when XR devices get connected.
            InputDevices.deviceConnected -= XRDeviceConnected;
            InputDevices.deviceConnected += XRDeviceConnected;

            XRGetDevice();
            #endif
        }

        protected void OnDisable()
        {
            #if SCSM_XR && SSC_UIS
            InputDevices.deviceConnected -= XRDeviceConnected;
            #endif
        }

        protected void OnDestroy()
        {
            #if SCSM_XR && SSC_UIS
            InputDevices.deviceConnected -= XRDeviceConnected;
            #endif
        }

        protected void OnTriggerEnter (Collider other)
        {
            if (!isInitialised) { return; }
            else
            {
                if (!other.TryGetComponent(out stickyInteractive))
                {
                    if (other.TryGetComponent(out stickyInteractiveChild))
                    {
                        stickyInteractive = stickyInteractiveChild.stickyInteractive;
                    }
                }

                // Have we entered the range of (any) interactive object?
                bool isStickyInteractive = stickyInteractive != null;

                if (isStickyInteractive)
                {
                    // If we are already touching an object, do nothing
                    if (isCanTouch && stickyInteractive.IsTouchable && !CheckTouchInteractive() && !CheckHeldInteractive())
                    {
                        Vector3 touchColliderPos = touchCollider.bounds.center;

                        Vector3 hitPoint = other.ClosestPointOnBounds(touchColliderPos);
                        Vector3 hitNormal = (touchColliderPos - hitPoint).normalized;

                        TouchInteractive(stickyInteractive, hitPoint, hitNormal);
                    }

                    if (isCanGrab && stickyInteractive.IsGrabbable && !CheckHeldInteractive())
                    {
                        // Grab the object, if we are not already holding an object and Auto Grab is on.
                        if (isAutoGrab) { GrabInteractive(stickyInteractive, false); }
                        else
                        {
                            // Otherwise, register it as being in contact and potentially grabbable
                            contactInteractiveId = stickyInteractive.StickyInteractiveID;
                            contactInteractive = stickyInteractive;

                            //Debug.Log("[DEBUG] StickyInteractorBridge OnTriggerEnter " + name + " is in contact with " + contactInteractive.name + " contactid: " + contactInteractiveId + " T:" + Time.time);
                        }
                    }

                    //Debug.Log("[DEBUG] StickyInteractorBridge OnTriggerEnter " + name + " is touching " + other.name + " T:" + Time.time);
                }
            }
        }

        protected void OnTriggerExit (Collider other)
        {
            if (!isInitialised) { return; }
            else
            {
                if (!other.TryGetComponent(out stickyInteractive))
                {
                    if (other.TryGetComponent(out stickyInteractiveChild))
                    {
                        stickyInteractive = stickyInteractiveChild.stickyInteractive;
                    }
                }

                // Have we exited the range of (any) interactive object?
                bool isStickyInteractive = stickyInteractive != null;

                if (isStickyInteractive && stickyInteractive.StickyInteractiveID == touchingInteractiveId)
                {
                    StopTouchingInteractive(stickyInteractive);
                }

                if (isAutoDrop && CheckHeldInteractive())
                {
                    DropInteractive();
                }

                // Assume no longer in contact with the interative object.
                // Essentially we assume there are no overlapping interactive objects.
                if (isStickyInteractive && stickyInteractive.StickyInteractiveID == contactInteractiveId)
                {
                    contactInteractiveId = StickyInteractive.NoID;
                    contactInteractive = null;
                }

                //Debug.Log("[DEBUG] StickyInteractorBridge OnTriggerExit " + name + " other: " + other.name + " T:" + Time.time);
            }
        }

        #endregion

        #region Update Methods

        #if SCSM_XR && SSC_UIS
        // Update is called once per frame
        void Update()
        {
            if (isInitialised)
            {
                if (isHandVRAnimateEnabled)
                {                    
                    // Thumb trigger on Oculus Quest 2
                    if (handInputDeviceXR.isValid && handInputDeviceXR.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out float gripValue))
                    {
                        if (isLeftHand) { currentCharInputXR.leftHandGrip = gripValue; }
                        else { currentCharInputXR.rightHandGrip = gripValue; }
                    }

                    // Index finger trigger on Oculus Quest 2
                    if (handInputDeviceXR.isValid && handInputDeviceXR.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue))
                    {
                        if (isLeftHand) { currentCharInputXR.leftHandTrigger = triggerValue; }
                        else { currentCharInputXR.rightHandTrigger = triggerValue; }
                    }

                    XRAnimateHand();

                    XRCheckGrab();
                    XRCheckDrop();
                }
            }
        }
        #endif

        #endregion

        #region Public Virtual API Methods

        /// <summary>
        /// Drop a held interactive-enabled object
        /// </summary>
        public virtual void DropInteractive()
        {
            if (CheckHeldInteractive())
            {
                heldInteractive.SetExternalThrowStrength(1f);

                // Perform any actions required when the interactive-enabled object is dropped.
                // This includes performing actions configured in the StickyInteractive editor.
                heldInteractive.DropObject(0);

                // Remove the item from the hand
                if (heldTargetTfrm != null) { Destroy(heldTargetTfrm.gameObject); }
                heldInteractiveId = StickyInteractive.NoID;
                heldInteractive = null;
                //isHeldSecondaryHandHold = false;
            }
        }

        /// <summary>
        /// If there is an object being held, attempt to activate or deactivate it.
        /// If no object is being held, but is being touched and is grabbable, attempt to grab it.
        /// If no object is being held, but is being touched and is non-grabbale, attempt to activate or deactivate it.
        /// </summary>
        public virtual void EngageInteractive()
        {
            // Is an object being held in the component (hand)?
            if (CheckHeldInteractive())
            {
                // Attempt to activate or deactivate the object being held
                ActivateInteractive(heldInteractive, !heldInteractive.IsActivated);
            }
            else if (isTouching && touchingInteractiveId != StickyInteractive.NoID && touchingInteractive != null)
            {
                // Touchable objects that are grabbale, should be grabbed first
                if (touchingInteractive.IsGrabbable)
                {
                    GrabInteractive(touchingInteractive, false);
                }
                // If touching a non-grabble and Activable, attempt to toggle Activate on/off
                else if (touchingInteractive.IsActivable)
                {
                    ActivateInteractive(touchingInteractive, !touchingInteractive.IsActivated);
                }
            }
        }

        /// <summary>
        /// Grab an interactive-enabled Grabbable object. Override this method to:
        /// 1. Perform a specific grab action
        /// 2. Determine if you should be using the primary or secondary hand hold position.
        /// </summary>
        public virtual void GrabInteractive (StickyInteractive stickyInteractive, bool isSecondaryHandHold)
        {
            if (!CheckHeldInteractive())
            {
                if (isCanGrab && stickyInteractive.IsGrabbable)
                {
                    if (palmTransform != null)
                    {
                        // Get the unique ID of the interactive-enabled object we want to hold 
                        int interactiveID = stickyInteractive.StickyInteractiveID;

                        // If the component is touching this interactive-enabled object, stop touching it
                        if (touchingInteractiveId != StickyInteractive.NoID && touchingInteractiveId == interactiveID)
                        {
                            touchingInteractive = null;
                            touchingInteractiveId = 0;
                            isTouching = false;
                        }

                        // Perform any actions required when the interactive-enabled object is grabbed.
                        // This includes performing actions configured in the StickyInteractive editor.
                        stickyInteractive.GrabObject(0, isSecondaryHandHold);
                        // This is NOT being held by a Sticky3D character
                        stickyInteractive.IsHeld = false;

                        if (!stickyInteractive.isParentOnGrab || ParentIteractive(stickyInteractive, transform, isSecondaryHandHold))
                        {
                             // If not parenting the object to the hand when grabbed, set up a transform for the object to follow
                            if (!stickyInteractive.isParentOnGrab)
                            {
                                Vector3 wsTargetPosition = Vector3.zero;
                                Quaternion wsTargetRotation = Quaternion.identity;

                                if (stickyInteractive.IsReadable)
                                {
                                    // If readable, use the palm position rather than the grab position on the interactive object
                                    if (palmTransform != null)
                                    {
                                        wsTargetPosition = palmTransform.position;
                                        wsTargetRotation = palmTransform.rotation;
                                    }
                                    else
                                    {
                                        // Fallback to the interactor transform
                                        wsTargetPosition = transform.position;
                                        wsTargetRotation = transform.rotation;
                                    }
                                }
                                else
                                {
                                    GetHoldPositionAndRotation(stickyInteractive, isSecondaryHandHold, out wsTargetPosition, out wsTargetRotation);
                                }

                                GameObject _targetGO = new GameObject("FollowTarget");
                                heldTargetTfrm = _targetGO.transform;
                                heldTargetTfrm.SetPositionAndRotation(wsTargetPosition, wsTargetRotation);
                                heldTargetTfrm.SetParent(transform);
                                stickyInteractive.SetFollowTarget(heldTargetTfrm);
                            }

                            heldInteractive = stickyInteractive;
                            heldInteractiveId = interactiveID;
                            //isHeldSecondaryHandHold = isSecondaryHandHold;
                        }

                        // Perform any actions required, immediately after the object is grabbed
                        stickyInteractive.PostGrabObject(0, isSecondaryHandHold);
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("StickyInteractorBridge.GrabInteractive " + gameObject.name + " cannot grab " + stickyInteractive.name + " because the palm transform is not set on " + name); }
                    #endif
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("StickyInteractorBridge.GrabInteractive " + name + " cannot grab " + stickyInteractive.name + " because it is not Grabbable"); }
                #endif
            }
        }

        /// <summary>
        /// Initialise the StickyInteractorBridge. Has no effect if called multiple times.
        /// </summary>
        public virtual void Initialise()
        {
            if (isInitialised) { return; }

            if (!TryGetComponent<Collider>(out touchCollider))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: The StickyInteractorBridge component requires a trigger collider on " + name);
                #endif
            }
            else if (!touchCollider.isTrigger)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: The StickyInteractorBridge component requires a trigger collider on " + name);
                #endif
            }
            else
            {
                ValidateAnimActions();

                #if SCSM_XR && SSC_UIS
                EnableOrDisableHandVRAnimate(handAnimator != null);
                #else
                // Always disable Hand Animate if XR is not available
                EnableOrDisableHandVRAnimate(false);
                #endif

                if (currentCharInputXR == null) { currentCharInputXR = new CharacterInputXR(); }
                if (previousCharInputXR == null) { previousCharInputXR = new CharacterInputXR(); }

                isInitialised = true;
            }
        }

        /// <summary>
        /// Stop touching an interactive-enabled object.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        public virtual void StopTouchingInteractive (StickyInteractive stickyInteractive)
        {
            // Verify this is the object being touched (not sure when this wouldn't be true...)
            if (stickyInteractive.StickyInteractiveID == touchingInteractiveId)
            {
                isTouching = false;
                touchingInteractive = null;
                touchingInteractiveId = 0;

                stickyInteractive.StopTouchingObject(0);
            }
        }

        /// <summary>
        /// Indicate that this is component is touching a touchable interactive-enabled object.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        /// <param name="hitPoint"></param>
        /// <param name="hitNormal"></param>
        public virtual void TouchInteractive (StickyInteractive stickyInteractive, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (isCanTouch && stickyInteractive != null && stickyInteractive.IsTouchable)
            {
                isTouching = true;

                touchingInteractive = stickyInteractive;
                touchingInteractiveId = stickyInteractive.StickyInteractiveID;

                stickyInteractive.TouchObject(hitPoint, hitNormal, 0);
            }
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Get the world space palm position
        /// </summary>
        /// <returns></returns>
        public Vector3 GetHandPalmPosition()
        {
            return palmTransform != null ? palmTransform.position : Vector3.zero;
        }

        /// <summary>
        /// Get the world space palm rotation
        /// </summary>
        /// <returns></returns>
        public Quaternion GetHandPalmRotation()
        {
            return palmTransform != null ? palmTransform.rotation : Quaternion.identity;
        }

        /// <summary>
        /// Set if this item can activate an interactive-enabled Activable object?
        /// </summary>
        /// <param name="canActivate"></param>
        public void SetIsCanActivate (bool canActivate)
        {
            isCanActivate = canActivate;
        }

        /// Set if this item can grab an interactive-enabled Grabbable object?
        /// </summary>
        /// <param name="isCanTouch"></param>
        public void SetIsCanGrab(bool canGrab)
        {
            isCanGrab = canGrab;
        }

        /// <summary>
        /// Set if this item can touch an interactive-enabled Touchable object?
        /// </summary>
        /// <param name="canTouch"></param>
        public void SetIsCanTouch(bool canTouch)
        {
            isCanTouch = canTouch;
        }

        /// <summary>
        /// Set the transform that represents hand palm position and rotation.
        /// Returns true if set successfully.
        /// </summary>
        /// <param name="newPalmTransform"></param>
        public bool SetHandPalmTransform (Transform newPalmTransform)
        {
            if (newPalmTransform != null)
            {
                // we could impose other rules here

                palmTransform = newPalmTransform;
                return true;
            }
            else
            {
                palmTransform = null;
                return false;
            }
        }

        /// <summary>
        /// Validate that the animate actions have been created.
        /// </summary>
        public void ValidateAnimActions()
        {
            if (handVRGrip == null) { handVRGrip = new S3DAnimAction() { parameterType = S3DAnimAction.ParameterType.Float }; }
            if (handVRTrigger == null) { handVRTrigger = new S3DAnimAction() { parameterType = S3DAnimAction.ParameterType.Float }; }
        }

        #endregion
    }
}