using UnityEngine;
using UnityEngine.Events;
namespace Core3lb
{
    public class ToggleEvents : MonoBehaviour
    {
        public UnityEvent onEvent;
        public UnityEvent offEvent;
        public bool isOn = false;
        public bool onlyRunOnChange;

        public OutwardActivator outwardActivator;

        public void _ToggleEvent()
        {
            if (!isOn)
            {
                _ToggleOn();
            }
            else
            {
                _ToggleOff();
            }
        }

        public void _ToggleOn()
        {
            if(onlyRunOnChange && isOn)
            {
                return;
            }
            if (outwardActivator) outwardActivator._OnEvent();
            onEvent.Invoke();
            isOn = true;
        }

        public void _ToggleOff()
        {
            if (onlyRunOnChange && !isOn)
            {
                return;
            }
            if (outwardActivator) outwardActivator._OffEvent();
            offEvent.Invoke();
            isOn = false;
        }
    }
}
