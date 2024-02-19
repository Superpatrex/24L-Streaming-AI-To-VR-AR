using UnityEngine;
using System.Collections;

namespace Core3lb
{
    public class MaterialTransition : MonoBehaviour
    {
        public Renderer[] renderers;
        public float transitionTime;
        public bool setOnStart;
        public bool transitionOnStart;
        public bool isMultiMat;
        [CoreShowIf("isMultiMat")]
        public int multiMatIndex;


        [CoreToggleHeader("Float Changes")]
        public bool useFloatChanges = true;
        [CoreShowIf("useFloatChanges")]
        public string materialProperty;
        [CoreShowIf("useFloatChanges")]
        public float setTo;
        [CoreShowIf("useFloatChanges")]
        public float transistionTo;
        [CoreShowIf("useFloatChanges")]
        [CoreReadOnly]
        public float matOriginalValue;

        [CoreToggleHeader("Color Changes")]
        public bool useColorChanges;
        [CoreShowIf("useColorChanges")]
        public string colorProperty = "_Color";
        [CoreShowIf("useColorChanges")]
        public Color[] colors;
        [CoreShowIf("useColorChanges")]
        public int setColorTo = 0;
        [CoreShowIf("useColorChanges")]
        [CoreReadOnly]
        public Color originalColor;

        private void Awake()
        {
            if (renderers == null || renderers.Length == 0)
            {
                renderers = new Renderer[1];
                renderers[0] = gameObject.GetComponent<MeshRenderer>();
                if (renderers[0] == null)
                {
                    Debug.LogError("You must assign an array to " + gameObject);
                    return;
                }
            }
        }

        private void Start()
        {
            if (materialProperty != null && materialProperty != "")
            {
                if (useFloatChanges)
                {
                    if (isMultiMat)
                    {
                        matOriginalValue = renderers[0].materials[multiMatIndex].GetFloat(materialProperty);
                    }
                    else
                    {
                        matOriginalValue = renderers[0].material.GetFloat(materialProperty);
                    }
                    if (setOnStart)
                    {
                        SetTo(setTo);
                    }
                }
                if (useColorChanges)
                {
                    if (isMultiMat)
                    {
                        originalColor = renderers[0].materials[multiMatIndex].GetColor(colorProperty);
                    }
                    else
                    {
                        originalColor = renderers[0].material.GetColor(colorProperty);
                    }
                    if (setOnStart)
                    {
                        _SetColorTo(setColorTo);
                    }
                }
            }
            if (transitionOnStart)
            {
                _TransitionTo(transistionTo);
            }
        }

        //[CoreButton]
        //public void PauseTransitions()
        //{
        //    for (int i = 0; i < renderers.Length; i++)
        //    {
        //        if (isMultiMat)
        //        {
        //            renderers[i].materials[multiMatIndex].DOPause();
        //        }
        //        else
        //        {
        //            renderers[i].material.DOPause();
        //        }
        //    }
        //}

        [CoreButton]
        public void _TransitionColor()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                StartCoroutine(TransitionColorCoroutine(renderers[i], colors[setColorTo], transitionTime));
            }
        }

        public void _TransitionColorTo(int index)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                StartCoroutine(TransitionColorCoroutine(renderers[i], colors[index], transitionTime));
            }
        }
        public void _SetColorTo(int index)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (isMultiMat)
                {
                    renderers[i].materials[multiMatIndex].SetColor(colorProperty, colors[index]);
                }
                else
                {
                    renderers[i].material.SetColor(colorProperty, colors[index]);
                }
            }
        }

        [CoreButton]
        public void _ResetColor()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (isMultiMat)
                {
                    renderers[i].materials[multiMatIndex].SetColor(colorProperty, originalColor);
                }
                else
                {
                    renderers[i].material.SetColor(colorProperty, originalColor);
                }
            }
        }


        public void _TransitionTo(float value)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                StartCoroutine(TransitionFloatCoroutine(renderers[i], value, transitionTime));
            }
        }

        private IEnumerator TransitionColorCoroutine(Renderer renderer, Color targetColor, float duration)
        {
            Material mat = isMultiMat ? renderer.materials[multiMatIndex] : renderer.material;
            Color startColor = mat.GetColor(colorProperty);
            float elapsed = 0;

            while (elapsed < duration)
            {
                mat.SetColor(colorProperty, Color.Lerp(startColor, targetColor, elapsed / duration));
                elapsed += Time.deltaTime;
                yield return null;
            }

            mat.SetColor(colorProperty, targetColor);
        }

        private IEnumerator TransitionFloatCoroutine(Renderer renderer, float targetValue, float duration)
        {
            Material mat = isMultiMat ? renderer.materials[multiMatIndex] : renderer.material;
            float startValue = mat.GetFloat(materialProperty);
            float elapsed = 0;

            while (elapsed < duration)
            {
                mat.SetFloat(materialProperty, Mathf.Lerp(startValue, targetValue, elapsed / duration));
                elapsed += Time.deltaTime;
                yield return null;
            }

            mat.SetFloat(materialProperty, targetValue);
        }

        public void _TransitionTime(float value)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                StartCoroutine(TransitionFloatCoroutine(renderers[i], transistionTo, value));
            }
        }


        public void SetTo(float value)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (isMultiMat)
                {
                    renderers[i].materials[multiMatIndex].SetFloat(materialProperty, value);
                }
                else
                {
                    renderers[i].material.SetFloat(materialProperty, value);
                }
            }
        }

        public void _SwitchTo(Material mat)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (isMultiMat)
                {
                    //renderers[i].materials[multiMatIndex] = mat;
                    var rendMats = renderers[i].materials;
                    rendMats[multiMatIndex] = mat;
                    renderers[i].materials = rendMats;
                }
                else
                {
                    renderers[i].material = mat;
                }
            }
        }
    }
}
