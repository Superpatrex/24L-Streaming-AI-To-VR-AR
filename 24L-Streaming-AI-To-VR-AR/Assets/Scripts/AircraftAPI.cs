using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;
using Utility;
using CesiumForUnity;

public class AircraftAPI : MonoBehaviour
{
    // Public fields
    public static string ReturnJsonString
    {
        get => returnJsonString;
        set => returnJsonString = value;
    }

    [SerializeField] public CesiumGeoreference georeference;
    
    [SerializeField] public float timeSinceLastUpdate = 0.0f;

    [SerializeField] public float timePerUpdate = 10.0f;

    [SerializeField] public XMLHolder holder;
    private XMLShipStructure shipInformation;

    // Private fields
    private static string returnJsonString;

    /// <summary>
    /// On awake of the script, call the API to get the data, and read the XML file
    /// </summary>
    public void Awake()
    {
        shipInformation = XMLSerializer.ReadFromXmlStringShipInformation(holder.shipInformation.text.Replace("\n", "").Replace("\\", ""));
    }

    /// <summary>
    /// Update is called once per frame, essentially it calls the API every timePerUpdate in seconds, every timePerUpdate, the XML file is updated
    /// </summary>
    public void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= timePerUpdate)
        {
            // Begin the process of getting the data from the API
            shipInformation.craft.aircraftLocation.latitude = (float)georeference.latitude;
            shipInformation.craft.aircraftLocation.longitude = (float)georeference.longitude;
            shipInformation.craft.aircraftLocation.altitude = (float)georeference.height;
            timeSinceLastUpdate = 0.0f;
            holder.shipInformation.text = XMLSerializer.WriteToXmlStringShipInformation(shipInformation);
            //Debug.Log("Updated");
        }
    }

    /// <summary>
    /// On application quit, update the XML file with the current location of the aircraft
    /// </summary>
    public void OnApplicationQuit()
    {
        shipInformation.craft.aircraftLocation.latitude = (float)georeference.latitude;
        shipInformation.craft.aircraftLocation.longitude = (float)georeference.longitude;
        shipInformation.craft.aircraftLocation.altitude = (float)georeference.height;
        holder.shipInformation.text = XMLSerializer.WriteToXmlStringShipInformation(shipInformation);
        //Debug.Log("Updated on quit");
    } 

    public void SaveAircraft()
    {
        shipInformation.craft.aircraftLocation.latitude = (float)georeference.latitude;
        shipInformation.craft.aircraftLocation.longitude = (float)georeference.longitude;
        shipInformation.craft.aircraftLocation.altitude = (float)georeference.height;
        holder.shipInformation.text = XMLSerializer.WriteToXmlStringShipInformation(shipInformation);
        //Debug.Log("Updated on save");
    }


}