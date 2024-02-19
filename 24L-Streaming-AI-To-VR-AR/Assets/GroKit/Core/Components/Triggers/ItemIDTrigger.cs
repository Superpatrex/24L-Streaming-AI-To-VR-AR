using UnityEngine;

namespace Core3lb
{
    public class ItemIDTrigger : AdvancedTrigger
    {
        [CoreHeader("ItemIDTrigger")]
        public ItemIDConstants.ItemID acceptedItemID;
        public ItemIDConstants.GroupID groupID = ItemIDConstants.GroupID.Any;

        protected override bool IsAcceptable(Collider collision, bool isExit = false)
        {
            if(collision.TryGetComponent(out ItemID itemtest))
            {
                if (debugTrigger)
                {
                    Debug.LogError($"Colliders item IDs are {itemtest.itemID} | {itemtest.groupID} ", gameObject);
                }
                return ItemID.CheckItemID(itemtest,acceptedItemID,groupID);
            }
            return false;
        }
    }
}
