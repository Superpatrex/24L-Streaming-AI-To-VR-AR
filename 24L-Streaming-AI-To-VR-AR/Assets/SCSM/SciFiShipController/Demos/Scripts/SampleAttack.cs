using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Executes a simple state machine for determining which state an "attack" AI should be in.
    /// This is a SAMPLE ONLY and may get modified in future releases. If you wish to use something
    /// similar in your own game create a new script in your own namespace to avoid it getting
    /// overwritten by Sci-Fi Ship Controller updates.
    /// If instantiated / added at runtime, also call Initialise().
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Attack AI")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    [RequireComponent(typeof(ShipAIInputModule))]
    public class SampleAttack : MonoBehaviour
    {
        #region Public Variables

        /// <summary>
        /// If enabled, the Initialise() will be called as soon as Awake() runs. This should be disabled if you are
        /// instantiating the SampleRace through code.
        /// </summary>
        public bool initialiseOnAwake = false;

        public Bounds theatreBounds;
        public int attackStateID = AIState.dogfightStateID;

        /// <summary>
        /// Has the SampleAttack been initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }
        #endregion

        #region Public Delegates
        public delegate ShipControlModule CallbackGetNewTarget(ShipControlModule shipControlModule);

        /// <summary>
        /// The name of the developer-supplied custom method that is called when a new target is required
        /// </summary>
        public CallbackGetNewTarget callbackGetNewTarget = null;
        #endregion

        #region Private Variables
        private bool isInitialised = false;
        private ShipAIInputModule shipAIInputModule;
        private ShipControlModule shipControlModule;
        #endregion

        #region Initialise Methods

        void Awake()
        {
            if (initialiseOnAwake) { Initialise(); }
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            if (isInitialised)
            {
                // First check which state the AI is currently in
                int currentStateID = shipAIInputModule.GetState();

                Vector3 targetPosition = shipAIInputModule.GetTargetPosition();
                Ship targetShip = shipAIInputModule.GetTargetShip();
                
                if (currentStateID == attackStateID)
                {
                    // If our ship is outside the theatre area, head back to base hoping to get a new target before getting there
                    if (!theatreBounds.Contains(shipControlModule.shipInstance.TransformPosition))
                    {
                        shipAIInputModule.SetState(AIState.moveToStateID);
                    }
                    // Is the target ship still healthy?
                    else if (targetShip != null && !targetShip.Destroyed())
                    {
                        // If the target is inside the threatre then we're either inside the threatre area or heading back into it.
                        if (!theatreBounds.Contains(targetShip.TransformPosition))
                        {
                            //Debug.Log("[DEBUG] target for " + this.name + " is outside theatre bounds");

                            // Target is outside theatre of operations so abandon pursuit and look for another target
                            shipAIInputModule.SetState(AIState.moveToStateID);
                        }
                    }
                    else
                    {
                        // Target ship has probably been destroyed
                        shipAIInputModule.SetState(AIState.moveToStateID);
                    }

                    // Did we abandon pursuing a ship?
                    if (shipAIInputModule.GetState() == AIState.moveToStateID)
                    {
                        bool isTargetPositionSet = targetPosition.x != 0f || targetPosition.y != 0f || targetPosition.z != 0f;
                        // NOTE: if the ship isn't set to respawn, this value will be 0,0,0
                        if (isTargetPositionSet) { shipAIInputModule.AssignTargetPosition(targetPosition); }
                        else { shipAIInputModule.AssignTargetPosition(shipControlModule.shipInstance.RespawnPosition); }
                        shipAIInputModule.AssignTargetShip(null);
                    }
                }
                else if (currentStateID == AIState.moveToStateID)
                {
                    // No target, so try and aquire another one
                    if (callbackGetNewTarget != null)
                    {
                        ShipControlModule targetShipControlModule = callbackGetNewTarget(shipControlModule);
                        if (targetShipControlModule != null)
                        {
                            shipAIInputModule.SetState(attackStateID);
                            shipAIInputModule.AssignTargetShip(targetShipControlModule);
                        }
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// If shipAIInputModule is not initialised, SampleAttack will initialise it.
        /// </summary>
        public void Initialise()
        {
            // Don't attempt to re-initialise multiple times.
            if (isInitialised) { return; }

            // Get a reference to the ShipControlModule attached to this gameobject
            shipControlModule = GetComponent<ShipControlModule>();

            // Get a reference to the ship AI input module attached to this gameobject
            shipAIInputModule = GetComponent<ShipAIInputModule>();

            // Initialise the ship AI (if it hasn't been initialised already)
            if (!shipAIInputModule.IsInitialised) { shipAIInputModule.Initialise(); }

            // Default to a very large area of operations if one hasn't already been set
            if (theatreBounds.extents == Vector3.zero) { theatreBounds = new Bounds(Vector3.zero, Vector3.one * 10000000); }

            isInitialised = true;
        }

        #endregion

        #region Private Methods


        #endregion
    }
}