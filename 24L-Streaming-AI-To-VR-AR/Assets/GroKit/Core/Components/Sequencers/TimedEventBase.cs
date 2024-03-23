using UnityEngine;
using UnityEngine.Events;

namespace Core3lb
{
    public class TimedEventBase : MonoBehaviour
    {
        //[FancyHeader("Test")]
        public bool isActive;
        //[HideIf("useRandomInterval")]
        public float interval;
        //public bool useRandomInterval;
        //[ShowIf("useRandomInterval")]
        //public Vector2 randomIntervals;
        //CSW - random interval shouldn't be in BASE. Also, it wasn't doing anything.
        [CoreReadOnly]
        public float timer;


        public UnityEvent complete;
        public bool runEventOnStart;
        [CoreShowIf("runEventOnStart")]
        public UnityEvent start;
        public bool runEventOnReset;
        [CoreShowIf("runEventOnReset")]
        public UnityEvent reset;

        public virtual void FixedUpdate()
        {
            if (isActive)
            {
                if (EvaluateTime())
                {
                    _RunEvent();
                    TimerReached();
                }
            }
        }

        public virtual void _RunEvent()
        {
            complete.Invoke();
        }

        public virtual void TimerReached()
        {
            _Stop();
        }

        public virtual bool EvaluateTime()
        {
            timer += Time.deltaTime;
            return timer > interval;
        }

        public virtual void _Start()
        {
            start.Invoke();
            isActive = true;
        }

        public virtual void _Stop()
        {
            isActive = false;
        }

        public virtual void _ResetTime()
        {
            if(runEventOnReset)
            {
                reset.Invoke();
            }
            timer = 0;
        }

        public virtual void _RestartTimer()
        {
            _ResetTime();
            _Start();
        }

        public virtual void _StopAndReset()
        {
            _Stop();
            _ResetTime();
        }

    }
}
