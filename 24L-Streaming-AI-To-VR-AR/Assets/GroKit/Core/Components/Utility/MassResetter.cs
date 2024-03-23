
using System.Collections.Generic;
using UnityEngine;

namespace Core3lb
{
    public class MassResetter : MonoBehaviour
    {
        [SerializeField] List<ResettableObject> myObjects;


        void Start()
        {
            for (int i = 0; i < myObjects.Count; i++)
            {
                myObjects[i].ConnectResetter(this);
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < myObjects.Count; i++)
            {
                myObjects[i]?.RemoveResetter(this);
            }
        }

        [CoreButton]
        public void _ResetAllObjects()
        {
            for (int i = 0; i < myObjects.Count; i++)
            {
                myObjects[i]._ResetObject();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ResettableObject holder))
            {
                holder._ResetObject();
            }
        }

        public void RemoveResettable(ResettableObject resettable)
        {
            if (myObjects.Contains(resettable))
                myObjects.Remove(resettable);
        }
    }
}
