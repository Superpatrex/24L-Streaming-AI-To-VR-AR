using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;
using Utility;
using CesiumForUnity;

public class WeatherAPI : MonoBehaviour
{
    // Public fields
    public static string ReturnJsonString
    {
        get => returnJsonString;
        set => returnJsonString = value;
    }

    public enum weatherEffects
    {
        CLEAR_SKY,
        RAIN_DOWNPOUR
    }

    [SerializeField] public CesiumGeoreference georeference;
    
    [SerializeField] public float timeSinceLastUpdate = 0.0f;

    [SerializeField] public float timePerUpdate = 2.0f;

    // Skybox materials
    [SerializeField] public Material CLEAR_SKYBOX;
    [SerializeField] public Material FEW_CLOUDS_SKYBOX;
    [SerializeField] public Material CLOUDS_SKYBOX;
    [SerializeField] public Material OVERCAST_SKYBOX;
    [SerializeField] public Material SNOW_SKYBOX;
    [SerializeField] public Material DARK_CLOUDS_SKYBOX;
    [SerializeField] public Material SAND_SKYBOX;

    // Weather effects
    [SerializeField] public GameObject rainDownpour;
    [SerializeField] public GameObject rainDrizzle;
    [SerializeField] public GameObject rainLight;
    [SerializeField] public GameObject rainSteady;
    [SerializeField] public GameObject snowBlizzard;
    [SerializeField] public GameObject mistHazeFog;
    [SerializeField] public GameObject sandDust;
    [SerializeField] public GameObject smokeDustVolcanicAsh;
    private GameObject currentWeather = null;

    // Private fields
    private static string returnJsonString;
    public static bool isInUse = false;
    public static bool weatherIsReadyVRUser = false;
    public static bool weatherIsReadyInstructor = false;
    public static bool? isVRUser = null;



    /// <summary>
    /// On awake of the script, call the API to get the data
    /// </summary>
    public void Awake()
    {
        // Begin the process of getting the data from the API
        BeginGetApiData(georeference.latitude.ToString(), georeference.longitude.ToString(), null);
    }

    public void UpdateWeatherImmediately()
    {
        timeSinceLastUpdate = timePerUpdate;
    }

    /// <summary>
    /// Update is called once per frame, essentially it calls the API every timePerUpdate in seconds
    /// </summary>
    public void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= timePerUpdate && !isInUse)
        {
            // Begin the process of getting the data from the API
            BeginGetApiData(georeference.latitude.ToString(), georeference.longitude.ToString(), null);
            timeSinceLastUpdate = 0.0f;
            string curWeather = CurrentWeather.getWeatherInfo(XMLSerializer.ReadFromXmlStringWeather(returnJsonString));

            Debug.Log("OpenWeatherAPI update " + isVRUser);
            if (isVRUser == null)
            {
                ChangeSkyBox(curWeather);
            }
        }
    }

    /// <summary>
    /// Returns the XML string of the weather data of the latitude and the longtitude of the user
    /// </summary>
    /// <param name="lat">The string representation of the latitude</param>
    /// <param name="lon">The stirng representation of the longtitude</param>
    /// <returns></returns>
    public static IEnumerator GetApiData(string lat, string lon, bool? VRUser)
    {
        // Making sure that data being passed in is correct
        if (lat == null)
        {
            throw new ArgumentException("lat cannot be null");
        }
        else if (lon == null)
        {
            throw new ArgumentException("lon cannot be null");
        }
        else if (!double.TryParse(lat, out _))
        {
            throw new ArgumentException("lat needs to be a double");
        }
        else if (!double.TryParse(lon, out _))
        {
            throw new ArgumentException("lon needs to be a double");
        }
        else if (Math.Abs(double.Parse(lat)) > 90.0)
        {
            throw new ArgumentException("lat cannot be more than 90.0");
        }
        else if (Math.Abs(double.Parse(lon)) > 180.0)
        {
            throw new ArgumentException("lon cannot be more than 180.0");
        }

        string apiKey = "ecd4a3434faff0a74647d977fc4be299";
        if (VRUser != null)
        {
            isVRUser = VRUser;
        }
        string url = $"https://pro.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&APPID={apiKey}&mode=xml&units=imperial";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                // Show results as text
                //Debug.Log(www.downloadHandler.text);

                string resultString = www.downloadHandler.text;
                ReturnJsonString = resultString;
            }
        }

        if (isVRUser == true && VRUser != null)
        {
            weatherIsReadyVRUser = true;
        }
        else if (VRUser != null)
        {
            weatherIsReadyInstructor = true;
        }
    }

    /// <summary>
    /// Returns the XML string of the weather data of the latitude and the longtitude of the user
    /// </summary>
    /// <param name="latLonItem">The latitude and longtitude of the user</param>
    /// <returns></returns>
    public static IEnumerator GetApiData(LatLongLocation latLonItem, bool VRUser)
    {
        return WeatherAPI.GetApiData(latLonItem.Lat.ToString(), latLonItem.Long.ToString(), VRUser);
    }

    /// <summary>
    /// Begins the process of starting the StartCoroutine getting the data from the API
    /// </summary>
    /// <param name="lat">The string representation of the latitude</param>
    /// <param name="lon">The string representation of the longitude</param>
    public void BeginGetApiData(string lat, string lon, bool? VRUser)
    {
        StartCoroutine(GetApiData(lat, lon, null));
    }

    public void ChangeSkyBox(string weather)
    {
        Debug.LogError(weather);

        switch(weather)
        {
            case "Clear sky":
                ToggleOffWeather();
                RenderSettings.skybox = CLEAR_SKYBOX;
                break;
            case "Light intensity drizzle":
            case "Light intensity drizzle rain":
            case "Light rain":
            case "Light intensity shower rain":
                ToggleWeather(rainLight);
                RenderSettings.skybox = OVERCAST_SKYBOX;
                break;
            case "Drizzle":
            case "Drizzle rain":
            case "Shower drizzle":
            case "Shower rain and drizzle":
                ToggleWeather(rainDrizzle);
                RenderSettings.skybox = OVERCAST_SKYBOX;
                break;
            case "Moderate rain":
            case "Shower rain":
            case "Freezing rain":
                ToggleWeather(rainSteady);
                RenderSettings.skybox = OVERCAST_SKYBOX;
                break;
            case "Heavy intensity drizzle":
            case "Heavy intensity drizzle rain":
            case "Heavy shower rain and drizzle":
            case "Heavy intensity rain":
            case "Very heavy rain":
            case "Extreme rain":
            case "Heavy intensity shower rain":
            case "Ragged shower rain":
            case "Thunderstorm with light rain":
            case "Thunderstorm with rain":
            case "Thunderstorm with heavy rain":
            case "Thunderstorm with light drizzle":
            case "Thunderstorm with drizzle":
            case "Thunderstorm with heavy drizzle":
            case "Light thunderstorm":
            case "Ragged thunderstorm":
            case "Heavy thunderstorm":
            case "Thunderstorm":
            case "Squalls":
            case "Tornado":
                ToggleWeather(rainDownpour);
                RenderSettings.skybox = OVERCAST_SKYBOX;
                break;
            case "Overcast clouds: 85-100%":
                ToggleOffWeather();
                RenderSettings.skybox = OVERCAST_SKYBOX;
                break;
            case "Light snow":
            case "Snow":
            case "Sleet":
            case "Light shower sleet":
            case "Shower sleet":
            case "Light rain and snow":
            case "Rain and snow":
            case "Light shower snow":
            case "Shower snow":
                ToggleWeather(snowBlizzard);
                RenderSettings.skybox = SNOW_SKYBOX;
                break;
            case "Heavy snow":
            case "Heavy shower snow":
                ToggleWeather(snowBlizzard);
                RenderSettings.skybox = DARK_CLOUDS_SKYBOX;
                break;
            case "Few clouds: 11-25%":
                ToggleOffWeather();
                RenderSettings.skybox = FEW_CLOUDS_SKYBOX;
                break;
            case "Scattered clouds: 25-50%":
            case "Broken clouds: 51-84%":
                ToggleOffWeather();
                RenderSettings.skybox = CLOUDS_SKYBOX;
                break;
            case "Mist":
            case "Haze":
            case "Fog":
                ToggleWeather(mistHazeFog);
                RenderSettings.skybox = OVERCAST_SKYBOX;
                break;
            case "Sand/dust whirls":
            case "Sand":
                ToggleWeather(sandDust);
                RenderSettings.skybox = SAND_SKYBOX;
                break;
            case "Smoke":
            case "Dust":
            case "Volcanic ash":
                ToggleWeather(smokeDustVolcanicAsh);
                RenderSettings.skybox = OVERCAST_SKYBOX;
                break;
            default:
                Debug.LogError("Weather not found this is bad");
                break;
        }
    }

    public void ToggleWeather(GameObject weather)
    {
        if (currentWeather == null)
        {
            weather.SetActive(true);
            currentWeather = weather;
        }
        else if (currentWeather != weather)
        {
            currentWeather.SetActive(false);
            weather.SetActive(true);
            currentWeather = weather;
        }
    }

    public void ToggleOffWeather()
    {
        if (currentWeather != null)
        {
            currentWeather.SetActive(false);
            currentWeather = null;
        }
    }
}
