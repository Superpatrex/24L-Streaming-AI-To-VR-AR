using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This component can be attached to a ShipControlModule or a GameObject.
    /// When attached to a (mother)ship, it would typically be a slower moving capital ship
    /// or large platform. If attached to GameObject without a shipControlModule,
    /// it should be a stationary object in the scene.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Docking Components/Ship Docking Station")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class ShipDockingStation : MonoBehaviour
    {
        #region Enumerations

        #endregion

        #region Public Variables

        /// <summary>
        /// If enabled, the Initialise() will be called as soon as Awake() runs. This should be disabled if you are
        /// instantiating the ShipDockingStation through code.
        /// </summary>
        public bool initialiseOnAwake = false;

        public List<ShipDockingPoint> shipDockingPointList;

        /// <summary>
        /// The colour of the selected Docking Points in the scene view
        /// Non-selected docking points are slightly transparent
        /// </summary>
        public Color dockingPointGizmoColour = new Color(1f, 0.92f, 0.016f, 1.0f);

        /// <summary>
        /// Methods that get called immediately before Undock or UndockDelayed are executed.
        /// WARNING: Do NOT call UndockShip from this event.
        /// </summary>
        public SSCDockingEvt1 onPreUndock = null;

        /// <summary>
        /// Methods that get called immediately after docking is complete.
        /// WARNING: Do NOT call DockShip from this event.
        /// </summary>
        public SSCDockingEvt1 onPostDocked = null;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        public bool isDockingPointListExpanded = true;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [HideInInspector] public bool allowRepaint = false;

        #endregion

        #region Public Properties

        public bool IsInitialised { get; private set; }

        public bool IsDockingPointPathsInitialised { get; private set; }

        public int NumberOfDockingPoints { get { return shipDockingPointList == null ? 0 : shipDockingPointList.Count; } }

        /// <summary>
        /// [READONLY] Does this ShipDockingStation have a (mother)ship attached to it
        /// and has it been initialised?
        /// </summary>
        public bool IsMotherShip { get { return isStationShipInitialised; } }

        /// <summary>
        /// [READONLY] The runtime unique identifier for this Ship Docking Station
        /// </summary>
        public int ShipDockingStationId { get { return shipDockingStationId; } }

        #endregion

        #region Public Static Properties

        public static readonly int NoDockingPoint = -1;

        #endregion

        #region Private variables

        /// <summary>
        /// If the docking station is attached to a ship, cache the
        /// shipControlModule.
        /// </summary>
        private ShipControlModule shipControlModuleStation;
        private bool isStationShipInitialised = false;
        private RigidbodyInterpolation originalRBInterpolation = RigidbodyInterpolation.None;
        private bool isOriginalRBInterpolationSet = false;
        private Vector3 initialStationPosition;
        private Quaternion initialStationRotation = Quaternion.identity;
        private Vector3 currentStationPosition = Vector3.zero;
        private Quaternion currentStationRotation = Quaternion.identity;
        private Vector3 deltaPosition;
        private Quaternion deltaRotation = Quaternion.identity;
        private Vector3 pathVelocity, pathAngularVelocity;
        private SSCManager sscManager;
        private int numDockingPoints = 0;
        private int shipDockingStationId = -1;
        private Vector3 tfrmScale = Vector3.one;

        /// <summary>
        /// [INTERNAL ONLY]
        /// Remember which tabs etc were shown in the editor
        /// </summary>
        [SerializeField] private int selectedTabInt = 0;

        #endregion

        #region Initialisation Methods

        void Awake()
        {
            if (initialiseOnAwake) { Initialise(true); }
        }

        /// <summary>
        /// Initialise the docking station. This will have no effect if IsInitialised is true.
        /// If the station is attached to a mothership (rather than a stationary gameobject), the ship
        /// will be initialised automatically if it is not already initialised.
        /// Ships that are assigned with to a docking point, will also be automatically initialised.
        /// </summary>
        /// <param name="initDockingPointPaths">Assumes ShipDockingStation position and rotation is same as when Entry and Exit Paths where created.
        /// If not, set to false and afterwards call InitialiseDockingPointsPaths(..)</param>
        public void Initialise (bool initDockingPointPaths = true)
        {
            if (!IsInitialised)
            {
                if (shipDockingPointList == null) { shipDockingPointList = new List<ShipDockingPoint>(10); }

                tfrmScale = transform.localScale;
                shipDockingStationId = GetInstanceID();

                // Check to see if this Docking Station is attached to a large ship
                if (TryGetComponent(out shipControlModuleStation))
                {
                    // Auto-initialise mothership.
                    if (!shipControlModuleStation.IsInitialised) { shipControlModuleStation.InitialiseShip(); }

                    // cache to avoid having to check for null etc in FixedUpdate
                    isStationShipInitialised = shipControlModuleStation.IsInitialised;

                    if (isStationShipInitialised)
                    {
                        initialStationPosition = shipControlModuleStation.shipInstance.TransformPosition;
                        initialStationRotation = shipControlModuleStation.shipInstance.TransformRotation;
                    }
                }

                sscManager = SSCManager.GetOrCreateManager(gameObject.scene.handle);

                // cache the number of docking points
                numDockingPoints = shipDockingPointList == null ? 0 : shipDockingPointList.Count;

                // Keep compiler happy
                if (selectedTabInt < 0) { }

                IsInitialised = true;

                // Check to see if any ships are assigned the docking points
                // This need to be AFTER IsInitialised = true.
                InitialiseDockingPointShips();

                if (initDockingPointPaths) { InitialiseDockingPointsPaths(Vector3.zero, Quaternion.identity); }

                SetInterpolate(true);
            }
        }

        /// <summary>
        /// Check to see if any ships are assigned the docking points.
        /// Set the DockingState of the ships that are assigned a docking point.
        /// Initialise the ships (if required).
        /// </summary>
        private void InitialiseDockingPointShips()
        {
            for (int dpIdx = 0; dpIdx < numDockingPoints; dpIdx++)
            {
                ShipDockingPoint shipDockingPoint = shipDockingPointList[dpIdx];
                if (shipDockingPoint != null)
                {
                    if (shipDockingPoint.dockedShip != null)
                    {
                        // Check for a ShipDocking component on the ship that is assigned to the docking point
                        ShipDocking shipDocking = shipDockingPoint.dockedShip.GetShipDocking(true);

                        if (shipDocking != null)
                        {
                            if (!shipDockingPoint.dockedShip.IsInitialised) { shipDockingPoint.dockedShip.InitialiseShip(); }
                            if (!shipDocking.IsInitialised) { shipDocking.Initialise(); }
                           
                            shipDocking.shipDockingStation = this;
                            shipDocking.DockingPointId = dpIdx;
                            shipDocking.dockWithShip = shipControlModuleStation;
                            shipDocking.SetState((ShipDocking.DockingState)shipDocking.initialDockingState);
                        }
                        else
                        {
                            #if UNITY_EDITOR
                            Debug.LogWarning("ShipDockingStation.Initialise - " + shipDockingPoint.dockedShip.name + " is missing a ShipDocking component, but is assigned to Docking Point " + (dpIdx + 1) + " on Docking Station " + gameObject.name);
                            #endif
                        }
                    }
                }
            }
        }

        #endregion

        #region Update Methods

        private void Update()
        {
            // Is this docking station attached to a mothership?
            if (isStationShipInitialised)
            {
                currentStationPosition = shipControlModuleStation.shipInstance.TransformPosition;
                currentStationRotation = shipControlModuleStation.shipInstance.TransformRotation;

                deltaPosition = currentStationPosition - initialStationPosition;
                deltaRotation = currentStationRotation * Quaternion.Inverse(initialStationRotation);

                // Has the docking station moved during this last frame?
                if (deltaPosition.x != 0f || deltaPosition.y != 0f || deltaPosition.z != 0f)
                {
                    // The sequence number if used to ensure PathData is not updated multiple times per frame
                    float updateSeqNumber = Time.time;

                    // AI path following requires the velocity, angular velocity, and current position of the docking station
                    pathVelocity = shipControlModuleStation.shipInstance.WorldVelocity;
                    pathAngularVelocity = shipControlModuleStation.shipInstance.WorldAngularVelocity;

                    // If configured, move Entry and Exit path points in the scene to match the movement of the docking station
                    for (int dpIdx = 0; dpIdx < numDockingPoints; dpIdx++)
                    {
                        ShipDockingPoint shipDockingPoint = shipDockingPointList[dpIdx];

                        if (shipDockingPoint != null)
                        {
                            if (shipDockingPoint.guidHashEntryPath != 0)
                            {
                                PathData pathData = sscManager.GetPath(shipDockingPoint.guidHashEntryPath);
                                sscManager.MoveLocations(pathData, updateSeqNumber, currentStationPosition, deltaPosition, deltaRotation, pathVelocity, pathAngularVelocity);
                            }

                            if (shipDockingPoint.guidHashExitPath != 0 && shipDockingPoint.guidHashExitPath != shipDockingPoint.guidHashEntryPath)
                            {
                                PathData pathData = sscManager.GetPath(shipDockingPoint.guidHashExitPath);
                                sscManager.MoveLocations(pathData, updateSeqNumber, currentStationPosition, deltaPosition, deltaRotation, pathVelocity, pathAngularVelocity);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Private and Internal Methods

        /// <summary>
        /// Take into consideration the local scale (set at initialisation) and return the relative position
        /// of the Ship Docking Point on the ShipDockingStation.
        /// </summary>
        /// <param name="shipDockingPoint"></param>
        /// <returns></returns>
        internal Vector3 GetScaledRelativePosition(ShipDockingPoint shipDockingPoint)
        {
            if (shipDockingPoint != null)
            {
                return new Vector3(shipDockingPoint.relativePosition.x / tfrmScale.x, shipDockingPoint.relativePosition.y / tfrmScale.y, shipDockingPoint.relativePosition.z / tfrmScale.z);
            }
            else { return Vector3.zero; }
        }

        /// <summary>
        /// Take into consideration the local scale (set at initialisation) and return the relative position
        /// of the Ship Docking Point on the ShipDockingStation.
        /// </summary>
        /// <param name="relativePosition"></param>
        /// <returns></returns>
        internal Vector3 GetScaledRelativePosition(Vector3 wsPosition)
        {
            Vector3 relativePosition =  Quaternion.Inverse(transform.rotation) * (wsPosition - transform.position);

            return new Vector3(relativePosition.x / tfrmScale.x, relativePosition.y / tfrmScale.y, relativePosition.z / tfrmScale.z);
        }

        /// <summary>
        /// Will set RigidBody Interpolate if isOn is true,
        /// else will set it to None. It applies to all ships
        /// assigned to docking station points (except those in
        /// and undocked state, AND to the mothership of the
        /// Docking Station, if it has one.
        /// NOTE: Only applies to initialised ships.
        /// </summary>
        /// <param name="isOn"></param>
        internal void SetInterpolate(bool isOn)
        {
            // Does this Docking Station have a mothership?
            if (isStationShipInitialised)
            {
                // If we haven't recorded the original interpolation mode,
                // record it now and remember that it has been recorded.
                if (!isOriginalRBInterpolationSet)
                {
                    isOriginalRBInterpolationSet = true;
                    originalRBInterpolation = shipControlModuleStation.ShipRigidbody.interpolation;
                }

                shipControlModuleStation.ShipRigidbody.interpolation = isOn ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
            }

            if (IsInitialised)
            {
                // For ships that aren't Undocked but assigned to a docking point,
                // remmeber their original interpolation setting, then set according
                // to the paramter isOn.
                for (int dpIdx = 0; dpIdx < numDockingPoints; dpIdx++)
                {
                    ShipDockingPoint shipDockingPoint = shipDockingPointList[dpIdx];
                    // Is there an initialised ship assigned to this docking point?
                    if (shipDockingPoint != null && shipDockingPoint.dockedShip != null && shipDockingPoint.dockedShip.IsInitialised)
                    {
                        // Is the ship docked, undocking or docking?
                        ShipDocking shipDocking = shipDockingPoint.dockedShip.GetShipDocking(false);
                        if (shipDocking != null && shipDocking.GetStateInt() != ShipDocking.notDockedInt)
                        {
                            // If it hasn't already been recorded,
                            // save the original interpolation setting
                            shipDocking.SaveInterpolation();

                            shipDockingPoint.dockedShip.ShipRigidbody.interpolation = isOn ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reset the interpolation of the Station to the original setting.
        /// This does not update the docked ships.
        /// NOTE: NOT IN USE - can be changed as required.
        /// </summary>
        internal void ResetInterpolation()
        {
            if (isOriginalRBInterpolationSet && isStationShipInitialised)
            {
                shipControlModuleStation.ShipRigidbody.interpolation = originalRBInterpolation;
                isOriginalRBInterpolationSet = false;
            }
        }

        /// <summary>
        /// Delay the undocking manoeuvre.
        /// </summary>
        /// <param name="dockedShip"></param>
        /// <param name="shipDocking"></param>
        /// <param name="delayTime"></param>
        /// <returns></returns>
        private IEnumerator UndockShipDelayed(ShipControlModule dockedShip, ShipDocking shipDocking, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            // Check that the docked ship hasn't been destroyed during the delay
            if (dockedShip != null && !dockedShip.shipInstance.Destroyed())
            {
                UnDockShipInternal(dockedShip, shipDocking);
            }
        }

        /// <summary>
        /// Undock a ship from the docking point.
        /// </summary>
        /// <param name="dockedShip"></param>
        private void UnDockShipInternal (ShipControlModule dockedShip, ShipDocking shipDocking)
        {
            ShipAIInputModule aiShip = dockedShip.GetShipAIInputModule(false);

            // If it is an initialised AI ship then attempt to go into the Undocking state (and fly along the exit path - assuming there is one...)
            // NOTE: Behaviour change in 1.3.3 (will attempt to hover in Undocking state even if no exit path)
            if (aiShip != null && aiShip.IsInitialised) { shipDocking.SetState(ShipDocking.DockingState.Undocking); }
            // This is either NOT an AI nor an AI-assisted ship, so go straight to not docked state
            else { shipDocking.SetState(ShipDocking.DockingState.NotDocked); }
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Remember initial Entry/Exit docking path location positions for all ShipDockingPoints.
        /// Offset and rotate them as indicated. Initial positions may need to be modified if the ShipDockingStation
        /// has moved in the scene from where the Entry/Exit Paths were first setup.
        /// Also sets the initial velocity, angular velocity, and anchorPoint.
        /// </summary>
        /// <param name="positionOffset">The difference between where docking station was setup with the Paths in the scene and now.</param>
        /// <param name="rotationDelta">The amount the docking station has been rotated since the Paths were setup in the scene.</param>
        public void InitialiseDockingPointsPaths(Vector3 positionOffset, Quaternion rotationDelta)
        {
            if (!IsDockingPointPathsInitialised && IsInitialised)
            {
                // update cached number of docking points
                numDockingPoints = shipDockingPointList == null ? 0 : shipDockingPointList.Count;

                if (isStationShipInitialised)
                {
                    currentStationPosition = shipControlModuleStation.shipInstance.TransformPosition;
                    pathVelocity = shipControlModuleStation.shipInstance.WorldVelocity;
                    pathAngularVelocity = shipControlModuleStation.shipInstance.WorldAngularVelocity;
                }
                else
                {
                    currentStationPosition = transform.position;
                    pathVelocity = Vector3.zero;
                    pathAngularVelocity = Vector3.zero;
                }

                for (int dpIdx = 0; dpIdx < numDockingPoints; dpIdx++)
                {
                    ShipDockingPoint shipDockingPoint = shipDockingPointList[dpIdx];
                    if (shipDockingPoint != null)
                    {
                        if (shipDockingPoint.guidHashEntryPath != 0)
                        {
                            PathData pathData = sscManager.GetPath(shipDockingPoint.guidHashEntryPath);
                            sscManager.InitialiseLocations(pathData, currentStationPosition, positionOffset, rotationDelta, pathVelocity, pathAngularVelocity);
                        }

                        if (shipDockingPoint.guidHashExitPath != 0 && shipDockingPoint.guidHashExitPath != shipDockingPoint.guidHashEntryPath)
                        {
                            PathData pathData = sscManager.GetPath(shipDockingPoint.guidHashExitPath);
                            sscManager.InitialiseLocations(pathData, currentStationPosition, positionOffset, rotationDelta, pathVelocity, pathAngularVelocity);
                        }
                    }
                }

                IsDockingPointPathsInitialised = true;
            }
        }

        /// <summary>
        /// Assigns a ship to the docking point with index dockingPointIndex, if that docking point is currently available.
        /// You can check whether the docking point is available by calling IsDockingPointAvailable().
        /// If the ship and/or ShipDocking components are not initialised, they will be automatically initialised if the docking point is available.
        /// See also UnassignDockingPoint(dockingPointIndex).
        /// </summary>
        /// <param name="shipControlModule"></param>
        /// <param name="shipDocking"></param>
        /// <param name="dockingPointIndex"></param>
        /// <returns></returns>
        public bool AssignShipToDockingPoint (ShipControlModule shipControlModule, ShipDocking shipDocking, int dockingPointIndex)
        {
            if (!IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: ShipDockingStation.AssignShipToDockingPoint was called before it was initialised on " + gameObject.name + ". Either set initialiseOnAwake in the Editor or call Initialise() at runtime.");
                #endif
                return false;
            }
            else if (shipControlModule != null && shipDocking != null && shipDockingPointList.Count > dockingPointIndex)
            {
                shipDockingPointList[dockingPointIndex].dockedShip = shipControlModule;

                if (!shipControlModule.IsInitialised) { shipControlModule.InitialiseShip(); }
                if (!shipDocking.IsInitialised) { shipDocking.Initialise(); }

                // Cache the station, docking point and station ship (if there is one).
                shipDocking.shipDockingStation = this;
                shipDocking.DockingPointId = dockingPointIndex;
                shipDocking.dockWithShip = shipControlModuleStation;
                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// Assigns a ship to the first available docking point (if any are available).
        /// If the ship and/or ShipDocking components are not initialised, they will be automatically initialised if a docking point is available.
        /// See also UnassignDockingPoint(dockingPointIndex).
        /// </summary>
        /// <param name="shipControlModule"></param>
        /// <param name="shipDocking"></param>
        /// <returns></returns>
        public bool AssignShipToDockingPoint(ShipControlModule shipControlModule, ShipDocking shipDocking)
        {
            bool isAssignSuccessful = false;

            if (!IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: ShipDockingStation.AssignShipToDockingPoint was called before it was initialised on " + gameObject.name + ". Either set initialiseOnAwake in the Editor or call Initialise() at runtime.");
                #endif
                return false;
            }
            else if (shipControlModule != null && shipDocking != null)
            {
                for (int dpIdx = 0; dpIdx < numDockingPoints; dpIdx++)
                {
                    // If this proves to be to costly, we could use a BitArray for occupied docking points
                    // like we use in SSCRadar.
                    if (shipDockingPointList[dpIdx].dockedShip == null)
                    {
                        shipDockingPointList[dpIdx].dockedShip = shipControlModule;

                        if (!shipControlModule.IsInitialised) { shipControlModule.InitialiseShip(); }
                        if (!shipDocking.IsInitialised) { shipDocking.Initialise(); }

                        // Cache the station, docking point and station ship (if there is one).
                        shipDocking.shipDockingStation = this;
                        shipDocking.DockingPointId = dpIdx;
                        shipDocking.dockWithShip = shipControlModuleStation;
                        isAssignSuccessful = true;
                        break;
                    }
                }
            }

            return isAssignSuccessful;
        }

        /// <summary>
        /// Start the docking process for an initialised ship at a valid (zero-based) docking
        /// point index. The ship must be Undocked. If this is an AI ship and there is an
        /// entry path, it will go into the docking state. Otherwise it will become Docked.
        /// The ship must already be assigned to the Docking Point.
        /// See also AssignShipToDockingPoint(..)
        /// </summary>
        /// <param name="dockingPointIndex"></param>
        public void DockShip (int dockingPointIndex)
        {
            ShipControlModule shipControlModule = GetAssignedShip(dockingPointIndex);

            if (shipControlModule != null)
            {
                ShipAIInputModule aiShip = shipControlModule.GetShipAIInputModule(false);

                // Initially don't force a GetComponent.
                ShipDocking shipDocking = shipControlModule.GetShipDocking(false);

                // If the initial reference check failed, test again with a forced GetComponent
                if (shipDocking == null) { shipDocking = shipControlModule.GetShipDocking(true); }

                if (shipDocking != null)
                {
                    // If it is an initialised AI ship then attempt to go into the docking state (and fly along the entry path - assuming there is one...)
                    //if (aiShip != null && aiShip.IsInitialised && GetDockingPointEntryPathguidHash(dockingPointIndex) != 0) { shipDocking.SetState(ShipDocking.DockingState.Docking); }
                    if (aiShip != null && aiShip.IsInitialised) { shipDocking.SetState(ShipDocking.DockingState.Docking); }
                    // This is either NOT and AI ship OR there is no entry path
                    else { shipDocking.SetState(ShipDocking.DockingState.Docked); }
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("ShipDockingStation.DockShip could not find ShipDocking component on " + shipControlModule.name + " at Docking Point " + (dockingPointIndex+1).ToString());
                }
                #endif
            }
        }

        /// <summary>
        /// Start the docking process for an initialised ship at a valid (zero-based) docking
        /// point index. The ship must be Undocked. If this is an AI ship and there is an
        /// entry path, it will go into the docking state. Otherwise it will become Docked.
        /// The ship must already be assigned to the Docking Point.
        /// See also AssignShipToDockingPoint(..)
        /// </summary>
        /// <param name="shipControlModule"></param>
        public void DockShip (ShipControlModule shipControlModule)
        {
            if (shipControlModule != null && shipControlModule.IsInitialised)
            {
                DockShip(GetDockingPointIndex(shipControlModule.shipInstance.shipId));
            }
        }


        /// <summary>
        /// Safely retrieve a ShipDockingPoint from the ShipDockingStation.
        /// Example: ShipDockingPoint shipDockingPoint = shipDockingStation.GetDockingPoint(shipDocking.DockingPointId);
        /// </summary>
        /// <param name="dockingPointIndex"></param>
        /// <returns></returns>
        public ShipDockingPoint GetDockingPoint (int dockingPointIndex)
        {
            if (IsInitialised && dockingPointIndex >= 0 && numDockingPoints > dockingPointIndex)
            {
                return shipDockingPointList[dockingPointIndex];
            }
            else { return null; }
        }

        /// <summary>
        /// Get the zero-based docking point index which a ship is assigned to.
        /// If the ship is not initialised or the ship is not assigned to a docking
        /// point on this docking station, it will return ShipDockingStation.NoDockingPoint (-1).
        /// USAGE: int dpIdx = GetDockingPointIndex(shipControlModule.shipInstance.shipId);
        /// </summary>
        /// <param name="shipId"></param>
        /// <returns></returns>
        public int GetDockingPointIndex (int shipId)
        {
            int dockingPointIndex = NoDockingPoint;

            if (IsInitialised)
            {
                ShipControlModule shipControlModule;

                for (int dpIdx = 0; dpIdx < numDockingPoints; dpIdx++)
                {
                    shipControlModule = shipDockingPointList[dpIdx].dockedShip;

                    if (shipControlModule != null && shipControlModule.IsInitialised && shipControlModule.shipInstance.shipId == shipId)
                    {
                        dockingPointIndex = dpIdx;
                        break;
                    }
                }
            }

            return dockingPointIndex;
        }

        /// <summary>
        /// Return the world-space up direction of the docking point based on the relative rotation.
        /// THIS NEEDS TO BE DOUBLY CHECKED!!! 
        /// </summary>
        /// <param name="shipDockingPoint"></param>
        /// <returns></returns>
        public Vector3 GetDockingPointUp (ShipDockingPoint shipDockingPoint)
        {
            if (shipDockingPoint != null)
            {
                return  (isStationShipInitialised ? shipControlModuleStation.shipInstance.TransformRotation : transform.rotation)
                         * Quaternion.Euler(shipDockingPoint.relativeRotation)
                         * Quaternion.Euler(270f, 0f, 0f) * Vector3.forward;
            }

            else { return Vector3.up; }
        }

        /// <summary>
        /// Get the world space docking point rotation
        /// </summary>
        /// <param name="shipDockingPoint"></param>
        /// <returns></returns>
        public Quaternion GetDockingPointRotation (ShipDockingPoint shipDockingPoint)
        {
            if (shipDockingPoint != null)
            {
                return (isStationShipInitialised ? shipControlModuleStation.shipInstance.TransformRotation : transform.rotation) * Quaternion.Euler(shipDockingPoint.relativeRotation);
            }
            else { return Quaternion.identity; }
        }

        /// <summary>
        /// Get the world space position of the docking point.
        /// </summary>
        /// <param name="shipDockingPoint"></param>
        /// <returns></returns>
        public Vector3 GetDockingPointPositionWS (ShipDockingPoint shipDockingPoint)
        {
            if (shipDockingPoint != null)
            {
                return transform.position + (transform.rotation * shipDockingPoint.relativePosition);

                // I don't think we need to use scaled relative position - but not sure
                //return transform.position + (transform.rotation * GetScaledRelativePosition(shipDockingPoint));
            }
            else { return transform.position; }
        }

        /// <summary>
        /// If a ship is assigned to this docking point, the ShipControlModule for that ship is returned.
        /// If the ShipDockingStation is not initialised, or the docking point index is invalid, or there is
        /// no assigned ship, this method will return null.
        /// NOTE An assigned ship can have a DockingState of Docked, Undocking, or Docking.
        /// See also GetDockedShip(..).
        /// </summary>
        /// <param name="dockingPointIndex"></param>
        /// <returns></returns>
        public ShipControlModule GetAssignedShip (int dockingPointIndex)
        {
            if (IsInitialised && dockingPointIndex >= 0 && numDockingPoints > dockingPointIndex)
            {
                return shipDockingPointList[dockingPointIndex].dockedShip;
            }
            else { return null; }
        }

        /// <summary>
        /// If the ShipDockingStation is initialised, and the docking point is valid, and a ship is assigned
        /// to the docking point, and the ship's ShipDocking.DockingState is Docked, then return the ShipControlModule.
        /// For all other scenarios, return null.
        /// </summary>
        /// <param name="dockingPointIndex"></param>
        /// <returns></returns>
        public ShipControlModule GetDockedShip (int dockingPointIndex)
        {
            if (IsInitialised && dockingPointIndex >= 0 && numDockingPoints > dockingPointIndex)
            {
                ShipControlModule dockedShip = shipDockingPointList[dockingPointIndex].dockedShip;

                if (dockedShip != null && dockedShip.ShipIsDocked()) { return dockedShip; }
                else { return null; }
            }
            else { return null; }
        }

        /// <summary>
        /// Returns the velocity of the station.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetStationVelocity ()
        {
            // Check if there is a ship attached to this station
            if (isStationShipInitialised)
            {
                return shipControlModuleStation.shipInstance.WorldVelocity;
            }
            else
            {
                return Vector3.zero;
            }
        }

        /// <summary>
        /// Get the guidHash of the Entry Path for a docking point using the zero-based index.
        /// If the guidHash == 0 there is no path assigned.
        /// </summary>
        /// <param name="dockingPointIndex"></param>
        /// <returns></returns>
        public int GetDockingPointEntryPathguidHash (int dockingPointIndex)
        {
            if (IsInitialised && dockingPointIndex >= 0 && numDockingPoints > dockingPointIndex)
            {
                return shipDockingPointList[dockingPointIndex].guidHashEntryPath;
            }
            else { return 0; }
        }

        /// <summary>
        /// Get the guidHash of the Exit Path for a docking point using the zero-based index.
        /// If the guidHash == 0 there is no path assigned, the station is not initialised, or
        /// the docking point index is invalid.
        /// </summary>
        /// <param name="dockingPointIndex"></param>
        /// <returns></returns>
        public int GetDockingPointExitPathguidHash(int dockingPointIndex)
        {
            if (IsInitialised && dockingPointIndex >= 0 && numDockingPoints > dockingPointIndex)
            {
                return shipDockingPointList[dockingPointIndex].guidHashExitPath;
            }
            else { return 0; }
        }

        /// <summary>
        /// Is the docking point with index dockingPointIndex currently available?
        /// </summary>
        /// <param name="dockingPointIndex"></param>
        /// <returns></returns>
        public bool IsDockingPointAvailable(int dockingPointIndex)
        {
            if (IsInitialised && dockingPointIndex >= 0 && numDockingPoints > dockingPointIndex)
            {
                // Availability is determined by whether there is currently a ship docked, 
                // or in the process of docking or undocking
                return shipDockingPointList[dockingPointIndex].dockedShip == null;
            }
            else { return false; }
        }

        /// <summary>
        /// Is the ship assigned to any docking points of the Ship Docking Station?
        /// The ship does not have to be docked, to be assigned.
        /// </summary>
        /// <param name="shipId"></param>
        /// <returns></returns>
        public bool IsShipAssigned (int shipId)
        {
            return GetDockingPointIndex(shipId) >= 0;
        }

        /// <summary>
        /// Is the ship docked at any docking point of this Ship Docking Station?
        /// </summary>
        /// <param name="shipId"></param>
        /// <returns></returns>
        public bool IsShipDocked (int shipId)
        {
            return GetDockedShip(GetDockingPointIndex(shipId)) != null;           
        }

        /// <summary>
        /// Call this when you wish to remove any custom event listeners, like
        /// after creating them in code and then destroying the object.
        /// You could add this to your game play OnDestroy code.
        /// </summary>
        public void RemoveListeners()
        {
            if (IsInitialised)
            {
                if (onPreUndock != null) { onPreUndock.RemoveAllListeners(); }
                if (onPostDocked != null) { onPostDocked.RemoveAllListeners(); }
            }
        }

        /// <summary>
        /// Attempt to unassign any ship allocated to a docking point within the ShipDockingStation.
        /// If the ship is currently docked, this method will fail.
        /// </summary>
        /// <param name="dockingPointIndex"></param>
        public bool UnassignDockingPoint (int dockingPointIndex)
        {
            bool isSuccessful = false;

            if (IsInitialised && dockingPointIndex >= 0 && numDockingPoints > dockingPointIndex)
            {
                // TODO - check if there is a docked ship here...

                ShipControlModule assignedShip = shipDockingPointList[dockingPointIndex].dockedShip;

                if (assignedShip == null || !assignedShip.ShipIsDocked())
                {
                    shipDockingPointList[dockingPointIndex].dockedShip = null;
                    isSuccessful = true;
                }
            }

            return isSuccessful;
        }

        /// <summary>
        /// Attempt to unassign a ship that may be assigned to a Docking Point.
        /// It will success if the ship is NOT assigned or NOT Docked with a
        /// Docking Point.
        /// </summary>
        /// <param name="shipId"></param>
        public bool UnassignShip (int shipId)
        {
            bool isSuccessful = true;

            int dockingPointIndex = GetDockingPointIndex(shipId);

            if (dockingPointIndex >= 0)
            {
                isSuccessful = UnassignDockingPoint(dockingPointIndex);
            }

            return isSuccessful;
        }

        /// <summary>
        /// Undock an initialised ship that currently has ShipDocking.DockingState of Docked
        /// at a valid (zero-based) docking point index. If the docked ship is an AI ship it
        /// will go into the Undocking state (from SSC v1.3.3+ it no longer requires an Exit Path).
        /// The undocking ship will be delayed by ShipDocking.undockingDelay.
        /// WARNING: Do NOT call this from the onPreUndock event.
        /// </summary>
        /// <param name="dockingPointIndex"></param>
        public void UnDockShip (int dockingPointIndex)
        {
            ShipControlModule dockedShip = GetDockedShip(dockingPointIndex);
            if (dockedShip != null )
            {
                // Call any methods defined configured for this event
                if (onPreUndock != null) { onPreUndock.Invoke(shipDockingStationId, dockedShip.GetShipId, dockingPointIndex + 1, Vector3.zero); }

                // This must return not null or else GetDockedShip above would have failed
                ShipDocking shipDocking = dockedShip.GetShipDocking(false);

                shipDocking.ResetAutoUndock();

                if (shipDocking.undockingDelay > 0f)
                {
                    StartCoroutine(UndockShipDelayed(dockedShip, shipDocking, shipDocking.undockingDelay));
                }
                else
                {
                    UnDockShipInternal(dockedShip, shipDocking);
                }
            }
        }

        /// <summary>
        /// Undock an initialised ship that currently has ShipDocking.DockingState of Docked
        /// at a valid (zero-based) docking point index. If the docked ship is an AI ship it
        /// will go into the Undocking state (from SSC v1.3.3+ it no longer requires an Exit Path).
        /// The undocking ship will be delayed by ShipDocking.undockingDelay.
        /// WARNING: Do NOT call this from the onPreUndock event.
        /// </summary>
        /// <param name="shipControlModule"></param>
        public void UnDockShip (ShipControlModule shipControlModule)
        {
            if (shipControlModule != null && shipControlModule.IsInitialised)
            {
                UnDockShip(GetDockingPointIndex(shipControlModule.shipInstance.shipId));
            }
        }

        /// <summary>
        /// Import a json file from disk and return as list of ShipDockingPoints
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public List<ShipDockingPoint> ImportDockingPointDataFromJson (string folderPath, string fileName)
        {
            List<ShipDockingPoint> importedDockingPointList = null;

            if (!string.IsNullOrEmpty(folderPath) && !string.IsNullOrEmpty(fileName))
            {
                try
                {
                    string filePath = System.IO.Path.Combine(folderPath, fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        string jsonText = System.IO.File.ReadAllText(filePath);

                        importedDockingPointList = SSCUtils.FromJson<ShipDockingPoint>(jsonText);

                        // Currently cannot do this because we need a wrapper class for the list
                        //JsonUtility.FromJsonOverwrite(jsonText, importedDockingPointList);
                    }
                    #if UNITY_EDITOR
                    else
                    {
                        Debug.LogWarning("ERROR: ShipDockingStation Import Docking Points. Could not find file at " + filePath);
                    }
                    #endif
                }
                catch (System.Exception ex)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: ShipDockingStation - could not import docking point from: " + folderPath + " PLEASE REPORT - " + ex.Message);
                    #else
                    // Keep compiler happy
                    if (ex != null) { }
                    #endif
                }
            }

            return importedDockingPointList;
        }

        /// <summary>
        /// Save the list of docking points for this ShipDockingStation to a json file on disk.
        /// </summary>
        /// <param name="filePath"></param>
        public bool SaveDockingPointDataAsJson (string filePath)
        {
            bool isSuccessful = false;

            if (shipDockingPointList != null && !string.IsNullOrEmpty(filePath))
            {
                try
                {
                    string jsonData = SSCUtils.ToJson(shipDockingPointList);

                    if (!string.IsNullOrEmpty(jsonData) && !string.IsNullOrEmpty(filePath))
                    {
                        System.IO.File.WriteAllText(filePath, jsonData);
                        isSuccessful = true;
                    }
                }
                catch (System.Exception ex)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: ShipDockingStation - could not export: " + transform.name + " PLEASE REPORT - " + ex.Message);
                    #else
                    // Keep compiler happy
                    if (ex != null) { }
                    #endif
                }
            }

            return isSuccessful;
        }

        #endregion
    }
}