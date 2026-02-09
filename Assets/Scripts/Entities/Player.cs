using System;
using SlimeRpgEvolution2D.Core;
using SlimeRpgEvolution2D.Data;
using SlimeRpgEvolution2D.Logic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerConfig _currentConfig;

    public int Coins => DataManager.Instance.SaveData != null ? DataManager.Instance.SaveData.coins : 0;
    public int CurrentDamage => DataManager.Instance.GetCurrentDamage();

    public static event Action<int> OnCoinChanged;
    public static event Action OnStatsChanged;
    public static event Action OnAttackPerformed;

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
    }


    public void RefreshUI()
    {
        OnCoinChanged?.Invoke(Coins);
        OnStatsChanged?.Invoke();
    }


    private void OnEnable()
    {
        Enemy.OnEnemyKilled += AddCoins;
        GlobalEvents.OnTargetCliked += PerformAttack;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyKilled -= AddCoins;
        GlobalEvents.OnTargetCliked -= PerformAttack;
    }
}
