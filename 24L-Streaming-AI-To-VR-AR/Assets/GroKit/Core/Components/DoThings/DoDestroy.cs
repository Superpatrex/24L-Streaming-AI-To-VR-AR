using UnityEngine;

namespace Core3lb
{
    public class DoDestroy : MonoBehaviour
    {
        public float destroyTime = 0;

        public virtual void _DestroySelf()
        {
            DestroyInternal(gameObject);
        }

        public virtual void _DestroyObject(GameObject go)
        {
            DestroyInternal(go);
        }

        protected void DestroyInternal(GameObject go)
        {
            Destroy(go,destroyTime);
        }
    }
}
