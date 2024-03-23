using System;
using UnityEngine;

namespace Core3lb
{
    public class AnimationHolder : MonoBehaviour
    {
        public Animator setAnimator;

        public AnimationTask[] theTasks;

        [Serializable]
        public class AnimationTask
        {
            //this is for the animator controller
            public string param;
            public float floatChange;
        }

        public void _AnimateTaskBoolTrue(int chg)
        {
            setAnimator.SetBool(theTasks[chg].param, true);
            //Debug.LogError("Set-" +theTasks[chg].param + "-to true");
        }

        public void _AnimateTaskBoolToggle(int chg)
        {
            setAnimator.SetBool(theTasks[chg].param, !setAnimator.GetBool(theTasks[chg].param));
        }

        public void _AnimateTaskBoolFalse(int chg)
        {
            setAnimator.SetBool(theTasks[chg].param, false);
            //Debug.LogError("Set-" + theTasks[chg].param + "-to false");
        }

        public void _AnimateTaskTrigger(int chg)
        {
            setAnimator.SetTrigger(theTasks[chg].param);
        }

        public void _AnimateTaskFloat(int chg)
        {
            setAnimator.SetFloat(theTasks[chg].param, theTasks[chg].floatChange);
        }
    }
}