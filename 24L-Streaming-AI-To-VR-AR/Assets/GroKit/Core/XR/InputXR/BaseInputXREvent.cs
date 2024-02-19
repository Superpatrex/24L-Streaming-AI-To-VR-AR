using UnityEngine;
using UnityEngine.Events;

namespace Core3lb
{
    public class BaseInputXREvent : MonoBehaviour
    {
        public bool isOverriden = false;
        public UnityEvent onGetDown;
        public UnityEvent onGetUp;

        public UnityEvent onDoubleTap; // Event for double tap


        public bool debugInput = false;

        protected bool isInputUsed = false;
        [CoreReadOnly]
        public InputProcessorCore inputProcessor = new InputProcessorCore();
        // Variables for double tap detection
        protected float lastTapTime = 0f;
        protected float doubleTapThreshold = 0.5f; // Time allowed between taps

        protected virtual void Update()
        {
            if(isOverriden)
            {
                return;
            }
            inputProcessor.Process(GetInput());
            if (inputProcessor.isDown)
            {
                onGetDown.Invoke();
            }
            if (inputProcessor.isUp)
            {
                onGetUp.Invoke();
            }
            if (inputProcessor.isDoubleTap)
            {
                onDoubleTap.Invoke();
            }
        }

        public virtual bool GetInput()
        {
            Debug.LogError("GetInput() not implemented");
            return false;   
        }
    }
}
