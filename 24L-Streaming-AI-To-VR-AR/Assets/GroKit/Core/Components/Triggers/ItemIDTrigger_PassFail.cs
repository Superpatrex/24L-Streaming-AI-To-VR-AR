using UnityEngine;
using UnityEngine.Events;

namespace Core3lb
{
    public class ItemIDTrigger_PassFail : ItemIDTrigger
    {

        [CoreHeader("Fail Events")]
        [Tooltip("This will trigger the outward activator Secondary On")]
        public UnityEvent triggerIncorrectEnter;
        public bool hasExitFail;
        [CoreShowIf("hasExitFail")]
        [Tooltip("This will trigger the outward activator Secondary Off")]
        public UnityEvent triggerIncorrectExit;

        public override void TriggerEnterFail(Collider collision)
        {

            if(isWrongItem(collision))
            {
                if (outwardActivator)
                {
                    outwardActivator._OnSecondaryEvent();
                }
                triggerIncorrectEnter.Invoke();
            }

            base.TriggerEnterFail(collision);
        }

        public override void TriggerExitFail(Collider collision)
        {
            if (isWrongItem(collision))
            {
                if (outwardActivator)
                {
                    outwardActivator._OffSecondaryEvent();
                }
                triggerIncorrectExit.Invoke();
            }

            base.TriggerExitFail(collision);
        }


        protected virtual bool isWrongItem(Collider collision, bool isExit = false)
        {
            if(collision.TryGetComponent(out ItemID itemtest))
            {
                return true;
            }
            return false;
        }
    }
}
