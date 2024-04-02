using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowControls : MonoBehaviour
{
    public GameObject controls;
    public GameObject menu;
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
        controls.SetActive(false);
        menu.gameObject.SetActive(true);
    }
}
