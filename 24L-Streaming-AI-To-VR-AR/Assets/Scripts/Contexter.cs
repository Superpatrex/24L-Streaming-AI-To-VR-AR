using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using OpenAI;
using NaughtyAttributes;
using TMPro;
using Utility;
using CesiumForUnity;
using Meta.WitAi.TTS.Utilities;

public class Contexter : MonoBehaviour
{   
    public enum Context
    {
        CURRENT_WEATHER,
        OTHER_WEATHER,
        QUESTION,
        CHANGE,
        NULL
    }

    [SerializeField] public ArtificialIntelligence ai;
    [SerializeField] public CesiumGeoreference geoReference;

    private static UnityEvent context = new UnityEvent();

    private static Context currentContext = Context.NULL;
    public static string userInput = "";
    public static string response = "";
    public static bool hasResponseContext = false;
    public static bool hasResponseQuestion = false;
    public static bool hasResponseScenario = false;
    [SerializeField] public TMP_Text transcriptText;
    [SerializeField] public TMP_Text xmlShipInformation;
    [SerializeField] public TTSSpeaker tts;
    [SerializeField] public WeatherAPI api;
    [SerializeField] public MongoDBAPI mongoDBAPI;
    [SerializeField] public GameObject entireShip;


    static UnityEvent m_MyEvent = new UnityEvent();

    public void Start()
    {
    }

    /// <summary>
    /// Updates the script on each frame
    /// </summary>
    public void Update()
    {
        if (hasResponseContext)
        {
            hasResponseContext = false;
            ActOnContext();
        }
        else if (hasResponseQuestion)
        {
            hasResponseQuestion = false;
            tts.Speak(response);
        }
        else if (hasResponseScenario)
        {
            hasResponseScenario = false;

            if (UpdateCurrentScenario())
            {
                tts.Speak("Scenario successfully updated");
            }
            else
            {
                tts.Speak("Error updating scenario");
            }
        }

        if (WeatherAPI.weatherIsReadyUser)
        {
            WeatherAPI.weatherIsReadyUser = false;
            tts.Speak(WeatherAPI.ReturnJsonString);
        }
    }

    /// <summary>
    /// Acts on the context that was returned from the AI
    /// </summary>
    public void ActOnContext()
    {
        string [] spiltString = response.Trim('\"').Split(' ');

        Debug.Log("Contexter: Reponse: " + response);

        if (spiltString[0] == "null")
        {
            Debug.LogError("Contexter: No context");
            tts.Speak("Instructions not understood, please try again.");
        }
        else if (spiltString[0] == "Change")
        {
            geoReference.latitude = (float)(float.Parse(spiltString[1]) * ((spiltString[2] == "N") ? 1 : -1));
            geoReference.longitude = (float)(float.Parse(spiltString[3]) * ((spiltString[4] == "E") ? 1 : -1));
            geoReference.height = (float)0;
            //Debug.Log("Contexter: Changed location to: " + geoReference.latitude + " " + geoReference.longitude);
            api.UpdateWeatherImmediately();
            tts.Speak("Changing Location");
        }
        else if (spiltString[0] == "Weather")
        {
            string lat = ((float)(float.Parse(spiltString[1]) * ((spiltString[2] == "N") ? 1 : -1))).ToString();
            string lon = ((float)(float.Parse(spiltString[3]) * ((spiltString[4] == "E") ? 1 : -1))).ToString();
            //WeatherAPI.isInUse = true;

            //StartCoroutine(WeatherAPI.GetApiData(lat, lon));
            tts.Speak("The weather there is lovely!");
        }
        else if (spiltString[0] == "Question")
        {
            SendQuestionInputStringToAI();
        }
        else if (spiltString[0] == "xmlChange")
        {
            Debug.Log("Current Scenario code: " + spiltString[1]);
            SendScenarioInputStringToAI(spiltString[1]);
        }
        else
        {
            tts.Speak(response);
        }
    }

    /// <summary>
    /// Sends the context to the AI
    /// </summary>
    public void SendContext()
    {
        Debug.Log("Contexter: Sending context");
        userInput = transcriptText.text;
        SendContextInputStringToAI();
    }

    /// <summary>
    /// Sends the context string to the artificial intelligence
    /// </summary>
    public void SendContextInputStringToAI()
    {
        ArtificialIntelligence.userInput = userInput;
        ArtificialIntelligence.returnType = ArtificialIntelligence.AIReturnType.RETURN_STRING;
        ai.SendContexterButtonHandler();
    }

    /// <summary>
    /// Sends the user question to the aritificial intelligence
    /// </summary>
    public void SendQuestionInputStringToAI()
    {
        ArtificialIntelligence.userInput = xmlShipInformation.text + " " + userInput;
        ArtificialIntelligence.returnType = ArtificialIntelligence.AIReturnType.RETURN_STRING;
        ai.SendUserQuestionButtonHandler();
    }

    /// <summary>
    /// Sends the scenario code to the MongoDB API to retrieve the XML data
    /// </summary>
    /// <param name="code">The six digit code for the scenario</param>
    public void SendScenarioInputStringToAI(string code)
    {
        mongoDBAPI.ButtonHandler(code);
    }

    /// <summary>
    /// Updates the current scenario with the new XML data, specifically the lat, long, and the altitude of the aircraft
    /// </summary>
    public bool UpdateCurrentScenario()
    {
        XMLShipStructure tempShipStructure = MongoDBAPI.shipScenario;

        // Null checks to make sure that the scenario is valid
        if (tempShipStructure == null)
        {
            Debug.LogError("Contexter: ship structure is null");
            return false;
        }
        else if (tempShipStructure.craft == null)
        {
            Debug.LogError("Contexter: aircraft is null");
            return false;
        }

        if (string.IsNullOrEmpty(tempShipStructure.craft.name))
        {
            Debug.LogError("Contexter: ship name is null");
            return false;
        }
        else if (string.IsNullOrEmpty(tempShipStructure.craft.type))
        {
            Debug.LogError("Contexter: ship type is null");
            return false;
        }
        else if (tempShipStructure.craft.aircraftLocation == null)
        {
            Debug.LogError("Contexter: ship location is null");
            return false;
        }
        else if (tempShipStructure.craft.fuel == null)
        {
            Debug.LogError("Contexter: ship fuel is null");
            return false;
        }

        Debug.Log("Contexter: Ship Name: " + tempShipStructure.craft.name + " update in progress");

        // Update the ship's location within the environment and within the game environment
        geoReference.latitude = tempShipStructure.craft.aircraftLocation.latitude;
        geoReference.longitude = tempShipStructure.craft.aircraftLocation.longitude;
        entireShip.transform.position = new Vector3(0, tempShipStructure.craft.aircraftLocation.altitude, 0);
        return true;
    }
}