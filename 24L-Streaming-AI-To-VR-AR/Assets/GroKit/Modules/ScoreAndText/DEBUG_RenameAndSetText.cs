using TMPro;
using UnityEngine;

namespace Core3lb
{
    public class DEBUG_RenameAndSetText : MonoBehaviour
    {

        public string textToSet = "";
        [CoreHeader("TMP Text")]
        public string startOfStringTMP = "";
        public string endOfStringTMP = "";
        [CoreHeader("Game Object Name")]
        public string startOfStringGO = "Text_";
        public string endOfStringGO = "";
        [CoreButton]
        public void SetTMP_Text()
        {
            GetComponent<TMP_Text>().text = startOfStringTMP + textToSet + endOfStringTMP;
        }

        [CoreButton]
        public void GetNameFromGameObject()
        {
            string text = GetComponent<TMP_Text>().text;
            gameObject.name = text;
        }

        [CoreButton]
        public void SetNameGameObject()
        {
            gameObject.name = startOfStringGO + endOfStringGO + textToSet;
        }
    }
}
