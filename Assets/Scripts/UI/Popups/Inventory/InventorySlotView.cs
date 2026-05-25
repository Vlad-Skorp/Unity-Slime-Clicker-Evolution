using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlimeRpgEvolution2D.UI.Popups
{

    public class InventorySlotView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Button _slotButton;

        public void SetData(Sprite iconSprite, string amount, bool showAmount)
        {
            if (iconSprite != null)
            {
                _icon.sprite = iconSprite;
                _icon.enabled = true;

                _amountText.text = amount;
                _amountText.gameObject.SetActive(showAmount);
            }
            else
            {
                _icon.sprite = null;
                _icon.enabled = false;

                _amountText.text = null;
                _amountText.gameObject.SetActive(false);
            }
        }

        public void OnClick(Action action)
        {
            _slotButton.onClick.RemoveAllListeners();
            _slotButton.onClick.AddListener(() => action?.Invoke());
        }
    }
}