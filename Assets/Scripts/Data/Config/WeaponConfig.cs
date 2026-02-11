using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Config/Entities/WeaponConfig")]
    public class WeaponConfig : ScriptableObject, IIdentifiable<string>
    {
        public string weaponID;

        public string ID => weaponID;

        public string displayName;
        public int baseDamageBonus;
        public int basePurchasePrice;
        public Sprite weaponSprite;

        [Header("Progression Setings")]
        [Tooltip("Коэффицент роста за прокачку")]
        public float damageMultiplier = 1.2f;
        public float priceMultiplier = 1.5f;

        public int GetDamageAtLevel(int level)
        {
            if (level <= 0) return 0;
            return Mathf.RoundToInt(baseDamageBonus * Mathf.Pow(damageMultiplier, level - 1));
        }

        public int GetUpgradePrice(int currentLevel)
        {
            return Mathf.RoundToInt(basePurchasePrice * Mathf.Pow(priceMultiplier, currentLevel));
        }
    }
}