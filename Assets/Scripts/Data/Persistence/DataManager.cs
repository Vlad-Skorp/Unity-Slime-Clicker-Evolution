using SlimeRpgEvolution2D.Data;
using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private string fileName = "save_data.json";
    [SerializeField] private bool useEncryption = false;

    [Header("Configs")]
    [SerializeField] private List<PlayerConfig> allPlayerConfig;
    
    public GameSaveData SaveData { get; private set; }

    public event Action OnDataLoaded;

    public static event Action<int> OnCoinsChanged;
    private string SavePath => Path.Combine(Application.persistentDataPath, fileName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadGame();
    }

    [ContextMenu("Save Game")]
    public void SaveGame()
    {
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

        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                SaveData = JsonUtility.FromJson<GameSaveData>(json);
                Debug.Log("[DataManager] Data loaded successfully.");

                if (SaveData != null)
                {
                    Debug.Log("[DataManager] Загружено.");
                    OnDataLoaded?.Invoke();
                    return;
                }
            }

            catch (Exception e)
            {
                Debug.LogError($"[DataManager] Load error: {e.Message}");
            }
        }

        Debug.Log("[DataManager] Создаем новый профиль.");
        CreateNewProfile();
        OnDataLoaded?.Invoke();
    }

    private void CreateNewProfile()
    {
        SaveData = new GameSaveData { coins = 0 };
        SaveData.coins = 0;
        SaveGame();
    }

    public PlayerConfig GetCurrentPlayerConfig()
    {
        if (SaveData == null || allPlayerConfig == null || allPlayerConfig.Count == 0)
        {
            Debug.LogError("[DataManager] Нет данных или список конфигов пуст!");
            return null;
        }

        PlayerConfig config = allPlayerConfig.Find(c => c.CharacterID == SaveData.selectedCharacterID);

        return config ?? allPlayerConfig[0];
    }

    public int GetCurrentDamage()
    {
        PlayerConfig currentConfig = GetCurrentPlayerConfig();

        if (currentConfig == null) return 0;
        if (SaveData == null) return currentConfig.BaseDamage;

        return currentConfig.CalculateTotalDamage(SaveData);
    }

    private void OnApplicationPause(bool pauseStaus){ if(pauseStaus) SaveGame(); }
    private void OnApplicationQuit() { SaveGame(); }


    public void AddCoins(int amount)
    {
        if (SaveData == null) return;

        SaveData.coins += amount;

        OnCoinsChanged?.Invoke(SaveData.coins);
 
    }

    public bool TrySpendCoins(int amount)
    {
        if (SaveData == null || SaveData.coins < amount) return false;

        SaveData.coins -= amount;
        OnCoinsChanged?.Invoke(SaveData.coins);
        return true;
    }
}
