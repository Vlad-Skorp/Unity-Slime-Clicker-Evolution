using SlimeRpgEvolution2D.Data;
using System.Collections.Generic;
using UnityEngine;
using static StagePool;

namespace SlimeRpgEvolution2D.Data
{
    [CreateAssetMenu(fileName = "LevelSettings", menuName = "Config/Levels/LevelSettings")]
    public class LevelSettings : ScriptableObject, IIdentifiable<string>
    {
        [SerializeField] private string worldID;
        public string ID => worldID;

        [Header("Глобальные настройки мира")]
        [Tooltip("Красивое название локации для отображения игроку (например, 'Каньон Костей')")]
        [SerializeField] private string worldName;
        public string WorldName => worldName;

        [Tooltip("Маленькая круглая или квадратная иконка локации для отображения в списках требований квестов")]
        [SerializeField] private Sprite worldIcon;
        public Sprite WorldIcon => worldIcon;

        [Tooltip("Задний фон всей этой локации целиком")]
        [SerializeField] private Sprite stageBackground;
        public Sprite StageBackground => stageBackground;

        [Header("Условия Разблокировки Локации")]
        [SerializeField] private UnlockRequirement _unlockRequirement;
        public UnlockRequirement UnlockRequirement => _unlockRequirement;


        [Header("Страницы Прогресса Локации")]
        [SerializeField] private List<StagePool> stages;
        public List<StagePool> Stages => stages;

        public int totalStagesInLocation => stages.Count;



        // Поиск врага по страницам этапов для GameLevelManager
        public EnemyConfig GetRandomEnemyForStage(int currentStageNumber)
        {
            foreach (var stagePool in stages)
            {
                if (stagePool.stageNumber == currentStageNumber)
                {
                    return stagePool.GetRandomEnemyFromStagePool();
                }
            }
            if (stages.Count > 0) return stages[0].GetRandomEnemyFromStagePool();
            return null;
        }

        // ВОЗВРАЩЕНО И УЛУЧШЕНО: Автоматический подсчет процентов в инспекторе
        // ВОЗВРАЩЕНО И УЛУЧШЕНО: Твой оригинальный рабочий метод подсчета шансов
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            if (stages == null || stages.Count == 0) return;

            // Проходимся по этапам, вытаскиваем структуру, обновляем текст и записываем ОБРАТНО
            for (int i = 0; i < stages.Count; i++)
            {
                StagePool currentStage = stages[i];
                currentStage.UpdateChancePreview();
                stages[i] = currentStage; // Фиксируем изменения в реальном списке!
            }
        }
    }
}

    [System.Serializable]
    public struct EnemySpawnChance
    {
        [Header("Существо и Вес спавна")]
        public EnemyConfig enemyConfig;
        public float spawnWight;

        [Space(5)]
        [Header("Уровни врага")]
        public int minEnemyLevel;
        public int maxEnemyLevel;
    }

[System.Serializable]
public struct StagePool
{
    [HideInInspector]
    public int stageNumber;

    public List<EnemySpawnChance> stageEnemies;

    [Header("Шансы спавна на этом этапе")]
    [TextArea(3, 8)]
    [Tooltip("Расчет процентов происходит автоматически при любом изменении веса выше")]
    public string stageChanceDebug;


    public void UpdateChancePreview()
    {
        // 1. ПЕРВИЧНЫЕ ПРОВЕРКИ
        if (stageEnemies == null || stageEnemies.Count == 0)
        {
            stageChanceDebug = "Нет врагов на этом этапе.";
            return;
        }

        float totalWeight = 0;
        foreach (var enemy in stageEnemies) totalWeight += enemy.spawnWight;

        if (totalWeight <= 0)
        {
            stageChanceDebug = "Суммарный вес спавна равен 0. Задайте вес врагам!";
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("<b>Расчет шансов для текущего этапа:</b>");
        sb.AppendLine();

        // 2. ИНИЦИАЛИЗАЦИЯ ЭКСТРЕМУМОВ (Забиваем минимум максимальными девятками структуры)
        BigNumber absoluteMinHp = new BigNumber(0);
        absoluteMinHp.UpdateBaseValue(999999999);
        for (int i = 1; i < 4; i++)
        {
            absoluteMinHp.DeductSegment(i, -999999999);
        }
        BigNumber absoluteMaxHp = new BigNumber(0);

        // Переменные для точного накопления средних double-значений с учетом весов
        double averageGoldPerDefeat = 0d;
        double averageHpPerStage = 0d;

        // 3. ЦИКЛ ОТРИСОВКИ КАРТОЧЕК МОБОВ И СБОРА СТАТИСТИКИ
        foreach (var enemy in stageEnemies)
        {
            float pc = (enemy.spawnWight / totalWeight) * 100f; // Процент шанса спавна (например, 30.0f)
            float weightRatio = enemy.spawnWight / totalWeight; // Доля от общего веса (например, 0.3f)

            string enemyName = "Пусто";
            BigNumber minHp = new BigNumber(0);
            BigNumber maxHp = new BigNumber(0);
            BigNumber minGold = new BigNumber(0);
            BigNumber maxGold = new BigNumber(0);

            int displayMinLevel = enemy.minEnemyLevel;
            int displayMaxLevel = enemy.maxEnemyLevel;
            string limitNotice = "";

            if (enemy.enemyConfig != null)
            {
                enemyName = enemy.enemyConfig.enemyName;

                // Обрезаем уровни по лимиту maxlevel самого слизня
                int clampedMinLevel = Mathf.Min(enemy.minEnemyLevel, enemy.enemyConfig.maxlevel);
                int clampedMaxLevel = Mathf.Min(enemy.maxEnemyLevel, enemy.enemyConfig.maxlevel);

                if (enemy.maxEnemyLevel > enemy.enemyConfig.maxlevel)
                {
                    displayMaxLevel = enemy.enemyConfig.maxlevel;
                    limitNotice = " <color=#ff4d4d>(лимит моба)</color>";
                }
                if (enemy.minEnemyLevel > enemy.enemyConfig.maxlevel)
                {
                    displayMinLevel = enemy.enemyConfig.maxlevel;
                }

                // Рассчитываем ХП и Золото для clamped-уровней
                minHp = enemy.enemyConfig.GetHealthForLevel(clampedMinLevel);
                maxHp = enemy.enemyConfig.GetHealthForLevel(clampedMaxLevel);
                minGold = enemy.enemyConfig.GetGoldForLevel(clampedMinLevel);
                maxGold = enemy.enemyConfig.GetGoldForLevel(clampedMaxLevel);

                // Ищем экстремумы комнаты поразрядным сравнением BigNumber
                if (minHp < absoluteMinHp) absoluteMinHp = minHp;
                if (maxHp > absoluteMaxHp) absoluteMaxHp = maxHp;

                // РАСПАКОВКА ЗОЛОТА МОБА ДЛЯ СРЕДНЕГО ЗНАЧЕНИЯ
                float minGoldF = minGold.ToFloat();
                float maxGoldF = maxGold.ToFloat();
                averageGoldPerDefeat += ((minGoldF + maxGoldF) / 2f) * weightRatio;

                // РАСПАКОВКА ХП МОБА ДЛЯ СРЕДНЕГО ЗНАЧЕНИЯ
                float minHpF = minHp.ToFloat();
                float maxHpF = maxHp.ToFloat();
                averageHpPerStage += ((minHpF + maxHpF) / 2f) * weightRatio;
            }

            string rarityColor = "#ffffff";
            if (pc < 5f) rarityColor = "#ff4d4d";
            else if (pc < 20f) rarityColor = "#ff9f43";

            sb.AppendLine($"<b>Название</b> — <color={rarityColor}>{enemyName}</color>");

            // Форматируем BigNumber в красивые буквы кликера для карточки моба
            string formattedMinHp = NumberFormatter.Format(minHp);
            string formattedMaxHp = NumberFormatter.Format(maxHp);
            string formattedMinGold = NumberFormatter.Format(minGold);
            string formattedMaxGold = NumberFormatter.Format(maxGold);

            sb.AppendLine($"<b>Уровни</b> — {displayMinLevel}-{displayMaxLevel}{limitNotice} <color=#888888>({formattedMinHp}hp - {formattedMaxHp}hp | {formattedMinGold}💰 - {formattedMaxGold}💰)</color>");
            sb.AppendLine($"<b>Шанс</b> — <b>{pc:F1}%</b>");
            sb.AppendLine();
        } // <--- КОНЕЦ ЦИКЛА FOREACH

        // =========================================================================
        // 4. ФОРМИРОВАНИЕ НИЖНЕЙ СВОДКИ БАЛАНСА (Вызывается строго ПОСЛЕ цикла)
        // =========================================================================
        sb.AppendLine("<color=#7bed9f>━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        sb.AppendLine($"<b>💡 Сводка баланса этапа:</b>");

        BigNumber maxLimitCheck = new BigNumber("999999999Sp");

        // Проверяем, изменился ли наш стартовый максимум девятками в ячейке 3
        if (absoluteMinHp < maxLimitCheck)
        {
            string formattedMin = NumberFormatter.Format(absoluteMinHp);
            string formattedMax = NumberFormatter.Format(absoluteMaxHp);

            // ДОБАВЛЕНЫ ПРОБЕЛЫ: Теперь "1.5 M hp — 20.4 B hp" читается идеально
            sb.AppendLine($"• Разброс здоровья в этой комнате: <b>{formattedMin} hp — {formattedMax} hp</b>.");

            // ДОБАВЛЕН ПРОБЕЛ: Перед "hp"
            sb.AppendLine($"• Среднее здоровье врага на этапе: <b>{NumberFormatter.Format(new BigNumber(averageHpPerStage))} hp</b>.");
        }
        else
        {
            sb.AppendLine($"• Разброс здоровья в этой комнате: <b>0 hp</b>.");
        }

        // Считаем доход за полную зачистку (10 мобов)
        BigNumber totalStageGoldReward = new BigNumber(averageGoldPerDefeat * 10d);
        sb.AppendLine($"• Примерный доход за полную зачистку этапа (10 врагов): <b>~{NumberFormatter.Format(totalStageGoldReward)} 💰</b>.");

        if (absoluteMinHp > 0 && absoluteMinHp < maxLimitCheck)
        {
            float minHpF = absoluteMinHp.ToFloat();
            float maxHpF = absoluteMaxHp.ToFloat();

            float difficultySpike = minHpF > 0f ? (maxHpF / minHpF) : 0f;

            // ДОБАВЛЕН ПРОБЕЛ: Перед буквой "х" кратности (например, "3.5 х")
            sb.AppendLine($"• Скачок сложности (разница в ХП мобов): <b>{difficultySpike:F1} х</b>.");
        }

        stageChanceDebug = sb.ToString();
    }



    [System.Serializable]
    public struct StageRequirementData
    {
        [Tooltip("Ссылка на ScriptableObject локации, которую нужно пройти")]
        public LevelSettings targetWorld;

        [Tooltip("Какой этап должен быть открыт (например, 2)")]
        public int requiredStageNumber;

        [Tooltip("Какая волна должна быть пройдена на этом этапе (например, 5)")]
        public int requiredWaveNumber;
    }

    [System.Serializable]
    public struct WeaponRequirementData
    {
        public WeaponConfig requiredWeapon;
        public int requiredWeaponLevel;
    }

    [System.Serializable]
    public struct ItemRequirementData
    {
        public ItemConfig requiredItem;
        public int requiredItemAmount;
    }


    [System.Serializable]
    public struct UnlockRequirement
    {
        [Header("0. Требование по Золоту (Опционально)")]
        [Tooltip("0 — бесплатно, больше 0 — требует золото.")]
        public BigNumber unlockCost;

        [Header("Включение категорий условий")]
        public bool useStageRequirement;
        public bool useWeaponRequirement;
        public bool useItemRequirement;

        [Header("Динамические списки требований")]
        // Теперь это полноценные бесконечные списки в инспекторе Unity
        public List<StageRequirementData> stageRequirements;
        public List<WeaponRequirementData> weaponRequirements;
        public List<ItemRequirementData> itemRequirements;
    }






    public EnemyConfig GetRandomEnemyFromStagePool()
        {
            if (stageEnemies == null || stageEnemies.Count == 0) return null;

            float totalWight = 0;
            foreach (var entry in stageEnemies) totalWight += entry.spawnWight;

            float randomValue = Random.Range(0, totalWight);
            float currentWight = 0;

            foreach (var entry in stageEnemies)
            {
                currentWight += entry.spawnWight;
                if (randomValue <= currentWight) return entry.enemyConfig;
            }

            return stageEnemies[0].enemyConfig;
        }
    }
