using System;
using System.Collections.Generic;
using UnityEngine;


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
    public struct WorldSaveData
    {
        public string worldID;          // Например, "level_1"
        public int maxReachedStage;     // Максимальный открытый этап в этом мире
        public int currentStage;        // На каком этапе игрок находится СЕЙЧАС (выбрано стрелочками)
        public int killedEnemies;       // Сколько мобов из 10 убито конкретно в этой комнате

        public WorldSaveData(string id, int maxStage, int curStage, int killedCount)
        {
            worldID = id;
            maxReachedStage = maxStage;
            currentStage = curStage;
            killedEnemies = killedCount;
        }
    }

    [System.Serializable]
    public class GameSaveData
    {
        public const string CURRENT_VERSION_ID = "v0_2_1";

        [Header("Player Stats")]
        public string selectedCharacterID;

        [Header("Currencies")]
        [SerializeField] private BigNumber _coins;
        public BigNumber Coins => _coins;


        // СЮДА ПИШЕТСЯ ВЕРСИЯ ПРИ СОХРАНЕНИИ (чтобы понимать, с какого патча пришел игрок)
        [SerializeField] private string _savedVersionID = CURRENT_VERSION_ID;
        public string SavedVersionID => _savedVersionID;


        [Header("Lists Progress")]
        [SerializeField] private List<WeaponSaveData> _weapons;
        public List<WeaponSaveData> Weapons => _weapons;


        [SerializeField] private List<InventoryItemSaveData> _inventoryItems;
        public List<InventoryItemSaveData> InventoryItems => _inventoryItems;


        [SerializeField] private List<WorldSaveData> _worldsProgress;
        public List<WorldSaveData> WorldsProgress => _worldsProgress;


        [Header("Statuses")]
        [SerializeField] private bool _isInventoryUnlocked;
        public bool IsInventoryUnlocked => _isInventoryUnlocked;

        [SerializeField] private bool _isBackpackDropped;
        public bool IsBackpackDropped => _isBackpackDropped;


        [SerializeField] private bool _isBetaTester;
        public bool IsBetaTester => _isBetaTester;


        [SerializeField] private string _lastActiveWorldID;
        public string LastActiveWorldID => _lastActiveWorldID;


        public GameSaveData()
        {
            _coins = new BigNumber(0);
            _savedVersionID = CURRENT_VERSION_ID; // Фиксируем версию при рождении профиля

            selectedCharacterID = "DefaultPlayer";
            _weapons = new List<WeaponSaveData>();
            _inventoryItems = new List<InventoryItemSaveData>();
            _isInventoryUnlocked = false;
            _isBackpackDropped = false;
            _isBetaTester = false;

            _lastActiveWorldID = "level_1";
            _worldsProgress = new List<WorldSaveData>
            {
                new WorldSaveData("level_1", 1, 1, 0)
            };
        }

        // --- ИСПРАВЛЕНО: Единый чистый метод перезаписи кошелька для DataManager ---
        public void SetCoins(BigNumber newCoins, DataManager.AccessKey key)
        {
            if (key == null) return;
            _coins = newCoins;
        }
        
        public void AddCoinsReward(BigNumber reward, DataManager.AccessKey key)
        {
            if (key == null) return;
            _coins += reward; // Складываем большие числа безопасным оператором +=
        }

        // --- МЕТОД ДЛЯ ВЫДАЧИ ПРЕДМЕТОВ/СУНДУКОВ НАПРЯМУЮ ПРИ МИГРАЦИИ ---
        public void AddItemDirectly(string id, int amount, DataManager.AccessKey key)
        {
            if (key == null || string.IsNullOrEmpty(id) || amount <= 0) return;

            int itemIndex = _inventoryItems.FindIndex(item => item.itemID == id);

            if (itemIndex != -1)
            {
                var updatedItem = _inventoryItems[itemIndex];
                updatedItem.amount += amount;
                _inventoryItems[itemIndex] = updatedItem;
            }
            else
            {
                _inventoryItems.Add(new InventoryItemSaveData(id, amount));
            }
        }

        // --- МЕТОД ОБНОВЛЕНИЯ МЕТКИ ВЕРСИИ ВНУТРИ JSON ---
        public void UpdateVersionID(string newVersion, DataManager.AccessKey key)
        {
            if (key == null) return;
            _savedVersionID = newVersion;
        }

        // --- ОСТАЛЬНЫЕ ВАШИ СТАНДАРТНЫЕ МЕТОДЫ ОБНОВЛЕНИЯ СПИСКОВ ---
        public void UpdateWeapons(List<WeaponSaveData> weaponList, DataManager.AccessKey key)
        {
            if (key == null) return;
            _weapons = weaponList;
        }

        public void UpdateInventory(List<InventoryItemSaveData> itemList, DataManager.AccessKey key)
        {
            if (key == null) return;
            _inventoryItems = itemList;
        }

        public void UpdateInventoryUnlockStatus(bool isUnlocked, DataManager.AccessKey key)
        {
            if (key == null) return;
            _isInventoryUnlocked = isUnlocked;
        }

        public void UpdateBackpackDropStatus(bool isDropped, DataManager.AccessKey key)
        {
            if (key == null) return;
            _isBackpackDropped = isDropped;
        }

        public void SetBetaTesterStatus(bool status, DataManager.AccessKey key)
        {
            if (key == null) return;
            _isBetaTester = status;
        }

        public void UpdateLastActiveWorld(string worldID, DataManager.AccessKey key)
        {
            if (key == null) return;
            _lastActiveWorldID = worldID;
        }

        public void UpdateWorldProgressList(List<WorldSaveData> newList, DataManager.AccessKey key)
        {
            if (key == null) return;
            _worldsProgress = newList;
        }




        [Header("Temporary Event Data")]
        // Список ID пройденных ивентовых боссов (для наград или статистики)
        [SerializeField] private List<string> _defeatedEventBosses = new List<string>();
        public List<string> DefeatedEventBosses => _defeatedEventBosses;

        // Метод отметки победы, защищенный вашим токеном
        public void MarkEventBossDefeated(string bossID, DataManager.AccessKey key)
        {
            if (key == null) return;
            if (!_defeatedEventBosses.Contains(bossID))
            {
                _defeatedEventBosses.Add(bossID);
            }
        }
    }

}
