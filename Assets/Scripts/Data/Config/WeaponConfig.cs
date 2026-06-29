using System;
using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Config/Entities/WeaponConfig")]
    public class WeaponConfig : ScriptableObject, IIdentifiable<string>
    {
        [SerializeField] private string weaponID;
        public string ID => weaponID;


        [SerializeField] private string weaponName;
        public string DisplayName => weaponName;

        [Header("Damage Settings")]
        [SerializeField] private BigNumber baseDamageBonus;
        public BigNumber BaseDamageBonus => baseDamageBonus;


        [SerializeField] private Sprite weaponSprite;
        public Sprite Icon => weaponSprite;


        [Header("Progression Setings")]
        [Tooltip("Коэффицент роста за прокачку")]
        [SerializeField] private float damageMultiplier = 1.2f;

        public BigNumber GetDamageAtLevel(int level)
        {
            if (level <= 0) return new BigNumber(0);

            // 1. Распаковываем базовую структуру урона оружия в чистый double (собираем все этажи)
            double baseDamageDouble = baseDamageBonus.ToDouble();

            // 2. Считаем экспоненциальный рост уровня на основе чистого double
            double calculatedDamage = baseDamageDouble * Math.Pow((double)damageMultiplier, level - 1);

            // 3. Запаковываем итоговый double обратно в структуру BigNumber для всей остальной игры
            return new BigNumber(calculatedDamage);
        }
    }
}