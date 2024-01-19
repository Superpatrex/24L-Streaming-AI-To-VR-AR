using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Networking;

public class MyScript : MonoBehaviour
{
    public string lat;
    public string lon;

    private IEnumerator GetApiData()
    {
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
                Debug.Log(resultString);
            }
        }
    }

    void Start()
    {
        StartCoroutine(GetApiData());
    }   
   


}
