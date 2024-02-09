using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XMLShipStructure
{
    Aircraft craft;

}

class Aircraft
{
    string name;
    string type;
    Location aircraftLocation;
}

class Location
{
    float latitude;
    float longitude;
    float altitude;
}

class Fuel
{
    float fuelLevel;
    float fuelCapacity;
    float fuelConsumptionRate;
}
