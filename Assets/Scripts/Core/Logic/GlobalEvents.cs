using System;

namespace SlimeRpgEvolution2D.Core
{
    public static class GlobalEvents
    {
        public static event Action<IDamageable> OnTargetCliked;
        public static event Action<int> OnMoneyEarned;


        public static void SendTargetCliked(IDamageable target) => OnTargetCliked?.Invoke(target);

        public static void SendMoneyEarned(int amount) => OnMoneyEarned?.Invoke(amount);

    }
}
