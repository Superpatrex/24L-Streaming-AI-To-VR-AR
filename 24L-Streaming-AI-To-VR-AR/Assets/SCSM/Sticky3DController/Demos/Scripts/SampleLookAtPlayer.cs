using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple script to have a Sticky3D character look at (and optionally walk toward) a player character.
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// SETUP
    /// 1. Add this script to NEW CHILD gameobject under a S3D (NPC) character in the scene (not the player)
    /// 2. Make sure the sphere collider is set to Is Trigger (and it's on the parent gameobject)
    /// 3. Add the player S3D character from the scene to this script in the slot provided
    /// 4. Adjust the sphere collider radius to determine when this character should start looking at the player
    /// 5. Run the scene and have the player walk towards the this S3D character.
    /// 6. If you have StickyWeapons in the scene, create a Unity layer called say "WeaponsIgnore".
    /// 7. Set this sphere collider's gameobject to the WeaponsIgnore Unity layer.
    /// 8. On all StickyWeapons, on General Tab, exclude "WeaponsIgnore" in the "Hit Layer Mask"
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    [AddComponentMenu("Sticky3D Controller/Samples/Look at Player")]
    [DisallowMultipleComponent]
    public class SampleLookAtPlayer : MonoBehaviour
    {
        #region Public Variables
        public bool initialiseOnStart = true;
        [Tooltip("The S3D character to look at")]
        public StickyControlModule player = null;
        [Tooltip("Should the NPC character walk towards the player")]
        public bool walkTowardsPlayer = false;
        [Tooltip("How close the NPC can get to the player")]
        [Range(0f, 20f)] public float minApproachDistance = 1.5f;
        [Tooltip("How many seconds to delay turning toward (and optionally walking toward) the player")]
        public float delayTurnAndWalkTime = 0f;
        [Tooltip("The local space offset the NPC should have when approaching the player")]
        public Vector3 walkOffset = Vector3.zero;
        [Tooltip("Turn on Head IK and set the target to the player. Set a delay turn time for increased realism")]
        public bool useHeadIK = false;
        [Tooltip("Check if there is anything between the character and the object")]
        public bool checkLOS = false;
        #endregion

        #region Private Variables
        private StickyControlModule thisCharacter = null;
        private Collider distanceCollider = null;
        private bool isInitialised = false;
        private bool isLookAtPlayer = false;
        private bool isUsingHeadIK = false;
        private CharacterInput characterInput = null;
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Private Methods

        private void ConfigureHeadIK()
        {
            if (!thisCharacter.IsHeadIKEnabled) { thisCharacter.EnableHeadIK(true); }

            thisCharacter.SetHeadIKTarget(player.transform, Vector3.zero, true, true);
            isUsingHeadIK = true;
        }

        private void DelayedFacePlayer()
        {
            isLookAtPlayer = true;
        }

        /// <summary>
        /// Look and optionally walk, towards the player
        /// </summary>
        private void LookTowardsPlayer()
        {
            //Vector3 playerWorldPosition = player.transform.position;
            Vector3 thisCharacterWorldPosition = transform.position;

            // Get the world space vector the NPC character should move
            Vector3 worldVelocity = player.GetWorldPosition(walkOffset) -  thisCharacterWorldPosition;

            // Get the local space vector
            float deltaX = Vector3.Dot(transform.right, worldVelocity);
            float deltaZ = Vector3.Dot(transform.forward, worldVelocity);

            Vector3 localVelocity = new Vector3(deltaX, 0f, deltaZ);
            Vector3 localVelocityN = localVelocity.sqrMagnitude < Mathf.Epsilon ? Vector3.zero : localVelocity.normalized;

            // Optionally walk towards the player
            if (walkTowardsPlayer)
            {
                if (minApproachDistance == 0f || worldVelocity.sqrMagnitude > minApproachDistance * minApproachDistance)
                {
                    characterInput.verticalMove = localVelocityN.z;
                }
                else { characterInput.verticalMove = 0f; }
            }
            else { characterInput.verticalMove = 0f; }

            characterInput.horizontalLook = localVelocityN.x;

            thisCharacter.SendInput(characterInput);
        }

        /// <summary>
        /// Get eye level of the player, then check if there is line of sight
        /// from the eye level of this character to the player.
        /// </summary>
        /// <returns></returns>
        private bool IsPlayerInLoS()
        {
            return player != null && thisCharacter.IsInLineOfSight(player.GetWorldEyePosition(), true, false);
        }

        #endregion

        #region Update Methods

        private void Update()
        {
            if (isLookAtPlayer && isInitialised && !thisCharacter.IsMoveFixedUpdate)
            {
                LookTowardsPlayer();
            }
        }

        private void FixedUpdate()
        {
            if (isLookAtPlayer && isInitialised && thisCharacter.IsMoveFixedUpdate)
            {
                LookTowardsPlayer();
            }
        }

        #endregion

        #region Event Methods

        private void OnTriggerEnter(Collider other)
        {
            if (isInitialised)
            {
                // Is this the player collider and can we see them?
                if (player.IsColliderSelf(other) && (!checkLOS || IsPlayerInLoS()))
                {
                    if (useHeadIK) { ConfigureHeadIK(); }

                    if (delayTurnAndWalkTime > 0f)
                    {
                        Invoke("DelayedFacePlayer", delayTurnAndWalkTime);
                    }
                    else
                    {
                        isLookAtPlayer = true;
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (isInitialised)
            {
                // Is this the player collider
                if (player.IsColliderSelf(other))
                {
                    StopLookingAtPlayer();
                }
            }
        }

        #endregion

        #region Public Methods

        public void Initialise()
        {
            if (isInitialised) { return; }

            thisCharacter = GetComponentInParent<StickyControlModule>();

            if (thisCharacter == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleLookAtPlayer on " + name + " - did you forget to attach this script to a child gameobject of one of our non-player S3D characters?");
                #endif
            }
            else if (!thisCharacter.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleLookAtPlayer on " + name + " - your character " + thisCharacter.name + " is not initialised");
                #endif
            }
            else if (!TryGetComponent(out distanceCollider))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleLookAtPlayer on " + name + " requires a Sphere collider");
                #endif
            }
            else if (!distanceCollider.isTrigger)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleLookAtPlayer on " + name + " requires a trigger Sphere collider");
                #endif
            }
            else if (player == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleLookAtPlayer on " + name + " requires a Sticky3D player to be added to this script");
                #endif
            }
            else
            {
                thisCharacter.SetIsNPC(true);

                // Prevent StickyWeapon beams or projectiles hitting this proximity collider
                thisCharacter.RegisterWeaponNonHitCollider(distanceCollider);

                // Create a new input class so we can tell the character where to look
                characterInput = new CharacterInput();
                isInitialised = true;
            }
        }

        public void StopLookingAtPlayer()
        {
            if (isInitialised)
            {
                //Debug.Log("[DEBUG] StopLookingAtPlayer isLookAtPlayer " + isLookAtPlayer + " T:" + Time.time);

                // Are we currently looking at the player?
                if (isLookAtPlayer)
                {
                    isLookAtPlayer = false;
                }
                // Are we about to look at the player?
                else if (delayTurnAndWalkTime > 0f)
                {
                    CancelInvoke("DelayedFacePlayer");
                }

                // Are we currently using Head IK to look towards the player?
                if (isUsingHeadIK && useHeadIK && thisCharacter.IsHeadIKEnabled)
                {
                    // Rather than setting the target to null with SetHeadIKTarget(null, Vector3.zero, true),
                    // which would snap the head to straight ahead, smoothly disable Head IK.
                    thisCharacter.DisableHeadIK(true);
                    isUsingHeadIK = false;
                }
            }
        }

        /// <summary>
        /// Enable or disable walk toward player.
        /// </summary>
        /// <param name="isEnabled"></param>
        public void WalkTowardPlayer (bool isEnabled)
        {
            walkTowardsPlayer = isEnabled;

            // If not already looking at the player, do that now
            if (isEnabled && isInitialised && !isLookAtPlayer)
            {
                isLookAtPlayer = true;

                if (useHeadIK && !isUsingHeadIK) { ConfigureHeadIK(); }
            }
        }

        #endregion
    }
}