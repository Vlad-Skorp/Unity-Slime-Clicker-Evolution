using SlimeRpgEvolution2D.Data;
using System;

namespace SlimeRpgEvolution2D.Core
{
    public interface IDamageable
    {
        event Action<float> OnHealthChanged;

        BigNumber CurrentHealth { get; }
        BigNumber MaxHealth { get; }

        void TakeDamage(BigNumber damage);
        bool IsDead { get; }

        
    }
}