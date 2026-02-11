using System;
using UnityEngine;
using SlimeRpgEvolution2D.Core;
using SlimeRpgEvolution2D.Data;


public class Player : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerConfig _currentConfig;

    public static Player Local;

    public int Coins => (DataManager.Instance != null && DataManager.Instance.SaveData != null)
    ? DataManager.Instance.SaveData.Coins
    : 0;
    public int CurrentDamage => DataManager.Instance.GetCurrentDamage();

    public event Action<int> OnCoinChanged;
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
            OnAttackPerformed?.Invoke();
        }
    }

    public void AddCoins(int amount)
    {
        DataManager.Instance.AddCoins(amount);

        OnCoinChanged?.Invoke(this.Coins);
    }


    public void RefreshUI()
    {
        OnCoinChanged?.Invoke(Coins);
        OnStatsChanged?.Invoke();
    }


    private void OnEnable()
    {
        GlobalEvents.OnMoneyEarned += AddCoins;
        GlobalEvents.OnTargetCliked += PerformAttack;
    }

    private void OnDisable()
    {
        GlobalEvents.OnMoneyEarned -= AddCoins;
        GlobalEvents.OnTargetCliked -= PerformAttack;
    }
}
