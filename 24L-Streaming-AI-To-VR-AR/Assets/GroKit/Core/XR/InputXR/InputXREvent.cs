namespace Core3lb
{
    public class InputXREvent : BaseInputXREvent
    {
        public InputXR.Controller controller;
        public InputXR.Button buttons = InputXR.Button.Grab;

        public bool useOverride;
        [CoreShowIf("useOverride")]
        public InputReferencesXR overrideReference;

        public override bool GetInput()
        {
            if(useOverride)
            {
                return InputXR.HandleReference(overrideReference, controller, InputXR.InputRequest.Get);
            }
            else
            {
                return InputXR.instance.GetButton(buttons, controller);
            }
        }
    }
}
