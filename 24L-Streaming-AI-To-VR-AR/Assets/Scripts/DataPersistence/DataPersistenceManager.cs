using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CesiumForUnity;
using Unity.Mathematics;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]

    [SerializeField] private string fileName; 

    public static DataPersistenceManager instance { get; private set; }

    private GameData gameData;

    private List<IDataPersistence> dataPersistenceObjects;

    private FileDataHandler dataHandler;

    public CesiumGeoreference test;

    

    private void Awake()
    {
        if (instance != null) 
        {
            Debug.Log("Found more than one Data Persistence Manager in the scene.");
        }

        instance = this;
    }

    private void Start()
    {
        // Debug.Log(Application.persistentDataPath);
        // C:/Users/tayle/AppData/LocalLow/DefaultCompany/24L-Streaming-AI-To-VR-AR

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);

        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void NewGame() {
        this.gameData = new GameData();
    }

    public void LoadGame() {


        this.gameData = dataHandler.Load();


        if (this.gameData == null)
        {
            Debug.Log("No data was found. Initializing data to defaults.");
            NewGame();
        }

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);

        }


        test.SetOriginLongitudeLatitudeHeight(this.gameData.x, this.gameData.y, this.gameData.z);
       
    }

    public void SaveGame()
    {

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref gameData);

        }

       

        dataHandler.Save(gameData);
    }

    private void OnDestroy()
    {
        SaveGame();
    }

   /* public void ReloadScene()
    {
        SaveGame(); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    } */



    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }

}
