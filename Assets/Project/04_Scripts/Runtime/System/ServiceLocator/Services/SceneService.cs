using System;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneService : IGameService
{
    public event Action<string> OnLoadSceneStarted;
    public event Action<string> OnLoadSceneFinished;
    public event Action<float> OnLoadSceneProgress;

    public event Action<string> OnUnloadSceneStarted;
    public event Action<string> OnUnloadSceneFinished;
    public event Action<float> OnUnloadSceneProgress;

    private readonly SO_GameConfig m_gameConfig;
    private TransitionService m_transitionService;

    public SceneService(SO_GameConfig gameConfig)
    {
        m_gameConfig = gameConfig;
    }

    public void Dispose() { }

    public UniTask InitializeService()
    {
        m_transitionService = ServiceLocator.Get<TransitionService>();
        return UniTask.CompletedTask;
    }

    public void ShutDownService() { }
    public void Tick() { }
    public bool IsInitialized { get; set; }

    #region Load Scene Core

    public async UniTask LoadSceneAsync(string sceneName, bool setAsActiveScene = false, bool loadSingle = false)
    {
        OnLoadSceneStarted?.Invoke(sceneName);
        
        if (loadSingle)
        {
            await LoadSceneSingleAsync(sceneName);
        }
        else
        {
            await LoadSceneAdditiveAsync(sceneName, setAsActiveScene);
        }
        
        OnLoadSceneFinished?.Invoke(sceneName);
    }

    private async UniTask LoadSceneSingleAsync(string sceneName)
    {
        AsyncOperation handleSingleLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        if (handleSingleLoad == null)
        {
            throw new ArgumentNullException(nameof(sceneName),"[SceneService] Scene loading single failed.]");
        }

        while (!handleSingleLoad.isDone)
        {
            OnLoadSceneProgress?.Invoke(handleSingleLoad.progress);
            await UniTask.Yield();
        }

        await handleSingleLoad;
    }

    private async UniTask LoadSceneAdditiveAsync(string sceneName, bool setAsActiveScene)
    {
        AsyncOperation handleAdditiveLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        if (handleAdditiveLoad == null)
        {
            throw new ArgumentNullException(nameof(sceneName),"[SceneService] Scene loading additive failed.]");
        }
        
        while (!handleAdditiveLoad.isDone)
        {
            OnLoadSceneProgress?.Invoke(handleAdditiveLoad.progress);
            await UniTask.Yield();
        }

        await handleAdditiveLoad;

        if (setAsActiveScene)
        {
            Scene activeScene = SceneManager.GetSceneByName(sceneName);

            if (activeScene.IsValid())
            {
                SceneManager.SetActiveScene(activeScene);
            }
        }
    }
    
    public async UniTask UnloadScene(string sceneName)
    {
        OnUnloadSceneStarted?.Invoke(sceneName);
        
        var handle = SceneManager.UnloadSceneAsync(sceneName);
        
        if (handle == null) {
            throw new ArgumentNullException(nameof(sceneName),"[SceneService] Unload scene failed.]");
        }
            
        while (!handle.isDone)
        {
            OnUnloadSceneProgress?.Invoke(handle.progress);
            await UniTask.Yield();
        }
        
        await handle; 
        
        OnUnloadSceneFinished?.Invoke(sceneName);
    }

    #endregion

    public async UniTask LoadGameSceneAsync()
    {
        await UnloadScene(m_gameConfig.menuScene.Name);
        
        await LoadSceneAsync(m_gameConfig.gameplayScene.Name);
    }
    
}