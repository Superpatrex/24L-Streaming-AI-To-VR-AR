using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Sample to create a new Path in the scene at runtime.
    /// Setup:
    /// 1. Attach this to an empty gameobject in the scene
    /// 2. Add an NPC ship to the scene
    /// 3. Add a Ship AI Input Module to the NPC ship
    /// 4. Add the PlayerCamera prefab to the scene
    /// 5. Add the NPC ship to the PlayerCamera "TargetShip" slot
    /// 6. Play the scene
    /// This is only a sample to demonstrate how API calls could be used in
    /// your own code. You should write your own version of this in your own
    /// namespace.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Create Path")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleCreatePath : MonoBehaviour
    {
        #region Public Variables
        public ShipControlModule shipNPC = null;
        #endregion

        #region Private Variables
        private SSCManager sscManager = null;
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            
            sscManager = SSCManager.GetOrCreateManager(gameObject.scene.handle);
            if (sscManager != null)
            {
                #region Create the new path
                PathData newPath = new PathData();

                sscManager.pathDataList.Add(newPath);

                bool autoGenerateLocationNames = false;

                #if UNITY_EDITOR
                newPath.name = "MyPath";
                newPath.showLocationsInEditor = true;
                autoGenerateLocationNames = true;
                #endif

                sscManager.AppendLocation(newPath, new Vector3(100f,100f, 0f), autoGenerateLocationNames, false);
                sscManager.AppendLocation(newPath, new Vector3(200f,100f, 0f), autoGenerateLocationNames, false);
                sscManager.AppendLocation(newPath, new Vector3(300f,100f, 50f), autoGenerateLocationNames, false);

                sscManager.RefreshPathDistances(newPath);
                #endregion

                #region Assign Path to Ship
                if (shipNPC != null)
                {
                    // Check for an AI module
                    ShipAIInputModule shipAIInputModule = shipNPC.GetShipAIInputModule(true);
                    if (shipAIInputModule != null)
                    {
                        shipAIInputModule.Initialise();

                        // Tell the AI ship to follow the path
                        shipAIInputModule.AssignTargetPath(newPath);
                        shipAIInputModule.SetState(AIState.moveToStateID);
                    }
                }

                #endregion
            }
        }

        #endregion
    }
}