using UnityEngine;
using System.Collections.Generic;
using System.Linq;


namespace SlimeRpgEvolution2D.Data
{
    [CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Config/Database/WeaponDatabase")]
    public class WeaponDatabase : BaseDatabase<WeaponConfig, string> 
    {
    }
}
