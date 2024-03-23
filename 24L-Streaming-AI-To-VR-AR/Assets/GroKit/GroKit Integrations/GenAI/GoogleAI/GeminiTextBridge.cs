using UnityEngine;
using Core3lb.CoreSimpleJSON;
using UnityEngine.Networking;
using System.Collections;


namespace Core3lb
{
    public class GeminiTextBridge : BaseAI_Text
    {

        string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

        [CoreHeader("Gemini Settings")]
        [SerializeField] float temperature = 0.9f;
        [SerializeField] int topK = 1;
        [SerializeField] int topP = 1;
        [SerializeField] int maxOutputTokens = 2048;

        //-----------------PROMPT REQUEST-----------------//

        protected override IEnumerator DoTextPromptRequest(string promptText = "Are you Online?")
        {
            //
            promptText = addToPromptStart + " " + promptText;
            startRequest.Invoke(promptText);
            string requestData = "{\"contents\": [{\"parts\": [{\"text\": \"" + promptText + "\"}]}], \"generationConfig\": {\"temperature\": " + temperature + ", \"topK\": " + topK + ", \"topP\": " + topP + ", \"maxOutputTokens\": " + maxOutputTokens + ", \"stopSequences\": []}, \"safetySettings\": [{\"category\": \"HARM_CATEGORY_HARASSMENT\", \"threshold\": \"BLOCK_MEDIUM_AND_ABOVE\"}, {\"category\": \"HARM_CATEGORY_HATE_SPEECH\", \"threshold\": \"BLOCK_MEDIUM_AND_ABOVE\"}, {\"category\": \"HARM_CATEGORY_SEXUALLY_EXPLICIT\", \"threshold\": \"BLOCK_MEDIUM_AND_ABOVE\"}, {\"category\": \"HARM_CATEGORY_DANGEROUS_CONTENT\", \"threshold\": \"BLOCK_MEDIUM_AND_ABOVE\"}]}";

            UnityWebRequest www = new UnityWebRequest(apiUrl + "?key=" + apiKey, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                PromptFailed("Request Failure " + www.error + " Response:" + www.downloadHandler.text);
            }
            else
            {
                string responseText = www.downloadHandler.text;
                //Debug.LogError(responseText);
                JSONNode parsedData = JSONNode.Parse(responseText);
                string outputText = parsedData["candidates"][0]["content"]["parts"][0]["text"];
                if (debugResponse)
                {
                    Debug.LogError(promptText + " : " + responseText);
                }
                textOutput = outputText;
                PromptSuccess(outputText);
            }
        }
    }
}

