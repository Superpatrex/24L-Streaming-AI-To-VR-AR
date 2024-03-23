using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Utility;
using OpenAI;
using System.Threading.Tasks;
using System;
using UnityEngine.Networking;

public class MyTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void MyTestsSimplePasses()
    {
        // Assert.AreEqual
        int actualInt = 42;
        Assert.AreEqual(42, actualInt, "Values are not equal");

        // Assert.IsTrue / Assert.IsFalse
        bool trueCondition = true;
        Assert.IsTrue(trueCondition, "Condition should be true");

        bool falseCondition = false;
        Assert.IsFalse(falseCondition, "Condition should be false");

        // Assert.IsNull / Assert.IsNotNull
        object nullObject = null;
        Assert.IsNull(nullObject, "Object should be null");

        object notNullObject = new object();
        Assert.IsNotNull(notNullObject, "Object should not be null");

        // Assert.AreSame / Assert.AreNotSame
        string referenceString = "Hello";
        string sameReferenceString = referenceString;
        Assert.AreSame(referenceString, sameReferenceString, "Objects should be the same reference");

        string differentReferenceString = "World";
        Assert.AreNotSame(referenceString, differentReferenceString, "Objects should be different references");
    }

    [TestCase(28.3765, 'N', 81.5494, 'W', "Epcot", 28.3765, -81.5494)]
    [TestCase(28.3765, 'N', 81.5494, 'W', "Walt Disney World Epcot Park", 28.3765, -81.5494)]
    [TestCase(30.5595, 'S', 22.9375, 'E', "Cape Town", -30.5595, 22.9375)]
    [TestCase(30.5595, 'S', 22.9375, 'E', "Cape Town South Africa", -30.5595, 22.9375)]
    public void LatLongLocationIndividualPass(double lat, char latDirection, double lon, char longDirection, string locationName, double expectedLat, double expectedLong)
    {
        // Arrange
        LatLongLocation latLongLocation = new LatLongLocation(lat, latDirection, lon, longDirection, locationName);

        // Act
        double actualLat = latLongLocation.Lat;
        double actualLong = latLongLocation.Long;
        string actualLocationName = latLongLocation.LocationName;

        // Assert
        Assert.AreEqual(expectedLat, actualLat, "Latitude values are not equal");
        Assert.AreEqual(expectedLong, actualLong, "Longitude values are not equal");
        Assert.AreEqual(locationName, actualLocationName, "Location names are not equal");
    }

    [TestCase("28.3765 N, 81.5494 W Epcot", 28.3765, -81.5494, "Epcot")]
    [TestCase("28.3765 N, 81.5494 W Walt Disney World Epcot Park", 28.3765, -81.5494, "Walt Disney World Epcot Park")]
    [TestCase("30.5595 S, 22.9375 E Cape Town", -30.5595, 22.9375, "Cape Town")]
    [TestCase("30.5595 S, 22.9375 E Cape Town South Africa", -30.5595, 22.9375, "Cape Town South Africa")]
    public void LatLongLocationStringPass(string latlongString, double lat, double lon, string locationName)
    {
        // Arrange
        LatLongLocation latLongLocation = new LatLongLocation(latlongString);

        // Act
        double actualLat = latLongLocation.Lat;
        double actualLong = latLongLocation.Long;
        string actualLocationName = latLongLocation.LocationName;

        // Assert
        Assert.AreEqual(lat, actualLat, "Latitude values are not equal");
        Assert.AreEqual(lon, actualLong, "Longitude values are not equal");
        Assert.AreEqual(locationName, actualLocationName, "Location names are not equal");
    }

    [TestCase("28.3765 N, 81.5494 W Epcot")]
    [TestCase("28.3765 N, 81.5494 W Walt Disney World Epcot Park")]
    [TestCase("30.5595 S, 22.9375 E Cape Town")]
    [TestCase("30.5595 S, 22.9375 E Cape Town South Africa")]
    public void LatLongLocationToStringPass(string latLongString)
    {
        // Create LatLongLocation
        LatLongLocation latLongLocation = new LatLongLocation(latLongString);

        // Ensure that when we give it a string to represent we are able to get that string back
        Assert.AreEqual(latLongLocation.ToString(), latLongString);
    }

    [TestCase(null)]
    [TestCase(" ")]
    [TestCase("               ")]
    [TestCase("      Epcot    ")]
    [TestCase("Epcot    ")]
    [TestCase("LMAO THIS ISN'T GONNA WORK")]
    [TestCase("a")]
    [TestCase("N, W")]
    [TestCase("E E E E E E E E")]
    [TestCase("28.3765 N, 81.5494 W")]
    [TestCase("28.3765 N, 81.5494 W ")]
    [TestCase("28.3765 Nay, 81.5494 W")]
    [TestCase("28.3765 N, 81.5494 Wwow")]
    [TestCase("28.3765 Nnay, 81.5494 Wwow")]
    [TestCase("28.1 N, W")]
    [TestCase("N, 28.1 W")]
    [TestCase("N, W")]
    public void LatLongLocationStringFail(string latLonString)
    {
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(latLonString));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(latLonString));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(latLonString));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(latLonString));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(latLonString));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(latLonString));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(latLonString));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(latLonString));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(latLonString));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(latLonString));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(latLonString));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(latLonString));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(latLonString));
    }

    [TestCase(28.3765, 'N', 81.5494, 'W', null)]
    [TestCase(28.3765, 'N', 81.5494, 'W', "")]
    [TestCase(28.3765, 'N', 81.5494, 'W', " ")]
    [TestCase(28.3765, 'N', 81.5494, 'W', "                   ")]
    [TestCase(28.3765, 'N', 81.5494, 'X', "Epcot")]
    [TestCase(28.3765, 'X', 81.5494, 'E', "Epcot")]
    [TestCase(28.3765, 'X', 81.5494, 'X', "Epcot")]
    [TestCase(-28.3765, 'N', 81.5494, 'E', "Epcot")]
    [TestCase(28.3765, 'N', -81.5494, 'E', "Epcot")]
    [TestCase(28.3765, 'N', 500.5494, 'E', "Epcot")]
    [TestCase(500.3765, 'N', 81.5494, 'E', "Epcot")]
    [TestCase(500.3765, 'N', 500.5494, 'E', "Epcot")]
    public void LatLongLocationIndivFail(double lat, char latDirection, double lon, char longDirection, string locationName)
    {
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(lat, latDirection, lon, longDirection, locationName));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(lat, latDirection, lon, longDirection, locationName));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(lat, latDirection, lon, longDirection, locationName));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(lat, latDirection, lon, longDirection, locationName));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(lat, latDirection, lon, longDirection, locationName));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(lat, latDirection, lon, longDirection, locationName));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(lat, latDirection, lon, longDirection, locationName));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(lat, latDirection, lon, longDirection, locationName));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(lat, latDirection, lon, longDirection, locationName));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(lat, latDirection, lon, longDirection, locationName));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(lat, latDirection, lon, longDirection, locationName));
        Assert.Throws<System.ArgumentException>(() => new LatLongLocation(lat, latDirection, lon, longDirection, locationName));
    }

    [UnityTest]
    public IEnumerator OpenWeatherAPIStringPass()
    {
        IEnumerator enumerator = WeatherAPI.GetApiData("28.3765", "81.5494");

        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }

        Assert.IsNotNull(WeatherAPI.ReturnJsonString);
        Assert.IsTrue(!String.IsNullOrEmpty(WeatherAPI.ReturnJsonString));
    }

    [UnityTest]
    public IEnumerator OpenWeatherAPIPass()
    {
        LatLongLocation item = new LatLongLocation("28.3765 N, 81.5494 W Epcot");
        
        // Start the API request
        IEnumerator enumerator = WeatherAPI.GetApiData(item);

        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }

        Assert.IsNotNull(WeatherAPI.ReturnJsonString);
        Assert.IsTrue(!String.IsNullOrEmpty(WeatherAPI.ReturnJsonString));
    }

    [Test]
    public void ReadFromXmlStringWeatherPass()
    {
        string xmlStringWeather = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><current><city id=\"1283119\" name=\"Lobujya\"><coord lon=\"86.9253\" lat=\"27.9881\"></coord><country>NP</country><timezone>28800</timezone><sun rise=\"2024-02-20T00:46:36\" set=\"2024-02-20T12:05:58\"></sun></city><temperature value=\"-41.26\" min=\"-41.26\" max=\"-41.26\" unit=\"fahrenheit\"></temperature><feels_like value=\"-53.86\" unit=\"fahrenheit\"></feels_like><humidity value=\"33\" unit=\"%\"></humidity><pressure value=\"1026\" unit=\"hPa\"></pressure><wind><speed value=\"8.23\" unit=\"mph\" name=\"Gentle Breeze\"></speed><gusts value=\"8.77\"></gusts><direction value=\"261\" code=\"W\" name=\"West\"></direction></wind><clouds value=\"0\" name=\"clear sky\"></clouds><visibility value=\"10000\"></visibility><precipitation mode=\"no\"></precipitation><weather number=\"800\" value=\"clear sky\" icon=\"01n\"></weather><lastupdate value=\"2024-02-19T20:19:02\"></lastupdate></current>";

        CurrentWeather currentWeather = XMLSerializer.ReadFromXmlStringWeather(xmlStringWeather.Replace("\\", ""));

        Assert.AreEqual(currentWeather.City.Name, "Lobujya");
        Assert.AreEqual(currentWeather.City.Coord.Lat, "27.9881");
        Assert.AreEqual(currentWeather.City.Coord.Lon, "86.9253");
        Assert.AreEqual(currentWeather.Temperature.Value, "-41.26");
        Assert.AreEqual(currentWeather.Temperature.Min, "-41.26");
        Assert.AreEqual(currentWeather.Temperature.Max, "-41.26");
        Assert.AreEqual(currentWeather.Temperature.Unit, "fahrenheit");
        Assert.AreEqual(currentWeather.FeelsLike.Value, "-53.86");
        Assert.AreEqual(currentWeather.FeelsLike.Unit, "fahrenheit");
        Assert.AreEqual(currentWeather.Humidity.Value, "33");
        Assert.AreEqual(currentWeather.Humidity.Unit, "%");
        Assert.AreEqual(currentWeather.Pressure.Value, "1026");
        Assert.AreEqual(currentWeather.Pressure.Unit, "hPa");
        Assert.AreEqual(currentWeather.Wind.Speed.Value, "8.23");
        Assert.AreEqual(currentWeather.Wind.Speed.Unit, "mph");
        Assert.AreEqual(currentWeather.Wind.Speed.Name, "Gentle Breeze");
        Assert.AreEqual(currentWeather.Wind.Gusts.Value, "8.77");
        Assert.AreEqual(currentWeather.Wind.Direction.Value, "261");
        Assert.AreEqual(currentWeather.Wind.Direction.Code, "W");
        Assert.AreEqual(currentWeather.Wind.Direction.Name, "West");
        Assert.AreEqual(currentWeather.Clouds.Value, "0");
        Assert.AreEqual(currentWeather.Clouds.Name, "clear sky");
        Assert.AreEqual(currentWeather.Visibility.Value, "10000");
        Assert.AreEqual(currentWeather.Precipitation.Mode, "no");
        Assert.AreEqual(currentWeather.WeatherInfo.Number, "800");
        Assert.AreEqual(currentWeather.WeatherInfo.Value, "clear sky");
        Assert.AreEqual(currentWeather.WeatherInfo.Icon, "01n");
        Assert.AreEqual(currentWeather.LastUpdate.Value, DateTime.Parse("2024-02-19 20:19:02.000"));
    }

    [Test]
    public void ReadFromXmlStringShipInformationPass()
    {
        string xmlStringShipInformation = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><XMLShipStructure><Aircraft><name>FighterJet123</name><type>Jet</type><Location><latitude>34.0522</latitude><longitude>-118.2437</longitude><altitude>10000</altitude></Location><Fuel><fuelLevel>5000</fuelLevel><fuelCapacity>10000</fuelCapacity><fuelConsumptionRate>100</fuelConsumptionRate></Fuel></Aircraft></XMLShipStructure>";

        XMLShipStructure ship = XMLSerializer.ReadFromXmlStringShipInformation(xmlStringShipInformation.Replace("\\", ""));

        Assert.AreEqual(ship.craft.name, "FighterJet123");
        Assert.AreEqual(ship.craft.type, "Jet");
        Assert.AreEqual(ship.craft.aircraftLocation.latitude, 34.0522f);
        Assert.AreEqual(ship.craft.aircraftLocation.longitude, -118.2437f);
        Assert.AreEqual(ship.craft.aircraftLocation.altitude, 10000.0f);
        Assert.AreEqual(ship.craft.fuel.fuelLevel, 5000.0f);
        Assert.AreEqual(ship.craft.fuel.fuelCapacity, 10000.0f);
        Assert.AreEqual(ship.craft.fuel.fuelConsumptionRate, 100.0f);
    }

    [Test]
    public void WriteToXmlStringShipInformationPass()
    {
        XMLShipStructure shipInformation;

        shipInformation = new XMLShipStructure();
        shipInformation.craft = new Aircraft();
        shipInformation.craft.name = "FighterJet123";
        shipInformation.craft.type = "Jet";
        shipInformation.craft.aircraftLocation = new Location();
        shipInformation.craft.aircraftLocation.latitude = 34.0522f;
        shipInformation.craft.aircraftLocation.longitude = -118.2437f;
        shipInformation.craft.aircraftLocation.altitude = 10000.0f;
        shipInformation.craft.fuel = new Fuel();
        shipInformation.craft.fuel.fuelLevel = 5000.0f;
        shipInformation.craft.fuel.fuelCapacity = 10000.0f;
        shipInformation.craft.fuel.fuelConsumptionRate = 100.0f;

        string xmlCorrectString = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><XMLShipStructure><Aircraft><name>FighterJet123</name><type>Jet</type><Location><latitude>34.0522</latitude><longitude>-118.2437</longitude><altitude>10000</altitude></Location><Fuel><fuelLevel>5000</fuelLevel><fuelCapacity>10000</fuelCapacity><fuelConsumptionRate>100</fuelConsumptionRate></Fuel></Aircraft></XMLShipStructure>";
        string xmlStringShipInformation = XMLSerializer.WriteToXmlStringShipInformation(shipInformation);
        Assert.AreEqual(xmlCorrectString, xmlStringShipInformation);
    }
}