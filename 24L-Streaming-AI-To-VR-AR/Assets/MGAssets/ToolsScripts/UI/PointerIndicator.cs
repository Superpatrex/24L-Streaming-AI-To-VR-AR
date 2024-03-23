using UnityEngine;
using UnityEngine.UI;

public class PointerIndicator : MonoBehaviour
{
    public bool isActive = true;

    public RectTransform pointer;
    public Text valueTxt;

    [Space]
    public float rawValue;
    public float value;
    public float currentAngle;


    [Space]
    [Header("Factors and OffSets")]
    public float factorValue = 1;
    public float valueOffSet = 0;

    [Space]
    [Tooltip("Negative for clockwise direction")] public float factorAngle = -1;
    public float angleOffSet = 0;


    [Space]
    [Header("Alignment Values/Angles")]
    public float maxValue = 1;
    public float minValue = 0;

    [Tooltip("From 0-360 Degrees - Starting Up - RHO")]
    public float maxAngle = -360;
    public float minAngle = 0;

    [Space]
    public bool clampAngle = true;

    [Space(2)]
    [Tooltip("ReadOnly! Calculated from above values")]
    public float APV = 1;


    [Space(5)]
    [Header("Send to another Pointer")]
    public bool sendValueTo = false;
    public bool sendRaw = false;
    public PointerIndicator toPointer;


    /////// Use this only for Debug/Tweaking values in real time during playmode and disable afterwards
    /*
    [Space(15)]
    public bool debugUpdate = false;
    void Update() { if (debugUpdate) setValue(rawValue); else return;}
    //*/
    ///////

    //Update to Inicial Value
    void OnEnable() { if (isActive && gameObject.activeInHierarchy) setValue(rawValue); }
    //

    //External call to set value
    public void setValue(float newValue)
    {
        if (!isActive) return;

        //Update Value
        rawValue = newValue;
        value = valueOffSet + factorValue * rawValue;
        APV = Mathf.Abs((maxAngle - minAngle) / (maxValue - minValue));

        if (!clampAngle) currentAngle = Mathf.MoveTowardsAngle(currentAngle, minAngle + angleOffSet + factorAngle * value * APV, 360);
        else
        {
            currentAngle = Mathf.Clamp(
                Mathf.MoveTowardsAngle(currentAngle, minAngle + angleOffSet + factorAngle * value * APV, 360),
                (minAngle <= maxAngle)? minAngle: maxAngle, (maxAngle >= minAngle) ? maxAngle : minAngle
                );
        }
        //


        //Set Pointer angle
        if (pointer != null) pointer.localRotation = Quaternion.Euler(pointer.localRotation.eulerAngles.x, pointer.localRotation.eulerAngles.y, currentAngle);

        //Set Text value
        if (valueTxt != null) { valueTxt.text = value.ToString("000"); }

        //Send Value to another Pointer
        if (sendValueTo && toPointer != null) toPointer.setValue((sendRaw) ? rawValue : value);
        //
    }
    //



}
