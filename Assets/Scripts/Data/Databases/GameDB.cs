
using System.Collections.Generic;
using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{

    public static class GameDB
    {
        public static WeaponDatabase Weapons { get; internal set; }


        private static Dictionary<string, WeaponConfig> _weaponCache;

        public static void Initialize(WeaponDatabase weaponDb)
        {
            Weapons = weaponDb;
            Weapons.Initialize();
            Debug.Log("<color=green>[GameDB]</color> Все базы данных успешно подключены!");
        }
    }

}