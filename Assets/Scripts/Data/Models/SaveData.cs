using System.Collections.Generic;
using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{

    [System.Serializable]
    public class WeaponSaveData
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
        
        public string selectedCharacterID = "DefaultPlayer";
        public List<WeaponSaveData> weapons = new List<WeaponSaveData>();

        public GameSaveData()
        {
            _coins = 0;
            selectedCharacterID = "DefaultPlayer";
            weapons = new List<WeaponSaveData>();
        }

        public void UpdateCoins(int amount, DataManager.AccessKey key)
        {
            if (key == null) return;
            _coins = amount;
        }
    }    
}
