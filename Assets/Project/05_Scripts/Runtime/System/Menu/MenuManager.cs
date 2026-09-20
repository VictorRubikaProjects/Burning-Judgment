using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button[] optionButtons;
    
    [Header("Canvas Groups")]
    [SerializeField] private CanvasGroup optionCanvasGroup;
    
    [Header("Slider")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    
    [Header("RectTransform")]
    [SerializeField] private RectTransform m_playButtonRT;
    [SerializeField] private RectTransform m_optionButtonRT;
    [SerializeField] private RectTransform m_canvasRT;

    [Header("Intro Animation")]
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float offScreenMargin = 50f;
    [SerializeField] private Ease slideEase = Ease.OutCubic;
    
    [Header("Sound")] 
    [SerializeField] private SO_AudioUI audioUI;
    
    private SceneService m_sceneService;
    private AudioService m_audioService;
    private OptionController  m_optionController;

    private void Awake()
    {
        m_sceneService = ServiceLocator.Get<SceneService>();
        m_audioService = ServiceLocator.Get<AudioService>();
        
        playButton.onClick.AddListener(LoadGame);
        
        m_optionController = new OptionController(
            audioUI,
            optionButtons,
            optionCanvasGroup,
            volumeSlider,
            sfxSlider,
            musicSlider,
            pauseTime:false);
        
        m_optionController.Init();

        PlayIntroAnimation();
    }

    private void OnDestroy()
    {
        m_optionController.Clean();
    }

    private void PlayIntroAnimation()
    {
        Vector2 playTarget = m_playButtonRT.anchoredPosition;
        Vector2 optionTarget = m_optionButtonRT.anchoredPosition;

        m_playButtonRT.anchoredPosition = playTarget + Vector2.left * GetOffScreenOffset(m_playButtonRT);
        m_optionButtonRT.anchoredPosition = optionTarget + Vector2.right * GetOffScreenOffset(m_optionButtonRT);

        Tween.UIAnchoredPosition(m_playButtonRT, playTarget, slideDuration, slideEase);
        Tween.UIAnchoredPosition(m_optionButtonRT, optionTarget, slideDuration, slideEase);
    }

    private float GetOffScreenOffset(RectTransform rt) =>
        (m_canvasRT.rect.width * 0.5f) + (rt.rect.width * 0.5f) + offScreenMargin;

    private void LoadGame()
    {
        playButton.interactable = false;
        m_audioService.PlaySfx(audioUI.NewGameFx);
        m_sceneService.LoadGameSceneAsync().Forget();
    }
}