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
            if (saveData == null) return _baseDamage;

            int weaponDamage = saveData.weapons?.Sum(savedWeapon =>
            {
                if (savedWeapon == null) return 0;

                var config = allWeapons.Find(w => w.weaponID == savedWeapon.weaponID);
                return config != null ? config.GetDamageAtLevel(savedWeapon.currentLevel) : 0;

            }) ?? 0;

            return _baseDamage + weaponDamage;
        }
    }
}
