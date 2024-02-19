
namespace Core3lb
{
    public class MetaGestureEvent : BaseInputXREvent
    {
        [CoreHeader("Settings")]
        public MetaGestureInput.eHandType whichHand = MetaGestureInput.eHandType.leftHand;
        public MetaGestureInput.eGestureType gestureType = MetaGestureInput.eGestureType.Grab;

        public override bool GetInput()
        {
            return MetaGestureInput.instance.GetInput(whichHand, gestureType);
        }
    }
}
