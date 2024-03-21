using UnityEngine;

public class DisableEvent : MonoBehaviour
{
    public bool isActive = true;

    //
    [System.Serializable]
    public struct Actions
    {
        public GameObject[] disableObjs, enableObjs, toggleObjs;

        public bool sendTrigger;
        public GameObject[] triggerTargets;

        public bool sendMsg;
        public string stringMsg, argument;
        public System.Collections.Generic.List<GameObject> sendMsgTargets;
    }
    public Actions actions = new Actions();
    //

    ////////////////////// Run Actions during OnDisable
    void OnDisable()
    {
        if (!isActive) return;

        // Enable/Disable/Toogle Objects 
        if (actions.disableObjs.Length > 0) foreach (GameObject obj in actions.disableObjs) if (obj != this.gameObject) obj.SetActive(false);
        if (actions.enableObjs.Length > 0) foreach (GameObject obj in actions.enableObjs) obj.SetActive(true);
        if (actions.toggleObjs.Length > 0) foreach (GameObject obj in actions.toggleObjs) obj.SetActive(!obj.activeSelf);
        //


        // Sends Trigger Event to GameObjects on the List
        if (actions.sendTrigger && actions.triggerTargets.Length > 0) foreach (GameObject obj in actions.triggerTargets) obj.SendMessage("Trigger", SendMessageOptions.DontRequireReceiver); //obj.SendMessage("Trigger_Verb", "Trigger", SendMessageOptions.DontRequireReceiver);
        //

        // Send Msg to GameObjects
        if (actions.sendMsg)
        {
            if (actions.sendMsgTargets.Count > 0)
            {
                foreach (GameObject obj in actions.sendMsgTargets) { obj.SendMessage(actions.stringMsg, actions.argument, SendMessageOptions.DontRequireReceiver); }
            }
        }
        //
    }
    ////////////////////// Run Actions during OnDisable

}
