using System;
using Cysharp.Threading.Tasks;
using Project._05_Scripts.Runtime.System.ServiceLocator.Services;
using UnityEngine;

public class GameInitializer
{
    private SO_GameConfig m_gameConfig;
    private SO_AudioConfig m_audioConfig;
    
    private SceneService m_sceneService;
    private SaveService m_saveService;
    private CameraService m_cameraService;
    private FxService m_fxService;
    private AudioService m_audioService;
    private GameService m_gameService;
    private TransitionService m_transitionService;

    public bool IsInitialized { get; private set; } = false;

    public void Initialize(SO_GameConfig gameConfig, SO_AudioConfig audioConfig)
    {
        m_gameConfig = gameConfig;
        m_audioConfig = audioConfig;
        
        CreateServices();
        
        InitializeAsync().Forget();
    }

    private async UniTask InitializeAsync()
    {
        try
        {
            await RegisterServices();

            await m_sceneService.LoadSceneAsync(m_gameConfig.menuScene.Name, setAsActiveScene: true);
        
            m_transitionService.ToggleLoadingScreen(on : false);
            
            IsInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            throw;
        }
    }

    private void CreateServices()
    {
        m_transitionService = new TransitionService();
        
        m_sceneService = new SceneService(m_gameConfig);
        
        m_saveService =  new SaveService();
        
        m_cameraService = new CameraService(m_gameConfig.cameraConfig);

        m_fxService = new FxService();

        m_audioService = new AudioService(m_audioConfig);

        m_gameService = new GameService();
    }
    

    private async UniTask RegisterServices()
    {
        UniTask transitionTaskRegister = ServiceLocator.Register(m_transitionService);
        UniTask sceneTaskRegister = ServiceLocator.Register(m_sceneService);
        UniTask saveTaskRegister = ServiceLocator.Register(m_saveService);
        UniTask cameraTaskRegister = ServiceLocator.Register(m_cameraService);
        UniTask fxTaskRegister = ServiceLocator.Register(m_fxService);
        UniTask audioTaskRegister = ServiceLocator.Register(m_audioService);
        UniTask gameTaskRegister = ServiceLocator.Register(m_gameService);
        
        await UniTask.WhenAll(
            sceneTaskRegister,
            saveTaskRegister, 
            cameraTaskRegister,
            fxTaskRegister,
            audioTaskRegister,
            gameTaskRegister,
            transitionTaskRegister);
    }
}