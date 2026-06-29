using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SlimeRpgEvolution2D.UI.World
{
    public class RequirementItemUI : MonoBehaviour
    {
        [Header("UI элементы строчки")]
        [SerializeField] private Image _iconImage;           // Иконка (монета, меч или предмет)
        [SerializeField] private TextMeshProUGUI _nameText;  // Название предмета/босса
        [SerializeField] private TextMeshProUGUI _descText;  // Текст количества/уровня
        [SerializeField] private Image _statusCheckIcon;    // Наш квадрат статуса справа

        [Header("Спрайты статуса (Ваша пиксельная графика)")]
        [Tooltip("Перетащите сюда нарезанный спрайт зеленой галочки")]
        [SerializeField] private Sprite _completedSprite;
        [Tooltip("Перетащите сюда нарезанный спрайт красного крестика")]
        [SerializeField] private Sprite _lockedSprite;

        /// <summary>
        /// Универсальный метод заполнения строчки требования
        /// </summary>
        public void Setup(Sprite icon, string reqName, string description, bool isMet)
        {
            // 1. Настройка левой иконки
            if (_iconImage != null)
            {
                if (icon != null)
                {
                    _iconImage.gameObject.SetActive(true);
                    _iconImage.sprite = icon;
                }
                else
                {
                    // Если спрайта нет (например, для этапов), аккуратно скрываем объект, чтобы не было пустого места
                    _iconImage.gameObject.SetActive(false);
                }
            }

            // 2. Заполняем имя требования (заменит шаблон)
            if (_nameText != null)
            {
                _nameText.text = reqName;
                _nameText.color = isMet ? Color.green : Color.red;
            }

            // 3. Заполняем числовые данные (уровень или количество)
            if (_descText != null)
            {
                _descText.text = description;
                _descText.color = isMet ? Color.green : Color.red;
            }

            // 4. ПЕРЕКЛЮЧЕНИЕ ГАЛОЧКИ И КРЕСТИКА
            if (_statusCheckIcon != null)
            {
                if (_completedSprite != null && _lockedSprite != null)
                {
                    // Подставляем ваш спрайт в зависимости от выполнения квеста
                    _statusCheckIcon.sprite = isMet ? _completedSprite : _lockedSprite;

                    // Сбрасываем цвет в белый, чтобы Unity не накладывала цветной фильтр поверх вашего пиксель-арта
                    _statusCheckIcon.color = Color.white;
                }
                else
                {
                    // Заглушка: если в инспекторе префаба забыли перетащить картинки
                    _statusCheckIcon.color = isMet ? Color.green : Color.red;
                }
            }
        }
    }
}
