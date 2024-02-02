using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Component to control the docking and undocking of ships on larger ships (motherships)
    /// or stationary objects like hangars. Works with ShipDockingStation.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Docking Components/Ship Docking")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    [RequireComponent(typeof(ShipControlModule))]
    public class ShipDocking : MonoBehaviour
    {
        #region Enumerations

        /// <summary>
        /// The possible states of a ship which supports docking.
        /// </summary>
        public enum DockingState
        {
            /// <summary>
            /// Ship is not docked and can fly around unhindered
            /// </summary>
            NotDocked = 0,
            /// <summary>
            /// Ship is currently attempting to dock at a docking point on a Ship Docking Station.
            /// It may be assigned to a docking point Entry Path.
            /// </summary>
            Docking = 1,
            /// <summary>
            /// Ship is currently attempting to depart from a docking point on a Ship Docking station.
            /// It may be assigned to a docking point Exit Path.
            /// </summary>
            Undocking = 2,
            /// <summary>
            /// Ship is docked at a docking point on a Ship Docking Station. Typically it wll be set
            /// to kinematic.
            /// </summary>
            Docked = 3
        }

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// This is a subset of DockingState
        /// </summary>
        public enum InitialDockingState
        {
            NotDocked = 0,
            Docked = 3
        }

        /// <summary>
        /// The axes to snap to when docking.
        /// </summary>
        public enum DockSnapTo
        {
            AllAxes = 0,
            XY = 1,
            XZ = 2,
            YZ = 3,
            X = 4,
            Y = 5,
            Z = 6,
            None = 7
        }

        #endregion

        #region Public Static Properties

        // variables to avoid enumeration lookups
        public static readonly int notDockedInt = (int)DockingState.NotDocked;
        public static readonly int dockingInt = (int)DockingState.Docking;
        public static readonly int undockingInt = (int)DockingState.Undocking;
        public static readonly int dockedInt = (int)DockingState.Docked;

        public static readonly int dockSnapToAllInt = (int)DockSnapTo.AllAxes;
        public static readonly int dockSnapToXYInt = (int)DockSnapTo.XY;
        public static readonly int dockSnapToXZInt = (int)DockSnapTo.XZ;
        public static readonly int dockSnapToYZInt = (int)DockSnapTo.YZ;
        public static readonly int dockSnapToXInt = (int)DockSnapTo.X;
        public static readonly int dockSnapToYInt = (int)DockSnapTo.Y;
        public static readonly int dockSnapToZInt = (int)DockSnapTo.Z;
        public static readonly int dockSnapToNoneInt = (int)DockSnapTo.None;

        #endregion

        #region Public Variables - General

        /// <summary>
        /// If enabled, the Initialise() will be called as soon as Awake() runs. This should be disabled if you are
        /// instantiating the ShipDocking through code.
        /// </summary>
        public bool initialiseOnAwake = false;

        /// <summary>
        /// Ships can start in a state of Docked or Undocked.
        /// </summary>
        public InitialDockingState initialDockingState = InitialDockingState.NotDocked;

        /// <summary>
        /// How close the ship has to be (in metres) to the docking position before it can become docked.
        /// </summary>
        public float landingDistancePrecision = 0.01f;

        [Range(0.1f, 10f)]
        /// <summary>
        /// How close the ship has to be (in degrees) to the docking rotation before it can become docked.
        /// </summary>
        public float landingAnglePrecision = 2f;

        /// <summary>
        /// How close the ship has to be (in metres) to the hovering position before it is deemed to have reached the hover position.
        /// </summary>
        public float hoverDistancePrecision = 1f;

        /// <summary>
        /// How close the ship has to be (in degrees) to the hovering rotation before it is deemed to have reached the hover position.
        /// </summary>
        [Range(0.1f, 30f)]
        public float hoverAnglePrecision = 10f;

        /// <summary>
        /// Target time to lift off from the landing position and move to the hover position.
        /// Has no effect if the docking point hover height is 0.
        /// </summary>
        [Range(0f, 60f)]
        public float liftOffDuration = 2f;

        /// <summary>
        /// Target time to move from the hover position to the landing position.
        /// Has no effect if the docking point hover height is 0.
        /// </summary>
        [Range(0f, 60f)]
        public float landingDuration = 2f;

        /// <summary>
        /// Should physics collisions been detected when the state is Docked?
        /// </summary>
        public bool detectCollisionsWhenDocked = false;

        /// <summary>
        /// When used with ShipDockingStation.UndockShip(..), the number of seconds
        /// that the undocking manoeuvre is delayed. This allows you to create cinematic
        /// effects or perform other actions, before the Undocking process begins.
        /// </summary>
        [Range(0f, 60f)] public float undockingDelay = 0f;

        /// <summary>
        /// When value is greater than 0, the number of seconds the ship waits while docked,
        /// before automatically attempting to start the undocking procedure.
        /// </summary>
        [Range(0f, 300f)] public float autoUndockTime = 0f;

        /// <summary>
        /// This is additional velocity in an upwards direction relative to the mothership.
        /// </summary>
        public float undockVertVelocity = 2f;

        /// <summary>
        /// This is additional velocity in a forward direction relative to the mothership.
        /// </summary>
        public float undockFwdVelocity = 2f;

        /// <summary>
        /// The amount of force applied by the catapult when undocking in KiloNewtons.
        /// </summary>
        public float catapultThrust = 0f;

        /// <summary>
        /// The number of seconds that the force is applied from the catapult to the ship
        /// </summary>
        [Range(0f, 30f)] public float catapultDuration = 2f;

        /// <summary>
        /// A list of ship docking adapters. These are points on the ship where
        /// it can dock with a ShipDockingPoint on a ShipDockingStation.
        /// Typically you should not be updating this list yourself.
        /// </summary>
        public List<ShipDockingAdapter> adapterList;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [HideInInspector] public bool allowRepaint = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Is the adapter list expanded in the ShipDockingEditor?
        /// </summary>
        public bool isAdapterListExpanded = false;

        #endregion

        #region Public Variables - Events

        /// <summary>
        /// Methods that get called immediately after the ship has finished docking.
        /// </summary>
        public SSCDockingEvt1 onPostDocked = null;

        /// <summary>
        /// The time, in seconds, to delay the actioning of any onPostDocked methods.
        /// </summary>
        [Range(0f, 30f)] public float onPostDockedDelay = 0f;

        /// <summary>
        /// Methods that get called immediately after the Hover point is reached when docking.
        /// Typically used to perform a non-docking API action like lowering landing gear,
        /// disabling radar, disarming weapons etc.
        /// WARNING: Be careful not to call other docking APIs that might create a circular loop.
        /// </summary>
        public SSCDockingEvt1 onPostDockingHover = null;

        /// <summary>
        /// The time, in seconds, to delay the actioning of any onPostDockingHover methods.
        /// </summary>
        [Range(0f, 30f)] public float onPostDockingHoverDelay = 0f;

        /// <summary>
        /// Methods that get called immediately after docking has started.
        /// Typically used to perform a non-docking API action like chatter with ground staff,
        /// disabling radar, preparing for landing etc.
        /// WARNING: Be careful not to call other docking APIs that might create a circular loop.
        /// </summary>
        public SSCDockingEvt1 onPostDockingStart = null;

        /// <summary>
        /// The time, in seconds, to delay the actioning of any onPostDockingStart methods.
        /// </summary>
        [Range(0f, 30f)] public float onPostDockingStartDelay = 0f;

        /// <summary>
        /// Methods that get called immediately after the ship has finished undocking.
        /// </summary>
        public SSCDockingEvt1 onPostUndocked = null;

        /// <summary>
        /// The time, in seconds, to delay the actioning of any onPostUndocked methods.
        /// </summary>
        [Range(0f, 30f)] public float onPostUndockedDelay = 0f;

        /// <summary>
        /// Methods that get called immediately after the Hover point is reached when undocking.
        /// Typically used to perform a non-docking API action like raising landing gear, enabling
        /// radar, arming weapons etc.
        /// WARNING: Be careful not to call other docking APIs that might create a circular loop.
        /// </summary>
        public SSCDockingEvt1 onPostUndockingHover = null;

        /// <summary>
        /// The time, in seconds, to delay the actioning of any onPostUndockingHover methods.
        /// </summary>
        [Range(0f, 30f)] public float onPostUndockingHoverDelay = 0f;

        /// <summary>
        /// Methods that get called immediately after undocking has started.
        /// Typically used to perform a non-docking API action like dust or steam particle effects
        /// or opening hanger doors.
        /// WARNING: Be careful not to call other docking APIs that might create a circular loop.
        /// </summary>
        public SSCDockingEvt1 onPostUndockingStart = null;

        /// <summary>
        /// The time, in seconds, to delay the actioning of any onPostUndockingStart methods.
        /// </summary>
        [Range(0f, 30f)] public float onPostUndockingStartDelay = 0f;

        #endregion

        #region Public Properties

        /// <summary>
        /// [READ ONLY] The ID or the docking point on the shipDockingStation.
        /// To set, call shipDockingStation.AssignShipToDockingPoint(..).
        /// Internally uses the index in the list of docking points - however,
        /// this is subject to change.
        /// </summary>
        public int DockingPointId { get; internal set; }

        /// <summary>
        /// Typically used for debugging, is the Hover Point the target?
        /// </summary>
        public bool IsHoverPointTarget { get { return isHoverTarget; } }

        /// <summary>
        /// Is the docking component initialised?
        /// </summary>
        public bool IsInitialised { get; private set; }

        /// <summary>
        /// [READ ONLY] The docking station the ship may be docked with.
        /// To set, call shipDockingStation.AssignShipToDockingPoint(..)
        /// </summary>
        public ShipDockingStation shipDockingStation { get; internal set; }

        #endregion

        #region Public Delegates

        public delegate void CallbackOnStateChange(ShipDocking shipDocking, ShipControlModule shipControlModule, ShipAIInputModule shipAIInputModule, DockingState previousDockingState);

        /// <summary>
        /// The name of the custom method that is called immediately after the state is changed.
        /// Your method must take 4 parameters: shipDocking (never null), shipControlModule (never null),
        /// shipAIInputModule (could be null) and previousDockingState.
        /// This should be a lightweight method to avoid performance issues.
        /// Your method will NOT be called if ShipDocking.IsInitialised is false.
        /// </summary>
        public CallbackOnStateChange callbackOnStateChange = null;

        #endregion

        #region Private variables

        /// <summary>
        /// The position axes to snap to when a ship gets close to the docking point, and becomes Docked.
        /// The snap amount can be affected by the Landing Distance Precision.
        /// </summary>
        [SerializeField] private DockSnapTo dockSnapToPos = DockSnapTo.AllAxes;

        /// <summary>
        /// The rotation axes to snap to when a ship gets close to the docking point, and becomes Docked.
        /// The snap amount can be affected by the Landing Angle Precision.
        /// </summary>
        [SerializeField] private DockSnapTo dockSnapToRot = DockSnapTo.AllAxes;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Remember which tabs etc were shown in the editor
        /// </summary>
        [SerializeField] private int selectedTabInt = 0;

        private ShipControlModule shipControlModule;
        private ShipAIInputModule shipAIInputModule;
        private bool isShipInitialised = false;
        internal ShipControlModule dockWithShip;
        private SSCManager sscManager;

        /// <summary>
        /// The current state of the ship
        /// </summary>
        private DockingState dockingState = DockingState.NotDocked;
        private int dockingStateInt = 0;

        // TODO: CHECK USAGE
        private Vector3 dockedRelativePosition;
        private Quaternion dockedRelativeRotation;

        internal RigidbodyInterpolation originalRBInterpolation = RigidbodyInterpolation.None;
        internal bool isOriginalRBInterpolationSet = false;

        private Vector3 targetDockingPosition = Vector3.zero;
        private Quaternion targetDockingRotation = Quaternion.identity;

        private bool isInAIDockingState = false;
        private bool isHoverTarget = false;
        private float dockingActionCompletedDistance = 1f;
        private float dockingActionCompletedAngle = 1f;

        private float autoUndockTimer = 0f;

        private int dockSnapToPosInt = 0;
        private int dockSnapToRotInt = 0;

        private ShipAIInputModule.CallbackCompletedStateAction originalCompletedStateActionCallback = null;

        #endregion

        #region Initialisation Methods

        void Awake()
        {
            if (initialiseOnAwake) { Initialise(); }
        }

        public void Initialise()
        {
            if (!IsInitialised)
            {
                shipControlModule = GetComponent<ShipControlModule>();
                if (shipControlModule != null)
                {
                    shipAIInputModule = GetComponent<ShipAIInputModule>();

                    // cache to avoid having to check for null etc in FixedUpdate
                    isShipInitialised = shipControlModule.IsInitialised;
                }

                // Add capacity for 1 docking adapter as this is the current default.
                if (adapterList == null) { adapterList = new List<ShipDockingAdapter>(1); }
                if (adapterList != null && adapterList.Count == 0)
                {
                    adapterList.Add(new ShipDockingAdapter());
                }

                dockSnapToPosInt = (int)dockSnapToPos;
                dockSnapToRotInt = (int)dockSnapToRot;

                // Keep compiler happy
                if (selectedTabInt < 0) { }

                IsInitialised = true;
            }
        }

        #endregion

        #region Event Methods

        /// <summary>
        /// Automatically called by Unity immediately before the object is destroyed
        /// </summary>
        private void OnDestroy()
        {
            RemoveListeners();
            CancelInvoke();
            StopAllCoroutines();
        }

        #endregion

        #region Update Methods

        private void Update ()
        {
            if (isShipInitialised)
            {
                // Is docked?
                if (dockingStateInt == dockedInt)
                {
                    // IsKinematic should be enabled because ship is disabled
                    if (shipControlModule.ShipRigidbody.isKinematic)
                    {
                        if (dockWithShip != null && dockWithShip.ShipIsEnabled())
                        {
                            // Translate the relative local-space offset of the docked ship
                            // Update the rotation of the docked ship by adding the local rotation of the docked ship relative to the mother ship.
                            transform.SetPositionAndRotation(
                                dockWithShip.transform.position + dockWithShip.transform.TransformDirection(dockedRelativePosition),
                                dockWithShip.transform.rotation * dockedRelativeRotation);
                        }
                    }
                }

                // Is this a moving ship docking station?
                if (isInAIDockingState && shipDockingStation != null && shipDockingStation.IsMotherShip)
                {
                    UpdateDockingWSPositionAndRotation(shipDockingStation.GetDockingPoint(DockingPointId),
                        shipAIInputModule, isHoverTarget, dockingActionCompletedDistance, dockingActionCompletedAngle);
                }

                // Auto undocking - do last
                // Check if auto undock is active (gets activated at end of SetState(..)
                if (autoUndockTime > 0f && dockingStateInt == dockedInt && autoUndockTimer > 0f)
                {
                    autoUndockTimer += Time.deltaTime;

                    if (autoUndockTimer > autoUndockTime)
                    {
                        // Reset time (auto undock is not active)
                        autoUndockTimer = 0f;

                        shipDockingStation.UnDockShip(shipControlModule);
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        private bool IsShipDocked()
        {
            if (dockingStateInt == dockedInt)
            {
                // May be other conditions...

                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// Assign a new path to an AI Ship.
        /// If there is no matching path, then this will return false.
        /// </summary>
        /// <param name="pathGUIDHash"></param>
        private bool AssignNewPath(int pathGUIDHash)
        {
            bool isAssigned = false;

            if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(gameObject.scene.handle); }
            if (sscManager != null)
            {
                PathData pathData = sscManager.GetPath(pathGUIDHash);

                if (pathData != null)
                {
                    // Set the ship's state to the "move to" state
                    shipAIInputModule.SetState(AIState.moveToStateID);
                    // Set the target path to the new path
                    shipAIInputModule.AssignTargetPath(pathData);
                    isAssigned = true;
                }
            }
            return isAssigned;
        }

        /// <summary>
        /// Invoke any methods configured in the editor (persistent) or with AddListener (non-persistent)
        /// after the delayTime in seconds.
        /// </summary>
        /// <param name="shipDockingStationID"></param>
        /// <param name="shipId"></param>
        /// <param name="dockingPointId"></param>
        /// <param name="delayTime"></param>
        /// <returns></returns>
        private IEnumerator OnPostDockedDelayed (int shipDockingStationID, int shipId, int dockingPointId, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            // These will ALWAYS fire although events are checked before calling this method.
            if (onPostDocked != null)
            {
                onPostDocked.Invoke(shipDockingStationID, shipId, dockingPointId, Vector3.zero);
            }
        }

        /// <summary>
        /// Invoke any methods configured in the editor (persistent) or with AddListener (non-persistent)
        /// after the delayTime in seconds.
        /// </summary>
        /// <param name="shipDockingStationID"></param>
        /// <param name="shipId"></param>
        /// <param name="dockingPointId"></param>
        /// <param name="delayTime"></param>
        /// <returns></returns>
        private IEnumerator OnPostDockingHoverDelayed (int shipDockingStationID, int shipId, int dockingPointId, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            // These will ALWAYS fire although events are checked before calling this method.
            if (onPostDockingHover != null)
            {
                onPostDockingHover.Invoke(shipDockingStationID, shipId, dockingPointId, Vector3.zero);
            }
        }

        /// <summary>
        /// Invoke any methods configured in the editor (persistent) or with AddListener (non-persistent)
        /// after the delayTime in seconds.
        /// </summary>
        /// <param name="shipDockingStationID"></param>
        /// <param name="shipId"></param>
        /// <param name="dockingPointId"></param>
        /// <param name="delayTime"></param>
        /// <returns></returns>
        private IEnumerator OnPostDockingStartDelayed (int shipDockingStationID, int shipId, int dockingPointId, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            // These will ALWAYS fire although events are checked before calling this method.
            if (onPostDockingStart != null)
            {
                onPostDockingStart.Invoke(shipDockingStationID, shipId, dockingPointId, Vector3.zero);
            }
        }

        /// <summary>
        /// Invoke any methods configured in the editor (persistent) or with AddListener (non-persistent)
        /// after the delayTime in seconds.
        /// </summary>
        /// <param name="shipDockingStationID"></param>
        /// <param name="shipId"></param>
        /// <param name="dockingPointId"></param>
        /// <param name="delayTime"></param>
        /// <returns></returns>
        private IEnumerator OnPostUndockedDelayed (int shipDockingStationID, int shipId, int dockingPointId, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            // These will ALWAYS fire although events are checked before calling this method.
            if (onPostUndocked != null)
            {
                onPostUndocked.Invoke(shipDockingStationID, shipId, dockingPointId, Vector3.zero);
            }
        }

        /// <summary>
        /// Invoke any methods configured in the editor (persistent) or with AddListener (non-persistent)
        /// after the delayTime in seconds.
        /// </summary>
        /// <param name="shipDockingStationID"></param>
        /// <param name="shipId"></param>
        /// <param name="dockingPointId"></param>
        /// <param name="delayTime"></param>
        /// <returns></returns>
        private IEnumerator OnPostUndockingHoverDelayed (int shipDockingStationID, int shipId, int dockingPointId, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            // These will ALWAYS fire although events are checked before calling this method.
            if (onPostUndockingHover != null)
            {
                onPostUndockingHover.Invoke(shipDockingStationID, shipId, dockingPointId, Vector3.zero);
            }
        }

        /// <summary>
        /// Invoke any methods configured in the editor (persistent) or with AddListener (non-persistent)
        /// after the delayTime in seconds.
        /// </summary>
        /// <param name="shipDockingStationID"></param>
        /// <param name="shipId"></param>
        /// <param name="dockingPointId"></param>
        /// <param name="delayTime"></param>
        /// <returns></returns>
        private IEnumerator OnPostUndockingStartDelayed (int shipDockingStationID, int shipId, int dockingPointId, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            // These will ALWAYS fire although events are checked before calling this method.
            if (onPostUndockingStart != null)
            {
                onPostUndockingStart.Invoke(shipDockingStationID, shipId, dockingPointId, Vector3.zero);
            }
        }

        /// <summary>
        /// Calculates the world space docking position and rotation for an AI ship at a given docking point. 
        /// If hoverPosition is true, it will calculate the offset hover position.
        /// Then it updates the ship AI Input module with this information.
        /// Important: It does not set the docking state.
        /// </summary>
        /// <param name="shipDockingPoint"></param>
        /// <param name="shipAIInputModule"></param>
        /// <param name="hoverPosition"></param>
        /// <param name="actionCompletedDistance"></param>
        private void UpdateDockingWSPositionAndRotation (ShipDockingPoint shipDockingPoint, ShipAIInputModule shipAIInputModule, 
            bool hoverPosition, float actionCompletedDistance, float actionCompletedAngularDistance)
        {
            if (shipDockingPoint != null)
            {
                Vector3 targetDockingPos = Vector3.zero;
                Quaternion targetDockingRot = Quaternion.identity;

                // Calculate the world space docking position and rotation
                CalculateDockingWSPositionAndRotation(shipDockingPoint, hoverPosition, ref targetDockingPos, ref targetDockingRot);

                // Assign target information to AI ship
                AssignDockingWSPositionAndRotation(shipAIInputModule, targetDockingPos, targetDockingRot, 
                    shipDockingPoint, actionCompletedDistance, actionCompletedAngularDistance);
            }
        }

        /// <summary>
        /// Calculates the world space docking position and rotation for an AI ship at a given docking point. 
        /// If hoverPosition is true, it will calculate the offset hover position.
        /// </summary>
        /// <param name="shipDockingPoint"></param>
        /// <param name="hoverPosition"></param>
        /// <param name="targetDockingPos"></param>
        /// <param name="targetDockingRot"></param>
        private void CalculateDockingWSPositionAndRotation (ShipDockingPoint shipDockingPoint, 
            bool hoverPosition, ref Vector3 targetDockingPos, ref Quaternion targetDockingRot)
        {
            // Target docking position calculation:
            // Start with the world space position of the docking point
            targetDockingPos = shipDockingStation.GetDockingPointPositionWS(shipDockingPoint);
            // If required, offset the position by a hover distance
            if (hoverPosition)
            {
                targetDockingPos += shipDockingStation.GetDockingPointRotation(shipDockingPoint) * Vector3.up * shipDockingPoint.hoverHeight;
            }

            // Calculate the target rotation of the ship relative to the docking point
            Vector3 adapterDirection = adapterList[0].relativeDirection;
            float XYPlaneLength = Mathf.Sqrt((adapterDirection.x * adapterDirection.x) + (adapterDirection.y * adapterDirection.y));
            // Idea is that we first rotate the ship around the z-axis to get the XY direction pointing downwards, then
            // we rotate around the x-axis to get the YZ direction pointing downwards
            Quaternion shipRelativeRotation = Quaternion.Euler(Mathf.Atan2(adapterDirection.z, XYPlaneLength) * Mathf.Rad2Deg, 0f,
                -Mathf.Atan2(adapterDirection.x, -adapterDirection.y) * Mathf.Rad2Deg);
            // For special case of directly forwards direction, flip upside down to give more natural direction
            if ((adapterDirection.x > 0f ? adapterDirection.x : -adapterDirection.x) < 0.001f &&
                (adapterDirection.y > 0f ? adapterDirection.y : -adapterDirection.y) < 0.001f &&
                adapterDirection.z > 0.001f)
            {
                shipRelativeRotation *= Quaternion.Euler(0f, 0f, 180f);
            }

            // Target docking rotation calculation:
            // Start with the world space rotation of the docking point
            targetDockingRot = shipDockingStation.GetDockingPointRotation(shipDockingPoint);
            // Then add the rotation of the ship relative to the docking point
            targetDockingRot *= shipRelativeRotation;

            // Subtract the adapter relative position (in world space), so that the adapter point will line up with the docking point
            targetDockingPos -= targetDockingRot * adapterList[0].relativePosition;
        }

        /// <summary>
        /// Adjust where the ship snaps to when docking based on the dockSnapTo axes.
        /// Input the calculated docking position and rotation for the ship.
        /// Output the adjusted docking position and rotation.
        /// </summary>
        /// <param name="targetDockingPos"></param>
        /// <param name="targetDockingRot"></param>
        private void AdjustDockingSnapPoint(ref Vector3 targetDockingPos, ref Quaternion targetDockingRot)
        {
            // Determine the ship in local space (relative) to the docking point
            Vector3 shipRelativePosToDockingPoint = Quaternion.Inverse(targetDockingRot) * (transform.position - targetDockingPos);
            Vector3 shipRelativeRotToDockingPoint = (Quaternion.Inverse(targetDockingRot) * transform.rotation).eulerAngles;

            // Restrict snap position offset
            // Make no adjustments if dockSnapToNoneInt
            if (dockSnapToPosInt == dockSnapToAllInt)
            {
                shipRelativePosToDockingPoint.x = 0f;
                shipRelativePosToDockingPoint.y = 0f;
                shipRelativePosToDockingPoint.z = 0f;
            }
            else if (dockSnapToPosInt == dockSnapToXInt)
            {
                shipRelativePosToDockingPoint.x = 0f;
            }
            else if (dockSnapToPosInt == dockSnapToYInt)
            {
                shipRelativePosToDockingPoint.y = 0f;
            }
            else if (dockSnapToPosInt == dockSnapToZInt)
            {
                shipRelativePosToDockingPoint.z = 0f;
            }
            else if (dockSnapToPosInt == dockSnapToXYInt)
            {
                shipRelativePosToDockingPoint.x = 0f;
                shipRelativePosToDockingPoint.y = 0f;
            }
            else if (dockSnapToPosInt == dockSnapToXZInt)
            {
                shipRelativePosToDockingPoint.x = 0f;
                shipRelativePosToDockingPoint.z = 0f;
            }
            else if (dockSnapToPosInt == dockSnapToYZInt)
            {
                shipRelativePosToDockingPoint.y = 0f;
                shipRelativePosToDockingPoint.z = 0f;
            }

            // Restrict snap rotation offset
            // Make no adjustments if dockSnapToNoneInt
            if (dockSnapToRotInt == dockSnapToAllInt)
            {
                shipRelativeRotToDockingPoint.x = 0f;
                shipRelativeRotToDockingPoint.y = 0f;
                shipRelativeRotToDockingPoint.z = 0f;
            }
            else if (dockSnapToRotInt == dockSnapToXInt)
            {
                shipRelativeRotToDockingPoint.x = 0f;
            }
            else if (dockSnapToRotInt == dockSnapToYInt)
            {
                shipRelativeRotToDockingPoint.y = 0f;
            }
            else if (dockSnapToRotInt == dockSnapToZInt)
            {
                shipRelativeRotToDockingPoint.z = 0f;
            }
            else if (dockSnapToRotInt == dockSnapToXYInt)
            {
                shipRelativeRotToDockingPoint.x = 0f;
                shipRelativeRotToDockingPoint.y = 0f;
            }
            else if (dockSnapToRotInt == dockSnapToXZInt)
            {
                shipRelativeRotToDockingPoint.x = 0f;
                shipRelativeRotToDockingPoint.z = 0f;
            }
            else if (dockSnapToRotInt == dockSnapToYZInt)
            {
                shipRelativeRotToDockingPoint.y = 0f;
                shipRelativeRotToDockingPoint.z = 0f;
            }

            // Apply any residual offset to the intending landing position
            targetDockingPos += targetDockingRot * shipRelativePosToDockingPoint;

            targetDockingRot = Quaternion.Euler(shipRelativeRotToDockingPoint) * targetDockingRot;
        }

        /// <summary>
        /// Assigns a target position, rotation and radius to an AI ship in the docking AI state.
        /// </summary>
        /// <param name="shipAIInputModule"></param>
        /// <param name="targetDockingPos"></param>
        /// <param name="targetDockingRot"></param>
        /// <param name="shipDockingPoint"></param>
        /// <param name="actionCompletedDistance"></param>
        /// <param name="actionCompletedAngularDistance"></param>
        private void AssignDockingWSPositionAndRotation (ShipAIInputModule shipAIInputModule, Vector3 targetDockingPos, 
            Quaternion targetDockingRot, ShipDockingPoint shipDockingPoint, float actionCompletedDistance,
            float actionCompletedAngularDistance)
        {
            // Assign target position, rotation and radius data to AI ship
            shipAIInputModule.AssignTargetPosition(targetDockingPos);
            shipAIInputModule.AssignTargetRotation(targetDockingRot);
            // Target radius must be at least 1 metre
            shipAIInputModule.AssignTargetRadius(shipDockingPoint.hoverHeight > 1f ? shipDockingPoint.hoverHeight : 1f);
            shipAIInputModule.AssignTargetDistance(actionCompletedDistance);
            shipAIInputModule.AssignTargetAngularDistance(actionCompletedAngularDistance);
            shipAIInputModule.AssignTargetVelocity(shipDockingStation.GetStationVelocity());
        }

        /// <summary>
        /// Try to join an entry or exit path at the closest point to the ship
        /// </summary>
        /// <param name="guidHashPath"></param>
        /// <returns></returns>
        private bool TryJointPathAtClosestPoint(int guidHashPath)
        {
            bool isJoinedPath = false;

            // Attempt ot join exit path (if there is one)
            if (AssignNewPath(guidHashPath))
            {
                // Find the closest point on the exit path
                PathData pathData = sscManager.GetPath(guidHashPath);

                if (pathData != null)
                {
                    Vector3 closestPointOnPath = Vector3.zero;
                    float closestPointOnPathTValue = 0f;
                    int prevPathLocationIdx = 0;

                    if (SSCMath.FindClosestPointOnPath(pathData, shipControlModule.shipInstance.TransformPosition, ref closestPointOnPath, ref closestPointOnPathTValue, ref prevPathLocationIdx))
                    {
                        shipAIInputModule.SetPreviousTargetPathLocationIndex(prevPathLocationIdx);
                        int nextExitPathLocationIndex = SSCManager.GetNextPathLocationIndex(pathData, prevPathLocationIdx, pathData.isClosedCircuit);

                        if (nextExitPathLocationIndex >= 0)
                        {
                            shipAIInputModule.SetCurrentTargetPathLocationIndex(nextExitPathLocationIndex, closestPointOnPathTValue);
                            shipAIInputModule.SetPreviousTargetPathLocationIndex(prevPathLocationIdx);
                            isJoinedPath = true;
                        }
                    }
                }
            }

            return isJoinedPath;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// If it has not already been saved (set), record
        /// the original rigidbody interpolation setting
        /// of the ship this component is attached to.
        /// </summary>
        internal void SaveInterpolation()
        {
            if (!isOriginalRBInterpolationSet)
            {
                // If the ship is initialised use the cached rigidbody, else fetch it
                Rigidbody rbShip = isShipInitialised ? shipControlModule.ShipRigidbody : shipControlModule.GetComponent<Rigidbody>();

                // Remember the interpolation setting on the ship
                if (rbShip != null)
                {
                    originalRBInterpolation = rbShip.interpolation;
                    isOriginalRBInterpolationSet = true;
                }
            }
        }

        /// <summary>
        /// This can be used when another feature triggers undocking. It ensures
        /// that if the countdown has begun it doesn't continue.
        /// </summary>
        internal void ResetAutoUndock()
        {
            autoUndockTimer = 0f;
        }

        #endregion

        #region Internal Callback Methods

        /// <summary>
        /// Callback for when the ship AI has completed a state action.
        /// </summary>
        /// <param name="shipAIInputModule"></param>
        internal void AICompletedStateActionCallback(ShipAIInputModule shipAIInputModule)
        {
            if (shipAIInputModule != null && shipAIInputModule.IsInitialised)
            {
                // By default, not in AI docking state
                isInAIDockingState = false;

                dockingStateInt = (int)dockingState;
                int shipAIStateID = shipAIInputModule.GetState();

                #region If Docking
                if (dockingStateInt == dockingInt)
                {
                    ShipDockingPoint shipDockingPoint = shipDockingStation.GetDockingPoint(DockingPointId);

                    if (shipAIStateID == AIState.dockingStateID)
                    {
                        // Calculate the world space docking position and rotation for the landing position
                        Vector3 targetDockingPos = Vector3.zero;
                        Quaternion targetDockingRot = Quaternion.identity;
                        CalculateDockingWSPositionAndRotation(shipDockingPoint, false, ref targetDockingPos, ref targetDockingRot);

                        // Check if we have reached the landing position
                        if (Vector3.SqrMagnitude(shipAIInputModule.GetTargetPosition() - targetDockingPos) <  landingDistancePrecision * landingDistancePrecision)
                        {
                            // We have now finished the landing manoeuvre, set the state to docked
                            SetState(DockingState.Docked);
                            // Clean up - set the callback for CompletedStateAction back to its original value
                            shipAIInputModule.callbackCompletedStateAction = originalCompletedStateActionCallback;
                            shipAIInputModule.SetState(AIState.idleStateID);

                            // If there are Post Docked event peristent AND/OR non-persistent listeners.
                            if (SSCUtils.HasListeners(onPostDocked))
                            {
                                if (onPostDockedDelay > 0f)
                                {
                                    StartCoroutine(OnPostDockedDelayed(shipDockingStation.ShipDockingStationId, shipControlModule.shipInstance.shipId, DockingPointId + 1, onPostDockingHoverDelay));
                                }
                                else
                                {
                                    onPostDocked.Invoke(shipDockingStation.ShipDockingStationId, shipControlModule.shipInstance.shipId, DockingPointId + 1, Vector3.zero);
                                }
                            }
                        }
                        else
                        {
                            // We have now finished the hover manoeuvre
                            // Now we need to start the landing manoeuvre

                            // Reset the state action completed flag
                            shipAIInputModule.SetHasCompletedStateAction(false);

                            // Set the ship to target the landing position (we have already calculated it)
                            // Action will be completed when we get with x metres of the target position
                            dockingActionCompletedDistance = landingDistancePrecision;
                            dockingActionCompletedAngle = landingAnglePrecision;
                            AssignDockingWSPositionAndRotation(shipAIInputModule, targetDockingPos, targetDockingRot,
                                shipDockingPoint, dockingActionCompletedDistance, dockingActionCompletedAngle);

                            shipAIInputModule.AssignTargetTime(landingDuration);
                            isInAIDockingState = true;
                            isHoverTarget = false;

                            // If there are Post Docking Hover event peristent AND/OR non-persistent listeners.
                            if (SSCUtils.HasListeners(onPostDockingHover))
                            {
                                if (onPostDockingHoverDelay > 0f)
                                {
                                    StartCoroutine(OnPostDockingHoverDelayed(shipDockingStation.ShipDockingStationId, shipControlModule.shipInstance.shipId, DockingPointId + 1, onPostDockingHoverDelay));
                                }
                                else
                                {
                                    onPostDockingHover.Invoke(shipDockingStation.ShipDockingStationId, shipControlModule.shipInstance.shipId, DockingPointId + 1, Vector3.zero);
                                }
                            }
                        }
                    }
                    else
                    {
                        // If we were docking, we have now reached the end of the entry path
                        // Now we need to start the landing manoeuvre
                        // Set the ship's state to the "docking" state
                        shipAIInputModule.SetState(AIState.dockingStateID);

                        // Set the ship to target the hover position
                        // Action will be completed when we get with hover distance & angle of the target (hover) position
                        dockingActionCompletedDistance = hoverDistancePrecision;
                        dockingActionCompletedAngle = hoverAnglePrecision;
                        UpdateDockingWSPositionAndRotation(shipDockingPoint, shipAIInputModule, true, 
                            dockingActionCompletedDistance, dockingActionCompletedAngle);

                        shipAIInputModule.AssignTargetTime(landingDuration);

                        isInAIDockingState = true;
                        isHoverTarget = true;
                    }
                }
                #endregion

                #region Else Undocking
                else if (dockingStateInt == undockingInt)
                {
                    if (shipAIStateID == AIState.dockingStateID)
                    {
                        // If we were undocking, we have now finished the liftoff manoeuvre
                        // We need to transition to following the exit path
                        ShipDockingPoint shipDockingPoint = shipDockingStation.GetDockingPoint(DockingPointId);
                        if (shipDockingPoint != null)
                        {
                            isHoverTarget = false;
                            if (!AssignNewPath(shipDockingPoint.guidHashExitPath))
                            {
                                // If no valid exit path, immediately finish undocking manoeuvre
                                SetState(DockingState.NotDocked);
                                // Clean up - set the callback for CompletedStateAction back to its original value
                                shipAIInputModule.callbackCompletedStateAction = originalCompletedStateActionCallback;
                                shipAIInputModule.SetState(AIState.idleStateID);
                                // Call the callback to let the script know that it needs to take action
                                if (originalCompletedStateActionCallback != null) { originalCompletedStateActionCallback(shipAIInputModule); }

                                // If there any Post Undocked event peristent AND/OR non-persistent listeners.
                                if (SSCUtils.HasListeners(onPostUndocked))
                                {
                                    if (onPostUndockedDelay > 0f)
                                    {
                                        StartCoroutine(OnPostUndockedDelayed(shipDockingStation.ShipDockingStationId, shipControlModule.shipInstance.shipId, DockingPointId + 1, onPostDockingHoverDelay));
                                    }
                                    else
                                    {
                                        onPostUndocked.Invoke(shipDockingStation.ShipDockingStationId, shipControlModule.shipInstance.shipId, DockingPointId + 1, Vector3.zero);
                                    }
                                }
                            }
                            // Catapult launch for AI or AI assisted with an Exit path once hover height has been reached
                            else if (catapultThrust > 0f)
                            {
                                shipControlModule.shipInstance.AddBoost(Vector3.forward, catapultThrust, catapultDuration);
                            }

                            // If there is a Post Undocking Hover event peristent AND/OR non-persistent listeners.
                            if (SSCUtils.HasListeners(onPostUndockingHover))
                            {
                                if (onPostUndockingHoverDelay > 0f)
                                {
                                    StartCoroutine(OnPostUndockingHoverDelayed(shipDockingStation.ShipDockingStationId, shipControlModule.shipInstance.shipId, DockingPointId + 1, onPostUndockingHoverDelay));
                                }
                                else
                                {
                                    onPostUndockingHover.Invoke(shipDockingStation.ShipDockingStationId, shipControlModule.shipInstance.shipId, DockingPointId + 1, Vector3.zero);
                                }
                            }
                        }
                    }
                    else
                    {
                        // If we were undocking, we have now reached the end of the exit path
                        // Set the state to not docked
                        SetState(DockingState.NotDocked);
                        // Clean up - set the callback for CompletedStateAction back to its original value
                        shipAIInputModule.callbackCompletedStateAction = originalCompletedStateActionCallback;
                        // v1.2.3+ Undocking AI ships become idle at end of exit path
                        shipAIInputModule.SetState(AIState.idleStateID);
                        // Call the callback to let the script know that it needs to take action
                        if (originalCompletedStateActionCallback != null) { originalCompletedStateActionCallback(shipAIInputModule); }

                        // If there any Post Undocked event peristent AND/OR non-persistent listeners.
                        if (SSCUtils.HasListeners(onPostUndocked))
                        {
                            if (onPostUndockedDelay > 0f)
                            {
                                StartCoroutine(OnPostUndockedDelayed(shipDockingStation.ShipDockingStationId, shipControlModule.shipInstance.shipId, DockingPointId + 1, onPostDockingHoverDelay));
                            }
                            else
                            {
                                onPostUndocked.Invoke(shipDockingStation.ShipDockingStationId, shipControlModule.shipInstance.shipId, DockingPointId + 1, Vector3.zero);
                            }
                        }
                    }
                }
                #endregion
            }
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Return the current docking state
        /// </summary>
        /// <returns></returns>
        public DockingState GetState()
        {
            return dockingState;
        }

        /// <summary>
        /// Return the current dockSnapTo axes.
        /// </summary>
        /// <returns></returns>
        public DockSnapTo GetDockSnapTo()
        {
            return dockSnapToPos;
        }

        /// <summary>
        /// Get the world space position of a docking adapter for the attached ship.
        /// Currently only supports adapter number 1.
        /// </summary>
        /// <param name="adapterNumber"></param>
        /// <returns></returns>
        public Vector3 GetDockingAdapterPosition (int adapterNumber)
        {
            if (adapterNumber == 1)
            {
                return transform.position + (Quaternion.LookRotation(adapterList[adapterNumber-1].relativeDirection) * adapterList[adapterNumber - 1].relativePosition);
            }
            else { return transform.position; }
        }

        /// <summary>
        /// Get the world space rotation of a docking adapter for the attached ship.
        /// Currently only supports adapter number 1.
        /// </summary>
        /// <param name="adapterNumber"></param>
        /// <returns></returns>
        public Quaternion GetDockingAdapteRotation (int adapterNumber)
        {
            if (adapterNumber == 1)
            {
                return transform.rotation * Quaternion.LookRotation(adapterList[adapterNumber - 1].relativeDirection);
            }
            else { return transform.rotation; }
        }

        /// <summary>
        /// Return the current docking state as an integer.
        /// </summary>
        /// <returns></returns>
        public int GetStateInt()
        {
            return dockingStateInt;
        }

        /// <summary>
        /// Call this when you wish to remove any custom (non-persistent) event listeners,
        /// like after creating them in code and then destroying the object.
        /// This is automatically called by OnDestroy.
        /// </summary>
        public void RemoveListeners()
        {
            if (IsInitialised)
            {
                if (onPostDocked != null) { onPostDocked.RemoveAllListeners(); }
                if (onPostDockingHover != null) { onPostDockingHover.RemoveAllListeners(); }
                if (onPostDockingStart != null) { onPostDockingStart.RemoveAllListeners(); }
                if (onPostUndocked != null) { onPostUndocked.RemoveAllListeners(); }
                if (onPostUndockingHover != null) { onPostUndockingHover.RemoveAllListeners(); }
                if (onPostUndockingStart != null) { onPostUndockingStart.RemoveAllListeners(); }
            }
        }

        /// <summary>
        /// Set which position axes to snap to when a ship gets close to the docking point, and is becomes "Docked".
        /// </summary>
        /// <param name="newDockSnapTo"></param>
        public void SetDockSnapToPosition (DockSnapTo newDockSnapTo)
        {
            dockSnapToPos = newDockSnapTo;
            dockSnapToPosInt = (int)newDockSnapTo;
        }

        /// <summary>
        /// Set which rotation axes to snap to when a ship gets close to the docking point, and is becomes "Docked".
        /// </summary>
        /// <param name="newDockSnapTo"></param>
        public void SetDockSnapToRotation (DockSnapTo newDockSnapTo)
        {
            dockSnapToRot = newDockSnapTo;
            dockSnapToRotInt = (int)newDockSnapTo;
        }

        /// <summary>
        /// Set the docking state of the ship.
        /// When undocking, the velocity of ShipDockingStation (mothership) is considered.
        /// If configured, a custom method is called after the state has been changed.
        /// </summary>
        /// <param name="dockingState"></param>
        public void SetState (DockingState dockingState)
        {
            if (!IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: ShipDocking.SetState was called before it was initialised on " + gameObject.name + ". Either set initialiseOnAwake in the Editor or call Initialise() at runtime.");
                #endif
                return;
            }

            int previousDockingStateInt = dockingStateInt;

            this.dockingState = dockingState;
            dockingStateInt = (int)dockingState;

            // By default, not in AI docking state
            isInAIDockingState = false;

            if (shipControlModule != null)
            {
                // Used for AI docking without a path
                bool immediatelyCompleteState = false;

                // Ensure autoundocking is reset if the state is changed by another feature or API call.
                autoUndockTimer = 0f;

                #region Docked
                if (dockingStateInt == dockedInt)
                {
                    if (isShipInitialised && !shipControlModule.shipInstance.isThrusterFXStationary)
                    {
                        shipControlModule.StopThrusterEffects();
                    }
                    // NOTE: This will pause (but not stop) the thruster FX.
                    shipControlModule.DisableShipMovement();
                    // Check if we should override default behaviour after ship physics have been disabled
                    if (detectCollisionsWhenDocked) { shipControlModule.ShipRigidbody.detectCollisions = true; }
                    isHoverTarget = false;

                    // Should thrusters still be visibly running after ship is docked?
                    if (isShipInitialised && shipControlModule.shipInstance.isThrusterFXStationary && shipControlModule.shipInstance.isThrusterSystemsStarted)
                    {
                        shipControlModule.PauseOrResumeThrusters(true);
                    }

                    if (shipDockingStation != null)
                    {
                        /// Option to dock in current position and not snap to the exact docking point.
                        /// previousDockingState must be docking or undocking.
                        /// Would need to calc new dockedRelativePosition and Rotation.
                        if (dockSnapToPosInt == dockSnapToNoneInt && dockSnapToRotInt == dockSnapToNoneInt && (previousDockingStateInt == dockingInt || previousDockingStateInt == undockingInt))
                        {
                            // Keep current position of the ship relative to the ship docking station (don't snap to docking point)
                            dockedRelativePosition = shipDockingStation.GetScaledRelativePosition(transform.position);
                            dockedRelativeRotation = Quaternion.Inverse(shipDockingStation.transform.rotation) * transform.rotation;                            
                        }
                        else
                        {
                            ShipDockingPoint shipDockingPoint = shipDockingStation.GetDockingPoint(DockingPointId);

                            if (shipDockingPoint != null)
                            {
                                // This is where we should land (dock)
                                // Get the target position and rotation of the ship if snap to the docking point was enabled
                                CalculateDockingWSPositionAndRotation(shipDockingPoint, false, ref targetDockingPosition, ref targetDockingRotation);

                                if ((dockSnapToPosInt == dockSnapToAllInt && dockSnapToRotInt == dockSnapToAllInt) || (previousDockingStateInt != dockingInt && previousDockingStateInt != undockingInt))
                                {
                                    // NOTE: The scaled relative position hasn't been tested with moving motherships.
                                    dockedRelativePosition = shipDockingStation.GetScaledRelativePosition(shipDockingPoint);
                                    dockedRelativeRotation = Quaternion.Euler(shipDockingPoint.relativeRotation);

                                    // Set the docked position and rotation of the ship - snap to the docking point                                    
                                    transform.SetPositionAndRotation(targetDockingPosition, targetDockingRotation);
                                }
                                else
                                {
                                    AdjustDockingSnapPoint(ref targetDockingPosition, ref targetDockingRotation);

                                    // NOTE: The scaled relative position hasn't been tested with moving motherships.
                                    dockedRelativePosition = shipDockingStation.GetScaledRelativePosition(targetDockingPosition);
                                    dockedRelativeRotation = Quaternion.Inverse(shipDockingStation.transform.rotation) * targetDockingRotation;

                                    // Snap the ship to the point "near" the docking point
                                    transform.SetPositionAndRotation(targetDockingPosition, targetDockingRotation);
                                }
                            }
                        }
                    }

                    // If the ship is initialised use the cached rigidbody, else fetch it
                    Rigidbody rbShip = isShipInitialised ? shipControlModule.ShipRigidbody : shipControlModule.GetComponent<Rigidbody>();

                    // Remember the interpolation setting on the docking ship
                    if (rbShip != null && !isOriginalRBInterpolationSet)
                    {
                        originalRBInterpolation = rbShip.interpolation;
                        isOriginalRBInterpolationSet = true;
                    }

                    // Does this Docking Station have a mothership?
                    if (dockWithShip != null)
                    {
                        // Where possible use the cached rigidbodies
                        if (isShipInitialised && dockWithShip.IsInitialised)
                        {
                            // Match the interpolation setting on the ship being docked to, so that we avoid jerky behaviour
                            shipControlModule.ShipRigidbody.interpolation = dockWithShip.ShipRigidbody.interpolation;
                        }
                        else if (rbShip != null)
                        {
                            // Match the interpolation setting on the ship being docked to, so that we avoid jerky behaviour
                            Rigidbody rbShipToDockWith = dockWithShip.GetComponent<Rigidbody>();
                            if (rbShipToDockWith != null) { rbShip.interpolation = rbShipToDockWith.interpolation; }
                        }
                    }
                }
                #endregion

                #region NotDocked
                else if (dockingStateInt == notDockedInt)
                {
                    // If previous state was docked, reset the velocity, else don't.
                    shipControlModule.EnableShipMovement(previousDockingStateInt == dockedInt);

                    isHoverTarget = false;

                    // If previous state was docked, docking or undocking restore the original
                    // rigidbody interpolation.
                    if (previousDockingStateInt != notDockedInt)
                    {
                        // If the ship is initialised use the cached rigidbody, else fetch it
                        Rigidbody rbShip = shipControlModule.IsInitialised ? shipControlModule.ShipRigidbody : shipControlModule.GetComponent<Rigidbody>();

                        // Restore the original rigidbody interpolation
                        if (rbShip != null && isOriginalRBInterpolationSet)
                        {
                            rbShip.interpolation = originalRBInterpolation;
                            isOriginalRBInterpolationSet = false;
                        }

                        // Check for a mothership. i.e. a Docking Station on a Ship
                        if (shipControlModule.IsInitialised && dockWithShip != null && dockWithShip.IsInitialised)
                        {
                            // Undock with the same velocity as the mothership

                            shipControlModule.ShipRigidbody.velocity = dockWithShip.ShipRigidbody.velocity + (dockWithShip.shipInstance.RigidbodyUp * undockVertVelocity) + (dockWithShip.shipInstance.RigidbodyForward * undockFwdVelocity)
                                + Vector3.Cross(dockWithShip.ShipRigidbody.angularVelocity, shipControlModule.ShipRigidbody.position - dockWithShip.transform.TransformPoint(dockWithShip.shipInstance.centreOfMass));
                            shipControlModule.ShipRigidbody.angularVelocity = dockWithShip.ShipRigidbody.angularVelocity;
                        }

                        // Check for catapult with non-AI assisted undocking OR AI-assist with no Exit Path
                        if (previousDockingStateInt == dockedInt && catapultThrust > 0f)
                        {
                            shipControlModule.shipInstance.AddBoost(Vector3.forward, catapultThrust, catapultDuration);
                        }
                    }
                }
                #endregion

                #region Docking or UnDocking
                else if (dockingStateInt == dockingInt || dockingStateInt == undockingInt)
                {
                    if (previousDockingStateInt == dockedInt)
                    {
                        // If the ship was previously docked, we'll probably need to also reenable it.
                        if (!shipControlModule.ShipMovementIsEnabled() || !shipControlModule.ShipIsEnabled()) { shipControlModule.EnableShip(false, true); }
                    }

                    if (shipDockingStation != null && shipDockingStation.IsDockingPointPathsInitialised)
                    {
                        ShipDockingPoint shipDockingPoint = shipDockingStation.GetDockingPoint(DockingPointId);

                        if (shipDockingPoint != null)
                        {
                            // If this is an AI Ship (or AI-assisted player ship), assign it a path to follow (if it is setup in the docking point)
                            if (shipAIInputModule != null && shipAIInputModule.IsInitialised)
                            {
                                // Is switching directly to docking while currently undocking
                                if (previousDockingStateInt == undockingInt && dockingStateInt == dockingInt)
                                {
                                    // Restore user callback method (if any) - this gets updated again below
                                    shipAIInputModule.callbackCompletedStateAction = originalCompletedStateActionCallback;

                                    // Where are we in the current undocking maneouvre?
                                    int shipAIStateID = shipAIInputModule.GetState();

                                    // While undocking, was ship heading towards the hover position?
                                    if (isHoverTarget)
                                    {
                                        // Move directly towards the docking point
                                        isHoverTarget = false;
                                        isInAIDockingState = true;
                                    }
                                    // On Exit path, attempt to join Entry path
                                    else if (shipAIStateID == AIState.moveToStateID)
                                    {
                                        //Debug.Log("[DEBUG] switching directly between undocking and docking while on Exit path. AIState: " + shipAIStateID + " T:" + Time.time);

                                        // Attempt ot join entry path (if there is one)
                                        if (!TryJointPathAtClosestPoint(shipDockingPoint.guidHashEntryPath))
                                        {
                                            // No valid path, one was not specified, or close to end of path, so assume entry path has been completed.
                                            immediatelyCompleteState = true;
                                        }
                                    }
                                }
                                // Is switching directly to undocking while currently docking
                                else if (previousDockingStateInt == dockingInt && dockingStateInt == undockingInt)
                                {
                                    // Restore user callback method (if any) - this gets updated again below
                                    shipAIInputModule.callbackCompletedStateAction = originalCompletedStateActionCallback;

                                    // Where are we in the current docking maneouvre?
                                    int shipAIStateID = shipAIInputModule.GetState();

                                    // While docking, was ship heading towards the hover position?
                                    if (isHoverTarget)
                                    {
                                        // Stop flying towards hover position, and attempt to fly along exit path (if there is one).
                                        immediatelyCompleteState = true;
                                    }
                                    // Descending from hover positon to docking point
                                    else if (shipAIStateID == AIState.dockingStateID)
                                    {
                                        // Commence undocking
                                        shipAIInputModule.SetState(AIState.dockingStateID);

                                        // Action will be completed when we get close to the target position
                                        dockingActionCompletedDistance = hoverDistancePrecision;
                                        dockingActionCompletedAngle = hoverAnglePrecision;
                                        UpdateDockingWSPositionAndRotation(shipDockingPoint, shipAIInputModule, true,
                                            dockingActionCompletedDistance, dockingActionCompletedAngle);

                                        shipAIInputModule.AssignTargetTime(landingDuration);
                                            
                                        isInAIDockingState = true;
                                        isHoverTarget = true;
                                    }
                                    // On entry path
                                    else if (shipAIStateID == AIState.moveToStateID)
                                    {
                                        // Try to join the exit path at the closest point to the ship
                                        if (!TryJointPathAtClosestPoint(shipDockingPoint.guidHashExitPath))
                                        {
                                            immediatelyCompleteState = true;
                                        }
                                    }
                                }
                                // Assign the path when docking
                                else if (dockingStateInt == dockingInt)
                                {
                                    if (!AssignNewPath(shipDockingPoint.guidHashEntryPath))
                                    {
                                        // No path or one was not specified, so assume entry path has been completed.
                                        immediatelyCompleteState = true;
                                    }
                                }
                                else
                                {
                                    // When exiting (undocking), set the ship's AI state to the "docking"
                                    // state to carry out the liftoff manoeuvre, before following the 
                                    // exit path (if there is one).
                                    // NOTE: The ShipAI state of "docking" covers both docking and undocking.
                                    shipAIInputModule.SetState(AIState.dockingStateID);

                                    // Action will be completed when we get close to the target position
                                    dockingActionCompletedDistance = hoverDistancePrecision;
                                    dockingActionCompletedAngle = hoverAnglePrecision;
                                    UpdateDockingWSPositionAndRotation(shipDockingPoint, shipAIInputModule, true, 
                                        dockingActionCompletedDistance, dockingActionCompletedAngle);

                                    shipAIInputModule.AssignTargetTime(liftOffDuration);
                                    isInAIDockingState = true;
                                    isHoverTarget = true;
                                }
                                // Remember any callback the user had for CompletedStateAction
                                originalCompletedStateActionCallback = shipAIInputModule.callbackCompletedStateAction;
                                // Replace it with our own callback
                                shipAIInputModule.callbackCompletedStateAction = AICompletedStateActionCallback;
                            }
                        }
                    }
                    #if UNITY_EDITOR
                    else
                    {
                        Debug.Log("ERROR: attempting Docking or Undocking without a ShipDockingStation or without correctly initialising it");
                    }            
                    #endif
                }
                #endregion

                #region Callbacks, Notifications and AutoUndocking

                if (callbackOnStateChange != null) { callbackOnStateChange(this, shipControlModule, shipAIInputModule, (DockingState)previousDockingStateInt); }

                // AI docking without an entry path (added in 1.2.1)
                if (immediatelyCompleteState) { AICompletedStateActionCallback(shipAIInputModule); }

                if (dockingStateInt == dockedInt && shipDockingStation != null)
                {
                    // Do we need to notify the ship docking station that the ship has finished docking?
                    // This will call any event methods configured on the ship docking station.
                    if (SSCUtils.HasListeners(shipDockingStation.onPostDocked))
                    {
                        shipDockingStation.onPostDocked.Invoke(shipDockingStation.ShipDockingStationId, shipControlModule.GetShipId, DockingPointId + 1, Vector3.zero);
                    }

                    // If auto undocking is enabled, restart (activate) the timer.
                    if (autoUndockTime > 0f) { autoUndockTimer = 0.0001f; }
                }

                // Has started undocking from the Docked state
                if (dockingStateInt == undockingInt && previousDockingStateInt == dockedInt)
                {
                    // If there is a Post Undocking Start event peristent AND/OR non-persistent listeners.
                    if (SSCUtils.HasListeners(onPostUndockingStart))
                    {
                        if (onPostUndockingStartDelay > 0f)
                        {
                            StartCoroutine(OnPostUndockingStartDelayed(shipDockingStation.ShipDockingStationId, shipControlModule.shipInstance.shipId, DockingPointId + 1, onPostUndockingStartDelay));
                        }
                        else
                        {
                            onPostUndockingStart.Invoke(shipDockingStation.ShipDockingStationId, shipControlModule.shipInstance.shipId, DockingPointId + 1, Vector3.zero);
                        }
                    }
                }
                // Has started docking from the NotDocked state
                else if (dockingStateInt == dockingInt && previousDockingStateInt == notDockedInt)
                {
                    // If there is a Post Docking Start event peristent AND/OR non-persistent listeners.
                    if (SSCUtils.HasListeners(onPostDockingStart))
                    {
                        if (onPostDockingStartDelay > 0f)
                        {
                            StartCoroutine(OnPostDockingStartDelayed(shipDockingStation.ShipDockingStationId, shipControlModule.shipInstance.shipId, DockingPointId + 1, onPostDockingStartDelay));
                        }
                        else
                        {
                            onPostDockingStart.Invoke(shipDockingStation.ShipDockingStationId, shipControlModule.shipInstance.shipId, DockingPointId + 1, Vector3.zero);
                        }
                    }
                }

                #endregion
            }
        }

        /// <summary>
        /// Prevent a delayed Post Docked event from running once the delay has been triggered by the ship has docked.
        /// </summary>
        public void StopOnPostDocked()
        {
            StopCoroutine("OnPostDockedDelayed");
        }

        /// <summary>
        /// Prevent a delayed Post Docking Hover event from running once the delay has been triggered
        /// by the ship reaching the hover point while docking.
        /// </summary>
        public void StopOnPostDockingHover()
        {
            StopCoroutine("OnPostDockingHoverDelayed");
        }

        /// <summary>
        /// Prevent a delayed Post Docking Start event from running once the delay has been triggered
        /// by the ship starting to dock.
        /// </summary>
        public void StopOnPostDockingStart()
        {
            StopCoroutine("OnPostDockingStartDelayed");
        }

        /// <summary>
        /// Prevent a delayed Post Undocked event from running once the delay has been triggered by the
        /// ship finishing the docking manoeuvre.
        /// </summary>
        public void StopOnPostUndocked()
        {
            StopCoroutine("OnPostUndockedDelayed");
        }

        /// <summary>
        /// Prevent a delayed Post Undocking Hover event from running once the delay has been triggered
        /// by the ship reaching the hover point while undocking.
        /// </summary>
        public void StopOnPostUndockingHover()
        {
            StopCoroutine("OnPostUndockingHoverDelayed");
        }

        /// <summary>
        /// Prevent a delayed Post Undocking Start event from running once the delay has been triggered
        /// by the ship starting to undock.
        /// </summary>
        public void StopOnPostUndockingStart()
        {
            StopCoroutine("OnPostUndockingStartDelayed");
        }

        #endregion
    }
}