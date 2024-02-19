using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Core3lb.CoreSimpleJSON;

namespace Core3lb
{
    public class DalleImageBridge : BaseAI_ImageGen
    {
        protected override IEnumerator DoImagePromptRequest(string prompt)
        {
            prompt = addToPromptStart + " " + prompt;
            startRequest.Invoke(prompt);
            string url = "https://api.openai.com/v1/images/generations";
            string jsonPayload = $"{{\"model\": \"dall-e-3\", \"prompt\": \"{prompt}\", \"n\": 1, \"size\": \"{width}x{height}\"}}";

            UnityWebRequest webRequest = new UnityWebRequest(url, "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonPayload);
            webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
                PromptFailed("Error: " + webRequest.error);
            }
            else
            {
                var parsedData = JSON.Parse(webRequest.downloadHandler.text);
                textOutput = webRequest.downloadHandler.text;
                if(debugResponse)
                {
                    Debug.LogError("URL IS " + webRequest.downloadHandler.text);
                    Debug.LogError(parsedData["data"][0]["url"]);
                }
                StartCoroutine(DownloadAndApplyTexture(parsedData["data"][0]["url"]));
            }
        }
    }
}
