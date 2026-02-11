using UnityEngine;
using SlimeRpgEvolution2D.Data;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private WeaponDatabase _weaponDb;

    void Awake()
    {
        // Вот здесь магия оживает:
        GameDB.Initialize(_weaponDb);

        // После этого в любом скрипте проекта можно писать:
        // GameDB.Weapons.GetByID("...");
    }
}
