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
            if (_infoWindow != null) _infoWindow.SetActive(false);
            InitializeInventory();

            if (_animationRoutine != null) StopCoroutine(_animationRoutine);

            // ВЫЗЫВАЕМ ОБЩИЙ ХЕЛПЕР: Передаем компоненты этого окна в универсальную анимацию открытия
            _animationRoutine = StartCoroutine(UIAnimationHelper.AnimateWindow(
                _canvasGroup, _windowContent, 0f, 1f, 0.8f, 1f, _animationDuration
            ));
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

            if (_infoNameText != null) _infoNameText.text = config.DisplayName;
            if (_infoDescText != null) _infoDescText.text = config.Description;

            if (_infoIcon != null)
            {
                _infoIcon.sprite = config.Icon;
                _infoIcon.enabled = config.Icon != null;
            }
        }
        public void CloseInventory()
        {
            if (_animationRoutine != null) StopCoroutine(_animationRoutine);

            // Просто вызываем общую корутину из нашего хелпера!
            _animationRoutine = StartCoroutine(UIAnimationHelper.AnimateWindow(
                _canvasGroup, _windowContent, 1f, 0f, 1f, 0.8f, _animationDuration, () =>
                {
                    gameObject.SetActive(false);
                    UIManager.Instance?.NotifyWindowClosed();
                }
            ));
        }

        public void OpenInventoryDirect()
        {
            if (_animationRoutine != null) StopCoroutine(_animationRoutine);
            gameObject.SetActive(true);

            // Вызываем общую корутину на открытие
            _animationRoutine = StartCoroutine(UIAnimationHelper.AnimateWindow(
                _canvasGroup, _windowContent, 0f, 1f, 0.8f, 1f, _animationDuration
            ));
        }
    }
}