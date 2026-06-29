using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using SlimeRpgEvolution2D.UI.Core;

namespace SlimeRpgEvolution2D.UI.Popups
{
    public class SettingsManager : MonoBehaviour
    {

        [SerializeField] private AudioMixer _mixer;


        [Header("Music UI")]
        [SerializeField] private Sprite _musicOn;
        [SerializeField] private Sprite _musicOff;
        [SerializeField] private Image _musicButtonImage;
        [SerializeField] private Slider _musicSlider; 

        [Header("SFX UI")]
        [SerializeField] private Sprite _sfxOn;
        [SerializeField] private Sprite _sfxOff;
        [SerializeField] private Image _sfxButtonImage;
        [SerializeField] private Slider _sfxSlider;

        [Header("Animations Settings")]
        [SerializeField] private CanvasGroup _canvasGroup;   
        [SerializeField] private Transform _windowContent;   
        [SerializeField] private float _animationDuration = 0.2f;


        private Coroutine _animationRoutine;

        private bool _isMusicMuted;
        private bool _isSfxMuted;

        private void OnEnable()
        {
            if (_animationRoutine != null) StopCoroutine(_animationRoutine);

            // ВЫЗЫВАЕМ ОБЩИЙ ХЕЛПЕР: Передаем компоненты этого окна в универсальную анимацию открытия
            _animationRoutine = StartCoroutine(UIAnimationHelper.AnimateWindow(
                _canvasGroup, _windowContent, 0f, 1f, 0.8f, 1f, _animationDuration
            ));
        }
        

        private void Start()
        {
            
            float musicVol = PlayerPrefs.GetFloat("MusicVolSave", 1f);
            float sfxVol = PlayerPrefs.GetFloat("SfxVolSave", 1f);


            _musicSlider.value = musicVol;
            _sfxSlider.value = sfxVol;

            SetMusicVolume(musicVol);
            SetSfxVolume(sfxVol);
        }


        public void SetMusicVolume(float sliderValue)
        {
            float dbValue = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20;
            _mixer.SetFloat("MusicVol", dbValue);

            PlayerPrefs.SetFloat("MusicVolSave", sliderValue);

            _musicButtonImage.sprite = sliderValue <= 0.0001f ? _musicOff : _musicOn;
        }

        public void SetSfxVolume(float sliderValue)
        {
            float dbValue = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20;
            _mixer.SetFloat("SfxVol", dbValue);

            PlayerPrefs.SetFloat("SfxVolSave", sliderValue);

            _sfxButtonImage.sprite = sliderValue <= 0.0001f ? _sfxOff : _sfxOn;
        }

        public void ToggleMusic()
        {
            _isMusicMuted = !_isMusicMuted;

            float volume = _isMusicMuted ? 0.0001f : _musicSlider.value;
            float dbValue = Mathf.Log10(volume) * 20;

            _mixer.SetFloat("MusicVol", dbValue);

            _musicButtonImage.sprite = _isMusicMuted ? _musicOff : _musicOn;
        }


        public void ToggleSFX()
        {
            _isSfxMuted = !_isSfxMuted;

            float volume = _isSfxMuted ? 0.0001f : _sfxSlider.value;
            float dbValue = Mathf.Log10(volume) * 20;

            _mixer.SetFloat("SfxVol", dbValue);

            _sfxButtonImage.sprite = _isSfxMuted ? _sfxOff : _sfxOn;
        }

        public void CloseSettings()
        {
            if (_animationRoutine != null) StopCoroutine(_animationRoutine);

            // ВЫЗЫВАЕМ ОБЩИЙ ХЕЛПЕР НА ЗАКРЫТИЕ:
            _animationRoutine = StartCoroutine(UIAnimationHelper.AnimateWindow(
                _canvasGroup, _windowContent, 1f, 0f, 1f, 0.8f, _animationDuration, () =>
                {
                    gameObject.SetActive(false); // Выключаем объект только после конца анимации

                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.NotifyWindowClosed(); // Проверяем, нужно ли скрыть подложку
                    }
                }
            ));
        }

    }
}

