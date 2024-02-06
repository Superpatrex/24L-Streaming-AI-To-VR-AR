using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple script to have a Sticky3D character fire at another character using a ray.
    /// This is a very simple example to show the basic technique.
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// 1. Add a S3D player prefab character (e.g. PlayerBob) to your scene
    /// 2. Add at least one S3D NPC (e.g. NPC_Bob) to your scene
    /// 3. Ensure all characters have Initialise on Awake enabled on the Move tab
    /// 4. Attach this script to the S3D player
    /// 5. On the Player Input Module, add a Custom Input
    /// 6. Configure a Fire button on the Custom Input e.g. Mouse0 if InputMode is DirectKeyboard
    /// 7. Add a new Callback method to the Custom Input and drag the player parent gameobject into the slot
    /// 8. For the Function, select SampleFireAtCharacter, Fire1().
    /// 9. In the scene, expand PlayerBob and under s3d_bob, drag hand.R into the weaponLocation slot on this script
    /// 10. Test the scene
    /// 11. Change the Custom Input in the inspector to use Fire2() and then test the scene.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Fire at Character")]
    public class SampleFireAtCharacter : MonoBehaviour
    {
        #region Public Variables
        [Tooltip("This is the weapon location on the player - like a hand")]
        public Transform weaponLocation = null;
        [Tooltip("How far the weapon can shoot in a single frame")]
        public float weaponRange = 100f;
        [Tooltip("How much damage do we cause the other character when we hit it")]
        [Range(0.10f, 100f)] public float damage = 10f;
        [Tooltip("The NPC you are targeting")]
        public StickyControlModule targetNPC;

        #endregion

        #region Private Variables
        private StickyControlModule player = null;
        private bool isInitialised = false;
        private bool isZeroHealthNotified = false;
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            player = GetComponent<StickyControlModule>();

            if (player == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: Cannot find the Sticky3D Controller component for this player. This component should be attached to a Sticky3d Controller.");
                #endif
            }
            else if (weaponLocation == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: Please add the transform on the Player where the weapon will fire from. e.g. the right hand of PlayerBob");
                #endif
            }
            else if (!weaponLocation.IsChildOf(player.transform))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: The weapon location must be a child transform of the Player e.g. the right hand of PlayerBob");
                #endif
            }
            else if (targetNPC == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: Please add a S3D NPC prefab (e.g. NPC_Bob) to the scene and drag into the slot on the SampleFireAtCharacter component.");
                #endif
            }
            else if (!targetNPC.isTriggerColliderEnabled)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: The Trigger Collider setting on the Collide tab must be ENABLED for Sticky3D characters so that they can detect raycast hits.");
                #endif
            }
            else
            {
                isInitialised = true;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// In this example we attempt to hit a particular target character
        /// </summary>
        public void Fire1()
        {
            // Only fire if we haven't reduced the target health to 0
            if (isInitialised && !isZeroHealthNotified)
            {
                #if UNITY_EDITOR
                Debug.Log("FIRE! T:" + Time.time);
                #endif

                RaycastHit raycastHit = new RaycastHit();

                if (targetNPC.IsHit(weaponLocation.position, player.transform.forward, weaponRange, ref raycastHit, true))
                {
                    targetNPC.Health -= damage;

                    float health = targetNPC.Health;
                    #if UNITY_EDITOR
                    Debug.Log("Hit " + raycastHit.transform.name + " at " + raycastHit.point + " ! Health: " + health + " T:" + Time.time);
                    #endif

                    if (health <= 0f)
                    {
                        isZeroHealthNotified = true;
                        #if UNITY_EDITOR
                        Debug.Log(raycastHit.transform.name + " has run out of health! T:" + Time.time);
                        #endif
                    }
                }
            }
        }

        /// <summary>
        /// In this example we try to hit anything in front of the character and in range.
        /// </summary>
        public void Fire2()
        {
            if (isInitialised)
            {
                RaycastHit raycastHit = new RaycastHit();

                // Did we hit anything?
                if (player.IsHitOther(weaponLocation.position, player.transform.forward, weaponRange, ref raycastHit, true))
                {
                    // Was it a S3D character?
                    StickyControlModule otherCharacter = raycastHit.transform.GetComponent<StickyControlModule>();
                    if (otherCharacter != null)
                    {
                        otherCharacter.Health -= damage;

                        // Could also apply force to the rigidbody...
                        raycastHit.rigidbody.AddForce(-raycastHit.normal * 100f);

                        #if UNITY_EDITOR
                        float health = otherCharacter.Health;
                        Debug.Log("Hit " + raycastHit.transform.name + " at " + raycastHit.point + " ! Health: " + health + " T:" + Time.time);
                        #endif
                    }
                    else
                    {
                        #if UNITY_EDITOR
                        Debug.Log("Hit " + raycastHit.transform.name + " at " + raycastHit.point + " T:" + Time.time);
                        #endif
                    }
                }
            }
        }

        #endregion
    }
}