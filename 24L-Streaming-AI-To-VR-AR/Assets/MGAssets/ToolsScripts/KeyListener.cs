using UnityEngine;

namespace MGAssets
{

    public class KeyListener : MonoBehaviour
    {
        public bool isActive = true, onlyDebugMode = false;
        public KeyCode[] keyPressed;//, keyPressed2;

        public enum MouseButton { None, LeftMouse, MiddleMouse, RightMouse }
        public MouseButton mousePressed;
        public bool anyMouseButton = false;

        ////////////////////////////////////////// Update - Listening for Key
        void Update()
        {
            //
            if (isActive)
            {
                if (!onlyDebugMode || (onlyDebugMode && Input.GetKey(KeyCode.Delete)))
                {
                    foreach (KeyCode key in keyPressed) if (Input.GetKeyDown(key)) { run(); return; }
                    if (mousePressed != MouseButton.None || anyMouseButton)
                    {
                        if (anyMouseButton && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))) { run(); return; }
                        else
                        if (Input.GetMouseButtonDown(0) && mousePressed == MouseButton.LeftMouse) { run(); return; }
                        else
                        if (Input.GetMouseButtonDown(1) && mousePressed == MouseButton.RightMouse) { run(); return; }
                        else
                        if (Input.GetMouseButtonDown(2) && mousePressed == MouseButton.MiddleMouse) { run(); return; }
                        //{ run(); return; }
                    }
                }
            }
            //
        }
        ////////////////////////////////////////// Update - Listening for Key




        ///////////////////////////////////////////////////////////////////////////////// Run Actions
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
        public bool disableOnComplete = false, dontDisableThis = false;
        //
        //
        void run()
        {
            // Ativa/Desativa Objetos Referenciados
            if (actions.disableObjs.Length > 0) foreach (GameObject obj in actions.disableObjs) if (obj != this.gameObject) obj.SetActive(false); else disableOnComplete = !dontDisableThis;// else { if (!dontDisableThis) disableOnComplete = true; else disableOnComplete = false; print(name + ": disableOnComplete = " + disableOnComplete + " // dontDisableThis = " + dontDisableThis); }//else disableOnComplete = true; 
            if (actions.enableObjs.Length > 0) foreach (GameObject obj in actions.enableObjs) obj.SetActive(true);
            if (actions.toggleObjs.Length > 0) foreach (GameObject obj in actions.toggleObjs) obj.SetActive(!obj.activeSelf);
            //


            // Envia evento de Trigger para a lista de GameObjects
            if (actions.sendTrigger && actions.triggerTargets.Length > 0) foreach (GameObject obj in actions.triggerTargets) obj.SendMessage("Trigger", SendMessageOptions.DontRequireReceiver); //obj.SendMessage("Trigger_Verb", "Trigger", SendMessageOptions.DontRequireReceiver);
                                                                                                                                                                                                 //

            // Send Msg -> Executa um Obj_Custom 
            if (actions.sendMsg)
            {
                //Envia SendMsg para o array de Objetos -> ou Chama no Obj_Custom
                if (actions.sendMsgTargets.Count > 0)
                {
                    foreach (GameObject obj in actions.sendMsgTargets) { obj.SendMessage(actions.stringMsg, actions.argument, SendMessageOptions.DontRequireReceiver); }
                }
            }
            //

            if (disableOnComplete) this.gameObject.SetActive(false);
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////



    }
}
//