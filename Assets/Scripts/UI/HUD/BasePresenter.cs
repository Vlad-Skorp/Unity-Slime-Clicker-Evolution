using UnityEngine;

namespace SlimeRpgEvolution2D.UI.HUD
{
    public abstract class BasePresenter : MonoBehaviour
    {
        [Header("Base UI Settings")]
        [SerializeField] protected CounterView _view;

        [SerializeField] private bool _animateOnEnable = false;

        protected int _lastValue;

        protected virtual void OnEnable()
        {
            if (_view == null)
            {
                Debug.LogError($"{name}: CounterView is missing!", this);
                return;
            }

            if (Player.Local != null)
            {
                int initialValue = GetCurrentValue();

                _lastValue = initialValue;

                Subscribe();

                _view.SetValue(GetCurrentValue(), _animateOnEnable);
            }
        }
        

        protected virtual void OnDisable()
        {
            if (Player.Local != null) Unsubscribe();
        }

        protected abstract void Subscribe();
        protected abstract void Unsubscribe();
        protected abstract int GetCurrentValue();

        protected virtual void HandleUpdate(int amount)
        {
            int delta = amount - _lastValue;

            Debug.Log($"{name} Update: New={amount}, Old={_lastValue}, Delta={delta}");
            _lastValue = amount;

            _view.SetValue(amount, true);

            if (delta > 0) OnValueIncreased(delta);
            else if (delta < 0) OnValueDecreased(delta);
        }

        protected virtual void OnValueIncreased(int delta) { }
        protected virtual void OnValueDecreased(int delta) { }
    }
}
