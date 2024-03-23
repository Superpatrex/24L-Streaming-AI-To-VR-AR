using UnityEngine;
using UnityEngine.Events;

namespace Core3lb
{
    public class FXPlayer : MonoBehaviour
    {
        public bool isParented = true;
        Transform yourParent;
        public float deathTime;
        public UnityEvent FXEvent;

        void Init()
        {
            if (isParented)
            {
                yourParent = transform.parent;
            }
            transform.SetParent(null);
        }

        public void _PlayFX()
        {
            if (!yourParent)
            {
                Init();
            }
            gameObject.SetActive(true);
            FXEvent.Invoke();
            if (deathTime > 0)
            {
                Destroy(gameObject, deathTime);
            }
        }

        public void _PlayFXAt()
        {
            if (!yourParent)
            {
                Init();
            }
            transform.SetPositionAndRotation(yourParent.position, yourParent.rotation);
            gameObject.SetActive(true);
            FXEvent.Invoke();
            if (deathTime > 0)
            {
                Destroy(gameObject, deathTime);
            }
        }
        public void _PlayFXAt(Transform loc)
        {
            if (!yourParent)
            {
                Init();
            }
            transform.SetPositionAndRotation(loc.position, loc.rotation);
            gameObject.SetActive(true);
            FXEvent.Invoke();
            if (deathTime > 0)
            {
                Destroy(gameObject, deathTime);
            }
        }

        public void _PlayFXAt(Vector3 pos)
        {
            if (!yourParent)
            {
                Init();
            }
            transform.position = pos;
            gameObject.SetActive(true);
            FXEvent.Invoke();
            if (deathTime > 0)
            {
                Destroy(gameObject, deathTime);
            }
        }
    }
}
