using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [Header("Volume Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Volume Text (Optional)")]
    public Text bgmVolumeText;
    public Text sfxVolumeText;

    void Start()
    {
        // 슬라이더 초기값 설정
        if (AudioManager.instance != null)
        {
            bgmSlider.value = AudioManager.instance.bgmVolume;
            sfxSlider.value = AudioManager.instance.sfxVolume;
        }

        // 슬라이더 이벤트 연결
        bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

        // 초기 텍스트 업데이트
        UpdateVolumeText();
    }

    void OnBgmVolumeChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetBgmVolume(value);
            UpdateBgmText(value);
        }
    }

    void OnSfxVolumeChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetSfxVolume(value);
            UpdateSfxText(value);
            
            // 효과음 테스트 재생 (선택사항)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        }
    }

    void UpdateVolumeText()
    {
        UpdateBgmText(bgmSlider.value);
        UpdateSfxText(sfxSlider.value);
    }

    void UpdateBgmText(float value)
    {
        if (bgmVolumeText != null)
            bgmVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    void UpdateSfxText(float value)
    {
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    void OnDestroy()
    {
        // 이벤트 해제
        bgmSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
    }
}