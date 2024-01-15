using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;
using TMPro;

namespace OpenAI
{
    public class ArtificialIntelligence : MonoBehaviour
    {

        // Private fields
        private List<ChatMessage> _msg = new List<ChatMessage>();
        private OpenAIApi _openAI = new OpenAIApi("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");

        private static string _userInput = "";
        private static string _latLongString = "Give me the latitude of longitude in decimals of the location to which is specified. For instance, if the user were to say \"Epcot\" or \"Take me to Epcot\"  or \"Where is Epcot\" return \"28.3765 N, 81.5494 W\". Only return the latitude and longitude.";
        private static string _userQuestion = "";
        private static string _openAIModel = "gpt-4-1106-preview";

        // Public fields
        [SerializeField] public TMP_Text inputField;
        [SerializeField] public TMP_Text outputField;

        // Non-public and Non-private fields
        static UnityEvent m_LatEvemt = new UnityEvent();
        static UnityEvent m_QuestionEvent = new UnityEvent();


        /// <summary>
        /// Called when the script instance is being loaded.
        /// </summary>
        void Start()
        {
            m_LatEvemt.AddListener(SendLatLong);
            m_QuestionEvent.AddListener(SendUserQuestion);
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        void Update()
        {

        }

        public void SendLatLongButtonHandler()
        {
            m_LatEvemt.Invoke();
        }

        public void SendUserQuestionButtonHandler()
        {
            m_QuestionEvent.Invoke();
        }

        /// <summary>
        /// Send the latitude and longitude to the OpenAI API.
        /// <summary>
        private async void SendLatLong()
        {
            _userInput = inputField.text;

            var newMessage = new ChatMessage()
            {
                Role = "system",
                Content = _latLongString
            };

            this._msg.Add(newMessage);

            newMessage = new ChatMessage()
            {
                Role = "user",
                Content = _userInput
            };

            this._msg.Add(newMessage);

            try
            {
                var completionResponse = await this._openAI.CreateChatCompletion(new CreateChatCompletionRequest()
                {
                    Model = _openAIModel,
                    Messages = this._msg,
                    MaxTokens = 100,
                    Temperature = 1f
                });

                if (completionResponse.Choices == null)
                {
                    Debug.Log("Error: No response from OpenAI API, choices");
                }
                else if (completionResponse.Choices.Count == 0)
                {
                    Debug.Log("Error: No response from OpenAI API, choices count");
                }
            
                string choiceString = completionResponse.Choices[0].Message.Content.Trim();

                if (choiceString == "null")
                {
                    Debug.Log("Error: Not a valid question to the location");
                    outputField.text = "Not a valid destination"; 
                }
                else
                {
                    outputField.text = choiceString;
                }

            }
            catch (System.Exception e)
            {
                Debug.Log("Error: " + e);
            }
        }

        /// <summary>
        /// Send the user question to the OpenAI API.
        /// </summary>
        private async void SendUserQuestion()
        {
            var newMessage = new ChatMessage()
            {
                Role = "system",
                Content = _userQuestion
            };

            this._msg.Add(newMessage);

            newMessage = new ChatMessage()
            {
                Role = "user",
                Content = _userQuestion
            };

            this._msg.Add(newMessage);

            try
            {
                var completionResponse = await this._openAI.CreateChatCompletion(new CreateChatCompletionRequest()
                {
                    Model = _openAIModel,
                    Messages = this._msg,
                    MaxTokens = 100,
                    Temperature = 1f
                });

                if (completionResponse.Choices == null)
                {
                    Debug.Log("Error: No response from OpenAI API, choices");
                }
                else if (completionResponse.Choices.Count == 0)
                {
                    Debug.Log("Error: No response from OpenAI API, choices count");
                }
            
                outputField.text = completionResponse.Choices[0].Message.Content.Trim();
            }
            catch (System.Exception e)
            {
                Debug.Log("Error: " + e);
            }
        }
    }


}