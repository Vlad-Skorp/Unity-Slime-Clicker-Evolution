using SlimeRpgEvolution2D.Data;
using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private string fileName = "save_data.json";
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
    private string SavePath => Path.Combine(Application.persistentDataPath, fileName);




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
            File.WriteAllText(SavePath, json);
            Debug.Log($"[DataManager] Data saved to: {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataManager] Ошибка сохранения: {e.Message}");
        }
    }

    public void LoadGame()
    {

        if (!File.Exists(SavePath))
        {
            Debug.Log("[DataManager] Файл сохранения не найден. Создаем новый профиль.");
            CreateNewProfile();
            OnDataLoaded?.Invoke();
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            SaveData = JsonUtility.FromJson<GameSaveData>(json);

            if (SaveData == null)
            {
                Debug.LogError("[DataManager] Ошибка парсинга JSON. Создаем новый профиль.");
                CreateNewProfile();
            }
            else 
            {
                Debug.Log("[DataManager] Данные успешно загружены.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataManager] Load error: {e.Message}");
            CreateNewProfile();
        }


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
        if (File.Exists(SavePath)) File.Delete(SavePath);
        CreateNewProfile();
        Debug.Log("<color=red>Данные полностью удалены и сброшены!</color>");
    }
#endif
}
