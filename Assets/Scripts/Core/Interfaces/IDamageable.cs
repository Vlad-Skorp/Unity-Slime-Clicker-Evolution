using System;

namespace SlimeRpgEvolution2D.Logic
{
    public interface IDamageable
    {
        event Action<float> OnHealthChanged;

        void TakeDamage(int damage);
        bool IsDead { get; }
    }
}