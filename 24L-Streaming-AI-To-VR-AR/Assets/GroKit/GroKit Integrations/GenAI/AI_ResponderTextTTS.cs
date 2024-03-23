using UnityEngine.Events;
using UnityEngine;


namespace Core3lb
{
    public class AI_ResponderTextTTS : MonoBehaviour
    {
        [CoreRequired]
        public MetaWittSTT grokitProcessSpeech;
        [CoreRequired]
        public BaseAI_Text aiAPI;
        [CoreRequired]
        public MetaWittTTS grokitSpeaker;

        public UnityEvent onStartListening;
        public UnityEvent processingComplete;

        public void Start()
        {
            SetupListeners();
        }

        protected virtual void SetupListeners()
        {
            if(grokitProcessSpeech)
            {
                grokitProcessSpeech.onTranscriptComplete.AddListener(ProcessAI);
            }
            aiAPI.requestComplete.AddListener(_AIRequestSuccess);
            aiAPI.requestFailed.AddListener(_AIRequestFail);
        }

        //If your using STT Start Listening here
        [CoreButton("Start Listening")]
        public virtual void _StartListening()
        {
            grokitProcessSpeech._StartListening();
        }

        public virtual void _Speak(string whatToSay)
        {
            if(grokitSpeaker)
            {
                grokitSpeaker._Speak(whatToSay);
            }
        }

        //Proc
        public virtual void ProcessAI(string text)
        {
            aiAPI._TextPrompt(text);
        }

        public virtual void _AIRequestSuccess(string chg)
        {

            chg = chg.Replace("\n", " ");
            _Speak(chg);
            processingComplete.Invoke();
        }

        public virtual void _AIRequestFail(string chg)
        {
            //Do nothing for now with this error
            grokitSpeaker._Speak("I encountered an error please try again later");
        }
    }
}
