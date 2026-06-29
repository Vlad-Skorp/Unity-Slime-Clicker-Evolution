using SlimeRpgEvolution2D.Core;
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

    [Header("Configs")]
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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _token = AccessKey.Create();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadGame();
    }


    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        if (disabledSaving) return;
        if (SaveData == null) return;

        string json = JsonUtility.ToJson(SaveData, true);
        File.WriteAllText(CurrentSavePath, json);
    }

    // --- СИСТЕМА УМНЫХ ОБНОВЛЕНИЙ И МИГРАЦИИ ВЕРСИЙ ---
    public void LoadGame()
    {
        string currentVer = GameSaveData.CURRENT_VERSION_ID;

        // --- ПРИОРИТЕТ 1: Ищем родной файл текущей версии ---
        if (File.Exists(CurrentSavePath))
        {
            LoadDirectJson(CurrentSavePath);
            return;
        }

        // --- ПРИОРИТЕТ 2: Ищем файл строго ПРЕДЫДУЩЕЙ разрешенной версии ---
        string allowedPastSavePath = GetAllowedPreviousSavePath(currentVer);

        if (!string.IsNullOrEmpty(allowedPastSavePath) && File.Exists(allowedPastSavePath))
        {
            try
            {
                // Сначала считываем старый JSON во временную переменную для анализа версий!
                string oldJson = File.ReadAllText(allowedPastSavePath);
                GameSaveData oldData = JsonUtility.FromJson<GameSaveData>(oldJson);

                // Берем старую версию из самого файла и сравниваем с новой версией из кода
                string previousVersion = oldData.SavedVersionID;

                // ИСПРАВЛЕНО: Теперь мы ВСЕГДА спрашиваем у нашей маски, нужно ли сбросить прогресс!
                if (ShouldResetProgress(previousVersion, currentVer))
                {
                    Debug.Log($"<color=orange>[Мажорный патч] Смена версии с {previousVersion} на {currentVer}. Полный сброс баланса! Выдаем сундук ветерана.</color>");

                    // Создаем абсолютно чистый профиль с нуля под новую математику
                    SaveData = new GameSaveData();

                    // Проверяем достижения ветерана: если в старом файле был открыт 2-й мир
                    if (oldData.WorldsProgress.Any(w => w.worldID == "level_2"))
                    {
                        // Выдаем эпический подарок за закрытие сезона
                        SaveData.AddItemDirectly("veteran_chest_epic", 1, _token);
                    }
                    else
                    {
                        // Выдаем базовый сундучок тестера беты
                        SaveData.AddItemDirectly("veteran_chest_basic", 1, _token);
                    }
                }
                else
                {
                    Debug.Log($"<color=#7bed9f>[Филерный патч] Обновление с {previousVersion} на {currentVer}. прогресс успешно перенесен!</color>");

                    // Честный перенос данных для минорных и филерных версий
                    SaveData = new GameSaveData();
                    SaveData.SetCoins(oldData.Coins, _token);
                    SaveData.UpdateWeapons(oldData.Weapons, _token);
                    SaveData.UpdateWorldProgressList(oldData.WorldsProgress, _token);

                    // Заглушка под кастомные ачивки филеров на будущее
                    CheckAndApplyVeteranAchievements(oldData, currentVer);
                }

                // В самом конце фиксируем актуальную версию игры в файл и сохраняем его под новым именем
                SaveData.UpdateVersionID(currentVer, _token);
                SaveGame();

                OnDataLoaded?.Invoke();
                return;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] Ошибка миграции по цепочке версий: {e.Message}");
                CreateNewProfile();
                return;
            }
        }

        // --- ПРИОРИТЕТ 3: Абсолютно новый игрок (нет никаких сохранений вообще) ---
        Debug.Log($"[DataManager] Файлов прошлых сохранений для {currentVer} не найдено. Запуск чистой игры...");
        CreateNewProfile();
        OnDataLoaded?.Invoke();
    }


    private void LoadDirectJson(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            SaveData = JsonUtility.FromJson<GameSaveData>(json);
            Debug.Log($"[DataManager] Файл {Path.GetFileName(path)} успешно загружен.");
            OnDataLoaded?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataManager] Ошибка загрузки родного файла: {e.Message}");
            CreateNewProfile();
            OnDataLoaded?.Invoke();
        }
    }


    private string GetAllowedPreviousSavePath(string currentVersion)
    {
        string directory = Application.persistentDataPath;

        // 1. Парсим текущую версию игры в чистые цифры (например, "v0_2_2" -> 0, 2, 2)
        if (!TryExtractVersionNumbers(currentVersion, out int curMajor, out int curMinor, out int curPatch))
        {
            return string.Empty;
        }

        // Определяем, является ли текущая запущенная игра филером (имеет ли в названии "-f")
        bool isCurrentVersionAFiller = currentVersion.ToLower().Contains("-f");

        string[] allSaveFiles = Directory.GetFiles(directory, $"{baseFileName}_*.json");

        string bestMatchPath = string.Empty;
        int bestMajor = -1, bestMinor = -1, bestPatch = -1;

        foreach (string filePath in allSaveFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string fileVersionStr = fileName.Replace($"{baseFileName}_", "").ToLower();

            // 2. Очищаем имя файла от букв и переводим в цифры для сравнения
            if (!TryExtractVersionNumbers(fileVersionStr, out int fMajor, out int fMinor, out int fPatch))
                continue;

            // ПРАВИЛО ЗАПРЕТА 1: Файл из будущего? (Игнорируем намертво)
            if (fMajor > curMajor) continue;
            if (fMajor == curMajor && fMinor > curMinor) continue;
            if (fMajor == curMajor && fMinor == curMinor && fPatch > curPatch) continue;

            bool isFileAFiller = fileVersionStr.Contains("-f");

            // --- ЖЕЛЕЗНЫЕ ПРАВИЛА ИЗОЛЯЦИИ ФИЛЕРОВ (Ваше ТЗ) ---

            // Ситуация А: Сейчас запущена ОСНОВНАЯ игра (например, v0_2_2 или v0_2_1)
            if (!isCurrentVersionAFiller)
            {
                // ПРАВИЛО ЗАПРЕТА 2: Основная игра ИГНОРИРУЕТ любые файлы филеров из прошлого!
                // Переход 0.2.1-f -> 0.2.2 ЗАПРЕЩЕН. Игра пойдет искать чистую 0.2.1 или 0.2.0.
                if (isFileAFiller) continue;
            }
            // Ситуация Б: Сейчас запущен ФИЛЕРНЫЙ режим (например, v0_2_1-f)
            else
            {
                // ПРАВИЛО ЗАПРЕТА 3: Филер не может наследоваться от другого филера, если цифры не совпадают,
                // но может брать чистую основу как стартовую базу.
                // Если этот файл — тоже филер, но его цифры меньше текущего филера, мы его скипаем.
                if (isFileAFiller && (fMajor != curMajor || fMinor != curMinor || fPatch != curPatch)) continue;
            }

            // Особый случай равенства версий (например, на диске v0_2_1, а запускаем v0_2_1) -> скипаем, это приоритет 1
            if (fMajor == curMajor && fMinor == curMinor && fPatch == curPatch && fileVersionStr == currentVersion.ToLower())
            {
                continue;
            }

            // 3. Математический выбор САМОГО БЛИЗКОГО разрешенного сохранения снизу
            if (fMajor > bestMajor ||
               (fMajor == bestMajor && fMinor > bestMinor) ||
               (fMajor == bestMajor && fMinor == bestMinor && fPatch > bestPatch))
            {
                bestMajor = fMajor;
                bestMinor = fMinor;
                bestPatch = fPatch;
                bestMatchPath = filePath;
            }
        }

        return bestMatchPath;
    }


    // УНИВЕРСАЛЬНЫЙ ПОМОЩНИК: Очищает любую строку от "-f", "v" и разбивает на [0][2][1]
    private bool TryExtractVersionNumbers(string versionStr, out int major, out int minor, out int patch)
    {
        major = 0; minor = 0; patch = 0;
        if (string.IsNullOrEmpty(versionStr)) return false;

        try
        {
            // Убираем букву "v", а также все филерные хвосты вроде "-f", "-f1", "-beta"
            string cleanStr = versionStr.ToLower().Replace("v", "");
            if (cleanStr.Contains("-"))
            {
                cleanStr = cleanStr.Split('-')[0]; // Отрезаем всё, что после дефиса
            }

            // Разбиваем по точкам или нижним подчеркиваниям
            char[] separators = new char[] { '.', '_' };
            string[] parts = cleanStr.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 3)
            {
                major = int.Parse(parts[0]);
                minor = int.Parse(parts[1]);
                patch = int.Parse(parts[2]);
                return true;
            }
        }
        catch (Exception)
        {
            // Если игрок вручную переименовал файл в какую-то кашу, просто пропустим его
        }

        return false;
    }


    private void CheckAndApplyVeteranAchievements(GameSaveData oldData, string currentVersion)
    {
        // МЕТОД-ЗАГЛУШКА НА БУДУЩЕЕ
        // Здесь вы будете кодом проверять: "Если игрок убил босса в филере, переносим эту строку-ачивку в основу"

        // Пример структуры на будущее:
        /*
        if (currentVersion == "v0_2_2" && oldData.DefeatedEventBosses.Contains("gojo_satoru"))
        {
            // Выдаем ОДИН редкий коллекционный предмет ветерану в основу, который ни на что не влияет
            SaveData.AddItemDirectly("gojo_badge_rare", 1, _token);
            Debug.Log("[Награда] Перенесена коллекционная медаль за победу над Годжо!");
        }
        */
    }

    private bool ShouldResetProgress(string oldVersion, string newVersion)
    {
        if (string.IsNullOrEmpty(oldVersion)) return false;

        char[] separators = new char[] { '.', '_', '-' };
        string[] oldParts = oldVersion.Replace("v", "").Split(separators);
        string[] newParts = newVersion.Replace("v", "").Split(separators);

        if (oldParts.Length < 2 || newParts.Length < 2) return false;

        string oldMajor = oldParts[1];
        string newMajor = newParts[1];

        return oldMajor != newMajor; // true если изменилась мажорная цифра (0_2_0 -> 0_3_0)
    }

    private string GetLatestOldSavePath()
    {
        string directory = Application.persistentDataPath;
        var saveFiles = Directory.GetFiles(directory, $"{baseFileName}*.json")
            .Where(path => !path.EndsWith($"{GameSaveData.CURRENT_VERSION_ID}.json"))
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTime)
            .FirstOrDefault();

        return saveFiles != null ? saveFiles.FullName : string.Empty;
    }


    private void CreateNewProfile()
    {
        SaveData = new GameSaveData();
        SaveData.UpdateVersionID(GameSaveData.CURRENT_VERSION_ID, _token);
        SaveGame();
        Debug.Log("[DataManager] Новый профиль успешно создан.");
    }


    // --- БЕЗОПАСНАЯ И ЧИСТАЯ ЭКОНОМИКА БОЛЬШИХ ЧИСЕЛ ЧЕРЕЗ ОПЕРАТОРЫ ---
    public void AddCoins(BigNumber reward)
    {
        if (SaveData == null) return;
        SaveData.AddCoinsReward(reward, _token);
        OnCoinsChanged?.Invoke(SaveData.Coins.GetSegment(0));
    }


    public bool CanAfford(BigNumber cost)
    {
        if (SaveData == null) return false;
        return SaveData.Coins >= cost; // Идеальное сравнение в одну строчку через перегруженный оператор!
    }


    public bool TrySpendCoins(BigNumber cost)
    {
        if (SaveData == null) return false;

        if (SaveData.Coins < cost) return false; // Защита: не даем уйти балансу в минус

        // Списываем деньги математически правильным оператором вычитания в столбик
        BigNumber newBalance = SaveData.Coins - cost;
        SaveData.SetCoins(newBalance, _token); // Передаем обновленный BigNumber целиком

        OnCoinsChanged?.Invoke(SaveData.Coins.GetSegment(0));
        SaveGame();
        return true;
    }



    // --- СИСТЕМА ИНВЕНТАРЯ И ПРОГРЕССА МИРОВ ---
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
        SaveData.UpdateInventoryUnlockStatus(true, _token);
        SaveGame();
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

    public bool TrySpendItem(string id, int amount)
    {
        if (SaveData == null || string.IsNullOrEmpty(id) || amount <= 0) return false;

        List<InventoryItemSaveData> newItems = new List<InventoryItemSaveData>(SaveData.InventoryItems);
        int itemIndex = newItems.FindIndex(item => item.itemID == id);

        // Если предмета нет в инвентаре или его количество меньше, чем нужно списать
        if (itemIndex == -1 || newItems[itemIndex].amount < amount) return false;

        var updatedItem = newItems[itemIndex]; 
        updatedItem.amount -= amount;

        if(updatedItem.amount <= 0) newItems.RemoveAt(itemIndex);
        
        else newItems[itemIndex] = updatedItem;


        SaveData.UpdateInventory(newItems, _token);
        SaveGame();

        Debug.Log($"<color=orange>[DataManager]</color> Списано предметов: {id} x{amount}");
        return true;
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


    public string GetLastActiveWorldID()
    {
        return SaveData != null ? SaveData.LastActiveWorldID : "level_1";
    }

    public void SaveActiveWorldID(string worldID)
    {
        if (SaveData == null) return;
        SaveData.UpdateLastActiveWorld(worldID, _token);
    }

    // Сколько миров сейчас открыто (размер списка)
    public int GetMaxOpenedWorldCount()
    {
        return SaveData != null ? SaveData.WorldsProgress.Count : 1;
    }

    // Покупка мира = добавление новой пустой структуры в список миров!
    public void PurchaseNewWorld(string newWorldID)
    {
        if (SaveData == null) return;

        List<WorldSaveData> newWorlds = new List<WorldSaveData>(SaveData.WorldsProgress);

        // Если такого мира еще нет в списке — добавляем его (открываем)
        if (!newWorlds.Any(w => w.worldID == newWorldID))
        {
            newWorlds.Add(new WorldSaveData(newWorldID, 1, 1, 0));
            SaveData.UpdateWorldProgressList(newWorlds, _token);
            SaveGame();
            Debug.Log($"[DataManager] Куплен и добавлен в JSON новый мир: {newWorldID}");
        }
    }

    public WorldSaveData GetWorldState(string worldID)
    {
        if (SaveData == null) return new WorldSaveData(worldID, 1, 1, 0);
        var progress = SaveData.WorldsProgress.FirstOrDefault(w => w.worldID == worldID);
        return progress.worldID != null ? progress : new WorldSaveData(worldID, 1, 1, 0);
    }

    public void SaveWorldState(string worldID, int maxStage, int curStage, int killedCount)
    {
        if (SaveData == null) return;

        List<WorldSaveData> newWorlds = new List<WorldSaveData>(SaveData.WorldsProgress);
        var worldIndex = newWorlds.FindIndex(w => w.worldID == worldID);

        WorldSaveData updatedData = new WorldSaveData(worldID, maxStage, curStage, killedCount);

        if (worldIndex != -1) newWorlds[worldIndex] = updatedData;
        else newWorlds.Add(updatedData);

        SaveData.UpdateWorldProgressList(newWorlds, _token);
        SaveGame();
    }

    public BigNumber GetCurrentDamage()
    {
        // Если конфигурация пуста, возвращаем 0 урона во всех ячейках
        if (_playerConfig == null) return new BigNumber(0);

        // Если файла сохранений нет, возвращаем базовый урон, обернутый в BigNumber
        if (SaveData == null) return _playerConfig.BaseDamage;

        // Запрашиваем полный расчет у конфига игрока (он вернет нам BigNumber)
        return _playerConfig.CalculateTotalDamage(SaveData);
    }

    private void OnApplicationPause(bool pauseStaus) { if (pauseStaus) SaveGame(); }
    private void OnApplicationQuit() { SaveGame(); }



#if UNITY_EDITOR
    [ContextMenu("Debug/Add 1000 Coins")]
    public void DebugAddCoins()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Крутить можно только в режиме Play!");
            return;
        }

        AddCoins(new BigNumber(1000));
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


    public PlayerConfig GetCurrentPlayerConfig()
    {
        if (SaveData == null || _playerConfig == null)
        {
            Debug.LogError("[DataManager] Нет данных или список конфигов пуст!");
            return null;
        }

        return _playerConfig;
    }



    // Метод проверяет: является ли версия файла НОВЕЕ, чем сама игра?
    private bool IsSaveVersionNewer(string fileVersion, string gameVersion)
    {
        if (string.IsNullOrEmpty(fileVersion) || string.IsNullOrEmpty(gameVersion)) return false;
        if (fileVersion == gameVersion) return false;

        char[] separators = new char[] { '.', '_', '-' };
        string[] fileParts = fileVersion.Replace("v", "").Split(separators);
        string[] gameParts = gameVersion.Replace("v", "").Split(separators);

        if (fileParts.Length < 2 || gameParts.Length < 2) return false;

        // Сравниваем мажорные цифры (например, 3 и 2)
        int fileMajor = int.Parse(fileParts[0]);
        int gameMajor = int.Parse(gameParts[0]);
        if (fileMajor != gameMajor) return fileMajor > gameMajor;

        // Сравниваем минорные цифры (например, 2 и 2)
        int fileMinor = int.Parse(fileParts[1]);
        int gameMinor = int.Parse(gameParts[1]);
        if (fileMinor != gameMinor) return fileMinor > gameMinor;

        // Если дело дошло до филеров, патч без буквы "-f" считается новее патча с ней
        return false;
    }

}
