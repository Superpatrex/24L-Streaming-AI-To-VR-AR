using UnityEngine;
using System.Text;
using TMPro;
using UnityEngine.UI;

public class InstructorChat : MonoBehaviour
{
    private StringBuilder chatLog = new StringBuilder();
    [SerializeField] public TMP_Text chatText;
    [SerializeField] public TMP_Text chatInput;
    [SerializeField] public TMP_InputField inputField;
    [SerializeField] public Contexter contexter;

    public static InstructorChat Instance { get; private set; }

    public void AddvRITAMessage(string message)
    {
        chatLog.Append("vRTIA: ");
        chatLog.Append(message);
        chatLog.AppendLine("\n");
        chatText.text = chatLog.ToString();
    }

    public void AddInstructorMessage(string message)
    {
        chatLog.Append("Instructor: ");
        chatLog.Append(message);
        chatLog.AppendLine("\n");
        chatText.text = chatLog.ToString();
    }

    public void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Start()
    {
        
    }

    public void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.A) || UnityEngine.Input.GetKeyDown(KeyCode.B) || UnityEngine.Input.GetKeyDown(KeyCode.C) || UnityEngine.Input.GetKeyDown(KeyCode.D) || UnityEngine.Input.GetKeyDown(KeyCode.E) || UnityEngine.Input.GetKeyDown(KeyCode.F) || UnityEngine.Input.GetKeyDown(KeyCode.G) || UnityEngine.Input.GetKeyDown(KeyCode.H) || UnityEngine.Input.GetKeyDown(KeyCode.I) || UnityEngine.Input.GetKeyDown(KeyCode.J) || UnityEngine.Input.GetKeyDown(KeyCode.K) || UnityEngine.Input.GetKeyDown(KeyCode.L) || UnityEngine.Input.GetKeyDown(KeyCode.M) || UnityEngine.Input.GetKeyDown(KeyCode.N) || UnityEngine.Input.GetKeyDown(KeyCode.O) || UnityEngine.Input.GetKeyDown(KeyCode.P) || UnityEngine.Input.GetKeyDown(KeyCode.Q) || UnityEngine.Input.GetKeyDown(KeyCode.R) || UnityEngine.Input.GetKeyDown(KeyCode.S) || UnityEngine.Input.GetKeyDown(KeyCode.T) || UnityEngine.Input.GetKeyDown(KeyCode.U) || UnityEngine.Input.GetKeyDown(KeyCode.V) || UnityEngine.Input.GetKeyDown(KeyCode.W) || UnityEngine.Input.GetKeyDown(KeyCode.X) || UnityEngine.Input.GetKeyDown(KeyCode.Y) || UnityEngine.Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("Key pressed: " + UnityEngine.Input.inputString);
            inputField.text += UnityEngine.Input.inputString;
            return;
        }
        else if (UnityEngine.Input.GetKeyDown(KeyCode.Tab) || UnityEngine.Input.GetKeyDown(KeyCode.Space) || UnityEngine.Input.GetKeyDown(KeyCode.Period) || UnityEngine.Input.GetKeyDown(KeyCode.Comma) || UnityEngine.Input.GetKeyDown(KeyCode.Semicolon) || UnityEngine.Input.GetKeyDown(KeyCode.Colon) || UnityEngine.Input.GetKeyDown(KeyCode.Question) || UnityEngine.Input.GetKeyDown(KeyCode.Exclaim) || UnityEngine.Input.GetKeyDown(KeyCode.Quote) || UnityEngine.Input.GetKeyDown(KeyCode.DoubleQuote) || UnityEngine.Input.GetKeyDown(KeyCode.Minus) || UnityEngine.Input.GetKeyDown(KeyCode.Plus) || UnityEngine.Input.GetKeyDown(KeyCode.Equals) || UnityEngine.Input.GetKeyDown(KeyCode.Asterisk) || UnityEngine.Input.GetKeyDown(KeyCode.Slash) || UnityEngine.Input.GetKeyDown(KeyCode.Backslash) || UnityEngine.Input.GetKeyDown(KeyCode.LeftParen) || UnityEngine.Input.GetKeyDown(KeyCode.RightParen) || UnityEngine.Input.GetKeyDown(KeyCode.LeftCurlyBracket) || UnityEngine.Input.GetKeyDown(KeyCode.RightCurlyBracket) || UnityEngine.Input.GetKeyDown(KeyCode.Less) || UnityEngine.Input.GetKeyDown(KeyCode.Greater) || UnityEngine.Input.GetKeyDown(KeyCode.Pipe) || UnityEngine.Input.GetKeyDown(KeyCode.Ampersand) || UnityEngine.Input.GetKeyDown(KeyCode.Caret) || UnityEngine.Input.GetKeyDown(KeyCode.Percent) || UnityEngine.Input.GetKeyDown(KeyCode.Dollar) || UnityEngine.Input.GetKeyDown(KeyCode.At) || UnityEngine.Input.GetKeyDown(KeyCode.Tilde))
        {
            Debug.Log("Key pressed: " + UnityEngine.Input.inputString);
            inputField.text += UnityEngine.Input.inputString;
            return;
        }
        else if (
            UnityEngine.Input.GetKeyDown(KeyCode.Alpha0) ||
            UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) ||
            UnityEngine.Input.GetKeyDown(KeyCode.Alpha2) ||
            UnityEngine.Input.GetKeyDown(KeyCode.Alpha3) ||
            UnityEngine.Input.GetKeyDown(KeyCode.Alpha4) ||
            UnityEngine.Input.GetKeyDown(KeyCode.Alpha5) ||
            UnityEngine.Input.GetKeyDown(KeyCode.Alpha6) ||
            UnityEngine.Input.GetKeyDown(KeyCode.Alpha7) ||
            UnityEngine.Input.GetKeyDown(KeyCode.Alpha8) ||
            UnityEngine.Input.GetKeyDown(KeyCode.Alpha9))
        {
            Debug.Log("Key pressed: " + UnityEngine.Input.inputString);
            inputField.text += UnityEngine.Input.inputString;
            return;
        }
        else if (UnityEngine.Input.GetKeyDown(KeyCode.Backspace))
        {
            Debug.Log("Backspace pressed");
            if (inputField.text.Length > 0)
            {
                inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
            }
            return;
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
        {
            ButtonSendHandler();
            inputField.text = "";
        }

        if (Contexter.hasInstructorLocationChange)
        {
            Contexter.hasInstructorLocationChange = false;
            AddvRITAMessage("I have changed the location of the instructor.");
            chatText.text = chatLog.ToString();
        }
    }

    public void ButtonSendHandler()
    {
        string message = chatInput.text;
        chatInput.text = "";
        AddInstructorMessage(message);
        Contexter.userInput = message;
        contexter.SendContextInputStringToAI(false);
        chatText.text = chatLog.ToString();
    }
}