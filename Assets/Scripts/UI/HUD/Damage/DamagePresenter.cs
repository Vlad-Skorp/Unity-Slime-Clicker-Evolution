using UnityEngine;

namespace SlimeRpgEvolution2D.UI.HUD
{
    public class DamagePresenter : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private CounterView _view;

        private void OnEnable()
        {
            if (Player.Local != null)
            {
                Player.Local.OnDamageChanged += HandleUpdate;
                HandleUpdate(Player.Local.CurrentDamage);
            }
        }

        private void OnDisable()
        {
            if (Player.Local != null) Player.Local.OnDamageChanged -= HandleUpdate;
        }

        private void HandleUpdate(int amount)
        {
            _view.SetValue(amount);
        }
    }
}
