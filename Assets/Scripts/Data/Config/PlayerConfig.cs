using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace SlimeRpgEvolution2D.Data
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Config/Entities/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [SerializeField] private string _characterID;
        public string CharacterID => _characterID;
        [SerializeField] private string _characterName;
        public string CharacterName => _characterName;


        [Header("Damage Settings")]
        [SerializeField] private BigNumber _baseDamage;
        public BigNumber BaseDamage => _baseDamage;

        public List<WeaponConfig> allWeapons;

        public BigNumber CalculateTotalDamage(GameSaveData saveData)
        {
            // Если данных нет, возвращаем бессмертный базовый урон
            if (saveData == null || saveData.Weapons == null || GameDB.Weapons == null)
                return _baseDamage;

            // Стартуем расчет с копии базового урона игрока
            BigNumber totalDamage = BaseDamage;

          

            // В цикле поразрядно прибавляем урон от каждого прокачанного меча
            foreach (var savedWeapon in saveData.Weapons)
            {
                var config = GameDB.Weapons.GetByID(savedWeapon.weaponID);
                if (config != null)
                {
                    // Получаем урон меча в BigNumber
                    BigNumber weaponDamage = config.GetDamageAtLevel(savedWeapon.currentLevel);

                    // Сливаем (складываем) урон меча с общим уроном персонажа "в столбик"
                    totalDamage = totalDamage + weaponDamage;
                }
            }

            return totalDamage;
        }

    }
}
