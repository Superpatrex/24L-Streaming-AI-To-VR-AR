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

    /// <summary>
    /// Handles the button click event or just sends the data to the server
    /// </summary>
    /// <param name="code">The six letter string that corresponds to the database</param>
    public void ButtonHandler(string code, bool VRuser)
    {
        Debug.Log(code);
        StartCoroutine(SendDataToMongoDB(code, VRuser));
    }

    /// <summary>
    /// Sends the data to the MongoDB API
    /// </summary>
    /// <param name="data">The six letter string that corresponds to the database</param>
    /// <returns>Sets information to a field of the MonogDBAPI and sets that the Contexter has a response</returns>
    IEnumerator SendDataToMongoDB(string data, bool VRuser)
    {
        string endpoint = "/api/getXML"; // Replace with your specific API endpoint

        // Create a JSON object or format your data as needed
        string jsonBody = "{\"code\":\"" + data + "\"}";

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
                    returnXMLString = JsonUtility.FromJson<JsonXMLString>(request.downloadHandler.text).scenario;
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

                if (VRuser)
                {
                    Contexter.hasVRUserResponseScenario = true;
                }
                else
                {
                    Contexter.hasInstructorResponseScenario = true;
                }
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

class JsonXMLString
{
    public string scenario;
}
