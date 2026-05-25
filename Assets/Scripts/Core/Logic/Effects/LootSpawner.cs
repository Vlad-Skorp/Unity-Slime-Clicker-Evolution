using UnityEngine;
using SlimeRpgEvolution2D.Data;

namespace SlimeRpgEvolution2D.Logic.Effects
{
    public class LootSpawner : MonoBehaviour
    {
        public static LootSpawner Instance { get; private set; }

        // ЭТА СТРОКА СОЗДАСТ ПОЛЕ В ИНСПЕКТОРЕ:
        [Header("UI References")]
        [SerializeField] private RectTransform _coinPanelTransform;

        [Header("Backpack Settings")]
        [SerializeField] private GameObject _backpackPrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SpawnLootEffects(EnemyConfig config, Vector3 worldPosition, EnemyDropResult dropResult)
        {
            // Если в инспекторе забыли перетащить объект, скрипт подстрахует и найдет его сам
            if (_coinPanelTransform == null)
            {
                GameObject foundPanel = GameObject.Find("CoinPanel");
                if (foundPanel != null) _coinPanelTransform = foundPanel.GetComponent<RectTransform>();
            }

            if (config == null || config.droppedLootPrefab == null || _coinPanelTransform == null || Camera.main == null) return;

            // Переводим позицию смерти из игрового мира в экранные координаты Canvas
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            Vector3 targetHUDPos = _coinPanelTransform.position;

            // 1. СПАВН ВИЗУАЛЬНЫХ МОНЕТОК
            if (config.goldCoinSprite != null)
            {
                int visualCoinsCount = Mathf.Min(5, Mathf.CeilToInt((float)config.goldReward / 5f));

                for (int i = 0; i < visualCoinsCount; i++)
                {
                    // Монетка спавнится внутри родителя CoinPanel, чтобы принадлежать Canvas
                    GameObject coinGo = Instantiate(config.droppedLootPrefab, _coinPanelTransform.parent);
                    coinGo.transform.position = screenPos;
                    coinGo.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

                    if (coinGo.TryGetComponent<DroppedLoot>(out var droppedLoot))
                    {
                        droppedLoot.Initialize(config.goldCoinSprite, targetHUDPos, true);
                    }
                }
            }


            // 2. СПАВН ВИЗУАЛЬНЫХ ПРЕДМЕТОВ (ЯДЕР)
            if (dropResult.itemConfig != null && dropResult.itemConfig.itemSprite != null)
            {
                GameObject itemGo = Instantiate(config.droppedLootPrefab, _coinPanelTransform.parent);
                itemGo.transform.position = screenPos;
                itemGo.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

                if (itemGo.TryGetComponent<DroppedLoot>(out var droppedLoot))
                {
                    droppedLoot.Initialize(dropResult.itemConfig.itemSprite, Vector3.zero, false);
                }
            }

            // 3. ЗДЕСЬ БУДЕТ ВАШ МЕТОД ДЛЯ СПАВНА РЮКЗАКА:
            // SpawnBackpackClickable(screenPos)
            if (dropResult.itemConfig != null && _backpackPrefab != null)
            {
                // ИСПРАВЛЕНО: Теперь берем реальный статус из ваших сохранений!
                bool isInventoryUnlocked = false;

                if (DataManager.Instance != null && DataManager.Instance.SaveData != null)
                {
                    isInventoryUnlocked = DataManager.Instance.SaveData.IsInventoryUnlocked;
                }

                // Рюкзак выпадает только в том случае, если инвентарь у игрока ещё заблокирован
                if (!isInventoryUnlocked)
                {
                    GameObject backpackGo = Instantiate(_backpackPrefab, _coinPanelTransform.parent);
                    if (backpackGo.TryGetComponent<SlimeRpgEvolution2D.Logic.Effects.ClickableBackpack>(out var clickableBackpack))
                    {
                        clickableBackpack.Initialize(screenPos);
                    }
                }
            }
        }


    }
}