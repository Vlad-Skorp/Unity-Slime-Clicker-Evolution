using System;
using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    [CreateAssetMenu(fileName = "NewBackPack", menuName = "Config/Entities/ToolConfig/BackPack")]
    public class BackpackConfig : ToolConfig
    {
        public static event Action OnBackpackPurchased;

        // Рюкзак проверяет ваше свойство IsInventoryUnlocked
        public override bool IsPurchased()
        {
            if (DataManager.Instance == null || DataManager.Instance.SaveData == null) return false;
            return DataManager.Instance.SaveData.IsInventoryUnlocked;
        }

        // Рюкзак сам знает, какой метод вызвать для активации
        public override void OnPurchase()
        {
            if (DataManager.Instance == null) return;
            DataManager.Instance.UnlockInventory();

            OnBackpackPurchased?.Invoke();
        }
    }
}
