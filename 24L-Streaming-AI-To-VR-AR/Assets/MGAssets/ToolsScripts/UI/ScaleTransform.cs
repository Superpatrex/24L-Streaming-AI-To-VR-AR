using UnityEngine;
using UnityEngine.UI;


public class ScaleTransform : MonoBehaviour
{

    public bool isActive = true;
    public RectTransform rectTransform;
    public Transform objTransform;


    [Space]
    public bool updateOnEnable = false;

    [Space]
    public float currentScale = 1;
    public float scaleStep = 0.25f, scaleMin = 0.25f, scaleMax = 1;


    float initialScale = 1;

    ///////////////////// Initialization
    void Awake()
    {
        if (rectTransform != null) currentScale = rectTransform.localScale.x;
        else if (objTransform != null) currentScale = objTransform.localScale.x;
        else currentScale = 1;

        initialScale = currentScale;
    }
    //
    void OnEnable() { if (updateOnEnable && gameObject.activeInHierarchy && isActive) setScale(currentScale); }
    ///////////////////// Initialization

        

    /////////////////////// Update Scale
    public void setScale(float setScale = 1f)
    {
        if (!isActive) return;

        setScale -= setScale % scaleStep;
        setScale = Mathf.Clamp(setScale, scaleMin, scaleMax);
        currentScale = setScale;

        if (rectTransform != null) rectTransform.localScale = currentScale * Vector3.one;
        else if (objTransform != null) objTransform.localScale = currentScale * Vector3.one;

    }
    ///////////////////////



    //////////////// External Calls
    public void scaleIn (float value = 0) { if (value == 0) value = scaleStep; setScale(currentScale + value); }
    public void scaleOut(float value = 0) { if (value == 0) value = scaleStep; setScale(currentScale - value); }
    public void scaleReset() { setScale(initialScale); }
    ////////////////

}
