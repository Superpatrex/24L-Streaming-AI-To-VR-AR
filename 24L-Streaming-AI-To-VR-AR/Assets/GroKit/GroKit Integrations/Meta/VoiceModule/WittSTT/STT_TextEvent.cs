
using UnityEngine.Events;

namespace Core3lb
{
    public class STT_TextEvent : DisplayTMPBase
    {
        public MetaWittSTT voiceSystem;
        public string keyWord;
        public string extraText;

        public UnityEvent onKeyWordHeard;

        public override void Awake()
        {
            if (voiceSystem)
            {
                voiceSystem.onTranscriptComplete.AddListener(ProcessText);
            }
            base.Awake();
        }

        public void ProcessText(string text)
        {
            _SetText(text);
        }
    }
}
