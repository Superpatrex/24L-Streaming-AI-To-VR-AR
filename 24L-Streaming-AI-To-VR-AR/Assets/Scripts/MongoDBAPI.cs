using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System;

public class MongoDBAPI: MonoBehaviour
{

    // Public Fields
    [SerializeField] public TMP_Text changeText; // Replace with your UI text object
    [SerializeField] public TMP_Text inputField; // Replace with your UI input field object 

    // Private Fields
    private string apiUrl = "http://www.vrita.com"; // Replace with your API endpoint

    public void ButtonHandler()
    {
        Debug.Log(inputField.text);
        string temp = inputField.text;
        StartCoroutine(SendDataToMongoDB(temp));
    }

    IEnumerator SendDataToMongoDB(string data)
    {
        string endpoint = "/api/getXML"; // Replace with your specific API endpoint

        // Create a JSON object or format your data as needed
        string jsonBody = "{\"code\": \"" + data + "\"}";

        Debug.Log(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl + endpoint, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log("Data sent successfully!");
                // Handle response if needed
                changeText.text = request.downloadHandler.text;
            }
        }
    }
}
