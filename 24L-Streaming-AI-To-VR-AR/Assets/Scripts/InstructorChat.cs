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
        if (CameraViews.isChatCameraActive && !inputField.isFocused)
        {
            inputField.Select();
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
        {
            ButtonSendHandler();
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