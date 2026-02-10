using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Config/Entities/WeaponConfig")]
    public class WeaponConfig : ScriptableObject
    {
        public string weaponID;
        public string dispalyName;
        public int baseDamageBonus;
        public int basePurchasePrice;

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