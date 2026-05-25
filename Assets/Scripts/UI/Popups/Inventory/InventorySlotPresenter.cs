using UnityEngine;
using SlimeRpgEvolution2D.Data;
using System;


namespace SlimeRpgEvolution2D.UI.Popups
{
    public class InventorySlotPresenter : MonoBehaviour
    {
        [SerializeField] private InventorySlotView _view;
        private ItemConfig _config;
        public ItemConfig Config => _config;

        private int _currentAmount;

        public event Action<ItemConfig> SlotSelected;

        public void Initialize(ItemConfig config, int amount)
        {
            _config = config;
            _currentAmount = amount;

            UpdateUI();

            _view.OnClick(() => SlotSelected?.Invoke(_config));
        }

        public void UpdateUI()
        {
            Sprite iconSprite = (_config != null) ? _config.itemSprite : null;

            string amountString = $"x{_currentAmount}";

            bool showAmount = _config != null && _currentAmount > 0;

            _view.SetData(iconSprite, amountString, showAmount);
        }
    }
}
