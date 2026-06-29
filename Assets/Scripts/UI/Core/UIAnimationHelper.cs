using System;
using System.Collections;
using UnityEngine;

public static class UIAnimationHelper
{
    // Универсальная корутина для плавного открытия/закрытия любого окна
    public static IEnumerator AnimateWindow(CanvasGroup canvasGroup, Transform content, float startAlpha, float endAlpha, float startScale, float endScale, float duration, Action onComplete = null)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Единая плавность для всей игры
            float curve = Mathf.SmoothStep(0, 1, t);

            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curve);
            if (content != null) content.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, curve);

            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = endAlpha;
        if (content != null) content.localScale = Vector3.one * endScale;

        onComplete?.Invoke();
    }
}

