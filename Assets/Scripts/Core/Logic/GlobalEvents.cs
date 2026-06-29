using SlimeRpgEvolution2D.Data;
using System;
using UnityEngine;

namespace SlimeRpgEvolution2D.Core
{
    public static class GlobalEvents
    {
        public static event Action<IDamageable> OnTargetCliked;
        public static event Action<BigNumber> OnMoneyEarned;


        public static void SendTargetCliked(IDamageable target) => OnTargetCliked?.Invoke(target);

        public static void SendMoneyEarned(BigNumber amount) => OnMoneyEarned?.Invoke(amount);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticEvents()
        {
            OnTargetCliked = null;
            OnMoneyEarned = null;
            Debug.Log("<color=#7bed9f>[GlobalEvents] Все статические подписки успешно обнулены для новой сессии!</color>");
        }

    }
}
