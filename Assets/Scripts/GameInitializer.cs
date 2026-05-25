using UnityEngine;
using SlimeRpgEvolution2D.Data;
using System;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private WeaponDatabase _weaponDb;
    [SerializeField] private ItemDatabase _itemDb;
    [SerializeField] private ToolDatabase _toolDb;

    [SerializeField] private ShopDatabase _shopDb;
    void Awake()
    {
        // Вот здесь магия оживает:
        GameDB.Initialize(_weaponDb, _itemDb, _toolDb, _shopDb);

        // После этого в любом скрипте проекта можно писать:
        // GameDB.Weapons.GetByID("...");
    }
}
