using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Core3lb
{
    public class EventOnStart : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent awakeEvent;
        [SerializeField]
        private UnityEvent startEvent;
        [SerializeField]
        private UnityEvent delayStartEvent;
        public float delay = 0.2f;

        private void Awake()
        {
            awakeEvent.Invoke();
        }

        IEnumerator Start()
        {
            startEvent.Invoke();
            yield return new WaitForSeconds(delay);
            delayStartEvent.Invoke();
        }
    }
}
