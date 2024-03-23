using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Core3lb
{
    public class BaseAI_Text : MonoBehaviour
    {

        [SerializeField]
        [CoreEmphasize(true)]
        protected string apiKey = "YOUR_API_KEY";
        public string addToPromptStart = "Be Brief";
        //public TMP_Text outputText;

        [Space(10)]
        [SerializeField]
        protected string testPrompt = "Give me 3 facts about cats";
        public UnityEvent<string> startRequest;
        //Request is Complete
        public UnityEvent<string> requestComplete;
        //Request Failed
        public UnityEvent<string> requestFailed;

        [SerializeField]
        protected bool debugResponse;

        [TextArea(2, 5)]
        public string textOutput;

        public virtual void Start()
        {
            if(apiKey == "YOUR_API_KEY")
            {
                Debug.LogError("API KEY IS NOT SET ON ", gameObject);
            }
        }


        public virtual void _TextPrompt(string chg)
        {
            StartCoroutine(DoTextPromptRequest(chg));
        }


        public virtual void _TextPromptFromInput(TMP_InputField myText)
        {
            _TextPrompt(myText.text);
        }

        [CoreButton]
        public virtual void _TextPromptTest()
        {
            StartCoroutine(DoTextPromptRequest(testPrompt));
        }

        protected virtual IEnumerator DoTextPromptRequest(string promptText = "Are you Online?")
        {
            throw new NotImplementedException();
        }

        protected virtual void PromptSuccess(string text)
        {
            requestComplete.Invoke(text);
        }

        protected virtual void PromptFailed(string whyFail)
        {
            Debug.LogError("Error: " + whyFail);
            //OutputText("Failure");
            requestFailed.Invoke(whyFail);
        }

    }
}
