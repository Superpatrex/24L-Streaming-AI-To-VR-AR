using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

namespace Core3lb
{
    public class BinSystemUIBridge : MonoBehaviour
    {
        [CoreRequired]
        public BinTrigger binTrigger;
        [Space(10)]
        [CoreEmphasize]
        public TMP_Text binRequirementsText;
        [CoreEmphasize]
        public TMP_Text binContents;
        [CoreEmphasize]
        public TMP_Text binCorrect;
        [CoreEmphasize]
        public TMP_Text binIncorrect;
        [CoreEmphasize]
        public TMP_Text binCorrectCount;
        [CoreEmphasize]
        public TMP_Text binIncorrectCount;

        public virtual void Start()
        {
            binTrigger.binUpdated.AddListener(UpdateText);
            UpdateText();
        }

        public void UpdateText()
        {
            if(binRequirementsText)
            {
                binRequirementsText.text = GetItemsAsText(binTrigger.binRequirements);
            }
            if(binContents)
            {
                binContents.text = GetItemsAsText(binTrigger.itemsInBin);
            }
            if(binCorrect)
            {
                binCorrect.text = GetItemsAsText(binTrigger.listOfCorrectItems);
            }
            if(binIncorrect)
            {
                binIncorrect.text = GetItemsAsText(binTrigger.listOfIncorrectItems);
            }
            if(binCorrectCount)
            {
                binCorrectCount.text = binTrigger.correctItems.ToString();
            }
            if(binIncorrectCount)
            {
                binIncorrectCount.text = binTrigger.incorrectItems.ToString();
            }
        }

        public string GetItemsAsText(List<ItemIDString> theList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in theList)
            {
                sb.AppendLine(item.itemID);
            }
            return sb.ToString();
        }
    }
}
