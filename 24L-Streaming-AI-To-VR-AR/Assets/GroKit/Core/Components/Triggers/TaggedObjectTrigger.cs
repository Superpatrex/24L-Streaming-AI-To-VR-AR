using UnityEngine;

namespace Core3lb
{
    public class TaggedObjectTrigger : AdvancedTrigger
    {
        [CoreHideIf("takesCollider")]
        public string searchTag = "Player";
        [CoreHideIf("takesCollider")]
        public string[] additionalTags;

        /// <summary>
        /// This will check to see if the item is Accepted by the Trigger
        /// </summary>
        /// <param name="collision"></param>
        /// <returns></returns>
        protected override bool IsAcceptable(Collider collision, bool isExit = false)
        {
            if (collision.CompareTag(searchTag))
            {
                return true;
            }
            for (int i = 0; i < additionalTags.Length; i++)
            {
                if (collision.CompareTag(additionalTags[i]))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
