using SlimeRpgEvolution2D.UI.Popups;
using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    [CreateAssetMenu(fileName = "NewShopProduct", menuName = "Config/Shop/Product")]
    public class ShopProductConfig : ScriptableObject, IIdentifiable<string>
    {
        // ID самого товара равен ID предмета, который внутри него лежит
        public string ID => GetCurrentID();

        [Header("Витрина Магазина")]
        public ShopTabType tabCategory; // Вы выбираете вкладку: Weapons или Inventory
        public int purchasePrice;       // Цена конкретно в этом магазине

        [Header("Что именно продаем? (Выбираем ТОЛЬКО ОДНО поле в инспекторе)")]
        [Tooltip("Если продаем оружие (палка, деревянный меч), перетащите сюда")]
        public WeaponConfig weaponReference;

        [Tooltip("Если продаем рюкзак или инструменты добычи, перетащите сюда")]
        public ToolConfig toolReference;

        [Header("Для обычных предметов инвентаря (Сферы стихий и др.)")]
        [Tooltip("Если предмет должен просто покупаться в инвентарь, впишите его точный ID")]
        public string inventoryItemID;
        [Tooltip("Отображаемое имя для предметов инвентаря")]
        public string inventoryItemName;
        [Tooltip("Иконка для предметов инвентаря")]
        public Sprite inventoryItemSprite;

        // Автоматически вытягиваем имя для UI магазина
        public string DisplayName => weaponReference != null ? weaponReference.displayName :
                                     (toolReference != null ? toolReference.displayName : inventoryItemName);

        // Автоматически вытягиваем иконку для UI магазина
        public Sprite Icon => weaponReference != null ? weaponReference.weaponSprite :
                               (toolReference != null ? toolReference.itemSprite : inventoryItemSprite);

        // Универсальная проверка: куплен ли товар (чтобы заблокировать кнопку)
        public bool IsPurchasedOrMax()
        {
            if (DataManager.Instance == null) return false;

            // Оружие качается дальше (всегда доступно для покупки)
            if (weaponReference != null) return false;

            // Рюкзак проверяет свою покупку (разблокирован ли инвентарь)
            if (toolReference != null) return toolReference.IsPurchased();

            // Сферы и обычные предметы можно покупать бесконечно в инвентарь, кнопка не блокируется
            return false;
        }

        // Универсальный метод покупки для ShopManager
        public void Buy()
        {
            if (DataManager.Instance == null) return;

            if (weaponReference != null)
            {
                // Покупка/прокачка меча
                int currentLvl = DataManager.Instance.GetWeaponLevel(weaponReference.weaponID);
                DataManager.Instance.SetWeaponLevel(weaponReference.weaponID, currentLvl + 1);
            }
            else if (toolReference != null)
            {
                // Покупка рюкзака (вызовет UnlockInventory)
                toolReference.OnPurchase();
            }
            else if (!string.IsNullOrEmpty(inventoryItemID))
            {
                // Покупка сферы (улетает в инвентарь через ваш метод)
                DataManager.Instance.AddItemToSave(inventoryItemID, 1);
            }

            DataManager.Instance.SaveGame();
        }

        private string GetCurrentID()
        {
            if (weaponReference != null) return weaponReference.weaponID;
            if (toolReference != null) return toolReference.itemID;
            return inventoryItemID;
        }
    }
}
