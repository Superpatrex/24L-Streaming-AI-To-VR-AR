using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;

namespace Core3lb
{
    public class DisplayTMPBase : MonoBehaviour
    {
        [CoreRequired]
        public TMP_Text textMesh;
        public string startOfString = "";
        public string endOfString = "";

        public UnityEvent<string> textHasChanged;

        public event Action<string> textHasChangedAction;


        public virtual void Awake()
        {
            if (textMesh == null)
            {
                Debug.LogError("Missing TextMesh System",gameObject);
            }
        }

        public virtual void _SetText(string text)
        {
            if(textMesh.text == startOfString + text + endOfString)
            {
                return;
            }
            textMesh.text = startOfString + text + endOfString;
            EventActions(text);
        }

        public virtual void EventActions(string text)
        {
            textHasChangedAction?.Invoke(text);
            textHasChanged.Invoke(text);
        }
    }
}
