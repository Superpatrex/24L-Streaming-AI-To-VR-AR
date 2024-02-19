using UnityEngine;

namespace Core3lb
{
    public class PositionerTransforms : PositionerBase
    {
        public Transform[] whatTransforms;
        [SerializeField] bool doInOrder;
        int index = 0;

        public override Vector3 WhatPosition(Transform whereNow = null)
        {
            if(doInOrder)
            {
                int currentindex = index;
                index++;
                if(index == whatTransforms.Length)
                {
                    index = 0;
                }
                return whatTransforms[currentindex].position;
            }
            else
            {
                return whatTransforms.RandomItem().position;
            }
        }

        public Vector3 WhatPositionIndex(int chg)
        {
            return whatTransforms[index].position;
        }

        public Quaternion WhatRotationIndex(int chg)
        {
           return whatTransforms[index].rotation;
        }
        public override Quaternion WhatRotation(Transform whereNow = null)
        {
            if(randomiseRotation)
            {
                base.WhatRotation(whereNow);
            }
            return whatTransforms[index].rotation;
        }
    }
}
