using UnityEngine;
namespace MGAssets
{

    public class ActionEvents : MonoBehaviour
    {
        public enum Receiver { None = 0, All = 1, OnEnable = 2, Trigger = 3, TriggerA = 4, TriggerB = 5, AnimTrigger = 6 }
        [Space] public Receiver modeReceiver = Receiver.All;

        public bool disableOnComplete = false, dontDisableThis = false;
        public bool runOnce = false;




        //////////////////////////////////////////// Receive Event and calls TriggerAction
        void OnEnable()
        {
            if (modeReceiver == Receiver.OnEnable && gameObject.activeInHierarchy && gameObject.activeSelf && this.enabled) TriggerAction();
        }
        public void Trigger() { if (modeReceiver == Receiver.Trigger) TriggerAction(); }
        public void TriggerA() { if (modeReceiver == Receiver.TriggerA) TriggerAction(); }
        public void TriggerB() { if (modeReceiver == Receiver.TriggerB) TriggerAction(); }
        public void animTrigger(AnimationEvent animationEvent) { if (modeReceiver == Receiver.AnimTrigger) TriggerAction(); }
        //
        public void TriggerAction()
        {
            //Return if script component is not enabled or Receiver is set to "None", else Run Actions
            if (!this.enabled || modeReceiver == Receiver.None) return; else run();
        }
        ////////////////////////////////////////////





        ///////////////////////////////////////////////////////////////////////////////// Run Actions
        [ContextMenu("Run()")] void runContext() { run(); }
        [System.Serializable]
        public struct Actions
        {
            public GameObject[] disableObjs, enableObjs, toggleObjs;

            [Space]
            public bool sendTrigger;
            public GameObject[] triggerTargets;

            [Space]
            public bool sendMsg;
            public string stringMsg, argument;
            public System.Collections.Generic.List<GameObject> sendMsgTargets;
        }

        [Space] public Actions actions = new Actions();
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

            if (runOnce) this.enabled = false;
            if (disableOnComplete) this.gameObject.SetActive(false);
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////





    }
}