using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple script to have a Non-Player-Character (NPC) Sticky3D character follow another
    /// character using a navmesh.
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// IMPORTANT - Unity currently does not support moving a NavMesh. So this will
    /// not work when the objects a navmesh is baked from are moving.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Navmesh Follow Player")]
    public class SampleNavmeshFollowPlayer : MonoBehaviour
    {
        #region Public Variables
        public StickyControlModule playerToFollow = null;
        public GameObject waypoints = null;
        [Tooltip("How long the NPC can loose sight of player before resuming patrol")]
        public float lostLineOfSightTime = 0.1f;
        [Tooltip("How long the player needs to be in sight of NPC")]
        public float playerDetectionTime = 1f;
        [Tooltip("How close the NPC can get to the player")]
        [Range(0f, 20f)] public float minApproachDistance = 3.0f;
        #endregion

        #region Public Delegate

        public delegate void CallbackOnGotLOS(StickyControlModule s3dSource, StickyControlModule s3dTarget);
        public delegate void CallbackOnLostLOS(StickyControlModule s3dSource, StickyControlModule s3dTarget);

        /// <summary>
        /// Method to be called when the NPC gets Line-of-Sight to the player.
        /// </summary>
        public CallbackOnGotLOS callbackOnGotLOS = null;

        // Method to be called when the NPC looses Line-of-Sight to the player.
        public CallbackOnLostLOS callbackOnLostLOS = null;

        #endregion

        #region Private Variables
        private StickyControlModule s3dNPC = null;
        private NavMeshAgent navMeshAgent = null;
        private bool isInitialised = false;
        private CharacterInput npcInput;
        private bool isPatrolling = false;
        private List<Transform> waypointList;
        private int numWayPoints = 0;
        private int lastWayPoint = 0;
        private float lostLoSTimer = 0f;
        private float playerDetectionTimer = 0f;
        private int npcId = 0;
        private bool hasGotLOS = false;
        #endregion

        #region Initialise Methods
        // Start is called before the first frame update
        void Start()
        {
            GetWayPoints();

            if (!TryGetComponent(out s3dNPC))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: Cannot find the Sticky3D Controller component for this NPC. This component should be attached to a NPC character.");
                #endif
            }
            else if (!TryGetComponent(out navMeshAgent))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: Cannot find the NavMeshAgent component for this NPC. This component should be attached to a NPC character.");
                #endif
            }
            else if (playerToFollow == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: No Sticky3D Controller player to follow.");
                #endif
            }
            else if (waypoints == null)
            {
                 #if UNITY_EDITOR
                Debug.LogWarning("ERROR: Could not find waypoint parent gameobject. This contains waypoint objects or locations where the NPC will patrol.");
                #endif
            }
            else if (numWayPoints < 2)
            {
                 #if UNITY_EDITOR
                Debug.LogWarning("ERROR: You need at least 2 waypoints for the NPC to patrol");
                #endif
            }
            else
            {
                navMeshAgent.updatePosition = false;
                navMeshAgent.updateRotation = false;
                npcInput = new CharacterInput();
                npcId = s3dNPC.StickyID;
                isPatrolling = true;

                // Set up stuck notification
                if (s3dNPC.stuckTime == 0f) { s3dNPC.stuckTime = 2f; }
                s3dNPC.callbackOnStuck = NPCisStuck;

                lastWayPoint = GetBestWayPoint();
                isInitialised = true;

                MoveTowardWaypoint(lastWayPoint);
            }
        }

        #endregion

        #region Update Methods
        // Update is called once per frame
        void Update()
        {
            if (isInitialised)
            {
                bool isPlayerInLoS = IsPlayerInLoS();

                if (isPlayerInLoS)
                {
                    if (isPatrolling)
                    {
                        if (playerDetectionTimer < playerDetectionTime)
                        {
                            playerDetectionTimer += Time.deltaTime;
                        }
                        else { isPatrolling = false; }
                    }
                    
                    if (isPatrolling && s3dNPC.IsRightHandHoldingWeapon)
                    {
                        // Did the NPC just get stable LoS on the player?
                        if (!hasGotLOS && callbackOnGotLOS != null)
                        {
                            callbackOnGotLOS.Invoke(s3dNPC, playerToFollow);
                        }
                    }

                    hasGotLOS = true;
                    lostLoSTimer = 0f;
                }
                else
                {
                    playerDetectionTimer = 0f;

                    lostLoSTimer += Time.deltaTime;
                    if (lostLoSTimer > lostLineOfSightTime)
                    {
                        if (!isPatrolling)
                        {
                            lastWayPoint = GetBestWayPoint();
                            isPatrolling = true;
                        }

                        hasGotLOS = false;
                        if (callbackOnLostLOS != null)
                        {
                            callbackOnLostLOS.Invoke(s3dNPC, playerToFollow);
                        }
                    }
                }

                if (isPatrolling)
                {
                    Patrol();
                }
                else
                {
                    FollowPlayer();
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Find the positions of all the waypoints the NPC will patrol
        /// </summary>
        private void GetWayPoints()
        {
            if (waypoints != null)
            {
                numWayPoints = waypoints.transform.childCount;

                if (numWayPoints > 0)
                {
                    waypointList = new List<Transform>(numWayPoints);

                    for (int wpIdx = 0; wpIdx < numWayPoints; wpIdx++)
                    {
                        waypointList.Add(waypoints.transform.GetChild(wpIdx));
                    }
                }
            }
        }

        /// <summary>
        /// The NPC will attempt to follow the player
        /// </summary>
        private void FollowPlayer()
        {
            if (playerToFollow == null || playerToFollow.IsRespawning)
            {
                lastWayPoint = GetBestWayPoint();
                playerDetectionTimer = 0f;
                isPatrolling = true;
                hasGotLOS = false;
            }
            else
            {
                Vector3 playerWorldPosition = playerToFollow.transform.position;
                Vector3 npcWorldPosition = transform.position;

                if (navMeshAgent.enabled)
                {
                    // Check how close the NPC is to the player
                    Vector3 targetPosition = npcWorldPosition;
                    // Get the vector looking from the player to the NPC
                    Vector3 targetDir = npcWorldPosition - playerWorldPosition;
                    float targetDistSqr = targetDir.sqrMagnitude;
                    float minDistanceSqr = minApproachDistance * minApproachDistance;

                    if (targetDistSqr > minDistanceSqr)
                    {
                        targetPosition = playerWorldPosition;
                    }
                    else if (targetDistSqr > Vector3.kEpsilon)
                    {
                        targetPosition = playerWorldPosition + (S3DMath.Normalise(targetDir) * minApproachDistance);
                    }

                    navMeshAgent.SetDestination(targetPosition);

                    // Get the world space vector the NPC character should move
                    Vector3 worldVelocity = navMeshAgent.nextPosition - npcWorldPosition;

                    // Get the local space vector
                    float deltaX = Vector3.Dot(transform.right, worldVelocity);
                    float deltaZ = Vector3.Dot(transform.forward, worldVelocity);

                    Vector3 localVelocity = new Vector3(deltaX, 0f, deltaZ);
                    Vector3 localVelocityN = localVelocity.normalized;

                    npcInput.verticalMove = localVelocityN.z;
                    npcInput.horizontalLook = localVelocityN.x;

                    s3dNPC.SendInput(npcInput);
                }
            }
        }

        /// <summary>
        /// Find the closest waypoint
        /// </summary>
        /// <returns></returns>
        private int GetBestWayPoint()
        {
            int bestWayPointIndex = -1;

            float closestWaypointDistance = float.MaxValue;

            Vector3 npcPosition = transform.position;

            for (int wpIdx = 0; wpIdx < numWayPoints; wpIdx++)
            {
                Vector3 wayPointPos = waypointList[wpIdx].position;

                float distanceToWaypoint = Vector3.Distance(npcPosition, wayPointPos);

                if (distanceToWaypoint < closestWaypointDistance)
                {
                    closestWaypointDistance = distanceToWaypoint;
                    bestWayPointIndex = wpIdx;
                }
            }

            return bestWayPointIndex;
        }

        /// <summary>
        /// Get the next waypoint zero-based index. If there isn't a valid
        /// waypoint, return -1.
        /// </summary>
        /// <returns></returns>
        private int GetNextWayPoint()
        {
            if (numWayPoints < 2) { return -1; }
            else if (lastWayPoint >= numWayPoints - 1) { return 0; }
            else { return lastWayPoint + 1; }
        }

        /// <summary>
        /// The NPC will patrol between the waypoints. If it sights the player,
        /// it will attempt to follow it.
        /// </summary>
        private void Patrol()
        {
            if (numWayPoints > 1 && numWayPoints > lastWayPoint)
            {
                float distToWaypoint = Vector3.Distance(transform.position, waypointList[lastWayPoint].position);

                //Debug.Log("[DEBUG] distance to waypoint " + (lastWayPoint + 1) + ": " + distToWaypoint + " nmRemaining: " + navMeshAgent.remainingDistance + " T:" + Time.time);

                //if (navMeshAgent.remainingDistance < 1f)
                if (distToWaypoint < 1f)
                {
                    int nextWaypoint = GetNextWayPoint();
                    if (nextWaypoint >= 0)
                    {
                        MoveTowardWaypoint(nextWaypoint);
                        lastWayPoint = nextWaypoint;
                    }
                }
                else
                {
                    MoveTowardWaypoint(lastWayPoint);
                }
            }
        }

        /// <summary>
        /// Get eye level of the player, then check if there is line of sight
        /// from the eye level of the NPC to the player.
        /// </summary>
        /// <returns></returns>
        private bool IsPlayerInLoS()
        {
            return playerToFollow != null && s3dNPC.IsInLineOfSight(playerToFollow.GetWorldEyePosition(), true, true);
        }

        /// <summary>
        /// Tell the NPC to move towards a particular waypoint using the navmesh
        /// </summary>
        /// <param name="wayPointIndex"></param>
        private void MoveTowardWaypoint(int wayPointIndex)
        {
            if (numWayPoints > 0 && wayPointIndex < numWayPoints)
            {
                Transform waypointTfrm = waypointList[wayPointIndex];
                navMeshAgent.SetDestination(waypointTfrm.position);

                // Get the world space vector the NPC character should move
                Vector3 worldVelocity = navMeshAgent.nextPosition - transform.position;

                // Get the local space vector
                float deltaX = Vector3.Dot(transform.right, worldVelocity);
                float deltaZ = Vector3.Dot(transform.forward, worldVelocity);

                Vector3 localVelocity = new Vector3(deltaX, 0f, deltaZ);
                Vector3 localVelocityN = localVelocity.normalized;

                npcInput.verticalMove = localVelocityN.z;
                npcInput.horizontalLook = localVelocityN.x;

                s3dNPC.SendInput(npcInput);
            }
        }

        #endregion

        #region Public Callback Methods

        /// <summary>
        /// If the NPC gets stuck, S3D will call this method. We find the best waypoint,
        /// then teleport directly to that position and resume patrolling the area.
        /// </summary>
        /// <param name="stickyControlModule"></param>
        public void NPCisStuck(StickyControlModule stickyControlModule)
        {
            // Is this NPC stuck?
            if (stickyControlModule != null && stickyControlModule.StickyID == npcId)
            {
                lastWayPoint = GetBestWayPoint();

                if (lastWayPoint >= 0)
                {
                    Transform waypointTfrm = waypointList[lastWayPoint];
                    isPatrolling = true;

                    // When teleporting, ensure we don't land inside the waypoint object.
                    stickyControlModule.TelePort(waypointTfrm.position + (waypointTfrm.up * 0.25f), waypointTfrm.rotation, true);
                }
            }
        }

        #endregion
    }
}