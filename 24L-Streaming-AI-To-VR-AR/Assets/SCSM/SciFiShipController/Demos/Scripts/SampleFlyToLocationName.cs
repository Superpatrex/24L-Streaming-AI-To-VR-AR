using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This sample shows how you can:
    /// 1. Fly a ship to a Location created in the SSCManager
    /// To setup:
    /// 1. GameObject->3D Object->SciFi Ship Controller->SSCManager
    /// 2. On the Location tab, add a Location to the scene and give it a name
    /// 3. Add a NPC prefab ship to the scene
    /// 4. Add this component to the NPC ship
    /// 5. Add the Location name from SSCManager to this component in the scene
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Fly To Location Name")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleFlyToLocationName : MonoBehaviour
    {
        #region Public Variables

        [Tooltip("Create a new location in SSCManager in the scene and add name here")]
        public string locationName = "Location Name from SSCManager";
        #endregion

        #region Private Variables - General
        private ShipAIInputModule shipAIInputModule = null;
        private SSCManager sscManager = null;
        private LocationData locationData = null;
        #endregion

        #region Private Initialise Methods

        void Start()
        {
            if (!TryGetComponent(out shipAIInputModule))
            {
                #if UNITY_EDITOR
                Debug.LogWarning("SampleFlyToLocationName - did you forget to add this to a ship with a ShipAIInputModule?");
                #endif
            }
            else
            {
                sscManager = SSCManager.GetOrCreateManager(gameObject.scene.handle);
                shipAIInputModule.Initialise();

                locationData = sscManager.GetLocation(locationName);

                if (locationData == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("SampleFlyToLocationName - could not find the location [" + locationName + "] from the SSCManager Location tab.");
                    #endif
                }
                else
                {
                    // Get notified when we reach the destination
                    shipAIInputModule.callbackCompletedStateAction = ArrivedNotification;

                    // The ship starts in the Idle state.
                    // Tell the ship where it should fly to (see the manual Runtime and API section for many other options)
                    shipAIInputModule.AssignTargetLocation(locationData);

                    shipAIInputModule.SetState(AIState.moveToStateID);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This gets automatically called by Sci-Fi Ship Controller
        /// </summary>
        /// <param name="shipAIInputModule"></param>
        public void ArrivedNotification(ShipAIInputModule shipAIInputModule)
        {
            Debug.Log("You have arrived at " + locationName);

            // If you comment out this line the ship will fly round in circles
            shipAIInputModule.SetState(AIState.idleStateID);

            // NOTE: If you want the ship to slow down as it approaches the destination you
            // can write a custom arrival behaviour. See DemoFlyToLocation.cs for an example.
        }

        #endregion
    }
}