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
        [SerializeField] private int _baseDamage = 1;

        public int BaseDamage => _baseDamage;

        public List<WeaponConfig> allWeapons;

        public int CalculateTotalDamage(GameSaveData saveData)
        {
            if (saveData == null || saveData.Weapons == null || GameDB.Weapons == null)
                return _baseDamage;

            int weaponDamage = saveData.Weapons.Sum(savedWeapon =>
            {
                var config = GameDB.Weapons.GetByID(savedWeapon.weaponID);

                return (config != null) ? config.GetDamageAtLevel(savedWeapon.currentLevel) : 0;;
            });

            return _baseDamage + weaponDamage;
        }
    }
}
