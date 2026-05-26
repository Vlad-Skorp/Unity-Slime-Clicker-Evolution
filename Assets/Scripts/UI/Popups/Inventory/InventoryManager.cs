using SlimeRpgEvolution2D.Data;
using SlimeRpgEvolution2D.UI.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace SlimeRpgEvolution2D.UI.Popups
{

    public class InventoryManager : MonoBehaviour
    {
        // Небольшой вспомогательный класс для теста, имитирующий структуру в рюкзаке
        private class InventoryItemSlot
        {
            public string itemID;
            public int amount;
        }

        [Header("UI Settings")]
        [SerializeField] private InventorySlotPresenter _slotPrefab;
        [SerializeField] private Transform _container;

        private readonly List<InventorySlotPresenter> _activeSlots = new List<InventorySlotPresenter>();

        [Header("Inventory Config")]
        [SerializeField] private int _totalSlots = 20;

        [Header("Top Info Window (ItemInfo)")]
        [SerializeField] private GameObject _infoWindow;
        [SerializeField] private Image _infoIcon;
        [SerializeField] private TextMeshProUGUI _infoNameText;
        [SerializeField] private TextMeshProUGUI _infoDescText;

        [Header("Animations Settings")]
        [SerializeField] private CanvasGroup _canvasGroup;   // Перетащите сюда CanvasGroup инвентаря
        [SerializeField] private Transform _windowContent;   // Перетащите сюда внутренний контент окна для скейла
        [SerializeField] private float _animationDuration = 0.2f; // Длительность анимации (как в магазине)

        private Coroutine _animationRoutine;

        private void OnEnable()
        {
            if(_infoWindow != null) _infoWindow.SetActive(false);
            InitializeInventory();

            if (_animationRoutine != null) StopCoroutine(_animationRoutine);
            _animationRoutine = StartCoroutine(AnimateInventory(0f, 1f, 0.8f, 1f));
        }

        private void OnDisable()
        {
            foreach (var slot in _activeSlots)
                if (slot != null) slot.SlotSelected -= HandleSlotSelection;
        }

        private void InitializeInventory()
        {
            foreach(var slot in _activeSlots)
            {
                if (slot != null)
                {
                    slot.SlotSelected -= HandleSlotSelection;
                    Destroy(slot.gameObject);
                }
            }
            _activeSlots.Clear();

            var savedItems = DataManager.Instance.SaveData.InventoryItems;

            for (int i = 0; i < _totalSlots; i++)
            {
                var slotUI = Instantiate(_slotPrefab, _container);

                ItemConfig itemConfig = null;
                int itemAmount = 0;

                
                if (savedItems != null && i < savedItems.Count)
                {
                    string itemId = savedItems[i].itemID;
                    itemAmount = savedItems[i].amount;

                    if (GameDB.Items != null)
                    {
                        itemConfig = GameDB.Items.GetByID(itemId);
                    }
                }

                slotUI.Initialize(itemConfig, itemAmount);
                slotUI.SlotSelected += HandleSlotSelection;

                _activeSlots.Add(slotUI);

            }
        }

        private void HandleSlotSelection(ItemConfig config)
        {
            if(config == null)
            {
                if (_infoWindow != null) _infoWindow.SetActive(false);
                return;
            }

            if (_infoWindow != null) _infoWindow.SetActive(true);

            if (_infoNameText != null) _infoNameText.text = config.displayName;
            if (_infoDescText != null) _infoDescText.text = config.description;

            if (_infoIcon != null)
            {
                _infoIcon.sprite = config.itemSprite;
                _infoIcon.enabled = config.itemSprite != null;
            }
        }

        public void CloseInventory()
        {
            if (_animationRoutine != null) StopCoroutine(_animationRoutine);

            // Сначала проигрываем анимацию закрытия, затем передаем управление UIManager
            _animationRoutine = StartCoroutine(AnimateInventory(1f, 0f, 1f, 0.8f, () =>
            {
                // ИЗМЕНЕНО: Выключаем объект инвентаря по завершении анимации
                gameObject.SetActive(false);

                // ИЗМЕНЕНО: Уведомляем UIManager для корректного скрытия затемнения/слоя
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.NotifyWindowClosed();
                }
            }));
        }

        private IEnumerator AnimateInventory(float startAlpha, float endAlpha, float startScale, float endScale, System.Action onComplete = null)
        {
            float elapsed = 0;
            while (elapsed < _animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _animationDuration;

                // Плавность через SmoothStep (как в вашем ShopManager)
                float curve = Mathf.SmoothStep(0, 1, t);

                if (_canvasGroup != null) _canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curve);
                if (_windowContent != null) _windowContent.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, curve);

                yield return null;
            }

            if (_canvasGroup != null) _canvasGroup.alpha = endAlpha;
            if (_windowContent != null) _windowContent.localScale = Vector3.one * endScale;

            onComplete?.Invoke();
            _animationRoutine = null;
        }

        [System.Obsolete("Используйте UIManager.Instance.ToggleInventory для открытия инвентаря")]
        public void OpenInventory() => ToggleInventory();

        [System.Obsolete("Используйте UIManager.Instance.ToggleInventory для переключения инвентаря")]
        public void ToggleInventory()
        {
            if (DataManager.Instance == null || !DataManager.Instance.SaveData.IsInventoryUnlocked)
            {
                Debug.LogWarning("[Inventory] Попытка открыть инвентарь без купленного рюкзака!");
                return;
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ToggleInventory();
            }
        }
    }
}