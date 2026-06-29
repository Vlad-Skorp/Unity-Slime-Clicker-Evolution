using SlimeRpgEvolution2D.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static StagePool;

namespace SlimeRpgEvolution2D.UI.World
{
    public class WorldPresenter : MonoBehaviour
    {
        [Header("MVP Ссылки")]
        [SerializeField] private WorldView view;
        [SerializeField] private WorldManager manager;

        // Список для хранения динамически созданных требований текущей локации
        private readonly List<RequirementItemUI> _activeRequirements = new List<RequirementItemUI>();


        private void Start()
        {
            if (view == null || manager == null)
            {
                Debug.LogError("[WorldPresenter] Не все ссылки MVP назначены в инспекторе!");
                return;
            }

            // Подписываемся на клики кнопок из нашей View
            view.LeftArrowBtn.onClick.AddListener(OnLeftArrowClick);
            view.RightArrowBtn.onClick.AddListener(OnRightArrowClick);
            view.ActionButton.onClick.AddListener(OnActionBtnClick);
        }

        private void OnEnable()
        {
            // Когда панель открывается, синхронизируем индекс просмотра с текущим миром игры
            if (GameLevelManager.Instance != null && manager != null)
            {
                manager.ViewingWorldIndex = GameLevelManager.Instance.CurrentWorldNumber;
            }
            StartCoroutine(InitUIOnNextFrame());
        }

        private IEnumerator InitUIOnNextFrame()
        {
            // Ждем окончания текущего кадра, пока Unity полностью построит все Canvas компоненты
            yield return new WaitForEndOfFrame();

            // Теперь безопасно обновляем UI — всё отрисуется и скроется без багов с первого раза!
            UpdateUI();
        }

        private void OnLeftArrowClick()
        {
            if (manager.CanMoveBackward())
            {
                manager.ViewingWorldIndex--;
                UpdateUI();
            }
        }

        private void OnRightArrowClick()
        {
            if (manager.CanMoveForward())
            {
                manager.ViewingWorldIndex++;
                UpdateUI();
            }
        }

        private void OnActionBtnClick()
        {
            if (GameLevelManager.Instance == null) return;

            LevelSettings settings = manager.GetViewedWorldSettings();
            if (settings == null) return;

            int activeWorld = GameLevelManager.Instance.CurrentWorldNumber; // Тот мир, где идет бой прямо сейчас

            // Честно проверяем через ваше сохранение (как в UpdateUI), куплен ли этот конкретный мир
            bool isWorldPurchased = DataManager.Instance.SaveData.WorldsProgress.Any(w => w.worldID == settings.ID);

            if (isWorldPurchased)
            {
                // --- ЛОГИКА ВХОДА ---
                // ЗАЩИТА: Если игрок рассматривает мир, в котором ОН УЖЕ НАХОДИТСЯ — ничего не делаем
                if (manager.ViewingWorldIndex == activeWorld)
                {
                    Debug.Log("[WorldPresenter] Вы уже находитесь в этом мире!");
                    return;
                }

                // ВХОД: Мир открыт и он не текущий — сохраняем его ID и загружаем через ваш GameLevelManager
                DataManager.Instance.SaveActiveWorldID(settings.ID);
                GameLevelManager.Instance.LoadWorld(manager.ViewingWorldIndex);

                DataManager.Instance.SaveGame();

                // Закрываем панель, как у вас и было
                view.Hide();
            }
            else
            {
                // --- ЛОГИКА ПОКУПКИ И СПИСАНИЯ ---
                // Мир еще не куплен, запускаем метод проверки и списания монет/предметов
                HandleUnlockLocationRequest(settings);
            }
        }

        private void HandleUnlockLocationRequest(LevelSettings settings)
        {
            // 1. Проверяем, пройдены ли все условия квестов (мечи, этапы, предметы)
            bool isQuestsDone = manager.IsWorldQuestsRequirementsMet(settings);

            // 2. ИСПРАВЛЕНО: Теперь передаем в CanAfford структуру BigCoins.
            // Честно проверяем баланс по всем 4 ячейкам массива через DataManager
            bool hasCoins = DataManager.Instance.CanAfford(settings.UnlockRequirement.unlockCost);

            if (isQuestsDone && hasCoins)
            {
                UnlockRequirement req = settings.UnlockRequirement;

                // ИСПРАВЛЕНО: Списываем золото, передавая единую структуру BigCoins целиком.
                // Если мир бесплатный (все 4 ячейки равны 0), метод молча пропустит операцию и вернет true.
                DataManager.Instance.TrySpendCoins(req.unlockCost);

                // Списываем все квестовые предметы через ваш метод TrySpendItem
                if (req.useItemRequirement && req.itemRequirements != null)
                {
                    foreach (var itemReq in req.itemRequirements)
                    {
                        if (itemReq.requiredItem == null) continue;

                        // Тратим предметы по их уникальному ID
                        DataManager.Instance.TrySpendItem(itemReq.requiredItem.ID, itemReq.requiredItemAmount);
                    }
                }

                // Официально покупаем и заносим новый мир в JSON.
                // Из метода PurchaseNewWorld внутри DataManager теперь тоже можно 
                // удалить скрытый SaveGame(), так как TrySpendCoins уже сохранил всё на диск строчкой выше!
                DataManager.Instance.PurchaseNewWorld(settings.ID);

                // Полностью перерисовываем интерфейс карточки (кнопка станет "Войти", требования скроются)
                UpdateUI();

                // Обновляем золото на главном экране игры
                if (Player.Local != null) Player.Local.RefreshUI();
            }
        }





        private void UpdateUI()
        {
            LevelSettings settings = manager.GetViewedWorldSettings();
            if (settings == null) return;

            if (view.WorldNumberText != null)
                view.WorldNumberText.text = $"Мир {manager.ViewingWorldIndex}";

            if (view.WorldNameText != null)
                view.WorldNameText.text = settings.WorldName;

            if (view.WorldPreviewImage != null)
                view.WorldPreviewImage.sprite = settings.StageBackground;

            view.LeftArrowBtn.gameObject.SetActive(manager.CanMoveBackward());
            view.RightArrowBtn.gameObject.SetActive(manager.CanMoveForward());

            if (DataManager.Instance == null || DataManager.Instance.SaveData == null) return;

            string activeWorldID = DataManager.Instance.GetLastActiveWorldID();

            // Честно проверяем через LINQ, существует ли физически запись об этом мире в JSON-файле
            bool isWorldPurchased = DataManager.Instance.SaveData.WorldsProgress.Any(w => w.worldID == settings.ID);

            // --- ЛОГИКА ОТРЕСОВКИ ГЛАВНОЙ КНОПКИ ---

            if (settings.ID == activeWorldID)
            {
                if (view.ActionBtnText != null) view.ActionBtnText.text = "Вы здесь";
                view.ActionButton.interactable = false;
            }
            else if (isWorldPurchased)
            {
                if (view.ActionBtnText != null) view.ActionBtnText.text = "Войти";
                view.ActionButton.interactable = true;
            }
            else
            {
                // 1. Проверяем, пройдены ли все квесты (этапы, мечи, предметы)
                bool isQuestsDone = manager.IsWorldQuestsRequirementsMet(settings);

                if (isQuestsDone)
                {
                    // 2. ИСПРАВЛЕНО: Получаем актуальную структуру Price (вместо int)
                    BigNumber price = settings.UnlockRequirement.unlockCost;

                    // 3. ИСПРАВЛЕНО: Честно проверяем баланс по всем 4 ячейкам массива через DataManager
                    bool canAfford = DataManager.Instance.CanAfford(price);

                    if (view.ActionBtnText != null)
                    {
                        // ИСПРАВЛЕНО: Пропускаем цену через наш NumberFormatter.
                        // Текст на кнопке теперь красиво сократится (например: "15B" или "1T")
                        string formattedPrice = NumberFormatter.Format(price);

                        // Используем логику цветов как в вашем магазине (красный, если дорого)
                        string colorHex = canAfford ? "#ffffff" : "#ff4d4d";
                        view.ActionBtnText.text = $"<color={colorHex}>{formattedPrice} <sprite name=\"Coin_1\"></color>";
                    }

                    // Кнопка нажимается только если на неё хватает золота
                    view.ActionButton.interactable = canAfford;
                }
                else
                {
                    // Если хоть один квест (меч/босс) не выполнен — кнопка жестко пишет "Заперто"
                    if (view.ActionBtnText != null) view.ActionBtnText.text = "Заперто";
                    view.ActionButton.interactable = false;
                }
            }

            UpdateUnlockRequirementsUI(settings, DataManager.Instance.SaveData);
        }



        private void UpdateUnlockRequirementsUI(LevelSettings settings, GameSaveData saveData)
        {
            if (view.RequirementsContainer == null || view.RequirementPrefab == null || view.RequirementsTitleText == null)
            {
                Debug.LogError("В WorldView не назначены RequirementsContainer, RequirementPrefab или RequirementsTitleText!");
                return;
            }

            // 1. ОЧИСТКА (Аналог логики из InitializeShop в магазине)
            foreach (var item in _activeRequirements)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _activeRequirements.Clear(); 

            // Если этот мир уже куплен игроком, то требования прячем и выходим
            bool isWorldPurchased = saveData.WorldsProgress.Any(w => w.worldID == settings.ID);
            if (isWorldPurchased)
            {
                view.RequirementsContainer.gameObject.SetActive(false);
                return;
            }

            // Включаем контейнер для отрисовки активных требований
            view.RequirementsContainer.gameObject.SetActive(true);
            UnlockRequirement req = settings.UnlockRequirement;


            if (view.RequirementsTitleText != null)
            {
                // Проверяем, выполнены ли абсолютно ВСЕ квесты этой локации (мечи, этапы, предметы)
                bool areAllQuestsDone = manager.IsWorldQuestsRequirementsMet(settings);

                if (areAllQuestsDone)
                {
                    // Зеленый текст + тег вашей нарисованной галочки
                    view.RequirementsTitleText.text = "ДЛЯ РАЗБЛОКИРОВКИ ТРЕБУЕТСЯ: <sprite name=Check_1>";
                }
                else
                {
                    // Красный текст + тег вашего нарисованного крестика
                    view.RequirementsTitleText.text = "ДЛЯ РАЗБЛОКИРОВКИ ТРЕБУЕТСЯ: <sprite name=Cross_1>";
                }
            }

            // 2. ОТРИСОВКА ЭТАПОВ ЛОКАЦИЙ (Формат: Имя локации / Этап + Волна + Картинка)
            if (req.useStageRequirement && req.stageRequirements != null)
            {
                foreach (var stageReq in req.stageRequirements)
                {
                    if (stageReq.targetWorld == null) continue;

                    var targetWorldState = DataManager.Instance.GetWorldState(stageReq.targetWorld.ID);

                    // Проверка прогресса: учитываем этап и волну (кол-во убитых мобов)
                    bool stageMet = targetWorldState.maxReachedStage > stageReq.requiredStageNumber ||
                                    (targetWorldState.maxReachedStage == stageReq.requiredStageNumber && targetWorldState.killedEnemies >= stageReq.requiredWaveNumber);

                    // ФОРМИРУЕМ НАИМЕНОВАНИЕ СТРОГО ПО ВАШЕМУ ФОРМАТУ:
                    string reqName = stageReq.targetWorld.WorldName; // Сверху: Название локации (например, "Лес")
                    string reqProgress = $"{stageReq.requiredStageNumber} этап {stageReq.requiredWaveNumber} волна"; // Снизу: "2 этап 5 волна"

                    // БЕРЕМ КАРТИНКУ ЛОКАЦИИ ИЗ КОНФИГА
                    // Временно используем stageBackground, пока вы не создали поле для мини-иконки
                    Sprite locationIcon = stageReq.targetWorld.WorldIcon != null
                        ? stageReq.targetWorld.WorldIcon
                        : stageReq.targetWorld.StageBackground;

                    RequirementItemUI row = Instantiate(view.RequirementPrefab, view.RequirementsContainer);

                    // Теперь передаем иконку локации первым параметром!
                    row.Setup(locationIcon, reqName, reqProgress, stageMet);

                    _activeRequirements.Add(row);
                }
            }


            // 3. ОТРИСОВКА ВСЕХ ТРЕБУЕМЫХ МЕЧЕЙ / ОРУЖИЯ (Цикл по списку)
            if (req.useWeaponRequirement && req.weaponRequirements != null)
            {
                foreach (var weaponReq in req.weaponRequirements)
                {
                    if (weaponReq.requiredWeapon == null) continue;

                    string weaponID = weaponReq.requiredWeapon.ID;
                    string weaponName = weaponReq.requiredWeapon.DisplayName;
                    Sprite weaponCustomIcon = weaponReq.requiredWeapon.Icon;

                    int currentWeaponLevel = DataManager.Instance.GetWeaponLevel(weaponID);
                    bool weaponMet = currentWeaponLevel >= weaponReq.requiredWeaponLevel;

                    // ИСПРАВЛЕНО: Разделяем имя оружия и его уровень (передаем 4 аргумента)
                    string reqName = weaponName;
                    string reqProgress = $"({currentWeaponLevel}/{weaponReq.requiredWeaponLevel} ур.)";

                    RequirementItemUI row = Instantiate(view.RequirementPrefab, view.RequirementsContainer);
                    row.Setup(weaponCustomIcon, reqName, reqProgress, weaponMet);

                    _activeRequirements.Add(row);
                }
            }

            // 4. ОТРИСОВКА ВСЕХ ПРЕДМЕТОВ ИНВЕНТАРЯ (Синхронизировано с логикой инвентаря)
            if (req.useItemRequirement && req.itemRequirements != null)
            {
                foreach (var itemReq in req.itemRequirements)
                {
                    if (itemReq.requiredItem == null) continue;

                    // Получаем ID предмета из конфига требований
                    string itemID = itemReq.requiredItem.ID;

                    // Вытягиваем полноценный конфиг из вашей базы данных GameDB (как в инвентаре!)
                    ItemConfig dbItemConfig = null;
                    if (GameDB.Items != null)
                    {
                        dbItemConfig = GameDB.Items.GetByID(itemID);
                    }

                    // Ищем предмет в сохранении для отображения текущего количества
                    var inventoryItem = saveData.InventoryItems.FirstOrDefault(item => item.itemID == itemID);
                    int currentAmount = inventoryItem.itemID != null ? inventoryItem.amount : 0;
                    bool itemMet = currentAmount >= itemReq.requiredItemAmount;

                    // Берем данные: если база данных выдала конфиг — берем DisplayName и Icon, иначе — имя из инспектора требований
                    string itemName = dbItemConfig != null ? dbItemConfig.DisplayName : itemReq.requiredItem.DisplayName;
                    Sprite itemCustomIcon = dbItemConfig != null ? dbItemConfig.Icon : itemReq.requiredItem.Icon;

                    // Разделяем на имя и прогресс для префаба
                    string reqName = itemName;
                    string reqProgress = $"({currentAmount}/{itemReq.requiredItemAmount} шт.)";

                    RequirementItemUI row = Instantiate(view.RequirementPrefab, view.RequirementsContainer);
                    row.Setup(itemCustomIcon, reqName, reqProgress, itemMet);

                    _activeRequirements.Add(row);
                }
            }

            // Вставляем в самый конец метода UpdateUnlockRequirementsUI
            if (view.RequirementsContainer != null)
            {
                // Принудительно обновляем сетку для контейнера строк
                LayoutRebuilder.ForceRebuildLayoutImmediate(view.RequirementsContainer.GetComponent<RectTransform>());
            }

            if (view.RequirementsTitleText != null)
            {
                // Принудительно обновляем сетку для всего общего блока (вместе с вашим текстом заголовка)
                LayoutRebuilder.ForceRebuildLayoutImmediate(view.RequirementsTitleText.GetComponent<RectTransform>());
            }
        }


    }

}
