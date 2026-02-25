using DG.Tweening;
using TMPro;
using UnityEngine;


namespace SlimeRpgEvolution2D.UI.HUD
{
    public class CounterView : MonoBehaviour
    {
        [SerializeReference] private TextMeshProUGUI _text;

        public TextMeshProUGUI Text => _text;




        [SerializeField] private bool _useCounting; 
        [SerializeField] private float _duration = 0.3f;

        private int _currentValue;

        private Sequence _mainSequence;

        public void SetValue(int value, bool animate)
        {
            _mainSequence?.Kill(false);

            if (animate)
            {
                transform.localScale = Vector3.one;

                _mainSequence = DOTween.Sequence();
                _mainSequence.Join(transform.DOPunchScale(new Vector3(0.15f, -0.15f, 0), 0.2f));

                if (_useCounting)
                {
                    _mainSequence.Join(DOTween.To(() => _currentValue, x => {
                        _currentValue = x;
                        _text.text = x.ToString();
                    }, value, _duration).SetEase(Ease.OutQuad));
                }
                else
                {
                    _currentValue = value;
                    _text.text = value.ToString();
                }
            }
            else
            {
                transform.localScale = Vector3.one;
                _currentValue = value;
                _text.text = value.ToString();
            }
        }
    }
}
