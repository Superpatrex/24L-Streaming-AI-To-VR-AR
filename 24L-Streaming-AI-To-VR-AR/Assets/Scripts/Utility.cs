namespace Utility
{
    public class Utility
    {

    }

    public class LatLong
    {
        private double _lat;
        private double _long;
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

        public LatLong(double lat, char latDirection, double lon, char longDirection)
        {
            this._lat = lat *= (latDirection == 'N') ? 1 : -1;
            this._long = lon (latDirection == 'E') ? 1 : -1;
        }

        public LatLong(string latLongString)
        {
            
        }

        public override string ToString()
        {
            char latDirection = (this._lat >= 0) ? 'N' : 'S';
            char longDirection = (this._long >= 0) ? 'E' : 'W';

            return $"{this._lat} {latDirection}, {this._long} {longDirection}";
        }

    }
}
