using SlimeRpgEvolution2D.Data;
using System.Linq; // Нужен для FirstOrDefault при проверке инвентаря
using UnityEngine;
using System.Collections;
using static StagePool;
using SlimeRpgEvolution2D.UI.Core;

namespace SlimeRpgEvolution2D.UI.World
{
    public class WorldManager : MonoBehaviour
    {
        public static WorldManager Instance { get; private set; }

        // Индекс мира, на который игрок сейчас смотрит в меню (1, 2...)
        public int ViewingWorldIndex { get; set; } = 1;


        [Header("Animations Settings")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _windowContent;
        [SerializeField] private float _animationDuration = 0.2f;

        private Coroutine _animationRoutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            if (_animationRoutine != null) StopCoroutine(_animationRoutine);

            // ВЫЗЫВАЕМ ОБЩИЙ ХЕЛПЕР: Передаем компоненты этого окна в универсальную анимацию открытия
            _animationRoutine = StartCoroutine(UIAnimationHelper.AnimateWindow(
                _canvasGroup, _windowContent, 0f, 1f, 0.8f, 1f, _animationDuration
            ));
        }

        public LevelSettings GetViewedWorldSettings()
        {
            if (GameDB.Level == null) return null;
            string targetID = $"level_{ViewingWorldIndex}";
            return GameDB.Level.GetByID(targetID);
        }

        public bool CanMoveForward()
        {
            if (GameDB.Level == null || GameDB.Level.AllEntries == null) return false;
            return ViewingWorldIndex < GameDB.Level.AllEntries.Count;
        }

        public bool CanMoveBackward()
        {
            return ViewingWorldIndex > 1;
        }

        public void TryPurchaseCurrentViewedWorld(LevelSettings settings)
        {
            if (GameLevelManager.Instance != null && settings != null)
            {
                GameLevelManager.Instance.TryUnlockNextWorld(settings);
            }
        }

        public bool IsWorldQuestsRequirementsMet(LevelSettings worldSettings)
        {
            if (DataManager.Instance == null || DataManager.Instance.SaveData == null) return false;

            UnlockRequirement req = worldSettings.UnlockRequirement;

            // 1. Проверка списка этапов
            if (req.useStageRequirement && req.stageRequirements != null)
            {
                foreach (var stageReq in req.stageRequirements)
                {
                    if (stageReq.targetWorld == null) continue;

                    var targetWorldState = DataManager.Instance.GetWorldState(stageReq.targetWorld.ID);

                    // Условие выполнено, если игрок прошел дальше этого этапа,
                    // ЛИБО находится на этом этапе и его текущая волна (killedEnemies) больше или равна требуемой
                    bool stageMet = targetWorldState.maxReachedStage > stageReq.requiredStageNumber ||
                                    (targetWorldState.maxReachedStage == stageReq.requiredStageNumber && targetWorldState.killedEnemies >= stageReq.requiredWaveNumber);

                    if (!stageMet) return false;
                }
            }


            // 2. Проверка списка оружия
            if (req.useWeaponRequirement && req.weaponRequirements != null)
            {
                foreach (var weaponReq in req.weaponRequirements)
                {
                    if (weaponReq.requiredWeapon == null) continue;

                    int currentWeaponLevel = DataManager.Instance.GetWeaponLevel(weaponReq.requiredWeapon.ID);
                    if (currentWeaponLevel < weaponReq.requiredWeaponLevel) return false;
                }
            }

            // 3. Проверка списка предметов инвентаря
            if (req.useItemRequirement && req.itemRequirements != null)
            {
                foreach (var itemReq in req.itemRequirements)
                {
                    if (itemReq.requiredItem == null) continue;

                    var inventoryItem = DataManager.Instance.SaveData.InventoryItems.FirstOrDefault(item => item.itemID == itemReq.requiredItem.ID);
                    int currentAmount = inventoryItem.itemID != null ? inventoryItem.amount : 0;

                    if (currentAmount < itemReq.requiredItemAmount) return false;
                }
            }

            return true;
        }


        public void CloseWindow()
        {
            // Защита: останавливаем старую анимацию, если игрок спамит кнопку закрытия
            if (_animationRoutine != null) StopCoroutine(_animationRoutine);

            // ВЫЗЫВАЕМ ОБЩИЙ ХЕЛПЕР НА ЗАКРЫТИЕ:
            _animationRoutine = StartCoroutine(UIAnimationHelper.AnimateWindow(
                _canvasGroup, _windowContent, 1f, 0f, 1f, 0.8f, _animationDuration, () =>
                {
                    gameObject.SetActive(false); // Выключаем объект строго после завершения анимации

                    // Идеальная синхронизация с вашим UIManager:
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.NotifyWindowClosed(); // Проверяем слой подложки
                    }
                }
            ));
        }

    }
}
