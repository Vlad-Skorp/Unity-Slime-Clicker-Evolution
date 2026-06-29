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
            BigNumber currentPrice = _config.GetCurrentPrice();

            // 2. Формируем строку состояния (Lvl.X для мечей, пусто или "Куплено" для рюкзака)
            string stateText = string.Empty;
            // На кнопке теперь будет красиво: "500", "12.5M", "15B" или "1T"!
            string priceText = $"{NumberFormatter.Format(currentPrice)} <sprite name=\"Coin_1\">";
            bool isInteractable = canAfford;

            // Если товар — оружие, вытягиваем его уровень для отображения
            if (_config is ShopWeaponProduct weaponProduct)
            {
                int currentLevel = DataManager.Instance.GetWeaponLevel(weaponProduct.ID);
                stateText = $"Lvl. {currentLevel}";
            }
            else if (_config.IsPurchasedOrMax())
            {
                stateText = "Куплено"; // Текст для рюкзаков
            }

            if (_config.IsPurchasedOrMax())
            {
                priceText = "<color=#ff4d4d>Макс.</color>";
                isInteractable = false; // Намертво блокируем кнопку, она станет серой
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