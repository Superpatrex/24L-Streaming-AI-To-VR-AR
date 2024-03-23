using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Core3lb.CoreSimpleJSON;

namespace Core3lb
{
    public class ChatGPT_TextBridge : BaseAI_Text
    {
        [CoreHeader("OpenAI Settings")]
        private const string OPENAI_API_URL = "https://api.openai.com/v1/chat/completions";
        public string systemPrompt = "You are Helpful Assistant";
        [Range(0f, 1f)]
        public float temp = .7f;
        [Range(1, 2048)]
        public int maxTokens = 80;
        public string model = "gpt-3.5-turbo";


        protected override IEnumerator DoTextPromptRequest(string prompt)
        {
            // Prepare the request
            UnityWebRequest www = new UnityWebRequest(OPENAI_API_URL, "POST");

            // Set the request headers
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + apiKey);

            // Construct the request body
            string requestBody = $@"{{
            ""model"": {model},
            ""temperature"": {temp},
            ""max_tokens"": {maxTokens},
            ""messages"": [
                {{
                    ""role"": ""system"",
                    ""content"": ""{systemPrompt}""
                }},
                {{
                    ""role"": ""user"",
                    ""content"": ""{prompt}""
                }}
            ]
        }}";
            //string requestBody = $@"{{
            //    ""model"": ""gpt-3.5-turbo"",
            //    ""messages"": [
            //      {{
            //        ""role"": ""system"",
            //        ""content"": ""{systemPrompt}""
            //      }},
            //      {{
            //        ""role"": ""user"",
            //        ""content"": ""{prompt}""
            //      }}
            //    ]
            //}}";
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);

            // Set download handler
            www.downloadHandler = new DownloadHandlerBuffer();

            // Send the request
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                if (debugResponse)
                {
                    Debug.Log("Request Failure " + www.error + " Response:" + www.downloadHandler.text);
                }
                PromptFailed("Request Failure " + www.error + " Response:" + www.downloadHandler.text);
            }
            else
            {
                // Parse the response
                JSONNode jsonResponse = JSON.Parse(www.downloadHandler.text);
                string assistantResponse = jsonResponse["choices"][0]["message"]["content"];
                PromptSuccess(assistantResponse);
                if (debugResponse)
                {
                    Debug.Log("RESPONSE: --" + assistantResponse);
                }
            }
        }
    }
}