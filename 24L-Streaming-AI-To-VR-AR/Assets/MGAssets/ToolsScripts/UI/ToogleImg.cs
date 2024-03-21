using UnityEngine;
using UnityEngine.UI;

public class ToogleImg : MonoBehaviour
{
    public bool isActive = true;


    //////////////////////////////////// GUI Image Component 
    public Image image;             //Image Component
    public int indexSprite = 0;     //Index for the Sprite Array Below
    public Sprite[] sprites;        //Array with alternative Sprites for the Image Component

    // Changes the Image by array index, a custom one or Next-Previous index
    public void setSprite() { setSprite(indexSprite); }
    public void setSprite(int index)
    {
        if (!isActive || image == null) return;

        if (index != -1) indexSprite = index;
        if (indexSprite < sprites.Length && sprites[indexSprite] != null) image.sprite = sprites[indexSprite];
    }
    public void setSprite(Sprite customSprite)
    {
        if (!isActive || image == null) return;
        if (customSprite != null) image.sprite = customSprite;
    }
    //
    public void setSpriteNext() { indexSprite += 1; indexSprite %= sprites.Length; setSprite(indexSprite); }
    public void setSpriteBack() { indexSprite -= 1; if (indexSprite < 0) indexSprite = sprites.Length - 1; setSprite(indexSprite); }
    //    

    // Enable/Disable/Toogle the Image component
    public void toogleImg()  { if (isActive && image != null) image.enabled = !image.enabled; }
    public void enableImg()  { if (isActive && image != null) image.enabled = true;           }
    public void disableImg() { if (isActive && image != null) image.enabled = false;          }
    public void setImg(bool setTo = true) { if (setTo) enableImg(); else disableImg();        }
    //

    //////////////////////////////////// GUI Image Component 


    //////////////////////////////////// GUI Mask  Component
    public Mask mask; //Mask Component

    // Enable/Disable/Toogle -> Mask component
    public void toogleMask()  { if (isActive && mask != null) mask.enabled = !mask.enabled; }
    public void enableMask()  { if (isActive && mask != null) mask.enabled = true;          }
    public void disableMask() { if (isActive && mask != null) mask.enabled = false;         }
    //

    // Enable/Disable/Toogle -> MaskGraphic visibility
    public void toogleMaskGraph()  { if (isActive && mask != null) mask.showMaskGraphic = !mask.showMaskGraphic; }
    public void enableMaskGraph()  { if (isActive && mask != null) mask.showMaskGraphic = true;                  }
    public void disableMaskGraph() { if (isActive && mask != null) mask.showMaskGraphic = false;                 }
    //
    //////////////////////////////////// GUI Mask  Component

}
