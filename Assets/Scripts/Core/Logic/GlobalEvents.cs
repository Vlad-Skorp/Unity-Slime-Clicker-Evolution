using System;
using SlimeRpgEvolution2D.Logic;

namespace SlimeRpgEvolution2D.Core
{
    public static class GlobalEvents
    {
        public static event Action<IDamageable> OnTargetCliked;

        public static void SendTargetCliked(IDamageable target)
        {
            OnTargetCliked?.Invoke(target);
        }
    }
}
