using UnityEngine;

namespace SlimeRpgEvolution2D.UI.HUD
{
    public class CoinsPresenter : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private CoinView _view;

        private void OnEnable()
        {
            if (Player.Local != null)
            {
                Player.Local.OnCoinChanged += HandleUpdate;
                HandleUpdate(Player.Local.Coins);
            }

        }


        private void OnDisable()
        {
            if (Player.Local != null) Player.Local.OnCoinChanged -= HandleUpdate;
        }

        private void HandleUpdate(int amount)
        {
            _view.SetCoin(amount);
        }
    }
}
