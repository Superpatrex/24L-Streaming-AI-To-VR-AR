using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Utility;

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

    [TestCase("28.3765 N 81.5494 W Epcot", 28.3765, -81.5494, "Epcot")]
    [TestCase("28.3765 N 81.5494 W Walt Disney World Epcot Park", 28.3765, -81.5494, "Walt Disney World Epcot Park")]
    [TestCase("30.5595 S 22.9375 E Cape Town", -30.5595, 22.9375, "Cape Town")]
    [TestCase("30.5595 S 22.9375 E Cape Town South Africa", -30.5595, 22.9375, "Cape Town South Africa")]
    public void LatLongLocationIndividualPass(string latlongString, double lat, double lon, string locationName)
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
}