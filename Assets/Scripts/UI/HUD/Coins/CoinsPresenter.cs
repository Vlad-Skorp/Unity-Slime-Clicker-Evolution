using UnityEngine;

namespace SlimeRpgEvolution2D.UI.HUD
{
    public class CoinsPresenter : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private CoinView _view;

        private void OnEnable()
        {
            Player.OnCoinChanged += HandleUpdate;

            var activePlayer = Object.FindFirstObjectByType<Player>();

            if (activePlayer != null) HandleUpdate(activePlayer.Coins);
            
        }


        private void OnDisable()
        {
            Player.OnCoinChanged -= HandleUpdate;
        }

        private void HandleUpdate(int amount)
        {
            _view.SetCoin(amount);
        }
    }
}
