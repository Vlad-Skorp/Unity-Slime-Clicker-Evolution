using SlimeRpgEvolution2D.Core;
using SlimeRpgEvolution2D.Data;
using SlimeRpgEvolution2D.Logic.Effects;
using System;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private EnemyConfig _config;

    private int _currentHealth;

    [Header("Graphics")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;

    [SerializeField] private CapsuleCollider2D _collider;
    public bool IsDead { get; private set; }

    public event Action<float> OnHealthChanged;


    public static event Action<int> OnEnemyKilled;
    public static event Action OnDeathAnimationComplete;

    public void Initialize(EnemyConfig config)
    {
        _config = config;
        _currentHealth = _config.maxHealth;
        _spriteRenderer.sprite = _config.enemySprite;

        IsDead = false;
        _animator.SetBool("IsDead", false);

        if (_collider != null) _collider.enabled = false;

        _animator.SetTrigger("Respawn");
        OnHealthChanged?.Invoke(1f);
    }

    public void SetCombatReady(bool isReady)
    {

        if (IsDead) return;

        if (_collider != null) _collider.enabled = isReady;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        Debug.Log($"[Damage] Слизень получил {damage} урона. Осталось HP: {_currentHealth}");
        _currentHealth = Mathf.Max(0, _currentHealth - damage);

        float healthPercent = (float)_currentHealth / _config.maxHealth;
        OnHealthChanged?.Invoke(healthPercent);

        if (_currentHealth <= 0) Die();
        else
        {
            int randomHit = UnityEngine.Random.Range(0, 3);
            _animator.SetInteger("HitType", randomHit);
            _animator.SetTrigger("Hit");
        }
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        _collider.enabled = false;

        GlobalEvents.SendMoneyEarned(_config.goldReward);

        // 2. РАСЧЕТ ДРОПА ПРЕДМЕТОВ МАТЕМАТИЧЕСКИ
        EnemyDropResult dropResult = _config.RollRandomDrop();

        if (dropResult.itemConfig != null)
        {
            ItemConfig droppedItem = dropResult.itemConfig;
            int amountToGive = dropResult.amount;

            Debug.Log($"<color=green>[ДРОП ВЫПАЛ!]</color> Из слизня выпал предмет: <b>{droppedItem.displayName}</b> (ID: {droppedItem.itemID}) в количестве <b>х{amountToGive}</b>");

            // --- ОТПРАВКА В ИНВЕНТАРЬ ---
            if (DataManager.Instance != null)
            {
                DataManager.Instance.AddItemToSave(droppedItem.itemID, amountToGive);
            }
            else
            {
                Debug.LogError("[Enemy] DataManager.Instance не найден! Предмет не сохранен в инвентарь.");
            }
        }
        else
        {
            Debug.Log("<color=gray>[Дроп]</color> Из слизня ничего не выпало (выпал сектор 'Ничего').");
        }


        if (LootSpawner.Instance != null)
        {
            LootSpawner.Instance.SpawnLootEffects(_config, transform.position, dropResult);
        }
        else
        {
            Debug.LogWarning("[Enemy] LootSpawner.Instance на сцене не найден! Эффекты не будут заспавнены.");
        }


        _animator.ResetTrigger("Hit");
        _animator.SetBool("IsDead", true);
        _animator.SetTrigger("Die");
    }


    public void FinalizeObject()
    {
        OnDeathAnimationComplete?.Invoke(); 
        Destroy(gameObject);
    }
}
