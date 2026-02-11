using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlimeRpgEvolution2D.UI.Popups
{
    public class ShopItemView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private CanvasGroup _group;


        public void SetData(Sprite icon, string name, string price, Color priceColor)
        {
            _icon.sprite = icon;
            _nameText.text = name;
            _priceText.text = price;
            _priceText.color = priceColor;
        }

        public void SetInteraction(bool interactable, float alpha = 1f)
        {
            _buyButton.interactable = interactable;
            _group.alpha = alpha;
        }

        public void OnClick(Action action)
        {
            _buyButton.onClick.RemoveAllListeners();
            _buyButton.onClick.AddListener(() => action?.Invoke());

        }
    }
}
