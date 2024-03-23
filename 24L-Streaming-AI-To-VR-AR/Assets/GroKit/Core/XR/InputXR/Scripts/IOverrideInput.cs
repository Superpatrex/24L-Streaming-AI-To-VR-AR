namespace Core3lb
{
    //This Interface is used to override the Grab/Interact/Menu inputs for InputXR, This will be used for gestures and Networking purposes in the future
    public interface IOverrideInput
    {
        bool IenableOverride { get; set; }
        public bool GetGrab(InputXR.Controller selectedHand);
        public bool GetInteract(InputXR.Controller selectedHand);
        public bool GetAltInteract(InputXR.Controller selectedHand);
        public bool GetMove(InputXR.Controller selectedHand);
        public bool GetMenu(InputXR.Controller selectedHand);
        public void _ChangeOverride(bool chg);
    }
}
