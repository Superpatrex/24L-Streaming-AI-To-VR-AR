using CesiumForUnity;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewLocation : MonoBehaviour, IDataPersistence
{


    // public CesiumGeoreference spawn;
    public double updateX;
    public double updateY;
    public double updateZ;

   

   

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            Debug.Log("7 was pressed!");
            UpdateNewCoor(this.updateX, this.updateY, this.updateZ);
            // spawn.SetOriginLongitudeLatitudeHeight(this.newCoordinates.x, this.newCoordinates.y, this.newCoordinates.z);
            // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            Loader.Load(Loader.Scene.CesiumEnvironment);
        }
    }

    private void UpdateNewCoor(double x, double y, double z) 
    {
        this.updateX = x; 
        this.updateY = y; 
        this.updateZ = z;
    }


    public void LoadData(GameData data)
    {
        
        this.updateX = data.x; 
        this.updateY = data.y; 
        this.updateZ = data.z;
    }

    public void SaveData(ref GameData data)
    {
        
        data.x = this.updateX; 
        data.y = this.updateY; 
        data.z = this.updateZ;
    }


}
