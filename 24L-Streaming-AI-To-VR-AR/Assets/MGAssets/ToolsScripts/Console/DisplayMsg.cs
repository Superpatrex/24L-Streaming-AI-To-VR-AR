using UnityEngine;
using UnityEngine.UI;


public class DisplayMsg : MonoBehaviour
{
    public static DisplayMsg current;
    public Text msgTxt;

    [Space()]
    [Tooltip("This display will not receive MSGs sent by 'showAll' and will be handled directly by reference.")]
    public bool isolateInstance = false;

    string lastTimedMsg;

    //
    void OnEnable() { if(gameObject.activeInHierarchy && !isolateInstance) current = this; }
    //
    void displayMsg(string msg = "", float timed = 0)
    {
        if (msg == "") { if (lastTimedMsg != "") CancelInvoke("timedClearMsg"); lastTimedMsg = ""; msgTxt.text = ""; return; }          //Refresh
        msgTxt.text = msg; msgTxt.gameObject.SetActive(true);                                                                           //Sets Msg
        if (timed > 0) { if (lastTimedMsg != "") CancelInvoke("timedClearMsg"); lastTimedMsg = msg; Invoke("timedClearMsg", timed); }   //Calls Timer
    }
    void timedClearMsg() { if (lastTimedMsg == msgTxt.text) { lastTimedMsg = ""; msgTxt.text = ""; } return; }                          //Clear msg after timer
                                                                                                                                        //

    //Static and Public Calls
    public void displayMsg(string msg = "") { displayMsg(msg, 0); }
    public void displayQuickMsg(string msg = "") {displayMsg(msg, 5); }
    public void showQuick(string msg = "") { displayMsg(msg, 2); }
    public static void ShowQuick(string msg = "") { if (current != null) current.displayMsg(msg, 2); }
    public static void show(string msg = "", float timed = 0) { if(current != null) current.displayMsg(msg, timed); }
    public static void showAll(string msg = "", float timed = 0)
    {
        //foreach (DisplayMsg displayMsg in FindObjectsOfType<DisplayMsg>()) displayMsg.displayMsg(msg, timed);     //Updates Only Active Displays
        foreach (DisplayMsg displayMsg in Resources.FindObjectsOfTypeAll<DisplayMsg>())                             //Updates All Displays including disabled ones
        {
            if(!displayMsg.isolateInstance) displayMsg.displayMsg(msg, timed);
        }  
    }
    //
}

