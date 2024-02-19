
using UnityEngine;
using UnityEngine.Events;

namespace Core3lb
{
    public class DEBUG_EventTester : MonoBehaviour
    {
        public UnityEvent event1;
        [CoreButton("Event 1",true)]
        public void Event1()
        {
            event1.Invoke();    
        }

        public UnityEvent event2;
        [CoreButton("Event 2", true)]
        public void Event2()
        {
            event2.Invoke();
        }

        public UnityEvent event3;
        [CoreButton("Event 3", true)]
        public void Event3()
        {
            event3.Invoke();
        }
    }
}
