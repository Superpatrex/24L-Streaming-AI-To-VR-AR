using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;


namespace Core3lb
{
    public class DEBUG_EventFinder : MonoBehaviour
    {
        public enum SearchingFor
        {
            ForActivator,
            ForGameObject,
            ForComponentType,
            ForThisComponent
        }
        public SearchingFor searchForMode = SearchingFor.ForActivator;
        public BaseActivator searchForActivator;

        [Space(10)]
        public Object searchFor;
        public List<GameObject> objectsFound;
        public bool onlyInChildren = false;

        [CoreButton("Search Activators",true)]
        public void SearchActivatorsOnEvent()
        {
            ClearList();
            BaseActivator[] activators = FindObjectsOfType<BaseActivator>(true);
            if (onlyInChildren)
            {
                activators = GetComponentsInChildren<BaseActivator>(includeInactive: true);
            }
            foreach (BaseActivator item in activators)
            {
                if (item.onEvent.GetPersistentEventCount() > 0)
                {
                    for (int i = 0; i < item.onEvent.GetPersistentEventCount(); i++)
                    {
                        switch (searchForMode)
                        {
                            case SearchingFor.ForActivator:
                                if (item.onEvent.GetPersistentTarget(i) == searchForActivator)
                                {
                                    objectsFound.Add(item.gameObject);
                                }
                                break;
                            case SearchingFor.ForGameObject:
                                GameObject activator = item.onEvent.GetPersistentTarget(i) as GameObject;
                                GameObject holder = searchFor as GameObject;
                                if (activator == holder || activator == holder)
                                {
                                    objectsFound.Add(item.gameObject);
                                }
                                break;
                            case SearchingFor.ForComponentType:
                                Object obj1 = item.onEvent.GetPersistentTarget(i);
                                Object obj2 = searchFor; // Your second object instance
                                Type type1 = obj1.GetType();
                                Type type2 = obj2.GetType();
                                if (type1 == type2)
                                {
                                    objectsFound.Add(item.gameObject);
                                }
                                break;
                            case SearchingFor.ForThisComponent:
                                if (item.onEvent.GetPersistentTarget(i) == searchFor)
                                {
                                    objectsFound.Add(item.gameObject);
                                }
                                break;
                            default:
                                break;
                        }

                    }
                }
            }
            Debug.LogError("---------SEARCH COMPLETE-------");
        }

        [CoreButton("Search Activators Off", true)]
        public void SearchActivatorsOffEvent()
        {
            ClearList();
            BaseActivator[] activators = FindObjectsOfType<BaseActivator>(true);
            if (onlyInChildren)
            {
                activators = GetComponentsInChildren<BaseActivator>(includeInactive: true);
            }
            foreach (BaseActivator item in activators)
            {
                if (item.offEvent.GetPersistentEventCount() > 0)
                {
                    for (int i = 0; i < item.offEvent.GetPersistentEventCount(); i++)
                    {
                        switch (searchForMode)
                        {
                            case SearchingFor.ForActivator:
                                if (item.offEvent.GetPersistentTarget(i) == searchForActivator)
                                {
                                    objectsFound.Add(item.gameObject);
                                }
                                break;
                            case SearchingFor.ForGameObject:
                                GameObject activator = item.offEvent.GetPersistentTarget(i) as GameObject;
                                GameObject holder = searchFor as GameObject;
                                if (activator == holder || activator == holder)
                                {
                                    objectsFound.Add(item.gameObject);
                                }
                                break;
                            case SearchingFor.ForComponentType:
                                Object obj1 = item.offEvent.GetPersistentTarget(i);
                                Object obj2 = searchFor; // Your second object instance
                                Type type1 = obj1.GetType();
                                Type type2 = obj2.GetType();
                                if (type1 == type2)
                                {
                                    objectsFound.Add(item.gameObject);
                                }
                                break;
                            case SearchingFor.ForThisComponent:
                                if (item.offEvent.GetPersistentTarget(i) == item)
                                {
                                    objectsFound.Add(item.gameObject);
                                }
                                break;
                            default:
                                break;
                        }

                    }
                }
            }
            Debug.LogError("---------SEARCH COMPLETE-------");
        }

        public void ClearList()
        {
            objectsFound.Clear();
        }



        //thi
        public void SearchAllUnityInHeirarchyEvents()
        {

        }

        private void FindUnityEventsInGameObjects()
        {
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject go in allGameObjects)
            {
                FindUnityEventsInGameObject(go);
            }
        }

        private void FindUnityEventsInGameObject(GameObject go)
        {
            MonoBehaviour[] behaviours = go.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                FindUnityEventsInBehaviour(behaviour);
            }
        }

        private void FindUnityEventsInBehaviour(MonoBehaviour behaviour)
        {
            System.Type behaviourType = behaviour.GetType();
            var fields = behaviourType.GetFields();

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(UnityEvent))
                {
                    UnityEvent unityEvent = (UnityEvent)field.GetValue(behaviour);
                    if (unityEvent != null)
                    {
                        Debug.Log("UnityEvent found in " + behaviourType.Name + ": " + field.Name);
                        // You can do further processing with the found UnityEvent here
                    }
                }
            }
        }
    }
}
