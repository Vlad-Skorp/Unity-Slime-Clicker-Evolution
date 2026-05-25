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
        public int maxHealth;
        public int goldReward;
        public Sprite enemySprite;

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
            if (lootChancePreview == null) lootChancePreview = new List<string>();
            lootChancePreview.Clear();

            if (availableLoot == null || availableLoot.Count == 0) return;

            float total = 0;
            foreach (var e in availableLoot) total += e.dropWeight;

            if (total <= 0) return;

            foreach (var e in availableLoot)
            {
                float pc = (e.dropWeight / total) * 100f;
                string itemName = e.itemConfig != null ? e.itemConfig.displayName : "НИЧЕГО (Пустышка)";

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
}