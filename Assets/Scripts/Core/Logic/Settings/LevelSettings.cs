using SlimeRpgEvolution2D.Data;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelSettings", menuName = "Configs/Levels/LevelSettings")]
public class LevelSettings : ScriptableObject
{
    public int levelNumber;
    public Sprite backgroundSprite;

    public List<EnemySpawnChance> availableEnemies;

    [Header("Debug Info")]
    public List<string> chancePreview;

    public EnemyConfig GetRandomEnemy()
    {
        float totalWight = 0;
        foreach (var entry in availableEnemies) totalWight += entry.spawnWight;

        float randomValue = Random.Range(0, totalWight);
        float currentWight = 0;

        foreach(var entry in availableEnemies)
        {
            currentWight += entry.spawnWight;
            if(randomValue <= currentWight) return entry.enemyConfig;
        }

        return availableEnemies[0].enemyConfig;
    }

    public float GetEnemyChancePercent(EnemyConfig config)
    {
        float totalWeight = 0;
        float targetWeight = 0;

        foreach(var entry in availableEnemies)
        {
            totalWeight += entry.spawnWight;
            if(entry.enemyConfig == config) targetWeight += entry.spawnWight;
        }

        if (totalWeight <= 0) return 0;

        return (targetWeight / totalWeight) * 100f;
    }

    private void OnValidate()
    {
        chancePreview.Clear();
        float total = 0;
        foreach(var e in availableEnemies) total += e.spawnWight;

        if(total <= 0) return;
        
        foreach( var e in availableEnemies)
        {
            float pc = (e.spawnWight / total) * 100f;
            chancePreview.Add($"{e.enemyConfig.name}: {pc:F1}%");
        }
    }
}

[System.Serializable]
public struct EnemySpawnChance
{
    public EnemyConfig enemyConfig;
    [Tooltip("Чем выше число, тем чаще выпадает. Например: Обычный = 100, Редкий 5")]
    public float spawnWight;
}
