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

    [SerializeField] public CesiumGeoreference georeference;
    
    [SerializeField] public float timeSinceLastUpdate = 0.0f;

    [SerializeField] public float timePerUpdate = 2.0f;

    // Private fields
    private static string returnJsonString;


    /// <summary>
    /// On awake of the script, call the API to get the data
    /// </summary>
    public void Awake()
    {
        // Begin the process of getting the data from the API
        BeginGetApiData(georeference.latitude.ToString(), georeference.longitude.ToString());
    }

    /// <summary>
    /// Update is called once per frame, essentially it calls the API every timePerUpdate in seconds
    /// </summary>
    public void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= timePerUpdate)
        {
            // Begin the process of getting the data from the API
            BeginGetApiData(georeference.latitude.ToString(), georeference.longitude.ToString());
            timeSinceLastUpdate = 0.0f;
            Debug.Log(CurrentWeather.getWeatherInfo(XMLSerializer.ReadFromXmlStringWeather(returnJsonString)));
        }
    }

    /// <summary>
    /// Returns the XML string of the weather data of the latitude and the longtitude of the user
    /// </summary>
    /// <param name="lat">The string representation of the latitude</param>
    /// <param name="lon">The stirng representation of the longtitude</param>
    /// <returns></returns>
    public static IEnumerator GetApiData(string lat, string lon)
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
                Debug.Log(www.downloadHandler.text);

                string resultString = www.downloadHandler.text;
                ReturnJsonString = resultString;
            }
        }
    }

    /// <summary>
    /// Returns the XML string of the weather data of the latitude and the longtitude of the user
    /// </summary>
    /// <param name="latLonItem">The latitude and longtitude of the user</param>
    /// <returns></returns>
    public static IEnumerator GetApiData(LatLongLocation latLonItem)
    {
        return WeatherAPI.GetApiData(latLonItem.Lat.ToString(), latLonItem.Long.ToString());
    }

    /// <summary>
    /// Begins the process of starting the StartCoroutine getting the data from the API
    /// </summary>
    /// <param name="lat"></param>
    /// <param name="lon"></param>
    void BeginGetApiData(string lat, string lon)
    {
        StartCoroutine(GetApiData(lat, lon));
    }
}
