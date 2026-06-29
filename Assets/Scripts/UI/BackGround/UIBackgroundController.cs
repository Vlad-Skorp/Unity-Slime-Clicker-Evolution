using SlimeRpgEvolution2D.Data;
using UnityEngine;

public class UIBackgroundController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Перетащи сюда компонент SpriteRenderer с этого же объекта")]
    [SerializeField] private SpriteRenderer backgroundRenderer;

    private void Awake()
    {
        // Если забыл перетащить в инспекторе, скрипт попытается найти его сам
        if (backgroundRenderer == null)
        {
            backgroundRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void OnEnable()
    {
        // Подписываемся на событие смены мира
        GameLevelManager.OnWorldChanged += HandleWorldChanged;
    }

    private void OnDisable()
    {
        // Обязательно отписываемся, чтобы не было утечек памяти
        GameLevelManager.OnWorldChanged -= HandleWorldChanged;
    }

    private void HandleWorldChanged(LevelSettings newLevelSettings)
    {
        if (newLevelSettings == null || backgroundRenderer == null) return;

        // Берем спрайт из ScriptableObject, который ты настроил в инспекторе
        if (newLevelSettings.StageBackground != null)
        {
            backgroundRenderer.sprite = newLevelSettings.StageBackground;
            Debug.Log($"<color=cyan>[UIBackground]</color> Фон успешно изменен на: {newLevelSettings.StageBackground.name}");
        }
        else
        {
            Debug.LogWarning($"[UIBackground] В настройках мира {newLevelSettings.name} не задан спрайт заднего фона!");
        }
    }
}
