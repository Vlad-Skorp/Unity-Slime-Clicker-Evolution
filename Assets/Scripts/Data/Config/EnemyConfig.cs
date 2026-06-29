using System.Collections.Generic;
using UnityEngine;


namespace SlimeRpgEvolution2D.Data
{
    public struct EnemyDropResult
    {
        public ItemConfig itemConfig;
        public int amount;

        public EnemyDropResult(ItemConfig config, int count)
        {
            itemConfig = config;
            amount = count;
        }
    }


    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Config/Entities/EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        public string enemyName;
        [SerializeField] private BigNumber maxHealth;
        public BigNumber MaxHealth => maxHealth;

        [SerializeField] private BigNumber goldReward;
        public BigNumber GoldReward => goldReward;

        public Sprite enemySprite;

        [Header("Level Settings")]
        [Tooltip("Максимальный уровень врага в игре")]
        public int maxlevel;
        [Tooltip("Список точек изменения множителей характеристик по уровням")]
        public List<LevelMultiply> levelEvolution;


        [Header("Visual Effects (DOTween Loot)")]
        [Tooltip("Префаб DroppedLootPrefab, который мы создали в папке Project")]
        public GameObject droppedLootPrefab;

        [Tooltip("Спрайт золотой монетки для анимации полета")]
        public Sprite goldCoinSprite;

        [Header("Loot Settings (Weighted)")]
        [Tooltip("Список предметов, которые могут выпасть из этого слизня")]
        public List<ItemDropChance> availableLoot;

        [Header("Debug Loot Preview")]
        [Tooltip("Здесь в инспекторе автоматически покажутся реальные проценты выпадения предметов")]
        public List<string> lootChancePreview;

        #region Математика расчета HP и Монет по уровням (Исправленная накопительная логика)
        public BigNumber GetHealthForLevel(int targetLevel)
        {
            if (targetLevel > maxlevel) targetLevel = maxlevel;

            // ИСПРАВЛЕНО: Вместо лапши из нулей получаем точный double одной строчкой
            double currentHp = maxHealth.ToDouble();

            // Пошагово увеличиваем здоровье уровень за уровнем
            for (int currentLvl = 2; currentLvl <= targetLevel; currentLvl++)
            {
                double activeMultiplier = 1.1d;

                // Ищем актуальный порог для ТЕКУЩЕГО шага уровня
                for (int i = levelEvolution.Count - 1; i >= 0; i--)
                {
                    if (currentLvl >= levelEvolution[i].level)
                    {
                        activeMultiplier = levelEvolution[i].levelHpMyltipley;
                        break;
                    }
                }

                // Умножаем НАКОПЛЕННОЕ здоровье предыдущего уровня на текущий коэффициент
                currentHp *= activeMultiplier;
            }

            // Передаем итоговый огромный double в конструктор BigNumber
            return new BigNumber(currentHp);
        }


        public BigNumber GetGoldForLevel(int targetLevel)
        {
            if (targetLevel > maxlevel) targetLevel = maxlevel;

            // ИСПРАВЛЕНО: Точно так же убираем старую распаковку для золота
            double currentGold = goldReward.ToDouble();

            for (int currentLvl = 2; currentLvl <= targetLevel; currentLvl++)
            {
                double activeMultiplier = 1.05d;

                for (int i = levelEvolution.Count - 1; i >= 0; i--)
                {
                    if (currentLvl >= levelEvolution[i].level)
                    {
                        activeMultiplier = levelEvolution[i].levelCoinMyltipley;
                        break;
                    }
                }

                currentGold *= activeMultiplier;
            }

            // Передаем итоговый огромный double в конструктор BigNumber
            return new BigNumber(currentGold);
        }

        #endregion



        public EnemyDropResult RollRandomDrop()
        {
            if (availableLoot == null || availableLoot.Count == 0) return new EnemyDropResult(null, 0);

            float totalWeight = 0;
            foreach (var entry in availableLoot) totalWeight += entry.dropWeight;

            if (totalWeight <= 0) return new EnemyDropResult(null, 0);

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0;

            foreach (var entry in availableLoot)
            {
                currentWeight += entry.dropWeight;
                if(randomValue <= currentWeight)
                {
                    return new EnemyDropResult(entry.itemConfig, entry.dropAmount);
                }
            }

            return new EnemyDropResult(null, 0);
        }


        private void OnValidate()
        {
            if (Application.isPlaying) return;

            if (lootChancePreview == null) lootChancePreview = new List<string>();
            lootChancePreview.Clear();

            if (availableLoot == null || availableLoot.Count == 0) return;

            float total = 0;
            foreach (var e in availableLoot) total += e.dropWeight;

            if (total <= 0) return;

            foreach (var e in availableLoot)
            {
                float pc = (e.dropWeight / total) * 100f;
                string itemName = e.itemConfig != null ? e.itemConfig.DisplayName : "НИЧЕГО (Пустышка)";

                if (e.itemConfig != null)
                {
                    lootChancePreview.Add($"{itemName} (х{e.dropAmount}): {pc:F1}%");
                }
                else
                {
                    lootChancePreview.Add($"{itemName}: {pc:F1}%");
                }
            }
        }

    }

    [System.Serializable]
    public struct ItemDropChance
    {
        public ItemConfig itemConfig; // Ссылка на наш ScriptableObject предмета
        [Tooltip("Сколько штук этого предмета выдавать при выпадении данного элемента")]
        [Min(1)] public int dropAmount; // НАШЕ НОВОЕ ПОЛЕ КОЛИЧЕСТВА

        [Tooltip("Чем выше число, тем чаще выпадает этот предмет. Если хотите шанс 'Ничего не выпало', создайте элемент без конфига предмета.")]
        public float dropWeight;
    }


    [System.Serializable]
    public struct LevelMultiply
    {
        [Header("Уровент")]
        [Tooltip("Уровень для усиления")]
        public int level;

        [Header("Усиления")]
        [Tooltip("Усиление здоровье после достижения уровня")]
        public float levelHpMyltipley;
        [Tooltip("Усиление монет после достижения уровня")]
        public float levelCoinMyltipley;
    }
}