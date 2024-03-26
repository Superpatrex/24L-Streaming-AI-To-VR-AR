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
    public static bool hasResponse = false;
    public TMP_Text transcriptText;

    static UnityEvent m_MyEvent = new UnityEvent();

    [Button("Button Text")]    
    public void Start()
    {
    }

    public void Update()
    {
        if (hasResponse)
        {
            hasResponse = false;
            ActOnContext();
        }
    }

    public void ActOnContext()
    {
        string [] spiltString = response.Split(' ');

        if (spiltString[0] == "Change")
        {
            geoReference.latitude = (float)(float.Parse(spiltString[1]) * ((spiltString[2] == "N") ? 1 : -1));
            geoReference.longitude = (float)(float.Parse(spiltString[3]) * ((spiltString[4] == "E") ? 1 : -1));
            geoReference.height = (float)10000;
            Debug.Log("Contexter: Changed location to: " + geoReference.latitude + " " + geoReference.longitude);
        }
    }

    public void SendContext()
    {
        userInput = transcriptText.text;
        SendContextInputStringToAI();
    }

    public void SendContextInputStringToAI()
    {
        ArtificialIntelligence.userInput = userInput;
        ArtificialIntelligence.returnType = ArtificialIntelligence.AIReturnType.RETURN_STRING;
        ai.SendContexterButtonHandler();
    }
}