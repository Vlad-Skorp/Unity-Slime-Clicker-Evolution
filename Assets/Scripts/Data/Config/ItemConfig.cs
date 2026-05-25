using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Config/Entities/ItemConfig")]
    public class ItemConfig : ScriptableObject, IIdentifiable<string>
    {
        public string itemID;

        public string ID => itemID;

        public string displayName;
        [TextArea(3,5)]
        public string description;
        public Sprite itemSprite;
    }
}
