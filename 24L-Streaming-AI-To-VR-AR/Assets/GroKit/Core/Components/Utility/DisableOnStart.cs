using UnityEngine;

namespace Core3lb
{
    public class DisableOnStart : MonoBehaviour
    {
        public enum eDisableWhen { OnStart, AfterStart, OnAwake };
        public eDisableWhen disableWhen = eDisableWhen.OnStart;

        void Awake()
        {
            if (disableWhen == eDisableWhen.OnAwake)
            {
                TurnOff();
            }
        }
        void Start()
        {
            if (disableWhen == eDisableWhen.AfterStart)
            {
                CoreExtensions.OnEndOfFrame(this, TurnOff);
            }
            if (disableWhen == eDisableWhen.OnStart)
            {
                TurnOff();
            }
        }

        public void TurnOff()
        {
            gameObject.SetActive(false);
        }

    }
}
