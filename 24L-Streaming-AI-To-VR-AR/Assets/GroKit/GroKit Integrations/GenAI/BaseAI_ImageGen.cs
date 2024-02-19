using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Core3lb
{
    public class BaseAI_ImageGen : MonoBehaviour
    {
        [SerializeField]
        [CoreEmphasize(true)]
        protected string apiKey = "YOUR_API_KEY";
        public string addToPromptStart = "in rembrandt style ";

        public int width = 1024;
        public int height = 1024;
        ////public TMP_Text outputText;
        //[SerializeField] private Renderer thisRenderer;
        public const string defaultShaderString = "Universal Render Pipeline/Lit";

        [Space(10)]
        [SerializeField]
        [Tooltip("If you want to override the material or else it will use a standard Material")]
        public Material templateMaterial;
        public string testPrompt = "Give me a portrait of a cat";
        public UnityEvent<string> startRequest;
        //Request is Complete
        public UnityEvent<string> requestComplete;
        public UnityEvent<Material> requestMaterial;
        //Request Failed
        public UnityEvent<string> requestFailed;

        [SerializeField]
        protected bool debugResponse;

        [TextArea(2, 5)]
        public string textOutput;


        public virtual void _ImagePrompt(string chg)
        {
            StartCoroutine(DoImagePromptRequest(chg));
        }


        public virtual void _ImagePromptFromInput(TMP_InputField myText)
        {
            _ImagePrompt(myText.text);
        }

        [CoreButton]
        public virtual void _ImagePromptTest()
        {
            StartCoroutine(DoImagePromptRequest(testPrompt));
        }

        protected virtual IEnumerator DoImagePromptRequest(string promptText = "Are you Online?")
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


        protected virtual IEnumerator DownloadAndApplyTexture(string url)
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                    Material material;
                    if (templateMaterial == null)
                    {
                       material = new Material(Shader.Find(defaultShaderString));
                    }
                    else
                    {
                       material = new Material(templateMaterial);
                    }

                    material.mainTexture = texture;
                    // Apply the new material to the cube
                    PromptSuccess(url);
                    requestMaterial.Invoke(material);
                }
                else
                {
                    Debug.LogError("Failed to download texture. Error: " + uwr.error);
                    PromptFailed("Failed to download texture. Error: " + uwr.error);
                }
            }
        }
    }
}
