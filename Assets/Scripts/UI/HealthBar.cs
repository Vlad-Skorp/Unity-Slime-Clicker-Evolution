using SlimeRpgEvolution2D.Logic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Image _healthFill;
    [SerializeField] private float _smoothSpeed = 8f;

    private IDamageable _damageable;
    private float _targetFullness = 1f;
    private Coroutine _drainCoroutine;


    private void Awake() => _damageable = GetComponentInParent<IDamageable>();
    private void OnEnable()
    {
        if (_damageable != null)
        {
            _damageable.OnHealthChanged += OnHealthChanged;

            _healthFill.fillAmount = _targetFullness;
        }
    }

    private void OnDisable()
    {
        if (_damageable != null)
        {
            _damageable.OnHealthChanged -= OnHealthChanged;
        }

        if (_drainCoroutine != null)
        {
            StopCoroutine(_drainCoroutine);
            _drainCoroutine = null;
        }
    }

    private void OnHealthChanged(float percent) 
    {
        _targetFullness = percent;

        if(_drainCoroutine == null)
        {
            _drainCoroutine = StartCoroutine(SmoothUpdateBar());
        }
    }

    private IEnumerator SmoothUpdateBar()
    {
        while (Mathf.Abs(_healthFill.fillAmount - _targetFullness) > 0.001f)
        {
            _healthFill.fillAmount = Mathf.Lerp(
                _healthFill.fillAmount,
                _targetFullness,
                Time.deltaTime * _smoothSpeed
            );

            yield return null;
        }

        _healthFill.fillAmount = _targetFullness;
        _drainCoroutine = null;
    }
}
