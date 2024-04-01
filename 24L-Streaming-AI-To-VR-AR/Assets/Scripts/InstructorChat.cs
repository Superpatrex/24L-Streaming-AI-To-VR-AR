using UnityEngine;
using System.Text;
using TMPro;

public class InstructorChat : MonoBehaviour
{
    private StringBuilder chatLog = new StringBuilder();
    [SerializeField] public TMP_Text chatText;
    [SerializeField] public TMP_Text chatInput;
    [SerializeField] public Contexter contexter;

    public void AddvRITAMessage(string message)
    {
        chatLog.Append("vRTIA: ");
        chatLog.Append(message);
        chatLog.AppendLine("\n");
    }

    public void AddInstructorMessage(string message)
    {
        chatLog.Append("Instructor: ");
        chatLog.Append(message);
        chatLog.AppendLine("\n");
    }

    public void Start()
    {

    }

    public void Update()
    {
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
        contexter.ActOnContextInstructorUser();
        chatText.text = chatLog.ToString();
    }
}