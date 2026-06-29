using SlimeRpgEvolution2D.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIEnemyInfo : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private Image _healthFill;
    [SerializeField] private float _smoothSpeed = 8f;

    [Header("Health Text Settings")]
    [Tooltip("Текст для отображения ХП поверх полоски (например, 75K / 100K)")]
    [SerializeField] private TextMeshProUGUI _healthText;


    [Header("Text Elements (Раздельные)")]
    [Tooltip("Текст для ИМЕНИ слайма")]
    [SerializeField] private TextMeshProUGUI _enemyNameText;

    [Tooltip("Текст для УРОВНЯ слайма")]
    [SerializeField] private TextMeshProUGUI _enemyLevelText;

    [Header("Dynamic Positioning Settings")]
    [Tooltip("Отступ UI вверх от самой верхней точки короны/спрайта")]
    [SerializeField] private float _yOffset = 0.5f;

    private SpriteRenderer _areaLimit;
    private IDamageable _damageable;
    private float _targetFullness = 1f;
    private Coroutine _drainCoroutine;
    private RectTransform _rectTransform;

    // Ссылка на компонент прозрачности
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _damageable = GetComponentInParent<IDamageable>();
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        Enemy.OnEnemySpawnedUI += HandleEnemySpawned;

        if (_damageable != null)
        {
            _damageable.OnHealthChanged += OnHealthChanged;
            _healthFill.fillAmount = _targetFullness;
        }

        // ЗАЩИТА: В момент включения префаба делаем UI полностью невидимым,
        // чтобы он не прыгал по экрану во время спавна врага
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }
    }

    private void OnDisable()
    {
        Enemy.OnEnemySpawnedUI -= HandleEnemySpawned;
        if (_damageable != null) _damageable.OnHealthChanged -= OnHealthChanged;

        if (_drainCoroutine != null)
        {
            StopCoroutine(_drainCoroutine);
            _drainCoroutine = null;
        }
    }

    public void SetLimit(SpriteRenderer limitRenderer)
    {
        _areaLimit = limitRenderer;
    }

    private void HandleEnemySpawned(string enemyName, int enemyLevel)
    {
        if (_enemyNameText != null) _enemyNameText.text = enemyName;
        if (_enemyLevelText != null) _enemyLevelText.text = $"Lvl {enemyLevel}";
    }

    public void UpdateUIPosition()
    {
        if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();

        SpriteRenderer spriteRenderer = GetComponentInParent<Enemy>()?.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null && transform.parent != null)
        {
            spriteRenderer = transform.parent.GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null && _rectTransform != null && spriteRenderer.sprite != null)
        {
            float globalTopY = spriteRenderer.bounds.max.y;
            float targetWorldY = globalTopY + _yOffset;

            if (_areaLimit != null)
            {
                float minY = _areaLimit.bounds.min.y;
                float maxY = _areaLimit.bounds.max.y;

                targetWorldY = Mathf.Clamp(targetWorldY, minY, maxY);
            }

            if (transform.parent != null)
            {
                Vector3 worldPos = new Vector3(spriteRenderer.bounds.center.x, targetWorldY, spriteRenderer.transform.position.z);
                Vector3 localPos = transform.parent.InverseTransformPoint(worldPos);

                localPos.z = _rectTransform.localPosition.z;
                _rectTransform.localPosition = localPos;
            }

            // ПОБЕДА НАД БАГОМ: Позиция рассчитана и зажата в лимиты — теперь безопасно включаем видимость UI!
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }
    }

    private void OnHealthChanged(float percent)
    {
        _targetFullness = percent;

        if (_drainCoroutine == null)
        {
            _drainCoroutine = StartCoroutine(SmoothUpdateBar());
        }

        // ОБНОВЛЕНИЕ ТЕКСТА: Каждый раз при получении урона перерисовываем цифры ХП
        UpdateHealthText();
    }

    private IEnumerator SmoothUpdateBar()
    {
        while (Mathf.Abs(_healthFill.fillAmount - _targetFullness) > 0.001f)
        {
            _healthFill.fillAmount = Mathf.Lerp(_healthFill.fillAmount, _targetFullness, Time.deltaTime * _smoothSpeed);
            yield return null;
        }

        _healthFill.fillAmount = _targetFullness;
        _drainCoroutine = null;
    }


    private void UpdateHealthText()
    {
        if (_healthText == null || _damageable == null) return;

        // Берем точные BigNumber напрямую из интерфейса врага
        BigNumber currentHp = _damageable.CurrentHealth;
        BigNumber maxHp = _damageable.MaxHealth;

        // 1. ИСПРАВЛЕНО: Если моб мертв или его ХП ушло в минус/ноль, принудительно пишем "0"
        // Наш оператор <= 0, который мы написали в BigNumber.cs, идеально перехватит этот момент!
        if (_damageable.IsDead || currentHp <= 0)
        {
            _healthText.text = $"0 / {NumberFormatter.Format(maxHp)} hp";
            return;
        }

        // 2. Если ХП положительное — форматируем как обычно
        string formattedCurrent = NumberFormatter.Format(currentHp);
        string formattedMax = NumberFormatter.Format(maxHp);

        _healthText.text = $"{formattedCurrent} / {formattedMax} hp";
    }


    public void Info() { }
}
