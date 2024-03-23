using System.Collections;
using Core3lb.CoreSimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System;

namespace Core3lb
{
    public class ReplicateImageBridge : BaseAI_ImageGen
    {
        private const string baseUrl = "https://api.replicate.com/v1/predictions";
        public string modelID = "ed6d8bee9a278b0d7125872bddfb9dd3fc4c401426ad634d8246a660e387475b"; //DreamShaper Quick
        public string imagePrompt = "Two Cats Looking at the Moon";
        public float apiCheckTime = .2f;
        public Renderer imageOutput;
        [TextArea]
        public string stringOutput;


        protected override IEnumerator DoImagePromptRequest(string thePrompt)
        {
            thePrompt = addToPromptStart + " " + thePrompt;
            PromptSuccess(thePrompt);
            string modelVersion = modelID; // Replace with your desired model version

            string jsonPayload = $"{{\"version\": \"{modelVersion}\", \"input\": {{\"prompt\": \"{thePrompt}\"}}}}";
            byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
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
                    Debug.LogError("Request failed: " + www.ToString());
                    PromptFailed("Request failed: " + www.ToString());
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
                        Debug.Log("Prediction result: " + response.output);
                    }
                }
            }
        }

        public string ModifyJson(string jsonPayload)
        {
            JSONNode rootNode = JSON.Parse(jsonPayload);

            rootNode["version"] = "newVersionValue";
            rootNode["input"]["prompt"] = "newPromptValue";

            return rootNode.ToString();
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
                    PromptFailed("Request failed: " + www.ToString());
                }
                else
                {
                    var parsedData = JSON.Parse(www.downloadHandler.text);
                    var status = parsedData["status"].Value;
                    var id = parsedData["id"].Value;
                    if (status == "starting" || status == "processing")
                    {
                        // You may want to introduce a delay here before refetching
                        var node = JSON.Parse(www.downloadHandler.text);
                        string logs = node["logs"];

                        if(logs != null)
                        {
                            string[] splitLines = logs.Split(new string[] { @"\n" }, StringSplitOptions.None);
                            if(splitLines.Length > 3)
                            {
                                //Debug.LogError(splitLines[splitLines.Length - 2]);
                                stringOutput = splitLines[splitLines.Length - 2];
                            }
                            //foreach (string line in splitLines)
                            //{
                            //    Debug.Log(line);
                            //}
                        }
                        StartCoroutine(RefetchPrediction(id));
                    }
                    else
                    {
                        Debug.Log("Prediction result: " + parsedData["output"][0].Value);
                        StartCoroutine(DownloadAndApplyTexture(parsedData["output"][0].Value));
                        Debug.Log(www.downloadHandler.text);
                        PromptSuccess(parsedData["output"][0].Value);
                    }
                }
            }
        }


        public void UpdateText(string[] response)
        {
            stringOutput = "";
            if (response != null)
            {
                if (response.Length > 0)
                {
                    foreach (var item in response)
                    {
                        stringOutput += item;
                    }
                }
            }
        }

        [System.Serializable]
        private class PredictionResponse
        {
            public string id;
            public string status;
            public string output; // Array of strings for the output
        }
    }

}
