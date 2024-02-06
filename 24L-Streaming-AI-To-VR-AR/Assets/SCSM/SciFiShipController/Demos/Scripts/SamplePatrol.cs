using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Executes a simple state machine for determining which state a "patrol" AI should be in.
    /// This is only sample to demonstrate how API calls could be used in your own code.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Patrol AI")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    [RequireComponent(typeof(ShipAIInputModule))]
    public class SamplePatrol : MonoBehaviour
    {
        #region Public Variables

        /// <summary>
        /// The enemy ship this AI ship should patrol for.
        /// </summary>
        public ShipControlModule enemyShip;
        /// <summary>
        /// The name of the path this AI should patrol.
        /// </summary>
        public string pathName = "Path Name Here";

        /// <summary>
        /// The maximum distance at which this AI ship can "see" the enemy ship.
        /// </summary>
        public float FOVDistance = 250f;
        /// <summary>
        /// The maximum angle (in the field of view) at which this AI ship can "see" the enemy ship.
        /// </summary>
        public float FOVAngle = 120f;
        /// <summary>
        /// The maximum time of losing visual contact with the enemy ship before this AI ship returns to patrolling.
        /// </summary>
        public float contactLostTime = 10f;

        #endregion

        #region Private Variables

        private float contactLostTimer = 0f;

        private ShipAIInputModule shipAIInputModule;
        private SSCManager sscManager;
        private PathData patrolPath;

        private RaycastHit hitInfo;

        private int lastPatrolPathIndex = 0;

        #endregion

        #region Initialise Methods

        void Awake()
        {
            // Get a reference to the ship AI input module attached to this gameobject
            shipAIInputModule = GetComponent<ShipAIInputModule>();
            // Initialise the ship AI (if it hasn't been initialised already)
            shipAIInputModule.Initialise();

            // Get a reference to the Ship Controller Manager instance
            sscManager = SSCManager.GetOrCreateManager(gameObject.scene.handle);
            // Find the Path
            patrolPath = sscManager.GetPath(pathName);

            #if UNITY_EDITOR
            if (patrolPath == null) { Debug.Log("SamplePatrol: Path not found: " + pathName); }
            #endif

            // Initialise the AI in the "move to" state
            shipAIInputModule.SetState(AIState.moveToStateID);
            shipAIInputModule.AssignTargetPath(patrolPath);
        }

        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            // First check which state the AI is currently in
            int currentStateID = shipAIInputModule.GetState();
            if (currentStateID == AIState.moveToStateID)
            {
                // We are currently in the "move to" state - patrolling the given path
                // If we see the enemy ship, switch to the dogfight state, and set the enemy ship as the target
                if (EnemyShipAlive() && EnemyShipVisible())
                {
                    shipAIInputModule.SetState(AIState.dogfightStateID);
                    shipAIInputModule.AssignTargetShip(enemyShip);
                    contactLostTimer = 0f;
                    // Remember the last patrol path index
                    lastPatrolPathIndex = shipAIInputModule.GetCurrentTargetPathLocationIndex();
                }
            }
            else if (currentStateID == AIState.dogfightStateID)
            {
                // We are currently in the "dogfight" state - attacking the enemy ship
                // If we lose sight of the enemy, increment the lost contact timer
                if (!EnemyShipVisible()) { contactLostTimer += Time.deltaTime; }
                else { contactLostTimer = 0f; }
                // If the enemy ship is destroyed or we lose sight of it for a given amount of time, switch back to 
                // the move to state to continue patrolling, and set the patrol path as the target
                //if (!EnemyShipAlive() || contactLostTimer > contactLostTime)
                if (shipAIInputModule.HasCompletedStateAction() || contactLostTimer > contactLostTime)
                {
                    shipAIInputModule.SetState(AIState.moveToStateID);
                    shipAIInputModule.AssignTargetPath(patrolPath);
                    // Set the current target path location index to the index we were at 
                    // when we last stopped following the patrol path
                    shipAIInputModule.SetCurrentTargetPathLocationIndex(lastPatrolPathIndex, 0f);
                }
            }
            else
            {
                // We are not in one of the above states, hence we are not in the state we are supposed to be in
                // Change to the move to state and continue patrolling
                shipAIInputModule.SetState(AIState.moveToStateID);
                shipAIInputModule.AssignTargetPath(patrolPath);           
            }
        }

        #endregion

        #region Public Member Methods

        /// <summary>
        /// Returns true if the enemy ship is alive.
        /// </summary>
        /// <returns></returns>
        public bool EnemyShipAlive ()
        {
            return enemyShip == null ? false : !enemyShip.shipInstance.Destroyed();
        }

        /// <summary>
        /// Returns true if the enemy ship is visible to this ship.
        /// </summary>
        /// <returns></returns>
        public bool EnemyShipVisible ()
        {
            // Check that the enemy ship is within the FOV distance
            Vector3 toEnemyShipVector = enemyShip.transform.position - transform.position;
            float sqrDistToEnemyShip = toEnemyShipVector.sqrMagnitude;
            if (sqrDistToEnemyShip < FOVDistance * FOVDistance)
            {
                // Check that the enemy ship is within the FOV angle
                float distToEnemyShip = Mathf.Sqrt(sqrDistToEnemyShip);
                if (Mathf.Acos(Vector3.Dot(transform.forward, toEnemyShipVector) / distToEnemyShip) < FOVAngle * Mathf.Deg2Rad / 2f)
                {
                    // Check that there aren't any obstacles obstructing the ship from view
                    if (Physics.Linecast(transform.position, enemyShip.transform.position, out hitInfo))
                    {
                        return (hitInfo.rigidbody != null && hitInfo.rigidbody.gameObject == enemyShip.gameObject);
                    }
                    else { return true; }
                }
                else { return false; }
            }
            else { return false; }
        }

        #endregion
    }
}
