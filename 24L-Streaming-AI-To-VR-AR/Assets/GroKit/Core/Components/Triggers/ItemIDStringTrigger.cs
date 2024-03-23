using UnityEngine;

namespace Core3lb
{
    public class ItemIDStringTrigger : AdvancedTrigger
    {
        public string itemID = ItemIDString.ANY;
        public string groupID = ItemIDString.ANY;



        protected override bool IsAcceptable(Collider collision, bool isExit = false)
        {
            if (collision.TryGetComponent(out ItemIDString itemtest))
            {
                if(debugTrigger)
                {
                    Debug.LogError($"Colliders item IDs are {itemtest.itemID} | {itemtest.groupID} ", gameObject);
                }
                if(itemtest.itemID == itemID)
                {
                    if(itemtest.groupID == groupID)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
