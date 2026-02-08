using System.Collections.Generic;

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
        public int coins;
        public List<WeaponSaveData> weapons = new List<WeaponSaveData>();
    }    
}
