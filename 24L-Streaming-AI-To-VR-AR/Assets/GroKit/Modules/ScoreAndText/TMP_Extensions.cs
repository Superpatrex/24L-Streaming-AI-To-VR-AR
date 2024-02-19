using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Core3lb
{
    public static class TMP_Extensions
    {
        public static IEnumerator FadeWithDelay(this TMP_Text textMesh, float targetAlpha, float fadeTime = 2, float displayDuration = 0, Action callBack = null)
        {
            // Wait for the specified display duration
            yield return new WaitForSeconds(displayDuration);

            float initialAlpha = textMesh.color.a;
            float time = 0;

            while (time <= fadeTime)
            {
                time += Time.deltaTime;
                float normalizedTime = time / fadeTime; // normalized time ranges from 0 to 1
                float updatedAlpha = Mathf.Lerp(initialAlpha, targetAlpha, normalizedTime);

                Color updatedColor = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, updatedAlpha);
                textMesh.color = updatedColor;

                yield return null; // wait until next frame
            }

            // Ensure final alpha is set to targetAlpha
            Color finalColor = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, targetAlpha);
            textMesh.color = finalColor;
            callBack?.Invoke();
        }

        public static void SetTextAlpha(this TMP_Text textMesh, float alphaAmount = 0)
        {
            Color updatedColor = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, alphaAmount);
            textMesh.color = updatedColor;
        }

        public static void FlashAndFadeText(this TMP_Text textMesh, float fadeTime = 2, float displayDuration = 0, Action callBack = null)
        {
            SetTextAlpha(textMesh, 0);
            textMesh.StartCoroutine(FadeWithDelay(textMesh, 0, fadeTime, displayDuration, callBack));
        }
    }
}

