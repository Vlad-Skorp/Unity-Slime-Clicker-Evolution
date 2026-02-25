using DG.Tweening;
using UnityEngine;

namespace SlimeRpgEvolution2D.UI.HUD
{
    public class DamagePresenter : BasePresenter
    {
        protected override void Subscribe() => Player.Local.OnDamageChanged += HandleUpdate;
        protected override void Unsubscribe() => Player.Local.OnDamageChanged -= HandleUpdate;
        protected override int GetCurrentValue() => Player.Local.CurrentDamage;


        protected override void HandleUpdate(int amount)
        {
            base.HandleUpdate(amount); 
        }
    }
}
