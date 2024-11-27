using UnityEngine;

namespace core.modules
{
    [CreateAssetMenu(fileName = "New Base Item", menuName = "Inventory/Items")]
    public class InventoryItem : ScriptableObject
    {
        public string itemID;
        public string itemName;
        public string itemDescription;
        public string itemPrefabPath;
    }
}