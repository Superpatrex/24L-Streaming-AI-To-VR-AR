using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System;

public class MongoDBAPI: MonoBehaviour
{

    // Public Fields
    public static string returnXMLString = "";
    public static XMLShipStructure shipScenario = null;

    // Private Fields
    private string apiUrl = "https://vrita-server-02f7dd943082.herokuapp.com"; // Replace with your API endpoint

    public void ButtonHandler(string code)
    {
        Debug.Log(code);
        StartCoroutine(SendDataToMongoDB(code));
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

                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    returnXMLString = request.downloadHandler.text;
                }
                else
                {
                    Debug.LogError("No data received from the server, code may be wrong");
                }
                
            }
        }

        if (!string.IsNullOrEmpty(returnXMLString))
        {
            try
            {
                shipScenario = XMLSerializer.ReadFromXmlStringShipInformation(returnXMLString);
                Contexter.hasResponseScenario = true;
            }
            catch
            {
                Debug.LogError("Error reading XML data");
            }
        }
        else
        {
            Debug.LogError("No data received from the server");
        }
    }
}
