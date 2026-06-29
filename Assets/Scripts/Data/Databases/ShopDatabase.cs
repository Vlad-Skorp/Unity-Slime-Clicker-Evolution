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

            // =============================================================
            // 1. ПРОВЕРКА ОРУЖИЯ С ЛОГИКОЙ ПОСТЕПЕННОГО ОТКРЫТИЯ
            // =============================================================
            for (int i = 0; i < weaponsList.Count; i++)
            {
                var weapon = weaponsList[i];
                if (weapon == null) continue;

                // Если это оружие относится к другой вкладке (на всякий случай), просто пропускаем
                if (weapon.tabCategory != tabType) continue;

                // Если товар в принципе выключен через админку — пропускаем
                if (!weapon.CanBeSold) continue;

                // Получаем текущий уровень этого оружия у игрока из DataManager
                int currentLevel = DataManager.Instance.GetWeaponLevel(weapon.ID);

                // Условие 1: Самый первый меч в списке (i == 0) или уже купленный меч (currentLevel > 0) показываем ВСЕГДА
                if (i == 0 || currentLevel > 0)
                {
                    filtered.Add(weapon);
                    continue;
                }

                // Условие 2: Если этот меч еще не куплен (уровень 0), проверяем предыдущий меч в списке инспектора
                var previousWeapon = weaponsList[i - 1];
                int previousWeaponLevel = DataManager.Instance.GetWeaponLevel(previousWeapon.ID);

                // Если предыдущий меч прокачан хотя бы на 1 уровень, то текущий меч становится видимым в магазине
                if (previousWeaponLevel > 0)
                {
                    filtered.Add(weapon);
                }
                else
                {
                    // КРИТИЧЕСКИЙ МОМЕНТ: Если прошлый меч не куплен, то текущий и ВСЕ СЛЕДУЮЩИЕ за ним мечи
                    // остаются заблокированными. Прерываем цикл оружия досрочно!
                    break;
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
