using System;
using UnityEngine;
using SlimeRpgEvolution2D.Core;
using SlimeRpgEvolution2D.Data;


public class Player : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerConfig _currentConfig;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource; 
    [SerializeField] private AudioClip _attackSound;

    public static Player Local;

    public BigNumber Coins => (DataManager.Instance != null && DataManager.Instance.SaveData != null)
       ? DataManager.Instance.SaveData.Coins
       : new BigNumber(0);

    public BigNumber CurrentDamage => DataManager.Instance.GetCurrentDamage();

    public event Action<BigNumber> OnCoinChanged;
    public event Action<BigNumber> OnDamageChanged;

    public static event Action OnStatsChanged;
    public static event Action OnAttackPerformed;

    void Awake()
    {
        Local = this;
    }

    public void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        _currentConfig = DataManager.Instance.GetCurrentPlayerConfig();

        if (_currentConfig != null)
        {
            Debug.Log($"[Player] инициализирован как: {_currentConfig.CharacterID}");
        }

        RefreshUI();
    }

    public void PerformAttack(IDamageable target)
    {
        if (target != null && !target.IsDead)
        {
            target.TakeDamage(CurrentDamage);

            if (_attackSound != null && _audioSource != null)
            {
                _audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                _audioSource.PlayOneShot(_attackSound);
            }

            OnAttackPerformed?.Invoke();
        }
    }

    public void AddCoins(BigNumber amount)
    {
        if (DataManager.Instance == null) return;

        // Напрямую передаем всю гигантскую сумму в кошелек сохранения
        DataManager.Instance.AddCoins(amount);

        // Вызываем событие изменения монет, передавая обновленную структуру в UI
        OnCoinChanged?.Invoke(this.Coins);
    }



    public void RefreshDamage()
    {
        OnDamageChanged?.Invoke(CurrentDamage);
    }


    public void RefreshUI()
    {
        OnCoinChanged?.Invoke(Coins);
        OnDamageChanged?.Invoke(CurrentDamage);
        OnStatsChanged?.Invoke();
    }


    private void OnEnable()
    {
        // Переводим подписку на вынесенный метод вместо безымянной лямбды
        GlobalEvents.OnMoneyEarned += HandleMoneyReward;
        GlobalEvents.OnTargetCliked += PerformAttack;
    }

    private void OnDisable()
    {
        // ИСПРАВЛЕНО: Теперь мы честно и полностью отписываемся от ОБОИХ событий.
        // Никаких "призраков" и скрытого удвоения золота в редакторе больше не будет!
        GlobalEvents.OnMoneyEarned -= HandleMoneyReward;
        GlobalEvents.OnTargetCliked -= PerformAttack;
    }

    private void HandleMoneyReward(BigNumber coinReward)
    {
        // Вызываем ваш собственный метод, который обновит DataManager и дернет OnCoinChanged
        AddCoins(coinReward);
    }

    private void OnDestroy()
    {
        // Если этот конкретный объект игрока уничтожается (например, при смене сцены),
        // зануляем статическую ссылку, чтобы она не указывала на удаленную память
        if (Local == this)
        {
            Local = null;
        }
    }
}
