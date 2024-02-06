using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This component, currently in Technical Preview enables VR hands on a Sticky3D character
    /// to interact with interactive-enabled objects in the scene.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Character/Sticky XR Interactor")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyXRInteractor : MonoBehaviour
    {
        #region Enumerations

        public enum LookMode
        {
            Interactive = 10,
            Teleport = 30
        }

        public enum InteractorType
        {
            LeftHand = 0,
            RightHand = 1
        }

        public enum InteractivePointMode
        {
            Beam = 0,
            Target = 1
        }

        #endregion

        #region Public Static Variables

        public static int LookModeInteractiveInt = (int)LookMode.Interactive;
        public static int LookModeTeleportInt = (int)LookMode.Teleport;

        public static int InteractorTypeLeftHandInt = (int)InteractorType.LeftHand;
        public static int InteractorTypeRightHandInt = (int)InteractorType.RightHand;

        public static int InteractivePointModeBeamInt = (int)InteractivePointMode.Beam;
        public static int InteractivePointModeTargetInt = (int)InteractivePointMode.Target;

        public readonly static string targetPanelName = "TargetPanel";
        public readonly static string reticlePanelName = "ReticlePanel";

        #endregion

        #region Public Variables - General
        /// <summary>
        /// If enabled, the Initialise() will be called as soon as Start() runs. This should be disabled if you are
        /// instantiating the component through code.
        /// </summary>
        public bool initialiseOnStart = false;

        /// <summary>
        /// The Sticky3D character this interactor is a child of
        /// </summary>
        public StickyControlModule stickyControlModule = null;

        /// <summary>
        /// The start width (in metres) of the beam on the local x-axis
        /// In this version the width will be the same for the entire length of the beam.
        /// </summary>
        [Range(0.001f, 0.05f)] public float beamStartWidth = 0.01f;

        /// <summary>
        /// Maximum distance the interactor can see ahead
        /// </summary>
        [Range(0.1f, 50f)] public float maxDistance = 5f;

        #endregion

        #region Public Variables - Interactive

        /// <summary>
        /// When LookMode is Interactive, the beam can be pointed at a touchable interactive-enabled object.
        /// </summary>
        public bool isPointToTouch = true;

        /// <summary>
        /// When LookMode is Interactive, the beam can be pointed at a grabbable interactive-enabled object.
        /// </summary>
        public bool isPointToGrab = true;

        /// <summary>
        /// When an interactive-enabled object is dropped, the velocity of the hand is applied to it.
        /// The strength of the throw, applies more or less velocity to the object. This enables your
        /// character to have more strength in one hand that the other.
        /// </summary>
        [Range(0.1f, 10f)] public float throwStrength = 1.5f;

        /// <summary>
        /// When the Point Mode is Target, this is the normalised distance the Target sprite is
        /// moved toward the hand away from the object or obstacle to help prevent clipping.
        /// </summary>
        [Range(0f, 0.3f)] public float targetOffsetDistanceN = 0.05f;

        #endregion

        #region Public Variables - Teleport

        /// <summary>
        /// The normalised point along the beam at which the curve begins when the LookMode is Teleport.
        /// </summary>
        [Range(0.1f, 0.9f)] public float beamStartCurve = 0.4f;

        /// <summary>
        /// This is the prefab that is instantiated in the scene and is enabled and disabled
        /// during TelePort activities.
        /// </summary>
        public GameObject teleportReticle = null;

        /// <summary>
        /// Should the teleporter reticle show the destination ground normal?
        /// </summary>
        public bool isShowTeleportReticleNormal = true;

        /// <summary>
        /// When teleporting, check if the character would fit into the space above the location.
        /// NOTE: Currently does not check for S3D characters at that location.
        /// </summary>
        public bool isTeleportObstacleCheck = true;

        /// <summary>
        /// Is teleporting disabled after the character is teleported to a new location?
        /// </summary>
        public bool isAutoDisableTeleporter = true;

        #endregion

        #region Public Properties - General

        /// <summary>
        /// [READONLY]
        /// Has the module been initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// [READONLY]
        /// Is the interactor beam enabled and visible in the scene?
        /// </summary>
        public bool IsBeamEnabled { get { return isBeamEnabled; } }

        #endregion

        #region Public Properties - Interactive

        /// <summary>
        /// [READONLY]
        /// If the interactive beam is active, this could be a StickyInteractive object position or a point
        /// maxDistance from the hand if no interactive-enabled objects are being pointed at.
        /// </summary>
        public Vector3 GetLookingAtPoint { get { return isInitialised && isInteractiveEnabled ? GetBeamEndPoint() : Vector3.zero; } }

        /// <summary>
        /// [READONLY]
        /// Is the interactive feature currently active?
        /// </summary>
        public bool IsInteractiveEnabled { get { return isInteractiveEnabled; } }

        #endregion

        #region Public Properties - Teleport

        /// <summary>
        /// [READONLY]
        /// Is the teleporting feature currently active?
        /// </summary>
        public bool IsTeleportEnabled { get { return isTeleportEnabled; } }

        /// <summary>
        /// [READONLY]
        /// If the teleporter is active, return the potential Teleportation location in world space.
        /// Otherwise, return 0, 0, 0.
        /// </summary>
        public Vector3 TeleportLocation { get { return isInitialised && isTeleportEnabled ? GetBeamEndPoint() : Vector3.zero; } }

        /// <summary>
        /// [READONLY]
        /// If the teleporter is enabled, is the location a potential teleportation candidate?
        /// </summary>
        public bool IsTeleportLocationValid { get { return isInitialised && isTeleportEnabled && isTeleportCurrentLocationValid; } }

        #endregion

        #region Public Variables and Properties - Internal Only

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [HideInInspector] public bool allowRepaint = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool showGeneralSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool showInteractiveSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool showTeleportSettingsInEditor = false;

        #endregion

        #region Private Variables - General

        /// <summary>
        /// The type of the Sticky XR Interactor. E.g., left or right hand
        /// </summary>
        [SerializeField] private InteractorType interactorType = InteractorType.LeftHand;

        /// <summary>
        /// The method the interactor uses to interact with objects in the scene
        /// </summary>
        [SerializeField] private LookMode lookMode = LookMode.Interactive;

        /// <summary>
        /// The default colour gradient of the StickyXRInterctor beam
        /// </summary>
        [SerializeField] private Gradient defaultBeamGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            alphaKeys = new[] { new GradientAlphaKey(0.3f, 0f), new GradientAlphaKey(0.3f, 1f) },
        };

        /// <summary>
        /// The colour gradient of the StickyXRInteractor beam when it is interacting with an object in the scene
        /// </summary>
        [SerializeField] private Gradient activeBeamGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(Color.blue, 0f), new GradientColorKey(Color.blue, 1f) },
            alphaKeys = new[] { new GradientAlphaKey(0.3f, 0f), new GradientAlphaKey(0.3f, 1f) },
        };

        /// <summary>
        /// Is pointing permitted when a weapon is held? By default, this is disabled so that the same button
        /// can be configured for both pointing and firing a weapon. For example, a controller trigger.
        /// </summary>
        [SerializeField] private bool isPointWeaponHeld = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Reference to the LineRenderer component which should be on a child
        /// gameobject of this module.
        /// </summary>
        [System.NonSerialized] internal LineRenderer lineRenderer = null;

        private bool isInitialised = false;
        private RaycastHit raycastHit;
        private bool isBeamEnabled = false;
        private bool isTargetEnabled = false;
        private int numLinePositions = 0;
        private int lookModeInt = LookModeInteractiveInt;
        private int interactorTypeInt = InteractorTypeLeftHandInt;

        private int stickyID = 0;

        #endregion

        #region Private Variables - Interactive

        /// <summary>
        /// This needs to be a child transform of the hand. The z-axis (forward) should point in the direction
        /// of the palm is facing. This is the palm “normal”. The x-axis (right) is towards the direction of the
        /// index finger and, for the right hand, the y-axis (up) is in a thumbs-up direction.
        /// </summary>
        [SerializeField] private Transform palmTransform = null;

        /// <summary>
        /// The visual pointing mode used with a LookMode of Interactive.
        /// </summary>
        [SerializeField] private InteractivePointMode interactivePointMode = InteractivePointMode.Beam;

        /// <summary>
        /// The sprite that will be used instead of the pointer beam when the Point Mode is Target.
        /// </summary>
        [SerializeField] private Sprite interactiveTargetSprite = null;

        /// <summary>
        /// The default colour of the pointer reticle when the Point Mode is Target
        /// </summary>
        [SerializeField] private Color defaultTargetColour = Color.white;

        /// <summary>
        /// The colour of the StickyXRInteractor Target when it is interacting with an object in the scene
        /// </summary>
        [SerializeField] private Color activeTargetColour = Color.green;

        private int interactivePointModeInt = 0;

        [System.NonSerialized] private StickyInteractive lookingAtInteractive = null;
        private int lookingAtInteractiveId = 0;
        private Vector3 lookingAtHitNormal = Vector3.zero;
        private bool isTouching = false;

        [System.NonSerialized] private StickyInteractive touchingInteractive = null;
        private int touchingInteractiveId = 0;

        private bool isActiveColour = false;

        // UI raycast
        [System.NonSerialized] private UnityEngine.EventSystems.EventSystem eventSystem = null;
        //[System.NonSerialized] private StickyGraphicRaycaster graphicRaycaster = null;
        [System.NonSerialized] private S3DPointerEventData pointerEventData = null;
        [System.NonSerialized] private List<UnityEngine.EventSystems.RaycastResult> raycastResultList = null;
        #if SSC_UIS && SCSM_XR
        UnityEngine.InputSystem.UI.InputSystemUIInputModule uiInputModule;
        UnityEngine.InputSystem.InputAction leftClickInputAction = null;
        UnityEngine.InputSystem.InputAction rightClickInputAction = null;
        #endif

        // Cache UI items to improve performance
        [System.NonSerialized] private Canvas targetCanvas = null;
        [System.NonSerialized] private Transform targetCanvasTfrm = null;
        [System.NonSerialized] private RectTransform targetCanvasPanel = null;
        [System.NonSerialized] private RectTransform targetPanel = null;
        [System.NonSerialized] private Transform targetReticlePanel = null;
        [System.NonSerialized] private UnityEngine.UI.Image targetReticleImg = null;
        [System.NonSerialized] private UnityEngine.UI.Image targetBackgroundImg = null;

        /// <summary>
        /// [INTERNAL ONLY] The item being held in the hand
        /// </summary>
        [System.NonSerialized] private StickyInteractive heldInteractive = null;
        /// <summary>
        /// [INTERNAL ONLY] The ID of the object being held in the hand
        /// </summary>
        private int heldInteractiveId = 0;

        /// <summary>
        /// [INTERNAL ONLY] Is the interactive object being held at
        /// the secondary hand hold position with this hand?
        /// </summary>
        private bool isHeldSecondaryHandHold = false;

        /// <summary>
        /// This is the transform created at runtime as a child of the palmTransform,
        /// that a grabbable interactive-enabled object uses to follow the hand.
        /// It is used when the isParentOnGrab is NOT enabled on the object.
        /// </summary>
        [System.NonSerialized] private Transform heldTargetTfrm = null;

        #endregion

        #region Private Variables - Teleport

        private int numCurvedPositions = 8;

        /// <summary>
        /// Is LookMode interactive set and the beam enabled?
        /// </summary>
        private bool isInteractiveEnabled = false;

        private GameObject teleportPrefabInstance = null;
        private bool isTeleportEnabled = false;
        private bool isTeleportCurrentLocationValid = false;
        private bool isTeleportPreviousLocationValid = false;
        #endregion

        #region Initialise Methods
        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Private or Internal Methods

        /// <summary>
        /// Attempt to activate (or deactivate) an interactive-enabled object
        /// </summary>
        /// <param name="stickyInteractive"></param>
        private void ActivateInteractive(StickyInteractive stickyInteractive, bool isActivate)
        {
            if (isInitialised && stickyInteractive != null && stickyInteractive.IsActivable)
            {
                if (isActivate)
                {
                    stickyInteractive.ActivateObject(stickyID);
                }
                else
                {
                    stickyInteractive.DeactivateObject(stickyID);
                }
            }
        }

        /// <summary>
        /// Drop an interactive-enabled object
        /// </summary>
        /// <param name="stickyInteractive"></param>
        /// <param name="isLeftHand"></param>
        private void DropInteractive(StickyInteractive stickyInteractive)
        {
            // Verify that the hand is holding something
            if (heldInteractiveId != StickyInteractive.NoID)
            {
                // Unregister the active colliders BEFORE DropObject(..)
                Collider[] attachedObjectColliders = stickyInteractive.Colliders;
                int numAttachedColliders = attachedObjectColliders == null ? 0 : attachedObjectColliders.Length;

                for (int colIdx = 0; colIdx < numAttachedColliders; colIdx++)
                {
                    Collider attachCollider = attachedObjectColliders[colIdx];
                    if (attachCollider != null && attachCollider.enabled) { stickyControlModule.DetachCollider(attachCollider.GetInstanceID()); }
                }

                stickyInteractive.SetExternalThrowStrength(throwStrength);

                if (stickyID == 0) { stickyID = stickyControlModule.StickyID; }

                // Perform any actions required when the interactive-enabled object is dropped.
                // This includes performing actions configured in the StickyInteractive editor.
                stickyInteractive.DropObject(stickyID);

                // Remove the item from the hand
                if (heldTargetTfrm != null) { Destroy(heldTargetTfrm.gameObject); }
                heldInteractiveId = StickyInteractive.NoID;
                heldInteractive = null;
                isHeldSecondaryHandHold = false;

                stickyControlModule.ClearHoldingInteractive(interactorTypeInt == InteractorTypeLeftHandInt);
            }
        }

        private void EnableOrDisableBeam(bool isEnabled)
        {
            isBeamEnabled = isEnabled;

            if (isInitialised)
            {
                // Reset the line renderer visuals
                if (isBeamEnabled)
                {
                    lineRenderer.colorGradient = defaultBeamGradient;
                    isActiveColour = false;
                }

                lineRenderer.enabled = isBeamEnabled;
            }
        }

        private void EnableOrDisableTarget(bool isEnabled)
        {
            if (isInitialised)
            {
                if (isEnabled)
                {
                    ShowOrHideTarget(true);

                    // Reset the Target visual colour
                    if (targetReticleImg != null)
                    {
                        targetReticleImg.color = defaultTargetColour;
                        isActiveColour = false;
                    }

                    isTargetEnabled = true;
                }
                else
                {
                    ShowOrHideTarget(false);

                    isTargetEnabled = false;
                }
            }
            else
            {
                isTargetEnabled = false;
            }
        }

        /// <summary>
        /// Attempt to enable or disable interactive mode
        /// </summary>
        /// <param name="isEnabled"></param>
        private void EnableOrDisableInteractive(bool isEnabled)
        {
            if (!isInitialised) { return; }

            if (isEnabled)
            {
                // Check to see if we can point at interactive objects while holding a weapon
                if (!isPointWeaponHeld &&
                     ((interactorTypeInt == InteractorTypeRightHandInt && stickyControlModule.IsRightHandHoldingWeapon)
                  || (interactorTypeInt == InteractorTypeLeftHandInt && stickyControlModule.IsLeftHandHoldingWeapon)))
                {
                   // Do nothing
                }
                else if (lookModeInt == LookModeInteractiveInt)
                {
                    if (!stickyControlModule.IsLookInteractiveEnabled)
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning("WARNING: StickyXRInteractive cannot enable Interactive Look Mode as Interactive is not enabled on the Look tab of " + stickyControlModule.name);
                        #endif
                    }
                    else if (interactivePointModeInt == InteractivePointModeTargetInt)
                    {
                        if (isBeamEnabled) { EnableOrDisableBeam(false); }

                        if (!isTargetEnabled) { EnableOrDisableTarget(true); }
                    }
                    // If in Beam mode and beam is turned off, turn it on
                    else if (!isBeamEnabled)
                    {
                        // If the Target it on, turn it off as we're in Beam mode
                        if (isTargetEnabled) { EnableOrDisableTarget(false); }

                        EnableOrDisableBeam(true);
                    }
                }
                else // If required, switch to teleporting mode
                {
                    // If teleporting is on, turn it off
                    if (isTeleportEnabled && lookModeInt == LookModeTeleportInt)
                    {
                        EnableOrDisableTeleport(false);
                    }

                    SetLookMode(LookMode.Interactive);

                    // If the beam is turned off, turn it on
                    if (!isBeamEnabled)
                    {
                        EnableOrDisableBeam(true);
                    }
                }

                if (isBeamEnabled || isTargetEnabled)
                {
                    isInteractiveEnabled = true;

                    MovePointer();
                }
            }
            // Disable Interactive
            else if (lookModeInt == LookModeInteractiveInt)
            {
                if (isBeamEnabled) { EnableOrDisableBeam(false); }
                if (isTargetEnabled) { EnableOrDisableTarget(false); }
                isInteractiveEnabled = false;
            }
        }

        /// <summary>
        /// Attempt to enable or disable teleporting
        /// </summary>
        /// <param name="isEnabled"></param>
        private void EnableOrDisableTeleport(bool isEnabled)
        {
            // The component must be initialised for teleporting to work
            if (!isInitialised) { return; }

            isTeleportCurrentLocationValid = false;

            if (isEnabled)
            {
                // If required, switch to teleporting mode
                if (lookModeInt != LookModeTeleportInt)
                {
                    SetLookMode(LookMode.Teleport);
                }

                // If the beam is turned off, turn it on
                if (!isBeamEnabled)
                {
                    EnableOrDisableBeam(true);
                    MovePointer();
                }

                if (teleportPrefabInstance != null && isBeamEnabled)
                {
                    MoveTeleporter();

                    // If the teleport destination object isn't visible, turn it on
                    if (!teleportPrefabInstance.activeSelf)
                    {
                        teleportPrefabInstance.SetActive(true);
                    }
                }

                isTeleportEnabled = true;
            }
            else
            {
                // If the teleport destination object is showing in the scene, disable it
                if (teleportPrefabInstance != null && teleportPrefabInstance.activeSelf)
                {
                    teleportPrefabInstance.SetActive(false);
                }

                isTeleportEnabled = false;

                // If the beam is turned on, turn it off
                if (isBeamEnabled)
                {
                    EnableOrDisableBeam(false);
                }
            }
        }

        /// <summary>
        /// Calculate the desired position and rotation of the object to be held.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        /// <param name="isSecondaryHandHold"></param>
        /// <param name="wsPosition"></param>
        /// <param name="wsRotation"></param>
        private void GetHoldPositionAndRotation (StickyInteractive stickyInteractive, bool isSecondaryHandHold, out Vector3 wsPosition, out Quaternion wsRotation)
        {
            // Get the palm of the hand's relative position and rotation to the hand bone.
            wsPosition = GetHandPalmPosition();
            wsRotation = GetHandPalmRotation();

            Vector3 handHoldLocalRotEuler = isSecondaryHandHold ? stickyInteractive.handHold2Rotation : stickyInteractive.handHold1Rotation;

            if (interactorTypeInt == InteractorTypeLeftHandInt && ((isSecondaryHandHold && stickyInteractive.handHold2FlipForLeftHand) || (!isSecondaryHandHold && stickyInteractive.handHold1FlipForLeftHand)))
            {
                // Flip the local "up" direction of the hand hold rotation
                handHoldLocalRotEuler.y = -handHoldLocalRotEuler.y;
            }

            // We want the object's hand hold rotation to point towards the palm.
            Quaternion handHoldLocalRot = Quaternion.Euler(handHoldLocalRotEuler);

            wsRotation *= Quaternion.Inverse(handHoldLocalRot);

            // Subtract the hand hold offset
            wsPosition -= wsRotation * stickyInteractive.GetHandHoldLocalOffset(isSecondaryHandHold);
        }


        /// <summary>
        /// Get the Target RectTransform or panel
        /// </summary>
        /// <returns></returns>
        public RectTransform GetTargetPanel()
        {
            if (targetCanvas == null) { return null; }
            else { return S3DUtils.GetChildRectTransform(targetCanvas.transform, targetPanelName, this.name); }
        }

        /// <summary>
        /// Grab an interactive-enabled object and hold it in the hand
        /// </summary>
        /// <param name="stickyInteractive"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="isSecondaryHandHold"></param>
        private void HoldInteractive(StickyInteractive stickyInteractive, Transform handTrfm, bool isSecondaryHandHold)
        {
            // Check to see if the hand is already holding something
            if (heldInteractiveId != StickyInteractive.NoID)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("The " + (interactorTypeInt == InteractorTypeLeftHandInt ? "left" : "right") + " hand of " + stickyControlModule.name + " already holds an interactive object");
                #endif
            }
            else
            {
                // Get the unique ID of the interactive-enabled object we want to hold 
                int interactiveID = stickyInteractive.StickyInteractiveID;

                bool isReadyToGrab = true;
                bool isOtherHandHoldingSameObject = false;
                bool isOtherSameHandHold = false;

                // Check to see if the other hand is already gripping this object
                StickyXRInteractor otherStickyXRInteractive = interactorTypeInt == InteractorTypeLeftHandInt ? stickyControlModule.rightHandXRInteractor : stickyControlModule.leftHandXRInteractor;

                // The other hand is holding the object we want to grab
                if (otherStickyXRInteractive != null && otherStickyXRInteractive.heldInteractiveId == interactiveID)
                {
                    isOtherHandHoldingSameObject = true;
                    isOtherSameHandHold = otherStickyXRInteractive.isHeldSecondaryHandHold == isSecondaryHandHold;

                    if (otherStickyXRInteractive.SnatchHeldInteractive(stickyInteractive))
                    {
                        // Currently, can unconditionally grab an object from the other hand of the same player
                        isReadyToGrab = true;
                    }
                    else
                    {
                        isReadyToGrab = false;
                    }
                }

                if (isReadyToGrab)
                {
                    // If the character is looking at this interactive-enabled object, stop looking at it
                    if (lookingAtInteractiveId != StickyInteractive.NoID && lookingAtInteractiveId == interactiveID)
                    {
                        StopLookAtInteractive();
                    }

                    // If the character is touching this interactive-enabled object, stop touching it
                    if (touchingInteractiveId != StickyInteractive.NoID && touchingInteractiveId == interactiveID)
                    {
                        touchingInteractive = null;
                        touchingInteractiveId = 0;
                        isTouching = false;
                    }

                    if (stickyID == 0) { stickyID = stickyControlModule.StickyID; }

                    // If this is a weapon to be grabbed, check if we need to trigger anything
                    if (stickyInteractive.IsStickyWeapon && stickyControlModule.onPreStartHoldWeapon != null)
                    {
                        stickyControlModule.onPreStartHoldWeapon.Invoke(stickyID, interactiveID, false, Vector3.zero);
                    }

                    // If the object was previous held in the other hand, we don't want to perform another GrabObject
                    // as that will overwrite original rigidbody settings (used on next Drop). We also already have
                    // the active colliders registered, so don't need to do that either.
                    if (!isOtherHandHoldingSameObject)
                    {
                        // Perform any actions required when the interactive-enabled object is grabbed.
                        // This includes performing actions configured in the StickyInteractive editor.
                        stickyInteractive.GrabObject(stickyID, isSecondaryHandHold);

                        // Register the active colliders AFTER GrabObject(..)
                        Collider[] attachedObjectColliders = stickyInteractive.Colliders;
                        int numAttachedColliders = attachedObjectColliders == null ? 0 : attachedObjectColliders.Length;

                        for (int colIdx = 0; colIdx < numAttachedColliders; colIdx++)
                        {
                            Collider attachCollider = attachedObjectColliders[colIdx];
                            if (attachCollider.enabled) { stickyControlModule.AttachCollider(attachCollider); }
                        }
                    }

                    if (!stickyInteractive.isParentOnGrab || ParentIteractive(stickyInteractive, handTrfm, isSecondaryHandHold))
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
                            heldTargetTfrm.SetParent(handTrfm);
                            stickyInteractive.SetFollowTarget(heldTargetTfrm);
                        }

                        heldInteractive = stickyInteractive;
                        heldInteractiveId = interactiveID;
                        isHeldSecondaryHandHold = isSecondaryHandHold;

                        // Added v1.1.0 Beta 12i based on stickyControlModule.HoldInteractive(..)
                        stickyControlModule.SetHoldingInteractive(stickyInteractive, interactorTypeInt == InteractorTypeLeftHandInt);
                    }

                    stickyInteractive.PostGrabObject(stickyID, isSecondaryHandHold);
                }
            }
        }

        /// <summary>
        /// Move the teleport prefab to the end of the beam
        /// </summary>
        private void MoveTeleporter()
        {
            if (isBeamEnabled)
            {
                Quaternion reticleRotation = isShowTeleportReticleNormal && lookingAtHitNormal != Vector3.zero ? Quaternion.FromToRotation(Vector3.up, lookingAtHitNormal) : stickyControlModule.GetCurrentRotation;

                // Move the teleport object to the end of the beam
                teleportPrefabInstance.transform.SetPositionAndRotation(GetBeamEndPoint(), reticleRotation);
            }
            // If the beam has been turned off but Teleporting is still on,
            // turn it off.
            else if (isTeleportEnabled)
            {
                EnableOrDisableTeleport(false);
            }
        }

        /// <summary>
        /// Attempt to parent an interactive-enabled object to a hand.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        /// <param name="handTrfm"></param>
        /// <param name="isSecondaryHandHold"></param>
        /// <returns></returns>
        private bool ParentIteractive(StickyInteractive stickyInteractive, Transform handTrfm, bool isSecondaryHandHold)
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
        /// Set the correct number of positions for the type of LookMode, and set those positions to the default values.
        /// </summary>
        private void ReinitialisePositions()
        {
            lineRenderer.positionCount = lookModeInt == LookModeTeleportInt ? numCurvedPositions : 2;
            numLinePositions = lineRenderer.positionCount;

            if (numLinePositions > 1)
            {
                lineRenderer.SetPosition(0, new Vector3(0f, 0f, 0f));

                for (int i = 1; i < numLinePositions; i++)
                {
                    lineRenderer.SetPosition(i, new Vector3(0f, 0f, 1f));
                }
            }
        }

        /// <summary>
        /// Attempt to show or hide the Target reticle
        /// </summary>
        /// <param name="isShown"></param>
        private void ShowOrHideTarget(bool isShown)
        {
            if (targetPanel != null)
            {
                targetPanel.gameObject.SetActive(isShown);
            }
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            if (isInitialised)
            {
                if (lookModeInt == LookModeInteractiveInt)
                {
                    MovePointer();
                }
            }
        }

        private void FixedUpdate()
        {
            if (isInitialised)
            {
                // The teleporter needs to keep in sync with colliders like the
                // ground which could be moving.
                if (lookModeInt == LookModeTeleportInt)
                {
                    MovePointer();
                    if (isTeleportEnabled) { MoveTeleporter(); }
                }

                // TESTING - attempt to improve high speed collisions
                // Doesn't seem to make any difference
                //if (heldInteractiveId != StickyInteractive.NoID)
                //{
                //    if (heldInteractive != null)
                //    {
                //        Rigidbody rb = heldInteractive.ObjectRigidbody;
                //        if (rb != null)
                //        {
                //            rb.MovePosition(heldInteractive.transform.position);
                //        }
                //    }
                //}
            }
        }

        #endregion

        #region Event Methods

        private void OnDestroy()
        {
            // There is a circular reference in the StickyControlModule
            // back to this component. We need to be careful that we don't
            // prevent StickyControlModule or this component from being fully
            // destroyed or removed.
            // Objects with a reference are destroyed but NOT garbage collected.
            if (stickyControlModule != null)
            {
                if (interactorType == InteractorType.LeftHand)
                {
                    stickyControlModule.leftHandXRInteractor = null;
                }
                else
                {
                    stickyControlModule.rightHandXRInteractor = null;
                }
            }
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Attempt to activate an interactive-enabled object.
        /// If one isn't being held in the hand, check what is being looked at.
        /// Objects that are grabbable, must be held to be activated.
        /// </summary>
        public void ActivateInteractive()
        {
            // Is an object being held in the hand?
            if (CheckHeldInteractive() && heldInteractive.IsActivable)
            {
                // Attempt to activate the object being held
                ActivateInteractive(heldInteractive, true);
            }
            // If an activable object is not grabbable, and is being looked at, attempt to activate it.
            else if (CheckLookingAtInteractive() && lookingAtInteractive.IsActivable && !lookingAtInteractive.IsGrabbable)
            {
                // Attempt to activate the object being looked at
                ActivateInteractive(lookingAtInteractive, true);

                // If the object was already activated, or has now been activated,
                // turn off looking at.
                if (lookingAtInteractive != null && lookingAtInteractive.IsActivated)
                {
                    StopLookAtInteractive();
                }
            }
        }

        /// <summary>
        /// Check if there is an item being held in the hand.
        /// If the item being held was destroyed, make sure it isn't
        /// selected by the character, and return false.
        /// </summary>
        /// <returns>true if held, otherwise false</returns>
        public bool CheckHeldInteractive()
        {
            if (!isInitialised) { return false; }
            else if (heldInteractiveId != StickyInteractive.NoID)
            {
                // Check that it hasn't been destroyed
                if (heldInteractive == null)
                {
                    stickyControlModule.RemoveSelectedInteractiveID(heldInteractiveId);

                    if (heldTargetTfrm != null) { Destroy(heldTargetTfrm.gameObject); }

                    // Cannot find it so assume character is no longer holding it
                    heldInteractiveId = StickyInteractive.NoID;
                    isHeldSecondaryHandHold = false;

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
        /// Check if the character is looking or pointing at an interactive-enabled object.
        /// Also checks to see if the item hasn't been destroyed.
        /// </summary>
        /// <returns></returns>
        public bool CheckLookingAtInteractive()
        {
            bool isLookingAtSomething = false;

            // Are we currently looking at an interactive-enabled object?
            if (isInitialised && lookingAtInteractiveId != StickyInteractive.NoID)
            {
                // Check that it hasn't been destroyed
                if (lookingAtInteractive == null)
                {
                    // Remove it from the selected list if it has previously been selected
                    stickyControlModule.RemoveSelectedInteractiveID(lookingAtInteractiveId);

                    // Cannot find it so assume character is no longer looking at it
                    lookingAtInteractiveId = StickyInteractive.NoID;
                    lookingAtHitNormal = Vector3.zero;
                    isTouching = false;

                    //lookingAtPoint = Vector3.zero;
                }
                else
                {
                    isLookingAtSomething = true;
                }
            }

            return isLookingAtSomething;
        }

        /// <summary>
        /// Attempt to deactivate an interactive-enabled object.
        /// If one isn't being held in the hand, check what is being looked at.
        /// Objects that are grabbable, must be held to be deactivated.
        /// </summary>
        public void DeactivateInteractive()
        {
            // Is an object being held in the hand?
            if (CheckHeldInteractive())
            {
                // Attempt to deactivate the object being held
                ActivateInteractive(heldInteractive, false);
            }
            else if (CheckLookingAtInteractive() && !lookingAtInteractive.IsGrabbable)
            {
                // Attempt to deactivate the object being looked at
                ActivateInteractive(lookingAtInteractive, false);

                // If the object was already deactivated, or has now been deactivated,
                // turn off looking at.
                if (lookingAtInteractive != null && !lookingAtInteractive.IsActivated)
                {
                    StopLookAtInteractive();
                }
            }
        }

        /// <summary>
        /// Attempt to turn off the StickyXRInteractor beam
        /// </summary>
        public void DisableBeam()
        {
            EnableOrDisableBeam(false);
        }

        /// <summary>
        /// Attempt to turn off Interactive mode
        /// </summary>
        public void DisableInteractive()
        {
            EnableOrDisableInteractive(false);
        }

        /// <summary>
        /// Attempt to turn off Teleporting
        /// </summary>
        public void DisableTeleport()
        {
            EnableOrDisableTeleport(false);
        }

        /// <summary>
        /// Drop the interactive-enabled object in the hand of the character.
        /// </summary>
        public void DropInteractive()
        {
            DropInteractive(heldInteractive);
        }

        /// <summary>
        /// Attempt to turn the on StickyXRInteractor beam
        /// </summary>
        public void EnableBeam()
        {
            if (isInitialised)
            {
                if (lookModeInt == LookModeInteractiveInt)
                {
                    EnableOrDisableInteractive(true);
                }
                else if (lookModeInt == LookModeTeleportInt)
                {
                    EnableOrDisableTeleport(true);
                }
                else
                {
                    // fallback just turning on the beam
                    EnableOrDisableBeam(true);
                }
            }
        }

        /// <summary>
        /// Attempt to enable interactive mode
        /// </summary>
        public void EnableInteractive()
        {
            EnableOrDisableInteractive(true);
        }

        /// <summary>
        /// Attempt to turn on teleporting
        /// </summary>
        public void EnableTeleport()
        {
            EnableOrDisableTeleport(true);
        }

        /// <summary>
        /// If there is an object being held, attempt to activate or deactivate it.
        /// If no object is being held, but is being touched and is grabbable, attempt to grab it.
        /// If no object is being held, but is being touched and is non-grabbale, attempt to activate or deactivate it.
        /// If no object being held and is not being touched, attempt to engage with what is being looked at.
        /// Touching occurs when the StickyXRTouch component is attached to hand.
        /// </summary>
        public void EngageInteractive()
        {
            // Is an object being held in the hand?
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
            else
            {
                EngageLookingAtInteractive();
            }
        }

        /// <summary>
        /// Attempt to take action using a hand, based on what the
        /// interactive-enabled object the character is currently looking at.
        /// This will be based on what features are enabled on the object.
        /// </summary>
        public void EngageLookingAtInteractive()
        {
            // Are we currently looking at an interactive-enabled object?
            if (CheckLookingAtInteractive())
            {
                //bool isAttemptingToGrab = false;

                // When an item is grabbed it is automatically unselected
                if (lookingAtInteractive.IsSelectable)
                {
                    ToggleSelectLookedAtInteractive();
                }

                if (lookingAtInteractive.IsGrabbable)
                {
                    // If it is not also touchable, immediately attempt to grab the object.
                    // The character could be out of hand reach and the object could "snap"
                    // into the hand.
                    if (!lookingAtInteractive.IsTouchable)
                    {
                        GrabLookedAtInteractive(false);
                    }
                    // Touch is enabled and the hand is touching the object to grab
                    else if (isTouching)
                    {
                        //isAttemptingToGrab = true;
                        GrabLookedAtInteractive(false);
                    }
                }
                // Non-grabbable objects can potentially be activated (or deactivated)
                else if (lookingAtInteractive.IsActivable)
                {
                    // Attempt to toggle the Activated state of the object
                    ActivateInteractive(lookingAtInteractive, !lookingAtInteractive.IsActivated);
                }
            }
        }

        /// <summary>
        /// Return the current LookMode of the StickyXRInteractor
        /// </summary>
        /// <returns></returns>
        public LookMode GetLookMode()
        {
            return lookMode;
        }

        /// <summary>
        /// Return the active colour of the StickyXRInteractor Target
        /// </summary>
        /// <returns></returns>
        public Color GetActiveTargetColour()
        {
            return activeTargetColour;
        }

        /// <summary>
        /// Return the last known world space end point of the beam
        /// </summary>
        /// <returns></returns>
        public Vector3 GetBeamEndPoint()
        {
            if (isInitialised && numLinePositions > 0)
            {
                return lineRenderer.GetPosition(numLinePositions - 1);
            }
            else { return Vector3.zero; }
        }

        /// <summary>
        /// Return the default colour of the StickyXRInteractor Target
        /// </summary>
        /// <returns></returns>
        public Color GetDefaultTargetColour()
        {
            return defaultTargetColour;
        }

        /// <summary>
        /// Get the world space hand palm position
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
        /// Get the transform that represents the position and rotation of the palm of the hand
        /// </summary>
        /// <returns></returns>
        public Transform GetHandPalmTransform()
        {
            return palmTransform;
        }

        /// <summary>
        /// Get the InteractivePointMode of the Sticky XR Interactor. e.g., Beam or DisplayTarget
        /// </summary>
        /// <returns></returns>
        public InteractivePointMode GetInteractivePointMode()
        {
            return interactivePointMode;
        }

        /// <summary>
        /// Get the InteractorType of the Sticky XR Interactor. e.g., left or right hand
        /// </summary>
        /// <returns></returns>
        public InteractorType GetInteractorType()
        {
            return interactorType;
        }

        /// <summary>
        /// Get the interactive-enabled object that the character is currently looking toward or pointing
        /// at with the beam.
        /// </summary>
        /// <returns></returns>
        public StickyInteractive GetLookingAtInteractive()
        {
            return lookingAtInteractive;
        }

        /// <summary>
        /// Get the unique ID of the interactive-enabled object that the character is currently looking toward
        /// or pointing at with the beam. If none, 0 will be returned.
        /// </summary>
        /// <returns></returns>
        public int GetLookingAtInteractiveId()
        {
            return lookingAtInteractiveId;
        }

        /// <summary>
        /// Return the sprite that is used to show where the hand is pointing when Interactive Point Mode is Target.
        /// </summary>
        /// <returns></returns>
        public Sprite GetTargetSprite()
        {
            return interactiveTargetSprite;
        }

        /// <summary>
        /// Grab the interactive-enabled object currently being looked at (if any).
        /// Use the primary or secondary hand hold position on the object.
        /// Grabbed objects become unselected if they were previously selected.
        /// </summary>
        /// <param name="isSecondaryHandHold"></param>
        public void GrabLookedAtInteractive(bool isSecondaryHandHold)
        {
            // Can we grab an interactive-enabled object, and are we currently looking at one?
            if (isInitialised && lookingAtInteractiveId != StickyInteractive.NoID)
            {
                // Check that it hasn't been destroyed
                if (lookingAtInteractive == null)
                {
                    stickyControlModule.RemoveSelectedInteractiveID(lookingAtInteractiveId);

                    // Cannot find it so assume character is no longer looking at it
                    lookingAtInteractiveId = StickyInteractive.NoID;
                    lookingAtHitNormal = Vector3.zero;

                    isTouching = false;
                }
                else if (!lookingAtInteractive.IsGrabbable)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("StickyXRInteractor.GrabLookedAtInteractive " + stickyControlModule.name + " cannot grab " + lookingAtInteractive.name + " because it is not Grabbable");
                    #endif
                }
                else if (!isPointToGrab)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("StickyXRInteractor.GrabLookedAtInteractive " + stickyControlModule.name + " cannot grab " + lookingAtInteractive.name + " from a distance (Point to Grab is not enabled).");
                    #endif
                }
                else if (palmTransform == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("StickyXRInteractor.GrabLookedAtInteractive " + stickyControlModule.name + " cannot grab " + lookingAtInteractive.name + " because the palm transform is not set on " + name);
                    #endif
                }
                else
                {
                    // If the interactive-enable object is already selected, remove it from the list
                    stickyControlModule.RemoveSelectedInteractiveID(lookingAtInteractiveId);

                    HoldInteractive(lookingAtInteractive, palmTransform, isSecondaryHandHold);

                    // If the grab was successful, turn off Interactive
                    if (heldInteractiveId != StickyInteractive.NoID)
                    {
                        EnableOrDisableInteractive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Grab an interactive-enabled object in the palm of the hand.
        /// Use the primary or secondary hand hold on the object.
        /// Grabbed objects become unselected if they were previously selected.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        /// <param name="isSecondaryHandHold"></param>
        public void GrabInteractive(StickyInteractive stickyInteractive, bool isSecondaryHandHold)
        {
            if (isInitialised && stickyInteractive != null)
            {
                if (stickyInteractive.IsGrabbable)
                {
                    if (palmTransform != null)
                    {
                        // If the interactive-enable object is already selected, remove it from the list
                        stickyControlModule.RemoveSelectedInteractiveID(stickyInteractive.StickyInteractiveID);

                        HoldInteractive(stickyInteractive, palmTransform, isSecondaryHandHold);

                        // If the grab was successful, turn off Interactive
                        if (heldInteractiveId != StickyInteractive.NoID)
                        {
                            EnableOrDisableInteractive(false);
                        }
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("StickyXRInteractor.GrabInteractive " + stickyControlModule.name + " cannot grab " + stickyInteractive.name + " because the palm transform is not set on " + name); }
                    #endif
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("StickyXRInteractor.GrabInteractive " + name + " cannot grab " + stickyInteractive.name + " because it is not Grabbable"); }
                #endif
            }
        }

        /// <summary>
        /// Initialise the StickyXRInteractor. Has no effect if called multiple times.
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            #if UNITY_EDITOR
            if (stickyControlModule == null)
            {
                Debug.LogWarning("StickyXRInteractor.Initialise - cannot find the StickyControlModule on " + name + ". Did you add it to the slot provided in the editor?");
            }
            #endif

            lookModeInt = (int)lookMode;
            interactorTypeInt = (int)interactorType;

            if (lineRenderer == null) { lineRenderer = GetComponentInChildren<LineRenderer>(); }

            if (targetCanvas == null)
            {
                targetCanvas = GetComponentInChildren<Canvas>();
            }

            // The line renderer pointer is the default so must always be present
            if (lineRenderer != null)
            {
                lineRenderer.startWidth = beamStartWidth;
                lineRenderer.endWidth = beamStartWidth;
                lineRenderer.alignment = LineAlignment.View;
                lineRenderer.startColor = Color.white;
                lineRenderer.endColor = Color.white;

                // Our calculations assume world-space positions
                lineRenderer.useWorldSpace = true;

                // Start disabled
                lineRenderer.enabled = false;

                ReinitialisePositions();

                ReinitialiseTeleporting();

                if (stickyControlModule != null)
                {
                    if (interactorType == InteractorType.LeftHand)
                    {
                        stickyControlModule.leftHandXRInteractor = this;
                    }
                    else
                    {
                        stickyControlModule.rightHandXRInteractor = this;
                    }
                }

                ReinitialiseInteractiveTarget();

                isInitialised = numLinePositions > 1 && stickyControlModule != null;

                // If look mode is interactive, check the pointer mode by attempting
                // to (re)set it to the current value.
                SetInteractivePointMode(interactivePointMode);
            }
        }

        /// <summary>
        /// Move the beam or pointer in the scene at runtime. Typically, this will be called automatically each frame.
        /// </summary>
        public void MovePointer()
        {
            if (isInitialised && (isBeamEnabled || isTargetEnabled))
            {
                // The beam transform will move with the hand. Keep the first LineRenderer (local?) position at 0,0,0

                Vector3 currentPos = transform.position;
                Vector3 currentDir = transform.forward;

                Vector3 _startPosition = currentPos;
                Vector3 _endPosition = currentPos + (currentDir * maxDistance);

                bool _isHitAnything = false;

                #region Interactive Beam or Target
                if (lookModeInt == LookModeInteractiveInt)
                {
                    /// Uses S3D Interactive layer mask from Look tab
                    if (Physics.Linecast(currentPos, _endPosition, out raycastHit, stickyControlModule.lookInteractiveLayerMask, QueryTriggerInteraction.Ignore))
                    {
                        // Adjust the end position to the hit point
                        _endPosition = raycastHit.point;

                        _isHitAnything = true;

                        // Is what we're looking at an iteractive-enabled object?
                        StickyInteractive stickyInteractive = raycastHit.collider.gameObject.GetComponent<StickyInteractive>();

                        // If the colliders are not on the parent, then maybe it has a rigidbody and we can find it that way.
                        if (stickyInteractive == null)
                        {
                            Rigidbody hitRbody = raycastHit.rigidbody;

                            if (hitRbody == null || !hitRbody.TryGetComponent(out stickyInteractive))
                            {
                                // No rigidbody on parent so check if there is a StickyInteractChild component on the collider
                                StickyInteractiveChild stickyInteractiveChild = null;
                                if (raycastHit.collider.gameObject.TryGetComponent(out stickyInteractiveChild))
                                {
                                    stickyInteractive = stickyInteractiveChild.stickyInteractive;
                                }
                            }
                        }

                        if (stickyInteractive != null)
                        {
                            lookingAtHitNormal = raycastHit.normal;

                            // Is it a different object than the one we were looking at?
                            if (lookingAtInteractiveId != stickyInteractive.StickyInteractiveID)
                            {
                                StickyInteractive oldLookAt = lookingAtInteractive;

                                if (stickyID == 0) { stickyID = stickyControlModule.StickyID; }

                                // Now looking at a different object, so notify the old one.
                                if (oldLookAt != null)
                                {
                                    if (isTouching && oldLookAt.IsTouchable)
                                    {
                                        StopTouchingLookingAtInteractive();
                                    }

                                    // Call all onHoverExit methods (or set properties etc) and pass in the previously set StickyInteractiveID and the ID of this character.
                                    if (oldLookAt.onHoverExit != null) { oldLookAt.onHoverExit.Invoke(lookingAtInteractiveId, stickyID); }
                                }

                                // Change to the new Look At object
                                lookingAtInteractiveId = stickyInteractive.StickyInteractiveID;
                                lookingAtInteractive = stickyInteractive;

                                // Call all onHoverEnter methods (or set properties etc) and pass in the hitpoint, hitnormal, StickyInteractiveID and the ID of this character.
                                if (stickyInteractive.onHoverEnter != null) { stickyInteractive.onHoverEnter.Invoke(raycastHit.point, raycastHit.normal, lookingAtInteractiveId, stickyID); }

                                // Change the visuals on the LineRenderer
                                if (!isActiveColour)
                                {
                                    if (isTargetEnabled)
                                    {
                                        targetReticleImg.color = activeTargetColour;
                                    }
                                    else
                                    {
                                        lineRenderer.colorGradient = activeBeamGradient;
                                    }
                                    isActiveColour = true;
                                }

                                // The StickyInteractive being looked at has been changed, so call the callback if it has been set
                                //if (callbackOnChangeLookAtInteractive != null)
                                //{
                                //    callbackOnChangeLookAtInteractive(this, oldLookAt, lookingAtInteractive);
                                //}
                            }
                        }
                        else
                        {
                            StopLookAtInteractive();
                        }
                    }
                    // The raycast failed to hit anything
                    else
                    {
                        // The raycast failed to hit anything when previously was looking at an interactive-enabled object
                        if (lookingAtInteractiveId != StickyInteractive.NoID)
                        {
                            StopLookAtInteractive();
                        }
                    }

                    // Move the beam end (assumes world space)
                    lineRenderer.SetPosition(0, _startPosition);
                    lineRenderer.SetPosition(1, _endPosition);

                    // If required, move the Target canvas
                    if (isTargetEnabled && targetCanvasTfrm != null)
                    {
                        if (_isHitAnything)
                        {
                            // If we hit anything, move the Target sprite toward the hand to avoid clipping into the object
                            targetCanvasTfrm.position = _startPosition + ((_endPosition - _startPosition) * (1f - targetOffsetDistanceN));
                        }
                        else
                        {
                            targetCanvasTfrm.position = _endPosition;
                        }
                    }

                    // Check for hitting UI elements
                    pointerEventData = new S3DPointerEventData(eventSystem);
                    // Prob not what we want but can test to see if it hits a UI target
                    pointerEventData.position = targetCanvas.worldCamera.WorldToScreenPoint(_endPosition);
                    pointerEventData.ray = new Ray(_startPosition, S3DMath.Normalise(_startPosition, _endPosition));
                    raycastResultList.Clear();

                    // Perform raycast to find intersections with UI
                    eventSystem.RaycastAll(pointerEventData, raycastResultList);

                    if (raycastResultList.Count > 0)
                    {
                        RaycastResult rayCastResult = raycastResultList[0];

                        GameObject hitGameObject = rayCastResult.gameObject;

                        UnityEngine.UI.Button button;

                        // We could have hit the background UI.Image of a button
                        if (hitGameObject.TryGetComponent(out button))
                        {
                            eventSystem.SetSelectedGameObject(hitGameObject, pointerEventData);
                        }
                        // We could have hit the UI.Text of a button. This would be the case if the Text was
                        // a Raycast Target but there was no enabled image on the parent button RectTransform
                        else if (hitGameObject.transform.parent.TryGetComponent(out button))
                        {
                            eventSystem.SetSelectedGameObject(hitGameObject.transform.parent.gameObject, pointerEventData);
                        }
                        else
                        {
                            eventSystem.SetSelectedGameObject(hitGameObject, pointerEventData);
                        }

                        if (button != null)
                        {
                            #if SSC_UIS && SCSM_XR
                            // NOTE: This doesn't seem 100% reliable and needs more testing
                            if ((leftClickInputAction != null && leftClickInputAction.IsPressed()) || (rightClickInputAction != null && rightClickInputAction.IsPressed()))
                            {
                                //Debug.Log("[DEBUG] Button pressed on " + button.name + " T:" + Time.time);
                                button.onClick.Invoke();
                            }
                            #endif
                        }

                        //Debug.Log("[DEBUG] Hit UI " + hitGameObject.name + " (" + hitGameObject.transform.parent.name + ") count: " + raycastResultList.Count + " T:" + Time.time);

                        
                    }
                }
                #endregion

                #region Teleport Beam
                else
                {
                    // Move the beam end (assumes world space)
                    lineRenderer.SetPosition(0, _startPosition);

                    Vector3 _pos = Vector3.zero;
                    Vector3 _position1 = Vector3.zero;
                    Vector3 _downDir = -stickyControlModule.GetCurrentUp;
                    float _segmentLength = maxDistance * (1f - beamStartCurve) / (numCurvedPositions - 2);
                    float curveDropLength = 0.8f / (numCurvedPositions - 2);

                    isTeleportCurrentLocationValid = false;

                    for (int i = 1; i < numCurvedPositions; i++)
                    {
                        if (i == 1)
                        {
                            // The first x% of the teleporter look distance is a straight line
                            _pos = currentPos + (currentDir * (maxDistance * beamStartCurve));
                            _position1 = _pos;
                        }
                        else
                        {
                            // Gradually curve the line downward after x% of the max look distance of the teleporter
                            // This should be cheaper than calculating a bezier curve
                            _pos += (currentDir + (curveDropLength * (i - 1) * _downDir)).normalized * _segmentLength;
                        }

                        // Uses S3D collision layer mask from Collide tab
                        if (Physics.Linecast(currentPos, _pos, out raycastHit, stickyControlModule.collisionLayerMask, QueryTriggerInteraction.Ignore))
                        {
                            // Adjust the end position to the hit point
                            _pos = raycastHit.point;

                            isTeleportCurrentLocationValid = true;

                            lookingAtHitNormal = raycastHit.normal;

                            lineRenderer.SetPosition(i, _pos);

                            // Set any remaining segments to end at the same hit point
                            for (int j = i + 1; j < numCurvedPositions; j++)
                            {
                                lineRenderer.SetPosition(j, _pos);
                            }

                            break;
                        }
                        else
                        {
                            // Didn't hit anything
                            lineRenderer.SetPosition(i, _pos);
                        }
                    }

                    // Update the visuals on the LineRenderer if required
                    if (isTeleportPreviousLocationValid != isTeleportCurrentLocationValid)
                    {
                        if (!isTeleportCurrentLocationValid)
                        {
                            lookingAtHitNormal = Vector3.zero;
                        }

                        // Only update the colour if it needs to change
                        if (isActiveColour != isTeleportCurrentLocationValid)
                        {
                            lineRenderer.colorGradient = isTeleportCurrentLocationValid ? activeBeamGradient : defaultBeamGradient;
                            isActiveColour = isTeleportCurrentLocationValid;
                        }
                    }

                    isTeleportPreviousLocationValid = isTeleportCurrentLocationValid;
                }
                #endregion
            }
        }

        /// <summary>
        /// Reinitialise the target reticle used with Interactive Point Mode of Target.
        /// </summary>
        public void ReinitialiseInteractiveTarget()
        {
            if (targetCanvas != null)
            {
                targetCanvasTfrm = targetCanvas.transform;
                targetCanvasPanel = targetCanvas.gameObject.GetComponent<RectTransform>();

                // Unity VR does not support overlay and we don't want to use Camera mode
                if (targetCanvas.renderMode != RenderMode.WorldSpace)
                {
                    targetCanvas.renderMode = RenderMode.WorldSpace;
                }

                if (stickyControlModule != null)
                {
                    targetCanvas.worldCamera = stickyControlModule.lookFirstPersonCamera1;
                }

                // TESTING - UI Interaction Initialising
                eventSystem = UnityEngine.EventSystems.EventSystem.current;
                raycastResultList = new List<UnityEngine.EventSystems.RaycastResult>(10);

                #if SSC_UIS && SCSM_XR
                if (eventSystem.TryGetComponent(out uiInputModule))
                {
                    leftClickInputAction = uiInputModule.leftClick != null ? uiInputModule.leftClick.action : null;
                    rightClickInputAction = uiInputModule.rightClick != null ? uiInputModule.rightClick.action : null;
                }
                #endif

                //if (targetCanvasTfrm.TryGetComponent(out graphicRaycaster))
                //{
                //    //Debug.Log("[DEBUG] graphicRaycaster ignoreReverse " + graphicRaycaster.ignoreReversedGraphics + " for " + name);
                //}

                targetPanel = GetTargetPanel();

                if (targetPanel != null)
                {
                    targetBackgroundImg = targetPanel.GetComponent<UnityEngine.UI.Image>();

                    if (targetBackgroundImg != null) { targetBackgroundImg.raycastTarget = false; }

                    targetReticlePanel = S3DUtils.GetChildTransform(targetPanel.transform, reticlePanelName, this.name);

                    if (targetReticlePanel != null)
                    {
                        targetReticleImg = targetReticlePanel.GetComponent<UnityEngine.UI.Image>();

                        if (targetReticleImg != null) { targetReticleImg.raycastTarget = false; }
                    }

                    // The Target is off by default
                    targetPanel.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Reinitialise teleporting. Call this each time you change the TeleportReticle prefab at runtime.
        /// </summary>
        public void ReinitialiseTeleporting()
        {
            // If an instance already exists, destroy it.
            if (teleportPrefabInstance != null)
            {
                Destroy(teleportPrefabInstance);
                teleportPrefabInstance = null;
            }

            if (teleportReticle != null)
            {
                // Instantiate the prefab and immediately disable it
                teleportPrefabInstance = Instantiate(teleportReticle, Vector3.zero, Quaternion.identity);

                if (teleportPrefabInstance != null)
                {
                    teleportPrefabInstance.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Attempt to select the interactive-enabled and store it in characters engage store.
        /// </summary>
        public void SelectLookingAtInteractive()
        {
            if (isInitialised && isBeamEnabled && lookingAtInteractiveId != StickyInteractive.NoID)
            {
                // Check that it hasn't been destroyed
                if (lookingAtInteractive == null)
                {
                    stickyControlModule.RemoveSelectedInteractiveID(lookingAtInteractiveId);

                    // Cannot find it so assume StickyXRInteractor is no longer looking at it
                    lookingAtInteractiveId = StickyInteractive.NoID;
                    lookingAtHitNormal = Vector3.zero;
                    isTouching = false;
                }
                else if (lookingAtInteractive.IsSelectable)
                {
                    stickyControlModule.SelectInteractive(lookingAtInteractive);
                }
            }
        }

        /// <summary>
        /// If touching an interactive-enabled object that the handing is pointing to,
        /// stop touching it.
        /// </summary>
        public void StopTouchingLookingAtInteractive()
        {
            if (isTouching)
            {
                isTouching = false;
                touchingInteractiveId = 0;
                touchingInteractive = null;

                if (lookingAtInteractive != null)
                {
                    if (stickyID == 0) { stickyID = stickyControlModule.StickyID; }

                    lookingAtInteractive.StopTouchingObject(stickyID);
                }
            }
        }

        /// <summary>
        /// When teleporting is enabled, select the current teleport location
        /// and attempt to teleport the character to that location.
        /// NOTE: currently does not check if any S3D character is at that location.
        /// TODO - offset for slopes
        /// </summary>
        public void SelectTeleportLocation()
        {
            if (isTeleportEnabled && isTeleportCurrentLocationValid)
            {
                Vector3 teleportFeetPosition = GetBeamEndPoint();

                isTeleportCurrentLocationValid = false;

                if (stickyControlModule.GetCurrentUp != lookingAtHitNormal)
                {
                    // Check for obstacles and raise above a slope (pretty hacking and may not work on steep slopes)
                    // Should use better calc like when climbing steps
                    teleportFeetPosition += lookingAtHitNormal * (stickyControlModule.radius * 0.5f);
                }
                else
                {
                    // Teleport slightly above the ground
                    teleportFeetPosition += lookingAtHitNormal * 0.01f;
                }

                // Can the character fit in the teleported location?
                if (!isTeleportObstacleCheck || !stickyControlModule.IsObstacle(teleportFeetPosition, stickyControlModule.GetCurrentUp))
                {
                    stickyControlModule.TelePort(teleportFeetPosition, stickyControlModule.GetCurrentRotation, true);

                    if (isAutoDisableTeleporter) { EnableOrDisableTeleport(false); }
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("StickyXRInteractor.SelectTeleportLocation - " + stickyControlModule.name + " cannot fit in the selected location. T: " + Time.time);
                }
                #endif
            }
        }

        /// <summary>
        /// Set the active colour gradient of the StickyXRInteractor beam
        /// </summary>
        /// <param name="newGradient"></param>
        public void SetActiveBeamGradient(Gradient newGradient)
        {
            activeBeamGradient = newGradient;
        }

        /// <summary>
        /// Set the active colour of the StickyXRInteractor Target
        /// </summary>
        /// <param name="newColour"></param>
        public void SetActiveTargetColour(Color newColour)
        {
            activeTargetColour = newColour;
        }

        /// <summary>
        /// Set the default colour gradient of the StickyXRInteractor beam
        /// </summary>
        /// <param name="newGradient"></param>
        public void SetDefaultBeamGradient(Gradient newGradient)
        {
            defaultBeamGradient = newGradient;
        }

        /// <summary>
        /// Set the default colour of the StickyXRInteractor Target
        /// </summary>
        /// <param name="newColour"></param>
        public void SetDefaultTargetColour(Color newColour)
        {
            defaultTargetColour = newColour;
        }

        /// <summary>
        /// Set the transform that represents hand palm position and rotation.
        /// The StickyControlModule must be set first.
        /// Returns true if set successfully.
        /// </summary>
        /// <param name="newPalmTransform"></param>
        public bool SetHandPalmTransform(Transform newPalmTransform)
        {
            if (newPalmTransform != null)
            {
                if (stickyControlModule == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR StickyXRInteractor.SetPalmTransform - cannot set palm without the StickyControlModule being set first");
                    #endif
                    return false;
                }
                else if (!newPalmTransform.IsChildOf(stickyControlModule.transform))
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR StickyXRInteractor.SetPalmTransform - the palm must be a child or part of the character prefab");
                    #endif
                    return false;
                }
                else
                {
                    palmTransform = newPalmTransform;
                    return true;
                }
            }
            else
            {
                palmTransform = null;
                return false;
            }
        }

        /// <summary>
        /// Set the InteractivePointMode of the Sticky XR Interactor. e.g., Beam or DisplayTarget
        /// </summary>
        /// <param name="newInteractivePointMode"></param>
        public void SetInteractivePointMode(InteractivePointMode newInteractivePointMode)
        {
            if (isInitialised && lookModeInt == LookModeInteractiveInt)
            {
                // For DisplayTarget, make sure we have a HUD set, else fallback to Beam
                if (newInteractivePointMode == InteractivePointMode.Target && stickyControlModule == null)
                {
                    interactivePointMode = InteractivePointMode.Beam;
                }
                else
                {
                    interactivePointMode = newInteractivePointMode;
                }
            }
            else
            {
                interactivePointMode = newInteractivePointMode;
            }

            interactivePointModeInt = (int)interactivePointMode;
        }

        /// <summary>
        /// Set the InteractorType of the Sticky XR Interactor. e.g. left or right hand
        /// </summary>
        /// <param name="newInteractorType"></param>
        public void SetInteractorType(InteractorType newInteractorType)
        {
            interactorType = newInteractorType;

            interactorTypeInt = (int)interactorType;
        }

        /// <summary>
        /// Set a new LookMode for the StickyXRInteractor
        /// </summary>
        /// <param name="newLookMode"></param>
        public void SetLookMode(LookMode newLookMode)
        {
            int prevLookModeInt = (int)lookMode;

            lookMode = newLookMode;
            lookModeInt = (int)lookMode;

            if (isInitialised)
            {
                // If previously touching an object, check if we need to stop touching it.
                if (prevLookModeInt != lookModeInt && prevLookModeInt == LookModeInteractiveInt && isTouching)
                {
                    StopTouchingLookingAtInteractive();
                }

                if (lookModeInt == LookModeTeleportInt)
                {
                    if (numLinePositions != numCurvedPositions) { ReinitialisePositions(); }
                }
                else if (lookModeInt == LookModeInteractiveInt)
                {
                    if (numLinePositions != 2) { ReinitialisePositions(); };
                }
            }
        }

        /// <summary>
        /// Sets the sprite that is used to show where the hand is pointing when Interactive Point Mode is Target.
        /// </summary>
        /// <param name="newSprite"></param>
        public void SetTargetSprite(Sprite newSprite)
        {
            interactiveTargetSprite = newSprite;

            if (targetReticleImg != null)
            {
                targetReticleImg.sprite = newSprite;
            }
        }

        /// <summary>
        /// Something else snatches an interactive object that is currently being held in the hand.
        /// This does not invoke the Drop action or events. If the snatch is successful, return true.
        /// </summary>
        public bool SnatchHeldInteractive(StickyInteractive stickyInteractive)
        {
            bool isSnatched = false;

            // Get the unique ID of the interactive-enabled object we want to snatch 
            int interactiveID = stickyInteractive.StickyInteractiveID;

            // The object matches the one to be snatched, so stop holding this object
            if (interactiveID == heldInteractiveId)
            {

                // This ensures that when the object is grabbed by the "new" hand, the correct original
                // parent is remembered.
                if (stickyInteractive.isParentOnGrab && stickyInteractive.isReparentOnDrop)
                {
                    stickyInteractive.RestoreParent();
                }
                else
                {
                    stickyInteractive.transform.SetParent(null);
                }

                heldInteractiveId = StickyInteractive.NoID;
                heldInteractive = null;
                isHeldSecondaryHandHold = false;
                isSnatched = true;
            }

            return isSnatched;
        }

        /// <summary>
        /// Stop looking at an interactive-enabled object. It does not prevent the
        /// StickyXRInteractor from looking at it again.
        /// </summary>
        public void StopLookAtInteractive()
        {
            // Was a previous StickyInteractive component being observed?
            if (lookingAtInteractiveId != StickyInteractive.NoID)
            {
                if (isTouching) { StopTouchingLookingAtInteractive(); }

                // Call all onHoverExit methods (or set properties etc) and pass in the previously set StickyInteractiveID and the ID of this character.
                if (lookingAtInteractive != null && lookingAtInteractive.onHoverExit != null)
                {
                    if (stickyID == 0) { stickyID = stickyControlModule.StickyID; }

                    lookingAtInteractive.onHoverExit.Invoke(lookingAtInteractiveId, stickyID);
                }

                // The StickyXRInteractor has stopped looking at a StickyInteractive so call the callback if it has been set
                //if (callbackOnChangeLookAtInteractive != null)
                //{
                //    callbackOnChangeLookAtInteractive(this, lookingAtInteractive, null);
                //}

                // Only up date the colour if it needs to change
                if (isInitialised && isActiveColour)
                {
                    if (isTargetEnabled)
                    {
                        targetReticleImg.color = defaultTargetColour;
                    }
                    else
                    {
                        lineRenderer.colorGradient = defaultBeamGradient;
                    }
                    isActiveColour = false;
                }
            }

            lookingAtHitNormal = Vector3.zero;
            lookingAtInteractive = null;
            lookingAtInteractiveId = StickyInteractive.NoID;
        }

        /// <summary>
        /// Stop touching an interactive-enabled object. Typically this is automatically
        /// called by a StickyXRTouch component attached to the hand's trigger collider.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        public void StopTouchingInteractive (StickyInteractive stickyInteractive)
        {
            // Verify this is the object being touched (not sure when this wouldn't be true...)
            if (stickyInteractive.StickyInteractiveID == touchingInteractiveId)
            {
                isTouching = false;
                touchingInteractive = null;
                touchingInteractiveId = 0;

                if (stickyID == 0) { stickyID = stickyControlModule.StickyID; }

                stickyInteractive.StopTouchingObject(stickyID);
            }
        }

        /// <summary>
        /// Attempt to toggle the StickyXRInteractor beam on or off
        /// </summary>
        public void ToggleBeamOn()
        {
            if (isInitialised)
            {
                if (lookModeInt == LookModeInteractiveInt)
                {
                    ToggleInteractiveOn();
                }
                else if (lookModeInt == LookModeTeleportInt)
                {
                    ToggleTeleportOn();
                }
                else
                {
                    // Fallback
                    EnableOrDisableBeam(!isBeamEnabled);
                }
            }
        }

        /// <summary>
        /// Attempt to toggle the StickyXRInteractive interactive mode on or off
        /// </summary>
        public void ToggleInteractiveOn()
        {
            EnableOrDisableInteractive(!isInteractiveEnabled);
        }

        /// <summary>
        /// If an activable object is held, activate or deactivate it.
        /// If an activable object is being looked at, activate or deactivate it.
        /// If neither of the above, toggle Interactive on or off.
        /// </summary>
        public void ToggleInteractiveOrActivateOn()
        {
            // If an activable object is held, activate or deactivate it.
            if (CheckHeldInteractive() && heldInteractive.IsActivable)
            {
                ActivateInteractive(heldInteractive, !heldInteractive.IsActivated);
            }
            // If an activable object is not grabbable, and is being looked at, activate or deactivate it.
            else if (CheckLookingAtInteractive() && lookingAtInteractive.IsActivable && !lookingAtInteractive.IsGrabbable)
            {
                ActivateInteractive(lookingAtInteractive, !lookingAtInteractive.IsActivated);
            }
            // Turn the Beam or Target on or off
            else
            {
                EnableOrDisableInteractive(!isInteractiveEnabled);
            }
        }

        /// <summary>
        /// Attempt to select or unselect an interactive-enabled item in the scene that is being looked at
        /// </summary>
        public void ToggleSelectLookedAtInteractive()
        {
            if (CheckLookingAtInteractive())
            {
                stickyControlModule.ToggleSelectInteractive(lookingAtInteractive);
            }
        }

        /// <summary>
        /// Attempt to toggle the StickyXRInteractive teleporting on or off
        /// </summary>
        public void ToggleTeleportOn()
        {
            EnableOrDisableTeleport(!isTeleportEnabled);
        }

        /// <summary>
        /// Indicate that this is hand is touching a touchable interactive-enabled object. Typically, this
        /// is called automatically by a StickyXRTouch component attached to the hand's trigger collider.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        /// <param name="hitPoint"></param>
        /// <param name="hitNormal"></param>
        public void TouchInteractive (StickyInteractive stickyInteractive, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (stickyInteractive != null && stickyInteractive.IsTouchable)
            {
                isTouching = true;

                touchingInteractive = stickyInteractive;
                touchingInteractiveId = stickyInteractive.StickyInteractiveID;

                if (stickyID == 0) { stickyID = stickyControlModule.StickyID; }

                stickyInteractive.TouchObject(hitPoint, hitNormal, stickyID);
            }
        }

        /// <summary>
        /// Attempt to touch an interactive-enabled object that is being looked at or pointed at with the beam.
        /// This requires the object to be Touchable, and Point to Touch be enabled on this component.
        /// </summary>
        public void TouchLookingAtInteractive()
        {
            if (isInitialised && isBeamEnabled && lookingAtInteractiveId != StickyInteractive.NoID)
            {
                // Check that it hasn't been destroyed
                if (lookingAtInteractive == null)
                {
                    // Cannot find it so assume StickyXRInteractor is no longer looking at it
                    lookingAtInteractiveId = StickyInteractive.NoID;
                    lookingAtHitNormal = Vector3.zero;
                    isTouching = false;
                }
                else if (isPointToTouch && lookingAtInteractive.IsTouchable)
                {
                    isTouching = true;
                    if (stickyID == 0) { stickyID = stickyControlModule.StickyID; }
                    lookingAtInteractive.TouchObject(GetBeamEndPoint(), lookingAtHitNormal, stickyID);
                }
            }
        }

        /// <summary>
        /// Attempt to unselect or deselect an interactive-enabled item that is currently being looked at by this character
        /// </summary>
        public void UnselectLookedAtInteractive()
        {
            if (isInitialised && lookingAtInteractiveId != StickyInteractive.NoID)
            {
                // Check that it hasn't been destroyed
                if (lookingAtInteractive == null)
                {
                    stickyControlModule.RemoveSelectedInteractiveID(lookingAtInteractiveId);

                    // Cannot find it so assume character is no longer looking at it
                    lookingAtInteractiveId = StickyInteractive.NoID;
                }
                else if (stickyControlModule.IsInteractiveIDSelected(lookingAtInteractiveId))
                {
                    stickyControlModule.RemoveSelectedInteractiveID(lookingAtInteractiveId);
                    if (stickyID == 0) { stickyID = stickyControlModule.StickyID; }
                    lookingAtInteractive.UnselectObject(stickyID);
                }
            }
        }

        #endregion
    }
}