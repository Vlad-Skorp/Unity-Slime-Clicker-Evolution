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

            DataManager.OnCoinsChanged += _ => RefreshAllItems();

            bool hasBackpack = DataManager.Instance.SaveData.IsBackpackDropped;
            if (_inventoryTabButtonObject != null)
            {
                _inventoryTabButtonObject.SetActive(hasBackpack);
            }

            SelectTab(_currentTab, forceRefresh: true);

            if (_animationRoutine != null) StopCoroutine(_animationRoutine);

            // Вызываем общий метод анимации из UIAnimationHelper
            _animationRoutine = StartCoroutine(UIAnimationHelper.AnimateWindow(
                _canvasGroup, _windowContent, 0f, 1f, 0.8f, 1f, _animationDuration
            ));

        }

        private void OnDisable()
        {
            DataManager.OnCoinsChanged -= _ => RefreshAllItems();

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
            bool canAfford = DataManager.Instance.CanAfford(product.GetCurrentPrice());


            // Передаем универсальный продукт в UI префаба
            itemUI.Initialize(product, canAfford);
            itemUI.UpgradeRequested += HandleUpgradeRequest;

            _activeItems.Add(itemUI);
        }


        private void HandleUpgradeRequest(ShopProductConfig product)
        {
            if (product == null) return;

            // Если предмет уже куплен (актуально для рюкзаков), ничего не делаем
            if (product.IsPurchasedOrMax())
            {
                RefreshAllItems(); // На всякий случай принудительно делаем кнопку серой
                return;
            }

            BigNumber price = product.GetCurrentPrice();

            // Проверяем баланс и списываем монеты
            if (DataManager.Instance.TrySpendCoins(price))
            {
                int itemsCountBefore = _activeItems.Count;

                // Продукт САМ знает, что делать: поднять уровень меча, разблокировать рюкзак или выдать сферу
                product.Buy();

                int itemsCountAfter = GameDB.Shop.GetProductsForTab(_currentTab).Count;


                if (itemsCountAfter > itemsCountBefore)
                {
                    // Если открылся новый секретный меч — перестраиваем магазин полностью
                    InitializeShop();
                    Debug.Log("[Shop] Открылось новое секретное оружие! Витрина полностью перестроена.");
                }
                else
                {
                    // ИСПРАВЛЕНО: Теперь мы не передаем голые монеты int. 
                    // Мы просто вызываем метод обновления, а он сам спросит у DataManager актуальный баланс!
                    RefreshAllItems();
                }

                if (Player.Local != null) Player.Local.RefreshUI();

                Debug.Log($"[Shop] Успешно куплен товар: {product.DisplayName}");
            }
        }

        // ИСПРАВЛЕНО: Убрали аргумент (int currentCoin), так как он больше не нужен
        private void RefreshAllItems()
        {
            foreach (var item in _activeItems)
            {
                if (item == null || item.Config == null) continue;

                // ИСПРАВЛЕНО: Проверяем баланс для обновленной цены товара через наш всеядный DataManager
                bool canAfford = DataManager.Instance.CanAfford(item.Config.GetCurrentPrice());

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

            // ВЫЗЫВАЕМ ОБЩИЙ ХЕЛПЕР НА ОТКРЫТИЕ:
            if (_animationRoutine != null) StopCoroutine(_animationRoutine);
            _animationRoutine = StartCoroutine(UIAnimationHelper.AnimateWindow(
                _canvasGroup, _windowContent, 0f, 1f, 0.8f, 1f, _animationDuration
            ));
        }

        public void CloseShop()
        {
            if (_animationRoutine != null) StopCoroutine(_animationRoutine);

            // ВЫЗЫВАЕМ ОБЩИЙ ХЕЛПЕР НА ЗАКРЫТИЕ:
            // Передаем все параметры окна и финальное действие в наш универсальный метод
            _animationRoutine = StartCoroutine(UIAnimationHelper.AnimateWindow(
                _canvasGroup, _windowContent, 1f, 0f, 1f, 0.8f, _animationDuration, () =>
                {
                    gameObject.SetActive(false);

                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.NotifyWindowClosed();
                    }
                }
            ));
        }


    }
}
