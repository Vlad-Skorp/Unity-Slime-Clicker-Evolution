using SlimeRpgEvolution2D.UI.Popups;
using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    // --- 2. ТОВАР: УНИВЕРСАЛЬНЫЙ ИНСТРУМЕНТ (Подходит и для Кирки, и для Рюкзака!) ---
    [CreateAssetMenu(fileName = "Shop_Tool_", menuName = "Config/Shop/Products/Tool")]
    public class ShopToolProduct : ShopProductConfig
    {
        [Header("Настройки Инструмента")]
        [Tooltip("Перетащите сюда ЛЮБОЙ ToolConfig (включая BackpackConfig)")]
        public ToolConfig toolReference;
        public BigNumber fixedPrice;

        // ПЕРЕОПРЕДЕЛЯЕМ ДОСТУПНОСТЬ НА ПРИЛАВКЕ:
        public override bool CanBeSold
        {
            get
            {
                if (DataManager.Instance == null) return false;

                // УМНАЯ ПРОВЕРКА: Если этот ассет — рюкзак, то проверяем, выпал ли он с земли
                if (toolReference is BackpackConfig)
                {
                    return DataManager.Instance.SaveData.IsBackpackDropped;
                }

                // Для всех остальных инструментов (кирки, топоры) товар доступен в магазине всегда по умолчанию
                return true;
            }
        }

        public override string ID => toolReference != null ? toolReference.itemID : string.Empty;
        public override string DisplayName => toolReference != null ? toolReference.displayName : "Неизвестный инструмент";
        public override Sprite Icon => toolReference != null ? toolReference.itemSprite : null;

        public override BigNumber GetCurrentPrice() => fixedPrice;

        public override bool IsPurchasedOrMax()
        {
            // Благодарим полиморфизм: если это рюкзак, вызовется переопределенный метод из BackpackConfig!
            return toolReference != null && toolReference.IsPurchased();
        }

        public override void Buy()
        {
            if (toolReference == null || DataManager.Instance == null) return;

            // Магазин просто говорит абстрактному предмету: "Тебя купили!"
            toolReference.OnPurchase();
        }

    }
}
