using UnityEngine;

namespace Core3lb
{
    public class TMP_AI_TextDisplay : DisplayTMPBase
    {
        [CoreHeader("-AI SETTINGS-")]
        [SerializeField]
        [CoreRequired]
        BaseAI_Text aiToProcessFrom;
        [SerializeField]
        string processSuccessPre = "Assistant:";
        [SerializeField]
        bool showFailedTextOnly;
        [SerializeField]
        string processFailedText = "Sorry there was an error with your request";


        public void Start()
        {
            aiToProcessFrom.requestComplete.AddListener(_AIRequestSuccess);
            aiToProcessFrom.requestFailed.AddListener(_AIRequestFail);
        }

        private void _AIRequestFail(string text)
        {
            if(showFailedTextOnly)
            {
                _SetText(processFailedText);
            }
            else
            _SetText(processFailedText + text);
        }

        private void _AIRequestSuccess(string text)
        {
            _SetText(processSuccessPre + text);
        }
    }
}
