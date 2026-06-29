using SlimeRpgEvolution2D.Data;
using UnityEngine;
using System.Collections;

namespace SlimeRpgEvolution2D.Logic // Вы можете добавить свой namespace, если нужно
{

    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private Transform _spawnPoint;

        [Tooltip("Перетащите сюда ваш белый квадрат UI_Enemy_Limit со сцены")]
        [SerializeField] private SpriteRenderer _areaLimit;

        [Header("Spawn Settings")]
        [SerializeField] private float _minSpawnDelay = 0.5f;
        [SerializeField] private float _maxSpawnDelay = 1f;


        private GameObject currentActiveEnemy;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }



        private void OnEnable() => Enemy.OnDeathAnimationComplete += HandleEnemyKilled;
        private void OnDisable() => Enemy.OnDeathAnimationComplete -= HandleEnemyKilled;

        public void SpawnDynamicEnemy(EnemyConfig config, Sprite enemySprite, BigNumber hp, BigNumber gold, int level)
        {
            if (_spawnPoint == null || _enemyPrefab == null) return;

            // ЗАЩИТА: Если на поляне КТО-ТО УЖЕ СТОИТ (например, при смене этапа стрелочкой) — уничтожаем его!
            if (currentActiveEnemy != null)
            {
                Destroy(currentActiveEnemy);
            }

            // Создаем префаб слайма
            GameObject enemyObj = Instantiate(_enemyPrefab, _spawnPoint.position, Quaternion.identity);

            // Запоминаем его, чтобы при следующем перелистывании уничтожить
            currentActiveEnemy = enemyObj;


            if (enemyObj.TryGetComponent(out Enemy enemy))
            {
                // Передаем в слайма его персональный спрайт, ХП, золото и уровень для этой битвы
                enemy.Initialize(config, enemySprite, hp, gold, level);
                enemy.SetCombatReady(true);
            }

            UIEnemyInfo uiInfo = enemyObj.GetComponentInChildren<UIEnemyInfo>();
            if (uiInfo != null)
            {
                uiInfo.SetLimit(_areaLimit);
            }
        }


        private void HandleEnemyKilled()
        {
            if (GameLevelManager.Instance != null)
            {
                GameLevelManager.Instance.OnEnemyDefeated();
            }

            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            float delay = Random.Range(_minSpawnDelay, _maxSpawnDelay);
            yield return new WaitForSeconds(delay);

            if (GameLevelManager.Instance != null)
            {
                // 1. И только после этого просим создать нового случайного моба для текущего этапа
                GameLevelManager.Instance.RequestNextEnemySpawn();
            }
            else
            {
                Debug.LogError("[EnemySpawner] GameLevelManager.Instance не найден!");
            }
        }

    }
}