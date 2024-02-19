using System.Collections.Generic;
using UnityEngine;

namespace Core3lb
{
    public class TrackedSpawner : BaseSpawner
    {
        [Tooltip("-1 = Infinite")]
        public int maxSpawnAmount = -1;
        public List<GameObject> spawnedObjects;

        public override void SpawnObjectActual(GameObject what, Vector3 where, Quaternion rotation)
        {
            if (maxSpawnAmount != -1)
            {
                spawnedObjects.RemoveNulls();
                if (spawnedObjects.Count >= maxSpawnAmount)
                {
                    Debug.LogError("TooManyObjects");
                    return;
                }
            }
            spawnedObjects.Add(Instantiate(what, where, rotation));
        }

        public virtual void _DestoryAllObjects()
        {
            foreach (var obj in objectsToSpawn)
            {
                Destroy(obj);
            }
        }
    }
}
