using UnityEngine;
using System.Collections.Generic;


namespace SlimeRpgEvolution2D.Data
{

    [System.Serializable]
    public struct WeaponSaveData
    {
        public string weaponID;
        public int currentLevel;

        public WeaponSaveData(string id, int level)
        {
            weaponID = id;
            currentLevel = level;
        }
    }

    [System.Serializable]
    public struct InventoryItemSaveData
    {
        public string itemID;
        public int amount;

        public InventoryItemSaveData(string id, int count)
        {
            itemID = id;
            amount = count;
        }
    }

    [System.Serializable]
    public class GameSaveData
    {
        [SerializeField] private int _coins;
        public int Coins => _coins;

        [SerializeField] private List<WeaponSaveData> _weapons = new List<WeaponSaveData>();
        public IReadOnlyList<WeaponSaveData> Weapons => _weapons;

        [SerializeField] private List<InventoryItemSaveData> _inventoryItems = new List<InventoryItemSaveData>();

        public IReadOnlyList<InventoryItemSaveData> InventoryItems => _inventoryItems;

        [SerializeField] private bool _isInventoryUnlocked;
        public bool IsInventoryUnlocked => _isInventoryUnlocked;

        public string selectedCharacterID = "DefaultPlayer";

        public GameSaveData()
        {
            _coins = 0;
            selectedCharacterID = "DefaultPlayer";
            _weapons = new List<WeaponSaveData>();
            _inventoryItems = new List<InventoryItemSaveData>();
            _isInventoryUnlocked = false;
        }

        public void UpdateCoins(int amount, DataManager.AccessKey key)
        {
            if (key == null) return;
            _coins = amount;
        }

        public void UpdateWeapons(List<WeaponSaveData> weaponList, DataManager.AccessKey key)
        {
            if (key == null) return;
            _weapons = weaponList;
        }

        public void UpdateInventory(List<InventoryItemSaveData> itemList, DataManager.AccessKey key)
        {
            if(key == null) return;
            _inventoryItems = itemList;
        }

        public void UpdateInventoryUnlockStatus(bool isUnlocked, DataManager.AccessKey key)
        {
            if (key == null) return;
            _isInventoryUnlocked = isUnlocked;
        }
    }    
}
