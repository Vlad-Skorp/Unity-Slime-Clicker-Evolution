using SlimeRpgEvolution2D.Data; 
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

namespace SlimeRpgEvolution2D.UI.Core
{
    public class UIStageController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI stageText;          
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Button leftArrowBtn;     // Стрелочка влево
        [SerializeField] private Button rightArrowBtn;    // Стрелочка вправо


        private void Start()
        {
            // 1. Подписываемся на изменения этапа (чтобы скрывать стрелочки)
            GameLevelManager.OnStageChanged += RefreshStageUI;

            // NEW: Подписываемся на счетчик убийств 0 / 10
            GameLevelManager.OnStageProgressChanged += RefreshProgressText;

            // 2. Привязываем клики по кнопкам
            if (leftArrowBtn != null)
                leftArrowBtn.onClick.AddListener(OnLeftArrowClick);

            if (rightArrowBtn != null)
                rightArrowBtn.onClick.AddListener(OnRightArrowClick);
        }



        private void OnDestroy()
        {
            GameLevelManager.OnStageChanged -= RefreshStageUI;
            GameLevelManager.OnStageProgressChanged -= RefreshProgressText;

            if (leftArrowBtn != null) leftArrowBtn.onClick.RemoveListener(OnLeftArrowClick);
            if (rightArrowBtn != null) rightArrowBtn.onClick.RemoveListener(OnRightArrowClick);
        }

        // 1. Метод обновляет только цифры "3 / 10"
        private void RefreshProgressText(int killed, int required)
        {
            if (progressText != null)
            {
                progressText.text = $"{killed} / {required}";
            }
        }

        private void RefreshStageUI(int currentStage, int maxReachedStage)
        {
            // 1. Номер этапа по центру обновляем всегда мгновенно
            if (stageText != null)
                stageText.text = currentStage.ToString();

            if (progressText != null)
            {
                progressText.gameObject.SetActive(currentStage == maxReachedStage);
            }

         

            // 2. Видимость Левой стрелочки (Назад) — обновляем ВСЕГДА
            if (leftArrowBtn != null)
            {
                leftArrowBtn.gameObject.SetActive(currentStage > 1);

                // Кликабельность зависит от кулдауна: если антиспам активен — кнопка выключена
                leftArrowBtn.interactable = !isCooldownActive;
            }

            // 3. Видимость Правой стрелочки (Вперед) — обновляем ВСЕГДА
            if (rightArrowBtn != null && GameLevelManager.Instance != null && GameLevelManager.Instance.CurrentLevelSettings != null)
            {
                int totalStages = GameLevelManager.Instance.CurrentLevelSettings.totalStagesInLocation;

                // Она физически исчезнет с экрана, если этапов больше нет (2 < 2 -> false)
                rightArrowBtn.gameObject.SetActive(currentStage < totalStages);

                // Кликабельность: если кулдаун активен — выключена. 
                // Если кулдауна нет — проверяем честный прогресс игрока
                if (!isCooldownActive && rightArrowBtn.gameObject.activeSelf)
                {
                    rightArrowBtn.interactable = currentStage < maxReachedStage;
                }
                else if (isCooldownActive)
                {
                    rightArrowBtn.interactable = false;
                }
            }
        }





        [Header("Anti-Spam Settings")]
        [SerializeField] private float changeStageCooldown = 1.5f; // Время блокировки в секундах
        private bool isCooldownActive = false;

        private void OnLeftArrowClick()
        {
            // Если кулдаун активен — игнорируем клик
            if (isCooldownActive) return;

            if (GameLevelManager.Instance != null)
            {
                StartCoroutine(ButtonCooldownRoutine());
                GameLevelManager.Instance.MoveStageBackward();
            }
        }

        private void OnRightArrowClick()
        {
            // Если кулдаун активен — игнорируем клик
            if (isCooldownActive) return;

            if (GameLevelManager.Instance != null)
            {
                StartCoroutine(ButtonCooldownRoutine());
                GameLevelManager.Instance.MoveStageForward();
            }
        }

        private IEnumerator ButtonCooldownRoutine()
        {
            isCooldownActive = true;

            // Принудительно перерисовываем UI, чтобы кнопки затухли из-за флага isCooldownActive
            if (GameLevelManager.Instance != null)
            {
                GameLevelManager.Instance.UpdateStageUI();
            }

            yield return new WaitForSeconds(changeStageCooldown);

            isCooldownActive = false;

            // Кулдаун прошел — возвращаем кнопкам их честное состояние
            if (GameLevelManager.Instance != null)
            {
                GameLevelManager.Instance.UpdateStageUI();
            }
        }


    }
}