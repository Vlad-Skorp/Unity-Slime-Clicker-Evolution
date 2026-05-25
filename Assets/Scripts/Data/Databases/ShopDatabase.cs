using SlimeRpgEvolution2D.Data;
using SlimeRpgEvolution2D.UI.Popups;
using System.Collections.Generic;
using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    [CreateAssetMenu(fileName = "ShopDatabase", menuName = "Config/Database/ShopDatabase")]
    public class ShopDatabase : BaseDatabase<ShopProductConfig, string>
    {
        // Метод фильтрации предметов по вкладкам для менеджера
        public List<ShopProductConfig> GetProductsForTab(ShopTabType tabType)
        {
            List<ShopProductConfig> filtered = new List<ShopProductConfig>();
            foreach (var product in AllEntries)
            {
                if (product != null && product.tabCategory == tabType)
                {
                    filtered.Add(product);
                }
            }
            return filtered;
        }
    }
}
