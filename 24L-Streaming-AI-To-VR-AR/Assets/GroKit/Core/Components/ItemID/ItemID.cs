using UnityEngine;

namespace Core3lb
{
    public class ItemID : MonoBehaviour
    {
        public ItemIDConstants.ItemID itemID = ItemIDConstants.ItemID.Any;
        public ItemIDConstants.GroupID groupID = ItemIDConstants.GroupID.Any;

        public static bool CheckItemID(ItemID currentID, ItemIDConstants.ItemID acceptedItemID, ItemIDConstants.GroupID groupID = ItemIDConstants.GroupID.Any)
        {
            if (groupID == ItemIDConstants.GroupID.Any)
            {
                if (acceptedItemID == ItemIDConstants.ItemID.Any)
                {
                    Debug.LogError("THIS NOT");
                    return true;
                }
                else if (acceptedItemID == currentID.itemID)
                {
                    Debug.LogError("THIS NOT");
                    return true;
                }
            }
            else
            {
                if (groupID == currentID.groupID)
                {
                    if (acceptedItemID == ItemIDConstants.ItemID.Any)
                    {
                        return true;
                    }
                    else if (acceptedItemID == currentID.itemID)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
