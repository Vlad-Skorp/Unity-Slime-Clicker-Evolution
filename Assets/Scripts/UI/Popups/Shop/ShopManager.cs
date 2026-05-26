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
        private static ShopTabType _currentTab = ShopTabType.Weapons; 

        [Header("Animations")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _windowContent;
        [SerializeField] private float _animationDuration = 0.2f;

        private Coroutine _animationRoutine;



        [SerializeField] private GameObject _inventoryTabButtonObject;

        private void OnEnable()
        {
            if (DataManager.Instance == null)
            {
                Debug.LogError("[ShopManager] DataManager.Instance не найден! Закрываю магазин.");
                gameObject.SetActive(false); // Выключаем панель, чтобы не висел пустой интерфейс
                return;
            }

            DataManager.OnCoinsChanged += RefreshAllItems;

            bool hasBackpack = DataManager.Instance.SaveData.IsBackpackDropped;
            if (_inventoryTabButtonObject != null)
            {
                _inventoryTabButtonObject.SetActive(hasBackpack);
            }

            SelectTab(_currentTab, forceRefresh: true);

            if (_animationRoutine != null) StopCoroutine(_animationRoutine);
            _animationRoutine = StartCoroutine(AnimateShop(0f, 1f, 0.8f, 1f));
        }

        private void OnDisable()
        {
            DataManager.OnCoinsChanged -= RefreshAllItems;

            foreach (var item in _activeItems)
            {
                if (item != null) item.UpgradeRequested -= HandleUpgradeRequest;
            }
        }

        public void SelectTab(ShopTabType tabType, bool forceRefresh = false)
        {
            if (_currentTab == tabType && !forceRefresh)
            {
                return;
            }


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

        public void OpenShopOnTab(ShopTabType tabType)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ToggleShop();
            }

            if (_inventoryTabButtonObject != null && tabType == ShopTabType.Inventory)
            {
                _inventoryTabButtonObject.SetActive(true);
            }

            SelectTab(tabType);
        }

        public void CloseShop()
        {
            if (_animationRoutine != null) StopCoroutine(_animationRoutine);

            // Сначала проигрываем анимацию закрытия, а затем уведомляем UIManager
            _animationRoutine = StartCoroutine(AnimateShop(1f, 0f, 1f, 0.8f, () =>
            {
                // ИЗМЕНЕНО: Сначала полностью выключаем объект магазина
                gameObject.SetActive(false);

                // ИЗМЕНЕНО: Говорим менеджеру, что окно закрылось, чтобы он проверил слой попапов
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.NotifyWindowClosed();
                }
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
