using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowControls : MonoBehaviour
{
    public GameObject controls;
    public GameObject menu;

    public static ShowControls Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Controls()
    {
        // Hide menu, show controls
        controls.gameObject.SetActive(true);
        menu.SetActive(false);
    }

    public void Back()
    {
        // Show menu, hide controls
        menu.gameObject.SetActive(true);
        controls.SetActive(false);
    }

    public void BackToStart()
    {
        Destroy(ShowControls.Instance.gameObject);
        Destroy(SpawnEnemyAI.Instance.gameObject);
        Loader.Load(Loader.Scene.StartMenu);
    }
}
