using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple script to attach to weapon that an NPC can pick up and shoot.
    /// Currently used in NavMeshDemo scene.
    /// SETUP:
    /// 1. Add this component to a configured StickyWeapon prefab.
    /// 2. Add an Sticky3D character with "Non-Player Character"
    ///    enabled on the Move tab to the scene.
    /// 3. Place the weapon near the NPC
    /// 4. Set the weapon Fire Button 1 to Auto Fire
    /// 5. Set the Pickup Radius in this component.
    /// 6. Set the weapon targetSticky3D or targetTransform from your code
    ///    See SampleNavmeshFollowPlayer.cs for examples.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/NPC Fire Weapon")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class SampleNPCFireWeapon : MonoBehaviour
    {
        #region Public Variables

        [Tooltip("If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the component is enabled through code.")]
        public bool initialiseOnStart = false;

        [Tooltip("If an NPC character gets within this radius of the weapon, it will attempt to pick it up with the right hand")]
        [Range(0.1f, 5f)] public float pickupRadius = 2f;
        #endregion

        #region Private Variables - General

        private bool isInitialised = false;
        private StickyWeapon stickyWeapon = null;
        private SphereCollider sphereCollider = null;
        private bool isPickedUp = false;
        private int pickupCharacterStickyID = 0;
        #endregion

        #region Private Initialise Methods

        // Use this for initialization
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Events

        private void OnTriggerEnter (Collider other)
        {
            if (isInitialised && !isPickedUp)
            {
                Rigidbody rBody = other.attachedRigidbody;

                if (rBody != null)
                {
                    StickyControlModule pickupCharacter = null;

                    // Is this an S3D NPC that has entered the pickup zone for this weapon?
                    if (rBody.TryGetComponent(out pickupCharacter) && pickupCharacter.IsInitialised && pickupCharacter.IsNPC())
                    {
                        // Is the NPC already holding something in right hand?
                        if (pickupCharacter.IsRightHandHoldingInteractive)
                        {
                            // Alternatively they could attempt to pick up the weapon in the left hand
                            // or stash the weapon for later use.
                            #if UNITY_EDITOR
                            Debug.Log(pickupCharacter.name + " is already holding something and cannot pick up " + name);
                            #endif
                        }
                        else
                        {
                            // We could run some kind of pickup animation here but using the Animate API for the character.
                            // For simplicity, just instantly grab it

                            #if UNITY_EDITOR
                            Debug.Log(pickupCharacter.name + " is attempting to pick up " + name);
                            #endif

                            pickupCharacter.GrabInteractive(stickyWeapon, false, false);

                            isPickedUp = pickupCharacter.IsRightHandHoldingInteractive;

                            // Turn off the collider so it doesn't trigger other things
                            // Tell the NPC to update us when it gets or looses line-of-sight
                            // to the player.
                            if (isPickedUp)
                            {
                                sphereCollider.gameObject.SetActive(false);

                                // If this NPC is trying to follow the player, get it to
                                // update us with the details.
                                SampleNavmeshFollowPlayer followPlayerComponent = null;

                                if (pickupCharacter.TryGetComponent(out followPlayerComponent))
                                {
                                    followPlayerComponent.callbackOnGotLOS = GotLOS;
                                    followPlayerComponent.callbackOnLostLOS = LostLOS;
                                    pickupCharacterStickyID = pickupCharacter.StickyID;
                                }

                                #if UNITY_EDITOR
                                Debug.Log(pickupCharacter.name + " has picked up " + name);
                                #endif
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Initialise this component. Has no effect if already initialised
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            // Make sure this component looks like it is configured correctly
            if (!gameObject.TryGetComponent(out stickyWeapon))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleNPCFireWeapon - did you forget to attach this script to your StickyWeapon prefab?");
                #endif
            }
            else
            {
                if (!stickyWeapon.IsInitialised) { stickyWeapon.Initialise(); }

                // Add a sphere collider on a child object
                GameObject goCol = new GameObject("PickupArea");
                sphereCollider = goCol.AddComponent<SphereCollider>();
                sphereCollider.isTrigger = true;
                sphereCollider.radius = pickupRadius;
                goCol.transform.SetParent(transform);
                goCol.transform.localPosition = Vector3.zero;
                goCol.transform.localRotation = Quaternion.identity;

                isInitialised = true;
            }
        }


        #endregion

        #region Public CallBack Methods

        /// <summary>
        /// This is automatically called by SampleNavmeshFollowPlayer when the NPC gets sight of the player
        /// </summary>
        /// <param name="s3dSource"></param>
        /// <param name="s3dTarget"></param>
        public void GotLOS (StickyControlModule s3dSource, StickyControlModule s3dTarget)
        {
            if (isInitialised && s3dSource.StickyID == pickupCharacterStickyID)
            {
                if (stickyWeapon.IsHeldByNPC)
                {
                    stickyWeapon.SetFiringButton1(StickyWeapon.FiringButton.AutoFire);

                    stickyWeapon.AssignTargetCharacter(s3dTarget);
                }
            }
        }

        /// <summary>
        /// This is automatically called by SampleNavmeshFollowPlayer when the NPC looses sight of the player
        /// </summary>
        /// <param name="s3dSource"></param>
        /// <param name="s3dTarget"></param>
        public void LostLOS(StickyControlModule s3dSource, StickyControlModule s3dTarget)
        {
            if (isInitialised && s3dSource.StickyID == pickupCharacterStickyID)
            {
                stickyWeapon.UnAssignTargetCharacter();

                if (stickyWeapon.IsHeldByNPC && stickyWeapon.FiringButton1 == StickyWeapon.FiringButton.AutoFire)
                {
                    stickyWeapon.SetFiringButton1(StickyWeapon.FiringButton.Manual);
                }
            }
        }

        #endregion
    }
}