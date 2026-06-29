using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Config/Entities/ItemConfig")]
    public class ItemConfig : ScriptableObject, IIdentifiable<string>
    {
        [SerializeField] private string itemID;
        public string ID => itemID;

        [SerializeField] private string displayName;
        public string DisplayName => displayName;


        [TextArea(3,5)]
        [SerializeField] private string description;
        public string Description => description;

        [SerializeField]
        private Sprite itemSprite;
        public Sprite Icon => itemSprite;
    }
}
