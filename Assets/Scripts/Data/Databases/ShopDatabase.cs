using SlimeRpgEvolution2D.UI.Popups;
using System.Collections.Generic;
using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    [CreateAssetMenu(fileName = "ShopDatabase", menuName = "Config/Database/ShopDatabase")]
    public class ShopDatabase : ScriptableObject
    {
        [Header("--- Списки товаров для удобства в Инспекторе ---")]

        [Tooltip("Перетаскивайте сюда только созданные ассеты Оружия (ShopWeaponProduct)")]
        public List<ShopWeaponProduct> weaponsList = new List<ShopWeaponProduct>();

        [Tooltip("Перетаскивайте сюда только созданные ассеты Инструментов/Рюкзаков (ShopToolProduct)")]
        public List<ShopToolProduct> toolsList = new List<ShopToolProduct>();

        [Tooltip("Перетаскивайте сюда только созданные ассеты Сфер и предметов инвентаря (ShopInventoryItemProduct)")]
        public List<ShopInventoryItemProduct> inventoryItemsList = new List<ShopInventoryItemProduct>();

        /// <summary>
        /// Универсальный метод фильтрации для менеджера магазина. 
        /// Он сам соберет все предметы из раздельных списков и отфильтрует по нужной вкладке UI.
        /// </summary>
        public List<ShopProductConfig> GetProductsForTab(ShopTabType tabType)
        {
            List<ShopProductConfig> filtered = new List<ShopProductConfig>();

            // 1. Проверяем список оружия
            foreach (var weapon in weaponsList)
            {
                if (weapon != null && weapon.CanBeSold && weapon.tabCategory == tabType)
                {
                    filtered.Add(weapon);
                }
            }

            // 2. Проверяем список инструментов
            foreach (var tool in toolsList)
            {
                if (tool != null && tool.CanBeSold && tool.tabCategory == tabType)
                {
                    filtered.Add(tool);
                }
            }

            // 3. Проверяем список обычных предметов (сфер)
            foreach (var item in inventoryItemsList)
            {
                if (item != null && item.CanBeSold && item.tabCategory == tabType)
                {
                    filtered.Add(item);
                }
            }

            return filtered;
        }
    }
}
