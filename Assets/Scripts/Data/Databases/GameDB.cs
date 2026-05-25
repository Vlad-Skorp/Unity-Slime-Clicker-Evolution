
using System.Collections.Generic;
using UnityEngine;

namespace SlimeRpgEvolution2D.Data
{

    public static class GameDB
    {
        public static WeaponDatabase Weapons { get; internal set; }
        public static ItemDatabase Items { get; internal set; }
        public static ToolDatabase Tools { get; internal set; }


        public static ShopDatabase Shop { get; internal set; }

        public static void Initialize(WeaponDatabase weaponDb, ItemDatabase itemDb, ToolDatabase toolDb, ShopDatabase shopDb)
        {
            Weapons = weaponDb;
            if(Weapons != null) Weapons.Initialize();

            Items = itemDb;
            if (Items != null) Items.Initialize();

            Tools = toolDb;
            if (Tools != null) Tools.Initialize();

            Shop = shopDb;

            Debug.Log("<color=green>[GameDB]</color> Все базы данных успешно подключены!");
        }
    }
}