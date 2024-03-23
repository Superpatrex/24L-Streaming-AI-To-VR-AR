using UnityEngine;

namespace Core3lb
{
    public class XRHandTrigger : AdvancedTrigger
    {
        [CoreHeader("XR HandTrigger")]
        public bool onlyAcceptPokers;
        [HideInInspector]
        public XRHand currentHand;
        [HideInInspector]
        public XRPoker curPoker;
        protected override bool IsAcceptable(Collider collision, bool isExit = false)
        {
            if (collision.TryGetComponent(out XRPoker thisPoke))
            {
                if (isExit)
                {
                    curPoker = null;
                }
                else
                {
                    curPoker = thisPoke;
                }
                return true;
            }
            else if (onlyAcceptPokers)
            {
                return false;
            }
            if (collision.TryGetComponent(out XRHand thisHand))
            {
                if (isExit)
                {
                    currentHand = null;
                }
                else
                {
                    currentHand = thisHand;
                }
                return true;
            }
            return false;
            //return base.IsAcceptable(collision, isExit);
        }
    }
}
