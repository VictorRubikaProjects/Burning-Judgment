using Cysharp.Threading.Tasks;
using Event_Bus;

public class GameService : IGameService
{
    public bool IsInitialized { get; set; }
    public GameState CurrentState { get; private set; }
    public int CurrentFloor { get; private set; }

    private SceneService m_sceneService;
    private TransitionService m_transitionService;

    public GameService()
    {
    }

    public void Dispose() { }

    public UniTask InitializeService()
    {
        m_sceneService = ServiceLocator.Get<SceneService>();
        m_transitionService = ServiceLocator.Get<TransitionService>();
        return UniTask.CompletedTask;
    }

    public void ShutDownService() { }

    public void Tick() { }

    public async UniTask StartGame()
    {
        CurrentFloor = 0;
        
        CurrentState = GameState.Playing;

        EventBus<RunStartedEvent>.Raise(new RunStartedEvent());

        await m_transitionService.CircleTransitionFadeIn();

        m_transitionService.ToggleLoadingScreen(true);
        
        await m_sceneService.LoadGameSceneAsync();

        await StartFloor();
        
    }

    public async UniTask StartFloor()
    {
        CurrentFloor++;

        await m_transitionService.ShowFloorAsync(CurrentFloor);
        
        m_transitionService.ToggleLoadingScreen(false);

        await m_transitionService.CircleTransitionFadeOut();

        EventBus<FloorStartedEvent>.Raise(new FloorStartedEvent(CurrentFloor));
    }

    public void OnPortalTaken()
    {
        EventBus<FloorCompletedEvent>.Raise(new FloorCompletedEvent(CurrentFloor));

        StartFloor().Forget();
    }

    public void EndRun(bool victory)
    {
        CurrentState = victory ? GameState.Victory : GameState.GameOver;

        EventBus<RunEndedEvent>.Raise(new RunEndedEvent(victory, CurrentFloor));
    }

    public void Pause()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Paused;
    }

    public void Resume()
    {
        if (CurrentState != GameState.Paused) return;
        CurrentState = GameState.Playing;
    }
}

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    Victory
}