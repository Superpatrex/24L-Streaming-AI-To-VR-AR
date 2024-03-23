using UnityEngine;

namespace Core3lb
{
    public class DoParentThings : MonoBehaviour
    {
        [CoreToggleHeader("On Start")]
        public bool onStart;

        [CoreShowIf("onStart")]
        [Tooltip("DoAwake Instead of Start")]
        public bool awakeInstead;
        [CoreShowIf("onStart")]
        public bool unparent;
        [CoreShowIf("onStart")]
        public bool parent;
        [CoreShowIf("parent")]
        public bool resetPosition;
        [CoreShowIf("parent")]
        public bool resetRotation;


        Vector3 positionOffset;
        public Transform parentTo;

        public void Awake()
        {
            if(awakeInstead)
            {
                ParentAtBeginning();
            }
        }

        private void Start()
        {
            if(onStart && awakeInstead == false)
            {
                ParentAtBeginning();
            }
        }

        public void ParentAtBeginning()
        {
            if (unparent)
            {
                transform.parent = null;
            }
            if (parent)
            {
                if (parentTo)
                {
                    Parenting(parentTo);
                    if (resetPosition)
                    {
                        transform.localPosition = Vector3.zero + positionOffset;
                    }
                    if (resetRotation)
                    {
                        transform.localEulerAngles = Vector3.zero;
                    }
                }
            }
        }

       

        void Parenting(Transform which)
        {
            transform.parent = which.transform;
        }

        public void _Parent(Transform which)
        {
            Parenting(which);
            transform.localPosition = Vector3.zero + positionOffset;
            transform.localEulerAngles = Vector3.zero;
        }

        public void _ParentNoReset(Transform which)
        {
            Parenting(which);
        }

        public void _Parent()
        {
            Parenting(parentTo);
            transform.localPosition = Vector3.zero + positionOffset;
            transform.localEulerAngles = Vector3.zero;
        }

        public void _ParentNoReset()
        {
            Parenting(parentTo);
        }

        public void _Unparent()
        {
            transform.parent = null;
        }
    }
}
