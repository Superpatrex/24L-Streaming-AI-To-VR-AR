using System;
using UnityEngine;

namespace Utility
{
    /// <summary>
    /// A class to hold utility functions.
    /// </summary>
    public class Utility
    {

    }

    /// <summary>
    /// A class to hold the latitude and longitude of a location in addition to the location name.
    /// </summary>
    public class LatLongLocation
    {
        // Private fields
        private double _lat;
        private double _long;
        private string _locationName;

        // Public fields
        public double Lat
        {
            get => this._lat;
            set => this._lat = value;
        }

        public double Long
        {
            get => this._long;
            set => this._long = value;
        }

        public string LocationName
        {
            get => this._locationName;
            set => this._locationName = value;
        }

        /// <summary>
        /// Constructor for the LatLongLocation class for the individual elements of the object.
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="latDirection"></param>
        /// <param name="lon"></param>
        /// <param name="longDirection"></param>
        /// <param name="locationName"></param>
        public LatLongLocation(double lat, char latDirection, double lon, char longDirection, string locationName)
        {
            if (Math.Abs(lat) >= 90)
            {
                throw new ArgumentException("Latitude must be between -90 and 90 degrees.");
            }
            else if (Math.Abs(lon) >= 180)
            {
                throw new ArgumentException("Longitude must be between -180 and 180 degrees.");
            }
            else if (locationName == null || string.IsNullOrEmpty(locationName) || locationName.Trim().Length == 0)
            {
                throw new ArgumentException("Location name cannot be null.");
            }
            else if (latDirection != 'N' && latDirection != 'S')
            {
                throw new ArgumentException("Latitude direction must be either 'N' or 'S'.");
            }
            else if (longDirection != 'E' && longDirection != 'W')
            {
                throw new ArgumentException("Longitude direction must be either 'E' or 'W'.");
            }
            else if (lat < 0 || lon < 0)
            {
                throw new ArgumentException("Latitude and longitude must be positive.");
            }

            locationName = locationName.Trim();

            this.Lat = lat *= (latDirection == 'N') ? 1 : -1;
            this.Long = lon *= (longDirection == 'E') ? 1 : -1;
            this.LocationName = locationName;
        }

        /// <summary>
        /// Constructor for the LatLongLocation class for a string representation of the latitude and longitude and the location name.
        /// </summary>
        /// <param name="latLongString">String representation of the latittude and longitude and the location name such as "28.3765 N, 81.5494 W Epcot"</param>
        public LatLongLocation(string latLongString)
        {
            if (latLongString == null)
            {
                throw new ArgumentException("Input string cannot be null");
            }
            else if (string.IsNullOrEmpty(latLongString))
            {
                throw new ArgumentException("Input string cannot be empty");
            }
            else if (latLongString.Trim().Length == 0)
            {
                throw new ArgumentException("Input string cannot be whitespace");
            }

            latLongString = latLongString.Trim();

            string [] items = latLongString.Split(' ');

            if (items.Length < 5)
            {
                throw new ArgumentException("The string representation of the latitude and longitude and the location name is not in the correct format.");
            }

            try
            {
                double.Parse(items[0]);
                double.Parse(items[2]);
            }
            catch (FormatException)
            {
                throw new ArgumentException("This string representation does not contain a valid latitude and longitude.");
            }

            if (items[1][0] != 'N' && items[1][0] != 'S' && items[1].Length == 2 || items[3][0] != 'E' && items[3][0] != 'W' && items[3].Length == 1)
            {
                throw new ArgumentException("This string representation does not contain a valid latitude direction.");
            }

            int latIndex = 4;
            string [] locationName = new string[items.Length - latIndex]; 
            Array.Copy(items, 4, locationName, 0, locationName.Length);

            this.Lat = double.Parse(items[0]) * ((items[1][0] == 'N' && items[1].Length == 2) ? 1 : -1);
            this.Long = double.Parse(items[2]) * ((items[3][0] == 'E' && items[3].Length == 1) ? 1 : -1);
            this.LocationName = string.Join(" ", locationName);
        }

        /// <summary>
        /// Returns a string representation of the latitude and longitude with the location name.
        /// </summary>
        /// <returns>A string representation of the latitude and longitude with the location name such as "28.3765 N, 81.5494 W Epcot"</returns>
        public override string ToString()
        {
            char latDirection = (this._lat >= 0) ? 'N' : 'S';
            char longDirection = (this._long >= 0) ? 'E' : 'W';
            double lat = Math.Abs(this.Lat);
            double lon = Math.Abs(this.Long);

            return $"{lat} {latDirection}, {lon} {longDirection} {this.LocationName}";
        }

    }
}
