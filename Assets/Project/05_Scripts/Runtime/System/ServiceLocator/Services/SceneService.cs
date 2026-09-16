using System;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneService : IGameService
{
    #region Variables

    private GameObject m_loadingScreenGo;
    private GameObject m_transitionCircle;
    
    public event Action<string> OnLoadSceneStarted;
    public event Action<string> OnLoadSceneFinished;
    public event Action<float> OnLoadSceneProgress;
    
    public event Action<string> OnUnloadSceneStarted;
    public event Action<string> OnUnloadSceneFinished;
    public event Action<float> OnUnloadSceneProgress;
    
    private readonly SO_GameConfig m_gameConfig;

    private Material m_transitionMaterialInstance;
    
    private const float m_transitionCircleFadeInValue = 0;
    private const float m_transitionCircleFadeOutValue = 1;
    
    private int m_transitionShaderParam = Shader.PropertyToID("_Progress");

    #endregion
    
    #region IGameService Core

    public SceneService(SO_GameConfig gameConfig)
    {
        m_gameConfig = gameConfig;
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(m_transitionMaterialInstance);
    }

    public UniTask InitializeService()
    {
        m_loadingScreenGo = GameObject.Find("PF_LoadingScreen");
        m_transitionCircle = GameObject.Find("PF_Transition_Circle");
        
        if (m_loadingScreenGo == null)
        {
            Debug.LogError("[SceneService] LoadingScreen object not found.");
        }
        
        if (m_transitionCircle == null)
        {
            Debug.LogError("[SceneService] Circle Transition object not found.");
        }
        
        Image transitionCircle = m_transitionCircle.GetComponent<Image>();
        Material transitionMaterial = transitionCircle.material;  
        m_transitionMaterialInstance = new Material(transitionMaterial); 
        transitionCircle.material = m_transitionMaterialInstance;
        m_transitionMaterialInstance.SetFloat(m_transitionShaderParam, m_transitionCircleFadeOutValue);
        
        return UniTask.CompletedTask;
    }

    public void ShutDownService() { }

    public void Tick() { }

    public bool IsInitialized { get; set; }

    #endregion

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
    

    public void ToggleLoadingScreen(bool on)
    {
        if (!m_loadingScreenGo) throw new ArgumentNullException(nameof(on),"[SceneService.ToggleLoadingScreen] LoadingScreen object not found.");
        m_loadingScreenGo.SetActive(on);
    }

    public async UniTaskVoid LoadGameSceneAsync()
    {
        await CircleTransitionFadeIn();
        
        ToggleLoadingScreen(true);
        
        await UnloadScene(m_gameConfig.menuScene.Name);
        
        await LoadSceneAsync(m_gameConfig.gameplayScene.Name);
        
        ToggleLoadingScreen(false);
        
        await CircleTransitionFadeOut();
    }

    private async UniTask CircleTransition(float target)
    {
        float duration = 0.5f;
        
        await Tween.MaterialProperty(
            m_transitionMaterialInstance,
            m_transitionShaderParam,
            target,
            duration,
            Ease.InOutQuad);
        
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
    }

    public async UniTask CircleTransitionFadeIn() => await CircleTransition(m_transitionCircleFadeInValue);
    public async UniTask CircleTransitionFadeOut() => await CircleTransition(m_transitionCircleFadeOutValue);
    
}