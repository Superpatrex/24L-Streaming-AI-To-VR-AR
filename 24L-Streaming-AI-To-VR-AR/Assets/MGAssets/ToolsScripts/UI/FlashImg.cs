using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class FlashImg : MonoBehaviour
{
    public bool isFlashing = false, flashOnEnable = true, disableOnDisable = false;

    [Space]
    public Image image;
    public float speed = 3, seconds = -1, secondsRand = -1;

    [Space]
    public Color normalColor, flashColor;

    [Space]
    public int indexSnd;
    public AudioClip flashSnd;
    public float volume = 1f;


    float totalTime, randomTime = 0;

    ////////////////////////////////////
    void Awake()
    {
        if (image == null) image = GetComponent<Image>();
        //if (normalColor == null) normalColor = image.color;
    }
    void OnEnable() { if ( (isFlashing || flashOnEnable) && gameObject.activeInHierarchy && gameObject.activeSelf) flash(); }
    void OnDisable()
    {
        if (disableOnDisable) { StopCoroutine("flashCall"); isFlashing = false; }
        image.color = normalColor;
    }
    ////////////////////////////////////


    //////////////////////////////////
    public void stopFlash() { isFlashing = false; }
    public void flash() { StopCoroutine("flashCall"); if(gameObject.activeInHierarchy) StartCoroutine("flashCall"); }
    //
    IEnumerator flashCall()
    {
        totalTime = 0; randomTime = 0;
        if (secondsRand > 0) randomTime = secondsRand * Random.Range(-1f, 1f);

        isFlashing = true;
        yield return null;

        if (indexSnd != 0) SndPlayer.play(indexSnd);
        else if(flashSnd != null) SndPlayer.play(flashSnd, volume);         

        while (isFlashing)
        {
            image.color = Color.Lerp(normalColor, flashColor, Mathf.PingPong(Time.time * speed, 1f));

            if(seconds != -1)
            {
                totalTime += Time.deltaTime;
                if (totalTime >= seconds + randomTime) isFlashing = false;
            }
            yield return null;
        }

        while (image.color != normalColor) { image.color = Color.Lerp(image.color, normalColor, Time.deltaTime * speed); yield return null; }


        isFlashing = false; yield break;
    }
    //////////////////////////////////
}
