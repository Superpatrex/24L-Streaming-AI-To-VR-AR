using UnityEngine;

namespace Core3lb
{
    public class MetaHapticEvent : MonoBehaviour
    {
        public float frequency = .5f;
        public float amplitude = 1.0f;

        public void _VibrateLeft()
        {
            HapticBuzz(InputXR.Controller.Left,frequency, amplitude);
        }

        public void _VibrateRight()
        {
            HapticBuzz(InputXR.Controller.Right, frequency, amplitude);
        }

        public void _VibrateBoth()
        {
            HapticBuzz(InputXR.Controller.Left, frequency, amplitude);
            HapticBuzz(InputXR.Controller.Right, frequency, amplitude);
        }
        public static void HapticBuzz(InputXR.Controller which,float frequency = .3f, float amplitude = 1)
        {
            if(which == InputXR.Controller.Left)
            {
                OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.LTouch);
            }
            if (which == InputXR.Controller.Right)
            {
                OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.LTouch);
            }
        }
    }
}
