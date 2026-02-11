using UnityEngine;
using System.Collections.Generic;


namespace SlimeRpgEvolution2D.Data
{

    [System.Serializable]
    public struct WeaponSaveData
    {
        public string weaponID;
        public int currentLevel;

        public WeaponSaveData(string id, int level)
        {
            weaponID = id;
            currentLevel = level;
        }
    }

    [System.Serializable]
    public class GameSaveData
    {
        [SerializeField] private int _coins;
        public int Coins => _coins;

        [SerializeField] private List<WeaponSaveData> _weapons = new List<WeaponSaveData>();
        public IReadOnlyList<WeaponSaveData> Weapons => _weapons;

        public string selectedCharacterID = "DefaultPlayer";

        public GameSaveData()
        {
            _coins = 0;
            selectedCharacterID = "DefaultPlayer";
            _weapons = new List<WeaponSaveData>();
        }

        public void UpdateCoins(int amount, DataManager.AccessKey key)
        {
            if (key == null) return;
            _coins = amount;
        }

        public void UpdateWeapons(List<WeaponSaveData> weaponList, DataManager.AccessKey key)
        {
            if (key == null) return;
            _weapons = weaponList;
        }
    }    
}
