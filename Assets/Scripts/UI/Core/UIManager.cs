using UnityEngine;
using UnityEngine.UI;
using SlimeRpgEvolution2D.Data;
using SlimeRpgEvolution2D.UI.Popups;

namespace SlimeRpgEvolution2D.UI.Core
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Layers")]
        [SerializeField] private GameObject _hudLayer;
        [SerializeField] private GameObject _popupLayer;

        [Header("Inventory HUD Button Settings")]
        [SerializeField] private Button _inventoryButton; // Ссылка на плашку рюкзака внизу
        [SerializeField] private GameObject _lockIcon;

        [Header("Popups References")]
        [Tooltip("Перетащите сюда игровой объект Магазина со сцены (ShopPanel)")]
        [SerializeField] private ShopManager _shopWindow;

        [Tooltip("Перетащите сюда игровой объект Инвентаря со сцены (InventoryPanel)")]
        [SerializeField] private InventoryManager _inventoryWindow;

        [Tooltip("Перетащите сюда игровой объект Настроек со сцены (SettingsPanel)")]
        [SerializeField] private SettingsManager _settingsWindow;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return; 
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitLayers();
        }

        private void Start()
        {
            if (DataManager.Instance != null)
            {
                // 1. Обязательно сначала отписываемся (на случай, если подписка уже была),
                // чтобы событие не вызывалось дважды и не плодило баги
                DataManager.Instance.OnDataLoaded -= RefreshInventoryButtonState;

                // 2. Подписываемся заново для текущего запуска
                DataManager.Instance.OnDataLoaded += RefreshInventoryButtonState;

                // 3. ПОДСТРАХОВКА: Если DataManager УЖЕ успел загрузить JSON к этому моменту,
                // мы не ждем события, а принудительно настраиваем кнопку прямо сейчас!
                if (DataManager.Instance.SaveData != null)
                {
                    RefreshInventoryButtonState();
                }
            }

            // То же самое делаем для покупки рюкзака
            BackpackConfig.OnBackpackPurchased -= HandleBackpackUnlock;
            BackpackConfig.OnBackpackPurchased += HandleBackpackUnlock;

            CloseAllWindows();
        }

        private void OnDestroy()
        {
            // Переносим очистку в OnDestroy, который сработает только при полном выходе из игры
            if (DataManager.Instance != null)
            {
                DataManager.Instance.OnDataLoaded -= RefreshInventoryButtonState;
            }
            BackpackConfig.OnBackpackPurchased -= HandleBackpackUnlock;
        }

        private void InitLayers()
        {
            if (_hudLayer != null) _hudLayer.SetActive(true);
            if (_popupLayer != null) _popupLayer.SetActive(false);
        }


        public void RefreshInventoryButtonState()
        {
            if (DataManager.Instance == null || DataManager.Instance.SaveData == null) return;

            bool isUnlocked = DataManager.Instance.SaveData.IsInventoryUnlocked;

            if (_inventoryButton != null) _inventoryButton.interactable = isUnlocked;
            if (_lockIcon != null) _lockIcon.SetActive(!isUnlocked);
        }

        private void HandleBackpackUnlock()
        {
            // 1. Убираем замок и активируем кнопку на нижней панели HUD
            if (_lockIcon != null) _lockIcon.SetActive(false);
            if (_inventoryButton != null) _inventoryButton.interactable = true;

            // 2. Ищем менеджер магазина в слое попапов и плавно закрываем его
            if (_popupLayer != null)
            {
                var shopManager = _popupLayer.GetComponentInChildren<ShopManager>(true);
                if (shopManager != null) shopManager.CloseShop();
            }

            // 3. Ищем инвентарь в слое попапов и плавно распахиваем его
            if (_popupLayer != null)
            {
                var inventoryManager = _popupLayer.GetComponentInChildren<InventoryManager>(true);
                if (inventoryManager != null) inventoryManager.ToggleInventory();
            }
        }

        public void ToggleShop()
        {
            if (_shopWindow == null) return;

            if (_shopWindow.gameObject.activeSelf)
            {
                // ИЗМЕНЕНО: Если магазин открыт, вызываем его внутренний метод закрытия
                _shopWindow.CloseShop();
            }
            else
            {
                // ИЗМЕНЕНО: Перед открытием жестко гасим соседа без анимации, чтобы они не накладывались
                if (_inventoryWindow != null) _inventoryWindow.gameObject.SetActive(false);
                if (_settingsWindow != null) _settingsWindow.gameObject.SetActive(false);

                _shopWindow.gameObject.SetActive(true);
                if (_popupLayer != null) _popupLayer.SetActive(true);
            }
        }

        public void ToggleInventory()
        {
            if (_inventoryWindow == null) return;
            if (DataManager.Instance == null || !DataManager.Instance.SaveData.IsInventoryUnlocked) return;

            if (_inventoryWindow.gameObject.activeSelf)
            {
                // ИЗМЕНЕНО: Если инвентарь открыт, плавно закрываем его
                _inventoryWindow.CloseInventory();
            }
            else
            {
                // ИЗМЕНЕНО: Перед открытием жестко гасим магазин и настройки
                if (_shopWindow != null) _shopWindow.gameObject.SetActive(false);
                if (_settingsWindow != null) _settingsWindow.gameObject.SetActive(false);

                _inventoryWindow.gameObject.SetActive(true);
                if (_popupLayer != null) _popupLayer.SetActive(true);
            }
        }

        public void ToggleSettings()
        {
            if (_settingsWindow == null) return;

            if (_settingsWindow.gameObject.activeSelf)
            {
                _settingsWindow.CloseSettings();
            }
            else
            {
                if (_shopWindow != null) _shopWindow.gameObject.SetActive(false);
                if (_inventoryWindow != null) _inventoryWindow.gameObject.SetActive(false);

                _settingsWindow.gameObject.SetActive(true);
                if (_popupLayer != null) _popupLayer.SetActive(true);
            }
        }

        public void NotifyWindowClosed()
        {
            bool anyOpen = (_shopWindow != null && _shopWindow.gameObject.activeSelf) ||
                           (_inventoryWindow != null && _inventoryWindow.gameObject.activeSelf) ||
                           (_settingsWindow != null && _settingsWindow.gameObject.activeSelf);

            if (!anyOpen && _popupLayer != null)
            {
                _popupLayer.SetActive(false);
            }
        }
        public void CloseAllWindows()
        {
            if (_shopWindow != null) _shopWindow.gameObject.SetActive(false);
            if (_inventoryWindow != null) _inventoryWindow.gameObject.SetActive(false);
            if (_settingsWindow != null) _settingsWindow.gameObject.SetActive(false);

            if (_popupLayer != null) _popupLayer.SetActive(false);
        }


        public void OpenPopup(GameObject prefab)
        {
            if (prefab == null || _popupLayer == null) return;

            _popupLayer.SetActive(true);

            GameObject popup = Instantiate(prefab, _popupLayer.transform);
        }

        public void CloseAllPopups()
        {
            foreach (Transform child in _popupLayer.transform)
            {
                Destroy(child.gameObject);
            }
            _popupLayer.SetActive(false);
        }

        public void ToggleHUD(bool isVisible)
        {
            if (_hudLayer != null) _hudLayer.SetActive(isVisible);
        }
      
    }
}
