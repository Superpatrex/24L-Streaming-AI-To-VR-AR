using UnityEngine;

namespace Core3lb
{
    public class PlayerTrigger : BaseTrigger
    {
        protected override bool IsAcceptable(Collider collision, bool isExit = false)
        {
            if (collision.CompareTag("Player"))
            {
                return true;
            }
            return false;
        }
    }
}
