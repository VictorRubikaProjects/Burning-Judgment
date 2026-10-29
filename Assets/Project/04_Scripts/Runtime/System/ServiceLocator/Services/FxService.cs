using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class FxService : IGameService
    {
        private TimeEffect  m_timeEffect;
        
        private readonly SO_ConfigVfx m_configVfx;

        #region IGameService Core

        public void Dispose() { }

        public FxService(SO_ConfigVfx configVfx)
        {
            m_configVfx = configVfx;
        }
        
        public UniTask InitializeService()
        {
            m_timeEffect =  new TimeEffect();
            return UniTask.CompletedTask;
        }

        public void ShutDownService() { }

        public void Tick() { }

        public bool IsInitialized { get; set; }

        #endregion
        
        public void HitStop() => m_timeEffect.HitStop().Forget();

        private async UniTaskVoid LaunchEffect(ParticleSystem effect, Transform targetTransform, float duration)
        {
            if (effect == null || targetTransform == null) return;

            ParticleSystem ps = Object.Instantiate(effect, Vector3.zero, Quaternion.identity, targetTransform);
            
            ps.transform.localPosition = Vector3.zero;
            ps.transform.localRotation = Quaternion.identity;
            ps.transform.localScale = Vector3.one;

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            ps.Play();
            
            CancellationToken token = ps.GetCancellationTokenOnDestroy();

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
                
                ps.transform.SetParent(null);
                
                await UniTask.Delay(TimeSpan.FromSeconds(ps.main.startLifetime.constantMax), cancellationToken: token);
            }
            catch (OperationCanceledException) { return; }

            if (ps != null) Object.Destroy(ps.gameObject);
        }

        private async UniTaskVoid LaunchEffectUnparented(ParticleSystem effect, Transform targetTransform, float duration)
        {
            if (effect == null || targetTransform == null) return;

            ParticleSystem ps = Object.Instantiate(effect, targetTransform.position, targetTransform.rotation);
            
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
            
            CancellationToken token = ps.GetCancellationTokenOnDestroy();

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
            }
            catch (OperationCanceledException) { return; }

            if (ps) Object.Destroy(ps.gameObject);
        }
        
        public void PlayEffect(ParticleSystem effectPrefab, Transform targetTransform, float duration)
            => LaunchEffect(effectPrefab, targetTransform, duration).Forget();

        
        public void PlayEffectAtTargetPosition(ParticleSystem effectPrefab, Transform targetTransform, float duration)
            => LaunchEffectUnparented(effectPrefab, targetTransform, duration).Forget();

        #region Helper

        public void PlayEjectionEffect(Transform targetTransform, float duration)
            => LaunchEffect(m_configVfx.VfxPrefabEjection, targetTransform, duration).Forget();
        
        public void PlayChainSuccessEffect(Transform targetTransform, float duration) 
            => PlayEffectAtTargetPosition(m_configVfx.VfxPrefabChainInputSuccess,targetTransform, duration);
        #endregion
    }