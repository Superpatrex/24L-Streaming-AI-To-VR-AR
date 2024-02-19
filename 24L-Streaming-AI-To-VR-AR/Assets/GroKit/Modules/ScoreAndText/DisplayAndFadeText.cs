using System.Collections;
using UnityEngine.Events;

namespace Core3lb
{
    public class DisplayAndFadeText : DisplayTMPBase
    {
        [CoreHeader("DisplayAndFadeText")]
        public float fadeTime;
        public float displayDuration;
        IEnumerator myCoroutine;

        public UnityEvent textFadeComplete;

        public override void _SetText(string text)
        {
            if (textMesh.text == startOfString + text + endOfString)
            {
                return;
            }
            textMesh.text = startOfString + text + endOfString;
            EventActions(text);
            textMesh.SetTextAlpha(0);
            StartCoroutine(textMesh.FadeWithDelay(0, fadeTime, displayDuration, TextFadeComplete));
        }

        public void TextFadeComplete()
        {
            textFadeComplete.Invoke();
        }
    }
}
