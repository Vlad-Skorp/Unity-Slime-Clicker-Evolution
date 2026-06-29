using DG.Tweening;
using SlimeRpgEvolution2D.Data;
using UnityEngine;

namespace SlimeRpgEvolution2D.UI.HUD
{
    public class CoinsPresenter : BasePresenter
    {
        [Header("Settings")]
        [SerializeField] private Color _gainColor = Color.green;
        [SerializeField] private Color _spendColor = Color.red;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _gainSound;
        [SerializeField] private AudioClip _spendSound;


        private int _lastBaseValue;

        protected override void Subscribe() => Player.Local.OnCoinChanged += HandleUpdate;
        protected override void Unsubscribe() => Player.Local.OnCoinChanged -= HandleUpdate;

        protected override BigNumber GetCurrentValue() => Player.Local.Coins;

        protected override void OnValueIncreased(BigNumber newValue)
        {
            if (_view.Text != null)
            {
                _view.Text.DOKill();
                _view.Text.DOColor(_gainColor, 0.1f).OnComplete(() => _view.Text.DOColor(Color.white, 0.3f));
            }
            PlaySound(_gainSound);
        }

        protected override void OnValueDecreased(BigNumber newValue)
        {
            if (_view.Text != null)
            {
                _view.Text.DOKill();
                _view.Text.DOColor(_spendColor, 0.1f).OnComplete(() => _view.Text.DOColor(Color.white, 0.3f));
            }

            // Трясем всю плашку кошелька для сочности
            transform.DOKill(true);
            transform.DOShakePosition(0.3f, strength: new Vector3(10, 0, 0), vibrato: 20);

            PlaySound(_spendSound);
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }
    }
}
