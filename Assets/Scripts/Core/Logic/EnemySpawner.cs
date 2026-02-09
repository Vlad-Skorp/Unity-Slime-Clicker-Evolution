using SlimeRpgEvolution2D.Data;
using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private Transform _spawnPoint;

    [Header("Current Level Configuration")]
    [SerializeField] private LevelSettings _currentLevel;

    [Header("Spawn Settings")]
    [SerializeField] private float _minSpawnDelay = 1f;
    [SerializeField] private float _maxSpawnDelay = 2f;

    private void OnEnable() => Enemy.OnDeathAnimationComplete += HandleEnemyKilled;
    private void OnDisable() => Enemy.OnDeathAnimationComplete -= HandleEnemyKilled;

    private void Start()
    {
        SpawnEnemy();
    }

    public void SpawnEnemy()
    {
        if (_currentLevel == null)
        {
            Debug.LogError("EnemySpawner: Current Level Settings не назначены!");
            return;
        }

        EnemyConfig randomEnemyData = _currentLevel.GetRandomEnemy();

        GameObject enemyObj = Instantiate(_enemyPrefab, _spawnPoint.position, Quaternion.identity);

        if(enemyObj.TryGetComponent(out Enemy enemy))
        {
            enemy.Initialize(randomEnemyData);
        }
    }


    private void HandleEnemyKilled()
    { 
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        float delay = Random.Range(_minSpawnDelay, _maxSpawnDelay);
        yield return new WaitForSeconds(delay);

        SpawnEnemy();
    }
}
