using UnityEngine;


namespace Core3lb
{
    public class XRPlayerManager : Singleton<XRPlayerManager>
    {
        [CoreRequired]
        public Camera head;
        [CoreRequired]
        public XRHand leftHand;
        [CoreRequired]
        public XRHand rightHand;
    }
}
