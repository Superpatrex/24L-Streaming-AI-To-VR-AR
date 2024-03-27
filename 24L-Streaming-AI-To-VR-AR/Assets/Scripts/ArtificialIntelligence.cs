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
        private OpenAIApi _openAI = new OpenAIApi("API_KEY");

        private static string _userInput = "";
        private static string _latLongString = "Give me the latitude of longitude in decimals of the location to which is specified. For instance, if the user were to say \"Epcot\" or \"Take me to Epcot\"  or \"Where is Epcot\" return \"28.3765 N, 81.5494 W\". Only return the latitude and longitude and the name of the location after the latitude and longitude separated with a space.";
        private static string _funFact = "You are an artificial intelligence model that only gives fun facts about virtual reality, flight simulators, and artificial intelligence. For instance, when prompted the artificial intelligence should give a fact like \"The precursor to the flight simulator was the \"Tonneau Antoinette\"\". Only produce one have and make it succinct and making it 10-20 words. Do not respond to the user's actual questions or comments only produce facts.";
        private static string _userQuestion = "You are the vRITA artificial intelligence that's sole function is to answer the user's questions. You will be inputted XML data and a user's question. You will return an answer to the user based on the XML data or if the the question does not directly require the XML data then just answer the question. Answers should be in the metric system.";
        private static string _contexter = "You are an artificial intelligence model that receives strings and must classify the string.  If the string is about changing, taking, or moving the user or the aircraft to a location like a request or a command \"Take me to Epcot\" or \"Change the environment to Epcot\", return \"Change\" and the latitude and longitude like \"13.2 N 34.2 E\" of the location in the question. If there is no direct classification then return \"null\". If the string is a question about anything that does not involve the aircraft, the environment, changing the environment, or the application then simply return \"Question\". If the string is a question or command involving the weather of a specific place, country, city, or other similar places then return the word \"Weather\" and the latitude and longitude like \"13.2 N 34.2 E\". If the string is a question or statement about chaning the scenario or the xml scenario return the word \"xmlChange\" and the 6 letter string scenario code, if none was given return \"null\". Do not return an string besides the one's mention to return";
        private readonly static string _openAIModel = "gpt-4-turbo-preview";

        // Public fields
        //[SerializeField] public TMP_Text inputFunFactField;
        //[SerializeField] public TMP_Text outputFunFactField;
        //[SerializeField] public TMP_Text inputUserQuestionField;
        //[SerializeField] public TMP_Text outputUserQuestionField;
        //[SerializeField] public TMP_Text inputLatLongField;
        //[SerializeField] public TMP_Text outputLatLongField;

        public static string returnString = "";
        [SerializeField] public static AIReturnType returnType = AIReturnType.RETURN_TEXT_BOX;
        public static string userInput
        {
            get => _userInput;
            set => _userInput = value;
        }

        // Public enums

        public enum AIReturnType
        {
            RETURN_STRING,
            RETURN_TEXT_BOX
        }

        // Non-public and Non-private fields
        public static UnityEvent m_LatEvemt = new UnityEvent();
        public static UnityEvent m_QuestionEvent = new UnityEvent();
        public static UnityEvent m_FunFactEvent = new UnityEvent();
        public static UnityEvent m_ContexterEvent = new UnityEvent();


        /// <summary>
        /// Called when the script instance is being loaded.
        /// </summary>
        void Start()
        {
            m_LatEvemt.AddListener(SendLatLong);
            m_QuestionEvent.AddListener(SendUserQuestion);
            m_FunFactEvent.AddListener(SendFunFact);
            m_ContexterEvent.AddListener(SendContexter);
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

        public void SendContexterButtonHandler()
        {
            m_ContexterEvent.Invoke();
        }

        /// <summary>
        /// Send the latitude and longitude to the OpenAI API.
        /// <summary>
        public async void SendLatLong()
        {
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

                    if (returnType == AIReturnType.RETURN_STRING)
                    {
                        returnString = "Not a valid destination";
                    }
                }
                else
                {
                    if (returnType == AIReturnType.RETURN_STRING)
                    {
                        returnString = choiceString;
                    }
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
        public async void SendUserQuestion()
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

                if (returnType == AIReturnType.RETURN_STRING)
                {
                    returnString = choiceString;
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("Error: " + e);
            }
        }

        /// <summary>
        /// Send the fun fact to the OpenAI API.
        /// </summary>
        public async void SendFunFact()
        {
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
            
                string choiceString = completionResponse.Choices[0].Message.Content.Trim();

                Contexter.response = choiceString;
                Contexter.hasResponseQuestion = true;
            }
            catch (System.Exception e)
            {
                Debug.Log("Error: " + e);
            }
        }

        /// <summary>
        /// Returns the contexter string being what the string is about.
        /// </summary>
        public async void SendContexter()
        {
            var newMessage = new ChatMessage()
            {
                Role = "system",
                Content = _contexter
            };

            this._msg.Add(newMessage);

            newMessage = new ChatMessage()
            {
                Role = "user",
                Content = _userInput
            };

            this._msg.Add(newMessage);

            //Debug.Log("Sending context");

            try
            {
                var completionResponse = await this._openAI.CreateChatCompletion(new CreateChatCompletionRequest()
                {
                    Model = _openAIModel,
                    Messages = this._msg,
                    MaxTokens = 500,
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
                Contexter.response = choiceString;
                Contexter.hasResponseContext = true;

                //Debug.LogError("Choice String: " + Contexter.response);
            }
            catch (System.Exception e)
            {
                Debug.Log("Error: " + e);
            }
        }
    }


}