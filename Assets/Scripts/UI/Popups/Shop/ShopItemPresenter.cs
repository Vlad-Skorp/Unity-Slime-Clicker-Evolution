using System;
using UnityEngine;
using SlimeRpgEvolution2D.Data;

namespace SlimeRpgEvolution2D.UI.Popups
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ShopItemPresenter : MonoBehaviour
    {
        [SerializeField] private ShopItemView _view;
        private ShopProductConfig _config;
        public ShopProductConfig Config => _config;

        [Header("Settings")]
        [SerializeField] private Color _affordableColor = Color.white;
        [SerializeField] private Color _tooExpensiveColor = Color.red;

        public event Action<ShopProductConfig> UpgradeRequested;

        public void Initialize(ShopProductConfig config, bool canAfford)
        {
            _config = config;
            UpdateUI(canAfford);


            _view.OnClick(() => UpgradeRequested?.Invoke(_config));
        }


        public void UpdateUI(bool canAfford)
        {
            if (_config == null) return;

            // 1. Получаем актуальную цену (фиксированную или прогрессивную от уровня)
            int price = _config.GetCurrentPrice();

            // 2. Формируем строку состояния (Lvl.X для мечей, пусто или "Куплено" для рюкзака)
            string stateText = string.Empty;
            string priceText = $"{price} <sprite name=\"Coin_1\">";
            bool isInteractable = canAfford;

            // Если товар — оружие, вытягиваем его уровень для отображения
            if (_config is ShopWeaponProduct weaponProduct)
            {
                int currentLevel = DataManager.Instance.GetWeaponLevel(weaponProduct.ID);
                stateText = $"Lvl.{currentLevel}";
            }
            else if (_config.IsPurchasedOrMax())
            {
                stateText = "Куплено";
                priceText = "Макс.";
                isInteractable = false; // Блокируем кнопку, так как рюкзак одноразовый
            }

            _view.SetData(
                _config.Icon,
                _config.DisplayName,
                stateText,
                priceText,
                isInteractable ? _affordableColor : _tooExpensiveColor
            );

            _view.SetInteraction(canAfford);
        }
    }
}