using SlimeRpgEvolution2D.UI.Popups;
using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    // --- 1. ТОВАР: ОРУЖИЕ (МЕЧИ) ---
    [CreateAssetMenu(fileName = "Shop_Weapon_", menuName = "Config/Shop/Products/Weapon")]
    public class ShopWeaponProduct : ShopProductConfig
    {
        [Header("Настройки Оружия")]
        [Tooltip("Перетащите сюда WeaponConfig этого меча")]
        public WeaponConfig weaponReference;

        [Tooltip("Базовая (начальная) цена меча 1-го уровня в этом магазине")]
        public int basePurchasePrice = 100;

        [Tooltip("Коэффициент удорожания цены за уровень прокачки")]
        public float priceMultiplier = 1.5f;

        public override string ID => weaponReference != null ? weaponReference.weaponID : string.Empty;
        public override string DisplayName => weaponReference != null ? weaponReference.displayName : "Неизвестный меч";
        public override Sprite Icon => weaponReference != null ? weaponReference.weaponSprite : null;

        public override int GetCurrentPrice()
        {
            if (weaponReference == null || DataManager.Instance == null) return 0;

            // Динамический расчет цены на основе уровня из сохранения
            int currentLevel = DataManager.Instance.GetWeaponLevel(ID);
            return Mathf.RoundToInt(basePurchasePrice * Mathf.Pow(priceMultiplier, currentLevel));
        }

        public override bool IsPurchasedOrMax() => false; // Мечи качаются бесконечно

        public override void Buy()
        {
            if (weaponReference == null || DataManager.Instance == null) return;

            // Логика оружия: увеличиваем уровень/урон меча в сохранении
            int currentLvl = DataManager.Instance.GetWeaponLevel(ID);
            DataManager.Instance.SetWeaponLevel(ID, currentLvl + 1);
            DataManager.Instance.SaveGame();
        }
    }
}
