using UnityEngine;
using UnityEngine.Events;
using System;

namespace Core3lb
{
    public class BaseTrigger : MonoBehaviour
    {
        [CoreEmphasize]
        public OutwardActivator outwardActivator;
        [Space]
        public UnityEvent enterEvent;
        public bool runsExit = false;
        [CoreShowIf("runsExit")]
        public UnityEvent exitEvent;
        //The Simple trigger is used for inheriting the basic trigger functions do not use this script directly
        public bool useTriggerReaction = true;
        public bool debugTrigger = false;

        protected TriggerReaction heldReaction;

        public virtual event Action enterAction;
        public virtual event Action exitAction;


        public virtual void OnTriggerEnter(Collider collision)
        {
            if (useTriggerReaction)
            {
                if (collision.TryGetComponent(out TriggerReaction holder))
                {
                    heldReaction = holder;
                }
                else
                {
                    heldReaction = null;
                }
            }
            if (debugTrigger)
            {
                CoreDebug.LogError("TriggerEnterCalled", collision);
            }
            if (!DoesThisCountAsEntered(collision))
            {
                return;
            }
            if (IsAcceptable(collision))
            {
                TriggerEnterSuccess(collision);
            }
            else
            {
                TriggerEnterFail(collision);
            }
        }

        public virtual bool DoesThisCountAsEntered(Collider collision)
        {
            return true;
        }



        public virtual void TriggerEnterSuccess(Collider collision)
        {
            if (heldReaction)
            {
                heldReaction._TriggerRight();
            }
            _EnterEvent();
        }

        public virtual void TriggerEnterFail(Collider collision)
        {
            if (heldReaction)
            {
                heldReaction._TriggerWrong();
            }
        }

        public virtual void TriggerExitSuccess(Collider collision)
        {
            if (heldReaction)
            {
                heldReaction._ExitTrigger();
            }
            _ExitEvent();
        }


        public virtual void TriggerExitFail(Collider collision)
        {
            if (heldReaction)
            {
                heldReaction._ExitTrigger();
            }
        }

        public virtual void OnTriggerExit(Collider collision)
        {
            if (debugTrigger)
            {
                CoreDebug.LogError("TriggerExitCalled", collision);
            }
            if (useTriggerReaction)
            {
                if (collision.TryGetComponent(out TriggerReaction holder))
                {
                    heldReaction = holder;
                }
                else
                {
                    heldReaction = null;
                }
            }
            if (!DoesThisCountAsExit(collision))
            {
                return;
            }
            if (IsAcceptable(collision, true))
            {
                TriggerExitSuccess(collision);
            }
            else
            {
                TriggerExitFail(collision);
            }
            heldReaction = null;
        }


        public virtual bool DoesThisCountAsExit(Collider collision)
        {
            if (!runsExit)
            {
                if (heldReaction)
                {
                    heldReaction._ExitTrigger();
                }
                heldReaction = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// This will check to see if the item is Accepted by the Trigger
        /// </summary>
        /// <param name="collision"></param>
        /// <returns></returns>
        protected virtual bool IsAcceptable(Collider collision, bool isExit = false)
        {
            return true;
        }


        public virtual void _EnterEvent()
        {
            if (outwardActivator)
            {
                outwardActivator._OnEvent();
            }
            enterAction?.Invoke();
            enterEvent.Invoke();
        }


        public virtual void _ExitEvent()
        {
            if (outwardActivator)
            {
                outwardActivator._OffEvent();
            }
            exitAction?.Invoke();
            exitEvent.Invoke();
        }

        public virtual void _Reset()
        {
            heldReaction = null;
        }

        //These are for the Running Events in the Editor
        [CoreButton("Enter Event")]
        private void EnterEventClicker()
        {
            _EnterEvent();
        }

        [CoreButton("Exit Event")]
        private void ExitEventClicker()
        {
            _ExitEvent();
        }
    }
}