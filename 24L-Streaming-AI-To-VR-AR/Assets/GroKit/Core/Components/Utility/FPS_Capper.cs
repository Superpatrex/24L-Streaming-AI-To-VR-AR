using UnityEngine;

namespace Core3lb
{
    public class FPS_Capper : MonoBehaviour
    {
        [Range(30,120)]
        public int targetFrameRate = 60;
        public void Awake()
        {
            Application.targetFrameRate = 60;
        }
    }
}
