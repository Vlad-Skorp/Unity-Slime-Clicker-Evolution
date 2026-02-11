using System;
using UnityEngine;
using SlimeRpgEvolution2D.Data;

namespace SlimeRpgEvolution2D.UI.Popups
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ShopItemPresenter : MonoBehaviour
    {
        [SerializeField] private ShopItemView _view;
        private WeaponConfig _config;
        public WeaponConfig Config => _config;


        [Header("Settings")]
        [SerializeField] private Color _affordableColor = Color.white;
        [SerializeField] private Color _tooExpensiveColor = Color.red;

        public event Action<WeaponConfig> UpgradeRequested;

        public void Initialize(WeaponConfig config, int level, bool canAfford)
        {
            _config = config;
            UpdateUI(level, canAfford);


            _view.OnClick(() => UpgradeRequested?.Invoke(_config));
        }


        public void UpdateUI(int level, bool canAfford)
        {
            int price = _config.GetUpgradePrice(level);

            _view.SetData(
                _config.weaponSprite,
                $"{_config.displayName} Lvl.{level}",
                $"{price} <sprite name=\"coin\">",
                canAfford ? _affordableColor : _tooExpensiveColor
            );

            _view.SetInteraction(canAfford);
        }
    }
}