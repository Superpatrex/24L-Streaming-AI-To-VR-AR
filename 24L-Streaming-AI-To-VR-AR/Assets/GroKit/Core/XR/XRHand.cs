using UnityEngine;

namespace Core3lb
{
    public class XRHand : MonoBehaviour
    {
        public InputXR.Controller controller;
        public Transform handVisual;
        public Transform controllerVisual;
        public SkinnedMeshRenderer skinnedMesh;
        [Tooltip("DefaultGrabPoint for spawning things in hand")]
        public Transform defaultGrabPoint;
        public bool isGrabbing;
        public BaseXRGrabObject currentHeldObject;
        [HideInInspector]
        public InputProcessorCore GrabProcessor = new InputProcessorCore();
        [HideInInspector]
        public InputProcessorCore InteractProcessor = new InputProcessorCore();



        public void Awake()
        {
            if(defaultGrabPoint == null)
            {
                defaultGrabPoint = transform;
            }
        }

        public void Update()
        {
            GrabProcessor.Process(GetGrab());
            InteractProcessor.Process(GetInteract());
        }

        public bool GetGrab()
        {
            return InputXR.instance.GetGrab(controller);
        }

        public bool GetInteract()
        {
            return InputXR.instance.GetInteract(controller);
        }
    }
}
