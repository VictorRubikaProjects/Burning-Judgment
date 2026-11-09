using System;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button[] optionButtons; 
    
    [Header("Panels / Canvas Groups")]
    [SerializeField] private CanvasGroup optionCanvasGroup;
    [SerializeField] private CanvasGroup creditsCanvasGroup; 
    [SerializeField] private Button closeCreditsButton;     

    [Header("Slider")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    
    [Header("RectTransforms (Menu Principal)")]
    [SerializeField] private RectTransform[] menuButtonRects; 
    [SerializeField] private RectTransform m_canvasRT;

    [Header("Intro Animation")]
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float staggerDelay = 0.1f; 
    [SerializeField] private float offScreenMargin = 50f;
    [SerializeField] private Ease slideEase = Ease.OutCubic;
    
    [Header("Sound")] 
    [SerializeField] private SO_AudioUI audioUI;
    
    private SceneService m_sceneService;
    private AudioService m_audioService;
    private OptionController m_optionController;

    private void Awake()
    {
        m_sceneService = ServiceLocator.Get<SceneService>();
        m_audioService = ServiceLocator.Get<AudioService>();
        
        playButton.onClick.AddListener(LoadGame);
        settingsButton.onClick.AddListener(OpenSettings);
        creditsButton.onClick.AddListener(OpenCredits);
        exitButton.onClick.AddListener(ExitGame);
        
        if (closeCreditsButton != null)
        {
            closeCreditsButton.onClick.AddListener(CloseCredits);
        }
        
        m_optionController = new OptionController(
            audioUI,
            optionButtons,
            optionCanvasGroup,
            volumeSlider,
            sfxSlider,
            musicSlider,
            pauseTime: false);
        
        m_optionController.Init();

        SetupCreditsPanel(false, instantaneous: true);

        PlayIntroAnimation();
    }

    private void OnDestroy()
    {
        m_optionController.Clean();
    }

    private void PlayIntroAnimation()
    {
        for (int i = 0; i < menuButtonRects.Length; i++)
        {
            RectTransform rt = menuButtonRects[i];
            Vector2 targetPos = rt.anchoredPosition;

            bool comeFromLeft = (i % 2 == 0);
            float offScreenX = comeFromLeft ? 
                -(m_canvasRT.rect.width * 0.5f + rt.rect.width * 0.5f + offScreenMargin) : 
                 (m_canvasRT.rect.width * 0.5f + rt.rect.width * 0.5f + offScreenMargin);

            rt.anchoredPosition = new Vector2(offScreenX, targetPos.y);

            float delay = i * staggerDelay;
            Tween.UIAnchoredPosition(rt, targetPos, slideDuration, slideEase, startDelay: delay);
        }
    }

    private void LoadGame()
    {
        SetButtonsInteractivity(false);
        m_audioService.PlaySfx(audioUI.NewGameFx);
        ServiceLocator.Get<GameService>().StartGame().Forget();
    }

    private void OpenSettings()
    {
        //m_audioService.PlaySfx(audioUI.ClickFx);
        optionCanvasGroup.alpha = 1f;
        optionCanvasGroup.interactable = true;
        optionCanvasGroup.blocksRaycasts = true;
    }

    private void OpenCredits()
    {
        //m_audioService.PlaySfx(audioUI.ClickFx);
        SetupCreditsPanel(true, instantaneous: false);
    }

    private void CloseCredits()
    {
        //m_audioService.PlaySfx(audioUI.ClickFx);
        SetupCreditsPanel(false, instantaneous: false);
    }

    private void SetupCreditsPanel(bool isOpen, bool instantaneous)
    {
        if (creditsCanvasGroup == null) return;

        float targetAlpha = isOpen ? 1f : 0f;

        if (instantaneous)
        {
            creditsCanvasGroup.alpha = targetAlpha;
            creditsCanvasGroup.interactable = isOpen;
            creditsCanvasGroup.blocksRaycasts = isOpen;
        }
        else
        {
            creditsCanvasGroup.interactable = isOpen;
            creditsCanvasGroup.blocksRaycasts = isOpen;
            Tween.Alpha(creditsCanvasGroup, targetAlpha, 0.3f, Ease.OutQuad);
        }
    }

    private void ExitGame()
    {
        //m_audioService.PlaySfx(audioUI.ClickFx);
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void SetButtonsInteractivity(bool state)
    {
        playButton.interactable = state;
        settingsButton.interactable = state;
        creditsButton.interactable = state;
        exitButton.interactable = state;
    }
}