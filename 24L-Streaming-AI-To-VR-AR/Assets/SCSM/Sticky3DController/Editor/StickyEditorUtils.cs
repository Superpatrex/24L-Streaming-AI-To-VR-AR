using UnityEditor;
using UnityEngine;

namespace scsmmedia
{
    /// <summary>
    /// This is NOT part of the Sticky3D functionality and should
    /// be used with caution. We've included it as it might be
    /// useful but is AS IS and is NOT SUPPORTED.
    /// </summary>
    public class StickyEditorUtils
    {
        /// <summary>
        /// Tool for reversing AND OVERWRITING selected animations.
        /// Based on work done by Bunning83 and later Straafe.
        /// https://github.com/Straafe/unity-editor-tools
        /// This version OVERWRITES the existing animation.
        /// It is really written for our internal use so use
        /// with caution AT YOUR OWN RISK.
        /// Usage:
        /// 1. Duplicate the animation clip CTRL-D
        /// 2. Select clip, then right-click over top of clip in inspector
        /// 3. Click ReverseAnim
        /// </summary>
        [MenuItem("CONTEXT/AnimationClip/ReverseAnim", false, 0)]
        static void ReverseAnimation(MenuCommand menuCommand)
        {
            AnimationClip clip = (AnimationClip)menuCommand.context;

            if (clip != null)
            {
                float clipLength = clip.length;
                EditorCurveBinding[] curves = AnimationUtility.GetCurveBindings(clip);

                foreach (EditorCurveBinding binding in curves)
                {
                    // Read the curve from the clip
                    AnimationCurve animCurve = AnimationUtility.GetEditorCurve(clip, binding);
                    Keyframe[] keys = animCurve.keys;
                    int keyCount = keys.Length;

                    // Reverse the keys and the tangents
                    for (int i = 0; i < keyCount; i++)
                    {
                        Keyframe K = keys[i];
                        K.time = clipLength - K.time;
                        var tmp = -K.inTangent;
                        K.inTangent = -K.outTangent;
                        K.outTangent = tmp;
                        keys[i] = K;
                    }

                    animCurve.keys = keys;
                    // Write the curve back to the animation clip
                    clip.SetCurve(binding.path, binding.type, binding.propertyName, animCurve);
                }

                AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
                if (events.Length > 0)
                {
                    for (int i = 0; i < events.Length; i++)
                    {
                        events[i].time = clipLength - events[i].time;
                    }
                    AnimationUtility.SetAnimationEvents(clip, events);
                }
            }
        }
    }
}