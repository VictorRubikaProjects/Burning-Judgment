using Cysharp.Threading.Tasks;
using Event_Bus;
using UnityEngine;

public class GameService : IGameService
{
    public bool IsInitialized { get; set; }
    public GameState CurrentState { get; private set; }
    public int CurrentFloor { get; private set; }

    private SceneService m_sceneService;
    private TransitionService m_transitionService;
    private EnemySystem m_enemySystem;
    private SO_EntityPooling m_entityConfig;
    private PlayerCharacter m_playerCharacter;

    public GameService(SO_EntityPooling entityConfig)
    {
        m_entityConfig = entityConfig;
    }

    public void Dispose() { }

    public UniTask InitializeService()
    {
        m_sceneService = ServiceLocator.Get<SceneService>();
        m_transitionService = ServiceLocator.Get<TransitionService>();

        m_enemySystem = new EnemySystem(m_entityConfig); 
        m_enemySystem.Initialize();

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

        await m_sceneService.UnloadMenu();
        
        await StartFloor(true);
        
        await m_transitionService.CircleTransitionFadeOut();
    }

    public async UniTask StartFloor(bool isFirstFloor = false)
    {
        CurrentFloor++;

        if (!isFirstFloor) await m_sceneService.UnloadScene(m_enemySystem.ConfigCurrentRound.RoundData.Map.Name);
        
        m_enemySystem.SetupRound();
        
        await m_sceneService.LoadSceneAsync(m_enemySystem.ConfigCurrentRound.RoundData.Map.Name);
        
        await SpawnPlayer();
        
        await m_transitionService.ShowFloorAsync(CurrentFloor);

        m_enemySystem.SetupFloor(m_playerCharacter);

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

    private async UniTask SpawnPlayer()
    {
        if (m_playerCharacter == null)
        {
            m_playerCharacter = Object.Instantiate(m_entityConfig.Player);
        }
        
        m_playerCharacter.gameObject.SetActive(false);

        await UniTask.WaitForFixedUpdate();
        
        m_playerCharacter.TransformCache.position = m_enemySystem.ConfigCurrentRound.PlayerSpawnPosition;
        m_playerCharacter.TransformCache.rotation = Quaternion.identity;
        
        await UniTask.WaitForFixedUpdate();
        
        m_playerCharacter.gameObject.SetActive(true);
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