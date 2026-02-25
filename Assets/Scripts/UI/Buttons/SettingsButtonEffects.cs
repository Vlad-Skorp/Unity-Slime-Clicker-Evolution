using UnityEngine;
using UnityEngine.EventSystems; // Нужно для отслеживания мышки
using DG.Tweening;


namespace SlimeRpgEvolution2D.UI.Buttons
{
    public class SettingsButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Settings")]
        [SerializeField] private float _rotateDuration = 0.5f;
        [SerializeField] private float _punchStrength = 0.2f;

        private Tween _rotateTween;

        // Срабатывает, когда мышка НАВЕДЕНА на кнопку
        public void OnPointerEnter(PointerEventData eventData)
        {
            // ОСТАНАВЛИВАЕМ ВСЁ ПЕРЕД СТАРТОМ (чтобы не дёргалась)
            transform.DOKill();

            // Запускаем плавное бесконечное вращение по оси Z
            _rotateTween = transform.DOLocalRotate(new Vector3(0, 0, -360), _rotateDuration, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear);
        }

        // Срабатывает, когда мышка УШЛА с кнопки
        public void OnPointerExit(PointerEventData eventData)
        {
            _rotateTween?.Kill();
            // Плавно возвращаем в 0, а не бросаем на полпути
            transform.DOLocalRotate(Vector3.zero, 0.2f);
        }

        // Срабатывает при КЛИКЕ
        public void OnPointerClick(PointerEventData eventData)
        {
            // Небольшой "пульс" при нажатии
            transform.DOPunchScale(Vector3.one * _punchStrength, 0.2f);
        }
    }
}