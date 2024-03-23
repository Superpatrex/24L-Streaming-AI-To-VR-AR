using UnityEngine;

namespace Core3lb
{
    public class BaseMultiSpawner : BaseSpawner
    {
        public int index;
        public bool doinOrder;
        public bool doAll;

        public override GameObject PickObject()
        {
            if(doinOrder)
            {
                index++;
            }
            return base.PickObject();
        }

        public override void InternalSpawnObject(GameObject what, Vector3 where, Quaternion rotation)
        {
            if(doAll)
            {
                foreach (GameObject t in objectsToSpawn)
                {
                    InternalSpawnObject(t, Position, Rotation);
                }
            }
            else
            {
                base.SpawnObjectActual(what, where, rotation);
            }
        }
    }
}
