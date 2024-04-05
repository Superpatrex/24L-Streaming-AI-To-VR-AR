using System;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using OpenAI;
using NaughtyAttributes;
using TMPro;
using Utility;
using CesiumForUnity;
using Meta.WitAi.TTS.Utilities;
using SciFiShipController;

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
    [SerializeField] public Cesium3DTileset tileset;

    private static UnityEvent context = new UnityEvent();

    private static Context currentContext = Context.NULL;
    public static string userInput = "";
    public static string response = "";
    public static bool hasVRUserResponseContext = false;
    public static bool hasVRUserResponseQuestion = false;
    public static bool hasVRUserResponseScenario = false;
    public static bool hasVRUserResponseWeather = false;
    public static bool hasInstructorResponseContext = false;
    public static bool hasInstructorLocationChange = false;
    public static bool hasInstructorResponseQuestion = false;
    public static bool hasInstructorResponseScenario = false;
    public static bool hasInstructorResponseWeather = false;
    [SerializeField] public TMP_Text transcriptText;
    [SerializeField] public TMP_Text xmlShipInformation;
    [SerializeField] public TTSSpeaker tts;
    [SerializeField] public WeatherAPI api;
    [SerializeField] public MongoDBAPI mongoDBAPI;
    [SerializeField] public GameObject entireShip;
    [SerializeField] public GameObject uiScreen;
    [SerializeField] public InstructorChat chat;
    [SerializeField] public ShipControlModule shipControl;

    static UnityEvent m_MyEvent = new UnityEvent();

    public void Start()
    {
        // Set the text to speech voice to a name depending on the start menu
        tts.customWitVoiceSettings.voice = Settings.Instance.GetTTSName();
        tileset.maximumScreenSpaceError = Settings.Instance.GetCesiumGraphicsQuality();
    }

    /// <summary>
    /// Updates the script on each frame
    /// </summary>
    public void Update()
    {
        if (hasVRUserResponseContext)
        {
            hasVRUserResponseContext = false;
            ActOnContextVRUser();
        }
        else if (hasVRUserResponseQuestion)
        {
            hasVRUserResponseQuestion = false;
            Debug.Log("I should be saying this " + response);
            var chunks = SplitIntoSpeakableChunks(response);
            StartCoroutine(SpeakChunks(chunks));
        }
        else if (hasVRUserResponseScenario)
        {
            hasVRUserResponseScenario = false;

            if (UpdateCurrentScenario())
            {
                api.UpdateWeatherImmediately();
                tts.Speak("Scenario successfully updated");
            }
            else
            {
                tts.Speak("Error updating scenario");
            }
        }
        else if (hasVRUserResponseWeather)
        {
            hasVRUserResponseWeather = false;
            WeatherAPI.isVRUser = null;
            tts.Speak(response);
        }

        if (hasInstructorResponseContext)
        {
            hasInstructorResponseContext = false;
            ActOnContextInstructorUser();
        }
        else if (hasInstructorResponseQuestion)
        {
            hasInstructorResponseQuestion = false;
            chat.AddvRITAMessage(response);
        }
        else if (hasInstructorResponseScenario)
        {
            hasInstructorResponseScenario = false;
            if (UpdateCurrentScenario())
            {
                chat.AddvRITAMessage("Scenario successfully updated");
            }
            else
            {
                chat.AddvRITAMessage("Error updating scenario");
            }
        }
        else if (hasInstructorResponseWeather)
        {
            hasInstructorResponseWeather = false;
            WeatherAPI.isVRUser = null;
            chat.AddvRITAMessage(response);
        }

        if (WeatherAPI.weatherIsReadyVRUser)
        {
            WeatherAPI.weatherIsReadyVRUser = false;
            //Debug.LogError(WeatherAPI.ReturnJsonString);
            //tts.Speak(WeatherAPI.ReturnJsonString);
            SendWeatherInputStringToAI(true);
        }
        else if (WeatherAPI.weatherIsReadyInstructor)
        {
            WeatherAPI.weatherIsReadyInstructor = false;
            //Debug.LogError(WeatherAPI.ReturnJsonString);
            //chat.AddvRITAMessage(WeatherAPI.ReturnJsonString);
            SendWeatherInputStringToAI(false);
        }

        
    }

    private IEnumerator SpeakChunks(List<string> chunks)
    {
        foreach (var chunk in chunks)
        {
            Debug.Log("Contexter: Speaking chunk: " + chunk);
            tts.Speak(chunk);

            // Assuming tts.Speak() starts the speech immediately and tts.IsSpeaking() returns whether the speech is ongoing
            while (tts.IsActive)
            {
                yield return null; // Wait for the next frame
            }
        }
    }

    private List<string> SplitIntoSpeakableChunks(string str)
    {
        var words = str.Split(' ');
        var chunk = new StringBuilder();
        var chunks = new List<string>();

        foreach (var word in words)
        {
            if (chunk.Length + word.Length >= 200)
            {
                chunks.Add(chunk.ToString());
                chunk.Length = 0;  // Clear the StringBuilder
            }

            if (chunk.Length > 0)
                chunk.Append(' ');

            chunk.Append(word);
        }

        if (chunk.Length > 0)
            chunks.Add(chunk.ToString());

        return chunks;
    }

    /// <summary>
    /// Acts on the context that was returned from the AI
    /// </summary>
    public void ActOnContextVRUser()
    {
        string [] spiltString = response.Trim('\"').Split(' ');

        Debug.Log("Contexter: VR User Reponse: " + response);

        if (spiltString[0] == "null")
        {
            Debug.LogError("Contexter: No context");
            tts.Speak("Instructions not understood or not supported, please try again.");
        }
        else if (spiltString[0] == "Change")
        {
            geoReference.latitude = (float)(float.Parse(spiltString[1]) * ((spiltString[2] == "N") ? 1 : -1));
            geoReference.longitude = (float)(float.Parse(spiltString[3]) * ((spiltString[4] == "E") ? 1 : -1));
            geoReference.height = (float)0;
            //Debug.Log("Contexter: Changed location to: " + geoReference.latitude + " " + geoReference.longitude);
            api.UpdateWeatherImmediately();
            Debug.Log("I should be saying this " + response);
            tts.Speak("Changing Location");
        }
        else if (spiltString[0] == "Weather")
        {
            string lat;
            string lon;

            if (spiltString.Length == 1)
            {
                lat = geoReference.latitude.ToString();
                lon = geoReference.longitude.ToString();
            }
            else
            {
                lat = ((float)(float.Parse(spiltString[1]) * ((spiltString[2] == "N") ? 1 : -1))).ToString();
                lon = ((float)(float.Parse(spiltString[3]) * ((spiltString[4] == "E") ? 1 : -1))).ToString();
            }

            //WeatherAPI.isInUse = true;
            StartCoroutine(WeatherAPI.GetApiData(lat, lon, true));
            //tts.Speak("The weather there is lovely!");
        }
        else if (spiltString[0] == "Question")
        {
            SendQuestionInputStringToAI(true);
        }
        else if (spiltString[0] == "xmlChange")
        {
            Debug.Log("Current Scenario code: " + spiltString[1]);
            SendScenarioInputStringToAI(spiltString[1], true);
        }
        else if (spiltString[0] == "Spawn")
        {
            SpawnEnemyAI.Instance.spawn = true;
            tts.Speak("Spawning enemy aircrafts");
        }
        else if (spiltString[0] == "Despawn")
        {
            SpawnEnemyAI.Instance.despawn = true;
            tts.Speak("Despawning enemy aircrafts");
        }
        else
        {
            tts.Speak(response);
        }
    }

    /// <summary>
    /// Sends the context to the AI from the Instructor
    /// </summary>
    public void ActOnContextInstructorUser()
    {
        string [] spiltString = response.Trim('\"').Split(' ');

        Debug.Log("Contexter: Instructor Reponse: " + response);

        if (spiltString[0] == "null")
        {
            Debug.LogError("Contexter: No instructor context");
            chat.AddvRITAMessage("Instructions not understood or not supported, please try again.");
            //tts.Speak("Instructions not understood, please try again.");
        }
        else if (spiltString[0] == "Change")
        {
            geoReference.latitude = (float)(float.Parse(spiltString[1]) * ((spiltString[2] == "N") ? 1 : -1));
            geoReference.longitude = (float)(float.Parse(spiltString[3]) * ((spiltString[4] == "E") ? 1 : -1));
            geoReference.height = (float)0;
            //Debug.Log("Contexter: Changed location to: " + geoReference.latitude + " " + geoReference.longitude);
            api.UpdateWeatherImmediately();
            chat.AddvRITAMessage("Changing Location");
            tts.Speak("Instructor changed location");
            //tts.Speak("Changing Location");
        }
        else if (spiltString[0] == "Weather")
        {
            string lat = ((float)(float.Parse(spiltString[1]) * ((spiltString[2] == "N") ? 1 : -1))).ToString();
            string lon = ((float)(float.Parse(spiltString[3]) * ((spiltString[4] == "E") ? 1 : -1))).ToString();
            //WeatherAPI.isInUse = true;

            StartCoroutine(WeatherAPI.GetApiData(lat, lon, false));
            //tts.Speak("The weather there is lovely!");
        }
        else if (spiltString[0] == "Question")
        {
            SendQuestionInputStringToAI(false);
        }
        else if (spiltString[0] == "xmlChange")
        {
            Debug.Log("Current Scenario code: " + spiltString[1]);
            SendScenarioInputStringToAI(spiltString[1], false);
            tts.Speak("Instructor changed the scenario to " + spiltString[1]);
        }
        else if (spiltString[0] == "Spawn")
        {
            SpawnEnemyAI.Instance.spawn = true;
            chat.AddvRITAMessage("Spawning enemy aircrafts");
            tts.Speak("Instructor spawned enemy aircrafts");
        }
        else if (spiltString[0] == "Despawn")
        {
            SpawnEnemyAI.Instance.despawn = true;
            chat.AddvRITAMessage("Despawning enemy aircrafts");
            tts.Speak("Instructor despawned enemy aircrafts");
        }
        else
        {
            chat.AddvRITAMessage(response);
        }
    }

    /// <summary>
    /// Sends the context to the AI
    /// </summary>
    public void SendContext(bool VRUser)
    {
        if (!uiScreen.activeSelf)
        {
            Debug.Log("Contexter: UI Screen is not active");
            return;
        }

        Debug.Log("Contexter: Sending context " + (VRUser ? "VR User" : "Instructor") + " context to AI");
        userInput = transcriptText.text;
        SendContextInputStringToAI(VRUser);
    }

    /// <summary>
    /// Sends the context string to the artificial intelligence
    /// </summary>
    public void SendContextInputStringToAI(bool VRuser)
    {
        ArtificialIntelligence.userInput = userInput;
        ai.SendContexterButtonHandler(VRuser);
    }

    /// <summary>
    /// Sends the user question to the aritificial intelligence
    /// </summary>
    public void SendQuestionInputStringToAI(bool VRuser)
    {
        ArtificialIntelligence.userInput = xmlShipInformation.text + " " + userInput;
        ai.SendUserQuestionButtonHandler(VRuser);
    }

    public void SendWeatherInputStringToAI(bool VRuser)
    {
        ArtificialIntelligence.userInput = WeatherAPI.ReturnJsonString + " " + userInput;
        //Debug.Log(ArtificialIntelligence.userInput);
        ai.SendWeatherButtonHandler(VRuser);
    }

    /// <summary>
    /// Sends the scenario code to the MongoDB API to retrieve the XML data
    /// </summary>
    /// <param name="code">The six digit code for the scenario</param>
    public void SendScenarioInputStringToAI(string code, bool VRuser)
    {
        mongoDBAPI.ButtonHandler(code, VRuser);
    }

    /// <summary>
    /// Updates the current scenario with the new XML data, specifically the lat, long, and the altitude of the aircraft
    /// </summary>
    /// <returns>True if the scenario was updated and false otherwise</returns>
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

        if (tempShipStructure.craft.aircraftLocation.enemyAircraft == null)
        {
            Debug.Log("Contexter: enemy aircraft will not be spawned");
        }
        else if (tempShipStructure.craft.aircraftLocation.enemyAircraft.areEnemies == false)
        {
            Debug.Log("Contexter: enemy aircraft are being deleted");
            SpawnEnemyAI.Instance.despawn = SpawnEnemyAI.Instance.shipFlag;
        }
        else
        {
            Debug.Log("Contexter: enemy aircraft will be spawned");
            SpawnEnemyAI.Instance.spawn = true;
        }

        Debug.Log("Contexter: Ship Name: " + tempShipStructure.craft.name + " update in progress");

        // Update the ship's location within the environment and within the game environment
        geoReference.latitude = tempShipStructure.craft.aircraftLocation.latitude;
        geoReference.longitude = tempShipStructure.craft.aircraftLocation.longitude;
        entireShip.transform.position = new Vector3(0, tempShipStructure.craft.aircraftLocation.altitude, 0);

        if (tempShipStructure.craft.weapons == null)
        {
            Debug.LogError("Contexter: ship weapons are null");
            return false;
        }
        
        List<SciFiShipController.Weapon> weapons = shipControl.GetWeapons();

        foreach (Weapon w in weapons)
        {
            if (w.name == "Left Gun")
            {
                w.ammunition = tempShipStructure.craft.weapons.leftGun.roundsLeft;
                w.reloadTime = tempShipStructure.craft.weapons.leftGun.timeBetweenFiring;
                w.inaccuracy = tempShipStructure.craft.weapons.leftGun.inaccuracy;
                w.inaccuracy = tempShipStructure.craft.weapons.leftGun.weaponType;
            }
            else if (w.name == "Right Gun")
            {
                w.ammunition = tempShipStructure.craft.weapons.rightGun.roundsLeft;
                w.reloadTime = tempShipStructure.craft.weapons.rightGun.timeBetweenFiring;
                w.inaccuracy = tempShipStructure.craft.weapons.rightGun.inaccuracy;
                w.inaccuracy = tempShipStructure.craft.weapons.rightGun.weaponType;
            }
            else if (w.name == "Left Missile")
            {
                w.ammunition = tempShipStructure.craft.weapons.leftMissle.roundsLeft;
                w.reloadTime = tempShipStructure.craft.weapons.leftMissle.timeBetweenFiring;
                w.inaccuracy = tempShipStructure.craft.weapons.leftMissle.inaccuracy;
                w.inaccuracy = tempShipStructure.craft.weapons.leftMissle.weaponType;
            }
            else if (w.name == "Right Missile")
            {
                w.ammunition = tempShipStructure.craft.weapons.rightMissle.roundsLeft;
                w.reloadTime = tempShipStructure.craft.weapons.rightMissle.timeBetweenFiring;
                w.inaccuracy = tempShipStructure.craft.weapons.rightMissle.inaccuracy;
                w.inaccuracy = tempShipStructure.craft.weapons.rightMissle.weaponType;
            }
        }
        return true;
    }
}