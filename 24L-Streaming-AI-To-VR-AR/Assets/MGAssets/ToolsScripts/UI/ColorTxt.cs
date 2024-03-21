using UnityEngine;
using UnityEngine.UI;


public class ColorTxt : MonoBehaviour
{
    public bool isActive = true;

    public Text txt;

    public bool updateOnEnable = false;

    [Tooltip("If True, the index 0 of colors will be updated during awake with the current color on txt component")]
    public bool updateIndexZero = false;

    public int indexColor = 0;
    public Color[] colors = new Color[2];

    //
    void Awake()
    {
        if (txt == null) txt = GetComponent<Text>();

        if (updateIndexZero && colors.Length > 0)
        {
            if (txt != null) colors[0] = txt.color;
        }
    }
    void OnEnable() { if (updateOnEnable && gameObject.activeInHierarchy && gameObject.activeSelf) setColor(indexColor); }
    //


    //
    public void setColor(int index = 0)
    {
        if (!isActive) return;

        if (txt != null && index <= colors.Length - 1) { txt.color = colors[index]; indexColor = index; }
    }
    //
    public void toogleColor()
    {
        if (colors.Length < 2) return;
        if (indexColor == 0) setColor(1); else setColor(0);
    }
    //

}
