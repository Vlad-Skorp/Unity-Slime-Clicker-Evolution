using System;

namespace SlimeRpgEvolution2D.Core
{
    public interface IDamageable
    {
        event Action<float> OnHealthChanged;

        void TakeDamage(int damage);
        bool IsDead { get; }

        
    }
}