using SlimeRpgEvolution2D.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private string baseFileName = "save_data";
    [SerializeField] private bool useEncryption = false;

    [Header("Configs")]
    // [SerializeField] private List<PlayerConfig> allPlayerConfig; 
    //Если добавлю Классы или Рассы и другие аккаунты, а лучше создай отдельно PlayerDatabase
    [SerializeField] private PlayerConfig _playerConfig;



    [Header("Debug Settings")]
    [SerializeField] private bool disabledSaving = false;
    

    public GameSaveData SaveData { get; private set; }

    public event Action OnDataLoaded;

    public static event Action<int> OnCoinsChanged;
    private string OldSavePath => Path.Combine(Application.persistentDataPath, "save_data.json");

    private string CurrentSavePath => Path.Combine(Application.persistentDataPath, $"{baseFileName}_{GameSaveData.CURRENT_VERSION_ID}.json");


    public class AccessKey { private AccessKey() { } internal static AccessKey Create() => new(); }
    private AccessKey _token;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _token = AccessKey.Create();

        LoadGame();
    }

    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        if (disabledSaving) return;

        try
        {
            string json = JsonUtility.ToJson(SaveData, true);
            File.WriteAllText(CurrentSavePath, json);
            Debug.Log($"[DataManager] Data saved to: {CurrentSavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataManager] Ошибка сохранения: {e.Message}");
        }
    }

    public void LoadGame()
    {
        if (File.Exists(CurrentSavePath))
        {
            try
            {
                string json = File.ReadAllText(CurrentSavePath);
                SaveData = JsonUtility.FromJson<GameSaveData>(json);

                if (SaveData == null)
                {
                    Debug.LogError("[DataManager] Ошибка парсинга JSON текущей версии. Создаем новый профиль.");
                    CreateNewProfile();
                }
                else
                {
                    Debug.Log($"[DataManager] Данные версии ({GameSaveData.CURRENT_VERSION_ID}) успешно загружены.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] Load error: {e.Message}");
                CreateNewProfile();
            }

            OnDataLoaded?.Invoke();
            return;
        }

        if (File.Exists(OldSavePath))
        {
            Debug.Log("[DataManager] Обнаружено сохранение старой версии! Начинаем чистую игру v0_2_0 со статусом ветерана.");

            // Создаем чистый профиль с нуля (сброс прогресса для нового баланса)
            SaveData = new GameSaveData();

            // Награждаем скрытым статусом тестера беты
            SaveData.SetBetaTesterStatus(true, _token);

            // Сохраняем, чтобы зафиксировать файл save_data_v0_2_0.json на диске
            SaveGame();

            OnDataLoaded?.Invoke();
            return;
        }

        // Сценарий 3: Абсолютно новый игрок (нет никаких сохранений вообще)
        Debug.Log("[DataManager] Файлов сохранения не найдено. Создаем чистый профиль для нового игрока.");
        CreateNewProfile();
        OnDataLoaded?.Invoke();
    }

    private void CreateNewProfile()
    {
        SaveData = new GameSaveData();
        SaveGame();
        Debug.Log("[DataManager] Новый профиль успешно создан.");
    }

    public PlayerConfig GetCurrentPlayerConfig()
    {
        if (SaveData == null || _playerConfig == null)
        {
            Debug.LogError("[DataManager] Нет данных или список конфигов пуст!");
            return null;
        }

        return _playerConfig;
    }

    public int GetCurrentDamage()
    {
        if (_playerConfig == null) return 0;
        if (SaveData == null) return _playerConfig.BaseDamage;

        return  _playerConfig.CalculateTotalDamage(SaveData);
    }

    private void OnApplicationPause(bool pauseStaus){ if(pauseStaus) SaveGame(); }
    private void OnApplicationQuit() { SaveGame(); }


    public void AddCoins(int amount)
    {
        if (SaveData == null || amount <= 0) return;

        int newTotal = SaveData.Coins + amount;
        SaveData.UpdateCoins(newTotal, _token);

        OnCoinsChanged?.Invoke(SaveData.Coins);

    }

    public bool TrySpendCoins(int amount)
    {
        if (SaveData == null || amount <= 0 || SaveData.Coins < amount) return false;

        int newTotal = SaveData.Coins - amount;
        SaveData.UpdateCoins(newTotal, _token);

        OnCoinsChanged?.Invoke(SaveData.Coins);


        return true;
    }


    public int GetWeaponLevel(string id)
    {
        var weapon = SaveData.Weapons.FirstOrDefault(w => w.weaponID == id);
        return weapon.weaponID != null ? weapon.currentLevel : 0;

    }

    public void SetWeaponLevel(string id, int level)
    {
        List<WeaponSaveData> newWeapons = new List<WeaponSaveData>(SaveData.Weapons);

        var weaponIndex = newWeapons.FindIndex(w => w.weaponID == id);

        if (weaponIndex != -1)
        {
            var updatedWeapon = newWeapons[weaponIndex];
            updatedWeapon.currentLevel = level;
            newWeapons[weaponIndex] = updatedWeapon;
        }
        else
        {
            newWeapons.Add(new WeaponSaveData(id, level));
        }

        SaveData.UpdateWeapons(newWeapons, _token);
    }

    public void UnlockInventory()
    {
        if (SaveData == null) return;

        // Передаем приватный токен _token, который сгенерирован в Awake
        SaveData.UpdateInventoryUnlockStatus(true, _token);

        // Сохраняем игру, чтобы статус записался в JSON-файл
        SaveGame();

        Debug.Log("<color=green>[DataManager]</color> Инвентарь успешно разблокирован!");
    }



    public void AddItemToSave(string id, int amount = 1)
    {
        if (SaveData == null || string.IsNullOrEmpty(id) || amount <= 0) return;

        List<InventoryItemSaveData> newItems = new List<InventoryItemSaveData>(SaveData.InventoryItems);

        int itemIndex = newItems.FindIndex(item => item.itemID == id);

        if (itemIndex != -1)
        {
            var updatedItem = newItems[itemIndex];
            updatedItem.amount += amount;
            newItems[itemIndex] = updatedItem;
        }
        else
        {
            newItems.Add(new InventoryItemSaveData(id, amount));
        }


        SaveData.UpdateInventory(newItems, _token);

        SaveGame();
    }

    public void SetBackpackDropped(bool isDropped)
    {
        if (SaveData == null) return;

        // Передаем приватный токен _token, сгенерированный в Awake
        SaveData.UpdateBackpackDropStatus(isDropped, _token);

        // Сразу сохраняем изменения на жесткий диск
        SaveGame();

        Debug.Log($"<color=cyan>[DataManager]</color> Статус выпадения рюкзака изменен на: {isDropped}");
    }


#if UNITY_EDITOR
    [ContextMenu("Debug/Add 1000 Coins")]
    public void DebugAddCoins()
    {
        if(!Application.isPlaying)
        {
            Debug.LogWarning("Крутить можно только в режиме Play!");
            return;
        }

        AddCoins(1000);
        Debug.Log("<color=yellow>Debug:</color> Добавлено 1000 монет");
    }

    [ContextMenu("Debug/Full Reset Data")]
    public void ResetData()
    {
        if (File.Exists(CurrentSavePath)) File.Delete(CurrentSavePath);
        CreateNewProfile();
        Debug.Log("<color=red>Данные полностью удалены и сброшены!</color>");
    }
#endif
}
