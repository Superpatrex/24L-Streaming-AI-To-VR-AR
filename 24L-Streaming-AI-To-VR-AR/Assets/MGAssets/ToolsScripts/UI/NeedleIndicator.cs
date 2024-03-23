using UnityEngine;
using UnityEngine.UI;


public class NeedleIndicator : MonoBehaviour
{
    public bool isActive = true;

    public RectTransform needle;
    public float value, valueOffSet = 0, factor = 1, maxValue = 1, minValue = -1;

    public float currentAngle, angleOffSet = 0;
    public bool clampAngle = true;
    public float maxAngle = 90, minAngle = -90;

    public Text valueTxt;

    /////// Use this only for Debug/Tweaking values in real time during playmode and disable afterwards
    /*
    [Space(15)]
    public bool debugUpdate = false;
    void Update() { if (debugUpdate) setValue(value); else return; }
    //*/
    ///////


    //Update to Inicial Value
    void OnEnable() { if (isActive && gameObject.activeInHierarchy) setValue(value); }
    //

    //External call to set value
    public void setValue(float newValue)
    {
        if (!isActive) return;

        //Update Value
        value = newValue;
        if(!clampAngle) currentAngle = angleOffSet + factor * value * (maxAngle - minAngle) / (maxValue - minValue) + valueOffSet;
        else currentAngle = Mathf.Clamp(angleOffSet + factor * value * (maxAngle - minAngle) / (maxValue - minValue) + valueOffSet, minAngle, maxAngle);
        //


        //Set needle angle
        if (needle != null) needle.localRotation = Quaternion.Euler( needle.localRotation.eulerAngles.x, needle.localRotation.eulerAngles.y, currentAngle);

        //Set Text value
        if (valueTxt != null) { valueTxt.text = value.ToString("000"); }
    }
    //
}
