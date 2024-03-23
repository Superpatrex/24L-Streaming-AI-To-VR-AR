using UnityEngine;
using UnityEngine.UI;

public class RollDigitIndicator : MonoBehaviour
{
    public bool isActive = true;

    public RawImage rawImg;

    [Space]
    public float rawValue, value;
    public float factor = 1;
    public float valueOffSet = 0;

    [Space]
    public bool continuousMode = false;
    public bool moveX = false, moveY = true;

    [Space]
    public bool autoStartXY = true;
    [Range(-1, 1)] public float startX = 0;
    [Range(-1, 1)] public float startY = 0;

    [Space(5)]
    public bool sendValueToNext = false;
    public bool sendRaw = false;
    public RollDigitIndicator nextDigit;


    float maxValue = 10, fraction, rollOverFactor;
    //

    /////// Use this only for Debug/Tweaking values in real time during playmode and disable afterwards
    //*
    [Space(15)]
    public bool debugUpdate = false;
    void Update() { if (debugUpdate) setValue(rawValue); else return; }
    //*/
    ///////


    //////// Inicialization and Update to Inicial Value
    void Start() { if (autoStartXY) { startX = rawImg.uvRect.x; startY = rawImg.uvRect.y; } }
    void OnEnable() { if (isActive && gameObject.activeInHierarchy) setValue(rawValue); }
    ////////



    ////////////////////////////////////////////////// SetValue of current Digit
    public void setValue(float newValue)
    {
        if (!isActive) return;

        //Read New Value and Calculate fraction
        rawValue = newValue;
        value = factor * (newValue + valueOffSet) % maxValue;
        if (!continuousMode) if(value >= 0) value = Mathf.Floor(value); else value = Mathf.Ceil(value);

        fraction = Mathf.Abs((value / maxValue) % 1);
        //


        //Send RollOver data to next Digits
        if (sendValueToNext && nextDigit != null)
        {
            if (continuousMode && value > 9f) nextDigit.rollOverFactor = (fraction - 0.9f) * 10f;
            else if (!continuousMode && value >= 9f) nextDigit.rollOverFactor = rollOverFactor;
            else nextDigit.rollOverFactor = 0;

            nextDigit.setValue((sendRaw) ? rawValue : value);
        }
        //


        //Update GUI values
        if (!continuousMode)
        {
            if (moveX) rawImg.uvRect = new Rect(fraction + rollOverFactor/10 + startX, rawImg.uvRect.y, rawImg.uvRect.width, rawImg.uvRect.height);
            if (moveY) rawImg.uvRect = new Rect(rawImg.uvRect.x, fraction + rollOverFactor/10 + startY, rawImg.uvRect.width, rawImg.uvRect.height);

        }
        else
        {
            if (moveX) rawImg.uvRect = new Rect(fraction + startX, rawImg.uvRect.y, rawImg.uvRect.width, rawImg.uvRect.height);
            if (moveY) rawImg.uvRect = new Rect(rawImg.uvRect.x, fraction + startY, rawImg.uvRect.width, rawImg.uvRect.height);
        }
        //
    }
    ////////////////////////////////////////////////// SetValue of current Digit
    

}