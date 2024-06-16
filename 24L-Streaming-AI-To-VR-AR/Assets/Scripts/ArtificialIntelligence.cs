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
        private List<ChatMessage> _questions = new List<ChatMessage>();
        private List<ChatMessage> _weatherMsg = new List<ChatMessage>();
        private OpenAIApi _openAI = new OpenAIApi("API_KEY");

        private static string _userInput = "";
        private static string _weather = "You are an artificial intelligence model called vRITA that is given XML data about the weather and answers a user's question about the weather. For instance, if the user asks a question like \"What is the current weather\" or \"what is the weather at this specific location\" you will give them a concise answer like \"The weather is partly cloudy with a temperature of 74 degree Fahrenheit with wind at 13 mph and visibility and precipitation of ...\". Be concise but be sure to explain the weather based on the xml. Do not have extra information in the response but be thorough.";
        private static string _funFact = "You are an artificial intelligence model that only gives fun facts about virtual reality, flight simulators, and artificial intelligence. For instance, when prompted the artificial intelligence should give a fact like \"The precursor to the flight simulator was the \"Tonneau Antoinette\"\". Only produce one have and make it succinct and making it 10-20 words. Do not respond to the user's actual questions or comments only produce facts.";
        private static string _userQuestion = "You are the vRITA artificial intelligence that's sole function is to answer the user's questions. You will be inputted XML data and a user's question. You will return an answer to the user based on the XML data or if the the question does not directly require the XML data then just answer the question. Answers should be in the imperial system and the speed will be in miles per hour.";
        private static string _contexter = "You are an artificial intelligence model that receives strings and must classify the string.  If the string is about changing, taking, or moving the user or the aircraft to a location like a request or a command \"Take me to Epcot\" or \"Change the environment to Epcot\", return \"Change\" and the latitude and longitude like \"13.2 N 34.2 E\" of the location in the question. If there is no direct classification then return \"null\". If the string is a question about anything that does not involve the aircraft, the weather, the environment, changing the environment, or the application then simply return \"Question\". If the string is a question or command involving the weather of a specific place, country, city, or other similar places then return the word \"Weather\" and the latitude and longitude like \"13.2 N 34.2 E\", if the location is the user's current location only return \"Weather\". If the string is about spawning enemies or spawning new aircrafts only return \"Spawn\". If the string is about despawning or destroying the enemy aircrafts only return \"Despawn\". If the string is a question or statement about chaning the scenario or the xml scenario return the word \"xmlChange\" and the 6 letter string scenario code, if none was given return \"null\". Do not return an string besides the one's mention to return";
        private readonly static string _openAIModel = "gpt-4-turbo-preview";

        // Public fields
        public static string returnString = "";
        public static string userInput
        {
            get => _userInput;
            set => _userInput = value;
        }

        public static bool VRUser = false;

        // Public enums


        // Non-public and Non-private fields
        public static UnityEvent m_Weather = new UnityEvent();
        public static UnityEvent m_QuestionEvent = new UnityEvent();
        public static UnityEvent m_FunFactEvent = new UnityEvent();
        public static UnityEvent m_ContexterEvent = new UnityEvent();


        /// <summary>
        /// Called when the script instance is being loaded.
        /// </summary>
        void Start()
        {
            m_Weather.AddListener(SendWeather);
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
        public void SendWeatherButtonHandler(bool VRuser)
        {
            VRUser = VRuser;
            m_Weather.Invoke();
        }

        /// <summary>
        /// Send the user question to the OpenAI API. Used for the button.
        /// </summary>
        public void SendUserQuestionButtonHandler(bool VRuser)
        {
            VRUser = VRuser;
            m_QuestionEvent.Invoke();
        }

        /// <summary>
        /// Send the fun fact to the OpenAI API. Used for the button.
        /// </summary>
        public void SendFunFactButtonHandler(bool VRuser)
        {
            VRUser = VRuser;
            m_FunFactEvent.Invoke();
        }

        public void SendContexterButtonHandler(bool VRuser)
        {
            VRUser = VRuser;
            Debug.Log("Contexter button pressed " + VRUser);
            m_ContexterEvent.Invoke();
        }

        /// <summary>
        /// Send the latitude and longitude to the OpenAI API.
        /// <summary>
        public async void SendWeather()
        {
            this._weatherMsg.Clear();

            var newMessage = new ChatMessage()
            {
                Role = "system",
                Content = _weather
            };

            this._weatherMsg.Add(newMessage);

            newMessage = new ChatMessage()
            {
                Role = "user",
                Content = _userInput
            };

            this._weatherMsg.Add(newMessage);

            try
            {
                var completionResponse = await this._openAI.CreateChatCompletion(new CreateChatCompletionRequest()
                {
                    Model = _openAIModel,
                    Messages = this._weatherMsg,
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

                if (VRUser)
                {
                    Debug.Log("VR User weather response from Artificial Intelligence");
                    Contexter.hasVRUserResponseWeather = true;
                }
                else
                {
                    Debug.Log("Instructor weather response from Artificial Intelligence");
                    Contexter.hasInstructorResponseWeather = true;
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

            this._questions.Add(newMessage);

            newMessage = new ChatMessage()
            {
                Role = "user",
                Content = _userInput
            };

            //Debug.Log("Sending this data: " + _userInput);

            this._questions.Add(newMessage);

            try
            {
                var completionResponse = await this._openAI.CreateChatCompletion(new CreateChatCompletionRequest()
                {
                    Model = _openAIModel,
                    Messages = this._questions,
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

                if (VRUser)
                {
                    Debug.Log("VR User question response from Artificial Intelligence");
                    Contexter.hasVRUserResponseQuestion = true;
                }
                else
                {
                    Debug.Log("Instructor question response from Artificial Intelligence");
                    Contexter.hasInstructorResponseQuestion = true;
                }

                Debug.Log("Answer: " + choiceString);
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

            Debug.Log("Sending context " + VRUser);

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

                if (VRUser)
                {
                    Debug.Log("VR User context response from Artificial Intelligence");
                    Contexter.hasVRUserResponseContext = true;
                }
                else
                {
                    Debug.Log("Instructor context response from Artificial Intelligence");
                    Contexter.hasInstructorResponseContext = true;
                }

                //Debug.LogError("Choice String: " + Contexter.response);
            }
            catch (System.Exception e)
            {
                Debug.Log("Error Bruh: " + e);
            }
        }
    }


}