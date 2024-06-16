using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;
using Utility;
using CesiumForUnity;
using SciFiShipController;

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
    [SerializeField] public ShipControlModule shipControl;
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

            List<Weapon> weapons = shipControl.GetWeapons();

            foreach (Weapon w in weapons)
            {
                if (shipInformation == null)
                {
                    Debug.Log("Ship Information is null");
                }
                else if (shipInformation.craft == null)
                {
                    Debug.Log("Craft is null");
                }
                else if (shipInformation.craft.weapons == null)
                {
                    Debug.Log("Weapons is null");
                }
                else if (shipInformation.craft.weapons.leftGun == null)
                {
                    Debug.Log("Left Gun is null");
                }
                else if (shipInformation.craft.weapons.rightGun == null)
                {
                    Debug.Log("Right Gun is null");
                }
                else if (shipInformation.craft.weapons.leftMissle == null)
                {
                    Debug.Log("Left Missile is null");
                }
                else if (shipInformation.craft.weapons.rightMissle == null)
                {
                    Debug.Log("Right Missile is null");
                }

                if (w.name == "Left Gun")
                {
                    shipInformation.craft.weapons.leftGun.roundsLeft = w.ammunition;
                    //Debug.Log("Left Gun: " + w.ammunition);
                }
                else if (w.name == "Right Gun")
                {
                    shipInformation.craft.weapons.rightGun.roundsLeft = w.ammunition;
                    //Debug.Log("Right Gun: " + w.ammunition);
                }
                else if (w.name == "Left Missile")
                {
                    shipInformation.craft.weapons.leftMissle.roundsLeft = w.ammunition;
                    //Debug.Log("Left Missile: " + w.ammunition);
                }
                else if (w.name == "Right Missile")
                {
                    shipInformation.craft.weapons.rightMissle.roundsLeft = w.ammunition;
                    //Debug.Log("Right Missile: " + w.ammunition);
                }
            }

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
        /*
        shipInformation.craft.aircraftLocation.latitude = (float)georeference.latitude;
        shipInformation.craft.aircraftLocation.longitude = (float)georeference.longitude;
        shipInformation.craft.aircraftLocation.altitude = (float)georeference.height;
        holder.shipInformation.text = XMLSerializer.WriteToXmlStringShipInformation(shipInformation);
        */
        this.timeSinceLastUpdate = this.timePerUpdate;
        Update();
        //Debug.Log("Updated on save");
    }


}