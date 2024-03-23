using UnityEngine;
using UnityEngine.Events;

namespace Core3lb
{
    public class XRInteract : BaseTrigger
    {
        public UnityEvent interact;
        protected XRHand currentHand;

        protected bool wasUsed = false;
        public void Update()
        {
            if (currentHand)
            {
                if (currentHand.GetGrab())
                {
                    Interact();
                }
                if (!currentHand.GetGrab())
                {
                    wasUsed = false;
                }
            }
        }


        public virtual void Interact()
        {
            if (wasUsed)
            {
                return;
            }
            wasUsed = true;
            interact.Invoke();
        }

        protected override bool IsAcceptable(Collider collision, bool isExit = true)
        {
            if (collision.TryGetComponent(out XRHand holder))
            {
                if(isExit)
                {
                    if(currentHand == holder)
                    {
                        currentHand = null;
                    }
                }
                else
                {
                    currentHand = holder;
                }
                return true;
            }
            return false;
        }
    }
}
