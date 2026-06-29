using SlimeRpgEvolution2D.Data;
using UnityEngine;

namespace SlimeRpgEvolution2D.UI.HUD
{
    public abstract class BasePresenter : MonoBehaviour
    {
        [Header("Base UI Settings")]
        [SerializeField] protected CounterView _view;

        [SerializeField] private bool _animateOnEnable = false;

        protected BigNumber _lastValue;

        protected virtual void OnEnable()
        {
            if (_view == null)
            {
                Debug.LogError($"{name}: CounterView is missing!", this);
                return;
            }

            if (Player.Local != null)
            {
                Subscribe();


                BigNumber initialValueText = GetCurrentValue();



                _view.SetValue(initialValueText, _animateOnEnable);
            }
        }


        protected virtual void OnDisable()
        {
            if (Player.Local != null) Unsubscribe();
        }

        protected abstract void Subscribe();
        protected abstract void Unsubscribe();
        protected abstract BigNumber GetCurrentValue();

        protected virtual void HandleUpdate(BigNumber amount)
        {
            if (_view == null) return;

            _view.SetValue(amount, true);

            // Поразрядное сравнение старого и нового BigNumber
            int comparison = CompareBigNumbers(amount, _lastValue);

            if (comparison > 0) OnValueIncreased(amount);
            else if (comparison < 0) OnValueDecreased(amount);

            _lastValue = amount;
        }

        protected virtual void OnValueIncreased(BigNumber newValue) { }
        protected virtual void OnValueDecreased(BigNumber newValue) { }

        private int CompareBigNumbers(BigNumber a, BigNumber b)
        {
            for (int i = 3; i >= 0; i--)
            {
                int segA = a.GetSegment(i);
                int segB = b.GetSegment(i);
                if (segA > segB) return 1;
                if (segA < segB) return -1;
            }
            return 0;
        }
    }
}
