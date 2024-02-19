using UnityEngine;

namespace Core3lb
{
    public class DEBUG_Disable : MonoBehaviour
    {
        public bool hideMeshOnAwake;
        public bool disableInBuild;

        private void Awake()
        {
            if (disableInBuild && !Application.isEditor)
            {
                gameObject.SetActive(false);
            }
            if (hideMeshOnAwake && gameObject.GetComponent<Renderer>())
            {
                gameObject.GetComponent<Renderer>().enabled = false;
            }
        }
    }
}