using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SlimeRpgEvolution2D.Data;
using SlimeRpgEvolution2D.UI.Core;



namespace SlimeRpgEvolution2D.UI.Popups
{
    public enum ShopTabType
    {
        Weapons,
        Inventory
    }

    public class ShopManager : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private ShopItemPresenter _itemPrefab;
        [SerializeField] private Transform _container;
        [SerializeField] private List<ShopTabButton> _tabButtons;

        private readonly List<ShopItemPresenter> _activeItems = new List<ShopItemPresenter>();
        private ShopTabType _currentTab = ShopTabType.Weapons; //Вкладка по умолчанию

        [Header("Animations")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _windowContent;
        [SerializeField] private float _animationDuration = 0.2f;

        private Coroutine _animationRoutine;

        private void OnEnable()
        {
            if (DataManager.Instance == null)
            {
                Debug.LogError("[ShopManager] DataManager.Instance не найден! Закрываю магазин.");
                gameObject.SetActive(false); // Выключаем панель, чтобы не висел пустой интерфейс
                return;
            }

            DataManager.OnCoinsChanged += RefreshAllItems;

            SelectTab(ShopTabType.Weapons);
        }

        public void SelectTab(ShopTabType tabType)
        {
            _currentTab = tabType;

            foreach(var button in _tabButtons)
            {
                if (button != null) button.UpdateVisualState(_currentTab);
            }

            InitializeShop();
        }

        private void InitializeShop()
        {
            if (DataManager.Instance == null || GameDB.Shop == null) return;

            foreach (var item in _activeItems)
            {
                if (item != null)
                {
                    item.UpgradeRequested -= HandleUpgradeRequest; 
                    Destroy(item.gameObject);
                }
            }
            _activeItems.Clear();

            List<ShopProductConfig> productsToShow = GameDB.Shop.GetProductsForTab(_currentTab);

            foreach (var product in productsToShow)
            {
                if (product == null) continue;
                CreateShopItem(product);
            }
        }

        private void CreateShopItem(ShopProductConfig product)
        {
            var itemUI = Instantiate(_itemPrefab, _container);

            // Проверяем, хватает ли денег на текущий (динамический) ценник товара
            bool canAfford = DataManager.Instance.SaveData.Coins >= product.GetCurrentPrice();

            // Передаем универсальный продукт в UI префаба
            itemUI.Initialize(product, canAfford);
            itemUI.UpgradeRequested += HandleUpgradeRequest;

            _activeItems.Add(itemUI);
        }


        private void HandleUpgradeRequest(ShopProductConfig product)
        {
            if (product == null) return;

            // Если предмет уже куплен (актуально для рюкзаков), ничего не делаем
            if (product.IsPurchasedOrMax()) return;

            int price = product.GetCurrentPrice();

            // Проверяем баланс и списываем монеты
            if (DataManager.Instance.TrySpendCoins(price))
            {
                // Продукт САМ знает, что делать: поднять уровень меча, разблокировать рюкзак или выдать сферу
                product.Buy();

                // Обновляем всю витрину (цены, доступность кнопок)
                RefreshAllItems(DataManager.Instance.SaveData.Coins);

                if (Player.Local != null) Player.Local.RefreshUI();

                Debug.Log($"[Shop] Успешно куплен товар: {product.DisplayName}");
            }
        }

        private void RefreshAllItems(int currentCoin)
        {
            foreach (var item in _activeItems)
            {
                if (item == null || item.Config == null) continue;

                // Проверяем баланс для обновленной цены товара
                bool canAfford = currentCoin >= item.Config.GetCurrentPrice();

                // Даем UI команду перерисовать кнопку и цену
                item.UpdateUI(canAfford);
            }
        }


        private void OnDisable()
        {
            DataManager.OnCoinsChanged -= RefreshAllItems;

            foreach (var item in _activeItems)
            {
                if (item != null) item.UpgradeRequested -= HandleUpgradeRequest;
            }
        }

        private bool _isOpen = false;

        public void ToggleShop()
        {
            _isOpen = !_isOpen;

            if (_isOpen)
            {
                gameObject.SetActive(true);
                if (_animationRoutine != null) StopCoroutine(_animationRoutine);
                _animationRoutine = StartCoroutine(AnimateShop(0, 1, 0.8f, 1f));
            }
            else
            {
                if (_animationRoutine != null) StopCoroutine(_animationRoutine);
                // Запускаем обратную анимацию и передаем callback на выключение
                _animationRoutine = StartCoroutine(AnimateShop(1, 0, 1f, 0.8f, () =>
                {
                    gameObject.SetActive(false);
                }));
            }
        }

        public void CloseShop()
        {
            if (_animationRoutine != null) StopCoroutine(_animationRoutine);
            // Запускаем обратную анимацию и передаем callback на выключение
            _animationRoutine = StartCoroutine(AnimateShop(1, 0, 1f, 0.8f, () => {
                gameObject.SetActive(false);
            }));
        }


        private IEnumerator AnimateShop(float startAlpha, float endAlpha, float startScale, float endScale, System.Action onComplete = null)
        {
            float elapsed = 0;
            while (elapsed < _animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _animationDuration;

                // Плавность через кривую (по желанию можно добавить SmoothStep)
                float curve = Mathf.SmoothStep(0, 1, t);

                _canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curve);
                _windowContent.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, curve);
                yield return null;
            }

            _canvasGroup.alpha = endAlpha;
            _windowContent.localScale = Vector3.one * endScale;

            onComplete?.Invoke();
            _animationRoutine = null;
        }

        public void CloseShops() => UIManager.Instance.CloseAllPopups();
    }
}
