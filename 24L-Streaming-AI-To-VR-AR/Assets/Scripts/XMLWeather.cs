using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;

[XmlRoot("current")] // 'XMLRoot' is not defined in the current context
public class CurrentWeather
{
    public static Dictionary<int, string> WeatherCodes = new Dictionary<int, string>()
    {
        {200, "Thunderstorm with light rain"},
        {201, "Thunderstorm with rain"},
        {202, "Thunderstorm with heavy rain"},
        {210, "Light thunderstorm"},
        {211, "Thunderstorm"},
        {212, "Heavy thunderstorm"},
        {221, "Ragged thunderstorm"},
        {230, "Thunderstorm with light drizzle"},
        {231, "Thunderstorm with drizzle"},
        {232, "Thunderstorm with heavy drizzle"},
        {300, "Light intensity drizzle"},
        {301, "Drizzle"},
        {302, "Heavy intensity drizzle"},
        {310, "Light intensity drizzle rain"},
        {311, "Drizzle rain"},
        {312, "Heavy intensity drizzle rain"},
        {313, "Shower rain and drizzle"},
        {314, "Heavy shower rain and drizzle"},
        {321, "Shower drizzle"},
        {500, "Light rain"},
        {501, "Moderate rain"},
        {502, "Heavy intensity rain"},
        {503, "Very heavy rain"},
        {504, "Extreme rain"},
        {511, "Freezing rain"},
        {520, "Light intensity shower rain"},
        {521, "Shower rain"},
        {522, "Heavy intensity shower rain"},
        {531, "Ragged shower rain"},
        {600, "Light snow"},
        {601, "Snow"},
        {602, "Heavy snow"},
        {611, "Sleet"},
        {612, "Light shower sleet"},
        {613, "Shower sleet"},
        {615, "Light rain and snow"},
        {616, "Rain and snow"},
        {620, "Light shower snow"},
        {621, "Shower snow"},
        {622, "Heavy shower snow"},
        {701, "Mist"},
        {711, "Smoke"},
        {721, "Haze"},
        {731, "Sand/dust whirls"},
        {741, "Fog"},
        {751, "Sand"},
        {761, "Dust"},
        {762, "Volcanic ash"},
        {771, "Squalls"},
        {781, "Tornado"},
        {800, "Clear sky"},
        {801, "Few clouds: 11-25%"},
        {802, "Scattered clouds: 25-50%"},
        {803, "Broken clouds: 51-84%"},
        {804, "Overcast clouds: 85-100%"}
    };

    public static string getWeatherInfo(CurrentWeather weather)
    {
        //return WeatherCodes[int.Parse(weather.Weather.WeatherInfo.Value)];
        return WeatherCodes[int.Parse(weather.WeatherInfo.Number)];
    }

    [XmlElement("city")]
    public City City { get; set; }

    [XmlElement("temperature")]
    public Temperature Temperature { get; set; }

    [XmlElement("feels_like")]
    public FeelsLike FeelsLike { get; set; }

    [XmlElement("humidity")]
    public Humidity Humidity { get; set; }

    [XmlElement("pressure")]
    public Pressure Pressure { get; set; }

    [XmlElement("wind")]
    public Wind Wind { get; set; }

    [XmlElement("clouds")]
    public Clouds Clouds { get; set; }

    [XmlElement("visibility")]
    public Visibility Visibility { get; set; }

    [XmlElement("precipitation")]
    public Precipitation Precipitation { get; set; }

    [XmlElement("weather")]
    public WeatherInfo WeatherInfo { get; set; }

    [XmlElement("lastupdate")]
    public LastUpdate LastUpdate { get; set; }
}

public class City
{
    [XmlAttribute("id")]
    public string Id { get; set; }

    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlElement("coord")]
    public Coord Coord { get; set; }

    [XmlElement("country")]
    public string Country { get; set; }

    public int Timezone { get; set; }

    public Sun Sun { get; set; }
}

public class Coord
{
    [XmlAttribute("lon")]
    public string Lon { get; set; }

    [XmlAttribute("lat")]
    public string Lat { get; set; }
}

public class Sun
{
    [XmlAttribute("rise")]
    public DateTime Rise { get; set; }

    [XmlAttribute("set")]
    public DateTime Set { get; set; }
}

public class Temperature
{
    [XmlAttribute("value")]
    public string Value { get; set; }

    [XmlAttribute("min")]
    public string Min { get; set; }

    [XmlAttribute("max")]
    public string Max { get; set; }

    [XmlAttribute("unit")]
    public string Unit { get; set; }
}

public class FeelsLike
{
    [XmlAttribute("value")]
    public string Value { get; set; }

    [XmlAttribute("unit")]
    public string Unit { get; set; }
}

public class Humidity
{
    [XmlAttribute("value")]
    public string Value { get; set; }

    [XmlAttribute("unit")]
    public string Unit { get; set; }
}

public class Pressure
{
    [XmlAttribute("value")]
    public string Value { get; set; }

    [XmlAttribute("unit")]
    public string Unit { get; set; }
}

public class Wind
{
    [XmlElement("speed")]
    public Speed Speed { get; set; }
    [XmlElement("gusts")]

    public Gusts Gusts { get; set; }
    [XmlElement("direction")]

    public Direction Direction { get; set; }
}

public class Speed
{
    [XmlAttribute("value")]
    public string Value { get; set; }

    [XmlAttribute("unit")]
    public string Unit { get; set; }

    [XmlAttribute("name")]
    public string Name { get; set; }
}

public class Gusts
{
    [XmlAttribute("value")]
    public string Value { get; set; }
}

public class Direction
{
    [XmlAttribute("value")]
    public string Value { get; set; }

    [XmlAttribute("code")]
    public string Code { get; set; }

    [XmlAttribute("name")]
    public string Name { get; set; }
}

public class Clouds
{
    [XmlAttribute("value")]
    public string Value { get; set; }

    [XmlAttribute("name")]
    public string Name { get; set; }
}

public class Visibility
{
    [XmlAttribute("value")]
    public string Value { get; set; }
}

public class Precipitation
{
    [XmlAttribute("mode")]
    public string Mode { get; set; }
}

public class WeatherInfo
{
    [XmlAttribute("number")]
    public string Number { get; set; }

    [XmlAttribute("value")]
    public string Value { get; set; }

    [XmlAttribute("icon")]
    public string Icon { get; set; }
}

public class LastUpdate
{
    [XmlAttribute("value")]
    public DateTime Value { get; set; }
}