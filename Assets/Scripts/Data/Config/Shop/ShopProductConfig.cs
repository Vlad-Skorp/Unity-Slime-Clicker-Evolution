using SlimeRpgEvolution2D.UI.Popups;
using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    // --- БАЗОВЫЙ КЛАСС ТОВАРА (МАГАЗИН РАБОТАЕТ ТОЛЬКО С НИМ) ---
    public abstract class ShopProductConfig : ScriptableObject, IIdentifiable<string>
    {
        [Header("Настройки витрины")]
        [Tooltip("Если снять галочку, предмет исчезнет из магазина")]
        [SerializeField] private bool _defaultCanBeSold = true;

        public virtual bool CanBeSold => _defaultCanBeSold;

        [Tooltip("В какой вкладке магазина будет находиться этот предмет")]
        public ShopTabType tabCategory;

        // Эти свойства UI магазина будет вызывать одинаково для ВСЕХ товаров:
        public abstract string ID { get; }
        public abstract string DisplayName { get; }
        public abstract Sprite Icon { get; }

        public abstract int GetCurrentPrice();
        public abstract bool IsPurchasedOrMax();
        public abstract void Buy();
    }
}
