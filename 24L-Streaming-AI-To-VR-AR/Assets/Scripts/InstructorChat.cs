using UnityEngine;
using System.Text;
using TMPro;

public class InstructorChat : MonoBehaviour
{
    private StringBuilder chatLog = new StringBuilder();
    [SerializeField] public TMP_Text chatText;
    [SerializeField] public TMP_Text chatInput;

    public void AddUserMessage(string message)
    {
        chatLog.Append("User: ");
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
    }

    public void ButtonSendHandler()
    {
        string message = chatInput.text;
        chatInput.text = "";
        AddUserMessage(message);
        chatText.text = chatLog.ToString();
    }
}