using DG.Tweening;
using SlimeRpgEvolution2D.Data;
using TMPro;
using UnityEngine;


namespace SlimeRpgEvolution2D.UI.HUD
{
    public class CounterView : MonoBehaviour
    {
        [SerializeReference] private TextMeshProUGUI _text;

        public TextMeshProUGUI Text => _text;

        [Header("Animation Settings")]
        [SerializeField] private float _duration = 0.3f;


        [SerializeField] private bool _useCounting;

        private Sequence _mainSequence;

        public void SetValue(BigNumber coinsValue, bool animate)
        {
            // Используем наш глобальный NumberFormatter без дублирования кода!
            string formattedText = NumberFormatter.Format(coinsValue);
            SetValueText(formattedText, animate);
        }

        public void SetValueText(string textValue, bool animate)
        {
            _mainSequence?.Kill(true);

            if (_text != null)
            {
                _text.text = textValue;
            }

            if (animate)
            {
                transform.localScale = Vector3.one;
                _mainSequence = DOTween.Sequence();
                _mainSequence.Join(transform.DOPunchScale(new Vector3(0.15f, -0.15f, 0), _duration));
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }
    }
}
