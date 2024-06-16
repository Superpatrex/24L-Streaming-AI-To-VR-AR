using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Meta.WitAi.TTS.Utilities;


public class TextToSpeechSettings : MonoBehaviour
{
    public static TTSSpeaker tts;

    // Start is called before the first frame update
    void Start()
    {
        tts = gameObject.GetComponent<TTSSpeaker>();
        SetName();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void SetName()
    {
        tts.customWitVoiceSettings.voice = Settings.Instance.GetTTSName();
    }
}
