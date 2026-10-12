using System;
using Cysharp.Threading.Tasks;
using Event_Bus;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverHud : MonoBehaviour
{
    [SerializeField] private Button buttonMenu;
    [SerializeField] private Button buttonRestart;
    
    [SerializeField] private CanvasGroup canvasGroupGameOver;
    
    [SerializeField] private TMP_Text textGameOver;

    private GameService m_gameService;
    
    private EventBinding<RunEndedEvent> m_runEndedEvent;
    
    private void Awake()
    {
        canvasGroupGameOver.alpha = 0f;
        
        buttonMenu.interactable = false;
        buttonRestart.interactable = false;
        
        m_gameService = ServiceLocator.Get<GameService>();
        
        buttonMenu.onClick.AddListener(LoadMenu);
        buttonRestart.onClick.AddListener(LoadRestart);
        
        m_runEndedEvent = new EventBinding<RunEndedEvent>(ShowGameOver);
        EventBus<RunEndedEvent>.Register(m_runEndedEvent);
    }

    private void OnDestroy()
    {
        EventBus<RunEndedEvent>.Unregister(m_runEndedEvent);
    }

    private void LoadMenu()
    {
        buttonMenu.interactable = false;
        buttonRestart.interactable = false;
        m_gameService.LoadMenuFromGameplay().Forget();
    }
    
    private void LoadRestart()
    {
        buttonMenu.interactable = false;
        buttonRestart.interactable = false;
        m_gameService.StartGameFromGameOver().Forget();
    }
    
    private void ShowGameOver() => ShowGameOverAsync().Forget();

    private async UniTask ShowGameOverAsync()
    {
        textGameOver.text = $"You have reached the {m_gameService.CurrentFloor} floor.";
        
        await Tween.Alpha(canvasGroupGameOver, 1f, 0.5f);
        
        buttonMenu.interactable = true;
        buttonRestart.interactable = true;
    }
}
