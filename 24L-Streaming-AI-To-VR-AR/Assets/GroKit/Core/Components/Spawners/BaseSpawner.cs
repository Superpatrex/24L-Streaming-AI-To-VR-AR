using UnityEngine;
using UnityEngine.Events;

namespace Core3lb
{
    public class BaseSpawner : MonoBehaviour
    {
        public PositionerBase postioner;
        public GameObject[] objectsToSpawn;
        [CoreReadOnly]
        public GameObject lastObjectSpawned;

        public UnityEvent objectSpawned;

        [HideInInspector]
        public bool doOverride;

        public virtual void _SpawnObject()
        {
            InternalSpawnObject(PickObject(), Position, Rotation);
        }

        public virtual void _SpawnObject(int index)
        {
            InternalSpawnObject(objectsToSpawn[index], Position, Rotation);
        }

        public virtual void _SpawnObject(Transform where)
        {
            InternalSpawnObject(PickObject(), where.position, where.rotation);
        }

        public virtual Vector3 Position
        {
            get
            {
                if (postioner)
                {
                    return postioner.WhatPosition();
                }
                else
                {
                    return transform.position;
                }
            }
        }

        public virtual Quaternion Rotation
        {
            get
            {
                if (postioner)
                {
                    return postioner.WhatRotation();
                }
                else
                {
                    return transform.rotation;
                }
            }
        }

        public virtual GameObject PickObject()
        {
            return objectsToSpawn.RandomItem();
        }

        public virtual void InternalSpawnObject(GameObject what,Vector3 where, Quaternion rotation)
        {
            SpawnObjectActual(what, where, rotation);
        }

        //For RPCS!
        public virtual void SpawnObjectActual(GameObject what, Vector3 where, Quaternion rotation)
        {
            if(!doOverride)
            {
                lastObjectSpawned = Instantiate(what, where, rotation);
                AfterSpawn();
            }
            objectSpawned.Invoke();
        }

        public virtual void AfterSpawn()
        {

        }
    }
}
