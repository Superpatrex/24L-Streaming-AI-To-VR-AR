using System;
using UnityEngine;
using UnityEngine.Events;

namespace Core3lb
{
    public class STT_KeyWordEvent : MonoBehaviour
    {
        public MetaWittSTT voiceSystem;
        public string keyWord;
        [CoreReadOnly]
        public string extraText;
        public bool usePartialTranscribe;
        public UnityEvent onKeyWordHeard;

        public void Awake()
        {
            if(voiceSystem)
            {
                if(usePartialTranscribe)
                {
                    voiceSystem.onTranscriptComplete.AddListener(ProcessText);
                }
                else
                {
                    voiceSystem.onTranscriptComplete.AddListener(ProcessText);
                }

            }
        }

        public void ProcessText(string text)
        {
            string holder = text;
            if(holder.StartsWith(keyWord, StringComparison.OrdinalIgnoreCase))
            {
                onKeyWordHeard.Invoke();
            }         
        }
    }
}
