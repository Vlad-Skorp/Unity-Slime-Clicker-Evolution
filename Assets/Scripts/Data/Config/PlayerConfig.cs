using SlimeRpgEvolution2D.Data;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName ="Data/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    [Header("Damage Settings")]
    [SerializeField] private int _baseDamage = 1;

    public int BaseDamage => _baseDamage;

    public List<WeaponConfig> allWeapons;

    public int CalculateTotalDamage(GameSaveData saveData)
    {
        int total = _baseDamage;

        foreach(var savedWeapon in saveData.weapons)
        {
            WeaponConfig config = allWeapons.Find(w => w.weaponID == savedWeapon.weaponID);

            if(config != null)
            {
                total += config.GetDamageAtLevel(savedWeapon.currentLevel);
            }
        }

        return total;
    }
}
