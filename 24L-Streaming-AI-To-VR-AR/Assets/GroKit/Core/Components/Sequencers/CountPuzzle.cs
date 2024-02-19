using UnityEngine;
using UnityEngine.Events;

namespace Core3lb
{
    public class CountPuzzle : MonoBehaviour
    {
        public int maxCount = 3;
        [CoreReadOnly]
        public int curCount;
        [CoreReadOnly]
        public bool isDone;

        [Header("Events")]
        public UnityEvent add;
        public UnityEvent subtract;
        public UnityEvent complete;
        public UnityEvent resetEvent;

        public bool isReversible;
        [CoreShowIf("isReversible")]
        public UnityEvent unComplete;

        public void _Add()
        {
            if (isDone)
            {
                return;
            }
            add.Invoke();
            curCount++;
            if (curCount >= maxCount)
            {
                _Complete();
            }
        }

        public void _Subtract()
        {
            if (isDone)
            {
                if (isReversible)
                {
                    _UnComplete();
                    isDone = false;
                }
                else
                {
                    return;
                }
            }
            curCount--;
            if (curCount <= 0)
            {
                curCount = 0;
            }
            subtract.Invoke();
        }

        public virtual void _UnComplete()
        {
            unComplete.Invoke();
        }

        public virtual void _Complete()
        {
            isDone = true;
            complete.Invoke();
        }

        public virtual void _DoReset()
        {
            curCount = 0;
            isDone = false;
            resetEvent.Invoke();
        }
    }
}
