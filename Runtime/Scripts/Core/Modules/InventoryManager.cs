using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using static core.GameManager;
using System.IO;

namespace core.modules
{
    public class InventoryManager : BaseModule
    {
        private bool m_inventoryIsLoaded = false;
        private List<InventoryItem> inventoryData = new List<InventoryItem>();

        public override void onInitialize()
        {
            LoadInventoryData();
        }

        private void LoadInventoryData() {
            InventoryItem[] inventoryItems = Resources.LoadAll<InventoryItem>("Items");

            if(inventoryItems.Length > 0) {
                inventoryData.AddRange(inventoryItems);
            }

            m_inventoryIsLoaded = true;
        }

        public void SpawnItemByID(string itemID) {
            if(!m_inventoryIsLoaded) {
                Debug.LogError("Inventory module not initialized!");
            }

            InventoryItem _item = GetItemByID(itemID);

            if(!_item) {
                Debug.LogError("Can't spawn unknown item (" + itemID + ")");
            }
            else {
                // GameObject _itemGameObject = UnityEngine.Object.Instantiate(m_NodePrefab);
                // Spawn item from pool system if available. Otherwise spawn item directly
                Debug.LogWarning("Item spawning not implemented! (" + _item.itemID + ")");
            }
        }

        public InventoryItem GetItemByID(string itemID) {
            if(!m_inventoryIsLoaded) {
                Debug.LogError("Inventory module not initialized!");
                return null;
            }

            return inventoryData.FirstOrDefault(item => item.itemID == itemID);
        }

        public override void UpdateModule(float deltaTime, float unscaledDeltaTime)
        {

        }
    }
}