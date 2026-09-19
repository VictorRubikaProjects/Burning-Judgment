using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private CanvasGroup optionPanel;
    
    private SceneService m_sceneService;
    private OptionController  m_optionController;

    private void Awake()
    {
        m_sceneService = ServiceLocator.Get<SceneService>();
        
        playButton.onClick.AddListener(LoadGame);
        
        m_optionController = new OptionController(optionButtons, optionPanel,false);
        m_optionController.Init();
    }

    private void LoadGame()
    {
        playButton.interactable = false;
        m_sceneService.LoadGameSceneAsync().Forget();
    }
}
