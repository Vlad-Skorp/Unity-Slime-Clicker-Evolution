using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SlimeRpgEvolution2D.UI.World
{
    public class WorldView : MonoBehaviour
    {
        [Header("Текстовые элементы карточки")]
        [SerializeField] private TMP_Text worldNumberText;
        [SerializeField] private TMP_Text worldNameText;
        [SerializeField] private TMP_Text actionBtnText;

        [Header("Картинка превью")]
        [SerializeField] private Image worldPreviewImage;

        [Header("Кнопки управления")]
        [SerializeField] private Button leftArrowBtn;
        [SerializeField] private Button rightArrowBtn;
        [SerializeField] private Button actionButton;

        // Публичные свойства (Геттеры), чтобы Presenter мог читать кнопки и тексты
        public TMP_Text WorldNumberText => worldNumberText;
        public TMP_Text WorldNameText => worldNameText;
        public TMP_Text ActionBtnText => actionBtnText;
        public Image WorldPreviewImage => worldPreviewImage;
        public Button LeftArrowBtn => leftArrowBtn;
        public Button RightArrowBtn => rightArrowBtn;
        public Button ActionButton => actionButton;

        [Header("Динамический Блок Требований Разблокировки")]
        [Tooltip("Сюда перетащить объект текста, где написано 'ДЛЯ РАЗБЛОКИРОВКИ ТРЕБУЕТСЯ:'")]
        [SerializeField] private TMP_Text _requirementsTitleText;
        public TMP_Text RequirementsTitleText => _requirementsTitleText;

        [Header("Динамический Блок Требований Разблокировки")]
        [Tooltip("Сюда перетащить объект с Vertical Layout Group (родитель для строчек)")]
        [SerializeField] private Transform _requirementsContainer;
        public Transform RequirementsContainer => _requirementsContainer;

        [Tooltip("Сюда перетащить префаб RequirementItemUI")]
        [SerializeField] private RequirementItemUI _requirementPrefab;
        public RequirementItemUI RequirementPrefab => _requirementPrefab;




        // Методы для быстрого включения/выключения окон
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
