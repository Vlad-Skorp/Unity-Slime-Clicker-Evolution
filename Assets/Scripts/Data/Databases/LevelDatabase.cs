using UnityEngine;
using System.Collections.Generic;

namespace SlimeRpgEvolution2D.Data
{
    [CreateAssetMenu(fileName = "LevelsDatabase", menuName = "Config/Database/LevelsDatabase")]
    public class LevelDatabase : BaseDatabase<LevelSettings, string>
    {
        // База будет хранить конфиги LevelSettings (которые мы разбирали ранее) 
        // и искать их по интовому ID (номеру уровня levelNumber)
    }
}

