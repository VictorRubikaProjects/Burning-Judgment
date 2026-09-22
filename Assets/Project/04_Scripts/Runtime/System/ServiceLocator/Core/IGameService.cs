using System;
using Cysharp.Threading.Tasks;

public interface IGameService : IDisposable
{
    async UniTask Initialize()
    {
        if (IsInitialized) return;
        
        await InitializeService();
        
        IsInitialized = true;
    }
    
    UniTask InitializeService();
    
    void ShutDownService();

    void Tick();
    
    //Never use it out of Initialize()
    bool IsInitialized { get; set; }
}