using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

/// <summary>
/// The XMLShipStructure class is used to store the XML data of the ship
/// </summary>
public class XMLShipStructure
{
    [XmlElement("Aircraft")]
    public Aircraft craft;
}

/// <summary>
/// The Aircraft class is used to store the XML data of the aircraft, a subclass of the XMLShipStructure
/// </summary>
public class Aircraft
{
    public string name;
    public string type;
    [XmlElement("Location")]
    public Location aircraftLocation;
    [XmlElement("Fuel")]
    public Fuel fuel;
    public Weapons weapons;
}

/// <summary>
/// The Location class is used to store the XML data of the location of the aircraft, a subclass of the Aircraft class
/// </summary>
public class Location
{
    // Aircraft location should be lat 21.482216 and long -158.039959 for the start aircraft which is Wheeler Army Airfield
    public float latitude;
    public float longitude;
    public float altitude;
    public EnemyAircraft enemyAircraft;
}

public class EnemyAircraft
{
    public bool areEnemies;
}

/// <summary>
/// The Fuel class is used to store the XML data of the fuel of the aircraft, a subclass of the Aircraft class
/// </summary>
public class Fuel
{
    public float fuelLevel;
    public float fuelCapacity;
    public float fuelConsumptionRate;
}

public class Weapons
{
    public IndividualWeapon leftMissle;
    public IndividualWeapon rightMissle;
    public IndividualWeapon leftGun;
    public IndividualWeapon rightGun;
}

public class IndividualWeapon
{
    public string name;
    public int roundsLeft; // Corrected field name
    public float timeBetweenFiring;
    
    // Weapon state should be "ready" or "reloading" or "jammed"
    public string weaponState;
    public float inaccuracy;
    public int weaponType;
}

