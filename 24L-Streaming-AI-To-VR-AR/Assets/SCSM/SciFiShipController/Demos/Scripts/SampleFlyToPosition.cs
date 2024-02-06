using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// This sample shows how you can:
    /// 1. Fly an AI ship to a given position in 3D space.
    /// Setup:
    /// 1. Add an AI ship to the scene. E.g. Demos\TechDemo\Prefabs\Ships\SSCInterceptor NPC (Arcade)
    /// 2. If they ship doesn't have an Ship AI Input Module attached, add it now
    /// 3. On the Ship AI Input Module component, ensure "Initialise On Awake" is enabled.
    /// 4. Create an empty gameobject in the scene. Rename it "Controller"
    /// 5. Add this script to the Controller gameobject
    /// 6. Add a cube somewhere in the scene and rename it "Destination"
    /// 7. Turn off the box collider on the cube
    /// 8. Drag the Ship into the slot on this component
    /// 9. Drag the Destination gameobject into the slot on this component.
    /// This is only sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own
    /// namespace.
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Fly To Position")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleFlyToPosition : MonoBehaviour
    {
        #region Public variables
        public ShipAIInputModule aiShip = null;
        public GameObject destination = null;
        #endregion

        #region Initialisation Methods

        // Start is called before the first frame update
        void Start()
        {
            if (aiShip != null)
            {
                if (destination != null)
                {
                    if (aiShip.IsInitialised)
                    {
                        // Get notified when we reach the destination
                        aiShip.callbackCompletedStateAction = ArrivedNotification;

                        // The ship starts in the Idle state.
                        // Tell the ship where it should fly to (see the manual Runtime and API section for many other options)
                        aiShip.AssignTargetPosition(destination.transform.position);

                        aiShip.SetState(AIState.moveToStateID);
                    }
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("SampleFlyToPosition - cannot find destination gameobject. Did you add drag it into this component?"); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("SampleFlyToPosition - cannot find aiShip. Did you add drag it into this component?"); }
            #endif
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This gets automatically called by Sci-Fi Ship Controller
        /// </summary>
        /// <param name="shipAIInputModule"></param>
        public void ArrivedNotification(ShipAIInputModule shipAIInputModule)
        {
            Debug.Log("You have arrived at your destination");

            // If you comment out this line the ship will fly round in circles
            shipAIInputModule.SetState(AIState.idleStateID);

            // NOTE: If you want the ship to slow down as it approaches the destination you
            // can write a custom arrival behaviour. See DemoFlyToLocation.cs for an example.
        }

        #endregion
    }
}