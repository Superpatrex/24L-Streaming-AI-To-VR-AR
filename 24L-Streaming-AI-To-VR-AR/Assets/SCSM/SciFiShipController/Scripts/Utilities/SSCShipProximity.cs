using System.Collections.Generic;
using UnityEngine;
using System;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This component let you call your game code, SSC API methods, and/or set properties
    /// on gameobjects when a ship enters or exits an area of your scene.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Utilities/SSC Ship Proximity")]
    [HelpURL("https://scsmmedia.com/ssc-documentation")]
    [DisallowMultipleComponent()]
    public class SSCShipProximity : MonoBehaviour
    {
        #region Public Variables
        public bool initialiseOnStart = false;

        /// <summary>
        /// How many seconds to delay initialisation if Initialise On Start is true
        /// </summary>
        [Range(0f, 30f)] public float initialiseDelay = 0f;

        /// <summary>
        /// The number of seconds, after initialisation, that events or callbacks will
        /// not be triggered by a ship entering or exiting the area. This can be useful
        /// if you do not want a ship within the collider area to immediately trigger
        /// event or callback notifications when the component is initialised.
        /// </summary>
        [Range(0f, 30f)] public float noNotifyDuration = 0f;

        /// <summary>
        /// Array of Unity Tags for ships that affect this collider area. If none are provided, all objects can affect this area. NOTE: All tags MUST exist.
        /// </summary>
        public string[] tags = new string[] { };

        /// <summary>
        /// [Optional] An array of ship factionIds to detect when entering the area
        /// </summary>
        public int[] factionsToInclude = new int[] { };

        /// <summary>
        /// [Optional] An array of ship factionIds to ignore when entering the area
        /// </summary>
        public int[] factionsToExclude = new int[] { };

        /// <summary>
        /// [Optional] An array of squadronIds to detect when entering the area
        /// </summary>
        public int[] squadronsToInclude = new int[] { };

        /// <summary>
        /// [Optional] An array of squadronIds to ignore when entering the area
        /// </summary>
        public int[] squadronsToExclude = new int[] { };

        /// <summary>
        /// Methods that get called when a ship enters the trigger area
        /// </summary>
        public SSCShipProximityEvt1 onEnterMethods = null;

        /// <summary>
        /// Methods that get called when a ship exits the trigger area
        /// </summary>
        public SSCShipProximityEvt1 onExitMethods = null;

        #endregion

        #region Public Properties
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// If initialised, return a bounding box of the proximity region.
        /// </summary>
        public Bounds ProximityRegion { get { return isInitialised ? proximityCollider.bounds : new Bounds(); } }

        #endregion

        #region Public Static Variables

        #endregion

        #region Private Variables
        [System.NonSerialized] private Collider proximityCollider = null;
        private bool isInitialised = false;
        private int numTags = 0;
        private bool isNotificationEnabled = false;

        /// <summary>
        /// Stores an unordered unique list of ShipIds that we are currently inside.
        /// Helps with OnTriggerEnter/Exit events
        /// </summary>
        [System.NonSerialized] private HashSet<int> inTriggerShips;

        /// <summary>
        /// List of Key value pairs containing shipId and colliderId
        /// </summary>
        [System.NonSerialized] private  List<KeyValuePair<int, int>> shipColliderList;

        #endregion

        #region Public Delegates

        public delegate void CallbackOnEnter(ShipControlModule shipControlModule, SSCShipProximity shipProximity);
        public delegate void CallbackOnExit(ShipControlModule shipControlModule, SSCShipProximity shipProximity);

        /// <summary>
        /// The name of the custom method that is called immediately
        /// after a ship that fits the criteria enters the trigger area.
        /// Your method must take 2 parameters: ShipControlModule
        /// and SSCShipProximity.
        /// This should be a lightweight method to avoid
        /// performance issues.
        /// </summary>
        [NonSerialized] public CallbackOnEnter callbackOnEnter = null;

        /// <summary>
        /// The name of the custom method that is called immediately
        /// after a ship that fits the criteria exits the trigger area.
        /// Your method must take 2 parameters: ShipControlModule
        /// and SSCShipProximity.
        /// This should be a lightweight method to avoid
        /// performance issues.
        /// </summary>
        [NonSerialized] public CallbackOnExit callbackOnExit = null;

        #endregion

        #region Private Initialise Methods

        // Start is called before the first frame update
        private void Start()
        {
            if (initialiseOnStart)
            {
                if (initialiseDelay > 0f)
                {
                    DisableProximityCollider();
                    Invoke("Initialise", initialiseDelay);
                }
                else { Initialise(); }
            }
            else
            {
                DisableProximityCollider();
            }
        }

        private void DisableProximityCollider()
        {
            proximityCollider = GetComponent<Collider>();

            if (proximityCollider != null && proximityCollider.isTrigger && proximityCollider.enabled)
            {
                proximityCollider.enabled = false;
            }
        }

        #endregion

        #region Private and Internal Methods

        /// <summary>
        /// Does the gameobject have a tag that matches the array configured by the user.
        /// Will return true if a match is found OR there are no tags configured.
        /// </summary>
        /// <param name="objectGameObject"></param>
        /// <returns></returns>
        private bool IsObjectTagMatched(GameObject objectGameObject)
        {
            if (!isInitialised) { return false; }

            if (objectGameObject == null) { return false; }
            else if (numTags < 1) { return true; }
            else
            {
                bool isMatch = false;
                for (int tgIdx = 0; tgIdx < numTags; tgIdx++)
                {
                    if (objectGameObject.CompareTag(tags[tgIdx]))
                    {
                        isMatch = true;
                        break;
                    }
                }
                return isMatch;
            }
        }

        /// <summary>
        /// Should the ship be included?
        /// </summary>
        /// <param name="factionId"></param>
        /// <param name="squadronId"></param>
        /// <returns></returns>
        private bool IsShipMatched(int factionId, int squadronId)
        {
            int numFactionsToInclude = factionsToInclude == null ? 0 : factionsToInclude.Length;
            int numFactionsToExclude = factionsToExclude == null ? 0 : factionsToExclude.Length;
            int numSquadronsToInclude = squadronsToInclude == null ? 0 : squadronsToInclude.Length;
            int numSquadronsToExclude = squadronsToExclude == null ? 0 : squadronsToExclude.Length;

            // Skip anything in the array of factions to exclude
            if (numFactionsToExclude > 0 && System.Array.IndexOf(factionsToExclude, factionId) > -1) { return false; }

            // Skip anything in the array of squadrons to exclude
            if (numSquadronsToExclude > 0 && System.Array.IndexOf(squadronsToExclude, squadronId) > -1) { return false; }

            // Skip any factions not in the array of factions to include
            if (numFactionsToInclude > 0 && System.Array.IndexOf(factionsToInclude, factionId) < 0) { return false; }

            // Skip any squadron not in the array of squadrons to include
            if (numSquadronsToInclude > 0 && System.Array.IndexOf(squadronsToInclude, squadronId) < 0) { return false; }

            return true;
        }

        /// <summary>
        /// Removes any empty or null tags. NOTE: May increase GC so don't use each frame.
        /// </summary>
        private void ValidateTags()
        {
            numTags = tags == null ? 0 : tags.Length;

            if (numTags > 0)
            {
                List<string> tagList = new List<string>(tags);

                for (int tgIdx = numTags - 1; tgIdx >= 0; tgIdx--)
                {
                    // Remove invalid tag
                    if (string.IsNullOrEmpty(tagList[tgIdx])) { tagList.RemoveAt(tgIdx); }
                }

                // If there were invalid entries, update the array
                if (tagList.Count != numTags)
                {
                    tags = tagList.ToArray();
                    numTags = tags == null ? 0 : tags.Length;
                }
            }
        }

        #endregion

        #region Events

        private void OnDestroy()
        {
            CancelInvoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            ShipControlModule shipControlModule = null;

            if (isInitialised && ShipControlModule.IsObjectAShip(other, out shipControlModule) && IsObjectTagMatched(other.gameObject))
            {
                // TODO find the bounds intersect point between trigger and other collider

                int shipId = shipControlModule.GetShipId;
                int factionId = shipControlModule.FactionId;
                int squadronId = shipControlModule.SquadronId;

                int colliderId = other.GetInstanceID();


                // Check includes/exclusions, and ignore if this collider is already registered as inside the trigger area
                if (IsShipMatched(factionId, squadronId) && !shipColliderList.Exists(k => k.Value == colliderId))
                {
                    // Keep a record of it as inside the trigger area
                    shipColliderList.Add(new KeyValuePair<int, int>(shipId, colliderId));

                    // Make sure another collider on the ship hasn't already entered the trigger area.
                    if (!inTriggerShips.Contains(shipId))
                    {
                        inTriggerShips.Add(shipId);

                        if (isNotificationEnabled)
                        {
                            if (callbackOnEnter != null) { callbackOnEnter.Invoke(shipControlModule, this); }

                            if (onEnterMethods != null) { onEnterMethods.Invoke(shipId, factionId, squadronId, GetInstanceID()); }

                            //Debug.Log("[DEBUG] " + shipControlModule + " entered " + name + " T:" + Time.time);
                        }
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            ShipControlModule shipControlModule = null;

            if (isInitialised && ShipControlModule.IsObjectAShip(other, out shipControlModule) && IsObjectTagMatched(other.gameObject))
            {
                int shipId = shipControlModule.GetShipId;
                int factionId = shipControlModule.FactionId;
                int squadronId = shipControlModule.SquadronId;

                int colliderId = other.GetInstanceID();

                shipColliderList.Remove(new KeyValuePair<int, int>(shipId, colliderId));

                // Check inclusions and exclusions.
                // Make sure there aren't any other colliders for this ship still in the trigger area
                if (IsShipMatched(factionId, squadronId) && inTriggerShips.Contains(shipId) && !shipColliderList.Exists(k => k.Key == shipId))
                {
                    inTriggerShips.Remove(shipId);

                    if (isNotificationEnabled)
                    {
                        if (callbackOnExit != null) { callbackOnExit.Invoke(shipControlModule, this); }

                        if (onExitMethods != null) { onExitMethods.Invoke(shipId, factionId, squadronId, GetInstanceID()); }

                        //Debug.Log("[DEBUG] " + shipControlModule + " exited " + name + " T:" + Time.time);
                    }
                }
            }
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Call this to manually initialise the component
        /// </summary>
        public void Initialise()
        {
            if (!isInitialised)
            {
                if (!TryGetComponent(out proximityCollider))
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("[ERROR] SSCShipProximity could not find a (trigger) collider component. Did you attach one to the " + name + " gameobject?");
                    #endif
                }
                else if (!proximityCollider.isTrigger)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("[ERROR] SSCShipProximity the collider is not a trigger on " + name);
                    #endif
                }
                else
                {
                    ValidateTags();
                    inTriggerShips = new HashSet<int>();
                    shipColliderList = new List<KeyValuePair<int, int>>();

                    if (noNotifyDuration > 0f) { Invoke("TurnNotificationsOn", noNotifyDuration); }
                    else { isNotificationEnabled = true; }

                    isInitialised = true;

                    if (!proximityCollider.enabled) { proximityCollider.enabled = true; }
                }
            }
        }

        /// <summary>
        /// Allow event and callback notifications to be triggered when a ship
        /// enters or exits the collider area.
        /// </summary>
        public void TurnNotificationsOn()
        {
            if (isInitialised) { isNotificationEnabled = true; }
        }

        /// <summary>
        /// Stop event and callback notifications being triggered when a ship
        /// enters or exits the collider area.
        /// </summary>
        public void TurnNotificationsOff()
        {
            isNotificationEnabled = false;
        }

        #endregion
    }
}