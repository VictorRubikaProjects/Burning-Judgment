using System;
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
    
    private SceneService m_sceneService;
    private OptionController  m_optionController;

    private void Awake()
    {
        m_sceneService = ServiceLocator.Get<SceneService>();
        
        playButton.onClick.AddListener(LoadGame);
        
        m_optionController = new OptionController(
            optionButtons,
            optionCanvasGroup,
            volumeSlider,
            sfxSlider,
            musicSlider,
            pauseTime:false);
        
        m_optionController.Init();
    }

    private void LoadGame()
    {
        playButton.interactable = false;
        m_sceneService.LoadGameSceneAsync().Forget();
    }
}
