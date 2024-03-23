using UnityEngine;

namespace Core3lb
{
    public class ActivatorSequencer : MonoBehaviour
    {
        public InwardActivator[] Activators;
        public bool runTheirOffEvents;
        public bool loops = true;
        [CoreReadOnly]
        public int currentIndex;
        
        public void _JumpToStep(int chg)
        {
            currentIndex = chg;
            RunActivator();
        }

        protected void RunActivator()
        {
            if(runTheirOffEvents)
            {
                foreach (var item in Activators)
                {
                    item._OffEvent();
                }
            }
            Activators[currentIndex]._OnEvent();
        }

        public void _StepForward()
        {
            currentIndex++;
            if(currentIndex >= Activators.Length)
            {
                if (!loops)
                {
                    return;
                }
                currentIndex = 0;
            }
            RunActivator();
        }

        public void _StepBack()
        {
            currentIndex--;
            if (currentIndex == -1)
            {
                if (!loops)
                {
                    return;
                }
                currentIndex = Activators.Length-1;
            }
            RunActivator();
        }
    }
}
