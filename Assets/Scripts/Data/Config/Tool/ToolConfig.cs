using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    [CreateAssetMenu(fileName = "NewTool", menuName = "Config/Entities/ToolConfig")]
    public abstract class ToolConfig : ScriptableObject, IIdentifiable<string>
    {
        public string itemID;
        public string ID => itemID;

        public string displayName;
        public Sprite itemSprite;

        // 1. Метод проверки: куплен ли этот конкретный предмет?
        public abstract bool IsPurchased();

        // 2. Метод действия: что происходит в сохранениях при покупке?
        public abstract void OnPurchase();
    }
}