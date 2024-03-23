using UnityEngine;
using UnityEngine.UI;


public class CompassBar : MonoBehaviour
{
    public RawImage rawImg;
    public Text headingTxt;

    public float heading, headingOffSet = 0, factor = 1, maxValue = 360;
    public bool isActive = true, moveX = true;

    [Space(5)]
    public bool autoStart = true;
    public float startX = 0, startY = 0, startWidth, startHeight;
    //
    void Start()
    {
        if (autoStart)
        {
            startX = rawImg.uvRect.x; startY = rawImg.uvRect.y;
            startWidth = rawImg.uvRect.width; startHeight = rawImg.uvRect.height;
        }
    }
    //

    //Update to Inicial Value
    void OnEnable() { if (isActive && gameObject.activeInHierarchy) setValue(heading); }
    //


    //
    public void setValue(float value)
    {
        if (!isActive) return;

        heading = value;

        if (moveX) rawImg.uvRect = new Rect(factor * (heading + headingOffSet) / maxValue + startX, rawImg.uvRect.y, rawImg.uvRect.width, rawImg.uvRect.height);
        if (headingTxt != null) { if (heading < 0) headingTxt.text = (heading + 360f).ToString("000"); else headingTxt.text = heading.ToString("000"); }
    }
    //

    /////////////////////////////// Reserved
    ////void Update()
    ////{
    ////    if (isActive)
    ////    {
    ////        if(moveX) rawImg.uvRect = new Rect(factor * (heading + headingOffSet) / maxValue + startX, rawImg.uvRect.y, rawImg.uvRect.width, rawImg.uvRect.height);

    ////        if (headingTxt != null) { if (heading < 0) headingTxt.text = (heading + 360f).ToString("000"); else headingTxt.text = heading.ToString("000"); }
    ////    }
    ////}
    ///////////////////////////////

}
