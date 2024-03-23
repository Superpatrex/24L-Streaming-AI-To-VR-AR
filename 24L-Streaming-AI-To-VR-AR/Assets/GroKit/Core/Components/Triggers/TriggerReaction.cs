using UnityEngine;
using UnityEngine.Events;
using System;

namespace Core3lb
{
    public class TriggerReaction : MonoBehaviour
    {
        [TextArea]
        public string notes;
        public UnityEvent triggerRight;
        public UnityEvent triggerWrong;
        public bool alwaysRunOnExit;
        public UnityEvent onExitEvent;

        public event Action triggerRightAction;
        public event Action triggerWrongAction;
        public event Action onExitAction;


        [CoreButton("OnExit")]
        public virtual void _ExitTrigger()
        {
            onExitAction?.Invoke();
            onExitEvent.Invoke();
        }

        [CoreButton("Trigger Right")]
        public void _TriggerRight()
        {
            triggerRightAction?.Invoke();
            triggerRight.Invoke();
        }

        [CoreButton("Trigger Wrong")]
        public void _TriggerWrong()
        {
            triggerWrongAction?.Invoke();
             triggerWrong.Invoke();
        }
    }
}
