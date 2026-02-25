using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

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


        private bool _isMusicMuted;
        private bool _isSfxMuted;


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
    }
}

