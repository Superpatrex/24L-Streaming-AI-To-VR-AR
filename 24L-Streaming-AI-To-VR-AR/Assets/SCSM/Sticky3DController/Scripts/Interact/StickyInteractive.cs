using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This component, can be used to make non-poolable objects in your scene interactive-enabled.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Objects/Sticky Interactive")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyInteractive : MonoBehaviour, IStickyGravity
    {
        #region Public Variables - General
        /// <summary>
        /// If enabled, the Initialise() will be called as soon as Start() runs. This should be disabled if you are
        /// instantiating the component through code.
        /// </summary>
        public bool initialiseOnStart = false;

        /// <summary>
        /// When Is Activable and Is Grabbable is true, Activable will be considered before Grab when
        /// EngageLookAtInteractive is called. This can be helpful when you want to display a popup
        /// which could include an option to Grab.
        /// </summary>
        public bool isActivablePriorityOverGrab = false;

        /// <summary>
        /// When a grabbable and activable object is dropped while activated, it will be deactivated.
        /// </summary>
        public bool isDeactivateOnDrop = true;

        /// <summary>
        /// When grabbed, the interactive-enabled object will be held in the palm of the hand. Turn this off
        /// for objects like levers with a non-kinematic (dynamic) rigidbody which you do not want the character to carry.
        /// Currently, should always be on unless using VR and StickyXRInteractors.
        /// </summary>
        public bool isCarryInHand = true;

        /// <summary>
        /// When grabbed, the interactive-enabled object will be parented to the hand of S3D character
        /// </summary>
        public bool isParentOnGrab = false;

        /// <summary>
        /// Attempt to reparent the interactive-enabled object to the original parent gameobject when it was grabbed.
        /// </summary>
        public bool isReparentOnDrop = false;

        /// <summary>
        /// When grabbed, non-trigger colliders on this interactive-enabled object will be disabled.
        /// Unless this is a Weapon or Magazine, when dropped, they will NOT be enabled. To enable
        /// them add EnableNonTriggerColliders() to OnDropped.
        /// </summary>
        public bool isDisableRegularColOnGrab = true;

        /// <summary>
        /// When grabbed, trigger colliders on this interactive-enabled object will be disabled.
        /// Unless this is a Weapon or Magazine, when dropped, they will NOT be enabled. To enable
        /// them add EnableTriggerColliders() to OnDropped.
        /// </summary>
        public bool isDisableTriggerColOnGrab = false;

        /// <summary>
        /// When grabbed, if there is a rigidbody attached, remove it.
        /// </summary>
        public bool isRemoveRigidbodyOnGrab = true;

        #endregion

        #region Public Variables - Equippable

        /// <summary>
        /// The amount of time, in seconds, that a held object will delay being
        /// parented to the equip point. This could be used to allow time for
        /// say a weapon holster animation to run before the equip operation
        /// is completed.
        /// </summary>
        [Range(0f, 5f)] public float equipFromHeldDelay = 0f;

        /// <summary>
        /// The local space equip position offset from a character equip point
        /// </summary>
        public Vector3 equipOffset = Vector3.zero;

        /// <summary>
        /// The local space rotation, stored in degrees, around a character equip point
        /// </summary>
        public Vector3 equipRotation = Vector3.zero;

        /// <summary>
        /// The colour of the selected Equip point in the scene view
        /// Non-selected points are slightly transparent
        /// </summary>
        public Color equipPointGizmoColour = new Color(1f, 0.92f, 0.016f, 1.0f);

        #endregion

        #region Public Variables - Handhold

        /// <summary>
        /// The first or primary local space hand hold offset
        /// </summary>
        public Vector3 handHold1Offset = Vector3.zero;

        /// <summary>
        /// The second or secondary local space hand hold offset
        /// </summary>
        public Vector3 handHold2Offset = Vector3.zero;

        /// <summary>
        /// The first or primary local space hand hold rotation stored in degrees
        /// </summary>
        public Vector3 handHold1Rotation = Vector3.zero;

        /// <summary>
        /// The second or secondary local space hand hold rotation stored in degrees
        /// </summary>
        public Vector3 handHold2Rotation = Vector3.zero;

        /// <summary>
        /// For the first or primary hand hold, flip the rotation for left hand when using Sticky XR Interactor.
        /// This can be useful when the hand hold position is on a symmetrical handle like a bat, racket, or spear.
        /// </summary>
        public bool handHold1FlipForLeftHand = false;

        /// <summary>
        /// For the second or secondary hand hold, flip the rotation for left hand when using Sticky XR Interactor.
        /// This can be useful when the hand hold position is on a symmetrical handle like a bat, racket, or spear.
        /// </summary>
        public bool handHold2FlipForLeftHand = false;

        #endregion

        #region Public Variables - Readable

        /// <summary>
        /// When the readable object is released (dropped) it will automatically
        /// return to the centre using the spring mechanism.
        /// </summary>
        public bool readableAutoRecentre = true;

        /// <summary>
        /// The normalised amount the pivot can move before values are updated.
        /// </summary>
        [Range(0f, 0.1f)] public float readableDeadZone = 0.05f;

        /// <summary>
        /// Invert the x-axis value (left and right) from the readable pivot
        /// </summary>
        public bool readableInvertX = false;

        /// <summary>
        /// Invert the z-axis value (forward and backward) from the readable pivot
        /// </summary>
        public bool readableInvertZ = false;

        /// <summary>
        /// The maximum angle, in degrees, the pivot can rotate left.
        /// </summary>
        [Range(0f, 90f)] public float readableMinX = 90f;

        /// <summary>
        /// The maximum angle, in degrees, the pivot can rotate backward.
        /// </summary>
        [Range(0f, 90f)] public float readableMinZ = 90f;

        /// <summary>
        /// The maximum angle, in degrees, the pivot can rotate right.
        /// </summary>
        [Range(0f, 90f)] public float readableMaxX = 90f;

        /// <summary>
        /// The maximum angle, in degrees, the pivot can rotate forward.
        /// </summary>
        [Range(0f, 90f)] public float readableMaxZ = 90f;

        /// <summary>
        /// Speed to move towards target left-right value. Lower values make it less sensitive
        /// </summary>
        [Range(0.01f, 10f)] public float readableSensitivityX = 3f;

        /// <summary>
        /// Speed to move towards target forward-back value. Lower values make it less sensitive
        /// </summary>
        [Range(0.01f, 10f)] public float readableSensitivityZ = 3f;

        /// <summary>
        /// The the strength of the spring used to return the readable pivot back to centre
        /// </summary>
        [Range(0.1f, 100f)] public float readableSpringStrength = 50f;

        #endregion

        #region Public Variables - Sittable

        /// <summary>
        /// The local space relative offset that the character should
        /// aim for when getting ready to sit down.
        /// </summary>
        public Vector3 sitTargetOffset = Vector3.zero;

        #endregion

        #region Public Variables - Socketable

        /// <summary>
        /// The local space position offset which will snap to the StickySocket
        /// </summary>
        public Vector3 socketOffset = Vector3.zero;

        /// <summary>
        /// The local space rotation, stored in degrees, around a StickySocket point
        /// </summary>
        public Vector3 socketRotation = Vector3.zero;

        /// <summary>
        /// The colour of the selected Socket point in the scene view
        /// Non-selected points are slightly transparent
        /// </summary>
        public Color socketPointGizmoColour = new Color(0.435f, 0.67f, 0.717f, 1.0f);

        #endregion

        #region Public Variables - Events

        /// <summary>
        /// These are triggered by a S3D character when they activate this interactive object.
        /// </summary>
        public S3DInteractiveEvt4 onActivated = null;

        /// <summary>
        /// These are triggered by a S3D character when they deactivate this interactive object.
        /// </summary>
        public S3DInteractiveEvt2 onDeactivated = null;

        /// <summary>
        /// These are triggered by a S3D character when they grab this interactive object.
        /// </summary>
        public S3DInteractiveEvt1 onGrabbed = null;

        /// <summary>
        /// These are triggered by a S3D character when they drop this interactive object.
        /// </summary>
        public S3DInteractiveEvt2 onDropped = null;

        /// <summary>
        /// These are triggered by a S3D character when they start looking at this interactive object.
        /// </summary>
        public S3DInteractiveEvt1 onHoverEnter = null;

        /// <summary>
        /// These are triggered by a S3D character when they stop looking at this interactive object. 
        /// </summary>
        public S3DInteractiveEvt2 onHoverExit = null;

        /// <summary>
        /// These are triggered by a S3D character immediately after they equip this interactive object.
        /// </summary>
        public S3DInteractiveEvt8 onPostEquipped = null;

        /// <summary>
        /// These are triggered by a S3D character immediately after they grab this interactive object.
        /// </summary>
        public S3DInteractiveEvt1 onPostGrabbed = null;

        /// <summary>
        /// These are triggered immediately after the interactive-enabled object is initialised
        /// </summary>
        public S3DInteractiveEvt7 onPostInitialised = null;

        /// <summary>
        /// There are triggered by a S3D character immediately after they stash (in inventory) this interactive object.
        /// </summary>
        public S3DInteractiveEvt6 onPostStashed = null;

        /// <summary>
        /// These are triggered by a S3D character when the interactive object is selected in the scene.
        /// </summary>
        public S3DInteractiveEvt3 onSelected = null;

        /// <summary>
        /// These are triggered by a S3D character when the interactive object is unselected in the scene.
        /// </summary>
        public S3DInteractiveEvt2 onUnselected = null;

        /// <summary>
        /// These are triggered by a S3D character when the interactive object is first touched in the scene.
        /// </summary>
        public S3DInteractiveEvt1 onTouched = null;

        /// <summary>
        /// These are triggered by a S3D character when the interactive object is no longer being touched.
        /// </summary>
        public S3DInteractiveEvt2 onStoppedTouching = null;

        /// <summary>
        /// These are triggered when the value of the joint position changes if the interactive object is readable. 
        /// </summary>
        public S3DInteractiveEvt5 onReadableValueChanged = null;

        #endregion

        #region Public Variables - Editor

        /// <summary>
        /// [INTERNAL ONLY]
        /// Remember which tabs etc were shown in the editor
        /// </summary>
        [HideInInspector] public int selectedTabInt = 0;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [HideInInspector] public bool allowRepaint = false;

        #endregion

        #region Public Variables - Popup

        /// <summary>
        /// The default local space offset a StickyPopupModule appears relative to the interactive-enabled object.
        /// </summary>
        public Vector3 defaultPopupOffset = Vector3.zero;

        /// <summary>
        /// The relative up direction to apply default Popup Offset. This can be useful if you want a popup
        /// to appear "above" the interactive object. Only applies when Use Gravity is enabled.
        /// </summary>
        public bool isPopupRelativeUp = false;

        #endregion

        #region Public Properties - General

        /// <summary>
        /// Get the current world space position of the interactive-enabled object
        /// </summary>
        public Vector3 GetCurrentPosition { get { return !isMovementDataStale && isInitialised ? trfmPos : transform.position; } }

        /// <summary>
        /// Get the current world space rotation of the interactive-enabled object
        /// </summary>
        public Quaternion GetCurrentRotation { get { return !isMovementDataStale && isInitialised ? trfmRot : transform.rotation; } }

        /// <summary>
        /// Is the interactive object in the process of being equipped with delayed parenting
        /// to a character's Equip Point?
        /// </summary>
        public bool IsEquipFinaliseDelayed { get; internal set; }

        /// <summary>
        /// Is this interactive object currently Equipped by a Sticky3D character?
        /// </summary>
        public bool IsEquipped { get { return isEquipped; } internal set { isEquipped = value; } }

        /// <summary>
        /// Is the interactive object currently equipped on a Non-Player Sticky3D character?
        /// </summary>
        public bool IsEquippedByNPC { get { return isEquipped && stickyControlModule != null && stickyControlModule.IsNPC(); } }

        /// <summary>
        /// Is the interactive object currently held by a Sticky3D character?
        /// </summary>
        public bool IsHeld { get { return isHeld; } internal set { isHeld = value; } }

        /// <summary>
        /// Is the interactive object currently held by a Non-Player Sticky3D character?
        /// </summary>
        public bool IsHeldByNPC { get { return isHeld && stickyControlModule != null && stickyControlModule.IsNPC(); } }

        /// <summary>
        /// [READONLY]
        /// Has the module been initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// Is the interactive object currently attached to a socket?
        /// </summary>
        public bool IsSocketed { get { return isSocketed; } internal set { isSocketed = value; } }

        /// <summary>
        /// Is the interactive object currently in the Stash of a Sticky3D character?
        /// </summary>
        public bool IsStashed { get { return isStashed; } internal set { isStashed = value; } }

        /// <summary>
        /// [READONLY]
        /// Runtime ID of this Sticky Interactive component
        /// </summary>
        public int StickyInteractiveID { get { return guidHash; } }
        
        /// <summary>
        /// [READONLY]
        /// When Readable, when will return the current value of the jointed object
        /// Will be in the range -1.0 to 1.0.
        /// </summary>
        public Vector3 CurrentReadableValue { get { return currentReadValue; } }

        /// <summary>
        /// [READONLY]
        /// Can the item be activated?
        /// </summary>
        public bool IsActivable { get { return isActivable; } }

        /// <summary>
        /// [READONLY]
        /// Has the interactive-enabled object been activated?
        /// This should always be false when IsActivable is false.
        /// </summary>
        public bool IsActivated { get { return isActivated; } }

        /// <summary>
        /// [READONLY]
        /// Does the activated item act like a single on/off action?
        /// </summary>
        public bool IsAutoDeactivate { get { return isAutoDeactivate; } }

        /// <summary>
        /// {READONLY]
        /// Does the selected item act like a clickable button?
        /// </summary>
        public bool IsAutoUnselect { get { return isAutoUnselect; } }

        /// <summary>
        /// [READONLY]
        /// Can this item be equipped or attached to the body?
        /// </summary>
        public bool IsEquippable { get { return isEquippable; } }

        /// <summary>
        /// [READONLY]
        /// Can this item be grabbed or held?
        /// </summary>
        public bool IsGrabbable { get { return isGrabbable; } }

        /// <summary>
        /// [READONLY]
        /// Can values be read from this object? This is typically used
        /// for reading the position of a virtual lever or joystick.
        /// </summary>
        public bool IsReadable { get { return isReadable; } }

        /// <summary>
        /// [READONLY]
        /// Can this item be selected in the scene?
        /// </summary>
        public bool IsSelectable { get { return isSelectable; } }

        /// <summary>
        /// [READONLY]
        /// Is this item suitable for sitting on?
        /// </summary>
        public bool IsSittable { get { return isSittable; } }

        /// <summary>
        /// [READONLY]
        /// Can this item be attached to a StickySocket?
        /// </summary>
        public bool IsSocketable { get { return isSocketable; } }

        /// <summary>
        /// [READONLY]
        /// Can this item be stashed in the inventory of a character?
        /// </summary>
        public bool IsStashable { get { return isStashable; } }

        /// <summary>
        /// [READONLY]
        /// Can this item be touched by a character? Typically used with Hand IK.
        /// </summary>
        public bool IsTouchable { get { return isTouchable; } }

        /// <summary>
        /// [READONLY]
        /// Is the seat allocated, reserved, or taken?
        /// See also AllocateSet(), DeallocateSeat()
        /// </summary>
        public bool IsSeatAllocated { get { return isSeatAllocated; } }

        /// <summary>
        /// [READONLY]
        /// The mass of the object in kilograms.
        /// </summary>
        public float Mass { get { return mass; } }

        /// <summary>
        /// [READONLY]
        /// Get the rigidbody attached to the interactive-enabled object (if any)
        /// </summary>
        public Rigidbody ObjectRigidbody { get { return rBody; } }

        /// <summary>
        /// Get or set the S3D character that has this interactive-enabled object.
        /// NOTE: You need to get or set IsHeld or IsStashed separately.
        /// </summary>
        public StickyControlModule Sticky3DCharacter { get { return stickyControlModule; } set { SetSticky3DCharacter(value); } }

        /// <summary>
        /// If the object is held, equipped or stashed, get the StickyID for the character that currently possesses it.
        /// Otherwise return 0.
        /// </summary>
        public int Sticky3DCharacterID { get { return (isHeld || isEquipped || isStashed) && stickyControlModule != null ? stickyControlModule.StickyID : 0; } }

        /// <summary>
        /// Get or set which socket (if any) is this interactive-enabled object attached to?
        /// NOTE: You need to set IsSocketed separately.
        /// </summary>
        public StickySocket StickySocketedOn { get { return stickySocketedOn; } set { SetStickySocket(value); } }

        #endregion

        #region Public Properties - Gravity

        /// <summary>
        /// [READONLY] Get the current reference frame transform
        /// </summary>
        public Transform CurrentReferenceFrame { get { return currentReferenceFrame; } }

        /// <summary>
        /// Get or set the gravity in metres per second per second
        /// </summary>
        public float GravitationalAcceleration { get { return gravitationalAcceleration; } set { SetGravitationalAcceleration(value); } }

        /// <summary>
        /// Get or set the world space direction that gravity acts upon the weapon when GravityMode is Direction.
        /// </summary>
        public Vector3 GravityDirection { get { return gravityDirection; } set { SetGravityDirection(value); } }

        /// <summary>
        /// Get or set the method used to determine in which direction gravity is acting.
        /// </summary>
        public StickyManager.GravityMode GravityMode { get { return gravityMode; } set { SetGravityMode(value); } }

        /// <summary>
        /// [READONLY]
        /// Is there a rigidbody currently attached to the object?
        /// </summary>
        public bool HasRigidbody { get { return isRBody; } }

        /// <summary>
        /// Get if the object is affected by gravity
        /// </summary>
        public bool IsUseGravity { get { return isUseGravity; } }

        #endregion

        #region Public Virtual Properties

        /// <summary>
        /// Is this an interactive-enabled StickyMagazine?
        /// </summary>
        public virtual bool IsStickyMagazine { get { return false; } }

        /// <summary>
        /// Is this an interactive-enabled StickyWeapon?
        /// </summary>
        public virtual bool IsStickyWeapon { get { return false; } }

        #endregion

        #region Public Static Variables
        /// <summary>
        /// Used to denote that an interactive reference is not set.
        /// </summary>
        public static int NoID = 0;

        #endregion

        #region Internal only Properties

        #if UNITY_EDITOR
        /// <summary>
        /// [INTERNAL ONLY]
        /// Show hand hold, equip point, and socket point gizmos in the scene view
        /// </summary>
        public bool ShowEQPGizmosInSceneView { get { return showEQPGizmosInSceneView; } }
        public bool ShowSOCPGizmosInSceneView { get { return showSOCPGizmosInSceneView; } }
        public bool ShowHH1GizmosInSceneView { get { return showHH1GizmosInSceneView; } }
        public bool ShowHH2GizmosInSceneView { get { return showHH2GizmosInSceneView; } }
        public bool ShowHH1LHGizmosInSceneView { get { return showHH1LHGizmosInSceneView; } }
        public bool ShowHH2LHGizmosInSceneView { get { return showHH2LHGizmosInSceneView; } }

        #endif

        internal Collider[] Colliders { get { return isInitialised ? colliders : null; } }

        /// <summary>
        /// Is a lasso operation in progress? THis moves the iteractive object from one place to another over time.
        /// E.g., Hand to Socket, (character) Equip Point to Socket, Socket to Hand, Socket to (character) Equip Pont.
        /// </summary>
        internal bool IsLassoEnabled { get { return isLassoEnabled; } set { isLassoEnabled = value; } }

        /// <summary>
        /// The source or destination Sticky3D character for the lasso operation
        /// </summary>
        internal StickyControlModule LassoCharacter { get; set; }

        /// <summary>
        /// The destination Equip Point on a Sticky3D character for the lasso operation
        /// </summary>
        internal S3DEquipPoint LassoEquipPoint { get; set; }

        /// <summary>
        /// The position, in destination local space, where the lasso operation begins
        /// </summary>
        internal Vector3 LassoSourcePositionLS { get { return lassoSourcePositionLS; } set { lassoSourcePositionLS = value; } }

        /// <summary>
        /// The rotation, in destination local space, where the lasso operation begins
        /// </summary>
        internal Quaternion LassoSourceRotationLS { get { return lassoSourceRotationLS; } set { lassoSourceRotationLS = value; } }

        /// <summary>
        /// The destination position, in destination local space, there the lasso operation end.
        /// </summary>
        internal Vector3 LassoTargetPositionLS { get { return lassoTargetPositionLS; } set { lassoTargetPositionLS = value; } }

        /// <summary>
        /// The destination rotation, in destination local space, where the lasso operation ends.
        /// </summary>
        internal Quaternion LassoTargetRotationLS { get { return lassoTargetRotationLS; } set { lassoTargetRotationLS = value; } }

        /// <summary>
        /// The source or destination socket
        /// </summary>
        internal StickySocket LassoSocket { get { return lassoSocket; } set { lassoSocket = value; } }

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        internal Transform PreEquippedParentTfrm { get { return preEquippedParentTfrm; } set { preEquippedParentTfrm = value; } }

        /// <summary>
        /// [INTERNAL ONLY]
        /// Used in a few specific cases like when an item is grabbed, stashed, then grabbed from the stash.
        /// </summary>
        internal Transform PreGrabParentTfrm { get { return preGrabParentTfrm; } set { preGrabParentTfrm = value; } }

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        internal Transform PreSocketParentTfrm { get { return preSocketParentTfrm; } set { preSocketParentTfrm = value; } }

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        internal Transform PreStashParentTfrm { get { return preStashParentTfrm; } set { preStashParentTfrm = value; } }

        #endregion

        #region Protected Variables - Serialized General

        [SerializeField] protected int guidHash = 0;

        /// <summary>
        /// The interactive tag used to determine compatibility with things like StickySockets or character Equip Points.
        /// </summary>
        [SerializeField] protected int interactiveTag = 1 << 0;

        /// <summary>
        /// The common interactive tags scriptableobject used to determine which interactive objects
        /// can be added to a StickySocket or character Equip Point.
        /// Typically, only one would be used for all interactive objects as the text is only used to help you remember what each number from 1-32 represents.
        /// </summary>
        [SerializeField] protected S3DInteractiveTags interactiveTags = null;

        /// <summary>
        /// Can this item be activated? Typically used with grabble objects.
        /// </summary>
        [SerializeField] protected bool isActivable = false;

        /// <summary>
        /// Can the item be attached to the character body (typically a bone). When equipped,
        /// interactive-enabled objects become dormant or inactive. The objects remain visible.
        /// See also isGrabbable and isStashable.
        /// </summary>
        [SerializeField] protected bool isEquippable = false;

        /// <summary>
        /// Can this item be grabbed or held?
        /// </summary>
        [SerializeField] protected bool isGrabbable = false;

        /// <summary>
        /// Can values be read from this object? This is typically used
        /// for reading the position of a virtual lever or joystick.
        /// </summary>
        [SerializeField] protected bool isReadable = false;

        /// <summary>
        /// Can this item be selected in the scene?
        /// </summary>
        [SerializeField] protected bool isSelectable = false;

        /// <summary>
        /// Is this item suitable for sitting on?
        /// </summary>
        [SerializeField] protected bool isSittable = false;

        /// <summary>
        /// Can this item be attached to a StickySocket?
        /// </summary>
        [SerializeField] protected bool isSocketable = false;

        /// <summary>
        /// Can this item be stashed in the inventory of a character?
        /// </summary>
        [SerializeField] protected bool isStashable = false;

        /// <summary>
        /// Can this item be touched by a character? Typically used with Hand IK.
        /// </summary>
        [SerializeField] protected bool isTouchable = false;

        /// <summary>
        /// When the item is selected, it is automatically unselected making it like a button click event. 
        /// Only the OnSelected events are triggered.
        /// </summary>
        [SerializeField] protected bool isAutoUnselect = false;

        /// <summary>
        /// When the item is activated, it is automatically deactivated making it a single on and off action.
        /// Only the OnActivated events are triggered.
        /// </summary>
        [SerializeField] protected bool isAutoDeactivate = false;

        /// <summary>
        /// Mass of the object in kilograms. Can be used when dropping the object
        /// </summary>
        [SerializeField] protected float mass = 1f;

        /// <summary>
        /// The number of seconds to delay firing the onPostInitialised event methods after
        /// the interactive object has been initialised.
        /// </summary>
        [SerializeField, Range(0f, 30f)] private float onPostInitialisedEvtDelay = 0f;

        /// <summary>
        /// The joint to read positional data from when Is Readable is true.
        /// [CURRENTLY NOT USED - See readablePivot]
        /// </summary>
        //[SerializeField] protected Joint readableJoint = null;

        /// <summary>
        /// The point where the readable lever pivots around.
        /// </summary>
        [SerializeField] protected Transform readablePivot = null;

        /// <summary>
        /// If the object is sittable, is the seat allocated? Typically,
        /// used when reserve a seat for a character to sit on.
        /// </summary>
        [SerializeField] protected bool isSeatAllocated = false;

        /// <summary>
        /// Show the Equip Point gizmos in the scene view
        /// </summary>
        [SerializeField] private bool showEQPGizmosInSceneView = false;

        /// <summary>
        /// Show the Socket Point gizmos in the scene view
        /// </summary>
        [SerializeField] private bool showSOCPGizmosInSceneView = false;

        /// <summary>
        /// Show the primary hand hold gizmos in the scene view
        /// </summary>
        [SerializeField] private bool showHH1GizmosInSceneView = false;
        /// <summary>
        /// Show the left-hand hand hold gizmos in the scnee view (default is right hand)
        /// </summary>
        [SerializeField] private bool showHH1LHGizmosInSceneView = false;

        /// <summary>
        /// Show the secondary hand hold gizmos in the scene view
        /// </summary>
        [SerializeField] private bool showHH2GizmosInSceneView = false;
        /// <summary>
        /// Show the left-hand hand hold gizmos in the scnee view (default is right hand)
        /// </summary>
        [SerializeField] private bool showHH2LHGizmosInSceneView = false;

        #endregion

        #region Protected Variables - Serialized Gravity

        /// <summary>
        /// The amount of angular drag the object has
        /// </summary>
        [SerializeField] protected float angularDrag = 0.05f;

        /// <summary>
        /// The rigidbody collision detection mode
        /// </summary>
        [SerializeField] protected CollisionDetectionMode collisionDetection = CollisionDetectionMode.Discrete;

        /// <summary>
        /// The amount of drag the object has. A solid block of metal would be 0.001, while a feather would be 10.
        /// </summary>
        [SerializeField] protected float drag = 0.01f;

        /// <summary>
        /// Object is effected by gravity
        /// </summary>
        [SerializeField] protected bool isUseGravity = false;

        /// <summary>
        /// The gravity in metres per second per second
        /// </summary>
        [SerializeField] protected float gravitationalAcceleration = 9.81f;

        /// <summary>
        /// The world space direction that gravity acts upon the weapon when GravityMode is Direction.
        /// </summary>
        [SerializeField] protected Vector3 gravityDirection = Vector3.down;

        /// <summary>
        /// The method used to determine in which direction gravity is acting.
        /// </summary>
        [SerializeField] protected StickyManager.GravityMode gravityMode = StickyManager.GravityMode.UnityPhysics;

        /// <summary>
        /// if gravity is enabled and Gravity Type is Reference Frame, when the object is dropped,
        /// it will inherit the reference frame from the character.
        /// </summary>
        [SerializeField] protected bool isInheritGravity = true;

        /// <summary>
        /// Initial or default reference frame transform the object will stick to when Use Gravity is enabled and Gravity Mode is ReferenceFrame.
        /// </summary>
        [SerializeField] protected Transform initialReferenceFrame;

        /// <summary>
        /// The rigidbody interpolation
        /// </summary>
        [SerializeField] protected RigidbodyInterpolation interpolation = RigidbodyInterpolation.None;

        #endregion

        #region Protected Variables - Editor

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [SerializeField] protected bool showGravitySettingsInEditor = true;

        #endregion

        #region Protected and Private Variables

        protected bool isInitialised = false;

        /// <summary>
        /// As reference to all enabled colliders on this interactive-enabled object when it is initialised
        /// </summary>
        [System.NonSerialized] protected Collider[] colliders;
        private int numColliders = 0;

        /// <summary>
        /// This is the original parent before it is equipped
        /// </summary>
        [System.NonSerialized] protected Transform preEquippedParentTfrm = null;

        /// <summary>
        /// If isReparentOnDrop is true, this is the original parent before it is grabbed.
        /// </summary>
        [System.NonSerialized] protected Transform preGrabParentTfrm = null;

        /// <summary>
        /// This is the original parent before it is socketed
        /// </summary>
        [System.NonSerialized] protected Transform preSocketParentTfrm = null;

        /// <summary>
        /// This is the original parent before it is stashed
        /// </summary>
        [System.NonSerialized] protected Transform preStashParentTfrm = null;
        
        /// <summary>
        /// The rigidbody (if any) attached to this gameobject
        /// </summary>
        [System.NonSerialized] protected Rigidbody rBody = null;

        /// <summary>
        /// Is there a rigidbody attached to this gameobject?
        /// </summary>
        protected bool isRBody = false;

        // Remember pre-Grab or pre-Stash rigidbody settings
        [System.NonSerialized] protected S3DRBodySettings origRBodySettings = null;

        protected Vector3 prevPosition = Vector3.zero;
        protected Vector3 currentVelo = Vector3.zero;

        protected float externalThrowStrength = 1f;

        /// <summary>
        /// The object or tool is activated or not activated. This is independent
        /// from what hand(s) and character(s) are currently interacting with the object.
        /// </summary>
        protected bool isActivated = false;

        /// <summary>
        /// Grabbable objects can follow a target transform rather than being parented
        /// to the character's hand.
        /// </summary>
        [System.NonSerialized] protected Transform followTarget = null;

        protected bool isFollowingTarget = false;

        /// <summary>
        /// Is the object currently being held by a Sticky3D character?
        /// This should NOT be true when equipped or stashed.
        /// The StickyInteractorBridge allows non-S3D objects to grab interactive objects.
        /// This should NOT be true when non-S3D characters grab it.
        /// </summary>
        protected bool isHeld = false;

        /// <summary>
        /// [EXPERIMENTAL]
        /// When being grabbed by a character, will it spring toward the
        /// hands rather than being snapped to hand position?
        /// </summary>
        protected bool isLassoEnabled = false;

        /// <summary>
        /// [EXPERIMENTAL]
        /// </summary>
        protected Vector3 lassoSourcePositionLS = Vector3.zero;

        /// <summary>
        /// [EXPERIMENTAL]
        /// </summary>
        protected Quaternion lassoSourceRotationLS = Quaternion.identity;

        /// <summary>
        /// [EXPERIMENTAL]
        /// </summary>
        protected Vector3 lassoTargetPositionLS = Vector3.zero;

        /// <summary>
        /// [EXPERIMENTAL]
        /// </summary>
        protected Quaternion lassoTargetRotationLS = Quaternion.identity;

        /// <summary>
        /// [EXPERIMENTAL]
        /// Where the object is being grabbed from to sent to
        /// </summary>
        [System.NonSerialized] protected StickySocket lassoSocket = null;

        /// <summary>
        /// Is this object currently attached to a StickySocket?
        /// It should NOT be true when Held, Equipped or Stashed
        /// </summary>
        protected bool isSocketed = false;

        /// <summary>
        /// Is this object currently in Stashed by a Sticky3D character?
        /// It should NOT be true when Held, Socketed or Equipped
        /// </summary>
        protected bool isStashed = false;

        /// <summary>
        /// Is this object currently Equipped by a Sticky3D character?
        /// It should NOT be true when Held, Socketed or Stashed
        /// </summary>
        protected bool isEquipped = false;

        protected bool isReadableReady = false;
        protected bool isReadablePivot = false;
        protected bool isReadableSmoothDisable = false;
        protected Vector3 currentReadValue = Vector3.zero;
        protected Vector3 prevReadValue = Vector3.zero;
        protected Quaternion initialPivotRotation = Quaternion.identity;
        [System.NonSerialized] protected Transform pivotTfrm = null;
        protected float prevPivotAngleX = 0f;
        protected float prevPivotAngleZ = 0f;
        protected float pivotOffsetAngleX = 0f;
        protected float pivotOffsetAngleZ = 0f;

        /// <summary>
        /// The currently Active StickyPopupModule.
        /// i.e. being displayed near the object.
        /// 0 = none
        /// </summary>
        protected int popupActiveInstanceID = 0;

        /// <summary>
        /// The character (if any) that has this interactive-enabled object.
        /// WARNING: Need to set this to null when it is dropped.
        /// </summary>
        [System.NonSerialized] protected StickyControlModule stickyControlModule = null;

        /// <summary>
        /// The socket (if any) that this interactive-enabled object is attached to.
        /// WARNING: Need to set this to null when detached.
        /// </summary>
        [System.NonSerialized] protected StickySocket stickySocketedOn = null;

        #endregion

        #region Protected Movement Variables

        protected Vector3 trfmUp;
        protected Vector3 trfmFwd;
        protected Vector3 trfmRight;
        protected Vector3 trfmPos;
        protected Quaternion trfmRot;
        protected Quaternion trfmInvRot;
        protected bool isMovementDataStale = true;

        #endregion

        #region Protected Variables - Gravity

        /// <summary>
        /// This enables inheritted classes to call ApplyGravity in their own
        /// FixedUpate AFTER base.FixedUpdate runs.
        /// </summary>
        protected bool isDelayApplyGravity = false;

        protected bool isGravityEnabled = false;
        protected int gravityModeInt = StickyManager.GravityModeDirectionInt;
        protected float defaultGravitationalAcceleration = 0f;
        protected Vector3 defaultGravityDirection = Vector3.down;

        // The current world position and rotation
        protected Vector3 currentWorldPosition = Vector3.zero;
        protected Quaternion currentWorldRotation = Quaternion.identity;

        protected Transform currentReferenceFrame = null;
        protected Transform previousReferenceFrame = null;
        protected int currentReferenceFrameId = -1;

        // The current position and rotation of the reference frame in world space
        protected Vector3 currentReferenceFramePosition = Vector3.zero;
        protected Quaternion currentReferenceFrameRotation = Quaternion.identity;
        protected Vector3 currentReferenceFrameUp = Vector3.up;
        // The current position and rotation relative to the reference frame
        protected Vector3 currentRelativePosition = Vector3.zero;
        protected Quaternion currentRelativeRotation = Quaternion.identity;

        #endregion

        #region Initialise Methods
        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart && !isInitialised) { Initialise(); }
        }

        #endregion

        #region Event Methods

        private void OnDestroy()
        {
            RemoveListeners();
        }

        #endregion

        #region Update Virtual Methods

        /// <summary>
        /// Update method that is visible and can be overridden by inherited
        /// classes like StickyWeapon.cs.
        /// Call base.Update() in protected override Update() of inherited class.
        /// </summary>
        protected virtual void Update()
        {
            if (isInitialised && !isStashed)
            {
                isMovementDataStale = true;

                if (isGrabbable)
                {
                    if (isFollowingTarget && !isStashed) { FollowTarget(false); }
                    else if (isReadableSmoothDisable) { ReadableSpring(); }

                    Vector3 currentPosition = transform.position;
                    currentVelo = (currentPosition - prevPosition) / Time.deltaTime;
                    prevPosition = currentPosition;
                }   
            }
        }

        /// <summary>
        /// FixedUpdate method that is visible and can be overridden by inherited
        /// classes like StickyWeapon.cs.
        /// Call base.FixedUpdate() in protected override FixedUpdate() of inherited class.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            if (isInitialised && !isStashed)
            {
                isMovementDataStale = true;

                if (isFollowingTarget) { FollowTarget(true); }
                else if (isReadableSmoothDisable) { ReadableSpring(); }

                if (isReadable) { ReadObject(); }

                if (!isHeld && !isDelayApplyGravity) { ApplyGravity(); }
            }
        }

        #endregion

        #region Private, Protected or Internal Methods

        /// <summary>
        /// Call the onPostInitialised events after a delayed period.
        /// See end of Initialise()
        /// </summary>
        protected void DelayOnPostInitialiseEvents()
        {
            onPostInitialised.Invoke(guidHash, 0);
        }

        /// <summary>
        /// Get the current readable joint angles.
        /// X is Left and Right
        /// Z is Forward and Back
        /// Y should be twist
        /// This has only been testing with hinge joints with forward/back angles.
        /// </summary>
        /// <returns></returns>
        //protected virtual Vector3 GetJointAngles()
        //{
        //    Vector3 jointAngles = Vector3.zero;

        //    if (readableJoint != null)
        //    {
        //        // Joint angle can be unreliable, so use local Euler Angles instead
        //        Vector3 _angles = readableJoint.transform.localEulerAngles;

        //        // Left and Right, rotates around the local z-axis
        //        // Forward and Back, rotates around the local x-axis
        //        jointAngles.x = _angles.z;
        //        jointAngles.z = _angles.x;
        //        jointAngles.y = _angles.y;

        //        if (jointAngles.x > 180f) { jointAngles.x -= 360f; }
        //        if (jointAngles.z > 180f) { jointAngles.z -= 360f; }
        //        if (jointAngles.y > 180f) { jointAngles.y -= 360f; }
        //    }

        //    return jointAngles;
        //}

        /// <summary>
        /// Get the current readable pivot angles.
        /// X is Left (-ve) and Right (+ve)
        /// Z is Forward (+ve) and Back (-ve)
        /// Y should be twist (currently unused)
        /// </summary>
        /// <returns></returns>
        protected virtual Vector3 GetPivotAngles()
        {
            Vector3 pivotAngles = Vector3.zero;

            if (readablePivot)
            {
                // Joint angle can be unreliable, so use local Euler Angles instead
                Vector3 _angles = readablePivot.localEulerAngles;

                // Left (-ve) and Right (+ve), rotates around the local z-axis
                // Forward (+ve) and Back (-ve), rotates around the local x-axis
                pivotAngles.x = -_angles.z;
                pivotAngles.z = _angles.x;
                pivotAngles.y = _angles.y;

                // Values to the right are -360 -> -270
                if (pivotAngles.x > 180f) { pivotAngles.x -= 360f; }
                else if (pivotAngles.x < -180f) { pivotAngles.x += 360f; }

                if (pivotAngles.z > 180f) { pivotAngles.z -= 360f; }
                if (pivotAngles.y > 180f) { pivotAngles.y -= 360f; }
            }

            return pivotAngles;
        }

        private void EnableOrDisableNonTriggerColliders(bool isEnabled)
        {
            for (int cIdx = 0; cIdx < numColliders; cIdx++)
            {
                Collider collider = colliders[cIdx];
                if (collider != null && !collider.isTrigger) { collider.enabled = isEnabled; }
            }
        }

        private void EnableOrDisableTriggerColliders(bool isEnabled)
        {
            for (int cIdx = 0; cIdx < numColliders; cIdx++)
            {
                Collider collider = colliders[cIdx];
                if (collider != null && collider.isTrigger) { collider.enabled = isEnabled; }
            }
        }

        /// <summary>
        /// Calculate the world space rotation and position for the readable pivot.
        /// Typically used for in-game levers and joysticks
        /// </summary>
        /// <param name="newPivotPosition"></param>
        /// <param name="newPivotRotation"></param>
        private void GetPivotMovement(out Vector3 newPivotPosition, out Quaternion newPivotRotation)
        {
            // Reference a base transform that we don't rotate in local space (pivotTfrm)
            Quaternion pivotTfrmRot = pivotTfrm.rotation;
            newPivotPosition = rBody.position;

            // Get local space direction
            Vector3 targetDir = Quaternion.Inverse(pivotTfrmRot) * (followTarget.position - pivotTfrm.position);

            if (targetDir.y < 0.01f) { targetDir.y = 0.01f; }
            if (targetDir.sqrMagnitude < Mathf.Epsilon) { targetDir = Vector3.up; }
            else { targetDir = targetDir.normalized; }

            // Forward-backward. Subtract the at rest angle offset (if any).
            float signedAngleZ = -Vector2.SignedAngle(new Vector2(targetDir.y, targetDir.z), Vector2.right) - pivotOffsetAngleZ;
            if (signedAngleZ < -readableMinZ) { signedAngleZ = -readableMinZ; }
            else if (signedAngleZ > readableMaxZ) { signedAngleZ = readableMaxZ; }

            // Left-right - rotate around local z-axis on xy plane. Subtract the at rest angle offset (if any).
            float signedAngleX = Vector2.SignedAngle(new Vector2(targetDir.x, targetDir.y), Vector2.up) - pivotOffsetAngleX;

            // Clamp left-right based on inspector values
            if (signedAngleX < -readableMinX) { signedAngleX = -readableMinX; }
            else if (signedAngleX > readableMaxX) { signedAngleX = readableMaxX; }

            //readablePivot.localRotation = Quaternion.Euler(new Vector3(signedAngleZ, 0f, -signedAngleX));
            newPivotRotation = pivotTfrmRot * Quaternion.Euler(new Vector3(signedAngleZ, 0f, -signedAngleX));

            prevPivotAngleX = signedAngleX;
            prevPivotAngleZ = signedAngleZ;
        }

        /// <summary>
        /// Follow the target (hand) if this object currently has target.
        /// NOTE: Levers use a non-kinematic rigidbody.
        /// </summary>
        private void FollowTarget(bool isFixedUpdate)
        {
            if (followTarget == null)
            {
                isFollowingTarget = false;
            }
            else if (isFixedUpdate)
            {
                if (rBody != null)
                {
                    if (rBody.isKinematic)
                    {
                        if (isReadableReady && pivotTfrm != null)
                        {
                            Vector3 newPivotPosition;
                            Quaternion newPivotRotation;

                            GetPivotMovement(out newPivotPosition, out newPivotRotation);

                            rBody.MovePosition(newPivotPosition);
                            rBody.MoveRotation(newPivotRotation);

                            // If we only update the rBody position seems to significantly lag behind the direction
                            // of travel (by many metres!!!) - no idea why...
                            readablePivot.position = newPivotPosition;
                            readablePivot.rotation = newPivotRotation;
                        }
                        else
                        {
                            // Test - snap to followTarget
                            rBody.MovePosition(followTarget.position);
                            rBody.MoveRotation(followTarget.rotation);
                        }
                    }
                    // Non-Kinematic (dynamic) rigidbody
                    else
                    {
                        // NOTE: For levers, we previously have to freeze Z-axis on rigidbody to avoid
                        // side-to-side movement.

                        //if (pivotTfrm != null)
                        //{
                        //    // Reference a base transform that we don't rotate - use as reference forward and up direction
                        //    // with which to calculate the relative direction to the target and the relative angle of rotation.

                        //    // Project the direction vector onto the y-z plane
                        //    Vector3 targetDir = Vector3.ProjectOnPlane(followTarget.position - pivotTfrm.position, pivotTfrm.right);

                        //    if (targetDir.sqrMagnitude < Mathf.Epsilon) { targetDir = pivotTfrm.up; }
                        //    else { targetDir = targetDir.normalized; }

                        //    //DebugExtension.DebugArrow(pivotTfrm.position, targetDir, Color.yellow);

                        //    float signedAngleZ = Vector3.SignedAngle(pivotTfrm.up, targetDir, pivotTfrm.right);

                        //    //Debug.Log("[DEBUG] targetDir: " + targetDir + " signedAngle: " + signedAngleZ);

                        //    rBody.angularVelocity = (signedAngleZ - GetJointAngles().z) * Mathf.Deg2Rad * 10f * pivotTfrm.right;

                        //    prevPivotAngleZ = signedAngleZ;
                        //}
                        //else
                        {
                            // Use v1.0.8 version
                            Vector3 handHoldPos = rBody.position + (rBody.rotation * handHold1Offset);

                            Vector3 targetVelo = followTarget.position - handHoldPos - rBody.velocity;

                            rBody.AddForceAtPosition(targetVelo * rBody.mass, handHoldPos, ForceMode.VelocityChange);
                        }
                    }
                }
            }
            else if (isCarryInHand)
            {
                transform.SetPositionAndRotation(followTarget.position, followTarget.rotation);
            }
        }

        /// <summary>
        /// Spring action for the readable lever or joystick.
        /// TODO - spring is currently linear but should add some damping options
        /// </summary>
        protected void ReadableSpring()
        {
            if (isReadable)
            {
                if (isReadableSmoothDisable)
                {
                    readablePivot.localRotation = Quaternion.RotateTowards(readablePivot.localRotation, initialPivotRotation, readableSpringStrength * Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// Update position and movement data using data obtained from the transform (and rigidbody in the future).
        /// </summary>
        protected void UpdatePositionAndMovementData()
        {
            // Update data obtained from transform
            trfmPos = transform.position;
            trfmFwd = transform.forward;
            trfmRight = transform.right;
            trfmUp = transform.up;
            trfmRot = transform.rotation;
            trfmInvRot = Quaternion.Inverse(trfmRot);

            // Update data obtained from rigidbody

            isMovementDataStale = false;
        }

        #endregion

        #region Protected Non-Virtual Methods - Gravity

        /// <summary>
        /// Calculate Position and Rotation From Previous Frame
        /// </summary>
        protected void CalculateFromPreviousFrame()
        {
            // Get information from reference frame
            if (currentReferenceFrame != null)
            {
                // Get reference frame position and rotation
                currentReferenceFrameRotation = currentReferenceFrame.rotation;
                currentReferenceFramePosition = currentReferenceFrame.position;
                currentReferenceFrameUp = currentReferenceFrame.up;
            }
            else
            {
                // If reference frame is null assume a "zero" reference frame
                currentReferenceFramePosition = Vector3.zero;
                currentReferenceFrameRotation = Quaternion.identity;
                currentReferenceFrameUp = Vector3.up;
            }

            // We retain the relative position and rotation from the last frame
            // Convert current position and rotation into world space
            currentWorldPosition = currentReferenceFramePosition + (currentReferenceFrameRotation * currentRelativePosition);
            currentWorldRotation = currentReferenceFrameRotation * currentRelativeRotation;

            // Remember previous relative/world position and rotation
            //currentInteractiveUp = currentWorldRotation * Vector3.up;
            //currentInteractiveFwd = currentWorldRotation * Vector3.forward;
            //currentInteractiveRight = currentWorldRotation * Vector3.right;
        }

        #endregion

        #region Protected Virtual Methods - Gravity

        /// <summary>
        /// Apply Gravity if required.
        /// This should always be called from FixedUpdate()
        /// </summary>
        protected virtual void ApplyGravity()
        {
            if (isUseGravity && isRBody)
            {
                // No need to do anything if Unity Physics gravity is in use.

                if (gravityModeInt == StickyManager.GravityModeDirectionInt)
                {
                    // Add gravity based on world space gravity direction, mass of the object, and the fixed time step.
                    // Uses ForceMode.Force
                    rBody.AddForce(gravityDirection * gravitationalAcceleration * mass);
                }
                else if (gravityModeInt == StickyManager.GravityModeRefFrameInt && currentReferenceFrameId != 0)
                {
                    // If the reference frame has been destroyed, clear it.
                    if (currentReferenceFrame == null) { SetCurrentReferenceFrame(null); }
                    else
                    {                      
                        if (isMovementDataStale) { UpdatePositionAndMovementData(); }

                        CalculateFromPreviousFrame();

                        /// TODO - Option to Move and rotate this interactive object as the reference frame moves and rotates


                        // Apply gravity relative to the reference frame Down direction.
                        rBody.AddForce(-currentReferenceFrameUp * gravitationalAcceleration * mass);
                    }
                }
            }
        }

        /// <summary>
        /// Set up a rigidbody for use with gravity settings.
        /// Has no effect if Use Gravity is disabled.
        /// </summary>
        protected virtual void SetUpRigidbody()
        {
            // Do nothing if not using gravity
            if (!isUseGravity) { return; }

            if (!isRBody)
            {
                AddRigidbody(false);
            }

            if (isRBody)
            {
                rBody.useGravity = gravityModeInt == StickyManager.GravityModeUnityInt;
                rBody.mass = mass;
                rBody.drag = drag;
                rBody.angularDrag = angularDrag;
                rBody.interpolation = interpolation;
                rBody.collisionDetectionMode = collisionDetection;
            }
        }

        /// <summary>
        /// Updates the position of the transform in an Update statement. Only called when we are in fixed update move mode. This
        /// is used to make sure that the visual position of the object doesn't lag one frame behind the position of the
        /// reference frame.
        /// </summary>
        protected void UpdateTransformPosition ()
        {
            // Get information from reference frame
            if (currentReferenceFrame != null)
            {
                // Get reference frame position and rotation
                currentReferenceFrameRotation = currentReferenceFrame.rotation;
                currentReferenceFramePosition = currentReferenceFrame.position;
                currentReferenceFrameUp = currentReferenceFrame.up;
            }
            else
            {
                // If reference frame is null assume a "zero" reference frame
                currentReferenceFramePosition = Vector3.zero;
                currentReferenceFrameRotation = Quaternion.identity;
                currentReferenceFrameUp = Vector3.up;
            }

            // We retain the relative position and rotation from the last frame
            // Convert current position and rotation into world space
            currentWorldPosition = currentReferenceFramePosition + (currentReferenceFrameRotation * currentRelativePosition);
            currentWorldRotation = currentReferenceFrameRotation * currentRelativeRotation;
            // Assign our new position and rotation to the transform
            //MoveInteractiveObject(false);
        }

        #endregion

        #region Public API Virtual Methods - General

        /// <summary>
        /// Attempt to activate the object. Only initialised and activable objects can be
        /// activated.
        /// public override void ActivateObject (int stickyID)
        /// {
        ///    base.ActivateObject (int stickyID);
        ///    // Do stuff here
        /// }
        /// </summary>
        /// <param name="stickyID"></param>
        public virtual void ActivateObject (int stickyID)
        {
            if (isInitialised && isActivable && !isActivated)
            {
                isActivated = true;

                if (onActivated != null) { onActivated.Invoke(StickyInteractiveID, stickyID, Vector3.zero, Vector3.zero); }

                // If auto deactive is on, immediately deactive the object.
                if (isAutoDeactivate) { DeactivateObject(stickyID); }
            }
        }

        /// <summary>
        /// Attempt to deactivate the object. Only initialised and activable objects can
        /// be deactivated.
        /// If required, you can override this method.
        /// public override void DeactivateObject (int stickyID)
        /// {
        ///    base.DeactivateObject (int stickyID);
        ///    // Do stuff here
        /// }
        /// </summary>
        /// <param name="stickyID"></param>
        public virtual void DeactivateObject (int stickyID)
        {
            if (isInitialised && isActivable && isActivated)
            {
                isActivated = false;

                if (!isAutoDeactivate && onDeactivated != null) { onDeactivated.Invoke(StickyInteractiveID, stickyID); }
            }
        }

        /// <summary>
        /// Safely destroy the interactive object
        /// 1. Return active popup (if any) to the pool
        /// 2. Destroy the actual gameobject
        /// </summary>
        public virtual void DestroyInteractive()
        {
            if (isInitialised)
            {
                // Return the active popup to the pool (if there is one)
                if (popupActiveInstanceID != 0)
                {
                    /// TODO - Fix potential Garbage created here.
                    StickyPopupModule activePopup = GetComponentInChildren<StickyPopupModule>(true);

                    // If there is a popup, call the overridden version of DestroyGenericObject.
                    if (activePopup != null) { activePopup.DestroyGenericObject(); }
                }
            }

            #if UNITY_EDITOR
            DestroyImmediate(gameObject);
            #else
            Destroy(gameObject);
            #endif
        }

        /// <summary>
        /// Reparent if required, and invoke an OnDropped items.
        /// If Parent on Grab is enabled, the object will be unparented from the hand.
        /// Only initialised and grabbable or stashable objects can be dropped.
        /// If you wish to reenable colliders, call the API methods
        /// from the onDropped events in the editor.
        /// If required, you can override this method.
        /// public override void DropObject (int stickyID)
        /// {
        ///    base.DropObject (int stickyID);
        ///    // Do stuff here
        /// }
        /// </summary>
        /// <param name="stickyID"></param>
        public virtual void DropObject (int stickyID)
        {
            if (isInitialised && (isGrabbable || isStashable || isEquippable))
            {
                // Activable and Grabbable activated objects can be automatically
                // deactivated when they are dropped.
                if (isActivable && isActivated && isDeactivateOnDrop)
                {
                    DeactivateObject(stickyID);
                }

                if (isGrabbable && isHeld)
                {
                    // If the object was following a (hand) target, stop doing that.
                    if (isFollowingTarget)
                    {
                        // If this is a lever or joystick, return it to the centre
                        // or original location when released (if required)
                        if (isReadable && !isParentOnGrab && readableAutoRecentre)
                        {
                            RecentreReadable(true);
                        }

                        isFollowingTarget = false;
                        followTarget = null;
                    }

                    // If parent on grab is enabled, we want to unparent when dropping the object
                    if (isReparentOnDrop && preGrabParentTfrm != null)
                    {
                        RestoreParent();
                    }
                    else if (isParentOnGrab)
                    {
                        transform.SetParent(null);
                        preGrabParentTfrm = null;
                    }
                }
                else if (isStashable && isStashed)
                {
                    if (isReparentOnDrop)
                    {
                        transform.SetParent(preStashParentTfrm);
                        preStashParentTfrm = null;
                    }
                }
                else if (isEquippable && isEquipped)
                {
                    if (isReparentOnDrop)
                    {
                        transform.SetParent(preEquippedParentTfrm);
                        preEquippedParentTfrm = null;
                    }
                }

                if (rBody != null)
                {
                    isRBody = true;
                    RestoreRigidbodySettings();

                    rBody.velocity = currentVelo * externalThrowStrength;
                }

                // Should we get the reference frame from the character?
                Transform inherittedRefFrame = isInheritGravity ? GetCharacterReferenceFrame() : null;

                // Is there an inheritted reference frame?
                if (isUseGravity && gravityModeInt == StickyManager.GravityModeRefFrameInt && inherittedRefFrame != null)
                {
                    SetCurrentReferenceFrame(inherittedRefFrame);
                }

                // Remove reference to the character
                stickyControlModule = null;
                isHeld = false;
                isStashed = false;
                isEquipped = false;

                if (onDropped != null) { onDropped.Invoke(StickyInteractiveID, stickyID); }
            }
        }

        /// <summary>
        /// This is automatically called by stickyControlModule.EquipInteractiveInternal(..)
        /// Automatically deactivate IsActivable objects before it is equipped.
        /// </summary>
        /// <param name="equippedBy"></param>
        /// <param name="equippedAt"></param>
        /// <returns></returns>
        public virtual bool EquipObject (StickyControlModule equippedBy, S3DEquipPoint equippedAt)
        {
            bool equippedSuccessfully = false;

            if (isInitialised)
            {
                if (CheckCanBeEquipped(equippedBy, equippedAt))
                {
                    int equippedByStickyID = equippedBy.StickyID;
                    bool isCanBeEquipped = true;

                    // Automatically deactivate IsActivable objects before it is equipped
                    if (isActivable && isActivated)
                    {
                        DeactivateObject(equippedByStickyID);
                    }

                    // Has this object already been grabbed?
                    if (isHeld && isGrabbable)
                    {
                        // Is it being held by another character?
                        // Currently characters cannot steal from one other
                        if (stickyControlModule.StickyID != equippedByStickyID)
                        {
                            isCanBeEquipped = false;

                            #if UNITY_EDITOR
                            string alreadyHeldBy = stickyControlModule == null ? "unknown character" : stickyControlModule.name;

                            Debug.LogWarning("ERROR StickyInteractive.EquipObject() - " + equippedBy.name + " cannot Equip " + name + " because it is already held by " + alreadyHeldBy);
                            #endif             
                        }
                        else
                        {
                            // If grabbed, the colliders should already be registered with the character
                            // and don't need to be registered again.

                            // If equipping a held weapon, restore any animation that were changed when grabbed
                            // May also need to turn off a few other things too.
                            if (IsStickyWeapon)
                            {
                                StickyWeapon weapon = (StickyWeapon)this;

                                weapon.TurnOffLaserSight();
                                weapon.StopAiming(false);
                                weapon.TurnOffScope();

                                // Must be called after weapon.StopAiming(..).
                                weapon.RestoreReticle();

                                weapon.AnimateCheckStateExitEquip();
                                weapon.RestoreLookSettings();
                                weapon.RevertWeaponAnimSets();
                            }
                        }
                    }
                    // Has this object already been stashed?
                    else if (isStashed && isStashable)
                    {
                        /// TOOD - Equip stashed item - might need to register colliders on character
                    }

                    if (isCanBeEquipped)
                    {
                        // If there is a rigidbody, save the settings and remove it.
                        // Objects that were previously stashed should not have a rigidbody.
                        if (isRBody && !origRBodySettings.isSaved)
                        {
                            origRBodySettings.SaveSettings(rBody);
                            RemoveRigidbody();
                        }

                        equippedSuccessfully = true;
                    }

                    isHeld = false;
                    isStashed = false;
                }
            }

            return equippedSuccessfully;
        }

        /// <summary>
        /// Disable colliders if required, and invoke an OnGrabbed items.
        /// Only initialised and grabbable objects can be grabbed.
        /// If there is a rigidbody attached, remove it.
        /// NOTE: Not all grabbed objects are held by S3D characters.
        /// The StickyInteractorBridge allows non-S3D objects to grab interactive objects.
        /// This method DOES NOT set IsHeld.
        /// If required, you can override this method.
        /// public override void GrabObject (int stickyID, bool isSecondaryHandHold)
        /// {
        ///    base.GrabObject (stickyID, isSecondaryHandHold);
        ///    // Do stuff here
        /// }
        /// </summary>
        /// <param name="stickyID"></param>
        /// <param name="isSecondaryHandHold"></param>
        public virtual void GrabObject (int stickyID, bool isSecondaryHandHold)
        {
            if (isInitialised && isGrabbable)
            {
                // A held object cannot be in a character's stash or attached to a Equip Point.
                // Nor can it be attached to a socket.
                isStashed = false;
                isEquipped = false;
                isSocketed = false;

                // If required, disable colliders
                if (numColliders > 0)
                {
                    if (isDisableRegularColOnGrab) { EnableOrDisableNonTriggerColliders(false); }
                    if (isDisableTriggerColOnGrab) { EnableOrDisableTriggerColliders(false); }
                }

                if (gameObject.TryGetComponent(out rBody))
                {
                    // Remember settings
                    if (!origRBodySettings.isSaved) { origRBodySettings.SaveSettings(rBody); }

                    // If there is a rigidbody attached, remove it.
                    if (isRemoveRigidbodyOnGrab)
                    {
                        RemoveRigidbody();
                    }
                    else
                    {
                        isRBody = true;

                        // switch to kinematic for object to be carried in the palm of the hand (DEFAULT SETTING)
                        if (isCarryInHand || rBody.isKinematic)
                        {
                            // Kinematic does not support ContinuousDynamic.
                            if (rBody.collisionDetectionMode == CollisionDetectionMode.ContinuousDynamic || rBody.collisionDetectionMode == CollisionDetectionMode.Continuous)
                            {
                                rBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                            }

                            rBody.isKinematic = true;
                            rBody.useGravity = false;
                            rBody.drag = 0f;
                            rBody.angularDrag = 0f;
                        }
                        // Originally a non-kinematic rigidbody that is not to be carried by the character in a hand.
                        // Typically used by things like levers
                        else
                        {
                            rBody.useGravity = false;
                            rBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                        }
                    }
                }
                else { isRBody = false; }

                if (onGrabbed != null) { onGrabbed.Invoke(GetHandHoldPosition(isSecondaryHandHold), GetHandHoldNormal(isSecondaryHandHold), StickyInteractiveID, stickyID); }
            }
        }

        /// <summary>
        /// Initialise the StickyInteractive component at runtime. Has no effect if already initialised.
        /// If you wish to override this in a child (inherited) class you almost always will want to
        /// call the base method first.
        /// public override void Initialise()
        /// {
        ///    base.Initialise();
        ///    // Do stuff here
        /// }
        /// </summary>
        public virtual void Initialise()
        {
            if (!isInitialised)
            {
                // If this component doesn't have a unique code, create one now.
                if (guidHash == 0) { guidHash = S3DMath.GetHashCodeFromGuid(); }

                // Keep compiler happy
                if (showHH1GizmosInSceneView || showHH2GizmosInSceneView || showHH1LHGizmosInSceneView || showHH2LHGizmosInSceneView
                    || showGravitySettingsInEditor || showEQPGizmosInSceneView || showSOCPGizmosInSceneView) { }

                ReinitialiseColliders();

                prevPosition = transform.position;

                // Remember initial gravity settings. See also ResetGravity().
                defaultGravitationalAcceleration = gravitationalAcceleration;
                defaultGravityDirection = gravityDirection;
                gravityModeInt = (int)gravityMode;

                if (origRBodySettings == null) { origRBodySettings = new S3DRBodySettings(); }
                else { origRBodySettings.isSaved = false; }

                isRBody = TryGetComponent(out rBody);

                if (isUseGravity)
                {
                    SetUpRigidbody();
                }

                ReInitialiseReadable();

                if (gravityModeInt == StickyManager.GravityModeRefFrameInt)
                {
                    SetCurrentReferenceFrame(initialReferenceFrame);
                }

                isInitialised = true;

                if (onPostInitialised != null)
                {
                    if (onPostInitialisedEvtDelay > 0f) { Invoke("DelayOnPostInitialiseEvents", onPostInitialisedEvtDelay); }
                    else { onPostInitialised.Invoke(guidHash, 0); }
                }
            }
        }

        /// <summary>
        /// This is called (automatically) immediately after this interactive-enabled object
        /// is equipped by a S3D character. if required, invoke any onPostEquipped items.
        /// If required, you can override this method.
        /// public override void PostEquipObject (int stickyID, S3DEquipPoint equipPoint)
        /// {
        ///    base.onPostEquipped (stickyID, equipPoint, storeItemID);
        ///    // Do stuff here
        /// }
        /// </summary>
        /// <param name="stickyID"></param>
        /// <param name="equipPoint"></param>
        /// <param name="storeItemID"></param>
        public virtual void PostEquipObject (int stickyID, S3DEquipPoint equipPoint, int storeItemID)
        {
            if (isInitialised && isEquippable && onPostEquipped != null)
            {
                onPostEquipped.Invoke(StickyInteractiveID, stickyID, storeItemID, equipPoint.guidHash);
            }
        }

        /// <summary>
        /// This is called (automatically) immediately after this interactive-enabled object
        /// is grabbed by a S3D character. if required, invoke any OnPostGrabbed items.
        /// If required, you can override this method.
        /// public override void PostGrabObject (int stickyID, bool isSecondaryHandHold)
        /// {
        ///    base.PostGrabObject (stickyID, isSecondaryHandHold);
        ///    // Do stuff here
        /// }
        /// </summary>
        /// <param name="stickyID"></param>
        /// <param name="isSecondaryHandHold"></param>
        public virtual void PostGrabObject (int stickyID, bool isSecondaryHandHold)
        {
            if (isInitialised && isGrabbable && onPostGrabbed != null)
            {
                onPostGrabbed.Invoke(GetHandHoldPosition(isSecondaryHandHold), GetHandHoldNormal(isSecondaryHandHold), StickyInteractiveID, stickyID);
            }
        }

        /// <summary>
        /// This is called (automatically) immediately after this interactive-enabled object
        /// is stashed by a S3D character. if required, invoke any OnPostStashed items.
        /// If required, you can override this method.
        /// public override void PostGrabObject (int stickyID, int storeItemID, Vector3 futureValue)
        /// {
        ///    base.PostStashObject (stickyID, storeItemID, futureValue);
        ///    // Do stuff here
        /// }
        /// </summary>
        /// <param name="stickyID"></param>
        /// <param name="storeItemID">The stashed S3DStoreItem.StoreItemID</param>
        /// <param name="futureValue">Currently not used - use Vector3.zero</param>
        public virtual void PostStashObject (int stickyID, int storeItemID, Vector3 futureValue)
        {
            if (isInitialised && isStashable && onPostStashed != null)
            {
                onPostStashed.Invoke(StickyInteractiveID, stickyID, storeItemID, futureValue);
            }
        }

        /// <summary>
        /// Read data from the object. Typically, read the x and/or z-axis
        /// values from a lever or joystick.
        /// </summary>
        public virtual void ReadObject()
        {
            if (isInitialised && isReadable)
            {
                prevReadValue = currentReadValue;

                if (isReadableReady)
                {
                    currentReadValue = Vector3.zero;

                    if (isReadablePivot)
                    {
                        Vector3 _pivotAngles = GetPivotAngles();

                        // Left and Right (limits to left and right might be different, so normalise accordingly)
                        if (_pivotAngles.x < 0f) { currentReadValue.x = -S3DMath.Normalise(-_pivotAngles.x, 0f, readableMinX); }
                        else { currentReadValue.x = S3DMath.Normalise(_pivotAngles.x, 0f, readableMaxX); }

                        // Forward and Back (limits forward and back might be different, so normalise accordingly)
                        if (_pivotAngles.z < 0f) { currentReadValue.z = -S3DMath.Normalise(-_pivotAngles.z, 0f, readableMinZ); }
                        else { currentReadValue.z = S3DMath.Normalise(_pivotAngles.z, 0f, readableMaxZ); }

                        // Clamp -1.0 to 1.0 to avoid floating point errors
                        if (currentReadValue.x < -1f) { currentReadValue.x = -1f; }
                        else if (currentReadValue.x > 1f) { currentReadValue.x = 1f; }

                        if (currentReadValue.z < -1f) { currentReadValue.z = -1f; }
                        else if (currentReadValue.z > 1f) { currentReadValue.z = 1f; }
                    }

                    // x-axis check deadzone
                    if (currentReadValue.x > -readableDeadZone && currentReadValue.x < readableDeadZone)
                    {
                        currentReadValue.x = 0f;
                    }
                    else if (readableInvertX)
                    {
                        currentReadValue.x = -currentReadValue.x;
                    }

                    // z-axis check deadzone
                    if (currentReadValue.z > -readableDeadZone && currentReadValue.z < readableDeadZone)
                    {
                        currentReadValue.z = 0f;
                    }
                    else if (readableInvertZ)
                    {
                        currentReadValue.z = -currentReadValue.z;
                    }

                    currentReadValue.x = StickyInputModule.CalculateAxisInput(currentReadValue.x, prevReadValue.x, readableSensitivityX, 0f);
                    currentReadValue.z = StickyInputModule.CalculateAxisInput(currentReadValue.z, prevReadValue.z, readableSensitivityZ, 0f);
                }

                if (currentReadValue != prevReadValue && onReadableValueChanged != null)
                {
                    onReadableValueChanged.Invoke(StickyInteractiveID, currentReadValue, prevReadValue, Vector3.zero);
                }
            }
        }

        /// <summary>
        /// Return the readable lever or joystick back to the original location
        /// </summary>
        /// <param name="isSmooth"></param>
        public virtual void RecentreReadable (bool isSmooth)
        {
            if (isReadable && isReadableReady && readablePivot != null)
            {
                if (isSmooth)
                {
                    isReadableSmoothDisable = true;
                }
                else
                {
                    isReadableSmoothDisable = false;
                    readablePivot.localRotation = initialPivotRotation;
                }
            }
            else
            {
                isReadableSmoothDisable = false;
            }
        }

        /// <summary>
        /// Initialise this Readable object.
        /// public override void ReInitialiseReadable()
        /// {
        ///    base.ReInitialiseReadable();
        ///    // Do stuff here
        /// }
        /// If required, you can override this method.
        /// </summary>
        public virtual void ReInitialiseReadable()
        {
            if (readablePivot != null)
            {
                initialPivotRotation = readablePivot.localRotation;
                //isHingeJoint = false;

                isReadablePivot = false;

                if (rBody != null || TryGetComponent(out rBody))
                {
                    isRBody = true;
                    rBody.isKinematic = true;

                    // Assume the lever has a parent
                    if (pivotTfrm == null && readablePivot.parent != null)
                    {
                        // This lets us track and use the original position and rotation of the joint, relative to the parent.
                        // If this is problematic, it might be possible to calculate it instead using initalJointRotation.
                        GameObject pivotGO = new GameObject("_Pivot");
                        if (pivotGO != null)
                        {
                            pivotTfrm = pivotGO.transform;
                            pivotTfrm.SetPositionAndRotation(readablePivot.position, readablePivot.rotation);
                            pivotTfrm.SetParent(readablePivot.parent);

                            // Get local space direction from the pivot point to the primary grab hand position
                            Vector3 targetDir = Quaternion.Inverse(pivotTfrm.rotation) * (GetHandHoldPosition(false) - pivotTfrm.position);

                            if (targetDir.y < 0.01f) { targetDir.y = 0.01f; }
                            if (targetDir.sqrMagnitude < Mathf.Epsilon) { targetDir = Vector3.up; }
                            else { targetDir = targetDir.normalized; }

                            // Forward-backward offset angle. With a vertical lever or joystick this will be 0 degrees.
                            pivotOffsetAngleZ = -Vector2.SignedAngle(new Vector2(targetDir.y, targetDir.z), Vector2.right);

                            // Left-right offset angle. Rotate around local z-axis on xy plane. This will be 0 deg for a vertical joystick.
                            pivotOffsetAngleX = Vector2.SignedAngle(new Vector2(targetDir.x, targetDir.y), Vector2.up);

                            isReadablePivot = true;
                        }
                    }
                }
                else
                {
                    isRBody = false;
                }

                isReadableReady = isReadablePivot;
            }
            else
            {
                // Reset all Readable values
                currentReadValue = Vector3.zero;
                prevReadValue = currentReadValue;

                isReadablePivot = false;
                isReadableReady = false;
            }
        }

        /// <summary>
        /// Currently interactive-enabled objects can be selected by multiple characters or things
        /// at the same time. The thing calling this method needs to keep track of if this object
        /// is selected or not. S3D characters do this automatically.
        /// Only initialised and selectable objects can be selected.
        /// public override void SelectObject (int stickyID, int selectedStoreItemID)
        /// {
        ///    base.SelectObject (int stickyID, int selectedStoreItemID);
        ///    // Do stuff here
        /// }
        /// If required, you can override this method.
        /// </summary>
        /// <param name="stickyID"></param>
        /// <param name="selectedStoreItemID"></param>
        public virtual void SelectObject (int stickyID, int selectedStoreItemID)
        {
            if (isInitialised && isSelectable)
            {
                // The event includes the ID of StickyInterative object, the character that selected it,
                // and the storeItemID of the items the character has selected in the scene.
                if (onSelected != null) { onSelected.Invoke(StickyInteractiveID, stickyID, selectedStoreItemID); }
            }
        }

        /// <summary>
        /// Which character (if any) has this interactive-enabled object
        /// NOTE: You need to set IsHeld or IsStashed separately.
        /// </summary>
        /// <param name="sticky3DCharacter"></param>
        public virtual void SetSticky3DCharacter (StickyControlModule sticky3DCharacter)
        {
            stickyControlModule = sticky3DCharacter;
        }

        /// <summary>
        /// Which socket (if any) is this interactive-enabled object attached to?
        /// NOTE: You need to set IsSocketed separately.
        /// </summary>
        /// <param name="stickySocket"></param>
        public virtual void SetStickySocket (StickySocket stickySocket)
        {
            stickySocketedOn = stickySocket;
        }

        /// <summary>
        /// This is automatically called by stickySocket.AddItem(..).
        /// Calls any stickySocket.onPreAdd event methods.
        /// </summary>
        /// <param name="stickySocket"></param>
        /// <returns></returns>
        public virtual bool SocketObject (StickySocket stickySocket)
        {
            bool socketedSuccessfully = false;

            if (stickySocket == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR StickyInteractive.SocketObject() - " + name + " cannot be socketed because the stickySocket is null");
                #endif
            }
            else if (!isInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR StickyInteractive.SocketObject() - " + stickySocket.name + " cannot add " + name + " because the interactive object is not initialised");
                #endif     
            }
            else if (!isSocketable)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR StickyInteractive.SocketObject() - " + stickySocket.name + " cannot add " + name + " because IsSocketable is false");
                #endif     
            }
            else if (isSocketed)
            {
                #if UNITY_EDITOR
                string alreadySocketedOn = stickySocketedOn == null ? "unknown socket" : stickySocketedOn.name;

                Debug.LogWarning("ERROR StickyInteractive.SocketObject() - " + stickySocket.name + " cannot add " + name + " because it is socketed on " + alreadySocketedOn);
                #endif 
            }
            else
            {
                int stickyID = Sticky3DCharacterID;
                bool isCanBeSocketed = true;

                // Fire any StickySocket onPreAdd event methods
                if (stickySocket.onPreAdd != null) { stickySocket.onPreAdd.Invoke(stickySocket.SocketID, stickyID, StickyInteractiveID); }

                // Automatically deactivate IsActivable object before it is attached to a socket
                if (isActivable && isActivated)
                {
                    DeactivateObject(stickyID);
                }

                // Is the object held, equipped or stashed by a Sticky3D character?
                if (stickyID != 0)
                {
                    if (isHeld || isEquipped)
                    {
                        // Detach active colliders from the character possessing the object.
                        // NOTE: We "may" need to do this during the finalise stage of socketing to avoid
                        // the character interacting with the colliders if not snapping to the socket position.
                        Collider[] attachedObjectColliders = colliders;
                        int numAttachedColliders = attachedObjectColliders == null ? 0 : attachedObjectColliders.Length;

                        for (int colIdx = 0; colIdx < numAttachedColliders; colIdx++)
                        {
                            Collider attachCollider = attachedObjectColliders[colIdx];
                            if (attachCollider != null && attachCollider.enabled) { stickyControlModule.DetachCollider(attachCollider.GetInstanceID()); }
                        }

                        // If socketing a held weapon, restore any animation that were changed when grabbed,
                        // stop aiming, and turn off things like lasersight and scope.
                        if (isHeld && IsStickyWeapon)
                        {
                            StickyWeapon weapon = (StickyWeapon)this;

                            weapon.TurnOffLaserSight();
                            weapon.StopAiming(false);
                            weapon.TurnOffScope();

                            weapon.AnimateCheckStateExitSocket();
                            weapon.RevertWeaponAnimSets();
                        }
                    }
                }

                if (isCanBeSocketed)
                {
                    // If there is a rigidbody, save the settings and remove it.
                    // Objects that were previously held, equipped, or stashed should not have a rigidbody.
                    if (isRBody && !origRBodySettings.isSaved)
                    {
                        origRBodySettings.SaveSettings(rBody);
                        RemoveRigidbody();
                    }

                    // If required, disable colliders based on StickySocket settings
                    if (numColliders > 0)
                    {
                        if (stickySocket.isDisableRegularColOnAdd) { EnableOrDisableNonTriggerColliders(false); }
                        if (stickySocket.isDisableTriggerColOnAdd) { EnableOrDisableTriggerColliders(false); }
                    }

                    socketedSuccessfully = true;
                }
            }

            return socketedSuccessfully;
        }

        /// <summary>
        /// This is automatically called by stickyControlModule.StashItem(..)
        /// </summary>
        /// <param name="sticky3DCharacter"></param>
        public virtual bool StashObject (StickyControlModule stashedBy)
        {
            bool stashedSuccessfully = false;

            if (!CheckCanBeStashed(stashedBy)) { }
            else if (isSocketed)
            {
                #if UNITY_EDITOR
                string alreadySocketedOn = stickySocketedOn == null ? "unknown socket" : stickySocketedOn.name;

                Debug.LogWarning("ERROR StickyInteractive.StashObject() - " + stashedBy.name + " cannot Stash " + name + " because it is socketed on " + alreadySocketedOn);
                #endif 
            }
            else
            {
                int stashedByStickyID = stashedBy.StickyID;
                bool isCanBeStashed = true;

                // Has this object already been grabbed?
                if (isHeld && isGrabbable)
                {
                    // Is it being held by another character?
                    // Currently characters cannot steal from one other
                    if (stickyControlModule.StickyID != stashedByStickyID)
                    {
                        isCanBeStashed = false;

                        #if UNITY_EDITOR
                        string alreadyHeldBy = stickyControlModule == null ? "unknown character" : stickyControlModule.name;

                        Debug.LogWarning("ERROR StickyInteractive.StashObject() - " + stashedBy.name + " cannot Stash " + name + " because it is already held by " + alreadyHeldBy);
                        #endif             
                    }
                    else
                    {
                        // Unregister the active colliders BEFORE item is stashed
                        Collider[] attachedObjectColliders = colliders;
                        int numAttachedColliders = attachedObjectColliders == null ? 0 : attachedObjectColliders.Length;

                        for (int colIdx = 0; colIdx < numAttachedColliders; colIdx++)
                        {
                            Collider attachCollider = attachedObjectColliders[colIdx];
                            if (attachCollider != null && attachCollider.enabled) { stashedBy.DetachCollider(attachCollider.GetInstanceID()); }
                        }

                        // If stashing a held weapon, restore any animation that were changed when grabbed,
                        // stop aiming, and turn off things like lasersight and scope.
                        if (IsStickyWeapon)
                        {
                            StickyWeapon weapon = (StickyWeapon)this;

                            weapon.TurnOffLaserSight();
                            weapon.StopAiming(false);
                            weapon.TurnOffScope();

                            weapon.AnimateCheckStateExitStash();
                            weapon.RevertWeaponAnimSets();
                        }
                    }
                }

                if (isCanBeStashed)
                {
                    // If there is a rigidbody, save the settings and remove it
                    if (isRBody && !origRBodySettings.isSaved)
                    {
                        origRBodySettings.SaveSettings(rBody);
                        RemoveRigidbody();
                    }

                    stashedSuccessfully = true;
                }

                isHeld = false;
                isEquipped = false;
                isSocketed = false;
            }

            return stashedSuccessfully;
        }

        /// <summary>
        /// Currently interactive-enabled objects can be selected by multiple characters or things
        /// at the same time. The thing calling this method needs to keep track of if this object
        /// is selected or not. S3D characters do this automatically.
        /// Only initialised objects can be unselected. It doesn't need to be selectable as that
        /// may have just been turned off.
        /// If required, you can override this method.
        /// public override void UnselectObject (int stickyID)
        /// {
        ///    base.UnselectObject (int stickyID);
        ///    // Do stuff here
        /// }
        /// </summary>
        /// <param name="stickyID"></param>
        /// <param name="selectedStoreItemID"></param>
        public virtual void UnselectObject (int stickyID)
        {
            if (isInitialised && !isAutoUnselect)
            {
                if (onUnselected != null) { onUnselected.Invoke(StickyInteractiveID, stickyID); }
            }
        }

        /// <summary>
        /// The interactive-enabled object has been touched at a point with a given normal.
        /// If required, you can override this method.
        /// public override void TouchObject (Vector3 hitPoint, Vector3 hitNormal, int stickyID)
        /// {
        ///    base.TouchObject (Vector3 hitPoint, Vector3 hitNormal, int stickyID);
        ///    // Do stuff here
        /// }
        /// </summary>
        /// <param name="hitPoint"></param>
        /// <param name="hitNormal"></param>
        /// <param name="stickyID"></param>
        public virtual void TouchObject (Vector3 hitPoint, Vector3 hitNormal, int stickyID)
        {
            if (isInitialised && isTouchable)
            {
                if (onTouched != null) { onTouched.Invoke(hitPoint, hitNormal, StickyInteractiveID, stickyID); }
            }
        }

        /// <summary>
        /// Stop touching this interactive-enabled object. Currently, multiple characters or things
        /// can be touching this object at the same time.
        /// If required, you can override this method.
        /// public override void StopTouchingObject (int stickyID)
        /// {
        ///    base.StopTouchingObject (int stickyID);
        ///    // Do stuff here
        /// }
        /// </summary>
        /// <param name="stickyID"></param>
        public virtual void StopTouchingObject (int stickyID)
        {
            if (isInitialised)
            {
                if (onStoppedTouching != null) { onStoppedTouching.Invoke(StickyInteractiveID, stickyID); }
            }
        }

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Add a rigidbody to the same gameobject.
        /// See also HasRigidbody and ObjectRigidbody properties.
        /// </summary>
        /// <param name="isKinematic">Make the new rigidbody kinematic</param>
        public void AddRigidbody (bool isKinematic)
        {
            // Ensure one doesn't already exist
            if (!isRBody && !gameObject.TryGetComponent(out rBody))
            {
                rBody = gameObject.AddComponent<Rigidbody>();
                if (rBody != null)
                {
                    isRBody = true;
                    rBody.isKinematic = isKinematic;
                }
                else
                {
                    isRBody = false;
                }
            }
        }

        /// <summary>
        /// Allocate or reserve a seat. This should be used to indicate the seat is taken or occupied.
        /// Use IsSeatAllocated property to get the current status.
        /// </summary>
        public void AllocateSeat()
        {
            isSeatAllocated = true;
        }

        /// <summary>
        /// Check if this interactive-enabled object can be equipped onto a EquipPoint on the given character
        /// </summary>
        /// <param name="equippedBy"></param>
        /// <param name="equippedAt"></param>
        /// <returns></returns>
        public bool CheckCanBeEquipped (StickyControlModule equippedBy, S3DEquipPoint equippedAt)
        {
            bool canBeEquipped = false;

            if (equippedBy == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR StickyInteractive.CheckCanBeEquipped() - " + name + " cannot be Equipped because the equippedBy character is null");
                #endif
            }
            else if (equippedAt == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR StickyInteractive.CheckCanBeEquipped() - " + name + " cannot be Equipped because the character S3DEquipPoint parameter is null");
                #endif
            }
            else if (!isInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR StickyInteractive.CheckCanBeEquipped() - " + equippedBy.name + " cannot Equip " + name + " because the interactive object is not initialised");
                #endif     
            }
            else if (!isEquippable)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR StickyInteractive.CheckCanBeEquipped() - " + equippedBy.name + " cannot Equip " + name + " because IsEquippable is false");
                #endif     
            }
            else if (isEquipped)
            {
                #if UNITY_EDITOR
                string alreadyEquippedBy = stickyControlModule == null ? "unknown character" : stickyControlModule.name;

                Debug.LogWarning("ERROR StickyInteractive.CheckCanBeEquipped() - " + equippedBy.name + " cannot Equip " + name + " because it is already stashed by " + alreadyEquippedBy);
                #endif 
            }
            else if ((interactiveTag & equippedAt.permittedTags) == 0)
            {
                #if UNITY_EDITOR
                // Display in the editor as a warning, because it may be checking multiple equip points.
                Debug.LogWarning("WARNING: StickyInteractive.CheckCanBeEquipped() " + equippedBy.name + " cannot Equip " + name + " on " + equippedAt.equipPointName + " because the interactive tag is not permitted on this Equip Point");
                #endif
            }
            else
            {
                canBeEquipped = true;
            }

            return canBeEquipped;
        }

        /// <summary>
        /// Check if this interactive-enabled object can be placed into the Stash (personal inventory) of the given character.
        /// NOTE: By design, it doesn't check if the item is currently attached to a StickySocket.
        /// </summary>
        /// <param name="stashedBy"></param>
        /// <returns></returns>
        public bool CheckCanBeStashed (StickyControlModule stashedBy)
        {
            bool canBeStashed = false;

            if (stashedBy == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR StickyInteractive.CheckCanBeStashed() - " + name + " cannot be Stashed because the stashedBy character is null");
                #endif
            }
            else if (!isInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR StickyInteractive.CheckCanBeStashed() - " + stashedBy.name + " cannot Stash " + name + " because the interactive object is not initialised");
                #endif     
            }
            else if (!isStashable)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR StickyInteractive.CheckCanBeStashed() - " + stashedBy.name + " cannot Stash " + name + " because IsStashable is false");
                #endif     
            }
            else if (isStashed)
            {
                #if UNITY_EDITOR
                string alreadyStashedBy = stickyControlModule == null ? "unknown character" : stickyControlModule.name;

                Debug.LogWarning("ERROR StickyInteractive.CheckCanBeStashed() - " + stashedBy.name + " cannot Stash " + name + " because it is already stashed by " + alreadyStashedBy);
                #endif 
            }
            else
            {
                canBeStashed = true;
            }

            return canBeStashed;
        }

        /// <summary>
        /// Typically called automatically when a character lasso operation completes.
        /// </summary>
        public void ClearLasso()
        {
            IsLassoEnabled = false;
            LassoSocket = null;
            LassoCharacter = null;
            LassoEquipPoint = null;
        }

        /// <summary>
        /// Deallocate or unreserve a seat. This should be used to indicate the seat is vacant.
        /// Use IsSeatAllocated property to get the current status.
        /// </summary>
        public void DeallocateSeat()
        {
            isSeatAllocated = false;
        }

        /// <summary>
        /// Disable all non-trigger colliders that were enabled during initialisation.
        /// </summary>
        public void DisableNonTriggerColliders()
        {
            EnableOrDisableNonTriggerColliders(false);
        }

        /// <summary>
        /// Disable all trigger colliders that were enabled during initialisation.
        /// </summary>
        public void DisableTriggerColliders()
        {
            EnableOrDisableTriggerColliders(false);
        }

        /// <summary>
        /// Enable all non-trigger colliders that were enabled during initialisation.
        /// </summary>
        public void EnableNonTriggerColliders()
        {
            EnableOrDisableNonTriggerColliders(true);
        }

        /// <summary>
        /// Enable all trigger colliders that were enabled during initialisation.
        /// </summary>
        public void EnableTriggerColliders()
        {
            EnableOrDisableTriggerColliders(true);
        }

        /// <summary>
        /// Return the Instance ID of the active StickyPopupModule.
        /// If there isn't one, return 0. See also SetActivePopupID(..).
        /// </summary>
        /// <returns></returns>
        public int GetActivePopupID()
        {
            return popupActiveInstanceID;
        }

        /// <summary>
        /// Get the local space hand hold offset. By default, uses the first or primary hand hold.
        /// </summary>
        /// <param name="isSecondaryHandHold"></param>
        /// <returns></returns>
        public Vector3 GetHandHoldLocalOffset (bool isSecondaryHandHold = false)
        {
            return isSecondaryHandHold ? handHold2Offset : handHold1Offset;
        }

        /// <summary>
        /// Get the local space hand hold rotation. By default, uses the first or primary hand hold.
        /// </summary>
        /// <param name="isSecondaryHandHold"></param>
        /// <returns></returns>
        public Quaternion GetHandHoldLocalRotation (bool isSecondaryHandHold = false)
        {
            return Quaternion.Euler(isSecondaryHandHold ? handHold2Rotation : handHold1Rotation);
        }

        /// <summary>
        /// Get the world space equip point position.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetEquipPosition()
        {
            return transform.position + (transform.rotation * equipOffset);
        }

        /// <summary>
        /// Get the world space equip point rotation.
        /// </summary>
        /// <returns></returns>
        public Quaternion GetEquipRotation()
        {
            return transform.rotation * Quaternion.Euler(equipRotation);
        }

        /// <summary>
        /// Get the world space hand hold position. By default, uses the first or primary hand hold.
        /// </summary>
        /// <param name="isSecondaryHandHold"></param>
        /// <returns></returns>
        public Vector3 GetHandHoldPosition (bool isSecondaryHandHold = false)
        {
            return transform.position + (transform.rotation * (isSecondaryHandHold ? handHold2Offset : handHold1Offset));
        }

        /// <summary>
        /// Get the world space hand hold rotation. By default, uses the first or primary hand hold.
        /// </summary>
        /// <param name="isSecondaryHandHold"></param>
        /// <returns></returns>
        public Quaternion GetHandHoldRotation (bool isSecondaryHandHold = false)
        {
            return transform.rotation * Quaternion.Euler(isSecondaryHandHold ? handHold2Rotation : handHold1Rotation);
        }

        /// <summary>
        /// Get the world space hand hold normal (or direction it is facing).
        /// By default, uses the first or primary hand hold.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetHandHoldNormal (bool isSecondaryHandHold = false)
        {
            return transform.rotation * Quaternion.Euler(isSecondaryHandHold ? handHold2Rotation : handHold1Rotation) * Vector3.forward;
        }

        /// <summary>
        /// Get the 32-bit mask used to determine compatibility with things like StickySockets or character Equip Points.
        /// Default is 1 << 0.
        /// </summary>
        public int GetInteractiveTag()
        {
            return interactiveTag;
        }

        /// <summary>
        /// Get the common interactive tags scriptableobject used to determine compatibility with things like StickySocket and character Equip Points
        /// </summary>
        public S3DInteractiveTags GetInteractiveTags()
        {
            return interactiveTags;
        }

        /// <summary>
        /// Get a local space position on the interactive-enabled object, given a world space position
        /// (converts a world space position to a local space position on the interactive-enabled object)
        /// </summary>
        /// <param name="wsPosition"></param>
        /// <returns></returns>
        public Vector3 GetLocalPosition (Vector3 wsPosition)
        {
            if (isInitialised && !isMovementDataStale) { return Quaternion.Inverse(trfmRot) * (wsPosition - trfmPos); }
            else { return Quaternion.Inverse(transform.rotation) * (wsPosition - transform.position); }
        }

        /// <summary>
        /// Get a local space rotation of a rotated object relative to the interactive-enabled object.
        /// (Converts a world space rotation to a local space rotation on the interactive-enabled object)
        /// </summary>
        /// <param name="wsRotation"></param>
        /// <returns></returns>
        public Quaternion GetLocalRotation (Quaternion wsRotation)
        {
            if (isInitialised && !isMovementDataStale) { return trfmInvRot * wsRotation; }
            else { return Quaternion.Inverse(transform.rotation) * wsRotation; }
        }

        /// <summary>
        /// Get the world space default popup position. Takes into consideration isPopupRelativeUp.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPopupPosition()
        {
            if (isPopupRelativeUp && isUseGravity)
            {
                // Local space offset use the forward and up direction of the reference frame
                if (currentReferenceFrameId != 0 && gravityModeInt == StickyManager.GravityModeRefFrameInt)
                {
                    return transform.position + (currentReferenceFrameRotation * defaultPopupOffset);
                }
                // Local space offset uses the forward direction of the popup, and the "up" vector of gravity
                else
                {
                    return transform.position + (Quaternion.LookRotation(transform.forward, -gravityDirection) * defaultPopupOffset);
                }
            }
            else { return transform.position + (transform.rotation * defaultPopupOffset); }
        }

        /// <summary>
        /// Get the world space sit offset position. This is the location the character
        /// should stand before attempting to sit down. The position may need to be adjusted
        /// if characters have a different radius from one another.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetSitOffsetPosition()
        {
            return transform.position + (transform.rotation * sitTargetOffset);
        }

        /// <summary>
        /// Get the world space socket attach point position.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetSocketPosition()
        {
            return transform.position + (transform.rotation * socketOffset);
        }

        /// <summary>
        /// Get the world space socket attach point rotation.
        /// </summary>
        /// <returns></returns>
        public Quaternion GetSocketRotation()
        {
            return transform.rotation * Quaternion.Euler(socketRotation);
        }

        /// <summary>
        /// Call this if you add or remove colliders at runtime
        /// </summary>
        public void ReinitialiseColliders()
        {
            // Get an array of all active colliders. Can be used when grabbing or dropping an object.
            colliders = GetComponentsInChildren<Collider>();
            numColliders = colliders == null ? 0 : colliders.Length;
        }

        /// <summary>
        /// Call this when you wish to remove any custom event listeners, like
        /// after creating them in code and then destroying the object.
        /// You could add this to your game play OnDestroy code.
        /// </summary>
        public void RemoveListeners()
        {
            if (isInitialised)
            {
                if (onDropped != null) { onDropped.RemoveAllListeners(); }
                if (onGrabbed != null) { onGrabbed.RemoveAllListeners(); }
                if (onHoverEnter != null) { onHoverEnter.RemoveAllListeners(); }
                if (onHoverExit != null) { onHoverExit.RemoveAllListeners(); }
                if (onPostEquipped != null) { onPostEquipped.RemoveAllListeners(); }
                if (onPostGrabbed != null) { onPostGrabbed.RemoveAllListeners(); }
                if (onPostStashed != null) { onPostStashed.RemoveAllListeners(); }
                if (onSelected != null) { onSelected.RemoveAllListeners(); }
                if (onStoppedTouching != null) { onStoppedTouching.RemoveAllListeners(); }
                if (onTouched != null) { onTouched.RemoveAllListeners(); }
                if (onUnselected != null) { onUnselected.RemoveAllListeners(); }
            }
        }

        /// <summary>
        /// If there is a rigidbody attached to the same gameobject, remove it.
        /// </summary>
        public void RemoveRigidbody()
        {
            if (rBody != null)
            {
                isRBody = false;
                Destroy(rBody);
            }
        }

        /// <summary>
        /// If there was a rigidbody attached to the object when it was grabbed or stashed, this will restore
        /// those original settings.
        /// After the rigidbody settings are restored, they are now considered stale.
        /// </summary>
        public void RestoreRigidbodySettings()
        {
            if (isInitialised && origRBodySettings.isSaved)
            {
                origRBodySettings.RestoreSettings(rBody);

                origRBodySettings.isSaved = false;
            }
        }

        /// <summary>
        /// If there is a previous parent transform recorded, re-parent
        /// the object to that transform.
        /// </summary>
        public void RestoreParent()
        {
            if (preGrabParentTfrm != null)
            {
                transform.SetParent(preGrabParentTfrm);
                // Reset after use
                preGrabParentTfrm = null;
            }
        }

        /// <summary>
        /// Set the instance ID of the active StickyPopupModule currently being displayed.
        /// See also GetActivePopupIDs().
        /// </summary>
        /// <param name="instanceID"></param>
        public void SetActivePopupID (int instanceID)
        {
            popupActiveInstanceID = instanceID;
        }

        /// <summary>
        /// When the object is dropped, the currently velocity of the object is applied
        /// to the rigidbody (if one is present). This applies more or less velocity to
        /// the object.
        /// </summary>
        /// <param name="throwStrength"></param>
        public void SetExternalThrowStrength (float throwStrength)
        {
            externalThrowStrength = throwStrength;
        }

        /// <summary>
        /// Grabbable objects with a rigidbody which are not parented to the hand on grab,
        /// can follow a child transform of the hand.
        /// Typically this is called automatically when the object is grabbed and isParentOnGrab
        /// is not set.
        /// </summary>
        /// <param name="newTargetToFollow"></param>
        public void SetFollowTarget (Transform newTargetToFollow)
        {
            if (isInitialised && isGrabbable && !isParentOnGrab && rBody != null)
            {
                followTarget = newTargetToFollow;
                isFollowingTarget = followTarget != null;
                isReadableSmoothDisable = false;
            }
        }

        /// <summary>
        /// Set the hand hold local space offset. By default, sets the primary hand hold offset.
        /// </summary>
        /// <param name="handHoldOffset"></param>
        /// <param name="isSecondaryHandHold"></param>
        public void SetHandHoldOffset (Vector3 handHoldOffset, bool isSecondaryHandHold = false)
        {
            if (isSecondaryHandHold) { handHold2Offset = handHoldOffset; }
            else { handHold1Offset = handHoldOffset; }
        }

        /// <summary>
        /// Set the hand hold relative rotation. The rotation is stored as Euler angles (degrees).
        /// By default, sets the primary hand hold rotation.
        /// </summary>
        /// <param name="handHoldRotation"></param>
        /// <param name="isSecondaryHandHold"></param>
        public void SetHandHoldRotation (Vector3 handHoldRotation, bool isSecondaryHandHold = false)
        {
            if (isSecondaryHandHold) { handHold2Rotation = handHoldRotation; }
            else { handHold1Rotation = handHoldRotation; }
        }

        /// <summary>
        /// Set the 32-bit mask used to determine compatibility with things like StickySockets or character Equip Points.
        /// Default is 1 << 0.
        /// </summary>
        /// <param name="bitMask"></param>
        public void SetInteractiveTag (int bitMask)
        {
            interactiveTag = bitMask;
        }

        /// <summary>
        /// Set the common interactive tags scriptableobject used to determine compatibility with things like StickySocket and character Equip Points
        /// </summary>
        /// <param name="newInteractiveTags"></param>
        public void SetInteractiveTags (S3DInteractiveTags newInteractiveTags)
        {
            interactiveTags = newInteractiveTags;
        }

        /// <summary>
        /// Set this interactive-enabled object to be activable or not.
        /// If it is turned off, OnDeactivated events are not triggered.
        /// </summary>
        /// <param name="activable"></param>
        public void SetIsActivable (bool activable)
        {
            isActivable = activable;

            // If this feature is turned off, make sure it is not activated.
            if (!isActivable) { isActivated = false; }
        }

        /// <summary>
        /// Set this interactive-enabled object to be auto deactivated or not in the scene
        /// </summary>
        /// <param name="selectable"></param>
        public void SetIsAutoDeactivate(bool deactivate)
        {
            isAutoDeactivate = deactivate;
        }

        /// <summary>
        /// Set this interactive-enabled object to be auto unselected or not in the scene
        /// </summary>
        /// <param name="selectable"></param>
        public void SetIsAutoUnselect (bool unselect)
        {
            isAutoUnselect = unselect;
        }

        /// <summary>
        /// Set this interactive-enabled object to be equippable or not
        /// </summary>
        /// <param name="equippable"></param>
        public void SetIsEquippable (bool equippable)
        {
            isEquippable = equippable;
        }

        /// <summary>
        /// Set this interactive-enabled object to be grabble or not
        /// </summary>
        /// <param name="equippable"></param>
        public void SetIsGrabbable (bool grabbable)
        {
            isGrabbable = grabbable;
        }

        /// <summary>
        /// Set this interactive-enabled object to be readable or not
        /// </summary>
        /// <param name="readable"></param>
        public void SetIsReadable (bool readable)
        {
            isReadable = readable;
        }

        /// <summary>
        /// Set this interactive-enabled object to be selectable or not in the scene
        /// </summary>
        /// <param name="selectable"></param>
        public void SetIsSelectable (bool selectable)
        {
            isSelectable = selectable;
        }

        /// <summary>
        /// Set this interactive-enabled object is suitable for sitting on or not.
        /// </summary>
        /// <param name="selectable"></param>
        public void SetIsSittable (bool sittable)
        {
            isSittable = sittable;
        }

        /// <summary>
        /// Set this interactive-enabled object to be attachable to a StickySocket or not.
        /// </summary>
        /// <param name="socketable"></param>
        public void SetIsSocketable (bool socketable)
        {
            isSocketable = socketable;
        }

        /// <summary>
        /// Set this interactive-enabled object to stashable on or not.
        /// </summary>
        /// <param name="selectable"></param>
        public void SetIsStashable (bool stashable)
        {
            isStashable = stashable;
        }

        /// <summary>
        /// Set this interactive-enabled object to be touchable or not by a character.
        /// Typically used with Hand IK.
        /// </summary>
        /// <param name="touchable"></param>
        public void SetIsTouchable (bool touchable)
        {
            isTouchable = touchable;
        }

        /// <summary>
        /// Change the amount of time the configured event methods are called
        /// after the component is initialised. This will have no effect after
        /// the component is initialised.
        /// </summary>
        /// <param name="newValue"></param>
        public void SetOnPostInitialisedEvtDelay(float newValue)
        {
            if (newValue >= 0f && newValue < 30f)
            {
                onPostInitialisedEvtDelay = newValue;
            }
        }

        /// <summary>
        /// Set the pivot point to read positional data from when Is Readable is true.
        /// </summary>
        /// <param name="newPivotTransform"></param>
        public void SetReadablePivot (Transform newPivotTransform)
        {
            readablePivot = newPivotTransform;
            ReInitialiseReadable();
        }

        /// <summary>
        /// Set the mass of the object in kilograms
        /// </summary>
        /// <param name="newMass"></param>
        public void SetMass(float newMass)
        {
            if (newMass > 0f)
            {
                mass = newMass;
            }
        }

        /// <summary>
        /// Parent this object to another transform. If Reparent on Drop
        /// is enabled, remember the original transform.
        /// </summary>
        /// <param name="parentTfrm"></param>
        public void SetObjectParent (Transform parentTfrm)
        {
            if (isReparentOnDrop)
            {
                preGrabParentTfrm = transform.parent;
            }
            else { preGrabParentTfrm = null; }

            transform.SetParent(parentTfrm, true);
        }

        /// <summary>
        /// Attempt to Activate or Deactivate an interactive-enabled
        /// object that isActivable
        /// </summary>
        public void ToggleActivateObject (int stickyID)
        {
            if (isActivated) { DeactivateObject(stickyID); }
            else { ActivateObject(stickyID); }
        }

        #endregion

        #region Public API Methods - Gravity

        /// <summary>
        /// If the object is held or stashed, and using a Gravity Mode of Reference Frame, return the current reference frame of the character.
        /// </summary>
        /// <returns></returns>
        public Transform GetCharacterReferenceFrame ()
        {
            if ((isHeld || isStashed) && isUseGravity && gravityModeInt == StickyManager.GravityModeRefFrameInt && stickyControlModule != null)
            {
                return stickyControlModule.GetCurrentReferenceFrame;
            }
            else { return null; }
        }

        /// <summary>
        /// Reset gravity to default (starting) values.
        /// Typically gets called automatically when required.
        /// </summary>
        public void ResetGravity()
        {
            gravitationalAcceleration = defaultGravitationalAcceleration;
            gravityDirection = defaultGravityDirection;
        }

        /// <summary>
        /// If gravity was previously in use, restore it.
        /// NOTE: This is not part of IStickyGravity and only applies to StickyInteractive objects
        /// </summary>
        public void RestoreGravity()
        {
            if (isUseGravity)
            {
                SetUpRigidbody();
                RestoreRigidbodySettings();
            }
        }

        /// <summary>
        /// Attempt to restore the current reference frame, to the initial or default setting.
        /// This is automatically called when exiting a StickyZone.
        /// </summary>
        public virtual void RestoreDefaultReferenceFrame()
        {
            SetCurrentReferenceFrame(initialReferenceFrame);
        }

        /// <summary>
        /// Attempt to restore the previous reference frame that was being used before what is
        /// currently set. NOTE: We do not support nesting.
        /// </summary>
        public virtual void RestorePreviousReferenceFrame()
        {
            SetCurrentReferenceFrame(previousReferenceFrame);
        }

        /// <summary>
        /// Sets the current reference frame.
        /// </summary>
        /// <param name="newReferenceFrame"></param>
        public virtual void SetCurrentReferenceFrame (Transform newReferenceFrame)
        {
            if (newReferenceFrame != null)
            {
                // Only update if the reference frame has changed
                if (currentReferenceFrameId != newReferenceFrame.GetHashCode())
                {
                    previousReferenceFrame = currentReferenceFrame;

                    // Calculate the new relative position and rotation
                    // The INTENT is to maintain the same forward direction of the interactive object
                    // as it crosses reference frame boundaries. These reference object may
                    // appear on be on a similar plane but be rotated say 90, 180 or 270 deg.
                    // We also, if possible want the character up direction to match the normal
                    // or new reference object.                    

                    // NOTE: CURRENTLY DOESN'T WORK WTIH SCALED MESHES
                    //currentRelativePosition = newReferenceFrame.InverseTransformPoint(transform.position);
                    currentReferenceFramePosition = Quaternion.Inverse(newReferenceFrame.rotation) * (transform.position - newReferenceFrame.position);

                    // Project the relative rotation into the new reference frame
                    currentRelativeRotation = Quaternion.Inverse(newReferenceFrame.rotation) * transform.rotation;

                    // Set the current reference frame transform
                    currentReferenceFrame = newReferenceFrame;

                    // Update reference frame data
                    currentReferenceFramePosition = currentReferenceFrame.position;
                    currentReferenceFrameRotation = currentReferenceFrame.rotation;
                    currentReferenceFrameUp = currentReferenceFrame.up;

                    // GetHashCode seems to return same value as GetInstanceID()
                    // GetHashCode is faster than GetInstanceID() when doing comparisons.
                    currentReferenceFrameId = currentReferenceFrame.GetHashCode();
                }
            }
            else
            {
                // Calculate the new relative position and rotation
                currentRelativePosition = Vector3.zero;
                currentRelativeRotation = Quaternion.identity;

                // Currently we do not have a reference frame assigned
                currentReferenceFrame = null;

                // Update reference frame data
                currentReferenceFramePosition = Vector3.zero;
                currentReferenceFrameRotation = Quaternion.identity;
                currentReferenceFrameUp = Vector3.up;

                currentReferenceFrameId = 0;
            }
        }

        /// <summary>
        /// Set the gravity in metres per second per second.
        /// </summary>
        /// <param name="newAcceleration">0 or greater. Earth gravity 9.81.</param>
        public void SetGravitationalAcceleration (float newAcceleration)
        {
            gravitationalAcceleration = newAcceleration >= 0 ? gravitationalAcceleration : -newAcceleration;
        }

        /// <summary>
        /// Set the world space direction that gravity acts upon the dynamic object when GravityMode is Direction. 
        /// </summary>
        /// <param name="newDirection"></param>
        public void SetGravityDirection (Vector3 newDirection)
        {
            if (newDirection.sqrMagnitude < Mathf.Epsilon)
            {
                gravityDirection = Vector3.forward;
            }
            else
            {
                gravityDirection = newDirection.normalized;
            }
        }

        /// <summary>
        /// Set the method used to determine in which direction gravity is acting.
        /// </summary>
        /// <param name="newGravityMode"></param>
        public void SetGravityMode (StickyManager.GravityMode newGravityMode)
        {
            gravityMode = newGravityMode;
            gravityModeInt = (int)gravityMode;

            if (isInitialised && isRBody) { rBody.useGravity = gravityModeInt == StickyManager.GravityModeUnityInt; }
        }

        /// <summary>
        /// Change the way reference frames are determined.
        /// </summary>
        /// <param name="newRefUpdateType"></param>
        public void SetReferenceUpdateType (StickyControlModule.ReferenceUpdateType newRefUpdateType)
        {
            #if UNITY_EDITOR
            Debug.LogWarning("StickyInteractive - SetReferenceUpdateType is currently not implemented");
            #endif
        }

        #endregion
    }
}

