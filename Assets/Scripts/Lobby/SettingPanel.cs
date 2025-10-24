using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace ForYou.Lobby
{
    public class SettingPanel : MonoBehaviour
    {
        [SerializeField] Slider ScreeLightSlider;
        [SerializeField] Slider BGMSlider;
        [SerializeField] Slider SFXSlider;

        [SerializeField] Button ApplyButton;
        [SerializeField] Button ExitButton;

        [SerializeField] AudioMixerGroup BGMGroup;
        [SerializeField] AudioMixerGroup SFXGroup;

        const string ScreenLightKey = "ScreenLight";
        const string BGMKey = "BGM";
        const string SFXKey = "SFX";

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            LoadValues();
            ApplyValues();
        }


        void ApplyValues()
        {
            BGMGroup.audioMixer.SetFloat("BGMVolume", Mathf.Log10(BGMSlider.value) * 20);
            SFXGroup.audioMixer.SetFloat("SFXVolume", Mathf.Log10(SFXSlider.value) * 20);
            BrightnessVolume.Instance.SetValue(ScreeLightSlider.value);
            SaveValues();
        }

        void LoadValues()
        {
            ScreeLightSlider.value = PlayerPrefs.GetFloat(ScreenLightKey, 1f);
            BGMSlider.value = PlayerPrefs.GetFloat(BGMKey, 0.5f);
            SFXSlider.value = PlayerPrefs.GetFloat(SFXKey, 0.5f);
        }
        void SaveValues()
        {
            PlayerPrefs.SetFloat(ScreenLightKey, ScreeLightSlider.value);
            PlayerPrefs.SetFloat(BGMKey, BGMSlider.value);
            PlayerPrefs.SetFloat(SFXKey, SFXSlider.value);
        }
        
        private void Awake()
        {
            LoadValues();

            ScreeLightSlider.onValueChanged.AddListener((v) =>
            {
                ApplyValues();
            });
            BGMSlider.onValueChanged.AddListener((v) =>
            {
                ApplyValues();
            });
            SFXSlider.onValueChanged.AddListener((v) =>
            {
                ApplyValues();
            });
            ApplyButton.onClick.AddListener(() =>
            {
                ApplyValues();
            });
            ExitButton.onClick.AddListener(() =>
            {
                ApplyValues();
                LobbyUIController.Instance.ShowStartSceen();
            });
        }
    }
}