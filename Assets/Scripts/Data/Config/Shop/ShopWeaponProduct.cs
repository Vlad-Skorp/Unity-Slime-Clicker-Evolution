using SlimeRpgEvolution2D.UI.Popups;
using System;
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
        public BigNumber basePurchasePrice;

        [Tooltip("Коэффициент удорожания цены за уровень прокачки")]
        public float priceMultiplier = 1.5f;

        [Tooltip("Максимальный уровень прокачки меча в магазине (например, 10 для первой палки)")]
        [SerializeField] private int _maxWeaponLevel = 10;

        public override string ID => weaponReference != null ? weaponReference.ID : string.Empty;
        public override string DisplayName => weaponReference != null ? weaponReference.DisplayName : "Неизвестный меч";
        public override Sprite Icon => weaponReference != null ? weaponReference.Icon : null;

        public int MaxWeaponLevel => _maxWeaponLevel;

        public override BigNumber GetCurrentPrice()
        {
            if (weaponReference == null || DataManager.Instance == null) return new BigNumber(0);

            // Динамический расчет цены на основе уровня из сохранения
            int currentLevel = DataManager.Instance.GetWeaponLevel(ID);

            // Добавляем букву 'd' к каждому множителю (1000000000d). 
            // Это превращает их в double прямо на этапе компиляции, убирая лимиты и переполнения!
            double totalBasePrice = basePurchasePrice.ToDouble();

            if (currentLevel >= _maxWeaponLevel) return new BigNumber(0);

            // 2. ВЫЧИСЛЕНИЕ: Теперь умножаем обычный double на double. Компилятор будет доволен!
            double calculatedCost = totalBasePrice * Math.Pow(priceMultiplier, currentLevel);

            // 3. УПАКОВКА: Возвращаем через конструктор, который сам распилит double обратно на 4 ячейки
            return new BigNumber(calculatedCost);
        }

        public override bool IsPurchasedOrMax()
        {
            if (weaponReference == null || DataManager.Instance == null) return false;

            int currentLevel = DataManager.Instance.GetWeaponLevel(ID);
            return currentLevel >= _maxWeaponLevel;
        }

        public override void Buy()
        {
            if (weaponReference == null || DataManager.Instance == null) return;

            int currentLvl = DataManager.Instance.GetWeaponLevel(ID);

            // ЗАЩИТА: Не даем качать меч выше лимита, если игрок умудрился нажать кнопку
            if (currentLvl >= _maxWeaponLevel) return;

            DataManager.Instance.SetWeaponLevel(ID, currentLvl + 1);
            DataManager.Instance.SaveGame();
        }
    }
}
