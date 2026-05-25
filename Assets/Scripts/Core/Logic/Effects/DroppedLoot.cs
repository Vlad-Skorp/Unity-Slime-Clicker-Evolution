using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Обязательно подключаем DOTween!

namespace SlimeRpgEvolution2D.Logic.Effects
{
    public class DroppedLoot : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image _uiImage;
        public void Initialize(Sprite lootSprite, Vector3 targetHUDPosition, bool isCoin)
        {
            _uiImage = GetComponent<Image>();

            if (_uiImage != null)
            {
                _uiImage.sprite = lootSprite;
            }


            // 1. НАЧАЛЬНЫЙ ВЫЛЕТ ЧЕРЕЗ DOTWEEN (Прыжок монетки)
            // Так как мы работаем в координатах интерфейса (UI), значения прыжка должны быть больше
            Vector3 jumpTarget = transform.position + new Vector3(
                Random.Range(-150f, 150f),
                Random.Range(-50f, 100f),
                0
            );

            Sequence lootSequence = DOTween.Sequence();

            // Эффект прыжка по дуге (высота дуги 150)
            lootSequence.Append(transform.DOJump(jumpTarget, 150f, 1, 0.5f).SetEase(Ease.OutQuad));
            // Закручиваем монетку в воздухе для красоты
            transform.DORotate(new Vector3(0, 0, Random.Range(-180f, 180f)), 0.5f);

            // Маленькая пауза на «земле» перед полетом к счетчику
            lootSequence.AppendInterval(0.2f);

            // 2. ПОЛЕТ К ЦЕЛИ
            if (isCoin && targetHUDPosition != Vector3.zero)
            {
                // Если это монетка — притягиваем её к счетчику, плавно уменьшая в размерах
                lootSequence.Append(transform.DOMove(targetHUDPosition, 0.6f).SetEase(Ease.InBack));
                transform.DOScale(Vector3.one * 0.4f, 0.6f).SetDelay(0.7f);
            }
            else
            {
                // Если это ядро дропа — оно плавно взлетает вверх и растворяется
                lootSequence.Append(transform.DOMoveY(transform.position.y + 100f, 0.8f).SetEase(Ease.OutCubic));
                if (_uiImage != null)
                {
                    lootSequence.Join(_uiImage.DOFade(0f, 0.8f).SetDelay(0.7f));
                }
            }

            // Полное удаление объекта, когда вся анимация завершилась
            lootSequence.OnComplete(() =>
            {
                Destroy(gameObject);
            });
        }
    }
}
