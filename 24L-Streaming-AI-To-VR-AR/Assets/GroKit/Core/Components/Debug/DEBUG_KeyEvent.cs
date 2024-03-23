using UnityEngine;
using UnityEngine.Events;

namespace Core3lb
{
    public class DEBUG_KeyEvent : MonoBehaviour
    {
        public KeyCode key1 = KeyCode.Q;
        public UnityEvent key1Event;

        public KeyCode key2 = KeyCode.E;
        public UnityEvent key2Event;

        public KeyCode key3 = KeyCode.F;
        public UnityEvent key3Event;

        public KeyCode key4 = KeyCode.R;
        public UnityEvent key4Event;

        void Update()
        {
            if (Input.GetKeyDown(key1))
            {
                key1Event.Invoke();
            }

            if (Input.GetKeyDown(key2))
            {
                key2Event.Invoke();
            }

            if (Input.GetKeyDown(key3))
            {
                key3Event.Invoke();
            }

            if (Input.GetKeyDown(key4))
            {
                key4Event.Invoke();
            }
        }
    }
}
