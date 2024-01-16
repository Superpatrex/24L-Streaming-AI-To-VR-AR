using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;
using TMPro;

namespace OpenAI
{
    /// <summary>
    /// The artificial intelligence class that will be used to communicate with the OpenAI API.
    /// </summary>
    /// /// <seealso cref="cref="MonoBehavior"/> 
    public class ArtificialIntelligence : MonoBehaviour
    {

        // Private fields
        private List<ChatMessage> _msg = new List<ChatMessage>();
        private OpenAIApi _openAI = new OpenAIApi("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");

        private static string _userInput = "";
        private static string _latLongString = "Give me the latitude of longitude in decimals of the location to which is specified. For instance, if the user were to say \"Epcot\" or \"Take me to Epcot\"  or \"Where is Epcot\" return \"28.3765 N, 81.5494 W\". Only return the latitude and longitude.";
        private static string _funFact = "You are an artificial intelligence model that only gives fun facts about virtual reality, flight simulators, and artificial intelligence. For instance, when prompted the artificial intelligence should give a fact like \"The precursor to the flight simulator was the \"Tonneau Antoinette\"\". Only produce one have and make it succinct and making it 10-20 words. Do not respond to the user's actual questions or comments only produce facts.";
        private static string _userQuestion = "You are the vRITA artificial intelligence that's sole function is to answer the user's questions. You will be inputted XML data and a user's question. You will return an answer to the user based on the XML data or if the the question does not directly require the XML data then just answer the question. Answers should be in the metric system.";
        private static string _openAIModel = "gpt-4-1106-preview";

        // Public fields
        [SerializeField] public TMP_Text inputFunFactField;
        [SerializeField] public TMP_Text outputFunFactField;
        [SerializeField] public TMP_Text inputUserQuestionField;
        [SerializeField] public TMP_Text outputUserQuestionField;
        [SerializeField] public TMP_Text inputLatLongField;
        [SerializeField] public TMP_Text outputLatLongField;

        // Non-public and Non-private fields
        static UnityEvent m_LatEvemt = new UnityEvent();
        static UnityEvent m_QuestionEvent = new UnityEvent();
        static UnityEvent m_FunFactEvent = new UnityEvent();


        /// <summary>
        /// Called when the script instance is being loaded.
        /// </summary>
        void Start()
        {
            m_LatEvemt.AddListener(SendLatLong);
            m_QuestionEvent.AddListener(SendUserQuestion);
            m_FunFactEvent.AddListener(SendFunFact);
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        void Update()
        {

        }

        /// <summary>
        /// Send the latitude and longitude to the OpenAI API. Used for the button.
        /// </summary>
        public void SendLatLongButtonHandler()
        {
            m_LatEvemt.Invoke();
        }

        /// <summary>
        /// Send the user question to the OpenAI API. Used for the button.
        /// </summary>
        public void SendUserQuestionButtonHandler()
        {
            m_QuestionEvent.Invoke();
        }

        /// <summary>
        /// Send the fun fact to the OpenAI API. Used for the button.
        /// </summary>
        public void SendFunFactButtonHandler()
        {
            m_FunFactEvent.Invoke();
        }

        /// <summary>
        /// Send the latitude and longitude to the OpenAI API.
        /// <summary>
        private async void SendLatLong()
        {
            _userInput = inputLatLongField.text;

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
                    outputLatLongField.text = "Not a valid destination"; 
                }
                else
                {
                    outputLatLongField.text = choiceString;
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
            _userInput = inputUserQuestionField.text;

            var newMessage = new ChatMessage()
            {
                Role = "system",
                Content = _userQuestion
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
            
                outputUserQuestionField.text = completionResponse.Choices[0].Message.Content.Trim();
            }
            catch (System.Exception e)
            {
                Debug.Log("Error: " + e);
            }
        }

        /// <summary>
        /// Send the fun fact to the OpenAI API.
        /// </summary>
        private async void SendFunFact()
        {
            _userInput = inputFunFactField.text;

            var newMessage = new ChatMessage()
            {
                Role = "system",
                Content = _funFact
            };

            this._msg.Add(newMessage);

            newMessage = new ChatMessage()
            {
                Role = "user",
                Content = "Fact time"
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
            
                outputFunFactField.text = completionResponse.Choices[0].Message.Content.Trim();
            }
            catch (System.Exception e)
            {
                Debug.Log("Error: " + e);
            }
        }
    }


}