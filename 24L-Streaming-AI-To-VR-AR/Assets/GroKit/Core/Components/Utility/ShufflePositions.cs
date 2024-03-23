using System.Collections.Generic;
using UnityEngine;

namespace Core3lb
{
    public class ShufflePositions : MonoBehaviour
    {
        public List<GameObject> ListOfObjects;
        public List<Transform> RandomPositions;

        // Start is called before the first frame update
        private void Start()
        {
            //If you need to randomize the top
            //ListOfObjects = Randomize(ListOfObjects);
            RandomPositions = Randomize(RandomPositions);
            for (int i = 0; i < ListOfObjects.Count; i++)
            {
                ListOfObjects[i].transform.position = RandomPositions[i].position;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public static List<T> Randomize<T>(List<T> list)
        {
            List<T> randomizedList = new List<T>();
            System.Random rnd = new System.Random();
            while (list.Count > 0)
            {
                int index = rnd.Next(0, list.Count); //pick a random item from the master list
                randomizedList.Add(list[index]); //place it at the end of the randomized list
                list.RemoveAt(index);
            }
            return randomizedList;
        }
    }
}
