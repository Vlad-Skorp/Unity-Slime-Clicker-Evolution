using SlimeRpgEvolution2D.UI.Popups;
using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    public interface IShopItem : IIdentifiable<string>
    {
        string DisplayName { get; }
        Sprite Icon { get; }
        ShopTabType TabCategory { get; } // Чтобы база знала, на какую вкладку отправить предмет

        // Универсальные методы, которые мы уже настроили
        bool IsOwnedOrMax();
        int GetPrice();
        void HandlePurchase();
    }
}
