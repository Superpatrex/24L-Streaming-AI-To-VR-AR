using UnityEngine;

namespace Core3lb
{
    public class MultiStateInwards : MonoBehaviour
    {
        public InwardActivator[] activators;

        public bool runResetOnAll;

        public void _RunInwardOn(int index)
        {
            if(runResetOnAll)
            {
                _ResetAll();
            }
            activators[index]._OnEvent();
        }

        public void _RunInwardOff(int index)
        {
            if (runResetOnAll)
            {
                _ResetAll();
            }
            activators[index]._OffEvent();
        }

        public virtual void _AllOff()
        {
            foreach (var activator in activators)
            {
                activator._OffEvent();
            }
        }

        public virtual void _AllOn()
        {
            foreach (var activator in activators)
            {
                activator._OnEvent();
            }
        }

        public virtual void _ResetAll()
        {
            foreach (var activator in activators)
            {
                activator._RunResetEvent();
            }
        }
    }
}
