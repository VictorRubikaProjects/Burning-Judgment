using Cysharp.Threading.Tasks;
using Event_Bus;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameService : IGameService
{
    public bool IsInitialized { get; set; }
    public int CurrentFloor { get; private set; }

    private SceneService m_sceneService;
    private TransitionService m_transitionService;
    private EnemySystem m_enemySystem;
    private SO_EntityPooling m_entityConfig;
    public PlayerCharacter Player { get; private set; }
    
    private readonly AugmentService m_augmentService;
    private readonly AudioService m_audioService;

    private PlayerCharacter.PlayerPersistentData m_playerDataPersistent;

    public GameService(SO_EntityPooling entityConfig, AugmentService augmentService, AudioService audioService)
    {
        m_entityConfig = entityConfig;
        m_augmentService = augmentService;
        m_audioService = audioService;
    }

    public void Dispose() 
    {
        m_enemySystem?.Dispose();
    }

    public UniTask InitializeService()
    {
        m_sceneService = ServiceLocator.Get<SceneService>();
        m_transitionService = ServiceLocator.Get<TransitionService>();

        m_enemySystem = new EnemySystem(m_entityConfig); 
        m_enemySystem.Initialize();

        return UniTask.CompletedTask;
    }

    public void ShutDownService() 
    {
        m_enemySystem?.Dispose();
    }

    public void Tick() { }

    public async UniTask StartGame()
    {
        await InternalStartGame(async () => await m_sceneService.UnloadMenu());
    }

    public async UniTask StartGameFromGameOver()
    {
        await InternalStartGame(async () => await m_sceneService.UnloadScene(SceneManager.GetActiveScene().name));
    }

    private async UniTask InternalStartGame(System.Func<UniTask> unloadPreviousSceneAction)
    {
        m_audioService.PlaySfx(m_audioService.Bank.StartGameEvent);
        
        CurrentFloor = 0;
        m_augmentService.Reset();

        EventBus<RunStartedEvent>.Raise(new RunStartedEvent());

        await m_transitionService.CircleTransitionFadeIn();
        await unloadPreviousSceneAction();
        await StartFloor(true);
        await m_transitionService.CircleTransitionFadeOut();
    }

    public async UniTask StartFloor(bool isFirstFloor = false)
    {
        CurrentFloor++;

        UniTask taskDeloadGameplay = isFirstFloor 
            ? UniTask.CompletedTask 
            : m_sceneService.UnloadScene(m_enemySystem.ConfigCurrentRound.RoundData.Map.Name);
        
        m_enemySystem.SetupRound();

        UniTask taskLoadGameplay = m_sceneService.LoadSceneAsync(m_enemySystem.ConfigCurrentRound.RoundData.Map.Name, true);

        await UniTask.WhenAll(taskLoadGameplay, taskDeloadGameplay);
        
        await SpawnPlayer(isFirstFloor);
        
        await m_transitionService.ShowFloorAsync(CurrentFloor);

        m_enemySystem.SetupFloor(Player);

        EventBus<FloorStartedEvent>.Raise(new FloorStartedEvent(CurrentFloor));
    }

    public void OnPortalTaken()
    {
        m_playerDataPersistent = Player.GetPersistentData();
        EventBus<FloorCompletedEvent>.Raise(new FloorCompletedEvent(CurrentFloor));
        InternalPortalTransition().Forget();
    }

    private async UniTaskVoid InternalPortalTransition()
    {
        await m_transitionService.CircleTransitionFadeIn();

        await StartFloor();

        await m_transitionService.CircleTransitionFadeOut();
    }

    public void EndRun()
    {
        EventBus<RunEndedEvent>.Raise(new RunEndedEvent(CurrentFloor));
    }
    

    private async UniTask SpawnPlayer(bool isFirstFloor = false)
    {
        if (Player == null)
        {
            Player = Object.Instantiate(m_entityConfig.Player);
        }

        if (isFirstFloor)
        {
            m_playerDataPersistent = new PlayerCharacter.PlayerPersistentData(Player.GetRuntimeStability());
        }
        
        Player.SetControlContext(PlayerControlContext.RoundCountdown);
        Player.SetPersistentData(m_playerDataPersistent);
        Player.SetIdle();
        Player.gameObject.SetActive(false);

        await UniTask.WaitForFixedUpdate();

        Player.TransformCache.position = m_enemySystem.ConfigCurrentRound.PlayerSpawnPosition;
        Player.TransformCache.rotation = Quaternion.identity;

        await UniTask.WaitForFixedUpdate();

        Player.gameObject.SetActive(true);
    }

    public async UniTask LoadMenuFromGameplay()
    {
        await m_transitionService.CircleTransitionFadeIn();
        await m_sceneService.UnloadScene(SceneManager.GetActiveScene().name);
        await m_sceneService.LoadMenu();
        await m_transitionService.CircleTransitionFadeOut();
    }
    
}
