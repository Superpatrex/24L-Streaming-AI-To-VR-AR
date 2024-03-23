
using UnityEngine;
using Meta.WitAi.TTS.Utilities;
using UnityEngine.Events;

namespace Core3lb
{
    public class MetaWittTTS : MonoBehaviour
    {
        [CoreRequired]
        public TTSSpeaker mySpeaker;
        [Tooltip("Change Voice in TTSpeaker")]
        [TextArea]
        public string testText = "Hello World";
        [Tooltip("Will attempt to make it use Speech Synthesis Markup Language (SSML)")]
        public bool useSSML;

        public UnityEvent<string> onSpeak;
        public void Awake()
        {
            if(mySpeaker == null)
            {
                mySpeaker = GetComponent<TTSSpeaker>();
                if(mySpeaker == null)
                {
                    Debug.LogError("No Speaker Found");
                }
            }

        }

        [CoreButton]
        public void TestSpeaker()
        {
            _Speak(testText);
        }

        public void _Speak(string whatToSay)
        {
            if (useSSML)
            {
                mySpeaker.PrependedText = "<speak>";
                mySpeaker.AppendedText = "</speak>";
            }
            mySpeaker.Speak(whatToSay);
            onSpeak.Invoke(whatToSay);
        }
    }
}
