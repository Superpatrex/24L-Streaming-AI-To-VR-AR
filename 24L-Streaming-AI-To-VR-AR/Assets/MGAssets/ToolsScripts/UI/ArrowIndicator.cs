using UnityEngine;
using UnityEngine.UI;

public class ArrowIndicator : MonoBehaviour
{
    public bool isActive = true;

    [Space]
    public RectTransform arrow;
    public float value, valueFactor = 1, valueOffSet = 0;
    public float maxValue = 100, minValue = -100;

    [Space]
    public bool moveX = true;
    public float currentX = 0, factorX = 1, offSetX = 0;
    public float maxX = 115, minX = -115;
    public bool clampX = true;

    [Space]
    public bool moveY = false;
    public float currentY, factorY = 1, offSetY = 0;
    public float maxY = 115, minY = -115;
    public bool clampY = true;

    [Space]
    public Text valueTxt;

    //Update to Inicial Value
    void OnEnable() { if (isActive && gameObject.activeInHierarchy) setValue(value); }
    //

    //External call to set value
    [ContextMenu("Update Value")]
    public void setValue(float newValue) 
    {
        if (!isActive) return;

        //Update Value
        value = valueOffSet + valueFactor * newValue;

        //Calculate X
        if (moveX)
        {
            //currentX = offSetX + factorX * value;
            currentX = offSetX + factorX * value * (maxX - minX) / (maxValue - minValue);
            if (clampX) currentX = Mathf.Clamp(currentX, minX, maxX);

            //Set Arrow X position
            if (arrow != null && !float.IsNaN(currentX)) arrow.localPosition = new Vector3(currentX, arrow.localPosition.y, arrow.localPosition.z);
        }
        //

        //Calculate Y
        if (moveY)
        {
            //currentY = offSetY + factorY * value;
            currentY = offSetY + factorY * value * (maxY - minY) / (maxValue - minValue);
            if (clampY) currentY = Mathf.Clamp(currentY, minY, maxY);

            //Set Arrow Y position
            if (arrow != null && !float.IsNaN(currentY)) arrow.localPosition = new Vector3(arrow.localPosition.x, currentY, arrow.localPosition.z);
        }
        //

        //Set UI Text value
        if (valueTxt != null) { valueTxt.text = value.ToString("000"); }
    }
    //
}
