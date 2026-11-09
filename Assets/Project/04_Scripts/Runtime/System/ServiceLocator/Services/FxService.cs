using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public sealed class FxService : IGameService
{
    private TimeEffect m_timeEffect;
    
    private readonly SO_ConfigVfx m_configVfx;
    private readonly SO_GameConfig m_gameConfig;

    private readonly Dictionary<ParticleSystem, IObjectPool<ParticleSystem>> m_effectPools = new();
    private readonly Dictionary<Transform, ActiveEffectData> m_activeEffects = new();

    private struct ActiveEffectData
    {
        public ParticleSystem ParticleSystem;
        public ParticleSystem PrefabSource;
        public CancellationTokenSource Cts;
    }

    #region IGameService Core

    public void Dispose() 
    {
        foreach (var activeEffect in m_activeEffects.Values)
        {
            activeEffect.Cts?.Cancel();
            activeEffect.Cts?.Dispose();
        }
        m_activeEffects.Clear();

        foreach (var pool in m_effectPools.Values)
        {
            pool.Clear();
        }
        m_effectPools.Clear();
    }

    public FxService(SO_ConfigVfx configVfx, SO_GameConfig gameConfig)
    {
        m_configVfx = configVfx;
        m_gameConfig = gameConfig;
    }
    
    public UniTask InitializeService()
    {
        m_timeEffect = new TimeEffect();
        return UniTask.CompletedTask;
    }

    public void ShutDownService() => Dispose();
    public void Tick() { }
    public bool IsInitialized { get; set; }

    #endregion
    
    public void HitStop() => m_timeEffect.HitStop().Forget();
    
    private Scene GetBootstrapScene()
    {
        string sceneName = m_gameConfig.BootStrapScene.Name; 
        return SceneManager.GetSceneByName(sceneName);
    }
    
    private void MoveToBootstrapScene(GameObject go)
    {
        if (go == null) return;

        go.transform.SetParent(null);

        var bootstrapScene = GetBootstrapScene();
        if (bootstrapScene.IsValid() && bootstrapScene.isLoaded)
        {
            SceneManager.MoveGameObjectToScene(go, bootstrapScene);
        }
    }

    private IObjectPool<ParticleSystem> GetOrCreatePool(ParticleSystem prefab)
    {
        if (!m_effectPools.TryGetValue(prefab, out var pool))
        {
            pool = new ObjectPool<ParticleSystem>(
                createFunc: () =>
                {
                    var instance = Object.Instantiate(prefab);
                    MoveToBootstrapScene(instance.gameObject);
                    return instance;
                },
                actionOnGet: ps => { if (ps) ps.gameObject.SetActive(true); },
                actionOnRelease: ps => 
                { 
                    if (ps) 
                    {
                        ps.gameObject.SetActive(false);
                        MoveToBootstrapScene(ps.gameObject);
                    } 
                },
                actionOnDestroy: ps => { if (ps) Object.Destroy(ps.gameObject); },
                collectionCheck: true,
                defaultCapacity: 5,
                maxSize: 20
            );
            m_effectPools[prefab] = pool;
        }
        return pool;
    }

    private async UniTaskVoid LaunchEffect(ParticleSystem effectPrefab, Transform targetTransform, float duration)
    {
        if (effectPrefab == null || targetTransform == null) return;

        StopEffect(targetTransform);

        var pool = GetOrCreatePool(effectPrefab);
        ParticleSystem ps = pool.Get();

        if (ps == null) return; 

        ps.transform.SetParent(targetTransform);
        ps.transform.localPosition = Vector3.zero;
        ps.transform.localRotation = Quaternion.identity;
        ps.transform.localScale = Vector3.one;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play();

        var cts = new CancellationTokenSource();
        
        m_activeEffects[targetTransform] = new ActiveEffectData
        {
            ParticleSystem = ps,
            PrefabSource = effectPrefab,
            Cts = cts
        };
        
        CancellationToken targetToken = targetTransform.GetCancellationTokenOnDestroy();
        CancellationToken psToken = ps.GetCancellationTokenOnDestroy();
        
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(targetToken, psToken, cts.Token);

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: linkedCts.Token);
            
            MoveToBootstrapScene(ps.gameObject);
            
            await UniTask.Delay(TimeSpan.FromSeconds(ps.main.startLifetime.constantMax), cancellationToken: linkedCts.Token);
        }
        catch (OperationCanceledException) 
        { 
            ReturnToPool(targetTransform, ps, effectPrefab, cts, isCancelledExternally: true);
            return; 
        }

        ReturnToPool(targetTransform, ps, effectPrefab, cts, isCancelledExternally: false);
    }

    private async UniTaskVoid LaunchEffectUnparented(ParticleSystem effectPrefab, Transform targetTransform, float duration)
    {
        if (effectPrefab == null || targetTransform == null) return;

        var pool = GetOrCreatePool(effectPrefab);
        ParticleSystem ps = pool.Get();
        if (ps == null) return;

        MoveToBootstrapScene(ps.gameObject);

        ps.transform.position = targetTransform.position;
        ps.transform.rotation = targetTransform.rotation;
        ps.transform.localScale = Vector3.one;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play();

        var cts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ps.GetCancellationTokenOnDestroy(), cts.Token);

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: linkedCts.Token);
        }
        catch (OperationCanceledException) 
        { 
            if (ps != null && ps.gameObject != null) pool.Release(ps);
            cts.Dispose();
            return; 
        }

        if (ps != null && ps.gameObject != null) pool.Release(ps);
        cts.Dispose();
    }

    private void ReturnToPool(Transform targetTransform, ParticleSystem ps, ParticleSystem effectPrefab, CancellationTokenSource cts, bool isCancelledExternally)
    {
        if (targetTransform != null)
        {
            m_activeEffects.Remove(targetTransform);
        }

        cts?.Dispose();

        if (ps != null && ps.gameObject != null)
        {
            if (!isCancelledExternally)
            {
                MoveToBootstrapScene(ps.gameObject);
            }
            GetOrCreatePool(effectPrefab).Release(ps);
        }
    }

    public void StopEffect(Transform targetTransform)
    {
        if (targetTransform != null && m_activeEffects.TryGetValue(targetTransform, out var data))
        {
            data.Cts?.Cancel();
        }
    }

    public void PlayEffect(ParticleSystem effectPrefab, Transform targetTransform, float duration)
        => LaunchEffect(effectPrefab, targetTransform, duration).Forget();

    public void PlayEffectAtTargetPosition(ParticleSystem effectPrefab, Transform targetTransform, float duration)
        => LaunchEffectUnparented(effectPrefab, targetTransform, duration).Forget();

    #region Helper
    public void PlayEjectionEffect(Transform targetTransform, float duration)
        => PlayEffect(m_configVfx.VfxPrefabEjection, targetTransform, duration);
    
    public void PlayerAttackPlayerEffect(Transform playerTransform, float duration)
        => PlayEffect(m_configVfx.VfxPrefabAttackPlayer, playerTransform, duration);
    
    public void PlayDashEffect(Transform targetTransform, float duration)
        => PlayEffect(m_configVfx.VfxPrefabDash, targetTransform, duration);
    
    public void PlayChainSuccessEffect(Transform targetTransform, float duration) 
        => PlayEffectAtTargetPosition(m_configVfx.VfxPrefabChainInputSuccess, targetTransform, duration);
    #endregion
}