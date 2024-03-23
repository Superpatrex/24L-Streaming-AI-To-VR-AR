using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace Core3lb
{
    public class BinTrigger : MonoBehaviour
    {
        [CoreToggleHeader("Randomize Bin")]
        public bool randomizeBinOnStart;
        [CoreShowIf("randomizeBinOnStart")]
        public Vector2 binRange = new Vector2(2, 6);
        [CoreShowIf("randomizeBinOnStart")]
        public List<ItemIDString> listOfItems;


        public List<ItemIDString> binRequirements;
        [CoreHeader("Debug")]
        [CoreReadOnly]
        public List<ItemIDString> itemsInBin;
        [CoreReadOnly]
        public int incorrectItems;
        [CoreReadOnly]
        public int correctItems;
        [Space]

        [CoreReadOnly]
        public List<ItemIDString> listOfCorrectItems;
        [CoreReadOnly]
        public List<ItemIDString> listOfIncorrectItems;
        [CoreHeader("Events")]
        //Fires if Bin is correct
        public UnityEvent correctEvent;
        //Fires if Bin is wrong
        public UnityEvent incorrectEvent;
        //Fires on Resetting Bin
        public UnityEvent resetBin;
        //Fires when Bin is updated (Item added or removed)
        public UnityEvent binUpdated;
        int binMax;

        public virtual void Start()
        {
            if (randomizeBinOnStart)
            {
                _RandomizeBin();
            }
        }

        [CoreButton]
        public virtual void _RandomizeBin()
        {
            binMax = Mathf.RoundToInt(Random.Range(binRange.x, binRange.y));
            for (int i = 0; i < binMax; i++)
            {
                binRequirements.Add(listOfItems.RandomItem());
            }
            binRequirements = binRequirements.OrderBy(o => o.itemID).ToList();
        }

        [CoreButton]
        public virtual void UpdateItemCount()
        {
            incorrectItems = 0;
            correctItems = 0;
            listOfCorrectItems.Clear();
            listOfIncorrectItems.Clear();
            // Copy binRequirements to a new list so we can modify it
            var unmatchedRequirements = new List<ItemIDString>(binRequirements);

            foreach (var item in itemsInBin)
            {
                var matchedItem = unmatchedRequirements.FirstOrDefault(req => req.itemID == item.itemID);
                if (matchedItem != null)
                {
                    correctItems++;
                    listOfCorrectItems.Add(item);
                    unmatchedRequirements.Remove(matchedItem); // Remove the matched item to handle duplicates
                }
                else
                {
                    listOfIncorrectItems.Add(item);
                    incorrectItems++;
                }
            }
            binUpdated.Invoke();
        }

        [CoreButton]
        public virtual void _CheckBin()
        {
            UpdateItemCount();
            if (correctItems == binRequirements.Count && incorrectItems == 0)
            {
                _BinCorrect();
            }
            else
            {
                _BinIncorrect();
            }
        }


        public virtual void _BinCorrect()
        {
            //Debug.Log("Bin Right");
            correctEvent.Invoke();
        }

        public virtual void _BinIncorrect()
        {
            //Debug.Log("Bin Wrong");
            incorrectEvent.Invoke();
        }

        public virtual void _ClearBin()
        {
            for (int i = 0; i < itemsInBin.Count; i++)
            {
                Destroy(itemsInBin[i].gameObject);
            }
            itemsInBin.Clear();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<ItemIDString>())
            {
                if (!itemsInBin.Contains(other.GetComponent<ItemIDString>()))
                    itemsInBin.Add(other.GetComponent<ItemIDString>());
                UpdateItemCount();
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<ItemIDString>())
            {
                if (itemsInBin.Contains(other.GetComponent<ItemIDString>()))
                    itemsInBin.Remove(other.GetComponent<ItemIDString>());
                UpdateItemCount();
            }
        }
    }
}
