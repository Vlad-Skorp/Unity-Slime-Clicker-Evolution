using UnityEngine;
using UnityEngine.UI;

namespace SlimeRpgEvolution2D.UI.Popups
{
    [RequireComponent(typeof(Button))]
    public class ShopTabButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShopManager _shopManager;
        [SerializeField] private ShopTabType _targetTab;

        [Header("Visual Settings (Optional")]
        [SerializeField] private Image _buttonImage;
        [SerializeField] private Color _activeColor = Color.white;
        [SerializeField] private Color _inactiveColor = Color.gray;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnTabClicked);
        }

        private void OnTabClicked()
        {
            if (_shopManager != null)
            {
                _shopManager.SelectTab(_targetTab);
            }
        }

        public void UpdateVisualState(ShopTabType activeTab)
        {
            if (_buttonImage == null) return;

            _buttonImage.color = (activeTab == _targetTab) ? _activeColor : _inactiveColor; 
        }
    }
}