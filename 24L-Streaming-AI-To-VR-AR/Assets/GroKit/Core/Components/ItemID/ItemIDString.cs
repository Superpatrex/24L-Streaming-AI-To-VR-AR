using UnityEngine;

namespace Core3lb
{
    public class ItemIDString : MonoBehaviour
    {
        public const string ANY = "ANY";
        [Tooltip("Use strings to identify themselves.")]
        public string itemID = ANY; 
        public string groupID = ANY;
        [Tooltip("Description can be pulled by other scripts")]
        public string DisplayName;
        [TextArea]
        public string Description;
    }
}
