using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Sample to create a new Path in the scene at runtime and then teleport the ship and path.
    /// This works a bit like a floating point error manager.
    /// Setup:
    /// 1. Attach this to an empty gameobject in the scene
    /// 2. Add an NPC ship to the scene
    /// 3. Add a Ship AI Input Module to the NPC ship
    /// 4. Add the PlayerCamera prefab to the scene
    /// 5. Add the PlayerCamera to the shipCameraModule slot
    /// 6. Place other non-ship environment-type objects in a separate root-level gameobject
    /// 7. Add the environment gameobject to the slot in this component.
    /// 8. Play the scene
    /// This is only a sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own
    /// namespace.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/TelePort World")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleTelePortWorld : MonoBehaviour
    {
        #region Public Variables
        public ShipControlModule shipNPC = null;
        public ShipCameraModule shipCameraModule = null;
        public float moveOriginDistance = 200f;
        public GameObject worldEnvironment = null;
        #endregion

        #region Private Variables
        private ShipAIInputModule shipAIInputModule = null;
        private SSCManager sscManager = null;
        private PathData myPath = null;
        private bool isInitialised = false;
        //private Vector3 initialPosition = Vector3.zero;
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            sscManager = SSCManager.GetOrCreateManager(gameObject.scene.handle);
            if (shipCameraModule == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleTelePortWorld shipCameraModule is null. Did you forget to add it to the slot provided on this component?");
                #endif
            }
            else if (shipNPC == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleTelePortWorld shipNPC is null. Did you forget to add it to the slot provided on this component?");
                #endif
            }
            else if (shipNPC.GetShipAIInputModule(true) == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleTelePortWorld shipNPC does not seem to have a ShipAIInputModule component.");
                #endif
            }
            else if (sscManager != null)
            {
                shipCameraModule.SetTarget(shipNPC);

                #region Create the new path
                myPath = new PathData();

                sscManager.pathDataList.Add(myPath);

                bool autoGenerateLocationNames = false;

                #if UNITY_EDITOR
                myPath.name = "MyMovingPath";
                myPath.showLocationsInEditor = true;
                autoGenerateLocationNames = true;
                #endif

                sscManager.AppendLocation(myPath, new Vector3(100f,100f, 0f), autoGenerateLocationNames, false);
                sscManager.AppendLocation(myPath, new Vector3(300f,100f, 0f), autoGenerateLocationNames, false);
                sscManager.AppendLocation(myPath, new Vector3(600f,100f, 50f), autoGenerateLocationNames, false);
                sscManager.AppendLocation(myPath, new Vector3(1000f,200f, 50f), autoGenerateLocationNames, false);
                sscManager.AppendLocation(myPath, new Vector3(800f,200f, 300f), autoGenerateLocationNames, false);
                sscManager.AppendLocation(myPath, new Vector3(500f,150f, 500f), autoGenerateLocationNames, false);
                sscManager.AppendLocation(myPath, new Vector3(300f,150f, 300f), autoGenerateLocationNames, false);
                sscManager.AppendLocation(myPath, new Vector3(200f,150f, 350f), autoGenerateLocationNames, false);
                sscManager.AppendLocation(myPath, new Vector3(100f,100f, 200f), autoGenerateLocationNames, false);

                myPath.isClosedCircuit = true;

                sscManager.RefreshPathDistances(myPath);

                sscManager.InitialiseLocations(myPath, transform.position, Vector3.zero, Quaternion.identity, Vector3.zero, Vector3.zero);
                #endregion

                #region Assign Path to Ship

                // Check for an AI module
                shipAIInputModule = shipNPC.GetShipAIInputModule(true);
                if (shipAIInputModule != null)
                {
                    shipAIInputModule.Initialise();

                    //initialPosition = shipNPC.shipInstance.TransformPosition;

                    // Tell the AI ship to follow the path
                    shipAIInputModule.AssignTargetPath(myPath);
                    shipAIInputModule.SetState(AIState.moveToStateID);

                    isInitialised = true;
                }

                #endregion
            }
        }
        #endregion

        #region Update Methods

        // Update is called once per frame
        void Update()
        {
            if (isInitialised)
            {
                // Keep the ship near the origin (0,0,0)
                Vector3 _shipPosDelta = -shipNPC.shipInstance.TransformPosition;
                Vector3 _absPosDelta = SSCMath.Abs(_shipPosDelta);

                if ((_absPosDelta.x > _absPosDelta.z && _absPosDelta.x > moveOriginDistance) || (_absPosDelta.z > moveOriginDistance && _absPosDelta.z > _absPosDelta.x))
                {
                    // Teleport - ignore Y axis
                    TelePortWorld(new Vector3(_shipPosDelta.x, 0f, _shipPosDelta.z));
                }
            }
        }

        #endregion

        #region Private Methods

        private void TelePortWorld(Vector3 deltaTP)
        {
            if (isInitialised)
            {
                // Move Path. As the path is not part of a ShipDockingStation on large mothership, we can use
                // a simplified MoveLocations method.
                sscManager.MoveLocations(myPath, Time.deltaTime, deltaTP, Quaternion.identity);

                // Teleport the ShipAIInputModule rather than the ShipControlModule
                shipAIInputModule.TelePort(deltaTP, false);

                shipCameraModule.TelePort(deltaTP);

                sscManager.TelePortProjectiles(deltaTP);

                if (worldEnvironment != null) { worldEnvironment.transform.position += deltaTP; }
            }
        }

        #endregion
    }
}