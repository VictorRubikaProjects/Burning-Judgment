using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Project._05_Scripts.Runtime.System.ServiceLocator.Services
{
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
            ParticleSystem ps = Object.Instantiate(effect, targetTransform);
            
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                
                if (targetTransform == null) break;
                
                ps.transform.position = targetTransform.position;
                ps.transform.rotation = targetTransform.rotation;
                
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(1));
            
            GameObject.Destroy(ps);
        }

        public void PlayEjectionEffect(Transform targetTransform, float duration)
            => LaunchEffect(m_configVfx.VfxPrefabPushed, targetTransform, duration).Forget();
    }
}