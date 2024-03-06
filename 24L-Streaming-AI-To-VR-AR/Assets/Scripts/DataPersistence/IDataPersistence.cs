using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CesiumForUnity;
using Unity.Mathematics;

public interface IDataPersistence
{
    void LoadData(GameData data);
    void SaveData(ref GameData data);


}
