using UnityEngine;
using UnityEngine.UI;

public class UpdateString : MonoBehaviour
{
    public bool isActive = true;
    public bool onEnable = false, update = true;

    public Text toString, fromString;

    public bool useCustomString = false;
    public string customString = "";

    void Awake(){ if (toString == null) toString = GetComponent<Text>(); }


    //////////////////////////// Sets the String Text of a UI element
    void updateString()
    {
        if (toString == null) { print(name + " : null toString!"); return; }

        if (useCustomString) toString.text = customString;
        else if (fromString != null) toString.text = fromString.text;
        else toString.text = "";
    }
    ////////////////////////////

    ////////////////////////////// Calls for String Update - (Once during OnEnable / Constant Update)
    void OnEnable() { if(gameObject.activeInHierarchy && isActive && onEnable) updateString(); }
    void Update()
    {
        if (!isActive || !update) return;
        updateString();
    }
    //////////////////////////////
}
