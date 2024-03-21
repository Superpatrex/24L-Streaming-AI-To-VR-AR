using UnityEngine;
using UnityEngine.UI;


public class UpdateFill : MonoBehaviour
{
    public bool isActive = true;
    public bool onEnable = false, update = true;

    [Space]
    public Slider fromSlider;
    public Image fromFill;

    [Space]
    public Slider[] toSliders;
    public Image[] toFills;


    //////////////////////////////  Update - (Once during OnEnable / Constant Update)
    void OnEnable() { if (gameObject.activeInHierarchy && isActive && onEnable) updateFill(); }
    void Update()
    {
        if (!isActive || !update) return;
        updateFill();
    }
    //////////////////////////////


    ////////////////////////////////////////////////////////// Sets Fill Value
    public void updateFill()
    {
        if (toSliders.Length == 0 && toFills.Length == 0) return;
        //
        if (toSliders.Length > 0)
        {
            if (fromSlider != null) foreach (Slider slider in toSliders) if(slider != null) slider.value = fromSlider.value;
            if (fromFill != null) foreach (Slider slider in toSliders) if (slider != null) slider.value = fromFill.fillAmount;
        }
        //
        if (toFills.Length > 0)
        {
            if (fromSlider != null) foreach (Image fill in toFills) if (fill != null) fill.fillAmount = fromSlider.value;
            if (fromFill != null) foreach (Image fill in toFills) if (fill != null) fill.fillAmount = fromFill.fillAmount;
        }
    }
    //////////////////////////////////////////////////////////

}
