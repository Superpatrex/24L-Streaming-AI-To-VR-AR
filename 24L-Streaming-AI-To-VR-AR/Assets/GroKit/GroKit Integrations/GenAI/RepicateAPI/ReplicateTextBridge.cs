using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.Networking;

namespace Core3lb
{
    public class ReplicateTextBridge : BaseAI_Text
    {
        private const string baseUrl = "https://api.replicate.com/v1/predictions";
        public string modelID = "58d078176e02c219e11eb4da5a02a7830a283b14cf8f94537af893ccff5ee781";
        public float apiCheckTime = .2f;
        [TextArea]
        public string stringOutput;


        public bool showDebugs;


        protected override IEnumerator DoTextPromptRequest(string thePrompt)
        {
            startRequest.Invoke("");
            thePrompt = addToPromptStart+ " " + thePrompt;
            string jsonTemplate = $"{{\"version\": \"{modelID}\", \"input\": {{\"prompt\": \"{thePrompt}\"}}}}";
            byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonTemplate);

            using (UnityWebRequest www = new UnityWebRequest(baseUrl, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(postData);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", "Token " + apiKey);

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Request failed: " + www.error);
                    requestFailed.Invoke($"Request failed:{www.error}");
                }
                else
                {
                    PredictionResponse response = JsonUtility.FromJson<PredictionResponse>(www.downloadHandler.text);
                    if (response.status == "starting")
                    {
                        StartCoroutine(RefetchPrediction(response.id));
                    }
                    else
                    {
                        if(showDebugs)
                        {
                            Debug.Log("Prediction result: " + response.output);
                        }
                    }
                }
            }
        }

        IEnumerator RefetchPrediction(string predictionId)
        {
            string fetchUrl = $"{baseUrl}/{predictionId}";
            using (UnityWebRequest www = UnityWebRequest.Get(fetchUrl))
            {
                www.SetRequestHeader("Authorization", "Token " + apiKey);

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Refetch failed: " + www.error);
                }
                else
                {

                    PredictionResponse response = JsonUtility.FromJson<PredictionResponse>(www.downloadHandler.text);
                    if(showDebugs)
                    {
                        Debug.LogError(response.status);
                    }
                    if (response.status == "starting" || response.status == "processing")
                    {
                        // You may want to introduce a delay here before refetching
                        yield return new WaitForSeconds(apiCheckTime);
                        StartCoroutine(RefetchPrediction(response.id));
                    }
                    else
                    {
                        if(showDebugs)
                        {
                            Debug.Log("Prediction result: " + response.output);
                            Debug.Log(www.downloadHandler.text);
                        }
                        //DEBUG END!!!!!
                        string outputResponse = "";
                        foreach (var item in response.output)
                        {
                            outputResponse += item;
                        }
                        if (showDebugs)
                        {
                            Debug.Log("Output Result: " + outputResponse);
                        }
                        requestComplete.Invoke(outputResponse);
                    }
                }
            }
        }

        [System.Serializable]
        private class PredictionResponse
        {
            public string id;
            public string status;
            public string[] output; // Array of strings for the output
        }
    }

}
