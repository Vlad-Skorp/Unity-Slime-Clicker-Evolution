using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SlimeRpgEvolution2D.Data;
using UnityEngine.PlayerLoop;

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


        private void OnEnable()
        {
            if(_infoWindow != null) _infoWindow.SetActive(false);
            InitializeInventory();
        }

        private void InitializeInventory()
        {
            foreach(var slot in _activeSlots)
            {
                slot.SlotSelected -= HandleSlotSelection;
                Destroy(slot.gameObject);
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

        private void OnDisable()
        {
            foreach (var slot in _activeSlots)
                slot.SlotSelected -= HandleSlotSelection;
        }
    }
}