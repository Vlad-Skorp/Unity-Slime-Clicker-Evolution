using DG.Tweening;
using SlimeRpgEvolution2D.Data;
using UnityEngine;

namespace SlimeRpgEvolution2D.UI.HUD
{
    public class DamagePresenter : BasePresenter
    {
        protected override void Subscribe() => Player.Local.OnDamageChanged += HandleUpdate;
        protected override void Unsubscribe() => Player.Local.OnDamageChanged -= HandleUpdate;
        protected override BigNumber GetCurrentValue() => Player.Local.CurrentDamage;
    }
}
