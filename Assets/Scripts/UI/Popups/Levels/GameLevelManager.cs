using SlimeRpgEvolution2D.Logic;
using System;
using System.Collections;
using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    public class GameLevelManager : MonoBehaviour
    {
        // Делаем Синглтон, чтобы из любого скрипта (например, из Enemy или UI) писать: GameLevelManager.Instance
        public static GameLevelManager Instance { get; private set; }

        [Header("Честный прогресс игрока (Из сохранения)")]
        [Tooltip("Максимальный открытый мир/локация в игре (1, 2...)")]
        [SerializeField] private int maxOpenedWorld = 1;
        [Tooltip("Максимальный до которого дошел игрок на ЭТОЙ локации")]
        [SerializeField] private int maxReachedStage = 1;

        [Header("Визуальный выбор (Что сейчас на экране)")]
        [Tooltip("Мир, который игрок сейчас просматривает или в котором воюет")]
        [SerializeField] private int currentWorldNumber = 1;
        [Tooltip("Этап, выбранный стрелочками на экране прямо сейчас")]
        [SerializeField] private int currentStageNumber = 1;

        [Header("Текущая Битва")]
        private LevelSettings currentLevelSettings; // Ссылка на ScriptableObject текущей локации
        private int enemiesKilledOnCurrentStage = 0; // Сколько слизней убили на этом этапе
        private const int ENEMIES_PER_STAGE_REQUIRED = 10; // Сколько нужно убить для прохождения (например, 10)

        // Событие сообщает UI: (выбранный этап, максимальный открытый этап)
        public static event Action<int, int> OnStageChanged;
        // Событие сообщает UI, что загрузился новый мир, и передает его настройки (включая фон!)
        public static event Action<LevelSettings> OnWorldChanged;

        // Передает: (Сколько убито, Сколько нужно)
        public static event Action<int, int> OnStageProgressChanged;




        [Header("События для UI (Интерфейса)")]
        // Публичные свойства, чтобы другие скрипты могли безопасно читать наши переменные
        public LevelSettings CurrentLevelSettings => currentLevelSettings;
        public int CurrentStageNumber => currentStageNumber;
        public int CurrentWorldNumber => currentWorldNumber;
        public int MaxOpenedWorld => maxOpenedWorld;

        private void Awake()
        {
            // Настройка синглтона
            if (Instance == null)
            {
                Instance = this;

                // Сначала вытаскиваем GameManager из папки -- SYSTEM -- на самый верх иерархии сцены
                transform.SetParent(null);

                DontDestroyOnLoad(gameObject); // Менеджер не уничтожится при смене сцен
            }
            else
            {
                Destroy(gameObject);
            }
        }


        private void Start()
        {
            // Запускаем безопасную инициализацию, чтобы базы данных успели "проснуться"
            StartCoroutine(InitLevelManagerRoutine());
        }

        private IEnumerator InitLevelManagerRoutine()
        {
            // Ждем окончания текущего кадра, чтобы GameDB и DataManager гарантированно загрузились
            yield return null;

            // 1. Подгружаем сохраненный прогресс
            InitProgressFromSave();

            // 2. Загружаем наш текущий мир (например, level_1)
            LoadWorld(currentWorldNumber);

            Debug.Log($"<color=green>[GameLevelManager]</color> Успешно запущен! Текущий мир: {currentWorldNumber}, Этап: {currentStageNumber}");
        }

        private void InitProgressFromSave()
        {
            if (DataManager.Instance == null) return;

            // Вместо инта считываем строковый ID последнего сохраненного мира (например, "level_1")
            string lastWorldID = DataManager.Instance.GetLastActiveWorldID();

            // Вытаскиваем числовой номер чисто для визуала стрелочек, если нужно (из подстроки "level_1" -> 1)
            if (int.TryParse(lastWorldID.Replace("level_", ""), out int parsedWorldNum))
            {
                currentWorldNumber = parsedWorldNum;
            }

            // Лимит открытых миров теперь равен размеру списка в JSON!
            maxOpenedWorld = DataManager.Instance.GetMaxOpenedWorldCount();
        }

        public void LoadWorld(int worldNumber)
        {
            currentWorldNumber = worldNumber;
            string worldKey = $"level_{currentWorldNumber}";

            if (GameDB.Level != null)
            {
                currentLevelSettings = GameDB.Level.GetByID(worldKey);
            }

            if (currentLevelSettings == null)
            {
                Debug.LogError($"[GameLevelManager] Ошибка: Мир с ID '{worldKey}' не найден в GameDB!");
                return;
            }

            // ВОССТАНОВЛЕНИЕ ИЗ JSON "ПОД КЛЮЧ":
            if (DataManager.Instance != null)
            {
                WorldSaveData savedState = DataManager.Instance.GetWorldState(worldKey);

                maxReachedStage = savedState.maxReachedStage;

                // ТЕПЕРЬ ПРАВИЛЬНО: Загружаем именно текущий этап, на котором остановился игрок!
                currentStageNumber = savedState.currentStage;

                // ТЕПЕРЬ ПРАВИЛЬНО: Восстанавливаем точное количество убитых врагов (например, 5 из 10)
                enemiesKilledOnCurrentStage = savedState.killedEnemies;

                // Запоминаем текущий активный мир в корне JSON
                DataManager.Instance.SaveActiveWorldID(worldKey);
            }
            else
            {
                maxReachedStage = 1;
                currentStageNumber = 1;
                enemiesKilledOnCurrentStage = 0;
            }

            maxOpenedWorld = DataManager.Instance.GetMaxOpenedWorldCount();

            // Обновляем интерфейсы боевой сцены
            OnStageProgressChanged?.Invoke(enemiesKilledOnCurrentStage, ENEMIES_PER_STAGE_REQUIRED);
            OnWorldChanged?.Invoke(currentLevelSettings);
            UpdateStageUI();

            // Спавним слизня строго под восстановленные параметры
            SpawnEnemyForCurrentStage();
        }





        public void TryUnlockNextWorld(LevelSettings targetWorldSettings)
        {
            if (targetWorldSettings == null) return;

            // Заглушка денег
            bool hasEnoughGold = true;
            if (!hasEnoughGold) return;

            // Вместо maxOpenedWorld++ просто добавляем новый мир в список сохранения!
            string nextWorldKey = $"level_{maxOpenedWorld + 1}";

            if (DataManager.Instance != null)
            {
                DataManager.Instance.PurchaseNewWorld(nextWorldKey);
                // Обновляем локальный лимит на основе обновленного списка
                maxOpenedWorld = DataManager.Instance.GetMaxOpenedWorldCount();
            }

            Debug.Log($"<color=green>[GameLevelManager]</color> Мир {nextWorldKey} успешно добавлен в список сохранения!");
        }



        public void UpdateStageUI()
        {
            OnStageChanged?.Invoke(currentStageNumber, maxReachedStage);

            // Отправляем текущий прогресс комнаты в UI
            OnStageProgressChanged?.Invoke(enemiesKilledOnCurrentStage, ENEMIES_PER_STAGE_REQUIRED);
        }


        #region Логика Этапов (Верхние стрелочки)

        public void MoveStageBackward()
        {
            if (currentStageNumber > 1)
            {
                currentStageNumber--;
                string worldKey = $"level_{currentWorldNumber}";

                if (DataManager.Instance != null)
                {
                    var savedState = DataManager.Instance.GetWorldState(worldKey);

                    if (currentStageNumber < maxReachedStage)
                    {
                        // Пройденные этапы всегда визуально показывают 10/10
                        enemiesKilledOnCurrentStage = ENEMIES_PER_STAGE_REQUIRED;
                    }
                    else
                    {
                        // ИСПРАВЛЕНО: Просто забираем честное число из JSON без обнулений!
                        enemiesKilledOnCurrentStage = savedState.killedEnemies;
                    }
                }

                OnStageProgressChanged?.Invoke(enemiesKilledOnCurrentStage, ENEMIES_PER_STAGE_REQUIRED);
                UpdateStageUI();
                SpawnEnemyForCurrentStage();
            }
        }

        public void MoveStageForward()
        {
            if (currentStageNumber < maxReachedStage)
            {
                currentStageNumber++;
                string worldKey = $"level_{currentWorldNumber}";

                if (DataManager.Instance != null)
                {
                    var savedState = DataManager.Instance.GetWorldState(worldKey);

                    if (currentStageNumber < maxReachedStage)
                    {
                        enemiesKilledOnCurrentStage = ENEMIES_PER_STAGE_REQUIRED;
                    }
                    else
                    {
                        // ИСПРАВЛЕНО: Просто забираем честное число из JSON без обнулений!
                        // Если там лежит 10 (финал зачищен) — вернется 10. Если там лежит 0 — вернется 0.
                        enemiesKilledOnCurrentStage = savedState.killedEnemies;
                    }
                }

                OnStageProgressChanged?.Invoke(enemiesKilledOnCurrentStage, ENEMIES_PER_STAGE_REQUIRED);
                UpdateStageUI();
                SpawnEnemyForCurrentStage();
            }
        }





        #endregion


        #region RPG Спавн врагов по страницам этапов

        private void SpawnEnemyForCurrentStage()
        {
            if (currentLevelSettings == null) return;

            // ВСЕГДА роллим честный случайный выбор по процентам из твоей книги баланса!
            EnemyConfig baseEnemyConfig = currentLevelSettings.GetRandomEnemyForStage(currentStageNumber);

            if (baseEnemyConfig == null)
            {
                Debug.LogWarning($"[GameLevelManager] Не удалось зароллить врага для этапа {currentStageNumber}!");
                return;
            }

            // Твой оригинальный код поиска уровней (двойной цикл)
            int minLvl = 1;
            int maxLvl = 1;

            foreach (var stagePool in currentLevelSettings.Stages)
            {
                if (stagePool.stageNumber == currentStageNumber)
                {
                    foreach (var entry in stagePool.stageEnemies)
                    {
                        if (entry.enemyConfig == baseEnemyConfig)
                        {
                            minLvl = entry.minEnemyLevel;
                            maxLvl = entry.maxEnemyLevel;
                            break;
                        }
                    }
                    break;
                }
            }

            int rolledEnemyLevel = UnityEngine.Random.Range(minLvl, maxLvl + 1);
            BigNumber calculatedMaxHealth = baseEnemyConfig.GetHealthForLevel(rolledEnemyLevel);
            BigNumber calculatedGoldReward = baseEnemyConfig.GetGoldForLevel(rolledEnemyLevel);
            Sprite enemySprite = baseEnemyConfig.enemySprite;

            if (EnemySpawner.Instance != null)
            {
                // Передаем новые бессмертные типы в спавнер!
                // ⚠️ Напоминание: в самом скрипте EnemySpawner.cs на этом шаге временно появится ошибка,
                // так как его метод SpawnDynamicEnemy внутри всё еще ожидает старые int'ы.
                EnemySpawner.Instance.SpawnDynamicEnemy(baseEnemyConfig, enemySprite, calculatedMaxHealth, calculatedGoldReward, rolledEnemyLevel);
            }

            else
            {
                Debug.LogError("[GameLevelManager] Ошибка: EnemySpawner.Instance не найден на сцене!");
            }
        }

        public void RequestNextEnemySpawn()
        {
            SpawnEnemyForCurrentStage();
        }

        #endregion

        public void OnEnemyDefeated()
        {
            string worldKey = $"level_{currentWorldNumber}";

            if (currentStageNumber == maxReachedStage)
            {
                if (enemiesKilledOnCurrentStage < ENEMIES_PER_STAGE_REQUIRED)
                {
                    enemiesKilledOnCurrentStage++;
                }

                if (enemiesKilledOnCurrentStage >= ENEMIES_PER_STAGE_REQUIRED)
                {
                    if (maxReachedStage < currentLevelSettings.totalStagesInLocation)
                    {
                        maxReachedStage++; // 1. Открываем 2-й этап для правой стрелочки

                        // ВАШЕ ТОЧНОЕ РЕШЕНИЕ: Так как этап открыт, счетчик убийств для мира СРАЗУ обнуляется!
                        // Нам больше не нужно копить убийства на 1-м этапе сверх нормы (хоть 100 штук), прогресс зафиксирован.
                        enemiesKilledOnCurrentStage = 0;

                        // 2. Сразу записываем на диск чистый стейт: maxReachedStage = 2, но убито = 0!
                        // При этом currentStageNumber остается равным 1, потому что игрок еще не нажал стрелочку.
                        if (DataManager.Instance != null)
                        {
                            DataManager.Instance.SaveWorldState(worldKey, maxReachedStage, currentStageNumber, enemiesKilledOnCurrentStage);
                        }

                        Debug.Log($"[Прогресс] Этап зачищен! Открыт этап {maxReachedStage}. Счетчик сброшен в 0.");
                    }
                    else
                    {

                        if (DataManager.Instance != null)
                        {
                            DataManager.Instance.SaveWorldState(worldKey, maxReachedStage, currentStageNumber, enemiesKilledOnCurrentStage);
                        }

                        CheckUnlockButtonState();
                    }
                }
                else
                {
                    // Обычное промежуточное убийство (например, 2/10 или 5/10) — просто сохраняем шаг
                    if (DataManager.Instance != null)
                    {
                        DataManager.Instance.SaveWorldState(worldKey, maxReachedStage, currentStageNumber, enemiesKilledOnCurrentStage);
                    }
                }
            }

            // Если игрок убивает мобов на старых этапах (currentStageNumber < maxReachedStage), 
            // этот код вообще игнорируется, и лишние сохранения на диск не улетают!

            OnStageProgressChanged?.Invoke(enemiesKilledOnCurrentStage, ENEMIES_PER_STAGE_REQUIRED);
            UpdateStageUI();
        }





        private void CheckUnlockButtonState()
        {
            // Оставляем метод пустым, его логику для нижней кнопки покупки миров мы соберем чуть позже
        }

    }
}
