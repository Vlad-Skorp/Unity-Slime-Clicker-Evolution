using SlimeRpgEvolution2D.Core;
using SlimeRpgEvolution2D.Data;
using SlimeRpgEvolution2D.Logic.Effects;
using System;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private EnemyConfig _config;

    private int _enemyRpgLevel;
    private BigNumber _maxHealth;
    private BigNumber _currentHealth;

    public BigNumber CurrentHealth => _currentHealth;
    public BigNumber MaxHealth => _maxHealth;

    private BigNumber _dynamicGoldReward;


    [Header("Graphics")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;

    [SerializeField] private CapsuleCollider2D _collider;
    public bool IsDead { get; private set; }

    public event Action<float> OnHealthChanged;

    public static event Action<string, int> OnEnemySpawnedUI;

    public static event Action OnDeathAnimationComplete;

    public void Initialize(EnemyConfig config, Sprite enemySprite, BigNumber calculatedMaxHp, BigNumber calculatedGold, int currentLevel)
    {
        _config = config;
        _maxHealth = calculatedMaxHp;       // Накопленное ХП для этого уровня
        _currentHealth = _maxHealth;
        _dynamicGoldReward = calculatedGold; // Увеличенное золото
        _enemyRpgLevel = currentLevel;

        _spriteRenderer.sprite = enemySprite;


        UpdateColliderSize(enemySprite);

        IsDead = false;
        _animator.SetBool("IsDead", false);

        if (_collider != null) _collider.enabled = false;

        _animator.SetTrigger("Respawn");
        OnHealthChanged?.Invoke(1f);

        OnEnemySpawnedUI?.Invoke(_config.enemyName, _enemyRpgLevel);
    }

    public void SetCombatReady(bool isReady)
    {

        if (IsDead) return;

        if (_collider != null) _collider.enabled = isReady;
    }

    public void TakeDamage(BigNumber damage)
    {
        if (IsDead) return;

        // 1. БЕЗОПАСНОЕ ВЫЧИТАНИЕ УРОНА ЧЕРЕЗ ОПЕРАТОР -=
        // Оператор сам займет миллиарды из старших ячеек, если базовый сегмент уйдет в минус!
        _currentHealth -= damage;

        // Для красивого лога используем ваш NumberFormatter или ToString() струкруты
        Debug.Log($"[Damage] Слизень получил {NumberFormatter.Format(damage)} урона. Осталось HP: {NumberFormatter.Format(_currentHealth)}");

        // 2. РАСЧЕТ ПРОЦЕНТА ХП ДЛЯ ПОЛОСКИ СЛАЙДЕРА В UI
        // Метод ToFloat() берет под контроль все 4 сегмента без риска переполнения разрядов double
        float currentF = _currentHealth.ToFloat();
        float maxF = _maxHealth.ToFloat();

        float healthPercent = maxF > 0f ? (currentF / maxF) : 0f;

        // Передаем правильный процент в UI. Ваша корутина SmoothUpdateBar теперь оживет!
        OnHealthChanged?.Invoke(Mathf.Clamp01(healthPercent));

        // 3. ПРОВЕРКА НА СМЕРТЬ
        // Используем наш оператор сравнения <=. Если объект равен 0 или ушел в минус — он мертв
        if (_currentHealth <= 0)
        {
            // На всякий случай страхуем кошелек здоровья чистым нулем перед смертью
            _currentHealth = 0;
            Die();
        }
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

        GlobalEvents.SendMoneyEarned(_dynamicGoldReward);


       

        // 2. РАСЧЕТ ДРОПА ПРЕДМЕТОВ МАТЕМАТИЧЕСКИ
        EnemyDropResult dropResult = _config.RollRandomDrop();

        if (dropResult.itemConfig != null)
        {
            ItemConfig droppedItem = dropResult.itemConfig;
            int amountToGive = dropResult.amount;

            Debug.Log($"<color=green>[ДРОП ВЫПАЛ!]</color> Из слизня выпал предмет: <b>{droppedItem.DisplayName}</b> (ID: {droppedItem.ID}) в количестве <b>х{amountToGive}</b>");

            // --- ОТПРАВКА В ИНВЕНТАРЬ ---
            if (DataManager.Instance != null)
            {
                DataManager.Instance.AddItemToSave(droppedItem.ID, amountToGive);
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

    [ContextMenu("Update Collider Size")]
    private void UpdateColliderSizeDebug()
    {
        if (_spriteRenderer != null)
        {
            UpdateColliderSize(_spriteRenderer.sprite);
        }
    }

    private void UpdateColliderSize(Sprite enemySprite)
    {
        if (_collider == null || enemySprite == null || _spriteRenderer == null) return;

        // 1. Получаем размеры самого спрайта с учетом Pixels Per Unit
        Bounds spriteBounds = enemySprite.bounds;

        // 2. Учитываем масштаб (Scale) объекта, на котором висит SpriteRenderer (объект Visual)
        Vector3 spriteScale = _spriteRenderer.transform.localScale;

        // Вычисляем итоговую ширину и высоту в мировых координатах Unity
        float realWidth = spriteBounds.size.x * spriteScale.x;
        float realHeight = spriteBounds.size.y * spriteScale.y;

        // 3. Устанавливаем точный размер CapsuleCollider2D под спрайт
        _collider.size = new Vector2(realWidth, realHeight);

        // 4. Считаем смещение (Offset) по Y.
        // Так как Pivot выставлен в Bottom Center, центр капсулы должен быть строго на высоте половины спрайта.
        // Также учитываем локальное смещение по Y самого объекта Visual (если оно есть)
        float visualOffsetY = _spriteRenderer.transform.localPosition.y;
        _collider.offset = new Vector2(0f, (realHeight / 2f) + visualOffsetY);

        // 5. Автоматически разворачиваем капсулу: 
        // Horizontal — для широких круглых слизней, Vertical — для высоких вытянутых врагов
        _collider.direction = realWidth > realHeight ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
    }

}
