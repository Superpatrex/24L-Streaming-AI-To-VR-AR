using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Demo Control Module to create squadrons of ships and have them attack each other.
    /// Ship prefabs should NOT contain a PlayerInputModule
    /// This is only a sample to demonstrate how API calls could be used in your own code.
    /// In a future version of SSC, many of the concepts and methods used here will
    /// be available as API calls to reduce your coding - much like you can set say the
    /// Dogfighting or Movement state of an AI ship now in a couple of lines of code.
    /// This demo uses a custom AIState to show how one could be setup and used, although
    /// in this demo the built-in DogFighting state could just as easily been used.
    /// </summary>
    public class DemoControlModule : MonoBehaviour
    {

        #region Enumerations

        public enum AITargets
        {
            RandomPlayer,
            NextShip,
            RandomShip
        }

        #endregion

        #region Public variables

        public List<Squadron> squadronList = null;

        [Header("AI Targets")]
        public bool assignAITargets = true;
        public AITargets aiTargets = AITargets.NextShip;
        // How often should we reassign targets?
        public float reassignTargetSecs = 30f;

        // Adds the TestAIScript with default values if it isn't on prefab
        public bool AddAIScriptIfMissing = true;

        [Header("AI Behaviour")]
        // When magnitude of velocity approaches zero, health is reduced
        // Stationary ships are eventually destroyed due to lack of health.
        // Mainly used for when ships spiral to the ground
        public bool CrashAffectsHealth = true;

        public Bounds theatreBounds;

        [Header("Radar")]
        public bool useRadar = false;

        public bool isInitialised = false;

        public int NumberOfSquadrons { get { return numSquadrons; } }

        #endregion

        #region Private variables

        private float timer = 0;

        private PlayerInputModule[] playersArray;

        // Each squadron has it's own list of ships
        private List<List<ShipControlModule>> squadronShipList;
        private List<int> numSquadronShipsList;

        // Each squadron needs to know which squadron it should target
        // For now, keep this separate to the squadron class so that
        // squadron remains more generic.
        private List<int> squadronTargetList;
        
        private int numSquadrons = 0;

        // This lets us create our own custom state or use an existing one
        private int attackStateID = AIState.dogfightStateID;

        #endregion

        #region Initialise Methods
        // Start is called before the first frame update
        void Awake()
        {
            // Create a custom AIState
            AIState.Initialise();
            attackStateID = AIState.AddState("DemoAttack", DemoAttackState, AIState.BehaviourCombiner.PrioritisedDithering);

            // Spawner a player ship inside squadron

            timer = reassignTargetSecs; // Don't reset targets immediately when game starts

            numSquadrons = squadronList == null ? 0 : squadronList.Count;

            squadronShipList = new List<List<ShipControlModule>>();
            numSquadronShipsList = new List<int>(numSquadrons);
            squadronTargetList = new List<int>(numSquadrons);

            for (int sqIdx = 0; sqIdx < numSquadrons; sqIdx++)
            {
                Squadron squadron = squadronList[sqIdx];
                if (squadron != null)
                {
                    ShipSpawner shipSpawner = new ShipSpawner();
                    if (shipSpawner != null)
                    {
                        GameObject squadronGameObject = new GameObject(squadron.squadronName);
                        if (squadronGameObject != null)
                        {
                            squadronGameObject.transform.SetParent(gameObject.transform);
                            if (shipSpawner.CreateSquadron(squadron, squadronGameObject.transform, ShipDestroyCallBack, AddAIScriptIfMissing ? typeof(ShipAIInputModule) : null))
                            {
                                // Show the ship InstanceIDs for the squadron
                                int numShips = squadron.shipList == null ? 0 : squadron.shipList.Count;

                                //Debug.Log("[DEBUG] Number of ships added to " + squadron.squadronName + ": " + numShips);

                                // Get all the ships in this squadron once to reduce overhead
                                squadronShipList.Add(new List<ShipControlModule>(numShips));
                                squadronGameObject.GetComponentsInChildren(true, squadronShipList[sqIdx]);
                                numSquadronShipsList.Add(numShips);
                            }
                            else
                            {
                                // Add an empty list if squadron was not created
                                squadronShipList.Add(new List<ShipControlModule>());
                                numSquadronShipsList.Add(0);
                            }
                        }
                    }

                    int targetSquadronId = GetTargetSquadron(squadron.factionId);

                    if (targetSquadronId >= 0)
                    {
                        squadronTargetList.Add(targetSquadronId);

                        if (aiTargets == AITargets.NextShip)
                        {
                            Debug.LogWarning("DemoControlModule NextShip AI Targets only work with 1 squadron. Switching to Random Ship targetting.");
                            aiTargets = AITargets.RandomShip;
                        }
                    }
                    else
                    {
                        // No potential squadrons to target, so target itself
                        squadronTargetList.Add(squadron.squadronId);
                    }

                    //Debug.Log("[DEBUG] " + squadron.squadronName + " is targeting " + squadronList.Find(sq => sq.squadronId == squadronTargetList[sqIdx]).squadronName);
                }
            }

            // Initialise all the AI ships
            for (int sqIdx = 0; sqIdx < numSquadrons; sqIdx++)
            {
                int shipsListLength = numSquadronShipsList[sqIdx];
                for (int shIdx = 0; shIdx < shipsListLength; shIdx++)
                {
                    ShipControlModule shipControlModule = squadronShipList[sqIdx][shIdx];
                    if (shipControlModule != null)
                    {
                        // NOTE: Ideally only the ships near the player camera will be set to Interpolate and
                        // all others will be set to None. However, for convenience set all to Interpolate.
                        if (shipControlModule.IsInitialised)
                        {
                            shipControlModule.ShipRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                            if (useRadar) { shipControlModule.EnableRadar(); }
                        }

                        // Add Script to handle basic state machine logic. We could do the same from a central
                        // gameplay script (like this democontrolmodule) and loop through all ships in the scene.
                        // In this example we'll attach a script to each ship.
                        SampleAttack sampleAttack = shipControlModule.GetComponent<SampleAttack>();
                        if (sampleAttack == null) { sampleAttack = shipControlModule.gameObject.AddComponent<SampleAttack>(); }

                        ShipAIInputModule shipAIInputModule = shipControlModule.GetComponent<ShipAIInputModule>();
                        if (shipAIInputModule != null && sampleAttack != null)
                        {
                            // TODO - Ship Radius should be calculated
                            // Comment this out if set in a prefab
                            shipAIInputModule.shipRadius = 7f;

                            sampleAttack.attackStateID = attackStateID;

                            if (!sampleAttack.IsInitialised)
                            {
                                shipAIInputModule.movementAlgorithm = ShipAIInputModule.AIMovementAlgorithm.PlanarFlightBanking;
                                // If shipAIInputModule is not initialised, SampleAttack will initialise it.
                                sampleAttack.Initialise();
                            }
                        }
                    }
                }
            }

            // Assign targets to all ships
            UpdateSquadrons(true);

            if (useRadar)
            {
                SSCRadar sscRadar = SSCRadar.GetOrCreateRadar();
                sscRadar.ShowUI();
            }

            isInitialised = true;
        }

        #endregion

        #region Update Methods
        // Update is called once per frame
        void Update()
        {
            if (assignAITargets && isInitialised)
            {
                timer -= Time.deltaTime;

                if (timer < 0f)
                {
                    // reset timer so we don't re-assign targets too often
                    timer = reassignTargetSecs;

                    UpdateSquadrons(false);
                }
            }
        }

        /// <summary>
        /// Process all squadrons, and assign targets to ships in those squadrons
        /// </summary>
        /// <param name="isFirstTime"></param>
        private void UpdateSquadrons(bool isFirstTime = false)
        {
            // Get list of all players
            // NOTE: FindObjectsOfType generates GC.Alloc.

            #if UNITY_2022_2_OR_NEWER
            playersArray = FindObjectsByType<PlayerInputModule>(FindObjectsSortMode.None);
            #else
            playersArray = FindObjectsOfType<PlayerInputModule>();
            #endif

            int playersArrayLength = playersArray == null ? 0 : playersArray.Length;

            for (int sqIdx = 0; sqIdx < numSquadrons; sqIdx++)
            {
                UpdateSquadron(sqIdx, playersArrayLength, isFirstTime);
            }
        }

        // Process all ships in a squadron, and assign it a target

        /// <summary>
        /// If isFirstTime, sets the callbackGetNewTarget on ShipAIInputModule after applying
        /// the first target.
        /// </summary>
        /// <param name="squadronIdx"></param>
        /// <param name="playersArrayLength"></param>
        /// <param name="isFirstTime"></param>
        private void UpdateSquadron(int squadronIdx, int playersArrayLength, bool isFirstTime = false)
        {
            // Finding all the AI ships each frame is too expensive.
            // TODO - NOTE: We will now need to maintain the list
            // Currently respawned ships are not re-added to list

            Squadron squadron = squadronList[squadronIdx];

            // Get the squadron for this squadron to target
            int targetSquadronId = squadronTargetList[squadronIdx];
            //int targetSquadronIdx = squadronList.FindIndex(sq => sq.squadronId == targetSquadronId);
            // Use a for-loop on small list instead of a FindIndex which causes GC.Alloc.
            int targetSquadronIdx = -1;
            for (int i = 0; i < numSquadrons; i++)
            {
                if (squadronList[i].squadronId == targetSquadronId) { targetSquadronIdx = i; break; }
            }

            if (targetSquadronIdx < 0) { targetSquadronIdx = squadronIdx; }

            int shipsListLength = numSquadronShipsList[squadronIdx];
            int shipsListTargetLength = numSquadronShipsList[targetSquadronIdx];

            //Debug.Log("[DEBUG] updating squadron " + squadron.squadronName + ", which is targetting " + squadronList[targetSquadronIdx].squadronName + ". Potential targets: " + shipsListTargetLength);

            ShipControlModule aiTarget = null;
            ShipAIInputModule aiScript;
            SampleAttack saScript;
            int targetIdx = -1;

            // Loop through all the ships in the current squadron
            for (int i = 0; i < shipsListLength; i++)
            {
                // Get the ship in the current squadron being updated
                ShipControlModule shipControlModule = squadronShipList[squadronIdx][i];

                //Debug.Log("[DEBUG] squadronIdx: " + squadronIdx + " i: " + i);

                // Remove ships that have been destroyed
                if (shipControlModule == null)
                {
                    squadronShipList[squadronIdx].RemoveAt(i);
                    shipsListLength--;
                    i--;
                    numSquadronShipsList[squadronIdx] -= 1;
                    continue;

                }
                else if (shipControlModule.gameObject.activeSelf)
                {
                    // Get the AI Script attached to the current ship
                    aiScript = shipControlModule.GetComponent<ShipAIInputModule>();
                    saScript = shipControlModule.GetComponent<SampleAttack>();                     
                    if (aiScript != null && saScript != null)
                    {
                        // Set the boundaries for the theatre of operation
                        if (isFirstTime) { saScript.theatreBounds = theatreBounds; }

                        // If an AI ship is not moving, reduce it's health (it may have hit the ground)
                        if (!isFirstTime && CrashAffectsHealth && Vector3.Magnitude(shipControlModule.shipInstance.WorldVelocity) < 0.01f)
                        { shipControlModule.shipInstance.mainDamageRegion.Health -= 20f; }

                        aiTarget = null;
                        switch (aiTargets)
                        {
                            // Ships that are inactive will be targetted, but maybe that's ok for now
                            case AITargets.RandomPlayer:
                                PlayerInputModule playerInputModule = playersArray[Random.Range(0, playersArrayLength)];
                                if (playerInputModule != null) { aiTarget = playerInputModule.GetComponent<ShipControlModule>(); }
                                break;
                            case AITargets.NextShip:
                                targetIdx = (i == shipsListLength - 1) ? 0 : i + 1;
                                shipControlModule = squadronShipList[squadronIdx][targetIdx];

                                if (shipControlModule != null) { aiTarget = shipControlModule; }
                                else
                                {
                                    // Find next non-null ship. i.e. one that hasn't been destroyed AND is inside the theatre area
                                    targetIdx = squadronShipList[squadronIdx].FindIndex(targetIdx, shp => shp != null && shp.shipInstance != null && theatreBounds.Contains(shp.shipInstance.TransformPosition));
                                    // If no ships at end of list, start at beginning
                                    if (targetIdx < 0) { targetIdx = squadronShipList[squadronIdx].FindIndex(shp => shp != null && shp.shipInstance != null && theatreBounds.Contains(shp.shipInstance.TransformPosition)); }

                                    if (targetIdx >= 0) { aiTarget = squadronShipList[squadronIdx][targetIdx]; }
                                }
                                break;
                            case AITargets.RandomShip:                    
                                // If there are potential targets, find a random ship
                                if (shipsListTargetLength > 0)
                                {
                                    targetIdx = Random.Range(0, shipsListTargetLength);
                                    shipControlModule = squadronShipList[targetSquadronIdx][targetIdx];
                                    if (shipControlModule != null) { aiTarget = shipControlModule; }
                                    else
                                    {
                                        // Find next non-null ship. i.e. one that hasn't been destroyed AND is inside the theatre area
                                        targetIdx = squadronShipList[targetSquadronIdx].FindIndex(targetIdx, shp => shp != null && shp.shipInstance != null && theatreBounds.Contains(shp.shipInstance.TransformPosition));
                                        // If no ships at end of list, start at beginning
                                        if (targetIdx < 0) { targetIdx = squadronShipList[targetSquadronIdx].FindIndex(shp => shp != null && shp.shipInstance != null && theatreBounds.Contains(shp.shipInstance.TransformPosition)); }

                                        if (targetIdx >= 0) { aiTarget = squadronShipList[targetSquadronIdx][targetIdx]; }
                                    }
                                }
                                break;
                        }

                        //Debug.Log("[DEBUG] dcm.UpdateSquadron ship " + i.ToString("000") + " assigned target: " + (aiTarget != null).ToString());

                        // If no target, this is still valid.
                        // TODO - test if a null target is handled correctly in v1.06+
                        aiScript.SetState(attackStateID);
                        aiScript.AssignTargetShip(aiTarget);

                        // After the target has been assigned the first time, setup the callback method
                        if (isFirstTime) { saScript.callbackGetNewTarget = GetNewTarget; }
                    }
                }
            }

            //Debug.Log("[DEBUG] updated squadron " + squadron.squadronName + ", which is targetting " + squadronList[targetSquadronIdx].squadronName + ". Potential targets: " + shipsListTargetLength);

        }

        #endregion

        #region Public Member Methods

        ///// <summary>
        ///// Demo custom AIState
        ///// </summary>
        ///// <param name="stateMethodParameters"></param>
        //private static void DemoAttackState(AIStateMethodParameters stateMethodParameters)
        //{           
        //    if (stateMethodParameters.targetShip != null)
        //    {
        //        // Pre-calculation
        //        Vector3 fromTargetShipVector = stateMethodParameters.shipControlModule.shipInstance.TransformPosition - stateMethodParameters.targetShip.TransformPosition;
        //        float distToTargetShip = fromTargetShipVector.magnitude;
        //        float approxPursueInterceptionTime = distToTargetShip / stateMethodParameters.shipControlModule.shipInstance.WorldVelocity.magnitude;

        //        // Priority #1: Obstacle avoidance
        //        stateMethodParameters.aiBehaviourInputsList[0].behaviourType = AIBehaviourInput.AIBehaviourType.CustomObstacleAvoidance;
        //        stateMethodParameters.aiBehaviourInputsList[0].weighting = 1f;

        //        // Priority #2: Pursue/seek target ship
        //        if (approxPursueInterceptionTime > 3f)
        //        {
        //            stateMethodParameters.aiBehaviourInputsList[1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomPursuitArrival;
        //            //stateMethodParameters.aiBehaviourInputsList[1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomSeekMovingArrival;
        //        }
        //        else
        //        {
        //            stateMethodParameters.aiBehaviourInputsList[1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomSeekMovingArrival;
        //        }

        //        stateMethodParameters.aiBehaviourInputsList[1].targetPosition = stateMethodParameters.targetShip.TransformPosition;
        //        stateMethodParameters.aiBehaviourInputsList[1].targetVelocity = stateMethodParameters.targetShip.WorldVelocity;
        //        stateMethodParameters.aiBehaviourInputsList[1].weighting = 1f;

        //        // Set the state action as completed once the target ship is destroyed
        //        if (stateMethodParameters.targetShip.Destroyed())
        //        {
        //            stateMethodParameters.shipAIInputModule.SetHasCompletedStateAction(true);
        //        }
        //    }
        //    else
        //    {
        //        // If the target ship is null, do nothing
        //        stateMethodParameters.aiBehaviourInputsList[0].behaviourType = AIBehaviourInput.AIBehaviourType.Idle;
        //        stateMethodParameters.aiBehaviourInputsList[0].weighting = 1f;
        //    }
        //}

        /// <summary>
        /// The state method for the custom demo attack state.
        /// </summary>
        /// <param name="stateMethodParameters"></param>
        private void DemoAttackState(AIStateMethodParameters stateMethodParameters)
        {
            if (stateMethodParameters.targetShip != null)
            {
                // Pre-calculation
                Vector3 fromTargetShipVector = stateMethodParameters.shipControlModule.shipInstance.TransformPosition - stateMethodParameters.targetShip.TransformPosition;
                float distToTargetShip = fromTargetShipVector.magnitude;
                float approxPursueInterceptionTime = distToTargetShip / stateMethodParameters.shipControlModule.shipInstance.WorldVelocity.magnitude;
                float approxEvadeInterceptionTime = distToTargetShip / stateMethodParameters.targetShip.WorldVelocity.magnitude;

                // Priority #1: Obstacle avoidance
                stateMethodParameters.aiBehaviourInputsList[0].behaviourType = AIBehaviourInput.AIBehaviourType.CustomObstacleAvoidance;
                stateMethodParameters.aiBehaviourInputsList[0].weighting = 1f;

                bool ourShipInTheatre = theatreBounds.Contains(stateMethodParameters.shipControlModule.shipInstance.TransformPosition);
                bool targetShipInTheatre = theatreBounds.Contains(stateMethodParameters.targetShip.TransformPosition);

                // If either our ship or the target ship is outside the theatre, simply go back to home base
                if (!ourShipInTheatre || !targetShipInTheatre)
                {
                    // Priority #2: Seek home base
                    stateMethodParameters.aiBehaviourInputsList[1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomSeek;
                    stateMethodParameters.aiBehaviourInputsList[1].targetPosition = theatreBounds.center;
                    stateMethodParameters.aiBehaviourInputsList[1].weighting = 1f;

                    // If the target ship is outside the theatre, ask for a new target by stating
                    // the current state action (attacking the target ship) has been completed
                    if (!targetShipInTheatre) { stateMethodParameters.shipAIInputModule.SetHasCompletedStateAction(true); }
                }
                else
                {
                    // Priority #2: Evade target ship's targeting region
                    stateMethodParameters.aiBehaviourInputsList[1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomUnblockCone;
                    stateMethodParameters.aiBehaviourInputsList[1].targetPosition = stateMethodParameters.targetShip.TransformPosition;
                    stateMethodParameters.aiBehaviourInputsList[1].targetForwards = stateMethodParameters.targetShip.TransformForward;
                    stateMethodParameters.aiBehaviourInputsList[1].targetFOVAngle = 5f;
                    stateMethodParameters.aiBehaviourInputsList[1].weighting = 0.2f;

                    // Priority #3: Evade target ship (if we are "in front" of the target ship and within 3 seconds evade interception time)
                    if (Vector3.Dot(fromTargetShipVector, stateMethodParameters.targetShip.TransformForward) > 0f &&
                        Vector3.Dot(fromTargetShipVector, stateMethodParameters.shipControlModule.shipInstance.TransformForward) > 0f &&
                        approxEvadeInterceptionTime < 3f)
                    {
                        stateMethodParameters.aiBehaviourInputsList[2].behaviourType = AIBehaviourInput.AIBehaviourType.CustomFlee;
                        stateMethodParameters.aiBehaviourInputsList[2].targetPosition = stateMethodParameters.targetShip.TransformPosition;
                        stateMethodParameters.aiBehaviourInputsList[2].targetVelocity = stateMethodParameters.targetShip.WorldVelocity;
                        stateMethodParameters.aiBehaviourInputsList[2].weighting = 1f;
                    }

                    // Priority #4: Pursue/seek target ship
                    if (approxPursueInterceptionTime > 3f && approxPursueInterceptionTime < 10f)
                    {
                        stateMethodParameters.aiBehaviourInputsList[3].behaviourType = AIBehaviourInput.AIBehaviourType.CustomPursuitArrival;
                    }
                    else
                    {
                        stateMethodParameters.aiBehaviourInputsList[3].behaviourType = AIBehaviourInput.AIBehaviourType.CustomSeekMovingArrival;
                    }
                    stateMethodParameters.aiBehaviourInputsList[3].targetPosition = stateMethodParameters.targetShip.TransformPosition;
                    stateMethodParameters.aiBehaviourInputsList[3].targetVelocity = stateMethodParameters.targetShip.WorldVelocity;
                    stateMethodParameters.aiBehaviourInputsList[3].weighting = 1f;

                    // Set the state action as completed once the target ship is destroyed
                    // TODO: should possibly choose different action if the target ship is destroyed?
                    if (stateMethodParameters.targetShip.Destroyed())
                    {
                        stateMethodParameters.shipAIInputModule.SetHasCompletedStateAction(true);
                    }
                }   
            }
            else
            {
                // Fallback: If the target ship is null (which it shouldn't be) simply seek the target position with obstacle avoidance

                // Priority #1: Obstacle avoidance
                stateMethodParameters.aiBehaviourInputsList[0].behaviourType = AIBehaviourInput.AIBehaviourType.CustomObstacleAvoidance;
                stateMethodParameters.aiBehaviourInputsList[0].weighting = 1f;

                // Priority #2: Seek target position
                stateMethodParameters.aiBehaviourInputsList[1].behaviourType = AIBehaviourInput.AIBehaviourType.CustomSeek;
                stateMethodParameters.aiBehaviourInputsList[1].targetPosition = stateMethodParameters.targetPosition;
                stateMethodParameters.aiBehaviourInputsList[1].weighting = 1f;
            }
        }

        /// <summary>
        /// Find a new target ship for the ship supplied.
        /// TODO - Fix GC in GetNewTarget
        /// </summary>
        /// <param name="shipControlModule"></param>
        /// <returns></returns>
        public ShipControlModule GetNewTarget(ShipControlModule shipControlModule)
        {
            ShipControlModule aiTarget = null;
            ShipControlModule potentialTarget = null;

            // Determine which squadron this ship belongs to
            if (shipControlModule != null && shipControlModule.shipInstance != null)
            {
                // Get the source Squadron index in the list of squadrons
                int squadronId = shipControlModule.shipInstance.squadronId;

                // Use a for-loop on small list instead of a FindIndex which causes GC.Alloc.
                int squadronIdx = -1;
                for (int i = 0; i < numSquadrons; i++)
                {
                    if (squadronList[i].squadronId == squadronId) { squadronIdx = i; break; }
                }

                // Get the target squadron for this ship
                int targetSquadronId = squadronTargetList[squadronIdx];

                // Use a for-loop on small list instead of a FindIndex which causes GC.Alloc.
                int targetSquadronIdx = -1;
                for (int i = 0; i < numSquadrons; i++)
                {
                    if (squadronList[i].squadronId == targetSquadronId) { targetSquadronIdx = i; break; }
                }

                if (targetSquadronIdx < 0) { targetSquadronIdx = squadronIdx; }

                int shipsListLength = numSquadronShipsList[squadronIdx];
                int shipsListTargetLength = numSquadronShipsList[targetSquadronIdx];
                int targetIdx = -1;

                switch (aiTargets)
                {
                    // Ships that are inactive will be targetted, but maybe that's ok for now
                    case AITargets.RandomPlayer:
                        //if (playersArray != null)
                        {
                            PlayerInputModule playerInputModule = playersArray[Random.Range(0, playersArray.Length)];
                            if (playerInputModule != null) { aiTarget = playerInputModule.GetComponent<ShipControlModule>(); }
                        }
                        break;
                    case AITargets.NextShip:
                        // Get the current index of the source ship in the squadron
                        int sourceShipIdx = squadronShipList[squadronIdx].FindIndex(scm => scm == shipControlModule);
                        //Debug.Log("[DEBUG] sourceShipIdx: " + sourceShipIdx);

                        // Find the next ship in the same squadron as the current ship
                        targetIdx = (sourceShipIdx == shipsListLength - 1) ? 0 : sourceShipIdx + 1;
                        potentialTarget = squadronShipList[squadronIdx][targetIdx];

                        // Check that the next ship is not null and is inside the theatre area
                        if (potentialTarget != null && potentialTarget.shipInstance != null && theatreBounds.Contains(potentialTarget.shipInstance.TransformPosition)) { aiTarget = potentialTarget; }
                        else
                        {
                            // Find next non-null ship. i.e. one that hasn't been destroyed
                            targetIdx = squadronShipList[squadronIdx].FindIndex(targetIdx, shp => shp != null && shp.shipInstance != null && theatreBounds.Contains(shp.shipInstance.TransformPosition));
                            // If no ships at end of list, start at beginning
                            if (targetIdx < 0) { targetIdx = squadronShipList[squadronIdx].FindIndex(shp => shp != null && shp.shipInstance != null && theatreBounds.Contains(shp.shipInstance.TransformPosition)); }

                            // Make sure the ship doesn't target itself!
                            if (targetIdx >= 0 && targetIdx != sourceShipIdx) { aiTarget = squadronShipList[squadronIdx][targetIdx]; }
                        }
                        break;
                    case AITargets.RandomShip:
                        // If there are potential targets, find a random ship
                        if (shipsListTargetLength > 0)
                        {
                            targetIdx = Random.Range(0, shipsListTargetLength);
                            potentialTarget = squadronShipList[targetSquadronIdx][targetIdx];
                            // Check that the target ship is not null and is inside the theatre area
                            if (potentialTarget != null && potentialTarget.shipInstance != null && theatreBounds.Contains(potentialTarget.shipInstance.TransformPosition)) { aiTarget = potentialTarget; }
                            else
                            {
                                // Find next non-null ship. i.e. one that hasn't been destroyed AND is inside the theatre area.
                                targetIdx = squadronShipList[targetSquadronIdx].FindIndex(targetIdx, shp => shp != null && shp.shipInstance != null && theatreBounds.Contains(shp.shipInstance.TransformPosition));
                                // If no ships at end of list, start at beginning
                                if (targetIdx < 0) { targetIdx = squadronShipList[targetSquadronIdx].FindIndex(shp => shp != null && shp.shipInstance != null && theatreBounds.Contains(shp.shipInstance.TransformPosition)); }

                                if (targetIdx >= 0) { aiTarget = squadronShipList[targetSquadronIdx][targetIdx]; }
                                else
                                {
                                    // Cannot find a target ship in the target squadron, so attempt to find another (enemy) squadron.
                                    targetSquadronId = GetTargetSquadron(shipControlModule.shipInstance.factionId);
                                    if (targetSquadronId >= 0) { squadronTargetList[squadronIdx] = targetSquadronId; }
                                }
                            }

                            // TODO - TEST CODE REMOVE
                            //if (aiTarget != null) { Debug.Log("[DEBUG] " + shipControlModule.name + " target: " + aiTarget.name + " " + aiTarget.shipInstance.TransformPosition + " in " + theatreBounds.ToString());  }
                        }
                        break;
                }
            }

            //if (shipControlModule != null)
            //{
            //    Debug.Log("[DEBUG] dcm.GetNewTarget(..) for " + shipControlModule.gameObject.name + " target: " + (aiTarget == null ? "no target" : aiTarget.gameObject.name));
            //}

            return aiTarget;
        }

        /// <summary>
        /// Get the next non-null ship starting from the zero-based index in the
        /// list of ships within the given squadron.
        /// </summary>
        /// <param name="squadronId"></param>
        /// <param name="startIdx"></param>
        /// <returns></returns>
        public ShipControlModule GetNextShip(int squadronId, int startIdx)
        {
            ShipControlModule shipControlModule = null;

            if (isInitialised && squadronId >= 0)
            {
                Squadron squadron = null;
                int squadronIdx = -1;

                // Get the correct squadron. This can change at runtime
                for (int sqIdx = 0; sqIdx < numSquadrons; sqIdx++)
                {
                    if (squadronList[sqIdx].squadronId == squadronId)
                    {
                        squadronIdx = sqIdx;
                        squadron = squadronList[sqIdx];
                        break;
                    }
                }

                // Found the requested squadron
                if (squadronIdx >= 0)
                {
                    // Validate the startIdx
                    if (startIdx < numSquadronShipsList[squadronIdx])
                    {
                        // Find next non-null ship. i.e. one that hasn't been destroyed
                        int shipIdx = squadronShipList[squadronIdx].FindIndex(startIdx, shp => shp != null);
                        // If no ships at end of list, start at beginning
                        if (shipIdx < 0) { shipIdx = squadronShipList[squadronIdx].FindIndex(shp => shp != null); }

                        if (shipIdx >= 0) { shipControlModule = squadronShipList[squadronIdx][shipIdx]; }
                    }
                }
            }

            return shipControlModule;
        }

        /// <summary>
        /// Randomly select a squadron from an opposing faction.
        /// NOTE: It currently does not check to see how many ships are in the squadrons as that
        /// might be a performance overhead.
        /// </summary>
        /// <param name="factionId"></param>
        /// <returns></returns>
        public int GetTargetSquadron(int factionId)
        {
            // Find all he squadrons in a different faction
            List<Squadron> squadronsToTargetList = squadronList.FindAll(sq => sq.factionId != factionId);

            int numTargetSquadrons = squadronsToTargetList == null ? 0 : squadronsToTargetList.Count;

            if (numTargetSquadrons > 0)
            {
                // Randomly select a squadron from an opposing faction
                return squadronsToTargetList[UnityEngine.Random.Range(0, numTargetSquadrons)].squadronId;
            }
            else { return -1; }
        }

        /// <summary>
        /// Get the next squadron which has ships. If the squadronId is unassigned (-1),
        /// the first squadron with ships (if any) will be returned.
        /// </summary>
        /// <param name="squadronId"></param>
        /// <returns></returns>
        public int GetNextSquadronWithShips(int squadronId)
        {
            // Get the index of the squadron within the list of squadrons
            // Use a for-loop on small list instead of a FindIndex which causes GC.Alloc.
            int squadronIdx = -1;

            // If the squadronId is unassigned (typically -1 by default), attempt to find the first squadron with ships
            if (squadronId < 0)
            {
                for (int k = 0; k < numSquadrons; k++)
                {
                    if (squadronShipList[k].Exists(shp => shp != null)) { squadronIdx = k; break; }
                }
            }
            else
            {
                for (int i = 0; i < numSquadrons; i++)
                {
                    if (squadronList[i].squadronId == squadronId)
                    {
                        // Get the next squadron with ships
                        for (int j = i + 1; j < numSquadrons; j++)
                        {
                            if (squadronShipList[j].Exists(shp => shp != null)) { squadronIdx = j; break; }
                        }

                        // Did we find the next squadron with ships?
                        if (squadronIdx > i)
                        {
                            break;
                        }
                        else
                        {
                            // Loop back to beginning of squadron list and process previous squadrons
                            for (int k = 0; k < i; k++)
                            {
                                if (squadronShipList[k].Exists(shp => shp != null)) { squadronIdx = k; break; }
                            }
                        }
                    }
                }
            }

            if (squadronIdx >= 0) { return squadronList[squadronIdx].squadronId; }
            else { return -1; } 
        }

        #endregion

        #region Private Member Methods

        private void ShipDestroyCallBack(Ship ship)
        {
            // Need to determine which squadron ship belonged to.
            // OR just keep track of #ships in each faction

            //Debug.Log("Ship Destroyed: " + ship.shipId);
        }


        #endregion

        #region Event Methods

        // Test only - Should use new gui instead.
        private void OnGUI()
        {
            for (int sqIdx = 0; sqIdx < numSquadrons; sqIdx++)
            {
                GUI.Label(new Rect(10, 50 + (sqIdx * 20), 100, 20), squadronShipList[sqIdx].Count(sp => sp != null).ToString());
            }
        }

        #endregion
    }
}