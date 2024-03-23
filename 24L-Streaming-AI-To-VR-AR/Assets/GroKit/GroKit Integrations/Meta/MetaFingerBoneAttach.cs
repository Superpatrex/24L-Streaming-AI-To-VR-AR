using Oculus.Interaction.Input;
using UnityEngine;

namespace Core3lb
{
    public class MetaFingerBoneAttach : MonoBehaviour
    {
        public Vector3 posOffset;
        public Vector3 rotOffset;
        public HandJointId handJoint;
        public MetaGestureInput.eHandType whichHand;
        [CoreReadOnly]
        public Transform jointTransform;

        public void Start()
        {
            DoParenting();
        }

        public void DoParenting()
        {
            Transform holdPosition = MetaGestureInput.instance.GetHandBone(handJoint, whichHand);
            transform.position = holdPosition.position + posOffset;
            transform.eulerAngles = holdPosition.eulerAngles + rotOffset;
            transform.parent = holdPosition;
        }

        public void DeParent()
        {
            transform.parent = null;
        }
    }
}
