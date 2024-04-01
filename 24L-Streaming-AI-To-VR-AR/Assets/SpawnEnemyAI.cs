using CesiumForUnity;
using SciFiShipController;
using scsmmedia;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public class SpawnEnemyAI : MonoBehaviour
{
    public double3 enemyShipsLocation;

    public CesiumGeoreference cesium;

    public GameObject enemyShip;
    private bool shipFlag = false;

    public GameObject player;

    public string pathName = "Path Name Here";

    private SSCManager sscManager;
    private PathData flightPath;
    private Vector3 newPosition;
    private List<GameObject> tempList = new List<GameObject>();


    private void Awake()
    {
        // Get a reference to the Ship Controller Manager instance
        sscManager = SSCManager.GetOrCreateManager(gameObject.scene.handle);
        // Find the Path
        flightPath = sscManager.GetPath(pathName);
        
    }

    private void FixedUpdate()
    {
        
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha0) && !shipFlag)
        {
            LocationData nextLocationData = null;
            PathLocationData tempPathLocationData = null;

            
            for (int i = 0; i < flightPath.pathLocationDataList.Count; i++)
            {
                tempPathLocationData = flightPath.pathLocationDataList[i];
                nextLocationData = tempPathLocationData.locationData;

                float xOffset = 0f;
                float zOffset = 0f;

                // Adjust position based on index
                switch (i)
                {
                    case 0:
                        xOffset = 400f;
                        break;
                    case 1:
                        zOffset = 400f;
                        break;
                    case 2:
                        xOffset = -400f;
                        break;
                    default:
                        zOffset = -400f;
                        break;
                }

                // Create new position
                newPosition = new Vector3(player.transform.position.x + xOffset, player.transform.position.y, player.transform.position.z + zOffset);
                tempList.Add(Instantiate(enemyShip, newPosition, Quaternion.identity));

                // Update location
                sscManager.UpdateLocation(nextLocationData, newPosition, false);
            }

            sscManager.RefreshPathDistances(flightPath);
            shipFlag = true;

        }
        else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha0) && shipFlag) 
        {

            foreach (GameObject enemyAIShips in tempList) 
            { 
                Destroy(enemyAIShips);
            }

            tempList.Clear();

            shipFlag = false;
        }

      

    }


}
