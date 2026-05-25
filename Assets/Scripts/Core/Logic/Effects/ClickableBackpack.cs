using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SlimeRpgEvolution2D.Logic.Effects
{
    public class ClickableBackpack : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Button _button;

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (_button != null)
            {
                _button.onClick.AddListener(HandleBackpackClick);
            }
        }

        public void Initialize(Vector3 startScreenPos)
        {
            transform.position = startScreenPos;
            transform.localScale = Vector3.zero;

            Vector3 jumpTarget = startScreenPos + new Vector3(
                Random.Range(-100f, 100f),
                Random.Range(50f, 150f),
                0
            );

            Sequence spawnSequence = DOTween.Sequence();
            spawnSequence.Append(transform.DOJump(jumpTarget, 120f, 1, 0.5f).SetEase(Ease.OutQuad));
            spawnSequence.Join(transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));

            spawnSequence.OnComplete(() =>
            {
                transform.DOMoveY(transform.position.y + 15f, 0.8f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutQuad);
            });
        }

        private void HandleBackpackClick()
        {
            transform.DOKill();
            if (_button != null) _button.interactable = false;

            transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                // ИСПРАВЛЕНО: Находим менеджер магазина, даже если его игровой объект (ShopPanel) полностью выключен!
                var shop = Object.FindAnyObjectByType<SlimeRpgEvolution2D.UI.Popups.ShopManager>(FindObjectsInactive.Include);

                if (shop != null)
                {
                    // Активируем сначала сам объект панели, чтобы методы анимации сработали корректно
                    shop.gameObject.SetActive(true);

                    // Вызываем ваш метод открытия/анимации магазина
                    shop.ToggleShop();

                    Debug.Log("[Рюкзак] Успешно найден скрытый ShopManager, открываю магазин.");
                }
                else
                {
                    Debug.LogError("[Рюкзак] Ошибка! Не удалось найти ShopManager на сцене даже среди выключенных объектов.");
                }

                Destroy(gameObject);
            });
        }
    }
}
