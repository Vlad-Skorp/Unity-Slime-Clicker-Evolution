using SlimeRpgEvolution2D.UI.Popups;
using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    // --- 3. ТОВАР: СФЕРЫ И ПРЕДМЕТЫ ИНВЕНТАРЯ ---
    [CreateAssetMenu(fileName = "Shop_Item_", menuName = "Config/Shop/Products/Inventory Item")]
    public class ShopInventoryItemProduct : ShopProductConfig
    {
        [Header("Настройки Предмета Инвентаря")]
        public string inventoryItemID;
        public string inventoryItemName;
        public Sprite inventoryItemSprite;
        public BigNumber fixedPrice;

        public override string ID => inventoryItemID;
        public override string DisplayName => inventoryItemName;
        public override Sprite Icon => inventoryItemSprite;

        public override BigNumber GetCurrentPrice() => fixedPrice;

        public override bool IsPurchasedOrMax() => false; // Сферы можно покупать бесконечно

        public override void Buy()
        {
            if (DataManager.Instance == null || string.IsNullOrEmpty(inventoryItemID)) return;

            // Логика сферы: просто улетает в инвентарь игрока
            DataManager.Instance.AddItemToSave(inventoryItemID, 1);
        }
    }
}