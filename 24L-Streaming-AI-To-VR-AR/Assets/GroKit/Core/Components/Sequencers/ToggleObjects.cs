using UnityEngine;

namespace Core3lb
{
    public class ToggleObjects : MonoBehaviour
    {
        public GameObject[] ThingsToToggle;
        public GameObject[] ThingsToToggleInverse;
        public bool isOn = true;
        public bool useDelay;

        [CoreReadOnly]
        public int curIndex;

        public void _ToggleOneObject(int index)
        {
            if (useDelay)
            {
                ThingsToToggle.SetArrayGO(false);
            }
            else
            {
                StartCoroutine(ThingsToToggle.SetArrayGOWithDelay(false));
            }

            ThingsToToggle[index].SetActive(true);
            curIndex = index;
        }

        public void _ToggleThings()
        {
            isOn = !isOn;
            ToggleObjectIternal(isOn, !isOn);

        }

        public void _TurnThingsOn()
        {
            ToggleObjectIternal(true, false);
        }

        public void _TurnThingsOff()
        {
            ToggleObjectIternal(false, true);
        }

        protected void ToggleObjectIternal(bool array1, bool array2)
        {
            if (useDelay)
            {
                StartCoroutine(ThingsToToggle.SetArrayGOWithDelay(array1));
                StartCoroutine(ThingsToToggleInverse.SetArrayGOWithDelay(array2));
            }
            else
            {
                ThingsToToggle.SetArrayGO(array1);
                ThingsToToggleInverse.SetArrayGO(array2);
            }
        }


    }
}
