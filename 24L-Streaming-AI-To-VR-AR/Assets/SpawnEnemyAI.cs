using CesiumForUnity;
using SciFiShipController;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public class SpawnEnemyAI : MonoBehaviour
{
    public double3 enemyShipsLocation;

    public CesiumGeoreference cesium;

    public GameObject enemyShip1;
    public GameObject shipHolder;

    public GameObject player;

    private void Update()
    {
        
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha9) && shipHolder.activeInHierarchy == false)
        {

             // Set the Ships Parent Object to true to activate enemy ships
             shipHolder.SetActive(true);

             // Change the cesium environment to the Grand Canyon (designated training field with enemy AI ships)
             cesium.SetOriginLongitudeLatitudeHeight(enemyShipsLocation.x, enemyShipsLocation.y, enemyShipsLocation.z);


            // Change the players location behind one of the enemy AI ships
            // This allows the player to see 1/4 enemies to target
            // playerInputModule.ResetInput();
            Vector3 enemyPosition = enemyShip1.transform.position - enemyShip1.transform.forward * 35f;

             player.transform.position = enemyPosition;


            Debug.Log("Player: " + player.transform.position);
            Debug.Log("Enemy: " + enemyShip1.transform.position);
            
        }
        else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha9) && shipHolder.activeInHierarchy == true) { 
            shipHolder.SetActive(false);
        }

    }

}
