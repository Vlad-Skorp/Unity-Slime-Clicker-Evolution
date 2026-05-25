using SlimeRpgEvolution2D.UI.Popups;
using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    // --- 2. ТОВАР: ИНСТРУМЕНТ (РЮКЗАК) ---
    [CreateAssetMenu(fileName = "Shop_Tool_", menuName = "Config/Shop/Products/Tool")]
    public class ShopToolProduct : ShopProductConfig
    {
        [Header("Настройки Инструмента")]
        [Tooltip("Перетащите сюда ToolConfig этого рюкзака")]
        public ToolConfig toolReference;
        public int fixedPrice = 500;

        public override string ID => toolReference != null ? toolReference.itemID : string.Empty;
        public override string DisplayName => toolReference != null ? toolReference.displayName : "Неизвестный инструмент";
        public override Sprite Icon => toolReference != null ? toolReference.itemSprite : null;

        public override int GetCurrentPrice() => fixedPrice;

        public override bool IsPurchasedOrMax()
        {
            // Рюкзак проверяет, разблокирован ли он уже
            return toolReference != null && toolReference.IsPurchased();
        }

        public override void Buy()
        {
            if (toolReference == null) return;

            // Логика рюкзака: открывает инвентарь (вызывает OnPurchase)
            toolReference.OnPurchase();
            DataManager.Instance.SaveGame();
        }
    }
}
