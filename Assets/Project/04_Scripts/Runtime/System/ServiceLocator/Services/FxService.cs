using Cysharp.Threading.Tasks;
using NotImplementedException = System.NotImplementedException;

namespace Project._05_Scripts.Runtime.System.ServiceLocator.Services
{
    public class FxService : IGameService
    {
        TimeEffect  m_timeEffect;
        
        public void Dispose() { }

        public UniTask InitializeService()
        {
            m_timeEffect =  new TimeEffect();
            return UniTask.CompletedTask;
        }

        public void ShutDownService() { }

        public void Tick() { }

        public bool IsInitialized { get; set; }
        
        public void HitStop() => m_timeEffect.HitStop().Forget();
    }
}