
using UnityEngine;

namespace Core3lb
{
    public class DEBUG_ClickerEditor : MonoBehaviour
    {
        public AdvancedTrigger myTrigger;
        public BaseActivator myHolder;
        [Space]
        public Transform moveTo;
        public Transform objectToMove;
        public UnityEngine.Events.UnityEvent testEventQuick;



        [CoreButton("Do UnityEvent")]
        void DoTestEvent()
        {
            testEventQuick.Invoke();
        }

        [CoreButton("Do Trigger")]
        void DoTrigger()
        {
            if (myTrigger)
            {
                myTrigger.enterEvent.Invoke();
            }
            else
            {
                GetComponent<AdvancedTrigger>().enterEvent.Invoke();
            }
        }

        [CoreButton("Do Activator On")]
        void DoActivatorOn()
        {
            if (myHolder)
            {
                myHolder._OnEvent();
            }
        }
        [CoreButton("Do Activator Off")]
        void DoActivatorOff()
        {
            if (myHolder)
            {
                myHolder._OffEvent();
            }
        }

        [CoreButton]
        void MoveObject()
        {
            objectToMove.transform.position = moveTo.position;
        }

        [CoreButton]
        void RotateTo()
        {
            objectToMove.transform.rotation = moveTo.rotation;
        }
    }
}
