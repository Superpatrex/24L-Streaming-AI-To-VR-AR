using UnityEngine;

namespace Core3lb
{
    public class SpecificCollidersTrigger : AdvancedTrigger
    {
        [CoreEmphasize]
        public Collider selectedCollider;
        public bool andTheseExtraOnes;
        [CoreShowIf("andTheseExtraOnes")]
        public Collider[] extraColliders;

        /// <summary>
        /// This will check to see if the item is Accepted by the Trigger
        /// </summary>
        /// <param name="collision"></param>
        /// <returns></returns>
        protected override bool IsAcceptable(Collider collision, bool isExit = false)
        {
            if (collision == selectedCollider)
            {
                return true;
            }
            foreach (Collider collider in extraColliders)
            {
                if (collision == selectedCollider)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
