using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using OpenAI;
using NaughtyAttributes;
using TMPro;

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

    private static ArtificialIntelligence ai = new ArtificialIntelligence();

    private static UnityEvent context = new UnityEvent();

    private static Context currentContext = Context.NULL;
    public static string userInput = "";
    public static string response = "";
    public TMP_Text transcriptText;

    static UnityEvent m_MyEvent = new UnityEvent();

    [Button("Button Text")]    
    public void Start()
    {
    }

    [Button("Button Text")]
    public void SendContext()
    {
        userInput = transcriptText.text;
        currentStringContext();

        switch (currentContext)
        {
            case Context.CURRENT_WEATHER:
                // Do something
                break;
            case Context.OTHER_WEATHER:
                // Do something
                break;
            case Context.QUESTION:
                // Do something
                break;
            case Context.CHANGE:
                // Do something
                break;
            case Context.NULL:
                // Do something
                break;
        }

        Debug.Log("Context: " + currentContext + " " + userInput);
    }

    public static void currentStringContext()
    {
        ArtificialIntelligence.userInput = userInput;
        ArtificialIntelligence.returnType = ArtificialIntelligence.AIReturnType.RETURN_STRING;
        ai.SendContexterButtonHandler();
    }
}