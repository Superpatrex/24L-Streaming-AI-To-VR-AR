using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

public class XMLShipStructure
{
    [XmlElement("Aircraft")]
    public Aircraft craft;
}

public class Aircraft
{
    public string name;
    public string type;
    [XmlElement("Location")]
    public Location aircraftLocation;
    [XmlElement("Fuel")]
    public Fuel fuel;
}

public class Location
{
    public float latitude;
    public float longitude;
    public float altitude;
}

public class Fuel
{
    public float fuelLevel;
    public float fuelCapacity;
    public float fuelConsumptionRate;
}
