using UnityEngine;

namespace Core3lb
{
    public class FXSpawner : MonoBehaviour
    {
        //This spawner is for making and then quickly destorying a prefab. If you want to spawn something use Spawners instead
        [SerializeField] GameObject[] objectsToSpawn;
        //When Networking this should be called on the local server or need to be extended
        [Tooltip("Needs to Die")]
        [SerializeField] float deathTime = 1;

        GameObject storedPrefab;

        public void _SpawnOnSelf()
        {
            InternalSpawnObject(objectsToSpawn.RandomItem(), transform.position, transform.rotation);
        }

        public void _SpawnHere(Transform where)
        {
            InternalSpawnObject(objectsToSpawn.RandomItem(), where.position, where.rotation);
        }

        public virtual GameObject PickObject()
        {
            return objectsToSpawn.RandomItem();
        }

        public virtual void InternalSpawnObject(GameObject what, Vector3 where, Quaternion rotation)
        {
            InternalSpawnObject(what, where, rotation);
        }

        //For RPCS!
        public virtual void SpawnObjectActual(GameObject what, Vector3 where, Quaternion rotation)
        {
            Destroy(Instantiate(what, where, rotation),deathTime);
        }
    }
}
