using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class OptionController
{
    private Button[] m_optionButtons;
    
    private Slider m_sliderMaster;
    private Slider m_sliderSfx;
    private Slider m_sliderMusic;
    
    private CanvasGroup m_optionPanel;
    
    private bool m_isOpen;
    private bool m_pauseTime;

    private float m_durationOptionTween = 0.3f;
    
    Tween m_optionTween;
    
    private AudioService m_audioService;
    private SaveService m_saveService;
    
    [Header("Sound")] 
    private SO_AudioUI m_audioUI;

    public OptionController(SO_AudioUI audioUI, Button[] optionButtons, CanvasGroup optionPanel,Slider sliderMaster,Slider sliderSfx,Slider sliderMusic, bool pauseTime = true, float startingAlpha = 0)
    {
        m_audioUI = audioUI;
        
        m_optionButtons =  optionButtons;
        m_optionPanel = optionPanel;
        m_pauseTime = pauseTime;
        m_optionPanel.alpha = startingAlpha;
        
        if (startingAlpha == 0) m_optionPanel.blocksRaycasts = false;
        
        m_sliderMaster = sliderMaster;
        m_sliderSfx = sliderSfx;
        m_sliderMusic = sliderMusic;
        
        m_audioService = ServiceLocator.Get<AudioService>();
        m_saveService = ServiceLocator.Get<SaveService>();
    }

    public void Init()
    {
        BindUIEvents();
    }

    public void Clean()
    {
        UnbindUIEvents();
    }

    private void ToggleOption()
    {
        if (m_optionTween.isAlive)
            m_optionTween.Stop();

        m_isOpen = !m_isOpen;

        if (m_isOpen)
            OpenOptions();
        else
            CloseOptions();
    }

    private void OpenOptions()
    {
        UpdateUIFromSave();

        m_optionPanel.blocksRaycasts = true;
        if (m_pauseTime) Time.timeScale = 0;

        m_optionTween = Tween.Alpha(m_optionPanel, 1f, m_durationOptionTween, Ease.Linear, useUnscaledTime: true);
        
        m_audioService.PlaySfx(m_audioUI.OptionToggleFx,Vector3.zero);
    }

    private void CloseOptions()
    {
        SaveSettings();

        m_optionPanel.blocksRaycasts = false;
        if (m_pauseTime) Time.timeScale = 1;

        m_optionTween = Tween.Alpha(m_optionPanel, 0f, m_durationOptionTween, Ease.Linear, useUnscaledTime: true);
        m_audioService.PlaySfx(m_audioUI.OptionToggleFx,Vector3.zero);
    }
    
    private void UpdateUIFromSave()
    {
        if (m_saveService == null)
            return;

        SettingsData settings = m_saveService.Settings;

        if (m_sliderMaster)
            m_sliderMaster.SetValueWithoutNotify(settings.MasterVolume);

        if (m_sliderMusic)
            m_sliderMusic.SetValueWithoutNotify(settings.MusicVolume);

        if (m_sliderSfx)
            m_sliderSfx.SetValueWithoutNotify(settings.SfxVolume);
    }
    

    private void OnMasterVolumeChanged(float value)
    {
        if (m_saveService == null)
            return;

        m_saveService.Settings.MasterVolume = value;
        m_audioService.SetVolume(AudioService.BusEnum.MASTER, value);
        m_audioService.PlaySfx(m_audioUI.SliderFx, Vector3.zero);
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (m_saveService == null)
            return;

        m_saveService.Settings.MusicVolume = value;
        m_audioService.SetVolume(AudioService.BusEnum.MUSIC, value);
        m_audioService.PlaySfx(m_audioUI.SliderFx, Vector3.zero);
    }

    private void OnSfxVolumeChanged(float value)
    {
        if (m_saveService == null)
            return;

        m_saveService.Settings.SfxVolume = value;
        m_audioService.SetVolume(AudioService.BusEnum.SFX, value);
        m_audioService.PlaySfx(m_audioUI.SliderFx, Vector3.zero);
    }
    
    
    private void BindUIEvents()
    {
        foreach (Button button in m_optionButtons)
            button.onClick.AddListener(ToggleOption);
        
        m_sliderMaster.onValueChanged.AddListener(OnMasterVolumeChanged);
        m_sliderSfx.onValueChanged.AddListener(OnSfxVolumeChanged);
        m_sliderMusic.onValueChanged.AddListener(OnMusicVolumeChanged);
    }

    private void UnbindUIEvents()
    {
        foreach (Button button in m_optionButtons)
            button.onClick.RemoveListener(ToggleOption);
        
        m_sliderMaster.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        m_sliderSfx.onValueChanged.RemoveListener(OnSfxVolumeChanged);
        m_sliderMusic.onValueChanged.RemoveListener(OnMusicVolumeChanged);
    }
    
    private void SaveSettings() => m_saveService?.SaveSettings().Forget();
}
