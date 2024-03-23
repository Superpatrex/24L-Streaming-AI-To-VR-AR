////////////////////////////////////////////////////////////////////////////////////////
//// Moves the GameObject "MoveOBJ" to current position of "toObj" GameObject
////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

public class MoveFuObject : MonoBehaviour
{
    public GameObject moveObj, toObj;

    public bool translate = true, rotate = true;

    [Space]
    public bool isMoving = true;
    public bool hasFinished = false;

    public bool smoothLook = false, clampRotateFactor = false;
    public float translateFactor = 2f, rotateFactor = 10f;

    public Vector3 positionOffSet;

    public bool disableOnComplete = false, runActionOnComplete = false;
    public float translateGap = 0.025f, rotateAngle = 0.05f; // The value -1 ignores smooth and 0 ignores the movement
    public bool snapGap = false;
    float gap = 0, angle = 0;

    //
    void OnEnable() { if (gameObject.activeInHierarchy && gameObject.activeSelf) hasFinished = false; }
    //
    void FixedUpdate()
    {
        if (moveObj == null) { isMoving = false; hasFinished = false; gameObject.SetActive(false); return; }
        if (toObj == null) toObj = this.gameObject;

        if (isMoving)
        {

            //Moves and Rotates the GameObject without smoothing (Instant Teleport)
            if (!smoothLook)
            {
                if (translate) moveObj.transform.position = toObj.transform.position + positionOffSet.x * transform.right + positionOffSet.y * transform.up + positionOffSet.z * transform.forward;
                if (rotate) moveObj.transform.rotation = toObj.transform.rotation;
            }
            else
            {
                //Moves and Rotates the GameObject with smoothing
                if (translate)
                {
                    if (translateFactor == -1) moveObj.transform.position = toObj.transform.position + positionOffSet.x * transform.right + positionOffSet.y * transform.up + positionOffSet.z * transform.forward;
                    else if (translateFactor != 0) moveObj.transform.position = Vector3.Lerp(moveObj.transform.position, toObj.transform.position + positionOffSet.x * transform.right + positionOffSet.y * transform.up + positionOffSet.z * transform.forward, translateFactor * Time.deltaTime);
                }

                if (rotate)
                {
                    if (rotateFactor == -1) moveObj.transform.rotation = toObj.transform.rotation;
                    else if (rotateFactor != 0)
                    {
                        if (clampRotateFactor) moveObj.transform.rotation = Quaternion.Slerp(moveObj.transform.rotation, toObj.transform.rotation, Mathf.Clamp(rotateFactor * Time.deltaTime * 50, 0.2f, 0.8f));
                        else moveObj.transform.rotation = Quaternion.Slerp(moveObj.transform.rotation, toObj.transform.rotation, rotateFactor * Time.deltaTime);
                    }
                }
                //

                //Verifies if the destination position and angle is reached (hasFinished), snaps it and triggers actions
                if (disableOnComplete || runActionOnComplete)
                {
                    if (translateGap >= 0) gap = Vector3.Distance(moveObj.transform.position, transform.position);
                    else gap = 0;

                    if (rotateAngle >= 0) angle = Vector3.Angle(moveObj.transform.rotation * Vector3.forward, transform.rotation * Vector3.forward);
                    else angle = 0;

                    if (!hasFinished && (gap <= translateGap || gap == 0) && (angle <= rotateAngle || angle == 0))
                    {
                        if (snapGap)
                        {
                            if (translateFactor > 0) moveObj.transform.position = toObj.transform.position + positionOffSet.x * transform.right + positionOffSet.y * transform.up + positionOffSet.z * transform.forward;
                            if (rotateFactor > 0) moveObj.transform.rotation = toObj.transform.rotation;
                        }

                        hasFinished = true;
                        if (runActionOnComplete) run(); else if (disableOnComplete) gameObject.SetActive(false);
                    }
                }
                //
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////
    }
    //



    ///////////////////////////////////////////////////////////////////////////////////////////////////////// Run Actions after reaching destination (optional)
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
    //
    void run()
    {
        //Enable-Disable Objects listed on each corresponding arrays
        if (actions.disableObjs.Length > 0) foreach (GameObject obj in actions.disableObjs) if (obj != toObj) obj.SetActive(false); else disableOnComplete = true;
        if (actions.enableObjs.Length > 0) foreach (GameObject obj in actions.enableObjs) obj.SetActive(true);
        if (actions.toggleObjs.Length > 0) foreach (GameObject obj in actions.toggleObjs) obj.SetActive(!obj.activeSelf);
        //


        // Sends a Trigger event to Objects listed on the array "triggerTargets"
        if (actions.sendTrigger && actions.triggerTargets.Length > 0) foreach (GameObject obj in actions.triggerTargets) obj.SendMessage("Trigger", SendMessageOptions.DontRequireReceiver);//obj.SendMessage("Trigger_Verb", "Trigger", SendMessageOptions.DontRequireReceiver);
        //

        // Sends a Custom Message to objs listed on the sendMsgCustom array
        if (actions.sendMsg)
        {
            //Envia SendMsg para o array de Objetos -> ou Chama no Obj_Custom
            if (actions.sendMsgTargets.Count > 0)
            {
                foreach (GameObject obj in actions.sendMsgTargets) { obj.SendMessage(actions.stringMsg, actions.argument, SendMessageOptions.DontRequireReceiver); }
            }
        }
        //

        if (disableOnComplete) toObj.gameObject.SetActive(false);
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
}
