using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameInitializer
{
    private SO_GameConfig _gameConfig;
    
    private SceneService _sceneService;
    private SaveService _saveService;

    public bool IsInitialized { get; private set; } = false;

    public void Initialize(SO_GameConfig gameConfig)
    {
        _gameConfig = gameConfig;
        
        CreateServices();
        
        InitializeAsync().Forget();
    }

    private async UniTask InitializeAsync()
    {
        try
        {
            await RegisterServices();

            await _sceneService.LoadSceneAsync(_gameConfig.menuScene.Name, setAsActiveScene: true);
        
            _sceneService.ToggleLoadingScreen(on : false);
            
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
        _sceneService = new SceneService();
        _saveService =  new SaveService();
    }
    

    private async UniTask RegisterServices()
    {
        UniTask sceneTaskRegister = ServiceLocator.Register(_sceneService);
        UniTask saveTaskRegister = ServiceLocator.Register(_saveService);
        
        await UniTask.WhenAll(sceneTaskRegister,saveTaskRegister);
    }
}