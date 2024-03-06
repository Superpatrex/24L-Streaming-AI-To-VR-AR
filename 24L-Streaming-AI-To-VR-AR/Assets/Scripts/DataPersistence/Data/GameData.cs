using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CesiumForUnity;
using Unity.Mathematics;

[System.Serializable]
public class GameData 
{
    public double x;
    public double y;
    public double z;
    public List<double3> locations = new List<double3>();

    public GameData()
    {
        x = 86.92528;
        y = 27.98806; 
        z = 2208.3806427013874;

    }


}
