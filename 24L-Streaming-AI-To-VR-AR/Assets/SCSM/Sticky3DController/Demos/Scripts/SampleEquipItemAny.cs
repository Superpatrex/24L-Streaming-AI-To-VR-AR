using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Sample script to equip an interactive object to a character when it is near the object.
    /// SETUP:
    /// 1. Add this component to a configured StickyInteractive prefab in the scene.
    /// 2. Add an Sticky3D character in the scene
    /// 3. Place the interactive object near the character
    /// 4. Ensure the character has at least 1 Equip Point configured on the Engage tab
    /// 5. On this component, tick Initialise On Start
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Equip Item Any")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class SampleEquipItemAny : MonoBehaviour
    {
        #region Public Variables

        [Tooltip("If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the component is enabled through code.")]
        public bool initialiseOnStart = false;

        [Tooltip("If a character gets within this radius of the interactive object, it will attempt to equip to the first available Equip Point")]
        [Range(0.1f, 5f)] public float pickupRadius = 2f;

        [Tooltip("Only NPCs can pickup and equip this item")]
        public bool onlyNPC = false;

        [Tooltip("When 0, the item will be equipped to the first available Equip point. When > 0 will attempt to use the Equip Point indicated")]
        [Range(0, 10)] public int defaultEquipPoint = 0;

        #endregion

        #region Private Variables - General

        private bool isInitialised = false;
        private StickyInteractive stickyInteractive = null;
        private SphereCollider sphereCollider = null;
        private bool isPickedUp = false;
        #endregion

        #region Private Initialise Methods

        // Use this for initialization
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Events

        /// <summary>
        /// This gets automatically called when a collider enters the sphere collider
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (isInitialised && !isPickedUp)
            {
                Rigidbody rBody = other.attachedRigidbody;

                if (rBody != null)
                {
                    StickyControlModule pickupCharacter = null;

                    // Is this an S3D that has entered the pickup zone for this interactive object?
                    // Check if it must be an NPC or not
                    if (rBody.TryGetComponent(out pickupCharacter) && pickupCharacter.IsInitialised && (!onlyNPC || pickupCharacter.IsNPC()))
                    {
                        S3DEquipPoint equipPoint = pickupCharacter.GetFirstAvailableEquipPoint();

                        if (equipPoint == null)
                        {
                            Debug.Log(pickupCharacter.name + " could not equip " + name + " as there were no available Equip Points");
                        }
                        else
                        {
                            if (pickupCharacter.EquipItem(stickyInteractive, equipPoint) != S3DStoreItem.NoStoreItem)
                            {
                                Debug.Log(pickupCharacter.name + " equipped " + name + " on " + equipPoint.equipPointName);
                                isPickedUp = true;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Public API Methods - General

        public void Initialise()
        {
            if (isInitialised) { return; }

            // Make sure this component looks like it is configured correctly
            if (!gameObject.TryGetComponent(out stickyInteractive))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleEquipItemAny - did you forget to attach this script to your StickyInteractive prefab in the scene?");
                #endif
            }
            else
            {
                if (!stickyInteractive.IsInitialised) { stickyInteractive.Initialise(); }

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
    }
}