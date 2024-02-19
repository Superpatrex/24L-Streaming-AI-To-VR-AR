using UnityEngine;
namespace Core3lb
{
    public class OutwardActivator : BaseActivator
    {
        [CoreHeader("Also Run These")]
        [CoreEmphasize]
        public InwardActivator inwardActivator;
        public InwardActivator[] extraActivators;

        public void DealWithInWards(ActivatorEvents whichEvent)
        {
            if(inwardActivator != null)
            {
                inwardActivator.CallEvent(whichEvent);
            }
            foreach(var activator in extraActivators)
            {
                activator.CallEvent(whichEvent);
            }
        }

        public override void _OnEvent()
        {
            DealWithInWards(ActivatorEvents.OnEvent);
            base._OnEvent();
        }

        public override void _OffEvent()
        {
            DealWithInWards(ActivatorEvents.OffEvent);
            base._OffEvent();
        }

        public override void _OnSecondaryEvent()
        {
            DealWithInWards(ActivatorEvents.OnSecondaryEvent);
            base._OnSecondaryEvent();
        }

        public override void _OffSecondaryEvent()
        {
            DealWithInWards(ActivatorEvents.OnSecondaryEvent);
            base._OffSecondaryEvent();
        }

        public override void _RunResetEvent()
        {
            DealWithInWards(ActivatorEvents.OnReset);
            base._RunResetEvent();
        }
    }
}
