using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Demo script to show how to instruct an AI Ship through the use of colliders in the scene.
    /// Also shows how to create a custom behaviour.
    /// This is only sample to demonstrate how API calls could be used in your own code.
    /// </summary>
    public class DemoLocation : MonoBehaviour
    {
        #region Public variables
        public AIBehaviourInput.AIBehaviourType primaryBehaviourType = AIBehaviourInput.AIBehaviourType.SeekArrival;
        public DemoLocation nextLocation;
        public bool showLocationLabel = false;

        [Header("Optional Squadrons to allow")]
        public int[] squadronIdFilter;

        #endregion

        #region Private variables
        private Collider locCollider;
        private int numSquadronsToFilter = 0;
        #endregion

        #region Initialisation Methods

        // Use this for initialization
        void Awake()
        {
            locCollider = GetComponent<Collider>();

            if (locCollider == null || !(locCollider.GetType() == typeof(SphereCollider) || locCollider.GetType() == typeof(BoxCollider)))
            {
                #if UNITY_EDITOR
                throw new MissingComponentException("Missing Box or Sphere Collider on " + gameObject.name);
                #endif
            }
            else if(!locCollider.isTrigger)
            {
                #if UNITY_EDITOR
                throw new MissingComponentException(gameObject.name + " must be a trigger box or sphere collider");
                #endif
            }
            else
            {
                Initialise();
            }
        }
        #endregion

        #region Event Methods

        private void OnTriggerEnter(Collider other)
        {
            if (other != null && nextLocation != null)
            {
                // Is this an AI Ship?

                // Recurse upward in the hierarchy until we find the ship prefab parent
                ShipAIInputModule shipAIInputModule = other.GetComponentInParent<ShipAIInputModule>();

                if (shipAIInputModule != null)
                {
                    bool getNextLocation = numSquadronsToFilter == 0;

                    // Check if ships should only be included if in filter array
                    if (!getNextLocation)
                    {
                        int squadronId = shipAIInputModule.GetComponent<ShipControlModule>().shipInstance.squadronId;

                        for (int i = 0; i < numSquadronsToFilter; i++)
                        {
                            if (squadronId == squadronIdFilter[i]) { getNextLocation = true; break; }
                        }
                    }

                    if (getNextLocation)
                    {
                        // Tell the AI Ship where to go
                        shipAIInputModule.AssignTargetPosition(nextLocation.transform.position);
                        // Change the current AI behaviour type
                        DemoFlyToLocationShipData shipData = shipAIInputModule.GetComponent<DemoFlyToLocationShipData>();
                        shipData.currentBehaviourType = primaryBehaviourType;
                        // Add one or more behaviours to the ShipAIModule
                        //shipAIInputModule.ClearAssignedBehaviours();
                        //shipAIInputModule.aiBehaviourList.Add(new AIBehaviour(primaryBehaviourType, 1f));
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Must be called if the squardronIdFilter array is changed
        /// </summary>
        public void Initialise()
        {
            numSquadronsToFilter = squadronIdFilter == null ? 0 : squadronIdFilter.Length;

            if (showLocationLabel)
            {
                TextMesh textMesh = gameObject.GetComponent<TextMesh>();

                if (textMesh == null) { textMesh = gameObject.AddComponent<TextMesh>(); }

                if (textMesh != null)
                {
                    textMesh.text = gameObject.name;
                    textMesh.alignment = TextAlignment.Center;
                }
            }
        }

        #endregion
    }
}