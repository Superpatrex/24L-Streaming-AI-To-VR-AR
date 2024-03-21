using UnityEngine;
using UnityEngine.UI;

public class ColorImg : MonoBehaviour
{
    public bool isActive = true;

    public Image image;
    public RawImage rawImage;

    public bool updateOnEnable = false;

    [Tooltip("If True, the index 0 of colors will be updated during awake with the current color on image component")]
    public bool updateIndexZero = false;

    public int indexColor = 0;
    public Color[] colors = new Color[2];

    //
    void Awake()
    {
        if (image == null) image = GetComponent<Image>();
        if (image == null && rawImage == null) rawImage = GetComponent<RawImage>();

        if (updateIndexZero && colors.Length > 0)
        {
            if (image != null) colors[0] = image.color;
            else if (rawImage != null) colors[0] = rawImage.color;
        }
    }
    void OnEnable() { if (updateOnEnable && gameObject.activeInHierarchy && gameObject.activeSelf) setColor(indexColor); }
    //


    //
    public void setColor(int index = 0)
    {
        if (!isActive) return;

        if (   image != null && index <= colors.Length - 1) {    image.color = colors[index]; indexColor = index; }
        if (rawImage != null && index <= colors.Length - 1) { rawImage.color = colors[index]; indexColor = index; }
    }
    //
    public void toogleColor()
    {
        if (colors.Length < 2) return;
        if (indexColor == 0) setColor(1); else setColor(0);
    }
//

}
